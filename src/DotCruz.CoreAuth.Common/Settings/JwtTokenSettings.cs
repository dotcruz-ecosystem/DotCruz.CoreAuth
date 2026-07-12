namespace DotCruz.CoreAuth.Common.Settings
{
    public class JwtTokenSettings
    {
        public uint ExpirationTimeMinutes { get; set; }
        public uint RefreshTokenExpirationTimeDays { get; set; }
        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
        public string Kid { get; set; } = string.Empty;
        public string PrivateKeyPem { get; set; } = string.Empty;
    }
}
