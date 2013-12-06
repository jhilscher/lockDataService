using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Web;
using LockDataService.Model;
using LockDataService.Model.Entity;
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
        /// Length of the RSA-Key.
        /// </summary>
        private const int KeyLength = 2048;
        

        /// <summary>
        /// Generates a one time token.
        /// </summary>
        /// <param name="userModel">UserModel</param>
        /// <returns>Token as string</returns>
        public static string GenerateToken(UserModel userModel)
        {
            UserModel user = Repository.GetUserByUserName(userModel.UserName);

            if (user == null)
                throw new ArgumentException("User not found.");

            if (user.Status != 1)
                throw new AccessViolationException();

            if(!Repository.CalculateCurrentRisk(userModel.UserName, userModel.UserAgent, userModel.IpAdress))
                throw new AccessViolationException();


            RNGCryptoServiceProvider random = new RNGCryptoServiceProvider();

            // random bytes
            byte[] token = new byte[RandomByteSize];
            random.GetBytes(token);

            string tokenString = PasswordHash.ByteArrayToString(token);   

            // user secret
            string secret = user.Secret;

                 

            // Timestamp
            string timeStamp =  Math.Abs((long)DateTime.Now.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).ToLocalTime()).TotalMilliseconds).ToString();
           
            // convert to byte array
            byte[] timeBytes = PasswordHash.StringToByteArray(BitConverter.ToString(GetBytes(timeStamp)).Replace("-", "")); 


            byte[] combinedArray = new byte[token.Length + timeBytes.Length];

            for (int i = 0; i < combinedArray.Length; i++)
            {
                combinedArray[i] = i < token.Length ? token[i] : timeBytes[i - token.Length];
            }

            var hexTime = BitConverter.ToString(GetBytes(timeStamp)).Replace("-", "");
            var desc = tokenString + hexTime;

            // encrypt
            byte[] alpha = EncryptString(combinedArray, secret);

            // hashed token
            byte[] hashedToken = PasswordHash.PBKDF2(tokenString.ToUpper(),
                                                     timeBytes,
                                                     Iterations, HashByteSize);

            // convert to string
            string hashedTokenString = PasswordHash.ByteArrayToString(hashedToken);

            // save login attempt to log
            LoginLog loginLog = Repository.AddLogEntry(userModel);

            // add to waitlist
            AuthHandler.AddToWaitList(user.ClientId, hashedTokenString, loginLog);


            string result = PasswordHash.ByteArrayToString(alpha);
            // return alpha + epoch timestamp 
            return result;

        }

        /// <summary>
        /// Encryptes a byte-array.
        /// </summary>
        /// <param name="input">Input to encryt</param>
        /// <param name="publicKey">publicKey as string, xml-encoded.</param>
        /// <returns></returns>
        private static byte[] EncryptString(byte[] input, string publicKey)
        {

            RSACryptoServiceProvider rsaCryptoServiceProvider = new RSACryptoServiceProvider(KeyLength);
            
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

            if (String.IsNullOrEmpty(clientID) || String.IsNullOrEmpty(token))
                return String.Empty;

            // gets the old token from the waitlist
            string tokenFromWaitlist = AuthHandler.GetToken(clientID);

            LoginLog loginLog = AuthHandler.GetLoginLog(clientID);

            if (loginLog == null || String.IsNullOrEmpty(tokenFromWaitlist))
                return String.Empty;

            Repository.SetLoginSuccess(loginLog, false);

            token = token.ToUpper();

            // check token
            if (!token.Equals(tokenFromWaitlist))
                return String.Empty;

            UserModel model = Repository.GetUserByClientId(clientID);

            if (model == null)
                return String.Empty;
            
            // Set log to success: true
            Repository.SetLoginSuccess(loginLog, true);
            AuthHandler.RemoveToken(clientID);

            return model.UserName;
        }

        /// <summary>
        /// Helper to convert a string to a byte array.
        /// </summary>
        /// <param name="str">input string</param>
        /// <returns>input converted to byte array.</returns>
        private static byte[] GetBytes(string str)
        {
            byte[] bytes = new byte[str.Length * sizeof(char)];
            Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return Encoding.UTF8.GetBytes(str); 
        }
    }
}