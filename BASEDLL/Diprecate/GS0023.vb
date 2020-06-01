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
''' Leftボックス用品名２取得（APSRVOrg）
''' </summary>
''' <remarks></remarks>
Public Class GS0023PRODUCTSRVget
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
    ''' 出庫日
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property SHUKODATE() As Date
    ''' <summary>
    ''' 油種コード一覧
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property OILTYPECODE() As List(Of String)
    ''' <summary>
    ''' 品名1コード一覧
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property PRODUCT1CODE() As List(Of String)
    ''' <summary>
    ''' 品名2コード一覧
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property PRODUCT2CODE() As List(Of String)
    ''' <summary>
    ''' 品名2名称一覧
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property PRODUCT2NAME() As List(Of String)
    ''' <summary>
    ''' 品名情報一覧
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks>品名コード：品名1</remarks>
    Public Property LISTBOX1() As ListBox
    ''' <summary>
    ''' 品名情報一覧
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks>品名コード：品名1＋品名2</remarks>
    Public Property LISTBOX2() As ListBox
    ''' <summary>
    ''' 配送単位一覧
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property HTANI() As List(Of String)

    Protected METHOD_NAME As String = "GS0023PRODUCTSRVget"
    ''' <summary>
    ''' 品名２取得（APSRVOrg）
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub GS0023PRODUCTSRVget()
        '<< エラー説明 >>
        'O_ERR = OK:00000,ERR:00002(環境エラー),ERR:00003(DBerr)

        '●初期処理
        OILTYPECODE = New List(Of String)
        PRODUCT1CODE = New List(Of String)
        PRODUCT2CODE = New List(Of String)
        PRODUCT2NAME = New List(Of String)
        HTANI = New List(Of String)
        'セッション制御宣言
        Dim sm As New CS0050SESSION
        'PARAM EXTRA01: USERID
        If IsNothing(USERID) Then
            USERID = sm.USERID
        End If
        'PARAM EXTRA02: ORG
        If ORG = "" Or IsNothing(ORG) Then
            ORG = sm.APSV_ID
        End If
        'PARAM EXTRA03: SHUKODATE
        If SHUKODATE < C_DEFAULT_YMD Then
            SHUKODATE = Date.Now
        End If

        If IsNothing(LISTBOX1) Then
            LISTBOX1 = New ListBox
        Else
            Try
                CType(LISTBOX1, ListBox).Items.Clear()
            Catch ex As Exception
            End Try
        End If

        If IsNothing(LISTBOX2) Then
            LISTBOX2 = New ListBox
        Else
            Try
                CType(LISTBOX2, ListBox).Items.Clear()
            Catch ex As Exception
            End Try
        End If

        '●Leftボックス用品名取得（APSRVOrg）
        'DataBase接続文字
        Dim SQLcon = sm.getConnection
        SQLcon.Open() 'DataBase接続(Open)
        Dim SQLcmd As SqlCommand = Nothing
        Try
            If OILTYPE = "" Then
                '○ セッション変数（APSRVOrg）に紐付くデータ取得
                '   [油種]に値が存在しない場合
                Dim SQLStr As String = _
                            "       SELECT rtrim(A.OILTYPE)    as OILTYPECODE ,    " _
                        & "              rtrim(A.PRODUCT1)   as PRODUCT1CODE ,   " _
                        & "              rtrim(A.PRODUCT2)   as PRODUCT2CODE ,   " _
                        & "              rtrim(B.NAMES)      as PRODUCT2NAME ,   " _
                        & "              isnull(rtrim(B.STANI),'')  as STANI ,   " _
                        & "              isnull(rtrim(A.HTANI),'')  as HTANI     " _
                        & "         FROM MC005_PRODORG   as A                    " _
                        & "   INNER JOIN MC004_PRODUCT   as B                    " _
                        & "           ON B.OILTYPE   = A.OILTYPE                 " _
                        & "          and B.PRODUCT1  = A.PRODUCT1                " _
                        & "          and B.PRODUCT2  = A.PRODUCT2                " _
                        & "          and B.STYMD    <= @P1                       " _
                        & "          and B.ENDYMD   >= @P1                       " _
                        & "          and B.DELFLG   <> '1'                       " _
                        & "        Where A.CAMPCODE  = @P2                       " _
                        & "          and A.UORG      = @P3                       " _
                        & "          and A.STYMD    <= @P1                       " _
                        & "          and A.ENDYMD   >= @P1                       " _
                        & "          and A.DELFLG   <> '1'                       " _
                        & "     GROUP BY A.OILTYPE ,                             " _
                        & "              A.PRODUCT1 ,                            " _
                        & "              A.PRODUCT2 ,                            " _
                        & "              B.NAMES ,                               " _
                        & "              B.STANI ,                               " _
                        & "              A.HTANI                                 " _
                        & "     ORDER BY A.OILTYPE ,                             " _
                        & "              A.PRODUCT1 ,                            " _
                        & "              A.PRODUCT2 ,                            " _
                        & "              B.NAMES ,                               " _
                        & "              B.STANI ,                               " _
                        & "              A.HTANI                                 "


                SQLcmd = New SqlCommand(SQLStr, SQLcon)
                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.Date)
                Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.NVarChar, 20)
                PARA1.Value = SHUKODATE
                PARA2.Value = CAMPCODE
                PARA3.Value = ORG

            Else
                If PRODUCT1 = "" Then
                    '○ セッション変数（APSRVOrg）に紐付くデータ取得
                    '   [油種]に値が存在する場合、かつ、[品名1]に値が存在しない場合
                    Dim SQLStr As String = _
                                "       SELECT rtrim(A.OILTYPE)    as OILTYPECODE ,    " _
                            & "              rtrim(A.PRODUCT1)   as PRODUCT1CODE ,   " _
                            & "              rtrim(A.PRODUCT2)   as PRODUCT2CODE ,   " _
                            & "              rtrim(B.NAMES)      as PRODUCT2NAME ,   " _
                            & "              isnull(rtrim(B.STANI),'')  as STANI ,   " _
                            & "              isnull(rtrim(A.HTANI),'')  as HTANI     " _
                            & "         FROM MC005_PRODORG   as A                    " _
                            & "   INNER JOIN MC004_PRODUCT   as B                    " _
                            & "           ON B.OILTYPE   = A.OILTYPE                 " _
                            & "          and B.PRODUCT1  = A.PRODUCT1                " _
                            & "          and B.PRODUCT2  = A.PRODUCT2                " _
                            & "          and B.STYMD    <= @P1                       " _
                            & "          and B.ENDYMD   >= @P1                       " _
                            & "          and B.DELFLG   <> '1'                       " _
                            & "        Where A.CAMPCODE  = @P2                       " _
                            & "          and A.UORG      = @P3                       " _
                            & "          and A.OILTYPE   = @P4                       " _
                            & "          and A.STYMD    <= @P1                       " _
                            & "          and A.ENDYMD   >= @P1                       " _
                            & "          and A.DELFLG   <> '1'                       " _
                            & "     GROUP BY A.OILTYPE ,                             " _
                            & "              A.PRODUCT1 ,                            " _
                            & "              A.PRODUCT2 ,                            " _
                            & "              B.NAMES ,                               " _
                            & "              B.STANI ,                               " _
                            & "              A.HTANI                                 " _
                            & "     ORDER BY A.OILTYPE ,                             " _
                            & "              A.PRODUCT1 ,                            " _
                            & "              A.PRODUCT2 ,                            " _
                            & "              B.NAMES ,                               " _
                            & "              B.STANI ,                               " _
                            & "              A.HTANI                                 "

                    SQLcmd = New SqlCommand(SQLStr, SQLcon)
                    Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.Date)
                    Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.NVarChar, 20)
                    Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.NVarChar, 20)
                    Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", System.Data.SqlDbType.NVarChar, 20)
                    PARA1.Value = SHUKODATE
                    PARA2.Value = CAMPCODE
                    PARA3.Value = ORG
                    PARA4.Value = OILTYPE
                Else
                    '○ セッション変数（APSRVOrg）に紐付くデータ取得
                    '   [油種]に値が存在する場合、かつ、[品名1]に値が存在する場合
                    Dim SQLStr As String = _
                                "       SELECT rtrim(A.OILTYPE)    as OILTYPECODE ,    " _
                            & "              rtrim(A.PRODUCT1)   as PRODUCT1CODE ,   " _
                            & "              rtrim(A.PRODUCT2)   as PRODUCT2CODE ,   " _
                            & "              rtrim(B.NAMES)      as PRODUCT2NAME ,   " _
                            & "              isnull(rtrim(B.STANI),'')  as STANI ,   " _
                            & "              isnull(rtrim(A.HTANI),'')  as HTANI     " _
                            & "         FROM MC005_PRODORG   as A                    " _
                            & "   INNER JOIN MC004_PRODUCT   as B                    " _
                            & "           ON B.OILTYPE   = A.OILTYPE                 " _
                            & "          and B.PRODUCT1  = A.PRODUCT1                " _
                            & "          and B.PRODUCT2  = A.PRODUCT2                " _
                            & "          and B.STYMD    <= @P1                       " _
                            & "          and B.ENDYMD   >= @P1                       " _
                            & "          and B.DELFLG   <> '1'                       " _
                            & "        Where A.CAMPCODE  = @P2                       " _
                            & "          and A.UORG      = @P3                       " _
                            & "          and A.OILTYPE   = @P4                       " _
                            & "          and A.PRODUCT1  = @P5                       " _
                            & "          and A.STYMD    <= @P1                       " _
                            & "          and A.ENDYMD   >= @P1                       " _
                            & "          and A.DELFLG   <> '1'                       " _
                            & "     GROUP BY A.OILTYPE ,                             " _
                            & "              A.PRODUCT1 ,                            " _
                            & "              A.PRODUCT2 ,                            " _
                            & "              B.NAMES ,                               " _
                            & "              B.STANI ,                               " _
                            & "              A.HTANI                                 " _
                            & "     ORDER BY A.OILTYPE ,                             " _
                            & "              A.PRODUCT1 ,                            " _
                            & "              A.PRODUCT2 ,                            " _
                            & "              B.NAMES ,                               " _
                            & "              B.STANI ,                               " _
                            & "              A.HTANI                                 "

                    SQLcmd = New SqlCommand(SQLStr, SQLcon)
                    Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.Date)
                    Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.NVarChar, 20)
                    Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.NVarChar, 20)
                    Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", System.Data.SqlDbType.NVarChar, 20)
                    Dim PARA5 As SqlParameter = SQLcmd.Parameters.Add("@P5", System.Data.SqlDbType.NVarChar, 20)
                    Dim PARA6 As SqlParameter = SQLcmd.Parameters.Add("@P6", System.Data.SqlDbType.NVarChar, 20)

                    PARA1.Value = SHUKODATE
                    PARA2.Value = CAMPCODE
                    PARA3.Value = ORG
                    PARA4.Value = OILTYPE
                    PARA5.Value = PRODUCT1
                    PARA6.Value = USERID
                End If
            End If

            Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

            While SQLdr.Read
                '○出力編集
                OILTYPECODE.Add(SQLdr("OILTYPECODE"))
                PRODUCT1CODE.Add(SQLdr("PRODUCT1CODE"))
                PRODUCT2CODE.Add(SQLdr("PRODUCT2CODE"))
                PRODUCT2NAME.Add(SQLdr("PRODUCT2NAME"))
                LISTBOX1.Items.Add(New ListItem(SQLdr("PRODUCT2NAME"), SQLdr("PRODUCT2CODE")))
                LISTBOX2.Items.Add(New ListItem(SQLdr("PRODUCT2NAME"), SQLdr("PRODUCT1CODE") & SQLdr("PRODUCT2CODE")))
                '配送単位がスペースの場合、請求単位を設定
                If SQLdr("HTANI") = "" Then
                    HTANI.Add(SQLdr("STANI"))
                Else
                    HTANI.Add(SQLdr("HTANI"))
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
            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME          'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:MC005_PRODORG Select"          '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            ERR = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try
    End Sub

End Class
