package api

import (
	"bytes"
	"context"
	"encoding/json"
	"fmt"
	"io"
	"net/http"
	"time"
)

type Client struct {
	baseURL    string
	token      string
	httpClient *http.Client
}

func NewClient(baseURL, token string) *Client {
	if baseURL[len(baseURL)-1] != '/' {
		baseURL += "/"
	}
	if baseURL[:7] != "http://" && baseURL[:8] != "https://" {
		baseURL = "https://" + baseURL
	}
	return &Client{
		baseURL: baseURL,
		token:   token,
		httpClient: &http.Client{
			Timeout: 10 * time.Second,
		},
	}
}

func (c *Client) Handshake() error {
	// TODO
	return c.doRequest(context.Background(), http.MethodGet, "handshake", nil, nil)
}

func (c *Client) doRequest(ctx context.Context, method, path string, reqBody any, resBody *any) error {
	endpoint := fmt.Sprintf("%s%s", c.baseURL, path)

	var bodyReader io.Reader

	if reqBody != nil {
		jsonData, err := json.Marshal(reqBody)
		if err != nil {
			return fmt.Errorf("failed to marshal request body: %w", err)
		}
		bodyReader = bytes.NewReader(jsonData)
	}

	req, err := http.NewRequestWithContext(ctx, method, endpoint, bodyReader)
	if err != nil {
		return fmt.Errorf("failed to create request: %w", err)
	}

	req.Header.Set("X-Node-Token", c.token)
	req.Header.Set("Accept", "application/json")
	if reqBody != nil {
		req.Header.Set("Content-Type", "application/json")
	}

	res, err := c.httpClient.Do(req)
	if err != nil {
		return fmt.Errorf("http request failed: %w", err)
	}
	defer res.Body.Close()

	if res.StatusCode < 200 || res.StatusCode >= 300 {
		return fmt.Errorf("panel returned error status: %d", res.StatusCode)
	}

	if reqBody != nil {
		if err := json.NewDecoder(res.Body).Decode(resBody); err != nil {
			return fmt.Errorf("failed to decode response body: %w", err)
		}
	}

	return nil
}
