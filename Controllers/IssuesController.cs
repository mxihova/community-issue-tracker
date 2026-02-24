using Microsoft.AspNetCore.Identity;
using Community_Issue_Tracker.Data;
using Community_Issue_Tracker.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Community_Issue_Tracker.Controllers
{
    public class IssuesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public IssuesController(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // =========================
        // GET: Issues
        // =========================
        public async Task<IActionResult> Index(string sortOrder, IssueCategory? categoryFilter)
        {
            var issuesQuery = _context.Issues.AsQueryable();

            if (categoryFilter.HasValue)
            {
                issuesQuery = issuesQuery.Where(i => i.Category == categoryFilter.Value);
            }

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

            ViewBag.TotalIssues = await _context.Issues.CountAsync();
            ViewBag.OpenIssues = await _context.Issues.CountAsync(i => i.Status == IssueStatus.Open);
            ViewBag.InProgressIssues = await _context.Issues.CountAsync(i => i.Status == IssueStatus.InProgress);
            ViewBag.ResolvedIssues = await _context.Issues.CountAsync(i => i.Status == IssueStatus.Resolved);

            return View(issuesList);
        }

        // =========================
        // GET: Create
        // =========================
        [Authorize]
        public IActionResult Create()
        {
            return View();
        }

        // =========================
        // POST: Create
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Create(Issue issue)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                issue.CreatedByUserId = user?.Id;
                issue.CreatedAt = DateTime.UtcNow;

                _context.Add(issue);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(issue);
        }

        // =========================
        // GET: Details
        // =========================
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var issue = await _context.Issues
                .FirstOrDefaultAsync(m => m.Id == id);

            if (issue == null)
                return NotFound();

            return View(issue);
        }

        // =========================
        // GET: Edit
        // =========================
        [Authorize]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var issue = await _context.Issues.FindAsync(id);

            if (issue == null)
                return NotFound();

            var user = await _userManager.GetUserAsync(User);

            if (issue.CreatedByUserId != user?.Id &&
                !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            return View(issue);
        }

        // =========================
        // POST: Edit
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Edit(int id, Issue updatedIssue)
        {
            if (id != updatedIssue.Id)
                return NotFound();

            var existingIssue = await _context.Issues.FindAsync(id);

            if (existingIssue == null)
                return NotFound();

            var user = await _userManager.GetUserAsync(User);

            if (existingIssue.CreatedByUserId != user?.Id &&
                !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            if (ModelState.IsValid)
            {
                // Allowed fields
                existingIssue.Title = updatedIssue.Title;
                existingIssue.Description = updatedIssue.Description;
                existingIssue.Category = updatedIssue.Category;
                existingIssue.Priority = updatedIssue.Priority;

                // Only Admin can change Status
                if (User.IsInRole("Admin"))
                {
                    existingIssue.Status = updatedIssue.Status;
                }

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(updatedIssue);
        }
    }
}