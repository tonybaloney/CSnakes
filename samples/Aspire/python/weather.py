from typing import List, Dict, Any
from datetime import datetime, timedelta
from otlp_tracing import configure_oltp_grpc_tracing, get_span_context

import psycopg2
import os
import random
import logging
from opentelemetry.instrumentation.psycopg2 import Psycopg2Instrumentor

summaries: List[str] = ["Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"]

logging.basicConfig(level=logging.INFO)
tracer = configure_oltp_grpc_tracing()
logger = logging.getLogger(__name__)

Psycopg2Instrumentor().instrument()

ado_conn_str = os.getenv("ConnectionStrings__weather")
# Convert the connection string to a dictionary
ado_conn_dict = dict(item.split("=") for item in ado_conn_str.split(";"))
cnx = psycopg2.connect(dbname=ado_conn_dict["Database"], user=ado_conn_dict["Username"], password=ado_conn_dict["Password"], host=ado_conn_dict["Host"], port=ado_conn_dict["Port"])


def get_weather_forecast(trace_id: str = None, span_id: str = None) -> List[Dict[str, Any]]:
    with tracer.start_as_current_span("generate-forecast", get_span_context(trace_id, span_id)):
        logger.info("Generating weather forecast from Python")
        
        # read 6 random records from the pg database
        cursor = cnx.cursor()
        cursor.execute("""SELECT * FROM public."WeatherForecasts" ORDER BY RANDOM() LIMIT 6;""")
        forecast = cursor.fetchall()
        cursor.close()
        cnx.commit()
        # Convert the records to a list of dictionaries
        forecast = [dict(zip([column[0] for column in cursor.description], row)) for row in forecast]
        
        logger.info("Generated forecast: %s", forecast)

    return forecast

def seed_database() -> None:
    with tracer.start_as_current_span("seed-database"):
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
        cursor = cnx.cursor()
        for record in forecast:
            cursor.execute("""INSERT INTO public."WeatherForecasts"(
                        "Date", "TemperatureC", "Summary")
                        VALUES (%s, %s, %s);""", (record["date"], record["temperature_c"], record["summary"]))
        cursor.close()
        cnx.commit()
