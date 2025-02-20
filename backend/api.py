from fastapi import FastAPI, File, UploadFile, Form
import subprocess
import shutil
import os
import sys
import uvicorn

pid_file = "fastapi_pid.txt"
with open(pid_file, "w") as f:
    f.write(str(os.getpid()))
    
print("🔥 FastAPI is starting...", file=sys.stderr)
sys.stderr.flush()

app = FastAPI()

# ✅ Force FastAPI à rester actif
if __name__ == "__main__":
    print("🚀 Running FastAPI...")
    uvicorn.run(app, host="127.0.0.1", port=8000)

UPLOAD_DIR = "uploads"
EXPORT_DIR = "exports"

os.makedirs(UPLOAD_DIR, exist_ok=True)
os.makedirs(EXPORT_DIR, exist_ok=True)

@app.get("/")
def read_root():
    return {"message": "API is running!"}

@app.post("/generate-avatar/")
async def generate_avatar(
    file: UploadFile = File(...),
    gender: str = Form(...),
    height: float = Form(...),
    weight: float = Form(...)
):
    """ Reçoit la photo et les paramètres, lance Blender pour générer l’avatar """
    
    photo_path = os.path.join(UPLOAD_DIR, file.filename)
    export_path = os.path.join(EXPORT_DIR, f"{file.filename}.fbx")
    
    # Sauvegarde la photo
    with open(photo_path, "wb") as buffer:
        shutil.copyfileobj(file.file, buffer)

    BLENDER_PATH = r"C:\Program Files\Blender Foundation\Blender 4.3\blender.exe"

    # Lance Blender avec le script Python de génération
    subprocess.run([
       BLENDER_PATH, "--background", "--python", "generate_avatar.py",
        "--", photo_path, gender, str(height), str(weight), export_path
    ])
    
    return {"message": "Avatar generated", "file": export_path}
