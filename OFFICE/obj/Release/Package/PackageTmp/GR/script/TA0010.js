// ○OnLoad用処理（左右Box非表示）
function InitDisplay() {

    document.getElementById("RF_RIGHTBOX").style.width = "0em";
    if (document.getElementById('WF_RightboxOpen').value == "Open") {
        document.getElementById("RF_RIGHTBOX").style.width = "26em";
    };
    /* 共通一覧のスクロールイベント紐づけ */
    bindListCommonEvents(pnlListAreaId, IsPostBack);
};

// SETECTOR行情報取得処理（行情報退避用）
function SELECTOR_Click(tabNo, NAME) {
    //サーバー未処理（MF_SUBMIT="FALSE"）のときのみ、SUBMIT
    if (document.getElementById("MF_SUBMIT").value == "FALSE") {
        document.getElementById("MF_SUBMIT").value = "TRUE"
        // Field名退避
        document.getElementById('WF_SELECTOR_Chg').value = tabNo;
        document.getElementById('WF_SELECTOR_SW').value = "ON";
        document.getElementById('WF_ButtonClick').value = "WF_SELECTOR_SW_Click";
        if (tabNo == '0') {
            document.getElementById("WF_SaveSX").value = document.all('ORGSelect').scrollLeft;
            document.getElementById("WF_SaveSY").value = document.all('ORGSelect').scrollTop;
            document.getElementById('WF_SELECTOR_PosiORG').value = NAME;
        };
        document.body.style.cursor = "wait";
        document.forms[0].submit();                             //aspx起動
    } else {
        return false;
    };
};
