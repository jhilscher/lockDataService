//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace LockDataService.Model.Entity
{
    using System;
    using System.Collections.Generic;
    
    public partial class ClientIdentifier
    {
        public int Id { get; set; }
        public string HashedClientId { get; set; }
        public string Salt { get; set; }
        public string Secret { get; set; }
        public string UserName { get; set; }
        public Nullable<System.DateTime> DateCreated { get; set; }
        public Nullable<System.DateTime> LastLogin { get; set; }
    }
}
