Imports System.Data.SqlClient
Imports System.IO
Imports OFFICE.GRIS0005LeftBox
Imports OFFICE.GRT00015COM

''' <summary>
''' 庸車実績（実行）
''' </summary>
''' <remarks></remarks>
Public Class GRT00015SUPPLJISSKI
    Inherits System.Web.UI.Page

    Private T00015ds As DataSet                                     '格納ＤＳ
    Private T00015tbl As DataTable                                  'Grid格納用テーブル
    Private T00015INPtbl As DataTable                               'Detail入力用テーブル
    Private T00015UPDtbl As DataTable                               '更新時作業テーブル
    Private T00015SUMtbl As DataTable                               '更新時作業テーブル
    Private T00015WKtbl As DataTable                                '更新時作業テーブル
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
    Private Const CONST_DSPROWCOUNT As Integer = 40                 '１画面表示対象
    Private Const CONST_SCROLLROWCOUNT As Integer = 20              'マウススクロール時の増分
    Private Const CONST_DETAIL_TABID As String = "DTL1"             '詳細部タブID

    Private Const C_UPLOAD_EXCEL_REPORTID_NJS As String = "NJS配車表"
    Private Const C_UPLOAD_EXCEL_PROFID_NJS As String = "Default"

    ''' <summary>
    ''' 車輛タイプ
    ''' </summary>
    Private Class SYARYOTYPE
        Public Const SHARYO_CHAASSIS As String = "A"
        Public Const SHARYO_TANK As String = "B"
        Public Const SHARYO_TRACTOR As String = "C"
        Public Const SHARYO_TRAILER As String = "D"

        Public Const SHARYO_CHAASSIS_YO As String = "E"
        Public Const SHARYO_TANK_YO As String = "F"
        Public Const SHARYO_TRACTOR_YO As String = "G"
        Public Const SHARYO_TRAILER_YO As String = "H"

        ''' <summary>
        ''' 車輛タイプリスト
        ''' </summary>
        Public Shared ReadOnly SHARYO_LIST As String() = {"A", "B", "C", "D", "E", "F", "G", "H"}

        ''' <summary>
        ''' 車検切れチェック対象車輛タイプリスト
        ''' </summary>
        Public Shared ReadOnly INSPECTION_LIST As String() = {"A", "C", "D", "E", "G", "H"}
        ''' <summary>
        ''' 容器検査切れチェック対象車輛タイプリスト
        ''' </summary>
        Public Shared ReadOnly TANK_LIST As String() = {"B", "D", "F", "H"}
    End Class


    ''' <summary>
    ''' サーバ処理の遷移先
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

        Try
            '■■■ 作業用データベース設定 ■■■
            T00015ds = New DataSet()                                      '初期化
            T00015tbl = T00015ds.Tables.Add("T00015TBL")
            T00015INPtbl = T00015ds.Tables.Add("T00015INPTBL")
            T00015UPDtbl = T00015ds.Tables.Add("T00015UPDtbl")
            T00015SUMtbl = T00015ds.Tables.Add("T00015SUMtbl")
            T00015WKtbl = T00015ds.Tables.Add("T00015WKtbl")
            T00015ds.EnforceConstraints = False
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
                        Case "WF_ButtonGet"                     '取り込み
                            WF_ButtonGet_Click()
                        Case "WF_ButtonSAVE"                    '一時保存
                            WF_ButtonSAVE_Click()
                        Case "WF_ButtonExtract"                 '絞り込み
                            WF_ButtonExtract_Click()
                        Case "WF_ButtonNEW"                     '新規
                            WF_ButtonNEW_Click()
                        Case "WF_ButtonUPDATE"                  'DB更新
                            WF_ButtonUPDATE_Click()
                        Case "WF_ButtonCSV"                     'ﾀﾞｳﾝﾛｰﾄﾞ
                            WF_ButtonCSV_Click()
                        Case "WF_ButtonPrint"                   '一覧印刷
                            WF_Print_Click()
                        Case "WF_ButtonFIRST"                   '先頭頁[image]
                            WF_ButtonFIRST_Click()
                        Case "WF_ButtonLAST"                    '最終頁[image]
                            WF_ButtonLAST_Click()
                        Case "WF_ButtonALLSELECT"               '全選択
                            WF_ButtonALLSELECT_Click()
                        Case "WF_ButtonEND"                     '終了
                            WF_ButtonEND_Click()

                            '********* 一覧 *********
                        Case "WF_GridDBclick"                   'DBClick
                            WF_Grid_DBclick()
                        Case "WF_MouseWheelDown"                'MouseDown
                            WF_GRID_Scrole()
                        Case "WF_MouseWheelUp"                  'MouseUp
                            WF_GRID_Scrole()
                        Case "WF_UPLOAD_EXCEL"                  'EXCEL_UPLOAD
                            UPLOAD_EXCEL()

                            '********* 詳細部 *********
                        Case "WF_UPDATE"                        '表更新
                            WF_UPDATE_Click()
                        Case "WF_CLEAR"                         'クリア
                            WF_CLEAR_Click()
                        Case "WF_BACK"                          '戻る
                            WF_BACK_Click()

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
            If Not IsNothing(T00015ds) Then
                For Each tbl In T00015ds.Tables
                    tbl.Dispose()
                    tbl = Nothing
                Next
                T00015ds.Dispose()
                T00015ds = Nothing
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
        Master.MAPID = GRT00015WRKINC.MAPID
        WF_FIELD.Value = ""
        WF_FIELD_REP.Value = ""
        WF_SELTORICODE.Focus()

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

        '○業務車番設定
        InitGSHABAN()

        '■■■ 画面（GridView）表示項目取得 ■■■
        If work.WF_SEL_RESTART.Text = "RESTART" Then
            '○画面表示データ復元
            Master.RecoverTable(T00015tbl, work.WF_SEL_XMLsaveTmp.Text)

        Else
            '○画面表示データ取得
            GRID_INITset()

            '○数量、台数合計の設定
            SUMMRY_SET()
        End If

        '○Grid情報保存先のファイル名
        Master.createXMLSaveFile()

        '○画面表示データ保存
        Master.SaveTable(T00015tbl)

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


    End Sub

    ''' <summary>
    ''' GridView用データ取得
    ''' </summary>
    ''' <remarks>データベース（T00015）を検索し画面表示する一覧を作成する</remarks>
    Private Sub GRID_INITset()

        '○画面表示データ取得
        DBselect_T15SELECT()

        '○ソート
        'ソート文字列取得
        CS0026TBLSORTget.COMPCODE = work.WF_SEL_CAMPCODE.Text
        CS0026TBLSORTget.MAPID = Master.MAPID
        CS0026TBLSORTget.PROFID = Master.PROF_VIEW
        CS0026TBLSORTget.VARI = Master.MAPvariant
        CS0026TBLSORTget.TAB = ""
        CS0026TBLSORTget.getSorting()

        'ソート＆データ抽出
        CS0026TBLSORTget.TABLE = T00015tbl
        CS0026TBLSORTget.SORTING = CS0026TBLSORTget.SORTING
        CS0026TBLSORTget.FILTER = "SELECT = 1"
        CS0026TBLSORTget.Sort(T00015tbl)

        '○LineCNT付番・枝番再付番
        Dim WW_LINECNT As Integer = 0
        Dim WW_SEQ As Integer = 0

        For i As Integer = 0 To T00015tbl.Rows.Count - 1

            Dim T00015row = T00015tbl.Rows(i)

            If T00015row("LINECNT") = 0 Then

                WW_LINECNT = WW_LINECNT + 1
                WW_SEQ = 0

                For j As Integer = i To T00015tbl.Rows.Count - 1

                    If T00015tbl.Rows(j)("LINECNT") = 0 Then
                        If CompareOrder(T00015row, T00015tbl.Rows(j)) Then

                            WW_SEQ = WW_SEQ + 1
                            T00015tbl.Rows(j)("LINECNT") = WW_LINECNT
                            T00015tbl.Rows(j)("SEQ") = WW_SEQ.ToString("00")

                            If WW_SEQ = 1 Then
                                T00015tbl.Rows(j)("HIDDEN") = 0
                            Else
                                '枝番データは非表示
                                T00015tbl.Rows(j)("HIDDEN") = 1
                            End If
                        Else
                            'Exit For    …　ソート定義に依存するのでExitできない
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

        '表示対象行カウント(絞り込み対象)
        If T00015tbl.Columns.Count = 0 Then
            '○画面表示データ復元
            If Master.RecoverTable(T00015tbl) <> True Then Exit Sub
        End If
        '　※　絞込（Cells("Hidden")： 0=表示対象 , 1=非表示対象)
        For Each T00015row In T00015tbl.Rows
            If T00015row("HIDDEN") = "0" Then
                WW_DataCNT = WW_DataCNT + 1
            End If
        Next

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

        '○画面（GridView）表示
        Dim WW_TBLview As DataView = New DataView(T00015tbl)

        'ソート
        WW_TBLview.Sort = "LINECNT"
        WW_TBLview.RowFilter = "HIDDEN = 0 and LINECNT >= " & WW_GridPosition.ToString & " and LINECNT < " & (WW_GridPosition + CONST_DSPROWCOUNT).ToString

        '一覧作成
        CS0013ProfView.CAMPCODE = work.WF_SEL_CAMPCODE.Text
        CS0013ProfView.PROFID = Master.PROF_VIEW
        CS0013ProfView.MAPID = Master.MAPID
        CS0013ProfView.VARI = Master.VIEWID
        CS0013ProfView.SRCDATA = WW_TBLview.ToTable
        CS0013ProfView.TBLOBJ = pnlListArea
        CS0013ProfView.SCROLLTYPE = CS0013ProfView.SCROLLTYPE_ENUM.None
        CS0013ProfView.LEVENT = "ondblclick"
        CS0013ProfView.LFUNC = "ListDbClick"
        CS0013ProfView.TITLEOPT = True
        CS0013ProfView.CS0013ProfView()

        '○クリア
        If WW_TBLview.Count = 0 Then
            WF_GridPosition.Text = "1"
        Else
            WF_GridPosition.Text = WW_TBLview.Item(0)("LINECNT")
        End If

        '1.現在表示しているLINECNTのリストをビューステートに保持
        '2.チェックがついているチェックボックスオブジェクトをチェック状態にする
        If WW_TBLview.ToTable IsNot Nothing AndAlso WW_TBLview.ToTable.Rows.Count > 0 Then
            Dim displayLineCnt As List(Of Integer) = (From dr As DataRow In WW_TBLview.ToTable
                                                      Select Convert.ToInt32(dr.Item("LINECNT"))).ToList
            ViewState("DISPLAY_LINECNT_LIST") = displayLineCnt
            Dim targetCheckBoxLineCnt = (From dr As DataRow In WW_TBLview.ToTable
                                         Where Convert.ToString(dr.Item("ROWDEL")) <> ""
                                         Select Convert.ToInt32(dr.Item("LINECNT")))
            For Each lineCnt As Integer In targetCheckBoxLineCnt
                Dim chkObjId As String = "chk" & Me.pnlListArea.ID & "ROWDEL" & lineCnt.ToString
                Dim tmpObj As Control = Me.pnlListArea.FindControl(chkObjId)
                Dim hchkObjId As String = "hchk" & Me.pnlListArea.ID & "ROWDEL" & lineCnt.ToString
                Dim htmpObj As Control = Me.pnlListArea.FindControl(hchkObjId)
                If Not IsNothing(tmpObj) AndAlso Not IsNothing(htmpObj) Then
                    Dim chkObj As CheckBox = DirectCast(tmpObj, CheckBox)
                    Dim hchkObj As Label = DirectCast(htmpObj, Label)
                    If hchkObj.Text = "1" Then
                        chkObj.Checked = 1
                    Else
                        chkObj.Checked = 0
                    End If
                End If
            Next
        Else
            ViewState("DISPLAY_LINECNT_LIST") = Nothing
        End If

        WW_TBLview.Dispose()
        WW_TBLview = Nothing

        CODENAME_get("TORICODE", WF_SELTORICODE.Text, WF_SELTORICODE_TEXT.Text, WW_DUMMY)         '取引先

    End Sub

    ''' <summary>
    ''' 日報取込ボタン押下処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonGet_Click()

        rightview.setErrorReport("")
        Dim O_RTN As String = C_MESSAGE_NO.NORMAL
        Dim WW_DATENOW As Date = Date.Now

        '日報データ取得
        NippoDATAget(WW_ERRCODE)

        '庸車実績削除
        NippoDATAdelete(WW_DATENOW, WW_ERRCODE)

        '日報データ登録
        NippoDATAinsert(WW_DATENOW, WW_ERRCODE)

        If Not isNormal(WW_ERRCODE) Then
            Master.Output(WW_ERRCODE, C_MESSAGE_TYPE.ABORT, "庸車実績DB追加")
        End If

        '○画面表示データ取得
        GRID_INITset()

        '○数量、台数合計の設定
        SUMMRY_SET()

        '○Grid情報保存先のファイル名
        Master.CreateXMLSaveFile()

        '○画面表示データ保存
        Master.SaveTable(T00015tbl)

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
                   & "   AND B.ENDYMD >= A.YMD                                            " _
                   & " WHERE A.CAMPCODE         = @P01                                   " _
                   & "   and A.YMD             <= @P02                                   " _
                   & "   and A.YMD             >= @P03                                   " _
                   & "   and A.SHIPORG          = @P04                                   " _
                   & "   and A.SHARYOTYPEF     IN ('E','G')                              " _
                   & "   and A.WORKKBN          = 'B3'                                   " _
                   & "   and A.DELFLG          <> '1'                                    "

                SQLStr = SQLStr & " ORDER BY CAMPCODE ,YMD ,SEQ                        "

                Dim SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", System.Data.SqlDbType.NVarChar, 2)
                Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", System.Data.SqlDbType.Date)
                Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@P03", System.Data.SqlDbType.Date)
                Dim PARA04 As SqlParameter = SQLcmd.Parameters.Add("@P04", System.Data.SqlDbType.NVarChar, 15)
                PARA01.Value = work.WF_SEL_CAMPCODE.Text
                PARA02.Value = work.WF_SEL_SHUKODATET.Text
                PARA03.Value = work.WF_SEL_SHUKODATEF.Text
                PARA04.Value = work.WF_SEL_SHIPORG.Text

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
    ''' T00015tbl関連データ削除
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
                      " UPDATE T0015_SUPPLJISSKI        " _
                    & "    SET UPDYMD      = @P11,      " _
                    & "        UPDUSER     = @P12,      " _
                    & "        UPDTERMID   = @P13,      " _
                    & "        RECEIVEYMD  = @P14,      " _
                    & "        DELFLG      = '1'        " _
                    & "  WHERE CAMPCODE    = @P01       " _
                    & "    AND TORICODE    = @P02       " _
                    & "    AND SHIPORG     = @P03       " _
                    & "    AND ORDERNO     = @P04       " _
                    & "    AND GSHABAN     = @P05       " _
                    & "    AND DELFLG     <> '1'        "

            Dim SQLcmd As New SqlCommand(SQLStr, SQLcon)
            SQLcmd.CommandTimeout = 300

            Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", System.Data.SqlDbType.NVarChar)
            Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", System.Data.SqlDbType.NVarChar)
            Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@P03", System.Data.SqlDbType.NVarChar)
            Dim PARA04 As SqlParameter = SQLcmd.Parameters.Add("@P04", System.Data.SqlDbType.NVarChar)
            Dim PARA05 As SqlParameter = SQLcmd.Parameters.Add("@P05", System.Data.SqlDbType.NVarChar)

            Dim PARA11 As SqlParameter = SQLcmd.Parameters.Add("@P11", System.Data.SqlDbType.DateTime)
            Dim PARA12 As SqlParameter = SQLcmd.Parameters.Add("@P12", System.Data.SqlDbType.NVarChar)
            Dim PARA13 As SqlParameter = SQLcmd.Parameters.Add("@P13", System.Data.SqlDbType.NVarChar)
            Dim PARA14 As SqlParameter = SQLcmd.Parameters.Add("@P14", System.Data.SqlDbType.DateTime)

            Dim WW_TORICODE As String = ""
            Dim WW_SHIPORG As String = ""
            Dim WW_ORDERNO As String = ""
            Dim WW_GSHABAN As String = ""

            For Each T0005row In T0005tbl.Rows

                If T0005row("TORICODE") <> WW_TORICODE OrElse
                   T0005row("SHIPORG") <> WW_SHIPORG OrElse
                   T0005row("ORDERNO") <> WW_ORDERNO OrElse
                   T0005row("GSHABAN") <> WW_GSHABAN Then

                    PARA01.Value = T0005row("CAMPCODE")
                    PARA02.Value = T0005row("TORICODE")
                    PARA03.Value = T0005row("SHIPORG")
                    PARA04.Value = T0005row("ORDERNO")
                    PARA05.Value = T0005row("GSHABAN")

                    PARA11.Value = I_DATENOW
                    PARA12.Value = Master.USERID
                    PARA13.Value = Master.USERTERMID
                    PARA14.Value = C_DEFAULT_YMD

                    SQLcmd.ExecuteNonQuery()

                    'ブレイクキー退避
                    WW_TORICODE = T0005row("TORICODE")
                    WW_SHIPORG = T0005row("SHIPORG")
                    WW_ORDERNO = T0005row("ORDERNO")
                    WW_GSHABAN = T0005row("GSHABAN")

                End If

            Next

            'CLOSE
            SQLcmd.Dispose()
            SQLcmd = Nothing

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "T0015_SUPPLJISSKI(old) DEL")
            CS0011LOGWRITE.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:T0015_SUPPLJISSKI(old) DEL"
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

        Dim cnt As Integer = 0

        Try

            Using SQLcon As SqlConnection = CS0050SESSION.getConnection
                SQLcon.Open()       'DataBase接続(Open)

                Dim SQLStr As String =
                           " INSERT INTO T0015_SUPPLJISSKI              " _
                         & "             (CAMPCODE,                     " _
                         & "              ORDERNO,                      " _
                         & "              DETAILNO,                     " _
                         & "              TRIPNO,                       " _
                         & "              DROPNO,                       " _
                         & "              SEQ,                          " _
                         & "              ENTRYDATE,                    " _
                         & "              TORICODE,                     " _
                         & "              OILTYPE,                      " _
                         & "              SHUKODATE,                    " _
                         & "              KIKODATE,                     " _
                         & "              SHUKADATE,                    " _
                         & "              SHIPORG,                      " _
                         & "              SHUKABASHO,                   " _
                         & "              GSHABAN,                      " _
                         & "              RYOME,                        " _
                         & "              SHAFUKU,                      " _
                         & "              STAFFCODE,                    " _
                         & "              SUBSTAFFCODE,                 " _
                         & "              TODOKEDATE,                   " _
                         & "              TODOKECODE,                   " _
                         & "              PRODUCT1,                     " _
                         & "              PRODUCT2,                     " _
                         & "              CONTNO,                       " _
                         & "              JSURYO,                       " _
                         & "              JDAISU,                       " _
                         & "              REMARKS1,                     " _
                         & "              REMARKS2,                     " _
                         & "              REMARKS3,                     " _
                         & "              REMARKS4,                     " _
                         & "              REMARKS5,                     " _
                         & "              REMARKS6,                     " _
                         & "              DELFLG,                       " _
                         & "              INITYMD,                      " _
                         & "              UPDYMD,                       " _
                         & "              UPDUSER,                      " _
                         & "              UPDTERMID,                    " _
                         & "              RECEIVEYMD,                   " _
                         & "              KIJUNDATE,                    " _
                         & "              SHARYOTYPEF,                  " _
                         & "              TSHABANF,                     " _
                         & "              SHARYOTYPEB,                  " _
                         & "              TSHABANB,                     " _
                         & "              SHARYOTYPEB2,                 " _
                         & "              TSHABANB2,                    " _
                         & "              STANI,                        " _
                         & "              PRODUCTCODE,                  " _
                         & "              JISSEKIKBN)                   " _
                         & "      VALUES (@P01,@P02,@P03,@P04,@P05,@P06,@P07,@P08,@P09,@P10,     " _
                         & "              @P11,@P12,@P13,@P14,@P15,@P16,@P17,@P18,@P19,@P20,     " _
                         & "              @P21,@P22,@P23,@P24,@P25,@P26,@P27,@P28,@P29,@P30,     " _
                         & "              @P31,@P32,@P33,@P34,@P35,@P36,@P37,@P38,@P39,@P40,     " _
                         & "              @P41,@P42,@P43,@P44,@P45,@P46,@P47,@P48                " _
                         & "              );    "

                Dim SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", System.Data.SqlDbType.NVarChar, 10)
                Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@P03", System.Data.SqlDbType.NVarChar, 10)
                Dim PARA04 As SqlParameter = SQLcmd.Parameters.Add("@P04", System.Data.SqlDbType.NVarChar, 10)
                Dim PARA05 As SqlParameter = SQLcmd.Parameters.Add("@P05", System.Data.SqlDbType.NVarChar, 10)
                Dim PARA06 As SqlParameter = SQLcmd.Parameters.Add("@P06", System.Data.SqlDbType.NVarChar, 2)
                Dim PARA07 As SqlParameter = SQLcmd.Parameters.Add("@P07", System.Data.SqlDbType.NVarChar, 25)
                Dim PARA08 As SqlParameter = SQLcmd.Parameters.Add("@P08", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA09 As SqlParameter = SQLcmd.Parameters.Add("@P09", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA10 As SqlParameter = SQLcmd.Parameters.Add("@P10", System.Data.SqlDbType.DateTime)
                Dim PARA11 As SqlParameter = SQLcmd.Parameters.Add("@P11", System.Data.SqlDbType.DateTime)
                Dim PARA12 As SqlParameter = SQLcmd.Parameters.Add("@P12", System.Data.SqlDbType.DateTime)
                Dim PARA13 As SqlParameter = SQLcmd.Parameters.Add("@P13", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA14 As SqlParameter = SQLcmd.Parameters.Add("@P14", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA15 As SqlParameter = SQLcmd.Parameters.Add("@P15", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA16 As SqlParameter = SQLcmd.Parameters.Add("@P16", System.Data.SqlDbType.NVarChar, 1)
                Dim PARA17 As SqlParameter = SQLcmd.Parameters.Add("@P17", System.Data.SqlDbType.Decimal)
                Dim PARA18 As SqlParameter = SQLcmd.Parameters.Add("@P18", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA19 As SqlParameter = SQLcmd.Parameters.Add("@P19", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA20 As SqlParameter = SQLcmd.Parameters.Add("@P20", System.Data.SqlDbType.DateTime)
                Dim PARA21 As SqlParameter = SQLcmd.Parameters.Add("@P21", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA22 As SqlParameter = SQLcmd.Parameters.Add("@P22", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA23 As SqlParameter = SQLcmd.Parameters.Add("@P23", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA24 As SqlParameter = SQLcmd.Parameters.Add("@P24", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA25 As SqlParameter = SQLcmd.Parameters.Add("@P25", System.Data.SqlDbType.Decimal)
                Dim PARA26 As SqlParameter = SQLcmd.Parameters.Add("@P26", System.Data.SqlDbType.Int)
                Dim PARA27 As SqlParameter = SQLcmd.Parameters.Add("@P27", System.Data.SqlDbType.NVarChar, 50)
                Dim PARA28 As SqlParameter = SQLcmd.Parameters.Add("@P28", System.Data.SqlDbType.NVarChar, 50)
                Dim PARA29 As SqlParameter = SQLcmd.Parameters.Add("@P29", System.Data.SqlDbType.NVarChar, 50)
                Dim PARA30 As SqlParameter = SQLcmd.Parameters.Add("@P30", System.Data.SqlDbType.NVarChar, 50)
                Dim PARA31 As SqlParameter = SQLcmd.Parameters.Add("@P31", System.Data.SqlDbType.NVarChar, 50)
                Dim PARA32 As SqlParameter = SQLcmd.Parameters.Add("@P32", System.Data.SqlDbType.NVarChar, 50)
                Dim PARA33 As SqlParameter = SQLcmd.Parameters.Add("@P33", System.Data.SqlDbType.NVarChar, 1)
                Dim PARA34 As SqlParameter = SQLcmd.Parameters.Add("@P34", System.Data.SqlDbType.DateTime)
                Dim PARA35 As SqlParameter = SQLcmd.Parameters.Add("@P35", System.Data.SqlDbType.DateTime)
                Dim PARA36 As SqlParameter = SQLcmd.Parameters.Add("@P36", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA37 As SqlParameter = SQLcmd.Parameters.Add("@P37", System.Data.SqlDbType.NVarChar, 30)
                Dim PARA38 As SqlParameter = SQLcmd.Parameters.Add("@P38", System.Data.SqlDbType.DateTime)
                Dim PARA39 As SqlParameter = SQLcmd.Parameters.Add("@P39", System.Data.SqlDbType.DateTime)
                Dim PARA40 As SqlParameter = SQLcmd.Parameters.Add("@P40", System.Data.SqlDbType.NVarChar, 1)
                Dim PARA41 As SqlParameter = SQLcmd.Parameters.Add("@P41", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA42 As SqlParameter = SQLcmd.Parameters.Add("@P42", System.Data.SqlDbType.NVarChar, 1)
                Dim PARA43 As SqlParameter = SQLcmd.Parameters.Add("@P43", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA44 As SqlParameter = SQLcmd.Parameters.Add("@P44", System.Data.SqlDbType.NVarChar, 1)
                Dim PARA45 As SqlParameter = SQLcmd.Parameters.Add("@P45", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA46 As SqlParameter = SQLcmd.Parameters.Add("@P46", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA47 As SqlParameter = SQLcmd.Parameters.Add("@P47", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA48 As SqlParameter = SQLcmd.Parameters.Add("@P48", System.Data.SqlDbType.NVarChar, 1)

                For Each T0005row As DataRow In T0005tbl.Rows

                    '削除は対象外
                    If T0005row("DELFLG") = C_DELETE_FLG.DELETE AndAlso T0005row("TIMSTP") = "0" Then
                        Continue For
                    End If

                    PARA01.Value = T0005row("CAMPCODE")                           '会社コード(CAMPCODE)
                    PARA02.Value = T0005row("ORDERNO").PadLeft(7, "0")            '受注番号(ORDERNO)
                    PARA03.Value = T0005row("DETAILNO").PadLeft(3, "0")           '明細№(DETAILNO)
                    PARA04.Value = T0005row("TRIPNO").PadLeft(3, "0")             'トリップ(TRIPNO)
                    PARA05.Value = T0005row("DROPNO").PadLeft(3, "0")             'ドロップ(DROPNO)
                    PARA07.Value = I_DATENOW.ToString("yyyyMMddHHmmssfff")       'エントリー日時(ENTRYDATE)
                    PARA08.Value = T0005row("TORICODE")                           '取引先コード(TORICODE)
                    If T0005row("L1SHUKODATE") = "" Then                          '出庫日(SHUKODATE)
                        PARA10.Value = "2000/01/01"
                    Else
                        PARA10.Value = RTrim(T0005row("L1SHUKODATE"))
                    End If
                    If T0005row("KIKODATE") = "" Then                             '帰庫日(KIKODATE)
                        PARA11.Value = "2000/01/01"
                    Else
                        PARA11.Value = RTrim(T0005row("KIKODATE"))
                    End If
                    If T0005row("SHUKADATE") = "" Then                            '出荷日(SHUKADATE)
                        PARA12.Value = "2000/01/01"
                    Else
                        PARA12.Value = RTrim(T0005row("SHUKADATE"))
                    End If
                    PARA13.Value = T0005row("SHIPORG")                            '出荷部署(SHIPORG)
                    PARA14.Value = T0005row("SHUKABASHO")                         '出荷場所(SHUKABASHO)
                    PARA15.Value = T0005row("GSHABAN")                            '業務車番(GSHABAN)
                    PARA16.Value = "1"                                            '両目(RYOME)
                    If String.IsNullOrWhiteSpace(RTrim(T0005row("SHAFUKU"))) Then '車腹（積載量）(SHAFUKU)
                        PARA17.Value = 0.0
                    Else
                        PARA17.Value = CType(T0005row("SHAFUKU"), Double)
                    End If
                    PARA18.Value = T0005row("STAFFCODE")                          '乗務員コード(STAFFCODE)
                    PARA19.Value = T0005row("SUBSTAFFCODE")                       '副乗務員コード(SUBSTAFFCODE)
                    If RTrim(T0005row("TODOKEDATE")) = "" Then                    '届日(TODOKEDATE)
                        PARA20.Value = "2000/01/01"
                    Else
                        PARA20.Value = RTrim(T0005row("TODOKEDATE"))
                    End If
                    PARA21.Value = T0005row("TODOKECODE")                         '届先コード(TODOKECODE)
                    PARA24.Value = ""                                             'コンテナ番号(CONTNO)
                    PARA27.Value = ""                                             '備考１(REMARKS1)
                    PARA28.Value = ""                                             '備考２(REMARKS2)
                    PARA29.Value = ""                                             '備考３(REMARKS3)
                    PARA30.Value = ""                                             '備考４(REMARKS4)
                    PARA31.Value = ""                                             '備考５(REMARKS5)
                    PARA32.Value = ""                                             '備考６(REMARKS6)
                    PARA33.Value = C_DELETE_FLG.ALIVE                             '削除フラグ(DELFLG)
                    PARA34.Value = I_DATENOW                                      '登録年月日(INITYMD)
                    PARA35.Value = I_DATENOW                                      '更新年月日(UPDYMD)
                    PARA36.Value = Master.USERID                                  '更新ユーザＩＤ(UPDUSER)
                    PARA37.Value = Master.USERTERMID                              '更新端末(UPDTERMID)
                    PARA38.Value = C_DEFAULT_YMD                                  '集信日時(RECEIVEYMD)

                    '売上区分が１の場合、出荷日　２の場合、届日
                    If T0005row("URIKBN") = "1" Then
                        If RTrim(T0005row("SHUKADATE")) = "" Then
                            PARA39.Value = "2000/01/01"
                        Else
                            PARA39.Value = RTrim(T0005row("SHUKADATE"))               '基準日(KIJUNDATE)
                        End If
                    ElseIf T0005row("URIKBN") = "2" Then
                        If RTrim(T0005row("TODOKEDATE")) = "" Then
                            PARA39.Value = "2000/01/01"
                        Else
                            PARA39.Value = RTrim(T0005row("TODOKEDATE"))              '基準日(KIJUNDATE)
                        End If
                    Else
                        PARA39.Value = "2000/01/01"
                    End If
                    PARA40.Value = T0005row("SHARYOTYPEF")                        '統一車番前(SHARYOTYPEF)
                    PARA41.Value = T0005row("TSHABANF")                           '統一車番前(TSHABANF)
                    PARA42.Value = T0005row("SHARYOTYPEB")                        '統一車番後(SHARYOTYPEB)
                    PARA43.Value = T0005row("TSHABANB")                           '統一車番後(TSHABANB)
                    PARA44.Value = T0005row("SHARYOTYPEB2")                       '統一車番後2(SHARYOTYPEB2)
                    PARA45.Value = T0005row("TSHABANB2")                          '統一車番後2(TSHABANB2)
                    PARA48.Value = "2"                                            '実績区分(JISSEKIKBN)

                    For index = 1 To 8

                        Select Case index

                            Case 1

                                If String.IsNullOrEmpty(T0005row("OILTYPE1")) Then
                                    Continue For
                                End If

                                PARA06.Value = index.ToString("00")                           '枝番(SEQ)
                                PARA09.Value = T0005row("OILTYPE1")                           '油種(OILTYPE)
                                PARA22.Value = T0005row("PRODUCT11")                          '品名１(PRODUCT1)
                                PARA23.Value = T0005row("PRODUCT21")                          '品名２(PRODUCT2)

                                If String.IsNullOrWhiteSpace(RTrim(T0005row("SURYO1"))) Then
                                    PARA25.Value = 0.0
                                    PARA26.Value = 0
                                Else
                                    PARA25.Value = CType(T0005row("SURYO1"), Double)          '配送実績数量(JSURYO)
                                    PARA26.Value = 1                                          '配送実績台数(JDAISU)
                                End If

                                PARA46.Value = T0005row("STANI1")                             '配送実績単位(STANI)
                                PARA47.Value = T0005row("PRODUCTCODE1")                       '品名コード(PRODUCTCODE)

                            Case 2

                                If String.IsNullOrEmpty(T0005row("OILTYPE2")) Then
                                    Continue For
                                End If

                                PARA06.Value = index.ToString("00")                           '枝番(SEQ)
                                PARA09.Value = T0005row("OILTYPE2")                           '油種(OILTYPE)
                                PARA22.Value = T0005row("PRODUCT12")                          '品名１(PRODUCT1)
                                PARA23.Value = T0005row("PRODUCT22")                          '品名２(PRODUCT2)

                                If String.IsNullOrWhiteSpace(RTrim(T0005row("SURYO2"))) Then
                                    PARA25.Value = 0.0
                                    PARA26.Value = 0
                                Else
                                    PARA25.Value = CType(T0005row("SURYO2"), Double)          '配送実績数量(JSURYO)
                                    PARA26.Value = 1                                          '配送実績台数(JDAISU)
                                End If

                                PARA46.Value = T0005row("STANI2")                             '配送実績単位(STANI)
                                PARA47.Value = T0005row("PRODUCTCODE2")                       '品名コード(PRODUCTCODE)

                            Case 3

                                If String.IsNullOrEmpty(T0005row("OILTYPE3")) Then
                                    Continue For
                                End If

                                PARA06.Value = index.ToString("00")                           '枝番(SEQ)
                                PARA09.Value = T0005row("OILTYPE3")                           '油種(OILTYPE)
                                PARA22.Value = T0005row("PRODUCT13")                          '品名１(PRODUCT1)
                                PARA23.Value = T0005row("PRODUCT23")                          '品名２(PRODUCT2)

                                If String.IsNullOrWhiteSpace(RTrim(T0005row("SURYO3"))) Then
                                    PARA25.Value = 0.0
                                    PARA26.Value = 0
                                Else
                                    PARA25.Value = CType(T0005row("SURYO3"), Double)          '配送実績数量(JSURYO)
                                    PARA26.Value = 1                                          '配送実績台数(JDAISU)
                                End If

                                PARA46.Value = T0005row("STANI3")                             '配送実績単位(STANI)
                                PARA47.Value = T0005row("PRODUCTCODE3")                       '品名コード(PRODUCTCODE)

                            Case 4

                                If String.IsNullOrEmpty(T0005row("OILTYPE4")) Then
                                    Continue For
                                End If

                                PARA06.Value = index.ToString("00")                           '枝番(SEQ)
                                PARA09.Value = T0005row("OILTYPE4")                           '油種(OILTYPE)
                                PARA22.Value = T0005row("PRODUCT14")                          '品名１(PRODUCT1)
                                PARA23.Value = T0005row("PRODUCT24")                          '品名２(PRODUCT2)

                                If String.IsNullOrWhiteSpace(RTrim(T0005row("SURYO4"))) Then
                                    PARA25.Value = 0.0
                                    PARA26.Value = 0
                                Else
                                    PARA25.Value = CType(T0005row("SURYO4"), Double)          '配送実績数量(JSURYO)
                                    PARA26.Value = 1                                          '配送実績台数(JDAISU)
                                End If

                                PARA46.Value = T0005row("STANI4")                             '配送実績単位(STANI)
                                PARA47.Value = T0005row("PRODUCTCODE4")                       '品名コード(PRODUCTCODE)

                            Case 5

                                If String.IsNullOrEmpty(T0005row("OILTYPE5")) Then
                                    Continue For
                                End If

                                PARA06.Value = index.ToString("00")                           '枝番(SEQ)
                                PARA09.Value = T0005row("OILTYPE5")                           '油種(OILTYPE)
                                PARA22.Value = T0005row("PRODUCT15")                          '品名１(PRODUCT1)
                                PARA23.Value = T0005row("PRODUCT25")                          '品名２(PRODUCT2)

                                If String.IsNullOrWhiteSpace(RTrim(T0005row("SURYO5"))) Then
                                    PARA25.Value = 0.0
                                    PARA26.Value = 0
                                Else
                                    PARA25.Value = CType(T0005row("SURYO5"), Double)          '配送実績数量(JSURYO)
                                    PARA26.Value = 1                                          '配送実績台数(JDAISU)
                                End If

                                PARA46.Value = T0005row("STANI5")                             '配送実績単位(STANI)
                                PARA47.Value = T0005row("PRODUCTCODE5")                       '品名コード(PRODUCTCODE)

                            Case 6

                                If String.IsNullOrEmpty(T0005row("OILTYPE6")) Then
                                    Continue For
                                End If

                                PARA06.Value = index.ToString("00")                           '枝番(SEQ)
                                PARA09.Value = T0005row("OILTYPE6")                           '油種(OILTYPE)
                                PARA22.Value = T0005row("PRODUCT16")                          '品名１(PRODUCT1)
                                PARA23.Value = T0005row("PRODUCT26")                          '品名２(PRODUCT2)

                                If String.IsNullOrWhiteSpace(RTrim(T0005row("SURYO6"))) Then
                                    PARA25.Value = 0.0
                                    PARA26.Value = 0
                                Else
                                    PARA25.Value = CType(T0005row("SURYO6"), Double)          '配送実績数量(JSURYO)
                                    PARA26.Value = 1                                          '配送実績台数(JDAISU)
                                End If

                                PARA46.Value = T0005row("STANI6")                             '配送実績単位(STANI)
                                PARA47.Value = T0005row("PRODUCTCODE6")                       '品名コード(PRODUCTCODE)

                            Case 7

                                If String.IsNullOrEmpty(T0005row("OILTYPE7")) Then
                                    Continue For
                                End If

                                PARA06.Value = index.ToString("00")                           '枝番(SEQ)
                                PARA09.Value = T0005row("OILTYPE7")                           '油種(OILTYPE)
                                PARA22.Value = T0005row("PRODUCT17")                          '品名１(PRODUCT1)
                                PARA23.Value = T0005row("PRODUCT27")                          '品名２(PRODUCT2)

                                If String.IsNullOrWhiteSpace(RTrim(T0005row("SURYO7"))) Then
                                    PARA25.Value = 0.0
                                    PARA26.Value = 0
                                Else
                                    PARA25.Value = CType(T0005row("SURYO7"), Double)          '配送実績数量(JSURYO)
                                    PARA26.Value = 1                                          '配送実績台数(JDAISU)
                                End If

                                PARA46.Value = T0005row("STANI7")                             '配送実績単位(STANI)
                                PARA47.Value = T0005row("PRODUCTCODE7")                       '品名コード(PRODUCTCODE)

                            Case 8

                                If String.IsNullOrEmpty(T0005row("OILTYPE8")) Then
                                    Continue For
                                End If

                                PARA06.Value = index.ToString("00")                           '枝番(SEQ)
                                PARA09.Value = T0005row("OILTYPE8")                           '油種(OILTYPE)
                                PARA22.Value = T0005row("PRODUCT18")                          '品名１(PRODUCT1)
                                PARA23.Value = T0005row("PRODUCT28")                          '品名２(PRODUCT2)

                                If String.IsNullOrWhiteSpace(RTrim(T0005row("SURYO8"))) Then
                                    PARA25.Value = 0.0
                                    PARA26.Value = 0
                                Else
                                    PARA25.Value = CType(T0005row("SURYO8"), Double)          '配送実績数量(JSURYO)
                                    PARA26.Value = 1                                          '配送実績台数(JDAISU)
                                End If

                                PARA46.Value = T0005row("STANI8")                             '配送実績単位(STANI)
                                PARA47.Value = T0005row("PRODUCTCODE8")                       '品名コード(PRODUCTCODE)

                        End Select

                        SQLcmd.CommandTimeout = 300
                        SQLcmd.ExecuteNonQuery()

                    Next

                Next

                'CLOSE
                SQLcmd.Dispose()
                SQLcmd = Nothing

            End Using

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "T0015_SUPPLJISSKI INSERT")
            CS0011LOGWRITE.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:T0015_SUPPLJISSKI INSERT"      '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                 '
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力

            O_RTN = C_MESSAGE_NO.DB_ERROR
            Exit Sub

        End Try

    End Sub

    ''' <summary>
    ''' 一時保存ボタン処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonSAVE_Click()

        '■■■ セッション変数設定 ■■■
        '○入力値チェック
        Dim WW_CONVERT As String = ""
        Dim WW_TEXT As String = ""

        '○画面表示データ復元
        If Not Master.RecoverTable(T00015tbl) Then
            Exit Sub
        End If

        '一時保存ファイルに出力
        If Master.SaveTable(T00015tbl, work.WF_SEL_XMLsaveTmp.Text) = False Then
            Exit Sub
        End If

        '一時保存ファイルに条件パラメータ出力
        Dim T0015PARMtbl As DataTable = New DataTable
        work.PARMtbl_ColumnsAdd(T0015PARMtbl)

        Dim WW_T0015PARMrow As DataRow = T0015PARMtbl.NewRow

        WW_T0015PARMrow("LINECNT") = 1

        '会社コード　
        WW_T0015PARMrow("CAMPCODE") = work.WF_SEL_CAMPCODE.Text
        '出庫日
        WW_T0015PARMrow("SHUKODATEF") = work.WF_SEL_SHUKODATEF.Text
        WW_T0015PARMrow("SHUKODATET") = work.WF_SEL_SHUKODATET.Text
        '出荷日
        WW_T0015PARMrow("SHUKADATEF") = work.WF_SEL_SHUKADATEF.Text
        WW_T0015PARMrow("SHUKADATET") = work.WF_SEL_SHUKADATET.Text
        '届日　
        WW_T0015PARMrow("TODOKEDATEF") = work.WF_SEL_TODOKEDATEF.Text
        WW_T0015PARMrow("TODOKEDATET") = work.WF_SEL_TODOKEDATET.Text
        '出荷部署
        WW_T0015PARMrow("SHIPORG") = work.WF_SEL_SHIPORG.Text
        '油種
        WW_T0015PARMrow("OILTYPE") = work.WF_SEL_OILTYPE.Text

        T0015PARMtbl.Rows.Add(WW_T0015PARMrow)

        '条件（パラメタファイル）
        If Master.SaveTable(T0015PARMtbl, work.WF_SEL_XMLsavePARM.Text) = False Then
            Master.Output(C_MESSAGE_NO.FILE_IO_ERROR, C_MESSAGE_TYPE.ABORT)
            Exit Sub
        End If

        '○メッセージ表示
        Master.output(C_MESSAGE_NO.NORMAL, C_MESSAGE_TYPE.INF)

        '○カーソル設定
        WF_FIELD.Value = "WF_SELTORICODE"
        WF_STAFFCODE.Focus()

    End Sub


    ''' <summary>
    ''' 一覧絞り込みボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonExtract_Click()

        '○入力値チェック
        Dim WW_LINECNT As Integer

        '○画面表示データ復元
        Master.RecoverTable(T00015tbl)

        '○絞り込み操作（GridView明細Hidden設定）
        For Each row In T00015tbl.Rows

            '削除データは対象外
            If row("DELFLG") = C_DELETE_FLG.DELETE Then Continue For

            row("HIDDEN") = 1

            '行番号が相違の場合は絞込判定対象、同一の場合は非表示設定
            If row("LINECNT") <> WW_LINECNT Then
                WW_LINECNT = row("LINECNT")

                'オブジェクト　グループコード　絞込判定
                If (WF_SELTORICODE.Text = "") Then
                    row("HIDDEN") = 0
                End If

                If (WF_SELTORICODE.Text <> "") Then
                    If row("TORICODE") = WF_SELTORICODE.Text Then
                        row("HIDDEN") = 0
                    End If
                End If
            End If

        Next

        '○画面表示データ保存
        Master.SaveTable(T00015tbl)

        '画面先頭を表示
        WF_GridPosition.Text = "1"

        '○メッセージ表示
        Master.output(C_MESSAGE_NO.DATA_FILTER_SUCCESSFUL, C_MESSAGE_TYPE.INF)

        '○カーソル設定
        WF_FIELD.Value = "WF_SELTORICODE"
        WF_SELTORICODE.Focus()

    End Sub

    ''' <summary>
    ''' 新規ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonNEW_Click()

        Dim WW_INDEX As Integer = 0

        '■■■ Detailデータ設定 ■■■

        '一時Table(T00015INPtbl)準備
        Master.CreateEmptyTable(T00015INPtbl)

        WF_REP_LINECNT.Value = ""     '表示LINECNT（打変不可）

        Dim T00015INProw As DataRow
        '空行を4件作成
        For i As Integer = 1 To 4
            T00015INProw = T00015INPtbl.NewRow()
            T00015INProw("LINECNT") = 0
            T00015INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA
            T00015INProw("TIMSTP") = 0
            T00015INProw("SELECT") = 1
            T00015INProw("HIDDEN") = 0
            T00015INProw("INDEX") = WW_INDEX

            T00015INProw("CAMPCODE") = work.WF_SEL_CAMPCODE.Text
            T00015INProw("CAMPCODENAME") = ""
            T00015INProw("ORDERNO") = ""
            T00015INProw("DETAILNO") = ""
            T00015INProw("TRIPNO") = ""
            T00015INProw("DROPNO") = ""
            T00015INProw("SEQ") = ""
            T00015INProw("TORICODE") = ""
            T00015INProw("TORICODENAME") = ""
            T00015INProw("OILTYPE") = work.WF_SEL_OILTYPE.Text
            T00015INProw("OILTYPENAME") = ""
            T00015INProw("SHUKODATE") = work.WF_SEL_SHUKODATEF.Text  '出庫日
            T00015INProw("KIKODATE") = ""
            T00015INProw("KIJUNDATE") = ""
            T00015INProw("SHUKADATE") = work.WF_SEL_SHUKADATEF.Text  '出荷日
            T00015INProw("SHIPORG") = work.WF_SEL_SHIPORG.Text
            T00015INProw("SHIPORGNAME") = ""
            T00015INProw("SHUKABASHO") = ""
            T00015INProw("SHUKABASHONAME") = ""
            T00015INProw("GSHABAN") = ""
            T00015INProw("GSHABANLICNPLTNO") = ""
            T00015INProw("RYOME") = "1"
            T00015INProw("SHAFUKU") = ""
            T00015INProw("STAFFCODE") = ""
            T00015INProw("STAFFCODENAME") = ""
            T00015INProw("SUBSTAFFCODE") = ""
            T00015INProw("SUBSTAFFCODENAME") = ""
            T00015INProw("TODOKEDATE") = work.WF_SEL_TODOKEDATEF.Text
            T00015INProw("TODOKECODE") = ""
            T00015INProw("TODOKECODENAME") = ""
            T00015INProw("PRODUCT1") = ""
            T00015INProw("PRODUCT1NAME") = ""
            T00015INProw("PRODUCT2") = ""
            T00015INProw("PRODUCT2NAME") = ""
            T00015INProw("PRODUCTCODE") = ""
            T00015INProw("PRODUCTNAME") = ""
            T00015INProw("CONTNO") = ""
            T00015INProw("JSURYO") = ""
            T00015INProw("JSURYO_SUM") = ""
            T00015INProw("JDAISU") = ""
            T00015INProw("JDAISU_SUM") = ""
            T00015INProw("REMARKS1") = ""
            T00015INProw("REMARKS2") = ""
            T00015INProw("REMARKS3") = ""
            T00015INProw("REMARKS4") = ""
            T00015INProw("REMARKS5") = ""
            T00015INProw("REMARKS6") = ""
            T00015INProw("SHARYOTYPEF") = ""
            T00015INProw("TSHABANF") = ""
            T00015INProw("SHARYOTYPEB") = ""
            T00015INProw("TSHABANB") = ""
            T00015INProw("SHARYOTYPEB2") = ""
            T00015INProw("TSHABANB2") = ""
            T00015INProw("JISSEKIKBN") = ""
            T00015INProw("DELFLG") = ""
            T00015INProw("ADDR") = ""
            T00015INProw("NOTES1") = ""
            T00015INProw("NOTES2") = ""
            T00015INProw("NOTES3") = ""
            T00015INProw("NOTES4") = ""
            T00015INProw("NOTES5") = ""
            T00015INProw("NOTES6") = ""
            T00015INProw("NOTES7") = ""
            T00015INProw("NOTES8") = ""
            T00015INProw("NOTES9") = ""
            T00015INProw("NOTES10") = ""
            T00015INProw("SHARYOINFO1") = ""
            T00015INProw("SHARYOINFO2") = ""
            T00015INProw("SHARYOINFO3") = ""
            T00015INProw("SHARYOINFO4") = ""
            T00015INProw("SHARYOINFO5") = ""
            T00015INProw("SHARYOINFO6") = ""
            T00015INProw("STAFFNOTES1") = ""
            T00015INProw("STAFFNOTES2") = ""
            T00015INProw("STAFFNOTES3") = ""
            T00015INProw("STAFFNOTES4") = ""
            T00015INProw("STAFFNOTES5") = ""

            T00015INProw("WORK_NO") = ""

            T00015INPtbl.Rows.Add(T00015INProw)


            WW_INDEX = WW_INDEX + 1
        Next

        'ヘッダ初期表示
        Dim WW_TEXT As String = ""
        For Each INProw In T00015INPtbl.Rows

            '出庫日
            WF_SHUKODATE.Text = INProw("SHUKODATE")
            '帰庫日
            WF_KIKODATE.Text = INProw("KIKODATE")
            '両目
            WF_RYOME.Text = INProw("RYOME")

            '出荷日
            WF_SHUKADATE.Text = INProw("SHUKADATE")
            '届日
            WF_TODOKEDATE.Text = INProw("TODOKEDATE")

            '油種
            WF_OILTYPE.Text = INProw("OILTYPE")
            CODENAME_get("OILTYPE", WF_OILTYPE.Text, WW_TEXT, WW_DUMMY)
            WF_OILTYPE_TEXT.Text = WW_TEXT

            '荷主
            WF_TORICODE.Text = INProw("TORICODE")
            CODENAME_get("TORICODE", WF_TORICODE.Text, WW_TEXT, WW_DUMMY)
            WF_TORICODE_TEXT.Text = WW_TEXT

            '出荷組織
            WF_SHIPORG.Text = INProw("SHIPORG")
            CODENAME_get("SHIPORG", WF_SHIPORG.Text, WW_TEXT, WW_DUMMY)
            WF_SHIPORG_TEXT.Text = WW_TEXT

            '業務車番
            WF_GSHABAN.Text = INProw("GSHABAN")

            '車腹
            WF_SHAFUKU.Text = INProw("SHAFUKU")

            'トリップ
            WF_TRIPNO.Text = INProw("TRIPNO")

            'ドロップ
            WF_DROPNO.Text = INProw("DROPNO")

            '乗務員
            WF_STAFFCODE.Text = INProw("STAFFCODE")
            CODENAME_get("STAFFCODE", WF_STAFFCODE.Text, WW_TEXT, WW_DUMMY)
            WF_STAFFCODE_TEXT.Text = WW_TEXT

            '副乗務員
            WF_SUBSTAFFCODE.Text = INProw("SUBSTAFFCODE")
            CODENAME_get("SUBSTAFFCODE", WF_SUBSTAFFCODE.Text, WW_TEXT, WW_DUMMY)
            WF_SUBSTAFFCODE_TEXT.Text = WW_TEXT

            '実績区分
            WF_JISSEKIKBN.Text = INProw("JISSEKIKBN")

            Exit For
        Next

        '○Detail初期設定
        Repeater_INIT()

        'leftBOXキャンセルボタン処理
        WF_ButtonCan_Click()

        'close
        WF_IsHideDetailBox.Value = "0"

        WF_Sel_LINECNT.Enabled = True
        WF_SHUKODATE.Enabled = True
        WF_SHUKADATE.Enabled = True
        WF_TODOKEDATE.Enabled = True
        WF_KIKODATE.Enabled = True
        WF_RYOME.Enabled = True
        WF_ORDERNO.Enabled = True
        WF_DETAILNO.Enabled = True
        WF_SHIPORG.Enabled = True
        WF_TORICODE.Enabled = True
        WF_OILTYPE.Enabled = True
        WF_GSHABAN.Enabled = True
        WF_TSHABANF.Enabled = True
        WF_TSHABANB.Enabled = True
        WF_TSHABANB2.Enabled = True
        WF_SHAFUKU.Enabled = True
        WF_TRIPNO.Enabled = True
        WF_DROPNO.Enabled = True
        WF_JISSEKIKBN.Enabled = True

        'カーソル設定
        WF_FIELD.Value = "WF_SHUKODATE"
        WF_SHUKODATE.Focus()

    End Sub


    ''' <summary>
    ''' DB更新ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonUPDATE_Click()

        '○画面表示データ復元
        Master.RecoverTable(T00015tbl)


        '■■■ DB更新 ■■■

        Dim WW_DATENOW As Date = Date.Now
        Dim SQLcon = CS0050SESSION.getConnection
        SQLcon.Open()

        '' L1統計DB
        'Dim cL1TOKEI As L1TOKEI = New L1TOKEI(SQLcon, Master.USERID, Master.USERTERMID)

        ' ***  T00015UPDtbl更新データ（画面表示受注+画面非表示受注）作成　＆　タイムスタンプチェック処理
        DBupdate_T00015UPDtblget(WW_DUMMY)

        '' ***  L0001_TOKEIテーブル編集（T00015UPDtblより）
        'cL1TOKEI.Edit(T00015UPDtbl, WW_ERRCODE)
        'If Not isNormal(WW_ERRCODE) Then
        '    Master.output(WW_ERRCODE, C_MESSAGE_TYPE.ABORT, "伝票番号採番")
        'End If

        ' ***  T00015tbl関連データ削除
        DBupdate_T15DELETE(WW_DATENOW, WW_ERRCODE)

        ' ***  T00015tbl追加
        DBupdate_T15INSERT(WW_DATENOW, WW_ERRCODE)

        '' ***  L1追加
        'cL1TOKEI.Update(T00015UPDtbl, WW_ERRCODE)
        'If Not isNormal(WW_ERRCODE) Then
        '    Master.output(WW_ERRCODE, C_MESSAGE_TYPE.ABORT, "統計DB追加")
        'End If

        'サマリ処理
        SUMMRY_SET()

        '○画面表示データ保存
        Master.SaveTable(T00015tbl)
        '○メッセージ表示
        Master.output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF)

        'カーソル設定
        WF_FIELD.Value = "WF_SELTORICODE"
        WF_SELTORICODE.Focus()

        WF_ButtonALLSELECT.Checked = False

    End Sub

    ''' <summary>
    ''' 一覧印刷ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_Print_Click()

        '○画面表示データ復元
        Master.RecoverTable(T00015tbl)

        '帳票出力dll Interface
        CS0030REPORT.CAMPCODE = work.WF_SEL_CAMPCODE.Text           '会社コード
        CS0030REPORT.PROFID = Master.PROF_REPORT                    'プロファイルID
        CS0030REPORT.MAPID = Master.MAPID                           'PARAM01:画面ID
        CS0030REPORT.REPORTID = rightview.getReportId()             'PARAM02:帳票ID
        CS0030REPORT.FILEtyp = "pdf"                                'PARAM03:出力ファイル形式
        CS0030REPORT.TBLDATA = T00015tbl                            'PARAM04:データ参照tabledata
        CS0030REPORT.CS0030REPORT()

        If isNormal(CS0030REPORT.ERR) Then
        Else
            If CS0030REPORT.ERR = C_MESSAGE_NO.REPORT_EXCEL_NOT_FOUND_ERROR Then
                Master.output(CS0030REPORT.ERR, C_MESSAGE_TYPE.ERR)
            Else
                Master.output(CS0030REPORT.ERR, C_MESSAGE_TYPE.ABORT, "CS0022REPORT")
            End If
            Exit Sub
        End If

        '別画面でPDFを表示
        WF_PrintURL.Value = CS0030REPORT.URL
        ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_PDFPrint()", True)

    End Sub

    ''' <summary>
    ''' ダウンロードボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonCSV_Click()

        '○画面表示データ復元
        Master.RecoverTable(T00015tbl)

        '削除データを除外
        CS0026TBLSORTget.TABLE = T00015tbl
        CS0026TBLSORTget.SORTING = "LINECNT ASC , SEQ ASC"
        CS0026TBLSORTget.FILTER = "DELFLG <> '1'"
        CS0026TBLSORTget.Sort(T00015tbl)

        '○ 帳票出力dll Interface
        CS0030REPORT.CAMPCODE = work.WF_SEL_CAMPCODE.Text           '会社コード
        CS0030REPORT.PROFID = Master.PROF_REPORT                    'プロファイルID
        CS0030REPORT.MAPID = Master.MAPID                           'PARAM01:画面ID
        CS0030REPORT.REPORTID = rightview.getReportId()             'PARAM02:帳票ID
        CS0030REPORT.FILEtyp = "XLSX"                               'PARAM03:出力ファイル形式
        CS0030REPORT.TBLDATA = T00015tbl                            'PARAM04:データ参照tabledata
        CS0030REPORT.CS0030REPORT()

        If isNormal(CS0030REPORT.ERR) Then
        Else
            Master.output(CS0030REPORT.ERR, C_MESSAGE_TYPE.ABORT, "CS0030REPORT")
            Exit Sub
        End If

        '○帳票部署データリスト追加
        Dim addReport As AddReportOrgData = New AddReportOrgData() With {
            .CAMPCODE = work.WF_SEL_CAMPCODE.Text,
            .UORG = work.WF_SEL_SHIPORG.Text,
            .ROLECODE = Master.ROLE_ORG,
            .FILEPATH = CS0030REPORT.FILEpath,
            .SHEETNAME = "リスト"
        }
        addReport.AddOrgData()
        If isNormal(addReport.ERR) Then
        Else
            'エラーでも継続
            Master.Output(addReport.ERR, C_MESSAGE_TYPE.ABORT, "AddReport")
        End If

        '別画面でExcelを表示
        WF_PrintURL.Value = CS0030REPORT.URL
        ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_ExcelPrint()", True)

    End Sub


    ''' <summary>
    ''' 全選択ボタン処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonALLSELECT_Click()

        '○画面表示データ復元
        Master.RecoverTable(T00015tbl)

        Dim checked As String = If(WF_ButtonALLSELECT.Checked, "1", "0")
        '全チェックボックスON/OFF
        For Each row In T00015tbl.Rows
            If row("HIDDEN") = 0 Then
                row("ROWDEL") = checked
            End If
        Next

        '○画面表示データ保存
        Master.SaveTable(T00015tbl)

        '画面先頭を表示
        WF_GridPosition.Text = "1"

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

        '○画面表示データ復元
        Master.RecoverTable(T00015tbl)

        '○先頭頁に移動
        WF_GridPosition.Text = "1"
    End Sub
    ''' <summary>
    ''' 最終頁ボタン処理 
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonLAST_Click()

        '○画面表示データ復元
        Master.RecoverTable(T00015tbl)

        '○ソート
        Dim WW_TBLview As DataView
        WW_TBLview = New DataView(T00015tbl)
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

        '○画面表示データ復元
        Master.RecoverTable(T00015tbl)
    End Sub


    ''' <summary>
    ''' 詳細画面-表更新ボタン処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_UPDATE_Click()
        Dim WW_ERRWORD As String

        '〇エラーレポート準備
        rightview.setErrorReport("")
        '○画面表示データ復元
        Master.RecoverTable(T00015tbl)

        '●DetailBoxをT00015INPtblへ退避 …　画面Detail --> T15INPtbl
        DetailBoxToINP()

        '前回OPERATIONの設定
        For Each T00015INProw In T00015INPtbl.Rows
            For j As Integer = 0 To T00015tbl.Rows.Count - 1
                If T00015tbl.Rows(j)("LINECNT") = T00015INProw("LINECNT") Then
                    EditOperationText(T00015tbl.Rows(j), False, T00015INProw)
                    Exit For
                End If
            Next
        Next

        WW_ERRLIST = New List(Of String)

        '■■■ 項目チェック ■■■        …　チェック結果：エラーコード（WW_ERR）
        '●チェック処理
        INPtbl_CHEK(WW_ERRCODE)

        '●関連チェック処理
        INPtbl_CHEK_DATE(WW_ERRCODE)

        '■■■ 変更有無チェック ■■■
        '    Grid画面明細：T00015INProw("WORK_NO")、T00015INProw("LINECNT")、T00015INProw("ORDERNO")クリア
        '　　変更発生　　：T00015INProw("OPERATION")へ"更新"or"エラー"を設定

        '●変更有無取得
        Dim WW_Change As String = ""
        Dim WW_GridNew As String = ""
        For CNT As Integer = 0 To T00015INPtbl.Rows.Count - 1

            Dim T00015INProw = T00015INPtbl.Rows(CNT)

            '○変更有無判定
            WW_Change = ""

            If T00015INProw("WORK_NO") = "" AndAlso Val(T00015INProw("JSURYO")) = 0 AndAlso Val(T00015INProw("JDAISU")) = 0 Then Continue For

            '変更またはエラーの場合、"有"とする
            If T00015INProw("OPERATION").ToString.Contains(C_LIST_OPERATION_CODE.UPDATING) OrElse
                T00015INProw("OPERATION").ToString.Contains(C_LIST_OPERATION_CODE.WARNING) OrElse
                T00015INProw("OPERATION").ToString.Contains(C_LIST_OPERATION_CODE.ERRORED) Then
                WW_Change = "有"
            Else
                '明細が消された場合は"有"とする
                If Val(T00015INProw("JSURYO")) = 0 AndAlso Val(T00015INProw("JDAISU")) = 0 AndAlso T00015INProw("WORK_NO") <> "" Then
                    WW_Change = "有"
                Else
                    If WF_REP_Change.Value = "" Then
                        '空更新
                        WW_Change = "有"
                    Else
                        WW_Change = "有"
                    End If
                End If
            End If


            '比較対象がなければ"新"とする
            If T00015tbl.Rows.Count = 0 Then
                WW_Change = "有"
                T00015INProw("WORK_NO") = ""
                T00015INProw("LINECNT") = 0
                T00015INProw("ORDERNO") = C_LIST_OPERATION_CODE.NODATA
            End If

            '①詳細画面で追記された場合は"新"とする
            If (Val(T00015INProw("JSURYO")) <> 0 Or Val(T00015INProw("JDAISU")) <> 0) And T00015INProw("WORK_NO") = "" Then
                WW_Change = "有"
                T00015INProw("WORK_NO") = ""
                T00015INProw("LINECNT") = 0
                T00015INProw("ORDERNO") = ""
            End If

            '②詳細画面で、行番号クリア操作時（参照コピー）。
            If WF_Sel_LINECNT.Text = "" Then
                WW_Change = "有"
                T00015INProw("WORK_NO") = ""
                T00015INProw("LINECNT") = 0
                T00015INProw("ORDERNO") = ""
            End If

            '③詳細画面で、画面表示単位（受注番号要素）が変更された場合。
            If WF_REP_Change.Value = "1" Then
                WW_Change = "有"
            End If
            '④詳細画面で、画面表示単位（配送明細要素）が変更された場合。
            If WF_REP_Change.Value = "2" Then
                WW_Change = "有"
            End If

            'エラーは設定しない
            If T00015INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA And (WW_Change = "有" Or WW_Change = "新") Then
                T00015INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
            End If

        Next

        '●チェック処理2       …　新規登録（T00015INProw("WORK_NO") = ""）でT15tblに存在した場合、エラー。
        For CNT As Integer = 0 To T00015INPtbl.Rows.Count - 1

            Dim T00015INProw = T00015INPtbl.Rows(CNT)

            If T00015INProw("WORK_NO") = "" AndAlso Val(T00015INProw("JSURYO")) = 0 AndAlso Val(T00015INProw("JDAISU")) = 0 Then Continue For

            If T00015INProw("WORK_NO") = "" Then        '新規明細の場合
                For j As Integer = 0 To T00015tbl.Rows.Count - 1

                    '自明細以外　かつ　取引先、油種、基準日（出荷日or届日）、受注部署、出荷部署、出庫日、業務車番、両目、トリップ、ドロップが同一
                    If Val(T00015tbl.Rows(j)("LINECNT")) <> Val(WF_Sel_LINECNT.Text) And
                       T00015tbl.Rows(j)("SHIPORG") = T00015INProw("SHIPORG") And
                       T00015tbl.Rows(j)("SHUKODATE") = T00015INProw("SHUKODATE") And
                       T00015tbl.Rows(j)("GSHABAN") = T00015INProw("GSHABAN") And
                       T00015tbl.Rows(j)("TRIPNO") = T00015INProw("TRIPNO") And
                       T00015tbl.Rows(j)("DROPNO") = T00015INProw("DROPNO") And
                       T00015tbl.Rows(j)("DELFLG") <> "1" And
                       T00015INProw("DELFLG") <> "1" Then

                        Dim WW_ERR_MES As StringBuilder = New StringBuilder()
                        WW_ERR_MES.AppendLine("・更新できないレコード(同一受注)です。")
                        WW_ERR_MES.AppendLine("  --> " & " 同一条件の配車が既に存在します。 , ")
                        WW_ERR_MES.AppendLine("  --> " & " （業務車番、トリップ、ドロップが同一） ")
                        WW_ERR_MES.AppendLine("  --> 項番　　= " & T00015INProw("LINECNT").ToString() & " , ")
                        WW_ERR_MES.AppendLine("  --> 明細番号= " & CNT.ToString("000") & " , ")
                        WW_ERR_MES.AppendLine("  --> 取引先　=" & T00015INProw("TORICODE") & " , ")
                        WW_ERR_MES.AppendLine("  --> 届先　　=" & T00015INProw("TODOKECODE") & " , ")
                        WW_ERR_MES.AppendLine("  --> 出荷場所=" & T00015INProw("SHUKABASHO") & " , ")
                        WW_ERR_MES.AppendLine("  --> 出庫日　=" & T00015INProw("SHUKODATE") & " , ")
                        WW_ERR_MES.AppendLine("  --> 届日　　=" & T00015INProw("TODOKEDATE") & " , ")
                        WW_ERR_MES.AppendLine("  --> 出荷日　=" & T00015INProw("SHUKADATE") & " , ")
                        WW_ERR_MES.AppendLine("  --> 車番　　=" & T00015INProw("GSHABAN") & " , ")
                        WW_ERR_MES.AppendLine("  --> 乗務員　=" & T00015INProw("STAFFCODE") & " , ")
                        WW_ERR_MES.AppendLine("  --> 品名  　=" & T00015INProw("PRODUCTCODE") & " , ")
                        WW_ERR_MES.AppendLine("  --> ﾄﾘｯﾌﾟ 　=" & T00015INProw("TRIPNO") & " , ")
                        WW_ERR_MES.AppendLine("  --> ﾄﾞﾛｯﾌﾟ　=" & T00015INProw("DROPNO") & " , ")
                        WW_ERR_MES.AppendLine("  --> 削除　　=" & T00015INProw("DELFLG") & " ")
                        rightview.AddErrorReport(WW_ERR_MES.ToString)

                        Master.Output(C_MESSAGE_NO.BOX_ERROR_EXIST, C_MESSAGE_TYPE.ERR)

                        'エラーメッセージ内の項番、明細番号置き換え
                        WW_ERRWORD = rightview.GetErrorReport()
                        For i As Integer = 0 To T00015INPtbl.Rows.Count - 1
                            '項番
                            WW_ERRWORD = WW_ERRWORD.Replace("@L" & i.ToString("0000") & "L@", Val(WF_REP_LINECNT.Value).ToString)
                            '明細番号
                            WW_ERRWORD = WW_ERRWORD.Replace("@D" & i.ToString("000") & "D@", (i + 1).ToString)
                        Next
                        rightview.SetErrorReport(WW_ERRWORD)

                        WW_ERRCODE = C_MESSAGE_NO.BOX_ERROR_EXIST
                        T00015INProw("OPERATION") = C_LIST_OPERATION_CODE.ERRORED

                    End If
                Next
            End If

        Next

        '●重大エラー時の処理
        If WW_ERRCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR Then
            'エラー箇所置換
            WW_ERRWORD = rightview.GetErrorReport()
            For i As Integer = 0 To T00015INPtbl.Rows.Count - 1
                '項番
                WW_ERRWORD = WW_ERRWORD.Replace("@L" & i.ToString("0000") & "L@", T00015INPtbl.Rows(i)("LINECNT"))
                '明細番号
                WW_ERRWORD = WW_ERRWORD.Replace("@D" & i.ToString("000") & "D@", (i + 1).ToString())
            Next
            rightview.SetErrorReport(WW_ERRWORD)

            'メッセージ表示
            Master.Output(WW_ERRCODE, C_MESSAGE_TYPE.ERR)

            Exit Sub
        End If

        '■■■ 更新前処理（入力情報へ受注番号設定、Grid画面の同一行情報を削除）　■■■
        For i As Integer = 0 To T00015INPtbl.Rows.Count - 1

            Dim T00015INProw = T00015INPtbl.Rows(i)

            If T00015INProw("WORK_NO") = "" AndAlso Val(T00015INProw("JSURYO")) = 0 AndAlso Val(T00015INProw("JDAISU")) = 0 Then Continue For

            For j As Integer = 0 To T00015tbl.Rows.Count - 1

                '状態をクリア設定
                EditOperationText(T00015tbl.Rows(j), False)

                If T00015INProw("OPERATION") <> C_LIST_OPERATION_CODE.NODATA Then

                    'Grid画面行追加の場合は受注番号を取得
                    If T00015tbl.Rows(j)("TORICODE") = T00015INProw("TORICODE") And
                       T00015tbl.Rows(j)("OILTYPE") = T00015INProw("OILTYPE") And
                       T00015tbl.Rows(j)("KIJUNDATE") = T00015INProw("KIJUNDATE") And
                       T00015tbl.Rows(j)("SHIPORG") = T00015INProw("SHIPORG") Then

                        T00015INProw("ORDERNO") = T00015tbl.Rows(j)("ORDERNO")
                        T00015INProw("DETAILNO") = "000"

                    End If


                    '同一行情報を論理削除（T15実態が存在する場合、物理削除。）
                    If WF_Sel_LINECNT.Text <> "" And T00015tbl.Rows(j)("LINECNT") = Val(WF_REP_LINECNT.Value) And
                       Val(WF_REP_LINECNT.Value) <> 0 Then

                        T00015tbl.Rows(j)("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                        T00015tbl.Rows(j)("DELFLG") = "1"   '削除
                        T00015tbl.Rows(j)("HIDDEN") = "1"   '非表示
                        T00015tbl.Rows(j)("SELECT") = "0"   '明細表示対象外

                    End If

                End If

            Next
        Next

        'T00015tblの削除データを物理削除
        CS0026TBLSORTget.TABLE = T00015tbl
        CS0026TBLSORTget.SORTING = "LINECNT ASC , SEQ ASC"
        CS0026TBLSORTget.FILTER = "DELFLG <> '1' or TIMSTP <> 0 or (DELFLG = '1' and HIDDEN = '0')"
        CS0026TBLSORTget.Sort(T00015tbl)

        '■■■ 更新前処理（入力情報へ操作を反映）　■■■
        INPtbl_PreUpdate1()

        '■■■ 更新前処理（入力情報へLINECNTを付番）　■■■
        INPtbl_PreUpdate2()

        '■■■ 更新前処理（入力情報へ暫定受注番号を付番）　■■■
        INPtbl_PreUpdate3()

        '■■■ GridView更新 ■■■
        ' 状態クリア
        EditOperationText(T00015tbl, False)

        '○サマリ処理 
        CS0026TBLSORTget.TABLE = T00015tbl
        CS0026TBLSORTget.SORTING = "LINECNT ASC , SEQ ASC"
        CS0026TBLSORTget.FILTER = ""
        CS0026TBLSORTget.Sort(T00015tbl)
        SUMMRY_SET()

        'エラーメッセージ内の項番、明細番号置き換え
        WW_ERRWORD = rightview.GetErrorReport()
        For i As Integer = 0 To T00015INPtbl.Rows.Count - 1
            '項番
            WW_ERRWORD = WW_ERRWORD.Replace("@L" & i.ToString("0000") & "L@", T00015INPtbl.Rows(i)("LINECNT"))
            '明細番号
            WW_ERRWORD = WW_ERRWORD.Replace("@D" & i.ToString("000") & "D@", T00015INPtbl.Rows(i)("SEQ"))
        Next
        rightview.SetErrorReport(WW_ERRWORD)

        '○画面表示データ保存
        Master.SaveTable(T00015tbl)

        '○Detailクリア
        'detailboxヘッダークリア
        ClearDetailBox()

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
        T00015INPtbl.Clear()

        'カーソル設定
        WF_FIELD.Value = "WF_SELTORICODE"
        WF_SELTORICODE.Focus()

        '○Close
        WF_DViewRep1.Visible = False
        WF_DViewRep1.Dispose()
        WF_DViewRep1 = Nothing

    End Sub

    ''' <summary>
    ''' detailbox クリアボタン処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_CLEAR_Click()

        '○detailboxヘッダークリア
        ClearDetailBox()

        '■■■ Detailデータ設定 ■■■
        '新規ボタン処理
        WF_ButtonNEW_Click()

        'メッセージ表示
        Master.Output(C_MESSAGE_NO.DATA_CLEAR_SUCCESSFUL, C_MESSAGE_TYPE.INF)

        'カーソル設定
        WF_FIELD.Value = "WF_SHUKODATE"
        WF_SHUKODATE.Focus()

    End Sub

    ''' <summary>
    ''' 詳細画面-戻るボタン処理  
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_BACK_Click()

        '○画面表示データ復元
        Master.RecoverTable(T00015tbl)

        '選択状態クリア
        EditOperationText(T00015tbl, False)

        '○ 画面表示データ保存
        Master.SaveTable(T00015tbl)

        '○detailboxクリア
        ClearDetailBox()

        '■■■ Detailデータ設定 ■■■
        'カーソル設定
        WF_FIELD.Value = "WF_SELTORICODE"
        WF_SELTORICODE.Focus()

        '画面切替設定
        WF_IsHideDetailBox.Value = "1"

        'leftBOXキャンセルボタン処理
        WF_ButtonCan_Click()

        'close
        pnlListArea.Visible = True
        WF_DViewRep1.Visible = False
        WF_DViewRep1.Dispose()
        WF_DViewRep1 = Nothing

    End Sub

    ' ******************************************************************************
    ' ***  共通処理                                                              ***　
    ' ******************************************************************************

    ''' <summary>
    ''' GridViewサマリ処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub SUMMRY_SET()

        Dim JSURYO_SUM As Decimal = 0
        Dim JDAISU_SUM As Long = 0

        CS0026TBLSORTget.TABLE = T00015tbl
        CS0026TBLSORTget.SORTING = "TORICODE ,OILTYPE ,KIJUNDATE ,SHIPORG ,SHUKODATE ,GSHABAN ,RYOME ,TRIPNO ,DROPNO ,SEQ"
        CS0026TBLSORTget.FILTER = ""
        CS0026TBLSORTget.Sort(T00015tbl)

        '最終行から初回行へループ
        For i As Integer = 0 To T00015tbl.Rows.Count - 1

            Dim T00015row = T00015tbl.Rows(i)

            If T00015row("SEQ") = "01" And T00015row("HIDDEN") <> "1" Then
                JSURYO_SUM = 0
                JDAISU_SUM = 0

                Dim findSeq As Boolean = False
                For j As Integer = i To T00015tbl.Rows.Count - 1
                    If CompareOrder(T00015row, T00015tbl.Rows(j)) Then
                        If T00015tbl.Rows(j)("DELFLG") <> C_DELETE_FLG.DELETE Then
                            '同一トリップが発生したら２件目以降は非表示
                            If findSeq = True Then
                                T00015tbl.Rows(j)("HIDDEN") = "1"
                            ElseIf T00015tbl.Rows(j)("SEQ") = "01" Then
                                findSeq = True
                            End If
                            Dim wkVal As Double
                            If Double.TryParse(T00015tbl.Rows(j)("JSURYO"), wkVal) Then
                                JSURYO_SUM += wkVal
                            End If

                            JDAISU_SUM = 1
                        End If
                    Else
                        Exit For
                    End If

                Next

                '表示行にサマリ結果を反映
                T00015row("JSURYO_SUM") = JSURYO_SUM.ToString("0.000")
                T00015row("JDAISU_SUM") = JDAISU_SUM.ToString("0")
                T00015row("HIDDEN") = 0   '0:表示

            Else
                T00015row("HIDDEN") = 1   '1:非表示
            End If

        Next

    End Sub


    ''' <summary>
    ''' LeftBox項目名称設定
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub CODENAME_set(ByRef T00015row As DataRow)


        '○名称付与

        '会社名称
        T00015row("CAMPCODENAME") = ""
        CODENAME_get("CAMPCODE", T00015row("CAMPCODE"), T00015row("CAMPCODENAME"), WW_DUMMY)

        '取引先名称
        T00015row("TORICODENAME") = ""
        CODENAME_get("TORICODE", T00015row("TORICODE"), T00015row("TORICODENAME"), WW_DUMMY)

        '油種名称
        T00015row("OILTYPENAME") = ""
        CODENAME_get("OILTYPE", T00015row("OILTYPE"), T00015row("OILTYPENAME"), WW_DUMMY)

        '品名１名称
        T00015row("PRODUCT1NAME") = ""
        CODENAME_get("PRODUCT1", T00015row("PRODUCT1"), T00015row("PRODUCT1NAME"), WW_DUMMY)

        '品名コード名称
        T00015row("PRODUCTNAME") = ""
        CODENAME_get("PRODUCTCODE", T00015row("PRODUCTCODE"), T00015row("PRODUCTNAME"), WW_DUMMY)
        T00015row("PRODUCT2NAME") = T00015row("PRODUCTNAME")

        '出荷場所名称
        T00015row("SHUKABASHONAME") = ""
        CODENAME_get("SHUKABASHO", T00015row("SHUKABASHO"), T00015row("SHUKABASHONAME"), WW_DUMMY)

        '出荷部署名称
        T00015row("SHIPORGNAME") = ""
        CODENAME_get("SHIPORG", T00015row("SHIPORG"), T00015row("SHIPORGNAME"), WW_DUMMY)

        '業務車番ナンバー
        T00015row("GSHABANLICNPLTNO") = ""
        CODENAME_get("GSHABAN", T00015row("GSHABAN"), T00015row("GSHABANLICNPLTNO"), WW_DUMMY)

        '乗務員コード
        T00015row("STAFFCODENAME") = ""
        CODENAME_get("STAFFCODE", T00015row("STAFFCODE"), T00015row("STAFFCODENAME"), WW_DUMMY)

        '副乗務員コード名称
        T00015row("SUBSTAFFCODENAME") = ""
        CODENAME_get("SUBSTAFFCODE", T00015row("SUBSTAFFCODE"), T00015row("SUBSTAFFCODENAME"), WW_DUMMY)

        '届先コード名称
        T00015row("TODOKECODENAME") = ""
        CODENAME_get("TODOKECODE", T00015row("TODOKECODE"), T00015row("TODOKECODENAME"), WW_DUMMY)

        '実績区分
        T00015row("JISSEKIKBNNAME") = ""
        CODENAME_get("JISSEKIKBN", T00015row("JISSEKIKBN"), T00015row("JISSEKIKBNNAME"), WW_DUMMY)

        '届先追加情報
        Dim datTodoke As JOT_MASTER.TODOKESAKI = JOTMASTER.GetTodoke(T00015row("TODOKECODE"))
        If Not IsNothing(datTodoke) AndAlso Not IsNothing(datTodoke.TODOKECODE) Then
            T00015row("ADDR") = datTodoke.ADDR                          '住所
            T00015row("NOTES1") = datTodoke.NOTES1                      '特定要件１
            T00015row("NOTES2") = datTodoke.NOTES2                      '特定要件２
            T00015row("NOTES3") = datTodoke.NOTES3                      '特定要件３
            T00015row("NOTES4") = datTodoke.NOTES4                      '特定要件４
            T00015row("NOTES5") = datTodoke.NOTES5                      '特定要件５
            T00015row("NOTES6") = datTodoke.NOTES6                      '特定要件６
            T00015row("NOTES7") = datTodoke.NOTES7                      '特定要件７
            T00015row("NOTES8") = datTodoke.NOTES8                      '特定要件８
            T00015row("NOTES9") = datTodoke.NOTES9                      '特定要件９
            T00015row("NOTES10") = datTodoke.NOTES10                    '特定要件１０
        End If

        ''車両追加情報
        For i As Integer = 0 To WF_ListGSHABAN.Items.Count - 1
            If WF_ListGSHABAN.Items(i).Value = T00015row("GSHABAN") Then
                If Val(T00015row("SHAFUKU")) = 0 Then
                    T00015row("SHAFUKU") = WF_ListSHAFUKU.Items(i).Value                  'List車腹
                End If
                T00015row("SHARYOTYPEF") = Mid(WF_ListTSHABANF.Items(i).Value, 1, 1)  'List統一車番（前）
                T00015row("TSHABANF") = Mid(WF_ListTSHABANF.Items(i).Value, 2, 19)    'List統一車番（前）
                T00015row("SHARYOTYPEB") = Mid(WF_ListTSHABANB.Items(i).Value, 1, 1)  'List統一車番（後）
                T00015row("TSHABANB") = Mid(WF_ListTSHABANB.Items(i).Value, 2, 19)    'List統一車番（後）
                T00015row("SHARYOTYPEB2") = Mid(WF_ListTSHABANB2.Items(i).Value, 1, 1) 'List統一車番（後）２
                T00015row("TSHABANB2") = Mid(WF_ListTSHABANB2.Items(i).Value, 2, 19)   'List統一車番（後）２
                T00015row("SHARYOINFO1") = WF_ListSHARYOINFO1.Items(i).Value          'List車両情報１
                T00015row("SHARYOINFO2") = WF_ListSHARYOINFO2.Items(i).Value          'List車両情報２
                T00015row("SHARYOINFO3") = WF_ListSHARYOINFO3.Items(i).Value          'List車両情報３
                T00015row("SHARYOINFO4") = WF_ListSHARYOINFO4.Items(i).Value          'List車両情報４
                T00015row("SHARYOINFO5") = WF_ListSHARYOINFO5.Items(i).Value          'List車両情報５
                T00015row("SHARYOINFO6") = WF_ListSHARYOINFO6.Items(i).Value          'List車両情報６
                Exit For
            End If
        Next

        '従業員追加情報
        Dim datStaff As JOT_MASTER.STAFF = JOTMASTER.GetStaff(T00015row("STAFFCODE"))
        If Not IsNothing(datStaff) AndAlso Not IsNothing(datStaff.STAFFCODE) Then
            T00015row("STAFFCODENAME") = datStaff.STAFFNAMES                '
            T00015row("STAFFNOTES1") = datStaff.NOTES1                      '備考１
            T00015row("STAFFNOTES2") = datStaff.NOTES2                      '備考２
            T00015row("STAFFNOTES3") = datStaff.NOTES3                      '備考３
            T00015row("STAFFNOTES4") = datStaff.NOTES4                      '備考４
            T00015row("STAFFNOTES5") = datStaff.NOTES5                      '備考５
        End If

    End Sub

    ''' <summary>
    ''' GridViewダブルクリック処理 
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_Grid_DBclick()

        Dim WW_LINECNT As Integer                                   'GridViewのダブルクリック行位置

        '○処理準備
        '○画面表示データ復元
        Master.RecoverTable(T00015tbl)

        'GridViewのダブルクリック行位置取得
        If Not Integer.TryParse(WF_GridDBclick.Text, WW_LINECNT) Then
            Exit Sub
        End If
        WF_REP_LINECNT.Value = WW_LINECNT

        '■■■ Grid内容(T00015tbl)よりDetail編集 ■■■
        Master.CreateEmptyTable(T00015INPtbl)

        '行位置が一致するデータ取得
        Dim T00015INP = (From tbl In T00015tbl.AsEnumerable Select tbl
                         Where tbl.Field(Of Integer)("LINECNT") = WW_LINECNT _
                           And tbl.Field(Of String)("SELECT") = "1")

        'DetailBoxKey画面編集
        If T00015INP.Count > 0 Then
            Dim T00015row = T00015INP.Last

            '１段目
            WF_Sel_LINECNT.Text = T00015row("LINECNT")

            '２段目
            WF_SHUKODATE.Text = T00015row("SHUKODATE")
            WF_KIKODATE.Text = T00015row("KIKODATE")
            WF_ORDERNO.Text = T00015row("ORDERNO")

            '３段目
            WF_SHUKADATE.Text = T00015row("SHUKADATE")
            WF_TODOKEDATE.Text = T00015row("TODOKEDATE")
            WF_RYOME.Text = T00015row("RYOME")
            WF_DETAILNO.Text = T00015row("DETAILNO")

            '４段目
            WF_OILTYPE.Text = T00015row("OILTYPE")
            WF_OILTYPE_TEXT.Text = T00015row("OILTYPENAME")
            WF_SHIPORG.Text = T00015row("SHIPORG")
            WF_SHIPORG_TEXT.Text = T00015row("SHIPORGNAME")

            '５段目
            WF_TORICODE.Text = T00015row("TORICODE")
            WF_TORICODE_TEXT.Text = T00015row("TORICODENAME")

            '６段目・７段目
            '業務車番
            WF_GSHABAN.Text = T00015row("GSHABAN")
            WF_TSHABANF.Text = T00015row("SHARYOTYPEF") & T00015row("TSHABANF")
            CODENAME_get("TSHABANF", WF_TSHABANF.Text, WF_TSHABANF_TEXT.Text, WW_DUMMY)
            WF_TSHABANB.Text = T00015row("SHARYOTYPEB") & T00015row("TSHABANB")
            CODENAME_get("TSHABANB", WF_TSHABANB.Text, WF_TSHABANB_TEXT.Text, WW_DUMMY)
            WF_TSHABANB2.Text = T00015row("SHARYOTYPEB2") & T00015row("TSHABANB2")
            CODENAME_get("TSHABANB2", WF_TSHABANB2.Text, WF_TSHABANB2_TEXT.Text, WW_DUMMY)
            WF_SHAFUKU.Text = T00015row("SHAFUKU")

            '８段目
            WF_TRIPNO.Text = T00015row("TRIPNO")
            WF_DROPNO.Text = T00015row("DROPNO")

            '９段目 乗務員
            WF_STAFFCODE.Text = T00015row("STAFFCODE")
            WF_STAFFCODE_TEXT.Text = T00015row("STAFFCODENAME")
            WF_SUBSTAFFCODE.Text = T00015row("SUBSTAFFCODE")
            WF_SUBSTAFFCODE_TEXT.Text = T00015row("SUBSTAFFCODENAME")

            WF_JISSEKIKBN.Text = T00015row("JISSEKIKBN")
            WF_JISSEKIKBN_TEXT.Text = T00015row("JISSEKIKBNNAME")

            For i As Integer = 0 To T00015INP.Count - 1
                T00015INP(i).Item("WORK_NO") = i

                ''○名称付与
                'CODENAME_set(T00015INP(i))
            Next
            '編集済みINPデータをINPtblに設定
            T00015INPtbl = T00015INP.CopyToDataTable

            WF_Sel_LINECNT.Enabled = True
            WF_SHUKODATE.Enabled = True
            WF_SHUKADATE.Enabled = True
            WF_TODOKEDATE.Enabled = True
            WF_KIKODATE.Enabled = True
            WF_RYOME.Enabled = True
            WF_ORDERNO.Enabled = True
            WF_DETAILNO.Enabled = True
            WF_SHIPORG.Enabled = True
            WF_TORICODE.Enabled = True
            WF_OILTYPE.Enabled = True
            WF_GSHABAN.Enabled = True
            WF_TSHABANF.Enabled = True
            WF_TSHABANB.Enabled = True
            WF_TSHABANB2.Enabled = True
            WF_SHAFUKU.Enabled = True
            WF_TRIPNO.Enabled = True
            WF_DROPNO.Enabled = True
            WF_JISSEKIKBN.Enabled = True

        End If

        '追記行（空行）を4件作成
        For i As Integer = 1 To 4
            Dim T00015INProw = T00015INPtbl.NewRow()
            T00015INProw("SELECT") = 1
            T00015INProw("HIDDEN") = 1
            T00015INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA
            T00015INProw("TIMSTP") = 0
            T00015INProw("LINECNT") = WW_LINECNT

            T00015INProw("CAMPCODE") = ""
            T00015INProw("TORICODE") = ""
            T00015INProw("OILTYPE") = ""
            T00015INProw("KIJUNDATE") = ""
            T00015INProw("SHUKADATE") = ""
            T00015INProw("SHIPORG") = ""

            T00015INProw("INDEX") = ""
            T00015INProw("CAMPCODENAME") = ""
            T00015INProw("ORDERNO") = ""
            T00015INProw("DETAILNO") = ""
            T00015INProw("TRIPNO") = ""
            T00015INProw("DROPNO") = ""
            T00015INProw("SEQ") = ""
            T00015INProw("TORICODENAME") = ""
            T00015INProw("OILTYPENAME") = ""
            T00015INProw("SHUKODATE") = ""
            T00015INProw("KIKODATE") = ""
            T00015INProw("SHIPORGNAME") = ""
            T00015INProw("SHUKABASHO") = ""
            T00015INProw("SHUKABASHONAME") = ""
            T00015INProw("GSHABAN") = ""
            T00015INProw("GSHABANLICNPLTNO") = ""
            T00015INProw("RYOME") = ""
            T00015INProw("SHAFUKU") = ""
            T00015INProw("STAFFCODE") = ""
            T00015INProw("STAFFCODENAME") = ""
            T00015INProw("SUBSTAFFCODE") = ""
            T00015INProw("SUBSTAFFCODENAME") = ""
            T00015INProw("TODOKEDATE") = ""
            T00015INProw("TODOKECODE") = ""
            T00015INProw("TODOKECODENAME") = ""
            T00015INProw("PRODUCT1") = ""
            T00015INProw("PRODUCT1NAME") = ""
            T00015INProw("PRODUCT2") = ""
            T00015INProw("PRODUCTCODE") = ""
            T00015INProw("PRODUCTNAME") = ""
            T00015INProw("CONTNO") = ""
            T00015INProw("JSURYO") = ""
            T00015INProw("JSURYO_SUM") = ""
            T00015INProw("JDAISU") = ""
            T00015INProw("JDAISU_SUM") = ""
            T00015INProw("REMARKS1") = ""
            T00015INProw("REMARKS2") = ""
            T00015INProw("REMARKS3") = ""
            T00015INProw("REMARKS4") = ""
            T00015INProw("REMARKS5") = ""
            T00015INProw("REMARKS6") = ""
            T00015INProw("SHARYOTYPEF") = ""
            T00015INProw("TSHABANF") = ""
            T00015INProw("SHARYOTYPEB") = ""
            T00015INProw("TSHABANB") = ""
            T00015INProw("SHARYOTYPEB2") = ""
            T00015INProw("TSHABANB2") = ""
            T00015INProw("JISSEKIKBN") = ""
            T00015INProw("JISSEKIKBNNAME") = ""
            T00015INProw("DELFLG") = ""

            T00015INProw("ADDR") = ""
            T00015INProw("NOTES1") = ""
            T00015INProw("NOTES2") = ""
            T00015INProw("NOTES3") = ""
            T00015INProw("NOTES4") = ""
            T00015INProw("NOTES5") = ""
            T00015INProw("NOTES6") = ""
            T00015INProw("NOTES7") = ""
            T00015INProw("NOTES8") = ""
            T00015INProw("NOTES9") = ""
            T00015INProw("NOTES10") = ""
            T00015INProw("STAFFNOTES1") = ""
            T00015INProw("STAFFNOTES2") = ""
            T00015INProw("STAFFNOTES3") = ""
            T00015INProw("STAFFNOTES4") = ""
            T00015INProw("STAFFNOTES5") = ""

            T00015INProw("SHARYOINFO1") = ""
            T00015INProw("SHARYOINFO2") = ""
            T00015INProw("SHARYOINFO3") = ""
            T00015INProw("SHARYOINFO4") = ""
            T00015INProw("SHARYOINFO5") = ""
            T00015INProw("SHARYOINFO6") = ""

            T00015INProw("WORK_NO") = ""

            T00015INPtbl.Rows.Add(T00015INProw)
        Next

        '○Detail初期設定
        Repeater_INIT()

        '■画面WF_GRID状態設定

        '選択明細のOperation項目に状態を設定(更新・追加・削除は編集中を設定しない)
        EditOperationText(T00015tbl, True, WW_LINECNT)

        '○ 画面表示データ保存
        Master.SaveTable(T00015tbl)

        'カーソル設定
        WF_FIELD.Value = "WF_SHUKADATE"
        WF_SHUKODATE.Focus()
        WF_REP_Change.Value = ""         'リピータ変更監視

        'leftBOXキャンセルボタン処理
        WF_ButtonCan_Click()

    End Sub

    ''' <summary>
    ''' 詳細画面項目クリア処理  
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub ClearDetailBox()

        '○detailboxヘッダークリア
        '出庫日
        WF_SHUKODATE.Text = ""
        '出荷日
        WF_SHUKADATE.Text = ""
        '届日
        WF_TODOKEDATE.Text = ""
        '帰庫日
        WF_KIKODATE.Text = ""

        WF_RYOME.Text = ""
        WF_ORDERNO.Text = ""
        WF_DETAILNO.Text = ""

        WF_SHIPORG.Text = ""
        WF_SHIPORG_TEXT.Text = ""
        WF_TORICODE.Text = ""
        WF_TORICODE_TEXT.Text = ""
        WF_OILTYPE.Text = ""
        WF_OILTYPE_TEXT.Text = ""

        '業務車番
        WF_GSHABAN.Text = ""
        WF_TSHABANF.Text = ""
        WF_TSHABANF_TEXT.Text = ""
        WF_TSHABANB.Text = ""
        WF_TSHABANB_TEXT.Text = ""
        WF_TSHABANB2.Text = ""
        WF_TSHABANB2_TEXT.Text = ""
        '車腹
        WF_SHAFUKU.Text = ""

        'トリップ
        WF_TRIPNO.Text = ""
        'ドロップ
        WF_DROPNO.Text = ""

        '乗務員
        WF_STAFFCODE.Text = ""
        WF_STAFFCODE_TEXT.Text = ""
        '副乗務員
        WF_SUBSTAFFCODE.Text = ""
        WF_SUBSTAFFCODE_TEXT.Text = ""

        '実績区分
        WF_JISSEKIKBN.Text = ""
        WF_JISSEKIKBN_TEXT.Text = ""

        WF_Sel_LINECNT.Text = ""

    End Sub

    ''' <summary>
    ''' 明細行 編集処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Function WF_ITEM_FORMAT(ByVal I_FIELD As String, ByRef I_VALUE As String) As String
        WF_ITEM_FORMAT = I_VALUE
        Select Case I_FIELD
            Case "SEQ"
                Try
                    WF_ITEM_FORMAT = CInt(I_VALUE).ToString("00")
                Catch ex As Exception
                End Try
            Case Else
        End Select
    End Function

    ''' <summary>
    ''' 詳細画面 初期設定(空明細作成 イベント追加)
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub Repeater_INIT()
        Dim repField As Label = Nothing
        Dim repValue As TextBox = Nothing
        Dim repName As Label = Nothing
        Dim repAttr As String = ""

        Try
            'リピーター作成
            CS0052DetailView.CAMPCODE = work.WF_SEL_CAMPCODE.Text
            CS0052DetailView.PROFID = Master.PROF_VIEW
            CS0052DetailView.MAPID = Master.MAPID
            CS0052DetailView.VARI = Master.VIEWID
            CS0052DetailView.TABID = CONST_DETAIL_TABID
            CS0052DetailView.SRCDATA = T00015INPtbl
            CS0052DetailView.REPEATER = WF_DViewRep1
            CS0052DetailView.COLPREFIX = "WF_Rep1_"
            CS0052DetailView.MaketDetailView()
            If Not isNormal(CS0052DetailView.ERR) Then
                Master.Output(CS0052DetailView.ERR, C_MESSAGE_TYPE.ABORT)
                Exit Sub
            End If

            'リピータの１明細の行数を保存
            WF_REP_ROWSCNT.Value = CS0052DetailView.ROWMAX
            WF_REP_COLSCNT.Value = CS0052DetailView.COLMAX

            Dim WW_T00015INPcnt As Integer
            WW_T00015INPcnt = T00015INPtbl.Select("TORICODE <> ''").Count

            WF_DetailMView.ActiveViewIndex = 0
            For row As Integer = 0 To (T00015INPtbl.Rows.Count * CS0052DetailView.ROWMAX) - 1
                If (row + 1) <= (CS0052DetailView.ROWMAX * WW_T00015INPcnt) Then
                    CType(WF_DViewRep1.Items(row).FindControl("WF_Rep1_MEISAINO"), System.Web.UI.WebControls.TextBox).Text =
                        ((row \ CS0052DetailView.ROWMAX) + 1).ToString("000")
                Else
                    CType(WF_DViewRep1.Items(row).FindControl("WF_Rep1_MEISAINO"), System.Web.UI.WebControls.TextBox).Text = ""
                End If
                Dim WW_RepeaterLINE = CType(WF_DViewRep1.Items(row).FindControl("WF_Rep1_LINEPOSITION"), System.Web.UI.WebControls.TextBox)

                For col As Integer = 1 To CS0052DetailView.COLMAX

                    If DirectCast(WF_DViewRep1.Items(row).FindControl("WF_Rep1_FIELD_" & col), System.Web.UI.WebControls.Label).Text <> "" Then

                        repField = DirectCast(WF_DViewRep1.Items(row).FindControl("WF_Rep1_FIELD_" & col), System.Web.UI.WebControls.Label)
                        repValue = DirectCast(WF_DViewRep1.Items(row).FindControl("WF_Rep1_VALUE_" & col), System.Web.UI.WebControls.TextBox)
                        repName = DirectCast(WF_DViewRep1.Items(row).FindControl("WF_Rep1_VALUE_TEXT_" & col), System.Web.UI.WebControls.Label)

                        '値（名称）設定
                        CODENAME_get(repField.Text, repValue.Text, repName.Text, WW_DUMMY)

                        'ダブルクリック時コード検索イベント追加
                        REP_ATTR_get(repField.Text, WW_RepeaterLINE.Text, repAttr)
                        If repAttr <> "" AndAlso repValue.ReadOnly = False Then
                            repValue.Attributes.Remove("ondblclick")
                            repValue.Attributes.Add("ondblclick", repAttr)
                            repName = DirectCast(WF_DViewRep1.Items(row).FindControl("WF_Rep1_FIELDNM_" & col), System.Web.UI.WebControls.Label)
                            repName.Attributes.Remove("style")
                            repName.Attributes.Add("style", "text-decoration: underline;")
                        End If
                        repValue.Attributes.Remove("onchange")
                        repValue.Attributes.Add("onchange", "f_Rep1_Change(2)")

                        repValue.Enabled = True

                    End If

                Next col

                '■■■ LINE表示設定（1明細目の最終行） ■■■
                If (CS0052DetailView.ROWMAX - 1) > 0 And ((row + 1) Mod CS0052DetailView.ROWMAX) = 0 Then
                    CType(WF_DViewRep1.Items(row).FindControl("WF_Rep1_LINE"), System.Web.UI.WebControls.Label).Style.Remove("display")
                    CType(WF_DViewRep1.Items(row).FindControl("WF_Rep1_LINE"), System.Web.UI.WebControls.Label).Style.Add("display", "block")
                End If
            Next row

            WF_DViewRep1.Visible = True

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.SYSTEM_ADM_ERROR, C_MESSAGE_TYPE.ABORT)
        Finally
        End Try

    End Sub

    ''' <summary>
    ''' 詳細画面-イベント文字取得
    ''' </summary>
    ''' <param name="I_FIELD">フィールド名</param>
    ''' <param name="O_ATTR">イベント内容</param>
    ''' <remarks></remarks>
    Protected Sub REP_ATTR_get(ByVal I_FIELD As String, ByVal I_INDEX As String, ByRef O_ATTR As String)

        O_ATTR = "Repeater_Gyou(" & I_INDEX & ");"
        Select Case I_FIELD
            Case "SHUKODATE"
                '出庫日
                O_ATTR &= "REF_Field_DBclick('SHUKODATE', 'WF_Rep_FIELD' , " & LIST_BOX_CLASSIFICATION.LC_CALENDAR & ");"
            Case "KIKODATE"
                '帰庫日
                O_ATTR &= "REF_Field_DBclick('KIKODATE', 'WF_Rep_FIELD' , " & LIST_BOX_CLASSIFICATION.LC_CALENDAR & ");"
            Case "PRODUCT1"
                '品名１
                O_ATTR &= "REF_Field_DBclick('PRODUCT1', 'WF_Rep_FIELD' , " & LIST_BOX_CLASSIFICATION.LC_GOODS & ");"
            Case "PRODUCT2"
                '品名２
                O_ATTR &= "REF_Field_DBclick('PRODUCT2', 'WF_Rep_FIELD' , " & LIST_BOX_CLASSIFICATION.LC_GOODS & ");"
            Case "PRODUCTCODE"
                '品名コード
                O_ATTR &= "REF_Field_DBclick('PRODUCTCODE', 'WF_Rep_FIELD' , " & LIST_BOX_CLASSIFICATION.LC_GOODS & ");"
            Case "SHUKABASHO"
                '出荷場所
                O_ATTR &= "REF_Field_DBclick('SHUKABASHO', 'WF_Rep_FIELD' , " & LIST_BOX_CLASSIFICATION.LC_DISTINATION & ");"
            Case "SHIPORG"
                '出荷部署
                O_ATTR &= "REF_Field_DBclick('SHIPORG', 'WF_Rep_FIELD' , " & LIST_BOX_CLASSIFICATION.LC_ORG & ");"
            Case "GSHABAN"
                '業務車番
                O_ATTR &= "REF_Field_DBclick('GSHABAN', 'WF_Rep_FIELD' , " & LIST_BOX_CLASSIFICATION.LC_EXTRA_LIST & ");"
            Case "STAFFCODE"
                '乗務員コード
                O_ATTR &= "REF_Field_DBclick('STAFFCODE', 'WF_Rep_FIELD' , " & LIST_BOX_CLASSIFICATION.LC_EXTRA_LIST & ");"
            Case "SUBSTAFFCODE"
                '副乗務員コード
                O_ATTR &= "REF_Field_DBclick('SUBSTAFFCODE', 'WF_Rep_FIELD' , " & LIST_BOX_CLASSIFICATION.LC_EXTRA_LIST & ");"
            Case "TODOKECODE"
                '届先コード
                O_ATTR &= "REF_Field_DBclick('TODOKECODE', 'WF_Rep_FIELD' , " & LIST_BOX_CLASSIFICATION.LC_DISTINATION & ");"
            Case "JISSEKIKBN"
                '実績区分
                O_ATTR &= "REF_Field_DBclick('JISSEKIKBN', 'WF_Rep_FIELD' , " & LIST_BOX_CLASSIFICATION.LC_FIX_VALUE & ");"
            Case "DELFLG"
                '削除
                O_ATTR &= "REF_Field_DBclick('DELFLG', 'WF_Rep_FIELD' , " & LIST_BOX_CLASSIFICATION.LC_DELFLG & ");"
            Case Else
                O_ATTR = String.Empty
        End Select


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

#Region "T0015テーブル関連"
    ''' <summary>
    ''' 画面表示用データ取得
    ''' </summary>
    ''' <remarks>データベース（T00015）を検索し画面表示用データを取得する</remarks>
    Private Sub DBselect_T15SELECT()

        Dim WW_DATE As Date
        'Dim WW_TIME As DateTime

        '〇GridView内容をテーブル退避
        'T00015テンポラリDB項目作成
        If T00015tbl Is Nothing Then
            T00015tbl = New DataTable
        End If

        If T00015tbl.Columns.Count = 0 Then
        Else
            T00015tbl.Columns.Clear()
        End If

        '○DB項目クリア
        T00015tbl.Clear()

        '〇画面表示用データ取得
        Try

            'DataBase接続文字
            Using SQLcon = CS0050SESSION.getConnection
                SQLcon.Open() 'DataBase接続(Open)

                '検索SQL文
                Dim SQLStr As String =
                     "SELECT 0                                     as LINECNT ,        " _
                   & "       ''                                    as OPERATION ,      " _
                   & "       '0'                                   as 'SELECT' ,       " _
                   & "       '0'                                   as HIDDEN ,         " _
                   & "       ''                                    as 'INDEX' ,        " _
                   & "       isnull(rtrim(A.CAMPCODE),'')          as CAMPCODE ,       " _
                   & "       isnull(rtrim(A.ORDERNO),'')           as ORDERNO ,        " _
                   & "       isnull(rtrim(A.DETAILNO),'')          as DETAILNO ,       " _
                   & "       isnull(rtrim(A.TRIPNO),'')            as TRIPNO ,         " _
                   & "       isnull(rtrim(A.DROPNO),'')            as DROPNO ,         " _
                   & "       isnull(rtrim(A.SEQ),'00')             as SEQ ,            " _
                   & "       isnull(rtrim(A.TORICODE),'')          as TORICODE ,       " _
                   & "       isnull(rtrim(A.OILTYPE),'')           as OILTYPE ,        " _
                   & "       isnull(rtrim(A.SHUKODATE),'')         as SHUKODATE ,      " _
                   & "       isnull(rtrim(A.KIKODATE),'')          as KIKODATE ,       " _
                   & "       isnull(rtrim(A.KIJUNDATE),'')         as KIJUNDATE ,      " _
                   & "       isnull(rtrim(A.SHUKADATE),'')         as SHUKADATE ,      " _
                   & "       isnull(rtrim(A.SHIPORG),'')           as SHIPORG ,        " _
                   & "       isnull(rtrim(A.SHUKABASHO),'')        as SHUKABASHO ,     " _
                   & "       isnull(rtrim(A.GSHABAN),'')           as GSHABAN ,        " _
                   & "       isnull(rtrim(A.RYOME),'')             as RYOME ,          " _
                   & "       isnull(rtrim(A.SHAFUKU),'')           as SHAFUKU ,        " _
                   & "       isnull(rtrim(A.STAFFCODE),'')         as STAFFCODE ,      " _
                   & "       isnull(rtrim(A.SUBSTAFFCODE),'')      as SUBSTAFFCODE ,   " _
                   & "       isnull(rtrim(A.TODOKEDATE),'')        as TODOKEDATE ,     " _
                   & "       isnull(rtrim(A.TODOKECODE),'')        as TODOKECODE ,     " _
                   & "       isnull(rtrim(A.PRODUCT1),'')          as PRODUCT1 ,       " _
                   & "       isnull(rtrim(A.PRODUCT2),'')          as PRODUCT2 ,       " _
                   & "       isnull(rtrim(A.PRODUCTCODE),'')       as PRODUCTCODE ,    " _
                   & "       isnull(rtrim(A.CONTNO),'')            as CONTNO ,         " _
                   & "       isnull(rtrim(A.JSURYO),'')            as JSURYO ,         " _
                   & "       isnull(rtrim(A.JDAISU),'')            as JDAISU ,         " _
                   & "       isnull(rtrim(A.REMARKS1),'')          as REMARKS1 ,       " _
                   & "       isnull(rtrim(A.REMARKS2),'')          as REMARKS2 ,       " _
                   & "       isnull(rtrim(A.REMARKS3),'')          as REMARKS3 ,       " _
                   & "       isnull(rtrim(A.REMARKS4),'')          as REMARKS4 ,       " _
                   & "       isnull(rtrim(A.REMARKS5),'')          as REMARKS5 ,       " _
                   & "       isnull(rtrim(A.REMARKS6),'')          as REMARKS6 ,       " _
                   & "       isnull(rtrim(A.SHARYOTYPEF),'')       as SHARYOTYPEF ,    " _
                   & "       isnull(rtrim(A.TSHABANF),'')          as TSHABANF ,       " _
                   & "       isnull(rtrim(A.SHARYOTYPEB),'')       as SHARYOTYPEB ,    " _
                   & "       isnull(rtrim(A.TSHABANB),'')          as TSHABANB ,       " _
                   & "       isnull(rtrim(A.SHARYOTYPEB2),'')      as SHARYOTYPEB2 ,   " _
                   & "       isnull(rtrim(A.TSHABANB2),'')         as TSHABANB2 ,      " _
                   & "       isnull(rtrim(A.JISSEKIKBN),'')        as JISSEKIKBN ,         " _
                   & "       isnull(rtrim(A.DELFLG),'')            as DELFLG ,         " _
                   & "       TIMSTP = cast(A.UPDTIMSTP  as bigint) ,        " _
                   & "       isnull(rtrim(B.SHARYOINFO1),'')       as SHARYOINFO1 ,    " _
                   & "       isnull(rtrim(B.SHARYOINFO2),'')       as SHARYOINFO2 ,    " _
                   & "       isnull(rtrim(B.SHARYOINFO3),'')       as SHARYOINFO3 ,    " _
                   & "       isnull(rtrim(B.SHARYOINFO4),'')       as SHARYOINFO4 ,    " _
                   & "       isnull(rtrim(B.SHARYOINFO5),'')       as SHARYOINFO5 ,    " _
                   & "       isnull(rtrim(B.SHARYOINFO6),'')       as SHARYOINFO6 ,    " _
                   & "       isnull(rtrim(D.ADDR1),'') +              				   " _
                   & "       isnull(rtrim(D.ADDR2),'') +            				   " _
                   & "       isnull(rtrim(D.ADDR3),'') +             				   " _
                   & "       isnull(rtrim(D.ADDR4),'')          	as ADDR ,          " _
                   & "       isnull(rtrim(D.NOTES1),'')        	    as NOTES1 ,        " _
                   & "       isnull(rtrim(D.NOTES2),'')          	as NOTES2 ,        " _
                   & "       isnull(rtrim(D.NOTES3),'')          	as NOTES3 ,        " _
                   & "       isnull(rtrim(D.NOTES4),'')          	as NOTES4 ,        " _
                   & "       isnull(rtrim(D.NOTES5),'')          	as NOTES5 ,        " _
                   & "       isnull(rtrim(D.NOTES6),'')        	    as NOTES6 ,        " _
                   & "       isnull(rtrim(D.NOTES7),'')          	as NOTES7 ,        " _
                   & "       isnull(rtrim(D.NOTES8),'')          	as NOTES8 ,        " _
                   & "       isnull(rtrim(D.NOTES9),'')          	as NOTES9 ,        " _
                   & "       isnull(rtrim(D.NOTES10),'')          	as NOTES10 ,       " _
                   & "       isnull(rtrim(E.NOTES1),'')        	    as STAFFNOTES1 ,   " _
                   & "       isnull(rtrim(E.NOTES2),'')          	as STAFFNOTES2 ,   " _
                   & "       isnull(rtrim(E.NOTES3),'')          	as STAFFNOTES3 ,   " _
                   & "       isnull(rtrim(E.NOTES4),'')          	as STAFFNOTES4 ,   " _
                   & "       isnull(rtrim(E.NOTES5),'')          	as STAFFNOTES5 ,   " _
                   & "       ''                                    as CAMPCODENAME ,   " _
                   & "       ''                                    as TORICODENAME ,   " _
                   & "       ''                                    as OILTYPENAME ,    " _
                   & "       ''                                    as SHIPORGNAME ,    " _
                   & "       ''                                    as SHUKABASHONAME , " _
                   & "       ''                                    as GSHABANLICNPLTNO ,    " _
                   & "       ''                                    as STAFFCODENAME ,    " _
                   & "       ''                                    as SUBSTAFFCODENAME ,    " _
                   & "       ''                                    as TODOKECODENAME ,    " _
                   & "       ''                                    as PRODUCT1NAME ,   " _
                   & "       ''                                    as PRODUCT2NAME ,   " _
                   & "       ''                                    as PRODUCTNAME ,   " _
                   & "       ''                                    as JISSEKIKBNNAME ,    " _
                   & "       ''                                    as JSURYO_SUM ,      " _
                   & "       ''                                    as JDAISU_SUM ,      " _
                   & "       ''                                    as ROWDEL ,   " _
                   & "       '0'                                   as WORK_NO          " _
                   & "  FROM T0015_SUPPLJISSKI AS A								" _
                   & " INNER JOIN ( SELECT Y.CAMPCODE, Y.CODE               " _
                   & "                FROM S0006_ROLE Y     				" _
                   & "               WHERE Y.CAMPCODE 	 	   = @P01		" _
                   & "                 and Y.OBJECT       	   = 'ORG'		" _
                   & "                 and Y.ROLE              = @P02		" _
                   & "                 and Y.PERMITCODE       in ('1','2')  " _
                   & "                 and Y.STYMD            <= @P03		" _
                   & "                 and Y.ENDYMD           >= @P04		" _
                   & "                 and Y.DELFLG           <> '1'		" _
                   & "            ) AS Z									" _
                   & "    ON Z.CAMPCODE		= A.CAMPCODE    				" _
                   & "   and Z.CODE       	= A.SHIPORG 	    			" _
                   & "  LEFT JOIN MA006_SHABANORG B							" _
                   & "    ON B.CAMPCODE     	= A.CAMPCODE 				" _
                   & "   and B.GSHABAN      	= A.GSHABAN 				" _
                   & "   and B.MANGUORG     	= A.SHIPORG 				" _
                   & "   and B.DELFLG          <> '1' 						" _
                   & "  LEFT JOIN MC007_TODKORG C 							" _
                   & "    ON C.CAMPCODE     	= A.CAMPCODE 				" _
                   & "   and C.TORICODE     	= A.TORICODE 				" _
                   & "   and C.TODOKECODE   	= A.TODOKECODE 				" _
                   & "   and C.UORG         	= A.SHIPORG 				" _
                   & "   and C.DELFLG          <> '1' 						" _
                   & "  LEFT JOIN MC006_TODOKESAKI D 						" _
                   & "    ON D.CAMPCODE     	= C.CAMPCODE 				" _
                   & "   and D.TORICODE     	= C.TORICODE				" _
                   & "   and D.TODOKECODE   	= C.TODOKECODE 				" _
                   & "   and D.STYMD           <= A.SHUKODATE				" _
                   & "   and D.ENDYMD          >= A.SHUKODATE				" _
                   & "   and D.DELFLG          <> '1' 						" _
                   & "  LEFT JOIN MB001_STAFF E      						" _
                   & "    ON E.CAMPCODE     	= A.CAMPCODE 				" _
                   & "   and E.STAFFCODE     	= A.STAFFCODE				" _
                   & "   and E.STYMD           <= A.SHUKODATE				" _
                   & "   and E.ENDYMD          >= A.SHUKODATE				" _
                   & "   and E.DELFLG          <> '1' 						" _
                   & " WHERE A.CAMPCODE         = @P01                      " _
                   & "   and A.SHUKADATE       <= @P05                      " _
                   & "   and A.SHUKADATE       >= @P06                      " _
                   & "   and A.TODOKEDATE      <= @P07                      " _
                   & "   and A.TODOKEDATE      >= @P08                      " _
                   & "   and A.SHUKODATE       <= @P09                      " _
                   & "   and A.SHUKODATE       >= @P10                      " _
                   & "   and A.DELFLG          <> '1'                       "

                '■テーブル検索条件追加

                '条件画面で指定された油種を抽出
                If work.WF_SEL_OILTYPE.Text <> Nothing Then
                    SQLStr = SQLStr & "   and A.OILTYPE          = @P11           		"
                End If

                '条件画面で指定された出荷部署を抽出
                If work.WF_SEL_SHIPORG.Text <> Nothing Then
                    SQLStr = SQLStr & "   and A.SHIPORG          = @P12           		"
                Else
                    '★★★未指定時はユーザ所属支店部署で縛る必要あり
                End If

                SQLStr = SQLStr & " ORDER BY A.TORICODE  ,A.OILTYPE ,A.SHUKADATE ,      " _
                                & " 		 A.SHIPORG ,	                " _
                                & " 		 A.SHUKODATE ,A.TODOKEDATE ,A.GSHABAN ,      " _
                                & " 		 A.RYOME     ,A.TRIPNO  ,A.DROPNO	 ,A.SEQ "

                Dim SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@P03", System.Data.SqlDbType.Date)      '権限(to)
                Dim PARA04 As SqlParameter = SQLcmd.Parameters.Add("@P04", System.Data.SqlDbType.Date)      '権限(from)
                Dim PARA05 As SqlParameter = SQLcmd.Parameters.Add("@P05", System.Data.SqlDbType.Date)      '出荷日(To)
                Dim PARA06 As SqlParameter = SQLcmd.Parameters.Add("@P06", System.Data.SqlDbType.Date)      '出荷日(From)
                Dim PARA07 As SqlParameter = SQLcmd.Parameters.Add("@P07", System.Data.SqlDbType.Date)      '届日(To)
                Dim PARA08 As SqlParameter = SQLcmd.Parameters.Add("@P08", System.Data.SqlDbType.Date)      '届日(From)
                Dim PARA09 As SqlParameter = SQLcmd.Parameters.Add("@P09", System.Data.SqlDbType.Date)      '出庫日(To)
                Dim PARA10 As SqlParameter = SQLcmd.Parameters.Add("@P10", System.Data.SqlDbType.Date)      '出庫日(From)
                Dim PARA11 As SqlParameter = SQLcmd.Parameters.Add("@P11", System.Data.SqlDbType.NVarChar, 20)  '油種
                Dim PARA12 As SqlParameter = SQLcmd.Parameters.Add("@P12", System.Data.SqlDbType.NVarChar, 20)  '出荷部署
                PARA01.Value = work.WF_SEL_CAMPCODE.Text
                PARA02.Value = Master.ROLE_ORG
                PARA03.Value = Date.Now
                PARA04.Value = Date.Now

                '出荷日(To)
                If String.IsNullOrWhiteSpace(work.WF_SEL_SHUKADATET.Text) Then
                    PARA05.Value = C_MAX_YMD
                Else
                    PARA05.Value = work.WF_SEL_SHUKADATET.Text
                End If
                '出荷日(From)
                If String.IsNullOrWhiteSpace(work.WF_SEL_SHUKADATEF.Text) Then
                    PARA06.Value = C_DEFAULT_YMD
                Else
                    PARA06.Value = work.WF_SEL_SHUKADATEF.Text
                End If
                '届日(To)
                If String.IsNullOrWhiteSpace(work.WF_SEL_TODOKEDATET.Text) Then
                    PARA07.Value = C_MAX_YMD
                Else
                    PARA07.Value = work.WF_SEL_TODOKEDATET.Text
                End If
                '届日(From)
                If String.IsNullOrWhiteSpace(work.WF_SEL_TODOKEDATEF.Text) Then
                    PARA08.Value = C_DEFAULT_YMD
                Else
                    PARA08.Value = work.WF_SEL_TODOKEDATEF.Text
                End If
                '出庫日(To)
                If String.IsNullOrWhiteSpace(work.WF_SEL_SHUKODATET.Text) Then
                    PARA09.Value = C_MAX_YMD
                Else
                    PARA09.Value = work.WF_SEL_SHUKODATET.Text
                End If
                '出庫日(From)
                If String.IsNullOrWhiteSpace(work.WF_SEL_SHUKODATEF.Text) Then
                    PARA10.Value = C_DEFAULT_YMD
                Else
                    PARA10.Value = work.WF_SEL_SHUKODATEF.Text
                End If

                PARA11.Value = work.WF_SEL_OILTYPE.Text
                PARA12.Value = work.WF_SEL_SHIPORG.Text

                Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                'フィールド名とフィールドの型を取得
                For index As Integer = 0 To SQLdr.FieldCount - 1
                    T00015tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                Next
                '〇テーブル検索結果をテーブル格納
                T00015tbl.Load(SQLdr)

                If T00015tbl.Rows.Count > CONST_DSPROW_MAX Then
                    'データ取得件数が65,000件を超えたため表示できません。選択条件を変更して下さい。
                    Master.Output(C_MESSAGE_NO.DISPLAY_RECORD_OVER, C_MESSAGE_TYPE.ABORT)
                    'Close
                    SQLdr.Close() 'Reader(Close)
                    SQLdr = Nothing

                    SQLcmd.Dispose()
                    SQLcmd = Nothing

                    SQLcon.Close() 'DataBase接続(Close)

                    T00015tbl.Clear()
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
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "T0015_SUPPLJISSKI SELECT")
            CS0011LOGWRITE.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:T0015_SUPPLJISSKI Select"           '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                  '
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try


        For Each T00015row In T00015tbl.Rows

            '○レコードの初期設定

            T00015row("LINECNT") = 0
            T00015row("SELECT") = 1   '1:表示
            T00015row("HIDDEN") = 0   '0:表示
            T00015row("INDEX") = ""
            T00015row("SEQ") = "00"
            T00015row("WORK_NO") = 0

            If Date.TryParse(T00015row("SHUKODATE"), WW_DATE) Then
                T00015row("SHUKODATE") = WW_DATE.ToString("yyyy/MM/dd")
            Else
                T00015row("SHUKODATE") = ""
            End If

            If Date.TryParse(T00015row("KIKODATE"), WW_DATE) Then
                T00015row("KIKODATE") = WW_DATE.ToString("yyyy/MM/dd")
            Else
                T00015row("KIKODATE") = ""
            End If

            If Date.TryParse(T00015row("KIJUNDATE"), WW_DATE) Then
                T00015row("KIJUNDATE") = WW_DATE.ToString("yyyy/MM/dd")
            Else
                T00015row("KIJUNDATE") = ""
            End If

            If Date.TryParse(T00015row("SHUKADATE"), WW_DATE) Then
                T00015row("SHUKADATE") = WW_DATE.ToString("yyyy/MM/dd")
            Else
                T00015row("SHUKADATE") = ""
            End If

            If Date.TryParse(T00015row("TODOKEDATE"), WW_DATE) Then
                T00015row("TODOKEDATE") = WW_DATE.ToString("yyyy/MM/dd")
            Else
                T00015row("TODOKEDATE") = ""
            End If

            '品名コード未登録は会社・油種・品名１・品名２から編集
            If String.IsNullOrEmpty(T00015row("PRODUCTCODE")) Then
                T00015row("PRODUCTCODE") = T00015row("CAMPCODE") + T00015row("OILTYPE") + T00015row("PRODUCT1") + T00015row("PRODUCT2")
            End If

            '○項目名称設定
            CODENAME_set(T00015row)

        Next

    End Sub

    ''' <summary>
    ''' T00015tbl追加
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub DBupdate_T15INSERT(ByVal I_DATENOW As Date, ByRef O_RTN As String)

        'DataBase接続文字
        Dim SQLcon = CS0050SESSION.getConnection
        SQLcon.Open() 'DataBase接続(Open)

        Dim WW_SORTstr As String = ""
        Dim WW_FILLstr As String = ""

        Dim WW_TORICODE As String = ""
        Dim WW_OILTYPE As String = ""
        Dim WW_SHUKADATE As String = ""
        Dim WW_KIJUNDATE As String = ""
        Dim WW_SHIPORG As String = ""


        '■■■ T00015UPDtblより配送受注追加 ■■■
        '
        For Each T00015UPDrow In T00015UPDtbl.Rows

            If T00015UPDrow("DELFLG") = "0" AndAlso
                (T00015UPDrow("OPERATION") = C_LIST_OPERATION_CODE.UPDATING OrElse T00015UPDrow("OPERATION") = C_LIST_OPERATION_CODE.WARNING) Then
                Try
                    '〇配送受注DB登録
                    Dim SQLStr As String =
                               " INSERT INTO T0015_SUPPLJISSKI              " _
                             & "             (CAMPCODE,                     " _
                             & "              ORDERNO,                      " _
                             & "              DETAILNO,                     " _
                             & "              TRIPNO,                       " _
                             & "              DROPNO,                       " _
                             & "              SEQ,                          " _
                             & "              ENTRYDATE,                    " _
                             & "              TORICODE,                     " _
                             & "              OILTYPE,                      " _
                             & "              SHUKODATE,                    " _
                             & "              KIKODATE,                     " _
                             & "              SHUKADATE,                    " _
                             & "              SHIPORG,                      " _
                             & "              SHUKABASHO,                   " _
                             & "              GSHABAN,                      " _
                             & "              RYOME,                        " _
                             & "              SHAFUKU,                      " _
                             & "              STAFFCODE,                    " _
                             & "              SUBSTAFFCODE,                 " _
                             & "              TODOKEDATE,                   " _
                             & "              TODOKECODE,                   " _
                             & "              PRODUCT1,                     " _
                             & "              PRODUCT2,                     " _
                             & "              CONTNO,                       " _
                             & "              JSURYO,                       " _
                             & "              JDAISU,                       " _
                             & "              REMARKS1,                     " _
                             & "              REMARKS2,                     " _
                             & "              REMARKS3,                     " _
                             & "              REMARKS4,                     " _
                             & "              REMARKS5,                     " _
                             & "              REMARKS6,                     " _
                             & "              DELFLG,                       " _
                             & "              INITYMD,                      " _
                             & "              UPDYMD,                       " _
                             & "              UPDUSER,                      " _
                             & "              UPDTERMID,                    " _
                             & "              RECEIVEYMD,                   " _
                             & "              KIJUNDATE,                    " _
                             & "              SHARYOTYPEF,                  " _
                             & "              TSHABANF,                     " _
                             & "              SHARYOTYPEB,                  " _
                             & "              TSHABANB,                     " _
                             & "              SHARYOTYPEB2,                 " _
                             & "              TSHABANB2,                    " _
                             & "              STANI,                        " _
                             & "              PRODUCTCODE,                  " _
                             & "              JISSEKIKBN)                   " _
                             & "      VALUES (@P01,@P02,@P03,@P04,@P05,@P06,@P07,@P08,@P09,@P10,     " _
                             & "              @P11,@P12,@P13,@P14,@P15,@P16,@P17,@P18,@P19,@P20,     " _
                             & "              @P21,@P22,@P23,@P24,@P25,@P26,@P27,@P28,@P29,@P30,     " _
                             & "              @P31,@P32,@P33,@P34,@P35,@P36,@P37,@P38,@P39,@P40,     " _
                             & "              @P41,@P42,@P43,@P44,@P45,@P46,@P47,@P48                " _
                             & "              );    "

                    Dim SQLcmd As New SqlCommand(SQLStr, SQLcon)
                    Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", System.Data.SqlDbType.NVarChar, 20)
                    Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", System.Data.SqlDbType.NVarChar, 10)
                    Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@P03", System.Data.SqlDbType.NVarChar, 10)
                    Dim PARA04 As SqlParameter = SQLcmd.Parameters.Add("@P04", System.Data.SqlDbType.NVarChar, 10)
                    Dim PARA05 As SqlParameter = SQLcmd.Parameters.Add("@P05", System.Data.SqlDbType.NVarChar, 10)
                    Dim PARA06 As SqlParameter = SQLcmd.Parameters.Add("@P06", System.Data.SqlDbType.NVarChar, 2)
                    Dim PARA07 As SqlParameter = SQLcmd.Parameters.Add("@P07", System.Data.SqlDbType.NVarChar, 25)
                    Dim PARA08 As SqlParameter = SQLcmd.Parameters.Add("@P08", System.Data.SqlDbType.NVarChar, 20)
                    Dim PARA09 As SqlParameter = SQLcmd.Parameters.Add("@P09", System.Data.SqlDbType.NVarChar, 20)
                    Dim PARA10 As SqlParameter = SQLcmd.Parameters.Add("@P10", System.Data.SqlDbType.DateTime)
                    Dim PARA11 As SqlParameter = SQLcmd.Parameters.Add("@P11", System.Data.SqlDbType.DateTime)
                    Dim PARA12 As SqlParameter = SQLcmd.Parameters.Add("@P12", System.Data.SqlDbType.DateTime)
                    Dim PARA13 As SqlParameter = SQLcmd.Parameters.Add("@P13", System.Data.SqlDbType.NVarChar, 20)
                    Dim PARA14 As SqlParameter = SQLcmd.Parameters.Add("@P14", System.Data.SqlDbType.NVarChar, 20)
                    Dim PARA15 As SqlParameter = SQLcmd.Parameters.Add("@P15", System.Data.SqlDbType.NVarChar, 20)
                    Dim PARA16 As SqlParameter = SQLcmd.Parameters.Add("@P16", System.Data.SqlDbType.NVarChar, 1)
                    Dim PARA17 As SqlParameter = SQLcmd.Parameters.Add("@P17", System.Data.SqlDbType.Decimal)
                    Dim PARA18 As SqlParameter = SQLcmd.Parameters.Add("@P18", System.Data.SqlDbType.NVarChar, 20)
                    Dim PARA19 As SqlParameter = SQLcmd.Parameters.Add("@P19", System.Data.SqlDbType.NVarChar, 20)
                    Dim PARA20 As SqlParameter = SQLcmd.Parameters.Add("@P20", System.Data.SqlDbType.DateTime)
                    Dim PARA21 As SqlParameter = SQLcmd.Parameters.Add("@P21", System.Data.SqlDbType.NVarChar, 20)
                    Dim PARA22 As SqlParameter = SQLcmd.Parameters.Add("@P22", System.Data.SqlDbType.NVarChar, 20)
                    Dim PARA23 As SqlParameter = SQLcmd.Parameters.Add("@P23", System.Data.SqlDbType.NVarChar, 20)
                    Dim PARA24 As SqlParameter = SQLcmd.Parameters.Add("@P24", System.Data.SqlDbType.NVarChar, 20)
                    Dim PARA25 As SqlParameter = SQLcmd.Parameters.Add("@P25", System.Data.SqlDbType.Decimal)
                    Dim PARA26 As SqlParameter = SQLcmd.Parameters.Add("@P26", System.Data.SqlDbType.Int)
                    Dim PARA27 As SqlParameter = SQLcmd.Parameters.Add("@P27", System.Data.SqlDbType.NVarChar, 50)
                    Dim PARA28 As SqlParameter = SQLcmd.Parameters.Add("@P28", System.Data.SqlDbType.NVarChar, 50)
                    Dim PARA29 As SqlParameter = SQLcmd.Parameters.Add("@P29", System.Data.SqlDbType.NVarChar, 50)
                    Dim PARA30 As SqlParameter = SQLcmd.Parameters.Add("@P30", System.Data.SqlDbType.NVarChar, 50)
                    Dim PARA31 As SqlParameter = SQLcmd.Parameters.Add("@P31", System.Data.SqlDbType.NVarChar, 50)
                    Dim PARA32 As SqlParameter = SQLcmd.Parameters.Add("@P32", System.Data.SqlDbType.NVarChar, 50)
                    Dim PARA33 As SqlParameter = SQLcmd.Parameters.Add("@P33", System.Data.SqlDbType.NVarChar, 1)
                    Dim PARA34 As SqlParameter = SQLcmd.Parameters.Add("@P34", System.Data.SqlDbType.DateTime)
                    Dim PARA35 As SqlParameter = SQLcmd.Parameters.Add("@P35", System.Data.SqlDbType.DateTime)
                    Dim PARA36 As SqlParameter = SQLcmd.Parameters.Add("@P36", System.Data.SqlDbType.NVarChar, 20)
                    Dim PARA37 As SqlParameter = SQLcmd.Parameters.Add("@P37", System.Data.SqlDbType.NVarChar, 30)
                    Dim PARA38 As SqlParameter = SQLcmd.Parameters.Add("@P38", System.Data.SqlDbType.DateTime)
                    Dim PARA39 As SqlParameter = SQLcmd.Parameters.Add("@P39", System.Data.SqlDbType.DateTime)
                    Dim PARA40 As SqlParameter = SQLcmd.Parameters.Add("@P40", System.Data.SqlDbType.NVarChar, 1)
                    Dim PARA41 As SqlParameter = SQLcmd.Parameters.Add("@P41", System.Data.SqlDbType.NVarChar, 20)
                    Dim PARA42 As SqlParameter = SQLcmd.Parameters.Add("@P42", System.Data.SqlDbType.NVarChar, 1)
                    Dim PARA43 As SqlParameter = SQLcmd.Parameters.Add("@P43", System.Data.SqlDbType.NVarChar, 20)
                    Dim PARA44 As SqlParameter = SQLcmd.Parameters.Add("@P44", System.Data.SqlDbType.NVarChar, 1)
                    Dim PARA45 As SqlParameter = SQLcmd.Parameters.Add("@P45", System.Data.SqlDbType.NVarChar, 20)
                    Dim PARA46 As SqlParameter = SQLcmd.Parameters.Add("@P46", System.Data.SqlDbType.NVarChar, 20)
                    Dim PARA47 As SqlParameter = SQLcmd.Parameters.Add("@P47", System.Data.SqlDbType.NVarChar, 20)
                    Dim PARA48 As SqlParameter = SQLcmd.Parameters.Add("@P48", System.Data.SqlDbType.NVarChar, 1)

                    PARA01.Value = T00015UPDrow("CAMPCODE")                           '会社コード(CAMPCODE)
                    PARA02.Value = T00015UPDrow("ORDERNO").PadLeft(7, "0")            '受注番号(ORDERNO)
                    PARA03.Value = T00015UPDrow("DETAILNO").PadLeft(3, "0")           '明細№(DETAILNO)
                    PARA04.Value = T00015UPDrow("TRIPNO").PadLeft(3, "0")             'トリップ(TRIPNO)
                    PARA05.Value = T00015UPDrow("DROPNO").PadLeft(3, "0")             'ドロップ(DROPNO)
                    PARA06.Value = T00015UPDrow("SEQ").PadLeft(2, "0")                '枝番(SEQ)
                    PARA07.Value = I_DATENOW.ToString("yyyyMMddHHmmssfff")            'エントリー日時(ENTRYDATE)
                    PARA08.Value = T00015UPDrow("TORICODE")                           '取引先コード(TORICODE)
                    PARA09.Value = T00015UPDrow("OILTYPE")                            '油種(OILTYPE)
                    If T00015UPDrow("SHUKODATE") = "" Then                            '出庫日(SHUKODATE)
                        PARA10.Value = "2000/01/01"
                    Else
                        PARA10.Value = RTrim(T00015UPDrow("SHUKODATE"))
                    End If
                    If T00015UPDrow("KIKODATE") = "" Then                             '帰庫日(KIKODATE)
                        PARA11.Value = "2000/01/01"
                    Else
                        PARA11.Value = RTrim(T00015UPDrow("KIKODATE"))
                    End If
                    If T00015UPDrow("SHUKADATE") = "" Then                            '出荷日(SHUKADATE)
                        PARA12.Value = "2000/01/01"
                    Else
                        PARA12.Value = RTrim(T00015UPDrow("SHUKADATE"))
                    End If
                    PARA13.Value = T00015UPDrow("SHIPORG")                            '出荷部署(SHIPORG)
                    PARA14.Value = T00015UPDrow("SHUKABASHO")                         '出荷場所(SHUKABASHO)
                    PARA15.Value = T00015UPDrow("GSHABAN")                            '業務車番(GSHABAN)
                    PARA16.Value = T00015UPDrow("RYOME")                              '両目(RYOME)
                    If String.IsNullOrWhiteSpace(RTrim(T00015UPDrow("SHAFUKU"))) Then '車腹（積載量）(SHAFUKU)
                        PARA17.Value = 0.0
                    Else
                        PARA17.Value = CType(T00015UPDrow("SHAFUKU"), Double)
                    End If
                    PARA18.Value = T00015UPDrow("STAFFCODE")                          '乗務員コード(STAFFCODE)
                    PARA19.Value = T00015UPDrow("SUBSTAFFCODE")                       '副乗務員コード(SUBSTAFFCODE)
                    If RTrim(T00015UPDrow("TODOKEDATE")) = "" Then                    '届日(TODOKEDATE)
                        PARA20.Value = "2000/01/01"
                    Else
                        PARA20.Value = RTrim(T00015UPDrow("TODOKEDATE"))
                    End If
                    PARA21.Value = T00015UPDrow("TODOKECODE")                         '届先コード(TODOKECODE)
                    PARA22.Value = T00015UPDrow("PRODUCT1")                           '品名１(PRODUCT1)
                    PARA23.Value = T00015UPDrow("PRODUCT2")                           '品名２(PRODUCT2)
                    PARA24.Value = T00015UPDrow("CONTNO")                             'コンテナ番号(CONTNO)
                    If String.IsNullOrWhiteSpace(RTrim(T00015UPDrow("JSURYO"))) Then   '配送実績数量(JSURYO)
                        PARA25.Value = 0.0
                    Else
                        PARA25.Value = CType(T00015UPDrow("JSURYO"), Double)
                    End If
                    If String.IsNullOrWhiteSpace(RTrim(T00015UPDrow("JDAISU"))) Then   '配送実績台数(JDAISU)
                        PARA26.Value = 0
                    Else
                        PARA26.Value = CType(T00015UPDrow("JDAISU"), Double)
                    End If
                    PARA27.Value = T00015UPDrow("REMARKS1")                           '備考１(REMARKS1)
                    PARA28.Value = T00015UPDrow("REMARKS2")                           '備考２(REMARKS2)
                    PARA29.Value = T00015UPDrow("REMARKS3")                           '備考３(REMARKS3)
                    PARA30.Value = T00015UPDrow("REMARKS4")                           '備考４(REMARKS4)
                    PARA31.Value = T00015UPDrow("REMARKS5")                           '備考５(REMARKS5)
                    PARA32.Value = T00015UPDrow("REMARKS6")                           '備考６(REMARKS6)
                    PARA33.Value = T00015UPDrow("DELFLG")                             '削除フラグ(DELFLG)
                    PARA34.Value = I_DATENOW                                          '登録年月日(INITYMD)
                    PARA35.Value = I_DATENOW                                          '更新年月日(UPDYMD)
                    PARA36.Value = Master.USERID                                      '更新ユーザＩＤ(UPDUSER)
                    PARA37.Value = Master.USERTERMID                                  '更新端末(UPDTERMID)
                    PARA38.Value = C_DEFAULT_YMD                                      '集信日時(RECEIVEYMD)

                    '基準日＝出荷日
                    If T00015UPDrow("KIJUNDATE") = "" Then                            '基準日(KIJUNDATE)
                        PARA39.Value = "2000/01/01"
                    Else
                        PARA39.Value = RTrim(T00015UPDrow("KIJUNDATE"))
                    End If
                    PARA40.Value = T00015UPDrow("SHARYOTYPEF")                        '統一車番前(SHARYOTYPEF)
                    PARA41.Value = T00015UPDrow("TSHABANF")                           '統一車番前(TSHABANF)
                    PARA42.Value = T00015UPDrow("SHARYOTYPEB")                        '統一車番前(SHARYOTYPEB)
                    PARA43.Value = T00015UPDrow("TSHABANB")                           '統一車番前(TSHABANB)
                    PARA44.Value = T00015UPDrow("SHARYOTYPEB2")                       '統一車番前(SHARYOTYPEB2)
                    PARA45.Value = T00015UPDrow("TSHABANB2")                          '統一車番前(TSHABANB2)
                    PARA46.Value = ""                                                 '配送実績単位(STANI)
                    PARA47.Value = T00015UPDrow("PRODUCTCODE")                        '品名コード(PRODUCTCODE)
                    PARA48.Value = T00015UPDrow("JISSEKIKBN")                         '実績区分(JISSEKIKBN)

                    SQLcmd.CommandTimeout = 300
                    SQLcmd.ExecuteNonQuery()

                    'CLOSE
                    SQLcmd.Dispose()
                    SQLcmd = Nothing

                Catch ex As Exception
                    Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "T0015_SUPPLJISSKI INSERT")
                    CS0011LOGWRITE.INFSUBCLASS = "MAIN"                         'SUBクラス名
                    CS0011LOGWRITE.INFPOSI = "DB:T0015_SUPPLJISSKI INSERT"           '
                    CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                  '
                    CS0011LOGWRITE.TEXT = ex.ToString()
                    CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                    CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
                    O_RTN = C_MESSAGE_NO.DB_ERROR
                    Exit Sub

                End Try

                '〇配送受注登録結果を画面情報へ戻す
                For Each T00015row In T00015tbl.Rows
                    If T00015row("CAMPCODE") = T00015UPDrow("CAMPCODE") AndAlso
                       T00015row("TORICODE") = T00015UPDrow("TORICODE") AndAlso
                       T00015row("OILTYPE") = T00015UPDrow("OILTYPE") AndAlso
                       T00015row("KIJUNDATE") = T00015UPDrow("KIJUNDATE") AndAlso
                       T00015row("SHIPORG") = T00015UPDrow("SHIPORG") AndAlso
                       T00015row("SHUKODATE") = T00015UPDrow("SHUKODATE") AndAlso
                       T00015row("GSHABAN") = T00015UPDrow("GSHABAN") AndAlso
                       T00015row("TRIPNO") = T00015UPDrow("TRIPNO") AndAlso
                       T00015row("DROPNO") = T00015UPDrow("DROPNO") AndAlso
                       T00015row("SEQ") = T00015UPDrow("SEQ") AndAlso
                       T00015row("DELFLG") <> "1" Then

                        T00015row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                        T00015row("ORDERNO") = T00015UPDrow("ORDERNO")
                        T00015row("DETAILNO") = T00015UPDrow("DETAILNO")
                        Exit For

                    End If
                Next
                Try
                    '更新結果(TIMSTP)再取得 …　連続処理を可能にする。
                    Dim SQLStr As String =
                               " SELECT CAST(UPDTIMSTP as bigint) as TIMSTP    " _
                             & "   FROM T0015_SUPPLJISSKI                      " _
                             & "  WHERE CAMPCODE       = @P01                  " _
                             & "    and TORICODE       = @P02                  " _
                             & "    and OILTYPE        = @P03                  " _
                             & "    and KIJUNDATE      = @P04                  " _
                             & "    and SHIPORG        = @P05                  " _
                             & "    and SHUKODATE      = @P06                  " _
                             & "    and GSHABAN        = @P07                  " _
                             & "    and TRIPNO         = @P08                  " _
                             & "    and DROPNO         = @P09                  " _
                             & "    and SEQ            = @P10                  " _
                             & "    and DELFLG        <> '1'                   "

                    Dim SQLcmd As New SqlCommand(SQLStr, SQLcon)
                    Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", System.Data.SqlDbType.NVarChar)
                    Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", System.Data.SqlDbType.NVarChar)
                    Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@P03", System.Data.SqlDbType.NVarChar)
                    Dim PARA04 As SqlParameter = SQLcmd.Parameters.Add("@P04", System.Data.SqlDbType.Date)
                    Dim PARA05 As SqlParameter = SQLcmd.Parameters.Add("@P05", System.Data.SqlDbType.NVarChar)
                    Dim PARA06 As SqlParameter = SQLcmd.Parameters.Add("@P06", System.Data.SqlDbType.Date)
                    Dim PARA07 As SqlParameter = SQLcmd.Parameters.Add("@P07", System.Data.SqlDbType.NVarChar)
                    Dim PARA08 As SqlParameter = SQLcmd.Parameters.Add("@P08", System.Data.SqlDbType.NVarChar)
                    Dim PARA09 As SqlParameter = SQLcmd.Parameters.Add("@P09", System.Data.SqlDbType.NVarChar)
                    Dim PARA10 As SqlParameter = SQLcmd.Parameters.Add("@P10", System.Data.SqlDbType.NVarChar)

                    PARA01.Value = T00015UPDrow("CAMPCODE")
                    PARA02.Value = T00015UPDrow("TORICODE")
                    PARA03.Value = T00015UPDrow("OILTYPE")
                    PARA04.Value = T00015UPDrow("KIJUNDATE")
                    PARA05.Value = T00015UPDrow("SHIPORG")
                    PARA06.Value = T00015UPDrow("SHUKODATE")
                    PARA07.Value = T00015UPDrow("GSHABAN")
                    PARA08.Value = T00015UPDrow("TRIPNO")
                    PARA09.Value = T00015UPDrow("DROPNO")
                    PARA10.Value = T00015UPDrow("SEQ")

                    Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

                    '画面情報へタイムスタンプ・受注番号をフィードバック
                    While SQLdr.Read
                        For Each T00015row In T00015tbl.Rows
                            If T00015row("CAMPCODE") = T00015UPDrow("CAMPCODE") AndAlso
                               T00015row("TORICODE") = T00015UPDrow("TORICODE") AndAlso
                               T00015row("OILTYPE") = T00015UPDrow("OILTYPE") AndAlso
                               T00015row("KIJUNDATE") = T00015UPDrow("KIJUNDATE") AndAlso
                               T00015row("SHIPORG") = T00015UPDrow("SHIPORG") AndAlso
                               T00015row("SHUKODATE") = T00015UPDrow("SHUKODATE") AndAlso
                               T00015row("GSHABAN") = T00015UPDrow("GSHABAN") AndAlso
                               T00015row("TRIPNO") = T00015UPDrow("TRIPNO") AndAlso
                               T00015row("DROPNO") = T00015UPDrow("DROPNO") AndAlso
                               T00015row("SEQ") = T00015UPDrow("SEQ") AndAlso
                               T00015row("DELFLG") <> C_DELETE_FLG.DELETE Then

                                T00015row("TIMSTP") = SQLdr("TIMSTP")
                                T00015row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                                T00015row("ORDERNO") = T00015UPDrow("ORDERNO")
                                T00015row("DETAILNO") = T00015UPDrow("DETAILNO")
                                Exit For

                            End If
                        Next
                    End While

                    'Close()
                    SQLdr.Close() 'Reader(Close)
                    SQLdr = Nothing

                    SQLcmd.Dispose()
                    SQLcmd = Nothing

                Catch ex As Exception
                    Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "T0015_SUPPLJISSKI SELECT")
                    CS0011LOGWRITE.INFSUBCLASS = "MAIN"                         'SUBクラス名
                    CS0011LOGWRITE.INFPOSI = "DB:T0015_SUPPLJISSKI SELECT"           '
                    CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                  '
                    CS0011LOGWRITE.TEXT = ex.ToString()
                    CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                    CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
                    Exit Sub

                End Try

            End If

        Next

        '更新→クリア
        For Each T00015row In T00015tbl.Rows
            If T00015row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING Then
                T00015row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
            End If
        Next

        SQLcon.Close()
        SQLcon.Dispose()

        O_RTN = C_MESSAGE_NO.NORMAL

    End Sub

    ' ***  T00015UPDtbl更新データ（画面表示受注+画面非表示受注）作成　＆　タイムスタンプチェック処理          済
    Protected Sub DBupdate_T00015UPDtblget(ByVal O_RTN As String)

        '更新対象受注の画面非表示（他出庫日）を取得。配送受注の更新最小単位は出荷部署単位。

        Dim WW_SORTstr As String = ""
        Dim WW_FILLstr As String = ""

        Dim WW_TORICODE As String = ""
        Dim WW_OILTYPE As String = ""
        Dim WW_SHUKADATE As String = ""
        Dim WW_KIJUNDATE As String = ""
        Dim WW_SHIPORG As String = ""

        Dim WW_SHUKODATE As String = ""
        Dim WW_GSHABAN As String = ""
        Dim WW_TRIPNO As String = ""
        Dim WW_DROPNO As String = ""

        '■■■ 更新前処理（入力情報へ操作を反映）　■■■

        For Each T00015row In T00015tbl.Rows
            '削除チェックがONの時、削除更新
            If T00015row("ROWDEL") = "1" Then
                If T00015row("OPERATION") <> C_LIST_OPERATION_CODE.ERRORED Then

                    T00015row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                    T00015row("DELFLG") = C_DELETE_FLG.DELETE
                    T00015row("HIDDEN") = 1
                End If
            End If

            If T00015row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING OrElse
                T00015row("OPERATION") = C_LIST_OPERATION_CODE.WARNING Then

                For j As Integer = 0 To T00015tbl.Rows.Count - 1
                    '取引先、油種、基準日（出荷日or届日）、受注部署、出荷部署が同一
                    If T00015tbl.Rows(j)("TORICODE") = T00015row("TORICODE") AndAlso
                       T00015tbl.Rows(j)("OILTYPE") = T00015row("OILTYPE") AndAlso
                       T00015tbl.Rows(j)("KIJUNDATE") = T00015row("KIJUNDATE") AndAlso
                       T00015tbl.Rows(j)("SHIPORG") = T00015row("SHIPORG") AndAlso
                       T00015tbl.Rows(j)("DELFLG") <> "1" Then

                        T00015tbl.Rows(j)("OPERATION") = C_LIST_OPERATION_CODE.UPDATING

                    End If
                Next
            End If

        Next

        '■■■ 受注最新レコード(DB格納)をT00015UPDtblへ格納 ■■■

        'Sort
        CS0026TBLSORTget.TABLE = T00015tbl
        CS0026TBLSORTget.SORTING = "TORICODE ,OILTYPE ,KIJUNDATE ,SHIPORG"
        CS0026TBLSORTget.FILTER = ""
        CS0026TBLSORTget.Sort(T00015tbl)
        '○作業用DBのカラム設定
        '更新元データ
        Master.CreateEmptyTable(T00015UPDtbl)
        '作業用データ
        Master.CreateEmptyTable(T00015WKtbl)

        '○更新対象受注のDB格納レコードを全て取得
        For Each T00015row In T00015tbl.Rows

            If T00015row("TORICODE") = WW_TORICODE AndAlso
               T00015row("OILTYPE") = WW_OILTYPE AndAlso
               T00015row("KIJUNDATE") = WW_KIJUNDATE AndAlso
               T00015row("SHIPORG") = WW_SHIPORG Then
            Else
                If T00015row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING OrElse
                    T00015row("OPERATION") = C_LIST_OPERATION_CODE.WARNING Then
                    T00015WKtbl.Clear()

                    'オブジェクト内容検索
                    Try
                        'DataBase接続文字
                        Dim SQLcon = CS0050SESSION.getConnection
                        SQLcon.Open() 'DataBase接続(Open)

                        '検索SQL文
                        Dim SQLStr As String =
                             "SELECT isnull(rtrim(A.CAMPCODE),'')          as CAMPCODE ,       " _
                           & "       isnull(rtrim(A.ORDERNO),'')           as ORDERNO ,        " _
                           & "       isnull(rtrim(A.DETAILNO),'')          as DETAILNO ,       " _
                           & "       isnull(rtrim(A.TRIPNO),'')            as TRIPNO ,         " _
                           & "       isnull(rtrim(A.DROPNO),'')            as DROPNO ,         " _
                           & "       isnull(rtrim(A.SEQ),'')               as SEQ ,            " _
                           & "       isnull(rtrim(A.TORICODE),'')          as TORICODE ,       " _
                           & "       isnull(rtrim(A.OILTYPE),'')           as OILTYPE ,        " _
                           & "       isnull(format(A.SHUKODATE, 'yyyy/MM/dd'),'') as SHUKODATE , " _
                           & "       isnull(format(A.KIKODATE,  'yyyy/MM/dd'),'') as KIKODATE  , " _
                           & "       isnull(format(A.KIJUNDATE, 'yyyy/MM/dd'),'') as KIJUNDATE , " _
                           & "       isnull(format(A.SHUKADATE, 'yyyy/MM/dd'),'') as SHUKADATE , " _
                           & "       isnull(rtrim(A.SHIPORG),'')           as SHIPORG ,        " _
                           & "       isnull(rtrim(A.SHUKABASHO),'')        as SHUKABASHO ,     " _
                           & "       isnull(rtrim(A.GSHABAN),'')           as GSHABAN ,        " _
                           & "       isnull(rtrim(A.RYOME),'')             as RYOME ,          " _
                           & "       isnull(rtrim(A.SHAFUKU),'')           as SHAFUKU ,        " _
                           & "       isnull(rtrim(A.STAFFCODE),'')         as STAFFCODE ,      " _
                           & "       isnull(rtrim(A.SUBSTAFFCODE),'')      as SUBSTAFFCODE ,   " _
                           & "       isnull(format(A.TODOKEDATE, 'yyyy/MM/dd'),'') as TODOKEDATE , " _
                           & "       isnull(rtrim(A.TODOKECODE),'')        as TODOKECODE ,     " _
                           & "       isnull(rtrim(A.PRODUCT1),'')          as PRODUCT1 ,       " _
                           & "       isnull(rtrim(A.PRODUCT2),'')          as PRODUCT2 ,       " _
                           & "       isnull(rtrim(A.PRODUCTCODE),'')       as PRODUCTCODE ,    " _
                           & "       isnull(rtrim(A.CONTNO),'')            as CONTNO ,         " _
                           & "       isnull(rtrim(A.JSURYO),'')            as JSURYO ,         " _
                           & "       isnull(rtrim(A.JDAISU),'')            as JDAISU ,         " _
                           & "       isnull(rtrim(A.REMARKS1),'')          as REMARKS1 ,       " _
                           & "       isnull(rtrim(A.REMARKS2),'')          as REMARKS2 ,       " _
                           & "       isnull(rtrim(A.REMARKS3),'')          as REMARKS3 ,       " _
                           & "       isnull(rtrim(A.REMARKS4),'')          as REMARKS4 ,       " _
                           & "       isnull(rtrim(A.REMARKS5),'')          as REMARKS5 ,       " _
                           & "       isnull(rtrim(A.REMARKS6),'')          as REMARKS6 ,       " _
                           & "       isnull(rtrim(A.SHARYOTYPEF),'')       as SHARYOTYPEF ,    " _
                           & "       isnull(rtrim(A.TSHABANF),'')          as TSHABANF ,       " _
                           & "       isnull(rtrim(A.SHARYOTYPEB),'')       as SHARYOTYPEB ,    " _
                           & "       isnull(rtrim(A.TSHABANB),'')          as TSHABANB ,       " _
                           & "       isnull(rtrim(A.SHARYOTYPEB2),'')      as SHARYOTYPEB2 ,   " _
                           & "       isnull(rtrim(A.TSHABANB2),'')         as TSHABANB2 ,      " _
                           & "       isnull(rtrim(A.JISSEKIKBN),'')        as JISSEKIKBN ,     " _
                           & "       isnull(rtrim(A.DELFLG),'')            as DELFLG ,         " _
                           & "       TIMSTP = cast(A.UPDTIMSTP  as bigint) ,        " _
                           & "       isnull(rtrim(B.SHARYOINFO1),'')       as SHARYOINFO1 ,    " _
                           & "       isnull(rtrim(B.SHARYOINFO2),'')       as SHARYOINFO2 ,    " _
                           & "       isnull(rtrim(B.SHARYOINFO3),'')       as SHARYOINFO3 ,    " _
                           & "       isnull(rtrim(B.SHARYOINFO4),'')       as SHARYOINFO4 ,    " _
                           & "       isnull(rtrim(B.SHARYOINFO5),'')       as SHARYOINFO5 ,    " _
                           & "       isnull(rtrim(B.SHARYOINFO6),'')       as SHARYOINFO6 ,    " _
                           & "       isnull(rtrim(D.ADDR1),'') +              					" _
                           & "       isnull(rtrim(D.ADDR2),'') +            					" _
                           & "       isnull(rtrim(D.ADDR3),'') +             					" _
                           & "       isnull(rtrim(D.ADDR4),'')          	as ADDR ,           " _
                           & "       isnull(rtrim(D.NOTES1),'')        	    as NOTES1 ,       	" _
                           & "       isnull(rtrim(D.NOTES2),'')          	as NOTES2 ,       	" _
                           & "       isnull(rtrim(D.NOTES3),'')          	as NOTES3 ,       	" _
                           & "       isnull(rtrim(D.NOTES4),'')          	as NOTES4 ,       	" _
                           & "       isnull(rtrim(D.NOTES5),'')          	as NOTES5 ,       	" _
                           & "       isnull(rtrim(E.NOTES1),'')        	    as STAFFNOTES1 ,   	" _
                           & "       isnull(rtrim(E.NOTES2),'')          	as STAFFNOTES2 ,   	" _
                           & "       isnull(rtrim(E.NOTES3),'')          	as STAFFNOTES3 ,   	" _
                           & "       isnull(rtrim(E.NOTES4),'')          	as STAFFNOTES4 ,   	" _
                           & "       isnull(rtrim(E.NOTES5),'')          	as STAFFNOTES5     	" _
                           & "  FROM T0015_SUPPLJISSKI AS A							" _
                           & "  LEFT JOIN MA006_SHABANORG B 						" _
                           & "    ON B.CAMPCODE     	= A.CAMPCODE 				" _
                           & "   and B.GSHABAN      	= A.GSHABAN 				" _
                           & "   and B.MANGUORG     	= A.SHIPORG 				" _
                           & "   and B.DELFLG          <> '1' 						" _
                           & "  LEFT JOIN MC007_TODKORG C 							" _
                           & "    ON C.CAMPCODE     	= A.CAMPCODE 				" _
                           & "   and C.TORICODE     	= A.TORICODE 				" _
                           & "   and C.TODOKECODE   	= A.TODOKECODE 				" _
                           & "   and C.UORG         	= A.SHIPORG 				" _
                           & "   and C.DELFLG          <> '1' 						" _
                           & "  LEFT JOIN MC006_TODOKESAKI D 						" _
                           & "    ON D.CAMPCODE     	= C.CAMPCODE 				" _
                           & "   and D.TORICODE     	= C.TORICODE				" _
                           & "   and D.TODOKECODE   	= C.TODOKECODE 				" _
                           & "   and D.STYMD           <= A.SHUKODATE				" _
                           & "   and D.ENDYMD          >= A.SHUKODATE				" _
                           & "   and D.DELFLG          <> '1' 						" _
                           & "  LEFT JOIN MB001_STAFF E     						" _
                           & "    ON E.CAMPCODE     	= A.CAMPCODE 				" _
                           & "   and E.STAFFCODE     	= A.STAFFCODE				" _
                           & "   and E.STYMD           <= A.SHUKODATE				" _
                           & "   and E.ENDYMD          >= A.SHUKODATE				" _
                           & "   and E.DELFLG          <> '1' 						" _
                           & " WHERE A.CAMPCODE         = @P01                      " _
                           & "  and  A.TORICODE         = @P02                      " _
                           & "  and  A.OILTYPE          = @P03           		    " _
                           & "  and  A.SHIPORG          = @P04           		    " _
                           & "  and  A.KIJUNDATE        = @P05                      " _
                           & "  and  A.DELFLG          <> '1'                       " _
                           & " ORDER BY A.TORICODE  ,A.OILTYPE ,A.KIJUNDATE ,       " _
                           & " 		    A.SHIPORG ,A.GSHABAN           "

                        Dim SQLcmd As New SqlCommand(SQLStr, SQLcon)
                        Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", System.Data.SqlDbType.NVarChar, 20)
                        Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", System.Data.SqlDbType.NVarChar, 20)  '荷主
                        Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@P03", System.Data.SqlDbType.NVarChar, 20)  '油種
                        Dim PARA04 As SqlParameter = SQLcmd.Parameters.Add("@P04", System.Data.SqlDbType.NVarChar, 20)  '出荷部署
                        Dim PARA05 As SqlParameter = SQLcmd.Parameters.Add("@P05", System.Data.SqlDbType.Date)      '出荷日

                        '○関連受注指定
                        PARA01.Value = T00015row("CAMPCODE")        '会社
                        PARA02.Value = T00015row("TORICODE")        '出荷日
                        PARA03.Value = T00015row("OILTYPE")         '油種
                        PARA04.Value = T00015row("SHIPORG")         '出荷部署
                        PARA05.Value = T00015row("KIJUNDATE")       '基準日

                        '■SQL実行
                        Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

                        '■テーブル検索結果をテーブル格納
                        T00015WKtbl.Load(SQLdr)
                        T00015UPDtbl.Merge(T00015WKtbl, False)
                        For Each T00015UPDrow In T00015UPDtbl.Rows
                            T00015UPDrow("LINECNT") = 0
                            T00015UPDrow("SELECT") = 1
                            T00015UPDrow("HIDDEN") = 0
                            T00015UPDrow("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                        Next

                        SQLdr.Close()
                        SQLdr = Nothing

                        SQLcmd.Dispose()
                        SQLcmd = Nothing

                        SQLcon.Close() 'DataBase接続(Close)
                        SQLcon.Dispose()
                        SQLcon = Nothing

                    Catch ex As Exception
                        CS0011LOGWRITE.INFSUBCLASS = "DBupdate_T00015UPDtblget"     'SUBクラス名
                        CS0011LOGWRITE.INFPOSI = "T0015_SUPPLJISSKI UPDATE"
                        CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
                        CS0011LOGWRITE.TEXT = ex.ToString()
                        CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                        CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力

                        O_RTN = C_MESSAGE_NO.DB_ERROR
                        Exit Sub

                    End Try

                    WW_TORICODE = T00015row("TORICODE")
                    WW_OILTYPE = T00015row("OILTYPE")
                    WW_KIJUNDATE = T00015row("KIJUNDATE")
                    WW_SHIPORG = T00015row("SHIPORG")

                Else
                End If

            End If
        Next

        '■■■ 受注番号　自動採番 ■■■                  

        'Sort(T00015tbl)
        CS0026TBLSORTget.TABLE = T00015tbl
        CS0026TBLSORTget.SORTING = "TORICODE ,OILTYPE ,KIJUNDATE  ,SHIPORG"
        CS0026TBLSORTget.FILTER = ""
        CS0026TBLSORTget.Sort(T00015tbl)

        '○　受注番号　自動採番
        For i As Integer = 0 To T00015tbl.Rows.Count - 1

            Dim T00015row = T00015tbl.Rows(i)


            If T00015row("ORDERNO").ToString.Contains("新") Then
                CS0033AutoNumber.SEQTYPE = CS0033AutoNumber.C_SEQTYPE.ORDERNO
                CS0033AutoNumber.CAMPCODE = work.WF_SEL_CAMPCODE.Text
                CS0033AutoNumber.MORG = T00015row("SHIPORG")
                CS0033AutoNumber.USERID = Master.USERID
                CS0033AutoNumber.getAutoNumber()

                If isNormal(CS0033AutoNumber.ERR) Then
                    '他レコードへ反映
                    For j As Integer = i To T00015tbl.Rows.Count - 1
                        If T00015tbl.Rows(j)("ORDERNO").ToString.Contains("新") Then
                            If T00015tbl.Rows(j)("TORICODE") = T00015row("TORICODE") AndAlso
                               T00015tbl.Rows(j)("OILTYPE") = T00015row("OILTYPE") AndAlso
                               T00015tbl.Rows(j)("KIJUNDATE") = T00015row("KIJUNDATE") AndAlso
                               T00015tbl.Rows(j)("SHIPORG") = T00015row("SHIPORG") Then

                                T00015tbl.Rows(j)("ORDERNO") = CS0033AutoNumber.SEQ
                            Else
                                Exit For
                            End If
                        End If
                    Next

                Else
                    Master.Output(CS0033AutoNumber.ERR, C_MESSAGE_TYPE.ABORT, CS0033AutoNumber.ERR_DETAIL)
                    Exit Sub
                End If
            End If

        Next

        '■■■ 画面非表示レコード+画面表示レコードによりT00015UPDtblを作成 ■■■

        '○T00015UPDtbl内の画面表示レコードを削除(日付による）…　T00015tblとレコード重複しているため

        Dim WW_TODOKEDATEF As Date
        Dim WW_TODOKEDATET As Date
        Dim WW_SHUKODATEF As Date
        Dim WW_SHUKODATET As Date
        '届日（FROM-TO）
        If String.IsNullOrEmpty(work.WF_SEL_TODOKEDATEF.Text) Then
            WW_TODOKEDATEF = C_DEFAULT_YMD
        Else
            WW_TODOKEDATEF = work.WF_SEL_TODOKEDATEF.Text
        End If
        If String.IsNullOrEmpty(work.WF_SEL_TODOKEDATET.Text) Then
            WW_TODOKEDATET = C_MAX_YMD
        Else
            WW_TODOKEDATET = work.WF_SEL_TODOKEDATET.Text
        End If
        '出荷日（FROM-TO）
        If String.IsNullOrEmpty(work.WF_SEL_SHUKODATEF.Text) Then
            WW_SHUKODATEF = C_DEFAULT_YMD
        Else
            WW_SHUKODATEF = work.WF_SEL_SHUKODATEF.Text
        End If
        If String.IsNullOrEmpty(work.WF_SEL_SHUKODATET.Text) Then
            WW_SHUKODATET = C_MAX_YMD
        Else
            WW_SHUKODATET = work.WF_SEL_SHUKODATET.Text
        End If

        WW_FILLstr =
            "TODOKEDATE < #" & WW_TODOKEDATEF & "# or " &
            "TODOKEDATE > #" & WW_TODOKEDATET & "# or " &
            "SHUKODATE < #" & WW_SHUKODATEF & "# or " &
            "SHUKODATE > #" & WW_SHUKODATET & "#    "
        '画面表示レコードを削除
        CS0026TBLSORTget.TABLE = T00015UPDtbl
        CS0026TBLSORTget.SORTING = "TORICODE ,OILTYPE ,KIJUNDATE ,SHIPORG"
        CS0026TBLSORTget.FILTER = WW_FILLstr
        CS0026TBLSORTget.Sort(T00015UPDtbl)

        '○画面表示レコードをマージ
        CS0026TBLSORTget.TABLE = T00015tbl
        CS0026TBLSORTget.SORTING = "TORICODE ,OILTYPE ,KIJUNDATE ,SHIPORG"
        CS0026TBLSORTget.FILTER = "OPERATION = '" & C_LIST_OPERATION_CODE.UPDATING & "' or OPERATION = '" & C_LIST_OPERATION_CODE.WARNING & "'"
        CS0026TBLSORTget.Sort(T00015WKtbl)
        T00015UPDtbl.Merge(T00015WKtbl, False)

        '○更新・エラーをT00015UPDtblへ反映(DB更新単位：荷主、油種、基準日（出荷日or届日）、受注部署、出荷部署)
        CS0026TBLSORTget.TABLE = T00015UPDtbl
        CS0026TBLSORTget.SORTING = "TORICODE ,OILTYPE ,KIJUNDATE ,SHIPORG"
        CS0026TBLSORTget.FILTER = ""
        CS0026TBLSORTget.Sort(T00015UPDtbl)

        For i As Integer = 0 To T00015UPDtbl.Rows.Count - 1
            Dim T00015UPDrow = T00015UPDtbl.Rows(i)

            If T00015UPDrow("OPERATION") = C_LIST_OPERATION_CODE.ERRORED Then
                For j As Integer = 0 To T00015UPDtbl.Rows.Count - 1
                    '荷主、油種、基準日（出荷日or届日）、受注部署、出荷部署が同一
                    If T00015UPDtbl.Rows(j)("TORICODE") = T00015UPDrow("TORICODE") AndAlso
                       T00015UPDtbl.Rows(j)("OILTYPE") = T00015UPDrow("OILTYPE") AndAlso
                       T00015UPDtbl.Rows(j)("KIJUNDATE") = T00015UPDrow("KIJUNDATE") AndAlso
                       T00015UPDtbl.Rows(j)("SHIPORG") = T00015UPDrow("SHIPORG") Then

                        T00015UPDtbl.Rows(j)("OPERATION") = C_LIST_OPERATION_CODE.ERRORED

                    Else
                        'Exit For
                    End If
                Next
            End If

            If T00015UPDrow("OPERATION") = C_LIST_OPERATION_CODE.UPDATING OrElse
                T00015UPDrow("OPERATION") = C_LIST_OPERATION_CODE.WARNING Then
                For j As Integer = 0 To T00015UPDtbl.Rows.Count - 1
                    '荷主、油種、基準日（出荷日or届日）、受注部署、出荷部署が同一
                    If T00015UPDtbl.Rows(j)("TORICODE") = T00015UPDrow("TORICODE") AndAlso
                       T00015UPDtbl.Rows(j)("OILTYPE") = T00015UPDrow("OILTYPE") AndAlso
                       T00015UPDtbl.Rows(j)("KIJUNDATE") = T00015UPDrow("KIJUNDATE") AndAlso
                       T00015UPDtbl.Rows(j)("SHIPORG") = T00015UPDrow("SHIPORG") AndAlso
                       T00015UPDtbl.Rows(j)("OPERATION") <> C_LIST_OPERATION_CODE.ERRORED Then

                        T00015UPDtbl.Rows(j)("OPERATION") = C_LIST_OPERATION_CODE.UPDATING

                    Else
                        'Exit For
                    End If
                Next
            End If

        Next

        '○更新対象以外のレコードを削除
        CS0026TBLSORTget.TABLE = T00015UPDtbl
        CS0026TBLSORTget.SORTING = "TORICODE ,OILTYPE ,KIJUNDATE ,SHIPORG ,SHUKODATE ,GSHABAN ,TRIPNO ,DROPNO , SEQ"
        CS0026TBLSORTget.FILTER = "OPERATION = '" & C_LIST_OPERATION_CODE.UPDATING & "' or OPERATION = '" & C_LIST_OPERATION_CODE.WARNING & "'"
        CS0026TBLSORTget.Sort(T00015UPDtbl)

        '■■■ T00015UPDtblのDetailNO、SEQを再付番 ■■■
        Dim WW_DETAILNO As Integer = 0
        Dim WW_SEQ As Integer = 0

        '○DetailNO再付番
        WW_TORICODE = ""
        WW_OILTYPE = ""
        WW_SHUKADATE = ""
        WW_KIJUNDATE = ""
        WW_SHIPORG = ""
        WW_SHUKODATE = ""
        WW_GSHABAN = ""
        WW_TRIPNO = ""
        WW_DROPNO = ""

        For Each T00015UPDrow In T00015UPDtbl.Rows

            If T00015UPDrow("DELFLG") <> "1" Then
                If WW_TORICODE = T00015UPDrow("TORICODE") AndAlso
                   WW_OILTYPE = T00015UPDrow("OILTYPE") AndAlso
                   WW_KIJUNDATE = T00015UPDrow("KIJUNDATE") AndAlso
                   WW_SHIPORG = T00015UPDrow("SHIPORG") Then

                    WW_DETAILNO += 1
                    T00015UPDrow("DETAILNO") = WW_DETAILNO.ToString("000")
                Else
                    WW_DETAILNO = 1
                    T00015UPDrow("DETAILNO") = WW_DETAILNO.ToString("000")

                    WW_TORICODE = T00015UPDrow("TORICODE")
                    WW_OILTYPE = T00015UPDrow("OILTYPE")
                    WW_KIJUNDATE = T00015UPDrow("KIJUNDATE")
                    WW_SHIPORG = T00015UPDrow("SHIPORG")

                End If
            End If

        Next

        '○台数設定
        WW_TORICODE = ""
        WW_OILTYPE = ""
        WW_SHUKADATE = ""
        WW_KIJUNDATE = ""
        WW_SHIPORG = ""
        WW_SHUKODATE = ""
        WW_GSHABAN = ""
        WW_TRIPNO = ""
        For Each T00015UPDrow In T00015UPDtbl.Rows

            If T00015UPDrow("DELFLG") <> "1" Then
                If WW_TORICODE = T00015UPDrow("TORICODE") AndAlso
                   WW_OILTYPE = T00015UPDrow("OILTYPE") AndAlso
                   WW_KIJUNDATE = T00015UPDrow("KIJUNDATE") AndAlso
                   WW_SHIPORG = T00015UPDrow("SHIPORG") AndAlso
                   WW_SHUKODATE = T00015UPDrow("SHUKODATE") AndAlso
                   WW_GSHABAN = T00015UPDrow("GSHABAN") AndAlso
                   WW_TRIPNO = T00015UPDrow("TRIPNO") Then

                    T00015UPDrow("JDAISU") = 0
                Else
                    T00015UPDrow("JDAISU") = 1

                    WW_TORICODE = T00015UPDrow("TORICODE")
                    WW_OILTYPE = T00015UPDrow("OILTYPE")
                    WW_KIJUNDATE = T00015UPDrow("KIJUNDATE")
                    WW_SHIPORG = T00015UPDrow("SHIPORG")
                    WW_SHUKODATE = T00015UPDrow("SHUKODATE")
                    WW_GSHABAN = T00015UPDrow("GSHABAN")
                    WW_TRIPNO = T00015UPDrow("TRIPNO")
                    WW_DROPNO = T00015UPDrow("DROPNO")

                End If
            End If

        Next

        '○SEQ再付番
        WW_TORICODE = ""
        WW_OILTYPE = ""
        WW_SHUKADATE = ""
        WW_KIJUNDATE = ""
        WW_SHIPORG = ""
        WW_SHUKODATE = ""
        WW_GSHABAN = ""
        WW_TRIPNO = ""
        WW_DROPNO = ""
        For Each T00015UPDrow In T00015UPDtbl.Rows

            If T00015UPDrow("DELFLG") <> "1" Then
                If WW_TORICODE = T00015UPDrow("TORICODE") AndAlso
                   WW_OILTYPE = T00015UPDrow("OILTYPE") AndAlso
                   WW_KIJUNDATE = T00015UPDrow("KIJUNDATE") AndAlso
                   WW_SHIPORG = T00015UPDrow("SHIPORG") AndAlso
                   WW_SHUKODATE = T00015UPDrow("SHUKODATE") AndAlso
                   WW_GSHABAN = T00015UPDrow("GSHABAN") AndAlso
                   WW_TRIPNO = T00015UPDrow("TRIPNO") AndAlso
                   WW_DROPNO = T00015UPDrow("DROPNO") Then

                    WW_SEQ += 1
                    T00015UPDrow("SEQ") = WW_SEQ.ToString("00")
                Else
                    WW_SEQ = 1
                    T00015UPDrow("SEQ") = WW_SEQ.ToString("00")

                    WW_TORICODE = T00015UPDrow("TORICODE")
                    WW_OILTYPE = T00015UPDrow("OILTYPE")
                    WW_KIJUNDATE = T00015UPDrow("KIJUNDATE")
                    WW_SHIPORG = T00015UPDrow("SHIPORG")
                    WW_SHUKODATE = T00015UPDrow("SHUKODATE")
                    WW_GSHABAN = T00015UPDrow("GSHABAN")
                    WW_TRIPNO = T00015UPDrow("TRIPNO")
                    WW_DROPNO = T00015UPDrow("DROPNO")

                End If
            End If

        Next

        '○close
        O_RTN = C_MESSAGE_NO.NORMAL

    End Sub

    ''' <summary>
    ''' T00015tbl関連データ削除
    ''' </summary>
    ''' <param name="I_DATENOW">更新時刻</param>
    ''' <param name="O_RTN">RTNCODE</param>
    ''' <remarks>更新対象受注の画面非表示（他出庫日）を取得。配送受注の更新最小単位は出荷部署単位。</remarks>
    Protected Sub DBupdate_T15DELETE(ByVal I_DATENOW As Date, ByVal O_RTN As String)

        '■■■ T00015UPDtbl関連の荷主受注・配送受注を論理削除 ■■■　…　削除情報はT00015UPDtblに存在

        'Sort
        CS0026TBLSORTget.TABLE = T00015UPDtbl
        CS0026TBLSORTget.SORTING = "TORICODE ,OILTYPE ,KIJUNDATE ,SHIPORG ,TIMSTP , DELFLG, OPERATION"
        CS0026TBLSORTget.FILTER = ""
        CS0026TBLSORTget.Sort(T00015UPDtbl)

        Try
            'DataBase接続文字
            Dim SQLcon = CS0050SESSION.getConnection
            SQLcon.Open() 'DataBase接続(Open)

            '更新SQL文･･･配送受注の現受注番号を一括論理削除
            Dim SQLStr As String =
                      " UPDATE T0015_SUPPLJISSKI        " _
                    & "    SET UPDYMD      = @P11,      " _
                    & "        UPDUSER     = @P12,      " _
                    & "        UPDTERMID   = @P13,      " _
                    & "        RECEIVEYMD  = @P14,      " _
                    & "        DELFLG      = '1'        " _
                    & "  WHERE CAMPCODE    = @P01       " _
                    & "    AND TORICODE    = @P02       " _
                    & "    AND OILTYPE     = @P03       " _
                    & "    AND SHIPORG     = @P04       " _
                    & "    AND KIJUNDATE   = @P05       " _
                    & "    AND DELFLG     <> '1'        "

            Dim SQLcmd As New SqlCommand(SQLStr, SQLcon)
            SQLcmd.CommandTimeout = 300

            Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", System.Data.SqlDbType.NVarChar)
            Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", System.Data.SqlDbType.NVarChar)
            Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@P03", System.Data.SqlDbType.NVarChar)
            Dim PARA04 As SqlParameter = SQLcmd.Parameters.Add("@P04", System.Data.SqlDbType.NVarChar)
            Dim PARA05 As SqlParameter = SQLcmd.Parameters.Add("@P05", System.Data.SqlDbType.Date)

            Dim PARA11 As SqlParameter = SQLcmd.Parameters.Add("@P11", System.Data.SqlDbType.DateTime)
            Dim PARA12 As SqlParameter = SQLcmd.Parameters.Add("@P12", System.Data.SqlDbType.NVarChar)
            Dim PARA13 As SqlParameter = SQLcmd.Parameters.Add("@P13", System.Data.SqlDbType.NVarChar)
            Dim PARA14 As SqlParameter = SQLcmd.Parameters.Add("@P14", System.Data.SqlDbType.DateTime)

            Dim WW_TORICODE As String = ""
            Dim WW_OILTYPE As String = ""
            Dim WW_SHUKADATE As String = ""
            Dim WW_KIJUNDATE As String = ""
            Dim WW_SHIPORG As String = ""

            For Each T00015UPDrow In T00015UPDtbl.Rows

                If T00015UPDrow("OPERATION") = C_LIST_OPERATION_CODE.UPDATING OrElse
                    T00015UPDrow("OPERATION") = C_LIST_OPERATION_CODE.WARNING Then
                    If T00015UPDrow("TORICODE") <> WW_TORICODE OrElse
                       T00015UPDrow("OILTYPE") <> WW_OILTYPE OrElse
                       T00015UPDrow("KIJUNDATE") <> WW_KIJUNDATE OrElse
                       T00015UPDrow("SHIPORG") <> WW_SHIPORG Then

                        '○T00015UPDtbl関連の配送受注を論理削除

                        PARA01.Value = T00015UPDrow("CAMPCODE")
                        PARA02.Value = T00015UPDrow("TORICODE")
                        PARA03.Value = T00015UPDrow("OILTYPE")
                        PARA04.Value = T00015UPDrow("SHIPORG")
                        PARA05.Value = T00015UPDrow("KIJUNDATE")

                        PARA11.Value = I_DATENOW
                        PARA12.Value = Master.USERID
                        PARA13.Value = Master.USERTERMID
                        PARA14.Value = C_DEFAULT_YMD

                        SQLcmd.ExecuteNonQuery()

                        'ブレイクキー退避
                        WW_TORICODE = T00015UPDrow("TORICODE")
                        WW_OILTYPE = T00015UPDrow("OILTYPE")
                        WW_KIJUNDATE = T00015UPDrow("KIJUNDATE")
                        WW_SHIPORG = T00015UPDrow("SHIPORG")
                    End If
                End If

            Next

            'CLOSE
            SQLcmd.Dispose()
            SQLcmd = Nothing

            O_RTN = C_MESSAGE_NO.NORMAL

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "T0015_SUPPLJISSKI(old) DEL")
            CS0011LOGWRITE.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:T0015_SUPPLJISSKI(old) DEL"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力

            O_RTN = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try

    End Sub

#End Region

#Region "T0015テーブル入力関連"
    ''' <summary>
    ''' 詳細画面をテーブルデータに退避する
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub DetailBoxToINP()

        '■■■ Detail変数設定 ■■■
        Master.CreateEmptyTable(T00015INPtbl)
        Dim WW_DetailMAX As Integer = 0

        WW_DetailMAX = WF_DViewRep1.Items.Count \ WF_REP_ROWSCNT.Value

        '■■■ DetailよりT00015INPtbl編集 ■■■
        'Detail入力レコード回数ループ
        For i As Integer = 0 To WW_DetailMAX - 1

            'Detail入力テーブル準備
            Dim T00015INProw = T00015INPtbl.NewRow

            T00015INProw("LINECNT") = 0
            T00015INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA
            T00015INProw("TIMSTP") = 0
            T00015INProw("SELECT") = 0
            T00015INProw("HIDDEN") = 0

            T00015INProw("INDEX") = ""
            T00015INProw("CAMPCODE") = ""
            T00015INProw("CAMPCODENAME") = ""
            T00015INProw("ORDERNO") = ""
            T00015INProw("DETAILNO") = ""
            T00015INProw("TRIPNO") = ""
            T00015INProw("DROPNO") = ""
            T00015INProw("SEQ") = ""
            T00015INProw("TORICODE") = ""
            T00015INProw("TORICODENAME") = ""
            T00015INProw("OILTYPE") = ""
            T00015INProw("OILTYPENAME") = ""
            T00015INProw("SHUKODATE") = ""
            T00015INProw("KIKODATE") = ""
            T00015INProw("KIJUNDATE") = ""
            T00015INProw("SHUKADATE") = ""
            T00015INProw("SHIPORG") = ""
            T00015INProw("SHIPORGNAME") = ""
            T00015INProw("SHUKABASHO") = ""
            T00015INProw("SHUKABASHONAME") = ""
            T00015INProw("GSHABAN") = ""
            T00015INProw("GSHABANLICNPLTNO") = ""
            T00015INProw("RYOME") = ""
            T00015INProw("SHAFUKU") = ""
            T00015INProw("STAFFCODE") = ""
            T00015INProw("STAFFCODENAME") = ""
            T00015INProw("SUBSTAFFCODE") = ""
            T00015INProw("SUBSTAFFCODENAME") = ""
            T00015INProw("TODOKEDATE") = ""
            T00015INProw("TODOKECODE") = ""
            T00015INProw("TODOKECODENAME") = ""
            T00015INProw("PRODUCT1") = ""
            T00015INProw("PRODUCT1NAME") = ""
            T00015INProw("PRODUCT2") = ""
            T00015INProw("PRODUCT2NAME") = ""
            T00015INProw("PRODUCTCODE") = ""
            T00015INProw("PRODUCTNAME") = ""
            T00015INProw("CONTNO") = ""
            T00015INProw("JSURYO") = ""
            T00015INProw("JSURYO_SUM") = ""
            T00015INProw("JDAISU") = ""
            T00015INProw("JDAISU_SUM") = ""
            T00015INProw("REMARKS1") = ""
            T00015INProw("REMARKS2") = ""
            T00015INProw("REMARKS3") = ""
            T00015INProw("REMARKS4") = ""
            T00015INProw("REMARKS5") = ""
            T00015INProw("REMARKS6") = ""
            T00015INProw("SHARYOTYPEF") = ""
            T00015INProw("TSHABANF") = ""
            T00015INProw("SHARYOTYPEB") = ""
            T00015INProw("TSHABANB") = ""
            T00015INProw("SHARYOTYPEB2") = ""
            T00015INProw("TSHABANB2") = ""
            T00015INProw("JISSEKIKBN") = ""
            T00015INProw("JISSEKIKBNNAME") = ""

            T00015INProw("DELFLG") = ""

            T00015INProw("ADDR") = ""
            T00015INProw("NOTES1") = ""
            T00015INProw("NOTES2") = ""
            T00015INProw("NOTES3") = ""
            T00015INProw("NOTES4") = ""
            T00015INProw("NOTES5") = ""
            T00015INProw("NOTES6") = ""
            T00015INProw("NOTES7") = ""
            T00015INProw("NOTES8") = ""
            T00015INProw("NOTES9") = ""
            T00015INProw("NOTES10") = ""
            T00015INProw("STAFFNOTES1") = ""
            T00015INProw("STAFFNOTES2") = ""
            T00015INProw("STAFFNOTES3") = ""
            T00015INProw("STAFFNOTES4") = ""
            T00015INProw("STAFFNOTES5") = ""

            T00015INProw("SHARYOINFO1") = ""
            T00015INProw("SHARYOINFO2") = ""
            T00015INProw("SHARYOINFO3") = ""
            T00015INProw("SHARYOINFO4") = ""
            T00015INProw("SHARYOINFO5") = ""
            T00015INProw("SHARYOINFO6") = ""

            T00015INProw("WORK_NO") = ""

            For j As Integer = (i * WF_REP_ROWSCNT.Value) To ((i + 1) * WF_REP_ROWSCNT.Value - 1)
                If j <= (WF_DViewRep1.Items.Count - 1) Then

                    T00015INProw("WORK_NO") =
                        CType(WF_DViewRep1.Items(j).FindControl("WF_Rep1_MEISAINO"), System.Web.UI.WebControls.TextBox).Text

                    For col As Integer = 1 To WF_REP_COLSCNT.Value

                        If CType(WF_DViewRep1.Items(j).FindControl("WF_Rep1_FIELD_" & col), System.Web.UI.WebControls.Label).Text <> "" Then
                            T00015INProw(CType(WF_DViewRep1.Items(j).FindControl("WF_Rep1_FIELD_" & col), System.Web.UI.WebControls.Label).Text) =
                                CType(WF_DViewRep1.Items(j).FindControl("WF_Rep1_VALUE_" & col), System.Web.UI.WebControls.TextBox).Text
                        End If

                    Next

                End If
            Next
            If WF_Sel_LINECNT.Text = "" Then
                T00015INProw("LINECNT") = 0
            Else
                If Not Integer.TryParse(WF_REP_LINECNT.Value, T00015INProw("LINECNT")) Then
                    T00015INProw("LINECNT") = 0
                End If
            End If
            T00015INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA
            T00015INProw("TIMSTP") = "0"
            T00015INProw("SELECT") = 1                                      '対象
            T00015INProw("HIDDEN") = 0                                      '表示

            '条件指定・会社コードで置き換え（入力項目を減らす））
            T00015INProw("CAMPCODE") = work.WF_SEL_CAMPCODE.Text
            '出庫日
            Master.EraseCharToIgnore(WF_SHUKODATE.Text)
            T00015INProw("SHUKODATE") = WF_SHUKODATE.Text
            '出荷日
            Master.EraseCharToIgnore(WF_SHUKADATE.Text)
            T00015INProw("SHUKADATE") = WF_SHUKADATE.Text
            '届日
            Master.EraseCharToIgnore(WF_TODOKEDATE.Text)
            T00015INProw("TODOKEDATE") = WF_TODOKEDATE.Text
            '帰庫日
            Master.EraseCharToIgnore(WF_KIKODATE.Text)
            T00015INProw("KIKODATE") = WF_KIKODATE.Text
            '両目
            Master.EraseCharToIgnore(WF_RYOME.Text)
            T00015INProw("RYOME") = WF_RYOME.Text
            '受注番号
            Master.EraseCharToIgnore(WF_ORDERNO.Text)
            T00015INProw("ORDERNO") = WF_ORDERNO.Text
            '明細番号
            Master.EraseCharToIgnore(WF_DETAILNO.Text)
            T00015INProw("DETAILNO") = WF_DETAILNO.Text
            '油種
            Master.EraseCharToIgnore(WF_OILTYPE.Text)
            T00015INProw("OILTYPE") = WF_OILTYPE.Text
            '取引先
            Master.EraseCharToIgnore(WF_TORICODE.Text)
            T00015INProw("TORICODE") = WF_TORICODE.Text
            '出荷部署
            Master.EraseCharToIgnore(WF_SHIPORG.Text)
            T00015INProw("SHIPORG") = WF_SHIPORG.Text
            '業務車番
            Master.EraseCharToIgnore(WF_GSHABAN.Text)
            T00015INProw("GSHABAN") = WF_GSHABAN.Text
            '車腹
            Master.EraseCharToIgnore(WF_SHAFUKU.Text)
            T00015INProw("SHAFUKU") = WF_SHAFUKU.Text
            'トリップ
            Master.EraseCharToIgnore(WF_TRIPNO.Text)
            T00015INProw("TRIPNO") = WF_TRIPNO.Text
            'ドロップ
            Master.EraseCharToIgnore(WF_DROPNO.Text)
            T00015INProw("DROPNO") = WF_DROPNO.Text
            '乗務員
            Master.EraseCharToIgnore(WF_STAFFCODE.Text)
            T00015INProw("STAFFCODE") = WF_STAFFCODE.Text
            '副乗務員
            Master.EraseCharToIgnore(WF_SUBSTAFFCODE.Text)
            T00015INProw("SUBSTAFFCODE") = WF_SUBSTAFFCODE.Text
            '実績区分
            Master.EraseCharToIgnore(WF_JISSEKIKBN.Text)
            T00015INProw("JISSEKIKBN") = WF_JISSEKIKBN.Text

            '○名称付与
            CODENAME_set(T00015INProw)

            '入力テーブル作成
            T00015INPtbl.Rows.Add(T00015INProw)

        Next

    End Sub


    ''' <summary>
    ''' 入力データ登録
    ''' </summary>
    ''' <param name="O_RTNCODE"></param>
    ''' <remarks></remarks>
    Protected Sub INPtbltoT15tbl(ByRef O_RTNCODE As String)

        '■■■ 数量ゼロは読み飛ばし ■■■
        For i As Integer = T00015INPtbl.Rows.Count - 1 To 0 Step -1
            Dim T00015INProw = T00015INPtbl.Rows(i)
            '出荷前々日以降は、データ取込対象外とする
            If Val(T00015INProw("JSURYO")) = 0 Then
                '数量なしは無視
                T00015INPtbl.Rows(i).Delete()
            End If
        Next


        '■■■ 項目チェック ■■■
        '●チェック処理
        INPtbl_CHEK(WW_ERRCODE)

        INPtbl_CHEK_DATE(WW_ERRCODE)

        '■■■ 変更有無チェック ■■■    
        '…　Grid画面へ別明細追加：T00015INProw("WORK_NO") = ""
        '　　変更発生　　：T00015INProw("OPERATION")へ"更新"or"エラー"を設定

        '●変更有無取得　　　     ※Excelは全て新規。全て更新とする。
        For Each T00015INProw In T00015INPtbl.Rows
            '数量・台数未設定時は対象外
            If T00015INProw("WORK_NO") = "" AndAlso Val(T00015INProw("JSURYO")) = 0 AndAlso Val(T00015INProw("JDAISU")) = 0 Then Continue For

            'エラーは設定しない
            If T00015INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA Then
                T00015INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
            End If
            If T00015INProw("OPERATION") <> C_LIST_OPERATION_CODE.ERRORED Then
                T00015INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
            End If

            T00015INProw("WORK_NO") = ""
            T00015INProw("LINECNT") = 0

        Next


        '■■■ 更新前処理（入力情報へ受注番号設定、Grid画面の同一行情報を削除）　■■■
        For i As Integer = 0 To T00015INPtbl.Rows.Count - 1

            Dim T00015INProw = T00015INPtbl.Rows(i)
            '数量・台数未設定時は対象外
            If T00015INProw("WORK_NO") = "" AndAlso Val(T00015INProw("JSURYO")) = 0 AndAlso Val(T00015INProw("JDAISU")) = 0 Then Continue For

            For j As Integer = 0 To T00015tbl.Rows.Count - 1

                '状態をクリア設定
                EditOperationText(T00015tbl.Rows(j), False)

                If T00015INProw("OPERATION") <> C_LIST_OPERATION_CODE.NODATA Then

                    'Grid画面行追加の場合は受注番号を取得
                    If T00015tbl.Rows(j)("TORICODE") = T00015INProw("TORICODE") AndAlso
                       T00015tbl.Rows(j)("OILTYPE") = T00015INProw("OILTYPE") AndAlso
                       T00015tbl.Rows(j)("KIJUNDATE") = T00015INProw("KIJUNDATE") AndAlso
                       T00015tbl.Rows(j)("SHIPORG") = T00015INProw("SHIPORG") Then

                        T00015INProw("ORDERNO") = T00015tbl.Rows(j)("ORDERNO")
                        T00015INProw("DETAILNO") = "000"

                    End If

                    '取引先、油種、基準日（出荷日or届日）、受注部署、出荷部署、出庫日、業務車番、両目、トリップ、ドロップが同一
                    If CompareOrder(T00015tbl.Rows(j), T00015INProw) Then

                        T00015INProw("LINECNT") = T00015tbl.Rows(j)("LINECNT")

                    End If

                    'EXCELは同一受注条件レコードを論理削除（T15実態が存在する場合、物理削除。）
                    If T00015tbl.Rows(j)("GSHABAN") = T00015INProw("GSHABAN") AndAlso
                       T00015tbl.Rows(j)("OILTYPE") = T00015INProw("OILTYPE") AndAlso
                       T00015tbl.Rows(j)("SHUKODATE") = T00015INProw("SHUKODATE") AndAlso
                       T00015tbl.Rows(j)("SHIPORG") = T00015INProw("SHIPORG") AndAlso
                       T00015tbl.Rows(j)("DELFLG") <> "1" Then

                        If Val(T00015tbl.Rows(j)("JSURYO")) = 0 Then
                            T00015tbl.Rows(j)("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                            T00015tbl.Rows(j)("DELFLG") = "1"   '削除
                            T00015tbl.Rows(j)("HIDDEN") = "1"   '非表示
                            T00015tbl.Rows(j)("SELECT") = "0"   '明細表示対象外
                        Else
                            T00015INProw("DELFLG") = "1"
                            T00015INProw("HIDDEN") = "1"   '非表示
                            T00015INProw("SELECT") = "0"   '明細表示対象外
                        End If
                    Else
                        If T00015tbl.Rows(j)("SHIPORG") = WF_DEFORG.Text Then
                            If T00015tbl.Rows(j)("GSHABAN") = "" AndAlso
                               T00015tbl.Rows(j)("OILTYPE") = T00015INProw("OILTYPE") AndAlso
                               T00015tbl.Rows(j)("SHUKODATE") = T00015INProw("SHUKODATE") AndAlso
                               T00015tbl.Rows(j)("SHIPORG") = T00015INProw("SHIPORG") AndAlso
                               T00015tbl.Rows(j)("DELFLG") <> "1" Then
                                If Val(T00015tbl.Rows(j)("JSURYO")) = 0 Then
                                    T00015tbl.Rows(j)("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                                    T00015tbl.Rows(j)("DELFLG") = "1"   '削除
                                    T00015tbl.Rows(j)("HIDDEN") = "1"   '非表示
                                    T00015tbl.Rows(j)("SELECT") = "0"   '明細表示対象外
                                Else
                                    T00015INProw("DELFLG") = "1"
                                    T00015INProw("HIDDEN") = "1"   '非表示
                                    T00015INProw("SELECT") = "0"   '明細表示対象外
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
        For i As Integer = T00015INPtbl.Rows.Count - 1 To 0 Step -1

            Dim T00015INProw = T00015INPtbl.Rows(i)

            If Date.TryParse(T00015INProw("SHUKODATE"), WW_DATE) Then

                '出庫前々日以降は、データ取込対象外とする
                '出荷日<当日は処理対象外（出荷当日までOK）
                If WW_DATE < WW_LOGONYMD Then
                    Dim WW_ERR_MES As String = "・更新できないレコード(過去日データ)です。"
                    WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 明細番号= @D" & i.ToString("000") & "D@ , "
                    WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 取引先　=" & T00015INProw("TORICODE") & " , "
                    WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 届先　　=" & T00015INProw("TODOKECODE") & " , "
                    WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 出荷場所=" & T00015INProw("SHUKABASHO") & " , "
                    WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 出庫日　=" & T00015INProw("SHUKODATE") & " , "
                    WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 届日　　=" & T00015INProw("TODOKEDATE") & " , "
                    WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 出荷日　=" & T00015INProw("SHUKADATE") & " , "
                    WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 車番　　=" & T00015INProw("GSHABAN") & " , "
                    WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 乗務員　=" & T00015INProw("STAFFCODE") & " , "
                    WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 品名  　=" & T00015INProw("PRODUCTCODE") & " , "
                    WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> ﾄﾘｯﾌﾟ 　=" & T00015INProw("TRIPNO") & " , "
                    WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> ﾄﾞﾛｯﾌﾟ　=" & T00015INProw("DROPNO") & " , "
                    WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 削除　　=" & T00015INProw("DELFLG") & " "
                    rightview.AddErrorReport(WW_ERR_MES)

                    T00015INProw("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
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
            CS0011LOGWRITE.INFSUBCLASS = "GRT0015"                   'SUBクラス名
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

        For i As Integer = 0 To T00015INPtbl.Rows.Count - 1

            Dim T00015INProw = T00015INPtbl.Rows(i)

            If T00015INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING OrElse
                T00015INProw("OPERATION") = C_LIST_OPERATION_CODE.WARNING Then
                For j As Integer = 0 To T00015tbl.Rows.Count - 1
                    '取引先、油種、基準日（出荷日or届日）、受注部署、出荷部署、出庫日、業務車番、両目、トリップ、ドロップが同一
                    If T00015tbl.Rows(j)("TORICODE") = T00015INProw("TORICODE") AndAlso
                       T00015tbl.Rows(j)("OILTYPE") = T00015INProw("OILTYPE") AndAlso
                       T00015tbl.Rows(j)("KIJUNDATE") = T00015INProw("KIJUNDATE") AndAlso
                       T00015tbl.Rows(j)("SHIPORG") = T00015INProw("SHIPORG") AndAlso
                       T00015tbl.Rows(j)("SHUKODATE") = T00015INProw("SHUKODATE") AndAlso
                       T00015tbl.Rows(j)("TRIPNO") = "000" Then

                        T00015tbl.Rows(j)("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                        T00015tbl.Rows(j)("DELFLG") = "1"

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

        For i As Integer = 0 To T00015INPtbl.Rows.Count - 1

            Dim T00015INProw = T00015INPtbl.Rows(i)

            If T00015INProw("WORK_NO") = "" And Val(T00015INProw("JSURYO")) = 0 And Val(T00015INProw("JDAISU")) = 0 Then
            Else
                If T00015INProw("OPERATION") = C_LIST_OPERATION_CODE.ERRORED Then
                    For j As Integer = i To T00015INPtbl.Rows.Count - 1
                        '取引先、油種、基準日（出荷日or届日）、受注部署、出荷部署、出庫日、業務車番、両目、トリップ、ドロップが同一
                        If CompareOrder(T00015INPtbl.Rows(j), T00015INProw) Then

                            T00015INPtbl.Rows(j)("OPERATION") = C_LIST_OPERATION_CODE.ERRORED

                        End If
                    Next
                End If

                If T00015INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING OrElse
                    T00015INProw("OPERATION") = C_LIST_OPERATION_CODE.WARNING Then
                    For j As Integer = 0 To T00015INPtbl.Rows.Count - 1
                        '取引先、油種、基準日（出荷日or届日）、受注部署、出荷部署、出庫日、業務車番、両目、トリップ、ドロップが同一
                        If CompareOrder(T00015INPtbl.Rows(j), T00015INProw) AndAlso
                           T00015INPtbl.Rows(j)("OPERATION") <> C_LIST_OPERATION_CODE.ERRORED Then

                            T00015INPtbl.Rows(j)("OPERATION") = C_LIST_OPERATION_CODE.UPDATING

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
        CS0026TBLSORTget.TABLE = T00015tbl
        CS0026TBLSORTget.SORTING = "LINECNT ASC"
        CS0026TBLSORTget.FILTER = ""
        CS0026TBLSORTget.Sort(T00015tbl)


        Dim WW_ORDERNO As Integer = 0
        Dim WW_DETAILNO As Integer = 0
        Dim WW_LINECNT As Integer = 0
        Dim WW_CNT As Integer = 0

        '受注番号初期値セット
        If T00015tbl.Rows.Count = 0 Then
            WW_LINECNT = 0
        Else
            WW_LINECNT = CInt(T00015tbl.Rows(T00015tbl.Rows.Count - 1)("LINECNT"))
        End If

        For i As Integer = 0 To T00015INPtbl.Rows.Count - 1

            Dim T00015INProw = T00015INPtbl.Rows(i)

            '新規有効明細
            If T00015INProw("WORK_NO") = "" AndAlso (Val(T00015INProw("JSURYO")) <> 0 OrElse Val(T00015INProw("JDAISU")) <> 0) Then

                If Val(T00015INProw("LINECNT")) = 0 Then

                    WW_LINECNT = WW_LINECNT + 1
                    WW_CNT = 0

                    '同一条件レコードへも反映
                    For j As Integer = 0 To T00015INPtbl.Rows.Count - 1
                        If T00015INPtbl.Rows(j)("WORK_NO") = "" AndAlso (Val(T00015INPtbl.Rows(j)("JSURYO")) <> 0 OrElse Val(T00015INPtbl.Rows(j)("JDAISU")) <> 0) Then

                            If CompareOrder(T00015INPtbl.Rows(j), T00015INProw) Then

                                WW_CNT = WW_CNT + 1
                                T00015INPtbl.Rows(j)("LINECNT") = WW_LINECNT.ToString("0")
                                T00015INPtbl.Rows(j)("SEQ") = WW_CNT.ToString("00")

                            End If

                        End If
                    Next
                Else
                    Dim WW_FIND = "OFF"
                    WW_CNT = 0
                    '同一条件レコードへも反映
                    For j As Integer = 0 To T00015INPtbl.Rows.Count - 1
                        If T00015INPtbl.Rows(j)("WORK_NO") = "" AndAlso (Val(T00015INPtbl.Rows(j)("JSURYO")) <> 0 OrElse Val(T00015INPtbl.Rows(j)("JDAISU")) <> 0) Then

                            If CompareOrder(T00015INPtbl.Rows(j), T00015INProw) Then

                                If T00015INPtbl.Rows(j)("SEQ") = "01" Then
                                    WW_FIND = "ON"
                                    Exit For
                                End If
                            End If

                        End If
                    Next
                    '枝番（SEQ）="01"が存在しない場合、SEQの振り直す
                    If WW_FIND = "OFF" Then
                        For j As Integer = 0 To T00015INPtbl.Rows.Count - 1
                            If T00015INPtbl.Rows(j)("WORK_NO") = "" AndAlso (Val(T00015INPtbl.Rows(j)("JSURYO")) <> 0 OrElse Val(T00015INPtbl.Rows(j)("JDAISU")) <> 0) Then

                                If CompareOrder(T00015INPtbl.Rows(j), T00015INProw) Then

                                    WW_CNT = WW_CNT + 1
                                    T00015INPtbl.Rows(j)("SEQ") = WW_CNT.ToString("00")
                                End If

                            End If
                        Next
                    End If
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
        For i As Integer = 0 To T00015INPtbl.Rows.Count - 1

            Dim T00015INProw = T00015INPtbl.Rows(i)

            '数量・台数未設定時は対象外
            If T00015INProw("WORK_NO") = "" AndAlso Val(T00015INProw("JSURYO")) = 0 AndAlso Val(T00015INProw("JDAISU")) = 0 Then Continue For

            '追加明細("WORK_NO")
            If T00015INProw("WORK_NO") = "" Then

                WW_ORDERNO = WW_ORDERNO + 1
                WW_DETAILNO = 0

                'T15INPtblへも反映（次レコード処理用）
                For j As Integer = 0 To T00015INPtbl.Rows.Count - 1
                    '数量・台数未設定時は対象外
                    If T00015INPtbl.Rows(j)("WORK_NO") = "" AndAlso Val(T00015INPtbl.Rows(j)("JSURYO")) = 0 AndAlso Val(T00015INPtbl.Rows(j)("JDAISU")) = 0 Then Continue For

                    If T00015INPtbl.Rows(j)("ORDERNO") = "" Then

                        '受注判定基準により同一受注に、新受注番号を付与
                        If T00015INPtbl.Rows(j)("TORICODE") = T00015INProw("TORICODE") AndAlso
                            T00015INPtbl.Rows(j)("OILTYPE") = T00015INProw("OILTYPE") AndAlso
                            T00015INPtbl.Rows(j)("SHIPORG") = T00015INProw("SHIPORG") AndAlso
                            T00015INPtbl.Rows(j)("KIJUNDATE") = T00015INProw("KIJUNDATE") Then

                            T00015INPtbl.Rows(j)("ORDERNO") = "新" & WW_ORDERNO.ToString("00")
                            WW_DETAILNO = WW_DETAILNO + 1
                            T00015INPtbl.Rows(j)("DETAILNO") = WW_DETAILNO.ToString("000")
                            T00015INPtbl.Rows(j)("WORK_NO") = "0"

                        End If
                    Else

                        '受注判定基準により同一受注に、新受注番号を付与
                        If T00015INPtbl.Rows(j)("WORK_NO") = "" AndAlso
                            T00015INPtbl.Rows(j)("TORICODE") = T00015INProw("TORICODE") AndAlso
                            T00015INPtbl.Rows(j)("OILTYPE") = T00015INProw("OILTYPE") AndAlso
                            T00015INPtbl.Rows(j)("SHIPORG") = T00015INProw("SHIPORG") AndAlso
                            T00015INPtbl.Rows(j)("KIJUNDATE") = T00015INProw("KIJUNDATE") Then

                            WW_DETAILNO = WW_DETAILNO + 1
                            T00015INPtbl.Rows(j)("DETAILNO") = WW_DETAILNO.ToString("000")
                            T00015INPtbl.Rows(j)("WORK_NO") = "0"

                        End If
                    End If

                Next

                '○T00015INProwをT00015tblへ追加
                T00015tbl.ImportRow(T00015INProw)

            Else

                If T00015INProw("OPERATION") <> C_LIST_OPERATION_CODE.NODATA Then

                    '○T00015INProwをT00015tblへ追加
                    T00015tbl.ImportRow(T00015INProw)
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

        '○インターフェイス初期値設定
        O_RTNCODE = C_MESSAGE_NO.NORMAL

        Dim WW_LINEerr As String = ""
        Dim WW_SEQ As Integer = 0
        Dim WW_CS0024FCHECKVAL As String = ""
        Dim WW_CS0024FCHECKERR As String = ""
        Dim WW_CS0024FCHECKREPORT As String = ""
        Dim WW_CheckMES1 As String = ""
        Dim WW_CheckMES2 As String = ""
        Dim WW_TEXT As String = ""

        WW_ERRLIST.Clear()
        If IsNothing(S0013tbl) Then
            S0013tbl = New DataTable
        End If

        For i As Integer = 0 To T00015INPtbl.Rows.Count - 1

            Dim T00015INProw = T00015INPtbl.Rows(i)

            WW_LINEerr = C_MESSAGE_NO.NORMAL

            '初期クリア
            T00015INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA

            '数量・台数未設定時はチェック対象外
            If T00015INProw("WORK_NO") = "" AndAlso T00015INProw("JSURYO") = "" AndAlso T00015INProw("JDAISU") = "" Then Continue For


            '■■■ 単項目チェック(ヘッダー情報) ■■■

            Dim WW_TORI_FLG As String = ""
            '■キー項目(取引先コード：TORICODE)
            '○必須・項目属性チェック
            WW_CS0024FCHECKVAL = T00015INProw("TORICODE")
            Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "TORICODE", WW_CS0024FCHECKVAL, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT, S0013tbl)
            If isNormal(WW_CS0024FCHECKERR) Then
                If Val(WW_CS0024FCHECKVAL) = 0 Then
                    CODENAME_get("TORICODE", T00015INProw("TORICODE"), WW_TEXT, WW_RTN_SW)
                    T00015INProw("TORICODENAME") = WW_TEXT
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・更新できないレコード(取引先コードエラー)です。"
                        WW_CheckMES2 = " マスタに存在しません。"
                        ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, T00015INProw)
                    Else
                        WW_TORI_FLG = "OK"
                    End If
                Else
                    T00015INProw("TORICODE") = WW_CS0024FCHECKVAL
                    '○LeftBox存在チェック
                    CODENAME_get("TORICODE", T00015INProw("TORICODE"), WW_TEXT, WW_RTN_SW)
                    T00015INProw("TORICODENAME") = WW_TEXT
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・更新できないレコード(取引先コードエラー)です。"
                        WW_CheckMES2 = " マスタに存在しません。"
                        ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, T00015INProw)
                    Else
                        WW_TORI_FLG = "OK"
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(取引先コードエラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, T00015INProw)
            End If

            Dim WW_OILTYPE_FLG As String = ""
            '○必須・項目属性チェック
            WW_CS0024FCHECKVAL = T00015INProw("OILTYPE")
            Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "OILTYPE", WW_CS0024FCHECKVAL, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT, S0013tbl)
            If isNormal(WW_CS0024FCHECKERR) Then
                If CInt(WW_CS0024FCHECKVAL) = 0 Then
                    WW_CheckMES1 = "・更新できないレコード(油種エラー)です。"
                    WW_CheckMES2 = WW_CS0024FCHECKREPORT
                    ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, T00015INProw)
                Else
                    T00015INProw("OILTYPE") = WW_CS0024FCHECKVAL
                    If Not String.IsNullOrEmpty(work.WF_SEL_OILTYPE.Text) AndAlso work.WF_SEL_OILTYPE.Text <> T00015INProw("OILTYPE") Then
                        WW_CheckMES1 = "・更新できないレコード(油種エラー)です。"
                        WW_CheckMES2 = " 条件入力で指定された油種と異ります( " & T00015INProw("OILTYPE") & ") "
                        ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, T00015INProw)
                    Else
                        WW_OILTYPE_FLG = "OK"
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(油種エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, T00015INProw)
            End If

            '■キー項目(出荷日：SHUKADATE)
            '○デフォルト
            If String.IsNullOrEmpty(T00015INProw("SHUKADATE")) Then
                T00015INProw("SHUKADATE") = T00015INProw("SHUKODATE")
            End If

            '○必須・項目属性チェック
            WW_CS0024FCHECKVAL = T00015INProw("SHUKADATE")
            Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "SHUKADATE", WW_CS0024FCHECKVAL, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT, S0013tbl)
            If isNormal(WW_CS0024FCHECKERR) Then
                T00015INProw("SHUKADATE") = WW_CS0024FCHECKVAL      'yyyy/MM/dd
            Else
                WW_CheckMES1 = "・更新できないレコード(出荷日エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, T00015INProw)
            End If

            '■明細項目(出荷部署：SHIPORG)

            '○デフォルト
            If T00015INProw("SHIPORG") = "" Then
                T00015INProw("SHIPORG") = WF_DEFORG.Text
            End If


            '○必須・項目属性チェック
            WW_CS0024FCHECKVAL = T00015INProw("SHIPORG")
            Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "SHIPORG", WW_CS0024FCHECKVAL, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT, S0013tbl)
            If isNormal(WW_CS0024FCHECKERR) Then
                T00015INProw("SHIPORG") = WW_CS0024FCHECKVAL

                '○LeftBox存在チェック
                If Not String.IsNullOrEmpty(T00015INProw("SHIPORG")) Then
                    CODENAME_get("SHIPORG", T00015INProw("SHIPORG"), WW_TEXT, WW_RTN_SW)
                    T00015INProw("SHIPORGNAME") = WW_TEXT
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・更新できないレコード(出荷部署エラー)です。"
                        WW_CheckMES2 = " マスタに存在しません。"
                        ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, T00015INProw)
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(出荷部署エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, T00015INProw)
            End If

            '■明細項目(出庫日：SHUKODATE)
            '○必須・項目属性チェック
            Dim WW_SHUKODATEERR As String = "OFF"
            WW_CS0024FCHECKVAL = T00015INProw("SHUKODATE")
            Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "SHUKODATE", WW_CS0024FCHECKVAL, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT, S0013tbl)
            If isNormal(WW_CS0024FCHECKERR) Then
                T00015INProw("SHUKODATE") = WW_CS0024FCHECKVAL      'yyyy/MM/dd
            Else
                WW_SHUKODATEERR = "ON"
                WW_CheckMES1 = "・エラーが存在します。(出庫日)"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, T00015INProw)
            End If

            '*******************  業務車番チェック  *********************

            Dim WW_CHKFLG As String = "ON"

            '■明細項目(業務車番：GSHABAN)
            '○必須・項目属性チェック
            If T00015INProw("SHIPORG") <> WF_DEFORG.Text Then
                '異なる拠点データ投入時はチェック対象外
                If T00015INProw("GSHABAN") = "" Then
                    WW_CHKFLG = "OFF"
                End If
            End If

            If WW_CHKFLG = "ON" Then
                WW_CS0024FCHECKVAL = T00015INProw("GSHABAN")
                Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "GSHABAN", WW_CS0024FCHECKVAL, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT, S0013tbl)
                If isNormal(WW_CS0024FCHECKERR) Then
                    T00015INProw("GSHABAN") = WW_CS0024FCHECKVAL

                    '○LeftBox存在チェック
                    If T00015INProw("GSHABAN") <> "" Then
                        CODENAME_get("GSHABAN", T00015INProw("GSHABAN"), WW_TEXT, WW_RTN_SW)
                        If Not isNormal(WW_RTN_SW) Then
                            WW_CheckMES1 = "・エラーが存在します。(業務車番)"
                            WW_CheckMES2 = " マスタに存在しません。"
                            ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.BOX_ERROR_EXIST, T00015INProw)
                        End If
                    End If
                Else
                    WW_CheckMES1 = "・エラーが存在します。(業務車番)"
                    WW_CheckMES2 = WW_CS0024FCHECKREPORT
                    ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.BOX_ERROR_EXIST, T00015INProw)
                End If

            End If

            '*******************  乗務員チェック  *********************

            '■明細項目(乗務員コード：STAFFCODE)
            '○必須・項目属性チェック
            WW_CHKFLG = "ON"
            If T00015INProw("SHIPORG") <> WF_DEFORG.Text Then
                '異なる拠点データ投入時はチェック対象外
                If T00015INProw("STAFFCODE") = "" Then
                    WW_CHKFLG = "OFF"
                End If
            End If

            If WW_CHKFLG = "ON" Then
                WW_CS0024FCHECKVAL = T00015INProw("STAFFCODE")
                Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "STAFFCODE", WW_CS0024FCHECKVAL, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT, S0013tbl)
                If isNormal(WW_CS0024FCHECKERR) Then
                    If CInt(WW_CS0024FCHECKVAL) = 0 Then
                    Else
                        T00015INProw("STAFFCODE") = WW_CS0024FCHECKVAL

                        '○LeftBox存在チェック
                        CODENAME_get("STAFFCODE", T00015INProw("STAFFCODE"), WW_TEXT, WW_RTN_SW)
                        T00015INProw("STAFFCODENAME") = WW_TEXT
                        If Not isNormal(WW_RTN_SW) Then
                            WW_CheckMES1 = "・エラーが存在します。(乗務員コード)"
                            WW_CheckMES2 = " マスタに存在しません。"
                            ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.BOX_ERROR_EXIST, T00015INProw)
                        End If
                    End If
                Else
                    WW_CheckMES1 = "・エラーが存在します。(乗務員コード)"
                    WW_CheckMES2 = WW_CS0024FCHECKREPORT
                    ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.BOX_ERROR_EXIST, T00015INProw)
                End If

            End If

            '*******************  副乗務員チェック  *********************

            '■明細項目(副乗務員コード：SUBSTAFFCODE)
            '○必須・項目属性チェック
            WW_CHKFLG = "ON"
            If T00015INProw("SHIPORG") <> WF_DEFORG.Text Then
                '異なる拠点データ投入時はチェック対象外
                If T00015INProw("SUBSTAFFCODE") = "" Then
                    WW_CHKFLG = "OFF"
                End If
            End If

            If WW_CHKFLG = "ON" Then
                WW_CS0024FCHECKVAL = T00015INProw("SUBSTAFFCODE")
                Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "SUBSTAFFCODE", WW_CS0024FCHECKVAL, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT, S0013tbl)
                If isNormal(WW_CS0024FCHECKERR) Then
                    If CInt(WW_CS0024FCHECKVAL) = 0 Then
                    Else
                        T00015INProw("SUBSTAFFCODE") = WW_CS0024FCHECKVAL

                        '○LeftBox存在チェック
                        CODENAME_get("SUBSTAFFCODE", T00015INProw("SUBSTAFFCODE"), WW_TEXT, WW_RTN_SW)
                        T00015INProw("SUBSTAFFCODENAME") = WW_TEXT
                        If Not isNormal(WW_RTN_SW) Then
                            WW_CheckMES1 = "・エラーが存在します。(副乗務員コード)"
                            WW_CheckMES2 = " マスタに存在しません。"
                            ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.BOX_ERROR_EXIST, T00015INProw)
                        End If
                    End If
                Else
                    WW_CheckMES1 = "・エラーが存在します。(副乗務員コード)"
                    WW_CheckMES2 = WW_CS0024FCHECKREPORT
                    ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.BOX_ERROR_EXIST, T00015INProw)
                End If

            End If

            '*******************  出荷場所チェック  *********************

            '■明細項目(出荷場所：SHUKABASHO)
            '○必須・項目属性チェック

            WW_CS0024FCHECKVAL = T00015INProw("SHUKABASHO")
            Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "SHUKABASHO", WW_CS0024FCHECKVAL, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT, S0013tbl)
            If isNormal(WW_CS0024FCHECKERR) Then
                T00015INProw("SHUKABASHO") = WW_CS0024FCHECKVAL

                '○LeftBox存在チェック
                CODENAME_get("SHUKABASHO", T00015INProw("SHUKABASHO"), WW_TEXT, WW_RTN_SW, work.createDistinationParam(work.WF_SEL_CAMPCODE.Text, T00015INProw("SHIPORG"), "", "2", True))
                T00015INProw("SHUKABASHONAME") = WW_TEXT
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・エラーが存在します。(出荷場所)"
                    WW_CheckMES2 = " マスタに存在しません。"
                    ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.BOX_ERROR_EXIST, T00015INProw)
                End If
            Else
                CODENAME_get("SHUKABASHO", T00015INProw("SHUKABASHO"), WW_TEXT, WW_RTN_SW, work.createDistinationParam(work.WF_SEL_CAMPCODE.Text, T00015INProw("SHIPORG"), "", "2", True))
                T00015INProw("SHUKABASHONAME") = WW_TEXT
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・エラーが存在します。(出荷場所)"
                    WW_CheckMES2 = " マスタに存在しません。"
                    ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.BOX_ERROR_EXIST, T00015INProw)
                End If
            End If

            '■明細項目(帰庫日：KIKODATE)
            '○デフォルト
            If String.IsNullOrEmpty(T00015INProw("KIKODATE")) Then
                T00015INProw("KIKODATE") = T00015INProw("SHUKODATE")
            End If

            '○必須・項目属性チェック
            WW_CS0024FCHECKVAL = T00015INProw("KIKODATE")
            Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "KIKODATE", WW_CS0024FCHECKVAL, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT, S0013tbl)
            If isNormal(WW_CS0024FCHECKERR) Then
                T00015INProw("KIKODATE") = WW_CS0024FCHECKVAL      'yyyy/MM/dd
            Else
                WW_CheckMES1 = "・エラーが存在します。(帰庫日)"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.BOX_ERROR_EXIST, T00015INProw)
            End If

            '■明細項目(車腹：SHAFUKU)
            '○デフォルト
            '業務車番より、車腹を再設定
            WW_CHKFLG = "ON"
            If T00015INProw("SHIPORG") <> WF_DEFORG.Text Then
                If T00015INProw("SHAFUKU") = "" Then
                    WW_CHKFLG = "OFF"
                End If
            End If

            If WW_CHKFLG = "ON" Then
                T00015INProw("SHAFUKU") = ""
                Dim item = WF_ListSHAFUKU.Items.FindByText(T00015INProw("GSHABAN"))
                If Not IsNothing(item) Then
                    T00015INProw("SHAFUKU") = item.Value
                End If

                '○必須・項目属性チェック
                WW_CS0024FCHECKVAL = T00015INProw("SHAFUKU")
                Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "SHAFUKU", WW_CS0024FCHECKVAL, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT, S0013tbl)
                If isNormal(WW_CS0024FCHECKERR) Then
                    'データ存在チェック（上記チェック方法がNUMのため、ゼロ埋めデータが出来てしまう）
                    If String.IsNullOrEmpty(T00015INProw("SHAFUKU")) AndAlso CInt(WW_CS0024FCHECKVAL) <> 0 Then
                        WW_CheckMES1 = "・エラーが存在します。(車腹登録なし)"
                        WW_CheckMES2 = ""
                        ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.BOX_ERROR_EXIST, T00015INProw)
                    Else
                        T00015INProw("SHAFUKU") = WW_CS0024FCHECKVAL
                    End If
                Else
                    WW_CheckMES1 = "・エラーが存在します。(車腹)"
                    WW_CheckMES2 = WW_CS0024FCHECKREPORT
                    ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.BOX_ERROR_EXIST, T00015INProw)
                End If

            End If

            WW_CHKFLG = "ON"
            If T00015INProw("SHIPORG") <> WF_DEFORG.Text Then
                If T00015INProw("TRIPNO") = "" Then
                    WW_SEQ += 1
                    T00015INProw("TRIPNO") = WW_SEQ.ToString("000")
                    WW_CHKFLG = "OFF"
                End If
            End If

            If WW_CHKFLG = "ON" Then
                '■明細項目(トリップ：TRIPNO)
                '○必須・項目属性チェック
                WW_CS0024FCHECKVAL = T00015INProw("TRIPNO")
                Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "TRIPNO", WW_CS0024FCHECKVAL, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT, S0013tbl)
                If isNormal(WW_CS0024FCHECKERR) Then
                    T00015INProw("TRIPNO") = WW_CS0024FCHECKVAL
                Else
                    WW_CheckMES1 = "・エラーが存在します。(トリップ)"
                    WW_CheckMES2 = WW_CS0024FCHECKREPORT
                    ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.BOX_ERROR_EXIST, T00015INProw)
                End If

            End If

            '■明細項目(ドロップ：DROPNO)
            '○必須・項目属性チェック
            WW_CHKFLG = "ON"
            If T00015INProw("SHIPORG") <> WF_DEFORG.Text Then
                If T00015INProw("DROPNO") = "" Then
                    T00015INProw("DROPNO") = "000"
                    WW_CHKFLG = "OFF"
                End If
            End If

            If WW_CHKFLG = "ON" Then
                WW_CS0024FCHECKVAL = T00015INProw("DROPNO")
                Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "DROPNO", WW_CS0024FCHECKVAL, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT, S0013tbl)
                If isNormal(WW_CS0024FCHECKERR) Then
                    T00015INProw("DROPNO") = WW_CS0024FCHECKVAL
                Else
                    WW_CheckMES1 = "・エラーが存在します。(ドロップ)"
                    WW_CheckMES2 = WW_CS0024FCHECKREPORT
                    ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.BOX_ERROR_EXIST, T00015INProw)
                End If

            End If

            '*******************  日付・時間チェック  *********************

            '■キー項目(届日：TODOKEDATE)
            '○デフォルト
            If String.IsNullOrEmpty(T00015INProw("TODOKEDATE")) Then
                T00015INProw("TODOKEDATE") = T00015INProw("SHUKODATE")
            End If

            '○必須・項目属性チェック
            WW_CS0024FCHECKVAL = T00015INProw("TODOKEDATE")
            Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "TODOKEDATE", WW_CS0024FCHECKVAL, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT, S0013tbl)
            If isNormal(WW_CS0024FCHECKERR) Then
                T00015INProw("TODOKEDATE") = WW_CS0024FCHECKVAL      'yyyy/MM/dd
            Else
                WW_CheckMES1 = "・エラーが存在します。(届日エラー)"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.BOX_ERROR_EXIST, T00015INProw)
            End If

            '*******************  品名チェック  *********************

            '・明細項目(品名コード：PRODUCTCODE)

            WW_CS0024FCHECKVAL = T00015INProw("PRODUCTCODE")
            Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "PRODUCTCODE", WW_CS0024FCHECKVAL, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT, S0013tbl)
            If isNormal(WW_CS0024FCHECKERR) Then
                If Not String.IsNullOrEmpty(WW_CS0024FCHECKVAL) Then
                    T00015INProw("PRODUCTCODE") = WW_CS0024FCHECKVAL

                    'LeftBox存在チェック
                    CODENAME_get("PRODUCTCODE", T00015INProw("PRODUCTCODE"), WW_TEXT, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・エラーが存在します。（品名コード）"
                        WW_CheckMES2 = "マスタに存在しません。"
                        ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.BOX_ERROR_EXIST, T00015INProw)
                    End If
                End If
            Else
                WW_CheckMES1 = "・エラーが存在します。（品名コード）"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.BOX_ERROR_EXIST, T00015INProw)
            End If

            '■明細項目(品名１：PRODUCT1)
            '■明細項目(品名２：PRODUCT2)
            If T00015INProw("PRODUCTCODE") <> "" AndAlso T00015INProw("PRODUCTCODE").ToString.Length = 11 Then
                Dim productCode As String = T00015INProw("PRODUCTCODE").ToString
                T00015INProw("PRODUCT1") = productCode.Substring(4, 2)
                T00015INProw("PRODUCT2") = productCode.Substring(6, 5)
            End If

            '*******************  届先チェック  *********************

            '■明細項目(届先コード：TODOKECODE)
            '○必須・項目属性チェック

            WW_CS0024FCHECKVAL = T00015INProw("TODOKECODE")
            Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "TODOKECODE", WW_CS0024FCHECKVAL, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT, S0013tbl)
            If isNormal(WW_CS0024FCHECKERR) Then

                '○LeftBox存在チェック
                CODENAME_get("TODOKECODE", T00015INProw("TODOKECODE"), WW_TEXT, WW_RTN_SW, work.createDistinationParam(work.WF_SEL_CAMPCODE.Text, T00015INProw("SHIPORG"), "", "1", True))
                T00015INProw("TODOKECODENAME") = WW_TEXT
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・エラーが存在します。(届先コード)"
                    WW_CheckMES2 = " マスタに存在しません。"
                    ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.BOX_ERROR_EXIST, T00015INProw)
                End If
            Else
                WW_CheckMES1 = "・エラーが存在します。(届先コード)"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.BOX_ERROR_EXIST, T00015INProw)
            End If


            '*******************  数量チェック  *********************

            '■明細項目(数量：JSURYO)
            '○必須・項目属性チェック
            WW_CS0024FCHECKVAL = T00015INProw("JSURYO")
            Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "JSURYO", WW_CS0024FCHECKVAL, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT, S0013tbl)
            If isNormal(WW_CS0024FCHECKERR) Then
                'データ存在チェック
                If String.IsNullOrEmpty(T00015INProw("JSURYO")) Then
                    T00015INProw("JSURYO") = ""
                Else
                    T00015INProw("JSURYO") = WW_CS0024FCHECKVAL
                End If
            Else
                WW_CheckMES1 = "・エラーが存在します。(数量)"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.BOX_ERROR_EXIST, T00015INProw)
            End If

            '■明細項目(台数：JDAISU)
            '○デフォルト
            If T00015INProw("OILTYPE") <> "04" Then
                T00015INProw("JDAISU") = 1
            End If

            '○必須・項目属性チェック
            WW_CS0024FCHECKVAL = T00015INProw("JDAISU")
            Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "JDAISU", WW_CS0024FCHECKVAL, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT, S0013tbl)
            If isNormal(WW_CS0024FCHECKERR) Then
                T00015INProw("JDAISU") = CInt(WW_CS0024FCHECKVAL)
            Else
                WW_CheckMES1 = "・エラーが存在します。(台数不正)"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.BOX_ERROR_EXIST, T00015INProw)
            End If

            '*******************  その他チェック  *********************


            '■明細項目(コンテナ番号：CONTNO)
            '○必須・項目属性チェック
            WW_CS0024FCHECKVAL = T00015INProw("CONTNO")
            Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "CONTNO", WW_CS0024FCHECKVAL, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT, S0013tbl)
            If isNormal(WW_CS0024FCHECKERR) Then
                T00015INProw("CONTNO") = WW_CS0024FCHECKVAL
            Else
                WW_CheckMES1 = "・エラーが存在します。(コンテナ番号)"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.BOX_ERROR_EXIST, T00015INProw)
            End If

            '■明細項目(枝番：SEQ)
            '○必須・項目属性チェック
            WW_CS0024FCHECKVAL = T00015INProw("SEQ")
            Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "SEQ", WW_CS0024FCHECKVAL, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT, S0013tbl)
            If isNormal(WW_CS0024FCHECKERR) Then
                T00015INProw("SEQ") = WW_CS0024FCHECKVAL
            Else
                WW_CheckMES1 = "・エラーが存在します。(枝番)"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.BOX_ERROR_EXIST, T00015INProw)
            End If

            '■明細項目(両目：RYOME)
            '○必須・項目属性チェック
            WW_CS0024FCHECKVAL = T00015INProw("RYOME")
            Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "RYOME", WW_CS0024FCHECKVAL, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT, S0013tbl)
            If isNormal(WW_CS0024FCHECKERR) Then
                T00015INProw("RYOME") = WW_CS0024FCHECKVAL
            Else
                WW_CheckMES1 = "・エラーが存在します。(両目)"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.BOX_ERROR_EXIST, T00015INProw)
            End If

            '■明細項目(削除フラグ：DELFLG)
            '○必須・項目属性チェック
            WW_CS0024FCHECKVAL = T00015INProw("DELFLG")
            Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "DELFLG", WW_CS0024FCHECKVAL, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT, S0013tbl)
            If isNormal(WW_CS0024FCHECKERR) Then
                T00015INProw("DELFLG") = WW_CS0024FCHECKVAL
            Else
                WW_CheckMES1 = "・エラーが存在します。(削除フラグ)"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.BOX_ERROR_EXIST, T00015INProw)
            End If

            '■■■ 関連チェック　■■■

            '■数量or台数入力チェック
            If Val(T00015INProw("JSURYO")) = 0 AndAlso Val(T00015INProw("JDAISU")) = 0 Then
                WW_CheckMES1 = "・更新できないレコード(数量・台数が未入力)です。"
                WW_CheckMES2 = ""
                ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, T00015INProw)
            End If

            '■出庫日・帰庫日
            If T00015INProw("SHUKODATE") <> "" AndAlso T00015INProw("KIKODATE") <> "" AndAlso T00015INProw("SHUKODATE") > T00015INProw("KIKODATE") Then
                WW_CheckMES1 = "・更新できないレコード(出庫日 > 帰庫日)です。"
                WW_CheckMES2 = ""
                ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, T00015INProw)
            End If

            '■容器検査期限、車検期限チェック（八戸、大井川、水島のみ）
            Dim WW_HPRSINSNYMDF As String = ""
            Dim WW_HPRSINSNYMDB As String = ""
            Dim WW_HPRSINSNYMDB2 As String = ""
            Dim WW_LICNYMDF As String = ""
            Dim WW_LICNYMDB As String = ""
            Dim WW_LICNYMDB2 As String = ""
            Dim WW_LICNPLTNOF As String = ""
            Dim WW_LICNPLTNOB As String = ""
            Dim WW_LICNPLTNOB2 As String = ""
            If WW_SHUKODATEERR = "OFF" AndAlso T00015INProw("SHUKODATE") <> "" Then
                If IsInspectionOrg(work.WF_SEL_CAMPCODE.Text, T00015INProw("SHIPORG").ToString, O_RTNCODE) Then

                    If T00015INProw("OILTYPE") = "02" Then
                        For j As Integer = 0 To WF_ListGSHABAN.Items.Count - 1
                            If WF_ListGSHABAN.Items(j).Value = T00015INProw("GSHABAN") Then
                                If WF_ListOILTYPE.Items(j).Value = T00015INProw("OILTYPE") Then
                                    WW_HPRSINSNYMDF = WF_ListHPRSINSNYMDF.Items(j).Value.Replace("-", "/")
                                    WW_HPRSINSNYMDB = WF_ListHPRSINSNYMDB.Items(j).Value.Replace("-", "/")
                                    WW_HPRSINSNYMDB2 = WF_ListHPRSINSNYMDB2.Items(j).Value.Replace("-", "/")
                                    WW_LICNYMDF = WF_ListLICNYMDF.Items(j).Value.Replace("-", "/")
                                    WW_LICNYMDB = WF_ListLICNYMDB.Items(j).Value.Replace("-", "/")
                                    WW_LICNYMDB2 = WF_ListLICNYMDB2.Items(j).Value.Replace("-", "/")
                                    WW_LICNPLTNOF = WF_ListLICNPLTNOF.Items(j).Value
                                    WW_LICNPLTNOB = WF_ListLICNPLTNOB.Items(j).Value
                                    WW_LICNPLTNOB2 = WF_ListLICNPLTNOB2.Items(j).Value
                                    Exit For
                                End If
                            End If
                        Next

                        '容器検査年月日チェック（２カ月前から警告、４日前はエラー）
                        '車検年月日チェック（１カ月前から警告、４日前はエラー）
                        '------ 車両前 -------------------------------------------------------------------------
                        '車検チェック
                        If SYARYOTYPE.INSPECTION_LIST.Contains(T00015INProw("SHARYOTYPEF")) Then
                            If IsDate(WW_LICNYMDF) Then
                                Dim WW_days As String = DateDiff("d", T00015INProw("SHUKODATE"), CDate(WW_LICNYMDF))
                                If CDate(WW_LICNYMDF) < T00015INProw("SHUKODATE") Then
                                    '車検切れ
                                    WW_CheckMES1 = "・更新できないレコード(車検切れ)です。(" & WW_LICNPLTNOF & " " & T00015INProw("SHARYOTYPEF") & T00015INProw("TSHABANF") & " " & WW_LICNYMDF & ")"
                                    WW_CheckMES2 = ""
                                    ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, T00015INProw)
                                ElseIf CDate(WW_LICNYMDF).AddDays(-4) < T00015INProw("SHUKODATE") Then
                                    '４日前はエラー
                                    WW_CheckMES1 = "・更新できないレコード(車検" & WW_days & "日前)です。(" & WW_LICNPLTNOF & " " & T00015INProw("SHARYOTYPEF") & T00015INProw("TSHABANF") & " " & WW_LICNYMDF & ")"
                                    WW_CheckMES2 = ""
                                    ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, T00015INProw)
                                ElseIf CDate(WW_LICNYMDF).AddMonths(-1) < T00015INProw("SHUKODATE") Then
                                    '1カ月前から警告
                                    WW_CheckMES1 = "・警告レコード(車検" & WW_days & "日前)です。(" & WW_LICNPLTNOF & " " & T00015INProw("SHARYOTYPEF") & T00015INProw("TSHABANF") & " " & WW_LICNYMDF & ")"
                                    WW_CheckMES2 = ""
                                    ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.WORNING_RECORD_EXIST, T00015INProw)
                                End If
                            Else
                                'エラー
                                WW_CheckMES1 = "・更新できないレコード(車両マスタ不備：車検有効年月日)です。(" & WW_LICNPLTNOF & " " & T00015INProw("SHARYOTYPEF") & T00015INProw("TSHABANF") & ")"
                                WW_CheckMES2 = ""
                                ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, T00015INProw)
                            End If
                        End If

                        '容器チェック
                        If SYARYOTYPE.TANK_LIST.Contains(T00015INProw("SHARYOTYPEF")) Then
                            If IsDate(WW_HPRSINSNYMDF) Then
                                Dim WW_days As String = DateDiff("d", T00015INProw("SHUKODATE"), CDate(WW_HPRSINSNYMDF))
                                If CDate(WW_HPRSINSNYMDF) < T00015INProw("SHUKODATE") Then
                                    '容器検査切れ
                                    WW_CheckMES1 = "・更新できないレコード(容器検査切れ)です。(" & WW_LICNPLTNOF & " " & T00015INProw("SHARYOTYPEF") & T00015INProw("TSHABANF") & " " & WW_HPRSINSNYMDF & ")"
                                    WW_CheckMES2 = ""
                                    ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, T00015INProw)
                                ElseIf CDate(WW_HPRSINSNYMDF).AddDays(-4) < T00015INProw("SHUKODATE") Then
                                    '４日前はエラー
                                    WW_CheckMES1 = "・更新できないレコード(容器検査" & WW_days & "日前)です。(" & WW_LICNPLTNOF & " " & T00015INProw("SHARYOTYPEF") & T00015INProw("TSHABANF") & " " & WW_HPRSINSNYMDF & ")"
                                    WW_CheckMES2 = ""
                                    ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, T00015INProw)
                                ElseIf CDate(WW_HPRSINSNYMDF).AddMonths(-2) < T00015INProw("SHUKODATE") Then
                                    '2カ月前から警告
                                    WW_CheckMES1 = "・警告レコード(容器検査" & WW_days & "日前)です。(" & WW_LICNPLTNOF & " " & T00015INProw("SHARYOTYPEF") & T00015INProw("TSHABANF") & " " & WW_HPRSINSNYMDF & ")"
                                    WW_CheckMES2 = ""
                                    ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.WORNING_RECORD_EXIST, T00015INProw)
                                End If
                            Else
                                'エラー
                                WW_CheckMES1 = "・更新できないレコード(車両マスタ不備：次回容器再検査年月日)です。(" & WW_LICNPLTNOF & " " & T00015INProw("SHARYOTYPEF") & T00015INProw("TSHABANF") & ")"
                                WW_CheckMES2 = ""
                                ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, T00015INProw)
                            End If

                        End If

                        '------ 車両後 -------------------------------------------------------------------------
                        '車検チェック
                        If SYARYOTYPE.INSPECTION_LIST.Contains(T00015INProw("SHARYOTYPEB")) Then
                            If IsDate(WW_LICNYMDB) Then
                                Dim WW_days As String = DateDiff("d", T00015INProw("SHUKODATE"), CDate(WW_LICNYMDB))
                                If CDate(WW_LICNYMDB) < T00015INProw("SHUKODATE") Then
                                    '車検切れ
                                    WW_CheckMES1 = "・更新できないレコード(車検切れ)です。(" & WW_LICNPLTNOB & " " & T00015INProw("SHARYOTYPEB") & T00015INProw("TSHABANB") & " " & WW_LICNYMDB & ")"
                                    WW_CheckMES2 = ""
                                    ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, T00015INProw)
                                ElseIf CDate(WW_LICNYMDB).AddDays(-4) < T00015INProw("SHUKODATE") Then
                                    '４日前はエラー
                                    WW_CheckMES1 = "・更新できないレコード(車検" & WW_days & "日前)です。(" & WW_LICNPLTNOB & " " & T00015INProw("SHARYOTYPEB") & T00015INProw("TSHABANB") & " " & WW_LICNYMDB & ")"
                                    WW_CheckMES2 = ""
                                    ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, T00015INProw)
                                ElseIf CDate(WW_LICNYMDB).AddMonths(-1) < T00015INProw("SHUKODATE") Then
                                    '1カ月前から警告
                                    WW_CheckMES1 = "・警告レコード(車検" & WW_days & "日前)です。(" & WW_LICNPLTNOB & " " & T00015INProw("SHARYOTYPEB") & T00015INProw("TSHABANB") & " " & WW_LICNYMDB & ")"
                                    WW_CheckMES2 = ""
                                    ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.WORNING_RECORD_EXIST, T00015INProw)
                                End If
                            Else
                                'エラー
                                WW_CheckMES1 = "・更新できないレコード(車両マスタ不備：車検有効年月日)です。(" & WW_LICNPLTNOB & " " & T00015INProw("SHARYOTYPEB") & T00015INProw("TSHABANB") & ")"
                                WW_CheckMES2 = ""
                                ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, T00015INProw)
                            End If
                        End If

                        '容器チェック
                        If SYARYOTYPE.TANK_LIST.Contains(T00015INProw("SHARYOTYPEB")) Then
                            If IsDate(WW_HPRSINSNYMDB) Then
                                Dim WW_days As String = DateDiff("d", T00015INProw("SHUKODATE"), CDate(WW_HPRSINSNYMDB))
                                If CDate(WW_HPRSINSNYMDB) < T00015INProw("SHUKODATE") Then
                                    '容器検査切れ
                                    WW_CheckMES1 = "・更新できないレコード(容器検査切れ)です。(" & WW_LICNPLTNOB & " " & T00015INProw("SHARYOTYPEB") & T00015INProw("TSHABANB") & " " & WW_HPRSINSNYMDB & ")"
                                    WW_CheckMES2 = ""
                                    ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, T00015INProw)
                                ElseIf CDate(WW_HPRSINSNYMDB).AddDays(-4) < T00015INProw("SHUKODATE") Then
                                    '４日前はエラー
                                    WW_CheckMES1 = "・更新できないレコード(容器検査" & WW_days & "日前)です。(" & WW_LICNPLTNOB & " " & T00015INProw("SHARYOTYPEB") & T00015INProw("TSHABANB") & " " & WW_HPRSINSNYMDB & ")"
                                    WW_CheckMES2 = ""
                                    ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, T00015INProw)
                                ElseIf CDate(WW_HPRSINSNYMDB).AddMonths(-2) < T00015INProw("SHUKODATE") Then
                                    '2カ月前から警告
                                    WW_CheckMES1 = "・警告レコード(容器検査" & WW_days & "日前)です。(" & WW_LICNPLTNOB & " " & T00015INProw("SHARYOTYPEB") & T00015INProw("TSHABANB") & " " & WW_HPRSINSNYMDB & ")"
                                    WW_CheckMES2 = ""
                                    ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.WORNING_RECORD_EXIST, T00015INProw)
                                End If
                            Else
                                'エラー
                                WW_CheckMES1 = "・更新できないレコード(車両マスタ不備：次回容器再検査年月日)です。(" & WW_LICNPLTNOB & " " & T00015INProw("SHARYOTYPEB") & T00015INProw("TSHABANB") & ")"
                                WW_CheckMES2 = ""
                                ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, T00015INProw)
                            End If

                        End If

                        '------ 車両後２ -------------------------------------------------------------------------
                        '車検チェック
                        If SYARYOTYPE.INSPECTION_LIST.Contains(T00015INProw("SHARYOTYPEB2")) Then
                            If IsDate(WW_LICNYMDB2) Then
                                Dim WW_days As String = DateDiff("d", T00015INProw("SHUKODATE"), CDate(WW_LICNYMDB2))
                                If CDate(WW_LICNYMDB2) < T00015INProw("SHUKODATE") Then
                                    '車検切れ
                                    WW_CheckMES1 = "・更新できないレコード(車検切れ)です。(" & WW_LICNPLTNOB2 & " " & T00015INProw("SHARYOTYPEB2") & T00015INProw("TSHABANB2") & " " & WW_LICNYMDB2 & ")"
                                    WW_CheckMES2 = ""
                                    ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, T00015INProw)
                                ElseIf CDate(WW_LICNYMDB2).AddDays(-4) < T00015INProw("SHUKODATE") Then
                                    '４日前はエラー
                                    WW_CheckMES1 = "・更新できないレコード(車検" & WW_days & "日前)です。(" & WW_LICNPLTNOB2 & " " & T00015INProw("SHARYOTYPEB2") & T00015INProw("TSHABANB2") & " " & WW_LICNYMDB2 & ")"
                                    WW_CheckMES2 = ""
                                    ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, T00015INProw)
                                ElseIf CDate(WW_LICNYMDB2).AddMonths(-1) < T00015INProw("SHUKODATE") Then
                                    '1カ月前から警告
                                    WW_CheckMES1 = "・警告レコード(車検" & WW_days & "日前)です。(" & WW_LICNPLTNOB2 & " " & T00015INProw("SHARYOTYPEB2") & T00015INProw("TSHABANB2") & " " & WW_LICNYMDB2 & ")"
                                    WW_CheckMES2 = ""
                                    ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.WORNING_RECORD_EXIST, T00015INProw)
                                End If
                            Else
                                'エラー
                                WW_CheckMES1 = "・更新できないレコード(車両マスタ不備：車検有効年月日)です。(" & WW_LICNPLTNOB2 & " " & T00015INProw("SHARYOTYPEB2") & T00015INProw("TSHABANB2") & ")"
                                WW_CheckMES2 = ""
                                ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, T00015INProw)
                            End If
                        End If

                        '容器チェック
                        If SYARYOTYPE.TANK_LIST.Contains(T00015INProw("SHARYOTYPEB2")) Then
                            If IsDate(WW_HPRSINSNYMDB2) Then
                                Dim WW_days As String = DateDiff("d", T00015INProw("SHUKODATE"), CDate(WW_HPRSINSNYMDB2))
                                If CDate(WW_HPRSINSNYMDB2) < T00015INProw("SHUKODATE") Then
                                    '容器検査切れ
                                    WW_CheckMES1 = "・更新できないレコード(容器検査切れ)です。(" & WW_LICNPLTNOB2 & " " & T00015INProw("SHARYOTYPEB2") & T00015INProw("TSHABANB2") & " " & WW_HPRSINSNYMDB2 & ")"
                                    WW_CheckMES2 = ""
                                    ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, T00015INProw)
                                ElseIf CDate(WW_HPRSINSNYMDB2).AddDays(-4) < T00015INProw("SHUKODATE") Then
                                    '４日前はエラー
                                    WW_CheckMES1 = "・更新できないレコード(容器検査" & WW_days & "日前)です。(" & WW_LICNPLTNOB2 & " " & T00015INProw("SHARYOTYPEB2") & T00015INProw("TSHABANB2") & " " & WW_HPRSINSNYMDB2 & ")"
                                    WW_CheckMES2 = ""
                                    ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, T00015INProw)
                                ElseIf CDate(WW_HPRSINSNYMDB2).AddMonths(-2) < T00015INProw("SHUKODATE") Then
                                    '2カ月前から警告
                                    WW_CheckMES1 = "・警告レコード(容器検査" & WW_days & "日前)です。(" & WW_LICNPLTNOB2 & " " & T00015INProw("SHARYOTYPEB2") & T00015INProw("TSHABANB2") & " " & WW_HPRSINSNYMDB2 & ")"
                                    WW_CheckMES2 = ""
                                    ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.WORNING_RECORD_EXIST, T00015INProw)
                                End If
                            Else
                                'エラー
                                WW_CheckMES1 = "・更新できないレコード(車両マスタ不備：次回容器再検査年月日)です。(" & WW_LICNPLTNOB2 & " " & T00015INProw("SHARYOTYPEB2") & T00015INProw("TSHABANB2") & ")"
                                WW_CheckMES2 = ""
                                ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, T00015INProw)
                            End If
                        End If
                    End If
                End If
            End If

            '■■■ 集計制御項目チェック（集計KEY必須チェック） ■■■

            '荷主受注集計制御マスタ取得
            If (WW_LINEerr = C_MESSAGE_NO.NORMAL OrElse WW_LINEerr = C_MESSAGE_NO.WORNING_RECORD_EXIST) AndAlso
               WW_TORI_FLG = "OK" AndAlso
               WW_OILTYPE_FLG = "OK" Then

                GS0029T3CNTLget.CAMPCODE = T00015INProw("CAMPCODE")
                GS0029T3CNTLget.TORICODE = T00015INProw("TORICODE")
                GS0029T3CNTLget.OILTYPE = T00015INProw("OILTYPE")
                GS0029T3CNTLget.ORDERORG = T00015INProw("SHIPORG")
                GS0029T3CNTLget.KIJUNDATE = Date.Now
                GS0029T3CNTLget.GS0029T3CNTLget()

                If isNormal(GS0029T3CNTLget.ERR) Then
                    If GS0029T3CNTLget.CNTL02 = "1" AndAlso T00015INProw("SHUKODATE") = "" Then     '集計区分(出庫日)
                        WW_CheckMES1 = "・更新できないレコード(出庫日未入力)です。"
                        WW_CheckMES2 = ""
                        ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, T00015INProw)
                    End If
                    If GS0029T3CNTLget.CNTL03 = "1" AndAlso T00015INProw("SHUKABASHO") = "" Then    '集計区分(出荷場所)
                        WW_CheckMES1 = "・更新できないレコード(出荷場所未入力)です。"
                        WW_CheckMES2 = ""
                        ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, T00015INProw)
                    End If
                    If T00015INProw("SHIPORG") <> WF_DEFORG.Text Then
                        '他部署は、チェックしない
                    Else
                        If GS0029T3CNTLget.CNTL04 = "1" AndAlso T00015INProw("GSHABAN") = "" Then       '集計区分(業務車番)
                            WW_CheckMES1 = "・更新できないレコード(業務車番未入力)です。"
                            WW_CheckMES2 = ""
                            ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, T00015INProw)
                        End If
                        If GS0029T3CNTLget.CNTL05 = "1" AndAlso T00015INProw("SHAFUKU") = "" Then       '集計区分(車腹(積載量))
                            WW_CheckMES1 = "・更新できないレコード(車腹未入力)です。"
                            WW_CheckMES2 = ""
                            ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, T00015INProw)
                        End If
                        If GS0029T3CNTLget.CNTL06 = "1" AndAlso T00015INProw("STAFFCODE") = "" Then     '集計区分(乗務員コード)
                            WW_CheckMES1 = "・更新できないレコード(乗務員未入力)です。"
                            WW_CheckMES2 = ""
                            ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, T00015INProw)
                        End If
                    End If
                    If GS0029T3CNTLget.CNTL07 = "1" AndAlso T00015INProw("TODOKECODE") = "" Then    '集計区分(届先コード)
                        WW_CheckMES1 = "・更新できないレコード(届先未入力)です。"
                        WW_CheckMES2 = ""
                        ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, T00015INProw)
                    End If
                    If GS0029T3CNTLget.CNTL08 = "1" AndAlso T00015INProw("PRODUCT1") = "" Then      '集計区分(品名１)
                        WW_CheckMES1 = "・更新できないレコード(品名１未入力)です。"
                        WW_CheckMES2 = ""
                        ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, T00015INProw)
                    End If
                    If GS0029T3CNTLget.CNTL09 = "1" AndAlso T00015INProw("PRODUCTCODE") = "" Then      '集計区分(品名２)
                        WW_CheckMES1 = "・更新できないレコード(品名２未入力)です。"
                        WW_CheckMES2 = ""
                        ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, T00015INProw)
                    End If
                    If GS0029T3CNTLget.CNTLVALUE = "1" AndAlso T00015INProw("JDAISU") = "" Then     '集計区分(数量/台数)
                        WW_CheckMES1 = "・更新できないレコード(台数未入力)です。"
                        WW_CheckMES2 = ""
                        ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, T00015INProw)
                    End If
                    If GS0029T3CNTLget.CNTLVALUE = "2" AndAlso T00015INProw("JSURYO") = "" Then     '集計区分(数量/台数)
                        WW_CheckMES1 = "・更新できないレコード(数量未入力)です。"
                        WW_CheckMES2 = ""
                        ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, T00015INProw)
                    End If

                    If T00015INProw("TODOKEDATE") = "" Then
                        WW_CheckMES1 = "・更新できないレコード(届日未入力)です。"
                        WW_CheckMES2 = ""
                        ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, T00015INProw)
                    End If
                    T00015INProw("KIJUNDATE") = T00015INProw("TODOKEDATE")
                Else
                    WW_CheckMES1 = "・更新できないレコード(荷主受注集計制御マスタ登録なし)です。"
                    WW_CheckMES2 = ""
                    ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, T00015INProw)
                End If

            End If

            '■■■ 権限チェック（更新権限） ■■■

            Dim WW_SHIPORG_ERR As String = ""


            '出荷部署
            If WW_SHIPORG_ERR = "ON" Then
                WW_CheckMES1 = "・更新できないレコード(出荷部署の権限無)です。"
                WW_CheckMES2 = ""
                ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, T00015INProw)
            End If

            If T00015INProw("DELFLG") = "" Then
                T00015INProw("DELFLG") = "0"
            End If

            '■ヘッダ項目(実績区分：JISSEKIKBN)
            '○必須・項目属性チェック
            WW_CS0024FCHECKVAL = T00015INProw("JISSEKIKBN")
            Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "JISSEKIKBN", WW_CS0024FCHECKVAL, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT, S0013tbl)
            If isNormal(WW_CS0024FCHECKERR) Then
                T00015INProw("JISSEKIKBN") = WW_CS0024FCHECKVAL
            Else
                WW_CheckMES1 = "・エラーが存在します。(実績区分)"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.BOX_ERROR_EXIST, T00015INProw)
            End If

            '■■■ 各種設定＆名称設定 ■■■

            '油種
            CODENAME_get("OILTYPE", T00015INProw("OILTYPE"), WW_TEXT, WW_DUMMY)
            T00015INProw("OILTYPENAME") = WW_TEXT

            '会社名称
            CODENAME_get("CAMPCODE", T00015INProw("CAMPCODE"), WW_TEXT, WW_DUMMY)
            T00015INProw("CAMPCODENAME") = WW_TEXT

            '品名１名称
            CODENAME_get("PRODUCT1", T00015INProw("PRODUCT1"), WW_TEXT, WW_DUMMY)
            T00015INProw("PRODUCT1NAME") = WW_TEXT

            '品名名称
            CODENAME_get("PRODUCTCODE", T00015INProw("PRODUCTCODE"), WW_TEXT, WW_DUMMY)
            T00015INProw("PRODUCTNAME") = WW_TEXT
            '品名２名称
            T00015INProw("PRODUCT2NAME") = WW_TEXT

            '業務車番名称
            CODENAME_get("GSHABAN", T00015INProw("GSHABAN"), WW_TEXT, WW_DUMMY)
            T00015INProw("GSHABANLICNPLTNO") = WW_TEXT

            '実績区分名称
            CODENAME_get("JISSEKIKBN", T00015INProw("JISSEKIKBN"), WW_TEXT, WW_DUMMY)
            T00015INProw("JISSEKIKBNNAME") = WW_TEXT

            Select Case WW_LINEerr
                Case C_MESSAGE_NO.NORMAL
                Case C_MESSAGE_NO.WORNING_RECORD_EXIST
                    T00015INProw("SELECT") = 1
                    T00015INProw("OPERATION") = C_LIST_OPERATION_CODE.WARNING
                Case C_MESSAGE_NO.BOX_ERROR_EXIST
                    T00015INProw("SELECT") = 1
                    T00015INProw("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                Case C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    T00015INProw("SELECT") = 1
                    T00015INProw("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End Select

        Next

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
    Protected Sub ERRMESSAGE_write(ByRef I_MESSAGE1 As String, ByRef I_MESSAGE2 As String, ByRef WW_LINEerr As String, ByRef i As Integer, ByVal I_ERRCD As String, ByVal T00015INProw As DataRow)

        'エラーレポート編集
        Dim WW_ERR_MES As String = String.Empty
        WW_ERR_MES = I_MESSAGE1
        If Not String.IsNullOrEmpty(I_MESSAGE2) Then
            WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> " & I_MESSAGE2 & " , "
        End If
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 項番　　= @L" & i.ToString("0000") & "L@ , "
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 明細番号= @D" & i.ToString("000") & "D@ , "
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 取引先　=" & T00015INProw("TORICODE") & " , "
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 届先　　=" & T00015INProw("TODOKECODE") & " , "
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 出荷場所=" & T00015INProw("SHUKABASHO") & " , "
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 出庫日　=" & T00015INProw("SHUKODATE") & " , "
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 届日　　=" & T00015INProw("TODOKEDATE") & " , "
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 出荷日　=" & T00015INProw("SHUKADATE") & " , "
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 車番　　=" & T00015INProw("GSHABAN") & " , "
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 乗務員　=" & T00015INProw("STAFFCODE") & " , "
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 品名  　=" & T00015INProw("PRODUCTCODE") & " , "
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> ﾄﾘｯﾌﾟ 　=" & T00015INProw("TRIPNO") & " , "
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> ﾄﾞﾛｯﾌﾟ　=" & T00015INProw("DROPNO") & " , "
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 削除　　=" & T00015INProw("DELFLG") & " "
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

        '取引先、油種、基準日（出荷日or届日）、受注部署、出荷部署、出庫日、業務車番、両目、トリップ、ドロップが同一
        If src("TORICODE") = dst("TORICODE") AndAlso
           src("OILTYPE") = dst("OILTYPE") AndAlso
           src("KIJUNDATE") = dst("KIJUNDATE") AndAlso
           src("SHIPORG") = dst("SHIPORG") AndAlso
           src("SHUKODATE") = dst("SHUKODATE") AndAlso
           src("GSHABAN") = dst("GSHABAN") AndAlso
           src("RYOME") = dst("RYOME") AndAlso
           src("TRIPNO") = dst("TRIPNO") AndAlso
           src("DROPNO") = dst("DROPNO") Then

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
        Master.RecoverTable(T00015tbl)

        'この段階でありえないがデータテーブルがない場合は終了
        If T00015tbl Is Nothing OrElse T00015tbl.Rows.Count = 0 Then
            Return
        End If

        'サフィックス抜き（LISTID)抜きのオブジェクト名リスト
        Dim objChkPrifix As String = "ctl00$contents1$chk" & Me.pnlListArea.ID
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
                For Each row In T00015tbl.Rows
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
        Master.SaveTable(T00015tbl)

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
            Master.RecoverTable(T00015tbl)
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
                    prmData = work.createTORIParam(work.WF_SEL_CAMPCODE.Text, WF_SHIPORG.Text)
                Case "WF_OILTYPE"                               '油種
                    prmData = work.createOilTypeParam(work.WF_SEL_CAMPCODE.Text)
                Case "WF_SHIPORG"                               '出荷部署
                    prmData = work.createORGParam(work.WF_SEL_CAMPCODE.Text, False)
                Case "WF_STAFFCODE",
                    "WF_SUBSTAFFCODE"                           '乗務員・副乗務員
                    prmData = work.createSTAFFParam(work.WF_SEL_CAMPCODE.Text, WF_SHIPORG.Text)
                Case "PRODUCT1"                                 '品名１
                    prmData = work.createGoods1Param(work.WF_SEL_CAMPCODE.Text)
                Case "PRODUCTCODE"                              '品名コード
                    prmData = work.createGoodsParam(work.WF_SEL_CAMPCODE.Text, WF_SHIPORG.Text, True)
                Case "TODOKECODE"                               '届先
                    prmData = work.createDistinationParam(work.WF_SEL_CAMPCODE.Text, WF_SHIPORG.Text, "", "1", True)
                Case "SHUKABASHO"                               '出荷場所
                    prmData = work.createDistinationParam(work.WF_SEL_CAMPCODE.Text, WF_SHIPORG.Text, "", "2", True)
                Case "WF_JISSEKIKBN"                            '実績区分
                    prmData = work.createFIXParam(work.WF_SEL_CAMPCODE.Text, "JISSEKIKBN")
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

                    If WF_FIELD.Value = "WF_STAFFCODE" Then
                        '乗務員の付加情報を設定
                        Dim datStaff As JOT_MASTER.STAFF = JOTMASTER.GetStaff(WW_SelectValue)
                        If Not IsNothing(datStaff) Then
                            Repeater_Value("STAFFNOTES1", datStaff.NOTES1, "D")
                            Repeater_Value("STAFFNOTES2", datStaff.NOTES2, "D")
                            Repeater_Value("STAFFNOTES3", datStaff.NOTES3, "D")
                            Repeater_Value("STAFFNOTES4", datStaff.NOTES4, "D")
                            Repeater_Value("STAFFNOTES5", datStaff.NOTES5, "D")
                        End If
                    End If

                Case "WF_GSHABAN"
                    '業務車番
                    WF_GSHABAN.Text = WW_SelectValue
                    Repeater_Value("SHARYOINFO1", WW_PARAM8, "H")
                    Repeater_Value("SHARYOINFO2", WW_PARAM9, "H")
                    Repeater_Value("SHARYOINFO3", WW_PARAM10, "H")
                    Repeater_Value("SHARYOINFO4", WW_PARAM11, "H")
                    Repeater_Value("SHARYOINFO5", WW_PARAM12, "H")
                    Repeater_Value("SHARYOINFO6", WW_PARAM13, "H")
                    WF_SHAFUKU.Text = WW_PARAM14
                    WF_TSHABANF.Text = WW_PARAM15
                    WF_TSHABANB.Text = WW_PARAM16
                    WF_TSHABANB2.Text = WW_PARAM17
                    WF_TSHABANF_TEXT.Text = WW_PARAM18
                    WF_TSHABANB_TEXT.Text = WW_PARAM19
                    WF_TSHABANB2_TEXT.Text = WW_PARAM20

                    WF_GSHABAN.Focus()

            End Select

        Else
            '変更
            WF_REP_Change.Value = "2"
            Dim exitFlg As Boolean = False
            '■■■ ディテール変数設定 ■■■
            For Each repItem In WF_DViewRep1.Items
                '[インデックス]が合致する場合
                If CType(repItem.FindControl("WF_Rep1_LINEPOSITION"), System.Web.UI.WebControls.TextBox).Text = WF_REP_POSITION.Value Then
                    For i As Integer = 1 To WF_REP_COLSCNT.Value
                        If CType(repItem.FindControl("WF_Rep1_FIELD_" & i), System.Web.UI.WebControls.Label).Text = WF_FIELD_REP.Value Then
                            CType(repItem.FindControl("WF_Rep1_VALUE_" & i), System.Web.UI.WebControls.TextBox).Text = WW_SelectValue
                            CType(repItem.FindControl("WF_Rep1_VALUE_TEXT_" & i), System.Web.UI.WebControls.Label).Text = WW_SelectTEXT
                            CType(repItem.FindControl("WF_Rep1_VALUE_" & i), System.Web.UI.WebControls.TextBox).Focus()
                            exitFlg = True
                            Exit For
                        End If
                    Next
                    '項目名が合致する場合
                    If exitFlg = True Then Exit For
                End If
            Next

            '届先コードの付加情報を設定
            If WF_FIELD_REP.Value = "TODOKECODE" Then
                Dim datTodoke As JOT_MASTER.TODOKESAKI = JOTMASTER.GetTodoke(WW_SelectValue)
                If Not IsNothing(datTodoke) Then
                    Repeater_Value("ADDR", datTodoke.ADDR, "D")
                    Repeater_Value("NOTES1", datTodoke.NOTES1, "D")
                    Repeater_Value("NOTES2", datTodoke.NOTES2, "D")
                    Repeater_Value("NOTES3", datTodoke.NOTES3, "D")
                    Repeater_Value("NOTES4", datTodoke.NOTES4, "D")
                    Repeater_Value("NOTES5", datTodoke.NOTES5, "D")
                End If
            End If

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
                Case "OILTYPE"
                    '油種名称
                    .CodeToName(LIST_BOX_CLASSIFICATION.LC_OILTYPE, I_VALUE, O_TEXT, O_RTN, work.createFIXParam(work.WF_SEL_CAMPCODE.Text))
                Case "PRODUCT1"
                    '品名１名称
                    .CodeToName(LIST_BOX_CLASSIFICATION.LC_GOODS, I_VALUE, O_TEXT, O_RTN, work.createGoods1Param(work.WF_SEL_CAMPCODE.Text))
                Case "PRODUCT2"
                    '品名２名称
                    .CodeToName(LIST_BOX_CLASSIFICATION.LC_GOODS, I_VALUE, O_TEXT, O_RTN, work.createGoods2Param(work.WF_SEL_CAMPCODE.Text))
                Case "PRODUCTCODE"
                    '品名名称
                    .CodeToName(LIST_BOX_CLASSIFICATION.LC_GOODS, I_VALUE, O_TEXT, O_RTN, work.createGoodsParam(work.WF_SEL_CAMPCODE.Text, work.WF_SEL_SHIPORG.Text))
                Case "SHUKABASHO"
                    '出荷場所名称
                    If IsNothing(args) Then
                        .CodeToName(LIST_BOX_CLASSIFICATION.LC_DISTINATION, I_VALUE, O_TEXT, O_RTN, work.createDistinationParam(work.WF_SEL_CAMPCODE.Text, work.WF_SEL_SHIPORG.Text, "", "2", True))
                    Else
                        .CodeToName(LIST_BOX_CLASSIFICATION.LC_DISTINATION, I_VALUE, O_TEXT, O_RTN, args)
                    End If
                Case "SHIPORG"
                    '出荷部署名称
                    .CodeToName(LIST_BOX_CLASSIFICATION.LC_ORG, I_VALUE, O_TEXT, O_RTN, work.createORGParam(work.WF_SEL_CAMPCODE.Text, False))
                Case "GSHABAN"
                    '業務車番名称
                    .CodeToName(LIST_BOX_CLASSIFICATION.LC_WORKLORRY, I_VALUE, O_TEXT, O_RTN, work.createWorkLorryParam(work.WF_SEL_CAMPCODE.Text, work.WF_SEL_SHIPORG.Text))
                Case "TSHABANF"
                    '統一車番名称
                    .CodeToName(LIST_BOX_CLASSIFICATION.LC_CARCODE, I_VALUE, O_TEXT, O_RTN, work.createCarCodeParam(work.WF_SEL_CAMPCODE.Text))
                Case "TSHABANB"
                    '統一車番名称
                    .CodeToName(LIST_BOX_CLASSIFICATION.LC_CARCODE, I_VALUE, O_TEXT, O_RTN, work.createCarCodeParam(work.WF_SEL_CAMPCODE.Text))
                Case "TSHABANB2"
                    '統一車番名称
                    .CodeToName(LIST_BOX_CLASSIFICATION.LC_CARCODE, I_VALUE, O_TEXT, O_RTN, work.createCarCodeParam(work.WF_SEL_CAMPCODE.Text))
                Case "STAFFCODE", "SUBSTAFFCODE"
                    '乗務員コード/副乗務員コード名称
                    .CodeToName(LIST_BOX_CLASSIFICATION.LC_STAFFCODE, I_VALUE, O_TEXT, O_RTN, work.createSTAFFParam(work.WF_SEL_CAMPCODE.Text, work.WF_SEL_SHIPORG.Text))
                Case "TODOKECODE"
                    '届先コード名称
                    If IsNothing(args) Then
                        .CodeToName(LIST_BOX_CLASSIFICATION.LC_DISTINATION, I_VALUE, O_TEXT, O_RTN, work.createDistinationParam(work.WF_SEL_CAMPCODE.Text, work.WF_SEL_SHIPORG.Text, "", "1", True))
                    Else
                        .CodeToName(LIST_BOX_CLASSIFICATION.LC_DISTINATION, I_VALUE, O_TEXT, O_RTN, args)
                    End If
                Case "DELFLG"
                    '削除名称
                    .CodeToName(LIST_BOX_CLASSIFICATION.LC_DELFLG, I_VALUE, O_TEXT, O_RTN, work.createFIXParam(work.WF_SEL_CAMPCODE.Text, "DELFLG"))
                Case "JISSEKIKBN"
                    '実績区分名称
                    .CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.createFIXParam(work.WF_SEL_CAMPCODE.Text, "JISSEKIKBN"))

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
    ''' LeftBOX選択値に伴う項目内容変更
    ''' </summary>
    Protected Sub Repeater_Value(ByVal I_FIELD As String, ByVal I_Value As String, I_HDKBN As String)

        Dim WW_ROWF As Integer = 0
        Dim WW_ROWT As Integer = 0
        'リピータの何明細目かを表す
        Dim WW_CNT As Integer = 0
        Dim WW_CNT2 As Integer = 0

        Dim WW_TEXT As String = ""

        If I_HDKBN = "H" Then
        Else
            '選択された行数　－　１明細の行数を繰り返し、何明細目がを判定
            WW_CNT2 = Int32.TryParse(WF_REP_POSITION.Value, WW_CNT2)
            Do While WW_CNT2 > WF_REP_ROWSCNT.Value
                WW_CNT2 = WW_CNT2 - WF_REP_ROWSCNT.Value
                WW_CNT += 1
            Loop
        End If

        WW_ROWF = WW_CNT * WF_REP_ROWSCNT.Value + 1
        WW_ROWT = WW_ROWF + WF_REP_ROWSCNT.Value - 1


        For i As Integer = WW_ROWF To WW_ROWT - 1
            For col As Integer = 1 To WF_REP_COLSCNT.Value
                If CType(WF_DViewRep1.Items(i).FindControl("WF_Rep1_FIELD_" & col), System.Web.UI.WebControls.Label).Text = I_FIELD Then
                    CType(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_" & col), System.Web.UI.WebControls.TextBox).Text = I_Value
                    CType(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_TEXT_" & col), System.Web.UI.WebControls.Label).Text = WW_TEXT
                    Exit For
                End If
            Next
        Next

    End Sub


    ''' <summary>
    ''' LeftBox業務車番データ取得
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub InitGSHABAN()

        '出庫日を取得
        Dim WW_SHUKODATE As Date
        If Date.TryParse(WF_SHUKODATE.Text, WW_SHUKODATE) Then
            If WW_SHUKODATE < C_DEFAULT_YMD Then
                WW_SHUKODATE = Date.Now
            End If
        Else
            WW_SHUKODATE = Date.Now
        End If

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
                   & "    and H.ENDYMD     >= @P1                           " _
                   & "    and H.DELFLG     <> '1' 						    " _
                   & "  Where A.CAMPCODE  = @P2                             " _
                   & "    and A.MANGUORG  = @P3                             " _
                   & "    and A.DELFLG   <> '1'                             " _
                   & "  ORDER BY A.SEQ ,A.GSHABAN                           "

                Dim SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.Date)
                Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.NVarChar)
                Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.NVarChar)
                PARA1.Value = WW_SHUKODATE
                PARA2.Value = work.WF_SEL_CAMPCODE.Text
                If String.IsNullOrEmpty(work.WF_SEL_SHIPORG.Text) Then
                    PARA3.Value = WF_DEFORG.Text
                Else
                    PARA3.Value = work.WF_SEL_SHIPORG.Text
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

        '出庫日を取得
        Dim WW_SHUKODATE As Date
        If Date.TryParse(WF_SHUKODATE.Text, WW_SHUKODATE) Then
            If WW_SHUKODATE < C_DEFAULT_YMD Then
                WW_SHUKODATE = Date.Now
            End If
        Else
            WW_SHUKODATE = Date.Now
        End If

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
        Dim WW_TBLVIEW As DataView = New DataView(T00015tbl)
        WW_TBLVIEW.Sort = "SHUKODATE , GSHABAN"

        For Each GSHABANrow In GSHABANtbl.Rows

            '業務車番・出庫日が合致する場合
            WW_TBLVIEW.RowFilter = "GSHABAN = '" & GSHABANrow("GSHABAN") & "' and " & "SHUKODATE = '" & WW_SHUKODATE.ToString("yyyy/MM/dd") & "'"

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
        Master.RecoverTable(T00015tbl)


        '■■■ UPLOAD_XLSデータ取得 ■■■
        If work.WF_SEL_CAMPCODE.Text = GRT00015WRKINC.C_CAMPCODE.NJS Then
            XLStoINPtblForNJS(WW_ERRCODE)
        Else
            XLStoINPtbl(WW_ERRCODE)
        End If
        If Not isNormal(WW_ERRCODE) Then
            Master.Output(WW_ERRCODE, C_MESSAGE_TYPE.ERR)
            Exit Sub
        End If

        '■■■ INPデータ登録 ■■■
        INPtbltoT15tbl(WW_ERRCODE)

        '■■■ GridView更新 ■■■
        ' 状態クリア
        EditOperationText(T00015tbl, False)

        '○サマリ処理 
        CS0026TBLSORTget.TABLE = T00015tbl
        CS0026TBLSORTget.SORTING = "LINECNT ASC , SEQ ASC"
        CS0026TBLSORTget.FILTER = ""
        CS0026TBLSORTget.Sort(T00015tbl)
        SUMMRY_SET()

        'エラーメッセージ内の項番、明細番号置き換え
        Dim WW_ERRWORD As String = rightview.GetErrorReport()
        For i As Integer = 0 To T00015INPtbl.Rows.Count - 1
            '項番
            WW_ERRWORD = WW_ERRWORD.Replace("@L" & i.ToString("0000") & "L@", T00015INPtbl.Rows(i)("LINECNT"))
            '明細番号
            WW_ERRWORD = WW_ERRWORD.Replace("@D" & i.ToString("000") & "D@", T00015INPtbl.Rows(i)("SEQ"))
        Next
        rightview.SetErrorReport(WW_ERRWORD)

        '○画面表示データ保存
        Master.SaveTable(T00015tbl)

        '○Detailクリア
        'detailboxヘッダークリア
        ClearDetailBox()

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
        T00015INPtbl.Clear()

        'カーソル設定
        WF_FIELD.Value = "WF_SELTORICODE"
        WF_SELTORICODE.Focus()

    End Sub

    ''' <summary>
    ''' Excel→T00015tbl処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub XLStoINPtbl(ByRef O_RTN As String)

        '■■■ UPLOAD_XLSデータ取得 ■■■
        CS0023XLSUPLOAD.CAMPCODE = work.WF_SEL_CAMPCODE.Text
        CS0023XLSUPLOAD.MAPID = GRT00015WRKINC.MAPID
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

        '○T00015INPtblカラム設定
        Master.CreateEmptyTable(T00015INPtbl)

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

            '○XLSTBL明細⇒T00015INProw
            Dim T00015INProw = T00015INPtbl.NewRow

            T00015INProw("LINECNT") = 0
            T00015INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA
            T00015INProw("TIMSTP") = "0"
            T00015INProw("SELECT") = 1
            T00015INProw("HIDDEN") = 0

            T00015INProw("INDEX") = WW_INDEX
            WW_INDEX += WW_INDEX

            T00015INProw("CAMPCODE") = work.WF_SEL_CAMPCODE.Text


            If WW_COLUMNS.IndexOf("ORDERNO") < 0 Then
                T00015INProw("ORDERNO") = ""
            Else
                T00015INProw("ORDERNO") = uploadRow("ORDERNO")
            End If

            If WW_COLUMNS.IndexOf("DETAILNO") < 0 Then
                T00015INProw("DETAILNO") = ""
            Else
                T00015INProw("DETAILNO") = uploadRow("DETAILNO")
            End If

            If WW_COLUMNS.IndexOf("OILTYPE") < 0 Then
                T00015INProw("OILTYPE") = ""
            Else
                T00015INProw("OILTYPE") = uploadRow("OILTYPE")
            End If

            If WW_COLUMNS.IndexOf("TRIPNO") < 0 Then
                T00015INProw("TRIPNO") = ""
            Else
                T00015INProw("TRIPNO") = uploadRow("TRIPNO")
            End If

            If WW_COLUMNS.IndexOf("DROPNO") < 0 Then
                T00015INProw("DROPNO") = ""
            Else
                T00015INProw("DROPNO") = uploadRow("DROPNO")
            End If

            If WW_COLUMNS.IndexOf("SEQ") < 0 Then
                T00015INProw("SEQ") = ""
            Else
                T00015INProw("SEQ") = uploadRow("SEQ")
            End If

            If WW_COLUMNS.IndexOf("TORICODE") < 0 Then
                T00015INProw("TORICODE") = ""
            Else
                T00015INProw("TORICODE") = uploadRow("TORICODE")
            End If

            If WW_COLUMNS.IndexOf("SHUKODATE") < 0 Then
                T00015INProw("SHUKODATE") = ""
            Else
                T00015INProw("SHUKODATE") = uploadRow("SHUKODATE")
            End If

            If WW_COLUMNS.IndexOf("KIKODATE") < 0 Then
                T00015INProw("KIKODATE") = ""
            Else
                T00015INProw("KIKODATE") = uploadRow("KIKODATE")
            End If

            If WW_COLUMNS.IndexOf("KIJUNDATE") < 0 Then
                T00015INProw("KIJUNDATE") = ""
            Else
                T00015INProw("KIJUNDATE") = uploadRow("KIJUNDATE")
            End If

            If WW_COLUMNS.IndexOf("SHUKADATE") < 0 Then
                T00015INProw("SHUKADATE") = ""
            Else
                T00015INProw("SHUKADATE") = uploadRow("SHUKADATE")
            End If

            If WW_COLUMNS.IndexOf("SHIPORG") < 0 Then
                T00015INProw("SHIPORG") = WF_DEFORG.Text
            Else
                T00015INProw("SHIPORG") = uploadRow("SHIPORG").ToString.PadLeft(WF_DEFORG.Text.Length, "0")
            End If

            If WW_COLUMNS.IndexOf("SHUKABASHO") < 0 Then
                T00015INProw("SHUKABASHO") = ""
            Else
                T00015INProw("SHUKABASHO") = uploadRow("SHUKABASHO")
            End If

            If WW_COLUMNS.IndexOf("GSHABAN") < 0 Then
                T00015INProw("GSHABAN") = ""
            Else
                T00015INProw("GSHABAN") = uploadRow("GSHABAN")
            End If

            If WW_COLUMNS.IndexOf("RYOME") < 0 Then
                T00015INProw("RYOME") = "1"
            Else
                If uploadRow("RYOME") = Nothing Then
                    T00015INProw("RYOME") = "1"
                Else
                    T00015INProw("RYOME") = uploadRow("RYOME")
                End If
            End If

            If WW_COLUMNS.IndexOf("SHAFUKU") < 0 Then
                T00015INProw("SHAFUKU") = ""
            Else
                T00015INProw("SHAFUKU") = uploadRow("SHAFUKU")
            End If

            If WW_COLUMNS.IndexOf("STAFFCODE") < 0 Then
                T00015INProw("STAFFCODE") = ""
            Else
                T00015INProw("STAFFCODE") = uploadRow("STAFFCODE")
            End If

            If WW_COLUMNS.IndexOf("SUBSTAFFCODE") < 0 Then
                T00015INProw("SUBSTAFFCODE") = ""
            Else
                T00015INProw("SUBSTAFFCODE") = uploadRow("SUBSTAFFCODE")
            End If

            If WW_COLUMNS.IndexOf("TODOKEDATE") < 0 Then
                T00015INProw("TODOKEDATE") = ""
            Else
                T00015INProw("TODOKEDATE") = uploadRow("TODOKEDATE")
            End If

            If WW_COLUMNS.IndexOf("TODOKECODE") < 0 Then
                T00015INProw("TODOKECODE") = ""
            Else
                T00015INProw("TODOKECODE") = uploadRow("TODOKECODE")
            End If

            If WW_COLUMNS.IndexOf("PRODUCT1") < 0 Then
                T00015INProw("PRODUCT1") = ""
            Else
                T00015INProw("PRODUCT1") = uploadRow("PRODUCT1")
            End If

            If WW_COLUMNS.IndexOf("PRODUCT2") < 0 Then
                T00015INProw("PRODUCT2") = ""
            Else
                T00015INProw("PRODUCT2") = uploadRow("PRODUCT2")
            End If

            If WW_COLUMNS.IndexOf("PRODUCTCODE") < 0 Then
                T00015INProw("PRODUCTCODE") = ""
            Else
                T00015INProw("PRODUCTCODE") = uploadRow("PRODUCTCODE")
            End If

            If WW_COLUMNS.IndexOf("CONTNO") < 0 Then
                T00015INProw("CONTNO") = ""
            Else
                T00015INProw("CONTNO") = uploadRow("CONTNO")
            End If

            If WW_COLUMNS.IndexOf("JSURYO") < 0 Then
                T00015INProw("JSURYO") = ""
            Else
                T00015INProw("JSURYO") = uploadRow("JSURYO")
            End If

            If WW_COLUMNS.IndexOf("JDAISU") < 0 Then
                T00015INProw("JDAISU") = ""
            Else
                T00015INProw("JDAISU") = uploadRow("JDAISU")
            End If

            If WW_COLUMNS.IndexOf("REMARKS1") < 0 Then
                T00015INProw("REMARKS1") = ""
            Else
                T00015INProw("REMARKS1") = uploadRow("REMARKS1")
            End If

            If WW_COLUMNS.IndexOf("REMARKS2") < 0 Then
                T00015INProw("REMARKS2") = ""
            Else
                T00015INProw("REMARKS2") = uploadRow("REMARKS2")
            End If

            If WW_COLUMNS.IndexOf("REMARKS3") < 0 Then
                T00015INProw("REMARKS3") = ""
            Else
                T00015INProw("REMARKS3") = uploadRow("REMARKS3")
            End If

            If WW_COLUMNS.IndexOf("REMARKS4") < 0 Then
                T00015INProw("REMARKS4") = ""
            Else
                T00015INProw("REMARKS4") = uploadRow("REMARKS4")
            End If

            If WW_COLUMNS.IndexOf("REMARKS5") < 0 Then
                T00015INProw("REMARKS5") = ""
            Else
                T00015INProw("REMARKS5") = uploadRow("REMARKS5")
            End If

            If WW_COLUMNS.IndexOf("REMARKS6") < 0 Then
                T00015INProw("REMARKS6") = ""
            Else
                T00015INProw("REMARKS6") = uploadRow("REMARKS6")
            End If

            If WW_COLUMNS.IndexOf("DELFLG") < 0 Then
                T00015INProw("DELFLG") = "0"
            Else
                T00015INProw("DELFLG") = uploadRow("DELFLG")
            End If

            If WW_COLUMNS.IndexOf("SHARYOTYPEF") < 0 Then
                T00015INProw("SHARYOTYPEF") = ""
            Else
                T00015INProw("SHARYOTYPEF") = uploadRow("SHARYOTYPEF")
            End If

            If WW_COLUMNS.IndexOf("SHARYOTYPEB") < 0 Then
                T00015INProw("SHARYOTYPEB") = ""
            Else
                T00015INProw("SHARYOTYPEB") = uploadRow("SHARYOTYPEB")
            End If

            If WW_COLUMNS.IndexOf("SHARYOTYPEB2") < 0 Then
                T00015INProw("SHARYOTYPEB2") = ""
            Else
                T00015INProw("SHARYOTYPEB2") = uploadRow("SHARYOTYPEB2")
            End If

            If WW_COLUMNS.IndexOf("JISSEKIKBN") < 0 Then
                T00015INProw("JISSEKIKBN") = ""
            Else
                T00015INProw("JISSEKIKBN") = uploadRow("JISSEKIKBN")
            End If

            'Grid追加明細（新規追加と同じ）とする
            T00015INProw("WORK_NO") = ""

            '■■■ 数量ゼロは読み飛ばし ■■■
            If Val(T00015INProw("JSURYO")) = 0 Then
                Continue For
            End If

            '品名コード未存在時は油種・品名1・品名2から作成
            If WW_COLUMNS.IndexOf("PRODUCTCODE") < 0 Then
                If Not String.IsNullOrEmpty(T00015INProw("OILTYPE")) AndAlso
                    Not String.IsNullOrEmpty(T00015INProw("PRODUCT1")) AndAlso
                    Not String.IsNullOrEmpty(T00015INProw("PRODUCT2")) Then
                    T00015INProw("PRODUCTCODE") = T00015INProw("CAMPCODE").ToString.PadLeft(2, "0") & T00015INProw("OILTYPE").ToString.PadLeft(2, "0") & T00015INProw("PRODUCT1").ToString.PadLeft(2, "0") & T00015INProw("PRODUCT2").ToString.PadLeft(5, "0")
                End If
            ElseIf Not String.IsNullOrEmpty(T00015INProw("PRODUCTCODE")) AndAlso T00015INProw("PRODUCTCODE").ToString.Length = 11 Then
                '油種未存在は品名コードから作成
                If WW_COLUMNS.IndexOf("OILTYPE") < 0 Then
                    T00015INProw("OILTYPE") = Mid(T00015INProw("PRODUCTCODE").ToString, 3, 2)
                End If
                '品名１未存在は品名コードから作成
                If WW_COLUMNS.IndexOf("PRODUCT1") < 0 Then
                    T00015INProw("PRODUCT1") = Mid(T00015INProw("PRODUCTCODE").ToString, 5, 2)
                End If
                '品名２未存在は品名コードから作成
                If WW_COLUMNS.IndexOf("PRODUCT2") < 0 Then
                    T00015INProw("PRODUCT2") = Mid(T00015INProw("PRODUCTCODE").ToString, 7, 5)
                End If

            End If


            '○名称付与
            CODENAME_set(T00015INProw)

            '入力テーブル追加
            T00015INPtbl.Rows.Add(T00015INProw)

        Next

    End Sub

    ''' <summary>
    ''' NJS Excel→T00015tbl処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub XLStoINPtblForNJS(ByRef O_RTN As String)

        '■■■ UPLOAD_XLSデータ取得 ■■■   ☆☆☆ 2015/4/30追加
        CS0023XLSUPLOAD.CAMPCODE = work.WF_SEL_CAMPCODE.Text
        CS0023XLSUPLOAD.MAPID = GRT00015WRKINC.MAPID
        CS0023XLSUPLOAD.CS0023XLSUPLOAD(C_UPLOAD_EXCEL_REPORTID_NJS, C_UPLOAD_EXCEL_PROFID_NJS)
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

        '○T00015INPtblカラム設定
        Master.CreateEmptyTable(T00015INPtbl)

        '○必須項目の指定チェック
        If CS0023XLSUPLOAD.TBLDATA.Columns.Contains("SHUKODATE") AndAlso
            CS0023XLSUPLOAD.TBLDATA.Columns.Contains("TODOKEDATE") AndAlso
            CS0023XLSUPLOAD.TBLDATA.Columns.Contains("TODOKECODE") AndAlso
            CS0023XLSUPLOAD.TBLDATA.Columns.Contains("PRODUCTCODE") AndAlso
            CS0023XLSUPLOAD.TBLDATA.Columns.Contains("JSURYO") AndAlso
            CS0023XLSUPLOAD.TBLDATA.Columns.Contains("SHARYOCD") AndAlso
            CS0023XLSUPLOAD.TBLDATA.Columns.Contains("STAFFCODE1") AndAlso
            CS0023XLSUPLOAD.TBLDATA.Columns.Contains("STAFFCODE2") Then
        Else
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            rightview.AddErrorReport("・アップロードExcelに『出庫日、納入日、届先コード、品名コード、数量、車輛コード、運転手コード1、運転手コード2』が存在しません。")
            Exit Sub
        End If

        '○JSRコードマスタ作成
        Using jsrCvt As JSRCODE_MASTER = New JSRCODE_MASTER()
            jsrCvt.CAMPCODE = work.WF_SEL_CAMPCODE.Text
            If String.IsNullOrEmpty(work.WF_SEL_SHIPORG.Text) Then
                jsrCvt.ORGCODE = WF_DEFORG.Text
            Else
                jsrCvt.ORGCODE = work.WF_SEL_SHIPORG.Text
            End If
            'JSRコード一括読込
            If jsrCvt.ReadJSRData() = False Then
                O_RTN = jsrCvt.ERR
                Master.Output(O_RTN, C_MESSAGE_TYPE.ABORT, "read JSRCODE")
                Exit Sub
            End If

            '■■■ Excelデータ毎にチェック＆更新 ■■■
            Dim WW_INDEX As Integer = 0
            '配送時刻順でトリップ作成の為にソート
            Dim uploadData = CS0023XLSUPLOAD.TBLDATA.
                AsEnumerable.
                OrderBy(Function(x) x.Item("SHARYOCD")).
                ThenBy(Function(x) x.Item("TODOKEDATE")).
                ThenBy(Function(x) x.Item("WORKTIME3"))
            For Each uploadRow As DataRow In uploadData

                Dim datTodoke = New JSRCODE_MASTER.JSRCODE_TODOKE
                Dim datProduct = New JSRCODE_MASTER.JSRCODE_PRODUCT
                Dim datStaff = New JSRCODE_MASTER.JSRCODE_STAFF
                Dim datSubStaff = New JSRCODE_MASTER.JSRCODE_STAFF
                Dim WW_SHUKODATE As Date
                Dim WW_SHUKADATE As Date
                Dim WW_TODOKEDATE As Date
                Dim WW_KIKODATE As Date
                Dim WW_RELATIVEDAYS3 As Integer
                Dim WW_RELATIVEDAYS4 As Integer

                '2:出庫日
                If Not DateTime.TryParseExact(uploadRow("SHUKODATE"), "yyyyMMdd", Nothing, Nothing, WW_SHUKODATE) Then
                    O_RTN = C_MESSAGE_NO.DATE_FORMAT_ERROR
                    rightview.AddErrorReport("・アップロードExcel『出庫日』の日付書式が正しくありません。")
                    Exit Sub
                End If
                '3:納入日
                If Not DateTime.TryParseExact(uploadRow("TODOKEDATE"), "yyyyMMdd", Nothing, Nothing, WW_TODOKEDATE) Then
                    O_RTN = C_MESSAGE_NO.DATE_FORMAT_ERROR
                    rightview.AddErrorReport("・アップロードExcel『納入日』の日付書式が正しくありません。")
                    Exit Sub
                End If
                '20:相対日数１（出荷（積））
                '21:相対日数２（出発）
                '22:相対日数３（納入）
                '23:相対日数４（帰庫）
                '24:相対日数５（点検）
                If WW_COLUMNS.Contains("RELATIVEDAYS3") Then
                    If Not Int32.TryParse(uploadRow("RELATIVEDAYS3"), WW_RELATIVEDAYS3) Then
                        WW_RELATIVEDAYS3 = 0
                    End If
                End If
                If WW_COLUMNS.Contains("RELATIVEDAYS4") Then
                    If Not Int32.TryParse(uploadRow("RELATIVEDAYS4"), WW_RELATIVEDAYS4) Then
                        WW_RELATIVEDAYS4 = 0
                    End If
                End If

                '出庫日    ＝ 出荷日
                '出荷日    ＝ 出庫日
                '届日      ＝ 納入日
                '出荷日    ＝ 出荷日
                WW_SHUKADATE = WW_SHUKODATE
                WW_SHUKODATE = WW_SHUKODATE.AddDays(WW_RELATIVEDAYS3)
                WW_TODOKEDATE = WW_TODOKEDATE
                WW_KIKODATE = WW_TODOKEDATE.AddDays(WW_RELATIVEDAYS4)

                '***** 取込除外条件 *****
                ' ①運転手コード１
                '  -a NULL
                '  -b 0000
                '  -c 0001
                If uploadRow("STAFFCODE1") = String.Empty OrElse
                    uploadRow("STAFFCODE1") = "0000" OrElse
                    uploadRow("STAFFCODE1") = "0001" Then
                    Continue For
                End If
                ' ②車輛コード
                '  -a NULL
                '  -b 9XX
                If uploadRow("SHARYOCD") = String.Empty OrElse
                    uploadRow("SHARYOCD").ToString.StartsWith("9") Then
                    Continue For
                End If
                ' ③届日（納入日）
                '  -a 当日以前
                If WW_TODOKEDATE <= CS0050SESSION.LOGONDATE Then
                    Continue For
                End If

                If WW_COLUMNS.Contains("TODOKECODE") Then
                    datTodoke = jsrCvt.GetTodokeCode(uploadRow("TODOKECODE"))
                    If IsNothing(datTodoke) Then
                        Dim WW_CheckMES1 = "・変換エラーが存在します。(届先コード)"
                        Dim WW_CheckMES2 = uploadRow("TODOKECODE")
                        ERRMESSAGE_write_NJS(WW_CheckMES1, WW_CheckMES2, WW_DUMMY, WW_INDEX + 1, C_MESSAGE_NO.BOX_ERROR_EXIST, uploadRow)
                    End If
                    'グループ作業用届先は除外
                    If datTodoke.IsGroupWork Then
                        Continue For
                    End If
                End If
                If WW_COLUMNS.Contains("PRODUCTCODE") Then
                    If jsrCvt.CovertProductCode(uploadRow("PRODUCTCODE"), datProduct) = False Then
                        Dim WW_CheckMES1 = "・変換エラーが存在します。(品名コード)"
                        Dim WW_CheckMES2 = uploadRow("PRODUCTCODE")
                        ERRMESSAGE_write_NJS(WW_CheckMES1, WW_CheckMES2, WW_DUMMY, WW_INDEX + 1, C_MESSAGE_NO.BOX_ERROR_EXIST, uploadRow)
                    End If
                End If
                If WW_COLUMNS.Contains("STAFFCODE1") AndAlso
                    Not String.IsNullOrEmpty(uploadRow("STAFFCODE1")) Then
                    If jsrCvt.CovertStaffCode(uploadRow("STAFFCODE1"), datStaff) = False Then
                        Dim WW_CheckMES1 = "・変換エラーが存在します。(運転手コード1)"
                        Dim WW_CheckMES2 = uploadRow("STAFFCODE1")
                        ERRMESSAGE_write_NJS(WW_CheckMES1, WW_CheckMES2, WW_DUMMY, WW_INDEX + 1, C_MESSAGE_NO.BOX_ERROR_EXIST, uploadRow)
                    End If
                End If
                If WW_COLUMNS.Contains("STAFFCODE2") AndAlso
                    Not String.IsNullOrEmpty(uploadRow("STAFFCODE2")) Then
                    If jsrCvt.CovertStaffCode(uploadRow("STAFFCODE2"), datSubStaff) = False Then
                        Dim WW_CheckMES1 = "・変換エラーが存在します。(運転手コード2)"
                        Dim WW_CheckMES2 = uploadRow("STAFFCODE2")
                        ERRMESSAGE_write_NJS(WW_CheckMES1, WW_CheckMES2, WW_DUMMY, WW_INDEX + 1, C_MESSAGE_NO.BOX_ERROR_EXIST, uploadRow)
                    End If
                End If


                '○XLSTBL明細⇒T00015INProw
                Dim T00015INProw = T00015INPtbl.NewRow
                '***** T15項目順に編集 *****
                T00015INProw("LINECNT") = 0
                T00015INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                T00015INProw("TIMSTP") = "0"
                T00015INProw("SELECT") = 1
                T00015INProw("HIDDEN") = 0
                T00015INProw("INDEX") = WW_INDEX
                WW_INDEX += WW_INDEX

                T00015INProw("CAMPCODE") = work.WF_SEL_CAMPCODE.Text
                If String.IsNullOrEmpty(datTodoke.TORICODE) Then
                    T00015INProw("TORICODE") = ""
                Else
                    T00015INProw("TORICODE") = datTodoke.TORICODE
                End If
                If String.IsNullOrEmpty(datProduct.OILTYPE) Then
                    T00015INProw("OILTYPE") = ""
                Else
                    T00015INProw("OILTYPE") = datProduct.OILTYPE
                End If
                If String.IsNullOrEmpty(work.WF_SEL_SHIPORG.Text) Then
                    T00015INProw("SHIPORG") = WF_DEFORG.Text
                Else
                    T00015INProw("SHIPORG") = work.WF_SEL_SHIPORG.Text
                End If

                T00015INProw("KIJUNDATE") = ""                                      ' T3CTLから設定
                T00015INProw("ORDERNO") = ""                                        ' 後続で受注番号自動設定
                T00015INProw("DETAILNO") = "001"
                '車番下３桁のみ使用
                T00015INProw("GSHABAN") = uploadRow("SHARYOCD").ToString.PadLeft(20, "0").Substring(20 - 3)
                'トリップは暫定（同一出庫日存在時は編集）
                T00015INProw("TRIPNO") = "001"
                T00015INProw("DROPNO") = "001"
                T00015INProw("SEQ") = "01"
                If IsNothing(WW_SHUKODATE) Then
                    T00015INProw("SHUKODATE") = ""
                Else
                    T00015INProw("SHUKODATE") = WW_SHUKODATE.ToString("yyyy/MM/dd")
                End If
                If IsNothing(WW_KIKODATE) Then
                    T00015INProw("KIKODATE") = ""
                Else
                    T00015INProw("KIKODATE") = WW_KIKODATE.ToString("yyyy/MM/dd")
                End If
                '出庫日→出荷日
                If IsNothing(WW_SHUKADATE) Then
                    T00015INProw("SHUKADATE") = ""
                Else
                    T00015INProw("SHUKADATE") = WW_SHUKADATE.ToString("yyyy/MM/dd")
                End If
                If IsNothing(WW_TODOKEDATE) Then
                    T00015INProw("TODOKEDATE") = ""
                Else
                    T00015INProw("TODOKEDATE") = WW_TODOKEDATE.ToString("yyyy/MM/dd")
                End If
                If String.IsNullOrEmpty(datTodoke.SHUKABASHO) Then
                    T00015INProw("SHUKABASHO") = ""
                Else
                    T00015INProw("SHUKABASHO") = datTodoke.SHUKABASHO
                End If
                If String.IsNullOrEmpty(datStaff.STAFFCODE) Then
                    T00015INProw("STAFFCODE") = uploadRow("STAFFCODE1")
                Else
                    T00015INProw("STAFFCODE") = datStaff.STAFFCODE
                End If
                If String.IsNullOrEmpty(datSubStaff.STAFFCODE) Then
                    T00015INProw("SUBSTAFFCODE") = uploadRow("STAFFCODE2")
                Else
                    T00015INProw("SUBSTAFFCODE") = datSubStaff.STAFFCODE
                End If
                T00015INProw("RYOME") = "1"
                If String.IsNullOrEmpty(datTodoke.TODOKECODE) Then
                    'T00015INProw("TODOKECODE") = uploadRow("TODOKECODE")
                    T00015INProw("TODOKECODE") = "!" & datTodoke.JSRTODOKECODE & "!"
                Else
                    T00015INProw("TODOKECODE") = datTodoke.TODOKECODE
                End If
                If String.IsNullOrEmpty(datProduct.PRODUCT1) Then
                    T00015INProw("PRODUCT1") = ""
                Else
                    T00015INProw("PRODUCT1") = datProduct.PRODUCT1
                End If
                If String.IsNullOrEmpty(datProduct.PRODUCT2) Then
                    T00015INProw("PRODUCT2") = ""
                Else
                    T00015INProw("PRODUCT2") = datProduct.PRODUCT2
                End If
                If String.IsNullOrEmpty(datProduct.PRODUCTCODE) Then
                    'T00015INProw("PRODUCTCODE") = uploadRow("PRODUCTCODE")
                    T00015INProw("PRODUCTCODE") = "!" & datProduct.JSRPRODUCT & "!"
                Else
                    T00015INProw("PRODUCTCODE") = datProduct.PRODUCTCODE
                End If
                T00015INProw("CONTNO") = ""
                T00015INProw("SHAFUKU") = ""
                If WW_COLUMNS.Contains("JSURYO") Then
                    '数量単位 NJS(L)→JOT(kL)
                    T00015INProw("JSURYO") = uploadRow("JSURYO") / 1000
                Else
                    T00015INProw("JSURYO") = "0"
                End If
                T00015INProw("JDAISU") = "1"
                'T00015INProw("JSURYO") = "0"
                'T00015INProw("JDAISU") = "0"
                '契約番号
                If WW_COLUMNS.Contains("CONTRACTNO") Then
                    T00015INProw("REMARKS1") = uploadRow("CONTRACTNO")
                Else
                    T00015INProw("REMARKS1") = ""
                End If
                '社内備考
                If WW_COLUMNS.Contains("SYANAINOTES") Then
                    T00015INProw("REMARKS2") = uploadRow("SYANAINOTES")
                Else
                    T00015INProw("REMARKS2") = ""
                End If
                '社外備考
                If WW_COLUMNS.Contains("SYAGAINOTES") Then
                    T00015INProw("REMARKS3") = uploadRow("SYAGAINOTES")
                Else
                    T00015INProw("REMARKS3") = ""
                End If
                T00015INProw("REMARKS4") = ""
                T00015INProw("REMARKS5") = ""
                T00015INProw("REMARKS6") = ""
                ' T3CTLから設定
                T00015INProw("SHARYOTYPEF") = ""
                T00015INProw("TSHABANF") = ""
                T00015INProw("SHARYOTYPEB") = ""
                T00015INProw("TSHABANB") = ""
                T00015INProw("SHARYOTYPEB2") = ""
                T00015INProw("TSHABANB2") = ""
                T00015INProw("JISSEKIKBN") = "1"
                T00015INProw("DELFLG") = "0"

                'Grid追加明細（新規追加と同じ）とする
                T00015INProw("WORK_NO") = ""

                '○名称付与
                CODENAME_set(T00015INProw)

                '同一出庫日最新トリップ№取得
                Dim latestTrip As Integer = GetLatestTripNo(T00015INProw)

                Dim tripCnt As Integer = latestTrip + 1
                T00015INProw("TRIPNO") = tripCnt.ToString("000")

                '入力テーブル追加
                T00015INPtbl.Rows.Add(T00015INProw)

                '****************************
                'トリップ増幅
                '  ※個数が２以上にレコード編集後に複製
                '****************************
                '個数＝トリップ
                Dim num As Integer
                If WW_COLUMNS.Contains("NUM") Then
                    num = uploadRow("NUM")
                Else
                    num = 1
                End If
                For i As Integer = 2 To num
                    Dim T00015INPAddrow = T00015INPtbl.NewRow()
                    T00015INPAddrow.ItemArray = T00015INProw.ItemArray
                    tripCnt += 1
                    T00015INPAddrow("TRIPNO") = tripCnt.ToString("000")
                    T00015INPtbl.Rows.Add(T00015INPAddrow)
                Next

                '****************************
                '日跨ぎデータ増幅 WW_KIKODATE
                '　※出庫～届日までの日数分（修正前）
                '　※出庫～帰庫までの日数分（修正後　2020/10/28）
                '****************************
                If Not IsNothing(WW_SHUKODATE) AndAlso Not IsNothing(WW_KIKODATE) Then
                    Dim days As Integer = (WW_KIKODATE - WW_SHUKODATE).Days
                    For i As Integer = 1 To days
                        Dim T00015INPAddrow = T00015INPtbl.NewRow()
                        T00015INPAddrow.ItemArray = T00015INProw.ItemArray
                        '
                        T00015INPAddrow("SHUKODATE") = WW_SHUKODATE.AddDays(i).ToString("yyyy/MM/dd")
                        '増幅時のトリップは001にリセット
                        T00015INPAddrow("TRIPNO") = "001"
                        T00015INPtbl.Rows.Add(T00015INPAddrow)
                    Next
                End If

            Next

        End Using

    End Sub
    ''' <summary>
    ''' NJS同一出庫日最大トリップ№取得
    ''' </summary>
    ''' <remarks></remarks>
    Protected Function GetLatestTripNo(ByVal row As DataRow) As Integer
        Dim tripNo As Integer = 0

        '入力・一覧データテーブル両方から対象車番の同一出庫日レコードを検索
        Dim t15inp = T00015INPtbl.AsEnumerable.
                    Where(Function(x) x.Item("SHIPORG") = work.WF_SEL_SHIPORG.Text AndAlso
                                      x.Item("DELFLG") = C_DELETE_FLG.ALIVE AndAlso
                                      x.Item("SHUKODATE") = row("SHUKODATE") AndAlso
                                      x.Item("GSHABAN") = row("GSHABAN") AndAlso
                                      x.Item("TODOKECODE") <> row("TODOKECODE"))
        Dim t15tbl = T00015tbl.AsEnumerable.
                    Where(Function(x) x.Item("SHIPORG") = work.WF_SEL_SHIPORG.Text AndAlso
                                      x.Item("DELFLG") = C_DELETE_FLG.ALIVE AndAlso
                                      x.Item("SHUKODATE") = row("SHUKODATE") AndAlso
                                      x.Item("GSHABAN") = row("GSHABAN") AndAlso
                                      x.Item("TODOKECODE") <> row("TODOKECODE"))

        Dim t15 = t15tbl.Union(t15inp)
        If t15.Count > 0 Then
            '届先時刻順で最終のTRIPNOを取得
            tripNo = Val(t15.Last.Item("TRIPNO"))
        End If

        Return tripNo

    End Function

#End Region


End Class
