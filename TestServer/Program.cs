var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var hasResponded = false;

app.MapGet("/", () =>
    {
        if (hasResponded)
        {
            return Results.StatusCode(200);
        }
        hasResponded = true;
        return Results.StatusCode(502);
    }
);

app.Run();
