using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolDistrictBilling.Data;
using SchoolDistrictBilling.Models;

namespace SchoolDistrictBilling.Controllers
{
    public class SchoolDistrictsController : Controller
    {
        private readonly AppDbContext _context;

        public SchoolDistrictsController(AppDbContext db)
        {
            _context = db;
        }

        // GET: SchoolDistricts
        public async Task<IActionResult> Index()
        {
            return View(await _context.SchoolDistricts.ToListAsync());
        }

        // GET: SchoolDistricts/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: SchoolDistricts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name")] SchoolDistrict schoolDistrict)
        {
            if (ModelState.IsValid)
            {
                _context.Add(schoolDistrict);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(schoolDistrict);
        }

        // GET: SchoolDistricts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var schoolDistrict = await _context.SchoolDistricts
                .FirstOrDefaultAsync(sd => sd.SchoolDistrictUid == id);
            if (schoolDistrict == null)
            {
                return NotFound();
            }

            return View(schoolDistrict);
        }

        // POST: SchoolDistricts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var schoolDistrict = await _context.SchoolDistricts.FindAsync(id);
            _context.SchoolDistricts.Remove(schoolDistrict);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: SchoolDistricts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var schoolDistrict = await _context.SchoolDistricts.FindAsync(id);
            if (schoolDistrict == null)
            {
                return NotFound();
            }
            return View(schoolDistrict);
        }

        // POST: SchoolDistricts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, SchoolDistrict schoolDistrict)
        {
            if (id != schoolDistrict.SchoolDistrictUid)
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
                    _context.Update(schoolDistrict);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SchoolDistrictExists(schoolDistrict.SchoolDistrictUid))
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

            return View(schoolDistrict);
        }
        private bool SchoolDistrictExists(int uid)
        {
            return _context.SchoolDistricts.Any(sd => sd.SchoolDistrictUid == uid);
        }
    }
}