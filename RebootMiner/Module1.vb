Imports System.Drawing
Imports System.Drawing.Imaging
Imports System.Net.Mail

Module Module1
    Private Declare Function ShowWindow Lib "user32.dll" (ByVal hWnd As IntPtr, ByVal nCmdShow As Integer) As Boolean
    Declare Function GetUserName Lib "advapi32.dll" Alias "GetUserNameA" (ByVal lpBuffer As String, ByRef nSize As Integer) As Integer

    Private EmailAddress, CrashImage As String
    Private Image1 As Bitmap
    Private Timer1 As New System.Timers.Timer

    Sub Main()
        CrashImage = Environment.GetEnvironmentVariable("Temp") & "\Crash.jpg"
        EmailAddress = Command()
        Timer1.AutoReset = True
        Timer1.Interval = 60000
        AddHandler Timer1.Elapsed, AddressOf tick
        Timer1.Start()
        For Each p As Process In Process.GetProcesses
            If p.ProcessName.Contains("RebootMiner") Then
                ShowWindow(p.MainWindowHandle, 6)
            End If
        Next p
        Console.ReadKey()
    End Sub

    Private Sub tick(ByVal sender As Object, ByVal e As System.Timers.ElapsedEventArgs)

        If AreSameImage(Image1) Then
            Timer1.Stop()
            If EmailAddress <> "" Then SendGmail()
            Shell("Shutdown -r -t 5 -f")
            End
        End If

    End Sub



    Public Function AreSameImage(ByVal Last As Bitmap) As Boolean

        Dim screenSize As Size = New Size(My.Computer.Screen.Bounds.Width, CInt(My.Computer.Screen.Bounds.Height * 0.9))
        Using screenGrab As New Bitmap(My.Computer.Screen.Bounds.Width, CInt(My.Computer.Screen.Bounds.Height * 0.9))
            Using g As Graphics = Graphics.FromImage(screenGrab)
                g.CopyFromScreen(New Point(0, 0), New Point(0, 0), screenSize)
                'If Not Image1 Is Nothing Then image1.dispose
                Image1 = New Bitmap(screenGrab)
                If Not Last Is Nothing Then
                    Using Last
                        Using screenGrab
                            For X = 0 To Last.Width - 1
                                For y = 0 To screenGrab.Height - 1
                                    If Last.GetPixel(X, y) <> screenGrab.GetPixel(X, y) Then
                                        Return False
                                    End If
                                Next
                            Next
                        End Using
                    End Using
                Else
                    Return False
                End If
            End Using
        End Using

        Return True

    End Function


    Private Sub SendGmail()
        Try
            SaveScreen()

            Using SmtpServer As New SmtpClient("smtp.gmail.com", 587)
                Using Email As New MailMessage("RebootMinerNET@gmail.com", EmailAddress, "RebootMinerNET", "Miner crashed on PC: " & Environment.MachineName & vbCrLf & "Username: " & GetUserName() & vbCrLf & vbCrLf & "See attached screenshot for details." & vbCrLf & "Thanks for using RebootMinerNET!")
                    SmtpServer.EnableSsl = True
                    SmtpServer.Credentials = New Net.NetworkCredential("RebootMinerNET@gmail.com", "RebootMiner!@#$")
                    Email.Attachments.Add(New System.Net.Mail.Attachment(CrashImage))
                    SmtpServer.Send(Email)
                End Using
            End Using
            If System.IO.File.Exists(CrashImage) Then System.IO.File.Delete(CrashImage)
        Catch ex As Exception
        End Try
    End Sub
    Private Function GetUserName() As String
        Dim iReturn As Integer
        Dim userName As String
        userName = New String(CChar(" "), 50)
        iReturn = GetUserName(userName, 50)
        GetUserName = userName.Substring(0, userName.IndexOf(Chr(0))).ToLower
    End Function

    Private Sub SaveScreen()
        If System.IO.File.Exists(CrashImage) Then System.IO.File.Delete(CrashImage)
        Dim myImageCodecInfo As ImageCodecInfo
        Dim myEncoder As Encoder
        Dim myEncoderParameter As EncoderParameter
        Dim myEncoderParameters As EncoderParameters
        myImageCodecInfo = GetEncoderInfo(ImageFormat.Jpeg)
        myEncoder = Encoder.Quality
        myEncoderParameters = New EncoderParameters(1)
        myEncoderParameter = New EncoderParameter(myEncoder, CType(10L, Int32))
        myEncoderParameters.Param(0) = myEncoderParameter

        Image1.Save(CrashImage, myImageCodecInfo, myEncoderParameters)

        myImageCodecInfo = Nothing
        myEncoder = Nothing
        myEncoderParameter = Nothing
        myEncoderParameters = Nothing
    End Sub

    Private Function ResizeImage(ByVal InputImage As Image) As Image
        Return New Bitmap(InputImage, New Size(InputImage.Width * 0.5, InputImage.Height * 0.5))
    End Function
    Private Function GetEncoderInfo(ByVal format As ImageFormat) As ImageCodecInfo
        Dim j As Integer
        Dim encoders() As ImageCodecInfo
        encoders = ImageCodecInfo.GetImageEncoders()

        j = 0
        While j < encoders.Length
            If encoders(j).FormatID = format.Guid Then
                Return encoders(j)
            End If
            j += 1
        End While
        Return Nothing

    End Function
End Module
