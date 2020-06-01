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
''' ログ格納ディレクトリ取得
''' </summary>
''' <remarks>INIファイルからログ格納ディレクトリのみを取得する</remarks>
Public Structure CS0003LOGdir

    ''' <summary>
    ''' PARAM01:Log格納ディレクトリ
    ''' </summary>
    ''' <value></value>
    ''' <returns>Log格納ディレクトリ</returns>
    ''' <remarks></remarks>
    Public Property LOGDIRSTR() As String

    ''' <summary>
    ''' エラーコード
    ''' </summary>
    ''' <value>エラーコード</value>
    ''' <returns>0;正常、それ以外：エラー</returns>
    ''' <remarks></remarks>
    Public Property ERR() As String

    ''' <summary>
    ''' ログの出力先を取得する
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub CS0003LOGDIRget()
        '●Out PARAM初期設定
        LOGDIRSTR = Nothing
        ERR = C_MESSAGE_NO.NORMAL

        Dim sr As System.IO.StreamReader

        '●メイン処理
        Try
            Dim IniFileC As String = "C:\APPL\APPLINI\APPL.ini"
            Dim IniFileD As String = "D:\APPL\APPLINI\APPL.ini"

            If System.IO.File.Exists(IniFileC) Then                'ファイルが存在するかチェック
                sr = New System.IO.StreamReader(IniFileC, System.Text.Encoding.UTF8)
            Else
                sr = New System.IO.StreamReader(IniFileD, System.Text.Encoding.UTF8)
            End If
            Dim LOGdirString As String
            Dim LOGdirStringBuf As String
            Dim LOGdirStringRef As Integer

            LOGdirString = ""
            'File内容のLog格納Dir情報をすべて読み込む
            While (Not sr.EndOfStream)
                LOGdirStringBuf = sr.ReadLine().Replace(vbTab, " ")
                '開始キーワード(<log directory>)～終了キーワード(</log directory>)間に含まれる文字列を取得
                If (LOGdirStringBuf.IndexOf("<log directory>") >= 0 Or LOGdirString <> "") Then
                    LOGdirString = LOGdirString & LOGdirStringBuf.ToString()
                    If InStr(LOGdirString, "'") >= 1 Then
                        LOGdirStringRef = InStr(LOGdirString, "'") - 1
                    Else
                        LOGdirStringRef = Len(LOGdirString)
                    End If
                    LOGdirString = Mid(LOGdirString, 1, LOGdirStringRef)
                End If
                '終了キーワード(</log directory>)が出現したら、不要文字を取り除く
                If LOGdirStringBuf.IndexOf("</log directory>") >= 0 Then
                    LOGdirString = LOGdirString.Replace("<directory string>", "")
                    LOGdirString = LOGdirString.Replace("</directory string>", "")
                    LOGdirString = LOGdirString.Replace("<log directory>", "")
                    LOGdirString = LOGdirString.Replace("</log directory>", "")
                    LOGdirString = LOGdirString.Replace(ControlChars.Quote, "")
                    LOGdirString = LOGdirString.Replace("path=", "")
                    Exit While
                End If

            End While

            sr.Close()
            sr.Dispose()
            sr = Nothing

            LOGDIRSTR = LOGdirString

        Catch ex As Exception
            ERR = C_MESSAGE_NO.SYSTEM_ADM_ERROR
            Exit Sub
        End Try

    End Sub

End Structure

