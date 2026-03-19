import logging
import shutil

import aiodocker
from aiodocker import DockerError
from pydantic import BaseModel

logger = logging.getLogger(__name__)


class DockerInfo(BaseModel):
    engine_version: str
    api_version: str


async def get_docker_info() -> DockerInfo:
    docker_path = shutil.which("docker")
    if docker_path and "/snap/" in docker_path:
        raise Exception(
            f"Docker is installed via snap, which is not supported: {docker_path}"
        )

    try:
        async with aiodocker.Docker() as docker:
            version_info = await docker.version()

            engine_version = version_info.get("Version", "Unknown")
            api_version = version_info.get("ApiVersion", "Unknown")

            return DockerInfo(
                engine_version=engine_version,
                api_version=api_version,
            )
    except DockerError as e:
        raise RuntimeError(f"Docker API rejected connection: {e.message}")
    except FileNotFoundError:
        raise RuntimeError("Could not find /var/run/docker.sock. Is Docker running?")
    except Exception as e:
        raise RuntimeError(f"Failed to connect to Docker: {str(e)}")
