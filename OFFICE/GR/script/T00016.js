// ○OnLoad用処理（左右Box非表示）
function InitDisplay() {

    // 全部消す
    document.getElementById("LF_LEFTBOX").style.width = "0em";
    document.getElementById("RF_RIGHTBOX").style.width = "0em";
    document.getElementById("leftbox").style.width = "0em";

    if (document.getElementById('WF_LeftboxOpen').value == "Open") {
        if (document.getElementById('WF_FIELD').value == "WF_GSHABAN") {
            document.getElementById("leftbox").style.width = "51em";
        } else {
            document.getElementById("LF_LEFTBOX").style.width = "26em";
        };
    };
    if (document.getElementById('WF_RightboxOpen').value == "Open") {
        document.getElementById("RF_RIGHTBOX").style.width = "26em";
    };
    //更新ボタン活性／非活性
    if (document.getElementById('WF_MAPpermitcode').value == "TRUE") {
        //活性
        document.getElementById("WF_ButtonUPDATE").disabled = "";
        document.getElementById("WF_ButtonGet").disabled = "";
    } else {
        //非活性 
        document.getElementById("WF_ButtonUPDATE").disabled = "disabled";
        document.getElementById("WF_ButtonGet").disabled = "disabled";
    };

    // ○画面切替用処理（表示/非表示切替「ヘッダー、ディティール」）
    if (document.getElementById('WF_IsHideDetailBox').value == "0") {
        document.getElementById("headerbox").style.visibility = "hidden";
        //document.getElementById("detailbox").style.visibility = "visible";
        // スクロールをTOP表示に切替
        f_ScrollTop(0, 0)

    } else {
        document.getElementById("headerbox").style.visibility = "visible";
        //document.getElementById("detailbox").style.visibility = "hidden";
    };


    /* 共通一覧のスクロールイベント紐づけ */
    /* 対象の一覧表IDを配列に格納 */
    let arrListId = new Array();
    if (typeof pnlListAreaId1 !== 'undefined') {
        arrListId.push(pnlListAreaId1);
    }
    if (typeof pnlListAreaId2 !== 'undefined') {
        arrListId.push(pnlListAreaId2);
    }
    if (typeof pnlListAreaId3 !== 'undefined') {
        arrListId.push(pnlListAreaId3);
    }
    if (typeof pnlListAreaId4 !== 'undefined') {
        arrListId.push(pnlListAreaId4);
    }
    /* 対象の一覧表IDをループ */
    for (let i = 0, len = arrListId.length; i < len; ++i) {
        let listObj = document.getElementById(arrListId[i]);
        // 対象の一覧表が未存在（レンダリングされていなければ）ならスキップ
        if (listObj === null) {
            continue;
        }
        // 一覧表のイベントバインド
        bindListCommonEvents(arrListId[i], IsPostBack, true, true, true, true);
        // テキストボックスEnter縦移動イベントバインド
        commonBindEnterToVerticalTabStep();
        // チェックボックス変更
        ChangeCheckBox(arrListId[i]);
        //// チェックボックス変更(Light)
        //ChangeCheckBoxLight(arrListId[i]);
    }

    //bindListCommonEvents(pnlListAreaId, IsPostBack);
    addLeftBoxExtention(leftListExtentionTarget);

    //// チェックボックス
    //ChangeCheckBox();
};

/**
 *  リストテーブルのEnterキーで下のテキストにタブを移すイベントバインド
 * @return {undefined} なし
 * @description 
 */
function commonBindEnterToVerticalTabStep() {
    let generatedTables = document.querySelectorAll("div[data-generated='1']");
    if (generatedTables === null) {
        return;
    }
    if (generatedTables.length === 0) {
        return;
    }
    let focusObjKey = document.forms[0].id + "ListFocusObjId";
    if (sessionStorage.getItem(focusObjKey) !== null) {
        if (IsPostBack === undefined) {
            sessionStorage.removeItem(focusObjKey);
        }
        if (IsPostBack === '1') {
            focusObjId = sessionStorage.getItem(focusObjKey);
            setTimeout(function () {
                document.getElementById(focusObjId).focus();
                sessionStorage.removeItem(focusObjKey);
            }, 10);
        } else {
            sessionStorage.removeItem(focusObjKey);
        }

    }
    for (let i = 0, len = generatedTables.length; i < len; ++i) {
        let generatedTable = generatedTables[i];
        let panelId = generatedTable.id;
        //生成したテーブルオブジェクトのテキストボックス確認
        let textBoxes = generatedTable.querySelectorAll('input[type=text]:not([disabled]):not([disabled=""])');
        //テキストボックスが無ければ次の描画されたリストテーブルへ
        if (textBoxes === null) {
            continue;
        }

        // テキストボックスのループ
        for (let j = 0; j < textBoxes.length; j++) {
            let textBox = textBoxes[j];
            let lineCnt = textBox.attributes.getNamedItem("rownum").value;
            let fieldName = textBox.id.substring(("txt" + panelId).length);
            fieldName = fieldName.substring(0, fieldName.length - lineCnt.length);
            let nextTextFieldName = fieldName;
            if (textBoxes.length === j + 1) {
                // 最後のテキストボックスは先頭のフィールド
                nextTextFieldName = textBoxes[0].id.substring(("txt" + panelId).length);
            } else if (textBoxes.length > j + 1) {
                nextTextFieldName = textBoxes[j + 1].id.substring(("txt" + panelId).length);
            }

            textBox.dataset.fieldName = fieldName;
            textBox.dataset.nextTextFieldName = nextTextFieldName;
            textBox.addEventListener('keypress', (function (textBox, panelId) {
                return function () {
                    if (event.key === 'Enter') {
                        if (commonKeyEnterProgress === false) {
                            commonKeyEnterProgress = true; //Enter連打抑止
                            commonListEnterToVerticalTabStep(textBox, panelId);
                            return setTimeout(function () {
                                commonKeyEnterProgress = false;　///Enter連打抑止
                            }, 10); // 5ミリ秒だと連打でフォーカスパニックになったので10ミリ秒に
                        }
                    }
                };
            })(textBox, panelId), true);
        }
    }
}

// ○チェックボックス表示設定
function ChangeCheckBox(listObjId) {

    var objTable = document.getElementById(listObjId + "_DR").children[0];
    // オブジェクトが存在しない場合抜ける
    if (objTable == undefined) {
        return;
    }

    var chkObjs = objTable.querySelectorAll("input[id^='chkpnlListAreaROWDEL']");
    var spnObjs = objTable.querySelectorAll("span[id^='hchkpnlListAreaROWDEL']");

    for (let i = 0; i < chkObjs.length; i++) {

        if (chkObjs[i] !== null) {
            if (spnObjs[i].innerText == "1") {
                chkObjs[i].checked = true;
            } else {
                chkObjs[i].checked = false;
            }
        }
    }
};

// ○チェックボックス変更
function f_onchange(obj, Line, fieldNM) {
    if (document.getElementById("MF_SUBMIT").value == "FALSE") {

        let trlst = document.getElementById("pnlListArea_DL").getElementsByTagName("tr");
        var objTable = document.getElementById("pnlListArea_DR").children[0];
        // オブジェクトが存在しない場合抜ける
        if (objTable == undefined) {
            return;
        }
        var chkObjs = objTable.querySelectorAll("input[id^='chkpnlListAreaROWDEL']");
        for (let i = 0; i < trlst.length; i++) {
            // イベント発生時のLINECNTと一致する行
            if (trlst[i].getElementsByTagName("th")[0].innerText == Line) {
                if (chkObjs[i].checked == false) {
                    document.getElementById("WF_ButtonALLSELECT").checked = false;
                }

                return;
            }
        }
    }
};

// ○左BOX[テーブル]用処理（DBクリック選択+値反映）
function TableDBclick() {
    if (document.getElementById("MF_SUBMIT").value == "FALSE") {
        document.getElementById("MF_SUBMIT").value = "TRUE"
        if (document.getElementById('WF_SelectedIndex').value != "") {
            document.getElementById('WF_LeftboxOpen').value = "";
            document.getElementById("WF_ButtonClick").value = "WF_ListboxDBclick";
            document.body.style.cursor = "wait";
            document.forms[0].submit();
        }
    };
};
//○左BOX行情報取得処理（行情報退避用）
function Leftbox_Gyou(Line) {
    // Field名退避
    document.getElementById('WF_SelectedIndex').value = Line;
};

// ○Repeater行情報取得処理（行情報退避用）
function Repeater_Gyou(Line) {
    // Field名退避
    document.getElementById('WF_REP_POSITION').value = Line;
};

// ○Repeater処理（スクロール切替）
function f_ScrollTop(x, y) {
    document.all('WF_DViewRep1_Area').scrollTop = x;
    document.all('WF_DViewRep1_Area').scrollLeft = y;

};
// ○DetailBox入力・変更監視処理
function f_Rep1_Change(type) {
    document.getElementById("WF_REP_Change").value = type;
};


// ○ドロップ処理（ドラッグドロップ入力）
function f_dragEvent(e) {
    document.getElementById("WF_MESSAGE").textContent = "ファイルアップロード開始";
    document.getElementById("WF_MESSAGE").style.color = "blue";
    document.getElementById("WF_MESSAGE").style.fontWeight = "bold";

    // ドラッグされたファイル情報を取得
    var files = e.dataTransfer.files;

    // 送信用FormData オブジェクトを用意
    var fd = new FormData();

    // ファイル情報をチェックする
    var csvFlg = [0, 0, 0];
    var xlsCnt = 0;
    var upLoadFile = ""
    for (var i = 0; i < files.length; i++) {
        var f = files[i].name
        if (f.match(/.xls/i) == null && f.match(/.xlsx/i) == null) {
            document.getElementById("WF_MESSAGE").textContent = "「" + f + "」は、アップロードできないファイルです。";
            document.getElementById("WF_MESSAGE").style.color = "red";
            document.getElementById("WF_MESSAGE").style.fontWeight = "bold";
            return false;
        }
        if (f.toLowerCase().match(/^jx/) != null) {
            csvFlg[0] = 1;
        }
        if (f.toLowerCase().match(/^tg/) != null) {
            csvFlg[1] = 1;
        }
        if (f.toLowerCase().match(/^cosmo/) != null) {
            csvFlg[2] = 1;
        }
        if (f.match(/.xls/i) != null || f.match(/.xlsx/i) != null) {
            xlsCnt += 1;
        }
    }
    //Excel/CSVファイルは、複数アップロードできない
    if (files.length > 1) {
        document.getElementById("WF_MESSAGE").textContent = "複数ファイル同時にアップロードできません。";
        document.getElementById("WF_MESSAGE").style.color = "red";
        document.getElementById("WF_MESSAGE").style.fontWeight = "bold";
        return false;
    }
    //Excelとcsvは混在してアップロードできない
    if (xlsCnt >= 1 && (csvFlg[0] == 1 || csvFlg[1] == 1 || csvFlg[2] == 1)) {
        document.getElementById("WF_MESSAGE").textContent = "Excelとcsvは、同時にアップロードできません。";
        document.getElementById("WF_MESSAGE").style.color = "red";
        document.getElementById("WF_MESSAGE").style.fontWeight = "bold";
        return false;
    }
    //特定のcsv何れでもない場合はアップロードできない
    if (xlsCnt == 0 && (csvFlg[0] == 0 && csvFlg[1] == 0 && csvFlg[2] == 0)) {
        document.getElementById("WF_MESSAGE").textContent = "対象外csvファイルは、アップロード出来ません。";
        document.getElementById("WF_MESSAGE").style.color = "red";
        document.getElementById("WF_MESSAGE").style.fontWeight = "bold";
        return false;
    } if (xlsCnt == 1) {
        upLoadFile = "EXCEL"
    }

    // ファイル情報を追加する
    for (var i = 0; i < files.length; i++) {
        fd.append("files", files[i]);
    }

    // XMLHttpRequest オブジェクトを作成
    var xhr = new XMLHttpRequest();
    // 「POST メソッド」「接続先 URL」を指定
    xhr.open("POST", "../GR/GRCO0104XLSUPMULTI.ashx", false)

    // イベント設定
    // ⇒XHR 送信正常で実行されるイベント
    xhr.onload = function (e) {
        if (e.currentTarget.status == 200) {
            document.getElementById('WF_ButtonClick').value = "WF_UPLOAD_" + upLoadFile;
            document.body.style.cursor = "wait";
            commonDispWait();
            document.forms[0].submit();                             //aspx起動
        } else {
            document.getElementById("WF_MESSAGE").textContent = "ファイルアップロードが失敗しました。";
            document.getElementById("WF_MESSAGE").style.color = "red";
            document.getElementById("WF_MESSAGE").style.fontWeight = "bold";
            commonHideWait();
        }
    };

    // ⇒XHR 送信ERRで実行されるイベント
    xhr.onerror = function (e) {
        document.getElementById("WF_MESSAGE").textContent = "ファイルアップロードが失敗しました。";
        document.getElementById("WF_MESSAGE").style.color = "red";
        document.getElementById("WF_MESSAGE").style.fontWeight = "bold";
        commonHideWait();
    };

    // ⇒XHR 通信中止すると実行されるイベント
    xhr.onabort = function (e) {
        document.getElementById("WF_MESSAGE").textContent = "通信を中止しました。";
        document.getElementById("WF_MESSAGE").style.color = "red";
        document.getElementById("WF_MESSAGE").style.fontWeight = "bold";
        commonHideWait();
    };

    // ⇒送信中にタイムアウトエラーが発生すると実行されるイベント
    xhr.ontimeout = function (e) {
        document.getElementById("WF_MESSAGE").textContent = "タイムアウトエラーが発生しました。";
        document.getElementById("WF_MESSAGE").style.color = "red";
        document.getElementById("WF_MESSAGE").style.fontWeight = "bold";
        commonHideWait();
    };

    // 「送信データ」を指定、XHR 通信を開始する
    xhr.send(fd);
}

// ○ディテール(タブ切替)処理
function DtabChange(tabNo) {
    if (document.getElementById("MF_SUBMIT").value === "FALSE") {
        document.getElementById("MF_SUBMIT").value = "TRUE";
        document.getElementById('WF_DTAB_CHANGE_NO').value = tabNo;
        document.getElementById('WF_ButtonClick').value = "WF_DTAB_Click";
        document.body.style.cursor = "wait";
        document.forms[0].submit();
    }
}

