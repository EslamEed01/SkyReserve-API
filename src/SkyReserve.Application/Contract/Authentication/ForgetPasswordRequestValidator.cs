﻿using FluentValidation;

namespace SkyReserve.Application.Contract.Authentication
{
    public class ForgetPasswordRequestValidator : AbstractValidator<ForgetPasswordRequest>
    {
        public ForgetPasswordRequestValidator()
        {
            RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();
        }

    }
}
