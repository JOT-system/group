﻿Imports System.Data.SqlClient

''' <summary>
''' ユーザ情報を取得する
''' </summary>
''' <remarks></remarks>
Public Class CS0051UserInfo : Implements IDisposable
    ''' <summary>
    ''' ユーザID
    ''' </summary>
    Public Property USERID As String
    ''' <summary>
    ''' 範囲開始日付
    ''' </summary>
    Public Property STYMD As Date
    ''' <summary>
    ''' 範囲終了日付
    ''' </summary>
    Public Property ENDYMD As Date
    ''' <summary>
    ''' 所属会社
    ''' </summary>
    Public Property CAMPCODE As String
    ''' <summary>
    ''' 所属組織
    ''' </summary>
    Public Property ORG As String
    ''' <summary>
    ''' 社員コード
    ''' </summary>
    Public Property STAFFCODE As String
    ''' <summary>
    ''' 社員名（短）
    ''' </summary>
    Public Property STAFFNAMES As String
    ''' <summary>
    ''' 社員名（長）
    ''' </summary>
    Public Property STAFFNAMEL As String
    ''' <summary>
    ''' 初期表示画面ＩＤ
    ''' </summary>
    Public Property MAPID As String
    ''' <summary>
    ''' メニュー表示用変数
    ''' </summary>
    Public Property MAPVARI As String
    ''' <summary>
    ''' 会社権限
    ''' </summary>
    Public Property CAMPROLE As String
    ''' <summary>
    ''' 更新権限
    ''' </summary>
    Public Property MAPROLE As String
    ''' <summary>
    ''' 部署権限
    ''' </summary>
    Public Property ORGROLE As String
    ''' <summary>
    ''' 画面プロファイルID
    ''' </summary>
    Public Property VIEWPROFID As String
    ''' <summary>
    ''' 帳票プロファイルID
    ''' </summary>
    Public Property RPRTPROFID As String
    ''' <summary>
    ''' 所属サーバID
    ''' </summary>
    Public Property SERVERID As String
    ''' <summary>
    ''' 所属サーバ名称
    ''' </summary>
    Public Property SERVERNAMES As String

    ''' <summary>
    ''' 所属サーバIPアドレス
    ''' </summary>
    Public Property SERVERIP As String

    ''' <summary>
    ''' エラーメッセージ
    ''' </summary>
    Public Property ERR As String


    ''' <summary>
    ''' メソッド名
    ''' </summary>
    ''' <remarks></remarks>
    Protected Const METHOD_NAME = "getInfo"
    ''' <summary>
    ''' 取得処理
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub getInfo()
        '●In PARAMチェック
        'PARAM01:ユーザID
        If IsNothing(USERID) Then
            ERR = C_MESSAGE_NO.DLL_IF_ERROR

            Dim CS0011LOGWRITE As New CS0011LOGWrite                'LogOutput DirString Get
            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME                'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "USERID"                            '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT             'メッセージタイプ
            CS0011LOGWRITE.TEXT = C_MESSAGE_TEXT.IN_PARAM_ERROR_TEXT
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                            'ログ出力
            Exit Sub
        End If
        '●初期化処理
        CAMPCODE = ""
        ORG = ""
        STAFFCODE = ""
        STAFFNAMES = ""
        STAFFNAMEL = ""
        MAPID = ""
        MAPVARI = ""
        CAMPROLE = ""
        MAPROLE = ""
        ORGROLE = ""
        VIEWPROFID = ""
        RPRTPROFID = ""
        'セッション管理
        Dim sm As New CS0050SESSION

        'EXTRA PARAM01:STYMD
        If STYMD < C_DEFAULT_YMD Then
            STYMD = Date.Now
        End If

        'EXTRA PARAM01:ENDYMD
        If ENDYMD < C_DEFAULT_YMD Then
            ENDYMD = Date.Now
        End If

        '●ユーザ情報取得
        Try
            '****************
            '*** 共通宣言 ***
            '****************
            'DataBase接続文字
            Using SQLcon = sm.getConnection
                SQLcon.Open() 'DataBase接続(Open)


                'Message検索SQL文
                Dim SQLStr As String =
                     "SELECT " _
                   & "   rtrim(CAMPCODE) as CAMPCODE " _
                   & " , rtrim(ORG) as ORG " _
                   & " , rtrim(STAFFCODE) as STAFFCODE " _
                   & " , rtrim(STAFFNAMES) as STAFFNAMES " _
                   & " , rtrim(STAFFNAMEL) as STAFFNAMEL " _
                   & " , rtrim(MAPID) as MAPID " _
                   & " , rtrim(VARIANT) as VARIANT " _
                   & " , rtrim(CAMPROLE) as CAMPROLE " _
                   & " , rtrim(MAPROLE) as MAPROLE " _
                   & " , rtrim(ORGROLE) as ORGROLE " _
                   & " , rtrim(VIEWPROFID) as VIEWPROFID " _
                   & " , rtrim(RPRTPROFID) as RPRTPROFID " _
                   & " FROM  S0004_USER " _
                   & " Where USERID = @P1 " _
                   & "   and STYMD <= @P3 " _
                   & "   and ENDYMD >= @P2 " _
                   & "   and DELFLG <> @P4 "
                Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                    Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.NVarChar, 20)
                    Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.Date)
                    Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.Date)
                    Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", System.Data.SqlDbType.NVarChar, 1)
                    PARA1.Value = USERID
                    PARA2.Value = STYMD
                    PARA3.Value = ENDYMD
                    PARA4.Value = C_DELETE_FLG.DELETE
                    Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

                    If SQLdr.Read Then
                        CAMPCODE = SQLdr("CAMPCODE")
                        ORG = SQLdr("ORG")
                        STAFFCODE = SQLdr("STAFFCODE")
                        STAFFNAMES = SQLdr("STAFFNAMES")
                        STAFFNAMEL = SQLdr("STAFFNAMEL")
                        MAPID = SQLdr("MAPID")
                        MAPVARI = SQLdr("VARIANT")
                        CAMPROLE = SQLdr("CAMPROLE")
                        MAPROLE = SQLdr("MAPROLE")
                        ORGROLE = SQLdr("ORGROLE")
                        VIEWPROFID = SQLdr("VIEWPROFID")
                        RPRTPROFID = SQLdr("RPRTPROFID")
                        ERR = C_MESSAGE_NO.NORMAL
                    Else
                        ERR = C_MESSAGE_NO.NO_DATA_EXISTS_ERROR
                    End If

                    'Close
                    SQLdr.Close() 'Reader(Close)
                    SQLdr = Nothing
                End Using

                SQLcon.Close() 'DataBase接続(Close)
            End Using

        Catch ex As Exception

            Dim CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get

            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME                    'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:S0004_USER Select"             '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力

            ERR = C_MESSAGE_NO.DB_ERROR

            Exit Sub

        End Try
    End Sub
    ''' <summary>
    ''' 所属サーバ情報取得処理
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub BelongtoServer()
        '●In PARAMチェック
        'PARAM01:所属部署
        If IsNothing(ORG) Then
            ERR = C_MESSAGE_NO.DLL_IF_ERROR

            Dim CS0011LOGWRITE As New CS0011LOGWrite                'LogOutput DirString Get
            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME                'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "ORG"                            '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT             'メッセージタイプ
            CS0011LOGWRITE.TEXT = C_MESSAGE_TEXT.IN_PARAM_ERROR_TEXT
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                            'ログ出力
            Exit Sub
        End If
        'PARAM02:所属会社
        If IsNothing(CAMPCODE) Then
            ERR = C_MESSAGE_NO.DLL_IF_ERROR

            Dim CS0011LOGWRITE As New CS0011LOGWrite                'LogOutput DirString Get
            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME                'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "CAMPCODE"                            '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT             'メッセージタイプ
            CS0011LOGWRITE.TEXT = C_MESSAGE_TEXT.IN_PARAM_ERROR_TEXT
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                            'ログ出力
            Exit Sub
        End If
        '●初期化処理
        SERVERID = String.Empty
        SERVERNAMES = String.Empty
        SERVERIP = String.Empty
        'セッション管理
        Dim sm As New CS0050SESSION

        'EXTRA PARAM01:STYMD
        If STYMD < C_DEFAULT_YMD Then
            STYMD = Date.Now
        End If

        'EXTRA PARAM01:ENDYMD
        If ENDYMD < C_DEFAULT_YMD Then
            ENDYMD = Date.Now
        End If

        '●端末情報取得
        Try
            '****************
            '*** 共通宣言 ***
            '****************
            'DataBase接続文字
            Using SQLcon = sm.getConnection
                SQLcon.Open() 'DataBase接続(Open)


                'Message検索SQL文
                Dim SQLStr As String =
                     "SELECT " _
                   & "   rtrim(TERMID) as TERMID " _
                   & " , rtrim(IPADDR) as IPADDR " _
                   & " , rtrim(TERMNAME) as TERMNAMES " _
                   & " FROM  S0001_TERM " _
                   & " Where TERMORG    = @P1 " _
                   & "   and TERMCAMP   = @P6 " _
                   & "   and TERMCLASS  = @P5 " _
                   & "   and STYMD     <= @P3 " _
                   & "   and ENDYMD    >= @P2 " _
                   & "   and DELFLG    <> @P4 " _
                   & " ORDER BY TERMCLASS ASC "
                Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                    Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.NVarChar, 30)
                    Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.Date)
                    Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.Date)
                    Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", System.Data.SqlDbType.NVarChar, 1)
                    Dim PARA5 As SqlParameter = SQLcmd.Parameters.Add("@P5", System.Data.SqlDbType.NVarChar, 1)
                    Dim PARA6 As SqlParameter = SQLcmd.Parameters.Add("@P6", System.Data.SqlDbType.NVarChar, 20)
                    PARA1.Value = ORG
                    PARA2.Value = STYMD
                    PARA3.Value = ENDYMD
                    PARA4.Value = C_DELETE_FLG.DELETE
                    PARA5.Value = C_TERMCLASS.BASE
                    PARA6.Value = CAMPCODE

                    Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

                    If SQLdr.Read Then
                        SERVERID = SQLdr("TERMID")
                        SERVERIP = SQLdr("IPADDR")
                        SERVERNAMES = SQLdr("TERMNAMES")
                        ERR = C_MESSAGE_NO.NORMAL
                    Else
                        ERR = C_MESSAGE_NO.NO_DATA_EXISTS_ERROR
                    End If

                    'Close
                    SQLdr.Close() 'Reader(Close)
                    SQLdr = Nothing
                End Using

                SQLcon.Close() 'DataBase接続(Close)
            End Using

        Catch ex As Exception

            Dim CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get

            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME                    'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:S0001_TERM Select"             '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力

            ERR = C_MESSAGE_NO.DB_ERROR

            Exit Sub

        End Try
    End Sub
    ''' <summary>
    ''' 解放処理
    ''' </summary>
    Protected Sub Dispose() Implements IDisposable.Dispose
        Dispose(True)
        'GC.SuppressFinalize(Me)
    End Sub

    ''' <summary>
    ''' 解放処理
    ''' </summary>
    Protected Sub Dispose(ByVal isDispose As Boolean)
        If isDispose Then

        End If
    End Sub
End Class


