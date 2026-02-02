#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.VFX;

public static class RuttEtraVFXSetup
{
    [MenuItem("RuttEtra/Setup VFX Graph Scene")]
    public static void SetupVFXScene()
    {
        // Check if VFX Graph package is installed
        var vfxType = System.Type.GetType("UnityEngine.VFX.VisualEffect, Unity.VisualEffectGraph.Runtime");
        if (vfxType == null)
        {
            EditorUtility.DisplayDialog("VFX Graph Required", 
                "Please install the Visual Effect Graph package first:\n\n" +
                "Window > Package Manager > Visual Effect Graph", "OK");
            return;
        }
        
        // Find or create controller
        var controller = Object.FindFirstObjectByType<RuttEtraController>();
        if (controller == null)
        {
            EditorUtility.DisplayDialog("Setup Required", 
                "Please run 'RuttEtra > Setup Scene' first to create the base scene.", "OK");
            return;
        }
        
        // Disable mesh-based renderer
        var meshObj = GameObject.Find("RuttEtra_Mesh");
        if (meshObj != null)
        {
            meshObj.SetActive(false);
            Debug.Log("Disabled mesh-based renderer");
        }
        
        // Create VFX GameObject
        var vfxObj = GameObject.Find("RuttEtra_VFX");
        if (vfxObj == null)
        {
            vfxObj = new GameObject("RuttEtra_VFX");
            vfxObj.transform.SetParent(controller.transform);
            vfxObj.transform.localPosition = Vector3.zero;
        }
        
        // Add VisualEffect component
        var vfx = vfxObj.GetComponent<VisualEffect>();
        if (vfx == null)
            vfx = vfxObj.AddComponent<VisualEffect>();
        
        // Add bridge script
        var bridge = vfxObj.GetComponent<RuttEtraVFX>();
        if (bridge == null)
            bridge = vfxObj.AddComponent<RuttEtraVFX>();
        
        bridge.settings = controller.settings;
        bridge.webcamCapture = controller.webcamCapture;
        
        // Check for VFX asset
        string vfxAssetPath = "Assets/VFX/RuttEtraVFX.vfx";
        var vfxAsset = AssetDatabase.LoadAssetAtPath<VisualEffectAsset>(vfxAssetPath);
        
        if (vfxAsset == null)
        {
            EditorUtility.DisplayDialog("VFX Graph Asset Needed", 
                "Please create a VFX Graph asset:\n\n" +
                "1. Right-click in Project > Create > Visual Effects > Visual Effect Graph\n" +
                "2. Name it 'RuttEtraVFX' and save to Assets/VFX/\n" +
                "3. Open it and follow the setup instructions in RuttEtraVFX.cs\n\n" +
                "Or see README for detailed VFX Graph setup guide.", "OK");
            
            // Create VFX folder
            if (!AssetDatabase.IsValidFolder("Assets/VFX"))
                AssetDatabase.CreateFolder("Assets", "VFX");
        }
        else
        {
            vfx.visualEffectAsset = vfxAsset;
        }
        
        Selection.activeGameObject = vfxObj;
        EditorUtility.SetDirty(vfxObj);
        
        Debug.Log("VFX Graph scene setup complete. Assign your VFX Graph asset to the VisualEffect component.");
    }
    
    [MenuItem("RuttEtra/Create VFX Graph Template")]
    public static void CreateVFXTemplate()
    {
        // Create VFX folder
        if (!AssetDatabase.IsValidFolder("Assets/VFX"))
            AssetDatabase.CreateFolder("Assets", "VFX");
        
        // Show instructions since we can't create .vfx programmatically easily
        EditorUtility.DisplayDialog("VFX Graph Setup Instructions",
            "To create the Rutt/Etra VFX Graph:\n\n" +
            "1. Right-click Assets/VFX > Create > Visual Effects > Visual Effect Graph\n" +
            "2. Name it 'RuttEtraVFX'\n" +
            "3. Double-click to open the VFX Graph editor\n\n" +
            "4. Add Blackboard properties:\n" +
            "   - WebcamTexture (Texture2D)\n" +
            "   - Resolution (Vector2, default 128,64)\n" +
            "   - DisplacementStrength (Float, default 2)\n" +
            "   - MeshWidth (Float, default 16)\n" +
            "   - MeshHeight (Float, default 9)\n" +
            "   - PrimaryColor (Color, green)\n" +
            "   - SecondaryColor (Color, cyan)\n" +
            "   - LineWidth (Float, default 0.02)\n" +
            "   - Brightness (Float, default 0)\n" +
            "   - Contrast (Float, default 1)\n\n" +
            "5. See detailed node setup in README.md",
            "OK");
    }
}
#endif
