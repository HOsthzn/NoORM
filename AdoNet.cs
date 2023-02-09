using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace RockEngineeringMVC.Utilities;

public class AdoNet
{
    private readonly string _connectionString;

    public AdoNet(string connectionString)
    {
        _connectionString = connectionString;
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
    ///     Executes a command against the database using the specified command text and parameters.
    /// </summary>
    /// <param name="commandText">The text of the command to be executed.</param>
    /// <param name="commandType">The type of command to be executed (default is Stored Procedure).</param>
    /// <param name="parameters">The parameters of the command to be executed.</param>
    /// <param name="outParameters">The output parameters of the command to be executed.</param>
    /// <returns>The collection of parameters that were used during the execution of the command.</returns>
    public SqlParameterCollection ExecuteCommand(string commandText,
        CommandType commandType = CommandType.StoredProcedure, SqlParameter[]? parameters = null,
        SqlParameter[]? outParameters = null)
    {
        using SqlConnection connection = new(_connectionString); // create a new connection using the connection string
        using SqlCommand
            command = new(commandText, connection); // create a new command using the command text and connection
        SetupCommand(command, commandText, commandType, parameters,
            outParameters); // set up the command with the specified parameters and output parameters

        connection.Open(); // open the connection to the database
        command.ExecuteNonQuery(); // execute the command against the database

        return command.Parameters; // return the collection of parameters used during the execution of the command
    }

    /// <summary>
    ///     Asynchronously executes a given command with the provided parameters and returns the collection of output
    ///     parameters if any.
    /// </summary>
    /// <param name="commandText">The text of the command to be executed.</param>
    /// <param name="commandType">The type of command to be executed. Default is stored procedure.</param>
    /// <param name="parameters">An array of parameters to be passed to the command. Optional.</param>
    /// <param name="outParameters">An array of output parameters. Optional.</param>
    /// <returns>A collection of output parameters, if any.</returns>
    public async Task<SqlParameterCollection> ExecuteCommandAsync(string commandText,
        CommandType commandType = CommandType.StoredProcedure, SqlParameter[]? parameters = null,
        SqlParameter[]? outParameters = null)
    {
        using SqlConnection connection = new(_connectionString);
        using SqlCommand command = new(commandText, connection);
        SetupCommand(command, commandText, commandType, parameters, outParameters);

        // Open the connection asynchronously.
        await connection.OpenAsync();

        // Execute the command asynchronously.
        await command.ExecuteNonQueryAsync();

        // Return the collection of output parameters.
        return command.Parameters;
    }


    /// <summary>
    ///     Executes the given T-SQL command as a scalar query and returns the result as the specified generic type `T`.
    /// </summary>
    /// <typeparam name="T">The type to convert the scalar result to.</typeparam>
    /// <param name="commandText">The T-SQL command to be executed.</param>
    /// <param name="commandType">The type of the command, either a stored procedure or T-SQL text.</param>
    /// <param name="parameters">An array of parameters to pass to the command, if any.</param>
    /// <param name="outParameters">An array of output parameters to pass to the command, if any.</param>
    /// <returns>A tuple containing the scalar result as type `T` and the collection of SQL parameters.</returns>
    public Tuple<T, SqlParameterCollection> ExecuteScalar<T>(string commandText,
        CommandType commandType = CommandType.StoredProcedure,
        SqlParameter[]? parameters = null, SqlParameter[]? outParameters = null)
    {
        using SqlConnection connection = new(_connectionString);
        using SqlCommand command = new(commandText, connection);
        SetupCommand(command, commandText, commandType, parameters, outParameters);

        // Open the connection asynchronously.
        connection.Open();

        // Execute the scalar command and convert the result to the specified type.
        // Then, return a tuple containing the result and the output parameters.
        return new Tuple<T, SqlParameterCollection>(
            (T)Convert.ChangeType(command.ExecuteScalar(), typeof(T)),
            command.Parameters);
    }


    /// <summary>
    /// Executes a scalar SQL command asynchronously and returns the result as well as the output parameters.
    /// </summary>
    /// <typeparam name="T">The type of the result of the scalar command.</typeparam>
    /// <param name="commandText">The text of the command to be executed.</param>
    /// <param name="commandType">The type of the command to be executed.</param>
    /// <param name="parameters">The parameters of the command, if any.</param>
    /// <param name="outParameters">The output parameters of the command, if any.</param>
    /// <returns>A tuple containing the result of the scalar command and the output parameters.</returns>
    public async Task<Tuple<T, SqlParameterCollection>> ExecuteScalarAsync<T>(string commandText,
        CommandType commandType = CommandType.StoredProcedure,
        SqlParameter[]? parameters = null, SqlParameter[]? outParameters = null)
    {
        using SqlConnection connection = new(_connectionString);
        using SqlCommand command = new(commandText, connection);
        SetupCommand(command, commandText, commandType, parameters, outParameters);

        // Open the connection asynchronously.
        await connection.OpenAsync();

        // Execute the scalar command asynchronously and convert the result to the specified type.
        // Then, return a tuple containing the result and the output parameters.
        return new Tuple<T, SqlParameterCollection>(
            (T)Convert.ChangeType(await command.ExecuteScalarAsync(), typeof(T)), command.Parameters);
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
                    propertyInfo => reader.GetOrdinal(propertyInfo.Name));

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
    /// Executes a command to retrieve a list of data from the database and maps the results to a list of objects of the specified type.
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
        using SqlConnection connection = new(_connectionString);
        using SqlCommand command = new(commandText, connection);
        SetupCommand(command, commandText, commandType, parameters);
        
        connection.Open();

        // Execute the command and get the data reader
        using SqlDataReader reader = command.ExecuteReader();
        // Map the results to a list of objects of the specified type
        return MapResultsToModels<T>(reader, selectTop);
    }

    /// <summary>
    /// Asynchronously executes a command to retrieve a list of data from the database and maps the results to a list of objects of the specified type.
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
        using SqlConnection connection = new(_connectionString);
        using SqlCommand command = new(commandText, connection);
        SetupCommand(command, commandText, commandType, parameters);
        
        await connection.OpenAsync();

        // Execute the command asynchronously and get the data reader
        using SqlDataReader reader = await command.ExecuteReaderAsync();
        // Map the results to a list of objects of the specified type
        return MapResultsToModels<T>(reader, selectTop);
    }
}
