﻿Imports System.IO
Imports OFFICE.GRIS0005LeftBox
Imports BASEDLL

''' <summary>
''' 事務員勤務入力（条件）
''' </summary>
''' <remarks></remarks>
Public Class GRT00014SELECT
    Inherits Page

    '○ 共通処理結果
    Private WW_ERR_SW As String
    Private WW_RTN_SW As String
    Private WW_DUMMY As String

    '○ 共通関数宣言(BASEDLL)
    Private CS0050SESSION As New CS0050SESSION          'セッション情報操作処理
    Private T0007COM As New GRT0007COM_V2               '勤怠共通

    ''' <summary>
    ''' サーバー処理の遷移先
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs) Handles Me.Load

        If IsPostBack Then
            '○ 各ボタン押下処理
            If Not String.IsNullOrEmpty(WF_ButtonClick.Value) Then
                Select Case WF_ButtonClick.Value
                    Case "WF_ButtonDO"                  '実行ボタン押下
                        WF_ButtonDO_Click()
                    Case "WF_ButtonEND"                 '終了ボタン押下
                        WF_ButtonEND_Click()
                    Case "WF_Field_DBClick"             'フィールドダブルクリック
                        WF_FIELD_DBClick()
                    Case "WF_LeftBoxSelectClick"        'フィールドチェンジ
                        WF_FIELD_Change()
                    Case "WF_ButtonSel"                 '(左ボックス)選択ボタン押下
                        WF_ButtonSel_Click()
                    Case "WF_ButtonCan"                 '(左ボックス)キャンセルボタン押下
                        WF_ButtonCan_Click()
                    Case "WF_ListboxDBclick"            '左ボックスダブルクリック
                        WF_ButtonSel_Click()
                    Case "WF_RIGHT_VIEW_DBClick"        '右ボックスダブルクリック
                        WF_RIGHTBOX_DBClick()
                    Case "WF_MEMOChange"                'メモ欄更新
                        WF_RIGHTBOX_Change()
                    Case "HELP"                         'ヘルプ表示
                        WF_HELP_Click()
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

        '○ 画面ID設定
        Master.MAPID = GRT00014WRKINC.MAPIDS

        WF_CAMPCODE.Focus()
        WF_FIELD.Value = ""
        WF_LeftboxOpen.Value = ""
        WF_LeftMViewChange.Value = ""
        WF_RightboxOpen.Value = ""
        leftview.ActiveListBox()

        '○ 画面の値設定
        WW_MAPValueSet()

    End Sub

    ''' <summary>
    ''' 画面初期値設定処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_MAPValueSet()

        If Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.MENU Then             'メニューからの画面遷移
            '画面間の情報クリア
            work.Initialize(Master.USERID)

            '初期変数設定処理
            Master.GetFirstValue(work.WF_SEL_CAMPCODE.Text, "CAMPCODE", WF_CAMPCODE.Text)           '会社コード

            '対象年月
            Master.GetFirstValue(work.WF_SEL_CAMPCODE.Text, "TAISHOYM", WF_TAISHOYM.Text)
            Dim WW_DATE As Date
            Try
                Date.TryParse(WF_TAISHOYM.Text, WW_DATE)
                WF_TAISHOYM.Text = WW_DATE.ToString("yyyy/MM")
            Catch ex As Exception
                WF_TAISHOYM.Text = Date.Now.ToString("yyyy/MM")
            End Try

            Master.GetFirstValue(work.WF_SEL_CAMPCODE.Text, "HORG", WF_HORG.Text)                   '配属部署
            Master.GetFirstValue(work.WF_SEL_CAMPCODE.Text, "STAFFKBN", WF_STAFFKBN.Text)           '職務区分
            Master.GetFirstValue(work.WF_SEL_CAMPCODE.Text, "STAFFCODE", WF_STAFFCODE.Text)         '従業員(コード)
            Master.GetFirstValue(work.WF_SEL_CAMPCODE.Text, "STAFFNAME", WF_STAFFNAMES.Text)        '従業員(名称)

        ElseIf Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.T00014 Then       '実行画面からの遷移
            '画面項目設定処理
            WF_CAMPCODE.Text = work.WF_SEL_CAMPCODE.Text            '会社コード
            WF_TAISHOYM.Text = work.WF_SEL_TAISHOYM.Text            '対象年月
            WF_HORG.Text = work.WF_SEL_HORG.Text                    '配属部署
            WF_STAFFKBN.Text = work.WF_SEL_STAFFKBN.Text            '職務区分
            WF_STAFFCODE.Text = work.WF_SEL_STAFFCODE.Text          '従業員(コード)
            WF_STAFFNAMES.Text = work.WF_SEL_STAFFNAMES.Text        '従業員(名称)
        End If

        '○ RightBox情報設定
        rightview.MAPIDS = GRT00014WRKINC.MAPIDS
        rightview.MAPID = GRT00014WRKINC.MAPID
        rightview.COMPCODE = WF_CAMPCODE.Text
        rightview.MAPVARI = Master.MAPvariant
        rightview.PROFID = Master.PROF_VIEW
        rightview.Initialize("画面レイアウト設定", WW_DUMMY)

        '○ 名称設定処理
        CODENAME_get("CAMPCODE", WF_CAMPCODE.Text, WF_CAMPCODE_TEXT.Text, WW_DUMMY)             '会社コード
        CODENAME_get("HORG", WF_HORG.Text, WF_HORG_TEXT.Text, WW_DUMMY)                         '配属部署
        CODENAME_get("STAFFKBN", WF_STAFFKBN.Text, WF_STAFFKBN_TEXT.Text, WW_DUMMY)             '職務区分
        CODENAME_get("STAFFCODE", WF_STAFFCODE.Text, WF_STAFFCODE_TEXT.Text, WW_DUMMY)          '従業員(コード)

    End Sub

    ''' <summary>
    ''' 実行ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonDO_Click()

        '○ 入力文字置き換え(使用禁止文字排除)
        Master.EraseCharToIgnore(WF_CAMPCODE.Text)          '会社コード
        Master.EraseCharToIgnore(WF_TAISHOYM.Text)          '対象年月
        Master.EraseCharToIgnore(WF_HORG.Text)              '配属部署
        Master.EraseCharToIgnore(WF_STAFFKBN.Text)          '職務区分
        Master.EraseCharToIgnore(WF_STAFFCODE.Text)         '従業員(コード)
        Master.EraseCharToIgnore(WF_STAFFNAMES.Text)        '従業員(名称)

        '○ チェック処理
        WW_Check(WW_ERR_SW)
        If WW_ERR_SW = "ERR" Then
            Exit Sub
        End If

        '○ 勤怠締テーブル取得（全部署チェックお行い、全て締っていればOKとする）
        Dim WW_LIMITFLG As String = "1"
        Dim prmData As New Hashtable
        prmData = work.CreateHORGParam(WF_CAMPCODE.Text, Master.USERID, Master.ROLE_ORG)
        Dim WW_LIST As ListBox = If(prmData.Item(C_PARAMETERS.LP_LIST), New ListBox)

        For i As Integer = 0 To WW_LIST.Items.Count - 1
            Dim WW_HORG As String = WW_LIST.Items(i).Value
            Dim WW_FLG As String = ""
            T0007COM.T00008get(WF_CAMPCODE.Text, WW_HORG, WF_TAISHOYM.Text, WW_FLG, WW_RTN_SW)
            If Not isNormal(WW_RTN_SW) Then
                Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "T0008_KINTAISTAT")
                Exit Sub
            End If
            If WW_FLG = "0" Then
                WW_LIMITFLG = WW_FLG
            Else
            End If
        Next


        '○ 権限テーブル取得
        Dim WW_PERMIT As String = C_PERMISSION.INVALID
        'NJSの場合、配属部署（任意）入力で未入力の場合あり
        T0007COM.OrgCheck(WF_CAMPCODE.Text, WF_HORG.Text, Master.ROLE_ORG, WW_PERMIT, WW_RTN_SW)
        If Not isNormal(WW_RTN_SW) Then
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "S0012_SRVAUTHOR")
            Exit Sub
        End If

        '○ 条件選択画面の入力値退避
        work.WF_SEL_CAMPCODE.Text = WF_CAMPCODE.Text            '会社コード
        work.WF_SEL_TAISHOYM.Text = WF_TAISHOYM.Text            '対象年月
        work.WF_SEL_HORG.Text = WF_HORG.Text                    '配属部署
        work.WF_SEL_STAFFKBN.Text = WF_STAFFKBN.Text            '職務区分
        work.WF_SEL_STAFFCODE.Text = WF_STAFFCODE.Text          '従業員(コード)
        work.WF_SEL_STAFFNAMES.Text = WF_STAFFNAMES.Text        '従業員(名称)
        work.WF_SEL_LIMITFLG.Text = WW_LIMITFLG                 '締フラグ
        work.WF_SEL_PERMITCODE.Text = WW_PERMIT                 '権限コード

        '○ 画面レイアウト設定
        Master.VIEWID = rightview.GetViewId(WF_CAMPCODE.Text)

        Master.CheckParmissionCode(WF_CAMPCODE.Text)
        If Not Master.MAPpermitcode = C_PERMISSION.INVALID Then
            '画面遷移
            Master.TransitionPage(WF_CAMPCODE.Text)
        End If

    End Sub

    ''' <summary>
    ''' チェック処理
    ''' </summary>
    ''' <param name="O_RTN"></param>
    ''' <remarks></remarks>
    Protected Sub WW_Check(ByRef O_RTN As String)

        O_RTN = ""
        Dim WW_TEXT As String = ""
        Dim WW_CS0024FCHECKERR As String = ""
        Dim WW_CS0024FCHECKREPORT As String = ""

        '○ 単項目チェック
        '会社コード
        Master.CheckField(WF_CAMPCODE.Text, "CAMPCODE", WF_CAMPCODE.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            '存在チェック
            CODENAME_get("CAMPCODE", WF_CAMPCODE.Text, WF_CAMPCODE_TEXT.Text, WW_RTN_SW)
            If Not isNormal(WW_RTN_SW) Then
                Master.Output(C_MESSAGE_NO.NO_DATA_EXISTS_ERROR, C_MESSAGE_TYPE.ERR, "会社コード : " & WF_CAMPCODE.Text)
                WF_CAMPCODE.Focus()
                O_RTN = "ERR"
                Exit Sub
            End If
        Else
            Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR)
            WF_CAMPCODE.Focus()
            O_RTN = "ERR"
            Exit Sub
        End If

        '対象年月
        Master.CheckField(WF_CAMPCODE.Text, "TAISHOYM", WF_TAISHOYM.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            Dim WW_DATE As Date
            Try
                Date.TryParse(WF_TAISHOYM.Text, WW_DATE)
                WF_TAISHOYM.Text = WW_DATE.ToString("yyyy/MM")
            Catch ex As Exception
                Master.Output(C_MESSAGE_NO.DATE_FORMAT_ERROR, C_MESSAGE_TYPE.ABORT, "対象年月 : " & WF_TAISHOYM.Text)
                WF_TAISHOYM.Focus()
                O_RTN = "ERR"
                Exit Sub
            End Try
        Else
            Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR, "対象年月 : " & WF_TAISHOYM.Text)
            WF_TAISHOYM.Focus()
            O_RTN = "ERR"
            Exit Sub
        End If

        '配属部署
        WW_TEXT = WF_HORG.Text
        Master.CheckField(WF_CAMPCODE.Text, "HORG", WF_HORG.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            If WW_TEXT = "" Then
                WF_HORG.Text = ""
            Else
                '存在チェック
                CODENAME_get("HORG", WF_HORG.Text, WF_HORG_TEXT.Text, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    Master.Output(C_MESSAGE_NO.NO_DATA_EXISTS_ERROR, C_MESSAGE_TYPE.ERR, "配属部署 : " & WF_HORG.Text)
                    WF_HORG.Focus()
                    O_RTN = "ERR"
                    Exit Sub
                End If
            End If
        Else
            Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR)
            WF_HORG.Focus()
            O_RTN = "ERR"
            Exit Sub
        End If

        '職務区分
        WW_TEXT = WF_STAFFKBN.Text
        Master.CheckField(WF_CAMPCODE.Text, "STAFFKBN", WF_STAFFKBN.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            If WW_TEXT = "" Then
                WF_STAFFKBN.Text = ""
            Else
                '存在チェック
                CODENAME_get("STAFFKBN", WF_STAFFKBN.Text, WF_STAFFKBN_TEXT.Text, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    Master.Output(C_MESSAGE_NO.NO_DATA_EXISTS_ERROR, C_MESSAGE_TYPE.ERR, "職務区分 : " & WF_STAFFKBN.Text)
                    WF_STAFFKBN.Focus()
                    O_RTN = "ERR"
                    Exit Sub
                End If
            End If
        Else
            Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR)
            WF_STAFFKBN.Focus()
            O_RTN = "ERR"
            Exit Sub
        End If

        '従業員(コード)
        WW_TEXT = WF_STAFFCODE.Text
        Master.CheckField(WF_CAMPCODE.Text, "STAFFCODE", WF_STAFFCODE.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            If WW_TEXT = "" Then
                WF_STAFFCODE.Text = ""
            Else
                '存在チェック
                CODENAME_get("STAFFCODE", WF_STAFFCODE.Text, WF_STAFFCODE_TEXT.Text, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    Master.Output(C_MESSAGE_NO.NO_DATA_EXISTS_ERROR, C_MESSAGE_TYPE.ERR, "従業員(コード) : " & WF_STAFFCODE.Text)
                    WF_STAFFCODE.Focus()
                    O_RTN = "ERR"
                    Exit Sub
                End If
            End If
        Else
            Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR)
            WF_STAFFCODE.Focus()
            O_RTN = "ERR"
            Exit Sub
        End If

        '従業員(名称)
        Master.CheckField(WF_CAMPCODE.Text, "STAFFNAME", WF_STAFFNAMES.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If Not isNormal(WW_CS0024FCHECKERR) Then
            Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR, "従業員(名称) : " & WF_STAFFNAMES.Text)
            WF_STAFFNAMES.Focus()
            O_RTN = "ERR"
            Exit Sub
        End If

        '○ 正常メッセージ
        Master.Output(C_MESSAGE_NO.NORMAL, C_MESSAGE_TYPE.NOR)

    End Sub


    ''' <summary>
    ''' 終了ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonEND_Click()

        '○ 前画面遷移
        Master.TransitionPrevPage()

    End Sub


    ''' <summary>
    ''' フィールドダブルクリック時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_FIELD_DBClick()

        If Not String.IsNullOrEmpty(WF_LeftMViewChange.Value) Then
            Try
                Integer.TryParse(WF_LeftMViewChange.Value, WF_LeftMViewChange.Value)
            Catch ex As Exception
                Exit Sub
            End Try

            With leftview
                Select Case WF_LeftMViewChange.Value
                    Case LIST_BOX_CLASSIFICATION.LC_CALENDAR
                        '日付の場合、入力日付のカレンダーが表示されるように入力値をカレンダーに渡す
                        .WF_Calendar.Text = WF_TAISHOYM.Text & "/01"        '対象年月
                        .ActiveCalendar()

                    Case Else
                        '以外
                        Dim prmData As New Hashtable
                        prmData.Item(C_PARAMETERS.LP_COMPANY) = WF_CAMPCODE.Text

                        'フィールドによってパラメータを変える
                        Select Case WF_FIELD.Value
                            Case "WF_STAFFCODE"         '従業員(コード)
                                prmData = work.CreateStaffCodeParam(GL0005StaffList.LC_STAFF_TYPE.EMPLOYEE_IN_ORG,
                                                                WF_CAMPCODE.Text, WF_TAISHOYM.Text, WF_HORG.Text, WF_STAFFKBN.Text)
                            Case "WF_HORG"              '配属部署
                                prmData = work.CreateHORGParam(WF_CAMPCODE.Text, Master.USERID, Master.ROLE_ORG)
                            Case "WF_STAFFKBN"          '職務区分
                                prmData = work.CreateStaffKBNParam(WF_CAMPCODE.Text)
                        End Select

                        .SetListBox(WF_LeftMViewChange.Value, WW_DUMMY, prmData)
                        .ActiveListBox()
                End Select
            End With
        End If

    End Sub


    ''' <summary>
    ''' フィールドチェンジ時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_FIELD_Change()

        '○ 変更した項目の名称をセット
        Select Case WF_FIELD.Value
            Case "WF_CAMPCODE"          '会社コード
                CODENAME_get("CAMPCODE", WF_CAMPCODE.Text, WF_CAMPCODE_TEXT.Text, WW_RTN_SW)
            Case "WF_HORG"              '配属部署
                CODENAME_get("HORG", WF_HORG.Text, WF_HORG_TEXT.Text, WW_RTN_SW)
            Case "WF_STAFFKBN"          '職務区分
                CODENAME_get("STAFFKBN", WF_STAFFKBN.Text, WF_STAFFKBN_TEXT.Text, WW_RTN_SW)
            Case "WF_STAFFCODE"         '従業員
                CODENAME_get("STAFFCODE", WF_STAFFCODE.Text, WF_STAFFCODE_TEXT.Text, WW_RTN_SW)
        End Select

        '○ メッセージ表示
        If isNormal(WW_RTN_SW) Then
            Master.Output(WW_RTN_SW, C_MESSAGE_TYPE.NOR)
        Else
            Master.Output(WW_RTN_SW, C_MESSAGE_TYPE.ERR)
        End If

    End Sub


    ' ******************************************************************************
    ' ***  LeftBox関連操作                                                       ***
    ' ******************************************************************************

    ''' <summary>
    ''' LeftBox選択時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonSel_Click()

        Dim WW_SelectValue As String = ""
        Dim WW_SelectText As String = ""

        '○ 選択内容を取得
        If Not IsNothing(leftview.GetActiveValue) Then
            WW_SelectValue = leftview.GetActiveValue(0)
            WW_SelectText = leftview.GetActiveValue(1)
        End If

        '○ 選択内容を画面項目へセット
        Select Case WF_FIELD.Value
            Case "WF_CAMPCODE"          '会社コード
                WF_CAMPCODE.Text = WW_SelectValue
                WF_CAMPCODE_TEXT.Text = WW_SelectText
                WF_CAMPCODE.Focus()

            Case "WF_TAISHOYM"          '対象年月
                Dim WW_DATE As Date
                Try
                    Date.TryParse(WW_SelectValue, WW_DATE)
                    WF_TAISHOYM.Text = WW_DATE.ToString("yyyy/MM")
                Catch ex As Exception
                End Try
                WF_TAISHOYM.Focus()

            Case "WF_HORG"              '配属部署
                WF_HORG.Text = WW_SelectValue
                WF_HORG_TEXT.Text = WW_SelectText
                WF_HORG.Focus()

            Case "WF_STAFFKBN"          '職務区分
                WF_STAFFKBN.Text = WW_SelectValue
                WF_STAFFKBN_TEXT.Text = WW_SelectText
                WF_STAFFKBN.Focus()

            Case "WF_STAFFCODE"         '従業員(コード)
                WF_STAFFCODE.Text = WW_SelectValue
                WF_STAFFCODE_TEXT.Text = WW_SelectText
                WF_STAFFCODE.Focus()
        End Select

        '○ 画面左右ボックス非表示は、画面JavaScript(InitLoad)で実行
        WF_FIELD.Value = ""
        WF_LeftboxOpen.Value = ""
        WF_LeftMViewChange.Value = ""

    End Sub


    ''' <summary>
    ''' LeftBoxキャンセルボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonCan_Click()

        '○ フォーカスセット
        Select Case WF_FIELD.Value
            Case "WF_CAMPCODE"          '会社コード
                WF_CAMPCODE.Focus()
            Case "WF_TAISHOYM"          '対象年月
                WF_TAISHOYM.Focus()
            Case "WF_HORG"              '配属部署
                WF_HORG.Focus()
            Case "WF_STAFFKBN"          '職務区分
                WF_STAFFKBN.Focus()
            Case "WF_STAFFCODE"         '従業員(コード)
                WF_STAFFCODE.Focus()
        End Select

        '○ 画面左右ボックス非表示は、画面JavaScript(InitLoad)で実行
        WF_FIELD.Value = ""
        WF_LeftboxOpen.Value = ""
        WF_LeftMViewChange.Value = ""

    End Sub


    ''' <summary>
    ''' RightBoxダブルクリック時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_RIGHTBOX_DBClick()

        rightview.InitViewID(WF_CAMPCODE.Text, WW_DUMMY)

    End Sub

    ''' <summary>
    ''' RightBoxメモ欄更新
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_RIGHTBOX_Change()

        rightview.Save(Master.USERID, Master.USERTERMID, WW_DUMMY)

    End Sub


    ''' <summary>
    ''' ヘルプ表示
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_HELP_Click()

        Master.ShowHelp()

    End Sub


    ' ******************************************************************************
    ' ***  共通処理                                                              ***
    ' ******************************************************************************

    ''' <summary>
    ''' 名称取得
    ''' </summary>
    ''' <param name="I_FIELD"></param>
    ''' <param name="I_VALUE"></param>
    ''' <param name="O_TEXT"></param>
    ''' <param name="O_RTN"></param>
    ''' <remarks></remarks>
    Protected Sub CODENAME_get(ByVal I_FIELD As String, ByVal I_VALUE As String, ByRef O_TEXT As String, ByRef O_RTN As String)

        O_TEXT = ""
        O_RTN = ""

        If I_VALUE = "" Then
            O_RTN = C_MESSAGE_NO.NORMAL
            Exit Sub
        End If

        Dim prmData As New Hashtable
        prmData.Item(C_PARAMETERS.LP_COMPANY) = WF_CAMPCODE.Text

        Try
            Select Case I_FIELD
                Case "CAMPCODE"         '会社コード
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_COMPANY, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "HORG"             '配属部署
                    prmData = work.CreateHORGParam(WF_CAMPCODE.Text, Master.USERID, Master.ROLE_ORG)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_EXTRA_LIST, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "STAFFKBN"         '職務区分
                    prmData = work.CreateStaffKBNParam(WF_CAMPCODE.Text)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_EXTRA_LIST, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "STAFFCODE"        '従業員
                    If Trim(work.WF_SEL_HORG.Text) = "" Then
                        prmData = work.CreateStaffCodeParam(GL0005StaffList.LC_STAFF_TYPE.EMPLOYEE_IN_ORG,
                                                            WF_CAMPCODE.Text, WF_TAISHOYM.Text, WF_HORG.Text, WF_STAFFKBN.Text)
                    Else
                        prmData = work.CreateStaffCodeParam(GL0005StaffList.LC_STAFF_TYPE.ATTENDANCE_FOR_ALL_IN_AORG,
                                                            WF_CAMPCODE.Text, WF_TAISHOYM.Text, work.WF_SEL_HORG.Text, WF_STAFFKBN.Text)
                    End If
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_STAFFCODE, I_VALUE, O_TEXT, O_RTN, prmData)
            End Select
        Catch ex As Exception
            O_RTN = C_MESSAGE_NO.NO_DATA_EXISTS_ERROR
            Exit Sub
        End Try

    End Sub

End Class
