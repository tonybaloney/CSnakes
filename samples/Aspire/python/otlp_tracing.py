import logging
import os
from opentelemetry import metrics, trace

# Logging (Experimental)
from opentelemetry._logs import set_logger_provider
from opentelemetry.exporter.otlp.proto.grpc._log_exporter import (
    OTLPLogExporter,
)
from opentelemetry.exporter.otlp.proto.grpc.metric_exporter import OTLPMetricExporter
from opentelemetry.exporter.otlp.proto.grpc.trace_exporter import OTLPSpanExporter
from opentelemetry.sdk._logs import LoggerProvider, LoggingHandler
from opentelemetry.sdk._logs.export import BatchLogRecordProcessor
from opentelemetry.sdk.metrics import MeterProvider
from opentelemetry.sdk.metrics.export import PeriodicExportingMetricReader
from opentelemetry.sdk.resources import SERVICE_NAME, Resource
from opentelemetry.sdk.trace import TracerProvider
from opentelemetry.sdk.trace.export import BatchSpanProcessor


def configure_oltp_grpc_tracing(
    endpoint: str
):
    # Service name is required for most backends
    resource = Resource(attributes={SERVICE_NAME: os.getenv("OTEL_SERVICE_NAME", "unknown")})

    # Configure Tracing
    traceProvider = TracerProvider(resource=resource)
    processor = BatchSpanProcessor(OTLPSpanExporter(endpoint=endpoint))
    traceProvider.add_span_processor(processor)
    trace.set_tracer_provider(traceProvider)

    # Configure Metrics
    reader = PeriodicExportingMetricReader(OTLPMetricExporter(endpoint=endpoint))
    meterProvider = MeterProvider(resource=resource, metric_readers=[reader])
    metrics.set_meter_provider(meterProvider)

    # Configure Logging
    logger_provider = LoggerProvider(resource=resource)
    set_logger_provider(logger_provider)

    exporter = OTLPLogExporter(endpoint=endpoint)
    logger_provider.add_log_record_processor(BatchLogRecordProcessor(exporter))
    handler = LoggingHandler(level=logging.NOTSET, logger_provider=logger_provider)

    # Attach OTLP handler to root logger
    logging.getLogger().addHandler(handler)
