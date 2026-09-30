# 0.1.3 確認視窗與 HTTP 標頭修正

獨立確認視窗不再放入 ContentDialog，而是直接顯示 DownloadConfirmationPage。主清單對話框和獨立頁面共用 DownloadForm，保留檢查、檔案選擇、位置與建立下載邏輯。確認頁使用單一主題背景與原生欄位，底部開始／取消固定可達，移除內層外框及周圍白色留邊。

原 HTTP 標頭欄位只分割 LF；WinUI 的 CR 換行會讓多個標頭併入第一個標頭值，產生 Go `invalid header field value`。HttpHeaders.Parse 同時處理 CR、LF、CRLF，保留 Sec-Ch-Ua 的引號、Cookie 與 Referer 的原值。

已驗證：

- CR／LF／CRLF 測試：每種換行均解析出 3 個標頭，Sec-Ch-Ua 引號、Referer、Cookie 值保持正確，值中沒有換行殘留。
- 真正透過 WinUI 確認表單，帶入 Sec-Ch-Ua、Referer、Accept-Encoding，解析並開始使用者的 VirtualBox 7.2.20 Windows 安裝檔下載。
- API 保存了 3 個標頭，Sec-Ch-Ua 內容正確，全部值中沒有 CR／LF。
- 下載完成：178,192,488 bytes。SHA-256 為 `a81777d2b36380ce042a29e9c554cf032eb46a793f62e3cc82e7411e535c2c26`，與官方 [SHA256SUMS](https://download.virtualbox.org/virtualbox/7.2.20/SHA256SUMS) 相同。
- 主清單新增對話框可開啟、載入預設 Downloads 路徑，並正常取消。
- Release／Microsoft WinUI analyzer 建置成功；3 個既有 picker migration 建議，沒有錯誤。
- 安裝更新成功，限定 finish review 判定 **ship**。

實測檔案另存於專案 work/header-verification，驗證後只移除該測試任務與檔案。未執行下載的 EXE。原本使用者的 VirtualBox 下載維持保留。
