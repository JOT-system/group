Imports System.Data.SqlClient
Imports System.IO
Imports OFFICE.GRIS0005LeftBox
Imports OFFICE.GRT00016COM

''' <summary>
''' 荷主請求メンテナンス（実行）
''' </summary>
''' <remarks></remarks>
Public Class GRT00016NSEIKYU
    Inherits System.Web.UI.Page

    Private T00016ds As DataSet                                     '格納ＤＳ
    Private T00016tbl As DataTable                                  'Grid格納用テーブル
    Private T00016tbl_tab1 As DataTable                             'Grid格納用テーブルTab1
    Private T00016tbl_tab2 As DataTable                             'Grid格納用テーブルTab2
    Private T00016tbl_tab3 As DataTable                             'Grid格納用テーブルTab3
    Private T00016tbl_tab4 As DataTable                             'Grid格納用テーブルTab4
    Private T00016INPtbl As DataTable                               'Detail入力用テーブル
    Private T00016UPDtbl As DataTable                               '更新時作業テーブル
    Private T00016SUMtbl As DataTable                               '更新時作業テーブル
    Private T00016WKtbl As DataTable                                '更新時作業テーブル
    Private T0005tbl As DataTable                                   '日報取込用テーブル

    Private S0013tbl As DataTable                                   'データフィールド

    Private JOTMASTER As JOT_MASTER                                 'JOTマスターデータ管理

    '共通関数宣言(BASEDLL)
    Private CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get
    Private CS0010CHARstr As New CS0010CHARget                      '例外文字排除 String Ge
    Private CS0020JOURNAL As New CS0020JOURNAL                      'Journal Out
    Private CS0023XLSUPLOAD As New CS0023XLSUPLOAD                  'XLSアップロード
    Private CS0030REPORT As New CS0030REPORT                        '帳票出力
    Private CS0013ProfView As New CS0013ProfView                    'Tableオブジェクト展開
    Private CS0050SESSION As New CS0050SESSION                      'セッション情報操作
    Private CS0052DetailView As New CS0052DetailView                'Repeterオブジェクト作成
    Private CS0033AutoNumber As New CS0033AutoNumber                '受注番号取得
    Private CS0026TBLSORTget As New CS0026TBLSORT                   'GridView用テーブルソート文字列取得
    Private GS0029T3CNTLget As New GS0029T3CNTLget                  '荷主受注集計制御マスタ取得

    '共通処理結果
    Private WW_ERRCODE As String = String.Empty                     'リターンコード
    Private WW_RTN_SW As String                                     '
    Private WW_DUMMY As String                                      '

    Private WW_ERRLISTCNT As Integer                                'エラーリスト件数               
    Private WW_ERRLIST_ALL As List(Of String)                       'インポート全体のエラー
    Private WW_ERRLIST As List(Of String)                           'インポート中の１セット分のエラー

    Private Const CONST_DSPROW_MAX As Integer = 65000
    Private Const CONST_SCROLLROWCOUNT As Integer = 18              'マウススクロール時の増分
    Private Const CONST_DISPROWCOUNT As Integer = 18                '1画面表示用


    ''' <summary>
    ''' サーバ処理の遷移先
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

        Try
            '■■■ 作業用データベース設定 ■■■
            T00016ds = New DataSet()                                      '初期化
            T00016tbl = T00016ds.Tables.Add("T00016TBL")
            T00016tbl_tab1 = T00016ds.Tables.Add("T00016TBL_tab1")
            T00016tbl_tab2 = T00016ds.Tables.Add("T00016TBL_tab2")
            T00016tbl_tab3 = T00016ds.Tables.Add("T00016TBL_tab3")
            T00016tbl_tab4 = T00016ds.Tables.Add("T00016tbl_tab4")
            T00016INPtbl = T00016ds.Tables.Add("T00016INPtbl")
            T00016UPDtbl = T00016ds.Tables.Add("T00016UPDtbl")
            T00016SUMtbl = T00016ds.Tables.Add("T00016SUMtbl")
            T00016WKtbl = T00016ds.Tables.Add("T00016WKtbl")
            T00016ds.EnforceConstraints = False
            JOTMASTER = New JOT_MASTER With {
                .CAMPCODE = work.WF_SEL_CAMPCODE.Text,
                .ORGCODE = work.WF_SEL_SHIPORG.Text
            }

            If IsPostBack Then
                '○ チェックボックス保持
                FileSaveDisplayInput()

                '■■■ 各ボタン押下処理 ■■■
                If Not String.IsNullOrEmpty(WF_ButtonClick.Value) Then
                    Select Case WF_ButtonClick.Value

                        '********* ヘッダ部 *********
                        Case "WF_ButtonAdd"                     '行追加
                            WF_ButtonAdd_Click()
                        Case "WF_ButtonExtract"                 '絞込み
                            WF_ButtonExtract_Click()
                        Case "WF_ButtonGet"                     '日報取込
                            WF_ButtonGet_Click()
                        Case "WF_ButtonSupplJisski"             '用車実績
                            WF_ButtonSupplJisski()
                        Case "WF_ButtonUPDATE"                  'DB更新
                            WF_ButtonUPDATE_Click()
                        Case "WF_ButtonCSV"                     'ﾀﾞｳﾝﾛｰﾄﾞ
                            WF_ButtonCSV_Click()
                        Case "WF_ButtonFIRST"                   '先頭頁[image]
                            WF_ButtonFIRST_Click()
                        Case "WF_ButtonLAST"                    '最終頁[image]
                            WF_ButtonLAST_Click()
                        Case "WF_ButtonEND"                     '終了
                            WF_ButtonEND_Click()
                        Case "WF_DTAB_Click"                    'DetailTab切替処理
                            WF_Detail_TABChange()

                            '********* 一覧 *********
                        Case "WF_GridDBclick"                   'DBClick
                            WF_Grid_DBclick()
                        Case "WF_MouseWheelDown"                'MouseDown
                            WF_GRID_Scrole()
                        Case "WF_MouseWheelUp"                  'MouseUp
                            WF_GRID_Scrole()
                        Case "WF_UPLOAD_EXCEL"                  'EXCEL_UPLOAD
                            UPLOAD_EXCEL()

                            '********* 入力フィールド *********
                        Case "WF_Field_DBClick"                 '項目DbClick
                            WF_Field_DBClick()

                            '********* 左BOX *********
                        Case "WF_ButtonSel"                     '選択
                            WF_ButtonSel_Click()
                        Case "WF_ButtonCan"                     'キャンセル
                            WF_ButtonCan_Click()
                        Case "WF_ListboxDBclick"                '値選択DbClick
                            WF_Listbox_DBClick()

                            '********* 右BOX *********
                        Case "WF_RadioButonClick"               '選択時
                            WF_RadioButon_Click()
                        Case "WF_MEMOChange"                    'メモ欄変更時
                            WF_MEMO_Change()

                            '********* その他はMasterPageで処理 *********
                        Case Else
                    End Select
                    '○一覧再表示処理
                    DisplayGrid()

                End If
            Else
                '〇初期化処理
                Initialize()
            End If

        Catch ex As Threading.ThreadAbortException
            'キャンセルやServerTransferにて後続の処理が打ち切られた場合のエラーは発生させない
            '○一覧再表示処理
            'DisplayGrid()
        Catch ex As Exception
            '○一覧再表示処理
            DisplayGrid()
            Master.Output(C_MESSAGE_NO.SYSTEM_ADM_ERROR, C_MESSAGE_TYPE.ERR)
        Finally

            '○Close
            If Not IsNothing(T00016ds) Then
                For Each tbl In T00016ds.Tables
                    tbl.Dispose()
                    tbl = Nothing
                Next
                T00016ds.Dispose()
                T00016ds = Nothing
            End If

            If Not IsNothing(S0013tbl) Then
                S0013tbl.Dispose()
                S0013tbl = Nothing
            End If

            If Not IsNothing(JOTMASTER) Then
                JOTMASTER.Dispose()
                JOTMASTER = Nothing
            End If

        End Try

    End Sub

    ''' <summary>
    ''' 初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub Initialize()
        '○初期値設定
        Master.MAPID = GRT00016WRKINC.MAPID
        WF_FIELD.Value = ""
        WF_FIELD_REP.Value = ""
        WF_SELTORIDATE.Focus()

        '〇ヘルプ無
        Master.dispHelp = False
        '〇ドラックアンドドロップON
        Master.eventDrop = True

        '左Boxへの値設定
        WF_LeftMViewChange.Value = ""
        leftview.activeListBox()

        '右Boxへの値設定
        rightview.resetindex()
        rightview.MAPID = Master.MAPID
        rightview.MAPVARI = Master.MAPvariant
        rightview.COMPCODE = work.WF_SEL_CAMPCODE.Text
        rightview.PROFID = Master.PROF_REPORT
        rightview.Initialize(WW_DUMMY)
        rightview.selectIndex(GRIS0004RightBox.RIGHT_TAB_INDEX.LS_ERROR_LIST)

        '〇画面モード（更新・参照）設定 
        If Master.MAPpermitcode = C_PERMISSION.UPDATE Then
            WF_MAPpermitcode.Value = "TRUE"
        Else
            WF_MAPpermitcode.Value = "FALSE"
        End If


        '部署未指定時
        If String.IsNullOrEmpty(work.WF_SEL_SHIPORG.Text) Then
            'ログインユーザ所属部署の管轄支店部署を設定
            WF_DEFORG.Text = Master.USER_ORG
        ElseIf Not String.IsNullOrEmpty(work.WF_SEL_SHIPORG.Text) Then
            WF_DEFORG.Text = work.WF_SEL_SHIPORG.Text
        Else
            WF_DEFORG.Text = work.WF_SEL_SHIPORG.Text
        End If

        '選択画面情報設定
        '対象年月
        If Not String.IsNullOrEmpty(work.WF_SEL_SEIKYUYMF.Text) Then

            Dim WW_DATE As Date
            If Date.TryParse(work.WF_SEL_SEIKYUYMF.Text, WW_DATE) Then
                WF_TAISHOYM_TEXT_LABEL.Text = WW_DATE.ToString("yyyy年MM月")
            End If

        ElseIf Not String.IsNullOrEmpty(work.WF_SEL_KEIJYODATEF.Text) Then

            Dim WW_DATE As Date
            If Date.TryParse(work.WF_SEL_KEIJYODATEF.Text, WW_DATE) Then
                WF_TAISHOYM_TEXT_LABEL.Text = WW_DATE.ToString("yyyy年MM月")
            End If

        End If
        '出荷部署
        If Not String.IsNullOrEmpty(work.WF_SEL_SHIPORG.Text) Then
            WF_SHUKAORG_TEXT_LABEL.Text = work.WF_SEL_SHIPORG.Text & " " & work.WF_SEL_SHIPORG_NAME.Text
        End If
        '油種
        If Not String.IsNullOrEmpty(work.WF_SEL_OILTYPE.Text) Then
            WF_OILTYPE_TEXT_LABEL.Text = work.WF_SEL_OILTYPE.Text & " " & work.WF_SEL_OILTYPE_NAME.Text
        End If

        '○業務車番設定
        InitGSHABAN()

        ''■■■ 画面（GridView）表示項目取得 ■■■
        'If work.WF_SEL_RESTART.Text = "RESTART" Then
        '    '○画面表示データ復元
        '    Master.RecoverTable(T00016tbl, work.WF_SEL_XMLsaveTmp.Text)

        'Else
        '○画面表示データ取得
        GRID_INITset()

        ''○数量、台数合計の設定
        'SUMMRY_SET()
        'End If

        '○Grid情報保存先のファイル名
        Master.createXMLSaveFile()

        '○画面表示データ保存
        Master.SaveTable(T00016tbl)

        Dim saveFile As String = Split(Master.XMLsaveF, ".")(0)

        'T00016項目作成タブ３
        work.WF_SEL_INPTAB3TBL.Text = saveFile & "-tab3.txt"
        T00016tbl_tab3_ColumnsAdd()
        Master.SaveTable(T00016tbl_tab3, work.WF_SEL_INPTAB3TBL.Text)
        'T00016項目作成タブ４
        work.WF_SEL_INPTAB4TBL.Text = saveFile & "-tab4.txt"
        T00016tbl_tab4_ColumnsAdd()
        Master.SaveTable(T00016tbl_tab4, work.WF_SEL_INPTAB4TBL.Text)

        '○一覧再表示処理
        DisplayGrid()

        '詳細非表示 
        WF_IsHideDetailBox.Value = "1"

        'テンポラリファイルの削除
        If File.Exists(work.WF_SEL_XMLsaveTmp.Text) Then
            File.Delete(work.WF_SEL_XMLsaveTmp.Text)
        End If
        If File.Exists(work.WF_SEL_XMLsavePARM.Text) Then
            File.Delete(work.WF_SEL_XMLsavePARM.Text)
        End If

        '〇 タブ切替
        WF_Detail_TABChange()

    End Sub

    ''' <summary>
    ''' GridView用データ取得
    ''' </summary>
    ''' <remarks>データベース（T00016）を検索し画面表示する一覧を作成する</remarks>
    Private Sub GRID_INITset()

        '○画面表示データ取得
        DBselect_T16SELECT()

        '○ソート
        'ソート文字列取得
        CS0026TBLSORTget.COMPCODE = work.WF_SEL_CAMPCODE.Text
        CS0026TBLSORTget.MAPID = Master.MAPID
        CS0026TBLSORTget.PROFID = Master.PROF_VIEW
        CS0026TBLSORTget.VARI = Master.MAPvariant
        CS0026TBLSORTget.TAB = ""
        CS0026TBLSORTget.getSorting()

        'ソート＆データ抽出
        CS0026TBLSORTget.TABLE = T00016tbl
        CS0026TBLSORTget.SORTING = CS0026TBLSORTget.SORTING
        CS0026TBLSORTget.FILTER = "SELECT = 1"
        CS0026TBLSORTget.Sort(T00016tbl)

        '○LineCNT付番・枝番再付番
        Dim WW_LINECNT As Integer = 0
        Dim WW_SEQ As Integer = 0

        For i As Integer = 0 To T00016tbl.Rows.Count - 1

            Dim T00016row = T00016tbl.Rows(i)

            If T00016row("LINECNT") = 0 Then

                WW_LINECNT = WW_LINECNT + 1
                WW_SEQ = 0

                For j As Integer = i To T00016tbl.Rows.Count - 1

                    If T00016tbl.Rows(j)("LINECNT") = 0 Then
                        If CompareOrder(T00016row, T00016tbl.Rows(j)) Then

                            WW_SEQ = WW_SEQ + 1
                            T00016tbl.Rows(j)("LINECNT") = WW_LINECNT

                            T00016tbl.Rows(j)("HIDDEN") = 0
                        End If

                    End If
                Next

            End If

        Next

    End Sub

    ''' <summary>
    ''' 一覧再表示処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub DisplayGrid()

        Dim WW_GridPosition As Integer                 '表示位置（開始）
        Dim WW_DataCNT As Integer = 0                  '(絞り込み後)有効Data数

        ''表示対象行カウント(絞り込み対象)
        'If T00016tbl.Columns.Count = 0 Then
        '    '○画面表示データ復元
        '    If Master.RecoverTable(T00016tbl) <> True Then Exit Sub
        'End If

        Select Case WF_DetailMView.ActiveViewIndex
            Case 0
                'タブ「合計(社内)」
                '　※　絞込（Cells("Hidden")： 0=表示対象 , 1=非表示対象)
                For Each T00016row In T00016tbl_tab1.Rows
                    If T00016row("HIDDEN") = "0" Then
                        WW_DataCNT = WW_DataCNT + 1
                    End If
                Next

            Case 1
                'タブ「合計(請求)」
                '　※　絞込（Cells("Hidden")： 0=表示対象 , 1=非表示対象)
                For Each T00016row In T00016tbl_tab2.Rows
                    If T00016row("HIDDEN") = "0" Then
                        WW_DataCNT = WW_DataCNT + 1
                    End If
                Next

            Case 2
                'タブ「明細(金額)」
                '　※　絞込（Cells("Hidden")： 0=表示対象 , 1=非表示対象)
                For Each T00016row In T00016tbl_tab3.Rows
                    If T00016row("HIDDEN") = "0" Then
                        WW_DataCNT = WW_DataCNT + 1
                    End If
                Next

            Case 3
                'タブ「明細(数量)」
                '　※　絞込（Cells("Hidden")： 0=表示対象 , 1=非表示対象)
                For Each T00016row In T00016tbl_tab4.Rows
                    If T00016row("HIDDEN") = "0" Then
                        WW_DataCNT = WW_DataCNT + 1
                    End If
                Next

        End Select

        '○表示Linecnt取得
        If WF_GridPosition.Text = "" Then
            WW_GridPosition = 1
        Else
            If Not Integer.TryParse(WF_GridPosition.Text, WW_GridPosition) Then
                WW_GridPosition = 1
            End If
        End If

        '○表示格納位置決定

        '表示開始_格納位置決定(次頁スクロール)
        If WF_ButtonClick.Value = "WF_MouseWheelUp" Then
            If (WW_GridPosition + CONST_SCROLLROWCOUNT) <= WW_DataCNT Then
                WW_GridPosition += CONST_SCROLLROWCOUNT
            End If
        End If

        '表示開始_位置決定(前頁スクロール)
        If WF_ButtonClick.Value = "WF_MouseWheelDown" Then
            If (WW_GridPosition - CONST_SCROLLROWCOUNT) > 0 Then
                WW_GridPosition -= CONST_SCROLLROWCOUNT
            Else
                WW_GridPosition = 1
            End If
        End If

        'タブ変更
        If WF_ButtonClick.Value = "WF_DTAB_Click" Then
            WW_GridPosition = 1
        End If

        '日報取込
        If WF_ButtonClick.Value = "WF_ButtonGet" Then

            WW_GridPosition = 1
            WF_DTAB_CHANGE_NO.Value = "3"
            GridViewInitializeTab4(True, WW_GridPosition)

            'タブ切替
            WF_Detail_TABChange()

            Exit Sub
        End If

        '〇 選択されたタブの一覧を再表示

        Select Case WF_DetailMView.ActiveViewIndex
            Case 0 'タブ「合計(社内)」
                ''○ 画面表示データ復元
                If Master.RecoverTable(T00016tbl_tab1, work.WF_SEL_INPTAB1TBL.Text) <> True Then Exit Sub
                GridViewInitializeTab1(WW_GridPosition)

            Case 1 'タブ「合計(請求)」
                '○ 画面表示データ復元
                If Master.RecoverTable(T00016tbl_tab2, work.WF_SEL_INPTAB2TBL.Text) <> True Then Exit Sub
                GridViewInitializeTab2(WW_GridPosition)

            Case 2 'タブ「明細(金額)」
                '○ 画面表示データ復元
                If Master.RecoverTable(T00016tbl_tab3, work.WF_SEL_INPTAB3TBL.Text) <> True Then Exit Sub
                If Master.RecoverTable(T00016tbl_tab4, work.WF_SEL_INPTAB4TBL.Text) <> True Then Exit Sub
                GridViewInitializeTab3(WW_GridPosition)

            Case 3 'タブ「明細(数量)」
                '○ 画面表示データ復元
                If Master.RecoverTable(T00016tbl_tab4, work.WF_SEL_INPTAB4TBL.Text) <> True Then Exit Sub
                GridViewInitializeTab4(False, WW_GridPosition)

            Case -1 '初回

                WF_DTAB_CHANGE_NO.Value = "3"
                GridViewInitializeTab4(True, WW_GridPosition)

        End Select

    End Sub


    ''' <summary>
    ''' GridViewデータ設定(タブ「合計(社内)」表示用)
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub GridViewInitializeTab1(WW_GridPosition As Integer)

        '○画面表示データ取得
        DBselect_T16SELECT_TAB1()

        '○LineCNT付番・枝番再付番
        Dim WW_LINECNT As Integer = 0
        Dim WW_SEQ As Integer = 0

        For i As Integer = 0 To T00016tbl_tab1.Rows.Count - 1

            Dim T00016row = T00016tbl_tab1.Rows(i)

            If IsDBNull(T00016row("CAMPCODE")) Then
                Continue For
            End If

            If T00016row("LINECNT") = 0 Then

                WW_LINECNT = WW_LINECNT + 1

                T00016tbl_tab1.Rows(i)("LINECNT") = WW_LINECNT

                T00016tbl_tab1.Rows(i)("HIDDEN") = 0

            End If

        Next

        '○ 画面表示データ保存
        Master.SaveTable(T00016tbl_tab1, work.WF_SEL_INPTAB1TBL.Text)

        '○ 一覧表示データ編集(性能対策)
        Dim TBLview As DataView = New DataView(T00016tbl_tab1)

        TBLview.RowFilter = "HIDDEN = 0 and LINECNT >= " & WW_GridPosition.ToString & " and LINECNT < " & (WW_GridPosition + CONST_DISPROWCOUNT).ToString

        CS0013ProfView.CAMPCODE = work.WF_SEL_CAMPCODE.Text
        CS0013ProfView.PROFID = Master.PROF_VIEW
        CS0013ProfView.MAPID = Master.MAPID
        CS0013ProfView.VARI = Master.VIEWID
        CS0013ProfView.SRCDATA = TBLview.ToTable
        CS0013ProfView.TBLOBJ = pnlListArea1
        CS0013ProfView.SCROLLTYPE = CS0013ProfView.SCROLLTYPE_ENUM.None
        CS0013ProfView.LEVENT = "ondblclick"
        CS0013ProfView.LFUNC = "ListDbClick"

        CS0013ProfView.TITLEOPT = True
        CS0013ProfView.CS0013ProfView()
        If Not isNormal(CS0013ProfView.ERR) Then
            Master.Output(CS0013ProfView.ERR, C_MESSAGE_TYPE.ABORT, "一覧設定エラー")
            Exit Sub
        End If

        WF_DetailMView.ActiveViewIndex = 0

        '○クリア
        If TBLview.Count = 0 Then
            WF_GridPosition.Text = "1"
        Else
            WF_GridPosition.Text = TBLview.Item(0)("LINECNT")
        End If

        TBLview.Dispose()
        TBLview = Nothing

    End Sub

    ''' <summary>
    ''' GridViewデータ設定(タブ「合計(請求)」表示用)
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub GridViewInitializeTab2(WW_GridPosition As Integer)

        '○画面表示データ取得
        DBselect_T16SELECT_TAB2()

        '○LineCNT付番・枝番再付番
        Dim WW_LINECNT As Integer = 0
        Dim WW_SEQ As Integer = 0

        For i As Integer = 0 To T00016tbl_tab2.Rows.Count - 1

            Dim T00016row = T00016tbl_tab2.Rows(i)

            If IsDBNull(T00016row("CAMPCODE")) Then
                Continue For
            End If

            If T00016row("LINECNT") = 0 Then

                WW_LINECNT = WW_LINECNT + 1

                T00016tbl_tab2.Rows(i)("LINECNT") = WW_LINECNT

                T00016tbl_tab2.Rows(i)("HIDDEN") = 0

            End If

        Next

        ''○ 画面表示データ保存
        Master.SaveTable(T00016tbl_tab2, work.WF_SEL_INPTAB2TBL.Text)

        '○ 一覧表示データ編集(性能対策)
        Dim TBLview As DataView = New DataView(T00016tbl_tab2)

        TBLview.RowFilter = "HIDDEN = 0 and LINECNT >= " & WW_GridPosition.ToString & " and LINECNT < " & (WW_GridPosition + CONST_DISPROWCOUNT).ToString

        CS0013ProfView.CAMPCODE = work.WF_SEL_CAMPCODE.Text
        CS0013ProfView.PROFID = Master.PROF_VIEW
        CS0013ProfView.MAPID = Master.MAPID + "TAB2"
        CS0013ProfView.VARI = Master.VIEWID
        CS0013ProfView.SRCDATA = TBLview.ToTable
        CS0013ProfView.TBLOBJ = pnlListArea2
        CS0013ProfView.SCROLLTYPE = CS0013ProfView.SCROLLTYPE_ENUM.None
        CS0013ProfView.LEVENT = "ondblclick"
        CS0013ProfView.LFUNC = "ListDbClick"

        CS0013ProfView.TITLEOPT = True
        CS0013ProfView.CS0013ProfView()
        If Not isNormal(CS0013ProfView.ERR) Then
            Master.Output(CS0013ProfView.ERR, C_MESSAGE_TYPE.ABORT, "一覧設定エラー")
            Exit Sub
        End If

        WF_DetailMView.ActiveViewIndex = 1

        '○クリア
        If TBLview.Count = 0 Then
            WF_GridPosition.Text = "1"
        Else
            WF_GridPosition.Text = TBLview.Item(0)("LINECNT")
        End If

        TBLview.Dispose()
        TBLview = Nothing

    End Sub

    ''' <summary>
    ''' GridViewデータ設定(タブ「明細(金額)」表示用)
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub GridViewInitializeTab3(WW_GridPosition As Integer)

        'Sort(T00016tbl)
        CS0026TBLSORTget.TABLE = T00016tbl_tab4
        CS0026TBLSORTget.SORTING = "TORICODE ,TORIHIKIORG ,SHUKABASHO ,TODOKECODE ,NSHABAN ,TORIHIKIYMD"
        CS0026TBLSORTget.FILTER = ""
        CS0026TBLSORTget.Sort(T00016tbl_tab4)

        '○画面表示データ取得
        DBselect_T16SELECT_TAB3()

        '○LineCNT付番・枝番再付番
        Dim WW_LINECNT As Integer = 0
        Dim WW_SEQ As Integer = 0

        'Sort(T00016tbl)
        CS0026TBLSORTget.TABLE = T00016tbl_tab3
        CS0026TBLSORTget.SORTING = "TORIHIKIYMD ,TORICODE ,TORIHIKIORG ,SHUKABASHO ,TODOKECODE ,NSHABAN"
        CS0026TBLSORTget.FILTER = ""
        CS0026TBLSORTget.Sort(T00016tbl_tab3)

        For i As Integer = 0 To T00016tbl_tab3.Rows.Count - 1

            Dim T00016row = T00016tbl_tab3.Rows(i)

            If IsDBNull(T00016row("CAMPCODE")) Then
                Continue For
            End If

            If T00016row("LINECNT") = 0 Then

                WW_LINECNT = WW_LINECNT + 1

                T00016tbl_tab3.Rows(i)("LINECNT") = WW_LINECNT

                T00016tbl_tab3.Rows(i)("HIDDEN") = 0

            End If

        Next

        '○ 画面表示データ保存
        Master.SaveTable(T00016tbl_tab3, work.WF_SEL_INPTAB3TBL.Text)

        'ポジション設定
        Dim WW_DataCNT As Integer = 0
        For Each T00016row In T00016tbl_tab3.Rows
            If T00016row("HIDDEN") = "0" Then
                WW_DataCNT = WW_DataCNT + 1
            End If
        Next

        If WW_GridPosition > WW_DataCNT Then
            WW_GridPosition -= CONST_SCROLLROWCOUNT
        End If

        '○ 一覧表示データ編集(性能対策)
        Dim TBLview As DataView = New DataView(T00016tbl_tab3)

        TBLview.RowFilter = "HIDDEN = 0 and LINECNT >= " & WW_GridPosition.ToString & " and LINECNT < " & (WW_GridPosition + CONST_DISPROWCOUNT).ToString

        CS0013ProfView.CAMPCODE = work.WF_SEL_CAMPCODE.Text
        CS0013ProfView.PROFID = Master.PROF_VIEW
        CS0013ProfView.MAPID = Master.MAPID + "TAB3"
        CS0013ProfView.VARI = Master.VIEWID
        CS0013ProfView.SRCDATA = TBLview.ToTable
        CS0013ProfView.TBLOBJ = pnlListArea3
        CS0013ProfView.SCROLLTYPE = CS0013ProfView.SCROLLTYPE_ENUM.None
        CS0013ProfView.LEVENT = "ondblclick"
        CS0013ProfView.LFUNC = "ListDbClick"

        CS0013ProfView.TITLEOPT = True
        CS0013ProfView.CS0013ProfView()
        If Not isNormal(CS0013ProfView.ERR) Then
            Master.Output(CS0013ProfView.ERR, C_MESSAGE_TYPE.ABORT, "一覧設定エラー")
            Exit Sub
        End If

        WF_DetailMView.ActiveViewIndex = 2

        '○クリア
        If TBLview.Count = 0 Then
            WF_GridPosition.Text = "1"
        Else
            WF_GridPosition.Text = TBLview.Item(0)("LINECNT")
        End If

        TBLview.Dispose()
        TBLview = Nothing

    End Sub

    ''' <summary>
    ''' GridViewデータ設定(タブ「明細(数量)」表示用)
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub GridViewInitializeTab4(initFlg As Boolean, WW_GridPosition As Integer)

        If initFlg Then
            '○画面表示データ取得
            DBselect_T16SELECT_TAB4()
        End If

        'Sort(T00016tbl)
        CS0026TBLSORTget.TABLE = T00016tbl_tab4
        CS0026TBLSORTget.SORTING = "TORIHIKIYMD ,TORICODE ,TORIHIKIORG ,SHUKABASHO ,TODOKECODE ,NSHABAN"
        CS0026TBLSORTget.FILTER = ""
        CS0026TBLSORTget.Sort(T00016tbl_tab4)

        '○LineCNT付番再付番
        Dim WW_LINECNT As Integer = 0

        For i As Integer = 0 To T00016tbl_tab4.Rows.Count - 1

            Dim T00016row = T00016tbl_tab4.Rows(i)

            If IsDBNull(T00016row("CAMPCODE")) Then
                Continue For
            End If

            If T00016row("LINECNT") = 0 Then

                WW_LINECNT = WW_LINECNT + 1

                T00016tbl_tab4.Rows(i)("LINECNT") = WW_LINECNT

                T00016tbl_tab4.Rows(i)("HIDDEN") = 0

            End If

        Next

        ''○ 画面表示データ保存
        Master.SaveTable(T00016tbl_tab4, work.WF_SEL_INPTAB4TBL.Text)

        '○ 一覧表示データ編集(性能対策)
        Dim TBLview As DataView = New DataView(T00016tbl_tab4)

        TBLview.RowFilter = "HIDDEN = 0 and LINECNT >= " & WW_GridPosition.ToString & " and LINECNT < " & (WW_GridPosition + CONST_DISPROWCOUNT).ToString

        CS0013ProfView.CAMPCODE = work.WF_SEL_CAMPCODE.Text
        CS0013ProfView.PROFID = Master.PROF_VIEW
        CS0013ProfView.MAPID = Master.MAPID + "TAB4"
        CS0013ProfView.VARI = Master.VIEWID
        CS0013ProfView.SRCDATA = TBLview.ToTable
        CS0013ProfView.TBLOBJ = pnlListArea4
        CS0013ProfView.SCROLLTYPE = CS0013ProfView.SCROLLTYPE_ENUM.None
        CS0013ProfView.LEVENT = "ondblclick"
        CS0013ProfView.LFUNC = "ListDbClick"

        CS0013ProfView.TITLEOPT = True
        CS0013ProfView.CS0013ProfView()
        If Not isNormal(CS0013ProfView.ERR) Then
            Master.Output(CS0013ProfView.ERR, C_MESSAGE_TYPE.ABORT, "一覧設定エラー")
            Exit Sub
        End If

        WF_DetailMView.ActiveViewIndex = 3

        '○クリア
        If TBLview.Count = 0 Then
            WF_GridPosition.Text = "1"
        Else
            WF_GridPosition.Text = TBLview.Item(0)("LINECNT")
        End If

        TBLview.Dispose()
        TBLview = Nothing

    End Sub


    ''' <summary>
    ''' 日報取込ボタン押下処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonGet_Click()

        rightview.setErrorReport("")
        Dim O_RTN As String = C_MESSAGE_NO.NORMAL
        Dim WW_DATENOW As Date = Date.Now

        '○画面表示データ復元
        Master.RecoverTable(T00016tbl)

        '日報データ取得
        NippoDATAget(WW_ERRCODE)

        '取引データ設定
        TorihikiDATAset(WW_DATENOW, WW_ERRCODE)

        If Not isNormal(WW_ERRCODE) Then
            Master.Output(WW_ERRCODE, C_MESSAGE_TYPE.ABORT, "取引DB追加")
        End If

        '■■■ 更新前処理（入力情報へ操作を反映）　■■■
        INPtbl_PreUpdate1()

        '■■■ 更新前処理（入力情報へLINECNTを付番）　■■■
        INPtbl_PreUpdate2()

        '読み込みデータ追加
        T00016tbl.Merge(T00016INPtbl)

        '○画面表示データ保存
        Master.SaveTable(T00016tbl)

        '○Detail初期設定
        T00016INPtbl.Clear()

    End Sub

    ''' <summary>
    ''' 追加ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonAdd_Click()


    End Sub
    ''' <summary>
    ''' 日報データ取得
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub NippoDATAget(ByVal O_RTN As String)

        'T0005テンポラリDB項目作成
        If T0005tbl Is Nothing Then
            T0005tbl = New DataTable
        End If

        If T0005tbl.Columns.Count = 0 Then
        Else
            T0005tbl.Columns.Clear()
        End If

        '○DB項目クリア
        T0005tbl.Clear()

        '〇日報データ取得
        Try

            'DataBase接続文字
            Using SQLcon = CS0050SESSION.getConnection
                SQLcon.Open() 'DataBase接続(Open)

                '検索SQL文
                Dim SQLStr As String =
                     "SELECT                                                             " _
                   & "       isnull(rtrim(A.CAMPCODE),'') as CAMPCODE ,                  " _
                   & "       isnull(rtrim(A.SHIPORG),'') as SHIPORG ,                    " _
                   & "       isnull(rtrim(A.TERMKBN),'') as TERMKBN ,                    " _
                   & "       isnull(rtrim(A.YMD),'') as YMD ,                            " _
                   & "       isnull(rtrim(A.STAFFCODE),'') as STAFFCODE ,                " _
                   & "       isnull(rtrim(A.SEQ),'') as SEQ ,                            " _
                   & "       isnull(rtrim(A.ENTRYDATE),'') as ENTRYDATE ,                " _
                   & "       isnull(rtrim(A.CREWKBN),'') as CREWKBN ,                    " _
                   & "       isnull(rtrim(A.NIPPONO),'') as NIPPONO ,                    " _
                   & "       isnull(rtrim(A.WORKKBN),'') as WORKKBN ,                    " _
                   & "       isnull(rtrim(A.GSHABAN),'') as GSHABAN ,                    " _
                   & "       isnull(rtrim(A.SUBSTAFFCODE),'') as SUBSTAFFCODE ,          " _
                   & "       isnull(rtrim(A.STDATE),'') as STDATE ,                      " _
                   & "       isnull(rtrim(A.STTIME),'') as STTIME ,                      " _
                   & "       isnull(rtrim(A.ENDDATE),'') as ENDDATE ,                    " _
                   & "       isnull(rtrim(A.ENDTIME),'') as ENDTIME ,                    " _
                   & "       isnull(rtrim(A.WORKTIME),'') as WORKTIME ,                  " _
                   & "       isnull(rtrim(A.MOVETIME),'') as MOVETIME ,                  " _
                   & "       isnull(rtrim(A.ACTTIME),'') as ACTTIME ,                    " _
                   & "       isnull(rtrim(A.PRATE),'') as PRATE ,                        " _
                   & "       isnull(rtrim(A.CASH),'') as CASH ,                          " _
                   & "       isnull(rtrim(A.TICKET),'') as TICKET ,                      " _
                   & "       isnull(rtrim(A.ETC),'') as ETC ,                            " _
                   & "       isnull(rtrim(A.TOTALTOLL),'') as TOTALTOLL ,                " _
                   & "       isnull(rtrim(A.STMATER),'') as STMATER ,                    " _
                   & "       isnull(rtrim(A.ENDMATER),'') as ENDMATER ,                  " _
                   & "       isnull(rtrim(A.RUIDISTANCE),'') as RUIDISTANCE ,            " _
                   & "       isnull(rtrim(A.SOUDISTANCE),'') as SOUDISTANCE ,            " _
                   & "       isnull(rtrim(A.JIDISTANCE),'') as JIDISTANCE ,              " _
                   & "       isnull(rtrim(A.KUDISTANCE),'') as KUDISTANCE ,              " _
                   & "       isnull(rtrim(A.IPPDISTANCE),'') as IPPDISTANCE ,            " _
                   & "       isnull(rtrim(A.KOSDISTANCE),'') as KOSDISTANCE ,            " _
                   & "       isnull(rtrim(A.IPPJIDISTANCE),'') as IPPJIDISTANCE ,        " _
                   & "       isnull(rtrim(A.IPPKUDISTANCE),'') as IPPKUDISTANCE ,        " _
                   & "       isnull(rtrim(A.KOSJIDISTANCE),'') as KOSJIDISTANCE ,        " _
                   & "       isnull(rtrim(A.KOSKUDISTANCE),'') as KOSKUDISTANCE ,        " _
                   & "       isnull(rtrim(A.KYUYU),'') as KYUYU ,                        " _
                   & "       isnull(rtrim(A.TORICODE),'') as TORICODE ,                  " _
                   & "       isnull(rtrim(A.SHUKABASHO),'') as SHUKABASHO ,              " _
                   & "       isnull(rtrim(A.SHUKADATE),'') as SHUKADATE ,                " _
                   & "       isnull(rtrim(A.TODOKECODE),'') as TODOKECODE ,              " _
                   & "       isnull(rtrim(A.TODOKEDATE),'') as TODOKEDATE ,              " _
                   & "       isnull(rtrim(A.OILTYPE1),'') as OILTYPE1 ,                  " _
                   & "       isnull(rtrim(A.PRODUCT11),'') as PRODUCT11 ,                " _
                   & "       isnull(rtrim(A.PRODUCT21),'') as PRODUCT21 ,                " _
                   & "       isnull(rtrim(A.PRODUCTCODE1),'') as PRODUCTCODE1 ,          " _
                   & "       isnull(rtrim(A.STANI1),'') as STANI1 ,                      " _
                   & "       isnull(rtrim(A.SURYO1),'') as SURYO1 ,                      " _
                   & "       isnull(rtrim(A.OILTYPE2),'') as OILTYPE2 ,                  " _
                   & "       isnull(rtrim(A.PRODUCT12),'') as PRODUCT12 ,                " _
                   & "       isnull(rtrim(A.PRODUCT22),'') as PRODUCT22 ,                " _
                   & "       isnull(rtrim(A.PRODUCTCODE2),'') as PRODUCTCODE2 ,          " _
                   & "       isnull(rtrim(A.STANI2),'') as STANI2 ,                      " _
                   & "       isnull(rtrim(A.SURYO2),'') as SURYO2 ,                      " _
                   & "       isnull(rtrim(A.OILTYPE3),'') as OILTYPE3 ,                  " _
                   & "       isnull(rtrim(A.PRODUCT13),'') as PRODUCT13 ,                " _
                   & "       isnull(rtrim(A.PRODUCT23),'') as PRODUCT23 ,                " _
                   & "       isnull(rtrim(A.PRODUCTCODE3),'') as PRODUCTCODE3 ,          " _
                   & "       isnull(rtrim(A.STANI3),'') as STANI3 ,                      " _
                   & "       isnull(rtrim(A.SURYO3),'') as SURYO3 ,                      " _
                   & "       isnull(rtrim(A.OILTYPE4),'') as OILTYPE4 ,                  " _
                   & "       isnull(rtrim(A.PRODUCT14),'') as PRODUCT14 ,                " _
                   & "       isnull(rtrim(A.PRODUCT24),'') as PRODUCT24 ,                " _
                   & "       isnull(rtrim(A.PRODUCTCODE4),'') as PRODUCTCODE4 ,          " _
                   & "       isnull(rtrim(A.STANI4),'') as STANI4 ,                      " _
                   & "       isnull(rtrim(A.SURYO4),'') as SURYO4 ,                      " _
                   & "       isnull(rtrim(A.OILTYPE5),'') as OILTYPE5 ,                  " _
                   & "       isnull(rtrim(A.PRODUCT15),'') as PRODUCT15 ,                " _
                   & "       isnull(rtrim(A.PRODUCT25),'') as PRODUCT25 ,                " _
                   & "       isnull(rtrim(A.PRODUCTCODE5),'') as PRODUCTCODE5 ,          " _
                   & "       isnull(rtrim(A.STANI5),'') as STANI5 ,                      " _
                   & "       isnull(rtrim(A.SURYO5),'') as SURYO5 ,                      " _
                   & "       isnull(rtrim(A.OILTYPE6),'') as OILTYPE6 ,                  " _
                   & "       isnull(rtrim(A.PRODUCT16),'') as PRODUCT16 ,                " _
                   & "       isnull(rtrim(A.PRODUCT26),'') as PRODUCT26 ,                " _
                   & "       isnull(rtrim(A.PRODUCTCODE6),'') as PRODUCTCODE6 ,          " _
                   & "       isnull(rtrim(A.STANI6),'') as STANI6 ,                      " _
                   & "       isnull(rtrim(A.SURYO6),'') as SURYO6 ,                      " _
                   & "       isnull(rtrim(A.OILTYPE7),'') as OILTYPE7 ,                  " _
                   & "       isnull(rtrim(A.PRODUCT17),'') as PRODUCT17 ,                " _
                   & "       isnull(rtrim(A.PRODUCT27),'') as PRODUCT27 ,                " _
                   & "       isnull(rtrim(A.PRODUCTCODE7),'') as PRODUCTCODE7 ,          " _
                   & "       isnull(rtrim(A.STANI7),'') as STANI7 ,                      " _
                   & "       isnull(rtrim(A.SURYO7),'') as SURYO7 ,                      " _
                   & "       isnull(rtrim(A.OILTYPE8),'') as OILTYPE8 ,                  " _
                   & "       isnull(rtrim(A.PRODUCT18),'') as PRODUCT18 ,                " _
                   & "       isnull(rtrim(A.PRODUCT28),'') as PRODUCT28 ,                " _
                   & "       isnull(rtrim(A.PRODUCTCODE8),'') as PRODUCTCODE8 ,          " _
                   & "       isnull(rtrim(A.STANI8),'') as STANI8 ,                      " _
                   & "       isnull(rtrim(A.SURYO8),'') as SURYO8 ,                      " _
                   & "       isnull(rtrim(A.TOTALSURYO),'') as TOTALSURYO ,              " _
                   & "       isnull(rtrim(A.TUMIOKIKBN),'') as TUMIOKIKBN ,              " _
                   & "       isnull(rtrim(A.ORDERNO),'') as ORDERNO ,                    " _
                   & "       isnull(rtrim(A.DETAILNO),'') as DETAILNO ,                  " _
                   & "       isnull(rtrim(A.TRIPNO),'') as TRIPNO ,                      " _
                   & "       isnull(rtrim(A.DROPNO),'') as DROPNO ,                      " _
                   & "       isnull(rtrim(A.JISSKIKBN),'') as JISSKIKBN ,                " _
                   & "       isnull(rtrim(A.URIKBN),'') as URIKBN ,                      " _
                   & "       isnull(rtrim(A.STORICODE),'') as STORICODE ,                " _
                   & "       isnull(rtrim(A.CONTCHASSIS),'') as CONTCHASSIS ,            " _
                   & "       isnull(rtrim(A.SHARYOTYPEF),'') as SHARYOTYPEF ,            " _
                   & "       isnull(rtrim(A.TSHABANF),'') as TSHABANF ,                  " _
                   & "       isnull(rtrim(A.SHARYOTYPEB),'') as SHARYOTYPEB ,            " _
                   & "       isnull(rtrim(A.TSHABANB),'') as TSHABANB ,                  " _
                   & "       isnull(rtrim(A.SHARYOTYPEB2),'') as SHARYOTYPEB2 ,          " _
                   & "       isnull(rtrim(A.TSHABANB2),'') as TSHABANB2 ,                " _
                   & "       isnull(rtrim(A.TAXKBN),'') as TAXKBN ,                      " _
                   & "       isnull(rtrim(A.LATITUDE),'') as LATITUDE ,                  " _
                   & "       isnull(rtrim(A.LONGITUDE),'') as LONGITUDE ,                " _
                   & "       isnull(rtrim(A.L1SHUKODATE),'') as L1SHUKODATE ,            " _
                   & "       isnull(rtrim(A.L1SHUKADATE),'') as L1SHUKADATE ,            " _
                   & "       isnull(rtrim(A.L1TODOKEDATE),'') as L1TODOKEDATE ,          " _
                   & "       isnull(rtrim(A.L1TRIPNO),'') as L1TRIPNO ,                  " _
                   & "       isnull(rtrim(A.L1DROPNO),'') as L1DROPNO ,                  " _
                   & "       isnull(rtrim(A.L1TORICODE),'') as L1TORICODE ,              " _
                   & "       isnull(rtrim(A.L1URIKBN),'') as L1URIKBN ,                  " _
                   & "       isnull(rtrim(A.L1STORICODE),'') as L1STORICODE ,            " _
                   & "       isnull(rtrim(A.L1TODOKECODE),'') as L1TODOKECODE ,          " _
                   & "       isnull(rtrim(A.L1SHUKABASHO),'') as L1SHUKABASHO ,          " _
                   & "       isnull(rtrim(A.L1CREWKBN),'') as L1CREWKBN ,                " _
                   & "       isnull(rtrim(A.L1STAFFKBN),'') as L1STAFFKBN ,              " _
                   & "       isnull(rtrim(A.L1STAFFCODE),'') as L1STAFFCODE ,            " _
                   & "       isnull(rtrim(A.L1SUBSTAFFCODE),'') as L1SUBSTAFFCODE ,      " _
                   & "       isnull(rtrim(A.L1ORDERNO),'') as L1ORDERNO ,                " _
                   & "       isnull(rtrim(A.L1DETAILNO),'') as L1DETAILNO ,              " _
                   & "       isnull(rtrim(A.L1ORDERORG),'') as L1ORDERORG ,              " _
                   & "       isnull(rtrim(A.L1KAISO),'') as L1KAISO ,                    " _
                   & "       isnull(rtrim(A.L1KUSHAKBN),'') as L1KUSHAKBN ,              " _
                   & "       isnull(rtrim(A.L1IPPDISTANCE),'') as L1IPPDISTANCE ,        " _
                   & "       isnull(rtrim(A.L1KOSDISTANCE),'') as L1KOSDISTANCE ,        " _
                   & "       isnull(rtrim(A.L1IPPJIDISTANCE),'') as L1IPPJIDISTANCE ,    " _
                   & "       isnull(rtrim(A.L1IPPKUDISTANCE),'') as L1IPPKUDISTANCE ,    " _
                   & "       isnull(rtrim(A.L1KOSJIDISTANCE),'') as L1KOSJIDISTANCE ,    " _
                   & "       isnull(rtrim(A.L1KOSKUDISTANCE),'') as L1KOSKUDISTANCE ,    " _
                   & "       isnull(rtrim(A.L1WORKTIME),'') as L1WORKTIME ,              " _
                   & "       isnull(rtrim(A.L1MOVETIME),'') as L1MOVETIME ,              " _
                   & "       isnull(rtrim(A.L1ACTTIME),'') as L1ACTTIME ,                " _
                   & "       isnull(rtrim(A.L1JIMOVETIME),'') as L1JIMOVETIME ,          " _
                   & "       isnull(rtrim(A.L1KUMOVETIME),'') as L1KUMOVETIME ,          " _
                   & "       isnull(rtrim(A.L1HAISOGROUP),'') as L1HAISOGROUP ,          " _
                   & "       isnull(rtrim(A.DELFLG),'') as DELFLG ,                      " _
                   & "       isnull(rtrim(B.MANGSHAFUKU),'') as SHAFUKU ,                " _
                   & "       (SELECT isnull(rtrim(C.STDATE),'') FROM T0005_NIPPO C       " _
                   & "  	  WHERE C.CAMPCODE = A.CAMPCODE 							 " _
                   & "  	    AND C.SHIPORG = A.SHIPORG							     " _
                   & "  	    AND C.YMD = A.YMD							             " _
                   & "  	    AND C.STAFFCODE = A.STAFFCODE							 " _
                   & "  	    AND C.NIPPONO = A.NIPPONO							     " _
                   & "  	    AND C.WORKKBN = 'F3'							         " _
                   & "  	    AND C.DELFLG <> '1' ) as KIKODATE						 " _
                   & "  FROM T0005_NIPPO A								                 " _
                   & "  LEFT JOIN MA002_SHARYOA B								         " _
                   & "    ON B.CAMPCODE = A.CAMPCODE							         " _
                   & "   AND B.SHARYOTYPE = A.SHARYOTYPEB						         " _
                   & "   AND B.TSHABAN = A.TSHABANB                                      " _
                   & "   AND B.STYMD <= A.YMD                                            " _
                   & "   AND B.ENDYMD >= A.YMD                                           " _
                   & " WHERE A.CAMPCODE         = @P01                                   " _
                   & "   and A.YMD             <= @P02                                   " _
                   & "   and A.YMD             >= @P03                                   " _
                   & "   and A.OILTYPE1         = @P05                                   " _
                   & "   and A.TORICODE         = @P06                                   " _
                   & "   and A.WORKKBN          = 'B3'                                   " _
                   & "   and A.PRODUCT11        = '21'                                   " _
                   & "   and A.DELFLG          <> '1'                                    "

                '条件画面で指定された出荷部署を抽出
                If work.WF_SEL_SHIPORG.Text <> Nothing Then
                    SQLStr = SQLStr & "   and A.SHIPORG          = @P04           	   "
                End If

                SQLStr = SQLStr & " ORDER BY CAMPCODE ,YMD ,SEQ                        "

                Dim SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", System.Data.SqlDbType.NVarChar, 2)
                Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", System.Data.SqlDbType.Date)
                Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@P03", System.Data.SqlDbType.Date)
                Dim PARA04 As SqlParameter = SQLcmd.Parameters.Add("@P04", System.Data.SqlDbType.NVarChar, 15)
                Dim PARA05 As SqlParameter = SQLcmd.Parameters.Add("@P05", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA06 As SqlParameter = SQLcmd.Parameters.Add("@P06", System.Data.SqlDbType.NVarChar, 20)

                PARA01.Value = work.WF_SEL_CAMPCODE.Text

                '対象年月
                If Not String.IsNullOrEmpty(work.WF_SEL_SEIKYUYMF.Text) Then

                    Dim WW_DATE As Date
                    If Date.TryParse(work.WF_SEL_SEIKYUYMF.Text, WW_DATE) Then
                        PARA02.Value = WW_DATE.AddMonths(1).AddDays(-1).ToString("yyyy/MM/dd")
                        PARA03.Value = work.WF_SEL_SEIKYUYMF.Text & "/01"
                    End If

                ElseIf Not String.IsNullOrEmpty(work.WF_SEL_KEIJYODATEF.Text) Then

                    PARA02.Value = work.WF_SEL_KEIJYODATET.Text
                    PARA03.Value = work.WF_SEL_KEIJYODATEF.Text

                End If

                PARA04.Value = work.WF_SEL_SHIPORG.Text

                PARA05.Value = work.WF_SEL_OILTYPE.Text

                PARA06.Value = work.WF_SEL_TORICODE.Text

                SQLcmd.CommandTimeout = 300

                Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                'フィールド名とフィールドの型を取得
                For index As Integer = 0 To SQLdr.FieldCount - 1
                    T0005tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                Next
                '〇テーブル検索結果をテーブル格納
                T0005tbl.Load(SQLdr)

                'Close
                SQLdr.Close() 'Reader(Close)
                SQLdr = Nothing

                SQLcmd.Dispose()
                SQLcmd = Nothing

                SQLcon.Close() 'DataBase接続(Close)
            End Using

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "T0005_NIPPO SELECT")
            CS0011LOGWRITE.INFSUBCLASS = "MAIN"                        'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:T0005_NIPPO Select"           '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                '
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                            'ログ出力

            O_RTN = C_MESSAGE_NO.DB_ERROR
            Exit Sub

        End Try

    End Sub

    ''' <summary>
    ''' 取引データ設定
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub TorihikiDATAset(ByVal I_DATENOW As Date, ByVal O_RTN As String)

        '○T00016INPtblカラム設定
        Master.CreateEmptyTable(T00016INPtbl)

        '○DB項目クリア
        T00016INPtbl.Clear()

        Try

            For Each T0005row As DataRow In T0005tbl.Rows

                Dim T00016INProw = T00016INPtbl.NewRow()

                T00016INProw("LINECNT") = 0
                T00016INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                T00016INProw("TIMSTP") = "0"
                T00016INProw("HIDDEN") = 0
                T00016INProw("WORK_NO") = ""

                T00016INProw("CAMPCODE") = T0005row("CAMPCODE")                     '会社コード(CAMPCODE)
                T00016INProw("DENKBN") = "01"                                       '伝票区分(DENKBN)

                '伝票番号採番
                CS0033AutoNumber.CAMPCODE = T0005row("CAMPCODE")
                CS0033AutoNumber.MORG = ""
                CS0033AutoNumber.SEQTYPE = "DENNO_TORIHIKI"
                CS0033AutoNumber.USERID = Master.USERID
                CS0033AutoNumber.getAutoNumber()
                If CS0033AutoNumber.ERR = C_MESSAGE_NO.NORMAL Then
                    T00016INProw("DENNO") = CS0033AutoNumber.SEQ                    '伝票番号(DENNO)
                Else
                    CS0011LOGWRITE.INFSUBCLASS = "TorihikiDATAset"                  'SUBクラス名
                    CS0011LOGWRITE.INFPOSI = "TorihikiDATAset"                      '
                    CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                     '
                    CS0011LOGWRITE.TEXT = "採番エラー"
                    CS0011LOGWRITE.MESSAGENO = CS0033AutoNumber.ERR
                    CS0011LOGWRITE.CS0011LOGWrite()                                 'ログ出力
                    Exit Sub
                End If

                T00016INProw("TORIHIKIYMD") = RTrim(T0005row("YMD"))                '取引日付(TORIHIKIYMD)
                T00016INProw("RECODEKBN") = "0"                                     'レコード区分(RECODEKBN)
                T00016INProw("TORICODE") = T0005row("TORICODE")                     '荷主（取引先）(TORICODE)
                T00016INProw("TODOKECODE") = T0005row("TODOKECODE")                 '届先(TODOKECODE)
                T00016INProw("GSHABAN") = T0005row("GSHABAN")                       '業務車番(GSHABAN)
                T00016INProw("NSHABAN") = T0005row("GSHABAN")                       '荷主車番(NSHABAN)
                T00016INProw("UNCHINCODE") = "1000"                                 '運賃コード(UNCHINCODE)
                T00016INProw("ENTRYDATE") = I_DATENOW.ToString("yyyyMMddHHmmssfff") 'エントリー日時(ENTRYDATE)
                T00016INProw("ACTORICODE") = ""                                     '経理用取引先(ACTORICODE)
                T00016INProw("URIKBN") = T0005row("URIKBN")                         '売上請求・支払請求計上基準(URIKBN)
                T00016INProw("TORIHIKIMANGORG") = T0005row("SHAFUKU")               '取引発生管理部署(TORIHIKIMANGORG)
                T00016INProw("TORIHIKIORG") = T0005row("SHIPORG")                   '取引発生部署(TORIHIKIORG)
                T00016INProw("SEIKYUSHIHARAIMANGORG") = ""                          '請求支払取纏め管理部署(SEIKYUSHIHARAIMANGORG)
                T00016INProw("SEIKYUSHIHARAIORG") = ""                              '請求支払取纏め部署(SEIKYUSHIHARAIORG)
                T00016INProw("SEIKYUSHIHARAIYM") = ""                               '請求支払取纏め年月(SEIKYUSHIHARAIYM)
                T00016INProw("URIKEIJYOYMD") = ""                                   '売上計上日付(URIKEIJYOYMD)
                T00016INProw("SEIKYUNO") = ""                                       '売上請求書No(SEIKYUNO)
                T00016INProw("URIPATERNKBN") = ""                                   '売上計上パターン分類(URIPATERNKBN)
                T00016INProw("URIPATTERNCODE") = ""                                 '売上計上パターンCD(URIPATTERNCODE)
                T00016INProw("URIAMT") = ""                                         '売上明細金額(URIAMT)
                T00016INProw("URITAXAMT") = ""                                      '売上明細消費税(URITAXAMT)
                T00016INProw("URISEGMENT1") = ""                                    '売上明細セグメント１(URISEGMENT1)
                T00016INProw("URISEGMENT2") = ""                                    '売上明細セグメント２(URISEGMENT2)
                T00016INProw("URISEGMENT3") = ""                                    '売上明細セグメント３(URISEGMENT3)
                T00016INProw("NDEADLINEDAYS") = ""                                  '荷主請求締日(NDEADLINEDAYS)
                T00016INProw("JOTSEIKYUKBN") = ""                                   'JOT請求区分(JOTSEIKYUKBN)
                T00016INProw("SEIKYUOUTYMD") = ""                                   '売上請求書発行日(SEIKYUOUTYMD)
                T00016INProw("NYUKINSITE") = ""                                     '入金サイト(NYUKINSITE)
                T00016INProw("NYUKINYMD") = ""                                      '入金予定日(NYUKINYMD)
                T00016INProw("SHIHARAIKEIJYOYMD") = ""                              '支払計上日付(SHIHARAIKEIJYOYMD)
                T00016INProw("SHIHARAINO") = ""                                     '支払請求書No(SHIHARAINO)
                T00016INProw("SHIHARAIPATERNKBN") = ""                              '支払計上パターン分類(SHIHARAIPATERNKBN)
                T00016INProw("SHIHARAIPATTERNCODE") = ""                            '支払計上パターンCD(SHIHARAIPATTERNCODE)
                T00016INProw("SHIHARAIAMT") = ""                                    '支払明細金額(SHIHARAIAMT)
                T00016INProw("SHIHARAITAXAMT") = ""                                 '支払明細消費税(SHIHARAITAXAMT)
                T00016INProw("SHIHARAISEGMENT1") = ""                               '支払明細セグメント１(SHIHARAISEGMENT1)
                T00016INProw("SHIHARAISEGMENT2") = ""                               '支払明細セグメント２(SHIHARAISEGMENT2)
                T00016INProw("SHIHARAISEGMENT3") = ""                               '支払明細セグメント３(SHIHARAISEGMENT3)
                T00016INProw("GDEADLINEDAYS") = ""                                  '業者請求締日(GDEADLINEDAYS)
                T00016INProw("SEIKYUMATCHYMD") = ""                                 '請求書照合日(SEIKYUMATCHYMD)
                T00016INProw("SHIHARAISITE") = ""                                   '支払サイト(SHIHARAISITE)
                T00016INProw("SHIHARAIYMD") = ""                                    '支払予定日(SHIHARAIYMD)
                T00016INProw("BANKCODE") = ""                                       '銀行CD(BANKCODE)
                T00016INProw("SEIKYUKBN") = ""                                      '請求書明細区分(SEIKYUKBN)
                T00016INProw("NIPPONO") = T0005row("NIPPONO")                       '日報No(NIPPONO)
                T00016INProw("ORDERNO") = T0005row("ORDERNO")                       '用車実績No(ORDERNO)
                T00016INProw("SHUKODATE") = T0005row("YMD")                         '出庫日(SHUKODATE)
                T00016INProw("SHUKADATE") = T0005row("SHUKADATE")                   '積日（出荷日）(SHUKADATE)
                T00016INProw("TODOKEDATE") = T0005row("TODOKEDATE")                 '届日(TODOKEDATE)
                T00016INProw("SHUKABASHO") = T0005row("SHUKABASHO")                 '出荷場所(SHUKABASHO)
                T00016INProw("SHUKACITIES") = ""                                    '出荷場所の市区町村ＣＤ(SHUKACITIES)
                T00016INProw("TODOKECITIES") = ""                                   '届先の市区町村ＣＤ(TODOKECITIES)
                T00016INProw("SHARYOTYPEF") = T0005row("SHARYOTYPEF")               '統一車番(前)(上)(SHARYOTYPEF)
                T00016INProw("TSHABANF") = T0005row("TSHABANF")                     '統一車番(前)(下)(TSHABANF)
                T00016INProw("SHARYOTYPEB") = T0005row("SHARYOTYPEB")               '統一車番(後)(上)(SHARYOTYPEB)
                T00016INProw("TSHABANB") = T0005row("TSHABANB")                     '統一車番(後)(下)(TSHABANB)
                T00016INProw("SHARYOTYPEB2") = T0005row("SHARYOTYPEB2")             '統一車番(後)(上)２(SHARYOTYPEB2)
                T00016INProw("TSHABANB2") = T0005row("TSHABANB2")                   '統一車番(後)(下)２(TSHABANB2)
                T00016INProw("SHARYOKBN") = ""                                      '車両区分(SHARYOKBN)
                T00016INProw("SHAFUKU") = T0005row("SHAFUKU")                       '車腹(SHAFUKU)
                T00016INProw("TRIPNO") = T0005row("TRIPNO")                         'トリップ(TRIPNO)
                T00016INProw("DROPNO") = T0005row("DROPNO")                         'ドロップ(DROPNO)

                Dim cnt As Integer = 0
                If Not String.IsNullOrEmpty(T0005row("STAFFCODE")) Then
                    cnt = cnt + 1
                End If

                If Not String.IsNullOrEmpty(T0005row("SUBSTAFFCODE")) Then
                    cnt = cnt + 1
                End If
                T00016INProw("STAFFSU") = cnt                                       '乗務人数(STAFFSU)

                T00016INProw("STAFFCODE") = T0005row("STAFFCODE")                   '乗務員(STAFFCODE)
                T00016INProw("SUBSTAFFCODE") = T0005row("SUBSTAFFCODE")             '副乗務員(SUBSTAFFCODE)

                T00016INProw("TUKORYOKBN") = ""                                     '通行料区分(TUKORYOKBN)
                T00016INProw("TUKORYO") = T0005row("TOTALTOLL")                     '日報通行料(TUKORYO)
                T00016INProw("TRIPSTTIME") = ""                                     'トリップ開始時間(TRIPSTTIME)
                T00016INProw("TRIPENDTIME") = ""                                    'トリップ終了時間(TRIPENDTIME)
                T00016INProw("KYUYU") = T0005row("KYUYU")                           '日報給油(KYUYU)
                T00016INProw("UNCHINDISTANCE") = T0005row("SOUDISTANCE")            '運賃計算配送距離(UNCHINDISTANCE)
                T00016INProw("KEIRYONO") = ""                                       '計量番号(KEIRYONO)
                T00016INProw("UNCHINCALCKBN") = ""                                  '運賃計算方法(UNCHINCALCKBN)
                T00016INProw("ROUNDTRIPDISTANCEAUTO") = ""                          '積届往復固定距離（自動）(ROUNDTRIPDISTANCEAUTO)
                T00016INProw("ROUNDTRIPDISTANCEHAND") = ""                          '積届往復固定距離（手入力）(ROUNDTRIPDISTANCEHAND)
                T00016INProw("ROUNDTRIPDISTANCE") = ""                              '積届往復固定距離（結果）(ROUNDTRIPDISTANCE)
                T00016INProw("UNKOUKAISUAUTO") = ""                                 '運行回数（自動）(UNKOUKAISUAUTO)
                T00016INProw("UNKOUKAISUHAND") = ""                                 '運行回数（手入力）(UNKOUKAISUHAND)
                T00016INProw("UNKOUKAISU") = ""                                     '運行回数（結果）(UNKOUKAISU)
                T00016INProw("UNKOUNISSUAUTO") = ""                                 '運行日数（自動）(UNKOUNISSUAUTO)
                T00016INProw("UNKOUNISSUHAND") = ""                                 '運行日数（手入力）(UNKOUNISSUHAND)
                T00016INProw("UNKOUNISSU") = ""                                     '運行日数（結果）(UNKOUNISSU)
                T00016INProw("PUBLICHOLIDAYNISSUAUTO") = ""                         '日祝日運行日数（自動）(PUBLICHOLIDAYNISSUAUTO)
                T00016INProw("PUBLICHOLIDAYNISSUHAND") = ""                         '日祝日運行日数（手入力）(PUBLICHOLIDAYNISSUHAND)
                T00016INProw("PUBLICHOLIDAYNISSU") = ""                             '日祝日運行日数（結果）(PUBLICHOLIDAYNISSU)
                T00016INProw("PUBLICHOLIDAYKADONISSUAUTO") = ""                     '日祝日車庫稼働日数（自動）(PUBLICHOLIDAYKADONISSUAUTO)
                T00016INProw("PUBLICHOLIDAYKADONISSUHAND") = ""                     '日祝日車庫稼働日数（手入力）(PUBLICHOLIDAYKADONISSUHAND)
                T00016INProw("PUBLICHOLIDAYKADONISSU") = ""                         '日祝日車庫稼働日数（結果）(PUBLICHOLIDAYKADONISSU)
                T00016INProw("NENMATUNEMSHINISSUAUTO") = ""                         '年末年始運行日数（自動）(NENMATUNEMSHINISSUAUTO)
                T00016INProw("NENMATUNEMSHINISSUHAND") = ""                         '年末年始運行日数（手入力）(NENMATUNEMSHINISSUHAND)
                T00016INProw("NENMATUNEMSHINISSU") = ""                             '年末年始運行日数（結果）(NENMATUNEMSHINISSU)
                T00016INProw("KEIYAKUDAISUAUTO") = ""                               '車両契約台数（自動）(KEIYAKUDAISUAUTO)
                T00016INProw("KEIYAKUDAISUHAND") = ""                               '車両契約台数（手入力）(KEIYAKUDAISUHAND)
                T00016INProw("KEIYAKUDAISU") = ""                                   '車両契約台数（結果）(KEIYAKUDAISU)
                T00016INProw("AMTAUTO") = ""                                        '請求支払額（自動）(AMTAUTO)
                T00016INProw("AMTHAND") = ""                                        '請求支払額（手入力）(AMTHAND)
                T00016INProw("AMT") = ""                                            '請求支払額（結果）(AMT)
                T00016INProw("RELATIONNO") = ""                                     '売上／支払関連番号(RELATIONNO)

                T00016INProw("DELFLG") = C_DELETE_FLG.ALIVE

                'T00016INProw("INITYMD") = I_DATENOW                                 '登録年月日(INITYMD)
                'T00016INProw("UPDYMD") = I_DATENOW                                  '更新年月日(UPDYMD)
                'T00016INProw("UPDUSER") = Master.USERID                             '更新ユーザＩＤ(UPDUSER)
                'T00016INProw("UPDTERMID") = Master.USERTERMID                       '更新端末(UPDTERMID)
                'T00016INProw("RECEIVEYMD") = C_DEFAULT_YMD                          '集信日時(RECEIVEYMD)

                For index = 1 To 8

                    Select Case index

                        Case 1

                            If String.IsNullOrEmpty(T0005row("OILTYPE1")) Then
                                Continue For
                            End If

                            T00016INProw("OILTYPE") = T0005row("OILTYPE1")                      '油種(OILTYPE)
                            T00016INProw("PRODUCTCODE") = T0005row("PRODUCTCODE1")              '品名(PRODUCTCODE)
                            T00016INProw("JSURYO") = CType(T0005row("SURYO1"), Double)          '配送実績数量(JSURYO)
                            T00016INProw("JTANI") = T0005row("STANI1")                          '配送実績数量単位(JTANI)

                            T00016INProw("SURYOAUTO") = CType(T0005row("SURYO1"), Double)       '配送数量（自動）(SURYOAUTO)
                            T00016INProw("SURYOHAND") = ""                                      '配送数量（手入力）(SURYOHAND)
                            T00016INProw("SURYO") = CType(T0005row("SURYO1"), Double)           '配送数量（結果）(SURYO)

                        Case 2

                            If String.IsNullOrEmpty(T0005row("OILTYPE2")) Then
                                Continue For
                            End If

                            T00016INProw("OILTYPE") = T0005row("OILTYPE2")                      '油種(OILTYPE)
                            T00016INProw("PRODUCTCODE") = T0005row("PRODUCTCODE2")              '品名(PRODUCTCODE)
                            T00016INProw("JSURYO") = CType(T0005row("SURYO2"), Double)          '配送実績数量(JSURYO)
                            T00016INProw("JTANI") = T0005row("STANI2")                          '配送実績数量単位(JTANI)

                            T00016INProw("SURYOAUTO") = CType(T0005row("SURYO2"), Double)       '配送数量（自動）(SURYOAUTO)
                            T00016INProw("SURYOHAND") = ""                                      '配送数量（手入力）(SURYOHAND)
                            T00016INProw("SURYO") = CType(T0005row("SURYO2"), Double)           '配送数量（結果）(SURYO)

                        Case 3

                            If String.IsNullOrEmpty(T0005row("OILTYPE3")) Then
                                Continue For
                            End If

                            T00016INProw("OILTYPE") = T0005row("OILTYPE3")                      '油種(OILTYPE)
                            T00016INProw("PRODUCTCODE") = T0005row("PRODUCTCODE3")              '品名(PRODUCTCODE)
                            T00016INProw("JSURYO") = CType(T0005row("SURYO3"), Double)          '配送実績数量(JSURYO)
                            T00016INProw("JTANI") = T0005row("STANI3")                          '配送実績数量単位(JTANI)

                            T00016INProw("SURYOAUTO") = CType(T0005row("SURYO3"), Double)       '配送数量（自動）(SURYOAUTO)
                            T00016INProw("SURYOHAND") = ""                                      '配送数量（手入力）(SURYOHAND)
                            T00016INProw("SURYO") = CType(T0005row("SURYO3"), Double)           '配送数量（結果）(SURYO)

                        Case 4

                            If String.IsNullOrEmpty(T0005row("OILTYPE4")) Then
                                Continue For
                            End If

                            T00016INProw("OILTYPE") = T0005row("OILTYPE4")                      '油種(OILTYPE)
                            T00016INProw("PRODUCTCODE") = T0005row("PRODUCTCODE4")              '品名(PRODUCTCODE)
                            T00016INProw("JSURYO") = CType(T0005row("SURYO4"), Double)          '配送実績数量(JSURYO)
                            T00016INProw("JTANI") = T0005row("STANI4")                          '配送実績数量単位(JTANI)

                            T00016INProw("SURYOAUTO") = CType(T0005row("SURYO4"), Double)       '配送数量（自動）(SURYOAUTO)
                            T00016INProw("SURYOHAND") = ""                                      '配送数量（手入力）(SURYOHAND)
                            T00016INProw("SURYO") = CType(T0005row("SURYO4"), Double)           '配送数量（結果）(SURYO)

                        Case 5

                            If String.IsNullOrEmpty(T0005row("OILTYPE5")) Then
                                Continue For
                            End If

                            T00016INProw("OILTYPE") = T0005row("OILTYPE5")                      '油種(OILTYPE)
                            T00016INProw("PRODUCTCODE") = T0005row("PRODUCTCODE5")              '品名(PRODUCTCODE)
                            T00016INProw("JSURYO") = CType(T0005row("SURYO5"), Double)          '配送実績数量(JSURYO)
                            T00016INProw("JTANI") = T0005row("STANI5")                          '配送実績数量単位(JTANI)

                            T00016INProw("SURYOAUTO") = CType(T0005row("SURYO5"), Double)       '配送数量（自動）(SURYOAUTO)
                            T00016INProw("SURYOHAND") = ""                                      '配送数量（手入力）(SURYOHAND)
                            T00016INProw("SURYO") = CType(T0005row("SURYO5"), Double)           '配送数量（結果）(SURYO)

                        Case 6

                            If String.IsNullOrEmpty(T0005row("OILTYPE6")) Then
                                Continue For
                            End If

                            T00016INProw("OILTYPE") = T0005row("OILTYPE6")                      '油種(OILTYPE)
                            T00016INProw("PRODUCTCODE") = T0005row("PRODUCTCODE6")              '品名(PRODUCTCODE)
                            T00016INProw("JSURYO") = CType(T0005row("SURYO6"), Double)          '配送実績数量(JSURYO)
                            T00016INProw("JTANI") = T0005row("STANI6")                          '配送実績数量単位(JTANI)

                            T00016INProw("SURYOAUTO") = CType(T0005row("SURYO6"), Double)       '配送数量（自動）(SURYOAUTO)
                            T00016INProw("SURYOHAND") = ""                                      '配送数量（手入力）(SURYOHAND)
                            T00016INProw("SURYO") = CType(T0005row("SURYO6"), Double)           '配送数量（結果）(SURYO)

                        Case 7

                            If String.IsNullOrEmpty(T0005row("OILTYPE7")) Then
                                Continue For
                            End If

                            T00016INProw("OILTYPE") = T0005row("OILTYPE7")                      '油種(OILTYPE)
                            T00016INProw("PRODUCTCODE") = T0005row("PRODUCTCODE7")              '品名(PRODUCTCODE)
                            T00016INProw("JSURYO") = CType(T0005row("SURYO7"), Double)          '配送実績数量(JSURYO)
                            T00016INProw("JTANI") = T0005row("STANI7")                          '配送実績数量単位(JTANI)

                            T00016INProw("SURYOAUTO") = CType(T0005row("SURYO7"), Double)       '配送数量（自動）(SURYOAUTO)
                            T00016INProw("SURYOHAND") = ""                                      '配送数量（手入力）(SURYOHAND)
                            T00016INProw("SURYO") = CType(T0005row("SURYO7"), Double)           '配送数量（結果）(SURYO)

                        Case 8

                            If String.IsNullOrEmpty(T0005row("OILTYPE8")) Then
                                Continue For
                            End If

                            T00016INProw("OILTYPE") = T0005row("OILTYPE8")                      '油種(OILTYPE)
                            T00016INProw("PRODUCTCODE") = T0005row("PRODUCTCODE8")              '品名(PRODUCTCODE)
                            T00016INProw("JSURYO") = CType(T0005row("SURYO8"), Double)          '配送実績数量(JSURYO)
                            T00016INProw("JTANI") = T0005row("STANI8")                          '配送実績数量単位(JTANI)

                            T00016INProw("SURYOAUTO") = CType(T0005row("SURYO8"), Double)       '配送数量（自動）(SURYOAUTO)
                            T00016INProw("SURYOHAND") = ""                                      '配送数量（手入力）(SURYOHAND)
                            T00016INProw("SURYO") = CType(T0005row("SURYO8"), Double)           '配送数量（結果）(SURYO)

                    End Select

                    T00016INProw("DETAILNO") = index                                '明細番号(DETAILNO)

                    '更新テーブル追加
                    T00016INPtbl.Rows.Add(T00016INProw)

                    ''○T00016INProwをT00016tblへ追加
                    'T00016tbl.ImportRow(T00016INProw)

                Next
            Next

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "T0005_NIPPO SELECT")
            CS0011LOGWRITE.INFSUBCLASS = "MAIN"                        'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:T0005_NIPPO Select"           '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                '
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                            'ログ出力

            O_RTN = C_MESSAGE_NO.DB_ERROR
            Exit Sub

        End Try

    End Sub

    ''' <summary>
    ''' T00016tbl関連データ削除
    ''' </summary>
    ''' <param name="I_DATENOW">更新時刻</param>
    ''' <param name="O_RTN">RTNCODE</param>
    ''' <remarks></remarks>
    Protected Sub NippoDATAdelete(ByVal I_DATENOW As Date, ByVal O_RTN As String)

        Try
            'DataBase接続文字
            Dim SQLcon = CS0050SESSION.getConnection
            SQLcon.Open() 'DataBase接続(Open)

            '更新SQL文･･･一括論理削除
            Dim SQLStr As String =
                      " UPDATE T0016_TORIHIKI           " _
                    & "    SET UPDYMD      = @P11,      " _
                    & "        UPDUSER     = @P12,      " _
                    & "        UPDTERMID   = @P13,      " _
                    & "        RECEIVEYMD  = @P14,      " _
                    & "        DELFLG      = '1'        " _
                    & "  WHERE CAMPCODE    = @P01       " _
                    & "    AND DENKBN      = @P02       " _
                    & "    AND DENNO       = @P03       " _
                    & "    AND TORIHIKIYMD = @P04       " _
                    & "    AND RECODEKBN   = @P05       " _
                    & "    AND TORICODE    = @P06       " _
                    & "    AND TODOKECODE  = @P07       " _
                    & "    AND GSHABAN     = @P08       " _
                    & "    AND NSHABAN     = @P09       " _
                    & "    AND DELFLG     <> '1'        "

            Dim SQLcmd As New SqlCommand(SQLStr, SQLcon)
            SQLcmd.CommandTimeout = 300

            Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", System.Data.SqlDbType.NVarChar)
            Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", System.Data.SqlDbType.NVarChar)
            Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@P03", System.Data.SqlDbType.NVarChar)
            Dim PARA04 As SqlParameter = SQLcmd.Parameters.Add("@P04", System.Data.SqlDbType.Date)
            Dim PARA05 As SqlParameter = SQLcmd.Parameters.Add("@P05", System.Data.SqlDbType.NVarChar)
            Dim PARA06 As SqlParameter = SQLcmd.Parameters.Add("@P06", System.Data.SqlDbType.NVarChar)
            Dim PARA07 As SqlParameter = SQLcmd.Parameters.Add("@P07", System.Data.SqlDbType.NVarChar)
            Dim PARA08 As SqlParameter = SQLcmd.Parameters.Add("@P08", System.Data.SqlDbType.NVarChar)
            Dim PARA09 As SqlParameter = SQLcmd.Parameters.Add("@P09", System.Data.SqlDbType.NVarChar)

            Dim PARA11 As SqlParameter = SQLcmd.Parameters.Add("@P11", System.Data.SqlDbType.DateTime)
            Dim PARA12 As SqlParameter = SQLcmd.Parameters.Add("@P12", System.Data.SqlDbType.NVarChar)
            Dim PARA13 As SqlParameter = SQLcmd.Parameters.Add("@P13", System.Data.SqlDbType.NVarChar)
            Dim PARA14 As SqlParameter = SQLcmd.Parameters.Add("@P14", System.Data.SqlDbType.DateTime)

            Dim WW_DENKBN As String = ""
            Dim WW_DENNO As String = ""
            Dim WW_TORIHIKIYMD As String = ""
            Dim WW_RECODEKBN As String = ""
            Dim WW_TORICODE As String = ""
            Dim WW_TODOKECODE As String = ""
            Dim WW_GSHABAN As String = ""
            Dim WW_NSHABAN As String = ""

            For Each T0005row In T0005tbl.Rows

                If T0005row("DENKBN") <> WW_DENKBN OrElse
                   T0005row("DENNO") <> WW_DENNO OrElse
                   T0005row("TORIHIKIYMD") <> WW_TORIHIKIYMD OrElse
                   T0005row("RECODEKBN") <> WW_RECODEKBN OrElse
                   T0005row("TORICODE") <> WW_TORICODE OrElse
                   T0005row("TODOKECODE") <> WW_TODOKECODE OrElse
                   T0005row("GSHABAN") <> WW_GSHABAN OrElse
                   T0005row("NSHABAN") <> WW_NSHABAN Then

                    PARA01.Value = T0005row("CAMPCODE")
                    PARA02.Value = T0005row("DENKBN")
                    PARA03.Value = T0005row("DENNO")
                    PARA04.Value = T0005row("TORIHIKIYMD")
                    PARA05.Value = T0005row("RECODEKBN")
                    PARA06.Value = T0005row("TORICODE")
                    PARA07.Value = T0005row("TODOKECODE")
                    PARA08.Value = T0005row("GSHABAN")
                    PARA09.Value = T0005row("NSHABAN")

                    PARA11.Value = I_DATENOW
                    PARA12.Value = Master.USERID
                    PARA13.Value = Master.USERTERMID
                    PARA14.Value = C_DEFAULT_YMD

                    SQLcmd.ExecuteNonQuery()

                    'ブレイクキー退避
                    WW_DENKBN = T0005row("DENKBN")
                    WW_DENNO = T0005row("DENNO")
                    WW_TORIHIKIYMD = T0005row("TORIHIKIYMD")
                    WW_RECODEKBN = T0005row("RECODEKBN")
                    WW_TORICODE = T0005row("TORICODE")
                    WW_TODOKECODE = T0005row("TODOKECODE")
                    WW_GSHABAN = T0005row("GSHABAN")
                    WW_NSHABAN = T0005row("NSHABAN")

                End If

            Next

            'CLOSE
            SQLcmd.Dispose()
            SQLcmd = Nothing

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "T0016_TORIHIKI(old) DEL")
            CS0011LOGWRITE.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:T0016_TORIHIKI(old) DEL"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力

            O_RTN = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try

    End Sub

    ''' <summary>
    ''' 日報データ登録
    ''' </summary>
    ''' <param name="I_DATENOW">更新時刻</param>
    ''' <param name="O_RTN">RTNCODE</param>
    ''' <remarks></remarks>
    Protected Sub NippoDATAinsert(ByVal I_DATENOW As Date, ByVal O_RTN As String)

        'Dim cnt As Integer = 0

        'Try

        '    Using SQLcon As SqlConnection = CS0050SESSION.getConnection
        '        SQLcon.Open()       'DataBase接続(Open)

        '        Dim SQLStr As String =
        '                   " INSERT INTO T0016_TORIHIKI                 " _
        '                & "             (CAMPCODE,                      " _
        '                & "              DENKBN,                        " _
        '                & "              DENNO,                         " _
        '                & "              TORIHIKIYMD,                   " _
        '                & "              RECODEKBN,                     " _
        '                & "              TORICODE,                      " _
        '                & "              TODOKECODE,                    " _
        '                & "              GSHABAN,                       " _
        '                & "              NSHABAN,                       " _
        '                & "              UNCHINCODE,                    " _
        '                & "              DETAILNO,                      " _
        '                & "              ENTRYDATE,                     " _
        '                & "              ACTORICODE,                    " _
        '                & "              URIKBN,                        " _
        '                & "              TORIHIKIMANGORG,               " _
        '                & "              TORIHIKIORG,                   " _
        '                & "              SEIKYUSHIHARAIMANGORG,         " _
        '                & "              SEIKYUSHIHARAIORG,             " _
        '                & "              SEIKYUSHIHARAIYM,              " _
        '                & "              URIKEIJYOYMD,                  " _
        '                & "              SEIKYUNO,                      " _
        '                & "              URIPATERNKBN,                  " _
        '                & "              URIPATTERNCODE,                " _
        '                & "              URIAMT,                        " _
        '                & "              URITAXAMT,                     " _
        '                & "              URISEGMENT1,                   " _
        '                & "              URISEGMENT2,                   " _
        '                & "              URISEGMENT3,                   " _
        '                & "              NDEADLINEDAYS,                 " _
        '                & "              JOTSEIKYUKBN,                  " _
        '                & "              SEIKYUOUTYMD,                  " _
        '                & "              NYUKINSITE,                    " _
        '                & "              NYUKINYMD,                     " _
        '                & "              SHIHARAIKEIJYOYMD,             " _
        '                & "              SHIHARAINO,                    " _
        '                & "              SHIHARAIPATERNKBN,             " _
        '                & "              SHIHARAIPATTERNCODE,           " _
        '                & "              SHIHARAIAMT,                   " _
        '                & "              SHIHARAITAXAMT,                " _
        '                & "              SHIHARAISEGMENT1,              " _
        '                & "              SHIHARAISEGMENT2,              " _
        '                & "              SHIHARAISEGMENT3,              " _
        '                & "              GDEADLINEDAYS,                 " _
        '                & "              SEIKYUMATCHYMD,                " _
        '                & "              SHIHARAISITE,                  " _
        '                & "              SHIHARAIYMD,                   " _
        '                & "              BANKCODE,                      " _
        '                & "              SEIKYUKBN,                     " _
        '                & "              NIPPONO,                       " _
        '                & "              ORDERNO,                       " _
        '                & "              SHUKODATE,                     " _
        '                & "              SHUKADATE,                     " _
        '                & "              TODOKEDATE,                    " _
        '                & "              SHUKABASHO,                    " _
        '                & "              SHUKACITIES,                   " _
        '                & "              TODOKECITIES,                  " _
        '                & "              SHARYOTYPEF,                   " _
        '                & "              TSHABANF,                      " _
        '                & "              SHARYOTYPEB,                   " _
        '                & "              TSHABANB,                      " _
        '                & "              SHARYOTYPEB2,                  " _
        '                & "              TSHABANB2,                     " _
        '                & "              SHARYOKBN,                     " _
        '                & "              SHAFUKU,                       " _
        '                & "              TRIPNO,                        " _
        '                & "              DROPNO,                        " _
        '                & "              STAFFSU,                       " _
        '                & "              STAFFCODE,                     " _
        '                & "              SUBSTAFFCODE,                  " _
        '                & "              OILTYPE,                       " _
        '                & "              PRODUCTCODE,                   " _
        '                & "              TUKORYOKBN,                    " _
        '                & "              TUKORYO,                       " _
        '                & "              TRIPSTTIME,                    " _
        '                & "              TRIPENDTIME,                   " _
        '                & "              KYUYU,                         " _
        '                & "              UNCHINDISTANCE,                " _
        '                & "              KEIRYONO,                      " _
        '                & "              JSURYO,                        " _
        '                & "              JTANI,                         " _
        '                & "              UNCHINCALCKBN,                 " _
        '                & "              ROUNDTRIPDISTANCEAUTO,         " _
        '                & "              ROUNDTRIPDISTANCEHAND,         " _
        '                & "              ROUNDTRIPDISTANCE,             " _
        '                & "              UNKOUKAISUAUTO,                " _
        '                & "              UNKOUKAISUHAND,                " _
        '                & "              UNKOUKAISU,                    " _
        '                & "              UNKOUNISSUAUTO,                " _
        '                & "              UNKOUNISSUHAND,                " _
        '                & "              UNKOUNISSU,                    " _
        '                & "              PUBLICHOLIDAYNISSUAUTO,        " _
        '                & "              PUBLICHOLIDAYNISSUHAND,        " _
        '                & "              PUBLICHOLIDAYNISSU,            " _
        '                & "              PUBLICHOLIDAYKADONISSUAUTO,    " _
        '                & "              PUBLICHOLIDAYKADONISSUHAND,    " _
        '                & "              PUBLICHOLIDAYKADONISSU,        " _
        '                & "              NENMATUNEMSHINISSUAUTO,        " _
        '                & "              NENMATUNEMSHINISSUHAND,        " _
        '                & "              NENMATUNEMSHINISSU,            " _
        '                & "              KEIYAKUDAISUAUTO,              " _
        '                & "              KEIYAKUDAISUHAND,              " _
        '                & "              KEIYAKUDAISU,                  " _
        '                & "              SURYOAUTO,                     " _
        '                & "              SURYOHAND,                     " _
        '                & "              SURYO,                         " _
        '                & "              AMTAUTO,                       " _
        '                & "              AMTHAND,                       " _
        '                & "              AMT,                           " _
        '                & "              RELATIONNO,                    " _
        '                & "              DELFLG,                        " _
        '                & "              INITYMD,                       " _
        '                & "              UPDYMD,                        " _
        '                & "              UPDUSER,                       " _
        '                & "              UPDTERMID,                     " _
        '                & "              RECEIVEYMD,                    " _
        '                & "              UPDTIMSTP)                     "
        '                & "      VALUES (@P01,@P02,@P03,@P04,@P05,@P06,@P07,@P08,@P09,@P10,               " _
        '                & "              @P11,@P12,@P13,@P14,@P15,@P16,@P17,@P18,@P19,@P20,               " _
        '                & "              @P21,@P22,@P23,@P24,@P25,@P26,@P27,@P28,@P29,@P30,               " _
        '                & "              @P31,@P32,@P33,@P34,@P35,@P36,@P37,@P38,@P39,@P40,               " _
        '                & "              @P41,@P42,@P43,@P44,@P45,@P46,@P47,@P48,@P49,@P50,               " _
        '                & "              @P51,@P52,@P53,@P54,@P55,@P56,@P57,@P58,@P59,@P60,               " _
        '                & "              @P61,@P62,@P63,@P64,@P65,@P66,@P67,@P68,@P69,@P70,               " _
        '                & "              @P71,@P72,@P73,@P74,@P75,@P76,@P77,@P78,@P79,@P80,               " _
        '                & "              @P81,@P82,@P83,@P84,@P85,@P86,@P87,@P88,@P89,@P90,               " _
        '                & "              @P91,@P92,@P93,@P94,@P95,@P96,@P97,@P98,@P99,@P100,              " _
        '                & "              @P101,@P102,@P103,@P104,@P105,@P106,@P107,@P108,@P109,@P110,     " _
        '                & "              @P111,@P112,@P113,@P114,@P115,@P116,@P117,@P118,@P119,@P120,     " _
        '                & "              @P121,@P122,@P123,@P124,@P125,@P126,@P127,@P128,@P129,@P130,     " _
        '                & "              @P131,@P132,@P133,@P134,@P135,@P136,@P137,@P138,@P139,@P140,     " _
        '                & "              @P141,@P142,@P143,@P144,@P145,@P146,@P147,@P148,@P149,@P150,     " _
        '                & "              @P151,@P152,@P153,@P154,@P155,@P156,@P157,@P158,@P159,@P160,     " _
        '                & "              @P161,@P162,@P163,@P164,@P165,@P166,@P167,@P168,@P169,@P170,     " _
        '                & "              @P171,@P172,@P173,@P174,@P175,@P176,@P177,@P178,@P179,@P180,     " _
        '                & "              @P181,@P182,@P183,@P184,@P185,@P186,@P187,@P188                  " _
        '                & "              );    "

        '        Dim SQLcmd As New SqlCommand(SQLStr, SQLcon)
        '        Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", System.Data.SqlDbType.NVarChar, 20)
        '        Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", System.Data.SqlDbType.NVarChar, 10)
        '        Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@P03", System.Data.SqlDbType.NVarChar, 10)
        '        Dim PARA04 As SqlParameter = SQLcmd.Parameters.Add("@P04", System.Data.SqlDbType.NVarChar, 10)
        '        Dim PARA05 As SqlParameter = SQLcmd.Parameters.Add("@P05", System.Data.SqlDbType.NVarChar, 10)
        '        Dim PARA06 As SqlParameter = SQLcmd.Parameters.Add("@P06", System.Data.SqlDbType.NVarChar, 2)
        '        Dim PARA07 As SqlParameter = SQLcmd.Parameters.Add("@P07", System.Data.SqlDbType.NVarChar, 25)
        '        Dim PARA08 As SqlParameter = SQLcmd.Parameters.Add("@P08", System.Data.SqlDbType.NVarChar, 20)
        '        Dim PARA09 As SqlParameter = SQLcmd.Parameters.Add("@P09", System.Data.SqlDbType.NVarChar, 20)
        '        Dim PARA10 As SqlParameter = SQLcmd.Parameters.Add("@P10", System.Data.SqlDbType.DateTime)
        '        Dim PARA11 As SqlParameter = SQLcmd.Parameters.Add("@P11", System.Data.SqlDbType.DateTime)
        '        Dim PARA12 As SqlParameter = SQLcmd.Parameters.Add("@P12", System.Data.SqlDbType.DateTime)
        '        Dim PARA13 As SqlParameter = SQLcmd.Parameters.Add("@P13", System.Data.SqlDbType.NVarChar, 20)
        '        Dim PARA14 As SqlParameter = SQLcmd.Parameters.Add("@P14", System.Data.SqlDbType.NVarChar, 20)
        '        Dim PARA15 As SqlParameter = SQLcmd.Parameters.Add("@P15", System.Data.SqlDbType.NVarChar, 20)
        '        Dim PARA16 As SqlParameter = SQLcmd.Parameters.Add("@P16", System.Data.SqlDbType.NVarChar, 1)
        '        Dim PARA17 As SqlParameter = SQLcmd.Parameters.Add("@P17", System.Data.SqlDbType.Decimal)
        '        Dim PARA18 As SqlParameter = SQLcmd.Parameters.Add("@P18", System.Data.SqlDbType.NVarChar, 20)
        '        Dim PARA19 As SqlParameter = SQLcmd.Parameters.Add("@P19", System.Data.SqlDbType.NVarChar, 20)
        '        Dim PARA20 As SqlParameter = SQLcmd.Parameters.Add("@P20", System.Data.SqlDbType.DateTime)
        '        Dim PARA21 As SqlParameter = SQLcmd.Parameters.Add("@P21", System.Data.SqlDbType.NVarChar, 20)
        '        Dim PARA22 As SqlParameter = SQLcmd.Parameters.Add("@P22", System.Data.SqlDbType.NVarChar, 20)
        '        Dim PARA23 As SqlParameter = SQLcmd.Parameters.Add("@P23", System.Data.SqlDbType.NVarChar, 20)
        '        Dim PARA24 As SqlParameter = SQLcmd.Parameters.Add("@P24", System.Data.SqlDbType.NVarChar, 20)
        '        Dim PARA25 As SqlParameter = SQLcmd.Parameters.Add("@P25", System.Data.SqlDbType.Decimal)
        '        Dim PARA26 As SqlParameter = SQLcmd.Parameters.Add("@P26", System.Data.SqlDbType.Int)
        '        Dim PARA27 As SqlParameter = SQLcmd.Parameters.Add("@P27", System.Data.SqlDbType.NVarChar, 50)
        '        Dim PARA28 As SqlParameter = SQLcmd.Parameters.Add("@P28", System.Data.SqlDbType.NVarChar, 50)
        '        Dim PARA29 As SqlParameter = SQLcmd.Parameters.Add("@P29", System.Data.SqlDbType.NVarChar, 50)
        '        Dim PARA30 As SqlParameter = SQLcmd.Parameters.Add("@P30", System.Data.SqlDbType.NVarChar, 50)
        '        Dim PARA31 As SqlParameter = SQLcmd.Parameters.Add("@P31", System.Data.SqlDbType.NVarChar, 50)
        '        Dim PARA32 As SqlParameter = SQLcmd.Parameters.Add("@P32", System.Data.SqlDbType.NVarChar, 50)
        '        Dim PARA33 As SqlParameter = SQLcmd.Parameters.Add("@P33", System.Data.SqlDbType.NVarChar, 1)
        '        Dim PARA34 As SqlParameter = SQLcmd.Parameters.Add("@P34", System.Data.SqlDbType.DateTime)
        '        Dim PARA35 As SqlParameter = SQLcmd.Parameters.Add("@P35", System.Data.SqlDbType.DateTime)
        '        Dim PARA36 As SqlParameter = SQLcmd.Parameters.Add("@P36", System.Data.SqlDbType.NVarChar, 20)
        '        Dim PARA37 As SqlParameter = SQLcmd.Parameters.Add("@P37", System.Data.SqlDbType.NVarChar, 30)
        '        Dim PARA38 As SqlParameter = SQLcmd.Parameters.Add("@P38", System.Data.SqlDbType.DateTime)
        '        Dim PARA39 As SqlParameter = SQLcmd.Parameters.Add("@P39", System.Data.SqlDbType.DateTime)
        '        Dim PARA40 As SqlParameter = SQLcmd.Parameters.Add("@P40", System.Data.SqlDbType.NVarChar, 1)
        '        Dim PARA41 As SqlParameter = SQLcmd.Parameters.Add("@P41", System.Data.SqlDbType.NVarChar, 20)
        '        Dim PARA42 As SqlParameter = SQLcmd.Parameters.Add("@P42", System.Data.SqlDbType.NVarChar, 1)
        '        Dim PARA43 As SqlParameter = SQLcmd.Parameters.Add("@P43", System.Data.SqlDbType.NVarChar, 20)
        '        Dim PARA44 As SqlParameter = SQLcmd.Parameters.Add("@P44", System.Data.SqlDbType.NVarChar, 1)
        '        Dim PARA45 As SqlParameter = SQLcmd.Parameters.Add("@P45", System.Data.SqlDbType.NVarChar, 20)
        '        Dim PARA46 As SqlParameter = SQLcmd.Parameters.Add("@P46", System.Data.SqlDbType.NVarChar, 20)
        '        Dim PARA47 As SqlParameter = SQLcmd.Parameters.Add("@P47", System.Data.SqlDbType.NVarChar, 20)
        '        Dim PARA48 As SqlParameter = SQLcmd.Parameters.Add("@P48", System.Data.SqlDbType.NVarChar, 1)

        '        For Each T0005row As DataRow In T0005tbl.Rows

        '            '削除は対象外
        '            If T0005row("DELFLG") = C_DELETE_FLG.DELETE AndAlso T0005row("TIMSTP") = "0" Then
        '                Continue For
        '            End If

        '            PARA01.Value = T0005row("CAMPCODE")                           '会社コード(CAMPCODE)
        '            PARA02.Value = T0005row("ORDERNO").PadLeft(7, "0")            '受注番号(ORDERNO)
        '            PARA03.Value = T0005row("DETAILNO").PadLeft(3, "0")           '明細№(DETAILNO)
        '            PARA04.Value = T0005row("TRIPNO").PadLeft(3, "0")             'トリップ(TRIPNO)
        '            PARA05.Value = T0005row("DROPNO").PadLeft(3, "0")             'ドロップ(DROPNO)
        '            PARA07.Value = I_DATENOW.ToString("yyyyMMddHHmmssfff")       'エントリー日時(ENTRYDATE)
        '            PARA08.Value = T0005row("TORICODE")                           '取引先コード(TORICODE)
        '            If T0005row("L1SHUKODATE") = "" Then                          '出庫日(SHUKODATE)
        '                PARA10.Value = "2000/01/01"
        '            Else
        '                PARA10.Value = RTrim(T0005row("L1SHUKODATE"))
        '            End If
        '            If T0005row("KIKODATE") = "" Then                             '帰庫日(KIKODATE)
        '                PARA11.Value = "2000/01/01"
        '            Else
        '                PARA11.Value = RTrim(T0005row("KIKODATE"))
        '            End If
        '            If T0005row("SHUKADATE") = "" Then                            '出荷日(SHUKADATE)
        '                PARA12.Value = "2000/01/01"
        '            Else
        '                PARA12.Value = RTrim(T0005row("SHUKADATE"))
        '            End If
        '            PARA13.Value = T0005row("SHIPORG")                            '出荷部署(SHIPORG)
        '            PARA14.Value = T0005row("SHUKABASHO")                         '出荷場所(SHUKABASHO)
        '            PARA15.Value = T0005row("GSHABAN")                            '業務車番(GSHABAN)
        '            PARA16.Value = "1"                                            '両目(RYOME)
        '            If String.IsNullOrWhiteSpace(RTrim(T0005row("SHAFUKU"))) Then '車腹（積載量）(SHAFUKU)
        '                PARA17.Value = 0.0
        '            Else
        '                PARA17.Value = CType(T0005row("SHAFUKU"), Double)
        '            End If
        '            PARA18.Value = T0005row("STAFFCODE")                          '乗務員コード(STAFFCODE)
        '            PARA19.Value = T0005row("SUBSTAFFCODE")                       '副乗務員コード(SUBSTAFFCODE)
        '            If RTrim(T0005row("TODOKEDATE")) = "" Then                    '届日(TODOKEDATE)
        '                PARA20.Value = "2000/01/01"
        '            Else
        '                PARA20.Value = RTrim(T0005row("TODOKEDATE"))
        '            End If
        '            PARA21.Value = T0005row("TODOKECODE")                         '届先コード(TODOKECODE)
        '            PARA24.Value = ""                                             'コンテナ番号(CONTNO)
        '            PARA27.Value = ""                                             '備考１(REMARKS1)
        '            PARA28.Value = ""                                             '備考２(REMARKS2)
        '            PARA29.Value = ""                                             '備考３(REMARKS3)
        '            PARA30.Value = ""                                             '備考４(REMARKS4)
        '            PARA31.Value = ""                                             '備考５(REMARKS5)
        '            PARA32.Value = ""                                             '備考６(REMARKS6)
        '            PARA33.Value = C_DELETE_FLG.ALIVE                             '削除フラグ(DELFLG)
        '            PARA34.Value = I_DATENOW                                      '登録年月日(INITYMD)
        '            PARA35.Value = I_DATENOW                                      '更新年月日(UPDYMD)
        '            PARA36.Value = Master.USERID                                  '更新ユーザＩＤ(UPDUSER)
        '            PARA37.Value = Master.USERTERMID                              '更新端末(UPDTERMID)
        '            PARA38.Value = C_DEFAULT_YMD                                  '集信日時(RECEIVEYMD)

        '            '売上区分が１の場合、出荷日　２の場合、届日
        '            If T0005row("URIKBN") = "1" Then
        '                If RTrim(T0005row("SHUKADATE")) = "" Then
        '                    PARA39.Value = "2000/01/01"
        '                Else
        '                    PARA39.Value = RTrim(T0005row("SHUKADATE"))               '基準日(KIJUNDATE)
        '                End If
        '            ElseIf T0005row("URIKBN") = "2" Then
        '                If RTrim(T0005row("TODOKEDATE")) = "" Then
        '                    PARA39.Value = "2000/01/01"
        '                Else
        '                    PARA39.Value = RTrim(T0005row("TODOKEDATE"))              '基準日(KIJUNDATE)
        '                End If
        '            Else
        '                PARA39.Value = "2000/01/01"
        '            End If
        '            PARA40.Value = T0005row("SHARYOTYPEF")                        '統一車番前(SHARYOTYPEF)
        '            PARA41.Value = T0005row("TSHABANF")                           '統一車番前(TSHABANF)
        '            PARA42.Value = T0005row("SHARYOTYPEB")                        '統一車番後(SHARYOTYPEB)
        '            PARA43.Value = T0005row("TSHABANB")                           '統一車番後(TSHABANB)
        '            PARA44.Value = T0005row("SHARYOTYPEB2")                       '統一車番後2(SHARYOTYPEB2)
        '            PARA45.Value = T0005row("TSHABANB2")                          '統一車番後2(TSHABANB2)
        '            PARA48.Value = "2"                                            '実績区分(JISSEKIKBN)

        '            For index = 1 To 8

        '                Select Case index

        '                    Case 1

        '                        If String.IsNullOrEmpty(T0005row("OILTYPE1")) Then
        '                            Continue For
        '                        End If

        '                        PARA06.Value = index.ToString("00")                           '枝番(SEQ)
        '                        PARA09.Value = T0005row("OILTYPE1")                           '油種(OILTYPE)
        '                        PARA22.Value = T0005row("PRODUCT11")                          '品名１(PRODUCT1)
        '                        PARA23.Value = T0005row("PRODUCT21")                          '品名２(PRODUCT2)

        '                        If String.IsNullOrWhiteSpace(RTrim(T0005row("SURYO1"))) Then
        '                            PARA25.Value = 0.0
        '                            PARA26.Value = 0
        '                        Else
        '                            PARA25.Value = CType(T0005row("SURYO1"), Double)          '配送実績数量(JSURYO)
        '                            PARA26.Value = 1                                          '配送実績台数(JDAISU)
        '                        End If

        '                        PARA46.Value = T0005row("STANI1")                             '配送実績単位(STANI)
        '                        PARA47.Value = T0005row("PRODUCTCODE1")                       '品名コード(PRODUCTCODE)

        '                    Case 2

        '                        If String.IsNullOrEmpty(T0005row("OILTYPE2")) Then
        '                            Continue For
        '                        End If

        '                        PARA06.Value = index.ToString("00")                           '枝番(SEQ)
        '                        PARA09.Value = T0005row("OILTYPE2")                           '油種(OILTYPE)
        '                        PARA22.Value = T0005row("PRODUCT12")                          '品名１(PRODUCT1)
        '                        PARA23.Value = T0005row("PRODUCT22")                          '品名２(PRODUCT2)

        '                        If String.IsNullOrWhiteSpace(RTrim(T0005row("SURYO2"))) Then
        '                            PARA25.Value = 0.0
        '                            PARA26.Value = 0
        '                        Else
        '                            PARA25.Value = CType(T0005row("SURYO2"), Double)          '配送実績数量(JSURYO)
        '                            PARA26.Value = 1                                          '配送実績台数(JDAISU)
        '                        End If

        '                        PARA46.Value = T0005row("STANI2")                             '配送実績単位(STANI)
        '                        PARA47.Value = T0005row("PRODUCTCODE2")                       '品名コード(PRODUCTCODE)

        '                    Case 3

        '                        If String.IsNullOrEmpty(T0005row("OILTYPE3")) Then
        '                            Continue For
        '                        End If

        '                        PARA06.Value = index.ToString("00")                           '枝番(SEQ)
        '                        PARA09.Value = T0005row("OILTYPE3")                           '油種(OILTYPE)
        '                        PARA22.Value = T0005row("PRODUCT13")                          '品名１(PRODUCT1)
        '                        PARA23.Value = T0005row("PRODUCT23")                          '品名２(PRODUCT2)

        '                        If String.IsNullOrWhiteSpace(RTrim(T0005row("SURYO3"))) Then
        '                            PARA25.Value = 0.0
        '                            PARA26.Value = 0
        '                        Else
        '                            PARA25.Value = CType(T0005row("SURYO3"), Double)          '配送実績数量(JSURYO)
        '                            PARA26.Value = 1                                          '配送実績台数(JDAISU)
        '                        End If

        '                        PARA46.Value = T0005row("STANI3")                             '配送実績単位(STANI)
        '                        PARA47.Value = T0005row("PRODUCTCODE3")                       '品名コード(PRODUCTCODE)

        '                    Case 4

        '                        If String.IsNullOrEmpty(T0005row("OILTYPE4")) Then
        '                            Continue For
        '                        End If

        '                        PARA06.Value = index.ToString("00")                           '枝番(SEQ)
        '                        PARA09.Value = T0005row("OILTYPE4")                           '油種(OILTYPE)
        '                        PARA22.Value = T0005row("PRODUCT14")                          '品名１(PRODUCT1)
        '                        PARA23.Value = T0005row("PRODUCT24")                          '品名２(PRODUCT2)

        '                        If String.IsNullOrWhiteSpace(RTrim(T0005row("SURYO4"))) Then
        '                            PARA25.Value = 0.0
        '                            PARA26.Value = 0
        '                        Else
        '                            PARA25.Value = CType(T0005row("SURYO4"), Double)          '配送実績数量(JSURYO)
        '                            PARA26.Value = 1                                          '配送実績台数(JDAISU)
        '                        End If

        '                        PARA46.Value = T0005row("STANI4")                             '配送実績単位(STANI)
        '                        PARA47.Value = T0005row("PRODUCTCODE4")                       '品名コード(PRODUCTCODE)

        '                    Case 5

        '                        If String.IsNullOrEmpty(T0005row("OILTYPE5")) Then
        '                            Continue For
        '                        End If

        '                        PARA06.Value = index.ToString("00")                           '枝番(SEQ)
        '                        PARA09.Value = T0005row("OILTYPE5")                           '油種(OILTYPE)
        '                        PARA22.Value = T0005row("PRODUCT15")                          '品名１(PRODUCT1)
        '                        PARA23.Value = T0005row("PRODUCT25")                          '品名２(PRODUCT2)

        '                        If String.IsNullOrWhiteSpace(RTrim(T0005row("SURYO5"))) Then
        '                            PARA25.Value = 0.0
        '                            PARA26.Value = 0
        '                        Else
        '                            PARA25.Value = CType(T0005row("SURYO5"), Double)          '配送実績数量(JSURYO)
        '                            PARA26.Value = 1                                          '配送実績台数(JDAISU)
        '                        End If

        '                        PARA46.Value = T0005row("STANI5")                             '配送実績単位(STANI)
        '                        PARA47.Value = T0005row("PRODUCTCODE5")                       '品名コード(PRODUCTCODE)

        '                    Case 6

        '                        If String.IsNullOrEmpty(T0005row("OILTYPE6")) Then
        '                            Continue For
        '                        End If

        '                        PARA06.Value = index.ToString("00")                           '枝番(SEQ)
        '                        PARA09.Value = T0005row("OILTYPE6")                           '油種(OILTYPE)
        '                        PARA22.Value = T0005row("PRODUCT16")                          '品名１(PRODUCT1)
        '                        PARA23.Value = T0005row("PRODUCT26")                          '品名２(PRODUCT2)

        '                        If String.IsNullOrWhiteSpace(RTrim(T0005row("SURYO6"))) Then
        '                            PARA25.Value = 0.0
        '                            PARA26.Value = 0
        '                        Else
        '                            PARA25.Value = CType(T0005row("SURYO6"), Double)          '配送実績数量(JSURYO)
        '                            PARA26.Value = 1                                          '配送実績台数(JDAISU)
        '                        End If

        '                        PARA46.Value = T0005row("STANI6")                             '配送実績単位(STANI)
        '                        PARA47.Value = T0005row("PRODUCTCODE6")                       '品名コード(PRODUCTCODE)

        '                    Case 7

        '                        If String.IsNullOrEmpty(T0005row("OILTYPE7")) Then
        '                            Continue For
        '                        End If

        '                        PARA06.Value = index.ToString("00")                           '枝番(SEQ)
        '                        PARA09.Value = T0005row("OILTYPE7")                           '油種(OILTYPE)
        '                        PARA22.Value = T0005row("PRODUCT17")                          '品名１(PRODUCT1)
        '                        PARA23.Value = T0005row("PRODUCT27")                          '品名２(PRODUCT2)

        '                        If String.IsNullOrWhiteSpace(RTrim(T0005row("SURYO7"))) Then
        '                            PARA25.Value = 0.0
        '                            PARA26.Value = 0
        '                        Else
        '                            PARA25.Value = CType(T0005row("SURYO7"), Double)          '配送実績数量(JSURYO)
        '                            PARA26.Value = 1                                          '配送実績台数(JDAISU)
        '                        End If

        '                        PARA46.Value = T0005row("STANI7")                             '配送実績単位(STANI)
        '                        PARA47.Value = T0005row("PRODUCTCODE7")                       '品名コード(PRODUCTCODE)

        '                    Case 8

        '                        If String.IsNullOrEmpty(T0005row("OILTYPE8")) Then
        '                            Continue For
        '                        End If

        '                        PARA06.Value = index.ToString("00")                           '枝番(SEQ)
        '                        PARA09.Value = T0005row("OILTYPE8")                           '油種(OILTYPE)
        '                        PARA22.Value = T0005row("PRODUCT18")                          '品名１(PRODUCT1)
        '                        PARA23.Value = T0005row("PRODUCT28")                          '品名２(PRODUCT2)

        '                        If String.IsNullOrWhiteSpace(RTrim(T0005row("SURYO8"))) Then
        '                            PARA25.Value = 0.0
        '                            PARA26.Value = 0
        '                        Else
        '                            PARA25.Value = CType(T0005row("SURYO8"), Double)          '配送実績数量(JSURYO)
        '                            PARA26.Value = 1                                          '配送実績台数(JDAISU)
        '                        End If

        '                        PARA46.Value = T0005row("STANI8")                             '配送実績単位(STANI)
        '                        PARA47.Value = T0005row("PRODUCTCODE8")                       '品名コード(PRODUCTCODE)

        '                End Select























        '                SQLcmd.CommandTimeout = 300
        '                SQLcmd.ExecuteNonQuery()

        '            Next

        '        Next

        '        'CLOSE
        '        SQLcmd.Dispose()
        '        SQLcmd = Nothing

        '    End Using

        'Catch ex As Exception
        '    Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "T0016_TORIHIKI INSERT")
        '    CS0011LOGWRITE.INFSUBCLASS = "MAIN"                         'SUBクラス名
        '    CS0011LOGWRITE.INFPOSI = "DB:T0016_TORIHIKI INSERT"         '
        '    CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                 '
        '    CS0011LOGWRITE.TEXT = ex.ToString()
        '    CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
        '    CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力

        '    O_RTN = C_MESSAGE_NO.DB_ERROR
        '    Exit Sub

        'End Try

    End Sub


    ''' <summary>
    ''' 用車実績ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonSupplJisski()


    End Sub


    ''' <summary>
    ''' 一覧絞り込みボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonExtract_Click()

        ''○入力値チェック
        'Dim WW_LINECNT As Integer

        ''○画面表示データ復元
        'Master.RecoverTable(T00016tbl)

        ''○絞り込み操作（GridView明細Hidden設定）
        'For Each row In T00016tbl.Rows

        '    '削除データは対象外
        '    If row("DELFLG") = C_DELETE_FLG.DELETE Then Continue For

        '    row("HIDDEN") = 1

        '    '行番号が相違の場合は絞込判定対象、同一の場合は非表示設定
        '    If row("LINECNT") <> WW_LINECNT Then
        '        WW_LINECNT = row("LINECNT")

        '        'オブジェクト　グループコード　絞込判定
        '        If (WF_SELTODOKESAKI.Text = "") Then
        '            row("HIDDEN") = 0
        '        End If

        '        If (WF_SELTODOKESAKI.Text <> "") Then
        '            If row("TORICODE") = WF_SELTODOKESAKI.Text Then
        '                row("HIDDEN") = 0
        '            End If
        '        End If
        '    End If

        'Next

        ''○画面表示データ保存
        'Master.SaveTable(T00016tbl)

        ''画面先頭を表示
        'WF_GridPosition.Text = "1"

        ''○メッセージ表示
        'Master.output(C_MESSAGE_NO.DATA_FILTER_SUCCESSFUL, C_MESSAGE_TYPE.INF)

        ''○カーソル設定
        'WF_FIELD.Value = "WF_SELTODOKESAKI"
        'WF_SELTODOKESAKI.Focus()

    End Sub

    ''' <summary>
    ''' DB更新ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonUPDATE_Click()

        ''○画面表示データ復元
        'Master.RecoverTable(T00016tbl)


        ''■■■ DB更新 ■■■

        'Dim WW_DATENOW As Date = Date.Now
        'Dim SQLcon = CS0050SESSION.getConnection
        'SQLcon.Open()

        '' ***  T00016UPDtbl更新データ（画面表示受注+画面非表示受注）作成　＆　タイムスタンプチェック処理
        'DBupdate_T00016UPDtblget(WW_DUMMY)

        '' ***  T00016tbl関連データ削除
        'DBupdate_T16DELETE(WW_DATENOW, WW_ERRCODE)

        '' ***  T00016tbl追加
        'DBupdate_T16INSERT(WW_DATENOW, WW_ERRCODE)

        ''サマリ処理
        ''SUMMRY_SET()

        ''○画面表示データ保存
        'Master.SaveTable(T00016tbl)
        ''○メッセージ表示
        'Master.output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF)

        ''カーソル設定
        'WF_FIELD.Value = "WF_SELTODOKESAKI"
        'WF_SELTODOKESAKI.Focus()

    End Sub

    ''' <summary>
    ''' ダウンロードボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonCSV_Click()

        ''○画面表示データ復元
        'Master.RecoverTable(T00016tbl)

        ''削除データを除外
        'CS0026TBLSORTget.TABLE = T00016tbl
        'CS0026TBLSORTget.SORTING = "LINECNT ASC , SEQ ASC"
        'CS0026TBLSORTget.FILTER = "DELFLG <> '1'"
        'CS0026TBLSORTget.Sort(T00016tbl)

        ''○ 帳票出力dll Interface
        'CS0030REPORT.CAMPCODE = work.WF_SEL_CAMPCODE.Text           '会社コード
        'CS0030REPORT.PROFID = Master.PROF_REPORT                    'プロファイルID
        'CS0030REPORT.MAPID = Master.MAPID                           'PARAM01:画面ID
        'CS0030REPORT.REPORTID = rightview.getReportId()             'PARAM02:帳票ID
        'CS0030REPORT.FILEtyp = "XLSX"                               'PARAM03:出力ファイル形式
        'CS0030REPORT.TBLDATA = T00016tbl                            'PARAM04:データ参照tabledata
        'CS0030REPORT.CS0030REPORT()

        'If isNormal(CS0030REPORT.ERR) Then
        'Else
        '    Master.output(CS0030REPORT.ERR, C_MESSAGE_TYPE.ABORT, "CS0030REPORT")
        '    Exit Sub
        'End If

        ''○帳票部署データリスト追加
        'Dim addReport As AddReportOrgData = New AddReportOrgData() With {
        '    .CAMPCODE = work.WF_SEL_CAMPCODE.Text,
        '    .UORG = work.WF_SEL_SHIPORG.Text,
        '    .ROLECODE = Master.ROLE_ORG,
        '    .FILEPATH = CS0030REPORT.FILEpath,
        '    .SHEETNAME = "リスト"
        '}
        'addReport.AddOrgData()
        'If isNormal(addReport.ERR) Then
        'Else
        '    'エラーでも継続
        '    Master.Output(addReport.ERR, C_MESSAGE_TYPE.ABORT, "AddReport")
        'End If

        ''別画面でExcelを表示
        'WF_PrintURL.Value = CS0030REPORT.URL
        'ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_ExcelPrint()", True)

    End Sub

    ''' <summary>
    ''' 終了ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonEND_Click()
        '画面遷移実行
        Master.transitionPrevPage()

    End Sub

    ''' <summary>
    ''' 先頭頁移動ボタン押下
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonFIRST_Click()

        Select Case WF_DetailMView.ActiveViewIndex
            Case 0
                '○合計(社内)タブ表示データ復元
                Master.RecoverTable(T00016tbl_tab1, work.WF_SEL_INPTAB1TBL.Text)
            Case 1
                '○合計(請求)タブ表示データ復元
                Master.RecoverTable(T00016tbl_tab2, work.WF_SEL_INPTAB2TBL.Text)
            Case 2
                '○明細(金額)タブ表示データ復元
                Master.RecoverTable(T00016tbl_tab3, work.WF_SEL_INPTAB3TBL.Text)
            Case 3
                '○明細(数量)タブ表示データ復元
                Master.RecoverTable(T00016tbl_tab4, work.WF_SEL_INPTAB4TBL.Text)
        End Select

        '○先頭頁に移動
        WF_GridPosition.Text = "1"
    End Sub
    ''' <summary>
    ''' 最終頁ボタン処理 
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonLAST_Click()

        '○ソート
        Dim WW_TBLview As DataView
        WW_TBLview = New DataView()

        Select Case WF_DetailMView.ActiveViewIndex
            Case 0
                '○合計(社内)タブ表示データ復元
                Master.RecoverTable(T00016tbl_tab1, work.WF_SEL_INPTAB1TBL.Text)
                WW_TBLview = New DataView(T00016tbl_tab1)
            Case 1
                '○合計(請求)タブ表示データ復元
                Master.RecoverTable(T00016tbl_tab2, work.WF_SEL_INPTAB2TBL.Text)
                WW_TBLview = New DataView(T00016tbl_tab2)
            Case 2
                '○明細(金額)タブ表示データ復元
                Master.RecoverTable(T00016tbl_tab3, work.WF_SEL_INPTAB3TBL.Text)
                WW_TBLview = New DataView(T00016tbl_tab3)
            Case 3
                '○明細(数量)タブ表示データ復元
                Master.RecoverTable(T00016tbl_tab4, work.WF_SEL_INPTAB4TBL.Text)
                WW_TBLview = New DataView(T00016tbl_tab4)
        End Select

        WW_TBLview.RowFilter = "HIDDEN= '0'"

        '最終頁に移動
        If WW_TBLview.Count Mod CONST_SCROLLROWCOUNT = 0 Then
            WF_GridPosition.Text = WW_TBLview.Count - (WW_TBLview.Count Mod CONST_SCROLLROWCOUNT)
        Else
            WF_GridPosition.Text = WW_TBLview.Count - (WW_TBLview.Count Mod CONST_SCROLLROWCOUNT) + 1
        End If
    End Sub

    ''' <summary>
    ''' 一覧画面-マウスホイール時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_GRID_Scrole()

        Select Case WF_DetailMView.ActiveViewIndex
            Case 0
                '○合計(社内)タブ表示データ復元
                Master.RecoverTable(T00016tbl_tab1)
            Case 1
                '○合計(請求)タブ表示データ復元
                Master.RecoverTable(T00016tbl_tab2)
            Case 2
                '○明細(金額)タブ表示データ復元
                Master.RecoverTable(T00016tbl_tab3)
            Case 3
                '○明細(数量)タブ表示データ復元
                Master.RecoverTable(T00016tbl_tab4)
        End Select

    End Sub

    ' ******************************************************************************
    ' ***  共通処理                                                              ***　
    ' ******************************************************************************

    '''' <summary>
    '''' GridViewサマリ処理
    '''' </summary>
    '''' <remarks></remarks>
    'Protected Sub SUMMRY_SET()

    '    Dim JSURYO_SUM As Decimal = 0
    '    Dim JDAISU_SUM As Long = 0

    '    CS0026TBLSORTget.TABLE = T00016tbl
    '    CS0026TBLSORTget.SORTING = "TORICODE ,OILTYPE ,KIJUNDATE ,SHIPORG ,SHUKODATE ,GSHABAN ,RYOME ,TRIPNO ,DROPNO ,SEQ"
    '    CS0026TBLSORTget.FILTER = ""
    '    CS0026TBLSORTget.Sort(T00016tbl)

    '    '最終行から初回行へループ
    '    For i As Integer = 0 To T00016tbl.Rows.Count - 1

    '        Dim T00016row = T00016tbl.Rows(i)

    '        If T00016row("SEQ") = "01" And T00016row("HIDDEN") <> "1" Then
    '            JSURYO_SUM = 0
    '            JDAISU_SUM = 0

    '            Dim findSeq As Boolean = False
    '            For j As Integer = i To T00016tbl.Rows.Count - 1
    '                If CompareOrder(T00016row, T00016tbl.Rows(j)) Then
    '                    If T00016tbl.Rows(j)("DELFLG") <> C_DELETE_FLG.DELETE Then
    '                        '同一トリップが発生したら２件目以降は非表示
    '                        If findSeq = True Then
    '                            T00016tbl.Rows(j)("HIDDEN") = "1"
    '                        ElseIf T00016tbl.Rows(j)("SEQ") = "01" Then
    '                            findSeq = True
    '                        End If
    '                        Dim wkVal As Double
    '                        If Double.TryParse(T00016tbl.Rows(j)("JSURYO"), wkVal) Then
    '                            JSURYO_SUM += wkVal
    '                        End If

    '                        JDAISU_SUM = 1
    '                    End If
    '                Else
    '                    Exit For
    '                End If

    '            Next

    '            '表示行にサマリ結果を反映
    '            T00016row("JSURYO_SUM") = JSURYO_SUM.ToString("0.000")
    '            T00016row("JDAISU_SUM") = JDAISU_SUM.ToString("0")
    '            T00016row("HIDDEN") = 0   '0:表示

    '        Else
    '            T00016row("HIDDEN") = 1   '1:非表示
    '        End If

    '    Next

    'End Sub


    ''' <summary>
    ''' LeftBox項目名称設定
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub CODENAME_set(ByRef T00016row As DataRow)


        '○名称付与

        '会社名称
        T00016row("CAMPCODENAME") = ""
        If Not IsDBNull(T00016row("CAMPCODE")) Then
            CODENAME_get("CAMPCODE", T00016row("CAMPCODE"), T00016row("CAMPCODENAME"), WW_DUMMY)
        End If

        '取引先名称
        T00016row("TORICODENAME") = ""
        If Not IsDBNull(T00016row("TORICODE")) Then
            CODENAME_get("TORICODE", T00016row("TORICODE"), T00016row("TORICODENAME"), WW_DUMMY)
        End If

        '出荷場所名称
        T00016row("SHUKABASHONAME") = ""
        If Not IsDBNull(T00016row("SHUKABASHO")) Then
            CODENAME_get("SHUKABASHO", T00016row("SHUKABASHO"), T00016row("SHUKABASHONAME"), WW_DUMMY)
        End If

        '届先名称
        T00016row("TODOKECODENAME") = ""
        If Not IsDBNull(T00016row("TODOKECODE")) Then
            CODENAME_get("TODOKECODE", T00016row("TODOKECODE"), T00016row("TODOKECODENAME"), WW_DUMMY)
        End If

        ''荷主車番名称
        'T00016row("NSHABANNAME") = ""
        'If Not IsDBNull(T00016row("NSHABAN")) Then
        'CODENAME_get("NSHABAN", T00016row("NSHABAN"), T00016row("NSHABANNAME"), WW_DUMMY)
        'End If

        ''車複名称
        'T00016row("SHAFUKUNAME") = ""
        'If Not IsDBNull(T00016row("SHAFUKU")) Then
        'CODENAME_get("SHAFUKU", T00016row("SHAFUKU"), T00016row("SHAFUKUNAME"), WW_DUMMY)
        'End If

        ''分類名称
        'T00016row("BUNRUINAME") = ""
        'If Not IsDBNull(T00016row("BUNRUI")) Then
        'CODENAME_get("BUNRUI", T00016row("BUNRUI"), T00016row("BUNRUINAME"), WW_DUMMY)
        'End If

        ''操作名称
        'T00016row("SOUSANAME") = ""
        'If Not IsDBNull(T00016row("SOUSA")) Then
        'CODENAME_get("SOUSA", T00016row("SOUSA"), T00016row("SOUSANAME"), WW_DUMMY)
        'End If

        '出荷部署名称
        T00016row("TORIHIKIORGNAME") = ""
        If Not IsDBNull(T00016row("TORIHIKIORG")) Then
            CODENAME_get("TORIHIKIORG", T00016row("TORIHIKIORG"), T00016row("TORIHIKIORGNAME"), WW_DUMMY)
        End If

        '届先追加情報
        If Not IsDBNull(T00016row("TODOKECODE")) Then
            Dim datTodoke As JOT_MASTER.TODOKESAKI = JOTMASTER.GetTodoke(T00016row("TODOKECODE"))
            If Not IsNothing(datTodoke) AndAlso Not IsNothing(datTodoke.TODOKECODE) Then
                T00016row("ADDR") = datTodoke.ADDR                          '住所
                T00016row("NOTES1") = datTodoke.NOTES1                      '特定要件１
                T00016row("NOTES2") = datTodoke.NOTES2                      '特定要件２
                T00016row("NOTES3") = datTodoke.NOTES3                      '特定要件３
                T00016row("NOTES4") = datTodoke.NOTES4                      '特定要件４
                T00016row("NOTES5") = datTodoke.NOTES5                      '特定要件５
                T00016row("NOTES6") = datTodoke.NOTES6                      '特定要件６
                T00016row("NOTES7") = datTodoke.NOTES7                      '特定要件７
                T00016row("NOTES8") = datTodoke.NOTES8                      '特定要件８
                T00016row("NOTES9") = datTodoke.NOTES9                      '特定要件９
                T00016row("NOTES10") = datTodoke.NOTES10                    '特定要件１０
            End If
        End If

        ''車両追加情報
        'For i As Integer = 0 To WF_ListGSHABAN.Items.Count - 1
        '    If WF_ListGSHABAN.Items(i).Value = T00016row("GSHABAN") Then
        '        If Val(T00016row("SHAFUKU")) = 0 Then
        '            T00016row("SHAFUKU") = WF_ListSHAFUKU.Items(i).Value                  'List車腹
        '        End If
        '        T00016row("SHARYOTYPEF") = Mid(WF_ListTSHABANF.Items(i).Value, 1, 1)  'List統一車番（前）
        '        T00016row("TSHABANF") = Mid(WF_ListTSHABANF.Items(i).Value, 2, 19)    'List統一車番（前）
        '        T00016row("SHARYOTYPEB") = Mid(WF_ListTSHABANB.Items(i).Value, 1, 1)  'List統一車番（後）
        '        T00016row("TSHABANB") = Mid(WF_ListTSHABANB.Items(i).Value, 2, 19)    'List統一車番（後）
        '        T00016row("SHARYOTYPEB2") = Mid(WF_ListTSHABANB2.Items(i).Value, 1, 1) 'List統一車番（後）２
        '        T00016row("TSHABANB2") = Mid(WF_ListTSHABANB2.Items(i).Value, 2, 19)   'List統一車番（後）２
        '        T00016row("SHARYOINFO1") = WF_ListSHARYOINFO1.Items(i).Value          'List車両情報１
        '        T00016row("SHARYOINFO2") = WF_ListSHARYOINFO2.Items(i).Value          'List車両情報２
        '        T00016row("SHARYOINFO3") = WF_ListSHARYOINFO3.Items(i).Value          'List車両情報３
        '        T00016row("SHARYOINFO4") = WF_ListSHARYOINFO4.Items(i).Value          'List車両情報４
        '        T00016row("SHARYOINFO5") = WF_ListSHARYOINFO5.Items(i).Value          'List車両情報５
        '        T00016row("SHARYOINFO6") = WF_ListSHARYOINFO6.Items(i).Value          'List車両情報６
        '        Exit For
        '    End If
        'Next

        ''従業員追加情報
        'Dim datStaff As JOT_MASTER.STAFF = JOTMASTER.GetStaff(T00016row("STAFFCODE"))
        'If Not IsNothing(datStaff) AndAlso Not IsNothing(datStaff.STAFFCODE) Then
        '    T00016row("STAFFCODENAME") = datStaff.STAFFNAMES                '
        '    T00016row("STAFFNOTES1") = datStaff.NOTES1                      '備考１
        '    T00016row("STAFFNOTES2") = datStaff.NOTES2                      '備考２
        '    T00016row("STAFFNOTES3") = datStaff.NOTES3                      '備考３
        '    T00016row("STAFFNOTES4") = datStaff.NOTES4                      '備考４
        '    T00016row("STAFFNOTES5") = datStaff.NOTES5                      '備考５
        'End If

    End Sub

    ''' <summary>
    ''' GridViewダブルクリック処理 
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_Grid_DBclick()

        'Dim WW_LINECNT As Integer                                   'GridViewのダブルクリック行位置

        ''○処理準備
        ''○画面表示データ復元
        'Master.RecoverTable(T00016tbl)

        ''GridViewのダブルクリック行位置取得
        'If Not Integer.TryParse(WF_GridDBclick.Text, WW_LINECNT) Then
        '    Exit Sub
        'End If
        'WF_REP_LINECNT.Value = WW_LINECNT

        ''■■■ Grid内容(T00016tbl)よりDetail編集 ■■■
        'Master.CreateEmptyTable(T00016INPtbl)

        ''leftBOXキャンセルボタン処理
        'WF_ButtonCan_Click()

    End Sub

    ''' <summary>
    ''' 右ボックスのラジオボタン選択時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_RadioButon_Click()
        '〇RightBox処理（ラジオボタン選択）
        If Not String.IsNullOrEmpty(WF_RightViewChange.Value) Then
            If Integer.TryParse(WF_RightViewChange.Value, WF_RightViewChange.Value) Then
                rightview.SelectIndex(WF_RightViewChange.Value)
                WF_RightViewChange.Value = ""
            End If
        End If
    End Sub

    ''' <summary>
    ''' メモ欄変更時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_MEMO_Change()
        '〇RightBox処理（右Boxメモ変更時）
        rightview.Save(Master.USERID, Master.USERTERMID, WW_DUMMY)
    End Sub

    ''' <summary>
    ''' Operation項目編集処理
    ''' </summary>
    ''' <param name="I_ROW" >行データ</param>
    ''' <param name="I_SEL" >TRUE:選択編集（"★"付加）| FALSE:選択解除（"★"除外）</param>
    ''' <param name="O_ROW" >編集後行データ</param>
    ''' <remarks></remarks>
    Protected Sub EditOperationText(ByRef I_ROW As DataRow, ByVal I_SEL As Boolean, Optional ByRef O_ROW As DataRow = Nothing)
        Dim outRow As DataRow
        If IsNothing(O_ROW) Then
            outRow = I_ROW
        Else
            outRow = O_ROW
        End If

        If I_SEL = True Then
            outRow("OPERATION") = I_ROW("OPERATION").ToString.Insert(0, C_LIST_OPERATION_CODE.SELECTED)
        Else
            outRow("OPERATION") = I_ROW("OPERATION").ToString.Replace(C_LIST_OPERATION_CODE.SELECTED, "")
        End If

    End Sub

    ''' <summary>
    ''' Operation項目編集処理
    ''' </summary>
    ''' <param name="I_TBL" >データテーブル</param>
    ''' <param name="I_SEL" >TRUE:選択編集（"★"付加）| FALSE:選択解除（"★"除外）</param>
    ''' <param name="I_LINECNT" >行指定</param>
    ''' <remarks></remarks>
    Protected Sub EditOperationText(ByRef I_TBL As DataTable, ByVal I_SEL As Boolean, Optional ByVal I_LINECNT As String = "")

        For Each row As DataRow In I_TBL.Rows
            If Not String.IsNullOrEmpty(I_LINECNT) Then
                '行位置指定時はその行のみ選択状態
                If row("LINECNT") = I_LINECNT Then
                    EditOperationText(row, True)
                Else
                    EditOperationText(row, False)
                End If
            Else
                EditOperationText(row, I_SEL)
            End If
        Next
    End Sub

#Region "T0016テーブル関連"
    ''' <summary>
    ''' 画面表示用データ取得
    ''' </summary>
    ''' <remarks>データベース（T00016）を検索し画面表示用データを取得する</remarks>
    Private Sub DBselect_T16SELECT()

        Dim WW_DATE As Date

        '〇GridView内容をテーブル退避
        'T00016テンポラリDB項目作成
        If T00016tbl Is Nothing Then
            T00016tbl = New DataTable
        End If

        If T00016tbl.Columns.Count = 0 Then
        Else
            T00016tbl.Columns.Clear()
        End If

        '○DB項目クリア
        T00016tbl.Clear()

        '〇画面表示用データ取得
        Try

            'DataBase接続文字
            Using SQLcon = CS0050SESSION.getConnection
                SQLcon.Open() 'DataBase接続(Open)

                '検索SQL文
                Dim SQLStr As String =
                      "SELECT 0                                                 as LINECNT ,                    " _
                    & "       ''                                                as OPERATION ,                  " _
                    & "       '0'                                               as 'SELECT' ,                   " _
                    & "       '0'                                               as HIDDEN ,                     " _
                    & "       ''                                                as 'INDEX' ,                    " _
                    & "       isnull(rtrim(A.CAMPCODE),'')                      as CAMPCODE ,                   " _
                    & "       isnull(rtrim(A.DENKBN),'')                        as DENKBN ,                     " _
                    & "       isnull(rtrim(A.DENNO),'')                         as DENNO ,                      " _
                    & "       isnull(rtrim(A.TORIHIKIYMD),'')                   as TORIHIKIYMD ,                " _
                    & "       isnull(rtrim(A.RECODEKBN),'')                     as RECODEKBN ,                  " _
                    & "       isnull(rtrim(A.TORICODE),'')                      as TORICODE ,                   " _
                    & "       isnull(rtrim(A.TODOKECODE),'')                    as TODOKECODE ,                 " _
                    & "       isnull(rtrim(A.GSHABAN),'')                       as GSHABAN ,                    " _
                    & "       isnull(rtrim(A.NSHABAN),'')                       as NSHABAN ,                    " _
                    & "       isnull(rtrim(A.UNCHINCODE),'')                    as UNCHINCODE ,                 " _
                    & "       isnull(rtrim(A.DETAILNO),'')                      as DETAILNO ,                   " _
                    & "       isnull(rtrim(A.ENTRYDATE),'')                     as ENTRYDATE ,                  " _
                    & "       isnull(rtrim(A.ACTORICODE),'')                    as ACTORICODE ,                 " _
                    & "       isnull(rtrim(A.URIKBN),'')                        as URIKBN ,                     " _
                    & "       isnull(rtrim(A.TORIHIKIMANGORG),'')               as TORIHIKIMANGORG ,            " _
                    & "       isnull(rtrim(A.TORIHIKIORG),'')                   as TORIHIKIORG ,                " _
                    & "       isnull(rtrim(A.SEIKYUSHIHARAIMANGORG),'')         as SEIKYUSHIHARAIMANGORG ,      " _
                    & "       isnull(rtrim(A.SEIKYUSHIHARAIORG),'')             as SEIKYUSHIHARAIORG ,          " _
                    & "       isnull(rtrim(A.SEIKYUSHIHARAIYM),'')              as SEIKYUSHIHARAIYM ,           " _
                    & "       isnull(rtrim(A.URIKEIJYOYMD),'')                  as URIKEIJYOYMD ,               " _
                    & "       isnull(rtrim(A.SEIKYUNO),'')                      as SEIKYUNO ,                   " _
                    & "       isnull(rtrim(A.URIPATERNKBN),'')                  as URIPATERNKBN ,               " _
                    & "       isnull(rtrim(A.URIPATTERNCODE),'')                as URIPATTERNCODE ,             " _
                    & "       isnull(rtrim(A.URIAMT),'')                        as URIAMT ,                     " _
                    & "       isnull(rtrim(A.URITAXAMT),'')                     as URITAXAMT ,                  " _
                    & "       isnull(rtrim(A.URISEGMENT1),'')                   as URISEGMENT1 ,                " _
                    & "       isnull(rtrim(A.URISEGMENT2),'')                   as URISEGMENT2 ,                " _
                    & "       isnull(rtrim(A.URISEGMENT3),'')                   as URISEGMENT3 ,                " _
                    & "       isnull(rtrim(A.NDEADLINEDAYS),'')                 as NDEADLINEDAYS ,              " _
                    & "       isnull(rtrim(A.JOTSEIKYUKBN),'')                  as JOTSEIKYUKBN ,               " _
                    & "       isnull(rtrim(A.SEIKYUOUTYMD),'')                  as SEIKYUOUTYMD ,               " _
                    & "       isnull(rtrim(A.NYUKINSITE),'')                    as NYUKINSITE ,                 " _
                    & "       isnull(rtrim(A.NYUKINYMD),'')                     as NYUKINYMD ,                  " _
                    & "       isnull(rtrim(A.SHIHARAIKEIJYOYMD),'')             as SHIHARAIKEIJYOYMD ,          " _
                    & "       isnull(rtrim(A.SHIHARAINO),'')                    as SHIHARAINO ,                 " _
                    & "       isnull(rtrim(A.SHIHARAIPATERNKBN),'')             as SHIHARAIPATERNKBN ,          " _
                    & "       isnull(rtrim(A.SHIHARAIPATTERNCODE),'')           as SHIHARAIPATTERNCODE ,        " _
                    & "       isnull(rtrim(A.SHIHARAIAMT),'')                   as SHIHARAIAMT ,                " _
                    & "       isnull(rtrim(A.SHIHARAITAXAMT),'')                as SHIHARAITAXAMT ,             " _
                    & "       isnull(rtrim(A.SHIHARAISEGMENT1),'')              as SHIHARAISEGMENT1 ,           " _
                    & "       isnull(rtrim(A.SHIHARAISEGMENT2),'')              as SHIHARAISEGMENT2 ,           " _
                    & "       isnull(rtrim(A.SHIHARAISEGMENT3),'')              as SHIHARAISEGMENT3 ,           " _
                    & "       isnull(rtrim(A.GDEADLINEDAYS),'')                 as GDEADLINEDAYS ,              " _
                    & "       isnull(rtrim(A.SEIKYUMATCHYMD),'')                as SEIKYUMATCHYMD ,             " _
                    & "       isnull(rtrim(A.SHIHARAISITE),'')                  as SHIHARAISITE ,               " _
                    & "       isnull(rtrim(A.SHIHARAIYMD),'')                   as SHIHARAIYMD ,                " _
                    & "       isnull(rtrim(A.BANKCODE),'')                      as BANKCODE ,                   " _
                    & "       isnull(rtrim(A.SEIKYUKBN),'')                     as SEIKYUKBN ,                  " _
                    & "       isnull(rtrim(A.NIPPONO),'')                       as NIPPONO ,                    " _
                    & "       isnull(rtrim(A.ORDERNO),'')                       as ORDERNO ,                    " _
                    & "       isnull(rtrim(A.SHUKODATE),'')                     as SHUKODATE ,                  " _
                    & "       isnull(rtrim(A.SHUKADATE),'')                     as SHUKADATE ,                  " _
                    & "       isnull(rtrim(A.TODOKEDATE),'')                    as TODOKEDATE ,                 " _
                    & "       isnull(rtrim(A.SHUKABASHO),'')                    as SHUKABASHO ,                 " _
                    & "       isnull(rtrim(A.SHUKACITIES),'')                   as SHUKACITIES ,                " _
                    & "       isnull(rtrim(A.TODOKECITIES),'')                  as TODOKECITIES ,               " _
                    & "       isnull(rtrim(A.SHARYOTYPEF),'')                   as SHARYOTYPEF ,                " _
                    & "       isnull(rtrim(A.TSHABANF),'')                      as TSHABANF ,                   " _
                    & "       isnull(rtrim(A.SHARYOTYPEB),'')                   as SHARYOTYPEB ,                " _
                    & "       isnull(rtrim(A.TSHABANB),'')                      as TSHABANB ,                   " _
                    & "       isnull(rtrim(A.SHARYOTYPEB2),'')                  as SHARYOTYPEB2 ,               " _
                    & "       isnull(rtrim(A.TSHABANB2),'')                     as TSHABANB2 ,                  " _
                    & "       isnull(rtrim(A.SHARYOKBN),'')                     as SHARYOKBN ,                  " _
                    & "       isnull(rtrim(A.SHAFUKU),'')                       as SHAFUKU ,                    " _
                    & "       isnull(rtrim(A.TRIPNO),'')                        as TRIPNO ,                     " _
                    & "       isnull(rtrim(A.DROPNO),'')                        as DROPNO ,                     " _
                    & "       isnull(rtrim(A.STAFFSU),'')                       as STAFFSU ,                    " _
                    & "       isnull(rtrim(A.STAFFCODE),'')                     as STAFFCODE ,                  " _
                    & "       isnull(rtrim(A.SUBSTAFFCODE),'')                  as SUBSTAFFCODE ,               " _
                    & "       isnull(rtrim(A.OILTYPE),'')                       as OILTYPE ,                    " _
                    & "       isnull(rtrim(A.PRODUCTCODE),'')                   as PRODUCTCODE ,                " _
                    & "       isnull(rtrim(A.TUKORYOKBN),'')                    as TUKORYOKBN ,                 " _
                    & "       isnull(rtrim(A.TUKORYO),'')                       as TUKORYO ,                    " _
                    & "       isnull(rtrim(A.TRIPSTTIME),'')                    as TRIPSTTIME ,                 " _
                    & "       isnull(rtrim(A.TRIPENDTIME),'')                   as TRIPENDTIME ,                " _
                    & "       isnull(rtrim(A.KYUYU),'')                         as KYUYU ,                      " _
                    & "       isnull(rtrim(A.UNCHINDISTANCE),'')                as UNCHINDISTANCE ,             " _
                    & "       isnull(rtrim(A.KEIRYONO),'')                      as KEIRYONO ,                   " _
                    & "       isnull(rtrim(A.JSURYO),'')                        as JSURYO ,                     " _
                    & "       isnull(rtrim(A.JTANI),'')                         as JTANI ,                      " _
                    & "       isnull(rtrim(A.UNCHINCALCKBN),'')                 as UNCHINCALCKBN ,              " _
                    & "       isnull(rtrim(A.ROUNDTRIPDISTANCEAUTO),'')         as ROUNDTRIPDISTANCEAUTO ,      " _
                    & "       isnull(rtrim(A.ROUNDTRIPDISTANCEHAND),'')         as ROUNDTRIPDISTANCEHAND ,      " _
                    & "       isnull(rtrim(A.ROUNDTRIPDISTANCE),'')             as ROUNDTRIPDISTANCE ,          " _
                    & "       isnull(rtrim(A.UNKOUKAISUAUTO),'')                as UNKOUKAISUAUTO ,             " _
                    & "       isnull(rtrim(A.UNKOUKAISUHAND),'')                as UNKOUKAISUHAND ,             " _
                    & "       isnull(rtrim(A.UNKOUKAISU),'')                    as UNKOUKAISU ,                 " _
                    & "       isnull(rtrim(A.UNKOUNISSUAUTO),'')                as UNKOUNISSUAUTO ,             " _
                    & "       isnull(rtrim(A.UNKOUNISSUHAND),'')                as UNKOUNISSUHAND ,             " _
                    & "       isnull(rtrim(A.UNKOUNISSU),'')                    as UNKOUNISSU ,                 " _
                    & "       isnull(rtrim(A.PUBLICHOLIDAYNISSUAUTO),'')        as PUBLICHOLIDAYNISSUAUTO ,     " _
                    & "       isnull(rtrim(A.PUBLICHOLIDAYNISSUHAND),'')        as PUBLICHOLIDAYNISSUHAND ,     " _
                    & "       isnull(rtrim(A.PUBLICHOLIDAYNISSU),'')            as PUBLICHOLIDAYNISSU ,         " _
                    & "       isnull(rtrim(A.PUBLICHOLIDAYKADONISSUAUTO),'')    as PUBLICHOLIDAYKADONISSUAUTO , " _
                    & "       isnull(rtrim(A.PUBLICHOLIDAYKADONISSUHAND),'')    as PUBLICHOLIDAYKADONISSUHAND , " _
                    & "       isnull(rtrim(A.PUBLICHOLIDAYKADONISSU),'')        as PUBLICHOLIDAYKADONISSU ,     " _
                    & "       isnull(rtrim(A.NENMATUNEMSHINISSUAUTO),'')        as NENMATUNEMSHINISSUAUTO ,     " _
                    & "       isnull(rtrim(A.NENMATUNEMSHINISSUHAND),'')        as NENMATUNEMSHINISSUHAND ,     " _
                    & "       isnull(rtrim(A.NENMATUNEMSHINISSU),'')            as NENMATUNEMSHINISSU ,         " _
                    & "       isnull(rtrim(A.KEIYAKUDAISUAUTO),'')              as KEIYAKUDAISUAUTO ,           " _
                    & "       isnull(rtrim(A.KEIYAKUDAISUHAND),'')              as KEIYAKUDAISUHAND ,           " _
                    & "       isnull(rtrim(A.KEIYAKUDAISU),'')                  as KEIYAKUDAISU ,               " _
                    & "       isnull(rtrim(A.SURYOAUTO),'')                     as SURYOAUTO ,                  " _
                    & "       isnull(rtrim(A.SURYOHAND),'')                     as SURYOHAND ,                  " _
                    & "       isnull(rtrim(A.SURYO),'')                         as SURYO ,                      " _
                    & "       isnull(rtrim(A.AMTAUTO),'')                       as AMTAUTO ,                    " _
                    & "       isnull(rtrim(A.AMTHAND),'')                       as AMTHAND ,                    " _
                    & "       isnull(rtrim(A.AMT),'')                           as AMT ,                        " _
                    & "       isnull(rtrim(A.RELATIONNO),'')                    as RELATIONNO ,                 " _
                    & "       isnull(rtrim(A.DELFLG),'')                        as DELFLG ,                     " _
                    & "       TIMSTP = cast(A.UPDTIMSTP  as bigint)                       ,                     " _
                    & "       isnull(rtrim(B.SHARYOINFO1),'')                   as SHARYOINFO1 ,                " _
                    & "       isnull(rtrim(B.SHARYOINFO2),'')                   as SHARYOINFO2 ,                " _
                    & "       isnull(rtrim(B.SHARYOINFO3),'')                   as SHARYOINFO3 ,                " _
                    & "       isnull(rtrim(B.SHARYOINFO4),'')                   as SHARYOINFO4 ,                " _
                    & "       isnull(rtrim(B.SHARYOINFO5),'')                   as SHARYOINFO5 ,                " _
                    & "       isnull(rtrim(B.SHARYOINFO6),'')                   as SHARYOINFO6 ,                " _
                    & "       isnull(rtrim(D.ADDR1),'') +                          				                " _
                    & "       isnull(rtrim(D.ADDR2),'') +                        				                " _
                    & "       isnull(rtrim(D.ADDR3),'') +                         				                " _
                    & "       isnull(rtrim(D.ADDR4),'')          	            as ADDR ,                       " _
                    & "       isnull(rtrim(D.NOTES1),'')        	            as NOTES1 ,                     " _
                    & "       isnull(rtrim(D.NOTES2),'')          	            as NOTES2 ,                     " _
                    & "       isnull(rtrim(D.NOTES3),'')          	            as NOTES3 ,                     " _
                    & "       isnull(rtrim(D.NOTES4),'')          	            as NOTES4 ,                     " _
                    & "       isnull(rtrim(D.NOTES5),'')          	            as NOTES5 ,                     " _
                    & "       isnull(rtrim(D.NOTES6),'')        	            as NOTES6 ,                     " _
                    & "       isnull(rtrim(D.NOTES7),'')          	            as NOTES7 ,                     " _
                    & "       isnull(rtrim(D.NOTES8),'')          	            as NOTES8 ,                     " _
                    & "       isnull(rtrim(D.NOTES9),'')          	            as NOTES9 ,                     " _
                    & "       isnull(rtrim(D.NOTES10),'')          	            as NOTES10 ,                    " _
                    & "       isnull(rtrim(E.NOTES1),'')        	            as STAFFNOTES1 ,                " _
                    & "       isnull(rtrim(E.NOTES2),'')          	            as STAFFNOTES2 ,                " _
                    & "       isnull(rtrim(E.NOTES3),'')          	            as STAFFNOTES3 ,                " _
                    & "       isnull(rtrim(E.NOTES4),'')          	            as STAFFNOTES4 ,                " _
                    & "       isnull(rtrim(E.NOTES5),'')          	            as STAFFNOTES5 ,                " _
                    & "       ''                                                as ROWDEL ,                     " _
                    & "       ''                                                as BUNRUI ,                     " _
                    & "       ''                                                as SOUSA ,                      " _
                    & "       '0'                                               as TANKA ,                      " _
                    & "       '0'                                               as KINGAKU ,                    " _
                    & "       ''                                                as CAMPCODENAME ,               " _
                    & "       ''                                                as TORICODENAME ,               " _
                    & "       ''                                                as SHUKABASHONAME ,             " _
                    & "       ''                                                as TODOKECODENAME ,             " _
                    & "       ''                                                as NSHABANNAME ,                " _
                    & "       ''                                                as SHAFUKUNAME ,                " _
                    & "       ''                                                as BUNRUINAME ,                 " _
                    & "       ''                                                as SOUSANAME ,                  " _
                    & "       ''                                                as TORIHIKIORGNAME ,            " _
                    & "       '0'                                               as TAISHOYM_SUM ,               " _
                    & "       '0'                                               as BUNRUI_SUM ,                 " _
                    & "       '0'                                               as HAISOSAKI_SUM ,              " _
                    & "       '0'                                               as KOUMOKU_SUM ,                " _
                    & "       '0'                                               as TANKA_SUM ,                  " _
                    & "       '0'                                               as TONSU_SUM ,                  " _
                    & "       '0'                                               as DAISU_SUM ,                  " _
                    & "       '0'                                               as KINGAKU_SUM ,                " _
                    & "       '0'                                               as KYUWARI_SUM ,                " _
                    & "       '0'                                               as SHOKEI_SUM ,                 " _
                    & "       '0'                                               as SHOHIZEI_SUM ,               " _
                    & "       '0'                                               as GOKEI_SUM ,                  " _
                    & "       '0'                                               as WORK_NO                      " _
                    & "  FROM T0016_TORIHIKI AS A								                                " _
                    & " INNER JOIN ( SELECT Y.CAMPCODE, Y.CODE                                                  " _
                    & "                FROM S0006_ROLE Y     				                                    " _
                    & "               WHERE Y.CAMPCODE 	 	   = @P01		                                    " _
                    & "                 and Y.OBJECT       	   = 'ORG'		                                    " _
                    & "                 and Y.ROLE             = @P02		                                    " _
                    & "                 and Y.PERMITCODE       in ('1','2')                                     " _
                    & "                 and Y.STYMD            <= @P03		                                    " _
                    & "                 and Y.ENDYMD           >= @P04		                                    " _
                    & "                 and Y.DELFLG           <> '1'		                                    " _
                    & "            ) AS Z									                                    " _
                    & "    ON Z.CAMPCODE		= A.CAMPCODE    			                                    " _
                    & "   and Z.CODE       	    = A.TORIHIKIORG 	    			                            " _
                    & "  LEFT JOIN MA006_SHABANORG B						                                    " _
                    & "    ON B.CAMPCODE     	= A.CAMPCODE 				                                    " _
                    & "   and B.GSHABAN      	= A.GSHABAN 				                                    " _
                    & "   and B.MANGUORG     	= A.TORIHIKIORG 				                                " _
                    & "   and B.DELFLG          <> '1' 						                                    " _
                    & "  LEFT JOIN MC007_TODKORG C 							                                    " _
                    & "    ON C.CAMPCODE     	= A.CAMPCODE 				                                    " _
                    & "   and C.TORICODE     	= A.TORICODE 				                                    " _
                    & "   and C.TODOKECODE   	= A.TODOKECODE 				                                    " _
                    & "   and C.UORG         	= A.TORIHIKIORG 				                                " _
                    & "   and C.DELFLG          <> '1' 						                                    " _
                    & "  LEFT JOIN MC006_TODOKESAKI D 						                                    " _
                    & "    ON D.CAMPCODE     	= C.CAMPCODE 				                                    " _
                    & "   and D.TORICODE     	= C.TORICODE				                                    " _
                    & "   and D.TODOKECODE   	= C.TODOKECODE 				                                    " _
                    & "   and D.STYMD           <= A.TORIHIKIYMD				                                " _
                    & "   and D.ENDYMD          >= A.TORIHIKIYMD				                                " _
                    & "   and D.DELFLG          <> '1' 						                                    " _
                    & "  LEFT JOIN MB001_STAFF E      						                                    " _
                    & "    ON E.CAMPCODE     	= A.CAMPCODE 				                                    " _
                    & "   and E.STAFFCODE     	= A.STAFFCODE				                                    " _
                    & "   and E.STYMD           <= A.TORIHIKIYMD				                                " _
                    & "   and E.ENDYMD          >= A.TORIHIKIYMD				                                " _
                    & "   and E.DELFLG          <> '1' 						                                    " _
                    & " WHERE A.CAMPCODE        = @P01                                                          " _
                    & "   and A.TORIHIKIYMD     <= @P05                                                         " _
                    & "   and A.TORIHIKIYMD     >= @P06                                                         " _
                    & "   and A.DELFLG          <> '1'                                                          "

                '■テーブル検索条件追加

                '条件画面で指定された油種を抽出
                If work.WF_SEL_OILTYPE.Text <> Nothing Then
                    SQLStr = SQLStr & "   and A.OILTYPE              = @P07           		"
                End If

                '条件画面で指定された管理部署を抽出
                If work.WF_SEL_MANGORG.Text <> Nothing Then
                    SQLStr = SQLStr & "   and A.TORIHIKIMANGORG      = @P08           		"
                End If

                '条件画面で指定された出荷部署を抽出
                If work.WF_SEL_SHIPORG.Text <> Nothing Then
                    SQLStr = SQLStr & "   and A.TORIHIKIORG          = @P09           		"
                End If

                '条件画面で指定された荷主を抽出
                If work.WF_SEL_TORICODE.Text <> Nothing Then
                    SQLStr = SQLStr & "   and A.TORICODE             = @P10           		"
                End If

                '条件画面で指定された用車会社を抽出
                If work.WF_SEL_SUPPLCAMP.Text <> Nothing Then
                    SQLStr = SQLStr & "   and A.ACTORICODE           = @P11           		"
                End If

                SQLStr = SQLStr & " ORDER BY A.TORIHIKIYMD ,A.OILTYPE ,A.TORIHIKIMANGORG ,      " _
                                & " 		 A.TORIHIKIORG ,A.TORICODE ,A.ACTORICODE	        "

                Dim SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@P03", System.Data.SqlDbType.Date)          '権限(to)
                Dim PARA04 As SqlParameter = SQLcmd.Parameters.Add("@P04", System.Data.SqlDbType.Date)          '権限(from)
                Dim PARA05 As SqlParameter = SQLcmd.Parameters.Add("@P05", System.Data.SqlDbType.Date)          '取引日付(To)
                Dim PARA06 As SqlParameter = SQLcmd.Parameters.Add("@P06", System.Data.SqlDbType.Date)          '取引日付(From)
                Dim PARA07 As SqlParameter = SQLcmd.Parameters.Add("@P07", System.Data.SqlDbType.NVarChar, 20)  '油種
                Dim PARA08 As SqlParameter = SQLcmd.Parameters.Add("@P08", System.Data.SqlDbType.NVarChar, 20)  '管理部署
                Dim PARA09 As SqlParameter = SQLcmd.Parameters.Add("@P09", System.Data.SqlDbType.NVarChar, 20)  '出荷部署
                Dim PARA10 As SqlParameter = SQLcmd.Parameters.Add("@P10", System.Data.SqlDbType.NVarChar, 20)  '荷主
                Dim PARA11 As SqlParameter = SQLcmd.Parameters.Add("@P11", System.Data.SqlDbType.NVarChar, 20)  '用車会社
                PARA01.Value = work.WF_SEL_CAMPCODE.Text
                PARA02.Value = Master.ROLE_ORG
                PARA03.Value = Date.Now
                PARA04.Value = Date.Now

                '請求月
                If Not String.IsNullOrEmpty(work.WF_SEL_SEIKYUYMF.Text) Then

                    Dim WW_DATE1 As Date
                    If Date.TryParse(work.WF_SEL_SEIKYUYMF.Text, WW_DATE1) Then
                        PARA05.Value = WW_DATE1.AddMonths(1).AddDays(-1).ToString("yyyy/MM/dd")
                        PARA06.Value = work.WF_SEL_SEIKYUYMF.Text & "/01"
                    End If

                ElseIf Not String.IsNullOrEmpty(work.WF_SEL_KEIJYODATEF.Text) Then

                    PARA05.Value = work.WF_SEL_KEIJYODATET.Text
                    PARA06.Value = work.WF_SEL_KEIJYODATEF.Text

                End If

                ''請求月(To)
                'If String.IsNullOrWhiteSpace(work.WF_SEL_SEIKYUYMT.Text) Then
                '    PARA05.Value = work.WF_SEL_KEIJYODATET.Text
                'Else
                '    PARA05.Value = work.WF_SEL_SEIKYUYMT.Text
                'End If
                ''請求月(From)
                'If String.IsNullOrWhiteSpace(work.WF_SEL_SEIKYUYMF.Text) Then
                '    PARA06.Value = work.WF_SEL_KEIJYODATEF.Text
                'Else
                '    PARA06.Value = work.WF_SEL_SEIKYUYMF.Text
                'End If

                '油種
                PARA07.Value = work.WF_SEL_OILTYPE.Text
                '管理部署
                PARA08.Value = work.WF_SEL_MANGORG.Text
                '出荷部署
                PARA09.Value = work.WF_SEL_SHIPORG.Text
                '荷主
                PARA10.Value = work.WF_SEL_TORICODE.Text
                '用車会社
                PARA11.Value = work.WF_SEL_SUPPLCAMP.Text

                Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                'フィールド名とフィールドの型を取得
                For index As Integer = 0 To SQLdr.FieldCount - 1
                    T00016tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                Next
                '〇テーブル検索結果をテーブル格納
                T00016tbl.Load(SQLdr)

                If T00016tbl.Rows.Count > CONST_DSPROW_MAX Then
                    'データ取得件数が65,000件を超えたため表示できません。選択条件を変更して下さい。
                    Master.Output(C_MESSAGE_NO.DISPLAY_RECORD_OVER, C_MESSAGE_TYPE.ABORT)
                    'Close
                    SQLdr.Close() 'Reader(Close)
                    SQLdr = Nothing

                    SQLcmd.Dispose()
                    SQLcmd = Nothing

                    SQLcon.Close() 'DataBase接続(Close)

                    T00016tbl.Clear()
                    Exit Sub
                End If

                'Close
                SQLdr.Close() 'Reader(Close)
                SQLdr = Nothing

                SQLcmd.Dispose()
                SQLcmd = Nothing

                SQLcon.Close() 'DataBase接続(Close)
            End Using

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "T0016_NSEIKYU SELECT")
            CS0011LOGWRITE.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:T0016_NSEIKYU Select"           '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                  '
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try

        For Each T00016row In T00016tbl.Rows

            '○レコードの初期設定

            T00016row("LINECNT") = 0
            T00016row("SELECT") = 1   '1:表示
            T00016row("HIDDEN") = 0   '0:表示
            T00016row("INDEX") = ""
            'T00016row("SEQ") = "00"
            T00016row("WORK_NO") = 0

            If Date.TryParse(T00016row("TORIHIKIYMD"), WW_DATE) Then
                T00016row("TORIHIKIYMD") = WW_DATE.ToString("MM月dd日")
            Else
                T00016row("TORIHIKIYMD") = ""
            End If

            '○項目名称設定
            CODENAME_set(T00016row)

        Next

    End Sub

    ''' <summary>
    ''' 画面表示用データ取得
    ''' </summary>
    ''' <remarks>データベース（T00016）を検索し画面表示用データを取得する</remarks>
    Private Sub DBselect_T16SELECT_TAB1()



    End Sub

    ''' <summary>
    ''' 画面表示用データ取得
    ''' </summary>
    ''' <remarks>データベース（T00016）を検索し画面表示用データを取得する</remarks>
    Private Sub DBselect_T16SELECT_TAB2()



    End Sub

    ''' <summary>
    ''' 画面表示用データ取得
    ''' </summary>
    ''' <remarks>データベース（T00016）を検索し画面表示用データを取得する</remarks>
    Private Sub DBselect_T16SELECT_TAB3()

        '○T00016tbl_tab3カラム設定
        Master.CreateEmptyTable(T00016tbl_tab3, work.WF_SEL_INPTAB3TBL.Text)

        Dim UNKOUKAISU_SUM As Double = 0
        Dim PUBLICHOLIDAYKADONISSU_SUM As Double = 0
        Dim NENMATUNEMSHINISSU_SUM As Double = 0
        Dim SURYO_SUM As Double = 0
        Dim TANKA_SUM As Double = 0
        Dim KINGAKU_SUM As Double = 0
        Dim wkVal As Double = 0

        Dim TORICODE_KEY = ""
        Dim SHUKABASHO_KEY = ""
        Dim TODOKECODE_KEY = ""
        Dim NSHABAN_KEY = ""

        Dim T00016TBLtab3row = T00016tbl_tab3.NewRow
        Dim initFlg As Boolean = True

        Dim WW_INDEX As Integer = 0
        For Each T00016TBLrow In T00016tbl_tab4.Rows

            If initFlg = False Then

                If TORICODE_KEY = T00016TBLrow("TORICODE") AndAlso
                    SHUKABASHO_KEY = T00016TBLrow("SHUKABASHO") AndAlso
                    TODOKECODE_KEY = T00016TBLrow("TODOKECODE") AndAlso
                    NSHABAN_KEY = T00016TBLrow("NSHABAN") Then

                    wkVal = 0
                    If Double.TryParse(T00016TBLrow("UNKOUKAISU"), wkVal) Then
                        UNKOUKAISU_SUM += wkVal
                    End If

                    wkVal = 0
                    If Double.TryParse(T00016TBLrow("PUBLICHOLIDAYKADONISSU"), wkVal) Then
                        PUBLICHOLIDAYKADONISSU_SUM += wkVal
                    End If

                    wkVal = 0
                    If Double.TryParse(T00016TBLrow("NENMATUNEMSHINISSU"), wkVal) Then
                        NENMATUNEMSHINISSU_SUM += wkVal
                    End If

                    wkVal = 0
                    If Double.TryParse(T00016TBLrow("SURYO"), wkVal) Then
                        SURYO_SUM += wkVal
                    End If

                    wkVal = 0
                    If Double.TryParse(T00016TBLrow("TANKA"), wkVal) Then
                        TANKA_SUM += wkVal
                    End If

                    wkVal = 0
                    If Double.TryParse(T00016TBLrow("KINGAKU"), wkVal) Then
                        KINGAKU_SUM += wkVal
                    End If

                    Continue For

                Else

                    T00016TBLtab3row("UNKOUKAISU") = UNKOUKAISU_SUM
                    T00016TBLtab3row("PUBLICHOLIDAYKADONISSU") = PUBLICHOLIDAYKADONISSU_SUM
                    T00016TBLtab3row("NENMATUNEMSHINISSU") = NENMATUNEMSHINISSU_SUM
                    T00016TBLtab3row("SURYO") = SURYO_SUM
                    T00016TBLtab3row("TANKA") = TANKA_SUM
                    T00016TBLtab3row("KINGAKU") = KINGAKU_SUM

                    '○名称付与
                    CODENAME_set(T00016TBLtab3row)

                    '入力テーブル追加
                    T00016tbl_tab3.Rows.Add(T00016TBLtab3row)

                    'クリア
                    UNKOUKAISU_SUM = 0
                    PUBLICHOLIDAYKADONISSU_SUM = 0
                    NENMATUNEMSHINISSU_SUM = 0
                    SURYO_SUM = 0
                    TANKA_SUM = 0
                    KINGAKU_SUM = 0

                End If

            End If

            initFlg = False

            T00016TBLtab3row = T00016tbl_tab3.NewRow

            T00016TBLtab3row("LINECNT") = 0
            T00016TBLtab3row("OPERATION") = T00016TBLrow("OPERATION")
            T00016TBLtab3row("TIMSTP") = "0"
            T00016TBLtab3row("SELECT") = 1
            T00016TBLtab3row("HIDDEN") = 0

            T00016TBLtab3row("INDEX") = WW_INDEX
            WW_INDEX += WW_INDEX

            T00016TBLtab3row("CAMPCODE") = work.WF_SEL_CAMPCODE.Text

            T00016TBLtab3row("DENKBN") = ""
            T00016TBLtab3row("DENNO") = ""

            Dim WW_DATE As Date
            If Date.TryParse(T00016TBLrow("TORIHIKIYMD"), WW_DATE) Then

                Dim WW_YYMM As Date
                Date.TryParse(WW_DATE.ToString("yyyy/MM") + "/01", WW_YYMM)

                T00016TBLtab3row("TORIHIKIYMD") = WW_YYMM.AddMonths(1).AddDays(-1).ToString("MM月dd日")

            Else
                T00016TBLtab3row("TORIHIKIYMD") = ""
            End If

            T00016TBLtab3row("RECODEKBN") = ""
            T00016TBLtab3row("TORICODE") = T00016TBLrow("TORICODE")
            T00016TBLtab3row("TODOKECODE") = T00016TBLrow("TODOKECODE")
            T00016TBLtab3row("GSHABAN") = ""
            T00016TBLtab3row("NSHABAN") = T00016TBLrow("NSHABAN")
            T00016TBLtab3row("UNCHINCODE") = ""
            T00016TBLtab3row("DETAILNO") = ""
            T00016TBLtab3row("ENTRYDATE") = ""
            T00016TBLtab3row("ACTORICODE") = ""
            T00016TBLtab3row("URIKBN") = ""
            T00016TBLtab3row("TORIHIKIMANGORG") = ""
            T00016TBLtab3row("TORIHIKIORG") = T00016TBLrow("TORIHIKIORG")
            T00016TBLtab3row("SEIKYUSHIHARAIMANGORG") = ""
            T00016TBLtab3row("SEIKYUSHIHARAIORG") = ""
            T00016TBLtab3row("SEIKYUSHIHARAIYM") = ""
            T00016TBLtab3row("URIKEIJYOYMD") = ""
            T00016TBLtab3row("SEIKYUNO") = ""
            T00016TBLtab3row("URIPATERNKBN") = ""
            T00016TBLtab3row("URIPATTERNCODE") = ""
            T00016TBLtab3row("URIAMT") = ""
            T00016TBLtab3row("URITAXAMT") = ""
            T00016TBLtab3row("URISEGMENT1") = ""
            T00016TBLtab3row("URISEGMENT2") = ""
            T00016TBLtab3row("URISEGMENT3") = ""
            T00016TBLtab3row("NDEADLINEDAYS") = ""
            T00016TBLtab3row("JOTSEIKYUKBN") = ""
            T00016TBLtab3row("SEIKYUOUTYMD") = ""
            T00016TBLtab3row("NYUKINSITE") = ""
            T00016TBLtab3row("NYUKINYMD") = ""
            T00016TBLtab3row("SHIHARAIKEIJYOYMD") = ""
            T00016TBLtab3row("SHIHARAINO") = ""
            T00016TBLtab3row("SHIHARAIPATERNKBN") = ""
            T00016TBLtab3row("SHIHARAIPATTERNCODE") = ""
            T00016TBLtab3row("SHIHARAIAMT") = ""
            T00016TBLtab3row("SHIHARAITAXAMT") = ""
            T00016TBLtab3row("SHIHARAISEGMENT1") = ""
            T00016TBLtab3row("SHIHARAISEGMENT2") = ""
            T00016TBLtab3row("SHIHARAISEGMENT3") = ""
            T00016TBLtab3row("GDEADLINEDAYS") = ""
            T00016TBLtab3row("SEIKYUMATCHYMD") = ""
            T00016TBLtab3row("SHIHARAISITE") = ""
            T00016TBLtab3row("SHIHARAIYMD") = ""
            T00016TBLtab3row("BANKCODE") = ""
            T00016TBLtab3row("SEIKYUKBN") = ""
            T00016TBLtab3row("NIPPONO") = ""
            T00016TBLtab3row("ORDERNO") = ""
            T00016TBLtab3row("SHUKODATE") = ""
            T00016TBLtab3row("SHUKADATE") = ""
            T00016TBLtab3row("TODOKEDATE") = ""
            T00016TBLtab3row("SHUKABASHO") = T00016TBLrow("SHUKABASHO")
            T00016TBLtab3row("SHUKACITIES") = ""
            T00016TBLtab3row("TODOKECITIES") = ""
            T00016TBLtab3row("SHARYOTYPEF") = ""
            T00016TBLtab3row("TSHABANF") = ""
            T00016TBLtab3row("SHARYOTYPEB") = ""
            T00016TBLtab3row("TSHABANB") = ""
            T00016TBLtab3row("SHARYOTYPEB2") = ""
            T00016TBLtab3row("TSHABANB2") = ""
            T00016TBLtab3row("SHARYOKBN") = ""
            T00016TBLtab3row("SHAFUKU") = T00016TBLrow("SHAFUKU")
            T00016TBLtab3row("TRIPNO") = ""
            T00016TBLtab3row("DROPNO") = ""
            T00016TBLtab3row("STAFFSU") = ""
            T00016TBLtab3row("STAFFCODE") = ""
            T00016TBLtab3row("SUBSTAFFCODE") = ""
            T00016TBLtab3row("OILTYPE") = ""
            T00016TBLtab3row("PRODUCTCODE") = ""
            T00016TBLtab3row("TUKORYOKBN") = ""
            T00016TBLtab3row("TUKORYO") = ""
            T00016TBLtab3row("TRIPSTTIME") = ""
            T00016TBLtab3row("TRIPENDTIME") = ""
            T00016TBLtab3row("KYUYU") = ""
            T00016TBLtab3row("UNCHINDISTANCE") = ""
            T00016TBLtab3row("KEIRYONO") = ""
            T00016TBLtab3row("JSURYO") = ""
            T00016TBLtab3row("JTANI") = ""
            T00016TBLtab3row("UNCHINCALCKBN") = ""
            T00016TBLtab3row("ROUNDTRIPDISTANCEAUTO") = ""
            T00016TBLtab3row("ROUNDTRIPDISTANCEHAND") = ""
            T00016TBLtab3row("ROUNDTRIPDISTANCE") = ""
            T00016TBLtab3row("UNKOUKAISUAUTO") = ""
            T00016TBLtab3row("UNKOUKAISUHAND") = ""

            'T00016TBLtab3row("UNKOUKAISU") = T00016TBLrow("UNKOUKAISU")

            wkVal = 0
            If Double.TryParse(T00016TBLrow("UNKOUKAISU"), wkVal) Then
                UNKOUKAISU_SUM += wkVal
            End If

            T00016TBLtab3row("UNKOUNISSUAUTO") = ""
            T00016TBLtab3row("UNKOUNISSUHAND") = ""
            T00016TBLtab3row("UNKOUNISSU") = ""
            T00016TBLtab3row("PUBLICHOLIDAYNISSUAUTO") = ""
            T00016TBLtab3row("PUBLICHOLIDAYNISSUHAND") = ""
            T00016TBLtab3row("PUBLICHOLIDAYNISSU") = ""
            T00016TBLtab3row("PUBLICHOLIDAYKADONISSUAUTO") = ""
            T00016TBLtab3row("PUBLICHOLIDAYKADONISSUHAND") = ""

            'T00016TBLtab3row("PUBLICHOLIDAYKADONISSU") = T00016TBLrow("PUBLICHOLIDAYKADONISSU")

            wkVal = 0
            If Double.TryParse(T00016TBLrow("PUBLICHOLIDAYKADONISSU"), wkVal) Then
                PUBLICHOLIDAYKADONISSU_SUM += wkVal
            End If

            T00016TBLtab3row("NENMATUNEMSHINISSUAUTO") = ""
            T00016TBLtab3row("NENMATUNEMSHINISSUHAND") = ""

            'T00016TBLtab3row("NENMATUNEMSHINISSU") = T00016TBLrow("NENMATUNEMSHINISSU")

            wkVal = 0
            If Double.TryParse(T00016TBLrow("NENMATUNEMSHINISSU"), wkVal) Then
                NENMATUNEMSHINISSU_SUM += wkVal
            End If

            T00016TBLtab3row("KEIYAKUDAISUAUTO") = ""
            T00016TBLtab3row("KEIYAKUDAISUHAND") = ""
            T00016TBLtab3row("KEIYAKUDAISU") = ""
            T00016TBLtab3row("SURYOAUTO") = ""
            T00016TBLtab3row("SURYOHAND") = ""

            'T00016TBLtab3row("SURYO") = T00016TBLrow("SURYO")

            wkVal = 0
            If Double.TryParse(T00016TBLrow("SURYO"), wkVal) Then
                SURYO_SUM += wkVal
            End If

            T00016TBLtab3row("AMTAUTO") = ""
            T00016TBLtab3row("AMTHAND") = ""
            T00016TBLtab3row("AMT") = ""
            T00016TBLtab3row("RELATIONNO") = ""
            T00016TBLtab3row("DELFLG") = C_DELETE_FLG.ALIVE
            T00016TBLtab3row("BUNRUI") = T00016TBLrow("BUNRUI")
            T00016TBLtab3row("SOUSA") = T00016TBLrow("SOUSA")

            'T00016TBLtab3row("TANKA") = T00016TBLrow("TANKA")

            wkVal = 0
            If Double.TryParse(T00016TBLrow("TANKA"), wkVal) Then
                TANKA_SUM += wkVal
            End If

            'T00016TBLtab3row("KINGAKU") = T00016TBLrow("KINGAKU")

            wkVal = 0
            If Double.TryParse(T00016TBLrow("KINGAKU"), wkVal) Then
                KINGAKU_SUM += wkVal
            End If

            'Grid追加明細（新規追加と同じ）とする
            T00016TBLtab3row("WORK_NO") = ""

            TORICODE_KEY = T00016TBLrow("TORICODE")
            SHUKABASHO_KEY = T00016TBLrow("SHUKABASHO")
            TODOKECODE_KEY = T00016TBLrow("TODOKECODE")
            NSHABAN_KEY = T00016TBLrow("NSHABAN")

        Next

        If T00016tbl_tab4.Rows.Count > 0 Then

            T00016TBLtab3row("UNKOUKAISU") = UNKOUKAISU_SUM
            T00016TBLtab3row("PUBLICHOLIDAYKADONISSU") = PUBLICHOLIDAYKADONISSU_SUM
            T00016TBLtab3row("NENMATUNEMSHINISSU") = NENMATUNEMSHINISSU_SUM
            T00016TBLtab3row("SURYO") = SURYO_SUM
            T00016TBLtab3row("TANKA") = TANKA_SUM
            T00016TBLtab3row("KINGAKU") = KINGAKU_SUM

            '○名称付与
            CODENAME_set(T00016TBLtab3row)

            '入力テーブル追加
            T00016tbl_tab3.Rows.Add(T00016TBLtab3row)

        End If

    End Sub

    ''' <summary>
    ''' 画面表示用データ取得
    ''' </summary>
    ''' <remarks>データベース（T00016）を検索し画面表示用データを取得する</remarks>
    Private Sub DBselect_T16SELECT_TAB4()

        '○T00016tbl_tab4カラム設定
        Master.CreateEmptyTable(T00016tbl_tab4, work.WF_SEL_INPTAB4TBL.Text)

        Dim WW_INDEX As Integer = 0
        For Each T00016TBLrow In T00016tbl.Rows

            Dim T00016TBLtab4row = T00016tbl_tab4.NewRow

            T00016TBLtab4row("LINECNT") = 0
            T00016TBLtab4row("OPERATION") = T00016TBLrow("OPERATION")
            T00016TBLtab4row("TIMSTP") = "0"
            T00016TBLtab4row("SELECT") = 1
            T00016TBLtab4row("HIDDEN") = 0

            T00016TBLtab4row("INDEX") = WW_INDEX
            WW_INDEX += WW_INDEX

            T00016TBLtab4row("CAMPCODE") = work.WF_SEL_CAMPCODE.Text

            T00016TBLtab4row("DENKBN") = ""
            T00016TBLtab4row("DENNO") = ""

            Dim WW_DATE As Date
            If Date.TryParse(T00016TBLrow("TORIHIKIYMD"), WW_DATE) Then
                T00016TBLtab4row("TORIHIKIYMD") = WW_DATE.ToString("MM月dd日")
            Else
                T00016TBLtab4row("TORIHIKIYMD") = ""
            End If

            T00016TBLtab4row("RECODEKBN") = ""
            T00016TBLtab4row("TORICODE") = T00016TBLrow("TORICODE")
            T00016TBLtab4row("TODOKECODE") = T00016TBLrow("TODOKECODE")
            T00016TBLtab4row("GSHABAN") = ""
            T00016TBLtab4row("NSHABAN") = T00016TBLrow("NSHABAN")
            T00016TBLtab4row("UNCHINCODE") = ""
            T00016TBLtab4row("DETAILNO") = ""
            T00016TBLtab4row("ENTRYDATE") = ""
            T00016TBLtab4row("ACTORICODE") = ""
            T00016TBLtab4row("URIKBN") = ""
            T00016TBLtab4row("TORIHIKIMANGORG") = ""
            T00016TBLtab4row("TORIHIKIORG") = T00016TBLrow("TORIHIKIORG")
            T00016TBLtab4row("SEIKYUSHIHARAIMANGORG") = ""
            T00016TBLtab4row("SEIKYUSHIHARAIORG") = ""
            T00016TBLtab4row("SEIKYUSHIHARAIYM") = ""
            T00016TBLtab4row("URIKEIJYOYMD") = ""
            T00016TBLtab4row("SEIKYUNO") = ""
            T00016TBLtab4row("URIPATERNKBN") = ""
            T00016TBLtab4row("URIPATTERNCODE") = ""
            T00016TBLtab4row("URIAMT") = ""
            T00016TBLtab4row("URITAXAMT") = ""
            T00016TBLtab4row("URISEGMENT1") = ""
            T00016TBLtab4row("URISEGMENT2") = ""
            T00016TBLtab4row("URISEGMENT3") = ""
            T00016TBLtab4row("NDEADLINEDAYS") = ""
            T00016TBLtab4row("JOTSEIKYUKBN") = ""
            T00016TBLtab4row("SEIKYUOUTYMD") = ""
            T00016TBLtab4row("NYUKINSITE") = ""
            T00016TBLtab4row("NYUKINYMD") = ""
            T00016TBLtab4row("SHIHARAIKEIJYOYMD") = ""
            T00016TBLtab4row("SHIHARAINO") = ""
            T00016TBLtab4row("SHIHARAIPATERNKBN") = ""
            T00016TBLtab4row("SHIHARAIPATTERNCODE") = ""
            T00016TBLtab4row("SHIHARAIAMT") = ""
            T00016TBLtab4row("SHIHARAITAXAMT") = ""
            T00016TBLtab4row("SHIHARAISEGMENT1") = ""
            T00016TBLtab4row("SHIHARAISEGMENT2") = ""
            T00016TBLtab4row("SHIHARAISEGMENT3") = ""
            T00016TBLtab4row("GDEADLINEDAYS") = ""
            T00016TBLtab4row("SEIKYUMATCHYMD") = ""
            T00016TBLtab4row("SHIHARAISITE") = ""
            T00016TBLtab4row("SHIHARAIYMD") = ""
            T00016TBLtab4row("BANKCODE") = ""
            T00016TBLtab4row("SEIKYUKBN") = ""
            T00016TBLtab4row("NIPPONO") = ""
            T00016TBLtab4row("ORDERNO") = ""
            T00016TBLtab4row("SHUKODATE") = ""
            T00016TBLtab4row("SHUKADATE") = ""
            T00016TBLtab4row("TODOKEDATE") = ""
            T00016TBLtab4row("SHUKABASHO") = T00016TBLrow("SHUKABASHO")
            T00016TBLtab4row("SHUKACITIES") = ""
            T00016TBLtab4row("TODOKECITIES") = ""
            T00016TBLtab4row("SHARYOTYPEF") = ""
            T00016TBLtab4row("TSHABANF") = ""
            T00016TBLtab4row("SHARYOTYPEB") = ""
            T00016TBLtab4row("TSHABANB") = ""
            T00016TBLtab4row("SHARYOTYPEB2") = ""
            T00016TBLtab4row("TSHABANB2") = ""
            T00016TBLtab4row("SHARYOKBN") = ""
            T00016TBLtab4row("SHAFUKU") = T00016TBLrow("SHAFUKU")
            T00016TBLtab4row("TRIPNO") = ""
            T00016TBLtab4row("DROPNO") = ""
            T00016TBLtab4row("STAFFSU") = ""
            T00016TBLtab4row("STAFFCODE") = ""
            T00016TBLtab4row("SUBSTAFFCODE") = ""
            T00016TBLtab4row("OILTYPE") = ""
            T00016TBLtab4row("PRODUCTCODE") = ""
            T00016TBLtab4row("TUKORYOKBN") = ""
            T00016TBLtab4row("TUKORYO") = ""
            T00016TBLtab4row("TRIPSTTIME") = ""
            T00016TBLtab4row("TRIPENDTIME") = ""
            T00016TBLtab4row("KYUYU") = ""
            T00016TBLtab4row("UNCHINDISTANCE") = ""
            T00016TBLtab4row("KEIRYONO") = ""
            T00016TBLtab4row("JSURYO") = ""
            T00016TBLtab4row("JTANI") = ""
            T00016TBLtab4row("UNCHINCALCKBN") = ""
            T00016TBLtab4row("ROUNDTRIPDISTANCEAUTO") = ""
            T00016TBLtab4row("ROUNDTRIPDISTANCEHAND") = ""
            T00016TBLtab4row("ROUNDTRIPDISTANCE") = ""
            T00016TBLtab4row("UNKOUKAISUAUTO") = ""
            T00016TBLtab4row("UNKOUKAISUHAND") = ""
            T00016TBLtab4row("UNKOUKAISU") = T00016TBLrow("UNKOUKAISU")
            T00016TBLtab4row("UNKOUNISSUAUTO") = ""
            T00016TBLtab4row("UNKOUNISSUHAND") = ""
            T00016TBLtab4row("UNKOUNISSU") = ""
            T00016TBLtab4row("PUBLICHOLIDAYNISSUAUTO") = ""
            T00016TBLtab4row("PUBLICHOLIDAYNISSUHAND") = ""
            T00016TBLtab4row("PUBLICHOLIDAYNISSU") = ""
            T00016TBLtab4row("PUBLICHOLIDAYKADONISSUAUTO") = ""
            T00016TBLtab4row("PUBLICHOLIDAYKADONISSUHAND") = ""
            T00016TBLtab4row("PUBLICHOLIDAYKADONISSU") = T00016TBLrow("PUBLICHOLIDAYKADONISSU")
            T00016TBLtab4row("NENMATUNEMSHINISSUAUTO") = ""
            T00016TBLtab4row("NENMATUNEMSHINISSUHAND") = ""
            T00016TBLtab4row("NENMATUNEMSHINISSU") = T00016TBLrow("NENMATUNEMSHINISSU")
            T00016TBLtab4row("KEIYAKUDAISUAUTO") = ""
            T00016TBLtab4row("KEIYAKUDAISUHAND") = ""
            T00016TBLtab4row("KEIYAKUDAISU") = ""
            T00016TBLtab4row("SURYOAUTO") = ""
            T00016TBLtab4row("SURYOHAND") = ""
            T00016TBLtab4row("SURYO") = T00016TBLrow("SURYO")
            T00016TBLtab4row("AMTAUTO") = ""
            T00016TBLtab4row("AMTHAND") = ""
            T00016TBLtab4row("AMT") = ""
            T00016TBLtab4row("RELATIONNO") = ""
            T00016TBLtab4row("DELFLG") = C_DELETE_FLG.ALIVE
            T00016TBLtab4row("BUNRUI") = T00016TBLrow("BUNRUI")
            T00016TBLtab4row("SOUSA") = T00016TBLrow("SOUSA")

            'Grid追加明細（新規追加と同じ）とする
            T00016TBLtab4row("WORK_NO") = ""

            '○名称付与
            CODENAME_set(T00016TBLtab4row)

            '入力テーブル追加
            T00016tbl_tab4.Rows.Add(T00016TBLtab4row)

        Next

    End Sub

    ''' <summary>
    ''' T00016tbl_tab1カラム設定
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub T00016tbl_tab1_ColumnsAdd()

        If T00016tbl_tab1.Columns.Count = 0 Then
        Else
            T00016tbl_tab1.Columns.Clear()
        End If

        'T00016テンポラリDB項目作成
        T00016tbl_tab1.Clear()
        T00016tbl_tab1.Columns.Add("LINECNT", GetType(Integer))            'DBの固定フィールド
        T00016tbl_tab1.Columns.Add("OPERATION", GetType(String))           'DBの固定フィールド
        T00016tbl_tab1.Columns.Add("TIMSTP", GetType(String))              'DBの固定フィールド
        T00016tbl_tab1.Columns.Add("SELECT", GetType(Integer))             'DBの固定フィールド
        T00016tbl_tab1.Columns.Add("HIDDEN", GetType(Integer))             'DBの固定フィールド

        T00016tbl_tab1.Columns.Add("INDEX", GetType(String))
        T00016tbl_tab1.Columns.Add("WORK_NO", GetType(String))

        T00016tbl_tab1.Columns.Add("CAMPCODE", GetType(String))
        T00016tbl_tab1.Columns.Add("DENKBN", GetType(String))
        T00016tbl_tab1.Columns.Add("DENNO", GetType(String))
        T00016tbl_tab1.Columns.Add("TORIHIKIYMD", GetType(String))
        T00016tbl_tab1.Columns.Add("RECODEKBN", GetType(String))
        T00016tbl_tab1.Columns.Add("TORICODE", GetType(String))
        T00016tbl_tab1.Columns.Add("TODOKECODE", GetType(String))
        T00016tbl_tab1.Columns.Add("GSHABAN", GetType(String))
        T00016tbl_tab1.Columns.Add("NSHABAN", GetType(String))
        T00016tbl_tab1.Columns.Add("UNCHINCODE", GetType(String))
        T00016tbl_tab1.Columns.Add("DETAILNO", GetType(String))
        T00016tbl_tab1.Columns.Add("ENTRYDATE", GetType(String))
        T00016tbl_tab1.Columns.Add("ACTORICODE", GetType(String))
        T00016tbl_tab1.Columns.Add("URIKBN", GetType(String))
        T00016tbl_tab1.Columns.Add("TORIHIKIMANGORG", GetType(String))
        T00016tbl_tab1.Columns.Add("TORIHIKIORG", GetType(String))
        T00016tbl_tab1.Columns.Add("SEIKYUSHIHARAIMANGORG", GetType(String))
        T00016tbl_tab1.Columns.Add("SEIKYUSHIHARAIORG", GetType(String))
        T00016tbl_tab1.Columns.Add("SEIKYUSHIHARAIYM", GetType(String))
        T00016tbl_tab1.Columns.Add("URIKEIJYOYMD", GetType(String))
        T00016tbl_tab1.Columns.Add("SEIKYUNO", GetType(String))
        T00016tbl_tab1.Columns.Add("URIPATERNKBN", GetType(String))
        T00016tbl_tab1.Columns.Add("URIPATTERNCODE", GetType(String))
        T00016tbl_tab1.Columns.Add("URIAMT", GetType(String))
        T00016tbl_tab1.Columns.Add("URITAXAMT", GetType(String))
        T00016tbl_tab1.Columns.Add("URISEGMENT1", GetType(String))
        T00016tbl_tab1.Columns.Add("URISEGMENT2", GetType(String))
        T00016tbl_tab1.Columns.Add("URISEGMENT3", GetType(String))
        T00016tbl_tab1.Columns.Add("NDEADLINEDAYS", GetType(String))
        T00016tbl_tab1.Columns.Add("JOTSEIKYUKBN", GetType(String))
        T00016tbl_tab1.Columns.Add("SEIKYUOUTYMD", GetType(String))
        T00016tbl_tab1.Columns.Add("NYUKINSITE", GetType(String))
        T00016tbl_tab1.Columns.Add("NYUKINYMD", GetType(String))
        T00016tbl_tab1.Columns.Add("SHIHARAIKEIJYOYMD", GetType(String))
        T00016tbl_tab1.Columns.Add("SHIHARAINO", GetType(String))
        T00016tbl_tab1.Columns.Add("SHIHARAIPATERNKBN", GetType(String))
        T00016tbl_tab1.Columns.Add("SHIHARAIPATTERNCODE", GetType(String))
        T00016tbl_tab1.Columns.Add("SHIHARAIAMT", GetType(String))
        T00016tbl_tab1.Columns.Add("SHIHARAITAXAMT", GetType(String))
        T00016tbl_tab1.Columns.Add("SHIHARAISEGMENT1", GetType(String))
        T00016tbl_tab1.Columns.Add("SHIHARAISEGMENT2", GetType(String))
        T00016tbl_tab1.Columns.Add("SHIHARAISEGMENT3", GetType(String))
        T00016tbl_tab1.Columns.Add("GDEADLINEDAYS", GetType(String))
        T00016tbl_tab1.Columns.Add("SEIKYUMATCHYMD", GetType(String))
        T00016tbl_tab1.Columns.Add("SHIHARAISITE", GetType(String))
        T00016tbl_tab1.Columns.Add("SHIHARAIYMD", GetType(String))
        T00016tbl_tab1.Columns.Add("BANKCODE", GetType(String))
        T00016tbl_tab1.Columns.Add("SEIKYUKBN", GetType(String))
        T00016tbl_tab1.Columns.Add("NIPPONO", GetType(String))
        T00016tbl_tab1.Columns.Add("ORDERNO", GetType(String))
        T00016tbl_tab1.Columns.Add("SHUKODATE", GetType(String))
        T00016tbl_tab1.Columns.Add("SHUKADATE", GetType(String))
        T00016tbl_tab1.Columns.Add("TODOKEDATE", GetType(String))
        T00016tbl_tab1.Columns.Add("SHUKABASHO", GetType(String))
        T00016tbl_tab1.Columns.Add("SHUKACITIES", GetType(String))
        T00016tbl_tab1.Columns.Add("TODOKECITIES", GetType(String))
        T00016tbl_tab1.Columns.Add("SHARYOTYPEF", GetType(String))
        T00016tbl_tab1.Columns.Add("TSHABANF", GetType(String))
        T00016tbl_tab1.Columns.Add("SHARYOTYPEB", GetType(String))
        T00016tbl_tab1.Columns.Add("TSHABANB", GetType(String))
        T00016tbl_tab1.Columns.Add("SHARYOTYPEB2", GetType(String))
        T00016tbl_tab1.Columns.Add("TSHABANB2", GetType(String))
        T00016tbl_tab1.Columns.Add("SHARYOKBN", GetType(String))
        T00016tbl_tab1.Columns.Add("SHAFUKU", GetType(String))
        T00016tbl_tab1.Columns.Add("TRIPNO", GetType(String))
        T00016tbl_tab1.Columns.Add("DROPNO", GetType(String))
        T00016tbl_tab1.Columns.Add("STAFFSU", GetType(String))
        T00016tbl_tab1.Columns.Add("STAFFCODE", GetType(String))
        T00016tbl_tab1.Columns.Add("SUBSTAFFCODE", GetType(String))
        T00016tbl_tab1.Columns.Add("OILTYPE", GetType(String))
        T00016tbl_tab1.Columns.Add("PRODUCTCODE", GetType(String))
        T00016tbl_tab1.Columns.Add("TUKORYOKBN", GetType(String))
        T00016tbl_tab1.Columns.Add("TUKORYO", GetType(String))
        T00016tbl_tab1.Columns.Add("TRIPSTTIME", GetType(String))
        T00016tbl_tab1.Columns.Add("TRIPENDTIME", GetType(String))
        T00016tbl_tab1.Columns.Add("KYUYU", GetType(String))
        T00016tbl_tab1.Columns.Add("UNCHINDISTANCE", GetType(String))
        T00016tbl_tab1.Columns.Add("KEIRYONO", GetType(String))
        T00016tbl_tab1.Columns.Add("JSURYO", GetType(String))
        T00016tbl_tab1.Columns.Add("JTANI", GetType(String))
        T00016tbl_tab1.Columns.Add("UNCHINCALCKBN", GetType(String))
        T00016tbl_tab1.Columns.Add("ROUNDTRIPDISTANCEAUTO", GetType(String))
        T00016tbl_tab1.Columns.Add("ROUNDTRIPDISTANCEHAND", GetType(String))
        T00016tbl_tab1.Columns.Add("ROUNDTRIPDISTANCE", GetType(String))
        T00016tbl_tab1.Columns.Add("UNKOUKAISUAUTO", GetType(String))
        T00016tbl_tab1.Columns.Add("UNKOUKAISUHAND", GetType(String))
        T00016tbl_tab1.Columns.Add("UNKOUKAISU", GetType(String))
        T00016tbl_tab1.Columns.Add("UNKOUNISSUAUTO", GetType(String))
        T00016tbl_tab1.Columns.Add("UNKOUNISSUHAND", GetType(String))
        T00016tbl_tab1.Columns.Add("UNKOUNISSU", GetType(String))
        T00016tbl_tab1.Columns.Add("PUBLICHOLIDAYNISSUAUTO", GetType(String))
        T00016tbl_tab1.Columns.Add("PUBLICHOLIDAYNISSUHAND", GetType(String))
        T00016tbl_tab1.Columns.Add("PUBLICHOLIDAYNISSU", GetType(String))
        T00016tbl_tab1.Columns.Add("PUBLICHOLIDAYKADONISSUAUTO", GetType(String))
        T00016tbl_tab1.Columns.Add("PUBLICHOLIDAYKADONISSUHAND", GetType(String))
        T00016tbl_tab1.Columns.Add("PUBLICHOLIDAYKADONISSU", GetType(String))
        T00016tbl_tab1.Columns.Add("NENMATUNEMSHINISSUAUTO", GetType(String))
        T00016tbl_tab1.Columns.Add("NENMATUNEMSHINISSUHAND", GetType(String))
        T00016tbl_tab1.Columns.Add("NENMATUNEMSHINISSU", GetType(String))
        T00016tbl_tab1.Columns.Add("KEIYAKUDAISUAUTO", GetType(String))
        T00016tbl_tab1.Columns.Add("KEIYAKUDAISUHAND", GetType(String))
        T00016tbl_tab1.Columns.Add("KEIYAKUDAISU", GetType(String))
        T00016tbl_tab1.Columns.Add("SURYOAUTO", GetType(String))
        T00016tbl_tab1.Columns.Add("SURYOHAND", GetType(String))
        T00016tbl_tab1.Columns.Add("SURYO", GetType(String))
        T00016tbl_tab1.Columns.Add("AMTAUTO", GetType(String))
        T00016tbl_tab1.Columns.Add("AMTHAND", GetType(String))
        T00016tbl_tab1.Columns.Add("AMT", GetType(String))
        T00016tbl_tab1.Columns.Add("RELATIONNO", GetType(String))
        T00016tbl_tab1.Columns.Add("DELFLG", GetType(String))
        T00016tbl_tab1.Columns.Add("BUNRUI", GetType(String))
        T00016tbl_tab1.Columns.Add("SOUSA", GetType(String))
        T00016tbl_tab1.Columns.Add("TANKA", GetType(String))
        T00016tbl_tab1.Columns.Add("KINGAKU", GetType(String))
        T00016tbl_tab1.Columns.Add("CAMPCODENAME", GetType(String))
        T00016tbl_tab1.Columns.Add("TORICODENAME", GetType(String))
        T00016tbl_tab1.Columns.Add("SHUKABASHONAME", GetType(String))
        T00016tbl_tab1.Columns.Add("TODOKECODENAME", GetType(String))
        T00016tbl_tab1.Columns.Add("NSHABANNAME", GetType(String))
        T00016tbl_tab1.Columns.Add("SHAFUKUNAME", GetType(String))
        T00016tbl_tab1.Columns.Add("BUNRUINAME", GetType(String))
        T00016tbl_tab1.Columns.Add("SOUSANAME", GetType(String))
        T00016tbl_tab1.Columns.Add("TORIHIKIORGNAME", GetType(String))
        T00016tbl_tab1.Columns.Add("TAISHOYM_SUM", GetType(String))
        T00016tbl_tab1.Columns.Add("BUNRUI_SUM", GetType(String))
        T00016tbl_tab1.Columns.Add("HAISOSAKI_SUM", GetType(String))
        T00016tbl_tab1.Columns.Add("KOUMOKU_SUM", GetType(String))
        T00016tbl_tab1.Columns.Add("TANKA_SUM", GetType(String))
        T00016tbl_tab1.Columns.Add("TONSU_SUM", GetType(String))
        T00016tbl_tab1.Columns.Add("DAISU_SUM", GetType(String))
        T00016tbl_tab1.Columns.Add("KINGAKU_SUM", GetType(String))
        T00016tbl_tab1.Columns.Add("KYUWARI_SUM", GetType(String))
        T00016tbl_tab1.Columns.Add("SHOKEI_SUM", GetType(String))
        T00016tbl_tab1.Columns.Add("SHOHIZEI_SUM", GetType(String))
        T00016tbl_tab1.Columns.Add("GOKEI_SUM", GetType(String))

    End Sub

    ''' <summary>
    ''' T00016tbl_tab2カラム設定
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub T00016tbl_tab2_ColumnsAdd()

        If T00016tbl_tab2.Columns.Count = 0 Then
        Else
            T00016tbl_tab2.Columns.Clear()
        End If

        'T00016テンポラリDB項目作成
        T00016tbl_tab2.Clear()
        T00016tbl_tab2.Columns.Add("LINECNT", GetType(Integer))            'DBの固定フィールド
        T00016tbl_tab2.Columns.Add("OPERATION", GetType(String))           'DBの固定フィールド
        T00016tbl_tab2.Columns.Add("TIMSTP", GetType(String))              'DBの固定フィールド
        T00016tbl_tab2.Columns.Add("SELECT", GetType(Integer))             'DBの固定フィールド
        T00016tbl_tab2.Columns.Add("HIDDEN", GetType(Integer))             'DBの固定フィールド

        T00016tbl_tab2.Columns.Add("INDEX", GetType(String))
        T00016tbl_tab2.Columns.Add("WORK_NO", GetType(String))

        T00016tbl_tab2.Columns.Add("CAMPCODE", GetType(String))
        T00016tbl_tab2.Columns.Add("DENKBN", GetType(String))
        T00016tbl_tab2.Columns.Add("DENNO", GetType(String))
        T00016tbl_tab2.Columns.Add("TORIHIKIYMD", GetType(String))
        T00016tbl_tab2.Columns.Add("RECODEKBN", GetType(String))
        T00016tbl_tab2.Columns.Add("TORICODE", GetType(String))
        T00016tbl_tab2.Columns.Add("TODOKECODE", GetType(String))
        T00016tbl_tab2.Columns.Add("GSHABAN", GetType(String))
        T00016tbl_tab2.Columns.Add("NSHABAN", GetType(String))
        T00016tbl_tab2.Columns.Add("UNCHINCODE", GetType(String))
        T00016tbl_tab2.Columns.Add("DETAILNO", GetType(String))
        T00016tbl_tab2.Columns.Add("ENTRYDATE", GetType(String))
        T00016tbl_tab2.Columns.Add("ACTORICODE", GetType(String))
        T00016tbl_tab2.Columns.Add("URIKBN", GetType(String))
        T00016tbl_tab2.Columns.Add("TORIHIKIMANGORG", GetType(String))
        T00016tbl_tab2.Columns.Add("TORIHIKIORG", GetType(String))
        T00016tbl_tab2.Columns.Add("SEIKYUSHIHARAIMANGORG", GetType(String))
        T00016tbl_tab2.Columns.Add("SEIKYUSHIHARAIORG", GetType(String))
        T00016tbl_tab2.Columns.Add("SEIKYUSHIHARAIYM", GetType(String))
        T00016tbl_tab2.Columns.Add("URIKEIJYOYMD", GetType(String))
        T00016tbl_tab2.Columns.Add("SEIKYUNO", GetType(String))
        T00016tbl_tab2.Columns.Add("URIPATERNKBN", GetType(String))
        T00016tbl_tab2.Columns.Add("URIPATTERNCODE", GetType(String))
        T00016tbl_tab2.Columns.Add("URIAMT", GetType(String))
        T00016tbl_tab2.Columns.Add("URITAXAMT", GetType(String))
        T00016tbl_tab2.Columns.Add("URISEGMENT1", GetType(String))
        T00016tbl_tab2.Columns.Add("URISEGMENT2", GetType(String))
        T00016tbl_tab2.Columns.Add("URISEGMENT3", GetType(String))
        T00016tbl_tab2.Columns.Add("NDEADLINEDAYS", GetType(String))
        T00016tbl_tab2.Columns.Add("JOTSEIKYUKBN", GetType(String))
        T00016tbl_tab2.Columns.Add("SEIKYUOUTYMD", GetType(String))
        T00016tbl_tab2.Columns.Add("NYUKINSITE", GetType(String))
        T00016tbl_tab2.Columns.Add("NYUKINYMD", GetType(String))
        T00016tbl_tab2.Columns.Add("SHIHARAIKEIJYOYMD", GetType(String))
        T00016tbl_tab2.Columns.Add("SHIHARAINO", GetType(String))
        T00016tbl_tab2.Columns.Add("SHIHARAIPATERNKBN", GetType(String))
        T00016tbl_tab2.Columns.Add("SHIHARAIPATTERNCODE", GetType(String))
        T00016tbl_tab2.Columns.Add("SHIHARAIAMT", GetType(String))
        T00016tbl_tab2.Columns.Add("SHIHARAITAXAMT", GetType(String))
        T00016tbl_tab2.Columns.Add("SHIHARAISEGMENT1", GetType(String))
        T00016tbl_tab2.Columns.Add("SHIHARAISEGMENT2", GetType(String))
        T00016tbl_tab2.Columns.Add("SHIHARAISEGMENT3", GetType(String))
        T00016tbl_tab2.Columns.Add("GDEADLINEDAYS", GetType(String))
        T00016tbl_tab2.Columns.Add("SEIKYUMATCHYMD", GetType(String))
        T00016tbl_tab2.Columns.Add("SHIHARAISITE", GetType(String))
        T00016tbl_tab2.Columns.Add("SHIHARAIYMD", GetType(String))
        T00016tbl_tab2.Columns.Add("BANKCODE", GetType(String))
        T00016tbl_tab2.Columns.Add("SEIKYUKBN", GetType(String))
        T00016tbl_tab2.Columns.Add("NIPPONO", GetType(String))
        T00016tbl_tab2.Columns.Add("ORDERNO", GetType(String))
        T00016tbl_tab2.Columns.Add("SHUKODATE", GetType(String))
        T00016tbl_tab2.Columns.Add("SHUKADATE", GetType(String))
        T00016tbl_tab2.Columns.Add("TODOKEDATE", GetType(String))
        T00016tbl_tab2.Columns.Add("SHUKABASHO", GetType(String))
        T00016tbl_tab2.Columns.Add("SHUKACITIES", GetType(String))
        T00016tbl_tab2.Columns.Add("TODOKECITIES", GetType(String))
        T00016tbl_tab2.Columns.Add("SHARYOTYPEF", GetType(String))
        T00016tbl_tab2.Columns.Add("TSHABANF", GetType(String))
        T00016tbl_tab2.Columns.Add("SHARYOTYPEB", GetType(String))
        T00016tbl_tab2.Columns.Add("TSHABANB", GetType(String))
        T00016tbl_tab2.Columns.Add("SHARYOTYPEB2", GetType(String))
        T00016tbl_tab2.Columns.Add("TSHABANB2", GetType(String))
        T00016tbl_tab2.Columns.Add("SHARYOKBN", GetType(String))
        T00016tbl_tab2.Columns.Add("SHAFUKU", GetType(String))
        T00016tbl_tab2.Columns.Add("TRIPNO", GetType(String))
        T00016tbl_tab2.Columns.Add("DROPNO", GetType(String))
        T00016tbl_tab2.Columns.Add("STAFFSU", GetType(String))
        T00016tbl_tab2.Columns.Add("STAFFCODE", GetType(String))
        T00016tbl_tab2.Columns.Add("SUBSTAFFCODE", GetType(String))
        T00016tbl_tab2.Columns.Add("OILTYPE", GetType(String))
        T00016tbl_tab2.Columns.Add("PRODUCTCODE", GetType(String))
        T00016tbl_tab2.Columns.Add("TUKORYOKBN", GetType(String))
        T00016tbl_tab2.Columns.Add("TUKORYO", GetType(String))
        T00016tbl_tab2.Columns.Add("TRIPSTTIME", GetType(String))
        T00016tbl_tab2.Columns.Add("TRIPENDTIME", GetType(String))
        T00016tbl_tab2.Columns.Add("KYUYU", GetType(String))
        T00016tbl_tab2.Columns.Add("UNCHINDISTANCE", GetType(String))
        T00016tbl_tab2.Columns.Add("KEIRYONO", GetType(String))
        T00016tbl_tab2.Columns.Add("JSURYO", GetType(String))
        T00016tbl_tab2.Columns.Add("JTANI", GetType(String))
        T00016tbl_tab2.Columns.Add("UNCHINCALCKBN", GetType(String))
        T00016tbl_tab2.Columns.Add("ROUNDTRIPDISTANCEAUTO", GetType(String))
        T00016tbl_tab2.Columns.Add("ROUNDTRIPDISTANCEHAND", GetType(String))
        T00016tbl_tab2.Columns.Add("ROUNDTRIPDISTANCE", GetType(String))
        T00016tbl_tab2.Columns.Add("UNKOUKAISUAUTO", GetType(String))
        T00016tbl_tab2.Columns.Add("UNKOUKAISUHAND", GetType(String))
        T00016tbl_tab2.Columns.Add("UNKOUKAISU", GetType(String))
        T00016tbl_tab2.Columns.Add("UNKOUNISSUAUTO", GetType(String))
        T00016tbl_tab2.Columns.Add("UNKOUNISSUHAND", GetType(String))
        T00016tbl_tab2.Columns.Add("UNKOUNISSU", GetType(String))
        T00016tbl_tab2.Columns.Add("PUBLICHOLIDAYNISSUAUTO", GetType(String))
        T00016tbl_tab2.Columns.Add("PUBLICHOLIDAYNISSUHAND", GetType(String))
        T00016tbl_tab2.Columns.Add("PUBLICHOLIDAYNISSU", GetType(String))
        T00016tbl_tab2.Columns.Add("PUBLICHOLIDAYKADONISSUAUTO", GetType(String))
        T00016tbl_tab2.Columns.Add("PUBLICHOLIDAYKADONISSUHAND", GetType(String))
        T00016tbl_tab2.Columns.Add("PUBLICHOLIDAYKADONISSU", GetType(String))
        T00016tbl_tab2.Columns.Add("NENMATUNEMSHINISSUAUTO", GetType(String))
        T00016tbl_tab2.Columns.Add("NENMATUNEMSHINISSUHAND", GetType(String))
        T00016tbl_tab2.Columns.Add("NENMATUNEMSHINISSU", GetType(String))
        T00016tbl_tab2.Columns.Add("KEIYAKUDAISUAUTO", GetType(String))
        T00016tbl_tab2.Columns.Add("KEIYAKUDAISUHAND", GetType(String))
        T00016tbl_tab2.Columns.Add("KEIYAKUDAISU", GetType(String))
        T00016tbl_tab2.Columns.Add("SURYOAUTO", GetType(String))
        T00016tbl_tab2.Columns.Add("SURYOHAND", GetType(String))
        T00016tbl_tab2.Columns.Add("SURYO", GetType(String))
        T00016tbl_tab2.Columns.Add("AMTAUTO", GetType(String))
        T00016tbl_tab2.Columns.Add("AMTHAND", GetType(String))
        T00016tbl_tab2.Columns.Add("AMT", GetType(String))
        T00016tbl_tab2.Columns.Add("RELATIONNO", GetType(String))
        T00016tbl_tab2.Columns.Add("DELFLG", GetType(String))
        T00016tbl_tab2.Columns.Add("BUNRUI", GetType(String))
        T00016tbl_tab2.Columns.Add("SOUSA", GetType(String))
        T00016tbl_tab2.Columns.Add("TANKA", GetType(String))
        T00016tbl_tab2.Columns.Add("KINGAKU", GetType(String))
        T00016tbl_tab2.Columns.Add("CAMPCODENAME", GetType(String))
        T00016tbl_tab2.Columns.Add("TORICODENAME", GetType(String))
        T00016tbl_tab2.Columns.Add("SHUKABASHONAME", GetType(String))
        T00016tbl_tab2.Columns.Add("TODOKECODENAME", GetType(String))
        T00016tbl_tab2.Columns.Add("NSHABANNAME", GetType(String))
        T00016tbl_tab2.Columns.Add("SHAFUKUNAME", GetType(String))
        T00016tbl_tab2.Columns.Add("BUNRUINAME", GetType(String))
        T00016tbl_tab2.Columns.Add("SOUSANAME", GetType(String))
        T00016tbl_tab2.Columns.Add("TORIHIKIORGNAME", GetType(String))
        T00016tbl_tab2.Columns.Add("TAISHOYM_SUM", GetType(String))
        T00016tbl_tab2.Columns.Add("BUNRUI_SUM", GetType(String))
        T00016tbl_tab2.Columns.Add("HAISOSAKI_SUM", GetType(String))
        T00016tbl_tab2.Columns.Add("KOUMOKU_SUM", GetType(String))
        T00016tbl_tab2.Columns.Add("TANKA_SUM", GetType(String))
        T00016tbl_tab2.Columns.Add("TONSU_SUM", GetType(String))
        T00016tbl_tab2.Columns.Add("DAISU_SUM", GetType(String))
        T00016tbl_tab2.Columns.Add("KINGAKU_SUM", GetType(String))
        T00016tbl_tab2.Columns.Add("KYUWARI_SUM", GetType(String))
        T00016tbl_tab2.Columns.Add("SHOKEI_SUM", GetType(String))
        T00016tbl_tab2.Columns.Add("SHOHIZEI_SUM", GetType(String))
        T00016tbl_tab2.Columns.Add("GOKEI_SUM", GetType(String))

    End Sub

    ''' <summary>
    ''' T00016tbl_tab3カラム設定
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub T00016tbl_tab3_ColumnsAdd()

        If T00016tbl_tab3.Columns.Count = 0 Then
        Else
            T00016tbl_tab3.Columns.Clear()
        End If

        'T00016テンポラリDB項目作成
        T00016tbl_tab3.Clear()
        T00016tbl_tab3.Columns.Add("LINECNT", GetType(Integer))            'DBの固定フィールド
        T00016tbl_tab3.Columns.Add("OPERATION", GetType(String))           'DBの固定フィールド
        T00016tbl_tab3.Columns.Add("TIMSTP", GetType(String))              'DBの固定フィールド
        T00016tbl_tab3.Columns.Add("SELECT", GetType(Integer))             'DBの固定フィールド
        T00016tbl_tab3.Columns.Add("HIDDEN", GetType(Integer))             'DBの固定フィールド

        T00016tbl_tab3.Columns.Add("INDEX", GetType(String))
        T00016tbl_tab3.Columns.Add("WORK_NO", GetType(String))

        T00016tbl_tab3.Columns.Add("CAMPCODE", GetType(String))
        T00016tbl_tab3.Columns.Add("DENKBN", GetType(String))
        T00016tbl_tab3.Columns.Add("DENNO", GetType(String))
        T00016tbl_tab3.Columns.Add("TORIHIKIYMD", GetType(String))
        T00016tbl_tab3.Columns.Add("RECODEKBN", GetType(String))
        T00016tbl_tab3.Columns.Add("TORICODE", GetType(String))
        T00016tbl_tab3.Columns.Add("TODOKECODE", GetType(String))
        T00016tbl_tab3.Columns.Add("GSHABAN", GetType(String))
        T00016tbl_tab3.Columns.Add("NSHABAN", GetType(String))
        T00016tbl_tab3.Columns.Add("UNCHINCODE", GetType(String))
        T00016tbl_tab3.Columns.Add("DETAILNO", GetType(String))
        T00016tbl_tab3.Columns.Add("ENTRYDATE", GetType(String))
        T00016tbl_tab3.Columns.Add("ACTORICODE", GetType(String))
        T00016tbl_tab3.Columns.Add("URIKBN", GetType(String))
        T00016tbl_tab3.Columns.Add("TORIHIKIMANGORG", GetType(String))
        T00016tbl_tab3.Columns.Add("TORIHIKIORG", GetType(String))
        T00016tbl_tab3.Columns.Add("SEIKYUSHIHARAIMANGORG", GetType(String))
        T00016tbl_tab3.Columns.Add("SEIKYUSHIHARAIORG", GetType(String))
        T00016tbl_tab3.Columns.Add("SEIKYUSHIHARAIYM", GetType(String))
        T00016tbl_tab3.Columns.Add("URIKEIJYOYMD", GetType(String))
        T00016tbl_tab3.Columns.Add("SEIKYUNO", GetType(String))
        T00016tbl_tab3.Columns.Add("URIPATERNKBN", GetType(String))
        T00016tbl_tab3.Columns.Add("URIPATTERNCODE", GetType(String))
        T00016tbl_tab3.Columns.Add("URIAMT", GetType(String))
        T00016tbl_tab3.Columns.Add("URITAXAMT", GetType(String))
        T00016tbl_tab3.Columns.Add("URISEGMENT1", GetType(String))
        T00016tbl_tab3.Columns.Add("URISEGMENT2", GetType(String))
        T00016tbl_tab3.Columns.Add("URISEGMENT3", GetType(String))
        T00016tbl_tab3.Columns.Add("NDEADLINEDAYS", GetType(String))
        T00016tbl_tab3.Columns.Add("JOTSEIKYUKBN", GetType(String))
        T00016tbl_tab3.Columns.Add("SEIKYUOUTYMD", GetType(String))
        T00016tbl_tab3.Columns.Add("NYUKINSITE", GetType(String))
        T00016tbl_tab3.Columns.Add("NYUKINYMD", GetType(String))
        T00016tbl_tab3.Columns.Add("SHIHARAIKEIJYOYMD", GetType(String))
        T00016tbl_tab3.Columns.Add("SHIHARAINO", GetType(String))
        T00016tbl_tab3.Columns.Add("SHIHARAIPATERNKBN", GetType(String))
        T00016tbl_tab3.Columns.Add("SHIHARAIPATTERNCODE", GetType(String))
        T00016tbl_tab3.Columns.Add("SHIHARAIAMT", GetType(String))
        T00016tbl_tab3.Columns.Add("SHIHARAITAXAMT", GetType(String))
        T00016tbl_tab3.Columns.Add("SHIHARAISEGMENT1", GetType(String))
        T00016tbl_tab3.Columns.Add("SHIHARAISEGMENT2", GetType(String))
        T00016tbl_tab3.Columns.Add("SHIHARAISEGMENT3", GetType(String))
        T00016tbl_tab3.Columns.Add("GDEADLINEDAYS", GetType(String))
        T00016tbl_tab3.Columns.Add("SEIKYUMATCHYMD", GetType(String))
        T00016tbl_tab3.Columns.Add("SHIHARAISITE", GetType(String))
        T00016tbl_tab3.Columns.Add("SHIHARAIYMD", GetType(String))
        T00016tbl_tab3.Columns.Add("BANKCODE", GetType(String))
        T00016tbl_tab3.Columns.Add("SEIKYUKBN", GetType(String))
        T00016tbl_tab3.Columns.Add("NIPPONO", GetType(String))
        T00016tbl_tab3.Columns.Add("ORDERNO", GetType(String))
        T00016tbl_tab3.Columns.Add("SHUKODATE", GetType(String))
        T00016tbl_tab3.Columns.Add("SHUKADATE", GetType(String))
        T00016tbl_tab3.Columns.Add("TODOKEDATE", GetType(String))
        T00016tbl_tab3.Columns.Add("SHUKABASHO", GetType(String))
        T00016tbl_tab3.Columns.Add("SHUKACITIES", GetType(String))
        T00016tbl_tab3.Columns.Add("TODOKECITIES", GetType(String))
        T00016tbl_tab3.Columns.Add("SHARYOTYPEF", GetType(String))
        T00016tbl_tab3.Columns.Add("TSHABANF", GetType(String))
        T00016tbl_tab3.Columns.Add("SHARYOTYPEB", GetType(String))
        T00016tbl_tab3.Columns.Add("TSHABANB", GetType(String))
        T00016tbl_tab3.Columns.Add("SHARYOTYPEB2", GetType(String))
        T00016tbl_tab3.Columns.Add("TSHABANB2", GetType(String))
        T00016tbl_tab3.Columns.Add("SHARYOKBN", GetType(String))
        T00016tbl_tab3.Columns.Add("SHAFUKU", GetType(String))
        T00016tbl_tab3.Columns.Add("TRIPNO", GetType(String))
        T00016tbl_tab3.Columns.Add("DROPNO", GetType(String))
        T00016tbl_tab3.Columns.Add("STAFFSU", GetType(String))
        T00016tbl_tab3.Columns.Add("STAFFCODE", GetType(String))
        T00016tbl_tab3.Columns.Add("SUBSTAFFCODE", GetType(String))
        T00016tbl_tab3.Columns.Add("OILTYPE", GetType(String))
        T00016tbl_tab3.Columns.Add("PRODUCTCODE", GetType(String))
        T00016tbl_tab3.Columns.Add("TUKORYOKBN", GetType(String))
        T00016tbl_tab3.Columns.Add("TUKORYO", GetType(String))
        T00016tbl_tab3.Columns.Add("TRIPSTTIME", GetType(String))
        T00016tbl_tab3.Columns.Add("TRIPENDTIME", GetType(String))
        T00016tbl_tab3.Columns.Add("KYUYU", GetType(String))
        T00016tbl_tab3.Columns.Add("UNCHINDISTANCE", GetType(String))
        T00016tbl_tab3.Columns.Add("KEIRYONO", GetType(String))
        T00016tbl_tab3.Columns.Add("JSURYO", GetType(String))
        T00016tbl_tab3.Columns.Add("JTANI", GetType(String))
        T00016tbl_tab3.Columns.Add("UNCHINCALCKBN", GetType(String))
        T00016tbl_tab3.Columns.Add("ROUNDTRIPDISTANCEAUTO", GetType(String))
        T00016tbl_tab3.Columns.Add("ROUNDTRIPDISTANCEHAND", GetType(String))
        T00016tbl_tab3.Columns.Add("ROUNDTRIPDISTANCE", GetType(String))
        T00016tbl_tab3.Columns.Add("UNKOUKAISUAUTO", GetType(String))
        T00016tbl_tab3.Columns.Add("UNKOUKAISUHAND", GetType(String))
        T00016tbl_tab3.Columns.Add("UNKOUKAISU", GetType(String))
        T00016tbl_tab3.Columns.Add("UNKOUNISSUAUTO", GetType(String))
        T00016tbl_tab3.Columns.Add("UNKOUNISSUHAND", GetType(String))
        T00016tbl_tab3.Columns.Add("UNKOUNISSU", GetType(String))
        T00016tbl_tab3.Columns.Add("PUBLICHOLIDAYNISSUAUTO", GetType(String))
        T00016tbl_tab3.Columns.Add("PUBLICHOLIDAYNISSUHAND", GetType(String))
        T00016tbl_tab3.Columns.Add("PUBLICHOLIDAYNISSU", GetType(String))
        T00016tbl_tab3.Columns.Add("PUBLICHOLIDAYKADONISSUAUTO", GetType(String))
        T00016tbl_tab3.Columns.Add("PUBLICHOLIDAYKADONISSUHAND", GetType(String))
        T00016tbl_tab3.Columns.Add("PUBLICHOLIDAYKADONISSU", GetType(String))
        T00016tbl_tab3.Columns.Add("NENMATUNEMSHINISSUAUTO", GetType(String))
        T00016tbl_tab3.Columns.Add("NENMATUNEMSHINISSUHAND", GetType(String))
        T00016tbl_tab3.Columns.Add("NENMATUNEMSHINISSU", GetType(String))
        T00016tbl_tab3.Columns.Add("KEIYAKUDAISUAUTO", GetType(String))
        T00016tbl_tab3.Columns.Add("KEIYAKUDAISUHAND", GetType(String))
        T00016tbl_tab3.Columns.Add("KEIYAKUDAISU", GetType(String))
        T00016tbl_tab3.Columns.Add("SURYOAUTO", GetType(String))
        T00016tbl_tab3.Columns.Add("SURYOHAND", GetType(String))
        T00016tbl_tab3.Columns.Add("SURYO", GetType(String))
        T00016tbl_tab3.Columns.Add("AMTAUTO", GetType(String))
        T00016tbl_tab3.Columns.Add("AMTHAND", GetType(String))
        T00016tbl_tab3.Columns.Add("AMT", GetType(String))
        T00016tbl_tab3.Columns.Add("RELATIONNO", GetType(String))
        T00016tbl_tab3.Columns.Add("DELFLG", GetType(String))
        T00016tbl_tab3.Columns.Add("BUNRUI", GetType(String))
        T00016tbl_tab3.Columns.Add("SOUSA", GetType(String))
        T00016tbl_tab3.Columns.Add("TANKA", GetType(String))
        T00016tbl_tab3.Columns.Add("KINGAKU", GetType(String))
        T00016tbl_tab3.Columns.Add("CAMPCODENAME", GetType(String))
        T00016tbl_tab3.Columns.Add("TORICODENAME", GetType(String))
        T00016tbl_tab3.Columns.Add("SHUKABASHONAME", GetType(String))
        T00016tbl_tab3.Columns.Add("TODOKECODENAME", GetType(String))
        T00016tbl_tab3.Columns.Add("NSHABANNAME", GetType(String))
        T00016tbl_tab3.Columns.Add("SHAFUKUNAME", GetType(String))
        T00016tbl_tab3.Columns.Add("BUNRUINAME", GetType(String))
        T00016tbl_tab3.Columns.Add("SOUSANAME", GetType(String))
        T00016tbl_tab3.Columns.Add("TORIHIKIORGNAME", GetType(String))
        T00016tbl_tab3.Columns.Add("TAISHOYM_SUM", GetType(String))
        T00016tbl_tab3.Columns.Add("BUNRUI_SUM", GetType(String))
        T00016tbl_tab3.Columns.Add("HAISOSAKI_SUM", GetType(String))
        T00016tbl_tab3.Columns.Add("KOUMOKU_SUM", GetType(String))
        T00016tbl_tab3.Columns.Add("TANKA_SUM", GetType(String))
        T00016tbl_tab3.Columns.Add("TONSU_SUM", GetType(String))
        T00016tbl_tab3.Columns.Add("DAISU_SUM", GetType(String))
        T00016tbl_tab3.Columns.Add("KINGAKU_SUM", GetType(String))
        T00016tbl_tab3.Columns.Add("KYUWARI_SUM", GetType(String))
        T00016tbl_tab3.Columns.Add("SHOKEI_SUM", GetType(String))
        T00016tbl_tab3.Columns.Add("SHOHIZEI_SUM", GetType(String))
        T00016tbl_tab3.Columns.Add("GOKEI_SUM", GetType(String))

    End Sub


    ''' <summary>
    ''' T00016tbl_tab4カラム設定
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub T00016tbl_tab4_ColumnsAdd()

        If T00016tbl_tab4.Columns.Count = 0 Then
        Else
            T00016tbl_tab4.Columns.Clear()
        End If

        'T00008テンポラリDB項目作成
        T00016tbl_tab4.Clear()
        T00016tbl_tab4.Columns.Add("LINECNT", GetType(Integer))            'DBの固定フィールド
        T00016tbl_tab4.Columns.Add("OPERATION", GetType(String))           'DBの固定フィールド
        T00016tbl_tab4.Columns.Add("TIMSTP", GetType(String))              'DBの固定フィールド
        T00016tbl_tab4.Columns.Add("SELECT", GetType(Integer))             'DBの固定フィールド
        T00016tbl_tab4.Columns.Add("HIDDEN", GetType(Integer))             'DBの固定フィールド

        T00016tbl_tab4.Columns.Add("INDEX", GetType(String))
        T00016tbl_tab4.Columns.Add("WORK_NO", GetType(String))

        T00016tbl_tab4.Columns.Add("CAMPCODE", GetType(String))
        T00016tbl_tab4.Columns.Add("DENKBN", GetType(String))
        T00016tbl_tab4.Columns.Add("DENNO", GetType(String))
        T00016tbl_tab4.Columns.Add("TORIHIKIYMD", GetType(String))
        T00016tbl_tab4.Columns.Add("RECODEKBN", GetType(String))
        T00016tbl_tab4.Columns.Add("TORICODE", GetType(String))
        T00016tbl_tab4.Columns.Add("TODOKECODE", GetType(String))
        T00016tbl_tab4.Columns.Add("GSHABAN", GetType(String))
        T00016tbl_tab4.Columns.Add("NSHABAN", GetType(String))
        T00016tbl_tab4.Columns.Add("UNCHINCODE", GetType(String))
        T00016tbl_tab4.Columns.Add("DETAILNO", GetType(String))
        T00016tbl_tab4.Columns.Add("ENTRYDATE", GetType(String))
        T00016tbl_tab4.Columns.Add("ACTORICODE", GetType(String))
        T00016tbl_tab4.Columns.Add("URIKBN", GetType(String))
        T00016tbl_tab4.Columns.Add("TORIHIKIMANGORG", GetType(String))
        T00016tbl_tab4.Columns.Add("TORIHIKIORG", GetType(String))
        T00016tbl_tab4.Columns.Add("SEIKYUSHIHARAIMANGORG", GetType(String))
        T00016tbl_tab4.Columns.Add("SEIKYUSHIHARAIORG", GetType(String))
        T00016tbl_tab4.Columns.Add("SEIKYUSHIHARAIYM", GetType(String))
        T00016tbl_tab4.Columns.Add("URIKEIJYOYMD", GetType(String))
        T00016tbl_tab4.Columns.Add("SEIKYUNO", GetType(String))
        T00016tbl_tab4.Columns.Add("URIPATERNKBN", GetType(String))
        T00016tbl_tab4.Columns.Add("URIPATTERNCODE", GetType(String))
        T00016tbl_tab4.Columns.Add("URIAMT", GetType(String))
        T00016tbl_tab4.Columns.Add("URITAXAMT", GetType(String))
        T00016tbl_tab4.Columns.Add("URISEGMENT1", GetType(String))
        T00016tbl_tab4.Columns.Add("URISEGMENT2", GetType(String))
        T00016tbl_tab4.Columns.Add("URISEGMENT3", GetType(String))
        T00016tbl_tab4.Columns.Add("NDEADLINEDAYS", GetType(String))
        T00016tbl_tab4.Columns.Add("JOTSEIKYUKBN", GetType(String))
        T00016tbl_tab4.Columns.Add("SEIKYUOUTYMD", GetType(String))
        T00016tbl_tab4.Columns.Add("NYUKINSITE", GetType(String))
        T00016tbl_tab4.Columns.Add("NYUKINYMD", GetType(String))
        T00016tbl_tab4.Columns.Add("SHIHARAIKEIJYOYMD", GetType(String))
        T00016tbl_tab4.Columns.Add("SHIHARAINO", GetType(String))
        T00016tbl_tab4.Columns.Add("SHIHARAIPATERNKBN", GetType(String))
        T00016tbl_tab4.Columns.Add("SHIHARAIPATTERNCODE", GetType(String))
        T00016tbl_tab4.Columns.Add("SHIHARAIAMT", GetType(String))
        T00016tbl_tab4.Columns.Add("SHIHARAITAXAMT", GetType(String))
        T00016tbl_tab4.Columns.Add("SHIHARAISEGMENT1", GetType(String))
        T00016tbl_tab4.Columns.Add("SHIHARAISEGMENT2", GetType(String))
        T00016tbl_tab4.Columns.Add("SHIHARAISEGMENT3", GetType(String))
        T00016tbl_tab4.Columns.Add("GDEADLINEDAYS", GetType(String))
        T00016tbl_tab4.Columns.Add("SEIKYUMATCHYMD", GetType(String))
        T00016tbl_tab4.Columns.Add("SHIHARAISITE", GetType(String))
        T00016tbl_tab4.Columns.Add("SHIHARAIYMD", GetType(String))
        T00016tbl_tab4.Columns.Add("BANKCODE", GetType(String))
        T00016tbl_tab4.Columns.Add("SEIKYUKBN", GetType(String))
        T00016tbl_tab4.Columns.Add("NIPPONO", GetType(String))
        T00016tbl_tab4.Columns.Add("ORDERNO", GetType(String))
        T00016tbl_tab4.Columns.Add("SHUKODATE", GetType(String))
        T00016tbl_tab4.Columns.Add("SHUKADATE", GetType(String))
        T00016tbl_tab4.Columns.Add("TODOKEDATE", GetType(String))
        T00016tbl_tab4.Columns.Add("SHUKABASHO", GetType(String))
        T00016tbl_tab4.Columns.Add("SHUKACITIES", GetType(String))
        T00016tbl_tab4.Columns.Add("TODOKECITIES", GetType(String))
        T00016tbl_tab4.Columns.Add("SHARYOTYPEF", GetType(String))
        T00016tbl_tab4.Columns.Add("TSHABANF", GetType(String))
        T00016tbl_tab4.Columns.Add("SHARYOTYPEB", GetType(String))
        T00016tbl_tab4.Columns.Add("TSHABANB", GetType(String))
        T00016tbl_tab4.Columns.Add("SHARYOTYPEB2", GetType(String))
        T00016tbl_tab4.Columns.Add("TSHABANB2", GetType(String))
        T00016tbl_tab4.Columns.Add("SHARYOKBN", GetType(String))
        T00016tbl_tab4.Columns.Add("SHAFUKU", GetType(String))
        T00016tbl_tab4.Columns.Add("TRIPNO", GetType(String))
        T00016tbl_tab4.Columns.Add("DROPNO", GetType(String))
        T00016tbl_tab4.Columns.Add("STAFFSU", GetType(String))
        T00016tbl_tab4.Columns.Add("STAFFCODE", GetType(String))
        T00016tbl_tab4.Columns.Add("SUBSTAFFCODE", GetType(String))
        T00016tbl_tab4.Columns.Add("OILTYPE", GetType(String))
        T00016tbl_tab4.Columns.Add("PRODUCTCODE", GetType(String))
        T00016tbl_tab4.Columns.Add("TUKORYOKBN", GetType(String))
        T00016tbl_tab4.Columns.Add("TUKORYO", GetType(String))
        T00016tbl_tab4.Columns.Add("TRIPSTTIME", GetType(String))
        T00016tbl_tab4.Columns.Add("TRIPENDTIME", GetType(String))
        T00016tbl_tab4.Columns.Add("KYUYU", GetType(String))
        T00016tbl_tab4.Columns.Add("UNCHINDISTANCE", GetType(String))
        T00016tbl_tab4.Columns.Add("KEIRYONO", GetType(String))
        T00016tbl_tab4.Columns.Add("JSURYO", GetType(String))
        T00016tbl_tab4.Columns.Add("JTANI", GetType(String))
        T00016tbl_tab4.Columns.Add("UNCHINCALCKBN", GetType(String))
        T00016tbl_tab4.Columns.Add("ROUNDTRIPDISTANCEAUTO", GetType(String))
        T00016tbl_tab4.Columns.Add("ROUNDTRIPDISTANCEHAND", GetType(String))
        T00016tbl_tab4.Columns.Add("ROUNDTRIPDISTANCE", GetType(String))
        T00016tbl_tab4.Columns.Add("UNKOUKAISUAUTO", GetType(String))
        T00016tbl_tab4.Columns.Add("UNKOUKAISUHAND", GetType(String))
        T00016tbl_tab4.Columns.Add("UNKOUKAISU", GetType(String))
        T00016tbl_tab4.Columns.Add("UNKOUNISSUAUTO", GetType(String))
        T00016tbl_tab4.Columns.Add("UNKOUNISSUHAND", GetType(String))
        T00016tbl_tab4.Columns.Add("UNKOUNISSU", GetType(String))
        T00016tbl_tab4.Columns.Add("PUBLICHOLIDAYNISSUAUTO", GetType(String))
        T00016tbl_tab4.Columns.Add("PUBLICHOLIDAYNISSUHAND", GetType(String))
        T00016tbl_tab4.Columns.Add("PUBLICHOLIDAYNISSU", GetType(String))
        T00016tbl_tab4.Columns.Add("PUBLICHOLIDAYKADONISSUAUTO", GetType(String))
        T00016tbl_tab4.Columns.Add("PUBLICHOLIDAYKADONISSUHAND", GetType(String))
        T00016tbl_tab4.Columns.Add("PUBLICHOLIDAYKADONISSU", GetType(String))
        T00016tbl_tab4.Columns.Add("NENMATUNEMSHINISSUAUTO", GetType(String))
        T00016tbl_tab4.Columns.Add("NENMATUNEMSHINISSUHAND", GetType(String))
        T00016tbl_tab4.Columns.Add("NENMATUNEMSHINISSU", GetType(String))
        T00016tbl_tab4.Columns.Add("KEIYAKUDAISUAUTO", GetType(String))
        T00016tbl_tab4.Columns.Add("KEIYAKUDAISUHAND", GetType(String))
        T00016tbl_tab4.Columns.Add("KEIYAKUDAISU", GetType(String))
        T00016tbl_tab4.Columns.Add("SURYOAUTO", GetType(String))
        T00016tbl_tab4.Columns.Add("SURYOHAND", GetType(String))
        T00016tbl_tab4.Columns.Add("SURYO", GetType(String))
        T00016tbl_tab4.Columns.Add("AMTAUTO", GetType(String))
        T00016tbl_tab4.Columns.Add("AMTHAND", GetType(String))
        T00016tbl_tab4.Columns.Add("AMT", GetType(String))
        T00016tbl_tab4.Columns.Add("RELATIONNO", GetType(String))
        T00016tbl_tab4.Columns.Add("DELFLG", GetType(String))
        T00016tbl_tab4.Columns.Add("BUNRUI", GetType(String))
        T00016tbl_tab4.Columns.Add("SOUSA", GetType(String))
        T00016tbl_tab4.Columns.Add("TANKA", GetType(String))
        T00016tbl_tab4.Columns.Add("KINGAKU", GetType(String))
        T00016tbl_tab4.Columns.Add("CAMPCODENAME", GetType(String))
        T00016tbl_tab4.Columns.Add("TORICODENAME", GetType(String))
        T00016tbl_tab4.Columns.Add("SHUKABASHONAME", GetType(String))
        T00016tbl_tab4.Columns.Add("TODOKECODENAME", GetType(String))
        T00016tbl_tab4.Columns.Add("NSHABANNAME", GetType(String))
        T00016tbl_tab4.Columns.Add("SHAFUKUNAME", GetType(String))
        T00016tbl_tab4.Columns.Add("BUNRUINAME", GetType(String))
        T00016tbl_tab4.Columns.Add("SOUSANAME", GetType(String))
        T00016tbl_tab4.Columns.Add("TORIHIKIORGNAME", GetType(String))
        T00016tbl_tab4.Columns.Add("TAISHOYM_SUM", GetType(String))
        T00016tbl_tab4.Columns.Add("BUNRUI_SUM", GetType(String))
        T00016tbl_tab4.Columns.Add("HAISOSAKI_SUM", GetType(String))
        T00016tbl_tab4.Columns.Add("KOUMOKU_SUM", GetType(String))
        T00016tbl_tab4.Columns.Add("TANKA_SUM", GetType(String))
        T00016tbl_tab4.Columns.Add("TONSU_SUM", GetType(String))
        T00016tbl_tab4.Columns.Add("DAISU_SUM", GetType(String))
        T00016tbl_tab4.Columns.Add("KINGAKU_SUM", GetType(String))
        T00016tbl_tab4.Columns.Add("KYUWARI_SUM", GetType(String))
        T00016tbl_tab4.Columns.Add("SHOKEI_SUM", GetType(String))
        T00016tbl_tab4.Columns.Add("SHOHIZEI_SUM", GetType(String))
        T00016tbl_tab4.Columns.Add("GOKEI_SUM", GetType(String))

    End Sub


    ''' <summary>
    ''' T00016tbl追加
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub DBupdate_T16INSERT(ByVal I_DATENOW As Date, ByRef O_RTN As String)

        ''DataBase接続文字
        'Dim SQLcon = CS0050SESSION.getConnection
        'SQLcon.Open() 'DataBase接続(Open)

        'Dim WW_SORTstr As String = ""
        'Dim WW_FILLstr As String = ""

        'Dim WW_TORICODE As String = ""
        'Dim WW_OILTYPE As String = ""
        'Dim WW_SHUKADATE As String = ""
        'Dim WW_KIJUNDATE As String = ""
        'Dim WW_SHIPORG As String = ""


        ''■■■ T00016UPDtblより配送受注追加 ■■■
        ''
        'For Each T00016UPDrow In T00016UPDtbl.Rows

        '    If T00016UPDrow("DELFLG") = "0" AndAlso
        '        (T00016UPDrow("OPERATION") = C_LIST_OPERATION_CODE.UPDATING OrElse T00016UPDrow("OPERATION") = C_LIST_OPERATION_CODE.WARNING) Then
        '        Try
        '            '〇配送受注DB登録
        '            Dim SQLStr As String =
        '                       " INSERT INTO T0016_TORIHIKI                 " _
        '                    & "             (CAMPCODE,                      " _
        '                    & "              DENKBN,                        " _
        '                    & "              DENNO,                         " _
        '                    & "              TORIHIKIYMD,                   " _
        '                    & "              RECODEKBN,                     " _
        '                    & "              TORICODE,                      " _
        '                    & "              TODOKECODE,                    " _
        '                    & "              GSHABAN,                       " _
        '                    & "              NSHABAN,                       " _
        '                    & "              UNCHINCODE,                    " _
        '                    & "              DETAILNO,                      " _
        '                    & "              ENTRYDATE,                     " _
        '                    & "              ACTORICODE,                    " _
        '                    & "              URIKBN,                        " _
        '                    & "              TORIHIKIMANGORG,               " _
        '                    & "              TORIHIKIORG,                   " _
        '                    & "              SEIKYUSHIHARAIMANGORG,         " _
        '                    & "              SEIKYUSHIHARAIORG,             " _
        '                    & "              SEIKYUSHIHARAIYM,              " _
        '                    & "              URIKEIJYOYMD,                  " _
        '                    & "              SEIKYUNO,                      " _
        '                    & "              URIPATERNKBN,                  " _
        '                    & "              URIPATTERNCODE,                " _
        '                    & "              URIAMT,                        " _
        '                    & "              URITAXAMT,                     " _
        '                    & "              URISEGMENT1,                   " _
        '                    & "              URISEGMENT2,                   " _
        '                    & "              URISEGMENT3,                   " _
        '                    & "              NDEADLINEDAYS,                 " _
        '                    & "              JOTSEIKYUKBN,                  " _
        '                    & "              SEIKYUOUTYMD,                  " _
        '                    & "              NYUKINSITE,                    " _
        '                    & "              NYUKINYMD,                     " _
        '                    & "              SHIHARAIKEIJYOYMD,             " _
        '                    & "              SHIHARAINO,                    " _
        '                    & "              SHIHARAIPATERNKBN,             " _
        '                    & "              SHIHARAIPATTERNCODE,           " _
        '                    & "              SHIHARAIAMT,                   " _
        '                    & "              SHIHARAITAXAMT,                " _
        '                    & "              SHIHARAISEGMENT1,              " _
        '                    & "              SHIHARAISEGMENT2,              " _
        '                    & "              SHIHARAISEGMENT3,              " _
        '                    & "              GDEADLINEDAYS,                 " _
        '                    & "              SEIKYUMATCHYMD,                " _
        '                    & "              SHIHARAISITE,                  " _
        '                    & "              SHIHARAIYMD,                   " _
        '                    & "              BANKCODE,                      " _
        '                    & "              SEIKYUKBN,                     " _
        '                    & "              NIPPONO,                       " _
        '                    & "              ORDERNO,                       " _
        '                    & "              SHUKODATE,                     " _
        '                    & "              SHUKADATE,                     " _
        '                    & "              TODOKEDATE,                    " _
        '                    & "              SHUKABASHO,                    " _
        '                    & "              SHUKACITIES,                   " _
        '                    & "              TODOKECITIES,                  " _
        '                    & "              SHARYOTYPEF,                   " _
        '                    & "              TSHABANF,                      " _
        '                    & "              SHARYOTYPEB,                   " _
        '                    & "              TSHABANB,                      " _
        '                    & "              SHARYOTYPEB2,                  " _
        '                    & "              TSHABANB2,                     " _
        '                    & "              SHARYOKBN,                     " _
        '                    & "              SHAFUKU,                       " _
        '                    & "              TRIPNO,                        " _
        '                    & "              DROPNO,                        " _
        '                    & "              STAFFSU,                       " _
        '                    & "              STAFFCODE,                     " _
        '                    & "              SUBSTAFFCODE,                  " _
        '                    & "              OILTYPE,                       " _
        '                    & "              PRODUCTCODE,                   " _
        '                    & "              TUKORYOKBN,                    " _
        '                    & "              TUKORYO,                       " _
        '                    & "              TRIPSTTIME,                    " _
        '                    & "              TRIPENDTIME,                   " _
        '                    & "              KYUYU,                         " _
        '                    & "              UNCHINDISTANCE,                " _
        '                    & "              KEIRYONO,                      " _
        '                    & "              JSURYO,                        " _
        '                    & "              JTANI,                         " _
        '                    & "              UNCHINCALCKBN,                 " _
        '                    & "              ROUNDTRIPDISTANCEAUTO,         " _
        '                    & "              ROUNDTRIPDISTANCEHAND,         " _
        '                    & "              ROUNDTRIPDISTANCE,             " _
        '                    & "              UNKOUKAISUAUTO,                " _
        '                    & "              UNKOUKAISUHAND,                " _
        '                    & "              UNKOUKAISU,                    " _
        '                    & "              UNKOUNISSUAUTO,                " _
        '                    & "              UNKOUNISSUHAND,                " _
        '                    & "              UNKOUNISSU,                    " _
        '                    & "              PUBLICHOLIDAYNISSUAUTO,        " _
        '                    & "              PUBLICHOLIDAYNISSUHAND,        " _
        '                    & "              PUBLICHOLIDAYNISSU,            " _
        '                    & "              PUBLICHOLIDAYKADONISSUAUTO,    " _
        '                    & "              PUBLICHOLIDAYKADONISSUHAND,    " _
        '                    & "              PUBLICHOLIDAYKADONISSU,        " _
        '                    & "              NENMATUNEMSHINISSUAUTO,        " _
        '                    & "              NENMATUNEMSHINISSUHAND,        " _
        '                    & "              NENMATUNEMSHINISSU,            " _
        '                    & "              KEIYAKUDAISUAUTO,              " _
        '                    & "              KEIYAKUDAISUHAND,              " _
        '                    & "              KEIYAKUDAISU,                  " _
        '                    & "              SURYOAUTO,                     " _
        '                    & "              SURYOHAND,                     " _
        '                    & "              SURYO,                         " _
        '                    & "              AMTAUTO,                       " _
        '                    & "              AMTHAND,                       " _
        '                    & "              AMT,                           " _
        '                    & "              RELATIONNO,                    " _
        '                    & "              DELFLG,                        " _
        '                    & "              INITYMD,                       " _
        '                    & "              UPDYMD,                        " _
        '                    & "              UPDUSER,                       " _
        '                    & "              UPDTERMID,                     " _
        '                    & "              RECEIVEYMD,                    " _
        '                    & "              UPDTIMSTP)                     "
        '                    & "      VALUES (@P01,@P02,@P03,@P04,@P05,@P06,@P07,@P08,@P09,@P10,               " _
        '                    & "              @P11,@P12,@P13,@P14,@P15,@P16,@P17,@P18,@P19,@P20,               " _
        '                    & "              @P21,@P22,@P23,@P24,@P25,@P26,@P27,@P28,@P29,@P30,               " _
        '                    & "              @P31,@P32,@P33,@P34,@P35,@P36,@P37,@P38,@P39,@P40,               " _
        '                    & "              @P41,@P42,@P43,@P44,@P45,@P46,@P47,@P48,@P49,@P50,               " _
        '                    & "              @P51,@P52,@P53,@P54,@P55,@P56,@P57,@P58,@P59,@P60,               " _
        '                    & "              @P61,@P62,@P63,@P64,@P65,@P66,@P67,@P68,@P69,@P70,               " _
        '                    & "              @P71,@P72,@P73,@P74,@P75,@P76,@P77,@P78,@P79,@P80,               " _
        '                    & "              @P81,@P82,@P83,@P84,@P85,@P86,@P87,@P88,@P89,@P90,               " _
        '                    & "              @P91,@P92,@P93,@P94,@P95,@P96,@P97,@P98,@P99,@P100,              " _
        '                    & "              @P101,@P102,@P103,@P104,@P105,@P106,@P107,@P108,@P109,@P110,     " _
        '                    & "              @P111,@P112,@P113,@P114,@P115,@P116,@P117,@P118,@P119,@P120,     " _
        '                    & "              @P121,@P122,@P123,@P124,@P125,@P126,@P127,@P128,@P129,@P130,     " _
        '                    & "              @P131,@P132,@P133,@P134,@P135,@P136,@P137,@P138,@P139,@P140,     " _
        '                    & "              @P141,@P142,@P143,@P144,@P145,@P146,@P147,@P148,@P149,@P150,     " _
        '                    & "              @P151,@P152,@P153,@P154,@P155,@P156,@P157,@P158,@P159,@P160,     " _
        '                    & "              @P161,@P162,@P163,@P164,@P165,@P166,@P167,@P168,@P169,@P170,     " _
        '                    & "              @P171,@P172,@P173,@P174,@P175,@P176,@P177,@P178,@P179,@P180,     " _
        '                    & "              @P181,@P182,@P183,@P184,@P185,@P186,@P187,@P188                  " _
        '                    & "              );    "

        '            Dim SQLcmd As New SqlCommand(SQLStr, SQLcon)
        '            Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", System.Data.SqlDbType.NVarChar, 20)
        '            Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", System.Data.SqlDbType.NVarChar, 10)
        '            Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@P03", System.Data.SqlDbType.NVarChar, 10)
        '            Dim PARA04 As SqlParameter = SQLcmd.Parameters.Add("@P04", System.Data.SqlDbType.NVarChar, 10)
        '            Dim PARA05 As SqlParameter = SQLcmd.Parameters.Add("@P05", System.Data.SqlDbType.NVarChar, 10)
        '            Dim PARA06 As SqlParameter = SQLcmd.Parameters.Add("@P06", System.Data.SqlDbType.NVarChar, 2)
        '            Dim PARA07 As SqlParameter = SQLcmd.Parameters.Add("@P07", System.Data.SqlDbType.NVarChar, 25)
        '            Dim PARA08 As SqlParameter = SQLcmd.Parameters.Add("@P08", System.Data.SqlDbType.NVarChar, 20)
        '            Dim PARA09 As SqlParameter = SQLcmd.Parameters.Add("@P09", System.Data.SqlDbType.NVarChar, 20)
        '            Dim PARA10 As SqlParameter = SQLcmd.Parameters.Add("@P10", System.Data.SqlDbType.DateTime)
        '            Dim PARA11 As SqlParameter = SQLcmd.Parameters.Add("@P11", System.Data.SqlDbType.DateTime)
        '            Dim PARA12 As SqlParameter = SQLcmd.Parameters.Add("@P12", System.Data.SqlDbType.DateTime)
        '            Dim PARA13 As SqlParameter = SQLcmd.Parameters.Add("@P13", System.Data.SqlDbType.NVarChar, 20)
        '            Dim PARA14 As SqlParameter = SQLcmd.Parameters.Add("@P14", System.Data.SqlDbType.NVarChar, 20)
        '            Dim PARA15 As SqlParameter = SQLcmd.Parameters.Add("@P15", System.Data.SqlDbType.NVarChar, 20)
        '            Dim PARA16 As SqlParameter = SQLcmd.Parameters.Add("@P16", System.Data.SqlDbType.NVarChar, 1)
        '            Dim PARA17 As SqlParameter = SQLcmd.Parameters.Add("@P17", System.Data.SqlDbType.Decimal)
        '            Dim PARA18 As SqlParameter = SQLcmd.Parameters.Add("@P18", System.Data.SqlDbType.NVarChar, 20)
        '            Dim PARA19 As SqlParameter = SQLcmd.Parameters.Add("@P19", System.Data.SqlDbType.NVarChar, 20)
        '            Dim PARA20 As SqlParameter = SQLcmd.Parameters.Add("@P20", System.Data.SqlDbType.DateTime)
        '            Dim PARA21 As SqlParameter = SQLcmd.Parameters.Add("@P21", System.Data.SqlDbType.NVarChar, 20)
        '            Dim PARA22 As SqlParameter = SQLcmd.Parameters.Add("@P22", System.Data.SqlDbType.NVarChar, 20)
        '            Dim PARA23 As SqlParameter = SQLcmd.Parameters.Add("@P23", System.Data.SqlDbType.NVarChar, 20)
        '            Dim PARA24 As SqlParameter = SQLcmd.Parameters.Add("@P24", System.Data.SqlDbType.NVarChar, 20)
        '            Dim PARA25 As SqlParameter = SQLcmd.Parameters.Add("@P25", System.Data.SqlDbType.Decimal)
        '            Dim PARA26 As SqlParameter = SQLcmd.Parameters.Add("@P26", System.Data.SqlDbType.Int)
        '            Dim PARA27 As SqlParameter = SQLcmd.Parameters.Add("@P27", System.Data.SqlDbType.NVarChar, 50)
        '            Dim PARA28 As SqlParameter = SQLcmd.Parameters.Add("@P28", System.Data.SqlDbType.NVarChar, 50)
        '            Dim PARA29 As SqlParameter = SQLcmd.Parameters.Add("@P29", System.Data.SqlDbType.NVarChar, 50)
        '            Dim PARA30 As SqlParameter = SQLcmd.Parameters.Add("@P30", System.Data.SqlDbType.NVarChar, 50)
        '            Dim PARA31 As SqlParameter = SQLcmd.Parameters.Add("@P31", System.Data.SqlDbType.NVarChar, 50)
        '            Dim PARA32 As SqlParameter = SQLcmd.Parameters.Add("@P32", System.Data.SqlDbType.NVarChar, 50)
        '            Dim PARA33 As SqlParameter = SQLcmd.Parameters.Add("@P33", System.Data.SqlDbType.NVarChar, 1)
        '            Dim PARA34 As SqlParameter = SQLcmd.Parameters.Add("@P34", System.Data.SqlDbType.DateTime)
        '            Dim PARA35 As SqlParameter = SQLcmd.Parameters.Add("@P35", System.Data.SqlDbType.DateTime)
        '            Dim PARA36 As SqlParameter = SQLcmd.Parameters.Add("@P36", System.Data.SqlDbType.NVarChar, 20)
        '            Dim PARA37 As SqlParameter = SQLcmd.Parameters.Add("@P37", System.Data.SqlDbType.NVarChar, 30)
        '            Dim PARA38 As SqlParameter = SQLcmd.Parameters.Add("@P38", System.Data.SqlDbType.DateTime)
        '            Dim PARA39 As SqlParameter = SQLcmd.Parameters.Add("@P39", System.Data.SqlDbType.DateTime)
        '            Dim PARA40 As SqlParameter = SQLcmd.Parameters.Add("@P40", System.Data.SqlDbType.NVarChar, 1)
        '            Dim PARA41 As SqlParameter = SQLcmd.Parameters.Add("@P41", System.Data.SqlDbType.NVarChar, 20)
        '            Dim PARA42 As SqlParameter = SQLcmd.Parameters.Add("@P42", System.Data.SqlDbType.NVarChar, 1)
        '            Dim PARA43 As SqlParameter = SQLcmd.Parameters.Add("@P43", System.Data.SqlDbType.NVarChar, 20)
        '            Dim PARA44 As SqlParameter = SQLcmd.Parameters.Add("@P44", System.Data.SqlDbType.NVarChar, 1)
        '            Dim PARA45 As SqlParameter = SQLcmd.Parameters.Add("@P45", System.Data.SqlDbType.NVarChar, 20)
        '            Dim PARA46 As SqlParameter = SQLcmd.Parameters.Add("@P46", System.Data.SqlDbType.NVarChar, 20)
        '            Dim PARA47 As SqlParameter = SQLcmd.Parameters.Add("@P47", System.Data.SqlDbType.NVarChar, 20)
        '            Dim PARA48 As SqlParameter = SQLcmd.Parameters.Add("@P48", System.Data.SqlDbType.NVarChar, 1)

        '            PARA01.Value = T00016UPDrow("CAMPCODE")                           '会社コード(CAMPCODE)
        '            PARA02.Value = T00016UPDrow("ORDERNO").PadLeft(7, "0")            '受注番号(ORDERNO)
        '            PARA03.Value = T00016UPDrow("DETAILNO").PadLeft(3, "0")           '明細№(DETAILNO)
        '            PARA04.Value = T00016UPDrow("TRIPNO").PadLeft(3, "0")             'トリップ(TRIPNO)
        '            PARA05.Value = T00016UPDrow("DROPNO").PadLeft(3, "0")             'ドロップ(DROPNO)
        '            PARA06.Value = T00016UPDrow("SEQ").PadLeft(2, "0")                '枝番(SEQ)
        '            PARA07.Value = I_DATENOW.ToString("yyyyMMddHHmmssfff")            'エントリー日時(ENTRYDATE)
        '            PARA08.Value = T00016UPDrow("TORICODE")                           '取引先コード(TORICODE)
        '            PARA09.Value = T00016UPDrow("OILTYPE")                            '油種(OILTYPE)
        '            If T00016UPDrow("SHUKODATE") = "" Then                            '出庫日(SHUKODATE)
        '                PARA10.Value = "2000/01/01"
        '            Else
        '                PARA10.Value = RTrim(T00016UPDrow("SHUKODATE"))
        '            End If
        '            If T00016UPDrow("KIKODATE") = "" Then                             '帰庫日(KIKODATE)
        '                PARA11.Value = "2000/01/01"
        '            Else
        '                PARA11.Value = RTrim(T00016UPDrow("KIKODATE"))
        '            End If
        '            If T00016UPDrow("SHUKADATE") = "" Then                            '出荷日(SHUKADATE)
        '                PARA12.Value = "2000/01/01"
        '            Else
        '                PARA12.Value = RTrim(T00016UPDrow("SHUKADATE"))
        '            End If
        '            PARA13.Value = T00016UPDrow("SHIPORG")                            '出荷部署(SHIPORG)
        '            PARA14.Value = T00016UPDrow("SHUKABASHO")                         '出荷場所(SHUKABASHO)
        '            PARA15.Value = T00016UPDrow("GSHABAN")                            '業務車番(GSHABAN)
        '            PARA16.Value = T00016UPDrow("RYOME")                              '両目(RYOME)
        '            If String.IsNullOrWhiteSpace(RTrim(T00016UPDrow("SHAFUKU"))) Then '車腹（積載量）(SHAFUKU)
        '                PARA17.Value = 0.0
        '            Else
        '                PARA17.Value = CType(T00016UPDrow("SHAFUKU"), Double)
        '            End If
        '            PARA18.Value = T00016UPDrow("STAFFCODE")                          '乗務員コード(STAFFCODE)
        '            PARA19.Value = T00016UPDrow("SUBSTAFFCODE")                       '副乗務員コード(SUBSTAFFCODE)
        '            If RTrim(T00016UPDrow("TODOKEDATE")) = "" Then                    '届日(TODOKEDATE)
        '                PARA20.Value = "2000/01/01"
        '            Else
        '                PARA20.Value = RTrim(T00016UPDrow("TODOKEDATE"))
        '            End If
        '            PARA21.Value = T00016UPDrow("TODOKECODE")                         '届先コード(TODOKECODE)
        '            PARA22.Value = T00016UPDrow("PRODUCT1")                           '品名１(PRODUCT1)
        '            PARA23.Value = T00016UPDrow("PRODUCT2")                           '品名２(PRODUCT2)
        '            PARA24.Value = T00016UPDrow("CONTNO")                             'コンテナ番号(CONTNO)
        '            If String.IsNullOrWhiteSpace(RTrim(T00016UPDrow("JSURYO"))) Then   '配送実績数量(JSURYO)
        '                PARA25.Value = 0.0
        '            Else
        '                PARA25.Value = CType(T00016UPDrow("JSURYO"), Double)
        '            End If
        '            If String.IsNullOrWhiteSpace(RTrim(T00016UPDrow("JDAISU"))) Then   '配送実績台数(JDAISU)
        '                PARA26.Value = 0
        '            Else
        '                PARA26.Value = CType(T00016UPDrow("JDAISU"), Double)
        '            End If
        '            PARA27.Value = T00016UPDrow("REMARKS1")                           '備考１(REMARKS1)
        '            PARA28.Value = T00016UPDrow("REMARKS2")                           '備考２(REMARKS2)
        '            PARA29.Value = T00016UPDrow("REMARKS3")                           '備考３(REMARKS3)
        '            PARA30.Value = T00016UPDrow("REMARKS4")                           '備考４(REMARKS4)
        '            PARA31.Value = T00016UPDrow("REMARKS5")                           '備考５(REMARKS5)
        '            PARA32.Value = T00016UPDrow("REMARKS6")                           '備考６(REMARKS6)
        '            PARA33.Value = T00016UPDrow("DELFLG")                             '削除フラグ(DELFLG)
        '            PARA34.Value = I_DATENOW                                          '登録年月日(INITYMD)
        '            PARA35.Value = I_DATENOW                                          '更新年月日(UPDYMD)
        '            PARA36.Value = Master.USERID                                      '更新ユーザＩＤ(UPDUSER)
        '            PARA37.Value = Master.USERTERMID                                  '更新端末(UPDTERMID)
        '            PARA38.Value = C_DEFAULT_YMD                                      '集信日時(RECEIVEYMD)

        '            '基準日＝出荷日
        '            If T00016UPDrow("KIJUNDATE") = "" Then                            '基準日(KIJUNDATE)
        '                PARA39.Value = "2000/01/01"
        '            Else
        '                PARA39.Value = RTrim(T00016UPDrow("KIJUNDATE"))
        '            End If
        '            PARA40.Value = T00016UPDrow("SHARYOTYPEF")                        '統一車番前(SHARYOTYPEF)
        '            PARA41.Value = T00016UPDrow("TSHABANF")                           '統一車番前(TSHABANF)
        '            PARA42.Value = T00016UPDrow("SHARYOTYPEB")                        '統一車番前(SHARYOTYPEB)
        '            PARA43.Value = T00016UPDrow("TSHABANB")                           '統一車番前(TSHABANB)
        '            PARA44.Value = T00016UPDrow("SHARYOTYPEB2")                       '統一車番前(SHARYOTYPEB2)
        '            PARA45.Value = T00016UPDrow("TSHABANB2")                          '統一車番前(TSHABANB2)
        '            PARA46.Value = ""                                                 '配送実績単位(STANI)
        '            PARA47.Value = T00016UPDrow("PRODUCTCODE")                        '品名コード(PRODUCTCODE)
        '            PARA48.Value = T00016UPDrow("JISSEKIKBN")                         '実績区分(JISSEKIKBN)

        '            SQLcmd.CommandTimeout = 300
        '            SQLcmd.ExecuteNonQuery()

        '            'CLOSE
        '            SQLcmd.Dispose()
        '            SQLcmd = Nothing

        '        Catch ex As Exception
        '            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "T0015_SUPPLJISSKI INSERT")
        '            CS0011LOGWRITE.INFSUBCLASS = "MAIN"                         'SUBクラス名
        '            CS0011LOGWRITE.INFPOSI = "DB:T0015_SUPPLJISSKI INSERT"           '
        '            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                  '
        '            CS0011LOGWRITE.TEXT = ex.ToString()
        '            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
        '            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
        '            O_RTN = C_MESSAGE_NO.DB_ERROR
        '            Exit Sub

        '        End Try

        '        '〇配送受注登録結果を画面情報へ戻す
        '        For Each T00016row In T00016tbl.Rows
        '            If T00016row("CAMPCODE") = T00016UPDrow("CAMPCODE") AndAlso
        '               T00016row("TORICODE") = T00016UPDrow("TORICODE") AndAlso
        '               T00016row("OILTYPE") = T00016UPDrow("OILTYPE") AndAlso
        '               T00016row("KIJUNDATE") = T00016UPDrow("KIJUNDATE") AndAlso
        '               T00016row("SHIPORG") = T00016UPDrow("SHIPORG") AndAlso
        '               T00016row("SHUKODATE") = T00016UPDrow("SHUKODATE") AndAlso
        '               T00016row("GSHABAN") = T00016UPDrow("GSHABAN") AndAlso
        '               T00016row("TRIPNO") = T00016UPDrow("TRIPNO") AndAlso
        '               T00016row("DROPNO") = T00016UPDrow("DROPNO") AndAlso
        '               T00016row("SEQ") = T00016UPDrow("SEQ") AndAlso
        '               T00016row("DELFLG") <> "1" Then

        '                T00016row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
        '                T00016row("ORDERNO") = T00016UPDrow("ORDERNO")
        '                T00016row("DETAILNO") = T00016UPDrow("DETAILNO")
        '                Exit For

        '            End If
        '        Next
        '        Try
        '            '更新結果(TIMSTP)再取得 …　連続処理を可能にする。
        '            Dim SQLStr As String =
        '                       " SELECT CAST(UPDTIMSTP as bigint) as TIMSTP    " _
        '                     & "   FROM T0015_SUPPLJISSKI                      " _
        '                     & "  WHERE CAMPCODE       = @P01                  " _
        '                     & "    and TORICODE       = @P02                  " _
        '                     & "    and OILTYPE        = @P03                  " _
        '                     & "    and KIJUNDATE      = @P04                  " _
        '                     & "    and SHIPORG        = @P05                  " _
        '                     & "    and SHUKODATE      = @P06                  " _
        '                     & "    and GSHABAN        = @P07                  " _
        '                     & "    and TRIPNO         = @P08                  " _
        '                     & "    and DROPNO         = @P09                  " _
        '                     & "    and SEQ            = @P10                  " _
        '                     & "    and DELFLG        <> '1'                   "

        '            Dim SQLcmd As New SqlCommand(SQLStr, SQLcon)
        '            Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", System.Data.SqlDbType.NVarChar)
        '            Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", System.Data.SqlDbType.NVarChar)
        '            Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@P03", System.Data.SqlDbType.NVarChar)
        '            Dim PARA04 As SqlParameter = SQLcmd.Parameters.Add("@P04", System.Data.SqlDbType.Date)
        '            Dim PARA05 As SqlParameter = SQLcmd.Parameters.Add("@P05", System.Data.SqlDbType.NVarChar)
        '            Dim PARA06 As SqlParameter = SQLcmd.Parameters.Add("@P06", System.Data.SqlDbType.Date)
        '            Dim PARA07 As SqlParameter = SQLcmd.Parameters.Add("@P07", System.Data.SqlDbType.NVarChar)
        '            Dim PARA08 As SqlParameter = SQLcmd.Parameters.Add("@P08", System.Data.SqlDbType.NVarChar)
        '            Dim PARA09 As SqlParameter = SQLcmd.Parameters.Add("@P09", System.Data.SqlDbType.NVarChar)
        '            Dim PARA10 As SqlParameter = SQLcmd.Parameters.Add("@P10", System.Data.SqlDbType.NVarChar)

        '            PARA01.Value = T00016UPDrow("CAMPCODE")
        '            PARA02.Value = T00016UPDrow("TORICODE")
        '            PARA03.Value = T00016UPDrow("OILTYPE")
        '            PARA04.Value = T00016UPDrow("KIJUNDATE")
        '            PARA05.Value = T00016UPDrow("SHIPORG")
        '            PARA06.Value = T00016UPDrow("SHUKODATE")
        '            PARA07.Value = T00016UPDrow("GSHABAN")
        '            PARA08.Value = T00016UPDrow("TRIPNO")
        '            PARA09.Value = T00016UPDrow("DROPNO")
        '            PARA10.Value = T00016UPDrow("SEQ")

        '            Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

        '            '画面情報へタイムスタンプ・受注番号をフィードバック
        '            While SQLdr.Read
        '                For Each T00016row In T00016tbl.Rows
        '                    If T00016row("CAMPCODE") = T00016UPDrow("CAMPCODE") AndAlso
        '                       T00016row("TORICODE") = T00016UPDrow("TORICODE") AndAlso
        '                       T00016row("OILTYPE") = T00016UPDrow("OILTYPE") AndAlso
        '                       T00016row("KIJUNDATE") = T00016UPDrow("KIJUNDATE") AndAlso
        '                       T00016row("SHIPORG") = T00016UPDrow("SHIPORG") AndAlso
        '                       T00016row("SHUKODATE") = T00016UPDrow("SHUKODATE") AndAlso
        '                       T00016row("GSHABAN") = T00016UPDrow("GSHABAN") AndAlso
        '                       T00016row("TRIPNO") = T00016UPDrow("TRIPNO") AndAlso
        '                       T00016row("DROPNO") = T00016UPDrow("DROPNO") AndAlso
        '                       T00016row("SEQ") = T00016UPDrow("SEQ") AndAlso
        '                       T00016row("DELFLG") <> C_DELETE_FLG.DELETE Then

        '                        T00016row("TIMSTP") = SQLdr("TIMSTP")
        '                        T00016row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
        '                        T00016row("ORDERNO") = T00016UPDrow("ORDERNO")
        '                        T00016row("DETAILNO") = T00016UPDrow("DETAILNO")
        '                        Exit For

        '                    End If
        '                Next
        '            End While

        '            'Close()
        '            SQLdr.Close() 'Reader(Close)
        '            SQLdr = Nothing

        '            SQLcmd.Dispose()
        '            SQLcmd = Nothing

        '        Catch ex As Exception
        '            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "T0015_SUPPLJISSKI SELECT")
        '            CS0011LOGWRITE.INFSUBCLASS = "MAIN"                         'SUBクラス名
        '            CS0011LOGWRITE.INFPOSI = "DB:T0015_SUPPLJISSKI SELECT"           '
        '            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                  '
        '            CS0011LOGWRITE.TEXT = ex.ToString()
        '            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
        '            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
        '            Exit Sub

        '        End Try

        '    End If

        'Next

        ''更新→クリア
        'For Each T00016row In T00016tbl.Rows
        '    If T00016row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING Then
        '        T00016row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
        '    End If
        'Next

        'SQLcon.Close()
        'SQLcon.Dispose()

        O_RTN = C_MESSAGE_NO.NORMAL

    End Sub

    ' ***  T00016UPDtbl更新データ（画面表示受注+画面非表示受注）作成　＆　タイムスタンプチェック処理          済
    Protected Sub DBupdate_T00016UPDtblget(ByVal O_RTN As String)

        ''更新対象受注の画面非表示（他出庫日）を取得。配送受注の更新最小単位は出荷部署単位。

        'Dim WW_SORTstr As String = ""
        'Dim WW_FILLstr As String = ""

        'Dim WW_TORICODE As String = ""
        'Dim WW_OILTYPE As String = ""
        'Dim WW_SHUKADATE As String = ""
        'Dim WW_KIJUNDATE As String = ""
        'Dim WW_SHIPORG As String = ""

        'Dim WW_SHUKODATE As String = ""
        'Dim WW_GSHABAN As String = ""
        'Dim WW_TRIPNO As String = ""
        'Dim WW_DROPNO As String = ""

        ''■■■ 更新前処理（入力情報へ操作を反映）　■■■

        'For Each T00016row In T00016tbl.Rows
        '    '削除チェックがONの時、削除更新
        '    If T00016row("ROWDEL") = "1" Then
        '        If T00016row("OPERATION") <> C_LIST_OPERATION_CODE.ERRORED Then

        '            T00016row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
        '            T00016row("DELFLG") = C_DELETE_FLG.DELETE
        '            T00016row("HIDDEN") = 1
        '        End If
        '    End If

        '    If T00016row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING OrElse
        '        T00016row("OPERATION") = C_LIST_OPERATION_CODE.WARNING Then

        '        For j As Integer = 0 To T00016tbl.Rows.Count - 1
        '            '取引先、油種、基準日（出荷日or届日）、受注部署、出荷部署が同一
        '            If T00016tbl.Rows(j)("TORICODE") = T00016row("TORICODE") AndAlso
        '               T00016tbl.Rows(j)("OILTYPE") = T00016row("OILTYPE") AndAlso
        '               T00016tbl.Rows(j)("KIJUNDATE") = T00016row("KIJUNDATE") AndAlso
        '               T00016tbl.Rows(j)("SHIPORG") = T00016row("SHIPORG") AndAlso
        '               T00016tbl.Rows(j)("DELFLG") <> "1" Then

        '                T00016tbl.Rows(j)("OPERATION") = C_LIST_OPERATION_CODE.UPDATING

        '            End If
        '        Next
        '    End If

        'Next

        ''■■■ 受注最新レコード(DB格納)をT00016UPDtblへ格納 ■■■

        ''Sort
        'CS0026TBLSORTget.TABLE = T00016tbl
        'CS0026TBLSORTget.SORTING = "TORICODE ,OILTYPE ,KIJUNDATE ,SHIPORG"
        'CS0026TBLSORTget.FILTER = ""
        'CS0026TBLSORTget.Sort(T00016tbl)
        ''○作業用DBのカラム設定
        ''更新元データ
        'Master.CreateEmptyTable(T00016UPDtbl)
        ''作業用データ
        'Master.CreateEmptyTable(T00016WKtbl)

        ''○更新対象受注のDB格納レコードを全て取得
        'For Each T00016row In T00016tbl.Rows

        '    If T00016row("TORICODE") = WW_TORICODE AndAlso
        '       T00016row("OILTYPE") = WW_OILTYPE AndAlso
        '       T00016row("KIJUNDATE") = WW_KIJUNDATE AndAlso
        '       T00016row("SHIPORG") = WW_SHIPORG Then
        '    Else
        '        If T00016row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING OrElse
        '            T00016row("OPERATION") = C_LIST_OPERATION_CODE.WARNING Then
        '            T00016WKtbl.Clear()

        '            'オブジェクト内容検索
        '            Try
        '                'DataBase接続文字
        '                Dim SQLcon = CS0050SESSION.getConnection
        '                SQLcon.Open() 'DataBase接続(Open)

        '                '検索SQL文
        '                Dim SQLStr As String =
        '                     "SELECT isnull(rtrim(A.CAMPCODE),'')          as CAMPCODE ,       " _
        '                   & "       isnull(rtrim(A.ORDERNO),'')           as ORDERNO ,        " _
        '                   & "       isnull(rtrim(A.DETAILNO),'')          as DETAILNO ,       " _
        '                   & "       isnull(rtrim(A.TRIPNO),'')            as TRIPNO ,         " _
        '                   & "       isnull(rtrim(A.DROPNO),'')            as DROPNO ,         " _
        '                   & "       isnull(rtrim(A.SEQ),'')               as SEQ ,            " _
        '                   & "       isnull(rtrim(A.TORICODE),'')          as TORICODE ,       " _
        '                   & "       isnull(rtrim(A.OILTYPE),'')           as OILTYPE ,        " _
        '                   & "       isnull(format(A.SHUKODATE, 'yyyy/MM/dd'),'') as SHUKODATE , " _
        '                   & "       isnull(format(A.KIKODATE,  'yyyy/MM/dd'),'') as KIKODATE  , " _
        '                   & "       isnull(format(A.KIJUNDATE, 'yyyy/MM/dd'),'') as KIJUNDATE , " _
        '                   & "       isnull(format(A.SHUKADATE, 'yyyy/MM/dd'),'') as SHUKADATE , " _
        '                   & "       isnull(rtrim(A.SHIPORG),'')           as SHIPORG ,        " _
        '                   & "       isnull(rtrim(A.SHUKABASHO),'')        as SHUKABASHO ,     " _
        '                   & "       isnull(rtrim(A.GSHABAN),'')           as GSHABAN ,        " _
        '                   & "       isnull(rtrim(A.RYOME),'')             as RYOME ,          " _
        '                   & "       isnull(rtrim(A.SHAFUKU),'')           as SHAFUKU ,        " _
        '                   & "       isnull(rtrim(A.STAFFCODE),'')         as STAFFCODE ,      " _
        '                   & "       isnull(rtrim(A.SUBSTAFFCODE),'')      as SUBSTAFFCODE ,   " _
        '                   & "       isnull(format(A.TODOKEDATE, 'yyyy/MM/dd'),'') as TODOKEDATE , " _
        '                   & "       isnull(rtrim(A.TODOKECODE),'')        as TODOKECODE ,     " _
        '                   & "       isnull(rtrim(A.PRODUCT1),'')          as PRODUCT1 ,       " _
        '                   & "       isnull(rtrim(A.PRODUCT2),'')          as PRODUCT2 ,       " _
        '                   & "       isnull(rtrim(A.PRODUCTCODE),'')       as PRODUCTCODE ,    " _
        '                   & "       isnull(rtrim(A.CONTNO),'')            as CONTNO ,         " _
        '                   & "       isnull(rtrim(A.JSURYO),'')            as JSURYO ,         " _
        '                   & "       isnull(rtrim(A.JDAISU),'')            as JDAISU ,         " _
        '                   & "       isnull(rtrim(A.REMARKS1),'')          as REMARKS1 ,       " _
        '                   & "       isnull(rtrim(A.REMARKS2),'')          as REMARKS2 ,       " _
        '                   & "       isnull(rtrim(A.REMARKS3),'')          as REMARKS3 ,       " _
        '                   & "       isnull(rtrim(A.REMARKS4),'')          as REMARKS4 ,       " _
        '                   & "       isnull(rtrim(A.REMARKS5),'')          as REMARKS5 ,       " _
        '                   & "       isnull(rtrim(A.REMARKS6),'')          as REMARKS6 ,       " _
        '                   & "       isnull(rtrim(A.SHARYOTYPEF),'')       as SHARYOTYPEF ,    " _
        '                   & "       isnull(rtrim(A.TSHABANF),'')          as TSHABANF ,       " _
        '                   & "       isnull(rtrim(A.SHARYOTYPEB),'')       as SHARYOTYPEB ,    " _
        '                   & "       isnull(rtrim(A.TSHABANB),'')          as TSHABANB ,       " _
        '                   & "       isnull(rtrim(A.SHARYOTYPEB2),'')      as SHARYOTYPEB2 ,   " _
        '                   & "       isnull(rtrim(A.TSHABANB2),'')         as TSHABANB2 ,      " _
        '                   & "       isnull(rtrim(A.JISSEKIKBN),'')        as JISSEKIKBN ,     " _
        '                   & "       isnull(rtrim(A.DELFLG),'')            as DELFLG ,         " _
        '                   & "       TIMSTP = cast(A.UPDTIMSTP  as bigint) ,        " _
        '                   & "       isnull(rtrim(B.SHARYOINFO1),'')       as SHARYOINFO1 ,    " _
        '                   & "       isnull(rtrim(B.SHARYOINFO2),'')       as SHARYOINFO2 ,    " _
        '                   & "       isnull(rtrim(B.SHARYOINFO3),'')       as SHARYOINFO3 ,    " _
        '                   & "       isnull(rtrim(B.SHARYOINFO4),'')       as SHARYOINFO4 ,    " _
        '                   & "       isnull(rtrim(B.SHARYOINFO5),'')       as SHARYOINFO5 ,    " _
        '                   & "       isnull(rtrim(B.SHARYOINFO6),'')       as SHARYOINFO6 ,    " _
        '                   & "       isnull(rtrim(D.ADDR1),'') +              					" _
        '                   & "       isnull(rtrim(D.ADDR2),'') +            					" _
        '                   & "       isnull(rtrim(D.ADDR3),'') +             					" _
        '                   & "       isnull(rtrim(D.ADDR4),'')          	as ADDR ,           " _
        '                   & "       isnull(rtrim(D.NOTES1),'')        	    as NOTES1 ,       	" _
        '                   & "       isnull(rtrim(D.NOTES2),'')          	as NOTES2 ,       	" _
        '                   & "       isnull(rtrim(D.NOTES3),'')          	as NOTES3 ,       	" _
        '                   & "       isnull(rtrim(D.NOTES4),'')          	as NOTES4 ,       	" _
        '                   & "       isnull(rtrim(D.NOTES5),'')          	as NOTES5 ,       	" _
        '                   & "       isnull(rtrim(E.NOTES1),'')        	    as STAFFNOTES1 ,   	" _
        '                   & "       isnull(rtrim(E.NOTES2),'')          	as STAFFNOTES2 ,   	" _
        '                   & "       isnull(rtrim(E.NOTES3),'')          	as STAFFNOTES3 ,   	" _
        '                   & "       isnull(rtrim(E.NOTES4),'')          	as STAFFNOTES4 ,   	" _
        '                   & "       isnull(rtrim(E.NOTES5),'')          	as STAFFNOTES5     	" _
        '                   & "  FROM T0015_SUPPLJISSKI AS A							" _
        '                   & "  LEFT JOIN MA006_SHABANORG B 						" _
        '                   & "    ON B.CAMPCODE     	= A.CAMPCODE 				" _
        '                   & "   and B.GSHABAN      	= A.GSHABAN 				" _
        '                   & "   and B.MANGUORG     	= A.SHIPORG 				" _
        '                   & "   and B.DELFLG          <> '1' 						" _
        '                   & "  LEFT JOIN MC007_TODKORG C 							" _
        '                   & "    ON C.CAMPCODE     	= A.CAMPCODE 				" _
        '                   & "   and C.TORICODE     	= A.TORICODE 				" _
        '                   & "   and C.TODOKECODE   	= A.TODOKECODE 				" _
        '                   & "   and C.UORG         	= A.SHIPORG 				" _
        '                   & "   and C.DELFLG          <> '1' 						" _
        '                   & "  LEFT JOIN MC006_TODOKESAKI D 						" _
        '                   & "    ON D.CAMPCODE     	= C.CAMPCODE 				" _
        '                   & "   and D.TORICODE     	= C.TORICODE				" _
        '                   & "   and D.TODOKECODE   	= C.TODOKECODE 				" _
        '                   & "   and D.STYMD           <= A.SHUKODATE				" _
        '                   & "   and D.ENDYMD          >= A.SHUKODATE				" _
        '                   & "   and D.DELFLG          <> '1' 						" _
        '                   & "  LEFT JOIN MB001_STAFF E     						" _
        '                   & "    ON E.CAMPCODE     	= A.CAMPCODE 				" _
        '                   & "   and E.STAFFCODE     	= A.STAFFCODE				" _
        '                   & "   and E.STYMD           <= A.SHUKODATE				" _
        '                   & "   and E.ENDYMD          >= A.SHUKODATE				" _
        '                   & "   and E.DELFLG          <> '1' 						" _
        '                   & " WHERE A.CAMPCODE         = @P01                      " _
        '                   & "  and  A.TORICODE         = @P02                      " _
        '                   & "  and  A.OILTYPE          = @P03           		    " _
        '                   & "  and  A.SHIPORG          = @P05           		    " _
        '                   & "  and  A.KIJUNDATE        = @P06                      " _
        '                   & "  and  A.DELFLG          <> '1'                       " _
        '                   & " ORDER BY A.TORICODE  ,A.OILTYPE ,A.KIJUNDATE ,       " _
        '                   & " 		    A.SHIPORG ,A.GSHABAN           "

        '                Dim SQLcmd As New SqlCommand(SQLStr, SQLcon)
        '                Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", System.Data.SqlDbType.NVarChar, 20)
        '                Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", System.Data.SqlDbType.NVarChar, 20)  '荷主
        '                Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@P03", System.Data.SqlDbType.NVarChar, 20)  '油種
        '                Dim PARA04 As SqlParameter = SQLcmd.Parameters.Add("@P04", System.Data.SqlDbType.NVarChar, 20)  '出荷部署
        '                Dim PARA05 As SqlParameter = SQLcmd.Parameters.Add("@P05", System.Data.SqlDbType.Date)      '出荷日

        '                '○関連受注指定
        '                PARA01.Value = T00016row("CAMPCODE")        '会社
        '                PARA02.Value = T00016row("TORICODE")        '出荷日
        '                PARA03.Value = T00016row("OILTYPE")         '油種
        '                PARA04.Value = T00016row("SHIPORG")         '出荷部署
        '                PARA05.Value = T00016row("KIJUNDATE")       '基準日

        '                '■SQL実行
        '                Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

        '                '■テーブル検索結果をテーブル格納
        '                T00016WKtbl.Load(SQLdr)
        '                T00016UPDtbl.Merge(T00016WKtbl, False)
        '                For Each T00016UPDrow In T00016UPDtbl.Rows
        '                    T00016UPDrow("LINECNT") = 0
        '                    T00016UPDrow("SELECT") = 1
        '                    T00016UPDrow("HIDDEN") = 0
        '                    T00016UPDrow("OPERATION") = C_LIST_OPERATION_CODE.NODATA
        '                Next

        '                SQLdr.Close()
        '                SQLdr = Nothing

        '                SQLcmd.Dispose()
        '                SQLcmd = Nothing

        '                SQLcon.Close() 'DataBase接続(Close)
        '                SQLcon.Dispose()
        '                SQLcon = Nothing

        '            Catch ex As Exception
        '                CS0011LOGWRITE.INFSUBCLASS = "DBupdate_T00016UPDtblget"     'SUBクラス名
        '                CS0011LOGWRITE.INFPOSI = "T0016_TORIHIKI UPDATE"
        '                CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
        '                CS0011LOGWRITE.TEXT = ex.ToString()
        '                CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
        '                CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力

        '                O_RTN = C_MESSAGE_NO.DB_ERROR
        '                Exit Sub

        '            End Try

        '            WW_TORICODE = T00016row("TORICODE")
        '            WW_OILTYPE = T00016row("OILTYPE")
        '            WW_KIJUNDATE = T00016row("KIJUNDATE")
        '            WW_SHIPORG = T00016row("SHIPORG")

        '        Else
        '        End If

        '    End If
        'Next

        ''■■■ 受注番号　自動採番 ■■■                  

        ''Sort(T00016tbl)
        'CS0026TBLSORTget.TABLE = T00016tbl
        'CS0026TBLSORTget.SORTING = "TORICODE ,OILTYPE ,KIJUNDATE  ,SHIPORG"
        'CS0026TBLSORTget.FILTER = ""
        'CS0026TBLSORTget.Sort(T00016tbl)

        ''○　受注番号　自動採番
        'For i As Integer = 0 To T00016tbl.Rows.Count - 1

        '    Dim T00016row = T00016tbl.Rows(i)


        '    If T00016row("ORDERNO").ToString.Contains("新") Then
        '        CS0033AutoNumber.SEQTYPE = CS0033AutoNumber.C_SEQTYPE.ORDERNO
        '        CS0033AutoNumber.CAMPCODE = work.WF_SEL_CAMPCODE.Text
        '        CS0033AutoNumber.MORG = T00016row("SHIPORG")
        '        CS0033AutoNumber.USERID = Master.USERID
        '        CS0033AutoNumber.getAutoNumber()

        '        If isNormal(CS0033AutoNumber.ERR) Then
        '            '他レコードへ反映
        '            For j As Integer = i To T00016tbl.Rows.Count - 1
        '                If T00016tbl.Rows(j)("ORDERNO").ToString.Contains("新") Then
        '                    If T00016tbl.Rows(j)("TORICODE") = T00016row("TORICODE") AndAlso
        '                       T00016tbl.Rows(j)("OILTYPE") = T00016row("OILTYPE") AndAlso
        '                       T00016tbl.Rows(j)("KIJUNDATE") = T00016row("KIJUNDATE") AndAlso
        '                       T00016tbl.Rows(j)("SHIPORG") = T00016row("SHIPORG") Then

        '                        T00016tbl.Rows(j)("ORDERNO") = CS0033AutoNumber.SEQ
        '                    Else
        '                        Exit For
        '                    End If
        '                End If
        '            Next

        '        Else
        '            Master.Output(CS0033AutoNumber.ERR, C_MESSAGE_TYPE.ABORT, CS0033AutoNumber.ERR_DETAIL)
        '            Exit Sub
        '        End If
        '    End If

        'Next

        ''■■■ 画面非表示レコード+画面表示レコードによりT00016UPDtblを作成 ■■■

        ''○T00016UPDtbl内の画面表示レコードを削除(日付による）…　T00016tblとレコード重複しているため

        'Dim WW_TODOKEDATEF As Date
        'Dim WW_TODOKEDATET As Date
        'Dim WW_SHUKODATEF As Date
        'Dim WW_SHUKODATET As Date
        ''届日（FROM-TO）
        'If String.IsNullOrEmpty(work.WF_SEL_TODOKEDATEF.Text) Then
        '    WW_TODOKEDATEF = C_DEFAULT_YMD
        'Else
        '    WW_TODOKEDATEF = work.WF_SEL_TODOKEDATEF.Text
        'End If
        'If String.IsNullOrEmpty(work.WF_SEL_TODOKEDATET.Text) Then
        '    WW_TODOKEDATET = C_MAX_YMD
        'Else
        '    WW_TODOKEDATET = work.WF_SEL_TODOKEDATET.Text
        'End If
        ''出荷日（FROM-TO）
        'If String.IsNullOrEmpty(work.WF_SEL_SHUKODATEF.Text) Then
        '    WW_SHUKODATEF = C_DEFAULT_YMD
        'Else
        '    WW_SHUKODATEF = work.WF_SEL_SHUKODATEF.Text
        'End If
        'If String.IsNullOrEmpty(work.WF_SEL_SHUKODATET.Text) Then
        '    WW_SHUKODATET = C_MAX_YMD
        'Else
        '    WW_SHUKODATET = work.WF_SEL_SHUKODATET.Text
        'End If

        'WW_FILLstr =
        '    "TODOKEDATE < #" & WW_TODOKEDATEF & "# or " &
        '    "TODOKEDATE > #" & WW_TODOKEDATET & "# or " &
        '    "SHUKODATE < #" & WW_SHUKODATEF & "# or " &
        '    "SHUKODATE > #" & WW_SHUKODATET & "#    "
        ''画面表示レコードを削除
        'CS0026TBLSORTget.TABLE = T00016UPDtbl
        'CS0026TBLSORTget.SORTING = "TORICODE ,OILTYPE ,KIJUNDATE ,SHIPORG"
        'CS0026TBLSORTget.FILTER = WW_FILLstr
        'CS0026TBLSORTget.Sort(T00016UPDtbl)

        ''○画面表示レコードをマージ
        'CS0026TBLSORTget.TABLE = T00016tbl
        'CS0026TBLSORTget.SORTING = "TORICODE ,OILTYPE ,KIJUNDATE ,SHIPORG"
        'CS0026TBLSORTget.FILTER = "OPERATION = '" & C_LIST_OPERATION_CODE.UPDATING & "' or OPERATION = '" & C_LIST_OPERATION_CODE.WARNING & "'"
        'CS0026TBLSORTget.Sort(T00016WKtbl)
        'T00016UPDtbl.Merge(T00016WKtbl, False)

        ''○更新・エラーをT00016UPDtblへ反映(DB更新単位：荷主、油種、基準日（出荷日or届日）、受注部署、出荷部署)
        'CS0026TBLSORTget.TABLE = T00016UPDtbl
        'CS0026TBLSORTget.SORTING = "TORICODE ,OILTYPE ,KIJUNDATE ,SHIPORG"
        'CS0026TBLSORTget.FILTER = ""
        'CS0026TBLSORTget.Sort(T00016UPDtbl)

        'For i As Integer = 0 To T00016UPDtbl.Rows.Count - 1
        '    Dim T00016UPDrow = T00016UPDtbl.Rows(i)

        '    If T00016UPDrow("OPERATION") = C_LIST_OPERATION_CODE.ERRORED Then
        '        For j As Integer = 0 To T00016UPDtbl.Rows.Count - 1
        '            '荷主、油種、基準日（出荷日or届日）、受注部署、出荷部署が同一
        '            If T00016UPDtbl.Rows(j)("TORICODE") = T00016UPDrow("TORICODE") AndAlso
        '               T00016UPDtbl.Rows(j)("OILTYPE") = T00016UPDrow("OILTYPE") AndAlso
        '               T00016UPDtbl.Rows(j)("KIJUNDATE") = T00016UPDrow("KIJUNDATE") AndAlso
        '               T00016UPDtbl.Rows(j)("SHIPORG") = T00016UPDrow("SHIPORG") Then

        '                T00016UPDtbl.Rows(j)("OPERATION") = C_LIST_OPERATION_CODE.ERRORED

        '            Else
        '                'Exit For
        '            End If
        '        Next
        '    End If

        '    If T00016UPDrow("OPERATION") = C_LIST_OPERATION_CODE.UPDATING OrElse
        '        T00016UPDrow("OPERATION") = C_LIST_OPERATION_CODE.WARNING Then
        '        For j As Integer = 0 To T00016UPDtbl.Rows.Count - 1
        '            '荷主、油種、基準日（出荷日or届日）、受注部署、出荷部署が同一
        '            If T00016UPDtbl.Rows(j)("TORICODE") = T00016UPDrow("TORICODE") AndAlso
        '               T00016UPDtbl.Rows(j)("OILTYPE") = T00016UPDrow("OILTYPE") AndAlso
        '               T00016UPDtbl.Rows(j)("KIJUNDATE") = T00016UPDrow("KIJUNDATE") AndAlso
        '               T00016UPDtbl.Rows(j)("SHIPORG") = T00016UPDrow("SHIPORG") AndAlso
        '               T00016UPDtbl.Rows(j)("OPERATION") <> C_LIST_OPERATION_CODE.ERRORED Then

        '                T00016UPDtbl.Rows(j)("OPERATION") = C_LIST_OPERATION_CODE.UPDATING

        '            Else
        '                'Exit For
        '            End If
        '        Next
        '    End If

        'Next

        ''○更新対象以外のレコードを削除
        'CS0026TBLSORTget.TABLE = T00016UPDtbl
        'CS0026TBLSORTget.SORTING = "TORICODE ,OILTYPE ,KIJUNDATE ,SHIPORG ,SHUKODATE ,GSHABAN ,TRIPNO ,DROPNO , SEQ"
        'CS0026TBLSORTget.FILTER = "OPERATION = '" & C_LIST_OPERATION_CODE.UPDATING & "' or OPERATION = '" & C_LIST_OPERATION_CODE.WARNING & "'"
        'CS0026TBLSORTget.Sort(T00016UPDtbl)

        ''■■■ T00016UPDtblのDetailNO、SEQを再付番 ■■■
        'Dim WW_DETAILNO As Integer = 0
        'Dim WW_SEQ As Integer = 0

        ''○DetailNO再付番
        'WW_TORICODE = ""
        'WW_OILTYPE = ""
        'WW_SHUKADATE = ""
        'WW_KIJUNDATE = ""
        'WW_SHIPORG = ""
        'WW_SHUKODATE = ""
        'WW_GSHABAN = ""
        'WW_TRIPNO = ""
        'WW_DROPNO = ""

        'For Each T00016UPDrow In T00016UPDtbl.Rows

        '    If T00016UPDrow("DELFLG") <> "1" Then
        '        If WW_TORICODE = T00016UPDrow("TORICODE") AndAlso
        '           WW_OILTYPE = T00016UPDrow("OILTYPE") AndAlso
        '           WW_KIJUNDATE = T00016UPDrow("KIJUNDATE") AndAlso
        '           WW_SHIPORG = T00016UPDrow("SHIPORG") Then

        '            WW_DETAILNO += 1
        '            T00016UPDrow("DETAILNO") = WW_DETAILNO.ToString("000")
        '        Else
        '            WW_DETAILNO = 1
        '            T00016UPDrow("DETAILNO") = WW_DETAILNO.ToString("000")

        '            WW_TORICODE = T00016UPDrow("TORICODE")
        '            WW_OILTYPE = T00016UPDrow("OILTYPE")
        '            WW_KIJUNDATE = T00016UPDrow("KIJUNDATE")
        '            WW_SHIPORG = T00016UPDrow("SHIPORG")

        '        End If
        '    End If

        'Next

        ''○台数設定
        'WW_TORICODE = ""
        'WW_OILTYPE = ""
        'WW_SHUKADATE = ""
        'WW_KIJUNDATE = ""
        'WW_SHIPORG = ""
        'WW_SHUKODATE = ""
        'WW_GSHABAN = ""
        'WW_TRIPNO = ""
        'For Each T00016UPDrow In T00016UPDtbl.Rows

        '    If T00016UPDrow("DELFLG") <> "1" Then
        '        If WW_TORICODE = T00016UPDrow("TORICODE") AndAlso
        '           WW_OILTYPE = T00016UPDrow("OILTYPE") AndAlso
        '           WW_KIJUNDATE = T00016UPDrow("KIJUNDATE") AndAlso
        '           WW_SHIPORG = T00016UPDrow("SHIPORG") AndAlso
        '           WW_SHUKODATE = T00016UPDrow("SHUKODATE") AndAlso
        '           WW_GSHABAN = T00016UPDrow("GSHABAN") AndAlso
        '           WW_TRIPNO = T00016UPDrow("TRIPNO") Then

        '            T00016UPDrow("JDAISU") = 0
        '        Else
        '            T00016UPDrow("JDAISU") = 1

        '            WW_TORICODE = T00016UPDrow("TORICODE")
        '            WW_OILTYPE = T00016UPDrow("OILTYPE")
        '            WW_KIJUNDATE = T00016UPDrow("KIJUNDATE")
        '            WW_SHIPORG = T00016UPDrow("SHIPORG")
        '            WW_SHUKODATE = T00016UPDrow("SHUKODATE")
        '            WW_GSHABAN = T00016UPDrow("GSHABAN")
        '            WW_TRIPNO = T00016UPDrow("TRIPNO")
        '            WW_DROPNO = T00016UPDrow("DROPNO")

        '        End If
        '    End If

        'Next

        ''○SEQ再付番
        'WW_TORICODE = ""
        'WW_OILTYPE = ""
        'WW_SHUKADATE = ""
        'WW_KIJUNDATE = ""
        'WW_SHIPORG = ""
        'WW_SHUKODATE = ""
        'WW_GSHABAN = ""
        'WW_TRIPNO = ""
        'WW_DROPNO = ""
        'For Each T00016UPDrow In T00016UPDtbl.Rows

        '    If T00016UPDrow("DELFLG") <> "1" Then
        '        If WW_TORICODE = T00016UPDrow("TORICODE") AndAlso
        '           WW_OILTYPE = T00016UPDrow("OILTYPE") AndAlso
        '           WW_KIJUNDATE = T00016UPDrow("KIJUNDATE") AndAlso
        '           WW_SHIPORG = T00016UPDrow("SHIPORG") AndAlso
        '           WW_SHUKODATE = T00016UPDrow("SHUKODATE") AndAlso
        '           WW_GSHABAN = T00016UPDrow("GSHABAN") AndAlso
        '           WW_TRIPNO = T00016UPDrow("TRIPNO") AndAlso
        '           WW_DROPNO = T00016UPDrow("DROPNO") Then

        '            WW_SEQ += 1
        '            T00016UPDrow("SEQ") = WW_SEQ.ToString("00")
        '        Else
        '            WW_SEQ = 1
        '            T00016UPDrow("SEQ") = WW_SEQ.ToString("00")

        '            WW_TORICODE = T00016UPDrow("TORICODE")
        '            WW_OILTYPE = T00016UPDrow("OILTYPE")
        '            WW_KIJUNDATE = T00016UPDrow("KIJUNDATE")
        '            WW_SHIPORG = T00016UPDrow("SHIPORG")
        '            WW_SHUKODATE = T00016UPDrow("SHUKODATE")
        '            WW_GSHABAN = T00016UPDrow("GSHABAN")
        '            WW_TRIPNO = T00016UPDrow("TRIPNO")
        '            WW_DROPNO = T00016UPDrow("DROPNO")

        '        End If
        '    End If

        'Next

        '○close
        O_RTN = C_MESSAGE_NO.NORMAL

    End Sub

    ''' <summary>
    ''' T00016tbl関連データ削除
    ''' </summary>
    ''' <param name="I_DATENOW">更新時刻</param>
    ''' <param name="O_RTN">RTNCODE</param>
    ''' <remarks>更新対象受注の画面非表示（他出庫日）を取得。配送受注の更新最小単位は出荷部署単位。</remarks>
    Protected Sub DBupdate_T16DELETE(ByVal I_DATENOW As Date, ByVal O_RTN As String)

        '■■■ T00016UPDtbl関連の荷主受注・配送受注を論理削除 ■■■　…　削除情報はT00016UPDtblに存在

        'Sort
        CS0026TBLSORTget.TABLE = T00016UPDtbl
        CS0026TBLSORTget.SORTING = "TORICODE ,OILTYPE ,KIJUNDATE ,SHIPORG ,TIMSTP , DELFLG, OPERATION"
        CS0026TBLSORTget.FILTER = ""
        CS0026TBLSORTget.Sort(T00016UPDtbl)

        Try
            'DataBase接続文字
            Dim SQLcon = CS0050SESSION.getConnection
            SQLcon.Open() 'DataBase接続(Open)

            '更新SQL文･･･配送受注の現受注番号を一括論理削除
            Dim SQLStr As String =
                      " UPDATE T0016_TORIHIKI           " _
                    & "    SET UPDYMD      = @P11,      " _
                    & "        UPDUSER     = @P12,      " _
                    & "        UPDTERMID   = @P13,      " _
                    & "        RECEIVEYMD  = @P14,      " _
                    & "        DELFLG      = '1'        " _
                    & "  WHERE CAMPCODE    = @P01       " _
                    & "    AND DENKBN      = @P02       " _
                    & "    AND DENNO       = @P03       " _
                    & "    AND TORIHIKIYMD = @P04       " _
                    & "    AND RECODEKBN   = @P05       " _
                    & "    AND TORICODE    = @P06       " _
                    & "    AND TODOKECODE  = @P07       " _
                    & "    AND GSHABAN     = @P08       " _
                    & "    AND NSHABAN     = @P09       " _
                    & "    AND DELFLG     <> '1'        "


            Dim SQLcmd As New SqlCommand(SQLStr, SQLcon)
            SQLcmd.CommandTimeout = 300

            Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", System.Data.SqlDbType.NVarChar)
            Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", System.Data.SqlDbType.NVarChar)
            Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@P03", System.Data.SqlDbType.NVarChar)
            Dim PARA04 As SqlParameter = SQLcmd.Parameters.Add("@P04", System.Data.SqlDbType.Date)
            Dim PARA05 As SqlParameter = SQLcmd.Parameters.Add("@P05", System.Data.SqlDbType.NVarChar)
            Dim PARA06 As SqlParameter = SQLcmd.Parameters.Add("@P06", System.Data.SqlDbType.NVarChar)
            Dim PARA07 As SqlParameter = SQLcmd.Parameters.Add("@P07", System.Data.SqlDbType.NVarChar)
            Dim PARA08 As SqlParameter = SQLcmd.Parameters.Add("@P08", System.Data.SqlDbType.NVarChar)
            Dim PARA09 As SqlParameter = SQLcmd.Parameters.Add("@P09", System.Data.SqlDbType.NVarChar)

            Dim PARA11 As SqlParameter = SQLcmd.Parameters.Add("@P11", System.Data.SqlDbType.DateTime)
            Dim PARA12 As SqlParameter = SQLcmd.Parameters.Add("@P12", System.Data.SqlDbType.NVarChar)
            Dim PARA13 As SqlParameter = SQLcmd.Parameters.Add("@P13", System.Data.SqlDbType.NVarChar)
            Dim PARA14 As SqlParameter = SQLcmd.Parameters.Add("@P14", System.Data.SqlDbType.DateTime)

            Dim WW_DENKBN As String = ""
            Dim WW_DENNO As String = ""
            Dim WW_TORIHIKIYMD As String = ""
            Dim WW_RECODEKBN As String = ""
            Dim WW_TORICODE As String = ""
            Dim WW_TODOKECODE As String = ""
            Dim WW_GSHABAN As String = ""
            Dim WW_NSHABAN As String = ""

            For Each T00016UPDrow In T00016UPDtbl.Rows

                If T00016UPDrow("OPERATION") = C_LIST_OPERATION_CODE.UPDATING OrElse
                    T00016UPDrow("OPERATION") = C_LIST_OPERATION_CODE.WARNING Then
                    If T00016UPDrow("DENKBN") <> WW_DENKBN OrElse
                       T00016UPDrow("DENNO") <> WW_DENNO OrElse
                       T00016UPDrow("TORIHIKIYMD") <> WW_TORIHIKIYMD OrElse
                       T00016UPDrow("RECODEKBN") <> WW_RECODEKBN OrElse
                       T00016UPDrow("TORICODE") <> WW_TORICODE OrElse
                       T00016UPDrow("TODOKECODE") <> WW_TODOKECODE OrElse
                       T00016UPDrow("GSHABAN") <> WW_GSHABAN OrElse
                       T00016UPDrow("NSHABAN") <> WW_NSHABAN Then

                        '○T00016UPDtbl関連の配送受注を論理削除

                        PARA01.Value = T00016UPDrow("CAMPCODE")
                        PARA02.Value = T00016UPDrow("DENKBN")
                        PARA03.Value = T00016UPDrow("DENNO")
                        PARA04.Value = T00016UPDrow("TORIHIKIYMD")
                        PARA05.Value = T00016UPDrow("RECODEKBN")
                        PARA06.Value = T00016UPDrow("TORICODE")
                        PARA07.Value = T00016UPDrow("TODOKECODE")
                        PARA08.Value = T00016UPDrow("GSHABAN")
                        PARA09.Value = T00016UPDrow("NSHABAN")

                        PARA11.Value = I_DATENOW
                        PARA12.Value = Master.USERID
                        PARA13.Value = Master.USERTERMID
                        PARA14.Value = C_DEFAULT_YMD

                        SQLcmd.ExecuteNonQuery()

                        'ブレイクキー退避
                        WW_DENKBN = T00016UPDrow("DENKBN")
                        WW_DENNO = T00016UPDrow("DENNO")
                        WW_TORIHIKIYMD = T00016UPDrow("TORIHIKIYMD")
                        WW_RECODEKBN = T00016UPDrow("RECODEKBN")
                        WW_TORICODE = T00016UPDrow("TORICODE")
                        WW_TODOKECODE = T00016UPDrow("TODOKECODE")
                        WW_GSHABAN = T00016UPDrow("GSHABAN")
                        WW_NSHABAN = T00016UPDrow("NSHABAN")
                    End If
                End If

            Next

            'CLOSE
            SQLcmd.Dispose()
            SQLcmd = Nothing

            O_RTN = C_MESSAGE_NO.NORMAL

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "T0016_TORIHIKI(old) DEL")
            CS0011LOGWRITE.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:T0016_TORIHIKI(old) DEL"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力

            O_RTN = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try

    End Sub

#End Region

#Region "T0016テーブル入力関連"

    ''' <summary>
    ''' 入力データ登録
    ''' </summary>
    ''' <param name="O_RTNCODE"></param>
    ''' <remarks></remarks>
    Protected Sub INPtbltoT16tbl(ByRef O_RTNCODE As String)

        '■■■ 数量ゼロは読み飛ばし ■■■
        For i As Integer = T00016INPtbl.Rows.Count - 1 To 0 Step -1
            Dim T00016INProw = T00016INPtbl.Rows(i)
            '出荷前々日以降は、データ取込対象外とする
            If Val(T00016INProw("JSURYO")) = 0 Then
                '数量なしは無視
                T00016INPtbl.Rows(i).Delete()
            End If
        Next


        '■■■ 項目チェック ■■■
        '●チェック処理
        INPtbl_CHEK(WW_ERRCODE)

        INPtbl_CHEK_DATE(WW_ERRCODE)

        '■■■ 変更有無チェック ■■■    
        '…　Grid画面へ別明細追加：T00016INProw("WORK_NO") = ""
        '　　変更発生　　：T00016INProw("OPERATION")へ"更新"or"エラー"を設定

        '●変更有無取得　　　     ※Excelは全て新規。全て更新とする。
        For Each T00016INProw In T00016INPtbl.Rows
            '数量・台数未設定時は対象外
            If T00016INProw("WORK_NO") = "" AndAlso Val(T00016INProw("JSURYO")) = 0 Then Continue For

            'エラーは設定しない
            If T00016INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA Then
                T00016INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
            End If
            If T00016INProw("OPERATION") <> C_LIST_OPERATION_CODE.ERRORED Then
                T00016INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
            End If

            T00016INProw("WORK_NO") = ""
            T00016INProw("LINECNT") = 0

        Next


        '■■■ 更新前処理（入力情報へ受注番号設定、Grid画面の同一行情報を削除）　■■■
        For i As Integer = 0 To T00016INPtbl.Rows.Count - 1

            Dim T00016INProw = T00016INPtbl.Rows(i)
            '数量・台数未設定時は対象外
            If T00016INProw("WORK_NO") = "" AndAlso Val(T00016INProw("JSURYO")) = 0 Then Continue For

            For j As Integer = 0 To T00016tbl.Rows.Count - 1

                '状態をクリア設定
                EditOperationText(T00016tbl.Rows(j), False)

                If T00016INProw("OPERATION") <> C_LIST_OPERATION_CODE.NODATA Then

                    'Grid画面行追加の場合は受注番号を取得
                    If T00016tbl.Rows(j)("TORICODE") = T00016INProw("TORICODE") AndAlso
                       T00016tbl.Rows(j)("OILTYPE") = T00016INProw("OILTYPE") AndAlso
                       T00016tbl.Rows(j)("KIJUNDATE") = T00016INProw("KIJUNDATE") AndAlso
                       T00016tbl.Rows(j)("SHIPORG") = T00016INProw("SHIPORG") Then

                        T00016INProw("ORDERNO") = T00016tbl.Rows(j)("ORDERNO")
                        T00016INProw("DETAILNO") = "000"

                    End If

                    '取引先、油種、基準日（出荷日or届日）、受注部署、出荷部署、出庫日、業務車番、両目、トリップ、ドロップが同一
                    If CompareOrder(T00016tbl.Rows(j), T00016INProw) Then

                        T00016INProw("LINECNT") = T00016tbl.Rows(j)("LINECNT")

                    End If

                    'EXCELは同一受注条件レコードを論理削除（T16実態が存在する場合、物理削除。）
                    If T00016tbl.Rows(j)("GSHABAN") = T00016INProw("GSHABAN") AndAlso
                       T00016tbl.Rows(j)("OILTYPE") = T00016INProw("OILTYPE") AndAlso
                       T00016tbl.Rows(j)("SHUKODATE") = T00016INProw("SHUKODATE") AndAlso
                       T00016tbl.Rows(j)("SHIPORG") = T00016INProw("SHIPORG") AndAlso
                       T00016tbl.Rows(j)("DELFLG") <> "1" Then

                        If Val(T00016tbl.Rows(j)("JSURYO")) = 0 Then
                            T00016tbl.Rows(j)("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                            T00016tbl.Rows(j)("DELFLG") = "1"   '削除
                            T00016tbl.Rows(j)("HIDDEN") = "1"   '非表示
                            T00016tbl.Rows(j)("SELECT") = "0"   '明細表示対象外
                        Else
                            T00016INProw("DELFLG") = "1"
                            T00016INProw("HIDDEN") = "1"   '非表示
                            T00016INProw("SELECT") = "0"   '明細表示対象外
                        End If
                    Else
                        If T00016tbl.Rows(j)("SHIPORG") = WF_DEFORG.Text Then
                            If T00016tbl.Rows(j)("GSHABAN") = "" AndAlso
                               T00016tbl.Rows(j)("OILTYPE") = T00016INProw("OILTYPE") AndAlso
                               T00016tbl.Rows(j)("SHUKODATE") = T00016INProw("SHUKODATE") AndAlso
                               T00016tbl.Rows(j)("SHIPORG") = T00016INProw("SHIPORG") AndAlso
                               T00016tbl.Rows(j)("DELFLG") <> "1" Then
                                If Val(T00016tbl.Rows(j)("JSURYO")) = 0 Then
                                    T00016tbl.Rows(j)("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                                    T00016tbl.Rows(j)("DELFLG") = "1"   '削除
                                    T00016tbl.Rows(j)("HIDDEN") = "1"   '非表示
                                    T00016tbl.Rows(j)("SELECT") = "0"   '明細表示対象外
                                Else
                                    T00016INProw("DELFLG") = "1"
                                    T00016INProw("HIDDEN") = "1"   '非表示
                                    T00016INProw("SELECT") = "0"   '明細表示対象外
                                End If
                            End If
                        End If
                    End If

                End If

            Next
        Next

        '■■■ 更新前処理（入力情報へ操作を反映）　■■■
        INPtbl_PreUpdate1()

        '■■■ 更新前処理（受注画面で自動作成された関連受注を削除）　■■■
        INPtbl_PreUpdateDel()

        '■■■ 更新前処理（入力情報へLINECNTを付番）　■■■
        INPtbl_PreUpdate2()

        '■■■ 更新前処理（入力情報へ暫定受注番号を付番）　■■■
        INPtbl_PreUpdate3()
    End Sub

    ''' <summary>
    ''' 入力データチェック（出庫日範囲）
    ''' </summary>
    ''' <param name="O_RTNCODE"></param>
    ''' <remarks></remarks>
    Protected Sub INPtbl_CHEK_DATE(ByRef O_RTNCODE As String)

        '●関連チェック処理
        Dim WW_DATE As Date
        Dim WW_LOGONYMD As Date = CS0050SESSION.LOGONDATE
        For i As Integer = T00016INPtbl.Rows.Count - 1 To 0 Step -1

            Dim T00016INProw = T00016INPtbl.Rows(i)

            If Date.TryParse(T00016INProw("SHUKODATE"), WW_DATE) Then

                '出庫前々日以降は、データ取込対象外とする
                '出荷日<当日は処理対象外（出荷当日までOK）
                If WW_DATE < WW_LOGONYMD Then
                    Dim WW_ERR_MES As String = "・更新できないレコード(過去日データ)です。"
                    WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 明細番号= @D" & i.ToString("000") & "D@ , "
                    WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 取引先　=" & T00016INProw("TORICODE") & " , "
                    WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 届先　　=" & T00016INProw("TODOKECODE") & " , "
                    WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 出荷場所=" & T00016INProw("SHUKABASHO") & " , "
                    WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 出庫日　=" & T00016INProw("SHUKODATE") & " , "
                    WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 届日　　=" & T00016INProw("TODOKEDATE") & " , "
                    WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 出荷日　=" & T00016INProw("SHUKADATE") & " , "
                    WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 車番　　=" & T00016INProw("GSHABAN") & " , "
                    WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 乗務員　=" & T00016INProw("STAFFCODE") & " , "
                    WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 品名  　=" & T00016INProw("PRODUCTCODE") & " , "
                    WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> ﾄﾘｯﾌﾟ 　=" & T00016INProw("TRIPNO") & " , "
                    WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> ﾄﾞﾛｯﾌﾟ　=" & T00016INProw("DROPNO") & " , "
                    WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 削除　　=" & T00016INProw("DELFLG") & " "
                    rightview.AddErrorReport(WW_ERR_MES)

                    T00016INProw("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                    WW_ERRCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If
        Next

    End Sub

    ''' <summary>
    ''' 車検切れ・容器検査切れチェック対象部署
    ''' </summary>
    ''' <param name="I_COMPCODE">会社コード</param>
    ''' <param name="I_ORGCODE">部署コード</param>
    ''' <param name="O_RTN">可否判定</param>
    ''' <remarks></remarks>
    Public Function IsInspectionOrg(ByVal I_COMPCODE As String, ByVal I_ORGCODE As String, ByRef O_RTN As String) As Boolean
        ' 車検切れ・容器検査切れチェック対象部署
        Static INSPECTION_CHECK_ORG As List(Of String) = Nothing

        If Not IsNothing(INSPECTION_CHECK_ORG) Then
            Return INSPECTION_CHECK_ORG.Contains(I_ORGCODE)
        End If

        Const CLASS_CODE As String = "INSPECTIONORG"
        O_RTN = C_MESSAGE_NO.NORMAL
        Try
            Using GS0032 As New GS0032FIXVALUElst
                GS0032.CAMPCODE = I_COMPCODE
                GS0032.CLAS = CLASS_CODE
                GS0032.STDATE = Date.Now
                GS0032.ENDDATE = Date.Now
                GS0032.GS0032FIXVALUElst()
                If Not isNormal(GS0032.ERR) Then
                    O_RTN = GS0032.ERR
                    Return False
                End If
                INSPECTION_CHECK_ORG = New List(Of String)
                For Each item As ListItem In GS0032.VALUE1.Items
                    INSPECTION_CHECK_ORG.Add(item.Value)
                Next
                '存在する場合TRUE、しない場合FALSEを帰す
                Return (Not IsNothing(GS0032.VALUE1.Items.FindByValue(I_ORGCODE)))
            End Using

        Catch ex As Exception
            CS0011LOGWRITE.INFSUBCLASS = "GRT0016"                   'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:INSPECTIONORG Select"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            O_RTN = C_MESSAGE_NO.DB_ERROR
            Return False
        End Try

    End Function

    ''' <summary>
    ''' 更新前処理（受注画面で自動作成された関連受注を削除）
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub INPtbl_PreUpdateDel()

        For i As Integer = 0 To T00016INPtbl.Rows.Count - 1

            Dim T00016INProw = T00016INPtbl.Rows(i)

            If T00016INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING OrElse
                T00016INProw("OPERATION") = C_LIST_OPERATION_CODE.WARNING Then
                For j As Integer = 0 To T00016tbl.Rows.Count - 1
                    '取引先、油種、基準日（出荷日or届日）、受注部署、出荷部署、出庫日、業務車番、両目、トリップ、ドロップが同一
                    If T00016tbl.Rows(j)("TORICODE") = T00016INProw("TORICODE") AndAlso
                       T00016tbl.Rows(j)("OILTYPE") = T00016INProw("OILTYPE") AndAlso
                       T00016tbl.Rows(j)("KIJUNDATE") = T00016INProw("KIJUNDATE") AndAlso
                       T00016tbl.Rows(j)("SHIPORG") = T00016INProw("SHIPORG") AndAlso
                       T00016tbl.Rows(j)("SHUKODATE") = T00016INProw("SHUKODATE") AndAlso
                       T00016tbl.Rows(j)("TRIPNO") = "000" Then

                        T00016tbl.Rows(j)("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                        T00016tbl.Rows(j)("DELFLG") = "1"

                    End If

                Next
            End If

        Next

    End Sub

    ''' <summary>
    ''' 更新前処理（入力情報へ操作を反映）
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub INPtbl_PreUpdate1()

        For i As Integer = 0 To T00016INPtbl.Rows.Count - 1

            Dim T00016INProw = T00016INPtbl.Rows(i)

            If T00016INProw("WORK_NO") = "" And Val(T00016INProw("JSURYO")) = 0 Then
            Else
                If T00016INProw("OPERATION") = C_LIST_OPERATION_CODE.ERRORED Then
                    For j As Integer = i To T00016INPtbl.Rows.Count - 1
                        '取引先、油種、基準日（出荷日or届日）、受注部署、出荷部署、出庫日、業務車番、両目、トリップ、ドロップが同一
                        If CompareOrder(T00016INPtbl.Rows(j), T00016INProw) Then

                            T00016INPtbl.Rows(j)("OPERATION") = C_LIST_OPERATION_CODE.ERRORED

                        End If
                    Next
                End If

                If T00016INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING OrElse
                    T00016INProw("OPERATION") = C_LIST_OPERATION_CODE.WARNING Then
                    For j As Integer = 0 To T00016INPtbl.Rows.Count - 1
                        '取引先、油種、基準日（出荷日or届日）、受注部署、出荷部署、出庫日、業務車番、両目、トリップ、ドロップが同一
                        If CompareOrder(T00016INPtbl.Rows(j), T00016INProw) AndAlso
                           T00016INPtbl.Rows(j)("OPERATION") <> C_LIST_OPERATION_CODE.ERRORED Then

                            T00016INPtbl.Rows(j)("OPERATION") = C_LIST_OPERATION_CODE.UPDATING

                        End If
                    Next
                End If
            End If

        Next

    End Sub

    ''' <summary>
    ''' 更新前処理（入力情報へLINECNTを付番）
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub INPtbl_PreUpdate2()

        '●LINECNTを付番
        'sort
        CS0026TBLSORTget.TABLE = T00016tbl
        CS0026TBLSORTget.SORTING = "LINECNT ASC"
        CS0026TBLSORTget.FILTER = ""
        CS0026TBLSORTget.Sort(T00016tbl)


        Dim WW_ORDERNO As Integer = 0
        Dim WW_DETAILNO As Integer = 0
        Dim WW_LINECNT As Integer = 0
        Dim WW_CNT As Integer = 0

        '受注番号初期値セット
        If T00016tbl.Rows.Count = 0 Then
            WW_LINECNT = 0
        Else
            WW_LINECNT = CInt(T00016tbl.Rows(T00016tbl.Rows.Count - 1)("LINECNT"))
        End If

        For i As Integer = 0 To T00016INPtbl.Rows.Count - 1

            Dim T00016INProw = T00016INPtbl.Rows(i)

            '新規有効明細
            If T00016INProw("WORK_NO") = "" AndAlso (Val(T00016INProw("JSURYO")) <> 0) Then

                If Val(T00016INProw("LINECNT")) = 0 Then

                    WW_LINECNT = WW_LINECNT + 1
                    WW_CNT = 0

                    '同一条件レコードへも反映
                    For j As Integer = 0 To T00016INPtbl.Rows.Count - 1
                        If T00016INPtbl.Rows(j)("WORK_NO") = "" AndAlso (Val(T00016INPtbl.Rows(j)("JSURYO")) <> 0) Then

                            If CompareOrder(T00016INPtbl.Rows(j), T00016INProw) Then

                                WW_CNT = WW_CNT + 1
                                T00016INPtbl.Rows(j)("LINECNT") = WW_LINECNT.ToString("0")

                            End If

                        End If
                    Next
                End If
            End If

        Next

    End Sub

    ''' <summary>
    ''' 更新前処理（入力情報へ暫定受注番号を付番）
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub INPtbl_PreUpdate3()

        Dim WW_ORDERNO As Integer = 0
        Dim WW_DETAILNO As Integer = 0

        '●暫定受注番号を付番
        For i As Integer = 0 To T00016INPtbl.Rows.Count - 1

            Dim T00016INProw = T00016INPtbl.Rows(i)

            '数量・台数未設定時は対象外
            If T00016INProw("WORK_NO") = "" AndAlso Val(T00016INProw("JSURYO")) = 0 Then Continue For

            '追加明細("WORK_NO")
            If T00016INProw("WORK_NO") = "" Then

                WW_ORDERNO = WW_ORDERNO + 1
                WW_DETAILNO = 0

                'T16INPtblへも反映（次レコード処理用）
                For j As Integer = 0 To T00016INPtbl.Rows.Count - 1
                    '数量・台数未設定時は対象外
                    If T00016INPtbl.Rows(j)("WORK_NO") = "" AndAlso Val(T00016INPtbl.Rows(j)("JSURYO")) = 0 Then Continue For

                    If T00016INPtbl.Rows(j)("ORDERNO") = "" Then

                        '受注判定基準により同一受注に、新受注番号を付与
                        If T00016INPtbl.Rows(j)("TORICODE") = T00016INProw("TORICODE") AndAlso
                            T00016INPtbl.Rows(j)("OILTYPE") = T00016INProw("OILTYPE") AndAlso
                            T00016INPtbl.Rows(j)("SHIPORG") = T00016INProw("SHIPORG") AndAlso
                            T00016INPtbl.Rows(j)("KIJUNDATE") = T00016INProw("KIJUNDATE") Then

                            T00016INPtbl.Rows(j)("ORDERNO") = "新" & WW_ORDERNO.ToString("00")
                            WW_DETAILNO = WW_DETAILNO + 1
                            T00016INPtbl.Rows(j)("DETAILNO") = WW_DETAILNO.ToString("000")
                            T00016INPtbl.Rows(j)("WORK_NO") = "0"

                        End If
                    Else

                        '受注判定基準により同一受注に、新受注番号を付与
                        If T00016INPtbl.Rows(j)("WORK_NO") = "" AndAlso
                            T00016INPtbl.Rows(j)("TORICODE") = T00016INProw("TORICODE") AndAlso
                            T00016INPtbl.Rows(j)("OILTYPE") = T00016INProw("OILTYPE") AndAlso
                            T00016INPtbl.Rows(j)("SHIPORG") = T00016INProw("SHIPORG") AndAlso
                            T00016INPtbl.Rows(j)("KIJUNDATE") = T00016INProw("KIJUNDATE") Then

                            WW_DETAILNO = WW_DETAILNO + 1
                            T00016INPtbl.Rows(j)("DETAILNO") = WW_DETAILNO.ToString("000")
                            T00016INPtbl.Rows(j)("WORK_NO") = "0"

                        End If
                    End If

                Next

                '○T00016INProwをT00016tblへ追加
                T00016tbl.ImportRow(T00016INProw)

            Else

                If T00016INProw("OPERATION") <> C_LIST_OPERATION_CODE.NODATA Then

                    '○T00016INProwをT00016tblへ追加
                    T00016tbl.ImportRow(T00016INProw)
                End If

            End If

        Next

    End Sub

    ''' <summary>
    ''' 入力データチェック
    ''' </summary>
    ''' <param name="O_RTNCODE"></param>
    ''' <remarks></remarks>
    Protected Sub INPtbl_CHEK(ByRef O_RTNCODE As String)

        ''○インターフェイス初期値設定
        'O_RTNCODE = C_MESSAGE_NO.NORMAL

        'Dim WW_LINEerr As String = ""
        'Dim WW_SEQ As Integer = 0
        'Dim WW_CS0024FCHECKVAL As String = ""
        'Dim WW_CS0024FCHECKERR As String = ""
        'Dim WW_CS0024FCHECKREPORT As String = ""
        'Dim WW_CheckMES1 As String = ""
        'Dim WW_CheckMES2 As String = ""
        'Dim WW_TEXT As String = ""

        'WW_ERRLIST.Clear()
        'If IsNothing(S0013tbl) Then
        '    S0013tbl = New DataTable
        'End If

        'For i As Integer = 0 To T00016INPtbl.Rows.Count - 1

        '    Dim T00016INProw = T00016INPtbl.Rows(i)

        '    WW_LINEerr = C_MESSAGE_NO.NORMAL

        '    '初期クリア
        '    T00016INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA

        '    '数量・台数未設定時はチェック対象外
        '    If T00016INProw("WORK_NO") = "" AndAlso T00016INProw("JSURYO") = "" AndAlso T00016INProw("JDAISU") = "" Then Continue For


        '    '■■■ 単項目チェック(ヘッダー情報) ■■■

        '    Dim WW_TORI_FLG As String = ""
        '    '■キー項目(取引先コード：TORICODE)
        '    '○必須・項目属性チェック
        '    WW_CS0024FCHECKVAL = T00016INProw("TORICODE")
        '    Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "TORICODE", WW_CS0024FCHECKVAL, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT, S0013tbl)
        '    If isNormal(WW_CS0024FCHECKERR) Then
        '        If Val(WW_CS0024FCHECKVAL) = 0 Then
        '            CODENAME_get("TORICODE", T00016INProw("TORICODE"), WW_TEXT, WW_RTN_SW)
        '            T00016INProw("TORICODENAME") = WW_TEXT
        '            If Not isNormal(WW_RTN_SW) Then
        '                WW_CheckMES1 = "・更新できないレコード(取引先コードエラー)です。"
        '                WW_CheckMES2 = " マスタに存在しません。"
        '                ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, T00016INProw)
        '            Else
        '                WW_TORI_FLG = "OK"
        '            End If
        '        Else
        '            T00016INProw("TORICODE") = WW_CS0024FCHECKVAL
        '            '○LeftBox存在チェック
        '            CODENAME_get("TORICODE", T00016INProw("TORICODE"), WW_TEXT, WW_RTN_SW)
        '            T00016INProw("TORICODENAME") = WW_TEXT
        '            If Not isNormal(WW_RTN_SW) Then
        '                WW_CheckMES1 = "・更新できないレコード(取引先コードエラー)です。"
        '                WW_CheckMES2 = " マスタに存在しません。"
        '                ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, T00016INProw)
        '            Else
        '                WW_TORI_FLG = "OK"
        '            End If
        '        End If
        '    Else
        '        WW_CheckMES1 = "・更新できないレコード(取引先コードエラー)です。"
        '        WW_CheckMES2 = WW_CS0024FCHECKREPORT
        '        ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, T00016INProw)
        '    End If

        '    Dim WW_OILTYPE_FLG As String = ""
        '    '○必須・項目属性チェック
        '    WW_CS0024FCHECKVAL = T00016INProw("OILTYPE")
        '    Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "OILTYPE", WW_CS0024FCHECKVAL, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT, S0013tbl)
        '    If isNormal(WW_CS0024FCHECKERR) Then
        '        If CInt(WW_CS0024FCHECKVAL) = 0 Then
        '            WW_CheckMES1 = "・更新できないレコード(油種エラー)です。"
        '            WW_CheckMES2 = WW_CS0024FCHECKREPORT
        '            ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, T00016INProw)
        '        Else
        '            T00016INProw("OILTYPE") = WW_CS0024FCHECKVAL
        '            If Not String.IsNullOrEmpty(work.WF_SEL_OILTYPE.Text) AndAlso work.WF_SEL_OILTYPE.Text <> T00016INProw("OILTYPE") Then
        '                WW_CheckMES1 = "・更新できないレコード(油種エラー)です。"
        '                WW_CheckMES2 = " 条件入力で指定された油種と異ります( " & T00016INProw("OILTYPE") & ") "
        '                ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, T00016INProw)
        '            Else
        '                WW_OILTYPE_FLG = "OK"
        '            End If
        '        End If
        '    Else
        '        WW_CheckMES1 = "・更新できないレコード(油種エラー)です。"
        '        WW_CheckMES2 = WW_CS0024FCHECKREPORT
        '        ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, T00016INProw)
        '    End If

        '    '■キー項目(出荷日：SHUKADATE)
        '    '○デフォルト
        '    If String.IsNullOrEmpty(T00016INProw("SHUKADATE")) Then
        '        T00016INProw("SHUKADATE") = T00016INProw("SHUKODATE")
        '    End If

        '    '○必須・項目属性チェック
        '    WW_CS0024FCHECKVAL = T00016INProw("SHUKADATE")
        '    Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "SHUKADATE", WW_CS0024FCHECKVAL, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT, S0013tbl)
        '    If isNormal(WW_CS0024FCHECKERR) Then
        '        T00016INProw("SHUKADATE") = WW_CS0024FCHECKVAL      'yyyy/MM/dd
        '    Else
        '        WW_CheckMES1 = "・更新できないレコード(出荷日エラー)です。"
        '        WW_CheckMES2 = WW_CS0024FCHECKREPORT
        '        ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, T00016INProw)
        '    End If

        '    '■明細項目(出荷部署：SHIPORG)

        '    '○デフォルト
        '    If T00016INProw("SHIPORG") = "" Then
        '        T00016INProw("SHIPORG") = WF_DEFORG.Text
        '    End If


        '    '○必須・項目属性チェック
        '    WW_CS0024FCHECKVAL = T00016INProw("SHIPORG")
        '    Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "SHIPORG", WW_CS0024FCHECKVAL, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT, S0013tbl)
        '    If isNormal(WW_CS0024FCHECKERR) Then
        '        T00016INProw("SHIPORG") = WW_CS0024FCHECKVAL

        '        '○LeftBox存在チェック
        '        If Not String.IsNullOrEmpty(T00016INProw("SHIPORG")) Then
        '            CODENAME_get("SHIPORG", T00016INProw("SHIPORG"), WW_TEXT, WW_RTN_SW)
        '            T00016INProw("SHIPORGNAME") = WW_TEXT
        '            If Not isNormal(WW_RTN_SW) Then
        '                WW_CheckMES1 = "・更新できないレコード(出荷部署エラー)です。"
        '                WW_CheckMES2 = " マスタに存在しません。"
        '                ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, T00016INProw)
        '            End If
        '        End If
        '    Else
        '        WW_CheckMES1 = "・更新できないレコード(出荷部署エラー)です。"
        '        WW_CheckMES2 = WW_CS0024FCHECKREPORT
        '        ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, T00016INProw)
        '    End If

        '    '■明細項目(出庫日：SHUKODATE)
        '    '○必須・項目属性チェック
        '    Dim WW_SHUKODATEERR As String = "OFF"
        '    WW_CS0024FCHECKVAL = T00016INProw("SHUKODATE")
        '    Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "SHUKODATE", WW_CS0024FCHECKVAL, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT, S0013tbl)
        '    If isNormal(WW_CS0024FCHECKERR) Then
        '        T00016INProw("SHUKODATE") = WW_CS0024FCHECKVAL      'yyyy/MM/dd
        '    Else
        '        WW_SHUKODATEERR = "ON"
        '        WW_CheckMES1 = "・エラーが存在します。(出庫日)"
        '        WW_CheckMES2 = WW_CS0024FCHECKREPORT
        '        ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, T00016INProw)
        '    End If

        '    '*******************  業務車番チェック  *********************

        '    Dim WW_CHKFLG As String = "ON"

        '    '■明細項目(業務車番：GSHABAN)
        '    '○必須・項目属性チェック
        '    If T00016INProw("SHIPORG") <> WF_DEFORG.Text Then
        '        '異なる拠点データ投入時はチェック対象外
        '        If T00016INProw("GSHABAN") = "" Then
        '            WW_CHKFLG = "OFF"
        '        End If
        '    End If

        '    If WW_CHKFLG = "ON" Then
        '        WW_CS0024FCHECKVAL = T00016INProw("GSHABAN")
        '        Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "GSHABAN", WW_CS0024FCHECKVAL, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT, S0013tbl)
        '        If isNormal(WW_CS0024FCHECKERR) Then
        '            T00016INProw("GSHABAN") = WW_CS0024FCHECKVAL

        '            '○LeftBox存在チェック
        '            If T00016INProw("GSHABAN") <> "" Then
        '                CODENAME_get("GSHABAN", T00016INProw("GSHABAN"), WW_TEXT, WW_RTN_SW)
        '                If Not isNormal(WW_RTN_SW) Then
        '                    WW_CheckMES1 = "・エラーが存在します。(業務車番)"
        '                    WW_CheckMES2 = " マスタに存在しません。"
        '                    ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.BOX_ERROR_EXIST, T00016INProw)
        '                End If
        '            End If
        '        Else
        '            WW_CheckMES1 = "・エラーが存在します。(業務車番)"
        '            WW_CheckMES2 = WW_CS0024FCHECKREPORT
        '            ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.BOX_ERROR_EXIST, T00016INProw)
        '        End If

        '    End If

        '    '*******************  乗務員チェック  *********************

        '    '■明細項目(乗務員コード：STAFFCODE)
        '    '○必須・項目属性チェック
        '    WW_CHKFLG = "ON"
        '    If T00016INProw("SHIPORG") <> WF_DEFORG.Text Then
        '        '異なる拠点データ投入時はチェック対象外
        '        If T00016INProw("STAFFCODE") = "" Then
        '            WW_CHKFLG = "OFF"
        '        End If
        '    End If

        '    If WW_CHKFLG = "ON" Then
        '        WW_CS0024FCHECKVAL = T00016INProw("STAFFCODE")
        '        Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "STAFFCODE", WW_CS0024FCHECKVAL, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT, S0013tbl)
        '        If isNormal(WW_CS0024FCHECKERR) Then
        '            If CInt(WW_CS0024FCHECKVAL) = 0 Then
        '            Else
        '                T00016INProw("STAFFCODE") = WW_CS0024FCHECKVAL

        '                '○LeftBox存在チェック
        '                CODENAME_get("STAFFCODE", T00016INProw("STAFFCODE"), WW_TEXT, WW_RTN_SW)
        '                T00016INProw("STAFFCODENAME") = WW_TEXT
        '                If Not isNormal(WW_RTN_SW) Then
        '                    WW_CheckMES1 = "・エラーが存在します。(乗務員コード)"
        '                    WW_CheckMES2 = " マスタに存在しません。"
        '                    ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.BOX_ERROR_EXIST, T00016INProw)
        '                End If
        '            End If
        '        Else
        '            WW_CheckMES1 = "・エラーが存在します。(乗務員コード)"
        '            WW_CheckMES2 = WW_CS0024FCHECKREPORT
        '            ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.BOX_ERROR_EXIST, T00016INProw)
        '        End If

        '    End If

        '    '*******************  副乗務員チェック  *********************

        '    '■明細項目(副乗務員コード：SUBSTAFFCODE)
        '    '○必須・項目属性チェック
        '    WW_CHKFLG = "ON"
        '    If T00016INProw("SHIPORG") <> WF_DEFORG.Text Then
        '        '異なる拠点データ投入時はチェック対象外
        '        If T00016INProw("SUBSTAFFCODE") = "" Then
        '            WW_CHKFLG = "OFF"
        '        End If
        '    End If

        '    If WW_CHKFLG = "ON" Then
        '        WW_CS0024FCHECKVAL = T00016INProw("SUBSTAFFCODE")
        '        Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "SUBSTAFFCODE", WW_CS0024FCHECKVAL, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT, S0013tbl)
        '        If isNormal(WW_CS0024FCHECKERR) Then
        '            If CInt(WW_CS0024FCHECKVAL) = 0 Then
        '            Else
        '                T00016INProw("SUBSTAFFCODE") = WW_CS0024FCHECKVAL

        '                '○LeftBox存在チェック
        '                CODENAME_get("SUBSTAFFCODE", T00016INProw("SUBSTAFFCODE"), WW_TEXT, WW_RTN_SW)
        '                T00016INProw("SUBSTAFFCODENAME") = WW_TEXT
        '                If Not isNormal(WW_RTN_SW) Then
        '                    WW_CheckMES1 = "・エラーが存在します。(副乗務員コード)"
        '                    WW_CheckMES2 = " マスタに存在しません。"
        '                    ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.BOX_ERROR_EXIST, T00016INProw)
        '                End If
        '            End If
        '        Else
        '            WW_CheckMES1 = "・エラーが存在します。(副乗務員コード)"
        '            WW_CheckMES2 = WW_CS0024FCHECKREPORT
        '            ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.BOX_ERROR_EXIST, T00016INProw)
        '        End If

        '    End If

        '    '*******************  出荷場所チェック  *********************

        '    '■明細項目(出荷場所：SHUKABASHO)
        '    '○必須・項目属性チェック

        '    WW_CS0024FCHECKVAL = T00016INProw("SHUKABASHO")
        '    Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "SHUKABASHO", WW_CS0024FCHECKVAL, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT, S0013tbl)
        '    If isNormal(WW_CS0024FCHECKERR) Then
        '        T00016INProw("SHUKABASHO") = WW_CS0024FCHECKVAL

        '        '○LeftBox存在チェック
        '        CODENAME_get("SHUKABASHO", T00016INProw("SHUKABASHO"), WW_TEXT, WW_RTN_SW, work.createDistinationParam(work.WF_SEL_CAMPCODE.Text, T00016INProw("SHIPORG"), "", "2", True))
        '        T00016INProw("SHUKABASHONAME") = WW_TEXT
        '        If Not isNormal(WW_RTN_SW) Then
        '            WW_CheckMES1 = "・エラーが存在します。(出荷場所)"
        '            WW_CheckMES2 = " マスタに存在しません。"
        '            ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.BOX_ERROR_EXIST, T00016INProw)
        '        End If
        '    Else
        '        CODENAME_get("SHUKABASHO", T00016INProw("SHUKABASHO"), WW_TEXT, WW_RTN_SW, work.createDistinationParam(work.WF_SEL_CAMPCODE.Text, T00016INProw("SHIPORG"), "", "2", True))
        '        T00016INProw("SHUKABASHONAME") = WW_TEXT
        '        If Not isNormal(WW_RTN_SW) Then
        '            WW_CheckMES1 = "・エラーが存在します。(出荷場所)"
        '            WW_CheckMES2 = " マスタに存在しません。"
        '            ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.BOX_ERROR_EXIST, T00016INProw)
        '        End If
        '    End If

        '    '■明細項目(帰庫日：KIKODATE)
        '    '○デフォルト
        '    If String.IsNullOrEmpty(T00016INProw("KIKODATE")) Then
        '        T00016INProw("KIKODATE") = T00016INProw("SHUKODATE")
        '    End If

        '    '○必須・項目属性チェック
        '    WW_CS0024FCHECKVAL = T00016INProw("KIKODATE")
        '    Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "KIKODATE", WW_CS0024FCHECKVAL, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT, S0013tbl)
        '    If isNormal(WW_CS0024FCHECKERR) Then
        '        T00016INProw("KIKODATE") = WW_CS0024FCHECKVAL      'yyyy/MM/dd
        '    Else
        '        WW_CheckMES1 = "・エラーが存在します。(帰庫日)"
        '        WW_CheckMES2 = WW_CS0024FCHECKREPORT
        '        ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.BOX_ERROR_EXIST, T00016INProw)
        '    End If

        '    '■明細項目(車腹：SHAFUKU)
        '    '○デフォルト
        '    '業務車番より、車腹を再設定
        '    WW_CHKFLG = "ON"
        '    If T00016INProw("SHIPORG") <> WF_DEFORG.Text Then
        '        If T00016INProw("SHAFUKU") = "" Then
        '            WW_CHKFLG = "OFF"
        '        End If
        '    End If

        '    If WW_CHKFLG = "ON" Then
        '        T00016INProw("SHAFUKU") = ""
        '        Dim item = WF_ListSHAFUKU.Items.FindByText(T00016INProw("GSHABAN"))
        '        If Not IsNothing(item) Then
        '            T00016INProw("SHAFUKU") = item.Value
        '        End If

        '        '○必須・項目属性チェック
        '        WW_CS0024FCHECKVAL = T00016INProw("SHAFUKU")
        '        Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "SHAFUKU", WW_CS0024FCHECKVAL, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT, S0013tbl)
        '        If isNormal(WW_CS0024FCHECKERR) Then
        '            'データ存在チェック（上記チェック方法がNUMのため、ゼロ埋めデータが出来てしまう）
        '            If String.IsNullOrEmpty(T00016INProw("SHAFUKU")) AndAlso CInt(WW_CS0024FCHECKVAL) <> 0 Then
        '                WW_CheckMES1 = "・エラーが存在します。(車腹登録なし)"
        '                WW_CheckMES2 = ""
        '                ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.BOX_ERROR_EXIST, T00016INProw)
        '            Else
        '                T00016INProw("SHAFUKU") = WW_CS0024FCHECKVAL
        '            End If
        '        Else
        '            WW_CheckMES1 = "・エラーが存在します。(車腹)"
        '            WW_CheckMES2 = WW_CS0024FCHECKREPORT
        '            ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.BOX_ERROR_EXIST, T00016INProw)
        '        End If

        '    End If

        '    WW_CHKFLG = "ON"
        '    If T00016INProw("SHIPORG") <> WF_DEFORG.Text Then
        '        If T00016INProw("TRIPNO") = "" Then
        '            WW_SEQ += 1
        '            T00016INProw("TRIPNO") = WW_SEQ.ToString("000")
        '            WW_CHKFLG = "OFF"
        '        End If
        '    End If

        '    If WW_CHKFLG = "ON" Then
        '        '■明細項目(トリップ：TRIPNO)
        '        '○必須・項目属性チェック
        '        WW_CS0024FCHECKVAL = T00016INProw("TRIPNO")
        '        Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "TRIPNO", WW_CS0024FCHECKVAL, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT, S0013tbl)
        '        If isNormal(WW_CS0024FCHECKERR) Then
        '            T00016INProw("TRIPNO") = WW_CS0024FCHECKVAL
        '        Else
        '            WW_CheckMES1 = "・エラーが存在します。(トリップ)"
        '            WW_CheckMES2 = WW_CS0024FCHECKREPORT
        '            ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.BOX_ERROR_EXIST, T00016INProw)
        '        End If

        '    End If

        '    '■明細項目(ドロップ：DROPNO)
        '    '○必須・項目属性チェック
        '    WW_CHKFLG = "ON"
        '    If T00016INProw("SHIPORG") <> WF_DEFORG.Text Then
        '        If T00016INProw("DROPNO") = "" Then
        '            T00016INProw("DROPNO") = "000"
        '            WW_CHKFLG = "OFF"
        '        End If
        '    End If

        '    If WW_CHKFLG = "ON" Then
        '        WW_CS0024FCHECKVAL = T00016INProw("DROPNO")
        '        Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "DROPNO", WW_CS0024FCHECKVAL, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT, S0013tbl)
        '        If isNormal(WW_CS0024FCHECKERR) Then
        '            T00016INProw("DROPNO") = WW_CS0024FCHECKVAL
        '        Else
        '            WW_CheckMES1 = "・エラーが存在します。(ドロップ)"
        '            WW_CheckMES2 = WW_CS0024FCHECKREPORT
        '            ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.BOX_ERROR_EXIST, T00016INProw)
        '        End If

        '    End If

        '    '*******************  日付・時間チェック  *********************

        '    '■キー項目(届日：TODOKEDATE)
        '    '○デフォルト
        '    If String.IsNullOrEmpty(T00016INProw("TODOKEDATE")) Then
        '        T00016INProw("TODOKEDATE") = T00016INProw("SHUKODATE")
        '    End If

        '    '○必須・項目属性チェック
        '    WW_CS0024FCHECKVAL = T00016INProw("TODOKEDATE")
        '    Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "TODOKEDATE", WW_CS0024FCHECKVAL, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT, S0013tbl)
        '    If isNormal(WW_CS0024FCHECKERR) Then
        '        T00016INProw("TODOKEDATE") = WW_CS0024FCHECKVAL      'yyyy/MM/dd
        '    Else
        '        WW_CheckMES1 = "・エラーが存在します。(届日エラー)"
        '        WW_CheckMES2 = WW_CS0024FCHECKREPORT
        '        ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.BOX_ERROR_EXIST, T00016INProw)
        '    End If

        '    '*******************  品名チェック  *********************

        '    '・明細項目(品名コード：PRODUCTCODE)

        '    WW_CS0024FCHECKVAL = T00016INProw("PRODUCTCODE")
        '    Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "PRODUCTCODE", WW_CS0024FCHECKVAL, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT, S0013tbl)
        '    If isNormal(WW_CS0024FCHECKERR) Then
        '        If Not String.IsNullOrEmpty(WW_CS0024FCHECKVAL) Then
        '            T00016INProw("PRODUCTCODE") = WW_CS0024FCHECKVAL

        '            'LeftBox存在チェック
        '            CODENAME_get("PRODUCTCODE", T00016INProw("PRODUCTCODE"), WW_TEXT, WW_RTN_SW)
        '            If Not isNormal(WW_RTN_SW) Then
        '                WW_CheckMES1 = "・エラーが存在します。（品名コード）"
        '                WW_CheckMES2 = "マスタに存在しません。"
        '                ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.BOX_ERROR_EXIST, T00016INProw)
        '            End If
        '        End If
        '    Else
        '        WW_CheckMES1 = "・エラーが存在します。（品名コード）"
        '        WW_CheckMES2 = WW_CS0024FCHECKREPORT
        '        ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.BOX_ERROR_EXIST, T00016INProw)
        '    End If

        '    '■明細項目(品名１：PRODUCT1)
        '    '■明細項目(品名２：PRODUCT2)
        '    If T00016INProw("PRODUCTCODE") <> "" AndAlso T00016INProw("PRODUCTCODE").ToString.Length = 11 Then
        '        Dim productCode As String = T00016INProw("PRODUCTCODE").ToString
        '        T00016INProw("PRODUCT1") = productCode.Substring(4, 2)
        '        T00016INProw("PRODUCT2") = productCode.Substring(6, 5)
        '    End If

        '    '*******************  届先チェック  *********************

        '    '■明細項目(届先コード：TODOKECODE)
        '    '○必須・項目属性チェック

        '    WW_CS0024FCHECKVAL = T00016INProw("TODOKECODE")
        '    Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "TODOKECODE", WW_CS0024FCHECKVAL, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT, S0013tbl)
        '    If isNormal(WW_CS0024FCHECKERR) Then

        '        '○LeftBox存在チェック
        '        CODENAME_get("TODOKECODE", T00016INProw("TODOKECODE"), WW_TEXT, WW_RTN_SW, work.createDistinationParam(work.WF_SEL_CAMPCODE.Text, T00016INProw("SHIPORG"), "", "1", True))
        '        T00016INProw("TODOKECODENAME") = WW_TEXT
        '        If Not isNormal(WW_RTN_SW) Then
        '            WW_CheckMES1 = "・エラーが存在します。(届先コード)"
        '            WW_CheckMES2 = " マスタに存在しません。"
        '            ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.BOX_ERROR_EXIST, T00016INProw)
        '        End If
        '    Else
        '        WW_CheckMES1 = "・エラーが存在します。(届先コード)"
        '        WW_CheckMES2 = WW_CS0024FCHECKREPORT
        '        ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.BOX_ERROR_EXIST, T00016INProw)
        '    End If


        '    '*******************  数量チェック  *********************

        '    '■明細項目(数量：JSURYO)
        '    '○必須・項目属性チェック
        '    WW_CS0024FCHECKVAL = T00016INProw("JSURYO")
        '    Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "JSURYO", WW_CS0024FCHECKVAL, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT, S0013tbl)
        '    If isNormal(WW_CS0024FCHECKERR) Then
        '        'データ存在チェック
        '        If String.IsNullOrEmpty(T00016INProw("JSURYO")) Then
        '            T00016INProw("JSURYO") = ""
        '        Else
        '            T00016INProw("JSURYO") = WW_CS0024FCHECKVAL
        '        End If
        '    Else
        '        WW_CheckMES1 = "・エラーが存在します。(数量)"
        '        WW_CheckMES2 = WW_CS0024FCHECKREPORT
        '        ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.BOX_ERROR_EXIST, T00016INProw)
        '    End If

        '    '■明細項目(台数：JDAISU)
        '    '○デフォルト
        '    If T00016INProw("OILTYPE") <> "04" Then
        '        T00016INProw("JDAISU") = 1
        '    End If

        '    '○必須・項目属性チェック
        '    WW_CS0024FCHECKVAL = T00016INProw("JDAISU")
        '    Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "JDAISU", WW_CS0024FCHECKVAL, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT, S0013tbl)
        '    If isNormal(WW_CS0024FCHECKERR) Then
        '        T00016INProw("JDAISU") = CInt(WW_CS0024FCHECKVAL)
        '    Else
        '        WW_CheckMES1 = "・エラーが存在します。(台数不正)"
        '        WW_CheckMES2 = WW_CS0024FCHECKREPORT
        '        ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.BOX_ERROR_EXIST, T00016INProw)
        '    End If

        '    '*******************  その他チェック  *********************


        '    '■明細項目(コンテナ番号：CONTNO)
        '    '○必須・項目属性チェック
        '    WW_CS0024FCHECKVAL = T00016INProw("CONTNO")
        '    Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "CONTNO", WW_CS0024FCHECKVAL, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT, S0013tbl)
        '    If isNormal(WW_CS0024FCHECKERR) Then
        '        T00016INProw("CONTNO") = WW_CS0024FCHECKVAL
        '    Else
        '        WW_CheckMES1 = "・エラーが存在します。(コンテナ番号)"
        '        WW_CheckMES2 = WW_CS0024FCHECKREPORT
        '        ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.BOX_ERROR_EXIST, T00016INProw)
        '    End If

        '    '■明細項目(枝番：SEQ)
        '    '○必須・項目属性チェック
        '    WW_CS0024FCHECKVAL = T00016INProw("SEQ")
        '    Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "SEQ", WW_CS0024FCHECKVAL, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT, S0013tbl)
        '    If isNormal(WW_CS0024FCHECKERR) Then
        '        T00016INProw("SEQ") = WW_CS0024FCHECKVAL
        '    Else
        '        WW_CheckMES1 = "・エラーが存在します。(枝番)"
        '        WW_CheckMES2 = WW_CS0024FCHECKREPORT
        '        ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.BOX_ERROR_EXIST, T00016INProw)
        '    End If

        '    '■明細項目(両目：RYOME)
        '    '○必須・項目属性チェック
        '    WW_CS0024FCHECKVAL = T00016INProw("RYOME")
        '    Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "RYOME", WW_CS0024FCHECKVAL, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT, S0013tbl)
        '    If isNormal(WW_CS0024FCHECKERR) Then
        '        T00016INProw("RYOME") = WW_CS0024FCHECKVAL
        '    Else
        '        WW_CheckMES1 = "・エラーが存在します。(両目)"
        '        WW_CheckMES2 = WW_CS0024FCHECKREPORT
        '        ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.BOX_ERROR_EXIST, T00016INProw)
        '    End If

        '    '■明細項目(削除フラグ：DELFLG)
        '    '○必須・項目属性チェック
        '    WW_CS0024FCHECKVAL = T00016INProw("DELFLG")
        '    Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "DELFLG", WW_CS0024FCHECKVAL, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT, S0013tbl)
        '    If isNormal(WW_CS0024FCHECKERR) Then
        '        T00016INProw("DELFLG") = WW_CS0024FCHECKVAL
        '    Else
        '        WW_CheckMES1 = "・エラーが存在します。(削除フラグ)"
        '        WW_CheckMES2 = WW_CS0024FCHECKREPORT
        '        ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.BOX_ERROR_EXIST, T00016INProw)
        '    End If

        '    '■■■ 関連チェック　■■■

        '    '■数量or台数入力チェック
        '    If Val(T00016INProw("JSURYO")) = 0 AndAlso Val(T00016INProw("JDAISU")) = 0 Then
        '        WW_CheckMES1 = "・更新できないレコード(数量・台数が未入力)です。"
        '        WW_CheckMES2 = ""
        '        ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, T00016INProw)
        '    End If

        '    '■出庫日・帰庫日
        '    If T00016INProw("SHUKODATE") <> "" AndAlso T00016INProw("KIKODATE") <> "" AndAlso T00016INProw("SHUKODATE") > T00016INProw("KIKODATE") Then
        '        WW_CheckMES1 = "・更新できないレコード(出庫日 > 帰庫日)です。"
        '        WW_CheckMES2 = ""
        '        ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, T00016INProw)
        '    End If

        '    '■容器検査期限、車検期限チェック（八戸、大井川、水島のみ）
        '    Dim WW_HPRSINSNYMDF As String = ""
        '    Dim WW_HPRSINSNYMDB As String = ""
        '    Dim WW_HPRSINSNYMDB2 As String = ""
        '    Dim WW_LICNYMDF As String = ""
        '    Dim WW_LICNYMDB As String = ""
        '    Dim WW_LICNYMDB2 As String = ""
        '    Dim WW_LICNPLTNOF As String = ""
        '    Dim WW_LICNPLTNOB As String = ""
        '    Dim WW_LICNPLTNOB2 As String = ""
        '    If WW_SHUKODATEERR = "OFF" AndAlso T00016INProw("SHUKODATE") <> "" Then
        '        If IsInspectionOrg(work.WF_SEL_CAMPCODE.Text, T00016INProw("SHIPORG").ToString, O_RTNCODE) Then

        '            If T00016INProw("OILTYPE") = "02" Then
        '                For j As Integer = 0 To WF_ListGSHABAN.Items.Count - 1
        '                    If WF_ListGSHABAN.Items(j).Value = T00016INProw("GSHABAN") Then
        '                        If WF_ListOILTYPE.Items(j).Value = T00016INProw("OILTYPE") Then
        '                            WW_HPRSINSNYMDF = WF_ListHPRSINSNYMDF.Items(j).Value.Replace("-", "/")
        '                            WW_HPRSINSNYMDB = WF_ListHPRSINSNYMDB.Items(j).Value.Replace("-", "/")
        '                            WW_HPRSINSNYMDB2 = WF_ListHPRSINSNYMDB2.Items(j).Value.Replace("-", "/")
        '                            WW_LICNYMDF = WF_ListLICNYMDF.Items(j).Value.Replace("-", "/")
        '                            WW_LICNYMDB = WF_ListLICNYMDB.Items(j).Value.Replace("-", "/")
        '                            WW_LICNYMDB2 = WF_ListLICNYMDB2.Items(j).Value.Replace("-", "/")
        '                            WW_LICNPLTNOF = WF_ListLICNPLTNOF.Items(j).Value
        '                            WW_LICNPLTNOB = WF_ListLICNPLTNOB.Items(j).Value
        '                            WW_LICNPLTNOB2 = WF_ListLICNPLTNOB2.Items(j).Value
        '                            Exit For
        '                        End If
        '                    End If
        '                Next

        '                '容器検査年月日チェック（２カ月前から警告、４日前はエラー）
        '                '車検年月日チェック（１カ月前から警告、４日前はエラー）
        '                '------ 車両前 -------------------------------------------------------------------------
        '                '車検チェック
        '                If SYARYOTYPE.INSPECTION_LIST.Contains(T00016INProw("SHARYOTYPEF")) Then
        '                    If IsDate(WW_LICNYMDF) Then
        '                        Dim WW_days As String = DateDiff("d", T00016INProw("SHUKODATE"), CDate(WW_LICNYMDF))
        '                        If CDate(WW_LICNYMDF) < T00016INProw("SHUKODATE") Then
        '                            '車検切れ
        '                            WW_CheckMES1 = "・更新できないレコード(車検切れ)です。(" & WW_LICNPLTNOF & " " & T00016INProw("SHARYOTYPEF") & T00016INProw("TSHABANF") & " " & WW_LICNYMDF & ")"
        '                            WW_CheckMES2 = ""
        '                            ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, T00016INProw)
        '                        ElseIf CDate(WW_LICNYMDF).AddDays(-4) < T00016INProw("SHUKODATE") Then
        '                            '４日前はエラー
        '                            WW_CheckMES1 = "・更新できないレコード(車検" & WW_days & "日前)です。(" & WW_LICNPLTNOF & " " & T00016INProw("SHARYOTYPEF") & T00016INProw("TSHABANF") & " " & WW_LICNYMDF & ")"
        '                            WW_CheckMES2 = ""
        '                            ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, T00016INProw)
        '                        ElseIf CDate(WW_LICNYMDF).AddMonths(-1) < T00016INProw("SHUKODATE") Then
        '                            '1カ月前から警告
        '                            WW_CheckMES1 = "・警告レコード(車検" & WW_days & "日前)です。(" & WW_LICNPLTNOF & " " & T00016INProw("SHARYOTYPEF") & T00016INProw("TSHABANF") & " " & WW_LICNYMDF & ")"
        '                            WW_CheckMES2 = ""
        '                            ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.WORNING_RECORD_EXIST, T00016INProw)
        '                        End If
        '                    Else
        '                        'エラー
        '                        WW_CheckMES1 = "・更新できないレコード(車両マスタ不備：車検有効年月日)です。(" & WW_LICNPLTNOF & " " & T00016INProw("SHARYOTYPEF") & T00016INProw("TSHABANF") & ")"
        '                        WW_CheckMES2 = ""
        '                        ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, T00016INProw)
        '                    End If
        '                End If

        '                '容器チェック
        '                If SYARYOTYPE.TANK_LIST.Contains(T00016INProw("SHARYOTYPEF")) Then
        '                    If IsDate(WW_HPRSINSNYMDF) Then
        '                        Dim WW_days As String = DateDiff("d", T00016INProw("SHUKODATE"), CDate(WW_HPRSINSNYMDF))
        '                        If CDate(WW_HPRSINSNYMDF) < T00016INProw("SHUKODATE") Then
        '                            '容器検査切れ
        '                            WW_CheckMES1 = "・更新できないレコード(容器検査切れ)です。(" & WW_LICNPLTNOF & " " & T00016INProw("SHARYOTYPEF") & T00016INProw("TSHABANF") & " " & WW_HPRSINSNYMDF & ")"
        '                            WW_CheckMES2 = ""
        '                            ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, T00016INProw)
        '                        ElseIf CDate(WW_HPRSINSNYMDF).AddDays(-4) < T00016INProw("SHUKODATE") Then
        '                            '４日前はエラー
        '                            WW_CheckMES1 = "・更新できないレコード(容器検査" & WW_days & "日前)です。(" & WW_LICNPLTNOF & " " & T00016INProw("SHARYOTYPEF") & T00016INProw("TSHABANF") & " " & WW_HPRSINSNYMDF & ")"
        '                            WW_CheckMES2 = ""
        '                            ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, T00016INProw)
        '                        ElseIf CDate(WW_HPRSINSNYMDF).AddMonths(-2) < T00016INProw("SHUKODATE") Then
        '                            '2カ月前から警告
        '                            WW_CheckMES1 = "・警告レコード(容器検査" & WW_days & "日前)です。(" & WW_LICNPLTNOF & " " & T00016INProw("SHARYOTYPEF") & T00016INProw("TSHABANF") & " " & WW_HPRSINSNYMDF & ")"
        '                            WW_CheckMES2 = ""
        '                            ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.WORNING_RECORD_EXIST, T00016INProw)
        '                        End If
        '                    Else
        '                        'エラー
        '                        WW_CheckMES1 = "・更新できないレコード(車両マスタ不備：次回容器再検査年月日)です。(" & WW_LICNPLTNOF & " " & T00016INProw("SHARYOTYPEF") & T00016INProw("TSHABANF") & ")"
        '                        WW_CheckMES2 = ""
        '                        ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, T00016INProw)
        '                    End If

        '                End If

        '                '------ 車両後 -------------------------------------------------------------------------
        '                '車検チェック
        '                If SYARYOTYPE.INSPECTION_LIST.Contains(T00016INProw("SHARYOTYPEB")) Then
        '                    If IsDate(WW_LICNYMDB) Then
        '                        Dim WW_days As String = DateDiff("d", T00016INProw("SHUKODATE"), CDate(WW_LICNYMDB))
        '                        If CDate(WW_LICNYMDB) < T00016INProw("SHUKODATE") Then
        '                            '車検切れ
        '                            WW_CheckMES1 = "・更新できないレコード(車検切れ)です。(" & WW_LICNPLTNOB & " " & T00016INProw("SHARYOTYPEB") & T00016INProw("TSHABANB") & " " & WW_LICNYMDB & ")"
        '                            WW_CheckMES2 = ""
        '                            ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, T00016INProw)
        '                        ElseIf CDate(WW_LICNYMDB).AddDays(-4) < T00016INProw("SHUKODATE") Then
        '                            '４日前はエラー
        '                            WW_CheckMES1 = "・更新できないレコード(車検" & WW_days & "日前)です。(" & WW_LICNPLTNOB & " " & T00016INProw("SHARYOTYPEB") & T00016INProw("TSHABANB") & " " & WW_LICNYMDB & ")"
        '                            WW_CheckMES2 = ""
        '                            ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, T00016INProw)
        '                        ElseIf CDate(WW_LICNYMDB).AddMonths(-1) < T00016INProw("SHUKODATE") Then
        '                            '1カ月前から警告
        '                            WW_CheckMES1 = "・警告レコード(車検" & WW_days & "日前)です。(" & WW_LICNPLTNOB & " " & T00016INProw("SHARYOTYPEB") & T00016INProw("TSHABANB") & " " & WW_LICNYMDB & ")"
        '                            WW_CheckMES2 = ""
        '                            ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.WORNING_RECORD_EXIST, T00016INProw)
        '                        End If
        '                    Else
        '                        'エラー
        '                        WW_CheckMES1 = "・更新できないレコード(車両マスタ不備：車検有効年月日)です。(" & WW_LICNPLTNOB & " " & T00016INProw("SHARYOTYPEB") & T00016INProw("TSHABANB") & ")"
        '                        WW_CheckMES2 = ""
        '                        ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, T00016INProw)
        '                    End If
        '                End If

        '                '容器チェック
        '                If SYARYOTYPE.TANK_LIST.Contains(T00016INProw("SHARYOTYPEB")) Then
        '                    If IsDate(WW_HPRSINSNYMDB) Then
        '                        Dim WW_days As String = DateDiff("d", T00016INProw("SHUKODATE"), CDate(WW_HPRSINSNYMDB))
        '                        If CDate(WW_HPRSINSNYMDB) < T00016INProw("SHUKODATE") Then
        '                            '容器検査切れ
        '                            WW_CheckMES1 = "・更新できないレコード(容器検査切れ)です。(" & WW_LICNPLTNOB & " " & T00016INProw("SHARYOTYPEB") & T00016INProw("TSHABANB") & " " & WW_HPRSINSNYMDB & ")"
        '                            WW_CheckMES2 = ""
        '                            ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, T00016INProw)
        '                        ElseIf CDate(WW_HPRSINSNYMDB).AddDays(-4) < T00016INProw("SHUKODATE") Then
        '                            '４日前はエラー
        '                            WW_CheckMES1 = "・更新できないレコード(容器検査" & WW_days & "日前)です。(" & WW_LICNPLTNOB & " " & T00016INProw("SHARYOTYPEB") & T00016INProw("TSHABANB") & " " & WW_HPRSINSNYMDB & ")"
        '                            WW_CheckMES2 = ""
        '                            ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, T00016INProw)
        '                        ElseIf CDate(WW_HPRSINSNYMDB).AddMonths(-2) < T00016INProw("SHUKODATE") Then
        '                            '2カ月前から警告
        '                            WW_CheckMES1 = "・警告レコード(容器検査" & WW_days & "日前)です。(" & WW_LICNPLTNOB & " " & T00016INProw("SHARYOTYPEB") & T00016INProw("TSHABANB") & " " & WW_HPRSINSNYMDB & ")"
        '                            WW_CheckMES2 = ""
        '                            ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.WORNING_RECORD_EXIST, T00016INProw)
        '                        End If
        '                    Else
        '                        'エラー
        '                        WW_CheckMES1 = "・更新できないレコード(車両マスタ不備：次回容器再検査年月日)です。(" & WW_LICNPLTNOB & " " & T00016INProw("SHARYOTYPEB") & T00016INProw("TSHABANB") & ")"
        '                        WW_CheckMES2 = ""
        '                        ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, T00016INProw)
        '                    End If

        '                End If

        '                '------ 車両後２ -------------------------------------------------------------------------
        '                '車検チェック
        '                If SYARYOTYPE.INSPECTION_LIST.Contains(T00016INProw("SHARYOTYPEB2")) Then
        '                    If IsDate(WW_LICNYMDB2) Then
        '                        Dim WW_days As String = DateDiff("d", T00016INProw("SHUKODATE"), CDate(WW_LICNYMDB2))
        '                        If CDate(WW_LICNYMDB2) < T00016INProw("SHUKODATE") Then
        '                            '車検切れ
        '                            WW_CheckMES1 = "・更新できないレコード(車検切れ)です。(" & WW_LICNPLTNOB2 & " " & T00016INProw("SHARYOTYPEB2") & T00016INProw("TSHABANB2") & " " & WW_LICNYMDB2 & ")"
        '                            WW_CheckMES2 = ""
        '                            ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, T00016INProw)
        '                        ElseIf CDate(WW_LICNYMDB2).AddDays(-4) < T00016INProw("SHUKODATE") Then
        '                            '４日前はエラー
        '                            WW_CheckMES1 = "・更新できないレコード(車検" & WW_days & "日前)です。(" & WW_LICNPLTNOB2 & " " & T00016INProw("SHARYOTYPEB2") & T00016INProw("TSHABANB2") & " " & WW_LICNYMDB2 & ")"
        '                            WW_CheckMES2 = ""
        '                            ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, T00016INProw)
        '                        ElseIf CDate(WW_LICNYMDB2).AddMonths(-1) < T00016INProw("SHUKODATE") Then
        '                            '1カ月前から警告
        '                            WW_CheckMES1 = "・警告レコード(車検" & WW_days & "日前)です。(" & WW_LICNPLTNOB2 & " " & T00016INProw("SHARYOTYPEB2") & T00016INProw("TSHABANB2") & " " & WW_LICNYMDB2 & ")"
        '                            WW_CheckMES2 = ""
        '                            ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.WORNING_RECORD_EXIST, T00016INProw)
        '                        End If
        '                    Else
        '                        'エラー
        '                        WW_CheckMES1 = "・更新できないレコード(車両マスタ不備：車検有効年月日)です。(" & WW_LICNPLTNOB2 & " " & T00016INProw("SHARYOTYPEB2") & T00016INProw("TSHABANB2") & ")"
        '                        WW_CheckMES2 = ""
        '                        ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, T00016INProw)
        '                    End If
        '                End If

        '                '容器チェック
        '                If SYARYOTYPE.TANK_LIST.Contains(T00016INProw("SHARYOTYPEB2")) Then
        '                    If IsDate(WW_HPRSINSNYMDB2) Then
        '                        Dim WW_days As String = DateDiff("d", T00016INProw("SHUKODATE"), CDate(WW_HPRSINSNYMDB2))
        '                        If CDate(WW_HPRSINSNYMDB2) < T00016INProw("SHUKODATE") Then
        '                            '容器検査切れ
        '                            WW_CheckMES1 = "・更新できないレコード(容器検査切れ)です。(" & WW_LICNPLTNOB2 & " " & T00016INProw("SHARYOTYPEB2") & T00016INProw("TSHABANB2") & " " & WW_HPRSINSNYMDB2 & ")"
        '                            WW_CheckMES2 = ""
        '                            ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, T00016INProw)
        '                        ElseIf CDate(WW_HPRSINSNYMDB2).AddDays(-4) < T00016INProw("SHUKODATE") Then
        '                            '４日前はエラー
        '                            WW_CheckMES1 = "・更新できないレコード(容器検査" & WW_days & "日前)です。(" & WW_LICNPLTNOB2 & " " & T00016INProw("SHARYOTYPEB2") & T00016INProw("TSHABANB2") & " " & WW_HPRSINSNYMDB2 & ")"
        '                            WW_CheckMES2 = ""
        '                            ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, T00016INProw)
        '                        ElseIf CDate(WW_HPRSINSNYMDB2).AddMonths(-2) < T00016INProw("SHUKODATE") Then
        '                            '2カ月前から警告
        '                            WW_CheckMES1 = "・警告レコード(容器検査" & WW_days & "日前)です。(" & WW_LICNPLTNOB2 & " " & T00016INProw("SHARYOTYPEB2") & T00016INProw("TSHABANB2") & " " & WW_HPRSINSNYMDB2 & ")"
        '                            WW_CheckMES2 = ""
        '                            ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.WORNING_RECORD_EXIST, T00016INProw)
        '                        End If
        '                    Else
        '                        'エラー
        '                        WW_CheckMES1 = "・更新できないレコード(車両マスタ不備：次回容器再検査年月日)です。(" & WW_LICNPLTNOB2 & " " & T00016INProw("SHARYOTYPEB2") & T00016INProw("TSHABANB2") & ")"
        '                        WW_CheckMES2 = ""
        '                        ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, T00016INProw)
        '                    End If
        '                End If
        '            End If
        '        End If
        '    End If

        '    '■■■ 集計制御項目チェック（集計KEY必須チェック） ■■■

        '    '荷主受注集計制御マスタ取得
        '    If (WW_LINEerr = C_MESSAGE_NO.NORMAL OrElse WW_LINEerr = C_MESSAGE_NO.WORNING_RECORD_EXIST) AndAlso
        '       WW_TORI_FLG = "OK" AndAlso
        '       WW_OILTYPE_FLG = "OK" Then

        '        GS0029T3CNTLget.CAMPCODE = T00016INProw("CAMPCODE")
        '        GS0029T3CNTLget.TORICODE = T00016INProw("TORICODE")
        '        GS0029T3CNTLget.OILTYPE = T00016INProw("OILTYPE")
        '        GS0029T3CNTLget.ORDERORG = T00016INProw("SHIPORG")
        '        GS0029T3CNTLget.KIJUNDATE = Date.Now
        '        GS0029T3CNTLget.GS0029T3CNTLget()

        '        If isNormal(GS0029T3CNTLget.ERR) Then
        '            If GS0029T3CNTLget.CNTL02 = "1" AndAlso T00016INProw("SHUKODATE") = "" Then     '集計区分(出庫日)
        '                WW_CheckMES1 = "・更新できないレコード(出庫日未入力)です。"
        '                WW_CheckMES2 = ""
        '                ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, T00016INProw)
        '            End If
        '            If GS0029T3CNTLget.CNTL03 = "1" AndAlso T00016INProw("SHUKABASHO") = "" Then    '集計区分(出荷場所)
        '                WW_CheckMES1 = "・更新できないレコード(出荷場所未入力)です。"
        '                WW_CheckMES2 = ""
        '                ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, T00016INProw)
        '            End If
        '            If T00016INProw("SHIPORG") <> WF_DEFORG.Text Then
        '                '他部署は、チェックしない
        '            Else
        '                If GS0029T3CNTLget.CNTL04 = "1" AndAlso T00016INProw("GSHABAN") = "" Then       '集計区分(業務車番)
        '                    WW_CheckMES1 = "・更新できないレコード(業務車番未入力)です。"
        '                    WW_CheckMES2 = ""
        '                    ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, T00016INProw)
        '                End If
        '                If GS0029T3CNTLget.CNTL05 = "1" AndAlso T00016INProw("SHAFUKU") = "" Then       '集計区分(車腹(積載量))
        '                    WW_CheckMES1 = "・更新できないレコード(車腹未入力)です。"
        '                    WW_CheckMES2 = ""
        '                    ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, T00016INProw)
        '                End If
        '                If GS0029T3CNTLget.CNTL06 = "1" AndAlso T00016INProw("STAFFCODE") = "" Then     '集計区分(乗務員コード)
        '                    WW_CheckMES1 = "・更新できないレコード(乗務員未入力)です。"
        '                    WW_CheckMES2 = ""
        '                    ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, T00016INProw)
        '                End If
        '            End If
        '            If GS0029T3CNTLget.CNTL07 = "1" AndAlso T00016INProw("TODOKECODE") = "" Then    '集計区分(届先コード)
        '                WW_CheckMES1 = "・更新できないレコード(届先未入力)です。"
        '                WW_CheckMES2 = ""
        '                ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, T00016INProw)
        '            End If
        '            If GS0029T3CNTLget.CNTL08 = "1" AndAlso T00016INProw("PRODUCT1") = "" Then      '集計区分(品名１)
        '                WW_CheckMES1 = "・更新できないレコード(品名１未入力)です。"
        '                WW_CheckMES2 = ""
        '                ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, T00016INProw)
        '            End If
        '            If GS0029T3CNTLget.CNTL09 = "1" AndAlso T00016INProw("PRODUCTCODE") = "" Then      '集計区分(品名２)
        '                WW_CheckMES1 = "・更新できないレコード(品名２未入力)です。"
        '                WW_CheckMES2 = ""
        '                ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, T00016INProw)
        '            End If
        '            If GS0029T3CNTLget.CNTLVALUE = "1" AndAlso T00016INProw("JDAISU") = "" Then     '集計区分(数量/台数)
        '                WW_CheckMES1 = "・更新できないレコード(台数未入力)です。"
        '                WW_CheckMES2 = ""
        '                ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, T00016INProw)
        '            End If
        '            If GS0029T3CNTLget.CNTLVALUE = "2" AndAlso T00016INProw("JSURYO") = "" Then     '集計区分(数量/台数)
        '                WW_CheckMES1 = "・更新できないレコード(数量未入力)です。"
        '                WW_CheckMES2 = ""
        '                ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, T00016INProw)
        '            End If

        '            If T00016INProw("TODOKEDATE") = "" Then
        '                WW_CheckMES1 = "・更新できないレコード(届日未入力)です。"
        '                WW_CheckMES2 = ""
        '                ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, T00016INProw)
        '            End If
        '            T00016INProw("KIJUNDATE") = T00016INProw("TODOKEDATE")
        '        Else
        '            WW_CheckMES1 = "・更新できないレコード(荷主受注集計制御マスタ登録なし)です。"
        '            WW_CheckMES2 = ""
        '            ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, T00016INProw)
        '        End If

        '    End If

        '    '■■■ 権限チェック（更新権限） ■■■

        '    Dim WW_SHIPORG_ERR As String = ""


        '    '出荷部署
        '    If WW_SHIPORG_ERR = "ON" Then
        '        WW_CheckMES1 = "・更新できないレコード(出荷部署の権限無)です。"
        '        WW_CheckMES2 = ""
        '        ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, T00016INProw)
        '    End If

        '    If T00016INProw("DELFLG") = "" Then
        '        T00016INProw("DELFLG") = "0"
        '    End If

        '    '■ヘッダ項目(実績区分：JISSEKIKBN)
        '    '○必須・項目属性チェック
        '    WW_CS0024FCHECKVAL = T00016INProw("JISSEKIKBN")
        '    Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "JISSEKIKBN", WW_CS0024FCHECKVAL, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT, S0013tbl)
        '    If isNormal(WW_CS0024FCHECKERR) Then
        '        T00016INProw("JISSEKIKBN") = WW_CS0024FCHECKVAL
        '    Else
        '        WW_CheckMES1 = "・エラーが存在します。(実績区分)"
        '        WW_CheckMES2 = WW_CS0024FCHECKREPORT
        '        ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.BOX_ERROR_EXIST, T00016INProw)
        '    End If

        '    '■■■ 各種設定＆名称設定 ■■■

        '    '油種
        '    CODENAME_get("OILTYPE", T00016INProw("OILTYPE"), WW_TEXT, WW_DUMMY)
        '    T00016INProw("OILTYPENAME") = WW_TEXT

        '    '会社名称
        '    CODENAME_get("CAMPCODE", T00016INProw("CAMPCODE"), WW_TEXT, WW_DUMMY)
        '    T00016INProw("CAMPCODENAME") = WW_TEXT

        '    '品名１名称
        '    CODENAME_get("PRODUCT1", T00016INProw("PRODUCT1"), WW_TEXT, WW_DUMMY)
        '    T00016INProw("PRODUCT1NAME") = WW_TEXT

        '    '品名名称
        '    CODENAME_get("PRODUCTCODE", T00016INProw("PRODUCTCODE"), WW_TEXT, WW_DUMMY)
        '    T00016INProw("PRODUCTNAME") = WW_TEXT
        '    '品名２名称
        '    T00016INProw("PRODUCT2NAME") = WW_TEXT

        '    '業務車番名称
        '    CODENAME_get("GSHABAN", T00016INProw("GSHABAN"), WW_TEXT, WW_DUMMY)
        '    T00016INProw("GSHABANLICNPLTNO") = WW_TEXT

        '    '実績区分名称
        '    CODENAME_get("JISSEKIKBN", T00016INProw("JISSEKIKBN"), WW_TEXT, WW_DUMMY)
        '    T00016INProw("JISSEKIKBNNAME") = WW_TEXT

        '    Select Case WW_LINEerr
        '        Case C_MESSAGE_NO.NORMAL
        '        Case C_MESSAGE_NO.WORNING_RECORD_EXIST
        '            T00016INProw("SELECT") = 1
        '            T00016INProw("OPERATION") = C_LIST_OPERATION_CODE.WARNING
        '        Case C_MESSAGE_NO.BOX_ERROR_EXIST
        '            T00016INProw("SELECT") = 1
        '            T00016INProw("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
        '        Case C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        '            T00016INProw("SELECT") = 1
        '            T00016INProw("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
        '    End Select

        'Next

        If WW_ERRLIST.Count > 0 Then
            If WW_ERRLIST.Contains(C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR) Then
                O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            ElseIf WW_ERRLIST.Contains(C_MESSAGE_NO.BOX_ERROR_EXIST) Then
                O_RTNCODE = C_MESSAGE_NO.BOX_ERROR_EXIST
            Else
                O_RTNCODE = C_MESSAGE_NO.WORNING_RECORD_EXIST
            End If
        End If

    End Sub

    ''' <summary>
    ''' エラーレポート編集
    ''' </summary>
    ''' <param name="I_MESSAGE1"></param>
    ''' <param name="I_MESSAGE2"></param>
    ''' <param name="I_ERRCD"></param>
    ''' <remarks></remarks>
    Protected Sub ERRMESSAGE_write(ByRef I_MESSAGE1 As String, ByRef I_MESSAGE2 As String, ByRef WW_LINEerr As String, ByRef i As Integer, ByVal I_ERRCD As String, ByVal T00016INProw As DataRow)

        'エラーレポート編集
        Dim WW_ERR_MES As String = String.Empty
        WW_ERR_MES = I_MESSAGE1
        If Not String.IsNullOrEmpty(I_MESSAGE2) Then
            WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> " & I_MESSAGE2 & " , "
        End If
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 項番　　= @L" & i.ToString("0000") & "L@ , "
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 明細番号= @D" & i.ToString("000") & "D@ , "
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 取引先　=" & T00016INProw("TORICODE") & " , "
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 届先　　=" & T00016INProw("TODOKECODE") & " , "
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 出荷場所=" & T00016INProw("SHUKABASHO") & " , "
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 出庫日　=" & T00016INProw("SHUKODATE") & " , "
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 届日　　=" & T00016INProw("TODOKEDATE") & " , "
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 出荷日　=" & T00016INProw("SHUKADATE") & " , "
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 車番　　=" & T00016INProw("GSHABAN") & " , "
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 乗務員　=" & T00016INProw("STAFFCODE") & " , "
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 品名  　=" & T00016INProw("PRODUCTCODE") & " , "
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> ﾄﾘｯﾌﾟ 　=" & T00016INProw("TRIPNO") & " , "
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> ﾄﾞﾛｯﾌﾟ　=" & T00016INProw("DROPNO") & " , "
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 削除　　=" & T00016INProw("DELFLG") & " "
        rightview.AddErrorReport(WW_ERR_MES)

        WW_ERRLIST.Add(I_ERRCD)
        If WW_LINEerr <> C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR Then
            WW_LINEerr = I_ERRCD
        End If

    End Sub

    ''' <summary>
    ''' エラーレポート編集（JSRフォーマット）
    ''' </summary>
    ''' <param name="I_MESSAGE1"></param>
    ''' <param name="I_MESSAGE2"></param>
    ''' <param name="I_ERRCD"></param>
    ''' <remarks></remarks>
    Protected Sub ERRMESSAGE_write_NJS(ByRef I_MESSAGE1 As String, ByRef I_MESSAGE2 As String, ByRef WW_LINEerr As String, ByRef i As Integer, ByVal I_ERRCD As String, ByVal JSRINProw As DataRow)

        'エラーレポート編集
        Dim WW_ERR_MES As String = String.Empty
        WW_ERR_MES = I_MESSAGE1
        If Not String.IsNullOrEmpty(I_MESSAGE2) Then
            WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> " & I_MESSAGE2 & " , "
        End If
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 項番　　     = @L" & i.ToString("0000") & "L@ , "
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 契約番号　   =" & JSRINProw("CONTRACTNO") & " , "
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 出庫日　　   =" & JSRINProw("SHUKODATE") & " , "
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 納入日       =" & JSRINProw("TODOKEDATE") & " , "
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 届先コード　 =" & JSRINProw("TODOKECODE") & " , "
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 届先略称　　 =" & JSRINProw("TODOKECODENAME") & " , "
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 倉庫コード　 =" & JSRINProw("SHUKABASHO") & " , "
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 倉庫略称　　 =" & JSRINProw("SHUKABASHONAME") & " , "
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 品名コード　 =" & JSRINProw("PRODUCTCODE") & " , "
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 車輌コード　 =" & JSRINProw("SHARYOCD") & " , "
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 運転手コード1=" & JSRINProw("STAFFCODE1") & " , "
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 運転手コード2=" & JSRINProw("STAFFCODE2") & " , "
        rightview.AddErrorReport(WW_ERR_MES)

        WW_ERRLIST.Add(I_ERRCD)
        If WW_LINEerr <> C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR Then
            WW_LINEerr = I_ERRCD
        End If

    End Sub

    ''' <summary>
    ''' 同一オーダー判定
    ''' </summary>
    ''' <remarks></remarks>
    Protected Function CompareOrder(ByRef src As DataRow, ByRef dst As DataRow) As Boolean

        '伝票区分、伝票番号、取引日付、荷主（取引先）、届先、業務車番、荷主車番、取引発生部署　が同一
        If src("DENKBN") = dst("DENKBN") AndAlso
           src("DENNO") = dst("DENNO") AndAlso
           src("TORIHIKIYMD") = dst("TORIHIKIYMD") AndAlso
           src("TORICODE") = dst("TORICODE") AndAlso
           src("TODOKECODE") = dst("TODOKECODE") AndAlso
           src("NSHABAN") = dst("NSHABAN") AndAlso
           src("TORIHIKIORG") = dst("TORIHIKIORG") Then

            Return True
        Else
            Return False
        End If

    End Function

    ''' <summary>
    ''' 画面グリッドのデータを取得しファイルに保存する。
    ''' </summary>
    Private Sub FileSaveDisplayInput()
        'そもそも画面表示データがない状態の場合はそのまま終了
        If ViewState("DISPLAY_LINECNT_LIST") Is Nothing Then
            Return
        End If
        Dim displayLineCnt = DirectCast(ViewState("DISPLAY_LINECNT_LIST"), List(Of Integer))

        '○ 画面表示データ復元
        Master.RecoverTable(T00016tbl)

        'この段階でありえないがデータテーブルがない場合は終了
        If T00016tbl Is Nothing OrElse T00016tbl.Rows.Count = 0 Then
            Return
        End If

        'サフィックス抜き（LISTID)抜きのオブジェクト名リスト
        Dim objChkPrifix As String = "ctl00$contents1$chk" & Me.pnlListArea1.ID
        Dim fieldIdList As New Dictionary(Of String, String) From {{"ROWDEL", objChkPrifix}}

        Dim formToPost = New NameValueCollection(Request.Form)
        For Each i In displayLineCnt
            For Each fieldId As KeyValuePair(Of String, String) In fieldIdList
                Dim dispObjId As String = fieldId.Value & fieldId.Key & i
                Dim displayValue As String = ""
                If Request.Form.AllKeys.Contains(dispObjId) Then
                    displayValue = Request.Form(dispObjId)
                    formToPost.Remove(dispObjId)
                End If
                For Each row In T00016tbl.Rows
                    If row("LINECNT") = i Then
                        Dim before As String = row(fieldId.Key)
                        If displayValue = "on" Then
                            row(fieldId.Key) = "1"
                        Else
                            row(fieldId.Key) = "0"
                        End If
                    End If
                Next
            Next
        Next

        '○ 画面表示データ保存
        Master.SaveTable(T00016tbl)

        Return
    End Sub

#End Region

#Region "左BOX関連"

    ''' <summary>
    ''' フィールドダブルクリック処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_Field_DBClick()

        If String.IsNullOrEmpty(WF_LeftMViewChange.Value) Then Exit Sub
        If Not Integer.TryParse(WF_LeftMViewChange.Value, WF_LeftMViewChange.Value) Then Exit Sub

        Dim WW_FIELD As String = ""
        If WF_FIELD_REP.Value = "" Then
            WW_FIELD = WF_FIELD.Value
        Else
            WW_FIELD = WF_FIELD_REP.Value
        End If

        WF_LeftMView.ActiveViewIndex = -1
        If WF_LeftMViewChange.Value = LIST_BOX_CLASSIFICATION.LC_CALENDAR Then
            '日付の場合、入力日付のカレンダーが表示されるように入力値をカレンダーに渡す
            Dim obj = work.getControl(WW_FIELD)
            Dim txtBox = DirectCast(obj, TextBox)
            leftview.WF_Calendar.Text = txtBox.Text
            leftview.ActiveCalendar()

        ElseIf WF_LeftMViewChange.Value = LIST_BOX_CLASSIFICATION.LC_EXTRA_LIST Then
            '○画面表示データ復元
            Master.RecoverTable(T00016tbl)
            Select Case WW_FIELD
                Case "WF_GSHABAN"
                    WF_GSHABAN_Rep.Visible = True
                    DataBindGSHABAN()
                    WF_LeftMView.ActiveViewIndex = 0
            End Select
            WF_LeftMView.Visible = True

        Else
            Dim prmData As Hashtable = work.createFIXParam(work.WF_SEL_CAMPCODE.Text)

            'フィールドによってパラメーターを変える
            Select Case WW_FIELD
                Case "WF_CAMPCODE"                              '会社コード
                Case "WF_SELTORICODE"
                    prmData = work.createTORIParam(work.WF_SEL_CAMPCODE.Text, work.WF_SEL_SHIPORG.Text)
                Case "WF_TORICODE"
                    prmData = work.createTORIParam(work.WF_SEL_CAMPCODE.Text, work.WF_SEL_SHIPORG.Text)
                Case "WF_OILTYPE"                               '油種
                    prmData = work.createOilTypeParam(work.WF_SEL_CAMPCODE.Text)
                Case "WF_SHIPORG"                               '出荷部署
                    prmData = work.createORGParam(work.WF_SEL_CAMPCODE.Text, False)
                Case "WF_STAFFCODE",
                    "WF_SUBSTAFFCODE"                           '乗務員・副乗務員
                    prmData = work.createSTAFFParam(work.WF_SEL_CAMPCODE.Text, work.WF_SEL_SHIPORG.Text)
                Case "PRODUCT1"                                 '品名１
                    prmData = work.createGoods1Param(work.WF_SEL_CAMPCODE.Text)
                Case "PRODUCTCODE"                              '品名コード
                    prmData = work.createGoodsParam(work.WF_SEL_CAMPCODE.Text, work.WF_SEL_SHIPORG.Text, True)
                Case "TODOKECODE"                               '届先
                    prmData = work.createDistinationParam(work.WF_SEL_CAMPCODE.Text, work.WF_SEL_SHIPORG.Text, "", "1", True)
                Case "SHUKABASHO"                               '出荷場所
                    prmData = work.createDistinationParam(work.WF_SEL_CAMPCODE.Text, work.WF_SEL_SHIPORG.Text, "", "2", True)
                Case "DELFLG"                                   '削除フラグ
                    prmData.Item(C_PARAMETERS.LP_TYPEMODE) = "2"
            End Select
            leftview.SetListBox(WF_LeftMViewChange.Value, WW_DUMMY, prmData)
            leftview.ActiveListBox()
        End If
        WF_LeftMViewChange.Value = ""

    End Sub

    ''' <summary>
    '''  LeftBOX選択ボタン処理(ListBox値 ---> detailbox)
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonSel_Click()

        Dim WW_SelectTEXT As String = ""
        Dim WW_SelectValue As String = ""
        Dim WW_PARAM1 As String = ""
        Dim WW_PARAM2 As String = ""
        Dim WW_PARAM3 As String = ""
        Dim WW_PARAM4 As String = ""
        Dim WW_PARAM5 As String = ""
        Dim WW_PARAM6 As String = ""
        Dim WW_PARAM7 As String = ""
        Dim WW_PARAM8 As String = ""
        Dim WW_PARAM9 As String = ""
        Dim WW_PARAM10 As String = ""
        Dim WW_PARAM11 As String = ""
        Dim WW_PARAM12 As String = ""
        Dim WW_PARAM13 As String = ""
        Dim WW_PARAM14 As String = ""
        Dim WW_PARAM15 As String = ""
        Dim WW_PARAM16 As String = ""
        Dim WW_PARAM17 As String = ""
        Dim WW_PARAM18 As String = ""
        Dim WW_PARAM19 As String = ""
        Dim WW_PARAM20 As String = ""

        Dim WW_ACTIVE_VALUE As String()
        Select Case WF_LeftMView.ActiveViewIndex
            Case 0
                If Not String.IsNullOrEmpty(WF_SelectedIndex.Value) Then
                    WW_SelectValue = CType(WF_GSHABAN_Rep.Items(CInt(WF_SelectedIndex.Value)).FindControl("WF_GSHABAN_ItemCell1"), System.Web.UI.WebControls.TableCell).Text
                    WW_SelectTEXT = CType(WF_GSHABAN_Rep.Items(CInt(WF_SelectedIndex.Value)).FindControl("WF_GSHABAN_ItemCell7"), System.Web.UI.WebControls.TableCell).Text
                    WW_PARAM1 = CType(WF_GSHABAN_Rep.Items(CInt(WF_SelectedIndex.Value)).FindControl("WF_GSHABAN_ItemCell8"), System.Web.UI.WebControls.TableCell).Text
                    WW_PARAM2 = CType(WF_GSHABAN_Rep.Items(CInt(WF_SelectedIndex.Value)).FindControl("WF_GSHABAN_ItemCell9"), System.Web.UI.WebControls.TableCell).Text
                    WW_PARAM3 = CType(WF_GSHABAN_Rep.Items(CInt(WF_SelectedIndex.Value)).FindControl("WF_GSHABAN_ItemCell10"), System.Web.UI.WebControls.TableCell).Text
                    WW_PARAM4 = CType(WF_GSHABAN_Rep.Items(CInt(WF_SelectedIndex.Value)).FindControl("WF_GSHABAN_ItemCell11"), System.Web.UI.WebControls.TableCell).Text
                    WW_PARAM5 = CType(WF_GSHABAN_Rep.Items(CInt(WF_SelectedIndex.Value)).FindControl("WF_GSHABAN_ItemCell12"), System.Web.UI.WebControls.TableCell).Text
                    WW_PARAM6 = CType(WF_GSHABAN_Rep.Items(CInt(WF_SelectedIndex.Value)).FindControl("WF_GSHABAN_ItemCell13"), System.Web.UI.WebControls.TableCell).Text
                    WW_PARAM7 = CType(WF_GSHABAN_Rep.Items(CInt(WF_SelectedIndex.Value)).FindControl("WF_GSHABAN_ItemCell14"), System.Web.UI.WebControls.TableCell).Text
                    WW_PARAM8 = WF_ListSHARYOINFO1.Items(WF_SelectedIndex.Value).Value
                    WW_PARAM9 = WF_ListSHARYOINFO2.Items(WF_SelectedIndex.Value).Value
                    WW_PARAM10 = WF_ListSHARYOINFO3.Items(WF_SelectedIndex.Value).Value
                    WW_PARAM11 = WF_ListSHARYOINFO4.Items(WF_SelectedIndex.Value).Value
                    WW_PARAM12 = WF_ListSHARYOINFO5.Items(WF_SelectedIndex.Value).Value
                    WW_PARAM13 = WF_ListSHARYOINFO6.Items(WF_SelectedIndex.Value).Value
                    WW_PARAM14 = WF_ListSHAFUKU.Items(WF_SelectedIndex.Value).Value
                    WW_PARAM15 = WF_ListTSHABANF.Items(WF_SelectedIndex.Value).Value
                    WW_PARAM16 = WF_ListTSHABANB.Items(WF_SelectedIndex.Value).Value
                    WW_PARAM17 = WF_ListTSHABANB2.Items(WF_SelectedIndex.Value).Value
                    WW_PARAM18 = WF_ListLICNPLTNOF.Items(WF_SelectedIndex.Value).Value
                    WW_PARAM19 = WF_ListLICNPLTNOB.Items(WF_SelectedIndex.Value).Value
                    WW_PARAM20 = WF_ListLICNPLTNOB2.Items(WF_SelectedIndex.Value).Value
                End If
            Case Else
                If leftview.WF_LeftListBox.SelectedIndex >= 0 Then
                    WF_SelectedIndex.Value = leftview.WF_LeftListBox.SelectedIndex
                    WW_ACTIVE_VALUE = leftview.GetActiveValue
                    WW_SelectValue = WW_ACTIVE_VALUE(0)
                    WW_SelectTEXT = WW_ACTIVE_VALUE(1)
                End If

        End Select

        If WF_FIELD_REP.Value = "" Then
            '変更
            WF_REP_Change.Value = "1"

            Select Case WF_FIELD.Value

                Case "WF_SHUKODATE",
                    "WF_SHUKADATE",
                    "WF_TODOKEDATE",
                    "WF_KIKODATE"
                    'カレンダー関係
                    ' 出庫日
                    ' 出荷日
                    ' 届日
                    ' 帰庫日
                    Dim obj = work.getControl(WF_FIELD.Value)
                    Dim txtBox As TextBox = DirectCast(obj, TextBox)
                    txtBox.Text = WW_SelectValue
                    txtBox.Focus()

                Case "WF_SELTORICODE",
                    "WF_SELSHIPORG",
                    "WF_TORICODE",
                    "WF_OILTYPE",
                    "WF_SHIPORG",
                    "WF_STAFFCODE",
                    "WF_SUBSTAFFCODE",
                    "WF_JISSEKIKBN"

                    '取引先（絞込）
                    '受注受付部署
                    '取引先
                    '油種
                    '受注受付部署
                    '乗務員
                    '副乗務員
                    '実績区分
                    Dim obj = work.getControl(WF_FIELD.Value)
                    Dim objText = work.getControl(WF_FIELD.Value & "_TEXT")
                    Dim txtBox As TextBox = DirectCast(obj, TextBox)
                    Dim lblText As Label = DirectCast(objText, Label)

                    lblText.Text = WW_SelectTEXT
                    txtBox.Text = WW_SelectValue
                    txtBox.Focus()

                    'Case "WF_GSHABAN"
                    '    '業務車番
                    '    WF_GSHABAN.Text = WW_SelectValue
                    '    Repeater_Value("SHARYOINFO1", WW_PARAM8, "H")
                    '    Repeater_Value("SHARYOINFO2", WW_PARAM9, "H")
                    '    Repeater_Value("SHARYOINFO3", WW_PARAM10, "H")
                    '    Repeater_Value("SHARYOINFO4", WW_PARAM11, "H")
                    '    Repeater_Value("SHARYOINFO5", WW_PARAM12, "H")
                    '    Repeater_Value("SHARYOINFO6", WW_PARAM13, "H")
                    '    WF_SHAFUKU.Text = WW_PARAM14
                    '    WF_TSHABANF.Text = WW_PARAM15
                    '    WF_TSHABANB.Text = WW_PARAM16
                    '    WF_TSHABANB2.Text = WW_PARAM17
                    '    WF_TSHABANF_TEXT.Text = WW_PARAM18
                    '    WF_TSHABANB_TEXT.Text = WW_PARAM19
                    '    WF_TSHABANB2_TEXT.Text = WW_PARAM20

                    '    WF_GSHABAN.Focus()

            End Select

        End If

        '○ 画面左サイドボックス非表示は、画面JavaScript(InitLoad)で実行
        WF_GSHABAN_Rep.Dispose()
        WF_GSHABAN_Rep = Nothing

        WF_LeftMView.Visible = False
        WF_LeftboxOpen.Value = ""
        WF_LeftMViewChange.Value = ""
        WF_FIELD_REP.Value = ""
        WF_FIELD.Value = ""

    End Sub

    ''' <summary>
    ''' LeftBoxより名称取得＆チェック
    ''' </summary>
    ''' <param name="I_FIELD"></param>
    ''' <param name="I_VALUE"></param>
    ''' <param name="O_TEXT"></param>
    ''' <param name="O_RTN"></param>
    ''' <remarks></remarks>
    Protected Sub CODENAME_get(ByVal I_FIELD As String, ByRef I_VALUE As String, ByRef O_TEXT As String, ByRef O_RTN As String, Optional ByVal args As Hashtable = Nothing)

        '○名称取得
        O_TEXT = ""
        O_RTN = C_MESSAGE_NO.NORMAL

        '入力値が空は終了
        If String.IsNullOrEmpty(I_VALUE) Then Exit Sub
        With leftview
            Select Case I_FIELD
                Case "CAMPCODE"
                    '会社名称
                    .CodeToName(LIST_BOX_CLASSIFICATION.LC_COMPANY, I_VALUE, O_TEXT, O_RTN)
                Case "TORICODE"
                    '取引先名称
                    .CodeToName(LIST_BOX_CLASSIFICATION.LC_CUSTOMER, I_VALUE, O_TEXT, O_RTN, work.createTORIParam(work.WF_SEL_CAMPCODE.Text))
                Case "SHUKABASHO"
                    '出荷場所名称
                    If IsNothing(args) Then
                        .CodeToName(LIST_BOX_CLASSIFICATION.LC_DISTINATION, I_VALUE, O_TEXT, O_RTN, work.createDistinationParam(work.WF_SEL_CAMPCODE.Text, work.WF_SEL_SHIPORG.Text, "", "2", True))
                    Else
                        .CodeToName(LIST_BOX_CLASSIFICATION.LC_DISTINATION, I_VALUE, O_TEXT, O_RTN, args)
                    End If
                Case "TODOKECODE"
                    '届先名称
                    If IsNothing(args) Then
                        .CodeToName(LIST_BOX_CLASSIFICATION.LC_DISTINATION, I_VALUE, O_TEXT, O_RTN, work.createDistinationParam(work.WF_SEL_CAMPCODE.Text, work.WF_SEL_SHIPORG.Text, "", "1", True))
                    Else
                        .CodeToName(LIST_BOX_CLASSIFICATION.LC_DISTINATION, I_VALUE, O_TEXT, O_RTN, args)
                    End If
                Case "NSHABAN"
                    '荷主車番名称
                    .CodeToName(LIST_BOX_CLASSIFICATION.LC_WORKLORRY, I_VALUE, O_TEXT, O_RTN, work.createWorkLorryParam(work.WF_SEL_CAMPCODE.Text, work.WF_SEL_SHIPORG.Text))
                Case "SHAFUKU"
                    '車複名称
                    .CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.createFIXParam(work.WF_SEL_CAMPCODE.Text, "SHAFUKU"))
                Case "BUNRUI"
                    '分類名称
                    .CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.createFIXParam(work.WF_SEL_CAMPCODE.Text, "BUNRUI"))
                Case "SOUSA"
                    '操作名称
                    .CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.createFIXParam(work.WF_SEL_CAMPCODE.Text, "SOUSA"))
                Case "TORIHIKIORG"
                    '取引発生部署名称
                    .CodeToName(LIST_BOX_CLASSIFICATION.LC_ORG, I_VALUE, O_TEXT, O_RTN, work.createORGParam(work.WF_SEL_CAMPCODE.Text, False))
                Case "DELFLG"
                    '削除名称
                    .CodeToName(LIST_BOX_CLASSIFICATION.LC_DELFLG, I_VALUE, O_TEXT, O_RTN, work.createFIXParam(work.WF_SEL_CAMPCODE.Text, "DELFLG"))

            End Select
        End With

    End Sub

    ''' <summary>
    ''' 左リストボックスダブルクリック時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_Listbox_DBClick()
        WF_ButtonSel_Click()
        WF_FIELD_REP.Value = ""
        WF_FIELD.Value = ""
    End Sub

    ''' <summary>
    ''' LeftBOXキャンセルボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonCan_Click()

        'メモリ開放
        WF_GSHABAN_Rep.Visible = False
        WF_GSHABAN_Rep.Dispose()
        WF_GSHABAN_Rep = Nothing

        '○画面左サイドボックス非表示は、画面JavaScript(InitLoad)で実行
        WF_FIELD.Value = ""
        WF_FIELD_REP.Value = ""
        WF_LeftboxOpen.Value = ""

    End Sub


    ''' <summary>
    ''' LeftBox業務車番データ取得
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub InitGSHABAN()

        WF_ListGSHABAN.Items.Clear()
        WF_ListTSHABANF.Items.Clear()
        WF_ListTSHABANB.Items.Clear()
        WF_ListTSHABANB2.Items.Clear()
        WF_ListLICNPLTNOF.Items.Clear()
        WF_ListLICNPLTNOB.Items.Clear()
        WF_ListLICNPLTNOB2.Items.Clear()
        WF_ListSHARYOINFO1.Items.Clear()
        WF_ListSHARYOINFO2.Items.Clear()
        WF_ListSHARYOINFO3.Items.Clear()
        WF_ListSHARYOINFO4.Items.Clear()
        WF_ListSHARYOINFO5.Items.Clear()
        WF_ListSHARYOINFO6.Items.Clear()
        WF_ListOILTYPE.Items.Clear()
        WF_ListOILTYPENAME.Items.Clear()
        WF_ListSHAFUKU.Items.Clear()
        WF_ListOWNCODE.Items.Clear()
        WF_ListOWNCODENAME.Items.Clear()
        WF_ListSHARYOSTATUS.Items.Clear()
        WF_ListSHARYOSTATUSNAME.Items.Clear()
        WF_ListHPRSINSNYMDF.Items.Clear()
        WF_ListHPRSINSNYMDB.Items.Clear()
        WF_ListHPRSINSNYMDB2.Items.Clear()
        WF_ListLICNYMDF.Items.Clear()
        WF_ListLICNYMDB.Items.Clear()
        WF_ListLICNYMDB2.Items.Clear()

        '○　業務車番Table設定
        Try
            'DataBase接続文字
            Using SQLcon = CS0050SESSION.getConnection
                SQLcon.Open() 'DataBase接続(Open)

                Dim SQLStr As String =
                     " SELECT isnull(rtrim(A.GSHABAN),'') 		as GSHABAN ,   		    " _
                   & "        isnull(rtrim(A.KOEISHABAN),'') 	as KOEISHABAN ,		    " _
                   & "        isnull(rtrim(A.SHARYOTYPEF),'') +                         " _
                   & "        isnull(rtrim(A.TSHABANF),'')      as TSHABANF ,           " _
                   & "        isnull(rtrim(A.SHARYOTYPEB),'') +                         " _
                   & "        isnull(rtrim(A.TSHABANB),'')      as TSHABANB ,           " _
                   & "        isnull(rtrim(A.SHARYOTYPEB2),'') +                        " _
                   & "        isnull(rtrim(A.TSHABANB2),'')     as TSHABANB2 ,          " _
                   & "        isnull(rtrim(A.TSHABANFNAMES),'') as TSHABANFNAMES ,      " _
                   & "        isnull(rtrim(A.TSHABANBNAMES),'') as TSHABANBNAMES ,      " _
                   & "        isnull(rtrim(A.TSHABANB2NAMES),'') as TSHABANB2NAMES ,    " _
                   & "        isnull(rtrim(A.SHARYOINFO1),'') 	as SHARYOINFO1 ,        " _
                   & "        isnull(rtrim(A.SHARYOINFO2),'') 	as SHARYOINFO2 ,        " _
                   & "        isnull(rtrim(A.SHARYOINFO3),'') 	as SHARYOINFO3 ,        " _
                   & "        isnull(rtrim(A.SHARYOINFO4),'') 	as SHARYOINFO4 ,        " _
                   & "        isnull(rtrim(A.SHARYOINFO5),'') 	as SHARYOINFO5 ,        " _
                   & "        isnull(rtrim(A.SHARYOINFO6),'') 	as SHARYOINFO6 ,        " _
                   & "        isnull(rtrim(B.MANGOILTYPE),'') 	as OILTYPE ,            " _
                   & "        isnull(rtrim(C.VALUE1),'') 	    as OILTYPENAME ,        " _
                   & "        isnull(rtrim(B.MANGSHAFUKU),'')	as SHAFUKU ,   	        " _
                   & "        isnull(rtrim(B.MANGOWNCODE),'') 	as OWNCODE ,            " _
                   & "        isnull(rtrim(D.NAMES),'') 	    as OWNCODENAME ,        " _
                   & "        isnull(rtrim(E.KEYCODE),'')       as SHARYOSTATUS ,       " _
                   & "        isnull(rtrim(E.VALUE1),'')        as SHARYOSTATUSNAME ,   " _
                   & "        isnull(rtrim(F.HPRSINSNYMD),'')   as HPRSINSNYMDF,        " _
                   & "        isnull(rtrim(F.LICNYMD),'')       as LICNYMDF,            " _
                   & "        isnull(rtrim(G.HPRSINSNYMD),'')   as HPRSINSNYMDB,        " _
                   & "        isnull(rtrim(G.LICNYMD),'')       as LICNYMDB,            " _
                   & "        isnull(rtrim(H.HPRSINSNYMD),'')   as HPRSINSNYMDB2,       " _
                   & "        isnull(rtrim(H.LICNYMD),'')       as LICNYMDB2            " _
                   & "   FROM MA006_SHABANORG   as A                        " _
                   & "   LEFT JOIN MA002_SHARYOA B 						    " _
                   & "     ON B.CAMPCODE   	= A.CAMPCODE 				    " _
                   & "    and B.SHARYOTYPE  = A.SHARYOTYPEB 		        " _
                   & "    and B.TSHABAN     = A.TSHABANB 		            " _
                   & "    and B.STYMD      <= @P1                           " _
                   & "    and B.ENDYMD     >= @P1                           " _
                   & "    and B.DELFLG     <> '1' 						    " _
                   & "   LEFT JOIN MC001_FIXVALUE C 					    " _
                   & "     ON C.CAMPCODE   	= B.CAMPCODE       	            " _
                   & "    and C.CLASS       = 'MANGOILTYPE' 			    " _
                   & "    and C.KEYCODE     = B.MANGOILTYPE 			    " _
                   & "    and C.STYMD      <= @P1                           " _
                   & "    and C.ENDYMD     >= @P1                           " _
                   & "    and C.DELFLG     <> '1' 						    " _
                   & "   LEFT JOIN MC002_TORIHIKISAKI D 				    " _
                   & "     ON D.CAMPCODE   	 = B.CAMPCODE    			    " _
                   & "    and D.TORICODE   	 = B.MANGOWNCODE 			    " _
                   & "    and D.STYMD      <= @P1                           " _
                   & "    and D.ENDYMD     >= @P1                           " _
                   & "    and D.DELFLG     <> '1' 						    " _
                   & "   LEFT JOIN MC001_FIXVALUE E 					    " _
                   & "     ON E.CAMPCODE   	= B.CAMPCODE        		    " _
                   & "    and E.CLASS       = 'SHARYOSTATUS' 			    " _
                   & "    and E.KEYCODE     = B.SHARYOSTATUS 			    " _
                   & "    and E.STYMD      <= @P1                           " _
                   & "    and E.ENDYMD     >= @P1                           " _
                   & "    and E.DELFLG     <> '1' 						    " _
                   & "   LEFT JOIN MA004_SHARYOC F 						    " _
                   & "     ON F.CAMPCODE  　= A.CAMPCODE 				    " _
                   & "    and F.SHARYOTYPE  = A.SHARYOTYPEF 		        " _
                   & "    and F.TSHABAN     = A.TSHABANF 			        " _
                   & "    and F.STYMD      <= @P1                           " _
                   & "    and F.ENDYMD     >= @P1                           " _
                   & "    and F.DELFLG     <> '1' 						    " _
                   & "   LEFT JOIN MA004_SHARYOC G 						    " _
                   & "     ON G.CAMPCODE   	= A.CAMPCODE 				    " _
                   & "    and G.SHARYOTYPE  = A.SHARYOTYPEB 		        " _
                   & "    and G.TSHABAN     = A.TSHABANB 	                " _
                   & "    and G.STYMD      <= @P1                           " _
                   & "    and G.ENDYMD     >= @P1                           " _
                   & "    and G.DELFLG     <> '1' 						    " _
                   & "   LEFT JOIN MA004_SHARYOC H 						    " _
                   & "     ON H.CAMPCODE   	= A.CAMPCODE 				    " _
                   & "    and H.SHARYOTYPE  = A.SHARYOTYPEB2 		        " _
                   & "    and H.TSHABAN     = A.TSHABANB2 	                " _
                   & "    and H.STYMD      <= @P1                           " _
                   & "    and H.ENDYMD     >= @P2                           " _
                   & "    and H.DELFLG     <> '1' 						    " _
                   & "  Where A.CAMPCODE  = @P3                             " _
                   & "    and A.MANGUORG  = @P4                             " _
                   & "    and A.DELFLG   <> '1'                             " _
                   & "  ORDER BY A.SEQ ,A.GSHABAN                           "

                Dim SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.Date)
                Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.Date)
                Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.NVarChar)
                Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", System.Data.SqlDbType.NVarChar)

                Dim WW_DATE As Date
                If Date.TryParse(work.WF_SEL_SEIKYUYMF.Text, WW_DATE) Then
                    PARA1.Value = WW_DATE.AddMonths(1).AddDays(-1).ToString("yyyy/MM/dd")
                    PARA2.Value = work.WF_SEL_SEIKYUYMF.Text & "/01"
                End If

                PARA3.Value = work.WF_SEL_CAMPCODE.Text
                If String.IsNullOrEmpty(work.WF_SEL_SHIPORG.Text) Then
                    PARA4.Value = WF_DEFORG.Text
                Else
                    PARA4.Value = work.WF_SEL_SHIPORG.Text
                End If

                '○SQL実行
                Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                '○出力設定
                While SQLdr.Read
                    WF_ListGSHABAN.Items.Add(New ListItem(SQLdr("GSHABAN"), SQLdr("GSHABAN")))
                    WF_ListSHARYOINFO1.Items.Add(New ListItem(SQLdr("GSHABAN"), SQLdr("SHARYOINFO1")))
                    WF_ListSHARYOINFO2.Items.Add(New ListItem(SQLdr("GSHABAN"), SQLdr("SHARYOINFO2")))
                    WF_ListSHARYOINFO3.Items.Add(New ListItem(SQLdr("GSHABAN"), SQLdr("SHARYOINFO3")))
                    WF_ListSHARYOINFO4.Items.Add(New ListItem(SQLdr("GSHABAN"), SQLdr("SHARYOINFO4")))
                    WF_ListSHARYOINFO5.Items.Add(New ListItem(SQLdr("GSHABAN"), SQLdr("SHARYOINFO5")))
                    WF_ListSHARYOINFO6.Items.Add(New ListItem(SQLdr("GSHABAN"), SQLdr("SHARYOINFO6")))
                    WF_ListOILTYPE.Items.Add(New ListItem(SQLdr("GSHABAN"), SQLdr("OILTYPE")))
                    WF_ListOILTYPENAME.Items.Add(New ListItem(SQLdr("GSHABAN"), SQLdr("OILTYPENAME")))
                    WF_ListSHAFUKU.Items.Add(New ListItem(SQLdr("GSHABAN"), SQLdr("SHAFUKU")))
                    WF_ListOWNCODE.Items.Add(New ListItem(SQLdr("GSHABAN"), SQLdr("OWNCODE")))
                    WF_ListOWNCODENAME.Items.Add(New ListItem(SQLdr("GSHABAN"), SQLdr("OWNCODENAME")))
                    WF_ListSHARYOSTATUS.Items.Add(New ListItem(SQLdr("GSHABAN"), SQLdr("SHARYOSTATUS")))
                    WF_ListSHARYOSTATUSNAME.Items.Add(New ListItem(SQLdr("GSHABAN"), SQLdr("SHARYOSTATUSNAME")))
                    WF_ListLICNPLTNOF.Items.Add(New ListItem(SQLdr("GSHABAN"), SQLdr("TSHABANFNAMES")))
                    WF_ListLICNPLTNOB.Items.Add(New ListItem(SQLdr("GSHABAN"), SQLdr("TSHABANBNAMES")))
                    WF_ListLICNPLTNOB2.Items.Add(New ListItem(SQLdr("GSHABAN"), SQLdr("TSHABANB2NAMES")))
                    WF_ListTSHABANF.Items.Add(New ListItem(SQLdr("GSHABAN"), SQLdr("TSHABANF")))
                    WF_ListTSHABANB.Items.Add(New ListItem(SQLdr("GSHABAN"), SQLdr("TSHABANB")))
                    WF_ListTSHABANB2.Items.Add(New ListItem(SQLdr("GSHABAN"), SQLdr("TSHABANB2")))
                    WF_ListHPRSINSNYMDF.Items.Add(New ListItem(SQLdr("GSHABAN"), SQLdr("HPRSINSNYMDF")))
                    WF_ListHPRSINSNYMDB.Items.Add(New ListItem(SQLdr("GSHABAN"), SQLdr("HPRSINSNYMDB")))
                    WF_ListHPRSINSNYMDB2.Items.Add(New ListItem(SQLdr("GSHABAN"), SQLdr("HPRSINSNYMDB2")))
                    WF_ListLICNYMDF.Items.Add(New ListItem(SQLdr("GSHABAN"), SQLdr("LICNYMDF")))
                    WF_ListLICNYMDB.Items.Add(New ListItem(SQLdr("GSHABAN"), SQLdr("LICNYMDB")))
                    WF_ListLICNYMDB2.Items.Add(New ListItem(SQLdr("GSHABAN"), SQLdr("LICNYMDB2")))

                End While

                'Close()
                SQLdr.Close() 'Reader(Close)
                SQLdr = Nothing

                SQLcmd.Dispose()
                SQLcmd = Nothing

                SQLcon.Close() 'DataBase接続(Close)
            End Using

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "GSHABAN SELECT")
            CS0011LOGWRITE.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:GSHABAN Select"           '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                  '
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try

    End Sub

    ''' <summary>
    ''' LeftBox業務車番DataBind
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub DataBindGSHABAN()

        Dim GSHABANtbl As New DataTable()
        Dim GSHABANrow As DataRow

        '○カラム設定
        GSHABANtbl.Columns.Add("GSHABAN", GetType(String))
        GSHABANtbl.Columns.Add("OILTYPENAME", GetType(String))
        GSHABANtbl.Columns.Add("SHAFUKU", GetType(String))
        GSHABANtbl.Columns.Add("OWNCODENAME", GetType(String))
        GSHABANtbl.Columns.Add("SHARYOSTATUS", GetType(String))
        GSHABANtbl.Columns.Add("SHARYOSTATUSNAME", GetType(String))
        GSHABANtbl.Columns.Add("TSHABANF", GetType(String))
        GSHABANtbl.Columns.Add("TSHABANB", GetType(String))
        GSHABANtbl.Columns.Add("TSHABANB2", GetType(String))
        GSHABANtbl.Columns.Add("LICNPLTNOF", GetType(String))
        GSHABANtbl.Columns.Add("LICNPLTNOB", GetType(String))
        GSHABANtbl.Columns.Add("LICNPLTNOB2", GetType(String))
        GSHABANtbl.Columns.Add("SHARYOINFO1", GetType(String))
        GSHABANtbl.Columns.Add("SHARYOINFO2", GetType(String))
        GSHABANtbl.Columns.Add("SHARYOINFO3", GetType(String))
        GSHABANtbl.Columns.Add("SHARYOINFO4", GetType(String))
        GSHABANtbl.Columns.Add("SHARYOINFO5", GetType(String))
        GSHABANtbl.Columns.Add("SHARYOINFO6", GetType(String))
        GSHABANtbl.Columns.Add("HSTATUS1", GetType(String))
        GSHABANtbl.Columns.Add("HSTATUS2", GetType(String))
        GSHABANtbl.Columns.Add("HSTATUS3", GetType(String))
        GSHABANtbl.Columns.Add("HSTATUS4", GetType(String))
        GSHABANtbl.Columns.Add("HSTATUS5", GetType(String))
        GSHABANtbl.Columns.Add("HSTATUS6", GetType(String))
        GSHABANtbl.Columns.Add("HSTATUS7", GetType(String))
        GSHABANtbl.Columns.Add("HSTATUS8", GetType(String))

        '車両追加情報
        For i As Integer = 0 To WF_ListGSHABAN.Items.Count - 1
            GSHABANrow = GSHABANtbl.NewRow

            'テーブル項目セット
            GSHABANrow("GSHABAN") = WF_ListGSHABAN.Items(i).Value
            GSHABANrow("OILTYPENAME") = WF_ListOILTYPENAME.Items(i).Value
            GSHABANrow("SHAFUKU") = WF_ListSHAFUKU.Items(i).Value
            GSHABANrow("OWNCODENAME") = WF_ListOWNCODENAME.Items(i).Value
            GSHABANrow("SHARYOSTATUS") = WF_ListSHARYOSTATUS.Items(i).Value
            GSHABANrow("SHARYOSTATUSNAME") = WF_ListSHARYOSTATUSNAME.Items(i).Value
            GSHABANrow("LICNPLTNOF") = WF_ListLICNPLTNOF.Items(i).Value
            GSHABANrow("LICNPLTNOB") = WF_ListLICNPLTNOB.Items(i).Value
            GSHABANrow("LICNPLTNOB2") = WF_ListLICNPLTNOB2.Items(i).Value
            GSHABANrow("TSHABANF") = WF_ListTSHABANF.Items(i).Value
            GSHABANrow("TSHABANB") = WF_ListTSHABANB.Items(i).Value
            GSHABANrow("TSHABANB2") = WF_ListTSHABANB2.Items(i).Value
            GSHABANrow("SHARYOINFO1") = WF_ListSHARYOINFO1.Items(i).Value
            GSHABANrow("SHARYOINFO2") = WF_ListSHARYOINFO2.Items(i).Value
            GSHABANrow("SHARYOINFO3") = WF_ListSHARYOINFO3.Items(i).Value
            GSHABANrow("SHARYOINFO4") = WF_ListSHARYOINFO4.Items(i).Value
            GSHABANrow("SHARYOINFO5") = WF_ListSHARYOINFO5.Items(i).Value
            GSHABANrow("SHARYOINFO6") = WF_ListSHARYOINFO6.Items(i).Value
            GSHABANrow("HSTATUS1") = "○"
            GSHABANrow("HSTATUS2") = "○"
            GSHABANrow("HSTATUS3") = "○"
            GSHABANrow("HSTATUS4") = "○"
            GSHABANrow("HSTATUS5") = "○"
            GSHABANrow("HSTATUS6") = "○"
            GSHABANrow("HSTATUS7") = "○"
            GSHABANrow("HSTATUS8") = "○"

            'テーブル追加
            GSHABANtbl.Rows.Add(GSHABANrow)

        Next

        '○配送状況セット
        'ソート
        Dim WW_TBLVIEW As DataView = New DataView(T00016tbl)
        WW_TBLVIEW.Sort = "SHUKODATE , GSHABAN"

        For Each GSHABANrow In GSHABANtbl.Rows

            ''業務車番・出庫日が合致する場合
            'WW_TBLVIEW.RowFilter = "GSHABAN = '" & GSHABANrow("GSHABAN") & "' and " & "SHUKODATE = '" & WW_SHUKODATE.ToString("yyyy/MM/dd") & "'"

            For Each WW_TBLVIEWrow As DataRowView In WW_TBLVIEW
                Select Case WW_TBLVIEWrow("TRIPNO")
                    Case "001"
                        GSHABANrow("HSTATUS1") = "●"
                    Case "002"
                        GSHABANrow("HSTATUS2") = "●"
                    Case "003"
                        GSHABANrow("HSTATUS3") = "●"
                    Case "004"
                        GSHABANrow("HSTATUS4") = "●"
                    Case "005"
                        GSHABANrow("HSTATUS5") = "●"
                    Case "006"
                        GSHABANrow("HSTATUS6") = "●"
                    Case "007"
                        GSHABANrow("HSTATUS7") = "●"
                    Case "008"
                        GSHABANrow("HSTATUS8") = "●"
                    Case Else
                End Select
            Next

        Next

        '○データバインド
        WF_GSHABAN_Rep.DataSource = GSHABANtbl
        WF_GSHABAN_Rep.DataBind()

        '○イベント設定
        For i As Integer = 0 To WF_GSHABAN_Rep.Items.Count - 1
            Dim WW_SHARYOSTATUS As String = CType(WF_GSHABAN_Rep.Items(i).FindControl("WF_GSHABAN_ItemCell6"), System.Web.UI.WebControls.TableCell).Text
            '車両ステータスが運行可能ならイベント追加
            If String.IsNullOrEmpty(WW_SHARYOSTATUS) OrElse WW_SHARYOSTATUS = "1" Then
                CType(WF_GSHABAN_Rep.Items(i).FindControl("WF_GSHABAN_Items"), System.Web.UI.WebControls.TableRow).Attributes.Add("ondblclick", "Leftbox_Gyou('" & i & "');")
            End If
        Next

        'Close()
        WW_TBLVIEW.Dispose()
        WW_TBLVIEW = Nothing
        GSHABANtbl.Dispose()
        GSHABANtbl = Nothing

    End Sub

#End Region

#Region "UPLOADファイル"

    ''' <summary>
    ''' ファイルアップロード入力処理 
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub UPLOAD_EXCEL()

        WW_ERRLIST = New List(Of String)

        '○初期処理
        rightview.SetErrorReport("")

        '○画面表示データ復元
        Master.RecoverTable(T00016tbl)


        '■■■ UPLOAD_XLSデータ取得 ■■■
        If work.WF_SEL_CAMPCODE.Text = GRT00016WRKINC.C_CAMPCODE.NJS Then
            XLStoINPtblForNJS(WW_ERRCODE)
        Else
            XLStoINPtbl(WW_ERRCODE)
        End If
        If Not isNormal(WW_ERRCODE) Then
            Master.Output(WW_ERRCODE, C_MESSAGE_TYPE.ERR)
            Exit Sub
        End If

        '■■■ INPデータ登録 ■■■
        INPtbltoT16tbl(WW_ERRCODE)

        '■■■ GridView更新 ■■■
        ' 状態クリア
        EditOperationText(T00016tbl, False)

        ''○サマリ処理 
        'CS0026TBLSORTget.TABLE = T00016tbl
        'CS0026TBLSORTget.SORTING = "LINECNT ASC , SEQ ASC"
        'CS0026TBLSORTget.FILTER = ""
        'CS0026TBLSORTget.Sort(T00016tbl)
        'SUMMRY_SET()

        'エラーメッセージ内の項番、明細番号置き換え
        Dim WW_ERRWORD As String = rightview.GetErrorReport()
        For i As Integer = 0 To T00016INPtbl.Rows.Count - 1
            '項番
            WW_ERRWORD = WW_ERRWORD.Replace("@L" & i.ToString("0000") & "L@", T00016INPtbl.Rows(i)("LINECNT"))
            '明細番号
            WW_ERRWORD = WW_ERRWORD.Replace("@D" & i.ToString("000") & "D@", T00016INPtbl.Rows(i)("SEQ"))
        Next
        rightview.SetErrorReport(WW_ERRWORD)

        '○画面表示データ保存
        Master.SaveTable(T00016tbl)

        '■■■ Detailデータ設定 ■■■
        '画面切替設定
        WF_IsHideDetailBox.Value = "1"

        'leftBOXキャンセルボタン処理
        WF_ButtonCan_Click()

        '○メッセージ表示
        If isNormal(WW_ERRCODE) Then
            Master.Output(C_MESSAGE_NO.NORMAL, C_MESSAGE_TYPE.NOR)
        Else
            Master.Output(WW_ERRCODE, C_MESSAGE_TYPE.ERR)
        End If

        '○Detail初期設定
        T00016INPtbl.Clear()

        'カーソル設定
        WF_FIELD.Value = "WF_SELTORIDATE"
        WF_SELTORIDATE.Focus()

    End Sub

    ''' <summary>
    ''' Excel→T00016tbl処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub XLStoINPtbl(ByRef O_RTN As String)

        '■■■ UPLOAD_XLSデータ取得 ■■■
        CS0023XLSUPLOAD.CAMPCODE = work.WF_SEL_CAMPCODE.Text
        CS0023XLSUPLOAD.MAPID = GRT00016WRKINC.MAPID
        CS0023XLSUPLOAD.CS0023XLSUPLOAD(String.Empty, Master.PROF_REPORT)
        If isNormal(CS0023XLSUPLOAD.ERR) Then
            If CS0023XLSUPLOAD.TBLDATA.Rows.Count = 0 Then
                O_RTN = C_MESSAGE_NO.REGISTRATION_RECORD_NOT_EXIST_ERROR
                Master.Output(O_RTN, C_MESSAGE_TYPE.ERR)
                Exit Sub
            End If
        Else
            O_RTN = CS0023XLSUPLOAD.ERR
            Master.Output(O_RTN, C_MESSAGE_TYPE.ERR, "CS0023XLSUPLOAD")
            Exit Sub
        End If
        'EXCELデータの初期化（DBNullを撲滅）
        Dim CS0023XLSUPLOADrow As DataRow = CS0023XLSUPLOAD.TBLDATA.NewRow
        For i As Integer = 0 To CS0023XLSUPLOAD.TBLDATA.Rows.Count - 1
            CS0023XLSUPLOADrow.ItemArray = CS0023XLSUPLOAD.TBLDATA.Rows(i).ItemArray

            For j As Integer = 0 To CS0023XLSUPLOAD.TBLDATA.Columns.Count - 1
                If IsDBNull(CS0023XLSUPLOADrow.Item(j)) Or IsNothing(CS0023XLSUPLOADrow.Item(j)) Then
                    CS0023XLSUPLOADrow.Item(j) = ""
                End If
            Next
            CS0023XLSUPLOAD.TBLDATA.Rows(i).ItemArray = CS0023XLSUPLOADrow.ItemArray
        Next

        '○CS0023XLSUPLOAD.TBLDATAの入力値整備
        Dim WW_COLUMNS As New List(Of String)
        For Each column As DataColumn In CS0023XLSUPLOAD.TBLDATA.Columns
            WW_COLUMNS.Add(column.ColumnName)
        Next


        '■■■ エラーレポート準備 ■■■
        O_RTN = C_MESSAGE_NO.NORMAL

        '○T00016INPtblカラム設定
        Master.CreateEmptyTable(T00016INPtbl)

        '○必須項目の指定チェック
        If CS0023XLSUPLOAD.TBLDATA.Columns.Contains("TORICODE") AndAlso
            CS0023XLSUPLOAD.TBLDATA.Columns.Contains("SHUKADATE") AndAlso
            CS0023XLSUPLOAD.TBLDATA.Columns.Contains("SHUKODATE") AndAlso
            CS0023XLSUPLOAD.TBLDATA.Columns.Contains("TRIPNO") AndAlso
            CS0023XLSUPLOAD.TBLDATA.Columns.Contains("DROPNO") AndAlso
            CS0023XLSUPLOAD.TBLDATA.Columns.Contains("SHUKABASHO") AndAlso
            CS0023XLSUPLOAD.TBLDATA.Columns.Contains("GSHABAN") AndAlso
            CS0023XLSUPLOAD.TBLDATA.Columns.Contains("STAFFCODE") AndAlso
            (CS0023XLSUPLOAD.TBLDATA.Columns.Contains("PRODUCTCODE") OrElse
             CS0023XLSUPLOAD.TBLDATA.Columns.Contains("OILTYPE") AndAlso CS0023XLSUPLOAD.TBLDATA.Columns.Contains("PRODUCT2")) Then
        Else
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            Master.Output(O_RTN, C_MESSAGE_TYPE.ERR)
            rightview.AddErrorReport("・アップロードExcelに『出荷日、出庫日、荷主、(品名コード or 油種・品名２)、出荷場所、車番、乗務員、トリップ、ドロップ』が存在しません。")
            Exit Sub
        End If

        '○ソート処理
        CS0026TBLSORTget.TABLE = CS0023XLSUPLOAD.TBLDATA
        If CS0023XLSUPLOAD.TBLDATA.Columns.Contains("PRODUCTCODE") Then
            CS0026TBLSORTget.SORTING = "TORICODE, SHUKADATE, SHUKODATE, PRODUCTCODE, TRIPNO, DROPNO, SHUKABASHO, GSHABAN, STAFFCODE, PRODUCTCODE"
        ElseIf CS0023XLSUPLOAD.TBLDATA.Columns.Contains("PRODUCT2") Then
            CS0026TBLSORTget.SORTING = "TORICODE, SHUKADATE, SHUKODATE, OILTYPE, TRIPNO, DROPNO, SHUKABASHO, GSHABAN, STAFFCODE, PRODUCT2"
        End If
        CS0026TBLSORTget.FILTER = ""
        CS0026TBLSORTget.sort(CS0023XLSUPLOAD.TBLDATA)

        '■■■ Excelデータ毎にチェック＆更新 ■■■
        Dim WW_INDEX As Integer = 0
        For Each uploadRow In CS0023XLSUPLOAD.TBLDATA.Rows

            '○XLSTBL明細⇒T00016INProw
            Dim T00016INProw = T00016INPtbl.NewRow

            T00016INProw("LINECNT") = 0
            T00016INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA
            T00016INProw("TIMSTP") = "0"
            T00016INProw("SELECT") = 1
            T00016INProw("HIDDEN") = 0

            T00016INProw("INDEX") = WW_INDEX
            WW_INDEX += WW_INDEX

            T00016INProw("CAMPCODE") = work.WF_SEL_CAMPCODE.Text


            If WW_COLUMNS.IndexOf("ORDERNO") < 0 Then
                T00016INProw("ORDERNO") = ""
            Else
                T00016INProw("ORDERNO") = uploadRow("ORDERNO")
            End If

            If WW_COLUMNS.IndexOf("DETAILNO") < 0 Then
                T00016INProw("DETAILNO") = ""
            Else
                T00016INProw("DETAILNO") = uploadRow("DETAILNO")
            End If

            If WW_COLUMNS.IndexOf("OILTYPE") < 0 Then
                T00016INProw("OILTYPE") = ""
            Else
                T00016INProw("OILTYPE") = uploadRow("OILTYPE")
            End If

            If WW_COLUMNS.IndexOf("TRIPNO") < 0 Then
                T00016INProw("TRIPNO") = ""
            Else
                T00016INProw("TRIPNO") = uploadRow("TRIPNO")
            End If

            If WW_COLUMNS.IndexOf("DROPNO") < 0 Then
                T00016INProw("DROPNO") = ""
            Else
                T00016INProw("DROPNO") = uploadRow("DROPNO")
            End If

            'If WW_COLUMNS.IndexOf("SEQ") < 0 Then
            '    T00016INProw("SEQ") = ""
            'Else
            '    T00016INProw("SEQ") = uploadRow("SEQ")
            'End If

            If WW_COLUMNS.IndexOf("TORICODE") < 0 Then
                T00016INProw("TORICODE") = ""
            Else
                T00016INProw("TORICODE") = uploadRow("TORICODE")
            End If

            If WW_COLUMNS.IndexOf("SHUKODATE") < 0 Then
                T00016INProw("SHUKODATE") = ""
            Else
                T00016INProw("SHUKODATE") = uploadRow("SHUKODATE")
            End If

            If WW_COLUMNS.IndexOf("KIKODATE") < 0 Then
                T00016INProw("KIKODATE") = ""
            Else
                T00016INProw("KIKODATE") = uploadRow("KIKODATE")
            End If

            If WW_COLUMNS.IndexOf("KIJUNDATE") < 0 Then
                T00016INProw("KIJUNDATE") = ""
            Else
                T00016INProw("KIJUNDATE") = uploadRow("KIJUNDATE")
            End If

            If WW_COLUMNS.IndexOf("SHUKADATE") < 0 Then
                T00016INProw("SHUKADATE") = ""
            Else
                T00016INProw("SHUKADATE") = uploadRow("SHUKADATE")
            End If

            If WW_COLUMNS.IndexOf("SHIPORG") < 0 Then
                T00016INProw("SHIPORG") = WF_DEFORG.Text
            Else
                T00016INProw("SHIPORG") = uploadRow("SHIPORG").ToString.PadLeft(WF_DEFORG.Text.Length, "0")
            End If

            If WW_COLUMNS.IndexOf("SHUKABASHO") < 0 Then
                T00016INProw("SHUKABASHO") = ""
            Else
                T00016INProw("SHUKABASHO") = uploadRow("SHUKABASHO")
            End If

            If WW_COLUMNS.IndexOf("GSHABAN") < 0 Then
                T00016INProw("GSHABAN") = ""
            Else
                T00016INProw("GSHABAN") = uploadRow("GSHABAN")
            End If

            If WW_COLUMNS.IndexOf("RYOME") < 0 Then
                T00016INProw("RYOME") = "1"
            Else
                If uploadRow("RYOME") = Nothing Then
                    T00016INProw("RYOME") = "1"
                Else
                    T00016INProw("RYOME") = uploadRow("RYOME")
                End If
            End If

            If WW_COLUMNS.IndexOf("SHAFUKU") < 0 Then
                T00016INProw("SHAFUKU") = ""
            Else
                T00016INProw("SHAFUKU") = uploadRow("SHAFUKU")
            End If

            If WW_COLUMNS.IndexOf("STAFFCODE") < 0 Then
                T00016INProw("STAFFCODE") = ""
            Else
                T00016INProw("STAFFCODE") = uploadRow("STAFFCODE")
            End If

            If WW_COLUMNS.IndexOf("SUBSTAFFCODE") < 0 Then
                T00016INProw("SUBSTAFFCODE") = ""
            Else
                T00016INProw("SUBSTAFFCODE") = uploadRow("SUBSTAFFCODE")
            End If

            If WW_COLUMNS.IndexOf("TODOKEDATE") < 0 Then
                T00016INProw("TODOKEDATE") = ""
            Else
                T00016INProw("TODOKEDATE") = uploadRow("TODOKEDATE")
            End If

            If WW_COLUMNS.IndexOf("TODOKECODE") < 0 Then
                T00016INProw("TODOKECODE") = ""
            Else
                T00016INProw("TODOKECODE") = uploadRow("TODOKECODE")
            End If

            If WW_COLUMNS.IndexOf("PRODUCT1") < 0 Then
                T00016INProw("PRODUCT1") = ""
            Else
                T00016INProw("PRODUCT1") = uploadRow("PRODUCT1")
            End If

            If WW_COLUMNS.IndexOf("PRODUCT2") < 0 Then
                T00016INProw("PRODUCT2") = ""
            Else
                T00016INProw("PRODUCT2") = uploadRow("PRODUCT2")
            End If

            If WW_COLUMNS.IndexOf("PRODUCTCODE") < 0 Then
                T00016INProw("PRODUCTCODE") = ""
            Else
                T00016INProw("PRODUCTCODE") = uploadRow("PRODUCTCODE")
            End If

            If WW_COLUMNS.IndexOf("CONTNO") < 0 Then
                T00016INProw("CONTNO") = ""
            Else
                T00016INProw("CONTNO") = uploadRow("CONTNO")
            End If

            If WW_COLUMNS.IndexOf("JSURYO") < 0 Then
                T00016INProw("JSURYO") = ""
            Else
                T00016INProw("JSURYO") = uploadRow("JSURYO")
            End If

            'If WW_COLUMNS.IndexOf("JDAISU") < 0 Then
            '    T00016INProw("JDAISU") = ""
            'Else
            '    T00016INProw("JDAISU") = uploadRow("JDAISU")
            'End If

            If WW_COLUMNS.IndexOf("REMARKS1") < 0 Then
                T00016INProw("REMARKS1") = ""
            Else
                T00016INProw("REMARKS1") = uploadRow("REMARKS1")
            End If

            If WW_COLUMNS.IndexOf("REMARKS2") < 0 Then
                T00016INProw("REMARKS2") = ""
            Else
                T00016INProw("REMARKS2") = uploadRow("REMARKS2")
            End If

            If WW_COLUMNS.IndexOf("REMARKS3") < 0 Then
                T00016INProw("REMARKS3") = ""
            Else
                T00016INProw("REMARKS3") = uploadRow("REMARKS3")
            End If

            If WW_COLUMNS.IndexOf("REMARKS4") < 0 Then
                T00016INProw("REMARKS4") = ""
            Else
                T00016INProw("REMARKS4") = uploadRow("REMARKS4")
            End If

            If WW_COLUMNS.IndexOf("REMARKS5") < 0 Then
                T00016INProw("REMARKS5") = ""
            Else
                T00016INProw("REMARKS5") = uploadRow("REMARKS5")
            End If

            If WW_COLUMNS.IndexOf("REMARKS6") < 0 Then
                T00016INProw("REMARKS6") = ""
            Else
                T00016INProw("REMARKS6") = uploadRow("REMARKS6")
            End If

            If WW_COLUMNS.IndexOf("DELFLG") < 0 Then
                T00016INProw("DELFLG") = "0"
            Else
                T00016INProw("DELFLG") = uploadRow("DELFLG")
            End If

            If WW_COLUMNS.IndexOf("SHARYOTYPEF") < 0 Then
                T00016INProw("SHARYOTYPEF") = ""
            Else
                T00016INProw("SHARYOTYPEF") = uploadRow("SHARYOTYPEF")
            End If

            If WW_COLUMNS.IndexOf("SHARYOTYPEB") < 0 Then
                T00016INProw("SHARYOTYPEB") = ""
            Else
                T00016INProw("SHARYOTYPEB") = uploadRow("SHARYOTYPEB")
            End If

            If WW_COLUMNS.IndexOf("SHARYOTYPEB2") < 0 Then
                T00016INProw("SHARYOTYPEB2") = ""
            Else
                T00016INProw("SHARYOTYPEB2") = uploadRow("SHARYOTYPEB2")
            End If

            If WW_COLUMNS.IndexOf("JISSEKIKBN") < 0 Then
                T00016INProw("JISSEKIKBN") = ""
            Else
                T00016INProw("JISSEKIKBN") = uploadRow("JISSEKIKBN")
            End If

            'Grid追加明細（新規追加と同じ）とする
            T00016INProw("WORK_NO") = ""

            '■■■ 数量ゼロは読み飛ばし ■■■
            If Val(T00016INProw("JSURYO")) = 0 Then
                Continue For
            End If

            '品名コード未存在時は油種・品名1・品名2から作成
            If WW_COLUMNS.IndexOf("PRODUCTCODE") < 0 Then
                If Not String.IsNullOrEmpty(T00016INProw("OILTYPE")) AndAlso
                    Not String.IsNullOrEmpty(T00016INProw("PRODUCT1")) AndAlso
                    Not String.IsNullOrEmpty(T00016INProw("PRODUCT2")) Then
                    T00016INProw("PRODUCTCODE") = T00016INProw("CAMPCODE").ToString.PadLeft(2, "0") & T00016INProw("OILTYPE").ToString.PadLeft(2, "0") & T00016INProw("PRODUCT1").ToString.PadLeft(2, "0") & T00016INProw("PRODUCT2").ToString.PadLeft(5, "0")
                End If
            ElseIf Not String.IsNullOrEmpty(T00016INProw("PRODUCTCODE")) AndAlso T00016INProw("PRODUCTCODE").ToString.Length = 11 Then
                '油種未存在は品名コードから作成
                If WW_COLUMNS.IndexOf("OILTYPE") < 0 Then
                    T00016INProw("OILTYPE") = Mid(T00016INProw("PRODUCTCODE").ToString, 3, 2)
                End If
                '品名１未存在は品名コードから作成
                If WW_COLUMNS.IndexOf("PRODUCT1") < 0 Then
                    T00016INProw("PRODUCT1") = Mid(T00016INProw("PRODUCTCODE").ToString, 5, 2)
                End If
                '品名２未存在は品名コードから作成
                If WW_COLUMNS.IndexOf("PRODUCT2") < 0 Then
                    T00016INProw("PRODUCT2") = Mid(T00016INProw("PRODUCTCODE").ToString, 7, 5)
                End If

            End If


            '○名称付与
            CODENAME_set(T00016INProw)

            '入力テーブル追加
            T00016INPtbl.Rows.Add(T00016INProw)

        Next

    End Sub

    ''' <summary>
    ''' NJS Excel→T00016tbl処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub XLStoINPtblForNJS(ByRef O_RTN As String)

        ''■■■ UPLOAD_XLSデータ取得 ■■■
        'CS0023XLSUPLOAD.CAMPCODE = work.WF_SEL_CAMPCODE.Text
        'CS0023XLSUPLOAD.MAPID = GRT00016WRKINC.MAPID
        'CS0023XLSUPLOAD.CS0023XLSUPLOAD(C_UPLOAD_EXCEL_REPORTID_NJS, C_UPLOAD_EXCEL_PROFID_NJS)
        'If isNormal(CS0023XLSUPLOAD.ERR) Then
        '    If CS0023XLSUPLOAD.TBLDATA.Rows.Count = 0 Then
        '        O_RTN = C_MESSAGE_NO.REGISTRATION_RECORD_NOT_EXIST_ERROR
        '        Master.Output(O_RTN, C_MESSAGE_TYPE.ERR)
        '        Exit Sub
        '    End If
        'Else
        '    O_RTN = CS0023XLSUPLOAD.ERR
        '    Master.Output(O_RTN, C_MESSAGE_TYPE.ERR, "CS0023XLSUPLOAD")
        '    Exit Sub
        'End If
        ''EXCELデータの初期化（DBNullを撲滅）
        'Dim CS0023XLSUPLOADrow As DataRow = CS0023XLSUPLOAD.TBLDATA.NewRow
        'For i As Integer = 0 To CS0023XLSUPLOAD.TBLDATA.Rows.Count - 1
        '    CS0023XLSUPLOADrow.ItemArray = CS0023XLSUPLOAD.TBLDATA.Rows(i).ItemArray

        '    For j As Integer = 0 To CS0023XLSUPLOAD.TBLDATA.Columns.Count - 1
        '        If IsDBNull(CS0023XLSUPLOADrow.Item(j)) Or IsNothing(CS0023XLSUPLOADrow.Item(j)) Then
        '            CS0023XLSUPLOADrow.Item(j) = ""
        '        End If
        '    Next
        '    CS0023XLSUPLOAD.TBLDATA.Rows(i).ItemArray = CS0023XLSUPLOADrow.ItemArray
        'Next

        ''○CS0023XLSUPLOAD.TBLDATAの入力値整備
        'Dim WW_COLUMNS As New List(Of String)
        'For Each column As DataColumn In CS0023XLSUPLOAD.TBLDATA.Columns
        '    WW_COLUMNS.Add(column.ColumnName)
        'Next


        ''■■■ エラーレポート準備 ■■■
        'O_RTN = C_MESSAGE_NO.NORMAL

        ''○T00016INPtblカラム設定
        'Master.CreateEmptyTable(T00016INPtbl)

        ''○必須項目の指定チェック
        'If CS0023XLSUPLOAD.TBLDATA.Columns.Contains("SHUKODATE") AndAlso
        '    CS0023XLSUPLOAD.TBLDATA.Columns.Contains("TODOKEDATE") AndAlso
        '    CS0023XLSUPLOAD.TBLDATA.Columns.Contains("TODOKECODE") AndAlso
        '    CS0023XLSUPLOAD.TBLDATA.Columns.Contains("PRODUCTCODE") AndAlso
        '    CS0023XLSUPLOAD.TBLDATA.Columns.Contains("JSURYO") AndAlso
        '    CS0023XLSUPLOAD.TBLDATA.Columns.Contains("SHARYOCD") AndAlso
        '    CS0023XLSUPLOAD.TBLDATA.Columns.Contains("STAFFCODE1") AndAlso
        '    CS0023XLSUPLOAD.TBLDATA.Columns.Contains("STAFFCODE2") Then
        'Else
        '    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        '    rightview.AddErrorReport("・アップロードExcelに『出庫日、納入日、届先コード、品名コード、数量、車輛コード、運転手コード1、運転手コード2』が存在しません。")
        '    Exit Sub
        'End If

        ''○JSRコードマスタ作成
        'Using jsrCvt As JSRCODE_MASTER = New JSRCODE_MASTER()
        '    jsrCvt.CAMPCODE = work.WF_SEL_CAMPCODE.Text
        '    If String.IsNullOrEmpty(work.WF_SEL_SHIPORG.Text) Then
        '        jsrCvt.ORGCODE = WF_DEFORG.Text
        '    Else
        '        jsrCvt.ORGCODE = work.WF_SEL_SHIPORG.Text
        '    End If
        '    'JSRコード一括読込
        '    If jsrCvt.ReadJSRData() = False Then
        '        O_RTN = jsrCvt.ERR
        '        Master.Output(O_RTN, C_MESSAGE_TYPE.ABORT, "read JSRCODE")
        '        Exit Sub
        '    End If

        '    '■■■ Excelデータ毎にチェック＆更新 ■■■
        '    Dim WW_INDEX As Integer = 0
        '    '配送時刻順でトリップ作成の為にソート
        '    Dim uploadData = CS0023XLSUPLOAD.TBLDATA.
        '        AsEnumerable.
        '        OrderBy(Function(x) x.Item("SHARYOCD")).
        '        ThenBy(Function(x) x.Item("TODOKEDATE")).
        '        ThenBy(Function(x) x.Item("WORKTIME3"))
        '    For Each uploadRow As DataRow In uploadData

        '        Dim datTodoke = New JSRCODE_MASTER.JSRCODE_TODOKE
        '        Dim datProduct = New JSRCODE_MASTER.JSRCODE_PRODUCT
        '        Dim datStaff = New JSRCODE_MASTER.JSRCODE_STAFF
        '        Dim datSubStaff = New JSRCODE_MASTER.JSRCODE_STAFF
        '        Dim WW_SHUKODATE As Date
        '        Dim WW_SHUKADATE As Date
        '        Dim WW_TODOKEDATE As Date
        '        Dim WW_KIKODATE As Date
        '        Dim WW_RELATIVEDAYS3 As Integer
        '        Dim WW_RELATIVEDAYS4 As Integer

        '        '2:出庫日
        '        If Not DateTime.TryParseExact(uploadRow("SHUKODATE"), "yyyyMMdd", Nothing, Nothing, WW_SHUKODATE) Then
        '            O_RTN = C_MESSAGE_NO.DATE_FORMAT_ERROR
        '            rightview.AddErrorReport("・アップロードExcel『出庫日』の日付書式が正しくありません。")
        '            Exit Sub
        '        End If
        '        '3:納入日
        '        If Not DateTime.TryParseExact(uploadRow("TODOKEDATE"), "yyyyMMdd", Nothing, Nothing, WW_TODOKEDATE) Then
        '            O_RTN = C_MESSAGE_NO.DATE_FORMAT_ERROR
        '            rightview.AddErrorReport("・アップロードExcel『納入日』の日付書式が正しくありません。")
        '            Exit Sub
        '        End If
        '        '20:相対日数１（出荷（積））
        '        '21:相対日数２（出発）
        '        '22:相対日数３（納入）
        '        '23:相対日数４（帰庫）
        '        '24:相対日数５（点検）
        '        If WW_COLUMNS.Contains("RELATIVEDAYS3") Then
        '            If Not Int32.TryParse(uploadRow("RELATIVEDAYS3"), WW_RELATIVEDAYS3) Then
        '                WW_RELATIVEDAYS3 = 0
        '            End If
        '        End If
        '        If WW_COLUMNS.Contains("RELATIVEDAYS4") Then
        '            If Not Int32.TryParse(uploadRow("RELATIVEDAYS4"), WW_RELATIVEDAYS4) Then
        '                WW_RELATIVEDAYS4 = 0
        '            End If
        '        End If

        '        '出庫日    ＝ 出荷日
        '        '出荷日    ＝ 出庫日
        '        '届日      ＝ 納入日
        '        '出荷日    ＝ 出荷日
        '        WW_SHUKADATE = WW_SHUKODATE
        '        WW_SHUKODATE = WW_SHUKODATE.AddDays(WW_RELATIVEDAYS3)
        '        WW_TODOKEDATE = WW_TODOKEDATE
        '        WW_KIKODATE = WW_TODOKEDATE.AddDays(WW_RELATIVEDAYS4)

        '        '***** 取込除外条件 *****
        '        ' ①運転手コード１
        '        '  -a NULL
        '        '  -b 0000
        '        '  -c 0001
        '        If uploadRow("STAFFCODE1") = String.Empty OrElse
        '            uploadRow("STAFFCODE1") = "0000" OrElse
        '            uploadRow("STAFFCODE1") = "0001" Then
        '            Continue For
        '        End If
        '        ' ②車輛コード
        '        '  -a NULL
        '        '  -b 9XX
        '        If uploadRow("SHARYOCD") = String.Empty OrElse
        '            uploadRow("SHARYOCD").ToString.StartsWith("9") Then
        '            Continue For
        '        End If
        '        ' ③届日（納入日）
        '        '  -a 当日以前
        '        If WW_TODOKEDATE <= CS0050SESSION.LOGONDATE Then
        '            Continue For
        '        End If

        '        If WW_COLUMNS.Contains("TODOKECODE") Then
        '            datTodoke = jsrCvt.GetTodokeCode(uploadRow("TODOKECODE"))
        '            If IsNothing(datTodoke) Then
        '                Dim WW_CheckMES1 = "・変換エラーが存在します。(届先コード)"
        '                Dim WW_CheckMES2 = uploadRow("TODOKECODE")
        '                ERRMESSAGE_write_NJS(WW_CheckMES1, WW_CheckMES2, WW_DUMMY, WW_INDEX + 1, C_MESSAGE_NO.BOX_ERROR_EXIST, uploadRow)
        '            End If
        '            'グループ作業用届先は除外
        '            If datTodoke.IsGroupWork Then
        '                Continue For
        '            End If
        '        End If
        '        If WW_COLUMNS.Contains("PRODUCTCODE") Then
        '            If jsrCvt.CovertProductCode(uploadRow("PRODUCTCODE"), datProduct) = False Then
        '                Dim WW_CheckMES1 = "・変換エラーが存在します。(品名コード)"
        '                Dim WW_CheckMES2 = uploadRow("PRODUCTCODE")
        '                ERRMESSAGE_write_NJS(WW_CheckMES1, WW_CheckMES2, WW_DUMMY, WW_INDEX + 1, C_MESSAGE_NO.BOX_ERROR_EXIST, uploadRow)
        '            End If
        '        End If
        '        If WW_COLUMNS.Contains("STAFFCODE1") AndAlso
        '            Not String.IsNullOrEmpty(uploadRow("STAFFCODE1")) Then
        '            If jsrCvt.CovertStaffCode(uploadRow("STAFFCODE1"), datStaff) = False Then
        '                Dim WW_CheckMES1 = "・変換エラーが存在します。(運転手コード1)"
        '                Dim WW_CheckMES2 = uploadRow("STAFFCODE1")
        '                ERRMESSAGE_write_NJS(WW_CheckMES1, WW_CheckMES2, WW_DUMMY, WW_INDEX + 1, C_MESSAGE_NO.BOX_ERROR_EXIST, uploadRow)
        '            End If
        '        End If
        '        If WW_COLUMNS.Contains("STAFFCODE2") AndAlso
        '            Not String.IsNullOrEmpty(uploadRow("STAFFCODE2")) Then
        '            If jsrCvt.CovertStaffCode(uploadRow("STAFFCODE2"), datSubStaff) = False Then
        '                Dim WW_CheckMES1 = "・変換エラーが存在します。(運転手コード2)"
        '                Dim WW_CheckMES2 = uploadRow("STAFFCODE2")
        '                ERRMESSAGE_write_NJS(WW_CheckMES1, WW_CheckMES2, WW_DUMMY, WW_INDEX + 1, C_MESSAGE_NO.BOX_ERROR_EXIST, uploadRow)
        '            End If
        '        End If


        '        '○XLSTBL明細⇒T00016INProw
        '        Dim T00016INProw = T00016INPtbl.NewRow
        '        '***** T16項目順に編集 *****
        '        T00016INProw("LINECNT") = 0
        '        T00016INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA
        '        T00016INProw("TIMSTP") = "0"
        '        T00016INProw("SELECT") = 1
        '        T00016INProw("HIDDEN") = 0
        '        T00016INProw("INDEX") = WW_INDEX
        '        WW_INDEX += WW_INDEX

        '        T00016INProw("CAMPCODE") = work.WF_SEL_CAMPCODE.Text
        '        If String.IsNullOrEmpty(datTodoke.TORICODE) Then
        '            T00016INProw("TORICODE") = ""
        '        Else
        '            T00016INProw("TORICODE") = datTodoke.TORICODE
        '        End If
        '        If String.IsNullOrEmpty(datProduct.OILTYPE) Then
        '            T00016INProw("OILTYPE") = ""
        '        Else
        '            T00016INProw("OILTYPE") = datProduct.OILTYPE
        '        End If
        '        If String.IsNullOrEmpty(work.WF_SEL_SHIPORG.Text) Then
        '            T00016INProw("SHIPORG") = WF_DEFORG.Text
        '        Else
        '            T00016INProw("SHIPORG") = work.WF_SEL_SHIPORG.Text
        '        End If

        '        T00016INProw("KIJUNDATE") = ""                                      ' T3CTLから設定
        '        T00016INProw("ORDERNO") = ""                                        ' 後続で受注番号自動設定
        '        T00016INProw("DETAILNO") = "001"
        '        '車番下３桁のみ使用
        '        T00016INProw("GSHABAN") = uploadRow("SHARYOCD").ToString.PadLeft(20, "0").Substring(20 - 3)
        '        'トリップは暫定（同一出庫日存在時は編集）
        '        T00016INProw("TRIPNO") = "001"
        '        T00016INProw("DROPNO") = "001"
        '        T00016INProw("SEQ") = "01"
        '        If IsNothing(WW_SHUKODATE) Then
        '            T00016INProw("SHUKODATE") = ""
        '        Else
        '            T00016INProw("SHUKODATE") = WW_SHUKODATE.ToString("yyyy/MM/dd")
        '        End If
        '        If IsNothing(WW_KIKODATE) Then
        '            T00016INProw("KIKODATE") = ""
        '        Else
        '            T00016INProw("KIKODATE") = WW_KIKODATE.ToString("yyyy/MM/dd")
        '        End If
        '        '出庫日→出荷日
        '        If IsNothing(WW_SHUKADATE) Then
        '            T00016INProw("SHUKADATE") = ""
        '        Else
        '            T00016INProw("SHUKADATE") = WW_SHUKADATE.ToString("yyyy/MM/dd")
        '        End If
        '        If IsNothing(WW_TODOKEDATE) Then
        '            T00016INProw("TODOKEDATE") = ""
        '        Else
        '            T00016INProw("TODOKEDATE") = WW_TODOKEDATE.ToString("yyyy/MM/dd")
        '        End If
        '        If String.IsNullOrEmpty(datTodoke.SHUKABASHO) Then
        '            T00016INProw("SHUKABASHO") = ""
        '        Else
        '            T00016INProw("SHUKABASHO") = datTodoke.SHUKABASHO
        '        End If
        '        If String.IsNullOrEmpty(datStaff.STAFFCODE) Then
        '            T00016INProw("STAFFCODE") = uploadRow("STAFFCODE1")
        '        Else
        '            T00016INProw("STAFFCODE") = datStaff.STAFFCODE
        '        End If
        '        If String.IsNullOrEmpty(datSubStaff.STAFFCODE) Then
        '            T00016INProw("SUBSTAFFCODE") = uploadRow("STAFFCODE2")
        '        Else
        '            T00016INProw("SUBSTAFFCODE") = datSubStaff.STAFFCODE
        '        End If
        '        T00016INProw("RYOME") = "1"
        '        If String.IsNullOrEmpty(datTodoke.TODOKECODE) Then
        '            'T00016INProw("TODOKECODE") = uploadRow("TODOKECODE")
        '            T00016INProw("TODOKECODE") = "!" & datTodoke.JSRTODOKECODE & "!"
        '        Else
        '            T00016INProw("TODOKECODE") = datTodoke.TODOKECODE
        '        End If
        '        If String.IsNullOrEmpty(datProduct.PRODUCT1) Then
        '            T00016INProw("PRODUCT1") = ""
        '        Else
        '            T00016INProw("PRODUCT1") = datProduct.PRODUCT1
        '        End If
        '        If String.IsNullOrEmpty(datProduct.PRODUCT2) Then
        '            T00016INProw("PRODUCT2") = ""
        '        Else
        '            T00016INProw("PRODUCT2") = datProduct.PRODUCT2
        '        End If
        '        If String.IsNullOrEmpty(datProduct.PRODUCTCODE) Then
        '            'T00016INProw("PRODUCTCODE") = uploadRow("PRODUCTCODE")
        '            T00016INProw("PRODUCTCODE") = "!" & datProduct.JSRPRODUCT & "!"
        '        Else
        '            T00016INProw("PRODUCTCODE") = datProduct.PRODUCTCODE
        '        End If
        '        T00016INProw("CONTNO") = ""
        '        T00016INProw("SHAFUKU") = ""
        '        If WW_COLUMNS.Contains("JSURYO") Then
        '            '数量単位 NJS(L)→JOT(kL)
        '            T00016INProw("JSURYO") = uploadRow("JSURYO") / 1000
        '        Else
        '            T00016INProw("JSURYO") = "0"
        '        End If
        '        T00016INProw("JDAISU") = "1"
        '        'T00016INProw("JSURYO") = "0"
        '        'T00016INProw("JDAISU") = "0"
        '        '契約番号
        '        If WW_COLUMNS.Contains("CONTRACTNO") Then
        '            T00016INProw("REMARKS1") = uploadRow("CONTRACTNO")
        '        Else
        '            T00016INProw("REMARKS1") = ""
        '        End If
        '        '社内備考
        '        If WW_COLUMNS.Contains("SYANAINOTES") Then
        '            T00016INProw("REMARKS2") = uploadRow("SYANAINOTES")
        '        Else
        '            T00016INProw("REMARKS2") = ""
        '        End If
        '        '社外備考
        '        If WW_COLUMNS.Contains("SYAGAINOTES") Then
        '            T00016INProw("REMARKS3") = uploadRow("SYAGAINOTES")
        '        Else
        '            T00016INProw("REMARKS3") = ""
        '        End If
        '        T00016INProw("REMARKS4") = ""
        '        T00016INProw("REMARKS5") = ""
        '        T00016INProw("REMARKS6") = ""
        '        ' T3CTLから設定
        '        T00016INProw("SHARYOTYPEF") = ""
        '        T00016INProw("TSHABANF") = ""
        '        T00016INProw("SHARYOTYPEB") = ""
        '        T00016INProw("TSHABANB") = ""
        '        T00016INProw("SHARYOTYPEB2") = ""
        '        T00016INProw("TSHABANB2") = ""
        '        T00016INProw("JISSEKIKBN") = "1"
        '        T00016INProw("DELFLG") = "0"

        '        'Grid追加明細（新規追加と同じ）とする
        '        T00016INProw("WORK_NO") = ""

        '        '○名称付与
        '        CODENAME_set(T00016INProw)

        '        '同一出庫日最新トリップ№取得
        '        Dim latestTrip As Integer = GetLatestTripNo(T00016INProw)

        '        Dim tripCnt As Integer = latestTrip + 1
        '        T00016INProw("TRIPNO") = tripCnt.ToString("000")

        '        '入力テーブル追加
        '        T00016INPtbl.Rows.Add(T00016INProw)

        '        '****************************
        '        'トリップ増幅
        '        '  ※個数が２以上にレコード編集後に複製
        '        '****************************
        '        '個数＝トリップ
        '        Dim num As Integer
        '        If WW_COLUMNS.Contains("NUM") Then
        '            num = uploadRow("NUM")
        '        Else
        '            num = 1
        '        End If
        '        For i As Integer = 2 To num
        '            Dim T00016INPAddrow = T00016INPtbl.NewRow()
        '            T00016INPAddrow.ItemArray = T00016INProw.ItemArray
        '            tripCnt += 1
        '            T00016INPAddrow("TRIPNO") = tripCnt.ToString("000")
        '            T00016INPtbl.Rows.Add(T00016INPAddrow)
        '        Next

        '        '****************************
        '        '日跨ぎデータ増幅 WW_KIKODATE
        '        '　※出庫～届日までの日数分（修正前）
        '        '　※出庫～帰庫までの日数分（修正後　2020/10/28）
        '        '****************************
        '        If Not IsNothing(WW_SHUKODATE) AndAlso Not IsNothing(WW_KIKODATE) Then
        '            Dim days As Integer = (WW_KIKODATE - WW_SHUKODATE).Days
        '            For i As Integer = 1 To days
        '                Dim T00016INPAddrow = T00016INPtbl.NewRow()
        '                T00016INPAddrow.ItemArray = T00016INProw.ItemArray
        '                '
        '                T00016INPAddrow("SHUKODATE") = WW_SHUKODATE.AddDays(i).ToString("yyyy/MM/dd")
        '                '増幅時のトリップは001にリセット
        '                T00016INPAddrow("TRIPNO") = "001"
        '                T00016INPtbl.Rows.Add(T00016INPAddrow)
        '            Next
        '        End If

        '    Next

        'End Using

    End Sub
    ''' <summary>
    ''' NJS同一出庫日最大トリップ№取得
    ''' </summary>
    ''' <remarks></remarks>
    Protected Function GetLatestTripNo(ByVal row As DataRow) As Integer
        Dim tripNo As Integer = 0

        '入力・一覧データテーブル両方から対象車番の同一出庫日レコードを検索
        Dim t16inp = T00016INPtbl.AsEnumerable.
                    Where(Function(x) x.Item("SHIPORG") = work.WF_SEL_SHIPORG.Text AndAlso
                                      x.Item("DELFLG") = C_DELETE_FLG.ALIVE AndAlso
                                      x.Item("SHUKODATE") = row("SHUKODATE") AndAlso
                                      x.Item("GSHABAN") = row("GSHABAN") AndAlso
                                      x.Item("TODOKECODE") <> row("TODOKECODE"))
        Dim t16tbl = T00016tbl.AsEnumerable.
                    Where(Function(x) x.Item("SHIPORG") = work.WF_SEL_SHIPORG.Text AndAlso
                                      x.Item("DELFLG") = C_DELETE_FLG.ALIVE AndAlso
                                      x.Item("SHUKODATE") = row("SHUKODATE") AndAlso
                                      x.Item("GSHABAN") = row("GSHABAN") AndAlso
                                      x.Item("TODOKECODE") <> row("TODOKECODE"))

        Dim t16 = t16tbl.Union(t16inp)
        If t16.Count > 0 Then
            '届先時刻順で最終のTRIPNOを取得
            tripNo = Val(t16.Last.Item("TRIPNO"))
        End If

        Return tripNo

    End Function

#End Region

    ''' <summary>
    ''' Detail タブ切替処理
    ''' </summary>
    Protected Sub WF_Detail_TABChange()

        Dim WW_DTABChange As Integer
        Try
            Integer.TryParse(WF_DTAB_CHANGE_NO.Value, WW_DTABChange)
        Catch ex As Exception
            WW_DTABChange = 0
        End Try

        WF_DetailMView.ActiveViewIndex = WW_DTABChange

        '初期値（書式）変更

        '合計(社内)
        WF_Dtab01.CssClass = ""
        '合計(請求)
        WF_Dtab02.CssClass = ""
        '明細(金額)
        WF_Dtab03.CssClass = ""
        '明細(数量)
        WF_Dtab04.CssClass = ""

        Select Case WF_DetailMView.ActiveViewIndex
            Case 0
                '合計(社内)
                WF_Dtab01.CssClass = "selected"
            Case 1
                '合計(請求)
                WF_Dtab02.CssClass = "selected"
            Case 2
                '明細(金額)
                WF_Dtab03.CssClass = "selected"
            Case 3
                '明細(数量)
                WF_Dtab04.CssClass = "selected"
        End Select

    End Sub

End Class
