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

            var viewModel = from d in schoolDistricts
                            join r in schoolDistrictRates on d.SchoolDistrictUid equals r.SchoolDistrictUid into table1
                            from r in table1.ToList()
                            select new SchoolDistrictRateView
                            {
                                SchoolDistrict = d,
                                SchoolDistrictRate = r
                            };

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
    }
}