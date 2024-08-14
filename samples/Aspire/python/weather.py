from typing import List, Dict, Any
from datetime import datetime, timedelta
from otlp_tracing import configure_oltp_grpc_tracing
import random
import os
import logging

summaries: List[str] = ["Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"]

logging.basicConfig(level=logging.INFO)

configure_oltp_grpc_tracing(
    service_name=os.getenv("OTEL_SERVICE_NAME", "apiservice"),
    endpoint=os.getenv("OTEL_EXPORTER_OTLP_ENDPOINT"),
    insecure=os.getenv("OTEL_EXPORTER_OTLP_TRACES_INSECURE", "true").lower() == "true",
    api_key=os.getenv("OTEL_EXPORTER_OTLP_TRACES_API_KEY"),
)

def get_weather_forecast() -> List[Dict[str, Any]]:
    logging.info("Generating weather forecast")

    temperature_c = random.randint(-20, 55)
    forecast: List[Dict[str, Any]] = [
        {
            "date": (datetime.now() + timedelta(days=index)).date().strftime('%Y-%m-%d'),
            "temperature_c": temperature_c,
            "summary": random.choice(summaries)
        }
        for index in range(1, 6)
    ]

    logging.info(f"Generated forecast: {forecast}")

    return forecast
