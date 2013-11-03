using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Web;

namespace LockDataService.Model
{
    /// <summary>
    /// Model Class
    /// </summary>
    [DataContract]
    public class UserModel
    {

        [DataMember(Name = "userName")]
        public string UserName { get; set; }

        [DataMember(Name = "hashedClientId")]
        public string HashedClientId { get; set; }

        [DataMember(Name = "salt")]
        public string Salt { get; set; }

        [DataMember(Name = "secret")]
        public string Secret { get; set; }

        private DateTime? _dateCreated;

        private DateTime? _lastLogin;

        //[DataMember(Name = "created")] 
        public String DateCreated
        {
            get { return (_dateCreated != null) ? _dateCreated.Value.ToString(CultureInfo.InvariantCulture) : null; }
        }

        //[DataMember(Name = "loginAttempt")] 
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