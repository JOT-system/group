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
''' APサーバ名称取得
''' </summary>
''' <remarks>INIファイルからAPサーバ名称のみを取得する</remarks>
Public Structure CS0002APSRVname

    ''' <summary>
    ''' APサーバ名称
    ''' </summary>
    ''' <value></value>
    ''' <returns>APサーバ名称</returns>
    ''' <remarks></remarks>
    Public Property APSRVNAME() As String

    ''' <summary>
    ''' エラーコード
    ''' </summary>
    ''' <value>エラーコード</value>
    ''' <returns>0;正常、それ以外：エラー</returns>
    ''' <remarks></remarks>
    Public Property ERR() As String

    ''' <summary>
    ''' INIファイルからサーバ名を取得
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub CS0002APSRVNMget()
        '●Out PARAM初期設定
        APSRVname = Nothing
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
            Dim APSRV As String
            Dim APSRVnameBuf As String
            Dim APSRVnameRef As Integer

            APSRV = ""
            'File内容のap server情報をすべて読み込む
            While (Not sr.EndOfStream)
                APSRVnameBuf = sr.ReadLine().Replace(vbTab, " ")
                '開始キーワード(<ap server>)～終了キーワード(</ap server>)間に含まれる文字列を取得
                If (APSRVnameBuf.IndexOf("<ap server>") >= 0 Or APSRV <> "") Then
                    APSRV = APSRV & APSRVnameBuf.ToString()
                    If InStr(APSRV, "'") >= 1 Then
                        APSRVnameRef = InStr(APSRV, "'") - 1
                    Else
                        APSRVnameRef = Len(APSRV)
                    End If
                    APSRV = Mid(APSRV, 1, APSRVnameRef)
                End If
                '終了キーワード(</ap server>)が出現したら、不要文字を取り除く
                If APSRVnameBuf.IndexOf("</ap server>") >= 0 Then
                    APSRV = APSRV.Replace("<name string>", "")
                    APSRV = APSRV.Replace("</name string>", "")
                    APSRV = APSRV.Replace("<ap server>", "")
                    APSRV = APSRV.Replace("</ap server>", "")
                    APSRV = APSRV.Replace(ControlChars.Quote, "")
                    APSRV = APSRV.Replace("value=", "")
                    Exit While
                End If

            End While

            sr.Close()
            sr.Dispose()
            sr = Nothing

            APSRVname = APSRV

        Catch ex As Exception
            ERR = C_MESSAGE_NO.SYSTEM_ADM_ERROR
            Exit Sub

        End Try

    End Sub

End Structure

