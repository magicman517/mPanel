package docker

import (
	"context"
	"fmt"
	"log/slog"
	"strings"

	"github.com/docker/docker/client"
)

type Client struct {
	api *client.Client
}

func NewClient() (*Client, error) {
	cli, err := client.NewClientWithOpts(client.FromEnv, client.WithAPIVersionNegotiation())
	if err != nil {
		return nil, err
	}

	info, err := cli.Info(context.Background())
	if err != nil {
		return nil, err
	}

	if strings.Contains(info.DockerRootDir, "/var/snap/docker") {
		return nil, fmt.Errorf("Docker is running in a snap environment, which is not supported")
	}

	slog.Info("Connected to Docker daemon", "ServerVersion", info.ServerVersion, "Architecture", info.Architecture)

	return &Client{api: cli}, nil
}

func (m *Client) Ping(ctx context.Context) error {
	_, err := m.api.Ping(ctx)
	return err
}

func (m *Client) Close() error {
	return m.api.Close()
}
