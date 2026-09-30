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
  settings: 18dip
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

記錄依據：`src/Gopeed.Native` 實作、WinUI NuGet 套件 `Microsoft.WindowsAppSDK.WinUI` 2.3.9 的 `lib/native/Microsoft.UI/Themes/generic.xaml`，以及 `.impeccable/review/` 的 desktop、dark、compact、settings、extensions、add-dialog、delete-dialog 圖片。圖片用於確認構圖與可讀性；此文件不是 Narrator、高對比或記憶體效能的測試報告。

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

清單檔名以 `CharacterEllipsis` 截斷；詳情中的檔名、來源、路徑可換行，路徑及來源可選取複製。沒有自訂行高或字距。新增下載的加號為 16 DIP `FontIcon`，空佇列下載圖示為 40 DIP；其餘 `SymbolIcon`／`FontIcon` 使用 WinUI 圖示系統。

## Layout

所有尺寸以 DIP 表示，截圖像素不能直接當版面 token。視窗預設 1120 × 740 DIP，建立時依 DPI 轉換為像素；最小視窗 640 × 480 DIP。

- 頂部是原生 `TitleBar` 與 `NavigationView`，`PaneDisplayMode="Top"`。下載、擴充功能與內建設定入口共用導覽列。
- 頁面內容外距為左／右 24、上 8、下 16 DIP；錯誤 `InfoBar` 位於內容上方。
- 下載頁區段間距 14 DIP。標題和新增按鈕左右排列；篩選與搜尋在下一列，搜尋固定 240 DIP，篩選最小寬 160 DIP。
- 清單與詳情相隔 24 DIP。頁寬達 1000 DIP 時顯示 280 DIP 右側詳情欄；低於門檻收合側欄，由「詳細資訊」命令開啟可捲動 `ContentDialog`，並非另建下方詳情卡片。
- 下載清單為原生 `ListView`，單選、容器 padding 12 DIP；每列檔名與狀態、進度、傳輸量與速度共三列，列距 6 DIP、欄距 16 DIP，右側狀態／速度欄寬 115 DIP。
- 清單佔剩餘空間，底部並排摘要與背景下載說明。原生 CommandBar overflow 承載次要操作。
- 設定欄位區最大寬 760 DIP、間距 18 DIP，內容捲動；「儲存設定」位於獨立底列，保持可達。
- 擴充功能欄位區最大寬 800 DIP、間距 20 DIP，已安裝項目相距 16 DIP，各項內部 10 DIP，命令按鈕間距 8 DIP。
- 新增下載內容最小寬 420 DIP、間距 14 DIP，捲動區最大高 500 DIP；檔案選取清單最大高 180 DIP。

## Elevation & Depth

主視窗使用原生 `MicaBackdrop`，深度由平台材質、主題層與原生控制項呈現。下載列沒有自訂陰影。`ContentDialog` 與 `MenuFlyout` 的遮罩、陰影、邊框及浮出層由 WinUI 範本負責，不能依單張截圖推導出自訂 shadow token。沒有自訂動畫時長或 easing token。

## Shapes

WinUI 基本控制項使用 `ControlCornerRadius`，浮出層使用 `OverlayCornerRadius`；前置資料列出目前 SDK 基準。保留原生 TextBox、Button、ComboBox、Expander 與 ContentDialog 形狀，不把所有容器改為同一個巨大圓角。下載列保持平面且使用原生選取底色；它不是卡片元件。

## Components

- **主要按鈕**：原生 Button + `AccentButtonStyle`，用於新增下載、安裝擴充功能、儲存設定。hover、pressed、focus、disabled 全由原生範本。新增下載在核心未連線時停用。
- **命令列與內容功能表**：原生 CommandBar、AppBarButton、MenuFlyout；暫停只對可暫停狀態啟用，繼續／重試只對暫停與錯誤狀態啟用，開啟檔案只對完成項目啟用。沒有選取時，選取項目的命令停用。
- **下載列**：原生 ListViewItem、文字狀態與 ProgressBar。狀態包含下載中、已完成、做種中、已暫停、下載失敗、等待中、準備中。未知大小且正在下載時進度為 indeterminate；不單靠顏色區分狀態。完成列雙擊開啟檔案，其餘雙擊顯示詳情。
- **欄位**：原生 TextBox、AutoSuggestBox、ComboBox、NumberBox、PasswordBox、ToggleSwitch、CheckBox 與 Expander。欄位有 Header 或文字標籤；搜尋提示為「檔名或網址」。Token 使用 PasswordBox，可暫時顯示及複製。
- **新增下載**：原生 ContentDialog；單一連結先「檢查連結」，顯示解析預覽與可選檔案，再「開始下載」。多行連結主按鈕改為「開始 N 個下載」。忙碌時停用主按鈕並顯示 24 DIP ProgressRing；錯誤在內部 InfoBar 呈現，保留輸入。
- **移除下載**：原生 ContentDialog，清楚顯示下載名稱；刪除已下載檔案的 CheckBox 預設未勾選，預設按鈕是取消。
- **空清單與回饋**：沒有可見下載時顯示下載圖示、標題與下一步文字。設定儲存／複製 Token 成功使用 Success InfoBar；頁面/API 錯誤使用 Error InfoBar。
- **鍵盤**：Ctrl+N 新增、Ctrl+F 聚焦搜尋、F5 重新整理。關鍵新增欄位與清單已有 AutomationId；這是實作事實，不等同於完整 Narrator 或高對比驗證。

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
