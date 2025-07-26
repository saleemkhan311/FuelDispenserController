using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FuelDispenserController
{
    public class User
    {
        public int Id
        {
            get; set;
        }
        public string Username
        {
            get; set;
        }
        public string Password
        {
            get; set;
        }
        public string RegistrationDate
        {
            get; set;
        }  // Store as string or DateTime

        public string UserType
        {
            get; set;   
        }
    }
}
