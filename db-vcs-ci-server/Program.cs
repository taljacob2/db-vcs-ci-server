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

const string WORKING_DIRECTORY = @"C:\Windows\System32";
int EXIT_CODE = 0;
    
app.MapPost("/api/execute-cmd-command",
    async (HttpContext context, HttpRequest request, string workingDirectory) =>
{
    if (workingDirectory == null)
    {

        // Default working-directory is `WORKING_DIRECTORY`.
        workingDirectory = WORKING_DIRECTORY;
    }

    using (var reader = new StreamReader(request.Body, System.Text.Encoding.UTF8))
    {

        string commandOutput = "";

        // Read the raw file as a CMD `string` command.
        string cmdCommandTextString = await reader.ReadToEndAsync();

        // Create an empty directory for the requested working-directory.
        commandOutput += Environment.NewLine;
        commandOutput += await RunCmdCommand($"mkdir {workingDirectory}");

        // Execute the raw command given.
        commandOutput += Environment.NewLine;
        commandOutput += await RunCmdCommand(cmdCommandTextString);

        // Check result exitcode.
        if (EXIT_CODE != 0)
        {

            // The command has exited with an error.
            IResult result = Results.Text(commandOutput);
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            return result.ExecuteAsync(context);
        }
        else
        {

            // Return the raw output of the command.
            IResult result = Results.Text(commandOutput);
            context.Response.StatusCode = StatusCodes.Status200OK;
            return result.ExecuteAsync(context);
        }
    }
});

async Task<string> RunCmdCommand(string cmdCommandTextString,
    string workingDirectoryPath = WORKING_DIRECTORY)
{   
    var process = new Process();

    process.StartInfo.WorkingDirectory = workingDirectoryPath;
    process.StartInfo.FileName = @"C:\Windows\System32\cmd.exe";
    //process.FileName = @"C:\Windows\System32\WindowsPowerShell\v1.0\powershell.exe";  
    process.StartInfo.Verb = "runas";
    process.StartInfo.Arguments = "/c " + cmdCommandTextString;
    process.StartInfo.WindowStyle = ProcessWindowStyle.Normal;

    process.StartInfo.UseShellExecute = false;
    process.StartInfo.RedirectStandardOutput = true;
    process.StartInfo.RedirectStandardError = true;

    process.Start();

    string output = process.StandardOutput.ReadToEnd();
    string err = process.StandardError.ReadToEnd();

    process.WaitForExit();
    EXIT_CODE = process.ExitCode;

    return output + err;
}

IResult ShareFileDownload(string filePathToShare, string mimeType)
{
    return Results.File(filePathToShare, contentType: mimeType);
}

/// <summary>
/// Share a file via a download.
/// </summary>
app.MapGet("/api/download-file", (string filePathInServer, string mimeType) =>
{
    return ShareFileDownload(filePathInServer, mimeType);
});

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

