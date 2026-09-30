# 瀏覽器下載視窗 · 0.2.0

Mode: Operate。延伸既有 Windows Fluent 設計，以原生 WinUI 控制項實作。

THESIS: 捕捉到的下載先讓使用者確認，再於同一個精簡視窗追蹤進度。
FIRST VIEWPORT: 確認頁提供單行來源網址、位置、可編輯檔名、解析大小、請求選項與開始／取消。進度頁保留底列操作；完成時第一個 accent 按鈕是「開啟檔案」，其次「在資料夾中顯示」與「關閉」。
INTERACTION: 瀏覽器請求與 create protocol 先開確認窗，不直接建立下載，也不強制喚起主清單。取消關閉；確認開始才建立任務。進度窗獨立擁有 API client 與一秒輪詢生命週期，關閉停止前端輪詢，背景核心繼續下載。
FORM: Windows Fluent Page 與共用 DownloadForm UserControl，採 TextBox、Expander、ProgressBar、InfoBar、TextBlock、CheckBox 和 Button。確認頁填滿單一主題背景，外距 24 DIP、區段距 20 DIP；內容可捲動，底列固定。WindowAppearance 統一載入主題與部署目錄 AppIcon.ico。
FINISH: 依實作來源記錄；有效截圖與互動驗證另行列明，不能將空白截圖或未執行操作當作完成證據。

## 狀態與主動作

| 核心狀態 | 第一個 accent 按鈕 | 畫面行為 |
| --- | --- | --- |
| running／wait／ready | 暫停下載 | 顯示進度、傳輸量、速度與剩餘時間 |
| pause | 繼續下載 | 保留進度與位置 |
| error | 重試下載 | 自動展開來源與詳細資訊 |
| done | 開啟檔案 | 顯示完成勾號、大小與位置；隱藏進度條、停止輪詢 |

主清單與右鍵第一個命令共用 DownloadPresentation 狀態映射。清單以 24 DIP FontIcon 區分壓縮檔、安裝檔、音訊、影片、圖片及一般文件；不是外部縮圖。

## 偏好與檔案操作

「開啟檔案後關閉此視窗」只在完成時顯示，預設勾選，開啟成功才關閉；完成本身不會自動關閉視窗。設定頁提供記住下載位置與開啟後關閉進度窗的 CheckBox；UiPreferences 將偏好存入 preferences.json。記住下載位置預設開啟，成功建立後保存最後位置；表單載入時先用核心預設，再用記住的位置，最後套用外部請求明確指定的位置。

完成檔名可向外拖曳檔案或資料夾，DataPackage 使用 Copy；來源 Expander 可複製下載連結。主清單可從剪貼簿文字及拖入 WebLink／文字連結／torrent 開啟新增表單。文字連結接受 HTTP、HTTPS、magnet、file 絕對 URI；StorageItems 只接受 torrent。這些入口不繞過確認步驟。

## 尺寸與來源

確認窗預設 640 × 540 DIP、最小 540 × 500 DIP，置於工作區中央；開始後保留寬度，高度改為 480 DIP、最小高度 400 DIP。主窗維持 1000 DIP 詳情側欄門檻及 640 × 480 DIP 最小尺寸。

來源：DownloadWindow、DownloadConfirmationPage、DownloadForm、DownloadProgressPage、MainPage、DownloadPresentation、UiPreferences、NativeButtons、WindowAppearance、FileActions 與 App activation 分支。有效 0.2.0 主清單截圖為 work/v02-main-visible.png，已檢視圖示與未選取主動作；其他舊 v02 完成／暫停／確認截圖為空白，未採用。完成／暫停互動、新確認畫面、Narrator、高對比與記憶體效能目前只記錄實作，尚未在此文件宣稱通過驗證。
