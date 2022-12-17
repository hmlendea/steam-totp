using SG = SteamGuard.TOTP.SteamGuard;

namespace SteamTOTP
{
    public sealed class Program
    {
        public static void Main(string[] args)
        {
            SG steamGuard = new SG();
            string code = steamGuard.GenerateAuthenticationCode(args[0]);
            Console.WriteLine(code);
        }
    }
}
