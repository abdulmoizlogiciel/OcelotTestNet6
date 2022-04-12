using System.Security.Claims;

namespace OcelotTestNet6.Utils
{
    public static class UserClaims
    {
        public static (string userDesc, string boothId) ParseUserIdentifier(string userIdentifier)
        {
            var identifierSplit = userIdentifier.Split('@');
            var userDesc = identifierSplit[0];
            var boothId = identifierSplit.Length > 1 ? identifierSplit[1] : null;
            return (userDesc, boothId);
        }

        public static string UserIdentifier(this ClaimsPrincipal claimsPrincipal)
        {
            string clientId = claimsPrincipal.Claims.Where(x => x.Type.Equals("client_id")).Select(x => x.Value).FirstOrDefault();
            string sub = claimsPrincipal.Claims.Where(x => x.Type.Equals("sub")).Select(x => x.Value).FirstOrDefault();
            if (string.IsNullOrWhiteSpace(sub))
            {
                return clientId;
            }
            return GenerateUserIdentifier(sub, clientId);
        }

        public static string ClientId(this ClaimsPrincipal claimsPrincipal)
        {
            return claimsPrincipal.Claims.Where(x => x.Type.Equals("client_id")).Select(x => x.Value).FirstOrDefault();
        }

        public static string OriginatingUserId(this ClaimsPrincipal claimsPrincipal)
        {
            return claimsPrincipal.Claims.Where(x => x.Type.Equals("sub")).Select(x => x.Value).FirstOrDefault();
        }

        public static int MaxConnectionAllowed(this ClaimsPrincipal claimsPrincipal)
        {
            return claimsPrincipal.Claims.Where(x => x.Type.Equals("max_connection_allowed_for_hub")).Select(x => int.Parse(x.Value)).FirstOrDefault();
        }

        public static string GenerateUserIdentifier(string sub, string clientId) => $"{sub}@{clientId}";
    }
}
