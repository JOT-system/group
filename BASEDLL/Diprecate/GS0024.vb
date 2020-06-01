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
''' Leftボックス用車番取得
''' </summary>
''' <remarks>受注配車用</remarks>
Public Class GS0024SHABANSRVget
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
    ''' 出庫日
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property SHUKODATE() As Date
    ''' <summary>
    ''' 車番一覧
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property SHABAN() As List(Of String)
    ''' <summary>
    ''' ナンバー一覧
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property LICNPLTNO() As List(Of String)
    ''' <summary>
    ''' 車番一覧
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property LISTBOX() As ListBox

    Protected METHOD_NAME As String = "GS0024SHABANSRVget"
    ''' <summary>
    ''' 車番取得
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub GS0024SHABANSRVget()
        '<< エラー説明 >>
        'O_ERR = OK:00000,ERR:00002(環境エラー),ERR:00003(DBerr)
        '●初期処理
        SHABAN = New List(Of String)
        LICNPLTNO = New List(Of String)
        'セッション制御宣言
        Dim sm As New CS0050SESSION
        'PARAM EXTRA01: USERID
        If IsNothing(USERID) Then
            USERID = sm.USERID
        End If
        'PARAM EXTRA02: ORG
        If ORG = "" Or IsNothing(ORG) Then
            ORG = sm.APSV_ORG
        End If
        'PARAM EXTRA03: SHUKODATE
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
        Dim SQLcmd As SqlCommand
        '●Leftボックス用車番取得（APSRVOrg）
        Try
            If OILTYPE <> "" Then
                '○ セッション変数（APSRVOrg）に紐付くデータ取得
                '   [油種]に値が存在する場合
                Dim SQLStr As String = _
                        "       SELECT rtrim(A.GSHABAN) 	    as GSHABAN ,   		" _
                    & "              rtrim(C.LICNPLTNO1) +                      " _
                    & "              rtrim(C.LICNPLTNO2)    as LICNPLTNOF ,     " _
                    & "              rtrim(D.LICNPLTNO1) +                      " _
                    & "              rtrim(D.LICNPLTNO2)    as LICNPLTNOB       " _
                    & "   FROM       MA006_SHABANORG   as A                     " _
                    & "   INNER JOIN MA002_SHARYOA  as B 						" _
                    & "           ON B.CAMPCODE   	= A.CAMPCODE 				" _
                    & "          and B.SHARYOTYPE   = A.SHARYOTYPEB 		    " _
                    & "          and B.TSHABAN      = A.TSHABANB 		        " _
                    & "          and B.MANGOILTYPE  = @P3 		                " _
                    & "          and B.STYMD       <= @P1                       " _
                    & "          and B.ENDYMD      >= @P1                       " _
                    & "          and B.DELFLG      <> '1' 						" _
                    & "    LEFT JOIN MA004_SHARYOC  as C 						" _
                    & "           ON C.CAMPCODE   	= A.CAMPCODE 				" _
                    & "          and C.SHARYOTYPE   = A.SHARYOTYPEF 		    " _
                    & "          and C.TSHABAN      = A.TSHABANF 			    " _
                    & "          and C.STYMD       <= @P1                       " _
                    & "          and C.ENDYMD      >= @P1                       " _
                    & "          and C.DELFLG      <> '1' 						" _
                    & "    LEFT JOIN MA004_SHARYOC  as D 						" _
                    & "           ON D.CAMPCODE   	= A.CAMPCODE 				" _
                    & "          and D.SHARYOTYPE   = A.SHARYOTYPEB 		    " _
                    & "          and D.TSHABAN      = A.TSHABANB 	            " _
                    & "          and D.STYMD       <= @P1                       " _
                    & "          and D.ENDYMD      >= @P1                       " _
                    & "          and D.DELFLG      <> '1' 						" _
                    & "        Where A.CAMPCODE   = @P2                         " _
                    & "          and A.MANGUORG   = @P4                         " _
                    & "          and A.DELFLG    <> '1'                         " _
                    & "     ORDER BY A.SEQ ,                                    " _
                    & "              A.GSHABAN                                  "

                SQLcmd = New SqlCommand(SQLStr, SQLcon)
                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.Date)
                Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", System.Data.SqlDbType.NVarChar, 20)

                PARA1.Value = SHUKODATE
                PARA2.Value = CAMPCODE
                PARA3.Value = OILTYPE
                PARA4.Value = ORG

            Else
                '○ セッション変数（APSRVOrg）に紐付くデータ取得
                '   [油種]に値が存在しない場合
                Dim SQLStr As String = _
                        "       SELECT rtrim(A.GSHABAN) 	    as GSHABAN ,   		" _
                    & "              rtrim(C.LICNPLTNO1) +                      " _
                    & "              rtrim(C.LICNPLTNO2)    as LICNPLTNOF ,     " _
                    & "              rtrim(D.LICNPLTNO1) +                      " _
                    & "              rtrim(D.LICNPLTNO2)    as LICNPLTNOB       " _
                    & "   FROM       MA006_SHABANORG   as A                     " _
                    & "    LEFT JOIN MA002_SHARYOA  as B 						" _
                    & "           ON B.CAMPCODE   	= A.CAMPCODE 				" _
                    & "          and B.SHARYOTYPE   = A.SHARYOTYPEB 		    " _
                    & "          and B.TSHABAN      = A.TSHABANB 		        " _
                    & "          and B.STYMD       <= @P1                       " _
                    & "          and B.ENDYMD      >= @P1                       " _
                    & "          and B.DELFLG      <> '1' 						" _
                    & "    LEFT JOIN MA004_SHARYOC  as C 						" _
                    & "           ON C.CAMPCODE   	= A.CAMPCODE 				" _
                    & "          and C.SHARYOTYPE   = A.SHARYOTYPEF 		    " _
                    & "          and C.TSHABAN      = A.TSHABANF 			    " _
                    & "          and C.STYMD       <= @P1                       " _
                    & "          and C.ENDYMD      >= @P1                       " _
                    & "          and C.DELFLG      <> '1' 						" _
                    & "    LEFT JOIN MA004_SHARYOC  as D 						" _
                    & "           ON D.CAMPCODE   	= A.CAMPCODE 				" _
                    & "          and D.SHARYOTYPE   = A.SHARYOTYPEB 		    " _
                    & "          and D.TSHABAN      = A.TSHABANB 	            " _
                    & "          and D.STYMD       <= @P1                       " _
                    & "          and D.ENDYMD      >= @P1                       " _
                    & "          and D.DELFLG      <> '1' 						" _
                    & "        Where A.CAMPCODE   = @P2                         " _
                    & "          and A.MANGUORG   = @P4                         " _
                    & "          and A.DELFLG    <> '1'                         " _
                    & "     ORDER BY A.SEQ ,                                    " _
                    & "              A.GSHABAN                                  "

                SQLcmd = New SqlCommand(SQLStr, SQLcon)
                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.Date)
                Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", System.Data.SqlDbType.NVarChar, 20)

                PARA1.Value = SHUKODATE
                PARA2.Value = CAMPCODE
                PARA3.Value = OILTYPE
                PARA4.Value = ORG
            End If

            Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

            While SQLdr.Read
                '○出力編集
                If IsDBNull(SQLdr("LICNPLTNOF")) And
                    IsDBNull(SQLdr("LICNPLTNOB")) Then
                    SHABAN.Add(SQLdr("GSHABAN"))
                    LICNPLTNO.Add("")
                    LISTBOX.Items.Add(New ListItem("", SQLdr("GSHABAN")))
                ElseIf Not IsDBNull(SQLdr("LICNPLTNOF")) Then
                    SHABAN.Add(SQLdr("GSHABAN"))
                    LICNPLTNO.Add(SQLdr("LICNPLTNOF"))
                    LISTBOX.Items.Add(New ListItem(SQLdr("LICNPLTNOF"), SQLdr("GSHABAN")))
                Else
                    SHABAN.Add(SQLdr("GSHABAN"))
                    LICNPLTNO.Add(SQLdr("LICNPLTNOB"))
                    LISTBOX.Items.Add(New ListItem(SQLdr("LICNPLTNOB"), SQLdr("GSHABAN")))
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
            CS0011LOGWRITE.INFPOSI = "DB:MA006_SHABANORG Select"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try


    End Sub

End Class
