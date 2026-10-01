# 0.4.0 驗證紀錄

環境：Windows 11 x64、WinUI 3、192 DPI（200% 縮放）。本版替換下載清單、確認與進度視窗的布局，核心仍固定 Gopeed v1.9.3。

## 本版實測

- Go 核心與 Native Messaging host 必要檢查通過；C# 協定、UTF-8/Base64、HTTP CR/LF/CRLF、Sec-Ch-Ua 引號及狀態動作檢查通過。WinUI Release publish 成功。
- 官方 Native Messaging 的四位元組長度框架、ping 及 Base64 create 請求通過。已安裝版能開啟獨立確認視窗；確認前沒有建立任務，取消不會下載。這是 host 協定與註冊實測，未重新測試商店擴充套件的實際點擊接管流程。
- 64 MiB 本機 HTTP ZIP 經實際 WinUI 按鈕開始、暫停及續傳，完成檔案 SHA-256 與來源一致。請求包含 Cookie、Referer、Sec-Ch-Ua 及換行，沒有 invalid header。
- 完成後第一個主要按鈕為「開啟檔案」。透過 UI Automation 呼叫該按鈕，沿用「開啟後關閉」偏好，下載視窗確實關閉。
- 以 Win32 `GetClientRect` 與 `GetDpiForWindow` 讀取，瀏覽器確認內容 560×311 DIP、手動確認 560×375 DIP；展開下載選項上限為 560×480 DIP，內容可捲動。下載中及暫停為 520×208 DIP，完成為 520×195 DIP。
- `WS_EX_TOPMOST` 確認期間為 true，開始下載後及完成時為 false。確認與進度都不是固定的大視窗。
- 最小主視窗 640×480 DIP 的 UI Automation 顯示：大小欄已隱藏，檔名保留 175 DIP，下載列高 44 DIP；每列開啟按鈕寬 32 DIP、高 28 DIP。
- 窄視窗的 CommandBar 將設定移入「其他選項」；實際呼叫後可開設定並返回清單。手動新增仍有 Torrent、最近連結與進階下載選項。
- 瀏覽器更新來源的確認仍能操作；實際選擇「更新並繼續」後保留同一任務 ID 與已下載位元組，沒有新增第二個任務，完成檔案雜湊也相符。設定、擴充功能、返回清單的入口透過 UI Automation 呼叫成功。
- 0.3.0 → 0.4.0 實際靜默升級退出碼 0，登錄 DisplayVersion 為 0.4.0；已安裝前端 DLL、核心、host 與圖示的 SHA-256 逐一與 portable 產物相符。Chrome、Edge、Firefox 本機 host 登錄都存在。
- 本次自己建立的測試任務移除後，升級前原有四個任務 ID 皆保留。偏好與主題還原原本設定，保留下載檔案。

## 視覺驗收尚待完成

本次 Windows 處於鎖定／安全桌面，工具明確回報無互動桌面；PrintWindow 只取得空白內容與標題列，畫面截圖不能當成有效證據。獨立 Impeccable reviewer 的 disposition 是 **recapture**，不是視覺通過。

解鎖後仍須擷取並檢查本版實際淺色／深色、預設／最小主視窗、確認、展開選項、下載中／暫停／完成、設定與擴充功能畫面，再進行完整視覺審查。UI Automation 的尺寸與操作證據不能替代字級、對比、裁切及參考一致性的圖像驗收。本版以 prerelease 發布，沒有宣稱視覺驗收完成。

## 其他範圍

既有解壓縮、本機擴充管理、設定儲存、啟動續傳、Tracker、Webhook 與解除安裝的 0.3.0 測試，見 [0.3.0 驗證紀錄](verification-0.3.0.md)；不能視為本版全部重新測過。真實 BT/eD2k、第三方網站解析、Narrator、高對比、跨應用程式拖放與 Windows 10 仍未完成跨環境實測。

縮小介面不是 RAM 基準測試。本版沒有宣稱與官方前端的同工作量 RAM 比較結果。
