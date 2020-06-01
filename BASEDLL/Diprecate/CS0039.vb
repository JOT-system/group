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
''' システム格納ディレクトリ取得
''' </summary>
''' <remarks></remarks>
Public Structure CS0039SYSdir

    ''' <summary>
    ''' システム格納ディレクトリ
    ''' </summary>
    ''' <value>ディレクトリ</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property SYSDIRSTR() As String
    ''' <summary>
    ''' エラーコード
    ''' </summary>
    ''' <value>エラーコード</value>
    ''' <returns>0;正常、それ以外：エラー</returns>
    ''' <remarks>OK:00000,ERR:00002(環境エラー),ERR:00001(FILEIOERR)</remarks>
    Public Property ERR() As String

    ''' <summary>
    ''' 構造体/関数名
    ''' </summary>
    ''' <remarks></remarks>
    Public Const METHOD_NAME As String = "CS0039SYSDIRget"
    ''' <summary>
    ''' システム出力系ファイルの格納ディレクトリ取得
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub CS0039SYSDIRget()
        '●Out PARAM初期設定
        SYSDIRSTR = Nothing
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
            Dim SYSdirString As String
            Dim SYSdirStringBuf As String
            Dim SYSdirStringRef As Integer

            SYSdirString = ""
            'File内容のSQL接続文字情報をすべて読み込む
            While (Not sr.EndOfStream)
                SYSdirStringBuf = sr.ReadLine().Replace(vbTab, " ")
                '開始キーワード(<Sys directory>)～終了キーワード(/Sys directory>)間に含まれる文字列を取得
                If (SYSdirStringBuf.IndexOf("<Sys directory>") >= 0 Or SYSdirString <> "") Then
                    SYSdirString = SYSdirString & SYSdirStringBuf.ToString()
                    If InStr(SYSdirString, "'") >= 1 Then
                        SYSdirStringRef = InStr(SYSdirString, "'") - 1
                    Else
                        SYSdirStringRef = Len(SYSdirString)
                    End If
                    SYSdirString = Mid(SYSdirString, 1, SYSdirStringRef)
                End If
                '終了キーワード(/Sys directory>)が出現したら、不要文字を取り除く
                If SYSdirStringBuf.IndexOf("</Sys directory>") >= 0 Then
                    SYSdirString = SYSdirString.Replace("<Sys directory>", "")
                    SYSdirString = SYSdirString.Replace("</Sys directory>", "")
                    SYSdirString = SYSdirString.Replace("<directory string>", "")
                    SYSdirString = SYSdirString.Replace("</directory string>", "")
                    SYSdirString = SYSdirString.Replace(ControlChars.Quote, "")
                    SYSdirString = SYSdirString.Replace("path=", "")
                    Exit While
                End If

            End While

            SYSDIRSTR = SYSdirString

            sr.Close()
            sr.Dispose()
            sr = Nothing

        Catch ex As Exception
            ERR = C_MESSAGE_NO.SYSTEM_ADM_ERROR
            Exit Sub
        End Try

    End Sub

End Structure
