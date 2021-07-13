﻿Imports OFFICE.GRIS0005LeftBox

Public Class GRML0003WRKINC
    Inherits UserControl

    Public Const MAPIDS As String = "ML0003S"       'MAPID(選択)
    Public Const MAPID As String = "ML0003"         'MAPID(実行)

    ''' <summary>
    ''' ワークデータ初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub Initialize()

    End Sub


    ''' <summary>
    ''' 固定値マスタから一覧の取得
    ''' </summary>
    ''' <param name="COMPCODE"></param>
    ''' <param name="FIXCODE"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Function CreateFIXParam(ByVal COMPCODE As String, Optional ByVal FIXCODE As String = "") As Hashtable
        Dim prmData As New Hashtable
        prmData.Item(C_PARAMETERS.LP_COMPANY) = COMPCODE
        prmData.Item(C_PARAMETERS.LP_FIX_CLASS) = FIXCODE
        CreateFIXParam = prmData
    End Function


    ''' <summary>
    ''' 取引先一覧の取得
    ''' </summary>
    ''' <param name="COMPCODE"></param>
    ''' <returns></returns>
    ''' <remarks>全て</remarks>
    Function CreateTORIParam(ByVal COMPCODE As String) As Hashtable
        Dim prmData As New Hashtable
        prmData.Item(C_PARAMETERS.LP_COMPANY) = COMPCODE
        prmData.Item(C_PARAMETERS.LP_TYPEMODE) = GL0003CustomerList.LC_CUSTOMER_TYPE.ALL
        CreateTORIParam = prmData
    End Function


    ''' <summary>
    ''' 勘定科目一覧の取得
    ''' </summary>
    ''' <param name="COMPCODE"></param>
    ''' <returns></returns>
    ''' <remarks>全て</remarks>
    Function CreateACCParam(ByVal COMPCODE As String, ByVal ACSUBCODE As String) As Hashtable
        Dim prmData As New Hashtable
        prmData.Item(C_PARAMETERS.LP_COMPANY) = COMPCODE
        prmData.Item(C_PARAMETERS.LP_ACSUBCODE) = ACSUBCODE
        CreateACCParam = prmData
    End Function


    ''' <summary>
    ''' 出荷部署一覧の取得
    ''' </summary>
    ''' <param name="COMPCODE"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Function createORGParam(ByVal COMPCODE As String, ByVal ISORDER As Boolean) As Hashtable
        Dim prmData As New Hashtable
        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_COMPANY) = COMPCODE
        If ISORDER Then
            ' 受注部署
            prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_ORG_CATEGORYS) = New String() {GL0002OrgList.C_CATEGORY_LIST.CARAGE}
        Else
            ' 出荷部署
            prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_ORG_CATEGORYS) = New String() {GL0002OrgList.C_CATEGORY_LIST.CARAGE}
        End If
        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_PERMISSION) = C_PERMISSION.INVALID
        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_TYPEMODE) = GL0002OrgList.LS_AUTHORITY_WITH.USER
        createORGParam = prmData
    End Function

End Class
