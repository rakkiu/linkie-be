using FluentValidation;

namespace Application.Usecase.Auth.Login
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="AbstractValidator&lt;LoginCommand&gt;" />
    public class LoginValidator : AbstractValidator<LoginCommand>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LoginValidator"/> class.
        /// </summary>
        public LoginValidator()
        {
            RuleFor(x => x.Email).NotEmpty().EmailAddress().WithMessage("Email is required.");
            RuleFor(x => x.Password).NotEmpty().MinimumLength(6).WithMessage("Password must be at least 6 characters.");
        }
    }
}
