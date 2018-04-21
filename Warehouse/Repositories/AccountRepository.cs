using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Warehouse.Helpers;
using Warehouse.Models.Custom;
using Warehouse.Models.DAL;

namespace Warehouse.Repositories
{
    public class AccountRepository
    {
        private WarehouseEntities _warehouseEntities;

        public AccountRepository()
        {
            _warehouseEntities = new WarehouseEntities();
        }

        public bool IsLoginFree(string login)
        {
            User userToCheck = _warehouseEntities.Users.FirstOrDefault(u => u.Login == login && u.Deleted_at == null);
            if (userToCheck != null)
            {
                return false;
            }
            else
            {
                return true;
            }
            
        }
        //public void RegisterUser(Registration registration)
        //{
        //        User userToAdd = new User { Login = registration.Login, Password = SecurityHelper.EncodePassword(registration.Password, SecurityHelper.SALT), Role = registration.Role };
        //        _warehouseEntities.Users.Add(userToAdd);
        //        _warehouseEntities.SaveChanges();          
        //}
        //public void EditUser(Registration registration)
        //{
        //    User userToEdit = _warehouseEntities.Users.FirstOrDefault(u => u.Id == registration.Id);
        //    userToEdit.Login = registration.Login;
        //    userToEdit.Password = SecurityHelper.EncodePassword(registration.Password, SecurityHelper.SALT);
        //    userToEdit.Role = registration.Role;
        //    _warehouseEntities.Users.Add(userToEdit);
        //    _warehouseEntities.SaveChanges();
        //}

        public void ChangePassword(UserRegistration registration)
        {
            //User userWithChangePassword = _warehouseEntities.Users.FirstOrDefault(u => u.Id == registration.Id && u.Password == SecurityHelper.EncodePassword(registration.Password, SecurityHelper.SALT));
            //userWithChangePassword.Password 
        }
    }
}