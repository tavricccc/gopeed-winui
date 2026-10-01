package main

import (
	"net/url"
	"strings"
)

func mirroredURL(source string, extra map[string]any) string {
	config, ok := extra["githubMirror"].(map[string]any)
	if !ok || config["enabled"] != true {
		return source
	}
	mirrors, _ := config["mirrors"].([]any)
	for _, raw := range mirrors {
		mirror, ok := raw.(map[string]any)
		if !ok || mirror["isDeleted"] == true {
			continue
		}
		uri, err := url.Parse(source)
		if err != nil {
			return source
		}
		address, _ := mirror["url"].(string)
		address = strings.TrimRight(address, "/")
		if mirror["type"] == "ghProxy" && (uri.Host == "github.com" || uri.Host == "raw.githubusercontent.com") {
			return address + "/" + source
		}
		if mirror["type"] == "jsdelivr" && uri.Host == "raw.githubusercontent.com" {
			parts := strings.SplitN(strings.TrimPrefix(uri.Path, "/"), "/", 4)
			if len(parts) == 4 {
				return address + "/gh/" + parts[0] + "/" + parts[1] + "@" + parts[2] + "/" + parts[3]
			}
		}
		return source
	}
	return source
}
