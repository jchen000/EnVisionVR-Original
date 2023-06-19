import json
import matplotlib.pyplot as plt
import numpy as np
import random

from mpl_toolkits.mplot3d import Axes3D

# Genetic Algorithm Parameters
POPULATION_SIZE = 100
MAX_GENERATIONS = 100
MUTATION_RATE = 0.01

# Constants for scene boundaries (adjust according to your scene)
X_MIN, X_MAX = -8, 8
Y_MIN, Y_MAX = -4, 4
Z_MIN, Z_MAX = 0, 4

# Global variables to store the virtual objects and their importance values
objects = []
importance_values = []


def parse_json(json_data):
    data = json.loads(json_data)
    # Commented out to prevent plotting during optimization
    # fig = plt.figure()
    # ax = fig.add_subplot(111, projection='3d')
    # plot_element(ax, data)
    # ax.set_xlabel('X')
    # ax.set_ylabel('Z')
    # ax.set_zlabel('Y')
    # ax.set_xlim(-8, 8)
    # ax.set_zlim(0, 4)
    # ax.set_ylim(-4, 4)
    # plt.show()

    collect_objects(data)


def collect_objects(element):
    if "name" and "components" in element and element["importance"] > 0.2 and element["hierarchy_level"] < 3:
        name = element["name"]
        x = element["components"][0]["position"]["x"]
        y = element["components"][0]["position"]["y"]
        z = element["components"][0]["position"]["z"]
        importance = element["importance"]

        objects.append((x, y, z))
        importance_values.append(importance)

    if "children" in element:
        for child in element["children"]:
            collect_objects(child)


def fitness(camera_position):
    captured_objects = 0

    for i, obj in enumerate(objects):
        distance = np.linalg.norm(np.array(obj) - np.array(camera_position))
        weight = importance_values[i]

        if distance <= weight:
            captured_objects += 1

    return captured_objects


def generate_random_camera_position():
    x = random.uniform(X_MIN, X_MAX)
    y = random.uniform(Y_MIN, Y_MAX)
    z = random.uniform(Z_MIN, Z_MAX)
    return x, y, z


def mutate_camera_position(camera_position):
    x, y, z = camera_position
    x += random.uniform(-1, 1) * MUTATION_RATE
    y += random.uniform(-1, 1) * MUTATION_RATE
    z += random.uniform(-1, 1) * MUTATION_RATE

    # Ensure camera position is within the scene boundaries
    x = max(X_MIN, min(x, X_MAX))
    y = max(Y_MIN, min(y, Y_MAX))
    z = max(Z_MIN, min(z, Z_MAX))

    return x, y, z


def genetic_algorithm():
    population = []

    for _ in range(POPULATION_SIZE):
        population.append(generate_random_camera_position())

    for generation in range(MAX_GENERATIONS):
        population_fitness = [fitness(camera_position) for camera_position in population]
        best_fitness = max(population_fitness)
        best_index = population_fitness.index(best_fitness)
        best_camera_position = population[best_index]

        print(f"Generation {generation + 1}: Best Fitness = {best_fitness}")

        # Perform selection and reproduction
        next_generation = []
        for _ in range(POPULATION_SIZE):
            parent1 = select_parent(population, population_fitness)
            parent2 = select_parent(population, population_fitness)
            child = crossover(parent1, parent2)
            child = mutate(child)
            next_generation.append(child)

        population = next_generation

        # Return the best camera position found
    return best_camera_position

def select_parent(population, fitness_values):
    # Use roulette wheel selection
    total_fitness = sum(fitness_values) + 1e-10
    probabilities = [fitness / total_fitness for fitness in fitness_values]
    return random.choices(population, probabilities)[0]

def crossover(parent1, parent2):
    # Use uniform crossover
    child = []
    for i in range(3):
        if random.random() < 0.5:
            child.append(parent1[i])
        else:
            child.append(parent2[i])
    return tuple(child)

def mutate(camera_position):
    # Perform mutation on each coordinate
    mutated_position = []
    for coord in camera_position:
        if random.random() < MUTATION_RATE:
            coord += random.uniform(-1, 1) * MUTATION_RATE
            # Ensure camera position is within the scene boundaries
            coord = max(min(coord, X_MAX), X_MIN)
        mutated_position.append(coord)
    return tuple(mutated_position)

# Read the JSON file
with open("Assets/scene_graph_importance.json") as file:
    json_data = file.read()

# Parse the JSON data
parse_json(json_data)

# Perform genetic algorithm for camera optimization
best_camera_position = genetic_algorithm()

print("Optimal Camera Position:")
print(f"X: {best_camera_position[0]}")
print(f"Y: {best_camera_position[1]}")
print(f"Z: {best_camera_position[2]}")

