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


        List<UserModel> GetAll();


        int UpdateUser(UserModel userModel);


        int DeleteUser(string userName);


   }
}