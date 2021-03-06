﻿Imports System.Data.SqlClient
Imports System.Net

Public Class M00000LOGON
    Inherits System.Web.UI.Page

    Private CS0050Session As New CS0050SESSION                  'セッション情報

    Private Const MAPID As String = "M00000"                    '画面ID

    Private Const C_MAX_MISS_PASSWORD_COUNT As Integer = 6      'パスワード入力失敗の最大回数
    ''' <summary>
    ''' サーバー処理の遷移先
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

        '■■■　初期処理　■■■
        '共通セッション情報
        '   Namespace     : 名称空間(プロジェクト名)
        '   Class         : クラス(プロジェクト直下のクラス)
        '   Userid        : ユーザID
        '   APSRVname     : APサーバー名称
        '   APSRVCamp     : APサーバー設置会社(全社サーバー："＊"、個別設置サーバー：会社)
        '   APSRVOrg      : APサーバー設置部署(全社サーバー："＊"、個別設置サーバー：部署)
        '   MOrg          : 管理部署(営業部、支店レベル)
        '   Term          : 操作端末(端末操作情報として利用)
        '   TermCamp      : 操作端末会社(端末操作情報として利用)
        '   TermORG       : 操作端末部署(端末操作情報として利用)
        '   Selected_CAMPCODE   : 画面選択会社コード
        '   Selected_STYMD      : 画面選択
        '   Selected_ENDYMD     : 画面選択
        '   Selected_USERIDFrom : 画面選択
        '   Selected_USERIDTo   : 画面選択
        '   Selected_USERIDG1   : 画面選択
        '   Selected_USERIDG2   : 画面選択
        '   Selected_USERIDG3   : 画面選択
        '   Selected_USERIDG4   : 画面選択
        '   Selected_USERIDG5   : 画面選択
        '   Selected_MAPIDPFrom : 画面選択
        '   Selected_MAPIDPTo   : 画面選択
        '   Selected_MAPIDPG1   : 画面選択
        '   Selected_MAPIDPG2   : 画面選択
        '   Selected_MAPIDPG3   : 画面選択
        '   Selected_MAPIDPG4   : 画面選択
        '   Selected_MAPIDPG5   : 画面選択
        '   Selected_MAPIDFrom  : 画面選択
        '   Selected_MAPIDTo    : 画面選択
        '   Selected_MAPIDG1    : 画面選択
        '   Selected_MAPIDG2    : 画面選択
        '   Selected_MAPIDG3    : 画面選択
        '   Selected_MAPIDG4    : 画面選択
        '   Selected_MAPIDG5    : 画面選択

        '   DBcon         : DB接続文字列 
        '   LOGdir        : ログ出力ディレクトリ 
        '   PDFdir        : PDF用ワークのディレクトリ
        '   FILEdir       : FILE格納ディレクトリ
        '   JNLdir        : 更新ジャーナル格納ディレクトリ

        '   MAPmapid      : 画面間IF(MAPID)
        '   MAPvariant    : 画面間IF(変数)
        '   MAPpermitcode : 画面間IF(権限)
        '   MAPetc        : 画面間IF(各PRGで利用)
        '   DRIVERS       : 事務用URL：初期URL(=htt://xxxx/OFFICE)、乗務員用URL：初期URL(=htt://xxxx/DRIVERS)

        If IsPostBack Then
            PassWord.Attributes.Add("value", PassWord.Text)

            Dim CS001INIFILE As New CS0001INIFILEget            'INIファイル読み込み
            CS001INIFILE.CS0001INIFILEget()
            If Not isNormal(CS001INIFILE.ERR) Then
                Master.Output(CS001INIFILE.ERR, C_MESSAGE_TYPE.ABORT)
                Exit Sub
            End If

            '■■■ 各ボタン押下処理 ■■■
            If Not String.IsNullOrEmpty(WF_ButtonClick.Value) Then
                Select Case WF_ButtonClick.Value
                    Case "WF_ButtonOK"
                        WF_ButtonOK_Click(sender, e)
                End Select
            End If
        Else
            '〇初期化処理
            Initialize()
        End If

        Master.LOGINCOMP = WF_TERMCAMP.Text
    End Sub

    ''' <summary>
    ''' 初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub Initialize()
        '■■■　セッション変数設定　■■■
        Dim CS001INIFILE As New CS0001INIFILEget            'INIファイル読み込み
        Dim CS0006TERMchk As New CS0006TERMchk              'ローカルコンピュータ名存在チェック
        Dim CS0008ONLINEstat As New CS0008ONLINEstat        'ONLINE状態
        Dim CS0011LOGWRITE As New CS0011LOGWrite            'LogOutput DirString Get
        Master.dispHelp = False

        Master.MAPID = MAPID
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
            Master.output(CS001INIFILE.ERR, C_MESSAGE_TYPE.ABORT)
            Exit Sub
        End If

        '○ APサーバー情報からAPサーバー設置会社(APSRVCamp)、APサーバー設置部署(APSRVOrg)取得
        CS0006TERMchk.TERMID = CS0050Session.APSV_ID
        CS0006TERMchk.CS0006TERMchk()
        If isNormal(CS0006TERMchk.ERR) Then
            CS0050Session.APSV_COMPANY = CS0006TERMchk.TERMCAMP
            CS0050Session.APSV_ORG = CS0006TERMchk.TERMORG
            CS0050Session.APSV_M_ORG = CS0006TERMchk.MORG
        Else
            Master.output(CS0006TERMchk.ERR, C_MESSAGE_TYPE.ABORT, "CS0006TERMchk")
            Exit Sub
        End If


        '〇クライアント端末のIPを取得する
        Dim ClientIP As String = ""
        Try

            ClientIP = Request.UserHostAddress
            'Dim ClientIphEntry As IPHostEntry = Dns.GetHostEntry(ClientIP)
            'For Each ipAddr As IPAddress In ClientIphEntry.AddressList
            '    'IPv4の場合
            '    If ipAddr.AddressFamily = Sockets.AddressFamily.InterNetwork Then
            '        ClientIP = ipAddr.ToString
            '    End If
            'Next
        Catch ex As Exception
            Master.output(C_MESSAGE_NO.SYSTEM_ADM_ERROR, C_MESSAGE_TYPE.ABORT, "クライアントI IP取得失敗")
            CS0011LOGWRITE.INFSUBCLASS = "Main"
            CS0011LOGWRITE.INFPOSI = "クライアントIP取得失敗"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.SYSTEM_ADM_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()

            Exit Sub

        End Try

        Dim ClientIP3 As String = ""
        If ClientIP.LastIndexOf(".") < 0 Then
            ClientIP3 = ""
        Else
            ClientIP3 = Mid(ClientIP, 1, ClientIP.LastIndexOf("."))
        End If
        '■■■　運用ガイダンス表示　■■■

        WF_Guidance.Text = ""
        '○ 車検切れ、容器検査切れ車両の検索表示（運用ガイダンスに表示）d
        Dim WW_RTN As String = C_MESSAGE_NO.NORMAL

        'GetSHARYOC(WW_RTN, ClientIP3)
        'If Not isNormal(WW_RTN) Then Exit Sub

        '■■■　オンラインサービス判定　■■■

        '○オンラインサービス停止なら画面遷移しない 
        '接続サーバ（INIファイルのサーバ）、対象会社がオンラインか確認

        CS0008ONLINEstat.COMPCODE = WF_TERMCAMP.Text
        CS0008ONLINEstat.CS0008ONLINEstat()
        If isNormal(CS0008ONLINEstat.ERR) Then
            If CS0008ONLINEstat.ONLINESW = 0 Then
                Master.output(C_MESSAGE_NO.CLOSED_SERVICE, C_MESSAGE_TYPE.ERR)
                'WF_Guidance.Text = String.Empty
                Exit Sub
            Else
                'WF_Guidance.Text = WF_Guidance.Text & CS0008ONLINEstat.TEXT.Replace(vbCrLf, "<br />")
            End If
        Else
            Master.output(CS0008ONLINEstat.ERR, C_MESSAGE_TYPE.ABORT, "CS0008ONLINEstat")
            Exit Sub
        End If


        '■■■ 初期画面表示 ■■■

        '○パソコン名存在チェック
        ' ホスト名を取得する
        Dim WW_ipAddress As Object
        Dim WW_hostName As Object

        Try
            WW_ipAddress = Request.ServerVariables("REMOTE_HOST")
            WW_hostName = System.Net.Dns.GetHostEntry(WW_ipAddress).HostName()
            If InStr(WW_hostName.ToString, ".") = 0 Then
                CS0006TERMchk.TERMID = WW_hostName.ToString
            Else
                CS0006TERMchk.TERMID = Mid(WW_hostName.ToString, 1, InStr(WW_hostName.ToString, ".") - 1)
            End If


        Catch ex As Exception
            CS0006TERMchk.TERMID = Environment.MachineName                                       'サーバ名
        End Try

        CS0006TERMchk.TERMID = CS0050Session.APSV_ID

        CS0006TERMchk.CS0006TERMchk()
        If isNormal(CS0006TERMchk.ERR) Then
            CS0050Session.TERMID = CS0006TERMchk.TERMID
            CS0050Session.TERM_COMPANY = CS0006TERMchk.TERMCAMP
            CS0050Session.TERM_ORG = CS0006TERMchk.TERMORG
            CS0050Session.TERM_M_ORG = CS0006TERMchk.MORG
        Else
            Master.output(CS0006TERMchk.ERR, C_MESSAGE_TYPE.ABORT, "CS0006TERMchk")
            Exit Sub
        End If


        '■■■　初期メッセージ表示　■■■
        Master.output(C_MESSAGE_NO.INPUT_ID_PASSWD, C_MESSAGE_TYPE.INF)

        'C:\APPL\APPLFILES\XML_TMPディレクトリの不要データを掃除
        Dim WW_File As String

        For Each tempFile As String In System.IO.Directory.GetFiles( _
            CS0050Session.UPLOAD_PATH & "\XML_TMP", "*", System.IO.SearchOption.AllDirectories)
            ' ファイルパスからファイル名を取得
            WW_File = tempFile
            Do
                WW_File = Mid(WW_File, InStr(WW_File, "\") + 1, 200)
            Loop Until InStr(WW_File, "\") = 0

            '本日作成以外のファイルは削除
            If Mid(WW_File, 1, 8) <> Date.Now.ToString("yyyyMMdd") Then System.IO.File.Delete(tempFile)
        Next
        UserID.Focus()

    End Sub
    ''' <summary>
    '''　OKボタン押下時処理
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonOK_Click(sender As Object, e As EventArgs)

        '■■■　初期処理　■■■

        '○共通宣言
        '*共通関数宣言(APPLDLL)
        Dim CS0011LOGWRITE As New CS0011LOGWrite            'LogOutput DirString Get
        Dim CS0009MESSAGEout As New CS0009MESSAGEout        'メッセージ出力 out
        Dim CS0006TERMchk As New CS0006TERMchk              'ローカルコンピュータ名存在チェック
        Dim CS0008ONLINEstat As New CS0008ONLINEstat        'ONLINE状態

        '○オンラインサービス判定
        '画面UserIDの会社からDB(T0001_ONLINESTAT)検索
        CS0008ONLINEstat.CS0008ONLINEstat()
        If isNormal(CS0008ONLINEstat.ERR) Then
            'オンラインサービス停止時、ログオン画面へ遷移
            If CS0008ONLINEstat.ONLINESW = 0 Then
                Master.Output(C_MESSAGE_NO.CLOSED_SERVICE, C_MESSAGE_TYPE.ERR)
                Exit Sub
            End If
        Else
            Master.output(CS0008ONLINEstat.ERR, C_MESSAGE_TYPE.ABORT, "CS0008ONLINEstat")
            Exit Sub
        End If

        '■■■　メイン処理　■■■
        '〇ID、パスワードのいずれかが未入力なら抜ける
        If String.IsNullOrEmpty(UserID.Text) OrElse String.IsNullOrEmpty(PassWord.Text) Then Exit Sub

        '○ 入力文字内の禁止文字排除
        '   画面UserID内の使用禁止文字排除
        Master.eraseCharToIgnore(UserID.Text)
        Master.eraseCharToIgnore(PassWord.Text)

        '○ 画面UserIDのDB(S0004_USER)存在チェック
        Dim WW_USERID As String = String.Empty
        Dim WW_PASSWORD As String = String.Empty
        Dim WW_ORG As String = String.Empty
        Dim WW_STYMD As Date = Date.Now
        Dim WW_ENDYMD As Date = Date.Now
        Dim WW_MISSCNT As Integer = 0
        Dim WW_UPDYMD As Date
        Dim WW_UPDTIMSTP As Byte()
        Dim WW_MAPID As String = String.Empty
        Dim WW_VARIANT As String = String.Empty
        Dim WW_PASSENDYMD As String = String.Empty
        Dim WW_err As String = String.Empty
        Dim WW_RTN As String = String.Empty
        Dim WW_LOGONYMD As String = Date.Now.ToString("yyyy/MM/dd")
        Dim WW_URL As String = String.Empty
        Dim WW_MENUURL As String = String.Empty
        'DataBase接続文字
        Using SQLcon As SqlConnection = CS0050Session.getConnection
            SQLcon.Open() 'DataBase接続(Open)

            Try
                'S0004_USER検索SQL文
                Dim SQL_Str As String = _
                     "SELECT " _
                   & " rtrim(A.USERID)   as USERID    , " _
                   & " rtrim(A.ORG)      as ORG       , " _
                   & " A.STYMD                        , " _
                   & " A.ENDYMD                       , " _
                   & " rtrim(B.PASSWORD) as PASSWORD  , " _
                   & " B.MISSCNT                      , " _
                   & " A.INITYMD                      , " _
                   & " A.UPDYMD                       , " _
                   & " A.UPDTIMSTP                    , " _
                   & " rtrim(A.MAPID)    as MAPID     , " _
                   & " rtrim(A.VARIANT)  as VARIANT   , " _
                   & " B.PASSENDYMD      as PASSENDYMD  " _
                   & " FROM       S0004_USER       A    " _
                   & " INNER JOIN S0014_USERPASS   B ON " _
                   & "       B.USERID      = A.USERID   " _
                   & "   and B.DELFLG     <> @P4        " _
                   & " Where A.USERID      = @P1        " _
                   & "   and A.STYMD      <= @P2        " _
                   & "   and A.ENDYMD     >= @P3        " _
                   & "   and B.PASSENDYMD >= @P3        " _
                   & "   and A.DELFLG     <> @P4        "
                Using SQLcmd As New SqlCommand(SQL_Str, SQLcon)
                    Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.NVarChar, 20)
                    Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.Date)
                    Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.Date)
                    Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", System.Data.SqlDbType.NVarChar, 1)
                    PARA1.Value = UserID.Text
                    PARA2.Value = Date.Now
                    PARA3.Value = Date.Now
                    PARA4.Value = C_DELETE_FLG.DELETE
                    Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

                    WW_err = C_MESSAGE_NO.UNMATCH_ID_PASSWD_ERROR
                    If SQLdr.Read Then
                        WW_USERID = SQLdr("USERID")
                        WW_PASSWORD = SQLdr("PASSWORD")
                        WW_ORG = SQLdr("ORG")
                        WW_STYMD = SQLdr("STYMD")
                        WW_ENDYMD = SQLdr("ENDYMD")
                        WW_MISSCNT = SQLdr("MISSCNT")
                        WW_UPDYMD = SQLdr("UPDYMD")
                        WW_UPDTIMSTP = SQLdr("UPDTIMSTP")
                        WW_MAPID = SQLdr("MAPID")
                        WW_VARIANT = SQLdr("VARIANT")
                        WW_PASSENDYMD = SQLdr("PASSENDYMD")
                        WW_err = C_MESSAGE_NO.NORMAL
                    End If

                    'Close
                    SQLdr.Close() 'Reader(Close)
                    SQLdr = Nothing

                End Using

            Catch ex As Exception
                Master.output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "S0004_USER SELECT")

                CS0011LOGWRITE.INFSUBCLASS = "Main"                         'SUBクラス名
                CS0011LOGWRITE.INFPOSI = "S0004_USER SELECT"                           '
                CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
                CS0011LOGWRITE.TEXT = ex.ToString()
                CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR 'DBエラー。
                CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
                Exit Sub
            End Try

            'ユーザID誤り
            If Not isNormal(WW_err) OrElse _
                UserID.Text = C_DEFAULT_DATAKEY OrElse _
                UserID.Text = "INIT" Then
                Master.output(C_MESSAGE_NO.UNMATCH_ID_PASSWD_ERROR, C_MESSAGE_TYPE.ERR)
                CS0011LOGWRITE.INFSUBCLASS = "Main"
                CS0011LOGWRITE.INFPOSI = "パスワードERR USERID ERR"
                CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ERR
                CS0011LOGWRITE.TEXT = "(USERID=" & UserID.Text & "、PASS=" & PassWord.Text & ")"
                CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.UNMATCH_ID_PASSWD_ERROR 'ユーザID、パスワードに誤りがあります(1)。
                CS0011LOGWRITE.CS0011LOGWrite()
                UserID.Focus()
                Exit Sub
            End If

            '○ パスワードチェック
            'ユーザあり　かつ　(パスワード誤り　または　パスワード6回以上誤り)
            If (PassWord.Text <> WW_PASSWORD OrElse WW_MISSCNT >= C_MAX_MISS_PASSWORD_COUNT) Then
                Master.output(C_MESSAGE_NO.UNMATCH_ID_PASSWD_ERROR, C_MESSAGE_TYPE.ERR)
                CS0011LOGWRITE.INFSUBCLASS = "Main"
                CS0011LOGWRITE.INFPOSI = "パスワードERR、MAX回数"
                CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ERR
                CS0011LOGWRITE.TEXT = "(USERID=" & UserID.Text & "、PASS=" & PassWord.Text & ")"
                CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.UNMATCH_ID_PASSWD_ERROR 'ユーザID、パスワードに誤りがあります(2)。
                CS0011LOGWRITE.CS0011LOGWrite()
                'パスワードエラー回数のカウントUP
                Try
                    'S0014_USER更新SQL文
                    Dim SQL_Str As String = _
                         "Update S0014_USERPASS " _
                       & "Set    MISSCNT = @P1 , UPDYMD = @P2 , UPDUSER = @P3 " _
                       & "Where  USERID  = @P3 "
                    Using SQLcmd As New SqlCommand(SQL_Str, SQLcon)
                        Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.Int)
                        Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.DateTime)
                        Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.NVarChar, 20)
                        If WW_MISSCNT = 999 Then
                            PARA1.Value = WW_MISSCNT
                        Else
                            PARA1.Value = WW_MISSCNT + 1
                        End If
                        PARA2.Value = Date.Now
                        PARA3.Value = UserID.Text
                        SQLcmd.ExecuteNonQuery()

                    End Using
                Catch ex As Exception
                    Master.output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "S0014_USERPASS UPDATE")
                    CS0011LOGWRITE.INFSUBCLASS = "Main"
                    CS0011LOGWRITE.INFPOSI = "S0014_USERPASS Update"
                    CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
                    CS0011LOGWRITE.TEXT = ex.ToString()
                    CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR 'DBエラー。
                    CS0011LOGWRITE.CS0011LOGWrite()
                End Try
                UserID.Focus()
                Exit Sub

            End If

            '○ パスワードチェックＯＫ時処理
            'セッション情報（ユーザＩＤ）設定
            CS0050Session.USERID = UserID.Text

            'ミスカウントクリア
            Try
                'S0014_USER更新SQL文
                Dim SQL_Str As String = _
                     "Update S0014_USERPASS " _
                   & "Set    MISSCNT = @P1 , UPDYMD = @P2 , UPDUSER = @P3 " _
                   & "Where  USERID  = @P3 "
                Using SQLcmd As New SqlCommand(SQL_Str, SQLcon)
                    Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.Int)
                    Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.DateTime)
                    Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.NVarChar, 20)
                    PARA1.Value = 0
                    PARA2.Value = Date.Now
                    PARA3.Value = UserID.Text
                    SQLcmd.ExecuteNonQuery()

                End Using
            Catch ex As Exception
                Master.output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "S0014_USERPASS UPDATE")

                CS0011LOGWRITE.INFSUBCLASS = "Main"
                CS0011LOGWRITE.INFPOSI = "S0014_USERPASS Update"
                CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
                CS0011LOGWRITE.TEXT = ex.ToString()
                CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR 'DBエラー。
                CS0011LOGWRITE.CS0011LOGWrite()
                Exit Sub
            End Try

            '■■■　終了処理　■■■

            '○ パスワードチェックＯＫ時、指定画面へ遷移
            'ユーザマスタより、MAPIDおよびVARIANTを取得

            Try
                If WW_PASSENDYMD <= Date.Now.AddDays(7).ToString("yyyy/MM/dd") Then
                    'パスワード登録画面（1週間前）の場合
                    GetURL(WW_PASSENDYMD, "CO0014", WW_URL)
                    GetURL(WW_PASSENDYMD, WW_MAPID, WW_MENUURL)
                Else
                    GetURL(WW_PASSENDYMD, WW_MAPID, WW_URL)
                End If

            Catch ex As Exception
                Master.output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "S0009_URL SELECT")
                CS0011LOGWRITE.INFSUBCLASS = "Main"                         'SUBクラス名
                CS0011LOGWRITE.INFPOSI = "S0009_URL SELECT"
                CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
                CS0011LOGWRITE.TEXT = ex.ToString()
                CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR 'DBエラー。
                CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
                Exit Sub
            End Try

            '★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★
            'デバッグ時は、
            ' ①ログオン日付更新処理をコメントアウトする（リコンパイル）
            ' ②S0020_LOGONYMDテーブルの該当SRV（TERMID）のログオン日付をテスト対象日に手修正
            '
            '本番は、
            ' ①下記コメントを外し、ログオン日付更新処理を有効にする（リコンパイル）
            '★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★

            Try
                'S0020_LOGONYMD検索SQL文
                Dim SQL_Str As String = _
                     "SELECT isnull(LOGONYMD, '') as LOGONYMD " _
                   & " FROM  S0020_LOGONYMD " _
                   & " Where TERMID   = @P1 "
                Using SQLcmd As New SqlCommand(SQL_Str, SQLcon)
                    Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.NVarChar, 30)
                    PARA1.Value = CS0050Session.APSV_ID

                    Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

                    If SQLdr.Read Then
                        Try
                            Dim WW_DATE As Date
                            Date.TryParse(SQLdr("LOGONYMD"), WW_DATE)
                            WW_LOGONYMD = WW_DATE.ToString("yyyy/MM/dd")
                        Catch ex As Exception
                            WW_LOGONYMD = Date.Now
                        End Try
                    End If

                    'Close
                    SQLdr.Close() 'Reader(Close)
                    SQLdr = Nothing
                End Using

            Catch ex As Exception
                Master.output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "S0020_LOGONYMD SELECT")
                CS0011LOGWRITE.INFSUBCLASS = "Main"                         'SUBクラス名
                CS0011LOGWRITE.INFPOSI = "S0020_LOGONYMD SELECT"
                CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
                CS0011LOGWRITE.TEXT = ex.ToString()
                CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR 'DBエラー。
                CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
                Exit Sub
            End Try
        End Using

        CS0050Session.VIEW_MAPID = WW_MAPID
        CS0050Session.VIEW_MAP_VARIANT = WW_VARIANT
        CS0050Session.MAP_ETC = ""
        CS0050Session.VIEW_PERMIT = ""
        Master.MAPID = WW_MAPID
        Master.MAPvariant = WW_VARIANT
        Master.MAPpermitcode = ""
        CS0050Session.LOGONDATE = WW_LOGONYMD

        '画面遷移実行
        If CS0050Session.USERID <> "INIT" Then
            Server.Transfer(WW_URL)
        End If

    End Sub

    ''' <summary>
    ''' 車両マスタ（申請）取得（車検切れ、容器検査切れ判定）  
    ''' </summary>
    ''' <param name="O_RTN">可否判定</param>
    ''' <param name="I_ClientIP3">調査用IPアドレス</param>
    ''' <remarks>
    ''' <para>S0001_TERMを検索　IPADDRを見る　TERMID取得</para>
    ''' <para>TERMIDを基にM00006_STRUCTを検索</para>
    ''' <para >部署を基に運用ガイダンス表示</para>
    ''' </remarks>
    Private Sub GetSHARYOC(ByRef O_RTN As String, ByVal I_ClientIP3 As String)
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

            Dim SQLStr0 As String = _
                     " SELECT                                                                                                " _
                   & "         Z.TERMID                                                                      as TERMID       " _
                   & "       , Z.TERMCAMP                                                                    as COMPCODE     " _
                   & " FROM     S0001_TERM                                  Z                                                " _
                   & " WHERE                                                                                                 " _
                   & "         Z.IPADDR            = @P01                                                                    " _
                   & "   and   Z.TERMCLASS         = @P04                                                                    " _
                   & "   and   Z.STYMD            <= @P02                                                                    " _
                   & "   and   Z.ENDYMD           >= @P02                                                                    " _
                   & "   and   Z.DELFLG           <> @P03                                                                    "

            Dim SQLStr As String = _
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

                    PARA1.Value = I_ClientIP3
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
            Master.output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "S0012_SRVAUTHOR SELECT")
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
    ''' 遷移先URLの取得
    ''' </summary>
    ''' <param name="I_PASSENDYMD"></param>
    ''' <param name="I_MAPID"></param>
    ''' <param name="O_URL"></param>
    ''' <remarks></remarks>
    Protected Sub GetURL(ByVal I_PASSENDYMD As String, ByVal I_MAPID As String, ByRef O_URL As String)
        Dim WW_URL As String = ""

        'DataBase接続文字
        Using SQLcon As SqlConnection = CS0050Session.getConnection
            SQLcon.Open() 'DataBase接続(Open)

            'S0009_URL検索SQL文
            Dim SQL_Str As String = _
                 "SELECT rtrim(URL) as URL " _
               & " FROM  S0009_URL " _
               & " Where MAPID    = @P1 " _
               & "   and STYMD   <= @P2 " _
               & "   and ENDYMD  >= @P3 " _
               & "   and DELFLG  <> @P4 "
            Using SQLcmd As New SqlCommand(SQL_Str, SQLcon)
                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.Char, 50)
                Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.Date)
                Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.Date)
                Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", System.Data.SqlDbType.Char, 1)
                PARA1.Value = I_MAPID

                PARA2.Value = Date.Now
                PARA3.Value = Date.Now
                PARA4.Value = C_DELETE_FLG.DELETE
                Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

                If SQLdr.Read Then
                    O_URL = SQLdr("URL")
                End If

                'Close
                SQLdr.Close() 'Reader(Close)
                SQLdr = Nothing

            End Using
        End Using

    End Sub
End Class



