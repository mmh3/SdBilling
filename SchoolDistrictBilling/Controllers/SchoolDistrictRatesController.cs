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


            /*
            //TODO: Give a way to specify which account a file goes with.

            // process uploaded files

            // Create the result Excel file
            string outputFileName = hostEnvironment.WebRootPath + "/files/BudgetTransactions.xlsx";
            ExcelServices.CreateOutputTransactionFile(transactionFiles, outputFileName);

            // Don't rely on or trust the FileName property without validation.

            //return Ok(new { count = files.Count, size, filePaths });
            return Ok(new { inputFileCount = transactionFiles.Count, result = outputFileName });
            //return View(); //had to move Index.cshtml to shared to make this work. Is there a better way this should be done? RedirectToAction or something like that?
            */

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