using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using SchoolDistrictBilling.Data;
using SchoolDistrictBilling.Models;
using SchoolDistrictBilling.Services;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace SchoolDistrictBilling.Controllers
{
    public class MonthlyInvoiceController : Controller
    {
        private IWebHostEnvironment _hostEnvironment;
        private readonly AppDbContext _context;

        public MonthlyInvoiceController(IWebHostEnvironment environment, AppDbContext db)
        {
            _hostEnvironment = environment;
            _context = db;
        }

        // GET: MonthlyInvoice
        public async Task<IActionResult> Index()
        {
            List<CharterSchool> charterSchools = await _context.CharterSchools.ToListAsync();

            return View(new MonthlyInvoiceView(charterSchools));
        }

        // POST: MonthlyInvoice
        [HttpPost]
        [ValidateAntiForgeryToken]
        public FileResult Generate(MonthlyInvoiceView criteria)
        {
            if (!ModelState.IsValid)
            {
                //TODO: better handling here
                //return NotFound();
                
            }

            //open the template
            var files = ExcelServices.GenerateMonthlyInvoice(_context, _hostEnvironment.WebRootPath, criteria);

            //alter data

            //save it off?

            //convert to pdf

            //offer download to user

            //TODO: do we want a summary page here or something?
            //List<CharterSchool> charterSchools = await _context.CharterSchools.ToListAsync();
            //return View("Index", new MonthlyInvoiceView(charterSchools));

            var archive = _hostEnvironment.WebRootPath + "/archive.zip";
            var temp = _hostEnvironment.WebRootPath + "/temp";

            // clear any existing archive
            if (System.IO.File.Exists(archive))
            {
                System.IO.File.Delete(archive);
            }
            // empty the temp folder
            Directory.EnumerateFiles(temp).ToList().ForEach(f => System.IO.File.Delete(f));

            // copy the selected files to the temp folder
            files.ToList().ForEach(f => System.IO.File.Copy(f, Path.Combine(temp, Path.GetFileName(f))));

            // create a new archive
            ZipFile.CreateFromDirectory(temp, archive);

            return File("/archive.zip", "application/zip", "archive.zip");
        }
    }
}