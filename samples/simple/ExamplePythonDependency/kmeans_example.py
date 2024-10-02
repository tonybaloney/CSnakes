from sklearn.cluster import k_means
import numpy as np
from typing_extensions import Buffer

def calculate_kmeans_inertia(data: list[tuple[int, int]], n_clusters: int) -> tuple[Buffer, float]:
    # Demo data
    X = np.array(data)
    centroid, _, inertia = k_means(
        X, n_clusters=n_clusters, n_init="auto", random_state=0
    )
    return centroid, inertia
