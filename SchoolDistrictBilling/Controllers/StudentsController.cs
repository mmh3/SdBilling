using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolDistrictBilling.Data;
using SchoolDistrictBilling.Models;
using SchoolDistrictBilling.Services;

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
        public IActionResult Index()
        {
            return View(new StudentIndexView(_context));
        }

        // GET: Students/Create
        public IActionResult Create()
        {
            var view = new StudentView()
            {
                CharterSchools = _context.CharterSchools.ToList(),
                SchoolDistricts = _context.SchoolDistricts.ToList()
            };

            return View(view);
        }

        // POST: Students/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(StudentView studentView)
        {
            var errors = ModelState.Select(x => x.Value.Errors)
                .Where(y => y.Count > 0)
                .ToList();

            if (errors.Count() == 3 &&
                errors[0][0].ErrorMessage == "The Name field is required." &&
                errors[1][0].ErrorMessage == "The Aun field is required." &&
                errors[2][0].ErrorMessage == "The Name field is required.")
            {
                _context.Add(studentView.Student);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(studentView);
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

        [HttpPost]
        public async Task<IActionResult> ImportStudents(int ImportCharterSchoolUid, List<IFormFile> files)
        {
            List<string> fileNames = new List<string>();
            foreach (var formFile in files)
            {
                if (formFile.Length > 0)
                {
                    // full path to file in temp location
                    //TODO: What's the comment here about doing this differently?
                    var filePath = Path.GetTempFileName(); //we are using Temp file name just for the example. Add your own file path.

                    try
                    {
                        fileNames.Add(filePath);
                    }
                    catch (ArgumentException e)
                    {
                        //TODO: What to do here?
                    }

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await formFile.CopyToAsync(stream);
                    }
                }
            }

            ExcelServices.ImportStudents(_context, fileNames, ImportCharterSchoolUid);

            return RedirectToAction(nameof(Index));
        }
    }
}