using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using System.Web;
using LockDataService.Model;
using LockDataService.Model.Entity;
using LockDataService.Model.Repository;


namespace LockDataService.Service
{
    /// <summary>
    /// Holds Tokens to be authenticated.
    /// Deletes the Tokens after some time, if there is no confirmation of the token.
    /// </summary>
    public class AuthHandler
    {

        /// <summary>
        /// Repository to access the db.
        /// </summary>
        private static readonly IRepository Repository = new Repository();

        //private static readonly IRepository Repository = new MockRepository();

        /// <summary>
        /// Container for tokens to be validated.
        /// </summary>
        private static readonly Dictionary<string, WaitlistItem> Items = new Dictionary<string, WaitlistItem>();

        /// <summary>
        /// Time, when the token should be automaticly deleted.
        /// </summary>
        private const int Timeframe = 1000*60*2; // 2mins

        /// <summary>
        /// Adds a token to the waitlist.
        /// </summary>
        /// <param name="clientId">clientId: ID_A</param>
        /// <param name="hashedToken">hashed token</param>
        /// <param name="loginLog">Log-Object</param>
        public static void AddToWaitList(string clientId, string hashedToken, LoginLog loginLog)
        {

            if (String.IsNullOrEmpty(clientId) || String.IsNullOrEmpty(hashedToken) || loginLog == null)
                throw new ArgumentException("No null values or empty strings allowed.");

            hashedToken = hashedToken.ToUpper();

            // sets overridden log to success: false.
            if (Items.ContainsKey(clientId))
            {
                Repository.SetLoginSuccess(Items[clientId].LoginLog, false);
            }

            // will override old entry
            Items[clientId] = new WaitlistItem
                {
                    HashedToken = hashedToken,
                    LoginLog = loginLog
                };

            // Start Timer
            Timer removeTimer = new Timer {Interval = Timeframe};
            removeTimer.Elapsed += (sender, e) => RemoveFromWaitlist(sender, e, clientId);
            removeTimer.Enabled = true;

        }

        /// <summary>
        /// Removes Waitlist-entries by username.
        /// </summary>
        /// <param name="sender">sender-Object</param>
        /// <param name="e">EventArgs</param>
        /// <param name="clientId">ClientID</param>
        private static void RemoveFromWaitlist(object sender, ElapsedEventArgs e, string clientId)
        {

            if (Items.ContainsKey(clientId))
            {
                // if in waitlist, set log success to false and remove entry
                Repository.SetLoginSuccess(Items[clientId].LoginLog, false);

                Items.Remove(clientId);
            }

            // disable timer
            ((Timer)sender).Dispose();
        }

        /// <summary>
        /// Gets a token and removes it from the map.
        /// </summary>
        /// <param name="clientId">cliendId: ID_A</param>
        /// <returns>Token</returns>
        public static string GetToken(string clientId)
        {
            if (!Items.ContainsKey(clientId))
                return null;

            string token = Items[clientId].HashedToken;
            
            // remove the token.
            //Items.Remove(clientId);

            return token;
        }

        public static void RemoveToken(string clientId)
        {
            if (Items.ContainsKey(clientId))
                Items.Remove(clientId);
        }

        /// <summary>
        /// Gets a Login Log and removes it from the map.
        /// </summary>
        /// <param name="clientID">UserName</param>
        /// <returns>LoginInLog</returns>
        public static LoginLog GetLoginLog(string clientID)
        {
            LoginLog login = null;
            if (Items.ContainsKey(clientID))
                login = Items[clientID].LoginLog;

            // remove the login.
            //Items.Remove(clientID);

            return login;
        }

        /// <summary>
        /// Gets the username (key) by the token.
        /// </summary>
        /// <param name="token">Token</param>
        /// <returns>UserName</returns>
        public static string GetUserName(string token)
        {
            token = token.ToUpper();

            // searches the key of the token (value).
            var user = Items.FirstOrDefault(x => x.Value.HashedToken.Equals(token)).Key;

            if (user != null)
                Items.Remove(user);

            return user;
        }
    }
}