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
    }
}