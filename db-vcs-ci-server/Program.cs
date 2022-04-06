using Microsoft.VisualBasic;
using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// ---------- Controllers ---------- 

app.MapGet("/api/script", () =>
{
    return "script works!";
});

app.MapPost("/api/execute-a-export-db-sql", async (HttpRequest request) =>
{
    using (var reader = new StreamReader(request.Body, System.Text.Encoding.UTF8))
    {

        // Read the raw file as a CMD `string` command.
        string cmdCommandTextString = await reader.ReadToEndAsync();

        RunCmdCommand(cmdCommandTextString);

        return cmdCommandTextString;
    }
});

void RunCmdCommand(string cmdCommandTextString)
{
    var process = new ProcessStartInfo();
    process.UseShellExecute = true;
    process.WorkingDirectory = @"C:\Windows\System32";
    process.FileName = @"C:\Windows\System32\cmd.exe";
    //process.FileName = @"C:\Windows\System32\WindowsPowerShell\v1.0\powershell.exe";
    process.Verb = "runas";
    process.Arguments = "/c " + cmdCommandTextString;
    process.WindowStyle = ProcessWindowStyle.Normal;
    Process.Start(process);
}

// Tutorial down here: ------------------------------------------- TODO: remove.

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
       new WeatherForecast
       (
           DateTime.Now.AddDays(index),
           Random.Shared.Next(-20, 55),
           summaries[Random.Shared.Next(summaries.Length)]
       ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.Run();

internal record WeatherForecast(DateTime Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

