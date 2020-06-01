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
''' Leftボックス用作業部署グループ取得  
''' </summary>
''' <remarks>ログインユーザが設定した作業部署グループを取得する</remarks>
Public Class GS0019SORGGROUPget
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
    ''' 組織CODE
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property ORGCODE() As String
    ''' <summary>
    ''' グループCODE
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property OGRCODE() As List(Of String)
    ''' <summary>
    ''' グループ名称
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property ONODENAMES() As List(Of String)
    ''' <summary>
    ''' グループ一覧
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property LISTBOX() As ListBox

    Protected METHOD_NAME As String = "GS0019SORGGROUPget"
    ''' <summary>
    ''' Leftボックス用作業部署のグループ取得 
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub GS0019SORGGROUPget()
        '<< エラー説明 >>
        'ERR = OK:00000,ERR:00002(環境エラー),ERR:00003(DBerr)
        '●初期処理
        GRCODE = New List(Of String)
        NODENAMES = New List(Of String)
        'セッション制御宣言
        Dim sm As New CS0050SESSION
        'PARAM EXTRA01: USERID
        If IsNothing(USERID) Then
            USERID = sm.USERID
        End If
        '●Leftボックス用作業部署グループ取得
        Try
            'DataBase接続文字
            Dim SQLcon = sm.getConnection
            SQLcon.Open() 'DataBase接続(Open)

            '検索SQL文
            Dim SQLStr As String = _
                    "SELECT rtrim(W.CAMPCODE) as CAMPCODE ,rtrim(W.GRCODE) as CODE , rtrim(W.NODENAMES) as NAMES " _
                & " FROM  ( " _
                & "    SELECT " _
                & "       M2.CAMPCODE" _
                & "      ,M7.GRCODE" _
                & " 	 ,M7.NODENAMES	" _
                & "    FROM " _
                & "       M0002_ORG M2 " _
                & "	     INNER JOIN M0006_STRUCT M6" _
                & "	     ON M6.CAMPCODE = M2.CAMPCODE" _
                & "	     and M6.OBJECT = 'ORG'" _
                & "	     and M6.CODE = M2.ORGCODE" _
                & "	     and M6.STYMD <= @P2" _
                & "	     and M6.ENDYMD >= @P2" _
                & "	     and M6.DELFLG <> '1' " _
                & "	     and M6.GRCODE01 IS NOT NULL " _
                & "	     INNER JOIN M0007_GROUP M7 " _
                & "	     ON M7.CAMPCODE = M6.CAMPCODE " _
                & "	     and M7.STRUCT = M6.STRUCT" _
                & "	     and M7.OBJECT = M6.OBJECT" _
                & "	     and M7.USERID = @P1" _
                & "	     and M7.GRCODE = M6.GRCODE01" _
                & "	     and M7.DELFLG <> '1'" _
                & "    WHERE " _
                & "	     1=1 " _
                & "	     and M2.ORGLEVEL = '00010' " _
                & "    GROUP BY " _
                & "       M2.CAMPCODE " _
                & "      ,M7.GRCODE " _
                & "	    ,M7.NODENAMES	" _
                & "    UNION " _
                & "    SELECT " _
                & "       M2.CAMPCODE" _
                & "      ,M7.GRCODE" _
                & " 	    ,M7.NODENAMES	" _
                & "    FROM " _
                & "       M0002_ORG M2" _
                & "	     INNER JOIN M0006_STRUCT M6" _
                & "	     ON M6.CAMPCODE = M2.CAMPCODE" _
                & "	     and M6.OBJECT = 'ORG'" _
                & "	     and M6.CODE = M2.ORGCODE" _
                & "	     and M6.STYMD <= @P2" _
                & "	     and M6.ENDYMD >= @P2" _
                & "	     and M6.DELFLG <> '1' " _
                & "	     and M6.GRCODE02 IS NOT NULL " _
                & "	     INNER JOIN M0007_GROUP M7 " _
                & "	     ON M7.CAMPCODE = M6.CAMPCODE " _
                & "	     and M7.STRUCT = M6.STRUCT" _
                & "	     and M7.OBJECT = M6.OBJECT" _
                & "	     and M7.USERID = @P1" _
                & "	     and M7.GRCODE = M6.GRCODE02" _
                & "	     and M7.DELFLG <> '1'" _
                & "    WHERE " _
                & "	     1=1 " _
                & "	     and M2.ORGLEVEL = '00010' " _
                & "    GROUP BY " _
                & "       M2.CAMPCODE " _
                & "      ,M7.GRCODE " _
                & "	    ,M7.NODENAMES	" _
                & "    UNION " _
                & "    SELECT " _
                & "       M2.CAMPCODE" _
                & "      ,M7.GRCODE" _
                & " 	 ,M7.NODENAMES	" _
                & "    FROM " _
                & "       M0002_ORG M2" _
                & "	     INNER JOIN M0006_STRUCT M6" _
                & "	     ON M6.CAMPCODE = M2.CAMPCODE" _
                & "	     and M6.OBJECT = 'ORG'" _
                & "	     and M6.CODE = M2.ORGCODE" _
                & "	     and M6.STYMD <= @P2" _
                & "	     and M6.ENDYMD >= @P2" _
                & "	     and M6.DELFLG <> '1' " _
                & "	     and M6.GRCODE03 IS NOT NULL " _
                & "	     INNER JOIN M0007_GROUP M7 " _
                & "	     ON M7.CAMPCODE = M6.CAMPCODE " _
                & "	     and M7.STRUCT = M6.STRUCT" _
                & "	     and M7.OBJECT = M6.OBJECT" _
                & "	     and M7.USERID = @P1" _
                & "	     and M7.GRCODE = M6.GRCODE03" _
                & "	     and M7.DELFLG <> '1'" _
                & "    WHERE " _
                & "	     1=1 " _
                & "	     and M2.ORGLEVEL = '00010' " _
                & "    GROUP BY " _
                & "       M2.CAMPCODE " _
                & "      ,M7.GRCODE " _
                & "	    ,M7.NODENAMES	" _
                & "    UNION " _
                & "    SELECT " _
                & "       M2.CAMPCODE" _
                & "      ,M7.GRCODE" _
                & " 	    ,M7.NODENAMES	" _
                & "    FROM " _
                & "       M0002_ORG M2" _
                & "	     INNER JOIN M0006_STRUCT M6" _
                & "	     ON M6.CAMPCODE = M2.CAMPCODE" _
                & "	     and M6.OBJECT = 'ORG'" _
                & "	     and M6.CODE = M2.ORGCODE" _
                & "	     and M6.STYMD <= @P2" _
                & "	     and M6.ENDYMD >= @P2" _
                & "	     and M6.DELFLG <> '1' " _
                & "	     and M6.GRCODE04 IS NOT NULL " _
                & "	     INNER JOIN M0007_GROUP M7 " _
                & "	     ON M7.CAMPCODE = M6.CAMPCODE " _
                & "	     and M7.STRUCT = M6.STRUCT" _
                & "	     and M7.OBJECT = M6.OBJECT" _
                & "	     and M7.USERID = @P1" _
                & "	     and M7.GRCODE = M6.GRCODE04" _
                & "	     and M7.DELFLG <> '1'" _
                & "    WHERE " _
                & "	     1=1 " _
                & "	     and M2.ORGLEVEL = '00010' " _
                & "    GROUP BY " _
                & "       M2.CAMPCODE " _
                & "      ,M7.GRCODE " _
                & "	    ,M7.NODENAMES	" _
                & "    UNION " _
                & "    SELECT " _
                & "       M2.CAMPCODE" _
                & "      ,M7.GRCODE" _
                & " 	 ,M7.NODENAMES	" _
                & "    FROM " _
                & "       M0002_ORG M2" _
                & "	     INNER JOIN M0006_STRUCT M6" _
                & "	     ON M6.CAMPCODE = M2.CAMPCODE" _
                & "	     and M6.OBJECT = 'ORG'" _
                & "	     and M6.CODE = M2.ORGCODE" _
                & "	     and M6.STYMD <= @P2" _
                & "	     and M6.ENDYMD >= @P2" _
                & "	     and M6.DELFLG <> '1' " _
                & "	     and M6.GRCODE05 IS NOT NULL " _
                & "	     INNER JOIN M0007_GROUP M7 " _
                & "	     ON M7.CAMPCODE = M6.CAMPCODE " _
                & "	     and M7.STRUCT = M6.STRUCT" _
                & "	     and M7.OBJECT = M6.OBJECT" _
                & "	     and M7.USERID = @P1" _
                & "	     and M7.GRCODE = M6.GRCODE05" _
                & "	     and M7.DELFLG <> '1'" _
                & "    WHERE " _
                & "	     1=1 " _
                & "	     and M2.ORGLEVEL = '00010' " _
                & "    GROUP BY " _
                & "       M2.CAMPCODE " _
                & "      ,M7.GRCODE " _
                & "	     ,M7.NODENAMES	" _
                & "    UNION " _
                & "    SELECT " _
                & "       M2.CAMPCODE" _
                & "      ,M7.GRCODE" _
                & " 	 ,M7.NODENAMES	" _
                & "    FROM " _
                & "       M0002_ORG M2" _
                & "	     INNER JOIN M0006_STRUCT M6" _
                & "	     ON M6.CAMPCODE = M2.CAMPCODE" _
                & "	     and M6.OBJECT = 'ORG'" _
                & "	     and M6.CODE = M2.ORGCODE" _
                & "	     and M6.STYMD <= @P2" _
                & "	     and M6.ENDYMD >= @P2" _
                & "	     and M6.DELFLG <> '1' " _
                & "	     and M6.GRCODE06 IS NOT NULL " _
                & "	     INNER JOIN M0007_GROUP M7 " _
                & "	     ON M7.CAMPCODE = M6.CAMPCODE " _
                & "	     and M7.STRUCT = M6.STRUCT" _
                & "	     and M7.OBJECT = M6.OBJECT" _
                & "	     and M7.USERID = @P1" _
                & "	     and M7.GRCODE = M6.GRCODE06" _
                & "	     and M7.DELFLG <> '1'" _
                & "    WHERE " _
                & "	     1=1 " _
                & "	     and M2.ORGLEVEL = '00010' " _
                & "    GROUP BY " _
                & "       M2.CAMPCODE " _
                & "      ,M7.GRCODE " _
                & "	    ,M7.NODENAMES	" _
                & "    UNION " _
                & "    SELECT " _
                & "       M2.CAMPCODE" _
                & "      ,M7.GRCODE" _
                & "      ,M7.NODENAMES	" _
                & "    FROM " _
                & "       M0002_ORG M2" _
                & "	     INNER JOIN M0006_STRUCT M6" _
                & "	     ON M6.CAMPCODE = M2.CAMPCODE" _
                & "	     and M6.OBJECT = 'ORG'" _
                & "	     and M6.CODE = M2.ORGCODE" _
                & "	     and M6.STYMD <= @P2" _
                & "	     and M6.ENDYMD >= @P2" _
                & "	     and M6.DELFLG <> '1' " _
                & "	     and M6.GRCODE07 IS NOT NULL " _
                & "	     INNER JOIN M0007_GROUP M7 " _
                & "	     ON M7.CAMPCODE = M6.CAMPCODE " _
                & "	     and M7.STRUCT = M6.STRUCT" _
                & "	     and M7.OBJECT = M6.OBJECT" _
                & "	     and M7.USERID = @P1" _
                & "	     and M7.GRCODE = M6.GRCODE07" _
                & "	     and M7.DELFLG <> '1'" _
                & "    WHERE " _
                & "	     1=1 " _
                & "	     and M2.ORGLEVEL = '00010' " _
                & "    GROUP BY " _
                & "       M2.CAMPCODE " _
                & "      ,M7.GRCODE " _
                & "	    ,M7.NODENAMES	" _
                & "    UNION " _
                & "    SELECT " _
                & "       M2.CAMPCODE" _
                & "      ,M7.GRCODE" _
                & " 	 ,M7.NODENAMES	" _
                & "    FROM " _
                & "       M0002_ORG M2" _
                & "	     INNER JOIN M0006_STRUCT M6" _
                & "	     ON M6.CAMPCODE = M2.CAMPCODE" _
                & "	     and M6.OBJECT = 'ORG'" _
                & "	     and M6.CODE = M2.ORGCODE" _
                & "	     and M6.STYMD <= @P2" _
                & "	     and M6.ENDYMD >= @P2" _
                & "	     and M6.DELFLG <> '1' " _
                & "	     and M6.GRCODE08 IS NOT NULL " _
                & "	     INNER JOIN M0007_GROUP M7 " _
                & "	     ON M7.CAMPCODE = M6.CAMPCODE " _
                & "	     and M7.STRUCT = M6.STRUCT" _
                & "	     and M7.OBJECT = M6.OBJECT" _
                & "	     and M7.USERID = @P1" _
                & "	     and M7.GRCODE = M6.GRCODE08" _
                & "	     and M7.DELFLG <> '1'" _
                & "    WHERE " _
                & "	     1=1 " _
                & "	     and M2.ORGLEVEL = '00010' " _
                & "    GROUP BY " _
                & "       M2.CAMPCODE " _
                & "      ,M7.GRCODE " _
                & "	    ,M7.NODENAMES	" _
                & "    UNION " _
                & "    SELECT " _
                & "       M2.CAMPCODE" _
                & "      ,M7.GRCODE" _
                & " 	 ,M7.NODENAMES	" _
                & "    FROM " _
                & "       M0002_ORG M2" _
                & "	     INNER JOIN M0006_STRUCT M6" _
                & "	     ON M6.CAMPCODE = M2.CAMPCODE" _
                & "	     and M6.OBJECT = 'ORG'" _
                & "	     and M6.CODE = M2.ORGCODE" _
                & "	     and M6.STYMD <= @P2" _
                & "	     and M6.ENDYMD >= @P2" _
                & "	     and M6.DELFLG <> '1' " _
                & "	     and M6.GRCODE09 IS NOT NULL " _
                & "	     INNER JOIN M0007_GROUP M7 " _
                & "	     ON M7.CAMPCODE = M6.CAMPCODE " _
                & "	     and M7.STRUCT = M6.STRUCT" _
                & "	     and M7.OBJECT = M6.OBJECT" _
                & "	     and M7.USERID = @P1" _
                & "	     and M7.GRCODE = M6.GRCODE09" _
                & "	     and M7.DELFLG <> '1'" _
                & "    WHERE " _
                & "	     1=1 " _
                & "	     and M2.ORGLEVEL = '00010' " _
                & "    GROUP BY " _
                & "       M2.CAMPCODE " _
                & "      ,M7.GRCODE " _
                & "	    ,M7.NODENAMES	" _
                & "    UNION " _
                & "    SELECT " _
                & "       M2.CAMPCODE" _
                & "      ,M7.GRCODE" _
                & " 	    ,M7.NODENAMES	" _
                & "    FROM " _
                & "       M0002_ORG M2" _
                & "	     INNER JOIN M0006_STRUCT M6" _
                & "	     ON M6.CAMPCODE = M2.CAMPCODE" _
                & "	     and M6.OBJECT = 'ORG'" _
                & "	     and M6.CODE = M2.ORGCODE" _
                & "	     and M6.STYMD <= @P2" _
                & "	     and M6.ENDYMD >= @P2" _
                & "	     and M6.DELFLG <> '1' " _
                & "	     and M6.GRCODE10 IS NOT NULL " _
                & "	     INNER JOIN M0007_GROUP M7 " _
                & "	     ON M7.CAMPCODE = M6.CAMPCODE " _
                & "	     and M7.STRUCT = M6.STRUCT" _
                & "	     and M7.OBJECT = M6.OBJECT" _
                & "	     and M7.USERID = @P1" _
                & "	     and M7.GRCODE = M6.GRCODE10" _
                & "	     and M7.DELFLG <> '1'" _
                & "    WHERE " _
                & "	     1=1 " _
                & "	     and M2.ORGLEVEL = '00010' " _
                & "    GROUP BY " _
                & "       M2.CAMPCODE " _
                & "      ,M7.GRCODE " _
                & "	    ,M7.NODENAMES	" _
                & " )W " _
                & "ORDER BY W.GRCODE  "

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

                '○出力編集
                If CAMPCODE = "" Then
                    GRCODE.Add(SQLdr("CODE"))
                    NODENAMES.Add(SQLdr("NAMES"))
                    LISTBOX.Items.Add(New ListItem(SQLdr("NAMES"), SQLdr("CODE")))
                ElseIf CAMPCODE = SQLdr("CAMPCODE") Then
                    GRCODE.Add(SQLdr("CODE"))
                    NODENAMES.Add(SQLdr("NAMES"))
                    LISTBOX.Items.Add(New ListItem(SQLdr("NAMES"), SQLdr("CODE")))
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
            Dim CS0011LOGWRITE As New CS0011LOGWrite                'LogOutput DirString Get
            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME       'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:M0007_GROUP Select"        '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                         'ログ出力
            ERR = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try

        ERR = C_MESSAGE_NO.NORMAL

    End Sub

End Class
