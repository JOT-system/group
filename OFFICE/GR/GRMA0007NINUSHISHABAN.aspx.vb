Imports System.Data.SqlClient
Imports OFFICE.GRIS0005LeftBox
Imports BASEDLL

''' <summary>
''' 荷主車番マスタ（登録）
''' </summary>
''' <remarks></remarks>
Public Class GRMA0007NINUSHISHABAN
    Inherits Page

    '検索結果格納
    Private MA0007tbl As DataTable                              'Grid格納用テーブル
    Private MA0007INPtbl As DataTable                           'チェック用テーブル

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
                    If Not Master.RecoverTable(MA0007tbl) Then
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
            If Not IsNothing(MA0007tbl) Then
                MA0007tbl.Clear()
                MA0007tbl.Dispose()
                MA0007tbl = Nothing
            End If

            If Not IsNothing(MA0007INPtbl) Then
                MA0007INPtbl.Clear()
                MA0007INPtbl.Dispose()
                MA0007INPtbl = Nothing
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
        Master.SaveTable(MA0007tbl)

        '一覧表示データ編集（性能対策）
        Using TBLview As DataView = New DataView(MA0007tbl)
            TBLview.RowFilter = "LINECNT >= 1 and LINECNT <= " & CONST_DSPROWCOUNT
            CS0013ProfView.CAMPCODE = work.WF_SEL_CAMPCODE.Text
            CS0013ProfView.PROFID = Master.PROF_VIEW
            CS0013ProfView.MAPID = GRMA0007WRKINC.MAPID
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

        '○ 画面の値設定
        WW_MAPValueSet()

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
        For Each MA0007row As DataRow In MA0007tbl.Rows
            If MA0007row("HIDDEN") = 0 Then
                WW_DataCNT = WW_DataCNT + 1
                '行（ラインカウント）を再設定する。既存項目（SELECT）を利用
                MA0007row("SELECT") = WW_DataCNT
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
        Dim WW_TBLview As DataView = New DataView(MA0007tbl)

        'ソート
        WW_TBLview.Sort = "LINECNT"
        WW_TBLview.RowFilter = "HIDDEN = 0 and SELECT >= " & WW_GridPosition.ToString() & " and SELECT < " & (WW_GridPosition + CONST_DSPROWCOUNT).ToString()
        '一覧作成

        CS0013ProfView.CAMPCODE = work.WF_SEL_CAMPCODE.Text
        CS0013ProfView.PROFID = Master.PROF_VIEW
        CS0013ProfView.MAPID = GRMA0007WRKINC.MAPID
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



    ''' <summary>
    ''' 画面初期値設定処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_MAPValueSet()

        '○ 検索画面からの遷移
        If Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.MC0012S Then
            'Grid情報保存先のファイル名
            Master.CreateXMLSaveFile()
        End If

        '○ 名称設定処理
        CODENAME_get("CAMPCODE", work.WF_SEL_CAMPCODE.Text, WF_SEL_CAMPNAME.Text, WW_DUMMY)         '会社コード
        CODENAME_get("TORICODE", work.WF_SEL_TORICODE.Text, WF_SEL_TORICODE_TEXT.Text, WW_DUMMY)    '取引先コード

    End Sub


    ' ******************************************************************************
    ' ***  絞り込みボタン処理                                                    ***
    ' ******************************************************************************
    Protected Sub WF_ButtonExtract_Click()

        '○画面表示データ保存
        Master.SaveTable(MA0007tbl)

        '○画面表示
        '画面先頭を表示
        WF_GridPosition.Text = "1"

        '○メッセージ表示
        Master.Output(C_MESSAGE_NO.DATA_FILTER_SUCCESSFUL, C_MESSAGE_TYPE.INF)

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
            Master.SaveTable(MA0007tbl)
            Exit Sub
        End If
        Try
            Using SQLcon As SqlConnection = CS0050SESSION.getConnection
                SQLcon.Open()       'DataBase接続(Open)

                Dim SQLStr As String =
                      " DECLARE @hensuu as bigint ;                                                                    " _
                    & " set @hensuu = 0 ;                                                                              " _
                    & " DECLARE hensuu CURSOR FOR                                                                      " _
                    & "   SELECT CAST(UPDTIMSTP as bigint) as hensuu                                                   " _
                    & "     FROM    MA007_NINUSHISHABAN                                                                " _
                    & "     WHERE CAMPCODE =@P01 and TORICODE = @P02 and UNCHINFUNCCODE = @P03 and NSHABAN = @P04      " _
                    & "       and STYMD =@P05 ;　　　　　　　　　　　　　　　                                          " _
                    & " OPEN hensuu ;                                                                                  " _
                    & " FETCH NEXT FROM hensuu INTO @hensuu ;                                                          " _
                    & " IF ( @@FETCH_STATUS = 0 )                                                                      " _
                    & "    UPDATE   MA007_NINUSHISHABAN                                                                " _
                    & "       SET                                                                                      " _
                    & "         ENDYMD = @P06                                                                          " _
                    & "       , UNCHINSHAFUKU = @P07                                                                   " _
                    & "       , SHARYOKEIYAKUCODE = @P08                                                               " _
                    & "       , SHAFUKU = @P09                                                                         " _
                    & " 　　  , SHAGATA = @P10                                                                         " _
                    & " 　　  , CONTENASTATE = @P11                                                                    " _
                    & " 　　  , SUPPL = @P12                                                                           " _
                    & "       , DELFLG = @P13                                                                          " _
                    & "       , UPDYMD = @P15                                                                          " _
                    & "       , UPDUSER = @P16                                                                         " _
                    & "       , UPDTERMID    = @P17                                                                    " _
                    & "       , RECEIVEYMD   = @P18                                                                    " _
                    & "     WHERE CAMPCODE =@P01 and TORICODE = @P02 and UNCHINFUNCCODE = @P03 and NSHABAN = @P04      " _
                    & "       and STYMD =@P05 ;　　　　　　　　　　　　　　　                                          " _
                    & " IF ( @@FETCH_STATUS <> 0 )                                                                     " _
                    & "    INSERT INTO MA007_NINUSHISHABAN                                                              " _
                    & "       ( CAMPCODE                                                                               " _
                    & "       , TORICODE                                                                               " _
                    & "       , UNCHINFUNCCODE                                                                         " _
                    & "       , NSHABAN                                                                                " _
                    & "       , STYMD                                                                                  " _
                    & "       , ENDYMD                                                                                 " _
                    & "       , UNCHINSHAFUKU                                                                          " _
                    & "       , SHARYOKEIYAKUCODE                                                                      " _
                    & "       , SHAFUKU                                                                                " _
                    & " 　　  , SHAGATA                                                                                " _
                    & " 　　  , CONTENASTATE                                                                           " _
                    & " 　　  , SUPPL                                                                                  " _
                    & "       , DELFLG                                                                                 " _
                    & "       , INITYMD                                                                                " _
                    & "       , UPDYMD                                                                                 " _
                    & "       , UPDUSER                                                                                " _
                    & "       , UPDTERMID                                                                              " _
                    & "       , RECEIVEYMD )                                                                           " _
                    & "      VALUES (@P01,@P02,@P03,@P04,@P05,@P06,@P07,@P08,@P09,@P10,@P11,@P12,@P13,@P14,@P15,@P16   " _
                    & "             ,@P17,@P18) ;" _
                    & " CLOSE hensuu ;                                                                                 " _
                    & " DEALLOCATE hensuu ;                                                                            "

                Dim SQLStr1 As String =
                      " Select  CAMPCODE      , TORICODE          , UNCHINFUNCCODE, NSHABAN     , STYMD       , ENDYMD," _
                    & "         UNCHINSHAFUKU , SHARYOKEIYAKUCODE , SHAFUKU       , SHAGATA     , CONTENASTATE, SUPPL ," _
                    & "         DELFLG        , INITYMD           , UPDYMD        , UPDUSER     , UPDTERMID   ,        " _
                    & "         RECEIVEYMD    , CAST(UPDTIMSTP As bigint) As TIMSTP " _
                    & " FROM  MA007_NINUSHISHABAN " _
                    & "     WHERE CAMPCODE =@P01 and TORICODE = @P02 and UNCHINFUNCCODE = @P03 and NSHABAN = @P04      " _
                    & "       and STYMD =@P05 ;　　　　　　　　　　　　　　　                                          "

                Using SQLcmd As New SqlCommand(SQLStr, SQLcon), SQLcmd1 As New SqlCommand(SQLStr1, SQLcon)
                    Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", SqlDbType.NVarChar)          'CAMPCODE
                    Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", SqlDbType.NVarChar)          'TORICODE
                    Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@P03", SqlDbType.NVarChar)          'UNCHINFUNCCODE
                    Dim PARA04 As SqlParameter = SQLcmd.Parameters.Add("@P04", SqlDbType.NVarChar)          'NSHABAN
                    Dim PARA05 As SqlParameter = SQLcmd.Parameters.Add("@P05", SqlDbType.Date)              'STYMD
                    Dim PARA06 As SqlParameter = SQLcmd.Parameters.Add("@P06", SqlDbType.Date)              'ENDYMD
                    Dim PARA07 As SqlParameter = SQLcmd.Parameters.Add("@P07", SqlDbType.NVarChar)          'UNCHINSHAFUKU
                    Dim PARA08 As SqlParameter = SQLcmd.Parameters.Add("@P08", SqlDbType.NVarChar)          'SHARYOKEIYAKUCODE
                    Dim PARA09 As SqlParameter = SQLcmd.Parameters.Add("@P09", SqlDbType.NVarChar)          'SHAFUKU
                    Dim PARA10 As SqlParameter = SQLcmd.Parameters.Add("@P10", SqlDbType.NVarChar)          'SHAGATA
                    Dim PARA11 As SqlParameter = SQLcmd.Parameters.Add("@P11", SqlDbType.NVarChar)          'CONTENASTATE
                    Dim PARA12 As SqlParameter = SQLcmd.Parameters.Add("@P12", SqlDbType.NVarChar)          'SUPPL
                    Dim PARA13 As SqlParameter = SQLcmd.Parameters.Add("@P13", SqlDbType.NVarChar)          'DELFLG
                    Dim PARA14 As SqlParameter = SQLcmd.Parameters.Add("@P14", SqlDbType.SmallDateTime)     'INITYMD
                    Dim PARA15 As SqlParameter = SQLcmd.Parameters.Add("@P15", SqlDbType.DateTime)          'UPDYMD
                    Dim PARA16 As SqlParameter = SQLcmd.Parameters.Add("@P16", SqlDbType.NVarChar)          'UPDUSER
                    Dim PARA17 As SqlParameter = SQLcmd.Parameters.Add("@P17", SqlDbType.NVarChar)          'UPDTERMID
                    Dim PARA18 As SqlParameter = SQLcmd.Parameters.Add("@P18", SqlDbType.DateTime)          'RECEIVEYMD

                    Dim PARAS01 As SqlParameter = SQLcmd1.Parameters.Add("@P01", SqlDbType.NVarChar)         'CAMPCODE
                    Dim PARAS02 As SqlParameter = SQLcmd1.Parameters.Add("@P02", SqlDbType.NVarChar)         'TORICODE
                    Dim PARAS03 As SqlParameter = SQLcmd1.Parameters.Add("@P03", SqlDbType.NVarChar)         'UNCHINFUNCCODE
                    Dim PARAS04 As SqlParameter = SQLcmd1.Parameters.Add("@P04", SqlDbType.NVarChar)         'NSHABAN
                    Dim PARAS05 As SqlParameter = SQLcmd1.Parameters.Add("@P05", SqlDbType.Date)             'STYMD

                    '○ＤＢ更新
                    For Each MA0007row As DataRow In MA0007tbl.Rows
                        If Trim(MA0007row("OPERATION")) = C_LIST_OPERATION_CODE.UPDATING OrElse
                           Trim(MA0007row("OPERATION")) = C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING Then
                            '※追加レコードは、 MA0007tbl.Rows(i)("TIMSTP") = "0"となっているが状態のみで判定

                            PARA01.Value = MA0007row("CAMPCODE")
                            PARA02.Value = MA0007row("TORICODE")
                            PARA03.Value = MA0007row("UNCHINFUNCCODE")
                            PARA04.Value = MA0007row("NSHABAN")
                            PARA05.Value = MA0007row("STYMD")
                            PARA06.Value = MA0007row("ENDYMD")
                            PARA07.Value = MA0007row("UNCHINSHAFUKU")
                            PARA08.Value = MA0007row("SHARYOKEIYAKUCODE")
                            PARA09.Value = MA0007row("SHAFUKU")
                            PARA10.Value = MA0007row("SHAGATA")
                            PARA11.Value = MA0007row("CONTENASTATE")
                            PARA12.Value = MA0007row("SUPPL")
                            PARA13.Value = MA0007row("DELFLG")
                            PARA14.Value = Date.Now
                            PARA15.Value = Date.Now
                            PARA16.Value = Master.USERID
                            PARA17.Value = Master.USERTERMID
                            PARA18.Value = C_DEFAULT_YMD

                            SQLcmd.ExecuteNonQuery()

                            MA0007row("OPERATION") = C_LIST_OPERATION_CODE.NODATA

                            '○更新ジャーナル追加
                            Try
                                PARAS01.Value = MA0007row("CAMPCODE")
                                PARAS02.Value = MA0007row("TORICODE")
                                PARAS03.Value = MA0007row("UNCHINFUNCCODE")
                                PARAS04.Value = MA0007row("NSHABAN")
                                PARAS05.Value = MA0007row("STYMD")

                                Dim JOURds As New DataSet()
                                Dim SQLadp As SqlDataAdapter

                                SQLadp = New SqlDataAdapter(SQLcmd1)
                                SQLadp.Fill(JOURds, "JOURtbl")

                                CS0020JOURNAL.TABLENM = "MA007_NINUSHISHABAN"
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

                                MA0007row("TIMSTP") = JOURds.Tables("JOURtbl").Rows(0)("TIMSTP")

                                SQLadp.Dispose()
                                SQLadp = Nothing
                            Catch ex As Exception
                                If ex.Message = "Error raised In TIMSTP" Then
                                    MA0007row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                                End If
                                Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "MA007_NINUSHISHABAN JOURNAL")

                                CS0011LOGWRITE.INFSUBCLASS = "MAIN"                         'SUBクラス名
                                CS0011LOGWRITE.INFPOSI = "DB:MA007_NINUSHISHABAN JOURNAL"
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
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "MA007_NINUSHISHABAN UPDATE_INSERT")
            CS0011LOGWRITE.INFSUBCLASS = "MAIN"
            CS0011LOGWRITE.INFPOSI = "DB:MA007_NINUSHISHABAN UPDATE_INSERT"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()

            Exit Sub
        End Try

        '○画面表示データ保存
        Master.SaveTable(MA0007tbl)

        '詳細画面クリア
        Detailbox_Clear()

        '○メッセージ表示
        Master.Output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF)

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
        CS0030REPORl.MAPID = GRMA0007WRKINC.MAPID
        CS0030REPORl.REPORTID = rightview.GetReportId()
        CS0030REPORl.FILEtyp = "pdf"
        CS0030REPORl.TBLDATA = MA0007tbl
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
        CS0030REPORl.MAPID = GRMA0007WRKINC.MAPID
        CS0030REPORl.PROFID = Master.PROF_REPORT
        CS0030REPORl.REPORTID = rightview.GetReportId()
        CS0030REPORl.FILEtyp = "XLSX"
        CS0030REPORl.TBLDATA = MA0007tbl
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
        WW_TBLview = New DataView(MA0007tbl)
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
        Dim WK_SHIPORG As String = ""
        Dim WF_SHIWAKEPATERNKBN As String = ""

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
        WF_Sel_LINECNT.Text = MA0007tbl.Rows(WW_Position)("LINECNT")
        WF_CAMPCODE.Text = MA0007tbl.Rows(WW_Position)("CAMPCODE")
        WF_CAMPCODE_TEXT.Text = MA0007tbl.Rows(WW_Position)("CAMPNAMES")
        WF_TORICODE.Text = MA0007tbl.Rows(WW_Position)("TORICODE")
        WF_TORICODE_TEXT.Text = MA0007tbl.Rows(WW_Position)("TORICODENAMES")
        WF_UNCHINFUNCCODE.Text = MA0007tbl.Rows(WW_Position)("UNCHINFUNCCODE")
        WF_UNCHINFUNCCODE_TEXT.Text = MA0007tbl.Rows(WW_Position)("UNCHINFUNCCODENAMES")
        WF_NSHABAN.Text = MA0007tbl.Rows(WW_Position)("NSHABAN")

        '有効年月日
        WF_STYMD.Text = MA0007tbl.Rows(WW_Position)("STYMD")
        WF_ENDYMD.Text = MA0007tbl.Rows(WW_Position)("ENDYMD")
        '削除フラグ
        WF_DELFLG.Text = MA0007tbl.Rows(WW_Position)("DELFLG")
        CODENAME_get("DELFLG", WF_DELFLG.Text, WW_TEXT, WW_DUMMY)
        WF_DELFLG_TEXT.Text = WW_TEXT

        WW_TEXT = ""

        '○Grid設定処理
        For Each reitem As RepeaterItem In WF_DViewRep1.Items
            '左
            WW_FILED_OBJ = CType(reitem.FindControl("WF_Rep1_FIELD_1"), Label)

            If WW_FILED_OBJ.Text <> "" Then
                '値設定
                WW_VALUE = WF_ITEM_FORMAT(WW_FILED_OBJ.text, MA0007tbl.Rows(WW_LINECNT)(WW_FILED_OBJ.Text))

                CType(reitem.FindControl("WF_Rep1_VALUE_1"), TextBox).Text = WW_VALUE

                '値（名称）設定
                CODENAME_get(WW_FILED_OBJ.Text, WW_VALUE, WW_TEXT, WW_DUMMY)

                CType(reitem.FindControl("WF_Rep1_VALUE_TEXT_1"), Label).Text = WW_TEXT

            End If

            '中央
            WW_FILED_OBJ = CType(reitem.FindControl("WF_Rep1_FIELD_2"), Label)
            If WW_FILED_OBJ.Text <> "" Then
                '値設定
                WW_VALUE = WF_ITEM_FORMAT(WW_FILED_OBJ.text, MA0007tbl.Rows(WW_LINECNT)(WW_FILED_OBJ.Text))
                CType(reitem.FindControl("WF_Rep1_VALUE_2"), TextBox).Text = WW_VALUE

                '値（名称）設定
                CODENAME_get(WW_FILED_OBJ.Text, WW_VALUE, WW_TEXT, WW_DUMMY)

                CType(reitem.FindControl("WF_Rep1_VALUE_TEXT_2"), Label).Text = WW_TEXT
            End If

            '右
            WW_FILED_OBJ = CType(reitem.FindControl("WF_Rep1_FIELD_3"), Label)
            If WW_FILED_OBJ.Text <> "" Then
                '値設定
                WW_VALUE = WF_ITEM_FORMAT(WW_FILED_OBJ.text, MA0007tbl.Rows(WW_LINECNT)(WW_FILED_OBJ.Text))
                CType(reitem.FindControl("WF_Rep1_VALUE_3"), TextBox).Text = WW_VALUE

                '値（名称）設定
                CODENAME_get(WW_FILED_OBJ.Text, WW_VALUE, WW_TEXT, WW_DUMMY)

                CType(reitem.FindControl("WF_Rep1_VALUE_TEXT_3"), Label).Text = WW_TEXT
            End If
        Next

        '○画面WF_GRID状態設定
        '状態をクリア設定
        For Each MA0007row As DataRow In MA0007tbl.Rows
            Select Case MA0007row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    MA0007row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.NODISP
                    MA0007row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED
                    MA0007row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    MA0007row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    MA0007row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End Select
        Next

        '選択明細のOperation項目に状態を設定(更新・追加・削除は編集中を設定しない)
        Select Case MA0007tbl.Rows(WW_Position)("OPERATION")
            Case C_LIST_OPERATION_CODE.NODATA
                MA0007tbl.Rows(WW_Position)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
            Case C_LIST_OPERATION_CODE.NODISP
                MA0007tbl.Rows(WW_Position)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
            Case C_LIST_OPERATION_CODE.SELECTED
                MA0007tbl.Rows(WW_Position)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
            Case C_LIST_OPERATION_CODE.UPDATING
                MA0007tbl.Rows(WW_Position)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
            Case C_LIST_OPERATION_CODE.ERRORED
                MA0007tbl.Rows(WW_Position)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
            Case Else
        End Select

        '○画面切替設定
        WF_BOXChange.Value = "detailbox"

        '○画面表示データ保存
        Master.SaveTable(MA0007tbl)

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

        '○DetailBoxをMA0007INPtblへ退避
        Master.CreateEmptyTable(MA0007INPtbl)
        DetailBoxToMA0007INPtbl(WW_ERRCODE)
        If Not isNormal(WW_ERRCODE) Then
            Exit Sub
        End If

        '○項目チェック
        INPtbl_Check(WW_ERRCODE)

        '○GridView更新
        If isNormal(WW_ERRCODE) Then
            MA0007tbl_UPD()
        End If

        '○一覧( MA0007tbl)内で、新規追加（タイムスタンプ０）かつ削除の場合はレコード削除
        If isNormal(WW_ERRCODE) Then
            Dim WW_DEL As String = "ON"
            Do
                For i As Integer = 0 To MA0007tbl.Rows.Count - 1
                    If MA0007tbl.Rows(i)("TIMSTP") = 0 AndAlso MA0007tbl.Rows(i)("DELFLG") = C_DELETE_FLG.DELETE Then
                        MA0007tbl.Rows(i).Delete()
                        WW_DEL = "OFF"
                        Exit For
                    Else
                        If (MA0007tbl.Rows.Count - 1) <= i Then
                            WW_DEL = "ON"
                        End If
                    End If
                Next
            Loop Until WW_DEL = "ON"
        End If

        '○画面表示データ保存
        Master.SaveTable(MA0007tbl)

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

    End Sub


    ''' <summary>
    '''  詳細画面-テーブル退避
    ''' </summary>
    ''' <param name="O_RTNCODE"></param>
    ''' <remarks></remarks>
    Protected Sub DetailBoxToMA0007INPtbl(ByRef O_RTNCODE As String)

        Dim WW_TEXT As String = String.Empty
        Dim WW_RTN As String = String.Empty

        O_RTNCODE = C_MESSAGE_NO.NORMAL

        'MA0007テンポラリDB項目作成
        Master.CreateEmptyTable(MA0007INPtbl)

        '○入力文字置き換え & CS0007CHKテーブルレコード追加

        '○ 画面(Repeaterヘッダー情報)の使用禁止文字排除
        Master.EraseCharToIgnore(WF_CAMPCODE.Text)          '会社コード
        Master.EraseCharToIgnore(WF_TORICODE.Text)          '取引先コード
        Master.EraseCharToIgnore(WF_UNCHINFUNCCODE.Text)    '運賃計算機能コード
        Master.EraseCharToIgnore(WF_NSHABAN.Text)           '荷主車番
        Master.EraseCharToIgnore(WF_STYMD.Text)             '開始年月日
        Master.EraseCharToIgnore(WF_ENDYMD.Text)            '終了年月日
        Master.EraseCharToIgnore(WF_DELFLG.Text)            '削除フラグ

        'GridViewから未選択状態で表更新ボタンを押下時の例外を回避する 
        If String.IsNullOrEmpty(WF_Sel_LINECNT.Text) AndAlso
            String.IsNullOrEmpty(WF_TORICODE.Text) AndAlso
            String.IsNullOrEmpty(WF_UNCHINFUNCCODE.Text) AndAlso
            String.IsNullOrEmpty(WF_NSHABAN.Text) AndAlso
            String.IsNullOrEmpty(WF_STYMD.Text) AndAlso
            String.IsNullOrEmpty(WF_ENDYMD.Text) AndAlso
            String.IsNullOrEmpty(WF_DELFLG.Text) Then
            Master.Output(C_MESSAGE_NO.INVALID_PROCCESS_ERROR, C_MESSAGE_TYPE.ERR, "no Detail")
            CS0011LOGWRITE.INFSUBCLASS = "DetailBoxToMA0007INPtbl"      'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "non Detail"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ERR
            CS0011LOGWRITE.TEXT = "non Detail"
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.INVALID_PROCCESS_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力

            O_RTNCODE = C_MESSAGE_NO.INVALID_PROCCESS_ERROR

            Exit Sub
        End If

        '○画面(Repeaterヘッダー情報)のテーブル退避
        Dim MA0007INProw As DataRow = MA0007INPtbl.NewRow
        '初期クリア
        For Each MA0007INPcol As DataColumn In MA0007INProw.Table.Columns
            If MA0007INPcol.DataType.Name.ToString() = "String" Then
                MA0007INProw(MA0007INPcol.ColumnName) = ""
            End If
        Next

        If (String.IsNullOrEmpty(WF_Sel_LINECNT.Text)) Then
            MA0007INProw("LINECNT") = 0
        Else
            MA0007INProw("LINECNT") = CType(WF_Sel_LINECNT.Text, Integer)   'DBの固定フィールド
        End If
        MA0007INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA            'DBの固定フィールド
        MA0007INProw("TIMSTP") = 0                                          'DBの固定フィールド
        MA0007INProw("SELECT") = "0"                                        'DBの固定フィールド
        MA0007INProw("HIDDEN") = "0"                                        'DBの固定フィールド

        MA0007INProw("CAMPCODE") = WF_CAMPCODE.Text
        MA0007INProw("TORICODE") = WF_TORICODE.Text
        MA0007INProw("UNCHINFUNCCODE") = WF_UNCHINFUNCCODE.Text
        MA0007INProw("NSHABAN") = WF_NSHABAN.Text
        MA0007INProw("STYMD") = WF_STYMD.Text
        MA0007INProw("ENDYMD") = WF_ENDYMD.Text
        MA0007INProw("DELFLG") = WF_DELFLG.Text


        '○Detail設定処理
        For Each reitem As RepeaterItem In WF_DViewRep1.Items
            '左
            If CType(reitem.FindControl("WF_Rep1_FIELD_1"), Label).Text <> "" Then
                CS0010CHARstr.CHARIN = CType(reitem.FindControl("WF_Rep1_VALUE_1"), TextBox).Text
                CS0010CHARstr.CS0010CHARget()
                MA0007INProw(CType(reitem.FindControl("WF_Rep1_FIELD_1"), Label).Text) = CS0010CHARstr.CHAROUT
            End If

            '中央
            If CType(reitem.FindControl("WF_Rep1_FIELD_2"), Label).Text <> "" Then
                CS0010CHARstr.CHARIN = CType(reitem.FindControl("WF_Rep1_VALUE_2"), TextBox).Text
                CS0010CHARstr.CS0010CHARget()
                MA0007INProw(CType(reitem.FindControl("WF_Rep1_FIELD_2"), Label).Text) = CS0010CHARstr.CHAROUT
            End If

            '右
            If CType(reitem.FindControl("WF_Rep1_FIELD_3"), Label).Text <> "" Then
                CS0010CHARstr.CHARIN = CType(reitem.FindControl("WF_Rep1_VALUE_3"), TextBox).Text
                CS0010CHARstr.CS0010CHARget()
                MA0007INProw(CType(reitem.FindControl("WF_Rep1_FIELD_3"), Label).Text) = CS0010CHARstr.CHAROUT
            End If
        Next

        '○コード名称を設定する
        ' 会社コード
        WW_TEXT = ""
        CODENAME_get("CAMPCODE", MA0007INProw("CAMPCODE"), WW_TEXT, WW_DUMMY)
        MA0007INProw("CAMPNAMES") = WW_TEXT

        ' 取引先コード
        WW_TEXT = ""
        CODENAME_get("TORICODE", MA0007INProw("TORICODE"), WW_TEXT, WW_DUMMY)
        MA0007INProw("TORICODENAMES") = WW_TEXT

        ' 運賃計算機能コード
        WW_TEXT = ""
        CODENAME_get("UNCHINFUNCCODE", MA0007INProw("UNCHINFUNCCODE"), WW_TEXT, WW_DUMMY)
        MA0007INProw("UNCHINFUNCCODENAMES") = WW_TEXT


        ' 車両契約内容コード
        WW_TEXT = ""
        CODENAME_get("SHARYOKEIYAKUCODE", MA0007INProw("SHARYOKEIYAKUCODE"), WW_TEXT, WW_DUMMY)
        MA0007INProw("SHARYOKEIYAKUCODENAMES") = WW_TEXT

        ' 車型
        WW_TEXT = ""
        CODENAME_get("SHAGATA", MA0007INProw("SHAGATA"), WW_TEXT, WW_DUMMY)
        MA0007INProw("SHAGATANAMES") = WW_TEXT

        ' コンテナ状態
        WW_TEXT = ""
        CODENAME_get("CONTENASTATE", MA0007INProw("CONTENASTATE"), WW_TEXT, WW_DUMMY)
        MA0007INProw("CONTENASTATENAMES") = WW_TEXT

        ' 用車会社
        WW_TEXT = ""
        CODENAME_get("SUPPL", MA0007INProw("SUPPL"), WW_TEXT, WW_DUMMY)
        MA0007INProw("SUPPLNAMES") = WW_TEXT


        ' チェック用テーブルに登録する
        MA0007INPtbl.Rows.Add(MA0007INProw)

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

    End Sub


    ''' <summary>
    ''' 詳細画面-クリア処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub Detailbox_Clear()

        '○画面WF_GRID状態設定
        '状態をクリア設定
        For Each MA0007row As DataRow In MA0007tbl.Rows
            Select Case MA0007row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    MA0007row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.NODISP
                    MA0007row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED
                    MA0007row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    MA0007row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    MA0007row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End Select
        Next

        '○画面表示データ保存
        Master.SaveTable(MA0007tbl)

        '画面(Grid)のHIDDEN列により、表示/非表示を行う。

        WF_Sel_LINECNT.Text = ""
        WF_CAMPCODE.Text = work.WF_SEL_CAMPCODE.Text
        CODENAME_get("CAMPCODE", WF_CAMPCODE.Text, WF_CAMPCODE_TEXT.Text, WW_DUMMY)
        WF_TORICODE.Text = ""
        WF_TORICODE_TEXT.Text = ""
        WF_UNCHINFUNCCODE.Text = ""
        WF_UNCHINFUNCCODE_TEXT.Text = ""
        WF_NSHABAN.Text = ""
        WF_NSHABAN_TEXT.Text = ""
        WF_STYMD.Text = ""
        WF_ENDYMD.Text = ""
        WF_DELFLG_TEXT.Text = ""
        WF_DELFLG.Text = ""
        WF_SEQ.Value = ""

        '○Detail初期設定
        Repeater_INIT()

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
            Case "TORICODE"
                ' 取引先コード
                O_ATTR = "REF_Field_DBclick('TORICODE', 'WF_Rep_FIELD' , '" & LIST_BOX_CLASSIFICATION.LC_CUSTOMER & "');"

            Case "SHARYOKEIYAKUCODE"
                ' 車両契約内容コード
                O_ATTR = "REF_Field_DBclick('SHARYOKEIYAKUCODE', 'WF_Rep_FIELD' , '" & LIST_BOX_CLASSIFICATION.LC_FIX_VALUE & "');"

            Case "SHAGATA"
                ' 車型
                O_ATTR = "REF_Field_DBclick('SHAGATA', 'WF_Rep_FIELD' , '" & LIST_BOX_CLASSIFICATION.LC_FIX_VALUE & "');"

            Case "CONTENASTATE"
                ' コンテナ状態
                O_ATTR = "REF_Field_DBclick('CONTENASTATE', 'WF_Rep_FIELD' , '" & LIST_BOX_CLASSIFICATION.LC_FIX_VALUE & "');"
            Case "SUPPL"
                ' 用車会社
                O_ATTR = "REF_Field_DBclick('SUPPL', 'WF_Rep_FIELD' , '" & LIST_BOX_CLASSIFICATION.LC_CUSTOMER & "');"

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

                        'フィールドによってパラメーターを変える
                        Debug.Print(WF_FIELD.Value)
                        'フィールドによってパラメーターを変える
                        Select Case WW_FIELD
                            Case "WF_TORICOE"          '取引先
                                prmData = work.CreateTORIParam(WF_CAMPCODE.Text)
                            Case "WF_UNCHINFUNCCODE"          '荷主車番
                                prmData = work.CreateFIXParam(WF_CAMPCODE.Text, "UNCHINFUNCCODE")
                            Case "WF_NSHABAN"          '荷主車番
                                prmData = work.CreateFIXParam(WF_CAMPCODE.Text, "NSHABAN")
                            Case "SHARYOKEIYAKUCODE"   '車両契約内容コード
                                prmData = work.CreateFIXParam(WF_CAMPCODE.Text, "SHARYOKEIYAKUCODE")
                            Case "SHAGATA"             '車型
                                prmData = work.CreateFIXParam(WF_CAMPCODE.Text, "SHAGATA")
                            Case "CONTENASTATE"        'コンテナ状態
                                prmData = work.CreateFIXParam(WF_CAMPCODE.Text, "CONTENASTATE")
                        End Select

                        Debug.Print(WF_LeftMViewChange.Value)
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

        Dim WW_STAFFNAMES As String = String.Empty
        Dim WW_STAFFNAMEL As String = String.Empty

        '選択内容を取得

        If leftview.WF_LeftListBox.SelectedIndex >= 0 Then
            WF_SelectedIndex.Value = leftview.WF_LeftListBox.SelectedIndex
            WW_SelectValue = leftview.WF_LeftListBox.Items(WF_SelectedIndex.Value).Value
            WW_SelectTEXT = leftview.WF_LeftListBox.Items(WF_SelectedIndex.Value).Text
        End If

        '選択内容を画面項目へセット
        '項目セット　＆　フォーカス
        If WF_FIELD_REP.Value = "" Then
            Select Case WF_FIELD.Value

                Case "WF_TORICODE"
                    WF_TORICODE_TEXT.Text = WW_SelectTEXT
                    WF_TORICODE.Text = WW_SelectValue
                    WF_TORICODE.Focus()

                Case "WF_UNCHINFUNCCODE"
                    WF_UNCHINFUNCCODE_TEXT.Text = WW_SelectTEXT
                    WF_UNCHINFUNCCODE.Text = WW_SelectValue
                    WF_UNCHINFUNCCODE.Focus()

                Case "WF_NSHABAN"
                    WF_NSHABAN_TEXT.Text = WW_SelectTEXT
                    WF_NSHABAN.Text = WW_SelectValue
                    WF_NSHABAN.Focus()

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

                Case "WF_TORICODE"          '取引先コード(キー部)
                    WF_TORICODE.Focus()

                Case "WF_STYMD"             '有効年月日(キー部)
                    WF_STYMD.Focus()

                Case "WF_ENDYMD"            '有効年月日(キー部)
                    WF_ENDYMD.Focus()

                Case "WF_DELFLG"            '削除(キー部)
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

        Master.CreateEmptyTable(MA0007INPtbl)

        '○UPLOAD_XLSデータ取得        
        CS0023XLSUPLOAD.CAMPCODE = work.WF_SEL_CAMPCODE.Text
        CS0023XLSUPLOAD.MAPID = GRMA0007WRKINC.MAPID
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
           WW_COLUMNS.IndexOf("TORICODE") < 0 OrElse
           WW_COLUMNS.IndexOf("UNCHINFUNCCODE") < 0 OrElse
           WW_COLUMNS.IndexOf("NSHABAN") < 0 OrElse
           WW_COLUMNS.IndexOf("STYMD") < 0 Then
            ' インポート出来ません(項目： ?01 が存在しません)。
            Master.Output(C_MESSAGE_NO.IMPORT_ERROR, C_MESSAGE_TYPE.ERR, "Inport TITLE not find")
            Exit Sub
        End If

        '○Excelデータ毎にチェック＆更新
        For Each XLSTBLrow As DataRow In CS0023XLSUPLOAD.TBLDATA.Rows
            '○XLSTBL明細⇒MA0007INProw
            Dim MA0007INProw = MA0007INPtbl.NewRow

            '初期クリア
            For Each MA0007INPcol As DataColumn In MA0007INPtbl.Columns

                If IsDBNull(MA0007INProw.Item(MA0007INPcol)) OrElse IsNothing(MA0007INProw.Item(MA0007INPcol)) Then
                    Select Case MA0007INPcol.ColumnName
                        Case "LINECNT"
                            MA0007INProw.Item(MA0007INPcol) = 0
                        Case "TIMSTP"
                            MA0007INProw.Item(MA0007INPcol) = 0
                        Case "SELECT"
                            MA0007INProw.Item(MA0007INPcol) = 1
                        Case "HIDDEN"
                            MA0007INProw.Item(MA0007INPcol) = 0
                        Case "SEQ"
                            MA0007INProw.Item(MA0007INPcol) = 0
                        Case Else
                            If MA0007INPcol.DataType.Name = "String" Then
                                MA0007INProw.Item(MA0007INPcol) = ""
                            ElseIf MA0007INPcol.DataType.Name = "DateTime" Then
                                MA0007INProw.Item(MA0007INPcol) = C_DEFAULT_YMD
                            Else
                                MA0007INProw.Item(MA0007INPcol) = 0
                            End If
                    End Select
                End If
            Next

            '○変更元情報をデフォルト設定
            Dim WW_STYMD As String = ""

            If WW_COLUMNS.IndexOf("CAMPCODE") >= 0 AndAlso
               WW_COLUMNS.IndexOf("TORICODE") >= 0 AndAlso
               WW_COLUMNS.IndexOf("UNCHINFUNCCODE") >= 0 AndAlso
               WW_COLUMNS.IndexOf("NSHABAN") >= 0 AndAlso
               WW_COLUMNS.IndexOf("STYMD") >= 0 Then

                For Each MA0007row As DataRow In MA0007tbl.Rows
                    If XLSTBLrow("CAMPCODE") = MA0007row("CAMPCODE") AndAlso
                       XLSTBLrow("TORICODE") = MA0007row("TORICODE") AndAlso
                       XLSTBLrow("UNCHINFUNCCODE") = MA0007row("UNCHINFUNCCODE") AndAlso
                       XLSTBLrow("NSHABAN") = MA0007row("NSHABAN") AndAlso
                       XLSTBLrow("STYMD") = MA0007row("STYMD") Then
                        MA0007INProw.ItemArray = MA0007row.ItemArray
                        Exit For
                    End If
                Next
            End If

            '○項目セット
            '会社コード
            If WW_COLUMNS.IndexOf("CAMPCODE") >= 0 Then
                MA0007INProw("CAMPCODE") = XLSTBLrow("CAMPCODE")
            End If

            '会社名
            If WW_COLUMNS.IndexOf("CAMPNAMES") >= 0 Then
                MA0007INProw("CAMPNAMES") = XLSTBLrow("CAMPNAMES")
            End If


            '取引先コード
            If WW_COLUMNS.IndexOf("TORICODE") >= 0 Then
                MA0007INProw("TORICODE") = XLSTBLrow("TORICODE")
            End If

            '取引先名
            If WW_COLUMNS.IndexOf("TORICODENAMES") >= 0 Then
                MA0007INProw("TORICODENAMES") = XLSTBLrow("TORICODENAMES")
            End If


            '運賃計算機能コード
            If WW_COLUMNS.IndexOf("UNCHINFUNCCODE") >= 0 Then
                MA0007INProw("UNCHINFUNCCODE") = XLSTBLrow("UNCHINFUNCCODE")
            End If

            '運賃計算機能コード名
            If WW_COLUMNS.IndexOf("UNCHINFUNCCODENAMES") >= 0 Then
                MA0007INProw("UNCHINFUNCCODENAMES") = XLSTBLrow("UNCHINFUNCCODENAMES")
            End If


            '荷主車番
            If WW_COLUMNS.IndexOf("NSHABAN") >= 0 Then
                MA0007INProw("NSHABAN") = XLSTBLrow("NSHABAN")
            End If

            '運賃計算車腹
            If WW_COLUMNS.IndexOf("UNCHINSHAFUKU") >= 0 Then
                MA0007INProw("UNCHINSHAFUKU") = XLSTBLrow("UNCHINSHAFUKU")
            End If


            '車両契約内容コード
            If WW_COLUMNS.IndexOf("SHARYOKEIYAKUCODE") >= 0 Then
                MA0007INProw("SHARYOKEIYAKUCODE") = XLSTBLrow("SHARYOKEIYAKUCODE")
            End If


            '車両契約内容コード名
            If WW_COLUMNS.IndexOf("SHARYOKEIYAKUCODENAMES") >= 0 Then
                MA0007INProw("SHARYOKEIYAKUCODENAMES") = XLSTBLrow("SHARYOKEIYAKUCODENAMES")
            End If


            '車復
            If WW_COLUMNS.IndexOf("SHAFUKU") >= 0 Then
                MA0007INProw("SHAFUKU") = XLSTBLrow("SHAFUKU")
            End If


            '車型
            If WW_COLUMNS.IndexOf("SHAGATA") >= 0 Then
                MA0007INProw("SHAGATA") = XLSTBLrow("SHAGATA")
            End If

            '車型名
            If WW_COLUMNS.IndexOf("SHAGATANAMES") >= 0 Then
                MA0007INProw("SHAGATANAMES") = XLSTBLrow("SHAGATANAMES")
            End If

            'コンテナ状態
            If WW_COLUMNS.IndexOf("CONTENASTATE") >= 0 Then
                MA0007INProw("CONTENASTATE") = XLSTBLrow("CONTENASTATE")
            End If

            'コンテナ状態名
            If WW_COLUMNS.IndexOf("CONTENASTATENAMES") >= 0 Then
                MA0007INProw("CONTENASTATENAMES") = XLSTBLrow("CONTENASTATENAMES")
            End If

            '用車会社
            If WW_COLUMNS.IndexOf("SUPPL") >= 0 Then
                MA0007INProw("SUPPL") = XLSTBLrow("SUPPL")
            End If


            '用車会社名
            If WW_COLUMNS.IndexOf("SUPPLNAMES") >= 0 Then
                MA0007INProw("SUPPLNAMES") = XLSTBLrow("SUPPLNAMES")
            End If


            '有効開始日
            If WW_COLUMNS.IndexOf("STYMD") >= 0 Then
                If IsDate(XLSTBLrow("STYMD")) Then
                    Dim WW_DATE As Date
                    Date.TryParse(XLSTBLrow("STYMD"), WW_DATE)
                    MA0007INProw("STYMD") = WW_DATE.ToString("yyyy/MM/dd")
                End If
            End If

            '有効終了日
            If WW_COLUMNS.IndexOf("ENDYMD") >= 0 Then
                If IsDate(XLSTBLrow("ENDYMD")) Then
                    Dim WW_DATE As Date
                    Date.TryParse(XLSTBLrow("ENDYMD"), WW_DATE)
                    MA0007INProw("ENDYMD") = WW_DATE.ToString("yyyy/MM/dd")
                End If
            End If

            '削除
            If WW_COLUMNS.IndexOf("DELFLG") >= 0 Then
                MA0007INProw("DELFLG") = XLSTBLrow("DELFLG")
            End If

            MA0007INPtbl.Rows.Add(MA0007INProw)
        Next

        '○項目チェック
        INPtbl_Check(WW_ERRCODE)

        '○画面表示テーブル更新
        If isNormal(WW_ERRCODE) Then
            MA0007tbl_UPD()
        End If

        '○画面表示データ保存
        Master.SaveTable(MA0007tbl)

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
        If Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.MA0007S Then

            Master.MAPID = GRMA0007WRKINC.MAPID
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
            If MA0007tbl Is Nothing Then
                MA0007tbl = New DataTable
            End If

            If MA0007tbl.Columns.Count <> 0 Then
                MA0007tbl.Columns.Clear()
            End If

            '○DB項目クリア
            MA0007tbl.Clear()

            '○テーブル検索結果をテーブル退避
            'MA0007テンポラリDB項目作成

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
                    & "         TIMSTP = cast(isnull(UPDTIMSTP,0) as bigint)                    , " _
                    & "         1                                      as 'SELECT'              , " _
                    & "         0                                      as HIDDEN                , " _
                    & "         rtrim(CAMPCODE)                        as CAMPCODE              , " _
                    & "         ''                                     as CAMPNAMES             , " _
                    & "         rtrim(UNCHINFUNCCODE)         　　　　 as UNCHINFUNCCODE        , " _
                    & "         ''　　　　　　　　　　　　　　         as UNCHINFUNCCODENAMES   , " _
                    & "         rtrim(TORICODE)         　　　　       as TORICODE              , " _
                    & "         ''　　　　　　　　　　　　　　         as TORICODENAMES         , " _
                    & "         rtrim(NSHABAN)                         as NSHABAN               , " _
                    & "         rtrim(UNCHINSHAFUKU)                   as UNCHINSHAFUKU         , " _
                    & "         rtrim(SHARYOKEIYAKUCODE)               as SHARYOKEIYAKUCODE     , " _
                    & "         ''　　　　　　　　　                   as SHARYOKEIYAKUCODENAMES, " _
                    & "         rtrim(SHAFUKU)                         as SHAFUKU               , " _
                    & "         rtrim(SHAGATA)                         as SHAGATA               , " _
                    & "         ''　　　　　　　　　                   as SHAGATANAMES          , " _
                    & "         rtrim(CONTENASTATE)                    as CONTENASTATE   　     , " _
                    & "         ''　　　　　　　　　                   as CONTENASTATENAMES     , " _
                    & "         format(STYMD, 'yyyy/MM/dd')            as STYMD                 , " _
                    & "         format(ENDYMD, 'yyyy/MM/dd')           as ENDYMD                , " _
                    & "         rtrim(SUPPL)                           as SUPPL                 , " _
                    & "         ''　　　　　　　　　                   as SUPPLNAMES　　　　    , " _
                    & "         rtrim(DELFLG)                          as DELFLG                , " _
                    & "         ''                                     as INITYMD               , " _
                    & "         ''                                     as UPDYMD                , " _
                    & "         ''                                     as UPDUSER               , " _
                    & "         ''                                     as UPDTERMID             , " _
                    & "         ''                                     as RECEIVEYMD            , " _
                    & "         ''                                     as UPDTIMSTP               " _
                    & " FROM                                                                      " _
                    & "           MA007_NINUSHISHABAN                                             " _
                    & " WHERE                                                                     " _
                    & "           CAMPCODE    = @P01                                              "

                '取引先が入力されていた場合は条件にセット
                If work.WF_SEL_TORICODE.Text.Length <> 0 Then
                    SQLStr += "      and  TORICODE    = @P02                                      "
                End If

                '取引先が入力されていた場合は条件にセット
                If work.WF_SEL_UNCHINFUNCCODE.Text.Length <> 0 Then
                    SQLStr += "      and  UNCHINFUNCCODE  = @P03                                  "
                End If

                '荷主車番が入力されていた場合は条件にセット
                If work.WF_SEL_NSHABAN.Text.Length <> 0 Then
                    SQLStr += "      and  NSHABAN = @P04                                          "
                End If

                SQLStr += "  and  STYMD      <= @P05                                         " _
                    & "      and  ENDYMD     >= @P06                                         " _
                    & "      and  DELFLG     <> '1'                                          " _
                    & " ORDER BY                                                             " _
                    & "      CAMPCODE, TORICODE, UNCHINFUNCCODE, NSHABAN, STYMD              "

                Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                    Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P01", SqlDbType.NVarChar, 20)
                    Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P02", SqlDbType.NVarChar, 20)
                    Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P03", SqlDbType.NVarChar, 20)
                    Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P04", SqlDbType.NVarChar, 20)
                    Dim PARA5 As SqlParameter = SQLcmd.Parameters.Add("@P05", SqlDbType.Date)
                    Dim PARA6 As SqlParameter = SQLcmd.Parameters.Add("@P06", SqlDbType.Date)


                    PARA1.Value = work.WF_SEL_CAMPCODE.Text
                    PARA2.Value = work.WF_SEL_TORICODE.Text
                    PARA3.Value = work.WF_SEL_UNCHINFUNCCODE.Text
                    PARA4.Value = work.WF_SEL_NSHABAN.Text
                    PARA5.Value = work.WF_SEL_ENDYMD.Text
                    PARA6.Value = work.WF_SEL_STYMD.Text

                    Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                        'フィールド名とフィールドの型を取得
                        For index As Integer = 0 To SQLdr.FieldCount - 1
                            MA0007tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                        Next

                        '○ テーブル検索結果をテーブル格納
                        MA0007tbl.Load(SQLdr)

                        For Each MA0007row As DataRow In MA0007tbl.Rows
                            CODENAME_get("CAMPCODE", MA0007row("CAMPCODE"), MA0007row("CAMPNAMES"), WW_DUMMY)
                            CODENAME_get("UNCHINFUNCCODE", MA0007row("UNCHINFUNCCODE"), MA0007row("UNCHINFUNCCODENAMES"), WW_DUMMY)
                            CODENAME_get("TORICODE", MA0007row("TORICODE"), MA0007row("TORICODENAMES"), WW_DUMMY)
                            CODENAME_get("SHAGATA", MA0007row("SHAGATA"), MA0007row("SHAGATANAMES"), WW_DUMMY)
                            CODENAME_get("SHARYOKEIYAKUCODE", MA0007row("SHARYOKEIYAKUCODE"), MA0007row("SHARYOKEIYAKUCODENAMES"), WW_DUMMY)
                            CODENAME_get("CONTENASTATE", MA0007row("CONTENASTATE"), MA0007row("CONTENASTATENAMES"), WW_DUMMY)
                            CODENAME_get("TORICODE", MA0007row("SUPPL"), MA0007row("SUPPLNAMES"), WW_DUMMY)
                        Next

                    End Using
                End Using
            End Using
        Catch ex As Exception
            'ログ出力
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "MC013_UNCHINKETEI SELECT")
            CS0011LOGWRITE.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:MA007_NINUSHISHABAN Select"
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
        CS0026TBLSORT.TABLE = MA0007tbl
        CS0026TBLSORT.TAB = ""
        CS0026TBLSORT.FILTER = ""
        CS0026TBLSORT.SortandNumbring()
        If isNormal(CS0026TBLSORT.ERR) Then
            MA0007tbl = CS0026TBLSORT.TABLE
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

        For Each MA0007INProw As DataRow In MA0007INPtbl.Rows

            WW_LINEERR_SW = ""
            '○単項目チェック(会社コード)
            WW_TEXT = MA0007INProw("CAMPCODE")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "CAMPCODE", MA0007INProw("CAMPCODE"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '存在チェック
                If WW_TEXT = "" Then
                    MA0007INProw("CAMPCODE") = ""
                Else
                    CODENAME_get("CAMPCODE", MA0007INProw("CAMPCODE"), WW_DUMMY, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・更新できないレコード(会社エラー)です。"
                        WW_CheckMES2 = ""
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, MA0007INProw)
                        O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                        WW_LINEERR_SW = "ERR"
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(会社コードエラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, MA0007INProw)
                O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                WW_LINEERR_SW = "ERR"
            End If


            '○単項目チェック(取引先コード)
            WW_TEXT = MA0007INProw("TORICODE")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "TORICODE", MA0007INProw("TORICODE"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '存在チェック
                If WW_TEXT = "" Then
                    MA0007INProw("TORICODE") = ""
                Else
                    CODENAME_get("TORICODE", MA0007INProw("TORICODE"), WW_DUMMY, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・更新できないレコード(取引先コードエラー)です。"
                        WW_CheckMES2 = ""
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, MA0007INProw)
                        O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                        WW_LINEERR_SW = "ERR"
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(取引先コードエラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, MA0007INProw)
                O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                WW_LINEERR_SW = "ERR"
            End If


            WW_TEXT = MA0007INProw("UNCHINFUNCCODE")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "UNCHINFUNCCODE", MA0007INProw("UNCHINFUNCCODE"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '存在チェック
                If WW_TEXT = "" Then
                    MA0007INProw("UNCHINFUNCCODE") = ""
                Else
                    CODENAME_get("UNCHINFUNCCODE", MA0007INProw("UNCHINFUNCCODE"), WW_DUMMY, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・更新できないレコード(運賃計算機能コードエラー)です。"
                        WW_CheckMES2 = ""
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, MA0007INProw)
                        O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                        WW_LINEERR_SW = "ERR"
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(運賃計算機能コードエラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, MA0007INProw)
                O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                WW_LINEERR_SW = "ERR"
            End If


            '○単項目チェック(荷主車番)
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "NSHABAN", MA0007INProw("NSHABAN"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(荷主車番エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, MA0007INProw)
                O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                WW_LINEERR_SW = "ERR"
            End If


            '○単項目チェック(有効開始日付)
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "STYMD", MA0007INProw("STYMD"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(有効日付：開始エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, MA0007INProw)
                O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                WW_LINEERR_SW = "ERR"
            End If

            '○単項目チェック(有効終了日付)
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "ENDYMD", MA0007INProw("ENDYMD"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(有効日付：終了エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, MA0007INProw)
                O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                WW_LINEERR_SW = "ERR"
            End If


            '○単項目チェック(DELFLG)
            If MA0007INProw("DELFLG") = "" OrElse MA0007INProw("DELFLG") = C_DELETE_FLG.ALIVE OrElse MA0007INProw("DELFLG") = C_DELETE_FLG.DELETE Then
                If MA0007INProw("DELFLG") = "" Then
                    MA0007INProw("DELFLG") = C_DELETE_FLG.ALIVE
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(削除CD不正)です。"
                WW_CheckMES2 = ""
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, MA0007INProw)
                O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                WW_LINEERR_SW = "ERR"
            End If


            '○単項目チェック(運賃計算車腹)
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "UNCHINSHAFUKU", MA0007INProw("UNCHINSHAFUKU"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(運賃計算車腹エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, MA0007INProw)
                O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                WW_LINEERR_SW = "ERR"
            End If


            '○単項目チェック(車両契約内容コード)
            WW_TEXT = MA0007INProw("SHARYOKEIYAKUCODE")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "SHARYOKEIYAKUCODE", MA0007INProw("SHARYOKEIYAKUCODE"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '存在チェック
                If WW_TEXT = "" Then
                    MA0007INProw("SHARYOKEIYAKUCODE") = ""
                Else
                    CODENAME_get("SHARYOKEIYAKUCODE", MA0007INProw("SHARYOKEIYAKUCODE"), WW_DUMMY, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・更新できないレコード(車両契約内容コードエラー)です。"
                        WW_CheckMES2 = ""
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, MA0007INProw)
                        O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                        WW_LINEERR_SW = "ERR"
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(車両契約内容コードエラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, MA0007INProw)
                O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                WW_LINEERR_SW = "ERR"
            End If


            '○単項目チェック(車復)
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "SHAFUKU", MA0007INProw("SHAFUKU"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(車復エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, MA0007INProw)
                O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                WW_LINEERR_SW = "ERR"
            End If



            '○単項目チェック(車型)
            WW_TEXT = MA0007INProw("SHAGATA")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "SHAGATA", MA0007INProw("SHAGATA"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '存在チェック
                If WW_TEXT = "" Then
                    MA0007INProw("SHAGATA") = ""
                Else
                    CODENAME_get("SHAGATA", MA0007INProw("SHAGATA"), WW_DUMMY, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・更新できないレコード(車型エラー)です。"
                        WW_CheckMES2 = ""
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, MA0007INProw)
                        O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                        WW_LINEERR_SW = "ERR"
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(車型エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, MA0007INProw)
                O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                WW_LINEERR_SW = "ERR"
            End If


            '○単項目チェック(コンテナ状態)
            WW_TEXT = MA0007INProw("CONTENASTATE")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "CONTENASTATE", MA0007INProw("CONTENASTATE"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '存在チェック
                If WW_TEXT = "" Then
                    MA0007INProw("CONTENASTATE") = ""
                Else
                    CODENAME_get("CONTENASTATE", MA0007INProw("CONTENASTATE"), WW_DUMMY, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・更新できないレコード(コンテナ状態エラー)です。"
                        WW_CheckMES2 = ""
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, MA0007INProw)
                        O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                        WW_LINEERR_SW = "ERR"
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(コンテナ状態エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, MA0007INProw)
                O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                WW_LINEERR_SW = "ERR"
            End If


            '○単項目チェック(用車会社)
            WW_TEXT = MA0007INProw("SUPPL")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "SUPPL", MA0007INProw("SUPPL"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '存在チェック
                If WW_TEXT = "" Then
                    MA0007INProw("SUPPL") = ""
                Else
                    CODENAME_get("SUPPL", MA0007INProw("SUPPL"), WW_DUMMY, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・更新できないレコード(用車会社エラー)です。"
                        WW_CheckMES2 = ""
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, MA0007INProw)
                        O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                        WW_LINEERR_SW = "ERR"
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(用車会社エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, MA0007INProw)
                O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                WW_LINEERR_SW = "ERR"
            End If


            '○操作設定
            If WW_LINEERR_SW = "" Then
                If MA0007INProw("OPERATION") <> C_LIST_OPERATION_CODE.ERRORED Then
                    MA0007INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                End If
            Else
                MA0007INProw("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
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
        For Each MA0007INProw As DataRow In MA0007tbl.Rows

            '読み飛ばし
            If (MA0007INProw("OPERATION") <> C_LIST_OPERATION_CODE.UPDATING AndAlso
                MA0007INProw("OPERATION") <> C_LIST_OPERATION_CODE.ERRORED) OrElse
                MA0007INProw("DELFLG") = C_DELETE_FLG.DELETE OrElse
                MA0007INProw("STYMD") < C_DEFAULT_YMD Then
                Continue For
            End If

            WW_LINEERR_SW = ""

            'チェック
            For Each MA0007row As DataRow In MA0007tbl.Rows

                '日付以外の項目が等しい
                If MA0007INProw("CAMPCODE") = MA0007row("CAMPCODE") AndAlso
                   MA0007INProw("TORICODE") = MA0007row("TORICODE") AndAlso
                   MA0007INProw("UNCHINFUNCCODE") = MA0007row("UNCHINFUNCCODE") AndAlso
                   MA0007INProw("NSHABAN") = MA0007row("NSHABAN") AndAlso
                    MA0007row("DELFLG") <> C_DELETE_FLG.DELETE Then
                Else
                    Continue For
                End If

                '期間変更対象は読み飛ばし
                If MA0007INProw("STYMD") = MA0007row("STYMD") Then
                    Continue For
                End If

                Try
                    Date.TryParse(MA0007INProw("STYMD"), WW_DATE_ST)
                    Date.TryParse(MA0007INProw("ENDYMD"), WW_DATE_END)
                    Date.TryParse(MA0007row("STYMD"), WW_DATE_ST2)
                    Date.TryParse(MA0007row("ENDYMD"), WW_DATE_END2)
                Catch ex As Exception
                End Try

                ''開始日チェック
                'If (WW_DATE_ST >= WW_DATE_ST2 AndAlso WW_DATE_ST <= WW_DATE_END2) Then
                '    WW_CheckMES1 = "・エラー(期間重複)が存在します。"
                '    WW_CheckMES2 = ""
                '    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR,  MA0007row)
                '    O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                '    WW_LINEERR_SW = "ERR"
                '    Exit For
                'End If

                ''終了日チェック
                'If (WW_DATE_END >= WW_DATE_ST2 AndAlso WW_DATE_END <= WW_DATE_END2) Then
                '    WW_CheckMES1 = "・エラー(期間重複)が存在します。"
                '    WW_CheckMES2 = ""
                '    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR,  MA0007row)
                '    O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                '    WW_LINEERR_SW = "ERR"
                '    Exit For
                'End If

            Next

            If WW_LINEERR_SW = "" Then
                MA0007INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
            Else
                MA0007INProw("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End If
        Next

    End Sub


    ''' <summary>
    ''' 更新予定データ登録・更新処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub MA0007tbl_UPD()

        '○操作表示クリア
        For Each MA0007row As DataRow In MA0007tbl.Rows
            Select Case MA0007row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    MA0007row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.NODISP
                    MA0007row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED
                    MA0007row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    MA0007row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    MA0007row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End Select
        Next

        '○追加変更判定
        For Each MA0007INProw As DataRow In MA0007INPtbl.Rows

            'エラーレコード読み飛ばし
            If MA0007INProw("OPERATION") <> C_LIST_OPERATION_CODE.UPDATING Then
                Continue For
            End If

            '初期判定セット
            MA0007INProw("OPERATION") = "Insert"

            For Each MA0007row As DataRow In MA0007tbl.Rows

                If MA0007INProw("CAMPCODE") = MA0007row("CAMPCODE") AndAlso
                   MA0007INProw("TORICODE") = MA0007row("TORICODE") AndAlso
                   MA0007INProw("UNCHINFUNCCODE") = MA0007row("UNCHINFUNCCODE") AndAlso
                   MA0007INProw("NSHABAN") = MA0007row("NSHABAN") AndAlso
                   MA0007INProw("STYMD") = MA0007row("STYMD") Then
                Else
                    Continue For
                End If

                'レコード内容に変更があったか判定
                If MA0007row("CAMPCODE") = MA0007INProw("CAMPCODE") AndAlso
                    MA0007row("CAMPNAMES") = MA0007INProw("CAMPNAMES") AndAlso
                    MA0007row("TORICODE") = MA0007INProw("TORICODE") AndAlso
                    MA0007row("TORICODENAMES") = MA0007INProw("TORICODENAMES") AndAlso
                    MA0007row("UNCHINFUNCCODE") = MA0007INProw("UNCHINFUNCCODE") AndAlso
                    MA0007row("UNCHINFUNCCODENAMES") = MA0007INProw("UNCHINFUNCCODENAMES") AndAlso
                    MA0007row("NSHABAN") = MA0007INProw("NSHABAN") AndAlso
                    MA0007row("STYMD") = MA0007INProw("STYMD") AndAlso
                    MA0007row("ENDYMD") = MA0007INProw("ENDYMD") AndAlso
                    MA0007row("UNCHINSHAFUKU") = MA0007INProw("UNCHINSHAFUKU") AndAlso
                    MA0007row("SHARYOKEIYAKUCODE") = MA0007INProw("SHARYOKEIYAKUCODE") AndAlso
                    MA0007row("SHARYOKEIYAKUCODENAMES") = MA0007INProw("SHARYOKEIYAKUCODENAMES") AndAlso
                    MA0007row("SHAFUKU") = MA0007INProw("SHAFUKU") AndAlso
                    MA0007row("SHAGATA") = MA0007INProw("SHAGATA") AndAlso
                    MA0007row("SHAGATANAMES") = MA0007INProw("SHAGATANAMES") AndAlso
                    MA0007row("CONTENASTATE") = MA0007INProw("CONTENASTATE") AndAlso
                    MA0007row("CONTENASTATENAMES") = MA0007INProw("CONTENASTATENAMES") AndAlso
                    MA0007row("SUPPL") = MA0007INProw("SUPPL") AndAlso
                    MA0007row("DELFLG") = MA0007INProw("DELFLG") Then

                    MA0007INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Else
                    '○更新（Update）
                    TBL_Update_SUB(MA0007INProw, MA0007row)
                End If

                Exit For

            Next

            '○MA0007追加処理
            If MA0007INProw("OPERATION") = "Insert" Then
                '○更新（Insert）
                TBL_Insert_SUB(MA0007INProw)
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

        '○MA0007変更処理
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

        '○MA0007追加処理
        Dim MA0007row As DataRow = MA0007tbl.NewRow
        MA0007row.ItemArray = INProw.ItemArray

        MA0007row("LINECNT") = MA0007tbl.Rows.Count + 1
        MA0007row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
        MA0007row("TIMSTP") = 0
        MA0007row("SELECT") = 1
        MA0007row("HIDDEN") = 0
        MA0007tbl.Rows.Add(MA0007row)

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
    Protected Sub WW_CheckERR(ByRef I_MESSAGE1 As String, ByRef I_MESSAGE2 As String, ByVal I_ERRCD As String, ByVal MA0007INProw As DataRow)

        Dim WW_ERR_MES As String = ""
        WW_ERR_MES = I_MESSAGE1
        If I_MESSAGE2 <> "" Then
            WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> " & I_MESSAGE2 & " , "
        End If
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 会社コード　　　　　=" & MA0007INProw("CAMPCODE") & " , "
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 取引先コード　　　　=" & MA0007INProw("TORICODE") & " , "
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 運賃計算機能コード　=" & MA0007INProw("UNCHINFUNCCODE") & " , "
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 荷主車番　　　　　  =" & MA0007INProw("NSHABAN") & " , "
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 開始年月日　　　　　=" & MA0007INProw("STYMD") & " , "
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 終了年月日　　　　　=" & MA0007INProw("ENDYMD") & " , "
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 削除フラグ　　　　　=" & MA0007INProw("DELFLG") & " "
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

                    Case "TORICODE"             '取引先コード
                        .CodeToName(LIST_BOX_CLASSIFICATION.LC_CUSTOMER, I_VALUE, O_TEXT, O_RTN, work.CreateTORIParam(work.WF_SEL_CAMPCODE.Text))

                    Case "UNCHINFUNCCODE"       '運賃計算機能コード
                        .CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "UNCHINFUNCCODE"))

                    Case "SHARYOKEIYAKUCODE"    '車両契約内容コード
                        .CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "SHARYOKEIYAKUCODE"))

                    Case "CONTENASTATE"         'コンテナ状態
                        .CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "CONTENASTATE"))

                    Case "SHAGATA"              '車型
                        .CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "SHAGATA"))

                    Case "NSHABAN"              '荷主車番
                        .CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "NSHABAN"))

                    Case "SUPPL"             '取引先コード
                        .CodeToName(LIST_BOX_CLASSIFICATION.LC_CUSTOMER, I_VALUE, O_TEXT, O_RTN, work.CreateTORIParam(work.WF_SEL_CAMPCODE.Text))

                    Case "DELFLG"               '削除フラグ名称
                        .CodeToName(LIST_BOX_CLASSIFICATION.LC_DELFLG, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "DELFLG"))

                    Case Else
                        O_TEXT = ""                                                             '該当項目なし

                End Select
            End With
        End If
    End Sub

End Class
