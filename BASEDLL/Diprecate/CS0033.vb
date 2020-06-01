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
''' 自動採番
''' </summary>
''' <remarks></remarks>
Public Structure CS0033AUTONUM
    ''' <summary>
    ''' 採番対象パラメタ
    ''' </summary>
    ''' <value>パラメータ</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property InParm() As String
    ''' <summary>
    ''' 採番結果
    ''' </summary>
    ''' <value></value>
    ''' <returns>採番</returns>
    ''' <remarks></remarks>
    Public Property OutParm() As String
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
    Public Const METHOD_NAME As String = "CS0033AUTONUM"
    ''' <summary>
    '''意味なし連番の自動採番処理
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub CS0033AUTONUM()
        Dim CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get

        Dim WW_DBNAME As String = ""
        Dim WW_IPADDR As String = ""

        Select Case InParm
            Case "TODOKESAKI"
                WW_DBNAME = "MC008_TODOKENO"
            Case "SHARYOA", "SHARYOB", "SHARYOC", "SHARYOD"
                WW_DBNAME = "MA005_SHARYONO"
            Case Else
                ERR = C_MESSAGE_NO.DLL_IF_ERROR
                Exit Sub

        End Select

        '○初期化
        'セッション制御宣言
        Dim sm As New CS0050SESSION
        OutParm = ""
        ERR = C_MESSAGE_NO.NORMAL

        Try
            'DataBase接続文字
            Dim SQLcon = sm.getConnection
            SQLcon.Open() 'DataBase接続(Open)

            '検索SQL文
            Dim SQLStr As String = _
                      " SELECT rtrim(IPADDR)     as IPADDR      " _
                    & "   FROM S0001_TERM                       " _
                    & "  Where TERMCLASS    = '2'               " _
                    & "    and STYMD       <= @P1               " _
                    & "    and ENDYMD      >= @P1               " _
                    & "    and DELFLG      <> '1'               "

            Dim SQLcmd As New SqlCommand(SQLStr, SQLcon)
            Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.Date)
            PARA1.Value = Date.Now
            Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

            '全社サーバの端末を設定
            While SQLdr.Read
                WW_IPADDR = SQLdr("IPADDR")
            End While

            'データが抽出出来ない場合
            If SQLdr.HasRows = False Then
                ERR = C_MESSAGE_NO.DB_ERROR
                CS0011LOGWRITE.INFSUBCLASS = "CS0033AUTONUM"                'SUBクラス名
                CS0011LOGWRITE.INFPOSI = "DB:S0001_TERM Select"             '
                CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
                CS0011LOGWRITE.TEXT = "データが存在しません。"
                CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
                Exit Sub
            End If

            'Close
            SQLdr.Close() 'Reader(Close)
            SQLdr = Nothing

            SQLcmd.Dispose()
            SQLcmd = Nothing

            SQLcon.Close() 'DataBase接続(Close)
            SQLcon.Dispose()
            SQLcon = Nothing

        Catch ex As Exception
            ERR = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME               'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:S0001_TERM Select"             '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                 '
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try

        Try
            '○Web要求定義
            Dim WW_req As WebRequest = WebRequest.Create("http://" & WW_IPADDR & C_URL.NUMBER_ASSIGNMENT)
            ' 権限設定
            WW_req.Credentials = CredentialCache.DefaultCredentials
            '○ポスト・データの作成
            Dim WW_POSTitems As String = ""
            Dim WW_POSTitem As Hashtable = New Hashtable()

            '　ポスト・データ編集(KEYは複数設定可能)＆設定
            WW_POSTitem("text") = HttpUtility.UrlEncode(InParm, Encoding.UTF8)
            For Each k As String In WW_POSTitem.Keys
                WW_POSTitems = WW_POSTitem(k)
            Next

            Dim WW_data As Byte() = Encoding.UTF8.GetBytes(WW_POSTitems)
            WW_req.Method = "POST"
            WW_req.ContentType = "application/x-www-form-urlencoded"
            WW_req.ContentLength = WW_data.Length

            '○ポスト・データ書込み
            Dim reqStream As Stream = WW_req.GetRequestStream()
            reqStream.Write(WW_data, 0, WW_data.Length)
            reqStream.Close()

            '○ポスト実行
            Dim response As HttpWebResponse = CType(WW_req.GetResponse(), HttpWebResponse)

            '○自動採番結果取得
            Dim reader As New StreamReader(response.GetResponseStream())
            OutParm = reader.ReadToEnd()

            '○Close
            reader.Close()
            response.Close()

        Catch ex As System.Net.WebException
            Select Case CType(ex.Response, HttpWebResponse).StatusCode
                Case 300
                    ERR = C_MESSAGE_NO.FILE_IO_ERROR
                    CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME                'SUBクラス名
                    CS0011LOGWRITE.INFPOSI = "CS0001DBcon"      '
                    CS0011LOGWRITE.NIWEA = "システム管理者へ連絡して下さい(INI_File Not Find)"
                    CS0011LOGWRITE.TEXT = C_MESSAGE_TYPE.ABORT
                    CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.FILE_IO_ERROR
                    CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
                    Exit Sub
                Case 301
                    ERR = C_MESSAGE_NO.DB_ERROR
                    CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME               'SUBクラス名
                    CS0011LOGWRITE.INFPOSI = "DB:" & WW_DBNAME & " Select"      '
                    CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                 '
                    CS0011LOGWRITE.TEXT = "データが存在しません。"
                    CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                    CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
                    Exit Sub
                Case 302
                    ERR = C_MESSAGE_NO.DB_ERROR
                    CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME                'SUBクラス名
                    CS0011LOGWRITE.INFPOSI = "DB:" & WW_DBNAME & " Select"      '
                    CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                 '
                    CS0011LOGWRITE.TEXT = "更新に失敗しました。再度実行してください。"
                    CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                    CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
                    Exit Sub
                Case 303
                    ERR = C_MESSAGE_NO.DB_ERROR
                    CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME               'SUBクラス名
                    CS0011LOGWRITE.INFPOSI = "DB:" & WW_DBNAME & " Select"      '
                    CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                  '
                    CS0011LOGWRITE.TEXT = "システム管理者へ連絡して下さい(DB " & WW_DBNAME & " Select ERR)"
                    CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                    CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
                    Exit Sub
                Case Else
                    ERR = C_MESSAGE_NO.DB_ERROR
                    CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME               'SUBクラス名
                    CS0011LOGWRITE.INFPOSI = "DB:" & WW_DBNAME & " Select"      '
                    CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                  '
                    CS0011LOGWRITE.TEXT = ex.ToString()
                    CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                    CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
                    Exit Sub

            End Select
            Exit Sub
        End Try
    End Sub

End Structure
