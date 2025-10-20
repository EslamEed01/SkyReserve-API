using FluentValidation;

namespace SkyReserve.Application.Contract.Authentication
{
    public class ResendConfirmationEmailRequestValidator : AbstractValidator<ResendConfirmationEmailRequest>
    {

        public ResendConfirmationEmailRequestValidator()
        {
            RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();
        }
    }
}
