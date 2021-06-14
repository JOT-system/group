Imports OFFICE.GRTA0010WRKINC
Imports OFFICE.GRIS0005LeftBox

Public Class GRTA0010SELECT
    Inherits System.Web.UI.Page

    ' 共通関数宣言(BASEDLL)
    ''' <summary>
    ''' セッション情報管理
    ''' </summary>
    Private CS0050Session As New CS0050SESSION

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

#Region "# 初期処理"

    ''' <summary>
    ''' 画面描画後処理
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        If IsPostBack Then
            '■■■ 各ボタン押下処理 ■■■
            If Not String.IsNullOrEmpty(WF_ButtonClick.Value) Then
                Select Case WF_ButtonClick.Value
                    Case "WF_ButtonDO"                              ' 実行ボタン押下時処理
                        WF_ButtonDO_Click()
                    Case "WF_ButtonEND"                             ' 終了ボタン押下時処理
                        WF_ButtonEND_Click()
                    Case "WF_Field_DBClick"                         ' 入力領域ダブルクリック時処理
                        WF_Field_DBClick()
                    Case "WF_LeftBoxSelectClick"                    ' フィールドチェンジ
                        WF_Field_Change()
                    Case "WF_ButtonSel",                            ' 左ボックス選択ボタン押下時処理
                         "WF_ListboxDBclick"                        ' 左ボックスダブルクリック時処理
                        WF_LeftBoxSelect()
                    Case "WF_ButtonCan"                             ' 左ボックスキャンセルボタン押下時処理
                        WF_ButtonCan_Click()
                    Case "WF_RIGHT_VIEW_DBClick"                    ' 右ボックス表示時処理
                        WF_RIGHTBOX_DBClick()
                    Case "WF_MEMOChange"                            ' 右ボックスメモ欄変更時処理
                        WF_RIGHTBOX_Change()
                End Select
            End If
        Else
            '初期化処理
            Initialize()
        End If
    End Sub

    ''' <summary>
    ''' 初期化処理
    ''' </summary>
    Public Sub Initialize()

        ' 初期値設定
        WF_CAMPCODE.Focus()
        WF_FIELD.Value = ""
        WF_LeftMViewChange.Value = ""
        leftview.ActiveListBox()

        ' 遷移元別初期処理
        SetMapValue()

    End Sub

    ''' <summary>
    ''' 遷移元別初期処理
    ''' </summary>
    Protected Sub SetMapValue()

        ' MAPID設定
        If IsNothing(Master.MAPID) Then Master.MAPID = GRTA0010WRKINC.MAPIDS

        ' 遷移元別初期設定
        Dim strHttpHandler As String = Context.Handler.ToString().ToUpper
        If strHttpHandler = C_PREV_MAP_LIST.MENU Then
            ' メニューからの画面遷移
            SetInitialValue()
        ElseIf strHttpHandler = C_PREV_MAP_LIST.TA0010 Then
            ' 実行画面からの画面遷移
            WF_CAMPCODE.Text = work.WF_SEL_CAMPCODE.Text
            WF_TAISHOYM.Text = work.WF_SEL_TAISHOYM.Text
            WF_ORG.Text = work.WF_SEL_ORG.Text
        End If

        ' 名称設定
        SetNameValue()

        ' RightBox設定
        With rightview
            .MAPID = GRTA0010WRKINC.MAPID
            .MAPIDS = GRTA0010WRKINC.MAPIDS
            .COMPCODE = WF_CAMPCODE.Text
            .MAPVARI = Master.MAPvariant
            .PROFID = Master.PROF_VIEW
            .Initialize("画面レイアウト設定", WW_ERR_SW)
        End With
        If Not isNormal(WW_ERR_SW) Then Exit Sub

    End Sub

    ''' <summary>
    ''' 初期値設定
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub SetInitialValue()

        ' # FIXVALUEより設定
        '会社コード
        Master.GetFirstValue(work.WF_SEL_CAMPCODE.Text, "CAMPCODE", WF_CAMPCODE.Text)
        '対象年月
        Master.GetFirstValue(work.WF_SEL_CAMPCODE.Text, "TAISHOYM", WF_TAISHOYM.Text)
        If IsDate(WF_TAISHOYM.Text) Then WF_TAISHOYM.Text = CDate(WF_TAISHOYM.Text).ToString("yyyy/MM")
        '部署
        Master.GetFirstValue(work.WF_SEL_CAMPCODE.Text, "ORG", WF_ORG.Text)

        ' # FIXVALUEにない場合の値を設定
        ' 会社コード
        If String.IsNullOrWhiteSpace(WF_CAMPCODE.Text) Then
            ' ログインユーザの所属会社を取得
            WF_CAMPCODE.Text = Master.LOGINCOMP
        End If
        ' 対象年月
        If String.IsNullOrWhiteSpace(WF_TAISHOYM.Text) Then
            ' 前月の年月を取得
            WF_TAISHOYM.Text = Date.Now.AddMonths(-1).ToString("yyyy/MM")
        End If
        ' 部署
        If String.IsNullOrWhiteSpace(WF_ORG.Text) Then
            ' ログインユーザの所属部署を取得
            WF_ORG.Text = Master.USER_ORG
        End If

    End Sub

#End Region

#Region "# イベント"

#Region "## メイン画面"

    ''' <summary>
    ''' 実行ボタン押下時処理
    ''' </summary>
    Protected Sub WF_ButtonDO_Click()

        ' 権限確認
        If Not Master.MAPvariant.Equals("Default") AndAlso
           Not (Master.MAPvariant.EndsWith("管理") OrElse Master.MAPvariant.EndsWith("勤怠担当")) Then
            Master.Output(C_MESSAGE_NO.AUTHORIZATION_ERROR, C_MESSAGE_TYPE.ERR, "管理ユーザでなければいけません。")
            Master.ShowMessage()
            Exit Sub
        End If

        ' 検索条件検証
        WW_ERR_SW = VerifySearchCriteria()
        If Not isNormal(WW_ERR_SW) Then Exit Sub

        ' 検索条件退避
        work.WF_SEL_CAMPCODE.Text = WF_CAMPCODE.Text
        work.WF_SEL_TAISHOYM.Text = WF_TAISHOYM.Text
        work.WF_SEL_ORG.Text = WF_ORG.Text

        ' 画面遷移実行
        Master.VIEWID = rightview.GetViewId(WF_CAMPCODE.Text)
        Master.CheckParmissionCode(WF_CAMPCODE.Text)
        If Not Master.MAPpermitcode = C_PERMISSION.INVALID Then
            ' 画面遷移先URL取得
            Master.TransitionPage()
        End If

    End Sub

    ''' <summary>
    ''' 終了ボタン押下時処理
    ''' </summary>
    Protected Sub WF_ButtonEND_Click()
        Master.TransitionPrevPage()
    End Sub

    ''' <summary>
    ''' フィールドダブルクリック時処理
    ''' </summary>
    Protected Sub WF_Field_DBClick()

        If Not String.IsNullOrEmpty(WF_LeftMViewChange.Value) Then
            Try
                Integer.TryParse(WF_LeftMViewChange.Value, WF_LeftMViewChange.Value)
            Catch ex As Exception
                Exit Sub
            End Try

            With leftview
                If WF_LeftMViewChange.Value = LIST_BOX_CLASSIFICATION.LC_CALENDAR Then
                    '日付の場合、入力日付のカレンダーが表示されるように入力値をカレンダーに渡す
                    Select Case WF_FIELD.Value
                        Case WF_TAISHOYM.ID
                            .WF_Calendar.Text = WF_TAISHOYM.Text
                    End Select
                    .ActiveCalendar()
                Else
                    Dim prmData As Hashtable = work.CreateFIXParam(WF_CAMPCODE.Text)

                    Select Case WF_FIELD.Value
                        Case WF_ORG.ID
                            Try
                                prmData = work.CreateSORGParam(WF_CAMPCODE.Text, C_PERMISSION.REFERLANCE)
                            Catch ex As Exception
                                Exit Sub
                            End Try
                    End Select

                    .SetListBox(WF_LeftMViewChange.Value, WW_DUMMY, prmData)
                    .ActiveListBox()
                End If
            End With
        End If
    End Sub

    ''' <summary>
    ''' フィールドチェンジ時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_Field_Change()
        ' 名称設定処理 
        SetNameValue()
    End Sub

#End Region

#Region "## 左BOX"

    ''' <summary>
    ''' 左リストボックス選択時
    ''' </summary>
    Protected Sub WF_LeftBoxSelect()

        Dim selectValue As String = ""
        Dim selectText As String = ""
        Dim WW_SelectValues As String() = Nothing

        ' 選択内容を取得
        If leftview.WF_LeftListBox.SelectedIndex >= 0 Then
            WF_SelectedIndex.Value = leftview.WF_LeftListBox.SelectedIndex
            selectValue = leftview.WF_LeftListBox.Items(WF_SelectedIndex.Value).Value
            selectText = leftview.WF_LeftListBox.Items(WF_SelectedIndex.Value).Text
        End If

        ' 選択内容を画面項目へセット
        Select Case WF_FIELD.Value
            Case WF_CAMPCODE.ID         ' 会社コード
                WF_CAMPCODE.Text = selectValue
                WF_CAMPCODE_Text.Text = selectText
                WF_CAMPCODE.Focus()
            Case WF_TAISHOYM.ID         ' 年月
                If Not IsNothing(leftview.GetActiveValue) Then
                    WW_SelectValues = leftview.GetActiveValue
                End If
                Dim wDate As Date
                'If Date.TryParse(selectValue, wDate) AndAlso wDate >= C_DEFAULT_YMD Then
                '    WF_TAISHOYM.Text = wDate.ToString("yyyy/MM")
                'Else
                '    WF_TAISHOYM.Text = ""
                'End If

                Try
                    Date.TryParse(WW_SelectValues(0), wDate)
                    WF_TAISHOYM.Text = wDate.ToString("yyyy/MM")
                Catch ex As Exception
                End Try

                WF_TAISHOYM.Focus()
            Case WF_ORG.ID              ' 部署
                WF_ORG.Text = selectValue
                WF_ORG_TEXT.Text = selectText
                WF_ORG.Focus()
        End Select

        ' 画面左サイドボックス非表示は、画面JavaScript(InitLoad)で実行
        WF_LeftboxOpen.Value = ""
        WF_FIELD.Value = ""
        WF_LeftMViewChange.Value = ""

    End Sub

    ''' <summary>
    ''' 左リストボックスキャンセル時
    ''' </summary>
    Protected Sub WF_ButtonCan_Click()

        ' フォーカスセット
        Select Case WF_FIELD.Value
            Case WF_FIELD.ID
                ' 会社コード
                WF_CAMPCODE.Focus()
            Case WF_TAISHOYM.ID
                ' 年月
                WF_TAISHOYM.Focus()
            Case WF_ORG.ID
                ' 機能選択
                WF_ORG.Focus()
        End Select

        ' 画面左サイドボックス非表示は、画面JavaScript(InitLoad)で実行
        WF_LeftboxOpen.Value = ""
        WF_FIELD.Value = ""
        WF_LeftMViewChange.Value = ""

    End Sub

#End Region

#Region "## 右BOX"

    ''' <summary>
    ''' 右リストボックスダブルクリック処理
    ''' </summary>
    Protected Sub WF_RIGHTBOX_DBClick()
        rightview.InitViewID(WF_CAMPCODE.Text, WW_DUMMY)
    End Sub

    ''' <summary>
    ''' 右リストボックスMEMO欄更新
    ''' </summary>
    Protected Sub WF_RIGHTBOX_Change()
        rightview.Save(Master.USERID, Master.USERTERMID, WW_DUMMY)
    End Sub

#End Region

#End Region

    ''' <summary>
    ''' 名称設定処理
    ''' </summary>
    Protected Sub SetNameValue()
        Dim getResult As String = C_MESSAGE_NO.NORMAL

        ' 会社名称
        If Not String.IsNullOrWhiteSpace(WF_CAMPCODE.Text) Then
            leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_COMPANY, WF_CAMPCODE.Text, WF_CAMPCODE_Text.Text, getResult)
            If Not isNormal(getResult) Then
                WF_CAMPCODE_Text.Text = ""
                Master.Output(getResult, C_MESSAGE_TYPE.ERR)
                WF_CAMPCODE.Focus()
                Exit Sub
            End If
        Else
            WF_CAMPCODE_Text.Text = ""
        End If

        ' 部署名称
        If Not String.IsNullOrWhiteSpace(WF_ORG.Text) Then
            leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_ORG, WF_ORG.Text, WF_ORG_TEXT.Text, getResult, work.CreateSORGParam(WF_CAMPCODE.Text, C_PERMISSION.REFERLANCE))
            If Not isNormal(getResult) Then
                WF_ORG_TEXT.Text = ""
                Master.Output(getResult, C_MESSAGE_TYPE.ERR)
                WF_ORG.Focus()
                Exit Sub
            End If
        Else
            WF_ORG_TEXT.Text = ""
        End If

    End Sub

    ''' <summary>
    ''' 検索条件を検証する
    ''' </summary>
    ''' <returns></returns>
    Protected Function VerifySearchCriteria()

        ' ■入力禁止文字削除
        ' 会社コード
        Master.EraseCharToIgnore(WF_CAMPCODE.Text)
        ' 年月
        Master.EraseCharToIgnore(WF_TAISHOYM.Text)
        ' 作業部署
        Master.EraseCharToIgnore(WF_ORG.Text)

        '■入力項目チェック
        Dim checkIO As String = ""
        Dim checkResult As String = C_MESSAGE_NO.NORMAL
        Dim checkReport As String = ""

        ' 会社コード
        checkIO = WF_CAMPCODE.Text
        Master.CheckField(WF_CAMPCODE.Text, "CAMPCODE", checkIO, checkResult, checkReport)
        If isNormal(checkResult) Then
            ' 存在チェック
            If Not String.IsNullOrWhiteSpace(WF_CAMPCODE.Text) Then
                leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_COMPANY, WF_CAMPCODE.Text, WF_CAMPCODE_Text.Text, checkResult)
                If Not isNormal(checkResult) Then
                    checkResult = C_MESSAGE_NO.INVALID_SELECTION_DATA
                End If
            End If
        End If
        ' エラー時処理
        If Not isNormal(checkResult) Then
            Master.Output(checkResult, C_MESSAGE_TYPE.ERR)
            WF_CAMPCODE.Focus()
            Return checkResult
        End If

        ' 年月
        checkIO = WF_TAISHOYM.Text
        Master.CheckField(WF_CAMPCODE.Text, "TAISHOYM", checkIO, checkResult, checkReport)
        If isNormal(checkResult) Then
            ' 年月チェック
            Dim wDate As Date
            If Not Date.TryParse(checkIO, wDate) Then
                checkResult = C_MESSAGE_NO.INVALID_SELECTION_DATA
            End If
        End If
        ' エラー時処理
        If Not isNormal(checkResult) Then
            Master.Output(checkResult, C_MESSAGE_TYPE.ERR)
            WF_TAISHOYM.Focus()
            Return checkResult
        End If

        ' 作業部署
        checkIO = WF_ORG.Text
        Master.CheckField(WF_CAMPCODE.Text, "ORG", checkIO, checkResult, checkReport)
        If isNormal(checkResult) Then
            '存在チェック(LeftBoxチェック)
            If Not String.IsNullOrWhiteSpace(WF_ORG.Text) Then
                leftview.CodeToName(
                    LIST_BOX_CLASSIFICATION.LC_ORG,
                    WF_ORG.Text,
                    WF_ORG_TEXT.Text,
                    checkResult,
                    work.CreateSORGParam(WF_CAMPCODE.Text, C_PERMISSION.REFERLANCE)
                )
                If Not isNormal(checkResult) Then
                    checkResult = C_MESSAGE_NO.INVALID_SELECTION_DATA
                End If
            End If
        End If
        ' エラー時処理
        If Not isNormal(checkResult) Then
            Master.Output(checkResult, C_MESSAGE_TYPE.ERR)
            WF_ORG.Focus()
            Return checkResult
        End If

        ' 正常時処理
        Master.Output(C_MESSAGE_NO.NORMAL, C_MESSAGE_TYPE.NOR)
        Return C_MESSAGE_NO.NORMAL

    End Function

End Class