import numpy as np
import seaborn as sns
import matplotlib.pyplot as plt
from mpl_toolkits.mplot3d.art3d import Poly3DCollection
from sklearn.mixture import BayesianGaussianMixture

# Generate random data points
np.random.seed(42)  # Set random seed for reproducibility

# Number of data points
n_points = 100

# Generate 3D coordinates
x = np.random.uniform(0, 10, n_points)
y = np.random.uniform(0, 10, n_points)
z = np.random.uniform(0, 10, n_points)
print("x:", x)

# Generate random importance values between 0 and 1
importance = np.random.rand(n_points)
print("Importance:", importance)

# Create feature matrix
X = np.column_stack((x, y, z))

# Define the Bayesian GMM with cuboid prior
n_clusters = 3  # Number of clusters
bounds = [(0, 10), (0, 10), (0, 10)]  # Bounds for each dimension

gmm = BayesianGaussianMixture(n_components=n_clusters, covariance_type='full',
                              weight_concentration_prior_type='dirichlet_process')
gmm.fit(X)

# Get cluster assignments
cluster_labels = gmm.predict(X)

# Assign the center point with highest importance in each cluster
cluster_centers = []
for k in range(n_clusters):
    cluster_points = X[cluster_labels == k]
    center_index = np.argmax(importance[cluster_labels == k])
    cluster_centers.append(cluster_points[center_index])

# Generate cuboid boundaries for each cluster
cuboid_bounds = []
for k in range(n_clusters):
    cuboid_bound = []
    for dim in range(3):
        cuboid_bound.append(np.min(X[cluster_labels == k, dim]))
        cuboid_bound.append(np.max(X[cluster_labels == k, dim]))
    cuboid_bounds.append(cuboid_bound)

# Visualize the clusters, centers, and cuboid boundaries in a 3D plot
fig = plt.figure(figsize=(8, 6))
ax = fig.add_subplot(111, projection='3d')

# Scatter plot for each cluster
for k in range(n_clusters):
    cluster_points = X[cluster_labels == k]
    ax.scatter(cluster_points[:, 0], cluster_points[:, 1], cluster_points[:, 2], label=f"Cluster {k + 1}")

# Scatter plot for cluster centers
centers = np.array(cluster_centers)
ax.scatter(centers[:, 0], centers[:, 1], centers[:, 2], color='red', marker='X', s=100, label='Cluster Centers')

# Plot cuboid boundaries
for k in range(n_clusters):
    cuboid_bound = cuboid_bounds[k]
    cuboid_x = [cuboid_bound[0], cuboid_bound[0], cuboid_bound[1], cuboid_bound[1], cuboid_bound[0], cuboid_bound[0],
                cuboid_bound[1], cuboid_bound[1]]
    cuboid_y = [cuboid_bound[2], cuboid_bound[2], cuboid_bound[2], cuboid_bound[2], cuboid_bound[3], cuboid_bound[3],
                cuboid_bound[3], cuboid_bound[3]]
    cuboid_z = [cuboid_bound[4], cuboid_bound[5], cuboid_bound[5], cuboid_bound[4], cuboid_bound[4], cuboid_bound[5],
                cuboid_bound[5], cuboid_bound[4]]

    vertices = [
        [cuboid_x[0], cuboid_y[0], cuboid_z[0]],
        [cuboid_x[1], cuboid_y[1], cuboid_z[1]],
        [cuboid_x[2], cuboid_y[2], cuboid_z[2]],
        [cuboid_x[3], cuboid_y[3], cuboid_z[3]],
        [cuboid_x[4], cuboid_y[4], cuboid_z[4]],
        [cuboid_x[5], cuboid_y[5], cuboid_z[5]],
        [cuboid_x[6], cuboid_y[6], cuboid_z[6]],
        [cuboid_x[7], cuboid_y[7], cuboid_z[7]]
    ]

    cuboid_faces = [
        [vertices[0], vertices[1], vertices[2], vertices[3]],
        # [vertices[0], vertices[1], vertices[4], vertices[5]],
        [vertices[1], vertices[2], vertices[6], vertices[5]],
        [vertices[2], vertices[3], vertices[7], vertices[6]],
        [vertices[0], vertices[3], vertices[7], vertices[4]],
        [vertices[4], vertices[5], vertices[6], vertices[7]]
    ]

    ax.add_collection3d(
        Poly3DCollection(cuboid_faces, linewidths=0.5, edgecolors='black', facecolors=sns.color_palette()[k], alpha=0.5))

ax.set_xlabel('X')
ax.set_ylabel('Y')
ax.set_zlabel('Z')
ax.set_title('Clustering Results')
ax.legend()

plt.show()
