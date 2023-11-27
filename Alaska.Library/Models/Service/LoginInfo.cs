using Alaska.Library.Core.Factories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alaska.Library.Models.Service
{
    public class LoginInfo : IEntity
    {
        public string ApiKey { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public int CompanyId { get; set; }
    }
}
