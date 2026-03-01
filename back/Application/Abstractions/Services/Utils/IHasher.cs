namespace Application.Abstractions.Services.Utils;

public interface IHasher
{
    string Hash(string password);
    bool Verify(string password, string hash);
}