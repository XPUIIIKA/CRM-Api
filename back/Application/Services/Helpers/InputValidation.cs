using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Application.Services.Helpers;

public static class InputValidation
{
    private static readonly EmailAddressAttribute EmailValidator = new();
    private static readonly Regex PhoneRegex = new(@"^\+?[0-9\-\s\(\)]{7,20}$", RegexOptions.Compiled);

    public static bool IsValidEmail(string email) => EmailValidator.IsValid(email);

    public static bool IsValidPhone(string phone) =>
        string.IsNullOrWhiteSpace(phone) || PhoneRegex.IsMatch(phone);
}
