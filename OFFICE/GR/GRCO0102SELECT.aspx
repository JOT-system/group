﻿<%@ Page Title="CO0102S" Language="vb" AutoEventWireup="false" CodeBehind="GRCO0102SELECT.aspx.vb" Inherits="OFFICE.GRCO0102SELECT" %>
<%@ MasterType VirtualPath="~/GR/GRMasterPage.Master" %>

<%@ Import Namespace="OFFICE.GRIS0005LeftBox" %>

<%@ Register Src="~/inc/GRIS0003SRightBox.ascx" TagName="rightview" TagPrefix="MSINC" %>
<%@ Register Src="~/inc/GRIS0005LeftBox.ascx" TagName="leftview" TagPrefix="MSINC" %>
<%@ Register Src="~/GR/inc/GRCO0102WRKINC.ascx" TagName="wrklist" TagPrefix="MSINC" %>

<asp:Content ID="CO0102SH" ContentPlaceHolderID="head" runat="server">
    <link href='<%=ResolveUrl("~/GR/css/CO0102S.css")%>' rel="stylesheet" type="text/css" />
    <script type="text/javascript" src='<%=ResolveUrl("~/GR/script/CO0102S.js")%>'></script>
</asp:Content>

<asp:Content ID="CO0102S" ContentPlaceHolderID="contents1" runat="server">
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
            <asp:TextBox ID="WF_CAMPCODE" runat="server" Height="1.4em" Width="10em" onblur="MsgClear();"></asp:TextBox>
        </a>
        <a style="position:fixed; top:7.7em; left:27em;">
            <asp:Label ID="WF_CAMPCODE_TEXT" runat="server" Width="17em" CssClass="WF_TEXT"></asp:Label>
        </a>

        <!-- 画面ID -->
        <a style="position:fixed; top:9.9em; left:4em; font-weight:bold; text-decoration:underline;">画面ID</a>
        <a style="position:fixed; top:9.9em; left:11.5em;">範囲指定</a>
        <a style="position:fixed; top:9.9em; left:18em;" ondblclick="Field_DBclick('WF_MAPIDF', <%=LIST_BOX_CLASSIFICATION.LC_EXTRA_LIST%>);" onchange="TextBox_change('WF_MAPIDF');">
            <asp:TextBox ID="WF_MAPIDF" runat="server" Height="1.4em" Width="10em" onblur="MsgClear();"></asp:TextBox>
        </a>
        <a style="position:fixed; top:9.9em; left:27em;">
            <asp:Label ID="WF_MAPIDF_TEXT" runat="server" Width="17em" CssClass="WF_TEXT"></asp:Label>
        </a>
        <a style="position:fixed; top:9.9em; left:42.5em;">～</a>
        <a style="position:fixed; top:9.9em; left:44em;" ondblclick="Field_DBclick('WF_MAPIDT', <%=LIST_BOX_CLASSIFICATION.LC_EXTRA_LIST%>);" onchange="TextBox_change('WF_MAPIDT');">
            <asp:TextBox ID="WF_MAPIDT" runat="server" Height="1.4em" Width="10em" onblur="MsgClear();"></asp:TextBox>
        </a>
        <a style="position:fixed; top:9.9em; left:53em;">
            <asp:Label ID="WF_MAPIDT_TEXT" runat="server" Width="17em" CssClass="WF_TEXT"></asp:Label>
        </a>
                <!-- 機能選択 -->
        <a style="position:fixed; top:12.1em; left:4em; font-weight:bold; text-decoration:underline;">機能選択</a>
        <a style="position:fixed; top:12.1em; left:18em;">
            <span style="position:fixed; top:12.1em; left:18em;" >
                <asp:RadioButton ID="WF_right_SW1" runat="server" GroupName="rightbox" Text="Default表示あり" Width="20em" Checked="true" />
            </span>
            <span style="position:fixed; top:13.6em; left:18em;">
                <asp:RadioButton ID="WF_right_SW2" runat="server" GroupName="rightbox" Text="Default表示なし" Width="20em"/>
            </span>
        </a>
    </div>

    <!-- rightbox レイアウト -->
    <MSINC:rightview id="rightview" runat="server" />

    <!-- leftbox レイアウト -->
    <MSINC:leftview id="leftview" runat="server" />

    <!-- Work レイアウト -->
    <MSINC:wrklist id="work" runat="server" />

    <!-- イベント用 -->
    <div hidden="hidden">
        <input id="WF_FIELD" runat="server" value="" type="text" />                 <!-- Textbox DBクリックフィールド -->
        <input id="WF_SelectedIndex" runat="server" value="" type="text" />         <!-- Textbox DBクリックフィールド -->
        <input id="WF_LeftboxOpen" runat="server" value="" type="text" />           <!-- LeftBox 開閉 -->
        <input id="WF_RightboxOpen" runat="server" value="" type="text" />          <!-- Rightbox 開閉 -->
        <input id="WF_LeftMViewChange" runat="server" value="" type="text" />       <!-- LeftBox Mview切替 -->
        <input id="WF_ButtonClick" runat="server" value="" type="text" />           <!-- ボタン押下 -->
    </div>
</asp:Content>
