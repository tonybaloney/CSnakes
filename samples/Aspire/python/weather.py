from typing import List, Dict, Any
from datetime import datetime, timedelta
from otlp_tracing import configure_oltp_grpc_tracing, get_span_context
import random
import logging

summaries: List[str] = ["Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"]

logging.basicConfig(level=logging.INFO)
tracer = configure_oltp_grpc_tracing()
logger = logging.getLogger(__name__)


def get_weather_forecast(trace_id: str = None, span_id: str = None) -> List[Dict[str, Any]]:
    with tracer.start_as_current_span("generate-forecast", get_span_context(trace_id, span_id)):
        logger.info("Generating weather forecast from Python")
        
        # read 6 random records from the pg database

        logger.info("Generated forecast: %s", forecast)

    return forecast

def seed_database(trace_id: str = None, span_id: str = None) -> None:
    with tracer.start_as_current_span("seed-database", get_span_context(trace_id, span_id)):
        logger.info("Seeding database from Python")
        logger.info("Database seeded")

        temperature_c = random.randint(-20, 55)
        forecast: List[Dict[str, Any]] = [
            {
                "date": (datetime.now() + timedelta(days=index)).date().strftime('%Y-%m-%d'),
                "temperature_c": temperature_c,
                "summary": random.choice(summaries)
            }
            for index in range(1, 100)
        ]

        # write to pg database