<%@ Page Title="M00001" Language="vb" AutoEventWireup="true" CodeBehind="M00001MENU.aspx.vb" Inherits="OFFICE.M00001MENU"  %>
<%@ MasterType VirtualPath="~/GR/GRMasterPage.Master" %> 

<%@ register src="inc/GRM00001WRKINC.ascx" tagname="work" tagprefix="LSINC" %>

<asp:Content ID="MC0001H" ContentPlaceHolderID="head" runat="server">

    <link rel="stylesheet" type="text/css" href="<%=ResolveUrl("~/css/M00001.css")%>"/>
    <script type="text/javascript" src="<%=ResolveUrl("~/script/M00001.js")%>"></script>

</asp:Content> 
<asp:Content ID="MC0001" ContentPlaceHolderID="contents1" runat="server">

        <!-- 全体レイアウト　Menuheaderbox -->
        <div  class="Menuheaderbox" id="Menuheaderbox">

          <a  class="Menu_L" id="Menu_L"  >
            <asp:Repeater ID="Repeater_Menu_L" runat="server" >
                <HeaderTemplate>
                    <table>
                </HeaderTemplate>
                <ItemTemplate>

                    <tr>
                        <td >
                            <asp:Label ID="WF_MenuLabe_L" runat="server" CssClass="WF_MenuLabel_L"></asp:Label>
                            <asp:Label ID="WF_MenuURL_L" runat="server" Visible="False"></asp:Label>
                            <asp:Label ID="WF_MenuVARI_L" runat="server" Visible="False"></asp:Label>
                            <asp:Label ID="WF_MenuMAP_L" runat="server" Visible="False"></asp:Label>
                            <asp:Button ID="WF_MenuButton_L" runat="server" CssClass="WF_MenuButton_L" onmouseover="this.style.background='blue';this.style.color='white'" onmouseout="this.style.background='gray';this.style.color='black'" OnClientClick="commonDispWait();"/> 
                        </td>
                    </tr>

                </ItemTemplate>
                <FooterTemplate>
                    </table>
                </FooterTemplate>
             
            </asp:Repeater>
          </a>

          <a class="Menu_R" id="Menu_R" >
            <asp:Repeater ID="Repeater_Menu_R" runat="server" >
                <HeaderTemplate>
                    <table>
                </HeaderTemplate>
                <ItemTemplate>
                    <tr>
                        <td >
                            <asp:Label ID="WF_MenuLabe_R" runat="server" CssClass="WF_MenuLabel_R"></asp:Label>
                            <asp:Label ID="WF_MenuURL_R" runat="server"  Visible="False" ></asp:Label>
                            <asp:Label ID="WF_MenuVARI_R" runat="server"  Visible="False" ></asp:Label>
                            <asp:Label ID="WF_MenuMAP_R" runat="server" Visible="False"></asp:Label>
                            <asp:Button ID="WF_MenuButton_R" runat="server" CssClass="WF_MenuButton_R" onmouseover="this.style.background='blue';this.style.color='white'" onmouseout="this.style.background='gray';this.style.color='black'" OnClientClick="commonDispWait();"/> 
                        </td>
                    </tr>
                </ItemTemplate>

                <FooterTemplate>
                    </table>
                </FooterTemplate>
             
            </asp:Repeater>
          </a>

          <a hidden="hidden">
              <input id="WF_ButtonClick" runat="server" value=""  type="text" />        <!-- ボタン押下 -->
              <asp:TextBox ID="WF_TERMID" runat="server"></asp:TextBox>               <!-- 端末ID　 -->
              <asp:TextBox ID="WF_TERMCAMP" runat="server"></asp:TextBox>             <!-- 端末会社　 -->
          </a>
        </div>
        <div id="msgbox" class="msgbox">
            <table id="warnningbox" class="warnningbox" style="width:100%">
                <tr>
                    <td style="width:2em;">
                        <input type="button" id="WF_WARNNING" class="ZoomBtn" runat="server" value="◀ 車検、気密検査、容器検査" onclick="OpenClose();"/>
                    </td>
                    <td style="width:100%;text-align:right" >
                        <input type="button" class="UpdBtn" id="Button1" runat="server" value="🔄更新" arighn="right" onclick="ButtonClick('WF_WARNNING');" />
                    </td>
                </tr>
            </table>
            <div id="guidancebox" class="guidancebox">
                <span>
                    <asp:Label ID="WF_Guidance" runat="server" Text=""></asp:Label><br />
                </span>
            </div> 
            <table id="guidbox" class="guidbox" style="width:100%">
                <tr>
                    <td style="width:2em;">
                        <input type="button" id="WF_GUID" class="ZoomBtn" runat="server" value="◀ 運用ガイダンス" onclick="OpenClose();"/>
                    </td>
                    <td style="width:100%;text-align:right">
                        <input type="button" class="UpdBtn" id="Button2" runat="server" value="🔄更新" arighn="right" onclick="ButtonClick('WF_GUID');" />
                    </td>
                </tr>
            </table> 
            <div id="onlinestatbox" class="onlinestatbox">
                <span>
                    <asp:Label ID="WF_OnlineStat" runat="server" Text=""></asp:Label><br />
                </span>
            </div> 
        </div>
            <!-- Work レイアウト -->
        <LSINC:work id="work" runat="server" />

</asp:Content> 