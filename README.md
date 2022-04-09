# db-vcs-ci-server
Implemented in .NET Core 6 Minimal Api.

## Installation

- Make sure you have [.NET Core 6](https://dotnet.microsoft.com/en-us/download/dotnet/6.0) installed.

  It may be already installed on your computer.
  You can view all .NET installations on your computer, by running:
  ```
  dotnet --info
  ```

- Make sure you have [sqlcmd](https://docs.microsoft.com/en-us/sql/tools/sqlcmd-utility?view=sql-server-ver15) installed, to allow queries to the database through the cli.

  It may be already installed on your computer.
  You can check this by running:
  ```
  sqlcmd -?
  ```

## Developer

### Debugging CMD

For debugging cmd window,
you can navigate to [`db-vcs-ci-server/Program.cs`](/db-vcs-ci-server/Program.cs),
and in `RunCmdCommand` function change the lines from:
```csharp
process.WindowStyle = ProcessWindowStyle.Hidden;
process.StartInfo.UseShellExecute = false;
process.StartInfo.RedirectStandardOutput = true;
process.StartInfo.RedirectStandardError = true;
```
to
```csharp
process.WindowStyle = ProcessWindowStyle.Normal;
process.StartInfo.UseShellExecute = true;
process.StartInfo.RedirectStandardOutput = false;
process.StartInfo.RedirectStandardError = false;
```
and comment-out the lines:
```csharp
string output = process.StandardOutput.ReadToEnd();
string err = process.StandardError.ReadToEnd();
```
so you will view the cmd window launching.
