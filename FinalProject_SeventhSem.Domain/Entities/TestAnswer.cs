using FinalProject_SeventhSem.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalProject_SeventhSem.Domain.Entities;

public class TestAnswer : BaseEntity
{
    public int TestId { get; set; }
    public int QuestionId { get; set; }

    public string SelectedOption { get; set; } = string.Empty;

    public bool IsCorrect { get; set; }

    public Test Test { get; set; } = null!;
    public Question Question { get; set; } = null!;
}

