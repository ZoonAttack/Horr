using Entities;
using Entities.Users.FreelancerHelpers;
using Horr.Extentions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServiceContracts.DTOs.FreelancerProfile;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Horr.Controllers.FreelancerProfile
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PortfolioController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PortfolioController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Retrieves the list of portfolio items for the logged-in freelancer.
        /// </summary>
        /// <returns>A list of portfolio item details.</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PortfolioItemDto>>> GetPortfolio()
        {
            var userId = ClaimsPrincipalExtensions.GetLoggedInUserId<string>(User);
            var freelancer = await _context.Freelancers
                .Include(f => f.User)
                .FirstOrDefaultAsync(f => f.UserId == userId);
            if (freelancer == null || freelancer.User.IsDeleted) return NotFound("Freelancer profile not found or is deleted.");

            var items = await _context.PortfolioItems
                .Include(i => i.Media)
                .Where(i => i.FreelancerId == freelancer.UserId && !i.IsDeleted)
                .Select(i => new PortfolioItemDto
                {
                    Id = i.Id,
                    Title = i.Title,
                    Description = i.Description,
                    Role = i.Role,
                    VisitLink = i.VisitLink,
                    ThumbnailUrl = i.ThumbnailUrl,
                    CreatedAt = i.CreatedAt,
                    Media = i.Media.Select(m => new PortfolioMediaDto
                    {
                        Id = m.Id,
                        FileUrl = m.FileUrl,
                        FileType = m.FileType
                    }).ToList()
                })
                .ToListAsync();

            return Ok(items);
        }

        /// <summary>
        /// Retrieves a specific portfolio item by its ID.
        /// </summary>
        /// <param name="id">The ID of the portfolio item.</param>
        /// <returns>The portfolio item details.</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<PortfolioItemDto>> GetById(string id)
        {
            var userId = ClaimsPrincipalExtensions.GetLoggedInUserId<string>(User);
            var freelancer = await _context.Freelancers
                .Include(f => f.User)
                .FirstOrDefaultAsync(f => f.UserId == userId);
            if (freelancer == null || freelancer.User.IsDeleted) return NotFound("Freelancer profile not found or is deleted.");

            var item = await _context.PortfolioItems
                .Include(i => i.Media)
                .FirstOrDefaultAsync(i => i.Id == id && i.FreelancerId == freelancer.UserId && !i.IsDeleted);

            if (item == null) return NotFound();

            return Ok(new PortfolioItemDto
            {
                Id = item.Id,
                Title = item.Title,
                Description = item.Description,
                Role = item.Role,
                VisitLink = item.VisitLink,
                ThumbnailUrl = item.ThumbnailUrl,
                CreatedAt = item.CreatedAt,
                Media = item.Media.Select(m => new PortfolioMediaDto
                {
                    Id = m.Id,
                    FileUrl = m.FileUrl,
                    FileType = m.FileType
                }).ToList()
            });
        }

        /// <summary>
        /// Creates a new portfolio item with supporting media (images, videos, thumbnail).
        /// </summary>
        /// <param name="title">Title of the portfolio item.</param>
        /// <param name="description">Description of the project/work done.</param>
        /// <param name="role">The freelancer's role in the project.</param>
        /// <param name="visitLink">Optional external project URL.</param>
        /// <param name="thumbnail">The project thumbnail image.</param>
        /// <param name="images">Optional list of additional project images.</param>
        /// <param name="videos">Optional list of project demonstration videos.</param>
        /// <returns>The created portfolio item details.</returns>
        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<PortfolioItemDto>> Create([FromForm] string title, [FromForm] string description, [FromForm] string? role, [FromForm] string? visitLink, [FromForm] IFormFile thumbnail, [FromForm] List<IFormFile>? images, [FromForm] List<IFormFile>? videos)
        {
            var userId = ClaimsPrincipalExtensions.GetLoggedInUserId<string>(User);
            var freelancer = await _context.Freelancers
                .Include(f => f.User)
                .FirstOrDefaultAsync(f => f.UserId == userId);
            if (freelancer == null || freelancer.User.IsDeleted) return NotFound("Freelancer profile not found or is deleted.");

            var errors = ValidatePortfolioItem(title, description, visitLink, thumbnail, images, videos);
            if (errors.Any()) return BadRequest(new { errors });

            var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "portfolio");
            if (!Directory.Exists(uploadPath)) Directory.CreateDirectory(uploadPath);

            var thumbnailUrl = await SaveFileAsync(thumbnail, uploadPath);

            var item = new PortfolioItem
            {
                FreelancerId = freelancer.UserId,
                Title = title,
                Description = description,
                Role = role,
                VisitLink = visitLink,
                ThumbnailUrl = thumbnailUrl,
                IsDeleted = false
            };

            if (images != null)
            {
                foreach (var img in images)
                {
                    var url = await SaveFileAsync(img, uploadPath);
                    item.Media.Add(new PortfolioMedia { FileUrl = url, FileType = "Image" });
                }
            }

            if (videos != null)
            {
                foreach (var vid in videos)
                {
                    var url = await SaveFileAsync(vid, uploadPath);
                    item.Media.Add(new PortfolioMedia { FileUrl = url, FileType = "Video" });
                }
            }

            _context.PortfolioItems.Add(item);
            await _context.SaveChangesAsync();

            // Re-fetch to get database-generated fields if needed
            return CreatedAtAction(nameof(GetById), new { id = item.Id }, new PortfolioItemDto
            {
                Id = item.Id,
                Title = item.Title,
                Description = item.Description,
                Role = item.Role,
                VisitLink = item.VisitLink,
                ThumbnailUrl = item.ThumbnailUrl,
                CreatedAt = item.CreatedAt,
                Media = item.Media.Select(m => new PortfolioMediaDto
                {
                    Id = m.Id,
                    FileUrl = m.FileUrl,
                    FileType = m.FileType
                }).ToList()
            });
        }

        /// <summary>
        /// Updates an existing portfolio item and replaces its media files.
        /// </summary>
        /// <param name="id">The portfolio item ID to update.</param>
        /// <param name="title">Updated title.</param>
        /// <param name="description">Updated description.</param>
        /// <param name="role">Updated role.</param>
        /// <param name="visitLink">Updated external project URL.</param>
        /// <param name="thumbnail">Optional updated thumbnail image.</param>
        /// <param name="images">Optional list of updated project images.</param>
        /// <param name="videos">Optional list of updated project videos.</param>
        /// <returns>The updated portfolio item details.</returns>
        [HttpPut("{id}")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<PortfolioItemDto>> Update(string id, [FromForm] string title, [FromForm] string description, [FromForm] string? role, [FromForm] string? visitLink, [FromForm] IFormFile? thumbnail, [FromForm] List<IFormFile>? images, [FromForm] List<IFormFile>? videos)
        {
            var userId = ClaimsPrincipalExtensions.GetLoggedInUserId<string>(User);
            var freelancer = await _context.Freelancers
                .Include(f => f.User)
                .FirstOrDefaultAsync(f => f.UserId == userId);
            if (freelancer == null || freelancer.User.IsDeleted) return NotFound("Freelancer profile not found or is deleted.");

            var item = await _context.PortfolioItems
                .Include(i => i.Media)
                .FirstOrDefaultAsync(i => i.Id == id && i.FreelancerId == freelancer.UserId && !i.IsDeleted);

            if (item == null) return NotFound();

            var errors = ValidatePortfolioItem(title, description, visitLink, thumbnail, images, videos, isUpdate: true);
            if (errors.Any()) return BadRequest(new { errors });

            var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "portfolio");
            if (!Directory.Exists(uploadPath)) Directory.CreateDirectory(uploadPath);

            item.Title = title;
            item.Description = description;
            item.Role = role;
            item.VisitLink = visitLink;
            item.UpdatedAt = DateTime.UtcNow;

            if (thumbnail != null)
            {
                item.ThumbnailUrl = await SaveFileAsync(thumbnail, uploadPath);
            }

            // Replace all media
            _context.PortfolioMedia.RemoveRange(item.Media);
            item.Media.Clear();

            if (images != null)
            {
                foreach (var img in images)
                {
                    var url = await SaveFileAsync(img, uploadPath);
                    item.Media.Add(new PortfolioMedia { FileUrl = url, FileType = "Image" });
                }
            }

            if (videos != null)
            {
                foreach (var vid in videos)
                {
                    var url = await SaveFileAsync(vid, uploadPath);
                    item.Media.Add(new PortfolioMedia { FileUrl = url, FileType = "Video" });
                }
            }

            await _context.SaveChangesAsync();

            return Ok(new PortfolioItemDto
            {
                Id = item.Id,
                Title = item.Title,
                Description = item.Description,
                Role = item.Role,
                VisitLink = item.VisitLink,
                ThumbnailUrl = item.ThumbnailUrl,
                CreatedAt = item.CreatedAt,
                Media = item.Media.Select(m => new PortfolioMediaDto
                {
                    Id = m.Id,
                    FileUrl = m.FileUrl,
                    FileType = m.FileType
                }).ToList()
            });
        }

        /// <summary>
        /// Soft deletes a portfolio item by its ID.
        /// </summary>
        /// <param name="id">The portfolio item ID to delete.</param>
        /// <returns>No content on success.</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var userId = ClaimsPrincipalExtensions.GetLoggedInUserId<string>(User);
            var freelancer = await _context.Freelancers
                .Include(f => f.User)
                .FirstOrDefaultAsync(f => f.UserId == userId);
            if (freelancer == null || freelancer.User.IsDeleted) return NotFound("Freelancer profile not found or is deleted.");

            var item = await _context.PortfolioItems
                .FirstOrDefaultAsync(i => i.Id == id && i.FreelancerId == freelancer.UserId && !i.IsDeleted);

            if (item == null) return NotFound();

            item.IsDeleted = true;
            item.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private List<string> ValidatePortfolioItem(string title, string description, string? visitLink, IFormFile? thumbnail, List<IFormFile>? images, List<IFormFile>? videos, bool isUpdate = false)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(title) || title.Length > 150)
                errors.Add("Title is required and must be max 150 characters.");

            if (string.IsNullOrWhiteSpace(description) || description.Length > 1000)
                errors.Add("Description is required and must be max 1000 characters.");

            if (!string.IsNullOrEmpty(visitLink))
            {
                if (!Uri.TryCreate(visitLink, UriKind.Absolute, out _))
                    errors.Add("Visit link must be a valid absolute URL.");
            }

            if (!isUpdate && thumbnail == null)
                errors.Add("Thumbnail is required.");

            if (thumbnail != null)
            {
                var ext = Path.GetExtension(thumbnail.FileName).ToLower();
                if (ext != ".jpg" && ext != ".png" && ext != ".jpeg")
                    errors.Add("Thumbnail must be .jpg or .png.");
                if (thumbnail.Length > 5 * 1024 * 1024)
                    errors.Add("Thumbnail must be max 5MB.");
            }

            if (images != null)
            {
                if (images.Count > 10)
                    errors.Add("Max 10 images allowed.");
                foreach (var file in images)
                {
                    var ext = Path.GetExtension(file.FileName).ToLower();
                    if (ext != ".jpg" && ext != ".png" && ext != ".jpeg")
                        errors.Add($"File {file.FileName} is not a valid image.");
                    if (file.Length > 5 * 1024 * 1024)
                        errors.Add($"File {file.FileName} exceeds 5MB.");
                }
            }

            if (videos != null)
            {
                if (videos.Count > 2)
                    errors.Add("Max 2 videos allowed.");
                foreach (var file in videos)
                {
                    var ext = Path.GetExtension(file.FileName).ToLower();
                    if (ext != ".mp4")
                        errors.Add($"File {file.FileName} is not a valid video (.mp4 only).");
                    if (file.Length > 50 * 1024 * 1024)
                        errors.Add($"File {file.FileName} exceeds 50MB.");
                }
            }

            return errors;
        }

        private async Task<string> SaveFileAsync(IFormFile file, string uploadPath)
        {
            var fileName = $"{Guid.NewGuid()}_{file.FileName}";
            var filePath = Path.Combine(uploadPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return $"/uploads/portfolio/{fileName}";
        }
    }
}
