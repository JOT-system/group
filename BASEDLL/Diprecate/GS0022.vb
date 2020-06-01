Imports System.Web
Imports System.Data.SqlClient
Imports System
Imports System.IO
Imports System.Text
Imports System.Globalization
Imports Microsoft.VisualBasic
Imports System.Web.UI
Imports System.Web.UI.WebControls
Imports System.Web.UI.Control
Imports Microsoft.Office
Imports Microsoft.Office.Interop
Imports Microsoft.Office.Core
Imports System.Net

''' <summary>
''' Leftボックス用届先取得
''' </summary>
''' <remarks>受注配車用</remarks>
Public Class GS0022TODOKESRVget
    Inherits GS0000
    ''' <summary>
    ''' ユーザID
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property USERID As String
    ''' <summary>
    ''' 会社コード
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property CAMPCODE() As String
    ''' <summary>
    ''' 部署コード
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property ORG() As String
    ''' <summary>
    ''' 取引先CODE
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property TORICODE() As String
    ''' <summary>
    ''' 分類
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property CLAS() As String
    ''' <summary>
    ''' 出庫日
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property SHUKODATE() As Date
    ''' <summary>
    ''' 届先CODE一覧
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property TODOKECODE() As List(Of String)
    ''' <summary>
    ''' 届先名称一覧
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property TODOKECODENAME() As List(Of String)
    ''' <summary>
    ''' 住所一覧
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property ADDR() As List(Of String)
    ''' <summary>
    ''' :特定要件1
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property NOTES1() As List(Of String)
    ''' <summary>
    ''' :特定要件2
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property NOTES2() As List(Of String)
    ''' <summary>
    ''' :特定要件3
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property NOTES3() As List(Of String)
    ''' <summary>
    ''' :特定要件4
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property NOTES4() As List(Of String)
    ''' <summary>
    ''' :特定要件5
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property NOTES5() As List(Of String)
    ''' <summary>
    ''' :特定要件6
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property NOTES6() As List(Of String)
    ''' <summary>
    ''' :特定要件7
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property NOTES7() As List(Of String)
    ''' <summary>
    ''' :特定要件8
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property NOTES8() As List(Of String)
    ''' <summary>
    ''' :特定要件9
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property NOTES9() As List(Of String)
    ''' <summary>
    ''' :特定要件10
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property NOTES10() As List(Of String)
    ''' <summary>
    ''' 所要時間
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property ARRIVTIME() As List(Of String)
    ''' <summary>
    ''' 配送距離（配車用）
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property DISTANCE() As List(Of String)
    ''' <summary>
    ''' 届先情報一覧
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property LISTBOX() As ListBox

    Protected METHOD_NAME As String = "GS0022TODOKESRVget"
    ''' <summary>
    ''' 届先取得
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub GS0022TODOKESRVget()
        '<< エラー説明 >>
        'ERR = OK:00000,ERR:00002(環境エラー),ERR:00003(DBerr)
        '●初期処理
        TODOKECODE = New List(Of String)
        TODOKECODENAME = New List(Of String)
        ADDR = New List(Of String)
        NOTES1 = New List(Of String)
        NOTES2 = New List(Of String)
        NOTES3 = New List(Of String)
        NOTES4 = New List(Of String)
        NOTES5 = New List(Of String)
        NOTES6 = New List(Of String)
        NOTES7 = New List(Of String)
        NOTES8 = New List(Of String)
        NOTES9 = New List(Of String)
        NOTES10 = New List(Of String)
        ARRIVTIME = New List(Of String)
        DISTANCE = New List(Of String)
        'セッション制御宣言
        Dim sm As New CS0050SESSION
        'PARAM EXTRA01: USERID
        If IsNothing(USERID) Then
            USERID = sm.USERID
        End If
        'PARAM EXTRA02: CLAS
        If IsNothing(CLAS) Then
            CLAS = ""
        End If
        'PARAM EXTRA03: TORICODE
        If IsNothing(TORICODE) Then
            TORICODE = ""
        End If
        'PARAM EXTRA04: ORG
        If ORG = "" Or IsNothing(ORG) Then
            ORG = sm.APSV_ORG
        End If
        'PARAM EXTRA05: SHUKODATE
        If SHUKODATE < C_DEFAULT_YMD Then
            SHUKODATE = Date.Now
        End If
        Try
            If IsNothing(LISTBOX) Then
                LISTBOX = New ListBox
            Else
                CType(LISTBOX, ListBox).Items.Clear()
            End If
        Catch ex As Exception
        End Try

        'DataBase接続文字
        Dim SQLcon = sm.getConnection
        SQLcon.Open() 'DataBase接続(Open)
        Dim SQLcmd As SqlCommand = Nothing
        '●Leftボックス用届先取得（APSRVOrg）
        Try
            If CLAS = "" Then
                If TORICODE = "" Then
                    '○ セッション変数（APSRVOrg）に紐付くデータ取得
                    '   [分類]に値が存在しない場合、かつ、[取引先コード]に値が存在しない場合
                    Dim SQLStr As String = _
                                "       SELECT isnull(rtrim(B.TODOKECODE),'')    as TODOKECODE , " _
                            & "              isnull(rtrim(B.NAMES),'')         as NAMES ,      " _
                            & "              isnull(rtrim(B.ADDR1),'') +                       " _
                            & "              isnull(rtrim(B.ADDR2),'') +                       " _
                            & "              isnull(rtrim(B.ADDR3),'') +                       " _
                            & "              isnull(rtrim(B.ADDR4),'')         as ADDR ,       " _
                            & "              isnull(rtrim(B.NOTES1),'')        as NOTES1 ,     " _
                            & "              isnull(rtrim(B.NOTES2),'')        as NOTES2 ,     " _
                            & "              isnull(rtrim(B.NOTES3),'')        as NOTES3 ,     " _
                            & "              isnull(rtrim(B.NOTES4),'')        as NOTES4 ,     " _
                            & "              isnull(rtrim(B.NOTES5),'')        as NOTES5 ,     " _
                            & "              isnull(rtrim(B.NOTES6),'')        as NOTES6 ,     " _
                            & "              isnull(rtrim(B.NOTES7),'')        as NOTES7 ,     " _
                            & "              isnull(rtrim(B.NOTES8),'')        as NOTES8 ,     " _
                            & "              isnull(rtrim(B.NOTES9),'')        as NOTES9 ,     " _
                            & "              isnull(rtrim(B.NOTES10),'')       as NOTES10 ,    " _
                            & "              rtrim(A.ARRIVTIME)                as ARRIVTIME ,  " _
                            & "              isnull(rtrim(A.DISTANCE),'')      as DISTANCE     " _
                            & "         FROM MC007_TODKORG      as A                " _
                            & "   INNER JOIN MC006_TODOKESAKI   as B                " _
                            & "           ON B.CAMPCODE     = A.CAMPCODE            " _
                            & "          and B.TORICODE     = A.TORICODE            " _
                            & "          and B.TODOKECODE   = A.TODOKECODE          " _
                            & "          and B.STYMD       <= @P1                   " _
                            & "          and B.ENDYMD      >= @P1                   " _
                            & "          and B.DELFLG      <> '1'                   " _
                            & "        Where A.CAMPCODE     = @P2                   " _
                            & "          and A.UORG         = @P3                   " _
                            & "          and A.DELFLG      <> '1'                   " _
                            & "     GROUP BY A.SEQ ,  			                    " _
                            & "              B.TODOKECODE ,                         " _
                            & "              B.NAMES ,                              " _
                            & "              B.ADDR1 ,                              " _
                            & "              B.ADDR2 ,                              " _
                            & "              B.ADDR3 ,                              " _
                            & "              B.ADDR4 ,                              " _
                            & "              B.NOTES1 ,                             " _
                            & "              B.NOTES2 ,                             " _
                            & "              B.NOTES3 ,                             " _
                            & "              B.NOTES4 ,                             " _
                            & "              B.NOTES5 ,                             " _
                            & "              B.NOTES6 ,                             " _
                            & "              B.NOTES7 ,                             " _
                            & "              B.NOTES8 ,                             " _
                            & "              B.NOTES9 ,                             " _
                            & "              B.NOTES10 ,                            " _
                            & "              A.ARRIVTIME ,                          " _
                            & "              A.DISTANCE                             " _
                            & "     ORDER BY A.SEQ ,  			                    " _
                            & "              B.TODOKECODE ,                         " _
                            & "              B.NAMES ,                              " _
                            & "              B.ADDR1 ,                              " _
                            & "              B.ADDR2 ,                              " _
                            & "              B.ADDR3 ,                              " _
                            & "              B.ADDR4 ,                              " _
                            & "              B.NOTES1 ,                             " _
                            & "              B.NOTES2 ,                             " _
                            & "              B.NOTES3 ,                             " _
                            & "              B.NOTES4 ,                             " _
                            & "              B.NOTES5 ,                             " _
                            & "              B.NOTES6 ,                             " _
                            & "              B.NOTES7 ,                             " _
                            & "              B.NOTES8 ,                             " _
                            & "              B.NOTES9 ,                             " _
                            & "              B.NOTES10 ,                            " _
                            & "              A.ARRIVTIME ,                          " _
                            & "              A.DISTANCE                             "

                    SQLcmd = New SqlCommand(SQLStr, SQLcon)
                    Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.Date)
                    Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.NVarChar, 20)
                    Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.NVarChar, 20)

                    PARA1.Value = SHUKODATE
                    PARA2.Value = CAMPCODE
                    PARA3.Value = ORG

                Else
                    '○ セッション変数（APSRVOrg）に紐付くデータ取得
                    '   [分類]に値が存在しない場合、かつ、[取引先コード]に値が存在する場合
                    Dim SQLStr As String = _
                                "       SELECT isnull(rtrim(B.TODOKECODE),'')    as TODOKECODE , " _
                            & "              isnull(rtrim(B.NAMES),'')         as NAMES ,      " _
                            & "              isnull(rtrim(B.ADDR1),'') +                       " _
                            & "              isnull(rtrim(B.ADDR2),'') +                       " _
                            & "              isnull(rtrim(B.ADDR3),'') +                       " _
                            & "              isnull(rtrim(B.ADDR4),'')         as ADDR ,       " _
                            & "              isnull(rtrim(B.NOTES1),'')        as NOTES1 ,     " _
                            & "              isnull(rtrim(B.NOTES2),'')        as NOTES2 ,     " _
                            & "              isnull(rtrim(B.NOTES3),'')        as NOTES3 ,     " _
                            & "              isnull(rtrim(B.NOTES4),'')        as NOTES4 ,     " _
                            & "              isnull(rtrim(B.NOTES5),'')        as NOTES5 ,     " _
                            & "              isnull(rtrim(B.NOTES6),'')        as NOTES6 ,     " _
                            & "              isnull(rtrim(B.NOTES7),'')        as NOTES7 ,     " _
                            & "              isnull(rtrim(B.NOTES8),'')        as NOTES8 ,     " _
                            & "              isnull(rtrim(B.NOTES9),'')        as NOTES9 ,     " _
                            & "              isnull(rtrim(B.NOTES10),'')       as NOTES10 ,    " _
                            & "              rtrim(A.ARRIVTIME)                as ARRIVTIME ,  " _
                            & "              isnull(rtrim(A.DISTANCE),'')      as DISTANCE     " _
                            & "         FROM MC007_TODKORG      as A                " _
                            & "   INNER JOIN MC006_TODOKESAKI   as B                " _
                            & "           ON B.CAMPCODE     = A.CAMPCODE            " _
                            & "          and B.TORICODE     = A.TORICODE            " _
                            & "          and B.TODOKECODE   = A.TODOKECODE          " _
                            & "          and B.STYMD       <= @P1                   " _
                            & "          and B.ENDYMD      >= @P1                   " _
                            & "          and B.DELFLG      <> '1'                   " _
                            & "        Where A.CAMPCODE     = @P2                   " _
                            & "          and A.TORICODE     = @P4                   " _
                            & "          and A.UORG         = @P3                   " _
                            & "          and A.DELFLG      <> '1'                   " _
                            & "     GROUP BY A.SEQ ,  			                    " _
                            & "              B.TODOKECODE ,                         " _
                            & "              B.NAMES ,                              " _
                            & "              B.ADDR1 ,                              " _
                            & "              B.ADDR2 ,                              " _
                            & "              B.ADDR3 ,                              " _
                            & "              B.ADDR4 ,                              " _
                            & "              B.NOTES1 ,                             " _
                            & "              B.NOTES2 ,                             " _
                            & "              B.NOTES3 ,                             " _
                            & "              B.NOTES4 ,                             " _
                            & "              B.NOTES5 ,                             " _
                            & "              B.NOTES6 ,                             " _
                            & "              B.NOTES7 ,                             " _
                            & "              B.NOTES8 ,                             " _
                            & "              B.NOTES9 ,                             " _
                            & "              B.NOTES10 ,                            " _
                            & "              A.ARRIVTIME ,                          " _
                            & "              A.DISTANCE                             " _
                            & "     ORDER BY A.SEQ ,  			                    " _
                            & "              B.TODOKECODE ,                         " _
                            & "              B.NAMES ,                              " _
                            & "              B.ADDR1 ,                              " _
                            & "              B.ADDR2 ,                              " _
                            & "              B.ADDR3 ,                              " _
                            & "              B.ADDR4 ,                              " _
                            & "              B.NOTES1 ,                             " _
                            & "              B.NOTES2 ,                             " _
                            & "              B.NOTES3 ,                             " _
                            & "              B.NOTES4 ,                             " _
                            & "              B.NOTES5 ,                             " _
                            & "              B.NOTES6 ,                             " _
                            & "              B.NOTES7 ,                             " _
                            & "              B.NOTES8 ,                             " _
                            & "              B.NOTES9 ,                             " _
                            & "              B.NOTES10 ,                            " _
                            & "              A.ARRIVTIME ,                          " _
                            & "              A.DISTANCE                             "

                    SQLcmd = New SqlCommand(SQLStr, SQLcon)
                    Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.Date)
                    Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.NVarChar, 20)
                    Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.NVarChar, 20)
                    Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", System.Data.SqlDbType.NVarChar, 20)

                    PARA1.Value = SHUKODATE
                    PARA2.Value = CAMPCODE
                    PARA3.Value = ORG
                    PARA4.Value = TORICODE
                End If
            Else
                If TORICODE = "" Then
                    '○ セッション変数（APSRVOrg）に紐付くデータ取得
                    '   [分類]に値が存在する場合、かつ、[取引先コード]に値が存在しない場合
                    Dim SQLStr As String = _
                                "       SELECT isnull(rtrim(B.TODOKECODE),'')    as TODOKECODE , " _
                            & "              isnull(rtrim(B.NAMES),'')         as NAMES ,      " _
                            & "              isnull(rtrim(B.ADDR1),'') +                       " _
                            & "              isnull(rtrim(B.ADDR2),'') +                       " _
                            & "              isnull(rtrim(B.ADDR3),'') +                       " _
                            & "              isnull(rtrim(B.ADDR4),'')         as ADDR ,       " _
                            & "              isnull(rtrim(B.NOTES1),'')        as NOTES1 ,     " _
                            & "              isnull(rtrim(B.NOTES2),'')        as NOTES2 ,     " _
                            & "              isnull(rtrim(B.NOTES3),'')        as NOTES3 ,     " _
                            & "              isnull(rtrim(B.NOTES4),'')        as NOTES4 ,     " _
                            & "              isnull(rtrim(B.NOTES5),'')        as NOTES5 ,     " _
                            & "              isnull(rtrim(B.NOTES6),'')        as NOTES6 ,     " _
                            & "              isnull(rtrim(B.NOTES7),'')        as NOTES7 ,     " _
                            & "              isnull(rtrim(B.NOTES8),'')        as NOTES8 ,     " _
                            & "              isnull(rtrim(B.NOTES9),'')        as NOTES9 ,     " _
                            & "              isnull(rtrim(B.NOTES10),'')       as NOTES10 ,    " _
                            & "              rtrim(A.ARRIVTIME)                as ARRIVTIME ,  " _
                            & "              isnull(rtrim(A.DISTANCE),'')      as DISTANCE     " _
                            & "         FROM MC007_TODKORG      as A                " _
                            & "   INNER JOIN MC006_TODOKESAKI   as B                " _
                            & "           ON B.CAMPCODE     = A.CAMPCODE            " _
                            & "          and B.TORICODE     = A.TORICODE            " _
                            & "          and B.TODOKECODE   = A.TODOKECODE          " _
                            & "          and B.CLASS        = @P4                   " _
                            & "          and B.STYMD       <= @P1                   " _
                            & "          and B.ENDYMD      >= @P1                   " _
                            & "          and B.DELFLG      <> '1'                   " _
                            & "        Where A.CAMPCODE     = @P2                   " _
                            & "          and A.UORG         = @P3                   " _
                            & "          and A.DELFLG      <> '1'                   " _
                            & "     GROUP BY A.SEQ ,  			                    " _
                            & "              B.TODOKECODE ,                         " _
                            & "              B.NAMES ,                              " _
                            & "              B.ADDR1 ,                              " _
                            & "              B.ADDR2 ,                              " _
                            & "              B.ADDR3 ,                              " _
                            & "              B.ADDR4 ,                              " _
                            & "              B.NOTES1 ,                             " _
                            & "              B.NOTES2 ,                             " _
                            & "              B.NOTES3 ,                             " _
                            & "              B.NOTES4 ,                             " _
                            & "              B.NOTES5 ,                             " _
                            & "              B.NOTES6 ,                             " _
                            & "              B.NOTES7 ,                             " _
                            & "              B.NOTES8 ,                             " _
                            & "              B.NOTES9 ,                             " _
                            & "              B.NOTES10 ,                            " _
                            & "              A.ARRIVTIME ,                          " _
                            & "              A.DISTANCE                             " _
                            & "     ORDER BY A.SEQ ,  			                    " _
                            & "              B.TODOKECODE ,                         " _
                            & "              B.NAMES ,                              " _
                            & "              B.ADDR1 ,                              " _
                            & "              B.ADDR2 ,                              " _
                            & "              B.ADDR3 ,                              " _
                            & "              B.ADDR4 ,                              " _
                            & "              B.NOTES1 ,                             " _
                            & "              B.NOTES2 ,                             " _
                            & "              B.NOTES3 ,                             " _
                            & "              B.NOTES4 ,                             " _
                            & "              B.NOTES5 ,                             " _
                            & "              B.NOTES6 ,                             " _
                            & "              B.NOTES7 ,                             " _
                            & "              B.NOTES8 ,                             " _
                            & "              B.NOTES9 ,                             " _
                            & "              B.NOTES10 ,                            " _
                            & "              A.ARRIVTIME ,                          " _
                            & "              A.DISTANCE                             "

                    SQLcmd = New SqlCommand(SQLStr, SQLcon)
                    Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.Date)
                    Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.NVarChar, 20)
                    Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.NVarChar, 20)
                    Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", System.Data.SqlDbType.NVarChar, 1)
                    PARA1.Value = SHUKODATE
                    PARA2.Value = CAMPCODE
                    PARA3.Value = ORG
                    PARA4.Value = CLAS

                Else
                    '○ セッション変数（APSRVOrg）に紐付くデータ取得
                    '   [分類]に値が存在する場合、かつ、[取引先コード]に値が存在する場合
                    Dim SQLStr As String = _
                                "       SELECT isnull(rtrim(B.TODOKECODE),'')    as TODOKECODE , " _
                            & "              isnull(rtrim(B.NAMES),'')         as NAMES ,      " _
                            & "              isnull(rtrim(B.ADDR1),'') +                       " _
                            & "              isnull(rtrim(B.ADDR2),'') +                       " _
                            & "              isnull(rtrim(B.ADDR3),'') +                       " _
                            & "              isnull(rtrim(B.ADDR4),'')         as ADDR ,       " _
                            & "              isnull(rtrim(B.NOTES1),'')        as NOTES1 ,     " _
                            & "              isnull(rtrim(B.NOTES2),'')        as NOTES2 ,     " _
                            & "              isnull(rtrim(B.NOTES3),'')        as NOTES3 ,     " _
                            & "              isnull(rtrim(B.NOTES4),'')        as NOTES4 ,     " _
                            & "              isnull(rtrim(B.NOTES5),'')        as NOTES5 ,     " _
                            & "              isnull(rtrim(B.NOTES6),'')        as NOTES6 ,     " _
                            & "              isnull(rtrim(B.NOTES7),'')        as NOTES7 ,     " _
                            & "              isnull(rtrim(B.NOTES8),'')        as NOTES8 ,     " _
                            & "              isnull(rtrim(B.NOTES9),'')        as NOTES9 ,     " _
                            & "              isnull(rtrim(B.NOTES10),'')       as NOTES10 ,    " _
                            & "              rtrim(A.ARRIVTIME)                as ARRIVTIME ,  " _
                            & "              isnull(rtrim(A.DISTANCE),'')      as DISTANCE     " _
                            & "         FROM MC007_TODKORG      as A                " _
                            & "   INNER JOIN MC006_TODOKESAKI   as B                " _
                            & "           ON B.CAMPCODE     = A.CAMPCODE            " _
                            & "          and B.TORICODE     = A.TORICODE            " _
                            & "          and B.TODOKECODE   = A.TODOKECODE          " _
                            & "          and B.CLASS        = @P4                   " _
                            & "          and B.STYMD       <= @P1                   " _
                            & "          and B.ENDYMD      >= @P1                   " _
                            & "          and B.DELFLG      <> '1'                   " _
                            & "        Where A.CAMPCODE     = @P2                   " _
                            & "          and A.TORICODE     = @P5                   " _
                            & "          and A.UORG         = @P3                   " _
                            & "          and A.DELFLG      <> '1'                   " _
                            & "     GROUP BY A.SEQ ,  			                    " _
                            & "              B.TODOKECODE ,                         " _
                            & "              B.NAMES ,                              " _
                            & "              B.ADDR1 ,                              " _
                            & "              B.ADDR2 ,                              " _
                            & "              B.ADDR3 ,                              " _
                            & "              B.ADDR4 ,                              " _
                            & "              B.NOTES1 ,                             " _
                            & "              B.NOTES2 ,                             " _
                            & "              B.NOTES3 ,                             " _
                            & "              B.NOTES4 ,                             " _
                            & "              B.NOTES5 ,                             " _
                            & "              B.NOTES6 ,                             " _
                            & "              B.NOTES7 ,                             " _
                            & "              B.NOTES8 ,                             " _
                            & "              B.NOTES9 ,                             " _
                            & "              B.NOTES10 ,                            " _
                            & "              A.ARRIVTIME ,                          " _
                            & "              A.DISTANCE                             " _
                            & "     ORDER BY A.SEQ ,  			                    " _
                            & "              B.TODOKECODE ,                         " _
                            & "              B.NAMES ,                              " _
                            & "              B.ADDR1 ,                              " _
                            & "              B.ADDR2 ,                              " _
                            & "              B.ADDR3 ,                              " _
                            & "              B.ADDR4 ,                              " _
                            & "              B.NOTES1 ,                             " _
                            & "              B.NOTES2 ,                             " _
                            & "              B.NOTES3 ,                             " _
                            & "              B.NOTES4 ,                             " _
                            & "              B.NOTES5 ,                             " _
                            & "              B.NOTES6 ,                             " _
                            & "              B.NOTES7 ,                             " _
                            & "              B.NOTES8 ,                             " _
                            & "              B.NOTES9 ,                             " _
                            & "              B.NOTES10 ,                            " _
                            & "              A.ARRIVTIME ,                          " _
                            & "              A.DISTANCE                             "

                    SQLcmd = New SqlCommand(SQLStr, SQLcon)
                    Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.Date)
                    Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.NVarChar, 20)
                    Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.NVarChar, 20)
                    Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", System.Data.SqlDbType.NVarChar, 1)
                    Dim PARA5 As SqlParameter = SQLcmd.Parameters.Add("@P5", System.Data.SqlDbType.NVarChar, 20)
                    Dim PARA6 As SqlParameter = SQLcmd.Parameters.Add("@P6", System.Data.SqlDbType.NVarChar, 20)
                    PARA1.Value = SHUKODATE
                    PARA2.Value = CAMPCODE
                    PARA3.Value = ORG
                    PARA4.Value = CLAS
                    PARA5.Value = TORICODE
                    PARA6.Value = USERID
                End If
            End If

            Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

            While SQLdr.Read
                '○出力編集
                TODOKECODE.Add(SQLdr("TODOKECODE"))
                TODOKECODENAME.Add(SQLdr("NAMES"))
                ADDR.Add(SQLdr("ADDR"))
                NOTES1.Add(SQLdr("NOTES1"))
                NOTES2.Add(SQLdr("NOTES2"))
                NOTES3.Add(SQLdr("NOTES3"))
                NOTES4.Add(SQLdr("NOTES4"))
                NOTES5.Add(SQLdr("NOTES5"))
                NOTES6.Add(SQLdr("NOTES6"))
                NOTES7.Add(SQLdr("NOTES7"))
                NOTES8.Add(SQLdr("NOTES8"))
                NOTES9.Add(SQLdr("NOTES9"))
                NOTES10.Add(SQLdr("NOTES10"))
                If IsDBNull(SQLdr("ARRIVTIME")) Then
                    ARRIVTIME.Add("")
                Else
                    ARRIVTIME.Add(SQLdr("ARRIVTIME"))
                End If
                If IsDBNull(SQLdr("DISTANCE")) Then
                    DISTANCE.Add("")
                Else
                    DISTANCE.Add(SQLdr("DISTANCE"))
                End If
                LISTBOX.Items.Add(New ListItem(SQLdr("NAMES"), SQLdr("TODOKECODE")))
            End While

            'Close
            SQLdr.Close() 'Reader(Close)
            SQLdr = Nothing

            SQLcmd.Dispose()
            SQLcmd = Nothing

            SQLcon.Close() 'DataBase接続(Close)
            SQLcon.Dispose()
            SQLcon = Nothing

            ERR = C_MESSAGE_NO.NORMAL
        Catch ex As Exception
            Dim CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get
            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME           'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:MC007_TODKORG Select"          '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            ERR = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try
    End Sub

End Class
