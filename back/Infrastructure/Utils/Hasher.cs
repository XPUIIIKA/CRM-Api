using Application.Abstractions.Services.Utils;
using BC = BCrypt.Net.BCrypt;

namespace Infrastructure.Utils;

public class Hasher : IHasher
{
    public string Hash(string password)
    {
        return BC.HashPassword(password);
    }
    public bool Verify(string password, string hash)
    {
        return BC.Verify(password, hash);
    }
}