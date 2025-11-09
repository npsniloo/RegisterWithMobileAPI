using FluentValidation;
using RegisterApp.Models.Request;
using System.Text.RegularExpressions;

namespace RegisterApp.Validation
{
    public class RegisterRequestValidator : AbstractValidator<AuthRequest>
    {
        public RegisterRequestValidator()
        {
            RuleFor(x => x.MobileNumber)
                .NotEmpty()
                .Matches(@"^(?:989\d{9}|09\d{9})$")
                .WithMessage("Mobile must be 989123456789 or 09123456789");
        }
    }
}
