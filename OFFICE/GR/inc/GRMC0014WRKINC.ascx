<%@ Control Language="vb" AutoEventWireup="false" CodeBehind="GRMC0014WRKINC.ascx.vb" Inherits="OFFICE.GRMC0014WRKINC" %>

<!-- Work レイアウト -->
<div hidden="hidden">
    <asp:TextBox ID="WF_SEL_CAMPCODE" runat="server"></asp:TextBox>         <!-- 会社コード -->
    <asp:TextBox ID="WF_SEL_TORICODE" runat="server"></asp:TextBox>         <!-- 取引先コード -->
    <asp:TextBox ID="WF_SEL_PRODUCT1" runat="server"></asp:TextBox>         <!-- 品名１ -->
    <asp:TextBox ID="WF_SEL_URIHIYOKBN" runat="server"></asp:TextBox>       <!-- 売上費用区分 -->
    <asp:TextBox ID="WF_SEL_STYMD" runat="server"></asp:TextBox>            <!-- 有効年月日(From) -->
    <asp:TextBox ID="WF_SEL_ENDYMD" runat="server"></asp:TextBox>           <!-- 有効年月日(To) -->
</div>
