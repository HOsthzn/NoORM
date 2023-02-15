Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Reflection
Imports System.Threading.Tasks

Public Class AdoNet

    Private ReadOnly _connectionString As String

    Public Sub New(connectionString As String)
        ' Create a SqlConnectionStringBuilder object from the provided connection string
        Dim builder As New SqlConnectionStringBuilder(connectionString) With {
        ' Set the "Pooling" property of the SqlConnectionStringBuilder to true
        .Pooling = True
        }

        ' Update the connection string to reflect the changes made to the SqlConnectionStringBuilder
        _connectionString = builder.ConnectionString
    End Sub

    ' This function sets up a SqlCommand object with the given command text, type, and parameters.
    Private Shared Sub SetupCommand(command As SqlCommand, commandText As String, commandType As CommandType,
    Optional parameters As SqlParameter() = Nothing)

        ' Set the command text for the SqlCommand object
        command.CommandText = commandText

        ' Set the command type for the SqlCommand object
        command.CommandType = commandType

        ' If parameters are passed, add them to the SqlCommand object's parameters collection
        If parameters IsNot Nothing Then command.Parameters.AddRange(parameters)

    End Sub

    ''' <summary>
    ''' Executes a command against the database using the specified command text and parameters.
    ''' </summary>
    ''' <param name="commandText">The text of the command to be executed.</param>
    ''' <param name="commandType">The type of command to be executed (default is Stored Procedure).</param>
    ''' <param name="parameters">The parameters of the command to be executed.</param>
    ''' <returns>An anonymous object that contains the collection of parameters used during the execution of the command and the number of rows affected.</returns>
    Public Function ExecuteCommand(commandText As String,
    Optional commandType As CommandType = CommandType.StoredProcedure,
    Optional parameters As SqlParameter() = Nothing) As Object
        If String.IsNullOrEmpty(commandText) Then
            Throw New ArgumentException("Value cannot be null or empty.", NameOf(commandText))
        End If
        Try
            Using connection As New SqlConnection(_connectionString)
                Using command As New SqlCommand(commandText, connection)
                    SetupCommand(command, commandText, commandType, parameters)

                    connection.Open()

                    ' execute the command against the database
                    Dim rows As Integer = command.ExecuteNonQuery()
                    ' Return an anonymous object containing the collection of parameters used during the execution and the number of rows affected by the command.
                    Return New With {command.Parameters, .rowsAffected = rows}
                End Using
            End Using
        Catch e As Exception
            Console.WriteLine("An error occurred in ExecuteCommand: " & e.Message)
            Throw
        End Try
    End Function

    '''<summary>
    '''    Asynchronously executes a given command with the provided parameters And returns the collection of output
    '''    parameters if any.
    '''</summary>
    '''<param name="commandText">The text of the command to be executed.</param>
    '''<param name="commandType">The type of command to be executed. Default Is stored procedure.</param>
    '''<param name="parameters">An array of parameters to be passed to the command. Optional.</param>
    '''<returns>An anonymous object that contains the collection of parameters used during the execution of the command And the number of rows affected.</returns>
    Public Async Function ExecuteCommandAsync(ByVal commandText As String,
        Optional ByVal commandType As CommandType = CommandType.StoredProcedure, Optional ByVal parameters As SqlParameter() = Nothing) As Task(Of Object)
        If String.IsNullOrEmpty(commandText) Then
            Throw New ArgumentException("Value cannot be null or empty.", NameOf(commandText))
        End If
        Try
            Using connection As New SqlConnection(_connectionString)
                Using command As New SqlCommand(commandText, connection)
                    SetupCommand(command, commandText, commandType, parameters)

                    Await connection.OpenAsync()

                    ' Execute the command asynchronously.
                    Dim rows As Integer = Await command.ExecuteNonQueryAsync()
                    ' Return an anonymous object containing the collection of parameters used during the execution and the number of rows affected by the command.
                    Return New With {command.Parameters, .rowsAffected = rows}
                End Using
            End Using
        Catch e As Exception
            Console.WriteLine("An error occurred in ExecuteCommandAsync: " & e.Message)
            Throw
        End Try
    End Function

    ''' <summary>
    ''' Executes a command against the database using the specified command text and parameters and returns a DataSet containing the results.
    ''' </summary>
    ''' <param name="commandText">The text of the command to be executed.</param>
    ''' <param name="commandType">The type of command to be executed (default is Stored Procedure).</param>
    ''' <param name="parameters">The parameters of the command to be executed.</param>
    ''' <returns>The DataSet containing the results of the command.</returns>
    Public Function ExecuteCommandDataSet(commandText As String,
    Optional commandType As CommandType = CommandType.StoredProcedure,
    Optional parameters As SqlParameter() = Nothing) As DataSet
        If String.IsNullOrEmpty(commandText) Then
            Throw New ArgumentException("Value cannot be null or empty.", NameOf(commandText))
        End If

        Try
            Using connection As New SqlConnection(_connectionString)
                Using command As New SqlCommand(commandText, connection)
                    SetupCommand(command, commandText, commandType, parameters)

                    connection.Open()

                    ' Create a new DataSet to store the results of the command
                    Dim result As New DataSet()
                    ' Create a new SqlDataAdapter using the command
                    Using adapter As New SqlDataAdapter(command)
                        ' Fill the result DataSet with the results of the command
                        adapter.Fill(result)

                        ' return the DataSet containing the results of the command
                        Return result
                    End Using
                End Using
            End Using
        Catch e As Exception
            Console.WriteLine("An error occurred in ExecuteCommandDataSet: " & e.Message)
            Throw
        End Try
    End Function

    '''<summary>
    '''    Executes the given T-SQL command as a scalar query And returns the result as the specified generic type `T`.
    '''</summary>
    '''<typeparam name="T">The type to convert the scalar result to.</typeparam>
    '''<param name="commandText">The T-SQL command to be executed.</param>
    '''<param name="commandType">The type of the command, either a stored procedure Or T-SQL text.</param>
    '''<param name="parameters">An array of parameters to pass to the command, if any.</param>
    '''<returns>An anonymous object that contains the collection of parameters used during the execution And the result of the query, converted to type `T`.</returns>
    Public Function ExecuteScalar(Of T)(ByVal commandText As String,
        Optional ByVal commandType As CommandType = CommandType.StoredProcedure,
        Optional ByVal parameters As SqlParameter() = Nothing) As Object
        If String.IsNullOrEmpty(commandText) Then
            Throw New ArgumentException("Value cannot be null or empty.", NameOf(commandText))
        End If

        Try
            Using connection As New SqlConnection(_connectionString)
                Using command As New SqlCommand(commandText, connection)
                    SetupCommand(command, commandText, commandType, parameters)

                    ' Open the connection asynchronously.
                    connection.Open()

                    ' Return an anonymous object that contains the collection of parameters used during the execution and the result of the query, converted to type T
                    Return New With {command.Parameters, .result = CType(Convert.ChangeType(command.ExecuteScalar(), GetType(T)), T)}
                End Using
            End Using
        Catch e As Exception
            Console.WriteLine("An error occurred in ExecuteScalar: " & e.Message)
            Throw
        End Try
    End Function

    ''' <summary>
    ''' Executes a scalar SQL command asynchronously and returns the result as well as the output parameters.
    ''' </summary>
    ''' <typeparam name="T">The type of the result of the scalar command.</typeparam>
    ''' <param name="commandText">The text of the command to be executed.</param>
    ''' <param name="commandType">The type of the command to be executed.</param>
    ''' <param name="parameters">The parameters of the command, if any.</param>
    ''' <returns>A tuple containing the result of the scalar command and the output parameters.</returns>
    Public Async Function ExecuteScalarAsync(Of T)(commandText As String,
    Optional commandType As CommandType = CommandType.StoredProcedure,
    Optional parameters As SqlParameter() = Nothing) As Task(Of Object)
        If String.IsNullOrEmpty(commandText) Then
            Throw New ArgumentException("Value cannot be null or empty.", NameOf(commandText))
        End If
        Try
            Using connection As New SqlConnection(_connectionString)
                Using command As New SqlCommand(commandText, connection)
                    SetupCommand(command, commandText, commandType, parameters)
                    ' Open the connection asynchronously.
                    Await connection.OpenAsync()
                    'return an anonymous object that contains the collection of parameters used during the execution and the result of the query, converted to type T
                    Return New With {
                    command.Parameters,
                    .result = DirectCast(Convert.ChangeType(Await command.ExecuteScalarAsync(), GetType(T)), T)
                    }
                End Using
            End Using
        Catch e As Exception
            Console.WriteLine("An error occurred in ExecuteScalarAsync: " & e.Message)
            Throw
        End Try
    End Function

    Private Shared Function MapResultsToModels(Of T As {New, Class})(reader As IDataReader, Optional selectTop As Integer = -1) As List(Of T)
        ' List to store the resulting models
        Dim models As New List(Of T)
        ' Counter for number of rows read
        Dim rowCount As Integer = 0
        ' Dictionary to store the index of properties in the reader
        Dim index As Dictionary(Of String, Integer) = Nothing

        ' Read rows from the reader
        While reader.Read()
            ' Increment row count
            rowCount += 1

            ' Create a new model instance
            Dim model As New T
            ' If index is not initialized, initialize it with the names and indices of the properties in the reader
            If index Is Nothing Then
                index = model.GetType().GetProperties().ToDictionary(Function(propertyInfo) propertyInfo.Name, Function(propertyInfo) reader.GetOrdinal(propertyInfo.Name), StringComparer.OrdinalIgnoreCase)
            End If

            ' Loop through the properties in the index
            For Each keyValuePair As KeyValuePair(Of String, Integer) In index
                ' Get the property info for the current property
                Dim property As PropertyInfo = model.GetType().GetProperty(keyValuePair.Key)
                ' If the property exists and the corresponding value in the reader is not null, set the value of the property in the model
                If property IsNot Nothing AndAlso reader(keyValuePair.Value) IsNot DBNull.Value Then
                Property.SetValue(model, reader(keyValuePair.Value))
            End If
    Next

        ' Add the model to the list of models
        models.Add(model)
        ' If the number of rows read is equal to selectTop, break out of the loop
        If rowCount = selectTop Then Exit While
    End While

    ' Return the list of models
    Return models
    End Function

    ''' <summary>
    ''' Executes a command to retrieve a list of data from the database and maps the results to a list of objects of the specified type.
    ''' </summary>
    ''' <typeparam name="T">The type of the objects in the list to return.</typeparam>
    ''' <param name="commandText">The text of the command to execute.</param>
    ''' <param name="commandType">The type of the command to execute.</param>
    ''' <param name="parameters">The parameters to include with the command.</param>
    ''' <param name="selectTop">The maximum number of results to return. Use -1 to return all results.</param>
    ''' <returns>A list of objects of the specified type.</returns>
    Public Function ExecuteReader(Of T As New)(commandText As String, Optional commandType As CommandType = CommandType.StoredProcedure, Optional parameters As SqlParameter() = Nothing, Optional selectTop As Integer = -1) As List(Of T)
        If String.IsNullOrEmpty(commandText) Then
            Throw New ArgumentException("Value cannot be null or empty.", NameOf(commandText))
        End If

        Try
            Using connection As New SqlConnection(_connectionString)
                Using command As New SqlCommand(commandText, connection)
                    SetupCommand(command, commandText, commandType, parameters)

                    connection.Open()

                    ' Execute the command and get the data reader
                    Using reader As SqlDataReader = command.ExecuteReader()
                        ' Map the results to a list of objects of the specified type
                        Return MapResultsToModels(Of T)(reader, selectTop)
                    End Using
                End Using
            End Using
        Catch e As Exception
            Console.WriteLine("An error occurred in ExecuteReader: " & e.Message)
            Throw
        End Try
    End Function

    ''' <summary>
    ''' Asynchronously executes a command to retrieve a list of data from the database and maps the results to a list of objects of the specified type.
    ''' </summary>
    ''' <typeparam name="T">The type of the objects in the list to return.</typeparam>
    ''' <param name="commandText">The text of the command to execute.</param>
    ''' <param name="commandType">The type of the command to execute.</param>
    ''' <param name="parameters">The parameters to include with the command.</param>
    ''' <param name="selectTop">The maximum number of results to return. Use -1 to return all results.</param>
    ''' <returns>A task that represents the asynchronous operation. The task result is a list of objects of the specified type.</returns>
    Public Async Function ExecuteReaderAsync(Of T As New)(ByVal commandText As String, Optional ByVal commandType As CommandType = CommandType.StoredProcedure, Optional ByVal parameters As SqlParameter() = Nothing, Optional ByVal selectTop As Integer = -1) As Task(Of List(Of T))
        If String.IsNullOrEmpty(commandText) Then
            Throw New ArgumentException("Value cannot be null or empty.", NameOf(commandText))
        End If
        Try
            Using connection As New SqlConnection(_connectionString)
                Using command As New SqlCommand(commandText, connection)
                    SetupCommand(command, commandText, commandType, parameters)

                    Await connection.OpenAsync()

                    ' Execute the command asynchronously and get the data reader
                    Using reader As SqlDataReader = Await command.ExecuteReaderAsync()
                        ' Map the results to a list of objects of the specified type
                        Return MapResultsToModels(Of T)(reader, selectTop)
                    End Using
                End Using
            End Using
        Catch e As Exception
            Console.WriteLine("An error occurred in ExecuteReaderAsync: " & e.Message)
            Throw
        End Try
    End Function

End Class
