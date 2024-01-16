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
            string token = "testInviteeUser@example.com;testSender123;testProject123;testSecret123";

            // Act
            var result = InvitationTokenUtil.ParseInviteToken(token);

            // Assert
            Assert.Equal("testInviteeUser@example.com", result.Item1);
            Assert.Equal("testSender123", result.Item2);
            Assert.Equal("testProject123", result.Item3);
            Assert.Equal("testSecret123", result.Item4);
        }

        [Fact]
        public void ParseInviteLink_ReturnsValidResult()
        {
            // Arrange
            string token = "testSender123;testProject123;testSecret123";

            // Act
            var result = InvitationTokenUtil.ParseInviteLink(token);

            // Assert
            Assert.Equal("testSender123", result.Item1);
            Assert.Equal("testProject123", result.Item2);
            Assert.Equal("testSecret123", result.Item3);
        }

        [Fact]
        public void Base64Encode_DecodesSuccessfully()
        {
            // Arrange
            string plainText = "testPlainText";

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
            string value = "testValue123";

            // Act
            var hash = InvitationTokenUtil.Sha256Hash(value);

            // Assert
            Assert.NotNull(hash);
            Assert.NotEmpty(hash);
        }
    }
}
