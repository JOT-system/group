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
''' Leftボックス用グループ取得
''' </summary>
''' <remarks></remarks>
Public Class GS0011GROUPget
    Inherits GS0000
    ''' <summary>
    ''' ユーザID
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property USERID As String
    ''' <summary>
    ''' 開始年月日
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property STYMD As Date
    ''' <summary>
    ''' 終了年月日
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property ENDYMD As Date
    ''' <summary>
    ''' 会社コード
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property CAMPCODE() As String
    ''' <summary>
    ''' オブジェクトコード
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property OBJECTCD() As String
    ''' <summary>
    ''' グループコード
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property GORUPCODE() As List(Of String)
    ''' <summary>
    ''' グループ名称
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property GROUPNAME() As List(Of String)
    ''' <summary>
    ''' グループ一覧
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property LISTBOX() As Object

    Protected METHOD_NAME As String = "GS0011GROUPget"
    ''' <summary>
    ''' グループ一覧を取得する
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub GS0011GROUPget()
        '<< エラー説明 >>
        'O_ERR = OK:00000,ERR:00002(環境エラー),ERR:00003(DBerr)

        GORUPCODE = New List(Of String)
        GROUPNAME = New List(Of String)

        '●In PARAMチェック
        'PARAM01: CAMPCODE
        If checkParam(METHOD_NAME, CAMPCODE) Then
            Exit Sub
        End If

        'PARAM02: OBJECTCD
        If checkParam(METHOD_NAME, OBJECTCD) Then
            Exit Sub
        End If
        'セッション制御宣言
        Dim sm As New CS0050SESSION
        'PARAM EXTRA01: USERID
        If IsNothing(USERID) Then
            USERID = sm.USERID
        End If
        'PARAM EXTRA02: STYMD
        If STYMD < C_DEFAULT_YMD Then
            STYMD = Date.Now
        End If
        'PARAM EXTRA03: ENDYMD
        If ENDYMD < C_DEFAULT_YMD Then
            ENDYMD = Date.Now
        End If
        '●Leftボックス用グループ取得
        '○ DB(M0007_GROUP)検索
        Try
            'DataBase接続文字
            Dim SQLcon = sm.getConnection
            SQLcon.Open() 'DataBase接続(Open)

            '検索SQL文
            Dim SQLStr As String = _
                    "SELECT rtrim(A.CAMPCODE) as CAMPCODE , rtrim(A.GRCODE) as GRCODE , rtrim(A.NODENAMES) as NAMES " _
                & " FROM  M0007_GROUP A " _
                & " Where A.USERID   = @P1 " _
                & "   and A.OBJECT   = @P2 " _
                & "   and A.STYMD   <= @P4 " _
                & "   and A.ENDYMD  >= @P3 " _
                & "   and A.DELFLG  <> '1' " _
                & "ORDER BY A.NODENAMES "

            Dim SQLcmd As New SqlCommand(SQLStr, SQLcon)
            Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.NVarChar, 20)
            Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.NVarChar, 20)
            Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.Date)
            Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", System.Data.SqlDbType.Date)
            PARA1.Value = USERID
            PARA2.Value = OBJECTCD
            PARA3.Value = STYMD
            PARA4.Value = ENDYMD
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
                If CAMPCODE = "" Then
                    GORUPCODE.Add(SQLdr("GRCODE"))
                    GROUPNAME.Add(SQLdr("NAMES"))
                    LISTBOX.Items.Add(New ListItem(SQLdr("NAMES"), SQLdr("GRCODE")))
                ElseIf CAMPCODE = SQLdr("CAMPCODE") Then
                    GORUPCODE.Add(SQLdr("GRCODE"))
                    GROUPNAME.Add(SQLdr("NAMES"))
                    LISTBOX.Items.Add(New ListItem(SQLdr("NAMES"), SQLdr("GRCODE")))
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
            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME                    'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:M0007_GROUP Select"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            ERR = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try

        ERR = C_MESSAGE_NO.NORMAL

    End Sub

End Class