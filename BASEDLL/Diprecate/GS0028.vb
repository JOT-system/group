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
''' 品名１取得
''' </summary>
''' <remarks>油種・品名２より取得</remarks>
Public Class GS0028PRODUCT1get
    Inherits GS0000
    ''' <summary>
    ''' 油種
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property OILTYPE() As String
    ''' <summary>
    ''' 品名２
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property PRODUCT2() As String
    ''' <summary>
    ''' 日付
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property YMD() As Date
    ''' <summary>
    ''' 品名１
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property PRODUCT1() As String

    Protected METHOD_NAME As String = "GS0028PRODUCT1get"
    ''' <summary>
    ''' 品名１取得
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub GS0028PRODUCT1get()
        '<< エラー説明 >>
        'ERR = OK:00000,ERR:00002(環境エラー),ERR:00003(DBerr)
        '●初期処理
        PRODUCT1 = ""
        'セッション制御宣言
        Dim sm As New CS0050SESSION
        'PARAM EXTRA01:YMD
        If IsNothing(YMD) Then
            YMD = Date.Now
        End If

        '●品名１取得（油種・品名２より取得）
        'DataBase接続文字
        Dim SQLcon = sm.getConnection
        SQLcon.Open() 'DataBase接続(Open)
        '検索SQL文
        '○ User権限によりDB(MC004_PRODUCT)検索
        Try
            '検索SQL文
            Dim SQLStr As String = ""
            SQLStr = _
                    "SELECT rtrim(PRODUCT1) as PRODUCT1 " _
                    & " FROM  MC004_PRODUCT " _
                    & " Where   OILTYPE     = @P1 " _
                    & "   and   PRODUCT2    = @P2 " _
                    & "   and   STYMD      <= @P3 " _
                    & "   and   ENDYMD     >= @P3 " _
                    & "   and   DELFLG     <> '1' "

            Dim SQLcmd As New SqlCommand(SQLStr, SQLcon)
            Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.NVarChar, 20)
            Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.NVarChar, 20)
            Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.Date)
            PARA1.Value = OILTYPE
            PARA2.Value = PRODUCT2
            PARA3.Value = YMD
            Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

            If SQLdr.Read Then
                PRODUCT1 = SQLdr("PRODUCT1")
            End If

            'Close
            SQLdr.Close() 'Reader(Close)
            SQLdr = Nothing

            SQLcmd.Dispose()
            SQLcmd = Nothing

            SQLcon.Close() 'DataBase接続(Close)
            SQLcon.Dispose()
            SQLcon = Nothing

            ERR = C_MESSAGE_NO.NORMAL
        Catch ex As Exception
            Dim CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get
            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME           'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:MA006_PRODUCT Select"          '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            ERR = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try


    End Sub

End Class
