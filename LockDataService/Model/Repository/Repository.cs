using System;
using System.Collections.Generic;
using System.Data;
using LockDataService.Model.Entity;
using System.Linq;

namespace LockDataService.Model.Repository 
{
    public class Repository : IRepository
    {
        
        public static SSO_UserEntities Entities = new SSO_UserEntities();

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
            UserModel userModel = ConvertToUserModel(Entities.ClientIdentifier.FirstOrDefault(x => x.UserName == userName));

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
            ClientIdentifier clientIdentifier = Entities.ClientIdentifier.FirstOrDefault(x => x.UserName == userModel.UserName);

            if (clientIdentifier != null)
            {
                clientIdentifier.LastLogin = DateTime.Now;
                clientIdentifier.HashedClientId = userModel.HashedClientId;
                clientIdentifier.Salt = userModel.Salt;
                clientIdentifier.Secret = userModel.Secret;
            }


            return Entities.SaveChanges();
        }

        /// <summary>
        /// Deletes a user.
        /// </summary>
        /// <param name="userName">UserName of the Row.</param>
        /// <returns>Number of affected rows.</returns>
        public int DeleteUser(string userName)
        {
            ClientIdentifier cId = Entities.ClientIdentifier.FirstOrDefault(x => x.UserName.Equals(userName));
            Entities.ClientIdentifier.Remove(cId);
            return Entities.SaveChanges();
        }
        
        #region converter

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
                    HashedClientId = userModel.HashedClientId,
                    Salt = userModel.Salt,
                    Secret = userModel.Secret,
                    DateCreated = userModel.DateTimeCreated,
                    LastLogin = userModel.DateTimeLogin,
                    UserName = userModel.UserName
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
                HashedClientId = clientIdentifier.HashedClientId.Trim(),
                Salt = clientIdentifier.Salt.Trim(),
                Secret = clientIdentifier.Secret.Trim(),
                DateTimeCreated = clientIdentifier.DateCreated,
                DateTimeLogin = clientIdentifier.LastLogin,
                UserName = clientIdentifier.UserName.Trim()
            };
        }

        #endregion

    }
}