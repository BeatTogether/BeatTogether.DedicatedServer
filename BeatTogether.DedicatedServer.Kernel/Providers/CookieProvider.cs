using System.Security.Cryptography;
using BeatTogether.DedicatedServer.Kernel.Abstractions;

namespace BeatTogether.DedicatedServer.Kernel.Providers
{
    public class CookieProvider : ICookieProvider
    {
        private const int CookieLength = 32;

        private readonly RandomNumberGenerator _rngCryptoServiceProvider;

        public CookieProvider(RandomNumberGenerator rngCryptoServiceProvider)
        {
            _rngCryptoServiceProvider = rngCryptoServiceProvider;
        }

        public byte[] GetCookie()
        {
            var cookie = new byte[CookieLength];
            _rngCryptoServiceProvider.GetBytes(cookie);
            return cookie;
        }
    }
}
