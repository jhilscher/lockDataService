using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LockDataService.Model.Entity;

namespace LockDataService.Model
{
    public class WaitlistItem 
    {
        public LoginLog LoginLog { get; set; }

        public String HashedToken { get; set; }

        protected bool Equals(WaitlistItem other)
        {
            return string.Equals(HashedToken, other.HashedToken) && Equals(LoginLog, other.LoginLog);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((WaitlistItem) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((HashedToken != null ? HashedToken.GetHashCode() : 0)*397) ^ (LoginLog != null ? LoginLog.GetHashCode() : 0);
            }
        }
    }
}