<%@ Control Language="vb" AutoEventWireup="false" CodeBehind="GRT00016WRKINC.ascx.vb" Inherits="OFFICE.GRT00016WRKINC" %>
        <!-- Work レイアウト -->
        <div hidden="hidden">
            <!-- 　マルチウィンドウ　自画面情報 -->
            <asp:TextBox ID="WF_SEL_CAMPCODE" runat="server" ></asp:TextBox>                <!-- 会社コード　 -->
            <asp:TextBox ID="WF_SEL_SEIKYUYMF" runat="server"></asp:TextBox>                <!-- 請求月FROM　 -->
            <asp:TextBox ID="WF_SEL_SEIKYUYMT" runat="server"></asp:TextBox>                <!-- 請求月TO　 -->
            <asp:TextBox ID="WF_SEL_KEIJYODATEF" runat="server"></asp:TextBox>              <!-- 計上年月日FROM　 -->
            <asp:TextBox ID="WF_SEL_KEIJYODATET" runat="server"></asp:TextBox>              <!-- 計上年月日TO　 -->
            <asp:TextBox ID="WF_SEL_OILTYPE" runat="server"></asp:TextBox>                  <!-- 油種　 -->
            <asp:TextBox ID="WF_SEL_OILTYPE_NAME" runat="server"></asp:TextBox>             <!-- 油種名称　 -->
            <asp:TextBox ID="WF_SEL_MANGORG" runat="server"></asp:TextBox>                  <!-- 管理部署　 -->
            <asp:TextBox ID="WF_SEL_SHIPORG" runat="server"></asp:TextBox>                  <!-- 出荷部署　 -->
            <asp:TextBox ID="WF_SEL_SHIPORG_NAME" runat="server"></asp:TextBox>             <!-- 出荷部署名称　 -->
            <asp:TextBox ID="WF_SEL_TORICODE" runat="server"></asp:TextBox>                 <!-- 荷主　 -->
            <asp:TextBox ID="WF_SEL_SUPPLCAMP" runat="server"></asp:TextBox>                <!-- 庸車会社　 -->

            <asp:TextBox ID="WF_SEL_RESTART" runat="server"></asp:TextBox>                  <!-- 再開　 -->
            <asp:TextBox ID="WF_SEL_XMLsavePARM" runat="server"></asp:TextBox>              <!-- 抽出条件保存パス　 -->
            <asp:TextBox ID="WF_SEL_XMLsaveTmp" runat="server"></asp:TextBox>               <!-- 画面一覧保存パス　 -->

            <asp:TextBox ID="WF_SEL_INPTAB1TBL" runat="server"></asp:TextBox>               <!-- 画面一覧保存パスタブ１　 -->
            <asp:TextBox ID="WF_SEL_INPTAB2TBL" runat="server"></asp:TextBox>               <!-- 画面一覧保存パスタブ２　 -->
            <asp:TextBox ID="WF_SEL_INPTAB3TBL" runat="server"></asp:TextBox>               <!-- 画面一覧保存パスタブ３　 -->
            <asp:TextBox ID="WF_SEL_INPTAB4TBL" runat="server"></asp:TextBox>               <!-- 画面一覧保存パスタブ４　 -->

        </div>