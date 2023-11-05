using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;

namespace App
{
    public static class _let
    {
        public static bool first = true;
        public static void print(string? s)
        {
            // Console.BackgroundColor = ConsoleColor.Blue;
            // Console.ForegroundColor = ConsoleColor.White;//after this line every text will be white on blue background
            // System.Console.WriteLine(s);
            // Console.ResetColor();//reset to the defoult colour

            if (string.IsNullOrEmpty(s))
            {
                System.Console.WriteLine("====>  Null");
            }
            else
            {
                System.Console.WriteLine("====>  " + s);

            }

        }
        public static bool CheckNumberLetter(string stringToTest)
        {
            string pattern = @"^[A-Za-z0-9_-]*$";
            Regex regex = new Regex(pattern);

            // Compare a string against the regular expression
            return regex.IsMatch(stringToTest);
        }
        public static bool CheckEmail(string email)
        {
            string pattern = @"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$";
            Regex regex = new Regex(pattern);

            // Compare a string against the regular expression
            return regex.IsMatch(email);
        }
        public static string Random(int n)
        {
            Random rd = new Random();
            const string allowedChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz0123456789";
            char[] chars = new char[n];

            for (int i = 0; i < n; i++)
            {
                chars[i] = allowedChars[rd.Next(0, allowedChars.Length)];
            }
            string kq = new string(chars);
            return kq;
        }
        public static long GetTokenExpirationTime(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtSecurityToken = handler.ReadJwtToken(token);
            var tokenExp = jwtSecurityToken.Claims.First(claim => claim.Type.Equals("exp")).Value;
            var ticks = long.Parse(tokenExp);
            return ticks;
        }

        public static bool CheckTokenIsValid(string token)
        {
            try
            {
                var tokenTicks = GetTokenExpirationTime(token);
                var tokenDate = DateTimeOffset.FromUnixTimeSeconds(tokenTicks).UtcDateTime;

                var now = DateTime.Now.ToUniversalTime();

                var valid = tokenDate >= now;

                return valid;
            }
            catch
            {
                return false;
            }

        }
        public static string Unaccents(this string s)
        {

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            byte[] tempBytes;
            tempBytes = System.Text.Encoding.GetEncoding("ISO-8859-8").GetBytes(s);
            string asciiStr = System.Text.Encoding.UTF8.GetString(tempBytes);
            return asciiStr;
        }


















        public static string GenerateSlug(string str, bool hierarchical = true)
        {
            var slug = str.Trim().ToLower();

            string[] decomposed = new string[] { "à","á","ạ","ả","ã","â","ầ","ấ","ậ","ẩ","ẫ","ă",
                                                       "ằ","ắ","ặ","ẳ","ẵ","è","é","ẹ","ẻ","ẽ","ê","ề" ,
                                                       "ế","ệ","ể","ễ", "ì","í","ị","ỉ","ĩ", "ò","ó","ọ",
                                                       "ỏ","õ","ô","ồ","ố","ộ","ổ","ỗ","ơ" ,"ò","ớ","ợ","ở",
                                                       "õ", "ù","ú","ụ","ủ","ũ","ư","ừ","ứ","ự","ử","ữ",
                                                       "ỳ","ý","ỵ","ỷ","ỹ", "đ",
                                                       "À","À","Ạ","Ả","Ã","Â","Ầ","Ấ","Ậ","Ẩ","Ẫ","Ă" ,
                                                       "Ằ","Ắ","Ặ","Ẳ","Ẵ", "È","É","Ẹ","Ẻ","Ẽ","Ê","Ề",
                                                       "Ế","Ệ","Ể","Ễ", "Ì","Í","Ị","Ỉ","Ĩ", "Ò","Ó","Ọ","Ỏ",
                                                       "Õ","Ô","Ồ","Ố","Ộ","Ổ","Ỗ","Ơ" ,"Ờ","Ớ","Ợ","Ở","Ỡ",
                                                       "Ù","Ú","Ụ","Ủ","Ũ","Ư","Ừ","Ứ","Ự","Ử","Ữ", "Ỳ","Ý","Ỵ",
                                                       "Ỷ","Ỹ", "Đ"};
            string[] precomposed =  {  "à","á","ạ","ả","ã","â","ầ","ấ","ậ","ẩ","ẫ","ă",
                                             "ằ","ắ","ặ","ẳ","ẵ","è","é","ẹ","ẻ","ẽ","ê","ề" ,
                                             "ế","ệ","ể","ễ", "ì","í","ị","ỉ","ĩ", "ò","ó","ọ","ỏ",
                                             "õ","ô","ồ","ố","ộ","ổ","ỗ","ơ" ,"ờ","ớ","ợ","ở","ỡ", "ù",
                                             "ú","ụ","ủ","ũ","ư","ừ","ứ","ự","ử","ữ", "ỳ","ý","ỵ","ỷ","ỹ",
                                             "đ", "À","Á","Ạ","Ả","Ã","Â","Ầ","Ấ","Ậ","Ẩ","Ẫ","Ă" ,"Ằ","Ắ",
                                             "Ặ","Ẳ","Ẵ", "È","É","Ẹ","Ẻ","Ẽ","Ê","Ề","Ế","Ệ","Ể","Ễ", "Ì",
                                             "Í","Ị","Ỉ","Ĩ", "Ò","Ó","Ọ","Ỏ","Õ","Ô","Ồ","Ố","Ộ","Ổ","Ỗ",
                                             "Ơ" ,"Ờ","Ớ","Ợ","Ở","Ỡ", "Ù","Ú","Ụ","Ủ","Ũ","Ư","Ừ","Ứ","Ự",
                                             "Ử","Ữ", "Ỳ","Ý","Ỵ","Ỷ","Ỹ", "Đ"};
            string[] latin =  { "a","a","a","a","a","a","a","a","a","a","a" ,
                                   "a","a","a","a","a","a", "e","e","e","e","e",
                                   "e","e","e","e","e","e", "i","i","i","i","i", "o",
                                   "o","o","o","o","o","o","o","o","o","o","o" ,"o","o","o","o","o",
                                   "u","u","u","u","u","u","u","u","u","u","u", "y","y","y","y","y", "d",
                                   "a","a","a","a","a","a","a","a","a","a","a","a" ,"a","a","a","a","a",
                                   "e","e","e","e","e","e","e","e","e","e","e", "i","i","i","i","i", "o",
                                   "o","o","o","o","o","o","o","o","o","o","o" ,"o","o","o","o","o", "u",
                                   "u","u","u","u","u","u","u","u","u","u", "y","y","y","y","y", "d"};

            // Convert culture specific characters
            for (int i = 0; i < decomposed.Length; i++)
            {
                slug = slug.Replace(decomposed[i], latin[i]);
                slug = slug.Replace(precomposed[i], latin[i]);
            }

            // Remove special characters
            slug = Regex.Replace(slug, @"[^a-z0-9-/ ]", "").Replace("--", "-");

            // Remove whitespaces
            slug = Regex.Replace(slug.Replace("-", " "), @"\s+", " ").Replace(" ", "-");

            // Remove slash if non-hierarchical
            if (!hierarchical)
                slug = slug.Replace("/", "-");

            // Remove multiple dashes
            slug = Regex.Replace(slug, @"[-]+", "-");

            // Remove leading & trailing dashes
            if (slug.EndsWith("-"))
                slug = slug.Substring(0, slug.LastIndexOf("-"));
            if (slug.StartsWith("-"))
                slug = slug.Substring(Math.Min(slug.IndexOf("-") + 1, slug.Length));
            return slug;
        }

    }
    public static class ClaimsPrincipalExtensions
    {
        public static T GetLoggedInUserId<T>(this ClaimsPrincipal principal)
        {
            if (principal == null)
                throw new ArgumentNullException(nameof(principal));

            var loggedInUserId = principal.FindFirstValue(ClaimTypes.NameIdentifier);

            if (typeof(T) == typeof(string))
            {
                return (T)Convert.ChangeType(loggedInUserId, typeof(T));
            }
            else if (typeof(T) == typeof(int) || typeof(T) == typeof(long))
            {
                return loggedInUserId != null ? (T)Convert.ChangeType(loggedInUserId, typeof(T)) : (T)Convert.ChangeType(0, typeof(T));
            }
            else
            {
                throw new Exception("Invalid type provided");
            }
        }

        public static string GetLoggedInUserName(this ClaimsPrincipal principal)
        {
            if (principal == null)
                throw new ArgumentNullException(nameof(principal));

            return principal.FindFirstValue(ClaimTypes.Name);
        }

        public static string GetLoggedInUserEmail(this ClaimsPrincipal principal)
        {
            if (principal == null)
                throw new ArgumentNullException(nameof(principal));

            return principal.FindFirstValue(ClaimTypes.Email);
        }
    }

}
// public class UserRepository 
// {
//     private readonly IHttpContextAccessor _httpContextAccessor;

//     public UserRepository(IHttpContextAccessor httpContextAccessor) =>
//         _httpContextAccessor = httpContextAccessor;

//     public void LogCurrentUser()
//     {
//         var username = _httpContextAccessor.HttpContext.User.Identity.Name;
//         _
//         // ...
//     }
// }