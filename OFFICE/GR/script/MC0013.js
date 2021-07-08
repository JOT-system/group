﻿// ○OnLoad用処理（左右Box非表示）
function InitDisplay() {

    // 全部消す
    document.getElementById("LF_LEFTBOX").style.width = "0em";
    document.getElementById("RF_RIGHTBOX").style.width = "0em";

    if (document.getElementById('WF_LeftboxOpen').value == "Open") {
        document.getElementById("LF_LEFTBOX").style.width = "26em";
    };


    // ○画面切替用処理（表示/非表示切替「ヘッダー、ディティール」）
    if (document.getElementById('WF_BOXChange').value == "detailbox") {
        document.getElementById("headerbox").style.visibility = "hidden";
        document.getElementById("detailbox").style.display = "";
        document.getElementById('WF_BOXChange').value = "detailbox";
    } else {
        document.getElementById("headerbox").style.visibility = "visible";
        document.getElementById("detailbox").style.display = "none";
        document.getElementById('WF_BOXChange').value = "headerbox";
    };


    addLeftBoxExtention(leftListExtentionTarget);

    if (document.getElementById('WF_RightboxOpen').value == "Open") {
        document.getElementById("RF_RIGHTBOX").style.width = "26em";
    };
    //更新ボタン活性／非活性
    if (document.getElementById('WF_MAPpermitcode').value == "TRUE") {
        //活性
        document.getElementById("WF_ButtonUPDATE").disabled = "";
    }else if (document.getElementById('WF_MAPpermitcode').value == "SYSTEM") {
        //活性
        document.getElementById("WF_ButtonUPDATE").disabled = "";
        document.getElementById("KEY_LINE_7").style.display  = "block"; 
    } else {
        //非活性 
        document.getElementById("WF_ButtonUPDATE").disabled = "disabled";
    };
    /* 共通一覧のスクロールイベント紐づけ */
    bindListCommonEvents(pnlListAreaId, IsPostBack);
};

