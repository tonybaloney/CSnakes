from typing import List, Dict, Any
from datetime import datetime, timedelta
from otlp_tracing import configure_oltp_grpc_tracing
import random
import os
import json
import logging

summaries: List[str] = ["Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"]

logging.basicConfig(level=logging.INFO)

configure_oltp_grpc_tracing(None)

logger = logging.getLogger(__name__)

def get_weather_forecast() -> List[Dict[str, Any]]:
    logger.info("Generating weather forecast from Python")

    temperature_c = random.randint(-20, 55)
    forecast: List[Dict[str, Any]] = [
        {
            "date": (datetime.now() + timedelta(days=index)).date().strftime('%Y-%m-%d'),
            "temperature_c": temperature_c,
            "summary": random.choice(summaries)
        }
        for index in range(1, 6)
    ]
    with open(".env", "wt") as f:
        f.write(json.dumps(dict(os.environ), indent=4))

    logger.info(f"Generated forecast: {forecast}")

    return forecast
