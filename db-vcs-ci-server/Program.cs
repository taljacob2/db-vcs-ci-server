using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

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
int CMD_COMMAND_EXIT_CODE = 0;
const string CMD_COMMAND_FILE_NAME = "db-vcs-ci-server-command.bat";


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

            // Sumarizes all output.
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
            if (CMD_COMMAND_EXIT_CODE != 0)
            {

                // The command has exited with an error.
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                return context.Response.WriteAsync(commandOutput);
            }
            else
            {

                // Return the raw output of the command.
                context.Response.StatusCode = StatusCodes.Status200OK;
                return context.Response.WriteAsync(commandOutput);
            }
        }
    }).Accepts<IFormFile>("text/plain");

async Task<string> RunCmdCommand(string cmdCommandTextString,
    string workingDirectoryPath = WORKING_DIRECTORY)
{
    ExtractCommandAndArgs(cmdCommandTextString, out string command,
        out List<string> args);
    string commandArgsAsString = ConvertCommandArgsToLargeString(args);
    string cmdCommandFilePath = CreateBatchFile(command);

    var process = new Process();

    process.StartInfo.WorkingDirectory = workingDirectoryPath;
    process.StartInfo.FileName = @"C:\Windows\System32\cmd.exe";
    //process.FileName = @"C:\Windows\System32\WindowsPowerShell\v1.0\powershell.exe";  
    process.StartInfo.Verb = "runas"; // Run as administrator.
    process.StartInfo.Arguments =
        "/c " + $"{cmdCommandFilePath}{commandArgsAsString}";
    process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;

    process.StartInfo.UseShellExecute = false;
    process.StartInfo.RedirectStandardOutput = true;
    process.StartInfo.RedirectStandardError = true;

    process.Start();

    string output = process.StandardOutput.ReadToEnd();
    string err = process.StandardError.ReadToEnd();

    process.WaitForExit();

    // Update the exit-code variable.
    CMD_COMMAND_EXIT_CODE = process.ExitCode;

    // Delete the command file after its use.
    DeleteFile(cmdCommandFilePath);

    return output + err;
}

void ExtractCommandAndArgs(string cmdCommandTextString, out string command,
    out List<string> args)
{
    string flag = @"&ARGS[]=";
    command = cmdCommandTextString;
    args = new List<string>();
    if (!cmdCommandTextString.Contains(flag)) { return; }


    // There are arguments in the given command, we need to extract them.
    int firstArgIndex = cmdCommandTextString.IndexOf(flag);
    command = cmdCommandTextString.Substring(0, firstArgIndex);
    string argsAsString = cmdCommandTextString.Substring(firstArgIndex);

    while (true)
    {
        string argsAsStringWithoutFirstArg =
            argsAsString.Substring(flag.Length);
        string arg = argsAsStringWithoutFirstArg;

        if (argsAsStringWithoutFirstArg.Contains(flag))
        {
            arg = argsAsStringWithoutFirstArg.Substring(0,
                argsAsStringWithoutFirstArg.IndexOf(flag));
        }

        args.Add(arg); // Insert the extracted arg to list.

        argsAsString = argsAsString.Substring(flag.Length + arg.Length);
        if (argsAsStringWithoutFirstArg.Length - arg.Length == 0)
        {
            break;
        }
    }
}

string ConvertCommandArgsToLargeString(List<string> args)
{
    StringBuilder stringBuilder = new StringBuilder();
    foreach(string arg in args)
    {
        stringBuilder.Append(" " + arg);
    }

    return stringBuilder.ToString();
}

/// <summary>
///     Private. Use with caution.
/// </summary>
string CreateBatchFile(string command,
    string workingDirectoryPath = "",
    string cmdCommandFileName = CMD_COMMAND_FILE_NAME)
{
    if (workingDirectoryPath == "")
    {
        workingDirectoryPath = Directory.GetCurrentDirectory();
    }

    // Create the file, or overwrite if the file exists.
    string cmdCommandFilePath = $"{workingDirectoryPath}/{cmdCommandFileName}";
    using (FileStream fileStream = File.Create(cmdCommandFilePath))
    {

        // Create content to the file.
        byte[] fileContent = new UTF8Encoding(true).GetBytes(command);

        // Insert content to the file.
        fileStream.Write(fileContent, 0, fileContent.Length);
    }

    return cmdCommandFilePath;
}

void DeleteFile(string cmdCommandFileName)
{
    if (File.Exists(cmdCommandFileName))
    {
        File.Delete(cmdCommandFileName);
    }
}

/// <summary>
///     Share a file via a download.
/// </summary>
app.MapGet("/api/download-file", (string filePathInServer, string mimeType) =>
{
    return ShareFileDownload(filePathInServer, mimeType);
});

IResult ShareFileDownload(string filePathToShare, string mimeType)
{
    return Results.File(filePathToShare, contentType: mimeType);
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
