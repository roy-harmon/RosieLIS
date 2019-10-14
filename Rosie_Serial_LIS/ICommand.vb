Imports System.Runtime.CompilerServices
Public Module ICommand
     <Extension()>
     Public Sub AddWithValue(Of T)(ByVal command As IDbCommand, ByVal name As String, ByVal value As T)
          Dim parameter = command.CreateParameter()
          parameter.ParameterName = name
          parameter.Value = value
          command.Parameters.Add(parameter)
     End Sub

     <Extension()>
     Public Sub AddWithValue(Of T)(ByVal command As IDbCommand, ByVal name As String, ByVal value As T, ByVal type As DbType)
          Dim parameter = command.CreateParameter()
          parameter.ParameterName = name
          parameter.DbType = type
          parameter.Value = value
          command.Parameters.Add(parameter)
     End Sub

     <Extension()>
     Public Sub AddParam(ByVal command As IDbCommand, ByVal name As String, ByVal type As DbType)
          Dim parameter = command.CreateParameter()
          parameter.ParameterName = name
          parameter.DbType = type
          command.Parameters.Add(parameter)
     End Sub
End Module
