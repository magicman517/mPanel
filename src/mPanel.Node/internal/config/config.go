package config

import (
	"fmt"
	"os"
)

type Config struct {
	PanelURL  string
	NodeToken string
}

func Load() (*Config, error) {
	url := os.Getenv("PANEL_URL")
	token := os.Getenv("NODE_TOKEN")

	if url == "" {
		return nil, fmt.Errorf("PANEL_URL is required")
	}
	if token == "" {
		return nil, fmt.Errorf("NODE_TOKEN is required")
	}

	return &Config{
		PanelURL:  url,
		NodeToken: token,
	}, nil
}
