import asyncio
import logging
from contextlib import asynccontextmanager

import httpx
from fastapi import FastAPI

from core import get_saved_node_id, get_system_specs, save_node_id, setup_logging
from core.system import SystemSpecs
from services import PanelClient, get_docker_info
from workers import start_heartbeat_worker

logger = logging.getLogger(__name__)


async def perform_handshake(panel_api: PanelClient, specs: SystemSpecs) -> None:
    local_id = get_saved_node_id()
    try:
        response_data = await panel_api.send_handshake(local_id, specs)

        if not local_id:
            response_node_id = response_data["nodeId"]
            save_node_id(response_node_id)
            logger.info(f"Node identity locked to: {response_node_id}")
        else:
            logger.info(f"Successful handshake. Logged in as {local_id}")
    except httpx.HTTPStatusError as e:
        if e.response.status_code == 409:
            logger.critical(
                "Did you copy a token from another node? Check /etc/mpanel/.node_id"
            )
        else:
            logger.critical(
                f"Panel rejected handshake with status: {e.response.status_code}"
            )
        raise RuntimeError("Handshake failed")
    except Exception as e:
        logger.critical(f"Unexpected error during handshake: {e}")
        raise RuntimeError("Handshake failed")


@asynccontextmanager
async def lifespan(app: FastAPI):
    specs = get_system_specs()
    logger.info(f"System specs: {specs}")
    docker_info = await get_docker_info()
    logger.info(f"Docker: {docker_info}")

    async with httpx.AsyncClient() as http_client:
        panel_api = PanelClient(http_client)
        await perform_handshake(panel_api, specs)

        app.state.panel_api = panel_api
        app.state.heartbeat_task = asyncio.create_task(
            start_heartbeat_worker(panel_api, interval_seconds=10)
        )

        yield

    if hasattr(app.state, "heartbeat_task"):
        app.state.heartbeat_task.cancel()


app = FastAPI(lifespan=lifespan)


@app.get("/health")
async def health():
    return {"status": "ok"}


if __name__ == "__main__":
    setup_logging(level=logging.INFO)

    import uvicorn

    uvicorn.run(app, host="0.0.0.0", port=10001, log_config=None)
