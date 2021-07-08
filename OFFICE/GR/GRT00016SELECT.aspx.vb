Imports System
Imports System.IO
Imports System.Text
Imports System.Globalization
Imports System.Data.SqlClient
Imports Microsoft.VisualBasic
Imports System.Web
Imports System.Web.UI
Imports System.Web.UI.WebControls
Imports System.Web.UI.Control

Imports System.Drawing
Imports System.Net
Imports System.Data
Imports Microsoft.Office.Interop
Imports OFFICE.GRIS0005LeftBox

''' <summary>
''' 荷主請求メンテナンス（条件）
''' </summary>
''' <remarks></remarks>
Public Class GRT00016SELECT
    Inherits System.Web.UI.Page

    '共通処理結果
    Private WW_ERRCODE As String                                    '
    Private WW_ERR_SW As String                                     '
    Private WW_RTN_SW As String                                     '
    Private WW_DUMMY As String                                      '

    ''' <summary>
    ''' サーバ処理の遷移先
    ''' </summary>
    ''' <param name="sender">起動オブジェクト</param>
    ''' <param name="e">イベント発生時パラメータ</param>
    ''' <remarks></remarks>
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

        If IsPostBack Then
            '〇各ボタン押下処理
            If Not String.IsNullOrEmpty(WF_ButtonClick.Value) Then
                Select Case WF_ButtonClick.Value

                    Case "WF_ButtonDO"                      '実行
                        WF_ButtonDO_Click()
                    Case "WF_ButtonEND"                     '終了
                        WF_ButtonEND_Click()

                        '********* 入力フィールド *********
                    Case "WF_Field_DBClick"                 '項目DbClick
                        WF_Field_DBClick()
                    Case "WF_LeftBoxSelectClick"            'フィールドチェンジ
                        WF_LEFTBOX_SELECT_CLICK()

                        '********* 左BOX *********
                    Case "WF_ButtonSel"                     '選択
                        WF_ButtonSel_Click()
                    Case "WF_ButtonCan"                     'キャンセル
                        WF_ButtonCan_Click()
                    Case "WF_ListboxDBclick"                '値選択DbClick
                        WF_LEFTBOX_DBClick()

                        '********* 右BOX *********
                    Case "WF_RIGHT_VIEW_DBClick"            '右ボックス表示
                        WF_RIGHTBOX_DBClick()
                    Case "WF_MEMOChange"                    'メモ欄更新
                        WF_RIGHTBOX_Change()

                        '********* その他はMasterPageで処理 *********
                    Case Else
                End Select
            End If

        Else
            '〇初期化処理
            Initialize()
        End If

    End Sub

    ''' <summary>
    ''' 初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub Initialize()
        '○初期値設定
        Master.MAPID = GRT00016WRKINC.MAPIDS
        WF_FIELD.Value = ""
        WF_CAMPCODE.Focus()

        '〇ヘルプ有
        Master.dispHelp = True
        '〇ドラックアンドドロップOFF
        Master.eventDrop = False

        '左Boxへの値設定
        WF_LeftMViewChange.Value = ""
        leftview.ActiveListBox()

        '○画面の値設定
        WW_MAPValueSet()

        '○RightBox情報設定
        rightview.MAPID = GRT00016WRKINC.MAPID
        rightview.MAPIDS = GRT00016WRKINC.MAPIDS
        rightview.COMPCODE = WF_CAMPCODE.Text
        rightview.MAPVARI = Master.MAPvariant
        rightview.PROFID = Master.PROF_VIEW
        rightview.Initialize("画面レイアウト設定", WW_ERR_SW)
        If Not isNormal(WW_ERR_SW) Then
            Exit Sub
        End If

        '○ ボタン活性／非活性
        If System.IO.File.Exists(work.WF_SEL_XMLsavePARM.Text) Then
            '一時保存ファイルが存在した場合、ボタン活性
            WF_Restart.Value = "TRUE"
        Else
            '一時保存ファイルが存在しない場合、ボタン非活性
            WF_Restart.Value = "FALSE"
        End If

    End Sub

    ''' <summary>
    ''' 終了ボタン処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonEND_Click()

        '〇画面戻先URL取得
        Master.TransitionPrevPage()

    End Sub

    ''' <summary>
    ''' 実行ボタン処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonDO_Click()

        '○初期設定
        WF_FIELD.Value = ""

        '○入力文字置き換え(使用禁止文字排除)
        Master.EraseCharToIgnore(WF_CAMPCODE.Text)                           '会社コード
        Master.EraseCharToIgnore(WF_SEIKYUYMF.Text)                          '請求月FROM
        Master.EraseCharToIgnore(WF_SEIKYUYMT.Text)                          '請求月TO
        Master.EraseCharToIgnore(WF_KEIJYODATEF.Text)                        '計上年月日FROM
        Master.EraseCharToIgnore(WF_KEIJYODATET.Text)                        '計上年月日TO
        Master.EraseCharToIgnore(WF_OILTYPE.Text)                            '油種
        Master.EraseCharToIgnore(WF_MANGORG.Text)                            '管理部署
        Master.EraseCharToIgnore(WF_SHIPORG.Text)                            '出荷部署
        Master.EraseCharToIgnore(WF_TORICODE.Text)                           '荷主
        Master.EraseCharToIgnore(WF_SUPPLCAMP.Text)                          '庸車会社コード

        If Master.ConfirmOK = False Then
            '〇 チェック処理
            WW_Check(WW_ERR_SW)
            If Not isNormal(WW_ERR_SW) Then
                Exit Sub
            End If

        End If

        '○条件選択画面の入力値退避(選択情報のWF_SEL退避) 
        '会社コード　
        work.WF_SEL_CAMPCODE.Text = WF_CAMPCODE.Text
        '請求月　
        work.WF_SEL_SEIKYUYMF.Text = WF_SEIKYUYMF.Text
        If String.IsNullOrWhiteSpace(WF_SEIKYUYMT.Text) Then
            work.WF_SEL_SEIKYUYMT.Text = WF_SEIKYUYMF.Text
        Else
            work.WF_SEL_SEIKYUYMT.Text = WF_SEIKYUYMT.Text
        End If
        '計上年月日　
        work.WF_SEL_KEIJYODATEF.Text = WF_KEIJYODATEF.Text
        If String.IsNullOrWhiteSpace(WF_KEIJYODATET.Text) Then
            work.WF_SEL_KEIJYODATET.Text = WF_KEIJYODATEF.Text
        Else
            work.WF_SEL_KEIJYODATET.Text = WF_KEIJYODATET.Text
        End If
        '油種
        work.WF_SEL_OILTYPE.Text = WF_OILTYPE.Text
        '油種名称
        work.WF_SEL_OILTYPE_NAME.Text = WF_OILTYPE_Text.Text
        '管理部署
        work.WF_SEL_MANGORG.Text = WF_MANGORG.Text
        '出荷部署
        work.WF_SEL_SHIPORG.Text = WF_SHIPORG.Text
        '出荷部署名称
        work.WF_SEL_SHIPORG_NAME.Text = WF_SHIPORG_Text.Text
        '荷主
        work.WF_SEL_TORICODE.Text = WF_TORICODE.Text
        '庸車会社
        work.WF_SEL_SUPPLCAMP.Text = WF_SUPPLCAMP.Text

        work.WF_SEL_RESTART.Text = ""

        '〇右ボックスからPROFID取得
        Master.VIEWID = rightview.GetViewId(work.WF_SEL_CAMPCODE.Text)
        '〇 画面遷移実行
        Master.CheckParmissionCode(WF_CAMPCODE.Text)
        If Not Master.MAPpermitcode = C_PERMISSION.INVALID Then
            '〇画面遷移先URL取得
            Master.TransitionPage()
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
        Dim WW_Select() As String = leftview.GetActiveValue()
        If WW_Select(0).Length = 0 Then Exit Sub

        Select Case leftview.WF_LEFTMView.ActiveViewIndex
            Case 0                'ListBox
                Dim WW_TextBox As TextBox = DirectCast(work.getControl(WF_FIELD.Value), TextBox)
                Dim WW_Label As Label = DirectCast(work.getControl(WF_FIELD.Value & "_Text"), Label)
                WW_TextBox.Text = WW_Select(0)
                WW_Label.Text = WW_Select(1)
                WW_TextBox.Focus()
            Case 1                'Calendar

                Select Case WF_FIELD.Value
                    Case "WF_SEIKYUYMF"          '請求月(FROM)
                        Dim WW_TextBox As TextBox = DirectCast(work.getControl(WF_FIELD.Value), TextBox)

                        Dim WW_DATE As Date
                        Try
                            Date.TryParse(WW_Select(0), WW_DATE)
                            WW_TextBox.Text = WW_DATE.ToString("yyyy/MM")
                        Catch ex As Exception
                        End Try

                        WW_TextBox.Focus()

                    Case "WF_SEIKYUYMT"          '請求月(TO)
                        Dim WW_TextBox As TextBox = DirectCast(work.getControl(WF_FIELD.Value), TextBox)

                        Dim WW_DATE As Date
                        Try
                            Date.TryParse(WW_Select(0), WW_DATE)
                            WW_TextBox.Text = WW_DATE.ToString("yyyy/MM")
                        Catch ex As Exception
                        End Try

                        WW_TextBox.Focus()

                    Case Else
                        Dim WW_TextBox As TextBox = DirectCast(work.getControl(WF_FIELD.Value), TextBox)
                        WW_TextBox.Text = WW_Select(0)
                        WW_TextBox.Focus()

                End Select

            Case Else
        End Select

        '○画面左サイドボックス非表示は、画面JavaScript(InitLoad)で実行
        WF_FIELD.Value = ""
        WF_LeftboxOpen.Value = ""

    End Sub

    ''' <summary>
    ''' leftBOXキャンセルボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonCan_Click()

        Dim WW_TextBox As TextBox = DirectCast(work.getControl(WF_FIELD.Value), TextBox)
        WW_TextBox.Focus()

        '○ 画面左サイドボックス非表示は、画面JavaScript(InitLoad)で実行
        WF_FIELD.Value = ""
        WF_LeftboxOpen.Value = ""

    End Sub
    ''' <summary>
    ''' 左リストボックスダブルクリック処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_LEFTBOX_DBClick()
        '〇ListBoxダブルクリック処理()
        WF_ButtonSel_Click()
        WW_LeftBoxReSet()
    End Sub
    ''' <summary>
    ''' '〇TextBox変更時処理
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
        '〇フィールドダブルクリック時処理
        If String.IsNullOrEmpty(WF_LeftMViewChange.Value) Then Exit Sub
        If Not Integer.TryParse(WF_LeftMViewChange.Value, WF_LeftMViewChange.Value) Then Exit Sub

        With leftview
            If WF_LeftMViewChange.Value <> LIST_BOX_CLASSIFICATION.LC_CALENDAR Then
                Dim prmData As Hashtable = work.createFIXParam(WF_CAMPCODE.Text)

                Select Case WF_FIELD.Value
                    Case "WF_CAMPCODE"
                    Case "WF_OILTYPE"
                    Case "WF_MANGORG"
                        prmData = work.createORGParam(WF_CAMPCODE.Text, True)
                    Case "WF_SHIPORG"
                        prmData = work.createORGParam(WF_CAMPCODE.Text, False)
                    Case "WF_TORICODE"
                        prmData = work.createTORIParam(WF_CAMPCODE.Text)
                    Case "WF_SUPPLCAMP"
                        prmData = work.createTORIParam(WF_CAMPCODE.Text)
                    Case Else
                End Select

                .SetListBox(WF_LeftMViewChange.Value, WW_DUMMY, prmData)
                .ActiveListBox()
            Else
                Select Case WF_FIELD.Value
                    Case "WF_SEIKYUYMF"        '請求月
                        .WF_Calendar.Text = WF_SEIKYUYMF.Text & "/01"
                    Case "WF_SEIKYUYMT"
                        .WF_Calendar.Text = WF_SEIKYUYMT.Text & "/01"
                    Case "WF_KEIJYODATEF"        '出荷日
                        .WF_Calendar.Text = WF_KEIJYODATEF.Text
                    Case "WF_KEIJYODATET"
                        .WF_Calendar.Text = WF_KEIJYODATET.Text
                End Select
                .WF_Calendar.Focus()
                .ActiveCalendar()
            End If
        End With
        WF_LeftMViewChange.Value = ""

    End Sub
    ''' <summary>
    ''' TextBox変更時LeftBox設定処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_LeftBoxReSet()

        WF_CAMPCODE_Text.Text = ""
        WF_SHIPORG_Text.Text = ""
        WF_OILTYPE_Text.Text = ""

        '○入力文字置き換え(使用禁止文字排除)
        Master.EraseCharToIgnore(WF_CAMPCODE.Text)                            '会社コード
        Master.EraseCharToIgnore(WF_SEIKYUYMF.Text)                           '請求月FROM
        Master.EraseCharToIgnore(WF_SEIKYUYMT.Text)                           '請求月TO
        Master.EraseCharToIgnore(WF_KEIJYODATEF.Text)                         '計上年月日FROM
        Master.EraseCharToIgnore(WF_KEIJYODATET.Text)                         '計上年月日TO
        Master.EraseCharToIgnore(WF_OILTYPE.Text)                             '油種
        Master.EraseCharToIgnore(WF_MANGORG.Text)                             '管理部署
        Master.EraseCharToIgnore(WF_SHIPORG.Text)                             '出荷部署
        Master.EraseCharToIgnore(WF_TORICODE.Text)                            '荷主
        Master.EraseCharToIgnore(WF_SUPPLCAMP.Text)                           '庸車会社

        '〇 チェック処理
        WW_Check(WW_ERR_SW)

        '〇名称設定
        WW_NAMESet()

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
        '〇右Boxメモ変更時処理
        rightview.Save(Master.USERID, Master.USERTERMID, WW_DUMMY)
    End Sub

    ' ******************************************************************************
    ' ***  共通処理                                                              ***
    ' ******************************************************************************

    ''' <summary>
    ''' 画面遷移による初期値設定処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_MAPValueSet()
        Dim CS0050SESSION As New CS0050SESSION              'セッション情報管理

        '■■■ 選択画面の入力初期値設定 ■■■
        work.WF_SEL_XMLsavePARM.Text = CS0050SESSION.UPLOAD_PATH & "\XML_TMP\" & Date.Now.ToString("yyyyMMdd") & "-" & Master.USERID & "-" & GRT00016WRKINC.MAPIDS & "-" & "PARM.txt"
        work.WF_SEL_XMLsaveTmp.Text = CS0050SESSION.UPLOAD_PATH & "\XML_TMP\" & Date.Now.ToString("yyyyMMdd") & "-" & Master.USERID & "-" & GRT00016WRKINC.MAPID & "-" & "TMP.txt"

        If Context.Handler.ToString().ToUpper = C_PREV_MAP_LIST.MENU Then               'メニューからの画面遷移

            '〇選択情報のWF_SELクリア
            work.initialize()

            '○画面項目設定（変数より）処理
            WW_VARISet()

        ElseIf Context.Handler.ToString().ToUpper = C_PREV_MAP_LIST.T00016 Then     '実行画面からの画面遷移

            If System.IO.File.Exists(work.WF_SEL_XMLsavePARM.Text) Then
                Dim T0016PARMtbl As DataTable = New DataTable
                '○一時保存ファイルが存在する場合
                'テーブルデータ 復元
                work.PARMtbl_ColumnsAdd(T0016PARMtbl)
                If Not Master.RecoverTable(T0016PARMtbl, work.WF_SEL_XMLsavePARM.Text) Then
                    Exit Sub
                End If

                For Each PARMrow As DataRow In T0016PARMtbl.Rows
                    '会社コード　
                    work.WF_SEL_CAMPCODE.Text = PARMrow("CAMPCODE")
                    '請求月
                    work.WF_SEL_SEIKYUYMF.Text = PARMrow("SEIKYUSHIHARAIYMF")
                    work.WF_SEL_SEIKYUYMT.Text = PARMrow("SEIKYUSHIHARAIYMT")
                    '計上年月日
                    work.WF_SEL_KEIJYODATEF.Text = PARMrow("URIKEIJYOYMDF")
                    work.WF_SEL_KEIJYODATET.Text = PARMrow("URIKEIJYOYMDT")
                    '油種
                    work.WF_SEL_OILTYPE.Text = PARMrow("OILTYPE")
                    '管理部署
                    work.WF_SEL_MANGORG.Text = PARMrow("TORIHIKIMANGORG")
                    '出荷部署
                    work.WF_SEL_SHIPORG.Text = PARMrow("TORIHIKIORG")
                    '荷主
                    work.WF_SEL_TORICODE.Text = PARMrow("TORICODE")
                    '庸車会社
                    work.WF_SEL_SUPPLCAMP.Text = PARMrow("ACTORICODE")

                    '１レコードしか存在しない（念のためEXIT）
                    Exit For
                Next
            End If

            '会社コード　
            WF_CAMPCODE.Text = work.WF_SEL_CAMPCODE.Text
            '請求月
            WF_SEIKYUYMF.Text = work.WF_SEL_SEIKYUYMF.Text
            WF_SEIKYUYMT.Text = work.WF_SEL_SEIKYUYMT.Text
            '計上年月日
            WF_KEIJYODATEF.Text = work.WF_SEL_KEIJYODATEF.Text
            WF_KEIJYODATET.Text = work.WF_SEL_KEIJYODATET.Text
            '油種
            WF_OILTYPE.Text = work.WF_SEL_OILTYPE.Text
            '管理部署
            WF_MANGORG.Text = work.WF_SEL_MANGORG.Text
            '出荷部署
            WF_SHIPORG.Text = work.WF_SEL_SHIPORG.Text
            '荷主
            WF_TORICODE.Text = work.WF_SEL_TORICODE.Text
            '庸車会社
            WF_SUPPLCAMP.Text = work.WF_SEL_SUPPLCAMP.Text
        End If

        '■名称設定
        WW_NAMESet()

    End Sub

    ' ******************************************************************************
    ' ***  共通処理                                                              ***
    ' ******************************************************************************
    ''' <summary>
    ''' 変数設定処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_VARISet()

        Master.GetFirstValue(work.WF_SEL_CAMPCODE.Text, "CAMPCODE", WF_CAMPCODE.Text)               '会社コード
        Master.GetFirstValue(work.WF_SEL_CAMPCODE.Text, "SEIKYUSHIHARAIYMF", WF_SEIKYUYMF.Text)     '請求月(FROM)
        Dim WW_DATE1 As Date
        Try
            Date.TryParse(WF_SEIKYUYMF.Text, WW_DATE1)
            WF_SEIKYUYMF.Text = WW_DATE1.ToString("yyyy/MM")
        Catch ex As Exception
            WF_SEIKYUYMF.Text = Date.Now.ToString("yyyy/MM")
        End Try

        Master.GetFirstValue(work.WF_SEL_CAMPCODE.Text, "SEIKYUSHIHARAIYMT", WF_SEIKYUYMT.Text)     '請求月(TO)
        Dim WW_DATE2 As Date
        Try
            Date.TryParse(WF_SEIKYUYMT.Text, WW_DATE2)
            WF_SEIKYUYMT.Text = WW_DATE2.ToString("yyyy/MM")
        Catch ex As Exception
            WF_SEIKYUYMT.Text = Date.Now.ToString("yyyy/MM")
        End Try

        Master.GetFirstValue(work.WF_SEL_CAMPCODE.Text, "URIKEIJYOYMDF", WF_KEIJYODATEF.Text)       '計上年月日(FROM)
        Master.GetFirstValue(work.WF_SEL_CAMPCODE.Text, "URIKEIJYOYMDT", WF_KEIJYODATET.Text)       '計上年月日(TO)
        Master.GetFirstValue(work.WF_SEL_CAMPCODE.Text, "OILTYPE", WF_OILTYPE.Text)                 '油種
        Master.GetFirstValue(work.WF_SEL_CAMPCODE.Text, "TORIHIKIMANGORG", WF_MANGORG.Text)         '管理部署
        Master.GetFirstValue(work.WF_SEL_CAMPCODE.Text, "TORIHIKIORG", WF_SHIPORG.Text)             '出荷部署
        Master.GetFirstValue(work.WF_SEL_CAMPCODE.Text, "TORICODE", WF_TORICODE.Text)               '荷主
        Master.GetFirstValue(work.WF_SEL_CAMPCODE.Text, "ACTORICODE", WF_SUPPLCAMP.Text)            '庸車会社

    End Sub

    ''' <summary>
    ''' 名称設定処理処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_NAMESet()

        CODENAME_get("CAMPCODE", WF_CAMPCODE.Text, WF_CAMPCODE_Text.Text, WW_DUMMY)         '会社
        CODENAME_get("OILTYPE", WF_OILTYPE.Text, WF_OILTYPE_Text.Text, WW_DUMMY)            '油種
        CODENAME_get("TORIHIKIMANGORG", WF_MANGORG.Text, WF_MANGORG_Text.Text, WW_DUMMY)    '管理部署
        CODENAME_get("TORIHIKIORG", WF_SHIPORG.Text, WF_SHIPORG_Text.Text, WW_DUMMY)        '出荷部署
        CODENAME_get("TORICODE", WF_TORICODE.Text, WF_TORICODE_Text.Text, WW_DUMMY)         '荷主
        CODENAME_get("ACTORICODE", WF_SUPPLCAMP.Text, WF_SUPPLCAMP_Text.Text, WW_DUMMY)     '庸車会社

    End Sub

    ''' <summary>
    ''' チェック処理
    ''' </summary>
    ''' <param name="O_RTN">可否判定</param>
    ''' <remarks></remarks>
    Protected Sub WW_Check(ByRef O_RTN As String)

        O_RTN = C_MESSAGE_NO.NORMAL

        '〇 入力項目チェック
        Dim WW_CS0024FCHECKERR As String = ""
        Dim WW_CS0024FCHECKREPORT As String = ""
        WF_FIELD.Value = ""

        '会社コード 
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

        '●請求月(FROM) 
        Dim WW_SEIKYUYMF As Date

        Master.CheckField(WF_CAMPCODE.Text, "SEIKYUSHIHARAIYMF", WF_SEIKYUYMF.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            If Not Date.TryParse(WF_SEIKYUYMF.Text, WW_SEIKYUYMF) Then
                WW_SEIKYUYMF = C_DEFAULT_YMD
            End If

            Dim WW_DATE As Date
            Try
                Date.TryParse(WF_SEIKYUYMF.Text, WW_DATE)
                WF_SEIKYUYMF.Text = WW_DATE.ToString("yyyy/MM")
            Catch ex As Exception
                WF_SEIKYUYMF.Text = Date.Now.ToString("yyyy/MM")
            End Try

        Else
            Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR, "請求月(FROM)")
            WF_SEIKYUYMF.Focus()
            O_RTN = WW_CS0024FCHECKERR
            Exit Sub
        End If

        '●請求月(TO) 
        Dim WW_SEIKYUYMT As Date

        Master.CheckField(WF_CAMPCODE.Text, "SEIKYUSHIHARAIYMT", WF_SEIKYUYMT.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            If Not Date.TryParse(WF_SEIKYUYMT.Text, WW_SEIKYUYMT) Then
                WW_SEIKYUYMT = C_DEFAULT_YMD
            End If

            Dim WW_DATE As Date
            Try
                Date.TryParse(WF_SEIKYUYMT.Text, WW_DATE)
                WF_SEIKYUYMT.Text = WW_DATE.ToString("yyyy/MM")
            Catch ex As Exception
                WF_SEIKYUYMT.Text = Date.Now.ToString("yyyy/MM")
            End Try

        Else
            Master.Output(WW_CS0024FCHECKERR, BASEDLL.C_MESSAGE_TYPE.ERR, "請求月(TO)")
            WF_SEIKYUYMT.Focus()
            O_RTN = WW_CS0024FCHECKERR
            Exit Sub
        End If

        '関連チェック(開始＞終了)
        If WF_SEIKYUYMF.Text <> "" And WF_SEIKYUYMT.Text <> "" Then
            If WW_SEIKYUYMF > WW_SEIKYUYMT Then
                Master.Output(BASEDLL.C_MESSAGE_NO.START_END_RELATION_ERROR, BASEDLL.C_MESSAGE_TYPE.ERR)
                WF_SEIKYUYMT.Focus()
                O_RTN = C_MESSAGE_NO.START_END_RELATION_ERROR
                Exit Sub
            End If
        End If

        '●計上年月日(FROM) 
        Dim WW_KEIJYODATEF As Date

        Master.CheckField(WF_CAMPCODE.Text, "URIKEIJYOYMDF", WF_KEIJYODATEF.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            If Not Date.TryParse(WF_KEIJYODATEF.Text, WW_KEIJYODATEF) Then
                WW_KEIJYODATEF = C_DEFAULT_YMD
            End If
        Else
            Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR, "計上年月日(FROM)")
            WF_KEIJYODATEF.Focus()
            O_RTN = WW_CS0024FCHECKERR
            Exit Sub
        End If

        '●計上年月日(TO) 
        Dim WW_KEIJYODATET As Date

        Master.CheckField(WF_CAMPCODE.Text, "URIKEIJYOYMDT", WF_KEIJYODATET.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            If Not Date.TryParse(WF_KEIJYODATET.Text, WW_KEIJYODATET) Then
                WW_KEIJYODATET = C_DEFAULT_YMD
            End If
        Else
            Master.Output(WW_CS0024FCHECKERR, BASEDLL.C_MESSAGE_TYPE.ERR, "計上年月日(TO)")
            WF_KEIJYODATET.Focus()
            O_RTN = WW_CS0024FCHECKERR
            Exit Sub
        End If

        '関連チェック(開始＞終了)
        If WF_KEIJYODATEF.Text <> "" And WF_KEIJYODATET.Text <> "" Then
            If WW_KEIJYODATEF > WW_KEIJYODATET Then
                Master.Output(BASEDLL.C_MESSAGE_NO.START_END_RELATION_ERROR, BASEDLL.C_MESSAGE_TYPE.ERR)
                WF_KEIJYODATET.Focus()
                O_RTN = C_MESSAGE_NO.START_END_RELATION_ERROR
                Exit Sub
            End If
        End If

        '●油種
        Master.CheckField(WF_CAMPCODE.Text, "OILTYPE", WF_OILTYPE.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            '存在チェック(LeftBoxチェック)
            If WF_OILTYPE.Text <> "" Then
                CODENAME_get("OILTYPE", WF_OILTYPE.Text, WF_OILTYPE_Text.Text, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    Master.Output(C_MESSAGE_NO.INVALID_SELECTION_DATA, C_MESSAGE_TYPE.ERR)
                    WF_OILTYPE.Focus()
                    O_RTN = C_MESSAGE_NO.INVALID_SELECTION_DATA
                    Exit Sub
                End If
            End If
        Else
            Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR)
            WF_OILTYPE.Focus()
            O_RTN = WW_CS0024FCHECKERR
            Exit Sub
        End If

        '●管理部署
        Master.CheckField(WF_CAMPCODE.Text, "TORIHIKIMANGORG", WF_MANGORG.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            '存在チェック(LeftBoxチェック)
            If WF_MANGORG.Text <> "" Then
                CODENAME_get("TORIHIKIMANGORG", WF_MANGORG.Text, WF_MANGORG_Text.Text, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    Master.Output(C_MESSAGE_NO.INVALID_SELECTION_DATA, C_MESSAGE_TYPE.ERR)
                    WF_SHIPORG.Focus()
                    O_RTN = C_MESSAGE_NO.INVALID_SELECTION_DATA
                    Exit Sub
                End If
            End If
        Else
            Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR)
            WF_MANGORG.Focus()
            O_RTN = WW_CS0024FCHECKERR
            Exit Sub
        End If

        '●出荷部署
        Master.CheckField(WF_CAMPCODE.Text, "TORIHIKIORG", WF_SHIPORG.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            '存在チェック(LeftBoxチェック)
            If WF_SHIPORG.Text <> "" Then
                CODENAME_get("TORIHIKIORG", WF_SHIPORG.Text, WF_SHIPORG_Text.Text, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    Master.Output(C_MESSAGE_NO.INVALID_SELECTION_DATA, C_MESSAGE_TYPE.ERR)
                    WF_SHIPORG.Focus()
                    O_RTN = C_MESSAGE_NO.INVALID_SELECTION_DATA
                    Exit Sub
                End If
            End If
        Else
            Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR)
            WF_SHIPORG.Focus()
            O_RTN = WW_CS0024FCHECKERR
            Exit Sub
        End If

        '●荷主
        Master.CheckField(WF_CAMPCODE.Text, "TORICODE", WF_TORICODE.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            '存在チェック(LeftBoxチェック)
            If WF_TORICODE.Text <> "" Then
                CODENAME_get("TORICODE", WF_TORICODE.Text, WF_TORICODE_Text.Text, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    Master.Output(C_MESSAGE_NO.INVALID_SELECTION_DATA, C_MESSAGE_TYPE.ERR)
                    WF_TORICODE.Focus()
                    O_RTN = C_MESSAGE_NO.INVALID_SELECTION_DATA
                    Exit Sub
                End If
            End If
        Else
            Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR)
            WF_TORICODE.Focus()
            O_RTN = WW_CS0024FCHECKERR
            Exit Sub
        End If

        '●庸車会社
        Master.CheckField(WF_CAMPCODE.Text, "ACTORICODE", WF_SUPPLCAMP.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            '存在チェック(LeftBoxチェック)
            If WF_SUPPLCAMP.Text <> "" Then
                CODENAME_get("ACTORICODE", WF_SUPPLCAMP.Text, WF_SUPPLCAMP_Text.Text, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    Master.Output(C_MESSAGE_NO.INVALID_SELECTION_DATA, C_MESSAGE_TYPE.ERR)
                    WF_SUPPLCAMP.Focus()
                    O_RTN = C_MESSAGE_NO.INVALID_SELECTION_DATA
                    Exit Sub
                End If
            End If
        Else
            Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR)
            WF_SUPPLCAMP.Focus()
            O_RTN = WW_CS0024FCHECKERR
            Exit Sub
        End If

        '正常メッセージ
        Master.Output(C_MESSAGE_NO.NORMAL, C_MESSAGE_TYPE.NOR)

    End Sub

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

        '入力値が空は終了
        If String.IsNullOrEmpty(I_VALUE) Then Exit Sub

        Select Case I_FIELD
            Case "CAMPCODE"
                '会社コード
                leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_COMPANY, I_VALUE, O_TEXT, O_RTN)
            Case "OILTYPE"
                '油種
                leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_OILTYPE, I_VALUE, O_TEXT, O_RTN, work.createFIXParam(WF_CAMPCODE.Text))
            Case "TORIHIKIMANGORG"
                '管理部署
                leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_ORG, I_VALUE, O_TEXT, O_RTN, work.createORGParam(WF_CAMPCODE.Text, True))
            Case "TORIHIKIORG"
                '出荷部署
                leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_ORG, I_VALUE, O_TEXT, O_RTN, work.createORGParam(WF_CAMPCODE.Text, False))
            Case "TORICODE"
                '荷主
                leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_CUSTOMER, I_VALUE, O_TEXT, O_RTN, work.createTORIParam(WF_CAMPCODE.Text))
            Case "ACTORICODE"
                '庸車会社
                leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_CUSTOMER, I_VALUE, O_TEXT, O_RTN, work.createTORIParam(WF_CAMPCODE.Text))
            Case Else
        End Select

    End Sub

End Class