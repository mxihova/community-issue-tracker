using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Community_Issue_Tracker.Data;
using Community_Issue_Tracker.Models;

namespace Community_Issue_Tracker.Controllers
{
    public class IssuesController : Controller
    {
        private readonly ApplicationDbContext _context;

        // Constructor
        public IssuesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Issues
        public async Task<IActionResult> Index(string sortOrder, IssueCategory? categoryFilter)
        {
            var issuesQuery = _context.Issues.AsQueryable();

            // FILTER
            if (categoryFilter.HasValue)
            {
                issuesQuery = issuesQuery.Where(i => i.Category == categoryFilter.Value);
            }

            // SORT
            switch (sortOrder)
            {
                case "priority_desc":
                    issuesQuery = issuesQuery.OrderByDescending(i => i.Priority);
                    break;

                case "priority_asc":
                    issuesQuery = issuesQuery.OrderBy(i => i.Priority);
                    break;

                case "date_asc":
                    issuesQuery = issuesQuery.OrderBy(i => i.CreatedAt);
                    break;

                default:
                    issuesQuery = issuesQuery.OrderByDescending(i => i.CreatedAt);
                    break;
            }

            var issuesList = await issuesQuery.ToListAsync();

            // DASHBOARD COUNTS
            ViewBag.TotalIssues = await _context.Issues.CountAsync();
            ViewBag.OpenIssues = await _context.Issues.CountAsync(i => i.Status == IssueStatus.Open);
            ViewBag.InProgressIssues = await _context.Issues.CountAsync(i => i.Status == IssueStatus.InProgress);
            ViewBag.ResolvedIssues = await _context.Issues.CountAsync(i => i.Status == IssueStatus.Resolved);

            return View(issuesList);
        }

        // GET: Issues/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Issues/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Issue issue)
        {
            if (ModelState.IsValid)
            {
                _context.Add(issue);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(issue);
        }

        // GET: Issues/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var issue = await _context.Issues.FirstOrDefaultAsync(m => m.Id == id);

            if (issue == null)
                return NotFound();

            return View(issue);


        }

        // GET: Issues/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var issue = await _context.Issues.FindAsync(id);

            if (issue == null)
                return NotFound();

            return View(issue);
        }

        // POST: Issues/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Issue issue)
        {
            if (id != issue.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                _context.Update(issue);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(issue);
        }

    }
}
