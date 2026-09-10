using System;
using Xunit;

namespace Jcruse03.PortalPass;

public sealed class PasswordMapTests
{
    [Fact]
    public void ExactEndpointOverridesHostDefault()
    {
        var map = PasswordMap.Parse(new[] { "example.com=common", "example.com:2471=test" });

        Assert.True(map.TryResolve("EXAMPLE.COM.", 2471, out var exact));
        Assert.Equal("test", exact);
        Assert.True(map.TryResolve("example.com", 2456, out var fallback));
        Assert.Equal("common", fallback);
    }

    [Fact]
    public void DoesNotEquateHostnameAndIpAddress()
    {
        var map = PasswordMap.Parse(new[] { "valheim.example.net=secret" });
        Assert.False(map.TryResolve("203.0.113.20", 2456, out _));
    }

    [Fact]
    public void NormalizesIpv6AndSupportsExactOverride()
    {
        var map = PasswordMap.Parse(new[] { "2001:db8::1=common", "[2001:0db8:0:0:0:0:0:1]:2459=exact" });
        Assert.True(map.TryResolve("2001:db8::1", 2459, out var exact));
        Assert.Equal("exact", exact);
        Assert.True(map.TryResolve("2001:db8::1", 2462, out var fallback));
        Assert.Equal("common", fallback);
    }

    [Fact]
    public void SplitsOnlyOnFirstEqualsSign()
    {
        var map = PasswordMap.Parse(new[] { "example.com=abc=123" });
        Assert.True(map.TryResolve("example.com", 2456, out var password));
        Assert.Equal("abc=123", password);
    }

    [Fact]
    public void DuplicateNormalizedEndpointRejectsEntireFile()
    {
        var error = Assert.Throws<FormatException>(() => PasswordMap.Parse(new[] { "EXAMPLE.COM.=one", "example.com=two" }));
        Assert.Contains("duplicate endpoint", error.Message);
    }

    [Theory]
    [InlineData("example.com")]
    [InlineData("example.com:0=x")]
    [InlineData("example.com:65536=x")]
    [InlineData("[2001:db8::1=x")]
    [InlineData("example.com=")]
    public void MalformedLineIsRejected(string line)
    {
        Assert.Throws<FormatException>(() => PasswordMap.Parse(new[] { line }));
    }

    [Fact]
    public void CommentsAndQuotedPasswordsAreSupported()
    {
        var map = PasswordMap.Parse(new[] { "# fleet", "", "example.com = \"space pass\"" });
        Assert.True(map.TryResolve("example.com", 2456, out var password));
        Assert.Equal("space pass", password);
    }
}
