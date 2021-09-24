Imports System.Runtime.CompilerServices
Imports System.Net.Mail
Imports System.Configuration
Imports System.Text.RegularExpressions

Module Helper
    '<Extension()> Public Function SendMail(ByVal _Smtp As String, ByVal _Sender As String, ByVal _Receiver As String, ByRef _Subject As String, ByVal _Err As Boolean) As Boolean
    '    Dim mail As New MailMessage()
    '    Dim strb As New Text.StringBuilder
    '    Dim attachment As System.Net.Mail.Attachment
    '    Try
    '        Dim SmtpServer As New SmtpClient(_Smtp)
    '        mail.From = New MailAddress(_Sender)
    '        mail.[To].Add(_Receiver)
    '        mail.Subject = _Subject & IIf(_Err, " (Normally) ", " (Abnormally) ")
    '        mail.Body = GetSetting("msg" & CInt(Int(_Err)))
    '        If Not (_Err) Then
    '            attachment = New System.Net.Mail.Attachment(FrmMain._PathFileLog)
    '            mail.Attachments.Add(attachment)
    '        End If
    '        SmtpServer.Port = 25
    '        'SmtpServer.Credentials = New System.Net.NetworkCredential("username", "password")
    '        SmtpServer.EnableSsl = False
    '        SmtpServer.Send(mail)
    '        Return True
    '    Catch ex As Exception
    '        Return False
    '    End Try
    'End Function
    <Extension()> Friend Function SendMail(ByVal _Sender As String, ByVal _Sender_pass As String, ByVal _Receiver As String, ByRef _Subject As String, ByVal _Err As Boolean) As Boolean
        Try
            Dim SmtpServer As New SmtpClient()
            Dim mail As New MailMessage()
            Dim attachment As System.Net.Mail.Attachment
            SmtpServer.Credentials = New Net.NetworkCredential(_Sender, _Sender_pass)
            SmtpServer.Port = 587
            SmtpServer.Host = "smtp.gmail.com"
            SmtpServer.EnableSsl = True
            'SmtpServer.UseDefaultCredentials = True
            mail.From = New MailAddress(_Sender)
            mail.To.Add(_Receiver)
            mail.Subject = _Subject & IIf(_Err, " (Normally) ", " (Abnormally) ")
            mail.Body = GetSetting("msg" & CInt(Int(_Err)))
            If Not (_Err) Then
                attachment = New System.Net.Mail.Attachment(FrmMain._PathFileLog)
                mail.Attachments.Add(attachment)
            End If
            SmtpServer.Send(mail)
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function
    <Extension()> Friend Function GetSetting(ByVal Key As String) As String
        Dim Result As String = Nothing
        Dim _appSettings = ConfigurationSettings.AppSettings
        Try
            If _appSettings.Count = 0 Then
            Else
                Result = _appSettings(Key)
            End If
        Catch Ex As ConfigurationErrorsException
        End Try
        Return Result
    End Function
    <Extension()> Friend Function EmailValid(ByVal _MailStr As String) As Boolean
        Return Regex.IsMatch(_MailStr, "(?<=[_a-z0-9-]+(.[a-z0-9-]+)@[gG]mail.com\z)")
    End Function
End Module
