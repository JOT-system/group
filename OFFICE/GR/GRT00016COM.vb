Option Explicit On

Imports System.Data.SqlClient
Imports System.IO
Imports System.Reflection
Imports System.Text
Imports System.Linq

Imports Microsoft.Office.Interop
Imports System.Runtime.InteropServices

Namespace GRT00016COM
#Region "<< T16共通親クラス >>"
    ''' <summary>
    ''' T16系共通クラス
    ''' </summary>
    ''' <remarks></remarks>
    Public MustInherit Class GRT00016COM : Implements IDisposable

        ''' <summary>
        ''' SQLコネクション
        ''' </summary>
        ''' <remarks></remarks>
        Public Property SQLcon As SqlConnection

        ''' <summary>
        ''' ERRNoプロパティ
        ''' </summary>
        ''' <returns>ERRNo</returns>
        Public Property ERR As String


        ''' <summary>
        ''' セッション情報
        ''' </summary>
        Protected Property sm As CS0050SESSION

        ''' <summary>
        ''' コンストラクタ
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub New()
            Me.Initialize()
        End Sub

        ''' <summary>
        ''' 初期化
        ''' </summary>
        ''' <remarks></remarks> 
        Protected Sub Initialize()
            ERR = C_MESSAGE_NO.NORMAL
            sm = New CS0050SESSION
            SQLcon = sm.getConnection
        End Sub

        ''' <summary>
        ''' 解放処理
        ''' </summary>
        Public Overridable Sub Dispose() Implements IDisposable.Dispose
            Me.SQLcon = Nothing
            sm = Nothing
        End Sub

        ''' <summary>
        ''' ログ出力
        ''' </summary>
        ''' <remarks></remarks> 
        Protected Sub PutLog(ByVal messageNo As String,
                       ByVal niwea As String,
                       Optional ByVal messageText As String = "",
                       <System.Runtime.CompilerServices.CallerMemberName> Optional callerMemberName As String = Nothing)
            Dim logWrite As New CS0011LOGWrite With {
            .INFSUBCLASS = Me.GetType.Name,
            .INFPOSI = callerMemberName,
            .NIWEA = niwea,
            .TEXT = messageText,
            .MESSAGENO = messageNo
        }
            logWrite.CS0011LOGWrite()
        End Sub
    End Class
#End Region

#Region "<< JOTマスタ関連 >>"
    ''' <summary>
    ''' JOTマスタ管理
    ''' </summary>
    Public Class JOT_MASTER
        Inherits GRT00016COM

        ''' <summary>
        ''' 届先データ
        ''' </summary>
        Public Class TODOKESAKI
            Public CAMPCODE As String
            Public UORG As String
            Public TORICODE As String
            Public TODOKECODE As String
            Public NAMES As String
            Public ADDR As String
            Public NOTES1 As String
            Public NOTES2 As String
            Public NOTES3 As String
            Public NOTES4 As String
            Public NOTES5 As String
            Public NOTES6 As String
            Public NOTES7 As String
            Public NOTES8 As String
            Public NOTES9 As String
            Public NOTES10 As String
            Public [CLASS] As String
            Public LATITUDE As String
            Public LONGITUDE As String
            Public ARRIVTIME As String
            Public DISTANCE As String

            Public JSRTODOKECODE As String
            Public SHUKABASHO As String

            Public Function MakeDicKey() As String
                Return MakeDicKey(CAMPCODE, UORG, TODOKECODE)
            End Function
            Public Shared Function MakeDicKey(ByVal campcode As String, ByVal uorg As String, ByVal todokecode As String) As String
                Return String.Format("{1}{0}{2}{0}{3}", C_VALUE_SPLIT_DELIMITER, campcode, uorg, todokecode)
            End Function
        End Class
        ''' <summary>
        ''' 品名データ
        ''' </summary>
        Public Class PRODUCT
            Public CAMPCODE As String
            Public UORG As String
            Public PRODUCTCODE As String
            Public OILTYPE As String
            Public PRODUCT1 As String
            Public PRODUCT2 As String
            Public NAMES As String
            Public HTANI As String                      '配送単位
            Public KPRODUCT As String                   '光英車端用品名コード

            Public Function MakeDicKey() As String
                Dim sb As StringBuilder = New StringBuilder()
                sb.Append(CAMPCODE)
                sb.Append(C_VALUE_SPLIT_DELIMITER)
                sb.Append(UORG)
                sb.Append(C_VALUE_SPLIT_DELIMITER)
                sb.Append(PRODUCTCODE)
                Return sb.ToString
            End Function

        End Class
        ''' <summary>
        ''' 乗務員データ
        ''' </summary>
        Public Class STAFF
            Public CAMPCODE As String
            Public UORG As String
            Public STAFFCODE As String
            Public STAFFNAMES As String
            Public NOTES1 As String
            Public NOTES2 As String
            Public NOTES3 As String
            Public NOTES4 As String
            Public NOTES5 As String

            Public Function MakeDicKey() As String
                Dim sb As StringBuilder = New StringBuilder()
                sb.Append(CAMPCODE)
                sb.Append(C_VALUE_SPLIT_DELIMITER)
                sb.Append(UORG)
                sb.Append(C_VALUE_SPLIT_DELIMITER)
                sb.Append(STAFFCODE)
                Return sb.ToString
            End Function
        End Class

        ''' <summary>
        ''' 届先マスタ管理
        ''' </summary>
        Private _dicTodoke As Dictionary(Of String, TODOKESAKI)
        ''' <summary>
        ''' 品名マスタ管理
        ''' </summary>
        Private _dicProduct As Dictionary(Of String, PRODUCT)
        ''' <summary>
        ''' 従業員マスタ管理
        ''' </summary>
        Private _dicStaff As Dictionary(Of String, STAFF)
        ''' <summary>
        ''' 請求先取引先管理
        ''' </summary>
        Private _dicSTori As Dictionary(Of String, Tuple(Of String, String))

        ''' <summary>
        ''' 会社コード
        ''' </summary>
        ''' <remarks></remarks>
        Public Property CAMPCODE As String

        ''' <summary>
        ''' 部署コード
        ''' </summary>
        ''' <remarks></remarks>
        Public Property ORGCODE As String

        ''' <summary>
        ''' 初期化
        ''' </summary>
        ''' <remarks></remarks> 
        Public Overloads Sub Initialize()
            MyBase.Initialize()

            CAMPCODE = String.Empty
            ORGCODE = String.Empty

            If Not IsNothing(_dicTodoke) Then
                _dicTodoke.Clear()
                _dicTodoke = Nothing
            End If
            If Not IsNothing(_dicProduct) Then
                _dicProduct.Clear()
                _dicProduct = Nothing
            End If
            If Not IsNothing(_dicStaff) Then
                _dicStaff.Clear()
                _dicStaff = Nothing
            End If
            If Not IsNothing(_dicSTori) Then
                _dicSTori.Clear()
                _dicSTori = Nothing
            End If

        End Sub

        ''' <summary>
        ''' 解放処理
        ''' </summary>
        Public Overrides Sub Dispose()
            If Not IsNothing(_dicTodoke) Then
                _dicTodoke.Clear()
                _dicTodoke = Nothing
            End If
            If Not IsNothing(_dicProduct) Then
                _dicProduct.Clear()
                _dicProduct = Nothing
            End If
            If Not IsNothing(_dicStaff) Then
                _dicStaff.Clear()
                _dicStaff = Nothing
            End If
            If Not IsNothing(_dicSTori) Then
                _dicSTori.Clear()
                _dicSTori = Nothing
            End If
            MyBase.Dispose()
        End Sub

        ''' <summary>
        ''' 届先データ設定
        ''' </summary>
        ''' <remarks></remarks>
        Public Function InitTodoke(ByVal I_CAMPCODE As String, ByVal I_ORG As String) As Boolean

            If IsNothing(_dicTodoke) Then
                _dicTodoke = New Dictionary(Of String, TODOKESAKI)
            End If
            Try
                'DataBase接続文字
                Using SQLcon = sm.getConnection
                    SQLcon.Open() 'DataBase接続(Open)

                    Dim sb As StringBuilder = New StringBuilder()
                    sb.Append("Select ")
                    sb.Append("  rtrim(A.TORICODE) As TORICODE ")
                    sb.Append("  , rtrim(A.TODOKECODE) As TODOKECODE ")
                    sb.Append("  , rtrim(A.NAMES) As NAMES ")
                    sb.Append("  , rtrim(A.ADDR1) + rtrim(A.ADDR2) + rtrim(A.ADDR3) + rtrim(A.ADDR4) As ADDR ")
                    sb.Append("  , rtrim(A.NOTES1) As NOTES1 ")
                    sb.Append("  , rtrim(A.NOTES2) As NOTES2 ")
                    sb.Append("  , rtrim(A.NOTES3) As NOTES3 ")
                    sb.Append("  , rtrim(A.NOTES4) As NOTES4 ")
                    sb.Append("  , rtrim(A.NOTES5) As NOTES5 ")
                    sb.Append("  , rtrim(A.NOTES6) As NOTES6 ")
                    sb.Append("  , rtrim(A.NOTES7) As NOTES7 ")
                    sb.Append("  , rtrim(A.NOTES8) As NOTES8 ")
                    sb.Append("  , rtrim(A.NOTES9) As NOTES9 ")
                    sb.Append("  , rtrim(A.NOTES10) As NOTES10 ")
                    sb.Append("  , rtrim(A.LATITUDE) As LATITUDE ")
                    sb.Append("  , rtrim(A.LONGITUDE) As LONGITUDE ")
                    sb.Append("  , rtrim(A.CLASS) As CLASS ")
                    sb.Append("  , rtrim(B.ARRIVTIME) As ARRIVTIME ")
                    sb.Append("  , rtrim(B.DISTANCE) As DISTANCE ")
                    sb.Append("FROM ")
                    sb.Append("  MC006_TODOKESAKI A ")
                    sb.Append("  INNER JOIN MC007_TODKORG B ")
                    sb.Append("     On B.CAMPCODE = A.CAMPCODE ")
                    sb.Append("    And B.TORICODE = A.TORICODE ")
                    sb.Append("    And B.TODOKECODE = A.TODOKECODE ")
                    sb.Append("    And B.UORG = @ORG ")
                    sb.Append("    And B.DELFLG <> @DELFLG ")
                    sb.Append("Where ")
                    sb.Append("      A.CAMPCODE = @CAMPCODE ")
                    sb.Append("  And A.STYMD <= @STYMD ")
                    sb.Append("  And A.ENDYMD >= @ENDYMD ")
                    sb.Append("  And A.DELFLG <> @DELFLG ")

                    Dim SQLcmd As New SqlCommand(sb.ToString, SQLcon)
                    Dim CAMPCODE As SqlParameter = SQLcmd.Parameters.Add("@CAMPCODE", System.Data.SqlDbType.NVarChar)
                    Dim STYMD As SqlParameter = SQLcmd.Parameters.Add("@STYMD", System.Data.SqlDbType.Date)
                    Dim ENDYMD As SqlParameter = SQLcmd.Parameters.Add("@ENDYMD", System.Data.SqlDbType.Date)
                    Dim DELFLG As SqlParameter = SQLcmd.Parameters.Add("@DELFLG", System.Data.SqlDbType.NVarChar)
                    Dim ORG As SqlParameter = SQLcmd.Parameters.Add("@ORG", System.Data.SqlDbType.NVarChar)
                    CAMPCODE.Value = I_CAMPCODE
                    STYMD.Value = Date.Now
                    ENDYMD.Value = Date.Now
                    DELFLG.Value = C_DELETE_FLG.DELETE
                    ORG.Value = I_ORG

                    '○SQL実行
                    Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                    '○出力設定
                    While SQLdr.Read
                        Dim wkValue = New TODOKESAKI With {
                            .CAMPCODE = Me.CAMPCODE,
                            .UORG = Me.ORGCODE,
                            .TORICODE = SQLdr("TORICODE"),
                            .TODOKECODE = SQLdr("TODOKECODE"),
                            .NAMES = SQLdr("NAMES"),
                            .ADDR = SQLdr("ADDR"),
                            .NOTES1 = SQLdr("NOTES1"),
                            .NOTES2 = SQLdr("NOTES2"),
                            .NOTES3 = SQLdr("NOTES3"),
                            .NOTES4 = SQLdr("NOTES4"),
                            .NOTES5 = SQLdr("NOTES5"),
                            .NOTES6 = SQLdr("NOTES6"),
                            .NOTES7 = SQLdr("NOTES7"),
                            .NOTES8 = SQLdr("NOTES8"),
                            .NOTES9 = SQLdr("NOTES9"),
                            .NOTES10 = SQLdr("NOTES10"),
                            .LATITUDE = SQLdr("LATITUDE"),
                            .LONGITUDE = SQLdr("LONGITUDE"),
                            .CLASS = SQLdr("CLASS"),
                            .ARRIVTIME = Date.Parse(SQLdr("ARRIVTIME")).ToString("H:mm"),
                            .DISTANCE = SQLdr("DISTANCE")
                        }
                        _dicTodoke.Item(wkValue.MakeDicKey()) = wkValue
                    End While

                    'Close()
                    SQLdr.Close() 'Reader(Close)
                    SQLdr = Nothing

                    SQLcmd.Dispose()
                    SQLcmd = Nothing

                    SQLcon.Close() 'DataBase接続(Close)
                End Using

            Catch ex As Exception
                Me.ERR = C_MESSAGE_NO.SYSTEM_ADM_ERROR
                PutLog(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, ex.ToString)

                Return False
            End Try

            Return True

        End Function

        ''' <summary>
        ''' 品名データ設定
        ''' </summary>
        ''' <remarks></remarks>
        Public Function InitProduct(ByVal I_CAMPCODE As String, ByVal I_ORG As String) As Boolean

            If IsNothing(_dicProduct) Then
                _dicProduct = New Dictionary(Of String, PRODUCT)
            End If
            Try
                'DataBase接続文字
                Using SQLcon = sm.getConnection
                    SQLcon.Open() 'DataBase接続(Open)

                    Dim sb As StringBuilder = New StringBuilder()
                    sb.Append("Select ")
                    sb.Append("  rtrim(A.PRODUCTCODE) As PRODUCTCODE ")
                    sb.Append("  , rtrim(A.OILTYPE) As OILTYPE ")
                    sb.Append("  , rtrim(A.PRODUCT1) As PRODUCT1 ")
                    sb.Append("  , rtrim(A.PRODUCT2) As PRODUCT2 ")
                    sb.Append("  , rtrim(A.NAMES) As NAMES ")
                    sb.Append("  , rtrim(B.HTANI) As HTANI ")
                    sb.Append("  , rtrim(B.KPRODUCT) As KPRODUCT ")
                    sb.Append("FROM ")
                    sb.Append("  MD001_PRODUCT A ")
                    sb.Append("  INNER JOIN MD002_PRODORG B ")
                    sb.Append("    On B.PRODUCTCODE = A.PRODUCTCODE ")
                    sb.Append("    And B.CAMPCODE = A.CAMPCODE ")
                    sb.Append("    And B.UORG = @ORG ")
                    sb.Append("    And B.STYMD <= @STYMD ")
                    sb.Append(" And B.ENDYMD >= @ENDYMD ")
                    sb.Append("    And B.DELFLG <> @DELFLG ")
                    sb.Append("WHERE ")
                    sb.Append("  A.CAMPCODE = @CAMPCODE ")
                    sb.Append("  And A.STYMD <= @STYMD ")
                    sb.Append(" And A.ENDYMD >= @ENDYMD ")
                    sb.Append("  And A.DELFLG <> @DELFLG ")

                    Dim SQLcmd As New SqlCommand(sb.ToString, SQLcon)
                    Dim CAMPCODE As SqlParameter = SQLcmd.Parameters.Add("@CAMPCODE", System.Data.SqlDbType.NVarChar)
                    Dim STYMD As SqlParameter = SQLcmd.Parameters.Add("@STYMD", System.Data.SqlDbType.Date)
                    Dim ENDYMD As SqlParameter = SQLcmd.Parameters.Add("@ENDYMD", System.Data.SqlDbType.Date)
                    Dim DELFLG As SqlParameter = SQLcmd.Parameters.Add("@DELFLG", System.Data.SqlDbType.NVarChar)
                    Dim ORG As SqlParameter = SQLcmd.Parameters.Add("@ORG", System.Data.SqlDbType.NVarChar)
                    CAMPCODE.Value = I_CAMPCODE
                    STYMD.Value = Date.Now
                    ENDYMD.Value = Date.Now
                    DELFLG.Value = C_DELETE_FLG.DELETE
                    ORG.Value = I_ORG

                    '○SQL実行
                    Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                    '○出力設定
                    While SQLdr.Read
                        Dim wkValue = New PRODUCT With {
                            .CAMPCODE = I_CAMPCODE,
                            .UORG = I_ORG,
                            .PRODUCTCODE = SQLdr("PRODUCTCODE"),
                            .OILTYPE = SQLdr("OILTYPE"),
                            .PRODUCT1 = SQLdr("PRODUCT1"),
                            .PRODUCT2 = SQLdr("PRODUCT2"),
                            .NAMES = SQLdr("NAMES"),
                            .HTANI = SQLdr("HTANI"),
                            .KPRODUCT = SQLdr("KPRODUCT")
                        }
                        _dicProduct.Item(wkValue.MakeDicKey) = wkValue
                    End While

                    'Close()
                    SQLdr.Close() 'Reader(Close)
                    SQLdr = Nothing

                    SQLcmd.Dispose()
                    SQLcmd = Nothing

                    SQLcon.Close() 'DataBase接続(Close)
                End Using

            Catch ex As Exception
                Me.ERR = C_MESSAGE_NO.SYSTEM_ADM_ERROR
                PutLog(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, ex.ToString)

                Return False
            End Try

            '光英品名コードをキーにDictionary作成
            Dim _dicProductK = _dicProduct.Values _
                            .Where(Function(x) x.KPRODUCT <> "" And x.CAMPCODE = Me.CAMPCODE And x.UORG = Me.ORGCODE) _
                            .GroupBy(Function(x As PRODUCT) x.KPRODUCT) _
                            .Select(Function(x) x.First()) _
                            .ToDictionary(Function(x) String.Format("K{0}{1}{0}{2}{0}{3}", C_VALUE_SPLIT_DELIMITER, x.CAMPCODE, x.UORG, x.KPRODUCT))
            Dim tmp = _dicProduct.Concat(_dicProductK)
            _dicProduct = tmp.ToDictionary(Function(x) x.Key, Function(x) x.Value)
            Return True

        End Function

        ''' <summary>
        ''' 乗務員データ設定
        ''' </summary>
        ''' <remarks></remarks>
        Public Function InitStaff(ByVal I_CAMPCODE As String, ByVal I_ORG As String) As Boolean

            If IsNothing(_dicStaff) Then
                _dicStaff = New Dictionary(Of String, STAFF)
            End If
            Try
                'DataBase接続文字
                Using SQLcon = sm.getConnection
                    SQLcon.Open() 'DataBase接続(Open)

                    Dim sb As StringBuilder = New StringBuilder()
                    sb.Append("Select ")
                    sb.Append("  isnull(rtrim(A.STAFFCODE), '') as STAFFCODE ")
                    sb.Append("  , isnull(rtrim(A.STAFFNAMES), '') as STAFFNAMES ")
                    sb.Append("  , isnull(rtrim(A.NOTES1), '') as NOTES1 ")
                    sb.Append("  , isnull(rtrim(A.NOTES2), '') as NOTES2 ")
                    sb.Append("  , isnull(rtrim(A.NOTES3), '') as NOTES3 ")
                    sb.Append("  , isnull(rtrim(A.NOTES4), '') as NOTES4 ")
                    sb.Append("  , isnull(rtrim(A.NOTES5), '') as NOTES5 ")
                    sb.Append("FROM ")
                    sb.Append("  MB001_STAFF A ")
                    sb.Append("  INNER JOIN MB002_STAFFORG B ")
                    sb.Append("    ON B.CAMPCODE = A.CAMPCODE ")
                    sb.Append("    and B.STAFFCODE = A.STAFFCODE ")
                    sb.Append("    and B.SORG = @ORG ")
                    sb.Append("    and B.DELFLG <> @DELFLG ")
                    sb.Append("Where ")
                    sb.Append("     A.CAMPCODE = @CAMPCODE ")
                    sb.Append(" and A.STYMD  <= @STYMD ")
                    sb.Append(" and A.ENDYMD >= @ENDYMD ")
                    sb.Append("  and A.DELFLG <> @DELFLG ")
                    sb.Append("ORDER BY ")
                    sb.Append("  B.SEQ ")
                    sb.Append("  , A.STAFFCODE ")

                    Dim SQLcmd As New SqlCommand(sb.ToString, SQLcon)
                    Dim CAMPCODE As SqlParameter = SQLcmd.Parameters.Add("@CAMPCODE", System.Data.SqlDbType.NVarChar)
                    Dim STYMD As SqlParameter = SQLcmd.Parameters.Add("@STYMD", System.Data.SqlDbType.Date)
                    Dim ENDYMD As SqlParameter = SQLcmd.Parameters.Add("@ENDYMD", System.Data.SqlDbType.Date)
                    Dim DELFLG As SqlParameter = SQLcmd.Parameters.Add("@DELFLG", System.Data.SqlDbType.NVarChar)
                    Dim ORG As SqlParameter = SQLcmd.Parameters.Add("@ORG", System.Data.SqlDbType.NVarChar)
                    CAMPCODE.Value = I_CAMPCODE
                    STYMD.Value = Date.Now
                    ENDYMD.Value = Date.Now
                    DELFLG.Value = C_DELETE_FLG.DELETE
                    ORG.Value = I_ORG

                    '○SQL実行
                    Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                    '○出力設定
                    While SQLdr.Read
                        Dim wkValue = New STAFF With {
                            .CAMPCODE = I_CAMPCODE,
                            .UORG = I_ORG,
                            .STAFFCODE = SQLdr("STAFFCODE"),
                            .STAFFNAMES = SQLdr("STAFFNAMES"),
                            .NOTES1 = SQLdr("NOTES1"),
                            .NOTES2 = SQLdr("NOTES2"),
                            .NOTES3 = SQLdr("NOTES3"),
                            .NOTES4 = SQLdr("NOTES4"),
                            .NOTES5 = SQLdr("NOTES5")
                        }
                        _dicStaff(wkValue.MakeDicKey) = wkValue
                    End While

                    'Close()
                    SQLdr.Close() 'Reader(Close)
                    SQLdr = Nothing

                    SQLcmd.Dispose()
                    SQLcmd = Nothing

                    SQLcon.Close() 'DataBase接続(Close)
                End Using

            Catch ex As Exception
                Me.ERR = C_MESSAGE_NO.SYSTEM_ADM_ERROR

                PutLog(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, ex.ToString)

                Return False
            End Try

            Return True

        End Function

        ''' <summary>
        ''' 請求先取引先データ設定
        ''' </summary>
        ''' <remarks></remarks>
        Private Function InitSTori(ByVal I_CAMPCODE As String, ByVal I_ORG As String) As Boolean

            If IsNothing(_dicSTori) Then
                _dicSTori = New Dictionary(Of String, Tuple(Of String, String))
            End If
            Try
                'DataBase接続文字
                Using SQLcon = sm.getConnection
                    SQLcon.Open() 'DataBase接続(Open)

                    Dim SQLStr As String =
                      "       SELECT rtrim(A.TORICODE)   as TORICODE ,       " _
                    & "              rtrim(A.STORICODE)  as STORICODE ,       " _
                    & "              rtrim(B.NAMES) 	as NAMES 		    " _
                    & "         FROM MC003_TORIORG      as A 			    " _
                    & "   INNER JOIN MC002_TORIHIKISAKI as B 		        " _
                    & "           ON B.CAMPCODE   	= A.CAMPCODE 		    " _
                    & "          and B.TORICODE   	= A.STORICODE 		    " _
                    & "          and B.STYMD       <= @P1                   " _
                    & "          and B.ENDYMD      >= @P1 				    " _
                    & "          and B.DELFLG      <> '1' 				    " _
                    & "        Where A.CAMPCODE     = @P2 				    " _
                    & "          and A.UORG     	= @P3 				    " _
                    & "          and A.DELFLG      <> '1' 				    "

                    Dim SQLcmd As New SqlCommand(SQLStr, SQLcon)
                    Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.Date)
                    Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.NVarChar)
                    Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.NVarChar)
                    PARA1.Value = Date.Now
                    PARA2.Value = Me.CAMPCODE
                    PARA3.Value = Me.ORGCODE

                    Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                    While SQLdr.Read
                        Dim wkValue = Tuple.Create(Of String, String)(SQLdr("STORICODE"), SQLdr("NAMES"))
                        Dim wkKey As String = String.Format("{1}{0}{2}{0}{3}", C_VALUE_SPLIT_DELIMITER, Me.CAMPCODE, Me.ORGCODE, SQLdr("TORICODE"))
                        _dicSTori(wkKey) = wkValue
                    End While

                    'Close
                    SQLdr.Close() 'Reader(Close)
                    SQLdr = Nothing

                    SQLcmd.Dispose()
                    SQLcmd = Nothing

                    SQLcon.Close() 'DataBase接続(Close)
                End Using

            Catch ex As Exception
                Me.ERR = C_MESSAGE_NO.SYSTEM_ADM_ERROR

                PutLog(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, ex.ToString)

                Return False
            End Try

            Return True

        End Function

        ''' <summary>
        ''' 届先データ取得
        ''' </summary>
        ''' <remarks></remarks>
        Public Function GetTodoke(ByVal I_TODOKECODE As String) As TODOKESAKI
            Dim wkValue As TODOKESAKI = Nothing
            Dim wkKey As String = TODOKESAKI.MakeDicKey(CAMPCODE, ORGCODE, I_TODOKECODE)

            If IsNothing(_dicTodoke) Then
                If InitTodoke(Me.CAMPCODE, Me.ORGCODE) <> True Then
                    Return Nothing
                End If
            End If

            _dicTodoke.TryGetValue(wkKey, wkValue)

            Return wkValue

        End Function
        ''' <summary>
        ''' 品名データ取得
        ''' </summary>
        ''' <remarks></remarks>
        Public Function GetProduct(ByVal I_PRODUCTCODE As String) As PRODUCT
            Dim wkValue As PRODUCT = Nothing
            Dim wkKey As String = String.Format("{1}{0}{2}{0}{3}", C_VALUE_SPLIT_DELIMITER, Me.CAMPCODE, Me.ORGCODE, I_PRODUCTCODE)

            If IsNothing(_dicProduct) Then
                If InitProduct(Me.CAMPCODE, Me.ORGCODE) <> True Then
                    Return Nothing
                End If
            End If

            _dicProduct.TryGetValue(wkKey, wkValue)

            Return wkValue

        End Function

        ''' <summary>
        ''' 乗務員データ取得
        ''' </summary>
        ''' <remarks></remarks>
        Public Function GetStaff(ByVal I_STAFFCODE As String) As STAFF
            Dim wkValue As STAFF = Nothing
            Dim wkKey As String = String.Format("{1}{0}{2}{0}{3}", C_VALUE_SPLIT_DELIMITER, Me.CAMPCODE, Me.ORGCODE, I_STAFFCODE)

            If IsNothing(_dicStaff) Then
                If InitStaff(Me.CAMPCODE, Me.ORGCODE) <> True Then
                    Return Nothing
                End If
            End If

            _dicStaff.TryGetValue(wkKey, wkValue)

            Return wkValue

        End Function

        ''' <summary>
        ''' 請求先取得   
        ''' </summary>
        ''' <param name="I_TORICODE">取引先コード</param>
        ''' <remarks></remarks>
        Public Function GetSTori(ByVal I_TORICODE As String) As Tuple(Of String, String)
            Dim wkValue As Tuple(Of String, String) = Nothing
            Dim wkKey As String = String.Format("{1}{0}{2}{0}{3}", C_VALUE_SPLIT_DELIMITER, Me.CAMPCODE, Me.ORGCODE, I_TORICODE)

            If IsNothing(_dicSTori) Then
                If InitSTori(Me.CAMPCODE, Me.ORGCODE) <> True Then
                    Return Nothing
                End If
            End If

            _dicSTori.TryGetValue(wkKey, wkValue)

            Return wkValue


        End Function

    End Class

#End Region

#Region "<< JSRコード関連 >>"

    ''' <summary>
    ''' JSRマスタ管理クラス   
    ''' </summary>
    ''' <remarks></remarks>
    Public Class JSRCODE_MASTER
        Inherits GRT00016COM

        ''' <summary>
        ''' グループ作業判定名称（NOTES1：特定要件１）
        ''' </summary>
        Private Const C_GROUPWORK_NOTES As String = "グループ"

        ''' <summary>
        ''' JSR変換コード（届先）
        ''' </summary>
        Public Class JSRCODE_TODOKE
            ''' <summary>
            ''' JSR届先コード
            ''' </summary>
            Public JSRTODOKECODE As String

            ''' <summary>
            ''' 取引先コード
            ''' </summary>
            Public TORICODE As String
            ''' <summary>
            ''' 届先コード
            ''' </summary>
            Public TODOKECODE As String
            ''' <summary>
            ''' 出荷場所（届先コード：出荷場）
            ''' </summary>
            Public SHUKABASHO As String
            ''' <summary>
            ''' 特定要件１（グループ作業）
            ''' </summary>
            Public NOTES1 As String
            ''' <summary>
            ''' グループ作業判定
            ''' </summary>
            ''' <remarks>特定要件１"グループ"(かな・半角・全角)</remarks>
            ReadOnly Property IsGroupWork As Boolean
                Get
                    If Not String.IsNullOrEmpty(NOTES1) AndAlso
                        StrConv(Trim(NOTES1), VbStrConv.Katakana Or VbStrConv.Wide) = C_GROUPWORK_NOTES Then
                        Return True
                    Else
                        Return False
                    End If
                End Get
            End Property

        End Class
        ''' <summary>
        ''' JSR変換コード（品名）
        ''' </summary>
        Public Class JSRCODE_PRODUCT
            ''' <summary>
            ''' JSR品名コード
            ''' </summary>
            Public JSRPRODUCT As String

            ''' <summary>
            ''' 油種
            ''' </summary>
            Public OILTYPE As String
            ''' <summary>
            ''' 品名１
            ''' </summary>
            Public PRODUCT1 As String
            ''' <summary>
            ''' 品名２
            ''' </summary>
            Public PRODUCT2 As String
            ''' <summary>
            ''' 品名コード
            ''' </summary>
            Public PRODUCTCODE As String
        End Class
        ''' <summary>
        ''' JSR変換コード（車両）
        ''' </summary>
        Public Class JSRCODE_SHABAN
            ''' <summary>
            ''' JSR車番
            ''' </summary>
            Public JSRSHABAN As String

            ''' <summary>
            ''' 業務車番
            ''' </summary>
            Public GSHABAN As String
            ''' <summary>
            ''' 車輛タイプ（前）
            ''' </summary>
            Public SHARYOTYPEF As String
            ''' <summary>
            ''' 統一車番（前）
            ''' </summary>
            Public TSHABANF As String
            ''' <summary>
            ''' 車輛タイプ（後）
            ''' </summary>
            Public SHARYOTYPEB As String
            ''' <summary>
            ''' 統一車番（後）
            ''' </summary>
            Public TSHABANB As String
            ''' <summary>
            ''' 車輛タイプ２（後）
            ''' </summary>
            Public SHARYOTYPEB2 As String
            ''' <summary>
            ''' 統一車番２（後）
            ''' </summary>
            Public TSHABANB2 As String
        End Class
        ''' <summary>
        ''' JSR変換コード（従業員）
        ''' </summary>
        Public Class JSRCODE_STAFF
            ''' <summary>
            ''' JSR乗務員コード
            ''' </summary>
            Public JSRSTAFFCODE As String

            ''' <summary>
            ''' 乗務員コード
            ''' </summary>
            Public STAFFCODE As String
        End Class
        ''' <summary>
        ''' JSR届先マスタ
        ''' </summary>
        Private _dicTodoke As Dictionary(Of String, JSRCODE_TODOKE)
        ''' <summary>
        ''' JSR品名マスタ
        ''' </summary>
        Private _dicProduct As Dictionary(Of String, JSRCODE_PRODUCT)
        ''' <summary>
        ''' JSR車両マスタ
        ''' </summary>
        Private _dicShaban As Dictionary(Of String, JSRCODE_SHABAN)
        ''' <summary>
        ''' JSR乗務員マスタ
        ''' </summary>
        Private _dicStaff As Dictionary(Of String, JSRCODE_STAFF)

        ''' <summary>
        ''' 会社コード   
        ''' </summary>
        ''' <remarks></remarks>
        Public Property CAMPCODE As String
        ''' <summary>
        ''' [IN]ORGCODEプロパティ
        ''' </summary>
        ''' <returns>[IN]ORGCODE</returns>
        Public Property ORGCODE() As String

        ''' <summary>
        ''' 初期化
        ''' </summary>
        ''' <remarks></remarks> 
        Public Overloads Sub Initialize()
            MyBase.Initialize()

            CAMPCODE = String.Empty
            ORGCODE = String.Empty

            If Not IsNothing(_dicTodoke) Then
                _dicTodoke.Clear()
                _dicTodoke = Nothing
            End If
            If Not IsNothing(_dicProduct) Then
                _dicProduct.Clear()
                _dicProduct = Nothing
            End If
            If Not IsNothing(_dicShaban) Then
                _dicShaban.Clear()
                _dicShaban = Nothing
            End If
            If Not IsNothing(_dicStaff) Then
                _dicStaff.Clear()
                _dicStaff = Nothing
            End If
        End Sub
        ''' <summary>
        ''' 解放処理
        ''' </summary>
        Public Overrides Sub Dispose()
            If Not IsNothing(_dicTodoke) Then
                _dicTodoke.Clear()
                _dicTodoke = Nothing
            End If
            If Not IsNothing(_dicProduct) Then
                _dicProduct.Clear()
                _dicProduct = Nothing
            End If
            If Not IsNothing(_dicShaban) Then
                _dicShaban.Clear()
                _dicShaban = Nothing
            End If
            If Not IsNothing(_dicStaff) Then
                _dicStaff.Clear()
                _dicStaff = Nothing
            End If
            MyBase.Dispose()
        End Sub
        ''' <summary>
        ''' JSRコードデータ一括読込  
        ''' </summary>
        ''' <remarks></remarks>
        Public Function ReadJSRData() As Boolean

            Err = C_MESSAGE_NO.NORMAL

            If String.IsNullOrEmpty(ORGCODE) Then
                PutLog(C_MESSAGE_NO.DLL_IF_ERROR, C_MESSAGE_TYPE.ABORT, "未設定:ORGCODE")
                Return False
            End If

            If IsNothing(_dicTodoke) Then
                _dicTodoke = New Dictionary(Of String, JSRCODE_TODOKE)
            Else
                _dicTodoke.Clear()
            End If
            If IsNothing(_dicProduct) Then
                _dicProduct = New Dictionary(Of String, JSRCODE_PRODUCT)
            Else
                _dicProduct.Clear()
            End If
            If IsNothing(_dicShaban) Then
                _dicShaban = New Dictionary(Of String, JSRCODE_SHABAN)
            Else
                _dicShaban.Clear()
            End If
            If IsNothing(_dicStaff) Then
                _dicStaff = New Dictionary(Of String, JSRCODE_STAFF)
            Else
                _dicStaff.Clear()
            End If

            'JSR変換コード届先読込
            If ReadTodoke() = False Then
                Return False
            End If
            'JSR変換コード品名読込
            If ReadProduct() = False Then
                Return False
            End If
            'JSR変換コード車両読込
            If ReadShaban() = False Then
                Return False
            End If
            'JSR変換コード従業員読込
            If ReadStaff() = False Then
                Return False
            End If

            Return True

        End Function

        ''' <summary>
        ''' JSR変換コード届先取得   
        ''' </summary>
        ''' <param name="I_JSRCODE">JSRコード</param>
        ''' <returns >変換コード</returns>
        ''' <remarks></remarks>
        Public Function GetTodokeCode(ByVal I_JSRCODE As String) As JSRCODE_TODOKE
            Dim wkValue As JSRCODE_TODOKE = New JSRCODE_TODOKE
            CovertTodokeCode(I_JSRCODE, wkValue)
            Return wkValue
        End Function
        ''' <summary>
        ''' JSR変換コード品名取得   
        ''' </summary>
        ''' <param name="I_JSRCODE">JSRコード</param>
        ''' <returns >変換コード</returns>
        ''' <remarks></remarks>
        Public Function GetProductCode(ByVal I_JSRCODE As String) As JSRCODE_PRODUCT
            Dim wkValue As JSRCODE_PRODUCT = New JSRCODE_PRODUCT
            CovertProductCode(I_JSRCODE, wkValue)
            Return wkValue
        End Function
        ''' <summary>
        ''' JSR変換コード車番取得   
        ''' </summary>
        ''' <param name="I_JSRCODE">JSRコード</param>
        ''' <returns >変換コード</returns>
        ''' <remarks></remarks>
        Public Function GetShabanCode(ByVal I_JSRCODE As String) As JSRCODE_SHABAN
            Dim wkValue As JSRCODE_SHABAN = New JSRCODE_SHABAN
            CovertShabanCode(I_JSRCODE, wkValue)
            Return wkValue
        End Function
        ''' <summary>
        ''' JSR変換コード従業員取得   
        ''' </summary>
        ''' <param name="I_JSRCODE">JSRコード</param>
        ''' <returns >変換コード</returns>
        ''' <remarks></remarks>
        Public Function GetStaffCode(ByVal I_JSRCODE As String) As JSRCODE_STAFF
            Dim wkValue As JSRCODE_STAFF = New JSRCODE_STAFF
            CovertStaffCode(I_JSRCODE, wkValue)
            Return wkValue
        End Function

        ''' <summary>
        ''' JSR変換コード届先取得   
        ''' </summary>
        ''' <param name="I_JSRCODE">JSRコード</param>
        ''' <param name="O_CODEOBJ" >変換コード</param>
        ''' <remarks></remarks>
        Public Function CovertTodokeCode(ByVal I_JSRCODE As String, ByRef O_CODEOBJ As JSRCODE_TODOKE) As Boolean
            Dim rtn As Boolean = True
            Err = C_MESSAGE_NO.NORMAL

            If _dicTodoke.Count = 0 Then
                'JSR変換コード届先格納
                If ReadTodoke(I_JSRCODE) = False Then
                    Return False
                End If
            End If

            'DictionaryKey作成
            ' 部署|JSR届先コード
            Dim wkKey = MakeDicKey(I_JSRCODE)

            'Dictionary存在チェック
            Dim wkValue = New JSRCODE_TODOKE
            If _dicTodoke.TryGetValue(wkKey, wkValue) Then
                O_CODEOBJ = wkValue
            Else
                Err = C_MESSAGE_NO.NO_DATA_EXISTS_ERROR
                rtn = False
            End If

            Return rtn

        End Function
        ''' <summary>
        ''' JSR変換コード品名取得   
        ''' </summary>
        ''' <param name="I_JSRCODE">JSRコード</param>
        ''' <param name="O_CODEOBJ" >変換コード</param>
        ''' <remarks></remarks>
        Public Function CovertProductCode(ByVal I_JSRCODE As String, ByRef O_CODEOBJ As JSRCODE_PRODUCT) As Boolean
            Dim rtn As Boolean = True
            Err = C_MESSAGE_NO.NORMAL

            If _dicProduct.Count = 0 Then
                'JSR変換コード品名格納
                If ReadProduct(I_JSRCODE) = False Then
                    Return False
                End If
            End If

            'DictionaryKey作成
            ' 部署|JSRコード
            Dim wkKey = MakeDicKey(I_JSRCODE)

            'Dictionary存在チェック
            Dim wkValue = New JSRCODE_PRODUCT
            If _dicProduct.TryGetValue(wkKey, wkValue) Then
                O_CODEOBJ = wkValue
            Else
                Err = C_MESSAGE_NO.NO_DATA_EXISTS_ERROR
                rtn = False
            End If

            Return rtn

        End Function
        ''' <summary>
        ''' JSR変換コード車番取得   
        ''' </summary>
        ''' <param name="I_JSRCODE">JSRコード</param>
        ''' <param name="O_CODEOBJ" >変換コード</param>
        ''' <remarks></remarks>
        Public Function CovertShabanCode(ByVal I_JSRCODE As String, ByRef O_CODEOBJ As JSRCODE_SHABAN) As Boolean
            Dim rtn As Boolean = True
            Err = C_MESSAGE_NO.NORMAL

            If _dicShaban.Count = 0 Then
                'JSR変換コード車番格納
                If ReadShaban(I_JSRCODE) = False Then
                    Return False
                End If
            End If

            'DictionaryKey作成
            ' 部署|JSRコード
            Dim wkKey = MakeDicKey(I_JSRCODE)

            'Dictionary存在チェック
            Dim wkValue = New JSRCODE_SHABAN
            If _dicShaban.TryGetValue(wkKey, wkValue) Then
                O_CODEOBJ = wkValue
            Else
                Err = C_MESSAGE_NO.NO_DATA_EXISTS_ERROR
                rtn = False
            End If

            Return rtn

        End Function
        ''' <summary>
        ''' JSR変換コード従業員取得   
        ''' </summary>
        ''' <param name="I_JSRCODE">JSR従業員コード</param>
        ''' <param name="O_CODEOBJ" >JSR変換コード</param>
        ''' <remarks></remarks>
        Public Function CovertStaffCode(ByVal I_JSRCODE As String, ByRef O_CODEOBJ As JSRCODE_STAFF) As Boolean
            Dim rtn As Boolean = True
            Err = C_MESSAGE_NO.NORMAL

            If _dicStaff.Count = 0 Then
                'JSR変換コード従業員格納
                If ReadStaff(I_JSRCODE) = False Then
                    Return False
                End If
            End If

            'DictionaryKey作成
            ' 部署|JSRコード
            Dim wkKey = MakeDicKey(I_JSRCODE)

            'Dictionary存在チェック
            Dim wkValue = New JSRCODE_STAFF
            If _dicStaff.TryGetValue(wkKey, wkValue) Then
                O_CODEOBJ = wkValue
            Else
                Err = C_MESSAGE_NO.NO_DATA_EXISTS_ERROR
                rtn = False
            End If

            Return rtn

        End Function

        ''' <summary>
        ''' JSR変換コード届先読込
        ''' </summary>
        ''' <param name="I_JSRCODE" >未指定時は部署内全部</param>
        ''' <remarks></remarks>
        Private Function ReadTodoke(Optional ByVal I_JSRCODE As String = "") As Boolean
            Dim rtn As Boolean = True
            Err = C_MESSAGE_NO.NORMAL

            '初回アクセス時Dictionary作成
            If IsNothing(_dicTodoke) Then
                _dicTodoke = New Dictionary(Of String, JSRCODE_TODOKE)
            End If
            If String.IsNullOrEmpty(I_JSRCODE) Then
                _dicTodoke.Clear()
            End If

            'SQL
            Dim sb As StringBuilder = New StringBuilder()
            sb.Append("SELECT ")
            sb.Append("  rtrim(A.JSRTODOKECODE) as JSRTODOKECODE ")
            sb.Append("  , rtrim(A.TORICODE)    as TORICODE ")
            sb.Append("  , rtrim(A.TODOKECODE)  as TODOKECODE ")
            sb.Append("  , rtrim(A.SHUKABASHO)  as SHUKABASHO ")
            sb.Append("  , rtrim(B.NOTES1)      as NOTES1 ")
            sb.Append("FROM ")
            sb.Append("  MC007_TODKORG as A ")
            sb.Append("  INNER JOIN MC006_TODOKESAKI as B ")
            sb.Append("     ON B.CAMPCODE = A.CAMPCODE ")
            sb.Append("    and B.TORICODE = A.TORICODE ")
            sb.Append("    and B.TODOKECODE = A.TODOKECODE ")
            sb.Append("    and B.STYMD <= @P1 ")
            sb.Append("    and B.ENDYMD >= @P1 ")
            sb.Append("    and B.DELFLG <> '1' ")
            sb.Append("Where ")
            sb.Append("  A.CAMPCODE = @P2 ")
            sb.Append("  and A.UORG = @P3 ")
            sb.Append("  and A.DELFLG <> '1' ")
            If Not String.IsNullOrEmpty(I_JSRCODE) Then
                sb.Append("  and A.JSRTODOKECODE = @P4 ")
            End If

            Try
                If SQLcon.State <> ConnectionState.Open Then
                    SQLcon.Open() 'DataBase接続(Open)
                End If

                Using SQLcmd As New SqlCommand(sb.ToString, SQLcon)
                    Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.Date)
                    Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.NVarChar)
                    Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.NVarChar)
                    Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", System.Data.SqlDbType.NVarChar)
                    PARA1.Value = Date.Now
                    PARA2.Value = Me.CAMPCODE
                    PARA3.Value = Me.ORGCODE
                    PARA4.Value = I_JSRCODE

                    Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                        While SQLdr.Read
                            Dim wkValue = New JSRCODE_TODOKE With {
                                .JSRTODOKECODE = SQLdr("JSRTODOKECODE").ToString,
                                .TORICODE = SQLdr("TORICODE").ToString,
                                .TODOKECODE = SQLdr("TODOKECODE").ToString,
                                .SHUKABASHO = SQLdr("SHUKABASHO").ToString,
                                .NOTES1 = SQLdr("NOTES1").ToString
                            }
                            If String.IsNullOrEmpty(wkValue.JSRTODOKECODE) Then
                                Continue While
                            End If
                            'DictionaryKey作成
                            ' 部署|JSRコード
                            Dim wkKey = MakeDicKey(wkValue.JSRTODOKECODE)
                            '複数呼出OK
                            _dicTodoke(wkKey) = wkValue
                            '_dicTodoke.Add(wkKey, wkValue)
                        End While
                    End Using
                End Using

            Catch ex As Exception
                Err = C_MESSAGE_NO.DB_ERROR
                rtn = False
            End Try

            Return rtn

        End Function
        ''' <summary>
        ''' JSR変換コード品名読込
        ''' </summary>
        ''' <param name="I_JSRCODE" >未指定時は部署内全部</param>
        ''' <remarks></remarks>
        Private Function ReadProduct(Optional ByVal I_JSRCODE As String = "") As Boolean
            Dim rtn As Boolean = True
            Err = C_MESSAGE_NO.NORMAL

            '初回アクセス時Dictionary作成
            If IsNothing(_dicProduct) Then
                _dicProduct = New Dictionary(Of String, JSRCODE_PRODUCT)
            End If
            If String.IsNullOrEmpty(I_JSRCODE) Then
                _dicProduct.Clear()
            End If

            'SQL
            Dim sb As StringBuilder = New StringBuilder()
            sb.Append("SELECT ")
            sb.Append("  rtrim(A.JSRPRODUCT) as JSRPRODUCT ")
            sb.Append("  , rtrim(A.PRODUCTCODE) as PRODUCTCODE ")
            sb.Append("  , rtrim(B.OILTYPE) as OILTYPE ")
            sb.Append("  , rtrim(B.PRODUCT1) as PRODUCT1 ")
            sb.Append("  , rtrim(B.PRODUCT2) as PRODUCT2 ")
            sb.Append("FROM ")
            sb.Append("  MD002_PRODORG as A ")
            sb.Append("  INNER JOIN MD001_PRODUCT as B ")
            sb.Append("    ON B.CAMPCODE = A.CAMPCODE ")
            sb.Append("    and B.PRODUCTCODE = A.PRODUCTCODE ")
            sb.Append("    and B.STYMD <= @P1 ")
            sb.Append("    and B.ENDYMD >= @P1 ")
            sb.Append("    and B.DELFLG <> '1' ")
            sb.Append("Where ")
            sb.Append("  A.CAMPCODE = @P2 ")
            sb.Append("  and A.UORG = @P3 ")
            sb.Append("  and A.DELFLG <> '1' ")
            If Not String.IsNullOrEmpty(I_JSRCODE) Then
                sb.Append("  and A.JSRPRODUCT = @P4 ")
            End If

            Try
                If SQLcon.State <> ConnectionState.Open Then
                    SQLcon.Open() 'DataBase接続(Open)
                End If

                Using SQLcmd As New SqlCommand(sb.ToString, SQLcon)
                    Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.Date)
                    Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.NVarChar)
                    Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.NVarChar)
                    Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", System.Data.SqlDbType.NVarChar)
                    PARA1.Value = Date.Now
                    PARA2.Value = Me.CAMPCODE
                    PARA3.Value = Me.ORGCODE
                    PARA4.Value = I_JSRCODE

                    Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                        While SQLdr.Read
                            Dim wkValue = New JSRCODE_PRODUCT With {
                                .JSRPRODUCT = SQLdr("JSRPRODUCT").ToString,
                                .OILTYPE = SQLdr("OILTYPE").ToString,
                                .PRODUCT1 = SQLdr("PRODUCT1").ToString,
                                .PRODUCT2 = SQLdr("PRODUCT2").ToString,
                                .PRODUCTCODE = SQLdr("PRODUCTCODE").ToString
                            }
                            If String.IsNullOrEmpty(wkValue.JSRPRODUCT) Then
                                Continue While
                            End If
                            'DictionaryKey作成
                            ' 部署|JSRコード
                            Dim wkKey = MakeDicKey(wkValue.JSRPRODUCT)
                            '複数呼出OK
                            _dicProduct(wkKey) = wkValue
                            '_dicProduct.Add(wkKey, wkValue)
                        End While
                    End Using
                End Using

            Catch ex As Exception
                Err = C_MESSAGE_NO.DB_ERROR
                rtn = False
            End Try

            Return rtn

        End Function
        ''' <summary>
        ''' JSR変換コード車番読込
        ''' </summary>
        ''' <param name="I_JSRCODE" >未指定時は部署内全部</param>
        ''' <remarks></remarks>
        Private Function ReadShaban(Optional ByVal I_JSRCODE As String = "") As Boolean
            Dim rtn As Boolean = True
            Err = C_MESSAGE_NO.NORMAL

            '初回アクセス時Dictionary作成
            If IsNothing(_dicShaban) Then
                _dicShaban = New Dictionary(Of String, JSRCODE_SHABAN)
            End If
            If String.IsNullOrEmpty(I_JSRCODE) Then
                _dicShaban.Clear()
            End If

            'SQL
            Dim sb As StringBuilder = New StringBuilder()
            sb.Append("SELECT ")
            sb.Append("  rtrim(A.JSRSHABAN) as JSRSHABAN ")
            sb.Append("  , rtrim(A.GSHABAN) as GSHABAN ")
            sb.Append("  , rtrim(A.SHARYOTYPEF) as SHARYOTYPEF ")
            sb.Append("  , rtrim(A.TSHABANF) as TSHABANF ")
            sb.Append("  , rtrim(A.TSHABANFNAMES) as TSHABANFNAMES ")
            sb.Append("  , rtrim(A.SHARYOTYPEB) as SHARYOTYPEB ")
            sb.Append("  , rtrim(A.TSHABANB) as TSHABANB ")
            sb.Append("  , rtrim(A.TSHABANBNAMES) as TSHABANBNAMES ")
            sb.Append("  , rtrim(A.SHARYOTYPEB2) as SHARYOTYPEB2 ")
            sb.Append("  , rtrim(A.TSHABANB2) as TSHABANB2 ")
            sb.Append("  , rtrim(A.TSHABANB2NAMES) as TSHABANB2NAMES ")
            sb.Append("FROM ")
            sb.Append("  MA006_SHABANORG as A ")
            sb.Append("  INNER JOIN MA002_SHARYOA as B ")
            sb.Append("    ON B.CAMPCODE = A.CAMPCODE ")
            sb.Append("    and B.SHARYOTYPE = A.SHARYOTYPEF ")
            sb.Append("    and B.TSHABAN = A.TSHABANF ")
            sb.Append("    and B.STYMD <= @P1 ")
            sb.Append("    and B.ENDYMD >= @P1 ")
            sb.Append("    and B.DELFLG <> '1' ")
            sb.Append("  INNER JOIN MA002_SHARYOA as C ")
            sb.Append("    ON C.CAMPCODE = A.CAMPCODE ")
            sb.Append("    and C.SHARYOTYPE = A.SHARYOTYPEF ")
            sb.Append("    and C.TSHABAN = A.TSHABANF ")
            sb.Append("    and C.STYMD <= @P1 ")
            sb.Append("    and C.ENDYMD >= @P1 ")
            sb.Append("    and C.DELFLG <> '1' ")
            sb.Append("Where ")
            sb.Append("  A.CAMPCODE = @P2 ")
            sb.Append("  and A.MANGUORG = @P3 ")
            sb.Append("  and A.DELFLG <> '1' ")
            If Not String.IsNullOrEmpty(I_JSRCODE) Then
                sb.Append("  and A.JSRSHABAN = @P4 ")
            End If

            Try
                If SQLcon.State <> ConnectionState.Open Then
                    SQLcon.Open() 'DataBase接続(Open)
                End If

                Using SQLcmd As New SqlCommand(sb.ToString, SQLcon)
                    Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.Date)
                    Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.NVarChar)
                    Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.NVarChar)
                    Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", System.Data.SqlDbType.NVarChar)
                    PARA1.Value = Date.Now
                    PARA2.Value = Me.CAMPCODE
                    PARA3.Value = Me.ORGCODE
                    PARA4.Value = I_JSRCODE

                    Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                        While SQLdr.Read
                            Dim wkValue = New JSRCODE_SHABAN With {
                                .JSRSHABAN = SQLdr("JSRSHABAN").ToString,
                                .GSHABAN = SQLdr("GSHABAN").ToString,
                                .SHARYOTYPEF = SQLdr("SHARYOTYPEF").ToString,
                                .TSHABANF = SQLdr("TSHABANF").ToString,
                                .SHARYOTYPEB = SQLdr("SHARYOTYPEB").ToString,
                                .TSHABANB = SQLdr("TSHABANB").ToString,
                                .SHARYOTYPEB2 = SQLdr("SHARYOTYPEB2").ToString,
                                .TSHABANB2 = SQLdr("TSHABANB2").ToString
                            }
                            If String.IsNullOrEmpty(wkValue.JSRSHABAN) Then
                                Continue While
                            End If
                            'DictionaryKey作成
                            ' 部署|JSRコード
                            Dim wkKey = MakeDicKey(wkValue.JSRSHABAN)
                            '複数呼出OK
                            _dicShaban(wkKey) = wkValue
                            '_dicShaban.Add(wkKey, wkValue)
                        End While
                    End Using
                End Using

            Catch ex As Exception
                Err = C_MESSAGE_NO.DB_ERROR
                rtn = False
            End Try

            Return rtn

        End Function
        ''' <summary>
        ''' JSR変換コード従業員読込
        ''' </summary>
        ''' <param name="I_JSRCODE" >未指定時は部署内全部</param>
        ''' <remarks></remarks>
        Private Function ReadStaff(Optional ByVal I_JSRCODE As String = "") As Boolean
            Dim rtn As Boolean = True
            Err = C_MESSAGE_NO.NORMAL

            If IsNothing(_dicStaff) Then
                _dicStaff = New Dictionary(Of String, JSRCODE_STAFF)
            End If
            If String.IsNullOrEmpty(I_JSRCODE) Then
                _dicStaff.Clear()
            End If

            'SQL
            Dim sb As StringBuilder = New StringBuilder()
            sb.Append("SELECT ")
            sb.Append("  rtrim(A.JSRSTAFFCODE) as JSRSTAFFCODE ")
            sb.Append("  , rtrim(A.STAFFCODE) as STAFFCODE ")
            sb.Append("FROM ")
            sb.Append("  MB002_STAFFORG as A ")
            sb.Append("  INNER JOIN MB001_STAFF as B ")
            sb.Append("    ON B.CAMPCODE = A.CAMPCODE ")
            sb.Append("    and B.STAFFCODE = A.STAFFCODE ")
            sb.Append("    and B.STYMD <= @P1 ")
            sb.Append("    and B.ENDYMD >= @P1 ")
            sb.Append("    and B.DELFLG <> '1' ")
            sb.Append("Where ")
            sb.Append("  A.CAMPCODE = @P2 ")
            sb.Append("  and A.SORG = @P3 ")
            sb.Append("  and A.DELFLG <> '1' ")
            If Not String.IsNullOrEmpty(I_JSRCODE) Then
                sb.Append("  and A.JSRSTAFFCODE = @P4 ")
            End If

            Try
                If SQLcon.State <> ConnectionState.Open Then
                    SQLcon.Open() 'DataBase接続(Open)
                End If

                Using SQLcmd As New SqlCommand(sb.ToString, SQLcon)
                    Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.Date)
                    Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.NVarChar)
                    Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.NVarChar)
                    Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", System.Data.SqlDbType.NVarChar)
                    PARA1.Value = Date.Now
                    PARA2.Value = Me.CAMPCODE
                    PARA3.Value = Me.ORGCODE
                    PARA4.Value = I_JSRCODE

                    Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                        While SQLdr.Read
                            Dim wkValue = New JSRCODE_STAFF With {
                                .JSRSTAFFCODE = SQLdr("JSRSTAFFCODE").ToString,
                                .STAFFCODE = SQLdr("STAFFCODE").ToString
                            }

                            If String.IsNullOrEmpty(wkValue.JSRSTAFFCODE) Then
                                Continue While
                            End If
                            'DictionaryKey作成
                            ' 部署|JSRコード
                            Dim wkKey = MakeDicKey(wkValue.JSRSTAFFCODE)
                            '複数呼出OK
                            _dicStaff(wkKey) = wkValue
                            '_dicStaff.Add(wkKey, wkValue)
                        End While
                    End Using
                End Using

            Catch ex As Exception
                Err = C_MESSAGE_NO.DB_ERROR
                rtn = False
            End Try

            Return rtn

        End Function

        ''' <summary>
        ''' DictionaryKey作成
        ''' </summary>
        ''' <remarks></remarks>
        Private Function MakeDicKey(ByVal I_JSRCODE As String) As String
            Dim wkKey As String = String.Format("{1}{0}{2}", C_VALUE_SPLIT_DELIMITER, Me.ORGCODE, I_JSRCODE)
            ' 部署コード|JSRコード
            Return wkKey

        End Function
    End Class
#End Region

#Region "<< L1統計DB関連 >>"
    ''' <summary>
    ''' L1統計DB
    ''' </summary>
    ''' <remarks></remarks>
    Public Class L1TOKEI
        Inherits GRT00016COM

        Private CS0044L1INSERT As New BASEDLL.CS0044L1INSERT            '統計DB出力

        Private CS0033AutoNumber As New BASEDLL.CS0033AutoNumber        '自動採番
        Private CS0038ACCODEget As New BASEDLL.CS0038ACCODEget          '勘定科目判定
        Private CS0041TORIORGget As New BASEDLL.CS0041TORIORGget        '取引先タイプ取得
        Private CS0043STAFFORGget As New BASEDLL.CS0043STAFFORGget      '社員管理部署取得
        Private CS0045GSHABANORGget As New BASEDLL.CS0045GSHABANORGget  '車両管理部署取得

        Private L00001tbl As DataTable                                  '統計DB出力用テーブル

        Private ReadOnly UPDUSERID As String                            '更新ユーザID
        Private ReadOnly UPDTERMID As String                            '更新端末ID
        ''' <summary>
        ''' 休日区分カレンダー
        ''' </summary>
        ''' <remarks></remarks>
        Private _dicCal As Dictionary(Of String, String) = New Dictionary(Of String, String)

        ''' <summary>
        ''' コンストラクタ
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub New()
            MyBase.New()
            Initialize()

            '統計DB格納テーブル作成
            L00001tbl = New DataTable
            CS0044L1INSERT.CS0044L1ColmnsAdd(L00001tbl)

        End Sub
        ''' <summary>
        ''' コンストラクタ
        ''' </summary>
        ''' <param name="SQLCon" >DB接続</param>
        ''' <remarks></remarks>
        Public Sub New(ByRef SQLCon As SqlConnection, ByVal UPDUSERID As String, ByVal UPDUSERTERMID As String)
            Me.New()
            Me.SQLcon = SQLCon
            Me.UPDUSERID = UPDUSERID
            Me.UPDTERMID = UPDUSERTERMID
            CS0044L1INSERT.SQLCON = Me.SQLcon

        End Sub

        ''' <summary>
        ''' クローズ
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub Close()
            CS0044L1INSERT.SQLCON = Nothing

            L00001tbl.Clear()
            L00001tbl = Nothing
            MyBase.Dispose()
        End Sub

        ''' <summary>
        ''' 統計DBレコード編集
        ''' </summary>
        ''' <param name="T00004UPDtbl" >T4更新データ</param>
        ''' <param name="O_RTN" >ERR</param>
        ''' <remarks></remarks>
        Public Sub Edit(ByRef T00004UPDtbl As DataTable, ByRef O_RTN As String)

            Dim WW_DATENOW As Date = Date.Now
            Dim WW_M0008tbl As New DataTable
            Dim MC003tbl = New DataTable                                   '取引先部署テーブル
            Dim MA003tbl = New DataTable                                   '車両台帳テーブル
            Dim MA006tbl = New DataTable                                   '車両部署マスタテーブル
            Dim MB001tbl = New DataTable                                   '社員マスタテーブル

            Dim WW_hokidaykbn As String = ""

            O_RTN = C_MESSAGE_NO.NORMAL

            '■■■ T00004UPDtblより統計ＤＢ追加 ■■■
            '
            For Each T00004UPDrow As DataRow In T00004UPDtbl.Rows

                If T00004UPDrow("DELFLG") = C_DELETE_FLG.ALIVE AndAlso
                    T00004UPDrow("OPERATION") = C_LIST_OPERATION_CODE.UPDATING Then
                Else
                    Continue For
                End If

                '着地基準の場合、積配で予定を作成し積置は捨てる
                If T00004UPDrow("URIKBN") = "2" Then
                    If T00004UPDrow("TUMIOKIKBN") = "1" AndAlso
                       T00004UPDrow("SHUKODATE") = T00004UPDrow("SHUKADATE") Then
                        Continue For
                    End If
                End If


                Dim L00001row As DataRow = L00001tbl.NewRow

                Dim WW_SEQ As String = "000000"

                '伝票番号採番
                CS0033AutoNumber.SEQTYPE = CS0033AutoNumber.C_SEQTYPE.DENNO
                CS0033AutoNumber.CAMPCODE = T00004UPDrow("CAMPCODE")
                CS0033AutoNumber.MORG = T00004UPDrow("ORDERORG")
                CS0033AutoNumber.USERID = Me.UPDUSERID
                CS0033AutoNumber.getAutoNumber()
                If isNormal(CS0033AutoNumber.ERR) Then
                    WW_SEQ = CS0033AutoNumber.SEQ
                Else
                    O_RTN = CS0033AutoNumber.ERR
                    Exit Sub
                End If

                '---------------------------------------------------------
                'L1出力編集
                '---------------------------------------------------------
                L00001row("CAMPCODE") = T00004UPDrow("CAMPCODE")                              '会社コード
                L00001row("MOTOCHO") = "LOPLAN"                                               '元帳（非会計予定を設定）
                L00001row("VERSION") = "000"                                                  'バージョン
                L00001row("DENTYPE") = "T04"                                                  '伝票タイプ
                L00001row("TENKI") = "0"                                                      '統計転記
                L00001row("KEIJOYMD") = T00004UPDrow("KIJUNDATE")                             '計上日付（基準日を設定）
                L00001row("DENYMD") = T00004UPDrow("SHUKODATE")                               '伝票日付（出庫日を設定）
                '伝票番号
                L00001row("DENNO") = T00004UPDrow("ORDERORG") &
                                    CDate(T00004UPDrow("KIJUNDATE")).ToString("yyyy") &
                                    WW_SEQ
                '関連伝票No＋明細No
                L00001row("KANRENDENNO") = T00004UPDrow("ORDERORG") & " " _
                              & T00004UPDrow("ORDERNO") & " " _
                              & T00004UPDrow("TRIPNO") & " " _
                              & T00004UPDrow("DROPNO") & " " _
                              & T00004UPDrow("SEQ")

                L00001row("ACTORICODE") = ""                                                  '取引先コード
                L00001row("ACOILTYPE") = ""                                                   '油種
                L00001row("ACSHARYOTYPE") = ""                                                '統一車番(上)
                L00001row("ACTSHABAN") = ""                                                   '統一車番(下)
                L00001row("ACSTAFFCODE") = ""                                                 '従業員コード
                L00001row("ACBANKAC") = ""                                                    '銀行口座

                L00001row("ACKEIJOMORG") = T00004UPDrow("ORDERORG")                           '計上管理部署コード（受注部署）

                L00001row("ACTAXKBN") = ""                                                    '税区分
                L00001row("ACAMT") = 0                                                        '金額
                L00001row("NACSHUKODATE") = T00004UPDrow("SHUKODATE")                         '出庫日
                L00001row("NACSHUKADATE") = T00004UPDrow("SHUKADATE")                         '出荷日
                L00001row("NACTODOKEDATE") = T00004UPDrow("TODOKEDATE")                       '届日
                L00001row("NACTORICODE") = T00004UPDrow("TORICODE")                           '荷主コード
                L00001row("NACURIKBN") = T00004UPDrow("URIKBN")                               '売上計上基準
                L00001row("NACTODOKECODE") = T00004UPDrow("TODOKECODE")                       '届先コード
                L00001row("NACSTORICODE") = T00004UPDrow("STORICODE")                         '販売店コード
                L00001row("NACSHUKABASHO") = T00004UPDrow("SHUKABASHO")                       '出荷場所

                '取引先ORGより取得
                CS0041TORIORGget.TBL = MC003tbl
                CS0041TORIORGget.CAMPCODE = T00004UPDrow("CAMPCODE")
                CS0041TORIORGget.TORICODE = T00004UPDrow("TORICODE")
                CS0041TORIORGget.UORG = T00004UPDrow("SHIPORG")
                CS0041TORIORGget.CS0041TORIORGget()

                L00001row("NACTORITYPE01") = CS0041TORIORGget.TORITYPE01                    '取引先・取引タイプ01
                L00001row("NACTORITYPE02") = CS0041TORIORGget.TORITYPE02                    '取引先・取引タイプ02
                L00001row("NACTORITYPE03") = CS0041TORIORGget.TORITYPE03                    '取引先・取引タイプ03
                L00001row("NACTORITYPE04") = CS0041TORIORGget.TORITYPE04                    '取引先・取引タイプ04
                L00001row("NACTORITYPE05") = CS0041TORIORGget.TORITYPE05                    '取引先・取引タイプ05

                L00001row("NACOILTYPE") = T00004UPDrow("OILTYPE")                           '油種
                L00001row("NACPRODUCT1") = T00004UPDrow("PRODUCT1")                         '品名１
                L00001row("NACPRODUCT2") = T00004UPDrow("PRODUCT2")                         '品名２
                L00001row("NACPRODUCTCODE") = T00004UPDrow("PRODUCTCODE")                   '品名コード

                L00001row("NACGSHABAN") = T00004UPDrow("GSHABAN")                           '業務車番

                '車両マスタより
                CS0045GSHABANORGget.TBL = MA006tbl
                CS0045GSHABANORGget.CAMPCODE = T00004UPDrow("CAMPCODE")
                CS0045GSHABANORGget.UORG = T00004UPDrow("SHIPORG")
                CS0045GSHABANORGget.GSHABAN = T00004UPDrow("GSHABAN")
                CS0045GSHABANORGget.STYMD = T00004UPDrow("KIJUNDATE")
                CS0045GSHABANORGget.ENDYMD = T00004UPDrow("KIJUNDATE")
                CS0045GSHABANORGget.CS0045GSHABANORGget()

                If CS0045GSHABANORGget.MANGSUPPL = "" Then
                    L00001row("NACSUPPLIERKBN") = "1"                                       '社有・庸車区分
                    L00001row("NACSUPPLIER") = ""                                           '庸車会社
                Else
                    L00001row("NACSUPPLIERKBN") = "2"                                       '社有・庸車区分
                    L00001row("NACSUPPLIER") = CS0045GSHABANORGget.MANGSUPPL                '庸車会社
                End If

                L00001row("NACSHARYOOILTYPE") = CS0045GSHABANORGget.MANGOILTYPE             '車両登録油種

                L00001row("NACSHARYOTYPE1") = T00004UPDrow("SHARYOTYPEF")                   '統一車番(上)1
                L00001row("NACTSHABAN1") = T00004UPDrow("TSHABANF")                         '統一車番(下)1
                L00001row("NACMANGMORG1") = CS0045GSHABANORGget.MANGMORGF                   '車両管理部署1
                L00001row("NACMANGSORG1") = CS0045GSHABANORGget.MANGSORGF                   '車両設置部署1
                L00001row("NACMANGUORG1") = T00004UPDrow("SHIPORG")                         '車両運用部署1
                L00001row("NACBASELEASE1") = CS0045GSHABANORGget.BASELEASEF                 '車両所有1

                L00001row("NACSHARYOTYPE2") = T00004UPDrow("SHARYOTYPEB")                   '統一車番(上)2
                L00001row("NACTSHABAN2") = T00004UPDrow("TSHABANB")                         '統一車番(下)2
                L00001row("NACMANGMORG2") = CS0045GSHABANORGget.MANGMORGB                   '車両管理部署2
                L00001row("NACMANGSORG2") = CS0045GSHABANORGget.MANGSORGB                   '車両設置部署2
                L00001row("NACMANGUORG2") = T00004UPDrow("SHIPORG")                         '車両運用部署1
                L00001row("NACBASELEASE2") = CS0045GSHABANORGget.BASELEASEB                 '車両所有2

                L00001row("NACSHARYOTYPE3") = T00004UPDrow("SHARYOTYPEB2")                  '統一車番(上)3
                L00001row("NACTSHABAN3") = T00004UPDrow("TSHABANB2")                        '統一車番(下)3
                L00001row("NACMANGMORG3") = CS0045GSHABANORGget.MANGMORGB2                  '車両管理部署3
                L00001row("NACMANGSORG3") = CS0045GSHABANORGget.MANGSORGB2                  '車両設置部署3
                L00001row("NACMANGUORG3") = T00004UPDrow("SHIPORG")                         '車両運用部署1
                L00001row("NACBASELEASE3") = CS0045GSHABANORGget.BASELEASEB2                '車両所有3

                L00001row("NACCREWKBN") = "1"                                               '正副区分
                L00001row("NACSTAFFCODE") = T00004UPDrow("STAFFCODE")                       '従業員コード（正）
                '社員マスターより
                CS0043STAFFORGget.TBL = MB001tbl
                CS0043STAFFORGget.CAMPCODE = T00004UPDrow("CAMPCODE")
                CS0043STAFFORGget.STAFFCODE = T00004UPDrow("STAFFCODE")
                CS0043STAFFORGget.SORG = T00004UPDrow("SHIPORG")
                CS0043STAFFORGget.STYMD = T00004UPDrow("KIJUNDATE")
                CS0043STAFFORGget.ENDYMD = T00004UPDrow("KIJUNDATE")
                CS0043STAFFORGget.CS0043STAFFORGget()

                L00001row("NACSTAFFKBN") = CS0043STAFFORGget.STAFFKBN                       '社員区分（正）
                L00001row("NACMORG") = CS0043STAFFORGget.MORG                               '管理部署（正）
                L00001row("NACHORG") = CS0043STAFFORGget.HORG                               '配属部署（正）
                L00001row("NACSORG") = T00004UPDrow("SHIPORG")                              '作業部署（正）

                L00001row("NACSTAFFCODE2") = T00004UPDrow("SUBSTAFFCODE")                   '従業員コード（副）
                '社員マスターより
                CS0043STAFFORGget.TBL = MB001tbl
                CS0043STAFFORGget.CAMPCODE = T00004UPDrow("CAMPCODE")
                CS0043STAFFORGget.STAFFCODE = T00004UPDrow("SUBSTAFFCODE")
                CS0043STAFFORGget.SORG = T00004UPDrow("SHIPORG")
                CS0043STAFFORGget.STYMD = T00004UPDrow("KIJUNDATE")
                CS0043STAFFORGget.ENDYMD = T00004UPDrow("KIJUNDATE")
                CS0043STAFFORGget.CS0043STAFFORGget()

                L00001row("NACSTAFFKBN2") = CS0043STAFFORGget.STAFFKBN                      '社員区分（副）
                L00001row("NACMORG2") = CS0043STAFFORGget.MORG                              '管理部署（副）
                L00001row("NACHORG2") = CS0043STAFFORGget.HORG                              '配属部署（副）
                If T00004UPDrow("SUBSTAFFCODE") = "" Then
                    L00001row("NACSORG2") = ""                                              '作業部署（副）
                Else
                    L00001row("NACSORG2") = T00004UPDrow("SHIPORG")                         '作業部署（副）
                End If

                L00001row("NACORDERNO") = T00004UPDrow("ORDERNO")                           '受注番号
                L00001row("NACDETAILNO") = T00004UPDrow("DETAILNO")                         '明細№
                L00001row("NACTRIPNO") = T00004UPDrow("TRIPNO")                             'トリップ
                L00001row("NACDROPNO") = T00004UPDrow("DROPNO")                             'ドロップ
                L00001row("NACSEQ") = T00004UPDrow("SEQ")                                   'SEQ

                L00001row("NACORDERORG") = T00004UPDrow("ORDERORG")                         '受注部署
                L00001row("NACSHIPORG") = T00004UPDrow("SHIPORG")                           '配送部署
                L00001row("NACSURYO") = T00004UPDrow("SURYO")                               '受注・数量
                L00001row("NACTANI") = T00004UPDrow("HTANI")                                '受注・単位
                L00001row("NACJSURYO") = 0                                                  '実績・配送数量
                L00001row("NACSTANI") = ""                                                  '実績・配送単位
                L00001row("NACHAIDISTANCE") = 0                                             '実績・配送距離
                L00001row("NACKAIDISTANCE") = 0                                             '実績・回送作業距離
                L00001row("NACCHODISTANCE") = 0                                             '実績・勤怠調整距離
                L00001row("NACTTLDISTANCE") = 0                                             '実績・配送距離合計Σ
                L00001row("NACHAISTDATE") = C_DEFAULT_YMD                                   '実績・配送作業開始日時
                L00001row("NACHAIENDDATE") = C_DEFAULT_YMD                                  '実績・配送作業終了日時
                L00001row("NACHAIWORKTIME") = 0                                             '実績・配送作業時間（分）
                L00001row("NACGESSTDATE") = C_DEFAULT_YMD                                   '実績・下車作業開始日時
                L00001row("NACGESENDDATE") = C_DEFAULT_YMD                                  '実績・下車作業終了日時
                L00001row("NACGESWORKTIME") = 0                                             '実績・下車作業時間（分）
                L00001row("NACCHOWORKTIME") = 0                                             '実績・勤怠調整時間（分）
                L00001row("NACTTLWORKTIME") = 0                                             '実績・配送合計時間Σ（分）
                L00001row("NACOUTWORKTIME") = 0                                             '実績・就業外時間（分）
                L00001row("NACBREAKSTDATE") = C_DEFAULT_YMD                                 '実績・休憩開始日時
                L00001row("NACBREAKENDDATE") = C_DEFAULT_YMD                                '実績・休憩終了日時
                L00001row("NACBREAKTIME") = 0                                               '実績・休憩時間（分）
                L00001row("NACCHOBREAKTIME") = 0                                            '実績・休憩調整時間（分）
                L00001row("NACTTLBREAKTIME") = 0                                            '実績・休憩合計時間Σ（分）
                L00001row("NACCASH") = 0                                                    '実績・現金
                L00001row("NACETC") = 0                                                     '実績・ETC
                L00001row("NACTICKET") = 0                                                  '実績・回数券
                L00001row("NACKYUYU") = 0                                                   '実績・軽油
                L00001row("NACUNLOADCNT") = 0                                               '実績・荷卸回数
                L00001row("NACCHOUNLOADCNT") = 0                                            '実績・荷卸回数調整
                L00001row("NACTTLUNLOADCNT") = 0                                            '実績・荷卸回数合計Σ
                L00001row("NACKAIJI") = 0                                                   '実績・回次
                L00001row("NACJITIME") = 0                                                  '実績・実車時間（分）
                L00001row("NACJICHOSTIME") = 0                                              '実績・実車時間調整（分）
                L00001row("NACJITTLETIME") = 0                                              '実績・実車時間合計Σ（分）
                L00001row("NACKUTIME") = 0                                                  '実績・空車時間（分）
                L00001row("NACKUCHOTIME") = 0                                               '実績・空車時間調整（分）
                L00001row("NACKUTTLTIME") = 0                                               '実績・空車時間合計Σ（分）
                L00001row("NACJIDISTANCE") = 0                                              '実績・実車距離
                L00001row("NACJICHODISTANCE") = 0                                           '実績・実車距離調整
                L00001row("NACJITTLDISTANCE") = 0                                           '実績・実車距離合計Σ
                L00001row("NACKUDISTANCE") = 0                                              '実績・空車距離
                L00001row("NACKUCHODISTANCE") = 0                                           '実績・空車距離調整
                L00001row("NACKUTTLDISTANCE") = 0                                           '実績・空車距離合計Σ
                L00001row("NACTARIFFFARE") = 0                                              '実績・運賃タリフ額
                L00001row("NACFIXEDFARE") = 0                                               '実績・運賃固定額
                L00001row("NACINCHOFARE") = 0                                               '実績・運賃手入力調整額
                L00001row("NACTTLFARE") = 0                                                 '実績・運賃合計額Σ
                L00001row("NACOFFICESORG") = ""                                             '実績・作業部署
                L00001row("NACOFFICETIME") = 0                                              '実績・事務時間
                L00001row("NACOFFICEBREAKTIME") = 0                                         '実績・事務休憩時間
                L00001row("PAYSHUSHADATE") = C_DEFAULT_YMD                                  '出社日時
                L00001row("PAYTAISHADATE") = C_DEFAULT_YMD                                  '退社日時
                L00001row("PAYSTAFFCODE") = T00004UPDrow("STAFFCODE")                       '従業員コード
                L00001row("PAYSTAFFKBN") = L00001row("NACSTAFFKBN")                         '社員区分
                L00001row("PAYMORG") = L00001row("NACMORG")                                 '従業員管理部署
                L00001row("PAYHORG") = L00001row("NACHORG")                                 '従業員配属部署

                '休日区分取得
                GetHOLIDAYKBN(T00004UPDrow("CAMPCODE"), T00004UPDrow("SHUKODATE"), WW_hokidaykbn)

                L00001row("PAYHOLIDAYKBN") = WW_hokidaykbn                                  '休日区分
                L00001row("PAYKBN") = ""                                                    '勤怠区分
                L00001row("PAYSHUKCHOKKBN") = ""                                            '宿日直区分
                L00001row("PAYJYOMUKBN") = ""                                               '乗務区分
                L00001row("PAYOILKBN") = ""                                                 '勤怠用油種区分
                L00001row("PAYSHARYOKBN") = ""                                              '勤怠用車両区分
                L00001row("PAYWORKNISSU") = 0                                               '所労
                L00001row("PAYSHOUKETUNISSU") = 0                                           '傷欠
                L00001row("PAYKUMIKETUNISSU") = 0                                           '組欠
                L00001row("PAYETCKETUNISSU") = 0                                            '他欠
                L00001row("PAYNENKYUNISSU") = 0                                             '年休
                L00001row("PAYTOKUKYUNISSU") = 0                                            '特休
                L00001row("PAYCHIKOKSOTAINISSU") = 0                                        '遅早
                L00001row("PAYSTOCKNISSU") = 0                                              'ストック休暇
                L00001row("PAYKYOTEIWEEKNISSU") = 0                                         '協定週休
                L00001row("PAYWEEKNISSU") = 0                                               '週休
                L00001row("PAYDAIKYUNISSU") = 0                                             '代休
                L00001row("PAYWORKTIME") = 0                                                '所定労働時間（分）
                L00001row("PAYNIGHTTIME") = 0                                               '所定深夜時間（分）
                L00001row("PAYORVERTIME") = 0                                               '平日残業時間（分）
                L00001row("PAYWNIGHTTIME") = 0                                              '平日深夜時間（分）
                L00001row("PAYWSWORKTIME") = 0                                              '日曜出勤時間（分）
                L00001row("PAYSNIGHTTIME") = 0                                              '日曜深夜時間（分）
                L00001row("PAYHWORKTIME") = 0                                               '休日出勤時間（分）
                L00001row("PAYHNIGHTTIME") = 0                                              '休日深夜時間（分）
                L00001row("PAYBREAKTIME") = 0                                               '休憩時間（分）

                L00001row("PAYNENSHINISSU") = 0                                             '年始出勤
                L00001row("PAYSHUKCHOKNNISSU") = 0                                          '宿日直年始
                L00001row("PAYSHUKCHOKNISSU") = 0                                           '宿日直通常
                L00001row("PAYSHUKCHOKNHLDNISSU") = 0                                       '宿日直年始（翌日休み）
                L00001row("PAYSHUKCHOKHLDNISSU") = 0                                        '宿日直通常（翌日休み）
                L00001row("PAYTOKSAAKAISU") = 0                                             '特作A
                L00001row("PAYTOKSABKAISU") = 0                                             '特作B
                L00001row("PAYTOKSACKAISU") = 0                                             '特作C
                L00001row("PAYTENKOKAISU") = 0                                              '点呼回数
                L00001row("PAYHOANTIME") = 0                                                '保安検査入力（分）
                L00001row("PAYKOATUTIME") = 0                                               '高圧作業入力（分）
                L00001row("PAYTOKUSA1TIME") = 0                                             '特作Ⅰ（分）
                L00001row("PAYPONPNISSU") = 0                                               'ポンプ
                L00001row("PAYBULKNISSU") = 0                                               'バルク
                L00001row("PAYTRAILERNISSU") = 0                                            'トレーラ
                L00001row("PAYBKINMUKAISU") = 0                                             'B勤務
                L00001row("PAYYENDTIME") = "00:00"                                          '予定退社時刻
                L00001row("PAYAPPLYID") = ""                                                '申請ID
                L00001row("PAYRIYU") = ""                                                   '理由コード
                L00001row("PAYRIYUETC") = ""                                                '理由その他
                L00001row("APPKIJUN") = ""                                                  '配賦基準
                L00001row("APPKEY") = ""                                                    '配賦統計キー

                L00001row("WORKKBN") = ""                                                   '作業区分
                L00001row("KEYSTAFFCODE") = T00004UPDrow("STAFFCODE")                       '従業員コードキー
                L00001row("KEYGSHABAN") = T00004UPDrow("GSHABAN")                           '業務車番キー
                L00001row("KEYTRIPNO") = T00004UPDrow("TRIPNO")                             'トリップキー
                L00001row("KEYDROPNO") = T00004UPDrow("DROPNO")                             'ドロップキー

                L00001row("DELFLG") = C_DELETE_FLG.ALIVE                                    '削除フラグ

                '勘定科目判定テーブル検索（共通設定項目）
                CS0038ACCODEget.TBL = WW_M0008tbl                                           '勘定科目判定テーブル
                CS0038ACCODEget.CAMPCODE = L00001row("CAMPCODE")                            '会社コード
                CS0038ACCODEget.STYMD = L00001row("KEIJOYMD")                               '開始日
                CS0038ACCODEget.ENDYMD = L00001row("KEIJOYMD")                              '終了日
                CS0038ACCODEget.MOTOCHO = "LOPLAN"                                          '元帳
                CS0038ACCODEget.DENTYPE = "T04"                                             '伝票タイプ

                CS0038ACCODEget.TORICODE = L00001row("NACTORICODE")                         '荷主コード
                CS0038ACCODEget.TORITYPE01 = L00001row("NACTORITYPE01")                     '取引タイプ01
                CS0038ACCODEget.TORITYPE02 = L00001row("NACTORITYPE02")                     '取引タイプ02
                CS0038ACCODEget.TORITYPE03 = L00001row("NACTORITYPE03")                     '取引タイプ03
                CS0038ACCODEget.TORITYPE04 = L00001row("NACTORITYPE04")                     '取引タイプ04
                CS0038ACCODEget.TORITYPE05 = L00001row("NACTORITYPE05")                     '取引タイプ05
                CS0038ACCODEget.URIKBN = L00001row("NACURIKBN")                             '売上計上基準
                CS0038ACCODEget.STORICODE = L00001row("NACSTORICODE")                       '販売店コード
                CS0038ACCODEget.OILTYPE = L00001row("NACOILTYPE")                           '油種
                CS0038ACCODEget.PRODUCT1 = L00001row("NACPRODUCT1")                         '品名１
                CS0038ACCODEget.SUPPLIERKBN = L00001row("NACSUPPLIERKBN")                   '社有・庸車区分
                CS0038ACCODEget.MANGSORG = L00001row("NACMANGSORG1")                        '車両設置部署
                CS0038ACCODEget.MANGUORG = L00001row("NACMANGUORG1")                        '車両運用部署
                CS0038ACCODEget.BASELEASE = L00001row("NACBASELEASE1")                      '車両所有
                CS0038ACCODEget.STAFFKBN = L00001row("NACSTAFFKBN")                         '社員区分
                CS0038ACCODEget.HORG = L00001row("NACHORG")                                 '配属部署
                CS0038ACCODEget.SORG = L00001row("NACSORG")                                 '作業部署

                '勘定科目判定テーブル検索（借方）
                CS0038ACCODEget.ACHANTEI = "HID"                                            '勘定科目判定コード
                CS0038ACCODEget.CS0038ACCODEget()
                Dim WW_ACCODE_D As String = CS0038ACCODEget.ACCODE
                Dim WW_SUBACCODE_D As String = CS0038ACCODEget.SUBACCODE
                Dim WW_INQKBN_D As String = CS0038ACCODEget.INQKBN

                '勘定科目判定テーブル検索（貸方）
                CS0038ACCODEget.ACHANTEI = "HIC"                                            '勘定科目判定コード
                CS0038ACCODEget.CS0038ACCODEget()
                Dim WW_ACCODE_C As String = CS0038ACCODEget.ACCODE
                Dim WW_SUBACCODE_C As String = CS0038ACCODEget.SUBACCODE
                Dim WW_INQKBN_C As String = CS0038ACCODEget.INQKBN

                Dim WW_ROW As DataRow

                '------------------------------------------------------
                '追加データ
                '------------------------------------------------------
                '●借方
                If WW_INQKBN_D = "1" Then
                    L00001row("ACCODE") = WW_ACCODE_D                                 '勘定科目コード
                    L00001row("SUBACCODE") = WW_SUBACCODE_D                           '補助科目コード
                    L00001row("INQKBN") = WW_INQKBN_D                                 '照会区分
                    L00001row("ACDCKBN") = "D"                                        '貸借区分
                    L00001row("ACACHANTEI") = "HID"                                   '勘定科目判定コード
                    L00001row("DTLNO") = "01"                                         '明細番号
                    L00001row("ACKEIJOORG") = T00004UPDrow("ORDERORG")                '計上部署コード（受注部署）

                    WW_ROW = L00001tbl.NewRow
                    WW_ROW.ItemArray = L00001row.ItemArray
                    L00001tbl.Rows.Add(WW_ROW)
                End If
                '●貸方
                If WW_INQKBN_C = "1" Then
                    L00001row("ACCODE") = WW_ACCODE_C                                 '勘定科目コード
                    L00001row("SUBACCODE") = WW_SUBACCODE_C                           '補助科目コード
                    L00001row("INQKBN") = WW_INQKBN_C                                 '照会区分
                    L00001row("ACDCKBN") = "C"                                        '貸借区分
                    L00001row("ACACHANTEI") = "HIC"                                   '勘定科目判定コード
                    L00001row("DTLNO") = "02"                                         '明細番号
                    L00001row("ACKEIJOORG") = T00004UPDrow("SHIPORG")                 '計上部署コード（配送部署）

                    WW_ROW = L00001tbl.NewRow
                    WW_ROW.ItemArray = L00001row.ItemArray
                    L00001tbl.Rows.Add(WW_ROW)
                End If

            Next

        End Sub

        ''' <summary>
        ''' 統計DB更新
        ''' </summary>
        ''' <param name="O_RTN" >ERR</param>
        ''' <remarks></remarks>
        Public Sub Update(ByRef T00004UPDtbl As DataTable, ByRef O_RTN As String)

            Dim WW_DATENOW As Date = Date.Now

            O_RTN = C_MESSAGE_NO.NORMAL

            If IsNothing(Me.SQLcon) Then
                'DataBase接続文字
                Me.SQLcon = sm.getConnection
                SQLcon.Open() 'DataBase接続(Open)
            End If

            '日報ＤＢ更新
            Dim SQLStr As String =
                        "UPDATE L0001_TOKEI " _
                      & "SET DELFLG         = '1' " _
                      & "  , UPDYMD         = @P08 " _
                      & "  , UPDUSER        = @P09 " _
                      & "  , UPDTERMID      = @P10 " _
                      & "  , RECEIVEYMD     = @P11  " _
                      & "WHERE CAMPCODE     = @P01 " _
                      & "  and DENTYPE      = @P02 " _
                      & "  and NACSHUKODATE = @P03 " _
                      & "  and KEYSTAFFCODE = @P04 " _
                      & "  and KEYGSHABAN   = @P05 " _
                      & "  and KEYTRIPNO    = @P06 " _
                      & "  and KEYDROPNO    = @P07 " _
                      & "  and DELFLG      <> '1' ; "

            Dim SQLcmd As SqlCommand = New SqlCommand(SQLStr, SQLcon)
            Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", System.Data.SqlDbType.NVarChar, 20)
            Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", System.Data.SqlDbType.NVarChar, 20)
            Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@P03", System.Data.SqlDbType.Date)
            Dim PARA04 As SqlParameter = SQLcmd.Parameters.Add("@P04", System.Data.SqlDbType.NVarChar, 20)
            Dim PARA05 As SqlParameter = SQLcmd.Parameters.Add("@P05", System.Data.SqlDbType.NVarChar, 20)
            Dim PARA06 As SqlParameter = SQLcmd.Parameters.Add("@P06", System.Data.SqlDbType.NVarChar, 20)
            Dim PARA07 As SqlParameter = SQLcmd.Parameters.Add("@P07", System.Data.SqlDbType.NVarChar, 20)
            Dim PARA08 As SqlParameter = SQLcmd.Parameters.Add("@P08", System.Data.SqlDbType.DateTime)
            Dim PARA09 As SqlParameter = SQLcmd.Parameters.Add("@P09", System.Data.SqlDbType.NVarChar, 20)
            Dim PARA10 As SqlParameter = SQLcmd.Parameters.Add("@P10", System.Data.SqlDbType.NVarChar, 30)
            Dim PARA11 As SqlParameter = SQLcmd.Parameters.Add("@P11", System.Data.SqlDbType.DateTime)


            '■■■ 統計ＤＢ出力 ■■■
            '
            For Each T00004UPDrow In T00004UPDtbl.Rows

                If T00004UPDrow("TIMSTP") <> "0" AndAlso
                   T00004UPDrow("OPERATION") = C_LIST_OPERATION_CODE.UPDATING Then
                Else
                    Continue For
                End If

                Try

                    PARA01.Value = T00004UPDrow("CAMPCODE")
                    PARA02.Value = "T04"
                    PARA03.Value = T00004UPDrow("SHUKODATE")
                    PARA04.Value = T00004UPDrow("STAFFCODE")
                    PARA05.Value = T00004UPDrow("GSHABAN")
                    PARA06.Value = T00004UPDrow("TRIPNO")
                    PARA07.Value = T00004UPDrow("DROPNO")
                    PARA08.Value = WW_DATENOW
                    PARA09.Value = UPDUSERID
                    PARA10.Value = UPDTERMID
                    PARA11.Value = C_DEFAULT_YMD

                    SQLcmd.CommandTimeout = 300
                    SQLcmd.ExecuteNonQuery()

                Catch ex As Exception
                    O_RTN = C_MESSAGE_NO.DB_ERROR
                    PutLog(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, ex.ToString)

                    Exit Sub
                End Try

            Next
            'CLOSE
            SQLcmd.Dispose()
            SQLcmd = Nothing

            For Each L00001row In L00001tbl.Rows

                L00001row("INITYMD") = WW_DATENOW '登録年月日
                L00001row("UPDYMD") = WW_DATENOW  '更新年月日
                L00001row("UPDUSER") = UPDUSERID  '更新ユーザＩＤ
                L00001row("UPDTERMID") = UPDTERMID   '更新端末
                L00001row("RECEIVEYMD") = C_DEFAULT_YMD  '集信日時

            Next

            CS0044L1INSERT.CS0044L1Insert(L00001tbl)

        End Sub

        ''' <summary>
        ''' カレンダー取得 
        ''' </summary>
        ''' <param name="I_CAMPCODE" >会社コード</param>
        ''' <param name="I_WORKINGYMD" >日付</param>
        ''' <param name="O_HOLIDAYKBN" >休日区分</param>
        ''' <remarks></remarks>
        Private Sub GetHOLIDAYKBN(ByVal I_CAMPCODE As String, ByVal I_WORKINGYMD As Date, ByRef O_HOLIDAYKBN As String)

            Dim dicKey As String = I_CAMPCODE & "_" & I_WORKINGYMD.ToString("yyyy/MM/dd")

            Try
                ' 指定された会社コード・日付が未取得時は指定会社の日付全件取得
                If Not _dicCal.ContainsKey(dicKey) Then

                    Dim SQLStr As String =
                         "SELECT CAMPCODE " _
                       & ", WORKINGYMD " _
                       & ", isnull(rtrim(WORKINGKBN),'') as WORKINGKBN " _
                       & " FROM  MB005_CALENDAR " _
                       & " Where CAMPCODE   = @CAMPCODE " _
                       & "   and DELFLG    <> @DELFLG "

                    Dim SQLcmd As New SqlCommand(SQLStr, SQLcon)
                    Dim P_CAMPCODE As SqlParameter = SQLcmd.Parameters.Add("@CAMPCODE", System.Data.SqlDbType.NVarChar, 20)
                    Dim P_DELFLG As SqlParameter = SQLcmd.Parameters.Add("@DELFLG", System.Data.SqlDbType.NVarChar, 1)
                    P_CAMPCODE.Value = I_CAMPCODE
                    P_DELFLG.Value = C_DELETE_FLG.DELETE

                    Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

                    While SQLdr.Read
                        _dicCal(SQLdr("CAMPCODE") & "_" & SQLdr("WORKINGYMD")) = SQLdr("WORKINGKBN")
                    End While
                    'Close
                    SQLdr.Close() 'Reader(Close)
                    SQLdr = Nothing

                    SQLcmd.Dispose()
                    SQLcmd = Nothing

                End If

                O_HOLIDAYKBN = _dicCal.Item(dicKey)

            Catch ex As Exception
                PutLog(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT)
                'ログ出力
                Exit Sub
            End Try

        End Sub
    End Class
#End Region

#Region "<< 帳票追加>>"
    ''' <summary>
    ''' 部署マスタデータ帳票出力
    ''' </summary>
    ''' <remarks></remarks>
    Public Class AddReportOrgData
        Inherits GRT00016COM

        ''' <summary>
        ''' マスタ種別
        ''' </summary>
        Public Enum DATA_TYPE As Integer
            ''' <summary>
            ''' 全て
            ''' </summary>
            ALL
            ''' <summary>
            ''' 部署
            ''' </summary>
            ORG
            ''' <summary>
            ''' 車両
            ''' </summary>
            SHARYO
            ''' <summary>
            ''' 取引先
            ''' </summary>
            TORI
            ''' <summary>
            ''' 出荷場所
            ''' </summary>
            SHUKABASHO
            ''' <summary>
            ''' 届先
            ''' </summary>
            TODOKE
            ''' <summary>
            ''' 従業員（乗務員）
            ''' </summary>
            STAFF
            ''' <summary>
            ''' 品名
            ''' </summary>
            PRODUCT
            ''' <summary>
            ''' 油種
            ''' </summary>
            OILTYPE
            ''' <summary>
            ''' 品名1
            ''' </summary>
            PRODUCT1
            ''' <summary>
            ''' 品名2
            ''' </summary>
            PRODUCT2
        End Enum

        ' データ種別, タイトル名称, サブタイトル名称
        Private SERCH_LIST As Tuple(Of Integer, String, String)() = {
            New Tuple(Of Integer, String, String)(DATA_TYPE.ORG, "事業所リスト", "車庫名称"),
            New Tuple(Of Integer, String, String)(DATA_TYPE.SHARYO, "車両リスト", "登録番号"),
            New Tuple(Of Integer, String, String)(DATA_TYPE.TODOKE, "届先リスト", "届先名称"),
            New Tuple(Of Integer, String, String)(DATA_TYPE.TORI, "届先リスト", "荷主名称"),
            New Tuple(Of Integer, String, String)(DATA_TYPE.SHUKABASHO, "届先リスト", "出荷場名称"),
            New Tuple(Of Integer, String, String)(DATA_TYPE.STAFF, "乗務員リスト", "乗務員名称"),
            New Tuple(Of Integer, String, String)(DATA_TYPE.OILTYPE, "品名リスト", "油種名称"),
            New Tuple(Of Integer, String, String)(DATA_TYPE.PRODUCT1, "品名リスト", "品名1名称"),
            New Tuple(Of Integer, String, String)(DATA_TYPE.PRODUCT, "品名リスト", "品名2名称")
        }
        '   New Tuple(Of Integer, String, String)(DATA_TYPE.PRODUCT2, "品名リスト", "品名2名称"),
        '   New Tuple(Of Integer, String, String)(DATA_TYPE.PRODUCT, "品名リスト", "品名")

#Region "<< Class Property >>"
        ''' <summary>
        ''' 会社コード
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property CAMPCODE As String

        ''' <summary>
        ''' 部署コード
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property UORG As String

        ''' <summary>
        ''' ROLECODE
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property ROLECODE() As String

        ''' <summary>
        ''' 開始年月日
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property STYMD As String

        ''' <summary>
        ''' 終了年月日
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property ENDYMD As String

        ''' <summary>
        ''' 出力Dir＋ファイル名
        ''' </summary>
        ''' <value></value>
        ''' <returns>出力Dir＋ファイル名</returns>
        ''' <remarks></remarks>
        Public Property FILEPATH As String

        ''' <summary>
        ''' 対象シート名
        ''' </summary>
        ''' <value></value>
        ''' <returns>ExcelBookのシート名</returns>
        ''' <remarks></remarks>
        Public Property SHEETNAME As String

#End Region
        ''' <summary>
        ''' 部署マスタデータ追加出力
        ''' </summary>
        ''' <param name="_filePath">対象ファイル</param>
        ''' <param name="_sheetName">対象シート名称</param>
        ''' <remarks></remarks>
        Public Sub AddOrgData(Optional ByVal _filePath As String = "", Optional ByVal _sheetName As String = "")
#Region "<< パラメータチェック >>"

            Dim filePath As String
            If _filePath = "" Then
                filePath = Me.FILEPATH
            Else
                filePath = _filePath
            End If

            Dim sheetName As String
            If _sheetName = "" Then
                sheetName = Me.SHEETNAME
            Else
                sheetName = _sheetName
            End If

            '●In PARAMチェック
            Dim paramDic = New Dictionary(Of String, String) From {
                {"CAMPCODE", Me.CAMPCODE},
                {"ORGCODE", Me.UORG},
                {"ROLECODE", Me.ROLECODE},
                {"FILEPATH", filePath},
                {"SHEETNAME", sheetName}
            }

            For Each item In paramDic
                If String.IsNullOrEmpty(item.Value) Then
                    Err = C_MESSAGE_NO.DLL_IF_ERROR
                    PutLog(C_MESSAGE_NO.DLL_IF_ERROR, C_MESSAGE_TYPE.ABORT, C_MESSAGE_TEXT.IN_PARAM_ERROR_TEXT)
                    Exit Sub
                End If
            Next

            'ファイル存在チェック
            If Not File.Exists(filePath) Then
                Err = C_MESSAGE_NO.FILE_IO_ERROR
                PutLog(C_MESSAGE_NO.FILE_IO_ERROR, C_MESSAGE_TYPE.ABORT, "File(" & filePath & ")")
                Exit Sub
            End If
#End Region

            Dim W_ExcelApp As Excel.Application = Nothing
            Dim W_ExcelBooks As Excel.Workbooks = Nothing
            Dim W_ExcelBook As Excel.Workbook = Nothing
            Dim W_ExcelSheets As Excel.Sheets = Nothing
            Dim W_ExcelSheet As Excel.Worksheet = Nothing
            Dim W_ExcelSerch As Excel.Range = Nothing

            Dim W_Cells As Excel.Range = Nothing
            Dim W_Rows As Excel.Range = Nothing
            Dim W_Range As Excel.Range = Nothing
            Dim W_RangeS As Excel.Range = Nothing
            Dim W_RangeE As Excel.Range = Nothing

            '出力位置検索用
            Dim hitPos As Excel.Range = Nothing
            Dim hitPosSub As Excel.Range = Nothing
            '編集用
            Dim editRange(,) As Object = Nothing

            Try
                '■Excel起動
                W_ExcelApp = New Excel.Application
                W_ExcelBooks = W_ExcelApp.Workbooks
                W_ExcelBook = W_ExcelBooks.Open(filePath)
                W_ExcelSheets = W_ExcelBook.Worksheets

                'シート存在チェック
                'シートなければ処理終了
                '※シート自動追加はしない
                Dim sheetFind As Boolean = False
                For i = 1 To W_ExcelSheets.Count
                    W_ExcelSheet = CType(W_ExcelSheets(i), Excel.Worksheet)
                    If W_ExcelSheet.Name = sheetName Then
                        sheetFind = True
                        Exit For
                    End If
                    ExcelMemoryRelease(W_ExcelSheet)
                Next
                If sheetFind = False Then
                    Exit Sub
                End If

                'バックグラウンド
                W_ExcelApp.Visible = False
                '自動計算を止める
                W_ExcelApp.Calculation = Excel.XlCalculation.xlCalculationManual

                '検索範囲
                W_ExcelSerch = W_ExcelSheet.Range("A1:AZ5")

                For Each target In SERCH_LIST
                    Dim cellRow As Integer = 0
                    Dim cellCol As Integer = 0

                    '***** 出力データ特定 *****
                    'タイトル名称検索
                    hitPos = W_ExcelSerch.Find(target.Item2)
                    If IsNothing(hitPos) Then Continue For
                    If Not String.IsNullOrEmpty(target.Item3) Then
                        'サブタイトル検索（タイトル発見位置以降で）
                        hitPosSub = W_ExcelSerch.Find(target.Item3, hitPos)
                        If IsNothing(hitPosSub) Then Exit Sub
                        cellRow = hitPosSub.Row + 1
                        cellCol = hitPosSub.Column
                    Else
                        cellRow = hitPos.Row + 1
                        cellCol = hitPos.Column
                    End If
                    ExcelMemoryRelease(hitPosSub)
                    ExcelMemoryRelease(hitPos)

                    '***** データ取得 *****
                    Dim dataList As List(Of KeyValuePair(Of String, String)) = Nothing
                    If GetOrgData(target.Item1, dataList) = False Then
                        Exit Sub
                    End If

                    '***** Excelデータ出力処理 *****
                    W_Cells = W_ExcelSheet.Cells
                    W_Rows = W_ExcelSheet.Rows
                    '既存一覧クリア
                    W_RangeS = DirectCast(W_Cells.Item(cellRow, cellCol), Excel.Range)
                    W_RangeE = DirectCast(W_Cells.Item(W_Rows.Count, cellCol + 1), Excel.Range)
                    W_Range = W_ExcelSheet.Range(W_RangeS, W_RangeE)
                    W_Range.ClearContents()
                    ExcelMemoryRelease(W_Range)
                    ExcelMemoryRelease(W_RangeS)
                    ExcelMemoryRelease(W_RangeE)

                    '編集用エリア
                    W_RangeS = DirectCast(W_Cells.Item(cellRow, cellCol), Excel.Range)
                    W_RangeE = DirectCast(W_Cells.Item(cellRow + dataList.Count, cellCol + 1), Excel.Range)
                    W_Range = W_ExcelSheet.Range(W_RangeS, W_RangeE)
                    editRange = CType(W_Range.Value, Object(,))

                    'データ編集(名称・コード)
                    For i As Integer = 0 To dataList.Count - 1
                        editRange(i + 1, 1) = dataList(i).Value
                        editRange(i + 1, 2) = dataList(i).Key
                    Next

                    'データ貼り付け
                    W_Range.NumberFormatLocal = "@"
                    W_Range.Value = editRange

                    ExcelMemoryRelease(editRange)
                    ExcelMemoryRelease(W_Range)
                    ExcelMemoryRelease(W_RangeS)
                    ExcelMemoryRelease(W_RangeE)
                    ExcelMemoryRelease(W_Rows)
                    ExcelMemoryRelease(W_Cells)
                Next

                'Save
                W_ExcelApp.Calculation = Excel.XlCalculation.xlCalculationAutomatic
                W_ExcelBook.Save()

                Err = C_MESSAGE_NO.NORMAL

            Catch ex As Exception
                Err = C_MESSAGE_NO.WAIT_OTHER_EXCEL_JOB
                PutLog(C_MESSAGE_NO.WAIT_OTHER_EXCEL_JOB, C_MESSAGE_TYPE.ABORT, ex.ToString)

            Finally

                ExcelMemoryRelease(editRange)
                ExcelMemoryRelease(W_Range)
                ExcelMemoryRelease(W_RangeS)
                ExcelMemoryRelease(W_RangeE)
                ExcelMemoryRelease(W_Rows)
                ExcelMemoryRelease(W_Cells)
                ExcelMemoryRelease(hitPosSub)
                ExcelMemoryRelease(hitPos)
                ExcelMemoryRelease(W_ExcelSerch)
                ExcelMemoryRelease(W_ExcelSheet)
                ExcelMemoryRelease(W_ExcelSheets)

                If Not IsNothing(W_ExcelBook) Then
                    W_ExcelApp.DisplayAlerts = False
                    W_ExcelBook.Close(False)
                    W_ExcelApp.DisplayAlerts = True
                End If
                If Not IsNothing(W_ExcelApp) Then
                    'W_ExcelApp.Visible = True
                    W_ExcelApp.Quit()
                End If

                ExcelMemoryRelease(W_ExcelBook)
                ExcelMemoryRelease(W_ExcelBooks)
                ExcelMemoryRelease(W_ExcelApp)
            End Try

        End Sub

        ''' <summary>
        ''' 出力対象データ取得
        ''' </summary>
        ''' <param name="orgType">データ種別</param>
        ''' <returns>データリスト</returns>
        Private Function GetOrgData(ByVal orgType As Byte, ByRef orgData As List(Of KeyValuePair(Of String, String)))
            Dim glList As GL0000 = Nothing
            Dim wkList = New ListBox
            If IsNothing(orgData) Then
                orgData = New List(Of KeyValuePair(Of String, String))
            Else
                orgData.Clear()
            End If

            Try
                Select Case orgType
                    Case DATA_TYPE.ORG
                        glList = New GL0002OrgList With {
                        .CAMPCODE = CAMPCODE,
                        .ORGCODE = UORG,
                        .AUTHWITH = GL0002OrgList.LS_AUTHORITY_WITH.NO_AUTHORITY_WITH_ORG,
                        .Categorys = New String() {GL0002OrgList.C_CATEGORY_LIST.CARAGE}
                    }
                    Case DATA_TYPE.SHARYO
                        glList = New GL0008WorkLorryList With {
                        .CAMPCODE = CAMPCODE,
                        .ORGCODE = UORG
                    }
                    Case DATA_TYPE.TORI
                        glList = New GL0003CustomerList With {
                        .CAMPCODE = CAMPCODE,
                        .ORGCODE = UORG,
                        .ROLECODE = ROLECODE,
                        .PERMISSION = C_PERMISSION.REFERLANCE
                    }
                    Case DATA_TYPE.SHUKABASHO
                        glList = New GL0004DestinationList With {
                        .CAMPCODE = CAMPCODE,
                        .ORGCODE = UORG,
                        .ROLECODE = ROLECODE,
                        .PERMISSION = C_PERMISSION.REFERLANCE,
                        .TYPE = GL0004DestinationList.LC_DEST_TYPE.EXCEPT_JXCOSMO,
                        .CLASSCODE = "2"
                    }
                    Case DATA_TYPE.TODOKE
                        glList = New GL0004DestinationList With {
                        .CAMPCODE = CAMPCODE,
                        .ORGCODE = UORG,
                        .ROLECODE = ROLECODE,
                        .PERMISSION = C_PERMISSION.REFERLANCE,
                        .TYPE = GL0004DestinationList.LC_DEST_TYPE.EXCEPT_JXCOSMO,
                        .CLASSCODE = "1"
                    }
                    Case DATA_TYPE.STAFF
                        glList = New GL0005StaffList With {
                        .CAMPCODE = CAMPCODE,
                        .ORGCODE = UORG,
                        .ROLECODE = ROLECODE,
                        .PERMISSION = C_PERMISSION.REFERLANCE,
                        .TYPE = GL0005StaffList.LC_STAFF_TYPE.DRIVER
                    }
                    Case DATA_TYPE.PRODUCT
                        glList = New GL0006GoodsList With {
                        .CAMPCODE = CAMPCODE,
                        .ORGCODE = UORG,
                        .ROLECODE = ROLECODE,
                        .ORGCAMPCODE = CAMPCODE,
                        .PERMISSION = C_PERMISSION.REFERLANCE,
                        .TYPE = GL0006GoodsList.LC_GOODS_TYPE.GOODS_IN_ORG,
                        .OILTYPE = String.Empty,
                        .PRODUCT1 = String.Empty
                    }
                    Case DATA_TYPE.OILTYPE
                        glList = New GL0006GoodsList With {
                        .CAMPCODE = CAMPCODE,
                        .ORGCODE = UORG,
                        .ROLECODE = ROLECODE,
                        .ORGCAMPCODE = CAMPCODE,
                        .PERMISSION = C_PERMISSION.REFERLANCE,
                        .TYPE = GL0006GoodsList.LC_GOODS_TYPE.OILTYPE_IN_ORG
                    }
                    Case DATA_TYPE.PRODUCT1
                        glList = New GL0006GoodsList With {
                        .CAMPCODE = CAMPCODE,
                        .ORGCODE = UORG,
                        .ROLECODE = ROLECODE,
                        .ORGCAMPCODE = CAMPCODE,
                        .PERMISSION = C_PERMISSION.REFERLANCE,
                        .TYPE = GL0006GoodsList.LC_GOODS_TYPE.GOODS1_IN_ORG,
                        .OILTYPE = String.Empty
                    }
                    Case DATA_TYPE.PRODUCT2
                        glList = New GL0006GoodsList With {
                        .CAMPCODE = CAMPCODE,
                        .ORGCODE = UORG,
                        .ROLECODE = ROLECODE,
                        .ORGCAMPCODE = CAMPCODE,
                        .PERMISSION = C_PERMISSION.REFERLANCE,
                        .TYPE = GL0006GoodsList.LC_GOODS_TYPE.GOODS2_IN_ORG,
                        .OILTYPE = String.Empty,
                        .PRODUCT1 = String.Empty
                    }
                    Case Else
                        Me.ERR = C_MESSAGE_NO.DLL_IF_ERROR
                        Return False
                End Select

                Date.TryParse(STYMD, glList.STYMD)
                Date.TryParse(ENDYMD, glList.ENDYMD)
                glList.LIST = wkList
                'データ取得(ListItem)
                glList.getList()
                If Not isNormal(glList.ERR) Then
                    '取得エラー
                    Me.ERR = glList.ERR
                    Return False
                End If

                '先頭空行追加
                orgData.Add(New KeyValuePair(Of String, String)("", ""))
                For Each item As ListItem In wkList.Items
                    orgData.Add(New KeyValuePair(Of String, String)(item.Value, item.Text))
                Next

                Return True
            Catch ex As Exception
                Me.ERR = C_MESSAGE_NO.SYSTEM_ADM_ERROR
                Return False
            Finally
                wkList = Nothing
                glList = Nothing
            End Try

        End Function

        ''' <summary>
        ''' Excel操作のメモリ開放
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="objCom"></param>
        ''' <remarks></remarks>
        Public Sub ExcelMemoryRelease(Of T As Class)(ByRef objCom As T)

            'ランタイム実行対象がComObjectのアンマネージコードの場合、メモリ開放
            If objCom Is Nothing Then
                Return
            Else
                Try
                    If Marshal.IsComObject(objCom) Then
                        Dim count As Integer = Marshal.FinalReleaseComObject(objCom)
                    End If
                Finally
                    objCom = Nothing
                End Try
            End If

        End Sub
    End Class

#End Region
End Namespace

