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
''' Leftボックス用従業員取得（APSRVOrg）
''' </summary>
''' <remarks></remarks>
Public Class GS0025STAFFSRVget
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
    ''' 部署コード
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property ORG() As String
    ''' <summary>
    ''' 出庫日
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property SHUKODATE() As Date
    ''' <summary>
    ''' 従業員CODE
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property STAFFCODE() As List(Of String)
    ''' <summary>
    ''' 従業員名称
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property STAFFCODENAME() As List(Of String)
    ''' <summary>
    ''' 従業員情報一覧
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property LISTBOX() As ListBox

    Protected METHOD_NAME As String = "GS0025STAFFSRVget"
    ''' <summary>
    ''' 従業員取得
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub GS0025STAFFSRVget()
        '<< エラー説明 >>
        'ERR = OK:00000,ERR:00002(環境エラー),ERR:00003(DBerr)
        '●初期処理
        STAFFCODE = New List(Of String)
        STAFFCODENAME = New List(Of String)
        'セッション制御宣言
        Dim sm As New CS0050SESSION
        'PARAM EXTRA01: USERID
        If IsNothing(USERID) Then
            USERID = sm.USERID
        End If
        'PARAM EXTRA02: ORG
        If ORG = "" Or IsNothing(ORG) Then
            ORG = sm.APSV_ORG
        End If
        'PARAM EXTRA03: SHUKODATE
        If SHUKODATE < C_DEFAULT_YMD Then
            SHUKODATE = Date.Now
        End If


        Try
            If IsNothing(LISTBOX) Then
                LISTBOX = New ListBox
            Else
                CType(LISTBOX, ListBox).Items.Clear()
            End If
        Catch ex As Exception
        End Try

        'DataBase接続文字
        Dim SQLcon = sm.getConnection
        SQLcon.Open() 'DataBase接続(Open)

        '●Leftボックス用従業員取得（APSRVOrg）
        Try
            '○ セッション変数（APSRVOrg）に紐付くデータ取得
            Dim SQLStr As String = _
                    "       SELECT rtrim(A.STAFFCODE) 	as STAFFCODE ,  " _
                & "              rtrim(B.STAFFNAMES)    as NAMES        " _
                & "         FROM MB002_STAFFORG as A                    " _
                & "   INNER JOIN MB001_STAFF    as B 				    " _
                & "           ON B.CAMPCODE     = A.CAMPCODE 			" _
                & "          and B.STAFFCODE    = A.STAFFCODE           " _
                & "          and B.STYMD       <= @P1                   " _
                & "          and B.ENDYMD      >= @P1                   " _
                & "          and B.DELFLG      <> '1' 					" _
                & "        Where A.CAMPCODE     = @P2                   " _
                & "          and A.SORG         = @P3                   " _
                & "          and A.DELFLG      <> '1'                   " _
                & "     GROUP BY A.SEQ ,A.STAFFCODE ,B.STAFFNAMES       " _
                & "     ORDER BY A.SEQ ,A.STAFFCODE                     "

            Dim SQLcmd As New SqlCommand(SQLStr, SQLcon)
            Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.Date)
            Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.NVarChar, 20)
            Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.NVarChar, 20)

            PARA1.Value = SHUKODATE
            PARA2.Value = CAMPCODE
            PARA3.Value = ORG
            Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

            While SQLdr.Read
                '○出力編集
                STAFFCODE.Add(SQLdr("STAFFCODE"))
                STAFFCODENAME.Add(SQLdr("NAMES"))
                LISTBOX.Items.Add(New ListItem(SQLdr("NAMES"), SQLdr("STAFFCODE")))
            End While

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
            CS0011LOGWRITE.INFPOSI = "DB:MB002_STAFFORG Select"         '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try
    End Sub

End Class
