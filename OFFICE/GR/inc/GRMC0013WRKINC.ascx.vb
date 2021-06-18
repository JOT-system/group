Imports OFFICE.GRIS0005LeftBox

Public Class GRMC0013WRKINC
    Inherits UserControl

    Public Const MAPIDS As String = "MC0013S"       'MAPID(選択)
    Public Const MAPID As String = "MC0013"         'MAPID(実行)

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
    ''' 管理部署パラメーター
    ''' </summary>
    ''' <param name="I_COMPCODE"></param>
    ''' <returns></returns>
    ''' <remarks>支店、部、役員</remarks>
    Public Function CreateMORGParam(ByVal I_COMPCODE As String) As Hashtable

        Dim prmData As New Hashtable
        prmData.Item(C_PARAMETERS.LP_COMPANY) = I_COMPCODE
        prmData.Item(C_PARAMETERS.LP_TYPEMODE) = GL0002OrgList.LS_AUTHORITY_WITH.NO_AUTHORITY
        prmData.Item(C_PARAMETERS.LP_PERMISSION) = C_PERMISSION.INVALID
        prmData.Item(C_PARAMETERS.LP_ORG_CATEGORYS) = New String() {
            GL0002OrgList.C_CATEGORY_LIST.BRANCH_OFFICE,
            GL0002OrgList.C_CATEGORY_LIST.DEPARTMENT,
            GL0002OrgList.C_CATEGORY_LIST.OFFICER}

        CreateMORGParam = prmData

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


    ''' <summary>
    ''' 届先一覧取得用パラメータ設定
    ''' </summary>
    ''' <param name="COMPCODE">会社コード</param>
    ''' <param name="ORGCODE">部署コード</param>
    ''' <param name="SHIPCODE">取引先コード</param>
    ''' <param name="CLASSCODE">区分コード</param>
    ''' <param name="ALL">全件</param>
    ''' <returns>検索条件一覧</returns>
    ''' <remarks></remarks>
    Function createDistinationParam(ByVal COMPCODE As String, ByVal ORGCODE As String, ByVal SHIPCODE As String, ByVal CLASSCODE As String, ByVal ALL As Boolean) As Hashtable
        Dim prmData As New Hashtable
        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_COMPANY) = COMPCODE
        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_ORG) = ORGCODE

        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_CUSTOMER) = SHIPCODE
        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_CLASSCODE) = CLASSCODE
        If ALL Then
            prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_TYPEMODE) = GL0004DestinationList.LC_DEST_TYPE.ALL
        Else
            prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_TYPEMODE) = GL0004DestinationList.LC_DEST_TYPE.EXCEPT_JXCOSMO
        End If

        Return prmData
    End Function


    ''' <summary>
    ''' 届先一覧の取得
    ''' </summary>
    ''' <param name="COMPCODE"></param>
    ''' <param name="TORICODE"></param>
    ''' <returns></returns>
    ''' <remarks>全て</remarks>
    Function CreateTODOKEParam(ByVal COMPCODE As String, Optional ByVal TORICODE As String = Nothing) As Hashtable
        Dim prmData As New Hashtable
        prmData.Item(C_PARAMETERS.LP_COMPANY) = COMPCODE
        If Not String.IsNullOrEmpty(TORICODE) Then
            prmData.Item(C_PARAMETERS.LP_CUSTOMER) = TORICODE
        End If
        prmData.Item(C_PARAMETERS.LP_TYPEMODE) = GL0004DestinationList.LC_DEST_TYPE.ALL
        CreateTODOKEParam = prmData
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

End Class
