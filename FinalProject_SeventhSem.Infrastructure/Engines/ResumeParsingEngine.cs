using FinalProject_SeventhSem.Application.Common.Settings;
using FinalProject_SeventhSem.Application.Exceptions;
using FinalProject_SeventhSem.Application.Interfaces;
using FinalProject_SeventhSem.Application.Models.Resume;
using FinalProject_SeventhSem.Domain.Entities;
using FinalProject_SeventhSem.Domain.Interfaces;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UglyToad.PdfPig;

namespace FinalProject_SeventhSem.Infrastructure.Engines;


/// <summary>
/// Implements IResumeParsingService.
///
/// Algorithm 1 — Text Preprocessing (Text Normalization):
///   - Remove extra whitespace
///   - Remove special characters
///   - Convert to lowercase
///   - Fix broken lines
///
/// Algorithm 2 — Skill Extraction (Dictionary-Based NER):
///   - Exact match  → confidence 0.9
///   - Alias match  → confidence 0.7
///   - Guards: MinTokenLength, Stopword Exclusion, Duplicate Canonical, Max Cap
/// </summary>

public class ResumeParsingEngine : IResumeParsingService
{
    private readonly ResumeParsingSettings _settings;
    private readonly IRepository<Skill> _skillRepo;
    private readonly IRepository<SkillAlias> _aliasRepo;

    public ResumeParsingEngine(
        IOptions<ResumeParsingSettings> settings,
        IRepository<Skill> skillRepo,
        IRepository<SkillAlias> aliasRepo)
    {
        _settings = settings.Value;
        _skillRepo = skillRepo;
        _aliasRepo = aliasRepo;
    }


    public Task<string> ExtractTextAsync(Stream pdfStream)
    {
        try
        {
            if (pdfStream.CanSeek) pdfStream.Seek(0, SeekOrigin.Begin);

            using var document = PdfDocument.Open(pdfStream);
            var sb = new StringBuilder();

            foreach (var page in document.GetPages())
                sb.AppendLine(page.Text);

            return Task.FromResult(sb.ToString());
        }
        catch (Exception ex)
        {
            throw new BadRequestException($"Failed to extract text from PDF: {ex.Message}");
        }
    }



    //public string PreprocessText(string rawText)
    //{
    //    if (string.IsNullOrWhiteSpace(rawText)) return string.Empty;

    //    // 1. Fix broken lines — join hyphenated line-breaks
    //    var text = Regex.Replace(rawText, @"-\s*\n\s*", "");

    //    // 2. Normalize newlines to spaces
    //    text = Regex.Replace(text, @"[\r\n]+", " ");

    //    // 3. Preserve special-character skills BEFORE stripping
    //    text = Regex.Replace(text, @"\bC\+\+\b", "cplusplus", RegexOptions.IgnoreCase);
    //    text = Regex.Replace(text, @"\bC#\b", "csharp", RegexOptions.IgnoreCase);
    //    text = Regex.Replace(text, @"\bC\b", "clang");                  // ← plain C language
    //    text = Regex.Replace(text, @"\bF#\b", "fsharp", RegexOptions.IgnoreCase);
    //    text = Regex.Replace(text, @"\b\.NET\b", "dotnet", RegexOptions.IgnoreCase);
    //    text = Regex.Replace(text, @"\bASP\.NET\b", "aspnetcore", RegexOptions.IgnoreCase);
    //    text = Regex.Replace(text, @"\bNode\.js\b", "nodejs", RegexOptions.IgnoreCase);
    //    text = Regex.Replace(text, @"\bVue\.js\b", "vuejs", RegexOptions.IgnoreCase);
    //    text = Regex.Replace(text, @"\bExpress\.js\b", "expressjs", RegexOptions.IgnoreCase);
    //    text = Regex.Replace(text, @"\bNext\.js\b", "nextjs", RegexOptions.IgnoreCase);

    //    // 4. Remove special characters, keep letters, digits, spaces
    //    text = Regex.Replace(text, @"[^a-zA-Z0-9\s]", " ");

    //    // 5. Collapse multiple spaces
    //    text = Regex.Replace(text, @"\s{2,}", " ");

    //    // 6. Convert to lowercase
    //    text = text.ToLowerInvariant().Trim();

    //    return text;
    //}


    //public string PreprocessText(string rawText)
    //{
    //    if (string.IsNullOrWhiteSpace(rawText)) return string.Empty;

    //    // 1. Fix broken lines — join hyphenated line-breaks
    //    var text = Regex.Replace(rawText, @"-\s*\n\s*", "");

    //    // 2. Normalize newlines to spaces
    //    text = Regex.Replace(text, @"[\r\n]+", " ");

    //    // 3. Preserve special-character skills (Fixed boundaries)
    //    text = Regex.Replace(text, @"\bC#(?!\w)", "csharp", RegexOptions.IgnoreCase);
    //    text = Regex.Replace(text, @"\bC\+\+(?!\w)", "cplusplus", RegexOptions.IgnoreCase);
    //    text = Regex.Replace(text, @"\b\.NET\b", "dotnet", RegexOptions.IgnoreCase);
    //    text = Regex.Replace(text, @"\bASP\.NET\b", "aspnetcore", RegexOptions.IgnoreCase);
    //    text = Regex.Replace(text, @"\bNode\.js\b", "nodejs", RegexOptions.IgnoreCase);
    //    text = Regex.Replace(text, @"\bVue\.js\b", "vuejs", RegexOptions.IgnoreCase);
    //    text = Regex.Replace(text, @"\bExpress\.js\b", "expressjs", RegexOptions.IgnoreCase);
    //    text = Regex.Replace(text, @"\bNext\.js\b", "nextjs", RegexOptions.IgnoreCase);
    //    text = Regex.Replace(text, @"\bC\b", "clang"); // Kept as clang to safely separate from raw 'c' tokens

    //    // 4. Remove special characters, keep letters, digits, spaces
    //    text = Regex.Replace(text, @"[^a-zA-Z0-9\s]", " ");

    //    // 5. Collapse multiple spaces
    //    text = Regex.Replace(text, @"\s{2,}", " ");

    //    // 6. Convert to lowercase
    //    return text.ToLowerInvariant().Trim();
    //}
    // ── Algorithm 2: Dictionary-Based NER ────────────────────────────────


    public string PreprocessText(string rawText)
    {
        if (string.IsNullOrWhiteSpace(rawText)) return string.Empty;

        var text = Regex.Replace(rawText, @"-\s*\n\s*", "");

        text = Regex.Replace(text, @"[\r\n]+", " ");

        text = Regex.Replace(text, @"\bASP\.NET\s+Core\s+Web\s+API\b", "aspnetcorewebapi", RegexOptions.IgnoreCase);
        text = Regex.Replace(text, @"\bASP\.NET\s+Core\s+MVC\b", "aspnetcoremvc", RegexOptions.IgnoreCase);

        text = Regex.Replace(text, @"\bASP\.NET\b", "aspnetcore", RegexOptions.IgnoreCase);
        text = Regex.Replace(text, @"\b\.NET\b", "dotnet", RegexOptions.IgnoreCase);
        text = Regex.Replace(text, @"\bC#(?!\w)", "csharp", RegexOptions.IgnoreCase);
        text = Regex.Replace(text, @"\bC\+\+(?!\w)", "cplusplus", RegexOptions.IgnoreCase);
        text = Regex.Replace(text, @"\bNode\.js\b", "nodejs", RegexOptions.IgnoreCase);
        text = Regex.Replace(text, @"\bVue\.js\b", "vuejs", RegexOptions.IgnoreCase);
        text = Regex.Replace(text, @"\bExpress\.js\b", "expressjs", RegexOptions.IgnoreCase);
        text = Regex.Replace(text, @"\bNext\.js\b", "nextjs", RegexOptions.IgnoreCase);
        text = Regex.Replace(text, @"\bC\b", "clang");

        text = Regex.Replace(text, @"[^a-zA-Z0-9\s]", " ");

        text = Regex.Replace(text, @"\s{2,}", " ");

        return text.ToLowerInvariant().Trim();
    }
   
    public async Task<ResumeParseResponse> ExtractSkillsAsync(string cleanText)
    {
        var skills = await _skillRepo.GetAllAsync();
        var aliases = await _aliasRepo.GetAllAsync();

        var stopWords = _settings.StopWords.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var minLen = _settings.MinTokenLength;
        var maxCap = _settings.MaxSuggestionsCount;

        var tokens = cleanText
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Where(t => t.Length >= minLen)
            .Where(t => !stopWords.Contains(t))
            .ToHashSet();

        var exactMap = skills.ToDictionary(
            s => s.Name.ToLowerInvariant(),
            s => s.Id);

        var aliasMap = aliases
            .GroupBy(a => a.Alias)
            .ToDictionary(
                g => g.Key,
                g => g.Select(a => a.SkillId).ToList(),
                StringComparer.OrdinalIgnoreCase);

        var skillNameMap = skills.ToDictionary(s => s.Id, s => s.Name);

        var bestPerSkill = new Dictionary<int, (double Confidence, string MatchType)>();

        foreach (var token in tokens)
        {
            if (exactMap.TryGetValue(token, out var exactSkillId))
            {
                var prev = bestPerSkill.GetValueOrDefault(exactSkillId, (0, ""));
                if (0.9 > prev.Confidence)
                    bestPerSkill[exactSkillId] = (0.9, "Exact");
            }

            if (aliasMap.TryGetValue(token, out var matchingSkillIds))
            {
                foreach (var aliasSkillId in matchingSkillIds)
                {
                    var prev = bestPerSkill.GetValueOrDefault(aliasSkillId, (0, ""));
                    if (0.7 > prev.Confidence)
                        bestPerSkill[aliasSkillId] = (0.7, "Alias");
                }
            }
        }

        var suggestions = bestPerSkill
            .OrderByDescending(kv => kv.Value.Confidence)
            .Take(maxCap)
            .Select(kv => new SkillSuggestion(
                SkillId: kv.Key,
                SkillName: skillNameMap.GetValueOrDefault(kv.Key, "Unknown"),
                Confidence: kv.Value.Confidence,
                MatchType: kv.Value.MatchType))
            .ToList();

        return new ResumeParseResponse(
            Suggestions: suggestions,
            TotalExtracted: suggestions.Count);
    }
}

