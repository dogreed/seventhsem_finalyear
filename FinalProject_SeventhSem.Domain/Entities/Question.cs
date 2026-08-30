using FinalProject_SeventhSem.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalProject_SeventhSem.Domain.Entities;

public class Question : BaseEntity
{
    public int ChapterId { get; set; }
    public string Text { get; set; } = string.Empty;
    public string OptionA { get; set; } = string.Empty;
    public string OptionB { get; set; } = string.Empty;
    public string OptionC { get; set; } = string.Empty;
    public string OptionD { get; set; } = string.Empty;

    public string CorrectOption { get; set; } = string.Empty;

    public Chapter Chapter { get; set; } = null!;
    public ICollection<StudentSeenQuestion> SeenByStudents { get; set; } = new List<StudentSeenQuestion>();
}
