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
''' Leftボックス用取引先取得（）
''' </summary>
''' <remarks>受注配車用</remarks>
Public Class GS0021TORISRVget
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
    ''' 取引先タイプ
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property TORITYPE() As String
    ''' <summary>
    ''' 出庫日
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property SHUKODATE() As Date
    ''' <summary>
    ''' 取引先CODE
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property TORICODE() As List(Of String)
    ''' <summary>
    ''' 取引先名
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property TORICODENAME() As List(Of String)
    ''' <summary>
    ''' 取引先一覧
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property LISTBOX() As ListBox
    ''' <summary>
    ''' メソッド名
    ''' </summary>
    ''' <remarks></remarks>
    Protected METHOD_NAME As String = "GS0021TORISRVget"
    ''' <summary>
    ''' 取引先取得
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub GS0021TORISRVget()
        '<< エラー説明 >>
        'ERR = OK:00000,ERR:00002(環境エラー),ERR:00003(DBerr)
        '●初期処理
        TORICODE = New List(Of String)
        TORICODENAME = New List(Of String)
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
        Dim SQLcmd As SqlCommand = Nothing
        Dim SQLdr As SqlDataReader = Nothing
        Try
            '●Leftボックス用取引先取得（APSRVOrg）
            Select Case TORITYPE
                Case "NI"
                    '○ セッション変数（APSRVOrg）に紐付く荷主データ取得
                    '検索SQL文
                    Dim SQLStr As String = _
                            "   SELECT rtrim(B.TORICODE)      as TORICODE ,       " _
                        & "              rtrim(B.NAMES) 	as NAMES  		    " _
                        & "         FROM MC003_TORIORG      as A 			    " _
                        & "   INNER JOIN MC002_TORIHIKISAKI as B 		        " _
                        & "           ON B.TORICODE          = A.TORICODE 		" _
                        & "          and B.STYMD            <= @P1 				" _
                        & "          and B.ENDYMD           >= @P1 				" _
                        & "          and B.DELFLG           <> '1' 				" _
                        & "   Where A.CAMPCODE               = @P2 				" _
                        & "          and A.UORG              = @P3          	" _
                        & "          and A.TORITYPE02        = @P4 				" _
                        & "          and A.DELFLG           <> '1' 				" _
                        & "   GROUP BY A.SEQ ,  			                    " _
                        & "              B.TORICODE ,                           " _
                        & "              B.NAMES                                " _
                        & "   ORDER BY A.SEQ ,  			                    " _
                        & "              B.TORICODE ,                           " _
                        & "              B.NAMES                                "

                    SQLcmd = New SqlCommand(SQLStr, SQLcon)
                    Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.Date)
                    Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.NVarChar, 20)
                    Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.NVarChar, 20)
                    Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", System.Data.SqlDbType.NVarChar, 3)

                    PARA1.Value = SHUKODATE
                    PARA2.Value = CAMPCODE
                    PARA3.Value = ORG
                    PARA4.Value = TORITYPE

                Case "YO"
                    '○ セッション変数（APSRVOrg）に紐付く庸車データ取得
                    '検索SQL文
                    Dim SQLStr As String = _
                            "   SELECT rtrim(B.TORICODE)  as TORICODE ,           " _
                        & "              rtrim(B.NAMES) 	as NAMES  		    " _
                        & "         FROM MC003_TORIORG      as A 			    " _
                        & "   INNER JOIN MC002_TORIHIKISAKI as B 		        " _
                        & "           ON B.TORICODE          = A.TORICODE 		" _
                        & "          and B.STYMD            <= @P1 				" _
                        & "          and B.ENDYMD           >= @P1 				" _
                        & "          and B.DELFLG           <> '1' 				" _
                        & "        Where A.CAMPCODE          = @P2 				" _
                        & "          and A.UORG              = @P3              " _
                        & "          and A.TORITYPE03        = @P4 				" _
                        & "          and A.DELFLG           <> '1' 				" _
                        & "   GROUP BY A.SEQ ,  			                    " _
                        & "              B.TORICODE ,                           " _
                        & "              B.NAMES                                " _
                        & "   ORDER BY A.SEQ ,  			                    " _
                        & "              B.TORICODE ,                           " _
                        & "              B.NAMES                                "

                    SQLcmd = New SqlCommand(SQLStr, SQLcon)
                    Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.Date)
                    Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.NVarChar, 20)
                    Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.NVarChar, 20)
                    Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", System.Data.SqlDbType.NVarChar, 3)

                    PARA1.Value = SHUKODATE
                    PARA2.Value = CAMPCODE
                    PARA3.Value = ORG
                    PARA4.Value = TORITYPE

                Case Else
                    '○ セッション変数（APSRVOrg）に紐付くデータ取得
                    '検索SQL文
                    Dim SQLStr As String = _
                            "   SELECT rtrim(B.TORICODE)  as TORICODE ,           " _
                        & "              rtrim(B.NAMES) 	as NAMES  		    " _
                        & "         FROM MC003_TORIORG      as A 			    " _
                        & "   INNER JOIN MC002_TORIHIKISAKI as B 		        " _
                        & "           ON B.TORICODE          = A.TORICODE 		" _
                        & "          and B.STYMD            <= @P1 				" _
                        & "          and B.ENDYMD           >= @P1 				" _
                        & "          and B.DELFLG           <> '1' 				" _
                        & "        Where A.CAMPCODE          = @P2 				" _
                        & "          and A.UORG              = @P3   			" _
                        & "          and A.DELFLG           <> '1' 				" _
                        & "   GROUP BY A.SEQ ,  			                    " _
                        & "              B.TORICODE ,                           " _
                        & "              B.NAMES                                " _
                        & "   ORDER BY A.SEQ ,  			                    " _
                        & "              B.TORICODE ,                           " _
                        & "              B.NAMES                                "

                    SQLcmd = New SqlCommand(SQLStr, SQLcon)
                    Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.Date)
                    Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.NVarChar, 20)
                    Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.NVarChar, 20)
                    Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", System.Data.SqlDbType.NVarChar, 20)

                    PARA1.Value = SHUKODATE
                    PARA2.Value = CAMPCODE
                    PARA3.Value = ORG
                    PARA4.Value = USERID
            End Select

            SQLdr = SQLcmd.ExecuteReader()

            While SQLdr.Read
                '○出力編集
                TORICODE.Add(SQLdr("TORICODE"))
                TORICODENAME.Add(SQLdr("NAMES"))
                LISTBOX.Items.Add(New ListItem(SQLdr("NAMES"), SQLdr("TORICODE")))
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
            CS0011LOGWRITE.INFPOSI = "DB:MC003_TORIORG Select"          '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try
    End Sub

End Class
