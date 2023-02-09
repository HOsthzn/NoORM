## Usage Examples of AdoNet Class
AdoNet is a class in C# that provides a simple way to interact with a database using ADO.NET. The class is initialized with a connection string, and it provides several methods to execute database commands such as stored procedures, text commands, and scalar queries.

Here are some examples of how to use the AdoNet class:

### Initializing the AdoNet Class
```csharp
string connectionString = "Data Source=(local);Initial Catalog=MyDatabase;Integrated Security=True;";
AdoNet ado = new AdoNet(connectionString);
```


### Executing a Stored Procedure
```csharp
string storedProcName = "sp_GetEmployees";
SqlParameter[] parameters = new SqlParameter[] {
    new SqlParameter("@EmpID", SqlDbType.Int) { Value = 100 }
};

var result = ado.ExecuteCommand(storedProcName, CommandType.StoredProcedure, parameters);
```

### Executing a Stored Procedure Asynchronously
```csharp
string storedProcName = "sp_GetEmployees";
SqlParameter[] parameters = new SqlParameter[] {
    new SqlParameter("@EmpID", SqlDbType.Int) { Value = 100 }
};

var result = await ado.ExecuteCommandAsync(storedProcName, CommandType.StoredProcedure, parameters);
```

### Executing a T-SQL Command
```csharp
string tsqlCommand = "UPDATE Employees SET FirstName='John' WHERE EmpID=100";

var result = ado.ExecuteCommand(tsqlCommand, CommandType.Text);
```

### Executing a T-SQL Command Asynchronously
```csharp
string tsqlCommand = "UPDATE Employees SET FirstName='John' WHERE EmpID=100";

var result = await ado.ExecuteCommandAsync(tsqlCommand, CommandType.Text);
```

### Executing a Scalar Query
```csharp
string tsqlCommand = "SELECT COUNT(*) FROM Employees";

var result = ado.ExecuteScalar<int>(tsqlCommand, CommandType.Text);
```

### Executing a Scalar Query Asynchronously
```csharp
string tsqlCommand = "SELECT COUNT(*) FROM Employees";

var result = await ado.ExecuteScalarAsync<int>(tsqlCommand, CommandType.Text);
```
