using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Web;
using LockDataService.Model;
using LockDataService.Model.Repository;

namespace LockDataService.Service
{
    /// <summary>
    /// Service that handles the registration of a client.
    /// </summary>
    public class RegistrationService
    {

        private static readonly Repository Repository = new Repository();

        private const int RandomByteSize = 64;

        /// <summary>
        /// Request to register a client.
        /// </summary>
        /// <param name="sId">id of</param>
        /// <returns></returns>
        public static string RegisterRequest(string sId)
        {
            // generate random value
            string clientIdKey = generateRandom();
            
            // add the value to the waitlist
            RegistrationHandler.AddToWaitList(sId, clientIdKey);

            return clientIdKey;
        }

        /// <summary>
        /// Generates a secure random key.
        /// </summary>
        /// <returns>The key as string.</returns>
        private static string generateRandom()
        {
            RNGCryptoServiceProvider csprng = new RNGCryptoServiceProvider();
            byte[] salt = new byte[RandomByteSize];
            csprng.GetBytes(salt);
            return PasswordHash.ByteArrayToString(salt);
        }


        /// <summary>
        /// Confirms the registration and saves it to db.
        /// </summary>
        /// <param name="user"></param>
        /// <returns>Boolean if successful.</returns>
        public static bool ConfirmRegistration(UserModel user)
        {
            String storedClientId = RegistrationHandler.GetClientId(user.UserName);

            if (!storedClientId.Equals(user.HashedClientId))
                return false;

            // generate salt
            string salt = generateRandom();

            // hash the cient id
            byte[] hashedClientIdKeyBytes = PasswordHash.PBKDF2(storedClientId, PasswordHash.StringToByteArray(salt), 1000, 64);
            
            // convert it to string
            string hashedClientIdKey = PasswordHash.ByteArrayToString(hashedClientIdKeyBytes);

            Repository.CreateUser(new UserModel
                {
                    HashedClientId = hashedClientIdKey,
                    Salt = salt,
                    Secret = user.Secret,
                    UserName = user.UserName
                });
            user = null;

            return true;
        }
    }
}