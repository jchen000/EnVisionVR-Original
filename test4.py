import json
import numpy as np
from mpl_toolkits.mplot3d import Axes3D
import matplotlib.pyplot as plt
from GPyOpt.methods import BayesianOptimization

def parse_json(json_data, camera_position):
    data = json.loads(json_data)
    captured_objects = set()

    for element in data:
        if "name" and "components" in element and element["importance"] > 0.2 and element["hierarchy_level"] < 3:
            x = element["components"][0]["position"]["x"]
            y = element["components"][0]["position"]["y"]
            z = element["components"][0]["position"]["z"]
            importance = element["importance"]

            distance = np.sqrt((x - camera_position[0]) ** 2 + (y - camera_position[1]) ** 2 + (z - camera_position[2]) ** 2)
            if distance <= importance:
                captured_objects.add(element["name"])

    return len(captured_objects)

# Define the objective function for Bayesian optimization
def objective_function(x):
    camera_position = x[0]
    score = parse_json(json_data, camera_position)
    return -score  # Minimize the negative score

def plot_element(ax, element):
    # if "name" and "components" in element and element["hierarchy_level"] < 2:
    if "name" and "components" in element and element["importance"] > 0.2 and element["hierarchy_level"] < 3:
        name = element["name"]
        x = element["components"][0]["position"]["x"]
        y = element["components"][0]["position"]["y"]
        z = element["components"][0]["position"]["z"]
        importance = element["importance"]
        ax.scatter(x, z, y)
        # ax.text(x, z, y, name)
        print(f"Name: {name}")
        print(f"Coordinates: x={x}, y={y}, z={z}")
        print(f"Importance: {importance}")

        # Plotting the orientation vector
        rot_x = element["components"][0]["rotation"]["x"]
        rot_y = element["components"][0]["rotation"]["y"]
        rot_z = element["components"][0]["rotation"]["z"]
        vector = np.array([0.5, 0, 0])  # Unit vector coordinates
        rotation_matrix = get_rotation_matrix(rot_x, rot_y, rot_z)
        rotated_vector = rotation_matrix.dot(vector)
        # ax.quiver(x, y, z, rotated_vector[0], rotated_vector[1], rotated_vector[2], color='blue', label='Vector')


        print(f"Rotation: x={rot_x}, y={rot_y}, z={rot_z}")
        print()

    if "children" in element:
        for child in element["children"]:
            plot_element(ax, child)

def get_rotation_matrix(rot_x, rot_y, rot_z):
    rotation_matrix_x = np.array([[1, 0, 0],
                                  [0, np.cos(rot_x), -np.sin(rot_x)],
                                  [0, np.sin(rot_x), np.cos(rot_x)]])

    rotation_matrix_y = np.array([[np.cos(rot_y), 0, np.sin(rot_y)],
                                  [0, 1, 0],
                                  [-np.sin(rot_y), 0, np.cos(rot_y)]])

    rotation_matrix_z = np.array([[np.cos(rot_z), -np.sin(rot_z), 0],
                                  [np.sin(rot_z), np.cos(rot_z), 0],
                                  [0, 0, 1]])

    rotation_matrix = rotation_matrix_z.dot(rotation_matrix_y).dot(rotation_matrix_x)
    return rotation_matrix

# Read the JSON file
with open("Assets/scene_graph_importance.json") as file:
    json_data = file.read()

# Define the bounds of the camera position
bounds = [{'name': 'x', 'type': 'continuous', 'domain': (-8, 8)},
          {'name': 'y', 'type': 'continuous', 'domain': (-4, 4)},
          {'name': 'z', 'type': 'continuous', 'domain': (0, 4)}]

# Perform Bayesian optimization
optimizer = BayesianOptimization(f=objective_function, domain=bounds)
optimizer.run_optimization(max_iter=50)

# Get the optimal camera position
optimal_position = optimizer.x_opt
optimal_score = -optimizer.fx_opt

print("Optimal Camera Position:")
print("x = {:.2f}, y = {:.2f}, z = {:.2f}".format(*optimal_position))
print("Optimal Score: {:.2f}".format(optimal_score))

# Visualize the scene with the optimal camera position
fig = plt.figure()
ax = fig.add_subplot(111, projection='3d')
plot_element(ax, json.loads(json_data))
ax.set_xlabel('X')
ax.set_ylabel('Z')
ax.set_zlabel('Y')
ax.set_xlim(-8, 8)
ax.set_zlim(0, 4)
ax.set_ylim(-4, 4)

# Plot the optimal camera position
ax.scatter(optimal_position[0], optimal_position[2], optimal_position[1], color='red', label='Optimal Camera')

plt.legend()
plt.show()
