Imports System.Data.SqlClient
Imports OFFICE.GRTA0010WRKINC

Public Class GRTA0010JIMOVERTIMEWORK
    Inherits System.Web.UI.Page

    ' 共通関数宣言(BASEDLL)
    ''' <summary>
    ''' LogOutput DirString Get
    ''' </summary>
    Private CS0011LOGWRITE As New CS0011LOGWrite
    ''' <summary>
    ''' セッション情報管理
    ''' </summary>
    Private CS0050Session As New CS0050SESSION
    ''' <summary>
    ''' ユーザプロファイル（GridView）設定
    ''' </summary>
    Private CS0013ProfView As New CS0013ProfView
    ''' <summary>
    ''' 帳票出力(入力：TBL)
    ''' </summary>
    Private CS0030REPORT As New CS0030REPORT                        '帳票出力(入力：TBL)


    ' 共通処理結果
    ''' <summary>
    ''' 共通用エラーID保持枠
    ''' </summary>
    Private WW_ERR_SW As String
    ''' <summary>
    ''' 共通用戻値保持枠
    ''' </summary>
    Private WW_RTN_SW As String
    ''' <summary>
    ''' 共通用引数虚数設定用枠（使用は非推奨）
    ''' </summary>
    Private WW_DUMMY As String


    ' 検索結果
    ''' <summary>
    ''' 時間外労働表示データ
    ''' </summary>
    Private TA0010ViewTbl As DataTable


    ' 画面設定
    ''' <summary>
    ''' 一覧最大表示件数（一画面）
    ''' </summary>
    Public Const CONST_DSPROWCOUNT As Integer = 40
    ''' <summary>
    ''' 一覧のマウススクロール時の増分（件数）
    ''' </summary>
    Public Const CONST_SCROLLROWCOUNT As Integer = 20
    ''' <summary>
    ''' 表示時間の精度
    ''' </summary>
    Public Const PRECISION_DIGITS As Integer = 2
    ''' <summary>
    ''' 表示位置フラグ
    ''' </summary>
    Public Enum QUICK_POSITION As Integer
        Normal = 0
        First = 1
        Last = 2
    End Enum

#Region "# 初期処理"

    ''' <summary>
    ''' 画面描画後処理
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        If IsPostBack Then
            Dim quickPositionFlg As QUICK_POSITION = QUICK_POSITION.Normal
            '■■■ 各ボタン押下処理 ■■■
            If Not String.IsNullOrEmpty(WF_ButtonClick.Value) Then
                Select Case WF_ButtonClick.Value
                    Case "WF_ButtonXLS"                             '■Excel取得ボタン押下時処理
                        WF_ButtonXLS_Click()
                    Case "WF_ButtonEND"                             '■終了ボタン押下時処理
                        WF_ButtonEND_Click()
                    Case "WF_ButtonFIRST"                           '■最始行ボタンクリック時処理
                        quickPositionFlg = QUICK_POSITION.First
                    Case "WF_ButtonLAST"                            '■最終行ボタンクリック時処理
                        quickPositionFlg = QUICK_POSITION.Last
                    Case "WF_MEMOChange"                            '■右ボックスメモ欄変更時処理
                        WF_RIGHTBOX_Change()
                    Case "WF_SELECTOR_SW_Click"                     '■セレクタ選択時
                        SELECTOR_Click()
                End Select
            End If
            ' 一覧再表示処理
            DisplayGrid(quickPositionFlg)
        Else
            '初期化処理
            Initialize()
        End If
    End Sub

    ''' <summary>
    ''' 初期化処理
    ''' </summary>
    Public Sub Initialize()

        ' 遷移元別初期設定
        MapRefelence()

        ' 右BOX設定
        With rightview
            .ResetIndex()
            .MAPID = Master.MAPID
            .MAPVARI = Master.MAPvariant
            .COMPCODE = work.WF_SEL_CAMPCODE.Text
            .PROFID = Master.PROF_REPORT
            .Initialize(WW_DUMMY)
        End With

        ' フッター設定
        Master.dispHelp = False

        ' メイン画面設定
        Master.eventDrop = False

        ' 初期値設定
        SetInitialValue()

        ' 表示データ設定
        GetMapData()

        ' 表示データ保持
        ' ■■■ 画面（GridView）表示データ保存 ■■■
        If Not Master.SaveTable(TA0010ViewTbl, work.WF_SEL_XMLsaveF.Text) Then Exit Sub

        ' セレクタ初期表示処理
        WF_SelectorMView.ActiveViewIndex = 0

        ' 一覧再表示処理
        DisplayGrid()

    End Sub

    ''' <summary>
    ''' 変数設定処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub SetInitialValue()
        Dim wDate As Date = work.WF_SEL_TAISHOYM.Text

        ' 定数設定
        WF_Year.Text = wDate.Year
        WF_Month.Text = wDate.Month
    End Sub

    ''' <summary>
    ''' 遷移元別初期設定
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub MapRefelence()
        ' 検索条件画面からの画面遷移
        If Context.Handler.ToString().ToUpper = C_PREV_MAP_LIST.TA0010S Then
            ' MAPID設定
            If String.IsNullOrEmpty(Master.MAPID) Then
                Master.MAPID = MAPID
            End If
            ' Grid情報保存先設定
            Master.CreateXMLSaveFile()
            work.WF_SEL_XMLsaveF.Text = String.Format("{0}\XML_TMP\{1}-{2}-TA0010-{3}-{4}.txt", CS0050Session.UPLOAD_PATH, Date.Now.ToString("yyyyMMdd"), Master.USERID, Master.MAPvariant, Date.Now.ToString("HHmmss"))
        End If
    End Sub

    ''' <summary>
    ''' 一覧再表示処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub DisplayGrid(Optional ByVal quickPositionFlg As QUICK_POSITION = QUICK_POSITION.Normal)

        ' 表示データ取得
        If IsNothing(TA0010ViewTbl) Then
            If Not Master.RecoverTable(TA0010ViewTbl, work.WF_SEL_XMLsaveF.Text) Then Exit Sub
        End If

        ' 表示データ変換
        Dim wod As New ViewWorkOverData
        wod.SetTable(TA0010ViewTbl)

        ' 表示位置（begin<=LINECNT<end）
        Dim beginGridPosition As Integer
        ' 現在位置取得（取得できなければデフォルト値）
        If Not String.IsNullOrEmpty(WF_GridPosition.Text) AndAlso Not Integer.TryParse(WF_GridPosition.Text, beginGridPosition) Then
            beginGridPosition = 1
        End If

        If Not IsNothing(wod.Items) AndAlso wod.Items.Any() Then

            ' 表示対象行カウント(絞り込み対象)
            If Selector.ALL_SELECT_CODE <> WF_SELECTOR_PosiORG.Value Then
                wod.Items = wod.Items.
                Where(Function(r) r.OrgCode = WF_SELECTOR_PosiORG.Value).
                Select(Function(r, index)
                           r.LINECNT = index + 1
                           Return r
                       End Function).ToList()
            End If

            ' 表示位置取得
            If quickPositionFlg = QUICK_POSITION.First Then
                ' 最始頁の表示位置を設定
                beginGridPosition = 1
            ElseIf quickPositionFlg = QUICK_POSITION.Last Then
                ' 最終頁の表示位置を設定
                If wod.Items.Count Mod CONST_SCROLLROWCOUNT = 0 Then
                    beginGridPosition = wod.Items.Count - (wod.Items.Count Mod CONST_SCROLLROWCOUNT)
                Else
                    beginGridPosition = wod.Items.Count - (wod.Items.Count Mod CONST_SCROLLROWCOUNT) + 1
                End If
            End If

            ' 表示開始_格納位置決定(次頁スクロール)
            If WF_ButtonClick.Value = "WF_MouseWheelUp" Then
                If (beginGridPosition + CONST_SCROLLROWCOUNT) <= wod.Items.Count Then
                    beginGridPosition = beginGridPosition + CONST_SCROLLROWCOUNT
                End If
            End If

            ' 表示開始_位置決定(前頁スクロール)
            If WF_ButtonClick.Value = "WF_MouseWheelDown" Then
                If (beginGridPosition - CONST_SCROLLROWCOUNT) > 0 Then
                    beginGridPosition = beginGridPosition - CONST_SCROLLROWCOUNT
                Else
                    beginGridPosition = 1
                End If
            End If

            ' 表示終了位置決定
            Dim endGridPosition As Integer = beginGridPosition + CONST_DSPROWCOUNT

            ' 表示領域のみデータを抽出
            wod.Items = wod.Items.Where(Function(r) r.LINECNT >= beginGridPosition And r.LINECNT < endGridPosition).ToList()
        Else
            beginGridPosition = 1
        End If

        ' 表示位置設定
        WF_GridPosition.Text = beginGridPosition

        ' 表示領域適用
        CS0013ProfView.CAMPCODE = work.WF_SEL_CAMPCODE.Text
        CS0013ProfView.PROFID = Master.PROF_VIEW
        CS0013ProfView.MAPID = GRTA0010WRKINC.MAPID
        CS0013ProfView.VARI = Master.VIEWID
        CS0013ProfView.SRCDATA = wod.CopyToDataTable()
        CS0013ProfView.TBLOBJ = pnlListArea
        CS0013ProfView.SCROLLTYPE = CS0013ProfView.SCROLLTYPE_ENUM.Horizontal
        CS0013ProfView.TITLEOPT = True
        CS0013ProfView.HIDEOPERATIONOPT = True
        CS0013ProfView.WITHTAGNAMES = True
        CS0013ProfView.CS0013ProfView()

        ' 一覧装飾設定
        SetVieTableDesign(pnlListArea, wod)

    End Sub

    ''' <summary>
    ''' セレクタ初期設定
    ''' </summary>
    Private Sub InitialSelector(ByVal wod As ViewWorkOverData)

        ' セレクタ作成
        Dim orgSlct As New Selector
        orgSlct.Items.Add(New Selector.Item With {
                          .Code = Selector.ALL_SELECT_CODE,
                          .Name = Selector.ALL_SELECT_NAME,
                          .Seq = 0})

        If Not IsNothing(wod) AndAlso Not IsNothing(wod.Items) AndAlso wod.Items.Any() Then
            ' 部署データ整理
            Dim orgQuery As IEnumerable(Of Selector.Item) = wod.Items.
                Select(Function(x As ViewWorkOverData.Item) New Selector.Item With {
                            .Code = x.OrgCode,
                            .Name = x.OrgName,
                            .Seq = x.OrgSeq
                           }).OrderBy(Function(x) x.Seq).Distinct(New Selector.ItemKeyComparator)
            ' 項目追加
            If Not IsNothing(orgQuery) AndAlso orgQuery.Any() Then
                orgSlct.Items.AddRange(orgQuery)
            End If
        End If

        ' 初期選択設定（データバインド前必須）
        WF_SELECTOR_PosiORG.Value = orgSlct.Items.First.Code

        ' セレクタ設定
        WF_ORGselector.DataSource = orgSlct.CopyToDataTable
        ' データバインド
        WF_ORGselector.DataBind()

    End Sub

#End Region

#Region "# イベント"

#Region "## メイン画面"

    ''' <summary>
    ''' Excel取得ボタン押下時処理
    ''' </summary>
    Protected Sub WF_ButtonXLS_Click()

        ' 表示データ取得
        If IsNothing(TA0010ViewTbl) Then
            If Not Master.RecoverTable(TA0010ViewTbl, work.WF_SEL_XMLsaveF.Text) Then Exit Sub
        End If

        ' 表示データ変換
        Dim wod As New ViewWorkOverData
        wod.SetTable(TA0010ViewTbl)
        If IsNothing(wod.Items) OrElse Not wod.Items.Any() Then Exit Sub

        ' 表示対象行カウント(絞り込み対象)
        If Selector.ALL_SELECT_CODE <> WF_SELECTOR_PosiORG.Value Then
            wod.Items = wod.Items.
                Where(Function(r) r.OrgCode = WF_SELECTOR_PosiORG.Value).
                Select(Function(r, index)
                           r.LINECNT = index + 1
                           Return r
                       End Function).ToList()
        End If

        ' タイトルデータ挿入
        If IsNothing(wod.Items) OrElse Not wod.Items.Any() Then Exit Sub
        wod.Items.First().Year = WF_Year.Text
        wod.Items.First().Month = WF_Month.Text
        If Selector.ALL_SELECT_CODE <> WF_SELECTOR_PosiORG.Value Then
            wod.Items.First().SelectOrgName = wod.Items.
                Where(Function(r) r.OrgCode = WF_SELECTOR_PosiORG.Value).Select(Function(x) x.OrgName).FirstOrDefault()
        Else
            ' 部署
            Dim orgCode As String = work.WF_SEL_ORG.Text
            If String.IsNullOrWhiteSpace(orgCode) Then
                ' 指定されていなければ、ログインユーザの部署を指定
                orgCode = Master.USER_ORG
            End If
            wod.Items.First().SelectOrgName = wod.Items.
                Where(Function(r) r.OrgCode = orgCode).Select(Function(x) x.OrgName).FirstOrDefault()
        End If

        ' 帳票出力
        CS0030REPORT.CAMPCODE = work.WF_SEL_CAMPCODE.Text       '会社コード
        CS0030REPORT.PROFID = Master.PROF_REPORT                'プロファイルID
        CS0030REPORT.MAPID = GRTA0010WRKINC.MAPID               '画面ID
        CS0030REPORT.REPORTID = rightview.GetReportId()         '帳票ID
        CS0030REPORT.FILEtyp = "XLSX"                           '出力ファイル形式
        CS0030REPORT.TBLDATA = wod.CopyToDataTable()            'データ参照DataTable
        CS0030REPORT.CS0030REPORT()
        If Not isNormal(CS0030REPORT.ERR) Then
            Master.Output(CS0030REPORT.ERR, C_MESSAGE_TYPE.ABORT, "CS0030REPORTtbl")
            Exit Sub
        End If

        ' 別画面でExcelを表示
        WF_PrintURL.Value = CS0030REPORT.URL
        ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_ExcelPrint();", True)

    End Sub

    ''' <summary>
    ''' 終了ボタン押下時処理
    ''' </summary>
    Protected Sub WF_ButtonEND_Click()
        Master.TransitionPrevPage()
    End Sub

    ''' <summary>
    ''' セレクタ選択時
    ''' </summary>
    Private Sub SELECTOR_Click()
        Dim mvIndex As Integer = WF_SelectorMView.ActiveViewIndex
        If mvIndex = 0 Then
            For Each item In WF_ORGselector.Items
                Dim valueCtrl As Label = CType(item.FindControl("WF_SELorg_VALUE"), Label)
                Dim textCtrl As Label = CType(item.FindControl("WF_SELorg_TEXT"), Label)

                ' 背景色
                If valueCtrl.Text = WF_SELECTOR_PosiORG.Value Then
                    ' 選択項目
                    textCtrl.Style.Value = "height:1.5em;width:11.7em;background-color:darksalmon;border: solid 1.0px black;font-size:1.3rem;"
                Else
                    ' 非選択項目
                    textCtrl.Style.Value = "height:1.5em;width:11.7em;background-color:rgb(220,230,240);border: solid 1.0px black;font-size:1.3rem;"
                End If
            Next
        End If
        ' 表示位置初期化
        WF_GridPosition.Text = 1
    End Sub

    ''' <summary>
    ''' セレクタの各項目バインド時ハンドラ
    ''' </summary>
    ''' <param name="Sender"></param>
    ''' <param name="e"></param>
    Sub WF_ORGselector_ItemDataBound(ByVal Sender As Object, ByVal e As RepeaterItemEventArgs) Handles WF_ORGselector.ItemDataBound
        ' ItemTemplateのみ処理
        If e.Item.ItemType = ListItemType.Item Or e.Item.ItemType = ListItemType.AlternatingItem Then
            Dim row As DataRowView = CType(e.Item.DataItem, DataRowView)
            Dim valueCtrl As Label = CType(e.Item.FindControl("WF_SELorg_VALUE"), Label)
            Dim textCtrl As Label = CType(e.Item.FindControl("WF_SELorg_TEXT"), Label)
            ' 値とテキスト設定
            valueCtrl.Text = row("CODE").ToString
            textCtrl.Text = "　" & row("NAME").ToString
            ' 背景色
            If valueCtrl.Text = WF_SELECTOR_PosiORG.Value Then
                ' 選択項目
                textCtrl.Style.Value = "height:1.5em;width:11.7em;background-color:darksalmon;border: solid 1.0px black;font-size:1.3rem;"
            Else
                ' 非選択項目
                textCtrl.Style.Value = "height:1.5em;width:11.7em;background-color:rgb(220,230,240);border: solid 1.0px black;font-size:1.3rem;"
            End If
            ' イベント追加
            textCtrl.Attributes.Remove("onclick")
            textCtrl.Attributes.Add("onclick", String.Format("SELECTOR_Click('{0}','{1}');", "0", valueCtrl.Text))
        End If
    End Sub

#End Region

#Region "## 右BOX"

    ''' <summary>
    ''' 右BOXメモ欄変更時処理
    ''' </summary>
    Protected Sub WF_RIGHTBOX_Change()
        ' 右Boxメモ変更時処理
        rightview.Save(Master.USERID, Master.USERTERMID, WW_DUMMY)
    End Sub

#End Region

#End Region

    ''' <summary>
    ''' 表示データ取得
    ''' </summary>
    Private Sub GetMapData()

        ' 時間外労働基準データ取得
        Dim woc As New WorkOverCriteria
        Try
            woc.Fetch(CS0050Session.getConnection(), work.WF_SEL_CAMPCODE.Text)
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "L0001_OVERWORKCRITERIA SELECT")
            CS0011LOGWRITE.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:L0001_OVERWORKCRITERIA SELECT"           '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
        End Try

        ' 出力対象年月（末日）
        Dim endDate As Date = work.WF_SEL_TAISHOYM.Text
        endDate = endDate.AddMonths(1).AddDays(-1)
        ' 表示開始年月（出力対象年月の年度開始月）
        Dim fisicalYear As Integer = IIf(endDate.Month < 4, endDate.Year - 1, endDate.Year)
        Dim viewBeginDate As Date = Date.Parse(String.Format("{0}/04", fisicalYear.ToString))
        ' 取得データは出力対象年月の年度4月より平均算出に必要な月数を遡った年月～となる。
        Dim beginDate As Date = viewBeginDate.AddMonths(Math.Abs(woc.Items.AvgCalcMonths) * -1)

        ' 時間外労働時間データ取得
        Dim wod As New WorkOverData
        Try
            wod.Fetch(CS0050Session.getConnection(), work.WF_SEL_CAMPCODE.Text, work.WF_SEL_ORG.Text, beginDate, endDate, Master.USER_ORG)
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "L0001_KINTAI SELECT")
            CS0011LOGWRITE.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:L0001_OVERWORKCRITERIA SELECT"           '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
        End Try

        ' 表示用データテーブル作成
        Dim vwod As ViewWorkOverData = Nothing
        Try
            ' 表示用データ集計
            vwod = CreateViewWorkOverData(wod, woc.Items, viewBeginDate, endDate)
            ' テーブル変換（集計）
            TA0010ViewTbl = vwod.CopyToDataTable()
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "L0001_KINTAI AGGREGATE")
            CS0011LOGWRITE.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:L0001_KINTAI AGGREGATE"           '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.SELECT_DETAIL_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
        End Try

        ' セレクタ作成
        InitialSelector(vwod)

    End Sub

    ''' <summary>
    ''' 対象月から指定月数迄のそれぞれの平均が基準を超えている月のリストを作成
    ''' </summary>
    ''' <param name="wodItem"></param>
    ''' <param name="viewBeginDate"></param>
    ''' <param name="endDate"></param>
    ''' <param name="wocItem"></param>
    ''' <returns></returns>
    Private Function GetAvgMonthFlgList(ByVal wodItem As List(Of WorkOverData.Item), ByVal viewBeginDate As Date, ByVal endDate As Date, ByVal wocItem As WorkOverCriteria.Item) As Dictionary(Of MONTHS, VIEW_FLG)

        ' 平均算出月（2～acm）
        Dim AvgCulcMonths As Integer = Math.Abs(wocItem.AvgCalcMonths)
        ' 基準超過月リスト
        Dim targetYMs As New Dictionary(Of MONTHS, VIEW_FLG)

        ' 指定月のみの処理とする。（全月を基準としてやるならTo12とする。）
        For i As Integer = 1 To 1
            ' 計算基準年月
            Dim targetDate As Date = endDate.AddMonths(i * -1 + 1)
            ' 表示範囲外であれば計算は不要
            If viewBeginDate > targetDate Then Exit For

            Dim curMonFlg As VIEW_FLG = VIEW_FLG.None
            Dim overTimeListWithHoliday As New List(Of Decimal)
            For j As Integer = 1 To AvgCulcMonths
                ' 既にフラグが立っていれば以降は計算不要
                If curMonFlg And VIEW_FLG.Problematic Then Exit For

                ' 計算対象データ積み上げ
                Dim ym As String = targetDate.AddMonths(j * -1 + 1).ToString("yyyy/MM")
                Dim curItem As WorkOverData.Item = wodItem.Where(Function(x) x.TaishoYM = ym).FirstOrDefault()
                If IsNothing(curItem) Then
                    overTimeListWithHoliday.Add(0)
                Else
                    overTimeListWithHoliday.Add(curItem.OverTimeWithHoliday)
                End If

                ' 初回は計算しない
                If j = 1 Then Continue For

                ' 平均算出（j=2～）
                Dim avgOverTimeWithHoliday As Decimal = overTimeListWithHoliday.Average()

                ' フラグ設定
                If avgOverTimeWithHoliday > wocItem.AvgMonthMaxTime Then
                    curMonFlg = VIEW_FLG.Problematic
                ElseIf avgOverTimeWithHoliday > wocItem.AvgMonthWarnTime Then
                    curMonFlg = VIEW_FLG.Warning
                End If
            Next
            ' 追加
            targetYMs.Add(targetDate.Month, curMonFlg)
        Next

        Return targetYMs

    End Function

    ''' <summary>
    ''' 表示データ計算用除算
    ''' </summary>
    ''' <param name="x"></param>
    ''' <param name="y"></param>
    ''' <returns></returns>
    Private Function division(ByVal x As Decimal, ByVal y As Decimal) As Decimal
        If IsNothing(x) OrElse x = 0 Then Return 0
        If IsNothing(y) OrElse y = 0 Then Return x
        Return x / y
    End Function

    ''' <summary>
    ''' 表示データ作成（集計）
    ''' </summary>
    ''' <param name="wod"></param>
    ''' <param name="wocItem"></param>
    ''' <returns></returns>
    Private Function CreateViewWorkOverData(ByVal wod As WorkOverData, ByVal wocItem As WorkOverCriteria.Item, ByVal viewBeginDate As Date, endDate As Date) As ViewWorkOverData

        ' 表示時間の精度
        Dim precision As Integer = Fix(Math.Pow(10, PRECISION_DIGITS))
        Dim vfPrecision As String = "0.00"

        ' ユーザ別集計
        Dim vwod As New ViewWorkOverData
        vwod.Items = wod.Items.
            GroupBy(Function(x) x.StaffCode).
            Select(
                Function(g, index)

                    ' 行番号
                    Dim viewItem As New ViewWorkOverData.Item
                    viewItem.LINECNT = index + 1

                    ' 行固定項目
                    Dim rowConstItem As WorkOverData.Item = g.FirstOrDefault()
                    With viewItem
                        .StaffCode = rowConstItem.StaffCode
                        .StaffName = rowConstItem.StaffName
                        .OrgCode = rowConstItem.OrgCode
                        .OrgName = rowConstItem.OrgName
                    End With

                    ' 超過回数（複数月の平均）
                    Dim avgMonthFlgList As Dictionary(Of MONTHS, VIEW_FLG) = GetAvgMonthFlgList(g.ToList(), viewBeginDate, endDate, wocItem)
                    If Not IsNothing(avgMonthFlgList) AndAlso avgMonthFlgList.Any() Then
                        viewItem.MonthsAvgMaxTimeExceededCount = avgMonthFlgList.AsEnumerable.Count(Function(x) x.Value And VIEW_FLG.Problematic)
                        viewItem.MonthsAvgWarnTimeExceededCount = avgMonthFlgList.AsEnumerable.Count(Function(x) x.Value And VIEW_FLG.Warning)
                    Else
                        viewItem.MonthsAvgMaxTimeExceededCount = 0
                        viewItem.MonthsAvgWarnTimeExceededCount = 0
                    End If

                    ' 年度単位で集計が必要なデータを抽出
                    Dim vg As IEnumerable(Of WorkOverData.Item) = g.Where(Function(x) Date.Parse(x.TaishoYM) >= viewBeginDate)

                    Dim annualTotalTime As Decimal = 0
                    Dim averageTimeWithHoliday As Decimal = 0
                    If Not IsNothing(vg) AndAlso vg.Any() Then
                        ' 超過回数（単月）
                        viewItem.MonthMaxTimeExceededCountWithHoliday = vg.Count(Function(x) x.OverTimeWithHoliday > wocItem.MonthMaxTime)
                        viewItem.MonthPrincipleTimeExceededCount = vg.Count(Function(x) x.OverTime > wocItem.MonthPrincipleTime)
                        viewItem.MonthAvgMaxTimeExceededCountWithHoliday = vg.Count(Function(x) x.OverTimeWithHoliday > wocItem.AvgMonthMaxTime)

                        ' 累計
                        annualTotalTime = vg.Sum(Function(ax) ax.OverTime)
                        ' 1ヶ月平均（存在する月の平均）
                        averageTimeWithHoliday = vg.Average(Function(x) x.OverTimeWithHoliday)
                    Else
                        viewItem.MonthMaxTimeExceededCountWithHoliday = 0
                        viewItem.MonthPrincipleTimeExceededCount = 0
                        viewItem.MonthAvgMaxTimeExceededCountWithHoliday = 0
                    End If

                    ' ===== スタイルと書式設定 =====
                    viewItem.VF_MonthMaxTimeExceededCountWithHoliday = VIEW_FLG.FormatCount
                    viewItem.VF_MonthPrincipleTimeExceededCount = VIEW_FLG.FormatCount
                    viewItem.VF_MonthAvgMaxTimeExceededCountWithHoliday = VIEW_FLG.FormatCount

                    ' 累計
                    If annualTotalTime > wocItem.YearMaxTime Then
                        ' 違法状態設定
                        viewItem.VF_AnnualTotalTime = VIEW_FLG.Problematic
                    ElseIf annualTotalTime > wocItem.YearWarnTime Then
                        ' 警告設定
                        viewItem.VF_AnnualTotalTime = VIEW_FLG.Warning
                    End If
                    viewItem.AnnualTotalTime = division(Math.Floor(division(annualTotalTime, 60) * precision), precision).ToString(vfPrecision)

                    ' 上限超過残数
                    Dim exRemCount As Integer = wocItem.MonthPrincipleCount - viewItem.MonthPrincipleTimeExceededCount
                    viewItem.ExcessRemainingCount = exRemCount
                    If exRemCount < 0 Then
                        viewItem.VF_ExcessRemainingCount = VIEW_FLG.Problematic Or VIEW_FLG.ReplaceStatus
                    ElseIf exRemCount = 0 Then
                        viewItem.VF_ExcessRemainingCount = VIEW_FLG.Warning Or VIEW_FLG.FormatRemainingCount
                    Else
                        viewItem.VF_ExcessRemainingCount = VIEW_FLG.FormatRemainingCount
                    End If

                    ' ペース
                    Dim vMCount As Decimal = DateDiff("m", viewBeginDate, endDate) + 1
                    Dim pace As Decimal = division(viewItem.MonthPrincipleTimeExceededCount, vMCount)
                    Dim paceLimit As Decimal = division(wocItem.MonthPrincipleCount, 12)
                    If pace > paceLimit Then
                        viewItem.PaceStatus = "注意"
                        viewItem.VF_PaceStatus = VIEW_FLG.Warning
                    Else
                        viewItem.PaceStatus = ""
                    End If

                    ' 上限超過ステータス
                    If CInt(viewItem.MonthMaxTimeExceededCountWithHoliday) > 0 Then
                        viewItem.MonthMaxTimeExceededStatusWithHoliday = "違法状態！"
                        viewItem.VF_MonthMaxTimeExceededStatusWithHoliday = VIEW_FLG.Problematic
                    Else
                        viewItem.MonthMaxTimeExceededStatusWithHoliday = ""
                    End If

                    ' 複数月上限超過ステータス
                    If viewItem.MonthsAvgMaxTimeExceededCount > 0 Then
                        viewItem.MonthsAvgMaxTimeExceededStatusWithHoliday = "違法状態！"
                        viewItem.VF_MonthsAvgMaxTimeExceededStatusWithHoliday = VIEW_FLG.Problematic
                    ElseIf viewItem.MonthsAvgWarnTimeExceededCount > 0 Then
                        viewItem.MonthsAvgMaxTimeExceededStatusWithHoliday = "注意"
                        viewItem.VF_MonthsAvgMaxTimeExceededStatusWithHoliday = VIEW_FLG.Warning
                    Else
                        viewItem.MonthsAvgMaxTimeExceededStatusWithHoliday = ""
                    End If

                    ' 1ヶ月平均
                    If averageTimeWithHoliday > wocItem.AvgMonthMaxTime Then
                        viewItem.VF_AverageTimeWithHoliday = VIEW_FLG.Problematic
                    ElseIf averageTimeWithHoliday > wocItem.AvgMonthWarnTime Then
                        viewItem.VF_AverageTimeWithHoliday = VIEW_FLG.Warning
                    End If
                    viewItem.AverageTimeWithHoliday = division(Math.Floor(division(averageTimeWithHoliday, 60) * precision), precision).ToString(vfPrecision)

                    ' 各月処理
                    For i As Integer = 1 To 12
                        ' 計算基準年月
                        Dim targetDate As Date = endDate.AddMonths(i * -1 + 1)
                        ' 表示範囲外であれば計算は不要
                        If viewBeginDate > targetDate Then Exit For
                        ' 月名取得
                        Dim currentMonth As Integer = targetDate.Month
                        Dim currentMonthName As String = [Enum].GetName(GetType(MONTHS), currentMonth)
                        ' プロパティ取得
                        Dim overtimeInfo As Reflection.PropertyInfo = GetType(ViewWorkOverData.Item).GetProperty(currentMonthName & "Overtime")
                        Dim overtimeInfoWithHoliday As Reflection.PropertyInfo = GetType(ViewWorkOverData.Item).GetProperty(currentMonthName & "OvertimeWithHoliday")
                        Dim vfOvertimeInfo As Reflection.PropertyInfo = GetType(ViewWorkOverData.Item).GetProperty("VF_" & overtimeInfo.Name)
                        Dim coachingInfo As Reflection.PropertyInfo = GetType(ViewWorkOverData.Item).GetProperty(currentMonthName & "Coaching")
                        ' 当月データ取得
                        Dim curWodItem As WorkOverData.Item = g.Where(Function(x) x.TaishoYM = targetDate.ToString("yyyy/MM")).FirstOrDefault()
                        If IsNothing(curWodItem) Then Continue For
                        ' 時間を設定
                        overtimeInfo.SetValue(viewItem, division(Math.Floor(division(curWodItem.OverTime, 60) * precision), precision).ToString(vfPrecision))
                        overtimeInfoWithHoliday.SetValue(viewItem, division(Math.Floor(division(curWodItem.OverTimeWithHoliday, 60) * precision), precision).ToString(vfPrecision))
                        ' 時間の表示を設定
                        If curWodItem.OverTime > wocItem.MonthMaxTime Then
                            vfOvertimeInfo.SetValue(viewItem, VIEW_FLG.Problematic)
                        ElseIf curWodItem.OverTime > wocItem.MonthPrincipleTime Then
                            vfOvertimeInfo.SetValue(viewItem, VIEW_FLG.Warning)
                        End If
                        ' 指導を設定
                        If curWodItem.OverTimeWithHoliday > wocItem.MonthWarnTime Then
                            ' 指導
                            coachingInfo.SetValue(viewItem, "○")
                        End If
                    Next

                    Return viewItem
                End Function
            ).ToList()

        Return vwod

    End Function

    ''' <summary>
    ''' 一覧データのデザイン適用（画面固有）
    ''' </summary>
    ''' <param name="pnl"></param>
    ''' <param name="wod"></param>
    Private Sub SetVieTableDesign(ByVal pnl As Panel, ByVal wod As ViewWorkOverData)
        Try
            Dim rTblCtrl As Control = pnlListArea.FindControl("pnlListArea_DR").Controls(0)

            ' スタイル適用先と参照フラグのペアを作成
            Dim tmpWod As New ViewWorkOverData
            Dim propPairs As Dictionary(Of Reflection.PropertyInfo, Reflection.PropertyInfo) = Nothing
            propPairs = tmpWod.GetPropViewFlgPairs()

            If IsNothing(propPairs) OrElse Not propPairs.Any() Then Exit Sub
            Dim maxRowCount As Integer = wod.Items.Count()
            For Each row In wod.Items.Select(Function(wodItem, index) New With {wodItem, index})

                ' 表示フラグ取得
                Dim stylePairs As New Dictionary(Of String, VIEW_FLG)
                For Each propPair As KeyValuePair(Of Reflection.PropertyInfo, Reflection.PropertyInfo) In propPairs
                    Dim flg As VIEW_FLG = propPair.Value.GetValue(row.wodItem)
                    If flg <> VIEW_FLG.None Then
                        stylePairs.Add(propPair.Key.Name.ToUpper(), flg)
                    End If
                Next

                For Each rTblCellCtrl As Control In rTblCtrl.Controls.Item(row.index).Controls
                    If rTblCellCtrl.GetType.Name.ToUpper() <> "TABLECELL" Then Continue For
                    Dim rTableCell As TableCell = CType(rTblCellCtrl, TableCell)

                    ' 表示フラグ適用
                    For Each stylePair As KeyValuePair(Of String, VIEW_FLG) In stylePairs
                        If Not rTableCell.Attributes("name").StartsWith("R_" & stylePair.Key) Then Continue For

                        ' スタイル
                        If stylePair.Value And VIEW_FLG.Problematic Then
                            rTableCell.Style.Add("color", "yellow")
                            rTableCell.Style.Add("background-color", "red")
                        ElseIf stylePair.Value And VIEW_FLG.Warning Then
                            rTableCell.Style.Add("color", "red")
                            rTableCell.Style.Add("background-color", "yellow")
                        End If

                        ' 書式
                        If stylePair.Value And VIEW_FLG.FormatCount Then
                            rTableCell.Text = String.Format("{0}回", rTableCell.Text)
                        ElseIf stylePair.Value And VIEW_FLG.FormatRemainingCount Then
                            rTableCell.Text = String.Format("あと{0}回", rTableCell.Text)
                        End If

                        ' 置換
                        If stylePair.Value And VIEW_FLG.ReplaceStatus Then
                            If stylePair.Value And VIEW_FLG.Problematic Then
                                rTableCell.Text = "違法状態！"
                            ElseIf stylePair.Value And VIEW_FLG.Warning Then
                                rTableCell.Text = "注意"
                            End If
                            rTableCell.ToolTip = rTableCell.Text
                        End If

                    Next
                Next

            Next

        Catch ex As Exception
            Exit Sub
        End Try
    End Sub

End Class