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
''' Leftボックス用組織取得
''' </summary>
''' <remarks></remarks>
Public Class GS0009ORGget
    Inherits GS0000

    ''' <summary>
    ''' 部署取得区分
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks>"0"=営業部、"1"=営業部+支店、"2"or"△"=営業所、"3"=総務+営業部+支店、"4"=総務+営業部+営業所</remarks>
    Public Property ORGparam() As String
    ''' <summary>
    ''' ユーザID
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property USERID As String
    ''' <summary>
    ''' 開始年月日
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property STYMD As Date
    ''' <summary>
    ''' 終了年月日
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property ENDYMD As Date
    ''' <summary>
    ''' 会社コード
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property CAMPCODE() As String
    
    
    Public Property TAISHOYMD() As String
    ''' <summary>
    ''' 組織CODE
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property ORGCODE() As List(Of String)
    ''' <summary>
    ''' 組織名称
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property ORGNAME() As List(Of String)
    ''' <summary>
    ''' 組織一覧
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property LISTBOX() As ListBox
    ''' <summary>
    ''' 部署コード一覧を作成する
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub GS0009ORGget()
        '<< エラー説明 >>
        'O_ERR = OK:00000,ERR:00002(環境エラー),ERR:00003(DBerr)
        '●初期処理
        ORGCODE = New List(Of String)
        ORGNAME = New List(Of String)
        'セッション制御宣言
        Dim sm As New CS0050SESSION
        'PARAM EXTRA01: USERID
        If IsNothing(USERID) Then
            USERID = sm.USERID
        End If
        'PARAM EXTRA02: STYMD
        If STYMD < C_DEFAULT_YMD Then
            STYMD = Date.Now
        End If
        'PARAM EXTRA03: ENDYMD
        If ENDYMD < C_DEFAULT_YMD Then
            ENDYMD = Date.Now
        End If
        'PARAM EXTRA04: CAMPCODE
        If IsNothing(CAMPCODE) Then
            CAMPCODE = ""
        End If
        '●Leftボックス用組織取得
        '○ User権限によりDB(S0005_AUTHOR)検索
        Try
            'DataBase接続文字
            Dim SQLcon = sm.getConnection
            SQLcon.Open() 'DataBase接続(Open)

            '検索SQL文
            Dim SQLStr As String = ""
            If ORGparam = "0" Then
                SQLStr = _
                        "SELECT rtrim(B.CAMPCODE) as CAMPCODE , rtrim(B.CODE) as ORGCODE , rtrim(C.NAMES) as NAMES " _
                    & " FROM  S0005_AUTHOR A " _
                    & " INNER JOIN S0006_ROLE B " _
                    & "   ON    B.CAMPCODE = A.CAMPCODE " _
                    & "   and   B.OBJECT   = A.OBJECT " _
                    & "   and   B.ROLE     = A.ROLE " _
                    & "   and   B.PERMITCODE >= 1 " _
                    & "   and   B.STYMD   <= @P3 " _
                    & "   and   B.ENDYMD  >= @P2 " _
                    & "   and   B.DELFLG  <> '1' " _
                    & " INNER JOIN M0002_ORG C " _
                    & "   ON    C.CAMPCODE = B.CAMPCODE " _
                    & "   and   C.ORGCODE  = B.CODE " _
                    & "   and   C.ORGLEVEL = '01000' " _
                    & "   and   C.STYMD   <= @P3 " _
                    & "   and   C.ENDYMD  >= @P2 " _
                    & "   and   C.DELFLG  <> '1' " _
                    & " Where   A.USERID  = @P1 " _
                    & "   and   A.OBJECT   = 'ORG' " _
                    & "   and   A.STYMD   <= @P3 " _
                    & "   and   A.ENDYMD  >= @P2 " _
                    & "   and   A.DELFLG  <> '1' " _
                    & "GROUP BY B.CAMPCODE , B.CODE , C.NAMES " _
                    & "ORDER BY B.CAMPCODE , B.CODE , C.NAMES "
            ElseIf ORGparam = "1" Then
                SQLStr = _
                        "SELECT rtrim(B.CAMPCODE) as CAMPCODE , rtrim(B.CODE) as ORGCODE , rtrim(C.NAMES) as NAMES " _
                    & " FROM  S0005_AUTHOR A " _
                    & " INNER JOIN S0006_ROLE B " _
                    & "   ON    B.CAMPCODE = A.CAMPCODE " _
                    & "   and   B.OBJECT   = A.OBJECT " _
                    & "   and   B.ROLE     = A.ROLE " _
                    & "   and   B.PERMITCODE >= 1 " _
                    & "   and   B.STYMD   <= @P3 " _
                    & "   and   B.ENDYMD  >= @P2 " _
                    & "   and   B.DELFLG  <> '1' " _
                    & " INNER JOIN M0002_ORG C " _
                    & "   ON    C.CAMPCODE = B.CAMPCODE " _
                    & "   and   C.ORGCODE  = B.CODE " _
                    & "   and   C.ORGLEVEL IN ('01000', '00100') " _
                    & "   and   C.STYMD   <= @P3 " _
                    & "   and   C.ENDYMD  >= @P2 " _
                    & "   and   C.DELFLG  <> '1' " _
                    & " Where   A.USERID  = @P1 " _
                    & "   and   A.OBJECT   = 'ORG' " _
                    & "   and   A.STYMD   <= @P3 " _
                    & "   and   A.ENDYMD  >= @P2 " _
                    & "   and   A.DELFLG  <> '1' " _
                    & "GROUP BY B.CAMPCODE , B.CODE , C.NAMES " _
                    & "ORDER BY B.CAMPCODE , B.CODE , C.NAMES "
            ElseIf ORGparam = "2" Or ORGparam = "" Then
                SQLStr = _
                        "SELECT rtrim(B.CAMPCODE) as CAMPCODE , rtrim(B.CODE) as ORGCODE , rtrim(C.NAMES) as NAMES " _
                    & " FROM  S0005_AUTHOR A " _
                    & " INNER JOIN S0006_ROLE B " _
                    & "   ON    B.CAMPCODE = A.CAMPCODE " _
                    & "   and   B.OBJECT   = A.OBJECT " _
                    & "   and   B.ROLE     = A.ROLE " _
                    & "   and   B.PERMITCODE >= 1 " _
                    & "   and   B.STYMD   <= @P3 " _
                    & "   and   B.ENDYMD  >= @P2 " _
                    & "   and   B.DELFLG  <> '1' " _
                    & " INNER JOIN M0002_ORG C " _
                    & "   ON    C.CAMPCODE = B.CAMPCODE " _
                    & "   and   C.ORGCODE  = B.CODE " _
                    & "   and   C.ORGLEVEL  = '00010' " _
                    & "   and   C.STYMD   <= @P3 " _
                    & "   and   C.ENDYMD  >= @P2 " _
                    & "   and   C.DELFLG  <> '1' " _
                    & " Where   A.USERID  = @P1 " _
                    & "   and   A.OBJECT   = 'ORG' " _
                    & "   and   A.STYMD   <= @P3 " _
                    & "   and   A.ENDYMD  >= @P2 " _
                    & "   and   A.DELFLG  <> '1' " _
                    & "GROUP BY B.CAMPCODE , B.CODE , C.NAMES " _
                    & "ORDER BY B.CAMPCODE , B.CODE , C.NAMES "
            ElseIf ORGparam = "3" Then
                SQLStr = _
                        "SELECT rtrim(B.CAMPCODE) as CAMPCODE , rtrim(B.CODE) as ORGCODE , rtrim(C.NAMES) as NAMES " _
                    & " FROM  S0005_AUTHOR A " _
                    & " INNER JOIN S0006_ROLE B " _
                    & "   ON    B.CAMPCODE = A.CAMPCODE " _
                    & "   and   B.OBJECT   = A.OBJECT " _
                    & "   and   B.ROLE     = A.ROLE " _
                    & "   and   B.PERMITCODE >= 1 " _
                    & "   and   B.STYMD   <= @P3 " _
                    & "   and   B.ENDYMD  >= @P2 " _
                    & "   and   B.DELFLG  <> '1' " _
                    & " INNER JOIN M0002_ORG C " _
                    & "   ON    C.CAMPCODE = B.CAMPCODE " _
                    & "   and   C.ORGCODE  = B.CODE " _
                    & "   and   C.ORGLEVEL  IN ('02000', '01000', '00100') " _
                    & "   and   C.STYMD   <= @P3 " _
                    & "   and   C.ENDYMD  >= @P2 " _
                    & "   and   C.DELFLG  <> '1' " _
                    & " Where   A.USERID  = @P1 " _
                    & "   and   A.OBJECT   = 'ORG' " _
                    & "   and   A.STYMD   <= @P3 " _
                    & "   and   A.ENDYMD  >= @P2 " _
                    & "   and   A.DELFLG  <> '1' " _
                    & "GROUP BY B.CAMPCODE , B.CODE , C.NAMES " _
                    & "ORDER BY B.CAMPCODE , B.CODE , C.NAMES "
            ElseIf ORGparam = "4" Then
                SQLStr = _
                        "SELECT rtrim(B.CAMPCODE) as CAMPCODE , rtrim(B.CODE) as ORGCODE , rtrim(C.NAMES) as NAMES " _
                    & " FROM  S0005_AUTHOR A " _
                    & " INNER JOIN S0006_ROLE B " _
                    & "   ON    B.CAMPCODE = A.CAMPCODE " _
                    & "   and   B.OBJECT   = A.OBJECT " _
                    & "   and   B.ROLE     = A.ROLE " _
                    & "   and   B.PERMITCODE >= 1 " _
                    & "   and   B.STYMD   <= @P3 " _
                    & "   and   B.ENDYMD  >= @P2 " _
                    & "   and   B.DELFLG  <> '1' " _
                    & " INNER JOIN M0002_ORG C " _
                    & "   ON    C.CAMPCODE = B.CAMPCODE " _
                    & "   and   C.ORGCODE  = B.CODE " _
                    & "   and   C.ORGLEVEL  IN ('02000', '01000', '00100', '00010', '00001') " _
                    & "   and   C.STYMD   <= @P3 " _
                    & "   and   C.ENDYMD  >= @P2 " _
                    & "   and   C.DELFLG  <> '1' " _
                    & " Where   A.USERID  = @P1 " _
                    & "   and   A.OBJECT   = 'ORG' " _
                    & "   and   A.STYMD   <= @P3 " _
                    & "   and   A.ENDYMD  >= @P2 " _
                    & "   and   A.DELFLG  <> '1' " _
                    & "GROUP BY B.CAMPCODE , B.CODE , C.NAMES " _
                    & "ORDER BY B.CAMPCODE , B.CODE , C.NAMES "
            Else
                ERR = C_MESSAGE_NO.DLL_IF_ERROR
                Exit Sub
            End If



            Dim SQLcmd As New SqlCommand(SQLStr, SQLcon)
            Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.NVarChar, 20)
            Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.Date)
            Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.Date)
            PARA1.Value = USERID
            PARA2.Value = STYMD
            PARA3.Value = ENDYMD
            Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

            Try
                If IsNothing(LISTBOX) Then
                    LISTBOX = New ListBox
                Else
                    CType(LISTBOX, ListBox).Items.Clear()
                End If
            Catch ex As Exception
            End Try

            While SQLdr.Read
                '○出力編集
                If CAMPCODE = "" Then
                    ORGCODE.Add(SQLdr("ORGCODE"))
                    ORGNAME.Add(SQLdr("NAMES"))
                    LISTBOX.Items.Add(New ListItem(SQLdr("NAMES"), SQLdr("ORGCODE")))
                ElseIf CAMPCODE = SQLdr("CAMPCODE") Then
                    ORGCODE.Add(SQLdr("ORGCODE"))
                    ORGNAME.Add(SQLdr("NAMES"))
                    LISTBOX.Items.Add(New ListItem(SQLdr("NAMES"), SQLdr("ORGCODE")))
                End If
            End While

            'Close
            SQLdr.Close() 'Reader(Close)
            SQLdr = Nothing

            SQLcmd.Dispose()
            SQLcmd = Nothing

            SQLcon.Close() 'DataBase接続(Close)
            SQLcon.Dispose()
            SQLcon = Nothing

        Catch ex As Exception
            Dim CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get
            CS0011LOGWRITE.INFSUBCLASS = "GS0009ORGget"                 'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:S0005_AUTHOR Select"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            ERR = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try

        ERR = C_MESSAGE_NO.NORMAL

    End Sub

End Class
