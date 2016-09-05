using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CompetitionPlatform.Models
{
    public class AuthenticateModel
    {
        public string FullName { get; set; }
        public string Password { get; set; }
    }

    public class CompetitionPlatformUser
    {
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Documents { get; set; }

        public string GetFullName()
        {
            return FirstName + " " + LastName;
        }
    }
}
