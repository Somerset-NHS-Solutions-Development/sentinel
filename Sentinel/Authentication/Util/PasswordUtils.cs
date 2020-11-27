using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Sentinel.Util
{
    public class PasswordUtils
    {
        public static readonly int ITERATIONS = 100000;

        // Generate a random password of the specified size
        public static string GetUniqueKey(int size = 6, string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890")
        {
            using (var crypto = new RNGCryptoServiceProvider())
            {
                var data = new byte[size];

                // If chars.Length isn't a power of 2 then there is a bias if
                // we simply use the modulus operator. The first characters of
                // chars will be more probable than the last ones.

                // buffer used if we encounter an unusable random byte. We will
                // regenerate it in this buffer
                byte[] smallBuffer = null;

                // Maximum random number that can be used without introducing a
                // bias
                int maxRandom = byte.MaxValue - ((byte.MaxValue + 1) % chars.Length);

                crypto.GetBytes(data);

                var result = new char[size];

                for (int i = 0; i < size; i++)
                {
                    byte v = data[i];

                    while (v > maxRandom)
                    {
                        if (smallBuffer == null)
                        {
                            smallBuffer = new byte[1];
                        }

                        crypto.GetBytes(smallBuffer);
                        v = smallBuffer[0];
                    }

                    result[i] = chars[v % chars.Length];
                }

                return new string(result);
            }
        }

        // Hash the supplied password, returning the hash and the salt
        public static string HashPassword(string password, out byte[] salt)
        {
            // generate a 128-bit salt using a secure PRNG
            salt = new byte[128 / 8];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            // derive a 256-bit subkey (use HMACSHA512 with specified number of iterations)
            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA512,
                iterationCount: ITERATIONS,
                numBytesRequested: 256 / 8));

            return hashed;
        }

        // Vaidate a password against a hash that was generated with HashPassword()
        public static bool ValidatePassword(string password, string hashed, byte[] salt)
        {
            string toMatch = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA512,
                iterationCount: ITERATIONS,
                numBytesRequested: 256 / 8));

            return hashed == toMatch;
        }
    }
}
