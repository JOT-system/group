Imports System.Data.SqlClient

Public Class M00001MENU
    Inherits System.Web.UI.Page

    '*共通関数宣言(BASEDLL)
    Private CS0011LOGWRITE As New CS0011LOGWrite            'LogOutput DirString Get
    Private CS0050Session As New CS0050SESSION              'セッション情報

    ''' <summary>
    '''  パスワードの変更依頼（期限切れまで何日前からか）
    ''' </summary>
    Private Const C_PASSWORD_CHANGE_LIMIT_COUNT As Integer = 31
    ''' <summary>
    ''' サーバー処理の遷移先
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

        If IsPostBack Then
            Select Case WF_ButtonClick.Value
                Case "WF_WARNNING", "WF_GUID"        '更新ボタン
                    Initialize()
            End Select
        Else
            '★★★ 初期画面表示 ★★★
            Initialize()
        End If

    End Sub

    ''' <summary>
    ''' 初期処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub Initialize()
        Master.MAPID = GRM00001WRKINC.MAPID
        '★★★ メニュー貼り付け ★★★

        '○メニュー貼り付け（左）
        Dim WW_Select_CNT As String = String.Empty

        '　１回目（ユーザＩＤ）での貼り付け
        Using SQLcon As SqlConnection = CS0050Session.getConnection
            Try
                'DataBase接続文字
                SQLcon.Open() 'DataBase接続(Open)

                '検索SQL文 最大２１行で取得できたものを当て込むように修正する
                Dim SQLStr As String =
                          "WITH ROWIDX(ROWLINE)  AS (          " _
                        & " SELECT                             " _
                        & "      1               AS ROWLINE    " _
                        & " UNION ALL                          " _
                        & " SELECT                             " _
                        & "      ROWLINE + 1     AS ROWLINE    " _
                        & " FROM  ROWIDX                       " _
                        & " WHERE ROWLINE <= 21                " _
                        & ")                                   " _
                        & " SELECT                             " _
                        & "      rtrim(R.ROWLINE)               as SEQ     , " _
                        & "      rtrim(isnull(A.MAPID,''))      as MAPID   , " _
                        & "      rtrim(isnull(A.VARIANT,''))    as VARIANT , " _
                        & "      rtrim(isnull(A.TITLENAMES,'')) as TITLE   , " _
                        & "      rtrim(isnull(A.MAPNAMES,''))   as NAMES   , " _
                        & "      rtrim(isnull(A.MAPNAMEL,''))   as NAMEL   , " _
                        & "      rtrim(isnull(B.URL,''))        as URL       " _
                        & " FROM      ROWIDX                      R          " _
                        & " LEFT JOIN S0024_PROFMMAP              A       ON " _
                        & "       A.CAMPCODE = @P1                           " _
                        & "   and A.MAPIDP   = @P2                           " _
                        & "   and A.VARIANTP = @P3                           " _
                        & "   and A.TITLEKBN = 'I'                           " _
                        & "   and A.POSICOL  = @P4                           " _
                        & "   and A.STYMD   <= @P5                           " _
                        & "   and A.ENDYMD  >= @P6                           " _
                        & "   and A.DELFLG  <> @P7                           " _
                        & "   and A.POSIROW  = R.ROWLINE                     " _
                        & " LEFT JOIN S0009_URL                   B       ON " _
                        & "       B.MAPID    = A.MAPID                       " _
                        & "   and B.STYMD   <= @P5                           " _
                        & "   and B.ENDYMD  >= @P6                           " _
                        & "   and B.DELFLG  <> @P7                           " _
                        & " ORDER BY R.ROWLINE                               "
                Dim SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.NVarChar, 50)
                Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.NVarChar, 50)
                Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", System.Data.SqlDbType.NVarChar, 1)
                Dim PARA5 As SqlParameter = SQLcmd.Parameters.Add("@P5", System.Data.SqlDbType.Date)
                Dim PARA6 As SqlParameter = SQLcmd.Parameters.Add("@P6", System.Data.SqlDbType.Date)
                Dim PARA7 As SqlParameter = SQLcmd.Parameters.Add("@P7", System.Data.SqlDbType.NVarChar, 1)
                PARA1.Value = work.WF_SEL_CAMPCODE.Text
                PARA2.Value = Master.MAPID
                PARA3.Value = Master.MAPvariant
                PARA4.Value = "1"
                PARA5.Value = Date.Now
                PARA6.Value = Date.Now
                PARA7.Value = C_DELETE_FLG.DELETE
                Dim SQLdrL As SqlDataReader = SQLcmd.ExecuteReader()

                If SQLdrL.HasRows = True Then
                    Repeater_Menu_L.DataSource = SQLdrL
                    Repeater_Menu_L.DataBind()
                    WW_Select_CNT = "OK"
                Else
                    WW_Select_CNT = "NG"
                End If

                'Close
                SQLdrL.Close() 'Reader(Close)
                SQLdrL = Nothing

                '○メニュー貼り付け（右）
                WW_Select_CNT = ""

                '　１回目（ユーザＩＤ）での貼り付け
                PARA1.Value = work.WF_SEL_CAMPCODE.Text
                PARA2.Value = Master.MAPID
                PARA3.Value = Master.MAPvariant
                PARA4.Value = "2"
                PARA5.Value = Date.Now
                PARA6.Value = Date.Now
                PARA7.Value = C_DELETE_FLG.DELETE
                Dim SQLdrR As SqlDataReader = SQLcmd.ExecuteReader()

                If SQLdrR.HasRows = True Then
                    Repeater_Menu_R.DataSource = SQLdrR
                    Repeater_Menu_R.DataBind()
                    WW_Select_CNT = "OK"
                Else
                    WW_Select_CNT = "NG"
                End If

                'Close
                SQLdrR.Close() 'Reader(Close)
                SQLdrR = Nothing

                SQLcmd.Dispose()
                SQLcmd = Nothing


            Catch ex As Exception
                Master.output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "S0008_UPROFMAP SELECT")

                CS0011LOGWRITE.INFSUBCLASS = "Main"
                CS0011LOGWRITE.INFPOSI = "S0008_UPROFMAP SELECT"
                CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
                CS0011LOGWRITE.TEXT = ex.ToString()
                CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                CS0011LOGWRITE.CS0011LOGWrite()
                Exit Sub
            End Try

            '■■■ パスワード有効期限の警告表示 ■■■
            '○パスワード有効期限の警告表示
            Dim WW_ENDYMD As Date = Date.Now

            Try

                'S0014_USER検索SQL文
                Dim SQL_Str As String =
                     "SELECT PASSENDYMD " _
                   & " FROM  S0014_USERPASS " _
                   & " Where USERID = @P1 " _
                   & "   and DELFLG <> @P2 "
                Dim USERcmd As New SqlCommand(SQL_Str, SQLcon)
                Dim PARA1 As SqlParameter = USERcmd.Parameters.Add("@P1", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA2 As SqlParameter = USERcmd.Parameters.Add("@P2", System.Data.SqlDbType.NVarChar, 1)
                PARA1.Value = CS0050Session.USERID
                PARA2.Value = "1"
                Dim SQLdr As SqlDataReader = USERcmd.ExecuteReader()

                While SQLdr.Read
                    WW_ENDYMD = SQLdr("PASSENDYMD")
                    Exit While
                End While

                'Close
                SQLdr.Close() 'Reader(Close)
                SQLdr = Nothing

                USERcmd.Dispose()
                USERcmd = Nothing

            Catch ex As Exception
                Master.output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "S0014_USERPASS SELECT")

                CS0011LOGWRITE.INFSUBCLASS = "Main"                         'SUBクラス名
                CS0011LOGWRITE.INFPOSI = "S0014_USERPASS SELECT"                '
                CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
                CS0011LOGWRITE.TEXT = ex.ToString()
                CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR 'DBエラー。
                CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
                Exit Sub
            End Try

            If DateDiff("d", Date.Now, WW_ENDYMD) < C_PASSWORD_CHANGE_LIMIT_COUNT Then
                Master.output(C_MESSAGE_NO.PASSWORD_INVALID_AT_SOON, C_MESSAGE_TYPE.INF)
            End If

        End Using

        '2020/10/30 ADD +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        '運用ガイダンス、車検切れ等の情報取得
        Dim CS001INIFILE As New CS0001INIFILEget            'INIファイル読み込み
        Dim CS0006TERMchk As New CS0006TERMchk              'ローカルコンピュータ名存在チェック
        Dim CS0008ONLINEstat As New CS0008ONLINEstat        'ONLINE状態

        '○ 固定項目設定
        If String.IsNullOrEmpty(CS0050Session.USERID) Then
            CS0050Session.USERID = "INIT"
            CS0050Session.APSV_ID = "INIT"
            CS0050Session.APSV_COMPANY = "INIT"
            CS0050Session.APSV_ORG = "INIT"
            CS0050Session.SELECTED_COMPANY = "INIT"
            CS0050Session.DRIVERS = ""
        End If


        CS001INIFILE.CS0001INIFILEget()
        If Not isNormal(CS001INIFILE.ERR) Then
            Master.Output(CS001INIFILE.ERR, C_MESSAGE_TYPE.ABORT)
            Exit Sub
        End If

        '○ APサーバー情報からAPサーバー設置会社(APSRVCamp)、APサーバー設置部署(APSRVOrg)取得

        '〇クライアント端末のIPを取得する

        '■■■　運用ガイダンス表示　■■■
        Dim WW_RTN As String = C_MESSAGE_NO.NORMAL
        Dim WW_CAMP As String = ""
        Dim WW_ORG As String = ""

        WF_OnlineStat.Text = ""
        WF_Guidance.Text = ""

        'ユーザーマスタよりログオンユーザー情報を取得
        GetSTAFF(Master.USERID, WW_CAMP, WW_ORG, WW_RTN)
        If Not isNormal(WW_RTN) Then Exit Sub

        '○オンラインサービス停止なら画面遷移しない 
        '接続サーバ（INIファイルのサーバ）、対象会社がオンラインか確認
        CS0008ONLINEstat.COMPCODE = WW_CAMP
        CS0008ONLINEstat.CS0008ONLINEstat()
        If isNormal(CS0008ONLINEstat.ERR) Then
            If CS0008ONLINEstat.ONLINESW = 0 Then
                Master.Output(C_MESSAGE_NO.CLOSED_SERVICE, C_MESSAGE_TYPE.ERR)
                WF_OnlineStat.Text = String.Empty
            Else
                WF_OnlineStat.Text = CS0008ONLINEstat.TEXT.Replace(vbCrLf, "<br />")
            End If
        Else
            Master.Output(CS0008ONLINEstat.ERR, C_MESSAGE_TYPE.ABORT, "CS0008ONLINEstat")
            Exit Sub
        End If

        '○ 車検切れ、容器検査切れ車両の検索表示（運用ガイダンスに表示）
        GetSHARYOC(WW_ORG, WW_RTN)
        If Not isNormal(WW_RTN) Then Exit Sub
        '2020/10/30 ADD END +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

    End Sub
    ' ******************************************************************************
    ' ***  Repeater_Menu_L バインド時 編集（左）                                 ***
    ' ******************************************************************************
    Protected Sub rptInfo_ItemDataBound_L(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.RepeaterItemEventArgs) Handles Repeater_Menu_L.ItemDataBound

        '★★★ Repeater_Menu_Lバインド時 編集（左） ★★★
        '○ヘッダー編集 処理なし
        If (e.Item.ItemType = ListItemType.Header) Then
        End If

        '○アイテム編集
        If ((e.Item.ItemType = ListItemType.Item) Or (e.Item.ItemType = ListItemType.AlternatingItem)) Then
            CType(e.Item.FindControl("WF_MenuLabe_L"), Label).Text = DataBinder.Eval(e.Item.DataItem, "TITLE")
            CType(e.Item.FindControl("WF_MenuVARI_L"), Label).Text = DataBinder.Eval(e.Item.DataItem, "VARIANT")
            If IsDBNull(DataBinder.Eval(e.Item.DataItem, "URL")) Then
                CType(e.Item.FindControl("WF_MenuURL_L"), Label).Text = String.Empty
            Else
                CType(e.Item.FindControl("WF_MenuURL_L"), Label).Text = DataBinder.Eval(e.Item.DataItem, "URL")
            End If
            CType(e.Item.FindControl("WF_MenuMAP_L"), Label).Text = DataBinder.Eval(e.Item.DataItem, "MAPID")
            CType(e.Item.FindControl("WF_MenuButton_L"), Button).Text = "  " & DataBinder.Eval(e.Item.DataItem, "NAMES")

            If DataBinder.Eval(e.Item.DataItem, "TITLE") = "" Then
                If DataBinder.Eval(e.Item.DataItem, "NAMES") = "" Then
                    CType(e.Item.FindControl("WF_MenuLabe_L"), Label).Text = "　　"
                    CType(e.Item.FindControl("WF_MenuLabe_L"), Label).Visible = True
                    CType(e.Item.FindControl("WF_MenuVARI_L"), Label).Visible = False
                    CType(e.Item.FindControl("WF_MenuButton_L"), Button).Visible = False
                    CType(e.Item.FindControl("WF_MenuURL_L"), Label).Visible = False
                    CType(e.Item.FindControl("WF_MenuMAP_L"), Label).Visible = False
                Else
                    CType(e.Item.FindControl("WF_MenuLabe_L"), Label).Visible = False
                    CType(e.Item.FindControl("WF_MenuVARI_L"), Label).Visible = False
                    CType(e.Item.FindControl("WF_MenuButton_L"), Button).Visible = True
                    CType(e.Item.FindControl("WF_MenuURL_L"), Label).Visible = False
                    CType(e.Item.FindControl("WF_MenuMAP_L"), Label).Visible = False
                End If
            Else
                CType(e.Item.FindControl("WF_MenuLabe_L"), Label).Visible = True
                CType(e.Item.FindControl("WF_MenuVARI_L"), Label).Visible = False
                CType(e.Item.FindControl("WF_MenuButton_L"), Button).Visible = False
                CType(e.Item.FindControl("WF_MenuURL_L"), Label).Visible = False
                CType(e.Item.FindControl("WF_MenuMAP_L"), Label).Visible = False
            End If

        End If

        '○フッター編集　 処理なし
        If e.Item.ItemType = ListItemType.Footer Then
        End If

    End Sub

    ' ******************************************************************************
    ' ***  Repeater_Menu_R バインド時 編集（右）                                 ***
    ' ******************************************************************************
    Protected Sub rptInfo_ItemDataBound_R(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.RepeaterItemEventArgs) Handles Repeater_Menu_R.ItemDataBound

        '★★★ Repeater_Menu_Rバインド時 編集（右） ★★★
        '○ヘッダー編集　 処理なし
        If (e.Item.ItemType = ListItemType.Header) Then
        End If

        '○アイテム編集
        If ((e.Item.ItemType = ListItemType.Item) Or (e.Item.ItemType = ListItemType.AlternatingItem)) Then
            CType(e.Item.FindControl("WF_MenuLabe_R"), Label).Text = DataBinder.Eval(e.Item.DataItem, "TITLE")
            CType(e.Item.FindControl("WF_MenuVARI_R"), Label).Text = DataBinder.Eval(e.Item.DataItem, "VARIANT")
            If IsDBNull(DataBinder.Eval(e.Item.DataItem, "URL")) Then
                CType(e.Item.FindControl("WF_MenuURL_R"), Label).Text = ""
            Else
                CType(e.Item.FindControl("WF_MenuURL_R"), Label).Text = DataBinder.Eval(e.Item.DataItem, "URL")
            End If
            CType(e.Item.FindControl("WF_MenuMAP_R"), Label).Text = DataBinder.Eval(e.Item.DataItem, "MAPID")
            CType(e.Item.FindControl("WF_MenuButton_R"), Button).Text = "  " & DataBinder.Eval(e.Item.DataItem, "NAMES")

            If DataBinder.Eval(e.Item.DataItem, "TITLE") = "" Then
                If DataBinder.Eval(e.Item.DataItem, "NAMES") = "" Then
                    CType(e.Item.FindControl("WF_MenuLabe_R"), Label).Text = "　　"
                    CType(e.Item.FindControl("WF_MenuLabe_R"), Label).Visible = True
                    CType(e.Item.FindControl("WF_MenuVARI_R"), Label).Visible = False
                    CType(e.Item.FindControl("WF_MenuButton_R"), Button).Visible = False
                    CType(e.Item.FindControl("WF_MenuURL_R"), Label).Visible = False
                    CType(e.Item.FindControl("WF_MenuMAP_R"), Label).Visible = False
                Else
                    CType(e.Item.FindControl("WF_MenuLabe_R"), Label).Visible = False
                    CType(e.Item.FindControl("WF_MenuVARI_R"), Label).Visible = False
                    CType(e.Item.FindControl("WF_MenuButton_R"), Button).Visible = True
                    CType(e.Item.FindControl("WF_MenuURL_R"), Label).Visible = False
                    CType(e.Item.FindControl("WF_MenuMAP_R"), Label).Visible = False
                End If
            Else
                CType(e.Item.FindControl("WF_MenuLabe_R"), Label).Visible = True
                CType(e.Item.FindControl("WF_MenuVARI_R"), Label).Visible = False
                CType(e.Item.FindControl("WF_MenuButton_R"), Button).Visible = False
                CType(e.Item.FindControl("WF_MenuURL_R"), Label).Visible = False
                CType(e.Item.FindControl("WF_MenuMAP_R"), Label).Visible = False
            End If
        End If

        '○フッター編集　 処理なし
        If e.Item.ItemType = ListItemType.Footer Then
        End If

    End Sub

    ' ******************************************************************************
    ' ***  Repeater_Menu_L ボタン押下処理                                        ***
    ' ******************************************************************************
    Protected Sub Repeater_Menu_ItemCommand_L(source As Object, e As RepeaterCommandEventArgs) Handles Repeater_Menu_L.ItemCommand

        '共通宣言
        '*共通関数宣言(BASEDLL)
        Dim CS0011LOGWRITE As New CS0011LOGWrite            'LogOutput DirString Get
        Dim CS0009MESSAGEout As New CS0009MESSAGEout        'Message out
        Dim CS0007CheckAuthority As New CS0007CheckAuthority          'AUTHORmap

        '★★★ ボタン押下時、画面遷移（左） ★★★
        '○ボタン押下時、画面遷移情報取得
        Dim WW_COUNT As Integer = e.Item.ItemIndex.ToString()
        Dim WW_URL As Label = Repeater_Menu_L.Items(WW_COUNT).FindControl("WF_MenuURL_L")
        Dim WW_VARI As Label = Repeater_Menu_L.Items(WW_COUNT).FindControl("WF_MenuVARI_L")
        Dim WW_MAPID As Label = Repeater_Menu_L.Items(WW_COUNT).FindControl("WF_MenuMAP_L")

        '○画面遷移権限チェック（左）
        CS0007CheckAuthority.MAPID = WW_MAPID.Text
        CS0007CheckAuthority.ROLECODE_MAP = Master.ROLE_MAP
        CS0007CheckAuthority.check()
        If isNormal(CS0007CheckAuthority.ERR) Then
            If CS0007CheckAuthority.MAPPERMITCODE = C_PERMISSION.REFERLANCE OrElse
               CS0007CheckAuthority.MAPPERMITCODE = C_PERMISSION.UPDATE Then
                CS0050Session.VIEW_PERMIT = CS0007CheckAuthority.MAPPERMITCODE
                CS0050Session.VIEW_MAPID = WW_MAPID.Text
                CS0050Session.VIEW_MAP_VARIANT = WW_VARI.Text
                CS0050Session.MAP_ETC = ""

                Master.MAPvariant = WW_VARI.Text
                Master.MAPID = WW_MAPID.Text
                Master.MAPpermitcode = CS0007CheckAuthority.MAPPERMITCODE
                Master.output(C_MESSAGE_NO.NORMAL, C_MESSAGE_TYPE.NOR)
                Master.showMessage()

            Else
                Master.output(C_MESSAGE_NO.AUTHORIZATION_ERROR, C_MESSAGE_TYPE.ABORT, "画面:" & WW_MAPID.Text)
                Master.showMessage()

                Exit Sub
            End If
        Else
            Master.output(CS0007CheckAuthority.ERR, C_MESSAGE_TYPE.ABORT, "画面:" & WW_MAPID.Text)
            Master.showMessage()

            Exit Sub
        End If
        'セッション変数クリア
        HttpContext.Current.Session("Selected_STYMD") = ""
        HttpContext.Current.Session("Selected_ENDYMD") = ""

        HttpContext.Current.Session("Selected_USERIDFrom") = ""
        HttpContext.Current.Session("Selected_USERIDTo") = ""
        HttpContext.Current.Session("Selected_USERIDG1") = ""
        HttpContext.Current.Session("Selected_USERIDG2") = ""
        HttpContext.Current.Session("Selected_USERIDG3") = ""
        HttpContext.Current.Session("Selected_USERIDG4") = ""
        HttpContext.Current.Session("Selected_USERIDG5") = ""

        HttpContext.Current.Session("Selected_MAPIDPFrom") = ""
        HttpContext.Current.Session("Selected_MAPIDPTo") = ""
        HttpContext.Current.Session("Selected_MAPIDPG1") = ""
        HttpContext.Current.Session("Selected_MAPIDPG2") = ""
        HttpContext.Current.Session("Selected_MAPIDPG3") = ""
        HttpContext.Current.Session("Selected_MAPIDPG4") = ""
        HttpContext.Current.Session("Selected_MAPIDPG5") = ""

        HttpContext.Current.Session("Selected_MAPIDFrom") = ""
        HttpContext.Current.Session("Selected_MAPIDTo") = ""
        HttpContext.Current.Session("Selected_MAPIDG1") = ""
        HttpContext.Current.Session("Selected_MAPIDG2") = ""
        HttpContext.Current.Session("Selected_MAPIDG3") = ""
        HttpContext.Current.Session("Selected_MAPIDG4") = ""
        HttpContext.Current.Session("Selected_MAPIDG5") = ""
        'ボタン押下時、画面遷移
        Server.Transfer(WW_URL.Text)

    End Sub

    ' ******************************************************************************
    ' ***  Repeater_Menu_R ボタン押下処理                                        ***
    ' ******************************************************************************
    Protected Sub Repeater_Menu_ItemCommand_R(source As Object, e As RepeaterCommandEventArgs) Handles Repeater_Menu_R.ItemCommand

        '共通宣言
        '*共通関数宣言(BASEDLL)
        Dim CS0007CheckAuthority As New CS0007CheckAuthority          'AUTHORmap

        '★★★ ボタン押下時、画面遷移（右） ★★★
        'ボタン押下時、画面遷移
        Dim WW_COUNT As Integer = e.Item.ItemIndex.ToString()
        Dim WW_URL As Label = Repeater_Menu_R.Items(WW_COUNT).FindControl("WF_MenuURL_R")
        Dim WW_VARI As Label = Repeater_Menu_R.Items(WW_COUNT).FindControl("WF_MenuVARI_R")
        Dim WW_MAPID As Label = Repeater_Menu_R.Items(WW_COUNT).FindControl("WF_MenuMAP_R")

        '○画面遷移権限チェック（右）
        CS0007CheckAuthority.MAPID = WW_MAPID.Text
        CS0007CheckAuthority.ROLECODE_MAP = Master.ROLE_MAP
        CS0007CheckAuthority.check()
        If isNormal(CS0007CheckAuthority.ERR) Then
            If CS0007CheckAuthority.MAPPERMITCODE = C_PERMISSION.REFERLANCE OrElse
               CS0007CheckAuthority.MAPPERMITCODE = C_PERMISSION.UPDATE Then
                CS0050Session.VIEW_PERMIT = CS0007CheckAuthority.MAPPERMITCODE
                CS0050Session.VIEW_MAPID = WW_MAPID.Text
                CS0050Session.VIEW_MAP_VARIANT = WW_VARI.Text
                CS0050Session.MAP_ETC = ""

                Master.MAPvariant = WW_VARI.Text
                Master.MAPID = WW_MAPID.Text
                Master.MAPpermitcode = CS0007CheckAuthority.MAPPERMITCODE
                Master.output(C_MESSAGE_NO.NORMAL, C_MESSAGE_TYPE.NOR)
                Master.showMessage()
            Else
                Master.output(C_MESSAGE_NO.AUTHORIZATION_ERROR, C_MESSAGE_TYPE.ABORT, "画面:" & WW_MAPID.Text)
                Master.showMessage()

                Exit Sub
            End If
        Else
            Master.output(CS0007CheckAuthority.ERR, C_MESSAGE_TYPE.ABORT, "画面:" & WW_MAPID.Text)
            Master.showMessage()

            Exit Sub
        End If

        'セッション変数クリア
        HttpContext.Current.Session("Selected_STYMD") = ""
        HttpContext.Current.Session("Selected_ENDYMD") = ""

        HttpContext.Current.Session("Selected_USERIDFrom") = ""
        HttpContext.Current.Session("Selected_USERIDTo") = ""
        HttpContext.Current.Session("Selected_USERIDG1") = ""
        HttpContext.Current.Session("Selected_USERIDG2") = ""
        HttpContext.Current.Session("Selected_USERIDG3") = ""
        HttpContext.Current.Session("Selected_USERIDG4") = ""
        HttpContext.Current.Session("Selected_USERIDG5") = ""

        HttpContext.Current.Session("Selected_MAPIDPFrom") = ""
        HttpContext.Current.Session("Selected_MAPIDPTo") = ""
        HttpContext.Current.Session("Selected_MAPIDPG1") = ""
        HttpContext.Current.Session("Selected_MAPIDPG2") = ""
        HttpContext.Current.Session("Selected_MAPIDPG3") = ""
        HttpContext.Current.Session("Selected_MAPIDPG4") = ""
        HttpContext.Current.Session("Selected_MAPIDPG5") = ""

        HttpContext.Current.Session("Selected_MAPIDFrom") = ""
        HttpContext.Current.Session("Selected_MAPIDTo") = ""
        HttpContext.Current.Session("Selected_MAPIDG1") = ""
        HttpContext.Current.Session("Selected_MAPIDG2") = ""
        HttpContext.Current.Session("Selected_MAPIDG3") = ""
        HttpContext.Current.Session("Selected_MAPIDG4") = ""
        HttpContext.Current.Session("Selected_MAPIDG5") = ""

        Server.Transfer(WW_URL.Text)

    End Sub
    ''' <summary>
    ''' 車両マスタ（申請）取得（車検切れ、容器検査切れ判定）  
    ''' </summary>
    ''' <param name="I_ORG">組織コード</param>
    ''' <param name="O_RTN">可否判定</param>
    ''' <remarks>
    ''' <para>S0001_TERMを検索　IPADDRを見る　TERMID取得</para>
    ''' <para>TERMIDを基にM00006_STRUCTを検索</para>
    ''' <para >部署を基に運用ガイダンス表示</para>
    ''' </remarks>
    Private Sub GetSHARYOC(ByVal I_ORG As String, ByRef O_RTN As String)
        Dim CS0009MESSAGEout As New CS0009MESSAGEout        'Message out
        Dim CS0011LOGWRITE As New CS0011LOGWrite            'LogOutput DirString Get
        Dim GS0007FIXVALUElst As New GS0007FIXVALUElst      'FIXVALUE Get

        '○ ユーザ
        Try
            O_RTN = C_MESSAGE_NO.NORMAL

            Dim iTbl As DataTable = New DataTable
            iTbl.Columns.Add("SHARYOTYPE", GetType(String))                    '車両タイプ
            iTbl.Columns.Add("TSHABAN", GetType(String))                       '車両番号
            iTbl.Columns.Add("LICNPLTNO1", GetType(String))                    '登録番号１
            iTbl.Columns.Add("LICNPLTNO2", GetType(String))                    '登録番号２
            iTbl.Columns.Add("INSKBN", GetType(String))                        '検査区分
            iTbl.Columns.Add("LICNYMD", GetType(Date))                         '車検有効期限
            iTbl.Columns.Add("OTNKTINSNYMD", GetType(Date))                    '石油気密検査
            iTbl.Columns.Add("HPRSINSNYMD", GetType(Date))                     '高圧容器再検査
            iTbl.Columns.Add("SORTYMD", GetType(String))                       'ソート用年月日
            iTbl.Columns.Add("MSG", GetType(String))                           'メッセージ

            Dim SQLStr0 As String =
                     " SELECT                                                                                                " _
                   & "         Z.TERMID           as TERMID                                                                  " _
                   & "       , Z.TERMCAMP         as COMPCODE                                                                " _
                   & " FROM     S0001_TERM Z                                                                                 " _
                   & " WHERE                                                                                                 " _
                   & "         Z.TERMORG           = @P01                                                                    " _
                   & "   and   Z.TERMCLASS         = @P04                                                                    " _
                   & "   and   Z.STYMD            <= @P02                                                                    " _
                   & "   and   Z.ENDYMD           >= @P02                                                                    " _
                   & "   and   Z.DELFLG           <> @P03                                                                    "

            Dim SQLStr As String =
                     " SELECT  isnull(rtrim(A.SHARYOTYPE),'')              as SHARYOTYPE                                     " _
                   & "      ,  isnull(rtrim(A.TSHABAN),'')                  as TSHABAN                                       " _
                   & "      ,  isnull(rtrim(C.LICNPLTNO1),'')               as LICNPLTNO1                                    " _
                   & "      ,  isnull(rtrim(C.LICNPLTNO2),'')               as LICNPLTNO2                                    " _
                   & "      ,  isnull(rtrim(format(C.LICNYMD,'yyyy/MM/dd')),'" & C_DEFAULT_YMD & "') as LICNYMD              " _
                   & "      ,  isnull(rtrim(format(C.OTNKTINSNYMD,'yyyy/MM/dd')),'" & C_DEFAULT_YMD & "') as OTNKTINSNYMD    " _
                   & "      ,  isnull(rtrim(format(C.HPRSINSNYMD,'yyyy/MM/dd')),'" & C_DEFAULT_YMD & "') as HPRSINSNYMD      " _
                   & "      ,  isnull(rtrim(C.INSKBN),'')                   as INSKBN                                        " _
                   & "      ,  ''                                           as SORTYMD                                       " _
                   & "      ,  ''                                           as MSG                                           " _
                   & " FROM       MA002_SHARYOA                             A                                                " _
                   & " INNER JOIN M0006_STRUCT                              S                                         ON     " _
                   & "         S.STRUCT            = @P01                                                                    " _
                   & "   and   S.CAMPCODE          = @P05                                                                    " _
                   & "   and   S.OBJECT            = @P04                                                                    " _
                   & "   and   S.CODE              = A.MANGSORG                                                              " _
                   & "   and   S.STYMD            <= @P02                                                                    " _
                   & "   and   S.ENDYMD           >= @P02                                                                    " _
                   & "   and   S.DELFLG           <> @P03                                                                    " _
                   & " INNER JOIN MA003_SHARYOB                             B                                         ON     " _
                   & "         B.CAMPCODE          = A.CAMPCODE                                                              " _
                   & "   and   B.SHARYOTYPE        = A.SHARYOTYPE                                                            " _
                   & "   and   B.TSHABAN           = A.TSHABAN                                                               " _
                   & "   and   B.STYMD            <= @P02                                                                    " _
                   & "   and   B.ENDYMD           >= @P02                                                                    " _
                   & "   and   B.DELFLG           <> @P03                                                                    " _
                   & " LEFT  JOIN MA004_SHARYOC                             C                                         ON     " _
                   & "         C.CAMPCODE          = A.CAMPCODE                                                              " _
                   & "   and   C.SHARYOTYPE        = A.SHARYOTYPE                                                            " _
                   & "   and   C.TSHABAN           = A.TSHABAN                                                               " _
                   & "   and   C.STYMD            <= B.ENDYMD                                                                " _
                   & "   and   C.ENDYMD           >= B.STYMD                                                                 " _
                   & "   and   C.ENDYMD            = (                                                                       " _
                   & "           select   max(ENDYMD)                                                                        " _
                   & "           from     MA004_SHARYOC                                                                      " _
                   & "           where    CAMPCODE    = A.CAMPCODE                                                           " _
                   & "             and    SHARYOTYPE  = A.SHARYOTYPE                                                         " _
                   & "             and    TSHABAN     = A.TSHABAN                                                            " _
                   & "             and    STYMD      <= B.ENDYMD                                                             " _
                   & "             and    ENDYMD     >= B.STYMD                                                              " _
                   & "             and    DELFLG     <> '1'                                                                  " _
                   & "          )                                                                                            " _
                   & "   and   C.DELFLG           <> @P03                                                                    " _
                   & " WHERE                                                                                                 " _
                   & "         A.STYMD            <= @P02                                                                    " _
                   & "   and   A.ENDYMD           >= @P02                                                                    " _
                   & "   and   A.DELFLG           <> @P03                                                                    " _
                   & " ORDER BY C.INSKBN, A.SHARYOTYPE, A.TSHABAN                                                            "

            'DataBase接続文字
            Using SQLcon As SqlConnection = CS0050Session.getConnection
                SQLcon.Open() 'DataBase接続(Open)

                Using SQLcmd0 As New SqlCommand(SQLStr0, SQLcon)
                    Dim PARA1 As SqlParameter = SQLcmd0.Parameters.Add("@P01", System.Data.SqlDbType.NVarChar, 20)
                    Dim PARA2 As SqlParameter = SQLcmd0.Parameters.Add("@P02", System.Data.SqlDbType.Date)
                    Dim PARA3 As SqlParameter = SQLcmd0.Parameters.Add("@P03", System.Data.SqlDbType.NVarChar, 1)
                    Dim PARA4 As SqlParameter = SQLcmd0.Parameters.Add("@P04", System.Data.SqlDbType.NVarChar, 1)

                    PARA1.Value = I_ORG
                    PARA2.Value = Date.Now
                    PARA3.Value = C_DELETE_FLG.DELETE
                    PARA4.Value = C_TERMCLASS.CLIENT

                    Dim SQLdr As SqlDataReader = SQLcmd0.ExecuteReader()
                    If SQLdr.Read Then
                        WF_TERMID.Text = SQLdr("TERMID")
                        WF_TERMCAMP.Text = SQLdr("COMPCODE")
                    End If

                    'Close
                    SQLdr.Close() 'Reader(Close)
                    SQLdr = Nothing

                End Using
                Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                    Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P01", System.Data.SqlDbType.NVarChar, 20)
                    Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P02", System.Data.SqlDbType.Date)
                    Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P03", System.Data.SqlDbType.NVarChar, 1)
                    Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P04", System.Data.SqlDbType.NVarChar, 20)
                    Dim PARA5 As SqlParameter = SQLcmd.Parameters.Add("@P05", System.Data.SqlDbType.NVarChar, 20)

                    PARA1.Value = WF_TERMID.Text
                    PARA2.Value = Date.Now
                    PARA3.Value = C_DELETE_FLG.DELETE
                    PARA4.Value = "SYARYOCHK"
                    PARA5.Value = WF_TERMCAMP.Text

                    Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                    iTbl.Load(SQLdr)

                    'Close
                    SQLdr.Close() 'Reader(Close)
                    SQLdr = Nothing

                End Using
            End Using

            Dim oTbl As DataTable = iTbl.Clone
            Dim oRow As DataRow = Nothing

            For Each iRow As DataRow In iTbl.Rows

                '車検チェック
                If iRow("SHARYOTYPE") = "A" OrElse
                   iRow("SHARYOTYPE") = "C" OrElse
                   iRow("SHARYOTYPE") = "D" Then
                    If IsDate(iRow("LICNYMD")) Then
                        If iRow("LICNYMD") <> C_DEFAULT_YMD Then
                            Dim WW_days As String = DateDiff("d", Date.Now, CDate(iRow("LICNYMD")))
                            If CDate(iRow("LICNYMD")) < Date.Now Then
                                '車検切れ
                                oRow = oTbl.NewRow()
                                oRow.ItemArray = iRow.ItemArray
                                oRow("SORTYMD") = CDate(iRow("LICNYMD")).ToString("yyyy/MM/dd")
                                oRow("MSG") = "車検切れ"
                                oTbl.Rows.Add(oRow)
                            ElseIf CDate(iRow("LICNYMD")).AddMonths(-1) < Date.Now Then
                                '1カ月前から警告
                                oRow = oTbl.NewRow()
                                oRow.ItemArray = iRow.ItemArray
                                oRow("SORTYMD") = CDate(iRow("LICNYMD")).ToString("yyyy/MM/dd")
                                oRow("MSG") = "車検" & WW_days & "日前"
                                oTbl.Rows.Add(oRow)
                            End If
                        End If
                    End If
                End If
                '容器チェック
                If iRow("INSKBN") = "1" Then
                    If iRow("SHARYOTYPE") = "B" OrElse
                       iRow("SHARYOTYPE") = "D" Then
                        If IsDate(iRow("OTNKTINSNYMD")) Then
                            If iRow("OTNKTINSNYMD") <> C_DEFAULT_YMD Then
                                Dim WW_days As String = DateDiff("d", Date.Now, CDate(iRow("OTNKTINSNYMD")))
                                If CDate(iRow("OTNKTINSNYMD")) < Date.Now Then
                                    '容器検査切れ
                                    oRow = oTbl.NewRow()
                                    oRow.ItemArray = iRow.ItemArray
                                    oRow("SORTYMD") = CDate(iRow("OTNKTINSNYMD")).ToString("yyyy/MM/dd")
                                    oRow("MSG") = "石油気密検査切れ"
                                    oTbl.Rows.Add(oRow)
                                ElseIf CDate(iRow("OTNKTINSNYMD")).AddMonths(-2) < Date.Now Then
                                    '2カ月前から警告
                                    oRow = oTbl.NewRow()
                                    oRow.ItemArray = iRow.ItemArray
                                    oRow("SORTYMD") = CDate(iRow("OTNKTINSNYMD")).ToString("yyyy/MM/dd")
                                    oRow("MSG") = "石油気密検査" & WW_days & "日前"
                                    oTbl.Rows.Add(oRow)
                                End If
                            End If
                        End If
                    End If
                ElseIf iRow("INSKBN") = "2" Then
                    If iRow("SHARYOTYPE") = "B" OrElse
                       iRow("SHARYOTYPE") = "D" Then
                        If IsDate(iRow("HPRSINSNYMD")) Then
                            If iRow("HPRSINSNYMD") <> C_DEFAULT_YMD Then
                                Dim WW_days As String = DateDiff("d", Date.Now, CDate(iRow("HPRSINSNYMD")))
                                If CDate(iRow("HPRSINSNYMD")) < Date.Now Then
                                    '容器検査切れ
                                    oRow = oTbl.NewRow()
                                    oRow.ItemArray = iRow.ItemArray
                                    oRow("SORTYMD") = CDate(iRow("HPRSINSNYMD")).ToString("yyyy/MM/dd")
                                    oRow("MSG") = "高圧容器再検査切れ"
                                    oTbl.Rows.Add(oRow)
                                ElseIf CDate(iRow("HPRSINSNYMD")).AddMonths(-2) < Date.Now Then
                                    '2カ月前から警告
                                    oRow = oTbl.NewRow()
                                    oRow.ItemArray = iRow.ItemArray
                                    oRow("SORTYMD") = CDate(iRow("HPRSINSNYMD")).ToString("yyyy/MM/dd")
                                    oRow("MSG") = "高圧容器再検査" & WW_days & "日前"
                                    oTbl.Rows.Add(oRow)
                                End If
                            End If
                        End If
                    End If
                End If
            Next

            'ソート
            Dim WW_TBLview As DataView
            WW_TBLview = New DataView(oTbl)
            WW_TBLview.Sort = "SORTYMD, SHARYOTYPE, TSHABAN"
            oTbl = WW_TBLview.ToTable

            For Each wRow As DataRow In oTbl.Rows
                WF_Guidance.Text = WF_Guidance.Text & "・"
                WF_Guidance.Text = WF_Guidance.Text & wRow("MSG")
                WF_Guidance.Text = WF_Guidance.Text & " （"
                WF_Guidance.Text = WF_Guidance.Text & wRow("LICNPLTNO1")
                WF_Guidance.Text = WF_Guidance.Text & wRow("LICNPLTNO2")
                WF_Guidance.Text = WF_Guidance.Text & " "
                WF_Guidance.Text = WF_Guidance.Text & wRow("SHARYOTYPE")
                WF_Guidance.Text = WF_Guidance.Text & wRow("TSHABAN")
                WF_Guidance.Text = WF_Guidance.Text & " "
                WF_Guidance.Text = WF_Guidance.Text & wRow("SORTYMD")
                WF_Guidance.Text = WF_Guidance.Text & "）"
                WF_Guidance.Text = WF_Guidance.Text & "<br />"
            Next
            If oTbl.Rows.Count > 0 Then
                WF_Guidance.Text = WF_Guidance.Text & "<br />"
            End If

            WW_TBLview.Dispose()
            WW_TBLview = Nothing

            iTbl.Dispose()
            iTbl = Nothing

            oTbl.Dispose()
            oTbl = Nothing
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "S0012_SRVAUTHOR SELECT")
            CS0011LOGWRITE.INFSUBCLASS = "GetSHARYOC"                   'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:S0012_SRVAUTHOR SELECT"          '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                  '
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            O_RTN = C_MESSAGE_NO.DB_ERROR
        End Try

    End Sub

    ''' <summary>
    ''' 従業員マスタ取得  
    ''' </summary>
    ''' <param name="I_USERID">ユーザーID</param>
    ''' <param name="O_CAMP">会社コード</param>
    ''' <param name="O_ORG">部署</param>
    ''' <param name="O_RTN">結果</param>
    ''' <remarks>
    ''' <para>ログインユーザーの会社コードと所属部署を取得</para>
    ''' </remarks>
    Private Sub GetSTAFF(ByVal I_USERID As String, ByRef O_CAMP As String, ByRef O_ORG As String, ByRef O_RTN As String)
        Dim CS0009MESSAGEout As New CS0009MESSAGEout        'Message out
        Dim CS0011LOGWRITE As New CS0011LOGWrite            'LogOutput DirString Get
        Dim GS0007FIXVALUElst As New GS0007FIXVALUElst      'FIXVALUE Get

        '○ ユーザ
        Try
            O_RTN = C_MESSAGE_NO.NORMAL
            O_CAMP = ""
            O_ORG = ""

            Dim SQLStr As String =
                     " SELECT            " _
                   & "        CAMPCODE   " _
                   & "       ,ORG        " _
                   & " FROM   S0004_USER " _
                   & " WHERE                                                                                                 " _
                   & "         USERID    = @P01 " _
                   & "   and   STYMD    <= @P02 " _
                   & "   and   ENDYMD   >= @P02 " _
                   & "   and   DELFLG   <> @P03 "

            'DataBase接続文字
            Using SQLcon As SqlConnection = CS0050Session.getConnection
                SQLcon.Open() 'DataBase接続(Open)

                Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                    Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P01", System.Data.SqlDbType.NVarChar, 20)
                    Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P02", System.Data.SqlDbType.Date)
                    Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P03", System.Data.SqlDbType.NVarChar, 1)

                    PARA1.Value = I_USERID
                    PARA2.Value = Date.Now
                    PARA3.Value = C_DELETE_FLG.DELETE

                    Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                    If SQLdr.Read Then
                        O_CAMP = SQLdr("CAMPCODE")
                        O_ORG = SQLdr("ORG")
                    End If

                    'Close
                    SQLdr.Close() 'Reader(Close)
                    SQLdr = Nothing

                End Using
            End Using

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "S0004_USER SELECT")
            CS0011LOGWRITE.INFSUBCLASS = "GetSTAFF"                   'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:S0004_USER SELECT"           '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT               '
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            O_RTN = C_MESSAGE_NO.DB_ERROR
        End Try

    End Sub

End Class