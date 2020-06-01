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
''' 更新ジャーナル格納ディレクトリ取得
''' </summary>
''' <remarks></remarks>
Public Structure CS0019JNLdir

    ''' <summary>
    ''' 更新ジャーナル格納ディレクトリ
    ''' </summary>
    ''' <value>ディレクトリ</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property JNLDIRSTR() As String

    ''' <summary>
    ''' エラーコード
    ''' </summary>
    ''' <value>エラーコード</value>
    ''' <returns>0;正常、それ以外：エラー</returns>
    ''' <remarks>OK:00000,ERR:00002(環境エラー),ERR:00001(FILE I/O ERR)</remarks>
    Public Property ERR() As String

    ''' <summary>
    ''' ジャーナル用格納場所の取得
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub CS0019JNLDIRget()
        '●Out PARAM初期設定
        JNLDIRSTR = Nothing
        ERR = C_MESSAGE_NO.NORMAL

        '●メイン処理
        Try
            Dim IniFileC As String = "C:\APPL\APPLINI\APPL.ini"
            Dim IniFileD As String = "D:\APPL\APPLINI\APPL.ini"
            Dim sr As System.IO.StreamReader

            If System.IO.File.Exists(IniFileC) Then                'ファイルが存在するかチェック
                sr = New System.IO.StreamReader(IniFileC, System.Text.Encoding.UTF8)
            Else
                sr = New System.IO.StreamReader(IniFileD, System.Text.Encoding.UTF8)
            End If
            Dim JNLdirString As String
            Dim JNLdirStringBuf As String
            Dim JNLdirStringRef As Integer

            JNLdirString = ""
            'File内容の更新ジャーナル格納Dir情報をすべて読み込む
            While (Not sr.EndOfStream)
                JNLdirStringBuf = sr.ReadLine().Replace(vbTab, " ")
                '開始キーワード(<jnl directory>)～終了キーワード(</jnl directory>)間に含まれる文字列を取得
                If (JNLdirStringBuf.IndexOf("<jnl directory>") >= 0 Or JNLdirString <> "") Then
                    JNLdirString = JNLdirString & JNLdirStringBuf.ToString()
                    If InStr(JNLdirString, "'") >= 1 Then
                        JNLdirStringRef = InStr(JNLdirString, "'") - 1
                    Else
                        JNLdirStringRef = Len(JNLdirString)
                    End If
                    JNLdirString = Mid(JNLdirString, 1, JNLdirStringRef)
                End If
                '終了キーワード(</jnl directory>)が出現したら、不要文字を取り除く
                If JNLdirStringBuf.IndexOf("</jnl directory>") >= 0 Then
                    JNLdirString = JNLdirString.Replace("<directory string>", "")
                    JNLdirString = JNLdirString.Replace("</directory string>", "")
                    JNLdirString = JNLdirString.Replace("<jnl directory>", "")
                    JNLdirString = JNLdirString.Replace("</jnl directory>", "")
                    JNLdirString = JNLdirString.Replace(ControlChars.Quote, "")
                    JNLdirString = JNLdirString.Replace("path=", "")
                    Exit While
                End If

            End While

            sr.Close()
            sr.Dispose()
            sr = Nothing

            JNLDIRSTR = JNLdirString

        Catch ex As Exception
            ERR = C_MESSAGE_NO.SYSTEM_ADM_ERROR
            Exit Sub
        End Try

    End Sub

End Structure
