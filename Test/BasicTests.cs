using System.Net;
using System.Text;
using Flurl.Http.Testing;
using Polly;
using Polly.Flurl;
using Xunit.Abstractions;

namespace Test;

public class BasicTests
{
    private readonly ITestOutputHelper output;

    public BasicTests(ITestOutputHelper output)
    {
        this.output = output;
        Console.SetOut(new OutputConverter(output));
    }

    [Fact]
    public async Task RetryThenSuccess()
    {
        using var httpTest = new HttpTest();
        httpTest.RespondWith("", status: 500);
        httpTest.RespondWith("", status: 200);

        var response = await "http://www.google.com".WithRetry().GetAsync();
        response.StatusCode.Should().Be(200);
    }
}