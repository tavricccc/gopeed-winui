# 0.1.1 協定連結修正

使用者從瀏覽器開啟 Gopeed 連結時，Windows 顯示找不到應用程式。0.1.0 的 unpackaged 安裝包沒有註冊 URL 協定，前端也沒有處理傳入參數。

0.1.1 的每使用者安裝包在 `HKCU\Software\Classes\gopeed` 註冊連結，透過 `"Gopeed.Native.exe" "%1"` 啟動。前端接收啟動與 single-instance redirect 的參數，以官方 v1.9.3 `/create`、`/extension` 及 base64 JSON 格式處理，使用 UTF-8 解碼並保留請求資料。連結開啟既有 WinUI 對話框，讓使用者確認後再下載；擴充功能也不會因連結直接安裝。

已驗證：

- 解析測試：命令列引號、UTF-8 中文檔名、query escaping、HTTP headers、空 `/create`、`/extension`、base64url 與錯誤參數。
- 透過 Windows Shell 真正啟動 `gopeed:///create?params=…`，冷啟動顯示原生新增下載對話框。
- 已開啟狀態再次啟動連結，PID 維持 32544、前端程序數為 1；UIA 與本機截圖確認網址、中文檔名正確。
- 安裝程式回傳 0，解除安裝登錄項目的 DisplayVersion 為 0.1.1，協定命令指向目前使用者的安裝 EXE。
- Release 與 Microsoft WinUI analyzer 建置成功；有 3 個既有 WUI1001 file-picker API migration 建議，沒有錯誤。

測試連結使用 example.com 假檔案，沒有按開始下載。這輪驗證的是 Windows 協定啟動與資料交接，不代表所有網站、瀏覽器擴充套件直連模式都已驗證。

參考：[Microsoft URI activation](https://learn.microsoft.com/en-us/windows/apps/develop/launch/handle-uri-activation)、固定上游的 `upstream/ui/flutter/lib/app/modules/app/controllers/app_controller.dart`。
