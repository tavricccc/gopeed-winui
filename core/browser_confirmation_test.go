package main

import (
	"net/http"
	"net/http/httptest"
	"strings"
	"testing"
)

func TestBrowserConfirmation(t *testing.T) {
	created, prompted := 0, 0
	handler := browserConfirmation(http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) { created++; w.WriteHeader(204) }), "token", func(body []byte) (string, error) {
		prompted++
		if !strings.Contains(string(body), "Cookie") {
			t.Fatal("request headers lost")
		}
		return "pending-id", nil
	})
	request := func(token, confirmed, body string) int {
		r := httptest.NewRequest("POST", "/api/v1/tasks", strings.NewReader(body))
		r.Header.Set("X-Api-Token", token)
		r.Header.Set("X-Gopeed-Native-Confirmed", confirmed)
		w := httptest.NewRecorder()
		handler.ServeHTTP(w, r)
		return w.Code
	}
	body := `{"req":{"url":"https://example.com/file","extra":{"header":{"Cookie":"test=value"}}}}`
	if request("bad", "", body) != 401 || request("token", "", `{}`) != 400 {
		t.Fatal("invalid request accepted")
	}
	if request("token", "", body) != 200 || created != 0 || prompted != 1 {
		t.Fatal("browser download started before confirmation")
	}
	if request("token", "1", body) != 204 || created != 1 || prompted != 1 {
		t.Fatal("confirmed download did not reach engine")
	}
}
