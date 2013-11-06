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

        public List<UserModel> GetAll()
        {
            return _repository.GetAll();
        }

        public UserModel SendPost(UserModel json)
        {
            _repository.CreateUser(json);
            return json;
        }

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

        public UserModel GetUser(string userName)
        {
            UserModel user = _repository.GetUserByUserName(userName);
            if (user == null)
                throw new WebFaultException(HttpStatusCode.NoContent);
            return user;
        }

        public int Delete(string userName)
        {
            int code = _repository.DeleteUser(userName);
            if (code < 1)
                throw new WebFaultException(HttpStatusCode.NoContent);
            return code;
        }

        /// <summary>
        /// Set Login attempt.
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public int Update(UserModel json)
        {
            json.DateTimeLogin = DateTime.Now;
            return _repository.UpdateUser(json);
        }

        public string GetToken(string userName)
        {
            string token;
            try
            {
                token = AuthService.GenerateToken(userName);
            }
            catch (ArgumentException e)
            {
                throw new WebFaultException(HttpStatusCode.NoContent);
            }
            return token;
        }

        public string ValidateToken(UserModel json)
        {
            return AuthService.ValidateToken(json.Token, json.HashedClientId) ? GetUserToken(json.Token).UserName : null;
        }

        public UserModel GetUserToken(string token)
        {
            UserModel user = AuthService.GetUserByToken(token);
            if (user == null)
                throw new WebFaultException(HttpStatusCode.NoContent);
            return user;
        }
    }
}
