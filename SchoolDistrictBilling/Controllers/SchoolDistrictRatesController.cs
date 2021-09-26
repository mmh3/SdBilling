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
    public class SchoolDistrictRatesController : Controller
    {
        private readonly AppDbContext _context;

        public SchoolDistrictRatesController(AppDbContext db)
        {
            _context = db;
        }

        // GET: SchoolDistrictRates
        public async Task<IActionResult> Index()
        {
            List<SchoolDistrict> schoolDistricts = await _context.SchoolDistricts.ToListAsync();
            List<SchoolDistrictRate> schoolDistrictRates = await _context.SchoolDistrictRates.ToListAsync();

            //TODO: Only select the latest rate for each school district.
            //var viewModel = from d in schoolDistricts
            //                join r in schoolDistrictRates on d.SchoolDistrictUid equals r.SchoolDistrictUid into table1
            //                from r in table1.ToList()
            //                select new SchoolDistrictRateView
            //                {
            //                    SchoolDistrict = d,
            //                    SchoolDistrictRate = r
            //                };

            //TODO: How to get this using linq to be more efficient?
            //var viewModel = from r in schoolDistrictRates
            //                join d in schoolDistricts on r.SchoolDistrictUid equals d.SchoolDistrictUid into table1
            //                //from x in table1.ToList()
            //                where r.EffectiveDate == table1.Max(x => x.EffectiveDate)
            //                select new SchoolDistrictRateView
            //                {
            //                    SchoolDistrict = 
            //                };

            var viewModel = new List<SchoolDistrictRateView>();
            foreach(var school in schoolDistricts)
            {
                var rates = schoolDistrictRates.Where(r => r.SchoolDistrictUid == school.SchoolDistrictUid);
                if (rates.Count() == 0)
                {
                    continue;
                }
                var maxDate = rates.Max(r => r.EffectiveDate);
                var rate = rates.First(r => r.EffectiveDate == maxDate);

                viewModel.Add(new SchoolDistrictRateView(school, rate));
            }

            return View(viewModel);
        }

        [HttpPost("FileUpload")]
        // POST: FileUpload
        public async Task<IActionResult> Import(List<IFormFile> files)
        {
            List<string> fileNames = new List<string>();
            //var transactionFiles = new List<TransactionFile>();
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
                        //transactionFiles.Add(new TransactionFile(formFile.ContentType, filePath));
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

            List<SchoolDistrictRateView> rates = ExcelServices.ImportSchoolDistrictRates(_context, fileNames);

            //TODO: Can we just redirect to Index action here???
            List<SchoolDistrict> schoolDistricts = await _context.SchoolDistricts.ToListAsync();
            List<SchoolDistrictRate> schoolDistrictRates = await _context.SchoolDistrictRates.ToListAsync();

            var viewModel = from d in schoolDistricts
                            join r in schoolDistrictRates on d.SchoolDistrictUid equals r.SchoolDistrictUid into table1
                            from r in table1.ToList()
                            select new SchoolDistrictRateView
                            {
                                SchoolDistrict = d,
                                SchoolDistrictRate = r
                            };

            return View("Index", viewModel);
        }

        // GET: SchoolDistrictRates/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rate = await _context.SchoolDistrictRates.FirstOrDefaultAsync(r => r.SchoolDistrictRateUid == id);
            if (rate == null)
            {
                return NotFound();
            }

            return View(new SchoolDistrictRateView(_context, rate));
        }

        // POST: SchoolDistrictRates/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var rate = await _context.SchoolDistrictRates.FindAsync(id);
            _context.SchoolDistrictRates.Remove(rate);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: SchoolDistrictRates/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rate = await _context.SchoolDistrictRates.FindAsync(id);
            if (rate == null)
            {
                return NotFound();
            }

            return View(new SchoolDistrictRateView(_context, rate));
        }

        // POST: SchoolDistrictRates/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, SchoolDistrictRateView view)
        {
            if (id != view.SchoolDistrictRate.SchoolDistrictRateUid)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(view.SchoolDistrictRate);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RateExists(view.SchoolDistrictRate.SchoolDistrictRateUid))
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

            return View(view);
        }
        private bool RateExists(int uid)
        {
            return _context.SchoolDistrictRates.Any(r => r.SchoolDistrictRateUid == uid);
        }
    }
}