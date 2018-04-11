using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace Warehouse.Helpers
{
    public class SecurityHelper
    {
        public static string SALT = "6B583248-302F-4DC3-9E87-87652DB4C10C";
        public static string EncodePassword(string pass, string salt)
        {
            byte[] bytes = Encoding.Unicode.GetBytes(pass);
            byte[] src = Encoding.Unicode.GetBytes(salt); //Corrected 5/15/2013
            byte[] dst = new byte[src.Length + bytes.Length];
            Buffer.BlockCopy(src, 0, dst, 0, src.Length);
            Buffer.BlockCopy(bytes, 0, dst, src.Length, bytes.Length);
            var sha1 = System.Security.Cryptography.SHA1.Create();
            byte[] inArray = sha1.ComputeHash(dst);
            return Convert.ToBase64String(inArray);
        }
    }
}