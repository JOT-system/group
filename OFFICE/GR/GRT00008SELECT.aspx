﻿<%@ Page Title="T00008S" Language="vb" AutoEventWireup="false" CodeBehind="GRT00008SELECT.aspx.vb" Inherits="OFFICE.GRT00008SELECT" %>
<%@ MasterType VirtualPath="~/GR/GRMasterPage.Master" %> 

<%@ Import Namespace="OFFICE.GRIS0005LeftBox" %>

<%@ register src="~/inc/GRIS0003SRightBox.ascx" tagname="rightview" tagprefix="MSINC" %>
<%@ register src="~/inc/GRIS0005LeftBox.ascx" tagname="leftview" tagprefix="MSINC" %>
<%@ register src="inc/GRT00008WRKINC.ascx" tagname="work" tagprefix="LSINC" %>

<asp:Content ID="T00008SH" ContentPlaceHolderID="head" runat="server">
    <link rel="stylesheet" type="text/css" href="<%=ResolveUrl("~/GR/css/T00008S.css")%>"/>
    <script type="text/javascript" src="<%=ResolveUrl("~/GR/script/T00008S.js")%>"></script>
</asp:Content> 
<asp:Content ID="T00008S" ContentPlaceHolderID="contents1" runat="server">
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
                    <a style="position:fixed;top:7.7em;left:18em;" ondblclick="Field_DBclick('WF_CAMPCODE' ,<%= LIST_BOX_CLASSIFICATION.LC_COMPANY%>)" onchange="TextBox_change('WF_CAMPCODE')">
                        <asp:TextBox ID="WF_CAMPCODE" runat="server" Height="1.4em" Width="10em" onblur="MsgClear()"></asp:TextBox>
                    </a>
                    <a style="position:fixed;top:7.7em;left:27em;">
                        <asp:Label ID="WF_CAMPCODE_Text" runat="server" Text="" Width="17em" CssClass="WF_TEXT"></asp:Label>
                    </a>
                </p>
                <p class="LINE_2">
                    <!-- 　締年月　 -->
                    <a style="position:fixed;top:9.9em;left:4em;font-weight:bold;text-decoration:underline">締年月</a>
                    <a style="position:fixed;top:9.9em;left:11.5em;"></a>
                    <a style="position:fixed;top:9.9em;left:18em;" ondblclick="Field_DBclick('WF_LIMITYM', <%= LIST_BOX_CLASSIFICATION.LC_CALENDAR%>)">
                        <asp:TextBox ID="WF_LIMITYM" runat="server" Height="1.4em" Width="10em" onblur="MsgClear()"></asp:TextBox>
                    </a>
                </p>
                <p class="LINE_3">
                    <!-- 　部署コード　 -->
                    <a style="position:fixed;top:12.1em;left:4em;font-weight:bold;text-decoration:underline">部署コード</a>
                    <a style="position:fixed;top:12.1em;left:11.5em;"></a>
                    <a style="position:fixed;top:12.1em;left:18em;" ondblclick="Field_DBclick('WF_ORGCODE' ,<%= LIST_BOX_CLASSIFICATION.LC_EXTRA_LIST %>)" onchange="TextBox_change('WF_ORGCODE')">
                        <asp:TextBox ID="WF_ORGCODE" runat="server" Height="1.4em" Width="10em" onblur="MsgClear()"></asp:TextBox>
                    </a>
                    <a style="position:fixed;top:12.1em;left:27em;">
                        <asp:Label ID="WF_ORGCODE_Text" runat="server" Text="" Width="17em" CssClass="WF_TEXT"></asp:Label>
                    </a>
                </p>
            </div> 
 
            <a hidden="hidden">
                <input id="WF_FIELD"  runat="server" value=""  type="text" />          <!-- Textbox DBクリックフィールド -->
                <input id="WF_SelectedIndex"  runat="server" value=""  type="text" />  <!-- Textbox DBクリックフィールド -->

                <input id="WF_LeftMViewChange" runat="server" value="" type="text"/>   <!-- Leftbox Mview切替 -->
                <input id="WF_LeftboxOpen"  runat="server" value=""  type="text" />    <!-- Leftbox 開閉 -->

                <input id="WF_RightViewChange" runat="server" value="" type="text"/>   <!-- Rightbox Mview切替 -->
                <input id="WF_RightboxOpen" runat="server" value=""  type="text" />    <!-- Rightbox 開閉 -->

                <input id="WF_ButtonClick" runat="server" value=""  type="text" />     <!-- ボタン押下 -->
            </a>
        </div>
        <%-- rightview --%>
        <MSINC:rightview id="rightview" runat="server" />
        <%-- leftview --%>
        <MSINC:leftview id="leftview" runat="server" />
        <%-- Work --%>
        <LSINC:work id="work" runat="server" />
</asp:Content>
