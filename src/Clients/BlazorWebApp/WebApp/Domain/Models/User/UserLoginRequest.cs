﻿namespace WebApp.Domain.Models.User
{
    public class UserLoginRequest
    {
        public string UserName { get;  set; }
        public string Password { get; set; }

        public UserLoginRequest(string userName, string password)
        {
            UserName = userName;
            Password = password;
        }
    }

}
