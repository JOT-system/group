﻿Imports System.Data.SqlClient
Imports OFFICE.GRIS0005LeftBox
Imports BASEDLL

Public Class GRT00013WRKINC
    Inherits UserControl

    Public Const MAPIDS As String = "T00013S"               'MAPID(条件)
    Public Const MAPID As String = "T00013"                 'MAPID(実行)
    Public Const CAMP_ENEX As String = "02"                 '会社コード(エネックス)
    Public Const CAMP_KNK As String = "03"                  '会社コード(近石)
    Public Const CAMP_NJS As String = "04"                  '会社コード(NJS)
    Public Const CAMP_JKT As String = "05"                  '会社コード(JKT)

    Public T0007COM As New GRT0007COM                      '勤怠共通

    '○ 共通関数宣言(BASEDLL)
    Private CS0050SESSION As New CS0050SESSION          'セッション情報操作処理

    ''' <summary>
    ''' ワークデータ初期化処理
    ''' </summary>
    ''' <param name="I_USERID"></param>
    Public Sub Initialize(ByVal I_USERID As String)
    End Sub


    ''' <summary>
    ''' 部署パラメーター
    ''' </summary>
    ''' <param name="I_COMPCODE"></param>
    ''' <returns></returns>
    ''' <remarks>全部(名称取得用のため)</remarks>
    Public Function CreateORGParam(ByVal I_COMPCODE As String) As Hashtable

        Dim prmData As New Hashtable
        prmData.Item(C_PARAMETERS.LP_COMPANY) = I_COMPCODE
        prmData.Item(C_PARAMETERS.LP_TYPEMODE) = GL0002OrgList.LS_AUTHORITY_WITH.NO_AUTHORITY
        prmData.Item(C_PARAMETERS.LP_PERMISSION) = C_PERMISSION.INVALID
        prmData.Item(C_PARAMETERS.LP_ORG_CATEGORYS) = New String() {
            GL0002OrgList.C_CATEGORY_LIST.BRANCH_OFFICE,
            GL0002OrgList.C_CATEGORY_LIST.CARAGE,
            GL0002OrgList.C_CATEGORY_LIST.DEPARTMENT,
            GL0002OrgList.C_CATEGORY_LIST.OFFICE_PLACE,
            GL0002OrgList.C_CATEGORY_LIST.OFFICER}

        CreateORGParam = prmData

    End Function


    ''' <summary>
    ''' 職務区分パラメーター
    ''' </summary>
    ''' <param name="I_COMPCODE"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function CreateStaffKBNParam(ByVal I_COMPCODE As String) As Hashtable

        Dim prmData As New Hashtable
        Dim StaffKbnList As New ListBox
        Dim SQLcon = CS0050SESSION.getConnection()
        SQLcon.Open()
        Dim SQLcmd As New SqlCommand()
        Dim SQLdr As SqlDataReader = Nothing
        Dim PARA(10) As SqlParameter
        Dim SQLStr As New StringBuilder

        '○ 職務区分リストボックス作成
        Try

            '検索SQL文
            SQLStr.AppendLine(" SELECT rtrim(KEYCODE) as KEYCODE    ")
            SQLStr.AppendLine("       ,rtrim(VALUE1)  as VALUE1     ")
            SQLStr.AppendLine(" FROM  MC001_FIXVALUE                ")
            SQLStr.AppendLine(" Where CAMPCODE  = @P1               ")
            SQLStr.AppendLine("   and CLASS     = @P2               ")
            SQLStr.AppendLine("   and STYMD    <= @P3               ")
            SQLStr.AppendLine("   and ENDYMD   >= @P4               ")
            SQLStr.AppendLine("   and DELFLG   <> @P5               ")
            SQLStr.AppendLine("   and KEYCODE LIKE '03%'        ")
            SQLStr.AppendLine("ORDER BY KEYCODE                     ")

            SQLcmd = New SqlCommand(SQLStr.ToString, SQLcon)
            PARA(1) = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.NVarChar)
            PARA(2) = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.NVarChar)
            PARA(3) = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.Date)
            PARA(4) = SQLcmd.Parameters.Add("@P4", System.Data.SqlDbType.Date)
            PARA(5) = SQLcmd.Parameters.Add("@P5", System.Data.SqlDbType.NVarChar)
            PARA(1).Value = I_COMPCODE
            PARA(2).Value = "STAFFKBN"
            PARA(3).Value = Date.Now
            PARA(4).Value = Date.Now
            PARA(5).Value = C_DELETE_FLG.DELETE

            SQLdr = SQLcmd.ExecuteReader()

            While SQLdr.Read
                StaffKbnList.Items.Add(New ListItem(SQLdr("VALUE1"), SQLdr("KEYCODE")))
            End While

            prmData.Item(C_PARAMETERS.LP_LIST) = StaffKbnList

        Finally
            If Not IsNothing(SQLdr) Then
                SQLdr.Close()
                SQLdr = Nothing
            End If

            SQLcmd.Dispose()
            SQLcmd = Nothing

            SQLcon.Close()
            SQLcon.Dispose()
            SQLcon = Nothing
        End Try

        CreateStaffKBNParam = prmData

    End Function


    ''' <summary>
    ''' 配属部署パラメーター
    ''' </summary>
    ''' <param name="I_COMPCODE"></param>
    ''' <param name="I_USERID"></param>
    ''' <param name="I_ROLEORG"></param>
    ''' <returns>車庫、部、事業所</returns>
    Public Function CreateHORGParam(ByVal I_COMPCODE As String, ByVal I_USERID As String, ByVal I_ROLEORG As String) As Hashtable

        Dim prmData As New Hashtable
        Dim ORGList As New ListBox

        Using SQLcon As SqlConnection = CS0050SESSION.getConnection()
            SQLcon.Open()       'DataBase接続

            Dim SQLcmd As New SqlCommand()

            '○配属部署リストボックス作成
            Dim SQLStr As String =
                  " SELECT" _
                & "    RTRIM(M006.CODE)    AS ORG" _
                & "    , RTRIM(M002.NAMES) AS ORGNAMES" _
                & " FROM" _
                & "    M0006_STRUCT M006" _
                & "    INNER JOIN M0002_ORG M002" _
                & "        ON  M002.CAMPCODE    = M006.CAMPCODE" _
                & "        AND M002.ORGCODE     = M006.CODE" _
                & "        AND M002.STYMD      <= @P7" _
                & "        AND M002.ENDYMD     >= @P7" _
                & "        AND M002.DELFLG     <> @P8" _
                & "    INNER JOIN S0006_ROLE S006" _
                & "        ON  S006.CAMPCODE    = M006.CAMPCODE" _
                & "        AND S006.OBJECT      = M006.OBJECT" _
                & "        AND S006.ROLE        = @P5" _
                & "        AND S006.PERMITCODE >= @P6" _
                & "        AND S006.STYMD      <= @P7" _
                & "        AND S006.ENDYMD     >= @P7" _
                & "        AND M002.DELFLG     <> @P8" _
                & " WHERE" _
                & "    M006.CAMPCODE     = @P1" _
                & "    AND M006.OBJECT   = @P2" _
                & "    AND M006.STRUCT   = @P3" _
                & "    AND M006.GRCODE01 = @P4" _
                & "    AND M006.STYMD   <= @P7" _
                & "    AND M006.ENDYMD  >= @P7" _
                & "    AND M006.DELFLG  <> @P8" _
                & " GROUP BY" _
                & "    M006.CODE" _
                & "    , M002.NAMES"

            Try
                SQLcmd = New SqlCommand(SQLStr, SQLcon)

                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", SqlDbType.NVarChar, 20)        '会社コード
                Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", SqlDbType.NVarChar, 20)        'オブジェクト
                Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", SqlDbType.NVarChar, 50)        '構造コード
                Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", SqlDbType.NVarChar, 20)        'グループコード1
                Dim PARA5 As SqlParameter = SQLcmd.Parameters.Add("@P5", SqlDbType.NVarChar, 20)        'ロール
                Dim PARA6 As SqlParameter = SQLcmd.Parameters.Add("@P6", SqlDbType.Int)                 '権限コード
                Dim PARA7 As SqlParameter = SQLcmd.Parameters.Add("@P7", SqlDbType.Date)                '現在日付
                Dim PARA8 As SqlParameter = SQLcmd.Parameters.Add("@P8", SqlDbType.NVarChar, 1)         '削除フラグ

                PARA1.Value = I_COMPCODE
                PARA2.Value = C_ROLE_VARIANT.USER_ORG
                PARA3.Value = "管轄組織"
                PARA5.Value = I_ROLEORG
                PARA6.Value = C_PERMISSION.UPDATE
                PARA7.Value = Date.Now
                PARA8.Value = C_DELETE_FLG.DELETE

                For Each Category As String In {GL0002OrgList.C_CATEGORY_LIST.CARAGE,
                                            GL0002OrgList.C_CATEGORY_LIST.DEPARTMENT,
                                            GL0002OrgList.C_CATEGORY_LIST.OFFICE_PLACE}
                    PARA4.Value = Category

                    Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                        While SQLdr.Read
                            ORGList.Items.Add(New ListItem(SQLdr("ORGNAMES"), SQLdr("ORG")))
                        End While
                    End Using
                Next
            Finally
                SQLcmd.Dispose()
                SQLcmd = Nothing
            End Try
        End Using

        prmData.Item(C_PARAMETERS.LP_LIST) = ORGList
        CreateHORGParam = prmData

    End Function


    ''' <summary>
    ''' 従業員パラメーター
    ''' </summary>
    ''' <param name="I_TYPE"></param>
    ''' <param name="I_COMPCODE"></param>
    ''' <param name="I_TAISHOYM"></param>
    ''' <param name="I_HORG"></param>
    ''' <param name="I_STAFFKBN"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function CreateStaffCodeParam(ByVal I_TYPE As GL0005StaffList.LC_STAFF_TYPE, ByVal I_COMPCODE As String, ByVal I_TAISHOYM As String,
                                         ByVal I_HORG As String, ByVal I_STAFFKBN As String, Optional ByVal I_STAFFCODE As String = "") As Hashtable

        Dim prmData As New Hashtable

        prmData.Item(C_PARAMETERS.LP_TYPEMODE) = I_TYPE
        prmData.Item(C_PARAMETERS.LP_DEFAULT_SORT) = GL0005StaffList.C_DEFAULT_SORT.SEQ
        prmData.Item(C_PARAMETERS.LP_COMPANY) = I_COMPCODE

        '開始、終了日付
        Dim WW_DATE As Date
        If Not String.IsNullOrEmpty(I_TAISHOYM) AndAlso IsDate(I_TAISHOYM & "/01") Then
            WW_DATE = CDate(I_TAISHOYM & "/01")
        Else
            WW_DATE = CDate(Date.Now.ToString("yyyy/MM") & "/01")
        End If

        prmData.Item(C_PARAMETERS.LP_STYMD) = WW_DATE
        prmData.Item(C_PARAMETERS.LP_ENDYMD) = WW_DATE.AddMonths(1).AddDays(-1)

        '配属部署
        Dim orgCode As String = ""
        Dim retCode As String = ""
        T0007COM.ConvORGCODE(I_COMPCODE, I_HORG, orgCode, retCode)
        If retCode = C_MESSAGE_NO.NORMAL Then
            prmData.Item(C_PARAMETERS.LP_ORG) = orgCode
        Else
            prmData.Item(C_PARAMETERS.LP_ORG) = I_HORG
        End If

        '職務区分
        If Not String.IsNullOrEmpty(I_STAFFKBN) Then
            Dim KBNList As New List(Of String)
            KBNList.Add(I_STAFFKBN)
            prmData.Item(C_PARAMETERS.LP_STAFF_KBN_LIST) = KBNList
        End If

        '条件画面で従業員を絞っている場合
        If Not String.IsNullOrEmpty(I_STAFFCODE) Then
            prmData.Item(C_PARAMETERS.LP_SELECTED_CODE) = I_STAFFCODE
        End If

        CreateStaffCodeParam = prmData

    End Function


    ''' <summary>
    ''' 宿直区分パラメーター
    ''' </summary>
    ''' <returns></returns>
    Public Function CreateShukchokKBNParam() As Hashtable

        Dim prmData As New Hashtable
        Dim FixValueList As New ListBox

        'その他作業部署取得
        Dim specialOrg As ListBox = T0007COM.getList(WF_SEL_CAMPCODE.Text, GRT00007WRKINC.CONST_SPEC)

        Using SQLcon As SqlConnection = CS0050SESSION.getConnection()
            SQLcon.Open()       'DataBase接続

            Dim SQLcmd As New SqlCommand()
            Dim SQLdr As SqlDataReader = Nothing

            Dim SQLStr As String =
                  " SELECT" _
                & "    RTRIM(KEYCODE)  AS KEYCODE" _
                & "    , RTRIM(VALUE1) AS VALUE1" _
                & "    , RTRIM(VALUE2) AS VALUE2" _
                & " FROM" _
                & "    MC001_FIXVALUE" _
                & " WHERE" _
                & "    CAMPCODE    = @P1" _
                & "    AND CLASS   = @P2" _
                & "    AND STYMD  <= @P3" _
                & "    AND ENDYMD >= @P3" _
                & "    AND DELFLG <> @P4" _
                & " ORDER BY" _
                & "    KEYCODE"

            Try
                SQLcmd = New SqlCommand(SQLStr, SQLcon)

                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", SqlDbType.NVarChar, 20)        '会社コード
                Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", SqlDbType.NVarChar, 20)        '分類
                Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", SqlDbType.Date)                '現在日付
                Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", SqlDbType.NVarChar, 1)         '削除フラグ

                PARA1.Value = WF_SEL_CAMPCODE.Text
                PARA2.Value = "T0009_SHUKCHOKKBN"
                PARA3.Value = Date.Now
                PARA4.Value = C_DELETE_FLG.DELETE

                SQLdr = SQLcmd.ExecuteReader()

                While SQLdr.Read
                    If Not IsNothing(specialOrg.Items.FindByValue(WF_SEL_HORG.Text)) Then
                        FixValueList.Items.Add(New ListItem(SQLdr("VALUE2"), SQLdr("KEYCODE")))
                    Else
                        FixValueList.Items.Add(New ListItem(SQLdr("VALUE1"), SQLdr("KEYCODE")))
                    End If
                End While
            Finally
                If Not IsNothing(SQLdr) Then
                    SQLdr.Close()
                    SQLdr = Nothing
                End If

                SQLcmd.Dispose()
                SQLcmd = Nothing
            End Try
        End Using

        prmData.Item(C_PARAMETERS.LP_LIST) = FixValueList
        CreateShukchokKBNParam = prmData

    End Function

End Class
