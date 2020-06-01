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


'■Leftボックス用届先取得
Public Class GS0016TODOKESAKIget
    Inherits GS0000

    ''' <summary>
    ''' ユーザID
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property USERID() As String
    ''' <summary>
    ''' 会社コード
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property CAMPCODE() As String
    ''' <summary>
    ''' 取引先コード
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property TORICODE() As String
    ''' <summary>
    ''' 分類
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
    Public Property ORGCODE() As String
    ''' <summary>
    ''' 届先CODE一覧
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property TODOKECODE() As List(Of String)
    ''' <summary>
    ''' 届先名称一覧
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property TODOKENAME() As List(Of String)
    ''' <summary>
    ''' 住所一覧
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property ADDR() As List(Of String)
    ''' <summary>
    ''' 特定要件１一覧
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property NOTES1() As List(Of String)
    ''' <summary>
    ''' 特定要件２一覧
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property NOTES2() As List(Of String)
    ''' <summary>
    ''' 特定要件３一覧
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property NOTES3() As List(Of String)
    ''' <summary>
    ''' 特定要件４一覧
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property NOTES4() As List(Of String)
    ''' <summary>
    ''' 特定要件５一覧
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property NOTES5() As List(Of String)
    ''' <summary>
    ''' 所要時間一覧
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property ARRIVTIME() As List(Of String)
    ''' <summary>
    ''' 配送距離（配車用）
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property DISTANCE() As List(Of String)
    ''' <summary>
    ''' 届先情報一覧
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property LISTBOX() As ListBox

    Protected METHOD_NAME As String = "GS0016TODOKESAKIget"
    ''' <summary>
    ''' 届先取得
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub GS0016TODOKESAKIget()
        '<< エラー説明 >>
        'ERR = OK:00000,ERR:00002(環境エラー),ERR:00003(DBerr)

        '●初期処理
        TODOKECODE = New List(Of String)
        TODOKENAME = New List(Of String)
        ADDR = New List(Of String)
        NOTES1 = New List(Of String)
        NOTES2 = New List(Of String)
        NOTES3 = New List(Of String)
        NOTES4 = New List(Of String)
        NOTES5 = New List(Of String)
        ARRIVTIME = New List(Of String)
        DISTANCE = New List(Of String)
        'セッション制御宣言
        Dim sm As New CS0050SESSION
        'PARAM EXTRA01: USERID
        If IsNothing(USERID) Then
            USERID = sm.USERID
        End If
        'PARAM EXTRA02: CLAS
        If IsNothing(CLAS) Then
            CLAS = ""
        End If
        'PARAM EXTRA03: TORICODE
        If IsNothing(TORICODE) Then
            TORICODE = ""
        End If
        'PARAM EXTRA04: ORGCODE
        If IsNothing(ORGCODE) Then
            TORICODE = ""
        End If

        '●Leftボックス用届先取得
        If CLAS = "" Then
            If TORICODE = "" Then
                '[分類]に値が存在しない場合、かつ、[取引先コード]に値が存在しない場合
                Try
                    'DataBase接続文字
                    Dim SQLcon = sm.getConnection
                    SQLcon.Open() 'DataBase接続(Open)

                    Dim SQLStr As String
                    '検索SQL文
                    SQLStr = _
                            "  SELECT                                     " _
                        & "         rtrim(TODOKECODE) as TODOKECODE ,   " _
                        & "         rtrim(NAMES)      as NAMES          " _
                        & "    FROM MC006_TODOKESAKI                    " _
                        & "   Where CAMPCODE     = @P1                  " _
                        & "     and substring(TODOKECODE,1,2)  <> 'JX'  " _
                        & "     and STYMD       <= @P2                  " _
                        & "     and ENDYMD      >= @P2                  " _
                        & "     and DELFLG      <> '1'                  " _
                        & "ORDER BY TODOKECODE                          "

                    Dim SQLcmd As New SqlCommand(SQLStr, SQLcon)
                    Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.NVarChar, 20)
                    Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.Date)
                    PARA1.Value = CAMPCODE
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
                        TODOKECODE.Add(SQLdr("TODOKECODE"))
                        TODOKENAME.Add(SQLdr("NAMES"))
                        LISTBOX.Items.Add(New ListItem(SQLdr("NAMES"), SQLdr("TODOKECODE")))

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
                    CS0011LOGWRITE.INFSUBCLASS = "GS0016TODOKESAKIget"          'SUBクラス名
                    CS0011LOGWRITE.INFPOSI = "DB:MC006_TODOKESAKI Select"       '
                    CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                 '
                    CS0011LOGWRITE.TEXT = ex.ToString()
                    CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                    CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
                    Exit Sub
                End Try

                ERR = C_MESSAGE_NO.NORMAL
            Else
                '[分類]に値が存在しない場合、かつ、[取引先コード]に値が存在する場合
                Try
                    'DataBase接続文字
                    Dim SQLcon = sm.getConnection
                    SQLcon.Open() 'DataBase接続(Open)

                    Dim SQLStr As String
                    '検索SQL文
                    SQLStr = _
                            "  SELECT                                     " _
                        & "         rtrim(TODOKECODE) as TODOKECODE ,   " _
                        & "         rtrim(NAMES)      as NAMES          " _
                        & "    FROM MC006_TODOKESAKI                    " _
                        & "   Where CAMPCODE     = @P1                  " _
                        & "     and TORICODE     = @P2                  " _
                        & "     and substring(TODOKECODE,1,2)  <> 'JX'  " _
                        & "     and STYMD       <= @P3                  " _
                        & "     and ENDYMD      >= @P3                  " _
                        & "     and DELFLG      <> '1'                  " _
                        & "ORDER BY TODOKECODE                          "

                    Dim SQLcmd As New SqlCommand(SQLStr, SQLcon)
                    Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.NVarChar, 20)
                    Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.NVarChar, 20)
                    Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.Date)
                    PARA1.Value = CAMPCODE
                    PARA2.Value = TORICODE
                    PARA3.Value = Date.Now
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
                        TODOKECODE.Add(SQLdr("TODOKECODE"))
                        TODOKENAME.Add(SQLdr("NAMES"))
                        LISTBOX.Items.Add(New ListItem(SQLdr("NAMES"), SQLdr("TODOKECODE")))

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
                    CS0011LOGWRITE.INFSUBCLASS = "GS0016TODOKESAKIget"          'SUBクラス名
                    CS0011LOGWRITE.INFPOSI = "DB:MC006_TODOKESAKI Select"       '
                    CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
                    CS0011LOGWRITE.TEXT = ex.ToString()
                    CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                    CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
                    Exit Sub
                End Try

                ERR = C_MESSAGE_NO.NORMAL
            End If
        Else
            If ORGCODE = "" Then
                If TORICODE = "" Then
                    '[分類]に値が存在する場合、かつ、[組織CODE]に値が存在しない場合、かつ、[取引先コード]に値が存在しない場合
                    Try
                        'DataBase接続文字
                        Dim SQLcon = sm.getConnection
                        SQLcon.Open() 'DataBase接続(Open)
                        Dim SQLStr As String
                        '検索SQL文
                        SQLStr = _
                                "  SELECT rtrim(D.TODOKECODE) as TODOKECODE , " _
                            & "         rtrim(D.NAMES)      as NAMES ,      " _
                            & "         rtrim(D.ADDR1) +                    " _
                            & "         rtrim(D.ADDR2) +                    " _
                            & "         rtrim(D.ADDR3) +                    " _
                            & "         rtrim(D.ADDR4)      as ADDR ,       " _
                            & "         rtrim(D.NOTES1)     as NOTES1 ,     " _
                            & "         rtrim(D.NOTES2)     as NOTES2 ,     " _
                            & "         rtrim(D.NOTES3)     as NOTES3 ,     " _
                            & "         rtrim(D.NOTES4)     as NOTES4 ,     " _
                            & "         rtrim(D.NOTES5)     as NOTES5 ,     " _
                            & "         rtrim(C.ARRIVTIME)  as ARRIVTIME ,  " _
                            & "         rtrim(C.DISTANCE)   as DISTANCE     " _
                            & "    FROM S0005_AUTHOR A                      " _
                            & "   INNER JOIN S0006_ROLE B                   " _
                            & "      ON B.CAMPCODE      = A.CAMPCODE        " _
                            & "     and B.OBJECT        = A.OBJECT          " _
                            & "     and B.ROLE          = A.ROLE            " _
                            & "     and B.PERMITCODE   >= 1                 " _
                            & "     and B.STYMD        <= @P3               " _
                            & "     and B.ENDYMD       >= @P3               " _
                            & "     and B.DELFLG       <> '1'               " _
                            & "   INNER JOIN MC007_TODKORG C                " _
                            & "      ON C.CAMPCODE      = B.CAMPCODE        " _
                            & "     and C.UORG          = B.CODE            " _
                            & "     and C.DELFLG       <> '1'               " _
                            & "   INNER JOIN MC006_TODOKESAKI D             " _
                            & "      ON D.CAMPCODE      = C.CAMPCODE        " _
                            & "     and D.TORICODE 　　 = C.TORICODE        " _
                            & "     and D.TODOKECODE 　 = C.TODOKECODE      " _
                            & "     and D.CLASS 　      = @P4               " _
                            & "     and D.STYMD        <= @P3               " _
                            & "     and D.ENDYMD       >= @P3               " _
                            & "     and D.DELFLG       <> '1'               " _
                            & "   Where A.USERID        = @P1               " _
                            & "     and A.CAMPCODE      = @P2               " _
                            & "     and A.OBJECT        = 'ORG'             " _
                            & "     and A.STYMD        <= @P3               " _
                            & "     and A.ENDYMD       >= @P3               " _
                            & "     and A.DELFLG       <> '1'               " _
                            & "   ORDER BY C.SEQ ,D.TODOKECODE              "

                        Dim SQLcmd As New SqlCommand(SQLStr, SQLcon)
                        Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.NVarChar, 20)
                        Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.NVarChar, 20)
                        Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.Date)
                        Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", System.Data.SqlDbType.NVarChar, 1)
                        PARA1.Value = USERID
                        PARA2.Value = CAMPCODE
                        PARA3.Value = Date.Now
                        PARA4.Value = CLAS
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
                            TODOKECODE.Add(SQLdr("TODOKECODE"))
                            TODOKENAME.Add(SQLdr("NAMES"))
                            ADDR.Add(SQLdr("ADDR"))
                            NOTES1.Add(SQLdr("NOTES1"))
                            NOTES2.Add(SQLdr("NOTES2"))
                            NOTES3.Add(SQLdr("NOTES3"))
                            NOTES4.Add(SQLdr("NOTES4"))
                            NOTES5.Add(SQLdr("NOTES5"))
                            ARRIVTIME.Add(SQLdr("ARRIVTIME"))
                            DISTANCE.Add(SQLdr("DISTANCE"))
                            LISTBOX.Items.Add(New ListItem(SQLdr("NAMES"), SQLdr("TODOKECODE")))

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
                        CS0011LOGWRITE.INFSUBCLASS = "GS0016TODOKESAKIget"          'SUBクラス名
                        CS0011LOGWRITE.INFPOSI = "DB:MC006_TODOKESAKI Select"       '
                        CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                 '
                        CS0011LOGWRITE.TEXT = ex.ToString()
                        CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                        CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
                        Exit Sub
                    End Try

                    ERR = C_MESSAGE_NO.NORMAL
                Else
                    '[分類]に値が存在する場合、かつ、[組織CODE]に値が存在しない場合、かつ、[取引先コード]に値が存在する場合
                    Try
                        'DataBase接続文字
                        Dim SQLcon = sm.getConnection
                        SQLcon.Open() 'DataBase接続(Open)
                        Dim SQLStr As String
                        '検索SQL文
                        SQLStr = _
                                "  SELECT rtrim(D.TODOKECODE) as TODOKECODE , " _
                            & "         rtrim(D.NAMES)      as NAMES ,      " _
                            & "         rtrim(D.ADDR1) +                    " _
                            & "         rtrim(D.ADDR2) +                    " _
                            & "         rtrim(D.ADDR3) +                    " _
                            & "         rtrim(D.ADDR4)      as ADDR ,       " _
                            & "         rtrim(D.NOTES1)     as NOTES1 ,     " _
                            & "         rtrim(D.NOTES2)     as NOTES2 ,     " _
                            & "         rtrim(D.NOTES3)     as NOTES3 ,     " _
                            & "         rtrim(D.NOTES4)     as NOTES4 ,     " _
                            & "         rtrim(D.NOTES5)     as NOTES5 ,     " _
                            & "         rtrim(C.ARRIVTIME)  as ARRIVTIME ,  " _
                            & "         rtrim(C.DISTANCE)   as DISTANCE     " _
                            & "    FROM S0005_AUTHOR A                      " _
                            & "   INNER JOIN S0006_ROLE B                   " _
                            & "      ON B.CAMPCODE      = A.CAMPCODE        " _
                            & "     and B.OBJECT        = A.OBJECT          " _
                            & "     and B.ROLE          = A.ROLE            " _
                            & "     and B.PERMITCODE   >= 1                 " _
                            & "     and B.STYMD        <= @P4               " _
                            & "     and B.ENDYMD       >= @P4               " _
                            & "     and B.DELFLG       <> '1'               " _
                            & "   INNER JOIN MC007_TODKORG C                " _
                            & "      ON C.CAMPCODE      = B.CAMPCODE        " _
                            & "     and C.TORICODE      = @P3               " _
                            & "     and C.UORG          = B.CODE            " _
                            & "     and C.DELFLG       <> '1'               " _
                            & "   INNER JOIN MC006_TODOKESAKI D             " _
                            & "      ON D.CAMPCODE      = C.CAMPCODE        " _
                            & "     and D.TORICODE 　　 = C.TORICODE        " _
                            & "     and D.TODOKECODE 　 = C.TODOKECODE      " _
                            & "     and D.CLASS 　      = @P5               " _
                            & "     and D.STYMD        <= @P4               " _
                            & "     and D.ENDYMD       >= @P4               " _
                            & "     and D.DELFLG       <> '1'               " _
                            & "   Where A.USERID        = @P1               " _
                            & "     and A.CAMPCODE      = @P2               " _
                            & "     and A.OBJECT        = 'ORG'             " _
                            & "     and A.STYMD        <= @P4               " _
                            & "     and A.ENDYMD       >= @P4               " _
                            & "     and A.DELFLG       <> '1'               " _
                            & "   ORDER BY C.SEQ ,D.TODOKECODE              "

                        Dim SQLcmd As New SqlCommand(SQLStr, SQLcon)
                        Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.NVarChar, 20)
                        Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.NVarChar, 20)
                        Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.NVarChar, 20)
                        Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", System.Data.SqlDbType.Date)
                        Dim PARA5 As SqlParameter = SQLcmd.Parameters.Add("@P5", System.Data.SqlDbType.NVarChar, 1)
                        PARA1.Value = USERID
                        PARA2.Value = CAMPCODE
                        PARA3.Value = TORICODE
                        PARA4.Value = Date.Now
                        PARA5.Value = CLAS
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
                            TODOKECODE.Add(SQLdr("TODOKECODE"))
                            TODOKENAME.Add(SQLdr("NAMES"))
                            ADDR.Add(SQLdr("ADDR"))
                            NOTES1.Add(SQLdr("NOTES1"))
                            NOTES2.Add(SQLdr("NOTES2"))
                            NOTES3.Add(SQLdr("NOTES3"))
                            NOTES4.Add(SQLdr("NOTES4"))
                            NOTES5.Add(SQLdr("NOTES5"))
                            ARRIVTIME.Add(SQLdr("ARRIVTIME"))
                            DISTANCE.Add(SQLdr("DISTANCE"))
                            LISTBOX.Items.Add(New ListItem(SQLdr("NAMES"), SQLdr("TODOKECODE")))

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
                        CS0011LOGWRITE.INFSUBCLASS = "GS0016TODOKESAKIget"          'SUBクラス名
                        CS0011LOGWRITE.INFPOSI = "DB:MC006_TODOKESAKI Select"       '
                        CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                 '
                        CS0011LOGWRITE.TEXT = ex.ToString()
                        CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                        CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
                        Exit Sub
                    End Try

                    ERR = C_MESSAGE_NO.NORMAL

                End If

            Else
                If TORICODE = "" Then
                    '[分類]に値が存在する場合、かつ、[組織CODE]に値が存在する場合、かつ、[取引先コード]に値が存在しない場合
                    Try
                        'DataBase接続文字
                        Dim SQLcon = sm.getConnection
                        SQLcon.Open() 'DataBase接続(Open)
                        Dim SQLStr As String

                        '検索SQL文
                        SQLStr = _
                                "  SELECT rtrim(D.TODOKECODE) as TODOKECODE , " _
                            & "         rtrim(D.NAMES)      as NAMES ,      " _
                            & "         rtrim(D.ADDR1) +                    " _
                            & "         rtrim(D.ADDR2) +                    " _
                            & "         rtrim(D.ADDR3) +                    " _
                            & "         rtrim(D.ADDR4)      as ADDR ,       " _
                            & "         rtrim(D.NOTES1)     as NOTES1 ,     " _
                            & "         rtrim(D.NOTES2)     as NOTES2 ,     " _
                            & "         rtrim(D.NOTES3)     as NOTES3 ,     " _
                            & "         rtrim(D.NOTES4)     as NOTES4 ,     " _
                            & "         rtrim(D.NOTES5)     as NOTES5 ,     " _
                            & "         rtrim(C.ARRIVTIME)  as ARRIVTIME ,  " _
                            & "         rtrim(C.DISTANCE)   as DISTANCE     " _
                            & "    FROM S0005_AUTHOR A                      " _
                            & "   INNER JOIN S0006_ROLE B                   " _
                            & "      ON B.CAMPCODE      = A.CAMPCODE        " _
                            & "     and B.OBJECT        = A.OBJECT          " _
                            & "     and B.ROLE          = A.ROLE            " _
                            & "     and B.PERMITCODE   >= 1                 " _
                            & "     and B.STYMD        <= @P3               " _
                            & "     and B.ENDYMD       >= @P3               " _
                            & "     and B.DELFLG       <> '1'               " _
                            & "   INNER JOIN MC007_TODKORG C                " _
                            & "      ON C.CAMPCODE      = B.CAMPCODE        " _
                            & "     and C.UORG          = B.CODE            " _
                            & "     and C.UORG　　      = @P5               " _
                            & "     and C.DELFLG       <> '1'               " _
                            & "   INNER JOIN MC006_TODOKESAKI D             " _
                            & "      ON D.CAMPCODE      = C.CAMPCODE        " _
                            & "     and D.TORICODE 　　 = C.TORICODE        " _
                            & "     and D.TODOKECODE 　 = C.TODOKECODE      " _
                            & "     and D.CLASS 　      = @P4               " _
                            & "     and D.STYMD        <= @P3               " _
                            & "     and D.ENDYMD       >= @P3               " _
                            & "     and D.DELFLG       <> '1'               " _
                            & "   Where A.USERID        = @P1               " _
                            & "     and A.CAMPCODE      = @P2             " _
                            & "     and A.OBJECT        = 'ORG'             " _
                            & "     and A.STYMD        <= @P3               " _
                            & "     and A.ENDYMD       >= @P3               " _
                            & "     and A.DELFLG       <> '1'               " _
                            & "   ORDER BY C.SEQ ,D.TODOKECODE              "

                        Dim SQLcmd As New SqlCommand(SQLStr, SQLcon)
                        Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.NVarChar, 20)
                        Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.NVarChar, 20)
                        Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.Date)
                        Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", System.Data.SqlDbType.NVarChar, 1)
                        Dim PARA5 As SqlParameter = SQLcmd.Parameters.Add("@P5", System.Data.SqlDbType.NVarChar, 20)
                        PARA1.Value = HttpContext.Current.Session("Userid")
                        PARA2.Value = CAMPCODE
                        PARA3.Value = Date.Now
                        PARA4.Value = CLAS
                        PARA5.Value = ORGCODE
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
                            TODOKECODE.Add(SQLdr("TODOKECODE"))
                            TODOKENAME.Add(SQLdr("NAMES"))
                            ADDR.Add(SQLdr("ADDR"))
                            NOTES1.Add(SQLdr("NOTES1"))
                            NOTES2.Add(SQLdr("NOTES2"))
                            NOTES3.Add(SQLdr("NOTES3"))
                            NOTES4.Add(SQLdr("NOTES4"))
                            NOTES5.Add(SQLdr("NOTES5"))
                            ARRIVTIME.Add(SQLdr("ARRIVTIME"))
                            DISTANCE.Add(SQLdr("DISTANCE"))
                            LISTBOX.Items.Add(New ListItem(SQLdr("NAMES"), SQLdr("TODOKECODE")))

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
                        CS0011LOGWRITE.INFSUBCLASS = "GS0016TODOKESAKIget"          'SUBクラス名
                        CS0011LOGWRITE.INFPOSI = "DB:MC006_TODOKESAKI Select"       '
                        CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
                        CS0011LOGWRITE.TEXT = ex.ToString()
                        CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                        CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
                        Exit Sub
                    End Try

                    ERR = C_MESSAGE_NO.NORMAL
                Else
                    '[分類]に値が存在する場合、かつ、[組織CODE]に値が存在する場合、かつ、[取引先コード]に値が存在する場合
                    Try
                        'DataBase接続文字
                        Dim SQLcon = sm.getConnection
                        SQLcon.Open() 'DataBase接続(Open)
                        Dim SQLStr As String

                        '検索SQL文
                        SQLStr = _
                                "  SELECT rtrim(D.TODOKECODE) as TODOKECODE , " _
                            & "         rtrim(D.NAMES)      as NAMES ,      " _
                            & "         rtrim(D.ADDR1) +                    " _
                            & "         rtrim(D.ADDR2) +                    " _
                            & "         rtrim(D.ADDR3) +                    " _
                            & "         rtrim(D.ADDR4)      as ADDR ,       " _
                            & "         rtrim(D.NOTES1)     as NOTES1 ,     " _
                            & "         rtrim(D.NOTES2)     as NOTES2 ,     " _
                            & "         rtrim(D.NOTES3)     as NOTES3 ,     " _
                            & "         rtrim(D.NOTES4)     as NOTES4 ,     " _
                            & "         rtrim(D.NOTES5)     as NOTES5 ,     " _
                            & "         rtrim(C.ARRIVTIME)  as ARRIVTIME ,  " _
                            & "         rtrim(C.DISTANCE)   as DISTANCE     " _
                            & "    FROM S0005_AUTHOR A                      " _
                            & "   INNER JOIN S0006_ROLE B                   " _
                            & "      ON B.CAMPCODE      = A.CAMPCODE        " _
                            & "     and B.OBJECT        = A.OBJECT          " _
                            & "     and B.ROLE          = A.ROLE            " _
                            & "     and B.PERMITCODE   >= 1                 " _
                            & "     and B.STYMD        <= @P4               " _
                            & "     and B.ENDYMD       >= @P4               " _
                            & "     and B.DELFLG       <> '1'               " _
                            & "   INNER JOIN MC007_TODKORG C                " _
                            & "      ON C.CAMPCODE      = B.CAMPCODE        " _
                            & "     and C.TORICODE      = @P3               " _
                            & "     and C.UORG          = B.CODE            " _
                            & "     and C.UORG　　      = @P6               " _
                            & "     and C.DELFLG       <> '1'               " _
                            & "   INNER JOIN MC006_TODOKESAKI D             " _
                            & "      ON D.CAMPCODE      = C.CAMPCODE        " _
                            & "     and D.TORICODE 　　 = C.TORICODE        " _
                            & "     and D.TODOKECODE 　 = C.TODOKECODE      " _
                            & "     and D.CLASS 　      = @P5               " _
                            & "     and D.STYMD        <= @P4               " _
                            & "     and D.ENDYMD       >= @P4               " _
                            & "     and D.DELFLG       <> '1'               " _
                            & "   Where A.USERID        = @P1               " _
                            & "     and A.CAMPCODE      = @P2               " _
                            & "     and A.OBJECT        = 'ORG'             " _
                            & "     and A.STYMD        <= @P4               " _
                            & "     and A.ENDYMD       >= @P4               " _
                            & "     and A.DELFLG       <> '1'               " _
                            & "   ORDER BY C.SEQ ,D.TODOKECODE              "

                        Dim SQLcmd As New SqlCommand(SQLStr, SQLcon)
                        Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.NVarChar, 20)
                        Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.NVarChar, 20)
                        Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.NVarChar, 20)
                        Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", System.Data.SqlDbType.Date)
                        Dim PARA5 As SqlParameter = SQLcmd.Parameters.Add("@P5", System.Data.SqlDbType.NVarChar, 1)
                        Dim PARA6 As SqlParameter = SQLcmd.Parameters.Add("@P6", System.Data.SqlDbType.NVarChar, 20)
                        PARA1.Value = USERID
                        PARA2.Value = CAMPCODE
                        PARA3.Value = TORICODE
                        PARA4.Value = Date.Now
                        PARA5.Value = CLAS
                        PARA6.Value = ORGCODE
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
                            TODOKECODE.Add(SQLdr("TODOKECODE"))
                            TODOKENAME.Add(SQLdr("NAMES"))
                            ADDR.Add(SQLdr("ADDR"))
                            NOTES1.Add(SQLdr("NOTES1"))
                            NOTES2.Add(SQLdr("NOTES2"))
                            NOTES3.Add(SQLdr("NOTES3"))
                            NOTES4.Add(SQLdr("NOTES4"))
                            NOTES5.Add(SQLdr("NOTES5"))
                            ARRIVTIME.Add(SQLdr("ARRIVTIME"))
                            DISTANCE.Add(SQLdr("DISTANCE"))
                            LISTBOX.Items.Add(New ListItem(SQLdr("NAMES"), SQLdr("TODOKECODE")))

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
                        CS0011LOGWRITE.INFSUBCLASS = "GS0016TODOKESAKIget"          'SUBクラス名
                        CS0011LOGWRITE.INFPOSI = "DB:MC006_TODOKESAKI Select"       '
                        CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
                        CS0011LOGWRITE.TEXT = ex.ToString()
                        CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                        CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
                        Exit Sub
                    End Try

                    ERR = C_MESSAGE_NO.NORMAL
                End If
            End If
        End If
    End Sub

End Class
