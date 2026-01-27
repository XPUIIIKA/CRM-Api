namespace Application.Abstractions.Services.Utils;

public interface IPasswordGenerator
{
    string Generate(int length = 12);
}