namespace RegisterApp.Models.Response
{
    public class VerifyResponse
    {
        public string Token { get; }
        public string TokenType { get;}
        public VerifyResponse(string token, string tokenType)
        {
            Token = token; 
            TokenType = tokenType;
        }


    }
}
