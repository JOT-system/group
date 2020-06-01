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
''' 画面実行URL取得
''' </summary>
''' <remarks>遷移先のURIを取得する</remarks>
Public Structure CS0018DOURLget
    ''' <summary>
    ''' 親画面ID
    ''' </summary>
    ''' <value>画面ID</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property MAPIDP() As String

    ''' <summary>
    ''' 親画面ID用変数
    ''' </summary>
    ''' <value>変数</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property VARIP() As String

    ''' <summary>
    ''' 遷移先URL
    ''' </summary>
    ''' <value>URL</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property URL() As String

    ''' <summary>
    ''' USERID
    ''' </summary>
    ''' <value>USERID</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property USERID() As String

    ''' <summary>
    ''' ボタン名称
    ''' </summary>
    ''' <value>ボタン名称</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property NAMES() As String

    ''' <summary>
    ''' エラーコード
    ''' </summary>
    ''' <value>エラーコード</value>
    ''' <returns>0;正常、それ以外：エラー</returns>
    ''' <remarks>OK:00000,ERR:00002(環境エラー),ERR:00003(DBerr)</remarks>
    Public Property ERR() As String

    ''' <summary>
    ''' 構造体/関数名
    ''' </summary>
    ''' <remarks></remarks>
    Public Const METHOD_NAME As String = "CS0018DOURLget"

    ''' <summary>
    ''' URL取得
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub CS0018DOURLget()
        'セッション制御宣言
        Dim sm As New CS0050SESSION

        '●In PARAMチェック
        'PARAM01: MAPIDP
        If IsNothing(MAPIDP) Then
            Dim CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get

            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME               'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "MAPIDP"                         '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                   '
            CS0011LOGWRITE.TEXT = C_MESSAGE_TEXT.IN_PARAM_ERROR_TEXT
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            ERR = C_MESSAGE_NO.DLL_IF_ERROR
            Exit Sub
        End If

        'PARAM02: VARIP …任意項目
        'PARAM EXTRA01 USERID
        If IsNothing(USERID) Then
            USERID = sm.USERID
        End If
        '●変数情報取得
        '○ DB(S0008_UPROFMAP)検索

        Try
            'DataBase接続文字
            Dim SQLcon = sm.getConnection
            SQLcon.Open() 'DataBase接続(Open)

            '検索SQL文
            Dim SQLStr As String = _
                 "SELECT rtrim(B.URL) as URL , rtrim(A.NAMES) as NAMES " _
               & " FROM  S0008_UPROFMAP A " _
               & " LEFT JOIN S0009_URL B " _
               & "   ON  B.MAPID    = A.MAPID " _
               & "   and B.STYMD   <= @P4 " _
               & "   and B.ENDYMD  >= @P5 " _
               & "   and B.DELFLG  <> @P6 " _
               & " Where A.USERID   = @P1 " _
               & "   and A.MAPIDP   = @P2 " _
               & "   and A.VARIANTP = @P3 " _
               & "   and A.TITOLKBN = 'I' " _
               & "   and A.STYMD   <= @P4 " _
               & "   and A.ENDYMD  >= @P5 " _
               & "   and A.DELFLG  <> @P6 " _
               & "ORDER BY A.SEQ "
            Dim SQLcmd As New SqlCommand(SQLStr, SQLcon)
            Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.NVarChar, 20)
            Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.NVarChar, 50)
            Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.NVarChar, 50)
            Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", System.Data.SqlDbType.Date)
            Dim PARA5 As SqlParameter = SQLcmd.Parameters.Add("@P5", System.Data.SqlDbType.Date)
            Dim PARA6 As SqlParameter = SQLcmd.Parameters.Add("@P6", System.Data.SqlDbType.NVarChar, 1)
            PARA1.Value = USERID
            PARA2.Value = MAPIDP
            PARA3.Value = VARIP
            PARA4.Value = Date.Now
            PARA5.Value = Date.Now
            PARA6.Value = C_DELETE_FLG.DELETE
            Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

            ERR = C_MESSAGE_NO.DLL_IF_ERROR
            While SQLdr.Read
                ERR = C_MESSAGE_NO.NORMAL
                URL = SQLdr("URL")
                NAME = SQLdr("NAMES")
                Exit While
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

            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME
            CS0011LOGWRITE.INFPOSI = "S0008_UPROFMAP SELECT"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()

            ERR = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try

        '●デフォルトでの変数情報取得
        '○ DB(S0008_UPROFMAP)検索

        If Not isNormal(ERR) Then
            Try
                'DataBase接続文字
                Dim SQLcon = sm.getConnection
                SQLcon.Open() 'DataBase接続(Open)

                '検索SQL文
                Dim SQLStr As String = _
                     "SELECT rtrim(B.URL) as URL " _
                   & " FROM  S0008_UPROFMAP A " _
                   & " LEFT JOIN S0009_URL B " _
                   & "   ON  B.MAPID    = A.MAPID " _
                   & "   and B.STYMD   <= @P4 " _
                   & "   and B.ENDYMD  >= @P5 " _
                   & "   and B.DELFLG  <> @P6 " _
                   & " Where A.USERID   = @P1 " _
                   & "   and A.MAPIDP   = @P2 " _
                   & "   and A.VARIANTP = @P3 " _
                   & "   and A.TITOLKBN = 'I' " _
                   & "   and A.STYMD   <= @P4 " _
                   & "   and A.ENDYMD  >= @P5 " _
                   & "   and A.DELFLG  <> @P6 " _
                   & "ORDER BY A.SEQ "
                Dim SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.NVarChar, 50)
                Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.NVarChar, 50)
                Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", System.Data.SqlDbType.Date)
                Dim PARA5 As SqlParameter = SQLcmd.Parameters.Add("@P5", System.Data.SqlDbType.Date)
                Dim PARA6 As SqlParameter = SQLcmd.Parameters.Add("@P6", System.Data.SqlDbType.NVarChar, 1)
                PARA1.Value = C_DEFAULT_DATAKEY
                PARA2.Value = MAPIDP
                PARA3.Value = VARIP
                PARA4.Value = Date.Now
                PARA5.Value = Date.Now
                PARA6.Value = C_DELETE_FLG.DELETE
                Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

                ERR = C_MESSAGE_NO.DLL_IF_ERROR
                While SQLdr.Read
                    ERR = C_MESSAGE_NO.NORMAL
                    URL = SQLdr("URL")
                    Exit While
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

                CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME
                CS0011LOGWRITE.INFPOSI = "S0008_UPROFMAP SELECT"
                CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
                CS0011LOGWRITE.TEXT = ex.ToString()
                CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                CS0011LOGWRITE.CS0011LOGWrite()
                ERR = C_MESSAGE_NO.DB_ERROR
                Exit Sub
            End Try
        End If

    End Sub

End Structure
