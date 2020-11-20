// ○OnLoad用処理（左右Box非表示）
function InitDisplay() {
};

function OpenClose() {
    if (document.getElementById("WF_WARNNING").value == "◀ 車検、気密検査、容器検査") {
        document.getElementById("msgbox").classList.remove("msgbox");
        document.getElementById("msgbox").classList.add("msgboxZoom");
        document.getElementById("WF_WARNNING").value = "▶ 車検、気密検査、容器検査";
        document.getElementById("WF_GUID").value = "▶ 運用ガイダンス";
    } else {
        document.getElementById("msgbox").classList.add("msgbox");
        document.getElementById("msgbox").classList.remove("msgboxZoom");
        document.getElementById("WF_WARNNING").value = "◀ 車検、気密検査、容器検査";
        document.getElementById("WF_GUID").value = "◀ 運用ガイダンス";
    }
};
