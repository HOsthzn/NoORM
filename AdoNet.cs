public class AdoNet
{
    private readonly string _connectionString;

    public AdoNet(string connectionString)
    {
        // Create a SqlConnectionStringBuilder object from the provided connection string
        SqlConnectionStringBuilder builder = new(connectionString)
        {
            // Set the "Pooling" property of the SqlConnectionStringBuilder to true
            Pooling = true
        };

        // Update the connection string to reflect the changes made to the SqlConnectionStringBuilder
        _connectionString = builder.ConnectionString;
    }

    // This function sets up a SqlCommand object with the given command text, type, and parameters. 
    private static void SetupCommand(SqlCommand command, string commandText, CommandType commandType,
        SqlParameter[]? parameters = null, SqlParameter[]? outParameters = null)
    {
        // Set the command text for the SqlCommand object
        command.CommandText = commandText;

        // Set the command type for the SqlCommand object
        command.CommandType = commandType;

        // If parameters are passed, add them to the SqlCommand object's parameters collection
        if (parameters != null) command.Parameters.AddRange(parameters);

        // If output parameters are passed, add them to the SqlCommand object's parameters collection
        if (outParameters == null) return;
        foreach (SqlParameter parameter in outParameters)
            if (parameter.Direction == ParameterDirection.Output)
                command.Parameters.Add(parameter);
    }

    /// <summary>
    /// Executes a command against the database using the specified command text and parameters.
    /// </summary>
    /// <param name="commandText">The text of the command to be executed.</param>
    /// <param name="commandType">The type of command to be executed (default is Stored Procedure).</param>
    /// <param name="parameters">The parameters of the command to be executed.</param>
    /// <param name="outParameters">The output parameters of the command to be executed.</param>
    /// <returns>An anonymous object that contains the collection of parameters used during the execution of the command and the number of rows affected.</returns>
    public object ExecuteCommand(string commandText,
        CommandType commandType = CommandType.StoredProcedure, SqlParameter[]? parameters = null,
        SqlParameter[]? outParameters = null)
    {
        if (string.IsNullOrEmpty(commandText))
            throw new ArgumentException("Value cannot be null or empty.", nameof(commandText));
        try
        {
            using SqlConnection connection = new(_connectionString);
            using SqlCommand command = new(commandText, connection);
            SetupCommand(command, commandText, commandType, parameters, outParameters);

            connection.Open();

            // execute the command against the database
            int rows = command.ExecuteNonQuery();
            // Return an anonymous object containing the collection of parameters used during the execution and the number of rows affected by the command.
            return new { command.Parameters, rowsAffected = rows };
        }
        catch (Exception e)
        {
            Console.WriteLine("An error occurred in ExecuteCommand: " + e.Message);
            throw;
        }
    }

    /// <summary>
    ///     Asynchronously executes a given command with the provided parameters and returns the collection of output
    ///     parameters if any.
    /// </summary>
    /// <param name="commandText">The text of the command to be executed.</param>
    /// <param name="commandType">The type of command to be executed. Default is stored procedure.</param>
    /// <param name="parameters">An array of parameters to be passed to the command. Optional.</param>
    /// <param name="outParameters">An array of output parameters. Optional.</param>
    /// <returns>An anonymous object that contains the collection of parameters used during the execution of the command and the number of rows affected.</returns>
    public async Task<object> ExecuteCommandAsync(string commandText,
        CommandType commandType = CommandType.StoredProcedure, SqlParameter[]? parameters = null,
        SqlParameter[]? outParameters = null)
    {
        if (string.IsNullOrEmpty(commandText))
            throw new ArgumentException("Value cannot be null or empty.", nameof(commandText));
        try
        {
            using SqlConnection connection = new(_connectionString);
            using SqlCommand command = new(commandText, connection);
            SetupCommand(command, commandText, commandType, parameters, outParameters);

            await connection.OpenAsync();

            // Execute the command asynchronously.
            int rows = await command.ExecuteNonQueryAsync();
            // Return an anonymous object containing the collection of parameters used during the execution and the number of rows affected by the command.
            return new { command.Parameters, rowsAffected = rows };
        }
        catch (Exception e)
        {
            Console.WriteLine("An error occurred in ExecuteCommandAsync: " + e.Message);
            throw;
        }
    }


    /// <summary>
    /// Executes a command against the database using the specified command text and parameters and returns a DataSet containing the results.
    /// </summary>
    /// <param name="commandText">The text of the command to be executed.</param>
    /// <param name="commandType">The type of command to be executed (default is Stored Procedure).</param>
    /// <param name="parameters">The parameters of the command to be executed.</param>
    /// <param name="outParameters">The output parameters of the command to be executed.</param>
    /// <returns>The DataSet containing the results of the command.</returns>
    public DataSet ExecuteCommandDataSet(string commandText,
        CommandType commandType = CommandType.StoredProcedure, SqlParameter[]? parameters = null,
        SqlParameter[]? outParameters = null)
    {
        if (string.IsNullOrEmpty(commandText))
            throw new ArgumentException("Value cannot be null or empty.", nameof(commandText));
        try
        {
            using SqlConnection connection = new(_connectionString);
            using SqlCommand command = new(commandText, connection);
            SetupCommand(command, commandText, commandType, parameters, outParameters);

            connection.Open();

            // Create a new DataSet to store the results of the command
            DataSet result = new();
            // Create a new SqlDataAdapter using the command
            using SqlDataAdapter adapter = new(command);
            // Fill the result DataSet with the results of the command
            adapter.Fill(result);

            // return the DataSet containing the results of the command
            return result;
        }
        catch (Exception e)
        {
            Console.WriteLine("An error occurred in ExecuteCommandDataSet: " + e.Message);
            throw;
        }
    }

    /// <summary>
    ///     Executes the given T-SQL command as a scalar query and returns the result as the specified generic type `T`.
    /// </summary>
    /// <typeparam name="T">The type to convert the scalar result to.</typeparam>
    /// <param name="commandText">The T-SQL command to be executed.</param>
    /// <param name="commandType">The type of the command, either a stored procedure or T-SQL text.</param>
    /// <param name="parameters">An array of parameters to pass to the command, if any.</param>
    /// <param name="outParameters">An array of output parameters to pass to the command, if any.</param>
    /// <returns>An anonymous object that contains the collection of parameters used during the execution and the result of the query, converted to type `T`.</returns>
    public object ExecuteScalar<T>(string commandText,
        CommandType commandType = CommandType.StoredProcedure,
        SqlParameter[]? parameters = null, SqlParameter[]? outParameters = null)
    {
        if (string.IsNullOrEmpty(commandText))
            throw new ArgumentException("Value cannot be null or empty.", nameof(commandText));
        try
        {
            using SqlConnection connection = new(_connectionString);
            using SqlCommand command = new(commandText, connection);
            SetupCommand(command, commandText, commandType, parameters, outParameters);

            // Open the connection asynchronously.
            connection.Open();

            //return an anonymous object that contains the collection of parameters used during the execution and the result of the query, converted to type T
            return new { command.Parameters, result = (T)Convert.ChangeType(command.ExecuteScalar(), typeof(T)) };
        }
        catch (Exception e)
        {
            Console.WriteLine("An error occurred in ExecuteScalar: " + e.Message);
            throw;
        }
    }


    /// <summary>
    ///     Executes a scalar SQL command asynchronously and returns the result as well as the output parameters.
    /// </summary>
    /// <typeparam name="T">The type of the result of the scalar command.</typeparam>
    /// <param name="commandText">The text of the command to be executed.</param>
    /// <param name="commandType">The type of the command to be executed.</param>
    /// <param name="parameters">The parameters of the command, if any.</param>
    /// <param name="outParameters">The output parameters of the command, if any.</param>
    /// <returns>A tuple containing the result of the scalar command and the output parameters.</returns>
    public async Task<object> ExecuteScalarAsync<T>(string commandText,
        CommandType commandType = CommandType.StoredProcedure,
        SqlParameter[]? parameters = null, SqlParameter[]? outParameters = null)
    {
        if (string.IsNullOrEmpty(commandText))
            throw new ArgumentException("Value cannot be null or empty.", nameof(commandText));
        try
        {
            using SqlConnection connection = new(_connectionString);
            using SqlCommand command = new(commandText, connection);
            SetupCommand(command, commandText, commandType, parameters, outParameters);

            // Open the connection asynchronously.
            await connection.OpenAsync();
            //return an anonymous object that contains the collection of parameters used during the execution and the result of the query, converted to type T
            return new
            {
                command.Parameters, result = (T)Convert.ChangeType(await command.ExecuteScalarAsync(), typeof(T))
            };
        }
        catch (Exception e)
        {
            Console.WriteLine("An error occurred in ExecuteScalarAsync: " + e.Message);
            throw;
        }
    }


    private static List<T> MapResultsToModels<T>(IDataReader reader, int selectTop = -1) where T : new()
    {
        // List to store the resulting models
        List<T> models = new();
        // Counter for number of rows read
        int rowCount = 0;
        // Dictionary to store the index of properties in the reader
        Dictionary<string, int>? index = null;

        // Read rows from the reader
        while (reader.Read())
        {
            // Increment row count
            rowCount++;

            // Create a new model instance
            T model = new();
            // If index is not initialized, initialize it with the names and indices of the properties in the reader
            index ??= model.GetType()
                .GetProperties()
                .ToDictionary(propertyInfo => propertyInfo.Name,
                    propertyInfo => reader.GetOrdinal(propertyInfo.Name)
                             , StringComparer.OrdinalIgnoreCase);

            // Loop through the properties in the index
            foreach (KeyValuePair<string, int> keyValuePair in index)
            {
                // Get the property info for the current property
                PropertyInfo property = model.GetType().GetProperty(keyValuePair.Key);
                // If the property exists and the corresponding value in the reader is not null, set the value of the property in the model
                if (property != null && reader[keyValuePair.Value] != DBNull.Value)
                    property.SetValue(model, reader[keyValuePair.Value]);
            }

            // Add the model to the list of models
            models.Add(model);
            // If the number of rows read is equal to selectTop, break out of the loop
            if (rowCount == selectTop) break;
        }

        // Return the list of models
        return models;
    }


    /// <summary>
    ///     Executes a command to retrieve a list of data from the database and maps the results to a list of objects of the
    ///     specified type.
    /// </summary>
    /// <typeparam name="T">The type of the objects in the list to return.</typeparam>
    /// <param name="commandText">The text of the command to execute.</param>
    /// <param name="commandType">The type of the command to execute.</param>
    /// <param name="parameters">The parameters to include with the command.</param>
    /// <param name="selectTop">The maximum number of results to return. Use -1 to return all results.</param>
    /// <returns>A list of objects of the specified type.</returns>
    public List<T> ExecuteReader<T>(string commandText, CommandType commandType = CommandType.StoredProcedure,
        SqlParameter[]? parameters = null, int selectTop = -1) where T : new()
    {
        if (string.IsNullOrEmpty(commandText))
            throw new ArgumentException("Value cannot be null or empty.", nameof(commandText));
        try
        {
            using SqlConnection connection = new(_connectionString);
            using SqlCommand command = new(commandText, connection);
            SetupCommand(command, commandText, commandType, parameters);

            connection.Open();

            // Execute the command and get the data reader
            using SqlDataReader reader = command.ExecuteReader();
            // Map the results to a list of objects of the specified type
            return MapResultsToModels<T>(reader, selectTop);
        }
        catch (Exception e)
        {
            Console.WriteLine("An error occurred in ExecuteReader: " + e.Message);
            throw;
        }
    }

    /// <summary>
    ///     Asynchronously executes a command to retrieve a list of data from the database and maps the results to a list of
    ///     objects of the specified type.
    /// </summary>
    /// <typeparam name="T">The type of the objects in the list to return.</typeparam>
    /// <param name="commandText">The text of the command to execute.</param>
    /// <param name="commandType">The type of the command to execute.</param>
    /// <param name="parameters">The parameters to include with the command.</param>
    /// <param name="selectTop">The maximum number of results to return. Use -1 to return all results.</param>
    /// <returns>A task that represents the asynchronous operation. The task result is a list of objects of the specified type.</returns>
    public async Task<List<T>> ExecuteReaderAsync<T>(string commandText,
        CommandType commandType = CommandType.StoredProcedure, SqlParameter[]? parameters = null, int selectTop = -1)
        where T : new()
    {
        if (string.IsNullOrEmpty(commandText))
            throw new ArgumentException("Value cannot be null or empty.", nameof(commandText));
        try
        {
            using SqlConnection connection = new(_connectionString);
            using SqlCommand command = new(commandText, connection);
            SetupCommand(command, commandText, commandType, parameters);

            await connection.OpenAsync();

            // Execute the command asynchronously and get the data reader
            using SqlDataReader reader = await command.ExecuteReaderAsync();
            // Map the results to a list of objects of the specified type
            return MapResultsToModels<T>(reader, selectTop);
        }
        catch (Exception e)
        {
            Console.WriteLine("An error occurred in ExecuteReaderAsync: " + e.Message);
            throw;
        }
    }
}
