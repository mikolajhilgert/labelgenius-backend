using System.Security.Cryptography;
using System.Text;

namespace projectservice.Utils
{
    public static class InvitationTokenUtil
    {
        public static (string Token, string Secret) CreateInvitationToken(string inviteeEmail, string projectInvitedToId, string sender)
        {
            string tokenBase = $"{inviteeEmail};{sender};{projectInvitedToId}";
            string secret = Guid.NewGuid().ToString();
            tokenBase += $";{secret}";
            return (tokenBase, secret);
        }

        public static (string Token, string Secret) CreateInvitationForLink(string projectId, string sender)
        {
            string tokenBase = $"{sender};{projectId}";
            string secret = Guid.NewGuid().ToString();
            tokenBase += $";{secret}";
            return (tokenBase, secret);
        }

        public static (string InviteeEmail, string Sender, string ProjectInvitedToId, string Secret) ParseInviteToken(string token)
        {
            string[] words = token.Split(';');
            return (words[0], words[1], words[2], words[3]);
        }

        public static (string Sender, string ProjectId, string Secret) ParseInviteLink(string token)
        {
            string[] words = token.Split(';');
            return (words[0], words[1], words[2]);
        }

        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(plainTextBytes);
        }

        public static string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = Convert.FromBase64String(base64EncodedData);
            return Encoding.UTF8.GetString(base64EncodedBytes);
        }

        public static string Sha256Hash(string value)
        {
            StringBuilder sb = new StringBuilder();

            using (SHA256 hash = SHA256.Create())
            {
                byte[] result = hash.ComputeHash(Encoding.UTF8.GetBytes(value));

                foreach (byte b in result)
                    sb.Append(b.ToString("x2"));
            }

            return sb.ToString();
        }
    }
}
