using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Claims;
using IdentityManager2.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection;

namespace IdentityManager2.Configuration.Hosting
{
    // Adapted from:
    // Microsoft.Owin.Security.DataHandler.Serializer.TicketSerializer
    // Microsoft.AspNetCore.Identity.DataProtectorTokenProvider
    internal class AuthenticationTicketTokenProvider : ITokenProvider<AuthenticationTicket>
    {
        private readonly IDataProtector protector;
        private readonly IdentityManagerOptions options;

        public AuthenticationTicketTokenProvider(IDataProtectionProvider dataProtectionProvider, IdentityManagerOptions options)
        {
            protector = dataProtectionProvider.CreateProtector("authTicket");
            this.options = options;

        }

        public string Generate(AuthenticationTicket model)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));

            var identity = model.Principal.Identities.First();
            if (identity == null) throw new InvalidOperationException("Invalid identity type");

            using (var ms = new MemoryStream())
            {
                using (var gzip = new GZipStream(ms, CompressionLevel.Optimal))
                {
                    using (var writer = new BinaryWriter(gzip))
                    {
                        writer.Write(model.AuthenticationScheme);
                        writer.Write(DateTimeOffset.UtcNow.UtcTicks);
                        WriteWithDefault(writer, identity.NameClaimType, DefaultValues.NameClaimType);
                        WriteWithDefault(writer, identity.RoleClaimType, DefaultValues.RoleClaimType);
                        writer.Write(identity.Claims.Count());
                        foreach (var claim in identity.Claims)
                        {
                            WriteWithDefault(writer, claim.Type, identity.NameClaimType);
                            writer.Write(claim.Value);
                            WriteWithDefault(writer, claim.ValueType, DefaultValues.StringValueType);
                            WriteWithDefault(writer, claim.Issuer, DefaultValues.LocalAuthority);
                            WriteWithDefault(writer, claim.OriginalIssuer, claim.Issuer);
                        }

                        var bc = identity.BootstrapContext as string;
                        if (bc == null || string.IsNullOrWhiteSpace(bc))
                        {
                            writer.Write(0);
                        }
                        else
                        {
                            writer.Write(bc.Length);
                            writer.Write(bc);
                        }

                        PropertiesSerializer.Default.Write(writer, model.Properties);
                    }
                }

                var protectedBytes = protector.Protect(ms.ToArray());
                return protectedBytes.ToBase64UrlEncoded();
            }
        }

        public AuthenticationTicket Validate(string data)
        {
            if (string.IsNullOrWhiteSpace(data)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(data));

            try
            {
                var unprotectedData = protector.Unprotect(data.FromBase64UrlEncodedBytes());

                using (var ms = new MemoryStream(unprotectedData))
                using (var gzip = new GZipStream(ms, CompressionMode.Decompress))
                using (var reader = new BinaryReader(gzip))
                {
                    var authenticationType = reader.ReadString();

                    var creationTime = new DateTimeOffset(reader.ReadInt64(), TimeSpan.Zero);
                    var expirationTime = creationTime + options.SecurityConfiguration.TokenExpiration;
                    if (expirationTime < DateTimeOffset.UtcNow)
                    {
                        return null;
                    }

                    var nameClaimType = ReadWithDefault(reader, DefaultValues.NameClaimType);
                    var roleClaimType = ReadWithDefault(reader, DefaultValues.RoleClaimType);
                    var count = reader.ReadInt32();
                    var claims = new Claim[count];
                    for (var index = 0; index != count; ++index)
                    {
                        var type = ReadWithDefault(reader, nameClaimType);
                        var value = reader.ReadString();
                        var valueType = ReadWithDefault(reader, DefaultValues.StringValueType);
                        var issuer = ReadWithDefault(reader, DefaultValues.LocalAuthority);
                        var originalIssuer = ReadWithDefault(reader, issuer);
                        claims[index] = new Claim(type, value, valueType, issuer, originalIssuer);
                    }

                    var identity = new ClaimsIdentity(claims, authenticationType, nameClaimType, roleClaimType);
                    var bootstrapContextSize = reader.ReadInt32();
                    if (bootstrapContextSize > 0)
                    {
                        identity.BootstrapContext = reader.ReadString();
                    }

                    var properties = PropertiesSerializer.Default.Read(reader);

                    return new AuthenticationTicket(new ClaimsPrincipal(identity), properties, authenticationType);
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static void WriteWithDefault(BinaryWriter writer, string value, string defaultValue)
        {
            if (string.Equals(value, defaultValue, StringComparison.Ordinal)) writer.Write(DefaultValues.DefaultStringPlaceholder);
            else writer.Write(value);
        }

        private static string ReadWithDefault(BinaryReader reader, string defaultValue)
        {
            var value = reader.ReadString();
            return string.Equals(value, DefaultValues.DefaultStringPlaceholder, StringComparison.Ordinal) ? defaultValue : value;
        }

        private static class DefaultValues
        {
            public const string DefaultStringPlaceholder = "\0";
            public const string NameClaimType = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name";
            public const string RoleClaimType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role";
            public const string LocalAuthority = "LOCAL AUTHORITY";
            public const string StringValueType = "http://www.w3.org/2001/XMLSchema#string";
        }
    }
}