import asyncio
import logging

import httpx

from core import get_heartbeat_stats
from services import PanelClient

logger = logging.getLogger(__name__)


async def start_heartbeat_worker(api: PanelClient, interval_seconds: int = 10) -> None:
    logger.info(f"Heartbeat worker started with interval {interval_seconds} seconds")

    while True:
        try:
            stats = get_heartbeat_stats()
            await api.send_heartbeat(stats)
            logger.info(f"Heartbeat sent. Stats: {stats}")
        except httpx.HTTPError as e:
            logger.warning(f"Failed to send heartbeat: {e}")
        except Exception as e:
            logger.error(f"Unexpected error: {e}")

        try:
            await asyncio.sleep(interval_seconds)
        except asyncio.CancelledError:
            logger.info("Shutting down heartbeat worker")
            break
