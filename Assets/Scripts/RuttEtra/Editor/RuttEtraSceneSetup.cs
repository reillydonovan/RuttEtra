#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class RuttEtraSceneSetup : EditorWindow
{
    [MenuItem("RuttEtra/Setup Scene")]
    public static void SetupScene()
    {
        // Create settings asset if it doesn't exist
        string settingsPath = "Assets/Settings/RuttEtraSettings.asset";
        RuttEtraSettings settings = AssetDatabase.LoadAssetAtPath<RuttEtraSettings>(settingsPath);
        
        if (settings == null)
        {
            settings = ScriptableObject.CreateInstance<RuttEtraSettings>();
            
            // Ensure directory exists
            if (!AssetDatabase.IsValidFolder("Assets/Settings"))
                AssetDatabase.CreateFolder("Assets", "Settings");
            
            AssetDatabase.CreateAsset(settings, settingsPath);
            AssetDatabase.SaveAssets();
            Debug.Log("Created RuttEtraSettings at " + settingsPath);
        }
        
        // Create main controller object
        GameObject controllerObj = new GameObject("RuttEtra_Controller");
        var controller = controllerObj.AddComponent<RuttEtraController>();
        controller.settings = settings;
        
        // Create webcam capture
        GameObject webcamObj = new GameObject("Webcam_Capture");
        webcamObj.transform.SetParent(controllerObj.transform);
        var webcam = webcamObj.AddComponent<WebcamCapture>();
        controller.webcamCapture = webcam;
        
        // Create mesh generator
        GameObject meshObj = new GameObject("RuttEtra_Mesh");
        meshObj.transform.SetParent(controllerObj.transform);
        meshObj.AddComponent<MeshFilter>();
        meshObj.AddComponent<MeshRenderer>();
        var meshGen = meshObj.AddComponent<RuttEtraMeshGenerator>();
        meshGen.settings = settings;
        meshGen.webcamCapture = webcam;
        controller.meshGenerator = meshGen;
        
        // Setup camera
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            mainCam.transform.position = new Vector3(0, 2, -15);
            mainCam.transform.LookAt(Vector3.zero);
            mainCam.backgroundColor = Color.black;
            mainCam.clearFlags = CameraClearFlags.SolidColor;
            controller.mainCamera = mainCam;
        }
        
        // Select the controller
        Selection.activeGameObject = controllerObj;
        
        Debug.Log("RuttEtra scene setup complete!");
        EditorUtility.DisplayDialog("RuttEtra Setup", 
            "Scene setup complete!\n\n" +
            "1. Press Play to start\n" +
            "2. Use Arrow Keys to orbit camera\n" +
            "3. Mouse scroll to zoom\n" +
            "4. Space to invert displacement\n" +
            "5. Tab to toggle source colors\n" +
            "6. V to toggle vertical lines\n" +
            "7. R to reset camera",
            "OK");
    }
    
    [MenuItem("RuttEtra/Create Settings Asset")]
    public static void CreateSettingsAsset()
    {
        string path = "Assets/Settings/RuttEtraSettings.asset";
        
        if (!AssetDatabase.IsValidFolder("Assets/Settings"))
            AssetDatabase.CreateFolder("Assets", "Settings");
        
        RuttEtraSettings settings = ScriptableObject.CreateInstance<RuttEtraSettings>();
        AssetDatabase.CreateAsset(settings, path);
        AssetDatabase.SaveAssets();
        
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = settings;
        
        Debug.Log("Created RuttEtraSettings at " + path);
    }
}
#endif




