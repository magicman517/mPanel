package main

import (
	"context"
	"log/slog"
	"os"

	"monkeygames.dev/mPanel/internal/api"
	"monkeygames.dev/mPanel/internal/config"
	"monkeygames.dev/mPanel/internal/docker"
)

func main() {
	slog.Info("Starting node...")

	config, err := config.Load()
	if err != nil {
		slog.Error("Failed to load configuration", "Error", err)
		os.Exit(1)
	}

	api := api.NewClient(config.PanelURL, config.NodeToken)

	docker, err := docker.NewClient()
	if err != nil {
		slog.Error("Failed to create Docker manager", "Error", err)
		os.Exit(1)
	}
	defer docker.Close()

	if err := docker.Ping(context.Background()); err != nil {
		slog.Error("Failed to connect to Docker daemon", "Error", err)
		os.Exit(1)
	}

	api.Handshake()
}
