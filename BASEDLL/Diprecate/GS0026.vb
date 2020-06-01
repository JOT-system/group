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
''' <remarks>（APSRVOrg）</remarks>
Public Class GS0026ORGSRVget
    Inherits GS0000
    ''' <summary>
    ''' ユーザID
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property USERID As String
    ''' <summary>
    ''' 端末ID
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property TERMID() As String
    ''' <summary>
    ''' 部署取得区分
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks>"0"=営業部、"1"=営業部+支店、"2"=営業部+支店+営業所、"3"=本社+営業部+営業所</remarks>
    Public Property ORGparam() As String
    ''' <summary>
    ''' 会社コード
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property CAMPCODE() As String
    ''' <summary>
    ''' 出庫日
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property SHUKODATE() As Date
    ''' <summary>
    ''' 部署コード一覧
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property ORGCODE() As List(Of String)
    ''' <summary>
    ''' 部署名一覧
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property OORGNAME() As List(Of String)
    ''' <summary>
    ''' 部署一覧
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property LISTBOX() As ListBox

    Protected METHOD_NAME As String = "GS0026ORGSRVget"
    ''' <summary>
    ''' 組織取得
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub GS0026ORGSRVget()
        '<< エラー説明 >>
        'ERR = OK:00000,ERR:00002(環境エラー),ERR:00003(DBerr)
        '●初期処理
        ORGCODE = New List(Of String)
        ORGNAME = New List(Of String)
        'セッション制御宣言
        Dim sm As New CS0050SESSION
        'PARAM EXTRA01: USERID
        If IsNothing(USERID) Then
            USERID = sm.USERID
        End If
        'PARAM EXTRA02: SHUKODATE
        If SHUKODATE < C_DEFAULT_YMD Then
            SHUKODATE = Date.Now
        End If
        'PARAM EXTRA03: TERMID
        If IsNothing(TERMID) Then
            TERMID = sm.TERMID
        End If
        '●Leftボックス用組織取得（APSRVOrg）
        '○ User権限によりDB(S0005_AUTHOR)検索
        Try
            'DataBase接続文字
            Dim SQLcon = sm.getConnection
            SQLcon.Open() 'DataBase接続(Open)

            '検索SQL文
            Dim SQLStr As String = ""
            If ORGparam = "0" Then
                SQLStr = _
                        "       SELECT rtrim(E.CAMPCODE)   as CAMPCODE ,   " _
                    & "              rtrim(E.ORGCODE)    as ORGCODE ,    " _
                    & "              rtrim(E.NAMES)      as NAMES        " _
                    & "         FROM S0005_AUTHOR    as A                " _
                    & "   INNER JOIN S0006_ROLE      as B                " _
                    & "           ON B.CAMPCODE  = A.CAMPCODE            " _
                    & "          and B.OBJECT    = A.OBJECT              " _
                    & "          and B.ROLE      = A.ROLE                " _
                    & "          and B.PERMITCODE >= 1                   " _
                    & "          and B.STYMD    <= @P2                   " _
                    & "          and B.ENDYMD   >= @P2                   " _
                    & "          and B.DELFLG   <> '1'                   " _
                    & "   INNER JOIN S0012_SRVAUTHOR as C                " _
                    & "           ON C.TERMID    = @P3                   " _
                    & "          and C.OBJECT    = 'SRVORG'              " _
                    & "          and C.STYMD    <= @P2                   " _
                    & "          and C.ENDYMD   >= @P2                   " _
                    & "          and C.DELFLG   <> '1'                   " _
                    & "   INNER JOIN S0006_ROLE      as D                " _
                    & "           ON D.CAMPCODE  = C.CAMPCODE            " _
                    & "          and D.OBJECT    = C.OBJECT              " _
                    & "          and D.ROLE      = C.ROLE                " _
                    & "          and D.CODE      = B.CODE                " _
                    & "          and D.PERMITCODE >= 1                   " _
                    & "          and D.STYMD    <= @P2                   " _
                    & "          and D.ENDYMD   >= @P2                   " _
                    & "          and D.DELFLG   <> '1'                   " _
                    & "   INNER JOIN M0002_ORG       as E                " _
                    & "           ON E.CAMPCODE  = D.CAMPCODE            " _
                    & "          and E.ORGCODE   = D.CODE                " _
                    & "          and E.ORGLEVEL  = '01000'               " _
                    & "          and E.STYMD    <= @P2                   " _
                    & "          and E.ENDYMD   >= @P2                   " _
                    & "          and E.DELFLG   <> '1'                   " _
                    & "        Where A.USERID    = @P1                   " _
                    & "          and A.OBJECT    = 'ORG'                 " _
                    & "          and A.STYMD    <= @P2                   " _
                    & "          and A.ENDYMD   >= @P2                   " _
                    & "          and A.DELFLG   <> '1'                   " _
                    & "     GROUP BY E.CAMPCODE ,                        " _
                    & "              D.SEQ ,                             " _
                    & "              E.ORGCODE ,                         " _
                    & "              E.NAMES                             " _
                    & "     ORDER BY E.CAMPCODE ,                        " _
                    & "              D.SEQ ,                             " _
                    & "              E.ORGCODE ,                         " _
                    & "              E.NAMES                             "

            ElseIf ORGparam = "1" Then
                SQLStr = _
                        "       SELECT rtrim(E.CAMPCODE)   as CAMPCODE ,   " _
                    & "              rtrim(E.ORGCODE)    as ORGCODE ,    " _
                    & "              rtrim(E.NAMES)      as NAMES        " _
                    & "         FROM S0005_AUTHOR    as A                " _
                    & "   INNER JOIN S0006_ROLE      as B                " _
                    & "           ON B.CAMPCODE  = A.CAMPCODE            " _
                    & "          and B.OBJECT    = A.OBJECT              " _
                    & "          and B.ROLE      = A.ROLE                " _
                    & "          and B.PERMITCODE >= 1                   " _
                    & "          and B.STYMD    <= @P2                   " _
                    & "          and B.ENDYMD   >= @P2                   " _
                    & "          and B.DELFLG   <> '1'                   " _
                    & "   INNER JOIN S0012_SRVAUTHOR as C                " _
                    & "           ON C.TERMID    = @P3                   " _
                    & "          and C.OBJECT    = 'SRVORG'              " _
                    & "          and C.STYMD    <= @P2                   " _
                    & "          and C.ENDYMD   >= @P2                   " _
                    & "          and C.DELFLG   <> '1'                   " _
                    & "   INNER JOIN S0006_ROLE      as D                " _
                    & "           ON D.CAMPCODE  = C.CAMPCODE            " _
                    & "          and D.OBJECT    = C.OBJECT              " _
                    & "          and D.ROLE      = C.ROLE                " _
                    & "          and D.CODE      = B.CODE                " _
                    & "          and D.PERMITCODE >= 1                   " _
                    & "          and D.STYMD    <= @P2                   " _
                    & "          and D.ENDYMD   >= @P2                   " _
                    & "          and D.DELFLG   <> '1'                   " _
                    & "   INNER JOIN M0002_ORG       as E                " _
                    & "           ON E.CAMPCODE  = D.CAMPCODE            " _
                    & "          and E.ORGCODE   = D.CODE                " _
                    & "   		and ( (E.ORGLEVEL  = '01000') 			" _
                    & "   		 or   (E.ORGLEVEL  = '00100') ) 		" _
                    & "          and E.STYMD    <= @P2                   " _
                    & "          and E.ENDYMD   >= @P2                   " _
                    & "          and E.DELFLG   <> '1'                   " _
                    & "        Where A.USERID    = @P1                   " _
                    & "          and A.OBJECT    = 'ORG'                 " _
                    & "          and A.STYMD    <= @P2                   " _
                    & "          and A.ENDYMD   >= @P2                   " _
                    & "          and A.DELFLG   <> '1'                   " _
                    & "     GROUP BY E.CAMPCODE ,                        " _
                    & "              D.SEQ ,                             " _
                    & "              E.ORGCODE ,                         " _
                    & "              E.NAMES                             " _
                    & "     ORDER BY E.CAMPCODE ,                        " _
                    & "              D.SEQ ,                             " _
                    & "              E.ORGCODE ,                         " _
                    & "              E.NAMES                             "
            ElseIf ORGparam = "2" Then
                SQLStr = _
                        "       SELECT rtrim(E.CAMPCODE)   as CAMPCODE ,   " _
                    & "              rtrim(E.ORGCODE)    as ORGCODE ,    " _
                    & "              rtrim(E.NAMES)      as NAMES        " _
                    & "         FROM S0005_AUTHOR    as A                " _
                    & "   INNER JOIN S0006_ROLE      as B                " _
                    & "           ON B.CAMPCODE  = A.CAMPCODE            " _
                    & "          and B.OBJECT    = A.OBJECT              " _
                    & "          and B.ROLE      = A.ROLE                " _
                    & "          and B.PERMITCODE >= 1                   " _
                    & "          and B.STYMD    <= @P2                   " _
                    & "          and B.ENDYMD   >= @P2                   " _
                    & "          and B.DELFLG   <> '1'                   " _
                    & "   INNER JOIN S0012_SRVAUTHOR as C                " _
                    & "           ON C.TERMID    = @P3                   " _
                    & "          and C.OBJECT    = 'SRVORG'              " _
                    & "          and C.STYMD    <= @P2                   " _
                    & "          and C.ENDYMD   >= @P2                   " _
                    & "          and C.DELFLG   <> '1'                   " _
                    & "   INNER JOIN S0006_ROLE      as D                " _
                    & "           ON D.CAMPCODE  = C.CAMPCODE            " _
                    & "          and D.OBJECT    = C.OBJECT              " _
                    & "          and D.ROLE      = C.ROLE                " _
                    & "          and D.CODE      = B.CODE                " _
                    & "          and D.PERMITCODE >= 1                   " _
                    & "          and D.STYMD    <= @P2                   " _
                    & "          and D.ENDYMD   >= @P2                   " _
                    & "          and D.DELFLG   <> '1'                   " _
                    & "   INNER JOIN M0002_ORG       as E                " _
                    & "           ON E.CAMPCODE  = D.CAMPCODE            " _
                    & "          and E.ORGCODE   = D.CODE                " _
                    & "          and E.ORGLEVEL  = '00010'               " _
                    & "          and E.STYMD    <= @P2                   " _
                    & "          and E.ENDYMD   >= @P2                   " _
                    & "          and E.DELFLG   <> '1'                   " _
                    & "        Where A.USERID    = @P1                   " _
                    & "          and A.OBJECT    = 'ORG'                 " _
                    & "          and A.STYMD    <= @P2                   " _
                    & "          and A.ENDYMD   >= @P2                   " _
                    & "          and A.DELFLG   <> '1'                   " _
                    & "     GROUP BY E.CAMPCODE ,                        " _
                    & "              D.SEQ ,                             " _
                    & "              E.ORGCODE ,                         " _
                    & "              E.NAMES                             " _
                    & "     ORDER BY E.CAMPCODE ,                        " _
                    & "              D.SEQ ,                             " _
                    & "              E.ORGCODE ,                         " _
                    & "              E.NAMES                             "
            Else
                SQLStr = _
                        "       SELECT rtrim(E.CAMPCODE)   as CAMPCODE ,   " _
                    & "              rtrim(E.ORGCODE)    as ORGCODE ,    " _
                    & "              rtrim(E.NAMES)      as NAMES        " _
                    & "         FROM S0005_AUTHOR    as A                " _
                    & "   INNER JOIN S0006_ROLE      as B                " _
                    & "           ON B.CAMPCODE  = A.CAMPCODE            " _
                    & "          and B.OBJECT    = A.OBJECT              " _
                    & "          and B.ROLE      = A.ROLE                " _
                    & "          and B.PERMITCODE >= 1                   " _
                    & "          and B.STYMD    <= @P2                   " _
                    & "          and B.ENDYMD   >= @P2                   " _
                    & "          and B.DELFLG   <> '1'                   " _
                    & "   INNER JOIN S0012_SRVAUTHOR as C                " _
                    & "           ON C.TERMID    = @P3                   " _
                    & "          and C.OBJECT    = 'SRVORG'              " _
                    & "          and C.STYMD    <= @P2                   " _
                    & "          and C.ENDYMD   >= @P2                   " _
                    & "          and C.DELFLG   <> '1'                   " _
                    & "   INNER JOIN S0006_ROLE      as D                " _
                    & "           ON D.CAMPCODE  = C.CAMPCODE            " _
                    & "          and D.OBJECT    = C.OBJECT              " _
                    & "          and D.ROLE      = C.ROLE                " _
                    & "          and D.CODE      = B.CODE                " _
                    & "          and D.PERMITCODE >= 1                   " _
                    & "          and D.STYMD    <= @P2                   " _
                    & "          and D.ENDYMD   >= @P2                   " _
                    & "          and D.DELFLG   <> '1'                   " _
                    & "   INNER JOIN M0002_ORG       as E                " _
                    & "           ON E.CAMPCODE  = D.CAMPCODE            " _
                    & "          and E.ORGCODE   = D.CODE                " _
                    & "   		and E.ORGLEVEL  IN ('02000','01000','00010') " _
                    & "          and E.STYMD    <= @P2                   " _
                    & "          and E.ENDYMD   >= @P2                   " _
                    & "          and E.DELFLG   <> '1'                   " _
                    & "        Where A.USERID    = @P1                   " _
                    & "          and A.OBJECT    = 'ORG'                 " _
                    & "          and A.STYMD    <= @P2                   " _
                    & "          and A.ENDYMD   >= @P2                   " _
                    & "          and A.DELFLG   <> '1'                   " _
                    & "     GROUP BY E.CAMPCODE ,                        " _
                    & "              D.SEQ ,                             " _
                    & "              E.ORGCODE ,                         " _
                    & "              E.NAMES                             " _
                    & "     ORDER BY E.CAMPCODE ,                        " _
                    & "              D.SEQ ,                             " _
                    & "              E.ORGCODE ,                         " _
                    & "              E.NAMES                             "
            End If

            Dim SQLcmd As New SqlCommand(SQLStr, SQLcon)
            Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.NVarChar, 20)
            Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.Date)
            Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.NVarChar, 20)
            PARA1.Value = USERID
            PARA2.Value = SHUKODATE
            PARA3.Value = TERMID
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

            ERR = C_MESSAGE_NO.NORMAL

        Catch ex As Exception
            Dim CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get
            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME             'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:S0005_AUTHOR Select"           '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            ERR = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try

    End Sub

End Class
