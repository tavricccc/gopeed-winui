package main

import (
 "crypto/rand"
 "encoding/hex"
 "encoding/json"
 "flag"
 "fmt"
 "net"
 "net/http"
 "os"
 "os/signal"
 "path/filepath"
 "time"

 "github.com/GopeedLab/gopeed/pkg/rest"
 "github.com/GopeedLab/gopeed/pkg/rest/model"
)

type session struct {
 Port int `json:"port"`
 Token string `json:"token"`
 PID int `json:"pid"`
 ControlPort int `json:"controlPort"`
}

func main() {
 root := flag.String("data", "", "Application data directory")
 flag.Parse()
 if *root == "" { panic("--data is required") }
 if err := os.MkdirAll(*root, 0700); err != nil { panic(err) }
 secret := make([]byte, 32)
 if _, err := rand.Read(secret); err != nil { panic(err) }
 token := hex.EncodeToString(secret)
 port, err := rest.Start(&model.StartConfig{
  Address: "127.0.0.1:0", Storage: model.StorageBolt,
  StorageDir: *root, ApiToken: token, ProductionMode: true,
  RefreshInterval: 1000,
 })
 if err != nil { panic(err) }
 defer rest.Stop()
 stop := make(chan os.Signal, 1)
 signal.Notify(stop, os.Interrupt)
 // A separate authenticated lifecycle endpoint avoids modifications to upstream.
 control := http.NewServeMux()
 control.HandleFunc("POST /shutdown", func(w http.ResponseWriter, r *http.Request) {
  if r.Header.Get("X-Api-Token") != token { http.Error(w,"unauthorized",401); return }
  w.WriteHeader(204)
  select { case stop <- os.Interrupt: default: }
 })
 listener, err := listenControl()
 if err != nil { panic(err) }
 server := &http.Server{Handler:control, ReadHeaderTimeout:5*time.Second}
 go server.Serve(listener)
 defer server.Close()
 state := session{port, token, os.Getpid(), listener.Addr().(*net.TCPAddr).Port}
 payload, _ := json.Marshal(state)
 sessionPath := filepath.Join(*root,"session.json")
 if err := os.WriteFile(sessionPath+".tmp",payload,0600); err != nil { panic(err) }
 if err := os.Rename(sessionPath+".tmp",sessionPath); err != nil { panic(err) }
 defer os.Remove(sessionPath)
 fmt.Println("Gopeed Native core ready")
 <-stop
}
