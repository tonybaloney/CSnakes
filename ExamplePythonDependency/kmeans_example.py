from sklearn.cluster import k_means
import numpy as np

def calculate_kmeans_interia(n_clusters: int) -> float:
    # Demo data
    X = np.array([[1, 2], [1, 4], [1, 0],
              [10, 2], [10, 4], [10, 0]])
    centroid, label, inertia = k_means(
        X, n_clusters=n_clusters, n_init="auto", random_state=0
    )
    return inertia
