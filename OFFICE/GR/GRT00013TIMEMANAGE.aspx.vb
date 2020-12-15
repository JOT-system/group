Imports System.Drawing
Imports System.Data.SqlClient
Imports OFFICE.GRIS0005LeftBox
Imports BASEDLL

''' <summary>
''' 乗務員休憩・配送時間入力（実行）
''' </summary>
''' <remarks></remarks>
Public Class GRT00013TIMEMANAGE
    Inherits Page

    '○ 検索結果格納Table
    Private T00013tbl As DataTable                          '一覧格納用テーブル
    Private T00013INPtbl As DataTable                       'チェック用テーブル

    '○ 共通関数宣言(BASEDLL)
    Private CS0011LOGWrite As New CS0011LOGWrite            'ログ出力
    Private CS0013ProfView As New CS0013ProfView            'Tableオブジェクト展開
    Private CS0023XLSUPLOAD As New CS0023XLSUPLOAD          'XLSアップロード
    Private CS0026TBLSORT As New CS0026TBLSORT              '表示画面情報ソート
    Private CS0030REPORT As New CS0030REPORT                '帳票出力
    Private CS0050SESSION As New CS0050SESSION              'セッション情報操作処理

    Private T0007COM As New GRT0007COM                      '勤怠共通
    Private T0013UPDATE As New GRT0013UPDATE                '休憩・配送時間DB更新

    '○ 共通処理結果
    Private WW_ERR_SW As String = ""
    Private WW_RTN_SW As String = ""
    Private WW_RTN_SW2 As String = ""
    Private WW_DUMMY As String = ""

    ''' <summary>
    ''' サーバー処理の遷移先
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs) Handles Me.Load

        Try
            If IsPostBack Then
                '○ 各ボタン押下処理
                If Not String.IsNullOrEmpty(WF_ButtonClick.Value) Then
                    '○ 画面表示データ復元
                    If Not Master.RecoverTable(T00013tbl, WF_XMLsaveF.Value) OrElse
                        Not Master.RecoverTable(T00013INPtbl, WF_XMLsaveF_INP.Value) Then
                        Exit Sub
                    End If

                    Select Case WF_ButtonClick.Value
                        Case "WF_ButtonDOWN"            '前頁ボタン押下
                            WF_ButtonDOWN_Click()
                        Case "WF_ButtonUP"              '次頁ボタン押下
                            WF_ButtonUP_Click()
                        Case "WF_ButtonSAVE"            '一時保存ボタン押下
                            WF_ButtonSAVE_Click()
                        Case "WF_ButtonExtract"         '絞り込みボタン押下
                            WF_ButtonExtract_Click()
                        Case "WF_ButtonUPDATE"          'DB更新ボタン押下
                            WF_ButtonUPDATE_Click()
                        Case "WF_ButtonCSV"             'ダウンロードボタン押下
                            WF_ButtonPrint_Click("XLSX")
                        Case "WF_ButtonPrint"           '一覧印刷ボタン押下
                            WF_ButtonPrint_Click("pdf")
                        Case "WF_ButtonEND"             '終了ボタン押下
                            WF_ButtonEND_Click()
                        Case "WF_EXCEL_UPLOAD"          'ファイルアップロード
                            WF_FILEUPLOAD()
                        Case "WF_ListChange"            'リスト変更
                            WF_ListChange()
                        Case "WF_Field_DBClick"         'フィールドダブルクリック
                            WF_FIELD_DBClick()
                        Case "WF_ButtonSel"             '(左ボックス)選択ボタン押下
                            WF_ButtonSel_Click()
                        Case "WF_ButtonCan"             '(左ボックス)キャンセルボタン押下
                            WF_ButtonCan_Click()
                        Case "WF_ListboxDBclick"        '左ボックスダブルクリック
                            WF_ButtonSel_Click()
                        Case "WF_RadioButonClick"       '(右ボックス)ラジオボタン選択
                            WF_RadioButton_Click()
                        Case "WF_MEMOChange"            '(右ボックス)メモ欄更新
                            WF_RIGHTBOX_Change()
                        Case "HELP"                     'ヘルプ表示
                            WF_HELP_Click()
                    End Select

                    '○ 一覧再表示処理
                    DisplayGrid()
                End If
            Else
                '○ 初期化処理
                Initialize()
            End If
        Finally
            '○ 格納Table Close
            If Not IsNothing(T00013tbl) Then
                T00013tbl.Clear()
                T00013tbl.Dispose()
                T00013tbl = Nothing
            End If

            If Not IsNothing(T00013INPtbl) Then
                T00013INPtbl.Clear()
                T00013INPtbl.Dispose()
                T00013INPtbl = Nothing
            End If

        End Try

    End Sub

    ''' <summary>
    ''' 初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub Initialize()

        '○ 画面ID設定
        Master.MAPID = GRT00013WRKINC.MAPID

        WF_SELSTAFFCODE.Focus()
        WF_FIELD.Value = ""
        WF_LeftboxOpen.Value = ""
        WF_LeftMViewChange.Value = ""
        WF_RightboxOpen.Value = ""
        leftview.ActiveListBox()
        rightview.ResetIndex()

        Dim WW_CheckMES As String = ""
        Dim WW_MSGNO As String = C_MESSAGE_NO.NORMAL

        '○ 画面の値設定
        WW_MAPValueSet(WW_CheckMES, WW_MSGNO)
        If Not isNormal(WW_MSGNO) Then
            Master.Output(WW_MSGNO, C_MESSAGE_TYPE.ABORT)
            WW_CheckERR(WW_CheckMES, "")
        End If

        '○ 右ボックスへの値設定
        rightview.MAPID = Master.MAPID
        rightview.MAPVARI = Master.MAPvariant
        rightview.COMPCODE = work.WF_SEL_CAMPCODE.Text
        rightview.PROFID = Master.PROF_REPORT
        rightview.TARGETDATE = work.WF_SEL_TAISHOYM.Text & "/01"
        rightview.Initialize(WW_DUMMY)

        '○ GridView初期設定
        GridViewInitialize()

    End Sub

    ''' <summary>
    ''' 画面初期値設定処理
    ''' </summary>
    ''' <param name="O_MSG"></param>
    ''' <param name="O_RTN"></param>
    ''' <remarks></remarks>
    Protected Sub WW_MAPValueSet(ByRef O_MSG As String, ByRef O_RTN As String)

        O_MSG = ""
        O_RTN = C_MESSAGE_NO.NORMAL
        Dim WW_ERR_MSG As String = ""

        'Grid情報保存先のファイル名
        WF_XMLsaveF.Value = CS0050SESSION.UPLOAD_PATH & "\XML_TMP\" & Date.Now.ToString("yyyyMMdd") & "-" &
            Master.USERID & "-" & Master.MAPID & "-" & Master.MAPvariant & "-" & Date.Now.ToString("HHmmss") & ".txt"

        WF_XMLsaveF_INP.Value = CS0050SESSION.UPLOAD_PATH & "\XML_TMP\" & Date.Now.ToString("yyyyMMdd") & "-" &
            Master.USERID & "-" & Master.MAPID & "INP-" & Master.MAPvariant & "-" & Date.Now.ToString("HHmmss") & ".txt"

        If Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.T00013S Then          '検索画面からの遷移
            If Master.MAPpermitcode = C_PERMISSION.UPDATE Then
                If work.WF_SEL_LIMITFLG.Text = "0" Then
                    If work.WF_SEL_PERMITCODE.Text = C_PERMISSION.UPDATE Then
                        '更新権限あり
                        WF_MAPpermitcode.Value = "TRUE"
                    Else
                        '更新権限なし
                        WF_MAPpermitcode.Value = "FALSE"
                        O_RTN = C_MESSAGE_NO.UPDATE_AUTHORIZATION_ERROR
                        WW_ERR_MSG = "・選択した配属部署は、更新権限がありません。"
                        O_MSG = O_MSG & ControlChars.NewLine & WW_ERR_MSG
                    End If
                Else
                    '対象年月の締後は更新できない
                    WF_MAPpermitcode.Value = "FALSE"
                    O_RTN = C_MESSAGE_NO.OVER_CLOSING_DATE_ERROR
                    WW_ERR_MSG = "・勤怠締後は更新できません。"
                    O_MSG = O_MSG & ControlChars.NewLine & WW_ERR_MSG
                End If
            Else
                '更新権限なし
                WF_MAPpermitcode.Value = "FALSE"
                O_RTN = C_MESSAGE_NO.UPDATE_AUTHORIZATION_ERROR
                WW_ERR_MSG = "・営業勤怠登録の更新権限がありません。"
                O_MSG = O_MSG & ControlChars.NewLine & WW_ERR_MSG
            End If

            '画面初期従業員設定
            Dim prmdata As New Hashtable
            If Trim(work.WF_SEL_HORG.Text) = "" Then
                prmdata = work.CreateStaffCodeParam(GL0005StaffList.LC_STAFF_TYPE.DRIVER, work.WF_SEL_CAMPCODE.Text,
                            work.WF_SEL_TAISHOYM.Text, work.WF_SEL_HORG.Text, work.WF_SEL_STAFFKBN.Text, work.WF_SEL_STAFFCODE.Text)
            Else
                prmdata = work.CreateStaffCodeParam(GL0005StaffList.LC_STAFF_TYPE.ATTENDANCE_FOR_DRIVER, work.WF_SEL_CAMPCODE.Text,
                            work.WF_SEL_TAISHOYM.Text, work.WF_SEL_HORG.Text, work.WF_SEL_STAFFKBN.Text, work.WF_SEL_STAFFCODE.Text)
            End If
            leftview.SetListBox(LIST_BOX_CLASSIFICATION.LC_STAFFCODE, WW_DUMMY, prmdata)
            For i As Integer = 0 To leftview.WF_LeftListBox.Items.Count - 1
                WF_STAFFCODE.Text = leftview.WF_LeftListBox.Items(i).Value
                Exit For
            Next
            WF_TAISHOYM.Text = work.WF_SEL_TAISHOYM.Text
            WF_HORG.Text = work.WF_SEL_HORG.Text

            '名称取得
            CODENAME_get("STAFFCODE", WF_STAFFCODE.Text, WF_STAFFCODE_TEXT.Text, WW_DUMMY)
            CODENAME_get("HORG", WF_HORG.Text, WF_HORG_TEXT.Text, WW_DUMMY)
        End If

        '○ ファイルドロップ有無
        Master.eventDrop = True

    End Sub

    ''' <summary>
    ''' GridViewデータ設定
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub GridViewInitialize()

        '○ 再開ボタン押下時
        If work.WF_SEL_RESTARTFLG.Text = "TRUE" Then
            If Not Master.RecoverTable(T00013tbl, work.WF_SEL_XMLsaveTMP.Text) Then
                Exit Sub
            End If
        Else
            '○ 画面表示データ取得
            Using SQLcon As SqlConnection = CS0050SESSION.getConnection()
                SQLcon.Open()       'DataBase接続

                MAPDataGet(SQLcon)
            End Using
        End If

        '○ 画面表示データ保存
        Master.SaveTable(T00013tbl, WF_XMLsaveF.Value)

        Master.CreateEmptyTable(T00013INPtbl, WF_XMLsaveF.Value)

        '○ 初期画面の乗務員分のデータを格納
        CS0026TBLSORT.TABLE = T00013tbl
        CS0026TBLSORT.SORTING = "LINECNT, STAFFCODE, WORKDATE"
        CS0026TBLSORT.FILTER = "STAFFCODE = '" & WF_STAFFCODE.Text & "'"
        CS0026TBLSORT.Sort(T00013INPtbl)
        Master.SaveTable(T00013INPtbl, WF_XMLsaveF_INP.Value)

        '○ 一覧表示データ編集(性能対策)
        Dim TBLview As DataView = New DataView(T00013INPtbl)
        TBLview.RowFilter = "HIDDEN = 0"

        '○ 一部画面表示編集(00:00をブランクに変更)
        ZeroToBlank(TBLview)

        CS0013ProfView.CAMPCODE = work.WF_SEL_CAMPCODE.Text
        CS0013ProfView.PROFID = Master.PROF_VIEW
        CS0013ProfView.MAPID = Master.MAPID
        CS0013ProfView.VARI = Master.VIEWID
        CS0013ProfView.SRCDATA = TBLview.ToTable
        CS0013ProfView.TBLOBJ = pnlListArea
        CS0013ProfView.SCROLLTYPE = CS0013ProfView.SCROLLTYPE_ENUM.Both
        CS0013ProfView.LEVENT = "Onchange"
        CS0013ProfView.LFUNC = "ListChange"
        CS0013ProfView.NOCOLUMNWIDTHOPT = -1
        CS0013ProfView.TITLEOPT = True
        CS0013ProfView.WITHTAGNAMES = True
        CS0013ProfView.TARGETDATE = work.WF_SEL_TAISHOYM.Text & "/01"
        CS0013ProfView.CS0013ProfView()
        If Not isNormal(CS0013ProfView.ERR) Then
            Master.Output(CS0013ProfView.ERR, C_MESSAGE_TYPE.ABORT, "一覧設定エラー")
            Exit Sub
        End If

        TBLview.Dispose()
        TBLview = Nothing

        '○ 曜日表示色変更
        WeekColorChange()

        If Not String.IsNullOrEmpty(work.WF_SEL_XMLsaveTMP.Text) Then
            System.IO.File.Delete(work.WF_SEL_XMLsaveTMP.Text)
        End If

    End Sub

    ''' <summary>
    ''' 画面表示データ取得
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub MAPDataGet(ByVal SQLcon As SqlConnection)

        '対象年月
        If IsNothing(T00013tbl) Then
            T00013tbl = New DataTable
        End If

        If T00013tbl.Columns.Count <> 0 Then
            T00013tbl.Columns.Clear()
        End If

        T00013tbl.Clear()

        Dim SQLcmd As New SqlCommand()

        '開始と終了の日付を準備
        Dim WW_DATE_ST As Date
        Dim WW_DATE_END As Date
        Try
            Date.TryParse(work.WF_SEL_TAISHOYM.Text & "/01", WW_DATE_ST)
            WW_DATE_END = WW_DATE_ST.AddMonths(1).AddDays(-1)
        Catch ex As Exception
            WW_DATE_ST = Convert.ToDateTime(Date.Now.ToString("yyyy/MM") & "/01")
            WW_DATE_END = WW_DATE_ST.AddMonths(1).AddDays(-1)
        End Try

        Try
            '○ テンポラリーテーブルを作成する
            Dim SQLstr As String =
                  " CREATE TABLE #MBTemp" _
                & " (" _
                & "    CAMPCODE nvarchar(20)" _
                & "    , STAFFCODE nvarchar(20)" _
                & "    , HORG nvarchar(20)" _
                & " )"

            SQLcmd = New SqlCommand(SQLstr, SQLcon)
            SQLcmd.ExecuteNonQuery()

            '○ テンポラリーテーブル用のデータを取得する
            SQLstr =
                 " SELECT" _
                & "    ISNULL(RTRIM(MB01.CAMPCODE), '')    AS CAMPCODE" _
                & "    , ISNULL(RTRIM(MB01.STAFFCODE), '') AS STAFFCODE" _
                & "    , ISNULL(RTRIM(MB01.HORG), '')      AS HORG" _
                & " FROM" _
                & "    MB001_STAFF MB01" _
                & "    INNER JOIN S0012_SRVAUTHOR S012" _
                & "        ON  S012.TERMID    = @P1" _
                & "        AND S012.CAMPCODE  = @P2" _
                & "        AND S012.OBJECT    = @P3" _
                & "        AND S012.STYMD    <= @P9" _
                & "        AND S012.ENDYMD   >= @P9" _
                & "        AND S012.DELFLG   <> @P10" _
                & "    INNER JOIN S0006_ROLE S006" _
                & "        ON  S006.CAMPCODE  = S012.CAMPCODE" _
                & "        AND S006.OBJECT    = @P3" _
                & "        AND S006.ROLE      = S012.ROLE" _
                & "        AND S006.STYMD    <= @P9" _
                & "        AND S006.ENDYMD   >= @P9" _
                & "        AND S006.DELFLG   <> @P10" _
                & "    INNER JOIN (" _
                & "            SELECT" _
                & "                ISNULL(RTRIM(CODE), '') AS CODE" _
                & "            FROM" _
                & "                M0006_STRUCT" _
                & "            WHERE" _
                & "                CAMPCODE     = @P2" _
                & "                AND OBJECT   = @P4" _
                & "                AND STRUCT   = @P5" _
                & "                AND GRCODE01 like @P6 + '%'" _
                & "                AND STYMD   <= @P9" _
                & "                AND ENDYMD  >= @P9" _
                & "                AND DELFLG  <> @P10) M006" _
                & "        ON  M006.CODE      = S006.CODE" _
                & "        AND M006.CODE      = MB01.HORG" _
                & " WHERE" _
                & "    MB01.CAMPCODE      = @P2" _
                & "    AND MB01.STAFFKBN  LIKE '03%'" _
                & "    AND MB01.STYMD    <= @P7" _
                & "    AND MB01.ENDYMD   >= @P8" _
                & "    AND MB01.DELFLG   <> @P10"

            '○ 条件指定で指定されたものでSQLで可能なものを追加する
            '従業員(コード)
            If Not String.IsNullOrEmpty(work.WF_SEL_STAFFCODE.Text) Then
                SQLstr &= String.Format("    AND MB01.STAFFCODE = '{0}'", work.WF_SEL_STAFFCODE.Text)
            End If
            '職務区分
            If Not String.IsNullOrEmpty(work.WF_SEL_STAFFKBN.Text) Then
                SQLstr &= String.Format("    AND MB01.STAFFKBN  = '{0}'", work.WF_SEL_STAFFKBN.Text)
            End If
            '従業員(名称)
            If Not String.IsNullOrEmpty(work.WF_SEL_STAFFNAMES.Text) Then
                SQLstr &= String.Format("    AND MB01.STAFFNAMES LIKE '%{0}%'", work.WF_SEL_STAFFNAMES.Text)
            End If

            SQLstr &=
                  " GROUP BY" _
                & "    MB01.CAMPCODE" _
                & "    , MB01.STAFFCODE" _
                & "    , MB01.HORG" _
                & " ORDER BY" _
                & "    MB01.CAMPCODE" _
                & "    , MB01.STAFFCODE" _
                & "    , MB01.HORG"

            SQLcmd = New SqlCommand(SQLstr, SQLcon)

            Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", SqlDbType.NVarChar, 30)           '端末ID
            Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", SqlDbType.NVarChar, 20)           '会社コード
            Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", SqlDbType.NVarChar, 20)           'オブジェクト
            Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", SqlDbType.NVarChar, 20)           'オブジェクト
            Dim PARA5 As SqlParameter = SQLcmd.Parameters.Add("@P5", SqlDbType.NVarChar, 50)           '構造コード
            Dim PARA6 As SqlParameter = SQLcmd.Parameters.Add("@P6", SqlDbType.NVarChar, 20)           'グループコード1
            Dim PARA7 As SqlParameter = SQLcmd.Parameters.Add("@P7", SqlDbType.Date)                   '開始年月日
            Dim PARA8 As SqlParameter = SQLcmd.Parameters.Add("@P8", SqlDbType.Date)                   '終了年月日
            Dim PARA9 As SqlParameter = SQLcmd.Parameters.Add("@P9", SqlDbType.Date)                   '現在日付
            Dim PARA10 As SqlParameter = SQLcmd.Parameters.Add("@P10", SqlDbType.NVarChar, 1)          '削除フラグ

            PARA1.Value = CS0050SESSION.APSV_ID
            PARA2.Value = work.WF_SEL_CAMPCODE.Text
            PARA3.Value = C_ROLE_VARIANT.SERV_ORG
            PARA4.Value = C_ROLE_VARIANT.USER_ORG
            PARA5.Value = "勤怠管理組織"
            PARA6.Value = work.WF_SEL_HORG.Text
            PARA7.Value = WW_DATE_END
            PARA8.Value = WW_DATE_ST
            PARA9.Value = Date.Now
            PARA10.Value = C_DELETE_FLG.DELETE

            Dim WW_TABLE As DataTable = New DataTable

            Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                WW_TABLE.Load(SQLdr)
            End Using

            'テンポラリーテーブルに出力
            Using SQLbc As New SqlBulkCopy(SQLcon)
                SQLbc.DestinationTableName = "#MBTemp"
                SQLbc.WriteToServer(WW_TABLE)
            End Using

            If Not IsNothing(WW_TABLE) Then
                WW_TABLE.Clear()
                WW_TABLE.Dispose()
                WW_TABLE = Nothing
            End If

            '○ 画面表示のメインSQL
            SQLstr =
                  " SELECT" _
                & "    0                                                      AS LINECNT" _
                & "    , ''                                                   AS OPERATION" _
                & "    , CAST(ISNULL(T013.UPDTIMSTP, 0) AS bigint)            AS TIMSTP" _
                & "    , 1                                                    AS 'SELECT'" _
                & "    , 0                                                    AS HIDDEN" _
                & "    , '0'                                                  AS EXTRACTCNT" _
                & "    , ''                                                   AS STATUS" _
                & "    , (CASE WHEN @P5 = 'BB' " _
                & "            THEN '休憩時間一覧'" _
                & "            ELSE '配送時間一覧' END)                       AS TITLE_TXT" _
                & "    , ISNULL(RTRIM(TEMP.CAMPCODE), '')                     AS CAMPCODE" _
                & "    , ''                                                   AS CAMPNAMES" _
                & "    , '会社コード'                                         AS CAMPCODE_TXT" _
                & "    , ISNULL(FORMAT(MB05.WORKINGYMD, 'yyyy/MM'), '')       AS TAISHOYM" _
                & "    , '対象年月'                                           AS TAISHOYM_TXT" _
                & "    , ISNULL(RTRIM(TEMP.STAFFCODE), '')                    AS STAFFCODE" _
                & "    , ''                                                   AS STAFFNAMES" _
                & "    , ISNULL(RTRIM(TEMP.HORG), '')                         AS HORG" _
                & "    , ''                                                   AS HORGNAMES" _
                & "    , '配属部署'                                           AS HORG_TXT" _
                & "    , ISNULL(FORMAT(MB05.WORKINGYMD, 'yyyy/MM/dd'), '')    AS WORKDATE" _
                & "    , ISNULL(FORMAT(MB05.WORKINGYMD, 'dd'), '')            AS WORKDAY" _
                & "    , ISNULL(RTRIM(MB05.WORKINGWEEK), '')                  AS WORKINGWEEK" _
                & "    , ''                                                   AS WORKINGWEEKNAMES" _
                & "    , ISNULL(RTRIM(T013.WORKKBN), @P5)                     AS WORKKBN" _
                & "    , ''                                                   AS WORKKBNNAMES" _
                & "    , ISNULL(CONVERT(char(5), T013.STTIME01), '00:00')     AS STTIME01" _
                & "    , ISNULL(CONVERT(char(5), T013.ENDTIME01), '00:00')    AS ENDTIME01" _
                & "    , ISNULL(CONVERT(char(5), T013.STTIME02), '00:00')     AS STTIME02" _
                & "    , ISNULL(CONVERT(char(5), T013.ENDTIME02), '00:00')    AS ENDTIME02" _
                & "    , ISNULL(CONVERT(char(5), T013.STTIME03), '00:00')     AS STTIME03" _
                & "    , ISNULL(CONVERT(char(5), T013.ENDTIME03), '00:00')    AS ENDTIME03" _
                & "    , ISNULL(CONVERT(char(5), T013.STTIME04), '00:00')     AS STTIME04" _
                & "    , ISNULL(CONVERT(char(5), T013.ENDTIME04), '00:00')    AS ENDTIME04" _
                & "    , ISNULL(CONVERT(char(5), T013.STTIME05), '00:00')     AS STTIME05" _
                & "    , ISNULL(CONVERT(char(5), T013.ENDTIME05), '00:00')    AS ENDTIME05" _
                & "    , ISNULL(CONVERT(char(5), T013.STTIME06), '00:00')     AS STTIME06" _
                & "    , ISNULL(CONVERT(char(5), T013.ENDTIME06), '00:00')    AS ENDTIME06" _
                & "    , ISNULL(CONVERT(char(5), T013.STTIME07), '00:00')     AS STTIME07" _
                & "    , ISNULL(CONVERT(char(5), T013.ENDTIME07), '00:00')    AS ENDTIME07" _
                & "    , ISNULL(CONVERT(char(5), T013.STTIME08), '00:00')     AS STTIME08" _
                & "    , ISNULL(CONVERT(char(5), T013.ENDTIME08), '00:00')    AS ENDTIME08" _
                & "    , ISNULL(CONVERT(char(5), T013.STTIME09), '00:00')     AS STTIME09" _
                & "    , ISNULL(CONVERT(char(5), T013.ENDTIME09), '00:00')    AS ENDTIME09" _
                & "    , ISNULL(CONVERT(char(5), T013.STTIME10), '00:00')     AS STTIME10" _
                & "    , ISNULL(CONVERT(char(5), T013.ENDTIME10), '00:00')    AS ENDTIME10" _
                & "    , CAST(ISNULL(T013.TTLTIME, '0') AS char)              AS TTLTIME" _
                & "    , CAST(ISNULL(RTRIM(MB02.SEQ), 0) as int)              AS SEQ" _
                & "    , ISNULL(RTRIM(T013.DELFLG), '0')                      AS DELFLG" _
                & "    , ''                                                   AS DELFLGNAMES" _
                & "    , (CASE WHEN ISNULL(RTRIM(T013.WORKKBN), '') <> ''" _
                & "            THEN '1'" _
                & "            ELSE '0' END)                                  AS DBUMUFLG" _
                & " FROM" _
                & "    #MBTemp TEMP" _
                & "    INNER JOIN (" _
                & "        SELECT" _
                & "            ISNULL(RTRIM(CAMPCODE), '')                    AS CAMPCODE" _
                & "            , WORKINGYMD                                   AS WORKINGYMD" _
                & "            , ISNULL(RTRIM(WORKINGWEEK), '')               AS WORKINGWEEK" _
                & "            , ISNULL(RTRIM(WORKINGKBN), '')                AS WORKINGKBN" _
                & "        FROM" _
                & "            MB005_CALENDAR" _
                & "        WHERE" _
                & "            CAMPCODE          = @P1" _
                & "            AND WORKINGYMD   >= @P2" _
                & "            AND WORKINGYMD   <= @P3" _
                & "            AND DELFLG       <> @P4" _
                & "        ) MB05" _
                & "        ON  MB05.CAMPCODE     = TEMP.CAMPCODE" _
                & "    LEFT JOIN MB001_STAFF MB01" _
                & "        ON  MB01.CAMPCODE     = TEMP.CAMPCODE" _
                & "        AND MB01.STAFFCODE    = TEMP.STAFFCODE" _
                & "        AND MB01.STYMD       <= MB05.WORKINGYMD" _
                & "        AND MB01.ENDYMD      >= MB05.WORKINGYMD" _
                & "        AND MB01.DELFLG      <> @P4" _
                & "    LEFT JOIN MB002_STAFFORG MB02" _
                & "        ON  MB02.CAMPCODE     = TEMP.CAMPCODE" _
                & "        AND MB02.STAFFCODE    = TEMP.STAFFCODE" _
                & "        AND MB02.SORG         = TEMP.HORG" _
                & "        AND MB02.DELFLG      <> @P4" _
                & "    LEFT JOIN T0013_TIMEMANAGE T013" _
                & "        ON  T013.CAMPCODE     = TEMP.CAMPCODE" _
                & "        AND T013.WORKDATE     = MB05.WORKINGYMD" _
                & "        AND T013.STAFFCODE    = TEMP.STAFFCODE" _
                & "        AND T013.WORKKBN      = @P5" _
                & "        AND T013.DELFLG      <> @P4" _
                & " WHERE" _
                & "    TEMP.CAMPCODE = @P1" _
                & " ORDER BY" _
                & "    TEMP.CAMPCODE" _
                & "    , MB01.HORG" _
                & "    , TEMP.STAFFCODE" _
                & "    , MB05.WORKINGYMD"

            '休憩時間取得（G1）
            SQLcmd = New SqlCommand(SQLstr, SQLcon)

            PARA1 = SQLcmd.Parameters.Add("@P1", SqlDbType.NVarChar, 20)        '会社コード
            PARA2 = SQLcmd.Parameters.Add("@P2", SqlDbType.Date)                '対象年月初
            PARA3 = SQLcmd.Parameters.Add("@P3", SqlDbType.Date)                '対象年月末
            PARA4 = SQLcmd.Parameters.Add("@P4", SqlDbType.NVarChar, 1)         '削除フラグ
            PARA5 = SQLcmd.Parameters.Add("@P5", SqlDbType.NVarChar, 2)         '作業区分

            PARA1.Value = work.WF_SEL_CAMPCODE.Text
            PARA2.Value = WW_DATE_ST
            PARA3.Value = WW_DATE_END
            PARA4.Value = C_DELETE_FLG.DELETE
            PARA5.Value = work.WF_SEL_WORKKBN.Text

            Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                '○ フィールド名とフィールドの型を取得
                For index As Integer = 0 To SQLdr.FieldCount - 1
                    T00013tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                Next

                '○ テーブル検索結果をテーブル格納
                T00013tbl.Load(SQLdr)
            End Using

            Dim WW_LINECNT As Integer = 0
            Dim WW_SAVEKEY As String = ""
            For Each T00013row As DataRow In T00013tbl.Rows
                Dim WW_KEY As String = T00013row("CAMPCODE") & "," & T00013row("STAFFCODE")
                If WW_SAVEKEY <> WW_KEY Then
                    WW_LINECNT = 0
                    WW_SAVEKEY = WW_KEY
                End If

                '固定項目
                WW_LINECNT = WW_LINECNT + 1
                T00013row("LINECNT") = WW_LINECNT
                T00013row("SELECT") = 1
                T00013row("HIDDEN") = 0

                '設定項目

                Dim WW_TOTAL As Integer = 0

                '合計算出、分 → 時:分に変換(formatHHMM)
                T00013row("TTLTIME") = T0007COM.formatHHMM(T00013row("TTLTIME"))                        '合計時間

                '名称取得
                CODENAME_get("CAMPCODE", T00013row("CAMPCODE"), T00013row("CAMPNAMES"), WW_DUMMY)                       '会社コード
                CODENAME_get("STAFFCODE", T00013row("STAFFCODE"), T00013row("STAFFNAMES"), WW_DUMMY)                    '従業員コード
                CODENAME_get("WORKINGWEEK", T00013row("WORKINGWEEK"), T00013row("WORKINGWEEKNAMES"), WW_DUMMY)          '営業日曜日
                CODENAME_get("HORG", T00013row("HORG"), T00013row("HORGNAMES"), WW_DUMMY)                               '配属部署
                CODENAME_get("WORKKBN", T00013row("WORKKBN"), T00013row("WORKKBNNAMES"), WW_DUMMY)                      '作業区分
                CODENAME_get("DELFLG", T00013row("DELFLG"), T00013row("DELFLGNAMES"), WW_DUMMY)                         '削除フラグ
            Next
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "T0013_TIMEMANAGE SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:T0013_TIMEMANAGE Select"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        Finally
            SQLcmd.Dispose()
            SQLcmd = Nothing
        End Try

    End Sub

    ''' <summary>
    ''' 一覧再表示処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub DisplayGrid()

        '○ ヘッダ編集
        For Each T00013INProw As DataRow In T00013INPtbl.Rows
            WF_TAISHOYM.Text = CDate(T00013INProw("TAISHOYM") & "/01").ToString("yyyy/MM")
            WF_STAFFCODE.Text = T00013INProw("STAFFCODE")
            WF_HORG.Text = T00013INProw("HORG")

            '名称取得
            CODENAME_get("STAFFCODE", WF_STAFFCODE.Text, WF_STAFFCODE_TEXT.Text, WW_DUMMY)
            CODENAME_get("ORG", WF_HORG.Text, WF_HORG_TEXT.Text, WW_DUMMY)
            Exit For
        Next

        '○ 一覧表示データ編集(性能対策)
        Dim TBLview As DataView = New DataView(T00013INPtbl)

        ZeroToBlank(TBLview)

        CS0013ProfView.CAMPCODE = work.WF_SEL_CAMPCODE.Text
        CS0013ProfView.PROFID = Master.PROF_VIEW
        CS0013ProfView.MAPID = Master.MAPID
        CS0013ProfView.VARI = Master.VIEWID
        CS0013ProfView.SRCDATA = TBLview.ToTable
        CS0013ProfView.TBLOBJ = pnlListArea
        CS0013ProfView.SCROLLTYPE = CS0013ProfView.SCROLLTYPE_ENUM.Both
        CS0013ProfView.LEVENT = "Onchange"
        CS0013ProfView.LFUNC = "ListChange"
        CS0013ProfView.NOCOLUMNWIDTHOPT = -1
        CS0013ProfView.TITLEOPT = True
        CS0013ProfView.WITHTAGNAMES = True
        CS0013ProfView.TARGETDATE = work.WF_SEL_TAISHOYM.Text & "/01"
        CS0013ProfView.CS0013ProfView()
        If Not isNormal(CS0013ProfView.ERR) Then
            Master.Output(CS0013ProfView.ERR, C_MESSAGE_TYPE.ABORT, "一覧設定エラー")
            Exit Sub
        End If

        TBLview.Dispose()
        TBLview = Nothing

        '○ 曜日表示色変更
        WeekColorChange()

    End Sub

    ''' <summary>
    ''' 00:00をブランクに変換
    ''' </summary>
    ''' <param name="I_VIEW"></param>
    ''' <remarks></remarks>
    Protected Sub ZeroToBlank(ByRef I_VIEW As DataView)


        For Each row As DataRow In I_VIEW.Table.Rows
            Dim WW_STZERO_FLG() As Integer = {0, 0, 0, 0, 0, 0, 0, 0, 0, 0}
            Dim WW_ENDZERO_FLG() As Integer = {0, 0, 0, 0, 0, 0, 0, 0, 0, 0}

            For Each col As DataColumn In I_VIEW.Table.Columns
                '下記項目は00:00をブランクに変更
                For i As Integer = 1 To 10
                    Dim WW_STTIME As String = "STTIME" & i.ToString("00")
                    Dim WW_ENDTIME As String = "ENDTIME" & i.ToString("00")
                    If col.ColumnName = WW_STTIME OrElse
                       col.ColumnName = WW_ENDTIME Then
                        Dim WW_TIME As String() = row(col).Split(":")
                        If WW_TIME.Count > 1 AndAlso
                           row(col) = "00:00" Then
                            row(col) = ""
                            Select Case col.ColumnName
                                Case WW_STTIME
                                    WW_STZERO_FLG(i - 1) = 1
                                Case WW_ENDTIME
                                    WW_ENDZERO_FLG(i - 1) = 1
                            End Select
                        End If
                    End If
                Next
            Next

            '開始と終了のいづれかが00:00以外なら00:00を表示する
            For Each col As DataColumn In I_VIEW.Table.Columns
                For i As Integer = 1 To 10
                    Dim WW_STTIME As String = "STTIME" & i.ToString("00")
                    Dim WW_ENDTIME As String = "ENDTIME" & i.ToString("00")
                    If col.ColumnName = WW_STTIME OrElse
                       col.ColumnName = WW_ENDTIME Then
                        Select Case col.ColumnName
                            Case WW_STTIME
                                If WW_STZERO_FLG(i - 1) = 1 AndAlso
                                   WW_ENDZERO_FLG(i - 1) = 0 Then
                                    row(col) = "00:00"
                                End If
                            Case WW_ENDTIME
                                If WW_STZERO_FLG(i - 1) = 0 AndAlso
                                   WW_ENDZERO_FLG(i - 1) = 1 Then
                                    row(col) = "00:00"
                                End If
                        End Select
                    End If
                Next
            Next
        Next

    End Sub

    ''' <summary>
    ''' 曜日の表示色変更
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WeekColorChange()

        Dim WW_T00013tbl As DataTable = New DataTable
        CS0026TBLSORT.TABLE = T00013INPtbl
        CS0026TBLSORT.SORTING = "LINECNT, STAFFCODE, WORKDATE"
        CS0026TBLSORT.FILTER = "HIDDEN = 0"
        CS0026TBLSORT.Sort(WW_T00013tbl)

        Dim tblDataL As Control = pnlListArea.FindControl(pnlListArea.ID & "_DL").Controls(0)

        For i As Integer = 0 To WW_T00013tbl.Rows.Count - 1
            Dim rows As Control = tblDataL.Controls(i)
            Dim WeekCell As TableCell = Nothing

            For Each cell As TableCell In rows.Controls
                'LabelセルにはIDを持っていないためテキストで探す
                If cell.Text = "月" OrElse
                    cell.Text = "火" OrElse
                    cell.Text = "水" OrElse
                    cell.Text = "木" OrElse
                    cell.Text = "金" OrElse
                    cell.Text = "土" OrElse
                    cell.Text = "日" Then
                    WeekCell = cell
                    Exit For
                End If
            Next

            If IsNothing(WeekCell) Then
                Continue For
            End If

            If WW_T00013tbl.Rows(i)("WORKINGWEEK") = "0" Then
                '日曜日は赤
                WeekCell.ForeColor = Color.Red
            Else
                '平日は黒
                WeekCell.ForeColor = Color.Black
            End If
        Next

        If Not IsNothing(WW_T00013tbl) Then
            WW_T00013tbl.Clear()
            WW_T00013tbl.Dispose()
            WW_T00013tbl = Nothing
        End If

    End Sub


    ''' <summary>
    ''' 前頁ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonDOWN_Click()

        '○ 絞込条件が入力されている場合処理しない
        WF_SELSTAFFCODE_TEXT.Text = ""
        If WF_SELSTAFFCODE.Text <> "" Then
            Master.EraseCharToIgnore(WF_SELSTAFFCODE.Text)
            CODENAME_get("STAFFCODE", WF_SELSTAFFCODE.Text, WF_SELSTAFFCODE_TEXT.Text, WW_DUMMY)
            Exit Sub
        End If

        '○ 全体データにT00013INPtbl(個人)を反映(削除してマージ)
        CS0026TBLSORT.TABLE = T00013tbl
        CS0026TBLSORT.SORTING = "LINECNT, STAFFCODE, WORKDATE"
        CS0026TBLSORT.FILTER = "STAFFCODE <> '" & WF_STAFFCODE.Text & "'"
        CS0026TBLSORT.Sort(T00013tbl)
        T00013tbl.Merge(T00013INPtbl)

        '○ 前の乗務員を取得(既に最初の場合変更無し)
        Dim prmData As New Hashtable
        If Trim(work.WF_SEL_HORG.Text) = "" Then
            prmData = work.CreateStaffCodeParam(GL0005StaffList.LC_STAFF_TYPE.DRIVER, work.WF_SEL_CAMPCODE.Text,
                        work.WF_SEL_TAISHOYM.Text, work.WF_SEL_HORG.Text, work.WF_SEL_STAFFKBN.Text, work.WF_SEL_STAFFCODE.Text)
        Else
            prmData = work.CreateStaffCodeParam(GL0005StaffList.LC_STAFF_TYPE.ATTENDANCE_FOR_DRIVER, work.WF_SEL_CAMPCODE.Text,
                        work.WF_SEL_TAISHOYM.Text, work.WF_SEL_HORG.Text, work.WF_SEL_STAFFKBN.Text, work.WF_SEL_STAFFCODE.Text)
        End If
        leftview.SetListBox(LIST_BOX_CLASSIFICATION.LC_STAFFCODE, WW_DUMMY, prmData)
        Dim WW_STAFF As String = WF_STAFFCODE.Text
        For i As Integer = 0 To leftview.WF_LeftListBox.Items.Count - 1
            If leftview.WF_LeftListBox.Items(i).Value = WF_STAFFCODE.Text Then
                Exit For
            End If

            WW_STAFF = leftview.WF_LeftListBox.Items(i).Value
        Next

        CS0026TBLSORT.TABLE = T00013tbl
        CS0026TBLSORT.SORTING = "LINECNT, STAFFCODE, WORKDATE"
        CS0026TBLSORT.FILTER = "STAFFCODE = '" & WW_STAFF & "'"
        CS0026TBLSORT.Sort(T00013INPtbl)

        '○ テーブル保存
        Master.SaveTable(T00013tbl, WF_XMLsaveF.Value)
        Master.SaveTable(T00013INPtbl, WF_XMLsaveF_INP.Value)

        WF_FIELD.Value = ""
        WF_LeftboxOpen.Value = ""
        WF_LeftMViewChange.Value = ""

    End Sub

    ''' <summary>
    ''' 次頁ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonUP_Click()

        '○ 絞込条件が入力されている場合処理しない
        WF_SELSTAFFCODE_TEXT.Text = ""
        If WF_SELSTAFFCODE.Text <> "" Then
            Master.EraseCharToIgnore(WF_SELSTAFFCODE.Text)
            CODENAME_get("STAFFCODE", WF_SELSTAFFCODE.Text, WF_SELSTAFFCODE_TEXT.Text, WW_DUMMY)
            Exit Sub
        End If

        '○ 全体データにT00013INPtbl(個人)を反映(削除してマージ)
        CS0026TBLSORT.TABLE = T00013tbl
        CS0026TBLSORT.SORTING = "LINECNT, STAFFCODE, WORKDATE"
        CS0026TBLSORT.FILTER = "STAFFCODE <> '" & WF_STAFFCODE.Text & "'"
        CS0026TBLSORT.Sort(T00013tbl)
        T00013tbl.Merge(T00013INPtbl)

        '○ 次の乗務員を取得(既に最後の場合変更無し)
        Dim prmData As New Hashtable
        If Trim(work.WF_SEL_HORG.Text) = "" Then
            prmData = work.CreateStaffCodeParam(GL0005StaffList.LC_STAFF_TYPE.DRIVER, work.WF_SEL_CAMPCODE.Text,
                        work.WF_SEL_TAISHOYM.Text, work.WF_SEL_HORG.Text, work.WF_SEL_STAFFKBN.Text, work.WF_SEL_STAFFCODE.Text)
        Else
            prmData = work.CreateStaffCodeParam(GL0005StaffList.LC_STAFF_TYPE.ATTENDANCE_FOR_DRIVER, work.WF_SEL_CAMPCODE.Text,
                        work.WF_SEL_TAISHOYM.Text, work.WF_SEL_HORG.Text, work.WF_SEL_STAFFKBN.Text, work.WF_SEL_STAFFCODE.Text)
        End If
        leftview.SetListBox(LIST_BOX_CLASSIFICATION.LC_STAFFCODE, WW_DUMMY, prmData)
        Dim WW_STAFF As String = WF_STAFFCODE.Text
        For i As Integer = leftview.WF_LeftListBox.Items.Count - 1 To 0 Step -1
            If leftview.WF_LeftListBox.Items(i).Value = WF_STAFFCODE.Text Then
                Exit For
            End If

            WW_STAFF = leftview.WF_LeftListBox.Items(i).Value
        Next

        CS0026TBLSORT.TABLE = T00013tbl
        CS0026TBLSORT.SORTING = "LINECNT, STAFFCODE, WORKDATE"
        CS0026TBLSORT.FILTER = "STAFFCODE = '" & WW_STAFF & "'"
        CS0026TBLSORT.Sort(T00013INPtbl)

        '○ テーブル保存
        Master.SaveTable(T00013tbl, WF_XMLsaveF.Value)
        Master.SaveTable(T00013INPtbl, WF_XMLsaveF_INP.Value)

        WF_FIELD.Value = ""
        WF_LeftboxOpen.Value = ""
        WF_LeftMViewChange.Value = ""

    End Sub


    ''' <summary>
    ''' 一時保存ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonSAVE_Click()

        '○ 一時保存ファイルに出力
        If Not Master.SaveTable(T00013tbl, work.WF_SEL_XMLsaveTMP.Text) Then
            Exit Sub
        End If

        '○ 従業員名称はブランクに
        work.WF_SEL_STAFFNAMES.Text = ""

        '○ メッセージ表示
        Master.Output(C_MESSAGE_NO.NORMAL, C_MESSAGE_TYPE.INF)

    End Sub


    ''' <summary>
    ''' 絞り込みボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonExtract_Click()

        '○ 使用禁止文字排除
        Master.EraseCharToIgnore(WF_SELSTAFFCODE.Text)

        '○ 名称取得
        CODENAME_get("STAFFCODE", WF_SELSTAFFCODE.Text, WF_SELSTAFFCODE_TEXT.Text, WW_RTN_SW)
        If Not isNormal(WW_RTN_SW) Then
            Master.Output(C_MESSAGE_NO.MASTER_NOT_FOUND_ERROR, C_MESSAGE_TYPE.ERR, "絞込従業員 : " & WF_SELSTAFFCODE.Text)
            Exit Sub
        End If

        '○ 全体データにT00013INPtbl(個人)を反映(削除してマージ)
        CS0026TBLSORT.TABLE = T00013tbl
        CS0026TBLSORT.SORTING = "LINECNT, STAFFCODE, WORKDATE"
        CS0026TBLSORT.FILTER = "STAFFCODE <> '" & WF_STAFFCODE.Text & "'"
        CS0026TBLSORT.Sort(T00013tbl)
        T00013tbl.Merge(T00013INPtbl)

        '○ 画面表示変更
        CS0026TBLSORT.TABLE = T00013tbl
        CS0026TBLSORT.SORTING = "LINECNT, STAFFCODE, WORKDATE"
        If WF_SELSTAFFCODE.Text = "" Then
            '絞込従業員が空欄の場合、今の画面のまま
            CS0026TBLSORT.FILTER = "STAFFCODE = '" & WF_STAFFCODE.Text & "'"
        Else
            '絞込従業員を表示
            CS0026TBLSORT.FILTER = "STAFFCODE = '" & WF_SELSTAFFCODE.Text & "'"
        End If
        CS0026TBLSORT.Sort(T00013INPtbl)

        '○ テーブル保存
        Master.SaveTable(T00013tbl, WF_XMLsaveF.Value)
        Master.SaveTable(T00013INPtbl, WF_XMLsaveF_INP.Value)

    End Sub


    ''' <summary>
    ''' DB更新ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonUPDATE_Click()

        Dim SQLtrn As SqlClient.SqlTransaction = Nothing
        Dim WW_CheckMES1 As String = ""
        Dim WW_CheckMES2 As String = ""

        '○ 現在エラーレコードが1件でもある場合、更新処理を行わない
        Dim WW_ERR As Boolean = False
        For Each T00013row As DataRow In T00013tbl.Rows
            If T00013row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED Then
                WW_CheckMES1 = "エラーデータが存在します。"
                WW_CheckMES2 = ""
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00013row)
                WW_ERR = True
            End If
            '"00:00"～"00:00"（間に空があれば）左詰めする
            If T00013row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING Then
                Dim W00013tbl As DataTable = T00013tbl.Clone
                For i As Integer = 1 To 10
                    Dim WW_STTIME As String = "STTIME" & i.ToString("00")
                    Dim WW_ENDTIME As String = "ENDTIME" & i.ToString("00")
                    Dim W13row As DataRow = W00013tbl.NewRow
                    W13row("CAMPCODE") = T00013row("CAMPCODE")
                    W13row("TAISHOYM") = T00013row("TAISHOYM")
                    W13row("STAFFCODE") = T00013row("STAFFCODE")
                    W13row("WORKDATE") = T00013row("WORKDATE")
                    W13row("WORKKBN") = T00013row("WORKKBN")
                    W13row("STTIME01") = T00013row(WW_STTIME)
                    W13row("ENDTIME01") = T00013row(WW_ENDTIME)
                    W00013tbl.Rows.Add(W13row)

                    T00013row(WW_STTIME) = "00:00"
                    T00013row(WW_ENDTIME) = "00:00"
                Next
                CS0026TBLSORT.TABLE = W00013tbl
                CS0026TBLSORT.SORTING = "STAFFCODE, WORKDATE, STTIME01, ENDTIME01"
                CS0026TBLSORT.FILTER = "STTIME01 <> '00:00' or ENDTIME01 <> '00:00'"
                CS0026TBLSORT.Sort(W00013tbl)

                Dim WW_NO As Integer = 0
                For Each W13row As DataRow In W00013tbl.Rows
                    WW_NO += 1
                    Dim WW_STTIME As String = "STTIME" & WW_NO.ToString("00")
                    Dim WW_ENDTIME As String = "ENDTIME" & WW_NO.ToString("00")
                    T00013row(WW_STTIME) = W13row("STTIME01")
                    T00013row(WW_ENDTIME) = W13row("ENDTIME01")
                Next
            End If
        Next

        If WW_ERR Then
            Master.Output(C_MESSAGE_NO.BOX_ERROR_EXIST, C_MESSAGE_TYPE.ERR)
            Exit Sub
        End If

        '○ 全データをソート
        CS0026TBLSORT.TABLE = T00013tbl
        CS0026TBLSORT.SORTING = "STAFFCODE, WORKDATE"
        CS0026TBLSORT.FILTER = ""
        CS0026TBLSORT.Sort(T00013tbl)

        '○ 休憩・配送時間DB更新用のテーブル作成
        Dim WW_UPDATEtbl As DataTable = New DataTable
        Dim WW_T00013tbl As DataTable = New DataTable
        Dim WW_NOW As DateTime = Date.Now

        CS0026TBLSORT.TABLE = T00013tbl
        CS0026TBLSORT.SORTING = "CAMPCODE, TAISHOYM, STAFFCODE, WORKDATE"
        CS0026TBLSORT.FILTER = "OPERATION = '" & C_LIST_OPERATION_CODE.UPDATING & "' and SELECT = '1'"
        CS0026TBLSORT.Sort(WW_UPDATEtbl)

        '○ 休憩・配送時間DB出力編集
        T0013UPDATE.AddColumnT0013UPDtbl(WW_T00013tbl)
        UpdTableEdit(WW_UPDATEtbl, WW_T00013tbl, WW_NOW)

        '○ 休憩・配送時間DB更新
        Using SQLcon As SqlConnection = CS0050SESSION.getConnection()
            SQLcon.Open() 'DataBase接続(Open)
            T0013UPDATE.SQLcon = SQLcon
            T0013UPDATE.SQLtrn = SQLtrn
            T0013UPDATE.UPDUSERID = Master.USERID
            T0013UPDATE.UPDTERMID = Master.USERTERMID
            For Each T13row As DataRow In WW_T00013tbl.Rows
                T0013UPDATE.UpdateT0013(T13row, WW_ERR_SW)
                If Not isNormal(WW_ERR_SW) Then
                    Exit Sub
                End If
            Next
        End Using

        '○ テーブル初期化
        If Not IsNothing(WW_UPDATEtbl) Then
            WW_UPDATEtbl.Clear()
            WW_UPDATEtbl.Dispose()
            WW_UPDATEtbl = Nothing
        End If

        If Not IsNothing(WW_T00013tbl) Then
            WW_T00013tbl.Clear()
            WW_T00013tbl.Dispose()
            WW_T00013tbl = Nothing
        End If

        If Not IsNothing(T00013tbl) Then
            T00013tbl.Clear()
            T00013tbl.Dispose()
            T00013tbl = Nothing
        End If

        If Not IsNothing(T00013INPtbl) Then
            T00013INPtbl.Clear()
            T00013INPtbl.Dispose()
            T00013INPtbl = Nothing
        End If

        '○ 画面表示データ再取得
        Using SQLcon As SqlConnection = CS0050SESSION.getConnection()
            SQLcon.Open()       'DataBase接続

            MAPDataGet(SQLcon)
        End Using

        '○ 画面表示データ保存
        Master.SaveTable(T00013tbl, WF_XMLsaveF.Value)

        '○ 現在表示している乗務員分のデータを格納
        CS0026TBLSORT.TABLE = T00013tbl
        CS0026TBLSORT.SORTING = "LINECNT, STAFFCODE, WORKDATE"
        CS0026TBLSORT.FILTER = "STAFFCODE = '" & WF_STAFFCODE.Text & "'"
        CS0026TBLSORT.sort(T00013INPtbl)
        Master.SaveTable(T00013INPtbl, WF_XMLsaveF_INP.Value)

    End Sub

    ''' <summary>
    ''' 更新テーブル編集
    ''' </summary>
    ''' <param name="I_TABLE"></param>
    ''' <param name="O_TABLE"></param>
    ''' <param name="I_NOW"></param>
    ''' <remarks></remarks>
    Protected Sub UpdTableEdit(ByVal I_TABLE As DataTable, ByRef O_TABLE As DataTable, ByVal I_NOW As DateTime)

        For Each I_ROW As DataRow In I_TABLE.Rows
            Dim O_ROW As DataRow = O_TABLE.NewRow

            O_ROW("CAMPCODE") = I_ROW("CAMPCODE")                                               '会社コード
            O_ROW("TAISHOYM") = I_ROW("TAISHOYM")                                               '対象年月
            O_ROW("STAFFCODE") = I_ROW("STAFFCODE")                                             '従業員コード
            O_ROW("WORKDATE") = I_ROW("WORKDATE")                                               '勤務年月日
            O_ROW("WORKKBN") = I_ROW("WORKKBN")                                                 '作業区分

            For i As Integer = 1 To 10
                Dim WW_STTIME As String = "STTIME" & i.ToString("00")
                Dim WW_ENDTIME As String = "ENDTIME" & i.ToString("00")
                '開始時刻
                If IsDate(I_ROW(WW_STTIME)) Then
                    O_ROW(WW_STTIME) = I_ROW(WW_STTIME)
                Else
                    O_ROW(WW_STTIME) = "00:00"
                End If

                '終了時刻
                If IsDate(I_ROW(WW_ENDTIME)) Then
                    O_ROW(WW_ENDTIME) = I_ROW(WW_ENDTIME)
                Else
                    O_ROW(WW_ENDTIME) = "00:00"
                End If

            Next

            O_ROW("TTLTIME") = T0007COM.HHMMtoMinutes(I_ROW("TTLTIME"))                         '合計時間
            If O_ROW("TTLTIME") = 0 Then
                O_ROW("DELFLG") = C_DELETE_FLG.DELETE                                           '削除フラグ
            Else
                O_ROW("DELFLG") = C_DELETE_FLG.ALIVE                                            '削除フラグ
            End If
            O_ROW("INITYMD") = I_NOW                                                            '登録年月日
            O_ROW("UPDYMD") = I_NOW                                                             '更新年月日
            O_ROW("UPDUSER") = Master.USERID                                                    '更新ユーザID
            O_ROW("UPDTERMID") = Master.USERTERMID                                              '更新端末
            O_ROW("RECEIVEYMD") = C_DEFAULT_YMD                                                 '集信日時

            O_TABLE.Rows.Add(O_ROW)
        Next

    End Sub

    ''' <summary>
    ''' ダウンロード、一覧印刷ボタン押下時処理
    ''' </summary>
    ''' <param name="I_FILETYPE"></param>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonPrint_Click(ByVal I_FILETYPE As String)

        Dim TBLview As DataView = New DataView(T00013tbl)
        TBLview.Sort = "SEQ, STAFFCODE, WORKDATE"

        '○ 一部画面表示編集(00:00をブランクに変更)
        ZeroToBlank(TBLview)

        '○ 帳票出力
        CS0030REPORT.CAMPCODE = work.WF_SEL_CAMPCODE.Text               '会社コード
        CS0030REPORT.PROFID = Master.PROF_REPORT                        'プロファイルID
        CS0030REPORT.MAPID = Master.MAPID                               '画面ID
        CS0030REPORT.REPORTID = rightview.GetReportId()                 '帳票ID
        CS0030REPORT.FILEtyp = I_FILETYPE                               '出力ファイル形式
        CS0030REPORT.TBLDATA = TBLview.ToTable                          'データ参照Table
        CS0030REPORT.TARGETDATE = work.WF_SEL_TAISHOYM.Text & "/01"     '対象日付
        CS0030REPORT.CS0030REPORT()
        If Not isNormal(CS0030REPORT.ERR) Then
            If CS0030REPORT.ERR = C_MESSAGE_NO.REPORT_EXCEL_NOT_FOUND_ERROR Then
                Master.output(CS0030REPORT.ERR, C_MESSAGE_TYPE.ERR)
            Else
                Master.output(CS0030REPORT.ERR, C_MESSAGE_TYPE.ABORT, "CS0030REPORT")
            End If
            Exit Sub
        End If

        '○ 別画面でPDFを表示
        WF_PrintURL.Value = CS0030REPORT.URL
        If I_FILETYPE = "XLSX" Then
            ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_ExcelPrint();", True)
        Else
            ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_PDFPrint();", True)
        End If

    End Sub

    ''' <summary>
    ''' 終了ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonEND_Click()

        '前画面に戻る
        Master.TransitionPrevPage()

    End Sub


    ''' <summary>
    ''' ファイルアップロード時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_FILEUPLOAD()

        '○ エラーレポート準備
        rightview.setErrorReport("")

        '○ UPLOAD XLSデータ取得
        CS0023XLSUPLOAD.CAMPCODE = work.WF_SEL_CAMPCODE.Text        '会社コード
        CS0023XLSUPLOAD.MAPID = Master.MAPID                        '画面ID
        CS0023XLSUPLOAD.CS0023XLSUPLOAD(String.Empty, Master.PROF_REPORT)
        If isNormal(CS0023XLSUPLOAD.ERR) Then
            If CS0023XLSUPLOAD.TBLDATA.Rows.Count = 0 Then
                Master.Output(C_MESSAGE_NO.REGISTRATION_RECORD_NOT_EXIST_ERROR, C_MESSAGE_TYPE.ERR)
                Exit Sub
            End If
        Else
            Master.output(CS0023XLSUPLOAD.ERR, C_MESSAGE_TYPE.ABORT, "CS0023XLSUPLOAD")
            Exit Sub
        End If

        '○ インポートファイルの列情報有り無し判定
        Master.CreateEmptyTable(T00013INPtbl, WF_XMLsaveF.Value)
        ExcelInpMake(CS0023XLSUPLOAD.TBLDATA)

        '○ INPデータチェック
        For Each T00013INProw As DataRow In T00013INPtbl.Rows
            INPTableCheck(T00013INProw, WW_ERR_SW)

            If isNormal(WW_ERR_SW) Then
                T00013INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                T00013INProw("SELECT") = 1
            Else
                T00013INProw("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                T00013INProw("SELECT") = 0
            End If
        Next

        '○ 重大エラーの場合、インポートデータから削除
        For i As Integer = T00013INPtbl.Rows.Count - 1 To 0 Step -1
            If T00013INPtbl.Rows(i)("SELECT") = 0 Then
                T00013INPtbl.Rows(i).Delete()
            End If
        Next

        '○ 画面表示の従業員のみ抽出
        Dim WW_COLs As String() = {"STAFFCODE"}
        Dim WW_KEYtbl As DataTable = New DataTable
        Dim TBLview As DataView = New DataView(T00013tbl)
        WW_KEYtbl = TBLview.ToTable(True, WW_COLs)

        Dim WW_FIND As Boolean = False
        For i As Integer = T00013INPtbl.Rows.Count - 1 To 0 Step -1
            WW_FIND = False
            For Each WW_KEYrow As DataRow In WW_KEYtbl.Rows
                If WW_KEYrow("STAFFCODE") = T00013INPtbl.Rows(i)("STAFFCODE") Then
                    WW_FIND = True
                    Exit For
                End If
            Next

            If Not WW_FIND Then
                Dim WW_CheckMES1 As String = "・更新できないレコード(従業員エラー)です。"
                Dim WW_CheckMES2 As String = "画面選択されていない従業員です。"
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00013INPtbl.Rows(i))

                T00013INPtbl.Rows(i).Delete()
            End If
        Next

        TBLview.Dispose()
        TBLview = Nothing

        If Not IsNothing(WW_KEYtbl) Then
            WW_KEYtbl.Clear()
            WW_KEYtbl.Dispose()
            WW_KEYtbl = Nothing
        End If


        '○ テーブルソート
        CS0026TBLSORT.TABLE = T00013INPtbl
        CS0026TBLSORT.SORTING = "SEQ, STAFFCODE, WORKDATE"
        CS0026TBLSORT.FILTER = ""
        CS0026TBLSORT.sort(T00013INPtbl)

        CS0026TBLSORT.TABLE = T00013tbl
        CS0026TBLSORT.SORTING = "SEQ, STAFFCODE, WORKDATE"
        CS0026TBLSORT.FILTER = ""
        CS0026TBLSORT.sort(T00013tbl)

        Dim WW_INDEX As Integer = 0
        Dim WW_KEY_INP As String = ""
        Dim WW_KEY_TBL As String = ""

        For Each T00013INProw As DataRow In T00013INPtbl.Rows
            WW_KEY_INP = T00013INProw("STAFFCODE") & T00013INProw("WORKDATE") & T00013INProw("WORKKBN")

            If T00013INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING Then
                For i As Integer = WW_INDEX To T00013tbl.Rows.Count - 1
                    Dim T00013row As DataRow = T00013tbl.Rows(i)
                    WW_KEY_TBL = T00013row("STAFFCODE") & T00013row("WORKDATE") & T00013row("WORKKBN")

                    If WW_KEY_TBL < WW_KEY_INP Then
                        Continue For
                    End If

                    If WW_KEY_TBL = WW_KEY_INP Then
                        T00013row("OPERATION") = T00013INProw("OPERATION")
                        T00013row("SELECT") = 0
                        T00013row("HIDDEN") = 1
                        T00013row("DELFLG") = C_DELETE_FLG.DELETE
                    End If

                    If WW_KEY_TBL > WW_KEY_INP Then
                        WW_INDEX = i
                        Exit For
                    End If
                Next
            End If
        Next

        '○ 当画面で生成したデータ(タイムスタンプ = 0)に対する変更は、変更前を削除する
        For i As Integer = T00013tbl.Rows.Count - 1 To 0 Step -1
            If T00013tbl.Rows(i)("TIMSTP") = 0 AndAlso
                T00013tbl.Rows(i)("SELECT") = 0 Then
                T00013tbl.Rows(i).Delete()
            End If
        Next

        T00013tbl.Merge(T00013INPtbl)

        '○ 画面表示用データ
        CS0026TBLSORT.TABLE = T00013tbl
        CS0026TBLSORT.SORTING = "SEQ, STAFFCODE, WORKDATE"
        CS0026TBLSORT.FILTER = "STAFFCODE = '" & WF_STAFFCODE.Text & "' and SELECT = 1 and DELFLG = '" & C_DELETE_FLG.ALIVE & "'"
        CS0026TBLSORT.Sort(T00013INPtbl)

        Master.SaveTable(T00013tbl, WF_XMLsaveF.Value)
        Master.SaveTable(T00013INPtbl, WF_XMLsaveF_INP.Value)

        If isNormal(WW_ERR_SW) Then
            Master.Output(C_MESSAGE_NO.NORMAL, C_MESSAGE_TYPE.NOR)
        Else
            Master.Output(WW_ERR_SW, C_MESSAGE_TYPE.ERR)
        End If

        '○ Close
        CS0023XLSUPLOAD.TBLDATA.Dispose()
        CS0023XLSUPLOAD.TBLDATA.Clear()

    End Sub

    ''' <summary>
    ''' インポートデータを取得
    ''' </summary>
    ''' <param name="I_TABLE"></param>
    ''' <remarks></remarks>
    Protected Sub ExcelInpMake(ByVal I_TABLE As DataTable)

        '○ CS0023XLSUPLOAD.TBLDATAの入力値整備
        Dim WW_COLUMNS As New List(Of String)
        For Each TBLcol As DataColumn In I_TABLE.Columns
            WW_COLUMNS.Add(TBLcol.ColumnName.ToString())
        Next

        Dim WW_ROW As DataRow = I_TABLE.NewRow
        For Each TBLrow As DataRow In I_TABLE.Rows
            WW_ROW.ItemArray = TBLrow.ItemArray

            For Each TBLcol As DataColumn In I_TABLE.Columns
                If IsDBNull(WW_ROW.Item(TBLcol)) OrElse IsNothing(WW_ROW.Item(TBLcol)) Then
                    WW_ROW.Item(TBLcol) = ""
                End If
            Next

            TBLrow.ItemArray = WW_ROW.ItemArray
        Next

        For Each TBLrow As DataRow In I_TABLE.Rows
            Dim T00013INProw As DataRow = T00013INPtbl.NewRow

            '初期クリア
            For Each T00013INPcol As DataColumn In T00013INPtbl.Columns
                If IsDBNull(T00013INProw.Item(T00013INPcol)) OrElse IsNothing(T00013INProw.Item(T00013INPcol)) Then
                    Select Case T00013INPcol.ColumnName
                        Case "LINECNT"
                            T00013INProw.Item(T00013INPcol) = 0
                        Case "OPERATION"
                            T00013INProw.Item(T00013INPcol) = C_LIST_OPERATION_CODE.NODATA
                        Case "TIMSTP"
                            T00013INProw.Item(T00013INPcol) = 0
                        Case "SELECT"
                            T00013INProw.Item(T00013INPcol) = 1
                        Case "HIDDEN"
                            T00013INProw.Item(T00013INPcol) = 0
                        Case "SEQ"
                            T00013INProw.Item(T00013INPcol) = 0
                        Case Else
                            T00013INProw.Item(T00013INPcol) = ""
                    End Select
                End If
            Next

            '共通項目
            T00013INProw("LINECNT") = 0
            T00013INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
            T00013INProw("TIMSTP") = 0
            T00013INProw("SELECT") = 1
            T00013INProw("HIDDEN") = 0

            T00013INProw("SEQ") = 0

            '○ 項目セット
            '会社コード
            If WW_COLUMNS.IndexOf("CAMPCODE") >= 0 Then
                T00013INProw("CAMPCODE") = TBLrow("CAMPCODE")
            Else
                T00013INProw("CAMPCODE") = work.WF_SEL_CAMPCODE.Text
            End If

            '対象年月
            If WW_COLUMNS.IndexOf("TAISHOYM") >= 0 Then
                T00013INProw("TAISHOYM") = TBLrow("TAISHOYM")
            Else
                T00013INProw("TAISHOYM") = ""
            End If

            '従業員コード
            If WW_COLUMNS.IndexOf("STAFFCODE") >= 0 Then
                T00013INProw("STAFFCODE") = TBLrow("STAFFCODE")
            Else
                T00013INProw("STAFFCODE") = ""
            End If

            '作業区分
            If WW_COLUMNS.IndexOf("WORKKBN") >= 0 Then
                T00013INProw("WORKKBN") = TBLrow("WORKKBN")
            Else
                T00013INProw("WORKKBN") = ""
            End If

            '勤務年月日
            If WW_COLUMNS.IndexOf("WORKDATE") >= 0 Then
                Dim WW_DATE As Date
                Try
                    Date.TryParse(TBLrow("WORKDATE"), WW_DATE)
                    T00013INProw("WORKDATE") = WW_DATE.ToString("yyyy/MM/dd")
                    T00013INProw("WORKDAY") = WW_DATE.ToString("dd")
                Catch ex As Exception
                    T00013INProw("WORKDATE") = ""
                End Try
            Else
                T00013INProw("WORKDATE") = ""
            End If

            '配属部署
            If WW_COLUMNS.IndexOf("HORG") >= 0 Then
                T00013INProw("HORG") = TBLrow("HORG")
            Else
                T00013INProw("HORG") = ""
            End If

            '曜日
            If WW_COLUMNS.IndexOf("WORKINGWEEK") >= 0 Then
                T00013INProw("WORKINGWEEK") = TBLrow("WORKINGWEEK")
            Else
                T00013INProw("WORKINGWEEK") = ""
            End If

            '開始時刻０１～１０、終了時刻０１～１０
            For i As Integer = 1 To 10
                Dim WW_STTIME As String = "STTIME" & i.ToString("00")
                Dim WW_ENDTIME As String = "ENDTIME" & i.ToString("00")
                '開始時刻
                If WW_COLUMNS.IndexOf(WW_STTIME) >= 0 Then
                    Dim WW_TIME As Date
                    Try
                        Date.TryParse(TBLrow(WW_STTIME), WW_TIME)
                        T00013INProw(WW_STTIME) = WW_TIME.ToString("HH:mm")
                    Catch ex As Exception
                        T00013INProw(WW_STTIME) = ""
                    End Try
                Else
                    T00013INProw(WW_STTIME) = "00:00"
                End If

                '終了時刻
                If WW_COLUMNS.IndexOf(WW_ENDTIME) >= 0 Then
                    Dim WW_TIME As Date
                    Try
                        Date.TryParse(TBLrow(WW_ENDTIME), WW_TIME)
                        T00013INProw(WW_ENDTIME) = WW_TIME.ToString("HH:mm")
                    Catch ex As Exception
                        T00013INProw(WW_ENDTIME) = ""
                    End Try
                Else
                    T00013INProw(WW_ENDTIME) = "00:00"
                End If
            Next

            '合計時間
            If WW_COLUMNS.IndexOf("TTLTIME") >= 0 Then
                T00013INProw("TTLTIME") = TBLrow("TTLTIME")
            Else
                T00013INProw("TTLTIME") = "00:00"
            End If

            '削除フラグ
            If WW_COLUMNS.IndexOf("DELFLG") >= 0 Then
                T00013INProw("DELFLG") = TBLrow("DELFLG")
            Else
                T00013INProw("DELFLG") = C_DELETE_FLG.ALIVE
            End If

            T00013INPtbl.Rows.Add(T00013INProw)
        Next

    End Sub


    ''' <summary>
    ''' リスト変更時処理
    ''' </summary>
    ''' <param name="I_CHANGED"></param>
    ''' <remarks></remarks>
    Protected Sub WF_ListChange(Optional ByVal I_CHANGED As String = "")

        Dim WW_WORKDATE As Date
        rightview.SetErrorReport("")
        Dim timef As New GRT00009TIMEFORMAT

        '○ 変更箇所の日付を取得
        Try
            Date.TryParse(Convert.ToString(WF_TAISHOYM.Text & "/" & WF_SelectedIndex.Value), WW_WORKDATE)

            'Date.TryParse(Convert.ToString(Request.Form("txt" & pnlListArea.ID & "WORKDATE" & WF_SelectedIndex.Value)), WW_WORKDATE)
        Catch ex As Exception
            Exit Sub
        End Try

        '○ 画面項目チェック
        For Each T00013INProw As DataRow In T00013INPtbl.Rows
            If T00013INProw("STAFFCODE") <> WF_STAFFCODE.Text OrElse
               T00013INProw("WORKDATE") <> WW_WORKDATE.ToString("yyyy/MM/dd") Then
                Continue For
            End If

            '変更内容取得(入力禁止文字除外)
            '開始時刻１～１０
            For i As Integer = 1 To 10
                Dim WW_STTIME As String = "STTIME" & i.ToString("00")
                Dim WW_ENDTIME As String = "ENDTIME" & i.ToString("00")
                If Not IsNothing(Request.Form("txt" & pnlListArea.ID & WW_STTIME & WF_SelectedIndex.Value)) Then
                    If T00013INProw(WW_STTIME) <> Convert.ToString(Request.Form("txt" & pnlListArea.ID & WW_STTIME & WF_SelectedIndex.Value)) Then
                        T00013INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                    End If
                    T00013INProw(WW_STTIME) = Convert.ToString(Request.Form("txt" & pnlListArea.ID & WW_STTIME & WF_SelectedIndex.Value))
                End If
                Master.EraseCharToIgnore(T00013INProw(WW_STTIME))
                T00013INProw(WW_STTIME) = timef.FormatHHMM(T00013INProw(WW_STTIME))

                If Not IsNothing(Request.Form("txt" & pnlListArea.ID & WW_ENDTIME & WF_SelectedIndex.Value)) Then
                    If T00013INProw(WW_ENDTIME) <> Convert.ToString(Request.Form("txt" & pnlListArea.ID & WW_ENDTIME & WF_SelectedIndex.Value)) Then
                        T00013INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                    End If
                    T00013INProw(WW_ENDTIME) = Convert.ToString(Request.Form("txt" & pnlListArea.ID & WW_ENDTIME & WF_SelectedIndex.Value))
                End If
                Master.EraseCharToIgnore(T00013INProw(WW_ENDTIME))
                T00013INProw(WW_ENDTIME) = timef.FormatHHMM(T00013INProw(WW_ENDTIME))
            Next

            If T00013INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING Then
                T00013INProw("TIMSTP") = 0
            End If

            '項目チェック
            INPTableCheck(T00013INProw, WW_ERR_SW)
            If Not isNormal(WW_ERR_SW) Then
                T00013INProw("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                Master.output(WW_ERR_SW, C_MESSAGE_TYPE.ABORT)
            End If
        Next

        '全体データにINPtblに反映(削除してマージ)
        CS0026TBLSORT.TABLE = T00013tbl
        CS0026TBLSORT.SORTING = "LINECNT, STAFFCODE, WORKDATE"
        CS0026TBLSORT.FILTER = "STAFFCODE <> '" & WF_STAFFCODE.Text & "'"
        CS0026TBLSORT.Sort(T00013tbl)
        T00013tbl.Merge(T00013INPtbl)

        '○ 画面表示データ保存
        Master.SaveTable(T00013tbl, WF_XMLsaveF.Value)
        Master.SaveTable(T00013INPtbl, WF_XMLsaveF_INP.Value)

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
                Dim prmData As New Hashtable
                prmData.Item(C_PARAMETERS.LP_COMPANY) = work.WF_SEL_CAMPCODE.Text

                Select Case WF_FIELD.Value
                    Case "WF_SELSTAFFCODE"          '従業員コード
                        If Trim(work.WF_SEL_HORG.Text) = "" Then
                            prmData = work.CreateStaffCodeParam(GL0005StaffList.LC_STAFF_TYPE.DRIVER, work.WF_SEL_CAMPCODE.Text,
                                    work.WF_SEL_TAISHOYM.Text, work.WF_SEL_HORG.Text, work.WF_SEL_STAFFKBN.Text, work.WF_SEL_STAFFCODE.Text)
                        Else
                            prmData = work.CreateStaffCodeParam(GL0005StaffList.LC_STAFF_TYPE.ATTENDANCE_FOR_DRIVER, work.WF_SEL_CAMPCODE.Text,
                                    work.WF_SEL_TAISHOYM.Text, work.WF_SEL_HORG.Text, work.WF_SEL_STAFFKBN.Text, work.WF_SEL_STAFFCODE.Text)
                        End If
                    Case "PAYKBN"                   '勤怠区分
                        prmData.Item(C_PARAMETERS.LP_FIX_CLASS) = "PAYKBN"
                        WF_LeftMViewChange.Value = LIST_BOX_CLASSIFICATION.LC_FIX_VALUE
                    Case "SHUKCHOKKBN"              '宿日直区分
                        prmData = work.CreateShukchokKBNParam()
                        WF_LeftMViewChange.Value = LIST_BOX_CLASSIFICATION.LC_EXTRA_LIST
                    Case "RIYU"                     '残業理由
                        prmData.Item(C_PARAMETERS.LP_FIX_CLASS) = "T0009_RIYU"
                        WF_LeftMViewChange.Value = LIST_BOX_CLASSIFICATION.LC_FIX_VALUE
                End Select

                .setListBox(WF_LeftMViewChange.Value, WW_DUMMY, prmData)
                .activeListBox()
            End With
        End If

    End Sub


    ' ******************************************************************************
    ' ***  leftBOX関連操作                                                       ***
    ' ******************************************************************************

    ''' <summary>
    ''' LeftBox選択時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonSel_Click()

        Dim WW_SelectValue As String = ""
        Dim WW_SelectText As String = ""

        '○ 選択内容を取得
        If Not IsNothing(leftview.getActiveValue) Then
            WW_SelectValue = leftview.getActiveValue(0)
            WW_SelectText = leftview.getActiveValue(1)
        End If

        '○ 選択内容を画面項目へセット
        If WF_FIELD.Value = "WF_SELSTAFFCODE" Then
            '従業員コード(絞込条件)
            WF_SELSTAFFCODE.Text = WW_SelectValue
            WF_SELSTAFFCODE_TEXT.Text = WW_SelectText
            WF_SELSTAFFCODE.Focus()
        End If

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
            Case "WF_SELSTAFFCODE"          '従業員コード
                WF_SELSTAFFCODE.Focus()
        End Select

        '○ 画面左右ボックス非表示は、画面JavaScript(InitLoad)で実行
        WF_FIELD.Value = ""
        WF_LeftboxOpen.Value = ""
        WF_LeftMViewChange.Value = ""

    End Sub


    ''' <summary>
    ''' RightBoxラジオボタン選択処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_RadioButton_Click()

        If Not String.IsNullOrEmpty(WF_RightViewChange.Value) Then
            Try
                Integer.TryParse(WF_RightViewChange.Value, WF_RightViewChange.Value)
            Catch ex As Exception
                Exit Sub
            End Try

            rightview.selectIndex(WF_RightViewChange.Value)
            WF_RightViewChange.Value = ""
        End If

    End Sub

    ''' <summary>
    ''' RightBoxメモ欄更新
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_RIGHTBOX_Change()

        rightview.save(Master.USERID, Master.USERTERMID, WW_DUMMY)

    End Sub


    ''' <summary>
    ''' ヘルプ表示
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_HELP_Click()

        Master.showHelp()

    End Sub


    ' ******************************************************************************
    ' ***  共通処理                                                              ***
    ' ******************************************************************************

    ''' <summary>
    ''' 入力値チェック
    ''' </summary>
    ''' <param name="T00013INProw"></param>
    ''' <param name="O_RTN"></param>
    ''' <remarks></remarks>
    Protected Sub INPTableCheck(ByRef T00013INProw As DataRow, ByRef O_RTN As String)

        O_RTN = C_MESSAGE_NO.NORMAL

        Dim WW_CheckMES1 As String = ""
        Dim WW_CheckMES2 As String = ""
        Dim WW_CS0036FCHECKERR As String = ""
        Dim WW_CS0036FCHECKREPORT As String = ""
        Dim WW_S0013tbl As DataTable = New DataTable

        '○ 単項目チェック
        '会社コード
        Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "CAMPCODE", T00013INProw("CAMPCODE"), WW_CS0036FCHECKERR, WW_CS0036FCHECKREPORT, WW_S0013tbl)
        If isNormal(WW_CS0036FCHECKERR) Then
            '存在チェック
            CODENAME_get("CAMPCODE", T00013INProw("CAMPCODE"), T00013INProw("CAMPNAMES"), WW_RTN_SW)
            If Not isNormal(WW_RTN_SW) Then
                WW_CheckMES1 = "・更新できないレコード(会社コードエラー)です。"
                WW_CheckMES2 = "マスタに存在しません。(" & T00013INProw("CAMPCODE") & ")"
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00013INProw)
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
        Else
            WW_CheckMES1 = "・更新できないレコード(会社コードエラー)です。"
            WW_CheckMES2 = WW_CS0036FCHECKREPORT
            WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00013INProw)
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If

        '対象年月
        If IsDate(T00013INProw("TAISHOYM")) Then
            If CDate(T00013INProw("TAISHOYM")).ToString("yyyy/MM") = WF_TAISHOYM.Text Then
            Else
                WW_CheckMES1 = "・更新できないレコード(対象年月エラー)です。"
                WW_CheckMES2 = T00013INProw("TAISHOYM")
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00013INProw)
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
        Else
            WW_CheckMES1 = "・更新できないレコード(対象年月不正)です。"
            WW_CheckMES2 = T00013INProw("TAISHOYM")
            WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00013INProw)
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If

        '従業員コード
        Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "STAFFCODE", T00013INProw("STAFFCODE"), WW_CS0036FCHECKERR, WW_CS0036FCHECKREPORT, WW_S0013tbl)
        If isNormal(WW_CS0036FCHECKERR) Then
            '存在チェック
            CODENAME_get("STAFFCODE", T00013INProw("STAFFCODE"), T00013INProw("STAFFNAMES"), WW_RTN_SW)
            If Not isNormal(WW_RTN_SW) Then
                WW_CheckMES1 = "・更新できないレコード(従業員コードエラー)です。"
                WW_CheckMES2 = "マスタに存在しません。(" & T00013INProw("STAFFCODE") & ")"
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00013INProw)
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
        Else
            WW_CheckMES1 = "・更新できないレコード(従業員コードエラー)です。"
            WW_CheckMES2 = WW_CS0036FCHECKREPORT
            WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00013INProw)
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If

        '作業区分コード
        Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "WORKKBN", T00013INProw("WORKKBN"), WW_CS0036FCHECKERR, WW_CS0036FCHECKREPORT, WW_S0013tbl)
        If isNormal(WW_CS0036FCHECKERR) Then
            If T00013INProw("WORKKBN") <> work.WF_SEL_WORKKBN.Text Then
                WW_CheckMES1 = "・更新できないレコード(作業区分エラー)です。"
                WW_CheckMES2 = "選択画面の選択（休憩（BB）or 配送（G1））と入力データが一致しません。"
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00013INProw)
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            Else
                '存在チェック
                CODENAME_get("WORKKBN", T00013INProw("WORKKBN"), T00013INProw("WORKKBNNAMES"), WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・更新できないレコード(作業区分エラー)です。"
                    WW_CheckMES2 = "マスタに存在しません。(" & T00013INProw("WORKKBN") & ")"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00013INProw)
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If
        Else
            WW_CheckMES1 = "・更新できないレコード(作業区分エラー)です。"
            WW_CheckMES2 = WW_CS0036FCHECKREPORT
            WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00013INProw)
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If

        '勤務年月日
        If String.IsNullOrEmpty(T00013INProw("WORKDATE")) Then
            WW_CheckMES1 = "・更新できないレコード(勤務年月日無)です。"
            WW_CheckMES2 = ""
            WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00013INProw)
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        Else
            Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "WORKDATE", T00013INProw("WORKDATE"), WW_CS0036FCHECKERR, WW_CS0036FCHECKREPORT, WW_S0013tbl)
            If isNormal(WW_CS0036FCHECKERR) Then
                '対象年月チェック
                If IsDate(T00013INProw("WORKDATE")) AndAlso
                   CDate(T00013INProw("WORKDATE")).ToString("yyyy/MM") <> WF_TAISHOYM.Text Then
                    WW_CheckMES1 = "・更新できないレコード(勤務年月日不正)です。"
                    WW_CheckMES2 = T00013INProw("WORKDATE")
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00013INProw)
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(勤務年月日エラー)です。"
                WW_CheckMES2 = WW_CS0036FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00013INProw)
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
        End If

        Dim WW_TTLTIME As Integer = 0
        For i As Integer = 1 To 10
            Dim WW_STTIME As String = "STTIME" & i.ToString("00")
            Dim WW_ENDTIME As String = "ENDTIME" & i.ToString("00")
            Dim WW_STRTN As String = C_MESSAGE_NO.NORMAL
            Dim WW_ENDRTN As String = C_MESSAGE_NO.NORMAL

            '開始時刻
            If String.IsNullOrEmpty(T00013INProw(WW_STTIME)) Then
                T00013INProw(WW_STTIME) = "00:00"
            Else
                Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "STTIME", T00013INProw(WW_STTIME), WW_CS0036FCHECKERR, WW_CS0036FCHECKREPORT, WW_S0013tbl)
                If isNormal(WW_CS0036FCHECKERR) Then
                    T00013INProw(WW_STTIME) = CDate(T00013INProw(WW_STTIME)).ToString("HH:mm")
                Else
                    WW_CheckMES1 = "・更新できないレコード(開始時刻" & i.ToString("00") & "エラー)です。"
                    WW_CheckMES2 = WW_CS0036FCHECKREPORT
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00013INProw)
                    WW_STRTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    O_RTN = WW_STRTN
                End If
            End If

            '終了時刻
            If String.IsNullOrEmpty(T00013INProw(WW_ENDTIME)) Then
                T00013INProw(WW_ENDTIME) = "00:00"
            Else
                Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "ENDTIME", T00013INProw(WW_ENDTIME), WW_CS0036FCHECKERR, WW_CS0036FCHECKREPORT, WW_S0013tbl)
                If isNormal(WW_CS0036FCHECKERR) Then
                    T00013INProw(WW_ENDTIME) = CDate(T00013INProw(WW_ENDTIME)).ToString("HH:mm")
                Else
                    WW_CheckMES1 = "・更新できないレコード(退社時刻" & i.ToString("00") & "エラー)です。"
                    WW_CheckMES2 = WW_CS0036FCHECKREPORT
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00013INProw)
                    WW_ENDRTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    O_RTN = WW_ENDRTN
                End If
            End If
            If isNormal(WW_STRTN) AndAlso isNormal(WW_ENDRTN) Then
                Dim WW_DATE_ST As String = T00013INProw("WORKDATE") & " " & T00013INProw(WW_STTIME)
                Dim WW_DATE_END As String = T00013INProw("WORKDATE") & " " & T00013INProw(WW_ENDTIME)

                If IsDate(WW_DATE_ST) AndAlso IsDate(WW_DATE_END) Then
                    If DateDiff("n", WW_DATE_ST, WW_DATE_END) < 0 Then
                        WW_DATE_END = CDate(T00013INProw("WORKDATE")).AddDays(1) & " " & T00013INProw(WW_ENDTIME)
                        If DateDiff("n", WW_DATE_ST, WW_DATE_END) >= 0 Then
                            WW_TTLTIME += DateDiff("n", WW_DATE_ST, WW_DATE_END)
                        End If
                    Else
                        WW_TTLTIME += DateDiff("n", WW_DATE_ST, WW_DATE_END)
                    End If
                End If
            End If
        Next
        T00013INProw("TTLTIME") = T0007COM.formatHHMM(WW_TTLTIME)
    End Sub

    ''' <summary>
    ''' エラーレポート編集
    ''' </summary>
    ''' <param name="MESSAGE1"></param>
    ''' <param name="MESSAGE2"></param>
    ''' <param name="T00013row"></param>
    ''' <remarks></remarks>
    Protected Sub WW_CheckERR(ByVal MESSAGE1 As String, ByVal MESSAGE2 As String, Optional ByVal T00013row As DataRow = Nothing)

        Dim WW_ERR_MES As String = ""
        WW_ERR_MES = MESSAGE1
        If MESSAGE2 <> "" Then
            WW_ERR_MES &= ControlChars.NewLine & "  --> " & MESSAGE2 & " , "
        End If

        If Not IsNothing(T00013row) Then
            WW_ERR_MES &= ControlChars.NewLine & "  --> 会社     =" & T00013row("CAMPCODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 従業員   =" & T00013row("STAFFCODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 従業員名 =" & T00013row("STAFFNAMES") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 配属部署 =" & T00013row("HORG") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 部署名   =" & T00013row("HORGNAMES") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 日付     =" & T00013row("WORKDATE")
        End If

        rightview.addErrorReport(WW_ERR_MES)

    End Sub

    ''' <summary>
    ''' DataRowをカンマ区切り文字列に変換
    ''' </summary>
    ''' <param name="I_ROW"></param>
    ''' <returns>カンマ区切り文字列</returns>
    ''' <remarks></remarks>
    Protected Function DataRowToCSV(ByVal I_ROW As DataRow) As String

        Dim O_CSV = ""

        If IsNothing(I_ROW) Then
            DataRowToCSV = O_CSV
        End If

        For i As Integer = 0 To I_ROW.ItemArray.Count - 1
            If i = 0 Then
                O_CSV = I_ROW.ItemArray(i).ToString()
            Else
                O_CSV = O_CSV & ControlChars.Tab & I_ROW.ItemArray(i).ToString()
            End If
        Next

        DataRowToCSV = O_CSV

    End Function

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
        prmData.Item(C_PARAMETERS.LP_COMPANY) = work.WF_SEL_CAMPCODE.Text

        Try
            Select Case I_FIELD
                Case "CAMPCODE"             '会社コード
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_COMPANY, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "STAFFCODE"            '従業員コード
                    If Trim(work.WF_SEL_HORG.Text) = "" Then
                        prmData = work.CreateStaffCodeParam(GL0005StaffList.LC_STAFF_TYPE.DRIVER, work.WF_SEL_CAMPCODE.Text,
                                    work.WF_SEL_TAISHOYM.Text, work.WF_SEL_HORG.Text, work.WF_SEL_STAFFKBN.Text, work.WF_SEL_STAFFCODE.Text)
                    Else
                        prmData = work.CreateStaffCodeParam(GL0005StaffList.LC_STAFF_TYPE.ATTENDANCE_FOR_DRIVER, work.WF_SEL_CAMPCODE.Text,
                                    work.WF_SEL_TAISHOYM.Text, work.WF_SEL_HORG.Text, work.WF_SEL_STAFFKBN.Text, work.WF_SEL_STAFFCODE.Text)
                    End If
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_STAFFCODE, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "WORKINGWEEK"          '営業日曜日
                    prmData.Item(C_PARAMETERS.LP_FIX_CLASS) = "WORKINGWEEK"
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "STATUS"               '状態
                    prmData.Item(C_PARAMETERS.LP_FIX_CLASS) = "APPROVAL"
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "HORG"                 '配属部署
                    prmData = work.CreateHORGParam(work.WF_SEL_CAMPCODE.Text, Master.USERID, Master.ROLE_ORG)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_EXTRA_LIST, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "STAFFKBN"             '社員区分
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_STAFFKBN, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "WORKKBN"              '作業区分
                    prmData.Item(C_PARAMETERS.LP_FIX_CLASS) = "WORKKBN"
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "DELFLG"               '削除
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_DELFLG, I_VALUE, O_TEXT, O_RTN, prmData)
            End Select
        Catch ex As Exception
            O_RTN = C_MESSAGE_NO.FILE_NOT_EXISTS_ERROR
            Exit Sub
        End Try

    End Sub

End Class
