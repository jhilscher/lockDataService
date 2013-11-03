using System;
using System.Collections.Generic;
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
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "DataService" in code, svc and config file together.
    public class DataService : IDataService
    {

        

        //private readonly IRepository _repository = new Repository();

        private readonly IRepository _repository = new MockRepository();

        public List<UserModel> GetAll()
        {
            return _repository.GetAll();
        }

        public UserModel SendPost(UserModel json)
        {
            _repository.CreateUser(json);
            return json;
        }

        public UserModel RegisterRequest(UserModel json)
        {
            int code = _repository.CreateUser(json);

            if (code < 0)
                throw new WebFaultException(HttpStatusCode.NotAcceptable);

            return json;
        }

        public UserModel Register(UserModel json)
        {
            json.DateTimeCreated = DateTime.Now;
            int code = _repository.CreateUser(json);

            if (code < 0)
                throw new WebFaultException(HttpStatusCode.NotAcceptable);

            return json;
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


        public string ValidateToken(string userName, string token, string timestamp)
        {
            return AuthService.ValidateToken(token, userName, timestamp) ? userName : null;
        }
    }
}
