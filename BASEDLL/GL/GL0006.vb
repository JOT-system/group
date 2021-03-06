﻿Imports System.Data.SqlClient
Imports System.Web.UI.WebControls

''' <summary>
''' 品名情報取得
''' </summary>
''' <remarks></remarks>
Public Class GL0006GoodsList
    Inherits GL0000
    ''' <summary>
    ''' 取得条件
    ''' </summary>
    Public Enum LC_GOODS_TYPE
        ''' <summary>
        ''' 品名コード
        ''' </summary>
        ALL
        ''' <summary>
        ''' OILTYPE
        ''' </summary>
        OILTYPE
        ''' <summary>
        ''' GOODS1
        ''' </summary>
        GOODS1
        ''' <summary>
        ''' GOODS2
        ''' </summary>
        GOODS2
        ''' <summary>
        ''' 品名コード
        ''' </summary>
        GOODS
        ''' <summary>
        ''' OILTYPE 名称（コード）表記
        ''' </summary>
        OILTYPE_MST
        ''' <summary>
        ''' GOODS1 名称（コード）表記
        ''' </summary>
        GOODS1_MST
        ''' <summary>
        ''' GOODS2 名称（コード）表記
        ''' </summary>
        GOODS2_MST
        ''' <summary>
        ''' 品名コード 名称（コード）表記
        ''' </summary>
        GOODS_MST
        ''' <summary>
        ''' 組織による油種 名称（コード）表記
        ''' </summary>
        OILTYPE_IN_ORG
        ''' <summary>
        ''' 組織による品名1 名称（コード）表記
        ''' </summary>
        GOODS1_IN_ORG
        ''' <summary>
        ''' 組織による品名2 名称（コード）表記
        ''' </summary>
        GOODS2_IN_ORG
        ''' <summary>
        ''' 組織による品名コード 名称（コード）表記
        ''' </summary>
        GOODS_IN_ORG
    End Enum

    ''' <summary>
    '''　取得区分
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property TYPE() As LC_GOODS_TYPE
    ''' <summary>
    ''' 会社コード
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property CAMPCODE() As String
    ''' <summary>
    ''' ROLECODE
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property ROLECODE() As String
    ''' <summary>
    ''' 権限フラグ
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property PERMISSION() As String
    ''' <summary>
    ''' 部署用会社コード
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property ORGCAMPCODE() As String
    ''' <summary>
    ''' 部署コード
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property ORGCODE() As String
    ''' <summary>
    ''' 固定値マスタの分類コード
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property FIXCLASSCODE() As String
    ''' <summary>
    ''' 油種
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property OILTYPE() As String
    ''' <summary>
    ''' 品名1
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property PRODUCT1() As String
    ''' <summary>
    ''' 固定値マスタの油種用分類コード
    ''' </summary>
    Protected Const C_OILTYPE_FIX_CLASS_CODE As String = "OILTYPE"
    ''' <summary>
    ''' 固定値マスタの品名１用分類コード
    ''' </summary>
    Protected Const C_GOODS1_FIX_CLASS_CODE As String = "PRODUCT1"
    ''' <summary>
    ''' メソッド名
    ''' </summary>
    ''' <remarks></remarks>
    Protected Const METHOD_NAME As String = "getList"
    ''' <summary>
    ''' 情報の取得
    ''' </summary>
    ''' <remarks></remarks>
    Public Overrides Sub getList()

        '<< エラー説明 >>
        'O_ERR = OK:00000,ERR:00002(環境エラー),ERR:00003(DBerr)
        '●初期処理


        'PARAM 01: TYPE
        If checkParam(METHOD_NAME, TYPE) Then
            Exit Sub
        End If
        'PARAM 02: CAMPCODE
        If checkParam(METHOD_NAME, CAMPCODE) Then
            Exit Sub
        End If
        'PARAM EXTRA01: ORGCODE
        If IsNothing(ORGCODE) Then
            ORGCODE = ""
        End If
        'PARAM EXTRA02: STYMD
        If STYMD < C_DEFAULT_YMD Then
            STYMD = Date.Now
        End If
        'PARAM EXTRA03: ENDYMD
        If ENDYMD < C_DEFAULT_YMD Then
            ENDYMD = Date.Now
        End If
        'PARAM EXTRA04: FIXCLASSCODE
        If IsNothing(FIXCLASSCODE) Then
            Select Case TYPE
                Case LC_GOODS_TYPE.OILTYPE, LC_GOODS_TYPE.OILTYPE_IN_ORG
                    FIXCLASSCODE = C_OILTYPE_FIX_CLASS_CODE
                Case LC_GOODS_TYPE.GOODS1, LC_GOODS_TYPE.GOODS1_IN_ORG, LC_GOODS_TYPE.GOODS1_MST
                    FIXCLASSCODE = C_GOODS1_FIX_CLASS_CODE
            End Select
        End If
        Try
            If IsNothing(LIST) Then
                LIST = New ListBox
            Else
                LIST.Items.Clear()
            End If
        Catch ex As Exception
        End Try
        'DataBase接続文字
        Dim SQLcon = sm.getConnection
        SQLcon.Open() 'DataBase接続(Open)

        Select Case TYPE
            Case LC_GOODS_TYPE.OILTYPE
                getOilTypeList(SQLcon)
            Case LC_GOODS_TYPE.GOODS1
                getGoods1List(SQLcon)
            Case LC_GOODS_TYPE.GOODS2
                getGoods2List(SQLcon)
            Case LC_GOODS_TYPE.GOODS
                getGoodsCodeList(SQLcon)
            Case LC_GOODS_TYPE.OILTYPE_MST
                getOilTypeMSTList(SQLcon)
            Case LC_GOODS_TYPE.GOODS1_MST
                getGoods1MSTList(SQLcon)
            Case LC_GOODS_TYPE.GOODS2_MST
                getGoods2MSTList(SQLcon)
            Case LC_GOODS_TYPE.GOODS_MST
                getGoodsCodeMstList(SQLcon)
            Case LC_GOODS_TYPE.OILTYPE_IN_ORG
                getOilTypeOrgList(SQLcon)
            Case LC_GOODS_TYPE.GOODS1_IN_ORG
                getGoods1OrgList(SQLcon)
            Case LC_GOODS_TYPE.GOODS2_IN_ORG
                getGoods2OrgList(SQLcon)
            Case LC_GOODS_TYPE.GOODS_IN_ORG
                getGoodsCodeOrgList(SQLcon)
            Case Else
                getListAll(SQLcon)
        End Select


        SQLcon.Close() 'DataBase接続(Close)
        SQLcon.Dispose()
        SQLcon = Nothing

    End Sub
    ''' <summary>
    ''' 品名一覧取得
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub getListAll(ByVal SQLcon As SqlConnection)
        '●Leftボックス用品名コード取得
        '検索SQL文
        Dim SQLStr As String = String.Empty

        '○ User権限によりDB(MD001_PRODUCT)検索
        Try
            '検索SQL文
            SQLStr =
                      " SELECT " _
                    & "            rtrim(A.PRODUCTCODE)  as CODE       , " _
                    & "            rtrim(A.NAMES)        as NAMES        " _
                    & " FROM                                             " _
                    & "          MD001_PRODUCT             A             " _
                    & " WHERE                                            " _
                    & "            A.STYMD         <= @P3                " _
                    & "       and  A.ENDYMD        >= @P2                " _
                    & "       and  A.CAMPCODE       = @P1                " _
                    & "       and  A.DELFLG        <> '1'                " _
                    & "GROUP BY A.PRODUCTCODE , A.NAMES                  " _
            '〇ソート条件追加
            Select Case DEFAULT_SORT
                Case C_DEFAULT_SORT.CODE, String.Empty
                    SQLStr = SQLStr & " ORDER BY A.PRODUCTCODE , A.NAMES "
                Case C_DEFAULT_SORT.NAMES
                    SQLStr = SQLStr & " ORDER BY A.NAMES, A.PRODUCTCODE "
                Case C_DEFAULT_SORT.SEQ
                    SQLStr = SQLStr & " ORDER BY A.PRODUCTCODE , A.NAMES "
                Case Else
            End Select

            Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.Date)
                Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.Date)
                PARA1.Value = CAMPCODE
                PARA2.Value = STYMD
                PARA3.Value = ENDYMD
                Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

                '○出力編集
                addListData(SQLdr)

                'Close
                SQLdr.Close() 'Reader(Close)
                SQLdr = Nothing

            End Using
        Catch ex As Exception
            Dim CS0011LOGWRITE As New CS0011LOGWrite            'LogOutput DirString Get
            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME            'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:MD001_PRODUCT Select"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            ERR = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try

        ERR = C_MESSAGE_NO.NORMAL
    End Sub
    ''' <summary>
    ''' 品名コード一覧取得
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub getGoodsCodeList(ByVal SQLcon As SqlConnection)
        '●Leftボックス用品名コード取得
        '検索SQL文
        Dim SQLStr As String = String.Empty
        Dim SQLGrpOrdr As String = String.Empty

        '○ User権限によりDB(MD001_PRODUCT)検索
        Try
            If String.IsNullOrEmpty(ORGCODE) Then
                SQLStr =
                          " SELECT " _
                        & "            rtrim(A.PRODUCTCODE)  as CODE       , " _
                        & "            rtrim(A.NAMES)        as NAMES        " _
                        & " FROM                                             " _
                        & "          MD001_PRODUCT             A             " _
                        & " WHERE                                            " _
                        & "            A.CAMPCODE       = @P1                " _
                        & "       and  A.STYMD         <= @P3                " _
                        & "       and  A.ENDYMD        >= @P2                " _
                        & "       and  A.DELFLG        <> '1'                "
                '〇ソート条件と集合条件追加
                SQLGrpOrdr = "GROUP BY A.PRODUCTCODE , A.NAMES      "

                Select Case DEFAULT_SORT
                    Case C_DEFAULT_SORT.CODE, String.Empty
                        SQLGrpOrdr = SQLGrpOrdr & " ORDER BY A.PRODUCTCODE, A.NAMES "
                    Case C_DEFAULT_SORT.NAMES
                        SQLGrpOrdr = SQLGrpOrdr & " ORDER BY A.NAMES, A.PRODUCTCODE "
                    Case C_DEFAULT_SORT.SEQ
                        SQLGrpOrdr = SQLGrpOrdr & " ORDER BY A.PRODUCTCODE, A.NAMES "
                    Case Else
                End Select
            Else
                '検索SQL文
                SQLStr =
                          " SELECT " _
                        & "            rtrim(A.PRODUCTCODE)  as CODE       , " _
                        & "            rtrim(A.NAMES)        as NAMES      , " _
                        & "            B.SEQ                 as SEQ          " _
                        & " FROM                                             " _
                        & "            MD001_PRODUCT           A             " _
                        & " INNER JOIN MD002_PRODORG           B          ON " _
                        & "            B.PRODUCTCODE    = A.PRODUCTCODE      " _
                        & "       and  B.CAMPCODE       = @P8                " _
                        & "       and  B.UORG           = @P5                " _
                        & "       and  B.STYMD         <= @P3                " _
                        & "       and  B.ENDYMD        >= @P2                " _
                        & "       and  B.DELFLG        <> '1'                " _
                        & " WHERE                                            " _
                        & "            A.CAMPCODE       = @P1                " _
                        & "       and  A.STYMD         <= @P3                " _
                        & "       and  A.ENDYMD        >= @P2                " _
                        & "       and  A.DELFLG        <> '1'                "
                '〇ソート条件と集合条件追加
                SQLGrpOrdr = "GROUP BY A.PRODUCTCODE , A.NAMES , B.SEQ     "

                Select Case DEFAULT_SORT
                    Case C_DEFAULT_SORT.CODE
                        SQLGrpOrdr = SQLGrpOrdr & " ORDER BY A.PRODUCTCODE, A.NAMES , B.SEQ"
                    Case C_DEFAULT_SORT.NAMES
                        SQLGrpOrdr = SQLGrpOrdr & " ORDER BY A.NAMES, A.PRODUCTCODE , B.SEQ"
                    Case C_DEFAULT_SORT.SEQ, String.Empty
                        SQLGrpOrdr = SQLGrpOrdr & " ORDER BY B.SEQ , A.PRODUCTCODE, A.NAMES "
                    Case Else
                End Select
            End If
            If Not String.IsNullOrEmpty(OILTYPE) Then
                SQLStr &= "       and  A.OILTYPE        = @P10    "
            End If
            If Not String.IsNullOrEmpty(PRODUCT1) Then
                SQLStr &= "       and  A.PRODUCT1       = @P11    "
            End If

            SQLStr = SQLStr & SQLGrpOrdr

            Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.Date)
                Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.Date)
                Dim PARA5 As SqlParameter = SQLcmd.Parameters.Add("@P5", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA8 As SqlParameter = SQLcmd.Parameters.Add("@P8", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA10 As SqlParameter = SQLcmd.Parameters.Add("@P10", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA11 As SqlParameter = SQLcmd.Parameters.Add("@P11", System.Data.SqlDbType.NVarChar, 20)
                PARA1.Value = CAMPCODE
                PARA2.Value = STYMD
                PARA3.Value = ENDYMD
                PARA5.Value = ORGCODE
                PARA8.Value = ORGCAMPCODE
                PARA10.Value = OILTYPE
                PARA11.Value = PRODUCT1
                Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

                '○出力編集
                addListData(SQLdr)

                'Close
                SQLdr.Close() 'Reader(Close)
                SQLdr = Nothing

            End Using
        Catch ex As Exception
            Dim CS0011LOGWRITE As New CS0011LOGWrite            'LogOutput DirString Get
            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME            'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:MD001_PRODUCT Select"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            ERR = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try

        ERR = C_MESSAGE_NO.NORMAL
    End Sub

    ''' <summary>
    ''' 品名一覧取得
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub getGoodsCodeMstList(ByVal SQLcon As SqlConnection)
        '●Leftボックス用品名コード取得
        '検索SQL文
        Dim SQLStr As String = String.Empty
        Dim SQLGrpOrdr As String = String.Empty

        '○ User権限によりDB(MD001_PRODUCT)検索
        Try
            If String.IsNullOrEmpty(ORGCODE) Then
                SQLStr =
                          " SELECT " _
                        & "            rtrim(A.PRODUCTCODE)  as CODE       , " _
                        & "            rtrim(A.NAMES)        as NAMES        " _
                        & " FROM                                             " _
                        & "          MD001_PRODUCT             A             " _
                        & " WHERE                                            " _
                        & "            A.CAMPCODE       = @P1                " _
                        & "       and  A.STYMD         <= @P3                " _
                        & "       and  A.ENDYMD        >= @P2                " _
                        & "       and  A.DELFLG        <> '1'                "
                '〇ソート条件と集合条件追加
                SQLGrpOrdr = "GROUP BY A.PRODUCTCODE , A.NAMES               "

                Select Case DEFAULT_SORT
                    Case C_DEFAULT_SORT.CODE, String.Empty
                        SQLGrpOrdr = SQLGrpOrdr & " ORDER BY A.PRODUCTCODE, A.NAMES   "
                    Case C_DEFAULT_SORT.NAMES
                        SQLGrpOrdr = SQLGrpOrdr & " ORDER BY A.NAMES, A.PRODUCTCODE   "
                    Case C_DEFAULT_SORT.SEQ
                        SQLGrpOrdr = SQLGrpOrdr & " ORDER BY A.PRODUCTCODE, A.NAMES   "
                    Case Else
                End Select
            Else
                '検索SQL文
                SQLStr =
                          " SELECT " _
                        & "            rtrim(A.PRODUCTCODE)  as CODE       , " _
                        & "            rtrim(A.NAMES)        as NAMES      , " _
                        & "            B.SEQ                 as SEQ          " _
                        & " FROM                                             " _
                        & "            MD001_PRODUCT           A             " _
                        & " INNER JOIN MD002_PRODORG           B          ON " _
                        & "            B.PRODUCTCODE    = A.PRODUCTCODE      " _
                        & "       and  B.CAMPCODE       = @P8                " _
                        & "       and  B.UORG           = @P5                " _
                        & "       and  B.STYMD         <= @P3                " _
                        & "       and  B.ENDYMD        >= @P2                " _
                        & "       and  B.DELFLG        <> '1'                " _
                        & " WHERE                                            " _
                        & "            A.CAMPCODE       = @P1                " _
                        & "       and  A.STYMD         <= @P3                " _
                        & "       and  A.ENDYMD        >= @P2                " _
                        & "       and  A.DELFLG        <> '1'                "
                '〇ソート条件と集合条件追加
                SQLGrpOrdr = "GROUP BY A.PRODUCTCODE , A.NAMES , B.SEQ     "

                Select Case DEFAULT_SORT
                    Case C_DEFAULT_SORT.CODE
                        SQLGrpOrdr = SQLGrpOrdr & " ORDER BY A.PRODUCTCODE, A.NAMES , B.SEQ"
                    Case C_DEFAULT_SORT.NAMES
                        SQLGrpOrdr = SQLGrpOrdr & " ORDER BY A.NAMES, A.PRODUCTCODE , B.SEQ"
                    Case C_DEFAULT_SORT.SEQ, String.Empty
                        SQLGrpOrdr = SQLGrpOrdr & " ORDER BY B.SEQ , A.PRODUCTCODE, A.NAMES "
                    Case Else
                End Select
            End If
            If Not String.IsNullOrEmpty(OILTYPE) Then
                SQLStr &= "       and  A.OILTYPE        = @P10    "
            End If
            If Not String.IsNullOrEmpty(PRODUCT1) Then
                SQLStr &= "       and  A.PRODUCT1       = @P11    "
            End If
            SQLStr = SQLStr & SQLGrpOrdr

            Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.Date)
                Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.Date)
                Dim PARA5 As SqlParameter = SQLcmd.Parameters.Add("@P5", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA8 As SqlParameter = SQLcmd.Parameters.Add("@P8", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA10 As SqlParameter = SQLcmd.Parameters.Add("@P10", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA11 As SqlParameter = SQLcmd.Parameters.Add("@P11", System.Data.SqlDbType.NVarChar, 20)
                PARA1.Value = CAMPCODE
                PARA2.Value = STYMD
                PARA3.Value = ENDYMD
                PARA5.Value = ORGCODE
                PARA8.Value = ORGCAMPCODE
                PARA10.Value = OILTYPE
                PARA11.Value = PRODUCT1
                Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

                '○出力編集
                addListData(SQLdr)

                'Close
                SQLdr.Close() 'Reader(Close)
                SQLdr = Nothing

            End Using
        Catch ex As Exception
            Dim CS0011LOGWRITE As New CS0011LOGWrite            'LogOutput DirString Get
            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME            'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:MD001_PRODUCT Select"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            ERR = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try

        ERR = C_MESSAGE_NO.NORMAL
    End Sub

    ''' <summary>
    ''' 品名一覧取得
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub getGoodsCodeOrgList(ByVal SQLcon As SqlConnection)
        '●Leftボックス用品名コード取得
        '検索SQL文
        Dim SQLStr As String = ""

        '○ User権限によりDB(MD001_PRODUCT)検索
        Try
            '検索SQL文
            SQLStr =
                      " SELECT " _
                    & "            rtrim(A.PRODUCTCODE)  as CODE       , " _
                    & "            rtrim(A.NAMES)        as NAMES      , " _
                    & "            B.SEQ                 as SEQ          " _
                    & " FROM       MD001_PRODUCT         A               " _
                    & " INNER JOIN MD002_PRODORG         B            ON " _
                    & "            B.PRODUCTCODE    = A.PRODUCTCODE      " _
                    & "       and  B.CAMPCODE       = @P8                " _
                    & "       and  B.UORG           = @P5                " _
                    & "       and  B.STYMD         <= @P3                " _
                    & "       and  B.ENDYMD        >= @P2                " _
                    & "       and  B.DELFLG        <> '1'                " _
                    & " INNER JOIN S0006_ROLE　          C            ON " _
                    & "            C.CAMPCODE       = B.CAMPCODE         " _
                    & "       and  C.CODE           = B.UORG             " _
                    & "       and  C.OBJECT         = @P6                " _
                    & "       and  C.ROLE           = @P4                " _
                    & "       and  C.PERMITCODE    >= @P7                " _
                    & "       and  C.STYMD         <= @P3                " _
                    & "       and  C.ENDYMD        >= @P2                " _
                    & "       and  C.DELFLG        <> '1'                " _
                    & " WHERE                                            " _
                    & "            A.CAMPCODE       = @P1                " _
                    & "       and  A.STYMD         <= @P3                " _
                    & "       and  A.ENDYMD        >= @P2                " _
                    & "       and  A.DELFLG        <> '1'                "
            If Not String.IsNullOrEmpty(OILTYPE) Then
                SQLStr &= "       and  A.OILTYPE         = @P10    "
            End If
            If Not String.IsNullOrEmpty(PRODUCT1) Then
                SQLStr &= "       and  A.PRODUCT1        = @P11    "
            End If
            SQLStr &= "GROUP BY A.PRODUCTCODE, A.NAMES , B.SEQ     "

            '〇ソート条件追加
            Select Case DEFAULT_SORT
                Case C_DEFAULT_SORT.CODE
                    SQLStr = SQLStr & " ORDER BY A.PRODUCTCODE, A.NAMES, B.SEQ "
                Case C_DEFAULT_SORT.NAMES
                    SQLStr = SQLStr & " ORDER BY A.NAMES, A.PRODUCTCODE, B.SEQ "
                Case C_DEFAULT_SORT.SEQ, String.Empty
                    SQLStr = SQLStr & " ORDER BY B.SEQ, A.PRODUCTCODE, A.NAMES "
                Case Else
            End Select

            Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.Date)
                Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.Date)
                Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA5 As SqlParameter = SQLcmd.Parameters.Add("@P5", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA6 As SqlParameter = SQLcmd.Parameters.Add("@P6", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA7 As SqlParameter = SQLcmd.Parameters.Add("@P7", System.Data.SqlDbType.Int)
                Dim PARA8 As SqlParameter = SQLcmd.Parameters.Add("@P8", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA10 As SqlParameter = SQLcmd.Parameters.Add("@P10", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA11 As SqlParameter = SQLcmd.Parameters.Add("@P11", System.Data.SqlDbType.NVarChar, 20)
                PARA1.Value = CAMPCODE
                PARA2.Value = STYMD
                PARA3.Value = ENDYMD
                PARA4.Value = ROLECODE
                PARA5.Value = ORGCODE
                PARA6.Value = C_ROLE_VARIANT.USER_ORG
                PARA7.Value = PERMISSION
                PARA8.Value = ORGCAMPCODE
                PARA10.Value = OILTYPE
                PARA11.Value = PRODUCT1
                Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

                '○出力編集
                addListData(SQLdr)

                'Close
                SQLdr.Close() 'Reader(Close)
                SQLdr = Nothing

            End Using
        Catch ex As Exception
            Dim CS0011LOGWRITE As New CS0011LOGWrite            'LogOutput DirString Get
            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME            'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:MC001_FIXVALUE Select"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            ERR = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try

        ERR = C_MESSAGE_NO.NORMAL
    End Sub
    ''' <summary>
    ''' 油種一覧取得
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub getOilTypeList(ByVal SQLcon As SqlConnection)
        '●Leftボックス用油種取得
        '検索SQL文
        Dim SQLStr As String = ""

        '○ User権限によりDB(MC001_FIXVALUE)検索
        Try
            '検索SQL文
            SQLStr =
                      " SELECT " _
                    & "       rtrim(A.KEYCODE)     as CODE    , " _
                    & "       rtrim(A.VALUE1)      as NAMES     " _
                    & " FROM       MC001_FIXVALUE    A          " _
                    & " WHERE                                   " _
                    & "            A.CAMPCODE    = @P1          " _
                    & "       and  A.CLASS       = @P8          " _
                    & "       and  A.STYMD      <= @P3          " _
                    & "       and  A.ENDYMD     >= @P2          " _
                    & "       and  A.DELFLG     <> '1'          " _
                    & " GROUP BY   A.KEYCODE , A.VALUE1         "
            '〇ソート条件追加
            Select Case DEFAULT_SORT
                Case C_DEFAULT_SORT.CODE, String.Empty
                    SQLStr = SQLStr & " ORDER BY A.KEYCODE , A.VALUE1 "
                Case C_DEFAULT_SORT.NAMES
                    SQLStr = SQLStr & " ORDER BY A.VALUE1 , A.KEYCODE "
                Case C_DEFAULT_SORT.SEQ
                    SQLStr = SQLStr & " ORDER BY A.KEYCODE , A.VALUE1 "
                Case Else
            End Select
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.Date)
                Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.Date)
                Dim PARA8 As SqlParameter = SQLcmd.Parameters.Add("@P8", System.Data.SqlDbType.NVarChar, 20)
                PARA1.Value = CAMPCODE
                PARA2.Value = STYMD
                PARA3.Value = ENDYMD
                PARA8.Value = FIXCLASSCODE
                Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

                '○出力編集
                addListData(SQLdr)

                'Close
                SQLdr.Close() 'Reader(Close)
                SQLdr = Nothing

            End Using
        Catch ex As Exception
            Dim CS0011LOGWRITE As New CS0011LOGWrite            'LogOutput DirString Get
            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME            'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:MC001_FIXVALUE Select"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            ERR = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try

        ERR = C_MESSAGE_NO.NORMAL
    End Sub
    ''' <summary>
    ''' 油種一覧取得
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub getOilTypeMSTList(ByVal SQLcon As SqlConnection)
        '●Leftボックス用油種取得
        '検索SQL文
        Dim SQLStr As String = ""

        '○ User権限によりDB(MC001_FIXVALUE)検索
        Try
            '検索SQL文
            SQLStr =
                      " SELECT " _
                    & "       rtrim(A.KEYCODE)     as CODE    , " _
                    & "       rtrim(A.VALUE1)      as NAMES     " _
                    & " FROM       MC001_FIXVALUE    A          " _
                    & " WHERE                                   " _
                    & "            A.CAMPCODE    = @P1          " _
                    & "       and  A.CLASS       = @P8          " _
                    & "       and  A.STYMD      <= @P3          " _
                    & "       and  A.ENDYMD     >= @P2          " _
                    & "       and  A.DELFLG     <> '1'          " _
                    & " GROUP BY   A.KEYCODE , A.VALUE1         "
            '〇ソート条件追加
            Select Case DEFAULT_SORT
                Case C_DEFAULT_SORT.CODE, String.Empty
                    SQLStr = SQLStr & " ORDER BY A.KEYCODE , A.VALUE1 "
                Case C_DEFAULT_SORT.NAMES
                    SQLStr = SQLStr & " ORDER BY A.VALUE1 , A.KEYCODE "
                Case C_DEFAULT_SORT.SEQ
                    SQLStr = SQLStr & " ORDER BY A.KEYCODE , A.VALUE1 "
                Case Else
            End Select
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.Date)
                Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.Date)
                Dim PARA8 As SqlParameter = SQLcmd.Parameters.Add("@P8", System.Data.SqlDbType.NVarChar, 20)
                PARA1.Value = CAMPCODE
                PARA2.Value = STYMD
                PARA3.Value = ENDYMD
                PARA8.Value = FIXCLASSCODE
                Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

                '○出力編集
                addListData(SQLdr)

                'Close
                SQLdr.Close() 'Reader(Close)
                SQLdr = Nothing

            End Using
        Catch ex As Exception
            Dim CS0011LOGWRITE As New CS0011LOGWrite            'LogOutput DirString Get
            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME            'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:MC001_FIXVALUE Select"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            ERR = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try

        ERR = C_MESSAGE_NO.NORMAL
    End Sub

    ''' <summary>
    ''' 品名１一覧取得
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub getGoods1List(ByVal SQLcon As SqlConnection)
        '●Leftボックス用品名１取得
        '検索SQL文
        Dim SQLStr As String = ""

        '○ User権限によりDB(MD001_PRODUCT)検索
        Try
            '検索SQL文
            SQLStr =
                      " SELECT " _
                    & "            rtrim(A.OILTYPE)    as OILTYPECODE  , " _
                    & "            rtrim(A.PRODUCT1)   as CODE         , " _
                    & "            rtrim(D.VALUE1)     as NAMES        , " _
                    & "            B.SEQ               as SEQ            " _
                    & " FROM       MD001_PRODUCT         A               " _
                    & " INNER JOIN MD002_PRODORG         B            ON " _
                    & "            B.PRODUCTCODE    = A.PRODUCTCODE      " _
                    & "       and  B.CAMPCODE       = @P8                " _
                    & "       and  B.UORG           = @P5                " _
                    & "       and  B.STYMD         <= @P3                " _
                    & "       and  B.ENDYMD        >= @P2                " _
                    & "       and  B.DELFLG        <> '1'                " _
                    & " INNER JOIN MC001_FIXVALUE        D               " _
                    & "            D.CAMPCODE       = A.CAMPCODE         " _
                    & "       and  D.KEYCODE        = A.PRODUCT1         " _
                    & "       and  D.CLASS          = @P9                " _
                    & "       and  D.STYMD         <= @P3                " _
                    & "       and  D.ENDYMD        >= @P2                " _
                    & "       and  D.DELFLG        <> '1'                " _
                    & " WHERE                                            " _
                    & "            A.CAMPCODE       = @P1                " _
                    & "       and  A.STYMD         <= @P3                " _
                    & "       and  A.ENDYMD        >= @P2                " _
                    & "       and  A.DELFLG        <> '1'                "
            If Not String.IsNullOrEmpty(OILTYPE) Then
                SQLStr &= "       and  A.OILTYPE        = @P10           "
            End If
            SQLStr &= "GROUP BY A.OILTYPE , A.PRODUCT1 , D.VALUE1 , B.SEQ  "
            '〇ソート条件追加
            Select Case DEFAULT_SORT
                Case C_DEFAULT_SORT.CODE
                    SQLStr = SQLStr & " ORDER BY A.OILTYPE , A.PRODUCT1 , D.VALUE1 , B.SEQ "
                Case C_DEFAULT_SORT.NAMES
                    SQLStr = SQLStr & " ORDER BY A.OILTYPE , D.VALUE1 , A.PRODUCT1 , B.SEQ "
                Case C_DEFAULT_SORT.SEQ, String.Empty
                    SQLStr = SQLStr & " ORDER BY A.OILTYPE , B.SEQ , D.VALUE1 , A.PRODUCT1  "
                Case Else
            End Select
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.Date)
                Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.Date)
                Dim PARA5 As SqlParameter = SQLcmd.Parameters.Add("@P5", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA8 As SqlParameter = SQLcmd.Parameters.Add("@P8", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA9 As SqlParameter = SQLcmd.Parameters.Add("@P9", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA10 As SqlParameter = SQLcmd.Parameters.Add("@P10", System.Data.SqlDbType.NVarChar, 20)
                PARA1.Value = CAMPCODE
                PARA2.Value = STYMD
                PARA3.Value = ENDYMD
                PARA5.Value = ORGCODE
                PARA8.Value = ORGCAMPCODE
                PARA9.Value = FIXCLASSCODE
                PARA10.Value = OILTYPE
                Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

                '○出力編集
                addListData(SQLdr)

                'Close
                SQLdr.Close() 'Reader(Close)
                SQLdr = Nothing

            End Using
        Catch ex As Exception
            Dim CS0011LOGWRITE As New CS0011LOGWrite            'LogOutput DirString Get
            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME            'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:MD001_PRODUCT Select"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            ERR = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try

        ERR = C_MESSAGE_NO.NORMAL
    End Sub
    ''' <summary>
    ''' 品名１マスタ一覧取得
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub getGoods1MSTList(ByVal SQLcon As SqlConnection)
        '●Leftボックス用品名１のマスタ情報取得
        '検索SQL文
        Dim SQLStr As String = ""

        '○ DB(MC001_FIXVALUE)検索
        Try
            '検索SQL文
            SQLStr =
                      " SELECT " _
                    & "            rtrim(A.KEYCODE)    as CODE         , " _
                    & "            rtrim(A.VALUE1)     as NAMES          " _
                    & " FROM       MC001_FIXVALUE        A               " _
                    & " WHERE                                            " _
                    & "            A.CAMPCODE       = @P1                " _
                    & "       and  A.CLASS          = @P9                " _
                    & "       and  A.STYMD         <= @P3                " _
                    & "       and  A.ENDYMD        >= @P2                " _
                    & "       and  A.DELFLG        <> '1'                "
            If Not String.IsNullOrEmpty(OILTYPE) Then
                SQLStr &= "   and  A.VALUE2         = @P10               "
            End If
            SQLStr &= "GROUP BY A.KEYCODE , A.VALUE1                     "
            '〇ソート条件追加
            Select Case DEFAULT_SORT
                Case C_DEFAULT_SORT.CODE, String.Empty
                    SQLStr = SQLStr & " ORDER BY A.KEYCODE , A.VALUE1 "
                Case C_DEFAULT_SORT.NAMES
                    SQLStr = SQLStr & " ORDER BY A.VALUE1 , A.KEYCODE "
                Case C_DEFAULT_SORT.SEQ
                    SQLStr = SQLStr & " ORDER BY A.KEYCODE , A.VALUE1 "
                Case Else
            End Select
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.Date)
                Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.Date)
                Dim PARA9 As SqlParameter = SQLcmd.Parameters.Add("@P9", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA10 As SqlParameter = SQLcmd.Parameters.Add("@P10", System.Data.SqlDbType.NVarChar, 20)
                PARA1.Value = CAMPCODE
                PARA2.Value = STYMD
                PARA3.Value = ENDYMD
                PARA9.Value = FIXCLASSCODE
                PARA10.Value = OILTYPE
                Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

                '○出力編集
                addListData(SQLdr)

                'Close
                SQLdr.Close() 'Reader(Close)
                SQLdr = Nothing

            End Using
        Catch ex As Exception
            Dim CS0011LOGWRITE As New CS0011LOGWrite            'LogOutput DirString Get
            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME            'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:MC001_FIXVALUE Select"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            ERR = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try

        ERR = C_MESSAGE_NO.NORMAL
    End Sub
    ''' <summary>
    ''' 品名2一覧取得
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub getGoods2MSTList(ByVal SQLcon As SqlConnection)
        '●Leftボックス用品名2取得
        '検索SQL文
        Dim SQLStr As String = ""

        '○ User権限によりDB(MD001_PRODUCT)検索
        Try
            '検索SQL文
            SQLStr =
                      " SELECT                                           " _
                    & "            rtrim(A.PRODUCT2)   as CODE         , " _
                    & "            rtrim(A.NAMES)      as NAMES          " _
                    & " FROM       MD001_PRODUCT         A               " _
                    & " WHERE                                            " _
                    & "            A.CAMPCODE       = @P1                " _
                    & "       and  A.STYMD         <= @P3                " _
                    & "       and  A.ENDYMD        >= @P2                " _
                    & "       and  A.DELFLG        <> '1'                "
            If Not String.IsNullOrEmpty(OILTYPE) Then
                SQLStr &= "       and  A.OILTYPE        = @P10           "
            End If
            If Not String.IsNullOrEmpty(PRODUCT1) Then
                SQLStr &= "       and  A.PRODUCT1       = @P11           "
            End If
            SQLStr &= " GROUP BY A.PRODUCT2, A.NAMES      "

            '〇ソート条件追加
            Select Case DEFAULT_SORT
                Case C_DEFAULT_SORT.CODE, String.Empty
                    SQLStr = SQLStr & " ORDER BY A.PRODUCT2 , A.NAMES "
                Case C_DEFAULT_SORT.NAMES
                    SQLStr = SQLStr & " ORDER BY A.NAMES , A.PRODUCT2 "
                Case C_DEFAULT_SORT.SEQ
                    SQLStr = SQLStr & " ORDER BY A.PRODUCT2 , A.NAMES "
                Case Else
            End Select

            Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.Date)
                Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.Date)
                Dim PARA10 As SqlParameter = SQLcmd.Parameters.Add("@P10", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA11 As SqlParameter = SQLcmd.Parameters.Add("@P11", System.Data.SqlDbType.NVarChar, 20)
                PARA1.Value = CAMPCODE
                PARA2.Value = STYMD
                PARA3.Value = ENDYMD
                PARA10.Value = OILTYPE
                PARA11.Value = PRODUCT1
                Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

                '○出力編集
                addListData(SQLdr)

                'Close
                SQLdr.Close() 'Reader(Close)
                SQLdr = Nothing

            End Using
        Catch ex As Exception
            Dim CS0011LOGWRITE As New CS0011LOGWrite            'LogOutput DirString Get
            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME            'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:MD001_PRODUCT Select"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            ERR = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try

        ERR = C_MESSAGE_NO.NORMAL
    End Sub
    ''' <summary>
    ''' 使用可能な品名2一覧取得
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub getGoods2List(ByVal SQLcon As SqlConnection)
        '●Leftボックス用品名2取得
        '検索SQL文
        Dim SQLStr As String = ""

        '○ User権限によりDB(MD001_PRODUCT)検索
        Try
            '検索SQL文
            SQLStr = _
                      " SELECT                                           " _
                    & "            rtrim(A.OILTYPE)    as OILTYPECODE  , " _
                    & "            rtrim(A.PRODUCT1)   as PRODUCT1CODE , " _
                    & "            rtrim(A.PRODUCT2)   as CODE         , " _
                    & "            rtrim(A.NAMES)      as NAMES        , " _
                    & "            B.SEQ               as SEQ            " _
                    & " FROM       MD001_PRODUCT         A               " _
                    & " INNER JOIN MD002_PRODORG         B            ON " _
                    & "            B.PRODUCTCODE    = A.PRODUCTCODE      " _
                    & "       and  B.CAMPCODE       = A.CAMPCODE         " _
                    & "       and  B.UORG           = @P4                " _
                    & "       and  B.STYMD         <= @P3                " _
                    & "       and  B.ENDYMD        >= @P2                " _
                    & "       and  B.DELFLG        <> '1'                " _
                    & " WHERE                                            " _
                    & "            A.STYMD         <= @P3                " _
                    & "       and  A.ENDYMD        >= @P2                " _
                    & "       and  A.DELFLG        <> '1'                " _
                    & "       and  A.CAMPCODE       = @P7                "
            If Not String.IsNullOrEmpty(OILTYPE) Then
                SQLStr &= "       and  A.OILTYPE        = @P10           "
            End If
            If Not String.IsNullOrEmpty(PRODUCT1) Then
                SQLStr &= "       and  A.PRODUCT1       = @P11           "
            End If
            SQLStr &= "GROUP BY A.OILTYPE , A.PRODUCT1 , A.PRODUCT2 , A.NAMES  , B.SEQ    "

            '〇ソート条件追加
            Select Case DEFAULT_SORT
                Case C_DEFAULT_SORT.CODE
                    SQLStr = SQLStr & " ORDER BY A.OILTYPE , A.PRODUCT1 , A.PRODUCT2 , A.NAMES , B.SEQ "
                Case C_DEFAULT_SORT.NAMES
                    SQLStr = SQLStr & " ORDER BY A.OILTYPE , A.PRODUCT1 , A.NAMES , A.PRODUCT2 , B.SEQ "
                Case C_DEFAULT_SORT.SEQ, String.Empty
                    SQLStr = SQLStr & " ORDER BY A.OILTYPE , A.PRODUCT1 , B.SEQ , A.PRODUCT2 , A.NAMES "
                Case Else
            End Select

            Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.Date)
                Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.Date)
                Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA5 As SqlParameter = SQLcmd.Parameters.Add("@P5", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA6 As SqlParameter = SQLcmd.Parameters.Add("@P6", System.Data.SqlDbType.Int)
                Dim PARA7 As SqlParameter = SQLcmd.Parameters.Add("@P7", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA8 As SqlParameter = SQLcmd.Parameters.Add("@P8", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA9 As SqlParameter = SQLcmd.Parameters.Add("@P9", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA10 As SqlParameter = SQLcmd.Parameters.Add("@P10", System.Data.SqlDbType.NVarChar, 20)
                PARA1.Value = ROLECODE
                PARA2.Value = STYMD
                PARA3.Value = ENDYMD
                PARA4.Value = ORGCODE
                PARA5.Value = C_ROLE_VARIANT.USER_ORG
                PARA6.Value = PERMISSION
                PARA7.Value = CAMPCODE
                PARA8.Value = FIXCLASSCODE
                PARA9.Value = OILTYPE
                PARA10.Value = PRODUCT1
                Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

                '○出力編集
                addListData(SQLdr)

                'Close
                SQLdr.Close() 'Reader(Close)
                SQLdr = Nothing

            End Using
        Catch ex As Exception
            Dim CS0011LOGWRITE As New CS0011LOGWrite            'LogOutput DirString Get
            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME            'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:MD001_PRODUCT Select"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            ERR = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try

        ERR = C_MESSAGE_NO.NORMAL
    End Sub

    ''' <summary>
    ''' 組織で使用可能な油種一覧取得
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub getOilTypeOrgList(ByVal SQLcon As SqlConnection)
        '●Leftボックス用油種取得
        '検索SQL文
        Dim SQLStr As String = ""

        '○ User権限によりDB(MC001_FIXVALUE)検索
        Try
            '検索SQL文
            SQLStr =
                      " SELECT " _
                    & "       rtrim(N.KEYCODE)     as CODE    ,  " _
                    & "       rtrim(N.VALUE1)      as NAMES      " _
                    & " FROM       MC001_FIXVALUE    N           " _
                    & " INNER JOIN MD001_PRODUCT     A       ON  " _
                    & "            A.CAMPCODE    = N.CAMPCODE    " _
                    & "       and  A.OILTYPE     = N.KEYCODE     " _
                    & "       and  A.STYMD      <= @P3           " _
                    & "       and  A.ENDYMD     >= @P2           " _
                    & "       and  A.DELFLG     <> '1'           " _
                    & " INNER JOIN MD002_PRODORG     B       ON  " _
                    & "            B.PRODUCTCODE = A.PRODUCTCODE " _
                    & "       and  B.CAMPCODE    = @P8           " _
                    & "       and  B.UORG        = @P5           " _
                    & "       and  B.STYMD      <= @P3           " _
                    & "       and  B.ENDYMD     >= @P2           " _
                    & "       and  B.DELFLG     <> '1'           " _
                    & " INNER JOIN S0006_ROLE        C       ON  " _
                    & "            C.CAMPCODE    = B.CAMPCODE    " _
                    & "       and  C.CODE        = B.UORG        " _
                    & "       and  C.OBJECT      = @P6           " _
                    & "       and  C.ROLE        = @P4           " _
                    & "       and  C.PERMITCODE >= @P7           " _
                    & "       and  C.STYMD      <= @P3           " _
                    & "       and  C.ENDYMD     >= @P2           " _
                    & " WHERE                                    " _
                    & "            N.CAMPCODE    = @P1           " _
                    & "       and  N.CLASS       = @P9           " _
                    & "       and  N.STYMD      <= @P3           " _
                    & "       and  N.ENDYMD     >= @P2           " _
                    & "       and  N.DELFLG     <> '1'           " _
                    & " GROUP BY   N.KEYCODE , N.VALUE1          "

            '〇ソート条件追加
            Select Case DEFAULT_SORT
                Case C_DEFAULT_SORT.CODE
                    SQLStr = SQLStr & " ORDER BY N.KEYCODE , N.VALUE1         "
                Case C_DEFAULT_SORT.NAMES
                    SQLStr = SQLStr & " ORDER BY N.VALUE1 , N.KEYCODE         "
                Case C_DEFAULT_SORT.SEQ, String.Empty
                    SQLStr = SQLStr & " ORDER BY N.KEYCODE , N.VALUE1         "
                Case Else
            End Select

            Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.Date)
                Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.Date)
                Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA5 As SqlParameter = SQLcmd.Parameters.Add("@P5", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA6 As SqlParameter = SQLcmd.Parameters.Add("@P6", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA7 As SqlParameter = SQLcmd.Parameters.Add("@P7", System.Data.SqlDbType.Int)
                Dim PARA8 As SqlParameter = SQLcmd.Parameters.Add("@P8", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA9 As SqlParameter = SQLcmd.Parameters.Add("@P9", System.Data.SqlDbType.NVarChar, 20)
                PARA1.Value = CAMPCODE
                PARA2.Value = STYMD
                PARA3.Value = ENDYMD
                PARA4.Value = ROLECODE
                PARA5.Value = ORGCODE
                PARA6.Value = C_ROLE_VARIANT.USER_ORG
                PARA7.Value = PERMISSION
                PARA8.Value = ORGCAMPCODE
                PARA9.Value = FIXCLASSCODE
                Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

                '○出力編集
                addListData(SQLdr)

                'Close
                SQLdr.Close() 'Reader(Close)
                SQLdr = Nothing

            End Using
        Catch ex As Exception
            Dim CS0011LOGWRITE As New CS0011LOGWrite            'LogOutput DirString Get
            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME            'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:MC001_FIXVALUE Select"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            ERR = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try

        ERR = C_MESSAGE_NO.NORMAL
    End Sub
    ''' <summary>
    ''' 組織で使用可能な品名１一覧取得
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub getGoods1OrgList(ByVal SQLcon As SqlConnection)
        '●Leftボックス用油種取得
        '検索SQL文
        Dim SQLStr As String = ""

        '○ User権限によりDB(MD001_PRODUCT)検索
        Try
            '検索SQL文
            SQLStr =
                      " SELECT " _
                    & "            rtrim(A.OILTYPE)    as OILTYPECODE  , " _
                    & "            rtrim(A.PRODUCT1)   as CODE         , " _
                    & "            rtrim(D.VALUE1)     as NAMES          " _
                    & " FROM       MD001_PRODUCT         A               " _
                    & " INNER JOIN MD002_PRODORG         B            ON " _
                    & "            B.PRODUCTCODE    = A.PRODUCTCODE      " _
                    & "       and  B.CAMPCODE       = @P8                " _
                    & "       and  B.UORG           = @P5                " _
                    & "       and  B.STYMD         <= @P3                " _
                    & "       and  B.ENDYMD        >= @P2                " _
                    & "       and  B.DELFLG        <> '1'                " _
                    & " INNER JOIN S0006_ROLE　          C            ON " _
                    & "            C.CAMPCODE       = B.CAMPCODE         " _
                    & "       and  C.CODE           = B.UORG             " _
                    & "       and  C.OBJECT         = @P6                " _
                    & "       and  C.ROLE           = @P4                " _
                    & "       and  C.PERMITCODE    >= @P7                " _
                    & "       and  C.STYMD         <= @P3                " _
                    & "       and  C.ENDYMD        >= @P2                " _
                    & "       and  C.DELFLG        <> '1'                " _
                    & " INNER JOIN MC001_FIXVALUE        D            ON " _
                    & "            D.KEYCODE        = A.PRODUCT1         " _
                    & "       and  D.CLASS          = @P9                " _
                    & "       and  D.CAMPCODE       = A.CAMPCODE         " _
                    & "       and  D.STYMD         <= @P3                " _
                    & "       and  D.ENDYMD        >= @P2                " _
                    & "       and  D.DELFLG        <> '1'                " _
                    & " WHERE                                            " _
                    & "            A.STYMD         <= @P3                " _
                    & "       and  A.ENDYMD        >= @P2                " _
                    & "       and  A.DELFLG        <> '1'                " _
                    & "       and  A.CAMPCODE       = @P1                "
            If Not String.IsNullOrEmpty(OILTYPE) Then
                SQLStr &= "       and  A.OILTYPE        = @P10           "
            End If
            SQLStr &= "GROUP BY A.OILTYPE , A.PRODUCT1 , D.VALUE1 "

            '〇ソート条件追加
            Select Case DEFAULT_SORT
                Case C_DEFAULT_SORT.CODE
                    SQLStr = SQLStr & " ORDER BY A.OILTYPE , A.PRODUCT1 , D.VALUE1 "
                Case C_DEFAULT_SORT.NAMES
                    SQLStr = SQLStr & " ORDER BY A.OILTYPE , D.VALUE1 , A.PRODUCT1 "
                Case C_DEFAULT_SORT.SEQ, String.Empty
                    SQLStr = SQLStr & " ORDER BY A.OILTYPE , A.PRODUCT1 , D.VALUE1 "
                Case Else
            End Select

            Dim SQLcmd As New SqlCommand(SQLStr, SQLcon)
            Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.NVarChar, 20)
            Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.Date)
            Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.Date)
            Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", System.Data.SqlDbType.NVarChar, 20)
            Dim PARA5 As SqlParameter = SQLcmd.Parameters.Add("@P5", System.Data.SqlDbType.NVarChar, 20)
            Dim PARA6 As SqlParameter = SQLcmd.Parameters.Add("@P6", System.Data.SqlDbType.NVarChar, 20)
            Dim PARA7 As SqlParameter = SQLcmd.Parameters.Add("@P7", System.Data.SqlDbType.Int)
            Dim PARA8 As SqlParameter = SQLcmd.Parameters.Add("@P8", System.Data.SqlDbType.NVarChar, 20)
            Dim PARA9 As SqlParameter = SQLcmd.Parameters.Add("@P9", System.Data.SqlDbType.NVarChar, 20)
            Dim PARA10 As SqlParameter = SQLcmd.Parameters.Add("@P10", System.Data.SqlDbType.NVarChar, 20)
            PARA1.Value = CAMPCODE
            PARA2.Value = STYMD
            PARA3.Value = ENDYMD
            PARA4.Value = ROLECODE
            PARA5.Value = ORGCODE
            PARA6.Value = C_ROLE_VARIANT.USER_ORG
            PARA7.Value = PERMISSION
            PARA8.Value = ORGCAMPCODE
            PARA9.Value = FIXCLASSCODE
            PARA10.Value = OILTYPE
            Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

            '○出力編集
            addListData(SQLdr)

            'Close
            SQLdr.Close() 'Reader(Close)
            SQLdr = Nothing

            SQLcmd.Dispose()
            SQLcmd = Nothing
        Catch ex As Exception
            Dim CS0011LOGWRITE As New CS0011LOGWrite            'LogOutput DirString Get
            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME            'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:MD001_PRODUCT Select"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            ERR = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try

        ERR = C_MESSAGE_NO.NORMAL
    End Sub
    ''' <summary>
    ''' 組織で使用可能な品名2一覧取得
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub getGoods2OrgList(ByVal SQLcon As SqlConnection)
        '●Leftボックス用品名2取得
        '検索SQL文
        Dim SQLStr As String = ""

        '○ User権限によりDB(MD001_PRODUCT)検索
        Try
            '検索SQL文
            SQLStr =
                      " SELECT                                           " _
                    & "            rtrim(A.OILTYPE)    as OILTYPECODE  , " _
                    & "            rtrim(A.PRODUCT1)   as PRODUCT1CODE , " _
                    & "            rtrim(A.PRODUCT2)   as CODE         , " _
                    & "            rtrim(A.NAMES)      as NAMES        , " _
                    & "            B.SEQ               as SEQ            " _
                    & " FROM       MD001_PRODUCT         A               " _
                    & " INNER JOIN MD002_PRODORG         B            ON " _
                    & "            B.PRODUCTCODE    = A.PRODUCTCODE      " _
                    & "       and  B.CAMPCODE       = @P8                " _
                    & "       and  B.UORG           = @P5                " _
                    & "       and  B.STYMD         <= @P3                " _
                    & "       and  B.ENDYMD        >= @P2                " _
                    & "       and  B.DELFLG        <> '1'                " _
                    & " INNER JOIN S0006_ROLE　          C            ON " _
                    & "            C.CAMPCODE       = B.CAMPCODE         " _
                    & "       and  C.CODE           = B.UORG             " _
                    & "       and  C.OBJECT         = @P6                " _
                    & "       and  C.ROLE           = @P4                " _
                    & "       and  C.PERMITCODE    >= @P7                " _
                    & "       and  C.STYMD         <= @P3                " _
                    & "       and  C.ENDYMD        >= @P2                " _
                    & "       and  C.DELFLG        <> '1'                " _
                    & " WHERE                                            " _
                    & "            A.STYMD         <= @P3                " _
                    & "       and  A.ENDYMD        >= @P2                " _
                    & "       and  A.DELFLG        <> '1'                " _
                    & "       and  A.CAMPCODE       = @P1                "
            If Not String.IsNullOrEmpty(OILTYPE) Then
                SQLStr &= "       and  A.OILTYPE        = @P10           "
            End If
            If Not String.IsNullOrEmpty(PRODUCT1) Then
                SQLStr &= "       and  A.PRODUCT1       = @P11           "
            End If
            SQLStr &= " GROUP BY A.OILTYPE , A.PRODUCT1 , A.PRODUCT2, A.NAMES , B.SEQ     "
            '〇ソート条件追加
            Select Case DEFAULT_SORT
                Case C_DEFAULT_SORT.CODE
                    SQLStr = SQLStr & " ORDER BY A.OILTYPE , A.PRODUCT1 , A.PRODUCT2 , A.NAMES , B.SEQ "
                Case C_DEFAULT_SORT.NAMES
                    SQLStr = SQLStr & " ORDER BY A.OILTYPE , A.PRODUCT1 , A.NAMES , A.PRODUCT2 , B.SEQ "
                Case C_DEFAULT_SORT.SEQ, String.Empty
                    SQLStr = SQLStr & " ORDER BY A.OILTYPE , A.PRODUCT1 , B.SEQ , A.PRODUCT2 , A.NAMES "
                Case Else
            End Select

            Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.Date)
                Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.Date)
                Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA5 As SqlParameter = SQLcmd.Parameters.Add("@P5", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA6 As SqlParameter = SQLcmd.Parameters.Add("@P6", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA7 As SqlParameter = SQLcmd.Parameters.Add("@P7", System.Data.SqlDbType.Int)
                Dim PARA8 As SqlParameter = SQLcmd.Parameters.Add("@P8", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA10 As SqlParameter = SQLcmd.Parameters.Add("@P10", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA11 As SqlParameter = SQLcmd.Parameters.Add("@P11", System.Data.SqlDbType.NVarChar, 20)
                PARA1.Value = CAMPCODE
                PARA2.Value = STYMD
                PARA3.Value = ENDYMD
                PARA4.Value = ROLECODE
                PARA5.Value = ORGCODE
                PARA6.Value = C_ROLE_VARIANT.USER_ORG
                PARA7.Value = PERMISSION
                PARA8.Value = ORGCAMPCODE
                PARA10.Value = OILTYPE
                PARA11.Value = PRODUCT1
                Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

                '○出力編集
                addListData(SQLdr)

                'Close
                SQLdr.Close() 'Reader(Close)
                SQLdr = Nothing
            End Using
        Catch ex As Exception
            Dim CS0011LOGWRITE As New CS0011LOGWrite            'LogOutput DirString Get
            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME            'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:MD001_PRODUCT Select"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            ERR = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try

        ERR = C_MESSAGE_NO.NORMAL
    End Sub

End Class

