using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using System.Web;

namespace LockDataService.Service
{
    public class RegistrationHandler
    {
        /// <summary>
        /// Dictonary, as container for tokens.
        /// </summary>
        private static readonly Dictionary<string, string> Tokens = new Dictionary<string, string>();

        /// <summary>
        /// Timespan, after a token should be automaticly deleted. In milliseconds.
        /// </summary>
        private const int Timeframe = 1000 * 60 * 2; // 2mins

        /// <summary>
        /// Adds a token to the waitlist.
        /// </summary>
        /// <param name="id">userName, acts as key for the dictonary.</param>
        /// <param name="token">generated token.</param>
        public static void AddToWaitList(string id, string token)
        {
            token = token.ToUpper();

            // will override old tokens
            Tokens[id] = token;

            // Start Timer
            Timer removeTimer = new Timer();
            removeTimer.Interval = Timeframe;
            removeTimer.Elapsed += (sender, e) => RemoveFromWaitlist(sender, e, id);
            removeTimer.Enabled = true;

        }

        /// <summary>
        /// Removes a token from the waitlist.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <param name="userName">userName as key.</param>
        private static void RemoveFromWaitlist(object sender, ElapsedEventArgs e, string userName)
        {
            Tokens.Remove(userName);

            // disable timer
            ((Timer) sender).Dispose();
        }

        /// <summary>
        /// Returns a token by it's key (userName).
        /// </summary>
        /// <param name="id">userName</param>
        /// <returns>token as string</returns>
        public static string GetClientId(string id)
        {
            return Tokens[id];
        }


    }
}