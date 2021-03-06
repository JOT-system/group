﻿// ○OnLoad用処理(左右Box非表示)
function InitDisplay() {

    // 全部消す
    document.getElementById("LF_LEFTBOX").style.width = "0em";
    document.getElementById("RF_RIGHTBOX").style.width = "0em";

    // 左ボックス
    if (document.getElementById("WF_LeftboxOpen").value == "Open") {
        document.getElementById("LF_LEFTBOX").style.width = "26em";
    }

    // 右ボックス
    if (document.getElementById("WF_RightboxOpen").value == "Open") {
        document.getElementById("RF_RIGHTBOX").style.width = "26em";
    }

    //更新ボタン活性／非活性
    if (document.getElementById('WF_MAPpermitcode').value == "TRUE") {
        //活性
        document.getElementById("WF_ButtonNIPPOEDIT").disabled = "";
        document.getElementById("WF_ButtonUPDATE").disabled = "";
    } else {
        //非活性 
        document.getElementById("WF_ButtonNIPPOEDIT").disabled = "disabled";
        document.getElementById("WF_ButtonUPDATE").disabled = "disabled";
    };

    //日報ボタン活性／非活性
    if (document.getElementById('WF_NIPPObtn').value == "TRUE") {
        //活性
        document.getElementById("WF_ButtonBREAKTIME").hidden = "";
        document.getElementById("WF_ButtonNIPPOEDIT").hidden = "";
        document.getElementById("WF_ButtonNIPPO").hidden = "";
    } else {
        //非活性 
        document.getElementById("WF_ButtonBREAKTIME").hidden = "hidden";
        document.getElementById("WF_ButtonNIPPOEDIT").hidden = "hidden";
        document.getElementById("WF_ButtonNIPPO").hidden = "hidden";
    };

    // 左ボックス拡張機能追加
    addLeftBoxExtention(leftListExtentionTarget);

    // リストの共通イベント(ホイール、横スクロール)をバインド
    bindListCommonEvents(pnlListAreaId, IsPostBack, false, true, true, false);

};

// ○ディテール(タブ切替)処理
function DtabChange(tabNo) {
    if (document.getElementById("MF_SUBMIT").value == "FALSE") {
        document.getElementById("MF_SUBMIT").value = "TRUE"
        document.getElementById('WF_DTABChange').value = tabNo;
        document.getElementById('WF_ButtonClick').value = "WF_DTABChange";
        document.body.style.cursor = "wait";
        document.forms[0].submit();                            //aspx起動
    } else {
        return false;
    }
}

function TTL_SUM() {
    document.getElementById('WF_MODELDISTANCETTL').value = eval(document.getElementById('WF_MODELDISTANCE_LNG1').value)
        + eval(document.getElementById('WF_MODELDISTANCE_LNG2').value)
        + eval(document.getElementById('WF_MODELDISTANCE_RATE1').value)
        + eval(document.getElementById('WF_MODELDISTANCE_RATE2').value)
};

// ○項目変更
function ItemChange(fieldNM) {
    if (document.getElementById("MF_SUBMIT").value == "FALSE") {
        document.getElementById("MF_SUBMIT").value = "TRUE"
        document.getElementById('WF_FIELD').value = fieldNM;
        document.getElementById('WF_ButtonClick').value = "WF_LeftBoxSelectClick";
        document.body.style.cursor = "wait";
        document.forms[0].submit();                            //aspx起動
    } else {
        return false;
    }
}