# 0.1.2 下載視窗驗證

官方擴充套件的遠端模式會直接 POST `/api/v1/tasks`。此版本保留地址與 Token，將外部建立請求交給獨立 WinUI 確認視窗；原生前端按開始後才標記為已確認並建立任務。無需修改上游 Gopeed 或重做瀏覽器套件。

實際安裝版驗證：

- 使用與遠端模式相同的 API 請求，確認前任務數 0、按開始後任務數 1。
- 確認視窗可編輯來源、檔名與儲存位置，顯示解析大小；來源欄完整可見。「開始下載」只需一次點擊。
- 取消視窗後沒有建立任務。
- 同一視窗接著顯示下載進度、速度、剩餘時間及來源／儲存位置。
- 真正操作進度視窗的暫停與繼續，API 狀態分別為 `pause`、`running`。
- 關閉主清單後獨立進度窗仍更新；關閉進度窗時進度為 8,290,304 bytes，核心繼續下載到 67,108,864 bytes 並完成。
- 64 MiB 來源與結果 SHA-256 相同：`281E519DF3077B557C6B03F5DA83C4E8D397219259615DD7C3308F89CAE8F2A6`。
- Go 整合測試及 browser confirmation 測試通過；protocol checks 通過；Release 與 WinUI analyzer 建置成功，保留 3 個既有 picker API migration 建議。
- 更新安裝回傳 0；finish review disposition 為 **ship**，本功能沒有剩餘阻擋項目。

驗證透過本機 HTTP fixture、API 請求及真實 WinUI 操作完成；沒有宣稱所有網站、Narrator、高對比或文字縮放均已實測。截圖以 winapp 原生擷取產生，沒有合成 UI。
