using System;
using System.Collections.Generic;
using System.Data;
using LockDataService.Model.Entity;
using System.Linq;

namespace LockDataService.Model.Repository
{
    public class Repository
    {
        
        public static SSO_UserEntities Entities = new SSO_UserEntities();

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

        public UserModel GetUserById(int id)
        {
            return ConvertToUserModel(Entities.ClientIdentifier.FirstOrDefault(x => x.Id == id));
        }

        public UserModel GetUserByUserName(string userName)
        {
            UserModel userModel = ConvertToUserModel(Entities.ClientIdentifier.FirstOrDefault(x => x.UserName == userName));

            return userModel;
        }

        public List<UserModel> GetAll()
        {
            return Entities.ClientIdentifier.ToList().Select(ConvertToUserModel).ToList();
        }

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

        public int DeleteUser(string userName)
        {
            ClientIdentifier cId = Entities.ClientIdentifier.FirstOrDefault(x => x.UserName.Equals(userName));
            Entities.ClientIdentifier.Remove(cId);
            return Entities.SaveChanges();
        }
        
#region converter

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

        private static UserModel ConvertToUserModel(ClientIdentifier clientIdentifier)
        {
            if (clientIdentifier == null)
                return null;

            return new UserModel
            {
                HashedClientId = clientIdentifier.HashedClientId,
                Salt = clientIdentifier.Salt,
                Secret = clientIdentifier.Secret,
                DateTimeCreated = clientIdentifier.DateCreated,
                DateTimeLogin = clientIdentifier.LastLogin,
                UserName = clientIdentifier.UserName
            };
        }

#endregion

    }
}