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
''' 伝票番号取得
''' </summary>
''' <remarks></remarks>
Public Structure CS0040DENNOget
    ''' <summary>
    ''' 会社コード
    ''' </summary>
    ''' <value>会社コード</value>
    ''' <returns>会社コード</returns>
    ''' <remarks></remarks>
    Public Property CAMPCODE() As String
    ''' <summary>
    ''' 組織コード
    ''' </summary>
    ''' <value>部署コード</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property ORG() As String
    ''' <summary>
    ''' ユーザID
    ''' </summary>
    ''' <value>ユーザーID</value>
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
    ''' 伝票番号
    ''' </summary>
    ''' <value>伝票番号</value>
    ''' <returns>伝票番号</returns>
    ''' <remarks></remarks>
    Public Property DENNO() As String
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
    Public Const METHOD_NAME As String = "CS0040DENNOget"

    ''' <summary>
    ''' 伝票番号採番処理
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub CS0040DENNOget()
        Dim CS0011LOGWRITE As New BASEDLL.CS0011LOGWrite                'LogOutput DirString Get

        '●In PARAMチェック
        'PARAM01: CAMPCODE
        If IsNothing(CAMPCODE) Then
            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME                'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "CAMPCODE"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = C_MESSAGE_TEXT.IN_PARAM_ERROR_TEXT
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            ERR = C_MESSAGE_NO.DLL_IF_ERROR
            Exit Sub
        End If
        'PARAM02: ORG
        If IsNothing(ORG) Then
            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME                'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "ORG"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = C_MESSAGE_TEXT.IN_PARAM_ERROR_TEXT
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            ERR = C_MESSAGE_NO.DLL_IF_ERROR
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
            TERMID = sm.APSV_ID
        End If

        Try
            Dim WW_DENSEQ As String = "00000"
            Dim WW_SEQ As Integer = 0
            Dim WW_TIMSTP As Long = 0
            Dim WW_UPDCNT As Integer = 0
            Dim PARA(8) As SqlParameter

            'DataBase接続文字
            Dim SQLcon = sm.getConnection
            SQLcon.Open() 'DataBase接続(Open)

            Dim SQLStr As String = _
                       "SELECT DENSEQ, MAXSEQ, CAST(UPDTIMSTP as bigint) as TIMSTP" _
                     & " FROM MC011_DENNO WITH (UPDLOCK) " _
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
                WW_SEQ = SQLdr("DENSEQ") + 1
                If WW_SEQ > SQLdr("MAXSEQ") Then
                    WW_SEQ = 1
                End If
                WW_DENSEQ = WW_SEQ.ToString("D6")
                WW_TIMSTP = SQLdr("TIMSTP")
            End While
            If SQLdr.HasRows = False Then
                Throw New Exception("伝票番号採番マスタ（MC011_DENNO）が存在しません")
            End If
            SQLdr.Close()
            SQLcmd.Dispose()
            SQLcmd = Nothing

            '伝票番号の更新
            SQLStr = _
                        "UPDATE MC011_DENNO " _
                      & "SET DENSEQ     = @P04 " _
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
            PARA(3) = SQLcmd.Parameters.Add("@P04", System.Data.SqlDbType.Int)
            PARA(4) = SQLcmd.Parameters.Add("@P05", System.Data.SqlDbType.DateTime)
            PARA(5) = SQLcmd.Parameters.Add("@P06", System.Data.SqlDbType.NVarChar, 20)
            PARA(6) = SQLcmd.Parameters.Add("@P07", System.Data.SqlDbType.NVarChar, 30)
            PARA(7) = SQLcmd.Parameters.Add("@P08", System.Data.SqlDbType.DateTime)
            PARA(0).Value = CAMPCODE
            PARA(1).Value = ORG
            PARA(2).Value = WW_TIMSTP
            PARA(3).Value = WW_SEQ
            PARA(4).Value = Date.Now
            PARA(5).Value = USERID
            PARA(6).Value = TERMID
            PARA(7).Value = C_DEFAULT_YMD

            WW_UPDCNT = SQLcmd.ExecuteNonQuery()
            If WW_UPDCNT = 0 Then
                Throw New Exception("他の端末と競合し、伝票番号が採番できませんでした")
            End If

            DENNO = WW_DENSEQ
            SQLcmd.Dispose()
            SQLcmd = Nothing

            SQLcon.Close()
            SQLcon.Dispose()
            SQLcon = Nothing

        Catch ex As Exception
            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME                'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:MC011_DENNO Select_Update"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力

            ERR = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try

        ERR = C_MESSAGE_NO.NORMAL

    End Sub

End Structure
