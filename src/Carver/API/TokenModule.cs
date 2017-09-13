using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using log4net;
using Nancy;
using Nancy.Security;
using Nancy.ModelBinding;
using Carver.Data;
using Carver.Data.Models;
using Carver.Data.TokenStore;

namespace Carver.API
{
    public class TokenModule : NancyModule
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(TokenModule));

        private readonly ITokenStore<string> TokenStore = DataStoreFactory.AppTokenDataStore;

        public TokenModule() : base("/tokens")
        {
            this.RequiresHttps();
            this.RequiresAuthentication();

            var VerifyClaims = new Func<Claim, List<Permission>, bool>((claim, allowed) =>
            {
                return allowed.Select(permission => Enum.GetName(typeof(Permission), permission)).Contains(claim.Value);
            });

            Post("/", async (ctx, ct) =>
            {
                this.RequiresClaims(c => VerifyClaims(c, new List<Permission> { Permission.CreateToken } ));
                
                TokenData body = this.Bind<TokenData>();

                DateTimeOffset? expiration = null;
                if (body.expiration_epoch_milli.HasValue)
                    expiration = DateTimeOffset.FromUnixTimeMilliseconds(body.expiration_epoch_milli.Value);

                return await TokenStore.Create(body.description, expiration);
            });

            Post("/verify", async (ctx, ct) =>
            {
                this.RequiresClaims(c => VerifyClaims(c, new List<Permission> { Permission.VerifyToken }));

                string token = this.Bind<string>();

                if (await TokenStore.Exists(token))
                    return HttpStatusCode.OK;

                return HttpStatusCode.NotFound;
            });

            Post("/revoke", async (ctx, ct) =>
            {
                this.RequiresClaims(c => VerifyClaims(c, new List<Permission> { Permission.VerifyToken }));

                string token = this.Bind<string>();
                await TokenStore.Invalidate(token);

                return HttpStatusCode.OK;
            });
        }

        private class TokenData
        {
            public string description { get; set; }
            public long? expiration_epoch_milli { get; set; }
        }
    }
}
