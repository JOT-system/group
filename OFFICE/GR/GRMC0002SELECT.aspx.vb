﻿Imports OFFICE.GRIS0005LeftBox
Imports BASEDLL

''' <summary>
''' 取引先マスタ入力（条件）
''' </summary>
''' <remarks></remarks>
Public Class GRMC0002SELECT
    Inherits Page

    '共通処理結果
    Private WW_ERR_SW As String
    Private WW_RTN_SW As String
    Private WW_DUMMY As String

    ''' <summary>
    ''' サーバ処理の遷移先
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs) Handles Me.Load

        If IsPostBack Then
            '○各ボタン押下処理
            If Not String.IsNullOrEmpty(WF_ButtonClick.Value) Then
                Select Case WF_ButtonClick.Value
                    Case "WF_ButtonDO"
                        WF_ButtonDO_Click()
                    Case "WF_ButtonEND"
                        WF_ButtonEND_Click()
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
            '○ 初期化処理
            Initialize()
        End If

    End Sub

    ''' <summary>
    ''' 初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub Initialize()

        '画面ID設定
        Master.MAPID = GRMC0002WRKINC.MAPIDS

        '○初期値設定
        WF_STYMD.Focus()
        WF_FIELD.Value = ""
        WF_LeftMViewChange.Value = ""
        leftview.activeListBox()

        '○ 画面の値設定
        WW_MAPValueSet()

    End Sub


    ''' <summary>
    ''' 終了ボタン処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonEND_Click()

        Master.transitionPrevPage()

    End Sub

    ''' <summary>
    ''' 実行ボタン処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonDO_Click()

        '○入力文字置き換え(使用禁止文字排除)
        Master.eraseCharToIgnore(WF_CAMPCODE.Text)          '会社コード
        Master.eraseCharToIgnore(WF_STYMD.Text)             '有効年月日(From)
        Master.eraseCharToIgnore(WF_ENDYMD.Text)            '有効年月日(To)
        Master.eraseCharToIgnore(WF_TORICODEF.Text)         '取引先(From)
        Master.eraseCharToIgnore(WF_TORICODET.Text)         '取引先(To)
        Master.eraseCharToIgnore(WF_TORINAME.Text)          '取引先名称
        Master.eraseCharToIgnore(WF_POSTNUM.Text)           '郵便番号
        Master.eraseCharToIgnore(WF_ADDR.Text)              '住所
        Master.eraseCharToIgnore(WF_TEL.Text)               '電話番号
        Master.eraseCharToIgnore(WF_FAX.Text)               'FAX番号

        '○チェック処理
        WW_Check(WW_ERR_SW)
        If WW_ERR_SW = "ERR" Then
            Exit Sub
        End If

        '○セッション変数　反映
        work.WF_SEL_CAMPCODE.Text = WF_CAMPCODE.Text            '会社コード
        work.WF_SEL_STYMD.Text = WF_STYMD.Text                  '有効年月日　
        If WF_ENDYMD.Text = "" Then
            work.WF_SEL_ENDYMD.Text = WF_STYMD.Text
        Else
            work.WF_SEL_ENDYMD.Text = WF_ENDYMD.Text
        End If
        work.WF_SEL_TORICODEF.Text = WF_TORICODEF.Text          '取引先(From)
        work.WF_SEL_TORICODET.Text = WF_TORICODET.Text          '取引先(To)
        work.WF_SEL_TORINAME.Text = WF_TORINAME.Text            '取引先名称
        work.WF_SEL_POSTNUM.Text = WF_POSTNUM.Text              '郵便番号
        work.WF_SEL_ADDR.Text = WF_ADDR.Text                    '住所
        work.WF_SEL_TEL.Text = WF_TEL.Text                      '電話番号
        work.WF_SEL_FAX.Text = WF_FAX.Text                      'FAX番号
        Master.VIEWID = rightview.getViewId(WF_CAMPCODE.Text)

        Master.checkParmissionCode(WF_CAMPCODE.Text)
        If Not Master.MAPpermitcode = C_PERMISSION.INVALID Then
            '○画面遷移先URL取得
            Master.transitionPage()
        End If

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
                    Dim prmData As New Hashtable

                    If WF_LeftMViewChange.Value = LIST_BOX_CLASSIFICATION.LC_CUSTOMER Then
                        prmData = work.createTORIParam(WF_CAMPCODE.Text)
                    Else
                        prmData.Item(C_PARAMETERS.LP_COMPANY) = WF_CAMPCODE.Text
                    End If
                    .setListBox(WF_LeftMViewChange.Value, WW_DUMMY, prmData)
                    .activeListBox()
                Else
                    '日付の場合、入力日付のカレンダーが表示されるように入力値をカレンダーに渡す
                    Select Case WF_FIELD.Value
                        Case "WF_STYMD"
                            .WF_Calendar.Text = WF_STYMD.Text
                        Case "WF_ENDYMD"
                            .WF_Calendar.Text = WF_ENDYMD.Text
                    End Select
                    .activeCalendar()
                End If
            End With
        End If

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
    ''' 右リストボックスダブルクリック処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_RIGHTBOX_DBClick()
        rightview.initViewID(WF_CAMPCODE.Text, WW_DUMMY)
    End Sub
    ''' <summary>
    ''' 右リストボックスMEMO欄更新
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_RIGHTBOX_Change()
        '○右Boxメモ変更時処理
        rightview.save(Master.USERID, Master.USERTERMID, WW_DUMMY)
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

            Case "WF_STYMD"             '有効年月日(From)
                Dim WW_DATE As Date
                Try
                    Date.TryParse(leftview.WF_Calendar.Text, WW_DATE)
                    If WW_DATE < C_DEFAULT_YMD Then
                        WF_STYMD.Text = ""
                    Else
                        WF_STYMD.Text = leftview.WF_Calendar.Text
                    End If
                Catch ex As Exception
                End Try
                WF_STYMD.Focus()

            Case "WF_ENDYMD"            '有効年月日(To)
                Dim WW_DATE As Date
                Try
                    Date.TryParse(leftview.WF_Calendar.Text, WW_DATE)
                    If WW_DATE < C_DEFAULT_YMD Then
                        WF_ENDYMD.Text = ""
                    Else
                        WF_ENDYMD.Text = leftview.WF_Calendar.Text
                    End If
                Catch ex As Exception

                End Try
                WF_ENDYMD.Focus()

            Case "WF_TORICODEF"         '取引先(From)
                WF_TORICODEF_Text.Text = WW_SelectTEXT
                WF_TORICODEF.Text = WW_SelectValue
                WF_TORICODEF.Focus()

            Case "WF_TORICODET"         '取引先(To)
                WF_TORICODET_Text.Text = WW_SelectTEXT
                WF_TORICODET.Text = WW_SelectValue
                WF_TORICODET.Focus()
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

        '○フォーカスセット
        Select Case WF_FIELD.Value
            Case "WF_CAMPCODE"          '会社コード
                WF_CAMPCODE.Focus()
            Case "WF_STYMD"             '有効年月日(From)
                WF_STYMD.Focus()
            Case "WF_ENDYMD"            '有効年月日(To)
                WF_ENDYMD.Focus()
            Case "WF_TORICODEF"         '取引先(From)
                WF_TORICODEF.Focus()
            Case "WF_TORICODET"         '取引先(To)
                WF_TORICODET.Focus()
        End Select

        '○画面左サイドボックス非表示は、画面JavaScript(InitLoad)で実行
        WF_FIELD.Value = ""
        WF_LeftboxOpen.Value = ""
        WF_LeftMViewChange.Value = ""

    End Sub

    ''' <summary>
    ''' TextBox変更時LeftBox設定処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_LeftBoxReSet()

        WF_CAMPCODE_Text.Text = String.Empty        '会社
        WF_TORICODEF_Text.Text = String.Empty       '取引先(From)
        WF_TORICODET_Text.Text = String.Empty       '取引先(To)

        '○入力文字置き換え(使用禁止文字排除)
        Master.eraseCharToIgnore(WF_CAMPCODE.Text)          '会社コード
        Master.eraseCharToIgnore(WF_STYMD.Text)             '有効年月日(From)
        Master.eraseCharToIgnore(WF_ENDYMD.Text)            '有効年月日(To)
        Master.eraseCharToIgnore(WF_TORICODEF.Text)         '取引先(From)
        Master.eraseCharToIgnore(WF_TORICODET.Text)         '取引先(To)
        Master.eraseCharToIgnore(WF_TORINAME.Text)          '取引先名称
        Master.eraseCharToIgnore(WF_POSTNUM.Text)           '郵便番号
        Master.eraseCharToIgnore(WF_ADDR.Text)              '住所
        Master.eraseCharToIgnore(WF_TEL.Text)               '電話番号
        Master.eraseCharToIgnore(WF_FAX.Text)               'FAX番号

        '○チェック処理
        WW_Check(WW_ERR_SW)

        '○名称設定
        leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_COMPANY, WF_CAMPCODE.Text, WF_CAMPCODE_Text.Text, WW_DUMMY)                                                  '会社コード
        leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_CUSTOMER, WF_TORICODEF.Text, WF_TORICODEF_Text.Text, WW_DUMMY, work.createTORIParam(WF_CAMPCODE.Text))       '取引先(From)
        leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_CUSTOMER, WF_TORICODET.Text, WF_TORICODET_Text.Text, WW_DUMMY, work.createTORIParam(WF_CAMPCODE.Text))       '取引先(To)

    End Sub

    ' ******************************************************************************
    ' ***  共通処理                                                              ***
    ' ******************************************************************************

    ''' <summary>
    ''' 画面初期値設定処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_MAPValueSet()

        If Context.Handler.ToString().ToUpper = C_PREV_MAP_LIST.MENU Then               'メニューからの画面遷移
            '○初期変数設定処理
            Master.getFirstValue(work.WF_SEL_CAMPCODE.Text, "CAMPCODE", WF_CAMPCODE.Text)           '会社コード
            Master.getFirstValue(work.WF_SEL_CAMPCODE.Text, "STYMD", WF_STYMD.Text)                 '有効年月日(From)
            Master.getFirstValue(work.WF_SEL_CAMPCODE.Text, "ENDYMD", WF_ENDYMD.Text)               '有効年月日(To)
            Master.getFirstValue(work.WF_SEL_CAMPCODE.Text, "TORICODEF", WF_TORICODEF.Text)         '取引先(From)
            Master.getFirstValue(work.WF_SEL_CAMPCODE.Text, "TORICODET", WF_TORICODET.Text)         '取引先(To)
            Master.getFirstValue(work.WF_SEL_CAMPCODE.Text, "TORINAME", WF_TORINAME.Text)           '取引先名称
            Master.getFirstValue(work.WF_SEL_CAMPCODE.Text, "POSTNUM", WF_POSTNUM.Text)             '郵便番号
            Master.getFirstValue(work.WF_SEL_CAMPCODE.Text, "ADDR", WF_ADDR.Text)                   '住所
            Master.getFirstValue(work.WF_SEL_CAMPCODE.Text, "TEL", WF_TEL.Text)                     '電話番号
            Master.getFirstValue(work.WF_SEL_CAMPCODE.Text, "FAX", WF_FAX.Text)                     'FAX番号
        ElseIf Context.Handler.ToString().ToUpper = C_PREV_MAP_LIST.MC0002 Then         '実行画面からの画面遷移
            '○画面項目設定処理
            WF_CAMPCODE.Text = work.WF_SEL_CAMPCODE.Text            '会社コード
            WF_STYMD.Text = work.WF_SEL_STYMD.Text                  '有効年月日(From)
            WF_ENDYMD.Text = work.WF_SEL_ENDYMD.Text                '有効年月日(To)
            WF_TORICODEF.Text = work.WF_SEL_TORICODEF.Text          '取引先(From)
            WF_TORICODET.Text = work.WF_SEL_TORICODET.Text          '取引先(To)
            WF_TORINAME.Text = work.WF_SEL_TORINAME.Text            '取引先名称
            WF_POSTNUM.Text = work.WF_SEL_POSTNUM.Text              '郵便番号
            WF_ADDR.Text = work.WF_SEL_ADDR.Text                    '住所
            WF_TEL.Text = work.WF_SEL_TEL.Text                      '電話番号
            WF_FAX.Text = work.WF_SEL_FAX.Text                      'FAX番号
        End If

        '○ RightBox情報設定
        rightview.MAPID = GRMC0002WRKINC.MAPID
        rightview.MAPIDS = GRMC0002WRKINC.MAPIDS
        rightview.COMPCODE = WF_CAMPCODE.Text
        rightview.MAPVARI = Master.MAPvariant
        rightview.PROFID = Master.PROF_VIEW
        rightview.Initialize("取引先入力", WW_ERR_SW)
        If Not isNormal(WW_ERR_SW) Then
            Exit Sub
        End If

        '○名称設定処理
        leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_COMPANY, WF_CAMPCODE.Text, WF_CAMPCODE_Text.Text, WW_DUMMY)                                                  '会社コード
        leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_CUSTOMER, WF_TORICODEF.Text, WF_TORICODEF_Text.Text, WW_DUMMY, work.createTORIParam(WF_CAMPCODE.Text))       '取引先(From)
        leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_CUSTOMER, WF_TORICODET.Text, WF_TORICODET_Text.Text, WW_DUMMY, work.createTORIParam(WF_CAMPCODE.Text))       '取引先(To)

    End Sub

    ''' <summary>
    ''' チェック処理
    ''' </summary>
    ''' <param name="O_RTN">成否判定</param>
    ''' <remarks></remarks>
    Protected Sub WW_Check(ByRef O_RTN As String)

        '○初期設定
        O_RTN = ""
        Dim WW_TEXT As String = ""
        Dim WW_CS0024FCHECKERR As String = ""
        Dim WW_CS0024FCHECKREPORT As String = ""
        WF_FIELD.Value = ""
        Dim WW_STYMD As Date
        Dim WW_ENDYMD As Date

        '○会社コード
        WW_TEXT = WF_CAMPCODE.Text
        Master.checkFIeld(WF_CAMPCODE.Text, "CAMPCODE", WF_CAMPCODE.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            '存在チェック
            If WW_TEXT = "" Then
                WF_CAMPCODE.Text = ""
            Else
                leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_COMPANY, WF_CAMPCODE.Text, WF_CAMPCODE_Text.Text, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    Master.output(C_MESSAGE_NO.INVALID_SELECTION_DATA, C_MESSAGE_TYPE.ERR)
                    WF_CAMPCODE.Focus()
                    O_RTN = "ERR"
                    Exit Sub
                End If
            End If
        Else
            Master.output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR)
            WF_CAMPCODE.Focus()
            O_RTN = "ERR"
            Exit Sub
        End If

        '○年度 WF_STYMD.Text
        Master.checkFIeld(WF_CAMPCODE.Text, "STYMD", WF_STYMD.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            Try
                Date.TryParse(WF_STYMD.Text, WW_STYMD)
            Catch ex As Exception
                WW_STYMD = C_DEFAULT_YMD
            End Try
        Else
            Master.output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR)
            WF_STYMD.Focus()
            O_RTN = "ERR"
            Exit Sub
        End If

        '○年度 WF_ENDYMD.Text
        If WF_ENDYMD.Text = Nothing Then
            WF_ENDYMD.Text = WF_STYMD.Text
        End If

        Master.checkFIeld(WF_CAMPCODE.Text, "ENDYMD", WF_ENDYMD.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            Try
                Date.TryParse(WF_ENDYMD.Text, WW_ENDYMD)
            Catch ex As Exception
                WW_ENDYMD = C_MAX_YMD
            End Try
        Else
            Master.output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR)
            WF_ENDYMD.Focus()
            O_RTN = "ERR"
            Exit Sub
        End If

        '関連チェック(開始＞終了)
        If WF_STYMD.Text <> "" AndAlso WF_ENDYMD.Text <> "" Then
            If WW_STYMD > WW_ENDYMD Then
                Master.output(C_MESSAGE_NO.START_END_DATE_RELATION_ERROR, C_MESSAGE_TYPE.ERR)
                WF_STYMD.Focus()
                O_RTN = "ERR"
                Exit Sub
            End If
        End If

        '○取引先(From)
        WW_TEXT = WF_TORICODEF.Text
        Master.checkFIeld(WF_CAMPCODE.Text, "TORICODEF", WF_TORICODEF.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            If WW_TEXT = "" Then
                WF_TORICODEF.Text = ""
            Else
                leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_CUSTOMER, WF_TORICODEF.Text, WF_TORICODEF_Text.Text, WW_RTN_SW, work.createTORIParam(WF_CAMPCODE.Text))
                If Not isNormal(WW_RTN_SW) Then
                    If WW_RTN_SW = "ERR" Then
                        Master.output(C_MESSAGE_NO.INVALID_SELECTION_DATA, C_MESSAGE_TYPE.ERR)
                        WF_TORICODEF.Focus()
                        O_RTN = "ERR"
                        Exit Sub
                    End If
                End If
            End If
        Else
            Master.output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR)
            WF_TORICODEF.Focus()
            O_RTN = "ERR"
            Exit Sub
        End If

        '○取引先(To)
        WW_TEXT = WF_TORICODET.Text
        Master.checkFIeld(WF_CAMPCODE.Text, "TORICODET", WF_TORICODET.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            '存在チェック(LeftBoxチェック)
            If WW_TEXT = "" Then
                WF_TORICODET.Text = ""
            Else
                leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_CUSTOMER, WF_TORICODET.Text, WF_TORICODET_Text.Text, WW_RTN_SW, work.createTORIParam(WF_CAMPCODE.Text))
                If Not isNormal(WW_RTN_SW) Then
                    Master.output(C_MESSAGE_NO.INVALID_SELECTION_DATA, C_MESSAGE_TYPE.ERR)
                    WF_TORICODET.Focus()
                    O_RTN = "ERR"
                    Exit Sub
                End If
            End If
        Else
            Master.output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR)
            WF_TORICODET.Focus()
            O_RTN = "ERR"
            Exit Sub
        End If

        '関連チェック(開始＞終了)
        If WF_TORICODEF.Text <> "" AndAlso WF_TORICODET.Text <> "" AndAlso WF_TORICODEF.Text > WF_TORICODET.Text Then
            Master.output(C_MESSAGE_NO.START_END_RELATION_ERROR, C_MESSAGE_TYPE.ERR)
            WF_TORICODEF.Focus()
            O_RTN = "ERR"
            Exit Sub
        End If

        '○取引先名称 WF_TORINAME.Text
        Master.checkFIeld(WF_CAMPCODE.Text, "TORINAME", WF_TORINAME.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_CUSTOMER, WF_TORINAME.Text, WF_TORINAME.Text, WW_RTN_SW, work.createTORIParam(WF_CAMPCODE.Text))
        If Not isNormal(WW_CS0024FCHECKERR) Then
            Master.output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR)
            WF_TORINAME.Focus()
            O_RTN = "ERR"
            Exit Sub
        End If

        '○郵便番号 WF_POSTNUM.Text
        Master.checkFIeld(WF_CAMPCODE.Text, "POSTNUM", WF_POSTNUM.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If Not isNormal(WW_CS0024FCHECKERR) Then
            Master.output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR)
            WF_POSTNUM.Focus()
            O_RTN = "ERR"
            Exit Sub
        End If

        '○住所 WF_ADDR.Text
        Master.checkFIeld(WF_CAMPCODE.Text, "ADDR", WF_ADDR.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If Not isNormal(WW_CS0024FCHECKERR) Then
            Master.output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR)
            WF_ADDR.Focus()
            O_RTN = "ERR"
            Exit Sub
        End If

        '○電話番号 WF_TEL.Text
        Master.checkFIeld(WF_CAMPCODE.Text, "TEL", WF_TEL.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If Not isNormal(WW_CS0024FCHECKERR) Then
            Master.output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR)
            WF_TEL.Focus()
            O_RTN = "ERR"
            Exit Sub
        End If

        '○FAX番号 WF_FAX.Text
        Master.checkFIeld(WF_CAMPCODE.Text, "FAX", WF_FAX.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If Not isNormal(WW_CS0024FCHECKERR) Then
            Master.output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR)
            WF_FAX.Focus()
            O_RTN = "ERR"
            Exit Sub
        End If

        '正常メッセージ
        Master.output(C_MESSAGE_NO.NORMAL, C_MESSAGE_TYPE.NOR)

    End Sub

End Class
