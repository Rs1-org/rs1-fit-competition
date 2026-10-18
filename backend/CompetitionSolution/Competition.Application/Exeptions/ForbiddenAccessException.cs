namespace Competition.Application.Exeptions;

public class ForbiddenAccessException(string message) : Exception(message);
