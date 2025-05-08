from typing import List, Dict, Any, Optional
from datetime import datetime, timedelta, date
from otlp_tracing import configure_oltp_grpc_tracing, get_span_context

import psycopg2
import psycopg2.extras
import os
import logging
from opentelemetry.instrumentation.psycopg2 import Psycopg2Instrumentor
import csv
import pathlib
import pandas as pd
import numpy as np

logging.basicConfig(level=logging.INFO)
tracer = configure_oltp_grpc_tracing()
logger = logging.getLogger(__name__)

Psycopg2Instrumentor().instrument(skip_dep_check=True, enable_commenter=True, commenter_options={})

ado_conn_str = os.getenv("ConnectionStrings__weather")
# Convert the connection string to a dictionary
ado_conn_dict = dict(item.split("=") for item in ado_conn_str.split(";"))
cnx = psycopg2.connect(dbname=ado_conn_dict["Database"], user=ado_conn_dict["Username"], password=ado_conn_dict["Password"], host=ado_conn_dict["Host"], port=ado_conn_dict["Port"])


def get_weather_forecast(days: int = 7, trace_id: Optional[str] = None, span_id: Optional[str] = None) -> List[Dict[str, Any]]:
    with tracer.start_as_current_span("generate-forecast", get_span_context(trace_id, span_id)):
        logger.info("Generating weather forecast")

        # Get the date for today and the next (7) days
        start = date.today()
        end = start + timedelta(days=days)

        # read 6 random records from the pg database
        cursor = cnx.cursor(cursor_factory=psycopg2.extras.DictCursor)
        cursor.execute(f"""SELECT "City", "Date", "Precipitation", "TemperatureMinC", "TemperatureMaxC", "Wind", "Summary", EXTRACT(DOY FROM "Date") as "DOY" FROM public."WeatherRecords" WHERE EXTRACT(DOY FROM "Date") >= %s AND EXTRACT(DOY FROM "Date") <= %s;""", (start.strftime('%j'), end.strftime('%j')))

        # Convert the records to a list of dictionaries
        df = pd.DataFrame(cursor.fetchall(), columns=["City", "Date", "Precipitation", "TemperatureMinC", "TemperatureMaxC", "Wind", "Summary", "DOY"])
        cursor.close()
        cnx.commit()
        logger.info("%s", list(df.columns))
        # for day in range, get the averages based on previous years
        estimates = []
        for i in range(days):
            doy = int((start + timedelta(days=i)).strftime("%j"))
            previous = df[df['DOY'] == doy]
            estimate = {
                'City': 'Seattle',
                'Date': start + timedelta(days=i),
                'Precipitation': previous.Precipitation.mean(),
                'TemperatureMinC':previous.TemperatureMinC.mean(),
                'TemperatureMaxC': previous.TemperatureMaxC.mean(),
                'Wind': previous.Wind.mean(),
                'Summary': previous.Summary.mode()[0]
                }
            estimates.append(estimate)

        logger.info("Generated forecast: %s", estimates[0:1])

    return estimates

def seed_database() -> None:
    with tracer.start_as_current_span("seed-database"):
        logger.info("Seeding database from Python")
        data_dir = pathlib.Path(os.getcwd())
        data_file = data_dir / "Data" / "seattle-weather.csv"
        with open(data_file) as f:
            weather_history = csv.DictReader(f)

            # write to pg database
            cursor = cnx.cursor()
            counter = 0
            for row in weather_history:
                cursor.execute("""INSERT INTO public."WeatherRecords"(
                            "City", "Date", "Precipitation", "TemperatureMinC", "TemperatureMaxC", "Wind", "Summary")
                            VALUES (%s, %s, %s, %s, %s, %s, %s);""", ("Seattle", row["date"], row["precipitation"], row["temp_min"], row["temp_max"], row["wind"], row["weather"]))
                counter += 1

            logger.info(f"Database seeded with {counter} records")
            cursor.close()
            cnx.commit()

