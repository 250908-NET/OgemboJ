var builder = WebApplication.CreateBuilder(args);



var app = builder.Build();


app.MapGet("/", () => Results.Ok(new { message = "Minimal API is running" }));


app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.Run();
