using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Lab_Management_System.Models;
using LabManagement.ViewModels;
using Lab_Management_System.Models.ViewModels;

namespace Lab_Management_System.Controllers
{
    public class BillsController : Controller
    {
        private readonly LabManagementDbContext _context;

        public BillsController(LabManagementDbContext context)
        {
            _context = context;
        }

        // GET: Bills
        public async Task<IActionResult> Index()
        {
            // Load Bills with Patient and BillItems
            var bills = await _context.Bills
                .Include(b => b.Patient)
                .Include(b => b.BillItems)
                .OrderByDescending(b => b.BillDate) // optional: newest first
                .ToListAsync();

            return View(bills);
        }


        // GET: Bills/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bill = await _context.Bills
                .Include(b => b.Patient)
                .FirstOrDefaultAsync(m => m.BillId == id);
            if (bill == null)
            {
                return NotFound();
            }

            return View(bill);
        }

        // GET: Bills/Create
        public IActionResult Create()
        {
            ViewBag.Patients = new SelectList(
                _context.Patients,
                "PatientId",
                "FullName"
            );

            ViewBag.Tests = _context.Tests.ToList();

            return View(new BillingViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(BillingViewModel model)
        {
            if (!ModelState.IsValid || model.SelectedTestIds.Count == 0)
            {
                ViewBag.Patients = new SelectList(
                    _context.Patients,
                    "PatientId",
                    "FullName",
                    model.PatientId
                );

                ViewBag.Tests = _context.Tests.ToList();
                return View(model);
            }

            // 1️⃣ Load tests
            var tests = _context.Tests
                .Where(t => model.SelectedTestIds.Contains(t.TestId))
                .ToList();

            // 2️⃣ Calculate amounts
            decimal totalAmount = tests.Sum(t => t.DefaultPrice);
            decimal discount = model.DiscountAmount;
            decimal netAmount = totalAmount - discount;
            decimal paid = model.PaidAmount;
            decimal remaining = netAmount - paid;

            // 3️⃣ Determine payment status
            string status;
            if (remaining <= 0)
                status = "Paid";
            else if (paid > 0)
                status = "Partial";
            else
                status = "Unpaid";

            var today = DateTime.Today;

            // GENERATE MONTHLY BILL NUMBER
            int lastBillNumber = _context.Bills
                .Where(b => b.BillMonth == today.Month && b.BillYear == today.Year)
                .Select(b => (int?)b.BillNumber)
                .Max() ?? 0;

            // 4️⃣ CREATE BILL
            var bill = new Bill
            {
                PatientId = model.PatientId,
                BillDate = DateOnly.FromDateTime(today),

                BillNumber = lastBillNumber + 1,
                BillMonth = today.Month,
                BillYear = today.Year,

                TotalAmount = totalAmount,
                DiscountAmount = model.DiscountAmount,
                PaidAmount = model.PaidAmount,
                RemainingAmount = remaining,
                PaymentStatus = status,
                IsLocked = false
            };

            _context.Bills.Add(bill);
            _context.SaveChanges();

            // 5️⃣ Create BillItems
            foreach (var test in tests)
            {
                _context.BillItems.Add(new BillItem
                {
                    BillId = bill.BillId,
                    TestId = test.TestId,
                    TestNameSnapshot = test.TestName,
                    TestPriceSnapshot = test.DefaultPrice
                });
            }

            _context.SaveChanges();
            return RedirectToAction("Print", new { id = bill.BillId });
            //return RedirectToAction(nameof(Index));
        }

        // GET: Bills/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bill = await _context.Bills.FindAsync(id);
            if (bill == null)
            {
                return NotFound();
            }
            if (bill.IsLocked)
            {
                return BadRequest("Cannot edit this bill after printing.");
            }
            ViewData["PatientId"] = new SelectList(_context.Patients, "PatientId", "PatientId", bill.PatientId);
            return View(bill);
        }

        // POST: Bills/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("BillId,PatientId,BillDate,TotalAmount,DiscountAmount,PaidAmount,BalanceAmount,PaymentStatus,CreatedAt")] Bill bill)
        {
            if (id != bill.BillId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(bill);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BillExists(bill.BillId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["PatientId"] = new SelectList(_context.Patients, "PatientId", "PatientId", bill.PatientId);
            return View(bill);
        }

        // GET: Bills/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bill = await _context.Bills
                .Include(b => b.Patient)
                .FirstOrDefaultAsync(m => m.BillId == id);
            if (bill == null)
            {
                return NotFound();
            }

            return View(bill);
        }

        // POST: Bills/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var bill = await _context.Bills.FindAsync(id);
            if (bill != null)
            {
                _context.Bills.Remove(bill);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BillExists(int id)
        {
            return _context.Bills.Any(e => e.BillId == id);
        }

        public IActionResult PatientCredits()
        {
            var credits = _context.Bills
                .Where(b => b.RemainingAmount > 0)
                .GroupBy(b => new { b.PatientId, b.Patient.FullName, b.Patient.PhoneNumber })
                .Select(g => new PatientCreditViewModel
                {
                    PatientId = g.Key.PatientId,
                    PatientName = g.Key.FullName,
                    PhoneNumber = g.Key.PhoneNumber ?? "",
                    TotalBills = g.Count(),
                    TotalCredit = g.Sum(x => x.RemainingAmount)
                })
                .OrderByDescending(x => x.TotalCredit)
                .ToList();

            return View(credits);
        }

        public IActionResult DailyClosing(DateTime? date)
        {
            DateTime selectedDate = date ?? DateTime.Today;

            //var bills = _context.Bills
            //.Where(b => b.BillDate == selectedDate)
            //.ToList();

            var bills = _context.Bills
                .Where(b => b.BillDate == DateOnly.FromDateTime(selectedDate)) // Convert DateTime to DateOnly for comparison
                .ToList();

            var model = new DailyClosingViewModel
            {
                Date = selectedDate,
                TotalBills = bills.Count,

                GrossAmount = bills.Sum(b => b.TotalAmount),
                TotalDiscount = bills.Sum(b => b.DiscountAmount),
                NetAmount = bills.Sum(b => b.NetAmount),

                CashReceived = bills.Sum(b => b.PaidAmount),
                CreditGiven = bills.Sum(b => b.RemainingAmount),

                PaidBills = bills.Count(b => b.PaymentStatus == "Paid"),
                PartialBills = bills.Count(b => b.PaymentStatus == "Partial"),
                UnpaidBills = bills.Count(b => b.PaymentStatus == "Unpaid")
            };

            return View(model);
        }

        public IActionResult Print(int id)
        {
            var bill = _context.Bills
                .Include(b => b.Patient)
                .Include(b => b.BillItems)
                    .ThenInclude(i => i.Test)
                .FirstOrDefault(b => b.BillId == id);

            if (bill == null) { return NotFound("Bill not found"); }
            else { bill.IsLocked = true; _context.SaveChanges(); }

            return View(bill);
        }

        public IActionResult PatientUnpaidBillsPartial(int patientId)
        {
            var bills = _context.Bills
                .Include(b => b.BillItems)   // ✅ REQUIRED
                .Where(b => b.PatientId == patientId && b.RemainingAmount > 0)
                .OrderByDescending(b => b.BillDate)
                .ToList();

            return PartialView("_PatientUnpaidBillsPartial", bills);
        }

        [HttpPost]
        public IActionResult ReceivePayment([FromBody] ReceivePaymentDto model)
        {
            var bill = _context.Bills.FirstOrDefault(b => b.BillId == model.BillId);

            if (bill == null)
                return BadRequest("Bill not found");

            if (model.Amount <= 0)
                return BadRequest("Invalid amount");

            var netAmount = bill.TotalAmount - bill.DiscountAmount;

            if (model.Amount > bill.RemainingAmount)
                return BadRequest("Amount exceeds remaining balance");

            bill.PaidAmount += model.Amount;
            bill.RemainingAmount = netAmount - bill.PaidAmount;

            if (bill.RemainingAmount <= 0)
            {
                bill.RemainingAmount = 0;
                bill.PaymentStatus = "Paid";
            }
            else
            {
                bill.PaymentStatus = "Partial";
            }

            _context.SaveChanges();

            return Ok();
        }

        public IActionResult CreateFullBill(int? patientId)
        {
            var vm = new CreateBillViewModel
            {
                Tests = _context.Tests.ToList(),
                ExistingPatients = _context.Patients.ToList()
            };

            if (patientId.HasValue)
            {
                // load bills for this patient
                vm.ExistingBills = _context.Bills
                    .Where(b => b.PatientId == patientId.Value)
                    .Include(b => b.BillItems)
                    .ToList();
            }

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateFullBill(CreateBillViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Tests = _context.Tests.ToList();
                return View(model);
            }

            using var transaction = _context.Database.BeginTransaction();

            try
            {
                // 1️⃣ CREATE OR REUSE PATIENT
                var patient = _context.Patients
                    .FirstOrDefault(p => p.FullName == model.PatientName
                                      && p.PhoneNumber == model.PhoneNumber);

                if (patient == null)
                {
                    patient = new Patient
                    {
                        FullName = model.PatientName,
                        PhoneNumber = model.PhoneNumber,
                        Gender = model.Gender,
                        Age = model.Age,
                        ReferredBy = model.ReferredBy
                    };

                    _context.Patients.Add(patient);
                    _context.SaveChanges(); // 🔑 get PatientId
                }

                // 2️⃣ GET SELECTED TESTS
                var tests = _context.Tests
                    .Where(t => model.SelectedTestIds.Contains(t.TestId))
                    .ToList();

                decimal total = tests.Sum(t => t.DefaultPrice);
                decimal net = total - model.DiscountAmount;
                decimal remaining = net - model.PaidAmount;

                var today = DateTime.Today;

                // 3️⃣ GENERATE MONTHLY BILL NUMBER
                int lastBillNumber = _context.Bills
                    .Where(b => b.BillMonth == today.Month && b.BillYear == today.Year)
                    .Select(b => (int?)b.BillNumber)
                    .Max() ?? 0;

                // 4️⃣ CREATE BILL
                var bill = new Bill
                {
                    PatientId = patient.PatientId,
                    BillDate = DateOnly.FromDateTime(today),

                    BillNumber = lastBillNumber + 1,
                    BillMonth = today.Month,
                    BillYear = today.Year,

                    TotalAmount = total,
                    DiscountAmount = model.DiscountAmount,
                    PaidAmount = model.PaidAmount,
                    RemainingAmount = remaining,
                    PaymentStatus = remaining > 0 ? "Partial" : "Paid",
                    IsLocked = false
                };

                _context.Bills.Add(bill);
                _context.SaveChanges(); // 🔑 get BillId

                // 5️⃣ CREATE BILL ITEMS
                foreach (var test in tests)
                {
                    _context.BillItems.Add(new BillItem
                    {
                        BillId = bill.BillId,
                        TestId = test.TestId,
                        TestNameSnapshot = test.TestName,
                        TestPriceSnapshot = test.DefaultPrice
                    });
                }

                _context.SaveChanges();

                transaction.Commit();

                // 6️⃣ AUTO PRINT
                return RedirectToAction("Print", new { id = bill.BillId });
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public IActionResult GetBillRowPartial(int billId)
        {
            var bill = _context.Bills
                .Include(b => b.Patient)
                .Include(b => b.BillItems)
                .FirstOrDefault(b => b.BillId == billId);

            if (bill == null) return NotFound();

            return PartialView("_BillRowPartial", bill);
        }

    }
}
