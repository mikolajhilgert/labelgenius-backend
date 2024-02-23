using projectservice.Utils;
using System.Text;

namespace projectservice.unittests
{
    public class UnitTests
    {
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
