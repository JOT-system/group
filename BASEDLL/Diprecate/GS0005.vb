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
''' 画面RightBOX用帳票ID取得
''' </summary>
''' <remarks></remarks>
Public Class GS0005REPORTIDget
    Inherits GS0000
    ''' <summary>
    ''' 画面ID
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property MAPID() As String
    ''' <summary>
    ''' ユーザID
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property USERID() As String
    ''' <summary>
    ''' RightBOX用帳票ID(ListBOX)
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property REPORTOBJ() As Object
    ''' <summary>
    ''' メソッド名
    ''' </summary>
    ''' <remarks></remarks>
    Protected METHOD_NAME As String = "GS0005REPORTIDget"
    ''' <summary>
    ''' 画面RightBOX用帳票ID取得
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub GS0005REPORTIDget()
        '<< エラー説明 >>
        'O_ERR = OK:00000,ERR:00002(環境エラー),ERR:00003(DBerr)

        '●In PARAMチェック
        'PARAM01: MAPID
        If checkParam(METHOD_NAME, MAPID) Then
            Exit Sub
        End If
        'セッション制御宣言
        Dim sm As New CS0050SESSION
        'PARAM EXTRA01: USERID
        If IsNothing(USERID) Then
            USERID = sm.USERID
        End If
        '●初期処理
        ERR = C_MESSAGE_NO.NO_DATA_EXISTS_ERROR                                                 '該当するマスタは存在しません
        Dim WW_ListBOX As New ListBox

        '●RightBOX用帳票List取得
        '○ DB(S0011_UPROFXLS)検索　…　入力パラメータによる検索
        Try
            'DataBase接続文字
            Dim SQLcon = sm.getConnection
            SQLcon.Open() 'DataBase接続(Open)

            'S0011_UPROFXLS検索SQL文
            Dim SQL_Str As String = _
                    "SELECT rtrim(REPORTID) as REPORTID , rtrim(FIELDNAME) as FIELDNAME , rtrim(EXCELFILE) as EXCELFILE " _
                & " FROM  S0011_UPROFXLS " _
                & " Where ( USERID  = @P1 or USERID  = 'Default' ) " _
                & "   and MAPID     = @P2 " _
                & "   and TITOLKBN  = @P3 " _
                & "   and STYMD    <= @P4 " _
                & "   and ENDYMD   >= @P5 " _
                & "   and DELFLG   <> @P6 " _
                & " GROUP BY REPORTID , FIELDNAME , EXCELFILE " _
                & " ORDER BY REPORTID "
            Dim SQLcmd As New SqlCommand(SQL_Str, SQLcon)
            Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.NVarChar, 20)
            Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.NVarChar, 50)
            Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.NVarChar, 1)
            Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", System.Data.SqlDbType.Date)
            Dim PARA5 As SqlParameter = SQLcmd.Parameters.Add("@P5", System.Data.SqlDbType.Date)
            Dim PARA6 As SqlParameter = SQLcmd.Parameters.Add("@P6", System.Data.SqlDbType.NVarChar, 1)
            PARA1.Value = USERID
            PARA2.Value = MAPID
            PARA3.Value = "H"
            PARA4.Value = Date.Now
            PARA5.Value = Date.Now
            PARA6.Value = C_DELETE_FLG.DELETE
            Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

            While SQLdr.Read
                If SQLdr("EXCELFILE") = "" Then
                    WW_ListBOX.Items.Add(New ListItem(SQLdr("FIELDNAME") & "(書式:なし)", SQLdr("REPORTID")))
                Else
                    WW_ListBOX.Items.Add(New ListItem(SQLdr("FIELDNAME") & "(書式:" & SQLdr("EXCELFILE") & ")", SQLdr("REPORTID")))
                End If
            End While
            ERR = C_MESSAGE_NO.NORMAL
            REPORTOBJ = WW_ListBOX

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

            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME            'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:S0011_UPROFXLS Select"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力

            ERR = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try


    End Sub

End Class
