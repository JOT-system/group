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
''' 変数取得
''' </summary>
''' <remarks></remarks>
Public Structure CS0016VARIget

    ''' <summary>
    ''' 画面ID
    ''' </summary>
    ''' <value>画面ID</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property MAPID() As String

    ''' <summary>
    ''' ユーザID
    ''' </summary>
    ''' <value>ユーザID</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property USERID As String

    ''' <summary>
    ''' 会社コード
    ''' </summary>
    ''' <value>会社コード</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property CAMPCODE() As String

    ''' <summary>
    ''' 変数
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property VARI() As String

    ''' <summary>
    ''' 項目
    ''' </summary>
    ''' <value>項目</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property FIELD() As String

    ''' <summary>
    ''' 設定値
    ''' </summary>
    ''' <value>設定値</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property VALUE() As String

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
    Public Const METHOD_NAME As String = "CS0016VARIget"

    ''' <summary>
    ''' ユーザプロファイルの変数設定値を取得する
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub CS0016VARIget()
        'セッション制御宣言
        Dim sm As New CS0050SESSION
        '●In PARAMチェック
        'PARAM01: MAPID
        If IsNothing(MAPID) Then
            Dim CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get

            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME                'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "MAPID"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = C_MESSAGE_TEXT.IN_PARAM_ERROR_TEXT
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End If

        'PARAM02: CAMPCODE

        'PARAM03: VARI
        If IsNothing(VARI) Then
            Dim CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get

            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME                'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "VARI"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = C_MESSAGE_TEXT.IN_PARAM_ERROR_TEXT
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End If

        'PARAM04: FIELD
        If IsNothing(FIELD) Then
            Dim CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get

            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME               'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "FIELD"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = C_MESSAGE_TEXT.IN_PARAM_ERROR_TEXT
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End If
        'PARAM EXTRA01 : USERID
        If IsNothing(USERID) Then
            USERID = sm.USERID
        End If
        '●変数情報取得
        '○ DB(S0007_UPROFVARI)検索
        Try
            '○指定ﾊﾟﾗﾒｰﾀで検索
            'DataBase接続文字
            Dim SQLcon = sm.getConnection
            SQLcon.Open() 'DataBase接続(Open)

            'I_CAMPCODE検索SQL文
            Dim SQL_Str As String = ""
            If CAMPCODE = "" Then
                SQL_Str = _
                     "SELECT rtrim(USERID) as USERID , rtrim(MAPID) as MAPID , rtrim(CAMPCODE) as CAMPCODE , rtrim(VARIANT) as VARIANT , rtrim(TITOLKBN) as TITOLKBN , SEQ , rtrim(FIELD) as FIELD , STYMD , ENDYMD , rtrim(VARIANTNAMES) as VARIANTNAMES , rtrim(TITOL) as TITOL , rtrim(VALUETYPE) as VALUETYPE , rtrim(VALUE) as VALUE , VALUEADDYY , VALUEADDMM , VALUEADDDD , rtrim(DELFLG) as DELFLG " _
                   & " FROM  S0007_UPROFVARI " _
                   & " Where USERID   = @P1 " _
                   & "   and MAPID    = @P2 " _
                   & "   and VARIANT  = @P4 " _
                   & "   and TITOLKBN = 'I' " _
                   & "   and FIELD    = @P5 " _
                   & "   and STYMD   <= @P6 " _
                   & "   and ENDYMD  >= @P7 " _
                   & "   and DELFLG  <> @P8 "
            Else
                SQL_Str = _
                     "SELECT rtrim(USERID) as USERID , rtrim(MAPID) as MAPID , rtrim(CAMPCODE) as CAMPCODE , rtrim(VARIANT) as VARIANT , rtrim(TITOLKBN) as TITOLKBN , SEQ , rtrim(FIELD) as FIELD , STYMD , ENDYMD , rtrim(VARIANTNAMES) as VARIANTNAMES , rtrim(TITOL) as TITOL , rtrim(VALUETYPE) as VALUETYPE , rtrim(VALUE) as VALUE , VALUEADDYY , VALUEADDMM , VALUEADDDD , rtrim(DELFLG) as DELFLG " _
                   & " FROM  S0007_UPROFVARI " _
                   & " Where USERID   = @P1 " _
                   & "   and MAPID    = @P2 " _
                   & "   and CAMPCODE = @P3 " _
                   & "   and VARIANT  = @P4 " _
                   & "   and TITOLKBN = 'I' " _
                   & "   and FIELD    = @P5 " _
                   & "   and STYMD   <= @P6 " _
                   & "   and ENDYMD  >= @P7 " _
                   & "   and DELFLG  <> @P8 "
            End If

            Dim SQLcmd As New SqlCommand(SQL_Str, SQLcon)
            Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.NVarChar, 20)
            Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.NVarChar, 50)
            Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.NVarChar, 20)
            Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", System.Data.SqlDbType.NVarChar, 50)
            Dim PARA5 As SqlParameter = SQLcmd.Parameters.Add("@P5", System.Data.SqlDbType.NVarChar, 50)
            Dim PARA6 As SqlParameter = SQLcmd.Parameters.Add("@P6", System.Data.SqlDbType.Date)
            Dim PARA7 As SqlParameter = SQLcmd.Parameters.Add("@P7", System.Data.SqlDbType.Date)
            Dim PARA8 As SqlParameter = SQLcmd.Parameters.Add("@P8", System.Data.SqlDbType.NVarChar, 1)
            PARA1.Value = USERID
            PARA2.Value = MAPID
            PARA3.Value = CAMPCODE
            PARA4.Value = VARI
            PARA5.Value = FIELD
            PARA6.Value = Date.Now
            PARA7.Value = Date.Now
            PARA8.Value = C_DELETE_FLG.DELETE
            Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

            VALUE = ""
            ERR = C_MESSAGE_NO.DLL_IF_ERROR

            Dim WW_DATE As Date = Date.Now
            While SQLdr.Read
                Select Case SQLdr("VALUETYPE")
                    Case "DATES"
                        WW_DATE = New DateTime(Date.Now.Year, Date.Now.Month, 1)
                        If SQLdr("VALUEADDYY") <> 0 Then
                            WW_DATE = WW_DATE.AddYears(SQLdr("VALUEADDYY"))
                        End If
                        If SQLdr("VALUEADDMM") <> 0 Then
                            WW_DATE = WW_DATE.AddMonths(SQLdr("VALUEADDMM"))
                        End If
                        If SQLdr("VALUEADDDD") <> 0 Then
                            WW_DATE = WW_DATE.AddDays(SQLdr("VALUEADDDD"))
                        End If
                        VALUE = WW_DATE.ToString("yyyy/MM/dd")
                        ERR = C_MESSAGE_NO.NORMAL
                    Case "DATENOW"
                        WW_DATE = Date.Now
                        If SQLdr("VALUEADDYY") <> 0 Then
                            WW_DATE = WW_DATE.AddYears(SQLdr("VALUEADDYY"))
                        End If
                        If SQLdr("VALUEADDMM") <> 0 Then
                            WW_DATE = WW_DATE.AddMonths(SQLdr("VALUEADDMM"))
                        End If
                        If SQLdr("VALUEADDDD") <> 0 Then
                            WW_DATE = WW_DATE.AddDays(SQLdr("VALUEADDDD"))
                        End If
                        VALUE = WW_DATE.ToString("yyyy/MM/dd")
                        ERR = C_MESSAGE_NO.NORMAL
                    Case "DATEFIX"
                        Try
                            Date.TryParse(SQLdr("VALUE"), WW_DATE)
                        Catch ex As Exception
                            Exit Sub
                        End Try
                        If SQLdr("VALUEADDYY") <> 0 Then
                            WW_DATE = WW_DATE.AddYears(SQLdr("VALUEADDYY"))
                        End If
                        If SQLdr("VALUEADDMM") <> 0 Then
                            WW_DATE = WW_DATE.AddMonths(SQLdr("VALUEADDMM"))
                        End If
                        If SQLdr("VALUEADDDD") <> 0 Then
                            WW_DATE = WW_DATE.AddDays(SQLdr("VALUEADDDD"))
                        End If
                        VALUE = WW_DATE.ToString("yyyy/MM/dd")
                        ERR = C_MESSAGE_NO.NORMAL
                    Case Else
                        VALUE = SQLdr("VALUE")
                        ERR = C_MESSAGE_NO.NORMAL
                End Select

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

            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME               'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:S0007_UPROFVARI Select"             '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                   '
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力

            ERR = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try

        '○DefaultユーザID、Default変数名のﾊﾟﾗﾒｰﾀで再検索
        'DataBase接続文字
        If Not isNormal(ERR) Then
            Try
                Dim SQLcon = sm.getConnection
                SQLcon.Open() 'DataBase接続(Open)

                'I_CAMPCODE検索SQL文
                Dim SQL_Str As String = ""

                If CAMPCODE = "" Then
                    SQL_Str = _
                         "SELECT rtrim(USERID) as USERID , rtrim(MAPID) as MAPID , rtrim(CAMPCODE) as CAMPCODE , rtrim(VARIANT) as VARIANT , rtrim(TITOLKBN) as TITOLKBN , SEQ , rtrim(FIELD) as FIELD , STYMD , ENDYMD , rtrim(VARIANTNAMES) as VARIANTNAMES , rtrim(TITOL) as TITOL , rtrim(VALUETYPE) as VALUETYPE , rtrim(VALUE) as VALUE , VALUEADDYY , VALUEADDMM , VALUEADDDD , rtrim(DELFLG) as DELFLG " _
                       & " FROM  S0007_UPROFVARI " _
                       & " Where USERID   = @P1 " _
                       & "   and MAPID    = @P2 " _
                       & "   and VARIANT  = @P4 " _
                       & "   and TITOLKBN = 'I' " _
                       & "   and FIELD    = @P5 " _
                       & "   and STYMD   <= @P6 " _
                       & "   and ENDYMD  >= @P7 " _
                       & "   and DELFLG  <> @P8 "
                Else
                    SQL_Str = _
                         "SELECT rtrim(USERID) as USERID , rtrim(MAPID) as MAPID , rtrim(CAMPCODE) as CAMPCODE , rtrim(VARIANT) as VARIANT , rtrim(TITOLKBN) as TITOLKBN , SEQ , rtrim(FIELD) as FIELD , STYMD , ENDYMD , rtrim(VARIANTNAMES) as VARIANTNAMES , rtrim(TITOL) as TITOL , rtrim(VALUETYPE) as VALUETYPE , rtrim(VALUE) as VALUE , VALUEADDYY , VALUEADDMM , VALUEADDDD , rtrim(DELFLG) as DELFLG " _
                       & " FROM  S0007_UPROFVARI " _
                       & " Where USERID   = @P1 " _
                       & "   and MAPID    = @P2 " _
                       & "   and CAMPCODE = @P3 " _
                       & "   and VARIANT  = @P4 " _
                       & "   and TITOLKBN = 'I' " _
                       & "   and FIELD    = @P5 " _
                       & "   and STYMD   <= @P6 " _
                       & "   and ENDYMD  >= @P7 " _
                       & "   and DELFLG  <> @P8 "
                End If
                Dim SQLcmd As New SqlCommand(SQL_Str, SQLcon)
                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.NVarChar, 50)
                Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", System.Data.SqlDbType.NVarChar, 50)
                Dim PARA5 As SqlParameter = SQLcmd.Parameters.Add("@P5", System.Data.SqlDbType.NVarChar, 50)
                Dim PARA6 As SqlParameter = SQLcmd.Parameters.Add("@P6", System.Data.SqlDbType.Date)
                Dim PARA7 As SqlParameter = SQLcmd.Parameters.Add("@P7", System.Data.SqlDbType.Date)
                Dim PARA8 As SqlParameter = SQLcmd.Parameters.Add("@P8", System.Data.SqlDbType.NVarChar, 1)
                PARA1.Value = C_DEFAULT_DATAKEY
                PARA2.Value = MAPID
                PARA3.Value = C_DEFAULT_DATAKEY
                PARA4.Value = VARI
                PARA5.Value = FIELD
                PARA6.Value = Date.Now
                PARA7.Value = Date.Now
                PARA8.Value = C_DELETE_FLG.DELETE
                Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

                VALUE = ""
                ERR = C_MESSAGE_NO.DLL_IF_ERROR

                Dim WW_DATE As Date = Date.Now
                While SQLdr.Read
                    Select Case SQLdr("VALUETYPE")
                        Case "DATES"
                            WW_DATE = New DateTime(Date.Now.Year, Date.Now.Month, 1)
                            If SQLdr("VALUEADDYY") <> 0 Then
                                WW_DATE = WW_DATE.AddYears(SQLdr("VALUEADDYY"))
                            End If
                            If SQLdr("VALUEADDMM") <> 0 Then
                                WW_DATE = WW_DATE.AddMonths(SQLdr("VALUEADDMM"))
                            End If
                            If SQLdr("VALUEADDDD") <> 0 Then
                                WW_DATE = WW_DATE.AddDays(SQLdr("VALUEADDDD"))
                            End If
                            VALUE = WW_DATE.ToString("yyyy/MM/dd")
                            ERR = C_MESSAGE_NO.NORMAL
                        Case "DATENOW"
                            WW_DATE = Date.Now
                            If SQLdr("VALUEADDYY") <> 0 Then
                                WW_DATE = WW_DATE.AddYears(SQLdr("VALUEADDYY"))
                            End If
                            If SQLdr("VALUEADDMM") <> 0 Then
                                WW_DATE = WW_DATE.AddMonths(SQLdr("VALUEADDMM"))
                            End If
                            If SQLdr("VALUEADDDD") <> 0 Then
                                WW_DATE = WW_DATE.AddDays(SQLdr("VALUEADDDD"))
                            End If
                            VALUE = WW_DATE.ToString("yyyy/MM/dd")
                            ERR = C_MESSAGE_NO.NORMAL
                        Case "DATEFIX"
                            Try
                                Date.TryParse(SQLdr("VALUE"), WW_DATE)
                            Catch ex As Exception
                                Exit Sub
                            End Try
                            If SQLdr("VALUEADDYY") <> 0 Then
                                WW_DATE = WW_DATE.AddYears(SQLdr("VALUEADDYY"))
                            End If
                            If SQLdr("VALUEADDMM") <> 0 Then
                                WW_DATE = WW_DATE.AddMonths(SQLdr("VALUEADDMM"))
                            End If
                            If SQLdr("VALUEADDDD") <> 0 Then
                                WW_DATE = WW_DATE.AddDays(SQLdr("VALUEADDDD"))
                            End If
                            VALUE = WW_DATE.ToString("yyyy/MM/dd")
                            ERR = C_MESSAGE_NO.NORMAL
                        Case Else
                            VALUE = SQLdr("VALUE")
                            ERR = C_MESSAGE_NO.NORMAL
                    End Select

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

                CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME               'SUBクラス名
                CS0011LOGWRITE.INFPOSI = "DB:S0007_UPROFVARI Select"             '
                CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                   '
                CS0011LOGWRITE.TEXT = ex.ToString()
                CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力

                ERR = C_MESSAGE_NO.DB_ERROR
                Exit Sub
            End Try
        End If

    End Sub

End Structure
