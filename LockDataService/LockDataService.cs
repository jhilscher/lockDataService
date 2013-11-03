using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Web;
using LockDataService.Interfaces;

namespace LockDataService
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class LockDataService : ILockDataService
    {
        public void setId(int id)
        {
            
        }
    }
}