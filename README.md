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

### Make Sure "db-vcs-ci-server" Will Have Credentials For Managing Your Database

#### Examples

- **For *SSMS* And *IIS***

  The IIS app (of "db-vcs-ci-server") that runs the "sqlcmd" command may not have credentials for managing the database. You should create a new "SQL Server Authentication" user in your SSMS, so your IIS app could use it to export the database through "sqlcmd".

  1. Follow this [video](https://www.youtube.com/watch?v=qfuK0V1tlrA) for doing so.
  2. Make sure the "db-vcs-ci-client" use these credentials when using the "sqlcmd" command.

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
