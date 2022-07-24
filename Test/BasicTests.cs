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
        Console.SetOut(new Converter(output));
    }

    [Fact]
    public async Task RetryThenSuccess()
    {
        using var httpTest = new HttpTest();
        httpTest.RespondWith("", status: 500);
        httpTest.RespondWith("", status: 200);
        var x = new Url("http://localhost:5102").WithRetry();

        output.WriteLine("type of handler: " + x.Client.HttpMessageHandler.ToString());

        var response = await x.GetAsync();
        response.StatusCode.Should().Be(200);
    }
    private class Converter : TextWriter
    {
        readonly ITestOutputHelper _output;
        public Converter(ITestOutputHelper output)
        {
            _output = output;
        }
        public override Encoding Encoding
        {
            get { return Encoding.ASCII; }
        }
        public override void WriteLine(string? message)
        {
            _output.WriteLine(message);
        }
        public override void WriteLine(string? format, params object?[] args)
        {
            _output.WriteLine(format, args);
        }

        public override void Write(char value)
        {
            throw new NotSupportedException("This text writer only supports WriteLine(string) and WriteLine(string, params object[]).");
        }
    }
}