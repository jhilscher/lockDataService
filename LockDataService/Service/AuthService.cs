using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using LockDataService.Model;
using LockDataService.Model.Repository;


namespace LockDataService.Service
{
    public class AuthService
    {
        private static readonly Repository Repository = new Repository();

        private const int RandomByteSize = 256;

        private const int Iterations = 1000;

        private const int HashByteSize = 64;

        //private static readonly Dictionary<String, byte[]> TokenMap = new Dictionary<string, byte[]>();

        /// <summary>
        /// XORs to byte arrays
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns>XORed byte array</returns>
        public static byte[] Xor(byte[] a, byte[] b)
        {
            int nLen = Math.Min(a.Length, b.Length);
            byte[] c = new byte[nLen];

            for (int i = 0; i < nLen; i++)
                c[i] = (byte) (a[i] ^ b[i]);

            return c;
        }

        public static String GenerateToken(String userName)
        {
            UserModel user = Repository.GetUserByUserName(userName);
            if (user == null)
                throw new ArgumentException("User not found.");

            RNGCryptoServiceProvider random = new RNGCryptoServiceProvider();

            // random bytes
            byte[] token = new byte[RandomByteSize];
            random.GetBytes(token);

            // user secret
            String secret = user.Secret;
            byte[] secretBytes = PasswordHash.StringToByteArray(secret);

            // xor to alpha
            byte[] alpha = Xor(secretBytes, token);

            // Timestamp
            int timeStamp = (int)DateTime.Now.ToUniversalTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;

            // hashed token
            byte[] hashedToeken = PasswordHash.PBKDF2(PasswordHash.ByteArrayToString(token),
                                                      BitConverter.GetBytes(timeStamp),
                                                      Iterations, HashByteSize);

            // Add to map
            //TokenMap.Add(userName, hashedToeken);
            //TokenMap[userName] = hashedToeken;
            AuthHandler.AddToWaitList(userName, hashedToeken);

            // return alpha + epoch timestamp 
            return PasswordHash.ByteArrayToString(alpha) + "#" + Convert.ToString(timeStamp);

        }

        public static Boolean ValidateToken(String token, String userName, String timeStamp)
        {
            byte[] oldHashedToken = AuthHandler.GetToken(userName);


            bool result = PasswordHash.ValidatePassword(token, PasswordHash.ByteArrayToString(oldHashedToken));

            // to validate token
            //byte[] tokenToValidate = PasswordHash.StringToByteArray(token);

            //bool result = PasswordHash.SlowEquals(oldHashedToken, tokenToValidate);

            return result;
        }
    }
}