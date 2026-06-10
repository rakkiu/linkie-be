namespace Application.Usecase.Auth.EmailVerification
{
    public class VerifyEmailRequest
    {
        public string Token { get; set; } = string.Empty;
    }
}
