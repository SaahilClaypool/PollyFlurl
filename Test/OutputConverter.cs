using System.Text;
using Xunit.Abstractions;

internal class OutputConverter : TextWriter
{
    readonly ITestOutputHelper _output;
    public OutputConverter(ITestOutputHelper output)
    {
        _output = output;
    }
    public override Encoding Encoding => Encoding.UTF8;
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