Imports System.Data.SqlClient
Imports System.Web.UI.WebControls

''' <summary>
''' 勘定科目一覧取得
''' </summary>
''' <remarks></remarks>
Public Class GL0015ACCODEList
    Inherits GL0000
    ''' <summary>
    ''' 会社コード
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property CAMPCODE() As String
    ''' <summary>
    ''' 勘定科目
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property ACCODE() As String
    ''' <summary>
    ''' 補助科目コード
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property ACSUBCODE() As String
    ''' <summary>
    ''' メソッド名
    ''' </summary>
    ''' <remarks></remarks>
    Protected Const METHOD_NAME As String = "getList"


    ''' <summary>
    ''' 情報の取得
    ''' </summary>
    ''' <remarks></remarks>
    Public Overrides Sub getList()

        '<< エラー説明 >>
        'O_ERR = OK:00000,ERR:00002(環境エラー),ERR:00003(DBerr)
        '●初期処理


        'PARAM EXTRA01: ACSUBCODE
        If IsNothing(ACSUBCODE) Then
            ACSUBCODE = ""
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
            If IsNothing(LIST) Then
                LIST = New ListBox
            Else
                LIST.Items.Clear()
            End If
        Catch ex As Exception
        End Try
        'DataBase接続文字
        Using SQLcon = sm.getConnection
            SQLcon.Open() 'DataBase接続(Open)

            getACCODEList(SQLcon)

        End Using

    End Sub


    ''' <summary>
    ''' 仕分けパターン一覧取得
    ''' </summary>
    Protected Sub getACCODEList(ByVal SQLcon As SqlConnection)

        Try
            Dim SQLStr As String
            '●Leftボックス用仕訳パターン取得
            SQLStr =
                  "  SELECT rtrim(A.ACCODE)  as CODE    ," _
                & "         rtrim(A.ACNAMES) as NAMES    " _
                & "    FROM ML001_ACCOUNT A                 " _
                & "   WHERE                                        " _
                & "         A.CAMPCODE      = @P1                  " _
                & "     And   STYMD        <= @P2                " _
                & "     And   ENDYMD       >= @P3                " _
                & "     AND A.ACSUBCODE = @P4               " _
                & "     AND A.DELFLG       <> '1'                  " _

            '〇ソート条件追加
            Select Case DEFAULT_SORT
                Case C_DEFAULT_SORT.CODE
                    SQLStr = SQLStr & " ORDER BY CODE, NAMES "
                Case C_DEFAULT_SORT.NAMES
                    SQLStr = SQLStr & " ORDER BY NAMES,CODE "
                Case C_DEFAULT_SORT.SEQ, String.Empty
                    SQLStr = SQLStr & " ORDER BY CODE, NAMES "
                Case Else
            End Select

            Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.Date)
                Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.Date)
                Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", System.Data.SqlDbType.NVarChar, 20)

                PARA1.Value = CAMPCODE
                PARA2.Value = STYMD
                PARA3.Value = ENDYMD
                PARA4.Value = ACSUBCODE

                Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

                '○出力編集
                addListData(SQLdr)

                'Close
                SQLdr.Close() 'Reader(Close)
                SQLdr = Nothing

            End Using
        Catch ex As Exception
            Dim CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get
            CS0011LOGWRITE.INFSUBCLASS = "GL0015"          'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:ML001_ACCOUNT Select"       '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try

        ERR = C_MESSAGE_NO.NORMAL
    End Sub


End Class

