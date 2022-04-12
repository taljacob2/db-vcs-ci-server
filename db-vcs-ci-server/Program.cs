using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

//// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
    app.UseSwagger();
    app.UseSwaggerUI();
//}

app.UseHttpsRedirection();

// ---------- Controller ---------- 

// ----- Global Variables -----

const string WORKING_DIRECTORY = @"C:\Windows\System32";
int COMMAND_EXIT_CODE = 0;

// Make sure it is ignored it `.gitignore`.
const string CMD_COMMAND_FILE_NAME = "db-vcs-ci-server-command";

// ----- Global Variables -----


/// <summary>
///     For tests.
/// </summary>
app.MapGet("/api/test", () =>
{
    return "test works!";
});

app.MapPost("/api/execute-command",
    async (HttpContext context, HttpRequest request, string workingDirectory,
    string cmdOrPsOrCustomPathToExecutable, string cmdOrPsOrCustomFileExtension) =>
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

            // Read the raw file as a `string` command.
            string commandTextString = await reader.ReadToEndAsync();

            // Create an empty directory for the requested working-directory.
            commandOutput += Environment.NewLine;
            commandOutput += await RunCommand("cmd", "cmd", $"mkdir {workingDirectory}");

            // Execute the raw command given.
            commandOutput += Environment.NewLine;
            commandOutput += await RunCommand(cmdOrPsOrCustomPathToExecutable,
                cmdOrPsOrCustomFileExtension, commandTextString);

            // Check result exitcode.
            if (COMMAND_EXIT_CODE != 0)
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

async Task<string> RunCommand(string cmdOrPsOrCustomPathToExecutable,
    string cmdOrPsOrCustomFileExtension,
    string commandTextString,
    string workingDirectoryPath = WORKING_DIRECTORY)
{
    ExtractCommandAndArgs(commandTextString, out string command,
        out List<string> args);
    string commandArgsAsString = ConvertCommandArgsToLargeString(args);
    string commandFilePath = 
        CreateCommandFile(cmdOrPsOrCustomFileExtension, command);
    
    var process = new Process();

    process.StartInfo.WorkingDirectory = workingDirectoryPath;

    if (cmdOrPsOrCustomPathToExecutable == "cmd")
    {
        process.StartInfo.FileName = @"C:\Windows\System32\cmd.exe";
    }
    else if (cmdOrPsOrCustomPathToExecutable == "ps")
    {
        process.StartInfo.FileName =
            @"C:\Windows\System32\WindowsPowerShell\v1.0\powershell.exe";
    }
    else { process.StartInfo.FileName = cmdOrPsOrCustomPathToExecutable; }

    // process.StartInfo.Verb = "runas"; // Run as administrator.

    if (cmdOrPsOrCustomPathToExecutable == "ps")
    {
        process.StartInfo.Arguments =
            "/c " + $"Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass; {commandFilePath}{commandArgsAsString}";
    }
    else
    {
        process.StartInfo.Arguments =
            "/c " + $"{commandFilePath}{commandArgsAsString}";
    }

    process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;

    process.StartInfo.UseShellExecute = false;
    process.StartInfo.RedirectStandardOutput = true;
    process.StartInfo.RedirectStandardError = true;

    process.Start();

    string output = process.StandardOutput.ReadToEnd();
    string err = process.StandardError.ReadToEnd();
    // Console.WriteLine(process); // DEBUG.

    process.WaitForExit();

    // Update the exit-code variable.
    COMMAND_EXIT_CODE = process.ExitCode;

    // Delete the command file after its use.
    DeleteFile(commandFilePath);

    return output + err;
}

void ExtractCommandAndArgs(string commandTextString,
    out string command, out List<string> args)
{
    string flag = @"&ARGS[]=";
    command = commandTextString;
    args = new List<string>();
    if (!commandTextString.Contains(flag)) { return; }


    // There are arguments in the given command, we need to extract them.
    int firstArgIndex = commandTextString.IndexOf(flag);
    command = commandTextString.Substring(0, firstArgIndex);
    string argsAsString = commandTextString.Substring(firstArgIndex);

    while (true)
    {
        string argsAsStringWithoutFirstArgFlag =
            argsAsString.Substring(flag.Length);
        string argValue = argsAsStringWithoutFirstArgFlag;

        if (argsAsStringWithoutFirstArgFlag.Contains(flag))
        {
            argValue = argsAsStringWithoutFirstArgFlag.Substring(0,
                argsAsStringWithoutFirstArgFlag.IndexOf(flag));
        }

        // Insert the extracted `argValue` to list.
        args.Add(argValue);

        // Step ahead.
        argsAsString = argsAsString.Substring(flag.Length + argValue.Length);

        // Check for stop condition.
        if (argsAsStringWithoutFirstArgFlag.Length - argValue.Length == 0)
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
string CreateCommandFile(string cmdOrPsOrCustomFileExtension, string command,
    string workingDirectoryPath = "",
    string commandFileNameWithoutExtension = CMD_COMMAND_FILE_NAME)
{
    string fileExtension = cmdOrPsOrCustomFileExtension;
    if (cmdOrPsOrCustomFileExtension == "cmd")
    {
        fileExtension = ".bat";
    }
    else if (cmdOrPsOrCustomFileExtension == "ps")
    {
        fileExtension = ".ps1";
    }

    if (workingDirectoryPath == "")
    {
        workingDirectoryPath = Directory.GetCurrentDirectory();
    }

    // Create the file, or overwrite if the file exists.
    string commandFilePath 
        = $"{workingDirectoryPath}/{commandFileNameWithoutExtension}{fileExtension}";
    using (FileStream fileStream = File.Create(commandFilePath))
    {

        // Create content to the file.
        byte[] fileContent = new UTF8Encoding(true).GetBytes(command);

        // Insert content to the file.
        fileStream.Write(fileContent, 0, fileContent.Length);
    }

    return commandFilePath;
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

app.Run();
