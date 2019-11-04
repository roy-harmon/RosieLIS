Imports System.Runtime.CompilerServices

Module ExceptionHelper
     <Extension()>
     Function LineNumber(ByVal e As Exception) As Integer
          Static ci As Globalization.CultureInfo = Globalization.CultureInfo.GetCultureInfo("en-US")
          Dim linenum As Integer = Convert.ToInt32(e.StackTrace.Substring(e.StackTrace.LastIndexOf(" "c)), ci.NumberFormat)

          Return linenum
     End Function
End Module
