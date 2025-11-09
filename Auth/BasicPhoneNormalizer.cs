using RegisterApp.Interfaces;
using System.Numerics;
using System.Text.RegularExpressions;

namespace RegisterApp.Auth
{
    public class IRANPhoneNormalizer : IPhoneNormalizer
    {
        // DEMO ONLY: assumes numbers are already country-coded or Iran local.
        public string NormalizeToE164(string phone)
        {
            phone = phone?.Trim() ?? "";
            if (Regex.IsMatch(phone, @"^09\d{9}$"))
                return "98" + phone.Substring(1); // 09XXXXXXXXX -> 989XXXXXXXXX
            if (Regex.IsMatch(phone, @"^989\d{9}$"))
                return phone;
            throw new ArgumentException("Invalid phone format.");
        }
    }
}