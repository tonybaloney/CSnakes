from fastembed import TextEmbedding

def generate_embeddings(documents: list[str]) -> list:
    embedding_model = TextEmbedding() #1
    embeddings_list = list(embedding_model.embed(documents)) #2
    
    return [
        [1.23, 4.56, 7.89],
        [9.87, 6.54, 3.21],
    ]
