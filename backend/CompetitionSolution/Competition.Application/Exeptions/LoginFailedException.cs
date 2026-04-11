namespace Competition.Application.Exeptions;

public class LoginFailedException : Exception
{
    public LoginFailedException(string message) : base(message) { }
}