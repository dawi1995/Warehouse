using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Warehouse.Models.Custom
{
    public class LoginResult
    {
        public string Token { get; set; }
        public string TokenType { get; set; }
        public int Role { get; set; }
        public bool Status { get; set; }
        public string Message { get; set; }
        public int ExpirationTime { get; set; }
    }
}