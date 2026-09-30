# Browser download window

Mode: Operate. Ordinary extension of the existing Windows Fluent design; code-led implementation with native WinUI controls.

THESIS: Each captured download waits for a user decision, then becomes a compact progress window.
FIRST VIEWPORT: The confirmation exposes the source URL, destination, editable file name, resolved size, optional request settings, and Start/Cancel. After Start, the same window shows progress, speed, remaining time, source and saved path; Pause, Continue, Open File and Folder stay reachable.
INTERACTION: Authenticated browser POSTs are handed to the frontend without creating a task. Cancel closes the window. Start commits the request once. Closing the progress window leaves the transfer running. Each progress window owns its API client and polling lifetime, so the main queue may close independently.
FORM: Windows Fluent Page with a shared DownloadForm UserControl, TextBox, Expander, ProgressBar, TextBlock and Button; system typography, theme and spacing. The confirmation fills the window with one themed background; no nested dialog frame or white surround. The main queue's AddDownloadDialog hosts the same form. No web frontend or imitation IDM widgets.
FINISH: unreviewed and undocumented is unfinished; this build ends with the finish review, the verdict, DESIGN.md, and every shipping raster carrying its provenance.
