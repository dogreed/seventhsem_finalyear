using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalProject_SeventhSem.Application.Models.Stacks;

public record CreateStackRequest(string Name);
public record UpdateStackRequest(string Name);

public record CreateChapterRequest(
    int StackId,
    string Name
);
public record UpdateChapterRequest(string Name);


public record CreateQuestionRequest(
    int StackId,
    int ChapterId,
    string Text,
    string OptionA,
    string OptionB,
    string OptionC,
    string OptionD,
    string CorrectOption
);

public record PatchQuestionRequest(
    string? Text,
    string? OptionA,
    string? OptionB,
    string? OptionC,
    string? OptionD,
    string? CorrectOption
);


public record StackResponse(
    int StackId,
    string Name,
    int ChapterCount,
    int TotalQuestions
);

public record ChapterResponse(
    int ChapterId,
    int StackId,
    string StackName,
    string Name,
    int QuestionCount
);

public record QuestionResponse(
    int QuestionId,
    int ChapterId,
    string ChapterName,
    string StackName,
    string Text,
    string OptionA,
    string OptionB,
    string OptionC,
    string OptionD,
    string CorrectOption
);