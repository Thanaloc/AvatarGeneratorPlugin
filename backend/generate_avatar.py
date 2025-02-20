import bpy
import sys
import os
import bpy

# 🔥 Supprimer tous les objets de la scène (dont le cube de base)
bpy.ops.object.select_all(action='SELECT')  # Sélectionner tous les objets
bpy.ops.object.delete()  # Supprimer la sélection

print("🗑️ Cube de départ supprimé !")

# Récupérer les arguments
argv = sys.argv
argv = argv[argv.index("--") + 1:]

print("Arguments reçus :", sys.argv)

photo_path, gender, height, weight, export_path = argv

MODEL_PATH = os.path.abspath("models/baseModel.fbx")
print("chargement :", MODEL_PATH)
bpy.ops.import_scene.fbx(filepath=MODEL_PATH)
print("✅ Modèle 3D chargé :", MODEL_PATH)

height = float(height)
weight = float(weight)

# Ajuste les proportions
avatar = bpy.context.selected_objects[0]

# Calcul du facteur d'échelle
base_height = 180  # Hauteur du modèle de base en cm (à ajuster si besoin)
scale_factor = height / base_height  # Normalisation de la taille

# Appliquer l’échelle sur tout l’avatar
avatar.scale = (scale_factor, scale_factor, scale_factor)

# Appliquer les transformations pour rendre le scale effectif
bpy.ops.object.transform_apply(scale=True)

# Export du modèle
bpy.ops.export_scene.fbx(filepath=export_path)

print("Avatar exporté :", export_path)
