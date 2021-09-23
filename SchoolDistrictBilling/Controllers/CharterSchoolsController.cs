using System;
using System.Collections.Generic;
using System.Globalization;
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
        public IActionResult Index()
        {
            return View(_context.CharterSchools.ToList());
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
        public async Task<IActionResult> Create(CharterSchool charterSchool)
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

            CharterSchoolView view = new CharterSchoolView(charterSchool);
            var schedules = _context.CharterSchoolSchedules.Where(s => s.CharterSchoolUid == charterSchool.CharterSchoolUid).ToList();
            //view.CharterSchoolScheduleDates = _context.CharterSchoolScheduleDates.Where(d => view.CharterSchoolSchedules..Contains(d.));
            //view.CharterSchoolScheduleDates = _context.CharterSchoolScheduleDates.Join
            var dates = from date in _context.CharterSchoolScheduleDates.ToList()
                        join schedule in schedules on date.CharterSchoolScheduleUid equals schedule.CharterSchoolScheduleUid
                        select date;

            if (schedules == null || schedules.Count() < 1)
            {
                var schedule = new CharterSchoolSchedule(charterSchool);

                _context.Add(schedule);
                await _context.SaveChangesAsync();

                // TODO: Does this have a UID on it now??
                schedules.Add(schedule);
            }

            schedules.Sort((x, y) =>
                {
                    if (y.LastDay == null) y.LastDay = DateTime.Today.Date;
                    if (x.LastDay == null) x.LastDay = DateTime.Today.Date;
                    int compareDate = ((DateTime)y.LastDay).CompareTo((DateTime)x.LastDay);

                    if (compareDate == 0)
                    {
                        if (x.StartGrade == "K")
                        {
                            return -1;
                        }
                        else if (y.StartGrade == "K")
                        {
                            return 1;
                        }
                        else
                        {
                            return x.StartGrade.CompareTo(y.StartGrade);
                        }
                    }

                    return compareDate;
                });

            view.CharterSchoolSchedules = schedules;
            view.CharterSchoolScheduleDates = dates.ToList();

            return View(view);
        }

        // POST: CharterSchools/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CharterSchoolView view)
        {
            if (id != view.CharterSchool.CharterSchoolUid)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(view.CharterSchool);

                    foreach (var schedule in view.CharterSchoolSchedules)
                    {
                        _context.Update(schedule);
                    }

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CharterSchoolExists(view.CharterSchool.CharterSchoolUid))
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

        // POST: CharterSchools/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var charterSchool = await _context.CharterSchools.FindAsync(id);
            _context.CharterSchools.Remove(charterSchool);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> CreateSchedule(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var schedule = new CharterSchoolSchedule()
            {
                CharterSchoolUid = (int)id,
                StartGrade = "K",
                EndGrade = "K"
            };

            _context.Add(schedule);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Edit), new { id = id });
        }

        // GET: CharterSchools/Calendar/5
        public async Task<IActionResult> Calendar(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var schedule = await _context.CharterSchoolSchedules.FindAsync(id);
            if (schedule == null)
            {
                return NotFound();
            }
            var charterSchool = _context.CharterSchools.Where(cs => cs.CharterSchoolUid == schedule.CharterSchoolUid).FirstOrDefault();

            CharterSchoolView view = new CharterSchoolView(charterSchool);
            view.CurrentScheduleUid = schedule.CharterSchoolScheduleUid;
            view.CharterSchoolSchedules = new List<CharterSchoolSchedule>() { schedule };
            view.CharterSchoolScheduleDates = _context.CharterSchoolScheduleDates.Where(d => d.CharterSchoolScheduleUid == schedule.CharterSchoolScheduleUid).ToList();

            return View(view);
        }

        // POST: CharterSchools/Calendar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Calendar(int id, CharterSchoolView schoolView)
        {
            if (id != schoolView.CharterSchoolSchedules.FirstOrDefault().CharterSchoolScheduleUid)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(schoolView.CharterSchoolSchedules.FirstOrDefault());
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CharterSchoolScheduleExists(schoolView.CharterSchoolSchedules.FirstOrDefault().CharterSchoolScheduleUid))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Edit), new { id = schoolView.CharterSchoolSchedules.FirstOrDefault().CharterSchoolUid });
            }
            return View(schoolView);
        }
        private bool CharterSchoolScheduleExists(int uid)
        {
            return _context.CharterSchoolSchedules.Any(s => s.CharterSchoolScheduleUid == uid);
        }

        // GET: CharterSchools/GetHolidays
        [HttpPost]
        public IActionResult GetCalendarEvents([FromBody] CalendarEvent evt)
        {
            List<CalendarEvent> events = new List<CalendarEvent>();

            var month = DateTime.ParseExact(evt.Title.Split(" ")[0], "MMMM", CultureInfo.CurrentCulture).Month;

            DateTime firstDayOfMonth = new DateTime(Int32.Parse(evt.Title.Split(" ")[1]), month, 1);
            DateTime lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

            var dates = _context.CharterSchoolScheduleDates.Where(d => d.CharterSchoolScheduleUid == evt.CharterSchoolScheduleUid &&
                                                                       d.Date >= firstDayOfMonth && d.Date <= lastDayOfMonth).ToList();

            //TODO: Do this better
            foreach (var date in dates)
            {
                events.Add(new CalendarEvent()
                {
                    EventId = date.CharterSchoolScheduleDateUid,
                    AllDay = true,
                    Start = date.Date.ToString(),
                    Title = "Holiday",
                    Description = "Holiday"
                });
            }

            return Json(events);
        }

        [HttpPost]
        public async Task<IActionResult> AddCalendarEvent([FromBody] CalendarEvent evt)
        {
            var date = new CharterSchoolScheduleDate()
            {
                CharterSchoolScheduleUid = evt.CharterSchoolScheduleUid,
                Date = Convert.ToDateTime(evt.Start)
            };

            _context.Add(date);
            await _context.SaveChangesAsync();

            //TODO: what should this be?
            var message = string.Empty;

            return Json(new { message, date.CharterSchoolScheduleDateUid });
        }

        [HttpPost]
        public async Task<IActionResult> RemoveCalendarEvent([FromBody] CalendarEvent evt)
        {
            try
            {
                //Do a retrieve to make sure we're getting the latest from the DB.
                var date = _context.CharterSchoolScheduleDates.Where(d => d.CharterSchoolScheduleDateUid == evt.EventId).ToList().FirstOrDefault();

                _context.CharterSchoolScheduleDates.Remove(date);
                await _context.SaveChangesAsync();

                // TODO: what should this be?
                var message = string.Empty;

                return Json(new { message, evt.EventId });
            }
            catch (Exception e)
            {
                var test = e.Message;

                return Ok();
            }
        }
    }
}