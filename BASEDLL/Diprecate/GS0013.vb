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
''' Leftボックス用油種取得
''' </summary>
''' <remarks>開始・終了は未使用</remarks>
Public Class GS0013OILTYPEget
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
    Public Property CAMPCODE() As String
    ''' <summary>
    ''' 項目記号名称
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property CLAS() As String
    ''' <summary>
    ''' 組織コード
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property ORG() As String
    ''' <summary>
    ''' 結果(ListBOX)
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property VALUE1() As ListBox
    ''' <summary>
    ''' 結果(ListBOX)
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property VALUE2() As ListBox
    ''' <summary>
    ''' 結果(ListBOX)
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property VALUE3() As ListBox
    ''' <summary>
    ''' 結果(ListBOX)
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property VALUE4() As ListBox
    ''' <summary>
    ''' 結果(ListBOX)
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property VALUE5() As ListBox
    ''' <summary>
    ''' 固定値1LISTBOX
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property LISTBOX1() As Object
    ''' <summary>
    ''' 固定値2LISTBOX
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property LISTBOX2() As Object
    ''' <summary>
    ''' 固定値3LISTBOX
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property LISTBOX3() As Object
    ''' <summary>
    ''' 固定値4LISTBOX
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property LISTBOX4() As Object
    ''' <summary>
    ''' 固定値5LISTBOX
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property LISTBOX5() As Object

    Protected METHOD_NAME As String = "GS0013OILTYPEget"
    ''' <summary>
    ''' 油種一覧
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub GS0013OILTYPEget()
        '<< エラー説明 >>
        'O_ERR = OK:00000,ERR:00002(環境エラー),ERR:00003(DBerr)
        '●初期処理
        VALUE1 = New ListBox
        VALUE2 = New ListBox
        VALUE3 = New ListBox
        VALUE4 = New ListBox
        VALUE5 = New ListBox
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

        Try
            If IsNothing(LISTBOX1) Then
                LISTBOX1 = New ListBox
            Else
                CType(LISTBOX1, ListBox).Items.Clear()
            End If

            If IsNothing(LISTBOX2) Then
                LISTBOX2 = New ListBox
            Else
                CType(LISTBOX2, ListBox).Items.Clear()
            End If

            If IsNothing(LISTBOX3) Then
                LISTBOX3 = New ListBox
            Else
                CType(LISTBOX3, ListBox).Items.Clear()
            End If

            If IsNothing(LISTBOX4) Then
                LISTBOX4 = New ListBox
            Else
                CType(LISTBOX4, ListBox).Items.Clear()
            End If

            If IsNothing(LISTBOX5) Then
                LISTBOX5 = New ListBox
            Else
                CType(LISTBOX5, ListBox).Items.Clear()
            End If

        Catch ex As Exception
        End Try

        ERR = C_MESSAGE_NO.DLL_IF_ERROR

        '●Leftボックス用油種取得
        'DataBase接続文字
        Dim SQLcon = sm.getConnection
        SQLcon.Open() 'DataBase接続(Open)
        '検索SQL文
        Dim SQLStr As String = ""

        '○ User権限によりDB(MC001_FIXVALUE)検索
        Try
            '検索SQL文
            SQLStr = _
                    "SELECT rtrim(D.KEYCODE) as KEYCODE , rtrim(D.VALUE1) as VALUE1 , rtrim(D.VALUE2) as VALUE2 , rtrim(D.VALUE3) as VALUE3 , rtrim(D.VALUE4) as VALUE4 , rtrim(D.VALUE5) as VALUE5 " _
                    & " FROM  S0005_AUTHOR A " _
                    & " INNER JOIN S0006_ROLE B " _
                    & "   ON    B.CAMPCODE    = A.CAMPCODE " _
                    & "   and   B.OBJECT      = 'ORG' " _
                    & "   and   B.ROLE        = A.ROLE " _
                    & "   and   B.STYMD      <= @P6 " _
                    & "   and   B.ENDYMD     >= @P6 " _
                    & "   and   B.DELFLG     <> '1' " _
                    & " INNER JOIN MC005_PRODORG C " _
                    & "   ON    C.CAMPCODE    = B.CAMPCODE " _
                    & "   and   C.UORG        = B.CODE " _
                    & "   and   C.DELFLG     <> '1' " _
                    & " INNER JOIN MC001_FIXVALUE D " _
                    & "   ON    D.KEYCODE     = C.OILTYPE " _
                    & "   and   D.STYMD      <= @P6 " _
                    & "   and   D.ENDYMD     >= @P6 " _
                    & "   and   D.DELFLG     <> '1' " _
                    & " Where   A.USERID      = @P1 " _
                    & "   and   A.OBJECT      = 'ORG' " _
                    & "   and   A.STYMD      <= @P6 " _
                    & "   and   A.ENDYMD     >= @P6 " _
                    & "   and   A.DELFLG     <> '1' " _
                    & "   and   B.CAMPCODE    = @P2 " _
                    & "   and   B.PERMITCODE >= 1 " _
                    & "   and   D.CLASS       = @P3 " _
                    & "   and   B.CODE        = @P4 " _
                    & "GROUP BY D.KEYCODE , D.VALUE1 , D.VALUE2 , D.VALUE3 , D.VALUE4 , D.VALUE5 " _
                    & "ORDER BY D.KEYCODE , D.VALUE1 , D.VALUE2 , D.VALUE3 , D.VALUE4 , D.VALUE5  "

            Dim SQLcmd As New SqlCommand(SQLStr, SQLcon)
            Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.NVarChar, 20)
            Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.NVarChar, 20)
            Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.NVarChar, 20)
            Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", System.Data.SqlDbType.NVarChar, 15)
            Dim PARA6 As SqlParameter = SQLcmd.Parameters.Add("@P6", System.Data.SqlDbType.Date)
            Dim PARA7 As SqlParameter = SQLcmd.Parameters.Add("@P7", System.Data.SqlDbType.Date)
            Dim PARA8 As SqlParameter = SQLcmd.Parameters.Add("@P8", System.Data.SqlDbType.Date)
            PARA1.Value = USERID
            PARA2.Value = CAMPCODE
            PARA3.Value = CLAS
            PARA4.Value = ORG
            PARA6.Value = Date.Now
            PARA7.Value = STYMD
            PARA8.Value = ENDYMD
            Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

            While SQLdr.Read
                If SQLdr("KEYCODE") <> "" Then
                    VALUE1.Items.Add(New ListItem(SQLdr("VALUE1"), SQLdr("KEYCODE")))
                    VALUE2.Items.Add(New ListItem(SQLdr("VALUE2"), SQLdr("KEYCODE")))
                    VALUE3.Items.Add(New ListItem(SQLdr("VALUE3"), SQLdr("KEYCODE")))
                    VALUE4.Items.Add(New ListItem(SQLdr("VALUE4"), SQLdr("KEYCODE")))
                    VALUE5.Items.Add(New ListItem(SQLdr("VALUE5"), SQLdr("KEYCODE")))

                    LISTBOX1.Items.Add(New ListItem(SQLdr("VALUE1"), SQLdr("KEYCODE")))
                    LISTBOX2.Items.Add(New ListItem(SQLdr("VALUE2"), SQLdr("KEYCODE")))
                    LISTBOX3.Items.Add(New ListItem(SQLdr("VALUE3"), SQLdr("KEYCODE")))
                    LISTBOX4.Items.Add(New ListItem(SQLdr("VALUE4"), SQLdr("KEYCODE")))
                    LISTBOX5.Items.Add(New ListItem(SQLdr("VALUE5"), SQLdr("KEYCODE")))
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
            Dim CS0011LOGWRITE As New CS0011LOGWrite            'LogOutput DirString Get
            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME            'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:MC005_PRODORG Select"
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
