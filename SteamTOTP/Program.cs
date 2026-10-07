using SG = SteamGuard.TOTP.SteamGuard;

namespace SteamTOTP
{
    public sealed class Program
    {
        public static void Main(string[] args)
            => Console.WriteLine(new SG().GenerateAuthenticationCode(args[0]));
    }
}
