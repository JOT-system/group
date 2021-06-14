Imports System.Data.SqlClient
Imports OFFICE.GRIS0005LeftBox
Imports BASEDLL

''' <summary>
''' 荷主運賃決定マスタ（登録）
''' </summary>
''' <remarks></remarks>
Public Class GRMC0013UNTINKETEI
    Inherits Page

    '検索結果格納
    Private MC0013tbl As DataTable                              'Grid格納用テーブル
    Private MC0013INPtbl As DataTable                           'チェック用テーブル

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
                    If Not Master.RecoverTable(MC0013tbl) Then
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
            If Not IsNothing(MC0013tbl) Then
                MC0013tbl.Clear()
                MC0013tbl.Dispose()
                MC0013tbl = Nothing
            End If

            If Not IsNothing(MC0013INPtbl) Then
                MC0013INPtbl.Clear()
                MC0013INPtbl.Dispose()
                MC0013INPtbl = Nothing
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
        WF_SELMANGORG.Focus()
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
        Master.SaveTable(MC0013tbl)

        '一覧表示データ編集（性能対策）
        Using TBLview As DataView = New DataView(MC0013tbl)
            TBLview.RowFilter = "LINECNT >= 1 and LINECNT <= " & CONST_DSPROWCOUNT
            CS0013ProfView.CAMPCODE = work.WF_SEL_CAMPCODE.Text
            CS0013ProfView.PROFID = Master.PROF_VIEW
            CS0013ProfView.MAPID = GRMD0001WRKINC.MAPID
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
        For Each MC0013row As DataRow In MC0013tbl.Rows
            If MC0013row("HIDDEN") = 0 Then
                WW_DataCNT = WW_DataCNT + 1
                '行（ラインカウント）を再設定する。既存項目（SELECT）を利用
                MC0013row("SELECT") = WW_DataCNT
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
        Dim WW_TBLview As DataView = New DataView(MC0013tbl)

        'ソート
        WW_TBLview.Sort = "LINECNT"
        WW_TBLview.RowFilter = "HIDDEN = 0 and SELECT >= " & WW_GridPosition.ToString() & " and SELECT < " & (WW_GridPosition + CONST_DSPROWCOUNT).ToString()
        '一覧作成

        CS0013ProfView.CAMPCODE = work.WF_SEL_CAMPCODE.Text
        CS0013ProfView.PROFID = Master.PROF_VIEW
        CS0013ProfView.MAPID = GRMD0001WRKINC.MAPID
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

        WF_SELMANGORG.Focus()

    End Sub


    ' ******************************************************************************
    ' ***  絞り込みボタン処理                                                    ***
    ' ******************************************************************************
    Protected Sub WF_ButtonExtract_Click()

        '○絞り込み操作（GridView明細Hidden設定）
        For Each MC0013row As DataRow In MC0013tbl.Rows

            Dim WW_HANTEI As Integer = 0

            ' 管理部署コードによる絞込判定
            If WF_SELMANGORG.Text = "" Then
                WW_HANTEI = WW_HANTEI + 0
                WF_SELMANGORG.Text = ""
            Else
                Dim wstr As String = MC0013row("MANGORG")
                If wstr.Substring(2).StartsWith(WF_SELMANGORG.Text) Then
                    WW_HANTEI = WW_HANTEI + 0
                Else
                    WW_HANTEI = WW_HANTEI + 1
                End If
            End If

            '画面(Grid)のHIDDEN列に結果格納
            If WW_HANTEI = 0 Then
                MC0013row("HIDDEN") = 0     '表示対象
            Else
                MC0013row("HIDDEN") = 1     '非表示対象
            End If
        Next

        '○画面表示データ保存
        Master.SaveTable(MC0013tbl)

        '○画面表示
        '画面先頭を表示
        WF_GridPosition.Text = "1"

        '○メッセージ表示
        Master.Output(C_MESSAGE_NO.DATA_FILTER_SUCCESSFUL, C_MESSAGE_TYPE.INF)

        'カーソル設定
        WF_SELMANGORG.Focus()

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
            Master.SaveTable(MC0013tbl)
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
                    & "     FROM    MD001_PRODUCT                                                                      " _
                    & "     WHERE CAMPCODE =@P17 and PRODUCTCODE = @P16 and STYMD = @P5 ;                              " _
                    & " OPEN hensuu ;                                                                                  " _
                    & " FETCH NEXT FROM hensuu INTO @hensuu ;                                                          " _
                    & " IF ( @@FETCH_STATUS = 0 )                                                                      " _
                    & "    UPDATE   MD001_PRODUCT                                                                      " _
                    & "       SET                                                                                      " _
                    & "         OILTYPE = @P1 , PRODUCT1 = @P2 , PRODUCT2 = @P3 ,                                       " _
                    & " SEQ = @P4 , ENDYMD = @P6 , NAMES = @P7 , NAMEL = @P8, DELFLG = @P9 ,                           " _
                    & "           UPDYMD = @P11 , UPDUSER = @P12 , UPDTERMID = @P13 , RECEIVEYMD = @P14 , STANI = @P15 " _
                    & "     WHERE CAMPCODE =@P17 and PRODUCTCODE = @P16 and STYMD = @P5                                " _
                    & " IF ( @@FETCH_STATUS <> 0 )                                                                     " _
                    & "    INSERT INTO MD001_PRODUCT                                                                   " _
                    & "       (OILTYPE , PRODUCT1 , PRODUCT2, SEQ, STYMD , ENDYMD , NAMES, NAMEL, DELFLG,              " _
                    & "        INITYMD , UPDYMD , UPDUSER , UPDTERMID , RECEIVEYMD , STANI, PRODUCTCODE, CAMPCODE)     " _
                    & "        VALUES (@P1,@P2,@P3,@P4,@P5,@P6,@P7,@P8,@P9,@P10,@P11,@P12,@P13,@P14,@P15,@P16,@P17) ;  " _
                    & " CLOSE hensuu ;                                                                                 " _
                    & " DEALLOCATE hensuu ;                                                                            "

                Dim SQLStr1 As String =
                      " SELECT  CAMPCODE, PRODUCTCODE, OILTYPE, PRODUCT1 , PRODUCT2, SEQ, STANI, STYMD, ENDYMD, NAMES , NAMEL, DELFLG," _
                    & "    INITYMD , UPDYMD , UPDUSER , UPDTERMID , RECEIVEYMD , CAST(UPDTIMSTP as bigint) as TIMSTP" _
                    & " FROM  MD001_PRODUCT " _
                    & "  WHERE CAMPCODE = @P1 " _
                    & "    and PRODUCTCODE = @P2 " _
                    & "    and STYMD = @P3 "

                Using SQLcmd As New SqlCommand(SQLStr, SQLcon), SQLcmd1 As New SqlCommand(SQLStr1, SQLcon)
                    Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", SqlDbType.NVarChar)
                    Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", SqlDbType.NVarChar)
                    Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", SqlDbType.NVarChar)
                    Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", SqlDbType.Int)
                    Dim PARA5 As SqlParameter = SQLcmd.Parameters.Add("@P5", SqlDbType.Date)
                    Dim PARA6 As SqlParameter = SQLcmd.Parameters.Add("@P6", SqlDbType.Date)
                    Dim PARA7 As SqlParameter = SQLcmd.Parameters.Add("@P7", SqlDbType.NVarChar)
                    Dim PARA8 As SqlParameter = SQLcmd.Parameters.Add("@P8", SqlDbType.NVarChar)
                    Dim PARA9 As SqlParameter = SQLcmd.Parameters.Add("@P9", SqlDbType.NVarChar, 1)
                    Dim PARA10 As SqlParameter = SQLcmd.Parameters.Add("@P10", SqlDbType.DateTime)
                    Dim PARA11 As SqlParameter = SQLcmd.Parameters.Add("@P11", SqlDbType.DateTime)
                    Dim PARA12 As SqlParameter = SQLcmd.Parameters.Add("@P12", SqlDbType.NVarChar)
                    Dim PARA13 As SqlParameter = SQLcmd.Parameters.Add("@P13", SqlDbType.NVarChar)
                    Dim PARA14 As SqlParameter = SQLcmd.Parameters.Add("@P14", SqlDbType.DateTime)
                    Dim PARA15 As SqlParameter = SQLcmd.Parameters.Add("@P15", SqlDbType.NVarChar)
                    Dim PARA16 As SqlParameter = SQLcmd.Parameters.Add("@P16", SqlDbType.NVarChar)
                    Dim PARA17 As SqlParameter = SQLcmd.Parameters.Add("@P17", SqlDbType.NVarChar)

                    Dim PARAS1 As SqlParameter = SQLcmd1.Parameters.Add("@P1", SqlDbType.NVarChar)
                    Dim PARAS2 As SqlParameter = SQLcmd1.Parameters.Add("@P2", SqlDbType.NVarChar)
                    Dim PARAS3 As SqlParameter = SQLcmd1.Parameters.Add("@P3", SqlDbType.Date)

                    '○ＤＢ更新
                    For Each MC0013row As DataRow In MC0013tbl.Rows
                        If Trim(MC0013row("OPERATION")) = C_LIST_OPERATION_CODE.UPDATING OrElse
                           Trim(MC0013row("OPERATION")) = C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING Then
                            '※追加レコードは、MC0013tbl.Rows(i)("TIMSTP") = "0"となっているが状態のみで判定

                            PARA1.Value = MC0013row("OILTYPE")
                            PARA2.Value = MC0013row("PRODUCT1")
                            PARA3.Value = MC0013row("PRODUCT2")
                            PARA4.Value = MC0013row("SEQ")
                            PARA5.Value = MC0013row("STYMD")
                            PARA6.Value = MC0013row("ENDYMD")
                            PARA7.Value = MC0013row("PRODUCTNAMES")
                            PARA8.Value = MC0013row("PRODUCTNAMEL")
                            PARA9.Value = MC0013row("DELFLG")
                            PARA10.Value = Date.Now
                            PARA11.Value = Date.Now
                            PARA12.Value = Master.USERID
                            PARA13.Value = Master.USERTERMID
                            PARA14.Value = C_DEFAULT_YMD
                            PARA15.Value = MC0013row("STANI")
                            PARA16.Value = MC0013row("PRODUCTCODE")
                            PARA17.Value = MC0013row("CAMPCODE")

                            SQLcmd.ExecuteNonQuery()

                            MC0013row("OPERATION") = C_LIST_OPERATION_CODE.NODATA

                            '○更新ジャーナル追加
                            Try
                                PARAS1.Value = MC0013row("CAMPCODE")
                                PARAS2.Value = MC0013row("PRODUCTCODE")
                                PARAS3.Value = MC0013row("STYMD")

                                Dim JOURds As New DataSet()
                                Dim SQLadp As SqlDataAdapter

                                SQLadp = New SqlDataAdapter(SQLcmd1)
                                SQLadp.Fill(JOURds, "JOURtbl")

                                CS0020JOURNAL.TABLENM = "MD001_PRODUCT"
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

                                MC0013row("TIMSTP") = JOURds.Tables("JOURtbl").Rows(0)("TIMSTP")

                                SQLadp.Dispose()
                                SQLadp = Nothing
                            Catch ex As Exception
                                If ex.Message = "Error raised in TIMSTP" Then
                                    MC0013row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                                End If
                                Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "MD001_PRODUCT JOURNAL")

                                CS0011LOGWRITE.INFSUBCLASS = "MAIN"                         'SUBクラス名
                                CS0011LOGWRITE.INFPOSI = "DB:MD001_PRODUCT JOURNAL"
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
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "MD001_PRODUCT UPDATE_INSERT")
            CS0011LOGWRITE.INFSUBCLASS = "MAIN"
            CS0011LOGWRITE.INFPOSI = "DB:MD001_PRODUCT UPDATE_INSERT"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()

            Exit Sub
        End Try

        '○画面表示データ保存
        Master.SaveTable(MC0013tbl)

        '詳細画面クリア
        Detailbox_Clear()

        '○メッセージ表示
        Master.Output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF)

        'カーソル設定
        WF_SELMANGORG.Focus()

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
        CS0030REPORl.MAPID = GRMD0001WRKINC.MAPID
        CS0030REPORl.REPORTID = rightview.GetReportId()
        CS0030REPORl.FILEtyp = "pdf"
        CS0030REPORl.TBLDATA = MC0013tbl
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
        CS0030REPORl.MAPID = GRMD0001WRKINC.MAPID
        CS0030REPORl.PROFID = Master.PROF_REPORT
        CS0030REPORl.REPORTID = rightview.GetReportId()
        CS0030REPORl.FILEtyp = "XLSX"
        CS0030REPORl.TBLDATA = MC0013tbl
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
        WW_TBLview = New DataView(MC0013tbl)
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
        WF_Sel_LINECNT.Text = MC0013tbl.Rows(WW_Position)("LINECNT")
        WF_CAMPCODE.Text = MC0013tbl.Rows(WW_Position)("CAMPCODE")
        WF_CAMPCODE_TEXT.Text = MC0013tbl.Rows(WW_Position)("CAMPNAMES")
        WF_TORICODE.Text = MC0013tbl.Rows(WW_Position)("TORICODE")
        WF_TORICODE_TEXT.Text = MC0013tbl.Rows(WW_Position)("TORICODENAMES")
        WF_OILTYPEGRP.Text = MC0013tbl.Rows(WW_Position)("OILTYPEGRP")
        WF_OILTYPEGRP_TEXT.Text = MC0013tbl.Rows(WW_Position)("OILTYPEGRPNAMES")
        WF_URIHIYOKBN.Text = MC0013tbl.Rows(WW_Position)("URIHIYOKBN")
        WF_URIHIYOKBN_TEXT.Text = MC0013tbl.Rows(WW_Position)("URIHIYOKBNNAMES")
        '表示順をHiddenに設定
        WF_SEQ.Value = MC0013tbl.Rows(WW_Position)("SEQ").ToString()
        '有効年月日
        WF_STYMD.Text = MC0013tbl.Rows(WW_Position)("STYMD")
        WF_ENDYMD.Text = MC0013tbl.Rows(WW_Position)("ENDYMD")
        '削除フラグ
        WF_DELFLG.Text = MC0013tbl.Rows(WW_Position)("DELFLG")
        CODENAME_get("DELFLG", WF_DELFLG.Text, WW_TEXT, WW_DUMMY)
        WF_DELFLG_TEXT.Text = WW_TEXT

        '○Grid設定処理
        For Each reitem As RepeaterItem In WF_DViewRep1.Items
            '左
            WW_FILED_OBJ = CType(reitem.FindControl("WF_Rep1_FIELD_1"), Label)
            If WW_FILED_OBJ.Text <> "" Then
                '値設定
                WW_VALUE = WF_ITEM_FORMAT(WW_FILED_OBJ.text, MC0013tbl.Rows(WW_LINECNT)(WW_FILED_OBJ.Text))
                CType(reitem.FindControl("WF_Rep1_VALUE_1"), TextBox).Text = WW_VALUE
                '値（名称）設定
                CODENAME_get(WW_FILED_OBJ.Text, WW_VALUE, WW_TEXT, WW_DUMMY)
                CType(reitem.FindControl("WF_Rep1_VALUE_TEXT_1"), Label).Text = WW_TEXT
            End If

            '中央
            WW_FILED_OBJ = CType(reitem.FindControl("WF_Rep1_FIELD_2"), Label)
            If WW_FILED_OBJ.Text <> "" Then
                '値設定
                WW_VALUE = WF_ITEM_FORMAT(WW_FILED_OBJ.text, MC0013tbl.Rows(WW_LINECNT)(WW_FILED_OBJ.Text))
                CType(reitem.FindControl("WF_Rep1_VALUE_2"), TextBox).Text = WW_VALUE
                '値（名称）設定
                CODENAME_get(WW_FILED_OBJ.Text, WW_VALUE, WW_TEXT, WW_DUMMY)
                CType(reitem.FindControl("WF_Rep1_VALUE_TEXT_2"), Label).Text = WW_TEXT
            End If

            '右
            WW_FILED_OBJ = CType(reitem.FindControl("WF_Rep1_FIELD_3"), Label)
            If WW_FILED_OBJ.Text <> "" Then
                '値設定
                WW_VALUE = WF_ITEM_FORMAT(WW_FILED_OBJ.text, MC0013tbl.Rows(WW_LINECNT)(WW_FILED_OBJ.Text))
                CType(reitem.FindControl("WF_Rep1_VALUE_3"), TextBox).Text = WW_VALUE
                '値（名称）設定
                CODENAME_get(WW_FILED_OBJ.Text, WW_VALUE, WW_TEXT, WW_DUMMY)
                CType(reitem.FindControl("WF_Rep1_VALUE_TEXT_3"), Label).Text = WW_TEXT
            End If
        Next

        '○画面WF_GRID状態設定
        '状態をクリア設定
        For Each MC0013row As DataRow In MC0013tbl.Rows
            Select Case MC0013row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    MC0013row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.NODISP
                    MC0013row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED
                    MC0013row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    MC0013row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    MC0013row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End Select
        Next

        '選択明細のOperation項目に状態を設定(更新・追加・削除は編集中を設定しない)
        Select Case MC0013tbl.Rows(WW_Position)("OPERATION")
            Case C_LIST_OPERATION_CODE.NODATA
                MC0013tbl.Rows(WW_Position)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
            Case C_LIST_OPERATION_CODE.NODISP
                MC0013tbl.Rows(WW_Position)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
            Case C_LIST_OPERATION_CODE.SELECTED
                MC0013tbl.Rows(WW_Position)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
            Case C_LIST_OPERATION_CODE.UPDATING
                MC0013tbl.Rows(WW_Position)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
            Case C_LIST_OPERATION_CODE.ERRORED
                MC0013tbl.Rows(WW_Position)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
            Case Else
        End Select

        '○画面表示データ保存
        Master.SaveTable(MC0013tbl)

        WF_TORICODE.Focus()
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

        '○DetailBoxをMC0013INPtblへ退避
        Master.CreateEmptyTable(MC0013INPtbl)
        DetailBoxToMC0013INPtbl(WW_ERRCODE)
        If Not isNormal(WW_ERRCODE) Then
            Exit Sub
        End If
        '○項目チェック
        INPtbl_Check(WW_ERRCODE)

        '○GridView更新
        If isNormal(WW_ERRCODE) Then
            MC0013tbl_UPD()
        End If

        '○一覧(MC0013tbl)内で、新規追加（タイムスタンプ０）かつ削除の場合はレコード削除
        If isNormal(WW_ERRCODE) Then
            Dim WW_DEL As String = "ON"
            Do
                For i As Integer = 0 To MC0013tbl.Rows.Count - 1
                    If MC0013tbl.Rows(i)("TIMSTP") = 0 AndAlso MC0013tbl.Rows(i)("DELFLG") = C_DELETE_FLG.DELETE Then
                        MC0013tbl.Rows(i).Delete()
                        WW_DEL = "OFF"
                        Exit For
                    Else
                        If (MC0013tbl.Rows.Count - 1) <= i Then
                            WW_DEL = "ON"
                        End If
                    End If
                Next
            Loop Until WW_DEL = "ON"
        End If

        '○画面表示データ保存
        Master.SaveTable(MC0013tbl)

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

        'カーソル設定
        WF_SELMANGORG.Focus()

    End Sub

    ''' <summary>
    '''  詳細画面-テーブル退避
    ''' </summary>
    ''' <param name="O_RTNCODE"></param>
    ''' <remarks></remarks>
    Protected Sub DetailBoxToMC0013INPtbl(ByRef O_RTNCODE As String)

        Dim WW_TEXT As String = String.Empty
        Dim WW_RTN As String = String.Empty

        O_RTNCODE = C_MESSAGE_NO.NORMAL

        'MD0001テンポラリDB項目作成
        Master.CreateEmptyTable(MC0013INPtbl)

        '○入力文字置き換え & CS0007CHKテーブルレコード追加

        '○ 画面(Repeaterヘッダー情報)の使用禁止文字排除
        Master.EraseCharToIgnore(WF_CAMPCODE.Text)          '会社コード
        Master.EraseCharToIgnore(WF_TORICODE.Text)          '取引先コード
        Master.EraseCharToIgnore(WF_OILTYPEGRP.Text)        '運賃計算油種グループ
        Master.EraseCharToIgnore(WF_URIHIYOKBN.Text)        '売上費用区分
        Master.EraseCharToIgnore(WF_UNCHINCODE.Text)        '運賃コード
        Master.EraseCharToIgnore(WF_STYMD.Text)             '開始年月日
        Master.EraseCharToIgnore(WF_ENDYMD.Text)            '終了年月日
        Master.EraseCharToIgnore(WF_DELFLG.Text)            '削除フラグ

        'GridViewから未選択状態で表更新ボタンを押下時の例外を回避する 
        If String.IsNullOrEmpty(WF_Sel_LINECNT.Text) AndAlso
            String.IsNullOrEmpty(WF_TORICODE.Text) AndAlso
            String.IsNullOrEmpty(WF_OILTYPEGRP.Text) AndAlso
            String.IsNullOrEmpty(WF_URIHIYOKBN.Text) AndAlso
            String.IsNullOrEmpty(WF_UNCHINCODE.Text) AndAlso
            String.IsNullOrEmpty(WF_STYMD.Text) AndAlso
            String.IsNullOrEmpty(WF_ENDYMD.Text) AndAlso
            String.IsNullOrEmpty(WF_DELFLG.Text) Then
            Master.Output(C_MESSAGE_NO.INVALID_PROCCESS_ERROR, C_MESSAGE_TYPE.ERR, "no Detail")
            CS0011LOGWRITE.INFSUBCLASS = "DetailBoxToMC0013INPtbl"      'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "non Detail"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ERR
            CS0011LOGWRITE.TEXT = "non Detail"
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.INVALID_PROCCESS_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力

            O_RTNCODE = C_MESSAGE_NO.INVALID_PROCCESS_ERROR

            Exit Sub
        End If

        '○画面(Repeaterヘッダー情報)のテーブル退避
        Dim MC0013INProw As DataRow = MC0013INPtbl.NewRow
        '初期クリア
        For Each MC0013INPcol As DataColumn In MC0013INProw.Table.Columns
            If MC0013INPcol.DataType.Name.ToString() = "String" Then
                MC0013INProw(MC0013INPcol.ColumnName) = ""
            End If
        Next

        If (String.IsNullOrEmpty(WF_Sel_LINECNT.Text)) Then
            MC0013INProw("LINECNT") = 0
        Else
            MC0013INProw("LINECNT") = CType(WF_Sel_LINECNT.Text, Integer)   'DBの固定フィールド
        End If
        MC0013INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA            'DBの固定フィールド
        MC0013INProw("TIMSTP") = 0                                          'DBの固定フィールド
        MC0013INProw("SELECT") = "0"                                        'DBの固定フィールド
        MC0013INProw("HIDDEN") = "0"                                        'DBの固定フィールド

        MC0013INProw("CAMPCODE") = WF_CAMPCODE.Text
        MC0013INProw("TORICODE") = WF_TORICODE.Text
        MC0013INProw("OILTYPEGRP") = WF_OILTYPEGRP.Text
        MC0013INProw("URIHIYOKBN") = WF_URIHIYOKBN.Text
        MC0013INProw("UNCHINCODE") = WF_UNCHINCODE.Text
        MC0013INProw("STYMD") = WF_STYMD.Text
        MC0013INProw("ENDYMD") = WF_ENDYMD.Text
        MC0013INProw("DELFLG") = WF_DELFLG.Text

        ' 表示順を復元
        If (String.IsNullOrEmpty(WF_SEQ.Value) = True) Then
            MC0013INProw("SEQ") = 1
        Else
            MC0013INProw("SEQ") = WF_SEQ.Value
        End If
        '○Detail設定処理
        For Each reitem As RepeaterItem In WF_DViewRep1.Items
            '左
            If CType(reitem.FindControl("WF_Rep1_FIELD_1"), Label).Text <> "" Then
                CS0010CHARstr.CHARIN = CType(reitem.FindControl("WF_Rep1_VALUE_1"), TextBox).Text
                CS0010CHARstr.CS0010CHARget()
                MC0013INProw(CType(reitem.FindControl("WF_Rep1_FIELD_1"), Label).Text) = CS0010CHARstr.CHAROUT
            End If

            '中央
            If CType(reitem.FindControl("WF_Rep1_FIELD_2"), Label).Text <> "" Then
                CS0010CHARstr.CHARIN = CType(reitem.FindControl("WF_Rep1_VALUE_2"), TextBox).Text
                CS0010CHARstr.CS0010CHARget()
                MC0013INProw(CType(reitem.FindControl("WF_Rep1_FIELD_2"), Label).Text) = CS0010CHARstr.CHAROUT
            End If

            '右
            If CType(reitem.FindControl("WF_Rep1_FIELD_3"), Label).Text <> "" Then
                CS0010CHARstr.CHARIN = CType(reitem.FindControl("WF_Rep1_VALUE_3"), TextBox).Text
                CS0010CHARstr.CS0010CHARget()
                MC0013INProw(CType(reitem.FindControl("WF_Rep1_FIELD_3"), Label).Text) = CS0010CHARstr.CHAROUT
            End If
        Next

        '○コード名称を設定する
        ' 会社コード
        WW_TEXT = ""
        CODENAME_get("CAMPCODE", MC0013INProw("CAMPCODE"), WW_TEXT, WW_DUMMY)
        MC0013INProw("CAMPNAMES") = WW_TEXT

        ' 取引先コード
        WW_TEXT = ""
        CODENAME_get("TORICODE", MC0013INProw("TORICODE"), WW_TEXT, WW_DUMMY)
        MC0013INProw("TORICODENAMES") = WW_TEXT

        ' 届先コード
        WW_TEXT = ""
        CODENAME_get("TODOKECODE", MC0013INProw("TODOKECODE"), WW_TEXT, WW_DUMMY)
        MC0013INProw("TODOKECODENAMES") = WW_TEXT

        ' 業者車番
        WW_TEXT = ""
        CODENAME_get("GSHABAN", MC0013INProw("GSHABAN"), WW_TEXT, WW_DUMMY, MC0013INProw("GSHABAN"))
        MC0013INProw("GSHABANNAMES") = WW_TEXT

        ' 荷主車番
        WW_TEXT = ""
        CODENAME_get("NSHABAN", MC0013INProw("NSHABAN"), WW_TEXT, WW_DUMMY, MC0013INProw("NSHABAN"))
        MC0013INProw("NSHABANNAMES") = WW_TEXT

        ' 売上仕入区分
        WW_TEXT = ""
        CODENAME_get("URISHIIREKBN", MC0013INProw("URISHIIREKBN"), WW_TEXT, WW_DUMMY)
        MC0013INProw("URISHIIREKBNNAMES") = WW_TEXT

        ' チェック用テーブルに登録する
        MC0013INPtbl.Rows.Add(MC0013INProw)

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

        '○カーソル設定
        WF_SELMANGORG.Focus()

    End Sub
    ''' <summary>
    ''' 詳細画面-クリア処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub Detailbox_Clear()

        '○画面WF_GRID状態設定
        '状態をクリア設定
        For Each MC0013row As DataRow In MC0013tbl.Rows
            Select Case MC0013row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    MC0013row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.NODISP
                    MC0013row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED
                    MC0013row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    MC0013row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    MC0013row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End Select
        Next

        '○画面表示データ保存
        Master.SaveTable(MC0013tbl)

        '画面(Grid)のHIDDEN列により、表示/非表示を行う。

        WF_Sel_LINECNT.Text = ""
        WF_CAMPCODE.Text = work.WF_SEL_CAMPCODE.Text
        CODENAME_get("CAMPCODE", WF_CAMPCODE.Text, WF_CAMPCODE_TEXT.Text, WW_DUMMY)
        WF_TORICODE.Text = ""
        WF_TORICODE_TEXT.Text = ""
        WF_OILTYPEGRP.Text = ""
        WF_OILTYPEGRP_TEXT.Text = ""
        WF_URIHIYOKBN.Text = ""
        WF_URIHIYOKBN_TEXT.Text = ""
        WF_UNCHINCODE.Text = ""
        WF_STYMD.Text = ""
        WF_ENDYMD.Text = ""
        WF_DELFLG.Text = ""
        WF_SEQ.Value = ""

        '○Detail初期設定
        Repeater_INIT()

        WF_SELMANGORG.Focus()

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
            Case "STANI"
                ' 請求単位
                O_ATTR = "REF_Field_DBclick('STANI', 'WF_Rep_FIELD' , '999');"

        End Select

    End Sub
    ''' <summary>
    ''' フィールドダブルクリック処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_Field_DBClick()
        '○LeftBox処理（フィールドダブルクリック時）
        If Not String.IsNullOrEmpty(WF_LeftMViewChange.Value) Then
            Try
                Integer.TryParse(WF_LeftMViewChange.Value, WF_LeftMViewChange.Value)
            Catch ex As Exception
                Exit Sub
            End Try
            With leftview
                If WF_LeftMViewChange.Value <> LIST_BOX_CLASSIFICATION.LC_CALENDAR Then

                    Dim prmData As New Hashtable
                    prmData.Item(C_PARAMETERS.LP_COMPANY) = WF_CAMPCODE.Text

                    'フィールドによってパラメーターを変える
                    Select Case WF_FIELD.Value
                        Case "WF_TORICOE"          '取引先
                            prmData = work.CreateTORIParam(WF_CAMPCODE.Text)
                        Case "WF_OILTYPEGRP"       '運賃計算油種グループ
                            prmData = work.CreateFIXParam(WF_CAMPCODE.Text, "OILTYPEGRP")
                        Case "WF_URIHIYOKBN"       '売上費用区分
                            prmData = work.CreateFIXParam(WF_CAMPCODE.Text, "URIHIYOKBN")
                    End Select

                Else
                    '日付の場合、入力日付のカレンダーが表示されるように入力値をカレンダーに渡す
                    Select Case WF_FIELD.Value
                        Case "WF_STYMD"
                            .WF_Calendar.Text = WF_STYMD.Text
                        Case "WF_ENDMD"
                            .WF_Calendar.Text = WF_ENDYMD.Text

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

                Case "WF_SELMANORG"
                    WF_SELMANGORG_TEXT.Text = WW_SelectTEXT
                    WF_SELMANGORG.Text = WW_SelectValue
                    WF_SELMANGORG.Focus()

                Case "WF_SELSHIPORG"
                    WF_SELSHIPORG_TEXT.Text = WW_SelectTEXT
                    WF_SELSHIPORG.Text = WW_SelectValue
                    WF_SELSHIPORG.Focus()

                Case "WF_TORICODE"
                    WF_TORICODE_TEXT.Text = WW_SelectTEXT
                    WF_TORICODE.Text = WW_SelectValue
                    WF_TORICODE.Focus()

                Case "WF_OILTYPEGRP"
                    WF_OILTYPEGRP_TEXT.Text = WW_SelectTEXT
                    WF_OILTYPEGRP.Text = WW_SelectValue
                    WF_OILTYPEGRP.Focus()

                Case "WF_URIHIYOKBN"
                    WF_URIHIYOKBN.Text = WW_SelectTEXT
                    WF_URIHIYOKBN.Text = WW_SelectValue
                    WF_URIHIYOKBN.Focus()

                Case "WF_UNCHINCODE"
                    WF_UNCHINCODE.Text = WW_SelectValue
                    WF_UNCHINCODE.Focus()

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

        '○フォーカスセット
        Select Case WF_FIELD.Value

            Case "WF_SELMANGORG"
                WF_SELMANGORG.Focus()   '管理部門（絞り込み）

            Case "WF_SELSHIPORG"
                WF_SELSHIPORG.Focus()   '出荷部門（絞り込み）

            Case "WF_OILTYPEGRP"        '運賃計算油種グループ（キー部）
                WF_OILTYPEGRP.Focus()

            Case "WF_URIHIYOKBN"
                WF_URIHIYOKBN.Focus()

            Case "WF_UNCHINCODE"
                WF_UNCHINCODE.Focus()

            Case "WF_STYMD"
                WF_STYMD.Focus()

            Case "WF_ENDYMD"
                WF_ENDYMD.Focus()

            Case "WF_DELFLG"
                WF_DELFLG.Focus()

        End Select

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

        Master.CreateEmptyTable(MC0013INPtbl)

        '○UPLOAD_XLSデータ取得        
        CS0023XLSUPLOAD.CAMPCODE = work.WF_SEL_CAMPCODE.Text
        CS0023XLSUPLOAD.MAPID = GRMD0001WRKINC.MAPID
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
        If WW_COLUMNS.IndexOf("STYMD") < 0 OrElse
           WW_COLUMNS.IndexOf("ENDYMD") < 0 OrElse
           WW_COLUMNS.IndexOf("CAMPCODE") < 0 OrElse
           WW_COLUMNS.IndexOf("TORICODE") < 0 OrElse
           WW_COLUMNS.IndexOf("TODOKECODE") < 0 OrElse
           WW_COLUMNS.IndexOf("GSHABAN") < 0 OrElse
           WW_COLUMNS.IndexOf("NSHABAN") < 0 Then
            ' インポート出来ません(項目： ?01 が存在しません)。
            Master.Output(C_MESSAGE_NO.IMPORT_ERROR, C_MESSAGE_TYPE.ERR, "Inport TITLE not find")
            Exit Sub
        End If

        '○Excelデータ毎にチェック＆更新
        For Each XLSTBLrow As DataRow In CS0023XLSUPLOAD.TBLDATA.Rows
            '○XLSTBL明細⇒MC0013INProw
            Dim MC0013INProw = MC0013INPtbl.NewRow

            '初期クリア
            For Each MD0001INPcol As DataColumn In MC0013INPtbl.Columns

                If IsDBNull(MC0013INProw.Item(MD0001INPcol)) OrElse IsNothing(MC0013INProw.Item(MD0001INPcol)) Then
                    Select Case MD0001INPcol.ColumnName
                        Case "LINECNT"
                            MC0013INProw.Item(MD0001INPcol) = 0
                        Case "TIMSTP"
                            MC0013INProw.Item(MD0001INPcol) = 0
                        Case "SELECT"
                            MC0013INProw.Item(MD0001INPcol) = 1
                        Case "HIDDEN"
                            MC0013INProw.Item(MD0001INPcol) = 0
                        Case "SEQ"
                            MC0013INProw.Item(MD0001INPcol) = 0
                        Case Else
                            If MD0001INPcol.DataType.Name = "String" Then
                                MC0013INProw.Item(MD0001INPcol) = ""
                            ElseIf MD0001INPcol.DataType.Name = "DateTime" Then
                                MC0013INProw.Item(MD0001INPcol) = C_DEFAULT_YMD
                            Else
                                MC0013INProw.Item(MD0001INPcol) = 0
                            End If
                    End Select
                End If
            Next

            '○変更元情報をデフォルト設定
            Dim WW_STYMD As String = ""

            If WW_COLUMNS.IndexOf("CAMPCODE") >= 0 AndAlso
               WW_COLUMNS.IndexOf("TORICODE") >= 0 AndAlso
               WW_COLUMNS.IndexOf("TODOKECODE") >= 0 AndAlso
               WW_COLUMNS.IndexOf("GSHABAN") >= 0 AndAlso
               WW_COLUMNS.IndexOf("NSHABAN") >= 0 AndAlso
               WW_COLUMNS.IndexOf("STYMD") >= 0 Then

                For Each MC0013row As DataRow In MC0013tbl.Rows
                    If XLSTBLrow("CAMPCODE") = MC0013row("CAMPCODE") AndAlso
                       XLSTBLrow("TORICODE") = MC0013row("TORICODE") AndAlso
                       XLSTBLrow("TODOKECODE") = MC0013row("TODOKECODE") AndAlso
                       XLSTBLrow("GSHABAN") = MC0013row("GSHABAN") AndAlso
                       XLSTBLrow("NSHABAN") = MC0013row("NSHABAN") AndAlso
                       XLSTBLrow("STYMD") = MC0013row("STYMD") Then
                        MC0013INProw.ItemArray = MC0013row.ItemArray
                        Exit For
                    End If
                Next
            End If

            '○項目セット
            '会社コード
            If WW_COLUMNS.IndexOf("CAMPCODE") >= 0 Then
                MC0013INProw("CAMPCODE") = XLSTBLrow("CAMPCODE")
            End If

            '取引先コード
            If WW_COLUMNS.IndexOf("TORICODE") >= 0 Then
                MC0013INProw("TORICODE") = XLSTBLrow("TORICODE")
            End If

            '取引先名
            If WW_COLUMNS.IndexOf("TORICODENAMES") >= 0 Then
                MC0013INProw("TORICODENAMES") = XLSTBLrow("TORICODENAMES")
            End If

            '届先コード
            If WW_COLUMNS.IndexOf("TODOKECODE") >= 0 Then
                MC0013INProw("TODOKECODE") = XLSTBLrow("TODOKECODE")
            End If

            '届先名
            If WW_COLUMNS.IndexOf("TODOKENAMES") >= 0 Then
                MC0013INProw("TODOKENAMES") = XLSTBLrow("TODOKENAMES")
            End If

            '業者車番
            If WW_COLUMNS.IndexOf("GSHABAN") >= 0 Then
                MC0013INProw("GSHABAN") = XLSTBLrow("GSHABAN")
            End If

            '業者車番名
            If WW_COLUMNS.IndexOf("GSHABANNAMES") >= 0 Then
                MC0013INProw("GSHABANNAMES") = XLSTBLrow("GSHABANNAMES")
            End If

            '荷主車番
            If WW_COLUMNS.IndexOf("NSHABAN") >= 0 Then
                MC0013INProw("NSHABAN") = XLSTBLrow("NSHABAN")
            End If

            '荷主車番名
            If WW_COLUMNS.IndexOf("NSHABANNAMES") >= 0 Then
                MC0013INProw("NSHABANNAMES") = XLSTBLrow("NSHABANNAMES")
            End If


            '有効開始日
            If WW_COLUMNS.IndexOf("STYMD") >= 0 Then
                If IsDate(XLSTBLrow("STYMD")) Then
                    Dim WW_DATE As Date
                    Date.TryParse(XLSTBLrow("STYMD"), WW_DATE)
                    MC0013INProw("STYMD") = WW_DATE.ToString("yyyy/MM/dd")
                End If
            End If

            '有効終了日
            If WW_COLUMNS.IndexOf("ENDYMD") >= 0 Then
                If IsDate(XLSTBLrow("ENDYMD")) Then
                    Dim WW_DATE As Date
                    Date.TryParse(XLSTBLrow("ENDYMD"), WW_DATE)
                    MC0013INProw("ENDYMD") = WW_DATE.ToString("yyyy/MM/dd")
                End If
            End If

            '削除
            If WW_COLUMNS.IndexOf("DELFLG") >= 0 Then
                MC0013INProw("DELFLG") = XLSTBLrow("DELFLG")
            End If

            MC0013INPtbl.Rows.Add(MC0013INProw)
        Next

        '○項目チェック
        INPtbl_Check(WW_ERRCODE)

        '○画面表示テーブル更新
        If isNormal(WW_ERRCODE) Then
            MC0013tbl_UPD()
        End If

        '○画面表示データ保存
        Master.SaveTable(MC0013tbl)

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
        If Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.MC0013S Then

            Master.MAPID = GRMD0001WRKINC.MAPID
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
    ''' <remarks>データベース（MD001_PRODUCT）を検索し画面表示する一覧を作成する</remarks>
    Private Sub MAPDATAget()

        '○画面表示用データ取得

        Try
            'MC0010テンポラリDB項目作成
            If MC0013tbl Is Nothing Then
                MC0013tbl = New DataTable
            End If

            If MC0013tbl.Columns.Count <> 0 Then
                MC0013tbl.Columns.Clear()
            End If

            '○DB項目クリア
            MC0013tbl.Clear()

            '○テーブル検索結果をテーブル退避
            'MD0001テンポラリDB項目作成

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
                      " SELECT  0                                      as LINECNT             , " _
                    & "         ''                                     as OPERATION           , " _
                    & "         TIMSTP = cast(isnull(MC13.UPDTIMSTP,0) as bigint)             , " _
                    & "         1                                      as 'SELECT'            , " _
                    & "         0                                      as HIDDEN              , " _
                    & "         rtrim(MC13.CAMPCODE)                   as CAMPCODE            , " _
                    & "         rtrim(M01.NAMES)                       as CAMPNAMES           , " _
                    & "         rtrim(MC13.TORICODE)                   as TORICODE            , " _
                    & "         rtrim(MC02.NAMES)                      as TORICODENAMES       , " _
                    & "         rtrim(MC13.OILTYPEGRP)                 as OILTYPEGRP          , " _
                    & "         rtrim(MC1OIL.VALUE1)                   as OILTYPEGRPNAMES     , " _
                    & "         rtrim(MC13.URIHIYOKBN)                 as URIHIYOKBN          , " _
                    & "         rtrim(MC1URI.VALUE1)                   as URIHIYOKBNNAMES     , " _
                    & "         rtrim(MC13.UNCHINCODE)                 as UNCHINCODE          , " _
                    & "         rtrim(MC13.UNCHINCODENAME)             as UNCHINCODENAME      , " _
                    & "         format(MC13.STYMD, 'yyyy/MM/dd')       as STYMD               , " _
                    & "         format(MC13.ENDYMD, 'yyyy/MM/dd')      as ENDYMD              , " _
                    & "         rtrim(MC13.UNCHINORG)                  as UNCHINORG           , " _
                    & "         rtrim(MC02.NAMES)                      as UNCHINORGNAMES      , " _
                    & "         rtrim(MC13.GYOSHA)                     as GYOSHA              , " _
                    & "         rtrim(MC13.MANGORG)                    as MANGORG             , " _
                    & "         rtrim(MC02.NAMES)                      as MANGORGNAMES        , " _
                    & "         rtrim(MC13.SHIPORG)                    as SHIPORG             , " _
                    & "         rtrim(MC02.NAMES)                      as SHIPORGNAMES        , " _
                    & "         rtrim(MC13.NSHABAN)                    as NSHABAN             , " _
                    & "         rtrim(MC13.SHUKABASHO)                 as SHUKABASHO          , " _
                    & "         rtrim(MC02.NAMES)                      as SHUKABASHONAMES     , " _
                    & "         rtrim(MC13.TODOKECODE)                 as TODOKECODE          , " _
                    & "         rtrim(MC06.NAMES)                      as TODOKECODENAMES     , " _
                    & "         rtrim(MC13.SHAFUKU)                    as SHAFUKU             , " _
                    & "         rtrim(MC13.SPOTRESCUEKBN)              as SPOTRESCUEKBN       , " _
                    & "         rtrim(MC1SYA.VALUE1)                   as SPOTRESCUEKBNNAMES  , " _
                    & "         rtrim(MC13.SHARYOCLASS  )              as SHARYOCLASS         , " _
                    & "         rtrim(MC1SCS.VALUE1)                   as SHARYOCLASSNAMES    , " _
                    & "         rtrim(MC13.OPEKBN)                     as OPEKBN              , " _
                    & "         rtrim(MC1OPE.VALUE1)                   as OPEKBNNAMES         , " _
                    & "         rtrim(MC13.INDATAKBN)                  as INDATAKBN           , " _
                    & "         rtrim(MC1INDATA.VALUE1)                as INDATAKBNNAMES      , " _
                    & "         rtrim(MC13.UNCHINCALCKBN)              as UNCHINCALCKBN       , " _
                    & "         rtrim(MC1UTN.VALUE1)                   as UNCHINCALCKBNNAMES  , " _
                    & "         rtrim(MC13.ROUNDKBN)                   as ROUNDKBN            , " _
                    & "         rtrim(MC1MR1.VALUE1)                   as ROUNDKBNNAMES       , " _
                    & "         rtrim(MC13.ROUNDDIGIT)                 as ROUNDDIGIT          , " _
                    & "         rtrim(MC1MR2.VALUE1)                   as ROUNDDIGITNAMES     , " _
                    & "         rtrim(MC13.COST)                       as COST                , " _
                    & "         rtrim(MC13.PATERNKBN)                  as PATERNKBN           , " _
                    & "         rtrim(MC13.SHIWAKEPATTERN)             as SHIWAKEPATTERN      , " _
                    & "         rtrim(ML03.PATERNNAME)                 as PATERNNAME          , " _
                    & "         rtrim(MC13.SEIKYUSUMKBN)               as SEIKYUSUMKBN        , " _
                    & "         rtrim(MC1SEI.VALUE1)                   as SEIKYUSUMKBNNAMES   , " _
                    & "         rtrim(MC13.OUTPUTSUMKBN)               as OUTPUTSUMKBN          , " _
                    & "         rtrim(MC1OUT.VALUE1)                   as OUTPUTSUMKBNNAMES     , " _
                    & "         rtrim(MC13.DELFLG)                     as DELFLG              , " _
                    & "         ''                                     as INITYMD             , " _
                    & "         ''                                     as UPDYMD              , " _
                    & "         ''                                     as UPDUSER             , " _
                    & "         ''                                     as UPDTERMID           , " _
                    & "         ''                                     as RECEIVEYMD          , " _
                    & "         ''                                     as UPDTIMSTP             " _
                    & " FROM                                                                    " _
                    & "           MC013_UNCHINKETEI MC13                                        " _
                    & " --会社名取得                                                            " & vbCrLf _
                    & " LEFT JOIN M0001_CAMP M01                                             ON " _
                    & "           M01.CAMPCODE    = MC13.CAMPCODE                               " _
                    & "      and  M01.STYMD      <= @P4                                         " _
                    & "      and  M01.ENDYMD     >= @P4                                         " _
                    & "      and  M01.DELFLG     <> '1'                                         " _
                    & " --取引先名取得                                                          " & vbCrLf _
                    & " LEFT JOIN MC002_TORIHIKISAKI MC02                                    ON " _
                    & "           MC02.CAMPCODE    = MC13.CAMPCODE                              " _
                    & "      and  MC02.TORICODE    = MC13.TORICODE                              " _
                    & "      and  MC02.STYMD      <= @P4                                        " _
                    & "      and  MC02.ENDYMD     >= @P4                                        " _
                    & "      and  MC02.DELFLG     <> '1'                                        " _
                    & " --運賃計算油種グループ取得                                                      " & vbCrLf _
                    & " LEFT JOIN MC001_FIXVALUE MC1OIL                                      ON " _
                    & "           MC1URI.CAMPCODE = MC13.CAMPCODE                               " _
                    & "      and  MC1URI.CLASS    = 'OILTYPEGRP'                                " _
                    & "      and  MC1URI.KEYCODE  =  MC13.OILTYPEGRP                            " _
                    & "      and  MC1URI.STYMD   <= @P4           　　　                        " _
                    & "      and  MC1URI.ENDYMD  >= @P4                 　　　                  " _
                    & "      and  MC1URI.DELFLG  <> '1'                       　　　            " _
                    & " --売上費用区分取得                                                      " & vbCrLf _
                    & " LEFT JOIN MC001_FIXVALUE MC1URI                                      ON " _
                    & "           MC1URI.CAMPCODE = MC13.CAMPCODE                               " _
                    & "      and  MC1URI.CLASS    = 'URIHIYOKBN'                                " _
                    & "      and  MC1URI.KEYCODE  =  MC13.URIHIYOKBN                            " _
                    & "      and  MC1URI.STYMD   <= @P4           　　　                        " _
                    & "      and  MC1URI.ENDYMD  >= @P4                 　　　                  " _
                    & "      and  MC1URI.DELFLG  <> '1'                       　　　            " _
                    & " --届先名取得                                                            " & vbCrLf _
                    & " LEFT JOIN MC006_TODOKESAKI  MC06                                     ON " _
                    & "           MC06.CAMPCODE    = MC13.CAMPCODE                              " _
                    & "      and  MC06.TORICODE    = MC13.TORICODE                              " _
                    & "      and  MC06.TODOKECODE  = MC13.TODOKECODE                            " _
                    & "      and  MC06.STYMD      <= @P4                                        " _
                    & "      and  MC06.ENDYMD     >= @P4                                        " _
                    & "      and  MC06.DELFLG     <> '1'                                        " _
                    & " --売上仕入区分取得                                                      " & vbCrLf _
                    & " LEFT JOIN MC001_FIXVALUE MC1URI                                      ON " _
                    & "           MC1URI.CAMPCODE = MC13.CAMPCODE                               " _
                    & "      and  MC1URI.CLASS    = 'URISHIIREKBN'                               " _
                    & "      and  MC1URI.KEYCODE  =  MC13.URISHIIREKBN                           " _
                    & "      and  MC1URI.STYMD   <= @P4           　　　                        " _
                    & "      and  MC1URI.ENDYMD  >= @P4                 　　　                  " _
                    & "      and  MC1URI.DELFLG  <> '1'                       　　　            " _
                    & " --操作区分取得                                              　　        " & vbCrLf _
                    & " LEFT JOIN MC001_FIXVALUE MC1OPE              　　　                  ON " _
                    & "           MC1OPE.CAMPCODE = MC13.CAMPCODE          　　　               " _
                    & "      and  MC1OPE.CLASS   = 'OPEKBN'    　                　　　         " _
                    & "      and  MC1OPE.KEYCODE =  MC13.OPEKBN                        　　　   " _
                    & "      and  MC1OPE.STYMD  <= @P4         　　　                           " _
                    & "      and  MC1OPE.ENDYMD >= @P4               　　　                     " _
                    & "      and  MC1OPE.DELFLG <> '1'                     　　　               " _
                    & " --運賃計算元情報取得                                        　　        " & vbCrLf _
                    & " LEFT JOIN MC001_FIXVALUE MC1INDATA                       　　　      ON " _
                    & "           MC1INDATA.CAMPCODE = MC13.CAMPCODE  　　　                    " _
                    & "      and  MC1INDATA.CLASS   = 'INDATAKBN'       　　　                  " _
                    & "      and  MC1INDATA.KEYCODE =  MC13.INDATAKBN         　　　            " _
                    & "      and  MC1INDATA.STYMD  <= @P4                           　　　      " _
                    & "      and  MC1INDATA.ENDYMD >= @P4                                　　　 " _
                    & "      and  MC1INDATA.DELFLG <> '1'              　　　                   " _
                    & " --運賃コード取得                                            　　        " & vbCrLf _
                    & " LEFT JOIN MC001_FIXVALUE MC1UNTIN                        　　　      ON " _
                    & "           MC1UNTIN.CAMPCODE = MC13.CAMPCODE                             " _
                    & "      and  MC1UNTIN.CLASS   = 'UNCHINCODE'                               " _
                    & "      and  MC1UNTIN.KEYCODE =  MC13.UNCHINCODE                           " _
                    & "      and  MC1UNTIN.STYMD  <= @P4                                        " _
                    & "      and  MC1UNTIN.ENDYMD >= @P4                                        " _
                    & "      and  MC1UNTIN.DELFLG <> '1'                                        " _
                    & " --運賃計算方法取得                                                    　" & vbCrLf _
                    & " LEFT JOIN MC001_FIXVALUE MC1UNTINC                                   ON " _
                    & "           MC1UNTINC.CAMPCODE = MC13.CAMPCODE                            " _
                    & "      and  MC1UNTINC.CLASS   = 'UNCHINCALCKBN'                           " _
                    & "      and  MC1UNTINC.KEYCODE =  MC13.UNCHINCALCKBN                       " _
                    & "      and  MC1UNTINC.STYMD  <= @P4                                       " _
                    & "      and  MC1UNTINC.ENDYMD >= @P4                                       " _
                    & "      and  MC1UNTINC.DELFLG <> '1'                                       " _
                    & " --請求書明細区分取得                                                  　" & vbCrLf _
                    & " LEFT JOIN MC001_FIXVALUE MC1SEIKYU                                   ON " _
                    & "           MC1SEIKYU.CAMPCODE = MC13.CAMPCODE                            " _
                    & "      and  MC1SEIKYU.CLASS   = 'SEIKYUKBN'                               " _
                    & "      and  MC1SEIKYU.KEYCODE =  MC13.SEIKYUKBN                           " _
                    & "      and  MC1SEIKYU.STYMD  <= @P4                                       " _
                    & "      and  MC1SEIKYU.ENDYMD >= @P4                                       " _
                    & "      and  MC1SEIKYU.DELFLG <> '1'                                       " _
                    & " --仕訳パターン取得                                                    　" & vbCrLf _
                    & " LEFT JOIN ML003_SHIWAKEPATTERN ML03                                  ON " _
                    & "           ML03.CAMPCODE   = MC13.CAMPCODE                               " _
                    & "      and  ML03.PATERNKBN  = MC13.PATERNKBN                              " _
                    & "      and  ML03.PATERNCODE = MC13.SHIWAKEPATTERN                         " _
                    & "      and  ML03.STYMD  <= @P4                                            " _
                    & "      and  ML03.ENDYMD >= @P4                                            " _
                    & "      and  ML03.DELFLG <> '1'                                            " _
                    & " WHERE                                                                   " _
                    & "           MC13.CAMPCODE    = @P1                                        " _
                    & "      and  MC13.STYMD      <= @P2                                        " _
                    & "      and  MC13.ENDYMD     >= @P3                                        " _
                    & "      and  MC13.DELFLG     <> '1'                                        " _
                    & " ORDER BY                                                                " _
                    & "      CAMPCODE, TORICODE, TODOKECODE, GSHABAN, NSHABAN, STYMD            "

                Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                    Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", SqlDbType.NVarChar, 20)
                    Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", SqlDbType.Date)
                    Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", SqlDbType.Date)
                    Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", SqlDbType.Date)

                    PARA1.Value = work.WF_SEL_CAMPCODE.Text
                    PARA2.Value = work.WF_SEL_ENDYMD.Text
                    PARA3.Value = work.WF_SEL_STYMD.Text
                    PARA4.Value = Date.Now

                    Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                        'フィールド名とフィールドの型を取得
                        For index As Integer = 0 To SQLdr.FieldCount - 1
                            MC0013tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                        Next

                        'MC0013tbl値設定
                        While SQLdr.Read

                            '2次抽出判定フラグ
                            Dim WW_SELECT_FLAG As Integer = 0    '0:対象外、1:対象

                            Dim MC0013row As DataRow = MC0013tbl.NewRow()

                            '○テンポラリTable追加
                            '固定項目
                            MC0013row("LINECNT") = SQLdr("LINECNT")
                            MC0013row("OPERATION") = SQLdr("OPERATION")
                            MC0013row("TIMSTP") = SQLdr("TIMSTP")
                            MC0013row("HIDDEN") = SQLdr("HIDDEN")

                            '画面毎の設定項目
                            MC0013row("CAMPCODE") = SQLdr("CAMPCODE")
                            MC0013row("CAMPNAMES") = SQLdr("CAMPNAMES")
                            MC0013row("TORICODE") = SQLdr("TORICODE")
                            MC0013row("TORICODENAMES") = SQLdr("TORICODENAMES")
                            MC0013row("TODOKECODE") = SQLdr("TODOKECODE")
                            MC0013row("TODOKECODENAMES") = SQLdr("TODOKECODENAMES")
                            MC0013row("GSHABAN") = SQLdr("GSHABAN")
                            MC0013row("NSHABAN") = SQLdr("NSHABAN")
                            MC0013row("STYMD") = CDate(SQLdr("STYMD")).ToString("yyyy/MM/dd")
                            MC0013row("ENDYMD") = CDate(SQLdr("ENDYMD")).ToString("yyyy/MM/dd")
                            MC0013row("URISHIIREKBN") = SQLdr("URISHIIREKBN")
                            MC0013row("URISHIIREKBNNAMES") = SQLdr("URISHIIREKBNNAMES")
                            MC0013row("OPEKBN") = SQLdr("OPEKBN")
                            MC0013row("OPEKBNNAMES") = SQLdr("OPEKBNNAMES")
                            MC0013row("INDATAKBN") = SQLdr("INDATAKBN")
                            MC0013row("INDATAKBNNAMES") = SQLdr("INDATAKBNNAMES")
                            MC0013row("UNCHINCODE") = SQLdr("UNCHINCODE")
                            MC0013row("UNCHINCODENNAMES") = SQLdr("UNCHINCODENNAMES")
                            MC0013row("UNCHINCALCKBN") = SQLdr("UNTINCALCKBN")
                            MC0013row("UNCHINCALCKBNNAMES") = SQLdr("UNTINCALCKBNNAMES")
                            MC0013row("COST") = SQLdr("COST")
                            MC0013row("PATERNKBN") = SQLdr("PATERNKBN")
                            MC0013row("SHIWAKEPATTERN") = SQLdr("SHIWAKEPATTERN")
                            MC0013row("SHIWAKEPATTERNNAMES") = SQLdr("SHIWAKEPATTERNNAMES")
                            MC0013row("SEIKYUKBN") = SQLdr("SEIKYUKBN")
                            MC0013row("SEIKYUKBNNAMES") = SQLdr("SEIKYUKBNNAMES")
                            MC0013row("DELFLG") = SQLdr("DELFLG")

                            WW_SELECT_FLAG = 1

                            If WW_SELECT_FLAG = 1 Then      'SELECT ... 1：対象,0：対象外
                                MC0013row("SELECT") = 1
                            Else
                                MC0013row("SELECT") = 0
                            End If

                            '抽出対象外の場合、レコード追加しない
                            If MC0013row("SELECT") = 1 Then
                                MC0013tbl.Rows.Add(MC0013row)
                            End If
                        End While
                    End Using
                End Using
            End Using
        Catch ex As Exception
            'ログ出力
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "MC013_UNCHINKETEI SELECT")
            CS0011LOGWRITE.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:MC013_UNCHINKETEI Select"
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
        CS0026TBLSORT.TABLE = MC0013tbl
        CS0026TBLSORT.TAB = ""
        CS0026TBLSORT.FILTER = ""
        CS0026TBLSORT.SortandNumbring()
        If isNormal(CS0026TBLSORT.ERR) Then
            MC0013tbl = CS0026TBLSORT.TABLE
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

        For Each MC0013INProw As DataRow In MC0013INPtbl.Rows

            WW_LINEERR_SW = ""
            '○単項目チェック(会社コード)
            WW_TEXT = MC0013INProw("CAMPCODE")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "CAMPCODE", MC0013INProw("CAMPCODE"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '存在チェック
                If WW_TEXT = "" Then
                    MC0013INProw("CAMPCODE") = ""
                Else
                    CODENAME_get("CAMPCODE", MC0013INProw("CAMPCODE"), WW_DUMMY, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・更新できないレコード(会社エラー)です。"
                        WW_CheckMES2 = ""
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, MC0013INProw)
                        O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                        WW_LINEERR_SW = "ERR"
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(会社コードエラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, MC0013INProw)
                O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                WW_LINEERR_SW = "ERR"
            End If

            '○単項目チェック(取引先コード)
            WW_TEXT = MC0013INProw("TORICODE")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "TORICODE", MC0013INProw("TORICODE"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '存在チェック
                If WW_TEXT = "" Then
                    MC0013INProw("TORICODE") = ""
                Else
                    CODENAME_get("TORICODE", MC0013INProw("TORICODE"), WW_DUMMY, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・更新できないレコード(取引先コードエラー)です。"
                        WW_CheckMES2 = ""
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, MC0013INProw)
                        O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                        WW_LINEERR_SW = "ERR"
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(取引先コードエラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, MC0013INProw)
                O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                WW_LINEERR_SW = "ERR"
            End If

            '○単項目チェック(届先コード)
            WW_TEXT = MC0013INProw("TODOKECODE")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "TODOKECODE", MC0013INProw("TODOKECODE"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '存在チェック
                If WW_TEXT = "" Then
                    MC0013INProw("TODOKECODE") = ""
                Else
                    CODENAME_get("TODOKECODE", MC0013INProw("TODOKECODE"), WW_DUMMY, WW_RTN_SW, MC0013INProw("TODOKECODE"))
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・更新できないレコード(届先コードエラー)です。"
                        WW_CheckMES2 = ""
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, MC0013INProw)
                        O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                        WW_LINEERR_SW = "ERR"
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(届先コードエラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, MC0013INProw)
                O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                WW_LINEERR_SW = "ERR"
            End If


            '○単項目チェック(業者車番)
            WW_TEXT = MC0013INProw("GSHABAN")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "GSHABAN", MC0013INProw("GSHABAN"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '存在チェック
                If WW_TEXT = "" Then
                    MC0013INProw("GSHABAN") = ""
                Else
                    CODENAME_get("GSHABAN", MC0013INProw("GSHABAN"), WW_DUMMY, WW_RTN_SW, MC0013INProw("GSHABAN"))
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・更新できないレコード(業者車番エラー)です。"
                        WW_CheckMES2 = ""
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, MC0013INProw)
                        O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                        WW_LINEERR_SW = "ERR"
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(業者車番エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, MC0013INProw)
                O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                WW_LINEERR_SW = "ERR"
            End If

            '○単項目チェック(荷主車番)
            WW_TEXT = MC0013INProw("NSHABAN")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "NSHABAN", MC0013INProw("NSHABAN"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '存在チェック
                If WW_TEXT = "" Then
                    MC0013INProw("NSHABAN") = ""
                Else
                    CODENAME_get("NSHABAN", MC0013INProw("NSHABAN"), WW_DUMMY, WW_RTN_SW, MC0013INProw("NSHABAN"))
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・更新できないレコード(荷主車番エラー)です。"
                        WW_CheckMES2 = ""
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, MC0013INProw)
                        O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                        WW_LINEERR_SW = "ERR"
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(荷主車番エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, MC0013INProw)
                O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                WW_LINEERR_SW = "ERR"
            End If


            '○単項目チェック(有効開始日付)
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "STYMD", MC0013INProw("STYMD"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(有効日付：開始エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, MC0013INProw)
                O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                WW_LINEERR_SW = "ERR"
            End If

            '○単項目チェック(有効終了日付)
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "ENDYMD", MC0013INProw("ENDYMD"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(有効日付：終了エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, MC0013INProw)
                O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                WW_LINEERR_SW = "ERR"
            End If

            '○単項目チェック(品名名称（短）)
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "PRODUCTNAMES", MC0013INProw("PRODUCTNAMES"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(品名名称（短）エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, MC0013INProw)
                O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                WW_LINEERR_SW = "ERR"
            End If

            '○単項目チェック(品名名称（長）)
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "PRODUCTNAMEL", MC0013INProw("PRODUCTNAMEL"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(品名名称（長）エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, MC0013INProw)
                O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                WW_LINEERR_SW = "ERR"
            End If

            '○単項目チェック(DELFLG)
            If MC0013INProw("DELFLG") = "" OrElse MC0013INProw("DELFLG") = C_DELETE_FLG.ALIVE OrElse MC0013INProw("DELFLG") = C_DELETE_FLG.DELETE Then
                If MC0013INProw("DELFLG") = "" Then
                    MC0013INProw("DELFLG") = C_DELETE_FLG.ALIVE
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(削除CD不正)です。"
                WW_CheckMES2 = ""
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, MC0013INProw)
                O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                WW_LINEERR_SW = "ERR"
            End If

            '○操作設定
            If WW_LINEERR_SW = "" Then
                If MC0013INProw("OPERATION") <> C_LIST_OPERATION_CODE.ERRORED Then
                    MC0013INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                End If
            Else
                MC0013INProw("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
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
        For Each MC0013INProw As DataRow In MC0013tbl.Rows

            '読み飛ばし
            If (MC0013INProw("OPERATION") <> C_LIST_OPERATION_CODE.UPDATING AndAlso
                MC0013INProw("OPERATION") <> C_LIST_OPERATION_CODE.ERRORED) OrElse
                MC0013INProw("DELFLG") = C_DELETE_FLG.DELETE OrElse
                MC0013INProw("STYMD") < C_DEFAULT_YMD Then
                Continue For
            End If

            WW_LINEERR_SW = ""

            'チェック
            For Each MC0013row As DataRow In MC0013tbl.Rows

                '日付以外の項目が等しい
                If MC0013INProw("CAMPCODE") = MC0013row("CAMPCODE") AndAlso
                   MC0013INProw("OILTYPE") = MC0013row("OILTYPE") AndAlso
                   MC0013INProw("PRODUCT1") = MC0013row("PRODUCT1") AndAlso
                   MC0013INProw("PRODUCT2") = MC0013row("PRODUCT2") AndAlso
                   MC0013row("DELFLG") <> C_DELETE_FLG.DELETE Then
                Else
                    Continue For
                End If

                '期間変更対象は読み飛ばし
                If MC0013INProw("STYMD") = MC0013row("STYMD") Then
                    Continue For
                End If

                Try
                    Date.TryParse(MC0013INProw("STYMD"), WW_DATE_ST)
                    Date.TryParse(MC0013INProw("ENDYMD"), WW_DATE_END)
                    Date.TryParse(MC0013row("STYMD"), WW_DATE_ST2)
                    Date.TryParse(MC0013row("ENDYMD"), WW_DATE_END2)
                Catch ex As Exception
                End Try

                '開始日チェック
                If (WW_DATE_ST >= WW_DATE_ST2 AndAlso WW_DATE_ST <= WW_DATE_END2) Then
                    WW_CheckMES1 = "・エラー(期間重複)が存在します。"
                    WW_CheckMES2 = ""
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, MC0013row)
                    O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    WW_LINEERR_SW = "ERR"
                    Exit For
                End If

                '終了日チェック
                If (WW_DATE_END >= WW_DATE_ST2 AndAlso WW_DATE_END <= WW_DATE_END2) Then
                    WW_CheckMES1 = "・エラー(期間重複)が存在します。"
                    WW_CheckMES2 = ""
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, MC0013row)
                    O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    WW_LINEERR_SW = "ERR"
                    Exit For
                End If

            Next

            If WW_LINEERR_SW = "" Then
                MC0013INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
            Else
                MC0013INProw("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End If
        Next

    End Sub

    ''' <summary>
    ''' 更新予定データ登録・更新処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub MC0013tbl_UPD()

        '○操作表示クリア
        For Each MC0013row As DataRow In MC0013tbl.Rows
            Select Case MC0013row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    MC0013row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.NODISP
                    MC0013row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED
                    MC0013row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    MC0013row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    MC0013row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End Select
        Next

        '○追加変更判定
        For Each MC0013INProw As DataRow In MC0013INPtbl.Rows

            'エラーレコード読み飛ばし
            If MC0013INProw("OPERATION") <> C_LIST_OPERATION_CODE.UPDATING Then
                Continue For
            End If

            '初期判定セット
            MC0013INProw("OPERATION") = "Insert"

            For Each MC0013row As DataRow In MC0013tbl.Rows

                If MC0013INProw("CAMPCODE") = MC0013row("CAMPCODE") AndAlso
                   MC0013INProw("OILTYPE") = MC0013row("OILTYPE") AndAlso
                   MC0013INProw("PRODUCT1") = MC0013row("PRODUCT1") AndAlso
                   MC0013INProw("PRODUCT2") = MC0013row("PRODUCT2") AndAlso
                   MC0013INProw("STYMD") = MC0013row("STYMD") Then
                Else
                    Continue For
                End If

                'レコード内容に変更があったか判定
                If MC0013row("CAMPCODE") = MC0013INProw("CAMPCODE") AndAlso
                   MC0013row("CAMPNAMES") = MC0013INProw("CAMPNAMES") AndAlso
                   MC0013row("TORICODE") = MC0013INProw("TORICODE") AndAlso
                   MC0013row("TORICODENAMES") = MC0013INProw("TORICODENAMES") AndAlso
                   MC0013row("TODOKECODE") = MC0013INProw("TODOKECODE") AndAlso
                   MC0013row("TODOKECODENAMES") = MC0013INProw("TODOKECODENAMES") AndAlso
                   MC0013row("GSHABAN") = MC0013INProw("GSHABAN") AndAlso
                   MC0013row("GSHABANNAMES") = MC0013INProw("GSHABANNAMES") AndAlso
                   MC0013row("NSHABAN") = MC0013INProw("NSHABAN") AndAlso
                   MC0013row("NSHABANNAMES") = MC0013INProw("NSHABANNAMES") AndAlso
                   MC0013row("STYMD") = MC0013INProw("STYMD") AndAlso
                   MC0013row("ENDYMD") = MC0013INProw("ENDYMD") AndAlso
                   MC0013row("URISHIIREKBN") = MC0013INProw("URISHIIREKBN") AndAlso
                   MC0013row("URISHIIREKBNNAMES") = MC0013INProw("URISHIIREKBNNAMES") AndAlso
                   MC0013row("OPEKBN") = MC0013INProw("OPEKBN") AndAlso
                   MC0013row("OPEKBNNAMES") = MC0013INProw("OPEKBNNAMES") AndAlso
                   MC0013row("INDATAKBN") = MC0013INProw("INDATAKBN") AndAlso
                   MC0013row("INDATAKBNNAMES") = MC0013INProw("INDATAKBNNAMES") AndAlso
                   MC0013row("UNCHINCODE") = MC0013INProw("UNCHINCODE") AndAlso
                   MC0013row("UNCHINCODENAMES") = MC0013INProw("UNCHINCODENAMES") AndAlso
                   MC0013row("UNCHINCALCKBN") = MC0013INProw("UNCHINCALCKBN") AndAlso
                   MC0013row("UNCHINCALCKBNNAMES") = MC0013INProw("UNCHINCALCKBNNAMES") AndAlso
                   MC0013row("COST") = MC0013INProw("COST") AndAlso
                   MC0013row("PATERNKBN") = MC0013INProw("PATERNKBN") AndAlso
                   MC0013row("SHIWAKEPATTERN") = MC0013INProw("SHIWAKEPATTERN") AndAlso
                   MC0013row("SHIWAKEPATTERNNAMES") = MC0013INProw("SHIWAKEPATTERNNAMES") AndAlso
                   MC0013row("SEIKYUKBN") = MC0013INProw("SEIKYUKBN") AndAlso
                   MC0013row("SEIKYUKBNNAMES") = MC0013INProw("SEIKYUKBNNAMES") AndAlso
                   MC0013row("DELFLG") = MC0013INProw("DELFLG") Then

                    MC0013INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Else
                    '○更新（Update）
                    TBL_Update_SUB(MC0013INProw, MC0013row)
                End If

                Exit For

            Next

            '○MD0001追加処理
            If MC0013INProw("OPERATION") = "Insert" Then
                '○更新（Insert）
                TBL_Insert_SUB(MC0013INProw)
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

        '○MD0001変更処理
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

        '○MD0001追加処理
        Dim MC0013row As DataRow = MC0013tbl.NewRow
        MC0013row.ItemArray = INProw.ItemArray

        MC0013row("LINECNT") = MC0013tbl.Rows.Count + 1
        MC0013row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
        MC0013row("TIMSTP") = 0
        MC0013row("SELECT") = 1
        MC0013row("HIDDEN") = 0
        MC0013row("PRODUCTCODE") = String.Format("{0}{1}{2}{3}", MC0013row("CAMPCODE"), MC0013row("OILTYPE"), MC0013row("PRODUCT1"), MC0013row("PRODUCT2"))
        MC0013tbl.Rows.Add(MC0013row)

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
    Protected Sub WW_CheckERR(ByRef I_MESSAGE1 As String, ByRef I_MESSAGE2 As String, ByVal I_ERRCD As String, ByVal MC0013INProw As DataRow)

        Dim WW_ERR_MES As String = ""
        WW_ERR_MES = I_MESSAGE1
        If I_MESSAGE2 <> "" Then
            WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> " & I_MESSAGE2 & " , "
        End If
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 会社コード　　=" & MC0013INProw("CAMPCODE") & " , "
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 油種　　　　　=" & MC0013INProw("OILTYPE") & " , "
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 品名１　　　　=" & MC0013INProw("PRODUCT1") & " , "
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 品名２　　　　=" & MC0013INProw("PRODUCT2") & " , "
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 開始年月日　　=" & MC0013INProw("STYMD") & " , "
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 終了年月日　　=" & MC0013INProw("ENDYMD") & " , "
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 品名名称（短）=" & MC0013INProw("PRODUCTNAMES") & " , "
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 品名名称（長）=" & MC0013INProw("PRODUCTNAMEL") & " , "
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 削除フラグ　　=" & MC0013INProw("DELFLG") & " "
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
    Protected Sub CODENAME_get(ByVal I_FIELD As String, ByRef I_VALUE As String, ByRef O_TEXT As String, ByRef O_RTN As String, Optional ByVal I_OPT_PARAM As String = "")

        '○名称取得

        O_TEXT = ""
        O_RTN = C_MESSAGE_NO.NORMAL

        If I_VALUE <> "" Then
            With leftview
                Select Case I_FIELD
                    Case "CAMPCODE"      '会社
                        .CodeToName(LIST_BOX_CLASSIFICATION.LC_COMPANY, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text))

                    Case "TORICODE"      '取引先コード
                        .CodeToName(LIST_BOX_CLASSIFICATION.LC_OILTYPE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_TORICODE.Text))

                    Case "OILTYPEGRP"    '
                        .CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "OILTYPEGRP"))

                    Case "URIHIYOKBN"    '
                        .CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "URIHIYOKBN"))

                    Case "DELFLG"       '削除フラグ名称
                        .CodeToName(LIST_BOX_CLASSIFICATION.LC_DELFLG, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "DELFLG"))

                    Case "STANI" '単位名称
                        .CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "STANI"))

                    Case Else
                        O_TEXT = ""                                                             '該当項目なし

                End Select
            End With
        End If
    End Sub

End Class
