using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using System.Web;


namespace LockDataService.Service
{
    public class AuthHandler
    {
        private static readonly Dictionary<string, string> Tokens = new Dictionary<string, string>();

        private const int Timeframe = 1000*60*2; // 2mins


        public static void AddToWaitList(string userName, string token)
        {
            token = token.ToUpper();

            // will override old tokens
            Tokens[userName] = token;

            // Start Timer
            //Timer removeTimer = new Timer();
            //removeTimer.Interval = Timeframe;
            //removeTimer.Elapsed += (sender, e) => RemoveFromWaitlist(sender, e, userName);
            //removeTimer.Enabled = true;

        }

        private static void RemoveFromWaitlist(object sender, ElapsedEventArgs e, string userName)
        {
            Tokens.Remove(userName);
        }

        public static string GetToken(string userName)
        {
            return Tokens[userName];
        }

        public static string GetUser(string token)
        {
            token = token.ToUpper();
            return Tokens.FirstOrDefault(x => x.Value.Equals(token)).Key;
        }
    }
}