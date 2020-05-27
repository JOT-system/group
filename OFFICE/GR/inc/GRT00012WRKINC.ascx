<%@ Control Language="vb" AutoEventWireup="false" CodeBehind="GRT00012WRKINC.ascx.vb" Inherits="OFFICE.GRT00012WRKINC" %>
        <!-- Work レイアウト -->
        <div hidden="hidden">
            <!--  画面（条件選択）  -->
            <asp:TextBox ID="WF_SEL_CAMPCODE" runat="server"></asp:TextBox>           <!-- 会社　 -->
            <asp:TextBox ID="WF_SEL_STYMD" runat="server"></asp:TextBox>              <!-- 出庫年月日開始　 -->
            <asp:TextBox ID="WF_SEL_ENDYMD" runat="server"></asp:TextBox>             <!-- 出庫年月日終了　 -->
            <asp:TextBox ID="WF_SEL_UORG" runat="server"></asp:TextBox>               <!-- 運用部署　 -->
            <asp:TextBox ID="WF_SEL_STAFFCODE" runat="server"></asp:TextBox>          <!-- 従業員　 -->
            <asp:TextBox ID="WF_SEL_STAFFNAME" runat="server"></asp:TextBox>          <!-- 従業員名　 -->
        </div>