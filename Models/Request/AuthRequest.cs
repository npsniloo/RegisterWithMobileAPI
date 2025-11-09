using System.ComponentModel.DataAnnotations;

namespace RegisterApp.Models.Request
{
    public class AuthRequest
    {
        [RegularExpression(@"^(?:989\d{9}|09\d{9})$", ErrorMessage = "Mobile must be 989123456789 or 09123456789.")]
        public string MobileNumber { get; set; } = "";
    }
}
