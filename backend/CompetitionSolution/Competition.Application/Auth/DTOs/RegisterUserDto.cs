namespace Competition.Application.Auth.DTOs;

public record RegisterUserDto(string Email, string Password, string FirstName, string LastName,string PhoneNumber);