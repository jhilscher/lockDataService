using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace LockDataService.Model
{
    [DataContract]
    public class LoginLogModel
    {
        public int UserId { get; set; }

        [DataMember(Name = "success")]
        public Nullable<int> Success { get; set; }

        [DataMember(Name = "ipAdress")]
        public string IpAdress { get; set; }

        [DataMember(Name = "userAgent")]
        public string UserAgent { get; set; }

        [DataMember(Name = "timeStamp")]
        public Nullable<System.DateTime> TimeStamp { get; set; }

     
    }
}