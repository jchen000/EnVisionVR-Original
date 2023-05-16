import json


def add_importance(data):
    if isinstance(data, dict):
        data["importance"] = 0
        if "components" in data:
            for component in data["components"]:
                if component.get("type") == "UnityEngine.MeshRenderer":
                    data["importance"] = data.get("importance", 0) + 0.2
                if component.get("type") == "UnityEngine.Animator":
                    data["importance"] = data.get("importance", 0) + 0.3
        if "name" in data and data["name"] == "Interactables":
            for child in data.get("children", []):
                child["importance"] = child.get("importance", 0) + 0.3
        if "name" in data and data["name"] == "Potions":
            for child in data.get("children", []):
                child["importance"] = child.get("importance", 0) + 0.1
        if "name" in data and data["name"] == "Interior":
            for child in data.get("children", []):
                child["importance"] = child.get("importance", 0) + 0.2
                # if "name" in child:
                #     child_name = child["name"]
                #     if child_name in data:
                #         data[child_name]["importance"] = data[child_name].get("importance", 0) + 0.3
        for key, value in data.items():
            if key == "children" or key == "components":
                for item in value:
                    add_importance(item)
            else:
                add_importance(value)
    elif isinstance(data, list):
        for item in data:
            add_importance(item)


# Load JSON data from file
with open('scene_graph.json') as file:
    json_data = json.load(file)

# Add "importance" property
add_importance(json_data)

# Save modified JSON data back to file
with open('scene_graph_importance.json', 'w') as file:
    json.dump(json_data, file, indent=4)  # Add line breaks with indentation
