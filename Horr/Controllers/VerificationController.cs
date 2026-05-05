using Entities;
using Entities.Enums;
using Entities.Users;
using Entities.Verification;
using Horr.Extentions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServiceContracts.DTOs.Verification;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Horr.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class VerificationController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly UserManager<User> _userManager;

        public VerificationController(AppDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpPost("submit")]
        [Authorize(Roles = "Freelancer")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Submit(
            [FromForm] IFormFile frontImage,
            [FromForm] IFormFile backImage,
            [FromForm] IFormFile selfie)
        {
            var userId = ClaimsPrincipalExtensions.GetLoggedInUserId<string>(User);
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound("User not found.");

            if (user.IsVerified)
                return Conflict("Your identity is already verified.");

            var hasPendingRequest = await _context.VerificationRequests
                .AnyAsync(r => r.UserId == userId && r.Status == VerificationStatus.Pending);

            if (hasPendingRequest)
                return Conflict("You already have a pending verification request.");

            var errors = ValidateImages(frontImage, backImage, selfie);
            if (errors.Any()) return BadRequest(new { errors });

            var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "verification");
            if (!Directory.Exists(uploadPath)) Directory.CreateDirectory(uploadPath);

            var frontUrl = await SaveFileAsync(frontImage, uploadPath);
            var backUrl = await SaveFileAsync(backImage, uploadPath);
            var selfieUrl = await SaveFileAsync(selfie, uploadPath);

            var request = new VerificationRequest
            {
                UserId = userId,
                FrontImageUrl = frontUrl,
                BackImageUrl = backUrl,
                SelfieUrl = selfieUrl,
                Status = VerificationStatus.Pending
            };

            _context.VerificationRequests.Add(request);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetMyStatus), new { }, MapToDto(request, user.FullName));
        }

        [HttpGet("my-status")]
        [Authorize(Roles = "Freelancer")]
        public async Task<IActionResult> GetMyStatus()
        {
            var userId = ClaimsPrincipalExtensions.GetLoggedInUserId<string>(User);
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound("User not found.");

            var request = await _context.VerificationRequests
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.SubmittedAt)
                .FirstOrDefaultAsync();

            if (request == null) return NotFound("No verification request found.");

            return Ok(MapToDto(request, user.FullName));
        }

        [HttpGet("pending")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> GetPending()
        {
            var requests = await _context.VerificationRequests
                .Include(r => r.User)
                .Where(r => r.Status == VerificationStatus.Pending)
                .OrderBy(r => r.SubmittedAt)
                .ToListAsync();

            return Ok(requests.Select(r => MapToDto(r, r.User?.FullName ?? "Unknown")).ToList());
        }

        [HttpGet("all")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> GetAll()
        {
            var requests = await _context.VerificationRequests
                .Include(r => r.User)
                .OrderByDescending(r => r.SubmittedAt)
                .ToListAsync();

            return Ok(requests.Select(r => MapToDto(r, r.User?.FullName ?? "Unknown")).ToList());
        }

        [HttpPost("review")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Review([FromBody] ReviewVerificationDto dto)
        {
            if (!dto.Approved && string.IsNullOrWhiteSpace(dto.RejectionReason))
                return BadRequest("Rejection reason is required when rejecting a request.");

            var request = await _context.VerificationRequests
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Id == dto.RequestId);

            if (request == null) return NotFound("Verification request not found.");

            if (request.Status != VerificationStatus.Pending)
                return BadRequest("This request has already been reviewed.");

            var adminId = ClaimsPrincipalExtensions.GetLoggedInUserId<string>(User);

            if (dto.Approved)
            {
                request.Status = VerificationStatus.Approved;
                if (request.User != null)
                {
                    request.User.IsVerified = true;
                }
            }
            else
            {
                request.Status = VerificationStatus.Rejected;
                request.RejectionReason = dto.RejectionReason;
            }

            request.ReviewedAt = DateTime.UtcNow;
            request.ReviewedByAdminId = adminId;

            await _context.SaveChangesAsync();

            return Ok(MapToDto(request, request.User?.FullName ?? "Unknown"));
        }

        private List<string> ValidateImages(IFormFile front, IFormFile back, IFormFile selfie)
        {
            var errors = new List<string>();
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
            long maxFileSize = 5 * 1024 * 1024; // 5MB

            ValidateImage(front, "Front ID Image", errors, allowedExtensions, maxFileSize);
            ValidateImage(back, "Back ID Image", errors, allowedExtensions, maxFileSize);
            ValidateImage(selfie, "Selfie Image", errors, allowedExtensions, maxFileSize);

            return errors;
        }

        private void ValidateImage(IFormFile file, string fieldName, List<string> errors, string[] allowedExtensions, long maxSize)
        {
            if (file == null)
            {
                errors.Add($"{fieldName} is required.");
                return;
            }

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
                errors.Add($"{fieldName} must be a .jpg, .jpeg, or .png image.");

            if (file.Length > maxSize)
                errors.Add($"{fieldName} must be less than 5MB.");
        }

        private async Task<string> SaveFileAsync(IFormFile file, string uploadPath)
        {
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine(uploadPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return $"/uploads/verification/{fileName}";
        }

        private VerificationRequestDto MapToDto(VerificationRequest request, string fullName)
        {
            return new VerificationRequestDto
            {
                Id = request.Id,
                UserId = request.UserId,
                UserFullName = fullName,
                FrontImageUrl = request.FrontImageUrl,
                BackImageUrl = request.BackImageUrl,
                SelfieUrl = request.SelfieUrl,
                Status = (int)request.Status,
                RejectionReason = request.RejectionReason,
                SubmittedAt = request.SubmittedAt,
                ReviewedAt = request.ReviewedAt
            };
        }
    }
}
