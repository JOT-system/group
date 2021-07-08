Imports OFFICE.GRIS0005LeftBox
Imports BASEDLL
Imports System.Data.SqlClient

''' <summary>
''' 車両付属情報（条件）
''' </summary>
''' <remarks></remarks>
Public Class GRMA0004SELECT
    Inherits Page

    '共通処理結果
    Private WW_ERR_SW As String
    Private WW_RTN_SW As String
    Private WW_DUMMY As String

    '検索結果格納ds
    Private MA0004tbl As DataTable                              '格納用テーブル

    '共通関数宣言(BASEDLL)
    Private CS0011LOGWRITE As New CS0011LOGWrite                'LogOutput DirString Get
    Private CS0050Session As New CS0050SESSION                  'セッション管理

    ''' <summary>
    ''' サーバ処理の遷移先
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs) Handles Me.Load
        If IsPostBack Then
            '○ 各ボタン押下処理
            If Not String.IsNullOrEmpty(WF_ButtonClick.Value) Then
                Select Case WF_ButtonClick.Value
                    Case "WF_ButtonDO"
                        WF_ButtonDO_Click()
                    Case "WF_ButtonEND"
                        WF_ButtonEND_Click()
                    Case "WF_ButtonCREATE"
                        WF_ButtonCREATE_Click()
                    Case "WF_ButtonSel"
                        WF_ButtonSel_Click()
                    Case "WF_ButtonCan"
                        WF_ButtonCan_Click()
                    Case "WF_Field_DBClick"
                        WF_Field_DBClick()
                    Case "WF_ListboxDBclick"
                        WF_LEFTBOX_DBClick()
                    Case "WF_LeftBoxSelectClick"
                        WF_LEFTBOX_SELECT_CLICK()
                    Case "WF_RIGHT_VIEW_DBClick"
                        WF_RIGHTBOX_DBClick()
                    Case "WF_MEMOChange"
                        WF_RIGHTBOX_Change()
                    Case Else
                End Select
            End If
        Else
            '○初期化
            initialize()
        End If

    End Sub
    ''' <summary>
    ''' 初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub initialize()
        '○初期値設定
        Master.MAPID = GRMA0004WRKINC.MAPIDS
        WF_YYF.Focus()
        WF_LeftMViewChange.Value = ""
        leftview.ActiveListBox()
        WF_FIELD.Value = ""

        '○画面の値設定
        WW_MAPValueSet()
    End Sub
    ''' <summary>
    ''' 終了ボタン処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonEND_Click()

        '○ 画面戻先URL取得
        Master.TransitionPrevPage()

    End Sub
    ''' <summary>
    ''' 検索実行処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonDO_Click()

        '○初期設定
        WF_FIELD.Value = ""

        '○ 入力文字置き換え(使用禁止文字排除)
        Master.EraseCharToIgnore(WF_CAMPCODE.Text)          '会社コード
        Master.EraseCharToIgnore(WF_YYF.Text)               '年度(From)
        Master.EraseCharToIgnore(WF_YYT.Text)               '年度(To)
        Master.EraseCharToIgnore(WF_MORG.Text)              '管理部署
        Master.EraseCharToIgnore(WF_SORG.Text)              '設置部署
        Master.EraseCharToIgnore(WF_OILTYPE1.Text)          '油種(1)
        Master.EraseCharToIgnore(WF_OILTYPE2.Text)          '油種(2)
        Master.EraseCharToIgnore(WF_OWNCODEF.Text)          '荷主(From)
        Master.EraseCharToIgnore(WF_OWNCODET.Text)          '荷主(To)
        Master.EraseCharToIgnore(WF_SHARYOTYPE1.Text)       '車両タイプ(1)
        Master.EraseCharToIgnore(WF_SHARYOTYPE2.Text)       '車両タイプ(2)
        Master.EraseCharToIgnore(WF_SHARYOTYPE3.Text)       '車両タイプ(3)
        Master.EraseCharToIgnore(WF_SHARYOTYPE4.Text)       '車両タイプ(4)
        Master.EraseCharToIgnore(WF_SHARYOTYPE5.Text)       '車両タイプ(5)

        '○ チェック処理
        WW_Check(WW_ERR_SW)
        If Not isNormal(WW_ERR_SW) Then
            Exit Sub
        End If

        '○ 条件選択画面の入力値退避
        work.WF_SEL_CAMPCODE.Text = WF_CAMPCODE.Text                '会社コード
        work.WF_SEL_YYF.Text = WF_YYF.Text                          '年度
        If WF_YYT.Text = "" Then
            work.WF_SEL_YYT.Text = WF_YYF.Text
        Else
            work.WF_SEL_YYT.Text = WF_YYT.Text
        End If
        work.WF_SEL_MORG.Text = WF_MORG.Text                        '管理部署
        work.WF_SEL_SORG.Text = WF_SORG.Text                        '設置部署
        work.WF_SEL_OILTYPE1.Text = WF_OILTYPE1.Text                '油種(1)
        work.WF_SEL_OILTYPE2.Text = WF_OILTYPE2.Text                '油種(2)
        work.WF_SEL_OWNCODE1.Text = WF_OWNCODEF.Text                '荷主
        If WF_OWNCODET.Text = "" Then
            work.WF_SEL_OWNCODE2.Text = WF_OWNCODEF.Text
        Else
            work.WF_SEL_OWNCODE2.Text = WF_OWNCODET.Text
        End If
        work.WF_SEL_SHARYOTYPE1.Text = WF_SHARYOTYPE1.Text          '車両タイプ(1)
        work.WF_SEL_SHARYOTYPE2.Text = WF_SHARYOTYPE2.Text          '車両タイプ(2)
        work.WF_SEL_SHARYOTYPE3.Text = WF_SHARYOTYPE3.Text          '車両タイプ(3)
        work.WF_SEL_SHARYOTYPE4.Text = WF_SHARYOTYPE4.Text          '車両タイプ(4)
        work.WF_SEL_SHARYOTYPE5.Text = WF_SHARYOTYPE5.Text          '車両タイプ(5)

        If WF_SW1.Checked Then
            work.WF_SEL_DISPCHG.Text = "NEW"                        '最新
        End If
        If WF_SW2.Checked Then
            work.WF_SEL_DISPCHG.Text = "HIST"                       '履歴
        End If

        work.WF_SEL_NENDO_CREATE.Text = WF_NENDO_CREATE.Text        '年度(作成)
        work.WF_SEL_SORG_CREATE.Text = WF_SORG_CREATE.Text          '設置部署(作成)

        '○右ボックスからViewID取得
        Master.VIEWID = rightview.GetViewId(work.WF_SEL_CAMPCODE.Text)

        '○ 画面遷移実行
        Master.CheckParmissionCode(WF_CAMPCODE.Text)
        If Not Master.MAPpermitcode = C_PERMISSION.INVALID Then
            '○画面遷移先URL取得
            Master.TransitionPage()
        End If

    End Sub

    ''' <summary>
    ''' 翌年度分作成処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonCREATE_Click()

        WF_MESSAGE_CREATE_Text.Text = ""

        '日付チェック
        If Not WF_NENDO_CREATE.Text = Date.Now.ToString("yyyy") Then
            WF_NENDO_CREATE.Focus()
            WF_MESSAGE_CREATE_Text.Text = "今年度分のみ作成可能です。"
            Exit Sub
        End If

        'マスタ存在チェック
        If Not String.IsNullOrEmpty(WF_SORG_CREATE.Text) Then
            If String.IsNullOrEmpty(WF_SORG_CREATE_Text.Text) Then
                WF_SORG_CREATE.Focus()
                WF_MESSAGE_CREATE_Text.Text = "マスタに存在しません。"
                Exit Sub
            End If
        End If

        '最新（有効車両）情報を取得
        DATAget()

        If MA0004tbl.Rows.Count = 0 Then
            WF_MESSAGE_CREATE_Text.Text = "対象データが存在しません。"
            Exit Sub
        End If

        '車両申請マスタ登録処理
        Dim cnt As Integer = DATAinsert()

        If cnt > 0 Then
            WF_MESSAGE_CREATE_Text.Text = "登録完了"
        Else
            WF_MESSAGE_CREATE_Text.Text = "既に登録済です！"
        End If

    End Sub

    ' ******************************************************************************
    ' ***  leftBOX関連操作                                                       ***
    ' ******************************************************************************

    ''' <summary>
    ''' LEFTBOXの選択された値をフィールドに戻す
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonSel_Click()

        Dim WW_SelectTEXT As String = ""
        Dim WW_SelectValue As String = ""

        If leftview.WF_LeftListBox.SelectedIndex >= 0 Then
            WF_SelectedIndex.Value = leftview.WF_LeftListBox.SelectedIndex
            WW_SelectValue = leftview.WF_LeftListBox.Items(WF_SelectedIndex.Value).Value
            WW_SelectTEXT = leftview.WF_LeftListBox.Items(WF_SelectedIndex.Value).Text
        End If

        Select Case WF_FIELD.Value
            Case "WF_CAMPCODE"          '会社コード
                WF_CAMPCODE_Text.Text = WW_SelectTEXT
                WF_CAMPCODE.Text = WW_SelectValue
                WF_CAMPCODE.Focus()

            Case "WF_MORG"              '管理部署
                WF_MORG_Text.Text = WW_SelectTEXT
                WF_MORG.Text = WW_SelectValue
                WF_MORG.Focus()

            Case "WF_SORG"              '設置部署
                WF_SORG_Text.Text = WW_SelectTEXT
                WF_SORG.Text = WW_SelectValue
                WF_SORG.Focus()

            Case "WF_OILTYPE1"          '油種(1)
                WF_OILTYPE1_Text.Text = WW_SelectTEXT
                WF_OILTYPE1.Text = WW_SelectValue
                WF_OILTYPE1.Focus()
            Case "WF_OILTYPE2"          '油種(2)
                WF_OILTYPE2_Text.Text = WW_SelectTEXT
                WF_OILTYPE2.Text = WW_SelectValue
                WF_OILTYPE2.Focus()

            Case "WF_OWNCODEF"          '荷主(From)
                WF_OWNCODEF_Text.Text = WW_SelectTEXT
                WF_OWNCODEF.Text = WW_SelectValue
                WF_OWNCODEF.Focus()
            Case "WF_OWNCODET"          '荷主(To)
                WF_OWNCODET_Text.Text = WW_SelectTEXT
                WF_OWNCODET.Text = WW_SelectValue
                WF_OWNCODET.Focus()

            Case "WF_SHARYOTYPE1"       '車両タイプ(1)
                WF_SHARYOTYPE1_Text.Text = WW_SelectTEXT
                WF_SHARYOTYPE1.Text = WW_SelectValue
                WF_SHARYOTYPE1.Focus()
            Case "WF_SHARYOTYPE2"       '車両タイプ(2)
                WF_SHARYOTYPE2_Text.Text = WW_SelectTEXT
                WF_SHARYOTYPE2.Text = WW_SelectValue
                WF_SHARYOTYPE2.Focus()
            Case "WF_SHARYOTYPE3"       '車両タイプ(3)
                WF_SHARYOTYPE3_Text.Text = WW_SelectTEXT
                WF_SHARYOTYPE3.Text = WW_SelectValue
                WF_SHARYOTYPE3.Focus()
            Case "WF_SHARYOTYPE4"       '車両タイプ(4)
                WF_SHARYOTYPE4_Text.Text = WW_SelectTEXT
                WF_SHARYOTYPE4.Text = WW_SelectValue
                WF_SHARYOTYPE4.Focus()
            Case "WF_SHARYOTYPE5"       '車両タイプ(5)
                WF_SHARYOTYPE5_Text.Text = WW_SelectTEXT
                WF_SHARYOTYPE5.Text = WW_SelectValue
                WF_SHARYOTYPE5.Focus()

            Case "WF_SORG_CREATE"       '設置部署
                WF_SORG_CREATE_Text.Text = WW_SelectTEXT
                WF_SORG_CREATE.Text = WW_SelectValue
                WF_SORG_CREATE.Focus()

        End Select

        '○ 画面左サイドボックス非表示は、画面JavaScript(InitLoad)で実行
        WF_LeftboxOpen.Value = ""
        WF_LeftMViewChange.Value = ""

    End Sub
    ''' <summary>
    ''' leftBOXキャンセルボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonCan_Click()

        '○ フォーカスセット
        Select Case WF_FIELD.Value
            Case "WF_CAMPCODE"          '会社コード
                WF_CAMPCODE.Focus()
            Case "WF_MORG"              '管理部署
                WF_MORG.Focus()
            Case "WF_SORG"              '設置部署
                WF_SORG.Focus()
            Case "WF_OILTYPE1"          '油種(1)
                WF_OILTYPE1.Focus()
            Case "WF_OILTYPE2"          '油種(2)
                WF_OILTYPE2.Focus()
            Case "WF_OWNCODEF"          '荷主(From)
                WF_OWNCODEF.Focus()
            Case "WF_OWNCODET"          '荷主(To)
                WF_OWNCODET.Focus()
            Case "WF_SHARYOTYPE1"       '車両タイプ(1)
                WF_SHARYOTYPE1.Focus()
            Case "WF_SHARYOTYPE2"       '車両タイプ(2)
                WF_SHARYOTYPE2.Focus()
            Case "WF_SHARYOTYPE3"       '車両タイプ(3)
                WF_SHARYOTYPE3.Focus()
            Case "WF_SHARYOTYPE4"       '車両タイプ(4)
                WF_SHARYOTYPE4.Focus()
            Case "WF_SHARYOTYPE5"       '車両タイプ(5)
                WF_SHARYOTYPE5.Focus()
            Case "WF_SORG_CREATE"       '設置部署
                WF_SORG_CREATE.Focus()
        End Select

        '○ 画面左サイドボックス非表示は、画面JavaScript(InitLoad)で実行
        WF_FIELD.Value = ""
        WF_LeftboxOpen.Value = ""
        WF_LeftMViewChange.Value = ""

    End Sub
    ''' <summary>
    ''' 左リストボックスダブルクリック処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_LEFTBOX_DBClick()
        '○ListBoxダブルクリック処理()
        WF_ButtonSel_Click()
        WW_LeftBoxReSet()
    End Sub
    ''' <summary>
    ''' ○TextBox変更時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_LEFTBOX_SELECT_CLICK()
        WW_LeftBoxReSet()
    End Sub
    ''' <summary>
    ''' フィールドダブルクリック時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_Field_DBClick()
        '○フィールドダブルクリック時処理
        If Not String.IsNullOrEmpty(WF_LeftMViewChange.Value) Then
            Try
                Integer.TryParse(WF_LeftMViewChange.Value, WF_LeftMViewChange.Value)
            Catch ex As Exception
                Exit Sub
            End Try
            With leftview
                If WF_LeftMViewChange.Value <> LIST_BOX_CLASSIFICATION.LC_CALENDAR Then
                    Dim prmData As Hashtable = work.CreateFIXParam(WF_CAMPCODE.Text)

                    Select Case WF_LeftMViewChange.Value
                        Case LIST_BOX_CLASSIFICATION.LC_ORG
                            If WF_FIELD.Value = "WF_MORG" Then
                                prmData = work.CreateORGParam(WF_CAMPCODE.Text, True)
                            Else
                                prmData = work.CreateORGParam(WF_CAMPCODE.Text, False)
                            End If
                        Case LIST_BOX_CLASSIFICATION.LC_CUSTOMER
                            prmData = work.CreateTODOParam(WF_CAMPCODE.Text)
                        Case 999
                            prmData = work.CreateFIXParam(WF_CAMPCODE.Text, "SHARYOTYPE")
                    End Select

                    .SetListBox(WF_LeftMViewChange.Value, WW_DUMMY, prmData)
                    .ActiveListBox()
                Else
                    '日付の場合、入力日付のカレンダーが表示されるように入力値をカレンダーに渡す
                    Select Case WF_FIELD.Value
                        Case Else
                    End Select
                    .ActiveCalendar()
                End If
            End With
        End If

    End Sub
    ''' <summary>
    ''' TextBox変更時LeftBox設定処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_LeftBoxReSet()

        WF_CAMPCODE_Text.Text = ""          '会社
        WF_MORG_Text.Text = ""              '管理部署
        WF_SORG_Text.Text = ""              '設置部署
        WF_OILTYPE1_Text.Text = ""          '油種(1)
        WF_OILTYPE2_Text.Text = ""          '油種(2)
        WF_OWNCODEF_Text.Text = ""          '荷主(From)
        WF_OWNCODET_Text.Text = ""          '荷主(To)
        WF_SHARYOTYPE1_Text.Text = ""       '車両タイプ(1)
        WF_SHARYOTYPE2_Text.Text = ""       '車両タイプ(2)
        WF_SHARYOTYPE3_Text.Text = ""       '車両タイプ(3)
        WF_SHARYOTYPE4_Text.Text = ""       '車両タイプ(4)
        WF_SHARYOTYPE5_Text.Text = ""       '車両タイプ(5)
        WF_SORG_CREATE_Text.Text = ""       '設置部署(作成)

        '○ 入力文字置き換え(使用禁止文字排除)
        Master.EraseCharToIgnore(WF_CAMPCODE.Text)          '会社コード
        Master.EraseCharToIgnore(WF_YYF.Text)               '年度(From)
        Master.EraseCharToIgnore(WF_YYT.Text)               '年度(To)
        Master.EraseCharToIgnore(WF_MORG.Text)              '管理部署
        Master.EraseCharToIgnore(WF_SORG.Text)              '設置部署
        Master.EraseCharToIgnore(WF_OILTYPE1.Text)          '油種(1)
        Master.EraseCharToIgnore(WF_OILTYPE2.Text)          '油種(2)
        Master.EraseCharToIgnore(WF_OWNCODEF.Text)          '荷主(From)
        Master.EraseCharToIgnore(WF_OWNCODET.Text)          '荷主(To)
        Master.EraseCharToIgnore(WF_SHARYOTYPE1.Text)       '車両タイプ(1)
        Master.EraseCharToIgnore(WF_SHARYOTYPE2.Text)       '車両タイプ(2)
        Master.EraseCharToIgnore(WF_SHARYOTYPE3.Text)       '車両タイプ(3)
        Master.EraseCharToIgnore(WF_SHARYOTYPE4.Text)       '車両タイプ(4)
        Master.EraseCharToIgnore(WF_SHARYOTYPE5.Text)       '車両タイプ(5)
        Master.EraseCharToIgnore(WF_SORG_CREATE.Text)       '設置部署(作成)

        '○ チェック処理
        WW_Check(WW_ERR_SW)

        '○ 名称設定
        CODENAME_get("CAMPCODE", WF_CAMPCODE.Text, WF_CAMPCODE_Text.Text, WW_DUMMY)                 '会社コード
        CODENAME_get("MORG", WF_MORG.Text, WF_MORG_Text.Text, WW_DUMMY)                             '管理部署
        CODENAME_get("SORG", WF_SORG.Text, WF_SORG_Text.Text, WW_DUMMY)                             '設置部署
        CODENAME_get("OILTYPE", WF_OILTYPE1.Text, WF_OILTYPE1_Text.Text, WW_DUMMY)                  '油種(1)
        CODENAME_get("OILTYPE", WF_OILTYPE2.Text, WF_OILTYPE2_Text.Text, WW_DUMMY)                  '油種(2)
        CODENAME_get("OWNCONT", WF_OWNCODEF.Text, WF_OWNCODEF_Text.Text, WW_DUMMY)                  '荷主(From)
        CODENAME_get("OWNCONT", WF_OWNCODET.Text, WF_OWNCODET_Text.Text, WW_DUMMY)                  '荷主(To)
        CODENAME_get("SHARYOTYPE", WF_SHARYOTYPE1.Text, WF_SHARYOTYPE1_Text.Text, WW_DUMMY)         '車両タイプ(1)
        CODENAME_get("SHARYOTYPE", WF_SHARYOTYPE2.Text, WF_SHARYOTYPE2_Text.Text, WW_DUMMY)         '車両タイプ(2)
        CODENAME_get("SHARYOTYPE", WF_SHARYOTYPE3.Text, WF_SHARYOTYPE3_Text.Text, WW_DUMMY)         '車両タイプ(3)
        CODENAME_get("SHARYOTYPE", WF_SHARYOTYPE4.Text, WF_SHARYOTYPE4_Text.Text, WW_DUMMY)         '車両タイプ(4)
        CODENAME_get("SHARYOTYPE", WF_SHARYOTYPE5.Text, WF_SHARYOTYPE5_Text.Text, WW_DUMMY)         '車両タイプ(5)
        CODENAME_get("SORG", WF_SORG_CREATE.Text, WF_SORG_CREATE_Text.Text, WW_DUMMY)               '設置部署(作成)

    End Sub

    ' ******************************************************************************
    ' ***  rightBOX関連操作                                                      ***
    ' ******************************************************************************

    ''' <summary>
    ''' 右リストボックスダブルクリック処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_RIGHTBOX_DBClick()
        rightview.InitViewID(WF_CAMPCODE.Text, WW_DUMMY)
    End Sub
    ''' <summary>
    ''' 右リストボックスMEMO欄更新
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_RIGHTBOX_Change()
        '○右Boxメモ変更時処理
        rightview.Save(Master.USERID, Master.USERTERMID, WW_DUMMY)
    End Sub

    ' ******************************************************************************
    ' ***  共通処理                                                              ***
    ' ******************************************************************************

    ' ***  初期値設定処理
    Protected Sub WW_MAPValueSet()

        If Context.Handler.ToString().ToUpper = C_PREV_MAP_LIST.MENU Then               'メニューからの画面遷移
            '○ワーク初期化
            work.Initialize()

            '○初期変数設定処理
            Master.GetFirstValue(work.WF_SEL_CAMPCODE.Text, "YYF", WF_YYF.Text)                         '年度(From)
            Master.GetFirstValue(work.WF_SEL_CAMPCODE.Text, "YYT", WF_YYT.Text)                         '年度(To)
            '○初期変数設定処理
            '年度(From)
            If Len(WF_YYF.Text) <> 4 AndAlso Len(WF_YYF.Text) <> 0 Then
                '変数がyyyy形式設定以外の場合
                Dim WW_date As Date
                Try
                    Date.TryParse(WF_YYF.Text, WW_date)
                Catch ex As Exception
                    WW_date = C_DEFAULT_YMD
                End Try

                If WW_date.ToString("MM") = "01" OrElse WW_date.ToString("MM") = "02" OrElse WW_date.ToString("MM") = "03" Then
                    WF_YYF.Text = (WW_date.Year - 1).ToString()
                Else
                    WF_YYF.Text = (WW_date.Year).ToString()
                End If
            End If
            '年度(To)
            If Len(WF_YYT.Text) <> 4 AndAlso Len(WF_YYT.Text) <> 0 Then
                '変数がyyyy形式設定以外の場合
                Dim WW_date As Date
                Try
                    Date.TryParse(WF_YYT.Text, WW_date)
                Catch ex As Exception
                    WW_date = C_DEFAULT_YMD
                End Try

                If WW_date.ToString("MM") = "01" OrElse WW_date.ToString("MM") = "02" OrElse WW_date.ToString("MM") = "03" Then
                    WF_YYT.Text = (WW_date.Year - 1).ToString()
                Else
                    WF_YYT.Text = (WW_date.Year).ToString()
                End If
            End If

            '年度(To)の値を年度(作成)に設定
            WF_NENDO_CREATE.Text = WF_YYT.Text

            Master.GetFirstValue(work.WF_SEL_CAMPCODE.Text, "CAMPCODE", WF_CAMPCODE.Text)               '会社コード
            Master.GetFirstValue(work.WF_SEL_CAMPCODE.Text, "MORG", WF_MORG.Text)                       '管理部署
            Master.GetFirstValue(work.WF_SEL_CAMPCODE.Text, "SORG", WF_SORG.Text)                       '設置部署
            Master.GetFirstValue(work.WF_SEL_CAMPCODE.Text, "OILTYPE1", WF_OILTYPE1.Text)               '油種(1)
            Master.GetFirstValue(work.WF_SEL_CAMPCODE.Text, "OILTYPE2", WF_OILTYPE2.Text)               '油種(2)
            Master.GetFirstValue(work.WF_SEL_CAMPCODE.Text, "OWNCODEF", WF_OWNCODEF.Text)               '荷主(From)
            Master.GetFirstValue(work.WF_SEL_CAMPCODE.Text, "OWNCODET", WF_OWNCODET.Text)               '荷主(To)
            Master.GetFirstValue(work.WF_SEL_CAMPCODE.Text, "SHARYOTYPE1", WF_SHARYOTYPE1.Text)         '車両タイプ(1)
            Master.GetFirstValue(work.WF_SEL_CAMPCODE.Text, "SHARYOTYPE2", WF_SHARYOTYPE2.Text)         '車両タイプ(2)
            Master.GetFirstValue(work.WF_SEL_CAMPCODE.Text, "SHARYOTYPE3", WF_SHARYOTYPE3.Text)         '車両タイプ(3)
            Master.GetFirstValue(work.WF_SEL_CAMPCODE.Text, "SHARYOTYPE4", WF_SHARYOTYPE4.Text)         '車両タイプ(4)
            Master.GetFirstValue(work.WF_SEL_CAMPCODE.Text, "SHARYOTYPE5", WF_SHARYOTYPE5.Text)         '車両タイプ(5)
            Master.GetFirstValue(work.WF_SEL_CAMPCODE.Text, "SORG", WF_SORG_CREATE.Text)                '設置部署(作成)

            WF_SW1.Checked = True
            WF_SW2.Checked = False

            '○RightBox情報設定
            rightview.MAPID = GRMA0004WRKINC.MAPID
            rightview.MAPIDS = GRMA0004WRKINC.MAPIDS
            rightview.COMPCODE = WF_CAMPCODE.Text
            rightview.MAPVARI = Master.MAPvariant
            rightview.PROFID = Master.PROF_VIEW
            rightview.Initialize("画面レイアウト設定", WW_ERR_SW)
            If Not isNormal(WW_ERR_SW) Then
                Exit Sub
            End If
        ElseIf Context.Handler.ToString().ToUpper = C_PREV_MAP_LIST.MA0004 Then         '実行画面からの画面遷移

            '○画面項目設定処理
            WF_CAMPCODE.Text = work.WF_SEL_CAMPCODE.Text                '会社コード
            WF_YYF.Text = work.WF_SEL_YYF.Text                          '年度(From)
            WF_YYT.Text = work.WF_SEL_YYT.Text                          '年度(To)
            WF_MORG.Text = work.WF_SEL_MORG.Text                        '管理部署
            WF_SORG.Text = work.WF_SEL_SORG.Text                        '設置部署
            WF_OILTYPE1.Text = work.WF_SEL_OILTYPE1.Text                '油種(1)
            WF_OILTYPE2.Text = work.WF_SEL_OILTYPE2.Text                '油種(2)
            WF_OWNCODEF.Text = work.WF_SEL_OWNCODE1.Text                '荷主(From)
            WF_OWNCODET.Text = work.WF_SEL_OWNCODE2.Text                '荷主(To)
            WF_SHARYOTYPE1.Text = work.WF_SEL_SHARYOTYPE1.Text          '車両タイプ(1)
            WF_SHARYOTYPE2.Text = work.WF_SEL_SHARYOTYPE2.Text          '車両タイプ(2)
            WF_SHARYOTYPE3.Text = work.WF_SEL_SHARYOTYPE3.Text          '車両タイプ(3)
            WF_SHARYOTYPE4.Text = work.WF_SEL_SHARYOTYPE4.Text          '車両タイプ(4)
            WF_SHARYOTYPE5.Text = work.WF_SEL_SHARYOTYPE5.Text          '車両タイプ(5)
            WF_NENDO_CREATE.Text = work.WF_SEL_NENDO_CREATE.Text        '年度(作成)
            WF_SORG_CREATE.Text = work.WF_SEL_SORG_CREATE.Text          '設置部署(作成)

            If work.WF_SEL_DISPCHG.Text = "NEW" Then
                WF_SW1.Checked = True
                WF_SW2.Checked = False
            Else
                WF_SW1.Checked = False
                WF_SW2.Checked = True
            End If

            '○RightBox情報設定
            rightview.MAPID = GRMA0004WRKINC.MAPID
            rightview.MAPIDS = GRMA0004WRKINC.MAPIDS
            rightview.COMPCODE = WF_CAMPCODE.Text
            rightview.MAPVARI = Master.MAPvariant
            rightview.PROFID = Master.PROF_VIEW
            rightview.Initialize("画面レイアウト設定", WW_ERR_SW)
            If Not isNormal(WW_ERR_SW) Then
                Exit Sub
            End If
        End If

        '○ 名称設定
        CODENAME_get("CAMPCODE", WF_CAMPCODE.Text, WF_CAMPCODE_Text.Text, WW_DUMMY)                 '会社コード
        CODENAME_get("MORG", WF_MORG.Text, WF_MORG_Text.Text, WW_DUMMY)                             '管理部署
        CODENAME_get("SORG", WF_SORG.Text, WF_SORG_Text.Text, WW_DUMMY)                             '設置部署
        CODENAME_get("OILTYPE", WF_OILTYPE1.Text, WF_OILTYPE1_Text.Text, WW_DUMMY)                  '油種(1)
        CODENAME_get("OILTYPE", WF_OILTYPE2.Text, WF_OILTYPE2_Text.Text, WW_DUMMY)                  '油種(2)
        CODENAME_get("OWNCONT", WF_OWNCODEF.Text, WF_OWNCODEF_Text.Text, WW_DUMMY)                  '荷主(From)
        CODENAME_get("OWNCONT", WF_OWNCODET.Text, WF_OWNCODET_Text.Text, WW_DUMMY)                  '荷主(To)
        CODENAME_get("SHARYOTYPE", WF_SHARYOTYPE1.Text, WF_SHARYOTYPE1_Text.Text, WW_DUMMY)         '車両タイプ(1)
        CODENAME_get("SHARYOTYPE", WF_SHARYOTYPE2.Text, WF_SHARYOTYPE2_Text.Text, WW_DUMMY)         '車両タイプ(2)
        CODENAME_get("SHARYOTYPE", WF_SHARYOTYPE3.Text, WF_SHARYOTYPE3_Text.Text, WW_DUMMY)         '車両タイプ(3)
        CODENAME_get("SHARYOTYPE", WF_SHARYOTYPE4.Text, WF_SHARYOTYPE4_Text.Text, WW_DUMMY)         '車両タイプ(4)
        CODENAME_get("SHARYOTYPE", WF_SHARYOTYPE5.Text, WF_SHARYOTYPE5_Text.Text, WW_DUMMY)         '車両タイプ(5)
        CODENAME_get("SORG", WF_SORG_CREATE.Text, WF_SORG_CREATE_Text.Text, WW_DUMMY)               '設置部署(作成)

    End Sub
    ''' <summary>
    ''' チェック処理
    ''' </summary>
    ''' <param name="O_RTN">可否判定</param>
    ''' <remarks></remarks>
    Protected Sub WW_Check(ByRef O_RTN As String)


        O_RTN = C_MESSAGE_NO.NORMAL

        '○ 入力項目チェック
        Dim WW_CS0024FCHECKERR As String = ""
        Dim WW_CS0024FCHECKREPORT As String = ""
        WF_FIELD.Value = ""

        '会社コード WF_CAMPCODE 
        Master.CheckField(WF_CAMPCODE.Text, "CAMPCODE", WF_CAMPCODE.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            '存在チェック(LeftBoxチェック)
            If WF_CAMPCODE.Text <> "" Then
                CODENAME_get("CAMPCODE", WF_CAMPCODE.Text, WF_CAMPCODE_Text.Text, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    Master.Output(C_MESSAGE_NO.INVALID_SELECTION_DATA, C_MESSAGE_TYPE.ERR)
                    WF_CAMPCODE.Focus()
                    O_RTN = C_MESSAGE_NO.INVALID_SELECTION_DATA
                    Exit Sub
                End If
            End If
        Else
            Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR)
            WF_CAMPCODE.Focus()
            O_RTN = WW_CS0024FCHECKERR
            Exit Sub
        End If

        '年度 WF_YYF.Text
        Master.CheckField(WF_CAMPCODE.Text, "YYF", WF_YYF.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            If WF_YYF.Text <> "" AndAlso (WF_YYF.Text <= "2000" OrElse WF_YYF.Text >= "2099") Then
                Master.Output(C_MESSAGE_NO.NUMBER_RANGE_ERROR, C_MESSAGE_TYPE.ERR)
                WF_YYF.Focus()
                O_RTN = C_MESSAGE_NO.NUMBER_RANGE_ERROR
                Exit Sub
            End If
        Else
            Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR)
            WF_YYF.Focus()
            O_RTN = WW_CS0024FCHECKERR
            Exit Sub
        End If

        '年度 WF_YYT.Text
        Master.CheckField(WF_CAMPCODE.Text, "YYT", WF_YYT.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            If WF_YYT.Text <> "" AndAlso (WF_YYT.Text <= "2000" OrElse WF_YYT.Text >= "2099") Then
                '範囲エラー
                Master.Output(C_MESSAGE_NO.NUMBER_RANGE_ERROR, C_MESSAGE_TYPE.ERR)
                WF_YYT.Focus()
                O_RTN = C_MESSAGE_NO.NUMBER_RANGE_ERROR
                Exit Sub
            End If
        Else
            Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR)
            WF_YYT.Focus()
            O_RTN = WW_CS0024FCHECKERR
            Exit Sub
        End If

        '関連チェック(開始＞終了)
        If WF_YYF.Text <> "" AndAlso WF_YYT.Text <> "" Then
            Dim WW_YYF As Integer = 0
            Dim WW_YYT As Integer = 0
            Try
                Integer.TryParse(WF_YYF.Text, WW_YYF)
                Integer.TryParse(WF_YYT.Text, WW_YYT)
                If WW_YYF > WW_YYT Then
                    Master.Output(C_MESSAGE_NO.START_END_RELATION_ERROR, C_MESSAGE_TYPE.ERR)
                    WF_YYF.Focus()
                    O_RTN = C_MESSAGE_NO.START_END_RELATION_ERROR
                    Exit Sub
                End If
            Catch ex As Exception
            End Try
        End If

        '管理部署 WF_MORG 
        Master.CheckField(WF_CAMPCODE.Text, "MORG", WF_MORG.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            '存在チェック(LeftBoxチェック)
            If WF_MORG.Text <> "" Then
                CODENAME_get("MORG", WF_MORG.Text, WF_MORG_Text.Text, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    Master.Output(C_MESSAGE_NO.INVALID_SELECTION_DATA, C_MESSAGE_TYPE.ERR)
                    WF_MORG.Focus()
                    O_RTN = C_MESSAGE_NO.INVALID_SELECTION_DATA
                    Exit Sub
                End If
            End If
        Else
            Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR)
            WF_MORG.Focus()
            O_RTN = WW_CS0024FCHECKERR
            Exit Sub
        End If

        '設置部署 WF_SORG
        Master.CheckField(WF_CAMPCODE.Text, "SORG", WF_SORG.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            '存在チェック(LeftBoxチェック)
            If WF_SORG.Text <> "" Then
                CODENAME_get("SORG", WF_SORG.Text, WF_SORG_Text.Text, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    Master.Output(C_MESSAGE_NO.INVALID_SELECTION_DATA, C_MESSAGE_TYPE.ERR)
                    WF_SORG.Focus()
                    O_RTN = C_MESSAGE_NO.INVALID_SELECTION_DATA
                    Exit Sub
                End If
            End If
        Else
            Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR)
            WF_SORG.Focus()
            O_RTN = WW_CS0024FCHECKERR
            Exit Sub
        End If

        '油種 WF_OILTYPE1
        Master.CheckField(WF_CAMPCODE.Text, "OILTYPE1", WF_OILTYPE1.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            '存在チェック(LeftBoxチェック)
            If WF_OILTYPE1.Text <> "" Then
                CODENAME_get("OILTYPE", WF_OILTYPE1.Text, WF_OILTYPE1_Text.Text, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    Master.Output(C_MESSAGE_NO.INVALID_SELECTION_DATA, C_MESSAGE_TYPE.ERR)
                    WF_OILTYPE1.Focus()
                    O_RTN = C_MESSAGE_NO.INVALID_SELECTION_DATA
                    Exit Sub
                End If
            End If
        Else
            Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR)
            WF_OILTYPE1.Focus()
            O_RTN = WW_CS0024FCHECKERR
            Exit Sub
        End If

        '油種 WF_OILTYPE2
        Master.CheckField(WF_CAMPCODE.Text, "OILTYPE2", WF_OILTYPE2.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            '存在チェック(LeftBoxチェック)
            If WF_OILTYPE2.Text <> "" Then
                CODENAME_get("OILTYPE", WF_OILTYPE2.Text, WF_OILTYPE2_Text.Text, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    Master.Output(C_MESSAGE_NO.INVALID_SELECTION_DATA, C_MESSAGE_TYPE.ERR)
                    WF_OILTYPE2.Focus()
                    O_RTN = C_MESSAGE_NO.INVALID_SELECTION_DATA
                    Exit Sub
                End If
            End If
        Else
            Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR)
            WF_OILTYPE2.Focus()
            O_RTN = WW_CS0024FCHECKERR
            Exit Sub
        End If

        '荷主 WF_OWNCODEF
        Master.CheckField(WF_CAMPCODE.Text, "OWNCONT1", WF_OWNCODEF.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            '存在チェック(LeftBoxチェック)
            If WF_OWNCODEF.Text <> "" Then
                CODENAME_get("OWNCONT", WF_OWNCODEF.Text, WF_OWNCODEF_Text.Text, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    Master.Output(C_MESSAGE_NO.INVALID_SELECTION_DATA, C_MESSAGE_TYPE.ERR)
                    WF_OWNCODEF.Focus()
                    O_RTN = C_MESSAGE_NO.INVALID_SELECTION_DATA
                    Exit Sub
                End If
            End If
        Else
            Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR)
            WF_OWNCODEF.Focus()
            O_RTN = WW_CS0024FCHECKERR
            Exit Sub
        End If

        '荷主 WF_OWNCODET
        Master.CheckField(WF_CAMPCODE.Text, "OWNCONT2", WF_OWNCODET.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            '存在チェック(LeftBoxチェック)
            If WF_OWNCODET.Text <> "" Then
                CODENAME_get("OWNCONT", WF_OWNCODET.Text, WF_OWNCODET_Text.Text, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    Master.Output(C_MESSAGE_NO.INVALID_SELECTION_DATA, C_MESSAGE_TYPE.ERR)
                    WF_OWNCODET.Focus()
                    O_RTN = C_MESSAGE_NO.INVALID_SELECTION_DATA
                    Exit Sub
                End If
            End If
        Else
            Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR)
            WF_OWNCODET.Focus()
            O_RTN = WW_CS0024FCHECKERR
            Exit Sub
        End If

        '関連チェック(開始＞終了)
        If WF_OWNCODET.Text <> "" AndAlso WF_OWNCODEF.Text <> "" AndAlso
           WF_OWNCODET.Text < WF_OWNCODEF.Text Then
            Master.Output(C_MESSAGE_NO.START_END_RELATION_ERROR, C_MESSAGE_TYPE.ERR)
            WF_OWNCODEF.Focus()
            O_RTN = C_MESSAGE_NO.START_END_RELATION_ERROR
            Exit Sub
        End If

        '車両タイプ WF_SHARYOTYPE1
        Master.CheckField(WF_CAMPCODE.Text, "SHARYOTYPE1", WF_SHARYOTYPE1.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            '存在チェック(LeftBoxチェック)
            If WF_SHARYOTYPE1.Text <> "" Then
                CODENAME_get("SHARYOTYPE", WF_SHARYOTYPE1.Text, WF_SHARYOTYPE1_Text.Text, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    Master.Output(C_MESSAGE_NO.INVALID_SELECTION_DATA, C_MESSAGE_TYPE.ERR)
                    WF_SHARYOTYPE1.Focus()
                    O_RTN = C_MESSAGE_NO.INVALID_SELECTION_DATA
                    Exit Sub
                End If
            End If
        Else
            Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR)
            WF_SHARYOTYPE1.Focus()
            O_RTN = WW_CS0024FCHECKERR
            Exit Sub
        End If


        '車両タイプ WF_SHARYOTYPE2
        Master.CheckField(WF_CAMPCODE.Text, "SHARYOTYPE2", WF_SHARYOTYPE2.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            '存在チェック(LeftBoxチェック)
            If WF_SHARYOTYPE2.Text <> "" Then
                CODENAME_get("SHARYOTYPE", WF_SHARYOTYPE2.Text, WF_SHARYOTYPE2_Text.Text, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    Master.Output(C_MESSAGE_NO.INVALID_SELECTION_DATA, C_MESSAGE_TYPE.ERR)
                    WF_SHARYOTYPE2.Focus()
                    O_RTN = C_MESSAGE_NO.INVALID_SELECTION_DATA
                    Exit Sub
                End If
            End If
        Else
            Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR)
            WF_SHARYOTYPE2.Focus()
            O_RTN = WW_CS0024FCHECKERR
            Exit Sub
        End If

        '車両タイプ WF_SHARYOTYPE3
        Master.CheckField(WF_CAMPCODE.Text, "SHARYOTYPE3", WF_SHARYOTYPE3.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            '存在チェック(LeftBoxチェック)
            If WF_SHARYOTYPE3.Text <> "" Then
                CODENAME_get("SHARYOTYPE", WF_SHARYOTYPE3.Text, WF_SHARYOTYPE3_Text.Text, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    Master.Output(C_MESSAGE_NO.INVALID_SELECTION_DATA, C_MESSAGE_TYPE.ERR)
                    WF_SHARYOTYPE3.Focus()
                    O_RTN = C_MESSAGE_NO.INVALID_SELECTION_DATA
                    Exit Sub
                End If
            End If
        Else
            Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR)
            WF_SHARYOTYPE3.Focus()
            O_RTN = WW_CS0024FCHECKERR
            Exit Sub
        End If

        '車両タイプ WF_SHARYOTYPE4
        Master.CheckField(WF_CAMPCODE.Text, "SHARYOTYPE4", WF_SHARYOTYPE4.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            '存在チェック(LeftBoxチェック)
            If WF_SHARYOTYPE4.Text <> "" Then
                CODENAME_get("SHARYOTYPE", WF_SHARYOTYPE4.Text, WF_SHARYOTYPE4_Text.Text, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    Master.Output(C_MESSAGE_NO.INVALID_SELECTION_DATA, C_MESSAGE_TYPE.ERR)
                    WF_SHARYOTYPE4.Focus()
                    O_RTN = C_MESSAGE_NO.INVALID_SELECTION_DATA
                    Exit Sub
                End If
            End If
        Else
            Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR)
            WF_SHARYOTYPE4.Focus()
            O_RTN = WW_CS0024FCHECKERR
            Exit Sub
        End If

        '車両タイプ WF_SHARYOTYPE5
        Master.CheckField(WF_CAMPCODE.Text, "SHARYOTYPE5", WF_SHARYOTYPE5.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            '存在チェック(LeftBoxチェック)
            If WF_SHARYOTYPE5.Text <> "" Then
                CODENAME_get("SHARYOTYPE", WF_SHARYOTYPE5.Text, WF_SHARYOTYPE5_Text.Text, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    Master.Output(C_MESSAGE_NO.INVALID_SELECTION_DATA, C_MESSAGE_TYPE.ERR)
                    WF_SHARYOTYPE5.Focus()
                    O_RTN = C_MESSAGE_NO.INVALID_SELECTION_DATA
                    Exit Sub
                End If
            End If
        Else
            Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR)
            WF_SHARYOTYPE5.Focus()
            O_RTN = WW_CS0024FCHECKERR
            Exit Sub
        End If
        '正常メッセージ
        Master.Output(C_MESSAGE_NO.NORMAL, C_MESSAGE_TYPE.NOR)

    End Sub


    ' ******************************************************************************
    ' ***  サブルーチン                                                          ***
    ' ******************************************************************************


    ''' <summary>
    ''' 左リストボックスより名称取得とチェックを行う
    ''' </summary>
    ''' <param name="I_FIELD"></param>
    ''' <param name="I_VALUE"></param>
    ''' <param name="O_TEXT"></param>
    ''' <param name="O_RTN"></param>
    ''' <remarks></remarks>
    Protected Sub CODENAME_get(ByVal I_FIELD As String, ByRef I_VALUE As String, ByRef O_TEXT As String, ByRef O_RTN As String)

        '○名称取得
        O_TEXT = ""
        O_RTN = C_MESSAGE_NO.NORMAL

        If I_VALUE <> "" Then
            Select Case I_FIELD
                Case "CAMPCODE"         '会社コード
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_COMPANY, I_VALUE, O_TEXT, O_RTN)
                Case "MORG"             '管理部署
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_ORG, I_VALUE, O_TEXT, O_RTN, work.CreateORGParam(WF_CAMPCODE.Text, True))
                Case "SORG"             '設置部署
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_ORG, I_VALUE, O_TEXT, O_RTN, work.CreateORGParam(WF_CAMPCODE.Text, False))
                Case "OILTYPE"          '油種
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_OILTYPE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(WF_CAMPCODE.Text))
                Case "OWNCONT"          '荷主
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_CUSTOMER, I_VALUE, O_TEXT, O_RTN, work.CreateTODOParam(WF_CAMPCODE.Text))
                Case "SHARYOTYPE"       '車両タイプ
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(WF_CAMPCODE.Text, "SHARYOTYPE"))
            End Select
        End If

    End Sub

    ''' <summary>
    ''' 最新（有効車両）情報取得
    ''' </summary>
    ''' <remarks>データベース（MA003_SHARYOB）を検索し最新（有効車両）情報を取得する</remarks>
    Protected Sub DATAget()

        Dim WW_SHARYOTYPE As String = ""
        Dim WW_TSHABAN As String = ""
        Dim WW_STYMD_C As String = ""

        '○選択日付編集
        Dim WW_str As String
        Dim WW_STYMD_Yuko As Date      '有効期限(開始)
        Dim WW_ENDYMD_Yuko As Date     '有効期限(終了)
        Dim WW_STYMD_Nendo As Date     '
        Dim WW_ENDYMD_Nendo As Date    '
        Dim WW_int As Integer

        '○有効期限(開始)
        WW_str = WF_NENDO_CREATE.Text & "/4/1"
        Try
            Date.TryParse(WW_str, WW_STYMD_Yuko)
        Catch ex As Exception
            WW_STYMD_Yuko = "2000/4/1"
        End Try

        '○有効期限(終了)
        Try
            Integer.TryParse(WF_NENDO_CREATE.Text, WW_int)
            WW_str = (WW_int + 1).ToString() & "/3/31"

            Date.TryParse(WW_str, WW_ENDYMD_Yuko)
        Catch ex As Exception
            WW_ENDYMD_Yuko = "2099/3/31"
        End Try

        '○対象年度(開始)
        WW_str = WF_NENDO_CREATE.Text & "/4/1"
        Try
            Date.TryParse(WW_str, WW_STYMD_Nendo)
        Catch ex As Exception
            WW_STYMD_Nendo = "2000/4/1"
        End Try

        '○対象年度((終了))
        Try
            Integer.TryParse(WF_NENDO_CREATE.Text, WW_int)
            WW_str = (WW_int + 1).ToString() & "/3/31"

            Date.TryParse(WW_str, WW_ENDYMD_Nendo)
        Catch ex As Exception
            WW_ENDYMD_Nendo = "2099/3/31"
        End Try

        '○画面表示用データ取得

        'ユーザプロファイル（変数）内容検索(自ユーザ権限＆抽出条件なしで検索)
        Try
            '■テーブル検索結果をテーブル退避
            'MA0004テンポラリDB項目作成
            If MA0004tbl Is Nothing Then
                MA0004tbl = New DataTable
            End If

            If MA0004tbl.Columns.Count <> 0 Then
                MA0004tbl.Columns.Clear()
            End If

            'DataBase接続文字
            Using SQLcon As SqlConnection = CS0050SESSION.getConnection
                SQLcon.Open()       'DataBase接続(Open)

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
                      " SELECT                                                             " _
                    & "         0                                      as LINECNT      ,   " _
                    & "         ''                                     as OPERATION    ,   " _
                    & "         TIMSTP = cast(isnull(A.UPDTIMSTP,0) as bigint)         ,   " _
                    & "         0                                      as 'SELECT'     ,   " _
                    & "         0                                      as HIDDEN       ,   " _
                    & "         0                                      as WORK_NO      ,   " _
                    & "         isnull(rtrim(A.MANGMORG),'')           as MANGMORG     ,   " _
                    & "         isnull(rtrim(A.MANGSORG),'')           as MANGSORG     ,   " _
                    & "         isnull(rtrim(A.MANGOILTYPE),'')        as MANGOILTYPE  ,   " _
                    & "         isnull(rtrim(A.MANGOWNCODE),'')        as MANGOWNCODE  ,   " _
                    & "         isnull(rtrim(A.MANGPROD1),'')          as MANGPROD1    ,   " _
                    & "         isnull(rtrim(A.MANGPROD2),'')          as MANGPROD2    ,   " _
                    & "         isnull(rtrim(A.MANGOWNCONT),'')        as MANGOWNCONT  ,   " _
                    & "         cast(isnull(A.MANGSHAFUKU,'0') as VarChar) as MANGSHAFUKU  ,   " _
                    & "         isnull(rtrim(A.MANGSUPPL),'')          as MANGSUPPL    ,   " _
                    & "         cast(isnull(A.MANGTTLDIST,'0') as VarChar) as MANGTTLDIST  ,   " _
                    & "         cast(isnull(B.BASERAGE,'') as VarChar) as BASERAGE     ,   " _
                    & "         cast(isnull(B.BASERAGEMM,'') as VarChar) as BASERAGEMM   ,   " _
                    & "         isnull(rtrim(A.BASELEASE),'')          as BASELEASE    ,   " _
                    & "         cast(isnull(B.BASERAGEYY,'') as VarChar) as BASERAGEYY   ,   " _
                    & "         rtrim(B.BASERDATE)                     as BASERDATE    ,   " _
                    & "         isnull(rtrim(B.FCTRDPR),'')            as FCTRDPR      ,   " _
                    & "         isnull(rtrim(B.FCTRAXLE),'')           as FCTRAXLE     ,   " _
                    & "         cast(isnull(B.FCTRFUELCAPA,'') as VarChar) as FCTRFUELCAPA ,   " _
                    & "         isnull(rtrim(B.FCTRFUELMATE),'')       as FCTRFUELMATE ,   " _
                    & "         isnull(rtrim(B.FCTRRESERVE1),'')       as FCTRRESERVE1 ,   " _
                    & "         isnull(rtrim(B.FCTRRESERVE2),'')       as FCTRRESERVE2 ,   " _
                    & "         isnull(rtrim(B.FCTRRESERVE3),'')       as FCTRRESERVE3 ,   " _
                    & "         isnull(rtrim(B.FCTRRESERVE4),'')       as FCTRRESERVE4 ,   " _
                    & "         isnull(rtrim(B.FCTRRESERVE5),'')       as FCTRRESERVE5 ,   " _
                    & "         isnull(rtrim(B.FCTRSHFTNUM),'')        as FCTRSHFTNUM  ,   " _
                    & "         isnull(rtrim(B.FCTRSUSP),'')           as FCTRSUSP     ,   " _
                    & "         isnull(rtrim(B.FCTRSMAKER),'')         as FCTRSMAKER   ,   " _
                    & "         isnull(rtrim(B.FCTRTMAKER),'')         as FCTRTMAKER   ,   " _
                    & "         isnull(rtrim(B.FCTRTIRE),'')           as FCTRTIRE     ,   " _
                    & "         isnull(rtrim(B.FCTRTMISSION),'')       as FCTRTMISSION ,   " _
                    & "         isnull(rtrim(B.FCTRUREA),'')           as FCTRUREA     ,   " _
                    & "         isnull(rtrim(B.OTNKBPIPE),'')          as OTNKBPIPE    ,   " _
                    & "         isnull(rtrim(B.OTNKCELLNO),'')         as OTNKCELLNO   ,   " _
                    & "         isnull(rtrim(B.OTNKVAPOR),'')          as OTNKVAPOR    ,   " _
                    & "         isnull(rtrim(B.OTNKCELPART),'')        as OTNKCELPART  ,   " _
                    & "         isnull(rtrim(B.OTNKCVALVE),'')         as OTNKCVALVE   ,   " _
                    & "         isnull(rtrim(B.OTNKDCD),'')            as OTNKDCD      ,   " _
                    & "         isnull(rtrim(B.OTNKDETECTOR),'')       as OTNKDETECTOR ,   " _
                    & "         isnull(rtrim(B.OTNKDISGORGE),'')       as OTNKDISGORGE ,   " _
                    & "         isnull(rtrim(B.OTNKHTECH),'')          as OTNKHTECH    ,   " _
                    & "         isnull(rtrim(B.OTNKINSSTAT),'')        as OTNKINSSTAT  ,   " _
                    & "         CASE WHEN B.OTNKINSYMD IS NULL THEN ''                     " _
                    & "              ELSE FORMAT(B.OTNKINSYMD,'yyyy/MM/dd')                " _
                    & "         END                                    as OTNKINSYMD   ,   " _
                    & "         isnull(rtrim(B.OTNKLVALVE),'')         as OTNKLVALVE   ,   " _
                    & "         isnull(rtrim(B.OTNKMATERIAL),'')       as OTNKMATERIAL ,   " _
                    & "         isnull(rtrim(B.OTNKPIPE),'')           as OTNKPIPE     ,   " _
                    & "         isnull(rtrim(B.OTNKPIPESIZE),'')       as OTNKPIPESIZE ,   " _
                    & "         isnull(rtrim(B.OTNKPUMP),'')           as OTNKPUMP     ,   " _
                    & "         isnull(rtrim(B.OTNKEXHASIZE),'')       as OTNKEXHASIZE ,   " _
                    & "         isnull(rtrim(B.OTNKTINSNO),'')         as OTNKTINSNO   ,   " _
                    & "         isnull(rtrim(B.OTNKTMAKER),'')         as OTNKTMAKER   ,   " _
                    & "         isnull(rtrim(B.HPRSINSISTAT),'')       as HPRSINSISTAT ,   " _
                    & "         CASE WHEN B.HPRSINSIYMD IS NULL THEN ''                    " _
                    & "              ELSE FORMAT(B.HPRSINSIYMD,'yyyy/MM/dd')               " _
                    & "         END                                    as HPRSINSIYMD  ,   " _
                    & "         isnull(rtrim(B.HPRSINSULATE),'')       as HPRSINSULATE ,   " _
                    & "         isnull(rtrim(B.HPRSMATR),'')           as HPRSMATR     ,   " _
                    & "         isnull(rtrim(B.HPRSPIPE),'')           as HPRSPIPE     ,   " _
                    & "         isnull(rtrim(B.HPRSPIPENUM),'')        as HPRSPIPENUM  ,   " _
                    & "         isnull(rtrim(B.HPRSPUMP),'')           as HPRSPUMP     ,   " _
                    & "         isnull(rtrim(B.HPRSRESSRE),'')         as HPRSRESSRE   ,   " _
                    & "         isnull(rtrim(B.HPRSSERNO),'')          as HPRSSERNO    ,   " _
                    & "         isnull(rtrim(B.HPRSSTRUCT),'')         as HPRSSTRUCT   ,   " _
                    & "         isnull(rtrim(B.HPRSVALVE),'')          as HPRSVALVE    ,   " _
                    & "         isnull(rtrim(B.HPRSPMPDR),'')          as HPRSPMPDR    ,   " _
                    & "         isnull(rtrim(B.HPRSHOSE),'')           as HPRSHOSE     ,   " _
                    & "         isnull(rtrim(B.HPRSTMAKER),'')         as HPRSTMAKER   ,   " _
                    & "         isnull(rtrim(B.CHEMCELLNO),'')         as CHEMCELLNO   ,   " _
                    & "         isnull(rtrim(B.CHEMCELPART),'')        as CHEMCELPART  ,   " _
                    & "         isnull(rtrim(B.CHEMDISGORGE),'')       as CHEMDISGORGE ,   " _
                    & "         isnull(rtrim(B.CHEMHOSE),'')           as CHEMHOSE     ,   " _
                    & "         isnull(rtrim(B.CHEMINSSTAT),'')        as CHEMINSSTAT  ,   " _
                    & "         CASE WHEN B.CHEMINSYMD IS NULL THEN ''                     " _
                    & "              ELSE FORMAT(B.CHEMINSYMD,'yyyy/MM/dd')                " _
                    & "         END                                    as CHEMINSYMD   ,   " _
                    & "         isnull(rtrim(B.CHEMMANOMTR),'')        as CHEMMANOMTR  ,   " _
                    & "         isnull(rtrim(B.CHEMMATERIAL),'')       as CHEMMATERIAL ,   " _
                    & "         isnull(rtrim(B.CHEMPMPDR),'')          as CHEMPMPDR    ,   " _
                    & "         isnull(rtrim(B.CHEMPRESDRV),'')        as CHEMPRESDRV  ,   " _
                    & "         isnull(rtrim(B.CHEMPRESEQ),'')         as CHEMPRESEQ   ,   " _
                    & "         isnull(rtrim(B.CHEMPUMP),'')           as CHEMPUMP     ,   " _
                    & "         isnull(rtrim(B.CHEMSTRUCT),'')         as CHEMSTRUCT   ,   " _
                    & "         isnull(rtrim(B.CHEMTHERM),'')          as CHEMTHERM    ,   " _
                    & "         isnull(rtrim(B.CHEMTINSNO),'')         as CHEMTINSNO   ,   " _
                    & "         isnull(rtrim(B.CHEMTMAKER),'')         as CHEMTMAKER   ,   " _
                    & "         isnull(rtrim(B.CONTSHAPE),'')          as CONTSHAPE    ,   " _
                    & "         isnull(rtrim(B.CONTPUMP),'')           as CONTPUMP     ,   " _
                    & "         isnull(rtrim(B.CONTPMPDR),'')          as CONTPMPDR    ,   " _
                    & "         isnull(rtrim(B.CONTTMAKER),'')         as CONTTMAKER   ,   " _
                    & "         isnull(rtrim(B.OFFCRESERVE1),'')       as OFFCRESERVE1 ,   " _
                    & "         isnull(rtrim(B.OFFCRESERVE2),'')       as OFFCRESERVE2 ,   " _
                    & "         isnull(rtrim(B.OFFCRESERVE3),'')       as OFFCRESERVE3 ,   " _
                    & "         isnull(rtrim(B.OFFCRESERVE4),'')       as OFFCRESERVE4 ,   " _
                    & "         isnull(rtrim(B.OFFCRESERVE5),'')       as OFFCRESERVE5 ,   " _
                    & "         isnull(rtrim(B.OTHRBMONITOR),'')       as OTHRBMONITOR ,   " _
                    & "         isnull(rtrim(B.OTHRBSONAR),'')         as OTHRBSONAR   ,   " _
                    & "         isnull(rtrim(B.OTHRDOCO),'')           as OTHRDOCO     ,   " _
                    & "         isnull(rtrim(B.OTHRDRRECORD),'')       as OTHRDRRECORD ,   " _
                    & "         isnull(rtrim(B.OTHRPAINTING),'')       as OTHRPAINTING ,   " _
                    & "         isnull(rtrim(B.OTHRRADIOCON),'')       as OTHRRADIOCON ,   " _
                    & "         isnull(rtrim(B.OTHRRTARGET),'')        as OTHRRTARGET  ,   " _
                    & "         isnull(rtrim(B.OTHRTERMINAL),'')       as OTHRTERMINAL ,   " _
                    & "         isnull(rtrim(B.OTHRTIRE1),'')          as OTHRTIRE1    ,   " _
                    & "         isnull(rtrim(B.OTHRTIRE2),'')          as OTHRTIRE2    ,   " _
                    & "         isnull(rtrim(B.OTHRTPMS),'')           as OTHRTPMS     ,   " _
                    & "         isnull(rtrim(B.OTHRETCNO),'')          as OTHRETCNO    ,   " _
                    & "         isnull(rtrim(B.OTHRASLID),'')          as OTHRASLID    ,   " _
                    & "         isnull(rtrim(B.OTHRETCCARDNO),'')      as OTHRETCCARDNO,   " _
                    & "         isnull(rtrim(B.OTHRKOUEINO),'')        as OTHRKOUEINO  ,   " _
                    & "         CASE WHEN B.ACCTLEASEEND IS NULL THEN ''                   " _
                    & "              ELSE FORMAT(B.ACCTLEASEEND,'yyyy/MM/dd')              " _
                    & "         END                                    as ACCTLEASEEND ,   " _
                    & "         isnull(rtrim(B.ACCTASST01),'')         as ACCTASST01   ,   " _
                    & "         isnull(rtrim(B.ACCTASST02),'')         as ACCTASST02   ,   " _
                    & "         isnull(rtrim(B.ACCTASST03),'')         as ACCTASST03   ,   " _
                    & "         isnull(rtrim(B.ACCTASST04),'')         as ACCTASST04   ,   " _
                    & "         isnull(rtrim(B.ACCTASST05),'')         as ACCTASST05   ,   " _
                    & "         isnull(rtrim(B.ACCTASST06),'')         as ACCTASST06   ,   " _
                    & "         isnull(rtrim(B.ACCTASST07),'')         as ACCTASST07   ,   " _
                    & "         isnull(rtrim(B.ACCTASST08),'')         as ACCTASST08   ,   " _
                    & "         isnull(rtrim(B.ACCTASST09),'')         as ACCTASST09   ,   " _
                    & "         isnull(rtrim(B.ACCTASST10),'')         as ACCTASST10   ,   " _
                    & "         isnull(rtrim(B.ACCTLEASE1),'')         as ACCTLEASE1   ,   " _
                    & "         isnull(rtrim(B.ACCTLEASE2),'')         as ACCTLEASE2   ,   " _
                    & "         isnull(rtrim(B.ACCTLEASE3),'')         as ACCTLEASE3   ,   " _
                    & "         isnull(rtrim(B.ACCTLEASE4),'')         as ACCTLEASE4   ,   " _
                    & "         isnull(rtrim(B.ACCTLEASE5),'')         as ACCTLEASE5   ,   " _
                    & "         isnull(rtrim(B.ACCTLSUPL1),'')         as ACCTLSUPL1   ,   " _
                    & "         isnull(rtrim(B.ACCTLSUPL2),'')         as ACCTLSUPL2   ,   " _
                    & "         isnull(rtrim(B.ACCTLSUPL3),'')         as ACCTLSUPL3   ,   " _
                    & "         isnull(rtrim(B.ACCTLSUPL4),'')         as ACCTLSUPL4   ,   " _
                    & "         isnull(rtrim(B.ACCTLSUPL5),'')         as ACCTLSUPL5   ,   " _
                    & "         cast(isnull(B.ACCTRCYCLE,'') as VarChar)as ACCTRCYCLE  ,   " _
                    & "         isnull(rtrim(B.NOTES),'')              as NOTES        ,   " _
                    & "         CASE WHEN C.CHEMTINSNYMD IS NULL THEN ''                   " _
                    & "              ELSE FORMAT(C.CHEMTINSNYMD,'yyyy/MM/dd')              " _
                    & "         END                                     as CHEMTINSNYMD,   " _
                    & "         CASE WHEN C.CHEMTINSYMD IS NULL THEN ''                    " _
                    & "              ELSE FORMAT(C.CHEMTINSYMD,'yyyy/MM/dd')               " _
                    & "         END                                     as CHEMTINSYMD ,   " _
                    & "         cast(isnull(C.LICN5LDCAPA,'') as VarChar) as LICN5LDCAPA ,   " _
                    & "         cast(isnull(C.LICNCWEIGHT,'') as VarChar) as LICNCWEIGHT ,   " _
                    & "         isnull(rtrim(C.LICNFRAMENO),'')         as LICNFRAMENO ,   " _
                    & "         cast(isnull(C.LICNLDCAPA,'') as VarChar) as LICNLDCAPA  ,   " _
                    & "         isnull(rtrim(C.LICNMNFACT),'')          as LICNMNFACT  ,   " _
                    & "         isnull(rtrim(C.LICNMODEL),'')           as LICNMODEL   ,   " _
                    & "         isnull(rtrim(C.LICNMOTOR),'')           as LICNMOTOR   ,   " _
                    & "         isnull(rtrim(C.LICNPLTNO1),'')          as LICNPLTNO1  ,   " _
                    & "         isnull(rtrim(C.LICNPLTNO2),'')          as LICNPLTNO2  ,   " _
                    & "         cast(isnull(C.LICNTWEIGHT,'') as VarChar) as LICNTWEIGHT ,   " _
                    & "         cast(isnull(C.LICNWEIGHT,'') as VarChar) as LICNWEIGHT  ,   " _
                    & "         CASE WHEN C.LICNYMD IS NULL THEN ''                        " _
                    & "              ELSE FORMAT(C.LICNYMD,'yyyy/MM/dd')                   " _
                    & "         END                                     as LICNYMD     ,   " _
                    & "         cast(isnull(C.TAXATAX,'') as VarChar)   as TAXATAX     ,   " _
                    & "         cast(isnull(C.TAXLINS,'') as VarChar)   as TAXLINS     ,   " _
                    & "         CASE WHEN C.TAXLINSYMD IS NULL THEN ''                     " _
                    & "              ELSE FORMAT(C.TAXLINSYMD,'yyyy/MM/dd')                " _
                    & "         END                                     as TAXLINSYMD  ,   " _
                    & "         cast(isnull(C.TAXVTAX,'') as VarChar) as TAXVTAX       ,   " _
                    & "         CASE WHEN C.OTNKTINSNYMD IS NULL THEN ''                   " _
                    & "              ELSE FORMAT(C.OTNKTINSNYMD,'yyyy/MM/dd')              " _
                    & "         END                                     as OTNKTINSNYMD,   " _
                    & "         CASE WHEN C.OTNKTINSYMD IS NULL THEN ''                    " _
                    & "              ELSE FORMAT(C.OTNKTINSYMD,'yyyy/MM/dd')               " _
                    & "         END                                     as OTNKTINSYMD ,   " _
                    & "         CASE WHEN C.HPRSINSNYMD IS NULL THEN ''                    " _
                    & "              ELSE FORMAT(C.HPRSINSNYMD,'yyyy/MM/dd')               " _
                    & "         END                                     as HPRSINSNYMD ,   " _
                    & "         CASE WHEN C.HPRSINSYMD IS NULL THEN ''                     " _
                    & "              ELSE FORMAT(C.HPRSINSYMD,'yyyy/MM/dd')                " _
                    & "         END                                     as HPRSINSYMD  ,   " _
                    & "         CASE WHEN C.HPRSJINSYMD IS NULL THEN ''                    " _
                    & "              ELSE FORMAT(C.HPRSJINSYMD,'yyyy/MM/dd')               " _
                    & "         END                                     as HPRSJINSYMD ,   " _
                    & "         isnull(rtrim(C.INSKBN),'')              as INSKBN      ,   " _
                    & "         isnull(rtrim(D.SHARYOTYPEF),'')         as SHARYOTYPEF ,   " _
                    & "         isnull(rtrim(D.TSHABANF),'')            as TSHABANF    ,   " _
                    & "         isnull(rtrim(D.SHARYOTYPEB),'')         as SHARYOTYPEB ,   " _
                    & "         isnull(rtrim(D.TSHABANB),'')            as TSHABANB    ,   " _
                    & "         isnull(rtrim(D.SHARYOTYPEB2),'')        as SHARYOTYPEB2,   " _
                    & "         isnull(rtrim(D.TSHABANB2),'')           as TSHABANB2   ,   " _
                    & "         ''                                      as SHARYOTYPEB3,   " _
                    & "         ''                                      as TSHABANB3   ,   " _
                    & "         isnull(rtrim(D.GSHABAN),'')             as GSHABAN     ,   " _
                    & "         isnull(D.SEQ,'0')                       as SEQ         ,   " _
                    & "         isnull(rtrim(D.MANGUORG),'')            as MANGUORG    ,   " _
                    & "         isnull(rtrim(A.CAMPCODE),'')            as CAMPCODE    ,   " _
                    & "         isnull(rtrim(A.SHARYOTYPE),'')          as SHARYOTYPE  ,   " _
                    & "         isnull(rtrim(A.TSHABAN),'')             as TSHABAN     ,   " _
                    & "         CASE WHEN C.STYMD IS NULL THEN ''                          " _
                    & "              ELSE FORMAT(C.STYMD,'yyyy/MM/dd')                     " _
                    & "         END                                     as STYMD       ,   " _
                    & "         CASE WHEN C.ENDYMD IS NULL THEN ''                         " _
                    & "              ELSE FORMAT(C.ENDYMD,'yyyy/MM/dd')                    " _
                    & "         END                                     as ENDYMD      ,   " _
                    & "         CASE WHEN A.STYMD IS NULL THEN ''                          " _
                    & "              ELSE FORMAT(A.STYMD,'yyyy/MM/dd')                     " _
                    & "         END                                     as STYMD_A     ,   " _
                    & "         CASE WHEN A.ENDYMD IS NULL THEN ''                         " _
                    & "              ELSE FORMAT(A.ENDYMD,'yyyy/MM/dd')                    " _
                    & "         END                                     as ENDYMD_A    ,   " _
                    & "         CASE WHEN B.STYMD IS NULL THEN ''                          " _
                    & "              ELSE FORMAT(B.STYMD,'yyyy/MM/dd')                     " _
                    & "         END                                     as STYMD_B     ,   " _
                    & "         CASE WHEN B.ENDYMD IS NULL THEN ''                         " _
                    & "              ELSE FORMAT(B.ENDYMD,'yyyy/MM/dd')                    " _
                    & "         END                                     as ENDYMD_B    ,   " _
                    & "         CASE WHEN C.STYMD IS NULL THEN ''                          " _
                    & "              ELSE FORMAT(C.STYMD,'yyyy/MM/dd')                     " _
                    & "         END                                     as STYMD_C     ,   " _
                    & "         CASE WHEN C.ENDYMD IS NULL THEN ''                         " _
                    & "              ELSE FORMAT(C.ENDYMD,'yyyy/MM/dd')                    " _
                    & "         END                                     as ENDYMD_C    ,   " _
                    & "         FORMAT(getdate(),'yyyy/MM/dd')          as STYMD_S     ,   " _
                    & "         FORMAT(getdate(),'yyyy/MM/dd')          as ENDYMD_S    ,   " _
                    & "         isnull(rtrim(C.DELFLG),'0')             as DELFLG      ,   " _
                    & "         ''                                      as INITYMD     ,   " _
                    & "         ''                                      as UPDYMD      ,   " _
                    & "         ''                                      as UPDUSER     ,   " _
                    & "         isnull(rtrim(A.SHARYOSTATUS),'')        as SHARYOSTATUS,   " _
                    & "         isnull(rtrim(D.SHARYOINFO1),'')         as SHARYOINFO1 ,   " _
                    & "         isnull(rtrim(D.SHARYOINFO2),'')         as SHARYOINFO2 ,   " _
                    & "         isnull(rtrim(D.SHARYOINFO3),'')         as SHARYOINFO3 ,   " _
                    & "         isnull(rtrim(D.SHARYOINFO4),'')         as SHARYOINFO4 ,   " _
                    & "         isnull(rtrim(D.SHARYOINFO5),'')         as SHARYOINFO5 ,   " _
                    & "         isnull(rtrim(D.SHARYOINFO6),'')         as SHARYOINFO6 ,   " _
                    & "         ''                                      as MANGMORGNAME,   " _
                    & "         ''                                      as MANGSORGNAME,   " _
                    & "         ''                                      as MANGOILTYPENAME," _
                    & "         ''                                      as MANGOWNCODENAME," _
                    & "         ''                                      as MANGOWNCONTNAME," _
                    & "         ''                                      as MANGSUPPLNAME,  " _
                    & "         ''                                      as MANGUORGNAME,   " _
                    & "         ''                                      as BASELEASENAME , " _
                    & "         ''                                      as FCTRAXLENAME,   " _
                    & "         ''                                      as FCTRDPRNAME ,   " _
                    & "         ''                                      as FCTRFUELMATENAME," _
                    & "         ''                                      as FCTRSHFTNUMNAME," _
                    & "         ''                                      as FCTRSUSPNAME,   " _
                    & "         ''                                      as FCTRTMISSIONNAME," _
                    & "         ''                                      as FCTRUREANAME,   " _
                    & "         ''                                      as OTNKBPIPENAME,  " _
                    & "         ''                                      as OTNKVAPORNAME,  " _
                    & "         ''                                      as OTNKCVALVENAME, " _
                    & "         ''                                      as OTNKDCDNAME ,   " _
                    & "         ''                                      as OTNKDETECTORNAME," _
                    & "         ''                                      as OTNKDISGORGENAME," _
                    & "         ''                                      as OTNKHTECHNAME,  " _
                    & "         ''                                      as OTNKLVALVENAME, " _
                    & "         ''                                      as OTNKMATERIALNAME," _
                    & "         ''                                      as OTNKPIPENAME,   " _
                    & "         ''                                      as OTNKPIPESIZENAME," _
                    & "         ''                                      as OTNKPUMPNAME,   " _
                    & "         ''                                      as HPRSINSULATENAME," _
                    & "         ''                                      as HPRSMATRNAME,   " _
                    & "         ''                                      as HPRSPIPENAME,   " _
                    & "         ''                                      as HPRSPIPENUMNAME," _
                    & "         ''                                      as HPRSPUMPNAME,   " _
                    & "         ''                                      as HPRSRESSRENAME, " _
                    & "         ''                                      as HPRSSTRUCTNAME, " _
                    & "         ''                                      as HPRSVALVENAME,  " _
                    & "         ''                                      as CHEMDISGORGENAME," _
                    & "         ''                                      as CHEMHOSENAME,   " _
                    & "         ''                                      as CHEMMANOMTRNAME," _
                    & "         ''                                      as CHEMMATERIALNAME," _
                    & "         ''                                      as CHEMPMPDRNAME,  " _
                    & "         ''                                      as CHEMPRESDRVNAME," _
                    & "         ''                                      as CHEMPRESEQNAME, " _
                    & "         ''                                      as CHEMPUMPNAME,   " _
                    & "         ''                                      as CHEMSTRUCTNAME, " _
                    & "         ''                                      as CHEMTHERMNAME,  " _
                    & "         ''                                      as OTHRBMONITORNAME," _
                    & "         ''                                      as OTHRBSONARNAME, " _
                    & "         ''                                      as FCTRTIRENAME,   " _
                    & "         ''                                      as OTHRDRRECORDNAME," _
                    & "         ''                                      as OTHRPAINTINGNAME," _
                    & "         ''                                      as OTHRRADIOCONNAME," _
                    & "         ''                                      as OTHRRTARGETNAME," _
                    & "         ''                                      as OTHRTERMINALNAME," _
                    & "         ''                                      as MANGPROD1NAME,  " _
                    & "         ''                                      as MANGPROD2NAME,  " _
                    & "         ''                                      as FCTRSMAKERNAME, " _
                    & "         ''                                      as FCTRTMAKERNAME, " _
                    & "         ''                                      as OTNKEXHASIZENAME," _
                    & "         ''                                      as HPRSPMPDRNAME,  " _
                    & "         ''                                      as HPRSHOSENAME ,  " _
                    & "         ''                                      as CONTSHAPENAME,  " _
                    & "         ''                                      as CONTPUMPNAME ,  " _
                    & "         ''                                      as CONTPMPDRNAME,  " _
                    & "         ''                                      as OTHRTPMSNAME ,  " _
                    & "         ''                                      as OTNKTMAKERNAME, " _
                    & "         ''                                      as HPRSTMAKERNAME, " _
                    & "         ''                                      as CHEMTMAKERNAME, " _
                    & "         ''                                      as CONTTMAKERNAME, " _
                    & "         ''                                      as INSKBNNAME  ,   " _
                    & "         ''                                      as SHARYOSTATUSNAME"
                Dim SQLStr2 As String = ""
                SQLStr2 = " FROM   MA002_SHARYOA       A                               " _
                & " INNER JOIN MA003_SHARYOB       B                             ON    " _
                & "             B.CAMPCODE        = A.CAMPCODE                         " _
                & "       and   B.SHARYOTYPE      = A.SHARYOTYPE                       " _
                & "       and   B.TSHABAN         = A.TSHABAN                          " _
                & "       and   B.STYMD          <= @P05                               " _
                & "       and   B.ENDYMD         >= @P04                               " _
                & "       and   B.DELFLG         <> '" & C_DELETE_FLG.DELETE & "'      " _
                & " LEFT  JOIN MA004_SHARYOC       C                             ON    " _
                & "             C.CAMPCODE        = A.CAMPCODE                         " _
                & "       and   C.SHARYOTYPE      = A.SHARYOTYPE                       " _
                & "       and   C.TSHABAN         = A.TSHABAN                          " _
                & "       and   C.STYMD          <= @P07                               " _
                & "       and   C.ENDYMD         >= @P06                               " _
                & "       and   C.ENDYMD          = (                                  " _
                & "          select                                                    " _
                & "                 max(ENDYMD)                                        " _
                & "          from     MA004_SHARYOC      MXC                           " _
                & "          where                                                     " _
                & "                    MXC.CAMPCODE      = A.CAMPCODE                  " _
                & "                and MXC.SHARYOTYPE    = A.SHARYOTYPE                " _
                & "                and MXC.TSHABAN       = A.TSHABAN                   " _
                & "                and MXC.STYMD        <= @P07                        " _
                & "                and MXC.ENDYMD       >= @P06                        " _
                & "                and MXC.DELFLG       <> '1'                         " _
                & "       )                                                            " _
                & "       and   C.DELFLG         <> '" & C_DELETE_FLG.DELETE & "'      " _
                & " LEFT  JOIN MA006_SHABANORG     D                             ON    " _
                & "             D.CAMPCODE        = A.CAMPCODE                         " _
                & "       and   D.MANGUORG        = A.MANGSORG                         " _
                & "       and   (                                                      " _
                & "                (                                                   " _
                & "                      D.SHARYOTYPEF     = A.SHARYOTYPE              " _
                & "                  and D.TSHABANF        = A.TSHABAN                 " _
                & "                )                                                   " _
                & "             or                                                     " _
                & "                (                                                   " _
                & "                      D.SHARYOTYPEB     = A.SHARYOTYPE              " _
                & "                  and D.TSHABANB        = A.TSHABAN                 " _
                & "                )                                                   " _
                & "             or                                                     " _
                & "                (                                                   " _
                & "                      D.SHARYOTYPEB2    = A.SHARYOTYPE              " _
                & "                  and D.TSHABANB2       = A.TSHABAN                 " _
                & "                )                                                   " _
                & "             )                                                      " _
                & "       and   D.DELFLG         <> '" & C_DELETE_FLG.DELETE & "'      " _
                & " INNER JOIN S0006_ROLE          Y                               ON  " _
                & "             Y.CAMPCODE        = A.CAMPCODE                         " _
                & "       and   (                                                      " _
                & "                   Y.CODE        = A.MANGMORG                       " _
                & "               or  Y.CODE        = A.MANGSORG                       " _
                & "             )                                                      " _
                & "       and   Y.OBJECT          = 'ORG'                              " _
                & "       and   Y.ROLE            = @P01                               " _
                & "       and   Y.STYMD          <= @P03                               " _
                & "       and   Y.ENDYMD         >= @P03                               " _
                & "       and   Y.DELFLG         <> '1'                                " _
                & " WHERE                                                              " _
                & "             A.CAMPCODE        = @P02                               " _
                & "       and   A.STYMD          <= @P05                               " _
                & "       and   A.ENDYMD         >= @P04                               " _
                & "       and   A.DELFLG         <> '1'                                " _
                & " ORDER BY B.SHARYOTYPE ASC, B.TSHABAN ASC, A.STYMD DESC, B.STYMD DESC, C.STYMD "

                Using SQLcmd As New SqlCommand(SQLStr & SQLStr2, SQLcon)
                    Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P01", SqlDbType.NVarChar, 20)
                    Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P02", SqlDbType.NVarChar, 20)
                    Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P03", SqlDbType.Date)
                    Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P04", SqlDbType.Date)
                    Dim PARA5 As SqlParameter = SQLcmd.Parameters.Add("@P05", SqlDbType.Date)
                    Dim PARA6 As SqlParameter = SQLcmd.Parameters.Add("@P06", SqlDbType.Date)
                    Dim PARA7 As SqlParameter = SQLcmd.Parameters.Add("@P07", SqlDbType.Date)

                    PARA1.Value = Master.ROLE_ORG
                    PARA2.Value = work.WF_SEL_CAMPCODE.Text
                    PARA3.Value = Date.Now
                    PARA4.Value = WW_STYMD_Yuko        '有効期限(開始)
                    PARA5.Value = WW_ENDYMD_Yuko       '有効期限(終了)
                    PARA6.Value = WW_STYMD_Nendo       '対象年度(開始)
                    PARA7.Value = WW_ENDYMD_Nendo      '対象年度(終了)

                    Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                        'フィールド名とフィールドの型を取得
                        For index As Integer = 0 To SQLdr.FieldCount - 1
                            MA0004tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                        Next

                        'MA0004tbl値設定
                        Dim WW_DATA_CNT As Integer = -1
                        While SQLdr.Read

                            '○テーブル初期化
                            Dim MA0004row As DataRow = MA0004tbl.NewRow()
                            Dim WW_DATE As Date

                            '○データ設定

                            '固定項目
                            WW_DATA_CNT = WW_DATA_CNT + 1
                            MA0004row("WORK_NO") = WW_DATA_CNT.ToString()
                            MA0004row("LINECNT") = 0
                            MA0004row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                            If IsDBNull(SQLdr("TIMSTP")) Then
                                MA0004row("TIMSTP") = "0"
                            Else
                                MA0004row("TIMSTP") = SQLdr("TIMSTP")
                            End If

                            MA0004row("SELECT") = 1   '1:表示
                            MA0004row("HIDDEN") = 0   '0:表示

                            '画面毎の設定項目
                            MA0004row("CAMPCODE") = SQLdr("CAMPCODE")
                            MA0004row("SHARYOTYPE") = SQLdr("SHARYOTYPE")
                            MA0004row("TSHABAN") = SQLdr("TSHABAN")

                            MA0004row("STYMD") = If(SQLdr("STYMD"), "")
                            MA0004row("ENDYMD") = If(SQLdr("ENDYMD"), "")

                            'デバック用フィールド
                            MA0004row("STYMD_S") = If(SQLdr("STYMD_S"), "")
                            MA0004row("ENDYMD_S") = If(SQLdr("ENDYMD_S"), "")

                            MA0004row("STYMD_A") = If(SQLdr("STYMD_A"), "")
                            MA0004row("ENDYMD_A") = If(SQLdr("ENDYMD_A"), "")

                            MA0004row("STYMD_B") = If(SQLdr("STYMD_B"), "")
                            MA0004row("ENDYMD_B") = If(SQLdr("ENDYMD_B"), "")

                            MA0004row("STYMD_C") = If(SQLdr("STYMD_C"), "")
                            MA0004row("ENDYMD_C") = If(SQLdr("ENDYMD_C"), "")


                            MA0004row("DELFLG") = SQLdr("DELFLG")
                            MA0004row("SHARYOTYPEF") = SQLdr("SHARYOTYPEF")
                            MA0004row("TSHABANF") = SQLdr("TSHABANF")
                            MA0004row("SHARYOTYPEB") = SQLdr("SHARYOTYPEB")
                            MA0004row("TSHABANB") = SQLdr("TSHABANB")
                            MA0004row("SHARYOTYPEB2") = SQLdr("SHARYOTYPEB2")
                            MA0004row("TSHABANB2") = SQLdr("TSHABANB2")
                            MA0004row("SHARYOTYPEB3") = SQLdr("SHARYOTYPEB3")
                            MA0004row("TSHABANB3") = SQLdr("TSHABANB3")
                            MA0004row("GSHABAN") = SQLdr("GSHABAN")
                            MA0004row("SEQ") = SQLdr("SEQ")
                            MA0004row("MANGMORG") = SQLdr("MANGMORG")
                            MA0004row("MANGSORG") = SQLdr("MANGSORG")
                            MA0004row("MANGOILTYPE") = SQLdr("MANGOILTYPE")
                            MA0004row("MANGOWNCODE") = SQLdr("MANGOWNCODE")
                            MA0004row("MANGOWNCONT") = SQLdr("MANGOWNCONT")
                            MA0004row("MANGSHAFUKU") = SQLdr("MANGSHAFUKU")
                            MA0004row("MANGSUPPL") = SQLdr("MANGSUPPL")
                            MA0004row("MANGTTLDIST") = SQLdr("MANGTTLDIST")
                            MA0004row("MANGUORG") = SQLdr("MANGUORG")
                            MA0004row("BASELEASE") = SQLdr("BASELEASE")
                            MA0004row("BASERAGE") = SQLdr("BASERAGE")
                            MA0004row("BASERAGEMM") = SQLdr("BASERAGEMM")
                            MA0004row("BASERAGEYY") = SQLdr("BASERAGEYY")
                            MA0004row("BASERDATE") = If(SQLdr("BASERDATE"), "")
                            If IsDBNull(SQLdr("BASERDATE")) OrElse SQLdr("BASERDATE") = "" Then
                                MA0004row("BASERDATE") = ""
                                MA0004row("BASERAGEMM") = "0"
                                MA0004row("BASERAGEYY") = "0"
                                MA0004row("BASERAGE") = "0"
                            Else
                                WW_DATE = SQLdr("BASERDATE")
                                MA0004row("BASERDATE") = SQLdr("BASERDATE")
                                Dim WW_DATENOW As Date = Date.Now
                                Dim WW_BASERAGEYY As Integer
                                Dim WW_BASERAGE As Integer
                                Dim WW_BASERAGEMM As Integer
                                WW_BASERAGE = DateDiff("m", WW_DATE, WW_DATENOW)
                                WW_BASERAGEYY = Math.Truncate(WW_BASERAGE / 12)
                                WW_BASERAGEMM = WW_BASERAGE Mod 12
                                MA0004row("BASERAGEMM") = WW_BASERAGEMM
                                MA0004row("BASERAGEYY") = WW_BASERAGEYY
                                MA0004row("BASERAGE") = WW_BASERAGE
                            End If
                            MA0004row("FCTRAXLE") = SQLdr("FCTRAXLE")
                            MA0004row("FCTRDPR") = SQLdr("FCTRDPR")
                            MA0004row("FCTRFUELCAPA") = SQLdr("FCTRFUELCAPA")
                            MA0004row("FCTRFUELMATE") = SQLdr("FCTRFUELMATE")
                            MA0004row("FCTRRESERVE1") = SQLdr("FCTRRESERVE1")
                            MA0004row("FCTRRESERVE2") = SQLdr("FCTRRESERVE2")
                            MA0004row("FCTRRESERVE3") = SQLdr("FCTRRESERVE3")
                            MA0004row("FCTRRESERVE4") = SQLdr("FCTRRESERVE4")
                            MA0004row("FCTRRESERVE5") = SQLdr("FCTRRESERVE5")
                            MA0004row("FCTRSHFTNUM") = SQLdr("FCTRSHFTNUM")
                            MA0004row("FCTRSUSP") = SQLdr("FCTRSUSP")
                            MA0004row("FCTRTIRE") = SQLdr("FCTRTIRE")
                            MA0004row("FCTRTMISSION") = SQLdr("FCTRTMISSION")
                            MA0004row("FCTRUREA") = SQLdr("FCTRUREA")
                            MA0004row("OTNKBPIPE") = SQLdr("OTNKBPIPE")
                            MA0004row("OTNKCELLNO") = SQLdr("OTNKCELLNO")
                            MA0004row("OTNKVAPOR") = SQLdr("OTNKVAPOR")
                            MA0004row("OTNKCELPART") = SQLdr("OTNKCELPART")
                            MA0004row("OTNKCVALVE") = SQLdr("OTNKCVALVE")
                            MA0004row("OTNKDCD") = SQLdr("OTNKDCD")
                            MA0004row("OTNKDETECTOR") = SQLdr("OTNKDETECTOR")
                            MA0004row("OTNKDISGORGE") = SQLdr("OTNKDISGORGE")
                            MA0004row("OTNKHTECH") = SQLdr("OTNKHTECH")
                            MA0004row("OTNKINSSTAT") = SQLdr("OTNKINSSTAT")
                            MA0004row("OTNKINSYMD") = If(SQLdr("OTNKINSYMD"), "")

                            MA0004row("OTNKLVALVE") = SQLdr("OTNKLVALVE")
                            MA0004row("OTNKMATERIAL") = SQLdr("OTNKMATERIAL")
                            MA0004row("OTNKPIPE") = SQLdr("OTNKPIPE")
                            MA0004row("OTNKPIPESIZE") = SQLdr("OTNKPIPESIZE")
                            MA0004row("OTNKPUMP") = SQLdr("OTNKPUMP")
                            MA0004row("OTNKTINSNO") = SQLdr("OTNKTINSNO")
                            MA0004row("HPRSINSISTAT") = SQLdr("HPRSINSISTAT")

                            MA0004row("HPRSINSIYMD") = If(SQLdr("HPRSINSIYMD"), "")

                            MA0004row("HPRSINSULATE") = SQLdr("HPRSINSULATE")
                            MA0004row("HPRSMATR") = SQLdr("HPRSMATR")
                            MA0004row("HPRSPIPE") = SQLdr("HPRSPIPE")
                            MA0004row("HPRSPIPENUM") = SQLdr("HPRSPIPENUM")
                            MA0004row("HPRSPUMP") = SQLdr("HPRSPUMP")
                            MA0004row("HPRSRESSRE") = SQLdr("HPRSRESSRE")
                            MA0004row("HPRSSERNO") = SQLdr("HPRSSERNO")
                            MA0004row("HPRSSTRUCT") = SQLdr("HPRSSTRUCT")
                            MA0004row("HPRSVALVE") = SQLdr("HPRSVALVE")
                            MA0004row("CHEMCELLNO") = SQLdr("CHEMCELLNO")
                            MA0004row("CHEMCELPART") = SQLdr("CHEMCELPART")
                            MA0004row("CHEMDISGORGE") = SQLdr("CHEMDISGORGE")
                            MA0004row("CHEMHOSE") = SQLdr("CHEMHOSE")
                            MA0004row("CHEMINSSTAT") = SQLdr("CHEMINSSTAT")

                            MA0004row("CHEMINSYMD") = If(SQLdr("CHEMINSYMD"), "")

                            MA0004row("CHEMMANOMTR") = SQLdr("CHEMMANOMTR")
                            MA0004row("CHEMMATERIAL") = SQLdr("CHEMMATERIAL")
                            MA0004row("CHEMPMPDR") = SQLdr("CHEMPMPDR")
                            MA0004row("CHEMPRESDRV") = SQLdr("CHEMPRESDRV")
                            MA0004row("CHEMPRESEQ") = SQLdr("CHEMPRESEQ")
                            MA0004row("CHEMPUMP") = SQLdr("CHEMPUMP")
                            MA0004row("CHEMSTRUCT") = SQLdr("CHEMSTRUCT")
                            MA0004row("CHEMTHERM") = SQLdr("CHEMTHERM")
                            MA0004row("CHEMTINSNO") = SQLdr("CHEMTINSNO")

                            MA0004row("CHEMTINSNYMD") = If(SQLdr("CHEMTINSNYMD"), "")
                            MA0004row("CHEMTINSYMD") = If(SQLdr("CHEMTINSYMD"), "")

                            MA0004row("OFFCRESERVE1") = SQLdr("OFFCRESERVE1")
                            MA0004row("OFFCRESERVE2") = SQLdr("OFFCRESERVE2")
                            MA0004row("OFFCRESERVE3") = SQLdr("OFFCRESERVE3")
                            MA0004row("OFFCRESERVE4") = SQLdr("OFFCRESERVE4")
                            MA0004row("OFFCRESERVE5") = SQLdr("OFFCRESERVE5")
                            MA0004row("OTHRBMONITOR") = SQLdr("OTHRBMONITOR")
                            MA0004row("OTHRBSONAR") = SQLdr("OTHRBSONAR")
                            MA0004row("OTHRDOCO") = SQLdr("OTHRDOCO")
                            MA0004row("OTHRDRRECORD") = SQLdr("OTHRDRRECORD")
                            MA0004row("OTHRPAINTING") = SQLdr("OTHRPAINTING")
                            MA0004row("OTHRRADIOCON") = SQLdr("OTHRRADIOCON")
                            MA0004row("OTHRRTARGET") = SQLdr("OTHRRTARGET")
                            MA0004row("OTHRTERMINAL") = SQLdr("OTHRTERMINAL")
                            MA0004row("OTHRETCNO") = SQLdr("OTHRETCNO")
                            MA0004row("OTHRASLID") = SQLdr("OTHRASLID")
                            MA0004row("OTHRETCCARDNO") = SQLdr("OTHRETCCARDNO")
                            MA0004row("OTHRKOUEINO") = SQLdr("OTHRKOUEINO")
                            MA0004row("ACCTLEASEEND") = If(SQLdr("ACCTLEASEEND"), "")
                            MA0004row("ACCTASST01") = SQLdr("ACCTASST01")
                            MA0004row("ACCTASST02") = SQLdr("ACCTASST02")
                            MA0004row("ACCTASST03") = SQLdr("ACCTASST03")
                            MA0004row("ACCTASST04") = SQLdr("ACCTASST04")
                            MA0004row("ACCTASST05") = SQLdr("ACCTASST05")
                            MA0004row("ACCTASST06") = SQLdr("ACCTASST06")
                            MA0004row("ACCTASST07") = SQLdr("ACCTASST07")
                            MA0004row("ACCTASST08") = SQLdr("ACCTASST08")
                            MA0004row("ACCTASST09") = SQLdr("ACCTASST09")
                            MA0004row("ACCTASST10") = SQLdr("ACCTASST10")
                            MA0004row("ACCTLEASE1") = SQLdr("ACCTLEASE1")
                            MA0004row("ACCTLEASE2") = SQLdr("ACCTLEASE2")
                            MA0004row("ACCTLEASE3") = SQLdr("ACCTLEASE3")
                            MA0004row("ACCTLEASE4") = SQLdr("ACCTLEASE4")
                            MA0004row("ACCTLEASE5") = SQLdr("ACCTLEASE5")
                            MA0004row("ACCTLSUPL1") = SQLdr("ACCTLSUPL1")
                            MA0004row("ACCTLSUPL2") = SQLdr("ACCTLSUPL2")
                            MA0004row("ACCTLSUPL3") = SQLdr("ACCTLSUPL3")
                            MA0004row("ACCTLSUPL4") = SQLdr("ACCTLSUPL4")
                            MA0004row("ACCTLSUPL5") = SQLdr("ACCTLSUPL5")
                            MA0004row("ACCTRCYCLE") = Format(Val(SQLdr("ACCTRCYCLE")), "#,#")
                            MA0004row("NOTES") = SQLdr("NOTES")
                            MA0004row("LICN5LDCAPA") = SQLdr("LICN5LDCAPA")
                            MA0004row("LICNCWEIGHT") = SQLdr("LICNCWEIGHT")
                            MA0004row("LICNFRAMENO") = SQLdr("LICNFRAMENO")
                            MA0004row("LICNLDCAPA") = SQLdr("LICNLDCAPA")
                            MA0004row("LICNMNFACT") = SQLdr("LICNMNFACT")
                            MA0004row("LICNMODEL") = SQLdr("LICNMODEL")
                            MA0004row("LICNMOTOR") = SQLdr("LICNMOTOR")
                            MA0004row("LICNPLTNO1") = SQLdr("LICNPLTNO1")
                            MA0004row("LICNPLTNO2") = SQLdr("LICNPLTNO2")
                            MA0004row("LICNTWEIGHT") = SQLdr("LICNTWEIGHT")
                            MA0004row("LICNWEIGHT") = SQLdr("LICNWEIGHT")

                            MA0004row("LICNYMD") = If(SQLdr("LICNYMD"), "")

                            If Val(SQLdr("TAXATAX")) = 0 Then
                                MA0004row("TAXATAX") = "0"
                            Else
                                MA0004row("TAXATAX") = Format(Val(SQLdr("TAXATAX")), "#,#")
                            End If
                            If Val(SQLdr("TAXLINS")) = 0 Then
                                MA0004row("TAXLINS") = "0"
                            Else
                                MA0004row("TAXLINS") = Format(Val(SQLdr("TAXLINS")), "#,#")
                            End If

                            MA0004row("TAXLINSYMD") = If(SQLdr("TAXLINSYMD"), "")

                            If Val(SQLdr("TAXVTAX")) = 0 Then
                                MA0004row("TAXVTAX") = "0"
                            Else
                                MA0004row("TAXVTAX") = Format(Val(SQLdr("TAXVTAX")), "#,#")
                            End If

                            MA0004row("OTNKTINSNYMD") = If(SQLdr("OTNKTINSNYMD"), "")
                            MA0004row("OTNKTINSYMD") = If(SQLdr("OTNKTINSYMD"), "")
                            MA0004row("HPRSINSNYMD") = If(SQLdr("HPRSINSNYMD"), "")
                            MA0004row("HPRSINSYMD") = If(SQLdr("HPRSINSYMD"), "")
                            MA0004row("HPRSJINSYMD") = If(SQLdr("HPRSJINSYMD"), "")

                            MA0004row("INSKBN") = SQLdr("INSKBN")
                            MA0004row("MANGPROD1") = SQLdr("MANGPROD1")
                            MA0004row("MANGPROD2") = SQLdr("MANGPROD2")
                            MA0004row("FCTRSMAKER") = SQLdr("FCTRSMAKER")
                            MA0004row("FCTRTMAKER") = SQLdr("FCTRTMAKER")
                            MA0004row("OTNKEXHASIZE") = SQLdr("OTNKEXHASIZE")
                            MA0004row("HPRSPMPDR") = SQLdr("HPRSPMPDR")
                            MA0004row("HPRSHOSE") = SQLdr("HPRSHOSE")
                            MA0004row("CONTSHAPE") = SQLdr("CONTSHAPE")
                            MA0004row("CONTPUMP") = SQLdr("CONTPUMP")
                            MA0004row("CONTPMPDR") = SQLdr("CONTPMPDR")
                            MA0004row("OTHRTIRE1") = SQLdr("OTHRTIRE1")
                            MA0004row("OTHRTIRE2") = SQLdr("OTHRTIRE2")
                            MA0004row("OTHRTPMS") = SQLdr("OTHRTPMS")
                            MA0004row("OTNKTMAKER") = SQLdr("OTNKTMAKER")
                            MA0004row("HPRSTMAKER") = SQLdr("HPRSTMAKER")
                            MA0004row("CHEMTMAKER") = SQLdr("CHEMTMAKER")
                            MA0004row("CONTTMAKER") = SQLdr("CONTTMAKER")

                            MA0004row("SHARYOSTATUS") = SQLdr("SHARYOSTATUS")
                            MA0004row("SHARYOINFO1") = SQLdr("SHARYOINFO1")
                            MA0004row("SHARYOINFO2") = SQLdr("SHARYOINFO2")
                            MA0004row("SHARYOINFO3") = SQLdr("SHARYOINFO3")
                            MA0004row("SHARYOINFO4") = SQLdr("SHARYOINFO4")
                            MA0004row("SHARYOINFO5") = SQLdr("SHARYOINFO5")
                            MA0004row("SHARYOINFO6") = SQLdr("SHARYOINFO6")

                            '統一車番＋S開始年月日がブレイク
                            If MA0004row("SHARYOTYPE") = WW_SHARYOTYPE AndAlso
                                MA0004row("TSHABAN") = WW_TSHABAN AndAlso
                                MA0004row("STYMD_C") = WW_STYMD_C Then
                                MA0004row("SELECT") = 0
                            Else
                                MA0004row("SELECT") = 1
                                MA0004row("HIDDEN") = 0   '0:表示
                                '前回キー保存
                                WW_SHARYOTYPE = MA0004row("SHARYOTYPE")
                                WW_TSHABAN = MA0004row("TSHABAN")
                                WW_STYMD_C = MA0004row("STYMD_C")
                            End If

                            '○条件画面で指定に該当するデータを抽出
                            If MA0004row("SELECT") = 1 Then

                                '条件画面で指定された設置部署を抽出
                                Dim WW_SELECT_SORG As Integer = 0    '0:対象外、1:対象
                                If WF_SORG_CREATE.Text = "" Then
                                    WW_SELECT_SORG = 1
                                Else
                                    If WF_SORG_CREATE.Text = MA0004row("MANGSORG") Then
                                        WW_SELECT_SORG = 1
                                    End If
                                End If
                                If WW_SELECT_SORG = 0 Then
                                    MA0004row("SELECT") = 0
                                End If

                                If MA0004row("SELECT") = 1 Then
                                    MA0004tbl.Rows.Add(MA0004row)
                                End If

                            End If

                        End While
                    End Using
                End Using
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "MA002_SHARYOA SELECT")
            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:MA002_SHARYOA Select"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try

    End Sub

    ''' <summary>
    ''' 車両申請マスタ登録処理
    ''' </summary>
    ''' <remarks>車両申請マスタ（MA004_SHARYOC）を登録する</remarks>
    Protected Function DATAinsert() As Integer

        Dim WW_DATENOW As DateTime = Date.Now
        Dim cnt As Integer = 0
        Dim WW_str As String
        Dim WW_STYMD As Date
        Dim WW_ENDYMD As Date
        Dim WW_int As Integer


        Try

            Using SQLcon As SqlConnection = CS0050Session.getConnection
                SQLcon.Open()       'DataBase接続(Open)

                Dim SQLStr As String =
                          "    INSERT INTO MA004_SHARYOC " _
                        & "             (CAMPCODE ,      " _
                        & "              SHARYOTYPE ,    " _
                        & "              TSHABAN ,       " _
                        & "              STYMD ,         " _
                        & "              ENDYMD ,        " _
                        & "              LICNPLTNO1 ,    " _
                        & "              LICNPLTNO2 ,    " _
                        & "              LICNMNFACT ,    " _
                        & "              LICNFRAMENO ,   " _
                        & "              LICNMODEL ,     " _
                        & "              LICNMOTOR ,     " _
                        & "              LICNLDCAPA ,    " _
                        & "              LICN5LDCAPA ,   " _
                        & "              LICNWEIGHT ,    " _
                        & "              LICNTWEIGHT ,   " _
                        & "              LICNCWEIGHT ,   " _
                        & "              LICNYMD ,       " _
                        & "              TAXLINSYMD ,    " _
                        & "              TAXLINS ,       " _
                        & "              TAXVTAX ,       " _
                        & "              TAXATAX ,       " _
                        & "              OTNKTINSYMD ,   " _
                        & "              OTNKTINSNYMD ,  " _
                        & "              HPRSJINSYMD ,   " _
                        & "              HPRSINSYMD ,    " _
                        & "              HPRSINSNYMD ,   " _
                        & "              CHEMTINSYMD ,   " _
                        & "              CHEMTINSNYMD ,  " _
                        & "              INSKBN ,        " _
                        & "              DELFLG ,        " _
                        & "              INITYMD ,       " _
                        & "              UPDYMD ,        " _
                        & "              UPDUSER ,       " _
                        & "              UPDTERMID ,     " _
                        & "              RECEIVEYMD )    " _
                        & "      VALUES (@P01,@P02,@P03,@P04,@P05,@P06,@P07,@P08,@P09,@P10," _
                        & "              @P11,@P12,@P13,@P14,@P15,@P16,@P17,@P18,@P19,@P20," _
                        & "              @P21,@P22,@P23,@P24,@P25,@P26,@P27,@P28,@P29,@P30," _
                        & "              @P31,@P32,@P33,@P34,@P35);"

                Dim SQLStr2 As String =
                          "      SELECT TOP 1            " _
                        & "              CAMPCODE ,      " _
                        & "              SHARYOTYPE ,    " _
                        & "              TSHABAN ,       " _
                        & "              STYMD ,         " _
                        & "              ENDYMD ,        " _
                        & "              LICNPLTNO1 ,    " _
                        & "              LICNPLTNO2 ,    " _
                        & "              LICNMNFACT ,    " _
                        & "              LICNFRAMENO ,   " _
                        & "              LICNMODEL ,     " _
                        & "              LICNMOTOR ,     " _
                        & "              LICNLDCAPA ,    " _
                        & "              LICN5LDCAPA ,   " _
                        & "              LICNWEIGHT ,    " _
                        & "              LICNTWEIGHT ,   " _
                        & "              LICNCWEIGHT ,   " _
                        & "              LICNYMD ,       " _
                        & "              TAXLINSYMD ,    " _
                        & "              TAXLINS ,       " _
                        & "              TAXVTAX ,       " _
                        & "              TAXATAX ,       " _
                        & "              OTNKTINSYMD ,   " _
                        & "              OTNKTINSNYMD ,  " _
                        & "              HPRSJINSYMD ,   " _
                        & "              HPRSINSYMD ,    " _
                        & "              HPRSINSNYMD ,   " _
                        & "              CHEMTINSYMD ,   " _
                        & "              CHEMTINSNYMD ,  " _
                        & "              INSKBN ,        " _
                        & "              DELFLG          " _
                        & "     FROM MA004_SHARYOC       " _
                        & "     WHERE    CAMPCODE       = @P01 " _
                        & "       and    SHARYOTYPE     = @P02 " _
                        & "       and    TSHABAN        = @P03 " _
                        & "       order by STYMD DESC ; "

                Using SQLcmd As New SqlCommand(SQLStr, SQLcon), SQLcmd2 As New SqlCommand(SQLStr2, SQLcon)
                    Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", SqlDbType.NVarChar, 20)
                    Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", SqlDbType.NVarChar, 1)
                    Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@P03", SqlDbType.NVarChar, 19)
                    Dim PARA04 As SqlParameter = SQLcmd.Parameters.Add("@P04", SqlDbType.Date)
                    Dim PARA05 As SqlParameter = SQLcmd.Parameters.Add("@P05", SqlDbType.Date)
                    Dim PARA06 As SqlParameter = SQLcmd.Parameters.Add("@P06", SqlDbType.NVarChar, 5)
                    Dim PARA07 As SqlParameter = SQLcmd.Parameters.Add("@P07", SqlDbType.NVarChar, 15)
                    Dim PARA08 As SqlParameter = SQLcmd.Parameters.Add("@P08", SqlDbType.NVarChar, 20)
                    Dim PARA09 As SqlParameter = SQLcmd.Parameters.Add("@P09", SqlDbType.NVarChar, 20)
                    Dim PARA10 As SqlParameter = SQLcmd.Parameters.Add("@P10", SqlDbType.NVarChar, 20)
                    Dim PARA11 As SqlParameter = SQLcmd.Parameters.Add("@P11", SqlDbType.NVarChar, 20)
                    Dim PARA12 As SqlParameter = SQLcmd.Parameters.Add("@P12", SqlDbType.BigInt)
                    Dim PARA13 As SqlParameter = SQLcmd.Parameters.Add("@P13", SqlDbType.BigInt)
                    Dim PARA14 As SqlParameter = SQLcmd.Parameters.Add("@P14", SqlDbType.BigInt)
                    Dim PARA15 As SqlParameter = SQLcmd.Parameters.Add("@P15", SqlDbType.BigInt)
                    Dim PARA16 As SqlParameter = SQLcmd.Parameters.Add("@P16", SqlDbType.BigInt)
                    Dim PARA17 As SqlParameter = SQLcmd.Parameters.Add("@P17", SqlDbType.DateTime)
                    Dim PARA18 As SqlParameter = SQLcmd.Parameters.Add("@P18", SqlDbType.DateTime)
                    Dim PARA19 As SqlParameter = SQLcmd.Parameters.Add("@P19", SqlDbType.Money)
                    Dim PARA20 As SqlParameter = SQLcmd.Parameters.Add("@P20", SqlDbType.Money)
                    Dim PARA21 As SqlParameter = SQLcmd.Parameters.Add("@P21", SqlDbType.Money)
                    Dim PARA22 As SqlParameter = SQLcmd.Parameters.Add("@P22", SqlDbType.Date)
                    Dim PARA23 As SqlParameter = SQLcmd.Parameters.Add("@P23", SqlDbType.Date)
                    Dim PARA24 As SqlParameter = SQLcmd.Parameters.Add("@P24", SqlDbType.Date)
                    Dim PARA25 As SqlParameter = SQLcmd.Parameters.Add("@P25", SqlDbType.Date)
                    Dim PARA26 As SqlParameter = SQLcmd.Parameters.Add("@P26", SqlDbType.Date)
                    Dim PARA27 As SqlParameter = SQLcmd.Parameters.Add("@P27", SqlDbType.Date)
                    Dim PARA28 As SqlParameter = SQLcmd.Parameters.Add("@P28", SqlDbType.Date)
                    Dim PARA29 As SqlParameter = SQLcmd.Parameters.Add("@P29", SqlDbType.NVarChar, 1)
                    Dim PARA30 As SqlParameter = SQLcmd.Parameters.Add("@P30", SqlDbType.NVarChar, 1)
                    Dim PARA31 As SqlParameter = SQLcmd.Parameters.Add("@P31", SqlDbType.SmallDateTime)
                    Dim PARA32 As SqlParameter = SQLcmd.Parameters.Add("@P32", SqlDbType.DateTime)
                    Dim PARA33 As SqlParameter = SQLcmd.Parameters.Add("@P33", SqlDbType.NVarChar, 20)
                    Dim PARA34 As SqlParameter = SQLcmd.Parameters.Add("@P34", SqlDbType.NVarChar, 30)
                    Dim PARA35 As SqlParameter = SQLcmd.Parameters.Add("@P35", SqlDbType.DateTime)

                    Dim PARA1 As SqlParameter = SQLcmd2.Parameters.Add("@P01", SqlDbType.NVarChar, 20)
                    Dim PARA2 As SqlParameter = SQLcmd2.Parameters.Add("@P02", SqlDbType.NVarChar, 1)
                    Dim PARA3 As SqlParameter = SQLcmd2.Parameters.Add("@P03", SqlDbType.NVarChar, 19)

                    For Each MA0004row As DataRow In MA0004tbl.Rows

                        '○ＤＢ更新

                        '日付が入っているデータは対象外
                        If Not (String.IsNullOrEmpty(MA0004row("STYMD")) AndAlso String.IsNullOrEmpty(MA0004row("ENDYMD"))) Then
                            Continue For
                        End If

                        '削除は対象外
                        If MA0004row("DELFLG") = C_DELETE_FLG.DELETE AndAlso MA0004row("TIMSTP") = "0" Then
                            Continue For
                        End If

                        PARA1.Value = MA0004row("CAMPCODE")
                        PARA2.Value = MA0004row("SHARYOTYPE")
                        PARA3.Value = MA0004row("TSHABAN")

                        Using SQLdr As SqlDataReader = SQLcmd2.ExecuteReader()

                            'MA0004tbl値設定
                            If SQLdr.Read Then

                                '○有効期限(開始)
                                WW_str = WF_NENDO_CREATE.Text & "/4/1"
                                Try
                                    Date.TryParse(WW_str, WW_STYMD)
                                Catch ex As Exception
                                    Continue For
                                End Try

                                '○有効期限(終了)
                                Try
                                    Integer.TryParse(WF_NENDO_CREATE.Text, WW_int)
                                    WW_str = (WW_int + 1).ToString() & "/3/31"

                                    Date.TryParse(WW_str, WW_ENDYMD)
                                Catch ex As Exception
                                    Continue For
                                End Try

                                PARA01.Value = SQLdr("CAMPCODE")
                                PARA02.Value = SQLdr("SHARYOTYPE")
                                PARA03.Value = SQLdr("TSHABAN")
                                PARA04.Value = RTrim(WW_STYMD)
                                PARA05.Value = RTrim(WW_ENDYMD)
                                PARA06.Value = SQLdr("LICNPLTNO1")
                                PARA07.Value = SQLdr("LICNPLTNO2")
                                PARA08.Value = SQLdr("LICNMNFACT")
                                PARA09.Value = SQLdr("LICNFRAMENO")
                                PARA10.Value = SQLdr("LICNMODEL")
                                PARA11.Value = SQLdr("LICNMOTOR")
                                PARA12.Value = SQLdr("LICNLDCAPA")
                                PARA13.Value = SQLdr("LICN5LDCAPA")
                                PARA14.Value = SQLdr("LICNWEIGHT")
                                PARA15.Value = SQLdr("LICNTWEIGHT")
                                PARA16.Value = SQLdr("LICNCWEIGHT")
                                If RTrim(SQLdr("LICNYMD")) = "" Then
                                    PARA17.Value = C_DEFAULT_YMD
                                Else
                                    PARA17.Value = SQLdr("LICNYMD")
                                End If
                                If RTrim(SQLdr("TAXLINSYMD")) = "" Then
                                    PARA18.Value = C_DEFAULT_YMD
                                Else
                                    PARA18.Value = SQLdr("TAXLINSYMD")
                                End If
                                PARA19.Value = SQLdr("TAXLINS")
                                PARA20.Value = SQLdr("TAXVTAX")
                                PARA21.Value = SQLdr("TAXATAX")
                                If RTrim(SQLdr("OTNKTINSYMD")) = "" Then
                                    PARA22.Value = C_DEFAULT_YMD
                                Else
                                    PARA22.Value = SQLdr("OTNKTINSYMD")
                                End If
                                If RTrim(SQLdr("OTNKTINSNYMD")) = "" Then
                                    PARA23.Value = C_DEFAULT_YMD
                                Else
                                    PARA23.Value = SQLdr("OTNKTINSNYMD")
                                End If
                                If RTrim(SQLdr("HPRSJINSYMD")) = "" Then
                                    PARA24.Value = C_DEFAULT_YMD
                                Else
                                    PARA24.Value = SQLdr("HPRSJINSYMD")
                                End If
                                If RTrim(SQLdr("HPRSINSYMD")) = "" Then
                                    PARA25.Value = C_DEFAULT_YMD
                                Else
                                    PARA25.Value = SQLdr("HPRSINSYMD")
                                End If
                                If RTrim(SQLdr("HPRSINSNYMD")) = "" Then
                                    PARA26.Value = C_DEFAULT_YMD
                                Else
                                    PARA26.Value = SQLdr("HPRSINSNYMD")
                                End If
                                If RTrim(SQLdr("CHEMTINSYMD")) = "" Then
                                    PARA27.Value = C_DEFAULT_YMD
                                Else
                                    PARA27.Value = SQLdr("CHEMTINSYMD")
                                End If
                                If RTrim(SQLdr("CHEMTINSNYMD")) = "" Then
                                    PARA28.Value = C_DEFAULT_YMD
                                Else
                                    PARA28.Value = SQLdr("CHEMTINSNYMD")
                                End If
                                PARA29.Value = SQLdr("INSKBN")
                                PARA30.Value = SQLdr("DELFLG")
                                PARA31.Value = WW_DATENOW
                                PARA32.Value = WW_DATENOW
                                PARA33.Value = Master.USERID
                                PARA34.Value = Master.USERTERMID
                                PARA35.Value = C_DEFAULT_YMD

                            Else
                                Continue For
                            End If
                        End Using

                        SQLcmd.ExecuteNonQuery()

                        cnt += 1

                    Next
                End Using
            End Using
        Catch ex As Exception
            'Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "MA004_SHARYOC UPDATE_INSERT")
            CS0011LOGWRITE.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:MA004_SHARYOC UPDATE_INSERT"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
        End Try

        Return cnt

    End Function

End Class
