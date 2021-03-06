﻿Imports System.Web
Imports System.Data.SqlClient


''' <summary>
''' 承認管理
''' </summary>
Public Structure CS0048Apploval

    ''' <summary>
    ''' シーケンスオブジェクトＩＤ
    ''' </summary>
    ''' <returns></returns>
    Public Property I_SEQOBJID As String

    ''' <summary>
    ''' 会社コード
    ''' </summary>
    ''' <returns></returns>
    Public Property I_CAMPCODE As String

    ''' <summary>
    ''' システムコード
    ''' </summary>
    ''' <returns></returns>
    Public Property I_SYSCODE As String

    ''' <summary>
    ''' 分類
    ''' </summary>
    ''' <returns></returns>
    Public Property I_CLASS As String

    ''' <summary>
    ''' マスタキー
    ''' </summary>
    ''' <returns></returns>
    Public Property I_KEYCODE As String

    ''' <summary>
    ''' 申請ID
    ''' </summary>
    ''' <returns></returns>
    Public Property I_APPLYID As String

    ''' <summary>
    ''' 画面ID
    ''' </summary>
    ''' <returns></returns>
    Public Property I_MAPID As String

    ''' <summary>
    ''' イベントコード
    ''' </summary>
    ''' <returns></returns>
    Public Property I_EVENTCODE As String

    ''' <summary>
    ''' サブコード
    ''' </summary>
    ''' <returns></returns>
    Public Property I_SUBCODE As String

    ''' <summary>
    ''' ステップ
    ''' </summary>
    ''' <returns></returns>
    Public Property I_STEP As String

    ''' <summary>
    ''' ユーザID
    ''' </summary>
    ''' <returns></returns>
    Public Property I_USERID As String

    ''' <summary>
    ''' 従業員コード
    ''' </summary>
    ''' <returns></returns>
    Public Property I_STAFFCODE As String

    ''' <summary>
    ''' 予備１
    ''' </summary>
    ''' <returns></returns>
    Public Property I_VALUE_C1 As String

    ''' <summary>
    ''' 予備２
    ''' </summary>
    ''' <returns></returns>
    Public Property I_VALUE_C2 As String

    ''' <summary>
    ''' 予備３
    ''' </summary>
    ''' <returns></returns>
    Public Property I_VALUE_C3 As String

    ''' <summary>
    ''' 予備４
    ''' </summary>
    ''' <returns></returns>
    Public Property I_VALUE_C4 As String

    ''' <summary>
    ''' 予備５
    ''' </summary>
    ''' <returns></returns>
    Public Property I_VALUE_C5 As String
    ''' <summary>
    ''' 更新ユーザー
    ''' </summary>
    ''' <returns></returns>
    Public Property I_UPDUSER As String
    ''' <summary>
    ''' 更新端末
    ''' </summary>
    ''' <returns></returns>
    Public Property I_UPDTERMID As String
    ''' <summary>
    ''' エラーコード(00000=正常)
    ''' </summary>
    ''' <returns></returns>
    Public Property O_ERR As String

    ''' <summary>
    ''' 申請ID
    ''' </summary>
    ''' <returns></returns>
    Public Property O_APPLYID As String

    ''' <summary>
    ''' 最終承認ステップ
    ''' </summary>
    ''' <returns></returns>
    Public Property O_LASTSTEP As String

    ''' <summary>
    ''' <para>申請状態</para>
    ''' <para>検索キー：ID("APPLYID")
    '''           申請日("APPLYDATE")
    '''           申請者("APPLICANTID")
    '''           ステータス("STATUS")
    '''           承認日("APPROVEDATE")
    '''           承認者("APPROVERID")</para>
    ''' </summary>
    ''' <returns></returns>
    Public Property O_APPLYSTATE As Dictionary(Of String, String)

    Const TBL_FIXVALUE As String = "MC001_FIXVALUE"
    Const TBL_APPROVAL As String = "S0022_APPROVAL"
    Const TBL_APPROVALHIST As String = "T0009_APPROVALHIST"


    ''' <summary>
    ''' <para>入力依頼登録</para>
    ''' <para>入力プロパティ(CAMPCODE,APPLYID,STEP)</para>
    ''' <para>出力プロパティ(ERR(処理結果コード):正常終了("00000")、他者による更新済み("10012")、以外エラー)</para>
    ''' </summary>
    Public Sub CS0048setInputRequest()
        Dim CS0011LOGWRITE As New BASEDLL.CS0011LOGWrite                'LogOutput DirString Get

        Try

            If IsNothing(I_CAMPCODE) And I_CAMPCODE = "" Then
                O_ERR = C_MESSAGE_NO.DLL_IF_ERROR
                CS0011LOGWRITE.INFSUBCLASS = "CS0048setInputRequest"        'SUBクラス名
                CS0011LOGWRITE.INFPOSI = "InParamチェック"                  '
                CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                 '
                CS0011LOGWRITE.TEXT = "DLLインターフェイスエラー(I_CAMPCODE)"
                CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
                CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
                Exit Sub
            End If

            If IsNothing(I_APPLYID) And I_APPLYID = "" Then
                O_ERR = C_MESSAGE_NO.DLL_IF_ERROR
                CS0011LOGWRITE.INFSUBCLASS = "CS0048setInputRequest"        'SUBクラス名
                CS0011LOGWRITE.INFPOSI = "InParamチェック"                  '
                CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                 '
                CS0011LOGWRITE.TEXT = "DLLインターフェイスエラー(I_APPLYID)"
                CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
                CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
                Exit Sub
            End If

            If IsNothing(I_STEP) And I_STEP = "" Then
                O_ERR = C_MESSAGE_NO.DLL_IF_ERROR
                CS0011LOGWRITE.INFSUBCLASS = "CS0048setInputRequest"        'SUBクラス名
                CS0011LOGWRITE.INFPOSI = "InParamチェック"                  '
                CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                 '
                CS0011LOGWRITE.TEXT = "DLLインターフェイスエラー(I_STEP)"
                CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
                CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
                Exit Sub
            End If

            If IsNothing(I_STAFFCODE) And I_STAFFCODE = "" Then
                O_ERR = C_MESSAGE_NO.DLL_IF_ERROR
                CS0011LOGWRITE.INFSUBCLASS = "CS0048setInputRequest"        'SUBクラス名
                CS0011LOGWRITE.INFPOSI = "InParamチェック"                  '
                CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                 '
                CS0011LOGWRITE.TEXT = "DLLインターフェイスエラー(I_STAFFCODE)"
                CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
                CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
                Exit Sub
            End If

            'SQL文の作成
            Dim sqlStat As New System.Text.StringBuilder
            sqlStat.AppendFormat("UPDATE {0}", TBL_APPROVALHIST).AppendLine()
            sqlStat.AppendLine("  SET ")
            sqlStat.AppendLine("   APPROVEDATE = getdate(), ")
            sqlStat.AppendLine("   APPROVERID = @APPROVERID, ")
            sqlStat.AppendLine("   STATUS = @STATUS2, ")
            sqlStat.AppendLine("   UPDYMD = getdate(), ")
            sqlStat.AppendLine("   UPDUSER = @UPDUSER, ")
            sqlStat.AppendLine("   UPDTERMID = @UPDTERMID, ")
            sqlStat.AppendLine("   RECEIVEYMD = @RECEIVEYMD ")
            sqlStat.AppendLine("  WHERE  CAMPCODE    = @CAMPCODE ")
            sqlStat.AppendLine("   and    APPLYID     = @APPLYID ")
            sqlStat.AppendLine("   and    STEP        = @STEP ")
            sqlStat.AppendLine("   and    DELFLG      = @DELFLG ")
            sqlStat.AppendLine("   and    STATUS      = @STATUS ")

            Using sqlConn As New SqlConnection(Convert.ToString(HttpContext.Current.Session("DBcon")))
                Dim sqlRet As Integer
                Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlConn)

                    sqlConn.Open()
                    Dim P_CAMPCODE As SqlParameter = sqlCmd.Parameters.Add("@CAMPCODE", System.Data.SqlDbType.VarChar, 20)
                    Dim P_APPLYID As SqlParameter = sqlCmd.Parameters.Add("@APPLYID", System.Data.SqlDbType.VarChar, 30)
                    Dim P_STEP As SqlParameter = sqlCmd.Parameters.Add("@STEP", System.Data.SqlDbType.VarChar, 20)
                    Dim P_DELFLG As SqlParameter = sqlCmd.Parameters.Add("@DELFLG", System.Data.SqlDbType.VarChar, 1)
                    Dim P_STATUS As SqlParameter = sqlCmd.Parameters.Add("@STATUS", System.Data.SqlDbType.VarChar, 20)
                    Dim P_APPROVERID As SqlParameter = sqlCmd.Parameters.Add("@APPROVERID", System.Data.SqlDbType.VarChar, 20)
                    Dim P_UPDUSER As SqlParameter = sqlCmd.Parameters.Add("@UPDUSER", System.Data.SqlDbType.VarChar, 20)
                    Dim P_STATUS2 As SqlParameter = sqlCmd.Parameters.Add("@STATUS2", System.Data.SqlDbType.VarChar, 20)
                    Dim P_UPDTERMID As SqlParameter = sqlCmd.Parameters.Add("@UPDTERMID", System.Data.SqlDbType.VarChar, 30)
                    Dim P_RECEIVEYMD As SqlParameter = sqlCmd.Parameters.Add("@RECEIVEYMD", System.Data.SqlDbType.DateTime)
                    P_CAMPCODE.Value = Me.I_CAMPCODE
                    P_APPLYID.Value = Me.I_APPLYID
                    P_STEP.Value = Me.I_STEP
                    P_DELFLG.Value = "0"
                    P_STATUS.Value = "02"
                    P_APPROVERID.Value = I_STAFFCODE
                    P_UPDUSER.Value = I_UPDUSER
                    P_STATUS2.Value = "00"
                    P_UPDTERMID.Value = I_UPDTERMID
                    P_RECEIVEYMD.Value = C_DEFAULT_YMD

                    sqlRet = sqlCmd.ExecuteNonQuery()

                End Using

                If sqlRet = 1 Then
                    O_ERR = C_MESSAGE_NO.NORMAL
                Else
                    '更新件数が１でない場合、すでに他者による更新済み
                    O_ERR = "99999"
                End If

            End Using

        Catch ex As Exception
            CS0011LOGWRITE.INFSUBCLASS = "CS0048setInputRequest"        'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:T0009_APPROVALHIST Update"     '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                 '
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            O_ERR = C_MESSAGE_NO.DB_ERROR
            Exit Sub

        End Try

    End Sub

    ''' <summary>
    ''' <para>申請登録</para>
    ''' <para>入力プロパティ(CAMPCODE,APPLYID,MAPID,EVENTCODE,SUBCODE)</para>
    ''' <para>出力プロパティ(ERR(処理結果コード):正常終了("00000")、以外エラー)</para>
    ''' </summary>
    Public Sub CS0048setApply()

        Dim CS0011LOGWRITE As New BASEDLL.CS0011LOGWrite                'LogOutput DirString Get
        Dim retValue As String = ""

        Try

            If IsNothing(I_CAMPCODE) And I_CAMPCODE = "" Then
                O_ERR = C_MESSAGE_NO.DLL_IF_ERROR
                CS0011LOGWRITE.INFSUBCLASS = "CS0048setApply"               'SUBクラス名
                CS0011LOGWRITE.INFPOSI = "InParamチェック"                  '
                CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                 '
                CS0011LOGWRITE.TEXT = "DLLインターフェイスエラー(I_CAMPCODE)"
                CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
                CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
                Exit Sub
            End If

            If IsNothing(I_APPLYID) And I_APPLYID = "" Then
                O_ERR = C_MESSAGE_NO.DLL_IF_ERROR
                CS0011LOGWRITE.INFSUBCLASS = "CS0048setApply"               'SUBクラス名
                CS0011LOGWRITE.INFPOSI = "InParamチェック"                  '
                CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                 '
                CS0011LOGWRITE.TEXT = "DLLインターフェイスエラー(I_APPLYID)"
                CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
                CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
                Exit Sub
            End If

            If IsNothing(I_MAPID) And I_MAPID = "" Then
                O_ERR = C_MESSAGE_NO.DLL_IF_ERROR
                CS0011LOGWRITE.INFSUBCLASS = "CS0048setApply"               'SUBクラス名
                CS0011LOGWRITE.INFPOSI = "InParamチェック"                  '
                CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                 '
                CS0011LOGWRITE.TEXT = "DLLインターフェイスエラー(I_MAPID)"
                CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
                CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
                Exit Sub
            End If

            If IsNothing(I_EVENTCODE) And I_EVENTCODE = "" Then
                O_ERR = C_MESSAGE_NO.DLL_IF_ERROR
                CS0011LOGWRITE.INFSUBCLASS = "CS0048setApply"               'SUBクラス名
                CS0011LOGWRITE.INFPOSI = "InParamチェック"                  '
                CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                 '
                CS0011LOGWRITE.TEXT = "DLLインターフェイスエラー(I_EVENTCODE)"
                CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
                CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
                Exit Sub
            End If

            If IsNothing(I_SUBCODE) And I_SUBCODE = "" Then
                O_ERR = C_MESSAGE_NO.DLL_IF_ERROR
                CS0011LOGWRITE.INFSUBCLASS = "CS0048setApply"               'SUBクラス名
                CS0011LOGWRITE.INFPOSI = "InParamチェック"                  '
                CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                 '
                CS0011LOGWRITE.TEXT = "DLLインターフェイスエラー(I_SUBCODE)"
                CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
                CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
                Exit Sub
            End If

            If IsNothing(I_STAFFCODE) And I_STAFFCODE = "" Then
                O_ERR = C_MESSAGE_NO.DLL_IF_ERROR
                CS0011LOGWRITE.INFSUBCLASS = "CS0048setApply"               'SUBクラス名
                CS0011LOGWRITE.INFPOSI = "InParamチェック"                  '
                CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                 '
                CS0011LOGWRITE.TEXT = "DLLインターフェイスエラー(I_STAFFCODE)"
                CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
                CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
                Exit Sub
            End If

            'SQL文の作成
            Dim sqlStat As New System.Text.StringBuilder
            sqlStat.AppendFormat("INSERT INTO {0}", TBL_APPROVALHIST).AppendLine()
            sqlStat.AppendLine("  ( ")
            sqlStat.AppendLine("   CAMPCODE, ")
            sqlStat.AppendLine("   APPLYID, ")
            sqlStat.AppendLine("   STEP, ")
            sqlStat.AppendLine("   MAPID, ")
            sqlStat.AppendLine("   EVENTCODE, ")
            sqlStat.AppendLine("   SUBCODE, ")
            sqlStat.AppendLine("   APPLYDATE, ")
            sqlStat.AppendLine("   APPLICANTID, ")
            sqlStat.AppendLine("   APPROVEDATE, ")
            sqlStat.AppendLine("   APPROVERID, ")
            sqlStat.AppendLine("   STATUS, ")
            sqlStat.AppendLine("   APPROVEDTEXT, ")
            sqlStat.AppendLine("   VALUE_C1, ")
            sqlStat.AppendLine("   VALUE_C2, ")
            sqlStat.AppendLine("   VALUE_C3, ")
            sqlStat.AppendLine("   VALUE_C4, ")
            sqlStat.AppendLine("   VALUE_C5, ")
            sqlStat.AppendLine("   REMARKS, ")
            sqlStat.AppendLine("   DELFLG, ")
            sqlStat.AppendLine("   INITYMD, ")
            sqlStat.AppendLine("   UPDYMD, ")
            sqlStat.AppendLine("   UPDUSER, ")
            sqlStat.AppendLine("   UPDTERMID, ")
            sqlStat.AppendLine("   RECEIVEYMD ")
            sqlStat.AppendLine("  ) ")
            sqlStat.AppendLine("  SELECT ")
            sqlStat.AppendLine("   CAMPCODE as CAMPCODE, ")
            sqlStat.AppendLine("   @APPLYID as APPLYID, ")
            sqlStat.AppendLine("   STEP as STEP, ")
            sqlStat.AppendLine("   MAPID as MAPID, ")
            sqlStat.AppendLine("   EVENTCODE as EVENTCODE, ")
            sqlStat.AppendLine("   SUBCODE as SUBCODE, ")
            sqlStat.AppendLine("   getdate() as APPLYDATE, ")
            sqlStat.AppendLine("   @APPLICANTID as APPLICANTID, ")
            sqlStat.AppendLine("   NULL as APPROVEDATE, ")
            sqlStat.AppendLine("   '' as APPROVERID, ")
            sqlStat.AppendLine("   @STATUS as STATUS, ")
            sqlStat.AppendLine("   '' as APPROVEDTEXT, ")
            sqlStat.AppendLine("   @VALUE_C1 as VALUE_C1, ")
            sqlStat.AppendLine("   @VALUE_C2 as VALUE_C2, ")
            sqlStat.AppendLine("   @VALUE_C3 as VALUE_C3, ")
            sqlStat.AppendLine("   @VALUE_C4 as VALUE_C4, ")
            sqlStat.AppendLine("   @VALUE_C5 as VALUE_C5, ")
            sqlStat.AppendLine("   '' as REMARKS, ")
            sqlStat.AppendLine("   @DELFLG as DELFLG, ")
            sqlStat.AppendLine("   getdate() as INITYMD, ")
            sqlStat.AppendLine("   getdate() as UPDYMD, ")
            sqlStat.AppendLine("   @UPDUSER as UPDUSER, ")
            sqlStat.AppendLine("   @UPDTERMID as UPDTERMID, ")
            sqlStat.AppendLine("   @RECEIVEYMD as RECEIVEYMD ")
            sqlStat.AppendFormat(" FROM {0}", TBL_APPROVAL).AppendLine()
            sqlStat.AppendLine("   WHERE  CAMPCODE    = @CAMPCODE ")
            sqlStat.AppendLine("   and    MAPID       = @MAPID ")
            sqlStat.AppendLine("   and    EVENTCODE   = @EVENTCODE ")
            sqlStat.AppendLine("   and    SUBCODE     = @SUBCODE ")
            sqlStat.AppendLine("   and    STYMD       <= @YMD ")
            sqlStat.AppendLine("   and    ENDYMD      >= @YMD ")
            sqlStat.AppendLine("   and    DELFLG      = @DELFLG ")
            sqlStat.AppendLine("GROUP BY CAMPCODE, MAPID, EVENTCODE, SUBCODE, STEP")
            sqlStat.AppendLine("ORDER BY STEP")

            Using sqlConn As New SqlConnection(Convert.ToString(HttpContext.Current.Session("DBcon")))
                Dim sqlRet As Integer
                Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlConn)
                    sqlConn.Open()
                    Dim P_CAMPCODE As SqlParameter = sqlCmd.Parameters.Add("@CAMPCODE", System.Data.SqlDbType.VarChar, 20)
                    Dim P_MAPID As SqlParameter = sqlCmd.Parameters.Add("@MAPID", System.Data.SqlDbType.VarChar, 20)
                    Dim P_EVENTCODE As SqlParameter = sqlCmd.Parameters.Add("@EVENTCODE", System.Data.SqlDbType.VarChar, 20)
                    Dim P_SUBCODE As SqlParameter = sqlCmd.Parameters.Add("@SUBCODE", System.Data.SqlDbType.VarChar, 20)
                    Dim P_YMD As SqlParameter = sqlCmd.Parameters.Add("@YMD", System.Data.SqlDbType.Date)
                    Dim P_DELFLG As SqlParameter = sqlCmd.Parameters.Add("@DELFLG", System.Data.SqlDbType.VarChar, 1)
                    Dim P_APPLYID As SqlParameter = sqlCmd.Parameters.Add("@APPLYID", System.Data.SqlDbType.VarChar, 30)
                    Dim P_STATUS As SqlParameter = sqlCmd.Parameters.Add("@STATUS", System.Data.SqlDbType.VarChar, 20)
                    Dim P_APPLICANTID As SqlParameter = sqlCmd.Parameters.Add("@APPLICANTID", System.Data.SqlDbType.VarChar, 20)
                    Dim P_VALUE_C1 As SqlParameter = sqlCmd.Parameters.Add("@VALUE_C1", System.Data.SqlDbType.VarChar, 200)
                    Dim P_VALUE_C2 As SqlParameter = sqlCmd.Parameters.Add("@VALUE_C2", System.Data.SqlDbType.VarChar, 200)
                    Dim P_VALUE_C3 As SqlParameter = sqlCmd.Parameters.Add("@VALUE_C3", System.Data.SqlDbType.VarChar, 200)
                    Dim P_VALUE_C4 As SqlParameter = sqlCmd.Parameters.Add("@VALUE_C4", System.Data.SqlDbType.VarChar, 200)
                    Dim P_VALUE_C5 As SqlParameter = sqlCmd.Parameters.Add("@VALUE_C5", System.Data.SqlDbType.VarChar, 200)
                    Dim P_UPDUSER As SqlParameter = sqlCmd.Parameters.Add("@UPDUSER", System.Data.SqlDbType.VarChar, 20)
                    Dim P_UPDTERMID As SqlParameter = sqlCmd.Parameters.Add("@UPDTERMID", System.Data.SqlDbType.VarChar, 30)
                    Dim P_RECEIVEYMD As SqlParameter = sqlCmd.Parameters.Add("@RECEIVEYMD", System.Data.SqlDbType.DateTime)
                    P_CAMPCODE.Value = Me.I_CAMPCODE
                    P_MAPID.Value = Me.I_MAPID
                    P_EVENTCODE.Value = Me.I_EVENTCODE
                    P_SUBCODE.Value = Me.I_SUBCODE
                    P_YMD.Value = Date.Now
                    P_DELFLG.Value = "0"
                    P_APPLYID.Value = Me.I_APPLYID
                    P_APPLICANTID.Value = I_STAFFCODE
                    P_STATUS.Value = "02"
                    If IsNothing(I_VALUE_C1) And I_VALUE_C1 = "" Then
                        P_VALUE_C1.Value = ""
                    Else
                        P_VALUE_C1.Value = Me.I_VALUE_C1
                    End If
                    If IsNothing(I_VALUE_C2) And I_VALUE_C2 = "" Then
                        P_VALUE_C2.Value = ""
                    Else
                        P_VALUE_C2.Value = Me.I_VALUE_C2
                    End If
                    If IsNothing(I_VALUE_C3) And I_VALUE_C3 = "" Then
                        P_VALUE_C3.Value = ""
                    Else
                        P_VALUE_C3.Value = Me.I_VALUE_C3
                    End If
                    If IsNothing(I_VALUE_C4) And I_VALUE_C4 = "" Then
                        P_VALUE_C4.Value = ""
                    Else
                        P_VALUE_C4.Value = Me.I_VALUE_C4
                    End If
                    If IsNothing(I_VALUE_C5) And I_VALUE_C5 = "" Then
                        P_VALUE_C5.Value = ""
                    Else
                        P_VALUE_C5.Value = Me.I_VALUE_C5
                    End If
                    P_UPDUSER.Value = I_UPDUSER
                    P_UPDTERMID.Value = I_UPDTERMID
                    P_RECEIVEYMD.Value = C_DEFAULT_YMD

                    sqlRet = sqlCmd.ExecuteNonQuery()
                End Using

                If sqlRet >= 1 Then
                    ' 申請登録あり
                    O_ERR = C_MESSAGE_NO.NORMAL

                    '最終承認ステップ取得
                    'SQL文の作成
                    Dim sqlStat1 As New System.Text.StringBuilder
                    sqlStat1.AppendFormat("SELECT MAX(STEP) AS LASTSTEP FROM {0}", TBL_APPROVALHIST).AppendLine()
                    sqlStat1.AppendLine("  WHERE  CAMPCODE    = @CAMPCODE ")
                    sqlStat1.AppendLine("   and    APPLYID     = @APPLYID ")
                    sqlStat1.AppendLine("   and    DELFLG      = @DELFLG ")
                    sqlStat1.AppendLine("  GROUP BY CAMPCODE, APPLYID ")
                    Using sqlCmd As New SqlCommand(sqlStat1.ToString, sqlConn)

                        Dim P_CAMPCODE As SqlParameter = sqlCmd.Parameters.Add("@CAMPCODE", System.Data.SqlDbType.VarChar, 20)
                        Dim P_APPLYID As SqlParameter = sqlCmd.Parameters.Add("@APPLYID", System.Data.SqlDbType.VarChar, 30)
                        Dim P_DELFLG As SqlParameter = sqlCmd.Parameters.Add("@DELFLG", System.Data.SqlDbType.VarChar, 1)
                        P_CAMPCODE.Value = Me.I_CAMPCODE
                        P_APPLYID.Value = Me.I_APPLYID
                        P_DELFLG.Value = "0"

                        Using sqlDr As SqlDataReader = sqlCmd.ExecuteReader()
                            While sqlDr.Read
                                retValue = Convert.ToString(sqlDr("LASTSTEP"))
                                Exit While
                            End While
                        End Using
                    End Using

                    '第１ステップ自動承認
                    'SQL文の作成
                    Dim sqlStat2 As New System.Text.StringBuilder
                    sqlStat2.AppendFormat("UPDATE {0}", TBL_APPROVALHIST).AppendLine()
                    sqlStat2.AppendLine("  SET ")
                    sqlStat2.AppendLine("   APPROVEDATE = getdate(), ")
                    sqlStat2.AppendLine("   APPROVERID = @APPROVERID, ")
                    sqlStat2.AppendLine("   STATUS = @STATUS, ")
                    sqlStat2.AppendLine("   UPDYMD = getdate(), ")
                    sqlStat2.AppendLine("   UPDUSER = @UPDUSER, ")
                    sqlStat2.AppendLine("   UPDTERMID = @UPDTERMID, ")
                    sqlStat2.AppendLine("   RECEIVEYMD = @RECEIVEYMD ")
                    sqlStat2.AppendFormat("FROM {0} as M, {1} as H", TBL_APPROVAL, TBL_APPROVALHIST).AppendLine()
                    sqlStat2.AppendLine("  WHERE   H.CAMPCODE    = @CAMPCODE ")
                    sqlStat2.AppendLine("   and    H.APPLYID     = @APPLYID ")
                    sqlStat2.AppendLine("   and    H.STEP        = @STEP ")
                    sqlStat2.AppendLine("   and    H.CAMPCODE    = M.CAMPCODE ")
                    sqlStat2.AppendLine("   and    H.MAPID       = M.MAPID ")
                    sqlStat2.AppendLine("   and    H.EVENTCODE   = M.EVENTCODE ")
                    sqlStat2.AppendLine("   and    H.SUBCODE     = M.SUBCODE ")
                    sqlStat2.AppendLine("   and    H.STEP        = M.STEP ")
                    sqlStat2.AppendLine("   and    M.STYMD       <= @YMD ")
                    sqlStat2.AppendLine("   and    M.ENDYMD      >= @YMD ")
                    sqlStat2.AppendLine("   and    M.DELFLG      = @DELFLG ")
                    sqlStat2.AppendLine("   and    M.APPROVALTYPE = @APPROVALTYPE ")
                    Using sqlCmd As New SqlCommand(sqlStat2.ToString, sqlConn)

                        Dim P_CAMPCODE As SqlParameter = sqlCmd.Parameters.Add("@CAMPCODE", System.Data.SqlDbType.VarChar, 20)
                        Dim P_APPLYID As SqlParameter = sqlCmd.Parameters.Add("@APPLYID", System.Data.SqlDbType.VarChar, 30)
                        Dim P_STEP As SqlParameter = sqlCmd.Parameters.Add("@STEP", System.Data.SqlDbType.VarChar, 20)
                        Dim P_YMD As SqlParameter = sqlCmd.Parameters.Add("@YMD", System.Data.SqlDbType.Date)
                        Dim P_DELFLG As SqlParameter = sqlCmd.Parameters.Add("@DELFLG", System.Data.SqlDbType.VarChar, 1)
                        Dim P_APPROVALTYPE As SqlParameter = sqlCmd.Parameters.Add("@APPROVALTYPE", System.Data.SqlDbType.VarChar, 20)
                        Dim P_APPROVERID As SqlParameter = sqlCmd.Parameters.Add("@APPROVERID", System.Data.SqlDbType.VarChar, 20)
                        Dim P_STATUS As SqlParameter = sqlCmd.Parameters.Add("@STATUS", System.Data.SqlDbType.VarChar, 20)
                        Dim P_UPDUSER As SqlParameter = sqlCmd.Parameters.Add("@UPDUSER", System.Data.SqlDbType.VarChar, 20)
                        Dim P_UPDTERMID As SqlParameter = sqlCmd.Parameters.Add("@UPDTERMID", System.Data.SqlDbType.VarChar, 20)
                        Dim P_RECEIVEYMD As SqlParameter = sqlCmd.Parameters.Add("@RECEIVEYMD", System.Data.SqlDbType.DateTime)
                        P_CAMPCODE.Value = Me.I_CAMPCODE
                        P_APPLYID.Value = Me.I_APPLYID
                        P_STEP.Value = "01"
                        P_YMD.Value = Date.Now
                        P_DELFLG.Value = "0"
                        P_APPROVALTYPE.Value = "3"
                        P_APPROVERID.Value = "SYSTEM"
                        P_STATUS.Value = "10"
                        P_UPDUSER.Value = I_UPDUSER
                        P_UPDTERMID.Value = I_UPDTERMID
                        P_RECEIVEYMD.Value = C_DEFAULT_YMD

                        sqlCmd.ExecuteNonQuery()
                    End Using

                    '第１ステップ入力依頼
                    'SQL文の作成
                    'Dim sqlStat3 As New System.Text.StringBuilder
                    'sqlStat3.AppendFormat("UPDATE {0}", TBL_APPROVALHIST).AppendLine()
                    'sqlStat3.AppendLine("  SET ")
                    'sqlStat3.AppendLine("   APPROVEDATE = getdate(), ")
                    'sqlStat3.AppendLine("   APPROVERID = M.STAFFCODE, ")
                    'sqlStat3.AppendLine("   STATUS = @P7, ")
                    'sqlStat3.AppendLine("   UPDYMD = getdate(), ")
                    'sqlStat3.AppendLine("   UPDUSER = M.USERID, ")
                    'sqlStat3.AppendLine("   UPDTERMID = @P8, ")
                    'sqlStat3.AppendLine("   RECEIVEYMD = @P9 ")
                    'sqlStat3.AppendFormat("FROM {0} as M, {1} as H", TBL_APPROVAL, TBL_APPROVALHIST).AppendLine()
                    'sqlStat3.AppendLine("  WHERE   H.CAMPCODE    = @P1 ")
                    'sqlStat3.AppendLine("   and    H.APPLYID     = @P2 ")
                    'sqlStat3.AppendLine("   and    H.STEP        = @P3 ")
                    'sqlStat3.AppendLine("   and    H.CAMPCODE    = M.CAMPCODE ")
                    'sqlStat3.AppendLine("   and    H.MAPID       = M.MAPID ")
                    'sqlStat3.AppendLine("   and    H.EVENTCODE   = M.EVENTCODE ")
                    'sqlStat3.AppendLine("   and    H.SUBCODE     = M.SUBCODE ")
                    'sqlStat3.AppendLine("   and    H.STEP        = M.STEP ")
                    'sqlStat3.AppendLine("   and    M.STYMD       <= @P4 ")
                    'sqlStat3.AppendLine("   and    M.ENDYMD      >= @P4 ")
                    'sqlStat3.AppendLine("   and    M.DELFLG      = @P5 ")
                    'sqlStat3.AppendLine("   and    M.APPROVALTYPE = @P6 ")
                    'Using sqlCmd As New SqlCommand(sqlStat3.ToString, sqlConn)

                    '    Dim PARA1 As SqlParameter = sqlCmd.Parameters.Add("@P1", System.Data.SqlDbType.VarChar, 20)
                    '    Dim PARA2 As SqlParameter = sqlCmd.Parameters.Add("@P2", System.Data.SqlDbType.VarChar, 30)
                    '    Dim PARA3 As SqlParameter = sqlCmd.Parameters.Add("@P3", System.Data.SqlDbType.VarChar, 20)
                    '    Dim PARA4 As SqlParameter = sqlCmd.Parameters.Add("@P4", System.Data.SqlDbType.Date)
                    '    Dim PARA5 As SqlParameter = sqlCmd.Parameters.Add("@P5", System.Data.SqlDbType.VarChar, 1)
                    '    Dim PARA6 As SqlParameter = sqlCmd.Parameters.Add("@P6", System.Data.SqlDbType.VarChar, 20)
                    '    Dim PARA7 As SqlParameter = sqlCmd.Parameters.Add("@P7", System.Data.SqlDbType.VarChar, 20)
                    '    Dim PARA8 As SqlParameter = sqlCmd.Parameters.Add("@P8", System.Data.SqlDbType.VarChar, 20)
                    '    Dim PARA9 As SqlParameter = sqlCmd.Parameters.Add("@P9", System.Data.SqlDbType.DateTime)
                    '    PARA1.Value = Me.I_CAMPCODE
                    '    PARA2.Value = Me.I_APPLYID
                    '    PARA3.Value = "01"
                    '    PARA4.Value = Date.Now
                    '    PARA5.Value = "0"
                    '    PARA6.Value = "0"
                    '    PARA7.Value = "00"
                    '    PARA8.Value = I_UPDTERMID
                    '    PARA9.Value = C_DEFAULT_YMD

                    '    sqlCmd.ExecuteNonQuery()
                    'End Using

                Else
                    '登録件数０件
                    O_ERR = "99999"
                End If
                Me.O_LASTSTEP = retValue
            End Using

        Catch ex As Exception
            CS0011LOGWRITE.INFSUBCLASS = "CS0048setApply"               'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:T0009_APPROVALHIST Update"     '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                 '
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            O_ERR = C_MESSAGE_NO.DB_ERROR
            Exit Sub

        End Try

    End Sub


    ''' <summary>
    ''' <para>承認登録</para>
    ''' <para>入力プロパティ(CAMPCODE,APPLYID,STEP)</para>
    ''' <para>出力プロパティ(ERR(処理結果コード):正常終了("00000")、他者による更新済み("10012")、以外エラー)</para>
    ''' </summary>
    Public Sub CS0048setApproval()

        Dim CS0011LOGWRITE As New CS0011LOGWrite                'LogOutput DirString Get

        Try

            If IsNothing(I_CAMPCODE) And I_CAMPCODE = "" Then
                O_ERR = C_MESSAGE_NO.DLL_IF_ERROR
                CS0011LOGWRITE.INFSUBCLASS = "CS0048setApproval"            'SUBクラス名
                CS0011LOGWRITE.INFPOSI = "InParamチェック"                  '
                CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                 '
                CS0011LOGWRITE.TEXT = "DLLインターフェイスエラー(I_CAMPCODE)"
                CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
                CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
                Exit Sub
            End If

            If IsNothing(I_APPLYID) And I_APPLYID = "" Then
                O_ERR = C_MESSAGE_NO.DLL_IF_ERROR
                CS0011LOGWRITE.INFSUBCLASS = "CS0048setApproval"            'SUBクラス名
                CS0011LOGWRITE.INFPOSI = "InParamチェック"                  '
                CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                 '
                CS0011LOGWRITE.TEXT = "DLLインターフェイスエラー(I_APPLYID)"
                CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
                CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
                Exit Sub
            End If

            If IsNothing(I_STEP) And I_STEP = "" Then
                O_ERR = C_MESSAGE_NO.DLL_IF_ERROR
                CS0011LOGWRITE.INFSUBCLASS = "CS0048setApproval"            'SUBクラス名
                CS0011LOGWRITE.INFPOSI = "InParamチェック"                  '
                CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                 '
                CS0011LOGWRITE.TEXT = "DLLインターフェイスエラー(I_STEP)"
                CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
                CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
                Exit Sub
            End If

            If IsNothing(I_STAFFCODE) And I_STAFFCODE = "" Then
                O_ERR = C_MESSAGE_NO.DLL_IF_ERROR
                CS0011LOGWRITE.INFSUBCLASS = "CS0048setApproval"            'SUBクラス名
                CS0011LOGWRITE.INFPOSI = "InParamチェック"                  '
                CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                 '
                CS0011LOGWRITE.TEXT = "DLLインターフェイスエラー(I_STAFFCODE)"
                CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
                CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
                Exit Sub
            End If

            'SQL文の作成
            Dim sqlStat As New System.Text.StringBuilder
            sqlStat.AppendFormat("UPDATE {0}", TBL_APPROVALHIST).AppendLine()
            sqlStat.AppendLine("  SET ")
            sqlStat.AppendLine("   APPROVEDATE = getdate(), ")
            sqlStat.AppendLine("   APPROVERID = @APPROVERID, ")
            sqlStat.AppendLine("   STATUS = @STATUS2, ")
            sqlStat.AppendLine("   UPDYMD = getdate(), ")
            sqlStat.AppendLine("   UPDUSER = @UPDUSER, ")
            sqlStat.AppendLine("   UPDTERMID = @UPDTERMID, ")
            sqlStat.AppendLine("   RECEIVEYMD = @RECEIVEYMD ")
            sqlStat.AppendLine("  WHERE  CAMPCODE    = @CAMPCODE ")
            sqlStat.AppendLine("   and    APPLYID     = @APPLYID ")
            sqlStat.AppendLine("   and    STEP        = @STEP ")
            sqlStat.AppendLine("   and    DELFLG      = @DELFLG ")
            sqlStat.AppendLine("   and    STATUS      = @STATUS ")

            Using sqlConn As New SqlConnection(Convert.ToString(HttpContext.Current.Session("DBcon")))
                Dim sqlRet As Integer
                Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlConn)

                    sqlConn.Open()
                    Dim P_CAMPCODE As SqlParameter = sqlCmd.Parameters.Add("@CAMPCODE", System.Data.SqlDbType.VarChar, 20)
                    Dim P_APPLYID As SqlParameter = sqlCmd.Parameters.Add("@APPLYID", System.Data.SqlDbType.VarChar, 30)
                    Dim P_STEP As SqlParameter = sqlCmd.Parameters.Add("@STEP", System.Data.SqlDbType.VarChar, 20)
                    Dim P_DELFLG As SqlParameter = sqlCmd.Parameters.Add("@DELFLG", System.Data.SqlDbType.VarChar, 1)
                    Dim P_STATUS As SqlParameter = sqlCmd.Parameters.Add("@STATUS", System.Data.SqlDbType.VarChar, 20)
                    Dim P_STATUS2 As SqlParameter = sqlCmd.Parameters.Add("@STATUS2", System.Data.SqlDbType.VarChar, 20)
                    Dim P_APPROVERID As SqlParameter = sqlCmd.Parameters.Add("@APPROVERID", System.Data.SqlDbType.VarChar, 20)
                    Dim P_UPDUSER As SqlParameter = sqlCmd.Parameters.Add("@UPDUSER", System.Data.SqlDbType.VarChar, 20)
                    Dim P_UPDTERMID As SqlParameter = sqlCmd.Parameters.Add("@UPDTERMID", System.Data.SqlDbType.VarChar, 30)
                    Dim P_RECEIVEYMD As SqlParameter = sqlCmd.Parameters.Add("@RECEIVEYMD", System.Data.SqlDbType.DateTime)
                    P_CAMPCODE.Value = Me.I_CAMPCODE
                    P_APPLYID.Value = Me.I_APPLYID
                    P_STEP.Value = Me.I_STEP
                    P_DELFLG.Value = "0"
                    P_STATUS.Value = "02"
                    P_APPROVERID.Value = I_STAFFCODE
                    P_UPDUSER.Value = I_UPDUSER
                    P_STATUS2.Value = "10"
                    P_UPDTERMID.Value = I_UPDTERMID
                    P_RECEIVEYMD.Value = C_DEFAULT_YMD

                    sqlRet = sqlCmd.ExecuteNonQuery()

                End Using

                If sqlRet = 1 Then
                    O_ERR = C_MESSAGE_NO.NORMAL

                    'スキップ承認（更新ステップ前のステップにスキップ設定）
                    'SQL文の作成
                    Dim sqlStat1 As New System.Text.StringBuilder
                    sqlStat1.AppendFormat("UPDATE {0}", TBL_APPROVALHIST).AppendLine()
                    sqlStat1.AppendLine("  SET ")
                    sqlStat1.AppendLine("   APPROVEDATE = getdate(), ")
                    sqlStat1.AppendLine("   APPROVERID = @APPROVERID, ")
                    sqlStat1.AppendLine("   STATUS = @STATUS2, ")
                    sqlStat1.AppendLine("   UPDYMD = getdate(), ")
                    sqlStat1.AppendLine("   UPDUSER = @UPDUSER, ")
                    sqlStat1.AppendLine("   UPDTERMID = @UPDTERMID, ")
                    sqlStat1.AppendLine("   RECEIVEYMD = @RECEIVEYMD ")
                    sqlStat1.AppendLine("  WHERE  CAMPCODE    = @CAMPCODE ")
                    sqlStat1.AppendLine("   and    APPLYID     = @APPLYID ")
                    sqlStat1.AppendLine("   and    STEP        < @STEP ")
                    sqlStat1.AppendLine("   and    DELFLG      = @DELFLG ")
                    sqlStat1.AppendLine("   and    STATUS      = @STATUS ")
                    Using sqlCmd As New SqlCommand(sqlStat1.ToString, sqlConn)

                        Dim P_CAMPCODE As SqlParameter = sqlCmd.Parameters.Add("@CAMPCODE", System.Data.SqlDbType.VarChar, 20)
                        Dim P_APPLYID As SqlParameter = sqlCmd.Parameters.Add("@APPLYID", System.Data.SqlDbType.VarChar, 30)
                        Dim P_STEP As SqlParameter = sqlCmd.Parameters.Add("@STEP", System.Data.SqlDbType.VarChar, 20)
                        Dim P_DELFLG As SqlParameter = sqlCmd.Parameters.Add("@DELFLG", System.Data.SqlDbType.VarChar, 1)
                        Dim P_STATUS As SqlParameter = sqlCmd.Parameters.Add("@STATUS", System.Data.SqlDbType.VarChar, 20)
                        Dim P_APPROVERID As SqlParameter = sqlCmd.Parameters.Add("@APPROVERID", System.Data.SqlDbType.VarChar, 20)
                        Dim P_UPDUSER As SqlParameter = sqlCmd.Parameters.Add("@UPDUSER", System.Data.SqlDbType.VarChar, 20)
                        Dim P_STATUS2 As SqlParameter = sqlCmd.Parameters.Add("@STATUS2", System.Data.SqlDbType.VarChar, 20)
                        Dim P_UPDTERMID As SqlParameter = sqlCmd.Parameters.Add("@UPDTERMID", System.Data.SqlDbType.VarChar, 30)
                        Dim P_RECEIVEYMD As SqlParameter = sqlCmd.Parameters.Add("@RECEIVEYMD", System.Data.SqlDbType.DateTime)
                        P_CAMPCODE.Value = Me.I_CAMPCODE
                        P_APPLYID.Value = Me.I_APPLYID
                        P_STEP.Value = Me.I_STEP
                        P_DELFLG.Value = "0"
                        P_STATUS.Value = "02"
                        P_APPROVERID.Value = I_STAFFCODE
                        P_UPDUSER.Value = I_UPDUSER
                        P_STATUS2.Value = "10"
                        P_UPDTERMID.Value = I_UPDTERMID
                        P_RECEIVEYMD.Value = C_DEFAULT_YMD

                        sqlCmd.ExecuteNonQuery()

                    End Using
                Else
                    '更新件数が１でない場合、すでに他者による更新済み
                    O_ERR = "99999"
                End If

            End Using

        Catch ex As Exception
            CS0011LOGWRITE.INFSUBCLASS = "CS0048setApproval"            'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:T0009_APPROVALHIST Update"     '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                 '
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            O_ERR = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try

    End Sub

    ''' <summary>
    ''' <para>強制 承認登録</para>
    ''' <para>入力プロパティ(CAMPCODE,APPLYID,STEP)</para>
    ''' <para>出力プロパティ(ERR(処理結果コード):正常終了("00000")、他者による更新済み("10012")、以外エラー)</para>
    ''' </summary>
    Public Sub CS0048setAllApproval()

        Dim CS0011LOGWRITE As New BASEDLL.CS0011LOGWrite                'LogOutput DirString Get

        Try

            If IsNothing(I_CAMPCODE) And I_CAMPCODE = "" Then
                O_ERR = C_MESSAGE_NO.DLL_IF_ERROR
                CS0011LOGWRITE.INFSUBCLASS = "CS0048setApproval"            'SUBクラス名
                CS0011LOGWRITE.INFPOSI = "InParamチェック"                  '
                CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                 '
                CS0011LOGWRITE.TEXT = "DLLインターフェイスエラー(I_CAMPCODE)"
                CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
                CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
                Exit Sub
            End If

            If IsNothing(I_SUBCODE) And I_SUBCODE = "" Then
                O_ERR = C_MESSAGE_NO.DLL_IF_ERROR
                CS0011LOGWRITE.INFSUBCLASS = "CS0048setApproval"            'SUBクラス名
                CS0011LOGWRITE.INFPOSI = "InParamチェック"                  '
                CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                 '
                CS0011LOGWRITE.TEXT = "DLLインターフェイスエラー(I_SUBCODE)"
                CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
                CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
                Exit Sub
            End If

            If IsNothing(I_STAFFCODE) And I_STAFFCODE = "" Then
                O_ERR = C_MESSAGE_NO.DLL_IF_ERROR
                CS0011LOGWRITE.INFSUBCLASS = "CS0048setApproval"            'SUBクラス名
                CS0011LOGWRITE.INFPOSI = "InParamチェック"                  '
                CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                 '
                CS0011LOGWRITE.TEXT = "DLLインターフェイスエラー(I_STAFFCODE)"
                CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
                CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
                Exit Sub
            End If

            'SQL文の作成
            Dim sqlStat As New System.Text.StringBuilder
            sqlStat.AppendFormat("UPDATE {0}", TBL_APPROVALHIST).AppendLine()
            sqlStat.AppendLine("  SET ")
            sqlStat.AppendLine("   APPROVEDATE = getdate(), ")
            sqlStat.AppendLine("   APPROVERID = @APPROVERID, ")
            sqlStat.AppendLine("   STATUS = @STATUS2, ")
            sqlStat.AppendLine("   UPDYMD = getdate(), ")
            sqlStat.AppendLine("   UPDUSER = @UPDUSER, ")
            sqlStat.AppendLine("   UPDTERMID = @UPDTERMID, ")
            sqlStat.AppendLine("   RECEIVEYMD = @RECEIVEYMD ")
            sqlStat.AppendLine("  WHERE  CAMPCODE    = @CAMPCODE ")
            sqlStat.AppendLine("   and    SUBCODE     = @SUBCODE ")
            sqlStat.AppendLine("   and    DELFLG      = @DELFLG ")
            sqlStat.AppendLine("   and    STATUS      = @STATUS ")

            Using sqlConn As New SqlConnection(Convert.ToString(HttpContext.Current.Session("DBcon")))
                Dim sqlRet As Integer
                Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlConn)

                    sqlConn.Open()
                    Dim P_CAMPCODE As SqlParameter = sqlCmd.Parameters.Add("@CAMPCODE", System.Data.SqlDbType.VarChar, 20)
                    Dim P_SUBCODE As SqlParameter = sqlCmd.Parameters.Add("@SUBCODE", System.Data.SqlDbType.VarChar, 20)
                    Dim P_DELFLG As SqlParameter = sqlCmd.Parameters.Add("@DELFLG", System.Data.SqlDbType.VarChar, 1)
                    Dim P_STATUS As SqlParameter = sqlCmd.Parameters.Add("@STATUS", System.Data.SqlDbType.VarChar, 20)
                    Dim P_STATUS2 As SqlParameter = sqlCmd.Parameters.Add("@STATUS2", System.Data.SqlDbType.VarChar, 20)
                    Dim P_APPROVERID As SqlParameter = sqlCmd.Parameters.Add("@APPROVERID", System.Data.SqlDbType.VarChar, 20)
                    Dim P_UPDUSER As SqlParameter = sqlCmd.Parameters.Add("@UPDUSER", System.Data.SqlDbType.VarChar, 20)
                    Dim P_UPDTERMID As SqlParameter = sqlCmd.Parameters.Add("@UPDTERMID", System.Data.SqlDbType.VarChar, 30)
                    Dim P_RECEIVEYMD As SqlParameter = sqlCmd.Parameters.Add("@RECEIVEYMD", System.Data.SqlDbType.DateTime)
                    P_CAMPCODE.Value = Me.I_CAMPCODE
                    P_SUBCODE.Value = Me.I_SUBCODE
                    P_DELFLG.Value = "0"
                    P_STATUS.Value = "02"
                    P_APPROVERID.Value = I_STAFFCODE
                    P_UPDUSER.Value = I_UPDUSER
                    P_STATUS2.Value = "10"
                    P_UPDTERMID.Value = I_UPDTERMID
                    P_RECEIVEYMD.Value = C_DEFAULT_YMD

                    sqlRet = sqlCmd.ExecuteNonQuery()

                End Using

                If sqlRet = 1 Then
                    O_ERR = C_MESSAGE_NO.NORMAL
                Else
                    '更新件数が１でない場合、すでに他者による更新済み
                    O_ERR = "99999"
                End If

            End Using

        Catch ex As Exception
            CS0011LOGWRITE.INFSUBCLASS = "CS0048setAllApproval"         'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:T0009_APPROVALHIST Update"     '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                 '
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            O_ERR = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try

    End Sub


    ''' <summary>
    ''' <para>否認登録</para>
    ''' <para>入力プロパティ(CAMPCODE,APPLYID,STEP)</para>
    ''' <para>出力プロパティ(ERR(処理結果コード):正常終了("00000")、他者による更新済み("10012")、以外エラー)</para>
    ''' </summary>
    Public Sub CS0048setDenial()

        Dim CS0011LOGWRITE As New BASEDLL.CS0011LOGWrite                'LogOutput DirString Get

        Try

            If IsNothing(I_CAMPCODE) And I_CAMPCODE = "" Then
                O_ERR = C_MESSAGE_NO.DLL_IF_ERROR
                CS0011LOGWRITE.INFSUBCLASS = "CS0048setDenial"              'SUBクラス名
                CS0011LOGWRITE.INFPOSI = "InParamチェック"                  '
                CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                 '
                CS0011LOGWRITE.TEXT = "DLLインターフェイスエラー(I_CAMPCODE)"
                CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
                CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
                Exit Sub
            End If

            If IsNothing(I_APPLYID) And I_APPLYID = "" Then
                O_ERR = C_MESSAGE_NO.DLL_IF_ERROR
                CS0011LOGWRITE.INFSUBCLASS = "CS0048setDenial"              'SUBクラス名
                CS0011LOGWRITE.INFPOSI = "InParamチェック"                  '
                CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                 '
                CS0011LOGWRITE.TEXT = "DLLインターフェイスエラー(I_APPLYID)"
                CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
                CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
                Exit Sub
            End If

            If IsNothing(I_STEP) And I_STEP = "" Then
                O_ERR = C_MESSAGE_NO.DLL_IF_ERROR
                CS0011LOGWRITE.INFSUBCLASS = "CS0048setDenial"              'SUBクラス名
                CS0011LOGWRITE.INFPOSI = "InParamチェック"                  '
                CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                 '
                CS0011LOGWRITE.TEXT = "DLLインターフェイスエラー(I_STEP)"
                CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
                CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
                Exit Sub
            End If

            If IsNothing(I_STAFFCODE) And I_STAFFCODE = "" Then
                O_ERR = C_MESSAGE_NO.DLL_IF_ERROR
                CS0011LOGWRITE.INFSUBCLASS = "CS0048setDenial"              'SUBクラス名
                CS0011LOGWRITE.INFPOSI = "InParamチェック"                  '
                CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                 '
                CS0011LOGWRITE.TEXT = "DLLインターフェイスエラー(I_STAFFCODE)"
                CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
                CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
                Exit Sub
            End If

            'SQL文の作成
            Dim sqlStat As New System.Text.StringBuilder
            sqlStat.AppendFormat("UPDATE {0}", TBL_APPROVALHIST).AppendLine()
            sqlStat.AppendLine("  SET ")
            sqlStat.AppendLine("   APPROVEDATE = getdate(), ")
            sqlStat.AppendLine("   APPROVERID = @APPROVERID, ")
            sqlStat.AppendLine("   STATUS = @STATUS2, ")
            sqlStat.AppendLine("   UPDYMD = getdate(), ")
            sqlStat.AppendLine("   UPDUSER = @UPDUSER, ")
            sqlStat.AppendLine("   UPDTERMID = @UPDTERMID, ")
            sqlStat.AppendLine("   RECEIVEYMD = @RECEIVEYMD ")
            sqlStat.AppendLine("  WHERE  CAMPCODE    = @CAMPCODE ")
            sqlStat.AppendLine("   and    APPLYID     = @APPLYID ")
            sqlStat.AppendLine("   and    STEP        = @STEP ")
            sqlStat.AppendLine("   and    DELFLG      = @DELFLG ")
            sqlStat.AppendLine("   and    STATUS      = @STATUS ")

            Using sqlConn As New SqlConnection(Convert.ToString(HttpContext.Current.Session("DBcon")))
                Dim sqlRet As Integer
                Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlConn)

                    sqlConn.Open()
                    Dim P_CAMPCODE As SqlParameter = sqlCmd.Parameters.Add("@CAMPCODE", System.Data.SqlDbType.VarChar, 20)
                    Dim P_APPLYID As SqlParameter = sqlCmd.Parameters.Add("@APPLYID", System.Data.SqlDbType.VarChar, 30)
                    Dim P_STEP As SqlParameter = sqlCmd.Parameters.Add("@STEP", System.Data.SqlDbType.VarChar, 20)
                    Dim P_DELFLG As SqlParameter = sqlCmd.Parameters.Add("@DELFLG", System.Data.SqlDbType.VarChar, 1)
                    Dim P_STATUS As SqlParameter = sqlCmd.Parameters.Add("@STATUS", System.Data.SqlDbType.VarChar, 20)
                    Dim P_APPROVERID As SqlParameter = sqlCmd.Parameters.Add("@APPROVERID", System.Data.SqlDbType.VarChar, 20)
                    Dim P_UPDUSER As SqlParameter = sqlCmd.Parameters.Add("@UPDUSER", System.Data.SqlDbType.VarChar, 20)
                    Dim P_STATUS2 As SqlParameter = sqlCmd.Parameters.Add("@STATUS2", System.Data.SqlDbType.VarChar, 20)
                    Dim P_UPDTERMID As SqlParameter = sqlCmd.Parameters.Add("@UPDTERMID", System.Data.SqlDbType.VarChar, 30)
                    Dim P_RECEIVEYMD As SqlParameter = sqlCmd.Parameters.Add("@RECEIVEYMD", System.Data.SqlDbType.DateTime)
                    P_CAMPCODE.Value = Me.I_CAMPCODE
                    P_APPLYID.Value = Me.I_APPLYID
                    P_STEP.Value = Me.I_STEP
                    P_DELFLG.Value = "0"
                    P_STATUS.Value = "02"
                    P_APPROVERID.Value = I_STAFFCODE
                    P_UPDUSER.Value = I_UPDUSER
                    P_STATUS2.Value = "09"
                    P_UPDTERMID.Value = I_UPDTERMID
                    P_RECEIVEYMD.Value = C_DEFAULT_YMD

                    sqlRet = sqlCmd.ExecuteNonQuery()

                End Using

                If sqlRet = 1 Then
                    O_ERR = C_MESSAGE_NO.NORMAL

                    'スキップ承認（更新ステップ前のステップに否認設定）
                    'SQL文の作成
                    Dim sqlStat1 As New System.Text.StringBuilder
                    sqlStat1.AppendFormat("UPDATE {0}", TBL_APPROVALHIST).AppendLine()
                    sqlStat1.AppendLine("  SET ")
                    sqlStat1.AppendLine("   APPROVEDATE = getdate(), ")
                    sqlStat1.AppendLine("   APPROVERID = @APPROVERID, ")
                    sqlStat1.AppendLine("   STATUS = @STATUS2, ")
                    sqlStat1.AppendLine("   UPDYMD = getdate(), ")
                    sqlStat1.AppendLine("   UPDUSER = @UPDUSER, ")
                    sqlStat1.AppendLine("   UPDTERMID = @UPDTERMID, ")
                    sqlStat1.AppendLine("   RECEIVEYMD = @RECEIVEYMD ")
                    sqlStat1.AppendLine("  WHERE  CAMPCODE    = @CAMPCODE ")
                    sqlStat1.AppendLine("   and    APPLYID    = @APPLYID ")
                    sqlStat1.AppendLine("   and    STEP       < @STEP ")
                    sqlStat1.AppendLine("   and    DELFLG     = @DELFLG ")
                    sqlStat1.AppendLine("   and    STATUS     = @STATUS ")
                    Using sqlCmd As New SqlCommand(sqlStat1.ToString, sqlConn)

                        Dim P_CAMPCODE As SqlParameter = sqlCmd.Parameters.Add("@CAMPCODE", System.Data.SqlDbType.VarChar, 20)
                        Dim P_APPLYID As SqlParameter = sqlCmd.Parameters.Add("@APPLYID", System.Data.SqlDbType.VarChar, 30)
                        Dim P_STEP As SqlParameter = sqlCmd.Parameters.Add("@STEP", System.Data.SqlDbType.VarChar, 20)
                        Dim P_DELFLG As SqlParameter = sqlCmd.Parameters.Add("@DELFLG", System.Data.SqlDbType.VarChar, 1)
                        Dim P_STATUS As SqlParameter = sqlCmd.Parameters.Add("@STATUS", System.Data.SqlDbType.VarChar, 20)
                        Dim P_APPROVERID As SqlParameter = sqlCmd.Parameters.Add("@APPROVERID", System.Data.SqlDbType.VarChar, 20)
                        Dim P_UPDUSER As SqlParameter = sqlCmd.Parameters.Add("@UPDUSER", System.Data.SqlDbType.VarChar, 20)
                        Dim P_STATUS2 As SqlParameter = sqlCmd.Parameters.Add("@STATUS2", System.Data.SqlDbType.VarChar, 20)
                        Dim P_UPDTERMID As SqlParameter = sqlCmd.Parameters.Add("@UPDTERMID", System.Data.SqlDbType.VarChar, 30)
                        Dim P_RECEIVEYMD As SqlParameter = sqlCmd.Parameters.Add("@RECEIVEYMD", System.Data.SqlDbType.DateTime)
                        P_CAMPCODE.Value = Me.I_CAMPCODE
                        P_APPLYID.Value = Me.I_APPLYID
                        P_STEP.Value = Me.I_STEP
                        P_DELFLG.Value = "0"
                        P_STATUS.Value = "02"
                        P_APPROVERID.Value = I_STAFFCODE
                        P_UPDUSER.Value = I_UPDUSER
                        P_STATUS2.Value = "09"
                        P_UPDTERMID.Value = I_UPDTERMID
                        P_RECEIVEYMD.Value = C_DEFAULT_YMD

                        sqlCmd.ExecuteNonQuery()

                    End Using


                    '否認（更新ステップ後前のステップに否認設定）
                    'SQL文の作成
                    Dim sqlStat2 As New System.Text.StringBuilder
                    sqlStat2.AppendFormat("UPDATE {0}", TBL_APPROVALHIST).AppendLine()
                    sqlStat2.AppendLine("  SET ")
                    sqlStat2.AppendLine("   APPROVEDATE = getdate(), ")
                    sqlStat2.AppendLine("   APPROVERID = @APPROVERID, ")
                    sqlStat2.AppendLine("   STATUS = @STATUS2, ")
                    sqlStat2.AppendLine("   UPDYMD = getdate(), ")
                    sqlStat2.AppendLine("   UPDUSER = @UPDUSER, ")
                    sqlStat2.AppendLine("   UPDTERMID = @UPDTERMID, ")
                    sqlStat2.AppendLine("   RECEIVEYMD = @RECEIVEYMD ")
                    sqlStat2.AppendLine("  WHERE  CAMPCODE    = @CAMPCODE ")
                    sqlStat2.AppendLine("   and    APPLYID    = @APPLYID ")
                    sqlStat2.AppendLine("   and    STEP       > @STEP ")
                    sqlStat2.AppendLine("   and    DELFLG     = @DELFLG ")
                    sqlStat2.AppendLine("   and    STATUS     = @STATUS ")
                    Using sqlCmd As New SqlCommand(sqlStat2.ToString, sqlConn)

                        Dim P_CAMPCODE As SqlParameter = sqlCmd.Parameters.Add("@CAMPCODE", System.Data.SqlDbType.VarChar, 20)
                        Dim P_APPLYID As SqlParameter = sqlCmd.Parameters.Add("@APPLYID", System.Data.SqlDbType.VarChar, 30)
                        Dim P_STEP As SqlParameter = sqlCmd.Parameters.Add("@STEP", System.Data.SqlDbType.VarChar, 20)
                        Dim P_DELFLG As SqlParameter = sqlCmd.Parameters.Add("@DELFLG", System.Data.SqlDbType.VarChar, 1)
                        Dim P_STATUS As SqlParameter = sqlCmd.Parameters.Add("@STATUS", System.Data.SqlDbType.VarChar, 20)
                        Dim P_APPROVERID As SqlParameter = sqlCmd.Parameters.Add("@APPROVERID", System.Data.SqlDbType.VarChar, 20)
                        Dim P_UPDUSER As SqlParameter = sqlCmd.Parameters.Add("@UPDUSER", System.Data.SqlDbType.VarChar, 20)
                        Dim P_STATUS2 As SqlParameter = sqlCmd.Parameters.Add("@STATUS2", System.Data.SqlDbType.VarChar, 20)
                        Dim P_UPDTERMID As SqlParameter = sqlCmd.Parameters.Add("@UPDTERMID", System.Data.SqlDbType.VarChar, 30)
                        Dim P_RECEIVEYMD As SqlParameter = sqlCmd.Parameters.Add("@RECEIVEYMD", System.Data.SqlDbType.DateTime)
                        P_CAMPCODE.Value = Me.I_CAMPCODE
                        P_APPLYID.Value = Me.I_APPLYID
                        P_STEP.Value = Me.I_STEP
                        P_DELFLG.Value = "0"
                        P_STATUS.Value = "02"
                        P_APPROVERID.Value = I_STAFFCODE
                        P_UPDUSER.Value = I_UPDUSER
                        P_STATUS2.Value = "09"
                        P_UPDTERMID.Value = I_UPDTERMID
                        P_RECEIVEYMD.Value = C_DEFAULT_YMD

                        sqlCmd.ExecuteNonQuery()

                    End Using
                Else
                    '更新件数が１でない場合、すでに他者による更新済み
                    O_ERR = "99999"
                End If

            End Using

        Catch ex As Exception
            CS0011LOGWRITE.INFSUBCLASS = "CS0048setDenial"              'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:T0009_APPROVALHIST Update"     '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                 '
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            O_ERR = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try

    End Sub

    ''' <summary>
    ''' <para>申請取り消し</para>
    ''' <para>入力プロパティ(CAMPCODE,APPLYID)</para>
    ''' <para>出力プロパティ(ERR(処理結果コード):正常終了("00000")、以外エラー)</para>
    ''' </summary>
    Public Sub CS0048delApply()

        Dim CS0011LOGWRITE As New BASEDLL.CS0011LOGWrite                'LogOutput DirString Get
        Dim retValue As String = "99"

        Try
            O_ERR = C_MESSAGE_NO.NORMAL

            If IsNothing(I_CAMPCODE) And I_CAMPCODE = "" Then
                O_ERR = C_MESSAGE_NO.DLL_IF_ERROR
                CS0011LOGWRITE.INFSUBCLASS = "CS0048delApply"               'SUBクラス名
                CS0011LOGWRITE.INFPOSI = "InParamチェック"                  '
                CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                 '
                CS0011LOGWRITE.TEXT = "DLLインターフェイスエラー(I_CAMPCODE)"
                CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
                CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
                Exit Sub
            End If

            If IsNothing(I_APPLYID) And I_APPLYID = "" Then
                O_ERR = C_MESSAGE_NO.DLL_IF_ERROR
                CS0011LOGWRITE.INFSUBCLASS = "CS0048delApply"               'SUBクラス名
                CS0011LOGWRITE.INFPOSI = "InParamチェック"                  '
                CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                 '
                CS0011LOGWRITE.TEXT = "DLLインターフェイスエラー(I_APPLYID)"
                CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
                CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
                Exit Sub
            End If

            '最終承認ステップ取得
            'SQL文の作成
            'SQL文の作成
            Dim sqlStat As New System.Text.StringBuilder
            sqlStat.AppendLine("  SELECT ")
            sqlStat.AppendLine("   isnull(rtrim(T.STATUS),'') AS STATUS ")
            sqlStat.AppendFormat("FROM {0} T ", TBL_APPROVALHIST).AppendLine()
            sqlStat.AppendLine("  WHERE T.CAMPCODE = @CAMPCODE ")
            sqlStat.AppendLine("  AND   T.APPLYID = @APPLYID ")
            sqlStat.AppendLine("  AND   T.STEP = ( ")
            sqlStat.AppendFormat("                SELECT MAX(STEP) FROM {0} ", TBL_APPROVALHIST).AppendLine()
            sqlStat.AppendLine("                  WHERE CAMPCODE = @CAMPCODE ")
            sqlStat.AppendLine("                  AND   APPLYID  = @APPLYID ")
            sqlStat.AppendLine("                  AND   DELFLG   = @DELFLG ")
            sqlStat.AppendLine("                 ) ")
            sqlStat.AppendLine("  AND  T.DELFLG = @DELFLG ")

            Using sqlConn As New SqlConnection(Convert.ToString(HttpContext.Current.Session("DBcon")))
                Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlConn)
                    sqlConn.Open()
                    Dim P_CAMPCODE As SqlParameter = sqlCmd.Parameters.Add("@CAMPCODE", System.Data.SqlDbType.VarChar, 20)
                    Dim P_APPLYID As SqlParameter = sqlCmd.Parameters.Add("@APPLYID", System.Data.SqlDbType.VarChar, 30)
                    Dim P_DELFLG As SqlParameter = sqlCmd.Parameters.Add("@DELFLG", System.Data.SqlDbType.VarChar, 1)
                    P_CAMPCODE.Value = Me.I_CAMPCODE
                    P_APPLYID.Value = Me.I_APPLYID
                    P_DELFLG.Value = "0"

                    Using sqlDr As SqlDataReader = sqlCmd.ExecuteReader()
                        While sqlDr.Read
                            retValue = Convert.ToString(sqlDr("STATUS"))
                            Exit While
                        End While
                    End Using
                End Using

                'SQL文の作成
                If retValue = "02" Then
                    '申請取り消し
                    Dim sqlStat1 As New System.Text.StringBuilder
                    sqlStat1.AppendFormat("UPDATE {0}", TBL_APPROVALHIST).AppendLine()
                    sqlStat1.AppendLine("  SET ")
                    sqlStat1.AppendLine("   STATUS = @STATUS, ")
                    sqlStat1.AppendLine("   DELFLG = @DELFLG, ")
                    sqlStat1.AppendLine("   UPDYMD = getdate(), ")
                    sqlStat1.AppendLine("   UPDUSER = @UPDUSER, ")
                    sqlStat1.AppendLine("   UPDTERMID = @UPDTERMID, ")
                    sqlStat1.AppendLine("   RECEIVEYMD = @RECEIVEYMD ")
                    sqlStat1.AppendLine("  WHERE  CAMPCODE    = @CAMPCODE ")
                    sqlStat1.AppendLine("   and    APPLYID    = @APPLYID ")

                    Using sqlCmd As New SqlCommand(sqlStat1.ToString, sqlConn)
                        Dim P_CAMPCODE As SqlParameter = sqlCmd.Parameters.Add("@CAMPCODE", System.Data.SqlDbType.VarChar, 20)
                        Dim P_APPLYID As SqlParameter = sqlCmd.Parameters.Add("@APPLYID", System.Data.SqlDbType.VarChar, 30)
                        Dim P_STATUS As SqlParameter = sqlCmd.Parameters.Add("@STATUS", System.Data.SqlDbType.VarChar, 20)
                        Dim P_DELFLG As SqlParameter = sqlCmd.Parameters.Add("@DELFLG", System.Data.SqlDbType.VarChar, 1)
                        Dim P_UPDUSER As SqlParameter = sqlCmd.Parameters.Add("@UPDUSER", System.Data.SqlDbType.VarChar, 20)
                        Dim P_UPDTERMID As SqlParameter = sqlCmd.Parameters.Add("@UPDTERMID", System.Data.SqlDbType.VarChar, 30)
                        Dim P_RECEIVEYMD As SqlParameter = sqlCmd.Parameters.Add("@RECEIVEYMD", System.Data.SqlDbType.DateTime)
                        P_CAMPCODE.Value = Me.I_CAMPCODE
                        P_APPLYID.Value = Me.I_APPLYID
                        P_STATUS.Value = "03"
                        P_DELFLG.Value = "1"
                        P_UPDUSER.Value = I_UPDUSER
                        P_UPDTERMID.Value = I_UPDTERMID
                        P_RECEIVEYMD.Value = C_DEFAULT_YMD

                        sqlCmd.ExecuteNonQuery()
                        O_ERR = C_MESSAGE_NO.NORMAL

                    End Using
                End If
            End Using

        Catch ex As Exception
            CS0011LOGWRITE.INFSUBCLASS = "CS0048delApply"               'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:T0009_APPROVALHIST Update"     '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                 '
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            O_ERR = C_MESSAGE_NO.DB_ERROR
            Exit Sub

        End Try

    End Sub

    ''' <summary>
    ''' <para>状態取得</para>
    ''' <para>入力プロパティ(CAMPCODE,APPLYID,MAPID,EVENTCODE,SUBCODE)</para>
    ''' <para>出力プロパティ(ERR(処理結果コード):正常終了("00000")、以外エラー)</para>
    ''' <para>出力プロパティ(APPLYSTATE(申請状況)：O_APPLYSTATEのsummary参照)</para>
    ''' </summary>
    Public Sub CS0048getApplyState()

        Dim CS0011LOGWRITE As New CS0011LOGWrite                'LogOutput DirString Get

        Try
            Me.O_ERR = C_MESSAGE_NO.NORMAL

            If IsNothing(I_CAMPCODE) And I_CAMPCODE = "" Then
                O_ERR = C_MESSAGE_NO.DLL_IF_ERROR
                CS0011LOGWRITE.INFSUBCLASS = "CS0048getApplyState"          'SUBクラス名
                CS0011LOGWRITE.INFPOSI = "InParamチェック"                  '
                CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                 '
                CS0011LOGWRITE.TEXT = "DLLインターフェイスエラー(I_CAMPCODE)"
                CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
                CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
                Exit Sub
            End If

            If IsNothing(I_APPLYID) And I_APPLYID = "" Then
                O_ERR = C_MESSAGE_NO.DLL_IF_ERROR
                CS0011LOGWRITE.INFSUBCLASS = "CS0048getApplyState"          'SUBクラス名
                CS0011LOGWRITE.INFPOSI = "InParamチェック"                  '
                CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                 '
                CS0011LOGWRITE.TEXT = "DLLインターフェイスエラー(I_APPLYID)"
                CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
                CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
                Exit Sub
            End If

            If IsNothing(I_MAPID) And I_MAPID = "" Then
                O_ERR = C_MESSAGE_NO.DLL_IF_ERROR
                CS0011LOGWRITE.INFSUBCLASS = "CS0048getApplyState"          'SUBクラス名
                CS0011LOGWRITE.INFPOSI = "InParamチェック"                  '
                CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                 '
                CS0011LOGWRITE.TEXT = "DLLインターフェイスエラー(I_MAPID)"
                CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
                CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
                Exit Sub
            End If

            If IsNothing(I_EVENTCODE) And I_EVENTCODE = "" Then
                O_ERR = C_MESSAGE_NO.DLL_IF_ERROR
                CS0011LOGWRITE.INFSUBCLASS = "CS0048getApplyState"          'SUBクラス名
                CS0011LOGWRITE.INFPOSI = "InParamチェック"                  '
                CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                 '
                CS0011LOGWRITE.TEXT = "DLLインターフェイスエラー(I_EVENTCODE)"
                CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
                CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
                Exit Sub
            End If

            If IsNothing(I_SUBCODE) And I_SUBCODE = "" Then
                O_ERR = C_MESSAGE_NO.DLL_IF_ERROR
                CS0011LOGWRITE.INFSUBCLASS = "CS0048getApplyState"          'SUBクラス名
                CS0011LOGWRITE.INFPOSI = "InParamチェック"                  '
                CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                 '
                CS0011LOGWRITE.TEXT = "DLLインターフェイスエラー(I_SUBCODE)"
                CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
                CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
                Exit Sub
            End If

            'SQL文の作成
            Dim sqlStat As New System.Text.StringBuilder
            sqlStat.AppendLine("  SELECT ")
            sqlStat.AppendLine("   T.APPLYID AS APPLYID, ")
            sqlStat.AppendLine("   T.APPLYDATE AS APPLYDATE, ")
            sqlStat.AppendLine("   T.APPLICANTID AS APPLICANTID, ")
            sqlStat.AppendLine("   T.STATUS AS STATUS, ")
            sqlStat.AppendLine("   T.APPROVEDATE AS APPROVEDATE, ")
            sqlStat.AppendLine("   T.APPROVERID AS APPROVERID ")
            sqlStat.AppendFormat("FROM {0} T ", TBL_APPROVALHIST).AppendLine()
            sqlStat.AppendLine("  WHERE T.CAMPCODE = @CAMPCODE ")
            sqlStat.AppendLine("  AND   T.APPLYID = @APPLYID ")
            sqlStat.AppendLine("  AND   T.STEP = ( ")
            sqlStat.AppendFormat("                SELECT MAX(STEP) FROM {0} ", TBL_APPROVAL).AppendLine()
            sqlStat.AppendLine("                  WHERE CAMPCODE = @CAMPCODE ")
            sqlStat.AppendLine("                  AND   MAPID = @MAPID ")
            sqlStat.AppendLine("                  AND   EVENTCODE = @EVENTCODE ")
            sqlStat.AppendLine("                  AND   SUBCODE = @SUBCODE ")
            sqlStat.AppendLine("                  AND   ENDYMD >= @YMD ")
            sqlStat.AppendLine("                  AND   STYMD <= @YMD ")
            sqlStat.AppendLine("                  AND   DELFLG = @DELFLG ")
            sqlStat.AppendLine("                 ) ")
            sqlStat.AppendLine("  AND  T.DELFLG = @P7 ")

            Using sqlConn As New SqlConnection(Convert.ToString(HttpContext.Current.Session("DBcon"))) _
                , sqlCmd As New SqlCommand(sqlStat.ToString, sqlConn)
                sqlConn.Open()
                Dim P_CAMPCODE As SqlParameter = sqlCmd.Parameters.Add("@CAMPCODE", System.Data.SqlDbType.VarChar, 20)
                Dim P_APPLYID As SqlParameter = sqlCmd.Parameters.Add("@APPLYID", System.Data.SqlDbType.VarChar, 30)
                Dim P_MAPID As SqlParameter = sqlCmd.Parameters.Add("@MAPID", System.Data.SqlDbType.VarChar, 20)
                Dim P_EVENTCODE As SqlParameter = sqlCmd.Parameters.Add("@EVENTCODE", System.Data.SqlDbType.VarChar, 20)
                Dim P_SUBCODE As SqlParameter = sqlCmd.Parameters.Add("@SUBCODE", System.Data.SqlDbType.VarChar, 20)
                Dim P_YMD As SqlParameter = sqlCmd.Parameters.Add("@YMD", System.Data.SqlDbType.Date)
                Dim P_DELFLG As SqlParameter = sqlCmd.Parameters.Add("@DELFLG", System.Data.SqlDbType.VarChar, 1)
                P_CAMPCODE.Value = Me.I_CAMPCODE
                P_APPLYID.Value = Me.I_APPLYID
                P_MAPID.Value = Me.I_MAPID
                P_EVENTCODE.Value = Me.I_EVENTCODE
                P_SUBCODE.Value = Me.I_SUBCODE
                P_YMD.Value = Date.Now
                P_DELFLG.Value = "0"

                Using sqlDr As SqlDataReader = sqlCmd.ExecuteReader()
                    While sqlDr.Read

                        O_APPLYSTATE.Add("APPLYID", Convert.ToString(sqlDr("APPLYID")))
                        O_APPLYSTATE.Add("APPLYDATE", Convert.ToString(sqlDr("APPLYDATE")))
                        O_APPLYSTATE.Add("APPLICANTID", Convert.ToString(sqlDr("APPLICANTID")))
                        O_APPLYSTATE.Add("STATUS", Convert.ToString(sqlDr("STATUS")))
                        O_APPLYSTATE.Add("APPROVEDATE", Convert.ToString(sqlDr("APPROVEDATE")))
                        O_APPLYSTATE.Add("APPROVERID", Convert.ToString(sqlDr("APPROVERID")))
                        Me.O_ERR = C_MESSAGE_NO.NORMAL

                        Exit While
                    End While
                End Using

                If Me.O_ERR <> C_MESSAGE_NO.NORMAL Then

                    ' データ未取得は予期せぬ状態
                    CS0011LOGWRITE.INFSUBCLASS = "CS0048getApplyState"          'SUBクラス名
                    CS0011LOGWRITE.INFPOSI = "DB:T0009_APPROVALHIST Select"     '
                    CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                 '
                    CS0011LOGWRITE.TEXT = "データ不整合（APPLYID=" & Me.I_APPLYID & ",MAPID=" & Me.I_MAPID & ",EVENTCODE=" & Me.I_EVENTCODE & ",SUBCODE=" & Me.I_SUBCODE & ")"
                    CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.SYSTEM_ADM_ERROR
                    CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力

                End If

            End Using

        Catch ex As Exception
            CS0011LOGWRITE.INFSUBCLASS = "CS0048getApplyState"          'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:T0009_APPROVALHIST Select"     '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                 '
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            O_ERR = C_MESSAGE_NO.DB_ERROR
        End Try

    End Sub

End Structure
