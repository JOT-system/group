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
''' ユーザプロファイル（Gridソート文字列）取得
''' </summary>
''' <remarks>廃止予定</remarks>
Public Structure CS0026TBLSORTget

    'ユーザプロファイル（Gridソート文字列）取得dll Interface
    Private I_MAPID As String                 'PARAM01:MAPID
    Private I_VARIANT As String               'PARAM02:変数
    Private I_TAB As String                   'PARAM03:タブ
    Private O_ERR As String                   'PARAM04:ERRNo
    Private O_SORTSTR As String               'PARAM05:テーブルソート用文字列

    Public Property MAPID() As String
        Get
            Return I_MAPID
        End Get
        Set(ByVal Value As String)
            I_MAPID = Value
        End Set
    End Property

    Public Property VARI() As String
        Get
            Return I_VARIANT
        End Get
        Set(ByVal Value As String)
            I_VARIANT = Value
        End Set
    End Property

    Public Property TAB() As String
        Get
            Return I_TAB
        End Get
        Set(ByVal Value As String)
            I_TAB = Value
        End Set
    End Property

    Public Property ERR() As String
        Get
            Return O_ERR
        End Get
        Set(ByVal Value As String)
            O_ERR = Value
        End Set
    End Property

    Public Property SORTSTR() As String
        Get
            Return O_SORTSTR
        End Get
        Set(ByVal Value As String)
            O_SORTSTR = Value
        End Set
    End Property

    Public Sub CS0026TBLSORTget()
        '<< エラー説明 >>
        'O_ERR = OK:00000,ERR:00002(環境エラー),ERR:00003(DBerr)

        '●In PARAMチェック
        'PARAM01: I_MAPID
        If IsNothing(I_MAPID) Then
            O_ERR = "00002"

            Dim CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get
            CS0011LOGWRITE.INFSUBCLASS = "CS0026TBLSORTget"             'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "I_MAPID"                           '
            CS0011LOGWRITE.NIWEA = "A"                                  '
            CS0011LOGWRITE.TEXT = "システム管理者へ連絡して下さい(In PARAM Err)"
            CS0011LOGWRITE.MESSAGENO = "00002"
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End If

        'PARAM02: I_VARIANT
        If IsNothing(I_VARIANT) Then
            O_ERR = "00002"

            Dim CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get
            CS0011LOGWRITE.INFSUBCLASS = "CS0026TBLSORTget"             'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "I_VARIANT"                           '
            CS0011LOGWRITE.NIWEA = "A"                                  '
            CS0011LOGWRITE.TEXT = "システム管理者へ連絡して下さい(In PARAM Err)"
            CS0011LOGWRITE.MESSAGENO = "00002"
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End If

        'PARAM03: I_TAB

        '●初期処理
        O_ERR = "00002"
        O_SORTSTR = ""

        '●ユーザプロファイル（Gridソート文字列）取得
        'ユーザプロファイル（ビュー）… 個別設定値を検索
        Try
            'DataBase接続文字
            Dim SQLcon As New SqlConnection(HttpContext.Current.Session("DBcon"))
            SQLcon.Open() 'DataBase接続(Open)

            '検索SQL文
            Dim SQLStr As String = _
                 "SELECT FIELD , EFFECT " _
               & " FROM  S0010_UPROFVIEW  " _
               & " Where USERID   = @P1 " _
               & "   and MAPID    = @P2 " _
               & "   and VARIANT  = @P3 " _
               & "   and HDKBN    = 'H' " _
               & "   and TAB      = @P4 " _
               & "   and TITOLKBN = 'I' " _
               & "   and STYMD   <= @P5 " _
               & "   and ENDYMD  >= @P5 " _
               & "   and SORT     > 0 " _
               & "   and DELFLG  <> '1' " _
               & "ORDER BY SORT  "
            '(説明 S0010_UPROFVIEW)　…　画面一覧データの特定方法
            '   HD区分=H(ヘッダー)の場合、TABフィールドは有効(複数TAB対応)。※左右位置は無効フィールド。
            '   HD区分=D(ディテール)の場合、TABフィールド(複数TAB対応)および左右位置は有効。

            Dim SQLcmd As New SqlCommand(SQLStr, SQLcon)
            Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.Char, 20)
            Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.Char, 50)
            Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.Char, 50)
            Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", System.Data.SqlDbType.Char, 20)
            Dim PARA5 As SqlParameter = SQLcmd.Parameters.Add("@P5", System.Data.SqlDbType.Date)
            PARA1.Value = HttpContext.Current.Session("Userid")
            PARA2.Value = I_MAPID
            PARA3.Value = I_VARIANT
            PARA4.Value = I_TAB
            PARA5.Value = Date.Now
            Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

            'GridViewの列項目作成
            While SQLdr.Read
                If SQLdr("EFFECT") = "Y" Then
                    If O_SORTSTR = "" Then
                        O_SORTSTR = O_SORTSTR & SQLdr("FIELD")
                    Else
                        O_SORTSTR = O_SORTSTR & " , " & SQLdr("FIELD")
                    End If
                End If

                O_ERR = "00000"
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

            CS0011LOGWRITE.INFSUBCLASS = "CS0026TBLSORTget"             'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:S0010_UPROFVIEW Select"        '
            CS0011LOGWRITE.NIWEA = "A"                                  '
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = "00003"
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try

        'ユーザプロファイル（Gridソート文字列）取得… デフォルト値を検索
        If O_ERR <> "00000" Then
            Try
                'DataBase接続文字
                Dim SQLcon As New SqlConnection(HttpContext.Current.Session("DBcon"))
                SQLcon.Open() 'DataBase接続(Open)

                '検索SQL文
                Dim SQLStr As String = _
                     "SELECT FIELD , EFFECT " _
                   & " FROM  S0010_UPROFVIEW  " _
                   & " Where USERID   = @P1 " _
                   & "   and MAPID    = @P2 " _
                   & "   and VARIANT  = @P3 " _
                   & "   and HDKBN    = 'H' " _
                   & "   and TAB      = @P4 " _
                   & "   and TITOLKBN = 'I' " _
                   & "   and STYMD   <= @P5 " _
                   & "   and ENDYMD  >= @P5 " _
                   & "   and SORT     > 0 " _
                   & "   and DELFLG  <> '1' " _
                   & "ORDER BY SORT  "
                '(説明 S0010_UPROFVIEW)　…　画面一覧データの特定方法
                '   HD区分=H(ヘッダー)の場合、TABフィールドは有効(複数TAB対応)。※左右位置は無効フィールド。
                '   HD区分=D(ディテール)の場合、TABフィールド(複数TAB対応)および左右位置は有効。

                Dim SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.Char, 20)
                Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.Char, 50)
                Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.Char, 50)
                Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", System.Data.SqlDbType.Char, 20)
                Dim PARA5 As SqlParameter = SQLcmd.Parameters.Add("@P5", System.Data.SqlDbType.Date)
                PARA1.Value = "Default"
                PARA2.Value = I_MAPID
                PARA3.Value = I_VARIANT
                PARA4.Value = I_TAB
                PARA5.Value = Date.Now
                Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

                'GridViewの列項目作成
                While SQLdr.Read
                    If SQLdr("EFFECT") = "Y" Then
                        If O_SORTSTR = "" Then
                            O_SORTSTR = O_SORTSTR & SQLdr("FIELD")
                        Else
                            O_SORTSTR = O_SORTSTR & " , " & SQLdr("FIELD")
                        End If
                    End If

                    O_ERR = "00000"
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

                CS0011LOGWRITE.INFSUBCLASS = "CS0026TBLSORTget"             'SUBクラス名
                CS0011LOGWRITE.INFPOSI = "DB:S0010_UPROFVIEW Select"        '
                CS0011LOGWRITE.NIWEA = "A"                                  '
                CS0011LOGWRITE.TEXT = ex.ToString()
                CS0011LOGWRITE.MESSAGENO = "00003"
                CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
                Exit Sub
            End Try
        End If

    End Sub

End Structure
