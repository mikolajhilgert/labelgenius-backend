using projectservice.Utils;
using System.Text;

namespace projectservice.unittests
{
    public class UnitTests
    {
        [Fact]
        public void ParseInviteToken_ReturnsValidResult()
        {
            // Arrange
            string token = "invitee@example.com;sender123;project123;secret123";

            // Act
            var result = InvitationTokenUtil.ParseInviteToken(token);

            // Assert
            Assert.Equal("invitee@example.com", result.Item1);
            Assert.Equal("sender123", result.Item2);
            Assert.Equal("project123", result.Item3);
            Assert.Equal("secret123", result.Item4);
        }

        [Fact]
        public void ParseInviteLink_ReturnsValidResult()
        {
            // Arrange
            string token = "sender123;project123;secret123";

            // Act
            var result = InvitationTokenUtil.ParseInviteLink(token);

            // Assert
            Assert.Equal("sender123", result.Item1);
            Assert.Equal("project123", result.Item2);
            Assert.Equal("secret123", result.Item3);
        }

        [Fact]
        public void Base64Encode_DecodesSuccessfully()
        {
            // Arrange
            string plainText = "plainText";

            // Act
            var encodedText = InvitationTokenUtil.Base64Encode(plainText);
            var decodedText = InvitationTokenUtil.Base64Decode(encodedText);

            // Assert 
            Assert.Equal(Convert.ToBase64String(Encoding.UTF8.GetBytes(plainText)), encodedText);
            Assert.Equal(plainText, decodedText);
        }

        [Fact]
        public void Sha256Hash_GeneratesHash()
        {
            // Arrange
            string value = "value123";

            // Act
            var hash = InvitationTokenUtil.Sha256Hash(value);

            // Assert
            Assert.NotNull(hash);
            Assert.NotEmpty(hash);
        }
    }
}