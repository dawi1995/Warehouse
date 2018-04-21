using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Web;
using System.Web.Http;
using Warehouse.Models.DAL;

namespace Warehouse.Helpers
{
    public class UserHelper
    {
        public static string GetCurrentUserName()
        {
            var identity = (ClaimsIdentity)HttpContext.Current.User.Identity;
            IEnumerable<Claim> claims = identity.Claims;
            var name = claims.FirstOrDefault();
            return name.Value;
        }
        public static int GetCurrentUserId()
        {
            using (WarehouseEntities _context = new WarehouseEntities())
            {
                string name = GetCurrentUserName();
                User user = _context.Users.Where(u => u.Login == name).FirstOrDefault();
                return user.Id;
            }
        }
        public static int GetCurrentUserRole()
        {
            using (WarehouseEntities _context = new WarehouseEntities())
            {
                string name = GetCurrentUserName();
                User user = _context.Users.Where(u => u.Login == name).FirstOrDefault();
                return user.Role;
            }
        }
        public static bool IsAuthorize(List<int> accesUserRoles)
        {
            
            if (!accesUserRoles.Contains(GetCurrentUserRole()))
            {
                return false;
            }
            return true;
        }
        
        //public static Registration GetCurrentUser()
        //{
        //    using (WakeParkEntities _context = new WakeParkEntities())
        //    {
        //        Registration currentUser = new Registration();
        //        string name = GetUserName();
        //        AspNetUser user = _context.AspNetUsers.Where(u => u.UserName == name).FirstOrDefault();
        //        var aspNetUserProfile = _context.AspNetUsersProfiles.FirstOrDefault(q => q.UserId == user.Id);
        //        var userProfile = _context.UserProfiles.FirstOrDefault(q => q.UserId == user.Id);
        //        if (aspNetUserProfile != null)
        //        {
        //            currentUser = new Registration
        //            {
        //                Id = user.Id,
        //                Name = aspNetUserProfile.Name == null ? string.Empty : aspNetUserProfile.Name,
        //                Surname = aspNetUserProfile.Surname == null ? string.Empty : aspNetUserProfile.Surname,
        //                BirthDate = aspNetUserProfile.BirthDate,
        //                AddressId = aspNetUserProfile.AddressId == null ? 0 : aspNetUserProfile.AddressId,
        //                Email = aspNetUserProfile.Email == null ? string.Empty : aspNetUserProfile.Email,
        //                PhoneNo = aspNetUserProfile.PhoneNo == null ? string.Empty : aspNetUserProfile.PhoneNo,
        //                EmergencyPhoneNo = aspNetUserProfile.EmergencyPhoneNo == null ? string.Empty : aspNetUserProfile.EmergencyPhoneNo,
        //                CompanyId = aspNetUserProfile.CompanyId == null ? 0 : aspNetUserProfile.CompanyId,
        //                Photo = aspNetUserProfile.Photo == null ? null : aspNetUserProfile.Photo,
        //                UserCity = aspNetUserProfile.Address.City == null ? string.Empty : aspNetUserProfile.Address.City,
        //                UserAddress = aspNetUserProfile.Address.Address1 == null ? string.Empty : aspNetUserProfile.Address.Address1,
        //                UserZipCode = aspNetUserProfile.Address.ZipCode == null ? string.Empty : aspNetUserProfile.Address.ZipCode,
        //                CompanyName = aspNetUserProfile.Company == null ? string.Empty : (aspNetUserProfile.Company.Name == null ? string.Empty : aspNetUserProfile.Company.Name),
        //                CompanyNIP = aspNetUserProfile.Company == null ? string.Empty : (aspNetUserProfile.Company.NIP == null ? string.Empty : aspNetUserProfile.Company.NIP),
        //                CompanyCity = aspNetUserProfile.Company == null ? string.Empty : (aspNetUserProfile.Company.Address.City == null ? string.Empty : aspNetUserProfile.Company.Address.City),
        //                CompanyAddress = aspNetUserProfile.Company == null ? string.Empty : (aspNetUserProfile.Company.Address.Address1 == null ? string.Empty : aspNetUserProfile.Company.Address.Address1),
        //                CompanyZipCode = aspNetUserProfile.Company == null ? string.Empty : (aspNetUserProfile.Company.Address.ZipCode == null ? string.Empty : aspNetUserProfile.Company.Address.ZipCode),
        //                Discount = aspNetUserProfile.Discount,
        //                Status = aspNetUserProfile.Status,
        //                ServiceGoodId = userProfile == null ? null : userProfile.ServiceGoodId,
        //                UserRole = aspNetUserProfile.UserRole ?? 3,
        //                ServiceDuration = userProfile == null ? null : userProfile.ServiceDuration,
        //                ServiceStartDT = userProfile == null ? null : userProfile.ServiceStartDT,
        //                TagRegistrationDT = userProfile == null ? null : userProfile.TagRegistrationDT,

        //            };
        //        }
        //        return currentUser;
        //    }

        //}
    }
}