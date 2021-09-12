using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SchoolDistrictBilling.Data;
using SchoolDistrictBilling.Models;

namespace SchoolDistrictBilling.Controllers
{
    public class PaymentsController : Controller
    {
        private readonly AppDbContext _context;

        public PaymentsController(AppDbContext db)
        {
            _context = db;
        }

        // GET: Payments
        public IActionResult Index()
        {
            var viewModel = from p in _context.Payments.ToList()
                            join cs in _context.CharterSchools.ToList() on p.CharterSchoolUid equals cs.CharterSchoolUid
                            join sd in _context.SchoolDistricts.ToList() on p.SchoolDistrictUid equals sd.SchoolDistrictUid
                            orderby p.Date descending
                            select new PaymentView(cs, sd, p);

            return View(viewModel);
        }

        // GET: Payments/Create
        public IActionResult Create()
        {
            return View(new PaymentView(_context.CharterSchools.ToList(), _context.SchoolDistricts.ToList()));
        }

        // POST: Payments/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PaymentView paymentView)
        {
            var payment = new Payment(paymentView, _context);

            var errors = ModelState.Select(x => x.Value.Errors)
                .Where(y => y.Count > 0)
                .ToList();

            if (ModelState.IsValid)
            {
                _context.Add(payment);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(paymentView);
        }

        // GET: Payments/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var payment = await _context.Payments
                .FirstOrDefaultAsync(p => p.PaymentUid == id);
            if (payment == null)
            {
                return NotFound();
            }

            return View(payment);
        }

        // POST: Payments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var payment = await _context.Payments.FindAsync(id);
            _context.Payments.Remove(payment);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Payments/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var payment = await _context.Payments.FindAsync(id);
            if (payment == null)
            {
                return NotFound();
            }

            // Get the list of aun's for this charter school
            var schoolDistrictAuns = _context.Students.Where(s => s.CharterSchoolUid == payment.CharterSchoolUid)
                                                          .Select(x => x.Aun)
                                                          .Distinct().ToList();
            // Get the list of charter schools based on the list of AUNs
            var schoolDistricts = _context.SchoolDistricts.Where(sd => schoolDistrictAuns.Contains(sd.Aun)).ToList();

            var paymentView = new PaymentView(_context.CharterSchools.ToList(), schoolDistricts, payment);
            return View(paymentView);
        }

        // POST: Payments/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PaymentView paymentView)
        {
            var payment = new Payment(paymentView, _context);

            if (id != payment.PaymentUid)
            {
                return NotFound();
            }

            var errors = ModelState.Select(x => x.Value.Errors)
                .Where(y => y.Count > 0)
                .ToList();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(payment);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PaymentExists(payment.PaymentUid))
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

            return View(paymentView);
        }
        private bool PaymentExists(int uid)
        {
            return _context.Payments.Any(p => p.PaymentUid == uid);
        }

        [HttpGet]
        public IActionResult GetSchoolDistricts(string charterSchoolName)
        {
            var charterSchoolUid = _context.CharterSchools.Where(cs => cs.Name == charterSchoolName)
                                                          .Select(cs => cs.CharterSchoolUid)
                                                          .FirstOrDefault();

            // Get a list containing one student per school district for the given charter school.
            var schoolDistrictAuns = _context.Students.Where(s => s.CharterSchoolUid == charterSchoolUid)
                .Select(s => s.Aun)
                .Distinct()
                .ToList();

            // Get a list of the school districts that the given charter school may need to bill.
            var schoolDistricts = _context.SchoolDistricts.Where(sd => schoolDistrictAuns.Contains(sd.Aun)).ToList();
            var selectList = new SelectList(schoolDistricts, "Name", "Name");

            return Json(selectList);
        }
    }
}