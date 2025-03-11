using FluentValidation;

namespace Chat.Api;

public class MessageDtoValidator : AbstractValidator<MessageDto>
{
    public MessageDtoValidator()
    {
        RuleFor(x => x.Text)
            .NotEmpty()
            .WithMessage("Message text cannot be empty");

        RuleFor(x => x.Sender.Email)
            .NotEmpty()
            .WithMessage("Email address is required")
            .EmailAddress()
            .WithMessage("A valid email address is required");

        RuleFor(x => x.Sender.Name)
            .NotEmpty()
            .WithMessage("Sender name is required");
    }
}