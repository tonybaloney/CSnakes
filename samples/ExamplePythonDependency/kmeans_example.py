from sklearn.cluster import k_means
import numpy as np

def calculate_kmeans_inertia(data: list[tuple[int, int]], n_clusters: int) -> tuple[list[list[float]], float]:
    # Demo data
    X = np.array(data)
    centroid, label, inertia = k_means(
        X, n_clusters=n_clusters, n_init="auto", random_state=0
    )
    return centroid.tolist(), inertia
