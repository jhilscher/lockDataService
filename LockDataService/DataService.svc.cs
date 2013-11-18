using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using LockDataService.Model;
using LockDataService.Model.Entity;
using LockDataService.Model.Repository;
using LockDataService.Service;

namespace LockDataService
{

    public class DataService : IDataService
    {

        /// <summary>
        /// Repository to access the db.
        /// </summary>
        private readonly IRepository _repository = new Repository();

        //private readonly IRepository _repository = new MockRepository();

      
        public void ConfirmRegister(UserModel json)
        {

            if (!RegistrationService.ConfirmRegistration(json))
                throw new WebFaultException(HttpStatusCode.NotAcceptable);

        }

        public string RequestRegister(UserModel json)
        {
            string resp = RegistrationService.RegisterRequest(json.UserName);

            if (resp == null)
                throw new WebFaultException(HttpStatusCode.NotAcceptable);
            return resp;
        }

        public int Delete(string userName)
        {
            int code = _repository.DeleteUser(userName);
            if (code < 1)
                throw new WebFaultException(HttpStatusCode.NoContent);
            return code;
        }

        public string GetToken(UserModel user)
        {
            string token;
            try
            {
                token = AuthService.GenerateToken(user);
            }
            catch (ArgumentException e)
            {
                throw new WebFaultException(HttpStatusCode.NoContent);
            }
            return token;
        }

        public string ValidateToken(UserModel json)
        {
            return AuthService.ValidateToken(json.Token, json.HashedClientId);
        }

        public UserModel GetUserData(string userName)
        {
            var user = _repository.GetUserByUserName(userName);

            if (user == null)
                return null;

            // map only relevant data to model
            var userModel = new UserModel
                {
                    UserName = user.UserName,
                    DateTimeCreated = user.DateTimeCreated,
                    DateTimeLogin = user.DateTimeLogin
                };

            return userModel;
        }

        public List<LoginLogModel> GetLogsFromUser(string userName)
        {
            var list = _repository.GetLogsFromUser(userName);
            return list;
        }

        public double GetCurrentRisk(UserModel user)
        {
            return _repository.CalculateRisk(user.UserName, user.UserAgent, user.IpAdress);
        }
    }
}
