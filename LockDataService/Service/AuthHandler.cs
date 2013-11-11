using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using System.Web;


namespace LockDataService.Service
{
    /// <summary>
    /// Holds Tokens to be authenticated.
    /// Deletes the Tokens after some time, if there is no confirmation of the token.
    /// </summary>
    public class AuthHandler
    {
        private static readonly Dictionary<string, string> Tokens = new Dictionary<string, string>();

        private const int Timeframe = 1000*60*10; // 2mins

        /// <summary>
        /// Adds a token to the waitlist.
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="token"></param>
        public static void AddToWaitList(string userName, string token)
        {
            token = token.ToUpper();

            // will override old tokens
            Tokens[userName] = token;

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
        /// Gets the username (key) by the token.
        /// </summary>
        /// <param name="token">Token</param>
        /// <returns>UserName</returns>
        public static string GetUserName(string token)
        {
            token = token.ToUpper();

            // searches the key of the token (value).
            return Tokens.FirstOrDefault(x => x.Value.Equals(token)).Key;
        }
    }
}