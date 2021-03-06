﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LockDataService.Model.Entity;

namespace LockDataService.Model.Repository
{
    /// <summary>
    /// Interface of Repository.
    /// </summary>
    interface IRepository
    {

        int CreateUser(UserModel userModel);


        UserModel GetUserById(int id);


        UserModel GetUserByUserName(string userName);

        UserModel GetUserByClientId(string clientId);

        List<UserModel> GetAll();


        int UpdateUser(UserModel userModel);

        bool CheckForDoS(string clientId, string ipAdress);

        int DeleteUser(string userName);

        LoginLog AddLogEntry(UserModel userModel);

        List<LoginLogModel> GetLogsFromUser(string userName);

        int CreateLog(LoginLog loginLog);

        int SetLoginSuccess(LoginLog model, bool success);

        double CalculateRisk(string userName, string userAgent, string ipAdress);

        bool CalculateCurrentRisk(string userName, string userAgent, string ipAdress);
    }
}
