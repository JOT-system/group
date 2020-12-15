// ○OnLoad用処理(左右Box非表示)
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

    for (let i = 1; i <= 6; i++) {
        ModifyChange(i);
    }
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
};

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
};

// ○モデル距離画面制御（１行目～６行目）
function ModifyChange(rowNo) {
    var SHARYOKBN = "WF_SHARYOKBN" + rowNo
    var OILPAYKBN = "WF_OILPAYKBN" + rowNo;
    var SHUKABASHO = "WF_SHUKABASHO" + rowNo;
    var TODOKECODE = "WF_TODOKECODE" + rowNo;
    var MODELDISTANCE = "WF_MODELDISTANCE" + rowNo;
    var MODIFYKBN = "WF_MODIFYKBN" + rowNo;

    var SHARYOKBN_DB = "WF_SHARYOKBN_DB" + rowNo;
    var OILPAYKBN_DB = "WF_OILPAYKBN_DB" + rowNo;
    var SHUKABASHO_DB = "WF_SHUKABASHO_DB" + rowNo;
    var TODOKECODE_DB = "WF_TODOKECODE_DB" + rowNo;

    //チェックボックスが☑の場合、非活性。□の場合、活性化する
    if (document.getElementById(MODIFYKBN).checked == true) {
        //チェックボックスが☑の場合、非活性
        document.getElementById(SHARYOKBN).disabled = false;
        document.getElementById(OILPAYKBN).disabled = false;
        document.getElementById(SHUKABASHO).disabled = false;
        document.getElementById(TODOKECODE).disabled = false;
        document.getElementById(MODELDISTANCE).disabled = false;
        //左BOXの表示を有効にする（無効にするクラス名（CCS）を消す）
        document.getElementById(SHARYOKBN_DB).className = "";
        document.getElementById(OILPAYKBN_DB).className = "";
        document.getElementById(SHUKABASHO_DB).className = "";
        document.getElementById(TODOKECODE_DB).className = "";
    } else {
        document.getElementById(SHARYOKBN).disabled = true;
        document.getElementById(OILPAYKBN).disabled = true;
        document.getElementById(SHUKABASHO).disabled = true;
        document.getElementById(TODOKECODE).disabled = true;
        document.getElementById(MODELDISTANCE).disabled = true;
        //左BOXの表示を無効にする（無効にするクラス名（CCS）を設定）
        document.getElementById(SHARYOKBN_DB).className = "disabled";
        document.getElementById(OILPAYKBN_DB).className = "disabled";
        document.getElementById(SHUKABASHO_DB).className = "disabled";
        document.getElementById(TODOKECODE_DB).className = "disabled";

    }
};