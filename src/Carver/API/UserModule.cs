using System;
using System.Security.Claims;
using Carver.Users;
using Nancy;
using Nancy.Security;
using log4net;

namespace Carver.API
{
    public class UserModule : NancyModule
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(UserModule));

        public UserModule() : base("/users")
        {
            this.RequiresHttps();
            this.RequiresClaims(c => c.Type == ClaimTypes.Role && c.Value == Enum.GetName(typeof(UserGroup), UserGroup.admin));

            #region Post -- Create new User
            Func<string, string, string, Response> checkInputValidity = (_userName, _password, _email) =>
            {
                if (_userName == null)
                    return new Response { StatusCode = HttpStatusCode.BadRequest, ReasonPhrase = "UserName field missing." };
                if (_password == null)
                    return new Response { StatusCode = HttpStatusCode.BadRequest, ReasonPhrase = "Password field missing." };
                if (_email == null)
                    return new Response { StatusCode = HttpStatusCode.BadRequest, ReasonPhrase = "Email field missing." };

                return null;
            };

            Post("/", async (ctx, ct) =>
            {

                dynamic userName = this.Request.Form.UserName;
                dynamic password = this.Request.Form.Password;
                dynamic email = this.Request.Form.Email;

                var verificationErrors = checkInputValidity(userName, password, email);
                if (verificationErrors != null) return verificationErrors;

                try
                {
                    await UserActions.CreateNewUser(userName, password, email, UserGroup.user);
                }
                catch (Exception ex)
                {
                    if (ex is UserActions.InvalidEmailException)
                        return new Response { StatusCode = HttpStatusCode.BadRequest, ReasonPhrase = "Email field provides an invalid address." };

                    return HttpStatusCode.InternalServerError;
                }

                return HttpStatusCode.OK;
            });
            #endregion

            // TODO: Put -- Update User Info
        }
    }
}
