using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Warehouse.Models.DAL;

namespace Warehouse.Managers
{
    public class AccountManager
    {
        private readonly WarehouseEntities _context;
        public AccountManager(WarehouseEntities context)
        {
            _context = context;
        }

        public int CountOfUsers(int role = 0, string needle = "")
        {
            if (role == 0)
            {
                return (from users in _context.Users
                        join clients in _context.Clients on users.Id equals clients.User_Id into q
                        from clients in q.DefaultIfEmpty()
                        where (users.Deleted_at == null && (users.Login.Contains(needle) || clients.Name.Contains(needle) || clients.Email.Contains(needle)))
                        select new { User = users, Client = clients }).Count();

            }
            else
            {
                return (from users in _context.Users
                               join clients in _context.Clients on users.Id equals clients.User_Id into q
                               from clients in q.DefaultIfEmpty()
                               where (users.Deleted_at == null && users.Role == role && (users.Login.Contains(needle) || clients.Name.Contains(needle) || clients.Email.Contains(needle)))
                               select new { User = users, Client = clients }).Count();
            }

        }
    }
}