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
''' 受注番号取得
''' </summary>
''' <remarks></remarks>
Public Structure CS0034ORDERNOget
    ''' <summary>
    ''' 会社コード
    ''' </summary>
    ''' <value>会社コード</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property CAMPCODE() As String
    ''' <summary>
    ''' 部署コード
    ''' </summary>
    ''' <value>部署コード</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property ORG() As String
    ''' <summary>
    ''' ユーザID
    ''' </summary>
    ''' <value>ユーザID</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property USERID() As String
    ''' <summary>
    ''' 端末ID
    ''' </summary>
    ''' <value>端末ID</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property TERMID() As String
    ''' <summary>
    ''' 採番された受注番号
    ''' </summary>
    ''' <value></value>
    ''' <returns>受注番号</returns>
    ''' <remarks></remarks>
    Public Property ORDERNO() As String
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
    Public Const METHOD_NAME As String = "CS0034ORDERNOget"

    ''' <summary>
    ''' 受注番号採番処理
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub CS0034ORDERNOget()
        Dim CS0011LOGWRITE As New CS0011LOGWrite                'LogOutput DirString Get

        '●In PARAMチェック
        'PARAM01: CAMPCODE
        If IsNothing(CAMPCODE) Then
            ERR = C_MESSAGE_NO.DLL_IF_ERROR
            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME                'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "CAMPCODE"                          '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                   '
            CS0011LOGWRITE.TEXT = C_MESSAGE_TEXT.IN_PARAM_ERROR_TEXT
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End If
        'PARAM02: ORG
        If IsNothing(ORG) Then
            ERR = C_MESSAGE_NO.DLL_IF_ERROR
            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME                'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "ORG"                            '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                   '
            CS0011LOGWRITE.TEXT = C_MESSAGE_TEXT.IN_PARAM_ERROR_TEXT
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End If

        'セッション制御宣言
        Dim sm As New CS0050SESSION

        'PARAM EXTRA01: USERID
        If IsNothing(USERID) Then
            USERID = sm.USERID
        End If
        'PARAM EXTRA02: TERMID
        If IsNothing(TERMID) Then
            USERID = sm.APSV_ID
        End If
        Try
            Dim WW_ORDERSEQ As String = "0000000"
            Dim WW_TIMSTP As Long = 0
            Dim WW_UPDCNT As Integer = 0
            Dim PARA(8) As SqlParameter

            'DataBase接続文字
            Dim SQLcon = sm.getConnection
            SQLcon.Open() 'DataBase接続(Open)

            Dim SQLStr As String = _
                       "SELECT FORMAT(ORDERSEQ + 1, '0000000') as ORDERSEQ , CAST(UPDTIMSTP as bigint) as TIMSTP" _
                     & " FROM MC009_ORDERNO WITH (UPDLOCK) " _
                     & " WHERE CAMPCODE   = @P01 " _
                     & " AND   TERMORG    = @P02 " _
                     & " AND   DELFLG    <> '1' "
            Dim SQLcmd As New SqlCommand(SQLStr, SQLcon)
            PARA(0) = SQLcmd.Parameters.Add("@P01", System.Data.SqlDbType.NVarChar, 20)
            PARA(1) = SQLcmd.Parameters.Add("@P02", System.Data.SqlDbType.NVarChar, 15)
            PARA(0).Value = CAMPCODE
            PARA(1).Value = ORG
            Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

            While SQLdr.Read
                WW_ORDERSEQ = SQLdr("ORDERSEQ")
                WW_TIMSTP = SQLdr("TIMSTP")
            End While
            If SQLdr.HasRows = False Then
                Throw New Exception("受注番号採番マスタ（MC009_ORDERNO）が存在しません")
            End If
            SQLdr.Close()
            SQLcmd.Dispose()
            SQLcmd = Nothing

            '車両番号の更新
            SQLStr = _
                        "UPDATE MC009_ORDERNO " _
                      & "SET ORDERSEQ   = @P04 " _
                      & "  , UPDYMD     = @P05 " _
                      & "  , UPDUSER    = @P06 " _
                      & "  , UPDTERMID  = @P07 " _
                      & "  , RECEIVEYMD = @P08  " _
                      & "WHERE " _
                      & "    CAMPCODE = @P01  " _
                      & "AND TERMORG  = @P02  " _
                      & "AND CAST(UPDTIMSTP as bigint)  = @P03; "
            SQLcmd = New SqlCommand(SQLStr, SQLcon)
            PARA(0) = SQLcmd.Parameters.Add("@P01", System.Data.SqlDbType.NVarChar, 20)
            PARA(1) = SQLcmd.Parameters.Add("@P02", System.Data.SqlDbType.NVarChar, 15)
            PARA(2) = SQLcmd.Parameters.Add("@P03", System.Data.SqlDbType.BigInt)
            PARA(3) = SQLcmd.Parameters.Add("@P04", System.Data.SqlDbType.NVarChar, 7)
            PARA(4) = SQLcmd.Parameters.Add("@P05", System.Data.SqlDbType.DateTime)
            PARA(5) = SQLcmd.Parameters.Add("@P06", System.Data.SqlDbType.NVarChar, 20)
            PARA(6) = SQLcmd.Parameters.Add("@P07", System.Data.SqlDbType.NVarChar, 30)
            PARA(7) = SQLcmd.Parameters.Add("@P08", System.Data.SqlDbType.DateTime)
            PARA(0).Value = CAMPCODE
            PARA(1).Value = ORG
            PARA(2).Value = WW_TIMSTP
            PARA(3).Value = WW_ORDERSEQ
            PARA(4).Value = Date.Now
            PARA(5).Value = USERID
            PARA(6).Value = TERMID
            PARA(7).Value = C_DEFAULT_YMD

            WW_UPDCNT = SQLcmd.ExecuteNonQuery()
            If WW_UPDCNT = 0 Then
                Throw New Exception("他の端末と競合し、受注番号が採番できませんでした")
            End If

            ORDERNO = WW_ORDERSEQ
            SQLcmd.Dispose()
            SQLcmd = Nothing

            SQLcon.Close()
            SQLcon.Dispose()
            SQLcon = Nothing

        Catch ex As Exception
            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME            'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:MC009_ORDERNO Select_Update"   '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                   '
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力

            ERR = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try

        ERR = C_MESSAGE_NO.NORMAL

    End Sub

End Structure
