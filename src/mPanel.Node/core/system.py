import platform

import psutil
from pydantic import BaseModel


class SystemSpecs(BaseModel):
    os_name: str
    architecture: str
    cpu_cores: int
    total_memory_mb: int
    total_disk_mb: int


class HeartbeatStats(BaseModel):
    cpu_usage: float
    memory_usage_mb: int
    disk_usage_mb: int


def get_system_specs() -> SystemSpecs:
    cpu_cores = psutil.cpu_count(logical=True)

    mem = psutil.virtual_memory()
    total_memory_mb = mem.total // (1024 * 1024)

    disk = psutil.disk_usage("/")
    total_disk_mb = disk.total // (1024 * 1024)

    return SystemSpecs(
        os_name=platform.system().lower(),
        architecture=platform.machine().lower(),
        cpu_cores=cpu_cores or 1,
        total_memory_mb=total_memory_mb,
        total_disk_mb=total_disk_mb,
    )


def get_heartbeat_stats() -> HeartbeatStats:
    cpu_percent = psutil.cpu_percent(interval=None)

    mem = psutil.virtual_memory()
    used_ram_mb = mem.used // (1024 * 1024)

    disk = psutil.disk_usage("/")
    used_disk_mb = disk.used // (1024 * 1024)

    return HeartbeatStats(
        cpu_usage=cpu_percent,
        memory_usage_mb=used_ram_mb,
        disk_usage_mb=used_disk_mb,
    )
