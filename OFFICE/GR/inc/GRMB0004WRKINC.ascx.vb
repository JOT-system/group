﻿Imports OFFICE.GRIS0005LeftBox
Imports BASEDLL

Public Class GRMB0004WRKINC
    Inherits UserControl

    Public Const MAPIDS As String = "MB0004S"       'MAPID(条件)
    Public Const MAPID As String = "MB0004"         'MAPID(実行)

    ''' <summary>
    ''' ワークデータ初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub Initialize()
    End Sub

    ''' <summary>
    ''' 配属部署パラメーター
    ''' </summary>
    ''' <param name="I_COMPCODE"></param>
    ''' <returns></returns>
    ''' <remarks>車庫、支店、部、役員</remarks>
    Public Function CreateHORGParam(ByVal I_COMPCODE As String) As Hashtable

        Dim prmData As New Hashtable
        prmData.Item(C_PARAMETERS.LP_COMPANY) = I_COMPCODE
        prmData.Item(C_PARAMETERS.LP_TYPEMODE) = GL0002OrgList.LS_AUTHORITY_WITH.NO_AUTHORITY
        prmData.Item(C_PARAMETERS.LP_PERMISSION) = C_PERMISSION.INVALID
        prmData.Item(C_PARAMETERS.LP_ORG_CATEGORYS) = New String() {
            GL0002OrgList.C_CATEGORY_LIST.BRANCH_OFFICE,
            GL0002OrgList.C_CATEGORY_LIST.CARAGE,
            GL0002OrgList.C_CATEGORY_LIST.DEPARTMENT,
            GL0002OrgList.C_CATEGORY_LIST.OFFICE_PLACE,
            GL0002OrgList.C_CATEGORY_LIST.OFFICER}

        CreateHORGParam = prmData

    End Function

End Class
