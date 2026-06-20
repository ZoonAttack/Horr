using System;
using System.IO;
using System.IO.Compression;
using System.Xml.Linq;
using System.Collections.Generic;
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
                var allowedExtensions = new[] { ".txt", ".csv", ".pdf", ".md", ".docx" };
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

                var keywords = ExtractKeywords(request.RequirementsSummary);

                foreach (var attachment in eligibleAttachments)
                {
                    try
                    {
                        var physicalPath = _fileStorageService.GetPhysicalPath(attachment.FileUrl);
                        if (!string.IsNullOrEmpty(physicalPath) && File.Exists(physicalPath))
                        {
                            var ext = Path.GetExtension(attachment.OriginalFileName ?? "")?.ToLowerInvariant() ?? "";
                            string rawContent;
                            if (ext == ".docx")
                            {
                                rawContent = ExtractTextFromDocx(physicalPath);
                            }
                            else
                            {
                                rawContent = File.ReadAllText(physicalPath);
                            }

                            string filteredContent = FilterContentByKeywords(rawContent, keywords);

                            eligibleFilesContext.AppendLine($"--- File: {attachment.OriginalFileName} ---");
                            eligibleFilesContext.AppendLine(filteredContent);
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
If the client requirements summary is empty, generic, or unrelated, evaluate the delivery against general best practices for a freelance project of this type.

RESPOND with ONLY a valid JSON object in this exact format:
{{
  ""verdict"": ""Satisfactory"" or ""Unsatisfactory"",
  ""note"": ""Your detailed reasoning here (2-4 sentences)""
}}
No explanation. Just the JSON object.";

                var specialistReviewSchema = new
                {
                    type = "OBJECT",
                    properties = new
                    {
                        verdict = new { type = "STRING" },
                        note = new { type = "STRING" }
                    },
                    required = new[] { "verdict", "note" }
                };

                string responseText = string.Empty;
                try
                {
                    responseText = await _geminiService.AskAsync(prompt, specialistReviewSchema);
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
        private static List<string> ExtractKeywords(string summary)
        {
            if (string.IsNullOrWhiteSpace(summary)) return new List<string>();

            var stopWords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "the", "a", "an", "and", "or", "but", "if", "then", "else", "when", "at", "by", "for", "with", "about", 
                "against", "between", "into", "through", "during", "before", "after", "above", "below", "to", "from", 
                "up", "down", "in", "out", "on", "off", "over", "under", "again", "further", "then", "once", "here", 
                "there", "all", "any", "both", "each", "few", "more", "most", "other", "some", "such", "no", "nor", 
                "not", "only", "own", "same", "so", "than", "too", "very", "s", "t", "can", "will", "just", "don", 
                "should", "now", "i", "me", "my", "myself", "we", "our", "ours", "ourselves", "you", "your", "yours",
                "yourself", "yourselves", "he", "him", "his", "himself", "she", "her", "hers", "herself", "it", "its",
                "itself", "they", "them", "their", "theirs", "themselves", "what", "which", "who", "whom", "this", 
                "that", "these", "those", "am", "is", "are", "was", "were", "be", "been", "being", "have", "has", "had", 
                "having", "do", "does", "did", "doing", "please", "need", "should", "must", "want", "require", "requirements"
            };

            return summary
                .Split(new[] { ' ', '.', ',', ';', ':', '?', '!', '\r', '\n', '(', ')', '[', ']', '{', '}' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(w => w.Trim().ToLowerInvariant())
                .Where(w => w.Length > 2 && !stopWords.Contains(w))
                .Distinct()
                .ToList();
        }

        private static string FilterContentByKeywords(string fullText, List<string> keywords)
        {
            if (string.IsNullOrWhiteSpace(fullText)) return string.Empty;
            if (keywords == null || !keywords.Any())
            {
                return fullText.Length > 5000 ? fullText.Substring(0, 5000) + "..." : fullText;
            }

            var segments = fullText.Split(new[] { "\r\n\r\n", "\n\n", "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            var matchedSegments = new List<string>();

            foreach (var segment in segments)
            {
                var lowerSegment = segment.ToLowerInvariant();
                if (keywords.Any(k => lowerSegment.Contains(k)))
                {
                    matchedSegments.Add(segment.Trim());
                }
            }

            if (!matchedSegments.Any())
            {
                return fullText.Length > 5000 ? fullText.Substring(0, 5000) + "..." : fullText;
            }

            var merged = string.Join("\n\n", matchedSegments);
            return merged.Length > 8000 ? merged.Substring(0, 8000) + "..." : merged;
        }

        private static string ExtractTextFromDocx(string filePath)
        {
            try
            {
                using (var archive = ZipFile.OpenRead(filePath))
                {
                    var entry = archive.GetEntry("word/document.xml");
                    if (entry == null) return string.Empty;

                    using (var stream = entry.Open())
                    {
                        var doc = XDocument.Load(stream);
                        if (doc.Root == null) return string.Empty;

                        XNamespace w = "http://schemas.openxmlformats.org/wordprocessingml/2006/main";
                        var textElements = doc.Descendants(w + "t");
                        return string.Join(" ", textElements.Select(e => e.Value));
                    }
                }
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}
