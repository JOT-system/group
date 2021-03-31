Imports System.Drawing
Imports System.Data.SqlClient
Imports OFFICE.GRIS0005LeftBox
Imports BASEDLL

''' <summary>
''' 奉書金登録（実行）
''' </summary>
''' <remarks></remarks>
Public Class GRT00014REWARD
    Inherits Page

    '○ 検索結果格納Table
    Private T00014tbl As DataTable                          '一覧格納用テーブル
    Private T00014INPtbl As DataTable                       'チェック用テーブル

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
                    If Not Master.RecoverTable(T00014tbl, WF_XMLsaveF.Value) OrElse
                        Not Master.RecoverTable(T00014INPtbl, WF_XMLsaveF_INP.Value) Then
                        Exit Sub
                    End If

                    Select Case WF_ButtonClick.Value
                        Case "WF_ButtonExtract"         '絞り込みボタン押下
                            WF_ButtonExtract_Click("PUSH")
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
            If Not IsNothing(T00014tbl) Then
                T00014tbl.Clear()
                T00014tbl.Dispose()
                T00014tbl = Nothing
            End If

            If Not IsNothing(T00014INPtbl) Then
                T00014INPtbl.Clear()
                T00014INPtbl.Dispose()
                T00014INPtbl = Nothing
            End If

        End Try

    End Sub

    ''' <summary>
    ''' 初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub Initialize()

        '○ 画面ID設定
        Master.MAPID = GRT00014WRKINC.MAPID

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

        If Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.T00014S Then          '検索画面からの遷移
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
            WF_TAISHOYM.Text = work.WF_SEL_TAISHOYM.Text

        End If

        '○ ファイルドロップ有無
        Master.eventDrop = True

    End Sub

    ''' <summary>
    ''' GridViewデータ設定
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub GridViewInitialize()

        '○ 画面表示データ取得
        Using SQLcon As SqlConnection = CS0050SESSION.getConnection()
            SQLcon.Open()       'DataBase接続

            MAPDataGet(SQLcon)
        End Using

        '○ 画面表示データ保存
        Master.SaveTable(T00014tbl, WF_XMLsaveF.Value)

        Master.CreateEmptyTable(T00014INPtbl, WF_XMLsaveF.Value)

        '○ 初期画面の乗務員分のデータを格納
        CS0026TBLSORT.TABLE = T00014tbl
        CS0026TBLSORT.SORTING = "LINECNT, HORG, STAFFKBN, STAFFCODE"
        CS0026TBLSORT.FILTER = ""
        CS0026TBLSORT.Sort(T00014INPtbl)
        Master.SaveTable(T00014INPtbl, WF_XMLsaveF_INP.Value)

        '○ 一覧表示データ編集(性能対策)
        Dim TBLview As DataView = New DataView(T00014INPtbl)
        TBLview.RowFilter = "HIDDEN = 0"

        CS0013ProfView.CAMPCODE = work.WF_SEL_CAMPCODE.Text
        CS0013ProfView.PROFID = Master.PROF_VIEW
        CS0013ProfView.MAPID = Master.MAPID
        CS0013ProfView.VARI = Master.VIEWID
        CS0013ProfView.SRCDATA = TBLview.ToTable
        CS0013ProfView.TBLOBJ = pnlListArea
        CS0013ProfView.SCROLLTYPE = CS0013ProfView.SCROLLTYPE_ENUM.Both
        CS0013ProfView.LEVENT = "Onchange"
        CS0013ProfView.LFUNC = "ListChange"
        CS0013ProfView.TITLEOPT = True
        CS0013ProfView.TARGETDATE = work.WF_SEL_TAISHOYM.Text & "/01"
        CS0013ProfView.CS0013ProfView()
        If Not isNormal(CS0013ProfView.ERR) Then
            Master.Output(CS0013ProfView.ERR, C_MESSAGE_TYPE.ABORT, "一覧設定エラー")
            Exit Sub
        End If

        TBLview.Dispose()
        TBLview = Nothing

    End Sub

    ''' <summary>
    ''' 画面表示データ取得
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub MAPDataGet(ByVal SQLcon As SqlConnection)

        '対象年月
        If IsNothing(T00014tbl) Then
            T00014tbl = New DataTable
        End If

        If T00014tbl.Columns.Count <> 0 Then
            T00014tbl.Columns.Clear()
        End If

        T00014tbl.Clear()

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
            '○ 画面表示のメインSQL
            Dim SQLstr As String =
                  " SELECT" _
                & "    0                                                      AS LINECNT" _
                & "    , ''                                                   AS OPERATION" _
                & "    , CAST(ISNULL(T014.UPDTIMSTP, 0) AS bigint)            AS TIMSTP" _
                & "    , 1                                                    AS 'SELECT'" _
                & "    , 0                                                    AS HIDDEN" _
                & "    , ISNULL(RTRIM(MB01.CAMPCODE), '')                     AS CAMPCODE" _
                & "    , ''                                                   AS CAMPNAME" _
                & "    , @P11                                                 AS TAISHOYM" _
                & "    , ISNULL(RTRIM(MB01.STAFFCODE), '')                    AS STAFFCODE" _
                & "    , ''                                                   AS STAFFNAME" _
                & "    , ISNULL(RTRIM(MB01.STAFFKBN), '')                     AS STAFFKBN" _
                & "    , ''                                                   AS STAFFKBNNAME" _
                & "    , ISNULL(RTRIM(MB01.HORG), '')                         AS HORG" _
                & "    , ''                                                   AS HORGNAME" _
                & "    , ISNULL(T014.REWARD1, 0)                              AS REWARD1" _
                & "    , ISNULL(T014.REWARD2, 0)                              AS REWARD2" _
                & "    , ISNULL(T014.REWARD3, 0)                              AS REWARD3" _
                & "    , ISNULL(T014.REWARD4, 0)                              AS REWARD4" _
                & "    , ISNULL(T014.REWARD5, 0)                              AS REWARD5" _
                & "    , ISNULL(RTRIM(T014.DELFLG), '0')                      AS DELFLG" _
                & "    , ''                                                   AS DELFLGNAME" _
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
                & "                AND GRCODE01 in ('車庫','部','事業所')" _
                & "                AND STYMD   <= @P9" _
                & "                AND ENDYMD  >= @P9" _
                & "                AND DELFLG  <> @P10) M006" _
                & "        ON  M006.CODE      = S006.CODE" _
                & "        AND M006.CODE      = MB01.HORG" _
                & "    LEFT JOIN T0014_REWARD T014" _
                & "        ON  T014.CAMPCODE     = MB01.CAMPCODE" _
                & "        AND T014.TAISHOYM     = @P11" _
                & "        AND T014.STAFFCODE    = MB01.STAFFCODE" _
                & "        AND T014.DELFLG      <> @P10" _
                & " WHERE" _
                & "        MB01.CAMPCODE  = @P2" _
                & "    AND MB01.STYMD    <= @P7" _
                & "    AND MB01.ENDYMD   >= @P8" _
                & "    AND MB01.DELFLG   <> @P10"

            '○ 条件指定で指定されたものでSQLで可能なものを追加する
            '配属部署
            If Not String.IsNullOrEmpty(work.WF_SEL_HORG.Text) Then
                SQLstr &= String.Format("    AND MB01.HORG = '{0}'", work.WF_SEL_HORG.Text)
            End If
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
                  " ORDER BY" _
                & "      MB01.CAMPCODE" _
                & "    , MB01.HORG" _
                & "    , MB01.STAFFKBN" _
                & "    , MB01.STAFFCODE"

            '休憩時間取得（G1）
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
            Dim PARA11 As SqlParameter = SQLcmd.Parameters.Add("@P11", SqlDbType.NVarChar)             '対象年月
            PARA1.Value = CS0050SESSION.APSV_ID
            PARA2.Value = work.WF_SEL_CAMPCODE.Text
            PARA3.Value = C_ROLE_VARIANT.SERV_ORG
            PARA4.Value = C_ROLE_VARIANT.USER_ORG
            PARA5.Value = "管轄組織"
            PARA6.Value = work.WF_SEL_HORG.Text
            PARA7.Value = WW_DATE_END
            PARA8.Value = WW_DATE_ST
            PARA9.Value = Date.Now
            PARA10.Value = C_DELETE_FLG.DELETE
            PARA11.Value = work.WF_SEL_TAISHOYM.Text

            Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                '○ フィールド名とフィールドの型を取得
                For index As Integer = 0 To SQLdr.FieldCount - 1
                    T00014tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                Next

                '○ テーブル検索結果をテーブル格納
                T00014tbl.Load(SQLdr)
            End Using

            Dim WW_LINECNT As Integer = 0
            For Each T00014row As DataRow In T00014tbl.Rows
                '固定項目
                WW_LINECNT = WW_LINECNT + 1
                T00014row("LINECNT") = WW_LINECNT
                T00014row("SELECT") = 1
                T00014row("HIDDEN") = 0

                '名称取得
                CODENAME_get("CAMPCODE", T00014row("CAMPCODE"), T00014row("CAMPNAME"), WW_DUMMY)                       '会社コード
                CODENAME_get("STAFFCODE", T00014row("STAFFCODE"), T00014row("STAFFNAME"), WW_DUMMY)                    '従業員コード
                CODENAME_get("STAFFKBN", T00014row("STAFFKBN"), T00014row("STAFFKBNNAME"), WW_DUMMY)                   '職務区分
                CODENAME_get("HORG", T00014row("HORG"), T00014row("HORGNAME"), WW_DUMMY)                               '配属部署
                CODENAME_get("DELFLG", T00014row("DELFLG"), T00014row("DELFLGNAME"), WW_DUMMY)                         '削除フラグ
            Next
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "T0014_REWARD SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:T0014_REWARD Select"
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

        '絞込条件を生かす
        WF_ButtonExtract_Click()

        '○ ヘッダ編集
        For Each T00014INProw As DataRow In T00014INPtbl.Rows
            WF_TAISHOYM.Text = CDate(T00014INProw("TAISHOYM") & "/01").ToString("yyyy/MM")
            Exit For
        Next

        '○ 一覧表示データ編集(性能対策)
        Dim TBLview As DataView = New DataView(T00014INPtbl)

        CS0013ProfView.CAMPCODE = work.WF_SEL_CAMPCODE.Text
        CS0013ProfView.PROFID = Master.PROF_VIEW
        CS0013ProfView.MAPID = Master.MAPID
        CS0013ProfView.VARI = Master.VIEWID
        CS0013ProfView.SRCDATA = TBLview.ToTable
        CS0013ProfView.TBLOBJ = pnlListArea
        CS0013ProfView.SCROLLTYPE = CS0013ProfView.SCROLLTYPE_ENUM.Both
        CS0013ProfView.LEVENT = "Onchange"
        CS0013ProfView.LFUNC = "ListChange"
        CS0013ProfView.TITLEOPT = True
        CS0013ProfView.TARGETDATE = work.WF_SEL_TAISHOYM.Text & "/01"
        CS0013ProfView.CS0013ProfView()
        If Not isNormal(CS0013ProfView.ERR) Then
            Master.Output(CS0013ProfView.ERR, C_MESSAGE_TYPE.ABORT, "一覧設定エラー")
            Exit Sub
        End If

        TBLview.Dispose()
        TBLview = Nothing

    End Sub

    ''' <summary>
    ''' 絞り込みボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonExtract_Click(Optional ByVal iBtn As String = Nothing)

        If IsNothing(iBtn) Then
            Exit Sub
        End If

        '○ 使用禁止文字排除
        Master.EraseCharToIgnore(WF_SELHORG.Text)
        Master.EraseCharToIgnore(WF_SELSTAFFKBN.Text)
        Master.EraseCharToIgnore(WF_SELSTAFFCODE.Text)
        Master.EraseCharToIgnore(WF_SELSTAFFNAME.Text)

        '○ 名称取得
        CODENAME_get("HORG", WF_SELHORG.Text, WF_SELHORG_TEXT.Text, WW_RTN_SW)
        If Not isNormal(WW_RTN_SW) Then
            Master.Output(C_MESSAGE_NO.MASTER_NOT_FOUND_ERROR, C_MESSAGE_TYPE.ERR, "配属部署 : " & WF_SELHORG.Text)
            Exit Sub
        End If
        CODENAME_get("STAFFKBN", WF_SELSTAFFKBN.Text, WF_SELSTAFFKBN_TEXT.Text, WW_RTN_SW)
        If Not isNormal(WW_RTN_SW) Then
            Master.Output(C_MESSAGE_NO.MASTER_NOT_FOUND_ERROR, C_MESSAGE_TYPE.ERR, "職務区分 : " & WF_SELSTAFFKBN.Text)
            Exit Sub
        End If
        CODENAME_get("STAFFCODE", WF_SELSTAFFCODE.Text, WF_SELSTAFFCODE_TEXT.Text, WW_RTN_SW)
        If Not isNormal(WW_RTN_SW) Then
            Master.Output(C_MESSAGE_NO.MASTER_NOT_FOUND_ERROR, C_MESSAGE_TYPE.ERR, "従業員 : " & WF_SELSTAFFCODE.Text)
            Exit Sub
        End If

        '○ 全体データにT00014INPtbl(個人)を反映(削除してマージ)
        CS0026TBLSORT.TABLE = T00014tbl
        CS0026TBLSORT.SORTING = "LINECNT, HORG, STAFFKBN, STAFFCODE"
        CS0026TBLSORT.FILTER = ""
        CS0026TBLSORT.Sort(T00014tbl)

        '○ 画面表示変更
        CS0026TBLSORT.TABLE = T00014tbl
        CS0026TBLSORT.SORTING = "LINECNT, HORG, STAFFKBN, STAFFCODE"
        CS0026TBLSORT.FILTER = String.Format("HORG like '{0}*' and STAFFKBN like '{1}*' and STAFFCODE like '{2}*' and STAFFNAME like '*{3}*'",
                                             WF_SELHORG.Text, WF_SELSTAFFKBN.Text, WF_SELSTAFFCODE.Text, WF_SELSTAFFNAME.Text)
        CS0026TBLSORT.Sort(T00014INPtbl)

        '○ テーブル保存
        Master.SaveTable(T00014tbl, WF_XMLsaveF.Value)
        Master.SaveTable(T00014INPtbl, WF_XMLsaveF_INP.Value)

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
        For Each T00014row As DataRow In T00014tbl.Rows
            If T00014row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED Then
                WW_CheckMES1 = "エラーデータが存在します。"
                WW_CheckMES2 = ""
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00014row)
                WW_ERR = True
            End If
        Next

        If WW_ERR Then
            Master.Output(C_MESSAGE_NO.BOX_ERROR_EXIST, C_MESSAGE_TYPE.ERR)
            Exit Sub
        End If

        '○ 褒賞金テーブル更新用のテーブル作成
        Dim WW_T00014tbl As DataTable = New DataTable
        Dim WW_NOW As DateTime = Date.Now

        CS0026TBLSORT.TABLE = T00014tbl
        CS0026TBLSORT.SORTING = "CAMPCODE, HORG, STAFFKBN, STAFFCODE"
        CS0026TBLSORT.FILTER = "OPERATION = '" & C_LIST_OPERATION_CODE.UPDATING & "' and SELECT = '1'"
        CS0026TBLSORT.Sort(T00014tbl)

        '○ 褒賞金テーブル出力編集
        AddColumnT0014UPDtbl(WW_T00014tbl)
        UpdTableEdit(T00014tbl, WW_T00014tbl, WW_NOW)

        '○ 褒賞金テーブル更新
        Using SQLcon As SqlConnection = CS0050SESSION.getConnection()
            SQLcon.Open() 'DataBase接続(Open)
            For Each T14row As DataRow In WW_T00014tbl.Rows
                Try
                    '褒賞金テーブルＤＢ更新
                    Dim SQLStr As String =
                          " DECLARE @hensuu as bigint ; " _
                        & " set @hensuu = 0 ; " _
                        & " DECLARE hensuu CURSOR FOR " _
                        & "   SELECT CAST(UPDTIMSTP as bigint) as hensuu  " _
                        & "     FROM T0014_REWARD " _
                        & " WHERE CAMPCODE     = @P01 " _
                        & "  and TAISHOYM      = @P02 " _
                        & "  and STAFFCODE     = @P03 " _
                        & " OPEN hensuu ; " _
                        & " FETCH NEXT FROM hensuu INTO @hensuu ; " _
                        & " IF ( @@FETCH_STATUS = 0 ) " _
                        & "    UPDATE T0014_REWARD " _
                        & "    SET REWARD1     = @P04 " _
                        & "      , REWARD2     = @P05 " _
                        & "      , REWARD3     = @P06 " _
                        & "      , REWARD4     = @P07 " _
                        & "      , REWARD5     = @P08 " _
                        & "      , DELFLG      = @P09 " _
                        & "      , UPDYMD      = @P11 " _
                        & "      , UPDUSER     = @P12 " _
                        & "      , UPDTERMID   = @P13 " _
                        & "      , RECEIVEYMD  = @P14  " _
                        & " WHERE CAMPCODE     = @P01 " _
                        & "  and TAISHOYM      = @P02 " _
                        & "  and STAFFCODE     = @P03 " _
                        & " IF ( @@FETCH_STATUS <> 0 ) " _
                        & "    INSERT INTO T0014_REWARD " _
                        & "             (CAMPCODE , " _
                        & "              TAISHOYM , " _
                        & "              STAFFCODE , " _
                        & "              REWARD1 , " _
                        & "              REWARD2 , " _
                        & "              REWARD3 , " _
                        & "              REWARD4 , " _
                        & "              REWARD5 , " _
                        & "              DELFLG , " _
                        & "              INITYMD , " _
                        & "              UPDYMD , " _
                        & "              UPDUSER ,  " _
                        & "              UPDTERMID , " _
                        & "              RECEIVEYMD ) " _
                        & "      VALUES (@P01,@P02,@P03,@P04,@P05,@P06,@P07,@P08,@P09,@P10, " _
                        & "              @P11,@P12,@P13,@P14); " _
                        & " CLOSE hensuu ; " _
                        & " DEALLOCATE hensuu ; "

                    Using SQLcmd As SqlCommand = New SqlCommand(SQLStr, SQLcon, SQLtrn)
                        Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", System.Data.SqlDbType.NVarChar)
                        Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", System.Data.SqlDbType.NVarChar)
                        Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@P03", System.Data.SqlDbType.NVarChar)
                        Dim PARA04 As SqlParameter = SQLcmd.Parameters.Add("@P04", System.Data.SqlDbType.Int)
                        Dim PARA05 As SqlParameter = SQLcmd.Parameters.Add("@P05", System.Data.SqlDbType.Int)
                        Dim PARA06 As SqlParameter = SQLcmd.Parameters.Add("@P06", System.Data.SqlDbType.Int)
                        Dim PARA07 As SqlParameter = SQLcmd.Parameters.Add("@P07", System.Data.SqlDbType.Int)
                        Dim PARA08 As SqlParameter = SQLcmd.Parameters.Add("@P08", System.Data.SqlDbType.Int)
                        Dim PARA09 As SqlParameter = SQLcmd.Parameters.Add("@P09", System.Data.SqlDbType.NVarChar)
                        Dim PARA10 As SqlParameter = SQLcmd.Parameters.Add("@P10", System.Data.SqlDbType.NVarChar)
                        Dim PARA11 As SqlParameter = SQLcmd.Parameters.Add("@P11", System.Data.SqlDbType.NVarChar)
                        Dim PARA12 As SqlParameter = SQLcmd.Parameters.Add("@P12", System.Data.SqlDbType.NVarChar)
                        Dim PARA13 As SqlParameter = SQLcmd.Parameters.Add("@P13", System.Data.SqlDbType.NVarChar)
                        Dim PARA14 As SqlParameter = SQLcmd.Parameters.Add("@P14", System.Data.SqlDbType.NVarChar)

                        PARA01.Value = T14row("CAMPCODE")
                        PARA02.Value = T14row("TAISHOYM")
                        PARA03.Value = T14row("STAFFCODE")
                        PARA04.Value = T14row("REWARD1")
                        PARA05.Value = T14row("REWARD2")
                        PARA06.Value = T14row("REWARD3")
                        PARA07.Value = T14row("REWARD4")
                        PARA08.Value = T14row("REWARD5")

                        PARA09.Value = T14row("DELFLG")
                        PARA10.Value = T14row("INITYMD")
                        PARA11.Value = T14row("UPDYMD")
                        PARA12.Value = Master.USERID
                        PARA13.Value = Master.USERTERMID
                        PARA14.Value = C_DEFAULT_YMD

                        SQLcmd.CommandTimeout = 300
                        SQLcmd.ExecuteNonQuery()

                        'CLOSE
                    End Using

                Catch ex As Exception
                    CS0011LOGWrite.INFSUBCLASS = "T0014_Update"                 'SUBクラス名
                    CS0011LOGWrite.INFPOSI = "DB:UPDATE T0014_REWARD"       '
                    CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                    CS0011LOGWrite.TEXT = ex.ToString()
                    CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                    CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
                    Exit Sub

                End Try
            Next
        End Using

        '○ テーブル初期化
        If Not IsNothing(WW_T00014tbl) Then
            WW_T00014tbl.Clear()
            WW_T00014tbl.Dispose()
            WW_T00014tbl = Nothing
        End If

        '○ 画面表示データ再取得
        Using SQLcon As SqlConnection = CS0050SESSION.getConnection()
            SQLcon.Open()       'DataBase接続

            MAPDataGet(SQLcon)
        End Using

        '○ 画面表示データ保存
        Master.SaveTable(T00014tbl, WF_XMLsaveF.Value)

        '○ 現在表示している乗務員分のデータを格納
        CS0026TBLSORT.TABLE = T00014tbl
        CS0026TBLSORT.SORTING = "LINECNT, HORG, STAFFKBN, STAFFCODE"
        CS0026TBLSORT.FILTER = String.Format("HORG like '{0}*' and STAFFKBN like '{1}*' and STAFFCODE like '{2}*' and STAFFNAME like '*{3}*'",
                                             WF_SELHORG.Text, WF_SELSTAFFKBN.Text, WF_SELSTAFFCODE.Text, WF_SELSTAFFNAME.Text)
        CS0026TBLSORT.Sort(T00014INPtbl)
        Master.SaveTable(T00014INPtbl, WF_XMLsaveF_INP.Value)

    End Sub

    ''' <summary>
    ''' 褒賞金テーブルカラム定義
    ''' </summary>
    ''' <param name="IO_TBL"></param>
    ''' <remarks></remarks>
    Public Sub AddColumnT0014UPDtbl(ByRef IO_TBL As DataTable)

        If IsNothing(IO_TBL) Then IO_TBL = New DataTable
        If IO_TBL.Columns.Count <> 0 Then IO_TBL.Columns.Clear()
        'T0013DB項目作成
        IO_TBL.Clear()
        IO_TBL.Columns.Add("CAMPCODE", GetType(String))
        IO_TBL.Columns.Add("TAISHOYM", GetType(String))
        IO_TBL.Columns.Add("STAFFCODE", GetType(String))
        IO_TBL.Columns.Add("REWARD1", GetType(Integer))
        IO_TBL.Columns.Add("REWARD2", GetType(Integer))
        IO_TBL.Columns.Add("REWARD3", GetType(Integer))
        IO_TBL.Columns.Add("REWARD4", GetType(Integer))
        IO_TBL.Columns.Add("REWARD5", GetType(Integer))
        IO_TBL.Columns.Add("DELFLG", GetType(String))
        IO_TBL.Columns.Add("INITYMD", GetType(String))
        IO_TBL.Columns.Add("UPDYMD", GetType(String))
        IO_TBL.Columns.Add("UPDUSER", GetType(String))
        IO_TBL.Columns.Add("UPDTERMID", GetType(String))
        IO_TBL.Columns.Add("RECEIVEYMD", GetType(String))

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
            O_ROW("REWARD1") = I_ROW("REWARD1")                                                 '褒賞金１
            O_ROW("REWARD2") = 0                                                                '褒賞金２
            O_ROW("REWARD3") = 0                                                                '褒賞金３
            O_ROW("REWARD4") = 0                                                                '褒賞金４
            O_ROW("REWARD5") = 0                                                                '褒賞金５

            O_ROW("DELFLG") = C_DELETE_FLG.ALIVE                                                '削除フラグ
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

        Dim TBLview As DataView = New DataView(T00014tbl)
        TBLview.Sort = "LINECNT, HORG, STAFFKBN, STAFFCODE"

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
                Master.Output(CS0030REPORT.ERR, C_MESSAGE_TYPE.ERR)
            Else
                Master.Output(CS0030REPORT.ERR, C_MESSAGE_TYPE.ABORT, "CS0030REPORT")
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

        Dim WW_ERR As String = C_MESSAGE_NO.NORMAL

        '○ エラーレポート準備
        rightview.SetErrorReport("")

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
            Master.Output(CS0023XLSUPLOAD.ERR, C_MESSAGE_TYPE.ABORT, "CS0023XLSUPLOAD")
            Exit Sub
        End If

        '○ インポートファイルの列情報有り無し判定
        Master.CreateEmptyTable(T00014INPtbl, WF_XMLsaveF.Value)
        ExcelInpMake(CS0023XLSUPLOAD.TBLDATA)

        '○ INPデータチェック
        For Each T00014INProw As DataRow In T00014INPtbl.Rows
            INPTableCheck(T00014INProw, WW_ERR_SW)

            If isNormal(WW_ERR_SW) Then
                T00014INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                T00014INProw("SELECT") = 1
            Else
                T00014INProw("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                T00014INProw("SELECT") = 0
                If WW_ERR_SW > WW_ERR Then
                    WW_ERR = WW_ERR_SW
                End If
            End If
        Next

        '○ 重大エラーの場合、インポートデータから削除
        For i As Integer = T00014INPtbl.Rows.Count - 1 To 0 Step -1
            If T00014INPtbl.Rows(i)("SELECT") = 0 Then
                T00014INPtbl.Rows(i).Delete()
            End If
        Next

        '○ 画面表示の従業員のみ抽出
        Dim WW_COLs As String() = {"STAFFCODE"}
        Dim WW_KEYtbl As DataTable = New DataTable
        Dim TBLview As DataView = New DataView(T00014tbl)
        WW_KEYtbl = TBLview.ToTable(True, WW_COLs)

        Dim WW_FIND As Boolean = False
        For i As Integer = T00014INPtbl.Rows.Count - 1 To 0 Step -1
            WW_FIND = False
            For Each WW_KEYrow As DataRow In WW_KEYtbl.Rows
                If WW_KEYrow("STAFFCODE") = T00014INPtbl.Rows(i)("STAFFCODE") Then
                    WW_FIND = True
                    Exit For
                End If
            Next

            If Not WW_FIND Then
                Dim WW_CheckMES1 As String = "・更新できないレコード(従業員エラー)です。"
                Dim WW_CheckMES2 As String = "画面選択されていない従業員です。"
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00014INPtbl.Rows(i))
                WW_ERR_SW = C_MESSAGE_NO.BOX_ERROR_EXIST
                T00014INPtbl.Rows(i).Delete()
                If WW_ERR_SW > WW_ERR Then
                    WW_ERR = WW_ERR_SW
                End If
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
        CS0026TBLSORT.TABLE = T00014INPtbl
        CS0026TBLSORT.SORTING = "TAISHOYM, HORG, STAFFCODE"
        CS0026TBLSORT.FILTER = ""
        CS0026TBLSORT.Sort(T00014INPtbl)

        CS0026TBLSORT.TABLE = T00014tbl
        CS0026TBLSORT.SORTING = "TAISHOYM, HORG, STAFFCODE"
        CS0026TBLSORT.FILTER = ""
        CS0026TBLSORT.Sort(T00014tbl)

        Dim WW_INDEX As Integer = 0
        Dim WW_KEY_INP As String = ""
        Dim WW_KEY_TBL As String = ""

        For Each T00014INProw As DataRow In T00014INPtbl.Rows
            WW_KEY_INP = T00014INProw("TAISHOYM") & T00014INProw("HORG") & T00014INProw("STAFFCODE")

            If T00014INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING Then
                For i As Integer = WW_INDEX To T00014tbl.Rows.Count - 1
                    Dim T00014row As DataRow = T00014tbl.Rows(i)
                    WW_KEY_TBL = T00014row("TAISHOYM") & T00014row("HORG") & T00014row("STAFFCODE")

                    If WW_KEY_TBL < WW_KEY_INP Then
                        Continue For
                    End If

                    If WW_KEY_TBL = WW_KEY_INP Then
                        T00014row("OPERATION") = T00014INProw("OPERATION")
                        T00014row("SELECT") = 0
                        T00014row("HIDDEN") = 1
                        T00014row("DELFLG") = C_DELETE_FLG.DELETE
                        T00014row("TIMSTP") = 0
                        'LINECNTの引継ぎ
                        T00014INProw("LINECNT") = T00014row("LINECNT")
                    End If

                    If WW_KEY_TBL > WW_KEY_INP Then
                        WW_INDEX = i
                        Exit For
                    End If
                Next
            End If
        Next

        '○ 当画面で生成したデータ(タイムスタンプ = 0)に対する変更は、変更前を削除する
        For i As Integer = T00014tbl.Rows.Count - 1 To 0 Step -1
            If T00014tbl.Rows(i)("TIMSTP") = 0 AndAlso
                T00014tbl.Rows(i)("SELECT") = 0 Then
                T00014tbl.Rows(i).Delete()
            End If
        Next

        T00014tbl.Merge(T00014INPtbl)

        '○ 画面表示データ保存
        Master.SaveTable(T00014tbl, WF_XMLsaveF.Value)

        '○ 現在表示している乗務員分のデータを格納
        CS0026TBLSORT.TABLE = T00014tbl
        CS0026TBLSORT.SORTING = "LINECNT, HORG, STAFFKBN, STAFFCODE"
        CS0026TBLSORT.FILTER = String.Format("HORG like '{0}*' and STAFFKBN like '{1}*' and STAFFCODE like '{2}*' and STAFFNAME like '*{3}*'",
                                             WF_SELHORG.Text, WF_SELSTAFFKBN.Text, WF_SELSTAFFCODE.Text, WF_SELSTAFFNAME.Text)
        CS0026TBLSORT.Sort(T00014INPtbl)
        Master.SaveTable(T00014INPtbl, WF_XMLsaveF_INP.Value)

        If isNormal(WW_ERR) Then
            Master.Output(C_MESSAGE_NO.NORMAL, C_MESSAGE_TYPE.NOR)
        Else
            Master.Output(WW_ERR, C_MESSAGE_TYPE.ERR)
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
            Dim T00014INProw As DataRow = T00014INPtbl.NewRow

            '初期クリア
            For Each T00014INPcol As DataColumn In T00014INPtbl.Columns
                If IsDBNull(T00014INProw.Item(T00014INPcol)) OrElse IsNothing(T00014INProw.Item(T00014INPcol)) Then
                    Select Case T00014INPcol.ColumnName
                        Case "LINECNT"
                            T00014INProw.Item(T00014INPcol) = 0
                        Case "OPERATION"
                            T00014INProw.Item(T00014INPcol) = C_LIST_OPERATION_CODE.NODATA
                        Case "TIMSTP"
                            T00014INProw.Item(T00014INPcol) = 0
                        Case "SELECT"
                            T00014INProw.Item(T00014INPcol) = 1
                        Case "HIDDEN"
                            T00014INProw.Item(T00014INPcol) = 0
                        Case "SEQ"
                            T00014INProw.Item(T00014INPcol) = 0
                        Case Else
                            Select Case T00014INPcol.DataType.Name
                                Case "String"
                                    T00014INProw.Item(T00014INPcol) = ""
                                Case "Datetime"
                                    T00014INProw.Item(T00014INPcol) = "1950/01/01"
                                Case "int32"
                                    T00014INProw.Item(T00014INPcol) = 0
                            End Select
                    End Select
                End If
            Next

            '共通項目
            T00014INProw("LINECNT") = 0
            T00014INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
            T00014INProw("TIMSTP") = 0
            T00014INProw("SELECT") = 1
            T00014INProw("HIDDEN") = 0

            '○ 項目セット
            '会社コード
            If WW_COLUMNS.IndexOf("CAMPCODE") >= 0 Then
                T00014INProw("CAMPCODE") = TBLrow("CAMPCODE")
            Else
                T00014INProw("CAMPCODE") = work.WF_SEL_CAMPCODE.Text
            End If

            '対象年月
            If WW_COLUMNS.IndexOf("TAISHOYM") >= 0 Then
                T00014INProw("TAISHOYM") = TBLrow("TAISHOYM")
            Else
                T00014INProw("TAISHOYM") = ""
            End If

            '従業員コード
            If WW_COLUMNS.IndexOf("STAFFCODE") >= 0 Then
                T00014INProw("STAFFCODE") = TBLrow("STAFFCODE")
            Else
                T00014INProw("STAFFCODE") = ""
            End If

            '作業区分
            If WW_COLUMNS.IndexOf("STAFFKBN") >= 0 Then
                T00014INProw("STAFFKBN") = TBLrow("STAFFKBN")
            Else
                T00014INProw("STAFFKBN") = ""
            End If

            '配属部署
            If WW_COLUMNS.IndexOf("HORG") >= 0 Then
                T00014INProw("HORG") = TBLrow("HORG")
            Else
                T00014INProw("HORG") = ""
            End If

            '褒賞金１～５
            For i As Integer = 1 To 5
                Dim WW_REWARD As String = "REWARD" & i.ToString("0")
                If WW_COLUMNS.IndexOf(WW_REWARD) >= 0 Then
                    If IsNumeric(TBLrow(WW_REWARD)) Then
                        T00014INProw(WW_REWARD) = TBLrow(WW_REWARD)
                    Else
                        T00014INProw(WW_REWARD) = 0
                    End If
                Else
                    T00014INProw(WW_REWARD) = 0
                End If
            Next
            '削除フラグ
            If WW_COLUMNS.IndexOf("DELFLG") >= 0 Then
                T00014INProw("DELFLG") = TBLrow("DELFLG")
            Else
                T00014INProw("DELFLG") = C_DELETE_FLG.ALIVE
            End If

            T00014INPtbl.Rows.Add(T00014INProw)
        Next

    End Sub


    ''' <summary>
    ''' リスト変更時処理
    ''' </summary>
    ''' <param name="I_CHANGED"></param>
    ''' <remarks></remarks>
    Protected Sub WF_ListChange(Optional ByVal I_CHANGED As String = "")

        rightview.SetErrorReport("")
        Dim WW_LINECNT As Integer = 0

        '○ 画面項目チェック
        For Each T00014INProw As DataRow In T00014INPtbl.Rows
            If Val(T00014INProw("LINECNT")) = Val(WF_SelectedIndex.Value) Then
                '変更内容取得(入力禁止文字除外)
                If Not IsNothing(Request.Form("txt" & pnlListArea.ID & "REWARD1" & WF_SelectedIndex.Value)) Then
                    If T00014INProw("REWARD1").ToString <> Convert.ToString(Request.Form("txt" & pnlListArea.ID & "REWARD1" & WF_SelectedIndex.Value)) Then
                        T00014INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                    End If
                    T00014INProw("REWARD1") = Val(Convert.ToString(Request.Form("txt" & pnlListArea.ID & "REWARD1" & WF_SelectedIndex.Value)))
                End If
                Master.EraseCharToIgnore(T00014INProw("REWARD1"))

                '項目チェック
                INPTableCheck(T00014INProw, WW_ERR_SW)
                If Not isNormal(WW_ERR_SW) Then
                    T00014INProw("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                    Master.Output(WW_ERR_SW, C_MESSAGE_TYPE.ABORT)
                End If
                Exit For
            End If
        Next

        '全体データにINPtblに反映(削除してマージ)
        CS0026TBLSORT.TABLE = T00014tbl
        CS0026TBLSORT.SORTING = "LINECNT, HORG, STAFFKBN, STAFFCODE"
        CS0026TBLSORT.FILTER = String.Format("LINECNT <> {0}", Val(WF_SelectedIndex.Value))
        CS0026TBLSORT.Sort(T00014tbl)

        Dim WW_T00014tbl As DataTable = T00014INPtbl.Clone
        CS0026TBLSORT.TABLE = T00014INPtbl
        CS0026TBLSORT.SORTING = "LINECNT, HORG, STAFFKBN, STAFFCODE"
        CS0026TBLSORT.FILTER = String.Format("LINECNT = {0}", Val(WF_SelectedIndex.Value))
        CS0026TBLSORT.Sort(WW_T00014tbl)

        T00014tbl.Merge(WW_T00014tbl)

        '○ 画面表示データ保存
        Master.SaveTable(T00014tbl, WF_XMLsaveF.Value)
        Master.SaveTable(T00014INPtbl, WF_XMLsaveF_INP.Value)

        WW_T00014tbl.Clear()
        WW_T00014tbl.Dispose()

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
                        prmData = work.CreateStaffCodeParam(GL0005StaffList.LC_STAFF_TYPE.EMPLOYEE_IN_ORG, work.WF_SEL_CAMPCODE.Text,
                                work.WF_SEL_TAISHOYM.Text, WF_SELHORG.Text, WF_SELSTAFFKBN.Text, "")
                    Case "WF_SELHORG"                   '所属部署
                        prmData = work.CreateHORGParam(work.WF_SEL_CAMPCODE.Text, Master.USERID, Master.ROLE_ORG)
                    Case "WF_SELSTAFFKBN"                     '職務区分
                        prmData = work.CreateStaffKBNParam(work.WF_SEL_CAMPCODE.Text)
                End Select

                .SetListBox(WF_LeftMViewChange.Value, WW_DUMMY, prmData)
                .ActiveListBox()
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
        If Not IsNothing(leftview.GetActiveValue) Then
            WW_SelectValue = leftview.GetActiveValue(0)
            WW_SelectText = leftview.GetActiveValue(1)
        End If

        '○ 選択内容を画面項目へセット
        If WF_FIELD.Value = "WF_SELHORG" Then
            '配属部署(絞込条件)
            WF_SELHORG.Text = WW_SelectValue
            WF_SELHORG_TEXT.Text = WW_SelectText
            WF_SELHORG.Focus()
        End If
        If WF_FIELD.Value = "WF_SELSTAFFCODE" Then
            '従業員コード(絞込条件)
            WF_SELSTAFFCODE.Text = WW_SelectValue
            WF_SELSTAFFCODE_TEXT.Text = WW_SelectText
            WF_SELSTAFFCODE.Focus()
        End If
        If WF_FIELD.Value = "WF_SELSTAFFKBN" Then
            '職務区分(絞込条件)
            WF_SELSTAFFKBN.Text = WW_SelectValue
            WF_SELSTAFFKBN_TEXT.Text = WW_SelectText
            WF_SELSTAFFKBN.Focus()
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

            rightview.SelectIndex(WF_RightViewChange.Value)
            WF_RightViewChange.Value = ""
        End If

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
    ''' 入力値チェック
    ''' </summary>
    ''' <param name="T00014INProw"></param>
    ''' <param name="O_RTN"></param>
    ''' <remarks></remarks>
    Protected Sub INPTableCheck(ByRef T00014INProw As DataRow, ByRef O_RTN As String)

        O_RTN = C_MESSAGE_NO.NORMAL

        Dim WW_CheckMES1 As String = ""
        Dim WW_CheckMES2 As String = ""
        Dim WW_CS0036FCHECKERR As String = ""
        Dim WW_CS0036FCHECKREPORT As String = ""
        Dim WW_S0013tbl As DataTable = New DataTable

        '○ 単項目チェック
        '会社コード
        Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "CAMPCODE", T00014INProw("CAMPCODE"), WW_CS0036FCHECKERR, WW_CS0036FCHECKREPORT, WW_S0013tbl)
        If isNormal(WW_CS0036FCHECKERR) Then
            '存在チェック
            CODENAME_get("CAMPCODE", T00014INProw("CAMPCODE"), T00014INProw("CAMPNAME"), WW_RTN_SW)
            If Not isNormal(WW_RTN_SW) Then
                WW_CheckMES1 = "・更新できないレコード(会社コードエラー)です。"
                WW_CheckMES2 = "マスタに存在しません。(" & T00014INProw("CAMPCODE") & ")"
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00014INProw)
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
        Else
            WW_CheckMES1 = "・更新できないレコード(会社コードエラー)です。"
            WW_CheckMES2 = WW_CS0036FCHECKREPORT
            WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00014INProw)
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If

        '対象年月
        If IsDate(T00014INProw("TAISHOYM")) Then
            If CDate(T00014INProw("TAISHOYM")).ToString("yyyy/MM") = WF_TAISHOYM.Text Then
            Else
                WW_CheckMES1 = "・更新できないレコード(対象年月エラー)です。"
                WW_CheckMES2 = T00014INProw("TAISHOYM")
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00014INProw)
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
        Else
            WW_CheckMES1 = "・更新できないレコード(対象年月不正)です。"
            WW_CheckMES2 = T00014INProw("TAISHOYM")
            WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00014INProw)
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If

        '配属部署
        Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "HORG", T00014INProw("HORG"), WW_CS0036FCHECKERR, WW_CS0036FCHECKREPORT, WW_S0013tbl)
        If isNormal(WW_CS0036FCHECKERR) Then
            '存在チェック
            CODENAME_get("HORG", T00014INProw("HORG"), T00014INProw("HORGNAME"), WW_RTN_SW)
            If Not isNormal(WW_RTN_SW) Then
                WW_CheckMES1 = "・更新できないレコード(配属部署エラー)です。"
                WW_CheckMES2 = "マスタに存在しません。(" & T00014INProw("HORG") & ")"
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00014INProw)
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
        Else
            WW_CheckMES1 = "・更新できないレコード(配属部署エラー)です。"
            WW_CheckMES2 = WW_CS0036FCHECKREPORT
            WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00014INProw)
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If

        '職務区分
        Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "STAFFKBN", T00014INProw("STAFFKBN"), WW_CS0036FCHECKERR, WW_CS0036FCHECKREPORT, WW_S0013tbl)
        If isNormal(WW_CS0036FCHECKERR) Then
            '存在チェック
            CODENAME_get("STAFFKBN", T00014INProw("STAFFKBN"), T00014INProw("STAFFKBNNAME"), WW_RTN_SW)
            If Not isNormal(WW_RTN_SW) Then
                WW_CheckMES1 = "・更新できないレコード(職務区分エラー)です。"
                WW_CheckMES2 = "マスタに存在しません。(" & T00014INProw("STAFFKBN") & ")"
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00014INProw)
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
        Else
            WW_CheckMES1 = "・更新できないレコード(職務区分エラー)です。"
            WW_CheckMES2 = WW_CS0036FCHECKREPORT
            WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00014INProw)
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If

        '従業員コード
        Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "STAFFCODE", T00014INProw("STAFFCODE"), WW_CS0036FCHECKERR, WW_CS0036FCHECKREPORT, WW_S0013tbl)
        If isNormal(WW_CS0036FCHECKERR) Then
            '存在チェック
            CODENAME_get("STAFFCODE", T00014INProw("STAFFCODE"), T00014INProw("STAFFNAME"), WW_RTN_SW)
            If Not isNormal(WW_RTN_SW) Then
                WW_CheckMES1 = "・更新できないレコード(従業員コードエラー)です。"
                WW_CheckMES2 = "マスタに存在しません。(" & T00014INProw("STAFFCODE") & ")"
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00014INProw)
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
        Else
            WW_CheckMES1 = "・更新できないレコード(従業員コードエラー)です。"
            WW_CheckMES2 = WW_CS0036FCHECKREPORT
            WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00014INProw)
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If

        For i As Integer = 1 To 5
            Dim WW_REWARD As String = "REWARD" & i.ToString("0")

            '褒賞金
            If String.IsNullOrEmpty(T00014INProw(WW_REWARD)) Then
                T00014INProw(WW_REWARD) = "0"
            Else
                Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "REWARD", T00014INProw(WW_REWARD), WW_CS0036FCHECKERR, WW_CS0036FCHECKREPORT, WW_S0013tbl)
                If isNormal(WW_CS0036FCHECKERR) Then
                Else
                    WW_CheckMES1 = "・更新できないレコード(褒賞金" & i.ToString("0") & "エラー)です。"
                    WW_CheckMES2 = WW_CS0036FCHECKREPORT
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00014INProw)
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If
        Next
    End Sub

    ''' <summary>
    ''' エラーレポート編集
    ''' </summary>
    ''' <param name="MESSAGE1"></param>
    ''' <param name="MESSAGE2"></param>
    ''' <param name="T00014row"></param>
    ''' <remarks></remarks>
    Protected Sub WW_CheckERR(ByVal MESSAGE1 As String, ByVal MESSAGE2 As String, Optional ByVal T00014row As DataRow = Nothing)

        Dim WW_ERR_MES As String = ""
        WW_ERR_MES = MESSAGE1
        If MESSAGE2 <> "" Then
            WW_ERR_MES &= ControlChars.NewLine & "  --> " & MESSAGE2 & " , "
        End If

        If Not IsNothing(T00014row) Then
            WW_ERR_MES &= ControlChars.NewLine & "  --> 会社     =" & T00014row("CAMPCODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 従業員   =" & T00014row("STAFFCODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 従業員名 =" & T00014row("STAFFNAME") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 配属部署 =" & T00014row("HORG") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 部署名   =" & T00014row("HORGNAME") & " , "
        End If

        rightview.AddErrorReport(WW_ERR_MES)

    End Sub


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
                    prmData = work.CreateStaffCodeParam(GL0005StaffList.LC_STAFF_TYPE.EMPLOYEE_IN_ORG, work.WF_SEL_CAMPCODE.Text,
                                work.WF_SEL_TAISHOYM.Text, "", work.WF_SEL_STAFFKBN.Text, work.WF_SEL_STAFFCODE.Text)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_STAFFCODE, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "HORG"                 '配属部署
                    prmData = work.CreateHORGParam(work.WF_SEL_CAMPCODE.Text, Master.USERID, Master.ROLE_ORG)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_EXTRA_LIST, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "STAFFKBN"             '社員区分
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_STAFFKBN, I_VALUE, O_TEXT, O_RTN, prmData)
            End Select
        Catch ex As Exception
            O_RTN = C_MESSAGE_NO.FILE_NOT_EXISTS_ERROR
            Exit Sub
        End Try

    End Sub

End Class
