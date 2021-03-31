// ○OnLoad用処理(左右Box非表示)
function InitDisplay() {

    // テキストボックスEnter縦移動イベントバインド
    commonBindEnterToVerticalTabStep();

    // 全部消す
    document.getElementById("LF_LEFTBOX").style.width = "0em";
    document.getElementById("RF_RIGHTBOX").style.width = "0em";

    document.getElementById("pnlListArea_DR").scrollLeft = Number(document.getElementById("WF_DISP_SaveX").value);
    document.getElementById("pnlListArea_DR").scrollTop = Number(document.getElementById("WF_DISP_SaveY").value);

    // 左ボックス
    if (document.getElementById("WF_LeftboxOpen").value == "Open") {
        document.getElementById("LF_LEFTBOX").style.width = "26em";
    }

    // 右ボックス
    if (document.getElementById("WF_RightboxOpen").value == "Open") {
        document.getElementById("RF_RIGHTBOX").style.width = "26em";
    }

    // 更新ボタン活性／非活性
    if (document.getElementById("WF_MAPpermitcode").value == "TRUE") {
        // 活性
        document.getElementById("WF_ButtonUPDATE").disabled = "";
    } else {
        // 非活性
        document.getElementById("WF_ButtonUPDATE").disabled = "disabled";
    }

    if (document.getElementById("WF_SELSTAFFCODE").hidden == false) {
        document.getElementById("WF_SELSTAFFCODE_L").style.display = "";
        document.getElementById("WF_SELSTAFFCODE").hidden = "";
        document.getElementById("WF_SELSTAFFCODE_TEXT").style.display = "";
        document.getElementById("WF_ButtonExtract").hidden = "";
    }
    document.getElementById("WF_ButtonUPDATE").hidden = "";
    document.getElementById("WF_ButtonCSV").hidden = "";
    document.getElementById("WF_ButtonPrint").hidden = "";
    document.getElementById("divListArea").style.display = "block";

    // 左ボックス拡張機能追加
    addLeftBoxExtention(leftListExtentionTarget);

    // リストの共通イベント
    bindListCommonEvents(pnlListAreaId, IsPostBack);

};

// 〇数値のみ入力可能
function CheckNum() {
    if (event.keyCode < 48 || event.keyCode > 57) {
        window.event.returnValue = false; // IEだと効かないので↓追加
        event.preventDefault(); // IEはこれで効く
    }
}

// リストの共通イベント
function bindListCommonEvents(listObjId, isPostBack) {

    var listObj = document.getElementById(listObjId);
    // そもそもリストがレンダリングされていなければ終了
    if (listObj == null) {
        return;
    }

    // 横スクロールイベントのバインド
    // 可変列ヘッダーテーブル、可変列データテーブルのオブジェクトを取得
    var headerTableObj = document.getElementById(listObjId + '_HR');
    var dataTableObj = document.getElementById(listObjId + '_DR');
    // 可変列の描画がない場合はそのまま終了
    if (headerTableObj == null || dataTableObj == null) {
        return;
    }

    // スクロールイベントのバインド
    dataTableObj.addEventListener('scroll', (function (listObj) {
        return function () {
            commonListScroll(listObj);
        };
    })(listObj), false);

    // スクロールを保持する場合
    if (isPostBack === '0') {
        // 初回ロード時は左スクロール位置を0とる
        setCommonListScrollXpos(listObj.id, '0');
    }
    // ポストバック時は保持したスクロール位置に戻す
    if (isPostBack === '1') {
        var xpos = getCommonListScrollXpos(listObj.id);
        dataTableObj.scrollLeft = xpos;
        footerTableObj.scrollLeft = xpos;
        var e = document.createEvent("UIEvents");
        e.initUIEvent("scroll", true, true, window, 1);
        dataTableObj.dispatchEvent(e);
        footerTableObj.dispatchEvent(e);
    }

    bindCommonListHighlight(listObj.id);
}

// ○リストデータ部スクロール共通処理（ヘッダー部、フッター部のスクロールを連動させる)
function commonListScroll(listObj) {
    var rightHeaderTableObj = document.getElementById(listObj.id + '_HR');
    var rightDataTableObj = document.getElementById(listObj.id + '_DR');
    var leftDataTableObj = document.getElementById(listObj.id + '_DL');

    setCommonListScrollXpos(listObj.id, rightDataTableObj.scrollLeft);
    rightHeaderTableObj.scrollLeft = rightDataTableObj.scrollLeft;          // 左右連動させる
    leftDataTableObj.scrollTop = rightDataTableObj.scrollTop;               // 上下連動させる
    rightFooterTableObj.scrollLeft = rightDataTableObj.scrollLeft;          // 左右連動させる
}

// ○活性非活性変更
function ChangeDisabled(obj, flag) {
    // オブジェクトが存在しない場合抜ける
    if (obj == undefined) {
        return;
    }

    obj.disabled = flag;
}


// ○リスト内容変更処理
function ListChange(pnlList, Line) {

    if (document.getElementById("MF_SUBMIT").value == "FALSE") {
        document.getElementById("MF_SUBMIT").value = "TRUE";
        document.getElementById("WF_SelectedIndex").value = Line;
        document.getElementById("WF_ButtonClick").value = "WF_ListChange";
        document.getElementById("WF_DISP_SaveX").value = document.getElementById("pnlListArea_DR").scrollLeft;
        document.getElementById("WF_DISP_SaveY").value = document.getElementById("pnlListArea_DR").scrollTop;
        document.body.style.cursor = "wait";
        document.forms[0].submit();
    }
}


// ○左BOX用処理（DBクリック選択+値反映）
function List_Field_DBclick(obj, Line, fieldNM) {

    if (document.getElementById("txtpnlListArea" + fieldNM + Line) != null) {
        if (document.getElementById("txtpnlListArea" + fieldNM + Line).disabled == true) {
            return;
        }
    }

    if (document.getElementById("MF_SUBMIT").value == "FALSE") {
        document.getElementById("MF_SUBMIT").value = "TRUE";
        document.getElementById("WF_FIELD").value = fieldNM;
        document.getElementById("WF_SelectedIndex").value = Line;
        document.getElementById("WF_LeftMViewChange").value = EXTRALIST;
        document.getElementById("WF_LeftboxOpen").value = "Open";
        document.getElementById("WF_ButtonClick").value = "WF_Field_DBClick";
        document.getElementById("WF_DISP_SaveX").value = document.getElementById("pnlListArea_DR").scrollLeft;
        document.getElementById("WF_DISP_SaveY").value = document.getElementById("pnlListArea_DR").scrollTop;
        document.body.style.cursor = "wait";
        document.forms[0].submit();
    }
}

var commonKeyEnterProgress = false; // これは関数(function)外部に設定(グローバルスコープの変数です)

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
/**
 *  リストテーブルのEnterキーで下のテキストにタブを移すイベント
 * @param {Node} textBox テキストボックス
 * @param {string} panelId テキストボックス
 * @return {undefined} なし
 * @description 
 */
function commonListEnterToVerticalTabStep(textBox, panelId) {
    let curLineCnt = Number(textBox.attributes.getNamedItem("rownum").value);
    let fieldName = textBox.dataset.fieldName;
    let nextTextFieldName = textBox.dataset.nextTextFieldName;
    let found = false;
    let focusNode;
    let maxLineCnt = 999; // 無限ループ抑止用の最大LineCntインクリメント
    let targetObjPrefix = "txt" + panelId + nextTextFieldName;
    while (found === false) {
        let targetObj = targetObjPrefix;
        focusNode = document.getElementById(targetObj);
        if (focusNode !== null) {
            found = true;
        } else {
            curLineCnt = curLineCnt + 1;

            targetObjPrefix = "txt" + panelId + nextTextFieldName;
        }

        // 無限ループ抑止
        if (maxLineCnt === curLineCnt) {
            found = true;
        }
    }
    //onchangeイベント（postbackする）を見つけてセッション変数にフォーカス先を保持する（load時にセッション変数からフォーカス先を取得させる）
    //注意）T9では、trタグでonchangeしているため（1行毎、全てのテキスト）判定を止める！！
    //      T9以外で利用する場合、対応が必要かも？
    //var parentNodeObj = textBox.parentNode;
    //if (parentNodeObj.hasAttribute('onchange')) {

    var focusObjKey = document.forms[0].id + "ListFocusObjId";
    sessionStorage.setItem(focusObjKey, focusNode.id);
    //}
    //var retValue = sessionStorage.getItem(forcusObjKey);
    //if (retValue === null) {
    //    retValue = '';
    //}
    focusNode.focus();
    return;
}
