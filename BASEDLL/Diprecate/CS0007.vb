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
''' 更新権限チェック（画面 APSRVチェック有）
''' </summary>
''' <remarks></remarks>
Public Structure CS0007AUTHORmap

    ''' <summary>
    ''' 権限チェックを行う画面ID
    ''' </summary>
    ''' <value>画面ID</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property MAPID() As String

    ''' <summary>
    ''' 権限チェックを行うユーザID
    ''' </summary>
    ''' <value>ユーザID</value>
    ''' <returns></returns>
    ''' <remarks>未設定時はセッションから取得する</remarks>
    Public Property USERID As String

    ''' <summary>
    ''' 権限チェックを行う端末ID
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks>未設定時はセッションから取得する</remarks>
    Public Property TERMID As String
    ''' <summary>
    ''' 権限結果
    ''' </summary>
    ''' <value>権限コード</value>
    ''' <returns>0：権限無　１：参照権限　２：参照更新権限</returns>
    ''' <remarks></remarks>
    Public Property MAPPERMITCODE() As String

    ''' <summary>
    ''' エラーコード
    ''' </summary>
    ''' <value>エラーコード</value>
    ''' <returns>0;正常、それ以外：エラー</returns>
    ''' <remarks>OK:00000,ERR:00002(Customize),ERR:00003(DBerr),ERR:10003(権限エラー)</remarks>
    Public Property ERR() As String

    ''' <summary>
    ''' 構造体/関数名
    ''' </summary>
    ''' <remarks></remarks>
    Public Const METHOD_NAME As String = "CS0007AUTHORmap"

    ''' <summary>
    ''' 各画面の更新権限情報を取得する
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub CS0007AUTHORmap()
        Dim sm As CS0050SESSION = New CS0050SESSION()
        '●In PARAMチェック
        'PARAM01: MAPID
        If IsNothing(MAPID) Then
            ERR = C_MESSAGE_NO.DLL_IF_ERROR

            Dim CS0011LOGWRITE As New CS0011LOGWrite            'LogOutput DirString Get
            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME            'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "MAPID"                  '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                         '
            CS0011LOGWRITE.TEXT = C_MESSAGE_TEXT.IN_PARAM_ERROR_TEXT
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                     'ログ出力
            Exit Sub
        End If
        'PARAM EXTRA 01: USERID
        If IsNothing(USERID) Then
            USERID = sm.USERID
        End If

        'PARAM EXTRA 01: TERMID
        If IsNothing(TERMID) Then
            TERMID = sm.APSV_ID
        End If

        Dim WW_USER_PERMIT As String = " "
        Dim WW_SRV_PERMIT As String = " "

        '●権限チェック（画面）　…　ユーザ操作権限取得

        MAPPERMITCODE = ""

        Try
            'DataBase接続文字
            Dim SQLcon = sm.getConnection
            SQLcon.Open() 'DataBase接続(Open)

            '検索SQL文
            Dim SQLStr As String = _
                 "SELECT rtrim(B.PERMITCODE) as PERMITCODE " _
               & " FROM  S0005_AUTHOR A " _
               & " INNER JOIN S0006_ROLE B " _
               & "   ON  B.OBJECT   = A.OBJECT " _
               & "   and B.ROLE     = A.ROLE " _
               & "   and B.STYMD   <= @P4 " _
               & "   and B.ENDYMD  >= @P5 " _
               & "   and B.DELFLG  <> @P6 " _
               & " Where A.USERID   = @P1 " _
               & "   and A.OBJECT   = @P2 " _
               & "   and B.CODE     = @P3 " _
               & "   and A.STYMD   <= @P4 " _
               & "   and A.ENDYMD  >= @P5 " _
               & "   and A.DELFLG  <> @P6 " _
               & "ORDER BY B.SEQ "

            Dim SQLcmd As New SqlCommand(SQLStr, SQLcon)
            Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.NVarChar, 20)
            Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.NVarChar, 20)
            Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.NVarChar, 20)
            Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", System.Data.SqlDbType.Date)
            Dim PARA5 As SqlParameter = SQLcmd.Parameters.Add("@P5", System.Data.SqlDbType.Date)
            Dim PARA6 As SqlParameter = SQLcmd.Parameters.Add("@P6", System.Data.SqlDbType.NVarChar, 1)
            PARA1.Value = USERID
            PARA2.Value = C_ROLE_VARIANT.USER_PERTMIT
            PARA3.Value = MAPID
            PARA4.Value = Date.Now
            PARA5.Value = Date.Now
            PARA6.Value = C_DELETE_FLG.DELETE
            Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

            '権限コード初期値(権限なし)設定
            MAPPERMITCODE = ""

            ERR = C_MESSAGE_NO.AUTHORIZATION_ERROR

            While SQLdr.Read
                WW_USER_PERMIT = SQLdr("PERMITCODE").ToString
                ERR = C_MESSAGE_NO.NORMAL
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

            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME              'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:S0005_AUTHOR Select"           '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                   '
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力

            ERR = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try

        '●権限チェック（画面）　…　サーバ操作権限取得

        MAPPERMITCODE = ""

        Try
            'DataBase接続文字
            Dim SQLcon = sm.getConnection
            SQLcon.Open() 'DataBase接続(Open)

            '検索SQL文
            Dim SQLStr As String = _
                 "SELECT rtrim(B.PERMITCODE) as PERMITCODE " _
               & " FROM  S0012_SRVAUTHOR A " _
               & " INNER JOIN S0006_ROLE B " _
               & "   ON  B.OBJECT   = A.OBJECT " _
               & "   and B.ROLE     = A.ROLE " _
               & "   and B.STYMD   <= @P4 " _
               & "   and B.ENDYMD  >= @P5 " _
               & "   and B.DELFLG  <> @P6 " _
               & " Where A.TERMID   = @P1 " _
               & "   and A.OBJECT   = @P2 " _
               & "   and B.CODE     = @P3 " _
               & "   and A.STYMD   <= @P4 " _
               & "   and A.ENDYMD  >= @P5 " _
               & "   and A.DELFLG  <> @P6 " _
               & "ORDER BY B.SEQ "

            Dim SQLcmd As New SqlCommand(SQLStr, SQLcon)
            Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.NVarChar, 30)
            Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.NVarChar, 20)
            Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.NVarChar, 20)
            Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", System.Data.SqlDbType.Date)
            Dim PARA5 As SqlParameter = SQLcmd.Parameters.Add("@P5", System.Data.SqlDbType.Date)
            Dim PARA6 As SqlParameter = SQLcmd.Parameters.Add("@P6", System.Data.SqlDbType.NVarChar, 1)
            PARA1.Value = TERMID
            PARA2.Value = C_ROLE_VARIANT.SERV_PERTMIT
            PARA3.Value = MAPID
            PARA4.Value = Date.Now
            PARA5.Value = Date.Now
            PARA6.Value = C_DELETE_FLG.DELETE
            Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

            '権限コード初期値(権限なし)設定
            MAPPERMITCODE = ""

            ERR = C_MESSAGE_NO.AUTHORIZATION_ERROR

            While SQLdr.Read
                WW_SRV_PERMIT = SQLdr("PERMITCODE").ToString
                ERR = C_MESSAGE_NO.NORMAL
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

            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME              'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:S0005_AUTHOR Select"           '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                 '
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力

            ERR = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try

        '権限コード判定
        If isNormal(ERR) Then
            MAPPERMITCODE = C_PERMISSION.REFERLANCE
            Select Case (WW_SRV_PERMIT & WW_USER_PERMIT)
                Case "00"
                    MAPPERMITCODE = C_PERMISSION.INVALID
                Case "01"
                    MAPPERMITCODE = C_PERMISSION.INVALID
                Case "02"
                    MAPPERMITCODE = C_PERMISSION.INVALID
                Case "10"
                    MAPPERMITCODE = C_PERMISSION.INVALID
                Case "11"
                    MAPPERMITCODE = C_PERMISSION.REFERLANCE
                Case "12"
                    MAPPERMITCODE = C_PERMISSION.REFERLANCE
                Case "20"
                    MAPPERMITCODE = C_PERMISSION.INVALID
                Case "21"
                    MAPPERMITCODE = C_PERMISSION.REFERLANCE
                Case "22"
                    MAPPERMITCODE = C_PERMISSION.UPDATE
            End Select
        End If

    End Sub

End Structure
