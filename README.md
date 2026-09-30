# Gopeed Native

Gopeed 下載引擎的 Windows 原生前端。介面使用 WinUI 3、繁體中文與系統主題，沒有 Flutter 或 WebView。這是獨立專案，並非 Gopeed 官方版本。

## 使用

執行安裝程式即可安裝至目前使用者，不需要管理員權限；也可解壓縮 portable ZIP 執行 `Gopeed.Native.exe`。目前提供 Windows x64 版本。

新增單一連結時先檢查來源，再選擇目的地與檔案；貼上多行連結可批次開始下載。支援暫停／繼續、搜尋與狀態篩選、原生檔案與資料夾選擇器、Torrent 檔案選擇、連線數／HTTP headers、擴充功能與代理設定。窄視窗可從命令列開啟詳細資訊。

關閉視窗後，獨立 Go 核心會留在系統匣繼續下載。系統匣可重新開啟介面或停止核心；設定頁也提供結束操作。Ctrl+N 新增、Ctrl+F 搜尋、F5 更新清單。

下載資料與 API Token 位於 `%LOCALAPPDATA%\GopeedNative`，與官方 Gopeed 的資料分開。解除安裝會停止本專案核心並保留資料與下載檔案。若需要完全清除，可在結束核心後手動刪除這個資料夾。

瀏覽器接管使用 Gopeed 官方擴充套件。設定頁提供套件入口、`http://127.0.0.1:18762` 與可複製的持久 API Token，將它們填入擴充套件連線設定。實際瀏覽器接管、BT/eD2k 網路傳輸與第三方擴充套件尚未完成端到端驗證。

安裝版 0.1.1 會註冊目前使用者的 `gopeed://` 協定。網站或擴充套件使用官方 `gopeed:///create?params=…` 連結時，將網址、檔名、目的地與 HTTP 標頭帶入原生新增下載視窗，確認後才開始下載。`gopeed:///extension?params=…` 可帶入擴充功能 repository，仍需手動確認安裝。已開啟的前端會接手連結，不另開第二個視窗。Portable 不主動修改 Windows 協定註冊。

## 開發

安裝 Go 1.24.9 以上、.NET 10 SDK 與 PowerShell 7；製作安裝包另外需要 Inno Setup 6，預設尋找目前使用者的安裝位置。首次取得專案需執行 `git submodule update --init --recursive`。建置前先關閉本專案前端與核心，避免正在執行的檔案被鎖定。

```powershell
pwsh -File scripts/build.ps1 -Test
pwsh -File scripts/package.ps1
```

建置產物位於 `artifacts/`。前端採自包含部署，不要求使用者另外安裝 .NET 或 Windows App SDK runtime。

- `upstream/`：固定 Gopeed v1.9.3，commit `a5cd53f94c18ac65add684b1113fa5f0b47cc4da`，不改上游原始碼。
- `core/`：REST API、系統匣、持久 Token 與關閉時等待下載狀態寫入；只有 Go 引擎。
- `src/Gopeed.Native/`：WinUI 頁面、ViewModel、資料模型與 API client。
- `scripts/`：建置、圖示生成與每使用者安裝包。
- `DESIGN.md`：原生介面與素材來源規範。

授權為 GPL-3.0，見 `LICENSE`。散布修改版應一併提供對應原始碼，包括固定版本的上游核心。

## 驗證

核心整合測試涵蓋 Token 驗證、建立下載、暫停、核心重新啟動、續傳、SHA-256 與移除任務。原生介面已實際操作 HTTP 下載、深淺主題、窄視窗、設定、對話框與依狀態啟用的命令。64 MiB 本機 HTTP 下載在關閉前端後持續增加進度並完成，檔案 SHA-256 與來源一致。

記憶體請以交付的 `verification.txt` 為準：私有工作集、含共用 DLL 的總工作集、私有提交量是不同指標。數值是這台電腦的實測快照，並非跨硬體保證或與官方版本相同工作量的對照測試。
