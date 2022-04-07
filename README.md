# db-vcs-ci-server
Implemented in .NET Core 6 Minimal Api.

## Installation

- Make sure you have [.NET Core 6](https://dotnet.microsoft.com/en-us/download/dotnet/6.0) installed.

  If you are not sure it is installed, you can view all .NET installations on the computer, by running:
  ```
  dotnet --info
  ```

- Make sure you install [sqlcmd](https://docs.microsoft.com/en-us/sql/tools/sqlcmd-utility?view=sql-server-ver15), to allow queries to the database through the cli.

## Developer

For debugging cmd window,
you can navigate to [`db-vcs-ci-server/Program.cs` line 55](https://github.com/taljacob2/db-vcs-ci-server/blob/22db4357ed0bbbd2026d1de739e8500b2d25a2a2/db-vcs-ci-server/Program.cs#L55),
and change the line from:
```csharp
process.WindowStyle = ProcessWindowStyle.Hidden;
```
to
```csharp
process.WindowStyle = ProcessWindowStyle.Normal;
```
so you will view the cmd window launching.
