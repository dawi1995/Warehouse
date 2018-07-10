using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Warehouse.Helpers
{
    public static class StringHelper
    {
        public static string RemoveWhiteSpaces(this String text)
        {
            if (text==null)
            {
                return null;
            }
            else
            {
                return string.Join("", text.Split(default(string[]), StringSplitOptions.RemoveEmptyEntries));
            }

        }
    }
}