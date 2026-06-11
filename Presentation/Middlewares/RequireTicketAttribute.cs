namespace Presentation.Middlewares
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class RequireTicketAttribute : Attribute
    {
    }
}
