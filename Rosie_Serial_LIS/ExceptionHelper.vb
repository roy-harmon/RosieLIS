Imports System.Runtime.CompilerServices

Module ExceptionHelper
     <Extension()>
     Function LineNumber(ByVal e As Exception) As Integer
          Dim linenum As Integer = 0

          Try
               linenum = Convert.ToInt32(e.StackTrace.Substring(e.StackTrace.LastIndexOf(" "c)))
          Catch
          End Try

          Return linenum
     End Function
End Module
