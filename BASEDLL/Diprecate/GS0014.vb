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
''' Leftボックス用品名取得
''' </summary>
''' <remarks></remarks>
Public Class GS0014PRODUCTget
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
    ''' 組織CODE
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property ORG() As String
    ''' <summary>
    ''' 油種
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property OILTYPE() As String
    ''' <summary>
    ''' 品名1
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property PRODUCT1() As String
    ''' <summary>
    ''' 油種一覧
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property OILTYPECODE() As List(Of String)
    ''' <summary>
    ''' 品名1一覧
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property PRODUCT1CODE() As List(Of String)
    ''' <summary>
    ''' 品名2一覧
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property PRODUCT2CODE() As List(Of String)
    ''' <summary>
    ''' 品名1名称一覧
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property PRODUCT2NAME() As List(Of String)
    ''' <summary>
    ''' 品名2一覧
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property LISTBOX() As ListBox


    Protected METHOD_NAME As String = "GS0014PRODUCTget"
    ''' <summary>
    ''' 品名一覧取得
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub GS0014PRODUCTget()
        '<< エラー説明 >>
        'ERR = OK:00000,ERR:00002(環境エラー),ERR:00003(DBerr)

        '●初期処理
        OILTYPECODE = New List(Of String)
        PRODUCT1CODE = New List(Of String)
        PRODUCT2CODE = New List(Of String)
        PRODUCT2NAME = New List(Of String)
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

        If IsNothing(LISTBOX) Then
            LISTBOX = New ListBox
        Else
            Try
                CType(LISTBOX, ListBox).Items.Clear()
            Catch ex As Exception
            End Try
        End If

        Try
            '●Leftボックス用品名取得
            Dim SQLcmd As New SqlCommand
            Dim PARA(10) As SqlParameter
            'DataBase接続文字
            Dim SQLcon = sm.getConnection
            SQLcon.Open() 'DataBase接続(Open)
            '検索SQL文
            Dim SQLStr As String = ""

            '○ User権限によりDB(MC005_PRODORG)検索
            If OILTYPE = "" And _
                PRODUCT1 = "" Then
                '検索SQL文
                SQLStr = _
                        "SELECT rtrim(C.OILTYPE) as OILTYPECODE , rtrim(C.PRODUCT1) as PRODUCT1CODE , rtrim(C.PRODUCT2) as PRODUCT2CODE , rtrim(D.NAMES) as PRODUCT2NAME " _
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
                        & " INNER JOIN MC004_PRODUCT D " _
                        & "   ON    D.OILTYPE     = C.OILTYPE " _
                        & "   and   D.PRODUCT1    = C.PRODUCT1 " _
                        & "   and   D.PRODUCT2    = C.PRODUCT2 " _
                        & "   and   D.STYMD      <= @P8 " _
                        & "   and   D.ENDYMD     >= @P7 " _
                        & "   and   D.DELFLG     <> '1' " _
                        & " Where   A.USERID      = @P1 " _
                        & "   and   A.OBJECT      = 'ORG' " _
                        & "   and   A.STYMD      <= @P6 " _
                        & "   and   A.ENDYMD     >= @P6 " _
                        & "   and   A.DELFLG     <> '1' " _
                        & "   and   B.CAMPCODE    = @P2 " _
                        & "   and   B.PERMITCODE >= 1 " _
                        & "   and   B.CODE        = @P3 " _
                        & "GROUP BY C.OILTYPE , C.PRODUCT1 , C.PRODUCT2 , D.NAMES " _
                        & "ORDER BY C.OILTYPE , C.PRODUCT1 , C.PRODUCT2 , D.NAMES "

                SQLcmd = New SqlCommand(SQLStr, SQLcon)
                PARA(1) = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.NVarChar, 20)
                PARA(2) = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.NVarChar, 20)
                PARA(3) = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.NVarChar, 15)
                PARA(6) = SQLcmd.Parameters.Add("@P6", System.Data.SqlDbType.Date)
                PARA(7) = SQLcmd.Parameters.Add("@P7", System.Data.SqlDbType.Date)
                PARA(8) = SQLcmd.Parameters.Add("@P8", System.Data.SqlDbType.Date)
                PARA(1).Value = USERID
                PARA(2).Value = CAMPCODE
                PARA(3).Value = ORG
                PARA(6).Value = Date.Now
                PARA(7).Value = STYMD
                PARA(8).Value = ENDYMD
            ElseIf OILTYPE <> "" And _
                PRODUCT1 = "" Then
                '検索SQL文
                SQLStr = _
                        "SELECT rtrim(C.OILTYPE) as OILTYPECODE , rtrim(C.PRODUCT1) as PRODUCT1CODE , rtrim(C.PRODUCT2) as PRODUCT2CODE , rtrim(D.NAMES) as PRODUCT2NAME " _
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
                        & " INNER JOIN MC004_PRODUCT D " _
                        & "   ON    D.OILTYPE     = C.OILTYPE " _
                        & "   and   D.PRODUCT1    = C.PRODUCT1 " _
                        & "   and   D.PRODUCT2    = C.PRODUCT2 " _
                        & "   and   D.STYMD      <= @P8 " _
                        & "   and   D.ENDYMD     >= @P7 " _
                        & "   and   D.DELFLG     <> '1' " _
                        & " Where   A.USERID      = @P1 " _
                        & "   and   A.OBJECT      = 'ORG' " _
                        & "   and   A.STYMD      <= @P6 " _
                        & "   and   A.ENDYMD     >= @P6 " _
                        & "   and   A.DELFLG     <> '1' " _
                        & "   and   B.CAMPCODE    = @P2 " _
                        & "   and   B.PERMITCODE >= 1 " _
                        & "   and   B.CODE        = @P3 " _
                        & "   and   C.OILTYPE     = @P4 " _
                        & "GROUP BY C.OILTYPE , C.PRODUCT1 , C.PRODUCT2 , D.NAMES " _
                        & "ORDER BY C.OILTYPE , C.PRODUCT1 , C.PRODUCT2 , D.NAMES "

                SQLcmd = New SqlCommand(SQLStr, SQLcon)
                PARA(1) = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.NVarChar, 20)
                PARA(2) = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.NVarChar, 20)
                PARA(3) = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.NVarChar, 15)
                PARA(4) = SQLcmd.Parameters.Add("@P4", System.Data.SqlDbType.NVarChar, 20)
                PARA(6) = SQLcmd.Parameters.Add("@P6", System.Data.SqlDbType.Date)
                PARA(7) = SQLcmd.Parameters.Add("@P7", System.Data.SqlDbType.Date)
                PARA(8) = SQLcmd.Parameters.Add("@P8", System.Data.SqlDbType.Date)
                PARA(1).Value = USERID
                PARA(2).Value = CAMPCODE
                PARA(3).Value = ORG
                PARA(4).Value = OILTYPE
                PARA(6).Value = Date.Now
                PARA(7).Value = STYMD
                PARA(8).Value = ENDYMD
            ElseIf OILTYPE <> "" And _
                PRODUCT1 <> "" Then
                '検索SQL文

                SQLStr = _
                        "SELECT rtrim(C.OILTYPE) as OILTYPECODE , rtrim(C.PRODUCT1) as PRODUCT1CODE , rtrim(C.PRODUCT2) as PRODUCT2CODE , rtrim(D.NAMES) as PRODUCT2NAME " _
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
                        & " INNER JOIN MC004_PRODUCT D " _
                        & "   ON    D.OILTYPE     = C.OILTYPE " _
                        & "   and   D.PRODUCT1    = C.PRODUCT1 " _
                        & "   and   D.PRODUCT2    = C.PRODUCT2 " _
                        & "   and   D.STYMD      <= @P8 " _
                        & "   and   D.ENDYMD     >= @P7 " _
                        & "   and   D.DELFLG     <> '1' " _
                        & " Where   A.USERID      = @P1 " _
                        & "   and   A.OBJECT      = 'ORG' " _
                        & "   and   A.STYMD      <= @P6 " _
                        & "   and   A.ENDYMD     >= @P6 " _
                        & "   and   A.DELFLG     <> '1' " _
                        & "   and   B.CAMPCODE    = @P2 " _
                        & "   and   B.PERMITCODE >= 1 " _
                        & "   and   B.CODE        = @P3 " _
                        & "   and   C.OILTYPE     = @P4 " _
                        & "   and   C.PRODUCT1    = @P5 " _
                        & "GROUP BY C.OILTYPE , C.PRODUCT1 , C.PRODUCT2 , D.NAMES " _
                        & "ORDER BY C.OILTYPE , C.PRODUCT1 , C.PRODUCT2 , D.NAMES "

                SQLcmd = New SqlCommand(SQLStr, SQLcon)
                PARA(1) = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.NVarChar, 20)
                PARA(2) = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.NVarChar, 20)
                PARA(3) = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.NVarChar, 15)
                PARA(4) = SQLcmd.Parameters.Add("@P4", System.Data.SqlDbType.NVarChar, 20)
                PARA(5) = SQLcmd.Parameters.Add("@P5", System.Data.SqlDbType.NVarChar, 20)
                PARA(6) = SQLcmd.Parameters.Add("@P6", System.Data.SqlDbType.Date)
                PARA(7) = SQLcmd.Parameters.Add("@P7", System.Data.SqlDbType.Date)
                PARA(8) = SQLcmd.Parameters.Add("@P8", System.Data.SqlDbType.Date)
                PARA(1).Value = USERID
                PARA(2).Value = CAMPCODE
                PARA(3).Value = ORG
                PARA(4).Value = OILTYPE
                PARA(5).Value = PRODUCT1
                PARA(6).Value = Date.Now
                PARA(7).Value = STYMD
                PARA(8).Value = ENDYMD

            End If

            Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()


            While SQLdr.Read

                '○出力編集
                OILTYPECODE.Add(SQLdr("OILTYPECODE"))
                PRODUCT1CODE.Add(SQLdr("PRODUCT1CODE"))
                PRODUCT2CODE.Add(SQLdr("PRODUCT2CODE"))
                PRODUCT2NAME.Add(SQLdr("PRODUCT2NAME"))
                LISTBOX.Items.Add(New ListItem(SQLdr("PRODUCT2NAME"), SQLdr("PRODUCT2CODE")))
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
            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME            'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:MC005_PRODORG Select"           '
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
