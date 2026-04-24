using BaseCore.Entities;
using BaseCore.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BaseCore.APIService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContentController : ControllerBase
    {
        private readonly BaseCoreDbContext _context;

        public ContentController(BaseCoreDbContext context)
        {
            _context = context;
        }

        [HttpGet("blog-posts")]
        public async Task<IActionResult> GetBlogPosts([FromQuery] string? category)
        {
            var query = _context.BlogPosts.Where(p => p.Status == "Published");
            if (!string.IsNullOrWhiteSpace(category))
            {
                query = query.Where(p => p.Category == category);
            }

            var posts = await query
                .OrderByDescending(p => p.PublishedAt)
                .ToListAsync();

            return Ok(posts);
        }

        [HttpGet("blog-posts/{slug}")]
        public async Task<IActionResult> GetBlogPost(string slug)
        {
            var post = await _context.BlogPosts.FirstOrDefaultAsync(p => p.Slug == slug && p.Status == "Published");
            if (post == null)
            {
                return NotFound(new { message = "Post not found" });
            }

            return Ok(post);
        }

        [HttpPost("blog-posts")]
        [Authorize]
        public async Task<IActionResult> CreateBlogPost([FromBody] BlogPost dto)
        {
            dto.AuthorUserId = GetUserId();
            dto.CreatedAt = DateTime.UtcNow;
            dto.UpdatedAt = DateTime.UtcNow;
            if (dto.Status == "Published" && dto.PublishedAt == null)
            {
                dto.PublishedAt = DateTime.UtcNow;
            }

            _context.BlogPosts.Add(dto);
            await _context.SaveChangesAsync();
            return Ok(dto);
        }

        [HttpGet("faqs")]
        public async Task<IActionResult> GetFaqs([FromQuery] string? category)
        {
            var query = _context.Faqs.Where(f => f.IsActive);
            if (!string.IsNullOrWhiteSpace(category))
            {
                query = query.Where(f => f.Category == category);
            }

            var faqs = await query
                .OrderBy(f => f.SortOrder)
                .ThenBy(f => f.Id)
                .ToListAsync();

            return Ok(faqs);
        }

        [HttpPost("contact-requests")]
        public async Task<IActionResult> CreateContactRequest([FromBody] ContactRequest dto)
        {
            dto.Status = "New";
            dto.CreatedAt = DateTime.UtcNow;
            _context.ContactRequests.Add(dto);
            await _context.SaveChangesAsync();
            return Ok(dto);
        }

        [HttpGet("contact-requests")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> GetContactRequests([FromQuery] string? status)
        {
            var query = _context.ContactRequests
                .Include(r => r.Product)
                .Include(r => r.Showroom)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(r => r.Status == status);
            }

            var requests = await query
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return Ok(requests);
        }

        [HttpPut("contact-requests/{id:int}/status")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> UpdateContactStatus(int id, [FromBody] UpdateContactStatusDto dto)
        {
            var request = await _context.ContactRequests.FirstOrDefaultAsync(r => r.Id == id);
            if (request == null)
            {
                return NotFound(new { message = "Contact request not found" });
            }

            request.Status = dto.Status;
            request.ProcessedAt = DateTime.UtcNow;
            request.ProcessedByUserId = GetUserId();
            await _context.SaveChangesAsync();

            return Ok(request);
        }

        [HttpGet("vouchers/{code}")]
        public async Task<IActionResult> GetVoucher(string code)
        {
            var now = DateTime.UtcNow;
            var voucher = await _context.Vouchers.FirstOrDefaultAsync(v =>
                v.Code == code &&
                v.IsActive &&
                v.StartAt <= now &&
                v.EndAt >= now &&
                (!v.UsageLimit.HasValue || v.UsedCount < v.UsageLimit.Value));

            if (voucher == null)
            {
                return NotFound(new { message = "Voucher is invalid or expired" });
            }

            return Ok(voucher);
        }

        private int? GetUserId()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userId, out var parsedUserId) ? parsedUserId : null;
        }
    }

    public class UpdateContactStatusDto
    {
        public string Status { get; set; } = "Processing";
    }
}
