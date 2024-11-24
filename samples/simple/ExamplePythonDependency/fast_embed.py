from fastembed import TextEmbedding

def generate_embeddings(documents: list[str]) -> list:
    embedding_model = TextEmbedding() #1
    embeddings_list = list(embedding_model.embed(documents)) #2
    return embeddings_list

