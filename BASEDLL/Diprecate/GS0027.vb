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
''' 車腹取得
''' </summary>
''' <remarks></remarks>
Public Class GS0027SHAFUKUget
    Inherits GS0000
    ''' <summary>
    ''' 端末ID
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property TERMID As String
    ''' <summary>
    ''' 会社コード
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property CAMPCODE() As String
    ''' <summary>
    ''' 業務車番
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property GSHABAN() As String
    ''' <summary>
    ''' 対象日付
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property YMD() As Date
    ''' <summary>
    ''' 車腹
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property SHAFUKU() As String

    Protected METHOD_NAME As String = "GS0027SHAFUKUget"
    ''' <summary>
    ''' 車腹取得
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub GS0027SHAFUKUget()
        '<< エラー説明 >>
        'ERR = OK:00000,ERR:00003(DBerr)
        '●初期処理
        'セッション制御宣言
        Dim sm As New CS0050SESSION
        'PARAM EXTRA01: TERMID
        If IsNothing(TERMID) Then
            TERMID = sm.TERMID
        End If
        '●車腹取得
        '○ User権限によりDB(S0005_AUTHOR)検索
        Try
            'DataBase接続文字
            Dim SQLcon = sm.getConnection
            SQLcon.Open() 'DataBase接続(Open)

            '検索SQL文
            Dim SQLStr As String = _
                    "SELECT D.MANGSHAFUKU as SHAFUKU " _
                & " FROM  S0012_SRVAUTHOR A " _
                & " INNER JOIN S0006_ROLE B " _
                & "   ON  B.CAMPCODE = A.CAMPCODE " _
                & "   and B.OBJECT   = A.OBJECT " _
                & "   and B.ROLE     = A.ROLE " _
                & "   and B.STYMD   <= @P2 " _
                & "   and B.ENDYMD  >= @P2 " _
                & "   and B.PERMITCODE >= 1 " _
                & "   and B.DELFLG  <> '1' " _
                & " INNER JOIN MA006_SHABANORG C " _
                & "   ON  C.CAMPCODE = B.CAMPCODE " _
                & "   and C.MANGUORG = B.CODE " _
                & "   and C.GSHABAN  = @P3 " _
                & "   and C.DELFLG  <> '1' " _
                & " INNER JOIN MA002_SHARYOA D " _
                & "   ON  D.CAMPCODE = C.CAMPCODE " _
                & "   and ((D.SHARYOTYPE = C.SHARYOTYPEF and D.TSHABAN = C.TSHABANF) or (D.SHARYOTYPE = C.SHARYOTYPEB and D.TSHABAN = C.TSHABANB) or (D.SHARYOTYPE = C.SHARYOTYPEB2 and D.TSHABAN = C.TSHABANB2)) " _
                & "   and D.STYMD   <= @P4 " _
                & "   and D.ENDYMD  >= @P4 " _
                & "   and D.DELFLG  <> '1' " _
                & " Where A.TERMID   = @P1 " _
                & "   and A.OBJECT   = 'SRVORG' " _
                & "   and A.STYMD   <= @P2 " _
                & "   and A.ENDYMD  >= @P2 " _
                & "   and A.DELFLG  <> '1' "

            Dim SQLcmd As New SqlCommand(SQLStr, SQLcon)
            Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.NVarChar, 20)
            Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.Date)
            Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.NVarChar, 20)
            Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", System.Data.SqlDbType.Date)

            PARA1.Value = TERMID
            PARA2.Value = Date.Now
            PARA3.Value = GSHABAN
            PARA4.Value = YMD
            Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

            ERR = C_MESSAGE_NO.NORMAL
            SHAFUKU = 0

            While SQLdr.Read
                If SQLdr("SHAFUKU") > 0 Then
                    SHAFUKU = SQLdr("SHAFUKU")
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
            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME            'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:MA002_SHARYOA Select"          '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            ERR = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try

    End Sub

End Class
