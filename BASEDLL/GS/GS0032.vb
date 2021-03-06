﻿Imports System.Data.SqlClient
Imports System.Web.UI.WebControls

''' <summary>
''' LeftBox 固定値リスト取得
''' </summary>
''' <remarks></remarks>
Public Class GS0032FIXVALUElst
    Inherits GS0000
    ''' <summary>
    ''' 会社コード
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property CAMPCODE() As String
    ''' <summary>
    ''' クラスコード
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property CLAS() As String
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
    Public Property STDATE As Date
    ''' <summary>
    ''' 終了年月日
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property ENDDATE As Date
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
    ''' 固定値一覧
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property LISTBOX1() As Object
    ''' <summary>
    ''' 固定値一覧
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property LISTBOX2() As Object
    ''' <summary>
    ''' 固定値一覧
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property LISTBOX3() As Object
    ''' <summary>
    ''' 固定値一覧
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property LISTBOX4() As Object
    ''' <summary>
    ''' 固定値一覧
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property LISTBOX5() As Object

    Protected METHOD_NAME As String = "GS0032FIXVALUElst"
    ''' <summary>
    ''' Leftbox固定値一覧取得
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub GS0032FIXVALUElst()
        '<< エラー説明 >>
        'ERR = OK:00000,ERR:00002(環境エラー),ERR:00003(DBerr)
        '●In PARAMチェック
        'PARAM01: CLAS
        If checkParam(METHOD_NAME, CLAS) Then
            Exit Sub
        End If

        'PARAM03: STDATE
        If checkParam(METHOD_NAME, STDATE) Then
            Exit Sub
        End If

        'PARAM04: ENDDATE
        If checkParam(METHOD_NAME, ENDDATE) Then
            Exit Sub
        End If

        '●初期処理
        ERR = C_MESSAGE_NO.DLL_IF_ERROR
        VALUE1 = New ListBox
        VALUE2 = New ListBox
        VALUE3 = New ListBox
        VALUE4 = New ListBox
        VALUE5 = New ListBox
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

        'セッション制御宣言
        Dim sm As New CS0050SESSION

        '●固定値リスト取得(指定値)
        '○ DB(MC001_FIXVALUE)検索
        Try
            'DataBase接続文字
            Dim SQLcon = sm.getConnection
            SQLcon.Open() 'DataBase接続(Open)

            'S0011_UPROFXLS検索SQL文
            Dim SQL_Str As String = _
                    "SELECT rtrim(KEYCODE) as KEYCODE , rtrim(VALUE1) as VALUE1 , rtrim(VALUE2) as VALUE2 , rtrim(VALUE3) as VALUE3 , rtrim(VALUE4) as VALUE4 , rtrim(VALUE5) as VALUE5 " _
                & " FROM  MC001_FIXVALUE " _
                & " Where CAMPCODE  = @P1 " _
                & "   and CLASS     = @P2 " _
                & "   and STYMD    <= @P3 " _
                & "   and ENDYMD   >= @P4 " _
                & "   and DELFLG   <> @P5 " _
                & " ORDER BY KEYCODE "
            Dim SQLcmd As New SqlCommand(SQL_Str, SQLcon)
            Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.NVarChar, 20)
            Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.NVarChar, 20)
            Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.Date)
            Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", System.Data.SqlDbType.Date)
            Dim PARA5 As SqlParameter = SQLcmd.Parameters.Add("@P5", System.Data.SqlDbType.NVarChar, 1)
            PARA1.Value = CAMPCODE
            PARA2.Value = CLAS
            PARA3.Value = ENDDATE
            PARA4.Value = STDATE
            PARA5.Value = C_DELETE_FLG.DELETE
            Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

            While SQLdr.Read
                If SQLdr("KEYCODE") = "" Then
                Else
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

            ERR = C_MESSAGE_NO.NORMAL

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
            CS0011LOGWRITE.INFPOSI = "DB:MC001_FIXVALUE Select"         '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力

            ERR = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try

        '●固定値リスト取得(デフォルト値)
        '○ DB(MC001_FIXVALUE)検索
        If VALUE1.Items.Count = 0 Then
            Try
                'DataBase接続文字
                Dim SQLcon = sm.getConnection
                SQLcon.Open() 'DataBase接続(Open)

                'S0011_UPROFXLS検索SQL文
                Dim SQL_Str As String = _
                        "SELECT rtrim(KEYCODE) as KEYCODE , rtrim(VALUE1) as VALUE1 , rtrim(VALUE2) as VALUE2 , rtrim(VALUE3) as VALUE3 , rtrim(VALUE4) as VALUE4 , rtrim(VALUE5) as VALUE5 " _
                    & " FROM  MC001_FIXVALUE " _
                    & " Where CAMPCODE  = @P1 " _
                    & "   and CLASS     = @P2 " _
                    & "   and STYMD    <= @P3 " _
                    & "   and ENDYMD   >= @P4 " _
                    & "   and DELFLG   <> @P5 " _
                    & " ORDER BY KEYCODE "
                Dim SQLcmd As New SqlCommand(SQL_Str, SQLcon)
                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.Date)
                Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", System.Data.SqlDbType.Date)
                Dim PARA5 As SqlParameter = SQLcmd.Parameters.Add("@P5", System.Data.SqlDbType.NVarChar, 1)
                PARA1.Value = C_DEFAULT_DATAKEY
                PARA2.Value = CLAS
                PARA3.Value = ENDDATE
                PARA4.Value = STDATE
                PARA5.Value = C_DELETE_FLG.DELETE
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

                ERR = C_MESSAGE_NO.NORMAL

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
                CS0011LOGWRITE.INFPOSI = "DB:MC001_FIXVALUE Select"         '
                CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
                CS0011LOGWRITE.TEXT = ex.ToString()
                CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力

                ERR = C_MESSAGE_NO.DB_ERROR
                Exit Sub
            End Try
        End If

    End Sub


End Class

