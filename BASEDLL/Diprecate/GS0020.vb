Imports System.Web
Imports System.Data.SqlClient
Imports System
Imports System.IO
Imports System.Text
Imports System.Globalization
Imports Microsoft.VisualBasic
Imports System.Web.UI
Imports System.Web.UI.WebControls
Imports System.Web.UI.Control
Imports Microsoft.Office
Imports Microsoft.Office.Interop
Imports Microsoft.Office.Core
Imports System.Net


''' <summary>
''' Leftボックス用乗務員取得
''' </summary>
''' <remarks></remarks>
Public Class GS0020CREWget
    Inherits GS0000

    ''' <summary>
    ''' ユーザID
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property USERID As String
    ''' <summary>
    ''' 会社コード
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property CAMPCODE() As String
    ''' <summary>
    ''' 組織コード
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property ORG() As String
    ''' <summary>
    ''' 乗務員コード一覧
    ''' </summary>
    ''' <value>社員コード</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property STAFFCODE() As List(Of String)
    ''' <summary>
    ''' 乗務員名一覧
    ''' </summary>
    ''' <value>社員名</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property STAFFNAME() As List(Of String)
    ''' <summary>
    ''' 乗務員情報一覧
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property LISTBOX() As ListBox

    Protected METHOD_NAME As String = "GS0020CREWget"
    ''' <summary>
    ''' 乗務員情報を取得する
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub GS0020CREWget()
        '<< エラー説明 >>
        'ERR = OK:00000,ERR:00002(環境エラー),ERR:00003(DBerr)

        '●初期処理
        STAFFCODE = New List(Of String)
        STAFFNAME = New List(Of String)
        STAFFKBN = New List(Of String)
        'セッション制御宣言
        Dim sm As New CS0050SESSION
        'PARAM EXTRA01: USERID
        If IsNothing(USERID) Then
            USERID = sm.USERID
        End If
        '●Leftボックス用組織取得
        '○ User権限によりDB(S0005_AUTHOR)検索
        Try
            'DataBase接続文字
            Dim SQLcon = sm.getConnection
            SQLcon.Open() 'DataBase接続(Open)

            '検索SQL文
            Dim SQLStr As String = ""
            If IsNothing(ORG) Then
                ORG = ""
                SQLStr = _
                            "SELECT rtrim(C.CAMPCODE) as CAMPCODE " _
                        & "      ,rtrim(C.STAFFCODE) as STAFFCODE " _
                        & "      ,rtrim(C.STAFFNAMES) as STAFFNAMES " _
                        & "      ,D.SEQ as SEQ " _
                        & " FROM  S0005_AUTHOR A " _
                        & " INNER JOIN S0006_ROLE B " _
                        & "   ON    B.CAMPCODE = A.CAMPCODE " _
                        & "   and   B.OBJECT   = A.OBJECT " _
                        & "   and   B.ROLE     = A.ROLE " _
                        & "   and   B.PERMITCODE >= 1 " _
                        & "   and   B.STYMD   <= @P2 " _
                        & "   and   B.ENDYMD  >= @P2 " _
                        & "   and   B.DELFLG  <> '1' " _
                        & " INNER JOIN MB001_STAFF C " _
                        & "   ON    C.CAMPCODE = B.CAMPCODE " _
                        & "   and   C.MORG     = B.CODE " _
                        & "   and   C.STYMD   <= @P2 " _
                        & "   and   C.ENDYMD  >= @P4 " _
                        & "   and   C.DELFLG  <> '1' " _
                        & " INNER JOIN MB002_STAFFORG D " _
                        & "   ON    D.CAMPCODE = C.CAMPCODE " _
                        & "   and   D.STAFFCODE= C.STAFFCODE " _
                        & "   and   D.SORG     = B.CODE " _
                        & "   and   D.DELFLG  <> '1' " _
                        & " INNER JOIN MC001_FIXVALUE E " _
                        & "   ON    E.CAMPCODE = 'Default' " _
                        & "   and   E.CLASS    = 'STAFFKBN' " _
                        & "   and   E.KEYCODE  = C.STAFFKBN " _
                        & "   and   E.KEYCODE like '03%' " _
                        & "   and   E.STYMD   <= @P2 " _
                        & "   and   E.ENDYMD  >= @P2 " _
                        & "   and   E.DELFLG  <> '1' " _
                        & " Where   A.USERID   = @P1 " _
                        & "   and   A.OBJECT   = 'ORG' " _
                        & "   and   A.STYMD   <= @P2 " _
                        & "   and   A.ENDYMD  >= @P2 " _
                        & "   and   A.DELFLG  <> '1' " _
                        & "GROUP BY C.CAMPCODE , D.SEQ , C.STAFFCODE , C.STAFFNAMES , C.STAFFKBN " _
                        & "ORDER BY C.CAMPCODE , D.SEQ , C.STAFFCODE , C.STAFFNAMES , C.STAFFKBN "
            Else
                SQLStr = _
                            "SELECT rtrim(C.CAMPCODE) as CAMPCODE " _
                        & "      ,rtrim(C.STAFFCODE) as STAFFCODE " _
                        & "      ,rtrim(C.STAFFNAMES) as STAFFNAMES " _
                        & "      ,D.SEQ as SEQ " _
                        & " FROM  S0005_AUTHOR A " _
                        & " INNER JOIN S0006_ROLE B " _
                        & "   ON    B.CAMPCODE = A.CAMPCODE " _
                        & "   and   B.OBJECT   = A.OBJECT " _
                        & "   and   B.ROLE     = A.ROLE " _
                        & "   and   B.PERMITCODE >= 1 " _
                        & "   and   B.STYMD   <= @P2 " _
                        & "   and   B.ENDYMD  >= @P2 " _
                        & "   and   B.DELFLG  <> '1' " _
                        & " INNER JOIN MB001_STAFF C " _
                        & "   ON    C.CAMPCODE = B.CAMPCODE " _
                        & "   and   C.MORG     = B.CODE " _
                        & "   and   C.STYMD   <= @P2 " _
                        & "   and   C.ENDYMD  >= @P4 " _
                        & "   and   C.DELFLG  <> '1' " _
                        & " INNER JOIN MB002_STAFFORG D " _
                        & "   ON    D.CAMPCODE = C.CAMPCODE " _
                        & "   and   D.STAFFCODE= C.STAFFCODE " _
                        & "   and   D.SORG     = @P3 " _
                        & "   and   D.DELFLG  <> '1' " _
                        & " INNER JOIN MC001_FIXVALUE E " _
                        & "   ON    E.CAMPCODE = 'Default' " _
                        & "   and   E.CLASS    = 'STAFFKBN' " _
                        & "   and   E.KEYCODE  = C.STAFFKBN " _
                        & "   and   E.KEYCODE like '03%' " _
                        & "   and   E.STYMD   <= @P2 " _
                        & "   and   E.ENDYMD  >= @P2 " _
                        & "   and   E.DELFLG  <> '1' " _
                        & " Where   A.USERID   = @P1 " _
                        & "   and   A.OBJECT   = 'ORG' " _
                        & "   and   A.STYMD   <= @P2 " _
                        & "   and   A.ENDYMD  >= @P2 " _
                        & "   and   A.DELFLG  <> '1' " _
                        & "GROUP BY C.CAMPCODE , D.SEQ , C.STAFFCODE , C.STAFFNAMES " _
                        & "ORDER BY C.CAMPCODE , D.SEQ , C.STAFFCODE , C.STAFFNAMES "
            End If

            Dim SQLcmd As New SqlCommand(SQLStr, SQLcon)
            Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.NVarChar, 20)
            Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.Date)
            Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.NVarChar, 20)
            Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", System.Data.SqlDbType.Date)
            PARA1.Value = USERID
            PARA2.Value = Date.Now
            PARA3.Value = ORG
            PARA4.Value = Date.Now.AddMonths(-1).Year.ToString("0000") & "/" & Date.Now.AddMonths(-1).Month.ToString("00") & "/01"
            Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

            Try
                If IsNothing(LISTBOX) Then
                    LISTBOX = New ListBox
                Else
                    CType(LISTBOX, ListBox).Items.Clear()
                End If
            Catch ex As Exception
            End Try

            While SQLdr.Read
                '○出力編集
                If CAMPCODE = "" Then
                    STAFFCODE.Add(SQLdr("STAFFCODE"))
                    STAFFNAME.Add(SQLdr("STAFFNAMES"))
                    LISTBOX.Items.Add(New ListItem(SQLdr("STAFFNAMES"), SQLdr("STAFFCODE")))
                ElseIf CAMPCODE = SQLdr("CAMPCODE") Then
                    STAFFCODE.Add(SQLdr("STAFFCODE"))
                    STAFFNAME.Add(SQLdr("STAFFNAMES"))
                    LISTBOX.Items.Add(New ListItem(SQLdr("STAFFNAMES"), SQLdr("STAFFCODE")))
                End If
            End While

            'Close
            SQLdr.Close() 'Reader(Close)
            SQLdr = Nothing

            SQLcmd.Dispose()
            SQLcmd = Nothing

            SQLcon.Close() 'DataBase接続(Close)
            SQLcon.Dispose()
            SQLcon = Nothing

        Catch ex As Exception
            Dim CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get
            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME                 'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:S0005_AUTHOR Select"           '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try

        ERR = C_MESSAGE_NO.NORMAL

    End Sub

End Class
