Imports Microsoft.VisualBasic
Imports System.Net.Mail

Module sendmail


    Public Sub sendnotification(ByRef notiefrom As String, ByRef notieTo As String, ByRef subject As String, ByRef notiemessage As String)

        Try
            ' Create the message
            Dim message As New MailMessage

            ' This is adress that will get notified
            message.From = New MailAddress(notiefrom)
            message.To.Add(notieTo)
            message.Subject = subject
            message.Body = notiemessage

            ' Set DeliveryNotificationOptions.OnFailure to state that the From-adress
            ' should be notified if the recipient couldn't be reached
            message.DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure

            ' Create smtp-service
            Dim smtpService As New Net.Mail.SmtpClient("10.236.80.29")

            'Send the message
            smtpService.Send(message)
        Catch ex As Exception
            ' MsgBox(ex.ToString)
        End Try

    End Sub

End Module
