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
''' 画面戻先URL取得
''' </summary>
''' <remarks>遷移先のURIを取得する</remarks>
Public Structure CS0017RETURNURLget

    ''' <summary>
    ''' 画面ID
    ''' </summary>
    ''' <value>画面ID</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property MAPID() As String

    ''' <summary>
    ''' 変数
    ''' </summary>
    ''' <value>変数</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property VARI() As String

    ''' <summary>
    ''' USERID
    ''' </summary>
    ''' <value>USERID</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property USERID() As String

    ''' <summary>
    ''' 遷移先URL
    ''' </summary>
    ''' <value>URL</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property URL() As String

    ''' <summary>
    ''' 画面戻先変数
    ''' </summary>
    ''' <value>画面戻先変数</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property VARI_RETURN() As String

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
    Public Const METHOD_NAME As String = "CS0017RETURNURLget"

    ''' <summary>
    ''' URL取得
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub CS0017RETURNURLget()

        '●In PARAMチェック
        'PARAM01: MAPID
        If IsNothing(MAPID) Then
            Dim CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get

            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME           'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "MAPID"                          '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                   '
            CS0011LOGWRITE.TEXT = C_MESSAGE_TEXT.IN_PARAM_ERROR_TEXT
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力

            ERR = C_MESSAGE_NO.DLL_IF_ERROR
            Exit Sub
        End If

        'PARAM02: VARI …任意項目

        'セッション制御宣言
        Dim sm As New CS0050SESSION

        'PARAM EXTRA01 USERID
        If IsNothing(USERID) Then
            USERID = sm.USERID
        End If

        '●画面戻先URL取得
        '○ DB(S0009_URL)検索

        Try
            'DataBase接続文字
            Dim SQLcon = sm.getConnection
            SQLcon.Open() 'DataBase接続(Open)

            '検索SQL文
            Dim SQLStr As String = _
                 "SELECT rtrim(A.MAPIDP) as MAPIDP , rtrim(A.VARIANTP) as VARIANTP , rtrim(A.NAMES) as NAMES , rtrim(B.URL) as URL " _
               & " FROM  S0008_UPROFMAP A " _
               & " INNER JOIN S0009_URL B " _
               & "   ON  B.MAPID     = A.MAPIDP " _
               & "   and B.STYMD    <= @P4 " _
               & "   and B.ENDYMD   >= @P5 " _
               & "   and B.DELFLG   <> @P6 " _
               & " Where A.USERID    = @P1 " _
               & "   and A.MAPID     = @P2 " _
               & "   and A.VARIANT   = @P3 " _
               & "   and A.TITOLKBN  = 'I' " _
               & "   and A.STYMD    <= @P4 " _
               & "   and A.ENDYMD   >= @P5 " _
               & "   and A.DELFLG   <> @P6 " _
               & "ORDER BY A.SEQ "
            Dim SQLcmd As New SqlCommand(SQLStr, SQLcon)
            Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.NVarChar, 20)
            Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.NVarChar, 50)
            Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.NVarChar, 50)
            Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", System.Data.SqlDbType.Date)
            Dim PARA5 As SqlParameter = SQLcmd.Parameters.Add("@P5", System.Data.SqlDbType.Date)
            Dim PARA6 As SqlParameter = SQLcmd.Parameters.Add("@P6", System.Data.SqlDbType.NVarChar, 1)
            PARA1.Value = USERID
            PARA2.Value = MAPID
            PARA3.Value = VARI
            PARA4.Value = Date.Now
            PARA5.Value = Date.Now
            PARA6.Value = C_DELETE_FLG.DELETE
            Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

            ERR = C_MESSAGE_NO.DLL_IF_ERROR
            While SQLdr.Read
                ERR = C_MESSAGE_NO.NORMAL
                URL = SQLdr("URL")
                VARI_RETURN = SQLdr("VARIANTP")
                NAMES = SQLdr("NAMES")
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
            CS0011LOGWRITE.INFPOSI = "S0008_UPROFMAP SELECT (" & USERID & " " & MAPID & " " & VARI & ")"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()
            ERR = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try

        '●デフォルトでの画面戻先URL取得
        '○ DB(S0009_URL)検索
        If ERR = C_MESSAGE_NO.DLL_IF_ERROR Then
            Try
                'DataBase接続文字
                Dim SQLcon = sm.getConnection
                SQLcon.Open() 'DataBase接続(Open)

                '検索SQL文
                Dim SQLStr As String = _
                     "SELECT rtrim(A.MAPIDP) as MAPIDP , rtrim(A.VARIANTP) as VARIANTP , rtrim(A.NAMES) as NAMES , rtrim(B.URL) as URL " _
                   & " FROM  S0008_UPROFMAP A " _
                   & " INNER JOIN S0009_URL B " _
                   & "   ON  B.MAPID     = A.MAPIDP " _
                   & "   and B.STYMD    <= @P4 " _
                   & "   and B.ENDYMD   >= @P5 " _
                   & "   and B.DELFLG   <> @P6 " _
                   & " Where A.USERID    = @P1 " _
                   & "   and A.MAPID     = @P2 " _
                   & "   and A.VARIANT   = @P3 " _
                   & "   and A.TITOLKBN  = 'I' " _
                   & "   and A.STYMD    <= @P4 " _
                   & "   and A.ENDYMD   >= @P5 " _
                   & "   and A.DELFLG   <> @P6 " _
                   & "ORDER BY A.SEQ "
                Dim SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.NVarChar, 50)
                Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.NVarChar, 50)
                Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", System.Data.SqlDbType.Date)
                Dim PARA5 As SqlParameter = SQLcmd.Parameters.Add("@P5", System.Data.SqlDbType.Date)
                Dim PARA6 As SqlParameter = SQLcmd.Parameters.Add("@P6", System.Data.SqlDbType.NVarChar, 1)
                PARA1.Value = C_DEFAULT_DATAKEY
                PARA2.Value = MAPID
                PARA3.Value = VARI
                PARA4.Value = Date.Now
                PARA5.Value = Date.Now
                PARA6.Value = C_DELETE_FLG.DELETE
                Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

                ERR = C_MESSAGE_NO.DLL_IF_ERROR
                While SQLdr.Read
                    ERR = C_MESSAGE_NO.NORMAL
                    URL = SQLdr("URL")
                    VARI_RETURN = SQLdr("VARIANTP")
                    NAMES = SQLdr("NAMES")
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
                CS0011LOGWRITE.INFPOSI = "S0008_UPROFMAP SELECT (Default" & " " & MAPID & " " & VARI & ")"
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
