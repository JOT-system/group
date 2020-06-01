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
''' Leftボックス用従業員取得
''' </summary>
''' <remarks>ログインユーザが参照可能な組織に属する従業員を取得する</remarks>
Public Class GS0018STAFFCODEget
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
    ''' 所属部署コード
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property HORG() As String
    ''' <summary>
    ''' 従業員コード
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property STAFFCODE() As String
    ''' <summary>
    ''' 従業員コード（検索範囲開始）
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property STAFFCODEFROM() As String
    ''' <summary>
    ''' 従業員コード（検索範囲終了）
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property STAFFCODETO() As String
    ''' <summary>
    ''' 従業員区分
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property STAFFKBN() As ListBox
    ''' <summary>
    ''' 従業員コード
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property OSTAFFCODE() As List(Of String)
    ''' <summary>
    ''' 従業員名称
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property OSTAFFNAME() As List(Of String)
    ''' <summary>
    ''' 従業員一覧
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property LISTBOX() As ListBox
    ''' <summary>
    ''' 検索従業員区分
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks>"0":営業部+支店+営業所、"1":総務部+営業部+支店+営業所</remarks>
    Public Property STAFFparm() As String

    Protected METHOD_NAME As String = "GS0018STAFFCODEget"

    ''' <summary>
    ''' 従業員取得
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub GS0018STAFFCODEget()
        '<< エラー説明 >>
        'ERR = OK:00000,ERR:00002(環境エラー),ERR:00003(DBerr)
        '●初期処理
        OSTAFFCODE = New List(Of String)
        OSTAFFNAME = New List(Of String)
        'セッション制御宣言
        Dim sm As New CS0050SESSION
        'PARAM EXTRA01: USERID
        If IsNothing(USERID) Then
            USERID = sm.USERID
        End If
        '●Leftボックス用従業員取得
        '○ User権限によりDB(S0005_AUTHOR)検索
        Try
            'DataBase接続文字
            Dim SQLcon = sm.getConnection
            SQLcon.Open() 'DataBase接続(Open)

            Dim SQLStr As String = ""
            If STAFFparm = "1" Then
                '検索SQL文
                SQLStr = _
                        "SELECT  rtrim(D.CAMPCODE)   as CAMPCODE " _
                    & "      , rtrim(D.STAFFCODE)  as CODE  " _
                    & "      , rtrim(D.STAFFNAMES) as NAMES  " _
                    & "      , rtrim(D.STAFFKBN)   as KBN  " _
                    & " FROM  S0005_AUTHOR A " _
                    & " INNER JOIN S0006_ROLE B " _
                    & "   ON  B.CAMPCODE = A.CAMPCODE " _
                    & "   and B.OBJECT   = A.OBJECT " _
                    & "   and B.ROLE     = A.ROLE " _
                    & "   and B.PERMITCODE >= 1 " _
                    & "   and B.STYMD   <= @P2 " _
                    & "   and B.ENDYMD  >= @P2 " _
                    & "   and B.DELFLG  <> '1' " _
                    & " INNER JOIN M0002_ORG C " _
                    & "   ON  C.CAMPCODE = B.CAMPCODE " _
                    & "   and C.DELFLG  <> '1' " _
                    & "   and C.STYMD   <= @P2 " _
                    & "   and C.ENDYMD  >= @P2 " _
                    & " INNER JOIN M0002_ORG S " _
                    & "   ON  S.CAMPCODE = C.CAMPCODE " _
                    & "   and S.DELFLG  <> '1' " _
                    & "   and S.STYMD   <= @P2 " _
                    & "   and S.ENDYMD  >= @P2 " _
                    & " INNER JOIN MB001_STAFF D " _
                    & "   ON  D.MORG     = C.ORGCODE " _
                    & "   and D.HORG     = S.ORGCODE " _
                    & "   and D.CAMPCODE = C.CAMPCODE " _
                    & "   and D.STYMD   <= @P2 " _
                    & "   and D.ENDYMD  >= @P2 " _
                    & "   and D.DELFLG  <> '1' " _
                    & " Where A.USERID   = @P1 " _
                    & "   and A.OBJECT   = 'ORG' " _
                    & "   and A.STYMD   <= @P2 " _
                    & "   and A.ENDYMD  >= @P2 " _
                    & "   and A.DELFLG  <> '1' "
            Else
                SQLStr = _
                        "SELECT  rtrim(D.CAMPCODE)   as CAMPCODE " _
                    & "      , rtrim(D.STAFFCODE)    as CODE  " _
                    & "      , rtrim(D.STAFFNAMES) as NAMES  " _
                    & "      , rtrim(D.STAFFKBN)   as KBN  " _
                    & " FROM  S0005_AUTHOR A " _
                    & " INNER JOIN S0006_ROLE B " _
                    & "   ON  B.CAMPCODE = A.CAMPCODE " _
                    & "   and B.OBJECT   = A.OBJECT " _
                    & "   and B.ROLE     = A.ROLE " _
                    & "   and B.PERMITCODE >= 1 " _
                    & "   and B.STYMD   <= @P2 " _
                    & "   and B.ENDYMD  >= @P2 " _
                    & "   and B.DELFLG  <> '1' " _
                    & " INNER JOIN M0002_ORG C " _
                    & "   ON  C.CAMPCODE = B.CAMPCODE " _
                    & "   and C.ORGLEVEL IN ('01000', '00100') " _
                    & "   and C.DELFLG  <> '1' " _
                    & "   and C.STYMD   <= @P2 " _
                    & "   and C.ENDYMD  >= @P2 " _
                    & " INNER JOIN M0002_ORG S " _
                    & "   ON  S.CAMPCODE = C.CAMPCODE " _
                    & "   and S.ORGLEVEL = '00010' " _
                    & "   and S.DELFLG  <> '1' " _
                    & "   and S.STYMD   <= @P2 " _
                    & "   and S.ENDYMD  >= @P2 " _
                    & " INNER JOIN MB001_STAFF D " _
                    & "   ON  D.MORG     = C.ORGCODE " _
                    & "   and D.HORG     = S.ORGCODE " _
                    & "   and D.CAMPCODE = C.CAMPCODE " _
                    & "   and D.STYMD   <= @P2 " _
                    & "   and D.ENDYMD  >= @P2 " _
                    & "   and D.DELFLG  <> '1' " _
                    & " Where A.USERID   = @P1 " _
                    & "   and A.OBJECT   = 'ORG' " _
                    & "   and A.STYMD   <= @P2 " _
                    & "   and A.ENDYMD  >= @P2 " _
                    & "   and A.DELFLG  <> '1' "
            End If

            Dim addSQL As String = String.Empty
            If (String.IsNullOrEmpty(Me.HORG) = False) Then
                addSQL &= String.Format(" and S.ORGCODE = '{0}' ", Me.HORG)
            End If
            If (String.IsNullOrEmpty(Me.STAFFCODE) = False) Then
                addSQL &= String.Format(" and D.STAFFCODE = '{0}' ", Me.STAFFCODE)
            End If
            If (String.IsNullOrEmpty(Me.STAFFCODEFROM) = False) Then
                addSQL &= String.Format(" and D.STAFFCODE >= '{0}' ", Me.STAFFCODEFROM)
            End If
            If (String.IsNullOrEmpty(Me.STAFFCODETO) = False) Then
                addSQL &= String.Format(" and D.STAFFCODE <= '{0}' ", Me.STAFFCODETO)
            End If
            addSQL &= "GROUP BY D.CAMPCODE , D.STAFFCODE , D.STAFFNAMES ,D.STAFFKBN "
            addSQL &= "ORDER BY D.CAMPCODE , D.STAFFCODE , D.STAFFNAMES "

            ' 最終SQL整形
            SQLStr &= addSQL

            Dim SQLcmd As New SqlCommand(SQLStr, SQLcon)
            Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.NVarChar, 20)
            Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.Date)
            PARA1.Value = USERID
            PARA2.Value = Date.Now
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
                ' 対象フラグ
                Dim WW_flag As Boolean = False
                ' 従業員コード
                Dim WW_STAFFCODE As String = SQLdr("CODE")
                ' 職務区分が設定されている場合は判定する
                If (IsNothing(Me.STAFFKBN) = False) Then
                    For Each SKBN As ListItem In Me.STAFFKBN.Items
                        If (SKBN.Value = SQLdr("KBN")) Then
                            WW_flag = True
                            Exit For
                        End If
                    Next
                Else
                    WW_flag = True
                End If
                If (WW_flag = True) Then
                    '○出力編集
                    If CAMPCODE = "" Then
                        OSTAFFCODE.Add(SQLdr("CODE"))
                        OSTAFFNAME.Add(SQLdr("NAMES"))
                        LISTBOX.Items.Add(New ListItem(SQLdr("NAMES"), SQLdr("CODE")))
                    ElseIf CAMPCODE = SQLdr("CAMPCODE") Then
                        OSTAFFCODE.Add(SQLdr("CODE"))
                        OSTAFFNAME.Add(SQLdr("NAMES"))
                        LISTBOX.Items.Add(New ListItem(SQLdr("NAMES"), SQLdr("CODE")))
                    End If
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
            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME             'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:S0005_AUTHOR Select"           '
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
