<%@ Page Title="T00016" Language="vb" AutoEventWireup="false" CodeBehind="GRT00016NSEIKYU.aspx.vb" Inherits="OFFICE.GRT00016NSEIKYU" %>
<%@ MasterType VirtualPath="~/GR/GRMasterPage.Master" %> 

<%@ Import Namespace="OFFICE.GRIS0005LeftBox" %>

<%@ register src="~/inc/GRIS0004RightBox.ascx" tagname="rightview" tagprefix="MSINC" %>
<%@ register src="~/inc/GRIS0005LeftBox.ascx" tagname="leftview" tagprefix="MSINC" %>
<%@ register src="inc/GRT00016WRKINC.ascx" tagname="work" tagprefix="LSINC" %>

<asp:Content ID="T00016H" ContentPlaceHolderID="head" runat="server">
    <link rel="stylesheet" type="text/css" href="<%=ResolveUrl("~/GR/css/T00016.css")%>"/>
    <script type="text/javascript">
        var pnlListAreaId1 = '<%=Me.pnlListArea1.ClientID%>';
        var pnlListAreaId2 = '<%=Me.pnlListArea2.ClientID%>';
        var pnlListAreaId3 = '<%=Me.pnlListArea3.ClientID%>';
        var pnlListAreaId4 = '<%=Me.pnlListArea4.ClientID%>';
        var IsPostBack = '<%=If(IsPostBack = True, "1", "0") %>';
    </script>
    <script type="text/javascript" src="<%=ResolveUrl("~/GR/script/T00016.js")%>"></script>
</asp:Content> 

<asp:Content ID="T00016" ContentPlaceHolderID="contents1" runat="server">

    <!-- 全体レイアウト　headerbox -->
    <div  class="headerboxOnly" id="headerbox">
        <div class="Operation">

            <!-- ■　ボタン　■ -->
            <a style="position:fixed;top:2.8em;left:2em;">
                <input type="button" id="WF_ButtonAdd" value="行追加"  style="Width:5em" onclick="ButtonClick('WF_ButtonAdd');" />
            </a>
            <a style="position:fixed;top:2.8em;left:6.5em;">
                <input type="button" id="WF_ButtonExtract" value="絞込み"  style="Width:5em" onclick="ButtonClick('WF_ButtonExtract');" />
            </a>
            <a style="position:fixed;top:2.8em;left:33em;">
                <input type="button" id="WF_ButtonGet" value="日報"  style="Width:5em" onclick="ButtonClick('WF_ButtonGet');" />
            </a>
            <a style="position:fixed;top:2.8em;left:37.5em;">
                <input type="button" id="WF_ButtonSupplJisski" value="用車実績"  style="Width:5em" onclick="ButtonClick('WF_ButtonSupplJisski');" />
            </a>
            <a style="position:fixed;top:2.8em;left:53.5em;">
            </a>
            <a style="position:fixed;top:2.8em;left:58em;">
                <input type="button" id="WF_ButtonUPDATE" value="DB更新"  style="Width:5em" onclick="ButtonClick('WF_ButtonUPDATE');" />
            </a>
            <a style="position:fixed;top:2.8em;left:62.5em;">
                <input type="button" id="WF_ButtonCSV" value="ﾀﾞｳﾝﾛｰﾄﾞ"  style="Width:5em" onclick="ButtonClick('WF_ButtonCSV');" />
            </a>
            <a style="position:fixed;top:2.8em;left:67em;">
                <input type="button" id="WF_ButtonEND" value="終了"  style="Width:5em" onclick="ButtonClick('WF_ButtonEND');" />
            </a>
            <a style="position:fixed;top:3.2em;left:75em;">
                <asp:Image ID="WF_ButtonFIRST" runat="server" ImageUrl="~/先頭頁.png" Width="1.5em" onclick="ButtonClick('WF_ButtonFIRST');" Height="1em" ImageAlign="AbsMiddle" />
            </a>
            <a style="position:fixed;top:3.2em;left:77em;">
                <asp:Image ID="WF_ButtonLAST" runat="server" ImageUrl="~/最終頁.png" Width="1.5em" onclick="ButtonClick('WF_ButtonLAST');" Height="1em" ImageAlign="AbsMiddle" />
            </a>

            <br/>
            <br/>
            <!-- ■　取引日付　■ -->
            <a ondblclick="Field_DBclick('WF_SELTORIDATE', <%= LIST_BOX_CLASSIFICATION.LC_CALENDAR%>)">
                <asp:Label ID="WF_SELTORIDATE_LABEL" runat="server" Text="取引日付" Width="4.5em" Font-Bold="True" Font-Underline="True"></asp:Label>
                <asp:TextBox ID="WF_SELTORIDATE" runat="server" Width="6em" CssClass="WF_TEXT_LEFT"></asp:TextBox>
            </a>
            <!-- ■　届先　■ -->
            <a ondblclick="Field_DBclick('WF_SELTODOKESAKI', <%= LIST_BOX_CLASSIFICATION.LC_DISTINATION%>)">
                <asp:Label ID="WF_SELTODOKESAKI_LABEL" runat="server" Text="取引先" Width="3.2em" Font-Bold="True" Font-Underline="True"></asp:Label>
                <asp:TextBox ID="WF_SELTODOKESAKI" runat="server" Width="6em" CssClass="WF_TEXT_LEFT"></asp:TextBox>
                <asp:Label ID="WF_SELTODOKESAKI_TEXT" runat="server" Width="12em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
            </a>
            <!-- ■　車番　■ -->
            <a ondblclick="Field_DBclick('WF_SELSHABAN', <%= LIST_BOX_CLASSIFICATION.LC_WORKLORRY%>)">
                <asp:Label ID="WF_SELSHABAN_LABEL" runat="server" Text="車番" Width="3.2em" Font-Bold="True" Font-Underline="True"></asp:Label>
                <asp:TextBox ID="WF_SELSHABAN" runat="server" Width="6em" CssClass="WF_TEXT_LEFT"></asp:TextBox>
            </a>
            <br/>

            <!-- ■　対象年月　■ -->
            <a>
                <asp:Label ID="WF_TAISHOYM_LABEL" runat="server" Text="対象年月：" Width="5em" Font-Bold="True" ></asp:Label>
                <asp:Label ID="WF_TAISHOYM_TEXT_LABEL" runat="server" Width="12em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
            </a>
            <!-- ■　出荷部署　■ -->
            <a>
                <asp:Label ID="WF_SHUKAORG_LABEL" runat="server" Text="出荷部署：" Width="5em" Font-Bold="True" ></asp:Label>
                <asp:Label ID="WF_SHUKAORG_TEXT_LABEL" runat="server" Width="12em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
            </a>
            <!-- ■　油種　■ -->
            <a>
                <asp:Label ID="WF_OILTYPE_LABEL" runat="server" Text="油種：" Width="3.2em" Font-Bold="True" ></asp:Label>
                <asp:Label ID="WF_OILTYPE_TEXT_LABEL" runat="server" Width="12em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
            </a>

        </div>

        <!-- 全体レイアウト　detailbox -->
        <div class="detailbox" id="detailkeybox">
            <!-- タブボックス -->
            <div id="tabBox">
                <div class="leftSide">
                    <!-- ■　Dタブ　■ -->
                    <asp:Label ID="WF_Dtab01" runat="server" Text="合計(社内)" data-itemelm="tab" onclick="DtabChange('0')" ></asp:Label>
                    <asp:Label ID="WF_Dtab02" runat="server" Text="合計(請求)" data-itemelm="tab" onclick="DtabChange('1')" ></asp:Label>
                    <asp:Label ID="WF_Dtab03" runat="server" Text="明細(金額)" data-itemelm="tab" onclick="DtabChange('2')" ></asp:Label>
                    <asp:Label ID="WF_Dtab04" runat="server" Text="明細(数量)" data-itemelm="tab" onclick="DtabChange('3')" ></asp:Label>
                </div>
                <div class="rightSide">
                    <span id="hideHeader">
                    </span>
                </div>
            </div>

            <asp:MultiView ID="WF_DetailMView" runat="server">
                <!-- ■ Tab No1　合計(社内)　■ -->
                <asp:View ID="WF_DView1" runat="server" >
                    <!-- 一覧レイアウト -->
                    <asp:panel id="pnlListArea1" runat="server" ></asp:panel>
                </asp:View>
                <!-- ■ Tab No2　合計(請求)　■ -->
                <asp:View ID="WF_DView2" runat="server">
                    <!-- 一覧レイアウト -->
                    <asp:panel id="pnlListArea2" runat="server" ></asp:panel>
                </asp:View>
                <!-- ■ Tab No3　明細(金額)　■ -->
                <asp:View ID="WF_DView3" runat="server">
                    <!-- 一覧レイアウト -->
                    <asp:panel id="pnlListArea3" runat="server" ></asp:panel>
                </asp:View>
                <!-- ■ Tab No4　明細(数量)　■ -->
                <asp:View ID="WF_DView4" runat="server">
                    <!-- 一覧レイアウト -->
                    <asp:panel id="pnlListArea4" runat="server" ></asp:panel>
                </asp:View>
            </asp:MultiView>
        </div>
    </div>

    <%-- rightview --%>
    <MSINC:rightview id="rightview" runat="server" />
    <%-- leftview --%>
    <MSINC:leftview id="leftview" runat="server" />

    <!-- leftview 画面独自 -->
    <div class="leftbox" id="leftbox">
        <div class="button" id="button" style="position:relative;left:0.5em;top:0.8em;">
            <input type="button" id="WF_ButtonSel" value="　選　択　"  onclick="ButtonClick('WF_ButtonSel');" />
            <input type="button" id="WF_ButtonCan" value="キャンセル"  onclick="ButtonClick('WF_ButtonCan');" />
        </div><br />
            
        <asp:MultiView ID="WF_LeftMView" runat="server">

            <!-- 　業務車番　 -->
            <asp:View id="LeftView1" runat="server" >
                <a  style="position:relative;height: 30.5em; width:24.7em;overflow: hidden;" ondblclick="TableDBclick()">
                    <span class="WF_TableArea">
                        <asp:Repeater ID="WF_GSHABAN_Rep" runat="server">
                            <HeaderTemplate>
                                    <asp:Table ID="WF_GSHABAN_HeadTable" runat="server" cellspacing="0" rules="all" border="1" CssClass="WF_HeaderArea">
                                    <asp:TableRow  runat="server" CssClass="WF_TEXT_CENTER">
                                        <asp:TableCell ID="WF_GSHABAN_HeadCell1"   runat="server" style="width:8.4em;" Text='業務車番' RowSpan="2"></asp:TableCell>
                                        <asp:TableCell ID="WF_GSHABAN_HeadCell2"   runat="server" style="width:8.4em;" Text='油種'     RowSpan="2"></asp:TableCell>
                                        <asp:TableCell ID="WF_GSHABAN_HeadCell3"   runat="server" style="width:8.4em;" Text='車腹'     RowSpan="2"></asp:TableCell>
                                        <asp:TableCell ID="WF_GSHABAN_HeadCell4"   runat="server" style="width:8.4em;" Text='荷主'     RowSpan="2"></asp:TableCell>
                                        <asp:TableCell ID="WF_GSHABAN_HeadCellH"   runat="server" style="width:8.4em;" Text='配車状況' ColumnSpan="8"></asp:TableCell>
                                        <asp:TableCell ID="WF_GSHABAN_HeadCell5"   runat="server" style="width:8.4em;" Text='運休'     RowSpan="2"></asp:TableCell>
                                    </asp:TableRow>
                                    <asp:TableRow  runat="server" CssClass="WF_TEXT_CENTER">
                                        <asp:TableCell ID="WF_GSHABAN_HeadCellH_1" runat="server" style="width:1.8em;" Text='1'></asp:TableCell>
                                        <asp:TableCell ID="WF_GSHABAN_HeadCellH_2" runat="server" style="width:1.8em;" Text='2'></asp:TableCell>
                                        <asp:TableCell ID="WF_GSHABAN_HeadCellH_3" runat="server" style="width:1.8em;" Text='3'></asp:TableCell>
                                        <asp:TableCell ID="WF_GSHABAN_HeadCellH_4" runat="server" style="width:1.8em;" Text='4'></asp:TableCell>
                                        <asp:TableCell ID="WF_GSHABAN_HeadCellH_5" runat="server" style="width:1.8em;" Text='5'></asp:TableCell>
                                        <asp:TableCell ID="WF_GSHABAN_HeadCellH_6" runat="server" style="width:1.8em;" Text='6'></asp:TableCell>
                                        <asp:TableCell ID="WF_GSHABAN_HeadCellH_7" runat="server" style="width:1.8em;" Text='7'></asp:TableCell>
                                        <asp:TableCell ID="WF_GSHABAN_HeadCellH_8" runat="server" style="width:1.8em;" Text='8'></asp:TableCell>
                                    </asp:TableRow>
                                </asp:Table>
                            </HeaderTemplate>

                            <ItemTemplate>
                                    <asp:Table ID="WF_GSHABAN_ItemTable" runat="server" cellspacing="0" rules="all" border="1" CssClass="WF_DetialArea">
                                    <asp:TableRow ID="WF_GSHABAN_Items" runat="server">
                                        <asp:TableCell ID="WF_GSHABAN_ItemCell6"   runat="server" style="width:7.0em;" Text='<%# Eval("SHARYOSTATUS")%>' hidden="hidden"></asp:TableCell>
                                        <asp:TableCell ID="WF_GSHABAN_ItemCell7"   runat="server" style="width:7.0em;" Text='<%# Eval("LICNPLTNOF")%>' hidden="hidden"></asp:TableCell>
                                        <asp:TableCell ID="WF_GSHABAN_ItemCell8"   runat="server" style="width:7.0em;" Text='<%# Eval("SHARYOINFO1")%>' hidden="hidden"></asp:TableCell>
                                        <asp:TableCell ID="WF_GSHABAN_ItemCell9"   runat="server" style="width:7.0em;" Text='<%# Eval("SHARYOINFO2")%>' hidden="hidden"></asp:TableCell>
                                        <asp:TableCell ID="WF_GSHABAN_ItemCell10"  runat="server" style="width:7.0em;" Text='<%# Eval("SHARYOINFO3")%>' hidden="hidden"></asp:TableCell>
                                        <asp:TableCell ID="WF_GSHABAN_ItemCell11"  runat="server" style="width:7.0em;" Text='<%# Eval("SHARYOINFO4")%>' hidden="hidden"></asp:TableCell>
                                        <asp:TableCell ID="WF_GSHABAN_ItemCell12"  runat="server" style="width:7.0em;" Text='<%# Eval("SHARYOINFO5")%>' hidden="hidden"></asp:TableCell>
                                        <asp:TableCell ID="WF_GSHABAN_ItemCell13"  runat="server" style="width:7.0em;" Text='<%# Eval("SHARYOINFO6")%>' hidden="hidden"></asp:TableCell>
                                        <asp:TableCell ID="WF_GSHABAN_ItemCell14"  runat="server" style="width:7.0em;" Text='<%# Eval("SHAFUKU")%>' hidden="hidden"></asp:TableCell>
                                        <asp:TableCell ID="WF_GSHABAN_TableCell5"  runat="server" style="width:7.0em;" Text='<%# Eval("TSHABANF")%>' hidden="hidden"></asp:TableCell>
                                        <asp:TableCell ID="WF_GSHABAN_TableCell6"  runat="server" style="width:7.0em;" Text='<%# Eval("TSHABANB")%>' hidden="hidden"></asp:TableCell>
                                        <asp:TableCell ID="WF_GSHABAN_TableCell7"  runat="server" style="width:7.0em;" Text='<%# Eval("TSHABANB2")%>' hidden="hidden"></asp:TableCell>
                                        <asp:TableCell ID="WF_GSHABAN_TableCell8"   runat="server" style="width:7.0em;" Text='<%# Eval("LICNPLTNOB")%>' hidden="hidden"></asp:TableCell>
                                        <asp:TableCell ID="WF_GSHABAN_TableCell9"   runat="server" style="width:7.0em;" Text='<%# Eval("LICNPLTNOB2")%>' hidden="hidden"></asp:TableCell>

                                        <asp:TableCell ID="WF_GSHABAN_ItemCell1"   runat="server" style="width:7.0em;" Text='<%# Eval("GSHABAN")%>'></asp:TableCell>
                                        <asp:TableCell ID="WF_GSHABAN_ItemCell2"   runat="server" style="width:7.0em;" Text='<%# Eval("OILTYPENAME")%>'></asp:TableCell>
                                        <asp:TableCell ID="WF_GSHABAN_ItemCell3"   runat="server" style="width:7.0em;" Text='<%# Eval("SHAFUKU")%>'></asp:TableCell>
                                        <asp:TableCell ID="WF_GSHABAN_ItemCell4"   runat="server" style="width:7.0em;" Text='<%# Eval("OWNCODENAME")%>'></asp:TableCell>
                                        <asp:TableCell ID="WF_GSHABAN_ItemCellH_1" runat="server" style="width:1.8em;" Text='<%# Eval("HSTATUS1")%>' CssClass="WF_TEXT_CENTER"></asp:TableCell>
                                        <asp:TableCell ID="WF_GSHABAN_ItemCellH_2" runat="server" style="width:1.8em;" Text='<%# Eval("HSTATUS2")%>' CssClass="WF_TEXT_CENTER"></asp:TableCell>
                                        <asp:TableCell ID="WF_GSHABAN_ItemCellH_3" runat="server" style="width:1.8em;" Text='<%# Eval("HSTATUS3")%>' CssClass="WF_TEXT_CENTER"></asp:TableCell>
                                        <asp:TableCell ID="WF_GSHABAN_ItemCellH_4" runat="server" style="width:1.8em;" Text='<%# Eval("HSTATUS4")%>' CssClass="WF_TEXT_CENTER"></asp:TableCell>
                                        <asp:TableCell ID="WF_GSHABAN_ItemCellH_5" runat="server" style="width:1.8em;" Text='<%# Eval("HSTATUS5")%>' CssClass="WF_TEXT_CENTER"></asp:TableCell>
                                        <asp:TableCell ID="WF_GSHABAN_ItemCellH_6" runat="server" style="width:1.8em;" Text='<%# Eval("HSTATUS6")%>' CssClass="WF_TEXT_CENTER"></asp:TableCell>
                                        <asp:TableCell ID="WF_GSHABAN_ItemCellH_7" runat="server" style="width:1.8em;" Text='<%# Eval("HSTATUS7")%>' CssClass="WF_TEXT_CENTER"></asp:TableCell>
                                        <asp:TableCell ID="WF_GSHABAN_ItemCellH_8" runat="server" style="width:1.8em;" Text='<%# Eval("HSTATUS8")%>' CssClass="WF_TEXT_CENTER"></asp:TableCell>
                                        <asp:TableCell ID="WF_GSHABAN_ItemCell5"   runat="server" style="width:7.0em;" Text='<%# Eval("SHARYOSTATUSNAME")%>'></asp:TableCell>
                                    </asp:TableRow>
                                </asp:Table>
                            </ItemTemplate>
                            <FooterTemplate>
                            </FooterTemplate>
                        </asp:Repeater>
                    </span>
                </a>
            </asp:View>

        </asp:MultiView>

    </div>

    <div hidden="hidden">
        <asp:TextBox ID="WF_GridDBclick" Text="" runat="server" ></asp:TextBox>         <!-- GridViewダブルクリック -->
        <asp:TextBox ID="WF_GridPosition" Text="" runat="server" ></asp:TextBox>        <!-- GridView表示位置フィールド -->

        <input id="WF_ButtonClick" runat="server" value=""  type="text" />              <!-- ボタン押下 -->
        <input id="WF_MAPpermitcode" runat="server" value=""  type="text" />            <!-- 権限 -->

        <asp:ListBox ID="WF_ListGSHABAN" runat="server"></asp:ListBox>                  <!-- List業務車番 -->
        <asp:ListBox ID="WF_ListSHARYOINFO1" runat="server"></asp:ListBox>              <!-- List車両情報１ -->
        <asp:ListBox ID="WF_ListSHARYOINFO2" runat="server"></asp:ListBox>              <!-- List車両情報２ -->
        <asp:ListBox ID="WF_ListSHARYOINFO3" runat="server"></asp:ListBox>              <!-- List車両情報３ -->
        <asp:ListBox ID="WF_ListSHARYOINFO4" runat="server"></asp:ListBox>              <!-- List車両情報４ -->
        <asp:ListBox ID="WF_ListSHARYOINFO5" runat="server"></asp:ListBox>              <!-- List車両情報５ -->
        <asp:ListBox ID="WF_ListSHARYOINFO6" runat="server"></asp:ListBox>              <!-- List車両情報６ -->
        <asp:ListBox ID="WF_ListOILTYPE" runat="server"></asp:ListBox>                  <!-- List車両油種 -->
        <asp:ListBox ID="WF_ListOILTYPENAME" runat="server"></asp:ListBox>              <!-- List車両油種名 -->
        <asp:ListBox ID="WF_ListSHAFUKU" runat="server"></asp:ListBox>                  <!-- List車腹 -->
        <asp:ListBox ID="WF_ListOWNCODE" runat="server"></asp:ListBox>                  <!-- List車両荷主 -->
        <asp:ListBox ID="WF_ListOWNCODENAME" runat="server"></asp:ListBox>              <!-- List車両荷主名称 -->
        <asp:ListBox ID="WF_ListSHARYOSTATUS" runat="server"></asp:ListBox>             <!-- List車両状態 -->
        <asp:ListBox ID="WF_ListSHARYOSTATUSNAME" runat="server"></asp:ListBox>         <!-- List車両状態名称 -->
        <asp:ListBox ID="WF_ListLICNPLTNOF" runat="server"></asp:ListBox>               <!-- List登録車番前 -->
        <asp:ListBox ID="WF_ListLICNPLTNOB" runat="server"></asp:ListBox>               <!-- List登録車番後 -->
        <asp:ListBox ID="WF_ListLICNPLTNOB2" runat="server"></asp:ListBox>              <!-- List登録車番後２ -->
        <asp:ListBox ID="WF_ListTSHABANF" runat="server"></asp:ListBox>                 <!-- List統一車番前 -->
        <asp:ListBox ID="WF_ListTSHABANB" runat="server"></asp:ListBox>                 <!-- List統一車番後 -->
        <asp:ListBox ID="WF_ListTSHABANB2" runat="server"></asp:ListBox>                <!-- List統一車番後２ -->
        <asp:ListBox ID="WF_ListHPRSINSNYMDF" runat="server"></asp:ListBox>             <!-- List統一次回容器検査年月日前 -->
        <asp:ListBox ID="WF_ListHPRSINSNYMDB" runat="server"></asp:ListBox>             <!-- List統一次回容器検査年月日後 -->
        <asp:ListBox ID="WF_ListHPRSINSNYMDB2" runat="server"></asp:ListBox>            <!-- List統一次回容器検査年月日後2 -->
        <asp:ListBox ID="WF_ListLICNYMDF" runat="server"></asp:ListBox>                 <!-- List統一車検有効年月日前 -->
        <asp:ListBox ID="WF_ListLICNYMDB" runat="server"></asp:ListBox>                 <!-- List統一車検有効年月日後 -->
        <asp:ListBox ID="WF_ListLICNYMDB2" runat="server"></asp:ListBox>                <!-- List統一車検有効年月日後2 -->

        <asp:ListBox ID="WF_ListGSHABAN_CONT" runat="server"></asp:ListBox>             <!-- Listコンテナ業務車番 -->
        <asp:ListBox ID="WF_ListOILTYPE_CONT" runat="server"></asp:ListBox>             <!-- Listコンテナ油種 -->
        <asp:ListBox ID="WF_ListOILTYPENAME_CONT" runat="server"></asp:ListBox>         <!-- Listコンテナ油種名称 -->
        <asp:ListBox ID="WF_ListSHAFUKU_CONT" runat="server"></asp:ListBox>             <!-- Listコンテナ車腹 -->
        <asp:ListBox ID="WF_ListOWNCODE_CONT" runat="server"></asp:ListBox>             <!-- Listコンテナ車両荷主 -->
        <asp:ListBox ID="WF_ListOWNCODENAME_CONT" runat="server"></asp:ListBox>         <!-- Listコンテナ車両荷主名称 -->
        <asp:ListBox ID="WF_ListSHARYOSTATUS_CONT" runat="server"></asp:ListBox>        <!-- Listコンテナ車両状態 -->
        <asp:ListBox ID="WF_ListSHARYOSTATUSNAME_CONT" runat="server"></asp:ListBox>    <!-- Listコンテナ車両状態名称 -->
        <asp:ListBox ID="WF_ListLICNPLTNOF_CONT" runat="server"></asp:ListBox>          <!-- Listコンテナ登録車番前 -->
        <asp:ListBox ID="WF_ListLICNPLTNOB_CONT" runat="server"></asp:ListBox>          <!-- Listコンテナ登録車番後 -->


        <asp:TextBox ID="WF_DEFORG" runat="server"></asp:TextBox>                       <!-- 所属部署　 -->

        <input id="WF_FIELD"  runat="server" value=""  type="text" />                   <!-- Textbox DBクリックフィールド -->
        <input id="WF_FIELD_REP"  runat="server" value=""  type="text" />               <!-- Textbox(Repeater) DBクリックフィールド -->

        <input id="WF_LeftMViewChange" runat="server" value="" type="text"/>            <!-- Leftbox Mview切替 -->
        <input id="WF_LeftboxOpen"  runat="server" value=""  type="text" />             <!-- Leftbox 開閉 -->

        <input id="WF_RightViewChange" runat="server" value="" type="text"/>            <!-- Rightbox Mview切替 -->
        <input id="WF_RightboxOpen" runat="server" value=""  type="text" />             <!-- Rightbox 開閉 -->

        <input id="WF_SelectedIndex"  runat="server" value=""  type="text" />           <!-- Leftbox DBクリックフィールド(行位置) -->

        <input id="WF_REP_LINECNT"  runat="server" value=""  type="text" />             <!-- Repeater 行位置 -->
        <input id="WF_REP_POSITION"  runat="server" value=""  type="text" />            <!-- Repeater 行位置 -->
        <input id="WF_REP_Change"  runat="server" value=""  type="text" />              <!-- Repeater 変更監視 -->
        <input id="WF_REP_ROWSCNT" runat="server" value=""  type="text" />              <!-- Repeaterの１明細の行数 -->
        <input id="WF_REP_COLSCNT" runat="server" value=""  type="text" />              <!-- Repeaterの列数 -->
            
        <input id="WF_IsHideDetailBox"  runat="server" value="1" type="text" />         <!-- 詳細画面非表示フラグ -->
    
        <input id="WF_PrintURL" runat="server" value=""  type="text" />                 <!-- Textbox Print URL -->
                
        <input id="WF_DTAB_CHANGE_NO" runat="server" value="" type="text"/>             <!-- DetailBox Mview切替 -->

    </div>

    <!-- Work レイアウト -->
    <LSINC:work id="work" runat="server" />

</asp:Content>