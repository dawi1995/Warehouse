﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Web;
using System.Web.Http;
using Warehouse.Models.Custom;
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
                string name = UserHelper.GetCurrentUserName();
                User user = _context.Users.Where(u => u.Login == name).FirstOrDefault();
                return user.Id;
            }
        }
        public static int GetCurrentUserRole()
        {
            using (WarehouseEntities _context = new WarehouseEntities())
            {
                string name = UserHelper.GetCurrentUserName();
                User user = _context.Users.Where(u => u.Login == name).FirstOrDefault();
                return user.Role;
            }
        }
        public static bool IsAuthorize(List<int> accesUserRoles)
        {
            
            if (!accesUserRoles.Contains(UserHelper.GetCurrentUserRole()))
            {
                return false;
            }
            return true;
        }
        
        public static UserInformation GetCurrentUser()
        {
            using (WarehouseEntities _context = new WarehouseEntities())
            {
                UserInformation userInfo = new UserInformation();
                int currentUserId = UserHelper.GetCurrentUserId();
                var user = _context.Users.FirstOrDefault(u => u.Id == currentUserId && u.Deleted_at == null);
                if (user != null)
                {
                    userInfo.Id = user.Id;
                    userInfo.Login = user.Login;
                    userInfo.Role = user.Role;
                    userInfo.Created_At = user.Created_at == null ? string.Empty : ((DateTime)user.Created_at).ToString("dd-MM-yyyy");
                    userInfo.Edited_At = user.Edited_at == null ? string.Empty : ((DateTime)user.Edited_at).ToString("dd-MM-yyyy");
                    Client client = _context.Clients.FirstOrDefault(c => c.User_Id == user.Id);
                    if (client != null)
                    {
                        userInfo.Name = client.Name;
                        userInfo.Address = client.Address;
                        userInfo.VAT_Id = client.VAT_Id;
                        userInfo.Email = client.Email;
                    }
                }
                return userInfo;
            }
           
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