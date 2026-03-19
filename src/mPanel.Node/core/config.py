import os
import platform
from pathlib import Path

from pydantic_settings import BaseSettings, SettingsConfigDict


def get_default_config_dir() -> str:
    if platform.system() == "Windows":
        base = os.environ.get("PROGRAMDATA", "C:\\ProgramData")
        return os.path.join(base, "mpanel")
    return "/etc/mpanel"


class Config(BaseSettings):
    token: str
    panel_url: str

    config_dir: Path = Path(get_default_config_dir())

    model_config = SettingsConfigDict(env_file=".env", env_file_encoding="utf-8")


config = Config()  # type: ignore


def _get_identity_file() -> Path:
    return config.config_dir / ".node_id"


def get_saved_node_id() -> str | None:
    identity_file = _get_identity_file()
    if identity_file.exists():
        return identity_file.read_text(encoding="utf-8").strip()
    return None


def save_node_id(node_id: str) -> None:
    identity_file = _get_identity_file()

    try:
        config.config_dir.mkdir(parents=True, exist_ok=True)
        identity_file.write_text(node_id, encoding="utf-8")

    except PermissionError:
        raise RuntimeError(
            f"\nPERMISSION DENIED: Cannot write to '{config.config_dir}'."
            f"\nFIX: If you are testing locally, add 'CONFIG_DIR=./tmp' to your .env file!"
        )
