using System.Collections;
using System.Collections.Generic;

namespace TraderV1.Models
{
    public static class Users 
    {
       
        
        public static List<User> UsersList = new List<User>()
        {
            new User(){UserName = "admin", Password = "ghbdtncexfhs32@"},
            new User(){UserName = "user", Password = "guest"}
        };
    }
}