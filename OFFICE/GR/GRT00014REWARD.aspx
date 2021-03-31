<%@ Page Title="T00014" Language="vb" AutoEventWireup="false" CodeBehind="GRT00014REWARD.aspx.vb" Inherits="OFFICE.GRT00014REWARD" %>
<%@ MasterType VirtualPath="~/GR/GRMasterPage.Master" %>

<%@ Import Namespace="OFFICE.GRIS0005LeftBox" %>

<%@ Register Src="~/inc/GRIS0004RightBox.ascx" TagName="rightview" TagPrefix="MSINC" %>
<%@ Register Src="~/inc/GRIS0005LeftBox.ascx" TagName="leftview" TagPrefix="MSINC" %>
<%@ Register Src="~/GR/inc/GRT00014WRKINC.ascx" TagName="wrklist" TagPrefix="MSINC" %>

<asp:Content ID="T00014H" ContentPlaceHolderID="head" runat="server">
    <link href='<%=ResolveUrl("~/GR/css/T00014.css")%>' rel="stylesheet" type="text/css" />
    <script type="text/javascript" src='<%=ResolveUrl("~/GR/script/T00014.js")%>'></script>
    <script type="text/javascript">
        var pnlListAreaId = '<%=Me.pnlListArea.ClientID%>';
        var IsPostBack = '<%=If(IsPostBack = True, "1", "0")%>';
        var EXTRALIST = '<%=LIST_BOX_CLASSIFICATION.LC_EXTRA_LIST%>';
    </script>
</asp:Content>

<asp:Content ID="T00014" ContentPlaceHolderID="contents1" runat="server">
    <!-- 全体レイアウト　headerbox -->
    <div class="headerboxOnly" id="headerbox">
        <div class="Operation">
            <!-- 絞込従業員 -->
            <a style="position:fixed; top:2.5em; left:3em;">
                <asp:Label ID="Label1" runat="server" Text="配属部署" Height="1.5em" Font-Bold="false" Font-Underline="true"></asp:Label>
            </a>
            <a style="position:fixed; top:2.5em; left:8em;" ondblclick="Field_DBclick('WF_SELHORG', <%=LIST_BOX_CLASSIFICATION.LC_EXTRA_LIST%>)">
                <asp:TextBox ID="WF_SELHORG" runat="server" Height="1.1em" Width="7em" CssClass="WF_TEXTBOX_CSS" BorderStyle="NotSet"></asp:TextBox>
                <asp:Label ID="WF_SELHORG_TEXT" runat="server" Width="30em" CssClass="WF_TEXT"></asp:Label>
            </a>
            <a style="position:fixed; top:2.5em; left:25em;">
                <asp:Label ID="Label2" runat="server" Text="職務区分" Height="1.5em" Font-Bold="false" Font-Underline="true"></asp:Label>
            </a>
            <a style="position:fixed; top:2.5em; left:30em;" ondblclick="Field_DBclick('WF_SELSTAFFKBN', <%=LIST_BOX_CLASSIFICATION.LC_EXTRA_LIST%>)">
                <asp:TextBox ID="WF_SELSTAFFKBN" runat="server" Height="1.1em" Width="7em" CssClass="WF_TEXTBOX_CSS" BorderStyle="NotSet"></asp:TextBox>
                <asp:Label ID="WF_SELSTAFFKBN_TEXT" runat="server" Width="30em" CssClass="WF_TEXT"></asp:Label>
            </a>

            <!-- ボタン -->
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
                <a style="position:fixed; top:4.0em; left:3em;">
                    <asp:Label ID="WF_SELSTAFFCODE_L" runat="server" Text="従業員" Height="1.5em" Font-Bold="false" Font-Underline="true"></asp:Label>
                </a>
                <a style="position:fixed; top:4.0em; left:8em;" ondblclick="Field_DBclick('WF_SELSTAFFCODE', <%=LIST_BOX_CLASSIFICATION.LC_STAFFCODE%>)">
                    <asp:TextBox ID="WF_SELSTAFFCODE" runat="server" Height="1.1em" Width="7em" CssClass="WF_TEXTBOX_CSS" BorderStyle="NotSet"></asp:TextBox>
                    <asp:Label ID="WF_SELSTAFFCODE_TEXT" runat="server" Width="30em" CssClass="WF_TEXT"></asp:Label>
                </a>
                <a style="position:fixed; top:4.0em; left:25em;">
                    <asp:Label ID="Label3" runat="server" Text="従業員名" Height="1.5em" Font-Bold="false" Font-Underline="false"></asp:Label>
                </a>
                <a style="position:fixed; top:4.0em; left:30em;">
                    <asp:TextBox ID="WF_SELSTAFFNAME" runat="server" Height="1.1em" Width="7em" CssClass="WF_TEXTBOX_CSS" BorderStyle="NotSet"></asp:TextBox>
                </a>
                <!-- 対象年月 -->
                <a style="position:fixed; top:5.5em; left:3em;">
                    <asp:Label ID="WF_TAISHOYM_L" runat="server" Text="対象年月" Width="4em" CssClass="WF_TEXT_LEFT"></asp:Label>
                </a>
                <a style="position:fixed; top:5.5em; left:8em;">
                    <asp:Label ID="WF_TAISHOYM" runat="server" Width="4em" CssClass="WF_TEXT_LEFT" Font-Bold="true"></asp:Label>
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
