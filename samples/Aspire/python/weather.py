from typing import List, Dict, Any
from collections.abc import Buffer
from datetime import datetime, timedelta, date

import psycopg2
import psycopg2.extras

import os
import logging
from io import BytesIO

from otlp_tracing import configure_oltp_grpc_tracing, get_span_context
from opentelemetry.instrumentation.psycopg2 import Psycopg2Instrumentor

import pandas as pd
from meteostat import Point, Daily
from geopy.geocoders import Nominatim
from matplotlib import pyplot as plt


logging.basicConfig(level=logging.INFO)
tracer = configure_oltp_grpc_tracing()
logger = logging.getLogger(__name__)

Psycopg2Instrumentor().instrument(skip_dep_check=True, enable_commenter=True, commenter_options={})

ado_conn_str = os.getenv("ConnectionStrings__weather")
# Convert the connection string to a dictionary
ado_conn_dict = dict(item.split("=") for item in ado_conn_str.split(";"))
cnx = psycopg2.connect(dbname=ado_conn_dict["Database"], user=ado_conn_dict["Username"], password=ado_conn_dict["Password"], host=ado_conn_dict["Host"], port=ado_conn_dict["Port"])


def get_weather_forecast(days: int = 7, trace_id: str = None, span_id: str = None) -> List[Dict[str, Any]]:
    with tracer.start_as_current_span("generate-forecast", get_span_context(trace_id, span_id)):
        logger.info("Generating weather forecast")

        # Get the date for today and the next (7) days
        start = date.today()
        end = start + timedelta(days=days)

        # read 6 random records from the pg database
        cursor = cnx.cursor(cursor_factory=psycopg2.extras.DictCursor)
        cursor.execute(f"""SELECT "City", "Date", "Precipitation", "TemperatureMinC", "TemperatureMaxC", "Wind", "Summary", EXTRACT(DOY FROM "Date") as "DOY"
                           FROM public."WeatherRecords" WHERE EXTRACT(DOY FROM "Date") >= %s AND EXTRACT(DOY FROM "Date") <= %s;""", (start.strftime('%j'), end.strftime('%j')))

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


def get_weather_chart(days: int = 7, trace_id: str = None, span_id: str = None) -> Buffer:
    with tracer.start_as_current_span("generate-chart", get_span_context(trace_id, span_id)):
        logger.info("Generating weather chart")

        # Get last 7 days
        start = date.today() - timedelta(days=days)
        end = date.today()

        # read 6 random records from the pg database
        cursor = cnx.cursor(cursor_factory=psycopg2.extras.DictCursor)
        cursor.execute(f"""SELECT "City", "Date", "Precipitation", "TemperatureMinC", "TemperatureMaxC"
                           FROM public."WeatherRecords" WHERE "Date" >= %s AND "Date" <= %s;""", (start, end))

        # Convert the records to a list of dictionaries
        df = pd.DataFrame(cursor.fetchall(), columns=["City", "Date", "Precipitation", "TemperatureMinC", "TemperatureMaxC"])
        cursor.close()
        cnx.commit()

        df.plot(x='Date', y=['Precipitation', 'TemperatureMinC', 'TemperatureMaxC'], title=f'Weather for the past {days} days')

        outstream = BytesIO()
        plt.savefig(outstream, format='png', dpi=300)
        return outstream.getbuffer()


def seed_database(location: str = "Seattle, USA", from_year: int = 2016) -> None:
    geolocator = Nominatim(user_agent="weatherBot", timeout=10)

    geolocation = geolocator.geocode(location)
    point = Point(geolocation.latitude, geolocation.longitude)
    data = Daily(point, start=datetime(from_year, 1, 1), end=datetime.today())
    data = data.fetch()

    data = data.rename(columns={'prcp': 'precipitation',
                                'tmin': 'temp_min',
                                'tmax': 'temp_max',
                                'wspd': 'wind'})
    
    # add "weather" column to summarise weather conditions. Start as blank
    data['weather'] = ''

    # if snow, set weather to snow
    data.loc[data['snow'] > 0, 'weather'] = 'snow'
    # if lots of rain, set weather to rain
    data.loc[data['precipitation'] > 10, 'weather'] = 'rain'
    # if some precipitation between 0 and 10, set weather to drizzle
    data.loc[(data['precipitation'] > 0) & (data['precipitation'] <=10), 'weather'] = 'drizzle'
    # if no precipitation, set weather to clear
    data.loc[data['precipitation'] == 0, 'weather'] = 'sun'
    # drop columns we don't need
    data = data.drop(columns=['tavg', 'wdir', 'snow', 'wpgt', 'pres', 'tsun'])
    
    # reorder columns to date, precipitation, temp_min, temp_max, wind
    data = data[['precipitation', 'temp_max', 'temp_min', 'wind', 'weather']]

    data.index.name = 'date'

    with tracer.start_as_current_span("seed-database"):
        logger.info("Seeding database from Python")

        # write to pg database
        cursor = cnx.cursor()
        counter = 0
        for index, row in data.iterrows():
            index: datetime
            cursor.execute("""INSERT INTO public."WeatherRecords"(
                        "City", "Date", "Precipitation", "TemperatureMinC", "TemperatureMaxC", "Wind", "Summary")
                        VALUES (%s, %s, %s, %s, %s, %s, %s);""", (location, index.strftime("%Y-%m-%d"), row["precipitation"], row["temp_min"], row["temp_max"], row["wind"], row["weather"]))
            counter += 1

        logger.info(f"Database seeded with {counter} records")
        cursor.close()
        cnx.commit()

