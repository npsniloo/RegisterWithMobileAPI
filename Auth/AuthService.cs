using Microsoft.EntityFrameworkCore;
using RegisterApp.DB;
using RegisterApp.Entities;
using RegisterApp.Interfaces;

namespace RegisterApp.Auth
{
    public class AuthService : IAuthService
    {
        private const string login = "login";
        private const string register = "register";
        private readonly OtpService otp;
        private readonly JwtService jwtSvc;
        private readonly IUserRepository userRepository;
        private readonly IPhoneNormalizer phoneNormalizer; 
        public AuthService(IPhoneNormalizer phoneNormalizer, OtpService otp, JwtService jwtSvc, IUserRepository userRepository)
        {
            this.phoneNormalizer = phoneNormalizer;
            this.otp = otp;
            this.jwtSvc = jwtSvc;
            this.userRepository = userRepository;
        }

        public async Task<(bool ok,string? message)> RegisterAsync(string mobileNumber, CancellationToken cancellationToken)
        {
            var phone = phoneNormalizer.NormalizeToE164(mobileNumber);

            //check user exists
            var exists = await userRepository.UserExist(phone, cancellationToken);
            if (exists) return (false,"Phone already registered. Use login.");

            //check otp code has sent before
            var sentBefore = await otp.HasOtpSentBefore(phone, register);
            if(sentBefore) return (false, "Please wait before requesting another code.");
          
            //create and send otp code
            await otp.CreateAndSendCodeAsync(phone, register, userId: null, cancellationToken);
            return (true, "Verification code sent." );
        }

        public async Task<(bool ok, string? token, string? tokenType, string? error)> VerifyRegisterAsync(string mobileNumber, string code, CancellationToken cancellationToken)
        {
            //verify code
            var phone = phoneNormalizer.NormalizeToE164(mobileNumber);
            var (ok, ticket, error) = await otp.VerifyAsync(phone, code, register, cancellationToken);
            if (!ok) 
                return (false, token: null, tokenType:null, error );

            // Create user 
            var user = await userRepository.GetUser(phone, cancellationToken);
            if (user is null)
            {
                user = new AppUser { PhoneE164 = phone, DisplayName = phone, PhoneVerified = true };
                await userRepository.AddUser(user, cancellationToken);
            }
            else
            {
                user.PhoneVerified = true;
            }
            //Create authentication token
            var token = jwtSvc.IssueToken(user);

            //update user login date
            user.LastLoginAt = DateTimeOffset.UtcNow;
            await userRepository.UpdateUser(user, cancellationToken);
            return (true,  token, tokenType: "Bearer", "");
        }

        public async Task<(bool ok, string? message)> LoginAsync(string mobileNumber, CancellationToken cancellationToken)
        {
            var phone = phoneNormalizer.NormalizeToE164(mobileNumber);
            
            //check user exists
            var user = await userRepository.GetUser(phone, cancellationToken);
            if (user is null) return(false, "No account for this phone. Register first.");
            
            //check sms has sent before
            var sentBefore = await otp.HasOtpSentBefore(phone, login);
            if (sentBefore) return (false, "Please wait before requesting another code.");
            
            //create and send otp code
            await otp.CreateAndSendCodeAsync(phone, login, user.Id, cancellationToken);
            return (true, "Login code sent.");
        }

        public async Task<(bool ok, string? token, string? tokenType, string? error)> VerifyLoginAsync(string mobileNumber, string code, CancellationToken cancellationToken)
        {
            var phone = phoneNormalizer.NormalizeToE164(mobileNumber);

            //get user
            var user = await userRepository.GetUser(phone, cancellationToken);
            if (user is null) return (false,token:null, tokenType:"", error : "No account for this phone." );

            //verify code
            var (ok, ticket, error) = await otp.VerifyAsync(phone, code, login, cancellationToken);
            if (!ok) return (false, token: null, tokenType: null, error);

            
            user.LastLoginAt = DateTimeOffset.UtcNow;
            if (!user.PhoneVerified) 
                user.PhoneVerified = true;


            //Create authentication token
            var token = jwtSvc.IssueToken(user);
            
            //update user
            await userRepository.UpdateUser(user, cancellationToken);

            return (true, token, tokenType : "Bearer" ,"");
        }
    }
}
