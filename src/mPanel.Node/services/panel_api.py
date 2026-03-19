from typing import Literal

import httpx

from core import config
from core.system import HeartbeatStats, SystemSpecs


class PanelClient:
    def __init__(self, http_client: httpx.AsyncClient) -> None:
        self.http_client = http_client
        self.base_url = config.panel_url.rstrip("/")
        self.token = config.token

    async def _request(
        self,
        method: Literal["GET", "POST", "PATCH", "PUT", "DELETE"],
        endpoint: str,
        json_data: dict | None = None,
    ) -> dict:
        url = f"{self.base_url}{endpoint}"
        headers = {
            "Accept": "application/json",
            "X-Node-Token": self.token,
        }
        response = await self.http_client.request(
            method=method,
            url=url,
            headers=headers,
            json=json_data,
            timeout=10.0,
        )
        response.raise_for_status()
        return response.json() if response.content else {}

    async def send_handshake(
        self, saved_node_id: str | None, specs: SystemSpecs
    ) -> dict:
        return await self._request(
            "POST",
            "/api/nodes/handshake",
            json_data={
                "nodeId": saved_node_id,
                "osName": specs.os_name,
                "architecture": specs.architecture,
                "cpuCores": specs.cpu_cores,
                "totalMemoryMb": specs.total_memory_mb,
                "totalDiskMb": specs.total_disk_mb,
            },
        )

    async def send_heartbeat(self, stats: HeartbeatStats) -> None:
        await self._request(
            "POST",
            "/api/nodes/heartbeat",
            json_data={
                "cpuUsage": stats.cpu_usage,
                "memoryUsageMb": stats.memory_usage_mb,
                "diskUsageMb": stats.disk_usage_mb,
            },
        )
