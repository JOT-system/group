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
''' ユーザプロファイル（GridView）設定
''' </summary>
''' <remarks></remarks>
Public Structure CS0013UPROFview

    'ユーザプロファイル（GridView）設定dll Interface
    Private I_View As Object                  'PARAM01:GridView
    Private I_MAPID As String                 'PARAM02:MAPID
    Private I_VARIANT As String               'PARAM03:VARI
    Private I_SELKBN As String                'PARAM04:'空白' or 'SEL'
    Private O_ERR As String                   'PARAM05:ERRNo

    Public Property View() As Object
        Get
            Return I_View
        End Get
        Set(ByVal Value As Object)
            I_View = Value
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

    Public Property MAPID() As String
        Get
            Return I_MAPID
        End Get
        Set(ByVal Value As String)
            I_MAPID = Value
        End Set
    End Property

    Public Property SELKBN() As String
        Get
            Return I_SELKBN
        End Get
        Set(ByVal Value As String)
            I_SELKBN = Value
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

    Public Sub CS0013UPROFview()
        '<< エラー説明 >>
        'O_ERR = OK:00000,ERR:00002(環境エラー),ERR:00003(DBerr)

        '●In PARAMチェック
        'PARAM01: I_View
        If IsNothing(I_View) Then
            O_ERR = "00002"

            Dim CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get
            CS0011LOGWRITE.INFSUBCLASS = "CS0013UPROFview"              'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "I_View"                           '
            CS0011LOGWRITE.NIWEA = "A"                                  '
            CS0011LOGWRITE.TEXT = "システム管理者へ連絡して下さい(In PARAM Err)"
            CS0011LOGWRITE.MESSAGENO = "00002"
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End If
        Dim W_OBJ As GridView = I_View

        'PARAM02: I_MAPID
        If IsNothing(I_MAPID) Then
            O_ERR = "00002"

            Dim CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get
            CS0011LOGWRITE.INFSUBCLASS = "CS0013UPROFview"              'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "I_MAPID"                           '
            CS0011LOGWRITE.NIWEA = "A"                                  '
            CS0011LOGWRITE.TEXT = "システム管理者へ連絡して下さい(In PARAM Err)"
            CS0011LOGWRITE.MESSAGENO = "00002"
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End If

        'PARAM03: I_VARIANT
        If IsNothing(I_VARIANT) Then
            I_VARIANT = ""
        End If

        '●ユーザプロファイル（View）取得
        '○ 画面UserIDのDB(S0010_UPROFVIEW)検索
        Dim WW_RECCNT As Integer = 0
        Dim WW_COLUMNtext As New BoundField

        'ユーザプロファイル（ビュー）… 個別設定値を検索
        Try

            W_OBJ.AutoGenerateColumns = False                    '列自動作成True(2重作成禁止)
            W_OBJ.ShowHeader = True                              'Header表示有無
            W_OBJ.ShowHeaderWhenEmpty = True                     'データ無時のHeader表示有無
            W_OBJ.AllowSorting = True                            'Sort許可

            'DataBase接続文字
            Dim SQLcon As New SqlConnection(HttpContext.Current.Session("DBcon"))
            SQLcon.Open() 'DataBase接続(Open)

            '検索SQL文
            Dim SQLStr As String = _
                 "SELECT rtrim(FIELD) as FIELD , rtrim(NAMES) as NAMES , LENGTH , UPDTIMSTP, EFFECT, SORT " _
               & " FROM  S0010_UPROFVIEW  " _
               & " Where USERID   = @P1 " _
               & "   and MAPID    = @P2 " _
               & "   and VARIANT  = @P3 " _
               & "   and TITOLKBN = 'I' " _
               & "   and HDKBN    = 'H' " _
               & "   and STYMD   <= @P4 " _
               & "   and ENDYMD  >= @P4 " _
               & "   and DELFLG  <> '1' " _
               & "ORDER BY SEQ "

            Dim SQLcmd As New SqlCommand(SQLStr, SQLcon)
            Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.Char, 20)
            Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.Char, 50)
            Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.Char, 50)
            Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", System.Data.SqlDbType.Date)
            PARA1.Value = HttpContext.Current.Session("Userid")
            PARA2.Value = I_MAPID
            PARA3.Value = I_VARIANT
            PARA4.Value = Date.Now
            Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

            'GridViewの列項目作成
            While SQLdr.Read
                If WW_RECCNT = 0 Then
                    WW_COLUMNtext = New BoundField
                    WW_COLUMNtext.HeaderText = "項番"
                    WW_COLUMNtext.DataField = "LINECNT"
                    W_OBJ.Columns.Add(WW_COLUMNtext)
                    W_OBJ.Columns(W_OBJ.Columns.Count - 1).HeaderStyle.Wrap = False
                    W_OBJ.Columns(W_OBJ.Columns.Count - 1).ItemStyle.Wrap = False
                    W_OBJ.Columns(W_OBJ.Columns.Count - 1).Visible = True

                    WW_COLUMNtext = New BoundField
                    WW_COLUMNtext.HeaderText = "操作"
                    WW_COLUMNtext.DataField = "OPERATION"
                    W_OBJ.Columns.Add(WW_COLUMNtext)
                    W_OBJ.Columns(W_OBJ.Columns.Count - 1).HeaderStyle.Wrap = False
                    W_OBJ.Columns(W_OBJ.Columns.Count - 1).ItemStyle.Wrap = False
                    W_OBJ.Columns(W_OBJ.Columns.Count - 1).Visible = True

                    WW_COLUMNtext = New BoundField
                    WW_COLUMNtext.HeaderText = "タイムスタンプ"
                    WW_COLUMNtext.DataField = "TIMSTP"
                    W_OBJ.Columns.Add(WW_COLUMNtext)
                    W_OBJ.Columns(W_OBJ.Columns.Count - 1).HeaderStyle.Wrap = False
                    W_OBJ.Columns(W_OBJ.Columns.Count - 1).ItemStyle.Wrap = False
                    W_OBJ.Columns(W_OBJ.Columns.Count - 1).Visible = True

                    WW_COLUMNtext = New BoundField
                    WW_COLUMNtext.HeaderText = "画面選択"
                    WW_COLUMNtext.DataField = "SELECT"
                    W_OBJ.Columns.Add(WW_COLUMNtext)
                    W_OBJ.Columns(W_OBJ.Columns.Count - 1).HeaderStyle.Wrap = False
                    W_OBJ.Columns(W_OBJ.Columns.Count - 1).ItemStyle.Wrap = False
                    W_OBJ.Columns(W_OBJ.Columns.Count - 1).Visible = True

                    WW_COLUMNtext = New BoundField
                    WW_COLUMNtext.HeaderText = "抽出"
                    WW_COLUMNtext.DataField = "HIDDEN"
                    W_OBJ.Columns.Add(WW_COLUMNtext)
                    W_OBJ.Columns(W_OBJ.Columns.Count - 1).HeaderStyle.Wrap = False
                    W_OBJ.Columns(W_OBJ.Columns.Count - 1).ItemStyle.Wrap = False
                    W_OBJ.Columns(W_OBJ.Columns.Count - 1).Visible = True

                End If

                'If I_SELKBN = "SEL" Then
                '    'EFECT='Y' or SORT<>0のみ抽出する
                '    If SQLdr("EFFECT") = "N" And SQLdr("SORT") = 0 Then
                '        WW_RECCNT = 1
                '        Continue While
                '    End If
                'End If

                'EFECT='Y' or SORT<>0のみ抽出する
                If SQLdr("EFFECT") = "N" And SQLdr("SORT") = 0 Then
                    WW_RECCNT = 1
                    Continue While
                End If

                WW_COLUMNtext = New BoundField
                WW_COLUMNtext.HeaderText = SQLdr("NAMES")
                WW_COLUMNtext.DataField = SQLdr("FIELD")
                W_OBJ.Columns.Add(WW_COLUMNtext)
                W_OBJ.Columns(W_OBJ.Columns.Count - 1).HeaderStyle.Wrap = False
                W_OBJ.Columns(W_OBJ.Columns.Count - 1).ItemStyle.Wrap = False
                W_OBJ.Columns(W_OBJ.Columns.Count - 1).Visible = True

                WW_RECCNT = 1
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

            CS0011LOGWRITE.INFSUBCLASS = "CS0013UPROFview"              'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:S0010_UPROFVIEW Select"        '
            CS0011LOGWRITE.NIWEA = "A"                                  '
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = "00003"
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try

        'ユーザプロファイル（ビュー）… デフォルト値を検索
        If WW_RECCNT = 0 Then
            Try
                'DataBase接続文字
                Dim SQLcon As New SqlConnection(HttpContext.Current.Session("DBcon"))
                SQLcon.Open() 'DataBase接続(Open)

                '検索SQL文
                Dim SQLStr As String = _
                 "SELECT rtrim(FIELD) as FIELD , rtrim(NAMES) as NAMES , LENGTH , UPDTIMSTP, EFFECT, SORT " _
                   & " FROM  S0010_UPROFVIEW  " _
                   & " Where USERID   = @P1 " _
                   & "   and MAPID    = @P2 " _
                   & "   and VARIANT  = @P3 " _
                   & "   and TITOLKBN = 'I' " _
                   & "   and HDKBN    = 'H' " _
                   & "   and STYMD   <= @P4 " _
                   & "   and ENDYMD  >= @P4 " _
                   & "   and DELFLG  <> '1' " _
                   & "ORDER BY SEQ "

                Dim SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.Char, 20)
                Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.Char, 50)
                Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.Char, 50)
                Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", System.Data.SqlDbType.Date)
                PARA1.Value = "Default"
                PARA2.Value = I_MAPID
                PARA3.Value = I_VARIANT
                PARA4.Value = Date.Now
                Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

                'GridViewの列項目作成
                While SQLdr.Read
                    If WW_RECCNT = 0 Then
                        WW_COLUMNtext = New BoundField
                        WW_COLUMNtext.HeaderText = "項番"
                        WW_COLUMNtext.DataField = "LINECNT"
                        W_OBJ.Columns.Add(WW_COLUMNtext)
                        W_OBJ.Columns(W_OBJ.Columns.Count - 1).HeaderStyle.Wrap = False
                        W_OBJ.Columns(W_OBJ.Columns.Count - 1).ItemStyle.Wrap = False
                        W_OBJ.Columns(W_OBJ.Columns.Count - 1).Visible = True

                        WW_COLUMNtext = New BoundField
                        WW_COLUMNtext.HeaderText = "操作"
                        WW_COLUMNtext.DataField = "OPERATION"
                        W_OBJ.Columns.Add(WW_COLUMNtext)
                        W_OBJ.Columns(W_OBJ.Columns.Count - 1).HeaderStyle.Wrap = False
                        W_OBJ.Columns(W_OBJ.Columns.Count - 1).ItemStyle.Wrap = False
                        W_OBJ.Columns(W_OBJ.Columns.Count - 1).Visible = True

                        WW_COLUMNtext = New BoundField
                        WW_COLUMNtext.HeaderText = "タイムスタンプ"
                        WW_COLUMNtext.DataField = "TIMSTP"
                        W_OBJ.Columns.Add(WW_COLUMNtext)
                        W_OBJ.Columns(W_OBJ.Columns.Count - 1).HeaderStyle.Wrap = False
                        W_OBJ.Columns(W_OBJ.Columns.Count - 1).ItemStyle.Wrap = False
                        W_OBJ.Columns(W_OBJ.Columns.Count - 1).Visible = True

                        WW_COLUMNtext = New BoundField
                        WW_COLUMNtext.HeaderText = "画面選択"
                        WW_COLUMNtext.DataField = "SELECT"
                        W_OBJ.Columns.Add(WW_COLUMNtext)
                        W_OBJ.Columns(W_OBJ.Columns.Count - 1).HeaderStyle.Wrap = False
                        W_OBJ.Columns(W_OBJ.Columns.Count - 1).ItemStyle.Wrap = False
                        W_OBJ.Columns(W_OBJ.Columns.Count - 1).Visible = True

                        WW_COLUMNtext = New BoundField
                        WW_COLUMNtext.HeaderText = "抽出"
                        WW_COLUMNtext.DataField = "HIDDEN"
                        W_OBJ.Columns.Add(WW_COLUMNtext)
                        W_OBJ.Columns(W_OBJ.Columns.Count - 1).HeaderStyle.Wrap = False
                        W_OBJ.Columns(W_OBJ.Columns.Count - 1).ItemStyle.Wrap = False
                        W_OBJ.Columns(W_OBJ.Columns.Count - 1).Visible = True
                    End If

                    'If I_SELKBN = "SEL" Then
                    '    'EFECT='Y' or SORT<>0のみ抽出する
                    '    If SQLdr("EFFECT") = "N" And SQLdr("SORT") = 0 Then
                    '        WW_RECCNT = 1
                    '        Continue While
                    '    End If
                    'End If
                    'EFECT='Y' or SORT<>0のみ抽出する
                    If SQLdr("EFFECT") = "N" And SQLdr("SORT") = 0 Then
                        WW_RECCNT = 1
                        Continue While
                    End If

                    WW_COLUMNtext = New BoundField
                    WW_COLUMNtext.HeaderText = SQLdr("NAMES")
                    WW_COLUMNtext.DataField = SQLdr("FIELD")
                    W_OBJ.Columns.Add(WW_COLUMNtext)
                    W_OBJ.Columns(W_OBJ.Columns.Count - 1).HeaderStyle.Wrap = False
                    W_OBJ.Columns(W_OBJ.Columns.Count - 1).ItemStyle.Wrap = False
                    W_OBJ.Columns(W_OBJ.Columns.Count - 1).Visible = True

                    WW_RECCNT = 1
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

                CS0011LOGWRITE.INFSUBCLASS = "CS0013UPROFview"              'SUBクラス名
                CS0011LOGWRITE.INFPOSI = "DB:S0010_UPROFVIEW Select"           '
                CS0011LOGWRITE.NIWEA = "A"                                  '
                CS0011LOGWRITE.TEXT = ex.ToString()
                CS0011LOGWRITE.MESSAGENO = "00003"
                CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
                Exit Sub
            End Try
        End If

        '最大幅設定準備
        For i As Integer = 0 To W_OBJ.Columns.Count - 1
            W_OBJ.Columns(i).HeaderStyle.Wrap = False
            W_OBJ.Columns(i).ItemStyle.Wrap = False
        Next

        I_View = W_OBJ

        O_ERR = "00000"


    End Sub

End Structure
