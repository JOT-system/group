﻿// ○OnLoad用処理（左右Box非表示）
function InitDisplay() {

    // 全部消す
    document.getElementById("LF_LEFTBOX").style.width = "0em";
    document.getElementById("RF_RIGHTBOX").style.width = "0em";

    if (document.getElementById('WF_LeftboxOpen').value == "Open") {
        document.getElementById("LF_LEFTBOX").style.width = "26em";
    };



    addLeftBoxExtention(leftListExtentionTarget);

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

};

// ○ドロップ処理（ドラッグドロップ入力）
function f_dragEvent(e, kbn) {
    document.getElementById("WF_MESSAGE").textContent = "ファイルアップロード開始";
    document.getElementById("WF_MESSAGE").style.color = "blue";
    document.getElementById("WF_MESSAGE").style.fontWeight = "bold";

    // ドラッグされたファイル情報を取得
    var files = e.dataTransfer.files;

    // 送信用FormData オブジェクトを用意
    var fd = new FormData();

    // ファイル情報を追加する
    for (var i = 0; i < files.length; i++) {
        fd.append("files", files[i]);
    }

    // XMLHttpRequest オブジェクトを作成
    var xhr = new XMLHttpRequest();

    // ドロップファイルによりURL変更
    if (files[0].type == "application/pdf") {
        // 「POST メソッド」「接続先 URL」を指定
        xhr.open("POST", "../GR/GRCO0101PDFUP.ashx", false)

        // イベント設定
        // ⇒XHR 送信正常で実行されるイベント
        xhr.onload = function (e) {
            if (e.currentTarget.status == 200) {
                document.getElementById('WF_ButtonClick').value = "WF_EXCEL_UPLOAD";
                document.getElementById("WF_EXCEL_UPLOAD").value = "PDF_LOADED";
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

    } else {
        // 「POST メソッド」「接続先 URL」を指定
        xhr.open("POST", "../GR/GRCO0100XLSUP.ashx", false)

        // イベント設定
        // ⇒XHR 送信正常で実行されるイベント
        xhr.onload = function (e) {
            if (e.currentTarget.status == 200) {

                document.getElementById('WF_ButtonClick').value = "WF_EXCEL_UPLOAD";
                if (kbn == "FILE_UP") {
                    document.getElementById("WF_EXCEL_UPLOAD").value = "XLS_SAVE";
                } else {
                    document.getElementById("WF_EXCEL_UPLOAD").value = "XLS_LOADED";
                }
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
    // 上位領域へのイベント伝搬不要
    event.stopPropagation();
};

