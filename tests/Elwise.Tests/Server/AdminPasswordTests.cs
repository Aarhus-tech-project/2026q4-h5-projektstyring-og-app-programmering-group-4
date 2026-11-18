using Elwise.Api.Security;
using Xunit;

namespace Elwise.Tests.Server;

public class AdminPasswordTests
{

    private const string Password = "Opvask2026!";

    [Fact]

    public void Verify_RightPassword_ReturnsTrue()
    {
        // Arrange
        string hash = AdminPassword.Hash(Password);

        // Act
        bool ok = AdminPassword.Verify(Password, hash);

        // Assert
        Assert.True(ok);
    }


    [Fact]
    public void Verify_WrongPassword_ReturnsFalse()
    {
        // Arrange
        string hash = AdminPassword.Hash(Password);

        // Act: only the first letter is different
        bool ok = AdminPassword.Verify("opvask2026!", hash);

        // Assert
        Assert.False(ok);
    }
    [Fact]
   
    public void Hash_SamePasswordTwice_GivesDifferentHashes()
    {
        // Act
        string first = AdminPassword.Hash(Password);
        string second = AdminPassword.Hash(Password);

        // Assert: the random salt makes every hash different
        Assert.NotEqual(first, second);
    }
    [Fact]
    public void Verify_BrokenHash_ReturnsFalse()
    {
        // Act
        bool ok = AdminPassword.Verify(Password, "not-a-hash");

        // Assert
        Assert.False(ok);
    }
}