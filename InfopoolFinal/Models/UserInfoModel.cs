using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace InfopoolFinal.Models
{
    public class UserInfoModel
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public int BloggerOrNot { get; set; }
        public string CurrentPassword { get; set; }
    }
}