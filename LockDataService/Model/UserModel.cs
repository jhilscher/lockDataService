﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Web;

namespace LockDataService.Model
{
    /// <summary>
    /// Model Class.
    /// Used as container for JSON.
    /// </summary>
    [DataContract]
    public class UserModel
    {

        [DataMember(Name = "userName")]
        public string UserName { get; set; }

        [DataMember(Name = "clientId")]
        public string ClientId { get; set; }

        /// <summary>
        /// Represents the status of this user.
        /// 
        /// 0: User Registration requested 
        /// 1: User is registered / ok
        /// 2: User is blocked
        /// </summary>
        [DataMember(Name = "status")]
        public int Status { get; set; }

        [DataMember(Name = "secret")]
        public string Secret { get; set; }

        [DataMember(Name = "token")]
        public string Token { get; set; }

        private DateTime? _dateCreated;

        [DataMember(Name = "loginAttempt")] 
        private DateTime? _lastLogin;

        [DataMember(Name = "ipAdress")] 
        public string IpAdress { get; set; }

        [DataMember(Name = "userAgent")]
        public string UserAgent { get; set; }



        //[DataMember(Name = "created")] 
        public String DateCreated
        {
            get { return (_dateCreated != null) ? _dateCreated.Value.ToString(CultureInfo.InvariantCulture) : null; }
        }


        public String LastLogin
        {
            get { return (_lastLogin != null) ? _lastLogin.Value.ToString(CultureInfo.InvariantCulture) : null; }
        }

        public DateTime? DateTimeCreated
        {
            set { _dateCreated = value;  }
            get { return _dateCreated;  }
        }

        public DateTime? DateTimeLogin
        {
            set { _lastLogin = value; }
            get { return _lastLogin; }
        }
    }
}