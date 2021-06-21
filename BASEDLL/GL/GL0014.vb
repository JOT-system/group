Imports System.Data.SqlClient
Imports System.Web.UI.WebControls

''' <summary>
''' 仕訳パターン報取得
''' </summary>
''' <remarks></remarks>
Public Class GL0014SHIWAKEPATTERNList
    Inherits GL0000
    ''' <summary>
    ''' 取得条件
    ''' </summary>
    Public Enum LC_ACDCKBN_TYPE
        ''' <summary>
        ''' 全取得
        ''' </summary>
        ALL
        ''' <summary>
        ''' 借方
        ''' </summary>
        DEBIT
        ''' <summary>
        ''' 貸方
        ''' </summary>
        CREDITOR
    End Enum

    ''' <summary>
    '''　取得区分
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property TYPE() As LC_ACDCKBN_TYPE
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
    Public Property SHIWAKEPATERN() As String
    ''' <summary>
    ''' 分類コード
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property SHIWAKEPATERNKBN() As String
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


        'PARAM 01: TYPE
        If checkParam(METHOD_NAME, TYPE) Then
            Exit Sub
        End If

        'PARAM EXTRA02: SHIWAKEPATERNKBN
        If IsNothing(SHIWAKEPATERNKBN) Then
            SHIWAKEPATERNKBN = ""
        End If

        'PARAM EXTRA03: SHIWAKEPATERNKBN
        If IsNothing(SHIWAKEPATERN) Then
            SHIWAKEPATERN = ""
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

            Select Case TYPE
                Case LC_ACDCKBN_TYPE.ALL,
                     LC_ACDCKBN_TYPE.DEBIT,
                     LC_ACDCKBN_TYPE.CREDITOR
                    getSHIWAKEPATTERNList(SQLcon)

            End Select

        End Using

    End Sub


    ''' <summary>
    ''' 仕分けパターン一覧取得
    ''' </summary>
    Protected Sub getSHIWAKEPATTERNList(ByVal SQLcon As SqlConnection)

        Try
            Dim SQLStr As String
            '●Leftボックス用仕訳パターン取得
            SQLStr =
                  "  SELECT rtrim(A.SHIWAKEPATTERN)    as CODE    ," _
                & "         rtrim(A.SHIWAKEPATERNNAME) as NAMES    " _
                & "    FROM ML003_SHIWAKEPATTERN A                 " _
                & "   WHERE                                        " _
                & "         A.CAMPCODE      = @P1                  " _
                & "     AND A.STYMD        <= @P3                  " _
                & "     AND A.ENDYMD       >= @P2                  " _
                & "     AND A.SHIWAKEPATERNKBN = @P4               " _
                & "     AND A.DELFLG       <> '1'                  " _

            '〇抽出条件追加
            If TYPE = LC_ACDCKBN_TYPE.DEBIT Then
                SQLStr = SQLStr _
                        & "     AND A.ACDCKBN  =  'D'  "
            ElseIf TYPE = LC_ACDCKBN_TYPE.CREDITOR Then
                SQLStr = SQLStr _
                        & "     AND A.ACDCKBN  =  'C'  "
            End If

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
                PARA4.Value = SHIWAKEPATERNKBN

                Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

                '○出力編集
                addListData(SQLdr)

                'Close
                SQLdr.Close() 'Reader(Close)
                SQLdr = Nothing

            End Using
        Catch ex As Exception
            Dim CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get
            CS0011LOGWRITE.INFSUBCLASS = "GL0014"          'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:ML003_SHIWAKEPATTERN Select"       '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try

        ERR = C_MESSAGE_NO.NORMAL
    End Sub


End Class

