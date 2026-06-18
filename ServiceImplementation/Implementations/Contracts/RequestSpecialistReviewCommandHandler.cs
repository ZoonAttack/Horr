using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Entities;
using Entities.Project;
using Entities.Enums;
using Entities.Users;
using ServiceContracts.DTOs.Contract;
using ServiceContracts.AI;
using ServiceContracts.Storage;
using ServiceImplementation.Exceptions;

namespace ServiceImplementation.Implementations.Contracts
{
    public class RequestSpecialistReviewCommandHandler : IRequestHandler<RequestSpecialistReviewCommand, ContractSpecialistReviewReadDto>
    {
        private readonly AppDbContext _context;
        private readonly IGeminiService _geminiService;
        private readonly IFileStorageService _fileStorageService;

        public RequestSpecialistReviewCommandHandler(
            AppDbContext context,
            IGeminiService geminiService,
            IFileStorageService fileStorageService)
        {
            _context = context;
            _geminiService = geminiService;
            _fileStorageService = fileStorageService;
        }

        public async Task<ContractSpecialistReviewReadDto> Handle(RequestSpecialistReviewCommand request, CancellationToken cancellationToken)
        {
            var delivery = await _context.ContractDeliveries
                .Include(d => d.Attachments)
                .Include(d => d.Contract)
                    .ThenInclude(c => c.Proposal)
                        .ThenInclude(p => p.JobPost)
                .Include(d => d.Contract)
                    .ThenInclude(c => c.JobPost)
                .FirstOrDefaultAsync(d => d.Id == request.DeliveryId, cancellationToken);

            if (delivery == null)
            {
                throw new NotFoundException($"Contract delivery with ID {request.DeliveryId} not found.");
            }

            if (delivery.Contract == null)
            {
                throw new NotFoundException("Associated contract not found.");
            }

            if (delivery.Contract.ClientId != request.ClientId)
            {
                throw new ForbiddenException("Only the contract client can request a specialist review.");
            }

            if (delivery.Status != DeliveryStatus.Pending)
            {
                throw new InvalidStateException("A specialist review can only be requested for pending deliveries.");
            }

            var hasActiveReview = await _context.ContractSpecialistReviews
                .AnyAsync(r => r.DeliveryId == request.DeliveryId &&
                               (r.Status == SpecialistReviewStatus.Pending || r.Status == SpecialistReviewStatus.InProgress),
                           cancellationToken);

            if (hasActiveReview)
            {
                throw new ConflictException("A specialist review is already pending or in progress for this delivery.");
            }

            var review = new ContractSpecialistReview
            {
                DeliveryId = request.DeliveryId,
                RequestedByClientId = request.ClientId,
                ReviewerType = request.ReviewerType,
                RequirementsSummary = request.RequirementsSummary,
                RequestedAt = DateTime.UtcNow
            };

            if (request.ReviewerType == ReviewerType.AI)
            {
                var allowedExtensions = new[] { ".txt", ".csv", ".pdf", ".md" };
                var eligibleAttachments = delivery.Attachments
                    .Where(a => a.Type == AttachmentType.File && !a.IsDeleted)
                    .Where(a => {
                        var ext = Path.GetExtension(a.OriginalFileName ?? "")?.ToLowerInvariant()
                                  ?? a.FileType?.ToLowerInvariant()
                                  ?? "";
                        if (!ext.StartsWith(".")) ext = "." + ext;
                        return Array.Exists(allowedExtensions, e => e == ext) && a.FileSizeBytes <= 500_000;
                    })
                    .Take(3)
                    .ToList();

                var eligibleFilesContext = new StringBuilder();
                var skippedFilesList = new StringBuilder();

                var skippedAttachments = delivery.Attachments
                    .Where(a => !eligibleAttachments.Contains(a))
                    .ToList();

                foreach (var attachment in eligibleAttachments)
                {
                    try
                     {
                        var physicalPath = _fileStorageService.GetPhysicalPath(attachment.FileUrl);
                        if (!string.IsNullOrEmpty(physicalPath) && File.Exists(physicalPath))
                        {
                            var fileContent = File.ReadAllText(physicalPath);
                            eligibleFilesContext.AppendLine($"--- File: {attachment.OriginalFileName} ---");
                            eligibleFilesContext.AppendLine(fileContent);
                            eligibleFilesContext.AppendLine();
                        }
                        else
                        {
                            skippedFilesList.AppendLine($"- {attachment.OriginalFileName} (File could not be found/read on disk)");
                        }
                    }
                    catch
                    {
                        skippedFilesList.AppendLine($"- {attachment.OriginalFileName} (Error reading file content)");
                    }
                }

                foreach (var attachment in skippedAttachments)
                {
                    var ext = Path.GetExtension(attachment.OriginalFileName ?? "") ?? attachment.FileType ?? "";
                    skippedFilesList.AppendLine($"- {attachment.OriginalFileName} ({ext})");
                }

                var jobTitle = delivery.Contract.Proposal?.JobPost?.Title
                               ?? delivery.Contract.JobPost?.Title
                               ?? delivery.Contract.CustomJobDescription
                               ?? "Freelance Contract Project";

                var prompt = $@"You are a professional quality reviewer on a freelance marketplace.

JOB TITLE: {jobTitle}

CLIENT REQUIREMENTS SUMMARY:
{request.RequirementsSummary}

DELIVERY NOTE FROM FREELANCER:
{delivery.DeliveryNote}

ATTACHMENT CONTEXT:
{eligibleFilesContext}

FILES NOT INCLUDED (non-text or too large):
{(skippedFilesList.Length > 0 ? skippedFilesList.ToString() : "None")}

TASK:
Based on the job context, client requirements summary, delivery note, and any readable file contents,
determine whether the delivery satisfies the client's stated requirements.

RESPOND with ONLY a valid JSON object in this exact format:
{{
  ""verdict"": ""Satisfactory"" or ""Unsatisfactory"",
  ""note"": ""Your detailed reasoning here (2-4 sentences)""
}}
No explanation. Just the JSON object.";

                string responseText = string.Empty;
                try
                {
                    responseText = await _geminiService.AskAsync(prompt);
                }
                catch
                {
                    // Fail-safe default parameters already set
                }

                ReviewVerdict verdict = ReviewVerdict.Unsatisfactory;
                string reviewNote = "AI review failed to produce a valid response. Please request a human specialist review.";

                if (!string.IsNullOrEmpty(responseText))
                {
                    try
                    {
                        responseText = responseText.Trim();
                        if (responseText.StartsWith("["))
                        {
                            var array = JsonSerializer.Deserialize<string[]>(responseText);
                            if (array != null && array.Length >= 2)
                            {
                                var verdictStr = array[0]?.Trim();
                                var noteStr = array[1]?.Trim();

                                if (string.Equals(verdictStr, "Satisfactory", StringComparison.OrdinalIgnoreCase))
                                {
                                    verdict = ReviewVerdict.Satisfactory;
                                    reviewNote = noteStr ?? string.Empty;
                                }
                                else if (string.Equals(verdictStr, "Unsatisfactory", StringComparison.OrdinalIgnoreCase))
                                {
                                    verdict = ReviewVerdict.Unsatisfactory;
                                    reviewNote = noteStr ?? string.Empty;
                                }
                            }
                        }
                        else if (responseText.StartsWith("{"))
                        {
                            using var doc = JsonDocument.Parse(responseText);
                            if (doc.RootElement.TryGetProperty("verdict", out var verdictProp) &&
                                doc.RootElement.TryGetProperty("note", out var noteProp))
                            {
                                var verdictStr = verdictProp.GetString()?.Trim();
                                var noteStr = noteProp.GetString()?.Trim();

                                if (string.Equals(verdictStr, "Satisfactory", StringComparison.OrdinalIgnoreCase))
                                {
                                    verdict = ReviewVerdict.Satisfactory;
                                    reviewNote = noteStr ?? string.Empty;
                                }
                                else if (string.Equals(verdictStr, "Unsatisfactory", StringComparison.OrdinalIgnoreCase))
                                {
                                    verdict = ReviewVerdict.Unsatisfactory;
                                    reviewNote = noteStr ?? string.Empty;
                                }
                            }
                        }
                    }
                    catch
                    {
                        // Keep fallback defaults
                    }
                }

                review.Status = SpecialistReviewStatus.Completed;
                review.CompletedAt = DateTime.UtcNow;
                review.Verdict = verdict;
                review.ReviewNote = reviewNote;
            }
            else
            {
                var specialists = await _context.Users
                    .Where(u => u.Role == UserRole.Specialist && !u.IsDeleted)
                    .ToListAsync(cancellationToken);

                if (specialists.Count == 0)
                {
                    throw new InvalidStateException("No specialists are currently available. Please try again later.");
                }

                var specialistLoads = await _context.ContractSpecialistReviews
                    .Where(r => r.Status == SpecialistReviewStatus.Pending || r.Status == SpecialistReviewStatus.InProgress)
                    .GroupBy(r => r.AssignedSpecialistId)
                    .Select(g => new { SpecialistId = g.Key, Count = g.Count() })
                    .ToListAsync(cancellationToken);

                var loadDict = specialistLoads
                    .Where(l => l.SpecialistId != null)
                    .ToDictionary(l => l.SpecialistId!, l => l.Count);

                User pickedSpecialist = null!;
                int minLoad = int.MaxValue;

                foreach (var spec in specialists)
                {
                    var load = loadDict.TryGetValue(spec.Id, out var count) ? count : 0;
                    if (load < minLoad)
                    {
                        minLoad = load;
                        pickedSpecialist = spec;
                    }
                }

                if (pickedSpecialist == null)
                {
                    pickedSpecialist = specialists[0];
                }

                review.Status = SpecialistReviewStatus.InProgress;
                review.AssignedSpecialistId = pickedSpecialist.Id;
            }

            _context.ContractSpecialistReviews.Add(review);
            await _context.SaveChangesAsync(cancellationToken);

            return review.ToReadDto();
        }
    }
}
