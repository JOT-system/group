﻿Imports System.Data.SqlClient

''' <summary>
''' 項目チェック編集
''' </summary>
''' <remarks>データフィールドによる項目のチェック FIXFIELDの会社コードは見直しが必要</remarks>
Public Structure CS0024FCHECK

    ''' <summary>
    ''' 会社コード
    ''' </summary>
    ''' <value>会社コード</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property CAMPCODE() As String

    ''' <summary>
    ''' チェック対象の画面ID
    ''' </summary>
    ''' <value>画面ID</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property MAPID() As String

    ''' <summary>
    ''' チェック対象の項目名
    ''' </summary>
    ''' <value>項目名</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property FIELD() As String

    ''' <summary>
    ''' チェック対象の内容
    ''' </summary>
    ''' <value>内容</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property VALUE() As String

    ''' <summary>
    ''' 結果LIST
    ''' </summary>
    ''' <value></value>
    ''' <returns>結果LIST</returns>
    ''' <remarks></remarks>
    Public Property CHECKREPORT() As String

    ''' <summary>
    ''' 編集結果の内容
    ''' </summary>
    ''' <value>内容</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property VALUEOUT() As String
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
    Public Const METHOD_NAME As String = "CS0024FCHECK"

    ''' <summary>
    ''' FILED　DATAによるチェック機能
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub CS0024FCHECK()


        '●In PARAMチェック

        'PARAM01: CAMPCODE
        If IsNothing(CAMPCODE) Then
            Dim CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get

            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME                'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "CAMPCODE"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = C_MESSAGE_TEXT.IN_PARAM_ERROR_TEXT
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            ERR = C_MESSAGE_NO.DLL_IF_ERROR
            Exit Sub
        End If

        'PARAM02: MAPID
        If IsNothing(MAPID) Then
            Dim CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get

            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME                'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "MAPID"                          '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                   '
            CS0011LOGWRITE.TEXT = C_MESSAGE_TEXT.IN_PARAM_ERROR_TEXT
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            ERR = C_MESSAGE_NO.DLL_IF_ERROR
            Exit Sub
        End If

        'PARAM03: FIELD
        If IsNothing(FIELD) Then
            Dim CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get

            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME                'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "FIELD"                          '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                   '
            CS0011LOGWRITE.TEXT = C_MESSAGE_TEXT.IN_PARAM_ERROR_TEXT
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            ERR = C_MESSAGE_NO.DLL_IF_ERROR
            Exit Sub
        End If
        'PARAM04: VALUE  空白を認める

        'セッション制御宣言
        Dim sm As New CS0050SESSION

        '●項目情報取得
        '○ DB(S0013_DATAFIELD)検索
        Try
            '○指定ﾊﾟﾗﾒｰﾀで検索
            'DataBase接続文字
            Dim SQLcon = sm.getConnection
            SQLcon.Open() 'DataBase接続(Open)

            'CAMPCODE検索SQL文
            Dim SQL_Str As String = _
                 " SELECT " _
               & "          FIELDTYPE                                 , " _
               & "          INTLENG                                   , " _
               & "          DECLENG                                   , " _
               & "          MUST                                      , " _
               & "          FVCHECK                                   , " _
               & "          KEYCODE                                     " _
               & " FROM                                                 " _
               & "  (                                            " _
               & "   SELECT                                             " _
               & "            A.FIELDTYPE                             , " _
               & "            A.INTLENG                               , " _
               & "            A.DECLENG                               , " _
               & "            A.MAST               AS MUST            , " _
               & "            A.FVCHECK                               , " _
               & "            B.KEYCODE                               , " _
               & "            ROW_NUMBER() OVER(                        " _
               & "                 PARTITION BY                         " _
               & "                        A.FIELD                     , " _
               & "                        A.MAPID                       " _
               & "                 ORDER BY                             " _
               & "                        CASE A.CAMPCODE               " _
               & "                        WHEN '" & C_DEFAULT_DATAKEY & "' THEN 2         " _
               & "                        ELSE 1 END                    " _
               & "                       ) AS RNK                       " _
               & "   FROM                                               " _
               & "             S0013_DATAFIELD             A            " _
               & "   LEFT JOIN MC001_FIXVALUE              B       ON   " _
               & "            B.CLASS      = A.FIELD                    " _
               & "        and B.KEYCODE    = @P4                        " _
               & "        and B.STYMD     <= @P5                        " _
               & "        and B.ENDYMD    >= @P6                        " _
               & "        and B.DELFLG    <> @P7                        " _
               & "   Where                                              " _
               & "            A.CAMPCODE IN (@P1,'" & C_DEFAULT_DATAKEY & "') " _
               & "        and A.MAPID      = @P2                        " _
               & "        and A.FIELD      = @P3                        " _
               & "        and A.STYMD     <= @P5                        " _
               & "        and A.ENDYMD    >= @P6                        " _
               & "        and A.DELFLG    <> @P7                        " _
               & "  ) MAIN                                              " _
               & " WHERE                                                " _
               & "           RNK = 1                                    "
            Dim SQLcmd As New SqlCommand(SQL_Str, SQLcon)
            Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.NVarChar, 20)
            Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.NVarChar, 50)
            Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.NVarChar, 50)
            Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", System.Data.SqlDbType.NVarChar, 50)
            Dim PARA5 As SqlParameter = SQLcmd.Parameters.Add("@P5", System.Data.SqlDbType.Date)
            Dim PARA6 As SqlParameter = SQLcmd.Parameters.Add("@P6", System.Data.SqlDbType.Date)
            Dim PARA7 As SqlParameter = SQLcmd.Parameters.Add("@P7", System.Data.SqlDbType.NVarChar, 1)
            PARA1.Value = CAMPCODE
            PARA2.Value = MAPID
            PARA3.Value = FIELD
            PARA4.Value = VALUE
            PARA5.Value = Date.Now
            PARA6.Value = Date.Now
            PARA7.Value = C_DELETE_FLG.DELETE
            Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

            '出力パラメータ初期設定
            CHECKREPORT = ""
            ERR = C_MESSAGE_NO.NORMAL
            VALUEOUT = VALUE

            Dim WW_DATE As Date = Date.Now
            Dim WW_TIME As DateTime
            While SQLdr.Read

                '○必須チェック
                If SQLdr("MUST") = CONST_FLAG_YES Then
                    If VALUE = "" Then
                        CHECKREPORT = C_MESSAGE_TEXT.PREREQUISITE_ERROR_TEXT & "(" & VALUE & ")"
                        ERR = C_MESSAGE_NO.PREREQUISITE_ERROR
                        Exit While
                    End If
                End If

                '○項目属性別チェック
                Select Case SQLdr("FIELDTYPE")
                    Case "NUM"

                        If VALUE = "" Then
                            '空欄は、0を設定
                            VALUE = "0"
                            VALUEOUT = "0"
                        End If

                        '有効桁数チェック
                        Dim WW_VALUE As Double = 0
                        Dim WW_int As String = ""
                        Dim WW_dec As String = ""
                        Dim WW_I_VALUE As String = Replace(VALUE, ",", "")

                        '項目属性チェック
                        If Double.TryParse(WW_I_VALUE, WW_VALUE) Then
                        Else
                            CHECKREPORT = C_MESSAGE_TEXT.NUMERIC_ERROR_TEXT & "(" & VALUE & ")"
                            ERR = C_MESSAGE_NO.NUMERIC_VALUE_ERROR
                            VALUEOUT = "0"
                            Exit While
                        End If

                        '桁数チェック準備
                        Try
                            If InStr(WW_I_VALUE, ".") = 0 Then
                                WW_int = WW_I_VALUE
                                WW_dec = ""
                            Else
                                WW_int = Mid(WW_I_VALUE, 1, InStr(WW_I_VALUE, ".") - 1)
                                WW_dec = Mid(WW_I_VALUE, InStr(WW_I_VALUE, ".") + 1, 100)
                            End If
                        Catch ex As Exception
                            CHECKREPORT = C_MESSAGE_TEXT.NUMERIC_ERROR_TEXT & "(" & VALUE & ")"
                            ERR = C_MESSAGE_NO.NUMERIC_VALUE_ERROR
                            VALUEOUT = "0"
                            Exit While
                        End Try

                        '　整数部チェック
                        If SQLdr("INTLENG") = 0 Then            'データフィールドマスタ(S0013_DATAFIELD)　桁数未設定
                        Else
                            Try
                                If WW_int.Length > SQLdr("INTLENG") Then
                                    CHECKREPORT = C_MESSAGE_TEXT.INTEGER_LENGTH_OVER_ERROR_TEXT & "(" & VALUE & ")"
                                    ERR = C_MESSAGE_NO.INTEGER_LENGTH_OVER_ERROR
                                    VALUEOUT = "0"
                                    Exit While
                                End If
                            Catch ex As Exception
                                CHECKREPORT = C_MESSAGE_TEXT.NUMERIC_ERROR_TEXT & "(" & VALUE & ")"
                                ERR = C_MESSAGE_NO.NUMERIC_VALUE_ERROR
                                VALUEOUT = "0"
                                Exit While
                            End Try
                        End If

                        '　小数部チェック
                        If SQLdr("DECLENG") = 0 Then            'データフィールドマスタ(S0013_DATAFIELD)　桁数未設定　
                            If WW_dec.Length > 0 Then
                                CHECKREPORT = C_MESSAGE_TEXT.DECIMAL_LENGTH_OVER_ERROR_TEXT & "(" & VALUE & ")"
                                ERR = C_MESSAGE_NO.DECIMAL_LENGTH_OVER_ERROR
                                VALUEOUT = "0"
                                Exit While
                            End If
                        Else
                            Try
                                If WW_dec.Length > SQLdr("DECLENG") Then
                                    CHECKREPORT = C_MESSAGE_TEXT.DECIMAL_LENGTH_OVER_ERROR_TEXT & "(" & VALUE & ")"
                                    ERR = C_MESSAGE_NO.DECIMAL_LENGTH_OVER_ERROR
                                    VALUEOUT = "0"
                                    Exit While
                                End If
                            Catch ex As Exception
                                CHECKREPORT = C_MESSAGE_TEXT.NUMERIC_ERROR_TEXT & "(" & VALUE & ")"
                                ERR = C_MESSAGE_NO.NUMERIC_VALUE_ERROR
                                VALUEOUT = "0"
                                Exit While
                            End Try
                        End If

                        '有効桁数編集
                        If SQLdr("INTLENG") <> 0 And SQLdr("DECLENG") = 0 Then
                            VALUEOUT = Right("0000000000" & WW_I_VALUE.ToString, SQLdr("INTLENG"))
                        Else
                            VALUEOUT = WW_I_VALUE
                        End If


                    Case "DATE"
                        ' 項目属性チェック
                        If VALUE <> "" Then
                            Try
                                Date.TryParse(VALUE, WW_DATE)
                            Catch ex As Exception
                                CHECKREPORT = C_MESSAGE_TEXT.DATE_FORMAT_ERROR_TEXT & "(" & VALUE & ")"
                                ERR = C_MESSAGE_NO.DATE_FORMAT_ERROR
                                Exit While
                            End Try

                            If WW_DATE < C_DEFAULT_YMD Then
                                CHECKREPORT = C_MESSAGE_TEXT.DATE_FORMAT_ERROR_TEXT & "(" & VALUE & ")"
                                ERR = C_MESSAGE_NO.DATE_FORMAT_ERROR
                                Exit While
                            End If

                            If WW_DATE > C_MAX_YMD Then
                                CHECKREPORT = C_MESSAGE_TEXT.DATE_MAX_OVER_ERROR_TEXT & "(" & VALUE & ")"
                                ERR = C_MESSAGE_NO.DATE_FORMAT_ERROR
                                Exit While
                            End If

                            VALUEOUT = WW_DATE.ToString("yyyy/MM/dd")
                        Else
                            VALUEOUT = ""
                        End If

                    Case "TIME"
                        ' 項目属性チェック
                        If VALUE <> "" Then
                            Try
                                WW_TIME = VALUE
                                VALUEOUT = WW_TIME.ToString("H:mm")
                            Catch ex As Exception
                                CHECKREPORT = C_MESSAGE_TEXT.TIME_FORMAT_ERROR_TEXT & "(" & VALUE & ")"
                                ERR = C_MESSAGE_NO.DATE_FORMAT_ERROR
                                Exit While
                            End Try
                        Else
                            VALUEOUT = ""
                        End If

                    Case "STR"
                        ' 有効桁数チェック
                        If SQLdr("INTLENG") <> 0 Then
                            '桁数判断
                            If VALUE.Length > SQLdr("INTLENG") Then
                                CHECKREPORT = C_MESSAGE_TEXT.STRING_LENGTH_OVER_ERROR_TEXT & "(" & VALUE & ")"
                                ERR = C_MESSAGE_NO.STRING_LENGTH_OVER_ERROR
                                Exit While
                            End If
                        End If

                        VALUEOUT = VALUE

                End Select

                '固定値マスタ存在チェック
                If SQLdr("FVCHECK") = CONST_FLAG_YES Then
                    If IsDBNull(SQLdr("KEYCODE")) Then
                        CHECKREPORT = C_MESSAGE_TEXT.SELECT_INVALID_VALUE_ERROR & "(" & VALUE & ")"
                        ERR = C_MESSAGE_NO.INVALID_SELECTION_DATA
                        Exit While
                    End If
                End If

                Exit While

            End While

            If SQLdr.HasRows = False Then
                Dim CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get

                CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME                 'SUBクラス名
                CS0011LOGWRITE.INFPOSI = "DB:S0013_DATAFIELD Select"             '
                CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                 '
                CS0011LOGWRITE.TEXT = "データフィールドマスタ（S0013_DATAFIELD）に存在しません。"
                CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.NO_DATA_EXISTS_ERROR
                CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力

                ERR = C_MESSAGE_NO.DB_ERROR
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
            Dim CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get

            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME                 'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:S0013_DATAFIELD Select"             '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                  '
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力

            ERR = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try


    End Sub

End Structure
