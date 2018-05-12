Imports Newtonsoft.Json

Module readJSON
    Public Class JSON_subscribe
        Public SID As String
        Public cno As String
        Public netsts As String
        Public ssid As String
    End Class
    Public Class JSON_4_publish
        Public sts As Integer
        Public inout As String
        Public cardno As String
        Public username As String
        Public opt As Integer
    End Class
    
    Sub test()
        Dim data As New JSON_4_publish()
        data.sts = 0
        data.inout = "IN"
        data.cardno = "0008657047"
        data.username = "Phang"
        data.opt = 0
        MsgBox(JSON_Data_publish(data))


    End Sub
    Sub test_parse()
        Dim data As New JSON_subscribe
        data.SID = "123"
        data.cno = "0008657047"
        data.netsts = "0"
        data.ssid = "wifiname"

        Dim textline As String = JSON_Data_publish(data)

        Dim result = JSON_parse(textline)
        MsgBox(result)


    End Sub

    Public Function JSON_Data_publish(data) As String

        'Call SeralizeObject to convert the object to JSON string'
        Dim output As String = JsonConvert.SerializeObject(data)
        
        'MsgBox(output)
        JSON_Data_publish = output

    End Function
   
    Public Function JSON_parse(textline) As JSON_subscribe



        Dim obj As JSON_subscribe
        obj = JsonConvert.DeserializeObject(Of JSON_subscribe)(textline)

        ' MsgBox(obj.cno)

        JSON_parse = obj

    End Function












End Module
