﻿Imports System.IO
Imports Microsoft.Office.Interop
Imports System.Runtime.InteropServices

''' <summary>
''' XLSアップロード
''' </summary>
''' <remarks></remarks>
Public Structure CS0023XLSUPLOAD

    ''' <summary>
    ''' 会社コード
    ''' </summary>
    ''' <value>会社コード</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property CAMPCODE As String

    ''' <summary>
    ''' プロファイルID
    ''' </summary>
    ''' <value></value>
    ''' <returns>プロファイルID</returns>
    ''' <remarks></remarks>
    Public Property PROFID As String

    ''' <summary>
    ''' 画面ID
    ''' </summary>
    ''' <value>画面ID</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property MAPID As String

    ''' <summary>
    ''' 帳票ID
    ''' </summary>
    ''' <value></value>
    ''' <returns>帳票ID</returns>
    ''' <remarks></remarks>
    Public Property REPORTID As String

    ''' <summary>
    ''' 結果tabledata
    ''' </summary>
    ''' <value></value>
    ''' <returns>結果tabledata</returns>
    ''' <remarks></remarks>
    Public Property TBLDATA As DataTable

    ''' <summary>
    ''' エラーコード
    ''' </summary>
    ''' <value></value>
    ''' <returns>エラーコード</returns>
    ''' <remarks>OK:00000,ERR:00002(環境エラー),ERR:00003(DBerr)</remarks>
    Public Property ERR As String

    Private Const METHOD_NAME = "CS0023XLSUPLOAD"

    ''' <summary>
    ''' XLSアップロード
    ''' </summary>
    ''' <param name="I_REPORTID">帳票ID</param>
    ''' <param name="I_PROFID">PROFID</param>
    ''' <remarks>OK:00000,ERR:00002(環境エラー),ERR:00003(DBerr)</remarks>
    Public Sub CS0023XLSUPLOAD(Optional ByVal I_REPORTID As String = "", Optional ByVal I_PROFID As String = "")

        '■共通宣言
        Dim CS0011LOGWRITE As New CS0011LOGWrite                'LogOutput DirString Get
        Dim CS0021PROFXLS As New CS0021PROFXLS                  'プロファイル(帳票)取得
        Dim CS0028STRUCT As New CS0028STRUCT                    '構造取得
        Dim CS0050SESSION As New CS0050SESSION                  'セッション情報操作処理

        Dim W_ExcelApp As Excel.Application = Nothing
        Dim W_ExcelBooks As Excel.Workbooks = Nothing
        Dim W_ExcelBook As Excel.Workbook = Nothing
        Dim W_ExcelSheets As Excel.Sheets = Nothing
        Dim W_ExcelSheet As Excel.Worksheet = Nothing

        '●InPARAMチェック
        'CAMPCODE
        If IsNothing(CAMPCODE) Then
            ERR = C_MESSAGE_NO.DLL_IF_ERROR

            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME                        'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "CAMPCODE"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = C_MESSAGE_TEXT.IN_PARAM_ERROR_TEXT
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                                 'ログ出力
        End If

        'MAPID
        If IsNothing(MAPID) Then
            ERR = C_MESSAGE_NO.DLL_IF_ERROR

            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME                        'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "MAPID"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = C_MESSAGE_TEXT.IN_PARAM_ERROR_TEXT
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                                 'ログ出力
            Exit Sub
        End If

        '■アップロードFILEディレクトリ取得
        Dim WW_FILEnm As String = ""

        Try
            For Each tempFile As String In Directory.GetFiles(CS0050SESSION.UPLOAD_PATH & "\UPLOAD_TMP\" & CS0050SESSION.USERID, "*.*")
                ' ファイルパスからファイル名を取得
                WW_FILEnm = tempFile
                Exit For
            Next

            If WW_FILEnm = "" Then
                ERR = C_MESSAGE_NO.REPORT_EXCEL_NOT_FOUND_ERROR

                CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME                                    'SUBクラス名
                CS0011LOGWRITE.INFPOSI = "EXCEL read"
                CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ERR
                CS0011LOGWRITE.TEXT = C_MESSAGE_TEXT.IN_PARAM_ERROR_TEXT
                CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.REPORT_EXCEL_NOT_FOUND_ERROR
                CS0011LOGWRITE.CS0011LOGWrite()                                             'ログ出力
                Exit Sub
            End If
        Catch ex As Exception
            ERR = C_MESSAGE_NO.REPORT_EXCEL_NOT_FOUND_ERROR

            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME                                    'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "EXCEL read"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ERR
            CS0011LOGWRITE.TEXT = C_MESSAGE_TEXT.IN_PARAM_ERROR_TEXT
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.REPORT_EXCEL_NOT_FOUND_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                                             'ログ出力
            Exit Sub
        End Try

        '■Excel起動
        Dim xlElement As Excel.Worksheet = Nothing
        Try
            W_ExcelApp = New Excel.Application
            W_ExcelBooks = W_ExcelApp.Workbooks
            W_ExcelBook = W_ExcelBooks.Open(WW_FILEnm)
            W_ExcelSheets = W_ExcelBook.Worksheets

            'シート名の取得
            Dim W_FIND As String = "OFF"

            For Each xlElement In W_ExcelSheets
                If xlElement.Name = "入力" Then
                    W_ExcelSheet = CType(xlElement, Excel.Worksheet)
                    W_FIND = "ON"
                    Exit For
                End If
                ExcelMemoryRelease(xlElement)
            Next

            If W_FIND = "OFF" Then
                For Each xlElement In W_ExcelSheets
                    If xlElement.Name = "入出力" Then
                        W_ExcelSheet = CType(xlElement, Excel.Worksheet)
                        W_FIND = "ON"
                        Exit For
                    End If
                    ExcelMemoryRelease(xlElement)
                Next

            End If
            If W_FIND = "OFF" Then
                W_ExcelSheet = CType(W_ExcelSheets.Item(1), Excel.Worksheet)
            End If

            W_ExcelApp.Visible = False

        Catch ex As Exception
            'EXCEL OPENエラー
            ERR = C_MESSAGE_NO.EXCEL_OPEN_ERROR

            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME            'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "Excel_Open"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = ERR
            CS0011LOGWRITE.CS0011LOGWrite()                     'ログ出力

            'Excel終了＆リリース
            ExcelMemoryRelease(xlElement)
            CloseExcel(W_ExcelApp, W_ExcelBooks, W_ExcelBook, W_ExcelSheets, W_ExcelSheet)
            Exit Sub
        End Try


        '～～～～～ データ取得 (開始) ～～～～～～～～～～～～～～～～

        '■Excelデータ設定
        Dim WW_Cells As Excel.Range = Nothing
        Dim WW_EXCELrange As Excel.Range = Nothing
        Dim WW_STARTpoint As Excel.Range = Nothing
        Dim WW_ENDpoint As Excel.Range = Nothing

        '○Excel(タイトル)よりプロファイルID、レポートID取得
        PROFID = ""
        REPORTID = ""
        Dim SCOLON As Integer = 0

        Try
            Dim WW_EXCELdat(0, 99) As Object     '行編集領域

            '　タイトル(1行目)範囲指定
            WW_Cells = W_ExcelSheet.Cells
            WW_STARTpoint = DirectCast(WW_Cells.Item(1, 1), Excel.Range)        'A1
            WW_ENDpoint = DirectCast(WW_Cells.Item(50, 100), Excel.Range)       'CV1
            WW_EXCELrange = W_ExcelSheet.Range(WW_STARTpoint, WW_ENDpoint)      'データの入力セル範囲

            '　1行目データからレポートIDとプロファイルID("ID:")を探す
            WW_EXCELdat = WW_EXCELrange.Value          'EXCELデータ取得
            For i As Integer = 1 To 50
                For j As Integer = 1 To 100
                    If InStr(WW_EXCELdat(i, j), "ID:") > 0 Then
                        REPORTID = Trim(WW_EXCELdat(i, j).Replace("ID:", ""))
                        If InStr(REPORTID, ";") > 0 Then
                            SCOLON = InStr(REPORTID, ";")
                            PROFID = Mid(REPORTID, SCOLON + 1, Len(REPORTID))
                            REPORTID = Mid(REPORTID, 1, SCOLON - 1)
                        End If
                        Exit For
                    End If
                Next
                If SCOLON > 0 Then Exit For
            Next

            'REPORTID取得できない場合はデフォルトIDを設定
            If String.IsNullOrEmpty(REPORTID) Then
                REPORTID = I_REPORTID
                PROFID = I_PROFID
            End If
            'PROFID取得できない場合はデフォルトIDを設定
            If String.IsNullOrEmpty(PROFID) Then
                PROFID = I_PROFID
            End If

            If REPORTID = "" Then
                ERR = C_MESSAGE_NO.REPORT_ID_NOT_EXISTS

                CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME                            'SUBクラス名
                CS0011LOGWRITE.INFPOSI = "Excel ID not findE"
                CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ERR
                CS0011LOGWRITE.TEXT = WW_FILEnm
                CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.REPORT_ID_NOT_EXISTS
                CS0011LOGWRITE.CS0011LOGWrite()                                     'ログ出力

                'Excel.Range 解放
                ExcelMemoryRelease(WW_Cells)
                ExcelMemoryRelease(WW_STARTpoint)
                ExcelMemoryRelease(WW_ENDpoint)
                ExcelMemoryRelease(WW_EXCELrange)

                'Excel終了＆リリース
                CloseExcel(W_ExcelApp, W_ExcelBooks, W_ExcelBook, W_ExcelSheets, W_ExcelSheet)
                Exit Sub
            End If
        Catch ex As Exception
            '他Excel処理完了待ち
            ERR = C_MESSAGE_NO.WAIT_OTHER_EXCEL_JOB

            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME                            'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "Excel_Titol_Range"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.WAIT_OTHER_EXCEL_JOB
            CS0011LOGWRITE.CS0011LOGWrite()                                     'ログ出力

            'Excel.Range 解放
            ExcelMemoryRelease(WW_Cells)
            ExcelMemoryRelease(WW_STARTpoint)
            ExcelMemoryRelease(WW_ENDpoint)
            ExcelMemoryRelease(WW_EXCELrange)

            'Excel終了＆リリース
            CloseExcel(W_ExcelApp, W_ExcelBooks, W_ExcelBook, W_ExcelSheets, W_ExcelSheet)
            Exit Sub
        Finally
            'Excel.Range 解放
            ExcelMemoryRelease(WW_Cells)
            ExcelMemoryRelease(WW_STARTpoint)
            ExcelMemoryRelease(WW_ENDpoint)
            ExcelMemoryRelease(WW_EXCELrange)
        End Try

        '■レポートレイアウト取得
        CS0021PROFXLS.CAMPCODE = CAMPCODE
        CS0021PROFXLS.PROFID = PROFID
        CS0021PROFXLS.MAPID = MAPID
        CS0021PROFXLS.REPORTID = REPORTID
        CS0021PROFXLS.CS0021PROFXLS()
        If isNormal(CS0021PROFXLS.ERR) Then
            If Not REPORTID = CS0021PROFXLS.REPORTID Then
                ERR = C_MESSAGE_NO.REPORT_ID_NOT_EXISTS

                CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME                            'SUBクラス名
                CS0011LOGWRITE.INFPOSI = "CS0021PROFXLS call"
                CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ERR
                CS0011LOGWRITE.TEXT = "帳票IDが存在しません。"
                CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.REPORT_ID_NOT_EXISTS
                CS0011LOGWRITE.CS0011LOGWrite()                                     'ログ出力

                'Excel終了＆リリース
                CloseExcel(W_ExcelApp, W_ExcelBooks, W_ExcelBook, W_ExcelSheets, W_ExcelSheet)
                Exit Sub
            End If
        Else
            '帳票ID未存在エラー
            ERR = C_MESSAGE_NO.REPORT_ID_NOT_EXISTS

            'Excel終了＆リリース
            CloseExcel(W_ExcelApp, W_ExcelBooks, W_ExcelBook, W_ExcelSheets, W_ExcelSheet)
            Exit Sub
        End If

        If CS0021PROFXLS.POSISTART = 0 Then
            CS0021PROFXLS.POSISTART = 1
        End If
        If CS0021PROFXLS.POSI_T_X_MAX = 0 Then
            CS0021PROFXLS.POSI_T_X_MAX = 1
        End If
        If CS0021PROFXLS.POSI_T_Y_MAX = 0 Then
            CS0021PROFXLS.POSI_T_Y_MAX = 1
        End If

        If CS0021PROFXLS.POSI_I_X_MAX = 0 Then
            CS0021PROFXLS.POSI_I_X_MAX = 1
        End If
        If CS0021PROFXLS.POSI_I_Y_MAX = 0 Then
            CS0021PROFXLS.POSI_I_Y_MAX = 1
        End If

        '■Excel(明細)データ格納準備（テーブル列追加）
        Dim WW_TBLDATA As New DataTable
        Dim WW_TBLDATArow As DataRow
        WW_TBLDATA.Clear()

        For i As Integer = 0 To CS0021PROFXLS.TITLEKBN.Count - 1
            If (CS0021PROFXLS.TITLEKBN(i) = "T" Or CS0021PROFXLS.TITLEKBN(i) = "I" Or CS0021PROFXLS.TITLEKBN(i) = "I_Data" Or CS0021PROFXLS.TITLEKBN(i) = "I_DataKey") And _
                CS0021PROFXLS.EFFECT(i) = "Y" Then
                '出力DATATABLEに列(項目)追加
                WW_TBLDATA.Columns.Add(CS0021PROFXLS.FIELD(i), GetType(String))
            End If
        Next

        '■明細データソート・性能対策

        '性能対策用(明細)  …  前提：CS0021PROFXLS出力パラListは、SORT順に格納されている
        Dim WW_I_TITOLKBN As List(Of String) = New List(Of String)
        Dim WW_I_FIELD As List(Of String) = New List(Of String)
        Dim WW_I_FIELDNAME As List(Of String) = New List(Of String)
        Dim WW_I_STRUCT As List(Of String) = New List(Of String)
        Dim WW_I_POSIX As List(Of Integer) = New List(Of Integer)
        Dim WW_I_POSIY As List(Of Integer) = New List(Of Integer)
        Dim WW_I_WIDTH As List(Of Integer) = New List(Of Integer)
        Dim WW_I_EFFECT As List(Of String) = New List(Of String)
        Dim WW_I_SORT As List(Of Integer) = New List(Of Integer)

        '性能対策用(明細データ)
        Dim WW_R_TITOLKBN As List(Of String) = New List(Of String)
        Dim WW_R_FIELD As List(Of String) = New List(Of String)
        Dim WW_R_FIELDNAME As List(Of String) = New List(Of String)
        Dim WW_R_STRUCT As List(Of String) = New List(Of String)
        Dim WW_R_POSIX As List(Of Integer) = New List(Of Integer)
        Dim WW_R_POSIY As List(Of Integer) = New List(Of Integer)
        Dim WW_R_WIDTH As List(Of Integer) = New List(Of Integer)
        Dim WW_R_EFFECT As List(Of String) = New List(Of String)

        For i As Integer = 0 To CS0021PROFXLS.TITLEKBN.Count - 1
            If CS0021PROFXLS.TITLEKBN(i) = "I" And CS0021PROFXLS.EFFECT(i) = "Y" And CS0021PROFXLS.POSIY(i) > 0 And CS0021PROFXLS.POSIX(i) > 0 Then
                WW_I_TITOLKBN.Add(CS0021PROFXLS.TITLEKBN(i))
                WW_I_FIELD.Add(CS0021PROFXLS.FIELD(i))
                WW_I_FIELDNAME.Add(CS0021PROFXLS.FIELDNAME(i))
                WW_I_STRUCT.Add(CS0021PROFXLS.STRUCT(i))
                WW_I_POSIX.Add(CS0021PROFXLS.POSIX(i))
                WW_I_POSIY.Add(CS0021PROFXLS.POSIY(i))
                WW_I_WIDTH.Add(CS0021PROFXLS.WIDTH(i))
                WW_I_EFFECT.Add(CS0021PROFXLS.EFFECT(i))
                WW_I_SORT.Add(CS0021PROFXLS.SORT(i))
            End If

            If ((CS0021PROFXLS.TITLEKBN(i) = "I_DataKey") Or (CS0021PROFXLS.TITLEKBN(i) = "I_Data")) And CS0021PROFXLS.EFFECT(i) = "Y" Then
                WW_R_TITOLKBN.Add(CS0021PROFXLS.TITLEKBN(i))
                WW_R_FIELD.Add(CS0021PROFXLS.FIELD(i))
                WW_R_FIELDNAME.Add(CS0021PROFXLS.FIELDNAME(i))
                WW_R_STRUCT.Add(CS0021PROFXLS.STRUCT(i))
                WW_R_POSIX.Add(CS0021PROFXLS.POSIX(i))
                WW_R_POSIY.Add(CS0021PROFXLS.POSIY(i))
                WW_R_WIDTH.Add(CS0021PROFXLS.WIDTH(i))
                WW_R_EFFECT.Add(CS0021PROFXLS.EFFECT(i))
            End If
        Next

        '■構造値格納テーブル作成
        'テーブル定義
        Dim WW_STRUCT_TBLDATA As New DataTable
        Dim WW_STRUCT_TBLDATArow As DataRow
        WW_STRUCT_TBLDATA.Clear()

        For i As Integer = 0 To CS0021PROFXLS.TITLEKBN.Count - 1
            If CS0021PROFXLS.TITLEKBN(i) = "I_DataKey" And CS0021PROFXLS.EFFECT(i) = "Y" Then
                '出力DATATABLEに列(項目)追加
                WW_STRUCT_TBLDATA.Columns.Add(CS0021PROFXLS.FIELD(i), GetType(String))
            End If
        Next

        '構造データ取得　
        Dim WW_CELL_KEY As List(Of String) = New List(Of String)

        For i As Integer = 0 To CS0021PROFXLS.TITLEKBN.Count - 1

            If CS0021PROFXLS.TITLEKBN(i) = "I_DataKey" And CS0021PROFXLS.EFFECT(i) = "Y" Then

                '構造データ取得　
                If CS0021PROFXLS.STRUCT(i) <> "" Then

                    CS0028STRUCT.CAMPCODE = CAMPCODE
                    CS0028STRUCT.STRUCT = CS0021PROFXLS.STRUCT(i)
                    CS0028STRUCT.CS0028STRUCT()
                    If isNormal(CS0028STRUCT.ERR) Then
                        '構造取得
                        If WW_CELL_KEY.Count = 0 Then
                            For CNT As Integer = 0 To CS0028STRUCT.CODE.Count - 1
                                '構造データ追加
                                WW_STRUCT_TBLDATArow = WW_STRUCT_TBLDATA.NewRow()
                                WW_STRUCT_TBLDATArow(CS0021PROFXLS.FIELD(i)) = CS0028STRUCT.CODE(CNT)
                                WW_STRUCT_TBLDATA.Rows.Add(WW_STRUCT_TBLDATArow)

                                WW_CELL_KEY.Add(CS0028STRUCT.CODE(CNT))
                            Next
                        Else
                            '複数定義された構造の列数が全て一致
                            If WW_CELL_KEY.Count = CS0028STRUCT.CODE.Count Then
                                '構造データ更新
                                For CNT As Integer = 0 To CS0028STRUCT.CODE.Count - 1
                                    WW_STRUCT_TBLDATA.Rows(CNT).Item(CS0021PROFXLS.FIELD(i)) = CS0028STRUCT.CODE(CNT)

                                    WW_CELL_KEY(CNT) = WW_CELL_KEY(CNT) & "_" & CS0028STRUCT.CODE(CNT)
                                Next
                            Else
                                'Excel書式定義エラー
                                ERR = C_MESSAGE_NO.EXCEL_COLUMNS_FORMAT_ERROR

                                CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME                    'SUBクラス名
                                CS0011LOGWRITE.INFPOSI = "CS0021PROFXLS"
                                CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ERR
                                CS0011LOGWRITE.TEXT = "Excel書式(列構造定義)不良"
                                CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
                                CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力

                                'ワークテーブル解放
                                Try
                                    WW_TBLDATA.Dispose()
                                    WW_TBLDATA = Nothing
                                    WW_STRUCT_TBLDATA.Dispose()
                                    WW_STRUCT_TBLDATA = Nothing
                                Catch ex As Exception
                                End Try
                                Exit Sub
                            End If
                        End If
                    Else
                        ERR = C_MESSAGE_NO.EXCEL_COLUMNS_FORMAT_ERROR

                        CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME                    'SUBクラス名
                        CS0011LOGWRITE.INFPOSI = "CS0021PROFXLS"
                        CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ERR
                        CS0011LOGWRITE.TEXT = "Excel書式(列構造定義)不良"
                        CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
                        CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力

                        'ワークテーブル解放
                        Try
                            WW_TBLDATA.Dispose()
                            WW_TBLDATA = Nothing
                            WW_STRUCT_TBLDATA.Dispose()
                            WW_STRUCT_TBLDATA = Nothing
                        Catch ex As Exception
                        End Try
                        Exit Sub
                    End If
                Else
                    'Excel書式定義エラー
                    ERR = C_MESSAGE_NO.EXCEL_COLUMNS_FORMAT_ERROR

                    CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME                    'SUBクラス名
                    CS0011LOGWRITE.INFPOSI = "CS0021PROFXLS"
                    CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ERR
                    CS0011LOGWRITE.TEXT = "Excel書式(列構造定義)不良"
                    CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
                    CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力

                    'ワークテーブル解放
                    Try
                        WW_TBLDATA.Dispose()
                        WW_TBLDATA = Nothing
                        WW_STRUCT_TBLDATA.Dispose()
                        WW_STRUCT_TBLDATA = Nothing
                    Catch ex As Exception
                    End Try
                    Exit Sub
                End If
            End If
        Next

        '■Excelデータ取得

        Dim WW_DATcnt As Integer = 0
        Dim WW_LoopEND As Integer = 1      '明細に何もない場合、０となる
        Dim WW_HENSYUrange(,) As Object = Nothing

        '■Excel(明細)データ取得
        If WW_CELL_KEY.Count <= 0 Then
            '******************************************************************
            '*  明細(I)処理                                                   *
            '******************************************************************
            Do
                Try
                    WW_DATcnt = WW_DATcnt + 1
                    WW_LoopEND = 0

                    '○１明細分のセルデータ切り出し領域(Excel内ｎ件目明細部データ→WW_HENSYUrange)

                    ReDim WW_HENSYUrange(CS0021PROFXLS.POSI_I_Y_MAX - 1, CS0021PROFXLS.POSI_I_X_MAX - 1)      '行編集領域
                    WW_Cells = W_ExcelSheet.Cells

                    '　ｎ件目の明細データ開始位置＝明細タイトル開始位置+明細MAX行　…　明細タイトルを考慮する事
                    WW_STARTpoint = DirectCast(WW_Cells.Item(CS0021PROFXLS.POSISTART + (WW_DATcnt) * CS0021PROFXLS.POSI_I_Y_MAX, 1), Excel.Range)     'A
                    '　ｎ件目の明細データ終了位置＝明細タイトル開始位置+明細MAX行*(ｎ+1)-1　…　明細タイトルを考慮する事
                    WW_ENDpoint = DirectCast(WW_Cells.Item(CS0021PROFXLS.POSISTART + (WW_DATcnt + 1) * CS0021PROFXLS.POSI_I_Y_MAX - 1, CS0021PROFXLS.POSI_I_X_MAX), Excel.Range)
                    WW_EXCELrange = W_ExcelSheet.Range(WW_STARTpoint, WW_ENDpoint)           'Excelデータの入力セル範囲

                    WW_HENSYUrange = WW_EXCELrange.Value

                    '○明細データ取得
                    WW_TBLDATArow = WW_TBLDATA.NewRow()

                    For i As Integer = 0 To WW_I_TITOLKBN.Count - 1

                        If WW_HENSYUrange(WW_I_POSIY(i), WW_I_POSIX(i)) = Nothing Then
                            If IsNothing(WW_HENSYUrange(WW_I_POSIY(i), WW_I_POSIX(i))) Then
                                'WW_TBLDATArow(WW_I_FIELD(i)) = ""
                            Else
                                Select Case WW_HENSYUrange(WW_I_POSIY(i), WW_I_POSIX(i)).GetType.ToString
                                    'Case "System.String"
                                    '    WW_TBLDATArow(WW_I_FIELD(i)) = ""
                                    Case "System.Integer"
                                        WW_TBLDATArow(WW_I_FIELD(i)) = "0"
                                    Case "System.Long"
                                        WW_TBLDATArow(WW_I_FIELD(i)) = "0"
                                    Case "System.Short"
                                        WW_TBLDATArow(WW_I_FIELD(i)) = "0"
                                    Case "System.Decimal"
                                        WW_TBLDATArow(WW_I_FIELD(i)) = "0"
                                    Case "System.Single"
                                        WW_TBLDATArow(WW_I_FIELD(i)) = "0"
                                    Case "System.Double"
                                        WW_TBLDATArow(WW_I_FIELD(i)) = "0"
                                    Case "System.Date"
                                        WW_TBLDATArow(WW_I_FIELD(i)) = "2000/1/1"
                                        'Case "Nothing"
                                        'Case Else
                                        '    WW_TBLDATArow(WW_I_FIELD(i)) = ""
                                End Select
                            End If
                        Else
                            Select Case WW_HENSYUrange(WW_I_POSIY(i), WW_I_POSIX(i)).GetType.ToString
                                Case "System.String"
                                    WW_TBLDATArow(WW_I_FIELD(i)) = WW_HENSYUrange(WW_I_POSIY(i), WW_I_POSIX(i))
                                    WW_LoopEND = 1
                                Case "System.Integer"
                                    WW_TBLDATArow(WW_I_FIELD(i)) = WW_HENSYUrange(WW_I_POSIY(i), WW_I_POSIX(i)).ToString
                                    WW_LoopEND = 1
                                Case "System.Long"
                                    WW_TBLDATArow(WW_I_FIELD(i)) = WW_HENSYUrange(WW_I_POSIY(i), WW_I_POSIX(i)).ToString
                                    WW_LoopEND = 1
                                Case "System.Short"
                                    WW_TBLDATArow(WW_I_FIELD(i)) = WW_HENSYUrange(WW_I_POSIY(i), WW_I_POSIX(i)).ToString
                                    WW_LoopEND = 1
                                Case "System.Decimal"
                                    WW_TBLDATArow(WW_I_FIELD(i)) = WW_HENSYUrange(WW_I_POSIY(i), WW_I_POSIX(i)).ToString
                                    WW_LoopEND = 1
                                Case "System.Single"
                                    WW_TBLDATArow(WW_I_FIELD(i)) = WW_HENSYUrange(WW_I_POSIY(i), WW_I_POSIX(i)).ToString
                                    WW_LoopEND = 1
                                Case "System.Double"
                                    WW_TBLDATArow(WW_I_FIELD(i)) = WW_HENSYUrange(WW_I_POSIY(i), WW_I_POSIX(i)).ToString
                                    WW_LoopEND = 1
                                Case "System.Date"
                                    WW_TBLDATArow(WW_I_FIELD(i)) = WW_HENSYUrange(WW_I_POSIY(i), WW_I_POSIX(i)).ToString("yyyy/MM/dd")
                                    WW_LoopEND = 1
                                Case "Nothing"
                                Case Else
                                    WW_TBLDATArow(WW_I_FIELD(i)) = WW_HENSYUrange(WW_I_POSIY(i), WW_I_POSIX(i)).ToString
                                    WW_LoopEND = 1
                            End Select
                        End If
                    Next

                    If WW_LoopEND = 1 Then
                        WW_TBLDATA.Rows.Add(WW_TBLDATArow)
                    End If
                Catch ex As Exception
                    '他Excel処理完了待ち
                    ERR = C_MESSAGE_NO.WAIT_OTHER_EXCEL_JOB

                    CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME                            'SUBクラス名
                    CS0011LOGWRITE.INFPOSI = "Excel_Detail_Range"
                    CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
                    CS0011LOGWRITE.TEXT = ex.ToString()
                    CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.WAIT_OTHER_EXCEL_JOB
                    CS0011LOGWRITE.CS0011LOGWrite()                                     'ログ出力

                    'Excel終了＆リリース
                    CloseExcel(W_ExcelApp, W_ExcelBooks, W_ExcelBook, W_ExcelSheets, W_ExcelSheet)

                    'ワークテーブル解放
                    Try
                        WW_TBLDATA.Dispose()
                        WW_TBLDATA = Nothing
                        WW_STRUCT_TBLDATA.Dispose()
                        WW_STRUCT_TBLDATA = Nothing
                    Catch err As Exception
                    End Try
                    Exit Sub
                Finally
                    'Excel.Range 解放
                    ExcelMemoryRelease(WW_HENSYUrange)
                    ExcelMemoryRelease(WW_Cells)
                    ExcelMemoryRelease(WW_STARTpoint)
                    ExcelMemoryRelease(WW_ENDpoint)
                    ExcelMemoryRelease(WW_EXCELrange)
                End Try

            Loop Until WW_LoopEND = 0
        Else
            '******************************************************************
            '*  明細(I_Data,I_DataKey)処理                                    *
            '******************************************************************
            Do
                Try
                    WW_DATcnt = WW_DATcnt + 1
                    WW_LoopEND = 0

                    '○１明細分のセルデータ切り出し領域(Excel内ｎ件目明細部データ→WW_HENSYUrange)

                    ReDim WW_HENSYUrange(CS0021PROFXLS.POSI_I_Y_MAX - 1, _
                                       CS0021PROFXLS.POSI_I_X_MAX + CS0021PROFXLS.POSI_R_X_MAX * WW_CELL_KEY.Count - 1)       '行編集領域
                    WW_Cells = W_ExcelSheet.Cells

                    'Dim WW_HENSYUrange(Math.Max(CS0021UPROFXLS.POSI_I_Y_MAX, CS0021UPROFXLS.POSI_R_Y_MAX) - 1, _
                    '                   CS0021UPROFXLS.POSI_I_X_MAX + CS0021UPROFXLS.POSI_R_X_MAX * WW_CELL_KEY.Count - 1) As Object             '行編集領域

                    '　ｎ件目の明細データ開始位置＝開始位置 + 明細タイトル行 + 明細MAX行 * (n - 1)     
                    'WW_STARTpoint = W_ExcelSheet.Cells.Item(CS0021UPROFXLS.POSISTART + CS0021UPROFXLS.POSI_I_Y_MAX + (WW_DATcnt - 1) * CS0021UPROFXLS.POSI_I_Y_MAX, 1)
                    WW_STARTpoint = DirectCast(WW_Cells.Item(CS0021PROFXLS.POSISTART + CS0021PROFXLS.POSI_I_Y_MAX + (WW_DATcnt - 1) * Math.Max(CS0021PROFXLS.POSI_I_Y_MAX, CS0021PROFXLS.POSI_R_Y_MAX), 1), Excel.Range)

                    '　ｎ件目の明細データ終了位置＝開始位置 + 明細タイトル行 + 明細MAX行 * (n    ) -1　
                    '　ｎ列目の明細データ終了位置＝明細タイトル行 + 明細MAX行 * (n    ) -1　
                    'WW_ENDpoint = W_ExcelSheet.Cells.Item(CS0021UPROFXLS.POSISTART + CS0021UPROFXLS.POSI_I_Y_MAX + (WW_DATcnt) * CS0021UPROFXLS.POSI_I_Y_MAX - 1, _
                    '                                      CS0021UPROFXLS.POSI_I_X_MAX + CS0021UPROFXLS.POSI_R_X_MAX * WW_CELL_KEY.Count)
                    WW_ENDpoint = DirectCast(WW_Cells.Item(CS0021PROFXLS.POSISTART + CS0021PROFXLS.POSI_I_Y_MAX + (WW_DATcnt) * Math.Max(CS0021PROFXLS.POSI_I_Y_MAX, CS0021PROFXLS.POSI_R_Y_MAX) - 1, _
                                                          CS0021PROFXLS.POSI_I_X_MAX + CS0021PROFXLS.POSI_R_X_MAX * WW_CELL_KEY.Count), Excel.Range)
                    'Excelデータの入力セル範囲
                    WW_EXCELrange = W_ExcelSheet.Range(WW_STARTpoint, WW_ENDpoint)

                    WW_HENSYUrange = WW_EXCELrange.Value

                    Dim WW_RecWrite As Integer = 0

                    For CNT As Integer = 0 To WW_CELL_KEY.Count - 1

                        WW_RecWrite = 0

                        '○明細データ取得
                        WW_TBLDATArow = WW_TBLDATA.NewRow()

                        '明細アイテム(I)
                        For i As Integer = 0 To WW_I_TITOLKBN.Count - 1
                            If WW_HENSYUrange(WW_I_POSIY(i), WW_I_POSIX(i)) = Nothing Then
                            Else
                                WW_LoopEND = 1
                                Select Case WW_HENSYUrange(WW_I_POSIY(i), WW_I_POSIX(i)).GetType.ToString
                                    Case "System.String"
                                        WW_TBLDATArow(WW_I_FIELD(i)) = WW_HENSYUrange(WW_I_POSIY(i), WW_I_POSIX(i))
                                    Case "System.Integer"
                                        WW_TBLDATArow(WW_I_FIELD(i)) = WW_HENSYUrange(WW_I_POSIY(i), WW_I_POSIX(i)).ToString
                                    Case "System.Long"
                                        WW_TBLDATArow(WW_I_FIELD(i)) = WW_HENSYUrange(WW_I_POSIY(i), WW_I_POSIX(i)).ToString
                                    Case "System.Short"
                                        WW_TBLDATArow(WW_I_FIELD(i)) = WW_HENSYUrange(WW_I_POSIY(i), WW_I_POSIX(i)).ToString
                                    Case "System.Decimal"
                                        WW_TBLDATArow(WW_I_FIELD(i)) = WW_HENSYUrange(WW_I_POSIY(i), WW_I_POSIX(i)).ToString
                                    Case "System.Single"
                                        WW_TBLDATArow(WW_I_FIELD(i)) = WW_HENSYUrange(WW_I_POSIY(i), WW_I_POSIX(i)).ToString
                                    Case "System.Double"
                                        WW_TBLDATArow(WW_I_FIELD(i)) = WW_HENSYUrange(WW_I_POSIY(i), WW_I_POSIX(i)).ToString
                                    Case "System.Date"
                                        WW_TBLDATArow(WW_I_FIELD(i)) = WW_HENSYUrange(WW_I_POSIY(i), WW_I_POSIX(i)).ToString("yyyy/MM/dd")
                                    Case "Nothing"
                                    Case Else
                                        WW_TBLDATArow(WW_I_FIELD(i)) = WW_HENSYUrange(WW_I_POSIY(i), WW_I_POSIX(i)).ToString
                                End Select
                            End If
                        Next

                        '明細アイテム(I_Data)
                        For i As Integer = 0 To WW_R_TITOLKBN.Count - 1
                            If WW_R_TITOLKBN(i) = "I_Data" Then

                                If WW_HENSYUrange(WW_R_POSIY(i), CS0021PROFXLS.POSI_I_X_MAX + CS0021PROFXLS.POSI_R_X_MAX * CNT + WW_R_POSIX(i)) = Nothing Then
                                    'WW_TBLDATArow(WW_R_FIELD(i)) = WW_HENSYUrange(WW_R_POSIY(i), CS0021UPROFXLS.POSI_I_X_MAX + CS0021UPROFXLS.POSI_R_X_MAX * CNT + WW_R_POSIX(i))
                                Else
                                    WW_RecWrite = 1

                                    Select Case WW_HENSYUrange(WW_R_POSIY(i), CS0021PROFXLS.POSI_I_X_MAX + CS0021PROFXLS.POSI_R_X_MAX * CNT + WW_R_POSIX(i)).GetType.ToString
                                        Case "System.String"
                                            WW_TBLDATArow(WW_R_FIELD(i)) = WW_HENSYUrange(WW_R_POSIY(i), CS0021PROFXLS.POSI_I_X_MAX + CS0021PROFXLS.POSI_R_X_MAX * CNT + WW_R_POSIX(i))
                                        Case "System.Integer"
                                            WW_TBLDATArow(WW_R_FIELD(i)) = WW_HENSYUrange(WW_R_POSIY(i), CS0021PROFXLS.POSI_I_X_MAX + CS0021PROFXLS.POSI_R_X_MAX * CNT + WW_R_POSIX(i)).ToString
                                        Case "System.Long"
                                            WW_TBLDATArow(WW_R_FIELD(i)) = WW_HENSYUrange(WW_R_POSIY(i), CS0021PROFXLS.POSI_I_X_MAX + CS0021PROFXLS.POSI_R_X_MAX * CNT + WW_R_POSIX(i)).ToString
                                        Case "System.Short"
                                            WW_TBLDATArow(WW_R_FIELD(i)) = WW_HENSYUrange(WW_R_POSIY(i), CS0021PROFXLS.POSI_I_X_MAX + CS0021PROFXLS.POSI_R_X_MAX * CNT + WW_R_POSIX(i)).ToString
                                        Case "System.Decimal"
                                            WW_TBLDATArow(WW_R_FIELD(i)) = WW_HENSYUrange(WW_R_POSIY(i), CS0021PROFXLS.POSI_I_X_MAX + CS0021PROFXLS.POSI_R_X_MAX * CNT + WW_R_POSIX(i)).ToString
                                        Case "System.Single"
                                            WW_TBLDATArow(WW_R_FIELD(i)) = WW_HENSYUrange(WW_R_POSIY(i), CS0021PROFXLS.POSI_I_X_MAX + CS0021PROFXLS.POSI_R_X_MAX * CNT + WW_R_POSIX(i)).ToString
                                        Case "System.Double"
                                            WW_TBLDATArow(WW_R_FIELD(i)) = WW_HENSYUrange(WW_R_POSIY(i), CS0021PROFXLS.POSI_I_X_MAX + CS0021PROFXLS.POSI_R_X_MAX * CNT + WW_R_POSIX(i)).ToString
                                        Case "System.Date"
                                            WW_TBLDATArow(WW_R_FIELD(i)) = WW_HENSYUrange(WW_R_POSIY(i), CS0021PROFXLS.POSI_I_X_MAX + CS0021PROFXLS.POSI_R_X_MAX * CNT + WW_R_POSIX(i)).ToString("yyyy/MM/dd")
                                        Case "Nothing"
                                        Case Else
                                            WW_TBLDATArow(WW_R_FIELD(i)) = WW_HENSYUrange(WW_R_POSIY(i), CS0021PROFXLS.POSI_I_X_MAX + CS0021PROFXLS.POSI_R_X_MAX * CNT + WW_R_POSIX(i)).ToString
                                    End Select
                                End If
                            End If
                        Next

                        '明細アイテム(I_DataKey)          
                        If WW_RecWrite = 1 Then
                            For i As Integer = 0 To WW_R_TITOLKBN.Count - 1
                                'If WW_HENSYUrange(WW_R_POSIY(i), CS0021UPROFXLS.POSI_I_X_MAX + CS0021UPROFXLS.POSI_R_X_MAX * CNT + WW_R_POSIX(i)) = Nothing Then
                                'Else
                                If WW_R_TITOLKBN(i) = "I_DataKey" Then
                                    WW_TBLDATArow(WW_R_FIELD(i)) = WW_STRUCT_TBLDATA.Rows(CNT).Item(WW_R_FIELD(i))
                                End If
                                'End If
                            Next
                        End If

                        If WW_RecWrite = 1 Then
                            WW_TBLDATA.Rows.Add(WW_TBLDATArow)
                        End If
                    Next
                Catch ex As Exception
                    '他Excel処理完了待ち
                    ERR = C_MESSAGE_NO.WAIT_OTHER_EXCEL_JOB

                    CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME                            'SUBクラス名
                    CS0011LOGWRITE.INFPOSI = "Excel_Detail_Range"
                    CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
                    CS0011LOGWRITE.TEXT = ex.ToString()
                    CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.WAIT_OTHER_EXCEL_JOB
                    CS0011LOGWRITE.CS0011LOGWrite()                                     'ログ出力

                    'Excel終了＆リリース
                    CloseExcel(W_ExcelApp, W_ExcelBooks, W_ExcelBook, W_ExcelSheets, W_ExcelSheet)

                    'ワークテーブル解放
                    Try
                        WW_TBLDATA.Dispose()
                        WW_TBLDATA = Nothing
                        WW_STRUCT_TBLDATA.Dispose()
                        WW_STRUCT_TBLDATA = Nothing
                    Catch err As Exception
                    End Try
                    Exit Sub
                Finally
                    'Excel.Range 解放
                    ExcelMemoryRelease(WW_HENSYUrange)
                    ExcelMemoryRelease(WW_Cells)
                    ExcelMemoryRelease(WW_STARTpoint)
                    ExcelMemoryRelease(WW_ENDpoint)
                    ExcelMemoryRelease(WW_EXCELrange)
                End Try

            Loop Until WW_LoopEND = 0
        End If


        '■Excel(タイトル)データ取得
        Try
            '○タイトルデータ切り出し領域(Excel内タイトル部データ→WW_HENSYUrange)
            ReDim WW_HENSYUrange(CS0021PROFXLS.POSI_T_Y_MAX - 1, CS0021PROFXLS.POSI_T_X_MAX - 1)      '行編集領域
            WW_Cells = W_ExcelSheet.Cells

            '　ｎ件目の明細データ開始位置＝明細タイトル開始位置+明細MAX行　…　明細タイトルを考慮する事
            WW_STARTpoint = DirectCast(WW_Cells.Item(1, 1), Excel.Range)    'A1
            '　ｎ件目の明細データ終了位置＝明細タイトル開始位置+明細MAX行*(ｎ+1)-1　…　明細タイトルを考慮する事
            WW_ENDpoint = DirectCast(WW_Cells.Item(CS0021PROFXLS.POSI_T_Y_MAX, CS0021PROFXLS.POSI_T_X_MAX), Excel.Range)
            WW_EXCELrange = W_ExcelSheet.Range(WW_STARTpoint, WW_ENDpoint)           'Excelデータの入力セル範囲

            WW_HENSYUrange = WW_EXCELrange.Value

            '○タイトルデータ取得
            For i As Integer = 0 To CS0021PROFXLS.TITLEKBN.Count - 1
                If CS0021PROFXLS.TITLEKBN(i) = "T" And CS0021PROFXLS.EFFECT(i) = "Y" And CS0021PROFXLS.POSIY(i) > 0 And CS0021PROFXLS.POSIX(i) > 0 Then
                    If WW_HENSYUrange(CS0021PROFXLS.POSIY(i), CS0021PROFXLS.POSIX(i)) = Nothing Then
                    Else
                        Select Case WW_HENSYUrange(CS0021PROFXLS.POSIY(i), CS0021PROFXLS.POSIX(i)).GetType.ToString
                            Case "System.String"
                                For j As Integer = 0 To WW_TBLDATA.Rows.Count - 1
                                    WW_TBLDATA.Rows(j)(CS0021PROFXLS.FIELD(i)) = WW_HENSYUrange(CS0021PROFXLS.POSIY(i), CS0021PROFXLS.POSIX(i))
                                Next
                            Case "System.Integer"
                                For j As Integer = 0 To WW_TBLDATA.Rows.Count - 1
                                    WW_TBLDATA.Rows(j)(CS0021PROFXLS.FIELD(i)) = WW_HENSYUrange(CS0021PROFXLS.POSIY(i), CS0021PROFXLS.POSIX(i)).ToString
                                Next
                            Case "System.Long"
                                For j As Integer = 0 To WW_TBLDATA.Rows.Count - 1
                                    WW_TBLDATA.Rows(j)(CS0021PROFXLS.FIELD(i)) = WW_HENSYUrange(CS0021PROFXLS.POSIY(i), CS0021PROFXLS.POSIX(i)).ToString
                                Next
                            Case "System.Short"
                                For j As Integer = 0 To WW_TBLDATA.Rows.Count - 1
                                    WW_TBLDATA.Rows(j)(CS0021PROFXLS.FIELD(i)) = WW_HENSYUrange(CS0021PROFXLS.POSIY(i), CS0021PROFXLS.POSIX(i)).ToString
                                Next
                            Case "System.Decimal"
                                For j As Integer = 0 To WW_TBLDATA.Rows.Count - 1
                                    WW_TBLDATA.Rows(j)(CS0021PROFXLS.FIELD(i)) = WW_HENSYUrange(CS0021PROFXLS.POSIY(i), CS0021PROFXLS.POSIX(i)).ToString
                                Next
                            Case "System.Single"
                                For j As Integer = 0 To WW_TBLDATA.Rows.Count - 1
                                    WW_TBLDATA.Rows(j)(CS0021PROFXLS.FIELD(i)) = WW_HENSYUrange(CS0021PROFXLS.POSIY(i), CS0021PROFXLS.POSIX(i)).ToString
                                Next
                            Case "System.Double"
                                For j As Integer = 0 To WW_TBLDATA.Rows.Count - 1
                                    WW_TBLDATA.Rows(j)(CS0021PROFXLS.FIELD(i)) = WW_HENSYUrange(CS0021PROFXLS.POSIY(i), CS0021PROFXLS.POSIX(i)).ToString
                                Next
                            Case "System.Date"
                                For j As Integer = 0 To WW_TBLDATA.Rows.Count - 1
                                    WW_TBLDATA.Rows(j)(CS0021PROFXLS.FIELD(i)) = WW_HENSYUrange(CS0021PROFXLS.POSIY(i), CS0021PROFXLS.POSIX(i)).ToString("yyyy/MM/dd")
                                Next
                            Case "Nothing"
                            Case Else
                                For j As Integer = 0 To WW_TBLDATA.Rows.Count - 1
                                    WW_TBLDATA.Rows(j)(CS0021PROFXLS.FIELD(i)) = WW_HENSYUrange(CS0021PROFXLS.POSIY(i), CS0021PROFXLS.POSIX(i)).ToString
                                Next
                        End Select
                    End If
                End If
            Next
        Catch ex As Exception
            '他Excel処理完了待ち
            ERR = C_MESSAGE_NO.WAIT_OTHER_EXCEL_JOB

            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME                            'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "Excel_TITOL_Range"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.WAIT_OTHER_EXCEL_JOB
            CS0011LOGWRITE.CS0011LOGWrite()                                     'ログ出力

            'Excel終了＆リリース
            CloseExcel(W_ExcelApp, W_ExcelBooks, W_ExcelBook, W_ExcelSheets, W_ExcelSheet)

            'ワークテーブル解放
            Try
                WW_TBLDATA.Dispose()
                WW_TBLDATA = Nothing
                WW_STRUCT_TBLDATA.Dispose()
                WW_STRUCT_TBLDATA = Nothing
            Catch err As Exception
            End Try
            Exit Sub
        Finally
            'Excel.Range 解放
            ExcelMemoryRelease(WW_HENSYUrange)
            ExcelMemoryRelease(WW_Cells)
            ExcelMemoryRelease(WW_STARTpoint)
            ExcelMemoryRelease(WW_ENDpoint)
            ExcelMemoryRelease(WW_EXCELrange)
        End Try

        For i As Integer = 0 To WW_TBLDATA.Rows.Count - 1
            For j As Integer = 0 To WW_TBLDATA.Columns.Count - 1
                If IsDBNull(WW_TBLDATA.Rows(i).Item(j)) Then
                    WW_TBLDATA.Rows(i).Item(j) = Nothing
                End If
            Next
        Next

        '～～～～～ データ設定 (終了) ～～～～～～～～～～～～～～～～

        '○1秒間表示して終了処理へ
        'System.Threading.Thread.Sleep(1000)

        '○保存時の問合せのダイアログを非表示に設定
        W_ExcelApp.DisplayAlerts = False

        '○Excel終了＆リリース
        CloseExcel(W_ExcelApp, W_ExcelBooks, W_ExcelBook, W_ExcelSheets, W_ExcelSheet)

        TBLDATA = WW_TBLDATA
        ERR = C_MESSAGE_NO.NORMAL

        'ワークテーブル解放
        WW_TBLDATA.Dispose()
        WW_TBLDATA = Nothing

        WW_STRUCT_TBLDATA.Dispose()
        WW_STRUCT_TBLDATA = Nothing

    End Sub

    ''' <summary>
    ''' Excel操作のメモリ開放
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="objCom"></param>
    ''' <remarks></remarks>
    Public Sub ExcelMemoryRelease(Of T As Class)(ByRef objCom As T)

        'ランタイム実行対象がComObjectのアンマネージコードの場合、メモリ開放
        If objCom Is Nothing Then
            Return
        Else
            Try
                If Marshal.IsComObject(objCom) Then
                    Dim count As Integer = Marshal.FinalReleaseComObject(objCom)
                End If
            Finally
                objCom = Nothing
            End Try
        End If

    End Sub

    ''' <summary>
    ''' Excel終了＆リリース
    ''' </summary>
    ''' <param name="W_ExcelApp"></param>
    ''' <param name="W_ExcelBooks"></param>
    ''' <param name="W_ExcelBook"></param>
    ''' <param name="W_ExcelSheets"></param>
    ''' <param name="W_ExcelSheet"></param>
    ''' <remarks></remarks>
    Public Sub CloseExcel(W_ExcelApp As Excel.Application, W_ExcelBooks As Excel.Workbooks, W_ExcelBook As Excel.Workbook, W_ExcelSheets As Excel.Sheets, W_ExcelSheet As Excel.Worksheet)

        'Excel終了＆リリース
        If Not W_ExcelBook Is Nothing Then
            W_ExcelApp.DisplayAlerts = False
            W_ExcelBook.Close(False)            '保存する必要は無い
            W_ExcelApp.DisplayAlerts = True
        End If

        ExcelMemoryRelease(W_ExcelSheet)        'ExcelSheet の解放
        ExcelMemoryRelease(W_ExcelSheets)       'ExcelSheets の解放
        ExcelMemoryRelease(W_ExcelBook)         'ExcelBook の解放
        ExcelMemoryRelease(W_ExcelBooks)        'ExcelBooks の解放

        Try
            W_ExcelApp.Visible = True
        Catch err As Exception
        End Try

        Try
            W_ExcelApp.Quit()
        Catch err As Exception
        End Try

        ExcelMemoryRelease(W_ExcelApp)          'ExcelApp を解放

    End Sub

End Structure
