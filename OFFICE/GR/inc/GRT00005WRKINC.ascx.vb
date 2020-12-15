﻿Imports System.Data.SqlClient
Imports System.IO

Public Class GRT00005WRKINC
    Inherits System.Web.UI.UserControl

    Public Const MAPIDS As String = "T00005S"                        'MAPID(選択)
    Public Const MAPID As String = "T00005"                          'MAPID(実行)
    Public Const MAPIDI As String = "T00005I"                        'MAPID(実行)
    Public Const MAPID7 As String = "T00007"
    Public Const MAPID7I As String = "T00007I"
    ''' <summary>
    ''' ボタン選択情報
    ''' </summary>
    ''' <remarks></remarks>
    Public Enum LC_BTN_TYPE As Integer
        BTN_NOSELECT
        BTN_DO
        BTN_RESTART
        BTN_NEW
    End Enum
    ''' <summary>
    ''' 端末区分
    ''' </summary>
    Public Class TERM_TYPE
        Public Const YAZAKI As String = "1"
        Public Const JX As String = "2"
        Public Const JOT As String = "3"
        Public Const HAND As String = "4"
        Public Const TG As String = "5"
        Public Const COSMO As String = "6"
    End Class

    'コンスタント
    Public Const C_TORICODE_JX As String = "0005700000"                    '取引先コード（JX)
    Public Const C_TORICODE_COSMO As String = "0094000000"                 '取引先コード（COSMO)

    Public Const C_TORICODE_NJS_DUMMY As String = "8888800001"             '取引先コード（NJSダミー)
    Public Const C_SHUKABASHO_NJS_DUMMY As String = "NJS8888"              '出荷場所コード（NJSダミー)
    Public Const C_TODOKECODE_NJS_DUMMY As String = "NJS888888"            '届先コード（NJSダミー)
    Public Const C_PRODUCT2_NJS_DUMMY As String = "88888"                  '品名２コード（NJSダミー)
    Public Const C_PRODUCTCODE_NJS_DUMMY As String = "04033188888"         '品名コード（NJSダミー)

    Public Const C_COMP_ENEX As String = "02"                              '会社コード（ENEX)
    Public Const C_COMP_KNK As String = "03"                               '会社コード（近石)
    Public Const C_COMP_NJS As String = "04"                               '会社コード（NJS)
    Public Const C_COMP_JKT As String = "05"                               '会社コード（JKT)
    Public Const C_OILTYPE01 As String = "01"                              '油種（石油）
    Public Const C_OILTYPE02 As String = "02"                              '油種（高圧）
    Public Const C_OILTYPE03 As String = "03"                              '油種（化成品）
    Public Const C_OILTYPE04 As String = "04"                              '油種（コンテナ）

    Public Const C_KOUEI_CLASS_CODE As String = "T00005_KOUEIORG"   '光英連携可否判定用FIXVAL KEY
    Public Const C_DIR_KOUEI As String = "KOUEI"                    '光英連携ディレクトリ名(LOCAL)
    Public Const C_DIR_KOUEI_RESULT As String = "result"            '光英連携日報ディレクトリ名
    Public Const C_KOUEI_RESULT_FILE_SERCH As String = "*_jotsyasai_*.csv"   '光英連携日報ファイルSearchPattern

    ''' <summary>
    ''' 光英ファイル接頭辞
    ''' </summary>
    Public Class FILE_SUFFIX
        Public Const JX As String = "jx"
        Public Const JOT As String = "jot"
        Public Const TG As String = "tg"
        Public Const COSMO As String = "cosmo"
        Public Shared Function Suffix2TermType(ByVal suffix As String) As String
            Select Case suffix
                Case JX : Return TERM_TYPE.JX
                Case JOT : Return TERM_TYPE.JOT
                Case TG : Return TERM_TYPE.TG
                Case COSMO : Return TERM_TYPE.COSMO
                Case Else : Return String.Empty
            End Select
        End Function
    End Class
    ''' <summary>
    ''' FTPターゲット(S0028_FTPFILES設定値)
    ''' </summary>
    Private ReadOnly FTP_LIST As String() = {
        "日報データ受信JX",
        "日報データ受信TG",
        "日報データ受信JOT",
        "日報データ受信COSMO"
    }

    ''' <summary>
    ''' セッション管理クラス
    ''' </summary>
    Private sm As New CS0050SESSION
    ''' <summary>
    ''' 一覧管理情報
    ''' </summary>
    Private lmap As New Hashtable
    ''' <summary>
    ''' ワークデータ初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub Initialize()
        '選択情報のWF_SELクリア

    End Sub

    ''' <summary>
    ''' 前画面からデータを再取得する
    ''' </summary>
    ''' <param name="W_PrePage"></param>
    ''' <remarks></remarks>
    Public Sub Copy(ByVal W_PrePage As UserControl)

        '会社コード　
        WF_SEL_CAMPCODE.Text = DirectCast(W_PrePage.FindControl("WF_SEL_CAMPCODE"), TextBox).Text
        '出庫日　
        WF_SEL_STYMD.Text = DirectCast(W_PrePage.FindControl("WF_SEL_STYMD"), TextBox).Text
        WF_SEL_ENDYMD.Text = DirectCast(W_PrePage.FindControl("WF_SEL_ENDYMD"), TextBox).Text
        '運用部署
        WF_SEL_UORG.Text = DirectCast(W_PrePage.FindControl("WF_SEL_UORG"), TextBox).Text
        '従業員コード
        WF_SEL_STAFFCODE.Text = DirectCast(W_PrePage.FindControl("WF_SEL_STAFFCODE"), TextBox).Text
        '従業員名称
        WF_SEL_STAFFNAME.Text = DirectCast(W_PrePage.FindControl("WF_SEL_STAFFNAME"), TextBox).Text
        '画面ID
        WF_SEL_VIEWID.Text = DirectCast(W_PrePage.FindControl("WF_SEL_VIEWID"), TextBox).Text
        WF_SEL_VIEWID_DTL.Text = DirectCast(W_PrePage.FindControl("WF_SEL_VIEWID_DTL"), TextBox).Text
        '権限、変数
        WF_SEL_MAPvariant.Text = DirectCast(W_PrePage.FindControl("WF_SEL_MAPvariant"), TextBox).Text
        WF_SEL_MAPpermitcode.Text = DirectCast(W_PrePage.FindControl("WF_SEL_MAPpermitcode"), TextBox).Text
        '一時保存パス
        WF_T5_XMLsaveTmp.Text = DirectCast(W_PrePage.FindControl("WF_T5_XMLsaveTmp"), TextBox).Text
        WF_T5_XMLsaveTmp9.Text = DirectCast(W_PrePage.FindControl("WF_T5_XMLsaveTmp9"), TextBox).Text
        WF_SEL_XMLsavePARM.Text = DirectCast(W_PrePage.FindControl("WF_SEL_XMLsavePARM"), TextBox).Text

    End Sub
    ''' <summary>
    ''' テンポラリファイルの削除
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub DeleteTmpFiles()
        If Not String.IsNullOrEmpty(WF_T5_XMLsaveTmp.Text) Then
            System.IO.File.Delete(WF_T5_XMLsaveTmp.Text)
        End If
        If Not String.IsNullOrEmpty(WF_T5_XMLsaveTmp9.Text) Then
            System.IO.File.Delete(WF_T5_XMLsaveTmp9.Text)
        End If
        If Not String.IsNullOrEmpty(WF_SEL_XMLsavePARM.Text) Then
            System.IO.File.Delete(WF_SEL_XMLsavePARM.Text)
        End If
    End Sub
    ''' <summary>
    ''' パラメータ設定用テーブルの作成
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function CreateParamTable() As DataTable
        Dim I_TBL As New DataTable
        'T0005DB項目作成
        I_TBL.Columns.Add("LINECNT", GetType(Integer))
        I_TBL.Columns.Add("OPERATION", GetType(String))
        I_TBL.Columns.Add("TIMSTP", GetType(Long))
        I_TBL.Columns.Add("SELECT", GetType(Integer))
        I_TBL.Columns.Add("HIDDEN", GetType(Integer))

        I_TBL.Columns.Add("CAMPCODE", GetType(String))
        I_TBL.Columns.Add("STYMD", GetType(Date))
        I_TBL.Columns.Add("ENDYMD", GetType(Date))
        I_TBL.Columns.Add("UORG", GetType(String))
        I_TBL.Columns.Add("STAFFCODE", GetType(String))
        I_TBL.Columns.Add("STAFFNAME", GetType(String))
        I_TBL.Columns.Add("IMPYM", GetType(String))
        Return I_TBL
    End Function
    ''' <summary>
    ''' 固定値マスタから一覧の取得
    ''' </summary>
    ''' <param name="COMPCODE">会社コード</param>
    ''' <param name="FIXNUM">連携番号</param>
    ''' <returns>検索条件一覧</returns>
    ''' <remarks></remarks>
    Function CreateFIXParam(ByVal COMPCODE As String, ByVal FIXNUM As Integer) As Hashtable
        Dim FIXCODE As String = ""
        Select Case FIXNUM
            Case 901 : FIXCODE = "WORKKBN"      ' 作業区分
            Case 902 : FIXCODE = "CREWKBN"      ' 乗務区分
            Case 903 : FIXCODE = "TUMIOKIKBN"   ' 積置区分
            Case 904 : FIXCODE = "URIKBN"       ' 売上計上基準
            Case 905 : FIXCODE = "TAXKBN"       ' 税区分
        End Select
        Return CreateFIXParam(COMPCODE, FIXCODE)
    End Function
    ''' <summary>
    ''' 固定値マスタから一覧の取得
    ''' </summary>
    ''' <param name="COMPCODE">会社コード</param>
    ''' <returns>検索条件一覧</returns>
    ''' <remarks></remarks>
    Function CreateFIXParam(ByVal COMPCODE As String) As Hashtable

        Return CreateFIXParam(COMPCODE, "")
    End Function
    ''' <summary>
    ''' 固定値マスタから一覧の取得
    ''' </summary>
    ''' <param name="COMPCODE">会社コード</param>
    ''' <param name="FIXCODE">固定値区分</param>
    ''' <returns>検索条件一覧</returns>
    ''' <remarks></remarks>
    Function CreateFIXParam(ByVal COMPCODE As String, ByVal FIXCODE As String) As Hashtable
        Dim prmData As New Hashtable
        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_COMPANY) = COMPCODE
        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_FIX_CLASS) = FIXCODE
        Return prmData
    End Function

    ''' <summary>
    ''' 品名一覧取得用パラメータ設定
    ''' </summary>
    ''' <param name="COMPCODE">会社コード</param>
    ''' <param name="ORGCODE" >部署コード</param>
    ''' <returns>検索条件一覧</returns>
    ''' <remarks></remarks>
    Function CreateGoodsParam(ByVal COMPCODE As String, ByVal ORGCODE As String) As Hashtable
        Return CreateGoodsParam(COMPCODE, ORGCODE, True)
    End Function

    ''' <summary>
    ''' 品名一覧取得用パラメータ設定
    ''' </summary>
    ''' <param name="COMPCODE">会社コード</param>
    ''' <param name="ORGCODE" >部署コード</param>
    ''' <returns>検索条件一覧</returns>
    ''' <remarks></remarks>
    Function CreateGoodsParam(ByVal COMPCODE As String, ByVal ORGCODE As String, ByVal isMaster As Boolean) As Hashtable
        Dim prmData As New Hashtable
        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_COMPANY) = COMPCODE
        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_ORG_COMP) = COMPCODE
        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_ORG) = ORGCODE
        If isMaster Then
            prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_TYPEMODE) = GL0006GoodsList.LC_GOODS_TYPE.GOODS_MST
        Else
            prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_TYPEMODE) = GL0006GoodsList.LC_GOODS_TYPE.GOODS
        End If

        Return prmData
    End Function

    ''' <summary>
    ''' 品名1一覧取得用パラメータ設定
    ''' </summary>
    ''' <param name="COMPCODE">会社コード</param>
    ''' <param name="OILTYPE" >油種</param>
    ''' <returns>検索条件一覧</returns>
    ''' <remarks></remarks>
    Function CreateGoods1Param(ByVal COMPCODE As String, ByVal OILTYPE As String) As Hashtable
        Dim prmData As New Hashtable
        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_COMPANY) = COMPCODE
        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_OILTYPE) = OILTYPE

        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_TYPEMODE) = GL0006GoodsList.LC_GOODS_TYPE.GOODS1_MST

        Return prmData
    End Function
    ''' <summary>
    ''' 荷主一覧取得用パラメータ設定
    ''' </summary>
    ''' <param name="COMPCODE">会社コード</param>
    ''' <returns>検索条件一覧</returns>
    ''' <remarks></remarks>
    Function CreateCustomerParam(COMPCODE As String) As Hashtable
        Dim prmData As New Hashtable
        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_COMPANY) = COMPCODE
        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_TYPEMODE) = GL0003CustomerList.LC_CUSTOMER_TYPE.OWNER
        Return prmData
    End Function
    ''' <summary>
    ''' 請求一覧取得用パラメータ設定
    ''' </summary>
    ''' <param name="COMPCODE">会社コード</param>
    ''' <param name="ORGCODE" >部署コード</param>
    ''' <returns>検索条件一覧</returns>
    ''' <remarks></remarks>
    Function CreateDemandParam(ByVal COMPCODE As String, ByVal ORGCODE As String) As Hashtable
        Dim prmData As New Hashtable
        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_COMPANY) = COMPCODE
        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_ORG) = ORGCODE
        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_TYPEMODE) = GL0003CustomerList.LC_CUSTOMER_TYPE.ALL
        Return prmData
    End Function
    ''' <summary>
    ''' 社員一覧取得用パラメータ設定
    ''' </summary>
    ''' <param name="COMPCODE">会社コード</param>
    ''' <param name="ORGCODE" >部署コード</param>
    ''' <returns>検索条件一覧</returns>
    ''' <remarks></remarks>
    Function CreateSTAFFParam(ByVal COMPCODE As String, ByVal ORGCODE As String, Optional ByVal STYMD As String = "", Optional ByVal ENDYMD As String = "") As Hashtable
        Dim prmData As New Hashtable
        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_COMPANY) = COMPCODE
        If Not String.IsNullOrEmpty(ORGCODE) Then
            prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_ORG) = ORGCODE
        End If
        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_TYPEMODE) = GL0005StaffList.LC_STAFF_TYPE.DRIVER
        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_STYMD) = STYMD
        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_ENDYMD) = ENDYMD
        Return prmData
    End Function

    ''' <summary>
    ''' 部署一覧取得用パラメータ設定
    ''' </summary>
    ''' <param name="COMPCODE">会社コード</param>
    ''' <param name="PRMIT">権限区分</param>
    ''' <returns>検索条件一覧</returns>
    ''' <remarks></remarks>
    Function CreateORGParam(ByVal COMPCODE As String, ByVal PRMIT As String) As Hashtable
        Dim prmData As New Hashtable
        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_COMPANY) = COMPCODE
        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_ORG_CATEGORYS) = New String() {GL0002OrgList.C_CATEGORY_LIST.CARAGE}
        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_PERMISSION) = PRMIT
        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_TYPEMODE) = GL0002OrgList.LS_AUTHORITY_WITH.USER
        Return prmData
    End Function
    ''' <summary>
    ''' 出荷部署一覧取得用パラメータ設定
    ''' </summary>
    ''' <param name="COMPCODE">会社コード</param>
    ''' <param name="PRMIT">権限区分</param>
    ''' <returns>検索条件一覧</returns>
    ''' <remarks></remarks>
    Function CreateShipORGParam(COMPCODE As String, PRMIT As String) As Hashtable
        Dim prmData As New Hashtable
        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_COMPANY) = COMPCODE
        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_ORG_CATEGORYS) = New String() {GL0002OrgList.C_CATEGORY_LIST.CARAGE}
        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_PERMISSION) = PRMIT
        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_TYPEMODE) = GL0002OrgList.LS_AUTHORITY_WITH.USER
        Return prmData
    End Function

    ''' <summary>
    ''' 届先一覧取得用パラメータ設定
    ''' </summary>
    ''' <param name="COMPCODE">会社コード</param>
    ''' <param name="ORGCODE">部署コード</param>
    ''' <param name="SHIPCODE">取引先コード</param>
    ''' <param name="CLASSCODE">区分コード</param>
    ''' <returns>検索条件一覧</returns>
    ''' <remarks></remarks>
    Function CreateDistinationParam(ByVal COMPCODE As String, ByVal ORGCODE As String, ByVal SHIPCODE As String, ByVal CLASSCODE As String) As Hashtable
        Dim prmData As New Hashtable
        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_COMPANY) = COMPCODE
        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_ORG) = ORGCODE

        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_CUSTOMER) = SHIPCODE
        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_CLASSCODE) = CLASSCODE

        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_TYPEMODE) = GL0004DestinationList.LC_DEST_TYPE.ALL
        Return prmData
    End Function
    ''' <summary>
    ''' 統一車番一覧取得用パラメータ設定
    ''' </summary>
    ''' <param name="COMPCODE">会社コード</param>
    ''' <param name="ORGCODE">部署コード</param>
    ''' <returns>検索条件一覧</returns>
    ''' <remarks></remarks>
    Function CreateWorkLorryParam(ByVal COMPCODE As String, ByVal ORGCODE As String) As Hashtable
        Dim prmData As New Hashtable
        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_COMPANY) = COMPCODE
        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_ORG) = ORGCODE
        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_DISPLAY_FORMAT) = GL0008WorkLorryList.C_VIEW_FORMAT_PATTERN.BOTH

        Return prmData
    End Function
    ''' <summary>
    ''' 車番一覧取得用パラメータ設定
    ''' </summary>
    ''' <param name="COMPCODE">会社コード</param>
    ''' <param name="ISFRONT">前方車両フラグ</param>
    ''' <returns>検索条件一覧</returns>
    ''' <remarks></remarks>
    Function CreateCarCodeParam(ByVal COMPCODE As String, ByVal ISFRONT As Boolean) As Hashtable
        Dim prmData As New Hashtable
        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_COMPANY) = COMPCODE
        If ISFRONT Then
            prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_TYPEMODE) = GL0007CarList.LC_LORRY_TYPE.FRONT
        Else
            prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_TYPEMODE) = GL0007CarList.LC_LORRY_TYPE.REAR
        End If

        Return prmData
    End Function

    ''' <summary>
    ''' 水素車一覧用パラメータ設定
    ''' </summary>
    ''' <param name="COMPCODE">会社コード</param>
    ''' <param name="ORGCODE">部署コード</param>
    ''' <param name="O_RTN">可否判定</param>
    ''' <returns>検索条件一覧</returns>
    ''' <remarks></remarks>
    Function CreateHydrogenParam(ByVal COMPCODE As String, ByVal ORGCODE As String, ByRef O_RTN As String) As Hashtable
        '○水素車ListBox設定             
        If lmap.Contains(COMPCODE & ORGCODE & "HydrogenList") Then
            Return lmap.Item(COMPCODE & ORGCODE & "HydrogenList")
        End If
        Using Com As SqlConnection = sm.getConnection
            Dim prmData As New Hashtable
            Try
                Com.Open()
                '検索SQL文（水素車抽出）
                Dim SQLStr As String =
                     "SELECT isnull(rtrim(A.GSHABAN),'') as GSHABAN " _
                   & " FROM  MA006_SHABANORG A " _
                   & " Where A.CAMPCODE   = @P1 " _
                   & "   and A.MANGUORG   = @P2 " _
                   & "   and A.SUISOKBN   = '2' " _
                   & "   and A.DELFLG    <> '1' " _
                   & "ORDER BY A.CAMPCODE , A.GSHABAN "

                Using SQLcmd As New SqlCommand(SQLStr, Com)
                    Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.NVarChar, 20)
                    Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.NVarChar, 20)
                    Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.Date)
                    PARA1.Value = COMPCODE
                    PARA2.Value = ORGCODE
                    PARA3.Value = Date.Now
                    Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

                        Dim ls As New ListBox
                        While SQLdr.Read
                            ls.Items.Add(New ListItem(SQLdr("GSHABAN"), SQLdr("GSHABAN")))
                        End While
                        prmData.Add(GRIS0005LeftBox.C_PARAMETERS.LP_LIST, ls)
                        lmap.Add(COMPCODE & ORGCODE & "HydrogenList", prmData)
                    End Using

                End Using

                Return prmData
            Catch ex As Exception
                O_RTN = C_MESSAGE_NO.DB_ERROR
                Return Nothing
            End Try
        End Using
    End Function
    ''' <summary>
    ''' 統一車番一覧系の取得
    ''' </summary>
    ''' <param name="COMPCODE">会社コード</param>
    ''' <param name="ORGCODE">部署コード</param>
    ''' <param name="O_RTN">可否判定</param>
    ''' <returns>統一車番一覧</returns>
    ''' <remarks></remarks>
    Public Function CreateSHABANLists(ByVal COMPCODE As String, ByVal ORGCODE As String, ByRef O_RTN As String) As ListBox
        If lmap.Contains(COMPCODE & ORGCODE & "SHABAN") Then
            Return lmap.Item(COMPCODE & ORGCODE & "SHABAN")
        End If
        Using Com As SqlConnection = sm.getConnection
            Try
                '○　業務車番ListBox設定()
                Com.Open()

                Dim SQLStr As String =
                     "SELECT isnull(rtrim(A.GSHABAN),'')  as GSHABAN " _
                       & "  ,isnull(rtrim(A.YAZKSHABAN),'') as YAZKSHABAN " _
                       & "  ,isnull(rtrim(A.KOEISHABAN),'') as KOEISHABAN " _
                       & "  ,isnull(rtrim(F.SHABAN),'') as KOEISHABAN_JXENEX " _
                       & "  ,isnull(rtrim(A.SHARYOTYPEF),'') as SHARYOTYPEF " _
                       & "  ,isnull(rtrim(A.TSHABANF),'') as TSHABANF " _
                       & "  ,isnull(rtrim(A.SHARYOTYPEB),'') as SHARYOTYPEB " _
                       & "  ,isnull(rtrim(A.TSHABANB),'') as TSHABANB " _
                       & "  ,isnull(rtrim(A.SHARYOTYPEB2),'') as SHARYOTYPEB2 " _
                       & "  ,isnull(rtrim(A.TSHABANB2),'') as TSHABANB2 " _
                       & "  ,isnull(rtrim(B.MANGSHAFUKU),'') as MANGSHAFUKU " _
                       & "  ,isnull(rtrim(B.MANGOILTYPE),'') as MANGOILTYPE " _
                       & "  ,isnull(rtrim(C.HPRSINSNYMD),'') as HPRSINSNYMDF " _
                       & "  ,isnull(rtrim(C.LICNYMD),'') as LICNYMDF " _
                       & "  ,isnull(rtrim(D.HPRSINSNYMD),'') as HPRSINSNYMDB " _
                       & "  ,isnull(rtrim(D.LICNYMD),'') as LICNYMDB " _
                       & "  ,isnull(rtrim(E.HPRSINSNYMD),'') as HPRSINSNYMDB2 " _
                       & "  ,isnull(rtrim(E.LICNYMD),'')                                     as LICNYMDB2 " _
                       & "  ,isnull(rtrim(C.LICNPLTNO1),'') + isnull(rtrim(C.LICNPLTNO2),'') as FRONT     " _
                       & "  ,isnull(rtrim(D.LICNPLTNO1),'') + isnull(rtrim(D.LICNPLTNO2),'') as BACK      " _
                       & "  ,isnull(rtrim(E.LICNPLTNO1),'') + isnull(rtrim(E.LICNPLTNO2),'') as BACK2     " _
                       & " FROM       MA006_SHABANORG    A " _
                       & "  LEFT JOIN MA002_SHARYOA      B " _
                       & "   ON    B.CAMPCODE    = A.CAMPCODE " _
                       & "   and   B.SHARYOTYPE  = A.SHARYOTYPEF " _
                       & "   and   B.TSHABAN     = A.TSHABANF " _
                       & "   and   B.STYMD      <= @P3 " _
                       & "   and   B.ENDYMD     >= @P3 " _
                       & "   and   B.DELFLG     <> '1' " _
                       & " LEFT JOIN MA004_SHARYOC C " _
                       & "    ON   C.CAMPCODE   = A.CAMPCODE " _
                       & "   and   C.SHARYOTYPE = A.SHARYOTYPEF " _
                       & "   and   C.TSHABAN    = A.TSHABANF " _
                       & "   and   C.STYMD     <= @P3 " _
                       & "   and   C.ENDYMD    >= @P3 " _
                       & "   and   C.DELFLG    <> '1' " _
                       & " LEFT JOIN MA004_SHARYOC D " _
                       & "    ON   D.CAMPCODE   = A.CAMPCODE " _
                       & "   and   D.SHARYOTYPE = A.SHARYOTYPEB " _
                       & "   and   D.TSHABAN    = A.TSHABANB " _
                       & "   and   D.STYMD     <= @P3 " _
                       & "   and   D.ENDYMD    >= @P3 " _
                       & "   and   D.DELFLG    <> '1' " _
                       & " LEFT JOIN MA004_SHARYOC E " _
                       & "    ON   E.CAMPCODE   = A.CAMPCODE " _
                       & "   and   E.SHARYOTYPE = A.SHARYOTYPEB2 " _
                       & "   and   E.TSHABAN    = A.TSHABANB2 " _
                       & "   and   E.STYMD     <= @P3 " _
                       & "   and   E.ENDYMD    >= @P3 " _
                       & "   and   E.DELFLG    <> '1' " _
                       & " LEFT JOIN W0003_KOUEISHARYO F " _
                       & "    ON   F.ORGCODE        = @P2 " _
                       & "   and   F.KOUEITYPE      = 'jxtg' " _
                       & "   and   F.REGISTERSHABAN = A.KOEISHABAN " _
                       & "   and   rtrim(F.REGISTERSHABAN) <> '' " _
                       & "   and   F.DELFLG    <> '1' " _
                       & " Where   A.CAMPCODE   = @P1 " _
                       & "   and   A.MANGUORG   = @P2 " _
                       & "   and   isnull(A.SUISOKBN,'0')  <> '2' " _
                       & "   and   A.DELFLG    <> '1' "

                Using SQLcmd As New SqlCommand(SQLStr, Com)
                    Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.NVarChar, 20)
                    Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.NVarChar, 20)
                    Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.Date)
                    PARA1.Value = COMPCODE
                    PARA2.Value = ORGCODE
                    PARA3.Value = Date.Now

                    Dim WF_ListBoxGSHABAN As New ListBox
                    Dim lstSBY2G As New ListBox
                    Dim lstSBK2G As New ListBox
                    Dim lstSBK2G2 As New ListBox
                    Dim lstSBG2FU As New ListBox
                    Dim lstSBG2B1U As New ListBox
                    Dim lstSBG2B2U As New ListBox

                    Dim lstTSHABAN As New ListBox
                    Dim lstKSHABAN As New ListBox
                    Dim lstYSHABAN As New ListBox
                    Dim lstOSHABAN As New ListBox               '車番に紐づく油種
                    Dim lstFSHABAN As New ListBox               '車番に紐づく車腹

                    Dim tblSSHABAN As New Hashtable
                    Dim WW_CODE As String = ""
                    Dim WW_NAME As String = ""

                    Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

                        While SQLdr.Read
                            '○出力編集
                            '         　CODE　 矢崎車番,光英車番,業務車番
                            '           VALUE　統一車番1 統一車番2 統一車番3 統一車番4 統一車番5 統一車番6 車腹 油種

                            WW_CODE = SQLdr("YAZKSHABAN") & "," & SQLdr("KOEISHABAN") & "," & SQLdr("KOEISHABAN_JXENEX") & "," & SQLdr("GSHABAN")
                            WW_NAME = SQLdr("SHARYOTYPEF") & " " & SQLdr("TSHABANF") & " " & SQLdr("SHARYOTYPEB") & " " & SQLdr("TSHABANB") & " " & SQLdr("SHARYOTYPEB2") & " " & SQLdr("TSHABANB2") & " " & SQLdr("MANGSHAFUKU") & " " & SQLdr("MANGOILTYPE")
                            WF_ListBoxGSHABAN.Items.Add(New ListItem(WW_NAME, WW_CODE))
                            lstSBY2G.Items.Add(New ListItem(SQLdr("GSHABAN"), SQLdr("YAZKSHABAN")))
                            lstSBK2G.Items.Add(New ListItem(SQLdr("GSHABAN"), SQLdr("KOEISHABAN")))
                            lstSBK2G2.Items.Add(New ListItem(SQLdr("GSHABAN"), SQLdr("KOEISHABAN_JXENEX")))
                            lstSBG2FU.Items.Add(New ListItem(SQLdr("SHARYOTYPEF") & SQLdr("TSHABANF"), SQLdr("GSHABAN")))
                            lstSBG2B1U.Items.Add(New ListItem(SQLdr("SHARYOTYPEB") & SQLdr("TSHABANB"), SQLdr("GSHABAN")))
                            lstSBG2B2U.Items.Add(New ListItem(SQLdr("SHARYOTYPEB2") & SQLdr("TSHABANB2"), SQLdr("GSHABAN")))
                            Dim subList As New ListBox
                            tblSSHABAN(SQLdr("GSHABAN")) = subList
                            subList.Items.Add(New ListItem("HPRSINSNYMDF", SQLdr("HPRSINSNYMDF")))
                            subList.Items.Add(New ListItem("HPRSINSNYMDB", SQLdr("HPRSINSNYMDB")))
                            subList.Items.Add(New ListItem("HPRSINSNYMDB2", SQLdr("HPRSINSNYMDB2")))
                            subList.Items.Add(New ListItem("LICNYMDF", SQLdr("LICNYMDF")))
                            subList.Items.Add(New ListItem("LICNYMDB", SQLdr("LICNYMDB")))
                            subList.Items.Add(New ListItem("LICNYMDB2", SQLdr("LICNYMDB2")))
                            subList.Items.Add(New ListItem("FRONT", SQLdr("FRONT")))
                            subList.Items.Add(New ListItem("BACK", SQLdr("BACK")))
                            subList.Items.Add(New ListItem("BACK2", SQLdr("BACK2")))

                            lstOSHABAN.Items.Add(New ListItem(SQLdr("MANGOILTYPE"), SQLdr("GSHABAN")))
                            lstFSHABAN.Items.Add(New ListItem(SQLdr("MANGSHAFUKU"), SQLdr("GSHABAN")))
                            lstTSHABAN.Items.Add(New ListItem(WW_NAME, SQLdr("GSHABAN")))
                            lstKSHABAN.Items.Add(New ListItem(WW_NAME, SQLdr("KOEISHABAN")))
                            lstYSHABAN.Items.Add(New ListItem(WW_NAME, SQLdr("YAZKSHABAN")))
                        End While
                        lmap.Add(COMPCODE & ORGCODE & "SBCY2G", lstSBY2G)
                        lmap.Add(COMPCODE & ORGCODE & "SBCK2G", lstSBK2G)
                        lmap.Add(COMPCODE & ORGCODE & "SBCK2G2", lstSBK2G2)
                        lmap.Add(COMPCODE & ORGCODE & "SBCG2FU", lstSBG2FU)
                        lmap.Add(COMPCODE & ORGCODE & "SBCG2B1U", lstSBG2B1U)
                        lmap.Add(COMPCODE & ORGCODE & "SBCG2B2U", lstSBG2B2U)

                        lmap.Add(COMPCODE & ORGCODE & "SHABAN", WF_ListBoxGSHABAN)
                        lmap.Add(COMPCODE & ORGCODE & "SSHABAN", tblSSHABAN)

                        lmap.Add(COMPCODE & ORGCODE & "TSHABAN", lstTSHABAN)
                        lmap.Add(COMPCODE & ORGCODE & "KSHABAN", lstKSHABAN)
                        lmap.Add(COMPCODE & ORGCODE & "YSHABAN", lstYSHABAN)
                        lmap.Add(COMPCODE & ORGCODE & "OSHABAN", lstOSHABAN)
                        lmap.Add(COMPCODE & ORGCODE & "FSHABAN", lstFSHABAN)
                    End Using
                    Return WF_ListBoxGSHABAN
                End Using
            Catch ex As Exception
                O_RTN = "ERR"
                Return Nothing
            End Try
        End Using
    End Function
    ''' <summary>
    ''' 矢崎車番から業務車番への変換
    ''' </summary>
    ''' <param name="COMPCODE">会社コード</param>
    ''' <param name="ORGCODE">組織コード</param>
    ''' <returns>矢崎車番変換テーブル</returns>
    ''' <remarks></remarks>
    Public Function CreateSHABANY2G(ByVal COMPCODE As String, ByVal ORGCODE As String) As ListBox
        If lmap.Contains(COMPCODE & ORGCODE & "SBCY2G") Then
            Return lmap.Item(COMPCODE & ORGCODE & "SBCY2G")
        Else
            Dim WW_RTN As String = C_MESSAGE_NO.NORMAL
            CreateSHABANLists(COMPCODE, ORGCODE, WW_RTN)
            If isNormal(WW_RTN) Then
                Return lmap.Item(COMPCODE & ORGCODE & "SBCY2G")
            Else
                Return Nothing
            End If
        End If
    End Function
    ''' <summary>
    ''' 光栄車番から業務車番への変換
    ''' </summary>
    ''' <param name="COMPCODE">会社コード</param>
    ''' <param name="ORGCODE">組織コード</param>
    ''' <returns>光栄車番変換テーブル</returns>
    ''' <remarks></remarks>
    Public Function CreateSHABANK2G(ByVal COMPCODE As String, ByVal ORGCODE As String) As ListBox
        If lmap.Contains(COMPCODE & ORGCODE & "SBCK2G") Then
            Return lmap.Item(COMPCODE & ORGCODE & "SBCK2G")
        Else
            Dim WW_RTN As String = C_MESSAGE_NO.NORMAL
            CreateSHABANLists(COMPCODE, ORGCODE, WW_RTN)
            If isNormal(WW_RTN) Then
                Return lmap.Item(COMPCODE & ORGCODE & "SBCK2G")
            Else
                Return Nothing
            End If
        End If
    End Function
    ''' <summary>
    ''' 光栄車番から業務車番への変換
    ''' </summary>
    ''' <param name="COMPCODE">会社コード</param>
    ''' <param name="ORGCODE">組織コード</param>
    ''' <returns>光栄車番変換テーブル</returns>
    ''' <remarks></remarks>
    Public Function CreateSHABANK2G2(ByVal COMPCODE As String, ByVal ORGCODE As String) As ListBox
        If lmap.Contains(COMPCODE & ORGCODE & "SBCK2G2") Then
            Return lmap.Item(COMPCODE & ORGCODE & "SBCK2G2")
        Else
            Dim WW_RTN As String = C_MESSAGE_NO.NORMAL
            CreateSHABANLists(COMPCODE, ORGCODE, WW_RTN)
            If isNormal(WW_RTN) Then
                Return lmap.Item(COMPCODE & ORGCODE & "SBCK2G2")
            Else
                Return Nothing
            End If
        End If
    End Function
    ''' <summary>
    ''' 統一車番付属情報一覧取得
    ''' </summary>
    ''' <param name="COMPCODE">会社コード</param>
    ''' <param name="ORGCODE">組織コード</param>
    ''' <returns>付属情報一覧</returns>
    ''' <remarks></remarks>
    Public Function GetShabanSubTable(ByVal COMPCODE As String, ByVal ORGCODE As String) As Hashtable
        If lmap.Contains(COMPCODE & ORGCODE & "SSHABAN") Then
            Return lmap.Item(COMPCODE & ORGCODE & "SSHABAN")
        Else
            Dim WW_RTN As String = C_MESSAGE_NO.NORMAL
            CreateSHABANLists(COMPCODE, ORGCODE, WW_RTN)
            If isNormal(WW_RTN) Then
                Return lmap.Item(COMPCODE & ORGCODE & "SSHABAN")
            Else
                Return Nothing
            End If
        End If
    End Function
    ''' <summary>
    ''' 矢崎車番情報の一覧取得
    ''' </summary>
    ''' <param name="COMPCODE">会社コード</param>
    ''' <param name="ORGCODE">組織コード</param>
    ''' <param name="O_RTN">可否判定</param>
    ''' <returns>車番情報一覧</returns>
    ''' <remarks></remarks>
    Public Function CreateYSHABANList(ByVal COMPCODE As String, ByVal ORGCODE As String, ByRef O_RTN As String) As ListBox
        If lmap.Contains(COMPCODE & ORGCODE & "YSHABAN") Then
            Return lmap.Item(COMPCODE & ORGCODE & "YSHABAN")
        Else
            CreateSHABANLists(COMPCODE, ORGCODE, O_RTN)
            If isNormal(O_RTN) Then
                Return lmap.Item(COMPCODE & ORGCODE & "YSHABAN")
            End If
        End If
        Return Nothing
    End Function
    ''' <summary>
    ''' 光栄車番情報の一覧取得
    ''' </summary>
    ''' <param name="COMPCODE">会社コード</param>
    ''' <param name="ORGCODE">組織コード</param>
    ''' <param name="O_RTN">可否判定</param>
    ''' <returns>車番情報一覧</returns>
    ''' <remarks></remarks>
    Public Function CreateKSHABANList(ByVal COMPCODE As String, ByVal ORGCODE As String, ByRef O_RTN As String) As ListBox
        If lmap.Contains(COMPCODE & ORGCODE & "KSHABAN") Then
            Return lmap.Item(COMPCODE & ORGCODE & "KSHABAN")
        Else
            CreateSHABANLists(COMPCODE, ORGCODE, O_RTN)
            If isNormal(O_RTN) Then
                Return lmap.Item(COMPCODE & ORGCODE & "KSHABAN")
            End If
        End If
        Return Nothing
    End Function
    ''' <summary>
    ''' 業務車番と統一車番の一覧取得
    ''' </summary>
    ''' <param name="COMPCODE">会社コード</param>
    ''' <param name="ORGCODE">組織コード</param>
    ''' <param name="O_RTN">可否判定</param>
    ''' <returns>車番情報一覧</returns>
    ''' <remarks></remarks>
    Public Function CreateTSHABANList(ByVal COMPCODE As String, ByVal ORGCODE As String, ByRef O_RTN As String) As ListBox
        If lmap.Contains(COMPCODE & ORGCODE & "TSHABAN") Then
            Return lmap.Item(COMPCODE & ORGCODE & "TSHABAN")
        Else
            CreateSHABANLists(COMPCODE, ORGCODE, O_RTN)
            If isNormal(O_RTN) Then
                Return lmap.Item(COMPCODE & ORGCODE & "TSHABAN")
            End If
        End If
        Return Nothing
    End Function
    ''' <summary>
    ''' 油種と統一車番の一覧取得
    ''' </summary>
    ''' <param name="COMPCODE">会社コード</param>
    ''' <param name="ORGCODE">組織コード</param>
    ''' <param name="O_RTN">可否判定</param>
    ''' <returns>車番情報一覧</returns>
    ''' <remarks></remarks>
    Public Function CreateSHABAN2OILList(ByVal COMPCODE As String, ByVal ORGCODE As String, ByRef O_RTN As String) As ListBox
        If lmap.Contains(COMPCODE & ORGCODE & "OSHABAN") Then
            Return lmap.Item(COMPCODE & ORGCODE & "OSHABAN")
        Else
            CreateSHABANLists(COMPCODE, ORGCODE, O_RTN)
            If isNormal(O_RTN) Then
                Return lmap.Item(COMPCODE & ORGCODE & "OSHABAN")
            End If
        End If
        Return Nothing
    End Function
    ''' <summary>
    ''' 車腹と統一車番の取得
    ''' </summary>
    ''' <param name="COMPCODE">会社コード</param>
    ''' <param name="ORGCODE">組織コード</param>
    ''' <param name="O_RTN">可否判定</param>
    ''' <returns>車番情報一覧</returns>
    ''' <remarks></remarks>
    Public Function CreateSHABAN2FUKUList(ByVal COMPCODE As String, ByVal ORGCODE As String, ByRef O_RTN As String) As ListBox
        If lmap.Contains(COMPCODE & ORGCODE & "FSHABAN") Then
            Return lmap.Item(COMPCODE & ORGCODE & "FSHABAN")
        Else
            CreateSHABANLists(COMPCODE, ORGCODE, O_RTN)
            If isNormal(O_RTN) Then
                Return lmap.Item(COMPCODE & ORGCODE & "FSHABAN")
            End If
        End If
        Return Nothing
    End Function
    ''' <summary>
    ''' 品名の一覧群取得
    ''' </summary>
    ''' <param name="COMPCODE">会社コード</param>
    ''' <param name="ORGCODE">組織コード</param>
    ''' <param name="STYMD">開始日</param>
    ''' <param name="ENDYMD">終了日</param>
    ''' <param name="O_RTN">可否判定</param>
    ''' <returns>品名一覧</returns>
    ''' <remarks></remarks>
    Public Function CreateProductLists(ByVal COMPCODE As String, ByVal ORGCODE As String, ByVal STYMD As String, ByVal ENDYMD As String, ByRef O_RTN As String) As ListBox
        If lmap.Contains(COMPCODE & ORGCODE & "PRODUCT") Then
            Return lmap.Item(COMPCODE & ORGCODE & "PRODUCT")
        Else
            Using Com As SqlConnection = sm.getConnection

                Try
                    '○　品名一覧ListBox設定()
                    Com.Open()

                    Dim SQLStr As String =
                         "SELECT isnull(rtrim(A.YPRODUCT),'')     as YPRODUCT     " _
                           & "  ,isnull(rtrim(A.KPRODUCT),'')     as KPRODUCT     " _
                           & "  ,isnull(rtrim(A.PRODUCTCODE),'')  as PRODUCTCODE  " _
                           & "  ,isnull(rtrim(B.PRODUCT2),'')     as PRODUCT2     " _
                           & "  ,isnull(rtrim(B.NAMES),'')        as PRODNAMES    " _
                           & "  ,isnull(rtrim(B.STANI),'')        as STANI        " _
                           & " FROM       MD002_PRODORG             A             " _
                           & " INNER JOIN MD001_PRODUCT             B             " _
                           & "   ON    B.PRODUCTCODE     = A.PRODUCTCODE          " _
                           & "   and   B.STYMD          <= @P5                    " _
                           & "   and   B.ENDYMD         >= @P4                    " _
                           & "   and   B.DELFLG         <> '1'                    " _
                           & " Where   A.CAMPCODE        = @P1                    " _
                           & "   and   A.UORG            = @P2                    " _
                           & "   and   A.STYMD          <= @P3                    " _
                           & "   and   A.ENDYMD         >= @P3                    " _
                           & "   and   A.DELFLG         <> '1'                    " _
                           & "GROUP BY A.YPRODUCT , A.KPRODUCT , A.PRODUCTCODE , B.PRODUCT2 , B.NAMES , B.STANI " _
                           & "ORDER BY A.YPRODUCT , A.KPRODUCT , A.PRODUCTCODE , B.PRODUCT2 , B.NAMES , B.STANI "
                    Using SQLcmd As New SqlCommand(SQLStr, Com)
                        Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.NVarChar, 20)
                        Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.NVarChar, 20)
                        Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.Date)
                        Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", System.Data.SqlDbType.Date)
                        Dim PARA5 As SqlParameter = SQLcmd.Parameters.Add("@P5", System.Data.SqlDbType.Date)
                        PARA1.Value = COMPCODE
                        PARA2.Value = ORGCODE
                        PARA3.Value = Date.Now
                        PARA4.Value = STYMD
                        PARA5.Value = ENDYMD

                        Dim lstY2G As New ListBox
                        Dim lstK2G As New ListBox
                        Dim lstP2G As New ListBox

                        Dim lstYPROD As New ListBox
                        Dim lstKPROD As New ListBox
                        Dim lstPPROD As New ListBox
                        Dim lstGPROD As New ListBox
                        Dim lstSTANI As New ListBox

                        Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                            While SQLdr.Read
                                '○出力編集
                                lstY2G.Items.Add(New ListItem(SQLdr("PRODUCTCODE"), SQLdr("YPRODUCT")))
                                lstK2G.Items.Add(New ListItem(SQLdr("PRODUCTCODE"), SQLdr("KPRODUCT")))
                                lstP2G.Items.Add(New ListItem(SQLdr("PRODUCTCODE"), SQLdr("PRODUCT2")))
                                lstYPROD.Items.Add(New ListItem(SQLdr("PRODNAMES"), SQLdr("YPRODUCT")))
                                lstKPROD.Items.Add(New ListItem(SQLdr("PRODNAMES"), SQLdr("KPRODUCT")))
                                lstPPROD.Items.Add(New ListItem(SQLdr("PRODNAMES"), SQLdr("PRODUCT2")))
                                If IsNothing(lstGPROD.Items.FindByValue(SQLdr("PRODUCTCODE"))) Then
                                    lstGPROD.Items.Add(New ListItem(SQLdr("PRODNAMES"), SQLdr("PRODUCTCODE")))
                                    lstSTANI.Items.Add(New ListItem(SQLdr("STANI"), SQLdr("PRODUCTCODE")))
                                End If
                            End While
                            lmap.Item(COMPCODE & ORGCODE & "PRODCY2G") = lstY2G
                            lmap.Item(COMPCODE & ORGCODE & "PRODCK2G") = lstK2G
                            lmap.Item(COMPCODE & ORGCODE & "PRODCP2G") = lstP2G

                            lmap.Item(COMPCODE & ORGCODE & "YPROD") = lstYPROD
                            lmap.Item(COMPCODE & ORGCODE & "KPROD") = lstKPROD
                            lmap.Item(COMPCODE & ORGCODE & "PROD2") = lstPPROD
                            lmap.Item(COMPCODE & ORGCODE & "PRODUCT") = lstGPROD
                            lmap.Item(COMPCODE & ORGCODE & "PRODSTANI") = lstSTANI

                        End Using
                        Return lstGPROD

                    End Using
                Catch ex As Exception
                    O_RTN = C_MESSAGE_NO.DB_ERROR
                    Return Nothing
                End Try
            End Using
        End If

    End Function
    ''' <summary>
    ''' 品名の請求単位一覧取得
    ''' </summary>
    ''' <param name="COMPCODE">会社コード</param>
    ''' <param name="ORGCODE">組織コード</param>
    ''' <param name="STYMD">開始日</param>
    ''' <param name="ENDYMD">終了日</param>
    ''' <param name="O_RTN">可否判定</param>
    ''' <returns>請求単位一覧</returns>
    ''' <remarks></remarks>
    Public Function CreateProduct2ClassList(ByVal COMPCODE As String, ByVal ORGCODE As String, ByVal STYMD As String, ByVal ENDYMD As String, ByRef O_RTN As String) As ListBox
        If lmap.Contains(COMPCODE & ORGCODE & "PRODSTANI") Then
            Return lmap.Item(COMPCODE & ORGCODE & "PRODSTANI")
        Else
            CreateProductLists(COMPCODE, ORGCODE, STYMD, ENDYMD, O_RTN)
            If isNormal(O_RTN) Then
                Return lmap.Item(COMPCODE & ORGCODE & "PRODSTANI")
            End If
        End If
        Return Nothing
    End Function
    ''' <summary>
    ''' 品名一覧取得
    ''' </summary>
    ''' <param name="COMPCODE">会社コード</param>
    ''' <param name="ORGCODE">組織コード</param>
    ''' <param name="STYMD">開始日</param>
    ''' <param name="ENDYMD">終了日</param>
    ''' <param name="O_RTN">可否判定</param>
    ''' <returns>請求単位一覧</returns>
    ''' <remarks></remarks>
    Public Function CreateProduct2Lists(ByVal COMPCODE As String, ByVal ORGCODE As String, ByVal STYMD As String, ByVal ENDYMD As String, ByRef O_RTN As String) As ListBox
        If lmap.Contains(COMPCODE & ORGCODE & "PROD2") Then
            Return lmap.Item(COMPCODE & ORGCODE & "PROD2")
        Else
            CreateProductLists(COMPCODE, ORGCODE, STYMD, ENDYMD, O_RTN)
            If isNormal(O_RTN) Then
                Return lmap.Item(COMPCODE & ORGCODE & "PROD2")
            End If
        End If
        Return Nothing
    End Function
    ''' <summary>
    ''' 矢崎用品名一覧取得
    ''' </summary>
    ''' <param name="COMPCODE">会社コード</param>
    ''' <param name="ORGCODE">組織コード</param>
    ''' <param name="STYMD">開始日</param>
    ''' <param name="ENDYMD">終了日</param>
    ''' <param name="O_RTN">可否判定</param>
    ''' <returns>請矢崎用品名一覧</returns>
    ''' <remarks></remarks>
    Public Function CreateYazakiProdList(ByVal COMPCODE As String, ByVal ORGCODE As String, ByVal STYMD As String, ByVal ENDYMD As String, ByRef O_RTN As String) As ListBox
        If lmap.Contains(COMPCODE & ORGCODE & "YPROD") Then
            Return lmap.Item(COMPCODE & ORGCODE & "YPROD")
        Else
            CreateProductLists(COMPCODE, ORGCODE, STYMD, ENDYMD, O_RTN)
            If isNormal(O_RTN) Then
                Return lmap.Item(COMPCODE & ORGCODE & "YPROD")
            End If
        End If
        Return Nothing
    End Function
    ''' <summary>
    ''' 光栄用品名一覧取得
    ''' </summary>
    ''' <param name="COMPCODE">会社コード</param>
    ''' <param name="ORGCODE">組織コード</param>
    ''' <param name="STYMD">開始日</param>
    ''' <param name="ENDYMD">終了日</param>
    ''' <param name="O_RTN">可否判定</param>
    ''' <returns>光栄崎用品名一覧</returns>
    ''' <remarks></remarks>
    Public Function CreateKoeiProdList(ByVal COMPCODE As String, ByVal ORGCODE As String, ByVal STYMD As String, ByVal ENDYMD As String, ByRef O_RTN As String) As ListBox
        If lmap.Contains(COMPCODE & ORGCODE & "KPROD") Then
            Return lmap.Item(COMPCODE & ORGCODE & "KPROD")
        Else
            CreateProductLists(COMPCODE, ORGCODE, STYMD, ENDYMD, O_RTN)
            If isNormal(O_RTN) Then
                Return lmap.Item(COMPCODE & ORGCODE & "KPROD")
            End If
        End If
        Return Nothing
    End Function
    ''' <summary>
    ''' 矢崎品名の変換一覧取得
    ''' </summary>
    ''' <param name="COMPCODE">会社コード</param>
    ''' <param name="ORGCODE">組織コード</param>
    ''' <param name="STYMD">開始日</param>
    ''' <param name="ENDYMD">終了日</param>
    ''' <param name="O_RTN">可否判定</param>
    ''' <returns>矢崎変換テーブル</returns>
    ''' <remarks></remarks>
    Public Function CreateProdY2G(ByVal COMPCODE As String, ByVal ORGCODE As String, ByVal STYMD As String, ByVal ENDYMD As String, ByRef O_RTN As String) As ListBox
        If lmap.Contains(COMPCODE & ORGCODE & "PRODCY2G") Then
            Return lmap.Item(COMPCODE & ORGCODE & "PRODCY2G")
        Else
            CreateProductLists(COMPCODE, ORGCODE, STYMD, ENDYMD, O_RTN)
            If isNormal(O_RTN) Then
                Return lmap.Item(COMPCODE & ORGCODE & "PRODCY2G")
            End If
        End If
        Return Nothing
    End Function
    ''' <summary>
    ''' 光栄品名の変換一覧取得
    ''' </summary>
    ''' <param name="COMPCODE">会社コード</param>
    ''' <param name="ORGCODE">組織コード</param>
    ''' <param name="STYMD">開始日</param>
    ''' <param name="ENDYMD">終了日</param>
    ''' <param name="O_RTN">可否判定</param>
    ''' <returns>光栄変換テーブル</returns>
    ''' <remarks></remarks>
    Public Function CreateProdK2G(ByVal COMPCODE As String, ByVal ORGCODE As String, ByVal STYMD As String, ByVal ENDYMD As String, ByRef O_RTN As String) As ListBox
        If lmap.Contains(COMPCODE & ORGCODE & "PRODCK2G") Then
            Return lmap.Item(COMPCODE & ORGCODE & "PRODCK2G")
        Else
            CreateProductLists(COMPCODE, ORGCODE, STYMD, ENDYMD, O_RTN)
            If isNormal(O_RTN) Then
                Return lmap.Item(COMPCODE & ORGCODE & "PRODCK2G")
            End If
        End If
        Return Nothing
    End Function
    ''' <summary>
    ''' 品名２の変換一覧取得
    ''' </summary>
    ''' <param name="COMPCODE">会社コード</param>
    ''' <param name="ORGCODE">組織コード</param>
    ''' <param name="STYMD">開始日</param>
    ''' <param name="ENDYMD">終了日</param>
    ''' <param name="O_RTN">可否判定</param>
    ''' <returns>光栄変換テーブル</returns>
    ''' <remarks></remarks>
    Public Function CreateProdP2G(ByVal COMPCODE As String, ByVal ORGCODE As String, ByVal STYMD As String, ByVal ENDYMD As String, ByRef O_RTN As String) As ListBox
        If lmap.Contains(COMPCODE & ORGCODE & "PRODCP2G") Then
            Return lmap.Item(COMPCODE & ORGCODE & "PRODCP2G")
        Else
            CreateProductLists(COMPCODE, ORGCODE, STYMD, ENDYMD, O_RTN)
            If isNormal(O_RTN) Then
                Return lmap.Item(COMPCODE & ORGCODE & "PRODCP2G")
            End If
        End If
        Return Nothing
    End Function
    ''' <summary>
    ''' 取引先一覧情報
    ''' </summary>
    ''' <param name="COMPCODE">会社コード</param>
    ''' <param name="ORGCODE">組織コード</param>
    ''' <param name="O_RTN">可否判定</param>
    ''' <returns>取引先情報一覧</returns>
    ''' <remarks></remarks>
    Public Function CreateShipperLists(ByVal COMPCODE As String, ByVal ORGCODE As String, ByRef O_RTN As String) As ListBox
        '○ 取引先ListBox設定（矢崎、光英）
        If lmap.Contains(COMPCODE & ORGCODE & "SHIPPER") Then
            Return lmap.Item(COMPCODE & ORGCODE & "SHIPPER")
        Else
            Using Com As SqlConnection = sm.getConnection

                Try
                    Com.Open()
                    Dim SQLStr As String =
                    "SELECT  isnull(rtrim(A.CAMPCODE),'')   as CAMPCODE " _
                   & "      ,isnull(rtrim(A.YTORICODE),'')  as YTORICODE " _
                   & "      ,isnull(rtrim(A.KTORICODE),'')  as KTORICODE " _
                   & "      ,isnull(rtrim(A.TORICODE),'')   as TORICODE " _
                   & "     , isnull(rtrim(B.NAMES),'')      as NAMES " _
                   & " FROM       MC003_TORIORG               A " _
                   & " INNER JOIN MC002_TORIHIKISAKI          B " _
                   & "   ON  B.TORICODE   = A.TORICODE " _
                   & "   and B.STYMD     <= @P3 " _
                   & "   and B.ENDYMD    >= @P3 " _
                   & "   and B.DELFLG    <> '1' " _
                   & " Where A.CAMPCODE   = @P1 " _
                   & "   and A.UORG       = @P2 " _
                   & "   and A.TORITYPE02 = 'NI' " _
                   & "   and A.DELFLG    <> '1' " _
                   & "ORDER BY A.CAMPCODE , A.SEQ , A.TORICODE, B.NAMES "

                    Using SQLcmd As New SqlCommand(SQLStr, Com)
                        Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.NVarChar, 20)
                        Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.NVarChar, 20)
                        Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.Date)

                        PARA1.Value = COMPCODE
                        PARA2.Value = ORGCODE
                        PARA3.Value = Date.Now

                        Dim lstSPY2G As New ListBox
                        Dim lstSPK2G As New ListBox

                        Dim lstShipper As New ListBox
                        Dim lstKShipper As New ListBox
                        Dim lstYShipper As New ListBox
                        Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

                            While SQLdr.Read
                                lstSPY2G.Items.Add(New ListItem(SQLdr("TORICODE"), SQLdr("YTORICODE")))
                                lstSPK2G.Items.Add(New ListItem(SQLdr("TORICODE"), SQLdr("KTORICODE")))

                                lstYShipper.Items.Add(New ListItem(SQLdr("NAMES"), SQLdr("YTORICODE")))
                                lstKShipper.Items.Add(New ListItem(SQLdr("NAMES"), SQLdr("KTORICODE")))
                                lstShipper.Items.Add(New ListItem(SQLdr("NAMES"), SQLdr("TORICODE")))
                            End While
                            lmap.Item(COMPCODE & ORGCODE & "SHIPPERCY2G") = lstSPY2G
                            lmap.Item(COMPCODE & ORGCODE & "SHIPPERCK2G") = lstSPK2G

                            lmap.Item(COMPCODE & ORGCODE & "YSHIPPER") = lstYShipper
                            lmap.Item(COMPCODE & ORGCODE & "KSHIPPER") = lstKShipper
                            lmap.Item(COMPCODE & ORGCODE & "SHIPPER") = lstShipper

                        End Using

                        Return lstShipper
                    End Using

                Catch ex As Exception
                    O_RTN = C_MESSAGE_NO.DB_ERROR
                    Return Nothing
                End Try
            End Using
        End If
    End Function
    ''' <summary>
    ''' 矢崎取引先変換一覧情報取得
    ''' </summary>
    ''' <param name="COMPCODE">会社コード</param>
    ''' <param name="ORGCODE">組織コード</param>
    ''' <param name="O_RTN">可否判定</param>
    ''' <returns>矢崎変換テーブル</returns>
    ''' <remarks></remarks>
    Public Function CreateShepperY2G(ByVal COMPCODE As String, ByVal ORGCODE As String, ByRef O_RTN As String) As ListBox
        If lmap.Contains(COMPCODE & ORGCODE & "SHIPPERCY2G") Then
            Return lmap.Item(COMPCODE & ORGCODE & "SHIPPERCY2G")
        Else
            CreateShipperLists(COMPCODE, ORGCODE, O_RTN)
            If isNormal(O_RTN) Then
                Return lmap.Item(COMPCODE & ORGCODE & "SHIPPERCY2G")
            End If
        End If
        Return Nothing
    End Function
    ''' <summary>
    ''' 光栄取引先変換一覧情報取得
    ''' </summary>
    ''' <param name="COMPCODE">会社コード</param>
    ''' <param name="ORGCODE">組織コード</param>
    ''' <param name="O_RTN">可否判定</param>
    ''' <returns>光栄変換テーブル</returns>
    ''' <remarks></remarks>
    Public Function CreateShepperK2G(ByVal COMPCODE As String, ByVal ORGCODE As String, ByRef O_RTN As String) As ListBox
        If lmap.Contains(COMPCODE & ORGCODE & "SHIPPERCK2G") Then
            Return lmap.Item(COMPCODE & ORGCODE & "SHIPPERCK2G")
        Else
            CreateShipperLists(COMPCODE, ORGCODE, O_RTN)
            If isNormal(O_RTN) Then
                Return lmap.Item(COMPCODE & ORGCODE & "SHIPPERCK2G")
            End If
        End If
        Return Nothing
    End Function
    ''' <summary>
    ''' 矢崎取引先一覧情報取得
    ''' </summary>
    ''' <param name="COMPCODE">会社コード</param>
    ''' <param name="ORGCODE">組織コード</param>
    ''' <param name="O_RTN">可否判定</param>
    ''' <returns>矢崎取引先情報一覧</returns>
    ''' <remarks></remarks>
    Public Function CreateYazakiShipperList(ByVal COMPCODE As String, ByVal ORGCODE As String, ByRef O_RTN As String) As ListBox
        If lmap.Contains(COMPCODE & ORGCODE & "YSHIPPER") Then
            Return lmap.Item(COMPCODE & ORGCODE & "YSHIPPER")
        Else
            CreateShipperLists(COMPCODE, ORGCODE, O_RTN)
            If isNormal(O_RTN) Then
                Return lmap.Item(COMPCODE & ORGCODE & "YSHIPPER")
            End If
        End If
        Return Nothing
    End Function
    ''' <summary>
    ''' 光栄取引先一覧情報取得
    ''' </summary>
    ''' <param name="COMPCODE">会社コード</param>
    ''' <param name="ORGCODE">組織コード</param>
    ''' <param name="O_RTN">可否判定</param>
    ''' <returns>光栄取引先情報一覧</returns>
    ''' <remarks></remarks>
    Public Function CreateKoeiShipperList(ByVal COMPCODE As String, ByVal ORGCODE As String, ByRef O_RTN As String) As ListBox
        If lmap.Contains(COMPCODE & ORGCODE & "KSHIPPER") Then
            Return lmap.Item(COMPCODE & ORGCODE & "KSHIPPER")
        Else
            CreateShipperLists(COMPCODE, ORGCODE, O_RTN)
            If isNormal(O_RTN) Then
                Return lmap.Item(COMPCODE & ORGCODE & "KSHIPPER")
            End If
        End If
        Return Nothing
    End Function
    ''' <summary>
    ''' 矢崎専用届け先取得
    ''' </summary>
    ''' <param name="COMPCODE">会社コード</param>
    ''' <param name="ORGCODE">部署コード</param>
    ''' <param name="CLASSCODE">区分コード</param>
    ''' <param name="O_RTN">可否判定</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function CreateYazakiConsigneeList(ByVal COMPCODE As String, ByVal ORGCODE As String, ByVal CLASSCODE As String, ByRef O_RTN As String) As ListBox
        '○ 届先コードListBox設定（矢崎）
        If lmap.Contains(COMPCODE & ORGCODE & "YCONSIGNEE" & CLASSCODE) Then
            Return lmap.Item(COMPCODE & ORGCODE & "YCONSIGNEE" & CLASSCODE)
        Else
            Using Com As SqlConnection = sm.getConnection

                Try
                    Com.Open()
                    Dim SQLStr As String =
                      "  SELECT isnull(rtrim(A.TODOKECODE),'')  as TODOKECODE   " _
                    & "        ,isnull(rtrim(A.YTODOKECODE),'') as YTODOKECODE  " _
                    & "        ,isnull(rtrim(B.NAMES),'')       as NAMES        " _
                    & "    FROM MC007_TODKORG A                     " _
                    & "   INNER JOIN MC006_TODOKESAKI B             " _
                    & "      ON B.CAMPCODE      = A.CAMPCODE        " _
                    & "     and B.TORICODE      = A.TORICODE        " _
                    & "     and B.TODOKECODE    = A.TODOKECODE      " _
                    & "     and B.CLASS        in (@P4, '')         " _
                    & "     and B.STYMD        <= @P3               " _
                    & "     and B.ENDYMD       >= @P3               " _
                    & "     and B.DELFLG       <> '1'               " _
                    & "   Where A.CAMPCODE      = @P1               " _
                    & "     and A.UORG          = @P2               " _
                    & "     and A.DELFLG       <> '1'               " _
                    & "   ORDER BY A.SEQ ,A.TODOKECODE              "


                    Using SQLcmd As New SqlCommand(SQLStr, Com)
                        Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.NVarChar, 20)
                        Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.NVarChar, 20)
                        Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.Date)
                        Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", System.Data.SqlDbType.NVarChar, 20)

                        PARA1.Value = COMPCODE
                        PARA2.Value = ORGCODE
                        PARA3.Value = Date.Now
                        PARA4.Value = CLASSCODE

                        Dim lstCGCY2G As New ListBox

                        Dim lstYConsignee As New ListBox

                        Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                            While SQLdr.Read
                                lstCGCY2G.Items.Add(New ListItem(SQLdr("TODOKECODE"), SQLdr("YTODOKECODE")))
                                lstYConsignee.Items.Add(New ListItem(SQLdr("NAMES"), SQLdr("TODOKECODE")))
                            End While
                            lmap.Item(COMPCODE & ORGCODE & "CGCY2G" & CLASSCODE) = lstCGCY2G

                            lmap.Item(COMPCODE & ORGCODE & "YCONSIGNEE" & CLASSCODE) = lstYConsignee
                        End Using
                        Return lstYConsignee
                    End Using
                Catch ex As Exception
                    O_RTN = C_MESSAGE_NO.DB_ERROR
                    Return Nothing
                End Try
            End Using
        End If
    End Function
    ''' <summary>
    ''' 矢崎届先変換情報取得
    ''' </summary>
    ''' <param name="COMPCODE">会社コード</param>
    ''' <param name="ORGCODE">部署コード</param>
    ''' <param name="CLASSCODE">区分コード</param>
    ''' <param name="O_RTN">可否判定</param>
    ''' <returns>矢崎届先変換テーブル</returns>
    ''' <remarks></remarks>
    Public Function CreateConsigneeY2G(ByVal COMPCODE As String, ByVal ORGCODE As String, ByVal CLASSCODE As String, ByRef O_RTN As String) As ListBox
        If lmap.Contains(COMPCODE & ORGCODE & "CGCY2G" & CLASSCODE) Then
            Return lmap.Item(COMPCODE & ORGCODE & "CGCY2G" & CLASSCODE)
        Else
            CreateYazakiConsigneeList(COMPCODE, ORGCODE, CLASSCODE, O_RTN)
            If isNormal(O_RTN) Then
                Return lmap.Item(COMPCODE & ORGCODE & "CGCY2G" & CLASSCODE)
            End If
        End If
        Return Nothing
    End Function
    ''' <summary>
    ''' 業務車番と統一車番の名称とコードを取得する
    ''' </summary>
    ''' <param name="COMPCODE">会社コード</param>
    ''' <param name="ORGCODE">組織コード</param>
    ''' <returns>業務車番と統一車番を連結した情報</returns>
    ''' <remarks></remarks>
    Public Function CreateWorkLorryList(ByVal COMPCODE As String, ByVal ORGCODE As String)
        Using Com As SqlConnection = sm.getConnection
            '○　業務車番ListBox設定()
            Try
                Com.Open()
                Dim SQLStr As String =
                     "SELECT isnull(rtrim(A.GSHABAN),'')  as GSHABAN " _
                       & "  ,isnull(rtrim(A.SHARYOTYPEF),'') + isnull(rtrim(A.TSHABANF),'') as TSHABANF " _
                       & "  ,isnull(rtrim(A.SHARYOTYPEB),'') + isnull(rtrim(A.TSHABANB),'') as TSHABANB " _
                       & "  ,isnull(rtrim(A.SHARYOTYPEB2),'') + isnull(rtrim(A.TSHABANB2),'') as TSHABANB2 " _
                       & "  ,isnull(rtrim(C.LICNPLTNO1),'') + isnull(rtrim(C.LICNPLTNO2),'') as FRONT " _
                       & "  ,isnull(rtrim(D.LICNPLTNO1),'') + isnull(rtrim(D.LICNPLTNO2),'') as BACK " _
                       & "  ,isnull(rtrim(E.LICNPLTNO1),'') + isnull(rtrim(E.LICNPLTNO2),'') as BACK2 " _
                       & "  ,isnull(rtrim(B.MANGOILTYPE),'') as MANGOILTYPE " _
                       & "  ,isnull(rtrim(C.HPRSINSNYMD),'') as HPRSINSNYMDF " _
                       & "  ,isnull(rtrim(C.LICNYMD),'') as LICNYMDF " _
                       & "  ,isnull(rtrim(D.HPRSINSNYMD),'') as HPRSINSNYMDB " _
                       & "  ,isnull(rtrim(D.LICNYMD),'') as LICNYMDB " _
                       & "  ,isnull(rtrim(E.HPRSINSNYMD),'') as HPRSINSNYMDB2 " _
                       & "  ,isnull(rtrim(E.LICNYMD),'') as LICNYMDB2 " _
                       & " FROM  MA006_SHABANORG   as A " _
                       & " LEFT JOIN MA002_SHARYOA B " _
                       & "   ON    B.CAMPCODE    = A.CAMPCODE " _
                       & "   and   B.SHARYOTYPE  = A.SHARYOTYPEF " _
                       & "   and   B.TSHABAN     = A.TSHABANF " _
                       & "   and   B.STYMD      <= @P3 " _
                       & "   and   B.ENDYMD     >= @P3 " _
                       & "   and   B.DELFLG     <> '1' " _
                       & " LEFT JOIN MA004_SHARYOC C " _
                       & "    ON   C.CAMPCODE   = A.CAMPCODE " _
                       & "   and   C.SHARYOTYPE = A.SHARYOTYPEF " _
                       & "   and   C.TSHABAN    = A.TSHABANF " _
                       & "   and   C.STYMD     <= @P3 " _
                       & "   and   C.ENDYMD    >= @P3 " _
                       & "   and   C.DELFLG    <> '1' " _
                       & " LEFT JOIN MA004_SHARYOC D " _
                       & "    ON   D.CAMPCODE   = A.CAMPCODE " _
                       & "   and   D.SHARYOTYPE = A.SHARYOTYPEB " _
                       & "   and   D.TSHABAN    = A.TSHABANB " _
                       & "   and   D.STYMD     <= @P3 " _
                       & "   and   D.ENDYMD    >= @P3 " _
                       & "   and   D.DELFLG    <> '1' " _
                       & " LEFT JOIN MA004_SHARYOC E " _
                       & "    ON   E.CAMPCODE   = A.CAMPCODE " _
                       & "   and   E.SHARYOTYPE = A.SHARYOTYPEB2 " _
                       & "   and   E.TSHABAN    = A.TSHABANB2 " _
                       & "   and   E.STYMD     <= @P3 " _
                       & "   and   E.ENDYMD    >= @P3 " _
                       & "   and   E.DELFLG    <> '1' " _
                       & " Where   A.CAMPCODE   = @P1 " _
                       & "   and   A.MANGUORG   = @P2 " _
                       & "   and   isnull(A.SUISOKBN,'0')  <> '2' " _
                       & "   and   A.DELFLG    <> '1' " _
                       & " ORDER BY A.SEQ, A.GSHABAN "

                Using SQLcmd = New SqlCommand(SQLStr, Com)

                    Dim PARA1 = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.NVarChar, 20)
                    Dim PARA2 = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.NVarChar, 20)
                    Dim PARA3 = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.Date)
                    PARA1.Value = COMPCODE
                    PARA2.Value = ORGCODE
                    PARA3.Value = Date.Now
                    Dim WF_ListBoxGSHABAN As New ListBox

                    Using SQLdr = SQLcmd.ExecuteReader()

                        Dim WW_NAME As String = ""
                        Dim WW_CODE As String = ""
                        While SQLdr.Read
                            '○出力編集
                            WW_CODE = SQLdr("GSHABAN") & "," & SQLdr("TSHABANF") & "," & SQLdr("TSHABANB") & "," & SQLdr("TSHABANB2")
                            WW_NAME = SQLdr("GSHABAN") & "　" & SQLdr("FRONT") & "　" & SQLdr("BACK") & "　" & SQLdr("BACK2")
                            WF_ListBoxGSHABAN.Items.Add(New ListItem(WW_NAME, WW_CODE))
                        End While
                    End Using

                    Return WF_ListBoxGSHABAN
                End Using
            Catch ex As Exception
                Return Nothing
            End Try
        End Using
    End Function

    ''' <summary>
    ''' 業務車番と付帯情報を取得する
    ''' </summary>
    ''' <param name="COMPCODE">会社コード</param>
    ''' <param name="ORGCODE">組織コード</param>
    ''' <returns>業務車番と付帯情報を連結した情報</returns>
    ''' <remarks></remarks>
    Public Function CreateWorkLorrySubLists(ByVal COMPCODE As String, ByVal ORGCODE As String) As Hashtable
        Using Com As SqlConnection = sm.getConnection
            '○　業務車番ListBox設定()
            Try
                Com.Open()
                Dim SQLStr As String =
                     "SELECT isnull(rtrim(A.GSHABAN),'')  as GSHABAN " _
                       & "  ,isnull(rtrim(A.SHARYOTYPEF),'') + isnull(rtrim(A.TSHABANF),'') as TSHABANF " _
                       & "  ,isnull(rtrim(A.SHARYOTYPEB),'') + isnull(rtrim(A.TSHABANB),'') as TSHABANB " _
                       & "  ,isnull(rtrim(A.SHARYOTYPEB2),'') + isnull(rtrim(A.TSHABANB2),'') as TSHABANB2 " _
                       & "  ,isnull(rtrim(C.LICNPLTNO1),'') + isnull(rtrim(C.LICNPLTNO2),'') as FRONT " _
                       & "  ,isnull(rtrim(D.LICNPLTNO1),'') + isnull(rtrim(D.LICNPLTNO2),'') as BACK " _
                       & "  ,isnull(rtrim(E.LICNPLTNO1),'') + isnull(rtrim(E.LICNPLTNO2),'') as BACK2 " _
                       & "  ,isnull(rtrim(B.MANGOILTYPE),'') as MANGOILTYPE " _
                       & "  ,isnull(rtrim(C.HPRSINSNYMD),'') as HPRSINSNYMDF " _
                       & "  ,isnull(rtrim(C.LICNYMD),'') as LICNYMDF " _
                       & "  ,isnull(rtrim(D.HPRSINSNYMD),'') as HPRSINSNYMDB " _
                       & "  ,isnull(rtrim(D.LICNYMD),'') as LICNYMDB " _
                       & "  ,isnull(rtrim(E.HPRSINSNYMD),'') as HPRSINSNYMDB2 " _
                       & "  ,isnull(rtrim(E.LICNYMD),'') as LICNYMDB2 " _
                       & " FROM  MA006_SHABANORG   as A " _
                       & " LEFT JOIN MA002_SHARYOA B " _
                       & "   ON    B.CAMPCODE    = A.CAMPCODE " _
                       & "   and   B.SHARYOTYPE  = A.SHARYOTYPEF " _
                       & "   and   B.TSHABAN     = A.TSHABANF " _
                       & "   and   B.STYMD      <= @P3 " _
                       & "   and   B.ENDYMD     >= @P3 " _
                       & "   and   B.DELFLG     <> '1' " _
                       & " LEFT JOIN MA004_SHARYOC C " _
                       & "    ON   C.CAMPCODE   = A.CAMPCODE " _
                       & "   and   C.SHARYOTYPE = A.SHARYOTYPEF " _
                       & "   and   C.TSHABAN    = A.TSHABANF " _
                       & "   and   C.STYMD     <= @P3 " _
                       & "   and   C.ENDYMD    >= @P3 " _
                       & "   and   C.DELFLG    <> '1' " _
                       & " LEFT JOIN MA004_SHARYOC D " _
                       & "    ON   D.CAMPCODE   = A.CAMPCODE " _
                       & "   and   D.SHARYOTYPE = A.SHARYOTYPEB " _
                       & "   and   D.TSHABAN    = A.TSHABANB " _
                       & "   and   D.STYMD     <= @P3 " _
                       & "   and   D.ENDYMD    >= @P3 " _
                       & "   and   D.DELFLG    <> '1' " _
                       & " LEFT JOIN MA004_SHARYOC E " _
                       & "    ON   E.CAMPCODE   = A.CAMPCODE " _
                       & "   and   E.SHARYOTYPE = A.SHARYOTYPEB2 " _
                       & "   and   E.TSHABAN    = A.TSHABANB2 " _
                       & "   and   E.STYMD     <= @P3 " _
                       & "   and   E.ENDYMD    >= @P3 " _
                       & "   and   E.DELFLG    <> '1' " _
                       & " Where   A.CAMPCODE   = @P1 " _
                       & "   and   A.MANGUORG   = @P2 " _
                       & "   and   isnull(A.SUISOKBN,'0')  <> '2' " _
                       & "   and   A.DELFLG    <> '1' " _
                       & " ORDER BY A.SEQ, A.GSHABAN "

                Using SQLcmd = New SqlCommand(SQLStr, Com)

                    Dim PARA1 = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.NVarChar, 20)
                    Dim PARA2 = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.NVarChar, 20)
                    Dim PARA3 = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.Date)
                    PARA1.Value = COMPCODE
                    PARA2.Value = ORGCODE
                    PARA3.Value = Date.Now

                    Dim WW_HASH As New Hashtable

                    Using SQLdr = SQLcmd.ExecuteReader()


                        Dim WW_NAME As String = ""
                        Dim WW_CODE As String = ""
                        While SQLdr.Read
                            '○出力編集
                            Dim GSHABANPARAMLIST As New ListBox

                            GSHABANPARAMLIST.Items.Add(New ListItem("HPRSINSNYMDF", SQLdr("HPRSINSNYMDF")))
                            GSHABANPARAMLIST.Items.Add(New ListItem("HPRSINSNYMDB", SQLdr("HPRSINSNYMDB")))
                            GSHABANPARAMLIST.Items.Add(New ListItem("HPRSINSNYMDB2", SQLdr("HPRSINSNYMDB2")))
                            GSHABANPARAMLIST.Items.Add(New ListItem("LICNYMDF", SQLdr("LICNYMDF")))
                            GSHABANPARAMLIST.Items.Add(New ListItem("LICNYMDB", SQLdr("LICNYMDB")))
                            GSHABANPARAMLIST.Items.Add(New ListItem("LICNYMDB2", SQLdr("LICNYMDB2")))
                            GSHABANPARAMLIST.Items.Add(New ListItem("FRONT", SQLdr("FRONT")))
                            GSHABANPARAMLIST.Items.Add(New ListItem("BACK", SQLdr("BACK")))
                            GSHABANPARAMLIST.Items.Add(New ListItem("BACK2", SQLdr("BACK2")))
                            GSHABANPARAMLIST.Items.Add(New ListItem("MANGOILTYPE", SQLdr("MANGOILTYPE")))
                            GSHABANPARAMLIST.Items.Add(New ListItem("GSHABAN", SQLdr("GSHABAN")))
                            GSHABANPARAMLIST.Items.Add(New ListItem("TSHABANF", SQLdr("TSHABANF")))
                            GSHABANPARAMLIST.Items.Add(New ListItem("TSHABANB", SQLdr("TSHABANB")))
                            GSHABANPARAMLIST.Items.Add(New ListItem("TSHABANB2", SQLdr("TSHABANB2")))
                            WW_HASH.Add(SQLdr("GSHABAN"), GSHABANPARAMLIST)
                        End While
                    End Using

                    Return WW_HASH
                End Using
            Catch ex As Exception
                'O_RTN = C_MESSAGE_NO.DB_ERROR
                Return Nothing
            End Try
        End Using
    End Function


    ''' <summary>
    '''光英ファイル取得処理
    ''' </summary>
    ''' <param name="orgCode">対象部署</param>
    ''' <param name="dicDataFile">受信ファイルリスト</param>
    ''' <param name="O_RTN">エラーコード</param>
    ''' <remarks></remarks>
    Public Sub GetKoueiFile(ByVal orgCode As String,
                            ByRef dicDataFile As Dictionary(Of String, List(Of FileInfo)),
                            ByRef O_RTN As String)

        If IsNothing(dicDataFile) Then
            dicDataFile = New Dictionary(Of String, List(Of FileInfo))
        Else
            dicDataFile.Clear()
        End If

        'FTPコントロール
        Dim control As FtpControl = Nothing

        Try
            control = New FtpControl

            For Each target In FTP_LIST
                'FTPリクエスト実行
                control.Request(target, orgCode)
                If Not isNormal(control.ERR) Then
                    O_RTN = control.ERR
                    Exit Sub
                End If
            Next

            '受信済みローカル光英ファイル取得
            Dim koueiPath As String = Path.Combine(sm.UPLOAD_PATH, C_DIR_KOUEI, orgCode, C_DIR_KOUEI_RESULT)
            Dim localDir = New DirectoryInfo(koueiPath)
            '[koueiType]_[jotsyasai]_[受信日時].csv
            Dim localFiles = localDir.GetFiles(C_KOUEI_RESULT_FILE_SERCH)

            If localFiles.Count = 0 Then
                Exit Sub
            End If

            For Each file In localFiles
                'ファイル名の1項目目はタイプ（区切り文字:"_"）
                Dim wk = file.Name.Split("_")
                Dim filetype = wk(0)
                'タイプ別ファイル一覧作成
                If Not dicDataFile.ContainsKey(filetype) Then
                    dicDataFile.Add(filetype, New List(Of FileInfo))
                End If
                dicDataFile(filetype).Add(file)
            Next

        Catch ex As Exception
            O_RTN = C_MESSAGE_NO.SYSTEM_ADM_ERROR
        Finally
            If Not IsNothing(control) Then
                control.Close()
            End If
        End Try
    End Sub

End Class