# 0.2.0 操作設計依據

本輪保留 WinUI 3 原生控制項，參考下載器的工作流程，重新安排操作優先順序。

| 官方來源 | 採用的設計 |
| --- | --- |
| [IDM 完成視窗](https://www.internetdownloadmanager.com/support/using_idm/completeD.html) | 完成後優先開啟檔案；資料夾定位在旁；提供完成檔案拖出與開啟後關閉選項。 |
| [IDM 主介面](https://www.internetdownloadmanager.com/support/main.html) | 主要下載動作、清楚狀態、清單操作及來源管理。 |
| [IDM 開始下載](https://help.internetdownloadmanager.com/support/using_idm/starting.html) | 開始前確認檔案與位置；本版只實作確認後立即開始。 |
| [AB Download Manager](https://github.com/amir1376/ab-download-manager) | 保留搜尋、篩選、批次與瀏覽器整合的效率；排程／佇列管理留待後續。 |
| [Microsoft Segoe Fluent Icons](https://learn.microsoft.com/en-us/windows/apps/design/iconography/segoe-fluent-icons-font) | 使用原生圖示區分檔案類型與命令，並保留文字標籤。 |
| [Microsoft 拖放](https://learn.microsoft.com/en-us/windows/apps/develop/data/drag-and-drop) | DataPackage 傳輸來源／StorageItems，非同步取得資料時使用 deferral。 |

實作在 DownloadPresentation 集中狀態規則，NativeButtons 處理原生按鈕標籤與圖示，FileActions 管理開啟／定位／複製，UiPreferences 管理少量偏好。主清單與獨立下載窗共用規則，避免兩處操作順序不同。

不複製其他下載器的按鈕位置、品牌素材或畫面。應用程式圖示由 scripts/create_icon.py 程式繪製；來源與 WinUI 平台資源詳見 DESIGN.md。
