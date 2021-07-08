<%@ Page Title="T00016S" Language="vb" AutoEventWireup="false" CodeBehind="GRT00016SELECT.aspx.vb" Inherits="OFFICE.GRT00016SELECT" %>
<%@ MasterType VirtualPath="~/GR/GRMasterPage.Master" %> 

<%@ Import Namespace="OFFICE.GRIS0005LeftBox" %>

<%@ register src="~/inc/GRIS0003SRightBox.ascx" tagname="rightview" tagprefix="MSINC" %>
<%@ register src="~/inc/GRIS0005LeftBox.ascx" tagname="leftview" tagprefix="MSINC" %>
<%@ register src="inc/GRT00016WRKINC.ascx" tagname="work" tagprefix="LSINC" %>

<asp:Content ID="T00016SH" ContentPlaceHolderID="head" runat="server">
    <link rel="stylesheet" type="text/css" href="<%=ResolveUrl("~/GR/css/T00016S.css")%>"/>
    <script type="text/javascript" src="<%=ResolveUrl("~/GR/script/T00016S.js")%>"></script>
</asp:Content> 
<asp:Content ID="T00016S" ContentPlaceHolderID="contents1" runat="server">
        <!-- 全体レイアウト　searchbox -->
        <div  class="searchbox" id="searchbox">
            <!-- ○ 固定項目 ○ -->
            <div id="searchbuttonbox" class="searchbuttonbox" >
                <!-- ■　ボタン　■ -->
                <a style="position:fixed;top:2.8em;left:62.5em;">
                    <input type="button" id="WF_ButtonDO" value="実行"  style="Width:5em" onclick="ButtonClick('WF_ButtonDO');" />
                </a>
                <a style="position:fixed;top:2.8em;left:67em;">
                    <input type="button" id="WF_ButtonEND" value="終了"  style="Width:5em" onclick="ButtonClick('WF_ButtonEND');" />
                </a>
            </div>
            <div id="searchkeybox" class="searchkeybox">
                <p class="LINE_1">
                    <!-- 　会社コード　 -->
                    <a style="position:fixed;top:7.7em;left:4em;font-weight:bold;text-decoration:underline">会社コード</a>
                    <a style="position:fixed;top:7.7em;left:11.5em;"></a>
                    <a style="position:fixed;top:7.7em;left:18em;" ondblclick="Field_DBclick('WF_CAMPCODE' ,  <%=LIST_BOX_CLASSIFICATION.LC_COMPANY  %>)" onchange="TextBox_change('WF_CAMPCODE')">
                        <asp:TextBox ID="WF_CAMPCODE" runat="server" Height="1.4em" Width="10em" onblur="MsgClear()"></asp:TextBox>
                    </a>
                    <a style="position:fixed;top:7.7em;left:27em;">
                        <asp:Label ID="WF_CAMPCODE_Text" runat="server" Text="" Width="17em" CssClass="WF_TEXT"></asp:Label>
                    </a>
                </p>
                <p class="LINE_2">
                    <!-- 　請求月　 -->
                    <a style="position:fixed;top:9.9em;left:4em;font-weight:bold;text-decoration:underline">請求月</a>
                    <%--<a style="position:fixed;top:9.9em;left:11.5em;">範囲指定</a>--%>
                    <a style="position:fixed;top:9.9em;left:18em;" ondblclick="Field_DBclick('WF_SEIKYUYMF', <%= LIST_BOX_CLASSIFICATION.LC_CALENDAR%>)">
                        <asp:TextBox ID="WF_SEIKYUYMF" runat="server" Height="1.4em" Width="10em" onblur="MsgClear()"></asp:TextBox>
                    </a>
                    <%--<a style="position:fixed;top:9.9em;left:42.5em;">～</a>--%>
                    <a style="position:fixed;top:9.9em;left:44em;" ondblclick="Field_DBclick('WF_SEIKYUYMT', <%= LIST_BOX_CLASSIFICATION.LC_CALENDAR%>)" hidden="hidden">
                        <asp:TextBox ID="WF_SEIKYUYMT" runat="server" Height="1.4em" Width="10em" onblur="MsgClear()" hidden="hidden"></asp:TextBox>
                    </a>
                </p>
<%--                <p class="LINE_3">
                    <a style="position:fixed;top:11em;left:4em;">or</a>
                </p>--%>
                <p class="LINE_4">
                    <!-- 　計上年月日　 -->
<%--                    <a style="position:fixed;top:12.1em;left:4em;font-weight:bold;text-decoration:underline">計上年月日</a>
                    <a style="position:fixed;top:12.1em;left:11.5em;">範囲指定</a>--%>
                    <a style="position:fixed;top:12.1em;left:18em;" ondblclick="Field_DBclick('WF_KEIJYODATEF', <%= LIST_BOX_CLASSIFICATION.LC_CALENDAR%>)" hidden="hidden">
                        <asp:TextBox ID="WF_KEIJYODATEF" runat="server" Height="1.4em" Width="10em" onblur="MsgClear()" hidden="hidden"></asp:TextBox>
                    </a>
                    <%--<a style="position:fixed;top:12.1em;left:42.5em;">～</a>--%>
                    <a style="position:fixed;top:12.1em;left:44em;"" ondblclick="Field_DBclick('WF_KEIJYODATET', <%= LIST_BOX_CLASSIFICATION.LC_CALENDAR%>)" hidden="hidden">
                        <asp:TextBox ID="WF_KEIJYODATET" runat="server" Height="1.4em" Width="10em" onblur="MsgClear()" hidden="hidden"></asp:TextBox>
                    </a>
                </p>
                <p class="LINE_5">
                    <!-- 　油種　 -->
                    <a style="position:fixed;top:12.1em;left:4em;font-weight:bold;text-decoration:underline">油種</a>
                    <a style="position:fixed;top:12.1em;left:11.5em;"></a>
                    <a style="position:fixed;top:12.1em;left:18em;" ondblclick="Field_DBclick('WF_OILTYPE' ,<%= LIST_BOX_CLASSIFICATION.LC_OILTYPE%>)" onchange="TextBox_change('WF_OILTYPE')">
                        <asp:TextBox ID="WF_OILTYPE" runat="server" Height="1.4em" Width="10em" onblur="MsgClear()"></asp:TextBox>
                    </a>
                    <a style="position:fixed;top:12.1em;left:27em;">
                        <asp:Label ID="WF_OILTYPE_Text" runat="server" Text="" Width="17em" CssClass="WF_TEXT"></asp:Label>
                    </a>
                </p>
                <p class="LINE_6">
                    <!-- 　管理部署　 -->
                    <a style="position:fixed;top:14.3em;left:4em;font-weight:bold;text-decoration:underline">管理部署</a>
                    <a style="position:fixed;top:14.3em;left:11.5em;"></a>
                    <a style="position:fixed;top:14.3em;left:18em;" ondblclick="Field_DBclick('WF_MANGORG' ,<%= LIST_BOX_CLASSIFICATION.LC_ORG%>)" onchange="TextBox_change('WF_MANGORG')">
                        <asp:TextBox ID="WF_MANGORG" runat="server" Height="1.4em" Width="10em" onblur="MsgClear()"></asp:TextBox>
                    </a>
                    <a style="position:fixed;top:14.3em;left:27em;">
                        <asp:Label ID="WF_MANGORG_Text" runat="server" Text="" Width="17em" CssClass="WF_TEXT"></asp:Label>
                    </a>
                </p>
                <p class="LINE_7">
                    <!-- 　出荷部署　 -->
                    <a style="position:fixed;top:16.5em;left:4em;font-weight:bold;text-decoration:underline">出荷部署</a>
                    <a style="position:fixed;top:16.5em;left:11.5em;"></a>
                    <a style="position:fixed;top:16.5em;left:18em;" ondblclick="Field_DBclick('WF_SHIPORG' ,<%= LIST_BOX_CLASSIFICATION.LC_ORG%>)" onchange="TextBox_change('WF_SHIPORG')">
                        <asp:TextBox ID="WF_SHIPORG" runat="server" Height="1.4em" Width="10em" onblur="MsgClear()"></asp:TextBox>
                    </a>
                    <a style="position:fixed;top:16.5em;left:27em;">
                        <asp:Label ID="WF_SHIPORG_Text" runat="server" Text="" Width="17em" CssClass="WF_TEXT"></asp:Label>
                    </a>
                </p>
                <p class="LINE_8">
                    <!-- 　荷主　 -->
                    <a style="position:fixed;top:18.7em;left:4em;font-weight:bold;text-decoration:underline">荷主</a>
                    <a style="position:fixed;top:18.7em;left:11.5em;"></a>
                    <a style="position:fixed;top:18.7em;left:18em;" ondblclick="Field_DBclick('WF_TORICODE' ,<%= LIST_BOX_CLASSIFICATION.LC_CUSTOMER%>)" onchange="TextBox_change('WF_TORICODE')">
                        <asp:TextBox ID="WF_TORICODE" runat="server" Height="1.4em" Width="10em" onblur="MsgClear()"></asp:TextBox>
                    </a>
                    <a style="position:fixed;top:18.7em;left:27em;">
                        <asp:Label ID="WF_TORICODE_Text" runat="server" Text="" Width="17em" CssClass="WF_TEXT"></asp:Label>
                    </a>
                </p>
                <p class="LINE_9">
                    <!-- 　庸車会社　 -->
                    <a style="position:fixed;top:20.9em;left:4em;font-weight:bold;text-decoration:underline">庸車会社</a>
                    <a style="position:fixed;top:20.9em;left:11.5em;"></a>
                    <a style="position:fixed;top:20.9em;left:18em;" ondblclick="Field_DBclick('WF_SUPPLCAMP' ,<%= LIST_BOX_CLASSIFICATION.LC_CUSTOMER%>)" onchange="TextBox_change('WF_SUPPLCAMP')">
                        <asp:TextBox ID="WF_SUPPLCAMP" runat="server" Height="1.4em" Width="10em" onblur="MsgClear()"></asp:TextBox>
                    </a>
                    <a style="position:fixed;top:20.9em;left:27em;">
                        <asp:Label ID="WF_SUPPLCAMP_Text" runat="server" Text="" Width="17em" CssClass="WF_TEXT"></asp:Label>
                    </a>
                </p>
            </div> 
 
            <a hidden="hidden">
                <input id="WF_FIELD"  runat="server" value=""  type="text" />          <!-- Textbox DBクリックフィールド -->
                <input id="WF_SelectedIndex"  runat="server" value=""  type="text" />  <!-- Textbox DBクリックフィールド -->

                <input id="WF_LeftboxOpen"  runat="server" value=""  type="text" />    <!-- Textbox DBクリックフィールド -->
                <input id="WF_LeftMViewChange" runat="server" value="" type="text"/>   <!-- Textbox DBクリックフィールド -->

                <input id="WF_RightViewChange" runat="server" value="" type="text"/>   <!-- Rightbox Mview切替 -->
                <input id="WF_RightboxOpen" runat="server" value=""  type="text" />    <!-- Rightbox 開閉 -->

                <input id="WF_ButtonClick" runat="server" value=""  type="text" />     <!-- ボタン押下 -->

                <input id="WF_Restart" runat="server" value=""  type="text" />         <!-- 一時保管 -->
            </a>
        </div>
        <%-- rightview --%>
        <MSINC:rightview id="rightview" runat="server" />
        <%-- leftview --%>
        <MSINC:leftview id="leftview" runat="server" />
        <%-- Work --%>
        <LSINC:work id="work" runat="server" />
</asp:Content>
