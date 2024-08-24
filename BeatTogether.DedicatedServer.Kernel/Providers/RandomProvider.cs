using System.Security.Cryptography;
using BeatTogether.DedicatedServer.Kernel.Abstractions;

namespace BeatTogether.DedicatedServer.Kernel.Providers
{
    public class RandomProvider : IRandomProvider
    {
        private const int RandomLength = 32;

        private readonly RandomNumberGenerator _rngCryptoServiceProvider;

        public RandomProvider(RandomNumberGenerator rngCryptoServiceProvider)
        {
            _rngCryptoServiceProvider = rngCryptoServiceProvider;
        }

        public byte[] GetRandom()
        {
            var random = new byte[RandomLength];
            _rngCryptoServiceProvider.GetBytes(random);
            return random;
        }
    }
}
