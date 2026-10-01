# Gopeed v1.9.3 功能對照

比較基準是上游 v1.9.3 的 Windows Flutter 前端與 API，固定 commit `a5cd53f94c18ac65add684b1113fa5f0b47cc4da`。Gopeed Native 0.4.0 使用同一核心，以下區分「有原生操作入口」與「已完成端到端實測」。

## 下載與檔案

| 原版功能 | 0.2.0 狀態 | 0.4.0 原生入口及行為 |
| --- | --- | --- |
| HTTP／HTTPS、磁力、Torrent | 已有基本新增 | 新增表單、拖放、剪貼簿、Torrent 選擇器；Installer 可註冊為 Windows 預設程式的候選 |
| eD2k | 核心支援，入口不完整 | 新增、剪貼簿與協定啟動；設定提供 TCP／UDP 及伺服器來源 |
| 批次建立 | 多行逐一建立 | 多行使用 `tasks/batch`；每筆保留同一組請求選項 |
| 解析大小、檔名、多檔選擇 | 基本列表 | 含路徑與大小的可虛擬化清單、全選／取消全選；原版資料夾樹改為平面路徑清單 |
| 直接下載 | 無獨立控制 | 新增的直接開始選項、預設設定；瀏覽器請求仍須確認 |
| HTTP 連線、標頭、方法與內容 | 只有連線和標頭 | 請求選項完整提供；保留 Cookie、Referer、Sec-Ch-Ua 引號 |
| 每筆代理、略過憑證驗證、Tracker | 缺入口 | 請求選項提供繼承／直連／自訂代理、憑證及 Tracker |
| 每筆自動 Torrent、解壓縮與密碼 | 核心支援，缺入口 | 新增表單提供繼承／啟用／停用、解壓縮密碼與刪除原檔選項 |
| 分類與最近連結 | 只有記住資料夾 | 設定維護分類及常用分類；新增表單選擇分類、使用／清除最近連結 |
| 暫停、繼續、重試 | 單筆及清單中全部 | Ctrl／Shift 多選、選取項批次操作；「全部」不受搜尋或篩選限制 |
| 搜尋、狀態篩選、排序 | 搜尋及篩選 | 新增日期方向、名稱、大小、進度排序 |
| 移除並選擇保留檔案 | 單筆 | 批次確認、記住保留選項，預設保留；另有清除全部完成紀錄 |
| 修改失效連結 | 缺入口 | 暫停或失敗的 HTTP 任務可修改網址及標頭後繼續，保留同一任務 |
| 等待下一個瀏覽器連結更新來源 | 缺入口 | 任務內容功能表啟用等待；新連結詢問更新原任務／建立新下載／取消 |
| 重新下載 | 缺入口 | 命令列更多操作重新開啟原請求與選項，先確認再建立新任務 |
| 檔案清單、開啟、定位、分享 | 僅主檔開啟與定位 | 詳細資訊的檔案分頁、選取檔案、原生 Windows 分享 |
| HTTP 連線資訊、BT peers／分享量 | 核心支援，缺入口 | 詳細資訊的連線分頁；做種顯示上傳速度、上傳量及停止入口 |
| 解壓縮進度／失敗／分卷等待 | 缺入口 | 清單與獨立進度窗顯示，尚未結束時持續輪詢；原檔刪除後主要操作改開啟解壓縮資料夾 |
| 獨立確認及進度窗 | 已有 | 官方本機與遠端請求共用確認流程，確認前不建立任務；主清單可保持關閉 |

## 設定與系統整合

| 原版功能 | 0.4.0 入口／實際執行位置 |
| --- | --- |
| 下載位置、同時下載數、HTTP 預設 | 一般及 HTTP 設定；同時下載及 HTTP 連線上限 256 |
| HTTP User-Agent、伺服器檔案時間 | HTTP 設定，交由上游執行 |
| BT 監聽、Tracker、自動訂閱更新 | BT 設定，手動更新；背景服務每日更新並合併自訂 Tracker |
| BT 做種、分享比例、時間 | BT 設定；上游執行做種，時間以分鐘輸入、秒儲存 |
| eD2k TCP／UDP、伺服器、server.met、nodes.dat | eD2k 設定；上游執行 |
| 系統／直連／自訂代理及帳密 | 代理設定；上游 HTTP 與代理取得 API 執行 |
| 自動 Torrent、完成後刪除 Torrent | 壓縮檔與 Torrent 設定；上游執行 |
| 自動解壓縮、成功後刪除壓縮檔 | 壓縮檔與 Torrent 設定；上游執行 |
| 自動移除已不存在的完成檔案 | 一般設定；上游執行 |
| 開啟時繼續下載、登入時啟動 | 一般設定；前者由背景服務執行，後者註冊目前使用者 Run，不常駐 WinUI 視窗 |
| 完成通知 | 一般設定；背景服務透過系統匣通知，實際顯示仍受 Windows 通知設定影響 |
| Webhook 及測試 | 完成後的動作；上游執行並提供測試按鈕 |
| 完成或失敗後執行程式／指令檔 | 完成後的動作；上游執行 |
| GitHub 鏡像 | 設定可維護 Proxy／jsDelivr 清單及優先順序；用於 Tracker 訂閱、安裝程式更新下載。未複製原版競速選擇多個鏡像行為 |
| 檢查／通知新版本 | 關於的手動檢查及開啟時檢查；下載本 fork 安裝程式至獨立確認／進度窗，完成後由使用者開啟安裝 |
| Torrent／magnet 預設程式 | 註冊 Windows 應用程式能力，按鈕開啟預設應用程式設定；選擇預設程式由 Windows 與使用者處理 |
| 瀏覽器接管 | Installer 註冊官方 `com.gopeed.gopeed` 本機 host，Chrome、Edge、Firefox；支援 ping／wakeup／base64 create／API forward；不需 HTTP 位址或 Token |
| 遠端連線 | 設定的進階展開區可取得位址／Token及設定本機連接埠；保持官方遠端請求相容 |
| 系統匣與背景下載 | 關閉所有視窗仍下載；系統匣可開啟介面或停止服務 |
| 主題與記錄 | 跟隨 Windows／淺色／深色；關於可開啟 logs 資料夾 |

## 擴充功能

| 原版功能 | 0.4.0 入口 |
| --- | --- |
| 商店、搜尋、排序、分頁 | 擴充功能商店；名稱／描述搜尋，收藏／安裝／更新排序，載入更多 |
| 安裝來源與子目錄 | 商店安裝及其他來源；保留 `repo#directory` 格式 |
| 本機資料夾 | 其他來源的選擇資料夾；複製為一般安裝，不啟用開發模式 |
| 已安裝列表、啟用、設定、更新、移除 | 已安裝分頁，原生型別欄位；檢查新版本後詢問更新 |
| metadata、網站與說明 | 名稱、作者、版本、描述、收藏／安裝數；詳細介紹連至來源，網站連至 homepage |
| 內嵌 README 與商店卡片圖示 | 本版使用來源介紹連結和文字列表，未嵌入 Markdown README 或來源圖示 |
| JavaScript 開發模式與原始碼編輯 | 未移植至產品介面；可在開發工具及上游 API 使用，非一般安裝功能 |

## 明確差異與驗證界線

- 目前原生文字為繁體中文；尚未提供原版完整多語系切換。
- 原版的網頁登入、Web UI、多平台行動介面、macOS 選單列屬其他平台，此 Windows 前端不承接。沒有新增使用資料收集。
- 排程器不是 v1.9.3 的既有功能，本次沒有宣稱新增排程器。
- 原版資料夾樹、內嵌 README、鏡像競速及擴充開發模式採上述不同處理；不能稱為逐畫面或逐選項完全相同。
- 入口與對應資料契約經原始碼比對和編譯，HTTP、來源更新、解壓縮、設定儲存、本機擴充管理、Native Messaging、啟動續傳、Tracker 訂閱、Webhook 另有實測。真實 BT／eD2k 網路、第三方網站解析、Windows 分享目標、通知顯示政策及無障礙仍需跨環境驗證。

## 對照來源

- [上游 API 與 Flutter 頁面](https://github.com/GopeedLab/gopeed/tree/v1.9.3/ui/flutter/lib)
- [上游 REST 路由](https://github.com/GopeedLab/gopeed/blob/v1.9.3/pkg/rest/api.go)
- [上游下載設定模型](https://github.com/GopeedLab/gopeed/blob/v1.9.3/pkg/base/model.go)
- [官方瀏覽器擴充套件](https://github.com/GopeedLab/browser-extension)
- [Windows 原生分享介面](https://learn.microsoft.com/en-us/windows/apps/develop/windows-integration/integrate-sharesheet-send)
- [Chrome Native Messaging 格式與註冊](https://developer.chrome.com/docs/extensions/develop/concepts/native-messaging)
- [Edge Native Messaging 與雙商店 extension ID](https://learn.microsoft.com/en-us/microsoft-edge/extensions/developer-guide/native-messaging)

實際安裝、測試與未驗證項目見 [0.4.0 驗證紀錄](verification-0.4.0.md)。
