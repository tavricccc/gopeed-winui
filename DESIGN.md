---
name: Gopeed Native
description: 以精簡單列與內容量測視窗呈現下載操作的原生 WinUI 3 介面
colors:
  icon-blue: "#0067C0"
  icon-white: "#FFFFFF"
typography:
  title:
    fontFamily: XamlAutoFontFamily
    fontSize: 20dip
    fontWeight: 600
  subtitle:
    fontFamily: XamlAutoFontFamily
    fontSize: 16dip
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
  row: 6dip
  inline: 8dip
  group: 10dip
  control: 12dip
  form: 14dip
  section: 16dip
  page: 20dip
  table-inset: 24dip
components:
  button-primary:
    rounded: "{rounded.control}"
  row-primary-button:
    rounded: "{rounded.control}"
    padding: 8dip 6dip
  row-action-button:
    backgroundColor: transparent
    rounded: "{rounded.control}"
    padding: 8dip 6dip
  download-row:
    padding: 12dip 6dip
  download-size-column:
    width: 140dip
  download-progress-column:
    width: 152dip
  download-speed-column:
    width: 88dip
  download-actions-column:
    width: 100dip
  download-filter-sort:
    width: 140dip
  download-confirmation:
    width: 560dip
  download-progress:
    width: 520dip
  download-footer:
    padding: 20dip 12dip
---

# Design System: Gopeed Native

## Overview

**Creative North Star: "Windows 下載工作佇列"**

以 Windows Fluent 原生控制項呈現下載狀態、進度與下一步操作。單列清單讓檔名、傳輸量、進度／狀態、速度及動作沿同一條水平線掃讀；CommandBar 直接承載新增、批次操作、設定與擴充功能。完整詳情按需開啟，不佔常駐版面。

介面由 WinUI 3 的 `XamlControlsResources`、`ThemeResource` 與控制項範本建立。繁體中文標籤與清楚的文字狀態是主要資訊，圖示及系統重點色輔助辨識。確認窗與進度窗各有內容寬度，依可見欄位量測高度；細線底列承載下一步操作。主題、字型、焦點、停用與浮出視窗外觀交由平台處理，沒有裝飾性動畫或攝影素材。

**Key Characteristics:**

- 原生 Fluent、主視窗 Mica 與直接操作的 CommandBar。
- 44 DIP 最小列高，資訊與三個操作在單列內對齊。
- 每列第一個按鈕使用系統重點色，狀態決定暫停、繼續、重試或開啟。
- 確認與進度視窗依內容量測高度，進階內容按需展開。
- 窄視窗隱藏大小欄；完整來源、路徑與檔案詳情按需開啟。

記錄依據：0.4.0 的 `src/Gopeed.Native` 原生 WinUI 3 實作、`.impeccable/compact-native-direction.md` 與 WinUI NuGet 套件 `Microsoft.WindowsAppSDK.WinUI` 2.3.9 的 `lib/native/Microsoft.UI/Themes/generic.xaml`。視覺參考是 `C:\Users\Tavric\projects\downlism\src\Downlism.App` 的 `MainWindow.xaml`、`NewDownloadWindow.xaml` 與 `NewDownloadWindow.xaml.cs`：原生命令列、單列清單、標籤／欄位對齊、細線底列與內容量測。Downlism 網站的字級與版面不屬於這個系統。

本文件描述完成的程式碼狀態，並非視覺驗收。0.4.0 已有 UI Automation 操作、Win32 client size／`WS_EX_TOPMOST`、下載 SHA-256 與安裝升級證據；本次 Windows 處於鎖定／安全桌面，沒有有效新版截圖，獨立 reviewer 的 disposition 為 **recapture**。待解鎖後重新擷取淺／深色、主窗預設／最小尺寸、確認／展開選項、下載中／暫停／完成及設定／擴充功能，再作完整視覺審查。詳見 [0.4.0 驗證紀錄](docs/verification-0.4.0.md)；舊版截圖與 ship 結論不移作本版驗收。

## Colors

介面色彩的規範來源是 WinUI 主題資源。前置資料只列出應用程式圖示真正固定的色彩，不能將它們當作整個介面的固定配色。

### Primary

- **系統重點色**：`AccentButtonStyle`、每列 `RowPrimaryButtonStyle`、原生 `ProgressBar` 與焦點狀態採平台資源。CommandBar 的新增下載使用 `AccentTextFillColorPrimaryBrush`。使用者的 Windows 重點色與淺深主題會改變實際色值。
- **圖示藍與白**：前置資料的 `icon-blue`、`icon-white` 僅用於程式生成的下載箭頭圖示；介面按鈕不硬編碼成圖示藍。

### Neutral

- **主要文字**：使用控制項預設前景。
- **次要文字**：`TextFillColorSecondaryBrush` 用於欄名、傳輸量、檔案圖示、次要動作、說明與底部摘要。
- **下載視窗背景**：`ApplicationPageBackgroundThemeBrush` 統一確認與進度頁背景。
- **操作底列**：`LayerFillColorDefaultBrush` 與 `DividerStrokeColorDefaultBrush` 提供細線分隔及輕微層次；CommandBar 背景透明。
- **透明清單內容**：`ControlFillColorTransparentBrush` 保留 `ListViewItem` 原生 hover、選取與焦點呈現。
- **完成提示**：以「下載完成」文字與「開啟檔案」主按鈕明示，不新增固定成功色。
- **主視窗標題列**：caption 按鈕前景隨實際主題切換白／黑，高對比時程式採系統前景色；背景透明，hover 背景由目前主題選擇深灰／淺灰。下載視窗使用標準系統標題列。

**The Theme Resource Rule.** 新增介面應沿用平台資源，不從截圖取色後覆蓋 WinUI 的完整狀態範本。所有 `ContentDialog` 經 `NativeDialogs.ShowAsync` 將 `RequestedTheme` 同步為視窗實際主題。

## Typography

字型繼承 WinUI 的 `XamlAutoFontFamily`，依 Windows 與文字語系選擇字型。不要固定中文替代字型或把畫面文字轉為圖片。前置資料記錄 `App.xaml` 的精簡標題樣式與目前原生文字基準；升級 SDK 時應重新核對繼承值。

| 角色 | 實作樣式 | 用途 |
| --- | --- | --- |
| title | `CompactPageTitleStyle` | 設定、擴充功能頁標題；主清單直接以命令列開始 |
| subtitle | `CompactSectionTitleStyle` | 擴充功能名稱等區段標題 |
| body | 原生 `TextBlock` 預設／`BodyTextBlockStyle` | 狀態、欄位標籤、說明、來源與路徑 |
| body-strong | 14 DIP `SemiBold`／`BodyStrongTextBlockStyle` | 進度窗檔名及需強調的名稱；主清單檔名使用原生一般文字 |
| caption | `SecondaryTextBlockStyle`／`CaptionTextBlockStyle` | 欄名、進度文字、底部摘要與進度窗量測資訊 |

清單及進度窗檔名以 `CharacterEllipsis` 截斷並提供完整 Tooltip；進度窗位置同樣截斷。詳情中的長內容可換行，來源網址在 Flyout 內可選取複製。沒有自訂行高或字距。每列三個操作使用 14 DIP `FontIcon`，檔案類型圖示 16 DIP；`NativeButtons` 的文字按鈕使用 16 DIP 圖示與 8 DIP 間距。空佇列圖示為 32 DIP、標題 18 DIP。圖示來自 WinUI 字型系統，`DownloadPresentation.FileGlyph` 依副檔名區分檔案類型，不載入 Shell 縮圖；操作皆提供 AutomationProperties.Name。

**The Compact Hierarchy Rule.** 頁標題與區段標題使用前置資料的精簡層級，下載清單不另加入大型頁首；狀態與摘要以文字維持掃讀。

## Layout

所有尺寸以 DIP 表示，截圖像素不能直接當版面 token。主視窗預設 960 × 560 DIP，建立時依 DPI 轉換為像素；最小視窗 640 × 480 DIP。獨立下載窗的寬度則指 `ResizeClient` 的內容寬度。

- 頂部是原生 `TitleBar` 及透明 `CommandBar`，命令標籤位於圖示右側，啟用 dynamic overflow。設定與擴充功能在同一個 Frame 顯示，CommandBar 出現「下載清單」返回入口。
- 篩選與排序各占固定欄寬，搜尋使用中間剩餘空間；左右外距沿用頁面 token，欄距 10 DIP。錯誤 `InfoBar` 位於命令列下方。
- 表頭與下載列使用同一組欄寬：檔名占剩餘寬度，其餘欄寬見前置資料。欄距 12 DIP，檔案圖示與檔名間距 8 DIP。大小、速度靠右對齊；進度條和文字在同一欄。
- 原生 `ListView` 使用 Extended 多選；每列最小高 44 DIP，容器 padding 見前置資料，三個操作相距 2 DIP。表格 Grid 可用寬度低於 720 DIP 時將大小欄寬改為 0 並隱藏該欄，保留檔名、狀態、速度與操作。
- 清單占剩餘高度，底部只保留摘要。完整資訊透過「詳細資訊」或內容功能表的原生 `ContentDialog` 開啟。
- 設定欄位區最大寬 760 DIP、分區間距 16 DIP，Expander 內部間距 12 DIP，只有一般預設展開；內容捲動，「儲存設定」位於獨立底列，保持可達。欄寬依可用頁寬縮減，避免水平捲動。
- 擴充功能欄位區最大寬 800 DIP、間距 16 DIP；商店與已安裝使用 Pivot，列表項目相距 20 DIP，各項內部 8／10 DIP，命令按鈕間距 8 DIP。「從其他來源安裝」另置 Expander。
- `DownloadForm` 以 44 DIP 標籤欄與剩餘欄位並排，欄距 12 DIP、列距 10 DIP，位置欄另有「瀏覽…」。一般來源可多行，高度 64–110 DIP；瀏覽器確認來源為單行。檔案選取清單最大高 160 DIP。
- 確認窗在工作區置中、不可最大化／最小化，確認期間置頂。確認與進度頁內容外距為 20／16／20／14 DIP；底列使用前置資料的 padding，以 1 DIP 原生分隔線區隔。上方內容可捲動，底列按鈕靠右。
- 確認內容寬度與進度內容寬度見前置資料；高度由 `MeasureContentHeight`／`PreferredHeight` 量測，目前限制在 150–480 DIP 並受工作區約束。量測後固定大小，展開選項或狀態變化會重新量測。確認不使用固定高度；開始後解除置頂並允許最小化。
- 0.4.0 在 200% 縮放的 Win32 實測 client size：瀏覽器確認 560 × 311、手動確認 560 × 375、展開選項 560 × 480、下載中／暫停 520 × 208、完成 520 × 195 DIP。這些是特定內容的量測證據，不是新畫面應硬編碼的高度 token。

**The Content Measurement Rule.** 確認與進度窗依可見內容重新量測 client height；進階欄位收合、來源細節浮出，不為尚未使用的內容預留大面積。

## Elevation & Depth

主視窗使用原生 `MicaBackdrop`；確認與進度頁是統一背景加細線底列。深度由平台材質、主題層與原生控制項呈現，下載列沒有自訂陰影。`ContentDialog`、`Flyout` 與 `MenuFlyout` 的遮罩、陰影、邊框及浮出層由 WinUI 範本負責，不能依單張截圖推導出自訂 shadow token。狀態更新、收合與浮出使用原生行為，沒有自訂動畫時長或 easing token。

## Shapes

WinUI 基本控制項使用 `ControlCornerRadius`，浮出層使用 `OverlayCornerRadius`；前置資料列出目前 SDK 基準。保留原生 TextBox、Button、ComboBox、Expander 與 ContentDialog 形狀，不把所有容器改為同一個巨大圓角。下載列保持平面且使用原生選取底色；它不是卡片元件。

## Components

瀏覽器接管與主清單新增都使用獨立原生 `DownloadWindow`，共用 `DownloadForm`。瀏覽器確認來源為單行、隱藏 Torrent 選擇與最近連結；手動新增保留多行來源、Torrent、分類、最近連結及檔案全選／取消全選。檔名與位置可編輯，單一來源先解析大小，再由使用者按「開始下載」；「直接開始下載」位於預設收合的「下載選項」，略過解析後仍須按開始提交。確認前不建立任務，取消關閉視窗。

開始後同一視窗改為進度頁，第一個 accent 按鈕依任務狀態變化：running／wait／ready 為「暫停下載」、pause 為「繼續下載」、error 為「重試下載」，一般 done 為「開啟檔案」。解壓處理期間主動作停用且標示「正在解壓縮」；解壓成功且原壓縮檔已刪除時改為「開啟解壓縮資料夾」。底列另有儲存資料夾／在資料夾中顯示、做種時才顯示的「停止做種」與關閉。

進度頁只保留協定、檔名、位置、傳輸量／速度／剩餘時間、進度及狀態。來源網址藏在 14 DIP 省略圖示的 `Flyout`，內容寬 380 DIP、最大高 220 DIP，可選取來源及複製網址。未知大小隱藏進度條，以狀態文字回饋；等待分卷與解壓狀態仍持續輪詢，不另設 indeterminate。完成且沒有解壓處理時隱藏進度條；只有完成、沒有解壓處理且沒有做種時才停止一秒輪詢。做種仍顯示已上傳量、速度及「停止做種」。解壓失敗明示原始檔案仍可開啟。完成且不在解壓處理時，檔名可拖出 OpenPath 對應的檔案／資料夾，以 Copy 交給其他應用程式。關閉停止該窗輪詢，背景核心繼續。

「開啟後關閉」只在完成、沒有解壓處理及做種時顯示，預設勾選；在使用者實際按開啟且成功後關閉，不在下載完成時自動關閉。設定頁另有「記住上次使用的下載位置」與「開啟檔案後關閉下載視窗」CheckBox。`UiPreferences` 保存這些偏好。記住位置時，成功建立下載後儲存最後資料夾；下次表單優先載入最後資料夾，外部請求明確指定位置時再覆蓋。最近連結去重保存最多 30 筆。

主清單可從剪貼簿文字、拖入 WebLink／文字連結或 Torrent 檔案開啟新增表單；文字接受 HTTP、HTTPS、magnet、eD2k、file 絕對 URI。儲存項目拖入目前只接受 Torrent。清單命令與進度窗來源 Flyout 可複製下載連結。這些入口都先讓使用者確認，不把一般檔案拖入誤當下載來源。

外部 create protocol、待處理下載請求與官方瀏覽器 native host 接管先開獨立確認窗，不強制喚起主清單；一般啟動與 extension 路由仍開主視窗。安裝程式提供原生 host，設定頁有啟用接管入口與可收合的遠端 HTTP／Token 設定。activation 錯誤在主窗回報；視窗保留強引用，最後視窗關閉時解除單一實例註冊。主窗與下載窗共用已保存主題及部署 AppIcon。

- **主要按鈕**：原生 Button + `AccentButtonStyle`，用於每列主動作、確認開始、進度動作、安裝擴充功能及儲存設定。每列主按鈕的 padding 見前置資料；資料夾與移除按鈕為透明背景、無額外邊框、次要前景。hover、pressed、focus、disabled 沿用原生範本。CommandBar 新增與貼上於核心未連線時停用。
- **命令列與內容功能表**：原生 CommandBar、AppBarButton、MenuFlyout。主要命令有新增、貼上、全部暫停／繼續、單選詳情、擴充功能及設定；overflow 保留選取暫停／繼續、來源更新、重新下載、複製、移除、全選、清除完成與重新整理。需要單一任務的詳情與來源更新在多選時停用。每列亦有相同狀態主動作、資料夾、詳情與來源更新功能表。
- **下載列**：原生 ListViewItem、文字狀態與 ProgressBar。第一個 accent 按鈕按狀態對應開啟／暫停／繼續／重試，後接資料夾與移除。狀態包含下載中、已完成、做種中、已暫停、下載失敗、等待中、準備中；已知大小且未完成、未失敗才顯示 determinate 進度條。不單靠顏色區分狀態。完成列雙擊開啟檔案，其餘雙擊顯示詳情。
- **欄位**：原生 TextBox、AutoSuggestBox、ComboBox、NumberBox、PasswordBox、ToggleSwitch、CheckBox 與 Expander。欄位有 Header 或文字標籤；搜尋提示為「搜尋檔名或來源」。Token 使用 PasswordBox，可暫時顯示及複製。
- **新增下載與選項**：獨立 Page 共用表單，保留逐步解析與批次建立。進階選項放在預設收合的「下載選項」Expander：直接開始、連線數、HTTP 標頭、HTTP 方法與內容、憑證選項、Tracker、代理與認證、Torrent 自動下載、解壓與密碼。代理欄位只在自訂模式啟用；PasswordBox 呈現密碼。忙碌停用主按鈕、顯示 18 DIP ProgressRing，InfoBar 錯誤保留輸入。
- **移除下載**：原生 ContentDialog，顯示數量與最多五個名稱，提醒未結束的下載／做種會停止；是否同時刪檔沿用偏好，預設按鈕為取消。清除完成紀錄保留檔案，排除做種與解壓中的任務。
- **排序與分類**：排序 ComboBox 提供最新、最舊、檔名、大小與進度。分類在設定中維護，表單有可用分類時顯示 ComboBox；最近連結以原生選單提供，保留使用者可編輯的輸入。
- **設定分區**：原生 Expander 按一般、分類、壓縮檔／Torrent、HTTP、BT、eD2k、鏡像、代理、瀏覽器接管、完成動作與關於分區，保留單一儲存動作。NumberBox 最多同時下載與 HTTP 連線數為 256。自動續傳、每日 Tracker 更新、Webhook 與完成執行程式以可見設定控制。
- **擴充功能**：商店／已安裝 Pivot；商店搜尋名稱／描述，依收藏、安裝或更新排序，分頁「載入更多」，已安裝項目的安裝按鈕停用。已安裝提供啟用 ToggleSwitch、設定、檢查更新、解除安裝；錯誤用 InfoBar，失敗商店可重新載入。
- **任務詳情與分享**：TaskDetailsDialog 使用資訊、檔案、連線 Pivot（最高 440 DIP、最小寬 420 DIP）；資訊與路徑可選取，檔案清單選取後提供開啟、定位與分享。開啟／分享需完成、不在解壓且檔案存在；連線顯示 HTTP 連線進度或 BT 統計。分享呼叫 Windows 原生分享介面，目標流程未實測。
- **更新失效來源**：HTTP 暫停／失敗任務可在對話框更新 URL 與 HTTP 標頭，預設更新後繼續；也可等待下一個瀏覽器連結，InfoBar 明示等待對象與取消入口。更新使用原任務 ID，與另建任務的「重新下載」區分。
- **空清單與回饋**：沒有可見下載時顯示下載圖示、標題與下一步文字。設定儲存／複製 Token 成功使用 Success InfoBar；頁面/API 錯誤使用 Error InfoBar。
- **鍵盤**：Ctrl+N 新增、Ctrl+F 搜尋、F5 重新整理；焦點在清單時 Ctrl+A 全選、Delete 開移除確認。確認窗 Escape 取消、Ctrl+Enter 檢查連結或開始下載。關鍵欄位與清單已有 AutomationId，保留 WinUI 焦點呈現；不等同 Narrator 或高對比驗證。

**圖像來源**：`scripts/create_icon.py` 使用 Pillow 程式繪製下載箭頭與圓角藍底，經 4× 繪製及 Lanczos 縮圖生成 `AppIcon.ico` 與六個 PNG（Square44 主圖及 24／48 targetsize、Square150、StoreLogo、LockScreenLogo）。沒有外部圖庫或 AI 生成圖片。`SplashScreen.scale-200.png` 與 `Wide310x150Logo.scale-200.png` 來自官方 `Microsoft.WindowsAppSDK.WinUI.CSharp.Templates`：`winapp` 0.7.0 執行 `new --name Gopeed.Native --template winui-mvvm --template-version latest --use-defaults`；保留為 Content，目前 unpackaged 介面未使用。打包 WinUI 的 Mica noise 屬於平台 runtime 素材，不是產品繪圖。

## Do's and Don'ts

### Do:

- Do 沿用原生 WinUI 控制項、ThemeResource 與完整互動狀態。
- Do 用繁體中文文字描述狀態、錯誤與操作結果。
- Do 保持單列欄位對齊，窄視窗隱藏大小欄並保留操作。
- Do 使用內容量測調整下載視窗高度，將進階內容收合。
- Do 將來源、路徑和長檔名放在可換行、可選取的詳情內。
- Do 保留圖示生成程式與範本素材來源；實測記憶體後才報告用量。

### Don't:

- Don't 用硬編碼配色取代系統重點色或深淺主題。
- Don't 恢復大型頁首、三列下載項目、常駐詳情欄或固定大進度窗。
- Don't 增加裝飾性卡片、儀表板摘要來取代下載工作佇列。
- Don't 將 Downlism 網站的展示版面帶入原生操作介面。
- Don't 移植 Flutter／網頁控制項或原 Gopeed 按鈕位置作為版面約束。
- Don't 將截圖中曾出現的主題缺陷寫成規範，也不要宣稱尚未實測的 Narrator、高對比或 RAM 節省。
