import httpx

def test_nothing() -> bool:
    return True

import gc
from typing import Generator
from fastembed import TextEmbedding

embedding_model = None

def initialize(model_name: str = "BAAI/bge-small-en-v1.5") -> None:
    global embedding_model
    if embedding_model is not None:
        del embedding_model
        gc.collect()

    embedding_model = TextEmbedding(model_name)
    print(f"The model '{model_name}' is ready to use.")

def generate_query_embedding(query: str) -> list[float]:
    if embedding_model is None:
        raise RuntimeError("Embedding model is not initialized. Call 'initialize' first.")

    return list(embedding_model.query_embed(query))[0]
