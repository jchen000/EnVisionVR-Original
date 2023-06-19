import json
import matplotlib.pyplot as plt
import numpy as np

from mpl_toolkits.mplot3d import Axes3D

def parse_json(json_data):
    data = json.loads(json_data)
    fig = plt.figure()
    ax = fig.add_subplot(111, projection='3d')
    plot_element(ax, data)
    ax.set_xlabel('X')
    ax.set_ylabel('Z')
    ax.set_zlabel('Y')
    ax.set_xlim(-8, 8)
    ax.set_zlim(0, 4)
    ax.set_ylim(-4, 4)
    plt.show()

def plot_element(ax, element):
    # if "name" and "components" in element and element["hierarchy_level"] < 2:
    if "name" and "components" in element and element["importance"] > 0.2 and element["hierarchy_level"] < 3:
        name = element["name"]
        x = element["components"][0]["position"]["x"]
        y = element["components"][0]["position"]["y"]
        z = element["components"][0]["position"]["z"]
        importance = element["importance"]
        ax.scatter(x, z, y)
        ax.text(x, z, y, name)
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
        ax.quiver(x, y, z, rotated_vector[0], rotated_vector[1], rotated_vector[2], color='blue', label='Vector')


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

# Parse the JSON data
parse_json(json_data)
