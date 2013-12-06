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

        /// <summary>
        /// Repository for db-access.
        /// </summary>
        private static readonly IRepository Repository = new Repository();
        //private static readonly IRepository Repository = new MockRepository();

        /// <summary>
        /// Size of the random salt in bytes.
        /// </summary>
        private const int RandomByteSize = 64;

        /// <summary>
        /// Request to register a client.
        /// </summary>
        /// <param name="userName">UserName</param>
        /// <returns>ClientIdKey</returns>
        public static string RegisterRequest(string userName)
        {
            // generate random value
            string clientIdKey = generateRandom();
            
            // add the value to the waitlist
            RegistrationHandler.AddToWaitList(userName, clientIdKey);

            return clientIdKey;
        }

        /// <summary>
        /// Generates a secure random key using RNGCryptoServiceProvider.
        /// </summary>
        /// <returns>The random value as string.</returns>
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
        /// <param name="user">UserModel, with ClientId, Secret, UserName</param>
        /// <returns>Boolean if successful.</returns>
        public static bool ConfirmRegistration(UserModel user)
        {
            // try to get clientId from the RegistrationHandler-Dictonary
            String storedClientId = RegistrationHandler.GetClientId(user.UserName);

            if (storedClientId == null)
                return false;

            // Check if clientId is equals with the stored one
            if (!storedClientId.Equals(user.ClientId))
                return false;

            Repository.CreateUser(new UserModel
                {
                    ClientId = storedClientId,
                    Secret = user.Secret,
                    UserName = user.UserName,
                    Status = 1 // set Status to registered
                });
            user = null;

            return true;
        }
    }
}