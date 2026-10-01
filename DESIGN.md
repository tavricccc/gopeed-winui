---
name: Gopeed Native
description: 以 Windows Fluent 呈現下載工作佇列的原生 WinUI 3 介面
colors:
  icon-blue: "#0067C0"
  icon-white: "#FFFFFF"
typography:
  title:
    fontFamily: XamlAutoFontFamily
    fontSize: 28dip
    fontWeight: 600
  subtitle:
    fontFamily: XamlAutoFontFamily
    fontSize: 20dip
    fontWeight: 600
  body:
    fontFamily: XamlAutoFontFamily
    fontSize: 14dip
    fontWeight: 400
  body-strong:
    fontFamily: XamlAutoFontFamily
    fontSize: 14dip
    fontWeight: 600
  caption:
    fontFamily: XamlAutoFontFamily
    fontSize: 12dip
    fontWeight: 400
rounded:
  control: 4dip
  overlay: 8dip
spacing:
  tight: 4dip
  row: 6dip
  inline: 8dip
  control: 12dip
  form: 14dip
  section: 16dip
  extensions: 20dip
  page: 24dip
components:
  button-primary:
    rounded: "{rounded.control}"
    padding: 11dip 5dip 11dip 6dip
  download-row:
    padding: "{spacing.control}"
  details-pane:
    width: 280dip
  download-search:
    width: 240dip
---

# Design System: Gopeed Native

## Overview

**Creative North Star: "Windows 下載工作佇列"**

以 Windows Fluent 原生控制項呈現下載狀態、進度與下一步操作。畫面資訊密度服務於工作佇列：上方導覽、平面下載清單、依選取項目啟用的命令與詳細資訊；不加入儀表板數字或裝飾性卡片。

介面由 WinUI 3 的 `XamlControlsResources`、`ThemeResource` 與控制項範本建立。繁體中文標籤與清楚的文字狀態是主要資訊，圖示及系統重點色輔助辨識。主題、字型、焦點、停用與浮出視窗外觀優先交由平台處理。

**Key Characteristics:**

- 原生 Fluent、Mica 視窗背景與頂部導覽。
- 平面清單、可讀的進度與文字狀態。
- 單一系統重點色，淺色與深色同步。
- 窄視窗保留操作，以對話框顯示完整詳情。

記錄依據：0.3.0 的 `src/Gopeed.Native` 原生 WinUI 3 實作與 WinUI NuGet 套件 `Microsoft.WindowsAppSDK.WinUI` 2.3.9 的 `lib/native/Microsoft.UI/Themes/generic.xaml`。有效截圖已保存於 `docs/images/`，含最終主清單、確認、完成，以及同版設定、商店與解壓完成畫面；個別建置範圍見 `docs/verification-0.3.0.md`。0.2.0 的 ship 結論不當作這一版的新驗證。

0.3.0 已核對官方 native host base64 create 到確認窗、取消不建立任務、ZIP 開始與解壓檔 hash、來源更新保留同一任務 ID 與已下載 bytes；UIA 對 NumberBox 輸入 Enter 提交後，API config.maxRunning 為 256。本機測試擴充功能的普通複製、設定與停用，以及啟動自動續傳、每日 Tracker 更新與 Webhook 已測。這些是指定流程的證據，不代表真實 BT／eD2k、第三方網站、Windows 分享目標、Narrator、高對比、跨應用程式拖出或 Windows 10 已驗證；不宣稱 RAM 節省。最終解除安裝與發布驗證由交付紀錄另行追蹤。

## Colors

介面色彩的規範來源是 WinUI 主題資源。前置資料只列出應用程式圖示真正固定的色彩，不能將它們當作整個介面的固定配色。

### Primary

- **系統重點色**：`AccentButtonStyle`、原生 `ProgressBar`、導覽選取指示與焦點狀態採平台資源。使用者的 Windows 重點色與淺深主題會改變實際色值。
- **圖示藍與白**：前置資料的 `icon-blue`、`icon-white` 僅用於程式生成的下載箭頭圖示；介面按鈕不硬編碼成圖示藍。

### Neutral

- **主要文字**：使用控制項預設前景。
- **次要文字**：`TextFillColorSecondaryBrush` 用於說明、傳輸量、速度、詳情欄位名稱與底部摘要。
- **命令列層**：`LayerFillColorDefaultBrush` 提供輕微層次。
- **透明清單內容**：`ControlFillColorTransparentBrush` 保留 `ListViewItem` 原生 hover、選取與焦點呈現。
- **完成提示**：獨立進度窗的完成勾號使用 `SystemFillColorSuccessBrush`；狀態仍以「下載完成」文字明示。
- **視窗標題列**：caption 按鈕前景隨實際主題切換白／黑，高對比時程式採系統前景色；背景透明，hover 背景由目前主題選擇深灰／淺灰。

**The Theme Resource Rule.** 新增介面應沿用平台資源，不從截圖取色後覆蓋 WinUI 的完整狀態範本。所有 `ContentDialog` 經 `NativeDialogs.ShowAsync` 將 `RequestedTheme` 同步為視窗實際主題。

## Typography

字型使用 WinUI 的 `XamlAutoFontFamily`，依 Windows 與文字語系選擇字型。不要固定中文替代字型或把畫面文字轉為圖片。前置資料記錄目前 SDK 資源的 DIP 基準，升級 SDK 時應重新核對，不能獨立於平台資源維護另一套數值。

| 角色 | 實作樣式 | 用途 |
| --- | --- | --- |
| title | `TitleTextBlockStyle` | 下載、設定、擴充功能頁標題 |
| subtitle | `SubtitleTextBlockStyle` | 詳細資訊、設定區段、擴充功能名稱、空佇列標題 |
| body | 原生 `TextBlock` 預設／`BodyTextBlockStyle` | 狀態、欄位標籤、說明、來源與路徑 |
| body-strong | `BodyStrongTextBlockStyle` | 清單檔名、選取下載名稱 |
| caption | `CaptionTextBlockStyle` | 傳輸量、速度、底部摘要 |

清單檔名以 `CharacterEllipsis` 截斷；詳情中的檔名、來源、路徑可換行，路徑及來源可選取複製。沒有自訂行高或字距。按鈕內容以 16 DIP `FontIcon` 與文字並排、間距 8 DIP；`NativeButtons` 同步設定 AutomationProperties.Name。清單檔案類型圖示為 24 DIP，進度窗狀態圖示 28 DIP，空佇列下載圖示 40 DIP；圖示皆來自 WinUI 字型系統。`DownloadPresentation.FileGlyph` 依副檔名分壓縮檔、安裝檔、音訊、影片、圖片及一般文件，不載入每個檔案的 Shell 縮圖。

## Layout

所有尺寸以 DIP 表示，截圖像素不能直接當版面 token。視窗預設 1120 × 740 DIP，建立時依 DPI 轉換為像素；最小視窗 640 × 480 DIP。

- 頂部是原生 `TitleBar` 與 `NavigationView`，`PaneDisplayMode="Top"`。下載、擴充功能與內建設定入口共用導覽列。
- 頁面內容外距為左／右 24、上 8、下 16 DIP；錯誤 `InfoBar` 位於內容上方。
- 下載頁區段間距 14 DIP。標題和新增按鈕左右排列；篩選、排序與搜尋在下一列，前兩欄等分剩餘寬度、最小寬 132 DIP，搜尋固定 240 DIP。
- 清單與詳情相隔 24 DIP。頁寬達 1000 DIP 時顯示 280 DIP 右側詳情欄；低於門檻收合側欄，由「詳細資訊」命令開啟可捲動 `ContentDialog`，並非另建下方詳情卡片。
- 下載清單為原生 `ListView`，Extended 多選、容器 padding 12 DIP；每列檔名與狀態、進度、傳輸量與速度共三列，列距 6 DIP、欄距 16 DIP，右側狀態／速度欄寬 115 DIP。
- 清單佔剩餘空間，底部並排摘要與背景下載說明。第一個 accent 選取動作最小寬 140 DIP，與右側 CommandBar 相距 12 DIP；CommandBar 啟用 dynamic overflow 承載次要操作。
- 設定欄位區最大寬 760 DIP、分區間距 16 DIP，Expander 內部間距 12 DIP，只有一般預設展開；內容捲動，「儲存設定」位於獨立底列，保持可達。欄寬依可用頁寬縮減，避免水平捲動。
- 擴充功能欄位區最大寬 800 DIP、間距 16 DIP；商店與已安裝使用 Pivot，列表項目相距 20 DIP，各項內部 8／10 DIP，命令按鈕間距 8 DIP。「從其他來源安裝」另置 Expander。
- 共用 DownloadForm 內容最小寬 420 DIP、間距 14 DIP；檔案選取清單最大高 180 DIP。獨立確認窗預設 640 × 540 DIP，最小 540 × 500 DIP，建立時置中並限制於工作區；轉進度頁後保留目前寬度，高度改為 480 DIP、最小高度 400 DIP。確認及進度頁外距 24 DIP、區段距 20 DIP，上方內容可捲動，底列按鈕固定可達。

## Elevation & Depth

主視窗使用原生 `MicaBackdrop`，深度由平台材質、主題層與原生控制項呈現。下載列沒有自訂陰影。`ContentDialog` 與 `MenuFlyout` 的遮罩、陰影、邊框及浮出層由 WinUI 範本負責，不能依單張截圖推導出自訂 shadow token。沒有自訂動畫時長或 easing token。

## Shapes

WinUI 基本控制項使用 `ControlCornerRadius`，浮出層使用 `OverlayCornerRadius`；前置資料列出目前 SDK 基準。保留原生 TextBox、Button、ComboBox、Expander 與 ContentDialog 形狀，不把所有容器改為同一個巨大圓角。下載列保持平面且使用原生選取底色；它不是卡片元件。

## Components

瀏覽器下載使用獨立原生視窗。來源使用單行欄位、檔名與位置可編輯，解析大小後由使用者按「開始下載」；「直接開始下載」可略過解析，但仍由開始按鈕提交。確認頁直接填滿視窗，使用 ApplicationPageBackgroundThemeBrush，底部按鈕靠右。DownloadForm UserControl 與主清單新增對話框共用；主清單保留多行與 torrent 選擇，加入分類、最近連結與檔案全選／取消全選。精簡確認頁隱藏 torrent 選擇和最近連結，把直接開始選項放入請求選項；較早同版確認圖仍顯示移動前的位置，不作為最後位置證據。

開始後同一視窗改為進度頁，第一個 accent 按鈕依任務狀態變化：running／wait／ready 為「暫停下載」、pause 為「繼續下載」、error 為「重試下載」，一般 done 為「開啟檔案」。解壓處理期間主動作停用且標示「正在解壓縮」；解壓成功且原壓縮檔已刪除時改為「開啟解壓縮資料夾」。底列另有儲存資料夾／在資料夾中顯示、做種時才顯示的「停止做種」與關閉。

下載 done 不立即停止輪詢：解壓中或等待分卷時仍顯示處理進度，等待分卷採 indeterminate；做種中持續顯示已上傳量與速度。只有 done 且沒有解壓處理、沒有做種時才隱藏進度條並停止一秒輪詢。解壓失敗保留原檔可開啟並明示錯誤；來源 Expander 在下載失敗時展開。完成且不在解壓處理時，檔名可拖出 OpenPath 對應的檔案／資料夾，以 Copy 交給其他應用程式。關閉停止該窗輪詢，背景核心繼續。

「開啟檔案後關閉此視窗」只在完成時顯示，預設勾選；在使用者實際按開啟且成功後關閉，不在下載完成時自動關閉。設定頁另有「記住上次使用的下載位置」與「開啟檔案後關閉下載視窗」CheckBox。`UiPreferences` 保存這些偏好。記住位置時，成功建立下載後儲存最後資料夾；下次表單優先載入最後資料夾，外部請求明確指定位置時再覆蓋。最近連結去重保存最多30筆。

主清單可從剪貼簿文字、拖入 WebLink／文字連結或 torrent 檔案開啟新增表單；文字接受 HTTP、HTTPS、magnet、file 絕對 URI。儲存項目拖入目前只接受 torrent。清單命令與進度窗詳情可複製下載連結。這些入口都先讓使用者確認，不把一般檔案拖入誤當下載來源。

外部 create protocol、待處理下載請求與官方瀏覽器 native host 接管先開獨立確認窗，不強制喚起主清單；一般啟動與 extension 路由仍開主視窗。安裝程式提供原生 host，設定頁有啟用接管入口與可收合的遠端 HTTP／Token 設定。activation 錯誤在主窗回報；視窗保留強引用，最後視窗關閉時解除單一實例註冊。主窗與下載窗共用已保存主題及部署 AppIcon。

- **主要按鈕**：原生 Button + `AccentButtonStyle`，用於新增下載、安裝擴充功能、儲存設定。hover、pressed、focus、disabled 全由原生範本。新增下載在核心未連線時停用。
- **命令列與內容功能表**：原生 CommandBar、AppBarButton、MenuFlyout。單選提供當前主動作與資料夾、詳情；多選主動作優先暫停可暫停項目，其次繼續可續傳項目，其餘開啟儲存資料夾。批次暫停、繼續、移除、複製連結與清除完成紀錄可達；需單一任務的詳情與來源更新在多選時停用。無選取時相關命令停用。
- **下載列**：原生 ListViewItem、文字狀態與 ProgressBar。狀態包含下載中、已完成、做種中、已暫停、下載失敗、等待中、準備中。未知大小且正在下載時進度為 indeterminate；不單靠顏色區分狀態。完成列雙擊開啟檔案，其餘雙擊顯示詳情。
- **欄位**：原生 TextBox、AutoSuggestBox、ComboBox、NumberBox、PasswordBox、ToggleSwitch、CheckBox 與 Expander。欄位有 Header 或文字標籤；搜尋提示為「檔名或網址」。Token 使用 PasswordBox，可暫時顯示及複製。
- **新增下載與選項**：原生 ContentDialog／獨立 Page 共用表單，保留逐步解析與批次建立。Advanced options 放在「請求與下載選項」Expander：HTTP 方法與內容、標頭、憑證選項、連線數、Tracker、代理與認證、Torrent 自動下載、解壓與密碼。代理欄位只在自訂模式啟用；PasswordBox 呈現密碼。忙碌停用主按鈕、顯示 24 DIP ProgressRing，InfoBar 錯誤保留輸入。
- **移除下載**：原生 ContentDialog，顯示數量與最多五個名稱，提醒未結束的下載／做種會停止；是否同時刪檔沿用偏好，預設按鈕為取消。清除完成紀錄保留檔案，排除做種與解壓中的任務。
- **排序與分類**：排序 ComboBox 提供最新、最舊、檔名、大小與進度。分類在設定中維護，表單有可用分類時顯示 ComboBox；最近連結以原生選單提供，保留使用者可編輯的輸入。
- **設定分區**：原生 Expander 按一般、分類、壓縮檔／Torrent、HTTP、BT、eD2k、鏡像、代理、瀏覽器接管、完成動作與關於分區，保留單一儲存動作。NumberBox 最多同時下載與 HTTP 連線數為 256。自動續傳、每日 Tracker 更新、Webhook 與完成執行程式以可見設定控制。
- **擴充功能**：商店／已安裝 Pivot；商店搜尋名稱／描述，依收藏、安裝或更新排序，分頁「載入更多」，已安裝項目的安裝按鈕停用。已安裝提供啟用 ToggleSwitch、設定、檢查更新、解除安裝；錯誤用 InfoBar，失敗商店可重新載入。
- **任務詳情與分享**：TaskDetailsDialog 使用資訊、檔案、連線 Pivot（最高 440 DIP、最小寬 420 DIP）；資訊與路徑可選取，檔案清單選取後提供開啟、定位與分享。開啟／分享需完成、不在解壓且檔案存在；連線顯示 HTTP 連線進度或 BT 統計。分享呼叫 Windows 原生分享介面，目標流程未實測。
- **更新失效來源**：HTTP 暫停／失敗任務可在對話框更新 URL 與 HTTP 標頭，預設更新後繼續；也可等待下一個瀏覽器連結，InfoBar 明示等待對象與取消入口。更新使用原任務 ID，與另建任務的「重新下載」區分。
- **空清單與回饋**：沒有可見下載時顯示下載圖示、標題與下一步文字。設定儲存／複製 Token 成功使用 Success InfoBar；頁面/API 錯誤使用 Error InfoBar。
- **鍵盤**：Ctrl+N 新增、Ctrl+F 搜尋、F5 重新整理；焦點在清單時 Ctrl+A 全選、Delete 開移除確認。關鍵欄位與清單已有 AutomationId，保留 WinUI 焦點呈現；不等同 Narrator 或高對比驗證。

**圖像來源**：`scripts/create_icon.py` 使用 Pillow 程式繪製下載箭頭與圓角藍底，經 4× 繪製及 Lanczos 縮圖生成 `AppIcon.ico` 與六個 PNG（Square44 主圖及 24／48 targetsize、Square150、StoreLogo、LockScreenLogo）。沒有外部圖庫或 AI 生成圖片。`SplashScreen.scale-200.png` 與 `Wide310x150Logo.scale-200.png` 來自官方 `Microsoft.WindowsAppSDK.WinUI.CSharp.Templates`：`winapp` 0.7.0 執行 `new --name Gopeed.Native --template winui-mvvm --template-version latest --use-defaults`；保留為 Content，目前 unpackaged 介面未使用。打包 WinUI 的 Mica noise 屬於平台 runtime 素材，不是產品繪圖。

## Do's and Don'ts

### Do:

- Do 沿用原生 WinUI 控制項、ThemeResource 與完整互動狀態。
- Do 用繁體中文文字描述狀態、錯誤與操作結果。
- Do 在窄視窗保留詳情命令與原生對話框。
- Do 將來源、路徑和長檔名放在可換行、可選取的詳情內。
- Do 保留圖示生成程式與範本素材來源；實測記憶體後才報告用量。

### Don't:

- Don't 用硬編碼配色取代系統重點色或深淺主題。
- Don't 增加裝飾性卡片、儀表板摘要來取代下載工作佇列。
- Don't 移植 Flutter／網頁控制項或原 Gopeed 按鈕位置作為版面約束。
- Don't 將截圖中曾出現的主題缺陷寫成規範，也不要宣稱尚未實測的 Narrator、高對比或 RAM 節省。
