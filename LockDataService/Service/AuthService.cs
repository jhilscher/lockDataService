using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
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
        private static readonly IRepository Repository = new Repository();
        //private static readonly IRepository Repository = new MockRepository();

        /// <summary>
        /// Size of the token in bytes.
        /// </summary>
        private const int RandomByteSize = 117; // max for 1024bit RSA

        /// <summary>
        /// Iterations of the pbkdf2-hash-function.
        /// </summary>
        private const int Iterations = 1000;

        /// <summary>
        /// Size of the hash in bytes.
        /// </summary>
        private const int HashByteSize = 64;


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

            string tValue = PasswordHash.ByteArrayToString(token);
            
            // encrypt
            byte[] alpha = EncryptString(token, secret);



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

            // add to waitlist
            AuthHandler.AddToWaitList(userName, hashedValue);

            // return alpha + epoch timestamp 
            return PasswordHash.ByteArrayToString(alpha) + "#" + timeStamp;

        }

        /// <summary>
        /// Encryptes a byte-array.
        /// </summary>
        /// <param name="input">Input to encryt</param>
        /// <param name="publicKey">publicKey as string, xml-encoded.</param>
        /// <returns></returns>
        private static byte[] EncryptString(byte[] input, string publicKey)
        {

            RSACryptoServiceProvider rsaCryptoServiceProvider = new RSACryptoServiceProvider(2048);
            
            // import key as xml
            rsaCryptoServiceProvider.FromXmlString(publicKey);

            // encrypt it
            byte[] encrypted = rsaCryptoServiceProvider.Encrypt(input, false);

            return encrypted;
        }

        /// <summary>
        /// Validates a token.
        /// </summary>
        /// <param name="token">Token</param>
        /// <param name="clientID">client id, not hashed</param>
        /// <returns>bool, if token is valid</returns>
        public static string ValidateToken(String token, String clientID)
        {
            // gets the old token from the authhandler
            string userNameFromMap = AuthHandler.GetUserName(token);

            if (userNameFromMap == null)
                return String.Empty;

            UserModel model = Repository.GetUserByUserName(userNameFromMap);

            if (model == null)
                return String.Empty;

            //generate Hash of ClientId
            byte[] hashedClientId = PasswordHash.PBKDF2(clientID, PasswordHash.StringToByteArray(model.Salt), Iterations, HashByteSize);
            string hashedClientIdString = PasswordHash.ByteArrayToString(hashedClientId);


            if (!model.HashedClientId.Equals(hashedClientIdString))
                return String.Empty;
            

            // updated user with login date
            model.DateTimeLogin = DateTime.Now;
            Repository.UpdateUser(model);
            

            return model.UserName;
        }

        /// <summary>
        /// Gets a User in the waitlist by it's token.
        /// </summary>
        /// <param name="token">Token</param>
        /// <returns>UserModel</returns>
        public static UserModel GetUserByToken(string token)
        {
            string username = AuthHandler.GetUserName(token);

            if (username == null)
                return null;

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