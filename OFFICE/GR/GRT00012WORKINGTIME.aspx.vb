Imports System.IO
Imports System.Data.SqlClient
Imports OFFICE.GRIS0005LeftBox

Public Class GRT00012WORKINGTIME
    Inherits Page

    '共通宣言
    ''' <summary>
    ''' ログ出力クラス
    ''' </summary>
    Private CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get
    ''' <summary>
    ''' 一覧表示用クラス
    ''' </summary>
    Private CS0013ProfView As New CS0013ProfView                    'ユーザプロファイル（GridView）設定
    ''' <summary>
    ''' 帳票クラス
    ''' </summary>
    Private CS0023XLSTBL As New CS0023XLSUPLOAD                     'UPLOAD_XLSデータ取得
    ''' <summary>
    ''' 帳票出力
    ''' </summary>
    Private CS0030REPORTtbl As New CS0030REPORT                     '帳票出力(入力：TBL)
    ''' <summary>
    ''' L1出力
    ''' </summary>
    Private CS0044L1INSERT As New CS0044L1INSERT                    '統計情報
    ''' <summary>
    ''' セッション管理
    ''' </summary>
    Private CS0050Session As New CS0050SESSION                      'セッション情報
    ''' <summary>
    ''' テーブルソート
    ''' </summary>
    Private CS0026TBLSORT As New CS0026TBLSORT
    ''' <summary>
    ''' 勤怠共通クラス
    ''' </summary>
    Private T0007COM As New GRT0007COM                              '勤怠共通
    ''' <summary>
    ''' 日報共通クラス
    ''' </summary>
    Private T0005COM As New GRT0005COM                              '日報共通

    Private T0005UPDATE As New GRT0005UPDATE                        '日報ＤＢ更新
    '共通処理結果
    Private WW_ERRCODE As String = String.Empty                     'リターンコード
    Private WW_RTN_SW As String                                     '
    Private WW_DUMMY As String                                      '

    Private T0012tbl As DataTable                                   '日報テーブル（GridView用）
    Private T0012INPtbl As DataTable                                '日報テーブル（取込用）
    Private T0012WKtbl As DataTable                                 '日報テーブル（ワーク）
    Private T0012WEEKtbl As DataTable                               '日報テーブル（一週間前）

    Private WW_ERRLISTCNT As Integer                                'エラーリスト件数               

    Private WW_ERRLIST_ALL As List(Of String)                       'インポート全体のエラー
    Private WW_ERRLIST As List(Of String)                           'インポート中の１セット分のエラー

    Private Const CONST_DSPROWCOUNT As Integer = 50                 '１画面表示対象
    Private Const CONST_SCROLLROWCOUNT As Integer = 20              'マウススクロール時の増分
    Private Const CONST_DETAIL_TABID As String = "DTL1"             '詳細部タブID


    ''' <summary>
    ''' サーバ処理の遷移先
    ''' </summary>
    ''' <param name="sender">起動オブジェクト</param>
    ''' <param name="e">イベント発生時パラメータ</param>
    ''' <remarks></remarks>
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs) Handles Me.Load
        Try
            If IsPostBack Then
                '■■■ 各ボタン押下処理 ■■■
                If Not String.IsNullOrEmpty(WF_ButtonClick.Value) Then
                    Select Case WF_ButtonClick.Value
                        Case "WF_ButtonExtract"                             '絞り込みボタン押下時処理
                            WF_ButtonExtract_Click()
                        Case "WF_ButtonUPDATE"
                            WF_ButtonUPDATE_Click()
                        Case "WF_ButtonCSV"
                            WF_Print_Click("XLSX")
                        Case "WF_ButtonPrint"
                            WF_Print_Click("pdf")
                        Case "WF_ButtonFIRST"
                            WF_ButtonFIRST_Click()
                        Case "WF_ButtonLAST"
                            WF_ButtonLAST_Click()
                        Case "WF_ButtonEND"
                            WF_ButtonEND_Click()
                        Case "WF_ButtonSel"
                            WF_ButtonSel_Click()
                        Case "WF_ButtonCan"
                            WF_ButtonCan_Click()
                        Case "WF_Field_DBClick"
                            WF_Field_DBClick()
                        Case "WF_ListboxDBclick"
                            WF_Listbox_DBClick()
                        Case "WF_RadioButonClick"
                            WF_RadioButon_Click()
                        Case "WF_MEMOChange"
                            WF_MEMO_Change()
                        Case "WF_ListChange"            'リスト変更
                            WF_ListChange()
                        Case "WF_MouseWheelDown"
                        Case "WF_MouseWheelUp"
                        Case "WF_EXCEL_UPLOAD"
                            UPLOAD_EXCEL()
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
        Catch ex As Exception
            '○一覧再表示処理
            DisplayGrid()
            Master.Output(C_MESSAGE_NO.SYSTEM_ADM_ERROR, C_MESSAGE_TYPE.ERR)
        Finally

            If Not IsNothing(T0012tbl) Then
                T0012tbl.Dispose()
                T0012tbl = Nothing
            End If
            If Not IsNothing(T0012INPtbl) Then
                T0012INPtbl.Dispose()
                T0012INPtbl = Nothing
            End If
            If Not IsNothing(T0012WKtbl) Then
                T0012WKtbl.Dispose()
                T0012WKtbl = Nothing
            End If
            If Not IsNothing(T0012WEEKtbl) Then
                T0012WEEKtbl.Dispose()
                T0012WEEKtbl = Nothing
            End If

        End Try
    End Sub
    ''' <summary>
    ''' 初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub Initialize()
        Dim O_RTN As String = C_MESSAGE_NO.NORMAL
        'メッセージクリア
        WF_FIELD.Value = ""
        WF_STAFFCODE.Focus()
        '〇画面遷移処理
        MAPrefelence(O_RTN)
        '〇ヘルプ無
        Master.dispHelp = False
        '〇ドラックアンドドロップON
        Master.eventDrop = True

        '■■■ 選択情報　設定処理 ■■■
        '〇右Boxへの値設定
        rightview.MAPID_MEMO = Master.MAPID
        rightview.MAPID_REPORT = GRT00012WRKINC.MAPID
        rightview.MAPVARI = Master.MAPvariant
        rightview.COMPCODE = work.WF_SEL_CAMPCODE.Text
        rightview.PROFID = Master.PROF_REPORT
        rightview.Initialize(WW_DUMMY)

        '〇通常検索
        GRID_INITset()

        '一覧再表示処理
        DisplayGrid()

    End Sub
    ''' <summary>
    ''' 一覧再表示処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub DisplayGrid()

        Dim WW_GridPosition As Integer                 '表示位置（開始）
        Dim WW_DataCNT As Integer = 0                  '(絞り込み後)有効Data数

        '表示対象行カウント(絞り込み対象)
        If IsNothing(T0012tbl) Then
            '○画面表示データ復元
            If Not Master.RecoverTable(T0012tbl) Then Exit Sub
        End If
        '　※　絞込（Cells(4)： 0=表示対象 , 1=非表示対象)
        For i As Integer = 0 To T0012tbl.Rows.Count - 1
            If T0012tbl.Rows(i)(4) = "0" Then
                WW_DataCNT = WW_DataCNT + 1
                '行（ラインカウント）を再設定する。既存項目（SELECT）を利用
                T0012tbl.Rows(i)("SELECT") = WW_DataCNT
            End If
        Next

        '○表示Linecnt取得
        If Not Integer.TryParse(WF_GridPosition.Text, WW_GridPosition) Then
            WW_GridPosition = 1
        End If

        '○表示格納位置決定

        '表示開始_格納位置決定(次頁スクロール)
        If WF_ButtonClick.Value = "WF_MouseWheelUp" Then
            If (WW_GridPosition + CONST_SCROLLROWCOUNT) <= WW_DataCNT Then
                WW_GridPosition = WW_GridPosition + CONST_SCROLLROWCOUNT
            End If
        End If

        '表示開始_位置決定(前頁スクロール)
        If WF_ButtonClick.Value = "WF_MouseWheelDown" Then
            If (WW_GridPosition - CONST_SCROLLROWCOUNT) > 0 Then
                WW_GridPosition = WW_GridPosition - CONST_SCROLLROWCOUNT
            Else
                WW_GridPosition = 1
            End If
        End If

        '○画面（GridView）表示
        Dim WW_TBLview As DataView = New DataView(T0012tbl)

        'ソート
        WW_TBLview.Sort = "LINECNT"
        WW_TBLview.RowFilter = "HIDDEN = 0 and SELECT >= " & WW_GridPosition.ToString & " and SELECT < " & (WW_GridPosition + CONST_DSPROWCOUNT).ToString
        '一覧作成
        CS0013ProfView.CAMPCODE = work.WF_SEL_CAMPCODE.Text
        CS0013ProfView.PROFID = Master.PROF_VIEW
        CS0013ProfView.MAPID = GRT00012WRKINC.MAPID
        CS0013ProfView.VARI = Master.VIEWID
        CS0013ProfView.SRCDATA = WW_TBLview.ToTable
        CS0013ProfView.TBLOBJ = pnlListArea
        CS0013ProfView.SCROLLTYPE = CS0013ProfView.SCROLLTYPE_ENUM.None
        CS0013ProfView.LEVENT = "Onchange"
        CS0013ProfView.LFUNC = "ListChange"
        CS0013ProfView.TITLEOPT = True
        CS0013ProfView.CS0013ProfView()

        '○クリア
        If WW_TBLview.Count = 0 Then
            WF_GridPosition.Text = "1"
        Else
            WF_GridPosition.Text = WW_TBLview.Item(0)("SELECT")
        End If
        WF_STAFFCODE.Focus()

    End Sub

    '★★★★★★★★★★★★★★★★★★★★★
    'イベント処理
    '★★★★★★★★★★★★★★★★★★★★★

    ''' <summary>
    ''' 絞り込みボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonExtract_Click()

        '○入力値チェック
        Dim WW_CONVERT As String = ""
        Dim WW_TEXT As String = ""
        '乗務員
        CODENAME_get("STAFFCODE", WF_STAFFCODE.Text, WW_TEXT, WW_DUMMY)
        WF_STAFFCODE_TEXT.Text = WW_TEXT

        '○テーブルデータ 復元（絞込みボタン押下の時のみ）
        If WF_ButtonClick.Value Like "WF_ButtonExtract*" Then
            '〇データリカバリ
            If IsNothing(T0012tbl) Then
                If Not Master.RecoverTable(T0012tbl) Then Exit Sub
            End If
        End If

        '○絞り込み操作（GridView明細Hidden設定）
        For Each T0012row As DataRow In T0012tbl.Select("HDKBN='H'", "SELECT, YMD, STAFFCODE, HDKBN DESC, STDATE, STTIME, ENDDATE, ENDTIME, WORKKBN, SEQ")
            If T0012row("SELECT") = 1 Then
                T0012row("HIDDEN") = 1

                '従業員・日報　絞込判定
                If (WF_STAFFCODE.Text = "") AndAlso (WF_YMD.Text = "") Then
                    T0012row("HIDDEN") = 0
                End If

                If (WF_STAFFCODE.Text <> "") AndAlso (WF_YMD.Text = "") Then
                    If T0012row("STAFFCODE") Like WF_STAFFCODE.Text & "*" Then
                        T0012row("HIDDEN") = 0
                    End If
                End If

                If (WF_STAFFCODE.Text = "") AndAlso (WF_YMD.Text <> "") Then
                    If Not IsDate(WF_YMD.Text) Then WF_YMD.Text = C_DEFAULT_YMD

                    If T0012row("YMD") = CDate(WF_YMD.Text).ToString("yyyy/MM/dd") Then T0012row("HIDDEN") = 0
                End If

                If (WF_STAFFCODE.Text <> "") AndAlso (WF_YMD.Text <> "") Then
                    If Not IsDate(WF_YMD.Text) Then WF_YMD.Text = C_DEFAULT_YMD

                    If T0012row("STAFFCODE") Like WF_STAFFCODE.Text & "*" AndAlso
                       T0012row("YMD") = CDate(WF_YMD.Text).ToString("yyyy/MM/dd") Then
                        T0012row("HIDDEN") = 0
                    End If
                End If
            End If
        Next

        If WF_ButtonClick.Value = "WF_ButtonExtract" Then
            WF_GridPosition.Text = "1"
        End If

        '○GridViewデータをテーブルに保存（絞込みボタン押下の時のみ）
        If WF_ButtonClick.Value Like "WF_ButtonExtract*" Then
            '〇データ保存
            If Not Master.SaveTable(T0012tbl) Then Exit Sub
        End If

        '○メッセージ表示
        Master.Output(C_MESSAGE_NO.NORMAL, C_MESSAGE_TYPE.INF)
        '○カーソル設定
        WF_FIELD.Value = "WF_STAFFCODE"
        WF_STAFFCODE.Focus()

    End Sub

    ''' <summary>
    ''' 更新ボタン処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonUPDATE_Click()

        Dim O_RTN As String = C_MESSAGE_NO.NORMAL
        Dim WW_RTN As String = C_MESSAGE_NO.NORMAL

        rightview.SetErrorReport("")
        If IsNothing(T0012tbl) Then
            If Not Master.RecoverTable(T0012tbl) Then Exit Sub
        End If
        '〇ヘッダーの反映
        T0012_HeadToDetail(T0012tbl)

        '重複チェック
        Dim WW_MSG As String = String.Empty
        T0005COM.CheckDuplicateDataT0005(T0012tbl, WW_MSG, WW_RTN)
        If Not isNormal(WW_RTN) Then
            rightview.AddErrorReport("内部処理エラー")
            rightview.AddErrorReport(ControlChars.NewLine & WW_MSG)

            CS0011LOGWRITE.INFSUBCLASS = "T0005_DuplCheck"             'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "T0005_DuplCheck"                 '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = WW_MSG
            CS0011LOGWRITE.MESSAGENO = WW_RTN
            CS0011LOGWRITE.CS0011LOGWrite()                            'ログ出力
            Master.Output(WW_RTN, C_MESSAGE_TYPE.ABORT, "T0005_DuplCheck")
            Exit Sub
        End If

        '--------------------------------------------------------------------
        'ＤＢ更新
        '--------------------------------------------------------------------
        'DataBase接続文字
        Using SQLcon As SqlConnection = CS0050Session.getConnection
            'トランザクション
            Dim SQLtrn As SqlClient.SqlTransaction = Nothing

            SQLcon.Open() 'DataBase接続(Open)
            'トランザクション開始
            SQLtrn = Nothing

            '〇日報ＤＢ更新

            '統計DB出力用項目設定
            CS0026TBLSORT.TABLE = T0012tbl
            CS0026TBLSORT.SORTING = "SELECT, YMD, STAFFCODE, HDKBN DESC, STDATE, STTIME, ENDDATE, ENDTIME, WORKKBN, SEQ"
            '削除データの退避
            CS0026TBLSORT.FILTER = "SELECT = '0'"
            Dim WW_T0012DELtbl As DataTable = CS0026TBLSORT.Sort()
            '有効データのみ
            CS0026TBLSORT.FILTER = "SELECT = '1'"
            Dim WW_T0012SELtbl As DataTable = CS0026TBLSORT.Sort()

            '〇トリップ判定・回送判定・出荷日内荷積荷卸回数判定
            T0005COM.ReEditT0005(WW_T0012SELtbl, work.WF_SEL_CAMPCODE.Text, WW_RTN)
            '有効データと１週間前データの分離
            CS0026TBLSORT.TABLE = WW_T0012SELtbl
            CS0026TBLSORT.SORTING = "SELECT, YMD, STAFFCODE, HDKBN DESC, STDATE, STTIME, ENDDATE, ENDTIME, WORKKBN, SEQ"
            CS0026TBLSORT.FILTER = "YMD >= #" & work.WF_SEL_STYMD.Text & "#"
            T0012tbl = CS0026TBLSORT.Sort()

            '有効レコード＋削除レコード（元に戻す）
            T0012tbl.Merge(WW_T0012DELtbl)

            Dim WW_DATE As Date = Date.Now
            '〇T0005更新処理
            T0005UPDATE.SQLcon = SQLcon
            T0005UPDATE.SQLtrn = SQLtrn
            T0005UPDATE.T0005tbl = T0012tbl
            T0005UPDATE.ENTRYDATE = WW_DATE
            T0005UPDATE.UPDUSERID = Master.USERID
            T0005UPDATE.UPDTERMID = Master.USERTERMID
            T0005UPDATE.Update()
            If isNormal(T0005UPDATE.ERR) Then
                T0012tbl = T0005UPDATE.T0005tbl
            Else
                Master.Output(T0005UPDATE.ERR, C_MESSAGE_TYPE.ABORT, "例外発生")
                Exit Sub
            End If
            '〇不要テーブルデータ除去
            If Not IsNothing(WW_T0012DELtbl) Then
                WW_T0012DELtbl.Dispose()
                WW_T0012DELtbl = Nothing
            End If
            If Not IsNothing(WW_T0012SELtbl) Then
                WW_T0012SELtbl.Dispose()
                WW_T0012SELtbl = Nothing
            End If
            '〇統計ＤＢ更新
            Dim L00001tbl = New DataTable
            CS0044L1INSERT.CS0044L1ColmnsAdd(L00001tbl)

            '有効データのみ
            CS0026TBLSORT.TABLE = T0012tbl
            CS0026TBLSORT.SORTING = "SELECT, YMD, STAFFCODE, HDKBN DESC, STDATE, STTIME, ENDDATE, ENDTIME, WORKKBN, SEQ"
            CS0026TBLSORT.FILTER = "SELECT = '1'"
            Dim WW_T0012LSELtbl As DataTable = CS0026TBLSORT.Sort()

            '削除データ（削除処理）
            Dim WW_DATENOW As DateTime = Date.Now
            '日報ＤＢ更新
            Dim SQLStr As String =
                        "UPDATE L0001_TOKEI " _
                      & "SET DELFLG         = '1'  " _
                      & "  , UPDYMD         = @P05 " _
                      & "  , UPDUSER        = @P06 " _
                      & "  , UPDTERMID      = @P07 " _
                      & "  , RECEIVEYMD     = @P08 " _
                      & "WHERE CAMPCODE     = @P01 " _
                      & "  and DENTYPE      = @P02 " _
                      & "  and NACSHUKODATE = @P03 " _
                      & "  and KEYSTAFFCODE = @P04 " _
                      & "  and DELFLG      <> '1'  "

            Using SQLcmd As SqlCommand = New SqlCommand(SQLStr, SQLcon, SQLtrn)
                Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@P03", System.Data.SqlDbType.Date)
                Dim PARA04 As SqlParameter = SQLcmd.Parameters.Add("@P04", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA05 As SqlParameter = SQLcmd.Parameters.Add("@P05", System.Data.SqlDbType.DateTime)
                Dim PARA06 As SqlParameter = SQLcmd.Parameters.Add("@P06", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA07 As SqlParameter = SQLcmd.Parameters.Add("@P07", System.Data.SqlDbType.NVarChar, 30)
                Dim PARA08 As SqlParameter = SQLcmd.Parameters.Add("@P08", System.Data.SqlDbType.DateTime)

                For Each T0012row As DataRow In WW_T0012LSELtbl.Rows
                    '〇更新対象レコードは統計情報を一度削除する（ヘッダーを用いて削除）
                    If T0012row("HDKBN") = "H" AndAlso T0012row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING Then

                        Try
                            PARA01.Value = work.WF_SEL_CAMPCODE.Text
                            PARA02.Value = "T05"
                            PARA03.Value = T0012row("YMD")
                            PARA04.Value = T0012row("STAFFCODE")
                            PARA05.Value = WW_DATENOW
                            PARA06.Value = Master.USERID
                            PARA07.Value = Master.USERTERMID
                            PARA08.Value = C_DEFAULT_YMD

                            SQLcmd.ExecuteNonQuery()
                        Catch ex As Exception
                            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "L0001_TOKEI")

                            CS0011LOGWRITE.INFSUBCLASS = "L0001_Delete"                 'SUBクラス名
                            CS0011LOGWRITE.INFPOSI = "DB:UPDATE L0001_TOKEI"            '
                            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
                            CS0011LOGWRITE.TEXT = ex.ToString()
                            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
                            Exit Sub

                        End Try

                    End If
                Next
            End Using
            '〇 L00001統計ＤＢ編集
            T0005COM.EditL00001(WW_T0012LSELtbl, L00001tbl, WW_RTN)
            '〇 L00001統計ＤＢサマリー
            T0005COM.SumL00001(work.WF_SEL_CAMPCODE.Text, work.WF_SEL_UORG.Text, Master.USERID, L00001tbl, WW_RTN)

            WW_DATENOW = Date.Now
            For Each L00001row As DataRow In L00001tbl.Rows
                L00001row("INITYMD") = WW_DATENOW '登録年月日
                L00001row("UPDYMD") = WW_DATENOW  '更新年月日
                L00001row("UPDUSER") = CS0050Session.USERID   '更新ユーザＩＤ
                L00001row("UPDTERMID") = CS0050Session.TERMID    '更新端末
                L00001row("RECEIVEYMD") = C_DEFAULT_YMD   '集信日時
            Next

            '統計DB出力
            CS0044L1INSERT.SQLCON = SQLcon
            CS0044L1INSERT.CS0044L1Insert(L00001tbl)
            If Not isNormal(CS0044L1INSERT.ERR) Then
                Master.Output(CS0044L1INSERT.ERR, C_MESSAGE_TYPE.ABORT, "CS0044L1INSERT")
                Exit Sub
            End If

            If Not IsNothing(WW_T0012LSELtbl) Then
                WW_T0012LSELtbl.Dispose()
                WW_T0012LSELtbl = Nothing
            End If
            If Not IsNothing(L00001tbl) Then
                L00001tbl.Dispose()
                L00001tbl = Nothing
            End If

            '検索SQL文
            Dim SQLStrTime As String =
                 "SELECT TIMSTP = cast(A.UPDTIMSTP  as bigint) " _
                & "     ,ENTRYDATE       					   " _
                & " FROM T0005_NIPPO AS A					   " _
                & " WHERE A.CAMPCODE         = @P01            " _
                & "  and  A.SHIPORG          = @P02            " _
                & "  and  A.TERMKBN          = @P03            " _
                & "  and  A.YMD              = @P04            " _
                & "  and  A.STAFFCODE        = @P05            " _
                & "  and  A.SEQ              = @P06            " _
                & "  and  A.NIPPONO          = @P07            " _
                & "  and  A.DELFLG          <> '1'             "

            Using SQLcmdTime As New SqlCommand(SQLStrTime, SQLcon, SQLtrn)

                Dim PARAT1 As SqlParameter = SQLcmdTime.Parameters.Add("@P01", System.Data.SqlDbType.NVarChar, 20)
                Dim PARAT2 As SqlParameter = SQLcmdTime.Parameters.Add("@P02", System.Data.SqlDbType.NVarChar, 20)
                Dim PARAT3 As SqlParameter = SQLcmdTime.Parameters.Add("@P03", System.Data.SqlDbType.NVarChar, 20)
                Dim PARAT4 As SqlParameter = SQLcmdTime.Parameters.Add("@P04", System.Data.SqlDbType.Date)
                Dim PARAT5 As SqlParameter = SQLcmdTime.Parameters.Add("@P05", System.Data.SqlDbType.NVarChar, 20)
                Dim PARAT6 As SqlParameter = SQLcmdTime.Parameters.Add("@P06", System.Data.SqlDbType.NVarChar, 20)
                Dim PARAT7 As SqlParameter = SQLcmdTime.Parameters.Add("@P07", System.Data.SqlDbType.NVarChar, 20)
                'タイムスタンプの取得
                For Each WW_row As DataRow In T0012tbl.Rows
                    Try
                        If WW_row("HDKBN") = "D" AndAlso
                           WW_row("SELECT") = "1" AndAlso
                           WW_row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING Then

                            '○関連受注指定
                            PARAT1.Value = WW_row("CAMPCODE")
                            PARAT2.Value = WW_row("SHIPORG")
                            PARAT3.Value = WW_row("TERMKBN")
                            PARAT4.Value = WW_row("YMD")
                            PARAT5.Value = WW_row("STAFFCODE")
                            PARAT6.Value = WW_row("SEQ")
                            PARAT7.Value = WW_row("NIPPONO")

                            '■SQL実行
                            Using SQLdr As SqlDataReader = SQLcmdTime.ExecuteReader()

                                While SQLdr.Read
                                    WW_row("TIMSTP") = SQLdr("TIMSTP")
                                    WW_row("ENTRYDATE") = SQLdr("ENTRYDATE")
                                End While

                            End Using
                        End If
                    Catch ex As Exception
                        CS0011LOGWRITE.INFSUBCLASS = "CS0047T5_Select"              'SUBクラス名
                        CS0011LOGWRITE.INFPOSI = "DB:SELECT T0005_NIPPO"            '
                        CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
                        CS0011LOGWRITE.TEXT = ex.ToString()
                        CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                        CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
                        Exit Sub
                    End Try
                Next
            End Using
            'タイムスタンプをヘッダに反映
            CS0026TBLSORT.TABLE = T0012tbl
            CS0026TBLSORT.SORTING = "SELECT, YMD, STAFFCODE, HDKBN DESC, STDATE, STTIME, ENDDATE, ENDTIME, WORKKBN, SEQ"
            CS0026TBLSORT.FILTER = ""
            T0012tbl = CS0026TBLSORT.Sort()

            For i As Integer = T0012tbl.Rows.Count - 1 To 0 Step -1
                Dim T0012row As DataRow = T0012tbl.Rows(i)
                If T0012row("SELECT") = "0" Then
                    T0012row.Delete()
                Else
                    If T0012row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING Then
                        T0012row("OPERATION") = C_LIST_OPERATION_CODE.NODATA

                        If T0012row("HDKBN") = "H" Then
                            Dim WW_row As DataRow = T0012tbl.Rows(i + 1)
                            T0012row("TIMSTP") = WW_row("TIMSTP")
                        End If
                    End If
                End If
            Next

        End Using
        '○GridViewデータをテーブルに保存
        If Not Master.SaveTable(T0012tbl) Then Exit Sub

        '絞込みボタン処理（GridViewの表示）を行う
        WF_ButtonExtract_Click()
    End Sub

    ''' <summary>
    ''' ﾀﾞｳﾝﾛｰﾄﾞ(PDF出力)・一覧印刷ボタン処理
    ''' </summary>
    ''' <param name="OutType"></param>
    ''' <remarks></remarks>
    Protected Sub WF_Print_Click(ByVal OutType As String)

        'テーブルデータ 復元
        If Not Master.RecoverTable(T0012tbl) Then Exit Sub

        Using WW_TBLview As DataView = New DataView(T0012tbl)
            WW_TBLview.Sort = "CAMPCODE, SHIPORG, TERMKBN, YMD, STAFFCODE, SEQ"
            WW_TBLview.RowFilter = "HDKBN='H' and SELECT = '1' and HIDDEN='0' "
            Using WW_TBL As DataTable = WW_TBLview.ToTable

                '帳票出力dll Interface
                CS0030REPORTtbl.CAMPCODE = work.WF_SEL_CAMPCODE.Text
                CS0030REPORTtbl.PROFID = Master.PROF_REPORT
                CS0030REPORTtbl.MAPID = GRT00012WRKINC.MAPID                   'PARAM01:画面ID
                CS0030REPORTtbl.REPORTID = rightview.GetReportId               'PARAM02:帳票ID
                CS0030REPORTtbl.FILEtyp = OutType                              'PARAM03:出力ファイル形式
                CS0030REPORTtbl.TBLDATA = WW_TBL                               'PARAM04:データ参照tabledata
                CS0030REPORTtbl.CS0030REPORT()

                If Not isNormal(CS0030REPORTtbl.ERR) Then
                    Master.Output(CS0030REPORTtbl.ERR, C_MESSAGE_TYPE.ABORT, "CS0030REPORT")
                    Exit Sub
                End If

                '別画面でPDFを表示
                WF_PrintURL.Value = CS0030REPORTtbl.URL
                ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_PDFPrint();", True)
            End Using
        End Using
    End Sub
    ''' <summary>
    ''' 終了ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonEND_Click()
        '画面遷移実行
        Master.TransitionPrevPage()

    End Sub

    ''' <summary>
    ''' 先頭頁ボタン処理  
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonFIRST_Click()
        '○データリカバリ 
        If Not Master.RecoverTable(T0012tbl) Then Exit Sub
        '先頭頁に移動
        WF_GridPosition.Text = "1"

    End Sub

    ''' <summary>
    ''' 最終頁ボタン処理  
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonLAST_Click()
        '○データリカバリ 
        If Not Master.RecoverTable(T0012tbl) Then Exit Sub
        '○対象データ件数取得
        Dim WW_TBLview As DataView
        WW_TBLview = New DataView(T0012tbl)
        WW_TBLview.RowFilter = "HIDDEN= '0'"

        '最終頁に移動
        If WW_TBLview.Count Mod 10 = 0 Then
            WF_GridPosition.Text = WW_TBLview.Count - (WW_TBLview.Count Mod CONST_SCROLLROWCOUNT)
        Else
            WF_GridPosition.Text = WW_TBLview.Count - (WW_TBLview.Count Mod CONST_SCROLLROWCOUNT) + 1
        End If

    End Sub
    ''' <summary>
    ''' フィールドダブルクリック処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_Field_DBClick()
        '〇LeftBox処理（フィールドダブルクリック時）
        If Not String.IsNullOrEmpty(WF_LeftMViewChange.Value) AndAlso
            Integer.TryParse(WF_LeftMViewChange.Value, WF_LeftMViewChange.Value) Then

            With leftview
                If WF_LeftMViewChange.Value <> LIST_BOX_CLASSIFICATION.LC_CALENDAR Then
                    Dim prmData As Hashtable = work.createFIXParam(work.WF_SEL_CAMPCODE.Text)
                    Select Case WF_LeftMViewChange.Value
                        Case LIST_BOX_CLASSIFICATION.LC_STAFFCODE
                            prmData = work.createSTAFFParam(work.WF_SEL_CAMPCODE.Text, work.WF_SEL_UORG.Text)

                    End Select
                    .SetListBox(WF_LeftMViewChange.Value, WW_DUMMY, prmData)
                    .ActiveListBox()
                Else
                    '日付の場合、入力日付のカレンダーが表示されるように入力値をカレンダーに渡す
                    Select Case WF_FIELD.Value
                        Case "WF_YMD"
                            .WF_Calendar.Text = WF_YMD.Text
                        Case "STDATE"
                            Dim stDate = Request.Form("txt" & pnlListArea.ID & "STDATE" & WF_GridDBclick.Text)
                            .WF_Calendar.Text = stDate
                        Case "ENDDATE"
                            Dim endDate = Request.Form("txt" & pnlListArea.ID & "ENDDATE" & WF_GridDBclick.Text)
                            .WF_Calendar.Text = endDate
                    End Select
                    .ActiveCalendar()
                End If
            End With
        End If
    End Sub
    ''' <summary>
    ''' 左リストボックスダブルクリック時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_Listbox_DBClick()
        WF_ButtonSel_Click()

    End Sub
    ''' <summary>
    ''' 右ボックスのラジオボタン選択時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_RadioButon_Click()
        '〇RightBox処理（ラジオボタン選択）
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
    ''' メモ欄変更時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_MEMO_Change()
        '〇RightBox処理（右Boxメモ変更時）
        rightview.MAPID = Master.MAPID
        rightview.Save(Master.USERID, Master.USERTERMID, WW_DUMMY)
    End Sub

    ''' <summary>
    ''' leftBOX選択ボタン処理(ListBox値 ---> detailbox)　
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonSel_Click()

        Dim WW_SelectValue() As String

        WF_SelectedIndex.Value = leftview.WF_LeftListBox.SelectedIndex
        WW_SelectValue = leftview.GetActiveValue

        Select Case WF_FIELD.Value
            Case "WF_STAFFCODE"
                '乗務員 
                WF_STAFFCODE_TEXT.Text = WW_SelectValue(1)
                WF_STAFFCODE.Text = WW_SelectValue(0)
                WF_STAFFCODE.Focus()
            Case "WF_YMD"
                '出庫日 
                WF_YMD.Text = WW_SelectValue(0)
                WF_YMD.Focus()
            Case "STDATE", "ENDDATE"
                '始業日付, 終業日付

                Dim WW_LINECNT As Integer = 0
                '○ LINECNT取得
                If Not Integer.TryParse(WF_GridDBclick.Text, WW_LINECNT) Then Exit Sub
                '○画面表示データ復元
                If Not Master.RecoverTable(T0012tbl) Then Exit Sub

                '対象ヘッダー取得
                Dim updHeader = T0012tbl.AsEnumerable.
                            FirstOrDefault(Function(x) x.Item("HDKBN") = "H" AndAlso x.Item("LINECNT") = WW_LINECNT)
                If IsNothing(updHeader) Then Exit Sub
                updHeader.Item(WF_FIELD.Value) = CDate(WW_SelectValue(0)).ToString("yyyy/MM/dd")
                updHeader("OPERATION") = C_LIST_OPERATION_CODE.UPDATING

                '○ 画面表示データ保存
                If Not Master.SaveTable(T0012tbl) Then Exit Sub

        End Select

        '○ 画面左サイドボックス非表示は、画面JavaScript(InitLoad)で実行
        WF_LeftboxOpen.Value = ""
        WF_FIELD.Value = ""
        WF_LeftMViewChange.Value = ""
    End Sub

    ''' <summary>
    ''' leftBOXキャンセルボタン処理　
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonCan_Click()

        '○ フォーカスセット
        Select Case WF_FIELD.Value
            Case "WF_STAFFCODE"
                '従業員コード　 
                WF_STAFFCODE.Focus()
            Case "WF_YMD"
                '出庫日　 
                WF_YMD.Focus()

        End Select

        '○ 画面左サイドボックス非表示は、画面JavaScript(InitLoad)で実行
        WF_LeftboxOpen.Value = ""
        WF_FIELD.Value = ""
        WF_LeftMViewChange.Value = ""

    End Sub


    ''' <summary>
    ''' リスト変更時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ListChange()

        Dim WW_LINECNT As Integer = 0

        '○ LINECNT取得
        If Not Integer.TryParse(WF_GridDBclick.Text, WW_LINECNT) Then Exit Sub
        '○画面表示データ復元
        If Not Master.RecoverTable(T0012tbl) Then Exit Sub

        '対象ヘッダー取得
        Dim updHeader = T0012tbl.AsEnumerable.
                            FirstOrDefault(Function(x) x.Item("HDKBN") = "H" AndAlso x.Item("LINECNT") = WW_LINECNT)
        If IsNothing(updHeader) Then Exit Sub

        '対象フォーム項目取得
        Dim stDate = Request.Form("txt" & pnlListArea.ID & "STDATE" & WF_GridDBclick.Text)
        Dim stTime = Request.Form("txt" & pnlListArea.ID & "STTIME" & WF_GridDBclick.Text)
        Dim endDate = Request.Form("txt" & pnlListArea.ID & "ENDDATE" & WF_GridDBclick.Text)
        Dim endTime = Request.Form("txt" & pnlListArea.ID & "ENDTIME" & WF_GridDBclick.Text)
        If IsNothing(stDate) OrElse IsNothing(stTime) OrElse IsNothing(endDate) OrElse IsNothing(endTime) Then Exit Sub

        If updHeader("STDATE").ToString.Replace("/", "") <> Convert.ToString(stDate).Replace("/", "") Then
            updHeader("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
            If IsDate(stDate) Then
                updHeader("STDATE") = CDate(stDate).ToString("yyyy/MM/dd")
            Else
                'updHeader("STDATE") = C_DEFAULT_YMD
                updHeader("STDATE") = ""
            End If
        End If

        Dim wkTime As Date

        If Not stTime.Contains(":") Then
            stTime = stTime.PadLeft(4, "0").Insert(2, ":")
        End If
        Date.TryParse(stTime, wkTime)
        If CDate(updHeader("STTIME").ToString) <> wkTime Then
            updHeader("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
        End If
        updHeader("STTIME") = wkTime.ToString("HH:mm")

        If updHeader("ENDDATE").ToString.Replace("/", "") <> Convert.ToString(endDate).Replace("/", "") Then
            updHeader("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
            If IsDate(endDate) Then
                updHeader("ENDDATE") = CDate(endDate).ToString("yyyy/MM/dd")
            Else
                'updHeader("ENDDATE") = C_DEFAULT_YMD
                updHeader("ENDDATE") = ""
            End If
        End If

        If Not endTime.Contains(":") Then
            endTime = endTime.PadLeft(4, "0").Insert(2, ":")
        End If
        Date.TryParse(endTime, wkTime)
        If CDate(updHeader("ENDTIME").ToString) <> wkTime Then
            updHeader("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
        End If
        updHeader("ENDTIME") = wkTime.ToString("HH:mm")

        ' 〇チェック処理
        T0012tbl_CheckHead(T0012tbl, WW_RTN_SW)

        CS0026TBLSORT.TABLE = T0012tbl
        CS0026TBLSORT.FILTER = "HDKBN = 'H'"
        CS0026TBLSORT.SORTING = "YMD, STAFFCODE, HDKBN"
        Dim WW_T0012tbl As DataTable = CS0026TBLSORT.Sort()
        For i As Integer = 0 To WW_T0012tbl.Rows.Count - 1
            Dim WW_ERRWORD As String = ""
            WW_ERRWORD = rightview.GetErrorReport.Replace("@L" & WW_T0012tbl(i)("YMD") & WW_T0012tbl(i)("STAFFCODE") & "L@", WW_T0012tbl(i)("LINECNT"))
            rightview.SetErrorReport(WW_ERRWORD)
        Next

        '○ 画面表示データ保存
        If Not Master.SaveTable(T0012tbl) Then Exit Sub
    End Sub

    ''' <summary>
    ''' GridView用データ取得
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub GRID_INITset()

        Dim WW_CONVERT As String = ""

        '■■■ 画面表示用データ取得 ■■■
        Dim WW_SORT As String = ""
        'ユーザプロファイル（変数）内容検索(自ユーザ権限＆抽出条件なしで検索)
        Try
            CS0026TBLSORT.COMPCODE = work.WF_SEL_CAMPCODE.Text
            CS0026TBLSORT.MAPID = Master.MAPID
            CS0026TBLSORT.PROFID = Master.PROF_VIEW
            CS0026TBLSORT.TAB = ""
            CS0026TBLSORT.VARI = Master.VIEWID
            CS0026TBLSORT.TABLE = T0012tbl
            CS0026TBLSORT.GetSorting()
            WW_SORT = CS0026TBLSORT.SORTING
            '■テーブル検索結果をテーブル退避
            '日報DB更新用テーブル

            T0005COM.AddColumnT0005tbl(T0012tbl)

            'DataBase接続文字
            Using SQLcon As SqlConnection = CS0050Session.getConnection
                SQLcon.Open() 'DataBase接続(Open)

                '検索SQL文
                '　検索説明
                '　　Step1：操作USERが、メンテナンス可能なUSERを取得
                '　　　　　　※権限ではUSER、MAPで行う必要があるが、絞り込み効率を勘案し、最初にUSERで処理を限定
                '　　Step2：メンテナンス可能USERおよびデフォルトUSERのTBL(S0007_UPROFVARI)を取得
                '　　        画面表示は、参照可能および更新ユーザに関連するTBLデータとなる
                '　　　　　　※権限について（参考）　権限チェックは、表追加のタイミングで行う。
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
                     "SELECT 0                                  as      LINECNT           , " _
                   & "       ''                                 as      OPERATION         , " _
                   & "       TIMSTP = cast(A.UPDTIMSTP as bigint)                         , " _
                   & "       0                                  as      'SELECT'          , " _
                   & "       1                                  as      HIDDEN            , " _
                   & "       ''                                 as      ORDERUMU          , " _
                   & "       '0'                                as      EXTRACTCNT        , " _
                   & "       'OFF'                              as      CTRL              , " _
                   & "       ''                                 as      TWOMANTRIP        , " _
                   & "       isnull(rtrim(A.CAMPCODE),'')       as      CAMPCODE          , " _
                   & "       isnull(rtrim(M1.NAMES),'')         as      CAMPNAMES         , " _
                   & "       isnull(rtrim(A.SHIPORG),'')        as      SHIPORG           , " _
                   & "       isnull(rtrim(M2.NAMES),'')         as      SHIPORGNAMES      , " _
                   & "       isnull(rtrim(A.TERMKBN),'')        as      TERMKBN           , " _
                   & "       isnull(rtrim(F1.VALUE1),'')        as      TERMKBNNAMES      , " _
                   & "       isnull(rtrim(A.YMD),'')            as      YMD               , " _
                   & "       isnull(rtrim(A.NIPPONO),'')        as      NIPPONO           , " _
                   & "       isnull(rtrim(A.WORKKBN),'')        as      WORKKBN           , " _
                   & "       isnull(rtrim(F2.VALUE1),'')        as      WORKKBNNAMES      , " _
                   & "       isnull(A.SEQ,'0')                  as      SEQ               , " _
                   & "       isnull(rtrim(A.STAFFCODE),'')      as      STAFFCODE         , " _
                   & "       isnull(rtrim(A.ENTRYDATE),'')      as      ENTRYDATE         , " _
                   & "       isnull(rtrim(B.STAFFNAMES),'')     as STAFFNAMES        , " _
                   & "       isnull(rtrim(A.SUBSTAFFCODE),'')   as SUBSTAFFCODE      , " _
                   & "       isnull(rtrim(B2.STAFFNAMES),'')    as SUBSTAFFNAMES     , " _
                   & "       isnull(rtrim(A.CREWKBN),'')        as CREWKBN           , " _
                   & "       isnull(rtrim(F3.VALUE1),'')        as CREWKBNNAMES      , " _
                   & "       isnull(rtrim(A.GSHABAN),'')        as GSHABAN           , " _
                   & "       ''                                 as GSHABANLICNPLTNO  , " _
                   & "       isnull(rtrim(A.STDATE),'')         as STDATE , " _
                   & "       isnull(rtrim(A.STTIME),'')         as STTIME , " _
                   & "       isnull(rtrim(A.ENDDATE),'')        as ENDDATE , " _
                   & "       isnull(rtrim(A.ENDTIME),'')        as ENDTIME , " _
                   & "       isnull(rtrim(A.WORKTIME),'')       as WORKTIME , " _
                   & "       isnull(rtrim(A.MOVETIME),'')       as MOVETIME , " _
                   & "       isnull(rtrim(A.ACTTIME),'')        as ACTTIME , " _
                   & "       isnull(A.PRATE,'0')                as PRATE , " _
                   & "       isnull(A.CASH,'0')                 as CASH , " _
                   & "       isnull(A.TICKET,'0')               as TICKET , " _
                   & "       isnull(A.ETC,'0')                  as ETC , " _
                   & "       isnull(A.TOTALTOLL,'0')            as TOTALTOLL , " _
                   & "       isnull(A.STMATER,'0')              as STMATER , " _
                   & "       isnull(A.ENDMATER,'0')             as ENDMATER , " _
                   & "       isnull(A.RUIDISTANCE,'0')          as RUIDISTANCE , " _
                   & "       isnull(A.SOUDISTANCE,'0')          as SOUDISTANCE , " _
                   & "       isnull(A.JIDISTANCE,'0')           as JIDISTANCE , " _
                   & "       isnull(A.KUDISTANCE,'0')           as KUDISTANCE , " _
                   & "       isnull(A.IPPDISTANCE,'0')          as IPPDISTANCE , " _
                   & "       isnull(A.KOSDISTANCE,'0')          as KOSDISTANCE , " _
                   & "       isnull(A.IPPJIDISTANCE,'0')        as IPPJIDISTANCE , " _
                   & "       isnull(A.IPPKUDISTANCE,'0')        as IPPKUDISTANCE , " _
                   & "       isnull(A.KOSJIDISTANCE,'0')        as KOSJIDISTANCE , " _
                   & "       isnull(A.KOSKUDISTANCE,'0')        as KOSKUDISTANCE , " _
                   & "       isnull(A.KYUYU,'0')                as KYUYU , " _
                   & "       isnull(rtrim(A.TORICODE),'')       as TORICODE , " _
                   & "       isnull(rtrim(MC2.NAMES),'')        as TORINAMES , " _
                   & "       isnull(rtrim(A.SHUKABASHO),'')     as SHUKABASHO , " _
                   & "       isnull(rtrim(MC62.NAMES),'')       as SHUKABASHONAMES , " _
                   & "       isnull(rtrim(A.SHUKADATE),'')      as SHUKADATE , " _
                   & "       isnull(rtrim(A.TODOKECODE),'')     as TODOKECODE , " _
                   & "       isnull(rtrim(MC6.NAMES),'')        as TODOKENAMES , " _
                   & "       isnull(rtrim(A.TODOKEDATE),'')     as TODOKEDATE , " _
                   & "       isnull(rtrim(A.OILTYPE1),'')       as OILTYPE1 , " _
                   & "       isnull(rtrim(A.PRODUCT11),'')      as PRODUCT11 , " _
                   & "       isnull(rtrim(A.PRODUCT21),'')      as PRODUCT21 , " _
                   & "       isnull(rtrim(A.PRODUCTCODE1),'')   as PRODUCTCODE1 ," _
                   & "       ''                                 as PRODUCT1NAMES , " _
                   & "       isnull(A.SURYO1,'0')               as SURYO1 , " _
                   & "       isnull(rtrim(A.STANI1),'')         as STANI1 , " _
                   & "       isnull(rtrim(F41.VALUE1),'')       as STANI1NAMES , " _
                   & "       isnull(rtrim(A.OILTYPE2),'')       as OILTYPE2 , " _
                   & "       isnull(rtrim(A.PRODUCT12),'')      as PRODUCT12 , " _
                   & "       isnull(rtrim(A.PRODUCT22),'')      as PRODUCT22 , " _
                   & "       isnull(rtrim(A.PRODUCTCODE2),'')   as PRODUCTCODE2 ," _
                   & "       ''                                 as PRODUCT2NAMES , " _
                   & "       isnull(A.SURYO2,'0')               as SURYO2 , " _
                   & "       isnull(rtrim(A.STANI2),'')         as STANI2 , " _
                   & "       isnull(rtrim(F42.VALUE1),'')       as STANI2NAMES , " _
                   & "       isnull(rtrim(A.OILTYPE3),'')       as OILTYPE3 , " _
                   & "       isnull(rtrim(A.PRODUCT13),'')      as PRODUCT13 , " _
                   & "       isnull(rtrim(A.PRODUCT23),'')      as PRODUCT23 , " _
                   & "       isnull(rtrim(A.PRODUCTCODE3),'')   as PRODUCTCODE3 ," _
                   & "       ''                                 as PRODUCT3NAMES , " _
                   & "       isnull(A.SURYO3,'0')               as SURYO3 , " _
                   & "       isnull(rtrim(A.STANI3),'')         as STANI3 , " _
                   & "       isnull(rtrim(F43.VALUE1),'')       as STANI3NAMES , " _
                   & "       isnull(rtrim(A.OILTYPE4),'')       as OILTYPE4 , " _
                   & "       isnull(rtrim(A.PRODUCT14),'')      as PRODUCT14 , " _
                   & "       isnull(rtrim(A.PRODUCT24),'')      as PRODUCT24 , " _
                   & "       isnull(rtrim(A.PRODUCTCODE4),'')   as PRODUCTCODE4 ," _
                   & "       ''                                 as PRODUCT4NAMES , " _
                   & "       isnull(A.SURYO4,'0')               as SURYO4 , " _
                   & "       isnull(rtrim(A.STANI4),'')         as STANI4 , " _
                   & "       isnull(rtrim(F44.VALUE1),'')       as STANI4NAMES , " _
                   & "       isnull(rtrim(A.OILTYPE5),'')       as OILTYPE5 , " _
                   & "       isnull(rtrim(A.PRODUCT15),'')      as PRODUCT15 , " _
                   & "       isnull(rtrim(A.PRODUCT25),'')      as PRODUCT25 , " _
                   & "       isnull(rtrim(A.PRODUCTCODE5),'')   as PRODUCTCODE5 ," _
                   & "       ''                                 as PRODUCT5NAMES , " _
                   & "       isnull(A.SURYO5,'0')               as SURYO5 , " _
                   & "       isnull(rtrim(A.STANI5),'')         as STANI5 , " _
                   & "       isnull(rtrim(F45.VALUE1),'')       as STANI5NAMES , " _
                   & "       isnull(rtrim(A.OILTYPE6),'')       as OILTYPE6 , " _
                   & "       isnull(rtrim(A.PRODUCT16),'')      as PRODUCT16 , " _
                   & "       isnull(rtrim(A.PRODUCT26),'')      as PRODUCT26 , " _
                   & "       isnull(rtrim(A.PRODUCTCODE6),'')   as PRODUCTCODE6 ," _
                   & "       ''                                 as PRODUCT6NAMES , " _
                   & "       isnull(A.SURYO6,'0')               as SURYO6 , " _
                   & "       isnull(rtrim(A.STANI6),'')         as STANI6 , " _
                   & "       isnull(rtrim(F46.VALUE1),'')       as STANI6NAMES , " _
                   & "       isnull(rtrim(A.OILTYPE7),'')       as OILTYPE7 , " _
                   & "       isnull(rtrim(A.PRODUCT17),'')      as PRODUCT17 , " _
                   & "       isnull(rtrim(A.PRODUCT27),'')      as PRODUCT27 , " _
                   & "       isnull(rtrim(A.PRODUCTCODE7),'')   as PRODUCTCODE7 ," _
                   & "       ''                                 as PRODUCT7NAMES , " _
                   & "       isnull(A.SURYO7,'0')               as SURYO7 , " _
                   & "       isnull(rtrim(A.STANI7),'')         as STANI7 , " _
                   & "       isnull(rtrim(F47.VALUE1),'')       as STANI7NAMES , " _
                   & "       isnull(rtrim(A.OILTYPE8),'')       as OILTYPE8 , " _
                   & "       isnull(rtrim(A.PRODUCT18),'')      as PRODUCT18 , " _
                   & "       isnull(rtrim(A.PRODUCT28),'')      as PRODUCT28 , " _
                   & "       isnull(rtrim(A.PRODUCTCODE8),'')   as PRODUCTCODE8 ," _
                   & "       ''                                 as PRODUCT8NAMES , " _
                   & "       isnull(A.SURYO8,'0')               as SURYO8 , " _
                   & "       isnull(rtrim(A.STANI8),'')         as STANI8 , " _
                   & "       isnull(rtrim(F48.VALUE1),'')       as STANI8NAMES , " _
                   & "       isnull(A.TOTALSURYO,'0')           as TOTALSURYO , " _
                   & "       isnull(rtrim(A.ORDERNO),'')        as ORDERNO , " _
                   & "       isnull(rtrim(A.DETAILNO),'')       as DETAILNO , " _
                   & "       isnull(rtrim(A.TRIPNO),'')         as TRIPNO , " _
                   & "       isnull(rtrim(A.DROPNO),'')         as DROPNO , " _
                   & "       isnull(rtrim(A.JISSKIKBN),'')      as JISSKIKBN , " _
                   & "       ''                                 as JISSKIKBNNAMES , " _
                   & "       isnull(rtrim(A.URIKBN),'')         as URIKBN , " _
                   & "       isnull(rtrim(F6.VALUE1),'')        as URIKBNNAMES , " _
                   & "       isnull(rtrim(A.TUMIOKIKBN),'')     as TUMIOKIKBN , " _
                   & "       isnull(rtrim(F5.VALUE1),'')        as TUMIOKIKBNNAMES , " _
                   & "       isnull(rtrim(A.STORICODE),'')      as STORICODE , " _
                   & "       isnull(rtrim(MC22.NAMES),'')       as STORICODENAMES , " _
                   & "       isnull(rtrim(A.CONTCHASSIS),'')    as CONTCHASSIS , " _
                   & "       ''                                 as CONTCHASSISLICNPLTNO , " _
                   & "       isnull(rtrim(A.SHARYOTYPEF),'')    as SHARYOTYPEF , " _
                   & "       isnull(rtrim(A.TSHABANF),'')       as TSHABANF , " _
                   & "       isnull(rtrim(A.SHARYOTYPEB),'')    as SHARYOTYPEB , " _
                   & "       isnull(rtrim(A.TSHABANB),'')       as TSHABANB , " _
                   & "       isnull(rtrim(A.SHARYOTYPEB2),'')   as SHARYOTYPEB2 , " _
                   & "       isnull(rtrim(A.TSHABANB2),'')      as TSHABANB2 , " _
                   & "       isnull(rtrim(A.TAXKBN),'')         as TAXKBN , " _
                   & "       isnull(rtrim(F7.VALUE1),'')        as TAXKBNNAMES , " _
                   & "       isnull(rtrim(A.LATITUDE),'')       as LATITUDE , " _
                   & "       isnull(rtrim(A.LONGITUDE),'')      as LONGITUDE , " _
                   & "       isnull(rtrim(A.L1HAISOGROUP),'')   as wHaisoGroup , " _
                   & "       isnull(rtrim(A.DELFLG),'0')        as DELFLG , " _
                   & "       ''                                 as HOLIDAYKBN , " _
                   & "       ''                                 as TORITYPE01 , " _
                   & "       ''                                 as TORITYPE02 , " _
                   & "       ''                                 as TORITYPE03 , " _
                   & "       ''                                 as TORITYPE04 , " _
                   & "       ''                                 as TORITYPE05 , " _
                   & "       ''                                 as SUPPLIERKBN , " _
                   & "       ''                                 as SUPPLIER , " _
                   & "       ''                                 as MANGOILTYPE , " _
                   & "       ''                                 as MANGMORG1 , " _
                   & "       ''                                 as MANGSORG1 , " _
                   & "       ''                                 as MANGUORG1 , " _
                   & "       ''                                 as BASELEASE1 , " _
                   & "       ''                                 as MANGMORG2 , " _
                   & "       ''                                 as MANGSORG2 , " _
                   & "       ''                                 as MANGUORG2 , " _
                   & "       ''                                 as BASELEASE2 , " _
                   & "       ''                                 as MANGMORG3 , " _
                   & "       ''                                 as MANGSORG3 , " _
                   & "       ''                                 as MANGUORG3 , " _
                   & "       ''                                 as BASELEASE3 , " _
                   & "       ''                                 as STAFFKBN , " _
                   & "       ''                                 as MORG , " _
                   & "       ''                                 as HORG , " _
                   & "       ''                                 as SUBSTAFFKBN , " _
                   & "       ''                                 as SUBMORG , " _
                   & "       ''                                 as SUBHORG , " _
                   & "       ''                                 as ORDERORG  " _
                   & " FROM      T0005_NIPPO A " _
                   & " LEFT JOIN MB001_STAFF B " _
                   & "   ON    B.CAMPCODE    = A.CAMPCODE " _
                   & "   and   B.STAFFCODE   = A.STAFFCODE " _
                   & "   and   B.STYMD      <= A.YMD " _
                   & "   and   B.ENDYMD     >= A.YMD " _
                   & "   and   B.STYMD       = ( " _
                   & "    SELECT MAX(STYMD)  " _
                   & "    FROM     MB001_STAFF    B2 " _
                   & "    WHERE B2.CAMPCODE = A.CAMPCODE and B2.STAFFCODE = A.STAFFCODE and B2.STYMD <= A.YMD and B2.ENDYMD >= A.YMD and DELFLG <> '1' ) " _
                   & "   and   B.DELFLG     <> '1' " _
                   & " LEFT JOIN MB001_STAFF B2 " _
                   & "   ON    B2.CAMPCODE    = @P1 " _
                   & "   and   B2.STAFFCODE   = A.SUBSTAFFCODE " _
                   & "   and   B2.STYMD      <= A.YMD " _
                   & "   and   B2.ENDYMD     >= A.YMD " _
                   & "   and   B2.STYMD       = (SELECT MAX(STYMD) FROM MB001_STAFF WHERE CAMPCODE = @P1 and STAFFCODE = A.SUBSTAFFCODE and STYMD <= A.YMD and ENDYMD >= A.YMD and DELFLG <> '1' ) " _
                   & "   and   B2.DELFLG     <> '1' " _
                   & " LEFT JOIN M0001_CAMP M1 " _
                   & "   ON    M1.CAMPCODE    = @P1 " _
                   & "   and   M1.STYMD      <= A.YMD " _
                   & "   and   M1.ENDYMD     >= A.YMD " _
                   & "   and   M1.STYMD       = (SELECT MAX(STYMD) FROM M0001_CAMP WHERE CAMPCODE = @P1 and STYMD <= A.YMD and ENDYMD >= A.YMD and DELFLG <> '1' )" _
                   & "   and   M1.DELFLG     <> '1' " _
                   & " LEFT JOIN M0002_ORG M2 " _
                   & "   ON    M2.CAMPCODE    = @P1 " _
                   & "   and   M2.ORGCODE     = A.SHIPORG " _
                   & "   and   M2.STYMD      <= A.YMD " _
                   & "   and   M2.ENDYMD     >= A.YMD " _
                   & "   and   M2.STYMD       = (SELECT MAX(STYMD) FROM M0002_ORG WHERE CAMPCODE = @P1 and ORGCODE = A.SHIPORG and STYMD <= A.YMD and ENDYMD >= A.YMD and DELFLG <> '1' )" _
                   & "   and   M2.DELFLG     <> '1' " _
                   & " LEFT JOIN MC002_TORIHIKISAKI MC2 " _
                   & "   ON    MC2.TORICODE    = A.TORICODE " _
                   & "   and   MC2.CAMPCODE    = @P1 " _
                   & "   and   MC2.STYMD      <= A.YMD " _
                   & "   and   MC2.ENDYMD     >= A.YMD " _
                   & "   and   MC2.STYMD       = (SELECT MAX(STYMD) FROM MC002_TORIHIKISAKI WHERE TORICODE = A.TORICODE and STYMD <= A.YMD and ENDYMD >= A.YMD and DELFLG <> '1' )" _
                   & "   and   MC2.DELFLG     <> '1' " _
                   & " LEFT JOIN MC002_TORIHIKISAKI MC22 " _
                   & "   ON    MC22.TORICODE    = A.STORICODE " _
                   & "   and   MC22.CAMPCODE    = @P1 " _
                   & "   and   MC22.STYMD      <= A.YMD " _
                   & "   and   MC22.ENDYMD     >= A.YMD " _
                   & "   and   MC22.STYMD       = (SELECT MAX(STYMD) FROM MC002_TORIHIKISAKI WHERE TORICODE = A.STORICODE and STYMD <= A.YMD and ENDYMD >= A.YMD and DELFLG <> '1' )" _
                   & "   and   MC22.DELFLG     <> '1' " _
                   & " LEFT JOIN MC006_TODOKESAKI MC6 " _
                   & "   ON    MC6.CAMPCODE    = A.CAMPCODE " _
                   & "   and   MC6.TORICODE    = A.TORICODE " _
                   & "   and   MC6.TODOKECODE  = A.TODOKECODE " _
                   & "   and   MC6.CLASS       = '1' " _
                   & "   and   MC6.STYMD      <= A.YMD " _
                   & "   and   MC6.ENDYMD     >= A.YMD " _
                   & "   and   MC6.STYMD       = (SELECT MAX(STYMD) FROM MC006_TODOKESAKI WHERE CAMPCODE = A.CAMPCODE and TORICODE = A.TORICODE and TODOKECODE = A.TODOKECODE and CLASS = '1' and STYMD <= A.YMD and ENDYMD >= A.YMD and DELFLG <> '1' )" _
                   & "   and   MC6.DELFLG     <> '1' " _
                   & " LEFT JOIN MC006_TODOKESAKI MC62 " _
                   & "   ON    MC62.CAMPCODE    = A.CAMPCODE " _
                   & "   and   MC62.TODOKECODE  = A.SHUKABASHO " _
                   & "   and   MC62.CLASS       = '2' " _
                   & "   and   MC62.STYMD      <= A.YMD " _
                   & "   and   MC62.ENDYMD     >= A.YMD " _
                   & "   and   MC62.STYMD       = (SELECT MAX(STYMD) FROM MC006_TODOKESAKI WHERE CAMPCODE = A.CAMPCODE and TODOKECODE = A.SHUKABASHO and CLASS = '2' and STYMD <= A.YMD and ENDYMD >= A.YMD and DELFLG <> '1' )" _
                   & "   and   MC62.DELFLG     <> '1' " _
                   & " LEFT JOIN MC001_FIXVALUE F1 " _
                   & "   ON    F1.CAMPCODE    = @P1 " _
                   & "   and   F1.CLASS       = 'TERMKBN' " _
                   & "   and   F1.KEYCODE     = A.TERMKBN " _
                   & "   and   F1.STYMD      <= A.YMD " _
                   & "   and   F1.ENDYMD     >= A.YMD " _
                   & "   and   F1.DELFLG     <> '1' " _
                   & " LEFT JOIN MC001_FIXVALUE F2 " _
                   & "   ON    F2.CAMPCODE    = @P1 " _
                   & "   and   F2.CLASS       = 'WORKKBN' " _
                   & "   and   F2.KEYCODE     = A.WORKKBN " _
                   & "   and   F2.STYMD      <= A.YMD " _
                   & "   and   F2.ENDYMD     >= A.YMD " _
                   & "   and   F2.DELFLG     <> '1' " _
                   & " LEFT JOIN MC001_FIXVALUE F3 " _
                   & "   ON    F3.CAMPCODE    = @P1 " _
                   & "   and   F3.CLASS       = 'CREWKBN' " _
                   & "   and   F3.KEYCODE     = A.CREWKBN " _
                   & "   and   F3.STYMD      <= A.YMD " _
                   & "   and   F3.ENDYMD     >= A.YMD " _
                   & "   and   F3.DELFLG     <> '1' " _
                   & " LEFT JOIN MC001_FIXVALUE F41 " _
                   & "   ON    F41.CAMPCODE    = @P1 " _
                   & "   and   F41.CLASS       = 'STANI' " _
                   & "   and   F41.KEYCODE     = A.STANI1 " _
                   & "   and   F41.STYMD      <= A.YMD " _
                   & "   and   F41.ENDYMD     >= A.YMD " _
                   & "   and   F41.DELFLG     <> '1' " _
                   & " LEFT JOIN MC001_FIXVALUE F42 " _
                   & "   ON    F42.CAMPCODE    = @P1 " _
                   & "   and   F42.CLASS       = 'STANI' " _
                   & "   and   F42.KEYCODE     = A.STANI2 " _
                   & "   and   F42.STYMD      <= A.YMD " _
                   & "   and   F42.ENDYMD     >= A.YMD " _
                   & "   and   F42.DELFLG     <> '1' " _
                   & " LEFT JOIN MC001_FIXVALUE F43 " _
                   & "   ON    F43.CAMPCODE    = @P1 " _
                   & "   and   F43.CLASS       = 'STANI' " _
                   & "   and   F43.KEYCODE     = A.STANI3 " _
                   & "   and   F43.STYMD      <= A.YMD " _
                   & "   and   F43.ENDYMD     >= A.YMD " _
                   & "   and   F43.DELFLG     <> '1' " _
                   & " LEFT JOIN MC001_FIXVALUE F44 " _
                   & "   ON    F44.CAMPCODE    = @P1 " _
                   & "   and   F44.CLASS       = 'STANI' " _
                   & "   and   F44.KEYCODE     = A.STANI4 " _
                   & "   and   F44.STYMD      <= A.YMD " _
                   & "   and   F44.ENDYMD     >= A.YMD " _
                   & "   and   F44.DELFLG     <> '1' " _
                   & " LEFT JOIN MC001_FIXVALUE F45 " _
                   & "   ON    F45.CAMPCODE    = @P1 " _
                   & "   and   F45.CLASS       = 'STANI' " _
                   & "   and   F45.KEYCODE     = A.STANI5 " _
                   & "   and   F45.STYMD      <= A.YMD " _
                   & "   and   F45.ENDYMD     >= A.YMD " _
                   & "   and   F45.DELFLG     <> '1' " _
                   & " LEFT JOIN MC001_FIXVALUE F46 " _
                   & "   ON    F46.CAMPCODE    = @P1 " _
                   & "   and   F46.CLASS       = 'STANI' " _
                   & "   and   F46.KEYCODE     = A.STANI6 " _
                   & "   and   F46.STYMD      <= A.YMD " _
                   & "   and   F46.ENDYMD     >= A.YMD " _
                   & "   and   F46.DELFLG     <> '1' " _
                   & " LEFT JOIN MC001_FIXVALUE F47 " _
                   & "   ON    F47.CAMPCODE    = @P1 " _
                   & "   and   F47.CLASS       = 'STANI' " _
                   & "   and   F47.KEYCODE     = A.STANI7 " _
                   & "   and   F47.STYMD      <= A.YMD " _
                   & "   and   F47.ENDYMD     >= A.YMD " _
                   & "   and   F47.DELFLG     <> '1' " _
                   & " LEFT JOIN MC001_FIXVALUE F48 " _
                   & "   ON    F48.CAMPCODE    = @P1 " _
                   & "   and   F48.CLASS       = 'STANI' " _
                   & "   and   F48.KEYCODE     = A.STANI8 " _
                   & "   and   F48.STYMD      <= A.YMD " _
                   & "   and   F48.ENDYMD     >= A.YMD " _
                   & "   and   F48.DELFLG     <> '1' " _
                   & " LEFT JOIN MC001_FIXVALUE F5 " _
                   & "   ON    F5.CAMPCODE    = @P1 " _
                   & "   and   F5.CLASS       = 'TUMIOKIKBN' " _
                   & "   and   F5.KEYCODE     = A.TUMIOKIKBN " _
                   & "   and   F5.STYMD      <= A.YMD " _
                   & "   and   F5.ENDYMD     >= A.YMD " _
                   & "   and   F5.DELFLG     <> '1' " _
                   & " LEFT JOIN MC001_FIXVALUE F6 " _
                   & "   ON    F6.CAMPCODE    = @P1 " _
                   & "   and   F6.CLASS       = 'URIKBN' " _
                   & "   and   F6.KEYCODE     = A.URIKBN " _
                   & "   and   F6.STYMD      <= A.YMD " _
                   & "   and   F6.ENDYMD     >= A.YMD " _
                   & "   and   F6.DELFLG     <> '1' " _
                   & " LEFT JOIN MC001_FIXVALUE F7 " _
                   & "   ON    F7.CAMPCODE    = @P1 " _
                   & "   and   F7.CLASS       = 'TAXKBN' " _
                   & "   and   F7.KEYCODE     = A.TAXKBN " _
                   & "   and   F7.STYMD      <= A.YMD " _
                   & "   and   F7.ENDYMD     >= A.YMD " _
                   & "   and   F7.DELFLG     <> '1' " _
                   & " WHERE   " _
                   & "         A.CAMPCODE    = @P1 " _
                   & "   and   A.SHIPORG     = @P2 " _
                   & "   and   A.YMD        <= @P4 " _
                   & "   and   A.YMD        >= @P3 " _
                   & "   and   A.DELFLG     <> '1' "

                Dim SQLWhere As String = ""
                If Not String.IsNullOrEmpty(work.WF_SEL_STAFFCODE.Text) Then
                    SQLWhere = SQLWhere & " and A.STAFFCODE = '" & Trim(work.WF_SEL_STAFFCODE.Text) & "' "
                End If
                If Not String.IsNullOrEmpty(work.WF_SEL_STAFFNAME.Text) Then
                    SQLWhere = SQLWhere & " and B.STAFFNAMES like '%" & Trim(work.WF_SEL_STAFFNAME.Text) & "%' "
                End If
                If WW_SORT = "" OrElse String.IsNullOrEmpty(WW_SORT) Then
                    WW_SORT = "ORDER BY A.YMD , A.STAFFCODE , A.STDATE , A.STTIME"
                Else
                    WW_SORT = "ORDER BY " & WW_SORT
                End If

                Dim SQLStr2 As String = SQLStr & SQLWhere & WW_SORT
                Using SQLcmd As New SqlCommand(SQLStr2, SQLcon)
                    Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.NVarChar, 20)
                    Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.NVarChar, 20)
                    Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.Date)
                    Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", System.Data.SqlDbType.Date)

                    PARA1.Value = work.WF_SEL_CAMPCODE.Text
                    PARA2.Value = work.WF_SEL_UORG.Text
                    PARA3.Value = work.WF_SEL_STYMD.Text
                    PARA4.Value = work.WF_SEL_ENDYMD.Text
                    SQLcmd.CommandTimeout = 300
                    '----------------------------
                    '画面指定の開始日付～終了日付を取得
                    '----------------------------
                    Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

                        '〇データをテーブルに設定
                        T0012tbl.Load(SQLdr)
                        If T0012tbl.Rows.Count > 65000 Then
                            'データ取得件数が65,000件を超えたため表示できません。選択条件を変更して下さい。
                            Master.Output(C_MESSAGE_NO.DISPLAY_RECORD_OVER, C_MESSAGE_TYPE.ABORT)
                            T0012tbl.Clear()
                            Exit Sub
                        End If

                    End Using

                End Using

                '----------------------------
                '一週間前の日報を取得
                '----------------------------
                WW_SORT = "ORDER BY A.YMD , A.STAFFCODE , A.STDATE , A.STTIME"

                SQLStr2 = SQLStr & WW_SORT
                Using SQLcmd2 As New SqlCommand(SQLStr2, SQLcon)
                    Dim PARA21 As SqlParameter = SQLcmd2.Parameters.Add("@P1", System.Data.SqlDbType.NVarChar, 20)
                    Dim PARA22 As SqlParameter = SQLcmd2.Parameters.Add("@P2", System.Data.SqlDbType.NVarChar, 20)
                    Dim PARA23 As SqlParameter = SQLcmd2.Parameters.Add("@P3", System.Data.SqlDbType.Date)
                    Dim PARA24 As SqlParameter = SQLcmd2.Parameters.Add("@P4", System.Data.SqlDbType.Date)
                    Dim PARA25 As SqlParameter = SQLcmd2.Parameters.Add("@P5", System.Data.SqlDbType.Date)

                    Dim WW_date As Date = Date.Parse(work.WF_SEL_STYMD.Text)
                    ' 一週間前
                    Dim WW_Fdate As String = WW_date.AddDays(-7).ToString("yyyy/MM/dd")
                    Dim WW_Tdate As String = WW_date.AddDays(-1).ToString("yyyy/MM/dd")

                    PARA21.Value = work.WF_SEL_CAMPCODE.Text
                    PARA22.Value = work.WF_SEL_UORG.Text
                    PARA23.Value = WW_Fdate
                    PARA24.Value = WW_Tdate
                    PARA25.Value = Date.Now
                    SQLcmd2.CommandTimeout = 300
                    Using SQLdr2 As SqlDataReader = SQLcmd2.ExecuteReader()

                        '■テーブル検索結果をテーブル退避
                        '日報DB更新用テーブル
                        T0005COM.AddColumnT0005tbl(T0012WEEKtbl)

                        T0012WEEKtbl.Load(SQLdr2)

                        '一週間前～開始日付－１日をマージ
                        T0012tbl.Merge(T0012WEEKtbl)
                    End Using
                End Using
            End Using

            Using WW_T0012tbl As DataTable = T0012tbl.Clone

                For i As Integer = 0 To T0012tbl.Rows.Count - 1
                    Dim T0012row As DataRow = WW_T0012tbl.NewRow
                    T0012row.ItemArray = T0012tbl.Rows(i).ItemArray

                    If IsDate(T0012row("YMD")) Then
                        T0012row("YMD") = CDate(T0012row("YMD")).ToString("yyyy/MM/dd")
                    Else
                        T0012row("YMD") = C_DEFAULT_YMD
                    End If

                    T0012row("SELECT") = "1"      '対象データ
                    T0012row("HIDDEN") = "1"      '非表示

                    T0012row("HDKBN") = "D"       'ヘッダ、明細区分
                    If IsDate(T0012row("SHUKADATE")) Then
                        T0012row("SHUKADATE") = CDate(T0012row("SHUKADATE")).ToString("yyyy/MM/dd")
                    End If
                    If IsDate(T0012row("TODOKEDATE")) Then
                        T0012row("TODOKEDATE") = CDate(T0012row("TODOKEDATE")).ToString("yyyy/MM/dd")
                    End If
                    T0012row("SEQ") = CInt(T0012row("SEQ")).ToString("000")
                    If IsDate(T0012row("STDATE")) Then
                        T0012row("STDATE") = CDate(T0012row("STDATE")).ToString("yyyy/MM/dd")
                    Else
                        T0012row("STDATE") = C_DEFAULT_YMD
                    End If
                    If IsDate(T0012row("STTIME")) Then
                        T0012row("STTIME") = CDate(T0012row("STTIME")).ToString("HH:mm")
                    Else
                        T0012row("STTIME") = "00:00"
                    End If
                    If IsDate(T0012row("ENDDATE")) Then
                        T0012row("ENDDATE") = CDate(T0012row("ENDDATE")).ToString("yyyy/MM/dd")
                    Else
                        T0012row("ENDDATE") = C_DEFAULT_YMD
                    End If
                    If IsDate(T0012row("ENDTIME")) Then
                        T0012row("ENDTIME") = CDate(T0012row("ENDTIME")).ToString("HH:mm")
                    Else
                        T0012row("ENDTIME") = "00:00"
                    End If

                    T0012row("WORKTIME") = T0005COM.MinutesToHHMM(T0012row("WORKTIME"))
                    T0012row("MOVETIME") = T0005COM.MinutesToHHMM(T0012row("MOVETIME"))
                    T0012row("ACTTIME") = T0005COM.MinutesToHHMM(T0012row("ACTTIME"))

                    WW_T0012tbl.Rows.Add(T0012row)
                Next

                T0012tbl = WW_T0012tbl.Copy

            End Using

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "T0005_NIPPO SELECT")
            CS0011LOGWRITE.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:T0005_NIPPO Select"      '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try

        'ヘッダ作成
        T0012_CreHead(T0012tbl)

        '○GridViewデータをテーブルに保存
        If Not Master.SaveTable(T0012tbl) Then Exit Sub

        '絞込みボタン処理（GridViewの表示）を行う
        WF_ButtonExtract_Click()

    End Sub

    ''' <summary>
    ''' ヘッダーレコード作成
    ''' </summary>
    ''' <param name="IO_TBL"></param>
    ''' <remarks></remarks>
    Protected Sub T0012_CreHead(ByRef IO_TBL As DataTable)

        Dim WW_IDX As Integer = 0
        Dim WW_RTN As String = ""
        Dim WW_T0012row As DataRow

        Try
            '出庫日、乗務員でグループ化しキーテーブル作成
            Dim staffs = IO_TBL.AsEnumerable.
                            Where(Function(x) x.Item("YMD") >= work.WF_SEL_STYMD.Text).
                            OrderBy(Function(x) x.Item("YMD")).
                            ThenBy(Function(x) x.Item("STAFFCODE")).
                            ThenBy(Function(x) x.Item("STDATE")).
                            ThenBy(Function(x) x.Item("STTIME")).
                            ThenBy(Function(x) x.Item("ENDDATE")).
                            ThenBy(Function(x) x.Item("ENDTIME")).
                            ThenBy(Function(x) x.Item("WORKKBN")).
                            ThenBy(Function(x) x.Item("SEQ")).
                            GroupBy(Function(x) New With {Key .YMD = x.Item("YMD"), Key .STAFFCODE = x.Item("STAFFCODE")})
            For Each staff In staffs
                '始業・終業レコード取得　※複数存在を考慮し[A1]は初回、[Z1]は最終
                Dim a1 = staff.First(Function(x) x.Item("WORKKBN") = "A1")
                Dim z1 = staff.Last(Function(x) x.Item("WORKKBN") = "Z1")

                WW_T0012row = IO_TBL.NewRow
                T0005COM.InitialT5INPRow(WW_T0012row)
                WW_IDX = WW_IDX + 1

                'ヘッダー項目
                WW_T0012row("CAMPCODE") = work.WF_SEL_CAMPCODE.Text
                WW_T0012row("SHIPORG") = work.WF_SEL_UORG.Text
                WW_T0012row("YMD") = staff.Key.YMD
                WW_T0012row("STAFFCODE") = staff.Key.STAFFCODE
                WW_T0012row("LINECNT") = WW_IDX
                WW_T0012row("SELECT") = "1"
                WW_T0012row("HIDDEN") = "0"
                WW_T0012row("HDKBN") = "H"
                WW_T0012row("SEQ") = "001"

                WW_T0012row("CAMPNAMES") = a1.Item("CAMPNAMES")
                WW_T0012row("SHIPORGNAMES") = a1.Item("SHIPORGNAMES")
                WW_T0012row("STAFFNAMES") = a1.Item("STAFFNAMES")
                WW_T0012row("CREWKBN") = a1.Item("CREWKBN")
                WW_T0012row("CREWKBNNAMES") = a1.Item("CREWKBNNAMES")
                '開始日、開始時間を取得
                WW_T0012row("STDATE") = a1.Item("STDATE")
                WW_T0012row("STTIME") = a1.Item("STTIME")
                '終了日、終了時間
                WW_T0012row("ENDDATE") = z1.Item("ENDDATE")
                WW_T0012row("ENDTIME") = z1.Item("ENDTIME")

                '変更前開始日・開始時間、終了日、終了時間を別項目に格納
                WW_T0012row("WORKTIME") = a1.Item("STDATE") & " " & a1.Item("STTIME")
                WW_T0012row("MOVETIME") = z1.Item("ENDDATE") & " " & z1.Item("ENDTIME")

                WW_T0012row("TIMSTP") = a1.Item("TIMSTP")
                WW_T0012row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                WW_T0012row("DELFLG") = C_DELETE_FLG.ALIVE


                IO_TBL.Rows.Add(WW_T0012row)
            Next

        Catch ex As Exception
            CS0011LOGWRITE.INFSUBCLASS = "T0012_CreHead"                'SUBクラス名
            CS0011LOGWRITE.INFPOSI = ""                                 '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.SYSTEM_ADM_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            Exit Sub

        End Try

    End Sub

    ''' <summary>
    ''' ヘッダー情報の反映
    ''' </summary>
    ''' <param name="IO_TBL"></param>
    Protected Sub T0012_HeadToDetail(ByRef IO_TBL As DataTable)

        Dim WW_UPDTBL As DataTable = IO_TBL.Clone

        '出庫日、乗務員でグループ化しキーテーブル作成
        '但し、ヘッダーレコードに更新が発生したグループのみ
        Dim staffs = IO_TBL.AsEnumerable.
                            OrderBy(Function(x) x.Item("YMD")).
                            ThenBy(Function(x) x.Item("STAFFCODE")).
                            ThenBy(Function(x) x.Item("STDATE")).
                            ThenBy(Function(x) x.Item("STTIME")).
                            ThenBy(Function(x) x.Item("ENDDATE")).
                            ThenBy(Function(x) x.Item("ENDTIME")).
                            ThenBy(Function(x) x.Item("WORKKBN")).
                            ThenBy(Function(x) x.Item("SEQ")).
                            GroupBy(Function(x) New With {Key .YMD = x.Item("YMD"), Key .STAFFCODE = x.Item("STAFFCODE")}).
                            Where(Function(g) g.Any(Function(x) x.Item("OPERATION") = C_LIST_OPERATION_CODE.UPDATING))
        For Each staff In staffs
            Dim head = staff.First(Function(x) x.Item("HDKBN") = "H")
            If head.Item("OPERATION") = C_LIST_OPERATION_CODE.UPDATING Then
                For Each rec In staff.Where(Function(x) x("HDKBN") = "D")
                    Dim newRec = WW_UPDTBL.NewRow
                    newRec.ItemArray = rec.ItemArray

                    '既存行の削除
                    rec("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                    rec("TIMSTP") = head("TIMSTP")
                    rec("DELFLG") = C_DELETE_FLG.DELETE
                    rec("SELECT") = "0"
                    rec("LINECNT") = 0
                    rec("HIDDEN") = "1"
                    '新規行の追加
                    newRec("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                    newRec("TIMSTP") = "0"
                    newRec("DELFLG") = C_DELETE_FLG.ALIVE
                    If rec("WORKKBN") = "A1" Then
                        newRec("STDATE") = head("STDATE")
                        newRec("STTIME") = head("STTIME")
                        newRec("ENDDATE") = head("STDATE")
                        newRec("ENDTIME") = head("STTIME")
                    ElseIf rec("WORKKBN") = "Z1" Then
                        'Z1
                        newRec("STDATE") = head("ENDDATE")
                        newRec("STTIME") = head("ENDTIME")
                        newRec("ENDDATE") = head("ENDDATE")
                        newRec("ENDTIME") = head("ENDTIME")
                    End If
                    WW_UPDTBL.Rows.Add(newRec)
                Next
            End If
        Next

        '〇追加情報をマージ
        IO_TBL.Merge(WW_UPDTBL)
    End Sub

    ''' <summary>
    ''' エラーレポート編集
    ''' </summary>
    ''' <param name="I_MESSAGE1"></param>
    ''' <param name="I_MESSAGE2"></param>
    ''' <param name="WW_LINEerr"></param>
    ''' <param name="T0012INProw"></param>
    ''' <param name="I_ERRCD"></param>
    ''' <remarks></remarks>
    Protected Sub ERRMSG_write(ByRef I_MESSAGE1 As String, ByRef I_MESSAGE2 As String, ByRef WW_LINEerr As String, ByRef T0012INProw As DataRow, ByVal I_ERRCD As String)

        Dim WW_ERR_MES As String = ""
        WW_ERR_MES = I_MESSAGE1
        If I_MESSAGE2 <> "" Then
            WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> " & I_MESSAGE2 & " , "
        End If
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 項番        =@L" & T0012INProw("YMD") & T0012INProw("STAFFCODE") & "L@ , "
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 出庫日      =" & T0012INProw("YMD") & " , "
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 乗務員コード=" & T0012INProw("STAFFCODE") & " , "
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 乗務員      =" & T0012INProw("STAFFNAMES") & " , "
        ErrMsgSet(WW_ERR_MES)
        If WW_LINEerr <> C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR Then
            WW_LINEerr = I_ERRCD
        End If

    End Sub

    '★★★★★★★★★★★★★★★★★★★★★
    'EXCEL取込み処理
    '★★★★★★★★★★★★★★★★★★★★★

    ''' <summary>
    ''' EXCELファイルアップロード入力処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub UPLOAD_EXCEL()

        T0005COM.AddColumnT0005tbl(T0012INPtbl)

        '■■■ UPLOAD_XLSデータ取得 ■■■ 
        CS0023XLSTBL.MAPID = Master.MAPID
        CS0023XLSTBL.CAMPCODE = work.WF_SEL_CAMPCODE.Text
        CS0023XLSTBL.CS0023XLSUPLOAD(String.Empty, Master.PROF_REPORT)
        If isNormal(CS0023XLSTBL.ERR) Then
            If CS0023XLSTBL.TBLDATA.Rows.Count = 0 Then
                Master.Output(C_MESSAGE_NO.REGISTRATION_RECORD_NOT_EXIST_ERROR, C_MESSAGE_TYPE.ERR, "例外発生")

                Exit Sub
            End If
        Else
            Master.Output(CS0023XLSTBL.ERR, C_MESSAGE_TYPE.ERR, "CS0023XLSTBL")

            Exit Sub
        End If

        'EXCELデータの初期化（DBNullを撲滅）
        Dim CS0023XLSTBLrow As DataRow = CS0023XLSTBL.TBLDATA.NewRow
        For i As Integer = 0 To CS0023XLSTBL.TBLDATA.Rows.Count - 1
            CS0023XLSTBLrow.ItemArray = CS0023XLSTBL.TBLDATA.Rows(i).ItemArray

            For j As Integer = 0 To CS0023XLSTBL.TBLDATA.Columns.Count - 1
                If IsDBNull(CS0023XLSTBLrow.Item(j)) OrElse IsNothing(CS0023XLSTBLrow.Item(j)) Then
                    CS0023XLSTBLrow.Item(j) = ""
                End If
            Next
            CS0023XLSTBL.TBLDATA.Rows(i).ItemArray = CS0023XLSTBLrow.ItemArray
        Next

        '○CS0023XLSTBL.TBLDATAの入力値整備
        Dim WW_COLUMNS As New List(Of String)
        For i As Integer = 0 To CS0023XLSTBL.TBLDATA.Columns.Count - 1
            WW_COLUMNS.Add(CS0023XLSTBL.TBLDATA.Columns.Item(i).ColumnName.ToString)
        Next

        '■■■ エラーレポート準備 ■■■
        Dim WW_RTN As String = ""
        Dim WW_DATE As Date

        '○ 初期処理
        rightview.SetErrorReport("")

        '○T0012INPtblカラム設定
        T0005COM.AddColumnT0005tbl(T0012INPtbl)

        Dim WW_TEXT As String = ""
        Dim WW_VALUE As String = ""

        '■■■ Excelデータ毎にチェック＆更新 ■■■
        For i As Integer = 0 To CS0023XLSTBL.TBLDATA.Rows.Count - 1

            '○XLSTBL明細⇒T0012INProw
            Dim T0012INProw As DataRow = T0012INPtbl.NewRow
            '○初期クリア
            T0005COM.InitialT5INPRow(T0012INProw)

            T0012INProw("LINECNT") = 0
            T0012INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
            T0012INProw("TIMSTP") = "0"
            T0012INProw("SELECT") = 1
            T0012INProw("HIDDEN") = 1

            If WW_COLUMNS.IndexOf("CAMPCODE") >= 0 Then
                T0012INProw("CAMPCODE") = work.WF_SEL_CAMPCODE.Text
            Else
                T0012INProw("CAMPCODE") = CS0023XLSTBL.TBLDATA.Rows(i)("CAMPCODE").PadLeft(2, "0"c)
                '名称付与
                WW_TEXT = ""
                CODENAME_get("CAMPCODE", T0012INProw("CAMPCODE"), WW_TEXT, WW_RTN)
                T0012INProw("CAMPNAMES") = WW_TEXT
            End If

            If WW_COLUMNS.IndexOf("SHIPORG") < 0 Then
                T0012INProw("SHIPORG") = work.WF_SEL_UORG.Text
                WW_TEXT = ""
                CODENAME_get("SHIPORG", T0012INProw("SHIPORG"), WW_TEXT, WW_RTN)
                T0012INProw("SHIPORGNAMES") = WW_TEXT
            Else
                T0012INProw("SHIPORG") = CS0023XLSTBL.TBLDATA.Rows(i)("SHIPORG")
                '名称付与
                WW_TEXT = ""
                CODENAME_get("SHIPORG", T0012INProw("SHIPORG"), WW_TEXT, WW_RTN)
                T0012INProw("SHIPORGNAMES") = WW_TEXT
            End If

            If WW_COLUMNS.IndexOf("YMD") >= 0 Then
                If IsDate(CS0023XLSTBL.TBLDATA.Rows(i)("YMD")) Then
                    WW_DATE = CS0023XLSTBL.TBLDATA.Rows(i)("YMD")
                    T0012INProw("YMD") = WW_DATE.ToString("yyyy/MM/dd")
                Else
                    T0012INProw("YMD") = ""
                End If
            End If

            T0012INProw("HDKBN") = "H"

            If WW_COLUMNS.IndexOf("SEQ") >= 0 Then
                If IsDBNull(CS0023XLSTBL.TBLDATA.Rows(i)("SEQ")) Then
                    T0012INProw("SEQ") = ""
                Else
                    T0012INProw("SEQ") = CS0023XLSTBL.TBLDATA.Rows(i)("SEQ")
                End If
            End If

            If WW_COLUMNS.IndexOf("STAFFCODE") >= 0 Then
                T0012INProw("STAFFCODE") = CS0023XLSTBL.TBLDATA.Rows(i)("STAFFCODE")
                '名称付与
                CODENAME_get("STAFFCODE", T0012INProw("STAFFCODE"), WW_TEXT, WW_RTN)
                T0012INProw("STAFFNAMES") = WW_TEXT
            End If


            If WW_COLUMNS.IndexOf("STDATE") >= 0 Then
                If IsDate(CS0023XLSTBL.TBLDATA.Rows(i)("STDATE")) Then
                    WW_DATE = CS0023XLSTBL.TBLDATA.Rows(i)("STDATE")
                    T0012INProw("STDATE") = WW_DATE.ToString("yyyy/MM/dd")
                Else
                    T0012INProw("STDATE") = ""
                End If
            End If

            If WW_COLUMNS.IndexOf("STTIME") >= 0 Then
                If IsDate(CS0023XLSTBL.TBLDATA.Rows(i)("STTIME")) Then
                    WW_DATE = CS0023XLSTBL.TBLDATA.Rows(i)("STTIME")
                    T0012INProw("STTIME") = WW_DATE.ToString("HH:mm")
                Else
                    T0012INProw("STTIME") = ""
                End If
            End If

            If WW_COLUMNS.IndexOf("ENDDATE") >= 0 Then
                If IsDate(CS0023XLSTBL.TBLDATA.Rows(i)("ENDDATE")) Then
                    WW_DATE = CS0023XLSTBL.TBLDATA.Rows(i)("ENDDATE")
                    T0012INProw("ENDDATE") = WW_DATE.ToString("yyyy/MM/dd")
                Else
                    T0012INProw("ENDDATE") = ""
                End If
            End If

            If WW_COLUMNS.IndexOf("ENDTIME") >= 0 Then
                If IsDate(CS0023XLSTBL.TBLDATA.Rows(i)("ENDTIME")) Then
                    WW_DATE = CS0023XLSTBL.TBLDATA.Rows(i)("ENDTIME")
                    T0012INProw("ENDTIME") = WW_DATE.ToString("HH:mm")
                Else
                    T0012INProw("ENDTIME") = ""
                End If
            End If

            T0012INPtbl.Rows.Add(T0012INProw)
        Next

        '■■■ GridView更新 ■■■
        Grid_UpdateExcel(WW_ERRCODE)

        '○メッセージ表示
        If isNormal(WW_ERRCODE) Then
            Master.Output(C_MESSAGE_NO.NORMAL, C_MESSAGE_TYPE.NOR)
        Else
            Master.Output(WW_ERRCODE, C_MESSAGE_TYPE.ERR)
        End If

        '■■■ 画面終了 ■■■

        'Close
        CS0023XLSTBL.TBLDATA.Dispose()
        CS0023XLSTBL.TBLDATA.Clear()

        'カーソル設定
        WF_FIELD.Value = "WF_STAFFCODE"
        WF_STAFFCODE.Focus()

    End Sub

    ''' <summary>
    '''  TODO:GridViewの更新（Excel）
    ''' </summary>
    ''' <param name="O_RTN">可否判定</param>
    ''' <remarks></remarks>
    Protected Sub Grid_UpdateExcel(ByRef O_RTN As String)

        Dim WW_UMU As Integer = 0

        Try
            O_RTN = C_MESSAGE_NO.NORMAL

            '○テーブルデータ 復元（GridView）
            'テーブルデータ 復元(TEXTファイルより復元)
            If Not Master.RecoverTable(T0012tbl) Then
                O_RTN = C_MESSAGE_NO.SYSTEM_ADM_ERROR
                Exit Sub
            End If

            CS0026TBLSORT.TABLE = T0012tbl
            CS0026TBLSORT.FILTER = ""
            CS0026TBLSORT.SORTING = "SELECT, YMD, STAFFCODE, HDKBN DESC, STDATE, STTIME, ENDDATE, ENDTIME, WORKKBN, SEQ"
            T0012tbl = CS0026TBLSORT.Sort()
            '-----------------------------------------------------------------------------------
            '差分データ（取込）とT0012tbl（GridView）を比較し、該当データが存在すれば上書き
            '存在しない場合はスルー
            '-----------------------------------------------------------------------------------
            For Each WW_INPRow As DataRow In T0012INPtbl.Rows
                For Each WW_Row As DataRow In T0012tbl.Rows
                    If WW_Row("HDKBN") <> "H" Then Continue For

                    '出庫日・乗務員・トリップ・ドロップ・車番
                    If WW_Row("YMD") = WW_INPRow("YMD") AndAlso
                       WW_Row("STAFFCODE") = WW_INPRow("STAFFCODE") AndAlso
                       WW_Row("SELECT") = "1" Then

                        '日時の反映
                        If WW_Row("STDATE") <> WW_INPRow("STDATE") Then
                            WW_Row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                            WW_Row("STDATE") = WW_INPRow("STDATE")
                        End If
                        If WW_Row("STTIME") <> WW_INPRow("STTIME") Then
                            WW_Row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                            WW_Row("STTIME") = WW_INPRow("STTIME")
                        End If
                        If WW_Row("ENDDATE") <> WW_INPRow("ENDDATE") Then
                            WW_Row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                            WW_Row("ENDDATE") = WW_INPRow("ENDDATE")
                        End If
                        If WW_Row("ENDTIME") <> WW_INPRow("ENDTIME") Then
                            WW_Row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                            WW_Row("ENDTIME") = WW_INPRow("ENDTIME")
                        End If
                        Exit For

                    End If
                Next
            Next

            '------------------------------------------------------------
            '■マージ後のチェック
            '------------------------------------------------------------
            T0012tbl_CheckHead(T0012tbl, O_RTN)
            If Not isNormal(O_RTN) Then
                'Exit Sub
            End If

            CS0026TBLSORT.TABLE = T0012tbl
            CS0026TBLSORT.FILTER = "HDKBN = 'H'"
            CS0026TBLSORT.SORTING = "YMD, STAFFCODE, HDKBN"
            Dim WW_T0012tbl As DataTable = CS0026TBLSORT.Sort()

            For i As Integer = 0 To WW_T0012tbl.Rows.Count - 1
                Dim WW_ERRWORD As String = ""
                WW_ERRWORD = rightview.GetErrorReport.Replace("@L" & WW_T0012tbl(i)("YMD") & WW_T0012tbl(i)("STAFFCODE") & "L@", WW_T0012tbl(i)("LINECNT"))
                rightview.SetErrorReport(WW_ERRWORD)
            Next

            '○GridViewデータをテーブルに保存
            If Not Master.SaveTable(T0012tbl) Then Exit Sub

            '絞込みボタン処理（GridViewの表示）を行う
            WF_ButtonExtract_Click()


        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "Grid_UpdateExcel")
            CS0011LOGWRITE.INFSUBCLASS = "Grid_Update"                  'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:EXCEL_IMPORT T0005_NIPPO"      '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            O_RTN = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try
    End Sub

    '★★★★★★★★★★★★★★★★★★★★★
    '共通処理処理
    '★★★★★★★★★★★★★★★★★★★★★

    ''' <summary>
    ''' ヘッダーチェック
    ''' </summary>
    ''' <param name="IO_TBL"></param>
    ''' <param name="O_RTN"></param>
    ''' <remarks></remarks>
    Protected Sub T0012tbl_CheckHead(ByRef IO_TBL As DataTable, ByRef O_RTN As String)

        Dim WW_LINEerr As String = C_MESSAGE_NO.NORMAL
        Dim WW_CheckMES1 As String = ""
        Dim WW_CheckMES2 As String = ""
        Dim WW_CHECKREPORT As String = String.Empty
        Dim WW_CHECKERR As String = ""

        '単項目チェック対象
        Dim checkTable As New List(Of String()) From
            {
                {New String() {"STDATE", "始業日付"}},
                {New String() {"STTIME", "始業時刻"}},
                {New String() {"ENDDATE", "終業日付"}},
                {New String() {"ENDTIME", "終業時刻"}}
            }

        O_RTN = C_MESSAGE_NO.NORMAL
        WW_ERRLIST = New List(Of String)
        WW_ERRLIST_ALL = New List(Of String)
        WW_ERRLISTCNT = 0

        Using S0013tbl As New DataTable

            '出庫日、乗務員でグループ化しキーテーブル作成
            '但し、ヘッダーレコードに更新が発生したグループのみ
            Dim staffs = IO_TBL.AsEnumerable.
                                OrderBy(Function(x) x.Item("YMD")).
                                ThenBy(Function(x) x.Item("STAFFCODE")).
                                ThenBy(Function(x) x.Item("STDATE")).
                                ThenBy(Function(x) x.Item("STTIME")).
                                ThenBy(Function(x) x.Item("ENDDATE")).
                                ThenBy(Function(x) x.Item("ENDTIME")).
                                ThenBy(Function(x) x.Item("WORKKBN")).
                                ThenBy(Function(x) x.Item("SEQ")).
                                GroupBy(Function(x) New With {Key .YMD = x.Item("YMD"), Key .STAFFCODE = x.Item("STAFFCODE")}).
                                Where(Function(g) g.Any(Function(x) x.Item("OPERATION") <> C_LIST_OPERATION_CODE.NODATA))
            For Each staff In staffs
                WW_LINEerr = C_MESSAGE_NO.NORMAL
                Dim head = staff.First(Function(x) x.Item("HDKBN") = "H")

                '始業・終業レコード取得　※複数存在を考慮し[A1]は初回、[Z1]は最終
                Dim a1 = staff.First(Function(x) x.Item("WORKKBN") = "A1")
                Dim z1 = staff.Last(Function(x) x.Item("WORKKBN") = "Z1")

                '①必須・項目属性チェック
                For Each check In checkTable
                    Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, check(0), head.Item(check(0)), WW_CHECKERR, WW_CHECKREPORT, S0013tbl)
                    If isNormal(WW_CHECKERR) Then
                    Else
                        'エラーレポート編集
                        WW_CheckMES1 = "・更新できないレコード(" & check(1) & "エラー)です。"
                        WW_CheckMES2 = WW_CHECKREPORT
                        ERRMSG_write(WW_CheckMES1, WW_CHECKREPORT, WW_LINEerr, head, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
                    End If
                Next

                '②関連チェック
                Dim wkDt As Date
                If Date.TryParse(head.Item("STDATE"), wkDt) AndAlso Date.TryParse(head.Item("STTIME"), wkDt) AndAlso
                    Date.TryParse(head.Item("ENDDATE"), wkDt) AndAlso Date.TryParse(head.Item("ENDTIME"), wkDt) Then

                    Dim stDt As Date = CDate(head.Item("STDATE") & " " & head.Item("STTIME"))
                    Dim endDt As Date = CDate(head.Item("ENDDATE") & " " & head.Item("ENDTIME"))
                    If stDt > endDt Then
                        'エラーレポート編集
                        WW_CheckMES1 = "・更新できないレコード(始業日時 > 終業日時)です。"
                        WW_CheckMES2 = ""
                        ERRMSG_write(WW_CheckMES1, WW_CHECKREPORT, WW_LINEerr, head, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
                    End If
                End If

                If Not isNormal(WW_LINEerr) Then
                    head.Item("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                    WW_ERRLIST_ALL.Add(WW_LINEerr)
                End If

            Next

            If WW_ERRLIST_ALL.Contains(C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR) Then
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

        End Using

    End Sub

    ''' <summary>
    ''' 名称設定処理   LeftBoxより名称取得＆チェック
    ''' </summary>
    ''' <param name="I_FIELD"></param>
    ''' <param name="I_VALUE"></param>
    ''' <param name="O_TEXT"></param>
    ''' <param name="O_RTN"></param>
    ''' <remarks></remarks>
    Protected Sub CODENAME_get(ByVal I_FIELD As String,
                               ByRef I_VALUE As String,
                               ByRef O_TEXT As String,
                               ByRef O_RTN As String)

        '○名称取得
        O_TEXT = String.Empty
        O_RTN = C_MESSAGE_NO.NORMAL

        If Not String.IsNullOrEmpty(I_VALUE) Then
            Select Case I_FIELD
                Case "DELFLG"
                    '削除フラグ　DELFLG
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_DELFLG, I_VALUE, O_TEXT, O_RTN, work.createFIXParam(work.WF_SEL_CAMPCODE.Text))
                Case "STAFFCODE"
                    '乗務員名
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_STAFFCODE, I_VALUE, O_TEXT, O_RTN, work.createSTAFFParam(work.WF_SEL_CAMPCODE.Text, work.WF_SEL_UORG.Text))
                Case "CAMPCODE"
                    '会社名
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_COMPANY, I_VALUE, O_TEXT, O_RTN)
                Case "SHIPORG"
                    '出荷部署名
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_ORG, I_VALUE, O_TEXT, O_RTN, work.createORGParam(work.WF_SEL_CAMPCODE.Text, C_PERMISSION.REFERLANCE))
                Case "CREWKBN"
                    '乗務区分名
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.createFIXParam(work.WF_SEL_CAMPCODE.Text, "CREWKBN"))
            End Select
        End If

    End Sub

    '★★★★★★★★★★★★★★★★★★★★★
    'データ操作
    '★★★★★★★★★★★★★★★★★★★★★

    ''' <summary>
    ''' 条件抽出画面情報退避
    ''' </summary>
    ''' <param name="O_RTN">可否判定</param>
    ''' <remarks></remarks>
    Protected Sub MAPrefelence(ByRef O_RTN As String)

        O_RTN = C_MESSAGE_NO.NORMAL

        '■■■ 選択画面の入力初期値設定 ■■■
        If Context.Handler.ToString().ToUpper = C_PREV_MAP_LIST.T00012S Then                                                    '条件画面からの画面遷移
            '○Grid情報保存先のファイル名
            Master.CreateXMLSaveFile()

        End If

        '勤怠締テーブル取得
        Dim WW_LIMITFLG As String = "0"
        T0007COM.T00008get(work.WF_SEL_CAMPCODE.Text,
                           work.WF_SEL_UORG.Text,
                           CDate(work.WF_SEL_STYMD.Text).ToString("yyyy/MM"),
                           WW_LIMITFLG,
                           WW_ERRCODE)
        If Not isNormal(WW_ERRCODE) Then
            Master.output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "T0008_KINTAISTAT")
            O_RTN = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End If

        If Master.MAPpermitcode = C_PERMISSION.UPDATE Then
            If WW_LIMITFLG = "0" Then
                '対象月の締前は更新ＯＫ
                WF_MAPpermitcode.Value = "TRUE"

                ''自分の部署と選択した配属部署が同一なら更新可能
                'If work.WF_SEL_UORG.Text = Master.USER_ORG Then
                '    WF_MAPpermitcode.Value = "TRUE"
                'Else
                '    WF_MAPpermitcode.Value = "FALSE"
                'End If
            Else
                WF_MAPpermitcode.Value = "FALSE"
            End If
        Else
            WF_MAPpermitcode.Value = "FALSE"
        End If

    End Sub
    ''' <summary>
    ''' エラーメッセージ編集
    ''' </summary>
    ''' <param name="I_MSG"></param>
    ''' <remarks></remarks>
    Private Sub ErrMsgSet(ByVal I_MSG As String)

        If WW_ERRLISTCNT <= 4000 Then
            rightview.addErrorReport(ControlChars.NewLine & I_MSG)

            WW_ERRLISTCNT += I_MSG.Length - I_MSG.Replace(vbCr, "").Length + 1

            If WW_ERRLISTCNT > 4000 Then
                Dim WW_ERR_MES As String = ""
                WW_ERR_MES = "※エラーが4000行超のため出力を停止しました。"
                rightview.addErrorReport(ControlChars.NewLine & WW_ERR_MES)
            End If

        End If
    End Sub
End Class