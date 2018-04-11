using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
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
using Warehouse.Models.Custom;
using Warehouse.Models.DAL;
using Warehouse.Repositories;

namespace Warehouse.Controllers
{
    [RoutePrefix("api/Account")]
    public class AccountController : ApiController
    {
        private readonly WarehouseEntities _context;
        private readonly AuthRepository _authRepository;
        private readonly AccountRepository _accountRepository;
        // GET: Account
        public AccountController()
        {
            _context = new WarehouseEntities();
            _authRepository = new AuthRepository();
            _accountRepository = new AccountRepository();
        }
        [Authorize]
        [HttpPost]
        [Route("RegisterUser")]
        public RegistrationResult RegisterUser([FromBody]Registration registration)
        {
            string PasswordHash = SecurityHelper.EncodePassword(registration.Password, SecurityHelper.SALT);
            RegistrationResult registrationResult = new RegistrationResult();
            try
            {
                if (_accountRepository.IsLoginFree(registration))
                {
                    User userToAdd = new User { Login = registration.Login, Password = PasswordHash, Role = registration.Role };
                    _context.Users.Add(userToAdd);
                    _context.SaveChanges();
                    registrationResult.Status = true;
                    registrationResult.Message = "The user has been registered";
                }
                else
                {
                    registrationResult.Status = false;
                    registrationResult.Message = "Login exists in system.";
                }
            }
            catch (Exception ex)
            {
                registrationResult.Status = false;
                registrationResult.Message = ex.ToString();
            }

            return registrationResult;
        }

        [HttpPost]
        [Route("EditUser")]
        public void EditUser([FromBody]Registration registration)
        {
            RegistrationResult registrationResult = new RegistrationResult();
            try
            {
                User userToEdit = _context.Users.FirstOrDefault(u => u.Id == registration.Id);
                userToEdit.Login = registration.Login;
                userToEdit.Password = SecurityHelper.EncodePassword(registration.Password, SecurityHelper.SALT);
                userToEdit.Role = registration.Role;
                _context.Users.Add(userToEdit);
                _context.SaveChanges();
                registrationResult.Status = true;
                registrationResult.Message = "The user has been edited";
            }
            catch (Exception ex)
            {
                registrationResult.Status = false;
                registrationResult.Message = ex.ToString();
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
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    throw new HttpException("User not found");
                }
                User loggedUser = _context.Users.FirstOrDefault(u => u.Login == registration.Login && u.Password == PasswordHash);
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
                    loginResult.ExpirationTime = Convert.ToInt32(tokenResult.expires_in);
                    loginResult.Message = "Login successfully";
                }
            }
            catch (Exception ex)
            {
                loginResult.Status = false;
                loginResult.Message = ex.ToString();
            }
            return loginResult;


        }

    }
    
}