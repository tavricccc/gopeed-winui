package main

import (
	"bytes"
	"encoding/json"
	"io"
	"net/http"
	"path/filepath"

	"github.com/GopeedLab/gopeed/pkg/rest"
	"github.com/GopeedLab/gopeed/pkg/rest/model"
)

// Upstream's local-folder endpoint only offers dev mode. Installed local copies
// use the same extension installer as repository extensions, without live editing.
func localExtensions(next http.Handler, token string) http.Handler {
	return http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {
		if r.Method != http.MethodPost || r.URL.Path != "/api/v1/extensions" {
			next.ServeHTTP(w, r)
			return
		}
		if r.Header.Get("X-Api-Token") != token {
			http.Error(w, "unauthorized", http.StatusUnauthorized)
			return
		}
		body, err := io.ReadAll(http.MaxBytesReader(w, r.Body, 1024*1024))
		if err != nil {
			http.Error(w, "invalid request", http.StatusBadRequest)
			return
		}
		var request model.InstallExtension
		if json.Unmarshal(body, &request) == nil && filepath.IsAbs(request.URL) && !request.DevMode {
			extension, err := rest.Downloader.InstallExtensionByFolder(request.URL, false)
			if err != nil {
				rest.WriteJson(w, model.NewErrorResult(err.Error()))
				return
			}
			rest.WriteJson(w, model.NewOkResult(extension.Identity))
			return
		}
		r.Body = io.NopCloser(bytes.NewReader(body))
		next.ServeHTTP(w, r)
	})
}
