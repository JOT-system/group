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
'''ユーザプロファイル（GridView）設定 
''' </summary>
''' <remarks>廃止予定</remarks>
Public Structure CS0029UPROFviewD

    'ユーザプロファイル（GridView）設定dll Interface
    Private I_MAPID As String                        'PARAM01:MAPID
    Private I_VARIANT As String                      'PARAM02:VARI
    Private I_TAB As String                          'PARAM03:TAB
    Private O_ERR As String                          'PARAM04:ERRNo
    Private O_TABLEDATA As System.Data.DataTable     'PARAM05:TABLE

    Private O_FIELD As List(Of String)               '
    Private O_NAMES As List(Of String)               '
    Private O_POJITION As List(Of String)            '
    Private O_EFFECT As List(Of String)              '
    Private O_SEQ As List(Of Integer)                '
    Private O_SEQMAX As Integer                      '


    Public Property VARI() As String
        Get
            Return I_VARIANT
        End Get
        Set(ByVal Value As String)
            I_VARIANT = Value
        End Set
    End Property

    Public Property MAPID() As String
        Get
            Return I_MAPID
        End Get
        Set(ByVal Value As String)
            I_MAPID = Value
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

    Public Property TABLEDATA() As System.Data.DataTable
        Get
            Return O_TABLEDATA
        End Get
        Set(ByVal Value As System.Data.DataTable)
            O_TABLEDATA = Value
        End Set
    End Property

    Public Property FIELD() As List(Of String)
        Get
            Return O_FIELD
        End Get
        Set(ByVal Value As List(Of String))
            O_FIELD = Value
        End Set
    End Property

    Public Property NAMES() As List(Of String)
        Get
            Return O_NAMES
        End Get
        Set(ByVal Value As List(Of String))
            O_NAMES = Value
        End Set
    End Property

    Public Property POJITION() As List(Of String)
        Get
            Return O_POJITION
        End Get
        Set(ByVal Value As List(Of String))
            O_POJITION = Value
        End Set
    End Property

    Public Property EFFECT() As List(Of String)
        Get
            Return O_EFFECT
        End Get
        Set(ByVal Value As List(Of String))
            O_EFFECT = Value
        End Set
    End Property

    Public Property SEQ() As List(Of Integer)
        Get
            Return O_SEQ
        End Get
        Set(ByVal Value As List(Of Integer))
            O_SEQ = Value
        End Set
    End Property

    Public Property SEQMAX() As Integer
        Get
            Return O_SEQMAX
        End Get
        Set(ByVal Value As Integer)
            O_SEQMAX = Value
        End Set
    End Property

    Public Sub CS0029UPROFviewD()
        '<< エラー説明 >>
        'O_ERR = OK:00000,ERR:00002(環境エラー),ERR:00003(DBerr)

        '●In PARAMチェック
        'PARAM01: I_MAPID
        If IsNothing(I_MAPID) Then
            O_ERR = "00002"

            Dim CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get
            CS0011LOGWRITE.INFSUBCLASS = "CS0029UPROFviewD"              'SUBクラス名
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

        '●ユーザプロファイル（View）取得
        '○ 画面UserIDのDB(S0010_UPROFVIEW)検索

        Dim WW_SEQMAX As Integer = 0

        Dim WW_FIELD As New List(Of String)
        Dim WW_NAMES As New List(Of String)
        Dim WW_POJITION As New List(Of String)
        Dim WW_EFFECT As New List(Of String)
        Dim WW_SEQ As New List(Of Integer)

        'ユーザプロファイル（ビュー）… 個別設定値を検索
        Try

            'DataBase接続文字
            Dim SQLcon As New SqlConnection(HttpContext.Current.Session("DBcon"))
            SQLcon.Open() 'DataBase接続(Open)

            '検索SQL文
            Dim SQLStr As String = _
                 "SELECT rtrim(FIELD) as FIELD , rtrim(POJITION) as POJITION , rtrim(NAMES) as NAMES , rtrim(EFFECT) as EFFECT , SEQ " _
               & " FROM  S0010_UPROFVIEW  " _
               & " Where USERID   = @P1 " _
               & "   and MAPID    = @P2 " _
               & "   and VARIANT  = @P3 " _
               & "   and TITOLKBN = 'I' " _
               & "   and HDKBN    = 'D' " _
               & "   and TAB      = @P4 " _
               & "   and STYMD   <= @P5 " _
               & "   and ENDYMD  >= @P5 " _
               & "   and DELFLG  <> '1' " _
               & "ORDER BY SEQ "

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
                WW_FIELD.Add(SQLdr("FIELD"))
                WW_NAMES.Add(SQLdr("NAMES"))
                WW_POJITION.Add(SQLdr("POJITION"))
                WW_EFFECT.Add(SQLdr("EFFECT"))
                WW_SEQ.Add(SQLdr("SEQ"))
                If SQLdr("SEQ") > WW_SEQMAX Then
                    WW_SEQMAX = SQLdr("SEQ")
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
            O_ERR = "00003"
            CS0011LOGWRITE.INFSUBCLASS = "CS0029UPROFviewD"              'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:S0010_UPROFVIEW Select"        '
            CS0011LOGWRITE.NIWEA = "A"                                  '
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = "00003"
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try

        'ユーザプロファイル（ビュー）… デフォルト値を検索
        If WW_SEQMAX = 0 Then
            Try
                'DataBase接続文字
                Dim SQLcon As New SqlConnection(HttpContext.Current.Session("DBcon"))
                SQLcon.Open() 'DataBase接続(Open)

                '検索SQL文
                Dim SQLStr As String = _
                 "SELECT rtrim(FIELD) as FIELD , rtrim(POJITION) as POJITION , rtrim(NAMES) as NAMES , rtrim(EFFECT) as EFFECT , SEQ " _
                   & " FROM  S0010_UPROFVIEW  " _
                   & " Where USERID   = @P1 " _
                   & "   and MAPID    = @P2 " _
                   & "   and VARIANT  = @P3 " _
                   & "   and TITOLKBN = 'I' " _
                   & "   and HDKBN    = 'D' " _
                   & "   and TAB      = @P4 " _
                   & "   and STYMD   <= @P5 " _
                   & "   and ENDYMD  >= @P5 " _
                   & "   and DELFLG  <> '1' " _
                   & "ORDER BY SEQ "

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
                    WW_FIELD.Add(SQLdr("FIELD"))
                    WW_NAMES.Add(SQLdr("NAMES"))
                    WW_POJITION.Add(SQLdr("POJITION"))
                    WW_EFFECT.Add(SQLdr("EFFECT"))
                    WW_SEQ.Add(SQLdr("SEQ"))
                    If SQLdr("SEQ") > WW_SEQMAX Then
                        WW_SEQMAX = SQLdr("SEQ")
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
                O_ERR = "00003"
                CS0011LOGWRITE.INFSUBCLASS = "CS0029UPROFviewD"              'SUBクラス名
                CS0011LOGWRITE.INFPOSI = "DB:S0010_UPROFVIEW Select"           '
                CS0011LOGWRITE.NIWEA = "A"                                  '
                CS0011LOGWRITE.TEXT = ex.ToString()
                CS0011LOGWRITE.MESSAGENO = "00003"
                CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
                Exit Sub
            End Try
        End If

        '■データ格納準備（テーブル列追加）
        O_SEQMAX = WW_SEQMAX

        Dim WW_TBLDATA As New System.Data.DataTable
        Dim WW_TBLDATArow As DataRow
        WW_TBLDATA.Clear()

        '出力DATATABLEに列(項目)追加
        WW_TBLDATA.Columns.Add("FIELDNM_L", GetType(String))           '左・項目見出し
        WW_TBLDATA.Columns.Add("FIELD_L", GetType(String))             '左・項目
        WW_TBLDATA.Columns.Add("VALUE_L", GetType(String))             '左・値
        WW_TBLDATA.Columns.Add("VALUE_TEXT_L", GetType(String))        '左・値テキスト
        WW_TBLDATA.Columns.Add("FIELDNM_R", GetType(String))           '右・項目見出し
        WW_TBLDATA.Columns.Add("FIELD_R", GetType(String))             '右・項目
        WW_TBLDATA.Columns.Add("VALUE_R", GetType(String))             '右・値
        WW_TBLDATA.Columns.Add("VALUE_TEXT_R", GetType(String))        '右・値テキスト
        WW_TBLDATA.Columns.Add("FIELDNM_M", GetType(String))           '中央・項目見出し
        WW_TBLDATA.Columns.Add("FIELD_M", GetType(String))             '中央・項目
        WW_TBLDATA.Columns.Add("VALUE_M", GetType(String))             '中央・値
        WW_TBLDATA.Columns.Add("VALUE_TEXT_M", GetType(String))        '中央・値テキスト

        '受注専用
        WW_TBLDATA.Columns.Add("FIELDNM_1", GetType(String))           '左1・項目見出し
        WW_TBLDATA.Columns.Add("FIELD_1", GetType(String))             '左1・項目
        WW_TBLDATA.Columns.Add("VALUE_1", GetType(String))             '左1・値
        WW_TBLDATA.Columns.Add("VALUE_TEXT_1", GetType(String))        '左1・値テキスト
        WW_TBLDATA.Columns.Add("FIELDNM_2", GetType(String))           '右1・項目見出し
        WW_TBLDATA.Columns.Add("FIELD_2", GetType(String))             '右1・項目
        WW_TBLDATA.Columns.Add("VALUE_2", GetType(String))             '右1・値
        WW_TBLDATA.Columns.Add("VALUE_TEXT_2", GetType(String))        '右1・値テキスト
        WW_TBLDATA.Columns.Add("FIELDNM_3", GetType(String))           '中央1・項目見出し
        WW_TBLDATA.Columns.Add("FIELD_3", GetType(String))             '中央1・項目
        WW_TBLDATA.Columns.Add("VALUE_3", GetType(String))             '中央1・値
        WW_TBLDATA.Columns.Add("VALUE_TEXT_3", GetType(String))        '中央1・値テキスト
        WW_TBLDATA.Columns.Add("FIELDNM_4", GetType(String))           '左2・項目見出し
        WW_TBLDATA.Columns.Add("FIELD_4", GetType(String))             '左2・項目
        WW_TBLDATA.Columns.Add("VALUE_4", GetType(String))             '左2・値
        WW_TBLDATA.Columns.Add("VALUE_TEXT_4", GetType(String))        '左2・値テキスト
        WW_TBLDATA.Columns.Add("FIELDNM_5", GetType(String))           '右2・項目見出し
        WW_TBLDATA.Columns.Add("FIELD_5", GetType(String))             '右2・項目
        WW_TBLDATA.Columns.Add("VALUE_5", GetType(String))             '右2・値
        WW_TBLDATA.Columns.Add("VALUE_TEXT_5", GetType(String))        '右2・値テキスト
        WW_TBLDATA.Columns.Add("FIELDNM_6", GetType(String))           '中央2・項目見出し
        WW_TBLDATA.Columns.Add("FIELD_6", GetType(String))             '中央2・項目
        WW_TBLDATA.Columns.Add("VALUE_6", GetType(String))             '中央2・値
        WW_TBLDATA.Columns.Add("VALUE_TEXT_6", GetType(String))        '中央2・値テキスト

        '○空明細作成
        If WW_SEQMAX > 0 Then
            For i As Integer = 0 To WW_SEQMAX - 1
                WW_TBLDATArow = WW_TBLDATA.NewRow()

                WW_TBLDATArow("FIELDNM_L") = ""
                WW_TBLDATArow("FIELD_L") = ""
                WW_TBLDATArow("VALUE_L") = ""
                WW_TBLDATArow("VALUE_TEXT_L") = ""

                WW_TBLDATArow("FIELDNM_R") = ""
                WW_TBLDATArow("FIELD_R") = ""
                WW_TBLDATArow("VALUE_R") = ""
                WW_TBLDATArow("VALUE_TEXT_R") = ""

                WW_TBLDATArow("FIELDNM_M") = ""
                WW_TBLDATArow("FIELD_M") = ""
                WW_TBLDATArow("VALUE_M") = ""
                WW_TBLDATArow("VALUE_TEXT_M") = ""

                '受注関連プログラム専用
                WW_TBLDATArow("FIELDNM_1") = ""
                WW_TBLDATArow("FIELD_1") = ""
                WW_TBLDATArow("VALUE_1") = ""
                WW_TBLDATArow("VALUE_TEXT_1") = ""

                WW_TBLDATArow("FIELDNM_2") = ""
                WW_TBLDATArow("FIELD_2") = ""
                WW_TBLDATArow("VALUE_2") = ""
                WW_TBLDATArow("VALUE_TEXT_2") = ""

                WW_TBLDATArow("FIELDNM_3") = ""
                WW_TBLDATArow("FIELD_3") = ""
                WW_TBLDATArow("VALUE_3") = ""
                WW_TBLDATArow("VALUE_TEXT_3") = ""

                WW_TBLDATArow("FIELDNM_4") = ""
                WW_TBLDATArow("FIELD_4") = ""
                WW_TBLDATArow("VALUE_4") = ""
                WW_TBLDATArow("VALUE_TEXT_4") = ""

                WW_TBLDATArow("FIELDNM_5") = ""
                WW_TBLDATArow("FIELD_5") = ""
                WW_TBLDATArow("VALUE_5") = ""
                WW_TBLDATArow("VALUE_TEXT_5") = ""

                WW_TBLDATArow("FIELDNM_6") = ""
                WW_TBLDATArow("FIELD_6") = ""
                WW_TBLDATArow("VALUE_6") = ""
                WW_TBLDATArow("VALUE_TEXT_6") = ""
                WW_TBLDATA.Rows.Add(WW_TBLDATArow)
            Next

            '○空明細作成
            Dim JYUTYUFLG As Boolean = False
            For i As Integer = 0 To WW_SEQ.Count - 1

                Select Case WW_POJITION.Item(i)
                    Case "L"
                        If (WW_SEQ.Item(i) - 1) <= 0 Then
                            WW_TBLDATA.Rows(0)("FIELD_L") = WW_FIELD.Item(i)
                            WW_TBLDATA.Rows(0)("FIELDNM_L") = WW_NAMES.Item(i)
                        Else
                            WW_TBLDATA.Rows((WW_SEQ.Item(i) - 1))("FIELD_L") = WW_FIELD.Item(i)
                            WW_TBLDATA.Rows((WW_SEQ.Item(i) - 1))("FIELDNM_L") = WW_NAMES.Item(i)
                        End If
                    Case "R"
                        If (WW_SEQ.Item(i) - 1) <= 0 Then
                            WW_TBLDATA.Rows(0)("FIELD_R") = WW_FIELD.Item(i)
                            WW_TBLDATA.Rows(0)("FIELDNM_R") = WW_NAMES.Item(i)
                        Else
                            WW_TBLDATA.Rows((WW_SEQ.Item(i) - 1))("FIELD_R") = WW_FIELD.Item(i)
                            WW_TBLDATA.Rows((WW_SEQ.Item(i) - 1))("FIELDNM_R") = WW_NAMES.Item(i)
                        End If
                    Case "M"
                        If (WW_SEQ.Item(i) - 1) <= 0 Then
                            WW_TBLDATA.Rows(0)("FIELD_M") = WW_FIELD.Item(i)
                            WW_TBLDATA.Rows(0)("FIELDNM_M") = WW_NAMES.Item(i)
                        Else
                            WW_TBLDATA.Rows((WW_SEQ.Item(i) - 1))("FIELD_M") = WW_FIELD.Item(i)
                            WW_TBLDATA.Rows((WW_SEQ.Item(i) - 1))("FIELDNM_M") = WW_NAMES.Item(i)
                        End If

                        '受注関連プログラム専用
                    Case "1"
                        JYUTYUFLG = True
                        If (WW_SEQ.Item(i) - 1) <= 0 Then
                            WW_TBLDATA.Rows(0)("FIELD_1") = WW_FIELD.Item(i)
                            WW_TBLDATA.Rows(0)("FIELDNM_1") = WW_NAMES.Item(i)
                        Else
                            WW_TBLDATA.Rows((WW_SEQ.Item(i) - 1))("FIELD_1") = WW_FIELD.Item(i)
                            WW_TBLDATA.Rows((WW_SEQ.Item(i) - 1))("FIELDNM_1") = WW_NAMES.Item(i)
                        End If
                    Case "2"
                        JYUTYUFLG = True
                        If (WW_SEQ.Item(i) - 1) <= 0 Then
                            WW_TBLDATA.Rows(0)("FIELD_2") = WW_FIELD.Item(i)
                            WW_TBLDATA.Rows(0)("FIELDNM_2") = WW_NAMES.Item(i)
                        Else
                            WW_TBLDATA.Rows((WW_SEQ.Item(i) - 1))("FIELD_2") = WW_FIELD.Item(i)
                            WW_TBLDATA.Rows((WW_SEQ.Item(i) - 1))("FIELDNM_2") = WW_NAMES.Item(i)
                        End If
                    Case "3"
                        JYUTYUFLG = True
                        If (WW_SEQ.Item(i) - 1) <= 0 Then
                            WW_TBLDATA.Rows(0)("FIELD_3") = WW_FIELD.Item(i)
                            WW_TBLDATA.Rows(0)("FIELDNM_3") = WW_NAMES.Item(i)
                        Else
                            WW_TBLDATA.Rows((WW_SEQ.Item(i) - 1))("FIELD_3") = WW_FIELD.Item(i)
                            WW_TBLDATA.Rows((WW_SEQ.Item(i) - 1))("FIELDNM_3") = WW_NAMES.Item(i)
                        End If
                    Case "4"
                        JYUTYUFLG = True
                        If (WW_SEQ.Item(i) - 1) <= 0 Then
                            WW_TBLDATA.Rows(0)("FIELD_4") = WW_FIELD.Item(i)
                            WW_TBLDATA.Rows(0)("FIELDNM_4") = WW_NAMES.Item(i)
                        Else
                            WW_TBLDATA.Rows((WW_SEQ.Item(i) - 1))("FIELD_4") = WW_FIELD.Item(i)
                            WW_TBLDATA.Rows((WW_SEQ.Item(i) - 1))("FIELDNM_4") = WW_NAMES.Item(i)
                        End If
                    Case "5"
                        JYUTYUFLG = True
                        If (WW_SEQ.Item(i) - 1) <= 0 Then
                            WW_TBLDATA.Rows(0)("FIELD_5") = WW_FIELD.Item(i)
                            WW_TBLDATA.Rows(0)("FIELDNM_5") = WW_NAMES.Item(i)
                        Else
                            WW_TBLDATA.Rows((WW_SEQ.Item(i) - 1))("FIELD_5") = WW_FIELD.Item(i)
                            WW_TBLDATA.Rows((WW_SEQ.Item(i) - 1))("FIELDNM_5") = WW_NAMES.Item(i)
                        End If
                    Case "6"
                        JYUTYUFLG = True
                        If (WW_SEQ.Item(i) - 1) <= 0 Then
                            WW_TBLDATA.Rows(0)("FIELD_6") = WW_FIELD.Item(i)
                            WW_TBLDATA.Rows(0)("FIELDNM_6") = WW_NAMES.Item(i)
                        Else
                            WW_TBLDATA.Rows((WW_SEQ.Item(i) - 1))("FIELD_6") = WW_FIELD.Item(i)
                            WW_TBLDATA.Rows((WW_SEQ.Item(i) - 1))("FIELDNM_6") = WW_NAMES.Item(i)
                        End If
                End Select
            Next
        End If

        O_TABLEDATA = WW_TBLDATA

        O_FIELD = WW_FIELD
        O_NAMES = WW_NAMES
        O_POJITION = WW_POJITION
        O_EFFECT = WW_EFFECT
        O_SEQ = WW_SEQ

        O_ERR = "00000"

        'ワークテーブル解放
        WW_TBLDATA.Dispose()
        WW_TBLDATA = Nothing

    End Sub

End Structure
