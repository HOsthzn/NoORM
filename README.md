## AdoNet Class Usage Examples

The AdoNet class provides several functions to execute SQL commands, retrieve results and manipulate data in a database using ADO.NET. Here's an example of how to use the different functions of the AdoNet class.

### ExecuteCommand
This method executes a non-query command, such as an INSERT, UPDATE, or DELETE statement, against a database and returns the number of rows affected and the parameters used during the execution.
```csharp
string connectionString = "Data Source=(local);Initial Catalog=TestDB;Integrated Security=True";
AdoNet adoNet = new AdoNet(connectionString);

SqlParameter[] parameters = 
{
    new SqlParameter("@firstName", "John"),
    new SqlParameter("@lastName", "Doe")
};

var result = adoNet.ExecuteCommand("INSERT INTO dbo.Employees (FirstName, LastName) VALUES (@firstName, @lastName)",
    CommandType.Text, parameters);

Console.WriteLine("Number of rows affected: " + result.rowsAffected);
```

### ExecuteCommandAsync
This method is the asynchronous version of ExecuteCommand. It executes a non-query command asynchronously and returns the number of rows affected and the parameters used during the execution.
```csharp
string connectionString = "Data Source=(local);Initial Catalog=TestDB;Integrated Security=True";
AdoNet adoNet = new AdoNet(connectionString);

SqlParameter[] parameters = 
{
    new SqlParameter("@firstName", "John"),
    new SqlParameter("@lastName", "Doe")
};

var result = await adoNet.ExecuteCommandAsync("INSERT INTO dbo.Employees (FirstName, LastName) VALUES (@firstName, @lastName)",
    CommandType.Text, parameters);

Console.WriteLine("Number of rows affected: " + result.rowsAffected);
```

### ExecuteCommandDataSet
This method executes a command and returns the results in a DataSet.
```csharp
string connectionString = "Data Source=(local);Initial Catalog=TestDB;Integrated Security=True";
AdoNet adoNet = new AdoNet(connectionString);

var result = adoNet.ExecuteCommandDataSet("SELECT * FROM dbo.Employees", CommandType.Text);

foreach (DataRow row in result.Tables[0].Rows)
{
    Console.WriteLine("First Name: " + row["FirstName"] + ", Last Name: " + row["LastName"]);
}
```

### ExecuteScalar
ExecuteScalar is used to execute a command that returns a single value as a result. The function returns an object that can be cast to the appropriate data type.
```csharp
string connectionString = "Your connection string here";
AdoNet adoNet = new AdoNet(connectionString);

string commandText = "SELECT COUNT(*) FROM dbo.TableName";
CommandType commandType = CommandType.Text;

object result = adoNet.ExecuteScalar(commandText, commandType);

int count = (int) result;
Console.WriteLine("Number of rows in TableName: " + count);
```

### ExecuteScalarAsync
This example is similar to the previous example, but uses the asynchronous version of the ExecuteScalar function. The function returns a Task<object>.
```csharp
string connectionString = "Your connection string here";
AdoNet adoNet = new AdoNet(connectionString);

string commandText = "SELECT COUNT(*) FROM dbo.TableName";
CommandType commandType = CommandType.Text;

object result = await adoNet.ExecuteScalarAsync(commandText, commandType);

int count = (int) result;
Console.WriteLine("Number of rows in TableName: " + count);
```

### ExecuteReader
ExecuteReader is used to execute a query and map the results to a list of objects.
```csharp
List<User> users = adoNet.ExecuteReader<User>("SELECT * FROM Users", CommandType.Text);
```

Executing a stored procedure and mapping the results to a list of objects
```csharp
List<User> users = adoNet.ExecuteReader<User>("GetUsers", CommandType.StoredProcedure);
```

OR, Executing a stored procedure with parameters and mapping the results to a list of objects
```csharp
SqlParameter[] parameters = new[]
{
    new SqlParameter("@city", "London"),
};

List<User> users = adoNet.ExecuteReader<User>("GetUsersByCity", CommandType.StoredProcedure, parameters);
```

### ExecuteReaderAsync
This example is similar to the previous example, but uses the asynchronous version of the ExecuteReaderAsync function,
```csharp
List<User> users = await adoNet.ExecuteReaderAsync<User>("SELECT * FROM Users", CommandType.Text);
```

```csharp
List<User> users = await adoNet.ExecuteReaderAsync<User>("GetUsers", CommandType.StoredProcedure);
```

```csharp
SqlParameter[] parameters = new[]
{
    new SqlParameter("@city", "London"),
};

List<User> users = await adoNet.ExecuteReaderAsync<User>("GetUsersByCity", CommandType.StoredProcedure, parameters);
```
