
// ○OnLoad用処理（左右Box非表示）
function InitDisplay() {

    // 全部消す
    document.getElementById("LF_LEFTBOX").style.width = "0em";
    document.getElementById("RF_RIGHTBOX").style.width = "0em";

    if (document.getElementById('WF_LeftboxOpen').value == "Open") {
        document.getElementById("LF_LEFTBOX").style.width = "26em";
    };

    if (document.getElementById('WF_RightboxOpen').value == "Open") {
        document.getElementById("RF_RIGHTBOX").style.width = "26em";
    };
    //更新ボタン活性／非活性
    if (document.getElementById('WF_MAPpermitcode').value == "TRUE") {
        //活性
        document.getElementById("WF_ButtonUPDATE").disabled = "";
    } else {
        //非活性 
        document.getElementById("WF_ButtonUPDATE").disabled = "disabled";
    };
    /* 共通一覧のスクロールイベント紐づけ */
    bindListCommonEvents(pnlListAreaId, IsPostBack);
    addLeftBoxExtention(leftListExtentionTarget);
};
// ○リスト内容変更処理
function ListChange(pnlList, Line) {
    if (document.getElementById("MF_SUBMIT").value == "FALSE") {
        document.getElementById("MF_SUBMIT").value = "TRUE";
        document.getElementById("WF_GridDBclick").value = Line
        document.getElementById("WF_ButtonClick").value = "WF_ListChange";
        document.forms[0].submit();
    }
}
// ○左Box用処理（左Box表示/非表示切り替え）
function ListField_DBclick(pnlList, Line, fieldNM) {
    if (document.getElementById("MF_SUBMIT").value == "FALSE") {
        document.getElementById("MF_SUBMIT").value = "TRUE";
        document.getElementById('WF_GridDBclick').value = Line;
        document.getElementById('WF_FIELD').value = fieldNM;
        document.getElementById('WF_LeftMViewChange').value = 17;
        document.getElementById('WF_LeftboxOpen').value = "Open";
        document.getElementById('WF_ButtonClick').value = "WF_Field_DBClick";
        document.body.style.cursor = "wait";
        document.forms[0].submit();
    };
};

