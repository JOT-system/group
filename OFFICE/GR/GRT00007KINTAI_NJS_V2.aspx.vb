﻿Imports System.Data.SqlClient
Imports OFFICE.GRIS0005LeftBox
Imports BASEDLL

''' <summary>
''' 営業勤怠登録
''' </summary>
''' <remarks></remarks>
Public Class GRT00007KINTAI_NJS_V2
    Inherits Page

    '共通宣言
    Private CS0010CHARstr As New CS0010CHARget              '例外文字排除 String Get
    Private CS0011LOGWRITE As New CS0011LOGWrite            'LogOutput DirString Get
    Private CS0013ProfView As New CS0013ProfView            'Tableオブジェクト展開
    Private CS0026TblSort As New CS0026TBLSORT              '表示画面情報ソート
    Private CS0036FCHECK As New CS0036FCHECK                '項目チェック
    Private CS0044L1INSERT As New CS0044L1INSERT            '統計DB出力
    Private CS0050SESSION As New CS0050SESSION              'セッション情報操作処理
    Private GS0007FIXVALUElst As New GS0007FIXVALUElst      'Leftボックス用固定値リスト取得

    Private T0005COM As New GRT0005COM                              '勤怠共通
    Private T0007COM As New GRT0007COM_V2                           '勤怠共通
    Private T0009TIME As New GRT00009TIMEFORMAT_V2                  '時間調整共通

    'CSV検索結果格納ds
    Private T0007tbl As DataTable                                  'Grid格納用テーブル
    Private T0007row As DataRow                                    '行のロウデータ
    Private T0005tbl As DataTable                                  'Grid格納用テーブル
    Private T0005WEEKtbl As DataTable                              '一週間前データ
    Private T0005row As DataRow                                    '行のロウデータ
    Private T0007_TORIHIKISAKIrow As DataRow
    Private T0007INPtbl As DataTable                               '勤怠テーブル（GridView用）
    Private T0007INProw As DataRow                                 '行のロウデータ
    Private T0007TTLrow As DataRow                                 '行のロウデータ
    Private S0013tbl As DataTable                                  'データフィールド

    Const CONST_SCROOL As Integer = 20
    Const CONST_YAZAKI As String = "1"
    Const CONST_JX As String = "2"
    Const CONST_ENEX As String = "3"
    Const CONST_HAND As String = "4"

    '共通処理結果
    Private WW_ERRCODE As String = String.Empty                     'リターンコード
    Private WW_RTN_SW As String                                     '
    Private WW_DUMMY As String                                      '

    Dim WW_ERRLIST As List(Of String)                               'インポート中の１セット分のエラー

    Private WW_ListBoxMODELCODE As ListBox = New ListBox
    Private WW_ListBoxMODELDISTANCE As ListBox = New ListBox

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

        '■■■ 作業用データベース設定 ■■■
        T0007tbl = New DataTable
        T0005tbl = New DataTable
        T0005WEEKtbl = New DataTable
        T0007INPtbl = New DataTable
        S0013tbl = New DataTable

        Try

            If IsPostBack Then

                '○ 各ボタン押下処理
                If Not String.IsNullOrEmpty(WF_ButtonClick.Value) Then

                    '○ 画面表示データ復元
                    'T0007COM.T0007tbl_ColumnsAdd(T0007INPtbl)
                    If Not Master.RecoverTable(T0007INPtbl, work.WF_T7KIN_XMLsaveF.Text) Then
                        Exit Sub
                    End If

                    Select Case WF_ButtonClick.Value
                        Case "WF_ButtonBREAKTIME"           '休憩不足分ボタン押下
                            WF_ButtonBREAKTIME_Click()
                        Case "WF_ButtonNIPPOEDIT"           '日報修正ボタン押下
                            WF_ButtonNIPPOEDIT_Click()
                        Case "WF_ButtonNIPPO"               '日報取込ボタン押下
                            WF_ButtonNIPPO_Click()
                        Case "WF_ButtonDOWN"                '前頁ボタン処理
                            WF_ButtonDOWN_Click()
                        Case "WF_ButtonUP"                  '次頁ボタン処理
                            WF_ButtonUP_Click()
                        Case "WF_ButtonRESET"               'モデル再取得ボタン押下
                            WF_buttonRESET_click()
                        Case "WF_MODELreset"                'モデルチェックボタンＯＦＦ
                            WF_MODELreset_click()
                        Case "WF_ButtonUPDATE"              '更新ボタン処理
                            WF_ButtonUPDATE_Click()
                        Case "WF_ButtonUPDATEMDL"           '更新ボタン処理
                            WF_ButtonUPDATE_Click("MDL")
                        Case "WF_ButtonEND"                 '終了ボタン押下
                            WF_ButtonEND_Click()
                        Case "WF_ButtonENDMDL"              '終了ボタン押下
                            WF_ButtonEND_Click("MDL")
                        Case "WF_ButtonSel"                 '(左ボックス)選択ボタン押下
                            WF_ButtonSel_Click()
                        Case "WF_ButtonCan"                 '(左ボックス)キャンセルボタン押下
                            WF_ButtonCan_Click()
                        Case "WF_ListboxDBclick"            '左ボックスダブルクリック
                            WF_ButtonSel_Click()
                        Case "WF_RadioButonClick"           '(右ボックス)ラジオボタン選択
                            WF_RadioButton_Click()
                        Case "WF_MEMOChange"                '(右ボックス)メモ欄更新
                            WF_RIGHTBOX_Change()
                        Case "WF_Field_DBClick"             'フィールドダブルクリック
                            WF_FIELD_DBClick()
                        Case "WF_LeftBoxSelectClick"        'フィールドチェンジ
                            WF_FIELD_Change()
                        Case "WF_DTABChange"                'DetailTab切替処理
                            WF_Detail_TABChange()
                        Case "WF_EXCEL_UPLOAD"
                            Master.Output(C_MESSAGE_NO.FILE_UPLOAD_ERROR, C_MESSAGE_TYPE.ERR)

                    End Select

                    'スクロール処理
                    Scrole_SUB()

                End If
            Else
                '○ 初期化処理
                Initialize()

            End If

        Finally
            '○ 格納Table Close
            If Not IsNothing(T0007tbl) Then
                T0007tbl.Clear()
                T0007tbl.Dispose()
                T0007tbl = Nothing
            End If

            If Not IsNothing(T0007INPtbl) Then
                T0007INPtbl.Clear()
                T0007INPtbl.Dispose()
                T0007INPtbl = Nothing
            End If

            If Not IsNothing(T0005tbl) Then
                T0005tbl.Clear()
                T0005tbl.Dispose()
                T0005tbl = Nothing
            End If

            If Not IsNothing(T0005WEEKtbl) Then
                T0005WEEKtbl.Clear()
                T0005WEEKtbl.Dispose()
                T0005WEEKtbl = Nothing
            End If

            If Not IsNothing(S0013tbl) Then
                S0013tbl.Clear()
                S0013tbl.Dispose()
                S0013tbl = Nothing
            End If

        End Try
    End Sub

    ''' <summary>
    ''' 初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub Initialize()

        '○ 画面ID設定
        Master.MAPID = GRT00007WRKINC_V2.MAPIDNJS

        WF_WORKDATE.Focus()
        WF_FIELD.Value = ""
        WF_LeftboxOpen.Value = ""
        WF_LeftMViewChange.Value = ""
        WF_RightboxOpen.Value = ""
        leftview.ActiveListBox()
        rightview.ResetIndex()

        '○ 画面の値設定
        WW_MAPValueSet()

        '○ 右ボックスへの値設定
        rightview.MAPID = GRT00007WRKINC_V2.MAPIDNJS
        rightview.MAPVARI = Master.MAPvariant
        rightview.COMPCODE = work.WF_T7SEL_CAMPCODE.Text
        rightview.PROFID = Master.PROF_REPORT
        rightview.Initialize(WW_DUMMY)

        '○ GridView初期設定
        GridViewInitialize()

    End Sub

    ''' <summary>
    ''' 画面初期値設定処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_MAPValueSet()

        'Grid情報保存先のファイル名
        Master.CreateXMLSaveFile()

        Dim WW_ERR_CODE As String = ""
        Dim WW_MSG As String = ""
        Dim WW_ERR_REPORT As String = ""
        '○ 検索画面からの遷移
        MAPrefelence(WW_MSG, WW_ERRCODE)
        WW_ERR_CODE = WW_ERRCODE

        '更新ボタン非活性（エラー）の場合、メッセージ出力（但し、すでにあるエラーメッセージを優先する）
        If WW_ERR_CODE <> C_MESSAGE_NO.NORMAL And rightview.GetErrorReport() = "" Then
            Master.Output(WW_ERR_CODE, C_MESSAGE_TYPE.ERR)
        End If
        If WW_MSG <> "" Then
            WW_ERR_REPORT = "内部処理エラー" & ControlChars.NewLine & WW_MSG
            rightview.AddErrorReport(WW_ERR_REPORT)
        End If

        '○ ヘルプボタン非表示
        Master.dispHelp = False

        '○ ファイルドロップ有無
        Master.eventDrop = True

        '○ 画面モード(更新・参照)設定
        If Master.MAPpermitcode = C_PERMISSION.UPDATE Then
            WF_MAPpermitcode.Value = "TRUE"
        Else
            WF_MAPpermitcode.Value = "FALSE"
        End If

    End Sub

    ''' <summary>
    ''' GridViewデータ設定
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub GridViewInitialize()

        '一覧から遷移した場合
        GRID_INITset()

        '○ 一覧表示データ編集(性能対策)
        Using TBLview As DataView = New DataView(T0007INPtbl)

            TBLview.Sort = "LINECNT"
            TBLview.RowFilter = "HIDDEN = 0 and LINECNT >= 1  "

            CS0013ProfView.CAMPCODE = work.WF_T7SEL_CAMPCODE.Text
            CS0013ProfView.PROFID = Master.PROF_VIEW
            CS0013ProfView.MAPID = Master.MAPID
            CS0013ProfView.VARI = Master.VIEWID
            CS0013ProfView.SRCDATA = TBLview.ToTable
            CS0013ProfView.TBLOBJ = pnlListArea
            CS0013ProfView.SCROLLTYPE = CS0013ProfView.SCROLLTYPE_ENUM.Vertical
            'CS0013ProfView.LEVENT = "ondblclick"
            'CS0013ProfView.LFUNC = "ListDbClick"
            CS0013ProfView.TITLEOPT = False
            CS0013ProfView.HIDEOPERATIONOPT = True
            CS0013ProfView.TARGETDATE = work.WF_T7SEL_TAISHOYM.Text & "/01"
            CS0013ProfView.CS0013ProfView()
            If Not isNormal(CS0013ProfView.ERR) Then
                Master.Output(CS0013ProfView.ERR, C_MESSAGE_TYPE.ABORT, "一覧設定エラー")
                Exit Sub
            End If

            '○ 先頭行に合わせる
            WF_GridPosition.Text = "1"

        End Using

    End Sub

    ''' <summary>
    ''' Detail タブ切替処理
    ''' </summary>
    Protected Sub WF_Detail_TABChange()

        Dim WW_DTABChange As Integer

        WF_ButtonNIPPOEDIT.Style.Remove("display")
        WF_ButtonNIPPO.Style.Remove("display")
        WF_ButtonDOWN.Style.Remove("display")
        WF_ButtonUP.Style.Remove("display")
        WF_ButtonUPDATE.Style.Remove("display")
        WF_ButtonEND.Style.Remove("display")

        Try
            Integer.TryParse(WF_DTABChange.Value, WW_DTABChange)
        Catch ex As Exception
            WW_DTABChange = 0
            WF_NIPPObtn.Value = "TRUE"
        End Try

        '月調整を選択した場合、指定日入力タブには切替られない！  
        If work.WF_T7KIN_RECODEKBN.Text = C_PERMISSION.UPDATE Then
            WW_DTABChange = 1
            '月合計の場合、ボタンを非表示
            WF_NIPPObtn.Value = "FALSE"
        Else
            WF_NIPPObtn.Value = "TRUE"
        End If

        If WW_DTABChange = 2 Then
            WF_ButtonBREAKTIME.Style.Add("display", "none")
            WF_ButtonNIPPOEDIT.Style.Add("display", "none")
            WF_ButtonNIPPO.Style.Add("display", "none")
            WF_ButtonDOWN.Style.Add("display", "none")
            WF_ButtonUP.Style.Add("display", "none")
            WF_ButtonUPDATE.Style.Add("display", "none")
            WF_ButtonEND.Style.Add("display", "none")
        End If

        WF_DetailMView.ActiveViewIndex = WW_DTABChange

    End Sub

    ''' <summary>
    ''' フィールドチェンジ時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_FIELD_Change()

        Dim WW_RESULT As String = ""
        rightview.SetErrorReport("")

        'テーブルデータ復元
        'T0007COM.T0007tbl_ColumnsAdd(T0007INPtbl)
        'If Not Master.RecoverTable(T0007INPtbl, work.WF_T7KIN_XMLsaveF.Text) Then
        '    Exit Sub
        'End If

        '前画面（T00007I）テーブルデータ復元
        'T0007COM.T0007tbl_ColumnsAdd(T0007tbl)
        If Not Master.RecoverTable(T0007tbl, work.WF_T7I_XMLsaveF.Text) Then
            Exit Sub
        End If

        '----------------------------------------------
        '画面項目チェック
        '----------------------------------------------
        '入力禁止文字除外
        InpCHARstr()

        '項目チェック
        T0007INProw_CHEK(WW_RESULT)
        If WW_RESULT <> C_MESSAGE_NO.NORMAL Then
            Master.Output(WW_RESULT, C_MESSAGE_TYPE.ABORT)
            Exit Sub
        Else
            Master.Output(C_MESSAGE_NO.NORMAL, C_MESSAGE_TYPE.NOR)
        End If

        '項目変更チェック
        ItemChangeCheck(WW_RESULT)

        '指定日入力画面に変更があった場合、残業計算を行う
        If WW_RESULT = "変更1" Then
            '----------------------------------------------
            '残業計算（特作の再計算も行う）
            '----------------------------------------------
            T0007COM.T0007_KintaiCalc_NJS(T0007INPtbl, T0007tbl, "TOKUSA")

        ElseIf WW_RESULT = "変更4" Then
            '----------------------------------------------
            '残業計算（特作の再計算を行わない
            '----------------------------------------------
            T0007COM.T0007_KintaiCalc_NJS(T0007INPtbl, T0007tbl)

        End If

        'スクロール処理
        CS0026TblSort.TABLE = T0007INPtbl
        CS0026TblSort.FILTER = ""
        CS0026TblSort.SORTING = "STAFFCODE, WORKDATE, STDATE, STTIME, ENDDATE, ENDTIME, WORKKBN"
        T0007INPtbl = CS0026TblSort.Sort()

        '画面編集
        DisplayEdit(T0007INPtbl)

        '■■■ 前画面（T00007I）用にテーブルデータ保存 ■■■
        If Not Master.SaveTable(T0007INPtbl, work.WF_T7KIN_XMLsaveF.Text) Then
            Exit Sub
        End If

        Select Case WF_FIELD.Value
            Case "WF_HOLIDAYKBN"
                WF_PAYKBN.Focus()
            Case "WF_PAYKBN"
                WF_SHUKCHOKKBN.Focus()
            Case "WF_SHUKCHOKKBN"
                WF_STDATE.Focus()
            Case "WF_STDATE"
                WF_STTIME.Focus()
            Case "WF_STTIME"
                WF_BINDSTDATE.Focus()
            Case "WF_BINDSTDATE"
                WF_BINDTIME.Focus()
            Case "WF_BINDTIME"
                WF_ENDDATE.Focus()
            Case "WF_ENDDATE"
                WF_ENDTIME.Focus()
            Case "WF_ENDTIME"
                WF_PAYKBN.Focus()
            Case "WF_BBSTTIME01"
                WF_BBENDTIME01.Focus()
            Case "WF_BBENDTIME01"
                WF_BBSTTIME02.Focus()
            Case "WF_BBSTTIME02"
                WF_BBENDTIME02.Focus()
            Case "WF_BBENDTIME02"
                WF_BBSTTIME03.Focus()
            Case "WF_BBSTTIME03"
                WF_BBENDTIME03.Focus()
            Case "WF_BBENDTIME03"
                WF_BBSTTIME04.Focus()
            Case "WF_BBSTTIME04"
                WF_BBENDTIME04.Focus()
            Case "WF_BBENDTIME04"
                WF_BBSTTIME05.Focus()
            Case "WF_BBSTTIME05"
                WF_BBENDTIME05.Focus()
            Case "WF_BBENDTIME05"
                WF_BBSTTIME06.Focus()
            Case "WF_BBSTTIME06"
                WF_BBENDTIME06.Focus()
            Case "WF_BBENDTIME06"
                WF_BBSTTIME07.Focus()
            Case "WF_BBSTTIME07"
                WF_BBENDTIME07.Focus()
            Case "WF_BBENDTIME07"
                WF_BBSTTIME08.Focus()
            Case "WF_BBSTTIME08"
                WF_BBENDTIME08.Focus()
            Case "WF_BBENDTIME08"
                WF_BBSTTIME09.Focus()
            Case "WF_BBSTTIME09"
                WF_BBENDTIME09.Focus()
            Case "WF_BBENDTIME09"
                WF_BBSTTIME10.Focus()
            Case "WF_BBSTTIME10"
                WF_BBENDTIME10.Focus()
            Case "WF_BBENDTIME10"
                WF_G1STTIME01.Focus()
            Case "WF_G1STTIME01"
                WF_G1ENDTIME01.Focus()
            Case "WF_G1ENDTIME01"
                WF_G1STTIME02.Focus()
            Case "WF_G1STTIME02"
                WF_G1ENDTIME02.Focus()
            Case "WF_G1ENDTIME02"
                WF_G1STTIME03.Focus()
            Case "WF_G1STTIME03"
                WF_G1ENDTIME03.Focus()
            Case "WF_G1ENDTIME03"
                WF_G1STTIME04.Focus()
            Case "WF_G1STTIME04"
                WF_G1ENDTIME04.Focus()
            Case "WF_G1ENDTIME04"
                WF_G1STTIME05.Focus()
            Case "WF_G1STTIME05"
                WF_G1ENDTIME05.Focus()
            Case "WF_G1ENDTIME05"
                WF_G1STTIME06.Focus()
            Case "WF_G1STTIME06"
                WF_G1ENDTIME06.Focus()
            Case "WF_G1ENDTIME06"
                WF_G1STTIME07.Focus()
            Case "WF_G1STTIME07"
                WF_G1ENDTIME07.Focus()
            Case "WF_G1ENDTIME07"
                WF_G1STTIME08.Focus()
            Case "WF_G1STTIME08"
                WF_G1ENDTIME08.Focus()
            Case "WF_G1ENDTIME08"
                WF_G1STTIME09.Focus()
            Case "WF_G1STTIME09"
                WF_G1ENDTIME09.Focus()
            Case "WF_G1ENDTIME09"
                WF_G1STTIME10.Focus()
            Case "WF_G1STTIME10"
                WF_G1ENDTIME10.Focus()
            Case "WF_G1ENDTIME10"
                WF_BBSTTIME01.Focus()
        End Select

        WF_FIELD.Value = ""

    End Sub

    ' ***  入力項目変更チェック＆更新処理                                        ***
    Protected Sub ItemChangeCheck(ByRef oRtn As String)
        Dim WW_RESULT As String = ""

        oRtn = ""
        Dim WW_UPD_FLG1 As String = "OFF"
        Dim WW_UPD_FLG2 As String = "OFF"
        Dim WW_UPD_ST As String = "OFF"
        Dim WW_UPD_END As String = "OFF"
        Dim WW_UPD_BREAK As String = "OFF"
        Dim WW_UPD_HAISO As String = "OFF"
        Dim WW_UPD_TOKUSA As String = "OFF"
        Dim WW_T0007tbl As DataTable = T0007INPtbl.Clone
        Dim WW_T0007row As DataRow

        '指定日入力の変更を取込む
        For Each T0007INProw As DataRow In T0007INPtbl.Rows
            'HDKBN（H:ﾍｯﾀﾞﾚｺｰﾄﾞ、D:明細ﾚｺｰﾄﾞ）、RECODEKBN（0:指定日ﾚｺｰﾄﾞ、1:月調整ﾚｺｰﾄﾞ、2:合計ﾚｺｰﾄﾞ）
            If T0007INProw("HDKBN") = "H" And T0007INProw("RECODEKBN") = "0" Then

                '空更新を可能とする

                '日報取得
                Dim T0005tbl As DataTable = New DataTable
                T00005ALLget("OLD", T0007INProw("STAFFCODE"), T0007INProw("NIPPOLINKCODE"), T0007INProw("WORKDATE"), T0007INProw("WORKDATE"), T0005tbl, WW_ERRCODE)
                If WW_ERRCODE <> C_MESSAGE_NO.NORMAL Then
                    Exit Sub
                End If

                'If T0005tbl.Rows.Count > 0 Then
                If WF_STDATE.Text <> T0007INProw("STDATE") Or
                   WF_STTIME.Text <> T0007INProw("STTIME") Then
                    WW_UPD_ST = "ON"
                End If

                If WF_ENDDATE.Text <> T0007INProw("ENDDATE") Or
                   WF_ENDTIME.Text <> T0007INProw("ENDTIME") Then
                    WW_UPD_END = "ON"
                End If

                If WF_NIPPOBREAKTIME.Text <> T0007INProw("NIPPOBREAKTIME") Then
                    WW_UPD_BREAK = "ON"
                End If
                'End If
                T0007INProw("OPERATION") = "更新"
                T0007INProw("CAMPCODE") = WF_CAMPCODE.Text
                T0007INProw("STATUS") = ""
                T0007INProw("TIMSTP") = 0
                T0007INProw("HOLIDAYKBN") = WF_HOLIDAYKBN.Text
                T0007INProw("PAYKBN") = WF_PAYKBN.Text
                T0007INProw("SHUKCHOKKBN") = WF_SHUKCHOKKBN.Text
                T0007INProw("STDATE") = WF_STDATE.Text
                T0007INProw("STTIME") = WF_STTIME.Text
                T0007INProw("ENDDATE") = WF_ENDDATE.Text
                T0007INProw("ENDTIME") = WF_ENDTIME.Text
                T0007INProw("BINDSTDATE") = WF_BINDSTDATE.Text
                T0007INProw("BINDTIME") = WF_BINDTIME.Text
                T0007INProw("BREAKTIME") = WF_BREAKTIME.Text
                If T0007INProw("TOKUSA1TIME") <> WF_TOKUSA1TIME.Text Then
                    WW_UPD_TOKUSA = "ON"
                End If
                T0007INProw("TOKUSA1TIME") = WF_TOKUSA1TIME.Text
                T0007INProw("TOKUSA1TIMETTL") = WF_TOKUSA1TIME.Text
                T0007INProw("CAMPNAMES") = ""
                CODENAME_get("CAMPCODE", T0007INProw("CAMPCODE"), T0007INProw("CAMPNAMES"), WW_DUMMY)
                T0007INProw("STAFFKBNNAMES") = ""
                CODENAME_get("STAFFKBN", T0007INProw("STAFFKBN"), T0007INProw("STAFFKBNNAMES"), WW_DUMMY)
                T0007INProw("MORGNAMES") = ""
                CODENAME_get("ORG", T0007INProw("MORG"), T0007INProw("MORGNAMES"), WW_DUMMY)
                T0007INProw("HORGNAMES") = ""
                CODENAME_get("HORG", T0007INProw("HORG"), T0007INProw("HORGNAMES"), WW_DUMMY)
                T0007INProw("HOLIDAYKBNNAMES") = ""
                CODENAME_get("HOLIDAYKBN", T0007INProw("HOLIDAYKBN"), T0007INProw("HOLIDAYKBNNAMES"), WW_DUMMY)
                T0007INProw("PAYKBNNAMES") = ""
                CODENAME_get("PAYKBN", T0007INProw("PAYKBN"), T0007INProw("PAYKBNNAMES"), WW_DUMMY)
                T0007INProw("SHUKCHOKKBNNAMES") = ""
                CODENAME_get("SHUKCHOKKBN", T0007INProw("SHUKCHOKKBN"), T0007INProw("SHUKCHOKKBNNAMES"), WW_DUMMY)
                If WF_SHACHUHAKKBN.Checked = True Then
                    T0007INProw("SHACHUHAKKBN") = "1"
                    T0007INProw("SHACHUHAKKBNNAMES") = "✔"
                Else
                    T0007INProw("SHACHUHAKKBN") = "0"
                    T0007INProw("SHACHUHAKKBNNAMES") = ""
                End If

                '2020/11/17 ADD
                T0007INProw("T13BBTTLTIME") = T0007COM.HHMMtoMinutes(WF_BBTTLTIME.Text)
                T0007INProw("NIPPOBREAKTIME") = WF_BBTTLTIME.Text
                T0007INProw("BREAKTIME") = "00:00"
                T0007INProw("BREAKTIMETTL") = WF_BBTTLTIME.Text
                T0007INProw("T13G1TTLTIME") = T0007COM.HHMMtoMinutes(WF_G1TTLTIME.Text)
                T0007INProw("HAISOTIME") = WF_G1TTLTIME.Text

                '配送時間の先頭（WF_G1STTIME01）が変更された場合、開始時刻を設定しなおす
                If CType(WF_DView1.FindControl("WF_G1STTIME01"), System.Web.UI.WebControls.TextBox).Text <> "" AndAlso
                   CType(WF_DView1.FindControl("WF_G1STTIME01"), System.Web.UI.WebControls.TextBox).Text <> T0007INProw("T13G1STTIME01") Then
                    WW_UPD_HAISO = "ON"
                End If
                For i As Integer = 1 To 10
                    Dim WF_BBSTTIME As String = "WF_BBSTTIME" & i.ToString("00")
                    Dim WF_BBENDTIME As String = "WF_BBENDTIME" & i.ToString("00")
                    Dim WF_G1STTIME As String = "WF_G1STTIME" & i.ToString("00")
                    Dim WF_G1ENDTIME As String = "WF_G1ENDTIME" & i.ToString("00")

                    Dim WW_BBSTTIME As String = "T13BBSTTIME" & i.ToString("00")
                    Dim WW_BBENDTIME As String = "T13BBENDTIME" & i.ToString("00")
                    Dim WW_G1STTIME As String = "T13G1STTIME" & i.ToString("00")
                    Dim WW_G1ENDTIME As String = "T13G1ENDTIME" & i.ToString("00")

                    If CType(WF_DView1.FindControl(WF_BBSTTIME), System.Web.UI.WebControls.TextBox).Text = "" Then
                        T0007INProw(WW_BBSTTIME) = "00:00"
                    Else
                        T0007INProw(WW_BBSTTIME) = CType(WF_DView1.FindControl(WF_BBSTTIME), System.Web.UI.WebControls.TextBox).Text
                    End If
                    If CType(WF_DView1.FindControl(WF_BBENDTIME), System.Web.UI.WebControls.TextBox).Text = "" Then
                        T0007INProw(WW_BBENDTIME) = "00:00"
                    Else
                        T0007INProw(WW_BBENDTIME) = CType(WF_DView1.FindControl(WF_BBENDTIME), System.Web.UI.WebControls.TextBox).Text
                    End If
                    If CType(WF_DView1.FindControl(WF_G1STTIME), System.Web.UI.WebControls.TextBox).Text = "" Then
                        T0007INProw(WW_G1STTIME) = "00:00"
                    Else
                        T0007INProw(WW_G1STTIME) = CType(WF_DView1.FindControl(WF_G1STTIME), System.Web.UI.WebControls.TextBox).Text
                    End If
                    If CType(WF_DView1.FindControl(WF_G1ENDTIME), System.Web.UI.WebControls.TextBox).Text = "" Then
                        T0007INProw(WW_G1ENDTIME) = "00:00"
                    Else
                        T0007INProw(WW_G1ENDTIME) = CType(WF_DView1.FindControl(WF_G1ENDTIME), System.Web.UI.WebControls.TextBox).Text
                    End If
                Next
                '2020/11/17 ADD END

                Dim WW_CNT As Integer = 0
                For i As Integer = 1 To 6
                    Dim WF_SHARYOKBN As String = "WF_SHARYOKBN" & i.ToString
                    Dim WF_OILPAYKBN As String = "WF_OILPAYKBN" & i.ToString
                    Dim WF_SHUKABASHO As String = "WF_SHUKABASHO" & i.ToString
                    Dim WF_TODOKECODE As String = "WF_TODOKECODE" & i.ToString
                    Dim WF_MODELDISTANCE As String = "WF_MODELDISTANCE" & i.ToString
                    Dim WF_MODIFYKBN As String = "WF_MODIFYKBN" & i.ToString

                    Dim WW_SHARYOKBN As String = "T10SHARYOKBN" & i.ToString
                    Dim WW_OILPAYKBN As String = "T10OILPAYKBN" & i.ToString
                    Dim WW_SHUKABASHO As String = "T10SHUKABASHO" & i.ToString
                    Dim WW_TODOKECODE As String = "T10TODOKECODE" & i.ToString
                    Dim WW_MODELDISTANCE As String = "T10MODELDISTANCE" & i.ToString
                    Dim WW_MODIFYKBN As String = "T10MODIFYKBN" & i.ToString

                    If CType(WF_DView3.FindControl(WF_SHARYOKBN), System.Web.UI.WebControls.TextBox).Text <> "" Then
                        WW_CNT += 1
                    End If

                    If CType(WF_DView3.FindControl(WF_SHARYOKBN), System.Web.UI.WebControls.TextBox).Text <> T0007INProw(WW_SHARYOKBN) Or
                        CType(WF_DView3.FindControl(WF_OILPAYKBN), System.Web.UI.WebControls.TextBox).Text <> T0007INProw(WW_OILPAYKBN) Or
                        CType(WF_DView3.FindControl(WF_SHUKABASHO), System.Web.UI.WebControls.TextBox).Text <> T0007INProw(WW_SHUKABASHO) Or
                        CType(WF_DView3.FindControl(WF_TODOKECODE), System.Web.UI.WebControls.TextBox).Text <> T0007INProw(WW_TODOKECODE) Or
                        Val(CType(WF_DView3.FindControl(WF_MODELDISTANCE), System.Web.UI.WebControls.TextBox).Text) <> Val(T0007INProw(WW_MODELDISTANCE)) Then
                        CType(WF_DView3.FindControl(WF_MODIFYKBN), System.Web.UI.WebControls.CheckBox).Checked = True
                        T0007INProw(WW_MODIFYKBN) = "1"
                    End If

                    T0007INProw(WW_SHARYOKBN) = CType(WF_DView3.FindControl(WF_SHARYOKBN), System.Web.UI.WebControls.TextBox).Text
                    T0007INProw(WW_OILPAYKBN) = CType(WF_DView3.FindControl(WF_OILPAYKBN), System.Web.UI.WebControls.TextBox).Text
                    T0007INProw(WW_SHUKABASHO) = CType(WF_DView3.FindControl(WF_SHUKABASHO), System.Web.UI.WebControls.TextBox).Text
                    T0007INProw(WW_TODOKECODE) = CType(WF_DView3.FindControl(WF_TODOKECODE), System.Web.UI.WebControls.TextBox).Text
                    T0007INProw(WW_MODELDISTANCE) = CType(WF_DView3.FindControl(WF_MODELDISTANCE), System.Web.UI.WebControls.TextBox).Text
                    If CType(WF_DView3.FindControl(WF_MODIFYKBN), System.Web.UI.WebControls.CheckBox).Checked = True Then
                        T0007INProw(WW_MODIFYKBN) = "1"
                    Else
                        T0007INProw(WW_MODIFYKBN) = "0"
                    End If
                Next
                T0007INProw("T10SAVECNT") = WW_CNT

                WW_UPD_FLG1 = "ON"

                If T0007INProw("HDKBN") = "H" Then
                    T0007INProw("HIDDEN") = "0" '表示
                    T0007INProw("DELFLG") = "0" '有効
                End If

                Exit For
            End If
        Next


        If WW_UPD_ST = "ON" Or WW_UPD_END = "ON" Or WW_UPD_BREAK = "ON" Or WW_UPD_HAISO = "ON" Then

            Dim WW_HEADtbl As DataTable = T0007INPtbl.Clone
            CS0026TblSort.TABLE = T0007INPtbl
            CS0026TblSort.FILTER = "RECODEKBN = '0' and HDKBN = 'H'"
            CS0026TblSort.SORTING = "STAFFCODE, WORKDATE, HDKBN DESC, STDATE, STTIME, ENDDATE, ENDTIME, WORKKBN"
            WW_HEADtbl = CS0026TblSort.Sort()

            Dim WW_TTLtbl As DataTable = T0007INPtbl.Clone
            CS0026TblSort.TABLE = T0007INPtbl
            CS0026TblSort.FILTER = "RECODEKBN <> '0'"
            CS0026TblSort.SORTING = "STAFFCODE, WORKDATE, HDKBN DESC, STDATE, STTIME, ENDDATE, ENDTIME, WORKKBN"
            WW_TTLtbl = CS0026TblSort.Sort()

            Dim WW_BREAKTIME As Integer = 0
            Dim WW_SEQ As Integer = 0
            Dim WW_WORKTIME As Integer = 0
            Dim WW_DATE_SV As String = ""
            Dim WW_TIME_SV As String = ""
            Dim WW_TIME As String = ""
            Dim WW_date As DateTime = Nothing
            For Each WW_HEADrow As DataRow In WW_HEADtbl.Rows
                Dim WW_NIPPONO As String = ""
                Dim WW_A1CNT As Integer = 0
                Dim WW_F1CNT As Integer = 0
                Dim WW_LATITUDE As String = ""
                Dim WW_LONGITUDE As String = ""

                WW_BREAKTIME = 0
                WW_SEQ = 0
                '日報取得
                Dim T0005tbl As DataTable = New DataTable
                T00005ALLget("NEW", WW_HEADrow("STAFFCODE"), WW_HEADrow("NIPPOLINKCODE"), WW_HEADrow("WORKDATE"), WW_HEADrow("WORKDATE"), T0005tbl, WW_ERRCODE)
                If WW_ERRCODE <> C_MESSAGE_NO.NORMAL Then
                    Exit Sub
                End If

                Dim WW_WORKKBN As String = ""
                Dim WW_A1sttime As String = ""
                For i As Integer = 0 To T0005tbl.Rows.Count - 1
                    Dim WW_NIPPOrow As DataRow = T0005tbl.Rows(i)
                    '休憩の合計
                    If WW_NIPPOrow("WORKKBN") = "BB" Then
                        WW_BREAKTIME = WW_BREAKTIME + WW_NIPPOrow("WORKTIME")
                    End If

                    If WW_NIPPOrow("WORKKBN") = "A1" And WW_A1CNT = 0 Then
                        WW_A1CNT += 1
                        WW_A1sttime = WW_NIPPOrow("STTIME")
                        '--------------------------------------------------------------------------------
                        '始業レコード作成
                        '--------------------------------------------------------------------------------
                        WW_T0007row = WW_T0007tbl.NewRow
                        T0007COM.INProw_Init(work.WF_T7SEL_CAMPCODE.Text, WW_T0007row)
                        '開始日時、前のレコードの終了日時
                        WW_T0007row("STDATE") = WF_STDATE.Text
                        WW_T0007row("STTIME") = WF_STTIME.Text
                        '終了日時、後ろレコードの開始日時
                        WW_T0007row("ENDDATE") = WF_STDATE.Text
                        WW_T0007row("ENDTIME") = WF_STTIME.Text

                        'その他の項目は、現在のレコードをコピーする
                        WW_T0007row("WORKDATE") = WW_HEADrow("WORKDATE")
                        WW_T0007row("STAFFCODE") = WW_HEADrow("STAFFCODE")
                        WW_T0007row("STAFFKBN") = WW_HEADrow("STAFFKBN")
                        WW_T0007row("OPERATION") = WW_HEADrow("OPERATION")
                        WW_T0007row("STATUS") = WW_HEADrow("STATUS")
                        WW_T0007row("TAISHOYM") = WW_HEADrow("TAISHOYM")
                        WW_T0007row("MORG") = WW_HEADrow("MORG")
                        WW_T0007row("HORG") = WW_HEADrow("HORG")
                        WW_T0007row("SORG") = WW_NIPPOrow("SHIPORG")
                        WW_SEQ += 1
                        WW_T0007row("SEQ") = WW_SEQ
                        WW_T0007row("WORKINGWEEK") = WW_HEADrow("WORKINGWEEK")
                        WW_T0007row("HIDDEN") = "1"
                        WW_T0007row("HDKBN") = "D"
                        WW_T0007row("DATAKBN") = "K"
                        WW_T0007row("RECODEKBN") = "0"
                        WW_T0007row("WORKKBN") = "A1"
                        '作業時間
                        WW_WORKTIME = DateDiff("n",
                                              WW_T0007row("STDATE") + " " + WW_T0007row("STTIME"),
                                              WW_T0007row("ENDDATE") + " " + WW_T0007row("ENDTIME")
                                             )
                        WW_T0007row("WORKTIME") = T0007COM.formatHHMM(WW_WORKTIME)
                        WW_T0007row("ACTTIME") = T0007COM.formatHHMM(WW_WORKTIME)

                        WW_T0007row("CAMPNAMES") = ""
                        CODENAME_get("CAMPCODE", WW_T0007row("CAMPCODE"), WW_T0007row("CAMPNAMES"), WW_DUMMY)
                        WW_T0007row("WORKKBNNAMES") = ""
                        CODENAME_get("WORKKBN", WW_T0007row("WORKKBN"), WW_T0007row("WORKKBNNAMES"), WW_DUMMY)
                        WW_T0007row("STAFFNAMES") = ""
                        CODENAME_get("STAFFCODE", WW_T0007row("STAFFCODE"), WW_T0007row("STAFFNAMES"), WW_DUMMY)
                        WW_T0007row("HOLIDAYKBNNAMES") = ""
                        CODENAME_get("HOLIDAYKBN", WW_T0007row("HOLIDAYKBN"), WW_T0007row("HOLIDAYKBNNAMES"), WW_DUMMY)
                        WW_T0007row("PAYKBNNAMES") = ""
                        CODENAME_get("PAYKBN", WW_T0007row("PAYKBN"), WW_T0007row("PAYKBNNAMES"), WW_DUMMY)
                        WW_T0007row("SHUKCHOKKBNNAMES") = ""
                        CODENAME_get("SHUKCHOKKBN", WW_T0007row("SHUKCHOKKBN"), WW_T0007row("SHUKCHOKKBNNAMES"), WW_DUMMY)
                        WW_T0007row("MORGNAMES") = ""
                        CODENAME_get("ORG", WW_T0007row("MORG"), WW_T0007row("MORGNAMES"), WW_DUMMY)
                        WW_T0007row("HORGNAMES") = ""
                        CODENAME_get("HORG", WW_T0007row("HORG"), WW_T0007row("HORGNAMES"), WW_DUMMY)
                        WW_T0007row("SORGNAMES") = ""
                        CODENAME_get("HORG", WW_T0007row("SORG"), WW_T0007row("SORGNAMES"), WW_DUMMY)
                        WW_T0007tbl.Rows.Add(WW_T0007row)

                        WW_DATE_SV = WW_T0007row("ENDDATE")
                        WW_TIME_SV = WW_T0007row("ENDTIME")
                        WW_WORKKBN = "A1"
                        Continue For
                    End If
                    '2020/11/17 ADD
                    If WW_NIPPOrow("WORKKBN") = "F1" Then
                        WW_F1CNT += 1

                        If WW_F1CNT = 1 AndAlso WW_UPD_HAISO = "ON" Then
                            '最初の出庫の緯度経度を取得
                            Dim WW_LATITUDE_F1 As String = WW_NIPPOrow("LATITUDE")
                            Dim WW_LONGITUDE_F1 As String = WW_NIPPOrow("LONGITUDE")

                            '配送開始01、配送終了01が00:00以外の場合、配送ボタンONとなる
                            WW_HEADrow("HAISOMINUS10FLG") = "OFF"
                            Dim WW_G1STTIME01 As String = CType(WF_DView1.FindControl("WF_G1STTIME01"), System.Web.UI.WebControls.TextBox).Text
                            If WW_G1STTIME01 <> "00:00" AndAlso
                               WW_G1STTIME01 <> "" Then
                                Dim WW_stdate As DateTime = CDate(WW_HEADrow("STDATE") & " " & WW_A1sttime)
                                Dim WW_dateG1 As DateTime = CDate(WW_HEADrow("STDATE") & " " & WW_G1STTIME01)
                                Dim WW_dateAdd60 As DateTime = WW_stdate.AddMinutes(60)
                                '出庫時刻から60分以内であれば、配送作業（グループ作業ではない）とする
                                If WW_dateG1 <= WW_dateAdd60 Then
                                    If T0005COM.ShakoCheck(work.WF_T7SEL_CAMPCODE.Text, WW_LATITUDE_F1, WW_LONGITUDE_F1) = "OK" Then
                                        '配送ボタンで車庫出発の場合、配送開始-１０分
                                        'WW_date = CDate(WW_HEADrow("STDATE") & " " & WW_G1STTIME01)
                                        'WW_HEADrow("STDATE") = WW_date.AddMinutes(-10).ToString("yyyy/MM/dd")
                                        'WW_HEADrow("STTIME") = WW_date.AddMinutes(-10).ToString("HH:mm")
                                        'WW_HEADrow("ENDDATE") = WW_date.AddMinutes(-10).ToString("yyyy/MM/dd")
                                        'WW_HEADrow("ENDTIME") = WW_date.AddMinutes(-10).ToString("HH:mm")
                                        WW_HEADrow("STTIME") = WW_dateG1.ToString("HH:mm")
                                        WW_HEADrow("HAISOMINUS10FLG") = "ON"
                                    Else
                                        '配送ボタンで車庫以外出発の場合、配送開始そのまま
                                        WW_HEADrow("STDATE") = WW_HEADrow("STDATE")
                                        WW_HEADrow("STTIME") = WW_G1STTIME01
                                        'WW_HEADrow("ENDDATE") = WW_HEADrow("STDATE")
                                        'WW_HEADrow("ENDTIME") = WW_G1STTIME01
                                    End If

                                    '------------------------------------------------------------
                                    '画面項目の入れ替え（配送開始０１を開始時刻へ
                                    '------------------------------------------------------------
                                    WF_STDATE.Text = WW_HEADrow("STDATE")
                                    WF_STTIME.Text = WW_HEADrow("STTIME")
                                End If
                            Else
                                '配送ボタン以外（グループ作業）の場合、上記のA1で決定した30分編集となる
                            End If
                        End If
                    End If
                    '2020/11/17 ADD END

                    If WW_NIPPOrow("WORKKBN") = "F3" Then
                        WW_NIPPONO = WW_NIPPOrow("NIPPONO")
                        WW_DATE_SV = WW_NIPPOrow("ENDDATE")
                        WW_TIME_SV = WW_NIPPOrow("ENDTIME")

                        WW_LATITUDE = WW_NIPPOrow("LATITUDE")
                        WW_LONGITUDE = WW_NIPPOrow("LONGITUDE")
                        Continue For
                    End If

                    '--------------------------------------------------------------------------------
                    '出庫が２回目以降は、前の日報と後ろの日報の間に、その他作業レコードを作成する
                    '--------------------------------------------------------------------------------
                    If WW_F1CNT > 1 Then
                        If WW_NIPPOrow("WORKKBN") = "F1" Then
                            '初期化
                            WW_T0007row = WW_T0007tbl.NewRow
                            T0007COM.INProw_Init(work.WF_T7SEL_CAMPCODE.Text, WW_T0007row)

                            '開始日時、前のレコードの終了日時
                            WW_T0007row("STDATE") = WW_DATE_SV
                            WW_T0007row("STTIME") = WW_TIME_SV
                            '終了日時、後ろレコードの開始日時
                            WW_T0007row("ENDDATE") = WW_NIPPOrow("STDATE")
                            WW_T0007row("ENDTIME") = WW_NIPPOrow("STTIME")

                            'その他の項目は、現在のレコードをコピーする
                            WW_T0007row("WORKDATE") = WW_HEADrow("WORKDATE")
                            WW_T0007row("STAFFCODE") = WW_HEADrow("STAFFCODE")
                            WW_T0007row("STAFFKBN") = WW_HEADrow("STAFFKBN")
                            WW_T0007row("OPERATION") = WW_HEADrow("OPERATION")
                            WW_T0007row("STATUS") = WW_HEADrow("STATUS")
                            WW_T0007row("TAISHOYM") = WW_HEADrow("TAISHOYM")
                            WW_T0007row("MORG") = WW_HEADrow("MORG")
                            WW_T0007row("HORG") = WW_HEADrow("HORG")
                            WW_T0007row("SORG") = WW_NIPPOrow("SHIPORG")
                            WW_SEQ += 1
                            WW_T0007row("SEQ") = WW_SEQ
                            WW_T0007row("WORKINGWEEK") = WW_HEADrow("WORKINGWEEK")
                            WW_T0007row("HIDDEN") = "1"
                            WW_T0007row("HDKBN") = "D"
                            WW_T0007row("DATAKBN") = "K"
                            WW_T0007row("RECODEKBN") = "0"
                            WW_T0007row("WORKKBN") = "BX"

                            '作業時間
                            WW_WORKTIME = DateDiff("n",
                                                  WW_T0007row("STDATE") + " " + WW_T0007row("STTIME"),
                                                  WW_T0007row("ENDDATE") + " " + WW_T0007row("ENDTIME")
                                                 )
                            WW_T0007row("WORKTIME") = T0007COM.formatHHMM(WW_WORKTIME)
                            WW_T0007row("ACTTIME") = T0007COM.formatHHMM(WW_WORKTIME)

                            WW_T0007row("CAMPNAMES") = ""
                            CODENAME_get("CAMPCODE", WW_T0007row("CAMPCODE"), WW_T0007row("CAMPNAMES"), WW_DUMMY)
                            WW_T0007row("WORKKBNNAMES") = ""
                            CODENAME_get("WORKKBN", WW_T0007row("WORKKBN"), WW_T0007row("WORKKBNNAMES"), WW_DUMMY)
                            WW_T0007row("STAFFNAMES") = ""
                            CODENAME_get("STAFFCODE", WW_T0007row("STAFFCODE"), WW_T0007row("STAFFNAMES"), WW_DUMMY)
                            WW_T0007row("HOLIDAYKBNNAMES") = ""
                            CODENAME_get("HOLIDAYKBN", WW_T0007row("HOLIDAYKBN"), WW_T0007row("HOLIDAYKBNNAMES"), WW_DUMMY)
                            WW_T0007row("PAYKBNNAMES") = ""
                            CODENAME_get("PAYKBN", WW_T0007row("PAYKBN"), WW_T0007row("PAYKBNNAMES"), WW_DUMMY)
                            WW_T0007row("SHUKCHOKKBNNAMES") = ""
                            CODENAME_get("SHUKCHOKKBN", WW_T0007row("SHUKCHOKKBN"), WW_T0007row("SHUKCHOKKBNNAMES"), WW_DUMMY)
                            WW_T0007row("MORGNAMES") = ""
                            CODENAME_get("HORG", WW_T0007row("MORG"), WW_T0007row("MORGNAMES"), WW_DUMMY)
                            WW_T0007row("HORGNAMES") = ""
                            CODENAME_get("HORG", WW_T0007row("HORG"), WW_T0007row("HORGNAMES"), WW_DUMMY)
                            WW_T0007row("SORGNAMES") = ""
                            CODENAME_get("HORG", WW_T0007row("SORG"), WW_T0007row("SORGNAMES"), WW_DUMMY)
                            WW_T0007tbl.Rows.Add(WW_T0007row)
                        End If
                    End If

                    WW_WORKKBN = ""
                Next
                '最終レコードの追加
                If T0005tbl.Rows.Count > 0 Then
                    Dim WW_BREAK_FLG As String = "OFF"
                    Dim WW_SHAKO_FLG As String = "OFF"
                    '2020/11/17 UPD
                    'If T0007COM.HHMMtoMinutes(WF_BREAKTIME.Text) > 0 Then
                    '    WW_BREAK_FLG = "ON"
                    '    WW_T0007row = WW_T0007tbl.NewRow
                    '    T0007COM.INProw_Init(work.WF_T7SEL_CAMPCODE.Text, WW_T0007row)

                    '    '開始日時、前のレコードの終了日時
                    '    WW_T0007row("STDATE") = T0005tbl.Rows(T0005tbl.Rows.Count - 1)("ENDDATE")
                    '    WW_T0007row("STTIME") = T0005tbl.Rows(T0005tbl.Rows.Count - 1)("ENDTIME")
                    '    '終了日時、後ろレコードの開始日時
                    '    '６０分－休憩時間
                    '    WW_date = CDate(WW_T0007row("STDATE") & " " & WW_T0007row("STTIME"))
                    '    WW_T0007row("ENDDATE") = WW_date.AddMinutes(T0007COM.HHMMtoMinutes(WF_BREAKTIME.Text)).ToString("yyyy/MM/dd")
                    '    WW_T0007row("ENDTIME") = WW_date.AddMinutes(T0007COM.HHMMtoMinutes(WF_BREAKTIME.Text)).ToString("HH:mm")

                    '    'その他の項目は、現在のレコードをコピーする
                    '    WW_T0007row("WORKDATE") = WW_HEADrow("WORKDATE")
                    '    WW_T0007row("STAFFCODE") = WW_HEADrow("STAFFCODE")
                    '    WW_T0007row("STAFFKBN") = WW_HEADrow("STAFFKBN")
                    '    WW_T0007row("OPERATION") = WW_HEADrow("OPERATION")
                    '    WW_T0007row("STATUS") = WW_HEADrow("STATUS")
                    '    WW_T0007row("TAISHOYM") = WW_HEADrow("TAISHOYM")
                    '    WW_T0007row("MORG") = WW_HEADrow("MORG")
                    '    WW_T0007row("HORG") = WW_HEADrow("HORG")
                    '    WW_T0007row("SORG") = T0005tbl.Rows(T0005tbl.Rows.Count - 1)("SHIPORG")
                    '    WW_SEQ += 1
                    '    WW_T0007row("SEQ") = WW_SEQ
                    '    WW_T0007row("WORKINGWEEK") = WW_HEADrow("WORKINGWEEK")
                    '    WW_T0007row("HIDDEN") = "1"
                    '    WW_T0007row("HDKBN") = "D"
                    '    WW_T0007row("DATAKBN") = "K"
                    '    WW_T0007row("RECODEKBN") = "0"
                    '    WW_T0007row("WORKKBN") = "BB"

                    '    '作業時間
                    '    WW_WORKTIME = DateDiff("n",
                    '                          WW_T0007row("STDATE") + " " + WW_T0007row("STTIME"),
                    '                          WW_T0007row("ENDDATE") + " " + WW_T0007row("ENDTIME")
                    '                         )
                    '    WW_T0007row("WORKTIME") = T0007COM.formatHHMM(WW_WORKTIME)
                    '    WW_T0007row("ACTTIME") = T0007COM.formatHHMM(WW_WORKTIME)
                    '    WW_T0007row("BREAKTIME") = T0007COM.formatHHMM(WW_WORKTIME)

                    '    WW_T0007row("CAMPNAMES") = ""
                    '    CODENAME_get("CAMPCODE", WW_T0007row("CAMPCODE"), WW_T0007row("CAMPNAMES"), WW_DUMMY)
                    '    WW_T0007row("WORKKBNNAMES") = ""
                    '    CODENAME_get("WORKKBN", WW_T0007row("WORKKBN"), WW_T0007row("WORKKBNNAMES"), WW_DUMMY)
                    '    WW_T0007row("STAFFNAMES") = ""
                    '    CODENAME_get("STAFFCODE", WW_T0007row("STAFFCODE"), WW_T0007row("STAFFNAMES"), WW_DUMMY)
                    '    WW_T0007row("HOLIDAYKBNNAMES") = ""
                    '    CODENAME_get("HOLIDAYKBN", WW_T0007row("HOLIDAYKBN"), WW_T0007row("HOLIDAYKBNNAMES"), WW_DUMMY)
                    '    WW_T0007row("PAYKBNNAMES") = ""
                    '    CODENAME_get("PAYKBN", WW_T0007row("PAYKBN"), WW_T0007row("PAYKBNNAMES"), WW_DUMMY)
                    '    WW_T0007row("SHUKCHOKKBNNAMES") = ""
                    '    CODENAME_get("SHUKCHOKKBN", WW_T0007row("SHUKCHOKKBN"), WW_T0007row("SHUKCHOKKBNNAMES"), WW_DUMMY)
                    '    WW_T0007row("MORGNAMES") = ""
                    '    CODENAME_get("ORG", WW_T0007row("MORG"), WW_T0007row("MORGNAMES"), WW_DUMMY)
                    '    WW_T0007row("HORGNAMES") = ""
                    '    CODENAME_get("HORG", WW_T0007row("HORG"), WW_T0007row("HORGNAMES"), WW_DUMMY)
                    '    WW_T0007row("SORGNAMES") = ""
                    '    CODENAME_get("HORG", WW_T0007row("SORG"), WW_T0007row("SORGNAMES"), WW_DUMMY)
                    '    WW_T0007tbl.Rows.Add(WW_T0007row)

                    '    WW_DATE_SV = WW_T0007row("ENDDATE")
                    '    WW_TIME_SV = WW_T0007row("ENDTIME")
                    'End If
                    '2020/11/17 UPD END

                    If T0005COM.ShakoCheck(WF_CAMPCODE.Text, WW_LATITUDE, WW_LONGITUDE) = "OK" Then
                        '--------------------------------------------------------------------------------
                        '車庫に帰ってきたら、他作業（＋１０分）レコード作成（最後のデータ）
                        '--------------------------------------------------------------------------------
                        WW_SHAKO_FLG = "ON"
                        WW_T0007row = WW_T0007tbl.NewRow
                        T0007COM.INProw_Init(work.WF_T7SEL_CAMPCODE.Text, WW_T0007row)

                        If WW_BREAK_FLG = "OFF" Then
                            '開始日時、前のレコードの終了日時
                            WW_T0007row("STDATE") = T0005tbl.Rows(T0005tbl.Rows.Count - 1)("ENDDATE")
                            WW_T0007row("STTIME") = T0005tbl.Rows(T0005tbl.Rows.Count - 1)("ENDTIME")
                        Else
                            '開始日時、前のレコードの終了日時
                            WW_T0007row("STDATE") = WW_DATE_SV
                            WW_T0007row("STTIME") = WW_TIME_SV
                            ''終了日時、後ろレコードの開始日時
                            'WW_T0007row("ENDDATE") = WW_DATE_SV
                        End If

                        '拘束時間（＋１０分）
                        WW_date = CDate(WW_T0007row("STDATE") & " " & WW_T0007row("STTIME"))
                        WW_T0007row("ENDDATE") = WW_date.AddMinutes(10).ToString("yyyy/MM/dd")
                        WW_T0007row("ENDTIME") = WW_date.AddMinutes(10).ToString("HH:mm")

                        'その他の項目は、現在のレコードをコピーする
                        WW_T0007row("WORKDATE") = WW_HEADrow("WORKDATE")
                        WW_T0007row("STAFFCODE") = WW_HEADrow("STAFFCODE")
                        WW_T0007row("STAFFKBN") = WW_HEADrow("STAFFKBN")
                        WW_T0007row("OPERATION") = WW_HEADrow("OPERATION")
                        WW_T0007row("STATUS") = WW_HEADrow("STATUS")
                        WW_T0007row("TAISHOYM") = WW_HEADrow("TAISHOYM")
                        WW_T0007row("MORG") = WW_HEADrow("MORG")
                        WW_T0007row("HORG") = WW_HEADrow("HORG")
                        WW_T0007row("SORG") = T0005tbl.Rows(T0005tbl.Rows.Count - 1)("SHIPORG")
                        WW_SEQ += 1
                        WW_T0007row("SEQ") = WW_SEQ
                        WW_T0007row("WORKINGWEEK") = WW_HEADrow("WORKINGWEEK")
                        WW_T0007row("HIDDEN") = "1"
                        WW_T0007row("HDKBN") = "D"
                        WW_T0007row("DATAKBN") = "K"
                        WW_T0007row("RECODEKBN") = "0"
                        WW_T0007row("WORKKBN") = "BX"
                        WW_T0007row("DELFLG") = "0"

                        '作業時間
                        WW_WORKTIME = DateDiff("n",
                                              WW_T0007row("STDATE") + " " + WW_T0007row("STTIME"),
                                              WW_T0007row("ENDDATE") + " " + WW_T0007row("ENDTIME")
                                             )
                        WW_T0007row("WORKTIME") = T0007COM.formatHHMM(WW_WORKTIME)
                        WW_T0007row("ACTTIME") = T0007COM.formatHHMM(WW_WORKTIME)

                        WW_T0007row("CAMPNAMES") = ""
                        CODENAME_get("CAMPCODE", WW_T0007row("CAMPCODE"), WW_T0007row("CAMPNAMES"), WW_DUMMY)
                        WW_T0007row("WORKKBNNAMES") = ""
                        CODENAME_get("WORKKBN", WW_T0007row("WORKKBN"), WW_T0007row("WORKKBNNAMES"), WW_DUMMY)
                        WW_T0007row("STAFFNAMES") = ""
                        CODENAME_get("STAFFCODE", WW_T0007row("STAFFCODE"), WW_T0007row("STAFFNAMES"), WW_DUMMY)
                        WW_T0007row("HOLIDAYKBNNAMES") = ""
                        CODENAME_get("HOLIDAYKBN", WW_T0007row("HOLIDAYKBN"), WW_T0007row("HOLIDAYKBNNAMES"), WW_DUMMY)
                        WW_T0007row("PAYKBNNAMES") = ""
                        CODENAME_get("PAYKBN", WW_T0007row("PAYKBN"), WW_T0007row("PAYKBNNAMES"), WW_DUMMY)
                        WW_T0007row("SHUKCHOKKBNNAMES") = ""
                        CODENAME_get("SHUKCHOKKBN", WW_T0007row("SHUKCHOKKBN"), WW_T0007row("SHUKCHOKKBNNAMES"), WW_DUMMY)
                        WW_T0007row("MORGNAMES") = ""
                        CODENAME_get("HORG", WW_T0007row("MORG"), WW_T0007row("MORGNAMES"), WW_DUMMY)
                        WW_T0007row("HORGNAMES") = ""
                        CODENAME_get("HORG", WW_T0007row("HORG"), WW_T0007row("HORGNAMES"), WW_DUMMY)
                        WW_T0007row("SORGNAMES") = ""
                        CODENAME_get("HORG", WW_T0007row("SORG"), WW_T0007row("SORGNAMES"), WW_DUMMY)
                        WW_T0007tbl.Rows.Add(WW_T0007row)

                        WW_DATE_SV = WW_T0007row("ENDDATE")
                        WW_TIME_SV = WW_T0007row("ENDTIME")
                    End If

                    '--------------------------------------------------------------------------------
                    '他作業（＋？？分）レコード作成（退社時間との差）
                    '--------------------------------------------------------------------------------
                    If CDate(WW_DATE_SV & " " & WW_TIME_SV) < CDate(WF_ENDDATE.Text & " " & WF_ENDTIME.Text) Then
                        WW_T0007row = WW_T0007tbl.NewRow
                        T0007COM.INProw_Init(work.WF_T7SEL_CAMPCODE.Text, WW_T0007row)

                        If WW_BREAK_FLG = "OFF" And WW_SHAKO_FLG = "OFF" Then
                            '開始日時、前のレコードの終了日時
                            WW_T0007row("STDATE") = T0005tbl.Rows(T0005tbl.Rows.Count - 1)("ENDDATE")
                            WW_T0007row("STTIME") = T0005tbl.Rows(T0005tbl.Rows.Count - 1)("ENDTIME")
                        Else
                            '開始日時、前のレコードの終了日時
                            WW_T0007row("STDATE") = WW_DATE_SV
                            WW_T0007row("STTIME") = WW_TIME_SV
                        End If
                        '終了日時、後ろレコードの開始日時
                        WW_T0007row("ENDDATE") = WF_ENDDATE.Text
                        WW_T0007row("ENDTIME") = WF_ENDTIME.Text

                        'その他の項目は、現在のレコードをコピーする
                        WW_T0007row("WORKDATE") = WW_HEADrow("WORKDATE")
                        WW_T0007row("STAFFCODE") = WW_HEADrow("STAFFCODE")
                        WW_T0007row("STAFFKBN") = WW_HEADrow("STAFFKBN")
                        WW_T0007row("OPERATION") = WW_HEADrow("OPERATION")
                        WW_T0007row("STATUS") = WW_HEADrow("STATUS")
                        WW_T0007row("TAISHOYM") = WW_HEADrow("TAISHOYM")
                        WW_T0007row("MORG") = WW_HEADrow("MORG")
                        WW_T0007row("HORG") = WW_HEADrow("HORG")
                        WW_T0007row("SORG") = T0005tbl.Rows(T0005tbl.Rows.Count - 1)("SHIPORG")
                        WW_SEQ += 1
                        WW_T0007row("SEQ") = WW_SEQ
                        WW_T0007row("WORKINGWEEK") = WW_HEADrow("WORKINGWEEK")
                        WW_T0007row("HIDDEN") = "1"
                        WW_T0007row("HDKBN") = "D"
                        WW_T0007row("DATAKBN") = "K"
                        WW_T0007row("RECODEKBN") = "0"
                        WW_T0007row("WORKKBN") = "BX"
                        WW_T0007row("DELFLG") = "0"

                        '作業時間
                        WW_WORKTIME = DateDiff("n",
                                                WW_T0007row("STDATE") + " " + WW_T0007row("STTIME"),
                                                WW_T0007row("ENDDATE") + " " + WW_T0007row("ENDTIME")
                                                )
                        WW_T0007row("WORKTIME") = T0007COM.formatHHMM(WW_WORKTIME)
                        WW_T0007row("ACTTIME") = T0007COM.formatHHMM(WW_WORKTIME)

                        WW_T0007row("CAMPNAMES") = ""
                        CODENAME_get("CAMPCODE", WW_T0007row("CAMPCODE"), WW_T0007row("CAMPNAMES"), WW_DUMMY)
                        WW_T0007row("WORKKBNNAMES") = ""
                        CODENAME_get("WORKKBN", WW_T0007row("WORKKBN"), WW_T0007row("WORKKBNNAMES"), WW_DUMMY)
                        WW_T0007row("STAFFNAMES") = ""
                        CODENAME_get("STAFFCODE", WW_T0007row("STAFFCODE"), WW_T0007row("STAFFNAMES"), WW_DUMMY)
                        WW_T0007row("HOLIDAYKBNNAMES") = ""
                        CODENAME_get("HOLIDAYKBN", WW_T0007row("HOLIDAYKBN"), WW_T0007row("HOLIDAYKBNNAMES"), WW_DUMMY)
                        WW_T0007row("PAYKBNNAMES") = ""
                        CODENAME_get("PAYKBN", WW_T0007row("PAYKBN"), WW_T0007row("PAYKBNNAMES"), WW_DUMMY)
                        WW_T0007row("SHUKCHOKKBNNAMES") = ""
                        CODENAME_get("SHUKCHOKKBN", WW_T0007row("SHUKCHOKKBN"), WW_T0007row("SHUKCHOKKBNNAMES"), WW_DUMMY)
                        WW_T0007row("MORGNAMES") = ""
                        CODENAME_get("ORG", WW_T0007row("MORG"), WW_T0007row("MORGNAMES"), WW_DUMMY)
                        WW_T0007row("HORGNAMES") = ""
                        CODENAME_get("HORG", WW_T0007row("HORG"), WW_T0007row("HORGNAMES"), WW_DUMMY)
                        WW_T0007row("SORGNAMES") = ""
                        CODENAME_get("HORG", WW_T0007row("SORG"), WW_T0007row("SORGNAMES"), WW_DUMMY)
                        WW_T0007tbl.Rows.Add(WW_T0007row)

                        WW_DATE_SV = WW_T0007row("ENDDATE")
                        WW_TIME_SV = WW_T0007row("ENDTIME")
                    End If
                    '--------------------------------------------------------------------------------
                    '終業レコード作成（最後のデータ）
                    '--------------------------------------------------------------------------------
                    WW_T0007row = WW_T0007tbl.NewRow
                    T0007COM.INProw_Init(work.WF_T7SEL_CAMPCODE.Text, WW_T0007row)

                    '開始日時、前のレコードの終了日時
                    WW_T0007row("STDATE") = WW_DATE_SV
                    WW_T0007row("STTIME") = WW_TIME_SV
                    '終了日時、後ろレコードの開始日時
                    WW_T0007row("ENDDATE") = WW_DATE_SV
                    WW_T0007row("ENDTIME") = WW_TIME_SV

                    'その他の項目は、現在のレコードをコピーする
                    WW_T0007row("WORKDATE") = WW_HEADrow("WORKDATE")
                    WW_T0007row("STAFFCODE") = WW_HEADrow("STAFFCODE")
                    WW_T0007row("STAFFKBN") = WW_HEADrow("STAFFKBN")
                    WW_T0007row("OPERATION") = WW_HEADrow("OPERATION")
                    WW_T0007row("STATUS") = WW_HEADrow("STATUS")
                    WW_T0007row("TAISHOYM") = WW_HEADrow("TAISHOYM")
                    WW_T0007row("MORG") = WW_HEADrow("MORG")
                    WW_T0007row("HORG") = WW_HEADrow("HORG")
                    WW_T0007row("SORG") = T0005tbl.Rows(T0005tbl.Rows.Count - 1)("SHIPORG")
                    WW_SEQ += 1
                    WW_T0007row("SEQ") = WW_SEQ
                    WW_T0007row("WORKINGWEEK") = WW_HEADrow("WORKINGWEEK")
                    WW_T0007row("HIDDEN") = "1"
                    WW_T0007row("HDKBN") = "D"
                    WW_T0007row("DATAKBN") = "K"
                    WW_T0007row("RECODEKBN") = "0"
                    WW_T0007row("WORKKBN") = "Z1"
                    WW_T0007row("DELFLG") = "0"

                    '作業時間
                    WW_WORKTIME = DateDiff("n",
                                            WW_T0007row("STDATE") + " " + WW_T0007row("STTIME"),
                                            WW_T0007row("ENDDATE") + " " + WW_T0007row("ENDTIME")
                                            )
                    WW_T0007row("WORKTIME") = T0007COM.formatHHMM(WW_WORKTIME)
                    WW_T0007row("ACTTIME") = T0007COM.formatHHMM(WW_WORKTIME)

                    WW_T0007row("CAMPNAMES") = ""
                    CODENAME_get("CAMPCODE", WW_T0007row("CAMPCODE"), WW_T0007row("CAMPNAMES"), WW_DUMMY)
                    WW_T0007row("WORKKBNNAMES") = ""
                    CODENAME_get("WORKKBN", WW_T0007row("WORKKBN"), WW_T0007row("WORKKBNNAMES"), WW_DUMMY)
                    WW_T0007row("STAFFNAMES") = ""
                    CODENAME_get("STAFFCODE", WW_T0007row("STAFFCODE"), WW_T0007row("STAFFNAMES"), WW_DUMMY)
                    WW_T0007row("HOLIDAYKBNNAMES") = ""
                    CODENAME_get("HOLIDAYKBN", WW_T0007row("HOLIDAYKBN"), WW_T0007row("HOLIDAYKBNNAMES"), WW_DUMMY)
                    WW_T0007row("PAYKBNNAMES") = ""
                    CODENAME_get("PAYKBN", WW_T0007row("PAYKBN"), WW_T0007row("PAYKBNNAMES"), WW_DUMMY)
                    WW_T0007row("SHUKCHOKKBNNAMES") = ""
                    CODENAME_get("SHUKCHOKKBN", WW_T0007row("SHUKCHOKKBN"), WW_T0007row("SHUKCHOKKBNNAMES"), WW_DUMMY)
                    WW_T0007row("MORGNAMES") = ""
                    CODENAME_get("ORG", WW_T0007row("MORG"), WW_T0007row("MORGNAMES"), WW_DUMMY)
                    WW_T0007row("HORGNAMES") = ""
                    CODENAME_get("HORG", WW_T0007row("HORG"), WW_T0007row("HORGNAMES"), WW_DUMMY)
                    WW_T0007row("SORGNAMES") = ""
                    CODENAME_get("HORG", WW_T0007row("SORG"), WW_T0007row("SORGNAMES"), WW_DUMMY)
                    WW_T0007tbl.Rows.Add(WW_T0007row)
                End If

                NIPPOget_T7Format2(WW_T0007tbl, T0005tbl, WW_HEADrow)

            Next

            '追加（BB）マージ
            T0007INPtbl = WW_HEADtbl.Copy
            T0007INPtbl.Merge(WW_TTLtbl)
            T0007INPtbl.Merge(WW_T0007tbl)

            WW_HEADtbl.Dispose()
            WW_HEADtbl = Nothing
            WW_TTLtbl.Dispose()
            WW_TTLtbl = Nothing
            WW_T0007tbl.Dispose()
            WW_T0007tbl = Nothing

            '上記処理で、明細（開始、終了、休憩）が変わったためヘッダを再度編集し、画面表示する
            'ヘッダの集計
            For Each WW_HEADrow As DataRow In T0007INPtbl.Rows
                If WW_HEADrow("HDKBN") = "H" And WW_HEADrow("RECODEKBN") = "0" Then
                Else
                    Continue For
                End If

                Dim WW_BREAKTIME2 As Integer = 0
                Dim WW_B3CNT As Integer = 0
                Dim WW_UNLOADCNT As Integer = 0                             '荷卸回数
                Dim WW_MATCH As String = "OFF"
                Dim WW_MATCH2 As String = "OFF"
                '勤怠レコードの必要情報からヘッダを集計
                For Each WW_DTLrow As DataRow In T0007INPtbl.Rows
                    If WW_DTLrow("RECODEKBN") = "0" Then
                        If WW_DTLrow("WORKKBN") = "A1" Then
                            '2020/11/17 UPD
                            If WW_UPD_HAISO = "ON" Then
                                '配送時間が更新されている場合、上の処理でヘッダの開始開始日、開始時間が決定しているためヘッダから明細に戻す
                                WW_DTLrow("STDATE") = WW_HEADrow("STDATE")
                                WW_DTLrow("STTIME") = WW_HEADrow("STTIME")
                                WW_DTLrow("ENDDATE") = WW_HEADrow("ENDDATE")
                                WW_DTLrow("ENDTIME") = WW_HEADrow("ENDTIME")
                                WF_BINDSTDATE.Text = WW_HEADrow("STTIME")
                            Else
                                '出社レコードより開始日、開始時間を取得
                                WW_HEADrow("STDATE") = WW_DTLrow("STDATE")
                                WW_HEADrow("STTIME") = WW_DTLrow("STTIME")
                            End If
                            '2020/11/17 UPD END
                        End If

                        If WW_DTLrow("WORKKBN") = "Z1" Then
                            '退社レコードの終了日、終了時間を取得
                            WW_HEADrow("ENDDATE") = WW_DTLrow("ENDDATE")
                            WW_HEADrow("ENDTIME") = WW_DTLrow("ENDTIME")
                        End If
                    End If

                    '2020/11/17 UPD
                    'If WW_DTLrow("DATAKBN") = "K" And WW_DTLrow("WORKKBN") = "BB" Then
                    '    '休憩レコードを取得
                    '    WW_BREAKTIME2 += TimeSpan.Parse(WW_DTLrow("WORKTIME")).TotalMinutes
                    'End If
                    '2020/11/17 UPD END
                Next

                '2020/11/17 UPD
                'If WW_BREAKTIME2 = 0 Then
                '    WW_HEADrow("BREAKTIME") = WF_BREAKTIME.Text
                '    WW_HEADrow("BREAKTIMETTL") = T0007COM.formatHHMM(T0007COM.HHMMtoMinutes(WF_BREAKTIME.Text) + WW_BREAKTIME2)
                'Else
                '    WW_HEADrow("BREAKTIME") = T0007COM.formatHHMM(WW_BREAKTIME2)
                '    WW_HEADrow("BREAKTIMETTL") = T0007COM.formatHHMM(WW_BREAKTIME + WW_BREAKTIME2)
                'End If
                WW_HEADrow("BREAKTIME") = "00:00"
                WW_HEADrow("BREAKTIMETTL") = WW_HEADrow("BREAKTIME")
                '2020/11/17 UPD END
                WW_HEADrow("BINDSTDATE") = WF_BINDSTDATE.Text
                If IsDBNull(WW_HEADrow("STDATE")) Or
                    IsDBNull(WW_HEADrow("ENDDATE")) Or
                    IsDBNull(WW_HEADrow("STTIME")) Or
                    IsDBNull(WW_HEADrow("ENDTIME")) Then
                    WW_HEADrow("WORKTIME") = T0007COM.formatHHMM(0)
                    WW_HEADrow("ACTTIME") = T0007COM.formatHHMM(0)
                Else
                    WW_WORKTIME = DateDiff("n",
                                         WW_HEADrow("STDATE") + " " + WW_HEADrow("STTIME"),
                                         WW_HEADrow("ENDDATE") + " " + WW_HEADrow("ENDTIME")
                                        )
                    WW_HEADrow("WORKTIME") = T0007COM.formatHHMM(WW_WORKTIME)
                    WW_HEADrow("ACTTIME") = T0007COM.formatHHMM(WW_WORKTIME)
                End If
            Next
        End If

        '月合計入力の変更取込
        For Each T0007INProw As DataRow In T0007INPtbl.Rows
            'HDKBN（H:ﾍｯﾀﾞﾚｺｰﾄﾞ、D:明細ﾚｺｰﾄﾞ）、RECODEKBN（0:指定日ﾚｺｰﾄﾞ、1:月調整ﾚｺｰﾄﾞ、2:合計ﾚｺｰﾄﾞ）
            If T0007INProw("HDKBN") = "H" And T0007INProw("RECODEKBN") = "2" Then
                T0007INProw("OPERATION") = "更新"
                T0007INProw("TIMSTP") = 0
                T0007INProw("WORKNISSUCHO") = Val(WF_WORKNISSUTTL.Text) - T0007INProw("WORKNISSU")
                T0007INProw("WORKNISSUTTL") = Val(T0007INProw("WORKNISSU")) + Val(T0007INProw("WORKNISSUCHO"))
                T0007INProw("SHOUKETUNISSUCHO") = Val(WF_SHOUKETUNISSUTTL.Text) - T0007INProw("SHOUKETUNISSU")
                T0007INProw("SHOUKETUNISSUTTL") = Val(T0007INProw("SHOUKETUNISSU")) + Val(T0007INProw("SHOUKETUNISSUCHO"))
                T0007INProw("KUMIKETUNISSUCHO") = Val(WF_KUMIKETUNISSUTTL.Text) - T0007INProw("KUMIKETUNISSU")
                T0007INProw("KUMIKETUNISSUTTL") = Val(T0007INProw("KUMIKETUNISSU")) + Val(T0007INProw("KUMIKETUNISSUCHO"))
                T0007INProw("ETCKETUNISSUCHO") = Val(WF_ETCKETUNISSUTTL.Text) - T0007INProw("ETCKETUNISSU")
                T0007INProw("ETCKETUNISSUTTL") = Val(T0007INProw("ETCKETUNISSU")) + Val(T0007INProw("ETCKETUNISSUCHO"))
                T0007INProw("NENKYUNISSUCHO") = Val(WF_NENKYUNISSUTTL.Text) - T0007INProw("NENKYUNISSU")
                T0007INProw("NENKYUNISSUTTL") = Val(T0007INProw("NENKYUNISSU")) + Val(T0007INProw("NENKYUNISSUCHO"))
                T0007INProw("TOKUKYUNISSUCHO") = Val(WF_TOKUKYUNISSUTTL.Text) - T0007INProw("TOKUKYUNISSU")
                T0007INProw("TOKUKYUNISSUTTL") = Val(T0007INProw("TOKUKYUNISSU")) + Val(T0007INProw("TOKUKYUNISSUCHO"))
                T0007INProw("CHIKOKSOTAINISSUCHO") = Val(WF_CHIKOKSOTAINISSUTTL.Text) - T0007INProw("CHIKOKSOTAINISSU")
                T0007INProw("CHIKOKSOTAINISSUTTL") = Val(T0007INProw("CHIKOKSOTAINISSU")) + Val(T0007INProw("CHIKOKSOTAINISSUCHO"))
                T0007INProw("STOCKNISSUCHO") = Val(WF_STOCKNISSUTTL.Text) - T0007INProw("STOCKNISSU")
                T0007INProw("STOCKNISSUTTL") = Val(T0007INProw("STOCKNISSU")) + Val(T0007INProw("STOCKNISSUCHO"))
                T0007INProw("KYOTEIWEEKNISSUCHO") = Val(WF_KYOTEIWEEKNISSUTTL.Text) - T0007INProw("KYOTEIWEEKNISSU")
                T0007INProw("KYOTEIWEEKNISSUTTL") = Val(T0007INProw("KYOTEIWEEKNISSU")) + Val(T0007INProw("KYOTEIWEEKNISSUCHO"))
                T0007INProw("WEEKNISSUCHO") = 0 - T0007INProw("WEEKNISSU")
                T0007INProw("WEEKNISSUTTL") = Val(T0007INProw("WEEKNISSU")) + Val(T0007INProw("WEEKNISSUCHO"))
                T0007INProw("ROSAIYUKYNIUSSUCHO") = Val(WF_ROSAIYUKYNIUSSUTTL.Text) - T0007INProw("ROSAIYUKYNIUSSU")
                T0007INProw("ROSAIYUKYNIUSSUTTL") = Val(T0007INProw("ROSAIYUKYNIUSSU")) + Val(T0007INProw("ROSAIYUKYNIUSSUCHO"))
                T0007INProw("TOKUKYUMUKYUNISSUCHO") = Val(WF_TOKUKYUMUKYUNISSUTTL.Text) - T0007INProw("TOKUKYUMUKYUNISSU")
                T0007INProw("TOKUKYUMUKYUNISSUTTL") = Val(T0007INProw("TOKUKYUMUKYUNISSU")) + Val(T0007INProw("TOKUKYUMUKYUNISSUCHO"))
                T0007INProw("KOKANGOYUKYUNISSUCHO") = Val(WF_KOKANGOYUKYUNISSUTTL.Text) - T0007INProw("KOKANGOYUKYUNISSU")
                T0007INProw("KOKANGOYUKYUNISSUTTL") = Val(T0007INProw("KOKANGOYUKYUNISSU")) + Val(T0007INProw("KOKANGOYUKYUNISSUCHO"))
                T0007INProw("KOKANGOMUKYUNISSUCHO") = Val(WF_KOKANGOMUKYUNISSUTTL.Text) - T0007INProw("KOKANGOMUKYUNISSU")
                T0007INProw("KOKANGOMUKYUNISSUTTL") = Val(T0007INProw("KOKANGOMUKYUNISSU")) + Val(T0007INProw("KOKANGOMUKYUNISSUCHO"))
                T0007INProw("DAIKYUNISSUCHO") = Val(WF_DAIKYUNISSUTTL.Text) - T0007INProw("DAIKYUNISSU")
                T0007INProw("DAIKYUNISSUTTL") = Val(T0007INProw("DAIKYUNISSU")) + Val(T0007INProw("DAIKYUNISSUCHO"))
                T0007INProw("NENSHINISSUCHO") = Val(WF_NENSHINISSUTTL.Text) - T0007INProw("NENSHINISSU")
                T0007INProw("NENSHINISSUTTL") = Val(T0007INProw("NENSHINISSU")) + Val(T0007INProw("NENSHINISSUCHO"))
                T0007INProw("NENMATUNISSUCHO") = Val(WF_NENMATUNISSUTTL.Text) - T0007INProw("NENMATUNISSU")
                T0007INProw("NENMATUNISSUTTL") = Val(T0007INProw("NENMATUNISSU")) + Val(T0007INProw("NENMATUNISSUCHO"))
                T0007INProw("SHACHUHAKNISSUCHO") = Val(WF_SHACHUHAKNISSUTTL.Text) - T0007INProw("SHACHUHAKNISSU")
                T0007INProw("SHACHUHAKNISSUTTL") = Val(T0007INProw("SHACHUHAKNISSU")) + Val(T0007INProw("SHACHUHAKNISSUCHO"))
                T0007INProw("SHUKCHOKNNISSUCHO") = 0
                T0007INProw("SHUKCHOKNNISSUTTL") = 0
                T0007INProw("SHUKCHOKNISSUCHO") = 0
                T0007INProw("SHUKCHOKNISSUTTL") = 0

                T0007INProw("SHUKCHOKNHLDNISSUCHO") = 0
                T0007INProw("SHUKCHOKNHLDNISSUTTL") = 0
                T0007INProw("SHUKCHOKHLDNISSUCHO") = 0
                T0007INProw("SHUKCHOKHLDNISSUTTL") = 0

                T0007INProw("TOKSAAKAISUCHO") = 0
                T0007INProw("TOKSAAKAISUTTL") = 0
                T0007INProw("TOKSABKAISUCHO") = 0
                T0007INProw("TOKSABKAISUTTL") = 0
                T0007INProw("TOKSACKAISUCHO") = 0
                T0007INProw("TOKSACKAISUTTL") = 0
                T0007INProw("TENKOKAISUCHO") = 0
                T0007INProw("TENKOKAISUTTL") = 0

                T0007INProw("NIGHTTIMECHO") = T0007COM.HHMMtoMinutes(WF_NIGHTTIMETTL.Text) - T0007COM.HHMMtoMinutes(T0007INProw("NIGHTTIME"))
                T0007INProw("NIGHTTIMETTL") = T0007COM.HHMMtoMinutes(T0007INProw("NIGHTTIME")) + T0007INProw("NIGHTTIMECHO")
                T0007INProw("ORVERTIMECHO") = T0007COM.HHMMtoMinutes(WF_ORVERTIMETTL.Text) - T0007COM.HHMMtoMinutes(T0007INProw("ORVERTIME"))
                T0007INProw("ORVERTIMETTL") = T0007COM.HHMMtoMinutes(T0007INProw("ORVERTIME")) + T0007INProw("ORVERTIMECHO")
                T0007INProw("WNIGHTTIMECHO") = T0007COM.HHMMtoMinutes(WF_WNIGHTTIMETTL.Text) - T0007COM.HHMMtoMinutes(T0007INProw("WNIGHTTIME"))
                T0007INProw("WNIGHTTIMETTL") = T0007COM.HHMMtoMinutes(T0007INProw("WNIGHTTIME")) + T0007INProw("WNIGHTTIMECHO")
                T0007INProw("SWORKTIMECHO") = T0007COM.HHMMtoMinutes(WF_SWORKTIMETTL.Text) - T0007COM.HHMMtoMinutes(T0007INProw("SWORKTIME"))
                T0007INProw("SWORKTIMETTL") = T0007COM.HHMMtoMinutes(T0007INProw("SWORKTIME")) + T0007INProw("SWORKTIMECHO")
                T0007INProw("SNIGHTTIMECHO") = T0007COM.HHMMtoMinutes(WF_SNIGHTTIMETTL.Text) - T0007COM.HHMMtoMinutes(T0007INProw("SNIGHTTIME"))
                T0007INProw("SNIGHTTIMETTL") = T0007COM.HHMMtoMinutes(T0007INProw("SNIGHTTIME")) + T0007INProw("SNIGHTTIMECHO")
                T0007INProw("HWORKTIMECHO") = T0007COM.HHMMtoMinutes(WF_HWORKTIMETTL.Text) - T0007COM.HHMMtoMinutes(T0007INProw("HWORKTIME"))
                T0007INProw("HWORKTIMETTL") = T0007COM.HHMMtoMinutes(T0007INProw("HWORKTIME")) + T0007INProw("HWORKTIMECHO")
                T0007INProw("HNIGHTTIMECHO") = T0007COM.HHMMtoMinutes(WF_HNIGHTTIMETTL.Text) - T0007COM.HHMMtoMinutes(T0007INProw("HNIGHTTIME"))
                T0007INProw("HNIGHTTIMETTL") = T0007COM.HHMMtoMinutes(T0007INProw("HNIGHTTIME")) + T0007INProw("HNIGHTTIMECHO")

                T0007INProw("HOANTIMECHO") = 0
                T0007INProw("HOANTIMETTL") = 0
                T0007INProw("KOATUTIMECHO") = 0
                T0007INProw("KOATUTIMETTL") = 0
                T0007INProw("TOKUSA1TIMECHO") = T0007COM.HHMMtoMinutes(WF_TOKUSA1TIMETTL.Text) - T0007COM.HHMMtoMinutes(T0007INProw("TOKUSA1TIME"))
                T0007INProw("TOKUSA1TIMETTL") = T0007COM.HHMMtoMinutes(T0007INProw("TOKUSA1TIME")) + T0007INProw("TOKUSA1TIMECHO")
                T0007INProw("PONPNISSUCHO") = 0
                T0007INProw("PONPNISSUTTL") = 0
                T0007INProw("BULKNISSUCHO") = 0
                T0007INProw("BULKNISSUTTL") = 0
                T0007INProw("TRAILERNISSUCHO") = 0
                T0007INProw("TRAILERNISSUTTL") = 0
                T0007INProw("BKINMUKAISUCHO") = 0
                T0007INProw("BKINMUKAISUTTL") = 0
                T0007INProw("JIKYUSHATIMECHO") = T0007COM.HHMMtoMinutes(WF_JIKYUSHATIMETTL.Text) - T0007COM.HHMMtoMinutes(T0007INProw("JIKYUSHATIME"))
                T0007INProw("JIKYUSHATIMETTL") = T0007COM.HHMMtoMinutes(T0007INProw("JIKYUSHATIME")) + T0007INProw("JIKYUSHATIMECHO")

                T0007INProw("NIGHTTIMECHO") = T0007COM.formatHHMM(T0007INProw("NIGHTTIMECHO"))
                T0007INProw("NIGHTTIMETTL") = T0007COM.formatHHMM(T0007INProw("NIGHTTIMETTL"))
                T0007INProw("ORVERTIMECHO") = T0007COM.formatHHMM(T0007INProw("ORVERTIMECHO"))
                T0007INProw("ORVERTIMETTL") = T0007COM.formatHHMM(T0007INProw("ORVERTIMETTL"))
                T0007INProw("WNIGHTTIMECHO") = T0007COM.formatHHMM(T0007INProw("WNIGHTTIMECHO"))
                T0007INProw("WNIGHTTIMETTL") = T0007COM.formatHHMM(T0007INProw("WNIGHTTIMETTL"))
                T0007INProw("SWORKTIMECHO") = T0007COM.formatHHMM(T0007INProw("SWORKTIMECHO"))
                T0007INProw("SWORKTIMETTL") = T0007COM.formatHHMM(T0007INProw("SWORKTIMETTL"))
                T0007INProw("SNIGHTTIMECHO") = T0007COM.formatHHMM(T0007INProw("SNIGHTTIMECHO"))
                T0007INProw("SNIGHTTIMETTL") = T0007COM.formatHHMM(T0007INProw("SNIGHTTIMETTL"))
                T0007INProw("HWORKTIMECHO") = T0007COM.formatHHMM(T0007INProw("HWORKTIMECHO"))
                T0007INProw("HWORKTIMETTL") = T0007COM.formatHHMM(T0007INProw("HWORKTIMETTL"))
                T0007INProw("HNIGHTTIMECHO") = T0007COM.formatHHMM(T0007INProw("HNIGHTTIMECHO"))
                T0007INProw("HNIGHTTIMETTL") = T0007COM.formatHHMM(T0007INProw("HNIGHTTIMETTL"))
                T0007INProw("HOANTIMECHO") = T0007COM.formatHHMM(T0007INProw("HOANTIMECHO"))
                T0007INProw("HOANTIMETTL") = T0007COM.formatHHMM(T0007INProw("HOANTIMETTL"))
                T0007INProw("KOATUTIMECHO") = T0007COM.formatHHMM(T0007INProw("KOATUTIMECHO"))
                T0007INProw("KOATUTIMETTL") = T0007COM.formatHHMM(T0007INProw("KOATUTIMETTL"))
                T0007INProw("TOKUSA1TIMECHO") = T0007COM.formatHHMM(T0007INProw("TOKUSA1TIMECHO"))
                T0007INProw("TOKUSA1TIMETTL") = T0007COM.formatHHMM(T0007INProw("TOKUSA1TIMETTL"))
                T0007INProw("JIKYUSHATIMECHO") = T0007COM.formatHHMM(T0007INProw("JIKYUSHATIMECHO"))
                T0007INProw("JIKYUSHATIMETTL") = T0007COM.formatHHMM(T0007INProw("JIKYUSHATIMETTL"))
                CODENAME_get("CAMPCODE", T0007INProw("CAMPCODE"), T0007INProw("CAMPNAMES"), WW_DUMMY)
                T0007INProw("STAFFKBNNAMES") = ""
                CODENAME_get("STAFFKBN", T0007INProw("STAFFKBN"), T0007INProw("STAFFKBNNAMES"), WW_DUMMY)
                T0007INProw("MORGNAMES") = ""
                CODENAME_get("ORG", T0007INProw("MORG"), T0007INProw("MORGNAMES"), WW_DUMMY)
                T0007INProw("HORGNAMES") = ""
                CODENAME_get("HORG", T0007INProw("HORG"), T0007INProw("HORGNAMES"), WW_DUMMY)
                T0007INProw("HOLIDAYKBNNAMES") = ""
                CODENAME_get("HOLIDAYKBN", T0007INProw("HOLIDAYKBN"), T0007INProw("HOLIDAYKBNNAMES"), WW_DUMMY)
                T0007INProw("PAYKBNNAMES") = ""
                CODENAME_get("PAYKBN", T0007INProw("PAYKBN"), T0007INProw("PAYKBNNAMES"), WW_DUMMY)
                T0007INProw("SHUKCHOKKBNNAMES") = ""
                CODENAME_get("SHUKCHOKKBN", T0007INProw("SHUKCHOKKBN"), T0007INProw("SHUKCHOKKBNNAMES"), WW_DUMMY)

                WW_UPD_FLG2 = "ON"
                Exit For
            End If
        Next

        '月調整入力の変更取込
        Dim wMODELDISTANCE As Double = 0
        Dim wMODELDISTANCECHO As Double = 0
        For Each T0007INProw As DataRow In T0007INPtbl.Rows

            If T0007INProw("HDKBN") = "D" And T0007INProw("RECODEKBN") = "2" Then
                '単車
                Select Case T0007INProw("OILPAYKBN")
                    Case "04" 'ＬＮＧ
                        Select Case T0007INProw("SHARYOKBN")
                            Case "1" '単車
                                If WF_MODELDISTANCE_LNG1.Text <> T0007INProw("MODELDISTANCE") Then
                                    T0007INProw("MODELDISTANCECHO") = Val(WF_MODELDISTANCE_LNG1.Text) - T0007INProw("MODELDISTANCE")
                                    WW_UPD_FLG2 = "ON"
                                End If
                            Case "2" 'トレーラ
                                If WF_MODELDISTANCE_LNG2.Text <> T0007INProw("MODELDISTANCE") Then
                                    T0007INProw("MODELDISTANCECHO") = Val(WF_MODELDISTANCE_LNG2.Text) - T0007INProw("MODELDISTANCE")
                                    WW_UPD_FLG2 = "ON"
                                End If
                        End Select
                    Case "09" 'ﾗﾃｯｸｽ
                        Select Case T0007INProw("SHARYOKBN")
                            Case "1" '単車
                                If WF_MODELDISTANCE_RATE1.Text <> T0007INProw("MODELDISTANCE") Then
                                    T0007INProw("MODELDISTANCECHO") = Val(WF_MODELDISTANCE_RATE1.Text) - T0007INProw("MODELDISTANCE")
                                    WW_UPD_FLG2 = "ON"
                                End If
                            Case "2" 'トレーラ
                                If WF_MODELDISTANCE_RATE2.Text <> T0007INProw("MODELDISTANCE") Then
                                    T0007INProw("MODELDISTANCECHO") = Val(WF_MODELDISTANCE_RATE2.Text) - T0007INProw("MODELDISTANCE")
                                    WW_UPD_FLG2 = "ON"
                                End If
                                WW_UPD_FLG2 = "ON"
                        End Select
                End Select
                If WW_UPD_FLG2 = "ON" Then
                    T0007INProw("OPERATION") = "更新"
                    T0007INProw("UNLOADCNTTTL") = 0
                    T0007INProw("HAIDISTANCETTL") = 0
                    T0007INProw("MODELDISTANCETTL") = Val(T0007INProw("MODELDISTANCE")) + Val(T0007INProw("MODELDISTANCECHO"))
                End If
                wMODELDISTANCE += T0007INProw("MODELDISTANCE")
                wMODELDISTANCECHO += T0007INProw("MODELDISTANCECHO")
            End If
        Next

        If WW_UPD_FLG2 = "ON" Then
            For Each T0007HEADrow As DataRow In T0007INPtbl.Rows
                'HDKBN（H:ﾍｯﾀﾞﾚｺｰﾄﾞ、D:明細ﾚｺｰﾄﾞ）、RECODEKBN（0:指定日ﾚｺｰﾄﾞ、1:月調整ﾚｺｰﾄﾞ、2:合計ﾚｺｰﾄﾞ）
                If T0007HEADrow("HDKBN") = "H" And T0007HEADrow("RECODEKBN") = "2" Then
                    T0007HEADrow("OPERATION") = "更新"
                    T0007HEADrow("MODELDISTANCE") = wMODELDISTANCE
                    T0007HEADrow("MODELDISTANCECHO") = wMODELDISTANCECHO
                    T0007HEADrow("HAIDISTANCETTL") = wMODELDISTANCE + wMODELDISTANCECHO
                End If
            Next
        End If

        '月調整入力の変更取込
        If WW_UPD_FLG1 = "ON" And WW_UPD_FLG2 = "OFF" Then
            If WW_UPD_TOKUSA = "OFF" Then
                '特作以外の変更だったら
                oRtn = "変更1"
            Else
                '特作が変更されていたら（特作の再計算を行わない）
                oRtn = "変更4"
            End If
        ElseIf WW_UPD_FLG1 = "OFF" And WW_UPD_FLG2 = "ON" Then
            oRtn = "変更2"
        ElseIf WW_UPD_FLG1 = "ON" And WW_UPD_FLG2 = "ON" Then
            oRtn = "変更3"
        End If

        If WW_UPD_FLG2 = "ON" Then
            For Each T0007INProw As DataRow In T0007INPtbl.Rows
                If T0007INProw("RECODEKBN") = "2" Then
                    T0007INProw("OPERATION") = "更新"
                End If
            Next
        End If

    End Sub

    ' ***  入力禁止文字除外
    Protected Sub InpCHARstr()

        WF_CAMPCODE.Text = charStr(WF_CAMPCODE.Text)
        WF_HOLIDAYKBN.Text = charStr(WF_HOLIDAYKBN.Text)
        WF_PAYKBN.Text = charStr(WF_PAYKBN.Text)
        WF_SHUKCHOKKBN.Text = charStr(WF_SHUKCHOKKBN.Text)
        WF_STDATE.Text = charStr(WF_STDATE.Text)
        WF_STTIME.Text = charStr(WF_STTIME.Text)
        WF_ENDDATE.Text = charStr(WF_ENDDATE.Text)
        WF_BINDSTDATE.Text = charStr(WF_BINDSTDATE.Text)
        WF_BINDTIME.Text = charStr(WF_BINDTIME.Text)
        WF_BREAKTIME.Text = charStr(WF_BREAKTIME.Text)
        WF_TOKUSA1TIME.Text = charStr(WF_TOKUSA1TIME.Text)
        '2020/11/17 ADD
        WF_BBSTTIME01.Text = charStr(WF_BBSTTIME01.Text)
        WF_BBENDTIME01.Text = charStr(WF_BBENDTIME01.Text)
        WF_BBSTTIME02.Text = charStr(WF_BBSTTIME02.Text)
        WF_BBENDTIME02.Text = charStr(WF_BBENDTIME02.Text)
        WF_BBSTTIME03.Text = charStr(WF_BBSTTIME03.Text)
        WF_BBENDTIME03.Text = charStr(WF_BBENDTIME03.Text)
        WF_BBSTTIME04.Text = charStr(WF_BBSTTIME04.Text)
        WF_BBENDTIME04.Text = charStr(WF_BBENDTIME04.Text)
        WF_BBSTTIME05.Text = charStr(WF_BBSTTIME05.Text)
        WF_BBENDTIME05.Text = charStr(WF_BBENDTIME05.Text)
        WF_BBSTTIME06.Text = charStr(WF_BBSTTIME06.Text)
        WF_BBENDTIME06.Text = charStr(WF_BBENDTIME06.Text)
        WF_BBSTTIME07.Text = charStr(WF_BBSTTIME07.Text)
        WF_BBENDTIME07.Text = charStr(WF_BBENDTIME07.Text)
        WF_BBSTTIME08.Text = charStr(WF_BBSTTIME08.Text)
        WF_BBENDTIME08.Text = charStr(WF_BBENDTIME08.Text)
        WF_BBSTTIME09.Text = charStr(WF_BBSTTIME09.Text)
        WF_BBENDTIME09.Text = charStr(WF_BBENDTIME09.Text)
        WF_BBSTTIME10.Text = charStr(WF_BBSTTIME10.Text)
        WF_BBENDTIME10.Text = charStr(WF_BBENDTIME10.Text)
        '2020/11/17 ADD END
        WF_SHARYOKBN1.Text = charStr(WF_SHARYOKBN1.Text)
        WF_OILPAYKBN1.Text = charStr(WF_OILPAYKBN1.Text)
        WF_SHUKABASHO1.Text = charStr(WF_SHUKABASHO1.Text)
        WF_TODOKECODE1.Text = charStr(WF_TODOKECODE1.Text)
        WF_MODELDISTANCE1.Text = charStr(WF_MODELDISTANCE1.Text)
        WF_SHARYOKBN2.Text = charStr(WF_SHARYOKBN2.Text)
        WF_OILPAYKBN2.Text = charStr(WF_OILPAYKBN2.Text)
        WF_SHUKABASHO2.Text = charStr(WF_SHUKABASHO2.Text)
        WF_TODOKECODE2.Text = charStr(WF_TODOKECODE2.Text)
        WF_MODELDISTANCE2.Text = charStr(WF_MODELDISTANCE2.Text)
        WF_SHARYOKBN3.Text = charStr(WF_SHARYOKBN3.Text)
        WF_OILPAYKBN3.Text = charStr(WF_OILPAYKBN3.Text)
        WF_SHUKABASHO3.Text = charStr(WF_SHUKABASHO3.Text)
        WF_TODOKECODE3.Text = charStr(WF_TODOKECODE3.Text)
        WF_MODELDISTANCE3.Text = charStr(WF_MODELDISTANCE3.Text)
        WF_SHARYOKBN4.Text = charStr(WF_SHARYOKBN4.Text)
        WF_OILPAYKBN4.Text = charStr(WF_OILPAYKBN4.Text)
        WF_SHUKABASHO4.Text = charStr(WF_SHUKABASHO4.Text)
        WF_TODOKECODE4.Text = charStr(WF_TODOKECODE4.Text)
        WF_MODELDISTANCE4.Text = charStr(WF_MODELDISTANCE4.Text)
        WF_SHARYOKBN5.Text = charStr(WF_SHARYOKBN5.Text)
        WF_OILPAYKBN5.Text = charStr(WF_OILPAYKBN5.Text)
        WF_SHUKABASHO5.Text = charStr(WF_SHUKABASHO5.Text)
        WF_TODOKECODE5.Text = charStr(WF_TODOKECODE5.Text)
        WF_MODELDISTANCE5.Text = charStr(WF_MODELDISTANCE5.Text)
        WF_SHARYOKBN6.Text = charStr(WF_SHARYOKBN6.Text)
        WF_OILPAYKBN6.Text = charStr(WF_OILPAYKBN6.Text)
        WF_SHUKABASHO6.Text = charStr(WF_SHUKABASHO6.Text)
        WF_TODOKECODE6.Text = charStr(WF_TODOKECODE6.Text)
        WF_MODELDISTANCE6.Text = charStr(WF_MODELDISTANCE6.Text)
        WF_WORKNISSUTTL.Text = charStr(WF_WORKNISSUTTL.Text)
        WF_SHOUKETUNISSUTTL.Text = charStr(WF_SHOUKETUNISSUTTL.Text)
        WF_KUMIKETUNISSUTTL.Text = charStr(WF_KUMIKETUNISSUTTL.Text)
        WF_ETCKETUNISSUTTL.Text = charStr(WF_ETCKETUNISSUTTL.Text)
        WF_NENKYUNISSUTTL.Text = charStr(WF_NENKYUNISSUTTL.Text)
        WF_TOKUKYUNISSUTTL.Text = charStr(WF_TOKUKYUNISSUTTL.Text)
        WF_CHIKOKSOTAINISSUTTL.Text = charStr(WF_CHIKOKSOTAINISSUTTL.Text)
        WF_STOCKNISSUTTL.Text = charStr(WF_STOCKNISSUTTL.Text)
        WF_KYOTEIWEEKNISSUTTL.Text = charStr(WF_KYOTEIWEEKNISSUTTL.Text)
        WF_ROSAIYUKYNIUSSUTTL.Text = charStr(WF_ROSAIYUKYNIUSSUTTL.Text)
        WF_TOKUKYUMUKYUNISSUTTL.Text = charStr(WF_TOKUKYUMUKYUNISSUTTL.Text)
        WF_KOKANGOYUKYUNISSUTTL.Text = charStr(WF_KOKANGOYUKYUNISSUTTL.Text)
        WF_KOKANGOMUKYUNISSUTTL.Text = charStr(WF_KOKANGOMUKYUNISSUTTL.Text)
        WF_DAIKYUNISSUTTL.Text = charStr(WF_DAIKYUNISSUTTL.Text)
        WF_NENSHINISSUTTL.Text = charStr(WF_NENSHINISSUTTL.Text)
        WF_ORVERTIMETTL.Text = charStr(WF_ORVERTIMETTL.Text)
        WF_NIGHTTIMETTL.Text = charStr(WF_NIGHTTIMETTL.Text)
        WF_SWORKTIMETTL.Text = charStr(WF_SWORKTIMETTL.Text)
        WF_SNIGHTTIMETTL.Text = charStr(WF_SNIGHTTIMETTL.Text)
        WF_HWORKTIMETTL.Text = charStr(WF_HWORKTIMETTL.Text)
        WF_HNIGHTTIMETTL.Text = charStr(WF_HNIGHTTIMETTL.Text)
        WF_TOKUSA1TIMETTL.Text = charStr(WF_TOKUSA1TIMETTL.Text)
        WF_MODELDISTANCE_RATE1.Text = charStr(WF_MODELDISTANCE_RATE1.Text)
        WF_MODELDISTANCE_RATE2.Text = charStr(WF_MODELDISTANCE_RATE2.Text)
        WF_MODELDISTANCE_LNG1.Text = charStr(WF_MODELDISTANCE_LNG1.Text)
        WF_MODELDISTANCE_LNG2.Text = charStr(WF_MODELDISTANCE_LNG2.Text)
        WF_JIKYUSHATIMETTL.Text = charStr(WF_JIKYUSHATIMETTL.Text)

    End Sub

    ''' <summary>
    ''' 入力禁止文字除外
    ''' </summary>
    Protected Function charStr(ByVal val As String) As String

        Dim retVal As String = ""

        If val <> "" Then
            CS0010CHARstr.CHARIN = val
            CS0010CHARstr.CS0010CHARget()
            retVal = CS0010CHARstr.CHAROUT

        Else
            retVal = val
        End If

        Return retVal

    End Function

    ''' <summary>
    ''' 日報修正ボタン処理
    ''' </summary>
    Protected Sub WF_ButtonNIPPOEDIT_Click()

        '日報修正画面へ遷移
        NIPPO_Screen()

    End Sub

    ' ***  リセットボタン処理
    Protected Sub WF_buttonRESET_click()

        '■■■ テーブルデータ復元 ■■■
        'T0007COM.T0007tbl_ColumnsAdd(T0007INPtbl)
        'If Not Master.RecoverTable(T0007INPtbl, work.WF_T7KIN_XMLsaveF.Text) Then
        '    Exit Sub
        'End If

        '■■■ 前画面（T00007I）テーブルデータ復元 ■■■
        'T0007COM.T0007tbl_ColumnsAdd(T0007tbl)
        If Not Master.RecoverTable(T0007tbl, work.WF_T7I_XMLsaveF.Text) Then
            Exit Sub
        End If

        WF_MODIFYKBN1.Checked = False
        WF_MODIFYKBN2.Checked = False
        WF_MODIFYKBN3.Checked = False
        WF_MODIFYKBN4.Checked = False
        WF_MODIFYKBN5.Checked = False
        WF_MODIFYKBN6.Checked = False

        '----------------------------------------------
        '日報取込チェック
        '----------------------------------------------
        Dim T0005tbl As DataTable = New DataTable
        Dim WW_NIPPOLINKCODE As String = ""
        T00005ALLget("NEW", WF_STAFFCODE.Text, WW_NIPPOLINKCODE, WF_WORKDATE.Text, WF_WORKDATE.Text, T0005tbl, WW_DUMMY)

        Dim WW_MODELtbl As DataTable = New DataTable
        T0007COM.ModelDistanceTbl(T0005tbl, work.WF_T7SEL_CAMPCODE.Text, work.WF_T7SEL_TAISHOYM.Text,
                                  WW_MODELtbl, Master.USERID, Master.USERTERMID)

        For i As Integer = 0 To WW_MODELtbl.Rows.Count - 1
            Dim WW_MODELrow As DataRow = WW_MODELtbl.Rows(i)
            For Each WW_HEADrow In T0007INPtbl.Rows
                If WW_HEADrow("HDKBN") <> "H" Then
                    Continue For
                End If
                If WW_HEADrow("WORKDATE") = WW_MODELrow("WORKDATE") And
                   WW_HEADrow("STAFFCODE") = WW_MODELrow("STAFFCODE") Then
                    WW_HEADrow("T10SAVECNT") = WW_MODELrow("SAVECNT")
                    For j As Integer = 1 To 6
                        Dim WW_SHARYOKBN As String = "SHARYOKBN" & j.ToString
                        Dim WW_OILPAYKBN As String = "OILPAYKBN" & j.ToString
                        Dim WW_SHUKABASHO As String = "SHUKABASHO" & j.ToString
                        Dim WW_TODOKECODE As String = "TODOKECODE" & j.ToString
                        Dim WW_MODELDISTANCE As String = "MODELDISTANCE" & j.ToString
                        Dim WW_MODIFYKBN As String = "MODIFYKBN" & j.ToString

                        Dim WW_T10SHARYOKBN As String = "T10SHARYOKBN" & j.ToString
                        Dim WW_T10OILPAYKBN As String = "T10OILPAYKBN" & j.ToString
                        Dim WW_T10SHUKABASHO As String = "T10SHUKABASHO" & j.ToString
                        Dim WW_T10TODOKECODE As String = "T10TODOKECODE" & j.ToString
                        Dim WW_T10MODELDISTANCE As String = "T10MODELDISTANCE" & j.ToString
                        Dim WW_T10MODIFYKBN As String = "T10MODIFYKBN" & j.ToString

                        WW_HEADrow(WW_T10SHARYOKBN) = WW_MODELrow(WW_SHARYOKBN)
                        WW_HEADrow(WW_T10OILPAYKBN) = WW_MODELrow(WW_OILPAYKBN)
                        WW_HEADrow(WW_T10SHUKABASHO) = WW_MODELrow(WW_SHUKABASHO)
                        WW_HEADrow(WW_T10TODOKECODE) = WW_MODELrow(WW_TODOKECODE)
                        WW_HEADrow(WW_T10MODELDISTANCE) = WW_MODELrow(WW_MODELDISTANCE)
                        WW_HEADrow(WW_T10MODIFYKBN) = WW_MODELrow(WW_MODIFYKBN)
                    Next
                End If
            Next
        Next

        '■■■ 前画面（T00007I）用にテーブルデータ保存 ■■■
        If Not Master.SaveTable(T0007INPtbl, work.WF_T7KIN_XMLsaveF.Text) Then
            Exit Sub
        End If

        'ソート処理
        CS0026TblSort.TABLE = T0007INPtbl
        CS0026TblSort.FILTER = ""
        CS0026TblSort.SORTING = "STAFFCODE, WORKDATE, STDATE, STTIME, ENDDATE, ENDTIME, WORKKBN"
        T0007INPtbl = CS0026TblSort.Sort()

        '画面編集
        DisplayEdit(T0007INPtbl)

    End Sub

    ' ***  モデルチェックボックスＯＦＦ処理
    Protected Sub WF_MODELreset_click()

        '■■■ 前画面（T00007I）テーブルデータ復元 ■■■
        'T0007COM.T0007tbl_ColumnsAdd(T0007tbl)
        If Not Master.RecoverTable(T0007tbl, work.WF_T7I_XMLsaveF.Text) Then
            Exit Sub
        End If

        '----------------------------------------------
        '日報取込チェック
        '----------------------------------------------
        Dim T0005tbl As DataTable = New DataTable
        Dim WW_NIPPOLINKCODE As String = ""
        T00005ALLget("NEW", WF_STAFFCODE.Text, WW_NIPPOLINKCODE, WF_WORKDATE.Text, WF_WORKDATE.Text, T0005tbl, WW_DUMMY)

        Dim WW_MODELtbl As DataTable = New DataTable
        T0007COM.ModelDistanceTbl(T0005tbl, work.WF_T7SEL_CAMPCODE.Text, work.WF_T7SEL_TAISHOYM.Text,
                                  WW_MODELtbl, Master.USERID, Master.USERTERMID)

        '画面のチェックボックスＯＦＦされた行を取得
        Dim WW_rowNo As Integer = WF_MODELrow.Value

        For i As Integer = 0 To WW_MODELtbl.Rows.Count - 1
            Dim WW_MODELrow As DataRow = WW_MODELtbl.Rows(i)
            For Each WW_HEADrow In T0007INPtbl.Rows
                If WW_HEADrow("HDKBN") <> "H" Then
                    Continue For
                End If
                If WW_HEADrow("WORKDATE") = WW_MODELrow("WORKDATE") And
                   WW_HEADrow("STAFFCODE") = WW_MODELrow("STAFFCODE") Then
                    WW_HEADrow("T10SAVECNT") = WW_MODELrow("SAVECNT")
                    Dim WW_SHARYOKBN As String = "SHARYOKBN" & WW_rowNo.ToString
                    Dim WW_OILPAYKBN As String = "OILPAYKBN" & WW_rowNo.ToString
                    Dim WW_SHUKABASHO As String = "SHUKABASHO" & WW_rowNo.ToString
                    Dim WW_TODOKECODE As String = "TODOKECODE" & WW_rowNo.ToString
                    Dim WW_MODELDISTANCE As String = "MODELDISTANCE" & WW_rowNo.ToString
                    Dim WW_MODIFYKBN As String = "MODIFYKBN" & WW_rowNo.ToString

                    Dim WW_T10SHARYOKBN As String = "T10SHARYOKBN" & WW_rowNo.ToString
                    Dim WW_T10OILPAYKBN As String = "T10OILPAYKBN" & WW_rowNo.ToString
                    Dim WW_T10SHUKABASHO As String = "T10SHUKABASHO" & WW_rowNo.ToString
                    Dim WW_T10TODOKECODE As String = "T10TODOKECODE" & WW_rowNo.ToString
                    Dim WW_T10MODELDISTANCE As String = "T10MODELDISTANCE" & WW_rowNo.ToString
                    Dim WW_T10MODIFYKBN As String = "T10MODIFYKBN" & WW_rowNo.ToString

                    WW_HEADrow(WW_T10SHARYOKBN) = WW_MODELrow(WW_SHARYOKBN)
                    WW_HEADrow(WW_T10OILPAYKBN) = WW_MODELrow(WW_OILPAYKBN)
                    WW_HEADrow(WW_T10SHUKABASHO) = WW_MODELrow(WW_SHUKABASHO)
                    WW_HEADrow(WW_T10TODOKECODE) = WW_MODELrow(WW_TODOKECODE)
                    WW_HEADrow(WW_T10MODELDISTANCE) = WW_MODELrow(WW_MODELDISTANCE)
                    WW_HEADrow(WW_T10MODIFYKBN) = WW_MODELrow(WW_MODIFYKBN)
                End If
            Next
        Next

        '■■■ 前画面（T00007I）用にテーブルデータ保存 ■■■
        If Not Master.SaveTable(T0007INPtbl, work.WF_T7KIN_XMLsaveF.Text) Then
            Exit Sub
        End If

        'ソート処理
        CS0026TblSort.TABLE = T0007INPtbl
        CS0026TblSort.FILTER = ""
        CS0026TblSort.SORTING = "STAFFCODE, WORKDATE, STDATE, STTIME, ENDDATE, ENDTIME, WORKKBN"
        T0007INPtbl = CS0026TblSort.Sort()

        '画面編集
        DisplayEdit(T0007INPtbl)

    End Sub

    ''' <summary>
    ''' 更新ボタン処理
    ''' </summary>
    ''' <param name="iPARM"></param>
    Protected Sub WF_ButtonUPDATE_Click(Optional ByVal iPARM As String = "")
        Dim WW_RESULT As String = ""

        rightview.SetErrorReport("")

        '■■■ テーブルデータ復元 ■■■
        'T0007COM.T0007tbl_ColumnsAdd(T0007INPtbl)
        'If Not Master.RecoverTable(T0007INPtbl, work.WF_T7KIN_XMLsaveF.Text) Then
        '    Exit Sub
        'End If

        '■■■ 前画面（T00007I）テーブルデータ復元 ■■■
        'T0007COM.T0007tbl_ColumnsAdd(T0007tbl)
        If Not Master.RecoverTable(T0007tbl, work.WF_T7I_XMLsaveF.Text) Then
            Exit Sub
        End If

        '----------------------------------------------
        '画面項目チェック
        '----------------------------------------------
        '入力禁止文字除外
        InpCHARstr()

        '項目チェック
        T0007INProw_CHEK(WW_RESULT)
        If WW_RESULT <> C_MESSAGE_NO.NORMAL Then
            Master.Output(WW_RESULT, C_MESSAGE_TYPE.ABORT)
            Exit Sub
        End If

        '関連チェック
        T0007INProw_KANREN_CHEK(WW_RESULT)
        If WW_RESULT <> C_MESSAGE_NO.NORMAL Then
            Master.Output(WW_RESULT, C_MESSAGE_TYPE.ABORT)
            Exit Sub
        End If

        '項目変更チェック
        ItemChangeCheck(WW_RESULT)

        '指定日入力画面に変更があった場合、残業計算を行う
        If WW_RESULT = "変更1" OrElse WW_RESULT = "変更4" Then

            '----------------------------------------------
            '残業計算
            '----------------------------------------------
            T0007COM.T0007_KintaiCalc_NJS(T0007INPtbl, T0007tbl)

        End If

        '画面項目チェック＆更新判定
        InDataUpdate()

        '重複チェック
        Dim WW_MSG As String = ""
        Dim WW_ERR_MES As String = ""
        T0007COM.T0007_DuplCheck(T0007tbl, WW_MSG, WW_ERRCODE)
        If WW_ERRCODE <> C_MESSAGE_NO.NORMAL Then

            WW_ERR_MES = "内部処理エラー" & ControlChars.NewLine & WW_ERR_MES

            rightview.AddErrorReport(WW_ERR_MES)

            CS0011LOGWRITE.INFSUBCLASS = "T0007_DuplCheck"             'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "T0007_DuplCheck"                 '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                 '
            CS0011LOGWRITE.TEXT = WW_ERR_MES
            CS0011LOGWRITE.MESSAGENO = WW_ERRCODE
            CS0011LOGWRITE.CS0011LOGWrite()                            'ログ出力

            Master.Output(WW_ERRCODE, C_MESSAGE_TYPE.ABORT)

            Exit Sub
        End If

        If iPARM = "MDL" Then
            '個別画面に戻る
            WF_DTABChange.Value = 0
            WF_Detail_TABChange()

            'ソート処理
            CS0026TblSort.TABLE = T0007INPtbl
            CS0026TblSort.FILTER = ""
            CS0026TblSort.SORTING = "STAFFCODE, WORKDATE, STDATE, STTIME, ENDDATE, ENDTIME, WORKKBN"
            T0007INPtbl = CS0026TblSort.Sort()

            '画面編集
            DisplayEdit(T0007INPtbl)

            Master.Output(C_MESSAGE_NO.NORMAL, C_MESSAGE_TYPE.NOR)
        Else
            '終了処理
            WF_ButtonEND_Click()
        End If
    End Sub

    ' ***  前頁ボタン処理                                                        ***
    Protected Sub WF_ButtonDOWN_Click()
        Dim WW_RESULT As String = ""

        rightview.SetErrorReport("")

        '■■■ テーブルデータ復元 ■■■
        'T0007COM.T0007tbl_ColumnsAdd(T0007INPtbl)
        'If Not Master.RecoverTable(T0007INPtbl, work.WF_T7KIN_XMLsaveF.Text) Then
        '    Exit Sub
        'End If

        '■■■ 前画面（T00007I）テーブルデータ復元 ■■■
        'T0007COM.T0007tbl_ColumnsAdd(T0007tbl)
        If Not Master.RecoverTable(T0007tbl, work.WF_T7I_XMLsaveF.Text) Then
            Exit Sub
        End If

        '----------------------------------------------
        '画面項目チェック
        '----------------------------------------------
        '入力禁止文字除外
        InpCHARstr()

        '項目チェック
        T0007INProw_CHEK(WW_RESULT)
        If WW_RESULT <> C_MESSAGE_NO.NORMAL Then
            Master.Output(WW_RESULT, C_MESSAGE_TYPE.ABORT)
            Exit Sub
        End If

        '項目変更チェック
        ItemChangeCheck(WW_RESULT)

        '指定日入力画面に変更があった場合、残業計算を行う
        If WW_RESULT = "変更1" OrElse WW_RESULT = "変更4" Then

            '----------------------------------------------
            '残業計算
            '----------------------------------------------
            T0007COM.T0007_KintaiCalc_NJS(T0007INPtbl, T0007tbl)

        End If

        '画面入力データ反映
        InDataUpdate()

        '次のデータ
        For i As Integer = T0007tbl.Rows.Count - 1 To 0 Step -1
            Dim T0007row As DataRow = T0007tbl.Rows(i)
            If T0007row("LINECNT") < work.WF_T7KIN_LINECNT.Text Then
                If T0007row("RECODEKBN") = "1" Then '月調整レコード
                    Continue For
                End If
                If T0007row("HIDDEN") = "1" Then '非表示
                    Continue For
                End If
                If WF_DetailMView.ActiveViewIndex = 0 Then
                    If T0007row("RECODEKBN") = "0" Then '日別レコード
                    Else
                        Continue For
                    End If
                End If
                If WF_DetailMView.ActiveViewIndex = 1 Then
                    If T0007row("RECODEKBN") = "2" Then '月合計レコード
                    Else
                        Continue For
                    End If
                End If
                work.WF_T7KIN_LINECNT.Text = T0007row("LINECNT")
                work.WF_T7KIN_WORKDATE.Text = T0007row("WORKDATE")
                work.WF_T7KIN_STAFFCODE.Text = T0007row("STAFFCODE")
                work.WF_T7KIN_RECODEKBN.Text = T0007row("RECODEKBN")
                Exit For
            End If
        Next

        '画面表示
        GRID_INITset()

    End Sub

    ' ***  次頁ボタン処理                                                        ***
    Protected Sub WF_ButtonUP_Click()
        Dim WW_RESULT As String = ""

        rightview.SetErrorReport("")

        '■■■ テーブルデータ復元 ■■■
        'T0007COM.T0007tbl_ColumnsAdd(T0007INPtbl)
        'If Not Master.RecoverTable(T0007INPtbl, work.WF_T7KIN_XMLsaveF.Text) Then
        '    Exit Sub
        'End If

        ''■■■ 前画面（T00007I）テーブルデータ復元 ■■■
        'T0007COM.T0007tbl_ColumnsAdd(T0007tbl)
        If Not Master.RecoverTable(T0007tbl, work.WF_T7I_XMLsaveF.Text) Then
            Exit Sub
        End If

        '----------------------------------------------
        '画面項目チェック
        '----------------------------------------------
        '入力禁止文字除外
        InpCHARstr()

        '項目チェック
        T0007INProw_CHEK(WW_RESULT)
        If WW_RESULT <> C_MESSAGE_NO.NORMAL Then
            Master.Output(WW_RESULT, C_MESSAGE_TYPE.ABORT)
            Exit Sub
        End If

        '項目変更チェック
        ItemChangeCheck(WW_RESULT)

        '指定日入力画面に変更があった場合、残業計算を行う
        If WW_RESULT = "変更1" OrElse WW_RESULT = "変更4" Then

            '----------------------------------------------
            '残業計算
            '----------------------------------------------
            T0007COM.T0007_KintaiCalc_NJS(T0007INPtbl, T0007tbl)

        End If

        '画面入力データ反映
        InDataUpdate()

        '次のデータ
        For Each T0007row As DataRow In T0007tbl.Rows
            If T0007row("LINECNT") > work.WF_T7KIN_LINECNT.Text Then
                If T0007row("RECODEKBN") = "1" Then '月調整レコード
                    Continue For
                End If
                If T0007row("HIDDEN") = "1" Then '非表示
                    Continue For
                End If
                If WF_DetailMView.ActiveViewIndex = 0 Then
                    If T0007row("RECODEKBN") = "0" Then '日別レコード
                    Else
                        Continue For
                    End If
                End If
                If WF_DetailMView.ActiveViewIndex = 1 Then
                    If T0007row("RECODEKBN") = "2" Then '月合計レコード
                    Else
                        Continue For
                    End If
                End If
                work.WF_T7KIN_LINECNT.Text = T0007row("LINECNT")
                work.WF_T7KIN_WORKDATE.Text = T0007row("WORKDATE")
                work.WF_T7KIN_STAFFCODE.Text = T0007row("STAFFCODE")
                work.WF_T7KIN_RECODEKBN.Text = T0007row("RECODEKBN")
                Exit For
            End If
        Next

        '画面表示
        GRID_INITset()

    End Sub

    ''' <summary>
    ''' 日報一括取込ボタン処理
    ''' </summary>
    Protected Sub WF_ButtonNIPPO_Click()

        Dim WW_RESULT As String = ""

        rightview.SetErrorReport("")

        '■■■ テーブルデータ復元 ■■■
        'T0007COM.T0007tbl_ColumnsAdd(T0007INPtbl)
        'If Not Master.RecoverTable(T0007INPtbl, work.WF_T7KIN_XMLsaveF.Text) Then
        '    Exit Sub
        'End If

        '■■■ 前画面（T00007I）テーブルデータ復元 ■■■
        'T0007COM.T0007tbl_ColumnsAdd(T0007tbl)
        If Not Master.RecoverTable(T0007tbl, work.WF_T7I_XMLsaveF.Text) Then
            Exit Sub
        End If

        '----------------------------------------------
        '画面項目チェック
        '----------------------------------------------
        '入力禁止文字除外
        InpCHARstr()

        '明細を削除し、新たに日報から明細を作成
        CS0026TblSort.TABLE = T0007INPtbl
        CS0026TblSort.FILTER = ""
        CS0026TblSort.SORTING = "HDKBN DESC"
        T0007INPtbl = CS0026TblSort.Sort()

        For i As Integer = T0007INPtbl.Rows.Count - 1 To 0 Step -1
            Dim T7row As DataRow = T0007INPtbl.Rows(i)
            If T7row("HDKBN") = "H" Then
                T7row("STATUS") = "日報取込"
            End If
            If T7row("HDKBN") = "D" Then
                T7row.Delete()
            End If
        Next

        '----------------------------------------------
        '日報取込チェック
        '----------------------------------------------
        Dim T0005tbl As DataTable = New DataTable
        Dim WW_NIPPOLINKCODE As String = ""
        T00005ALLget("NEW", WF_STAFFCODE.Text, WW_NIPPOLINKCODE, WF_WORKDATE.Text, WF_WORKDATE.Text, T0005tbl, WW_DUMMY)

        Dim WW_MODELtbl As DataTable = New DataTable
        T0007COM.ModelDistanceTbl(T0005tbl, work.WF_T7SEL_CAMPCODE.Text, work.WF_T7SEL_TAISHOYM.Text,
                                  WW_MODELtbl, Master.USERID, Master.USERTERMID)
        '2020/11/17 ADD
        '休憩・配送時間のリセット（一覧画面データから取り直し）
        TimeManageGet(T0007tbl, T0007INPtbl)
        '2020/11/17 ADD END

        '------------------------------------------------------------------
        '日報を取得し、作業（始業、終業、休憩、その他）レコード作成
        '------------------------------------------------------------------
        CreWORKKBN(T0007INPtbl, T0005tbl, WF_WORKDATE.Text, WF_WORKDATE.Text)

        '--------------------------------------------
        'ヘッダ編集
        '--------------------------------------------
        HeadEdit(T0007INPtbl, T0005tbl, WF_WORKDATE.Text, WF_WORKDATE.Text, WW_MODELtbl)

        '--------------------------------------------
        '拘束開始編集（日報有の分）
        '--------------------------------------------
        BindStDateSet(T0007INPtbl, T0007tbl, WF_WORKDATE.Text, WF_WORKDATE.Text)

        '項目チェック
        T0007INProw_CHEK(WW_RESULT)
        If WW_RESULT <> C_MESSAGE_NO.NORMAL Then
            Master.Output(WW_RESULT, C_MESSAGE_TYPE.ABORT)
            Exit Sub
        End If

        NIPPOget_T7Format(T0007INPtbl, T0005tbl, WF_WORKDATE.Text, WF_WORKDATE.Text)

        '----------------------------------------------
        '残業計算
        '----------------------------------------------
        T0007COM.T0007_KintaiCalc_NJS(T0007INPtbl, T0007tbl, "TOKUSA")

        '名称設定
        For Each WW_T0007row As DataRow In T0007INPtbl.Rows
            WW_T0007row("TIMSTP") = "0"
            WW_T0007row("OPERATION") = "更新"
            WW_T0007row("STATUS") = ""
            WW_T0007row("CAMPNAMES") = ""
            CODENAME_get("CAMPCODE", WW_T0007row("CAMPCODE"), WW_T0007row("CAMPNAMES"), WW_DUMMY)
            WW_T0007row("WORKKBNNAMES") = ""
            CODENAME_get("WORKKBN", WW_T0007row("WORKKBN"), WW_T0007row("WORKKBNNAMES"), WW_DUMMY)
            WW_T0007row("STAFFNAMES") = ""
            CODENAME_get("STAFFCODE", WW_T0007row("STAFFCODE"), WW_T0007row("STAFFNAMES"), WW_DUMMY)
            WW_T0007row("HOLIDAYKBNNAMES") = ""
            CODENAME_get("HOLIDAYKBN", WW_T0007row("HOLIDAYKBN"), WW_T0007row("HOLIDAYKBNNAMES"), WW_DUMMY)
            WW_T0007row("PAYKBNNAMES") = ""
            CODENAME_get("PAYKBN", WW_T0007row("PAYKBN"), WW_T0007row("PAYKBNNAMES"), WW_DUMMY)
            WW_T0007row("SHUKCHOKKBNNAMES") = ""
            CODENAME_get("SHUKCHOKKBN", WW_T0007row("SHUKCHOKKBN"), WW_T0007row("SHUKCHOKKBNNAMES"), WW_DUMMY)
            WW_T0007row("MORGNAMES") = ""
            CODENAME_get("ORG", WW_T0007row("MORG"), WW_T0007row("MORGNAMES"), WW_DUMMY)
            WW_T0007row("HORGNAMES") = ""
            CODENAME_get("ORG", WW_T0007row("HORG"), WW_T0007row("HORGNAMES"), WW_DUMMY)
            WW_T0007row("SORGNAMES") = ""
            CODENAME_get("ORG", WW_T0007row("SORG"), WW_T0007row("SORGNAMES"), WW_DUMMY)
        Next

        'ソート処理
        CS0026TblSort.TABLE = T0007INPtbl
        CS0026TblSort.FILTER = ""
        CS0026TblSort.SORTING = "STAFFCODE, WORKDATE, STDATE, STTIME, ENDDATE, ENDTIME, WORKKBN"
        T0007INPtbl = CS0026TblSort.Sort()

        '画面編集
        DisplayEdit(T0007INPtbl)

        If Not Master.SaveTable(T0007INPtbl, work.WF_T7KIN_XMLsaveF.Text) Then
            Exit Sub
        End If

        'モデル距離再取得を取りやめ（終了ボタン）場合に使用する
        If Not Master.SaveTable(T0007INPtbl, work.WF_T7KIN_XMLsaveF2.Text) Then
            Exit Sub
        End If

        Master.Output(C_MESSAGE_NO.NORMAL, C_MESSAGE_TYPE.NOR)

        T0005tbl.Dispose()
        T0005tbl = Nothing

    End Sub

    ''' <summary>
    ''' 終了ボタン処理
    ''' </summary>
    ''' <param name="iPARM"></param>
    Protected Sub WF_ButtonEND_Click(Optional ByVal iPARM As String = "")

        If iPARM = "MDL" Then

            '■■■ テーブルデータ復元 ■■■
            'T0007COM.T0007tbl_ColumnsAdd(T0007INPtbl)
            If Not Master.RecoverTable(T0007INPtbl, work.WF_T7KIN_XMLsaveF2.Text) Then
                Exit Sub
            End If

            '■■■ 画面（GridView）表示データ保存 ■■■
            If Not Master.SaveTable(T0007INPtbl, work.WF_T7KIN_XMLsaveF.Text) Then
                Exit Sub
            End If

            WF_DTABChange.Value = 0
            WF_Detail_TABChange()

            'ソート処理
            CS0026TblSort.TABLE = T0007INPtbl
            CS0026TblSort.FILTER = ""
            CS0026TblSort.SORTING = "STAFFCODE, WORKDATE, STDATE, STTIME, ENDDATE, ENDTIME, WORKKBN"
            T0007INPtbl = CS0026TblSort.Sort()

            '画面編集
            DisplayEdit(T0007INPtbl)

            rightview.SetErrorReport("")

            Master.Output(C_MESSAGE_NO.NORMAL, C_MESSAGE_TYPE.NOR)

        Else

            Master.TransitionPrevPage(VER:="V2")
        End If

    End Sub

    ' ***  leftBOX選択ボタン処理(ListBox値 ---> detailbox)　　　                 
    Protected Sub WF_ButtonSel_Click()

        Dim WW_SelectValues As String() = Nothing
        Dim WW_ERR_REPORT As String = ""
        Dim WW_ERR_MES As String = ""

        '○ 選択内容を取得
        If Not IsNothing(leftview.GetActiveValue) Then
            WW_SelectValues = leftview.GetActiveValue
        End If

        '○ 選択内容を画面項目へセット
        Select Case WF_FIELD.Value
            Case "WF_STAFFCODE"
                '従業員
                WF_STAFFCODE_TEXT.Text = WW_SelectValues(1)
                WF_STAFFCODE.Text = WW_SelectValues(0)
                WF_STAFFCODE.Focus()
            Case "WF_HOLIDAYKBN"
                '休日区分 
                If WF_HOLIDAYKBN.Text <> WW_SelectValues(0) Then
                    If WF_WORKINGWEEK_TEXT.Text = "日" AndAlso WW_SelectValues(0) <> "1" Then

                        WW_ERR_MES = "日曜日は法定休日です。"
                        WW_ERR_REPORT = "内部処理エラー" & ControlChars.NewLine & WW_ERR_MES

                        rightview.AddErrorReport(WW_ERR_MES)

                        '○ 画面左サイドボックス非表示は、画面JavaScript(InitLoad)で実行
                        WF_LeftboxOpen.Value = ""
                        Exit Sub
                    End If
                    If WF_WORKINGWEEK_TEXT.Text <> "日" AndAlso WW_SelectValues(0) = "1" Then

                        WW_ERR_MES = "法定休日は日曜日だけです。"
                        WW_ERR_REPORT = "内部処理エラー" & ControlChars.NewLine & WW_ERR_MES

                        rightview.AddErrorReport(WW_ERR_MES)

                        '○ 画面左サイドボックス非表示は、画面JavaScript(InitLoad)で実行
                        WF_LeftboxOpen.Value = ""
                        Exit Sub
                    End If
                    WF_HOLIDAYKBN.Text = WW_SelectValues(0)
                    WF_HOLIDAYKBN_TEXT.Text = WW_SelectValues(1)
                    '残業計算
                    WF_FIELD_Change()
                End If
                WF_HOLIDAYKBN.Focus()
            Case "WF_PAYKBN"
                '勤怠区分 
                If WF_PAYKBN.Text <> WW_SelectValues(0) Then
                    WF_PAYKBN.Text = WW_SelectValues(0)
                    WF_PAYKBN_TEXT.Text = WW_SelectValues(1)
                    '残業計算
                    WF_FIELD_Change()
                End If
                WF_PAYKBN.Focus()
            Case "WF_SHUKCHOKKBN"
                '宿日直区分 
                If WF_SHUKCHOKKBN.Text <> WW_SelectValues(0) Then
                    WF_SHUKCHOKKBN.Text = WW_SelectValues(0)
                    WF_SHUKCHOKKBN_TEXT.Text = WW_SelectValues(1)
                    '残業計算
                    WF_FIELD_Change()
                End If
                WF_SHUKCHOKKBN.Focus()
            Case "WF_SHARYOKBN1"
                '車輌区分 
                If WF_SHARYOKBN1.Text <> WW_SelectValues(0) Then
                    WF_SHARYOKBN1.Text = WW_SelectValues(0)
                    WF_SHARYOKBN1_TEXT.Text = WW_SelectValues(1)
                    '残業計算
                    WF_FIELD_Change()
                End If
                WF_SHARYOKBN1.Focus()
            Case "WF_SHARYOKBN2"
                '車輌区分 
                If WF_SHARYOKBN2.Text <> WW_SelectValues(0) Then
                    WF_SHARYOKBN2.Text = WW_SelectValues(0)
                    WF_SHARYOKBN2_TEXT.Text = WW_SelectValues(1)
                    '残業計算
                    WF_FIELD_Change()
                End If
                WF_SHARYOKBN2.Focus()
            Case "WF_SHARYOKBN3"
                '車輌区分 
                If WF_SHARYOKBN3.Text <> WW_SelectValues(0) Then
                    WF_SHARYOKBN3.Text = WW_SelectValues(0)
                    WF_SHARYOKBN3_TEXT.Text = WW_SelectValues(1)
                    '残業計算
                    WF_FIELD_Change()
                End If
                WF_SHARYOKBN3.Focus()
            Case "WF_SHARYOKBN4"
                '車輌区分 
                If WF_SHARYOKBN4.Text <> WW_SelectValues(0) Then
                    WF_SHARYOKBN4.Text = WW_SelectValues(0)
                    WF_SHARYOKBN4_TEXT.Text = WW_SelectValues(1)
                    '残業計算
                    WF_FIELD_Change()
                End If
                WF_SHARYOKBN4.Focus()
            Case "WF_SHARYOKBN5"
                '車輌区分 
                If WF_SHARYOKBN5.Text <> WW_SelectValues(0) Then
                    WF_SHARYOKBN5.Text = WW_SelectValues(0)
                    WF_SHARYOKBN5_TEXT.Text = WW_SelectValues(1)
                    '残業計算
                    WF_FIELD_Change()
                End If
                WF_SHARYOKBN5.Focus()
            Case "WF_SHARYOKBN6"
                '車輌区分 
                If WF_SHARYOKBN6.Text <> WW_SelectValues(0) Then
                    WF_SHARYOKBN6.Text = WW_SelectValues(0)
                    WF_SHARYOKBN6_TEXT.Text = WW_SelectValues(1)
                    '残業計算
                    WF_FIELD_Change()
                End If
                WF_SHARYOKBN6.Focus()
            Case "WF_OILPAYKBN1"
                '油種区分 
                If WF_OILPAYKBN1.Text <> WW_SelectValues(0) Then
                    WF_OILPAYKBN1.Text = WW_SelectValues(0)
                    WF_OILPAYKBN1_TEXT.Text = WW_SelectValues(1)
                    '残業計算
                    WF_FIELD_Change()
                End If
                WF_OILPAYKBN1.Focus()
            Case "WF_OILPAYKBN2"
                '油種区分 
                If WF_OILPAYKBN2.Text <> WW_SelectValues(0) Then
                    WF_OILPAYKBN2.Text = WW_SelectValues(0)
                    WF_OILPAYKBN2_TEXT.Text = WW_SelectValues(1)
                    '残業計算
                    WF_FIELD_Change()
                End If
                WF_OILPAYKBN2.Focus()
            Case "WF_OILPAYKBN3"
                '油種区分 
                If WF_OILPAYKBN3.Text <> WW_SelectValues(0) Then
                    WF_OILPAYKBN3.Text = WW_SelectValues(0)
                    WF_OILPAYKBN3_TEXT.Text = WW_SelectValues(1)
                    '残業計算
                    WF_FIELD_Change()
                End If
                WF_OILPAYKBN3.Focus()
            Case "WF_OILPAYKBN4"
                '油種区分 
                If WF_OILPAYKBN4.Text <> WW_SelectValues(0) Then
                    WF_OILPAYKBN4.Text = WW_SelectValues(0)
                    WF_OILPAYKBN4_TEXT.Text = WW_SelectValues(1)
                    '残業計算
                    WF_FIELD_Change()
                End If
                WF_OILPAYKBN4.Focus()
            Case "WF_OILPAYKBN5"
                '油種区分 
                If WF_OILPAYKBN5.Text <> WW_SelectValues(0) Then
                    WF_OILPAYKBN5.Text = WW_SelectValues(0)
                    WF_OILPAYKBN5_TEXT.Text = WW_SelectValues(1)
                    '残業計算
                    WF_FIELD_Change()
                End If
                WF_OILPAYKBN5.Focus()
            Case "WF_OILPAYKBN6"
                '油種区分 
                If WF_OILPAYKBN6.Text <> WW_SelectValues(0) Then
                    WF_OILPAYKBN6.Text = WW_SelectValues(0)
                    WF_OILPAYKBN6_TEXT.Text = WW_SelectValues(1)
                    '残業計算
                    WF_FIELD_Change()
                End If
                WF_OILPAYKBN6.Focus()
            Case "WF_SHUKABASHO1"
                '出荷場所 
                If WF_SHUKABASHO1.Text <> WW_SelectValues(0) Then
                    WF_SHUKABASHO1.Text = WW_SelectValues(0)
                    WF_SHUKABASHO1_TEXT.Text = WW_SelectValues(1)
                    MODELget(WF_SHUKABASHO1.Text, WF_TODOKECODE1.Text, WF_MODELDISTANCE1.Text, WW_DUMMY)
                    '残業計算
                    WF_FIELD_Change()
                End If
                WF_SHUKABASHO1.Focus()
            Case "WF_SHUKABASHO2"
                '出荷場所 
                If WF_SHUKABASHO2.Text <> WW_SelectValues(0) Then
                    WF_SHUKABASHO2.Text = WW_SelectValues(0)
                    WF_SHUKABASHO2_TEXT.Text = WW_SelectValues(1)
                    MODELget(WF_SHUKABASHO2.Text, WF_TODOKECODE2.Text, WF_MODELDISTANCE2.Text, WW_DUMMY)
                    '残業計算
                    WF_FIELD_Change()
                End If
                WF_SHUKABASHO2.Focus()
            Case "WF_SHUKABASHO3"
                '出荷場所 
                If WF_SHUKABASHO3.Text <> WW_SelectValues(0) Then
                    WF_SHUKABASHO3.Text = WW_SelectValues(0)
                    WF_SHUKABASHO3_TEXT.Text = WW_SelectValues(1)
                    MODELget(WF_SHUKABASHO3.Text, WF_TODOKECODE3.Text, WF_MODELDISTANCE3.Text, WW_DUMMY)
                    '残業計算
                    WF_FIELD_Change()
                End If
                WF_SHUKABASHO3.Focus()
            Case "WF_SHUKABASHO4"
                '出荷場所 
                If WF_SHUKABASHO4.Text <> WW_SelectValues(0) Then
                    WF_SHUKABASHO4.Text = WW_SelectValues(0)
                    WF_SHUKABASHO4_TEXT.Text = WW_SelectValues(1)
                    MODELget(WF_SHUKABASHO4.Text, WF_TODOKECODE4.Text, WF_MODELDISTANCE4.Text, WW_DUMMY)
                    '残業計算
                    WF_FIELD_Change()
                End If
                WF_SHUKABASHO4.Focus()
            Case "WF_SHUKABASHO5"
                '出荷場所 
                If WF_SHUKABASHO5.Text <> WW_SelectValues(0) Then
                    WF_SHUKABASHO5.Text = WW_SelectValues(0)
                    WF_SHUKABASHO5_TEXT.Text = WW_SelectValues(1)
                    MODELget(WF_SHUKABASHO5.Text, WF_TODOKECODE5.Text, WF_MODELDISTANCE5.Text, WW_DUMMY)
                    '残業計算
                    WF_FIELD_Change()
                End If
                WF_SHUKABASHO5.Focus()
            Case "WF_SHUKABASHO6"
                '出荷場所 
                If WF_SHUKABASHO6.Text <> WW_SelectValues(0) Then
                    WF_SHUKABASHO6.Text = WW_SelectValues(0)
                    WF_SHUKABASHO6_TEXT.Text = WW_SelectValues(1)
                    MODELget(WF_SHUKABASHO6.Text, WF_TODOKECODE6.Text, WF_MODELDISTANCE6.Text, WW_DUMMY)
                    '残業計算
                    WF_FIELD_Change()
                End If
                WF_SHUKABASHO6.Focus()
            Case "WF_TODOKECODE1"
                '届先 
                If WF_TODOKECODE1.Text <> WW_SelectValues(0) Then
                    WF_TODOKECODE1.Text = WW_SelectValues(0)
                    WF_TODOKECODE1_TEXT.Text = WW_SelectValues(1)
                    MODELget(WF_SHUKABASHO1.Text, WF_TODOKECODE1.Text, WF_MODELDISTANCE1.Text, WW_DUMMY)
                    '残業計算
                    WF_FIELD_Change()
                End If
                WF_TODOKECODE1.Focus()
            Case "WF_TODOKECODE2"
                '届先 
                If WF_TODOKECODE2.Text <> WW_SelectValues(0) Then
                    WF_TODOKECODE2.Text = WW_SelectValues(0)
                    WF_TODOKECODE2_TEXT.Text = WW_SelectValues(1)
                    MODELget(WF_SHUKABASHO2.Text, WF_TODOKECODE2.Text, WF_MODELDISTANCE2.Text, WW_DUMMY)
                    '残業計算
                    WF_FIELD_Change()
                End If
                WF_TODOKECODE2.Focus()
            Case "WF_TODOKECODE3"
                '届先 
                If WF_TODOKECODE3.Text <> WW_SelectValues(0) Then
                    WF_TODOKECODE3.Text = WW_SelectValues(0)
                    WF_TODOKECODE3_TEXT.Text = WW_SelectValues(1)
                    MODELget(WF_SHUKABASHO3.Text, WF_TODOKECODE3.Text, WF_MODELDISTANCE3.Text, WW_DUMMY)
                    '残業計算
                    WF_FIELD_Change()
                End If
                WF_TODOKECODE3.Focus()
            Case "WF_TODOKECODE4"
                '届先 
                If WF_TODOKECODE4.Text <> WW_SelectValues(0) Then
                    WF_TODOKECODE4.Text = WW_SelectValues(0)
                    WF_TODOKECODE4_TEXT.Text = WW_SelectValues(1)
                    MODELget(WF_SHUKABASHO4.Text, WF_TODOKECODE4.Text, WF_MODELDISTANCE4.Text, WW_DUMMY)
                    '残業計算
                    WF_FIELD_Change()
                End If
                WF_TODOKECODE4.Focus()
            Case "WF_TODOKECODE5"
                '届先 
                If WF_TODOKECODE5.Text <> WW_SelectValues(0) Then
                    WF_TODOKECODE5.Text = WW_SelectValues(0)
                    WF_TODOKECODE5_TEXT.Text = WW_SelectValues(1)
                    MODELget(WF_SHUKABASHO5.Text, WF_TODOKECODE5.Text, WF_MODELDISTANCE5.Text, WW_DUMMY)
                    '残業計算
                    WF_FIELD_Change()
                End If
                WF_TODOKECODE5.Focus()
            Case "WF_TODOKECODE6"
                '届先 
                If WF_TODOKECODE6.Text <> WW_SelectValues(0) Then
                    WF_TODOKECODE6.Text = WW_SelectValues(0)
                    WF_TODOKECODE6_TEXT.Text = WW_SelectValues(1)
                    MODELget(WF_SHUKABASHO6.Text, WF_TODOKECODE6.Text, WF_MODELDISTANCE6.Text, WW_DUMMY)
                    '残業計算
                    WF_FIELD_Change()
                End If
                WF_TODOKECODE6.Focus()
        End Select

        '○ 画面左サイドボックス非表示は、画面JavaScript(InitLoad)で実行
        WF_FIELD.Value = ""
        WF_LeftboxOpen.Value = ""
        WF_LeftMViewChange.Value = ""

    End Sub

    ' ***  leftBOXキャンセルボタン処理　　　                                     
    Protected Sub WF_ButtonCan_Click()

        '○ フォーカスセット
        Select Case WF_FIELD.Value
            Case "WF_PAYKBN"
                '勤怠区分
                WF_PAYKBN.Focus()
            Case "WF_SHUKCHOKKBN"
                '宿直区分
                WF_SHUKCHOKKBN.Focus()
        End Select

        '○ 画面左サイドボックス非表示は、画面JavaScript(InitLoad)で実行
        WF_FIELD.Value = ""
        WF_LeftboxOpen.Value = ""
        WF_LeftMViewChange.Value = ""

    End Sub

    ' ***  休憩不足ボタン押下時処理                                     ***
    Protected Sub WF_ButtonBREAKTIME_Click()

        If T0007COM.HHMMtoMinutes(WF_NIPPOBREAKTIME.Text) < 60 Then
            WF_BREAKTIME.Text = T0007COM.formatHHMM(60 - T0007COM.HHMMtoMinutes(WF_NIPPOBREAKTIME.Text))
        End If

        WF_FIELD_Change()
    End Sub

    ''' <summary>
    ''' データ更新処理
    ''' </summary>
    Protected Sub InDataUpdate()
        Dim WW_RESULT As String = ""

        Dim WW_UPD_FLG As String = "OFF"

        For Each T0007INProw As DataRow In T0007INPtbl.Rows
            If T0007INProw("HDKBN") = "H" And T0007INProw("OPERATION") = "更新" Then
                For Each T0007DTLrow As DataRow In T0007INPtbl.Rows
                    If T0007DTLrow("HDKBN") = "D" Then
                        T0007DTLrow("OPERATION") = T0007INProw("OPERATION")
                        T0007DTLrow("STATUS") = ""
                    End If
                Next
            End If
        Next

        CS0026TblSort.TABLE = T0007INPtbl
        CS0026TblSort.FILTER = ""
        CS0026TblSort.SORTING = "STAFFCODE, WORKDATE, RECODEKBN"
        T0007INPtbl = CS0026TblSort.Sort()

        CS0026TblSort.TABLE = T0007tbl
        CS0026TblSort.FILTER = ""
        CS0026TblSort.SORTING = "STAFFCODE, WORKDATE, RECODEKBN"
        T0007tbl = CS0026TblSort.Sort()

        Dim WW_YESTERDAYEDIT As String = "無"
        Dim WW_IDX As Integer = 0
        Dim WW_UPD_CNT As Integer = 0
        Dim WW_KEYINP As String = ""
        Dim WW_KEYTBL As String = ""
        For Each T0007INProw As DataRow In T0007INPtbl.Rows
            WW_KEYINP = T0007INProw("STAFFCODE") & T0007INProw("WORKDATE") & T0007INProw("RECODEKBN")

            If T0007INProw("OPERATION") = "更新" And T0007INProw("HDKBN") = "H" Then
                For i As Integer = WW_IDX To T0007tbl.Rows.Count - 1
                    Dim T0007row As DataRow = T0007tbl.Rows(i)
                    WW_KEYTBL = T0007row("STAFFCODE") & T0007row("WORKDATE") & T0007row("RECODEKBN")

                    If WW_KEYTBL < WW_KEYINP Then
                        Continue For
                    End If

                    If WW_KEYTBL = WW_KEYINP Then

                        'If T0007row("SELECT") = "1" Then
                        '    If T0007row("PAYKBN") <> T0007INProw("PAYKBN") Then
                        '        If T0007COM.CheckHOLIDAY("0", T0007INProw("PAYKBN")) Then
                        '            If T0007COM.CheckHOLIDAY("0", T0007row("PAYKBN")) Then
                        '            Else
                        '                WW_YESTERDAYEDIT = "有"
                        '            End If
                        '        End If
                        '    End If

                        '    If T0007COM.CheckHOLIDAY(T0007INProw("HOLIDAYKBN"), T0007INProw("PAYKBN")) Then
                        '        If T0007row("STTIME") = "00:00" AndAlso T0007row("ENDTIME") = "00:00" Then
                        '            If T0007INProw("STTIME") <> "00:00" AndAlso T0007INProw("ENDTIME") <> "00:00" Then
                        '                WW_YESTERDAYEDIT = "有"
                        '            End If
                        '        End If

                        '        If T0007row("STTIME") <> "00:00" AndAlso T0007row("ENDTIME") <> "00:00" Then
                        '            If T0007INProw("STTIME") = "00:00" AndAlso T0007INProw("ENDTIME") = "00:00" Then
                        '                WW_YESTERDAYEDIT = "有"
                        '            End If
                        '        End If
                        '    End If
                        'End If

                        WW_UPD_FLG = "ON"
                        T0007row("OPERATION") = T0007INProw("OPERATION")
                        T0007row("SELECT") = "0"
                        T0007row("HIDDEN") = "1" '非表示
                        T0007row("DELFLG") = "1"
                    End If

                    If WW_KEYTBL > WW_KEYINP Then
                        WW_IDX = i
                        Exit For
                    End If
                Next
            End If
        Next

        If WW_UPD_FLG = "ON" Then
            '当画面で生成したデータ（タイムスタンプ＝0）に対する変更は、変更前を物理削除する　
            For i As Integer = T0007tbl.Rows.Count - 1 To 0 Step -1
                Dim T0007row As DataRow = T0007tbl.Rows(i)
                If T0007row("TIMSTP") = "0" AndAlso
                   T0007row("SELECT") = "0" Then
                    T0007row.Delete()
                    Continue For
                End If

                '前日データをマーキング
                'If WW_YESTERDAYEDIT = "有" Then
                '    Dim WW_DATE As Date = CDate(WF_WORKDATE.Text).AddDays(-1)
                '    If T0007row("WORKDATE") = WW_DATE.ToString("yyyy/MM/dd") AndAlso
                '        T0007row("STAFFCODE") = WF_STAFFCODE.Text AndAlso
                '        T0007row("ENDDATE") >= WF_WORKDATE.Text Then
                '        If InStr(T0007row("STATUS"), "Ｂ勤再計算") > 0 Then
                '        Else
                '            If T0007row("STATUS") = "" Then
                '                T0007row("STATUS") = T0007row("STATUS") & "Ｂ勤再計算"
                '            Else
                '                T0007row("STATUS") = T0007row("STATUS") & ",Ｂ勤再計算"
                '            End If
                '        End If
                '    End If
                'End If
            Next

            '更新データを抽出
            Dim WW_T0007INPtbl As DataTable = T0007INPtbl.Clone
            Dim WW_SEL As String = "OPERATION = '更新'"

            CS0026TblSort.TABLE = T0007INPtbl
            CS0026TblSort.FILTER = WW_SEL
            CS0026TblSort.SORTING = "STAFFCODE, WORKDATE, RECODEKBN, STDATE, STTIME, ENDDATE, ENDTIME, WORKKBN"
            WW_T0007INPtbl = CS0026TblSort.Sort()

            '元のデータを削除後（上記）、画面入力データを新たに追加
            T0007tbl.Merge(WW_T0007INPtbl)

            WW_T0007INPtbl.Dispose()
            WW_T0007INPtbl = Nothing

            Dim WW_T0007SELtbl As DataTable = T0007tbl.Clone
            WW_SEL = "STAFFCODE = '" & WF_STAFFCODE.Text & "'"

            CS0026TblSort.TABLE = T0007tbl
            CS0026TblSort.FILTER = WW_SEL
            CS0026TblSort.SORTING = "STAFFCODE, WORKDATE, RECODEKBN, STDATE, STTIME, ENDDATE, ENDTIME, WORKKBN"
            WW_T0007SELtbl = CS0026TblSort.Sort()

            WW_SEL = "STAFFCODE <> '" & WF_STAFFCODE.Text & "'"

            CS0026TblSort.TABLE = T0007tbl
            CS0026TblSort.FILTER = WW_SEL
            CS0026TblSort.SORTING = "STAFFCODE, WORKDATE, RECODEKBN, STDATE, STTIME, ENDDATE, ENDTIME, WORKKBN"
            T0007tbl = CS0026TblSort.Sort()

            '月合計レコードの再作成
            If WF_DetailMView.ActiveViewIndex = 0 Or WF_DetailMView.ActiveViewIndex = 2 Then
                '日別
                T0007COM.T0007_TotalRecodeCreate(WW_T0007SELtbl)
            Else
                '月合計
                T0007COM.T0007_TotalRecodeEdit(WW_T0007SELtbl)
            End If

            '月調整レコードの再作成
            T0007COM.T0007_ChoseiRecodeCreate(WW_T0007SELtbl)

            T0007tbl.Merge(WW_T0007SELtbl)

            CS0026TblSort.TABLE = T0007tbl
            CS0026TblSort.FILTER = ""
            CS0026TblSort.SORTING = "ORGSEQ, STAFFCODE, WORKDATE, RECODEKBN, HDKBN DESC, STDATE, STTIME, ENDDATE, ENDTIME, WORKKBN"
            T0007tbl = CS0026TblSort.Sort()

            Dim WW_LINECNT As Integer = 0
            For Each WW_TBLrow As DataRow In T0007tbl.Rows
                If WW_TBLrow("SELECT") = "1" Then
                    If WW_TBLrow("HDKBN") = "H" And WW_TBLrow("DELFLG") = "0" Then
                        WW_TBLrow("SELECT") = "1"
                        WW_TBLrow("HIDDEN") = "0"      '表示
                        WW_LINECNT += 1
                        WW_TBLrow("LINECNT") = WW_LINECNT
                        If WW_TBLrow("RECODEKBN") = "2" AndAlso WW_TBLrow("STAFFCODE") = WF_STAFFCODE.Text Then
                            WW_TBLrow("OPERATION") = "更新"
                        End If
                    Else
                        WW_TBLrow("SELECT") = "1"
                        WW_TBLrow("HIDDEN") = "1"      '非表示
                        WW_TBLrow("LINECNT") = 0
                    End If

                    '絞込条件
                    If WW_TBLrow("HDKBN") = "H" Then
                        WW_TBLrow("HIDDEN") = 1

                        '従業員・日付の絞込判定　（絞込指定があれば、月調整、合計を非表示）
                        If work.WF_T7I_Head_STAFFCODE.Text = "" AndAlso
                           work.WF_T7I_Head_WORKDATE.Text = "" Then
                            WW_TBLrow("HIDDEN") = 0
                        End If

                        If work.WF_T7I_Head_STAFFCODE.Text <> "" AndAlso
                            work.WF_T7I_Head_WORKDATE.Text = "" Then
                            If WW_TBLrow("STAFFCODE") Like work.WF_T7I_Head_STAFFCODE.Text & "*" Then
                                WW_TBLrow("HIDDEN") = 0
                            End If
                        End If

                        If work.WF_T7I_Head_STAFFCODE.Text = "" AndAlso
                            work.WF_T7I_Head_WORKDATE.Text <> "" Then
                            If WW_TBLrow("WORKDATE") = work.WF_T7I_Head_WORKDATE.Text Then
                                If WW_TBLrow("RECODEKBN") = "0" Then
                                    WW_TBLrow("HIDDEN") = 0
                                Else
                                    WW_TBLrow("HIDDEN") = 1
                                End If
                            End If
                        End If

                        If work.WF_T7I_Head_STAFFCODE.Text <> "" AndAlso
                            work.WF_T7I_Head_WORKDATE.Text <> "" Then
                            If WW_TBLrow("STAFFCODE") Like work.WF_T7I_Head_STAFFCODE.Text & "*" AndAlso
                               WW_TBLrow("WORKDATE") = work.WF_T7I_Head_WORKDATE.Text Then
                                If WW_TBLrow("RECODEKBN") = "0" Then
                                    WW_TBLrow("HIDDEN") = 0
                                Else
                                    WW_TBLrow("HIDDEN") = 1
                                End If
                            End If
                        End If
                    End If

                End If
            Next

            '■■■ 前画面（T00007I）用にテーブルデータ保存 ■■■
            If Not Master.SaveTable(T0007tbl, work.WF_T7I_XMLsaveF.Text) Then
                Exit Sub
            End If
            If Not Master.SaveTable(T0007INPtbl, work.WF_T7KIN_XMLsaveF2.Text) Then
                Exit Sub
            End If
        End If

    End Sub

    ' *** GridView用データ取得                                                   
    Private Sub GRID_INITset()

        'ソート文字列取得
        Dim WW_SORT As String = ""
        CS0026TblSort.COMPCODE = work.WF_T7SEL_CAMPCODE.Text
        CS0026TblSort.PROFID = Master.PROF_VIEW
        CS0026TblSort.TAB = ""
        CS0026TblSort.MAPID = Master.MAPID
        CS0026TblSort.VARI = Master.VIEWID
        CS0026TblSort.GetSorting()
        If CS0026TblSort.ERR = C_MESSAGE_NO.NORMAL Then
            WW_SORT = "ORDER BY " & CS0026TblSort.SORTING
        End If

        '■■■ 画面表示用データ取得 ■■■
        '○処理準備
        '前画面のテーブルデータ 復元(TEXTファイルより復元)
        'T0007COM.T0007tbl_ColumnsAdd(T0007tbl)
        If Not Master.RecoverTable(T0007tbl, work.WF_T7I_XMLsaveF.Text) Then
            Exit Sub
        End If

        Dim WW_CHANGE As String = "OFF"

        Try
            Dim WW_FILTER As String = ""

            '対象データ抽出(指定日入力）
            If work.WF_T7KIN_RECODEKBN.Text = "0" Then
                T0007INPtbl = T0007tbl.Clone
                WW_FILTER = ""
                WW_FILTER = WW_FILTER & "WORKDATE  = '" & work.WF_T7KIN_WORKDATE.Text & "' and "
                WW_FILTER = WW_FILTER & "STAFFCODE = '" & work.WF_T7KIN_STAFFCODE.Text & "' and "
                WW_FILTER = WW_FILTER & "SELECT    = '1' and RECODEKBN = '0'"

                CS0026TblSort.TABLE = T0007tbl
                CS0026TblSort.FILTER = WW_FILTER
                CS0026TblSort.SORTING = "SELECT, STAFFCODE, WORKDATE, STDATE, STTIME, ENDDATE, ENDTIME, WORKKBN"
                T0007INPtbl = CS0026TblSort.Sort()
            End If

            '対象データ抽出(月合計入力））
            If work.WF_T7KIN_RECODEKBN.Text = "2" Then
                T0007INPtbl = T0007tbl.Clone
                WW_FILTER = ""
                WW_FILTER = WW_FILTER & "STAFFCODE = '" & work.WF_T7KIN_STAFFCODE.Text & "' and "
                WW_FILTER = WW_FILTER & "SELECT    = '1' and RECODEKBN = '2'"

                CS0026TblSort.TABLE = T0007tbl
                CS0026TblSort.FILTER = WW_FILTER
                CS0026TblSort.SORTING = "SELECT, STAFFCODE, WORKDATE, STDATE, STTIME, ENDDATE, ENDTIME, WORKKBN"
                T0007INPtbl = CS0026TblSort.Sort()

                '合計に明細レコードが存在するか？
                Dim WW_FIND As String = "OFF"
                For Each WW_TTLrow As DataRow In T0007INPtbl.Rows
                    If WW_TTLrow("HDKBN") = "D" Then
                        WW_FIND = "ON"
                        Exit For
                    End If
                Next

                '存在しない場合、月合計（明細）レコードを作成する
                Dim WW_T0007tbl As DataTable = T0007INPtbl.Clone
                Dim WW_T0007row As DataRow
                If WW_FIND = "OFF" Then
                    For Each WW_TTLrow As DataRow In T0007INPtbl.Rows

                        For i As Integer = 1 To 2
                            For j As Integer = 1 To 10
                                WW_T0007row = WW_T0007tbl.NewRow
                                T0007COM.INProw_Init(work.WF_T7SEL_CAMPCODE.Text, WW_T0007row)
                                'その他の項目は、現在のレコードをコピーする
                                WW_T0007row("TAISHOYM") = WW_TTLrow("TAISHOYM")
                                WW_T0007row("WORKDATE") = WW_TTLrow("WORKDATE")
                                WW_T0007row("STAFFCODE") = WW_TTLrow("STAFFCODE")
                                WW_T0007row("STAFFKBN") = WW_TTLrow("STAFFKBN")
                                WW_T0007row("MORG") = WW_TTLrow("MORG")
                                WW_T0007row("HORG") = WW_TTLrow("HORG")
                                WW_T0007row("HIDDEN") = "1"
                                WW_T0007row("HDKBN") = "D"
                                WW_T0007row("DATAKBN") = "K"
                                WW_T0007row("RECODEKBN") = "2"
                                WW_T0007row("SHARYOKBN") = i.ToString
                                WW_T0007row("OILPAYKBN") = j.ToString("00")
                                WW_T0007tbl.Rows.Add(WW_T0007row)
                            Next
                        Next
                    Next

                End If

                T0007INPtbl.Merge(WW_T0007tbl)

                WW_T0007tbl.Dispose()
                WW_T0007tbl = Nothing

            End If

            'ソート処理
            CS0026TblSort.TABLE = T0007INPtbl
            CS0026TblSort.FILTER = ""
            CS0026TblSort.SORTING = "STAFFCODE, WORKDATE, STDATE, STTIME, ENDDATE, ENDTIME, WORKKBN"
            T0007INPtbl = CS0026TblSort.Sort()

            '画面編集
            DisplayEdit(T0007INPtbl)

            '■■■ 画面表示（タブ切り替え） ■■■
            If work.WF_T7KIN_RECODEKBN.Text = "0" Then
                '指定日入力
                WF_DTABChange.Value = 0
            Else
                '月合計入力
                WF_DTABChange.Value = 1
            End If
            WF_Detail_TABChange()

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "T0007_NIPPO SELECT")

            CS0011LOGWRITE.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:T0007_NIPPO Select"      '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                  '
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try

        '■■■ 画面（GridView）表示データ保存 ■■■
        If Not Master.SaveTable(T0007INPtbl, work.WF_T7KIN_XMLsaveF.Text) Then
            Exit Sub
        End If

        'モデル距離再取得を取りやめ（終了ボタン）場合に使用する
        If Not Master.SaveTable(T0007INPtbl, work.WF_T7KIN_XMLsaveF2.Text) Then
            Exit Sub
        End If

        If WW_CHANGE = "ON" Then
            Master.Output(C_MESSAGE_NO.OVER_RETENTION_PERIOD_ERROR, C_MESSAGE_TYPE.INF)
        End If

    End Sub

    ' *** GridView用（日報）データ取得                                                   
    Private Sub NIPPOget_T7Format(ByRef ioT7tbl As DataTable, ByVal iT5tbl As DataTable, ByVal iYmdFrom As String, ByVal iYmdTo As String)

        'T5準備
        Dim iT0005view As DataView
        iT0005view = New DataView(iT5tbl)
        iT0005view.Sort = "YMD, STAFFCODE"

        '削除レコードを取得
        Dim WW_T0007DELtbl As DataTable = New DataTable
        CS0026TblSort.TABLE = ioT7tbl
        CS0026TblSort.FILTER = "SELECT = '0'"
        CS0026TblSort.SORTING = "STAFFCODE, WORKDATE, STDATE, STTIME, ENDDATE, ENDTIME"
        WW_T0007DELtbl = CS0026TblSort.Sort()

        '勤怠のヘッダレコードを取得
        Dim WW_T0007HEADtbl As DataTable = New DataTable
        CS0026TblSort.TABLE = ioT7tbl
        CS0026TblSort.FILTER = "SELECT = '1' and HDKBN = 'H'"
        CS0026TblSort.SORTING = "STAFFCODE, WORKDATE, STDATE, STTIME, ENDDATE, ENDTIME"
        WW_T0007HEADtbl = CS0026TblSort.Sort()

        '勤怠の明細レコードを取得
        Dim WW_T0007DTLtbl As DataTable = New DataTable

        CS0026TblSort.TABLE = ioT7tbl
        CS0026TblSort.FILTER = "SELECT = '1' and HDKBN = 'D'"
        CS0026TblSort.SORTING = "STAFFCODE, WORKDATE, STDATE, STTIME, ENDDATE, ENDTIME"
        WW_T0007DTLtbl = CS0026TblSort.Sort()

        'T7準備
        Dim iT0007view As DataView
        iT0007view = New DataView(WW_T0007HEADtbl)
        iT0007view.Sort = "WORKDATE, STAFFCODE"
        iT0007view.RowFilter = "HDKBN = 'H' and RECODEKBN ='0' and WORKDATE >= #" & iYmdFrom & "# and WORKDATE <= #" & iYmdTo & "#"
        Dim wT0007tbl As DataTable = iT0007view.ToTable

        For Each WW_HEADrow As DataRow In wT0007tbl.Rows
            iT0005view.RowFilter = "YMD = #" & WW_HEADrow("WORKDATE") & "# and STAFFCODE ='" & WW_HEADrow("STAFFCODE") & "'"
            Dim T0005tbl As DataTable = iT0005view.ToTable()

            '編集
            NIPPO_EDIT(ioT7tbl, T0005tbl)
        Next
        iT0005view.Dispose()
        iT0005view = Nothing
        WW_T0007DELtbl.Dispose()
        WW_T0007DELtbl = Nothing
        WW_T0007HEADtbl.Dispose()
        WW_T0007HEADtbl = Nothing
        WW_T0007DTLtbl.Dispose()
        WW_T0007DTLtbl = Nothing

    End Sub

    ' *** GridView用（日報）データ取得                                                   
    Private Sub NIPPOget_T7Format2(ByRef ioT7tbl As DataTable, ByVal iT5tbl As DataTable, ByVal iT7row As DataRow)

        'T5準備
        Using iT0005view As DataView = New DataView(iT5tbl)
            iT0005view.Sort = "YMD, STAFFCODE"

            iT0005view.RowFilter = "YMD = #" & iT7row("WORKDATE") & "# and STAFFCODE ='" & iT7row("STAFFCODE") & "'"
            Dim T0005tbl As DataTable = iT0005view.ToTable()

            '編集
            NIPPO_EDIT(ioT7tbl, T0005tbl)
            T0005tbl.Dispose()
            T0005tbl = Nothing
        End Using

    End Sub

    ' *** （日報）データ編集                                                   
    Public Sub NIPPO_EDIT(ByRef ioT7tbl As DataTable, ByRef iT0005tbl As DataTable)

        For Each T5row As DataRow In iT0005tbl.Rows
            If T5row("WORKKBN") = "A1" Or T5row("WORKKBN") = "Z1" Then
                Continue For
            End If

            Dim T0007row As DataRow = ioT7tbl.NewRow

            T0007row("LINECNT") = "0"
            T0007row("OPERATION") = ""
            T0007row("TIMSTP") = "0"
            T0007row("SELECT") = "1"
            T0007row("HIDDEN") = "1"
            T0007row("EXTRACTCNT") = "0"

            T0007row("STATUS") = "日報取込"
            T0007row("CAMPCODE") = T5row("CAMPCODE")
            T0007row("CAMPNAMES") = T5row("CAMPNAMES")
            T0007row("TAISHOYM") = work.WF_T7SEL_TAISHOYM.Text
            T0007row("STAFFCODE") = T5row("STAFFCODE")
            T0007row("STAFFNAMES") = T5row("STAFFNAMES")
            T0007row("WORKDATE") = T5row("YMD")
            T0007row("WORKINGWEEK") = T5row("WORKINGWEEK")
            T0007row("WORKINGWEEKNAMES") = T5row("WORKINGWEEKNAMES")
            T0007row("HDKBN") = "D"
            T0007row("RECODEKBN") = "0"
            T0007row("RECODEKBNNAMES") = ""
            T0007row("SEQ") = T5row("SEQ")
            T0007row("ENTRYDATE") = "              "
            T0007row("NIPPOLINKCODE") = T5row("UPDYMD")
            T0007row("MORG") = T5row("MORG")
            T0007row("MORGNAMES") = T5row("MORGNAMES")
            T0007row("HORG") = T5row("HORG")
            T0007row("HORGNAMES") = T5row("HORGNAMES")
            T0007row("SORG") = T5row("SORG")
            T0007row("SORGNAMES") = T5row("SORGNAMES")
            T0007row("STAFFKBN") = T5row("STAFFKBN")
            T0007row("STAFFKBNNAMES") = T5row("STAFFKBNNAMES")
            T0007row("HOLIDAYKBN") = T5row("HOLIDAYKBN")
            T0007row("HOLIDAYKBNNAMES") = T5row("HOLIDAYKBNNAMES")
            T0007row("PAYKBN") = ""
            T0007row("PAYKBNNAMES") = ""
            T0007row("SHUKCHOKKBN") = ""
            T0007row("SHUKCHOKKBNNAMES") = ""
            T0007row("WORKKBN") = T5row("WORKKBN")
            T0007row("WORKKBNNAMES") = T5row("WORKKBNNAMES")
            T0007row("STDATE") = T5row("STDATE")
            T0007row("STTIME") = T5row("STTIME")
            T0007row("ENDDATE") = T5row("ENDDATE")
            T0007row("ENDTIME") = T5row("ENDTIME")
            T0007row("WORKTIME") = T0007COM.formatHHMM(T5row("WORKTIME"))
            T0007row("MOVETIME") = T0007COM.formatHHMM(T5row("MOVETIME"))
            T0007row("ACTTIME") = T0007COM.formatHHMM(T5row("ACTTIME"))
            T0007row("BINDSTDATE") = "00:00"
            T0007row("BINDTIME") = "0"
            T0007row("NIPPOBREAKTIME") = "0"
            T0007row("BREAKTIME") = "0"
            T0007row("BREAKTIMECHO") = "0"
            T0007row("BREAKTIMETTL") = "0"
            T0007row("NIGHTTIME") = "0"
            T0007row("NIGHTTIMECHO") = "0"
            T0007row("NIGHTTIMETTL") = "0"
            T0007row("ORVERTIME") = "0"
            T0007row("ORVERTIMECHO") = "0"
            T0007row("ORVERTIMETTL") = "0"
            T0007row("WNIGHTTIME") = "0"
            T0007row("WNIGHTTIMECHO") = "0"
            T0007row("WNIGHTTIMETTL") = "0"
            T0007row("SWORKTIME") = "0"
            T0007row("SWORKTIMECHO") = "0"
            T0007row("SWORKTIMETTL") = "0"
            T0007row("SNIGHTTIME") = "0"
            T0007row("SNIGHTTIMECHO") = "0"
            T0007row("SNIGHTTIMETTL") = "0"
            T0007row("HWORKTIME") = "0"
            T0007row("HWORKTIMECHO") = "0"
            T0007row("HWORKTIMETTL") = "0"
            T0007row("HNIGHTTIME") = "0"
            T0007row("HNIGHTTIMECHO") = "0"
            T0007row("HNIGHTTIMETTL") = "0"
            T0007row("WORKNISSU") = "0"
            T0007row("WORKNISSUCHO") = "0"
            T0007row("WORKNISSUTTL") = "0"
            T0007row("SHOUKETUNISSU") = "0"
            T0007row("SHOUKETUNISSUCHO") = "0"
            T0007row("SHOUKETUNISSUTTL") = "0"
            T0007row("KUMIKETUNISSU") = "0"
            T0007row("KUMIKETUNISSUCHO") = "0"
            T0007row("KUMIKETUNISSUTTL") = "0"
            T0007row("ETCKETUNISSU") = "0"
            T0007row("ETCKETUNISSUCHO") = "0"
            T0007row("ETCKETUNISSUTTL") = "0"
            T0007row("NENKYUNISSU") = "0"
            T0007row("NENKYUNISSUCHO") = "0"
            T0007row("NENKYUNISSUTTL") = "0"
            T0007row("TOKUKYUNISSU") = "0"
            T0007row("TOKUKYUNISSUCHO") = "0"
            T0007row("TOKUKYUNISSUTTL") = "0"
            T0007row("CHIKOKSOTAINISSU") = "0"
            T0007row("CHIKOKSOTAINISSUCHO") = "0"
            T0007row("CHIKOKSOTAINISSUTTL") = "0"
            T0007row("STOCKNISSU") = "0"
            T0007row("STOCKNISSUCHO") = "0"
            T0007row("STOCKNISSUTTL") = "0"
            T0007row("KYOTEIWEEKNISSU") = "0"
            T0007row("KYOTEIWEEKNISSUCHO") = "0"
            T0007row("KYOTEIWEEKNISSUTTL") = "0"
            T0007row("WEEKNISSU") = "0"
            T0007row("WEEKNISSUCHO") = "0"
            T0007row("WEEKNISSUTTL") = "0"
            T0007row("ROSAIYUKYNIUSSU") = "0"
            T0007row("ROSAIYUKYNIUSSUCHO") = "0"
            T0007row("ROSAIYUKYNIUSSUTTL") = "0"
            T0007row("TOKUKYUMUKYUNISSU") = "0"
            T0007row("TOKUKYUMUKYUNISSUCHO") = "0"
            T0007row("TOKUKYUMUKYUNISSUTTL") = "0"
            T0007row("KOKANGOYUKYUNISSU") = "0"
            T0007row("KOKANGOYUKYUNISSUCHO") = "0"
            T0007row("KOKANGOYUKYUNISSUTTL") = "0"
            T0007row("KOKANGOMUKYUNISSU") = "0"
            T0007row("KOKANGOMUKYUNISSUCHO") = "0"
            T0007row("KOKANGOMUKYUNISSUTTL") = "0"
            T0007row("DAIKYUNISSU") = "0"
            T0007row("DAIKYUNISSUCHO") = "0"
            T0007row("DAIKYUNISSUTTL") = "0"
            T0007row("NENSHINISSU") = "0"
            T0007row("NENSHINISSUCHO") = "0"
            T0007row("NENSHINISSUTTL") = "0"
            T0007row("SHUKCHOKNNISSU") = "0"
            T0007row("SHUKCHOKNNISSUCHO") = "0"
            T0007row("SHUKCHOKNNISSUTTL") = "0"
            T0007row("SHUKCHOKNISSU") = "0"
            T0007row("SHUKCHOKNISSUCHO") = "0"
            T0007row("SHUKCHOKNISSUTTL") = "0"

            T0007row("SHUKCHOKNHLDNISSU") = "0"
            T0007row("SHUKCHOKNHLDNISSUCHO") = "0"
            T0007row("SHUKCHOKNHLDNISSUTTL") = "0"
            T0007row("SHUKCHOKHLDNISSU") = "0"
            T0007row("SHUKCHOKHLDNISSUCHO") = "0"
            T0007row("SHUKCHOKHLDNISSUTTL") = "0"

            T0007row("TOKSAAKAISU") = "0"
            T0007row("TOKSAAKAISUCHO") = "0"
            T0007row("TOKSAAKAISUTTL") = "0"
            T0007row("TOKSABKAISU") = "0"
            T0007row("TOKSABKAISUCHO") = "0"
            T0007row("TOKSABKAISUTTL") = "0"
            T0007row("TOKSACKAISU") = "0"
            T0007row("TOKSACKAISUCHO") = "0"
            T0007row("TOKSACKAISUTTL") = "0"
            T0007row("TENKOKAISU") = "0"
            T0007row("TENKOKAISUCHO") = "0"
            T0007row("TENKOKAISUTTL") = "0"
            T0007row("HOANTIME") = "0"
            T0007row("HOANTIMECHO") = "0"
            T0007row("HOANTIMETTL") = "0"
            T0007row("KOATUTIME") = "0"
            T0007row("KOATUTIMECHO") = "0"
            T0007row("KOATUTIMETTL") = "0"
            T0007row("TOKUSA1TIME") = "0"
            T0007row("TOKUSA1TIMECHO") = "0"
            T0007row("TOKUSA1TIMETTL") = "0"
            T0007row("HAYADETIME") = "0"
            T0007row("HAYADETIMECHO") = "0"
            T0007row("HAYADETIMETTL") = "0"
            T0007row("PONPNISSU") = "0"
            T0007row("PONPNISSUCHO") = "0"
            T0007row("PONPNISSUTTL") = "0"
            T0007row("BULKNISSU") = "0"
            T0007row("BULKNISSUCHO") = "0"
            T0007row("BULKNISSUTTL") = "0"
            T0007row("TRAILERNISSU") = "0"
            T0007row("TRAILERNISSUCHO") = "0"
            T0007row("TRAILERNISSUTTL") = "0"
            T0007row("BKINMUKAISU") = "0"
            T0007row("BKINMUKAISUCHO") = "0"
            T0007row("BKINMUKAISUTTL") = "0"
            If T5row("WORKKBN") = "B3" Then
                T0007row("SHARYOKBN") = T5row("SHARYOKBN")
                T0007row("SHARYOKBNNAMES") = T5row("SHARYOKBNNAMES")
                T0007row("OILPAYKBN") = T5row("OILPAYKBN")
                T0007row("OILPAYKBNNAMES") = T5row("OILPAYKBNNAMES")
                If T5row("SUISOKBN") = "1" Then
                    T0007row("UNLOADCNT") = "0"
                    T0007row("UNLOADCNTCHO") = "0"
                    T0007row("UNLOADCNTTTL") = "0"
                Else
                    T0007row("UNLOADCNT") = "1"
                    T0007row("UNLOADCNTCHO") = "0"
                    T0007row("UNLOADCNTTTL") = "1"
                End If
            Else
                T0007row("SHARYOKBN") = ""
                T0007row("SHARYOKBNNAMES") = ""
                T0007row("OILPAYKBN") = ""
                T0007row("OILPAYKBNNAMES") = ""
                T0007row("UNLOADCNT") = "0"
                T0007row("UNLOADCNTCHO") = "0"
                T0007row("UNLOADCNTTTL") = "0"
            End If
            T0007row("SHARYOKBN2") = T5row("SHARYOKBN")
            T0007row("SHARYOKBNNAMES2") = T5row("SHARYOKBNNAMES")
            T0007row("OILPAYKBN2") = T5row("OILPAYKBN")
            T0007row("OILPAYKBNNAMES2") = T5row("OILPAYKBNNAMES")
            If T5row("L1KAISO") = "回送" And T5row("SUISOKBN") <> "1" Then
                T0007row("HAIDISTANCE") = "0"
                T0007row("HAIDISTANCECHO") = "0"
                T0007row("HAIDISTANCETTL") = "0"
                T0007row("KAIDISTANCE") = Int(T5row("SOUDISTANCE"))
                T0007row("KAIDISTANCECHO") = "0"
                T0007row("KAIDISTANCETTL") = Int(T5row("SOUDISTANCE"))
            Else
                T0007row("HAIDISTANCE") = Int(T5row("SOUDISTANCE"))
                T0007row("HAIDISTANCECHO") = "0"
                T0007row("HAIDISTANCETTL") = Int(T5row("SOUDISTANCE"))
                T0007row("KAIDISTANCE") = "0"
                T0007row("KAIDISTANCECHO") = "0"
                T0007row("KAIDISTANCETTL") = "0"
            End If

            T0007row("DELFLG") = "0"

            T0007row("TRIPNO") = T5row("TRIPNO")

            T0007row("HWORKNISSU") = "0"
            T0007row("HWORKNISSUCHO") = "0"
            T0007row("HWORKNISSUTTL") = "0"

            T0007row("HDAIWORKTIME") = "0"
            T0007row("HDAIWORKTIMECHO") = "0"
            T0007row("HDAIWORKTIMETTL") = "0"

            T0007row("DATAKBN") = "N"
            T0007row("SHIPORG") = T5row("SHIPORG")
            T0007row("SHIPORGNAMES") = T5row("SHIPORGNAMES")
            T0007row("NIPPONO") = T5row("NIPPONO")
            T0007row("GSHABAN") = T5row("GSHABAN")
            T0007row("RUIDISTANCE") = T5row("RUIDISTANCE")
            T0007row("JIDISTANCE") = T5row("JIDISTANCE")
            T0007row("KUDISTANCE") = T5row("KUDISTANCE")
            T0007row("LATITUDE") = T5row("LATITUDE")
            T0007row("LONGITUDE") = T5row("LONGITUDE")
            T0007row("ORGSEQ") = 0

            'ポイント取得
            T0007row("MODELDISTANCE") = 0
            T0007row("MODELDISTANCECHO") = 0
            T0007row("MODELDISTANCETTL") = 0
            T0007row("wHaisoGroup") = T5row("wHaisoGroup")

            T0007row("UNLOADADDCNT1") = "0"
            T0007row("UNLOADADDCNT1CHO") = "0"
            T0007row("UNLOADADDCNT1TTL") = "0"
            T0007row("UNLOADADDCNT2") = "0"
            T0007row("UNLOADADDCNT2CHO") = "0"
            T0007row("UNLOADADDCNT2TTL") = "0"
            T0007row("UNLOADADDCNT3") = "0"
            T0007row("UNLOADADDCNT3CHO") = "0"
            T0007row("UNLOADADDCNT3TTL") = "0"
            T0007row("UNLOADADDCNT4") = "0"
            T0007row("UNLOADADDCNT4CHO") = "0"
            T0007row("UNLOADADDCNT4TTL") = "0"

            T0007row("SHORTDISTANCE1") = "0"
            T0007row("SHORTDISTANCE1CHO") = "0"
            T0007row("SHORTDISTANCE1TTL") = "0"
            T0007row("SHORTDISTANCE2") = "0"
            T0007row("SHORTDISTANCE2CHO") = "0"
            T0007row("SHORTDISTANCE2TTL") = "0"

            Select Case T5row("UNLOADADDTANKA")
                Case "0"
                Case "100"
                    T0007row("UNLOADADDCNT1") = "1"
                    T0007row("UNLOADADDCNT1CHO") = "0"
                    T0007row("UNLOADADDCNT1TTL") = "1"
                Case "200"
                    T0007row("UNLOADADDCNT2") = "1"
                    T0007row("UNLOADADDCNT2CHO") = "0"
                    T0007row("UNLOADADDCNT2TTL") = "1"
                Case "800"
                    T0007row("UNLOADADDCNT3") = "1"
                    T0007row("UNLOADADDCNT3CHO") = "0"
                    T0007row("UNLOADADDCNT3TTL") = "1"
                Case Else
                    T0007row("UNLOADADDCNT4") = "1"
                    T0007row("UNLOADADDCNT4CHO") = "0"
                    T0007row("UNLOADADDCNT4TTL") = "1"
            End Select

            Select Case T5row("UNLOADADDTANKA")
                Case "0"
                Case "1000"
                    T0007row("SHORTDISTANCE1") = "1"
                    T0007row("SHORTDISTANCE1CHO") = "0"
                    T0007row("SHORTDISTANCE1TTL") = "1"
                Case Else
                    T0007row("SHORTDISTANCE2") = "1"
                    T0007row("SHORTDISTANCE2CHO") = "0"
                    T0007row("SHORTDISTANCE2TTL") = "1"
            End Select

            ioT7tbl.Rows.Add(T0007row)
        Next

    End Sub
    ' ***  T0005データ取得処理
    Public Sub T00005ALLget(ByVal iKBN As String,
                            ByVal iSTAFFCODE As String,
                            ByVal iNIPPOLINKCODE As String,
                            ByVal iYmdFrom As String,
                            ByVal iYmdTo As String,
                            ByRef oTbl As DataTable,
                            ByRef oRtn As String)

        oRtn = C_MESSAGE_NO.NORMAL
        '■ 画面表示用データ取得

        'オブジェクト内容検索
        'ユーザプロファイル（変数）内容検索(自ユーザ権限＆抽出条件なしで検索)
        Try
            T0007COM.T0005tbl_ColumnsAdd(oTbl)

            'DataBase接続文字
            Dim SQLcon As SqlConnection = CS0050SESSION.getConnection
            SQLcon.Open() 'DataBase接続(Open)

            Dim SQLStr As String =
                 "SELECT 0 as LINECNT , " _
               & "       '' as OPERATION , " _
               & "       '1' as HIDDEN , " _
               & "       TIMSTP = cast(A.UPDTIMSTP as bigint) , " _
               & "       isnull(rtrim(A.CAMPCODE),'')  as CAMPCODE, " _
               & "       isnull(rtrim(A.SHIPORG),'') as SHIPORG , " _
               & "       '' as SHIPORGNAMES , " _
               & "       isnull(rtrim(A.TERMKBN),'') as TERMKBN, " _
               & "       '' as TERMKBNNAMES , " _
               & "       isnull(rtrim(A.YMD),'') as YMD , " _
               & "       isnull(rtrim(A.ENTRYDATE),'') as ENTRYDATE , " _
               & "       isnull(rtrim(A.NIPPONO),'') as NIPPONO , " _
               & "       isnull(A.SEQ,'0') as SEQ , " _
               & "       isnull(rtrim(A.WORKKBN),'') as WORKKBN , " _
               & "       isnull(rtrim(F1.VALUE2),'') as WORKKBNNAMES , " _
               & "       isnull(rtrim(A.STAFFCODE),'') as STAFFCODE , " _
               & "       isnull(rtrim(B.STAFFNAMES),'') as STAFFNAMES , " _
               & "       isnull(rtrim(A.SUBSTAFFCODE),'') as SUBSTAFFCODE , " _
               & "       isnull(rtrim(B2.STAFFNAMES),'') as SUBSTAFFNAMES , " _
               & "       isnull(rtrim(A.CREWKBN),'') as CREWKBN , " _
               & "       '' as CREWKBNNAMES , " _
               & "       isnull(rtrim(A.GSHABAN),'') as GSHABAN , " _
               & "       isnull(rtrim(MA4.LICNPLTNO2),'') as GSHABANLICNPLTNO , " _
               & "       isnull(rtrim(A.STDATE),'')  as STDATE , " _
               & "       isnull(rtrim(A.STTIME),'')  as STTIME , " _
               & "       isnull(rtrim(A.ENDDATE),'') as ENDDATE , " _
               & "       isnull(rtrim(A.ENDTIME),'') as ENDTIME , " _
               & "       isnull(rtrim(A.WORKTIME),'') as WORKTIME , " _
               & "       isnull(rtrim(A.MOVETIME),'') as MOVETIME , " _
               & "       isnull(rtrim(A.ACTTIME),'') as ACTTIME , " _
               & "       isnull(A.PRATE,'0') as PRATE , " _
               & "       isnull(A.CASH,'0') as CASH , " _
               & "       isnull(A.TICKET,'0') as TICKET , " _
               & "       isnull(A.ETC,'0') as ETC , " _
               & "       isnull(A.TOTALTOLL,'0') as TOTALTOLL , " _
               & "       isnull(A.STMATER,'0') as STMATER , " _
               & "       isnull(A.ENDMATER,'0') as ENDMATER , " _
               & "       isnull(A.RUIDISTANCE,'0') as RUIDISTANCE , " _
               & "       isnull(A.SOUDISTANCE,'0') as SOUDISTANCE , " _
               & "       isnull(A.JIDISTANCE,'0') as JIDISTANCE , " _
               & "       isnull(A.KUDISTANCE,'0') as KUDISTANCE , " _
               & "       isnull(A.IPPDISTANCE,'0') as IPPDISTANCE , " _
               & "       isnull(A.KOSDISTANCE,'0') as KOSDISTANCE , " _
               & "       isnull(A.IPPJIDISTANCE,'0') as IPPJIDISTANCE , " _
               & "       isnull(A.IPPKUDISTANCE,'0') as IPPKUDISTANCE , " _
               & "       isnull(A.KOSJIDISTANCE,'0') as KOSJIDISTANCE , " _
               & "       isnull(A.KOSKUDISTANCE,'0') as KOSKUDISTANCE , " _
               & "       isnull(A.KYUYU,'0') as KYUYU , " _
               & "       isnull(rtrim(A.TORICODE),'') as TORICODE , " _
               & "       isnull(rtrim(A.SHUKABASHO),'') as SHUKABASHO , " _
               & "       '' as SHUKABASHONAMES , " _
               & "       isnull(rtrim(A.TODOKECODE),'') as TODOKECODE , " _
               & "       '' as TODOKENAMES , " _
               & "       isnull(rtrim(A.TODOKEDATE),'') as TODOKEDATE , " _
               & "       isnull(rtrim(A.OILTYPE1),'') as OILTYPE1 , " _
               & "       isnull(rtrim(A.PRODUCT11),'') as PRODUCT11 , " _
               & "       isnull(rtrim(A.PRODUCT21),'') as PRODUCT21 , " _
               & "       isnull(rtrim(F41.VALUE1),'') as PRODUCT1NAMES , " _
               & "       isnull(rtrim(A.STANI1),'') as STANI1 , " _
               & "       '' as STANI1NAMES , " _
               & "       isnull(A.SURYO1,'0') as SURYO1 , " _
               & "       isnull(rtrim(A.OILTYPE2),'') as OILTYPE2 , " _
               & "       isnull(rtrim(A.PRODUCT12),'') as PRODUCT12 , " _
               & "       isnull(rtrim(A.PRODUCT22),'') as PRODUCT22 , " _
               & "       isnull(rtrim(F42.VALUE1),'') as PRODUCT2NAMES , " _
               & "       isnull(rtrim(A.STANI2),'') as STANI2 , " _
               & "       '' as STANI2NAMES , " _
               & "       isnull(A.SURYO2,'0') as SURYO2 , " _
               & "       isnull(rtrim(A.OILTYPE3),'') as OILTYPE3 , " _
               & "       isnull(rtrim(A.PRODUCT13),'') as PRODUCT13 , " _
               & "       isnull(rtrim(A.PRODUCT23),'') as PRODUCT23 , " _
               & "       isnull(rtrim(F43.VALUE1),'') as PRODUCT3NAMES , " _
               & "       isnull(rtrim(A.STANI3),'') as STANI3 , " _
               & "       '' as STANI3NAMES , " _
               & "       isnull(A.SURYO3,'0') as SURYO3 , " _
               & "       isnull(rtrim(A.OILTYPE4),'') as OILTYPE4 , " _
               & "       isnull(rtrim(A.PRODUCT14),'') as PRODUCT14 , " _
               & "       isnull(rtrim(A.PRODUCT24),'') as PRODUCT24 , " _
               & "       isnull(rtrim(F44.VALUE1),'') as PRODUCT4NAMES , " _
               & "       isnull(rtrim(A.STANI4),'') as STANI4 , " _
               & "       '' as STANI4NAMES , " _
               & "       isnull(A.SURYO4,'0') as SURYO4 , " _
               & "       isnull(rtrim(A.OILTYPE5),'') as OILTYPE5 , " _
               & "       isnull(rtrim(A.PRODUCT15),'') as PRODUCT15 , " _
               & "       isnull(rtrim(A.PRODUCT25),'') as PRODUCT25 , " _
               & "       isnull(rtrim(F45.VALUE1),'') as PRODUCT5NAMES , " _
               & "       isnull(rtrim(A.STANI5),'') as STANI5 , " _
               & "       '' as STANI5NAMES , " _
               & "       isnull(A.SURYO5,'0') as SURYO5 , " _
               & "       isnull(rtrim(A.OILTYPE6),'') as OILTYPE6 , " _
               & "       isnull(rtrim(A.PRODUCT16),'') as PRODUCT16 , " _
               & "       isnull(rtrim(A.PRODUCT26),'') as PRODUCT26 , " _
               & "       isnull(rtrim(F46.VALUE1),'') as PRODUCT6NAMES , " _
               & "       isnull(rtrim(A.STANI6),'') as STANI6 , " _
               & "       '' as STANI6NAMES , " _
               & "       isnull(A.SURYO6,'0') as SURYO6 , " _
               & "       isnull(rtrim(A.OILTYPE7),'') as OILTYPE7 , " _
               & "       isnull(rtrim(A.PRODUCT17),'') as PRODUCT17 , " _
               & "       isnull(rtrim(A.PRODUCT27),'') as PRODUCT27 , " _
               & "       isnull(rtrim(F47.VALUE1),'') as PRODUCT7NAMES , " _
               & "       isnull(rtrim(A.STANI7),'') as STANI7 , " _
               & "       '' as STANI7NAMES , " _
               & "       isnull(A.SURYO7,'0') as SURYO7 , " _
               & "       isnull(rtrim(A.OILTYPE8),'') as OILTYPE8 , " _
               & "       isnull(rtrim(A.PRODUCT18),'') as PRODUCT18 , " _
               & "       isnull(rtrim(A.PRODUCT28),'') as PRODUCT28 , " _
               & "       isnull(rtrim(F48.VALUE1),'') as PRODUCT8NAMES , " _
               & "       isnull(rtrim(A.STANI8),'') as STANI8 , " _
               & "       '' as STANI8NAMES , " _
               & "       isnull(A.SURYO8,'0') as SURYO8 , " _
               & "       isnull(A.TOTALSURYO,'0') as TOTALSURYO , " _
               & "       isnull(rtrim(A.TUMIOKIKBN),'') as TUMIOKIKBN , " _
               & "       '' as TUMIOKIKBNNAMES , " _
               & "       isnull(rtrim(A.ORDERNO),'') as ORDERNO , " _
               & "       isnull(rtrim(A.DETAILNO),'') as DETAILNO , " _
               & "       isnull(rtrim(A.TRIPNO),'') as TRIPNO , " _
               & "       isnull(rtrim(A.DROPNO),'') as DROPNO , " _
               & "       isnull(rtrim(A.JISSKIKBN),'') as JISSKIKBN , " _
               & "       '' as JISSKIKBNNAMES , " _
               & "       isnull(rtrim(A.URIKBN),'') as URIKBN , " _
               & "       '' as URIKBNNAMES , " _
               & "       isnull(rtrim(A.DELFLG),'') as DELFLG , " _
               & "       isnull(rtrim(A.SHARYOTYPEF),'') as SHARYOTYPEF , " _
               & "       isnull(rtrim(A.TSHABANF),'') as TSHABANF , " _
               & "       isnull(rtrim(A.SHARYOTYPEB),'') as SHARYOTYPEB , " _
               & "       isnull(rtrim(A.TSHABANB),'') as TSHABANB , " _
               & "       isnull(rtrim(A.SHARYOTYPEB2),'') as SHARYOTYPEB2 , " _
               & "       isnull(rtrim(A.TSHABANB2),'') as TSHABANB2 , " _
               & "       isnull(rtrim(A.TAXKBN),'') as TAXKBN , " _
               & "       '' as TAXKBNNAMES , " _
               & "       isnull(rtrim(A.LATITUDE),'') as LATITUDE , " _
               & "       isnull(rtrim(A.LONGITUDE),'') as LONGITUDE , " _
               & "       isnull(rtrim(MA6.SHARYOKBN),'') as SHARYOKBN , " _
               & "       isnull(rtrim(F2.VALUE1),'') as SHARYOKBNNAMES , " _
               & "       isnull(rtrim(MA6.OILKBN),'') as OILPAYKBN , " _
               & "       isnull(rtrim(F5.VALUE1),'') as OILPAYKBNNAMES , " _
               & "       isnull(rtrim(MA6.SUISOKBN),'0') as SUISOKBN , " _
               & "       isnull(rtrim(F6.VALUE1),'') as SUISOKBNNAMES , " _
               & "       isnull(rtrim(A.L1KAISO),'') as L1KAISO , " _
               & "       isnull(rtrim(CAL.WORKINGWEEK),'') as WORKINGWEEK , " _
               & "       isnull(rtrim(F7.VALUE1),'') as WORKINGWEEKNAMES , " _
               & "       isnull(rtrim(CAL.WORKINGKBN),'') as HOLIDAYKBN , " _
               & "       isnull(rtrim(F8.VALUE1),'') as HOLIDAYKBNNAMES , " _
               & "       isnull(rtrim(B.MORG),'') as MORG , " _
               & "       isnull(rtrim(M2M.NAMES),'') as MORGNAMES , " _
               & "       isnull(rtrim(B.HORG),'') as HORG , " _
               & "       isnull(rtrim(M2H.NAMES),'') as HORGNAMES , " _
               & "       isnull(rtrim(A.SHIPORG),'') as SORG , " _
               & "       isnull(rtrim(M2S.NAMES),'') as SORGNAMES , " _
               & "       isnull(rtrim(B.STAFFKBN),'') as STAFFKBN , " _
               & "       isnull(rtrim(F9.VALUE1),'') as STAFFKBNNAMES , " _
               & "       isnull(rtrim(P1.MODEL),'0') as MODELDISTANCE1 , " _
               & "       isnull(rtrim(P2.MODEL),'0') as MODELDISTANCE2 , " _
               & "       isnull(rtrim(P3.MODEL),'0') as MODELDISTANCE3 , " _
               & "       isnull(rtrim(A.L1HAISOGROUP),'') as wHaisoGroup , " _
               & "       isnull(rtrim(MD21.UNLOADADDTANKA),'0') as UNLOADADDTANKA , " _
               & "       isnull(rtrim(MD22.LOADINGTANKA),'0') as LOADINGTANKA , " _
               & "       isnull(rtrim(format(A.UPDYMD,'yyyyMMddHHmmss')),'') as UPDYMD " _
               & " FROM S0012_SRVAUTHOR X " _
               & " INNER JOIN S0006_ROLE Y " _
               & "   ON    Y.CAMPCODE     = X.CAMPCODE " _
               & "   and   Y.OBJECT       = 'SRVORG' " _
               & "   and   Y.ROLE         = X.ROLE" _
               & "   and   Y.STYMD       <= @P5 " _
               & "   and   Y.ENDYMD      >= @P5 " _
               & "   and   Y.DELFLG      <> '1' " _
               & " INNER JOIN (select CODE from M0006_STRUCT ORG " _
               & "             where ORG.CAMPCODE = @P1 " _
               & "              and  ORG.OBJECT   = 'ORG' " _
               & "              and  ORG.STRUCT   = '勤怠管理組織' " _
               & "              and  ORG.GRCODE01 = @P2 " _
               & "              and  ORG.STYMD   <= @P5 " _
               & "              and  ORG.ENDYMD  >= @P5 " _
               & "              and  ORG.DELFLG  <> '1'  " _
               & "            ) Z " _
               & "         ON  Z.CODE   = Y.CODE   "
            Dim SQLStr1 As String = ""
            SQLStr1 =
                 " INNER JOIN MB001_STAFF B " _
               & "   ON    B.CAMPCODE     = @P1 " _
               & "   and   B.STAFFCODE    = @P7 " _
               & "   and   B.STYMD       <= @P3 " _
               & "   and   B.ENDYMD      >= @P4 " _
               & "   and   B.STYMD        = (SELECT MAX(STYMD) FROM MB001_STAFF WHERE CAMPCODE = @P1 and STAFFCODE = @P7 and STYMD <= @P3 and ENDYMD >= @P4 and HORG = Y.CODE and DELFLG <> '1' ) " _
               & "   and   B.HORG         = Y.CODE " _
               & "   and   B.DELFLG      <> '1' "

            Dim SQLStr2 As String = ""
            If iKBN = "OLD" Then
                '古い日報を取得
                SQLStr2 =
                    " INNER JOIN T0005_NIPPO A " _
                   & "   ON    A.CAMPCODE   >= '02' " _
                   & "   and   A.STAFFCODE   = B.STAFFCODE " _
                   & "   and   A.YMD        <= @P3 " _
                   & "   and   A.YMD        >= @P4 " _
                   & "   and   format(A.UPDYMD,'yyyyMMddHHmmss') = @P8 "
            Else
                '最新の日報を取得
                SQLStr2 =
                  " INNER JOIN T0005_NIPPO A " _
                  & "   ON    A.CAMPCODE   >= '02' " _
                  & "   and   A.STAFFCODE   = B.STAFFCODE " _
                  & "   and   A.YMD        <= @P3 " _
                  & "   and   A.YMD        >= @P4 " _
                  & "   and   A.DELFLG     <> '1' "
            End If
            Dim SQLStr3 As String =
                 " LEFT JOIN MB001_STAFF B2 " _
               & "   ON    B2.CAMPCODE    = @P1 " _
               & "   and   B2.STAFFCODE   = A.SUBSTAFFCODE " _
               & "   and   B2.STYMD      <= A.YMD " _
               & "   and   B2.ENDYMD     >= A.YMD " _
               & "   and   B2.STYMD       = (SELECT MAX(STYMD) FROM MB001_STAFF WHERE CAMPCODE = @P1 and STAFFCODE = A.SUBSTAFFCODE and STYMD <= A.YMD and ENDYMD >= A.YMD and DELFLG <> '1' ) " _
               & "   and   B2.DELFLG     <> '1' " _
               & " LEFT JOIN M0002_ORG M2M " _
               & "   ON    M2M.CAMPCODE   = A.CAMPCODE " _
               & "   and   M2M.ORGCODE    = B.MORG " _
               & "   and   M2M.STYMD      <= A.YMD " _
               & "   and   M2M.ENDYMD     >= A.YMD " _
               & "   and   M2M.STYMD       = (SELECT MAX(STYMD) FROM M0002_ORG WHERE CAMPCODE = A.CAMPCODE and ORGCODE = B.MORG and STYMD <= A.YMD and ENDYMD >= A.YMD and DELFLG <> '1' )" _
               & "   and   M2M.DELFLG     <> '1' " _
               & " LEFT JOIN M0002_ORG M2H " _
               & "   ON    M2H.CAMPCODE   = A.CAMPCODE " _
               & "   and   M2H.ORGCODE    = B.HORG " _
               & "   and   M2H.STYMD      <= A.YMD " _
               & "   and   M2H.ENDYMD     >= A.YMD " _
               & "   and   M2H.STYMD       = (SELECT MAX(STYMD) FROM M0002_ORG WHERE CAMPCODE = A.CAMPCODE and ORGCODE = B.HORG and STYMD <= A.YMD and ENDYMD >= A.YMD and DELFLG <> '1' )" _
               & "   and   M2H.DELFLG     <> '1' " _
               & " LEFT JOIN M0002_ORG M2S " _
               & "   ON    M2S.CAMPCODE   = A.CAMPCODE " _
               & "   and   M2S.ORGCODE    = A.SHIPORG " _
               & "   and   M2S.STYMD      <= A.YMD " _
               & "   and   M2S.ENDYMD     >= A.YMD " _
               & "   and   M2S.STYMD       = (SELECT MAX(STYMD) FROM M0002_ORG WHERE CAMPCODE = A.CAMPCODE and ORGCODE = A.SHIPORG and STYMD <= A.YMD and ENDYMD >= A.YMD and DELFLG <> '1' )" _
               & "   and   M2S.DELFLG     <> '1' " _
               & " LEFT JOIN MC001_FIXVALUE F1 " _
               & "   ON    F1.CAMPCODE    = @P1 " _
               & "   and   F1.CLASS       = 'WORKKBN' " _
               & "   and   F1.KEYCODE     = A.WORKKBN " _
               & "   and   F1.STYMD      <= @P5 " _
               & "   and   F1.ENDYMD     >= @P5 " _
               & "   and   F1.DELFLG     <> '1' " _
               & " LEFT JOIN MA006_SHABANORG MA6 " _
               & "   ON    MA6.CAMPCODE    = X.CAMPCODE " _
               & "   and   MA6.MANGUORG    = A.SHIPORG " _
               & "   and   MA6.GSHABAN     = A.GSHABAN " _
               & "   and   MA6.DELFLG     <> '1' " _
               & " LEFT JOIN MC001_FIXVALUE F2 " _
               & "   ON    F2.CAMPCODE    = @P1 " _
               & "   and   F2.CLASS       = 'SHARYOKBN' " _
               & "   and   F2.KEYCODE     = MA6.SHARYOKBN " _
               & "   and   F2.STYMD      <= @P5 " _
               & "   and   F2.ENDYMD     >= @P5 " _
               & "   and   F2.DELFLG     <> '1' " _
               & " LEFT JOIN MC001_FIXVALUE F5 " _
               & "   ON    F5.CAMPCODE    = @P1 " _
               & "   and   F5.CLASS       = 'OILPAYKBN' " _
               & "   and   F5.KEYCODE     = MA6.OILKBN " _
               & "   and   F5.STYMD      <= @P5 " _
               & "   and   F5.ENDYMD     >= @P5 " _
               & "   and   F5.DELFLG     <> '1' " _
               & " LEFT JOIN MC001_FIXVALUE F41 " _
               & "   ON    F41.CAMPCODE    = @P1 " _
               & "   and   F41.CLASS       = 'PRODUCT1' " _
               & "   and   F41.KEYCODE     = A.PRODUCT11 " _
               & "   and   F41.STYMD      <= @P5 " _
               & "   and   F41.ENDYMD     >= @P5 " _
               & "   and   F41.DELFLG     <> '1' " _
               & " LEFT JOIN MC001_FIXVALUE F42 " _
               & "   ON    F42.CAMPCODE    = @P1 " _
               & "   and   F42.CLASS       = 'PRODUCT1' " _
               & "   and   F42.KEYCODE     = A.PRODUCT12 " _
               & "   and   F42.STYMD      <= @P5 " _
               & "   and   F42.ENDYMD     >= @P5 " _
               & "   and   F42.DELFLG     <> '1' " _
               & " LEFT JOIN MC001_FIXVALUE F43 " _
               & "   ON    F43.CAMPCODE    = @P1 " _
               & "   and   F43.CLASS       = 'PRODUCT1' " _
               & "   and   F43.KEYCODE     = A.PRODUCT13 " _
               & "   and   F43.STYMD      <= @P5 " _
               & "   and   F43.ENDYMD     >= @P5 " _
               & "   and   F43.DELFLG     <> '1' " _
               & " LEFT JOIN MC001_FIXVALUE F44 " _
               & "   ON    F44.CAMPCODE    = @P1 " _
               & "   and   F44.CLASS       = 'PRODUCT1' " _
               & "   and   F44.KEYCODE     = A.PRODUCT14 " _
               & "   and   F44.STYMD      <= @P5 " _
               & "   and   F44.ENDYMD     >= @P5 " _
               & "   and   F44.DELFLG     <> '1' " _
               & " LEFT JOIN MC001_FIXVALUE F45 " _
               & "   ON    F45.CAMPCODE    = @P1 " _
               & "   and   F45.CLASS       = 'PRODUCT1' " _
               & "   and   F45.KEYCODE     = A.PRODUCT15 " _
               & "   and   F45.STYMD      <= @P5 " _
               & "   and   F45.ENDYMD     >= @P5 " _
               & "   and   F45.DELFLG     <> '1' " _
               & " LEFT JOIN MC001_FIXVALUE F46 " _
               & "   ON    F46.CAMPCODE    = @P1 " _
               & "   and   F46.CLASS       = 'PRODUCT1' " _
               & "   and   F46.KEYCODE     = A.PRODUCT16 " _
               & "   and   F46.STYMD      <= @P5 " _
               & "   and   F46.ENDYMD     >= @P5 " _
               & "   and   F46.DELFLG     <> '1' " _
               & " LEFT JOIN MC001_FIXVALUE F47 " _
               & "   ON    F47.CAMPCODE    = @P1 " _
               & "   and   F47.CLASS       = 'PRODUCT1' " _
               & "   and   F47.KEYCODE     = A.PRODUCT17 " _
               & "   and   F47.STYMD      <= @P5 " _
               & "   and   F47.ENDYMD     >= @P5 " _
               & "   and   F47.DELFLG     <> '1' " _
               & " LEFT JOIN MC001_FIXVALUE F48 " _
               & "   ON    F48.CAMPCODE    = @P1 " _
               & "   and   F48.CLASS       = 'PRODUCT1' " _
               & "   and   F48.KEYCODE     = A.PRODUCT18 " _
               & "   and   F48.STYMD      <= @P5 " _
               & "   and   F48.ENDYMD     >= @P5 " _
               & "   and   F48.DELFLG     <> '1' " _
               & " LEFT JOIN MC001_FIXVALUE F6 " _
               & "   ON    F6.CAMPCODE    = @P1 " _
               & "   and   F6.CLASS       = 'SUISOKBN' " _
               & "   and   F6.KEYCODE     = isnull(MA6.SUISOKBN,'0') " _
               & "   and   F6.STYMD      <= @P5 " _
               & "   and   F6.ENDYMD     >= @P5 " _
               & "   and   F6.DELFLG     <> '1' " _
               & " LEFT JOIN MA004_SHARYOC MA4 " _
               & "   ON    MA4.CAMPCODE    = X.CAMPCODE " _
               & "   and   MA4.SHARYOTYPE  = A.SHARYOTYPEF " _
               & "   and   MA4.TSHABAN     = A.TSHABANF " _
               & "   and   MA4.STYMD      <= A.YMD " _
               & "   and   MA4.ENDYMD     >= A.YMD " _
               & "   and   MA4.DELFLG     <> '1' " _
               & " LEFT JOIN MB005_CALENDAR CAL " _
               & "   ON    CAL.CAMPCODE    = A.CAMPCODE " _
               & "   and   CAL.WORKINGYMD  = A.YMD " _
               & "   and   CAL.DELFLG     <> '1' " _
               & " LEFT JOIN MC001_FIXVALUE F7 " _
               & "   ON    F7.CAMPCODE    = @P1 " _
               & "   and   F7.CLASS       = 'WORKINGWEEK' " _
               & "   and   F7.KEYCODE     = CAL.WORKINGWEEK " _
               & "   and   F7.STYMD      <= @P5 " _
               & "   and   F7.ENDYMD     >= @P5 " _
               & "   and   F7.DELFLG     <> '1' " _
               & " LEFT JOIN MC001_FIXVALUE F8 " _
               & "   ON    F8.CAMPCODE    = @P1 " _
               & "   and   F8.CLASS       = 'HOLIDAYKBN' " _
               & "   and   F8.KEYCODE     = CAL.WORKINGKBN " _
               & "   and   F8.STYMD      <= @P5 " _
               & "   and   F8.ENDYMD     >= @P5 " _
               & "   and   F8.DELFLG     <> '1' " _
               & " LEFT JOIN MC001_FIXVALUE F9 " _
               & "   ON    F9.CAMPCODE    = @P1 " _
               & "   and   F9.CLASS       = 'STAFFKBN' " _
               & "   and   F9.KEYCODE     = B.STAFFKBN " _
               & "   and   F9.STYMD      <= @P5 " _
               & "   and   F9.ENDYMD     >= @P5 " _
               & "   and   F9.DELFLG     <> '1' " _
               & " LEFT JOIN MC012_MODEL P1 " _
               & "   ON    P1.CAMPCODE    = A.CAMPCODE " _
               & "   and   P1.UORG        = A.SHIPORG " _
               & "   and   P1.MODELPATTERN= '1' " _
               & "   and   P1.TODOKECODE  = A.TODOKECODE " _
               & "   and   P1.DELFLG     <> '1' " _
               & "   and   A.WORKKBN      = 'B3' " _
               & " LEFT JOIN MC012_MODEL P2 " _
               & "   ON    P2.CAMPCODE    = A.CAMPCODE " _
               & "   and   P2.UORG        = A.SHIPORG " _
               & "   and   P2.MODELPATTERN= '2' " _
               & "   and   P2.SHUKABASHO  = A.SHUKABASHO " _
               & "   and   P2.TODOKECODE  = A.TODOKECODE " _
               & "   and   P2.DELFLG     <> '1' " _
               & "   and   A.WORKKBN      = 'B3' " _
               & " LEFT JOIN MC012_MODEL P3 " _
               & "   ON    P3.CAMPCODE    = A.CAMPCODE " _
               & "   and   P3.UORG        = A.SHIPORG " _
               & "   and   P3.MODELPATTERN= '3' " _
               & "   and   P3.SHUKABASHO  = A.SHUKABASHO " _
               & "   and   P3.DELFLG     <> '1' " _
               & "   and   A.WORKKBN      = 'B2' " _
               & " LEFT JOIN MD002_PRODORG MD21 " _
               & "   ON    MD21.CAMPCODE    = A.CAMPCODE " _
               & "   and   MD21.UORG        = A.SHIPORG " _
               & "   and   'B3'             = A.WORKKBN " _
               & "   and   MD21.PRODUCTCODE = A.CAMPCODE + A.OILTYPE1 + A.PRODUCT11 + A.PRODUCT21 " _
               & "   and   MD21.STYMD      <= @P5 " _
               & "   and   MD21.ENDYMD     >= @P5 " _
               & "   and   MD21.DELFLG     <> '1' " _
               & " LEFT JOIN MD002_PRODORG MD22 " _
               & "   ON    MD22.CAMPCODE    = A.CAMPCODE " _
               & "   and   MD22.UORG        = A.SHIPORG " _
               & "   and   'B2'             = A.WORKKBN " _
               & "   and   MD22.PRODUCTCODE = A.CAMPCODE + A.OILTYPE1 + A.PRODUCT11 + A.PRODUCT21 " _
               & "   and   MD22.STYMD      <= @P5 " _
               & "   and   MD22.ENDYMD     >= @P5 " _
               & "   and   MD22.DELFLG     <> '1' " _
               & " WHERE   X.TERMID      = @P6 " _
               & "   and   X.CAMPCODE    = @P1 " _
               & "   and   X.OBJECT      = 'SRVORG' " _
               & "   and   X.STYMD      <= @P5 " _
               & "   and   X.ENDYMD     >= @P5 " _
               & "   and   X.DELFLG     <> '1' " _
               & " ORDER BY A.YMD , A.STAFFCODE , A.STDATE , A.STTIME, A.ENDDATE , A.ENDTIME"

            SQLStr = SQLStr & SQLStr1 & SQLStr2 & SQLStr3
            Dim SQLcmd As New SqlCommand(SQLStr, SQLcon)
            Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.NVarChar)
            Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.NVarChar)
            Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.Date)
            Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", System.Data.SqlDbType.Date)
            Dim PARA5 As SqlParameter = SQLcmd.Parameters.Add("@P5", System.Data.SqlDbType.Date)
            Dim PARA6 As SqlParameter = SQLcmd.Parameters.Add("@P6", System.Data.SqlDbType.NVarChar)
            Dim PARA7 As SqlParameter = SQLcmd.Parameters.Add("@P7", System.Data.SqlDbType.NVarChar)
            Dim PARA8 As SqlParameter = SQLcmd.Parameters.Add("@P8", System.Data.SqlDbType.NVarChar)
            PARA1.Value = work.WF_T7SEL_CAMPCODE.Text
            PARA2.Value = work.WF_T7SEL_HORG.Text
            PARA3.Value = iYmdTo
            PARA4.Value = iYmdFrom
            PARA5.Value = Date.Now
            PARA6.Value = CS0050SESSION.APSV_ID
            PARA7.Value = iSTAFFCODE
            PARA8.Value = iNIPPOLINKCODE
            Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

            '■テーブル検索結果をテーブル退避
            oTbl.Load(SQLdr)

            For i As Integer = 0 To oTbl.Rows.Count - 1
                T0005row = oTbl.Rows(i)
                T0005row("SELECT") = "1"

                If IsDate(T0005row("YMD")) Then
                    T0005row("YMD") = CDate(T0005row("YMD")).ToString("yyyy/MM/dd")
                Else
                    T0005row("YMD") = DBNull.Value
                End If
                If IsDate(T0005row("STDATE")) Then
                    T0005row("STDATE") = CDate(T0005row("STDATE")).ToString("yyyy/MM/dd")
                Else
                    T0005row("STDATE") = DBNull.Value
                End If
                If IsDate(T0005row("STTIME")) Then
                    T0005row("STTIME") = CDate(T0005row("STTIME")).ToString("HH:mm")
                Else
                    T0005row("STTIME") = DBNull.Value
                End If
                If IsDate(T0005row("ENDDATE")) Then
                    T0005row("ENDDATE") = CDate(T0005row("ENDDATE")).ToString("yyyy/MM/dd")
                Else
                    T0005row("ENDDATE") = DBNull.Value
                End If
                If IsDate(T0005row("ENDTIME")) Then
                    T0005row("ENDTIME") = CDate(T0005row("ENDTIME")).ToString("HH:mm")
                Else
                    T0005row("ENDTIME") = DBNull.Value
                End If
                T0005row("SOUDISTANCE") = Int(T0005row("SOUDISTANCE"))

            Next

            SQLdr.Dispose() 'Reader(Close)
            SQLdr = Nothing

            SQLcmd.Dispose()
            SQLcmd = Nothing

            SQLcon.Close() 'DataBase接続(Close)
            SQLcon.Dispose()
            SQLcon = Nothing

        Catch ex As Exception
            CS0011LOGWRITE.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:T0005_NIPPO Select"            '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                  '
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            oRtn = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try

    End Sub

    ' ***  モデル距離取得
    Protected Sub MODELget(ByVal iSHUKABASHO As String,
                           ByVal iTODOKECODE As String,
                           ByRef oMODEL As String,
                           ByRef oRtn As String)
        oRtn = C_MESSAGE_NO.NORMAL
        If String.IsNullOrEmpty(iSHUKABASHO) AndAlso String.IsNullOrEmpty(iTODOKECODE) Then
            oMODEL = "0"
            Exit Sub
        End If

        Try
            Dim WW_MC012tbl As DataTable = New DataTable

            WW_MC012tbl.Columns.Add("MODEL", GetType(String))

            Dim SQLStr As String = ""
            'DataBase接続文字
            Dim SQLcon As SqlConnection = CS0050SESSION.getConnection
            SQLcon.Open() 'DataBase接続(Open)

            '検索SQL文
            SQLStr =
                 " select isnull(MODEL,'0') as MODEL " _
               & "  from  MC012_MODEL A " _
               & " where  CAMPCODE      =    @CAMPCODE " _
               & "   and  UORG          =    @UORG " _
               & "   and  MODELPATTERN  =    @MODELPATTERN " _
               & "   and  SHUKABASHO    like @SHUKABASHO " _
               & "   and  TODOKECODE    like @TODOKECODE " _
               & "   and  DELFLG        <>   '1'  "

            Dim SQLcmd As New SqlCommand(SQLStr, SQLcon)

            Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@CAMPCODE", System.Data.SqlDbType.NVarChar)
            Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@UORG", System.Data.SqlDbType.NVarChar)
            Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@MODELPATTERN", System.Data.SqlDbType.NVarChar)
            Dim PARA04 As SqlParameter = SQLcmd.Parameters.Add("@SHUKABASHO", System.Data.SqlDbType.NVarChar)
            Dim PARA05 As SqlParameter = SQLcmd.Parameters.Add("@TODOKECODE", System.Data.SqlDbType.NVarChar)

            PARA01.Value = work.WF_T7SEL_CAMPCODE.Text
            PARA02.Value = work.WF_T7SEL_HORG.Text
            '出荷場所でモデル距離取得
            If Not String.IsNullOrEmpty(iSHUKABASHO) AndAlso String.IsNullOrEmpty(iTODOKECODE) Then
                PARA03.Value = "3"
                PARA04.Value = iSHUKABASHO
                PARA05.Value = "%"
            End If
            '届先でモデル距離取得
            If String.IsNullOrEmpty(iSHUKABASHO) AndAlso Not String.IsNullOrEmpty(iTODOKECODE) Then
                PARA03.Value = "1"
                PARA04.Value = "%"
                PARA05.Value = iTODOKECODE
            End If
            '出荷場所、届先でモデル距離取得
            If Not String.IsNullOrEmpty(iSHUKABASHO) AndAlso Not String.IsNullOrEmpty(iTODOKECODE) Then
                PARA03.Value = "2"
                PARA04.Value = iSHUKABASHO
                PARA05.Value = iTODOKECODE
            End If

            '■SQL実行
            Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

            WW_MC012tbl.Load(SQLdr)

            oMODEL = "0"
            For Each MC12row As DataRow In WW_MC012tbl.Rows
                oMODEL = MC12row("MODEL")
            Next

            SQLdr.Close()
            SQLdr = Nothing

            SQLcmd.Dispose()
            SQLcmd = Nothing

            SQLcon.Close() 'DataBase接続(Close)
            SQLcon.Dispose()
            SQLcon = Nothing

            WW_MC012tbl.Dispose()
            WW_MC012tbl = Nothing
        Catch ex As Exception
            CS0011LOGWRITE.INFSUBCLASS = "MC012_MODEL"                'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "MC012_MODEL SELECT"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力

            oRtn = C_MESSAGE_NO.DB_ERROR
            Exit Sub

        End Try


    End Sub

    ' ***  休憩・配送時間管理テーブル取得
    Protected Sub T0013get(ByRef oT7Tbl As DataTable,
                           ByVal iWORKKBN As String,
                           ByRef oRtn As String)
        oRtn = C_MESSAGE_NO.NORMAL

        Try
            Dim T00013tbl As DataTable = New DataTable

            Dim SQLStr As String = ""
            'DataBase接続文字
            Dim SQLcon As SqlConnection = CS0050SESSION.getConnection
            SQLcon.Open() 'DataBase接続(Open)

            '検索SQL文
            SQLStr =
                 " select " _
               & "   isnull(STTIME01,'00:00')  as STTIME01 " _
               & "  ,isnull(ENDTIME01,'00:00') as ENDTIME01 " _
               & "  ,isnull(STTIME02,'00:00')  as STTIME02 " _
               & "  ,isnull(ENDTIME02,'00:00') as ENDTIME02 " _
               & "  ,isnull(STTIME03,'00:00')  as STTIME03 " _
               & "  ,isnull(ENDTIME03,'00:00') as ENDTIME03 " _
               & "  ,isnull(STTIME04,'00:00')  as STTIME04 " _
               & "  ,isnull(ENDTIME04,'00:00') as ENDTIME04 " _
               & "  ,isnull(STTIME05,'00:00')  as STTIME05 " _
               & "  ,isnull(ENDTIME05,'00:00') as ENDTIME05 " _
               & "  ,isnull(STTIME06,'00:00')  as STTIME06 " _
               & "  ,isnull(ENDTIME06,'00:00') as ENDTIME06 " _
               & "  ,isnull(STTIME07,'00:00')  as STTIME07 " _
               & "  ,isnull(ENDTIME07,'00:00') as ENDTIME07 " _
               & "  ,isnull(STTIME08,'00:00')  as STTIME08 " _
               & "  ,isnull(ENDTIME08,'00:00') as ENDTIME08 " _
               & "  ,isnull(STTIME09,'00:00')  as STTIME09 " _
               & "  ,isnull(ENDTIME09,'00:00') as ENDTIME09 " _
               & "  ,isnull(STTIME10,'00:00')  as STTIME10 " _
               & "  ,isnull(ENDTIME10,'00:00') as ENDTIME10 " _
               & "  ,isnull(TTLTIME,'00:00')   as TTLTIME " _
               & "  from  T0013_TIMEMANAGE A " _
               & " where  CAMPCODE      =    @CAMPCODE " _
               & "   and  TAISHOYM      =    @TAISHOYM " _
               & "   and  STAFFCODE     =    @STAFFCODE " _
               & "   and  WORKDATE      =    @WORKDATE " _
               & "   and  WORKKBN       =    @WORKKBN " _
               & "   and  DELFLG        <>   '1'  "

            Dim SQLcmd As New SqlCommand(SQLStr, SQLcon)

            Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@CAMPCODE", System.Data.SqlDbType.NVarChar)
            Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@TAISHOYM", System.Data.SqlDbType.NVarChar)
            Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@STAFFCODE", System.Data.SqlDbType.NVarChar)
            Dim PARA04 As SqlParameter = SQLcmd.Parameters.Add("@WORKDATE", System.Data.SqlDbType.NVarChar)
            Dim PARA05 As SqlParameter = SQLcmd.Parameters.Add("@WORKKBN", System.Data.SqlDbType.NVarChar)

            PARA01.Value = work.WF_T7SEL_CAMPCODE.Text
            PARA02.Value = work.WF_T7SEL_TAISHOYM.Text
            PARA03.Value = WF_STAFFCODE.Text
            PARA04.Value = WF_WORKDATE.Text
            PARA05.Value = iWORKKBN

            '■SQL実行
            Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                '○ フィールド名とフィールドの型を取得
                For index As Integer = 0 To SQLdr.FieldCount - 1
                    T00013tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                Next

                '○ テーブル検索結果をテーブル格納
                T00013tbl.Load(SQLdr)
            End Using

            If iWORKKBN = "BB" Then
                For Each WW_HEADrow As DataRow In oT7Tbl.Rows
                    If WW_HEADrow("HDKBN") = "H" AndAlso WW_HEADrow("RECODEKBN") = "0" Then
                    Else
                        Continue For
                    End If

                    If T00013tbl.Rows.Count > 0 Then
                        For i As Integer = 1 To 10
                            Dim WW_BBSTTIME As String = "T13BBSTTIME" & i.ToString("00")
                            Dim WW_BBENDTIME As String = "T13BBENDTIME" & i.ToString("00")
                            Dim T13_BBSTTIME As String = "STTIME" & i.ToString("00")
                            Dim T13_BBENDTIME As String = "ENDTIME" & i.ToString("00")

                            WW_HEADrow(WW_BBSTTIME) = CDate(T00013tbl.Rows(0)(T13_BBSTTIME).hours & ":" & T00013tbl.Rows(0)(T13_BBSTTIME).minutes).ToString("HH:mm")
                            WW_HEADrow(WW_BBENDTIME) = CDate(T00013tbl.Rows(0)(T13_BBENDTIME).hours & ":" & T00013tbl.Rows(0)(T13_BBENDTIME).minutes).ToString("HH:mm")
                        Next
                        WW_HEADrow("T13BBTTLTIME") = T00013tbl.Rows(0)("TTLTIME")
                    End If
                Next
            End If
            If iWORKKBN = "G1" Then
                For Each WW_HEADrow As DataRow In oT7Tbl.Rows
                    If WW_HEADrow("HDKBN") = "H" AndAlso WW_HEADrow("RECODEKBN") = "0" Then
                    Else
                        Continue For
                    End If

                    If T00013tbl.Rows.Count > 0 Then
                        For i As Integer = 1 To 10
                            Dim WW_G1STTIME As String = "T13G1STTIME" & i.ToString("00")
                            Dim WW_G1ENDTIME As String = "T13G1ENDTIME" & i.ToString("00")
                            Dim T13_G1STTIME As String = "STTIME" & i.ToString("00")
                            Dim T13_G1ENDTIME As String = "ENDTIME" & i.ToString("00")

                            WW_HEADrow(WW_G1STTIME) = CDate(T00013tbl.Rows(0)(T13_G1STTIME).hours & ":" & T00013tbl.Rows(0)(T13_G1STTIME).minutes).ToString("HH:mm")
                            WW_HEADrow(WW_G1ENDTIME) = CDate(T00013tbl.Rows(0)(T13_G1ENDTIME).hours & ":" & T00013tbl.Rows(0)(T13_G1ENDTIME).minutes).ToString("HH:mm")
                        Next
                        WW_HEADrow("T13G1TTLTIME") = T00013tbl.Rows(0)("TTLTIME")
                    End If
                Next
            End If


            SQLcmd.Dispose()
            SQLcmd = Nothing

            SQLcon.Close() 'DataBase接続(Close)
            SQLcon.Dispose()
            SQLcon = Nothing

        Catch ex As Exception
            CS0011LOGWRITE.INFSUBCLASS = "T0013_TIMEMANAGE"                'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "T0013_TIMEMANAGE SELECT"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力

            oRtn = C_MESSAGE_NO.DB_ERROR
            Exit Sub

        End Try


    End Sub

    ' ******************************************************************************
    ' ***  日報を取得し作業区分（その他）レコード作成
    ' ***  ※１．始業、終業レコードを追加する
    ' ***  　２．日報が複数存在する場合（車両の乗り換）、乗り換の間にその他作業レコードを追加する
    ' ******************************************************************************
    Public Sub CreWORKKBN(ByRef ioTbl As DataTable, ByRef iT0005tbl As DataTable, ByVal iSTDATE As String, ByVal iENDDATE As String)
        Dim WW_WORKTIME As Integer = 0
        Dim WW_IDX As Integer = 0
        Dim WW_T0007tbl As DataTable = ioTbl.Clone
        Dim WW_T0007row As DataRow
        Dim WW_TIME As String = ""
        Dim WW_DATE_SV As String = ""
        Dim WW_TIME_SV As String = ""
        Dim WW_date As DateTime = Nothing

        '削除レコードを取得
        Dim WW_T0007DELtbl As DataTable = New DataTable
        CS0026TblSort.TABLE = ioTbl
        CS0026TblSort.FILTER = "SELECT = '0'"
        CS0026TblSort.SORTING = "STAFFCODE, WORKDATE, STDATE, STTIME, ENDDATE, ENDTIME"
        WW_T0007DELtbl = CS0026TblSort.Sort()

        '勤怠のヘッダレコードを取得
        Dim WW_T0007HEADtbl As DataTable = New DataTable
        CS0026TblSort.TABLE = ioTbl
        CS0026TblSort.FILTER = "SELECT = '1' and HDKBN = 'H'"
        CS0026TblSort.SORTING = "STAFFCODE, WORKDATE, STDATE, STTIME, ENDDATE, ENDTIME"
        WW_T0007HEADtbl = CS0026TblSort.Sort()

        '勤怠の明細レコードを取得
        Dim WW_T0007DTLtbl As DataTable = New DataTable
        CS0026TblSort.TABLE = ioTbl
        CS0026TblSort.FILTER = "SELECT = '1' and HDKBN = 'D'"
        CS0026TblSort.SORTING = "STAFFCODE, WORKDATE, STDATE, STTIME, ENDDATE, ENDTIME"
        WW_T0007DTLtbl = CS0026TblSort.Sort()

        '日報の変更を同一従業員の合計レコード（ヘッダ、明細）に反映
        '従業員+日付+レコード区分でソート
        CS0026TblSort.TABLE = WW_T0007HEADtbl
        CS0026TblSort.FILTER = ""
        CS0026TblSort.SORTING = "STAFFCODE, WORKDATE, RECODEKBN"
        WW_T0007HEADtbl = CS0026TblSort.Sort()

        Dim wSTATUS As String = ""
        For Each WW_HEADrow As DataRow In WW_T0007HEADtbl.Rows

            If WW_HEADrow("RECODEKBN") = "2" Then
                WW_HEADrow("STATUS") = wSTATUS
                wSTATUS = ""
            Else
                If (WW_HEADrow("STATUS") Like "*日報取込*" And wSTATUS = "") Or (WW_HEADrow("STATUS") Like "*日報変更*") Or (WW_HEADrow("STATUS") Like "*休憩*") Then
                    wSTATUS = WW_HEADrow("STATUS")
                End If
            End If
        Next
        CS0026TblSort.TABLE = WW_T0007HEADtbl
        CS0026TblSort.FILTER = ""
        CS0026TblSort.SORTING = "STAFFCODE, WORKDATE, STDATE, STTIME, ENDDATE, ENDTIME"
        WW_T0007HEADtbl = CS0026TblSort.Sort()

        '日報変更が発生した場合、作成済日報情報(DTL)を削除
        '　　（日報変更が発生したデータは始業（A1）、終業（Z1）、その他（BX）を再作成する。よって既存のデータから除外）
        WW_IDX = 0
        For Each WW_HEADrow As DataRow In WW_T0007HEADtbl.Rows
            If WW_HEADrow("STATUS") Like "*日報変更*" Or WW_HEADrow("STATUS") Like "*休憩*" Then
                Dim WW_MATCH As String = "OFF"
                For i As Integer = WW_IDX To WW_T0007DTLtbl.Rows.Count - 1
                    Dim WW_DTLrow As DataRow = WW_T0007DTLtbl.Rows(i)
                    If WW_HEADrow("WORKDATE") = WW_DTLrow("WORKDATE") And
                       WW_HEADrow("STAFFCODE") = WW_DTLrow("STAFFCODE") Then
                        WW_DTLrow("STATUS") = WW_HEADrow("STATUS")
                        WW_MATCH = "ON"
                    Else
                        If WW_MATCH = "ON" Then
                            WW_IDX = i
                            Exit For
                        End If
                    End If
                Next
            End If
        Next
        CS0026TblSort.TABLE = WW_T0007DTLtbl
        CS0026TblSort.FILTER = "STATUS = ''"
        CS0026TblSort.SORTING = "STAFFCODE, WORKDATE, STDATE, STTIME, ENDDATE, ENDTIME"
        WW_T0007DTLtbl = CS0026TblSort.Sort()

        'T5準備
        Dim iT0005view As DataView
        iT0005view = New DataView(iT0005tbl)
        iT0005view.Sort = "YMD, STAFFCODE"

        'T7準備
        Dim iT0007view As DataView
        iT0007view = New DataView(WW_T0007HEADtbl)
        iT0007view.Sort = "WORKDATE, STAFFCODE"
        iT0007view.RowFilter = "RECODEKBN ='0' and STATUS <> '' and WORKDATE >= #" & iSTDATE & "# and WORKDATE <= #" & iENDDATE & "#"
        Dim wT0007tbl As DataTable = iT0007view.ToTable

        'T7ディテイル作成
        Dim WW_BREAKTIME As Integer = 0
        Dim WW_SEQ As Integer = 0
        For Each WW_HEADrow As DataRow In wT0007tbl.Rows
            Dim WW_NIPPONO As String = ""
            Dim WW_A1CNT As Integer = 0
            Dim WW_F1CNT As Integer = 0
            Dim WW_LATITUDE As String = ""
            Dim WW_LONGITUDE As String = ""

            WW_BREAKTIME = 0
            WW_SEQ = 0

            iT0005view.RowFilter = "YMD = #" & WW_HEADrow("WORKDATE") & "# and STAFFCODE ='" & WW_HEADrow("STAFFCODE") & "'"
            Dim T0005tbl As DataTable = iT0005view.ToTable()
            '該当する日報を抽出し、新しいテーブルを作成する

            Dim WW_WORKKBN As String = ""
            For i As Integer = 0 To T0005tbl.Rows.Count - 1
                Dim WW_NIPPOrow As DataRow = T0005tbl.Rows(i)

                If WW_NIPPOrow("WORKKBN") = "A1" And WW_A1CNT = 0 Then
                    WW_A1CNT += 1
                    '--------------------------------------------------------------------------------
                    '始業レコード作成
                    '--------------------------------------------------------------------------------
                    WW_T0007row = WW_T0007tbl.NewRow
                    T0007COM.INProw_Init(work.WF_T7SEL_CAMPCODE.Text, WW_T0007row)

                    '開始日時、前のレコードの終了日時
                    WW_T0007row("STDATE") = WW_NIPPOrow("STDATE")
                    WW_T0007row("STTIME") = WW_NIPPOrow("STTIME")
                    '終了日時、後ろレコードの開始日時
                    WW_T0007row("ENDDATE") = WW_NIPPOrow("STDATE")
                    WW_T0007row("ENDTIME") = WW_NIPPOrow("STTIME")

                    'その他の項目は、現在のレコードをコピーする
                    WW_T0007row("WORKDATE") = WW_HEADrow("WORKDATE")
                    WW_T0007row("STAFFCODE") = WW_HEADrow("STAFFCODE")
                    WW_T0007row("STAFFKBN") = WW_HEADrow("STAFFKBN")
                    WW_T0007row("OPERATION") = WW_HEADrow("OPERATION")
                    WW_T0007row("STATUS") = WW_HEADrow("STATUS")
                    WW_T0007row("TAISHOYM") = WW_HEADrow("TAISHOYM")
                    WW_T0007row("MORG") = WW_HEADrow("MORG")
                    WW_T0007row("HORG") = WW_HEADrow("HORG")
                    WW_T0007row("SORG") = WW_NIPPOrow("SHIPORG")
                    WW_SEQ += 1
                    WW_T0007row("SEQ") = WW_SEQ
                    WW_T0007row("WORKINGWEEK") = WW_HEADrow("WORKINGWEEK")
                    WW_T0007row("HIDDEN") = "1"
                    WW_T0007row("HDKBN") = "D"
                    WW_T0007row("DATAKBN") = "K"
                    WW_T0007row("RECODEKBN") = "0"
                    WW_T0007row("WORKKBN") = "A1"
                    '作業時間
                    WW_WORKTIME = DateDiff("n",
                                          WW_T0007row("STDATE") + " " + WW_T0007row("STTIME"),
                                          WW_T0007row("ENDDATE") + " " + WW_T0007row("ENDTIME")
                                         )
                    WW_T0007row("WORKTIME") = T0007COM.formatHHMM(WW_WORKTIME)
                    WW_T0007row("ACTTIME") = T0007COM.formatHHMM(WW_WORKTIME)

                    WW_T0007row("CAMPNAMES") = ""
                    CODENAME_get("CAMPCODE", WW_T0007row("CAMPCODE"), WW_T0007row("CAMPNAMES"), WW_DUMMY)
                    WW_T0007row("WORKKBNNAMES") = ""
                    CODENAME_get("WORKKBN", WW_T0007row("WORKKBN"), WW_T0007row("WORKKBNNAMES"), WW_DUMMY)
                    WW_T0007row("STAFFNAMES") = ""
                    CODENAME_get("STAFFCODE", WW_T0007row("STAFFCODE"), WW_T0007row("STAFFNAMES"), WW_DUMMY)
                    WW_T0007row("HOLIDAYKBNNAMES") = ""
                    CODENAME_get("HOLIDAYKBN", WW_T0007row("HOLIDAYKBN"), WW_T0007row("HOLIDAYKBNNAMES"), WW_DUMMY)
                    WW_T0007row("PAYKBNNAMES") = ""
                    CODENAME_get("PAYKBN", WW_T0007row("PAYKBN"), WW_T0007row("PAYKBNNAMES"), WW_DUMMY)
                    WW_T0007row("SHUKCHOKKBNNAMES") = ""
                    CODENAME_get("SHUKCHOKKBN", WW_T0007row("SHUKCHOKKBN"), WW_T0007row("SHUKCHOKKBNNAMES"), WW_DUMMY)
                    WW_T0007row("MORGNAMES") = ""
                    CODENAME_get("HORG", WW_T0007row("MORG"), WW_T0007row("MORGNAMES"), WW_DUMMY)
                    WW_T0007row("HORGNAMES") = ""
                    CODENAME_get("HORG", WW_T0007row("HORG"), WW_T0007row("HORGNAMES"), WW_DUMMY)
                    WW_T0007row("SORGNAMES") = ""
                    CODENAME_get("HORG", WW_T0007row("SORG"), WW_T0007row("SORGNAMES"), WW_DUMMY)
                    WW_T0007tbl.Rows.Add(WW_T0007row)

                    WW_DATE_SV = WW_T0007row("ENDDATE")
                    WW_TIME_SV = WW_T0007row("ENDTIME")
                    WW_WORKKBN = "A1"
                    Continue For
                End If

                If WW_NIPPOrow("WORKKBN") = "F1" Then
                    WW_F1CNT += 1
                    '直前がA1（出社の場合）
                    If WW_WORKKBN = "A1" Then

                        If WW_NIPPOrow("STDATE") = WW_DATE_SV And
                           WW_NIPPOrow("STTIME") = WW_TIME_SV Then
                        Else
                            '--------------------------------------------------------------------------------
                            '他作業レコード作成
                            '--------------------------------------------------------------------------------
                            WW_T0007row = WW_T0007tbl.NewRow
                            T0007COM.INProw_Init(work.WF_T7SEL_CAMPCODE.Text, WW_T0007row)

                            '開始日時、前のレコードの終了日時
                            WW_T0007row("STDATE") = WW_DATE_SV
                            WW_T0007row("STTIME") = WW_TIME_SV
                            '終了日時、後ろレコードの開始日時
                            WW_T0007row("ENDDATE") = WW_NIPPOrow("STDATE")
                            WW_T0007row("ENDTIME") = WW_NIPPOrow("STTIME")

                            'その他の項目は、現在のレコードをコピーする
                            WW_T0007row("WORKDATE") = WW_HEADrow("WORKDATE")
                            WW_T0007row("STAFFCODE") = WW_HEADrow("STAFFCODE")
                            WW_T0007row("STAFFKBN") = WW_HEADrow("STAFFKBN")
                            WW_T0007row("OPERATION") = WW_HEADrow("OPERATION")
                            WW_T0007row("STATUS") = WW_HEADrow("STATUS")
                            WW_T0007row("TAISHOYM") = WW_HEADrow("TAISHOYM")
                            WW_T0007row("MORG") = WW_HEADrow("MORG")
                            WW_T0007row("HORG") = WW_HEADrow("HORG")
                            WW_T0007row("SORG") = WW_NIPPOrow("SHIPORG")
                            WW_SEQ += 1
                            WW_T0007row("SEQ") = WW_SEQ
                            WW_T0007row("WORKINGWEEK") = WW_HEADrow("WORKINGWEEK")
                            WW_T0007row("HIDDEN") = "1"
                            WW_T0007row("HDKBN") = "D"
                            WW_T0007row("DATAKBN") = "K"
                            WW_T0007row("RECODEKBN") = "0"
                            WW_T0007row("WORKKBN") = "BX"

                            '作業時間
                            WW_WORKTIME = DateDiff("n",
                                                  WW_T0007row("STDATE") + " " + WW_T0007row("STTIME"),
                                                  WW_T0007row("ENDDATE") + " " + WW_T0007row("ENDTIME")
                                                 )
                            WW_T0007row("WORKTIME") = T0007COM.formatHHMM(WW_WORKTIME)
                            WW_T0007row("ACTTIME") = T0007COM.formatHHMM(WW_WORKTIME)

                            WW_T0007row("CAMPNAMES") = ""
                            CODENAME_get("CAMPCODE", WW_T0007row("CAMPCODE"), WW_T0007row("CAMPNAMES"), WW_DUMMY)
                            WW_T0007row("WORKKBNNAMES") = ""
                            CODENAME_get("WORKKBN", WW_T0007row("WORKKBN"), WW_T0007row("WORKKBNNAMES"), WW_DUMMY)
                            WW_T0007row("STAFFNAMES") = ""
                            CODENAME_get("STAFFCODE", WW_T0007row("STAFFCODE"), WW_T0007row("STAFFNAMES"), WW_DUMMY)
                            WW_T0007row("HOLIDAYKBNNAMES") = ""
                            CODENAME_get("HOLIDAYKBN", WW_T0007row("HOLIDAYKBN"), WW_T0007row("HOLIDAYKBNNAMES"), WW_DUMMY)
                            WW_T0007row("PAYKBNNAMES") = ""
                            CODENAME_get("PAYKBN", WW_T0007row("PAYKBN"), WW_T0007row("PAYKBNNAMES"), WW_DUMMY)
                            WW_T0007row("SHUKCHOKKBNNAMES") = ""
                            CODENAME_get("SHUKCHOKKBN", WW_T0007row("SHUKCHOKKBN"), WW_T0007row("SHUKCHOKKBNNAMES"), WW_DUMMY)
                            WW_T0007row("MORGNAMES") = ""
                            CODENAME_get("HORG", WW_T0007row("MORG"), WW_T0007row("MORGNAMES"), WW_DUMMY)
                            WW_T0007row("HORGNAMES") = ""
                            CODENAME_get("HORG", WW_T0007row("HORG"), WW_T0007row("HORGNAMES"), WW_DUMMY)
                            WW_T0007row("SORGNAMES") = ""
                            CODENAME_get("HORG", WW_T0007row("SORG"), WW_T0007row("SORGNAMES"), WW_DUMMY)
                            WW_T0007tbl.Rows.Add(WW_T0007row)
                        End If
                        Continue For
                    End If
                End If

                If WW_NIPPOrow("WORKKBN") = "F3" Then
                    WW_NIPPONO = WW_NIPPOrow("NIPPONO")
                    WW_DATE_SV = WW_NIPPOrow("ENDDATE")
                    WW_TIME_SV = WW_NIPPOrow("ENDTIME")

                    WW_LATITUDE = WW_NIPPOrow("LATITUDE")
                    WW_LONGITUDE = WW_NIPPOrow("LONGITUDE")
                    Continue For
                End If

                '--------------------------------------------------------------------------------
                '出庫が２回目以降は、前の日報と後ろの日報の間に、その他作業レコードを作成する
                '--------------------------------------------------------------------------------
                If WW_F1CNT > 1 Then
                    If WW_NIPPOrow("WORKKBN") = "F1" Then
                        '初期化
                        WW_T0007row = WW_T0007tbl.NewRow
                        T0007COM.INProw_Init(work.WF_T7SEL_CAMPCODE.Text, WW_T0007row)

                        '開始日時、前のレコードの終了日時
                        WW_T0007row("STDATE") = WW_DATE_SV
                        WW_T0007row("STTIME") = WW_TIME_SV
                        '終了日時、後ろレコードの開始日時
                        WW_T0007row("ENDDATE") = WW_NIPPOrow("STDATE")
                        WW_T0007row("ENDTIME") = WW_NIPPOrow("STTIME")

                        'その他の項目は、現在のレコードをコピーする
                        WW_T0007row("WORKDATE") = WW_HEADrow("WORKDATE")
                        WW_T0007row("STAFFCODE") = WW_HEADrow("STAFFCODE")
                        WW_T0007row("STAFFKBN") = WW_HEADrow("STAFFKBN")
                        WW_T0007row("OPERATION") = WW_HEADrow("OPERATION")
                        WW_T0007row("STATUS") = WW_HEADrow("STATUS")
                        WW_T0007row("TAISHOYM") = WW_HEADrow("TAISHOYM")
                        WW_T0007row("MORG") = WW_HEADrow("MORG")
                        WW_T0007row("HORG") = WW_HEADrow("HORG")
                        WW_T0007row("SORG") = WW_NIPPOrow("SHIPORG")
                        WW_SEQ += 1
                        WW_T0007row("SEQ") = WW_SEQ
                        WW_T0007row("WORKINGWEEK") = WW_HEADrow("WORKINGWEEK")
                        WW_T0007row("HIDDEN") = "1"
                        WW_T0007row("HDKBN") = "D"
                        WW_T0007row("DATAKBN") = "K"
                        WW_T0007row("RECODEKBN") = "0"
                        WW_T0007row("WORKKBN") = "BX"

                        '作業時間
                        WW_WORKTIME = DateDiff("n",
                                              WW_T0007row("STDATE") + " " + WW_T0007row("STTIME"),
                                              WW_T0007row("ENDDATE") + " " + WW_T0007row("ENDTIME")
                                             )
                        WW_T0007row("WORKTIME") = T0007COM.formatHHMM(WW_WORKTIME)
                        WW_T0007row("ACTTIME") = T0007COM.formatHHMM(WW_WORKTIME)

                        WW_T0007row("CAMPNAMES") = ""
                        CODENAME_get("CAMPCODE", WW_T0007row("CAMPCODE"), WW_T0007row("CAMPNAMES"), WW_DUMMY)
                        WW_T0007row("WORKKBNNAMES") = ""
                        CODENAME_get("WORKKBN", WW_T0007row("WORKKBN"), WW_T0007row("WORKKBNNAMES"), WW_DUMMY)
                        WW_T0007row("STAFFNAMES") = ""
                        CODENAME_get("STAFFCODE", WW_T0007row("STAFFCODE"), WW_T0007row("STAFFNAMES"), WW_DUMMY)
                        WW_T0007row("HOLIDAYKBNNAMES") = ""
                        CODENAME_get("HOLIDAYKBN", WW_T0007row("HOLIDAYKBN"), WW_T0007row("HOLIDAYKBNNAMES"), WW_DUMMY)
                        WW_T0007row("PAYKBNNAMES") = ""
                        CODENAME_get("PAYKBN", WW_T0007row("PAYKBN"), WW_T0007row("PAYKBNNAMES"), WW_DUMMY)
                        WW_T0007row("SHUKCHOKKBNNAMES") = ""
                        CODENAME_get("SHUKCHOKKBN", WW_T0007row("SHUKCHOKKBN"), WW_T0007row("SHUKCHOKKBNNAMES"), WW_DUMMY)
                        WW_T0007row("MORGNAMES") = ""
                        CODENAME_get("HORG", WW_T0007row("MORG"), WW_T0007row("MORGNAMES"), WW_DUMMY)
                        WW_T0007row("HORGNAMES") = ""
                        CODENAME_get("HORG", WW_T0007row("HORG"), WW_T0007row("HORGNAMES"), WW_DUMMY)
                        WW_T0007row("SORGNAMES") = ""
                        CODENAME_get("HORG", WW_T0007row("SORG"), WW_T0007row("SORGNAMES"), WW_DUMMY)
                        WW_T0007tbl.Rows.Add(WW_T0007row)
                    End If
                End If

                WW_WORKKBN = ""
            Next
            '最終レコードの追加
            If T0005tbl.Rows.Count > 0 Then
                If T0005COM.ShakoCheck(WF_CAMPCODE.Text, WW_LATITUDE, WW_LONGITUDE) = "OK" Then
                    '--------------------------------------------------------------------------------
                    '車庫に帰ってきたら、他作業（＋１０分）レコード作成（最後のデータ）
                    '--------------------------------------------------------------------------------
                    WW_T0007row = WW_T0007tbl.NewRow
                    T0007COM.INProw_Init(work.WF_T7SEL_CAMPCODE.Text, WW_T0007row)

                    '開始日時、前のレコードの終了日時
                    WW_T0007row("STDATE") = T0005tbl.Rows(T0005tbl.Rows.Count - 1)("ENDDATE")
                    WW_T0007row("STTIME") = T0005tbl.Rows(T0005tbl.Rows.Count - 1)("ENDTIME")
                    '拘束時間（＋１０分）
                    WW_date = CDate(WW_T0007row("STDATE") & " " & WW_T0007row("STTIME"))
                    WW_T0007row("ENDDATE") = WW_date.AddMinutes(10).ToString("yyyy/MM/dd")
                    WW_T0007row("ENDTIME") = WW_date.AddMinutes(10).ToString("HH:mm")

                    'その他の項目は、現在のレコードをコピーする
                    WW_T0007row("WORKDATE") = WW_HEADrow("WORKDATE")
                    WW_T0007row("STAFFCODE") = WW_HEADrow("STAFFCODE")
                    WW_T0007row("STAFFKBN") = WW_HEADrow("STAFFKBN")
                    WW_T0007row("OPERATION") = WW_HEADrow("OPERATION")
                    WW_T0007row("STATUS") = WW_HEADrow("STATUS")
                    WW_T0007row("TAISHOYM") = WW_HEADrow("TAISHOYM")
                    WW_T0007row("MORG") = WW_HEADrow("MORG")
                    WW_T0007row("HORG") = WW_HEADrow("HORG")
                    WW_T0007row("SORG") = T0005tbl.Rows(T0005tbl.Rows.Count - 1)("SHIPORG")
                    WW_SEQ += 1
                    WW_T0007row("SEQ") = WW_SEQ
                    WW_T0007row("WORKINGWEEK") = WW_HEADrow("WORKINGWEEK")
                    WW_T0007row("HIDDEN") = "1"
                    WW_T0007row("HDKBN") = "D"
                    WW_T0007row("DATAKBN") = "K"
                    WW_T0007row("RECODEKBN") = "0"
                    WW_T0007row("WORKKBN") = "BX"
                    WW_T0007row("DELFLG") = "0"

                    '作業時間
                    WW_WORKTIME = DateDiff("n",
                                          WW_T0007row("STDATE") + " " + WW_T0007row("STTIME"),
                                          WW_T0007row("ENDDATE") + " " + WW_T0007row("ENDTIME")
                                         )
                    WW_T0007row("WORKTIME") = T0007COM.formatHHMM(WW_WORKTIME)
                    WW_T0007row("ACTTIME") = T0007COM.formatHHMM(WW_WORKTIME)

                    WW_T0007row("CAMPNAMES") = ""
                    CODENAME_get("CAMPCODE", WW_T0007row("CAMPCODE"), WW_T0007row("CAMPNAMES"), WW_DUMMY)
                    WW_T0007row("WORKKBNNAMES") = ""
                    CODENAME_get("WORKKBN", WW_T0007row("WORKKBN"), WW_T0007row("WORKKBNNAMES"), WW_DUMMY)
                    WW_T0007row("STAFFNAMES") = ""
                    CODENAME_get("STAFFCODE", WW_T0007row("STAFFCODE"), WW_T0007row("STAFFNAMES"), WW_DUMMY)
                    WW_T0007row("HOLIDAYKBNNAMES") = ""
                    CODENAME_get("HOLIDAYKBN", WW_T0007row("HOLIDAYKBN"), WW_T0007row("HOLIDAYKBNNAMES"), WW_DUMMY)
                    WW_T0007row("PAYKBNNAMES") = ""
                    CODENAME_get("PAYKBN", WW_T0007row("PAYKBN"), WW_T0007row("PAYKBNNAMES"), WW_DUMMY)
                    WW_T0007row("SHUKCHOKKBNNAMES") = ""
                    CODENAME_get("SHUKCHOKKBN", WW_T0007row("SHUKCHOKKBN"), WW_T0007row("SHUKCHOKKBNNAMES"), WW_DUMMY)
                    WW_T0007row("MORGNAMES") = ""
                    CODENAME_get("HORG", WW_T0007row("MORG"), WW_T0007row("MORGNAMES"), WW_DUMMY)
                    WW_T0007row("HORGNAMES") = ""
                    CODENAME_get("HORG", WW_T0007row("HORG"), WW_T0007row("HORGNAMES"), WW_DUMMY)
                    WW_T0007row("SORGNAMES") = ""
                    CODENAME_get("HORG", WW_T0007row("SORG"), WW_T0007row("SORGNAMES"), WW_DUMMY)
                    WW_T0007tbl.Rows.Add(WW_T0007row)

                    WW_DATE_SV = WW_T0007row("ENDDATE")
                    WW_TIME_SV = WW_T0007row("ENDTIME")
                End If
                '--------------------------------------------------------------------------------
                '終業レコード作成（最後のデータ）
                '--------------------------------------------------------------------------------
                WW_T0007row = WW_T0007tbl.NewRow
                T0007COM.INProw_Init(work.WF_T7SEL_CAMPCODE.Text, WW_T0007row)

                '開始日時、前のレコードの終了日時
                WW_T0007row("STDATE") = WW_DATE_SV
                WW_T0007row("STTIME") = WW_TIME_SV
                '終了日時、後ろレコードの開始日時
                WW_T0007row("ENDDATE") = WW_DATE_SV
                WW_T0007row("ENDTIME") = WW_TIME_SV

                'その他の項目は、現在のレコードをコピーする
                WW_T0007row("WORKDATE") = WW_HEADrow("WORKDATE")
                WW_T0007row("STAFFCODE") = WW_HEADrow("STAFFCODE")
                WW_T0007row("STAFFKBN") = WW_HEADrow("STAFFKBN")
                WW_T0007row("OPERATION") = WW_HEADrow("OPERATION")
                WW_T0007row("STATUS") = WW_HEADrow("STATUS")
                WW_T0007row("TAISHOYM") = WW_HEADrow("TAISHOYM")
                WW_T0007row("MORG") = WW_HEADrow("MORG")
                WW_T0007row("HORG") = WW_HEADrow("HORG")
                WW_T0007row("SORG") = T0005tbl.Rows(T0005tbl.Rows.Count - 1)("SHIPORG")
                WW_SEQ += 1
                WW_T0007row("SEQ") = WW_SEQ
                WW_T0007row("WORKINGWEEK") = WW_HEADrow("WORKINGWEEK")
                WW_T0007row("HIDDEN") = "1"
                WW_T0007row("HDKBN") = "D"
                WW_T0007row("DATAKBN") = "K"
                WW_T0007row("RECODEKBN") = "0"
                WW_T0007row("WORKKBN") = "Z1"
                WW_T0007row("DELFLG") = "0"

                '作業時間
                WW_WORKTIME = DateDiff("n",
                                      WW_T0007row("STDATE") + " " + WW_T0007row("STTIME"),
                                      WW_T0007row("ENDDATE") + " " + WW_T0007row("ENDTIME")
                                     )
                WW_T0007row("WORKTIME") = T0007COM.formatHHMM(WW_WORKTIME)
                WW_T0007row("ACTTIME") = T0007COM.formatHHMM(WW_WORKTIME)

                WW_T0007row("CAMPNAMES") = ""
                CODENAME_get("CAMPCODE", WW_T0007row("CAMPCODE"), WW_T0007row("CAMPNAMES"), WW_DUMMY)
                WW_T0007row("WORKKBNNAMES") = ""
                CODENAME_get("WORKKBN", WW_T0007row("WORKKBN"), WW_T0007row("WORKKBNNAMES"), WW_DUMMY)
                WW_T0007row("STAFFNAMES") = ""
                CODENAME_get("STAFFCODE", WW_T0007row("STAFFCODE"), WW_T0007row("STAFFNAMES"), WW_DUMMY)
                WW_T0007row("HOLIDAYKBNNAMES") = ""
                CODENAME_get("HOLIDAYKBN", WW_T0007row("HOLIDAYKBN"), WW_T0007row("HOLIDAYKBNNAMES"), WW_DUMMY)
                WW_T0007row("PAYKBNNAMES") = ""
                CODENAME_get("PAYKBN", WW_T0007row("PAYKBN"), WW_T0007row("PAYKBNNAMES"), WW_DUMMY)
                WW_T0007row("SHUKCHOKKBNNAMES") = ""
                CODENAME_get("SHUKCHOKKBN", WW_T0007row("SHUKCHOKKBN"), WW_T0007row("SHUKCHOKKBNNAMES"), WW_DUMMY)
                WW_T0007row("MORGNAMES") = ""
                CODENAME_get("HORG", WW_T0007row("MORG"), WW_T0007row("MORGNAMES"), WW_DUMMY)
                WW_T0007row("HORGNAMES") = ""
                CODENAME_get("HORG", WW_T0007row("HORG"), WW_T0007row("HORGNAMES"), WW_DUMMY)
                WW_T0007row("SORGNAMES") = ""
                CODENAME_get("HORG", WW_T0007row("SORG"), WW_T0007row("SORGNAMES"), WW_DUMMY)
                WW_T0007tbl.Rows.Add(WW_T0007row)
            End If
        Next


        ioTbl = WW_T0007DELtbl.Copy
        ioTbl.Merge(WW_T0007HEADtbl)
        ioTbl.Merge(WW_T0007DTLtbl)
        ioTbl.Merge(WW_T0007tbl)

        'ソート
        CS0026TblSort.TABLE = ioTbl
        CS0026TblSort.FILTER = ""
        CS0026TblSort.SORTING = "STAFFCODE, WORKDATE, STDATE, STTIME, HDKBN DESC, ENDDATE, ENDTIME, WORKKBN"
        ioTbl = CS0026TblSort.Sort()

        WW_T0007DELtbl.Dispose()
        WW_T0007DELtbl = Nothing
        WW_T0007HEADtbl.Dispose()
        WW_T0007HEADtbl = Nothing
        WW_T0007DTLtbl.Dispose()
        WW_T0007DTLtbl = Nothing
        WW_T0007tbl.Dispose()
        WW_T0007tbl = Nothing
        iT0005view.Dispose()
        iT0005view = Nothing
        iT0007view.Dispose()
        iT0007view = Nothing
        wT0007tbl.Dispose()
        wT0007tbl = Nothing

    End Sub

    ' ***  ヘッダレコード編集
    Public Sub HeadEdit(ByRef ioTbl As DataTable, ByRef iT0005tbl As DataTable, ByVal iSTDATE As String, ByVal iENDDATE As String, ByRef iMODELtbl As DataTable)

        Dim WW_LINECNT As Integer = 0
        Dim WW_IDX As Integer = 0
        Dim WW_SUISOKBN As String = ""

        'T5準備
        Dim iT0005view As DataView = New DataView(iT0005tbl)
        iT0005view.Sort = "YMD, STAFFCODE, STDATE, STTIME, ENDDATE, ENDTIME, SEQ"

        '削除レコードを取得
        Dim WW_T0007DELtbl As DataTable = New DataTable
        CS0026TblSort.TABLE = ioTbl
        CS0026TblSort.FILTER = "SELECT = '0'"
        CS0026TblSort.SORTING = "STAFFCODE, WORKDATE, STDATE, STTIME, ENDDATE, ENDTIME"
        WW_T0007DELtbl = CS0026TblSort.Sort()

        '勤怠のヘッダレコードを取得
        Dim WW_T0007HEADtbl As DataTable = New DataTable
        CS0026TblSort.TABLE = ioTbl
        CS0026TblSort.FILTER = "SELECT = '1' and HDKBN = 'H'"
        CS0026TblSort.SORTING = "STAFFCODE, WORKDATE, STDATE, STTIME, ENDDATE, ENDTIME"
        WW_T0007HEADtbl = CS0026TblSort.Sort()

        '勤怠の明細レコードを取得
        Dim WW_T0007DTLtbl As DataTable = New DataTable
        CS0026TblSort.TABLE = ioTbl
        CS0026TblSort.FILTER = "SELECT = '1' and HDKBN = 'D'"
        CS0026TblSort.SORTING = "STAFFCODE, WORKDATE, STDATE, STTIME, ENDDATE, ENDTIME"
        WW_T0007DTLtbl = CS0026TblSort.Sort()

        Dim iT0007view As DataView = New DataView(WW_T0007DTLtbl)

        '勤怠ヘッダの集計
        WW_IDX = 0
        For Each WW_HEADrow As DataRow In WW_T0007HEADtbl.Rows
            If (WW_HEADrow("STATUS") Like "*日報*" Or WW_HEADrow("STATUS") Like "*休憩*") And WW_HEADrow("RECODEKBN") = "0" And
               WW_HEADrow("WORKDATE") >= iSTDATE And WW_HEADrow("WORKDATE") <= iENDDATE Then
            Else
                Continue For
            End If


            '日報取得
            '該当する日報を抽出し、新しいテーブルを作成する
            'T7準備
            iT0007view.Sort = "STDATE, STTIME, ENDDATE, ENDTIME, WORKKBN"
            iT0007view.RowFilter = "WORKDATE = #" & WW_HEADrow("WORKDATE") & "# and STAFFCODE ='" & WW_HEADrow("STAFFCODE") & "' and RECODEKBN = '0'"

            Dim WW_BREAKTIME As Integer = 0
            Dim WW_MATCH As String = "OFF"
            Dim WW_G1 As String = "OFF"
            Dim WW_F1 As String = "OFF"
            Dim WW_F3 As String = "OFF"
            Dim WW_LATITUDE_F1 As String = ""
            Dim WW_LONGITUDE_F1 As String = ""
            Dim WW_LATITUDE_F3 As String = ""
            Dim WW_LONGITUDE_F3 As String = ""
            Dim WW_A1sttime As String = ""

            '勤怠レコードの必要情報からヘッダを集計
            For i As Integer = WW_IDX To iT0007view.Count - 1
                Dim WW_DTLrow As DataRow = iT0007view.Item(i).Row

                WW_DTLrow("PAYKBN") = "00"          '勤怠区分：通常
                If WW_DTLrow("WORKKBN") = "A1" Then
                    '出社レコードより開始日、開始時間を取得
                    WW_HEADrow("STDATE") = WW_DTLrow("STDATE")
                    WW_HEADrow("STTIME") = T0007COM.Minute30Edit(WW_DTLrow("STTIME"))
                    WW_A1sttime = WW_DTLrow("STTIME")
                End If


                '2020/11/17 UPD
                'If WW_DTLrow("WORKKBN") = "G1" And WW_G1 = "OFF" And WW_F3 = "OFF" Then
                '    '最初の配送ボタン取得
                '    If T0005COM.ShakoCheck(WF_CAMPCODE.Text, WW_LATITUDE_F1, WW_LONGITUDE_F1) = "OK" Then
                '        '配送ボタンで車庫出発の場合、配送開始-１０分
                '        Dim WW_date As DateTime = CDate(WW_DTLrow("STDATE") & " " & WW_DTLrow("STTIME"))
                '        WW_HEADrow("STDATE") = WW_date.AddMinutes(-10).ToString("yyyy/MM/dd")
                '        WW_HEADrow("STTIME") = WW_date.AddMinutes(-10).ToString("HH:mm")
                '    Else
                '        '配送ボタンで車庫以外出発の場合、配送開始そのまま
                '        WW_HEADrow("STDATE") = WW_DTLrow("STDATE")
                '        WW_HEADrow("STTIME") = WW_DTLrow("STTIME")
                '    End If
                '    WW_G1 = "ON"
                'End If
                '2020/11/17 UPD END

                If WW_DTLrow("WORKKBN") = "Z1" Then
                    '退社レコードの終了日、終了時間を取得
                    WW_HEADrow("ENDDATE") = WW_DTLrow("ENDDATE")
                    WW_HEADrow("ENDTIME") = WW_DTLrow("ENDTIME")
                End If

                '2020/11/17 UPD
                'If WW_DTLrow("WORKKBN") = "BB" Then
                '    '休憩レコードを取得
                '    WW_BREAKTIME += T0007COM.HHMMtoMinutes(WW_DTLrow("BREAKTIME"))
                'End If

                'If WW_DTLrow("WORKKBN") = "F3" Then
                '    WW_F3 = "ON"
                '    '最後の帰庫の緯度経度を取得
                '    WW_LATITUDE_F3 = WW_DTLrow("LATITUDE")
                '    WW_LONGITUDE_F3 = WW_DTLrow("LONGITUDE")
                'End If
                '2020/11/17 UPD END
                WW_MATCH = "ON"
            Next
            'iT0007view.Dispose()
            'iT0007view = Nothing

            '日報取得
            '該当する日報を抽出し、新しいテーブルを作成する
            iT0005view.RowFilter = "YMD = #" & WW_HEADrow("WORKDATE") & "# and STAFFCODE ='" & WW_HEADrow("STAFFCODE") & "' and WORKKBN in ('F1','F3','B3','B2')"
            Dim T0005tbl As DataTable = iT0005view.ToTable()

            Dim WW_BREAKTIME2 As Integer = 0
            Dim WW_HAISO As Integer = 0
            Dim WW_KAISO As Integer = 0
            Dim WW_B2CNT As Integer = 0
            Dim WW_B3CNT As Integer = 0
            '日報レコードの必要情報からヘッダを集計
            For i As Integer = 0 To T0005tbl.Rows.Count - 1
                Dim WW_NIPPOrow As DataRow = T0005tbl.Rows(i)

                If WW_NIPPOrow("WORKKBN") = "F1" And WW_F1 = "OFF" Then
                    '最初の出庫の緯度経度を取得
                    WW_LATITUDE_F1 = WW_NIPPOrow("LATITUDE")
                    WW_LONGITUDE_F1 = WW_NIPPOrow("LONGITUDE")

                    '配送開始01、配送終了01が00:00以外の場合、配送ボタンONとなる
                    WW_HEADrow("HAISOMINUS10FLG") = "OFF"
                    If WW_HEADrow("T13G1STTIME01") <> "00:00" AndAlso WW_HEADrow("T13G1ENDTIME01") <> "00:00" Then
                        Dim WW_date As DateTime = CDate(WW_HEADrow("STDATE") & " " & WW_A1sttime)
                        Dim WW_dateG1 As DateTime = CDate(WW_HEADrow("STDATE") & " " & WW_HEADrow("T13G1STTIME01"))
                        Dim WW_dateAdd60 As DateTime = WW_date.AddMinutes(60)
                        '出庫時刻から60分以内であれば、配送作業（グループ作業ではない）とする
                        If WW_dateG1 <= WW_dateAdd60 Then
                            If T0005COM.ShakoCheck(work.WF_T7SEL_CAMPCODE.Text, WW_LATITUDE_F1, WW_LONGITUDE_F1) = "OK" Then
                                '配送ボタンで車庫出発の場合、配送開始-１０分
                                WW_date = CDate(WW_HEADrow("STDATE") & " " & WW_HEADrow("T13G1STTIME01"))
                                WW_HEADrow("STDATE") = WW_date.AddMinutes(-10).ToString("yyyy/MM/dd")
                                WW_HEADrow("STTIME") = WW_date.AddMinutes(-10).ToString("HH:mm")
                                WW_HEADrow("T13G1STTIME01") = WW_HEADrow("STTIME")
                                WW_HEADrow("T13G1TTLTIME") += 10
                                WW_HEADrow("HAISOMINUS10FLG") = "ON"
                            Else
                                '配送ボタンで車庫以外出発の場合、配送開始そのまま
                                WW_HEADrow("STDATE") = WW_HEADrow("STDATE")
                                WW_HEADrow("STTIME") = WW_HEADrow("T13G1STTIME01")
                            End If
                        End If
                    Else
                        '配送ボタン以外（グループ作業）の場合、上記のA1で決定した30分編集となる
                    End If

                    WW_F1 = "ON"
                End If

                If WW_NIPPOrow("WORKKBN") = "F3" Then
                    WW_F3 = "ON"
                    '最後の帰庫の緯度経度を取得
                    WW_LATITUDE_F3 = WW_NIPPOrow("LATITUDE")
                    WW_LONGITUDE_F3 = WW_NIPPOrow("LONGITUDE")

                    '帰庫（F3）に持っている総走行キロを取得
                    If WW_NIPPOrow("L1KAISO") = "回送" And WW_NIPPOrow("SUISOKBN") <> "1" Then
                        WW_KAISO = WW_KAISO + WW_NIPPOrow("SOUDISTANCE")
                    Else
                        WW_HAISO = WW_HAISO + WW_NIPPOrow("SOUDISTANCE")
                    End If
                End If

                '2020/11/17 UPD
                'If WW_NIPPOrow("WORKKBN") = "BB" Then
                '    '休憩（BB）レコードの作業時間（休憩時間）を全て加算
                '    WW_BREAKTIME2 += WW_NIPPOrow("WORKTIME")
                'End If
                '2020/11/17 UPD END
                If WW_NIPPOrow("WORKKBN") = "B3" Then
                    If WW_NIPPOrow("SUISOKBN") <> "1" Then
                        '荷卸（B3）をカウントする（水素はカウントしない）
                        WW_B3CNT += 1
                    End If
                End If
                If WW_NIPPOrow("WORKKBN") = "B2" Then
                    '荷積（B2）積置きをカウントする
                    WW_B2CNT += 1
                End If
                '2020/11/17 UPD
                'WW_HEADrow("NIPPOLINKCODE") = WW_NIPPOrow("UPDYMD")
                WW_HEADrow("NIPPOLINKCODE") = WW_NIPPOrow("UPDYMD") & "_" & WW_HEADrow("T13ENTRYDATE")
                '2020/11/17 UPD END
            Next

            If WW_MATCH = "ON" Then
                '2020/11/17 UPD
                'WW_HEADrow("BREAKTIME") = T0007COM.formatHHMM(WW_BREAKTIME)
                'WW_HEADrow("BREAKTIMETTL") = T0007COM.formatHHMM(WW_BREAKTIME)
                WW_HEADrow("BREAKTIME") = "00:00"
                WW_HEADrow("BREAKTIMETTL") = T0007COM.formatHHMM(WW_HEADrow("T13BBTTLTIME"))
                '2020/11/17 UPD END
                WW_HEADrow("BINDSTDATE") = WW_HEADrow("STTIME")
                If IsDBNull(WW_HEADrow("STDATE")) Or
                    IsDBNull(WW_HEADrow("ENDDATE")) Or
                    IsDBNull(WW_HEADrow("STTIME")) Or
                    IsDBNull(WW_HEADrow("ENDTIME")) Then
                    WW_HEADrow("WORKTIME") = T0007COM.formatHHMM(0)
                    WW_HEADrow("ACTTIME") = T0007COM.formatHHMM(0)
                Else
                    Dim WW_WORKTIME As Integer = 0
                    WW_WORKTIME = DateDiff("n",
                                         WW_HEADrow("STDATE") + " " + WW_HEADrow("STTIME"),
                                         WW_HEADrow("ENDDATE") + " " + WW_HEADrow("ENDTIME")
                                        )
                    WW_HEADrow("WORKTIME") = T0007COM.formatHHMM(WW_WORKTIME)
                    WW_HEADrow("ACTTIME") = T0007COM.formatHHMM(WW_WORKTIME)
                End If

                '出庫が車庫で、帰庫が車庫以外の場合、車中泊１泊目
                WW_HEADrow("SHACHUHAKKBN") = "0"
                WW_HEADrow("SHACHUHAKKBNNAMES") = ""
                If T0005COM.ShakoCheck(WF_CAMPCODE.Text, WW_LATITUDE_F1, WW_LONGITUDE_F1) = "OK" Then
                    If T0005COM.ShakoCheck(WF_CAMPCODE.Text, WW_LATITUDE_F3, WW_LONGITUDE_F3) = "OK" Then
                        '出庫、帰庫が車庫の場合、車中泊ではない
                    Else
                        '出庫が車庫で、帰庫が車庫以外の場合、車中泊１泊目
                        WW_HEADrow("SHACHUHAKKBN") = "1"
                        WW_HEADrow("SHACHUHAKKBNNAMES") = "✔"
                    End If
                End If
            End If

            '日報の休憩
            WW_HEADrow("NIPPOBREAKTIME") = T0007COM.formatHHMM(WW_HEADrow("T13BBTTLTIME"))

            WW_HEADrow("UNLOADCNT") = WW_B3CNT
            WW_HEADrow("UNLOADCNTTTL") = WW_B3CNT
            WW_HEADrow("KAIDISTANCE") = WW_KAISO
            WW_HEADrow("KAIDISTANCETTL") = WW_KAISO + WW_HEADrow("KAIDISTANCECHO")
            WW_HEADrow("HAIDISTANCE") = WW_HAISO
            WW_HEADrow("HAIDISTANCETTL") = WW_HAISO + WW_HEADrow("HAIDISTANCECHO")
            WW_HEADrow("MODELDISTANCE") = 0
            WW_HEADrow("MODELDISTANCETTL") = 0

            For i As Integer = 0 To iMODELtbl.Rows.Count - 1
                Dim WW_MODELrow As DataRow = iMODELtbl.Rows(i)
                If WW_HEADrow("WORKDATE") = WW_MODELrow("WORKDATE") And
                   WW_HEADrow("STAFFCODE") = WW_MODELrow("STAFFCODE") Then
                    WW_HEADrow("T10SAVECNT") = WW_MODELrow("SAVECNT")
                    For j As Integer = 1 To 6
                        Dim WW_SHARYOKBN As String = "SHARYOKBN" & j.ToString
                        Dim WW_OILPAYKBN As String = "OILPAYKBN" & j.ToString
                        Dim WW_SHUKABASHO As String = "SHUKABASHO" & j.ToString
                        Dim WW_TODOKECODE As String = "TODOKECODE" & j.ToString
                        Dim WW_MODELDISTANCE As String = "MODELDISTANCE" & j.ToString
                        Dim WW_MODIFYKBN As String = "MODIFYKBN" & j.ToString

                        Dim WW_T10SHARYOKBN As String = "T10SHARYOKBN" & j.ToString
                        Dim WW_T10OILPAYKBN As String = "T10OILPAYKBN" & j.ToString
                        Dim WW_T10SHUKABASHO As String = "T10SHUKABASHO" & j.ToString
                        Dim WW_T10TODOKECODE As String = "T10TODOKECODE" & j.ToString
                        Dim WW_T10MODELDISTANCE As String = "T10MODELDISTANCE" & j.ToString
                        Dim WW_T10MODIFYKBN As String = "T10MODIFYKBN" & j.ToString
                        WW_HEADrow(WW_T10SHARYOKBN) = WW_MODELrow(WW_SHARYOKBN)
                        WW_HEADrow(WW_T10OILPAYKBN) = WW_MODELrow(WW_OILPAYKBN)
                        WW_HEADrow(WW_T10SHUKABASHO) = WW_MODELrow(WW_SHUKABASHO)
                        WW_HEADrow(WW_T10TODOKECODE) = WW_MODELrow(WW_TODOKECODE)
                        WW_HEADrow(WW_T10MODELDISTANCE) = WW_MODELrow(WW_MODELDISTANCE)
                        WW_HEADrow(WW_T10MODIFYKBN) = WW_MODELrow(WW_MODIFYKBN)

                        'If WW_HEADrow("SHACHUHAKKBN") = "1" AndAlso
                        '   WW_MODELrow(WW_SHUKABASHO) <> "" AndAlso
                        '   WW_MODELrow(WW_TODOKECODE) = "" Then
                        '    WW_HEADrow(WW_T10SHARYOKBN) = ""
                        '    WW_HEADrow(WW_T10OILPAYKBN) = ""
                        '    WW_HEADrow(WW_T10SHUKABASHO) = ""
                        '    WW_HEADrow(WW_T10TODOKECODE) = ""
                        '    WW_HEADrow(WW_T10MODELDISTANCE) = 0
                        '    WW_HEADrow(WW_T10MODIFYKBN) = "0"
                        'End If
                    Next
                End If
            Next
        Next

        '勤怠ヘッダのコピー
        ioTbl = WW_T0007HEADtbl.Copy

        '勤怠明細のマージ
        ioTbl.Merge(WW_T0007DTLtbl)

        '更新元（削除）データの戻し
        ioTbl.Merge(WW_T0007DELtbl)

        WW_T0007HEADtbl.Dispose()
        WW_T0007HEADtbl = Nothing
        WW_T0007DTLtbl.Dispose()
        WW_T0007DTLtbl = Nothing
        WW_T0007DELtbl.Dispose()
        WW_T0007DELtbl = Nothing

        iT0005view.Dispose()
        iT0005view = Nothing
        iT0007view.Dispose()
        iT0007view = Nothing
    End Sub


    ' ***  ヘッダレコード編集
    Public Sub BindStDateSet(ByRef ioTbl As DataTable, ByRef iT7Tbl As DataTable, ByVal iSTDATE As String, ByVal iENDDATE As String)

        Dim WW_LINECNT As Integer = 0
        Dim WW_IDX As Integer = 0
        Dim WW_SUISOKBN As String = ""

        CS0026TblSort.TABLE = ioTbl
        CS0026TblSort.FILTER = ""
        CS0026TblSort.SORTING = "SELECT, STAFFCODE, WORKDATE, HDKBN DESC, STDATE, STTIME, ENDDATE, ENDTIME"
        ioTbl = CS0026TblSort.Sort()

        Dim WW_T0007DELtbl As DataTable = ioTbl.Clone
        Dim WW_T0007HEADtbl As DataTable = ioTbl.Clone
        Dim WW_T0007DTLtbl As DataTable = ioTbl.Clone
        For i As Integer = 0 To ioTbl.Rows.Count - 1
            Dim ioTblrow As DataRow = ioTbl.Rows(i)

            '削除レコードを取得
            If ioTblrow("SELECT") = "0" Then
                Dim DELrow As DataRow = WW_T0007DELtbl.NewRow
                DELrow.ItemArray = ioTblrow.ItemArray
                WW_T0007DELtbl.Rows.Add(DELrow)
            End If

            '勤怠のヘッダレコードを取得
            If ioTblrow("SELECT") = "1" And ioTblrow("HDKBN") = "H" Then
                Dim HEADrow As DataRow = WW_T0007HEADtbl.NewRow
                HEADrow.ItemArray = ioTblrow.ItemArray
                WW_T0007HEADtbl.Rows.Add(HEADrow)
            End If

            '勤怠の明細レコードを取得
            If ioTblrow("SELECT") = "1" And ioTblrow("HDKBN") = "D" Then
                Dim DTLrow As DataRow = WW_T0007DTLtbl.NewRow
                DTLrow.ItemArray = ioTblrow.ItemArray
                WW_T0007DTLtbl.Rows.Add(DTLrow)
            End If
        Next


        '勤怠のヘッダレコードを取得
        '前月
        Dim WW_ZDAtE As String = CDate(iSTDATE).AddMonths(-1).ToString("yyyy/MM")
        Dim WW_TDAtE As String = CDate(iSTDATE).ToString("yyyy/MM")

        Dim WW_T0007HEADtbl2 As DataTable = New DataTable
        Dim WW_T0007HEADtbl3 As DataTable = New DataTable
        '前月分は、SELECT='0'（対象外）HIDDEN='1'で登録されている
        Dim WW_Filter As String = "HDKBN = 'H' and RECODEKBN = '0' and TAISHOYM = '" & WW_ZDAtE & "'"
        CS0026TblSort.TABLE = iT7Tbl
        CS0026TblSort.FILTER = WW_Filter
        CS0026TblSort.SORTING = "STAFFCODE, WORKDATE, STDATE, STTIME, ENDDATE, ENDTIME"
        WW_T0007HEADtbl2 = CS0026TblSort.Sort()

        WW_Filter = "SELECT = '1' and HDKBN = 'H' and RECODEKBN = '0' and TAISHOYM = '" & WW_TDAtE & "'"

        CS0026TblSort.TABLE = iT7Tbl
        CS0026TblSort.FILTER = WW_Filter
        CS0026TblSort.SORTING = "STAFFCODE, WORKDATE, STDATE, STTIME, ENDDATE, ENDTIME"
        WW_T0007HEADtbl3 = CS0026TblSort.Sort()

        '前月＋当月
        WW_T0007HEADtbl2.Merge(WW_T0007HEADtbl3)

        '直前、翌日取得用VIEW
        Dim iT0007view As DataView
        iT0007view = New DataView(WW_T0007HEADtbl2)
        iT0007view.Sort = "STAFFCODE, WORKDATE, STDATE, STTIME, ENDDATE, ENDTIME"

        '勤怠ヘッダの集計

        WW_IDX = 0
        For Each WW_HEADrow As DataRow In WW_T0007HEADtbl.Rows
            If (WW_HEADrow("STATUS") Like "*日報*" Or WW_HEADrow("STATUS") Like "*休憩*") And WW_HEADrow("RECODEKBN") = "0" And
               WW_HEADrow("WORKDATE") >= iSTDATE And WW_HEADrow("WORKDATE") <= iENDDATE Then
            Else
                Continue For
            End If

            '直前の勤務
            Dim WW_ZENFLG As String = "OFF"
            Dim WW_ZENFLG2 As String = "OFF"
            Dim dt As Date = CDate(WW_HEADrow("WORKDATE"))
            Dim WW_ZENDATE As String = dt.AddDays(-1).ToString("yyyy/MM/dd")

            iT0007view.RowFilter = "STAFFCODE = '" & WW_HEADrow("STAFFCODE") & "' and WORKDATE = #" & WW_ZENDATE & "#"
            If iT0007view.Count > 0 Then
                '前日が休みか判定
                If T0007COM.CheckHOLIDAY(iT0007view.Item(0).Row("HOLIDAYKBN"), iT0007view.Item(0).Row("PAYKBN")) Then
                    '1:法定休日、2:法定外休日
                    '01:年休, 02 : 特休, 04 : ｽﾄｯｸ, 05 : 協約週休, 06 : 週休
                    '07:傷欠, 08 : 組欠, 09 : 他欠, 11 : 代休, 13 : 指定休, 15 : 振休

                    '稼動しているか判定
                    If Val(iT0007view.Item(0).Row("ACTTIME")) = 0 Then
                        '休みで、稼働なし
                        WW_ZENFLG = "ON"
                    End If
                End If
            End If

            '前日が休みで稼働なしの場合、前々日を確認
            If WW_ZENFLG = "ON" Then
                '前々日以前を検索
                WW_ZENDATE = dt.AddDays(-2).ToString("yyyy/MM/dd")
                iT0007view.RowFilter = "STAFFCODE = '" & WW_HEADrow("STAFFCODE") & "' and WORKDATE = #" & WW_ZENDATE & "#"
                If iT0007view.Count > 0 Then
                    '前日が休みか判定
                    If T0007COM.CheckHOLIDAY(iT0007view.Item(0).Row("HOLIDAYKBN"), iT0007view.Item(0).Row("PAYKBN")) Then
                        '1:法定休日、2:法定外休日
                        '01:年休, 02 : 特休, 04 : ｽﾄｯｸ, 05 : 協約週休, 06 : 週休
                        '07:傷欠, 08 : 組欠, 09 : 他欠, 11 : 代休, 13 : 指定休, 15 : 振休

                        '稼動しているか判定
                        If Val(iT0007view.Item(0).Row("ACTTIME")) = 0 Then
                            '休みで、稼働なし
                            WW_ZENFLG2 = "ON"
                        End If
                    Else
                        '稼働日で日を跨いでいれば拘束開始を決定する
                        If iT0007view.Item(0).Row("STDATE") <> iT0007view.Item(0).Row("ENDDATE") Then
                            If WW_HEADrow("STTIME") < "08:00" Then
                                WW_HEADrow("BINDSTDATE") = "08:00"
                            End If
                        End If
                    End If
                End If
            End If

            '前々日が休みで稼働なしの場合、前々日を確認
            If WW_ZENFLG2 = "ON" Then
                '前々日以前を検索
                WW_ZENDATE = dt.AddDays(-3).ToString("yyyy/MM/dd")
                iT0007view.RowFilter = "STAFFCODE = '" & WW_HEADrow("STAFFCODE") & "' and WORKDATE = #" & WW_ZENDATE & "#"
                If iT0007view.Count > 0 Then
                    '前日が休みか判定
                    If T0007COM.CheckHOLIDAY(iT0007view.Item(0).Row("HOLIDAYKBN"), iT0007view.Item(0).Row("PAYKBN")) Then
                        '1:法定休日、2:法定外休日
                        '01:年休, 02 : 特休, 04 : ｽﾄｯｸ, 05 : 協約週休, 06 : 週休
                        '07:傷欠, 08 : 組欠, 09 : 他欠, 11 : 代休, 13 : 指定休, 15 : 振休

                        '以降は処理しない２連休までの対応とする
                    Else
                        '稼働日で日を跨いでいれば拘束開始を決定する
                        If iT0007view.Item(0).Row("STDATE") <> iT0007view.Item(0).Row("ENDDATE") Then
                            If WW_HEADrow("STTIME") < "08:00" Then
                                WW_HEADrow("BINDSTDATE") = "08:00"
                            End If
                        End If
                    End If
                End If
            End If
        Next

        '勤怠ヘッダのコピー
        ioTbl = WW_T0007HEADtbl.Copy

        '勤怠明細のマージ
        ioTbl.Merge(WW_T0007DTLtbl)

        '更新元（削除）データの戻し
        ioTbl.Merge(WW_T0007DELtbl)

        WW_T0007HEADtbl.Dispose()
        WW_T0007HEADtbl = Nothing
        WW_T0007DTLtbl.Dispose()
        WW_T0007DTLtbl = Nothing
        WW_T0007DELtbl.Dispose()
        WW_T0007DELtbl = Nothing
        WW_T0007HEADtbl2.Dispose()
        WW_T0007HEADtbl2 = Nothing
        WW_T0007HEADtbl3.Dispose()
        WW_T0007HEADtbl3 = Nothing

        iT0007view.Dispose()
        iT0007view = Nothing
    End Sub

    ' ***  休憩・配送の戻し編集
    Public Sub TimeManageGet(ByRef iTbl As DataTable, ByRef oT7Tbl As DataTable)

        T0013get(oT7Tbl, "BB", WW_DUMMY)
        T0013get(oT7Tbl, "G1", WW_DUMMY)

        'Dim WW_Tbl As DataTable = New DataTable
        'CS0026TblSort.TABLE = iTbl
        'CS0026TblSort.FILTER = "SELECT = '1' and HDKBN = 'H' and RECODEKBN = '0' and STAFFCODE = '" & WF_STAFFCODE.Text & "' and WORKDATE = #" & WF_WORKDATE.Text & "#"
        'CS0026TblSort.SORTING = "STAFFCODE, WORKDATE, STDATE, STTIME, ENDDATE, ENDTIME"
        'WW_Tbl = CS0026TblSort.Sort()

        'For Each WW_HEADrow As DataRow In oT7Tbl.Rows
        '    If WW_HEADrow("HDKBN") = "H" AndAlso WW_HEADrow("RECODEKBN") = "0" Then
        '    Else
        '        Continue For
        '    End If
        '    If WW_Tbl.Rows.Count > 0 Then
        '        For i As Integer = 1 To 10
        '            Dim WW_BBSTTIME As String = "T13BBSTTIME" & i.ToString("00")
        '            Dim WW_BBENDTIME As String = "T13BBENDTIME" & i.ToString("00")
        '            Dim WW_G1STTIME As String = "T13G1STTIME" & i.ToString("00")
        '            Dim WW_G1ENDTIME As String = "T13G1ENDTIME" & i.ToString("00")

        '            WW_HEADrow(WW_BBSTTIME) = WW_Tbl.Rows(0)(WW_BBSTTIME)
        '            WW_HEADrow(WW_BBENDTIME) = WW_Tbl.Rows(0)(WW_BBENDTIME)
        '            WW_HEADrow(WW_G1STTIME) = WW_Tbl.Rows(0)(WW_G1STTIME)
        '            WW_HEADrow(WW_G1ENDTIME) = WW_Tbl.Rows(0)(WW_G1ENDTIME)
        '        Next
        '        WW_HEADrow("T13BBTTLTIME") = WW_Tbl.Rows(0)("T13BBTTLTIME")
        '        WW_HEADrow("T13G1TTLTIME") = WW_Tbl.Rows(0)("T13G1TTLTIME")
        '    End If
        'Next

        'WW_Tbl.Dispose()
        'WW_Tbl = Nothing
    End Sub

    '★★★★★★★★★★★★★★★★★★★★★
    ' ***  共通処理                                                              
    '★★★★★★★★★★★★★★★★★★★★★

    '画面編集
    Protected Sub DisplayEdit(ByRef iTbl As DataTable)

        Dim WW_UNLOADCNT As Integer = 0
        Dim WW_HAIDISTANCE As Double = 0
        Dim WW_MODELDISTANCE0109 As Integer = 0
        Dim WW_MODELDISTANCE0204 As Integer = 0
        Dim WW_MODELDISTANCE0209 As Integer = 0
        Dim WW_WORKTIME As Integer = 0
        Dim WW_WORKTIME2 As Integer = 0
        '項番(LineCnt)設定
        Dim WW_LINECNT As Integer = 0

        For Each T0007INProw As DataRow In iTbl.Rows

            '指定日入力（編集）
            If T0007INProw("RECODEKBN") = "0" Then
                If T0007INProw("HDKBN") = "H" Then
                    WF_STATUS.Text = T0007INProw("STATUS")
                    WF_CAMPCODE.Text = T0007INProw("CAMPCODE")
                    WF_WORKDATE.Text = T0007INProw("WORKDATE")
                    WF_WORKDATE2.Text = T0007INProw("WORKDATE")
                    WF_WORKINGWEEK_TEXT.Text = T0007INProw("WORKINGWEEKNAMES")
                    WF_WORKINGWEEK2_TEXT.Text = T0007INProw("WORKINGWEEKNAMES")
                    WF_STDATE.Text = T0007INProw("STDATE")
                    WF_STTIME.Text = T0007INProw("STTIME")
                    WF_ENDDATE.Text = T0007INProw("ENDDATE")
                    WF_ENDTIME.Text = T0007INProw("ENDTIME")
                    WF_STAFFCODE.Text = T0007INProw("STAFFCODE")
                    WF_STAFFCODE_TEXT.Text = T0007INProw("STAFFNAMES")
                    WF_HORG.Text = T0007INProw("HORG")
                    WF_HORG_TEXT.Text = T0007INProw("HORGNAMES")
                    WF_HAIDISTANCE.Text = Val(T0007INProw("HAIDISTANCE")).ToString("0")
                    WF_KAIDISTANCE.Text = Val(T0007INProw("KAIDISTANCE")).ToString("0")
                    WF_ORVERTIME.Text = T0007INProw("ORVERTIMETTL")
                    WF_HWORKTIME.Text = T0007INProw("HWORKTIMETTL")
                    WF_HOLIDAYKBN.Text = T0007INProw("HOLIDAYKBN")
                    WF_HOLIDAYKBN_TEXT.Text = T0007INProw("HOLIDAYKBNNAMES")
                    WF_BINDSTDATE.Text = T0007INProw("BINDSTDATE")
                    WF_WNIGHTTIME.Text = T0007INProw("WNIGHTTIMETTL")
                    WF_HNIGHTTIME.Text = T0007INProw("HNIGHTTIMETTL")
                    WF_PAYKBN.Text = T0007INProw("PAYKBN")
                    WF_PAYKBN_TEXT.Text = T0007INProw("PAYKBNNAMES")
                    WF_BINDTIME.Text = T0007INProw("BINDTIME")
                    WF_TOKUSA1TIME.Text = T0007INProw("TOKUSA1TIMETTL")
                    WF_NIGHTTIME.Text = T0007INProw("NIGHTTIMETTL")
                    WF_SWORKTIME.Text = T0007INProw("SWORKTIMETTL")
                    WF_SHUKCHOKKBN.Text = T0007INProw("SHUKCHOKKBN")
                    WF_SHUKCHOKKBN_TEXT.Text = T0007INProw("SHUKCHOKKBNNAMES")
                    WF_SNIGHTTIME.Text = T0007INProw("SNIGHTTIMETTL")
                    '休憩時間
                    WF_NIPPOBREAKTIME.Text = T0007INProw("NIPPOBREAKTIME")
                    WF_BREAKTIME.Text = T0007INProw("BREAKTIME")
                    WF_HAISOTIME.Text = T0007INProw("HAISOTIME")
                    If T0007INProw("SHACHUHAKKBN") = "1" Then
                        WF_SHACHUHAKKBN.Checked = True
                    Else
                        WF_SHACHUHAKKBN.Checked = False
                    End If

                    WF_STAFFCODEMDL.Text = T0007INProw("STAFFCODE")
                    WF_STAFFCODEMDL_TEXT.Text = T0007INProw("STAFFNAMES")
                    WF_HORGMDL.Text = T0007INProw("HORG")
                    WF_HORGMDL_TEXT.Text = T0007INProw("HORGNAMES")

                    Dim WW_MODIFY As String = "OFF"
                    For i As Integer = 1 To 6
                        Dim WF_SHARYOKBN As String = "WF_SHARYOKBN" & i.ToString
                        Dim WF_OILPAYKBN As String = "WF_OILPAYKBN" & i.ToString
                        Dim WF_SHUKABASHO As String = "WF_SHUKABASHO" & i.ToString
                        Dim WF_TODOKECODE As String = "WF_TODOKECODE" & i.ToString
                        Dim WF_MODELDISTANCE As String = "WF_MODELDISTANCE" & i.ToString
                        Dim WF_MODIFYKBN As String = "WF_MODIFYKBN" & i.ToString

                        Dim WF_SHARYOKBN_TEXT As String = "WF_SHARYOKBN" & i.ToString & "_TEXT"
                        Dim WF_OILPAYKBN_TEXT As String = "WF_OILPAYKBN" & i.ToString & "_TEXT"
                        Dim WF_SHUKABASHO_TEXT As String = "WF_SHUKABASHO" & i.ToString & "_TEXT"
                        Dim WF_TODOKECODE_TEXT As String = "WF_TODOKECODE" & i.ToString & "_TEXT"

                        Dim WW_SHARYOKBN As String = "T10SHARYOKBN" & i.ToString
                        Dim WW_OILPAYKBN As String = "T10OILPAYKBN" & i.ToString
                        Dim WW_SHUKABASHO As String = "T10SHUKABASHO" & i.ToString
                        Dim WW_TODOKECODE As String = "T10TODOKECODE" & i.ToString
                        Dim WW_MODELDISTANCE As String = "T10MODELDISTANCE" & i.ToString
                        Dim WW_MODIFYKBN As String = "T10MODIFYKBN" & i.ToString

                        CType(WF_DView3.FindControl(WF_SHARYOKBN), System.Web.UI.WebControls.TextBox).Text = T0007INProw(WW_SHARYOKBN)
                        CODENAME_get("SHARYOKBN", T0007INProw(WW_SHARYOKBN), CType(WF_DView3.FindControl(WF_SHARYOKBN_TEXT), System.Web.UI.WebControls.Label).Text, WW_DUMMY)
                        CType(WF_DView3.FindControl(WF_OILPAYKBN), System.Web.UI.WebControls.TextBox).Text = T0007INProw(WW_OILPAYKBN)
                        CODENAME_get("OILPAYKBN", T0007INProw(WW_OILPAYKBN), CType(WF_DView3.FindControl(WF_OILPAYKBN_TEXT), System.Web.UI.WebControls.Label).Text, WW_DUMMY)
                        CType(WF_DView3.FindControl(WF_SHUKABASHO), System.Web.UI.WebControls.TextBox).Text = T0007INProw(WW_SHUKABASHO)
                        CODENAME_get("SHUKABASHO", T0007INProw(WW_SHUKABASHO), CType(WF_DView3.FindControl(WF_SHUKABASHO_TEXT), System.Web.UI.WebControls.Label).Text, WW_DUMMY)
                        CType(WF_DView3.FindControl(WF_TODOKECODE), System.Web.UI.WebControls.TextBox).Text = T0007INProw(WW_TODOKECODE)
                        CODENAME_get("TODOKECODE", T0007INProw(WW_TODOKECODE), CType(WF_DView3.FindControl(WF_TODOKECODE_TEXT), System.Web.UI.WebControls.Label).Text, WW_DUMMY)
                        CType(WF_DView3.FindControl(WF_MODELDISTANCE), System.Web.UI.WebControls.TextBox).Text = Val(T0007INProw(WW_MODELDISTANCE)).ToString("0")
                        If T0007INProw(WW_MODIFYKBN) = "1" Then
                            CType(WF_DView3.FindControl(WF_MODIFYKBN), System.Web.UI.WebControls.CheckBox).Checked = True
                            WW_MODIFY = "ON"
                        Else
                            CType(WF_DView3.FindControl(WF_MODIFYKBN), System.Web.UI.WebControls.CheckBox).Checked = False
                        End If

                        If T0007INProw(WW_SHARYOKBN) = "1" And T0007INProw(WW_OILPAYKBN) = "09" Then
                            WW_MODELDISTANCE0109 += Val(T0007INProw(WW_MODELDISTANCE))
                        End If
                        If T0007INProw(WW_SHARYOKBN) = "2" And T0007INProw(WW_OILPAYKBN) = "04" Then
                            WW_MODELDISTANCE0204 += Val(T0007INProw(WW_MODELDISTANCE))
                        End If
                        If T0007INProw(WW_SHARYOKBN) = "2" And T0007INProw(WW_OILPAYKBN) = "09" Then
                            WW_MODELDISTANCE0209 += Val(T0007INProw(WW_MODELDISTANCE))
                        End If

                    Next
                    If WW_MODIFY = "ON" Then
                        WF_MODIFY.Checked = True
                    Else
                        WF_MODIFY.Checked = False
                    End If
                    WF_MODELDISTANCE0109.Text = Val(WW_MODELDISTANCE0109).ToString("0")
                    WF_MODELDISTANCE0204.Text = Val(WW_MODELDISTANCE0204).ToString("0")
                    WF_MODELDISTANCE0209.Text = Val(WW_MODELDISTANCE0209).ToString("0")

                    '2020/11/17 ADD
                    If WF_STATUS.Text Like "*取込" Then
                        CType(WF_DView1.FindControl("WF_BBTTLTIME"), System.Web.UI.WebControls.TextBox).Text = T0007COM.formatHHMM(0)
                        CType(WF_DView1.FindControl("WF_G1TTLTIME"), System.Web.UI.WebControls.TextBox).Text = T0007COM.formatHHMM(0)
                        For i As Integer = 1 To 10
                            Dim WF_BBSTTIME As String = "WF_BBSTTIME" & i.ToString("00")
                            Dim WF_BBENDTIME As String = "WF_BBENDTIME" & i.ToString("00")
                            Dim WF_G1STTIME As String = "WF_G1STTIME" & i.ToString("00")
                            Dim WF_G1ENDTIME As String = "WF_G1ENDTIME" & i.ToString("00")

                            Dim WW_BBSTTIME As String = "T13BBSTTIME" & i.ToString("00")
                            Dim WW_BBENDTIME As String = "T13BBENDTIME" & i.ToString("00")
                            Dim WW_G1STTIME As String = "T13G1STTIME" & i.ToString("00")
                            Dim WW_G1ENDTIME As String = "T13G1ENDTIME" & i.ToString("00")

                            CType(WF_DView1.FindControl(WF_BBSTTIME), System.Web.UI.WebControls.TextBox).Text = ""
                            CType(WF_DView1.FindControl(WF_BBENDTIME), System.Web.UI.WebControls.TextBox).Text = ""
                            CType(WF_DView1.FindControl(WF_G1STTIME), System.Web.UI.WebControls.TextBox).Text = ""
                            CType(WF_DView1.FindControl(WF_G1ENDTIME), System.Web.UI.WebControls.TextBox).Text = ""
                        Next
                    Else
                        CType(WF_DView1.FindControl("WF_BBTTLTIME"), System.Web.UI.WebControls.TextBox).Text = T0007COM.formatHHMM(T0007INProw("T13BBTTLTIME"))
                        CType(WF_DView1.FindControl("WF_G1TTLTIME"), System.Web.UI.WebControls.TextBox).Text = T0007COM.formatHHMM(T0007INProw("T13G1TTLTIME"))
                        For i As Integer = 1 To 10
                            Dim WF_BBSTTIME As String = "WF_BBSTTIME" & i.ToString("00")
                            Dim WF_BBENDTIME As String = "WF_BBENDTIME" & i.ToString("00")
                            Dim WF_G1STTIME As String = "WF_G1STTIME" & i.ToString("00")
                            Dim WF_G1ENDTIME As String = "WF_G1ENDTIME" & i.ToString("00")

                            Dim WW_BBSTTIME As String = "T13BBSTTIME" & i.ToString("00")
                            Dim WW_BBENDTIME As String = "T13BBENDTIME" & i.ToString("00")
                            Dim WW_G1STTIME As String = "T13G1STTIME" & i.ToString("00")
                            Dim WW_G1ENDTIME As String = "T13G1ENDTIME" & i.ToString("00")

                            CType(WF_DView1.FindControl(WF_BBSTTIME), System.Web.UI.WebControls.TextBox).Text = T0009TIME.ZeroToSpace(T0007INProw(WW_BBSTTIME))
                            CType(WF_DView1.FindControl(WF_BBENDTIME), System.Web.UI.WebControls.TextBox).Text = T0009TIME.ZeroToSpace(T0007INProw(WW_BBENDTIME))
                            CType(WF_DView1.FindControl(WF_G1STTIME), System.Web.UI.WebControls.TextBox).Text = T0009TIME.ZeroToSpace(T0007INProw(WW_G1STTIME))
                            CType(WF_DView1.FindControl(WF_G1ENDTIME), System.Web.UI.WebControls.TextBox).Text = T0009TIME.ZeroToSpace(T0007INProw(WW_G1ENDTIME))
                        Next
                    End If
                    '2020/11/17 ADD END
                End If
            End If

            '月合計（編集）
            If T0007INProw("RECODEKBN") = "2" Then
                If T0007INProw("HDKBN") = "H" Then
                    WF_CAMPCODE.Text = T0007INProw("CAMPCODE")
                    WF_STAFFCODE.Text = T0007INProw("STAFFCODE")
                    WF_STAFFCODETTL.Text = T0007INProw("STAFFCODE") '従業員
                    WF_STAFFCODETTL_TEXT.Text = T0007INProw("STAFFNAMES") '従業員名称
                    WF_HORGTTL.Text = T0007INProw("HORG") '従業員
                    WF_HORGTTL_TEXT.Text = T0007INProw("HORGNAMES") '従業員名称
                    WF_WORKNISSUTTL.Text = T0007INProw("WORKNISSUTTL") '所労
                    WF_NENKYUNISSUTTL.Text = T0007INProw("NENKYUNISSUTTL") '年休
                    WF_KYOTEIWEEKNISSUTTL.Text = T0007INProw("KYOTEIWEEKNISSUTTL") '協約週休
                    WF_SHOUKETUNISSUTTL.Text = T0007INProw("SHOUKETUNISSUTTL") '傷欠
                    WF_TOKUKYUNISSUTTL.Text = T0007INProw("TOKUKYUNISSUTTL") '特休
                    WF_ROSAIYUKYNIUSSUTTL.Text = T0007INProw("ROSAIYUKYNIUSSUTTL") '労災
                    WF_TOKUKYUMUKYUNISSUTTL.Text = T0007INProw("TOKUKYUMUKYUNISSUTTL") '特休無給
                    WF_KOKANGOYUKYUNISSUTTL.Text = T0007INProw("KOKANGOYUKYUNISSUTTL") '子看有給
                    WF_KOKANGOMUKYUNISSUTTL.Text = T0007INProw("KOKANGOMUKYUNISSUTTL") '子看無給
                    WF_KUMIKETUNISSUTTL.Text = T0007INProw("KUMIKETUNISSUTTL") '組休
                    WF_CHIKOKSOTAINISSUTTL.Text = T0007INProw("CHIKOKSOTAINISSUTTL") '遅早
                    WF_DAIKYUNISSUTTL.Text = T0007INProw("DAIKYUNISSUTTL") '代休
                    WF_ETCKETUNISSUTTL.Text = T0007INProw("ETCKETUNISSUTTL") '他休
                    WF_STOCKNISSUTTL.Text = T0007INProw("STOCKNISSUTTL") 'ｽﾄｯｸ休暇
                    WF_NENMATUNISSUTTL.Text = T0007INProw("NENMATUNISSUTTL") '年末出勤日数
                    WF_NENSHINISSUTTL.Text = T0007INProw("NENSHINISSUTTL") '年始出勤日数
                    WF_ORVERTIMETTL.Text = T0007INProw("ORVERTIMETTL") '平日残業
                    WF_WNIGHTTIMETTL.Text = T0007INProw("WNIGHTTIMETTL") '平日深夜
                    WF_TOKUSA1TIMETTL.Text = T0007INProw("TOKUSA1TIMETTL") '特作I
                    WF_HWORKTIMETTL.Text = T0007INProw("HWORKTIMETTL") '休日出勤
                    WF_HNIGHTTIMETTL.Text = T0007INProw("HNIGHTTIMETTL") '休日深夜
                    WF_SWORKTIMETTL.Text = T0007INProw("SWORKTIMETTL") '日曜出勤
                    WF_SNIGHTTIMETTL.Text = T0007INProw("SNIGHTTIMETTL") '日曜深夜
                    WF_NIGHTTIMETTL.Text = T0007INProw("NIGHTTIMETTL") '所定深夜
                    WF_SHACHUHAKNISSUTTL.Text = T0007INProw("SHACHUHAKNISSUTTL") '車中泊
                    WF_JIKYUSHATIMETTL.Text = T0007INProw("JIKYUSHATIMETTL") '時給者作業

                    WF_MODELDISTANCETTL.Text = Val(T0007INProw("MODELDISTANCETTL")).ToString("0") '走行距離
                End If

                If T0007INProw("HDKBN") = "D" Then
                    Select Case T0007INProw("OILPAYKBN")
                        Case "04"  'ＬＮＧ
                            If T0007INProw("SHARYOKBN") = "1" Then
                                WF_MODELDISTANCE_LNG1.Text = Val(T0007INProw("MODELDISTANCETTL")).ToString("0")
                            End If
                            If T0007INProw("SHARYOKBN") = "2" Then
                                WF_MODELDISTANCE_LNG2.Text = Val(T0007INProw("MODELDISTANCETTL")).ToString("0")
                            End If
                        Case "09"  'ラテックス
                            If T0007INProw("SHARYOKBN") = "1" Then
                                WF_MODELDISTANCE_RATE1.Text = Val(T0007INProw("MODELDISTANCETTL")).ToString("0")
                            End If
                            If T0007INProw("SHARYOKBN") = "2" Then
                                WF_MODELDISTANCE_RATE2.Text = Val(T0007INProw("MODELDISTANCETTL")).ToString("0")
                            End If
                    End Select

                End If
            End If

            'SELECT=0（対象外）1（対象）、HIDDEN=0（表示）1（非表示）
            'ヘッダを非表示に（勤怠明細、日報明細をGridViewに表示する用）
            If T0007INProw("HDKBN") = "D" And T0007INProw("RECODEKBN") = "0" Then
                WW_LINECNT = WW_LINECNT + 1
                T0007INProw("LINECNT") = WW_LINECNT
                T0007INProw("SELECT") = "1"
                T0007INProw("HIDDEN") = "0"
            Else
                T0007INProw("LINECNT") = 0
                T0007INProw("SELECT") = "1"
                T0007INProw("HIDDEN") = "1"
            End If

            If T0007INProw("ORGSEQ").ToString = "" Then
                T0007INProw("ORGSEQ") = 0
            End If
        Next

    End Sub

    ''' <summary>
    ''' フィールドダブルクリック時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_FIELD_DBClick()

        '〇フィールドダブルクリック処理
        If String.IsNullOrEmpty(WF_LeftMViewChange.Value) OrElse WF_LeftMViewChange.Value = "" Then
        Else
            Try
                Integer.TryParse(WF_LeftMViewChange.Value, WF_LeftMViewChange.Value)
            Catch ex As Exception
                Exit Sub
            End Try

            With leftview
                Select Case WF_LeftMViewChange.Value
                    Case LIST_BOX_CLASSIFICATION.LC_CALENDAR
                        '日付の場合、入力日付のカレンダーが表示されるように入力値をカレンダーに渡す
                        Select Case WF_FIELD.Value
                        End Select
                        .ActiveCalendar()

                    Case Else
                        '上記以外

                        Dim prmData As New Hashtable
                        prmData.Item(C_PARAMETERS.LP_COMPANY) = work.WF_T7SEL_CAMPCODE.Text

                        'フィールドによってパラメータを変える
                        Select Case WF_FIELD.Value
                            Case "WF_STAFFCODE"         '乗務員
                                prmData = work.getStaffCodeList(work.WF_T7SEL_CAMPCODE.Text, work.WF_T7SEL_TAISHOYM.Text, work.WF_T7SEL_HORG.Text)
                            Case "WF_PAYKBN"            '勤怠区分
                                prmData = work.CreatePAYKBNParam(work.WF_T7SEL_CAMPCODE.Text)
                            Case "WF_HOLIDAYKBN"         '休日区分
                                prmData = work.CreateHOLIDAYKBNParam(work.WF_T7SEL_CAMPCODE.Text)
                            Case "WF_SHUKCHOKKBN"        '宿直区分
                                prmData = work.CreateSHUKCHOKKBNParam(work.WF_T7SEL_CAMPCODE.Text)
                            Case "WF_STAFFKBN"          '職務区分
                                prmData = work.CreateStaffKbnParam(work.WF_T7SEL_CAMPCODE.Text)
                            Case "WF_WORKKBN"           '作業区分
                                prmData.Item(C_PARAMETERS.LP_FIX_CLASS) = "WORKKBN"
                            Case "WF_TORICODE"          '取引先（マスタ）
                                prmData = work.CreateCustomerParam(work.WF_T7SEL_CAMPCODE.Text)
                            Case "WF_TODOKECODE"        '届先（マスタ）
                                prmData = work.createDistinationParam(work.WF_T7SEL_CAMPCODE.Text, "", "", "1")
                            Case "WF_SHUKABASHO"        '出荷場所（マスタ）
                                prmData = work.createDistinationParam(work.WF_T7SEL_CAMPCODE.Text, "", "", "2")
                            Case "WF_TODOKECODE1", "WF_TODOKECODE2", "WF_TODOKECODE3",
                                 "WF_TODOKECODE4", "WF_TODOKECODE5", "WF_TODOKECODE6" '届先（モデル距離）

                                Dim wkFieldName = WF_FIELD.Value.Replace("WF_TODOKECODE", "WF_SHUKABASHO")
                                Dim wkField = CType(Page.Master.FindControl("contents1").FindControl(wkFieldName), TextBox).Text

                                WF_LeftMViewChange.Value = LIST_BOX_CLASSIFICATION.LC_EXTRA_LIST
                                prmData = work.CreateModelPatternDestParam(work.WF_T7SEL_CAMPCODE.Text, work.WF_T7SEL_HORG.Text, "1", wkField)
                            Case "WF_SHUKABASHO1", "WF_SHUKABASHO2", "WF_SHUKABASHO3",
                                 "WF_SHUKABASHO4", "WF_SHUKABASHO5", "WF_SHUKABASHO6" '出荷場所（モデル距離）
                                Dim wkFieldName = WF_FIELD.Value.Replace("WF_SHUKABASHO", "WF_TODOKECODE")
                                Dim wkField = CType(Page.Master.FindControl("contents1").FindControl(wkFieldName), TextBox).Text

                                WF_LeftMViewChange.Value = LIST_BOX_CLASSIFICATION.LC_EXTRA_LIST
                                prmData = work.CreateModelPatternDestParam(work.WF_T7SEL_CAMPCODE.Text, work.WF_T7SEL_HORG.Text, "2", wkField)
                            Case "WF_SHARYOKBN", "WF_SHARYOKBN1", "WF_SHARYOKBN2", "WF_SHARYOKBN3",
                                                 "WF_SHARYOKBN4", "WF_SHARYOKBN5", "WF_SHARYOKBN6" '車輌区分
                                prmData = work.CreateSHARYOKBNParam(work.WF_T7SEL_CAMPCODE.Text)
                            Case "WF_OILPAYKBN", "WF_OILPAYKBN1", "WF_OILPAYKBN2", "WF_OILPAYKBN3",
                                                 "WF_OILPAYKBN4", "WF_OILPAYKBN5", "WF_OILPAYKBN6" '油種区分
                                prmData = work.CreateOILPAYKBNParam(work.WF_T7SEL_CAMPCODE.Text)
                        End Select

                        .SetListBox(WF_LeftMViewChange.Value, WW_DUMMY, prmData)
                        .ActiveListBox()
                End Select
            End With
        End If

    End Sub

    ' ***  GridView スクロールSUB
    Protected Sub Scrole_SUB()

        Dim WW_GridPosition As Integer                           '表示位置（開始）
        Dim WW_DataCNT As Integer = 0                            '(絞り込み後)有効Data数
        Dim WW_WORKTIME As Integer = 0
        Dim WW_WORKTIME2 As Integer = 0

        Dim t7inp = From x In T0007INPtbl.AsEnumerable()
                    Order By x.Item("STAFFCODE"), x.Item("WORKDATE"), x.Item("STDATE"), x.Item("STTIME"), x.Item("ENDDATE"), x.Item("ENDTIME"), x.Item("WORKKBN")

        'CS0026TblSort.TABLE = T0007INPtbl
        'CS0026TblSort.FILTER = ""
        'CS0026TblSort.SORTING = "STAFFCODE, WORKDATE, STDATE, STTIME, ENDDATE, ENDTIME, WORKKBN"
        'T0007INPtbl = CS0026TblSort.sort()

        '○表示Linecnt取得
        If WF_GridPosition.Text = String.Empty OrElse
            Not Integer.TryParse(WF_GridPosition.Text, WW_GridPosition) Then
            WW_GridPosition = 1
        End If

        '○画面（GridView）表示
        'Dim T0007tblGrid As New DataTable
        'T0007tblGrid = T0007INPtbl.Copy

        'Dim WW_TBLview As DataView = New DataView(T0007tblGrid)

        'ソート
        'WW_TBLview.Sort = "LINECNT"
        'WW_TBLview.RowFilter = "HIDDEN = 0 and LINECNT >= 1 "
        Dim t7view = t7inp.
                        Where(Function(x) x.Item("HIDDEN") = 0 AndAlso
                                          x.Item("LINECNT") >= 1).
                        OrderBy(Function(x) x.Item("LINECNT"))

        '○ 一覧作成
        CS0013ProfView.CAMPCODE = work.WF_T7SEL_CAMPCODE.Text
        CS0013ProfView.PROFID = Master.PROF_VIEW
        CS0013ProfView.MAPID = Master.MAPID
        CS0013ProfView.VARI = Master.VIEWID
        If t7view.Count = 0 Then
            CS0013ProfView.SRCDATA = T0007INPtbl.Clone
        Else
            CS0013ProfView.SRCDATA = t7view.CopyToDataTable
        End If
        'CS0013ProfView.SRCDATA = WW_TBLview.ToTable
        CS0013ProfView.TBLOBJ = pnlListArea
        CS0013ProfView.SCROLLTYPE = CS0013ProfView.SCROLLTYPE_ENUM.Vertical
        'CS0013ProfView.LEVENT = "ondblclick"
        'CS0013ProfView.LFUNC = "ListDbClick"
        CS0013ProfView.TITLEOPT = False
        CS0013ProfView.HIDEOPERATIONOPT = True
        CS0013ProfView.TARGETDATE = work.WF_T7SEL_TAISHOYM.Text & "/01"
        CS0013ProfView.CS0013ProfView()

        '○クリア
        WF_GridPosition.Text = "1"

        'T0007tblGrid.Dispose()
        'T0007tblGrid = Nothing
        'WW_TBLview.Dispose()
        'WW_TBLview = Nothing

    End Sub

    '★★★★★★★★★★★★★★★★★★★★★
    '共通処理部品
    '★★★★★★★★★★★★★★★★★★★★★

    ' ***  名称設定処理   LeftBoxより名称取得＆チェック
    Protected Sub CODENAME_get(ByVal I_FIELD As String, ByRef I_VALUE As String, ByRef O_TEXT As String, ByRef O_RTN As String, Optional ByVal optText1 As String = "", Optional ByVal optText2 As String = "")

        '○名称取得

        O_TEXT = ""
        O_RTN = ""

        If I_VALUE = "" Then
            O_RTN = C_MESSAGE_NO.NORMAL
            Exit Sub
        End If

        Dim prmData As New Hashtable
        prmData.Item(C_PARAMETERS.LP_COMPANY) = work.WF_T7SEL_CAMPCODE.Text

        Try
            Select Case I_FIELD

                Case "CAMPCODE"
                    '会社名
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_COMPANY, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "STAFFCODE"
                    '乗務員名
                    prmData = work.getStaffCodeList(work.WF_T7SEL_CAMPCODE.Text, work.WF_T7SEL_TAISHOYM.Text, work.WF_T7SEL_HORG.Text)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_STAFFCODE, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "ORG"
                    '出荷部署名
                    prmData = work.CreateORGParam(work.WF_T7SEL_CAMPCODE.Text)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_ORG, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "HORG"
                    '配属部署
                    prmData = work.CreateHORGParam(work.WF_T7SEL_CAMPCODE.Text)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_ORG, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "PAYKBN"
                    '勤怠区分名称
                    prmData = work.CreatePAYKBNParam(work.WF_T7SEL_CAMPCODE.Text)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "HOLIDAYKBN"
                    '休日区分名称
                    prmData = work.CreateHOLIDAYKBNParam(work.WF_T7SEL_CAMPCODE.Text)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "SHUKCHOKKBN"
                    '宿日直名称
                    prmData = work.CreateSHUKCHOKKBNParam(work.WF_T7SEL_CAMPCODE.Text)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "STAFFKBN"
                    '職務区分
                    prmData = work.CreateStaffKbnParam(work.WF_T7SEL_CAMPCODE.Text)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_EXTRA_LIST, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "WORKKBN"
                    '作業区分
                    prmData.Item(C_PARAMETERS.LP_FIX_CLASS) = "WORKKBN"
                    leftview.CodeToName(I_VALUE, O_TEXT, O_RTN, prmData)
                Case "TORICODE"
                    '取引先名称（マスタ）
                    prmData = work.CreateCustomerParam(work.WF_T7SEL_CAMPCODE.Text)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_CUSTOMER, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "TODOKECODE"
                    '届先名（マスタ）
                    prmData = work.createDistinationParam(work.WF_T7SEL_CAMPCODE.Text, optText1, optText2, "1")
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_DISTINATION, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "SHUKABASHO"
                    '出荷場所名称（マスタ）
                    prmData = work.createDistinationParam(work.WF_T7SEL_CAMPCODE.Text, optText1, optText2, "2")
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_DISTINATION, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "PRODUCT2"
                    '品名（マスタ）
                    prmData = work.CreatePRODUCTParam(work.WF_T7SEL_CAMPCODE.Text, work.WF_T7SEL_HORG.Text)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_EXTRA_LIST, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "TERMKBN"
                    '端末区分名
                    prmData = work.CreateTERMKBNParam(work.WF_T7SEL_CAMPCODE.Text)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "CREWKBN"
                    '実績登録区分名
                    prmData = work.CreateCREWKBNParam(work.WF_T7SEL_CAMPCODE.Text)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "SHARYOKBN"
                    '車輌区分名
                    prmData = work.CreateSHARYOKBNParam(work.WF_T7SEL_CAMPCODE.Text)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "OILPAYKBN"
                    '油種区分名
                    prmData = work.CreateOILPAYKBNParam(work.WF_T7SEL_CAMPCODE.Text)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "LATITUDE"
                    '緯度
                    prmData = work.CreateLATITUDEParam(work.WF_T7SEL_CAMPCODE.Text)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "LONGITUDE"
                    '軽度
                    prmData = work.CreateLONGITUDEParam(work.WF_T7SEL_CAMPCODE.Text)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "MODELCODE"
                    'モデル特殊処理コード設定
                    prmData = work.CreateMODELCODEParam(work.WF_T7SEL_CAMPCODE.Text)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "MODELDISTANCE"
                    'モデル特殊処理距離設定
                    prmData = work.CreateMODELDISTANCEParam(work.WF_T7SEL_CAMPCODE.Text)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, prmData)
            End Select
        Catch ex As Exception
            O_RTN = C_MESSAGE_NO.FILE_NOT_EXISTS_ERROR
            Exit Sub
        End Try

    End Sub

    ' ******************************************************************************
    ' ***  T0007INProwチェック
    ' ******************************************************************************
    Protected Sub T0007INProw_CHEK(ByRef RTN As String)

        '○インターフェイス初期値設定
        RTN = C_MESSAGE_NO.NORMAL

        Dim WW_RESULT As String = ""
        Dim WW_TEXT As String = ""
        Dim WW_LINEerr As String = C_MESSAGE_NO.NORMAL

        WW_ERRLIST = New List(Of String)

        '■■■ 単項目チェック(ヘッダー情報) ■■■
        CS0036FCHECK.CAMPCODE = work.WF_T7SEL_CAMPCODE.Text
        CS0036FCHECK.MAPID = GRT00007WRKINC_V2.MAPIDNJS
        CS0036FCHECK.TBL = S0013tbl

        '・キー項目(会社コード：CAMPCODE)
        '①必須・項目属性チェック
        CS0036FCHECK.FIELD = "CAMPCODE"
        CS0036FCHECK.VALUE = WF_CAMPCODE.Text
        CS0036FCHECK.CS0036FCHECK()
        If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then
            WF_CAMPCODE.Text = CS0036FCHECK.VALUEOUT
            CODENAME_get("CAMPCODE", WF_CAMPCODE.Text, WW_TEXT, WW_RTN_SW)
            If WW_RTN_SW <> C_MESSAGE_NO.NORMAL Then
                'エラーレポート編集
                Dim WW_ERR_MES As String = ""
                WW_ERR_MES = "・更新できないレコード(会社コードエラー)です。"
                WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> マスタに存在しません。(" & WF_CAMPCODE.Text & ") ,"
                ERRMSG_write(WW_ERR_MES, WW_LINEerr, "10023")
            End If
        Else
            'エラーレポート編集
            Dim WW_ERR_MES As String = ""
            WW_ERR_MES = "・更新できないレコード(会社コードエラー)です。"
            WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> " & CS0036FCHECK.CHECKREPORT & " , "
            ERRMSG_write(WW_ERR_MES, WW_LINEerr, "10023")
        End If

        '・キー項目(従業員：STAFFCODE)
        '①必須・項目属性チェック
        CS0036FCHECK.FIELD = "STAFFCODE"
        CS0036FCHECK.VALUE = WF_STAFFCODE.Text
        CS0036FCHECK.CS0036FCHECK()
        If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then
            WF_STAFFCODE.Text = CS0036FCHECK.VALUEOUT
            CODENAME_get("STAFFCODE", WF_STAFFCODE.Text, WW_TEXT, WW_RTN_SW)
            If WW_RTN_SW <> C_MESSAGE_NO.NORMAL Then
                'エラーレポート編集
                Dim WW_ERR_MES As String = ""
                WW_ERR_MES = "・更新できないレコード(従業員エラー)です。"
                WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> マスタに存在しません。(" & WF_STAFFCODE.Text & ") ,"
                ERRMSG_write(WW_ERR_MES, WW_LINEerr, "10023")
            End If
        Else
            'エラーレポート編集
            Dim WW_ERR_MES As String = ""
            WW_ERR_MES = "・更新できないレコード(従業員エラー)です。"
            WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> " & CS0036FCHECK.CHECKREPORT & " , "
            ERRMSG_write(WW_ERR_MES, WW_LINEerr, "10023")
        End If

        If work.WF_T7KIN_RECODEKBN.Text = "0" Then
            '・キー項目(勤務年月日：WORKDATE)
            '①必須・項目属性チェック
            CS0036FCHECK.FIELD = "WORKDATE"
            CS0036FCHECK.VALUE = WF_WORKDATE.Text
            CS0036FCHECK.CS0036FCHECK()
            If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then
                WF_WORKDATE.Text = CS0036FCHECK.VALUEOUT
            Else
                'エラーレポート編集
                Dim WW_ERR_MES As String = ""
                WW_ERR_MES = "・更新できないレコード(勤務年月日エラー)です。"
                WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> " & CS0036FCHECK.CHECKREPORT & " , "
                ERRMSG_write(WW_ERR_MES, WW_LINEerr, "10023")
            End If

            '・キー項目(休日区分：HOLIDAYKBN)
            '①必須・項目属性チェック
            CS0036FCHECK.FIELD = "HOLIDAYKBN"
            CS0036FCHECK.VALUE = WF_HOLIDAYKBN.Text
            CS0036FCHECK.CS0036FCHECK()
            If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then
                If WF_WORKINGWEEK_TEXT.Text = "日" And WF_HOLIDAYKBN.Text <> "1" Then
                    'エラーレポート編集
                    Dim WW_ERR_MES As String = ""
                    WW_ERR_MES = "・更新できないレコード(休日区分エラー)です。"
                    WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 日曜日は法定休日です。 , "
                    ERRMSG_write(WW_ERR_MES, WW_LINEerr, "10023")
                ElseIf WF_WORKINGWEEK_TEXT.Text <> "日" And WF_HOLIDAYKBN.Text = "1" Then
                    'エラーレポート編集
                    Dim WW_ERR_MES As String = ""
                    WW_ERR_MES = "・更新できないレコード(休日区分エラー)です。"
                    WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 法定休日は日曜日だけです。 , "
                    ERRMSG_write(WW_ERR_MES, WW_LINEerr, "10023")
                Else
                    WF_HOLIDAYKBN.Text = CS0036FCHECK.VALUEOUT
                End If
            Else
                'エラーレポート編集
                Dim WW_ERR_MES As String = ""
                WW_ERR_MES = "・更新できないレコード(休日区分エラー)です。"
                WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> " & CS0036FCHECK.CHECKREPORT & " , "
                ERRMSG_write(WW_ERR_MES, WW_LINEerr, "10023")
            End If

            '・キー項目(勤怠区分：PAYKBN)
            '①必須・項目属性チェック
            WF_PAYKBN_TEXT.Text = ""
            CS0036FCHECK.FIELD = "PAYKBN"
            CS0036FCHECK.VALUE = WF_PAYKBN.Text
            CS0036FCHECK.CS0036FCHECK()
            If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then
                WF_PAYKBN.Text = CS0036FCHECK.VALUEOUT
                CODENAME_get("PAYKBN", WF_PAYKBN.Text, WF_PAYKBN_TEXT.Text, WW_DUMMY)
            Else
                'エラーレポート編集
                Dim WW_ERR_MES As String = ""
                WW_ERR_MES = "・更新できないレコード(勤怠区分エラー)です。"
                WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> " & CS0036FCHECK.CHECKREPORT & " , "
                ERRMSG_write(WW_ERR_MES, WW_LINEerr, "10023")
            End If

            '・キー項目(宿日直区分：SHUKCHOKKBN)
            '①必須・項目属性チェック
            WF_SHUKCHOKKBN_TEXT.Text = ""
            CS0036FCHECK.FIELD = "SHUKCHOKKBN"
            CS0036FCHECK.VALUE = WF_SHUKCHOKKBN.Text
            CS0036FCHECK.CS0036FCHECK()
            If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then
                WF_SHUKCHOKKBN.Text = CS0036FCHECK.VALUEOUT
                CODENAME_get("SHUKCHOKKBN", WF_SHUKCHOKKBN.Text, WF_SHUKCHOKKBN_TEXT.Text, WW_DUMMY)
            Else
                'エラーレポート編集
                Dim WW_ERR_MES As String = ""
                WW_ERR_MES = "・更新できないレコード(宿日直区分エラー)です。"
                WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> " & CS0036FCHECK.CHECKREPORT & " , "
                ERRMSG_write(WW_ERR_MES, WW_LINEerr, "10023")
            End If

            '・キー項目(開始日：STDATE)
            '①必須・項目属性チェック
            If WF_STDATE.Text <> "" Then
                CS0036FCHECK.FIELD = "STDATE"
                CS0036FCHECK.VALUE = WF_STDATE.Text
                CS0036FCHECK.CS0036FCHECK()
                If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then
                    WF_STDATE.Text = CS0036FCHECK.VALUEOUT
                Else
                    'エラーレポート編集
                    Dim WW_ERR_MES As String = ""
                    WW_ERR_MES = "・更新できないレコード(出社日エラー)です。"
                    WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> " & CS0036FCHECK.CHECKREPORT & " , "
                    ERRMSG_write(WW_ERR_MES, WW_LINEerr, "10023")
                End If
            Else
                WF_STDATE.Text = WF_WORKDATE.Text
            End If

            '・キー項目(開始時刻：STTIME)
            '①必須・項目属性チェック
            If WF_STTIME.Text <> "" Then
                CS0036FCHECK.FIELD = "STTIME"
                CS0036FCHECK.VALUE = WF_STTIME.Text
                CS0036FCHECK.CS0036FCHECK()
                If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then
                    WF_STTIME.Text = CDate(CS0036FCHECK.VALUEOUT).ToString("HH:mm")
                    WF_BINDSTDATE.Text = WF_STTIME.Text
                Else
                    'エラーレポート編集
                    Dim WW_ERR_MES As String = ""
                    WW_ERR_MES = "・更新できないレコード(出社時刻エラー)です。"
                    WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> " & CS0036FCHECK.CHECKREPORT & " , "
                    ERRMSG_write(WW_ERR_MES, WW_LINEerr, "10023")
                End If
            Else
                WF_STTIME.Text = "00:00"
            End If

            '・キー項目(終了日：ENDDATE)
            '①必須・項目属性チェック
            If WF_ENDDATE.Text <> "" Then
                CS0036FCHECK.FIELD = "ENDDATE"
                CS0036FCHECK.VALUE = WF_ENDDATE.Text
                CS0036FCHECK.CS0036FCHECK()
                If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then
                    WF_ENDDATE.Text = CS0036FCHECK.VALUEOUT
                Else
                    'エラーレポート編集
                    Dim WW_ERR_MES As String = ""
                    WW_ERR_MES = "・更新できないレコード(退社日エラー)です。"
                    WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> " & CS0036FCHECK.CHECKREPORT & " , "
                    ERRMSG_write(WW_ERR_MES, WW_LINEerr, "10023")
                End If
            Else
                WF_ENDDATE.Text = WF_WORKDATE.Text
            End If

            '・キー項目(終了時刻：ENDTIME)
            '①必須・項目属性チェック
            If WF_ENDTIME.Text <> "" Then
                CS0036FCHECK.FIELD = "ENDTIME"
                CS0036FCHECK.VALUE = WF_ENDTIME.Text
                CS0036FCHECK.CS0036FCHECK()
                If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then
                    WF_ENDTIME.Text = CDate(CS0036FCHECK.VALUEOUT).ToString("HH:mm")
                Else
                    'エラーレポート編集
                    Dim WW_ERR_MES As String = ""
                    WW_ERR_MES = "・更新できないレコード(退社時刻エラー)です。"
                    WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> " & CS0036FCHECK.CHECKREPORT & " , "
                    ERRMSG_write(WW_ERR_MES, WW_LINEerr, "10023")
                End If
            Else
                WF_ENDTIME.Text = "00:00"
            End If

            '・キー項目(拘束開始時刻：BINDSTDATE)
            '①必須・項目属性チェック
            If WF_BINDSTDATE.Text <> "" Then
                CS0036FCHECK.FIELD = "BINDSTDATE"
                CS0036FCHECK.VALUE = WF_BINDSTDATE.Text
                CS0036FCHECK.CS0036FCHECK()
                If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then
                    WF_BINDSTDATE.Text = CDate(CS0036FCHECK.VALUEOUT).ToString("HH:mm")
                Else
                    'エラーレポート編集
                    Dim WW_ERR_MES As String = ""
                    WW_ERR_MES = "・更新できないレコード(拘束開始時刻エラー)です。"
                    WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> " & CS0036FCHECK.CHECKREPORT & " , "
                    ERRMSG_write(WW_ERR_MES, WW_LINEerr, "10023")
                End If
            Else
                WF_BINDSTDATE.Text = "00:00"
            End If

            '・キー項目(拘束時間：BINDTIME)
            '①必須・項目属性チェック
            If WF_BINDTIME.Text <> "" Then
                CS0036FCHECK.FIELD = "BINDTIME"
                CS0036FCHECK.VALUE = WF_BINDTIME.Text
                CS0036FCHECK.CS0036FCHECK()
                If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then
                    WF_BINDTIME.Text = CDate(CS0036FCHECK.VALUEOUT).ToString("HH:mm")
                Else
                    'エラーレポート編集
                    Dim WW_ERR_MES As String = ""
                    WW_ERR_MES = "・更新できないレコード(拘束時間エラー)です。"
                    WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> " & CS0036FCHECK.CHECKREPORT & " , "
                    ERRMSG_write(WW_ERR_MES, WW_LINEerr, "10023")
                End If
            Else
                WF_BINDTIME.Text = "00:00"
            End If

            '・キー項目(休憩時間：BREAKTIME)
            '①必須・項目属性チェック
            If WF_BREAKTIME.Text <> "" Then
                CS0036FCHECK.FIELD = "BREAKTIME"
                CS0036FCHECK.VALUE = WF_BREAKTIME.Text
                CS0036FCHECK.CS0036FCHECK()
                If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then
                    WF_BREAKTIME.Text = CDate(CS0036FCHECK.VALUEOUT).ToString("HH:mm")
                Else
                    'エラーレポート編集
                    Dim WW_ERR_MES As String = ""
                    WW_ERR_MES = "・更新できないレコード(休憩時間エラー)です。"
                    WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> " & CS0036FCHECK.CHECKREPORT & " , "
                    ERRMSG_write(WW_ERR_MES, WW_LINEerr, "10023")
                End If
            Else
                WF_BREAKTIME.Text = "00:00"
            End If

            '・キー項目(特作Ⅰ：TOKUSA1TIME)
            '①必須・項目属性チェック
            If WF_TOKUSA1TIME.Text <> "" Then
                CS0036FCHECK.FIELD = "TOKUSA1TIME"
                CS0036FCHECK.VALUE = WF_TOKUSA1TIME.Text
                CS0036FCHECK.CS0036FCHECK()
                If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then
                    WF_TOKUSA1TIME.Text = CDate(CS0036FCHECK.VALUEOUT).ToString("HH:mm")
                Else
                    'エラーレポート編集
                    Dim WW_ERR_MES As String = ""
                    WW_ERR_MES = "・更新できないレコード(特作Ⅰエラー)です。"
                    WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> " & CS0036FCHECK.CHECKREPORT & " , "
                    ERRMSG_write(WW_ERR_MES, WW_LINEerr, "10023")
                End If
            Else
                WF_TOKUSA1TIME.Text = "00:00"
            End If

            Dim WW_BBTTLTIME As Integer = 0
            Dim WW_G1TTLTIME As Integer = 0
            For i As Integer = 1 To 10
                Dim WW_BBSTTIME As String = "WF_BBSTTIME" & i.ToString("00")
                Dim WW_BBENDTIME As String = "WF_BBENDTIME" & i.ToString("00")
                Dim WW_G1STTIME As String = "WF_G1STTIME" & i.ToString("00")
                Dim WW_G1ENDTIME As String = "WF_G1ENDTIME" & i.ToString("00")
                Dim WW_STRTN As String = C_MESSAGE_NO.NORMAL
                Dim WW_ENDRTN As String = C_MESSAGE_NO.NORMAL

                '開始時刻（休憩）
                If CType(WF_DView1.FindControl(WW_BBSTTIME), System.Web.UI.WebControls.TextBox).Text <> "" Then
                    CS0036FCHECK.FIELD = "BBSTTIME"
                    CS0036FCHECK.VALUE = CType(WF_DView1.FindControl(WW_BBSTTIME), System.Web.UI.WebControls.TextBox).Text
                    CS0036FCHECK.CS0036FCHECK()
                    If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then
                        CType(WF_DView1.FindControl(WW_BBSTTIME), System.Web.UI.WebControls.TextBox).Text = CDate(CS0036FCHECK.VALUEOUT).ToString("HH:mm")
                    Else
                        'エラーレポート編集
                        Dim WW_ERR_MES As String = ""
                        WW_ERR_MES = "・更新できないレコード(開始時刻" & i.ToString("00") & "エラー)です。"
                        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> " & CS0036FCHECK.CHECKREPORT & " , "
                        ERRMSG_write(WW_ERR_MES, WW_LINEerr, "10023")
                        WW_STRTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If

                Else
                    CType(WF_DView1.FindControl(WW_BBSTTIME), System.Web.UI.WebControls.TextBox).Text = ""
                End If

                '終了時刻（休憩）
                If CType(WF_DView1.FindControl(WW_BBENDTIME), System.Web.UI.WebControls.TextBox).Text <> "" Then
                    CS0036FCHECK.FIELD = "BBENDTIME"
                    CS0036FCHECK.VALUE = CType(WF_DView1.FindControl(WW_BBENDTIME), System.Web.UI.WebControls.TextBox).Text
                    CS0036FCHECK.CS0036FCHECK()
                    If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then
                        CType(WF_DView1.FindControl(WW_BBENDTIME), System.Web.UI.WebControls.TextBox).Text = CDate(CS0036FCHECK.VALUEOUT).ToString("HH:mm")
                    Else
                        'エラーレポート編集
                        Dim WW_ERR_MES As String = ""
                        WW_ERR_MES = "・更新できないレコード(終了時刻" & i.ToString("00") & "エラー)です。"
                        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> " & CS0036FCHECK.CHECKREPORT & " , "
                        ERRMSG_write(WW_ERR_MES, WW_LINEerr, "10023")
                        WW_ENDRTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If

                Else
                    CType(WF_DView1.FindControl(WW_BBENDTIME), System.Web.UI.WebControls.TextBox).Text = ""
                End If
                If isNormal(WW_STRTN) AndAlso isNormal(WW_ENDRTN) Then
                    Dim WW_DATE_ST As String = WF_WORKDATE.Text & " " & CType(WF_DView1.FindControl(WW_BBSTTIME), System.Web.UI.WebControls.TextBox).Text
                    Dim WW_DATE_END As String = WF_WORKDATE.Text & " " & CType(WF_DView1.FindControl(WW_BBENDTIME), System.Web.UI.WebControls.TextBox).Text

                    If IsDate(WW_DATE_ST) AndAlso IsDate(WW_DATE_END) Then
                        If DateDiff("n", WW_DATE_ST, WW_DATE_END) < 0 Then
                            WW_DATE_END = CDate(WF_WORKDATE.Text).AddDays(1) & " " & CType(WF_DView1.FindControl(WW_BBENDTIME), System.Web.UI.WebControls.TextBox).Text
                            If DateDiff("n", WW_DATE_ST, WW_DATE_END) >= 0 Then
                                WW_BBTTLTIME += DateDiff("n", WW_DATE_ST, WW_DATE_END)
                            End If
                        Else
                            WW_BBTTLTIME += DateDiff("n", WW_DATE_ST, WW_DATE_END)
                        End If
                    End If
                End If

                '開始時刻（配送）
                If CType(WF_DView1.FindControl(WW_G1STTIME), System.Web.UI.WebControls.TextBox).Text <> "" Then
                    CS0036FCHECK.FIELD = "G1STTIME"
                    CS0036FCHECK.VALUE = CType(WF_DView1.FindControl(WW_G1STTIME), System.Web.UI.WebControls.TextBox).Text
                    CS0036FCHECK.CS0036FCHECK()
                    If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then
                        CType(WF_DView1.FindControl(WW_G1STTIME), System.Web.UI.WebControls.TextBox).Text = CDate(CS0036FCHECK.VALUEOUT).ToString("HH:mm")
                    Else
                        'エラーレポート編集
                        Dim WW_ERR_MES As String = ""
                        WW_ERR_MES = "・更新できないレコード(開始時刻" & i.ToString("00") & "エラー)です。"
                        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> " & CS0036FCHECK.CHECKREPORT & " , "
                        ERRMSG_write(WW_ERR_MES, WW_LINEerr, "10023")
                    End If

                Else
                    CType(WF_DView1.FindControl(WW_G1STTIME), System.Web.UI.WebControls.TextBox).Text = ""
                End If

                '終了時刻（配送）
                If CType(WF_DView1.FindControl(WW_G1ENDTIME), System.Web.UI.WebControls.TextBox).Text <> "" Then
                    CS0036FCHECK.FIELD = "G1ENDTIME"
                    CS0036FCHECK.VALUE = CType(WF_DView1.FindControl(WW_G1ENDTIME), System.Web.UI.WebControls.TextBox).Text
                    CS0036FCHECK.CS0036FCHECK()
                    If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then
                        CType(WF_DView1.FindControl(WW_G1ENDTIME), System.Web.UI.WebControls.TextBox).Text = CDate(CS0036FCHECK.VALUEOUT).ToString("HH:mm")
                    Else
                        'エラーレポート編集
                        Dim WW_ERR_MES As String = ""
                        WW_ERR_MES = "・更新できないレコード(終了時刻" & i.ToString("00") & "エラー)です。"
                        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> " & CS0036FCHECK.CHECKREPORT & " , "
                        ERRMSG_write(WW_ERR_MES, WW_LINEerr, "10023")
                    End If

                Else
                    CType(WF_DView1.FindControl(WW_G1ENDTIME), System.Web.UI.WebControls.TextBox).Text = ""
                End If
                If isNormal(WW_STRTN) AndAlso isNormal(WW_ENDRTN) Then
                    Dim WW_DATE_ST As String = WF_WORKDATE.Text & " " & CType(WF_DView1.FindControl(WW_G1STTIME), System.Web.UI.WebControls.TextBox).Text
                    Dim WW_DATE_END As String = WF_WORKDATE.Text & " " & CType(WF_DView1.FindControl(WW_G1ENDTIME), System.Web.UI.WebControls.TextBox).Text

                    If IsDate(WW_DATE_ST) AndAlso IsDate(WW_DATE_END) Then
                        If DateDiff("n", WW_DATE_ST, WW_DATE_END) < 0 Then
                            WW_DATE_END = CDate(WF_WORKDATE.Text).AddDays(1) & " " & CType(WF_DView1.FindControl(WW_G1ENDTIME), System.Web.UI.WebControls.TextBox).Text
                            If DateDiff("n", WW_DATE_ST, WW_DATE_END) >= 0 Then
                                WW_G1TTLTIME += DateDiff("n", WW_DATE_ST, WW_DATE_END)
                            End If
                        Else
                            WW_G1TTLTIME += DateDiff("n", WW_DATE_ST, WW_DATE_END)
                        End If
                    End If
                End If

            Next
            CType(WF_DView1.FindControl("WF_BBTTLTIME"), System.Web.UI.WebControls.TextBox).Text = T0007COM.formatHHMM(WW_BBTTLTIME)
            CType(WF_DView1.FindControl("WF_G1TTLTIME"), System.Web.UI.WebControls.TextBox).Text = T0007COM.formatHHMM(WW_G1TTLTIME)
            CType(WF_DView1.FindControl("WF_NIPPOBREAKTIME"), System.Web.UI.WebControls.TextBox).Text = T0007COM.formatHHMM(WW_BBTTLTIME)
            CType(WF_DView1.FindControl("WF_HAISOTIME"), System.Web.UI.WebControls.TextBox).Text = T0007COM.formatHHMM(WW_G1TTLTIME)
            '2020/11/17 ADD END

            '・キー項目(トレーラＬＮＧ配送距離：WF_MODELDISTANCE0204)
            '①必須・項目属性チェック
            CS0036FCHECK.FIELD = "MODELDISTANCETTL"
            CS0036FCHECK.VALUE = WF_MODELDISTANCE0204.Text
            CS0036FCHECK.CS0036FCHECK()
            If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then
                WF_MODELDISTANCE0204.Text = Val(CS0036FCHECK.VALUEOUT)
            Else
                'エラーレポート編集
                Dim WW_ERR_MES As String = ""
                WW_ERR_MES = "・更新できないレコード(トレーラＬＮＧモデル距離エラー)です。"
                WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> " & CS0036FCHECK.CHECKREPORT & " , "
                ERRMSG_write(WW_ERR_MES, WW_LINEerr, "10023")
            End If

            '・キー項目(単車ＬＮＧ配送距離：MODELDISTANCE0109)
            '①必須・項目属性チェック
            CS0036FCHECK.FIELD = "MODELDISTANCETTL"
            CS0036FCHECK.VALUE = WF_MODELDISTANCE0109.Text
            CS0036FCHECK.CS0036FCHECK()
            If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then
                WF_MODELDISTANCE0109.Text = Val(CS0036FCHECK.VALUEOUT)
            Else
                'エラーレポート編集
                Dim WW_ERR_MES As String = ""
                WW_ERR_MES = "・更新できないレコード(単車ラテックスモデル距離エラー)です。"
                WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> " & CS0036FCHECK.CHECKREPORT & " , "
                ERRMSG_write(WW_ERR_MES, WW_LINEerr, "10023")
            End If

            '・キー項目(トレーラＬＮＧ配送距離：MODELDISTANCE0209)
            '①必須・項目属性チェック
            CS0036FCHECK.FIELD = "MODELDISTANCETTL"
            CS0036FCHECK.VALUE = WF_MODELDISTANCE0209.Text
            CS0036FCHECK.CS0036FCHECK()
            If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then
                WF_MODELDISTANCE0209.Text = Val(CS0036FCHECK.VALUEOUT)
            Else
                'エラーレポート編集
                Dim WW_ERR_MES As String = ""
                WW_ERR_MES = "・更新できないレコード(トレーララテックスモデル距離エラー)です。"
                WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> " & CS0036FCHECK.CHECKREPORT & " , "
                ERRMSG_write(WW_ERR_MES, WW_LINEerr, "10023")
            End If

            For i As Integer = 1 To 6
                Dim WW_SHARYOKBN As String = "WF_SHARYOKBN" & i.ToString
                Dim WW_OILPAYKBN As String = "WF_OILPAYKBN" & i.ToString
                Dim WW_SHUKABASHO As String = "WF_SHUKABASHO" & i.ToString
                Dim WW_TODOKECODE As String = "WF_TODOKECODE" & i.ToString
                Dim WW_MODELDISTANCE As String = "WF_MODELDISTANCE" & i.ToString

                Dim WF_SHARYOKBN_TEXT As String = "WF_SHARYOKBN" & i.ToString & "_TEXT"
                Dim WF_OILPAYKBN_TEXT As String = "WF_OILPAYKBN" & i.ToString & "_TEXT"
                Dim WF_SHUKABASHO_TEXT As String = "WF_SHUKABASHO" & i.ToString & "_TEXT"
                Dim WF_TODOKECODE_TEXT As String = "WF_TODOKECODE" & i.ToString & "_TEXT"

                '・キー項目(単車・トレーラ区分１～６：SHARYOKBN)
                '①必須・項目属性チェック
                CS0036FCHECK.FIELD = "SHARYOKBN"
                CS0036FCHECK.VALUE = CType(WF_DView3.FindControl(WW_SHARYOKBN), System.Web.UI.WebControls.TextBox).Text
                CS0036FCHECK.CS0036FCHECK()
                If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then
                    CType(WF_DView3.FindControl(WW_SHARYOKBN), System.Web.UI.WebControls.TextBox).Text = CS0036FCHECK.VALUEOUT
                    If CS0036FCHECK.VALUEOUT <> "" Then
                        CODENAME_get("SHARYOKBN", CS0036FCHECK.VALUEOUT, WW_TEXT, WW_RTN_SW)
                        CType(WF_DView3.FindControl(WF_SHARYOKBN_TEXT), System.Web.UI.WebControls.Label).Text = WW_TEXT
                        If WW_RTN_SW <> C_MESSAGE_NO.NORMAL Then
                            'エラーレポート編集
                            Dim WW_ERR_MES As String = ""
                            WW_ERR_MES = "・更新できないレコード(No." & i & " 単車・トレーラ区分エラー)です。"
                            WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> マスタに存在しません。(" & CS0036FCHECK.VALUEOUT & ") ,"
                            ERRMSG_write(WW_ERR_MES, WW_LINEerr, "10023")
                        End If
                    End If
                Else
                    'エラーレポート編集
                    Dim WW_ERR_MES As String = ""
                    WW_ERR_MES = "・更新できないレコード(No." & i & " 単車・トレーラ区分エラー)です。"
                    WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> " & CS0036FCHECK.CHECKREPORT & " , "
                    ERRMSG_write(WW_ERR_MES, WW_LINEerr, "10023")
                End If

                '・キー項目(油種区分１～６：OILPAYKBN)
                '①必須・項目属性チェック
                CS0036FCHECK.FIELD = "OILPAYKBN"
                CS0036FCHECK.VALUE = CType(WF_DView3.FindControl(WW_OILPAYKBN), System.Web.UI.WebControls.TextBox).Text
                CS0036FCHECK.CS0036FCHECK()
                If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then
                    CType(WF_DView3.FindControl(WW_OILPAYKBN), System.Web.UI.WebControls.TextBox).Text = CS0036FCHECK.VALUEOUT
                    If CS0036FCHECK.VALUEOUT <> "" Then
                        CODENAME_get("OILPAYKBN", CS0036FCHECK.VALUEOUT, WW_TEXT, WW_RTN_SW)
                        CType(WF_DView3.FindControl(WF_OILPAYKBN_TEXT), System.Web.UI.WebControls.Label).Text = WW_TEXT
                        If WW_RTN_SW <> C_MESSAGE_NO.NORMAL Then
                            'エラーレポート編集
                            Dim WW_ERR_MES As String = ""
                            WW_ERR_MES = "・更新できないレコード(No." & i & " 油種区分エラー)です。"
                            WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> マスタに存在しません。(" & CS0036FCHECK.VALUEOUT & ") ,"
                            ERRMSG_write(WW_ERR_MES, WW_LINEerr, "10023")
                        End If
                    End If
                Else
                    'エラーレポート編集
                    Dim WW_ERR_MES As String = ""
                    WW_ERR_MES = "・更新できないレコード(No." & i & " 油種区分エラー)です。"
                    WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> " & CS0036FCHECK.CHECKREPORT & " , "
                    ERRMSG_write(WW_ERR_MES, WW_LINEerr, "10023")
                End If

                '・キー項目(出荷場所１～６：SHUKABASHO)
                '①必須・項目属性チェック
                CS0036FCHECK.FIELD = "SHUKABASHO"
                CS0036FCHECK.VALUE = CType(WF_DView3.FindControl(WW_SHUKABASHO), System.Web.UI.WebControls.TextBox).Text
                CS0036FCHECK.CS0036FCHECK()
                If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then
                    CType(WF_DView3.FindControl(WW_SHUKABASHO), System.Web.UI.WebControls.TextBox).Text = CS0036FCHECK.VALUEOUT
                    If CS0036FCHECK.VALUEOUT <> "" Then
                        CODENAME_get("SHUKABASHO", CS0036FCHECK.VALUEOUT, WW_TEXT, WW_RTN_SW)
                        CType(WF_DView3.FindControl(WF_SHUKABASHO_TEXT), System.Web.UI.WebControls.Label).Text = WW_TEXT
                        If WW_RTN_SW <> C_MESSAGE_NO.NORMAL Then
                            'エラーレポート編集
                            Dim WW_ERR_MES As String = ""
                            WW_ERR_MES = "・更新できないレコード(No." & i & " 出荷場所エラー)です。"
                            WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> マスタに存在しません。(" & CS0036FCHECK.VALUEOUT & ") ,"
                            ERRMSG_write(WW_ERR_MES, WW_LINEerr, "10023")
                        End If
                    End If
                Else
                    'エラーレポート編集
                    Dim WW_ERR_MES As String = ""
                    WW_ERR_MES = "・更新できないレコード(No." & i & " 出荷場所エラー)です。"
                    WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> " & CS0036FCHECK.CHECKREPORT & " , "
                    ERRMSG_write(WW_ERR_MES, WW_LINEerr, "10023")
                End If

                '・キー項目(届先１～６：TODOKECODE)
                '①必須・項目属性チェック
                CS0036FCHECK.FIELD = "TODOKECODE"
                CS0036FCHECK.VALUE = CType(WF_DView3.FindControl(WW_TODOKECODE), System.Web.UI.WebControls.TextBox).Text
                CS0036FCHECK.CS0036FCHECK()
                If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then
                    CType(WF_DView3.FindControl(WW_TODOKECODE), System.Web.UI.WebControls.TextBox).Text = CS0036FCHECK.VALUEOUT
                    If CS0036FCHECK.VALUEOUT <> "" Then
                        CODENAME_get("TODOKECODE", CS0036FCHECK.VALUEOUT, WW_TEXT, WW_RTN_SW)
                        CType(WF_DView3.FindControl(WF_TODOKECODE_TEXT), System.Web.UI.WebControls.Label).Text = WW_TEXT
                        If WW_RTN_SW <> C_MESSAGE_NO.NORMAL Then
                            'エラーレポート編集
                            Dim WW_ERR_MES As String = ""
                            WW_ERR_MES = "・更新できないレコード(No." & i & " 届先エラー)です。"
                            WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> マスタに存在しません。(" & CS0036FCHECK.VALUEOUT & ") ,"
                            ERRMSG_write(WW_ERR_MES, WW_LINEerr, "10023")
                        End If
                    End If
                Else
                    'エラーレポート編集
                    Dim WW_ERR_MES As String = ""
                    WW_ERR_MES = "・更新できないレコード(No." & i & " 届先エラー)です。"
                    WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> " & CS0036FCHECK.CHECKREPORT & " , "
                    ERRMSG_write(WW_ERR_MES, WW_LINEerr, "10023")
                End If

                '・キー項目(モデル距離１～６：MODELDISTANCE)
                '①必須・項目属性チェック
                CS0036FCHECK.FIELD = "MODELDISTANCE"
                CS0036FCHECK.VALUE = CType(WF_DView3.FindControl(WW_MODELDISTANCE), System.Web.UI.WebControls.TextBox).Text
                CS0036FCHECK.CS0036FCHECK()
                If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then
                    CType(WF_DView3.FindControl(WW_MODELDISTANCE), System.Web.UI.WebControls.TextBox).Text = Val(CS0036FCHECK.VALUEOUT)
                Else
                    'エラーレポート編集
                    Dim WW_ERR_MES As String = ""
                    WW_ERR_MES = "・更新できないレコード(モデル距離" & i & "エラー)です。"
                    WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> " & CS0036FCHECK.CHECKREPORT & " , "
                    ERRMSG_write(WW_ERR_MES, WW_LINEerr, "10023")
                End If
            Next
        End If


        '■月調整項目

        If work.WF_T7KIN_RECODEKBN.Text = "2" Then
            '・キー項目(所労：WORKNISSUTTL)
            '①必須・項目属性チェック
            CS0036FCHECK.FIELD = "WORKNISSUTTL"
            CS0036FCHECK.VALUE = WF_WORKNISSUTTL.Text
            CS0036FCHECK.CS0036FCHECK()
            If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then
                WF_WORKNISSUTTL.Text = Val(CS0036FCHECK.VALUEOUT)
            Else
                'エラーレポート編集
                Dim WW_ERR_MES As String = ""
                WW_ERR_MES = "・更新できないレコード(所労日数エラー)です。"
                WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> " & CS0036FCHECK.CHECKREPORT & " , "
                ERRMSG_write(WW_ERR_MES, WW_LINEerr, "10023")
            End If

            '・キー項目(所労：WORKNISSUTTL)
            '①必須・項目属性チェック
            CS0036FCHECK.FIELD = "WORKNISSUTTL"
            CS0036FCHECK.VALUE = WF_WORKNISSUTTL.Text
            CS0036FCHECK.CS0036FCHECK()
            If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then
                WF_WORKNISSUTTL.Text = Val(CS0036FCHECK.VALUEOUT)
            Else
                'エラーレポート編集
                Dim WW_ERR_MES As String = ""
                WW_ERR_MES = "・更新できないレコード(所労日数エラー)です。"
                WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> " & CS0036FCHECK.CHECKREPORT & " , "
                ERRMSG_write(WW_ERR_MES, WW_LINEerr, "10023")
            End If

            '・キー項目(傷欠：SHOUKETUNISSUTTL)
            '①必須・項目属性チェック
            CS0036FCHECK.FIELD = "SHOUKETUNISSUTTL"
            CS0036FCHECK.VALUE = WF_SHOUKETUNISSUTTL.Text
            CS0036FCHECK.CS0036FCHECK()
            If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then
                WF_SHOUKETUNISSUTTL.Text = Val(CS0036FCHECK.VALUEOUT)
            Else
                'エラーレポート編集
                Dim WW_ERR_MES As String = ""
                WW_ERR_MES = "・更新できないレコード(傷欠日数エラー)です。"
                WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> " & CS0036FCHECK.CHECKREPORT & " , "
                ERRMSG_write(WW_ERR_MES, WW_LINEerr, "10023")
            End If

            '・キー項目(組欠：KUMIKETUNISSUTTL)
            '①必須・項目属性チェック
            CS0036FCHECK.FIELD = "KUMIKETUNISSUTTL"
            CS0036FCHECK.VALUE = WF_KUMIKETUNISSUTTL.Text
            CS0036FCHECK.CS0036FCHECK()
            If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then
                WF_KUMIKETUNISSUTTL.Text = Val(CS0036FCHECK.VALUEOUT)
            Else
                'エラーレポート編集
                Dim WW_ERR_MES As String = ""
                WW_ERR_MES = "・更新できないレコード(組欠日数エラー)です。"
                WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> " & CS0036FCHECK.CHECKREPORT & " , "
                ERRMSG_write(WW_ERR_MES, WW_LINEerr, "10023")
            End If

            '・キー項目(他欠：ETCKETUNISSUTTL)
            '①必須・項目属性チェック
            CS0036FCHECK.FIELD = "ETCKETUNISSUTTL"
            CS0036FCHECK.VALUE = WF_ETCKETUNISSUTTL.Text
            CS0036FCHECK.CS0036FCHECK()
            If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then
                WF_ETCKETUNISSUTTL.Text = Val(CS0036FCHECK.VALUEOUT)
            Else
                'エラーレポート編集
                Dim WW_ERR_MES As String = ""
                WW_ERR_MES = "・更新できないレコード(他欠日数エラー)です。"
                WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> " & CS0036FCHECK.CHECKREPORT & " , "
                ERRMSG_write(WW_ERR_MES, WW_LINEerr, "10023")
            End If

            '・キー項目(年休：NENKYUNISSUTTL)
            '①必須・項目属性チェック
            CS0036FCHECK.FIELD = "NENKYUNISSUTTL"
            CS0036FCHECK.VALUE = WF_NENKYUNISSUTTL.Text
            CS0036FCHECK.CS0036FCHECK()
            If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then
                WF_NENKYUNISSUTTL.Text = Val(CS0036FCHECK.VALUEOUT)
            Else
                'エラーレポート編集
                Dim WW_ERR_MES As String = ""
                WW_ERR_MES = "・更新できないレコード(年休日数エラー)です。"
                WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> " & CS0036FCHECK.CHECKREPORT & " , "
                ERRMSG_write(WW_ERR_MES, WW_LINEerr, "10023")
            End If

            '・キー項目(特休：TOKUKYUNISSUTTL)
            '①必須・項目属性チェック
            CS0036FCHECK.FIELD = "TOKUKYUNISSUTTL"
            CS0036FCHECK.VALUE = WF_TOKUKYUNISSUTTL.Text
            CS0036FCHECK.CS0036FCHECK()
            If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then
                WF_TOKUKYUNISSUTTL.Text = Val(CS0036FCHECK.VALUEOUT)
            Else
                'エラーレポート編集
                Dim WW_ERR_MES As String = ""
                WW_ERR_MES = "・更新できないレコード(特休日数エラー)です。"
                WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> " & CS0036FCHECK.CHECKREPORT & " , "
                ERRMSG_write(WW_ERR_MES, WW_LINEerr, "10023")
            End If

            '・キー項目(遅早：CHIKOKSOTAINISSUTTL)
            '①必須・項目属性チェック
            CS0036FCHECK.FIELD = "CHIKOKSOTAINISSUTTL"
            CS0036FCHECK.VALUE = WF_CHIKOKSOTAINISSUTTL.Text
            CS0036FCHECK.CS0036FCHECK()
            If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then
                WF_CHIKOKSOTAINISSUTTL.Text = Val(CS0036FCHECK.VALUEOUT)
            Else
                'エラーレポート編集
                Dim WW_ERR_MES As String = ""
                WW_ERR_MES = "・更新できないレコード(遅早日数エラー)です。"
                WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> " & CS0036FCHECK.CHECKREPORT & " , "
                ERRMSG_write(WW_ERR_MES, WW_LINEerr, "10023")
            End If

            '・キー項目(ストック休暇：STOCKNISSUTTL)
            '①必須・項目属性チェック
            CS0036FCHECK.FIELD = "STOCKNISSUTTL"
            CS0036FCHECK.VALUE = WF_STOCKNISSUTTL.Text
            CS0036FCHECK.CS0036FCHECK()
            If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then
                WF_STOCKNISSUTTL.Text = Val(CS0036FCHECK.VALUEOUT)
            Else
                'エラーレポート編集
                Dim WW_ERR_MES As String = ""
                WW_ERR_MES = "・更新できないレコード(ストック休暇日数エラー)です。"
                WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> " & CS0036FCHECK.CHECKREPORT & " , "
                ERRMSG_write(WW_ERR_MES, WW_LINEerr, "10023")
            End If

            '・キー項目(協定週休：KYOTEIWEEKNISSUTTL)
            '①必須・項目属性チェック
            CS0036FCHECK.FIELD = "KYOTEIWEEKNISSUTTL"
            CS0036FCHECK.VALUE = WF_KYOTEIWEEKNISSUTTL.Text
            CS0036FCHECK.CS0036FCHECK()
            If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then
                WF_KYOTEIWEEKNISSUTTL.Text = Val(CS0036FCHECK.VALUEOUT)
            Else
                'エラーレポート編集
                Dim WW_ERR_MES As String = ""
                WW_ERR_MES = "・更新できないレコード(協定週休日数エラー)です。"
                WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> " & CS0036FCHECK.CHECKREPORT & " , "
                ERRMSG_write(WW_ERR_MES, WW_LINEerr, "10023")
            End If

            '・キー項目(特休無給：TOKUKYUMUKYUNISSUTTL)
            '①必須・項目属性チェック
            CS0036FCHECK.FIELD = "TOKUKYUMUKYUNISSUTTL"
            CS0036FCHECK.VALUE = WF_TOKUKYUMUKYUNISSUTTL.Text
            CS0036FCHECK.CS0036FCHECK()
            If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then
                WF_TOKUKYUMUKYUNISSUTTL.Text = Val(CS0036FCHECK.VALUEOUT)
            Else
                'エラーレポート編集
                Dim WW_ERR_MES As String = ""
                WW_ERR_MES = "・更新できないレコード(特休無給日数エラー)です。"
                WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> " & CS0036FCHECK.CHECKREPORT & " , "
                ERRMSG_write(WW_ERR_MES, WW_LINEerr, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
            End If

            '・キー項目(労災：ROSAIYUKYNIUSSUTTL)
            '①必須・項目属性チェック
            CS0036FCHECK.FIELD = "ROSAIYUKYNIUSSUTTL"
            CS0036FCHECK.VALUE = WF_ROSAIYUKYNIUSSUTTL.Text
            CS0036FCHECK.CS0036FCHECK()
            If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then
                WF_ROSAIYUKYNIUSSUTTL.Text = Val(CS0036FCHECK.VALUEOUT)
            Else
                'エラーレポート編集
                Dim WW_ERR_MES As String = ""
                WW_ERR_MES = "・更新できないレコード(労災日数エラー)です。"
                WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> " & CS0036FCHECK.CHECKREPORT & " , "
                ERRMSG_write(WW_ERR_MES, WW_LINEerr, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
            End If

            '・キー項目(子看有給：KOKANGOYUKYUNISSUTTL)
            '①必須・項目属性チェック
            CS0036FCHECK.FIELD = "KOKANGOYUKYUNISSUTTL"
            CS0036FCHECK.VALUE = WF_KOKANGOYUKYUNISSUTTL.Text
            CS0036FCHECK.CS0036FCHECK()
            If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then
                WF_KOKANGOYUKYUNISSUTTL.Text = Val(CS0036FCHECK.VALUEOUT)
            Else
                'エラーレポート編集
                Dim WW_ERR_MES As String = ""
                WW_ERR_MES = "・更新できないレコード(子看有給日数エラー)です。"
                WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> " & CS0036FCHECK.CHECKREPORT & " , "
                ERRMSG_write(WW_ERR_MES, WW_LINEerr, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
            End If

            '・キー項目(子看無給：KOKANGOMUKYUNISSUTTL)
            '①必須・項目属性チェック
            CS0036FCHECK.FIELD = "KOKANGOMUKYUNISSUTTL"
            CS0036FCHECK.VALUE = WF_KOKANGOMUKYUNISSUTTL.Text
            CS0036FCHECK.CS0036FCHECK()
            If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then
                WF_KOKANGOMUKYUNISSUTTL.Text = Val(CS0036FCHECK.VALUEOUT)
            Else
                'エラーレポート編集
                Dim WW_ERR_MES As String = ""
                WW_ERR_MES = "・更新できないレコード(子看無給日数エラー)です。"
                WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> " & CS0036FCHECK.CHECKREPORT & " , "
                ERRMSG_write(WW_ERR_MES, WW_LINEerr, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
            End If

            '・キー項目(代休：DAIKYUNISSUTTL)
            '①必須・項目属性チェック
            CS0036FCHECK.FIELD = "DAIKYUNISSUTTL"
            CS0036FCHECK.VALUE = WF_DAIKYUNISSUTTL.Text
            CS0036FCHECK.CS0036FCHECK()
            If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then
                WF_DAIKYUNISSUTTL.Text = Val(CS0036FCHECK.VALUEOUT)
            Else
                'エラーレポート編集
                Dim WW_ERR_MES As String = ""
                WW_ERR_MES = "・更新できないレコード(代休日数エラー)です。"
                WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> " & CS0036FCHECK.CHECKREPORT & " , "
                ERRMSG_write(WW_ERR_MES, WW_LINEerr, "10023")
            End If

            '・キー項目(年末出勤：NENMATUNISSUTTL)
            '①必須・項目属性チェック
            CS0036FCHECK.FIELD = "NENMATUNISSUTTL"
            CS0036FCHECK.VALUE = WF_NENMATUNISSUTTL.Text
            CS0036FCHECK.CS0036FCHECK()
            If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then
                WF_NENMATUNISSUTTL.Text = Val(CS0036FCHECK.VALUEOUT)
            Else
                'エラーレポート編集
                Dim WW_ERR_MES As String = ""
                WW_ERR_MES = "・更新できないレコード(年末出勤日数エラー)です。"
                WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> " & CS0036FCHECK.CHECKREPORT & " , "
                ERRMSG_write(WW_ERR_MES, WW_LINEerr, "10023")
            End If

            '・キー項目(年始出勤：NENSHINISSUTTL)
            '①必須・項目属性チェック
            CS0036FCHECK.FIELD = "NENSHINISSUTTL"
            CS0036FCHECK.VALUE = WF_NENSHINISSUTTL.Text
            CS0036FCHECK.CS0036FCHECK()
            If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then
                WF_NENSHINISSUTTL.Text = Val(CS0036FCHECK.VALUEOUT)
            Else
                'エラーレポート編集
                Dim WW_ERR_MES As String = ""
                WW_ERR_MES = "・更新できないレコード(年始出勤日数エラー)です。"
                WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> " & CS0036FCHECK.CHECKREPORT & " , "
                ERRMSG_write(WW_ERR_MES, WW_LINEerr, "10023")
            End If

            Dim WW_TIMEstr() As String = {}
            '・キー項目(特作I：TOKUSA1TIMETTL)
            '①必須・項目属性チェック
            WW_TIMEstr = WF_TOKUSA1TIMETTL.Text.Split(":")
            If WW_TIMEstr.Length = 2 Then
                CS0036FCHECK.FIELD = "TOKUSA1TIMETTL"
                CS0036FCHECK.VALUE = WW_TIMEstr(0)
                CS0036FCHECK.CS0036FCHECK()
                If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then

                    CS0036FCHECK.FIELD = "TOKUSA1TIMETTL"
                    CS0036FCHECK.VALUE = WW_TIMEstr(1)
                    CS0036FCHECK.CS0036FCHECK()
                    If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then
                        If Val(WW_TIMEstr(1)) < 60 Then
                        Else
                            'エラーレポート編集
                            Dim WW_ERR_MES As String = ""
                            WW_ERR_MES = "・更新できないレコード(特作Iエラー)です。"
                            WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> " & WF_TOKUSA1TIMETTL.Text & " , "
                            ERRMSG_write(WW_ERR_MES, WW_LINEerr, "10023")
                        End If
                    Else
                        'エラーレポート編集
                        Dim WW_ERR_MES As String = ""
                        WW_ERR_MES = "・更新できないレコード(特作Iエラー)です。"
                        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> " & CS0036FCHECK.CHECKREPORT & " , "
                        ERRMSG_write(WW_ERR_MES, WW_LINEerr, "10023")
                    End If
                Else
                    'エラーレポート編集
                    Dim WW_ERR_MES As String = ""
                    WW_ERR_MES = "・更新できないレコード(特作Iエラー)です。"
                    WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> " & CS0036FCHECK.CHECKREPORT & " , "
                    ERRMSG_write(WW_ERR_MES, WW_LINEerr, "10023")
                End If
            Else
                'エラーレポート編集
                Dim WW_ERR_MES As String = ""
                WW_ERR_MES = "・更新できないレコード(特作Iエラー)です。"
                WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> " & WF_TOKUSA1TIMETTL.Text & " , "
                ERRMSG_write(WW_ERR_MES, WW_LINEerr, "10023")
            End If

            '・キー項目(時給者作業：JIKYUSHATIMETTL)
            '①必須・項目属性チェック
            WW_TIMEstr = WF_JIKYUSHATIMETTL.Text.Split(":")
            If WW_TIMEstr.Length = 2 Then
                CS0036FCHECK.FIELD = "JIKYUSHATIMETTL"
                CS0036FCHECK.VALUE = WW_TIMEstr(0)
                CS0036FCHECK.CS0036FCHECK()
                If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then

                    CS0036FCHECK.FIELD = "JIKYUSHATIMETTL"
                    CS0036FCHECK.VALUE = WW_TIMEstr(1)
                    CS0036FCHECK.CS0036FCHECK()
                    If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then
                        If Val(WW_TIMEstr(1)) < 60 Then
                        Else
                            'エラーレポート編集
                            Dim WW_ERR_MES As String = ""
                            WW_ERR_MES = "・更新できないレコード(時給者作業エラー)です。"
                            WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> " & WF_SHACHUHAKNISSUTTL.Text & " , "
                            ERRMSG_write(WW_ERR_MES, WW_LINEerr, "10023")
                        End If
                    Else
                        'エラーレポート編集
                        Dim WW_ERR_MES As String = ""
                        WW_ERR_MES = "・更新できないレコード(時給者作業エラー)です。"
                        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> " & CS0036FCHECK.CHECKREPORT & " , "
                        ERRMSG_write(WW_ERR_MES, WW_LINEerr, "10023")
                    End If
                Else
                    'エラーレポート編集
                    Dim WW_ERR_MES As String = ""
                    WW_ERR_MES = "・更新できないレコード(時給者作業エラー)です。"
                    WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> " & CS0036FCHECK.CHECKREPORT & " , "
                    ERRMSG_write(WW_ERR_MES, WW_LINEerr, "10023")
                End If
            Else
                'エラーレポート編集
                Dim WW_ERR_MES As String = ""
                WW_ERR_MES = "・更新できないレコード(時給者作業エラー)です。"
                WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> " & WF_SHACHUHAKNISSUTTL.Text & " , "
                ERRMSG_write(WW_ERR_MES, WW_LINEerr, "10023")
            End If

            '・キー項目(平日残業：ORVERTIMETTL)
            '①必須・項目属性チェック
            WW_TIMEstr = WF_ORVERTIMETTL.Text.Split(":")
            If WW_TIMEstr.Length = 2 Then
                CS0036FCHECK.FIELD = "ORVERTIMETTL"
                CS0036FCHECK.VALUE = WW_TIMEstr(0)
                CS0036FCHECK.CS0036FCHECK()
                If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then
                    CS0036FCHECK.FIELD = "ORVERTIMETTL"
                    CS0036FCHECK.VALUE = WW_TIMEstr(1)
                    CS0036FCHECK.CS0036FCHECK()
                    If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then
                        If Val(WW_TIMEstr(1)) < 60 Then
                        Else
                            'エラーレポート編集
                            Dim WW_ERR_MES As String = ""
                            WW_ERR_MES = "・更新できないレコード(平日残業エラー)です。"
                            WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> " & WF_ORVERTIMETTL.Text & " , "
                            ERRMSG_write(WW_ERR_MES, WW_LINEerr, "10023")
                        End If
                    Else
                        'エラーレポート編集
                        Dim WW_ERR_MES As String = ""
                        WW_ERR_MES = "・更新できないレコード(平日残業エラー)です。"
                        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> " & CS0036FCHECK.CHECKREPORT & " , "
                        ERRMSG_write(WW_ERR_MES, WW_LINEerr, "10023")
                    End If
                Else
                    'エラーレポート編集
                    Dim WW_ERR_MES As String = ""
                    WW_ERR_MES = "・更新できないレコード(平日残業エラー)です。"
                    WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> " & CS0036FCHECK.CHECKREPORT & " , "
                    ERRMSG_write(WW_ERR_MES, WW_LINEerr, "10023")
                End If
            Else
                'エラーレポート編集
                Dim WW_ERR_MES As String = ""
                WW_ERR_MES = "・更新できないレコード(平日残業エラー)です。"
                WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> " & WF_ORVERTIMETTL.Text & " , "
                ERRMSG_write(WW_ERR_MES, WW_LINEerr, "10023")
            End If

            '・キー項目(平日深夜：WNIGHTTIMETTL)
            '①必須・項目属性チェック
            WW_TIMEstr = WF_WNIGHTTIMETTL.Text.Split(":")
            If WW_TIMEstr.Length = 2 Then
                CS0036FCHECK.FIELD = "WNIGHTTIMETTL"
                CS0036FCHECK.VALUE = WW_TIMEstr(0)
                CS0036FCHECK.CS0036FCHECK()
                If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then
                    CS0036FCHECK.FIELD = "WNIGHTTIMETTL"
                    CS0036FCHECK.VALUE = WW_TIMEstr(1)
                    CS0036FCHECK.CS0036FCHECK()
                    If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then
                        If Val(WW_TIMEstr(1)) < 60 Then
                        Else
                            'エラーレポート編集
                            Dim WW_ERR_MES As String = ""
                            WW_ERR_MES = "・更新できないレコード(平日深夜エラー)です。"
                            WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> " & WF_WNIGHTTIMETTL.Text & " , "
                            ERRMSG_write(WW_ERR_MES, WW_LINEerr, "10023")
                        End If
                    Else
                        'エラーレポート編集
                        Dim WW_ERR_MES As String = ""
                        WW_ERR_MES = "・更新できないレコード(平日深夜エラー)です。"
                        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> " & CS0036FCHECK.CHECKREPORT & " , "
                        ERRMSG_write(WW_ERR_MES, WW_LINEerr, "10023")
                    End If
                Else
                    'エラーレポート編集
                    Dim WW_ERR_MES As String = ""
                    WW_ERR_MES = "・更新できないレコード(平日深夜エラー)です。"
                    WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> " & CS0036FCHECK.CHECKREPORT & " , "
                    ERRMSG_write(WW_ERR_MES, WW_LINEerr, "10023")
                End If
            Else
                'エラーレポート編集
                Dim WW_ERR_MES As String = ""
                WW_ERR_MES = "・更新できないレコード(平日深夜エラー)です。"
                WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> " & WF_WNIGHTTIMETTL.Text & " , "
                ERRMSG_write(WW_ERR_MES, WW_LINEerr, "10023")
            End If

            '・キー項目(休日出勤：HWORKTIMETTL)
            '①必須・項目属性チェック
            WW_TIMEstr = WF_HWORKTIMETTL.Text.Split(":")
            If WW_TIMEstr.Length = 2 Then
                CS0036FCHECK.FIELD = "HWORKTIMETTL"
                CS0036FCHECK.VALUE = WW_TIMEstr(0)
                CS0036FCHECK.CS0036FCHECK()
                If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then
                    CS0036FCHECK.FIELD = "HWORKTIMETTL"
                    CS0036FCHECK.VALUE = WW_TIMEstr(1)
                    CS0036FCHECK.CS0036FCHECK()
                    If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then
                        If Val(WW_TIMEstr(1)) < 60 Then
                        Else
                            'エラーレポート編集
                            Dim WW_ERR_MES As String = ""
                            WW_ERR_MES = "・更新できないレコード(休日出勤エラー)です。"
                            WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> " & WF_HWORKTIMETTL.Text & " , "
                            ERRMSG_write(WW_ERR_MES, WW_LINEerr, "10023")
                        End If
                    Else
                        'エラーレポート編集
                        Dim WW_ERR_MES As String = ""
                        WW_ERR_MES = "・更新できないレコード(休日出勤エラー)です。"
                        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> " & CS0036FCHECK.CHECKREPORT & " , "
                        ERRMSG_write(WW_ERR_MES, WW_LINEerr, "10023")
                    End If
                Else
                    'エラーレポート編集
                    Dim WW_ERR_MES As String = ""
                    WW_ERR_MES = "・更新できないレコード(休日出勤エラー)です。"
                    WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> " & CS0036FCHECK.CHECKREPORT & " , "
                    ERRMSG_write(WW_ERR_MES, WW_LINEerr, "10023")
                End If
            Else
                'エラーレポート編集
                Dim WW_ERR_MES As String = ""
                WW_ERR_MES = "・更新できないレコード(休日出勤エラー)です。"
                WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> " & WF_HWORKTIMETTL.Text & " , "
                ERRMSG_write(WW_ERR_MES, WW_LINEerr, "10023")
            End If

            '・キー項目(休日深夜：HWORKTIMETTL)
            '①必須・項目属性チェック
            WW_TIMEstr = WF_HNIGHTTIMETTL.Text.Split(":")
            If WW_TIMEstr.Length = 2 Then
                CS0036FCHECK.FIELD = "HNIGHTTIMETTL"
                CS0036FCHECK.VALUE = WW_TIMEstr(0)
                CS0036FCHECK.CS0036FCHECK()
                If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then
                    CS0036FCHECK.FIELD = "HNIGHTTIMETTL"
                    CS0036FCHECK.VALUE = WW_TIMEstr(1)
                    CS0036FCHECK.CS0036FCHECK()
                    If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then
                        If Val(WW_TIMEstr(1)) < 60 Then
                        Else
                            'エラーレポート編集
                            Dim WW_ERR_MES As String = ""
                            WW_ERR_MES = "・更新できないレコード(休日深夜エラー)です。"
                            WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> " & WF_HNIGHTTIMETTL.Text & " , "
                            ERRMSG_write(WW_ERR_MES, WW_LINEerr, "10023")
                        End If
                    Else
                        'エラーレポート編集
                        Dim WW_ERR_MES As String = ""
                        WW_ERR_MES = "・更新できないレコード(休日深夜エラー)です。"
                        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> " & CS0036FCHECK.CHECKREPORT & " , "
                        ERRMSG_write(WW_ERR_MES, WW_LINEerr, "10023")
                    End If
                Else
                    'エラーレポート編集
                    Dim WW_ERR_MES As String = ""
                    WW_ERR_MES = "・更新できないレコード(休日深夜エラー)です。"
                    WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> " & CS0036FCHECK.CHECKREPORT & " , "
                    ERRMSG_write(WW_ERR_MES, WW_LINEerr, "10023")
                End If
            Else
                'エラーレポート編集
                Dim WW_ERR_MES As String = ""
                WW_ERR_MES = "・更新できないレコード(休日深夜エラー)です。"
                WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> " & WF_HWORKTIMETTL.Text & " , "
                ERRMSG_write(WW_ERR_MES, WW_LINEerr, "10023")
            End If

            '・キー項目(日曜出勤：SWORKTIMETTL)
            '①必須・項目属性チェック
            WW_TIMEstr = WF_SWORKTIMETTL.Text.Split(":")
            If WW_TIMEstr.Length = 2 Then
                CS0036FCHECK.FIELD = "SWORKTIMETTL"
                CS0036FCHECK.VALUE = WW_TIMEstr(0)
                CS0036FCHECK.CS0036FCHECK()
                If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then
                    CS0036FCHECK.FIELD = "SWORKTIMETTL"
                    CS0036FCHECK.VALUE = WW_TIMEstr(1)
                    CS0036FCHECK.CS0036FCHECK()
                    If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then
                        If Val(WW_TIMEstr(1)) < 60 Then
                        Else
                            'エラーレポート編集
                            Dim WW_ERR_MES As String = ""
                            WW_ERR_MES = "・更新できないレコード(日曜出勤エラー)です。"
                            WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> " & WF_SWORKTIMETTL.Text & " , "
                            ERRMSG_write(WW_ERR_MES, WW_LINEerr, "10023")
                        End If
                    Else
                        'エラーレポート編集
                        Dim WW_ERR_MES As String = ""
                        WW_ERR_MES = "・更新できないレコード(日曜出勤エラー)です。"
                        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> " & CS0036FCHECK.CHECKREPORT & " , "
                        ERRMSG_write(WW_ERR_MES, WW_LINEerr, "10023")
                    End If
                Else
                    'エラーレポート編集
                    Dim WW_ERR_MES As String = ""
                    WW_ERR_MES = "・更新できないレコード(日曜出勤エラー)です。"
                    WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> " & CS0036FCHECK.CHECKREPORT & " , "
                    ERRMSG_write(WW_ERR_MES, WW_LINEerr, "10023")
                End If
            Else
                'エラーレポート編集
                Dim WW_ERR_MES As String = ""
                WW_ERR_MES = "・更新できないレコード(日曜出勤エラー)です。"
                WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> " & WF_SWORKTIMETTL.Text & " , "
                ERRMSG_write(WW_ERR_MES, WW_LINEerr, "10023")
            End If

            '・キー項目(日曜深夜：SWORKTIMETTL)
            '①必須・項目属性チェック
            WW_TIMEstr = WF_SWORKTIMETTL.Text.Split(":")
            If WW_TIMEstr.Length = 2 Then
                CS0036FCHECK.FIELD = "SNIGHTTIMETTL"
                CS0036FCHECK.VALUE = WW_TIMEstr(0)
                CS0036FCHECK.CS0036FCHECK()
                If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then
                    CS0036FCHECK.FIELD = "SNIGHTTIMETTL"
                    CS0036FCHECK.VALUE = WW_TIMEstr(1)
                    CS0036FCHECK.CS0036FCHECK()
                    If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then
                        If Val(WW_TIMEstr(1)) < 60 Then
                        Else
                            'エラーレポート編集
                            Dim WW_ERR_MES As String = ""
                            WW_ERR_MES = "・更新できないレコード(日曜深夜エラー)です。"
                            WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> " & WF_SWORKTIMETTL.Text & " , "
                            ERRMSG_write(WW_ERR_MES, WW_LINEerr, "10023")
                        End If
                    Else
                        'エラーレポート編集
                        Dim WW_ERR_MES As String = ""
                        WW_ERR_MES = "・更新できないレコード(日曜深夜エラー)です。"
                        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> " & CS0036FCHECK.CHECKREPORT & " , "
                        ERRMSG_write(WW_ERR_MES, WW_LINEerr, "10023")
                    End If
                Else
                    'エラーレポート編集
                    Dim WW_ERR_MES As String = ""
                    WW_ERR_MES = "・更新できないレコード(日曜深夜エラー)です。"
                    WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> " & CS0036FCHECK.CHECKREPORT & " , "
                    ERRMSG_write(WW_ERR_MES, WW_LINEerr, "10023")
                End If
            Else
                'エラーレポート編集
                Dim WW_ERR_MES As String = ""
                WW_ERR_MES = "・更新できないレコード(日曜深夜エラー)です。"
                WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> " & WF_SWORKTIMETTL.Text & " , "
                ERRMSG_write(WW_ERR_MES, WW_LINEerr, "10023")
            End If

            '・キー項目(所定深夜：NIGHTTIMETTL)
            '①必須・項目属性チェック
            WW_TIMEstr = WF_NIGHTTIMETTL.Text.Split(":")
            If WW_TIMEstr.Length = 2 Then
                CS0036FCHECK.FIELD = "NIGHTTIMETTL"
                CS0036FCHECK.VALUE = WW_TIMEstr(0)
                CS0036FCHECK.CS0036FCHECK()
                If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then
                    CS0036FCHECK.FIELD = "NIGHTTIMETTL"
                    CS0036FCHECK.VALUE = WW_TIMEstr(1)
                    CS0036FCHECK.CS0036FCHECK()
                    If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then
                        If Val(WW_TIMEstr(1)) < 60 Then
                        Else
                            'エラーレポート編集
                            Dim WW_ERR_MES As String = ""
                            WW_ERR_MES = "・更新できないレコード(所定深夜エラー)です。"
                            WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> " & WF_NIGHTTIMETTL.Text & " , "
                            ERRMSG_write(WW_ERR_MES, WW_LINEerr, "10023")
                        End If
                    Else
                        'エラーレポート編集
                        Dim WW_ERR_MES As String = ""
                        WW_ERR_MES = "・更新できないレコード(所定深夜エラー)です。"
                        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> " & CS0036FCHECK.CHECKREPORT & " , "
                        ERRMSG_write(WW_ERR_MES, WW_LINEerr, "10023")
                    End If
                Else
                    'エラーレポート編集
                    Dim WW_ERR_MES As String = ""
                    WW_ERR_MES = "・更新できないレコード(所定深夜エラー)です。"
                    WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> " & CS0036FCHECK.CHECKREPORT & " , "
                    ERRMSG_write(WW_ERR_MES, WW_LINEerr, "10023")
                End If
            Else
                'エラーレポート編集
                Dim WW_ERR_MES As String = ""
                WW_ERR_MES = "・更新できないレコード(所定深夜エラー)です。"
                WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> " & WF_SWORKTIMETTL.Text & " , "
                ERRMSG_write(WW_ERR_MES, WW_LINEerr, "10023")
            End If

            '・キー項目(単車ＬＮＧ配送距離：MODELDISTANCETTL)
            '①必須・項目属性チェック
            CS0036FCHECK.FIELD = "MODELDISTANCETTL"
            CS0036FCHECK.VALUE = WF_MODELDISTANCE_LNG1.Text
            CS0036FCHECK.CS0036FCHECK()
            If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then
                WF_MODELDISTANCE_LNG1.Text = Val(CS0036FCHECK.VALUEOUT)
            Else
                'エラーレポート編集
                Dim WW_ERR_MES As String = ""
                WW_ERR_MES = "・更新できないレコード(単車ＬＮＧモデル距離エラー)です。"
                WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> " & CS0036FCHECK.CHECKREPORT & " , "
                ERRMSG_write(WW_ERR_MES, WW_LINEerr, "10023")
            End If

            '・キー項目(トレーラＬＮＧ配送距離：MODELDISTANCETTL)
            '①必須・項目属性チェック
            CS0036FCHECK.FIELD = "MODELDISTANCETTL"
            CS0036FCHECK.VALUE = WF_MODELDISTANCE_LNG2.Text
            CS0036FCHECK.CS0036FCHECK()
            If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then
                WF_MODELDISTANCE_LNG2.Text = Val(CS0036FCHECK.VALUEOUT)
            Else
                'エラーレポート編集
                Dim WW_ERR_MES As String = ""
                WW_ERR_MES = "・更新できないレコード(トレーラＬＮＧモデル距離エラー)です。"
                WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> " & CS0036FCHECK.CHECKREPORT & " , "
                ERRMSG_write(WW_ERR_MES, WW_LINEerr, "10023")
            End If

            '・キー項目(単車ラテックス配送距離：MODELDISTANCETTL)
            '①必須・項目属性チェック
            CS0036FCHECK.FIELD = "MODELDISTANCETTL"
            CS0036FCHECK.VALUE = WF_MODELDISTANCE_RATE1.Text
            CS0036FCHECK.CS0036FCHECK()
            If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then
                WF_MODELDISTANCE_RATE1.Text = Val(CS0036FCHECK.VALUEOUT)
            Else
                'エラーレポート編集
                Dim WW_ERR_MES As String = ""
                WW_ERR_MES = "・更新できないレコード(単車ラテックスモデル距離エラー)です。"
                WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> " & CS0036FCHECK.CHECKREPORT & " , "
                ERRMSG_write(WW_ERR_MES, WW_LINEerr, "10023")
            End If

            '・キー項目(トレーララテックス配送距離：MODELDISTANCETTL)
            '①必須・項目属性チェック
            CS0036FCHECK.FIELD = "MODELDISTANCETTL"
            CS0036FCHECK.VALUE = WF_MODELDISTANCE_RATE2.Text
            CS0036FCHECK.CS0036FCHECK()
            If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then
                WF_MODELDISTANCE_RATE2.Text = Val(CS0036FCHECK.VALUEOUT)
            Else
                'エラーレポート編集
                Dim WW_ERR_MES As String = ""
                WW_ERR_MES = "・更新できないレコード(トレーララテックスモデル距離エラー)です。"
                WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> " & CS0036FCHECK.CHECKREPORT & " , "
                ERRMSG_write(WW_ERR_MES, WW_LINEerr, "10023")
            End If

        End If

        If work.WF_T7KIN_RECODEKBN.Text = "0" Then
            '◆関連チェック◆
            Dim WW_ERR As String = ""
            Dim WW_ERR_MES1 As String = ""
            '01:年休, 02 : 特休, 04 : ｽﾄｯｸ, 05 : 協約週休, 06 : 週休
            '07:傷欠, 08 : 組欠, 09 : 他欠, 11 : 代休, 13 : 指定休, 15 : 振休
            If T0007COM.CheckHOLIDAY("0", WF_PAYKBN.Text) Then
                If WF_SHUKCHOKKBN.Text <> "0" Then
                    WW_ERR_MES1 = WW_ERR_MES1 & ControlChars.NewLine & "  --> 宿直区分 =" & WF_SHUKCHOKKBN.Text & " , "
                    WW_ERR = "ON"
                End If
                If WF_STTIME.Text <> "00:00" Then
                    WW_ERR_MES1 = WW_ERR_MES1 & ControlChars.NewLine & "  --> 出社時刻 =" & WF_STTIME.Text & " , "
                    WW_ERR = "ON"
                End If
                If WF_BINDSTDATE.Text <> "00:00" Then
                    WW_ERR_MES1 = WW_ERR_MES1 & ControlChars.NewLine & "  --> 拘束開始 =" & WF_BINDSTDATE.Text & " , "
                    WW_ERR = "ON"
                End If
                'If WF_BINDTIME.Text <> "00:00" Then
                '    WW_ERR = "ON"
                'End If
                If WF_ENDTIME.Text <> "00:00" Then
                    WW_ERR_MES1 = WW_ERR_MES1 & ControlChars.NewLine & "  --> 退社時刻 =" & WF_ENDTIME.Text & " , "
                    WW_ERR = "ON"
                End If
                If WF_TOKUSA1TIME.Text <> "00:00" Then
                    WW_ERR_MES1 = WW_ERR_MES1 & ControlChars.NewLine & "  --> 特作Ⅰ   =" & WF_TOKUSA1TIME.Text & " , "
                    WW_ERR = "ON"
                End If

                If WW_ERR = "ON" Then
                    'エラーレポート編集
                    Dim WW_ERR_MES As String = ""
                    WW_ERR_MES = "・更新できないレコードです。"
                    WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 休みが指定されているため、下記項目をクリアしてください。 ,"
                    WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & WW_ERR_MES1
                    ERRMSG_write(WW_ERR_MES, WW_LINEerr, "10023")
                End If
            Else
                If IsDate(WF_STDATE.Text) And IsDate(WF_STTIME.Text) And
                    IsDate(WF_ENDDATE.Text) And IsDate(WF_ENDTIME.Text) Then
                    Dim WW_STDATE As Date = CDate(WF_STDATE.Text & " " & WF_STTIME.Text)
                    Dim WW_ENDDATE As Date = CDate(WF_ENDDATE.Text & " " & WF_ENDTIME.Text)
                    If WW_STDATE > WW_ENDDATE Then
                        'エラーレポート編集
                        Dim WW_ERR_MES As String = ""
                        WW_ERR_MES = "・更新できないレコード(開始時刻　＞　終了時刻)です。"
                        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> " & WW_STDATE.ToString("yyyy/MM/dd HH:mm") & " > " & WW_ENDDATE.ToString("yyyy/MM/dd HH:mm") & " , "
                        ERRMSG_write(WW_ERR_MES, WW_LINEerr, "10018")
                    End If
                End If
            End If

        End If

        If WW_ERRLIST.Count > 0 Then
            If WW_ERRLIST.IndexOf(C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR) >= 0 Then
                RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            ElseIf WW_ERRLIST.IndexOf(C_MESSAGE_NO.BOX_ERROR_EXIST) >= 0 Then
                RTN = C_MESSAGE_NO.BOX_ERROR_EXIST
            End If
        End If

    End Sub

    ' ******************************************************************************
    ' ***  T0007INProwチェック
    ' ******************************************************************************
    Protected Sub T0007INProw_KANREN_CHEK(ByRef RTN As String)

        '○インターフェイス初期値設定
        RTN = C_MESSAGE_NO.NORMAL

        Dim WW_RESULT As String = ""
        Dim WW_TEXT As String = ""
        Dim WW_LINEerr As String = C_MESSAGE_NO.NORMAL
        Dim nullFlg As Boolean = False

        WW_ERRLIST = New List(Of String)

        If work.WF_T7KIN_RECODEKBN.Text = "0" Then
            '◆関連チェック◆
            Dim WW_ERR As String = ""
            Dim WW_ERR_MES1 As String = ""
            For i As Integer = 1 To 6
                Dim WW_SHARYOKBN As String = "WF_SHARYOKBN" & i
                Dim WW_OILPAYKBN As String = "WF_OILPAYKBN" & i
                Dim WW_SHUKABASHO As String = "WF_SHUKABASHO" & i
                Dim WW_TODOKECODE As String = "WF_TODOKECODE" & i
                Dim WW_MODELDISTANCE As String = "WF_MODELDISTANCE" & i
                Dim WW_MODIFYKBN As String = "WF_MODIFYKBN" & i

                Dim errMsg As String = ""

                If (CType(WF_DView3.FindControl(WW_SHARYOKBN), System.Web.UI.WebControls.TextBox).Text = "" AndAlso
                    CType(WF_DView3.FindControl(WW_OILPAYKBN), System.Web.UI.WebControls.TextBox).Text = "") AndAlso
                   (CType(WF_DView3.FindControl(WW_SHUKABASHO), System.Web.UI.WebControls.TextBox).Text <> "" OrElse
                    CType(WF_DView3.FindControl(WW_TODOKECODE), System.Web.UI.WebControls.TextBox).Text <> "" OrElse
                    CType(WF_DView3.FindControl(WW_TODOKECODE), System.Web.UI.WebControls.TextBox).Text <> "" OrElse
                    Val(CType(WF_DView3.FindControl(WW_MODELDISTANCE), System.Web.UI.WebControls.TextBox).Text) > 0) Then
                    'エラーレポート編集
                    If errMsg <> "" Then
                        errMsg = errMsg & "、"
                    End If
                    errMsg = errMsg & "車両区分、油種"
                End If

                'If (CType(WF_DView3.FindControl(WW_SHARYOKBN), System.Web.UI.WebControls.TextBox).Text <> "" OrElse
                '    CType(WF_DView3.FindControl(WW_OILPAYKBN), System.Web.UI.WebControls.TextBox).Text <> "") AndAlso
                '    CType(WF_DView3.FindControl(WW_SHUKABASHO), System.Web.UI.WebControls.TextBox).Text = "" AndAlso
                '    CType(WF_DView3.FindControl(WW_TODOKECODE), System.Web.UI.WebControls.TextBox).Text = "" Then
                '    'エラーレポート編集
                '    If errMsg <> "" Then
                '        errMsg = errMsg & "、"
                '    End If
                '    errMsg = errMsg & "出荷場所、届先"
                'End If

                'エラーレポート編集
                If errMsg <> "" Then
                    Dim WW_ERR_MES As String = "・更新できないレコード(No." & i & errMsg & "未入力)です。"
                    ERRMSG_write(WW_ERR_MES, WW_LINEerr, "10023")
                End If

                If CType(WF_DView3.FindControl(WW_SHARYOKBN), System.Web.UI.WebControls.TextBox).Text = "" AndAlso
                   CType(WF_DView3.FindControl(WW_OILPAYKBN), System.Web.UI.WebControls.TextBox).Text = "" AndAlso
                   CType(WF_DView3.FindControl(WW_SHUKABASHO), System.Web.UI.WebControls.TextBox).Text = "" AndAlso
                   CType(WF_DView3.FindControl(WW_TODOKECODE), System.Web.UI.WebControls.TextBox).Text = "" AndAlso
                   Val(CType(WF_DView3.FindControl(WW_MODELDISTANCE), System.Web.UI.WebControls.TextBox).Text) = 0 Then
                    CType(WF_DView3.FindControl(WW_MODIFYKBN), System.Web.UI.WebControls.CheckBox).Checked = False
                    nullFlg = True
                End If

                If nullFlg Then
                    If CType(WF_DView3.FindControl(WW_SHARYOKBN), System.Web.UI.WebControls.TextBox).Text <> "" OrElse
                       CType(WF_DView3.FindControl(WW_OILPAYKBN), System.Web.UI.WebControls.TextBox).Text <> "" Then

                        Dim WW_ERR_MES As String = ""
                        WW_ERR_MES = "・更新できないレコードです。"
                        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 間を開けずに入力してください。"
                        ERRMSG_write(WW_ERR_MES, WW_LINEerr, "10023")
                    End If
                End If

            Next
        End If

        If WW_ERRLIST.Count > 0 Then
            If WW_ERRLIST.IndexOf(C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR) >= 0 Then
                RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            ElseIf WW_ERRLIST.IndexOf(C_MESSAGE_NO.BOX_ERROR_EXIST) >= 0 Then
                RTN = C_MESSAGE_NO.BOX_ERROR_EXIST
            End If
        End If

    End Sub

    ' ***  エラーレポート編集
    Protected Sub ERRMSG_write(ByRef WW_ERR_MES As String, ByRef WW_LINEerr As String, ByVal I_ERRCD As String)

        rightview.AddErrorReport(WW_ERR_MES)

        WW_ERRLIST.Add(I_ERRCD)
        If WW_LINEerr <> "10023" Then
            WW_LINEerr = I_ERRCD
        End If

    End Sub

    ' *** GridView用データ取得                                                   
    Private Sub NIPPO_Screen()

        'ユーザプロファイル（変数）内容検索(自ユーザ権限＆抽出条件なしで検索)
        Try
            'DataBase接続文字
            Dim SQLcon As SqlConnection = CS0050SESSION.getConnection
            SQLcon.Open() 'DataBase接続(Open)

            '検索SQL文
            '　検索説明
            '　　Step1：操作USERが、メンテナンス可能なUSERを取得
            '　　　　　　※権限ではUSER、MAPで行う必要があるが、絞り込み効率を勘案し、最初にUSERで処理を限定
            '　　Step2：メンテナンス可能USERおよびデフォルトUSERのTBL(S0007_UPROFVARI)を取得
            '　　        画面表示は、参照可能および更新ユーザに関連するTBLデータとなる
            '　　　　　　※権限について（参考）　権限チャックは、表追加のタイミングで行う。
            '　　　　　　　　チェック内容
            '　　　　　　　　①操作USERは、TBL入力データ(USER)の更新権限をもっているか。
            '　　　　　　　　②TBL入力データ(USER)は、TBL入力データ(MAP)の参照および更新権限をもっているか。
            '　　　　　　　　③TBL入力データ(USER)は、TBL入力データ(CAMPCODE)の参照および更新権限をもっているか。
            '　　Step3：関連するグループコードを取得(操作USERに依存)
            '　　Step4：関連する名称を取得(TBL入力データ(USER)に依存)
            '　注意事項　日付について
            '　　権限判断はすべてDateNow。グループコード、名称取得は全てDateNow。表追加時の①はDateNow。
            '　　但し、表追加時の②および③は、TBL入力有効期限。

            Dim SQLStr As String =
                     "SELECT 0                                  as      LINECNT           , " _
                   & "       ''                                 as      OPERATION         , " _
                   & "       TIMSTP = cast(A.UPDTIMSTP as bigint)                         , " _
                   & "       0                                  as      'SELECT'          , " _
                   & "       1                                  as      HIDDEN            , " _
                   & "       ''                                 as      ORDERUMU          , " _
                   & "       '0'                                as      EXTRACTCNT        , " _
                   & "       'OFF'                              as      CTRL              , " _
                   & "       ''                                 as      TWOMANTRIP        , " _
                   & "       isnull(rtrim(A.CAMPCODE),'')       as      CAMPCODE          , " _
                   & "       isnull(rtrim(M1.NAMES),'')         as      CAMPNAMES         , " _
                   & "       isnull(rtrim(A.SHIPORG),'')        as      SHIPORG           , " _
                   & "       isnull(rtrim(M2.NAMES),'')         as      SHIPORGNAMES      , " _
                   & "       isnull(rtrim(A.TERMKBN),'')        as      TERMKBN           , " _
                   & "       isnull((select rtrim(F1.VALUE1) from MC001_FIXVALUE F1         " _
                   & "               where F1.CAMPCODE    = @P1 " _
                   & "               and   F1.CLASS       = 'TERMKBN' " _
                   & "               and   F1.KEYCODE     = A.TERMKBN " _
                   & "               and   F1.STYMD      <= A.YMD " _
                   & "               and   F1.ENDYMD     >= A.YMD " _
                   & "               and   F1.DELFLG     <> '1' " _
                   & "       ),'')                              as TERMKBNNAMES           , " _
                   & "       isnull(rtrim(A.YMD),'')            as      YMD               , " _
                   & "       isnull(rtrim(A.NIPPONO),'')        as      NIPPONO           , " _
                   & "       isnull(rtrim(A.WORKKBN),'')        as      WORKKBN           , " _
                   & "       isnull((select rtrim(F2.VALUE1) from MC001_FIXVALUE F2 " _
                   & "               where F2.CAMPCODE    = @P1 " _
                   & "               and   F2.CLASS       = 'WORKKBN' " _
                   & "               and   F2.KEYCODE     = A.WORKKBN " _
                   & "               and   F2.STYMD      <= A.YMD " _
                   & "               and   F2.ENDYMD     >= A.YMD " _
                   & "               and   F2.DELFLG     <> '1' " _
                   & "       ),'')                              as WORKKBNNAMES           , " _
                   & "       isnull(A.SEQ,'0')                  as      SEQ               , " _
                   & "       isnull(rtrim(A.STAFFCODE),'')      as      STAFFCODE         , " _
                   & "       isnull(rtrim(A.ENTRYDATE),'')      as      ENTRYDATE         , " _
                   & "       isnull(rtrim(B.STAFFNAMES),'')     as STAFFNAMES        , " _
                   & "       isnull(rtrim(A.SUBSTAFFCODE),'')   as SUBSTAFFCODE      , " _
                   & "       isnull(rtrim(B2.STAFFNAMES),'')    as SUBSTAFFNAMES     , " _
                   & "       isnull(rtrim(A.CREWKBN),'')        as CREWKBN           , " _
                   & "       isnull((select rtrim(F3.VALUE1) from MC001_FIXVALUE F3 " _
                   & "               where F3.CAMPCODE    = @P1 " _
                   & "               and   F3.CLASS       = 'CREWKBN' " _
                   & "               and   F3.KEYCODE     = A.CREWKBN " _
                   & "               and   F3.STYMD      <= A.YMD " _
                   & "               and   F3.ENDYMD     >= A.YMD " _
                   & "               and   F3.DELFLG     <> '1' " _
                   & "       ),'')                              as CREWKBNNAMES      , " _
                   & "       isnull(rtrim(A.GSHABAN),'')        as GSHABAN           , " _
                   & "       ''                                 as GSHABANLICNPLTNO  , " _
                   & "       isnull(rtrim(A.STDATE),'')         as STDATE , " _
                   & "       isnull(rtrim(A.STTIME),'')         as STTIME , " _
                   & "       isnull(rtrim(A.ENDDATE),'')        as ENDDATE , " _
                   & "       isnull(rtrim(A.ENDTIME),'')        as ENDTIME , " _
                   & "       isnull(rtrim(A.WORKTIME),'')       as WORKTIME , " _
                   & "       isnull(rtrim(A.MOVETIME),'')       as MOVETIME , " _
                   & "       isnull(rtrim(A.ACTTIME),'')        as ACTTIME , " _
                   & "       isnull(A.PRATE,'0')                as PRATE , " _
                   & "       isnull(A.CASH,'0')                 as CASH , " _
                   & "       isnull(A.TICKET,'0')               as TICKET , " _
                   & "       isnull(A.ETC,'0')                  as ETC , " _
                   & "       isnull(A.TOTALTOLL,'0')            as TOTALTOLL , " _
                   & "       isnull(A.STMATER,'0')              as STMATER , " _
                   & "       isnull(A.ENDMATER,'0')             as ENDMATER , " _
                   & "       isnull(A.RUIDISTANCE,'0')          as RUIDISTANCE , " _
                   & "       isnull(A.SOUDISTANCE,'0')          as SOUDISTANCE , " _
                   & "       isnull(A.JIDISTANCE,'0')           as JIDISTANCE , " _
                   & "       isnull(A.KUDISTANCE,'0')           as KUDISTANCE , " _
                   & "       isnull(A.IPPDISTANCE,'0')          as IPPDISTANCE , " _
                   & "       isnull(A.KOSDISTANCE,'0')          as KOSDISTANCE , " _
                   & "       isnull(A.IPPJIDISTANCE,'0')        as IPPJIDISTANCE , " _
                   & "       isnull(A.IPPKUDISTANCE,'0')        as IPPKUDISTANCE , " _
                   & "       isnull(A.KOSJIDISTANCE,'0')        as KOSJIDISTANCE , " _
                   & "       isnull(A.KOSKUDISTANCE,'0')        as KOSKUDISTANCE , " _
                   & "       isnull(A.KYUYU,'0')                as KYUYU , " _
                   & "       isnull(rtrim(A.TORICODE),'')       as TORICODE , " _
                   & "       isnull(rtrim(MC2.NAMES),'')        as TORINAMES , " _
                   & "       isnull(rtrim(A.SHUKABASHO),'')     as SHUKABASHO , " _
                   & "       isnull(rtrim(MC62.NAMES),'')       as SHUKABASHONAMES , " _
                   & "       isnull(rtrim(A.SHUKADATE),'')      as SHUKADATE , " _
                   & "       isnull(rtrim(A.TODOKECODE),'')     as TODOKECODE , " _
                   & "       isnull(rtrim(MC6.NAMES),'')        as TODOKENAMES , " _
                   & "       isnull(rtrim(A.TODOKEDATE),'')     as TODOKEDATE , " _
                   & "       isnull(rtrim(A.OILTYPE1),'')       as OILTYPE1 , " _
                   & "       isnull(rtrim(A.PRODUCT11),'')      as PRODUCT11 , " _
                   & "       isnull(rtrim(A.PRODUCT21),'')      as PRODUCT21 , " _
                   & "       isnull(rtrim(A.PRODUCTCODE1),'')   as PRODUCTCODE1 ," _
                   & "       ''                                 as PRODUCT1NAMES , " _
                   & "       isnull(A.SURYO1,'0')               as SURYO1 , " _
                   & "       isnull(rtrim(A.STANI1),'')         as STANI1 , " _
                   & "       isnull((select rtrim(F41.VALUE1) from MC001_FIXVALUE F41 " _
                   & "               where F41.CAMPCODE    = @P1 " _
                   & "               and   F41.CLASS       = 'STANI' " _
                   & "               and   F41.KEYCODE     = A.STANI1 " _
                   & "               and   F41.STYMD      <= A.YMD " _
                   & "               and   F41.ENDYMD     >= A.YMD " _
                   & "               and   F41.DELFLG     <> '1' " _
                   & "       ),'')                              as STANI1NAMES  , " _
                   & "       isnull(rtrim(A.OILTYPE2),'')       as OILTYPE2 , " _
                   & "       isnull(rtrim(A.PRODUCT12),'')      as PRODUCT12 , " _
                   & "       isnull(rtrim(A.PRODUCT22),'')      as PRODUCT22 , " _
                   & "       isnull(rtrim(A.PRODUCTCODE2),'')   as PRODUCTCODE2 ," _
                   & "       ''                                 as PRODUCT2NAMES , " _
                   & "       isnull(A.SURYO2,'0')               as SURYO2 , " _
                   & "       isnull(rtrim(A.STANI2),'')         as STANI2 , " _
                   & "       isnull((select rtrim(F42.VALUE1) from MC001_FIXVALUE F42 " _
                   & "               where F42.CAMPCODE    = @P1 " _
                   & "               and   F42.CLASS       = 'STANI' " _
                   & "               and   F42.KEYCODE     = A.STANI2 " _
                   & "               and   F42.STYMD      <= A.YMD " _
                   & "               and   F42.ENDYMD     >= A.YMD " _
                   & "               and   F42.DELFLG     <> '1' " _
                   & "       ),'')                              as STANI2NAMES  , " _
                   & "       isnull(rtrim(A.OILTYPE3),'')       as OILTYPE3 , " _
                   & "       isnull(rtrim(A.PRODUCT13),'')      as PRODUCT13 , " _
                   & "       isnull(rtrim(A.PRODUCT23),'')      as PRODUCT23 , " _
                   & "       isnull(rtrim(A.PRODUCTCODE3),'')   as PRODUCTCODE3 ," _
                   & "       ''                                 as PRODUCT3NAMES , " _
                   & "       isnull(A.SURYO3,'0')               as SURYO3 , " _
                   & "       isnull(rtrim(A.STANI3),'')         as STANI3 , " _
                   & "       isnull((select rtrim(F43.VALUE1) from MC001_FIXVALUE F43 " _
                   & "               where F43.CAMPCODE    = @P1 " _
                   & "               and   F43.CLASS       = 'STANI' " _
                   & "               and   F43.KEYCODE     = A.STANI3 " _
                   & "               and   F43.STYMD      <= A.YMD " _
                   & "               and   F43.ENDYMD     >= A.YMD " _
                   & "               and   F43.DELFLG     <> '1' " _
                   & "       ),'')                              as STANI3NAMES  , " _
                   & "       isnull(rtrim(A.OILTYPE4),'')       as OILTYPE4 , " _
                   & "       isnull(rtrim(A.PRODUCT14),'')      as PRODUCT14 , " _
                   & "       isnull(rtrim(A.PRODUCT24),'')      as PRODUCT24 , " _
                   & "       isnull(rtrim(A.PRODUCTCODE4),'')   as PRODUCTCODE4 ," _
                   & "       ''                                 as PRODUCT4NAMES , " _
                   & "       isnull(A.SURYO4,'0')               as SURYO4 , " _
                   & "       isnull(rtrim(A.STANI4),'')         as STANI4 , " _
                   & "       isnull((select rtrim(F44.VALUE1) from MC001_FIXVALUE F44 " _
                   & "               where F44.CAMPCODE    = @P1 " _
                   & "               and   F44.CLASS       = 'STANI' " _
                   & "               and   F44.KEYCODE     = A.STANI4 " _
                   & "               and   F44.STYMD      <= A.YMD " _
                   & "               and   F44.ENDYMD     >= A.YMD " _
                   & "               and   F44.DELFLG     <> '1' " _
                   & "       ),'')                              as STANI4NAMES  , " _
                   & "       isnull(rtrim(A.OILTYPE5),'')       as OILTYPE5 , " _
                   & "       isnull(rtrim(A.PRODUCT15),'')      as PRODUCT15 , " _
                   & "       isnull(rtrim(A.PRODUCT25),'')      as PRODUCT25 , " _
                   & "       isnull(rtrim(A.PRODUCTCODE5),'')   as PRODUCTCODE5 ," _
                   & "       ''                                 as PRODUCT5NAMES , " _
                   & "       isnull(A.SURYO5,'0')               as SURYO5 , " _
                   & "       isnull(rtrim(A.STANI5),'')         as STANI5 , " _
                   & "       isnull((select rtrim(F45.VALUE1) from MC001_FIXVALUE F45 " _
                   & "               where F45.CAMPCODE    = @P1 " _
                   & "               and   F45.CLASS       = 'STANI' " _
                   & "               and   F45.KEYCODE     = A.STANI5 " _
                   & "               and   F45.STYMD      <= A.YMD " _
                   & "               and   F45.ENDYMD     >= A.YMD " _
                   & "               and   F45.DELFLG     <> '1' " _
                   & "       ),'')                              as STANI5NAMES  , " _
                   & "       isnull(rtrim(A.OILTYPE6),'')       as OILTYPE6 , " _
                   & "       isnull(rtrim(A.PRODUCT16),'')      as PRODUCT16 , " _
                   & "       isnull(rtrim(A.PRODUCT26),'')      as PRODUCT26 , " _
                   & "       isnull(rtrim(A.PRODUCTCODE6),'')   as PRODUCTCODE6 ," _
                   & "       ''                                 as PRODUCT6NAMES , " _
                   & "       isnull(A.SURYO6,'0')               as SURYO6 , " _
                   & "       isnull(rtrim(A.STANI6),'')         as STANI6 , " _
                   & "       isnull((select rtrim(F46.VALUE1) from MC001_FIXVALUE F46 " _
                   & "               where F46.CAMPCODE    = @P1 " _
                   & "               and   F46.CLASS       = 'STANI' " _
                   & "               and   F46.KEYCODE     = A.STANI6 " _
                   & "               and   F46.STYMD      <= A.YMD " _
                   & "               and   F46.ENDYMD     >= A.YMD " _
                   & "               and   F46.DELFLG     <> '1' " _
                   & "       ),'')                              as STANI6NAMES  , " _
                   & "       isnull(rtrim(A.OILTYPE7),'')       as OILTYPE7 , " _
                   & "       isnull(rtrim(A.PRODUCT17),'')      as PRODUCT17 , " _
                   & "       isnull(rtrim(A.PRODUCT27),'')      as PRODUCT27 , " _
                   & "       isnull(rtrim(A.PRODUCTCODE7),'')   as PRODUCTCODE7 ," _
                   & "       ''                                 as PRODUCT7NAMES , " _
                   & "       isnull(A.SURYO7,'0')               as SURYO7 , " _
                   & "       isnull(rtrim(A.STANI7),'')         as STANI7 , " _
                   & "       isnull((select rtrim(F47.VALUE1) from MC001_FIXVALUE F47 " _
                   & "               where F47.CAMPCODE    = @P1 " _
                   & "               and   F47.CLASS       = 'STANI' " _
                   & "               and   F47.KEYCODE     = A.STANI7 " _
                   & "               and   F47.STYMD      <= A.YMD " _
                   & "               and   F47.ENDYMD     >= A.YMD " _
                   & "               and   F47.DELFLG     <> '1' " _
                   & "       ),'')                              as STANI7NAMES  , " _
                   & "       isnull(rtrim(A.OILTYPE8),'')       as OILTYPE8 , " _
                   & "       isnull(rtrim(A.PRODUCT18),'')      as PRODUCT18 , " _
                   & "       isnull(rtrim(A.PRODUCT28),'')      as PRODUCT28 , " _
                   & "       isnull(rtrim(A.PRODUCTCODE8),'')   as PRODUCTCODE8 ," _
                   & "       ''                                 as PRODUCT8NAMES , " _
                   & "       isnull(A.SURYO8,'0')               as SURYO8 , " _
                   & "       isnull(rtrim(A.STANI8),'')         as STANI8 , " _
                   & "       isnull((select rtrim(F48.VALUE1) from MC001_FIXVALUE F48 " _
                   & "               where F48.CAMPCODE    = @P1 " _
                   & "               and   F48.CLASS       = 'STANI' " _
                   & "               and   F48.KEYCODE     = A.STANI8 " _
                   & "               and   F48.STYMD      <= A.YMD " _
                   & "               and   F48.ENDYMD     >= A.YMD " _
                   & "               and   F48.DELFLG     <> '1' " _
                   & "       ),'')                              as STANI8NAMES  , " _
                   & "       isnull(A.TOTALSURYO,'0')           as TOTALSURYO , " _
                   & "       isnull(rtrim(A.ORDERNO),'')        as ORDERNO , " _
                   & "       isnull(rtrim(A.DETAILNO),'')       as DETAILNO , " _
                   & "       isnull(rtrim(A.TRIPNO),'')         as TRIPNO , " _
                   & "       isnull(rtrim(A.DROPNO),'')         as DROPNO , " _
                   & "       isnull(rtrim(A.JISSKIKBN),'')      as JISSKIKBN , " _
                   & "       ''                                 as JISSKIKBNNAMES , " _
                   & "       isnull(rtrim(A.URIKBN),'')         as URIKBN , " _
                   & "       isnull((select rtrim(F6.VALUE1) from MC001_FIXVALUE F6 " _
                   & "               where F6.CAMPCODE    = @P1 " _
                   & "               and   F6.CLASS       = 'URIKBN' " _
                   & "               and   F6.KEYCODE     = A.URIKBN " _
                   & "               and   F6.STYMD      <= A.YMD " _
                   & "               and   F6.ENDYMD     >= A.YMD " _
                   & "               and   F6.DELFLG     <> '1' " _
                   & "       ),'')                              as URIKBNNAMES  , " _
                   & "       isnull(rtrim(A.TUMIOKIKBN),'')     as TUMIOKIKBN , " _
                   & "       isnull((select rtrim(F5.VALUE1) from MC001_FIXVALUE F5 " _
                   & "               where F5.CAMPCODE    = @P1 " _
                   & "               and   F5.CLASS       = 'TUMIOKIKBN' " _
                   & "               and   F5.KEYCODE     = A.TUMIOKIKBN " _
                   & "               and   F5.STYMD      <= A.YMD " _
                   & "               and   F5.ENDYMD     >= A.YMD " _
                   & "               and   F5.DELFLG     <> '1' " _
                   & "       ),'')                              as TUMIOKIKBNNAMES  , " _
                   & "       isnull(rtrim(A.STORICODE),'')      as STORICODE , " _
                   & "       isnull(rtrim(MC22.NAMES),'')       as STORICODENAMES , " _
                   & "       isnull(rtrim(A.CONTCHASSIS),'')    as CONTCHASSIS , " _
                   & "       ''                                 as CONTCHASSISLICNPLTNO , " _
                   & "       isnull(rtrim(A.SHARYOTYPEF),'')    as SHARYOTYPEF , " _
                   & "       isnull(rtrim(A.TSHABANF),'')       as TSHABANF , " _
                   & "       isnull(rtrim(A.SHARYOTYPEB),'')    as SHARYOTYPEB , " _
                   & "       isnull(rtrim(A.TSHABANB),'')       as TSHABANB , " _
                   & "       isnull(rtrim(A.SHARYOTYPEB2),'')   as SHARYOTYPEB2 , " _
                   & "       isnull(rtrim(A.TSHABANB2),'')      as TSHABANB2 , " _
                   & "       isnull(rtrim(A.TAXKBN),'')         as TAXKBN , " _
                   & "       isnull((select rtrim(F7.VALUE1) from MC001_FIXVALUE F7 " _
                   & "               where F7.CAMPCODE    = @P1 " _
                   & "               and   F7.CLASS       = 'TAXKBN' " _
                   & "               and   F7.KEYCODE     = A.TAXKBN " _
                   & "               and   F7.STYMD      <= A.YMD " _
                   & "               and   F7.ENDYMD     >= A.YMD " _
                   & "               and   F7.DELFLG     <> '1' " _
                   & "       ),'')                              as TAXKBNNAMES  , " _
                   & "       isnull(rtrim(A.LATITUDE),'')       as LATITUDE , " _
                   & "       isnull(rtrim(A.LONGITUDE),'')      as LONGITUDE , " _
                   & "       isnull(rtrim(A.L1HAISOGROUP),'')   as wHaisoGroup , " _
                   & "       isnull(rtrim(A.DELFLG),'0')        as DELFLG , " _
                   & "       ''                                 as HOLIDAYKBN , " _
                   & "       ''                                 as TORITYPE01 , " _
                   & "       ''                                 as TORITYPE02 , " _
                   & "       ''                                 as TORITYPE03 , " _
                   & "       ''                                 as TORITYPE04 , " _
                   & "       ''                                 as TORITYPE05 , " _
                   & "       ''                                 as SUPPLIERKBN , " _
                   & "       ''                                 as SUPPLIER , " _
                   & "       ''                                 as MANGOILTYPE , " _
                   & "       ''                                 as MANGMORG1 , " _
                   & "       ''                                 as MANGSORG1 , " _
                   & "       ''                                 as MANGUORG1 , " _
                   & "       ''                                 as BASELEASE1 , " _
                   & "       ''                                 as MANGMORG2 , " _
                   & "       ''                                 as MANGSORG2 , " _
                   & "       ''                                 as MANGUORG2 , " _
                   & "       ''                                 as BASELEASE2 , " _
                   & "       ''                                 as MANGMORG3 , " _
                   & "       ''                                 as MANGSORG3 , " _
                   & "       ''                                 as MANGUORG3 , " _
                   & "       ''                                 as BASELEASE3 , " _
                   & "       ''                                 as STAFFKBN , " _
                   & "       ''                                 as MORG , " _
                   & "       ''                                 as HORG , " _
                   & "       ''                                 as SUBSTAFFKBN , " _
                   & "       ''                                 as SUBMORG , " _
                   & "       ''                                 as SUBHORG , " _
                   & "       ''                                 as ORDERORG , " _
                   & "       '' as wSHUKODATE, " _
                   & "       '' as wSHUKADATE, " _
                   & "       '' as wTODOKEDATE, " _
                   & "       '' as wTRIPNO_K, " _
                   & "       '' as wTRIPNO, " _
                   & "       '' as wDROPNO, " _
                   & "       '' as wTORICODE, " _
                   & "       '' as wURIKBN, " _
                   & "       '' as wSTORICODE, " _
                   & "       '' as wTODOKECODE, " _
                   & "       '' as wSHUKABASHO, " _
                   & "       '' as wCREWKBN, " _
                   & "       '' as wSTAFFKBN, " _
                   & "       '' as wSTAFFCODE, " _
                   & "       '' as wSUBSTAFFCODE, " _
                   & "       '' as wORDERNO, " _
                   & "       '' as wDETAILNO, " _
                   & "       '' as wORDERORG, " _
                   & "       '' as wKAISO, " _
                   & "       '' as wKUSHAKBN, " _
                   & "       '' as wTRIPDROPcnt, " _
                   & "       '' as wDATECHANGE, " _
                   & "       '' as wLASTstat, " _
                   & "       '' as wFirstCNTUP, " _
                   & "       '' as wF1F3flg, " _
                   & "       '' as wIPPDISTANCE, " _
                   & "       '' as wKOSDISTANCE, " _
                   & "       '' as wIPPJIDISTANCE, " _
                   & "       '' as wIPPKUDISTANCE, " _
                   & "       '' as wKOSJIDISTANCE, " _
                   & "       '' as wKOSKUDISTANCE, " _
                   & "       '' as wWORKTIME, " _
                   & "       '' as wMOVETIME, " _
                   & "       '' as wACTTIME, " _
                   & "       '' as wJIMOVETIME, " _
                   & "       '' as wKUMOVETIME, " _
                   & "       '' as wKAIJI, " _
                   & "       '' as wSUISOKBN " _
                   & " FROM      T0005_NIPPO A " _
                   & " LEFT JOIN MB001_STAFF B " _
                   & "   ON    B.CAMPCODE    = A.CAMPCODE " _
                   & "   and   B.STAFFCODE   = A.STAFFCODE " _
                   & "   and   B.STYMD      <= A.YMD " _
                   & "   and   B.ENDYMD     >= A.YMD " _
                   & "   and   B.STYMD       = ( " _
                   & "    SELECT MAX(STYMD)  " _
                   & "    FROM     MB001_STAFF    B2 " _
                   & "    WHERE B2.CAMPCODE = A.CAMPCODE and B2.STAFFCODE = A.STAFFCODE and B2.STYMD <= A.YMD and B2.ENDYMD >= A.YMD and DELFLG <> '1' ) " _
                   & "   and   B.DELFLG     <> '1' " _
                   & " LEFT JOIN MB001_STAFF B2 " _
                   & "   ON    B2.CAMPCODE    = @P1 " _
                   & "   and   B2.STAFFCODE   = A.SUBSTAFFCODE " _
                   & "   and   B2.STYMD      <= A.YMD " _
                   & "   and   B2.ENDYMD     >= A.YMD " _
                   & "   and   B2.STYMD       = (SELECT MAX(STYMD) FROM MB001_STAFF WHERE CAMPCODE = @P1 and STAFFCODE = A.SUBSTAFFCODE and STYMD <= A.YMD and ENDYMD >= A.YMD and DELFLG <> '1' ) " _
                   & "   and   B2.DELFLG     <> '1' " _
                   & " LEFT JOIN M0001_CAMP M1 " _
                   & "   ON    M1.CAMPCODE    = @P1 " _
                   & "   and   M1.STYMD      <= A.YMD " _
                   & "   and   M1.ENDYMD     >= A.YMD " _
                   & "   and   M1.STYMD       = (SELECT MAX(STYMD) FROM M0001_CAMP WHERE CAMPCODE = @P1 and STYMD <= A.YMD and ENDYMD >= A.YMD and DELFLG <> '1' )" _
                   & "   and   M1.DELFLG     <> '1' " _
                   & " LEFT JOIN M0002_ORG M2 " _
                   & "   ON    M2.CAMPCODE    = @P1 " _
                   & "   and   M2.ORGCODE     = A.SHIPORG " _
                   & "   and   M2.STYMD      <= A.YMD " _
                   & "   and   M2.ENDYMD     >= A.YMD " _
                   & "   and   M2.STYMD       = (SELECT MAX(STYMD) FROM M0002_ORG WHERE CAMPCODE = @P1 and ORGCODE = A.SHIPORG and STYMD <= A.YMD and ENDYMD >= A.YMD and DELFLG <> '1' )" _
                   & "   and   M2.DELFLG     <> '1' " _
                   & " LEFT JOIN MC002_TORIHIKISAKI MC2 " _
                   & "   ON    MC2.TORICODE    = A.TORICODE " _
                   & "   and   MC2.CAMPCODE    = @P1 " _
                   & "   and   MC2.STYMD      <= A.YMD " _
                   & "   and   MC2.ENDYMD     >= A.YMD " _
                   & "   and   MC2.STYMD       = (SELECT MAX(STYMD) FROM MC002_TORIHIKISAKI WHERE CAMPCODE = @P1 and TORICODE = A.TORICODE and STYMD <= A.YMD and ENDYMD >= A.YMD and DELFLG <> '1' )" _
                   & "   and   MC2.DELFLG     <> '1' " _
                   & " LEFT JOIN MC002_TORIHIKISAKI MC22 " _
                   & "   ON    MC22.TORICODE    = A.STORICODE " _
                   & "   and   MC22.CAMPCODE    = @P1 " _
                   & "   and   MC22.STYMD      <= A.YMD " _
                   & "   and   MC22.ENDYMD     >= A.YMD " _
                   & "   and   MC22.STYMD       = (SELECT MAX(STYMD) FROM MC002_TORIHIKISAKI WHERE CAMPCODE = @P1 and TORICODE = A.STORICODE and STYMD <= A.YMD and ENDYMD >= A.YMD and DELFLG <> '1' )" _
                   & "   and   MC22.DELFLG     <> '1' " _
                   & " LEFT JOIN MC006_TODOKESAKI MC6 " _
                   & "   ON    MC6.CAMPCODE    = A.CAMPCODE " _
                   & "   and   MC6.TODOKECODE  = A.TODOKECODE " _
                   & "   and   MC6.CLASS      in ('1','') " _
                   & "   and   MC6.STYMD       = (SELECT MAX(STYMD) FROM MC006_TODOKESAKI WHERE CAMPCODE = A.CAMPCODE and TODOKECODE = A.TODOKECODE and CLASS in('1','') and STYMD <= A.YMD and ENDYMD >= A.YMD and DELFLG <> '1' )" _
                   & "   and   MC6.STYMD      <= A.YMD " _
                   & "   and   MC6.ENDYMD     >= A.YMD " _
                   & "   and   MC6.DELFLG     <> '1' " _
                   & " LEFT JOIN MC006_TODOKESAKI MC62 " _
                   & "   ON    MC62.CAMPCODE    = A.CAMPCODE " _
                   & "   and   MC62.TODOKECODE = A.SHUKABASHO " _
                   & "   and   MC62.CLASS      in ('2','') " _
                   & "   and   MC62.STYMD      = (SELECT MAX(STYMD) FROM MC006_TODOKESAKI WHERE CAMPCODE = A.CAMPCODE and TODOKECODE = A.SHUKABASHO and CLASS in ('2','') and STYMD <= A.YMD and ENDYMD >= A.YMD and DELFLG <> '1' )" _
                   & "   and   MC62.STYMD     <= A.YMD " _
                   & "   and   MC62.ENDYMD    >= A.YMD " _
                   & "   and   MC62.DELFLG    <> '1' " _
                   & " WHERE   " _
                   & "         A.CAMPCODE    = @P1 " _
                   & "   and   A.SHIPORG     = @P2 " _
                   & "   and   A.YMD        <= @P4 " _
                   & "   and   A.YMD        >= @P3 " _
                   & "   and   A.DELFLG     <> '1' "

            Dim SQLWhere As String = ""
            SQLWhere = " and   A.STAFFCODE   = @STAFFCODE "
            Dim SQLStr2 As String = SQLStr & SQLWhere
            Dim SQLcmd As New SqlCommand(SQLStr2, SQLcon)
            Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.NVarChar)
            Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.NVarChar)
            Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.Date)
            Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", System.Data.SqlDbType.Date)
            Dim PARA5 As SqlParameter = SQLcmd.Parameters.Add("@P5", System.Data.SqlDbType.Date)
            Dim PARA6 As SqlParameter = SQLcmd.Parameters.Add("@P6", System.Data.SqlDbType.NVarChar)
            Dim P_STAFFCODE As SqlParameter = SQLcmd.Parameters.Add("@STAFFCODE", System.Data.SqlDbType.NVarChar)
            PARA1.Value = work.WF_T7SEL_CAMPCODE.Text
            PARA2.Value = work.WF_T7SEL_HORG.Text
            PARA3.Value = WF_WORKDATE.Text
            PARA4.Value = WF_WORKDATE.Text
            PARA5.Value = Date.Now
            PARA6.Value = CS0050SESSION.APSV_ID
            P_STAFFCODE.Value = WF_STAFFCODE.Text
            Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

            '■テーブル検索結果をテーブル退避
            '日報DB更新用テーブル
            T0005COM.AddColumnT0005tbl(T0005tbl)

            T0005tbl.Load(SQLdr)

            '----------------------------
            '一週間前の日報を取得
            '----------------------------
            Dim WW_SORT As String = "ORDER BY A.YMD , A.STAFFCODE , A.STDATE , A.STTIME"

            SQLStr2 = SQLStr & WW_SORT
            Dim SQLcmd2 As New SqlCommand(SQLStr2, SQLcon)
            Dim PARA21 As SqlParameter = SQLcmd2.Parameters.Add("@P1", System.Data.SqlDbType.NVarChar)
            Dim PARA22 As SqlParameter = SQLcmd2.Parameters.Add("@P2", System.Data.SqlDbType.NVarChar)
            Dim PARA23 As SqlParameter = SQLcmd2.Parameters.Add("@P3", System.Data.SqlDbType.Date)
            Dim PARA24 As SqlParameter = SQLcmd2.Parameters.Add("@P4", System.Data.SqlDbType.Date)
            Dim PARA25 As SqlParameter = SQLcmd2.Parameters.Add("@P5", System.Data.SqlDbType.Date)
            Dim PARA26 As SqlParameter = SQLcmd2.Parameters.Add("@P6", System.Data.SqlDbType.NVarChar)
            PARA21.Value = work.WF_T7SEL_CAMPCODE.Text
            PARA22.Value = work.WF_T7SEL_HORG.Text
            Dim WW_date As Date = Date.Parse(WF_WORKDATE.Text)
            ' 一週間前
            Dim WW_Fdate As Date = WW_date.AddDays(-7)
            Dim WW_Tdate As Date = WW_date.AddDays(-1)
            PARA23.Value = WW_Fdate.ToString("yyyy/MM/dd")
            PARA24.Value = WW_Tdate.ToString("yyyy/MM/dd")
            PARA25.Value = Date.Now
            PARA26.Value = CS0050SESSION.APSV_ID
            Dim SQLdr2 As SqlDataReader = SQLcmd2.ExecuteReader()

            '■テーブル検索結果をテーブル退避
            '日報DB更新用テーブル
            T0005COM.AddColumnT0005tbl(T0005WEEKtbl)
            T0005WEEKtbl.Load(SQLdr2)

            '一週間前～開始日付－１日をマージ
            T0005tbl.Merge(T0005WEEKtbl)


            For i As Integer = 0 To T0005tbl.Rows.Count - 1
                T0005row = T0005tbl.Rows(i)
                If IsDate(T0005row("YMD")) Then
                    T0005row("YMD") = CDate(T0005row("YMD")).ToString("yyyy/MM/dd")
                Else
                    T0005row("YMD") = "1950/01/01"
                End If

                T0005row("SELECT") = "1"      '対象データ
                T0005row("HIDDEN") = "1"      '非表示

                T0005row("HDKBN") = "D"       'ヘッダ、明細区分
                If IsDate(T0005row("SHUKADATE")) Then
                    T0005row("SHUKADATE") = CDate(T0005row("SHUKADATE")).ToString("yyyy/MM/dd")
                End If
                If IsDate(T0005row("TODOKEDATE")) Then
                    T0005row("TODOKEDATE") = CDate(T0005row("TODOKEDATE")).ToString("yyyy/MM/dd")
                End If
                T0005row("SEQ") = CInt(T0005row("SEQ")).ToString("000")
                If IsDate(T0005row("STDATE")) Then
                    T0005row("STDATE") = CDate(T0005row("STDATE")).ToString("yyyy/MM/dd")
                Else
                    T0005row("STDATE") = "1950/01/01"
                End If
                If IsDate(T0005row("STTIME")) Then
                    T0005row("STTIME") = CDate(T0005row("STTIME")).ToString("HH:mm")
                Else
                    T0005row("STTIME") = "00:00"
                End If
                If IsDate(T0005row("ENDDATE")) Then
                    T0005row("ENDDATE") = CDate(T0005row("ENDDATE")).ToString("yyyy/MM/dd")
                Else
                    T0005row("ENDDATE") = "1950/01/01"
                End If
                If IsDate(T0005row("STTIME")) Then
                    T0005row("ENDTIME") = CDate(T0005row("ENDTIME")).ToString("HH:mm")
                Else
                    T0005row("ENDTIME") = "00:00"
                End If
                T0005row("WORKTIME") = Format(Int(T0005row("WORKTIME") / 60) * 100 + T0005row("WORKTIME") Mod 60, "0#:##")
                T0005row("MOVETIME") = Format(Int(T0005row("MOVETIME") / 60) * 100 + T0005row("MOVETIME") Mod 60, "0#:##")
                T0005row("ACTTIME") = Format(Int(T0005row("ACTTIME") / 60) * 100 + T0005row("ACTTIME") Mod 60, "0#:##")
                T0005row("PRATE") = CInt(T0005row("PRATE")).ToString("#,0")

                T0005row("CASH") = CInt(T0005row("CASH")).ToString("#,0")
                T0005row("TICKET") = CInt(T0005row("TICKET")).ToString("#,0")
                T0005row("ETC") = CInt(T0005row("ETC")).ToString("#,0")
                T0005row("TOTALTOLL") = CInt(T0005row("TOTALTOLL")).ToString("#,0")
                T0005row("STMATER") = Val(T0005row("STMATER")).ToString("#,0.00")
                T0005row("ENDMATER") = Val(T0005row("ENDMATER")).ToString("#,0.00")
                T0005row("RUIDISTANCE") = Val(T0005row("RUIDISTANCE")).ToString("#,0.00")
                T0005row("SOUDISTANCE") = Val(T0005row("SOUDISTANCE")).ToString("#,0.00")
                T0005row("JIDISTANCE") = Val(T0005row("JIDISTANCE")).ToString("#,0.00")
                T0005row("KUDISTANCE") = Val(T0005row("KUDISTANCE")).ToString("#,0.00")
                T0005row("IPPDISTANCE") = Val(T0005row("IPPDISTANCE")).ToString("#,0.00")
                T0005row("KOSDISTANCE") = Val(T0005row("KOSDISTANCE")).ToString("#,0.00")
                T0005row("IPPJIDISTANCE") = Val(T0005row("IPPJIDISTANCE")).ToString("#,0.00")
                T0005row("IPPKUDISTANCE") = Val(T0005row("IPPKUDISTANCE")).ToString("#,0.00")
                T0005row("KOSJIDISTANCE") = Val(T0005row("KOSJIDISTANCE")).ToString("#,0.00")
                T0005row("KOSKUDISTANCE") = Val(T0005row("KOSKUDISTANCE")).ToString("#,0.00")
                T0005row("KYUYU") = Val(T0005row("KYUYU")).ToString("#,0.00")
                T0005row("SURYO1") = Val(T0005row("SURYO1")).ToString("#,0.000")
                T0005row("SURYO2") = Val(T0005row("SURYO2")).ToString("#,0.000")
                T0005row("SURYO3") = Val(T0005row("SURYO3")).ToString("#,0.000")
                T0005row("SURYO4") = Val(T0005row("SURYO4")).ToString("#,0.000")
                T0005row("SURYO5") = Val(T0005row("SURYO5")).ToString("#,0.000")
                T0005row("SURYO6") = Val(T0005row("SURYO6")).ToString("#,0.000")
                T0005row("SURYO7") = Val(T0005row("SURYO7")).ToString("#,0.000")
                T0005row("SURYO8") = Val(T0005row("SURYO8")).ToString("#,0.000")
                T0005row("TOTALSURYO") = Val(T0005row("TOTALSURYO")).ToString("#,0.000")

                '名前の取得
                Dim WW_PRODUCT As String = ""
                WW_PRODUCT = T0005row("OILTYPE1") & T0005row("PRODUCT11") & T0005row("PRODUCT21")
                T0005row("PRODUCT1NAMES") = ""
                CODENAME_get("PRODUCT2", WW_PRODUCT, T0005row("PRODUCT1NAMES"), WW_DUMMY)
                WW_PRODUCT = T0005row("OILTYPE2") & T0005row("PRODUCT12") & T0005row("PRODUCT22")
                T0005row("PRODUCT2NAMES") = ""
                CODENAME_get("PRODUCT2", WW_PRODUCT, T0005row("PRODUCT2NAMES"), WW_DUMMY)
                WW_PRODUCT = T0005row("OILTYPE3") & T0005row("PRODUCT13") & T0005row("PRODUCT23")
                T0005row("PRODUCT3NAMES") = ""
                CODENAME_get("PRODUCT2", WW_PRODUCT, T0005row("PRODUCT3NAMES"), WW_DUMMY)
                WW_PRODUCT = T0005row("OILTYPE4") & T0005row("PRODUCT14") & T0005row("PRODUCT24")
                T0005row("PRODUCT4NAMES") = ""
                CODENAME_get("PRODUCT2", WW_PRODUCT, T0005row("PRODUCT4NAMES"), WW_DUMMY)
                WW_PRODUCT = T0005row("OILTYPE5") & T0005row("PRODUCT15") & T0005row("PRODUCT25")
                T0005row("PRODUCT5NAMES") = ""
                CODENAME_get("PRODUCT2", WW_PRODUCT, T0005row("PRODUCT5NAMES"), WW_DUMMY)
                WW_PRODUCT = T0005row("OILTYPE6") & T0005row("PRODUCT16") & T0005row("PRODUCT26")
                T0005row("PRODUCT6NAMES") = ""
                CODENAME_get("PRODUCT2", WW_PRODUCT, T0005row("PRODUCT6NAMES"), WW_DUMMY)
                WW_PRODUCT = T0005row("OILTYPE7") & T0005row("PRODUCT17") & T0005row("PRODUCT27")
                T0005row("PRODUCT7NAMES") = ""
                CODENAME_get("PRODUCT2", WW_PRODUCT, T0005row("PRODUCT7NAMES"), WW_DUMMY)
                WW_PRODUCT = T0005row("OILTYPE8") & T0005row("PRODUCT18") & T0005row("PRODUCT28")
                T0005row("PRODUCT8NAMES") = ""
                CODENAME_get("PRODUCT2", WW_PRODUCT, T0005row("PRODUCT8NAMES"), WW_DUMMY)
            Next

            SQLdr.Dispose() 'Reader(Close)
            SQLdr = Nothing

            SQLcmd.Dispose()
            SQLcmd = Nothing

            SQLcon.Close() 'DataBase接続(Close)
            SQLcon.Dispose()
            SQLcon = Nothing

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "T0005_NIPPO SELECT")

            CS0011LOGWRITE.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:T0005_NIPPO Select"      '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                  '
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try

        'トリップ判定・回送判定・出荷日内荷積荷卸回数判定
        T0005COM.ReEditT0005(T0005tbl, work.WF_T7SEL_CAMPCODE.Text, WW_DUMMY)

        '--------------------------------------------
        'ヘッダレコード作成
        '--------------------------------------------
        '一週間前データを分離し、画面要求対象データを抽出
        Dim WW_Filter As String = "YMD < #" & WF_WORKDATE.Text & "#"
        CS0026TblSort.TABLE = T0005tbl
        CS0026TblSort.FILTER = WW_Filter
        CS0026TblSort.SORTING = "SELECT, YMD, CREWKBN, STAFFCODE, HDKBN DESC, STDATE, STTIME, ENDDATE, ENDTIME, WORKKBN"
        T0005WEEKtbl = CS0026TblSort.Sort()

        T0005_CreHead(T0005WEEKtbl)

        '--------------------------------------------
        'ヘッダレコード作成
        '--------------------------------------------
        WW_Filter = "YMD >= #" & WF_WORKDATE.Text & "#"
        CS0026TblSort.TABLE = T0005tbl
        CS0026TblSort.FILTER = WW_Filter
        CS0026TblSort.SORTING = "SELECT, YMD, CREWKBN, STAFFCODE, HDKBN DESC, STDATE, STTIME, ENDDATE, ENDTIME, WORKKBN"
        T0005tbl = CS0026TblSort.Sort()

        T0005_CreHead(T0005tbl)

        rightview.SetErrorReport("")

        '○GridViewデータをテーブルに保存
        If Not Master.SaveTable(T0005tbl, work.WF_T5I_XMLsaveF.Text) Then
            Exit Sub
        End If

        '○GridViewデータをテーブルに保存（一週間前データ）
        If Not Master.SaveTable(T0005WEEKtbl, work.WF_T5I_XMLsaveF9.Text) Then
            Exit Sub
        End If

        If T0005tbl.Rows.Count > 0 Then
            '出庫年月日開始
            work.WF_SEL_STYMD.Text = T0005tbl.Rows(0)("YMD")
            '出庫年月日終了
            work.WF_SEL_ENDYMD.Text = T0005tbl.Rows(0)("YMD")
            '運用部署
            work.WF_SEL_UORG.Text = work.WF_T7SEL_HORG.Text
            '画面ID（個別）
            work.WF_SEL_VIEWID_DTL.Text = "Default"
            '押下ボタン
            work.WF_T7SEL_BUTTON.Text = ""
            '選択番号
            work.WF_T5I_LINECNT.Text = work.WF_T7KIN_LINECNT.Text
            'ヘッダの日付
            work.WF_T5_YMD.Text = T0005tbl.Rows(0)("YMD")
            '従業員コード
            work.WF_T5_STAFFCODE.Text = T0005tbl.Rows(0)("STAFFCODE")
            '呼出元MAPID　
            work.WF_T5_FROMMAPID.Text = GRT00007WRKINC_V2.MAPIDNJS
            '画面一覧保存パス
            work.WF_SEL_XMLsaveF.Text = work.WF_T5I_XMLsaveF.Text
            work.WF_SEL_XMLsaveF9.Text = work.WF_T5I_XMLsaveF9.Text
            '権限
            work.WF_SEL_MAPpermitcode.Text = Master.MAPpermitcode

            '呼出元VARIANT
            work.WF_T5_FROMMAPVARIANT.Text = Master.MAPvariant & GRT00007WRKINC_V2.MAPVRNJS

            '画面遷移実行
            Master.TransitionPage(work.WF_T7SEL_CAMPCODE.Text)

        End If

    End Sub
    ' ***  ヘッダレコード作成
    Protected Sub T0005_CreHead(ByRef ioTbl As DataTable)

        Dim WW_LINECNT As Integer = 0
        Dim WW_IDX As Integer = 0
        Dim WW_CONVERT As String = ""
        Dim WW_Cols As String() = {"YMD", "STAFFCODE"}
        Dim WW_KEYtbl As DataTable
        Dim WW_T0005tbl As DataTable = ioTbl.Clone
        Dim WW_T0005DELtbl As DataTable = ioTbl.Clone
        Dim WW_T0005SELtbl As DataTable = ioTbl.Clone
        Dim WW_TBLview As DataView
        Dim WW_T0005row As DataRow

        Try
            '更新元（削除）データをキープ
            CS0026TblSort.TABLE = ioTbl
            CS0026TblSort.FILTER = "SELECT = '0'"
            CS0026TblSort.SORTING = "SELECT"
            WW_T0005DELtbl = CS0026TblSort.sort()

            '出庫日、乗務員でグループ化しキーテーブル作成
            CS0026TblSort.TABLE = ioTbl
            CS0026TblSort.FILTER = "HDKBN = 'D' and SELECT = '1'"
            CS0026TblSort.SORTING = "YMD, STAFFCODE, STDATE, STTIME, ENDDATE, ENDTIME, WORKKBN, SEQ"
            WW_T0005SELtbl = CS0026TblSort.sort()

            WW_TBLview = New DataView(WW_T0005SELtbl)

            '抽出後のテーブルに置き換える（ヘッダなし、明細のみ）
            ioTbl = WW_T0005SELtbl.Copy()
            'キーテーブル作成
            WW_KEYtbl = WW_TBLview.ToTable(True, WW_Cols)

            For Each WW_KEYrow As DataRow In WW_KEYtbl.Rows
                Dim WW_FIRST As String = "OFF"
                Dim WW_FIRST2 As String = "OFF"
                Dim WW_TOTALTOLL As Decimal = 0                             '通行料合計
                Dim WW_SOUDISTANCE As Decimal = 0                           '走行距離
                Dim WW_JIDISTANCE As Decimal = 0                            '実車距離
                Dim WW_KUDISTANCE As Decimal = 0                            '空車距離
                Dim WW_IPPDISTANCE As Decimal = 0                           '一般走行距離
                Dim WW_KOSDISTANCE As Decimal = 0                           '高速走行距離
                Dim WW_IPPJIDISTANCE As Decimal = 0                         '一般・実車距離
                Dim WW_IPPKUDISTANCE As Decimal = 0                         '一般・空車距離
                Dim WW_KOSJIDISTANCE As Decimal = 0                         '高速・実車距離
                Dim WW_KOSKUDISTANCE As Decimal = 0                         '高速・空車距離
                Dim WW_KYUYU As Decimal = 0                                 '給油
                Dim WW_STORICODE As String = ""                             '請求取引先コード
                Dim WW_CONTCHASSIS As String = ""                           'コンテナシャーシ
                Dim WW_OPE_UPD As String = "OFF"
                Dim WW_OPE_ERR As String = "OFF"
                Dim WW_DEL_FLG As String = "OFF"

                '初期化
                WW_T0005row = WW_T0005tbl.NewRow
                'INProw_Init(WW_T0005row)
                T0005COM.InitialT5INPRow(WW_T0005row)
                WW_T0005row("CAMPCODE") = work.WF_T7SEL_CAMPCODE.Text
                WW_T0005row("SHIPORG") = work.WF_T7SEL_HORG.Text

                For i As Integer = WW_IDX To WW_T0005SELtbl.Rows.Count - 1
                    Dim WW_SELrow As DataRow = WW_T0005SELtbl.Rows(i)
                    If WW_KEYrow("YMD") = WW_SELrow("YMD") And
                       WW_KEYrow("STAFFCODE") = WW_SELrow("STAFFCODE") Then
                        If WW_SELrow("DELFLG") = "0" Then
                            If WW_FIRST = "OFF" Then
                                WW_FIRST = "ON"
                                '先頭レコードより開始日、開始時間を取得
                                WW_T0005row("STDATE") = WW_SELrow("STDATE")
                                WW_T0005row("STTIME") = WW_SELrow("STTIME")
                                WW_T0005row("TERMKBN") = WW_SELrow("TERMKBN")
                                WW_T0005row("CREWKBN") = WW_SELrow("CREWKBN")
                                WW_T0005row("SUBSTAFFCODE") = WW_SELrow("SUBSTAFFCODE")
                                WW_T0005row("JISSKIKBN") = WW_SELrow("JISSKIKBN")
                            End If

                            '最終レコードの終了日、終了時間を取得
                            WW_T0005row("ENDDATE") = WW_SELrow("ENDDATE")
                            WW_T0005row("ENDTIME") = WW_SELrow("ENDTIME")

                            '帰庫レコードより合計値を取得
                            If WW_SELrow("WORKKBN") = "F3" Then
                                WW_TOTALTOLL = WW_TOTALTOLL + Val(WW_SELrow("TOTALTOLL").replace(",", ""))
                                WW_KYUYU = WW_KYUYU + Val(WW_SELrow("KYUYU").replace(",", ""))
                                WW_SOUDISTANCE = WW_SOUDISTANCE + Val(WW_SELrow("SOUDISTANCE").replace(",", ""))
                                WW_JIDISTANCE = WW_JIDISTANCE + Val(WW_SELrow("JIDISTANCE").replace(",", ""))
                                WW_KUDISTANCE = WW_KUDISTANCE + Val(WW_SELrow("KUDISTANCE").replace(",", ""))
                                WW_IPPDISTANCE = WW_IPPDISTANCE + Val(WW_SELrow("IPPDISTANCE").replace(",", ""))
                                WW_KOSDISTANCE = WW_KOSDISTANCE + Val(WW_SELrow("KOSDISTANCE").replace(",", ""))
                                WW_IPPJIDISTANCE = WW_IPPJIDISTANCE + Val(WW_SELrow("IPPJIDISTANCE").replace(",", ""))
                                WW_IPPKUDISTANCE = WW_IPPKUDISTANCE + Val(WW_SELrow("IPPKUDISTANCE").replace(",", ""))
                                WW_KOSJIDISTANCE = WW_KOSJIDISTANCE + Val(WW_SELrow("KOSJIDISTANCE").replace(",", ""))
                                WW_KOSKUDISTANCE = WW_KOSKUDISTANCE + Val(WW_SELrow("KOSKUDISTANCE").replace(",", ""))
                            End If

                            'タイムスタンプがゼロ以外が存在する場合、ヘッダにもとりあえずタイムスタンプ設定
                            'ヘッダで、ＤＢ登録済のデータか、初取込データ（新規を含む）かを判断できるようにする
                            If WW_SELrow("TIMSTP") <> "0" Then
                                WW_T0005row("TIMSTP") = WW_SELrow("TIMSTP")
                            End If
                        End If

                        If WW_SELrow("OPERATION") = "更新" Then
                            WW_OPE_UPD = "ON"
                        End If
                        If WW_SELrow("OPERATION") = "エラー" Then
                            WW_OPE_ERR = "ON"
                        End If
                        If WW_SELrow("DELFLG") = "0" Then
                            WW_DEL_FLG = "ON"
                        End If
                    Else
                        WW_IDX = i
                        Exit For
                    End If
                Next

                If WW_OPE_ERR = "ON" Then
                    WW_T0005row("OPERATION") = "エラー"
                ElseIf WW_OPE_UPD = "ON" Then
                    WW_T0005row("OPERATION") = "更新"
                Else
                    WW_T0005row("OPERATION") = ""
                End If
                WW_T0005row("YMD") = WW_KEYrow("YMD")
                WW_T0005row("STAFFCODE") = WW_KEYrow("STAFFCODE")
                WW_T0005row("SELECT") = "1"
                WW_T0005row("HIDDEN") = "0"
                WW_T0005row("HDKBN") = "H"
                WW_T0005row("SEQ") = "001"
                If WW_DEL_FLG = "ON" Then
                    WW_T0005row("DELFLG") = "0"
                Else
                    WW_T0005row("DELFLG") = "1"
                End If
                Dim WW_WORKTIME As Integer = 0

                '作業時間
                WW_WORKTIME = DateDiff("n",
                                      WW_T0005row("STDATE") + " " + WW_T0005row("STTIME"),
                                      WW_T0005row("ENDDATE") + " " + WW_T0005row("ENDTIME")
                                     )
                WW_T0005row("WORKTIME") = Format(Int(WW_WORKTIME / 60) * 100 + WW_WORKTIME Mod 60, "0#:##")
                WW_T0005row("ACTTIME") = Format(Int(WW_WORKTIME / 60) * 100 + WW_WORKTIME Mod 60, "0#:##")
                WW_T0005row("SOUDISTANCE") = Val(WW_SOUDISTANCE).ToString("#,0.00")
                WW_T0005row("KYUYU") = Val(WW_KYUYU).ToString("#,0.00")
                WW_T0005row("TOTALTOLL") = Val(WW_TOTALTOLL).ToString("#,0")

                WW_T0005row("SOUDISTANCE") = Val(WW_SOUDISTANCE).ToString("#,0.00")
                WW_T0005row("JIDISTANCE") = Val(WW_JIDISTANCE).ToString("#,0.00")
                WW_T0005row("KUDISTANCE") = Val(WW_KUDISTANCE).ToString("#,0.00")
                WW_T0005row("IPPDISTANCE") = Val(WW_IPPDISTANCE).ToString("#,0.00")
                WW_T0005row("KOSDISTANCE") = Val(WW_KOSDISTANCE).ToString("#,0.00")
                WW_T0005row("IPPJIDISTANCE") = Val(WW_IPPJIDISTANCE).ToString("#,0.00")
                WW_T0005row("IPPKUDISTANCE") = Val(WW_IPPKUDISTANCE).ToString("#,0.00")
                WW_T0005row("KOSJIDISTANCE") = Val(WW_KOSJIDISTANCE).ToString("#,0.00")
                WW_T0005row("KOSKUDISTANCE") = Val(WW_KOSKUDISTANCE).ToString("#,0.00")

                WW_T0005row("CAMPNAMES") = ""
                CODENAME_get("CAMPCODE", WW_T0005row("CAMPCODE"), WW_T0005row("CAMPNAMES"), WW_DUMMY)
                WW_T0005row("SHIPORGNAMES") = ""
                CODENAME_get("HORG", WW_T0005row("SHIPORG"), WW_T0005row("SHIPORGNAMES"), WW_DUMMY)
                WW_T0005row("TERMKBNNAMES") = ""
                CODENAME_get("TERMKBN", WW_T0005row("TERMKBN"), WW_T0005row("TERMKBNNAMES"), WW_DUMMY)
                WW_T0005row("STAFFNAMES") = ""
                CODENAME_get("STAFFCODE", WW_T0005row("STAFFCODE"), WW_T0005row("STAFFNAMES"), WW_DUMMY)
                WW_T0005row("SUBSTAFFNAMES") = ""
                CODENAME_get("STAFFCODE", WW_T0005row("SUBSTAFFCODE"), WW_T0005row("SUBSTAFFNAMES"), WW_DUMMY)
                WW_T0005row("CREWKBNNAMES") = ""
                CODENAME_get("CREWKBN", WW_T0005row("CREWKBN"), WW_T0005row("CREWKBNNAMES"), WW_DUMMY)
                WW_T0005row("JISSKIKBNNAMES") = ""

                WW_LINECNT = WW_LINECNT + 1
                WW_T0005row("LINECNT") = WW_LINECNT
                WW_T0005tbl.Rows.Add(WW_T0005row)
            Next

            'ヘッダのマージ
            ioTbl.Merge(WW_T0005tbl)

            '更新元（削除）データの戻し
            ioTbl.Merge(WW_T0005DELtbl)

            'ソート
            CS0026TblSort.TABLE = ioTbl
            CS0026TblSort.FILTER = ""
            CS0026TblSort.SORTING = "SELECT, YMD, CREWKBN, STAFFCODE, HDKBN DESC, STDATE, STTIME, ENDDATE, ENDTIME, WORKKBN"
            ioTbl = CS0026TblSort.sort()

            WW_KEYtbl.Dispose()
            WW_KEYtbl = Nothing
            WW_TBLview.Dispose()
            WW_TBLview = Nothing
            WW_T0005DELtbl.Dispose()
            WW_T0005DELtbl = Nothing
            WW_T0005SELtbl.Dispose()
            WW_T0005SELtbl = Nothing
            WW_T0005tbl.Dispose()
            WW_T0005tbl = Nothing

        Catch ex As Exception
            CS0011LOGWRITE.INFSUBCLASS = "T0005_CreHead"                'SUBクラス名
            CS0011LOGWRITE.INFPOSI = ""                                 '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                  '
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.SYSTEM_ADM_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            Exit Sub

        End Try

    End Sub

    ''' <summary>
    ''' RightBoxラジオボタン選択処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_RadioButton_Click()

        If WF_RightViewChange.Value = Nothing Or WF_RightViewChange.Value = "" Then
        Else
            Try
                Integer.TryParse(WF_RightViewChange.Value, WF_RightViewChange.Value)
            Catch ex As Exception
                Exit Sub
            End Try

            rightview.selectIndex(WF_RightViewChange.Value)
            WF_RightViewChange.Value = ""
        End If

    End Sub

    ''' <summary>
    ''' RightBoxメモ欄更新
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_RIGHTBOX_Change()

        rightview.save(Master.USERID, Master.USERTERMID, WW_DUMMY)

    End Sub

    ' ***  条件抽出画面情報退避
    Protected Sub MAPrefelence(ByRef O_MSG As String, ByRef O_RTN As String)

        O_MSG = ""
        O_RTN = C_MESSAGE_NO.NORMAL


        '■■■ 選択画面の入力初期値設定 ■■■
        If Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.T00007INJS_V2 Then       '条件画面からの画面遷移

        End If

        '■■■ 選択画面の入力初期値設定 ■■■
        If Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.T00005 Then

        End If

        '■■■ 画面モード（更新・参照）設定  ■■■
        '事務員勤怠登録（条件）画面から遷移した場合
        If Master.MAPpermitcode = C_PERMISSION.UPDATE Then
            If work.WF_T7SEL_LIMITFLG.Text = "0" Then
                '対象月の締前は更新ＯＫ
                WF_MAPpermitcode.Value = "TRUE"
            Else
                '対象月の締後は更新できない
                WF_MAPpermitcode.Value = "FALSE"
            End If
        Else
            WF_MAPpermitcode.Value = "FALSE"
        End If

        '月合計の場合、ボタンを非表示
        If work.WF_T7KIN_RECODEKBN.Text = "2" Then
            WF_NIPPObtn.Value = "FALSE"
        Else
            If WF_MAPpermitcode.Value = "TRUE" Then
                WF_NIPPObtn.Value = "TRUE"
            Else
                WF_NIPPObtn.Value = "FALSE"
            End If
        End If

        '○Grid情報保存先のファイル名
        Master.createXMLSaveFile()
        work.WF_T7KIN_XMLsaveF.Text = Master.XMLsaveF

        work.WF_T5I_XMLsaveF.Text = CS0050SESSION.UPLOAD_PATH & "\XML_TMP\" & Date.Now.ToString("yyyyMMdd") & "-" &
            CS0050SESSION.USERID & "-T00007NJS-T5-" & Master.MAPvariant & "-" & Date.Now.ToString("HHmmss") & ".txt"
        work.WF_T5I_XMLsaveF9.Text = CS0050SESSION.UPLOAD_PATH & "\XML_TMP\" & Date.Now.ToString("yyyyMMdd") & "-" &
            CS0050SESSION.USERID & "-T00007NJS-T59-" & Master.MAPvariant & "-" & Date.Now.ToString("HHmmss") & ".txt"

        work.WF_T7KIN_XMLsaveF2.Text = CS0050SESSION.UPLOAD_PATH & "\XML_TMP\" & Date.Now.ToString("yyyyMMdd") & "-" &
            CS0050SESSION.USERID & "-T00007NJS-MODEL-" & Master.MAPvariant & "-" & Date.Now.ToString("HHmmss") & ".txt"

    End Sub

End Class
