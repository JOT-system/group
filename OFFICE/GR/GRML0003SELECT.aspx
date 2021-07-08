﻿<%@ Page Title="ML0003S" Language="vb" AutoEventWireup="false" CodeBehind="GRML0003SELECT.aspx.vb" Inherits="OFFICE.GRML0003SELECT" %>
<%@ MasterType VirtualPath="~/GR/GRMasterPage.Master" %> 

<%@ Import Namespace="OFFICE.GRIS0005LeftBox" %>

<%@ register src="~/inc/GRIS0003SRightBox.ascx" tagname="rightview" tagprefix="MSINC" %>
<%@ register src="~/inc/GRIS0005LeftBox.ascx" tagname="leftview" tagprefix="MSINC" %>

<%@ register src="inc/GRML0003WRKINC.ascx" tagname="work" tagprefix="LSINC" %>
<asp:Content ID="GRML0003SH" ContentPlaceHolderID="head" runat="server">
    <link rel="stylesheet" type="text/css" href="<%=ResolveUrl("~/GR/css/ML0003S.css")%>"/>
    <script type="text/javascript" src="<%=ResolveUrl("~/GR/script/ML0003S.js")%>"></script>
</asp:Content>

<asp:Content ID="GRML0003S" ContentPlaceHolderID="contents1" runat="server">
    <!-- 全体レイアウト　searchbox -->
    <div class="searchbox" id="searchbox">
        <!-- ○ 固定項目 ○ -->
        <a style="position:fixed; top:2.8em; left:62.5em;">
            <input type="button" id="WF_ButtonDO" value="実行" style="Width:5em;" onclick="ButtonClick('WF_ButtonDO');" />
        </a>
        <a style="position:fixed; top:2.8em; left:67em;">
            <input type="button" id="WF_ButtonEND" value="終了" style="Width:5em;" onclick="ButtonClick('WF_ButtonEND');" />
        </a>

        <!-- ○ 変動項目 ○ -->
        <!-- 会社コード -->
        <a style="position:fixed; top:7.7em; left:4em; font-weight:bold; text-decoration:underline;">会社コード</a>

        <a style="position:fixed; top:7.7em; left:18em;" ondblclick="Field_DBclick('WF_CAMPCODE', <%=LIST_BOX_CLASSIFICATION.LC_COMPANY%>);" onchange="TextBox_change('WF_CAMPCODE');">
            <asp:TextBox ID="WF_CAMPCODE" runat="server" MaxLength="20"　Height="1.4em" Width="11em" onblur="MsgClear();"></asp:TextBox>
        </a>
        <a style="position:fixed; top:7.7em; left:28em;">
            <asp:Label ID="WF_CAMPCODE_TEXT" runat="server" Width="17em" CssClass="WF_TEXT"></asp:Label>
        </a>

        <!-- 有効年月日 -->
        <a style="position:fixed; top:9.9em; left:4em; font-weight:bold; text-decoration:underline;">有効年月日</a>
        <a style="position:fixed; top:9.9em; left:11.5em;">範囲指定</a>
        <a style="position:fixed; top:9.9em; left:18em;" ondblclick="Field_DBclick('WF_STYMD', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>);">
            <asp:TextBox ID="WF_STYMD" runat="server" MaxLength="10" Height="1.4em" Width="11em" onblur="MsgClear();"></asp:TextBox>
        </a>
        <a style="position:fixed; top:9.9em; left:42.5em;">～</a>
        <a style="position:fixed; top:9.9em; left:44em;" ondblclick="Field_DBclick('WF_ENDYMD', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>);">
            <asp:TextBox ID="WF_ENDYMD" runat="server" MaxLength="10" Height="1.4em" Width="11em" onblur="MsgClear();"></asp:TextBox>
        </a>


        <!-- 仕訳パターン分類 -->
        <a style="position:fixed; top:12.1em; left:4em; font-weight:bold; text-decoration:underline;">仕訳パターン分類</a>
        <a style="position:fixed; top:12.1em; left:18em;" ondblclick="Field_DBclick('WF_SHIWAKEPATERNKBN', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('WF_SHIWAKEPATERNKBN');">
            <asp:TextBox ID="WF_SHIWAKEPATERNKBN" runat="server" MaxLength="20" Height="1.4em" Width="11em" onblur="MsgClear();"></asp:TextBox>
        </a>
        <a style="position:fixed; top:12.1em; left:28em;">
            <asp:Label ID="WF_SHIWAKEPATERNKBN_TEXT" runat="server" Width="17em" CssClass="WF_TEXT"></asp:Label>
        </a>


        <!-- 貸借区分 -->
        <!--
        <a style="position:fixed; top:14.3em; left:4em; font-weight:bold; text-decoration:underline;">貸借区分</a>
        <a style="position:fixed; top:14.3em; left:18em;" ondblclick="Field_DBclick('WF_ACDCKBN', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('WF_ACDCKBN');">
            <asp:TextBox ID="WF_ACDCKBN" runat="server" MaxLength="1" Height="1.4em" Width="11em" onblur="MsgClear();"></asp:TextBox>
        </a>
        <a style="position:fixed; top:14.3em; left:28em;">
            <asp:Label ID="WF_ACDCKBN_TEXT" runat="server" Width="17em" CssClass="WF_TEXT"></asp:Label>
        </a>
        -->

    </div>

    <a hidden="hidden">
        <input id="WF_FIELD"  runat="server" value=""  type="text" />          <!-- Textbox DBクリックフィールド -->
        <input id="WF_SelectedIndex"  runat="server" value=""  type="text" />  <!-- Textbox DBクリックフィールド -->

        <input id="WF_LeftboxOpen"  runat="server" value=""  type="text" />    <!-- Textbox DBクリックフィールド -->
        <input id="WF_LeftMViewChange" runat="server" value="" type="text"/>   <!-- Textbox DBクリックフィールド -->

        <input id="WF_RightViewChange" runat="server" value="" type="text"/>      <!-- Rightbox Mview切替 -->
        <input id="WF_RightboxOpen" runat="server" value=""  type="text" />       <!-- Rightbox 開閉 -->

        <input id="WF_ButtonClick" runat="server" value=""  type="text" />     <!-- ボタン押下 -->
    </a>

    <%-- rightview --%>
    <MSINC:rightview id="rightview" runat="server" />
    <%-- leftview --%>
    <MSINC:leftview id="leftview" runat="server" />
    <%-- Work --%>
    <LSINC:work id="work" runat="server" />
</asp:Content>
