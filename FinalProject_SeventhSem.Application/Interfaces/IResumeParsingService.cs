using FinalProject_SeventhSem.Application.Models.Resume;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalProject_SeventhSem.Application.Interfaces;

public interface IResumeParsingService
{
    
    Task<string> ExtractTextAsync(Stream pdfStream);

   
    string PreprocessText(string rawText);

    
    Task<ResumeParseResponse> ExtractSkillsAsync(string cleanText);
}
