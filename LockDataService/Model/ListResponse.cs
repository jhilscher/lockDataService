using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using LockDataService.Model.Entity;

namespace LockDataService.Model
{
    [DataContract]
    public class ListResponse<T>
    {
        [DataMember]
        public List<T> Result { get; set; }
    }
}