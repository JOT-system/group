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
''' DB接続文字取得
''' </summary>
''' <remarks>INIファイルからDB接続文字列のみを取得する</remarks>
Public Structure CS0001DBcon

    ''' <summary>
    ''' DB接続文字
    ''' </summary>
    ''' <value></value>
    ''' <returns>DB接続文字</returns>
    ''' <remarks></remarks>
    Public Property DBCONSTR() As String

    ''' <summary>
    ''' エラーコード
    ''' </summary>
    ''' <value>エラーコード</value>
    ''' <returns>0;正常、それ以外：エラー</returns>
    ''' <remarks></remarks>
    Public Property ERR() As String

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub CS0001DBCONget()
        '●Out PARAM初期設定
        DBCONSTR = Nothing
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
            Dim DBconString As String
            Dim DBconStringBuf As String
            Dim DBconStringRef As Integer

            DBconString = ""
            'File内容のSQL接続文字情報をすべて読み込む
            While (Not sr.EndOfStream)
                DBconStringBuf = sr.ReadLine().Replace(vbTab, " ")
                '開始キーワード(<sql server>)～終了キーワード(/sql server>)間に含まれる文字列を取得
                If (DBconStringBuf.IndexOf("<sql server>") >= 0 Or DBconString <> "") Then
                    DBconString = DBconString & DBconStringBuf.ToString()
                    If InStr(DBconString, "'") >= 1 Then
                        DBconStringRef = InStr(DBconString, "'") - 1
                    Else
                        DBconStringRef = Len(DBconString)
                    End If
                    DBconString = Mid(DBconString, 1, DBconStringRef)
                End If
                '終了キーワード(/sql server>)が出現したら、不要文字を取り除く
                If DBconStringBuf.IndexOf("</sql server>") >= 0 Then
                    DBconString = DBconString.Replace("<sql server>", "")
                    DBconString = DBconString.Replace("</sql server>", "")
                    DBconString = DBconString.Replace("<connection string>", "")
                    DBconString = DBconString.Replace("</connection string>", "")
                    DBconString = DBconString.Replace(ControlChars.Quote, "")
                    DBconString = DBconString.Replace("value=", "")
                    Exit While
                End If

            End While

            sr.Close()
            sr.Dispose()
            sr = Nothing

            DBCONSTR = DBconString

        Catch ex As Exception
            ERR = C_MESSAGE_NO.SYSTEM_ADM_ERROR
            Exit Sub
        End Try

    End Sub

End Structure
