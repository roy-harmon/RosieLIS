Imports RosieService.ICommand
Imports System.Data.Common
Imports System.IO
Imports System.IO.Ports



Public Class ServRosie

     Public WithEvents Com1 As SerialPort = Nothing
     'Public cnSQL As IDbConnection ' = GetConnected() ' SqlConnection()
     Public intTemp As Integer
     'Public Enum SampleFields
     '    PatientName = 1
     '    SampleNo = 2
     '    LastName = 3
     '    Test1 = 4
     '    Test2 = 5
     '    Test3 = 6
     '    Test4 = 7
     '    Test5 = 8
     '    Test6 = 9
     '    PendingSending = 10
     '    ToDelete = 11
     '    Temp_ID = 12
     '    RejectCode = 13
     '    intPriority = 14
     '    DilFactor = 15
     'End Enum


     Protected Overrides Sub OnStart(ByVal args() As String)
          ' Add code here to start your service. This method should set things
          ' in motion so your service can do its work.
          'Com1 = My.Computer.Ports.OpenSerialPort("COM1", 19200, Parity.None, 8, 1)
          'Com1 = My.Computer.Ports.OpenSerialPort(My.Settings.portName, My.Settings.baudRate, My.Settings.parity, My.Settings.dataBits, My.Settings.stopBits)
          'Com1.Handshake = My.Settings.handShake 'Handshake.None
          'Com1.NewLine = Chr(3)
          SerialOpen()
          'If cnSQL Is Nothing Then cnSQL = GetConnected()
          'cnSQL.ConnectionString = GetConnectionString()
          Timer1.Start()
     End Sub

     Sub SerialOpen()
          Com1 = My.Computer.Ports.OpenSerialPort(My.Settings.portName, My.Settings.baudRate, My.Settings.parity, My.Settings.dataBits, My.Settings.stopBits)
          With Com1
               .Handshake = My.Settings.handShake 'Handshake.None
               .NewLine = Chr(3)
               .Open()
          End With
     End Sub
     Function GetConnected() As IDbConnection
          Dim cnSQL As IDbConnection
          Select Case My.Settings.databaseType
               Case "SQL Server"
                    cnSQL = New SqlClient.SqlConnection()
               'Case "Oracle"
               '     cnSQL = New Data.OracleClient.OracleConnection()
               Case "MySQL"
                    cnSQL = New MySql.Data.MySqlClient.MySqlConnection()
               Case "ODBC"
                    cnSQL = New Odbc.OdbcConnection()
               Case Else
                    cnSQL = New OleDb.OleDbConnection()
          End Select
          cnSQL.ConnectionString = My.Settings.connectionString
          cnSQL.Open()
          Return cnSQL
     End Function
     Protected Overrides Sub OnStop()
          ' Definitely want to close the COM port when we're done.
          If Com1 IsNot Nothing Then
               Com1.Close()
               Com1.Dispose()
               Com1 = Nothing
          End If
          ' And stop the timer.
          Timer1.Stop()
     End Sub

     'Protected Overrides Sub OnPause()
     '     MyBase.OnPause()
     '     cnSQL.Dispose()
     '     Com1.Dispose()
     'End Sub

     'Protected Overrides Sub OnContinue()
     '     MyBase.OnContinue()
     '     If cnSQL Is Nothing Then cnSQL = GetConnected()
     '     SerialOpen()
     'End Sub

     Private Sub Timer1_Tick(sender As Object, e As EventArgs) Handles Timer1.Tick
          ' Just to make sure the COM port data is being read.
          If Com1.BytesToRead > 0 Then ReceiveSerialData()
     End Sub

     Private Sub Com1_DataReceived(ByVal sender As Object, ByVal e As SerialDataReceivedEventArgs) Handles Com1.DataReceived
          ReceiveSerialData()
     End Sub

     Sub SendCommData(ByVal strData As String)
          ' Send strings to a serial port.
          ' Properties should already be set.
          If Not Com1.IsOpen Then
               Com1.Open()
          End If
          Com1.Write(strData)
     End Sub

     Sub ReceiveSerialData()
          ' Receive strings from a serial port.
          ' Properties should already be set.
          Dim strCheck As String = "", strData As String = ""
          Dim strTrans As String = ""
          Dim Incoming As String = ""

          If Com1.BytesToRead = 0 Then Exit Sub

          Do
               Incoming = Chr(Com1.ReadByte)
               If Not Incoming = Chr(3) Then
                    If Not Incoming = Chr(6) Then strTrans &= Incoming
               End If
          Loop Until Incoming = Chr(3)

          If strTrans.Substring(0, 1) = Chr(2) Then strTrans = strTrans.Substring(1)

          If Len(strTrans) > 2 Then
               ' Check the checksum.
               strCheck = Microsoft.VisualBasic.Left(strTrans, Len(strTrans) - 2)
               Debug.Print(CHKSum(strCheck))
               strData = Microsoft.VisualBasic.Right(Microsoft.VisualBasic.Left(strTrans, Len(strTrans) - 2), Len(strTrans) - 1)
               If CHKSum(strCheck) = Microsoft.VisualBasic.Right(strTrans, 2) Then
                    ' Acknowledge receipt (ACK to serial port).
                    SendCommData(Chr(6))
                    ' Log it.
                    AppendToLog("In: " & strTrans)
                    ' Handle the incoming data.
                    IncomingData(strData)
               Else
                    ' There was an error in the transmission.
                    ' Send NAK to serial port. The remote client will attempt to send again.
                    SendCommData(Chr(21))
                    ' Log it.
                    AppendToLog("In: " & strTrans)
               End If
          End If
     End Sub

     Function CHKSum(strString) As String
          ' This function returns the checksum for the data string passed to it.
          ' If I've done it right, the checksum is calculated by binary 8-bit addition of all included characters
          ' with the 8th or parity bit assumed zero. Carries beyond the 8th bit are lost. The 8-bit result is
          ' converted into two printable ASCII Hex characters ranging from 00 to FF, which are then inserted into
          ' the data stream. Hex alpha characters are always uppercase.

          Dim strTemp As String
          Dim strData As String
          Dim ascSum As Integer
          Dim modVal As Integer
          Dim checkSum As String

          strData = Trim(strString)

          While Len(strData)
               strTemp = Microsoft.VisualBasic.Left(strData, 1)
               If Trim(strTemp) <> Chr(2) Then
                    ascSum += Asc(strTemp)
               End If
               strData = Microsoft.VisualBasic.Right(strData, Len(strData) - 1)
          End While

          modVal = ascSum Mod 256
          checkSum = Hex(modVal)

          If Len(checkSum) = 2 Then
               CHKSum = checkSum
          Else
               CHKSum = "0" & checkSum
          End If

     End Function

     Function ResultAcceptance() As String
          ' This function returns a string indicating to the instrument that the result has been accepted by the computer.
          Dim strHex As String
          strHex = Chr(2) & "M" & Chr(28) & "A" & Chr(28) & Chr(28)
          strHex &= CHKSum(strHex) & Chr(3)
          ResultAcceptance = strHex

     End Function

     Sub IncomingData(strData As String)

          Dim strType As String, strOut As String
          Dim varRes() As String, intCount As Integer, intID As Integer
          Dim rstList As Object, intRes As Integer
          Const PatientName = 1
          Const SampleNo = 2
          'Const LastName = 3
          Const Test1 = 4
          Const Test2 = 5
          Const Test3 = 6
          Const Test4 = 7
          Const Test5 = 8
          Const Test6 = 9
          Const SampleType = 10
          Const ToDelete = 11
          Const Temp_ID = 12
          'Const RejectCode = 13
          Const intPriority = 14
          Const DilFactor = 15
          'Const PendingSending = 16

          Try
               strType = Mid(strData, 1, 1)
               rstList = Nothing
               Select Case strType
                    Case "P", "I"
                         ' strData is a Poll message or Query Message.
                         ' Send either a NoRequestMessage or a SampleRequestMessage.
                         Using cnSQL As IDbConnection = GetConnected()
                              Using command As DbCommand = cnSQL.CreateCommand()
                                   If strType = "P" Then
                                        ' Poll message. If there are any pending sample requests that haven't been sent, send one now.
                                        command.CommandText = "SELECT * FROM [TempPendings] WHERE [PendingSending]='TRUE'"
                                   ElseIf strType = "I" Then
                                        ' Query message. This likely requires a different database setup to be useful. 
                                        ' For now, just send any pending sample requests for the sample in question.
                                        varRes = Split(strData, Chr(28), , CompareMethod.Binary)
                                        command.CommandText = "SELECT * FROM [TempPendings] WHERE [PendingSending]='TRUE' AND SampleNo = @SampleNo"
                                        command.AddWithValue("@SampleNo", varRes(1))
                                   End If
                                   ' Note that the command may return more than one row. The only downside is marginally increased network traffic.
                                   ' Since some data sources (Microsoft) use "SELECT TOP 1 ..." and others use "SELECT ... LIMIT 1" 
                                   ' the omission of either statement is the simplest way to ensure compatibility. 
                                   ' However, only one row can be sent at a time because the instrument needs to respond with: 
                                   ' 1. an acknowledgement, 2. a request acceptance message, and 3. another poll or query message, which puts us back here.
                                   Using dr As DbDataReader = command.ExecuteReader
                                        If dr.Read() Then
                                             ' If there's at least one row, send one row. 
                                             Dim strArray(dr.FieldCount - 1)
                                             For i = 0 To dr.FieldCount - 1
                                                  strArray(i) = Nz(dr.Item(i))
                                             Next
                                             rstList = strArray
                                        End If
                                   End Using
                              End Using
                         End Using
                         If IsNothing(rstList(0)) Then
                              SendCommData(NoRequestMessage)
                              AppendToLog("Out: " & NoRequestMessage())
                         Else
                              Dim strTests() As String = Nothing
                              Dim intTests As Integer, strSampleType As String, intDil As Integer = 1
                              intTests = 6
                              If rstList(Test6) = "" Then intTests = 5
                              If rstList(Test5) = "" Then intTests = 4
                              If rstList(Test4) = "" Then intTests = 3
                              If rstList(Test3) = "" Then intTests = 2
                              If rstList(Test2) = "" Then intTests = 1
                              strSampleType = 1
                              'If Microsoft.VisualBasic.Left(rstList(Test1), 3) = "CSA" Then strSampleType = "W" ' If there's a better way to identify whole-blood tests, do it here.
                              strSampleType = rstList(SampleType)
                              Select Case intTests
                                   Case 1
                                        ReDim strTests(0)
                                        strTests(0) = rstList(Test1)
                                   Case 2
                                        ReDim strTests(1)
                                        strTests(0) = rstList(Test1)
                                        strTests(1) = rstList(Test2)
                                   Case 3
                                        ReDim strTests(2)
                                        strTests(0) = rstList(Test1)
                                        strTests(1) = rstList(Test2)
                                        strTests(2) = rstList(Test3)
                                   Case 4
                                        ReDim strTests(3)
                                        strTests(0) = rstList(Test1)
                                        strTests(1) = rstList(Test2)
                                        strTests(2) = rstList(Test3)
                                        strTests(3) = rstList(Test4)
                                   Case 5
                                        ReDim strTests(4)
                                        strTests(0) = rstList(Test1)
                                        strTests(1) = rstList(Test2)
                                        strTests(2) = rstList(Test3)
                                        strTests(3) = rstList(Test4)
                                        strTests(4) = rstList(Test5)
                                   Case 6
                                        ReDim strTests(5)
                                        strTests(0) = rstList(Test1)
                                        strTests(1) = rstList(Test2)
                                        strTests(2) = rstList(Test3)
                                        strTests(3) = rstList(Test4)
                                        strTests(4) = rstList(Test5)
                                        strTests(5) = rstList(Test6)
                              End Select
                              If Nz(rstList(DilFactor), 0) > 1 Then intDil = rstList(DilFactor)
                              strOut = SampleRequestMessage(rstList(ToDelete), rstList(PatientName), rstList(SampleNo), strSampleType, rstList(intPriority), strTests, intDil)
                              SendCommData(strOut)
                              intTemp = rstList(Temp_ID)
                              AppendToLog("Out: " & strOut)
                         End If
                         rstList = Nothing
                    Case "M"
                         ' strData is a Request Acceptance message.
                         ' strData contains sample position information, but it doesn't seem terribly useful.
                         ' It's basically just "42". We can maybe use it to differentiate accepted requests?
                         ' But at least now we can deal with rejected requests! Huzzah!
                         varRes = Split(strData, Chr(28), , vbBinaryCompare)
                         Using cnSQL As IDbConnection = GetConnected()
                              Using update As DbCommand = cnSQL.CreateCommand()
                                   With update
                                        Select Case varRes(1)
                                             Case "R"
                                                  ' Request rejected. Reason for rejection is stored in the table. Remember to work that into the front-end.
                                                  .CommandText = "UPDATE TempPendings SET TempPendings.Position = @Position, TempPendings.PendingSending = 'FALSE', TempPendings.RejectCode = @RejectCode WHERE TempPendings.Temp_ID = @intTemp"
                                                  .AddWithValue("@Position", varRes(5))
                                                  .AddWithValue("@RejectCode", varRes(2))
                                                  .AddWithValue("@intTemp", intTemp)
                                             Case "A"
                                                  ' Request accepted. Set the "position" and clear the PendingSending flag.
                                                  .CommandText = "UPDATE TempPendings SET TempPendings.Position = @Position, TempPendings.PendingSending = 'FALSE', TempPendings.ToDelete = 'FALSE' WHERE TempPendings.Temp_ID = @intTemp"
                                                  .AddWithValue("@Position", varRes(5))
                                                  .AddWithValue("@intTemp", intTemp)
                                        End Select
                                        .ExecuteNonQuery()
                                   End With
                              End Using
                         End Using
                         AppendToLog("In: " & strData)
                         rstList = Nothing
                    Case "R"
                         ' strData is a Result message.
                         ' Send a ResultAcceptance message, either accepting or rejecting it. Most likely accepting.
                         varRes = Split(strData, Chr(28), , vbBinaryCompare)
                         Dim strSQL As String = "INSERT INTO ResSamples ( Patient_ID, Sample_No, SampleType, Location, Priority, [DateTime], Cups, Dilution, TestsCount ) VALUES (@Patient_ID, @Sample_No, @SampleType, @Location, @Priority, @DateTime, @Cups, @Dilution, @TestsCount)"
                         Dim strSQL1 As String = "INSERT INTO ResSamples ( "
                         Dim strSQL2 As String = " ) VALUES ( "
                         Dim objID As Object
                         Using cnSQL As IDbConnection = GetConnected()
                              Using insert As DbCommand = cnSQL.CreateCommand()
                                   If Len(varRes(2)) > 0 Then
                                        insert.AddWithValue("@Patient_ID", varRes(2), DbType.String)
                                        strSQL1 &= "Patient_ID, "
                                        strSQL2 &= "@Patient_ID, "
                                   End If
                                   insert.AddWithValue("@Sample_No", varRes(3), DbType.String)
                                   strSQL1 &= "Sample_No"
                                   strSQL2 &= "@Sample_No"
                                   If Len(varRes(4)) > 0 Then
                                        insert.AddWithValue("@SampleType", varRes(4), DbType.String)
                                        strSQL1 &= ", SampleType"
                                        strSQL2 &= ", @SampleType"
                                   End If
                                   If Len(varRes(5)) > 0 Then
                                        insert.AddWithValue("@Location", varRes(5), DbType.String)
                                        strSQL1 &= ", Location"
                                        strSQL2 &= ", @Location"
                                   End If
                                   If Len(varRes(6)) > 0 Then
                                        insert.AddWithValue("@Priority", varRes(6), DbType.String)
                                        strSQL1 &= ", Priority"
                                        strSQL2 &= ", @Priority"
                                   End If
                                   If Len(varRes(7)) > 0 Then
                                        insert.AddWithValue("@DateTime", varRes(7), DbType.String)
                                        strSQL1 &= ", [DateTime]"
                                        strSQL2 &= ", @DateTime"
                                   End If
                                   If Len(varRes(8)) > 0 Then
                                        insert.AddWithValue("@Cups", varRes(8), DbType.String)
                                        strSQL1 &= ", Cups"
                                        strSQL2 &= ", @Cups"
                                   End If
                                   If Len(varRes(9)) > 0 Then
                                        insert.AddWithValue("@Dilution", varRes(9), DbType.String)
                                        strSQL1 &= ", Dilution"
                                        strSQL2 &= ", @Dilution"
                                   End If
                                   If Len(varRes(10)) > 0 Then
                                        insert.AddWithValue("@TestsCount", varRes(10), DbType.String)
                                        strSQL1 &= ", TestsCount"
                                        strSQL2 &= ", @TestsCount"
                                   End If
                                   For Each param As DbParameter In insert.Parameters
                                        If Len(param.Value) > 0 Then param.Size = Len(param.Value)
                                   Next
                                   insert.CommandText = strSQL1 & strSQL2 & " )"
                                   insert.Prepare()
                                   intRes = insert.ExecuteNonQuery()
                              End Using
                              Using getNew As DbCommand = cnSQL.CreateCommand()
                                   getNew.CommandText = "SELECT Max(ResSamples.ID) AS MaxOfID FROM ResSamples"
                                   objID = getNew.ExecuteScalar()
                                   intID = CInt(objID)
                              End Using
                              Dim str1 As String = "INSERT INTO SampleResults ( Sample_ID, TestName"
                              Dim str2 As String = " ) VALUES ( @Sample_ID, @TestName"
                              Do Until intCount = varRes(10)
                                   Using insert As DbCommand = cnSQL.CreateCommand()
                                        insert.AddWithValue("@Sample_ID", intID, DbType.Int32)
                                        insert.AddWithValue("@TestName", varRes(11 + intCount * 4), DbType.String)
                                        If Len(varRes(12 + intCount * 4)) > 0 Then
                                             str1 &= ", Result"
                                             str2 &= ", @Result"
                                             insert.AddParam("@Result", DbType.Double)
                                             insert.Parameters.Item("@Result").Value = varRes(12 + intCount * 4)
                                        End If
                                        If Len(varRes(13 + intCount * 4)) > 0 Then
                                             str1 &= ", Units"
                                             str2 &= ", @Units"
                                             insert.AddParam("@Units", DbType.String)
                                             insert.Parameters.Item("@Units").Value = varRes(13 + intCount * 4)
                                        End If
                                        If Len(varRes(14 + intCount * 4)) > 0 Then
                                             str1 &= ", Error "
                                             str2 &= ", @Error "
                                             insert.AddParam("@Error", DbType.String)
                                             insert.Parameters.Item("@Error").Value = varRes(14 + intCount * 4)
                                        End If
                                        For Each param As DbParameter In insert.Parameters
                                             If Len(param.Value) > 0 Then param.Size = Len(param.Value)
                                        Next
                                        insert.CommandText = str1 & str2 & " )"
                                        insert.Prepare()
                                        intRes = insert.ExecuteNonQuery()
                                        intCount += 1

                                   End Using
                              Loop
                         End Using

                         SendCommData(ResultAcceptance)
                         AppendToLog("Out: " & ResultAcceptance())

                    Case "C"
                         ' strData is a Calibration Result message.
                         ' *** 
                         ' *** NOTE: The data transmitted in a Calibration Result message is flawed!
                         ' *** This is inherent in the instrument software's design, mostly the practice of transmitting calibration data 
                         ' *** as soon as the measurements have been completed, rather than waiting until the new coefficients have been generated.
                         ' *** 
                         ' *** The following specific flaws have been observed as of 10/2019 (Siemens Dimension software versions through 10.3):
                         ' ***  -- The Cal_DateTime value actually represents the timestamp from the beginning of the reagent lot's previous calibration.
                         ' ***  -- Due to the above timestamp discrepancy, the first calibration of each lot will have a Cal_DateTime value of "000019311269" (12/31/xx69 7:00 PM).
                         ' ***  -- Since the new coefficients are not yet calculated when the data is transmitted, coefficient values given are from the previous calibration.
                         ' ***  -- All result ("Res##") values are calculated using the current calibration's measurements with the prior calibration's coefficients.
                         ' ***  -- The Cal_Slope value is always given as exactly "1".
                         ' ***  -- The Cal_Intercept value is always given as exactly "0". Since this holds true even when the actual intercept is >1,
                         ' ***     it seems unlikely that this is an integer rounding issue.
                         ' *** 
                         ' Processing Calibration Result messages can get a little more complicated than most. 
                         ' Due to the variations between test methods, not all fields will have a value.
                         Dim intVals As Integer, intCoefs As Integer, intField As Integer
                         Dim intValCount As Integer = 0
                         varRes = Split(strData, Chr(28), , vbBinaryCompare)
                         ' Build the command with parameters.
                         Using cnSQL As IDbConnection = GetConnected()
                              Using insert As DbCommand = cnSQL.CreateCommand()
                                   With insert
                                        .CommandText = "INSERT INTO CalibrationResults (Cal_Test, Cal_Units, Reagent_Lot, Cal_Product, Cal_Prod_Lot, Cal_Op, Cal_DateTime, Cal_Slope, Cal_Intercept, Coefficients_Num, Coefficient_0, Coefficient_1, Coefficient_2, Coefficient_3, Coefficient_4, Bottle_Vals, Val01, Res01, Val02, Res02, Val03, Res03, Val04, Res04, Val05, Res05, Val06, Res06, Val07, Res07, Val08, Res08, Val09, Res09, Val10, Res10, Val11, Res11, Val12, Res12, Val13, Res13, Val14, Res14, Val15, Res15, Val16, Res16, Val17, Res17, Val18, Res18, Val19, Res19, Val20, Res20) VALUES (@Cal_Test, @Cal_Units, @Reagent_Lot, @Cal_Product, @Cal_Prod_Lot, @Cal_Op, @Cal_DateTime, @Cal_Slope, @Cal_Intercept, @Coefficients_Num, @Coefficient_0, @Coefficient_1, @Coefficient_2, @Coefficient_3, @Coefficient_4, @Bottle_Vals, @Val01, @Res01, @Val02, @Res02, @Val03, @Res03, @Val04, @Res04, @Val05, @Res05, @Val06, @Res06, @Val07, @Res07, @Val08, @Res08, @Val09, @Res09, @Val10, @Res10, @Val11, @Res11, @Val12, @Res12, @Val13, @Res13, @Val14, @Res14, @Val15, @Res15, @Val16, @Res16, @Val17, @Res17, @Val18, @Res18, @Val19, @Res19, @Val20, @Res20)"
                                        .AddWithValue("@Cal_Test", varRes(1), DbType.String)
                                        '.Parameters.Item("@Cal_Test").Size = Len(varRes(1))
                                        .AddWithValue("@Cal_Units", varRes(2), DbType.String)
                                        '.Parameters.Item("@Cal_Units").Size = Len(varRes(2))
                                        .AddWithValue("@Reagent_Lot", varRes(3), DbType.String)
                                        '.Parameters.Item("@Reagent_Lot").Size = Len(varRes(3))
                                        .AddWithValue("@Cal_Product", varRes(4), DbType.String)
                                        '.Parameters.Item("@Cal_Product").Size = Len(varRes(4))
                                        .AddWithValue("@Cal_Prod_Lot", varRes(5), DbType.String)
                                        '.Parameters.Item("@Cal_Prod_Lot").Size = Len(varRes(5))
                                        .AddWithValue("@Cal_Op", varRes(6), DbType.String)
                                        '.Parameters.Item("@Cal_Op").Size = Len(varRes(6))
                                        .AddWithValue("@Cal_DateTime", varRes(7), DbType.String)
                                        '.Parameters.Item("@Cal_DateTime").Size = Len(varRes(7))
                                        .AddWithValue("@Cal_Slope", varRes(8), DbType.Double)
                                        .AddWithValue("@Cal_Intercept", varRes(9), DbType.Double)
                                        .AddWithValue("@Coefficients_Num", varRes(10), DbType.Int32)
                                        .AddWithValue("@Coefficient_0", varRes(11), DbType.Double)
                                        .AddWithValue("@Coefficient_1", varRes(12), DbType.Double)
                                        intCoefs = varRes(10) ' Number of coefficients.
                                        ' Not all tests have all 5 coefficients.
                                        Select Case intCoefs
                                             Case 2
                                                  .AddWithValue("@Coefficient_2", DBNull.Value, DbType.Double)
                                                  .AddWithValue("@Coefficient_3", DBNull.Value, DbType.Double)
                                                  .AddWithValue("@Coefficient_4", DBNull.Value, DbType.Double)
                                             Case 3
                                                  .AddWithValue("@Coefficient_2", varRes(13), DbType.Double)
                                                  .AddWithValue("@Coefficient_3", DBNull.Value, DbType.Double)
                                                  .AddWithValue("@Coefficient_4", DBNull.Value, DbType.Double)
                                             Case 4
                                                  .AddWithValue("@Coefficient_2", varRes(13), DbType.Double)
                                                  .AddWithValue("@Coefficient_3", varRes(14), DbType.Double)
                                                  .AddWithValue("@Coefficient_4", DBNull.Value, DbType.Double)
                                             Case 5
                                                  .AddWithValue("@Coefficient_2", varRes(13), DbType.Double)
                                                  .AddWithValue("@Coefficient_3", varRes(14), DbType.Double)
                                                  .AddWithValue("@Coefficient_4", varRes(15), DbType.Double)
                                        End Select

                                        .AddWithValue("@Bottle_Vals", varRes(11 + intCoefs), DbType.Int32)
                                        intVals = varRes(11 + intCoefs) ' Number of levels tested. Each option (3-5) has a different number of test results.
                                        Dim strVals As String, dblLevel As Double, intResults As Integer, intResCount As Integer
                                        intCount = 12 + intCoefs ' intCount keeps track of where we are in the string array. 
                                        ' The first 12 elements will always be used for the same fields, but the varying number of coefficients requires us to keep track of where we are from this point onward.
                                        intField = 16 ' Keeps our place in the record. Add 16 to account for the parameters we've already filled.

                                        ' This next part gets pretty crazy. 
                                        ' Turns out the least complicated way I've found is to add all the remaining parameters at once,
                                        ' and then we can use an integer variable to cycle through them.
                                        For i = 1 To 20
                                             strVals = Microsoft.VisualBasic.Right("0" & i, 2)
                                             .AddParam("@Val" & strVals, DbType.Double)
                                             .AddParam("@Res" & strVals, DbType.Double)
                                        Next
                                        ' Each of the different levels will have its own "bottle value" and number of results.
                                        ' For each result at each level, store the "bottle value" in one column (i.e. "Val01"), and the result in another column (i.e. "Res01").
                                        ' Some data will be repeated (the "bottle value" is stored twice if there are two results at that level), 
                                        ' but since different assays have varying calibration schemes, this seemed most efficient.
                                        Do
                                             If IsNumeric(varRes(intCount)) Then dblLevel = varRes(intCount)
                                             intResults = Nz(varRes(intCount + 1), 0)
                                             intResCount = 0
                                             Do Until intResCount = intResults
                                                  intResCount += 1
                                                  If Not intCount + intResCount + 1 < UBound(varRes, 1) Then Exit Do
                                                  If IsNumeric(varRes(intCount)) Then
                                                       .Parameters.Item(intField).Value = dblLevel
                                                  Else
                                                       .Parameters.Item(intField).Value = DBNull.Value
                                                  End If
                                                  intField += 1
                                                  If IsNumeric(varRes(intCount + 1 + intResCount)) Then .Parameters.Item(intField).Value = varRes(intCount + 1 + intResCount)
                                                  intField += 1
                                             Loop
                                             intCount = intCount + 2 + intResults
                                             intValCount += 1
                                        Loop Until intValCount = intVals

                                        For Each param As DbParameter In insert.Parameters
                                             ' Explicitly set the size of each parameter to make the Prepare command happy.
                                             If Len(param.Value) > 0 Then param.Size = Len(param.Value)
                                             ' Fill all remaining parameters with DBNulls.
                                             If IsNull(param.Value) Then param.Value = DBNull.Value
                                        Next
                                        'For i = 0 To .Parameters.Count - 1
                                        '     If IsNull(.Parameters.Item(i).Value) Then .Parameters.Item(i).Value = DBNull.Value
                                        'Next
                                        ' Try it out!
                                        Try
                                             .Prepare()
                                             Dim unused = .ExecuteNonQuery()
                                             SendCommData(ResultAcceptance)
                                             AppendToLog("Out: " & ResultAcceptance())
                                        Catch ex As Exception
                                             ' If it doesn't work, record the error and reject the result.
                                             ' We'll have 12 minutes to deal with it before a DMW/Host Communication Error is thrown.
                                             Dim trace = New System.Diagnostics.StackTrace(ex, True)
                                             Dim message As String
                                             message = ex.Source & " - Error: " & ex.Message & " at ServRosie.IncomingData Line " & ex.LineNumber() & vbCrLf & "Query: " & insert.ToString() 'trace.GetFrame(0).GetFileLineNumber().ToString()
                                             AppendToLog(message) 'ex.Source & " - Error: " & ex.Message)
                                             SendCommData(Chr(2) & "M" & Chr(28) & "R" & Chr(28) & "1" & Chr(28) & "24" & Chr(3)) ' Result Reject Message.
                                        End Try
                                   End With
                                   'End Using
                              End Using
                         End Using

               End Select

          Catch ex As Exception
               AppendToLog(ex.Source & " " & ex.TargetSite.Name & " " & ex.Message)
               EventLog.WriteEntry(ex.Source & " " & ex.Message & " " & ex.TargetSite.ToString)
               Com1.Write(Chr(21))
          End Try

     End Sub

     'Private Function GetDataAdapter(v As String, connection As Object) As IDataAdapter
     '     Select Case My.Settings.databaseType
     '          Case "SQL Server"
     '               Return SqlClient.SqlDataAdapter(v, connection)
     '               'Case "Oracle"

     '          Case "ODBC"
     '               Return Odbc.OdbcDataAdapter(v, connection)
     '          Case Else
     '               Return New Odbc.OdbcConnection(My.Settings.connectionString)
     '     End Select
     '     Throw New NotImplementedException()
     'End Function

     Public Shared Function IsNull(ByVal expression) As Boolean
          If expression Is Nothing Then
               IsNull = True
          Else
               IsNull = False
          End If
     End Function

     Sub AppendToLog(strText As String)
          Dim strName As String = "C:\Users\Public\Documents\Serial_Logs\SerialLog_" & Today.ToString("dd-MM-yyyy") & ".txt"
          If Directory.Exists("C:\Users\Public\Documents\Serial_Logs\") = False Then
               Directory.CreateDirectory("C:\Users\Public\Documents\Serial_Logs\")
          End If
          strText = Now() & " " & strText & vbCrLf
          File.AppendAllText(strName, strText)
     End Sub

     Function NoRequestMessage() As String
          ' This function returns a string indicating to the instrument that the computer has no pending requests.
          Dim strHex As String
          strHex = Chr(2) & "N" & Chr(28)
          strHex = strHex & CHKSum(strHex) & Chr(3)
          NoRequestMessage = strHex
     End Function

     Function SampleRequestMessage(boolDelete As Boolean, strPatientID As String, strSampleNo As String, strSampleType As String, intPriority As Integer, ByRef strTests() As String, Optional iDilFactor As Integer = 1) As String
          ' This function returns a string to tell the instrument what tests to run on a sample.
          Dim strHex As String
          Dim intTests As Integer, intCount As Integer

          ' Count how many tests we need to add.
          intTests = UBound(strTests, 1) + 1
          intCount = 0
          strHex = Chr(2) & "D" & Chr(28) & "0" & Chr(28) & "0" & Chr(28)
          If boolDelete Then
               strHex &= "D"
          Else
               strHex &= "A"
          End If
          strHex = strHex & Chr(28) & strPatientID & Chr(28) & strSampleNo & Chr(28) ' Message type, carrier ID, loadlist ID, Add/Delete, Patient ID, Sample ID.
          strHex = strHex & strSampleType & Chr(28) & "" & Chr(28) & intPriority & Chr(28) & "1" & Chr(28) '& "**" ' Sample type, location, priority, and # of cups for the sample.
          strHex = strHex & "**" & Chr(28) & iDilFactor & Chr(28) ' Sample position and dilution factor.
          strHex = strHex & intTests & Chr(28) ' The number of tests.
          Do
               strHex = strHex & StrConv(strTests(intCount), vbUpperCase) & Chr(28)
               intCount += 1
          Loop Until intCount = intTests

          strHex = strHex & CHKSum(strHex) & Chr(3)
          SampleRequestMessage = strHex

     End Function

     'Private Function GetSQL(Optional ByVal strCommand As String = "SELECT * FROM TempPendings WHERE PendingSending = 'TRUE'") As String()

     '     'Dim con As New SqlClient.SqlConnection
     '     'Dim strCon As String = GetConnectionString()
     '     Dim command As IDbCommand 'SqlCommand
     '     Dim dr As IDataReader 'Adapter 'SqlDataAdapter
     '     'Dim dt As New DataTable
     '     Try
     '          'con.ConnectionString = strCon
     '          Using cnSQL = GetConnected()
     '               command = cnSQL.CreateCommand() ' With {
     '               command.CommandText = strCommand
     '               '.CommandTimeout = 3000
     '               '}

     '               'da = 'DbDataAdapter()  ' (command)
     '               'da.Fill(dt)
     '               'Dim rows() As DataRow = dt.Select()
     '               'Dim strArray(dt.Columns.Count - 1) As String
     '               'For i = 0 To dt.Columns.Count - 1
     '               '     If dt.Rows.Count > 0 Then
     '               '          strArray(i) = Nz(rows(0)(i))
     '               '     Else
     '               '          strArray(i) = Nothing
     '               '     End If
     '               'Next
     '               dr = command.ExecuteReader
     '               If dr.Read() Then
     '                    Dim strArray(dr.FieldCount - 1)
     '                    For i = 0 To dr.FieldCount - 1
     '                         strArray(i) = Nz(dr.Item(i))
     '                    Next
     '                    Return strArray
     '               Else
     '                    Return Nothing
     '               End If
     '          End Using

     '     Catch ex As Exception
     '          ' Something went wrong. Return Nothing.
     '          Return Nothing
     '          'Finally
     '          '    If con.State = ConnectionState.Open Then
     '          '        con.Close()
     '          '    End If
     '     End Try
     'End Function

     'Private Function StoreSQL(ByVal strCommand As String) As Integer
     '     ' Use this for INSERT, UPDATE, or DELETE transactions that do NOT include concatenated user input in the command string.
     '     'Dim con As New SqlConnection
     '     'Dim command As DbCommand
     '     Try
     '          Using cnSQL = GetConnected()
     '               Using command As DbCommand = cnSQL.CreateCommand()
     '                    'con.ConnectionString = GetConnectionString()
     '                    'Using command As New SqlCommand(strCommand, cnSQL)
     '                    'command = cnSQL.CreateCommand()
     '                    command.CommandText = strCommand
     '                    'command.Connection.Open()
     '                    StoreSQL = command.ExecuteNonQuery()
     '               End Using
     '          End Using
     '     Catch ex As Exception
     '          AppendToLog(ex.Source & " " & ex.TargetSite.Name & " " & ex.Message)
     '          StoreSQL = 0
     '     End Try

     'End Function

     Public Shared Function Nz(ByVal Value As Object, Optional ByVal oDefault As Object = "") As Object
          If Value Is Nothing OrElse IsDBNull(Value) Then
               Return oDefault
          Else
               Return Value
          End If
     End Function

     'Function GetConnectionString() As String
     '     ' To avoid storing the connection string in your code,
     '     ' you can retrieve it from a configuration file. 
     '     'Return "Data Source=(local)\SQLExpress;Initial Catalog=LISDB;Integrated Security=True"
     '     Return My.Settings.connectionString
     'End Function
End Class
