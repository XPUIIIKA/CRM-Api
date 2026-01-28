using System.Security.Cryptography;
using System.Text;
using Application.Abstractions.Services.Utils;

namespace Infrastructure.Utils;

public class PasswordHasher : IPasswordGenerator
{
    private const string UpperCase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private const string LowerCase = "abcdefghijklmnopqrstuvwxyz";
    private const string Digits = "0123456789";
    private const string Specials = "!@#$%^&*()_-+=";
    private const string AllChars = UpperCase + LowerCase + Digits + Specials;

    public string Generate(int length = 12)
    {
        if (length < 8) length = 8;

        var password = new StringBuilder();
        
        password.Append(GetRandomChar(UpperCase));
        password.Append(GetRandomChar(LowerCase));
        password.Append(GetRandomChar(Digits));
        password.Append(GetRandomChar(Specials));

        for (int i = password.Length; i < length; i++)
        {
            password.Append(GetRandomChar(AllChars));
        }

        return new string(password.ToString().ToCharArray().OrderBy(s => RandomNumberGenerator.GetInt32(100)).ToArray());
    }

    private static char GetRandomChar(string charSet)
    {
        var index = RandomNumberGenerator.GetInt32(charSet.Length);
        return charSet[index];
    }
}