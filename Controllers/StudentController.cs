using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Lab4.Data;
using Lab4.Models;
using Lab4.Models.ViewModels;

namespace Lab4.Controllers
{
    public class StudentController : Controller
    {
        private readonly SchoolCommunityContext _context;

        public StudentController(SchoolCommunityContext context)
        {
            _context = context;
        }

        // GET: Students
        public async Task<IActionResult> Index(int? ID)
        {
            var viewModel = new StudentViewModel();
            viewModel.Students = await _context.Students
                .Include(c => c.CommunityMemberships)
                .ThenInclude(s => s.Community)
                .OrderBy(c => c.ID)
                .AsNoTracking()
                .ToListAsync();


            if (ID != null)
            {
                viewModel.CommunityMemberships = viewModel.Students.Where(x => x.ID == ID).Single().
                    CommunityMemberships.Where(x => x.StudentID == ID);
            }

            return View(viewModel);
        }

        public async Task<IActionResult> EditMemberships(int? ID, string communityID)
        {
            var viewModel = new StudentMembershipViewModel();
            viewModel.Student = _context.Students.Where(x => x.ID == ID).Single();
            var communities = _context.Communities.OrderBy(c => c.Title);

            var listOfCommunity =
                from community in communities
                select new CommunityMembershipViewModel()
                {
                    CommunityId = community.ID,
                    Title = community.Title,
                    IsMember = _context.Students.Where(x => x.ID == ID).Single().CommunityMemberships.Any(x => x.CommunityID == community.ID)
                };

            if (communityID != null)
            {

                if ((listOfCommunity.Where(x => x.CommunityId == communityID).Single().IsMember) == false)
                {
                    _context.CommunityMemberships.Add(new CommunityMembership()
                    {
                        Student = viewModel.Student,
                        StudentID = viewModel.Student.ID,
                        CommunityID = communityID,
                        Community = _context.Communities.Where(x => x.ID == communityID).Single()
                    });
                }
                else
                {
                    _context.CommunityMemberships.Remove(_context.CommunityMemberships
                        .Where(s => s.StudentID == ID)
                        .Where(c => c.CommunityID == communityID)
                        .Single()
                    );
                }
                _context.SaveChanges();
            }

            viewModel.Memberships = await listOfCommunity.OrderBy(x => x.IsMember).AsNoTracking().ToListAsync();
            return View(viewModel);
        }

        // GET: Students/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var student = await _context.Students
                .FirstOrDefaultAsync(m => m.ID == id);
            if (student == null)
            {
                return NotFound();
            }

            return View(student);
        }

        // GET: Students/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Students/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,LastName,FirstName,EnrollmentDate")] Student student)
        {
            if (ModelState.IsValid)
            {
                _context.Add(student);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(student);
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
            return View(student);
        }

        // POST: Students/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,LastName,FirstName,EnrollmentDate")] Student student)
        {
            if (id != student.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(student);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!StudentExists(student.ID))
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
            return View(student);
        }

        // GET: Students/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var student = await _context.Students
                .FirstOrDefaultAsync(m => m.ID == id);
            if (student == null)
            {
                return NotFound();
            }

            return View(student);
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

        private bool StudentExists(int id)
        {
            return _context.Students.Any(e => e.ID == id);
        }
    }
}
