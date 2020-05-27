Imports System.Data.SqlClient

Public Class GRT00012WRKINC
    Inherits UserControl

    Public Const MAPIDS As String = "T00012S"                        'MAPID(選択)
    Public Const MAPID As String = "T00012"                          'MAPID(実行)
    ''' <summary>
    ''' 端末区分
    ''' </summary>
    Public Class TERM_TYPE
        Public Const YAZAKI As String = "1"
        Public Const JX As String = "2"
        Public Const JOT As String = "3"
        Public Const HAND As String = "4"
        Public Const TG As String = "5"
        Public Const COSMO As String = "6"
    End Class

    ''' <summary>
    ''' ワークデータ初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub initialize()
        '選択情報のWF_SELクリア

    End Sub

    ''' <summary>
    ''' 固定値マスタから一覧の取得
    ''' </summary>
    ''' <param name="COMPCODE">会社コード</param>
    ''' <returns>検索条件一覧</returns>
    ''' <remarks></remarks>
    Function createFIXParam(ByVal COMPCODE As String) As Hashtable

        Return createFIXParam(COMPCODE, "")
    End Function
    ''' <summary>
    ''' 固定値マスタから一覧の取得
    ''' </summary>
    ''' <param name="COMPCODE">会社コード</param>
    ''' <param name="FIXCODE">固定値区分</param>
    ''' <returns>検索条件一覧</returns>
    ''' <remarks></remarks>
    Function createFIXParam(ByVal COMPCODE As String, ByVal FIXCODE As String) As Hashtable
        Dim prmData As New Hashtable
        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_COMPANY) = COMPCODE
        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_FIX_CLASS) = FIXCODE
        Return prmData
    End Function

    ''' <summary>
    ''' 社員一覧取得用パラメータ設定
    ''' </summary>
    ''' <param name="COMPCODE">会社コード</param>
    ''' <param name="ORGCODE" >部署コード</param>
    ''' <returns>検索条件一覧</returns>
    ''' <remarks></remarks>
    Function createSTAFFParam(ByVal COMPCODE As String, ByVal ORGCODE As String) As Hashtable
        Dim prmData As New Hashtable
        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_COMPANY) = COMPCODE
        If Not String.IsNullOrEmpty(ORGCODE) Then
            prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_ORG) = ORGCODE
        End If
        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_TYPEMODE) = GL0005StaffList.LC_STAFF_TYPE.DRIVER
        Return prmData
    End Function

    ''' <summary>
    ''' 部署一覧取得用パラメータ設定
    ''' </summary>
    ''' <param name="COMPCODE">会社コード</param>
    ''' <param name="PRMIT">権限区分</param>
    ''' <returns>検索条件一覧</returns>
    ''' <remarks></remarks>
    Function createORGParam(ByVal COMPCODE As String, ByVal PRMIT As String) As Hashtable
        Dim prmData As New Hashtable
        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_COMPANY) = COMPCODE
        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_ORG_CATEGORYS) = New String() {GL0002OrgList.C_CATEGORY_LIST.CARAGE}
        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_PERMISSION) = PRMIT
        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_TYPEMODE) = GL0002OrgList.LS_AUTHORITY_WITH.USER
        Return prmData
    End Function

End Class