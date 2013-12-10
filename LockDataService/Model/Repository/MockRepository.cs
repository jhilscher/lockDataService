using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using LockDataService.Model.Entity;
using LockDataService.Model;

namespace LockDataService.Model.Repository 
{
    /// <summary>
    /// Mock Repository without DB-Access.
    /// </summary>
    public class MockRepository : IRepository
    {

        public static MockEntities Entities = new MockEntities();

        public class MockEntities
        {
            public List<ClientIdentifier> ClientIdentifier = new List<ClientIdentifier>();

            public List<LoginLog> LoginLog = new List<LoginLog>();

            public int SaveChanges()
            {
                return 1;
            }

        }

        /// <summary>
        /// Creates a new UserModel, or updates it, if it already exists.
        /// </summary>
        /// <param name="userModel">UserModel,</param>
        /// <returns>Number of affectes rows, should be 1.</returns>
        public int CreateUser(UserModel userModel)
        {
            // check
            if (Entities.ClientIdentifier.FirstOrDefault(x => x.UserName == userModel.UserName) != null)
            {
                return UpdateUser(userModel);
            }

            userModel.DateTimeCreated = DateTime.Now;
            Entities.ClientIdentifier.Add(ConvertToClientIdentifier(userModel));

            return Entities.SaveChanges();
        }

        /// <summary>
        /// Returns a UserModel by it's id.
        /// </summary>
        /// <param name="id">id as integer.</param>
        /// <returns>UserModel or null, if none found.</returns>
        public UserModel GetUserById(int id)
        {
            return ConvertToUserModel(Entities.ClientIdentifier.FirstOrDefault(x => x.Id == id));
        }

        /// <summary>
        /// Returns a UserModel by it's userName.
        /// </summary>
        /// <param name="userName">UserName as string.</param>
        /// <returns>UserModel or null.</returns>
        public UserModel GetUserByUserName(string userName)
        {
            UserModel userModel =
                ConvertToUserModel(Entities.ClientIdentifier.FirstOrDefault(x => x.UserName == userName));

            return userModel;
        }

        /// <summary>
        /// Gets a user by it's clientId.
        /// </summary>
        /// <param name="clientId">string clientId</param>
        /// <returns>UserModel</returns>
        public UserModel GetUserByClientId(string clientId)
        {
            UserModel userModel =
                ConvertToUserModel(Entities.ClientIdentifier.FirstOrDefault(x => x.ClientId == clientId));

            return userModel;
        }

        /// <summary>
        /// Returns all UserModels.
        /// </summary>
        /// <returns>List of UserModels or null if none found.</returns>
        public List<UserModel> GetAll()
        {
            return Entities.ClientIdentifier.ToList().Select(ConvertToUserModel).ToList();
        }

        /// <summary>
        /// Updates a User.
        /// </summary>
        /// <param name="userModel">UserModel with new Data.</param>
        /// <returns>Number of affected rows, should be 1.</returns>
        public int UpdateUser(UserModel userModel)
        {
            ClientIdentifier clientIdentifier =
                Entities.ClientIdentifier.FirstOrDefault(x => x.UserName == userModel.UserName);

            if (clientIdentifier != null)
            {
                clientIdentifier.LastLogin = DateTime.Now;
                clientIdentifier.ClientId = userModel.ClientId;
                clientIdentifier.Status = userModel.Status;
                clientIdentifier.Secret = userModel.Secret;
            }


            return Entities.SaveChanges();
        }

        /// <summary>
        /// Adds a login attempt to a user's log.
        /// </summary>
        /// <param name="userModel">UserModel with new Data.</param>
        /// <returns>Number of affected rows, should be 1.</returns>
        public LoginLog AddLogEntry(UserModel userModel)
        {
            ClientIdentifier clientIdentifier =
                Entities.ClientIdentifier.FirstOrDefault(x => x.UserName == userModel.UserName);

            LoginLog loginLog = new LoginLog
                {
                    IpAdress = userModel.IpAdress,
                    TimeStamp = DateTime.Now,
                    UserAgent = userModel.UserAgent,
                    Success = 0

                };


            if (clientIdentifier != null)
            {
                clientIdentifier.LastLogin = DateTime.Now;

                if (clientIdentifier.LoginLog == null)
                    clientIdentifier.LoginLog = new Collection<LoginLog>();

                clientIdentifier.LoginLog.Add(loginLog);
            }


            Entities.SaveChanges();

            return loginLog;
        }

        /// <summary>
        /// Gets the logs list.
        /// </summary>
        /// <param name="userName">UserName</param>
        /// <returns>List of Logs</returns>
        public List<LoginLogModel> GetLogsFromUser(string userName)
        {
            var firstOrDefault = Entities.ClientIdentifier.Where(x => x.UserName.Equals(userName)).FirstOrDefault();
            if (firstOrDefault != null && firstOrDefault.LoginLog != null)
                return firstOrDefault.LoginLog.Select(ConvertToLoginLogModel).ToList();



            return null;
        }

        /// <summary>
        /// Deletes a user.
        /// </summary>
        /// <param name="userName">UserName of the Row.</param>
        /// <returns>Number of affected rows.</returns>
        public int DeleteUser(string userName)
        {

            ClientIdentifier cId = Entities.ClientIdentifier.FirstOrDefault(x => x.UserName.Equals(userName));

            // removes related logs
            foreach (var log in Entities.LoginLog.Where(l => l.UserId == cId.Id))
            {
                Entities.LoginLog.Remove(log);
            }


            Entities.ClientIdentifier.Remove(cId);
            return Entities.SaveChanges();
        }

        /// <summary>
        /// Sets the success of a login log entry.
        /// </summary>
        /// <param name="model">LoginLog</param>
        /// <param name="success">Boolean if successful</param>
        /// <returns>Number of affected rows. Should be 1.</returns>
        public int SetLoginSuccess(LoginLog model, bool success)
        {
            model.Success = success ? 1 : 0;
            model.TimeStamp = DateTime.Now;

            return Entities.SaveChanges();
        }

        /// <summary>
        /// Calaculates a risk-ranking for a request
        /// </summary>
        /// <param name="userName">UserName</param>
        /// <param name="userAgent">UserAgent</param>
        /// <param name="ipAdress">IpAdress</param>
        /// <returns>Double value of the risk-ranking, > 0 should be considered secure.</returns>
        public double CalculateRisk(string userName, string userAgent, string ipAdress)
        {
            //double risk = 0.0;

            var firstOrDefault = Entities.ClientIdentifier.Where(x => x.UserName.Equals(userName)).FirstOrDefault();

            if (firstOrDefault != null && firstOrDefault.LoginLog != null)
            {

                int total = firstOrDefault.LoginLog.Count();
                //int hits = firstOrDefault.LoginLog.Count(x => x.IpAdress.Equals(ipAdress));s

                // take successfull logins coming from the same ip range (last adress block is ignored)
                int closeIpHits = firstOrDefault.LoginLog.Where(x => x.Success != null && x.Success.Value == 1)
                                                .Count(
                                                    x =>
                                                    x.IpAdress.Contains(ipAdress.Substring(0, ipAdress.LastIndexOf('.'))));

                int closeIpFails = firstOrDefault.LoginLog.Where(x => x.Success != null && x.Success.Value == 0)
                                                 .Count(
                                                     x =>
                                                     x.IpAdress.Contains(ipAdress.Substring(0, ipAdress.LastIndexOf('.'))));

                if (total == 0 || closeIpHits == 0)
                    return 0.0; // first login

                int s = closeIpHits - closeIpFails;

                DateTime latest = firstOrDefault.LoginLog.Where(x => x.Success != null && x.Success.Value == 1)
                                                .Max(x => x.TimeStamp.Value);

                var diff = DateTime.Now - latest;
                int hours = diff.Hours;


                double order = Math.Log10(Math.Max(Math.Abs(s), 1));
                int y = (s > 0) ? 1 : (s < 0) ? -1 : 0;

                var result = y*order - hours/48; // 48 is as fixed value, to set hours in relation


                return result;
            }

            return 0.0;
        }

        #region converter

        /// <summary>
        /// Converter.
        /// </summary>
        /// <param name="loginLog"></param>
        /// <returns>LoginLogModel</returns>
        private static LoginLogModel ConvertToLoginLogModel(LoginLog loginLog)
        {
            if (loginLog == null)
                return null;

            return new LoginLogModel
                {
                    IpAdress = loginLog.IpAdress.Trim(),
                    Success = loginLog.Success,
                    TimeStamp = loginLog.TimeStamp,
                    UserAgent = loginLog.UserAgent.Trim()
                };
        }

        /// <summary>
        /// Converter.
        /// </summary>
        /// <param name="userModel">UserModel</param>
        /// <returns>ClientIdentigier, or null if UserModel is null.</returns>
        private static ClientIdentifier ConvertToClientIdentifier(UserModel userModel)
        {
            if (userModel == null)
                return null;

            return new ClientIdentifier
                {
                    ClientId = userModel.ClientId,
                    Status = userModel.Status,
                    Secret = userModel.Secret,
                    DateCreated = userModel.DateTimeCreated,
                    LastLogin = userModel.DateTimeLogin,
                    UserName = userModel.UserName,
                    LoginLog = null
                };
        }

        /// <summary>
        /// Converter.
        /// </summary>
        /// <param name="clientIdentifier">ClientIdentifier.</param>
        /// <returns>UserModel, or null if ClientIdentifier is null.</returns>
        private static UserModel ConvertToUserModel(ClientIdentifier clientIdentifier)
        {
            if (clientIdentifier == null)
                return null;

            return new UserModel
                {
                    ClientId = clientIdentifier.ClientId.Trim(),
                    //Salt = clientIdentifier.Salt.Trim(),
                    Secret = clientIdentifier.Secret.Trim(),
                    DateTimeCreated = clientIdentifier.DateCreated,
                    DateTimeLogin = clientIdentifier.LastLogin,
                    UserName = clientIdentifier.UserName.Trim()
                };
        }

        #endregion


        public bool CalculateCurrentRisk(string userName, string userAgent, string ipAdress)
        {
            throw new NotImplementedException();
        }


        public int CreateLog(LoginLog loginLog)
        {
            throw new NotImplementedException();
        }


        public bool CheckForDoS(string clientId, string ipAdress)
        {
            throw new NotImplementedException();
        }
    }
}