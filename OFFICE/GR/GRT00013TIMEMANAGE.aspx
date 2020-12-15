<%@ Page Title="T00013" Language="vb" AutoEventWireup="false" CodeBehind="GRT00013TIMEMANAGE.aspx.vb" Inherits="OFFICE.GRT00013TIMEMANAGE" %>
<%@ MasterType VirtualPath="~/GR/GRMasterPage.Master" %>

<%@ Import Namespace="OFFICE.GRIS0005LeftBox" %>

<%@ Register Src="~/inc/GRIS0004RightBox.ascx" TagName="rightview" TagPrefix="MSINC" %>
<%@ Register Src="~/inc/GRIS0005LeftBox.ascx" TagName="leftview" TagPrefix="MSINC" %>
<%@ Register Src="~/GR/inc/GRT00013WRKINC.ascx" TagName="wrklist" TagPrefix="MSINC" %>

<asp:Content ID="T00013H" ContentPlaceHolderID="head" runat="server">
    <link href='<%=ResolveUrl("~/GR/css/T00013.css")%>' rel="stylesheet" type="text/css" />
    <script type="text/javascript" src='<%=ResolveUrl("~/GR/script/T00013.js")%>'></script>
    <script type="text/javascript">
        var pnlListAreaId = '<%=Me.pnlListArea.ClientID%>';
        var IsPostBack = '<%=If(IsPostBack = True, "1", "0")%>';
        var EXTRALIST = '<%=LIST_BOX_CLASSIFICATION.LC_EXTRA_LIST%>';
    </script>
</asp:Content>

<asp:Content ID="T00013" ContentPlaceHolderID="contents1" runat="server">
    <!-- 全体レイアウト　headerbox -->
    <div class="headerboxOnly" id="headerbox">
        <div class="Operation">
            <!-- 絞込従業員 -->
            <a>
                <asp:Label ID="WF_SELSTAFFCODE_L" runat="server" Text="従業員" Height="1.5em" Font-Bold="true" Font-Underline="true"></asp:Label>
            </a>
            <a ondblclick="Field_DBclick('WF_SELSTAFFCODE', <%=LIST_BOX_CLASSIFICATION.LC_STAFFCODE%>)">
                <asp:TextBox ID="WF_SELSTAFFCODE" runat="server" Height="1.1em" Width="7em" CssClass="WF_TEXTBOX_CSS" BorderStyle="NotSet"></asp:TextBox>
            </a>
            <a>
                <asp:Label ID="WF_SELSTAFFCODE_TEXT" runat="server" Width="30em" CssClass="WF_TEXT"></asp:Label>
            </a>

            <!-- ボタン -->
            <a style="position:fixed; top:2.8em; left:30em;">
                <input type="button" id="WF_ButtonDOWN" value="前頁" style="Width:5em" onclick="ButtonClick('WF_ButtonDOWN');" />
            </a>
            <a style="position:fixed; top:2.8em; left:34.5em;">
                <input type="button" id="WF_ButtonUP" value="次頁" style="Width:5em" onclick="ButtonClick('WF_ButtonUP');" />
            </a>
            <a style="position:fixed; top:2.8em; left:42.5em;">
                <input type="button" id="WF_ButtonSAVE" value="一時保存" style="Width:5em" onclick="ButtonClick('WF_ButtonSAVE');" />
            </a>

            <a style="position:fixed; top:2.8em; left:49em;">
                <input type="button" id="WF_ButtonExtract" value="絞り込み" style="Width:5em" onclick="ButtonClick('WF_ButtonExtract');" />
            </a>
            <a style="position:fixed; top:2.8em; left:53.5em;">
                <input type="button" id="WF_ButtonUPDATE" value="DB更新" style="Width:5em" onclick="ButtonClick('WF_ButtonUPDATE');" />
            </a>
            <a style="position:fixed; top:2.8em; left:58em;">
                <input type="button" id="WF_ButtonCSV" value="ﾀﾞｳﾝﾛｰﾄﾞ" style="Width:5em" onclick="ButtonClick('WF_ButtonCSV');" />
            </a>
            <a style="position:fixed; top:2.8em; left:62.5em;">
                <input type="button" id="WF_ButtonPrint" value="一覧印刷" style="Width:5em" onclick="ButtonClick('WF_ButtonPrint');" />
            </a>
            <a style="position:fixed; top:2.8em; left:67em;">
                <input type="button" id="WF_ButtonEND" value="終了" style="Width:5em" onclick="ButtonClick('WF_ButtonEND');" />
            </a>
        </div>
        
        <div id="detailkeybox">
            <p>
                <!-- 対象年月 -->
                <a>
                    <asp:Label ID="WF_TAISHOYM_L" runat="server" Text="対象年月" Width="4em" CssClass="WF_TEXT_LEFT"></asp:Label>
                    <asp:Label ID="WF_TAISHOYM" runat="server" Width="4em" CssClass="WF_TEXT_LEFT" Font-Bold="true"></asp:Label>
                </a>

                <!-- 従業員 -->
                <a>
                    <asp:Label ID="WF_STAFFCODE_L" runat="server" Text="従業員" Width="4em" CssClass="WF_TEXT_LEFT"></asp:Label>
                    <asp:Label ID="WF_STAFFCODE" runat="server" Width="4em" CssClass="WF_TEXT_LEFT" Font-Bold="true"></asp:Label>
                    <asp:Label ID="WF_STAFFCODE_TEXT" runat="server" Width="12em" CssClass="WF_TEXT"></asp:Label>
                </a>

                <!-- 配属部署 -->
                <a>
                    <asp:Label ID="WF_HORG_L" runat="server" Text="配属部署" Width="4em" CssClass="WF_TEXT_LEFT"></asp:Label>
                    <asp:Label ID="WF_HORG" runat="server" Width="4em" CssClass="WF_TEXT_LEFT" Font-Bold="true"></asp:Label>
                    <asp:Label ID="WF_HORG_TEXT" runat="server" Width="12em" CssClass="WF_TEXT"></asp:Label>
                </a>

                <!-- インフォメーション -->
                <a>
                    <asp:Label ID="WF_INFO" runat="server" Text="" Width="30em" CssClass="WF_TEXT_LEFT" ForeColor="Red" Font-Bold="true"></asp:Label>
                </a>
            </p>
        </div>
        
        <!-- 明細行 -->
        <div id="divListArea">
            <asp:Panel id="pnlListArea" runat="server"></asp:Panel>
        </div>
    </div>

    <!-- rightbox レイアウト -->
    <MSINC:rightview id="rightview" runat="server" />

    <!-- leftbox レイアウト -->
    <MSINC:leftview id="leftview" runat="server" />

    <!-- Work レイアウト -->
    <MSINC:wrklist id="work" runat="server" />

    <!-- イベント用 -->
    <div hidden="hidden">
        <asp:TextBox ID="WF_GridDBclick" Text="" runat="server"></asp:TextBox>      <!-- GridView DBクリック-->

        <input id="WF_FIELD" runat="server" value="" type="text" />                 <!-- Textbox DBクリックフィールド -->
        <input id="WF_SelectedIndex" runat="server" value="" type="text" />         <!-- Textbox DBクリックフィールド -->
        
        <input id="WF_DISP_SaveX" runat="server" value="" type="text" />            <!-- 明細位置X軸 -->
        <input id="WF_DISP_SaveY" runat="server" value="" type="text" />            <!-- 明細位置Y軸 -->
        
        <input id="WF_LeftMViewChange" runat="server" value="" type="text" />       <!-- LeftBox Mview切替 -->
        <input id="WF_LeftboxOpen" runat="server" value="" type="text" />           <!-- LeftBox 開閉 -->
        <input id="WF_RightViewChange" runat="server" value="" type="text" />       <!-- Rightbox Mview切替 -->
        <input id="WF_RightboxOpen" runat="server" value="" type="text" />          <!-- Rightbox 開閉 -->

        <input id="WF_PrintURL" runat="server" value="" type="text" />              <!-- Textbox Print URL -->
        
        <input id="WF_XMLsaveF" runat="server" value="" type="text" />              <!-- 保存先TblURL -->
        <input id="WF_XMLsaveF_INP" runat="server" value="" type="text" />          <!-- 保存先INPTblURL -->
        <input id="WF_ButtonClick" runat="server" value="" type="text" />           <!-- ボタン押下 -->
        <input id="WF_MAPpermitcode" runat="server" value="" type="text" />         <!-- 権限 -->
    </div>
</asp:Content>
