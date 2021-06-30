Imports System.Data.SqlClient
Imports OFFICE.GRIS0005LeftBox
Imports BASEDLL

''' <summary>
''' 仕訳パターンマスタ（登録）
''' </summary>
''' <remarks></remarks>
Public Class GRML0003SHIWAKEPATTERN
    Inherits Page

    '検索結果格納
    Private ML0003tbl As DataTable                              'Grid格納用テーブル
    Private ML0003INPtbl As DataTable                           'チェック用テーブル

    '共通関数宣言(BASEDLL)
    Private CS0010CHARstr As New CS0010CHARget                  '例外文字排除 String Get
    Private CS0011LOGWRITE As New CS0011LOGWrite                'LogOutput DirString Get
    Private CS0013ProfView As New CS0013ProfView                'Tableオブジェクト展開
    Private CS0020JOURNAL As New CS0020JOURNAL                  'Journal Out
    Private CS0023XLSUPLOAD As New CS0023XLSUPLOAD              'XLSアップロード
    Private CS0025AUTHORget As New CS0025AUTHORget              '権限チェック(APサーバチェックなし)
    Private CS0026TBLSORT As New CS0026TBLSORT                  '表示画面情報ソート
    Private CS0030REPORl As New CS0030REPORT                    '帳票出力
    Private CS0050SESSION As New CS0050SESSION                  'セッション情報操作処理
    Private CS0052DetailView As New CS0052DetailView            'Repeterオブジェクト作成

    '共通処理結果
    Private WW_ERRCODE As String = String.Empty                 'リターンコード
    Private WW_RTN_SW As String
    Private WW_DUMMY As String

    Private Const CONST_DSPROWCOUNT As Integer = 45             '１画面表示対象
    Private Const CONST_SCROLLROWCOUNT As Integer = 10          'マウススクロール時の増分
    Private Const CONST_DETAIL_TABID As String = "DTL1"         '詳細部タブID

    ''' <summary>
    ''' サーバ処理の遷移先
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
                    If Not Master.RecoverTable(ML0003tbl) Then
                        Exit Sub
                    End If

                    Select Case WF_ButtonClick.Value
                        Case "WF_ButtonExtract"
                            WF_ButtonExtract_Click()
                        Case "WF_ButtonUPDATE"
                            WF_ButtonUPDATE_Click()
                        Case "WF_ButtonCSV"
                            WF_ButtonCSV_Click()
                        Case "WF_ButtonPrint"
                            WF_Print_Click()
                        Case "WF_ButtonFIRST"
                            WF_ButtonFIRST_Click()
                        Case "WF_ButtonLAST"
                            WF_ButtonLAST_Click()
                        Case "WF_UPDATE"
                            WF_UPDATE_CLICK()
                        Case "WF_CLEAR"
                            WF_CLEAR_Click()
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
                        Case "WF_GridDBclick"
                            WF_Grid_DBclick()
                        Case "WF_MouseWheelDown"
                            WF_GRID_ScroleDown()
                        Case "WF_MouseWheelUp"
                            WF_GRID_ScroleUp()
                        Case "WF_EXCEL_UPLOAD"
                            UPLOAD_EXCEL()
                        Case Else
                    End Select

                    '○ 一覧再表示処理
                    DisplayGrid()
                End If
            Else
                '○ 初期化処理
                Initialize()
            End If

            '○ 画面モード(更新・参照)設定
            If Master.MAPpermitcode = C_PERMISSION.UPDATE Then
                WF_MAPpermitcode.Value = "TRUE"
            Else
                WF_MAPpermitcode.Value = "FALSE"
            End If

        Finally
            '○ 格納Table Close
            If Not IsNothing(ML0003tbl) Then
                ML0003tbl.Clear()
                ML0003tbl.Dispose()
                ML0003tbl = Nothing
            End If

            If Not IsNothing(ML0003INPtbl) Then
                ML0003INPtbl.Clear()
                ML0003INPtbl.Dispose()
                ML0003INPtbl = Nothing
            End If
        End Try

    End Sub


    ''' <summary>
    ''' 初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub Initialize()
        '○初期値設定
        WF_FIELD.Value = ""
        'WF_SELSHIWAKEPATERNKBN.Focus()
        rightview.ResetIndex()
        leftview.ActiveListBox()
        MAPrefelence()
        '○ヘルプ無
        Master.dispHelp = False
        '○ドラックアンドドロップON
        Master.eventDrop = True

        '右Boxへの値設定
        rightview.MAPID = Master.MAPID
        rightview.MAPVARI = Master.MAPvariant
        rightview.COMPCODE = work.WF_SEL_CAMPCODE.Text
        rightview.PROFID = Master.PROF_REPORT
        rightview.Initialize(WW_DUMMY)

        '○画面表示データ取得
        MAPDATAget()

        '○画面表示データ保存
        Master.SaveTable(ML0003tbl)

        '一覧表示データ編集（性能対策）
        Using TBLview As DataView = New DataView(ML0003tbl)
            TBLview.RowFilter = "LINECNT >= 1 and LINECNT <= " & CONST_DSPROWCOUNT
            CS0013ProfView.CAMPCODE = work.WF_SEL_CAMPCODE.Text
            CS0013ProfView.PROFID = Master.PROF_VIEW
            CS0013ProfView.MAPID = GRML0003WRKINC.MAPID
            CS0013ProfView.VARI = Master.VIEWID
            CS0013ProfView.SRCDATA = TBLview.ToTable
            CS0013ProfView.TBLOBJ = pnlListArea
            CS0013ProfView.SCROLLTYPE = CS0013ProfView.SCROLLTYPE_ENUM.Horizontal
            CS0013ProfView.LEVENT = "ondblclick"
            CS0013ProfView.LFUNC = "ListDbClick"
            CS0013ProfView.TITLEOPT = True
            CS0013ProfView.CS0013ProfView()
        End Using
        If Not isNormal(CS0013ProfView.ERR) Then
            Master.Output(C_MESSAGE_NO.SYSTEM_ADM_ERROR, C_MESSAGE_TYPE.ABORT, "一覧設定エラー")
            Exit Sub
        End If
        '詳細-画面初期設定
        Repeater_INIT()
    End Sub


    ''' <summary>
    ''' 一覧再表示処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub DisplayGrid()

        Dim WW_GridPosition As Integer                 '表示位置（開始）
        Dim WW_DataCNT As Integer = 0                  '(絞り込み後)有効Data数

        '表示対象行カウント(絞り込み対象)
        '　※　絞込（Cells(4)： 0=表示対象 , 1=非表示対象)
        For Each ML0003row As DataRow In ML0003tbl.Rows
            If ML0003row("HIDDEN") = 0 Then
                WW_DataCNT = WW_DataCNT + 1
                '行（ラインカウント）を再設定する。既存項目（SELECT）を利用
                ML0003row("SELECT") = WW_DataCNT
            End If
        Next

        '○表示Linecnt取得
        If WF_GridPosition.Text = "" Then
            WW_GridPosition = 1
        Else
            Try
                Integer.TryParse(WF_GridPosition.Text, WW_GridPosition)
            Catch ex As Exception
                WW_GridPosition = 1
            End Try
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
        Dim WW_TBLview As DataView = New DataView(ML0003tbl)

        'ソート
        WW_TBLview.Sort = "LINECNT"
        WW_TBLview.RowFilter = "HIDDEN = 0 and SELECT >= " & WW_GridPosition.ToString() & " and SELECT < " & (WW_GridPosition + CONST_DSPROWCOUNT).ToString()
        '一覧作成

        CS0013ProfView.CAMPCODE = work.WF_SEL_CAMPCODE.Text
        CS0013ProfView.PROFID = Master.PROF_VIEW
        CS0013ProfView.MAPID = GRML0003WRKINC.MAPID
        CS0013ProfView.VARI = Master.VIEWID
        CS0013ProfView.SRCDATA = WW_TBLview.ToTable
        CS0013ProfView.TBLOBJ = pnlListArea
        CS0013ProfView.SCROLLTYPE = CS0013ProfView.SCROLLTYPE_ENUM.Horizontal
        CS0013ProfView.LEVENT = "ondblclick"
        CS0013ProfView.LFUNC = "ListDbClick"
        CS0013ProfView.TITLEOPT = True
        CS0013ProfView.CS0013ProfView()

        '○クリア
        If WW_TBLview.Count = 0 Then
            WF_GridPosition.Text = "1"
        Else
            WF_GridPosition.Text = WW_TBLview.Item(0)("SELECT")
        End If


    End Sub


    ' ******************************************************************************
    ' ***  絞り込みボタン処理                                                    ***
    ' ******************************************************************************
    Protected Sub WF_ButtonExtract_Click()

        '○絞り込み操作（GridView明細Hidden設定）
        For Each row As DataRow In ML0003tbl.Rows



            '一度全部非表示化する
            If WF_SELSHIWAKEPATERNKBN.Text = "" Then
                WF_SELSHIWAKEPATERNKBN_TEXT.Text = ""
            End If

            If WF_SELACDCKBN.Text = "" Then
                WF_SELACDCKBN_TEXT.Text = ""
            End If

            row("HIDDEN") = 1

            '仕訳パターン、貸借区分
            If WF_SELSHIWAKEPATERNKBN.Text = "" AndAlso WF_SELACDCKBN.Text = "" Then
                row("HIDDEN") = 0
            End If

            If WF_SELSHIWAKEPATERNKBN.Text <> "" AndAlso WF_SELACDCKBN.Text = "" Then
                Dim WW_STRING As String = row("SHIWAKEPATERNKBN")     '検索用文字列（前方一致）
                If WW_STRING.StartsWith(WF_SELSHIWAKEPATERNKBN.Text) Then
                    row("HIDDEN") = 0
                End If
            End If

            If WF_SELSHIWAKEPATERNKBN.Text = "" AndAlso WF_SELACDCKBN.Text <> "" Then
                Dim WW_STRING As String = row("ACDCKBN")              '検索用文字列（前方一致）
                If WW_STRING.StartsWith(WF_ACDCKBN.Text) Then
                    row("HIDDEN") = 0
                End If
            End If

            If WF_SELSHIWAKEPATERNKBN.Text <> "" AndAlso WF_SELACDCKBN.Text <> "" Then
                Dim WW_STRING1 As String = row("SHIWAKEPATERNKBN")    '検索用文字列（前方一致）
                Dim WW_STRING2 As String = row("ACDCKBN")           　'検索用文字列（前方一致）
                If WW_STRING1.StartsWith(WF_SELSHIWAKEPATERNKBN.Text) AndAlso WW_STRING2.StartsWith(WF_SELACDCKBN.Text) Then
                    row("HIDDEN") = 0
                End If
            End If

        Next

        '○画面表示データ保存
        Master.SaveTable(ML0003tbl)

        '○画面表示
        '画面先頭を表示
        WF_GridPosition.Text = "1"

        '○メッセージ表示
        Master.Output(C_MESSAGE_NO.DATA_FILTER_SUCCESSFUL, C_MESSAGE_TYPE.INF)

        'カーソル設定
        'WF_SELSHIWAKEPATERNKBN.Focus()

    End Sub


    ' ******************************************************************************
    ' ***  DB更新ボタン処理                                                      ***
    ' ******************************************************************************
    ''' <summary>
    ''' DB更新ボタン押下処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonUPDATE_Click()

        '○ 関連チェック
        RelatedCheck(WW_RTN_SW)
        If Not isNormal(WW_RTN_SW) Then

            '○メッセージ表示
            Master.Output(WW_RTN_SW, C_MESSAGE_TYPE.ABORT)

            '○画面表示データ保存
            Master.SaveTable(ML0003tbl)
            Exit Sub
        End If
        Try
            Using SQLcon As SqlConnection = CS0050SESSION.getConnection
                SQLcon.Open()       'DataBase接続(Open)

                Dim SQLStr As String =
                      " DECLARE @hensuu as bigint ;                                                      " _
                    & " set @hensuu = 0 ;                                                                " _
                    & " DECLARE hensuu CURSOR FOR                                                        " _
                    & "   SELECT CAST(UPDTIMSTP as bigint) as hensuu                                     " _
                    & "     FROM    ML003_SHIWAKEPATTERN                                                 " _
                    & "     WHERE CAMPCODE =@P01 and SHIWAKEPATERNKBN = @P02 and SHIWAKEPATTERN = @P03   " _
                    & "       and ACDCKBN = @P04                                                         " _
                    & "       and STYMD = @P05 ;                                                         " _
                    & " OPEN hensuu ;                                                                    " _
                    & " FETCH NEXT FROM hensuu INTO @hensuu ;                                            " _
                    & " IF ( @@FETCH_STATUS = 0 )                                                        " _
                    & "    UPDATE   ML003_SHIWAKEPATTERN                                                 " _
                    & "       SET                                                                        " _
                    & "         SHIWAKEPATERNNAME = @P06                                                 " _
                    & "       , ENDYMD = @P07                                                            " _
                    & "       , ACCODE = @P08                                                            " _
                    & "       , INQKBN = @P09                                                            " _
                    & "       , TORICODE = @P10                                                          " _
                    & " 　　  , BANKCODE = @P11                                                          " _
                    & " 　　  , SEGMENT1 = @P12                                                          " _
                    & " 　　  , SEGMENT2 = @P13                                                          " _
                    & " 　　  , SEGMENT3 = @P14                                                          " _
                    & " 　　  , TAXKBN = @P15                                                            " _
                    & "       , DELFLG = @P17                                                            " _
                    & "       , UPDYMD = @P18                                                            " _
                    & "       , UPDUSER = @P19                                                           " _
                    & "       , UPDTERMID    = @P20                                                      " _
                    & "       , RECEIVEYMD   = @P21                                                      " _
                    & "     WHERE CAMPCODE =@P01 and SHIWAKEPATERNKBN = @P02 and SHIWAKEPATTERN = @P03   " _
                    & "       and ACDCKBN = @P04                                                         " _
                    & "       and STYMD = @P05 ;                                                         " _
                    & " IF ( @@FETCH_STATUS <> 0 )                                                       " _
                    & "    INSERT INTO ML003_SHIWAKEPATTERN                                              " _
                    & "       ( CAMPCODE                                                                 " _
                    & "       , SHIWAKEPATERNKBN                                                         " _
                    & "       , SHIWAKEPATTERN                                                           " _
                    & "       , ACDCKBN                                                                  " _
                    & "       , STYMD                                                                    " _
                    & "       , SHIWAKEPATERNNAME                                                        " _
                    & "       , ENDYMD                                                                   " _
                    & "       , ACCODE                                                                   " _
                    & "       , INQKBN                                                                   " _
                    & " 　　  , TORICODE                                                                 " _
                    & " 　　  , BANKCODE                                                                 " _
                    & " 　　  , SEGMENT1                                                                 " _
                    & " 　　  , SEGMENT2                                                                 " _
                    & " 　　  , SEGMENT3                                                                 " _
                    & " 　　  , TAXKBN                                                                   " _
                    & "       , DELFLG                                                                   " _
                    & "       , INITYMD                                                                  " _
                    & "       , UPDYMD                                                                   " _
                    & "       , UPDUSER                                                                  " _
                    & "       , UPDTERMID                                                                " _
                    & "       , RECEIVEYMD )                                                             " _
                    & "      VALUES (@P01,@P02,@P03,@P04,@P05,@P06,@P07,@P08,@P09,@P10,@P11,@P12,@P13,@P14,@P15,@P16   " _
                    & "             ,@P17,@P18,@P19,@P20,@P21) ;                                         " _
                    & " CLOSE hensuu ;                                                                   " _
                    & " DEALLOCATE hensuu ;                                                              "

                Dim SQLStr1 As String =
                      " Select  CAMPCODE   , SHIWAKEPATERNKBN , SHIWAKEPATTERN , SHIWAKEPATERNNAME,      " _
                    & "         ACCODE     , STYMD            , ENDYMD         , ACCODE           ,      " _
                    & "         INQKBN     , TORICODE         , BANKCODE       , SEGMENT1         ,      " _
                    & "         SEGMENT2   , SEGMENT3         , TAXKBN         , DELFLG           ,      " _
                    & "         INITYMD    , UPDYMD           , UPDUSER        , UPDTERMID        ,      " _
                    & "         RECEIVEYMD , CAST(UPDTIMSTP As bigint) As TIMSTP                         " _
                    & " FROM  ML003_SHIWAKEPATTERN " _
                    & "     WHERE CAMPCODE =@P01 and SHIWAKEPATERNKBN = @P02 and SHIWAKEPATTERN = @P03   " _
                    & "       and ACDCKBN = @P04                                                         " _
                    & "       and STYMD = @P05 ;                                                         "

                Using SQLcmd As New SqlCommand(SQLStr, SQLcon), SQLcmd1 As New SqlCommand(SQLStr1, SQLcon)
                    Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", SqlDbType.NVarChar)          'CAMPCODE
                    Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", SqlDbType.NVarChar)          'SHIWAKEPATERNKBN
                    Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@P03", SqlDbType.NVarChar)          'SHIWAKEPATTERN
                    Dim PARA04 As SqlParameter = SQLcmd.Parameters.Add("@P04", SqlDbType.NVarChar)          'ACDCKBN
                    Dim PARA05 As SqlParameter = SQLcmd.Parameters.Add("@P05", SqlDbType.Date)              'STYMD
                    Dim PARA06 As SqlParameter = SQLcmd.Parameters.Add("@P06", SqlDbType.NVarChar)          'SHIWAKEPATERNNAME
                    Dim PARA07 As SqlParameter = SQLcmd.Parameters.Add("@P07", SqlDbType.Date)              'ENDYMD
                    Dim PARA08 As SqlParameter = SQLcmd.Parameters.Add("@P08", SqlDbType.NVarChar)          'ACCODE
                    Dim PARA09 As SqlParameter = SQLcmd.Parameters.Add("@P09", SqlDbType.NVarChar)          'INQKBN
                    Dim PARA10 As SqlParameter = SQLcmd.Parameters.Add("@P10", SqlDbType.NVarChar)          'TORICODE
                    Dim PARA11 As SqlParameter = SQLcmd.Parameters.Add("@P11", SqlDbType.NVarChar)          'BANKCODE
                    Dim PARA12 As SqlParameter = SQLcmd.Parameters.Add("@P12", SqlDbType.NVarChar)          'SEGMENT1
                    Dim PARA13 As SqlParameter = SQLcmd.Parameters.Add("@P13", SqlDbType.NVarChar)          'SEGMENT2
                    Dim PARA14 As SqlParameter = SQLcmd.Parameters.Add("@P14", SqlDbType.NVarChar)          'SEGMENT3
                    Dim PARA15 As SqlParameter = SQLcmd.Parameters.Add("@P15", SqlDbType.NVarChar)          'TAXKBN
                    Dim PARA16 As SqlParameter = SQLcmd.Parameters.Add("@P16", SqlDbType.NVarChar)          'DELFLG
                    Dim PARA17 As SqlParameter = SQLcmd.Parameters.Add("@P17", SqlDbType.SmallDateTime)     'INITYMD
                    Dim PARA18 As SqlParameter = SQLcmd.Parameters.Add("@P18", SqlDbType.DateTime)          'UPDYMD
                    Dim PARA19 As SqlParameter = SQLcmd.Parameters.Add("@P19", SqlDbType.NVarChar)          'UPDUSER
                    Dim PARA20 As SqlParameter = SQLcmd.Parameters.Add("@P20", SqlDbType.NVarChar)          'UPDTERMID
                    Dim PARA21 As SqlParameter = SQLcmd.Parameters.Add("@P21", SqlDbType.DateTime)          'RECEIVEYMD

                    Dim PARAS01 As SqlParameter = SQLcmd1.Parameters.Add("@P01", SqlDbType.NVarChar)         'CAMPCODE
                    Dim PARAS02 As SqlParameter = SQLcmd1.Parameters.Add("@P02", SqlDbType.NVarChar)         'SHIWAKEPATERNKBN
                    Dim PARAS03 As SqlParameter = SQLcmd1.Parameters.Add("@P03", SqlDbType.NVarChar)         'SHIWAKEPATTERN
                    Dim PARAS04 As SqlParameter = SQLcmd1.Parameters.Add("@P04", SqlDbType.NVarChar)         'ACDCKBN
                    Dim PARAS05 As SqlParameter = SQLcmd1.Parameters.Add("@P05", SqlDbType.Date)             'STYMD

                    '○ＤＢ更新
                    For Each ML0003row As DataRow In ML0003tbl.Rows
                        If Trim(ML0003row("OPERATION")) = C_LIST_OPERATION_CODE.UPDATING OrElse
                           Trim(ML0003row("OPERATION")) = C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING Then
                            '※追加レコードは、ML0003tbl.Rows(i)("TIMSTP") = "0"となっているが状態のみで判定

                            PARA01.Value = ML0003row("CAMPCODE")
                            PARA02.Value = ML0003row("SHIWAKEPATERNKBN")
                            PARA03.Value = ML0003row("SHIWAKEPATTERN")
                            PARA04.Value = ML0003row("ACDCKBN")
                            PARA05.Value = ML0003row("STYMD")
                            PARA06.Value = ML0003row("SHIWAKEPATERNNAME")
                            PARA07.Value = ML0003row("ENDYMD")
                            PARA08.Value = ML0003row("ACCODE")
                            PARA09.Value = ML0003row("INQKBN")
                            PARA10.Value = ML0003row("TORICODE")
                            PARA11.Value = ML0003row("BANKCODE")
                            PARA12.Value = ML0003row("SEGMENT1")
                            PARA13.Value = ML0003row("SEGMENT2")
                            PARA14.Value = ML0003row("SEGMENT3")
                            PARA15.Value = ML0003row("TAXKBN")
                            PARA16.Value = ML0003row("DELFLG")
                            PARA17.Value = Date.Now
                            PARA18.Value = Date.Now
                            PARA19.Value = Master.USERID
                            PARA20.Value = Master.USERTERMID
                            PARA21.Value = C_DEFAULT_YMD

                            SQLcmd.ExecuteNonQuery()

                            ML0003row("OPERATION") = C_LIST_OPERATION_CODE.NODATA

                            '○更新ジャーナル追加
                            Try
                                PARAS01.Value = ML0003row("CAMPCODE")
                                PARAS02.Value = ML0003row("SHIWAKEPATERNKBN")
                                PARAS03.Value = ML0003row("SHIWAKEPATTERN")
                                PARAS04.Value = ML0003row("ACDCKBN")
                                PARAS05.Value = ML0003row("STYMD")

                                Dim JOURds As New DataSet()
                                Dim SQLadp As SqlDataAdapter

                                SQLadp = New SqlDataAdapter(SQLcmd1)
                                SQLadp.Fill(JOURds, "JOURtbl")

                                CS0020JOURNAL.TABLENM = "ML003_SHIWAKEPATTERN"
                                CS0020JOURNAL.ACTION = "UPDATE_INSERT"
                                CS0020JOURNAL.ROW = JOURds.Tables("JOURtbl").Rows(0)
                                CS0020JOURNAL.CS0020JOURNAL()
                                If Not isNormal(CS0020JOURNAL.ERR) Then
                                    Master.Output(CS0020JOURNAL.ERR, C_MESSAGE_TYPE.ABORT, "CS0020JOURNAL JOURNAL")
                                    CS0011LOGWRITE.INFSUBCLASS = "MAIN"                         'SUBクラス名
                                    CS0011LOGWRITE.INFPOSI = "CS0020JOURNAL JOURNAL"
                                    CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
                                    CS0011LOGWRITE.TEXT = "CS0020JOURNAL Call err!"
                                    CS0011LOGWRITE.MESSAGENO = CS0020JOURNAL.ERR
                                    CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
                                    Exit Sub
                                End If

                                ML0003row("TIMSTP") = JOURds.Tables("JOURtbl").Rows(0)("TIMSTP")

                                SQLadp.Dispose()
                                SQLadp = Nothing
                            Catch ex As Exception
                                If ex.Message = "Error raised In TIMSTP" Then
                                    ML0003row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                                End If
                                Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "ML003_SHIWAKEPATTERN JOURNAL")

                                CS0011LOGWRITE.INFSUBCLASS = "MAIN"                         'SUBクラス名
                                CS0011LOGWRITE.INFPOSI = "DB:MC013_UNCHINKETEI JOURNAL"
                                CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
                                CS0011LOGWRITE.TEXT = ex.ToString()
                                CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                                CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力

                                Exit Sub
                            End Try
                        End If
                    Next
                End Using
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "ML003_SHIWAKEPATTERN UPDATE_INSERT")
            CS0011LOGWRITE.INFSUBCLASS = "MAIN"
            CS0011LOGWRITE.INFPOSI = "DB:ML003_SHIWAKEPATTERN UPDATE_INSERT"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()

            Exit Sub
        End Try

        '○画面表示データ保存
        Master.SaveTable(ML0003tbl)

        '詳細画面クリア
        Detailbox_Clear()

        '○メッセージ表示
        Master.Output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF)

        'カーソル設定
        'WF_SELSHIWAKEPATERNKBN.Focus()

    End Sub


    ' ******************************************************************************
    ' ***  ﾀﾞｳﾝﾛｰﾄﾞ(PDF出力)・一覧印刷ボタン処理                                 ***
    ' ******************************************************************************
    ''' <summary>
    ''' 一覧印刷ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_Print_Click()

        '○帳票出力
        CS0030REPORl.CAMPCODE = work.WF_SEL_CAMPCODE.Text
        CS0030REPORl.PROFID = Master.PROF_REPORT
        CS0030REPORl.MAPID = GRML0003WRKINC.MAPID
        CS0030REPORl.REPORTID = rightview.GetReportId()
        CS0030REPORl.FILEtyp = "pdf"
        CS0030REPORl.TBLDATA = ML0003tbl
        CS0030REPORl.CS0030REPORT()

        If Not isNormal(CS0030REPORl.ERR) Then
            If CS0030REPORl.ERR = C_MESSAGE_NO.REPORT_EXCEL_NOT_FOUND_ERROR Then
                Master.Output(CS0030REPORl.ERR, C_MESSAGE_TYPE.ERR)
            Else
                Master.Output(CS0030REPORl.ERR, C_MESSAGE_TYPE.ABORT, "CS0030REPORl")
            End If
            Exit Sub
        End If

        '○別画面でPDFを表示
        WF_PrintURL.Value = CS0030REPORl.URL
        ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_PDFPrint();", True)

    End Sub


    ' ******************************************************************************
    ' ***  ﾀﾞｳﾝﾛｰﾄﾞ(Excel出力)ボタン処理                                         ***
    ' ******************************************************************************
    ''' <summary>
    ''' ダウンロードボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonCSV_Click()

        '○帳票出力
        CS0030REPORl.CAMPCODE = work.WF_SEL_CAMPCODE.Text
        CS0030REPORl.MAPID = GRML0003WRKINC.MAPID
        CS0030REPORl.PROFID = Master.PROF_REPORT
        CS0030REPORl.REPORTID = rightview.GetReportId()
        CS0030REPORl.FILEtyp = "XLSX"
        CS0030REPORl.TBLDATA = ML0003tbl
        CS0030REPORl.CS0030REPORT()
        If Not isNormal(CS0030REPORl.ERR) Then
            If CS0030REPORl.ERR = C_MESSAGE_NO.REPORT_EXCEL_NOT_FOUND_ERROR Then
                Master.Output(CS0030REPORl.ERR, C_MESSAGE_TYPE.ERR)
            Else

                Master.Output(CS0030REPORl.ERR, C_MESSAGE_TYPE.ABORT, "CS0030REPORT")
            End If
            Exit Sub
        End If
        '○別画面でExcelを表示
        WF_PrintURL.Value = CS0030REPORl.URL
        ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_ExcelPrint();", True)
    End Sub


    ''' <summary>
    ''' 終了ボタン押下
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonEND_Click()

        Master.TransitionPrevPage()

    End Sub


    ''' <summary>
    ''' 先頭頁移動ボタン押下
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonFIRST_Click()

        '○先頭頁に移動
        WF_GridPosition.Text = "1"

    End Sub


    ''' <summary>
    ''' 最終頁遷移ボタン押下
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonLAST_Click()

        '○ソート
        Dim WW_TBLview As DataView
        WW_TBLview = New DataView(ML0003tbl)
        WW_TBLview.RowFilter = "HIDDEN= '0'"

        '○先頭頁に移動
        If WW_TBLview.Count Mod CONST_SCROLLROWCOUNT = 0 Then
            WF_GridPosition.Text = WW_TBLview.Count - (WW_TBLview.Count Mod CONST_SCROLLROWCOUNT)
        Else
            WF_GridPosition.Text = WW_TBLview.Count - (WW_TBLview.Count Mod CONST_SCROLLROWCOUNT) + 1
        End If

    End Sub


    ' ******************************************************************************
    ' ***  一覧表示関連操作                                                      ***
    ' ******************************************************************************

    ''' <summary>
    ''' 一覧の明細行ダブルクリック時処理
    ''' </summary>
    ''' <remarks>(GridView ---> detailbox)</remarks>
    Protected Sub WF_Grid_DBclick()

        '○抽出条件(ヘッダーレコードより)定義
        Dim WW_Position As Integer = 0
        Dim WW_FILED_OBJ As Object
        Dim WW_VALUE As String = ""
        Dim WW_TEXT As String = ""
        Dim WW_LINECNT As Integer

        '○LINECNT
        Try
            Integer.TryParse(WF_GridDBclick.Text, WW_Position)
            WW_Position = WW_Position - 1
            WW_LINECNT = WW_Position
        Catch ex As Exception
            Exit Sub
        End Try

        '○ダブルクリック明細情報取得設定（GridView --> Detailboxヘッダー情報)
        '選択行
        WF_Sel_LINECNT.Text = ML0003tbl.Rows(WW_Position)("LINECNT")
        WF_CAMPCODE.Text = ML0003tbl.Rows(WW_Position)("CAMPCODE")
        WF_CAMPCODE_TEXT.Text = ML0003tbl.Rows(WW_Position)("CAMPNAMES")
        WF_SHIWAKEPATERNKBN.Text = ML0003tbl.Rows(WW_Position)("SHIWAKEPATERNKBN")
        WF_SHIWAKEPATERNKBN_TEXT.Text = ML0003tbl.Rows(WW_Position)("SHIWAKEPATERNKBNNAMES")
        WF_SHIWAKEPATTERN.Text = ML0003tbl.Rows(WW_Position)("SHIWAKEPATTERN")
        WF_ACDCKBN.Text = ML0003tbl.Rows(WW_Position)("ACDCKBN")
        WF_ACDCKBN_TEXT.Text = ML0003tbl.Rows(WW_Position)("ACDCKBNNAMES")

        '有効年月日
        WF_STYMD.Text = ML0003tbl.Rows(WW_Position)("STYMD")
        WF_ENDYMD.Text = ML0003tbl.Rows(WW_Position)("ENDYMD")
        '削除フラグ
        WF_DELFLG.Text = ML0003tbl.Rows(WW_Position)("DELFLG")
        CODENAME_get("DELFLG", WF_DELFLG.Text, WW_TEXT, WW_DUMMY)
        WF_DELFLG_TEXT.Text = WW_TEXT


        '○Grid設定処理
        For Each reitem As RepeaterItem In WF_DViewRep1.Items
            '左
            WW_FILED_OBJ = CType(reitem.FindControl("WF_Rep1_FIELD_1"), Label)

            If WW_FILED_OBJ.Text <> "" Then
                '値設定
                WW_VALUE = WF_ITEM_FORMAT(WW_FILED_OBJ.text, ML0003tbl.Rows(WW_LINECNT)(WW_FILED_OBJ.Text))
                CType(reitem.FindControl("WF_Rep1_VALUE_1"), TextBox).Text = WW_VALUE

                '値（名称）設定
                CODENAME_get(WW_FILED_OBJ.Text, WW_VALUE, WW_TEXT, WW_DUMMY)

                CType(reitem.FindControl("WF_Rep1_VALUE_TEXT_1"), Label).Text = WW_TEXT

            End If

            '中央
            WW_FILED_OBJ = CType(reitem.FindControl("WF_Rep1_FIELD_2"), Label)
            If WW_FILED_OBJ.Text <> "" Then
                '値設定
                WW_VALUE = WF_ITEM_FORMAT(WW_FILED_OBJ.text, ML0003tbl.Rows(WW_LINECNT)(WW_FILED_OBJ.Text))
                CType(reitem.FindControl("WF_Rep1_VALUE_2"), TextBox).Text = WW_VALUE

                '値（名称）設定
                CODENAME_get(WW_FILED_OBJ.Text, WW_VALUE, WW_TEXT, WW_DUMMY)

                CType(reitem.FindControl("WF_Rep1_VALUE_TEXT_2"), Label).Text = WW_TEXT
            End If

            '右
            WW_FILED_OBJ = CType(reitem.FindControl("WF_Rep1_FIELD_3"), Label)
            If WW_FILED_OBJ.Text <> "" Then
                '値設定
                WW_VALUE = WF_ITEM_FORMAT(WW_FILED_OBJ.text, ML0003tbl.Rows(WW_LINECNT)(WW_FILED_OBJ.Text))
                CType(reitem.FindControl("WF_Rep1_VALUE_3"), TextBox).Text = WW_VALUE

                '値（名称）設定
                CODENAME_get(WW_FILED_OBJ.Text, WW_VALUE, WW_TEXT, WW_DUMMY)

                CType(reitem.FindControl("WF_Rep1_VALUE_TEXT_3"), Label).Text = WW_TEXT
            End If
        Next

        '○画面WF_GRID状態設定
        '状態をクリア設定
        For Each ML0003row As DataRow In ML0003tbl.Rows
            Select Case ML0003row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    ML0003row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.NODISP
                    ML0003row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED
                    ML0003row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    ML0003row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    ML0003row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End Select
        Next

        '選択明細のOperation項目に状態を設定(更新・追加・削除は編集中を設定しない)
        Select Case ML0003tbl.Rows(WW_Position)("OPERATION")
            Case C_LIST_OPERATION_CODE.NODATA
                ML0003tbl.Rows(WW_Position)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
            Case C_LIST_OPERATION_CODE.NODISP
                ML0003tbl.Rows(WW_Position)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
            Case C_LIST_OPERATION_CODE.SELECTED
                ML0003tbl.Rows(WW_Position)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
            Case C_LIST_OPERATION_CODE.UPDATING
                ML0003tbl.Rows(WW_Position)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
            Case C_LIST_OPERATION_CODE.ERRORED
                ML0003tbl.Rows(WW_Position)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
            Case Else
        End Select

        '○画面切替設定
        WF_BOXChange.Value = "detailbox"

        '○画面表示データ保存
        Master.SaveTable(ML0003tbl)

        'WF_SHIWAKEPATTERN.Focus()
        WF_GridDBclick.Text = ""

    End Sub

    Protected Function WF_ITEM_FORMAT(ByVal I_FIELD As String, ByRef I_VALUE As String) As String

        WF_ITEM_FORMAT = I_VALUE
        Select Case I_FIELD
            Case "SEQ"
                Try
                    WF_ITEM_FORMAT = Format(CInt(I_VALUE), "0")
                Catch ex As Exception
                End Try
            Case Else
        End Select

    End Function


    ' *** 一覧画面-スクロールSUB

    ' *** 一覧画面-非表示列削除（性能対策）

    ' ******************************************************************************
    ' ***  詳細表示関連操作                                                      ***
    ' ******************************************************************************

    ''' <summary>
    ''' 詳細画面-表更新ボタン処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_UPDATE_CLICK()

        '○エラーレポート準備
        rightview.SetErrorReport("")

        Dim WW_ERR10023 As String = C_MESSAGE_NO.NORMAL

        '○DetailBoxをML0003INPtblへ退避
        Master.CreateEmptyTable(ML0003INPtbl)
        DetailBoxToML0003INPtbl(WW_ERRCODE)
        If Not isNormal(WW_ERRCODE) Then
            Exit Sub
        End If

        '○項目チェック
        INPtbl_Check(WW_ERRCODE)

        '○GridView更新
        If isNormal(WW_ERRCODE) Then
            ML0003tbl_UPD()
        End If

        '○一覧(ML0003tbl)内で、新規追加（タイムスタンプ０）かつ削除の場合はレコード削除
        If isNormal(WW_ERRCODE) Then
            Dim WW_DEL As String = "ON"
            Do
                For i As Integer = 0 To ML0003tbl.Rows.Count - 1
                    If ML0003tbl.Rows(i)("TIMSTP") = 0 AndAlso ML0003tbl.Rows(i)("DELFLG") = C_DELETE_FLG.DELETE Then
                        ML0003tbl.Rows(i).Delete()
                        WW_DEL = "OFF"
                        Exit For
                    Else
                        If (ML0003tbl.Rows.Count - 1) <= i Then
                            WW_DEL = "ON"
                        End If
                    End If
                Next
            Loop Until WW_DEL = "ON"
        End If

        '○画面表示データ保存
        Master.SaveTable(ML0003tbl)

        'Detailクリア
        If isNormal(WW_ERRCODE) Then
            WF_CLEAR_Click()
        End If
        'メッセージ表示
        If isNormal(WW_ERRCODE) Then
            Master.Output(C_MESSAGE_NO.NORMAL, C_MESSAGE_TYPE.NOR)
        Else
            Master.Output(WW_ERRCODE, C_MESSAGE_TYPE.ERR)
        End If

        If isNormal(WW_ERRCODE) Then
            '○画面切替設定
            WF_BOXChange.Value = "headerbox"
        Else
        End If


        'カーソル設定
        'WF_SELSHIWAKEPATERNKBN.Focus()

    End Sub

    ''' <summary>
    '''  詳細画面-テーブル退避
    ''' </summary>
    ''' <param name="O_RTNCODE"></param>
    ''' <remarks></remarks>
    Protected Sub DetailBoxToML0003INPtbl(ByRef O_RTNCODE As String)

        Dim WW_TEXT As String = String.Empty
        Dim WW_RTN As String = String.Empty

        O_RTNCODE = C_MESSAGE_NO.NORMAL

        'ML0003テンポラリDB項目作成
        Master.CreateEmptyTable(ML0003INPtbl)

        '○入力文字置き換え & CS0007CHKテーブルレコード追加

        '○ 画面(Repeaterヘッダー情報)の使用禁止文字排除
        Master.EraseCharToIgnore(WF_CAMPCODE.Text)          '会社コード
        Master.EraseCharToIgnore(WF_SHIWAKEPATERNKBN.Text)  '仕訳パターン分類
        Master.EraseCharToIgnore(WF_SHIWAKEPATTERN.Text)    '仕訳パターン
        Master.EraseCharToIgnore(WF_ACDCKBN.Text)           '貸借区分
        Master.EraseCharToIgnore(WF_STYMD.Text)             '開始年月日
        Master.EraseCharToIgnore(WF_ENDYMD.Text)            '終了年月日
        Master.EraseCharToIgnore(WF_DELFLG.Text)            '削除フラグ

        'GridViewから未選択状態で表更新ボタンを押下時の例外を回避する 
        If String.IsNullOrEmpty(WF_Sel_LINECNT.Text) AndAlso
            String.IsNullOrEmpty(WF_SHIWAKEPATERNKBN.Text) AndAlso
            String.IsNullOrEmpty(WF_SHIWAKEPATTERN.Text) AndAlso
            String.IsNullOrEmpty(WF_ACDCKBN.Text) AndAlso
            String.IsNullOrEmpty(WF_STYMD.Text) AndAlso
            String.IsNullOrEmpty(WF_ENDYMD.Text) AndAlso
            String.IsNullOrEmpty(WF_DELFLG.Text) Then
            Master.Output(C_MESSAGE_NO.INVALID_PROCCESS_ERROR, C_MESSAGE_TYPE.ERR, "no Detail")
            CS0011LOGWRITE.INFSUBCLASS = "DetailBoxToML0003INPtbl"      'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "non Detail"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ERR
            CS0011LOGWRITE.TEXT = "non Detail"
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.INVALID_PROCCESS_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力

            O_RTNCODE = C_MESSAGE_NO.INVALID_PROCCESS_ERROR

            Exit Sub
        End If

        '○画面(Repeaterヘッダー情報)のテーブル退避
        Dim ML0003INProw As DataRow = ML0003INPtbl.NewRow
        '初期クリア
        For Each ML0003INPcol As DataColumn In ML0003INProw.Table.Columns
            If ML0003INPcol.DataType.Name.ToString() = "String" Then
                ML0003INProw(ML0003INPcol.ColumnName) = ""
            End If
        Next

        If (String.IsNullOrEmpty(WF_Sel_LINECNT.Text)) Then
            ML0003INProw("LINECNT") = 0
        Else
            ML0003INProw("LINECNT") = CType(WF_Sel_LINECNT.Text, Integer)   'DBの固定フィールド
        End If
        ML0003INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA            'DBの固定フィールド
        ML0003INProw("TIMSTP") = 0                                          'DBの固定フィールド
        ML0003INProw("SELECT") = "0"                                        'DBの固定フィールド
        ML0003INProw("HIDDEN") = "0"                                        'DBの固定フィールド

        ML0003INProw("CAMPCODE") = WF_CAMPCODE.Text
        ML0003INProw("SHIWAKEPATERNKBN") = WF_SHIWAKEPATERNKBN.Text
        ML0003INProw("SHIWAKEPATTERN") = WF_SHIWAKEPATTERN.Text
        ML0003INProw("ACDCKBN") = WF_ACDCKBN.Text
        ML0003INProw("STYMD") = WF_STYMD.Text
        ML0003INProw("ENDYMD") = WF_ENDYMD.Text
        ML0003INProw("DELFLG") = WF_DELFLG.Text


        '○Detail設定処理
        For Each reitem As RepeaterItem In WF_DViewRep1.Items
            '左
            If CType(reitem.FindControl("WF_Rep1_FIELD_1"), Label).Text <> "" Then
                CS0010CHARstr.CHARIN = CType(reitem.FindControl("WF_Rep1_VALUE_1"), TextBox).Text
                CS0010CHARstr.CS0010CHARget()
                ML0003INProw(CType(reitem.FindControl("WF_Rep1_FIELD_1"), Label).Text) = CS0010CHARstr.CHAROUT
            End If

            '中央
            If CType(reitem.FindControl("WF_Rep1_FIELD_2"), Label).Text <> "" Then
                CS0010CHARstr.CHARIN = CType(reitem.FindControl("WF_Rep1_VALUE_2"), TextBox).Text
                CS0010CHARstr.CS0010CHARget()
                ML0003INProw(CType(reitem.FindControl("WF_Rep1_FIELD_2"), Label).Text) = CS0010CHARstr.CHAROUT
            End If

            '右
            If CType(reitem.FindControl("WF_Rep1_FIELD_3"), Label).Text <> "" Then
                CS0010CHARstr.CHARIN = CType(reitem.FindControl("WF_Rep1_VALUE_3"), TextBox).Text
                CS0010CHARstr.CS0010CHARget()
                ML0003INProw(CType(reitem.FindControl("WF_Rep1_FIELD_3"), Label).Text) = CS0010CHARstr.CHAROUT
            End If
        Next

        '○コード名称を設定する
        ' 会社コード
        WW_TEXT = ""
        CODENAME_get("CAMPCODE", ML0003INProw("CAMPCODE"), WW_TEXT, WW_DUMMY)
        ML0003INProw("CAMPNAMES") = WW_TEXT

        ' 仕訳パターン分類(固定値マスタ)
        WW_TEXT = ""
        CODENAME_get("SHIWAKEPATERNKBN", ML0003INProw("SHIWAKEPATERNKBN"), WW_TEXT, WW_DUMMY)
        ML0003INProw("SHIWAKEPATERNKBNNAMES") = WW_TEXT

        ' 貸借区分(固定値マスタ)
        WW_TEXT = ""
        CODENAME_get("ACDCKBN", ML0003INProw("ACDCKBN"), WW_TEXT, WW_DUMMY)
        ML0003INProw("ACDCKBNNAMES") = WW_TEXT

        ' 勘定科目
        WW_TEXT = ""
        CODENAME_get("ACCODE", ML0003INProw("ACCODE"), WW_TEXT, WW_DUMMY)
        ML0003INProw("ACCODENAMES") = WW_TEXT

        ' 照会区分（画面）(固定値マスタ)
        WW_TEXT = ""
        CODENAME_get("INQKBN", ML0003INProw("INQKBN"), WW_TEXT, WW_DUMMY)
        ML0003INProw("INQKBNNAMES") = WW_TEXT

        ' 取引先
        WW_TEXT = ""
        CODENAME_get("TORICODE", ML0003INProw("TORICODE"), WW_TEXT, WW_DUMMY)
        ML0003INProw("TORICODENAMES") = WW_TEXT

        ' 銀行コード
        WW_TEXT = ""
        CODENAME_get("BANKCODE", ML0003INProw("BANKCODE"), WW_TEXT, WW_DUMMY)
        ML0003INProw("BANKCODENAMES") = WW_TEXT

        ' セグメント1(固定値マスタ)
        WW_TEXT = ""
        CODENAME_get("SEGMENT1", ML0003INProw("SEGMENT1"), WW_TEXT, WW_DUMMY)
        ML0003INProw("SEGMENT1NAMES") = WW_TEXT

        ' セグメント2(固定値マスタ)
        WW_TEXT = ""
        CODENAME_get("SEGMENT2", ML0003INProw("SEGMENT2"), WW_TEXT, WW_DUMMY)
        ML0003INProw("SEGMENT2NAMES") = WW_TEXT

        ' セグメント3(固定値マスタ)
        WW_TEXT = ""
        CODENAME_get("SEGMENT3", ML0003INProw("SEGMENT3"), WW_TEXT, WW_DUMMY)
        ML0003INProw("SEGMENT3NAMES") = WW_TEXT

        ' 税区分(固定値マスタ)
        WW_TEXT = ""
        CODENAME_get("TAXKBN", ML0003INProw("TAXKBN"), WW_TEXT, WW_DUMMY)
        ML0003INProw("TAXKBNNAMES") = WW_TEXT

        ' チェック用テーブルに登録する
        ML0003INPtbl.Rows.Add(ML0003INProw)

    End Sub

    ''' <summary>
    ''' 詳細画面-クリアボタン処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_CLEAR_Click()

        '○detailboxクリア
        Detailbox_Clear()

        'メッセージ表示
        Master.Output(C_MESSAGE_NO.DATA_CLEAR_SUCCESSFUL, C_MESSAGE_TYPE.INF)

        '○画面切替設定
        WF_BOXChange.Value = "headerbox"

        '○カーソル設定
        'WF_SELSHIWAKEPATERNKBN.Focus()

    End Sub


    ''' <summary>
    ''' 詳細画面-クリア処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub Detailbox_Clear()

        '○画面WF_GRID状態設定
        '状態をクリア設定
        For Each ML0003row As DataRow In ML0003tbl.Rows
            Select Case ML0003row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    ML0003row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.NODISP
                    ML0003row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED
                    ML0003row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    ML0003row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    ML0003row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End Select
        Next

        '○画面表示データ保存
        Master.SaveTable(ML0003tbl)

        '画面(Grid)のHIDDEN列により、表示/非表示を行う。

        WF_Sel_LINECNT.Text = ""
        WF_CAMPCODE.Text = work.WF_SEL_CAMPCODE.Text
        CODENAME_get("CAMPCODE", WF_CAMPCODE.Text, WF_CAMPCODE_TEXT.Text, WW_DUMMY)
        WF_SHIWAKEPATERNKBN.Text = ""
        WF_SHIWAKEPATERNKBN_TEXT.Text = ""
        WF_SHIWAKEPATTERN.Text = ""
        WF_ACDCKBN.Text = ""
        WF_ACDCKBN_TEXT.Text = ""
        WF_STYMD.Text = ""
        WF_ENDYMD.Text = ""
        WF_DELFLG_TEXT.Text = ""
        WF_DELFLG.Text = ""
        WF_SEQ.Value = ""

        '○Detail初期設定
        Repeater_INIT()

        'WF_SELSHIWAKEPATERNKBN.Focus()

    End Sub

    ''' <summary>
    ''' 詳細画面 初期設定(空明細作成 イベント追加)
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub Repeater_INIT()

        Dim dataTable As DataTable = New DataTable
        Dim repField As Label = Nothing
        Dim repValue As TextBox = Nothing
        Dim repName As Label = Nothing
        Dim repAttr As String = ""

        Try
            'カラム情報をリピーター作成用に取得
            Master.CreateEmptyTable(dataTable)
            dataTable.Rows.Add(dataTable.NewRow())

            'リピーター作成
            CS0052DetailView.CAMPCODE = work.WF_SEL_CAMPCODE.Text
            CS0052DetailView.PROFID = Master.PROF_VIEW
            CS0052DetailView.MAPID = Master.MAPID
            CS0052DetailView.VARI = Master.VIEWID
            CS0052DetailView.SRCDATA = dataTable
            CS0052DetailView.REPEATER = WF_DViewRep1
            CS0052DetailView.COLPREFIX = "WF_Rep1_"
            CS0052DetailView.MaketDetailView()
            If Not isNormal(CS0052DetailView.ERR) Then
                Exit Sub
            End If

            WF_DetailMView.ActiveViewIndex = 0

            For row As Integer = 0 To CS0052DetailView.ROWMAX - 1
                For col As Integer = 1 To CS0052DetailView.COLMAX

                    'ダブルクリック時コード検索イベント追加
                    If DirectCast(WF_DViewRep1.Items(row).FindControl("WF_Rep1_FIELD_" & col), Label).Text <> "" Then
                        repField = DirectCast(WF_DViewRep1.Items(row).FindControl("WF_Rep1_FIELD_" & col), Label)
                        repValue = DirectCast(WF_DViewRep1.Items(row).FindControl("WF_Rep1_VALUE_" & col), TextBox)
                        REP_ATTR_get(repField.Text, repAttr)
                        If repAttr <> "" AndAlso Not repValue.ReadOnly Then
                            repValue.Attributes.Remove("ondblclick")
                            repValue.Attributes.Add("ondblclick", repAttr)
                            repName = DirectCast(WF_DViewRep1.Items(row).FindControl("WF_Rep1_FIELDNM_" & col), Label)
                            repName.Attributes.Remove("style")
                            repName.Attributes.Add("style", "text-decoration: underline;")
                        End If
                    End If

                Next col
            Next row

            WF_DViewRep1.Visible = True

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.SYSTEM_ADM_ERROR, C_MESSAGE_TYPE.ABORT)
        Finally
            dataTable.Dispose()
            dataTable = Nothing
        End Try

    End Sub


    ''' <summary>
    ''' 詳細画面-イベント文字取得
    ''' </summary>
    ''' <param name="I_FIELD">フィールド名</param>
    ''' <param name="O_ATTR">イベント内容</param>
    ''' <remarks></remarks>
    Protected Sub REP_ATTR_get(ByVal I_FIELD As String, ByRef O_ATTR As String)

        O_ATTR = ""
        Select Case I_FIELD
            Case "SHIWAKEPATERNKBN"
                ' 仕訳パターン分類(固定値マスタ)
                O_ATTR = "REF_Field_DBclick('SHIWAKEPATERNKBN', 'WF_Rep_FIELD' , '" & LIST_BOX_CLASSIFICATION.LC_FIX_VALUE & "');"

            Case "ACDCKBN"
                ' 貸借区分(固定値マスタ)
                O_ATTR = "REF_Field_DBclick('ACDCKBN', 'WF_Rep_FIELD' , '" & LIST_BOX_CLASSIFICATION.LC_FIX_VALUE & "');"

            Case "ACCODE"
                ' 勘定科目
                O_ATTR = "REF_Field_DBclick('ACCODE', 'WF_Rep_FIELD' , '" & LIST_BOX_CLASSIFICATION.LC_ACCODE & "');"

            Case "INQKBN"
                ' 照会区分（画面）(固定値マスタ)
                O_ATTR = "REF_Field_DBclick('INQKBN', 'WF_Rep_FIELD' , '" & LIST_BOX_CLASSIFICATION.LC_FIX_VALUE & "');"

            'Case "TORICODE"
            '    ' 取引先
            '    O_ATTR = "REF_Field_DBclick('TORICODE', 'WF_Rep_FIELD' , '" & LIST_BOX_CLASSIFICATION.LC_CUSTOMER & "');"

            Case "SEGMENT1"
                ' セグメント1(固定値マスタ)
                O_ATTR = "REF_Field_DBclick('SEGMENT1', 'WF_Rep_FIELD' , '" & LIST_BOX_CLASSIFICATION.LC_FIX_VALUE & "');"

            Case "SEGMENT2"
                ' セグメント2(固定値マスタ)
                O_ATTR = "REF_Field_DBclick('SEGMENT2', 'WF_Rep_FIELD' , '" & LIST_BOX_CLASSIFICATION.LC_FIX_VALUE & "');"

            Case "SEGMENT3"
                ' セグメント3(固定値マスタ)
                O_ATTR = "REF_Field_DBclick('SEGMENT3', 'WF_Rep_FIELD' , '" & LIST_BOX_CLASSIFICATION.LC_FIX_VALUE & "');"

            Case "TAXKBN"
                ' 税区分(固定値マスタ)
                O_ATTR = "REF_Field_DBclick('TAXKBN', 'WF_Rep_FIELD' , '" & LIST_BOX_CLASSIFICATION.LC_FIX_VALUE & "');"

        End Select

    End Sub


    ''' <summary>
    ''' フィールドダブルクリック処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_Field_DBClick()
        Dim WW_VALUE As String = ""
        Dim WW_VALUE2 As String = ""

        '○LeftBox処理（フィールドダブルクリック時）
        If Not String.IsNullOrEmpty(WF_LeftMViewChange.Value) Then
            Try
                Integer.TryParse(WF_LeftMViewChange.Value, WF_LeftMViewChange.Value)
            Catch ex As Exception
                Exit Sub
            End Try

            Dim WW_FIELD As String = ""
            Dim WW_FIELD2 As String = ""

            If WF_FIELD_REP.Value = "" Then
                WW_FIELD = WF_FIELD.Value
            Else
                WW_FIELD = WF_FIELD_REP.Value
            End If

            With leftview
                Select Case WF_LeftMViewChange.Value
                    Case LIST_BOX_CLASSIFICATION.LC_CALENDAR
                        '日付の場合、入力日付のカレンダーが表示されるように入力値をカレンダーに渡す
                        Select Case WW_FIELD
                            Case "WF_STYMD"         '有効年月日(From)
                                .WF_Calendar.Text = WF_STYMD.Text
                            Case "WF_ENDYMD"        '有効年月日(To)
                                .WF_Calendar.Text = WF_ENDYMD.Text
                        End Select
                        .ActiveCalendar()

                    Case Else
                        '以外
                        Dim prmData As New Hashtable
                        prmData.Item(C_PARAMETERS.LP_COMPANY) = work.WF_SEL_CAMPCODE.Text

                        Debug.Print(WF_FIELD.Value)
                        'フィールドによってパラメーターを変える
                        Select Case WW_FIELD
                            Case "WF_SELSHIWAKEPATERNKBN"     　 '仕訳パターン分類(絞り込み)
                                prmData = work.CreateFIXParam(WF_CAMPCODE.Text, "SHIWAKEPATERNKBN")
                            Case "WF_SELACDCKBN"            　   '貸借区分(絞り込み)
                                prmData = work.CreateFIXParam(WF_CAMPCODE.Text, "ACDCKBN")
                            Case "WF_SHIWAKEPATERNKBN"              '仕訳パターン分類
                                prmData = work.CreateFIXParam(WF_CAMPCODE.Text, "SHIWAKEPATERNKBN")
                            Case "WF_ACDCKBN"                       '貸借区分
                                prmData = work.CreateFIXParam(WF_CAMPCODE.Text, "ACDCKBN")
                            Case "ACCODE"       　　             '勘定科目
                                prmData = work.CreateACCParam(WF_CAMPCODE.Text, "")
                            Case "INQKBN"       　　             '照会区分（画面）
                                prmData = work.CreateFIXParam(WF_CAMPCODE.Text, "INQKBN")
                            Case "TORICOE"                       '取引先
                                prmData = work.CreateTORIParam(WF_CAMPCODE.Text)
                            Case "SEGMENT1"       　　           'セグメント1
                                prmData = work.CreateFIXParam(WF_CAMPCODE.Text, "SEGMENT1")
                            Case "SEGMENT2"                 　　 'セグメント2
                                prmData = work.CreateFIXParam(WF_CAMPCODE.Text, "SEGMENT2")
                            Case "SEGMENT3"                      'セグメント3
                                prmData = work.CreateFIXParam(WF_CAMPCODE.Text, "SEGMENT3")
                            Case "TAXKBN"                        '税区分
                                prmData = work.CreateFIXParam(WF_CAMPCODE.Text, "TAXKBN")
                        End Select

                        .SetListBox(WF_LeftMViewChange.Value, WW_DUMMY, prmData)
                        .ActiveListBox()
                End Select

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


    ' ******************************************************************************
    ' ***  leftBOX関連操作                                                       ***
    ' ******************************************************************************
    ''' <summary>
    ''' LeftBOX選択ボタン処理(ListBox値 ---> detailbox)
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonSel_Click()

        Dim WW_SelectTEXT As String = "0"
        Dim WW_SelectTEXT_LONG As String = "0"
        Dim WW_SelectValue As String = ""

        '選択内容を取得

        If leftview.WF_LeftListBox.SelectedIndex >= 0 Then
            WF_SelectedIndex.Value = leftview.WF_LeftListBox.SelectedIndex
            WW_SelectValue = leftview.WF_LeftListBox.Items(WF_SelectedIndex.Value).Value
            WW_SelectTEXT = leftview.WF_LeftListBox.Items(WF_SelectedIndex.Value).Text
        End If

        Debug.Print(WF_FIELD.Value)
        '選択内容を画面項目へセット
        '項目セット　＆　フォーカス
        If WF_FIELD_REP.Value = "" Then
            Select Case WF_FIELD.Value

                Case "WF_SELSHIWAKEPATERNKBN"   '仕訳パターン分類(絞り込み)
                    WF_SELSHIWAKEPATERNKBN_TEXT.Text = WW_SelectTEXT
                    WF_SELSHIWAKEPATERNKBN.Text = WW_SelectValue
                    WF_SELSHIWAKEPATERNKBN.Focus()

                Case "WF_SHIWAKEPATERNKBN"      '仕訳パターン分類
                    WF_SHIWAKEPATERNKBN_TEXT.Text = WW_SelectTEXT
                    WF_SHIWAKEPATERNKBN.Text = WW_SelectValue
                    WF_SHIWAKEPATERNKBN.Focus()

                Case "WF_SELACDCKBN"               '貸借区分
                    WF_SELACDCKBN_TEXT.Text = WW_SelectTEXT
                    WF_SELACDCKBN.Text = WW_SelectValue
                    WF_SELACDCKBN.Focus()

                Case "WF_ACDCKBN"               '貸借区分
                    WF_ACDCKBN_TEXT.Text = WW_SelectTEXT
                    WF_ACDCKBN.Text = WW_SelectValue
                    WF_ACDCKBN.Focus()

                Case "WF_STYMD"
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

                Case "WF_ENDYMD"
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

                Case "WF_DELFLG"
                    WF_DELFLG_TEXT.Text = WW_SelectTEXT
                    WF_DELFLG.Text = WW_SelectValue
                    WF_DELFLG.Focus()

            End Select
        Else
            '○ディテール01（管理）変数設定
            For Each reitem As RepeaterItem In WF_DViewRep1.Items
                '***********  左サイド　***********
                If CType(reitem.FindControl("WF_Rep1_FIELD_1"), Label).Text = WF_FIELD_REP.Value Then
                    CType(reitem.FindControl("WF_Rep1_VALUE_1"), TextBox).Text = WW_SelectValue
                    CType(reitem.FindControl("WF_Rep1_VALUE_TEXT_1"), Label).Text = WW_SelectTEXT
                    CType(reitem.FindControl("WF_Rep1_VALUE_1"), TextBox).Focus()
                    Exit For
                End If

                '***********  右サイド　***********
                If CType(reitem.FindControl("WF_Rep1_FIELD_3"), Label).Text = WF_FIELD_REP.Value Then
                    CType(reitem.FindControl("WF_Rep1_VALUE_3"), TextBox).Text = WW_SelectValue
                    CType(reitem.FindControl("WF_Rep1_VALUE_TEXT_3"), Label).Text = WW_SelectTEXT
                    CType(reitem.FindControl("WF_Rep1_VALUE_3"), TextBox).Focus()
                    Exit For
                End If

                '***********  中央　***********
                If CType(reitem.FindControl("WF_Rep1_FIELD_2"), Label).Text = WF_FIELD_REP.Value Then
                    CType(reitem.FindControl("WF_Rep1_VALUE_2"), TextBox).Text = WW_SelectValue
                    CType(reitem.FindControl("WF_Rep1_VALUE_TEXT_2"), Label).Text = WW_SelectTEXT
                    CType(reitem.FindControl("WF_Rep1_VALUE_2"), TextBox).Focus()
                    Exit For
                End If
            Next
        End If

        '○画面左サイドボックス非表示は、画面JavaScript(InitLoad)で実行
        WF_FIELD.Value = ""
        WF_FIELD_REP.Value = ""
        WF_LeftboxOpen.Value = ""
        WF_LeftMViewChange.Value = ""

    End Sub

    ''' <summary>
    ''' LeftBOXキャンセルボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonCan_Click()

        If WF_FIELD_REP.Value = "" Then
            '○フォーカスセット
            Select Case WF_FIELD.Value

                Case "WF_SELSHIWAKEPATERNKBN"       '仕訳パターン分類（絞り込み）
                    WF_SELSHIWAKEPATERNKBN.Focus()

                Case "WF_SELACDCKBN"                '貸借区分（絞り込み）
                    WF_SELACDCKBN.Focus()

                Case "WF_SHIWAKEPATERNKBN"          '仕訳パターン分類(キー部)
                    WF_SHIWAKEPATERNKBN.Focus()

                Case "WF_ACDCKBN"                   '貸借区分（キー部）
                    WF_ACDCKBN.Focus()

                Case "WF_STYMD"                     '有効年月日(キー部)
                    WF_STYMD.Focus()

                Case "WF_ENDYMD"                    '有効年月日(キー部)
                    WF_ENDYMD.Focus()

                Case "WF_DELFLG"                    '削除(キー部)
                    WF_DELFLG.Focus()

            End Select
        Else

            '○ディテール01（管理）変数設定
            For Each reitem As RepeaterItem In WF_DViewRep1.Items
                '***********  左サイド　***********
                If CType(reitem.FindControl("WF_Rep1_FIELD_1"), Label).Text = WF_FIELD_REP.Value Then
                    CType(reitem.FindControl("WF_Rep1_VALUE_1"), TextBox).Focus()
                    Exit For
                End If

                '***********  中央　***********
                If CType(reitem.FindControl("WF_Rep1_FIELD_2"), Label).Text = WF_FIELD_REP.Value Then
                    CType(reitem.FindControl("WF_Rep1_VALUE_2"), TextBox).Focus()
                    Exit For
                End If

                '***********  右サイド　***********
                If CType(reitem.FindControl("WF_Rep1_FIELD_3"), Label).Text = WF_FIELD_REP.Value Then
                    CType(reitem.FindControl("WF_Rep1_VALUE_3"), TextBox).Focus()
                    Exit For
                End If

            Next

        End If


        '○画面左サイドボックス非表示は、画面JavaScript(InitLoad)で実行
        WF_FIELD.Value = ""
        WF_FIELD_REP.Value = ""
        WF_LeftboxOpen.Value = ""
        WF_LeftMViewChange.Value = ""

    End Sub


    ''' <summary>
    ''' 右ボックスのラジオボタン選択時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_RadioButon_Click()
        '○RightBox処理（ラジオボタン選択）
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
        '○RightBox処理（右Boxメモ変更時）
        rightview.MAPID = Master.MAPID
        rightview.Save(Master.USERID, Master.USERTERMID, WW_DUMMY)
    End Sub


    ''' <summary>
    ''' 一覧画面-マウスホイール時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_GRID_ScroleDown()

    End Sub


    ''' <summary>
    ''' 一覧画面-マウスホイール時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_GRID_ScroleUp()

    End Sub


    ''' <summary>
    ''' 一覧画面-マウスホイール時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_GRID_Scrole()

    End Sub


    ''' <summary>
    ''' ファイルアップロード入力処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub UPLOAD_EXCEL()

        '○初期処理
        '○エラーレポート準備
        rightview.SetErrorReport("")

        Master.CreateEmptyTable(ML0003INPtbl)

        '○UPLOAD_XLSデータ取得        
        CS0023XLSUPLOAD.CAMPCODE = work.WF_SEL_CAMPCODE.Text
        CS0023XLSUPLOAD.MAPID = GRML0003WRKINC.MAPID
        CS0023XLSUPLOAD.CS0023XLSUPLOAD(String.Empty, Master.PROF_REPORT)
        If isNormal(CS0023XLSUPLOAD.ERR) Then
            If CS0023XLSUPLOAD.TBLDATA.Rows.Count = 0 Then
                Master.Output(C_MESSAGE_NO.REGISTRATION_RECORD_NOT_EXIST_ERROR, C_MESSAGE_TYPE.ERR)
                Exit Sub
            End If
        Else
            Master.Output(CS0023XLSUPLOAD.ERR, C_MESSAGE_TYPE.ERR, "CS0023XLSTBL")
            Exit Sub
        End If
        '○CS0023XLSTBL.TBLDATAの入力値整備
        Dim WW_COLUMNS As New List(Of String)
        For Each XLSTBLcol As DataColumn In CS0023XLSUPLOAD.TBLDATA.Columns
            WW_COLUMNS.Add(XLSTBLcol.ColumnName.ToString())
        Next

        Dim CS0023XLSTBLrow As DataRow = CS0023XLSUPLOAD.TBLDATA.NewRow
        For Each XLSTBLrow As DataRow In CS0023XLSUPLOAD.TBLDATA.Rows
            CS0023XLSTBLrow.ItemArray = XLSTBLrow.ItemArray

            For Each XLSTBLcol As DataColumn In CS0023XLSUPLOAD.TBLDATA.Columns
                If IsDBNull(CS0023XLSTBLrow.Item(XLSTBLcol)) OrElse IsNothing(CS0023XLSTBLrow.Item(XLSTBLcol)) Then
                    CS0023XLSTBLrow.Item(XLSTBLcol) = ""
                End If
            Next

            XLSTBLrow.ItemArray = CS0023XLSTBLrow.ItemArray
        Next

        '○必須列の判定
        If WW_COLUMNS.IndexOf("CAMPCODE") < 0 OrElse
           WW_COLUMNS.IndexOf("SHIWAKEPATERNKBN") < 0 OrElse
           WW_COLUMNS.IndexOf("SHIWAKEPATTERN") < 0 OrElse
           WW_COLUMNS.IndexOf("ACDCKBN") < 0 OrElse
           WW_COLUMNS.IndexOf("STYMD") < 0 Then
            ' インポート出来ません(項目： ?01 が存在しません)。
            Master.Output(C_MESSAGE_NO.IMPORT_ERROR, C_MESSAGE_TYPE.ERR, "Inport TITLE not find")
            Exit Sub
        End If

        '○Excelデータ毎にチェック＆更新
        For Each XLSTBLrow As DataRow In CS0023XLSUPLOAD.TBLDATA.Rows
            '○XLSTBL明細⇒ML0003INProw
            Dim ML0003INProw = ML0003INPtbl.NewRow

            '初期クリア
            For Each ML0003INPcol As DataColumn In ML0003INPtbl.Columns

                If IsDBNull(ML0003INProw.Item(ML0003INPcol)) OrElse IsNothing(ML0003INProw.Item(ML0003INPcol)) Then
                    Select Case ML0003INPcol.ColumnName
                        Case "LINECNT"
                            ML0003INProw.Item(ML0003INPcol) = 0
                        Case "TIMSTP"
                            ML0003INProw.Item(ML0003INPcol) = 0
                        Case "SELECT"
                            ML0003INProw.Item(ML0003INPcol) = 1
                        Case "HIDDEN"
                            ML0003INProw.Item(ML0003INPcol) = 0
                        Case "SEQ"
                            ML0003INProw.Item(ML0003INPcol) = 0
                        Case Else
                            If ML0003INPcol.DataType.Name = "String" Then
                                ML0003INProw.Item(ML0003INPcol) = ""
                            ElseIf ML0003INPcol.DataType.Name = "DateTime" Then
                                ML0003INProw.Item(ML0003INPcol) = C_DEFAULT_YMD
                            Else
                                ML0003INProw.Item(ML0003INPcol) = 0
                            End If
                    End Select
                End If
            Next

            '○変更元情報をデフォルト設定
            Dim WW_STYMD As String = ""

            If WW_COLUMNS.IndexOf("CAMPCODE") >= 0 AndAlso
               WW_COLUMNS.IndexOf("SHIWAKEPATERNKBN") >= 0 AndAlso
               WW_COLUMNS.IndexOf("SHIWAKEPATTERN") >= 0 AndAlso
               WW_COLUMNS.IndexOf("ACDCKBN") >= 0 AndAlso
               WW_COLUMNS.IndexOf("STYMD") >= 0 Then

                For Each ML0003row As DataRow In ML0003tbl.Rows
                    If XLSTBLrow("CAMPCODE") = ML0003row("CAMPCODE") AndAlso
                       XLSTBLrow("SHIWAKEPATERNKBN") = ML0003row("SHIWAKEPATERNKBN") AndAlso
                       XLSTBLrow("SHIWAKEPATTERN") = ML0003row("SHIWAKEPATTERN") AndAlso
                       XLSTBLrow("ACDCKBN") = ML0003row("ACDCKBN") AndAlso
                       XLSTBLrow("STYMD") = ML0003row("STYMD") Then
                        ML0003INProw.ItemArray = ML0003row.ItemArray
                        Exit For
                    End If
                Next
            End If

            '○項目セット
            '会社コード
            If WW_COLUMNS.IndexOf("CAMPCODE") >= 0 Then
                ML0003INProw("CAMPCODE") = XLSTBLrow("CAMPCODE")
            End If

            '会社名
            If WW_COLUMNS.IndexOf("CAMPNAMES") >= 0 Then
                ML0003INProw("CAMPNAMES") = XLSTBLrow("CAMPNAMES")
            End If


            '仕訳パターン分類
            If WW_COLUMNS.IndexOf("SHIWAKEPATERNKBN") >= 0 Then
                ML0003INProw("SHIWAKEPATERNKBN") = XLSTBLrow("SHIWAKEPATERNKBN")
            End If


            '仕訳パターン分類名
            If WW_COLUMNS.IndexOf("SHIWAKEPATERNKBNNAMES") >= 0 Then
                ML0003INProw("SHIWAKEPATERNKBNNAMES") = XLSTBLrow("SHIWAKEPATERNKBNNAMES")
            End If

            '仕訳パターン
            If WW_COLUMNS.IndexOf("SHIWAKEPATTERN") >= 0 Then
                ML0003INProw("SHIWAKEPATTERN") = XLSTBLrow("SHIWAKEPATTERN")
            End If

            '仕訳パターン名
            If WW_COLUMNS.IndexOf("SHIWAKEPATERNNAME") >= 0 Then
                ML0003INProw("SHIWAKEPATERNNAME") = XLSTBLrow("SHIWAKEPATERNNAME")
            End If

            '貸借区分
            If WW_COLUMNS.IndexOf("ACDCKBN") >= 0 Then
                ML0003INProw("ACDCKBN") = XLSTBLrow("ACDCKBN")
            End If

            '貸借区分名
            If WW_COLUMNS.IndexOf("ACDCKBNNAMES") >= 0 Then
                ML0003INProw("ACDCKBNNAMES") = XLSTBLrow("ACDCKBNNAMES")
            End If

            '勘定科目
            If WW_COLUMNS.IndexOf("ACCODE") >= 0 Then
                ML0003INProw("ACCODE") = XLSTBLrow("ACCODE")
            End If


            '勘定科目名
            If WW_COLUMNS.IndexOf("ACCODENAMES") >= 0 Then
                ML0003INProw("ACCODENAMES") = XLSTBLrow("ACCODENAMES")
            End If


            '照会区分（画面）
            If WW_COLUMNS.IndexOf("INQKBN") >= 0 Then
                ML0003INProw("INQKBN") = XLSTBLrow("INQKBN")
            End If


            '照会区分（画面）名
            If WW_COLUMNS.IndexOf("INQKBNNAMES") >= 0 Then
                ML0003INProw("INQKBNNAMES") = XLSTBLrow("INQKBNNAMES")
            End If


            '取引先
            If WW_COLUMNS.IndexOf("TORICODE") >= 0 Then
                ML0003INProw("TORICODE") = XLSTBLrow("TORICODE")
            End If


            '取引先名
            If WW_COLUMNS.IndexOf("TORICODENAMES") >= 0 Then
                ML0003INProw("TORICODENAMES") = XLSTBLrow("TORICODENAMES")
            End If


            '銀行コード
            If WW_COLUMNS.IndexOf("BANKCODE") >= 0 Then
                ML0003INProw("BANKCODE") = XLSTBLrow("BANKCODE")
            End If


            '銀行コード名
            If WW_COLUMNS.IndexOf("BANKCODENAMES") >= 0 Then
                ML0003INProw("BANKCODENAMES") = XLSTBLrow("BANKCODENAMES")
            End If


            'セグメント１
            If WW_COLUMNS.IndexOf("SEGMENT1") >= 0 Then
                ML0003INProw("SEGMENT1") = XLSTBLrow("SEGMENT1")
            End If


            'セグメント１名
            If WW_COLUMNS.IndexOf("SEGMENT1NAMES") >= 0 Then
                ML0003INProw("SEGMENT1NAMES") = XLSTBLrow("SEGMENT1NAMES")
            End If


            'セグメント２
            If WW_COLUMNS.IndexOf("SEGMENT2") >= 0 Then
                ML0003INProw("SEGMENT2") = XLSTBLrow("SEGMENT2")
            End If


            'セグメント２名
            If WW_COLUMNS.IndexOf("SEGMENT2NAMES") >= 0 Then
                ML0003INProw("SEGMENT2NAMES") = XLSTBLrow("SEGMENT2NAMES")
            End If


            'セグメント３
            If WW_COLUMNS.IndexOf("SEGMENT3") >= 0 Then
                ML0003INProw("SEGMENT3") = XLSTBLrow("SEGMENT3")
            End If


            'セグメント３名
            If WW_COLUMNS.IndexOf("SEGMENT3NAMES") >= 0 Then
                ML0003INProw("SEGMENT3NAMES") = XLSTBLrow("SEGMENT3NAMES")
            End If


            '税区分
            If WW_COLUMNS.IndexOf("TAXKBN") >= 0 Then
                ML0003INProw("TAXKBN") = XLSTBLrow("TAXKBN")
            End If


            '税区分名
            If WW_COLUMNS.IndexOf("TAXKBNNAMES") >= 0 Then
                ML0003INProw("TAXKBNNAMES") = XLSTBLrow("TAXKBNNAMES")
            End If


            '有効開始日
            If WW_COLUMNS.IndexOf("STYMD") >= 0 Then
                If IsDate(XLSTBLrow("STYMD")) Then
                    Dim WW_DATE As Date
                    Date.TryParse(XLSTBLrow("STYMD"), WW_DATE)
                    ML0003INProw("STYMD") = WW_DATE.ToString("yyyy/MM/dd")
                End If
            End If

            '有効終了日
            If WW_COLUMNS.IndexOf("ENDYMD") >= 0 Then
                If IsDate(XLSTBLrow("ENDYMD")) Then
                    Dim WW_DATE As Date
                    Date.TryParse(XLSTBLrow("ENDYMD"), WW_DATE)
                    ML0003INProw("ENDYMD") = WW_DATE.ToString("yyyy/MM/dd")
                End If
            End If

            '削除
            If WW_COLUMNS.IndexOf("DELFLG") >= 0 Then
                ML0003INProw("DELFLG") = XLSTBLrow("DELFLG")
            End If

            ML0003INPtbl.Rows.Add(ML0003INProw)
        Next

        '○項目チェック
        INPtbl_Check(WW_ERRCODE)

        '○画面表示テーブル更新
        If isNormal(WW_ERRCODE) Then
            ML0003tbl_UPD()
        End If

        '○画面表示データ保存
        Master.SaveTable(ML0003tbl)

        'エラー編集
        If isNormal(WW_ERRCODE) Then
            Master.Output(C_MESSAGE_NO.NORMAL, C_MESSAGE_TYPE.NOR)
        Else
            Master.Output(WW_ERRCODE, C_MESSAGE_TYPE.ERR)
        End If

        'detailboxクリア
        Detailbox_Clear()

        CS0023XLSUPLOAD.TBLDATA.Dispose()
        CS0023XLSUPLOAD.TBLDATA.Clear()

    End Sub


    ' ******************************************************************************
    ' ***  共通処理                                                              ***
    ' ******************************************************************************

    ''' <summary>
    ''' 条件抽出画面情報退避
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub MAPrefelence()

        '○選択画面の入力初期値設定
        If Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.ML0003S Then

            Master.MAPID = GRML0003WRKINC.MAPID
            '○Grid情報保存先のファイル名
            Master.CreateXMLSaveFile()

            '会社コード表示
            WF_CAMPCODE.Text = work.WF_SEL_CAMPCODE.Text
            CODENAME_get("CAMPCODE", WF_CAMPCODE.Text, WF_CAMPCODE_TEXT.Text, WW_DUMMY)
        End If

    End Sub


    ''' <summary>
    ''' 画面データ取得
    ''' </summary>
    ''' <remarks>データベース（MC013_UNCHINKETEI）を検索し画面表示する一覧を作成する</remarks>
    Private Sub MAPDATAget()

        '○画面表示用データ取得

        Try
            'MC0010テンポラリDB項目作成
            If ML0003tbl Is Nothing Then
                ML0003tbl = New DataTable
            End If

            If ML0003tbl.Columns.Count <> 0 Then
                ML0003tbl.Columns.Clear()
            End If

            '○DB項目クリア
            ML0003tbl.Clear()

            '○テーブル検索結果をテーブル退避
            'ML0003テンポラリDB項目作成

            'DataBase接続文字
            Using SQLcon As SqlConnection = CS0050SESSION.getConnection
                SQLcon.Open()       'DataBase接続(Open)

                '検索SQL文
                '　検索説明
                '     条件指定に従い該当データを荷主運賃決定マスタから取得する
                '　注意事項　日付について
                '　　権限判断はすべてDateNow。グループコード、名称取得は全てDateNow。表追加時の①はDateNow。
                '　　但し、表追加時の②および③は、TBL入力有効期限。

                Dim SQLStr As String =
                      " SELECT  0                                      as LINECNT               , " _
                    & "         ''                                     as OPERATION             , " _
                    & "         TIMSTP = cast(isnull(ML03.UPDTIMSTP,0) as bigint)               , " _
                    & "         1                                      as 'SELECT'              , " _
                    & "         0                                      as HIDDEN                , " _
                    & "         rtrim(ML03.CAMPCODE)                   as CAMPCODE              , " _
                    & "         ''                                     as CAMPNAMES             , " _
                    & "         rtrim(ML03.SHIWAKEPATERNKBN)           as SHIWAKEPATERNKBN      , " _
                    & "         ''                                     as SHIWAKEPATERNKBNNAMES , " _
                    & "         rtrim(ML03.SHIWAKEPATTERN)             as SHIWAKEPATTERN        , " _
                    & "         rtrim(ML03.SHIWAKEPATERNNAME)          as SHIWAKEPATERNNAME     , " _
                    & "         rtrim(ML03.ACDCKBN)                    as ACDCKBN               , " _
                    & "         ''                                     as ACDCKBNNAMES          , " _
                    & "         format(ML03.STYMD, 'yyyy/MM/dd')       as STYMD                 , " _
                    & "         format(ML03.ENDYMD, 'yyyy/MM/dd')      as ENDYMD                , " _
                    & "         rtrim(ML03.ACCODE)                     as ACCODE                , " _
                    & "         ''                                     as ACCODENAMES           , " _
                    & "         rtrim(ML03.INQKBN   )                  as INQKBN                , " _
                    & "         ''                                     as INQKBNNAMES           , " _
                    & "         rtrim(ML03.TORICODE   )                as TORICODE                , " _
                    & "         ''                                     as TORICODENAMES           , " _
                    & "         rtrim(ML03.BANKCODE)                   as BANKCODE              , " _
                    & "         ''                                     as BANKCODENAMES         , " _
                    & "         rtrim(ML03.SEGMENT1)                   as SEGMENT1              , " _
                    & "         ''                                     as SEGMENT1NAMES         , " _
                    & "         rtrim(ML03.SEGMENT2)                   as SEGMENT2              , " _
                    & "         ''                                     as SEGMENT2NAMES         , " _
                    & "         rtrim(ML03.SEGMENT3)                   as SEGMENT3              , " _
                    & "         ''                                     as SEGMENT3NAMES         , " _
                    & "         rtrim(ML03.TAXKBN)                     as TAXKBN                , " _
                    & "         ''                                     as TAXKBNNAMES           , " _
                    & "         rtrim(ML03.DELFLG)                     as DELFLG                , " _
                    & "         ''                                     as INITYMD               , " _
                    & "         ''                                     as UPDYMD                , " _
                    & "         ''                                     as UPDUSER               , " _
                    & "         ''                                     as UPDTERMID             , " _
                    & "         ''                                     as RECEIVEYMD            , " _
                    & "         ''                                     as UPDTIMSTP               " _
                    & " FROM                                                                      " _
                    & "           ML003_SHIWAKEPATTERN ML03                                       " _
                    & " WHERE                                                                     " _
                    & "           ML03.CAMPCODE    = @P1                                          "

                '仕訳パターン分類が入力されていた場合は条件にセット
                If work.WF_SEL_SHIWAKEPATERNKBN.Text.Length <> 0 Then
                    SQLStr += "      and  ML03.SHIWAKEPATERNKBN    = @P2                          "
                End If

                '貸借区分が入力されていた場合は条件にセット
                If work.WF_SEL_ACDCKBN.Text.Length <> 0 Then
                    SQLStr += "      and  ML03.ACDCKBN = @P3                                    "
                End If

                SQLStr += "  and  ML03.STYMD      <= @P4                                        " _
                    & "      and  ML03.ENDYMD     >= @P5                                        " _
                    & "      and  ML03.DELFLG     <> '1'                                        " _
                    & " ORDER BY                                                                " _
                    & "      ML03.CAMPCODE, ML03.SHIWAKEPATERNKBN, ML03.SHIWAKEPATTERN,         " _
                    & "      ML03.ACDCKBN,  ML03.STYMD       "

                Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                    Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", SqlDbType.NVarChar, 20)
                    Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", SqlDbType.NVarChar, 20)
                    Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", SqlDbType.NVarChar, 1)
                    Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", SqlDbType.Date)
                    Dim PARA5 As SqlParameter = SQLcmd.Parameters.Add("@P5", SqlDbType.Date)

                    PARA1.Value = work.WF_SEL_CAMPCODE.Text
                    PARA2.Value = work.WF_SEL_SHIWAKEPATERNKBN.Text
                    PARA3.Value = work.WF_SEL_ACDCKBN.Text
                    PARA4.Value = work.WF_SEL_ENDYMD.Text
                    PARA5.Value = work.WF_SEL_STYMD.Text

                    Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                        'フィールド名とフィールドの型を取得
                        For index As Integer = 0 To SQLdr.FieldCount - 1
                            ML0003tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                        Next

                        '○ テーブル検索結果をテーブル格納
                        ML0003tbl.Load(SQLdr)

                        For Each ML0003row As DataRow In ML0003tbl.Rows
                            '会社名を取得
                            CODENAME_get("CAMPCODE", ML0003row("CAMPCODE"), ML0003row("CAMPNAMES"), WW_DUMMY)
                            '仕訳パターン分類名を取得(固定値マスタ)
                            CODENAME_get("SHIWAKEPATERNKBN", ML0003row("SHIWAKEPATERNKBN"), ML0003row("SHIWAKEPATERNKBNNAMES"), WW_DUMMY)
                            '貸借区分名を取得(固定値マスタ)
                            CODENAME_get("ACDCKBN", ML0003row("ACDCKBN"), ML0003row("ACDCKBNNAMES"), WW_DUMMY)
                            '勘定科目名を取得
                            CODENAME_get("ACCODE", ML0003row("ACCODE"), ML0003row("ACCODENAMES"), WW_DUMMY)
                            '照会区分名を取得(固定値マスタ)
                            CODENAME_get("INQKBN", ML0003row("INQKBN"), ML0003row("INQKBNNAMES"), WW_DUMMY)
                            '取引先名を取得
                            CODENAME_get("TORICODE", ML0003row("TORICODE"), ML0003row("TORICODENAMES"), WW_DUMMY)
                            '銀行コード名を取得
                            CODENAME_get("BANKCODE", ML0003row("BANKCODE"), ML0003row("BANKCODENAMES"), WW_DUMMY)
                            'セグメント1名を取得(固定値マスタ)
                            CODENAME_get("SEGMENT1", ML0003row("SEGMENT1"), ML0003row("SEGMENT1NAMES"), WW_DUMMY)
                            'セグメント2名を取得(固定値マスタ)
                            CODENAME_get("SEGMENT2", ML0003row("SEGMENT2"), ML0003row("SEGMENT2NAMES"), WW_DUMMY)
                            'セグメント3名を取得(固定値マスタ)
                            CODENAME_get("SEGMENT3", ML0003row("SEGMENT3"), ML0003row("SEGMENT3NAMES"), WW_DUMMY)
                            '税区分名を取得(固定値マスタ)
                            CODENAME_get("TAXKBN", ML0003row("TAXKBN"), ML0003row("TAXKBNNAMES"), WW_DUMMY)
                        Next

                    End Using
                End Using
            End Using
        Catch ex As Exception
            'ログ出力
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "ML003_SHIWAKEPATTERN SELECT")
            CS0011LOGWRITE.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:ML003_SHIWAKEPATTERN Select"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try

        '○ 画面表示データソート
        CS0026TBLSORT.COMPCODE = work.WF_SEL_CAMPCODE.Text
        CS0026TBLSORT.PROFID = Master.PROF_VIEW
        CS0026TBLSORT.MAPID = Master.MAPID
        CS0026TBLSORT.VARI = Master.VIEWID
        CS0026TBLSORT.TABLE = ML0003tbl
        CS0026TBLSORT.TAB = ""
        CS0026TBLSORT.FILTER = ""
        CS0026TBLSORT.SortandNumbring()
        If isNormal(CS0026TBLSORT.ERR) Then
            ML0003tbl = CS0026TBLSORT.TABLE
        End If

    End Sub


    ''' <summary>
    ''' 入力値チェック
    ''' </summary>
    ''' <param name="O_RTNCODE"></param>
    ''' <remarks></remarks>
    Protected Sub INPtbl_Check(ByRef O_RTNCODE As String)

        O_RTNCODE = C_MESSAGE_NO.NORMAL
        rightview.SetErrorReport("")

        Dim WW_LINEERR_SW As String = ""
        Dim WW_DUMMY As String = ""
        Dim WW_TEXT As String = ""
        Dim WW_CheckMES1 As String = ""
        Dim WW_CheckMES2 As String = ""
        Dim WW_CS0024FCHECKERR As String = ""
        Dim WW_CS0024FCHECKREPORT As String = ""

        '○権限チェック(操作者がデータ内USERの更新権限があるかチェック
        CS0025AUTHORget.USERID = CS0050SESSION.USERID
        CS0025AUTHORget.OBJCODE = C_ROLE_VARIANT.USER_PERTMIT
        CS0025AUTHORget.CODE = Master.MAPID
        CS0025AUTHORget.STYMD = Date.Now
        CS0025AUTHORget.ENDYMD = Date.Now
        CS0025AUTHORget.CS0025AUTHORget()
        If isNormal(CS0025AUTHORget.ERR) AndAlso CS0025AUTHORget.PERMITCODE = C_PERMISSION.UPDATE Then
        Else
            WW_CheckMES1 = "・ユーザ更新権限なしです。"
            WW_CheckMES2 = ""
            O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            WW_LINEERR_SW = "ERR"
            Exit Sub
        End If

        For Each ML0003INProw As DataRow In ML0003INPtbl.Rows

            WW_LINEERR_SW = ""
            '○単項目チェック(会社コード)
            WW_TEXT = ML0003INProw("CAMPCODE")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "CAMPCODE", ML0003INProw("CAMPCODE"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '存在チェック
                If WW_TEXT = "" Then
                    ML0003INProw("CAMPCODE") = ""
                Else
                    CODENAME_get("CAMPCODE", ML0003INProw("CAMPCODE"), WW_DUMMY, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・更新できないレコード(会社エラー)です。"
                        WW_CheckMES2 = ""
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, ML0003INProw)
                        O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                        WW_LINEERR_SW = "ERR"
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(会社コードエラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, ML0003INProw)
                O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                WW_LINEERR_SW = "ERR"
            End If


            '○単項目チェッ(仕訳パターン分類)
            WW_TEXT = ML0003INProw("SHIWAKEPATERNKBN")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "SHIWAKEPATERNKBN", ML0003INProw("SHIWAKEPATERNKBN"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '存在チェック
                If WW_TEXT = "" Then
                    ML0003INProw("SHIWAKEPATERNKBN") = ""
                Else
                    CODENAME_get("SHIWAKEPATERNKBN", ML0003INProw("SHIWAKEPATERNKBN"), WW_DUMMY, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・更新できないレコード(仕訳パターン分類エラー)です。"
                        WW_CheckMES2 = ""
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, ML0003INProw)
                        O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                        WW_LINEERR_SW = "ERR"
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(仕訳パターン分類エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, ML0003INProw)
                O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                WW_LINEERR_SW = "ERR"
            End If


            '○単項目チェック(貸借区分)
            WW_TEXT = ML0003INProw("ACDCKBN")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "ACDCKBN", ML0003INProw("ACDCKBN"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '存在チェック
                If WW_TEXT = "" Then
                    ML0003INProw("ACDCKBN") = ""
                Else
                    CODENAME_get("ACDCKBN", ML0003INProw("ACDCKBN"), WW_DUMMY, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・更新できないレコード(貸借区分エラー)です。"
                        WW_CheckMES2 = ""
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, ML0003INProw)
                        O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                        WW_LINEERR_SW = "ERR"
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(貸借区分エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, ML0003INProw)
                O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                WW_LINEERR_SW = "ERR"
            End If


            '○単項目チェック(仕訳パターン)
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "SHIWAKEPATTERN", ML0003INProw("SHIWAKEPATTERN"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(仕訳パターンエラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, ML0003INProw)
                O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                WW_LINEERR_SW = "ERR"
            End If


            '○単項目チェック(有効開始日付)
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "STYMD", ML0003INProw("STYMD"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(有効日付：開始エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, ML0003INProw)
                O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                WW_LINEERR_SW = "ERR"
            End If

            '○単項目チェック(有効終了日付)
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "ENDYMD", ML0003INProw("ENDYMD"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(有効日付：終了エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, ML0003INProw)
                O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                WW_LINEERR_SW = "ERR"
            End If


            '○単項目チェック(DELFLG)
            If ML0003INProw("DELFLG") = "" OrElse ML0003INProw("DELFLG") = C_DELETE_FLG.ALIVE OrElse ML0003INProw("DELFLG") = C_DELETE_FLG.DELETE Then
                If ML0003INProw("DELFLG") = "" Then
                    ML0003INProw("DELFLG") = C_DELETE_FLG.ALIVE
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(削除CD不正)です。"
                WW_CheckMES2 = ""
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, ML0003INProw)
                O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                WW_LINEERR_SW = "ERR"
            End If


            '○単項目チェック(仕訳パターン名)
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "SHIWAKEPATERNNAME", ML0003INProw("SHIWAKEPATERNNAME"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(仕訳パターン名エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, ML0003INProw)
                O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                WW_LINEERR_SW = "ERR"
            End If


            '○単項目チェック(勘定科目)
            WW_TEXT = ML0003INProw("ACCODE")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "ACCODE", ML0003INProw("ACCODE"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '存在チェック
                If WW_TEXT = "" Then
                    ML0003INProw("ACCODE") = ""
                Else
                    CODENAME_get("ACCODE", ML0003INProw("ACCODE"), WW_DUMMY, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・更新できないレコード(勘定科目エラー)です。"
                        WW_CheckMES2 = ""
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, ML0003INProw)
                        O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                        WW_LINEERR_SW = "ERR"
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(勘定科目エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, ML0003INProw)
                O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                WW_LINEERR_SW = "ERR"
            End If


            '○単項目チェック(照会区分)
            WW_TEXT = ML0003INProw("INQKBN")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "INQKBN", ML0003INProw("INQKBN"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '存在チェック
                If WW_TEXT = "" Then
                    ML0003INProw("INQKBN") = ""
                Else
                    CODENAME_get("INQKBN", ML0003INProw("INQKBN"), WW_DUMMY, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・更新できないレコード(照会区分エラー)です。"
                        WW_CheckMES2 = ""
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, ML0003INProw)
                        O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                        WW_LINEERR_SW = "ERR"
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(照会区分エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, ML0003INProw)
                O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                WW_LINEERR_SW = "ERR"
            End If


            '○単項目チェック(取引先)
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "TORICODE", ML0003INProw("TORICODE"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '存在チェック
                If WW_TEXT = "" Then
                    ML0003INProw("TORICODE") = ""
                Else
                    'CODENAME_get("TORICODE", ML0003INProw("TORICODE"), WW_DUMMY, WW_RTN_SW)
                    'If Not isNormal(WW_RTN_SW) Then
                    '    WW_CheckMES1 = "・更新できないレコード(取引先コードエラー)です。"
                    '    WW_CheckMES2 = ""
                    '    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, ML0003INProw)
                    '    O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    '    WW_LINEERR_SW = "ERR"
                    'End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(取引先コードエラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, ML0003INProw)
                O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                WW_LINEERR_SW = "ERR"
            End If


            '○単項目チェック(銀行コード)
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "BANKCODE", ML0003INProw("BANKCODE"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(銀行エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, ML0003INProw)
                O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                WW_LINEERR_SW = "ERR"
            End If


            '○単項目チェック(セグメント1)
            WW_TEXT = ML0003INProw("SEGMENT1")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "SEGMENT1", ML0003INProw("SEGMENT1"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '存在チェック
                If WW_TEXT = "" Then
                    ML0003INProw("SEGMENT1") = ""
                Else
                    CODENAME_get("SEGMENT1", ML0003INProw("SEGMENT1"), WW_DUMMY, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・更新できないレコード(セグメント1エラー)です。"
                        WW_CheckMES2 = ""
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, ML0003INProw)
                        O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                        WW_LINEERR_SW = "ERR"
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(セグメント1エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, ML0003INProw)
                O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                WW_LINEERR_SW = "ERR"
            End If


            '○単項目チェック(セグメント2)
            WW_TEXT = ML0003INProw("SEGMENT2")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "SEGMENT2", ML0003INProw("SEGMENT2"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '存在チェック
                If WW_TEXT = "" Then
                    ML0003INProw("SEGMENT2") = ""
                Else
                    CODENAME_get("SEGMENT2", ML0003INProw("SEGMENT2"), WW_DUMMY, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・更新できないレコード(セグメント2エラー)です。"
                        WW_CheckMES2 = ""
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, ML0003INProw)
                        O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                        WW_LINEERR_SW = "ERR"
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(セグメント2エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, ML0003INProw)
                O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                WW_LINEERR_SW = "ERR"
            End If


            '○単項目チェック(セグメント3)
            WW_TEXT = ML0003INProw("SEGMENT3")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "SEGMENT3", ML0003INProw("SEGMENT3"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '存在チェック
                If WW_TEXT = "" Then
                    ML0003INProw("SEGMENT3") = ""
                Else
                    CODENAME_get("SEGMENT3", ML0003INProw("SEGMENT3"), WW_DUMMY, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・更新できないレコード(セグメント3エラー)です。"
                        WW_CheckMES2 = ""
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, ML0003INProw)
                        O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                        WW_LINEERR_SW = "ERR"
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(セグメント3エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, ML0003INProw)
                O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                WW_LINEERR_SW = "ERR"
            End If


            '○単項目チェック(税区分)
            WW_TEXT = ML0003INProw("TAXKBN")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "TAXKBN", ML0003INProw("TAXKBN"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '存在チェック
                If WW_TEXT = "" Then
                    ML0003INProw("TAXKBN") = ""
                Else
                    CODENAME_get("TAXKBN", ML0003INProw("TAXKBN"), WW_DUMMY, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・更新できないレコード(税区分エラー)です。"
                        WW_CheckMES2 = ""
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, ML0003INProw)
                        O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                        WW_LINEERR_SW = "ERR"
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(税区分エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, ML0003INProw)
                O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                WW_LINEERR_SW = "ERR"
            End If


            '○操作設定
            If WW_LINEERR_SW = "" Then
                If ML0003INProw("OPERATION") <> C_LIST_OPERATION_CODE.ERRORED Then
                    ML0003INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                End If
            Else
                ML0003INProw("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End If
        Next

    End Sub


    ''' <summary>
    ''' 登録データ関連チェック
    ''' </summary>
    ''' <param name="O_RTNCODE"></param>
    ''' <remarks></remarks>
    Protected Sub RelatedCheck(ByRef O_RTNCODE As String)

        O_RTNCODE = C_MESSAGE_NO.NORMAL
        rightview.SetErrorReport("")

        Dim WW_LINEERR_SW As String = ""
        Dim WW_DUMMY As String = ""
        Dim WW_CheckMES1 As String = ""
        Dim WW_CheckMES2 As String = ""
        Dim WW_CS0024FCHECKERR As String = ""
        Dim WW_CS0024FCHECKREPORT As String = ""

        Dim WW_DATE_ST As Date
        Dim WW_DATE_END As Date
        Dim WW_DATE_ST2 As Date
        Dim WW_DATE_END2 As Date

        '○関連チェック
        For Each ML0003INProw As DataRow In ML0003tbl.Rows

            '読み飛ばし
            If (ML0003INProw("OPERATION") <> C_LIST_OPERATION_CODE.UPDATING AndAlso
                ML0003INProw("OPERATION") <> C_LIST_OPERATION_CODE.ERRORED) OrElse
                ML0003INProw("DELFLG") = C_DELETE_FLG.DELETE OrElse
                ML0003INProw("STYMD") < C_DEFAULT_YMD Then
                Continue For
            End If

            WW_LINEERR_SW = ""

            'チェック
            For Each ML0003row As DataRow In ML0003tbl.Rows

                '日付以外の項目が等しい
                If ML0003INProw("CAMPCODE") = ML0003row("CAMPCODE") AndAlso
                   ML0003INProw("SHIWAKEPATERNKBN") = ML0003row("SHIWAKEPATERNKBN") AndAlso
                   ML0003INProw("SHIWAKEPATTERN") = ML0003row("SHIWAKEPATTERN") AndAlso
                   ML0003INProw("ACDCKBN") = ML0003row("ACDCKBN") AndAlso
                   ML0003row("DELFLG") <> C_DELETE_FLG.DELETE Then
                Else
                    Continue For
                End If

                '期間変更対象は読み飛ばし
                If ML0003INProw("STYMD") = ML0003row("STYMD") Then
                    Continue For
                End If

                Try
                    Date.TryParse(ML0003INProw("STYMD"), WW_DATE_ST)
                    Date.TryParse(ML0003INProw("ENDYMD"), WW_DATE_END)
                    Date.TryParse(ML0003row("STYMD"), WW_DATE_ST2)
                    Date.TryParse(ML0003row("ENDYMD"), WW_DATE_END2)
                Catch ex As Exception
                End Try

                ''開始日チェック
                'If (WW_DATE_ST >= WW_DATE_ST2 AndAlso WW_DATE_ST <= WW_DATE_END2) Then
                '    WW_CheckMES1 = "・エラー(期間重複)が存在します。"
                '    WW_CheckMES2 = ""
                '    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, ML0003row)
                '    O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                '    WW_LINEERR_SW = "ERR"
                '    Exit For
                'End If

                ''終了日チェック
                'If (WW_DATE_END >= WW_DATE_ST2 AndAlso WW_DATE_END <= WW_DATE_END2) Then
                '    WW_CheckMES1 = "・エラー(期間重複)が存在します。"
                '    WW_CheckMES2 = ""
                '    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, ML0003row)
                '    O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                '    WW_LINEERR_SW = "ERR"
                '    Exit For
                'End If

            Next

            If WW_LINEERR_SW = "" Then
                ML0003INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
            Else
                ML0003INProw("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End If
        Next

    End Sub


    ''' <summary>
    ''' 更新予定データ登録・更新処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub ML0003tbl_UPD()

        '○操作表示クリア
        For Each ML0003row As DataRow In ML0003tbl.Rows
            Select Case ML0003row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    ML0003row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.NODISP
                    ML0003row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED
                    ML0003row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    ML0003row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    ML0003row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End Select
        Next

        '○追加変更判定
        For Each ML0003INProw As DataRow In ML0003INPtbl.Rows

            'エラーレコード読み飛ばし
            If ML0003INProw("OPERATION") <> C_LIST_OPERATION_CODE.UPDATING Then
                Continue For
            End If

            '初期判定セット
            ML0003INProw("OPERATION") = "Insert"

            For Each ML0003row As DataRow In ML0003tbl.Rows

                If ML0003INProw("CAMPCODE") = ML0003row("CAMPCODE") AndAlso
                   ML0003INProw("SHIWAKEPATERNKBN") = ML0003row("SHIWAKEPATERNKBN") AndAlso
                   ML0003INProw("SHIWAKEPATTERN") = ML0003row("SHIWAKEPATTERN") AndAlso
                   ML0003INProw("ACDCKBN") = ML0003row("ACDCKBN") AndAlso
                   ML0003INProw("STYMD") = ML0003row("STYMD") Then
                Else
                    Continue For
                End If

                'レコード内容に変更があったか判定
                If ML0003row("CAMPCODE") = ML0003INProw("CAMPCODE") AndAlso
                   ML0003row("CAMPNAMES") = ML0003INProw("CAMPNAMES") AndAlso
                   ML0003row("SHIWAKEPATERNKBN") = ML0003INProw("SHIWAKEPATERNKBN") AndAlso
                   ML0003row("SHIWAKEPATERNKBNNAMES") = ML0003INProw("SHIWAKEPATERNKBNNAMES") AndAlso
                   ML0003row("SHIWAKEPATTERN") = ML0003INProw("SHIWAKEPATTERN") AndAlso
                   ML0003row("SHIWAKEPATERNNAME") = ML0003INProw("SHIWAKEPATERNNAME") AndAlso
                   ML0003row("ACDCKBN") = ML0003INProw("ACDCKBN") AndAlso
                   ML0003row("ACDCKBNNAMES") = ML0003INProw("ACDCKBNNAMES") AndAlso
                   ML0003row("STYMD") = ML0003INProw("STYMD") AndAlso
                   ML0003row("ENDYMD") = ML0003INProw("ENDYMD") AndAlso
                   ML0003row("ACCODE") = ML0003INProw("ACCODE") AndAlso
                   ML0003row("ACCODENAMES") = ML0003INProw("ACCODENAMES") AndAlso
                   ML0003row("INQKBN") = ML0003INProw("INQKBN") AndAlso
                   ML0003row("INQKBNNAMES") = ML0003INProw("INQKBNNAMES") AndAlso
                   ML0003row("TORICODE") = ML0003INProw("TORICODE") AndAlso
                   ML0003row("TORICODENAMES") = ML0003INProw("TORICODENAMES") AndAlso
                   ML0003row("BANKCODE") = ML0003INProw("BANKCODE") AndAlso
                   ML0003row("BANKCODENAMES") = ML0003INProw("BANKCODENAMES") AndAlso
                   ML0003row("SEGMENT1") = ML0003INProw("SEGMENT1") AndAlso
                   ML0003row("SEGMENT1NAMES") = ML0003INProw("SEGMENT1NAMES") AndAlso
                   ML0003row("SEGMENT2") = ML0003INProw("SEGMENT2") AndAlso
                   ML0003row("SEGMENT2NAMES") = ML0003INProw("SEGMENT2NAMES") AndAlso
                   ML0003row("SEGMENT3") = ML0003INProw("SEGMENT3") AndAlso
                   ML0003row("SEGMENT3NAMES") = ML0003INProw("SEGMENT3NAMES") AndAlso
                   ML0003row("TAXKBN") = ML0003INProw("TAXKBN") AndAlso
                   ML0003row("TAXKBNNAMES") = ML0003INProw("TAXKBNNAMES") AndAlso
                   ML0003row("DELFLG") = ML0003INProw("DELFLG") Then

                    ML0003INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Else
                    '○更新（Update）
                    TBL_Update_SUB(ML0003INProw, ML0003row)
                End If

                Exit For

            Next

            '○ML0003追加処理
            If ML0003INProw("OPERATION") = "Insert" Then
                '○更新（Insert）
                TBL_Insert_SUB(ML0003INProw)
            End If
        Next

    End Sub


    ''' <summary>
    ''' 更新予定データの一覧更新時処理
    ''' </summary>
    ''' <param name="INProw"></param>
    ''' <remarks></remarks>
    Protected Sub TBL_Update_SUB(ByVal INProw As DataRow, ByRef UPDRow As DataRow)

        INProw("LINECNT") = UPDRow("LINECNT")
        INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
        INProw("TIMSTP") = UPDRow("TIMSTP")
        INProw("SELECT") = 1
        INProw("HIDDEN") = 0

        '○ML0003変更処理
        UPDRow.ItemArray = INProw.ItemArray
        If UPDRow("DELFLG") = "" Then
            UPDRow("DELFLG") = C_DELETE_FLG.ALIVE
        Else
            UPDRow("DELFLG") = UPDRow("DELFLG")
        End If

    End Sub


    ''' <summary>
    ''' 更新予定データの一覧登録時処理
    ''' </summary>
    ''' <param name="INProw"></param>
    ''' <remarks></remarks>
    Protected Sub TBL_Insert_SUB(ByRef INProw As DataRow)

        INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING

        '○ML0003追加処理
        Dim ML0003row As DataRow = ML0003tbl.NewRow
        ML0003row.ItemArray = INProw.ItemArray

        ML0003row("LINECNT") = ML0003tbl.Rows.Count + 1
        ML0003row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
        ML0003row("TIMSTP") = 0
        ML0003row("SELECT") = 1
        ML0003row("HIDDEN") = 0
        ML0003tbl.Rows.Add(ML0003row)

    End Sub

    ' ******************************************************************************
    ' ***  サブルーチン                                                          ***
    ' ******************************************************************************

    ''' <summary>
    ''' エラーレポート編集
    ''' </summary>
    ''' <param name="I_MESSAGE1"></param>
    ''' <param name="I_MESSAGE2"></param>
    ''' <param name="I_ERRCD"></param>
    ''' <remarks></remarks>
    Protected Sub WW_CheckERR(ByRef I_MESSAGE1 As String, ByRef I_MESSAGE2 As String, ByVal I_ERRCD As String, ByVal ML0003INProw As DataRow)

        Dim WW_ERR_MES As String = ""
        WW_ERR_MES = I_MESSAGE1
        If I_MESSAGE2 <> "" Then
            WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> " & I_MESSAGE2 & " , "
        End If
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 会社コード　　　 =" & ML0003INProw("CAMPCODE") & " , "
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 仕訳パターン分類 =" & ML0003INProw("SHIWAKEPATERNKBN") & " , "
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 仕訳パターン     =" & ML0003INProw("SHIWAKEPATTERN") & " , "
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 仕訳パターン名　 =" & ML0003INProw("SHIWAKEPATERNNAME") & " , "
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 貸借区分　　     =" & ML0003INProw("ACDCKBN") & " , "
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 開始年月日　　　　　　=" & ML0003INProw("STYMD") & " , "
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 終了年月日　　　　　　=" & ML0003INProw("ENDYMD") & " , "
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 削除フラグ　　　　　　=" & ML0003INProw("DELFLG") & " "
        rightview.AddErrorReport(WW_ERR_MES)

    End Sub


    ''' <summary>
    ''' LeftBoxより名称取得＆チェック
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
            With leftview
                Select Case I_FIELD
                    Case "CAMPCODE"             '会社
                        .CodeToName(LIST_BOX_CLASSIFICATION.LC_COMPANY, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text))

                    Case "SHIWAKEPATERNKBN"     '仕訳パターン分類(固定値マスタ)
                        .CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "SHIWAKEPATERNKBN"))

                    Case "ACDCKBN"              '貸借区分(固定値マスタ)
                        .CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "ACDCKBN"))

                    Case "ACCODE"               '勘定科目
                        .CodeToName(LIST_BOX_CLASSIFICATION.LC_ACCODE, I_VALUE, O_TEXT, O_RTN, work.CreateACCParam(work.WF_SEL_CAMPCODE.Text, ""))

                    Case "INQKBN"               '照会区分（画面）
                        .CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "INQKBN"))

                    'Case "TORICODE"             '取引先
                    '    .CodeToName(LIST_BOX_CLASSIFICATION.LC_CUSTOMER, I_VALUE, O_TEXT, O_RTN, work.CreateTORIParam(work.WF_SEL_CAMPCODE.Text))

                    'Case "BANKCODE"             '銀行コード
                    '    .CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "BANKCODE"))

                    Case "SEGMENT1"             'セグメント1(固定値マスタ)
                        .CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "SEGMENT1"))

                    Case "SEGMENT2"             'セグメント2(固定値マスタ)
                        .CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "SEGMENT2"))

                    Case "SEGMENT3"   　        'セグメント3(固定値マスタ)
                        .CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "SEGMENT3"))

                    Case "TAXKBN"  　           '税区分(固定値マスタ)
                        .CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "TAXKBN"))

                    Case "DELFLG"       　   '削除フラグ名称
                        .CodeToName(LIST_BOX_CLASSIFICATION.LC_DELFLG, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "DELFLG"))


                    Case Else
                        O_TEXT = ""                                                             '該当項目なし

                End Select
            End With
        End If
    End Sub

End Class
