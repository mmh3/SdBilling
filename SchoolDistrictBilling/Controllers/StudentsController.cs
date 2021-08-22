using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolDistrictBilling.Data;
using SchoolDistrictBilling.Models;

namespace SchoolDistrictBilling.Controllers
{
    public class StudentsController : Controller
    {
        private readonly AppDbContext _context;

        public StudentsController(AppDbContext db)
        {
            _context = db;
        }

        // GET: Students
        public async Task<IActionResult> Index()
        {
            List<Student> students = await _context.Students.ToListAsync();
            List<SchoolDistrict> schoolDistricts = await _context.SchoolDistricts.ToListAsync();
            List<CharterSchool> charterSchools = await _context.CharterSchools.ToListAsync();

            var viewModel = from s in students
                            join sd in schoolDistricts on s.Aun equals sd.Aun
                            join cs in charterSchools on s.CharterSchoolUid equals cs.CharterSchoolUid
                            select new StudentView
                            {
                                Student = s,
                                SchoolDistrict = sd,
                                CharterSchool = cs
                            };

            return View(viewModel);
        }

        // GET: Students/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Students/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(StudentView studentView)
        {
            if (ModelState.IsValid)
            {
                _context.Add(studentView.Student);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(studentView.Student);
        }

        // GET: Students/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.StudentUid == id);
            if (student == null)
            {
                return NotFound();
            }

            return View(new StudentView(_context, student));
        }

        // POST: Students/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var student = await _context.Students.FindAsync(id);
            _context.Students.Remove(student);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Students/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var student = await _context.Students.FindAsync(id);
            if (student == null)
            {
                return NotFound();
            }
            return View(new StudentView(_context, student));
        }

        // POST: Students/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, StudentView studentView)
        {
            if (id != studentView.Student.StudentUid)
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
                    _context.Update(studentView.Student);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!StudentExists(studentView.Student.StudentUid))
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

            return View(studentView);
        }
        private bool StudentExists(int uid)
        {
            return _context.Students.Any(s => s.StudentUid == uid);
        }
    }
}