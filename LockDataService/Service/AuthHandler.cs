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
        private static readonly Dictionary<string, string> Tokens = new Dictionary<string, string>();

        /// <summary>
        /// Container for login log data.
        /// </summary>
        private static readonly Dictionary<string, LoginLog> Logins = new Dictionary<string, LoginLog>();

        /// <summary>
        /// Time, when the token should be automaticly deleted.
        /// </summary>
        private const int Timeframe = 1000*60*2; // 2mins

        /// <summary>
        /// Adds a token to the waitlist.
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="token"></param>
        public static void AddToWaitList(string userName, string token, LoginLog loginLog)
        {
            token = token.ToUpper();

            // will override old tokens
            Tokens[userName] = token;

            if (Logins.ContainsKey(userName))
            {
                Repository.SetLoginSuccess(Logins[userName], false);
            }

            Logins[userName] = loginLog;

            // Start Timer
            Timer removeTimer = new Timer();
            removeTimer.Interval = Timeframe;
            removeTimer.Elapsed += (sender, e) => RemoveFromWaitlist(sender, e, userName);
            removeTimer.Enabled = true;

        }

        /// <summary>
        /// Method triggered after time, to remove the token.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <param name="userName"></param>
        private static void RemoveFromWaitlist(object sender, ElapsedEventArgs e, string userName)
        {
            Tokens.Remove(userName);

            if (Logins.ContainsKey(userName))
            {
                Repository.SetLoginSuccess(Logins[userName], false);

                Logins.Remove(userName);
            }

            // disable timer
            ((Timer)sender).Dispose();
        }

        /// <summary>
        /// Gets a token and removes it from the map.
        /// </summary>
        /// <param name="userName">UserName</param>
        /// <returns>Token</returns>
        public static string GetToken(string userName)
        {
            string token = Tokens[userName];
            
            // remove the token.
            Tokens.Remove(userName);

            return token;
        }

        /// <summary>
        /// Gets a Login Log and removes it from the map.
        /// </summary>
        /// <param name="userName">UserName</param>
        /// <returns>LoginInLog</returns>
        public static LoginLog GetLoginLog(string userName)
        {
            var login = Logins[userName];

            // remove the login.
            Logins.Remove(userName);

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
            var user = Tokens.FirstOrDefault(x => x.Value.Equals(token)).Key;

            if (user != null)
                Tokens.Remove(user);

            return user;
        }
    }
}