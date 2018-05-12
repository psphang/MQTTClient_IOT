
Imports System.Text
'including the M2Mqtt Library
' Before need to install M2MQTT
' Open View->Other Windows->Package Manager and type the following command there:
'Type command
'Install-Package M2Mqtt
Imports uPLibrary.Networking.M2Mqtt
Imports uPLibrary.Networking.M2Mqtt.Messages
Imports uPLibrary.Networking.M2Mqtt.Utility
Imports uPLibrary.Networking.M2Mqtt.Session
Imports uPLibrary.Networking.M2Mqtt.Internal
Imports MovementTimeClock.MySQLdata
Imports MovementTimeClock.readJSON

Public Class Form1
    Public Event MqttMsgSubscribed As MqttClient.MqttMsgSubscribedEventHandler
    Public Event MqttMsgPublishReceived As MqttClient.MqttMsgPublishEventHandler

    Private client As MqttClient
    Private clientId As String
    Private Delegate Sub SetTextCallback(text As String)
    Public txtreceived As String

    'MQTT Server parameter
    Const mqttServer As String = "10.236.80.189"
    Const mqttPort As Integer = 1883
    Const mqttUser As String = "wfuser"
    Const mqttPassword As String = "wfpass"
    'MQTT Publish/Subcribe TOPIC
    Const Topic As String = "esp/move"

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        Dim BrokerAddress As String = mqttServer

        client = New MqttClient(mqttServer)

        AddHandler client.MqttMsgSubscribed, AddressOf client_MqttMsgSubscribed
        AddHandler client.MqttMsgPublishReceived, AddressOf client_MqttMsgPublishReceived

        While (client.IsConnected = False)
            Dim clientId As String = Guid.NewGuid().ToString()
            Dim code As Byte = client.Connect(clientId, mqttUser, mqttPassword)

        End While
        ' MsgBox("mqtt Broker conection: " & client.IsConnected)

        'subscribe to the topic with QoS 2
        Dim ret = client.Subscribe(New String() {Topic & "/"}, {MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE})




       


    End Sub
    Public Sub client_MqttMsgPublishReceived(ByVal sender As Object, e As MqttMsgPublishEventArgs)
        Dim ReceivedMessage As String = Encoding.UTF8.GetString(e.Message)
        Debug.Print(ReceivedMessage)
        process_data(ReceivedMessage)
       
      
    End Sub
    Public Sub client_MqttMsgSubscribed(sender As Object, e As MqttMsgSubscribedEventArgs)
        ' Dim subscribeMessage As String = Encoding.UTF8.GetString(e.GrantedQoSLevels)
        Dim subscribeMessage As String = (e.MessageId)


        Debug.Print(subscribeMessage)
    End Sub
    

    Private Sub btnpublish_Click(sender As Object, e As EventArgs) Handles btnpublish.Click
        Dim txtpub As String = "Test"
        client.Publish(Topic, Encoding.UTF8.GetBytes(txtpub), MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, False)
    End Sub

    Private Sub process_data(ReceivedMessage)
        Dim SID As String

        Dim sts As Integer = 0
        Dim email_lst As String = "phang@mem.meap.com"
        Dim email_from As String = "phang@mem.meap.com"
        Dim net_sts As Integer = 0
        Dim use_ssid As String
        Dim employeeInfo As New Employee
        Dim CardNo As String
        Dim ret As Boolean
        Dim opt As Integer = 0
        Try

            Dim subdata As New JSON_subscribe

            subdata = JSON_parse(ReceivedMessage)
            Debug.Print(subdata.cno)



            SID = subdata.SID
            CardNo = subdata.cno
            net_sts = subdata.netsts
            use_ssid = subdata.ssid
            'MsgBox(SID & "," & CardNo & "," & net_sts & "," & use_ssid)
            '      SID = sender.Request.QueryString("SID") 'Sensor ID      
            '     CardNo = sender.Request.QueryString("CNo")  'EM card no 
            '      net_sts = sender.Request.QueryString("netsts") 'Network connection status
            '     use_ssid = sender.Request.QueryString("SSID")   'Wifi SSID/name

            'Test Data
            ' net_sts = 0
            'CardNo = "0008657047"

            If (net_sts > 0) Then 'Network resume send notification mail 
                Dim mailcontent As String = ""
                Dim mailsubject As String = ""

                If (net_sts = 1) Then
                    mailcontent = "The network connection start up " & Now() & ". Connected to (SSID) :" & use_ssid & ". Last success connection on : "
                    mailsubject = SID & "The network connection start up " & Now()
                ElseIf (net_sts = 2) Then
                    mailcontent = "The network connection RESUME " & Now() & ". Connected to (SSID) :" & use_ssid & ". Last success connection on : "
                    mailsubject = SID & "The network connection Resume " & Now()
                End If



                '  mailcontent = mailcontent & reader("lastupdate")

                sendnotification(email_from, email_lst, mailsubject, mailcontent)

            ElseIf (Not CardNo = "") Then

                employeeInfo.username = MySQLGetUserInfo(CardNo, "empfullname") 'empfullname or name
                If Not employeeInfo.username = "" Then 'Card no is exist
                    employeeInfo.inout = "OUT"
                    employeeInfo.iptxt = SID
                    ret = inoutTxt(employeeInfo)      'Add inout text
                    ret = TodayCount(employeeInfo)    'Add notetext
                    ret = Insertinfo(employeeInfo) 'Insert data

                    If ret Then
                        sts = 0    'Insert data success
                    Else
                        sts = 1    'write failed
                    End If

                Else
                    sts = 2 'Card no not found

                End If



            End If
            'publish data to Topic + /SID
            Dim pubdata As New JSON_4_publish()
            pubdata.sts = sts
            pubdata.inout = employeeInfo.inout
            pubdata.cardno = CardNo
            pubdata.username = employeeInfo.username
            pubdata.opt = 0
            Dim txtpub As String
            txtpub = JSON_Data_publish(pubdata)
            ' MsgBox(txtpub)
            client.Publish(Topic & "/" & SID, Encoding.UTF8.GetBytes(txtpub), MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, False)

            '  Response.Write("<sts>" & sts & "</sts>")
            '   Response.Write("<inout>" & employeeInfo.inout & "</inout>")
            '   Response.Write("<cardno>" & CardNo & "</cardno>")
            '   Response.Write("<username>" & employeeInfo.username & "</username>")
            '   Response.Write("<opt>" & opt & "</opt>")

        Catch ex As Exception
            ' Response.Write(ex.Message)
            sendnotification(email_from, email_lst, "Movement Time clock VB Exaption occur", ex.Message)
        End Try




    End Sub

End Class
