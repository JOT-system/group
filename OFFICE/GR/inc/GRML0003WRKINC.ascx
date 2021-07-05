<%@ Control Language="vb" AutoEventWireup="false" CodeBehind="GRML0003WRKINC.ascx.vb" Inherits="OFFICE.GRML0003WRKINC" %>

<!-- Work レイアウト -->
<div hidden="hidden">
    <asp:TextBox ID="WF_SEL_CAMPCODE" runat="server"></asp:TextBox>         <!-- 会社コード -->
    <asp:TextBox ID="WF_SEL_SHIWAKEPATERNKBN" runat="server"></asp:TextBox> <!-- 仕分パターン分類 -->
    <!--<asp:TextBox ID="WF_SEL_ACDCKBN" runat="server"></asp:TextBox>      <!-- 貸借区分 -->
    <asp:TextBox ID="WF_SEL_STYMD" runat="server"></asp:TextBox>            <!-- 有効年月日(From) -->
    <asp:TextBox ID="WF_SEL_ENDYMD" runat="server"></asp:TextBox>           <!-- 有効年月日(To) -->
</div>
