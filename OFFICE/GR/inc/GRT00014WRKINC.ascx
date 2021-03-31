<%@ Control Language="vb" AutoEventWireup="false" CodeBehind="GRT00014WRKINC.ascx.vb" Inherits="OFFICE.GRT00014WRKINC" %>

<!-- Work レイアウト -->
<div hidden="hidden">
    <asp:TextBox ID="WF_SEL_CAMPCODE" runat="server"></asp:TextBox>                 <!-- 会社コード -->
    <asp:TextBox ID="WF_SEL_TAISHOYM" runat="server"></asp:TextBox>                 <!-- 対象年月 -->
    <asp:TextBox ID="WF_SEL_HORG" runat="server"></asp:TextBox>                     <!-- 配属部署 -->
    <asp:TextBox ID="WF_SEL_STAFFKBN" runat="server"></asp:TextBox>                 <!-- 社員区分 -->
    <asp:TextBox ID="WF_SEL_STAFFCODE" runat="server"></asp:TextBox>                <!-- 従業員(コード) -->
    <asp:TextBox ID="WF_SEL_STAFFNAMES" runat="server"></asp:TextBox>               <!-- 従業員(名称) -->
    <asp:TextBox ID="WF_SEL_WORKKBN" runat="server"></asp:TextBox>                  <!-- 作業区分(コード) -->

    <asp:TextBox ID="WF_SEL_LIMITFLG" runat="server"></asp:TextBox>                 <!-- 締フラグ -->
    <asp:TextBox ID="WF_SEL_PERMITCODE" runat="server"></asp:TextBox>               <!-- 権限コード -->
</div>
