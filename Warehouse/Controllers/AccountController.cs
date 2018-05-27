using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Results;
using Warehouse.Helpers;
using Warehouse.Managers;
using Warehouse.Models.Custom;
using Warehouse.Models.DAL;
using Warehouse.Repositories;
using static Warehouse.Enums;

namespace Warehouse.Controllers
{
    [RoutePrefix("api/Account")]
    public class AccountController : ApiController
    {
        private readonly WarehouseEntities _context;
        private readonly AccountRepository _accountRepository;
        // GET: Account
        public AccountController()
        {
            _context = new WarehouseEntities();
            _accountRepository = new AccountRepository();
        }

        [Authorize]
        [HttpPost]
        [Route("RegisterUser")]
        public RequestResult RegisterUser([FromBody]UserRegistration registration)
        {
            if (UserHelper.IsAuthorize(new List<int> { (int)UserType.SuperAdmin }))
            {
                string PasswordHash = SecurityHelper.EncodePassword(registration.Password, SecurityHelper.SALT);
                RequestResult requestResult = new RequestResult();
                try
                {
                    if (_accountRepository.IsLoginFree(registration.Login))
                    {
                        User userToAdd = new User { Login = registration.Login, Password = PasswordHash, Role = registration.Role, Created_at = DateTime.Now, Name=registration.UserName, Surname=registration.UserSurname };
                        _context.Users.Add(userToAdd);
                        _context.SaveChanges();
                        requestResult.Status = true;
                        requestResult.Message = "The user has been registered";
                    }
                    else
                    {
                        requestResult.Status = false;
                        requestResult.Message = "Login exists in system.";
                    }
                }
                catch (Exception ex)
                {
                    requestResult.Status = false;
                    requestResult.Message = ex.ToString();
                }

                return requestResult;
            }
            else
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "User don't have acces to this method"));
            }
          
        }

        [Authorize]
        [HttpPost]
        [Route("RegisterClient")]
        public RequestResult RegisterClient([FromBody]ClientRegistration registration)
        {
            if (UserHelper.IsAuthorize(new List<int> { (int)UserType.SuperAdmin }))
            {
                string PasswordHash = SecurityHelper.EncodePassword(registration.Password, SecurityHelper.SALT);
                RequestResult requestResult = new RequestResult();
                try
                {
                    if (_accountRepository.IsLoginFree(registration.Login))
                    {
                        User userToAdd = new User { Login = registration.Login, Password = PasswordHash, Role = registration.Role, Name=registration.UserName, Surname=registration.UserSurname, Created_at = DateTime.Now };
                        _context.Users.Add(userToAdd);
                        _context.SaveChanges();
                        Client clientToAdd = new Client { Name = registration.Name, Address = registration.Address, VAT_Id = registration.VAT_Id, Email = registration.Email, User_Id = userToAdd.Id, Created_At = userToAdd.Created_at, PhoneNumber=registration.PhoneNumber };
                        _context.Clients.Add(clientToAdd);
                        _context.SaveChanges();
                        requestResult.Status = true;
                        requestResult.Message = "The client has been registered";
                    }
                    else
                    {
                        requestResult.Status = false;
                        requestResult.Message = "Login exists in system.";
                    }
                }
                catch (Exception ex)
                {
                    requestResult.Status = false;
                    requestResult.Message = ex.ToString();
                }
                return requestResult;
            }
            else
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "User don't have acces to this method"));
            }
            
        }

        [Authorize]
        [HttpPost]
        [Route("EditUser")]
        public RequestResult EditUser([FromBody]UserEdit registration)
        {
            if (UserHelper.IsAuthorize(new List<int> { (int)UserType.SuperAdmin }))
            {
                RequestResult requestResult = new RequestResult();
                try
                {
                    User userToEdit = _context.Users.FirstOrDefault(u => u.Id == registration.Id && u.Deleted_at == null);
                    if (userToEdit.Login != registration.Login)
                    {
                        if (_accountRepository.IsLoginFree(registration.Login))
                        {
                            userToEdit.Login = registration.Login;
                        }
                        else
                        {
                            requestResult.Status = false;
                            requestResult.Message = "Login exists in system.";
                            return requestResult;
                        }
                    }
                    if (!string.IsNullOrEmpty(registration.Password))
                    {
                        userToEdit.Password = SecurityHelper.EncodePassword(registration.Password, SecurityHelper.SALT);
                    }
                    userToEdit.Role = registration.Role;
                    userToEdit.Name = registration.UserName;
                    userToEdit.Surname = registration.UserSurname;
                    userToEdit.Edited_at = DateTime.Now;
                    _context.SaveChanges();
                    requestResult.Status = true;
                    requestResult.Message = "The user has been edited";


                }
                catch (Exception ex)
                {
                    requestResult.Status = false;
                    requestResult.Message = ex.ToString();
                }
                return requestResult;
            }
            else
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "User don't have acces to this method"));
            }
           

        }

        [Authorize]
        [HttpPost]
        [Route("EditClient")]
        public RequestResult EditClient([FromBody]ClientEdit registration)
        {
            if (UserHelper.IsAuthorize(new List<int> { (int)UserType.SuperAdmin }))
            {
                RequestResult requestResult = new RequestResult();
                try
                {
                    User userToEdit = _context.Users.FirstOrDefault(u => u.Id == registration.Id && u.Deleted_at == null);
                    if (userToEdit.Login != registration.Login)
                    {
                        if (_accountRepository.IsLoginFree(registration.Login))
                        {
                            userToEdit.Login = registration.Login;
                        }
                        else
                        {
                            requestResult.Status = false;
                            requestResult.Message = "Login exists in system.";
                            return requestResult;
                        }
                        
                    }
                    if (!string.IsNullOrEmpty(registration.Password))
                    {
                        userToEdit.Password = SecurityHelper.EncodePassword(registration.Password, SecurityHelper.SALT);
                    }
                    userToEdit.Role = registration.Role;
                    userToEdit.Name = registration.UserName;
                    userToEdit.Surname = registration.UserSurname;
                    userToEdit.Edited_at = DateTime.Now;
                    Client clientToEdit = _context.Clients.FirstOrDefault(c => c.User_Id == registration.Id);
                    clientToEdit.Name = registration.Name;
                    clientToEdit.Address = registration.Address;
                    clientToEdit.VAT_Id = registration.VAT_Id;
                    clientToEdit.Email = registration.Email;
                    clientToEdit.PhoneNumber = registration.PhoneNumber;
                    clientToEdit.Edited_At = userToEdit.Edited_at;
                    _context.SaveChanges();
                    requestResult.Status = true;
                    requestResult.Message = "The client has been edited";
                }
                catch (Exception ex)
                {
                    requestResult.Status = false;
                    requestResult.Message = ex.ToString();
                }
                return requestResult;
            }
            else
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "User don't have acces to this method"));
            }
        }

        [HttpPost]
        [Route("LoginUser")]
        public async Task<LoginResult> LoginUser([FromBody]LoginRequest registration)
        {
            HttpRequestMessage request = new HttpRequestMessage();
            string PasswordHash = SecurityHelper.EncodePassword(registration.Password, SecurityHelper.SALT);
            TokenResult tokenResult = new TokenResult();
            LoginResult loginResult = new LoginResult();
            HttpResponseMessage response;
            try
            {

                List<KeyValuePair<string, string>> postData = new List<KeyValuePair<string, string>>(){
                new KeyValuePair<string, string>("grant_type", "password"),
                new KeyValuePair<string, string>("username", registration.Login),
                new KeyValuePair<string, string>("password", registration.Password),
                };

                using (var httpClient = new HttpClient())
                {
                    using (var content = new FormUrlEncodedContent(postData))
                    {
                        httpClient.BaseAddress = new Uri("http://" + HttpContext.Current.Request.Url.Authority + "/token");
                        content.Headers.Clear();
                        content.Headers.Add("Content-Type", "application/x-www-form-urlencoded");

                        response = await httpClient.PostAsync("token", content);

                        tokenResult = JsonConvert.DeserializeObject<TokenResult>(await response.Content.ReadAsStringAsync());
                    }
                }
                User loggedUser = _context.Users.FirstOrDefault(u => u.Login == registration.Login && u.Password == PasswordHash && u.Deleted_at == null);
                if (loggedUser == null)
                {
                    loginResult.Status = false;
                    loginResult.Message = "User not found";
                }
                else
                {
                    loginResult.Token = tokenResult.access_token;
                    loginResult.TokenType = tokenResult.token_type;
                    loginResult.Status = true;
                    loginResult.Role = loggedUser.Role;
                    loginResult.UserId = loggedUser.Id;
                    loginResult.Message = "Login successfully";
                    loginResult.ExpirationTime = Convert.ToInt32(tokenResult.expires_in);
                }
            }
            catch (Exception ex)
            {
                loginResult.Status = false;
                loginResult.Message = ex.ToString();
            }
            return loginResult; 
        }

        [Authorize]
        [HttpGet]
        [Route("RemoveUser")]
        public RequestResult RemoveUser(int userId)
        {
            if (UserHelper.IsAuthorize(new List<int> { (int)UserType.SuperAdmin }))
            {
                RequestResult requestResult = new RequestResult();
                try
                {
                    User userToDelete = _context.Users.FirstOrDefault(u => u.Id == userId && u.Deleted_at == null);
                    if (userToDelete != null)
                    {
                        userToDelete.Deleted_at = DateTime.Now;
                        Client clientToDelete = _context.Clients.FirstOrDefault(c => c.User_Id == userToDelete.Id && c.Deleted_At == null);
                        if (clientToDelete != null)
                        {
                            clientToDelete.Deleted_At = DateTime.Now;
                        }
                        _context.SaveChanges();
                        requestResult.Status = true;
                        requestResult.Message = "The user has been deleted";

                    }
                    else
                    {
                        requestResult.Status = false;
                        requestResult.Message = "The user with the given ID does not exist";
                    }
                }
                catch (Exception ex)
                {
                    requestResult.Status = false;
                    requestResult.Message = ex.Message;
                }
                return requestResult;
            }
            else
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "User don't have acces to this method"));
            }
          
        }

        //[Authorize]
        //[HttpGet]
        //[Route("GetAllUsers")]
        //public List<UserInformation> GetAllUsers(int offset, int limit, int role = 0)
        //{
        //    if (UserHelper.IsAuthorize(new List<int> { (int)UserType.SuperAdmin }))
        //    {
        //        List<UserInformation> result = new List<UserInformation>();
        //        var listOfUsers = _context.Users.Where(u => u.Deleted_at == null).OrderByDescending(u => u.Login).Skip(offset).Take(limit).ToList();
        //        foreach (var user in listOfUsers)
        //        {
        //            UserInformation userInfo = new UserInformation();
        //            userInfo.Id = user.Id;
        //            userInfo.Login = user.Login;
        //            userInfo.Role = user.Role;
        //            userInfo.Created_At = user.Created_at;
        //            userInfo.Edited_At = user.Edited_at;
        //            Client client = _context.Clients.FirstOrDefault(c => c.User_Id == user.Id);
        //            if (client != null)
        //            {
        //                userInfo.Name = client.Name;
        //                userInfo.Address = client.Address;
        //                userInfo.VAT_Id = client.VAT_Id;
        //                userInfo.Email = client.Email;
        //            }
        //            result.Add(userInfo);
        //        }
        //        return result;
        //    }
        //    else
        //    {
        //        throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "User don't have acces to this method"));
        //    }
        
        //}

        [Authorize]
        [HttpGet]
        [Route("GetAllUsersByRole")]
        public UsersInformationList GetAllUsersByRole(int offset = 0, int limit = int.MaxValue, int role = 0, string needle = "")
        {
            if (UserHelper.IsAuthorize(new List<int> { (int)UserType.SuperAdmin }))
            {
                dynamic listOfUsers = null;
                List<UserInformation> listOfUsersInfo = new List<UserInformation>();
                UsersInformationList result = new UsersInformationList();

                if (role == 0)
                {
                    listOfUsers = (from users in _context.Users
                                  join clients in _context.Clients on users.Id equals clients.User_Id into q
                                      from clients in q.DefaultIfEmpty()
                                      where (users.Deleted_at == null && (users.Login.Contains(needle) || clients.Name.Contains(needle) || clients.Email.Contains(needle)))
                                      select new { User = users, Client = clients }).OrderByDescending(u => u.User.Created_at).Skip(offset).Take(limit);

                }
                else
                {
                    listOfUsers = (from users in _context.Users
                                      join clients in _context.Clients on users.Id equals clients.User_Id into q
                                      from clients in q.DefaultIfEmpty()
                                      where (users.Deleted_at == null && users.Role == role  && (users.Login.Contains(needle) || clients.Name.Contains(needle) || clients.Email.Contains(needle)))
                                      select new { User = users, Client = clients }).OrderByDescending(u => u.User.Created_at).Skip(offset).Take(limit);                  
                }

                foreach (var user in listOfUsers)
                {

                    UserInformation userInfo = new UserInformation();
                    userInfo.Id = user.User.Id;
                    userInfo.Login = user.User.Login;
                    userInfo.Role = user.User.Role;
                    userInfo.Created_At = user.User.Created_at == null ? string.Empty : ((DateTime)user.User.Created_at).ToString("dd-MM-yyyy");
                    userInfo.Edited_At = user.User.Edited_at == null ? string.Empty : ((DateTime)user.User.Edited_at).ToString("dd-MM-yyyy");
                    userInfo.Name = user.Client == null ? string.Empty : user.Client.Name;
                    userInfo.Address = user.Client == null ? string.Empty : user.Client.Address;
                    userInfo.VAT_Id = user.Client == null ? string.Empty : user.Client.VAT_Id;
                    userInfo.Email = user.Client == null ? string.Empty : user.Client.Email;
                    userInfo.PhoneNumber = user.Client == null ? string.Empty : user.Client.PhoneNumber;
                    userInfo.UserName = user.User.Name == null ? string.Empty : user.User.Name;
                    userInfo.UserSurname = user.User.Surname == null ? string.Empty : user.User.Surname;
                    listOfUsersInfo.Add(userInfo);

                }
                result.NumberOfUsers = AccountManager.CountOfUsers(role, needle);
                result.ListOfUsers = listOfUsersInfo;
                return result;
            }
            else
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "User don't have acces to this method"));
            }
        }

        [Authorize]
        [HttpGet]
        [Route("GetNumberUsersByRole")]
        public NumberUsers GetNumberUsersByRole(int role = 0)
        {
            if (UserHelper.IsAuthorize(new List<int> { (int)UserType.SuperAdmin, (int)UserType.Admin }))
            {
                NumberUsers number = new NumberUsers();
                if (role == 0)
                {
                    number.Number = _context.Users.Where(u => u.Deleted_at == null).Count();
                }
                else
                {
                    number.Number = _context.Users.Where(u => u.Deleted_at == null && u.Role == role).Count();
                }
                return number;
            }
            else
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "User don't have acces to this method"));
            }
        }

        [Authorize]
        [HttpGet]
        [Route("GetUser")]
        public UserInformation GetUser(int userId = 0)
        {
            if (userId == 0)
            {
                if (UserHelper.IsAuthorize(new List<int> { (int)UserType.SuperAdmin, (int)UserType.Admin, (int)UserType.Client }))
                {
                    return UserHelper.GetCurrentUser();
                }
                else
                {
                    throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "User don't have acces to this method"));
                }
            }
            else
            {
                if (UserHelper.IsAuthorize(new List<int> { (int)UserType.SuperAdmin, (int)UserType.Admin }))
                {
                    UserInformation userInfo = new UserInformation();
                    var user = _context.Users.FirstOrDefault(u => u.Id == userId && u.Deleted_at == null);
                    if (user != null)
                    {
                        userInfo.Id = user.Id;
                        userInfo.Login = user.Login;
                        userInfo.Role = user.Role;
                        userInfo.UserName = user.Name;
                        userInfo.UserSurname = user.Surname;
                        userInfo.Created_At = user.Created_at == null ? string.Empty : ((DateTime)user.Created_at).ToString("dd-MM-yyyy");
                        userInfo.Edited_At = user.Edited_at == null ? string.Empty : ((DateTime)user.Edited_at).ToString("dd-MM-yyyy");
                        Client client = _context.Clients.FirstOrDefault(c => c.User_Id == user.Id);
                        if (client != null)
                        {
                            userInfo.Name = client.Name;
                            userInfo.Address = client.Address;
                            userInfo.VAT_Id = client.VAT_Id;
                            userInfo.Email = client.Email;
                            userInfo.PhoneNumber = client.PhoneNumber;
                        }
                    }
                    return userInfo;
                }
                else
                {
                    throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "User don't have acces to this method"));
                }
            }
           
        }

        [Authorize]
        [HttpPost]
        [Route("ChangePassword")]
        public RequestResult ChangePassword([FromBody] ChangePassword changePassword)
        {
            RequestResult result = new RequestResult();
            var user = _context.Users.FirstOrDefault(u => u.Id == changePassword.UserId && u.Deleted_at == null);
            if (user != null)
            {
                string oldPasswordHash = SecurityHelper.EncodePassword(changePassword.OldPassword, SecurityHelper.SALT);
                if (user.Password == oldPasswordHash)
                {
                    string newPasswordHash = SecurityHelper.EncodePassword(changePassword.NewPassword, SecurityHelper.SALT);
                    user.Password = newPasswordHash;
                    _context.SaveChanges();
                    result.Status = false;
                    result.Message = "Password has been changed";
                }
                else
                {
                    result.Status = false;
                    result.Message = "Old password is incorrect";
                }
            }
            else
            {
                result.Status = false;
                result.Message = "User not found";
            }
            return result;
        }



    }
    
}