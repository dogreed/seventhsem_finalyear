using FinalProject_SeventhSem.Application.Models.Stacks;
using FluentValidation;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalProject_SeventhSem.Application.Features.Admin.Commands.ManageQuestion;

//public record CreateQuestionCommand(
//    int ChapterId,
//    string Text,
//    string OptionA,
//    string OptionB,
//    string OptionC,
//    string OptionD,
//    string CorrectOption
//) : IRequest<QuestionResponse>;

public record CreateQuestionCommand(
    int StackId,
    int ChapterId,
    string Text,
    string OptionA,
    string OptionB,
    string OptionC,
    string OptionD,
    string CorrectOption
) : IRequest<QuestionResponse>;

public class CreateQuestionCommandValidator : AbstractValidator<CreateQuestionCommand>
{
    private static readonly string[] ValidOptions = ["A", "B", "C", "D"];

    public CreateQuestionCommandValidator()
    {
        RuleFor(x => x.StackId).GreaterThan(0);
        RuleFor(x => x.ChapterId).GreaterThan(0);
        RuleFor(x => x.Text).NotEmpty().MaximumLength(1000);
        RuleFor(x => x.OptionA).NotEmpty().MaximumLength(500);
        RuleFor(x => x.OptionB).NotEmpty().MaximumLength(500);
        RuleFor(x => x.OptionC).NotEmpty().MaximumLength(500);
        RuleFor(x => x.OptionD).NotEmpty().MaximumLength(500);
        RuleFor(x => x.CorrectOption)
            .NotEmpty()
            .Must(o => ValidOptions.Contains(o.ToUpper()))
            .WithMessage("CorrectOption must be A, B, C, or D.");
    }
}

public class PatchQuestionCommandValidator : AbstractValidator<PatchQuestionCommand>
{
    private static readonly string[] ValidOptions = ["A", "B", "C", "D"];

    public PatchQuestionCommandValidator()
    {
        RuleFor(x => x.Text).NotEmpty().MaximumLength(1000).When(x => x.Text != null);
        RuleFor(x => x.OptionA).NotEmpty().MaximumLength(500).When(x => x.OptionA != null);
        RuleFor(x => x.OptionB).NotEmpty().MaximumLength(500).When(x => x.OptionB != null);
        RuleFor(x => x.OptionC).NotEmpty().MaximumLength(500).When(x => x.OptionC != null);
        RuleFor(x => x.OptionD).NotEmpty().MaximumLength(500).When(x => x.OptionD != null);

        RuleFor(x => x.CorrectOption)
            .Must(o => ValidOptions.Contains(o!.ToUpper()))
            .WithMessage("CorrectOption must be A, B, C, or D.")
            .When(x => !string.IsNullOrEmpty(x.CorrectOption));
    }
}
