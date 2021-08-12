using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolDistrictBilling.Data;
using SchoolDistrictBilling.Models;

namespace SchoolDistrictBilling.Controllers
{
    public class CharterSchoolsController : Controller
    {
        private readonly AppDbContext _context;

        public CharterSchoolsController(AppDbContext db)
        {
            _context = db;
        }

        // GET: CharterSchools
        public async Task<IActionResult> Index()
        {
            return View(await _context.CharterSchools.ToListAsync());
        }

        // GET: CharterSchools/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: CharterSchools/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name")] CharterSchool charterSchool)
        {
            if (ModelState.IsValid)
            {
                _context.Add(charterSchool);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(charterSchool);
        }

        // GET: CharterSchools/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var charterSchool = await _context.CharterSchools.FindAsync(id);
            if (charterSchool == null)
            {
                return NotFound();
            }
            return View(charterSchool);
        }

        // POST: CharterSchools/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CharterSchoolUid, Name")] CharterSchool charterSchool)
        {
            if (id != charterSchool.CharterSchoolUid)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(charterSchool);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CharterSchoolExists(charterSchool.CharterSchoolUid))
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
            return View(charterSchool);
        }
        private bool CharterSchoolExists(int uid)
        {
            return _context.CharterSchools.Any(cs => cs.CharterSchoolUid == uid);
        }

        // GET: CharterSchools/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var charterSchool = await _context.CharterSchools
                .FirstOrDefaultAsync(cs => cs.CharterSchoolUid == id);
            if (charterSchool == null)
            {
                return NotFound();
            }

            return View(charterSchool);
        }

        // POST: Students/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var charterSchool = await _context.CharterSchools.FindAsync(id);
            _context.CharterSchools.Remove(charterSchool);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}