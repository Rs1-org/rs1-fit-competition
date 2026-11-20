namespace Competition.Application.Exeptions;

public class ResourceConflictException(string propertyName, string message) : Exception(message)
{
    public string PropertyName { get; } = propertyName;
}