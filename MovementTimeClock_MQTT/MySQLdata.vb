Imports System.Data
Imports System.Data.SqlClient
Imports System.Data.OleDb
Imports System
Imports System.IO
Imports MySql.Data.MySqlClient
Imports System.String

Structure Employee
    Dim username As String
    Dim inout As String
    Dim notetxt As String
    Dim iptxt As String
End Structure



Module MySQLdata

    Public Function TodayCount(ByRef employeeInfor) As Boolean
        Dim connect As New MySqlConnection("server=10.236.80.189;user id=wfuser;password=wfpass;database=timeclock;SslMode=none")
        Dim cmd As New MySqlCommand

        Try
            With cmd
                .CommandText = "Select Count(`inout`) as TodayOutCount FROM INFO WHERE (fullname ='" & employeeInfor.username & "') AND `inout`='" & employeeInfor.inout & "' AND DATE(FROM_UNIXTIME(`timestamp`) +  interval 8 hour) = CURDATE();"
                .Connection = connect
                connect.Open()
                Dim cnt As Integer = .ExecuteScalar()
                employeeInfor.notetxt = "Count No " & cnt + 1

            End With
            connect.Close()
            Return True
        Catch ex As Exception
            'MsgBox(ex.Message)

            connect.Close()
            Return False
        End Try
    End Function


    Public Function inoutTxt(ByRef employeeInfor) As Boolean

        Dim connect As New MySqlConnection("server=10.236.80.189;user id=wfuser;password=wfpass;database=timeclock;SslMode=none")
        Dim cmd As New MySqlCommand
        Dim dr As MySqlDataReader


        Try
            With cmd
                .CommandText = "SELECT A.TSTAMP , B.INOUT " _
                                & "FROM ( " _
                                        & " (SELECT TSTAMP , empfullname FROM employees) A " _
                                        & " LEFT JOIN " _
                                        & " (SELECT fullname, `inout`, `timestamp` FROM INFO) B ON (B.FULLNAME=A.EMPFULLNAME AND B.TIMESTAMP=A.TSTAMP))" _
                             & "WHERE (A.empfullname ='" & employeeInfor.username & "');"
                .CommandType = CommandType.Text
                .Connection = connect
            End With
            connect.Open()
            dr = cmd.ExecuteReader
            Dim last_ts As Integer
            Dim last_inout As String = ""
            While dr.Read
                last_ts = Convert.ToInt64(dr("tstamp"))
                last_inout = Convert.ToString(dr("INOUT"))
            End While


            Dim nx As New DateTime(1970, 1, 1) ' UNIX epoch date
            Dim ts = DateAdd(DateInterval.Hour, -8, DateTime.UtcNow) - nx              'UtcNow, because timestamp is in GMT
            Dim timetxt = Convert.ToInt64(ts.TotalSeconds)
            Dim diff_min = (timetxt - last_ts) / 60
            If diff_min < 480 Then 'normally no OUT more than 480 minit (8 hour)
                If String.Compare(last_inout, "IN") = 0 Then
                    employeeInfor.inout = "OUT"
                Else
                    employeeInfor.inout = "IN"
                End If
            Else
                employeeInfor.inout = "OUT"
            End If


            dr.Close()
            connect.Close()
            Return True
        Catch ex As Exception
            'MsgBox(ex.Message)

            ' dr.Close()
            connect.Close()
            Return False
        End Try


    End Function
    Public Function Insertinfo(ByRef employeeInfo) As Boolean
        Dim connectionString As String = "server=10.236.80.189;user id=wfuser;password=wfpass;database=timeclock;SslMode=none"
        Dim SQLConnection As MySqlConnection = New MySqlConnection
        Dim oDt_sched As New DataTable()


        SQLConnection = New MySqlConnection()
        SQLConnection.ConnectionString = connectionString
        Dim sqlCommand As New MySqlCommand
        Dim str_carSql As String



        Dim nx As New DateTime(1970, 1, 1) ' UNIX epoch date
        Dim ts = DateAdd(DateInterval.Hour, -8, DateTime.UtcNow) - nx              'UtcNow, because timestamp is in GMT
        Dim timetxt = Convert.ToInt64(ts.TotalSeconds)


        Try
            SQLConnection.Open()
            ' str_carSql = "insert into info(fullname,inout,notes,ipaddress) values ('" & nametxt & "','" & inouttxt & "'," & "'" & notetxt & "','" & iptxt & "');"
            str_carSql = "INSERT INTO info (fullname, `inout`, `timestamp`, notes, ipaddress) VALUES('" & employeeInfo.username & "','" & employeeInfo.inout & "'," & timetxt & ",'" & employeeInfo.notetxt & "','" & employeeInfo.iptxt & "') ;"
            str_carSql &= "Update(employees) SET tstamp = " & timetxt & " WHERE empfullname = '" & employeeInfo.username & "';"


            'MsgBox(str_carSql)
            sqlCommand.Connection = SQLConnection
            sqlCommand.CommandText = str_carSql
            sqlCommand.ExecuteNonQuery()
            SQLConnection.Close()
            Return True

        Catch ex As Exception
            ' MsgBox(ex.Message)
            SQLConnection.Close()
            Return False

            'MsgBox("Error occured: Could not insert record")
        End Try

    End Function


    Public Function MySQLGetUserInfo(ByRef FindCardNo As String, ByRef UserData As String) As String

        Dim connect As New MySqlConnection("server=10.236.80.189;user id=wfuser;password=wfpass;database=timeclock;SslMode=none")

        Dim findresult = ""
        Try
            connect.Open()

            Dim sqladapter As New MySqlDataAdapter
            Dim sqlcmd As New MySqlCommand
            Dim dr As String
            Dim dt As New DataTable
            Dim findtxt = "0008657047"


            sqlcmd = New MySqlCommand("SELECT " & UserData & " FROM employees WHERE email ='" & FindCardNo & "'")

            sqlcmd.Connection = connect

            dr = sqlcmd.ExecuteScalar()
            'dr now contains the value of [COLUMN NAME] for the first returned row.

            connect.Close()
            Return dr
        Catch ex As Exception
            MsgBox(ex.Message)

            connect.Close()
        End Try

        Return findresult
    End Function






End Module
