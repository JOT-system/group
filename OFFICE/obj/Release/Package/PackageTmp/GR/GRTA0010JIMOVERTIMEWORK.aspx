<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="GRTA0010JIMOVERTIMEWORK.aspx.vb" Inherits="OFFICE.GRTA0010JIMOVERTIMEWORK" %>
<%@ MasterType VirtualPath="~/GR/GRMasterPage.Master" %>

<%@ register src="~/inc/GRIS0004RightBox.ascx" tagname="rightview" tagprefix="MSINC" %>

<%@ register src="inc/GRTA0010WRKINC.ascx" tagname="work" tagprefix="LSINC" %>
<asp:Content ID="GRTA0010H" ContentPlaceHolderID="head" runat="server">
    <link rel="stylesheet" type="text/css" href="<%=ResolveUrl("~/GR/css/TA0010.css")%>"/>
    <script type="text/javascript">
        var pnlListAreaId = '<%= Me.pnlListArea.ClientId %>';
        var IsPostBack = '<%= if(IsPostBack = True, "1", "0") %>';
    </script>
    <script type="text/javascript" src="<%=ResolveUrl("~/GR/script/TA0010.js")%>"></script>
</asp:Content>
<asp:Content ID="GRTA0010" ContentPlaceHolderID="contents1" runat="server">
    <!-- 全体レイアウト　headerbox -->
    <div  class="headerboxOnly" id="headerbox">
        <!-- ■　ボタン　■ -->
        <div class="Operation">
            <!-- 　対象年月　 -->
            <a  style="position:fixed;top:2.8em;left:1em;font-weight:bold;">
                <span style="text-align:right">
                    <asp:Label ID="WF_Year" runat="server" Text="" Width="2.5em" CssClass="WF_TEXT_TITLE"></asp:Label>
                </span>
                <asp:Label ID="Label1" runat="server" Text="年" Width="1.5em" CssClass="WF_TEXT_TITLE"></asp:Label>
                <span style="text-align:right">
                    <asp:Label ID="WF_Month" runat="server" Text="" Width="1.5em" CssClass="WF_TEXT_TITLE"></asp:Label>
                </span>
                <asp:Label ID="Label2" runat="server" Text="月度" Width="2.5em" CssClass="WF_TEXT_TITLE"></asp:Label>
            </a>
            <a style="position:fixed;top:2.8em;left:62.5em;">
                <input type="button" id="WF_ButtonXLS" value="Excel取得" style="Width:5em" onclick="ButtonClick('WF_ButtonXLS');" />
            </a>
            <a style="position:fixed;top:2.8em;left:67em;">
                <input type="button" id="WF_ButtonEND" value="終了"  style="Width:5em" onclick="ButtonClick('WF_ButtonEND');" />
            </a>
            <a style="position:fixed;top:3.0em;left:75em;">
                <asp:Image ID="WF_ButtonFIRST" runat="server" ImageUrl="~/先頭頁.png" Width="1.5em" Height="1em" ImageAlign="AbsMiddle" onclick="ButtonClick('WF_ButtonFIRST');" />
            </a>
            <a style="position:fixed;top:3.0em;left:77em;">
                <asp:Image ID="WF_ButtonLAST" runat="server" ImageUrl="~/最終頁.png" Width="1.5em" Height="1em" ImageAlign="AbsMiddle" onclick="ButtonClick('WF_ButtonLAST');" />
            </a>
        </div>
        <div class="leftMenubox">
            <!-- ■　照会選択タイトル　■ -->
            <div style="overflow-y:auto;height:1.5em;width:11.3em;text-align:left;vertical-align:middle;color:white;background-color:rgb(22,54,92);font-weight:bold;border: solid black;border-width:1.5px;">
                <a style="overflow:hidden;text-align:left;">
                    <asp:Label ID="Label3" runat="server" GroupName="selector" Text="　組織選択" Width="8em" />
                </a>
            </div>
            <%-- ■　照会選択項目表示　■ --%>
            <asp:MultiView ID="WF_SelectorMView" runat="server">
                <asp:View ID="WF_DView1" runat="server">
                    <!-- ■　組織セレクター　■ -->
                    <div id="ORGSelect" style="overflow-y:auto;width:11.3em;height:30em;color:black;background-color: white;border: solid;border-width:1.5px;">
                        <asp:Repeater ID="WF_ORGselector" runat="server">
                            <HeaderTemplate>
                                <table style="border-width:1px;margin:0.1em 0.1em 0.1em 0.1em;">
                            </HeaderTemplate>
                            <ItemTemplate>
                                <tr>
                                    <td hidden="hidden">
                                        <asp:Label ID="WF_SELorg_VALUE" runat="server"></asp:Label>
                                    </td>
                                    <td>
                                        <asp:Label ID="WF_SELorg_TEXT" runat="server" Text="" Width="11.3em" CssClass="WF_TEXT_LEFT"></asp:Label>
                                    </td>
                                </tr>
                            </ItemTemplate>
                            <FooterTemplate>
                                </table>
                            </FooterTemplate>
                        </asp:Repeater>
                    </div>
                </asp:View>
            </asp:MultiView>
        </div>
        <!-- 一覧レイアウト -->
        <div id="divListArea">
            <asp:panel id="pnlListArea" runat="server" ></asp:panel>
        </div>
    </div>
    <!-- 全体レイアウト　detailbox -->
    <div  class="detailbox" id="detailbox" hidden="hidden"></div>
    <div hidden="hidden">
        <asp:TextBox ID="WF_GridPosition" Text="" runat="server"></asp:TextBox>     <!-- GridView表示位置フィールド -->
        <input id="WF_RightViewChange" runat="server" value="" type="text"/>        <!-- Rightbox Mview切替 -->
        <input id="WF_RightboxOpen" runat="server" value=""  type="text" />         <!-- Rightbox 開閉 -->
        
        <input id="WF_PrintURL" runat="server" value=""  type="text" />              <!-- Textbox Print URL -->
        <input id="WF_ButtonClick" runat="server" value=""  type="text" />          <!-- ボタン押下 -->
        
        <input id="WF_SaveSX"  runat="server" value=""  type="text" />              <!-- セレクタ 変更位置X軸 -->
        <input id="WF_SaveSY"  runat="server" value=""  type="text" />              <!-- セレクタ 変更位置Y軸 -->

        <input id="WF_SELECTOR_SW" runat="server" value=""  type="text" />          <!-- セレクタの選択値 -->
        <input id="WF_SELECTOR_PosiORG" runat="server" value=""  type="text" />     <!-- セレクタの選択値（部署選択行）-->
        <input id="WF_SELECTOR_Chg" runat="server" value=""  type="text" />         <!-- セレクタの選択値（ラジオボタン） -->
        <input id="WF_SelectedIndex" runat="server" value="" type="text"/>          <!-- Textbox DBクリックフィールド -->
        <input id="WF_FIELD"  runat="server" value=""  type="text" />               <!-- Textbox DBクリックフィールド -->
    </div>
    <%-- rightview --%>
    <MSINC:rightview id="rightview" runat="server" />
    <%-- Work --%>
    <LSINC:work id="work" runat="server" />
</asp:Content>
