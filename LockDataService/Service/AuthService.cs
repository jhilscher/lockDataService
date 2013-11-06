using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using LockDataService.Model;
using LockDataService.Model.Repository;


namespace LockDataService.Service
{
    /// <summary>
    /// Service that handles authentification.
    /// </summary>
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

        /// <summary>
        /// Generates a one time token.
        /// </summary>
        /// <param name="userName">UserName</param>
        /// <returns>Token as string</returns>
        public static string GenerateToken(string userName)
        {
            UserModel user = Repository.GetUserByUserName(userName);
            if (user == null)
                throw new ArgumentException("User not found.");

            RNGCryptoServiceProvider random = new RNGCryptoServiceProvider();

            // random bytes
            byte[] token = new byte[RandomByteSize];
            random.GetBytes(token);

            // user secret
            string secret = user.Secret;
            byte[] secretBytes = PasswordHash.StringToByteArray(secret);

            string tValue = PasswordHash.ByteArrayToString(token);
            
            // take substring
            tValue = tValue.Substring(0, 64);

            // xor to alpha
            byte[] alpha = Xor(secretBytes, token);

            // Timestamp
            string timeStamp =  Math.Abs((long)DateTime.Now.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds).ToString();
           
            // convert to byte array
            byte[] timeBytes = GetBytes(timeStamp);

            // hashed token
            byte[] hashedToken = PasswordHash.PBKDF2(tValue.ToUpper(),
                                                     timeBytes,
                                                     Iterations, HashByteSize);

            // convert to string
            string hashedValue = PasswordHash.ByteArrayToString(hashedToken);

            AuthHandler.AddToWaitList(userName, hashedValue);

            // return alpha + epoch timestamp 
            return PasswordHash.ByteArrayToString(alpha) + "#" + timeStamp;

        }

        /// <summary>
        /// Validates a token.
        /// </summary>
        /// <param name="token">Token</param>
        /// <param name="clientID">client id, not hashed</param>
        /// <returns>bool, if token is valid</returns>
        public static Boolean ValidateToken(String token, String clientID)
        {
            // gets the old token from the authhandler
            string userNameFromMap = AuthHandler.GetUserName(token);

            if (userNameFromMap == null)
                return false;

            UserModel model = Repository.GetUserByUserName(userNameFromMap);

            if (model == null)
                return false;

            //generate Hash of ClientId
            byte[] hashedClientId = PasswordHash.PBKDF2(clientID, PasswordHash.StringToByteArray(model.Salt), Iterations, HashByteSize);
            string hashedClientIdString = PasswordHash.ByteArrayToString(hashedClientId);


            if (!model.HashedClientId.Equals(hashedClientIdString))
                return false;
            

            // updated user with login date
            model.DateTimeLogin = DateTime.Now;
            Repository.UpdateUser(model);
            

            return true;
        }


        public static UserModel GetUserByToken(string token)
        {
            string username = AuthHandler.GetUserName(token);
            return Repository.GetUserByUserName(username);
        }

        /// <summary>
        /// Helper to convert a string to a byte array.
        /// </summary>
        /// <param name="str">input string</param>
        /// <returns>input converted to byte array.</returns>
        private static byte[] GetBytes(string str)
        {
            byte[] bytes = new byte[str.Length * sizeof(char)];
            System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return System.Text.Encoding.UTF8.GetBytes(str); 
        }
    }
}