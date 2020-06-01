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
''' ユーザプロファイル（帳票）取得
''' </summary>
''' <remarks></remarks>
Public Structure CS0021UPROFXLS

    ''' <summary>
    ''' 画面ID
    ''' </summary>
    ''' <value>画面ID</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property MAPID() As String
    ''' <summary>
    ''' ユーザID
    ''' </summary>
    ''' <value>ユーザID</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property USERID() As String
    ''' <summary>
    ''' 出力用帳票ID
    ''' </summary>
    ''' <value>帳票ID</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property REPORTID() As String

    ''' <summary>
    ''' タイトル・明細区分
    ''' </summary>
    ''' <value>種別区分</value>
    ''' <returns></returns>
    ''' <remarks>明細(I or I_Data or I_DataKey)、タイトル(H or T) </remarks>
    Public Property TITOLKBN() As List(Of String)

    ''' <summary>
    ''' 表示項目
    ''' </summary>
    ''' <value>項目</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property FIELD() As List(Of String)
    ''' <summary>
    ''' 表示項目名
    ''' </summary>
    ''' <value>項目名</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property FIELDNAME() As List(Of String)
    ''' <summary>
    ''' 項目構造体
    ''' </summary>
    ''' <value>構造</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property STRUCT() As List(Of String)
    ''' <summary>
    ''' 列位置
    ''' </summary>
    ''' <value>列位置</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property POSIX() As List(Of Integer)

    ''' <summary>
    ''' 行位置
    ''' </summary>
    ''' <value>行位置</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property POSIY() As List(Of Integer)

    ''' <summary>
    ''' 表示幅
    ''' </summary>
    ''' <value>表示幅</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property WIDTH() As List(Of Integer)

    ''' <summary>
    ''' ソート順
    ''' </summary>
    ''' <value>ソート順</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property SORT() As List(Of Integer)

    ''' <summary>
    ''' 区分値タイトルの最大列数
    ''' </summary>
    ''' <value>最大列数</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property POSI_T_X_MAX() As Integer

    ''' <summary>
    ''' 区分値タイトルの最大行数
    ''' </summary>
    ''' <value>最大行数</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property POSI_T_Y_MAX() As Integer

    ''' <summary>
    ''' 区分値明細の最大列数
    ''' </summary>
    ''' <value>最大列数</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property POSI_I_X_MAX() As Integer

    ''' <summary>
    ''' 区分値明細の最大行数
    ''' </summary>
    ''' <value>最大行数</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property POSI_I_Y_MAX() As Integer

    ''' <summary>
    ''' 繰返アイテムの最大列数
    ''' </summary>
    ''' <value>最大列数</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property POSI_R_X_MAX() As Integer

    ''' <summary>
    ''' 繰返アイテムの最大行数
    ''' </summary>
    ''' <value>最大行数</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property POSI_R_Y_MAX() As Integer

    ''' <summary>
    ''' 有効区分
    ''' </summary>
    ''' <value>有効区分</value>
    ''' <returns></returns>
    ''' <remarks>Y:有効　N：無効</remarks>
    Public Property EFFECT() As List(Of String)

    ''' <summary>
    ''' 書式用Excelファイル名
    ''' </summary>
    ''' <value>Excelファイル名</value>
    ''' <returns>Excelファイル名</returns>
    ''' <remarks></remarks>
    Public Property EXCELFILE() As String

    ''' <summary>
    ''' 明細開始位置
    ''' </summary>
    ''' <value>位置</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property POSISTART() As Integer

    ''' <summary>
    ''' ソート文字列
    ''' </summary>
    ''' <value>ソート文字列</value>
    ''' <returns>ソート文字列</returns>
    ''' <remarks></remarks>
    Public Property SORTstr() As String

    ''' <summary>
    ''' ヘッダー記載
    ''' </summary>
    ''' <value>ヘッダー記載</value>
    ''' <returns>ヘッダー記載</returns>
    ''' <remarks></remarks>
    Public Property HEADWRITE() As String

    ''' <summary>
    ''' エラーコード
    ''' </summary>
    ''' <value>エラーコード</value>
    ''' <returns>0;正常、それ以外：エラー</returns>
    ''' <remarks>OK:00000,ERR:00002(環境エラー),ERR:00003(DBerr)</remarks>
    Public Property ERR() As String

    ''' <summary>
    ''' 構造体/関数名
    ''' </summary>
    ''' <remarks></remarks>
    Public Const METHOD_NAME As String = "CS0021UPROFXLS"

    ''' <summary>
    ''' プロファイル取得
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub CS0021UPROFXLS()

        '●In PARAMチェック
        'PARAM01: MAPID
        If IsNothing(MAPID) Then
            ERR = C_MESSAGE_NO.DLL_IF_ERROR

            Dim CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get
            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME           'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "I_MAPID"                          '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                   '
            CS0011LOGWRITE.TEXT = C_MESSAGE_TEXT.IN_PARAM_ERROR_TEXT
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End If

        'PARAM02: REPORTID
        If IsNothing(REPORTID) Then
            ERR = C_MESSAGE_NO.DLL_IF_ERROR

            Dim CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get
            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME           'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "REPORTID"                       '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                   '
            CS0011LOGWRITE.TEXT = C_MESSAGE_TEXT.IN_PARAM_ERROR_TEXT
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End If

        'セッション制御宣言
        Dim sm As New CS0050SESSION

        'PARAM EXTRA01 USERID
        If IsNothing(USERID) Then
            USERID = sm.USERID
        End If
        '●ユーザプロファイル（帳票）取得
        '○ 帳票IDよりDB(S0011_UFROFXLS)検索
        Try
            'DataBase接続文字
            Dim SQLcon = sm.getConnection
            SQLcon.Open() 'DataBase接続(Open)

            '検索SQL文
            Dim SQLStr As String = _
                 "SELECT rtrim(TITOLKBN) as TITOLKBN , rtrim(REPORTID) as REPORTID , rtrim(FIELD) as FIELD , rtrim(FIELDNAME) as FIELDNAME , POSIX , POSIY , WIDTH , rtrim(EFFECT) as EFFECT , rtrim(EXCELFILE) as EXCELFILE , rtrim(POSISTART) as POSISTART , rtrim(STRUCT) as STRUCT , rtrim(SORT) as SORT " _
               & " FROM  S0011_UPROFXLS " _
               & " Where USERID   = @P1 " _
               & "   and MAPID    = @P2 " _
               & "   and REPORTID = @P3 " _
               & "   and STYMD   <= @P4 " _
               & "   and ENDYMD  >= @P4 " _
               & "   and DELFLG  <> '1' " _
               & " ORDER BY SORT "

            Dim SQLcmd As New SqlCommand(SQLStr, SQLcon)
            Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.NVarChar, 20)
            Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.NVarChar, 50)
            Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.NVarChar, 50)
            Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", System.Data.SqlDbType.Date)
            PARA1.Value = USERID
            PARA2.Value = MAPID
            PARA3.Value = REPORTID
            PARA4.Value = Date.Now
            Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

            Dim i As Integer = 0
            POSI_T_X_MAX = 0
            POSI_T_Y_MAX = 0
            POSI_I_X_MAX = 0
            POSI_I_Y_MAX = 0

            TITOLKBN = New List(Of String)
            FIELD = New List(Of String)
            FIELDNAME = New List(Of String)
            STRUCT = New List(Of String)
            EFFECT = New List(Of String)
            POSIX = New List(Of Integer)
            POSIY = New List(Of Integer)
            WIDTH = New List(Of Integer)
            SORT = New List(Of Integer)
            EXCELFILE = ""
            SORTstr = ""

            ERR = C_MESSAGE_NO.DLL_IF_ERROR

            While SQLdr.Read
                Select Case SQLdr("TITOLKBN")
                    Case "H"
                        If IsDBNull(SQLdr("EXCELFILE")) Then
                        Else
                            EXCELFILE = SQLdr("EXCELFILE")
                        End If
                        POSISTART = SQLdr("POSISTART")
                        REPORTID = SQLdr("REPORTID")
                        HEADWRITE = SQLdr("EFFECT")

                    Case "T"
                        TITOLKBN.Add(SQLdr("TITOLKBN"))
                        FIELD.Add(SQLdr("FIELD"))
                        FIELDNAME.Add(SQLdr("FIELDNAME"))
                        If IsDBNull(SQLdr("STRUCT")) Then
                            STRUCT.Add(Space(20))
                        Else
                            STRUCT.Add(SQLdr("STRUCT"))
                        End If
                        POSIX.Add(SQLdr("POSIX"))
                        POSIY.Add(SQLdr("POSIY"))
                        WIDTH.Add(SQLdr("WIDTH"))
                        EFFECT.Add(SQLdr("EFFECT"))
                        SORT.Add(SQLdr("SORT"))

                        If SQLdr("POSIX") > POSI_T_X_MAX Then
                            POSI_T_X_MAX = SQLdr("POSIX")
                        End If
                        If SQLdr("POSIY") > POSI_T_Y_MAX Then
                            POSI_T_Y_MAX = SQLdr("POSIY")
                        End If

                        ERR = C_MESSAGE_NO.NORMAL

                    Case "I"    'アイテム領域(Item、Item_Key)
                        TITOLKBN.Add(SQLdr("TITOLKBN"))
                        FIELD.Add(SQLdr("FIELD"))
                        FIELDNAME.Add(SQLdr("FIELDNAME"))
                        If IsDBNull(SQLdr("STRUCT")) Then
                            STRUCT.Add(Space(20))
                        Else
                            STRUCT.Add(SQLdr("STRUCT"))
                        End If
                        POSIX.Add(SQLdr("POSIX"))
                        POSIY.Add(SQLdr("POSIY"))
                        WIDTH.Add(SQLdr("WIDTH"))
                        EFFECT.Add(SQLdr("EFFECT"))
                        SORT.Add(SQLdr("SORT"))

                        If SQLdr("POSIX") > POSI_I_X_MAX Then
                            POSI_I_X_MAX = SQLdr("POSIX")
                        End If
                        If SQLdr("POSIY") > POSI_I_Y_MAX Then
                            POSI_I_Y_MAX = SQLdr("POSIY")
                        End If

                        ERR = C_MESSAGE_NO.NORMAL

                    Case "I_Data"    '繰返アイテム領域(I_Data、I_DataKey)
                        TITOLKBN.Add(SQLdr("TITOLKBN"))
                        FIELD.Add(SQLdr("FIELD"))
                        FIELDNAME.Add(SQLdr("FIELDNAME"))
                        If IsDBNull(SQLdr("STRUCT")) Then
                            STRUCT.Add(Space(20))
                        Else
                            STRUCT.Add(SQLdr("STRUCT"))
                        End If
                        POSIX.Add(SQLdr("POSIX"))
                        POSIY.Add(SQLdr("POSIY"))
                        WIDTH.Add(SQLdr("WIDTH"))
                        EFFECT.Add(SQLdr("EFFECT"))
                        SORT.Add(SQLdr("SORT"))

                        If SQLdr("POSIX") > POSI_R_X_MAX Then
                            POSI_R_X_MAX = SQLdr("POSIX")
                        End If
                        If SQLdr("POSIY") > POSI_R_Y_MAX Then
                            POSI_R_Y_MAX = SQLdr("POSIY")
                        End If

                        ERR = C_MESSAGE_NO.NORMAL

                    Case "I_DataKey"
                        TITOLKBN.Add(SQLdr("TITOLKBN"))
                        FIELD.Add(SQLdr("FIELD"))
                        FIELDNAME.Add(SQLdr("FIELDNAME"))
                        If IsDBNull(SQLdr("STRUCT")) Then
                            STRUCT.Add(Space(20))
                        Else
                            STRUCT.Add(SQLdr("STRUCT"))
                        End If
                        POSIX.Add(SQLdr("POSIX"))
                        POSIY.Add(SQLdr("POSIY"))
                        WIDTH.Add(SQLdr("WIDTH"))
                        EFFECT.Add(SQLdr("EFFECT"))
                        SORT.Add(SQLdr("SORT"))

                        ERR = C_MESSAGE_NO.NORMAL

                End Select

                'ソート用文字列編集
                'ヘッダー以外の場合ソート文字列を編集する
                If Not (SQLdr("TITOLKBN") = "H" Or SQLdr("TITOLKBN") = "T") Then
                    If Not IsDBNull(SQLdr("SORT")) Then
                        If Not SQLdr("SORT") = 0 Then
                            If SORTstr = "" Then
                                SORTstr = SORTstr & SQLdr("FIELD")
                            Else
                                SORTstr = SORTstr & " , " & SQLdr("FIELD")
                            End If
                        End If
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
            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME               'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:S0011_UFROFXLS Select"         '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                   '
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try

        '○ デフォルト帳票IDよりDB(S0011_UFROFXLS)検索
        If ERR = C_MESSAGE_NO.DLL_IF_ERROR Then
            Try
                'DataBase接続文字
                Dim SQLcon = sm.getConnection
                SQLcon.Open() 'DataBase接続(Open)

                '検索SQL文
                Dim SQLStr As String = _
                 "SELECT rtrim(TITOLKBN) as TITOLKBN , rtrim(REPORTID) as REPORTID , rtrim(FIELD) as FIELD , rtrim(FIELDNAME) as FIELDNAME , POSIX , POSIY , WIDTH , rtrim(EFFECT) as EFFECT , rtrim(EXCELFILE) as EXCELFILE , rtrim(POSISTART) as POSISTART , rtrim(STRUCT) as STRUCT ,  rtrim(SORT) as SORT " _
                   & " FROM  S0011_UPROFXLS " _
                   & " Where USERID   = @P1 " _
                   & "   and MAPID    = @P2 " _
                   & "   and REPORTID = @P3 " _
                   & "   and STYMD   <= @P4 " _
                   & "   and ENDYMD  >= @P4 " _
                   & "   and DELFLG  <> '1' " _
                   & " ORDER BY SORT "

                Dim SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.NVarChar, 50)
                Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.NVarChar, 50)
                Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", System.Data.SqlDbType.Date)
                PARA1.Value = C_DEFAULT_DATAKEY
                PARA2.Value = MAPID
                PARA3.Value = REPORTID
                PARA4.Value = Date.Now
                Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

                Dim i As Integer = 0
                POSI_I_X_MAX = 0
                POSI_I_Y_MAX = 0

                TITOLKBN = New List(Of String)
                FIELD = New List(Of String)
                FIELDNAME = New List(Of String)
                STRUCT = New List(Of String)
                EFFECT = New List(Of String)
                POSIX = New List(Of Integer)
                POSIY = New List(Of Integer)
                WIDTH = New List(Of Integer)
                SORT = New List(Of Integer)
                EXCELFILE = ""
                SORTstr = ""
                HEADWRITE = ""

                ERR = C_MESSAGE_NO.DLL_IF_ERROR

                While SQLdr.Read
                    Select Case SQLdr("TITOLKBN")
                        Case "H"
                            If IsDBNull(SQLdr("EXCELFILE")) Then
                            Else
                                EXCELFILE = SQLdr("EXCELFILE")
                            End If
                            POSISTART = SQLdr("POSISTART")
                            REPORTID = SQLdr("REPORTID")
                            HEADWRITE = SQLdr("EFFECT")

                        Case "T"
                            TITOLKBN.Add(SQLdr("TITOLKBN"))
                            FIELD.Add(SQLdr("FIELD"))
                            FIELDNAME.Add(SQLdr("FIELDNAME"))
                            If IsDBNull(SQLdr("STRUCT")) Then
                                STRUCT.Add(Space(20))
                            Else
                                STRUCT.Add(SQLdr("STRUCT"))
                            End If
                            POSIX.Add(SQLdr("POSIX"))
                            POSIY.Add(SQLdr("POSIY"))
                            WIDTH.Add(SQLdr("WIDTH"))
                            EFFECT.Add(SQLdr("EFFECT"))
                            SORT.Add(SQLdr("SORT"))

                            If SQLdr("POSIX") > POSI_T_X_MAX Then
                                POSI_T_X_MAX = SQLdr("POSIX")
                            End If
                            If SQLdr("POSIY") > POSI_T_Y_MAX Then
                                POSI_T_Y_MAX = SQLdr("POSIY")
                            End If

                            ERR = C_MESSAGE_NO.NORMAL

                        Case "I"
                            TITOLKBN.Add(SQLdr("TITOLKBN"))
                            FIELD.Add(SQLdr("FIELD"))
                            FIELDNAME.Add(SQLdr("FIELDNAME"))
                            If IsDBNull(SQLdr("STRUCT")) Then
                                STRUCT.Add(Space(20))
                            Else
                                STRUCT.Add(SQLdr("STRUCT"))
                            End If
                            POSIX.Add(SQLdr("POSIX"))
                            POSIY.Add(SQLdr("POSIY"))
                            WIDTH.Add(SQLdr("WIDTH"))
                            EFFECT.Add(SQLdr("EFFECT"))
                            SORT.Add(SQLdr("SORT"))

                            If SQLdr("POSIX") > POSI_I_X_MAX Then
                                POSI_I_X_MAX = SQLdr("POSIX")
                            End If
                            If SQLdr("POSIY") > POSI_I_Y_MAX Then
                                POSI_I_Y_MAX = SQLdr("POSIY")
                            End If

                            ERR = C_MESSAGE_NO.NORMAL

                        Case "I_Data"
                            TITOLKBN.Add(SQLdr("TITOLKBN"))
                            FIELD.Add(SQLdr("FIELD"))
                            FIELDNAME.Add(SQLdr("FIELDNAME"))
                            If IsDBNull(SQLdr("STRUCT")) Then
                                STRUCT.Add(Space(20))
                            Else
                                STRUCT.Add(SQLdr("STRUCT"))
                            End If
                            POSIX.Add(SQLdr("POSIX"))
                            POSIY.Add(SQLdr("POSIY"))
                            WIDTH.Add(SQLdr("WIDTH"))
                            EFFECT.Add(SQLdr("EFFECT"))
                            SORT.Add(SQLdr("SORT"))

                            If SQLdr("POSIX") > POSI_R_X_MAX Then
                                POSI_R_X_MAX = SQLdr("POSIX")
                            End If
                            If SQLdr("POSIY") > POSI_R_Y_MAX Then
                                POSI_R_Y_MAX = SQLdr("POSIY")
                            End If

                            ERR = C_MESSAGE_NO.NORMAL

                        Case "I_DataKey"
                            TITOLKBN.Add(SQLdr("TITOLKBN"))
                            FIELD.Add(SQLdr("FIELD"))
                            FIELDNAME.Add(SQLdr("FIELDNAME"))
                            If IsDBNull(SQLdr("STRUCT")) Then
                                STRUCT.Add(Space(20))
                            Else
                                STRUCT.Add(SQLdr("STRUCT"))
                            End If
                            POSIX.Add(SQLdr("POSIX"))
                            POSIY.Add(SQLdr("POSIY"))
                            WIDTH.Add(SQLdr("WIDTH"))
                            EFFECT.Add(SQLdr("EFFECT"))
                            SORT.Add(SQLdr("SORT"))

                            ERR = C_MESSAGE_NO.NORMAL

                    End Select

                    'ソート用文字列編集
                    If Not (SQLdr("TITOLKBN") = "H" Or SQLdr("TITOLKBN") = "T") Then
                        If Not IsDBNull(SQLdr("SORT")) Then
                            If Not SQLdr("SORT") = 0 Then
                                If SORTstr = "" Then
                                    SORTstr = SORTstr & SQLdr("FIELD")
                                Else
                                    SORTstr = SORTstr & " , " & SQLdr("FIELD")
                                End If
                            End If
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
                CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME               'SUBクラス名
                CS0011LOGWRITE.INFPOSI = "DB:S0011_UFROFXLS Select"         '
                CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                  '
                CS0011LOGWRITE.TEXT = ex.ToString()
                CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
                Exit Sub
            End Try
        End If

    End Sub

End Structure
