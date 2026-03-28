using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace PRoCon.Core.Layer
{
    /// <summary>
    /// Generates and validates JWT tokens for LayerHub authentication.
    /// Replaces the salt/hash handshake in the old LayerClient login flow.
    /// </summary>
    public class LayerAuthService
    {
        /// <summary>
        /// Claim type used to store the privilege flags bitmask in the JWT.
        /// </summary>
        public const string PrivilegesClaimType = "procon/privileges";

        private readonly byte[] _signingKey;
        private readonly string _issuer;
        private readonly TimeSpan _tokenLifetime;

        /// <summary>
        /// Creates a new auth service instance.
        /// </summary>
        /// <param name="signingKey">
        /// A base-64-encoded secret key (>= 32 bytes recommended).
        /// If null or empty, a random 256-bit key is generated.
        /// </param>
        /// <param name="issuer">Token issuer claim. Defaults to "PRoCon.Layer".</param>
        /// <param name="tokenLifetime">
        /// How long issued tokens remain valid. Defaults to 24 hours.
        /// </param>
        public LayerAuthService(string signingKey = null, string issuer = "PRoCon.Layer", TimeSpan? tokenLifetime = null)
        {
            if (string.IsNullOrWhiteSpace(signingKey))
            {
                _signingKey = new byte[32];
                using (var rng = RandomNumberGenerator.Create())
                {
                    rng.GetBytes(_signingKey);
                }
            }
            else
            {
                _signingKey = Convert.FromBase64String(signingKey);
            }

            _issuer = issuer;
            _tokenLifetime = tokenLifetime ?? TimeSpan.FromHours(24);
        }

        /// <summary>
        /// Generates a signed JWT for the given username and privilege set.
        /// </summary>
        public string GenerateToken(string username, CPrivileges privileges)
        {
            if (string.IsNullOrEmpty(username))
                throw new ArgumentNullException(nameof(username));

            var key = new SymmetricSecurityKey(_signingKey);
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(PrivilegesClaimType, privileges.PrivilegesFlags.ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _issuer,
                audience: _issuer,
                claims: claims,
                expires: DateTime.UtcNow.Add(_tokenLifetime),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        /// <summary>
        /// Validates a JWT and returns the embedded claims on success.
        /// Returns null if the token is invalid, expired, or tampered with.
        /// </summary>
        public ClaimsPrincipal ValidateToken(string token)
        {
            if (string.IsNullOrEmpty(token))
                return null;

            var key = new SymmetricSecurityKey(_signingKey);

            var parameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = _issuer,
                ValidateAudience = true,
                ValidAudience = _issuer,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,
                ClockSkew = TimeSpan.FromMinutes(1)
            };

            try
            {
                var handler = new JwtSecurityTokenHandler();
                return handler.ValidateToken(token, parameters, out _);
            }
            catch (SecurityTokenException)
            {
                return null;
            }
        }

        /// <summary>
        /// Extracts the username from a validated <see cref="ClaimsPrincipal"/>.
        /// </summary>
        public static string GetUsername(ClaimsPrincipal principal)
        {
            return principal?.FindFirst(ClaimTypes.Name)?.Value;
        }

        /// <summary>
        /// Extracts the privilege flags from a validated <see cref="ClaimsPrincipal"/>
        /// and returns them as a <see cref="CPrivileges"/> instance.
        /// </summary>
        public static CPrivileges GetPrivileges(ClaimsPrincipal principal)
        {
            var claim = principal?.FindFirst(PrivilegesClaimType);
            if (claim != null && uint.TryParse(claim.Value, out var flags))
            {
                return new CPrivileges(flags);
            }

            return new CPrivileges();
        }
    }
}
