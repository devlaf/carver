using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Carver.Data;
using Carver.Data.Models;
using Carver.Data.UserStore;
using Nancy;
using Nancy.Security;
using Nancy.ModelBinding;
using log4net;

namespace Carver.API
{
    public class UserModule : NancyModule
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(UserModule));

        private readonly IUserStore UserStore = DataStoreFactory.UserDataStore;

        public UserModule() : base("/users")
        {
            this.RequiresHttps();
            this.RequiresAuthentication();

            Func<Claim, List<Permission>, bool> VerifyClaims = new Func<Claim, List<Permission>, bool>((_claim, _allowed) =>
            {
                return _allowed.Select(permission => Enum.GetName(typeof(Permission), permission)).Contains(_claim.Value);
            });

            Func<string, string, string, Response> ValidateInput = (_userName, _password, _email) =>
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
                this.RequiresClaims(c => VerifyClaims(c, new List<Permission> { Permission.ManageAnyUser }));

                dynamic username = this.Request.Form.username;
                dynamic password = this.Request.Form.password;
                dynamic email = this.Request.Form.email;

                var verificationErrors = ValidateInput(username, password, email);
                if (verificationErrors != null) return verificationErrors;

                try
                {
                    return await UserStore.CreateUser(username, password, email);
                }
                catch (Exception ex)
                {
                    if (ex is UserStore.InvalidEmailException)
                        return new Response { StatusCode = HttpStatusCode.BadRequest, ReasonPhrase = "Invalid email address." };

                    throw;
                }
            });

            Put("/{user_id}", async (ctx, ct) =>
            {
                this.RequiresClaims(c => VerifyClaims(c, new List<Permission> { Permission.ManageAnyUser, Permission.ManageSelfUser }));
                
                dynamic usernameMaybe = this.Request.Form.username;
                dynamic oldPasswordMaybe = this.Request.Form.old_password;
                dynamic newPasswordMaybe = this.Request.Form.new_password;
                dynamic emailMaybe = this.Request.Form.email;

                try
                {
                    if (newPasswordMaybe != null)
                        await UserStore.UpdateUserPassword(usernameMaybe ?? string.Empty, oldPasswordMaybe ?? string.Empty, newPasswordMaybe);
                    if (emailMaybe != null)
                        await UserStore.UpdateEmail(ctx.user_id, emailMaybe);

                    User updated = await UserStore.GetUser(ctx.user_id);
                    return new { id = updated.Id, username = updated.Username, email = updated.Email };
                }
                catch (Exception ex)
                {
                    if (ex is ArgumentException)
                        return new Response { StatusCode = HttpStatusCode.Unauthorized, ReasonPhrase = "Invalid username/old_password." };
                    if (ex is UserStore.InvalidEmailException)
                        return new Response { StatusCode = HttpStatusCode.BadRequest, ReasonPhrase = "Invalid email address." };

                    throw;
                }
            });
            
            Get("/{user_id}/permissions", async (ctx, ct) =>
            {
                this.RequiresClaims(c => VerifyClaims(c, new List<Permission> { Permission.ManageAnyUser }));

                List<Permission> permissions = await UserStore.GetPermissionsForUser(ctx.user_id);
                return permissions.Select(permission => Enum.GetName(typeof(Permission), permission)).ToList();
            });

            Put("/{user_id}/permissions", async (ctx, ct) =>
            {
                this.RequiresClaims(c => VerifyClaims(c, new List<Permission> { Permission.ManageAnyUser }));

                dynamic parsedBody = this.Bind<PermissionsRequest>().ToEnumList();

                if (parsedBody is Response)
                    return parsedBody;

                await UserStore.EnsurePermissionsForUser(ctx.user_id, parsedBody);
                
                return HttpStatusCode.OK;
            });

            Delete("/{user_id}/permissions", async (ctx, ct) =>
            {
                this.RequiresClaims(c => VerifyClaims(c, new List<Permission> { Permission.ManageAnyUser }));

                dynamic parsedBody = this.Bind<PermissionsRequest>().ToEnumList();

                if (parsedBody is Response)
                    return parsedBody;

                await UserStore.RevokePermissionsForUser(ctx.user_id, parsedBody);
                
                return HttpStatusCode.OK;
            });
        }

        private class PermissionsRequest
        {
            public List<string> PermissionNames { get; set; }
            
            public dynamic ToEnumList()
            {
                var parsed = PermissionNames.Select(permissionName => Parse(permissionName));
                var invalid = parsed.Where(perms => perms.Item2 == false).Select(perms => perms.Item1);
                if (invalid.Count() > 0)
                {
                    return new Response 
                    { 
                        StatusCode = HttpStatusCode.BadRequest, 
                        ReasonPhrase = $"The following permissions are not valid: [{ string.Join(',', invalid.ToList()) }]"
                    };
                }
                return parsed.Select(perms => perms.Item3).ToHashSet();
            }

            private Tuple<string, bool, Permission> Parse(string permission)
            {
                bool found = Enum.TryParse(typeof(Permission), permission, false, out var result);
                return new Tuple<string, bool, Permission>(permission, found, (Permission)result);
            }
        }
    }
}
