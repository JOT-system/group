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
''' Leftボックス用会社取得
''' </summary>
''' <remarks></remarks>
Public Class GS0008CAMPget
    Inherits GS0000
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
    Public Property CAMPCODE() As List(Of String)
    ''' <summary>
    ''' 会社名
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property CAMPNAME() As List(Of String)
    ''' <summary>
    ''' 会社情報
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property LISTBOX() As ListBox

    ''' <summary>
    ''' 選択可能な会社コード一覧を権限情報から取得する
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub GS0008CAMPget()
        '<< エラー説明 >>
        'O_ERR = OK:00000,ERR:00002(環境エラー),ERR:00003(DBerr)
        '●初期処理
        CAMPCODE = New List(Of String)
        CAMPNAME = New List(Of String)
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

        '●Leftボックス用会社取得
        '○ User権限によりDB(S0005_AUTHOR)検索
        Try
            'DataBase接続文字
            Dim SQLcon = sm.getConnection
            SQLcon.Open() 'DataBase接続(Open)

            '検索SQL文
            Dim SQLStr As String = _
                    "SELECT rtrim(B.CODE) as CAMPCODE , rtrim(C.NAMES) as NAMES " _
                & " FROM  S0005_AUTHOR A " _
                & " INNER JOIN S0006_ROLE B " _
                & "   ON  B.CAMPCODE = A.CAMPCODE " _
                & "   and B.OBJECT   = A.OBJECT " _
                & "   and B.ROLE     = A.ROLE " _
                & "   and B.PERMITCODE >= 1 " _
                & "   and B.STYMD   <= @P3 " _
                & "   and B.ENDYMD  >= @P2 " _
                & "   and B.DELFLG  <> '1' " _
                & " INNER JOIN M0001_CAMP C " _
                & "   ON  C.CAMPCODE = B.CODE " _
                & "   and C.STYMD   <= @P3 " _
                & "   and C.ENDYMD  >= @P2 " _
                & "   and C.DELFLG  <> '1' " _
                & " Where A.USERID  = @P1 " _
                & "   and A.OBJECT   = 'CAMP' " _
                & "   and A.STYMD   <= @P3 " _
                & "   and A.ENDYMD  >= @P2 " _
                & "   and A.DELFLG  <> '1' " _
                & "GROUP BY B.CODE , C.NAMES " _
                & "ORDER BY B.CODE , C.NAMES "

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
                CAMPCODE.Add(SQLdr("CAMPCODE"))
                CAMPNAME.Add(SQLdr("NAMES"))

                LISTBOX.Items.Add(New ListItem(SQLdr("NAMES"), SQLdr("CAMPCODE")))
                LISTBOX.SelectedIndex = 0
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
            CS0011LOGWRITE.INFSUBCLASS = "GS0008CAMPget"                'SUBクラス名
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
