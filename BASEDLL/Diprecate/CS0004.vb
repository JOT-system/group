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
''' PDF格納ディレクトリ取得
''' </summary>
''' <remarks>INIファイルからPDF格納ディレクトリのみを取得する</remarks>
Public Structure CS0004PDFdir

    ''' <summary>
    ''' PDF格納ディレクトリ
    ''' </summary>
    ''' <value></value>
    ''' <returns>PDF格納ディレクトリ</returns>
    ''' <remarks></remarks>
    Public Property PDFDIRSTR() As String

    ''' <summary>
    ''' エラーコード
    ''' </summary>
    ''' <value>エラーコード</value>
    ''' <returns>0;正常、それ以外：エラー</returns>
    ''' <remarks></remarks>
    Public Property ERR() As String

    ''' <summary>
    ''' PDF用ディレクトリ取得
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub CS0004PDFDIRget()
        '●Out PARAM初期設定
        PDFDIRSTR = Nothing
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
            Dim PDFDIRString As String
            Dim PDFDIRStringBuf As String
            Dim PDFDIRStringRef As Integer

            PDFDIRString = ""
            'File内容の画面退避PDF格納Dir文字情報をすべて読み込む
            While (Not sr.EndOfStream)
                PDFDIRStringBuf = sr.ReadLine().Replace(vbTab, " ")
                '開始キーワード(<PDF directory>)～終了キーワード(</PDF directory>)間に含まれる文字列を取得
                If (PDFDIRStringBuf.IndexOf("<PDF directory>") >= 0 Or PDFDIRString <> "") Then
                    PDFDIRString = PDFDIRString & PDFDIRStringBuf.ToString()
                    If InStr(PDFDIRString, "'") >= 1 Then
                        PDFDIRStringRef = InStr(PDFDIRString, "'") - 1
                    Else
                        PDFDIRStringRef = Len(PDFDIRString)
                    End If
                    PDFDIRString = Mid(PDFDIRString, 1, PDFDIRStringRef)
                End If
                '終了キーワード(</PDF directory>)が出現したら、不要文字を取り除く
                If PDFDIRStringBuf.IndexOf("</PDF directory>") >= 0 Then
                    PDFDIRString = PDFDIRString.Replace("<directory string>", "")
                    PDFDIRString = PDFDIRString.Replace("</directory string>", "")
                    PDFDIRString = PDFDIRString.Replace("<PDF directory>", "")
                    PDFDIRString = PDFDIRString.Replace("</PDF directory>", "")
                    PDFDIRString = PDFDIRString.Replace(ControlChars.Quote, "")
                    PDFDIRString = PDFDIRString.Replace("path=", "")
                    Exit While
                End If

            End While

            sr.Close()
            sr.Dispose()
            sr = Nothing

            PDFDIRSTR = PDFDIRString

        Catch ex As Exception
            ERR = C_MESSAGE_NO.SYSTEM_ADM_ERROR
            Exit Sub
        End Try

    End Sub

End Structure

