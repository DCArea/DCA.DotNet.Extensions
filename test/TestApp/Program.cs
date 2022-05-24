var builder = WebApplication.CreateBuilder(args);
builder.Logging.AddEnrichedJsonConsole();

var app = builder.Build();

// named endpoint
app.MapGet("/my-items/{name}", (string name) =>
{
    return new MyItem(name);
})
.WithName("GetMyItem");

// unnamed endpoint
app.MapGet("/my-items2/{name}", (string name) =>
{
    return new MyItem(name);
});

app.Run();

record MyItem(string name);

public partial class Program { }
