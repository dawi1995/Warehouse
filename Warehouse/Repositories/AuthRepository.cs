using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Warehouse.Helpers;
using Warehouse.Models.DAL;

namespace Warehouse.Repositories
{
    public class AuthRepository : IDisposable
    {
        private WarehouseEntities _context;

        private UserManager<IdentityUser> _userManager;

        public AuthRepository()
        {
            _context = new WarehouseEntities();
            _userManager = new UserManager<IdentityUser>(new UserStore<IdentityUser>(_context));
        }

        public User FindUser(string userName, string password)
        {
            var passwordHash = SecurityHelper.EncodePassword(password, SecurityHelper.SALT);
            var user = _context.Users.Where(u => u.Login == userName && u.Password == passwordHash);

            return user.FirstOrDefault();
        }

        public void Dispose()
        {
            _context.Dispose();
            _userManager.Dispose();

        }
    }
}