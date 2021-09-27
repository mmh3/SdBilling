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
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SchoolDistrictBilling.Controllers
{
    public class YearEndReconController : Controller
    {
        private IWebHostEnvironment _hostEnvironment;
        private readonly AppDbContext _context;

        public YearEndReconController(IWebHostEnvironment environment, AppDbContext db)
        {
            _hostEnvironment = environment;
            _context = db;
        }

        // GET: YearEndRecon
        public async Task<IActionResult> Index()
        {
            List<CharterSchool> charterSchools = await _context.CharterSchools.ToListAsync();

            return View(new ReportCriteriaView(charterSchools));
        }

        [HttpGet]
        public IActionResult GetSchoolDistricts(int charterSchoolUid)
        {
            // Get a list containing one student per school district for the given charter school.
            var schoolDistrictAuns = _context.GetAunsForCharterSchool(charterSchoolUid);

            // Get a list of the school districts that the given charter school may need to bill.
            var schoolDistricts = _context.SchoolDistricts.Where(sd => schoolDistrictAuns.Contains(sd.Aun)).ToList();
            var selectList = new SelectList(schoolDistricts, "Name", "Name");

            return Json(selectList);
        }

        // POST: YearEndRecon
        [HttpPost]
        [ValidateAntiForgeryToken]
        public FileResult Generate(ReportCriteriaView criteria)
        {
            if (!ModelState.IsValid)
            {
                //TODO: better handling here
                //return NotFound();

            }

            criteria.Year = criteria.Year.Split("-")[1];

            //open the template
            var files = ExcelServices.GenerateYearEndRecon(_context, _hostEnvironment.WebRootPath, criteria);

            //alter data

            //save it off?

            //convert to pdf

            //offer download to user

            //TODO: do we want a summary page here or something?
            //List<CharterSchool> charterSchools = await _context.CharterSchools.ToListAsync();
            //return View("Index", new ReportCriteriaView(charterSchools));


            //TODO: Is there a better way to do this?
            var archive = _hostEnvironment.WebRootPath + "/archive.zip";
            var temp = Directory.CreateDirectory(_hostEnvironment.WebRootPath + "/temp");

            // clear any existing archive
            if (System.IO.File.Exists(archive))
            {
                System.IO.File.Delete(archive);
            }
            // empty the temp folder
            Directory.EnumerateFiles(temp.FullName).ToList().ForEach(f => System.IO.File.Delete(f));

            // copy the selected files to the temp folder
            files.ToList().ForEach(f => System.IO.File.Copy(f, Path.Combine(temp.FullName, Path.GetFileName(f))));

            // create a new archive
            ZipFile.CreateFromDirectory(temp.FullName, archive);

            return File("/archive.zip", "application/zip", "reconciliation.zip");
        }
    }
}