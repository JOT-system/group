Imports System.Data.SqlClient
Imports System.Reflection

Public Class GRTA0010WRKINC
    Inherits System.Web.UI.UserControl

    ' # MAPID
    ''' <summary>
    ''' 検索条件設定画面
    ''' </summary>
    Public Const MAPIDS As String = "TA0010S"
    ''' <summary>
    ''' 検索結果表示画面
    ''' </summary>
    Public Const MAPID As String = "TA0010"

    '○ 共通関数宣言(BASEDLL)
    Private CS0050SESSION As New CS0050SESSION          'セッション情報操作処理

    Public Enum MONTHS As Integer
        January = 1
        February = 2
        March = 3
        April = 4
        May = 5
        June = 6
        July = 7
        August = 8
        September = 9
        October = 10
        November = 11
        December = 12
    End Enum

    ''' <summary>
    ''' 表示用フラグ
    ''' </summary>
    Public Enum VIEW_FLG As Integer
        ''' <summary>
        ''' なにもしない(Default)
        ''' </summary>
        None = 0
        ''' <summary>
        ''' 警告
        ''' </summary>
        Warning = 1
        ''' <summary>
        ''' 問題あり
        ''' </summary>
        Problematic = 2
        ''' <summary>
        ''' ステータスで置き換える
        ''' </summary>
        ReplaceStatus = 4
        ''' <summary>
        ''' "{0}回"で置き換える
        ''' </summary>
        FormatCount = 8
        ''' <summary>
        ''' "あと{0}回"で置き換える
        ''' </summary>
        FormatRemainingCount = 16
    End Enum

    ''' <summary>
    ''' ワークデータ初期化
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub Initialize()

    End Sub

    ''' <summary>
    ''' 固定値マスタから一覧の取得
    ''' </summary>
    ''' <param name="COMPCODE">会社コード</param>
    ''' <param name="FIXCODE">固定値コード</param>
    ''' <returns>検索条件テーブル</returns>
    ''' <remarks></remarks>
    Public Function CreateFIXParam(ByVal COMPCODE As String, Optional ByVal FIXCODE As String = "") As Hashtable
        Dim prmData As New Hashtable
        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_COMPANY) = COMPCODE
        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_FIX_CLASS) = FIXCODE
        Return prmData
    End Function

    ''' <summary>
    ''' 部署一覧の取得
    ''' </summary>
    ''' <param name="COMPCODE">会社コード</param>
    ''' <param name="PRMIT">権限コード</param>
    ''' <returns></returns>
    Public Function CreateSORGParam(ByVal COMPCODE As String, ByVal PRMIT As String) As Hashtable
        Dim prmData As New Hashtable
        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_COMPANY) = COMPCODE
        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_ORG_CATEGORYS) = New String() {
            GL0002OrgList.C_CATEGORY_LIST.BRANCH_OFFICE,
            GL0002OrgList.C_CATEGORY_LIST.CARAGE,
            GL0002OrgList.C_CATEGORY_LIST.OFFICE_PLACE,
            GL0002OrgList.C_CATEGORY_LIST.DEPARTMENT
        }
        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_PERMISSION) = PRMIT
        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_TYPEMODE) = GL0002OrgList.LS_AUTHORITY_WITH.USER
        Return prmData
    End Function

    ''' <summary>
    ''' 時間外労働基準データ取得用クラス
    ''' </summary>
    Public Class WorkOverCriteria
        Inherits ClassTable(Of Item)

        Public Overloads Property Items As Item

        Public Class Item
            Inherits BaseItem(Of Item)

            Public Property YearMaxTime As Decimal
            Public Property YearWarnTime As Decimal
            Public Property MonthMaxTime As Decimal
            Public Property MonthWarnTime As Decimal
            Public Property MonthPrincipleTime As Decimal
            Public Property MonthPrincipleCount As Decimal
            Public Property AvgMonthMaxTime As Decimal
            Public Property AvgMonthWarnTime As Decimal
            Public Property AvgCalcMonths As Decimal

        End Class

        Public Sub Fetch(ByVal SQLcon As SqlConnection, ByVal campCode As String)

            '検索SQL文
            Dim SQLStr As New StringBuilder(10000)
            SQLStr.AppendLine("SELECT ")
            SQLStr.AppendLine("  KEYCODE AS CODE, ")
            SQLStr.AppendLine("  VALUE1 AS VALUE ")
            SQLStr.AppendLine("FROM ")
            SQLStr.AppendLine("  MC001_FIXVALUE ")
            SQLStr.AppendLine("WHERE ")
            SQLStr.AppendLine("  CAMPCODE = @P1 ")
            SQLStr.AppendLine("  AND ")
            SQLStr.AppendLine("  CLASS = @P2 ")
            SQLStr.AppendLine("  AND ")
            SQLStr.AppendLine("  STYMD <= @P3 ")
            SQLStr.AppendLine("  AND ")
            SQLStr.AppendLine("  ENDYMD >= @P4 ")
            SQLStr.AppendLine("  AND ")
            SQLStr.AppendLine("  DELFLG <> @P5 ")

            Try
                'DataBase接続
                SQLcon.Open()

                Using SQLcmd As SqlCommand = New SqlCommand(SQLStr.ToString, SQLcon)
                    Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P1", SqlDbType.NVarChar, 20)
                    Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P2", SqlDbType.NVarChar, 20)
                    Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@P3", SqlDbType.Date)
                    Dim PARA04 As SqlParameter = SQLcmd.Parameters.Add("@P4", SqlDbType.Date)
                    Dim PARA05 As SqlParameter = SQLcmd.Parameters.Add("@P5", SqlDbType.NVarChar, 20)

                    PARA01.Value = campCode
                    PARA02.Value = "OVERWORK"
                    PARA03.Value = Date.Now
                    PARA04.Value = Date.Now
                    PARA05.Value = "1"

                    SQLcmd.CommandTimeout = 300

                    Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                        ' 結果読み込み
                        Dim itemList As IEnumerable(Of Item) = ReferDB(Of Item).ReadAll(SQLdr, "CODE", "VALUE")
                        If Not IsNothing(itemList) AndAlso itemList.Any() Then
                            ' 結果格納
                            Items = itemList.First()
                        Else
                            Items = New Item
                        End If
                    End Using
                End Using
            Catch ex As Exception
                Throw ex
            End Try
        End Sub

    End Class

    ''' <summary>
    ''' 時間外労働データ取得用クラス
    ''' </summary>
    Public Class WorkOverData
        Inherits ClassTable(Of Item)

        Public Class Item
            Inherits BaseItem(Of Item)
            Public Property OrgCode As String
            Public Property OrgName As String
            Public Property OrgSeq As Integer
            Public Property StaffCode As String
            Public Property StaffName As String
            Public Property TaishoYM As String
            Public Property OverTime As Decimal
            Public Property OverTimeWithHoliday As Decimal
        End Class

        Public Sub Fetch(ByVal SQLcon As SqlConnection,
                         ByVal campCode As String,
                         ByVal orgCode As String,
                         ByVal beginDate As Date,
                         ByVal endDate As Date,
                         ByVal loginUserOrgCode As String)

            '検索SQL文
            Dim SQLStr As New StringBuilder(10000)
            SQLStr.AppendLine("WITH ")
            SQLStr.AppendLine("  STRUCT_WITH AS ( ")
            SQLStr.AppendLine("    SELECT CAMPCODE, CODE, STRUCT, GRCODE01, STRUCT + '_' + GRCODE02 AS GRSTRUCT02, SEQ ")
            SQLStr.AppendLine("    FROM M0006_STRUCT ")
            SQLStr.AppendLine("    WHERE CAMPCODE = @P1 AND OBJECT = @P2 AND DELFLG <> @P7 AND STYMD <= @P5 AND ENDYMD >= @P5 ")
            SQLStr.AppendLine("  ), ")
            SQLStr.AppendLine("  ORG_WITH AS ( ")
            SQLStr.AppendLine("    SELECT ORGCODE, NAMES ")
            SQLStr.AppendLine("    FROM M0002_ORG ")
            SQLStr.AppendLine("    WHERE CAMPCODE = @P1 AND DELFLG <> @P7 AND STYMD <= @P5 AND ENDYMD >= @P5 ")
            SQLStr.AppendLine("  ), ")
            SQLStr.AppendLine("  ORGLIST_WITH AS ( ")
            SQLStr.AppendLine("    SELECT ")
            SQLStr.AppendLine("      SUB_STRUCT_C.CAMPCODE  AS CAMPCODE, ")
            SQLStr.AppendLine("      SUB_STRUCT_C.CODE    AS ORGCODE, ")
            SQLStr.AppendLine("      SUB_ORG.NAMES      AS ORGNAME, ")
            SQLStr.AppendLine("      SUB_STRUCT_C.SEQ    AS ORGSEQ ")
            SQLStr.AppendLine("    FROM ")
            SQLStr.AppendLine("      (SELECT GRSTRUCT02 FROM STRUCT_WITH WHERE STRUCT = @P3 AND CODE = @P9) SUB_STRUCT_A ")
            SQLStr.AppendLine("      INNER JOIN ")
            SQLStr.AppendLine("        STRUCT_WITH SUB_STRUCT_B ")
            SQLStr.AppendLine("        ON ")
            SQLStr.AppendLine("          SUB_STRUCT_A.GRSTRUCT02 = SUB_STRUCT_B.STRUCT ")
            SQLStr.AppendLine("      INNER JOIN ")
            SQLStr.AppendLine("        (SELECT CAMPCODE, CODE, SEQ FROM STRUCT_WITH WHERE STRUCT = @P3) SUB_STRUCT_C ")
            SQLStr.AppendLine("        ON ")
            SQLStr.AppendLine("          SUB_STRUCT_B.CODE = SUB_STRUCT_C.CODE ")
            SQLStr.AppendLine("      INNER JOIN ")
            SQLStr.AppendLine("        ORG_WITH SUB_ORG ")
            SQLStr.AppendLine("        ON ")
            SQLStr.AppendLine("          SUB_STRUCT_C.CODE = SUB_ORG.ORGCODE ")
            If Not String.IsNullOrWhiteSpace(orgCode) Then
                SQLStr.AppendLine("    WHERE ")
                SQLStr.AppendLine("      SUB_STRUCT_C.CODE = @P8 ")
            End If
            SQLStr.AppendLine("  ), ")
            SQLStr.AppendLine("  STAFF_WITH AS ( ")
            SQLStr.AppendLine("    SELECT ")
            SQLStr.AppendLine("      ORGLIST_WITH.CAMPCODE, ")
            SQLStr.AppendLine("      ORGLIST_WITH.ORGCODE, ")
            SQLStr.AppendLine("      ORGLIST_WITH.ORGNAME, ")
            SQLStr.AppendLine("      ORGLIST_WITH.ORGSEQ, ")
            SQLStr.AppendLine("      MB001_STAFF.STAFFCODE, ")
            SQLStr.AppendLine("      MB001_STAFF.STAFFNAMES AS STAFFNAME ")
            SQLStr.AppendLine("    FROM ")
            SQLStr.AppendLine("      ORGLIST_WITH ")
            SQLStr.AppendLine("      INNER JOIN ")
            SQLStr.AppendLine("        MB001_STAFF ")
            SQLStr.AppendLine("        ON ")
            SQLStr.AppendLine("          ORGLIST_WITH.CAMPCODE = MB001_STAFF.CAMPCODE ")
            SQLStr.AppendLine("          AND ")
            SQLStr.AppendLine("          ORGLIST_WITH.ORGCODE = MB001_STAFF.HORG ")
            SQLStr.AppendLine("      INNER JOIN ")
            SQLStr.AppendLine("        (SELECT KEYCODE AS STAFFKBN FROM MC001_FIXVALUE ")
            SQLStr.AppendLine("          WHERE CAMPCODE = @P1 AND CLASS = 'WORKOVERSTAFFKBN' ")
            SQLStr.AppendLine("          AND DELFLG <> @P7 AND STYMD <= @P6 AND ENDYMD >= @P6) MANAGEMENT_STAFF ")
            SQLStr.AppendLine("        ON ")
            SQLStr.AppendLine("          MB001_STAFF.STAFFKBN = MANAGEMENT_STAFF.STAFFKBN ")
            SQLStr.AppendLine("    WHERE ")
            SQLStr.AppendLine("      MB001_STAFF.DELFLG <> @P7 AND MB001_STAFF.STYMD <= @P5 AND MB001_STAFF.ENDYMD >= @P5 ")
            SQLStr.AppendLine("  ) ")
            SQLStr.AppendLine("SELECT ")
            SQLStr.AppendLine("  STAFF_WITH.ORGCODE, ")
            SQLStr.AppendLine("  STAFF_WITH.ORGNAME, ")
            SQLStr.AppendLine("  STAFF_WITH.ORGSEQ, ")
            SQLStr.AppendLine("  STAFF_WITH.STAFFCODE, ")
            SQLStr.AppendLine("  STAFF_WITH.STAFFNAME, ")
            SQLStr.AppendLine("  T0007_KINTAI.TAISHOYM, ")
            SQLStr.AppendLine("  SUM( ")
            SQLStr.AppendLine("    T0007_KINTAI.ORVERTIME + ")
            SQLStr.AppendLine("    T0007_KINTAI.ORVERTIMECHO + ")
            SQLStr.AppendLine("    T0007_KINTAI.WNIGHTTIME + ")
            SQLStr.AppendLine("    T0007_KINTAI.WNIGHTTIMECHO ")
            SQLStr.AppendLine("  ) AS OVERTIME, ")
            SQLStr.AppendLine("  SUM( ")
            SQLStr.AppendLine("    T0007_KINTAI.ORVERTIME + ")
            SQLStr.AppendLine("    T0007_KINTAI.ORVERTIMECHO + ")
            SQLStr.AppendLine("    T0007_KINTAI.WNIGHTTIME + ")
            SQLStr.AppendLine("    T0007_KINTAI.WNIGHTTIMECHO + ")
            SQLStr.AppendLine("    T0007_KINTAI.SWORKTIME + ")
            SQLStr.AppendLine("    T0007_KINTAI.SWORKTIMECHO + ")
            SQLStr.AppendLine("    T0007_KINTAI.SNIGHTTIME + ")
            SQLStr.AppendLine("    T0007_KINTAI.SNIGHTTIMECHO + ")
            SQLStr.AppendLine("    T0007_KINTAI.HWORKTIME + ")
            SQLStr.AppendLine("    T0007_KINTAI.HWORKTIMECHO + ")
            SQLStr.AppendLine("    T0007_KINTAI.HNIGHTTIME + ")
            SQLStr.AppendLine("    T0007_KINTAI.HNIGHTTIMECHO ")
            SQLStr.AppendLine("  ) AS OVERTIMEWITHHOLIDAY ")
            SQLStr.AppendLine("FROM ")
            SQLStr.AppendLine("  T0007_KINTAI ")
            SQLStr.AppendLine("  INNER JOIN ")
            SQLStr.AppendLine("    STAFF_WITH ")
            SQLStr.AppendLine("    ON ")
            SQLStr.AppendLine("      T0007_KINTAI.CAMPCODE = STAFF_WITH.CAMPCODE ")
            SQLStr.AppendLine("      AND ")
            SQLStr.AppendLine("      T0007_KINTAI.STAFFCODE = STAFF_WITH.STAFFCODE ")
            SQLStr.AppendLine("WHERE ")
            SQLStr.AppendLine("  T0007_KINTAI.TAISHOYM BETWEEN FORMAT(@P4,'yyyy/MM') AND FORMAT(@P5,'yyyy/MM') ")
            SQLStr.AppendLine("  AND T0007_KINTAI.HDKBN = 'H' AND T0007_KINTAI.RECODEKBN = '2' AND T0007_KINTAI.DELFLG <> @P7 ")
            SQLStr.AppendLine("GROUP BY ")
            SQLStr.AppendLine("  STAFF_WITH.ORGCODE, ")
            SQLStr.AppendLine("  STAFF_WITH.ORGNAME, ")
            SQLStr.AppendLine("  STAFF_WITH.ORGSEQ, ")
            SQLStr.AppendLine("  STAFF_WITH.STAFFCODE, ")
            SQLStr.AppendLine("  STAFF_WITH.STAFFNAME, ")
            SQLStr.AppendLine("  T0007_KINTAI.TAISHOYM ")
            SQLStr.AppendLine("ORDER BY ")
            SQLStr.AppendLine("  STAFF_WITH.ORGSEQ, STAFF_WITH.STAFFCODE ")

            Try
                'DataBase接続
                SQLcon.Open()

                Using SQLcmd As SqlCommand = New SqlCommand(SQLStr.ToString, SQLcon)
                    Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P1", SqlDbType.NVarChar, 20)
                    Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P2", SqlDbType.NVarChar, 20)
                    Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@P3", SqlDbType.NVarChar, 20)
                    Dim PARA04 As SqlParameter = SQLcmd.Parameters.Add("@P4", SqlDbType.Date)
                    Dim PARA05 As SqlParameter = SQLcmd.Parameters.Add("@P5", SqlDbType.Date)
                    Dim PARA06 As SqlParameter = SQLcmd.Parameters.Add("@P6", SqlDbType.Date)
                    Dim PARA07 As SqlParameter = SQLcmd.Parameters.Add("@P7", SqlDbType.NVarChar, 20)
                    Dim PARA08 As SqlParameter = SQLcmd.Parameters.Add("@P8", SqlDbType.NVarChar, 20)
                    Dim PARA09 As SqlParameter = SQLcmd.Parameters.Add("@P9", SqlDbType.NVarChar, 20)

                    PARA01.Value = campCode
                    PARA02.Value = C_ROLE_VARIANT.USER_ORG
                    PARA03.Value = "管轄組織"
                    PARA04.Value = beginDate
                    PARA05.Value = endDate
                    PARA06.Value = Date.Now
                    PARA07.Value = "1"
                    PARA08.Value = orgCode
                    PARA09.Value = loginUserOrgCode

                    SQLcmd.CommandTimeout = 300

                    Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                        ' 結果読み込み
                        Items = ReferDB(Of Item).ReadAll(SQLdr)
                    End Using
                End Using
            Catch ex As Exception
                Throw ex
            End Try

        End Sub

    End Class

    ''' <summary>
    ''' 時間外労働データ表示用クラス（Excelと共通）
    ''' </summary>
    Public Class ViewWorkOverData
        Inherits ClassTable(Of Item)

        Public Class Item
            Inherits BaseItem(Of Item)

            ' 共通項目（固定フィールド）
            <Sort(0)>
            Public Property LINECNT As Integer
            <Sort(1)>
            Public Property OPERATION As String
            <Sort(2)>
            Public Property TIMSTP As String
            <Sort(3)>
            Public Property [SELECT] As Integer = 1
            <Sort(4)>
            Public Property HIDDEN As Integer = 0

            ' 固有項目
            <Sort(5)>
            Public Property StaffCode As String
            <Sort(6)>
            Public Property StaffName As String
            <Sort(7)>
            Public Property OrgCode As String
            <Sort(8)>
            Public Property OrgName As String
            <Sort(9)>
            Public Property OrgSeq As Integer

            Public Property AnnualTotalTime As String
            Public Property ExcessRemainingCount As String
            Public Property PaceStatus As String
            Public Property MonthMaxTimeExceededStatusWithHoliday As String
            Public Property MonthsAvgMaxTimeExceededStatusWithHoliday As String
            Public Property AverageTimeWithHoliday As String

            Public Property AprilOvertime As String
            Public Property AprilCoaching As String
            Public Property MayOvertime As String
            Public Property MayCoaching As String
            Public Property JuneOvertime As String
            Public Property JuneCoaching As String
            Public Property JulyOvertime As String
            Public Property JulyCoaching As String
            Public Property AugustOvertime As String
            Public Property AugustCoaching As String
            Public Property SeptemberOvertime As String
            Public Property SeptemberCoaching As String
            Public Property OctoberOvertime As String
            Public Property OctoberCoaching As String
            Public Property NovemberOvertime As String
            Public Property NovemberCoaching As String
            Public Property DecemberOvertime As String
            Public Property DecemberCoaching As String
            Public Property JanuaryOvertime As String
            Public Property JanuaryCoaching As String
            Public Property FebruaryOvertime As String
            Public Property FebruaryCoaching As String
            Public Property MarchOvertime As String
            Public Property MarchCoaching As String

            Public Property AprilOvertimeWithHoliday As String
            Public Property MayOvertimeWithHoliday As String
            Public Property JuneOvertimeWithHoliday As String
            Public Property JulyOvertimeWithHoliday As String
            Public Property AugustOvertimeWithHoliday As String
            Public Property SeptemberOvertimeWithHoliday As String
            Public Property OctoberOvertimeWithHoliday As String
            Public Property NovemberOvertimeWithHoliday As String
            Public Property DecemberOvertimeWithHoliday As String
            Public Property JanuaryOvertimeWithHoliday As String
            Public Property FebruaryOvertimeWithHoliday As String
            Public Property MarchOvertimeWithHoliday As String

            Public Property MonthMaxTimeExceededCountWithHoliday As String
            Public Property MonthPrincipleTimeExceededCount As String
            Public Property MonthAvgMaxTimeExceededCountWithHoliday As String

            Public Property MonthsAvgMaxTimeExceededCount As Integer
            Public Property MonthsAvgWarnTimeExceededCount As Integer

            ' 表示用フラグ
            Public Property VF_AnnualTotalTime As Integer
            Public Property VF_ExcessRemainingCount As Integer
            Public Property VF_PaceStatus As Integer
            Public Property VF_MonthMaxTimeExceededStatusWithHoliday As Integer
            Public Property VF_MonthsAvgMaxTimeExceededStatusWithHoliday As Integer
            Public Property VF_AverageTimeWithHoliday As Integer

            Public Property VF_AprilOvertime As Integer
            Public Property VF_MayOvertime As Integer
            Public Property VF_JuneOvertime As Integer
            Public Property VF_JulyOvertime As Integer
            Public Property VF_AugustOvertime As Integer
            Public Property VF_SeptemberOvertime As Integer
            Public Property VF_OctoberOvertime As Integer
            Public Property VF_NovemberOvertime As Integer
            Public Property VF_DecemberOvertime As Integer
            Public Property VF_JanuaryOvertime As Integer
            Public Property VF_FebruaryOvertime As Integer
            Public Property VF_MarchOvertime As Integer

            Public Property VF_MonthMaxTimeExceededCountWithHoliday As Integer
            Public Property VF_MonthPrincipleTimeExceededCount As Integer
            Public Property VF_MonthAvgMaxTimeExceededCountWithHoliday As Integer

            ' Excel用
            Public Property Year As Integer
            Public Property Month As Integer
            Public Property SelectOrgName As String

        End Class

        ''' <summary>
        ''' 表示データと表示フラグのプロパティペアを作成
        ''' </summary>
        ''' <returns></returns>
        Public Function GetPropViewFlgPairs() As Dictionary(Of PropertyInfo, PropertyInfo)
            Dim rtn As New Dictionary(Of PropertyInfo, PropertyInfo)

            ' プロパティリスト取得
            Dim propInfoList As List(Of PropertyInfo) = GetType(Item).GetProperties().ToList()
            If IsNothing(propInfoList) OrElse Not propInfoList.Any() Then Return Nothing

            For Each propInfo As PropertyInfo In propInfoList
                ' プロパティ検索
                Dim findResult As PropertyInfo = propInfoList.Find(Function(x) x.Name.ToUpper().Equals("VF_" & propInfo.Name.ToUpper()))
                If IsNothing(findResult) Then Continue For
                rtn.Add(propInfo, findResult)
            Next
            Return rtn
        End Function

    End Class

    ''' <summary>
    ''' セレクタ
    ''' </summary>
    Public Class Selector
        Inherits ClassTable(Of Item)

        Public Const ALL_SELECT_CODE As String = "00000"
        Public Const ALL_SELECT_NAME As String = "全て"

        Public Class Item
            Inherits BaseItem(Of Item)

            Public Property Code As String
            Public Property Name As String
            Public Property Seq As Integer

            Sub New()
                Code = ""
                Name = ""
                Seq = 0
            End Sub

            Public Overrides Function GetHashCode() As Integer
                Return Code.GetHashCode() And Name.GetHashCode And Seq.GetHashCode()
            End Function

            Public Function GetKeyHashCode() As Integer
                Return Code.GetHashCode()
            End Function

        End Class

        Public Class ItemKeyComparator
            Implements IEqualityComparer(Of Item)

            Public Function IEqualityComparer_Equals(x As Item, y As Item) As Boolean Implements IEqualityComparer(Of Item).Equals
                Return x.GetKeyHashCode = y.GetKeyHashCode()
            End Function

            Public Function IEqualityComparer_GetHashCode(obj As Item) As Integer Implements IEqualityComparer(Of Item).GetHashCode
                Return obj.GetKeyHashCode()
            End Function
        End Class

    End Class

#Region "# 共通"

    ''' <summary>
    ''' アイテムクラスのベース（インターフェース）
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    Public Interface IBaseItem(Of T As {Class})
        Function GetItemPropertys() As IEnumerable(Of PropertyInfo)
    End Interface

    ''' <summary>
    ''' アイテムクラスのベース（実装）
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    Public MustInherit Class BaseItem(Of T As {Class})
        Implements IBaseItem(Of T)

        Public Function GetItemPropertys() As IEnumerable(Of PropertyInfo) Implements IBaseItem(Of T).GetItemPropertys
            Dim rtn As New List(Of PropertyInfo)
            ' 全量取得
            Dim allItems As IEnumerable(Of PropertyInfo) = GetType(T).GetProperties
            If Not IsNothing(allItems) AndAlso allItems.Any() Then
                ' ソート可能なもののみソートして取得
                Dim sorteditems As IEnumerable(Of PropertyInfo) = allItems.
                    Where(Function(x) Attribute.IsDefined(x, GetType(SortAttribute))).
                    OrderBy(Function(x) CType(Attribute.GetCustomAttribute(x, GetType(SortAttribute)), SortAttribute).SortIndex)
                If Not IsNothing(sorteditems) AndAlso sorteditems.Any() Then
                    rtn.AddRange(sorteditems)
                End If
                ' ソート不可なもののみ取得
                Dim notsorteditems As IEnumerable(Of PropertyInfo) = allItems.
                    Where(Function(x) Not Attribute.IsDefined(x, GetType(SortAttribute)))
                If Not IsNothing(notsorteditems) AndAlso notsorteditems.Any() Then
                    rtn.AddRange(notsorteditems)
                End If
            End If
            Return rtn
        End Function
    End Class

    ''' <summary>
    ''' ClassとDataTableを紐づける基底クラス（インターフェース）
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    Public Interface IClassTable(Of T As {Class, New, IBaseItem(Of T)})
        Property Items As List(Of T)
        Function CopyToDataTable(Optional ByVal rFunc As Func(Of T, T) = Nothing, Optional ByVal fFunc As Func(Of List(Of T), List(Of T)) = Nothing) As DataTable
        Sub SetTable(ByVal tbl As DataTable)
    End Interface

    ''' <summary>
    ''' ClassとDataTableを紐づける基底クラス（実装）
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    Public MustInherit Class ClassTable(Of T As {Class, New, IBaseItem(Of T)})
        Implements IClassTable(Of T)

        ''' <summary>
        ''' データ格納用
        ''' </summary>
        ''' <returns></returns>
        Public Property Items As New List(Of T) Implements IClassTable(Of T).Items

        ''' <summary>
        ''' データテーブルへ変換
        ''' </summary>
        ''' <returns></returns>
        Public Function CopyToDataTable(Optional ByVal rFunc As Func(Of T, T) = Nothing, Optional ByVal fFunc As Func(Of List(Of T), List(Of T)) = Nothing) As DataTable Implements IClassTable(Of T).CopyToDataTable

            Dim workTbl As DataTable = New DataTable

            ' プロパティリスト取得
            Dim item As New T
            Dim propInfoList As IEnumerable(Of PropertyInfo) = item.GetItemPropertys()
            If IsNothing(propInfoList) OrElse Not propInfoList.Any() Then Return Nothing

            ' 列設定
            For Each propInfo As PropertyInfo In propInfoList
                ' プロパティの型取得
                Dim propType As Type = IIf(propInfo.PropertyType.IsGenericType, Nullable.GetUnderlyingType(propInfo.PropertyType), propInfo.PropertyType)
                workTbl.Columns.Add(propInfo.Name.ToUpper(), propType)
            Next

            If IsNothing(Items) OrElse Not Items.Any() Then Return workTbl

            ' 明細行
            For Each tItem As T In Items
                Dim row As DataRow = workTbl.NewRow()
                ' 行へ設定
                If Not IsNothing(rFunc) Then
                    ' 行毎に集計しつつ設定
                    Dim rFuncResult As T = rFunc(tItem)
                    row.ItemArray = propInfoList.Select(Function(x) x.GetValue(rFuncResult)).ToArray()
                Else
                    ' そのまま設定
                    row.ItemArray = propInfoList.Select(Function(x) x.GetValue(tItem)).ToArray()
                End If
                ' データテーブルへ設定
                workTbl.Rows.Add(row)
            Next

            ' 合計行
            If Not IsNothing(fFunc) Then
                ' 集計
                Dim fList As List(Of T) = fFunc(Items)
                For Each fItem As T In fList
                    Dim row As DataRow = workTbl.NewRow()
                    ' データ設定
                    row.ItemArray = propInfoList.Select(Function(x) x.GetValue(fItem)).ToArray()
                    ' データテーブルへ設定
                    workTbl.Rows.Add(row)
                Next
            End If

            Return workTbl
        End Function

        ''' <summary>
        ''' データテーブルから設定
        ''' </summary>
        ''' <param name="tbl"></param>
        Public Sub SetTable(ByVal tbl As DataTable) Implements IClassTable(Of T).SetTable

            ' カラム名リスト取得
            Dim colNames As List(Of String) = tbl.Columns.Cast(Of DataColumn).Select(Function(x) x.ColumnName.ToUpper()).ToList()

            ' プロパティリスト取得
            Dim colIndex As Integer
            Dim propInfo As PropertyInfo
            Dim propType As Type
            Dim propInfoQuery = GetType(T).GetProperties().Where(Function(x) colNames.Contains(x.Name.ToUpper())).
                    Select(Function(x)
                               colIndex = colNames.IndexOf(x.Name.ToUpper())
                               propInfo = x
                               propType = IIf(x.PropertyType.IsGenericType, Nullable.GetUnderlyingType(x.PropertyType), x.PropertyType)
                               Return New With {colIndex, propInfo, propType}
                           End Function)

            If IsNothing(propInfoQuery) OrElse Not propInfoQuery.Any() Then Exit Sub

            ' データ設定
            Items = tbl.Rows.Cast(Of DataRow).
                    Select(Function(row)
                               Dim tIns As New T
                               For Each piq In propInfoQuery
                                   ' データが取得できない場合はSKIP
                                   If IsNothing(row(piq.colIndex)) Then Continue For
                                   ' プロパティへ設定
                                   piq.propInfo.SetValue(tIns, Convert.ChangeType(row(piq.colIndex), piq.propType))
                               Next
                               Return tIns
                           End Function).ToList()

        End Sub

    End Class

    ''' <summary>
    ''' DB取得用クラス
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    Public Class ReferDB(Of T As {Class, New, IBaseItem(Of T)})

        ''' <summary>
        ''' DB取得値をクラスとして取得
        ''' </summary>
        ''' <param name="SQLdr">SQL取得結果</param>
        ''' <param name="keyColumnName">指定クラスのプロパティ名とマッチさせる列名</param>
        ''' <param name="valueColumnName">指定クラスのプロパティ名とマッチした行で列名</param>
        Public Shared Function ReadAll(ByVal SQLdr As SqlDataReader,
                                       Optional ByVal keyColumnName As String = Nothing,
                                       Optional ByVal valueColumnName As String = Nothing) As IEnumerable(Of T)

            ' 返却値格納用
            Dim rtn As New List(Of T)

            ' プロパティリスト取得
            Dim propInfoList As List(Of PropertyInfo) = GetType(T).GetProperties().ToList()
            If IsNothing(propInfoList) OrElse Not propInfoList.Any() Then Return Nothing

            ' データ取得
            If String.IsNullOrWhiteSpace(keyColumnName) AndAlso String.IsNullOrWhiteSpace(valueColumnName) Then
                ' 列別読み込み
                While SQLdr.Read
                    Dim tIns As T = New T()
                    For col = 0 To SQLdr.FieldCount - 1
                        ' データが取得できない場合はSKIP
                        If SQLdr.IsDBNull(col) Then Continue While
                        ' 名称取得
                        Dim propName As String = SQLdr.GetName(col)
                        ' プロパティ検索
                        Dim propInfo As PropertyInfo = propInfoList.Find(Function(x) x.Name.ToUpper().Equals(propName.ToUpper()))
                        If IsNothing(propInfo) Then Continue For
                        ' プロパティの型取得
                        Dim propType As Type = IIf(propInfo.PropertyType.IsGenericType, Nullable.GetUnderlyingType(propInfo.PropertyType), propInfo.PropertyType)
                        ' プロパティへ設定
                        propInfo.SetValue(tIns, Convert.ChangeType(SQLdr(col), propType))
                    Next
                    rtn.Add(tIns)
                End While
            Else
                ' ##### KeyValue読み込み #####

                ' KeyValueName取得（取得できない場合は規定値）
                Dim keyName As String = keyColumnName
                Dim ValueName As String = valueColumnName
                If String.IsNullOrEmpty(keyName) Then
                    keyName = "Code"
                End If
                If String.IsNullOrEmpty(ValueName) Then
                    ValueName = "Value"
                End If

                ' 行読み込み
                Dim tIns As T = New T()
                While SQLdr.Read
                    ' データが取得できない場合はSKIP
                    If IsDBNull(SQLdr(keyName)) Then Continue While
                    If IsDBNull(SQLdr(ValueName)) Then Continue While
                    ' 名称取得
                    Dim propName As String = CStr(SQLdr(keyName))
                    ' プロパティ検索
                    Dim propInfo As PropertyInfo = propInfoList.Find(Function(x) x.Name.ToUpper().Equals(propName.ToUpper()))
                    If IsNothing(propInfo) Then Continue While
                    ' プロパティの型取得
                    Dim propType As Type = IIf(propInfo.PropertyType.IsGenericType, Nullable.GetUnderlyingType(propInfo.PropertyType), propInfo.PropertyType)
                    ' プロパティへ設定
                    propInfo.SetValue(tIns, Convert.ChangeType(SQLdr(ValueName), propType))
                End While
                rtn.Add(tIns)
            End If

            Return rtn
        End Function

    End Class

    ''' <summary>
    ''' プロパティ列挙順定義用
    ''' </summary>
    <AttributeUsage(AttributeTargets.Property, AllowMultiple:=False, Inherited:=True)>
    Public Class SortAttribute
        Inherits Attribute
        Public ReadOnly Property SortIndex As Integer
        Sub New(ByVal Index As Integer)
            SortIndex = Index
        End Sub
    End Class

#End Region

End Class