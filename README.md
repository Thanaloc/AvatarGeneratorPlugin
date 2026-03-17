# Avatar Generator Unity Plugin 🎭🎮

Welcome to the Avatar Generator Plugin for Unity! This tool allows you to automatically generate realistic, rigged 3D avatars based on user-defined parameters (height, gender, and an image). The generation process is handled via Blender and FastAPI, providing high-quality models ready for use in your Unity projects.

📌 Prerequisites
Before installing the plugin, make sure you have the following software installed on your system:

	- Python (required for running FastAPI) - Download from the official Python website
	- Blender (used for 3D avatar generation) - Download from the official Blender website

Note: Python scripting must be enabled in Blender. You can enable it by going to:
Edit > Preferences > Save & Load > Auto Run Python Scripts and enabling the option.

🚀 Installation Guide

Follow these steps in order to set up the Avatar Generator Plugin in Unity:

1. Install Python (make sure to check "Add Python to PATH" during installation).
2. Install Blender (version 4.x or later).
3. Enable Python Scripting in Blender:
	- Open Blender.
	- Go to Edit > Preferences > Save & Load.
	- Enable "Auto Run Python Scripts".
4. Install FastAPI & Uvicorn:
	- Open Windows PowerShell as Administrator and run: pip install fastapi uvicorn
5. Install the Unity Package:
	- Open your Unity project.
	- Import the provided .unitypackage file into Unity.
6. Open the Avatar Generator Panel:
	- Navigate to Tools → Avatar Generator in Unity.
7. Start the API and Generate an Avatar:
	- Click "Start API" (this launches the FastAPI server).
	- Fill in the required parameters (height, weight, gender, and image).
	- Click "Generate Avatar".
8. Retrieve Your Generated Avatars:
	- Your generated avatars will be saved in the "exports" folder inside your Unity project.

❓ Troubleshooting

1️⃣ API doesn't start / Python not found
	- Ensure that Python is installed and added to PATH.
	- Run python --version in PowerShell to verify.
	- If Python is missing, reinstall it from the official Python website.

2️⃣ API starts but Unity can't connect
	- Check if the API is running by opening a browser and going to: http://127.0.0.1:8000.
	- If the page doesn’t load, restart Unity and try again.

3️⃣ Generated avatars are missing / not exported
	- Ensure Blender is installed and Python scripting is enabled.
	- Check if the "exports" folder exists inside your Unity project.

💡 Additional Notes

	- This plugin is designed for Unity 2021+.
	- Make sure your Blender version is compatible with the Python API.
	- The system may require Admin privileges to install dependencies.

🛠️ Future Improvements

✅ Better avatar customization (facial features, clothing, accessories).
✅ Improved error handling & debugging tools.
✅ Support for additional 3D formats.

📜 License

This project is licensed under the MIT License. Feel free to modify and distribute!

📧 Need Help?

If you encounter any issues, feel free to open an issue on this repository or contact us via GitHub Discussions.

🚀 Happy coding! 🎮🎭
