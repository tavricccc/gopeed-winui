package main

import "testing"

func TestCoreInstanceOwnership(t *testing.T) {
	root := t.TempDir()
	release, acquired := acquireCore(root)
	if !acquired {
		t.Fatal("initial ownership")
	}
	if _, acquired = acquireCore(root); acquired {
		t.Fatal("second core accepted")
	}
	release()
	release, acquired = acquireCore(root)
	if !acquired {
		t.Fatal("ownership not released")
	}
	release()
}
func TestGitHubMirror(t *testing.T) {
	config := map[string]any{"githubMirror": map[string]any{"enabled": true, "mirrors": []any{map[string]any{"type": "jsdelivr", "url": "https://cdn.jsdelivr.net"}}}}
	source := "https://raw.githubusercontent.com/owner/repo/main/list.txt"
	if got := mirroredURL(source, config); got != "https://cdn.jsdelivr.net/gh/owner/repo@main/list.txt" {
		t.Fatal(got)
	}
	if got := mirroredURL("https://example.com/file.zip", config); got != "https://example.com/file.zip" {
		t.Fatal(got)
	}
}
