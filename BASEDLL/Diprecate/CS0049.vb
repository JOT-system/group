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

'■ユーザプロファイル（Grid幅）取得
Public Structure CS0049UPROFview

    'ユーザプロファイル（Grid幅）取得dll Interface
    Private I_MAPID As String                 'PARAM01:MAPID
    Private I_VARIANT As String               'PARAM02:変数
    Private I_FIELD As String                 'PARAM03:GridViewItem
    Private I_TBL As DataTable                'PARAM04:GridViewItem
    Private O_ERR As String                   'PARAM05:ERRNo
    Private O_LENGTH As Integer               'PARAM06:Grid幅
    Private O_ALIGN As String                 'PARAM07:文字配置
    Private O_EFFECT As String                'PARAM08:表示有無

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

    Public Property FIELD() As String
        Get
            Return I_FIELD
        End Get
        Set(ByVal Value As String)
            I_FIELD = Value
        End Set
    End Property

    Public Property TBL() As DataTable
        Get
            Return I_TBL
        End Get
        Set(ByVal Value As DataTable)
            I_TBL = Value
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

    Public Property LENGTH() As Integer
        Get
            Return O_LENGTH
        End Get
        Set(ByVal Value As Integer)
            O_LENGTH = Value
        End Set
    End Property

    Public Property ALIGN() As String
        Get
            Return O_ALIGN
        End Get
        Set(ByVal Value As String)
            O_ALIGN = Value
        End Set
    End Property

    Public Property EFFECT() As String
        Get
            Return O_EFFECT
        End Get
        Set(ByVal Value As String)
            O_EFFECT = Value
        End Set
    End Property

    Public Sub CS0049UPROFview()
        '<< エラー説明 >>
        'O_ERR = OK:00000,ERR:00002(環境エラー),ERR:00003(DBerr)

        Dim S0010row As DataRow

        '●In PARAMチェック
        'PARAM01: I_MAPID
        If IsNothing(I_MAPID) Then
            O_ERR = "00002"

            Dim CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get
            CS0011LOGWRITE.INFSUBCLASS = "CS0049UPROFview"              'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "I_MAPID"                           '
            CS0011LOGWRITE.NIWEA = "A"                                  '
            CS0011LOGWRITE.TEXT = "システム管理者へ連絡して下さい(In PARAM Err)"
            CS0011LOGWRITE.MESSAGENO = "00002"
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End If

        'PARAM02: I_VARIANT
        If IsNothing(I_VARIANT) Then
            I_VARIANT = ""
        End If

        'PARAM03: I_FIELD
        If IsNothing(I_FIELD) Then
            O_ERR = "00002"

            Dim CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get
            CS0011LOGWRITE.INFSUBCLASS = "CS0049UPROFview"              'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "I_FIELD"                          '
            CS0011LOGWRITE.NIWEA = "A"                                  '
            CS0011LOGWRITE.TEXT = "システム管理者へ連絡して下さい(In PARAM Err)"
            CS0011LOGWRITE.MESSAGENO = "00002"
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End If

        'PARAM04: I_TBL
        If IsNothing(I_TBL) Then
            O_ERR = "00002"

            Dim CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get
            CS0011LOGWRITE.INFSUBCLASS = "CS0049UPROFview"              'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "I_TBL"                            '
            CS0011LOGWRITE.NIWEA = "A"                                  '
            CS0011LOGWRITE.TEXT = "システム管理者へ連絡して下さい(In PARAM Err)"
            CS0011LOGWRITE.MESSAGENO = "00002"
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End If

        O_LENGTH = 0
        O_ERR = "00002"

        '●ユーザプロファイル（View）Grid幅取得
        'ユーザプロファイル（ビュー）… 個別設定値を検索
        If I_TBL.Columns.Count = 0 Then
            I_TBL.Clear()
            I_TBL.Columns.Add("FIELD", GetType(String))                  'フィールド名
            I_TBL.Columns.Add("LENGTH", GetType(Integer))                'フィールド表示長さ
            I_TBL.Columns.Add("ALIGN", GetType(String))                  'フィールド表示位置
            I_TBL.Columns.Add("EFFECT", GetType(String))                 'フィールド表示有無
            'インデックス作成
            I_TBL.DefaultView.Sort = "FIELD"

            Try
                'DataBase接続文字
                Dim SQLcon As New SqlConnection(HttpContext.Current.Session("DBcon"))
                SQLcon.Open() 'DataBase接続(Open)

                '検索SQL文
                Dim SQLStr As String = _
                     "SELECT FIELD, LENGTH, rtrim(ALIGN) as ALIGN, rtrim(EFFECT) as EFFECT " _
                   & " FROM  S0010_UPROFVIEW  " _
                   & " Where USERID   = @P1 " _
                   & "   and MAPID    = @P2 " _
                   & "   and VARIANT  = @P3 " _
                   & "   and TITOLKBN = 'I' " _
                   & "   and HDKBN    = 'H' " _
                   & "   and STYMD   <= @P5 " _
                   & "   and ENDYMD  >= @P5 " _
                   & "   and DELFLG  <> '1' "

                Dim SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.Char, 20)
                Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.Char, 50)
                Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.Char, 50)
                Dim PARA5 As SqlParameter = SQLcmd.Parameters.Add("@P5", System.Data.SqlDbType.Date)
                PARA1.Value = HttpContext.Current.Session("Userid")
                PARA2.Value = I_MAPID
                PARA3.Value = I_VARIANT
                PARA5.Value = Date.Now
                Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

                While SQLdr.Read
                    S0010row = I_TBL.NewRow
                    S0010row("FIELD") = SQLdr("FIELD")
                    S0010row("LENGTH") = SQLdr("LENGTH")
                    S0010row("ALIGN") = SQLdr("ALIGN")
                    S0010row("EFFECT") = SQLdr("EFFECT")
                    I_TBL.Rows.Add(S0010row)
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

                CS0011LOGWRITE.INFSUBCLASS = "CS0049UPROFview"              'SUBクラス名
                CS0011LOGWRITE.INFPOSI = "DB:S0010_UPROFVIEW Select"        '
                CS0011LOGWRITE.NIWEA = "A"                                  '
                CS0011LOGWRITE.TEXT = ex.ToString()
                CS0011LOGWRITE.MESSAGENO = "00003"
                CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
                Exit Sub
            End Try

            'ユーザプロファイル（ビュー）… デフォルト値を検索
            If I_TBL.Rows.Count = 0 Then
                Try
                    'DataBase接続文字
                    Dim SQLcon As New SqlConnection(HttpContext.Current.Session("DBcon"))
                    SQLcon.Open() 'DataBase接続(Open)

                    '検索SQL文
                    Dim SQLStr As String = _
                         "SELECT FIELD, LENGTH, rtrim(ALIGN) as ALIGN, rtrim(EFFECT) as EFFECT " _
                       & " FROM  S0010_UPROFVIEW  " _
                       & " Where USERID   = @P1 " _
                       & "   and MAPID    = @P2 " _
                       & "   and VARIANT  = @P3 " _
                       & "   and TITOLKBN = 'I' " _
                       & "   and HDKBN    = 'H' " _
                       & "   and STYMD   <= @P5 " _
                       & "   and ENDYMD  >= @P5 " _
                       & "   and DELFLG  <> '1' "

                    Dim SQLcmd As New SqlCommand(SQLStr, SQLcon)
                    Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.Char, 20)
                    Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.Char, 50)
                    Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.Char, 50)
                    Dim PARA5 As SqlParameter = SQLcmd.Parameters.Add("@P5", System.Data.SqlDbType.Date)
                    PARA1.Value = "Default"
                    PARA2.Value = I_MAPID
                    PARA3.Value = I_VARIANT
                    PARA5.Value = Date.Now
                    Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

                    While SQLdr.Read
                        S0010row = I_TBL.NewRow
                        S0010row("FIELD") = SQLdr("FIELD")
                        S0010row("LENGTH") = SQLdr("LENGTH")
                        S0010row("ALIGN") = SQLdr("ALIGN")
                        S0010row("EFFECT") = SQLdr("EFFECT")
                        I_TBL.Rows.Add(S0010row)
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

                    CS0011LOGWRITE.INFSUBCLASS = "CS0049UPROFview"              'SUBクラス名
                    CS0011LOGWRITE.INFPOSI = "DB:S0010_UPROFVIEW Select"           '
                    CS0011LOGWRITE.NIWEA = "A"                                  '
                    CS0011LOGWRITE.TEXT = ex.ToString()
                    CS0011LOGWRITE.MESSAGENO = "00003"
                    CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
                    Exit Sub
                End Try
            End If
        End If

        Dim WW_row() As DataRow = I_TBL.Select("FIELD='" & I_FIELD & "'")
        Dim i As Integer = 0

        If WW_row.Count > 0 Then
            O_LENGTH = WW_row(i)("LENGTH")
            O_ALIGN = WW_row(i)("ALIGN")
            O_EFFECT = WW_row(i)("EFFECT")
        Else
            O_LENGTH = 1
            O_ALIGN = "center"
            O_EFFECT = "N"
        End If

        O_ERR = "00000"

    End Sub

End Structure


