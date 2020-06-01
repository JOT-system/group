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
'''帳票出力 
''' </summary>
''' <remarks></remarks>
Public Structure CS0022REPORT

    '帳票出力dll Interface
    Private I_MAPID As String                 'PARAM01:画面ID
    Private I_REPORTID As String              'PARAM02:帳票ID
    Private I_FILEtyp As String               'PARAM03:出力ファイル形式
    Private I_INGridView As Object            'PARAM04:データ参照GridView

    Private O_ERR As String                   'PARAM05:ERRNo
    Private O_FILEpath As String              'PARAM06:出力Dir＋ファイル名
    Private O_URL As String                   'PARAM07:出力URL＋ファイル名

    Public Property MAPID() As String
        Get
            Return I_MAPID
        End Get
        Set(ByVal Value As String)
            I_MAPID = Value
        End Set
    End Property

    Public Property REPORTID() As String
        Get
            Return I_REPORTID
        End Get
        Set(ByVal Value As String)
            I_REPORTID = Value
        End Set
    End Property

    Public Property FILEtyp() As String
        Get
            Return I_FILEtyp
        End Get
        Set(ByVal Value As String)
            I_FILEtyp = Value
        End Set
    End Property

    Public Property INGridView() As Object
        Get
            Return I_INGridView
        End Get
        Set(ByVal Value As Object)
            I_INGridView = Value
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

    Public Property FILEpath() As String
        Get
            Return O_FILEpath
        End Get
        Set(ByVal Value As String)
            O_DIR = Value
        End Set
    End Property

    Public Property URL() As String
        Get
            Return O_URL
        End Get
        Set(ByVal Value As String)
            O_URL = Value
        End Set
    End Property

    Public Sub CS0022REPORT()
        '<< エラー説明 >>
        'O_ERR = OK:00000,ERR:00002(環境エラー),ERR:00003(DBerr)
        '■共通宣言
        Dim CS0009MESSAGEout As New CS0009MESSAGEout            'Message out
        Dim CS0011LOGWRITE As New CS0011LOGWrite                'LogOutput DirString Get
        Dim CS0021UPROFXLS As New CS0021UPROFXLS                'ユーザプロファイル（XLS）取得

        Dim W_ExcelApp As Excel.Application = Nothing
        Dim W_ExcelBooks As Excel.Workbooks = Nothing
        Dim W_ExcelBook As Excel.Workbook = Nothing
        Dim W_ExcelSheets As Excel.Sheets = Nothing
        Dim W_ExcelSheet As Excel.Worksheet = Nothing

        '●In PARAMチェック
        'PARAM01: I_MAPID
        If IsNothing(I_MAPID) Then
            O_ERR = "00002"
            CS0011LOGWRITE.INFSUBCLASS = "CS0022REPORT"                 'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "I_MAPID"                          '
            CS0011LOGWRITE.NIWEA = "A"                                  '
            CS0011LOGWRITE.TEXT = "システム管理者へ連絡して下さい(In PARAM Err)"
            CS0011LOGWRITE.MESSAGENO = "00002"
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End If

        'PARAM02: I_REPORTID
        If IsNothing(I_REPORTID) Then
            O_ERR = "00002"
            CS0011LOGWRITE.INFSUBCLASS = "CS0022REPORT"                 'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "I_REPORTID"                       '
            CS0011LOGWRITE.NIWEA = "A"                                  '
            CS0011LOGWRITE.TEXT = "システム管理者へ連絡して下さい(In PARAM Err)"
            CS0011LOGWRITE.MESSAGENO = "00002"
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End If

        'PARAM03: I_FILEtyp
        If IsNothing(I_FILEtyp) Then
            O_ERR = "00002"
            CS0011LOGWRITE.INFSUBCLASS = "CS0022REPORT"                 'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "I_FILEtyp"                        '
            CS0011LOGWRITE.NIWEA = "A"                                  '
            CS0011LOGWRITE.TEXT = "システム管理者へ連絡して下さい(In PARAM Err)"
            CS0011LOGWRITE.MESSAGENO = "00002"
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End If

        'PARAM04: I_INGridView
        Dim WW_IN_GridView As GridView
        If IsNothing(I_INGridView) Then
            O_ERR = "00002"
            CS0011LOGWRITE.INFSUBCLASS = "CS0022REPORT"                 'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "I_INGridView"                     '
            CS0011LOGWRITE.NIWEA = "A"                                  '
            CS0011LOGWRITE.TEXT = "システム管理者へ連絡して下さい(In PARAM Err)"
            CS0011LOGWRITE.MESSAGENO = "00002"
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        Else
            Try
                WW_IN_GridView = CType(I_INGridView, GridView)
            Catch ex As Exception
                O_ERR = "00002"
                CS0011LOGWRITE.INFSUBCLASS = "CS0022REPORT"                 'SUBクラス名
                CS0011LOGWRITE.INFPOSI = "I_INGridView"                     '
                CS0011LOGWRITE.NIWEA = "A"                                  '
                CS0011LOGWRITE.TEXT = "システム管理者へ連絡して下さい(In PARAM Err)"
                CS0011LOGWRITE.MESSAGENO = "00002"
                CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
                Exit Sub
            End Try
        End If

        '■出力レイアウト取得
        CS0021UPROFXLS.MAPID = I_MAPID
        CS0021UPROFXLS.REPORTID = I_REPORTID
        CS0021UPROFXLS.CS0021UPROFXLS()
        If CS0021UPROFXLS.ERR = "00000" Then
        Else
            O_ERR = "00002"
            CS0011LOGWRITE.INFSUBCLASS = "CS0022REPORT"                 'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "CS0021UPROFXLS"                   '
            CS0011LOGWRITE.NIWEA = "A"                                  '
            CS0011LOGWRITE.TEXT = "システム管理者へ連絡して下さい(In PARAM Err)"
            CS0011LOGWRITE.MESSAGENO = "00002"
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End If

        '■EXCELプロセスのお掃除
        Dim ps As System.Diagnostics.Process() = System.Diagnostics.Process.GetProcesses()
        For Each p As System.Diagnostics.Process In ps
            Try '拒否エラーのためのtry
                If Mid(p.ProcessName, 1, 5) = "EXCEL" Or Mid(p.ProcessName, 1, 5) = "excel" Then
                    Dim WW_START As Long = CInt((DateTime.Parse(p.StartTime)).ToString("HHmmss"))
                    Dim WW_NOW As Long = CInt(DateTime.Now.ToString("HHmmss"))
                    If (WW_NOW - WW_START) > 1 Then   '1秒
                        p.Kill()
                    End If
                End If
            Catch ex As Exception
            End Try
        Next

        '■Excel起動

        Dim WW_ExcelExist As String = ""

        Try
            'W_ExcelApp = New Excel.Application
            W_ExcelApp = CreateObject("Excel.Application")
            W_ExcelBooks = W_ExcelApp.Workbooks

            If CS0021UPROFXLS.EXCELFILE = "" Or System.IO.File.Exists(HttpContext.Current.Session("FILEdir") & "\PRINTFORMAT\" & HttpContext.Current.Session("Userid") & "\" & I_MAPID & "\" & CS0021UPROFXLS.EXCELFILE) = False Then
                '新規のファイルを開く場合
                W_ExcelBook = W_ExcelBooks.Add
                W_ExcelSheets = W_ExcelBook.Worksheets
                W_ExcelSheet = CType(W_ExcelSheets.Item(1), Excel.Worksheet)
            Else
                '既存のファイルを開く場合
                W_ExcelBook = W_ExcelBooks.Open(HttpContext.Current.Session("FILEdir") & "\PRINTFORMAT\" & HttpContext.Current.Session("Userid") & "\" & I_MAPID & "\" & CS0021UPROFXLS.EXCELFILE)
                W_ExcelSheets = W_ExcelBook.Worksheets
                W_ExcelSheet = CType(W_ExcelSheets.Item(1), Excel.Worksheet)

                '2016/07/22 add miyake
                Dim WW_STR As String = CS0021UPROFXLS.EXCELFILE.ToUpper()
                If WW_STR Like "*.XLSM" Then
                    I_FILEtyp = "XLSM"
                End If

                WW_ExcelExist = "ON"
            End If

            W_ExcelApp.Visible = False
        Catch ex As Exception

            O_ERR = "00006"
            CS0011LOGWRITE.INFSUBCLASS = "CS0022REPORT"                 'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "Excel_Open"                       '
            CS0011LOGWRITE.NIWEA = "A"                                  '
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = "00006"
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力

            'プロセスの開放＆終了
            Try
                W_ExcelApp.Quit()
            Catch err As Exception
            End Try

            Try
                W_ExcelApp.Visible = True
            Catch err As Exception
            End Try

            Try
                ExcelMemoryRelease(W_ExcelSheet)      'ExcelSheet の解放
            Catch err As Exception
            End Try

            Try
                ExcelMemoryRelease(W_ExcelSheets)     'ExcelSheets の解放
            Catch err As Exception
            End Try

            Try
                ExcelMemoryRelease(W_ExcelBook)       'ExcelBook の解放
            Catch err As Exception
            End Try

            Try
                ExcelMemoryRelease(W_ExcelBooks)      'ExcelBooks の解放
            Catch err As Exception
            End Try

            Try
                ExcelMemoryRelease(W_ExcelApp)        'ExcelApp を解放
            Catch err As Exception
            End Try

            Exit Sub
        End Try

        '■Excelデータ処理
        Dim WW_EXCELrange As Excel.Range
        Dim WW_STARTpoint As Excel.Range
        Dim WW_ENDpoint As Excel.Range

        '～～～～～ データ設定 (開始) ～～～～～～～～～～～～～～～～

        If CS0021UPROFXLS.POSISTART = 0 Then
            CS0021UPROFXLS.POSISTART = 1
        End If
        If CS0021UPROFXLS.POSI_T_X_MAX = 0 Then
            CS0021UPROFXLS.POSI_T_X_MAX = 1
        End If
        If CS0021UPROFXLS.POSI_T_Y_MAX = 0 Then
            CS0021UPROFXLS.POSI_T_Y_MAX = 1
        End If

        If CS0021UPROFXLS.POSI_I_X_MAX = 0 Then
            CS0021UPROFXLS.POSI_I_X_MAX = 1
        End If
        If CS0021UPROFXLS.POSI_I_Y_MAX = 0 Then
            CS0021UPROFXLS.POSI_I_Y_MAX = 1
        End If

        '○Excel(タイトル)表示
        '    タイトル区分(=H)の場合
        Dim WW_Range_str As String = ""

        Try
            WW_Range_str = "MaxX:" & CS0021UPROFXLS.POSI_T_Y_MAX.ToString & "_MaxY:" & CS0021UPROFXLS.POSI_T_X_MAX.ToString

            Dim WW_HENSYUrange(CS0021UPROFXLS.POSI_T_Y_MAX - 1, CS0021UPROFXLS.POSI_T_X_MAX - 1) As Object          '行編集領域　　※開始位置(0,0) …　object

            '　タイトル(1行目)範囲指定
            WW_STARTpoint = W_ExcelSheet.Cells.Item(1, 1) 'A1
            WW_ENDpoint = W_ExcelSheet.Cells.Item(CS0021UPROFXLS.POSI_T_Y_MAX, CS0021UPROFXLS.POSI_T_X_MAX)
            WW_EXCELrange = W_ExcelSheet.Range(WW_STARTpoint, WW_ENDpoint)                                      'データの入力セル範囲　　※開始位置(1,1)　…　Excel

            '　書式Excel内文字の退避
            Dim WW_DEFULTrange(CS0021UPROFXLS.POSI_T_Y_MAX - 1, CS0021UPROFXLS.POSI_T_X_MAX - 1) As Object          '行編集領域　　※開始位置(1,1) …　object
            WW_DEFULTrange = WW_EXCELrange.Value

            If IsNothing(WW_EXCELrange.Value) Then
            Else
                For i As Integer = 1 To (CS0021UPROFXLS.POSI_T_Y_MAX)
                    For j As Integer = 1 To (CS0021UPROFXLS.POSI_T_X_MAX)
                        WW_HENSYUrange(i - 1, j - 1) = WW_DEFULTrange(i, j)
                    Next
                Next
            End If

            '　タイトル設定(明細と同一レイアウトで明細タイトルを設定する)
            '    ※タイトルは、Ecel・セル位置(A1)を基準として、指定された位置に項目をセット
            '    ※タイトルに表示する項目指定値(Field)は、GridViewの１行目情報を表示する
            For i As Integer = 0 To CS0021UPROFXLS.TITOLKBN.Count - 1

                If CS0021UPROFXLS.TITOLKBN(i) = "T" And CS0021UPROFXLS.EFFECT(i) = "Y" And CS0021UPROFXLS.POSIX(i) > 0 And CS0021UPROFXLS.POSIY(i) > 0 Then
                    Select Case CS0021UPROFXLS.FIELD(i)
                        Case "EXCELTITOL"                'CS0021UPROFXLSパラメータ(FIELDNAME)をセット
                            WW_HENSYUrange(CS0021UPROFXLS.POSIY(i) - 1, CS0021UPROFXLS.POSIX(i) - 1) = CS0021UPROFXLS.FIELDNAME(i)

                        Case "REPORTID"                  'CS0021UPROFXLSパラメータ(REPORTID)をセット
                            WW_HENSYUrange(CS0021UPROFXLS.POSIY(i) - 1, CS0021UPROFXLS.POSIX(i) - 1) = "ID:" & CS0021UPROFXLS.REPORTID

                        Case Else                        'GridViewの1行目の該当項目値をセット
                            For j As Integer = 0 To WW_IN_GridView.Columns.Count - 1
                                Dim WW_DataField As BoundField = WW_IN_GridView.Columns.Item(j) '項目名取得用
                                If WW_DataField.DataField = CS0021UPROFXLS.FIELD(i) Then
                                    'GridViewの1行目内容をセット
                                    If WW_IN_GridView.Rows.Count <> 0 Then
                                        WW_HENSYUrange(CS0021UPROFXLS.POSIY(i) - 1, CS0021UPROFXLS.POSIX(i) - 1) = WW_IN_GridView.Rows(0).Cells(j).Text
                                    End If
                                    Exit For
                                End If
                            Next

                    End Select
                End If

            Next
            WW_EXCELrange.Value = WW_HENSYUrange                  'セルへデータの入力

            '　使い終わった時点で、WW_EXCELrange オブジェクトを解放 
            ExcelMemoryRelease(WW_EXCELrange)

        Catch ex As Exception
            O_ERR = "10042"
            CS0011LOGWRITE.INFSUBCLASS = "CS0022REPORT"                 'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "Excel_Titol_Range"                '
            CS0011LOGWRITE.NIWEA = "A"                                  '
            CS0011LOGWRITE.TEXT = WW_Range_str
            CS0011LOGWRITE.MESSAGENO = "10042"
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            Try
                W_ExcelApp.Quit()
            Catch err As Exception
            End Try

            Try
                W_ExcelApp.Visible = True
            Catch err As Exception
            End Try

            Try
                ExcelMemoryRelease(W_ExcelSheet)      'ExcelSheet の解放
            Catch err As Exception
            End Try

            Try
                ExcelMemoryRelease(W_ExcelSheets)     'ExcelSheets の解放
            Catch err As Exception
            End Try

            Try
                ExcelMemoryRelease(W_ExcelBook)       'ExcelBook の解放
            Catch err As Exception
            End Try

            Try
                ExcelMemoryRelease(W_ExcelBooks)      'ExcelBooks の解放
            Catch err As Exception
            End Try

            Try
                ExcelMemoryRelease(W_ExcelApp)        'ExcelApp を解放
            Catch err As Exception
            End Try

            Exit Sub
        End Try

        '○画面選択明細(GridView)からExcel(明細ヘッダー)へ表示
        Try
            WW_Range_str = "MaxX:" & CS0021UPROFXLS.POSI_I_Y_MAX.ToString & "_MaxY:" & CS0021UPROFXLS.POSI_I_X_MAX.ToString

            Dim WW_HENSYUrange(CS0021UPROFXLS.POSI_I_Y_MAX - 1, CS0021UPROFXLS.POSI_I_X_MAX - 1) As Object                                      '行編集領域　　※開始位置(0,0) …　object

            '　明細タイトル範囲指定
            WW_STARTpoint = W_ExcelSheet.Cells.Item(CS0021UPROFXLS.POSISTART, 1)                                                          '指定された行開始のA列
            WW_ENDpoint = W_ExcelSheet.Cells.Item(CS0021UPROFXLS.POSISTART + CS0021UPROFXLS.POSI_I_Y_MAX - 1, CS0021UPROFXLS.POSI_I_X_MAX)      '指定された行+明細行
            WW_EXCELrange = W_ExcelSheet.Range(WW_STARTpoint, WW_ENDpoint)                                                                'データの入力セル範囲

            '　明細タイトル設定
            For i As Integer = 0 To CS0021UPROFXLS.TITOLKBN.Count - 1
                If CS0021UPROFXLS.TITOLKBN(i) = "I" And CS0021UPROFXLS.EFFECT(i) = "Y" And CS0021UPROFXLS.POSIX(i) > 0 And CS0021UPROFXLS.POSIY(i) > 0 Then
                    If CS0021UPROFXLS.POSIY(i) > 0 And CS0021UPROFXLS.POSIX(i) > 0 Then
                        WW_HENSYUrange(CS0021UPROFXLS.POSIY(i) - 1, CS0021UPROFXLS.POSIX(i) - 1) = CS0021UPROFXLS.FIELDNAME(i)
                    End If
                End If
            Next

            WW_EXCELrange.Value = WW_HENSYUrange                                                                                          'セルへデータの入力

            '　使い終わった時点で、WW_EXCELrange オブジェクトを解放 
            ExcelMemoryRelease(WW_EXCELrange)
        Catch ex As Exception
            O_ERR = "10042"
            CS0011LOGWRITE.INFSUBCLASS = "CS0022REPORT"                                                                                   'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "Excel_DetailHeader_Range"                                                                           '
            CS0011LOGWRITE.NIWEA = "A"                                                                                                    '
            CS0011LOGWRITE.TEXT = WW_Range_str
            CS0011LOGWRITE.MESSAGENO = "10042"
            CS0011LOGWRITE.CS0011LOGWrite()                                                                                               'ログ出力

            Try
                W_ExcelApp.Quit()
            Catch err As Exception
            End Try

            Try
                W_ExcelApp.Visible = True
            Catch err As Exception
            End Try

            Try
                ExcelMemoryRelease(W_ExcelSheet)      'ExcelSheet の解放
            Catch err As Exception
            End Try

            Try
                ExcelMemoryRelease(W_ExcelSheets)     'ExcelSheets の解放
            Catch err As Exception
            End Try

            Try
                ExcelMemoryRelease(W_ExcelBook)       'ExcelBook の解放
            Catch err As Exception
            End Try

            Try
                ExcelMemoryRelease(W_ExcelBooks)      'ExcelBooks の解放
            Catch err As Exception
            End Try

            Try
                ExcelMemoryRelease(W_ExcelApp)        'ExcelApp を解放
            Catch err As Exception
            End Try

            Exit Sub
        End Try

        '○画面選択明細(GridView)からExcel(明細)へ表示

        '　明細範囲指定
        Try
            WW_Range_str = "MaxX:" & CS0021UPROFXLS.POSI_I_Y_MAX.ToString & "_MaxY:" & CS0021UPROFXLS.POSI_I_X_MAX.ToString

            Dim WW_HENSYUrange(WW_IN_GridView.Rows.Count * CS0021UPROFXLS.POSI_I_Y_MAX - 1, CS0021UPROFXLS.POSI_I_X_MAX - 1) As Object          '行編集領域　　※開始位置(0,0) …　object

            WW_STARTpoint = W_ExcelSheet.Cells.Item(CS0021UPROFXLS.POSISTART + CS0021UPROFXLS.POSI_I_Y_MAX, 1)                               '指定された行開始のA列
            WW_ENDpoint = _
                W_ExcelSheet.Cells.Item(CS0021UPROFXLS.POSISTART + (WW_IN_GridView.Rows.Count + 1) * CS0021UPROFXLS.POSI_I_Y_MAX - 1, CS0021UPROFXLS.POSI_I_X_MAX)  '指定された行+明細行+ｱｲﾃﾑ行
            WW_EXCELrange = W_ExcelSheet.Range(WW_STARTpoint, WW_ENDpoint)                                                                'データの入力セル範囲

            '明細範囲の書式(文字形式)指定　…　文字化け対策
            WW_EXCELrange.NumberFormatLocal = "@"

            '　明細設定
            Dim WW_ColunCount As Integer
            WW_ColunCount = 0
            For i As Integer = 0 To WW_IN_GridView.Columns.Count - 1
                Dim WW_DataField As BoundField = WW_IN_GridView.Columns.Item(i) '項目名取得用
                Select Case WW_DataField.DataField
                    Case "SELECT"
                    Case "HIDDEN"

                    Case Else
                        For j As Integer = 0 To WW_IN_GridView.Rows.Count - 1
                            For k As Integer = 0 To CS0021UPROFXLS.TITOLKBN.Count - 1
                                If CS0021UPROFXLS.TITOLKBN(k) = "I" And CS0021UPROFXLS.EFFECT(k) = "Y" And CS0021UPROFXLS.POSIX(k) > 0 And CS0021UPROFXLS.POSIY(k) > 0 And _
                                    CS0021UPROFXLS.FIELD(k) = WW_DataField.DataField Then
                                    If CS0021UPROFXLS.POSIY(k) > 0 And CS0021UPROFXLS.POSIX(k) > 0 Then
                                        If WW_IN_GridView.Rows(j).Cells(i).Text = "&nbsp;" Then
                                            WW_HENSYUrange(j * CS0021UPROFXLS.POSI_I_Y_MAX + CS0021UPROFXLS.POSIY(k) - 1, CS0021UPROFXLS.POSIX(k) - 1) = ""
                                        Else
                                            WW_HENSYUrange(j * CS0021UPROFXLS.POSI_I_Y_MAX + CS0021UPROFXLS.POSIY(k) - 1, CS0021UPROFXLS.POSIX(k) - 1) = WW_IN_GridView.Rows(j).Cells(i).Text
                                        End If
                                        Exit For
                                    End If
                                End If
                            Next
                        Next
                End Select
            Next

            WW_EXCELrange.Value = WW_HENSYUrange          'セルへデータの入力

            '　使い終わった時点で、WW_EXCELrange オブジェクトを解放 
            ExcelMemoryRelease(WW_EXCELrange)
        Catch ex As Exception
            O_ERR = "10042"
            CS0011LOGWRITE.INFSUBCLASS = "CS0022REPORT"                 'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "Excel_Detail_Range"               '
            CS0011LOGWRITE.NIWEA = "A"                                  '
            CS0011LOGWRITE.TEXT = WW_Range_str
            CS0011LOGWRITE.MESSAGENO = "10042"
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力

            Try
                W_ExcelApp.Quit()
            Catch err As Exception
            End Try

            Try
                W_ExcelApp.Visible = True
            Catch err As Exception
            End Try

            Try
                ExcelMemoryRelease(W_ExcelSheet)      'ExcelSheet の解放
            Catch err As Exception
            End Try

            Try
                ExcelMemoryRelease(W_ExcelSheets)     'ExcelSheets の解放
            Catch err As Exception
            End Try

            Try
                ExcelMemoryRelease(W_ExcelBook)       'ExcelBook の解放
            Catch err As Exception
            End Try

            Try
                ExcelMemoryRelease(W_ExcelBooks)      'ExcelBooks の解放
            Catch err As Exception
            End Try

            Try
                ExcelMemoryRelease(W_ExcelApp)        'ExcelApp を解放
            Catch err As Exception
            End Try

            Exit Sub
        End Try

        '○Excel書式設定
        Try
            '列幅設定
            If WW_ExcelExist = "ON" Then
                '書式ありの場合、書式変更しない
            Else
                For i As Integer = 0 To CS0021UPROFXLS.TITOLKBN.Count - 1
                    If CS0021UPROFXLS.POSIX(i) <> 0 And CS0021UPROFXLS.WIDTH(i) <> 0 Then
                        Dim WW_Columns As Integer = CS0021UPROFXLS.POSIX(i)
                        W_ExcelSheet.Columns(WW_Columns).ColumnWidth = CS0021UPROFXLS.WIDTH(i)
                    End If
                Next
            End If
            '　EXCEL印刷書式設定
            W_ExcelSheet.PageSetup.Orientation = Excel.XlPageOrientation.xlLandscape
            W_ExcelSheet.PageSetup.TopMargin = 20
            W_ExcelSheet.PageSetup.BottomMargin = 20
            W_ExcelSheet.PageSetup.LeftMargin = 20
            W_ExcelSheet.PageSetup.RightMargin = 20
            W_ExcelSheet.PageSetup.Zoom = False
            W_ExcelSheet.PageSetup.FitToPagesWide = 1 '横を1ページに収める
            W_ExcelSheet.PageSetup.PrintTitleRows = "$1:$" & (CS0021UPROFXLS.POSISTART + CS0021UPROFXLS.POSI_I_Y_MAX - 1).ToString  'ページタイトル固定

        Catch ex As Exception
            O_ERR = "10042"
            CS0011LOGWRITE.INFSUBCLASS = "CS0022REPORT"                 'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "Excel_OverLay"                    '
            CS0011LOGWRITE.NIWEA = "A"                                  '
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = "10042"
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力

            Try
                W_ExcelApp.Quit()
            Catch err As Exception
            End Try

            Try
                W_ExcelApp.Visible = True
            Catch err As Exception
            End Try

            Try
                ExcelMemoryRelease(W_ExcelSheet)      'ExcelSheet の解放
            Catch err As Exception
            End Try

            Try
                ExcelMemoryRelease(W_ExcelSheets)     'ExcelSheets の解放
            Catch err As Exception
            End Try

            Try
                ExcelMemoryRelease(W_ExcelBook)       'ExcelBook の解放
            Catch err As Exception
            End Try

            Try
                ExcelMemoryRelease(W_ExcelBooks)      'ExcelBooks の解放
            Catch err As Exception
            End Try

            Try
                ExcelMemoryRelease(W_ExcelApp)        'ExcelApp を解放
            Catch err As Exception
            End Try

            Exit Sub
        End Try

        '～～～～～ データ設定 (終了) ～～～～～～～～～～～～～～～～

        '○EXCEL保存
        Dim WW_Dir As String

        Try
            '　印刷用フォルダ作成
            WW_Dir = HttpContext.Current.Session("FILEdir") & "\" & "PRINTWORK"
            '　格納フォルダ存在確認＆作成(C:\apple\files\PRINTWORK)
            If System.IO.Directory.Exists(WW_Dir) Then
            Else
                System.IO.Directory.CreateDirectory(WW_Dir)
            End If

            '　格納フォルダ存在確認＆作成(C:\apple\files\PRINTWORK\端末名)
            WW_Dir = HttpContext.Current.Session("FILEdir") & "\" & "PRINTWORK" & "\" & HttpContext.Current.Session("Term")
            If System.IO.Directory.Exists(WW_Dir) Then
            Else
                System.IO.Directory.CreateDirectory(WW_Dir)
            End If

            '　印刷用フォルダ内不要ファイル削除(当日以外のファイルは削除)
            WW_Dir = HttpContext.Current.Session("FILEdir") & "\" & "PRINTWORK" & "\" & HttpContext.Current.Session("Term")
            For Each FileName As String In System.IO.Directory.GetFiles(WW_Dir, "*.*")
                ' ファイルパスからファイル名を取得
                Do
                    FileName = Mid(FileName, InStr(FileName, "\") + 1, 100)
                Loop Until InStr(FileName, "\") = 0

                If FileName = "" Then
                Else
                    If IsNumeric(Mid(FileName, 1, 8)) And Mid(FileName, 1, 8) = Date.Now.ToString("yyyyMMdd") Then
                    Else
                        For Each tempFile As String In System.IO.Directory.GetFiles(WW_Dir)
                            System.IO.File.Delete(tempFile)
                        Next
                        Exit For
                    End If
                End If
            Next
        Catch ex As Exception
            O_ERR = "00004"
            CS0011LOGWRITE.INFSUBCLASS = "CS0022REPORT"                 'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "Excel_Folder"                     '
            CS0011LOGWRITE.NIWEA = "A"                                  '
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = "00004"
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力

            Try
                W_ExcelApp.Quit()
            Catch err As Exception
            End Try

            Try
                W_ExcelApp.Visible = True
            Catch err As Exception
            End Try

            Try
                ExcelMemoryRelease(W_ExcelSheet)      'ExcelSheet の解放
            Catch err As Exception
            End Try

            Try
                ExcelMemoryRelease(W_ExcelSheets)     'ExcelSheets の解放
            Catch err As Exception
            End Try

            Try
                ExcelMemoryRelease(W_ExcelBook)       'ExcelBook の解放
            Catch err As Exception
            End Try

            Try
                ExcelMemoryRelease(W_ExcelBooks)      'ExcelBooks の解放
            Catch err As Exception
            End Try

            Try
                ExcelMemoryRelease(W_ExcelApp)        'ExcelApp を解放
            Catch err As Exception
            End Try

            Exit Sub
        End Try

        '○保存時の問合せのダイアログを非表示に設定
        W_ExcelApp.DisplayAlerts = False

        '○ファイル(PDF,CSV)保存
        Dim WW_datetime As String = DateTime.Now.ToString("yyyyMMddHHmmss") & DateTime.Now.Millisecond.ToString
        If I_FILEtyp = "PDF" Then
            I_FILEtyp = "pdf"
        End If
        If I_FILEtyp = "CSV" Then
            I_FILEtyp = "csv"
        End If
        If I_FILEtyp = "XLS" Then
            I_FILEtyp = "xls"
        End If
        If I_FILEtyp = "XLSX" Then
            I_FILEtyp = "xlsx"
        End If
        If I_FILEtyp = "XLSM" Then
            I_FILEtyp = "xlsm"
        End If
        Try
            Select Case I_FILEtyp
                Case "pdf"
                    O_FILEpath = HttpContext.Current.Session("FILEdir") & "\" & "PRINTWORK" & "\" & HttpContext.Current.Session("Term") & "\" & _
                                   WW_datetime & ".pdf"
                    O_URL = ""
                    W_ExcelBook.ExportAsFixedFormat(Type:=0, _
                         Filename:=O_FILEpath, _
                         Quality:=0, _
                         IncludeDocProperties:=True, _
                         IgnorePrintAreas:=False, _
                         OpenAfterPublish:=False)
                Case "csv"
                    O_FILEpath = HttpContext.Current.Session("FILEdir") & "\" & "PRINTWORK" & "\" & HttpContext.Current.Session("Term") & "\" & _
                                   WW_datetime & ".CSV"
                    O_URL = "http://localhost/" & "PRINTWORK" & "/" & HttpContext.Current.Session("Term") & "/" & WW_datetime & ".CSV"
                    W_ExcelApp.DisplayAlerts = False
                    W_ExcelSheet.SaveAs(Filename:=O_FILEpath, FileFormat:=Excel.XlFileFormat.xlCSV)
                Case "xls"
                    O_FILEpath = HttpContext.Current.Session("FILEdir") & "\" & "PRINTWORK" & "\" & HttpContext.Current.Session("Term") & "\" & _
                                   WW_datetime & ".XLSX"
                    O_URL = "http://localhost/" & "PRINTWORK" & "/" & HttpContext.Current.Session("Term") & "/" & WW_datetime & ".XLS"
                    W_ExcelBook.SaveAs(O_FILEpath)
                Case "xlsx"
                    O_FILEpath = HttpContext.Current.Session("FILEdir") & "\" & "PRINTWORK" & "\" & HttpContext.Current.Session("Term") & "\" & _
                                   WW_datetime & ".XLSX"
                    O_URL = "http://localhost/" & "PRINTWORK" & "/" & HttpContext.Current.Session("Term") & "/" & WW_datetime & ".XLS"
                    W_ExcelBook.SaveAs(O_FILEpath)
                Case "xlsm"
                    O_FILEpath = HttpContext.Current.Session("FILEdir") & "\" & "PRINTWORK" & "\" & HttpContext.Current.Session("Term") & "\" & _
                                   WW_datetime & ".XLSM"
                    O_URL = "http://localhost/" & "PRINTWORK" & "/" & HttpContext.Current.Session("Term") & "/" & WW_datetime & ".XLSM"
                    'W_ExcelBook.SaveAs(O_FILEpath)
                    W_ExcelBook.SaveAs(Filename:=O_FILEpath, FileFormat:=Excel.XlFileFormat.xlOpenXMLWorkbookMacroEnabled)
            End Select

        Catch ex As Exception
            O_ERR = "00004"
            CS0011LOGWRITE.INFSUBCLASS = "CS0022REPORT"                 'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "Excel_Save"                       '
            CS0011LOGWRITE.NIWEA = "A"                                  '
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = "00004"
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力

            Try
                W_ExcelApp.Quit()
            Catch err As Exception
            End Try

            Try
                W_ExcelApp.Visible = True
            Catch err As Exception
            End Try

            Try
                ExcelMemoryRelease(W_ExcelSheet)      'ExcelSheet の解放
            Catch err As Exception
            End Try

            Try
                ExcelMemoryRelease(W_ExcelSheets)     'ExcelSheets の解放
            Catch err As Exception
            End Try

            Try
                ExcelMemoryRelease(W_ExcelBook)       'ExcelBook の解放
            Catch err As Exception
            End Try

            Try
                ExcelMemoryRelease(W_ExcelBooks)      'ExcelBooks の解放
            Catch err As Exception
            End Try

            Try
                ExcelMemoryRelease(W_ExcelApp)        'ExcelApp を解放
            Catch err As Exception
            End Try

            Exit Sub

        End Try

        '○1秒間表示して終了処理へ
        'System.Threading.Thread.Sleep(1000)

        '○Excel終了＆リリース
        Try
            W_ExcelApp.Quit()
        Catch err As Exception
        End Try

        Try
            W_ExcelApp.Visible = True
        Catch err As Exception
        End Try

        Try
            ExcelMemoryRelease(W_ExcelSheet)      'ExcelSheet の解放
        Catch err As Exception
        End Try

        Try
            ExcelMemoryRelease(W_ExcelSheets)     'ExcelSheets の解放
        Catch err As Exception
        End Try

        Try
            ExcelMemoryRelease(W_ExcelBook)       'ExcelBook の解放
        Catch err As Exception
        End Try

        Try
            ExcelMemoryRelease(W_ExcelBooks)      'ExcelBooks の解放
        Catch err As Exception
        End Try

        Try
            ExcelMemoryRelease(W_ExcelApp)        'ExcelApp を解放
        Catch err As Exception
        End Try

        O_ERR = "00000"

    End Sub

    ' Excel操作のメモリ開放
    Public Sub ExcelMemoryRelease(Of T As Class)(ByRef objCom As T)
        'ランタイム実行対象がComObjectのアンマネージコードの場合、メモリ開放
        If objCom Is Nothing Then
            Return
        Else
            Try
                If System.Runtime.InteropServices.Marshal.IsComObject(objCom) Then
                    Dim count As Integer = System.Runtime.InteropServices.Marshal.FinalReleaseComObject(objCom)
                End If
            Finally
                objCom = Nothing
            End Try
        End If
    End Sub

End Structure
