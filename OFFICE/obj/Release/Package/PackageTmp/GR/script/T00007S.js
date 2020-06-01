// ○OnLoad用処理(左右Box非表示)
function InitDisplay() {

    // 全部消す
    document.getElementById("LF_LEFTBOX").style.width = "0em";
    for (var i = 0; i < document.getElementsByClassName("rightbox").length; ++i) {
        document.getElementsByClassName("rightbox")[i].style.width = "0em";
    }

    // 左ボックス
    if (document.getElementById("WF_LeftboxOpen").value == "Open") {
        document.getElementById("LF_LEFTBOX").style.width = "26em";
    }

    // 右ボックス
    if (document.getElementById("WF_RightboxOpen").value == "Open") {
        for (var i = 0; i < document.getElementsByClassName("rightbox").length; ++i) {
            var rvWidth = 26 * (document.getElementsByClassName("rightbox").length - i);
            document.getElementsByClassName("rightbox")[i].style.width = rvWidth + "em";
        }
    }

    // 左ボックス拡張機能追加
    addLeftBoxExtention(leftListExtentionTarget);

    //更新ボタン活性／非活性
    if (document.getElementById('WF_Restart').value == "TRUE") {
        //活性
        document.getElementById("WF_ButtonRESTART").disabled = "";
    } else {
        //非活性 
        document.getElementById("WF_ButtonRESTART").disabled = "disabled";
    };

};

// ○右Box用処理（右Box表示/非表示切り替え）
function r_boxDisplay() {
    if (document.getElementById('WF_RightboxOpen').value == "Open") {
        for (var i = 0; i < document.getElementsByClassName("rightbox").length; ++i) {
            document.getElementsByClassName("rightbox")[i].style.width = "0em";
        }
        document.getElementById('WF_RightboxOpen').value = "";
    } else {
        document.getElementById('WF_RightboxOpen').value = "Open";
        document.getElementById("WF_ButtonClick").value = "WF_RIGHT_VIEW_DBClick";
        document.body.style.cursor = "wait";
        document.forms[0].submit();
    };
};