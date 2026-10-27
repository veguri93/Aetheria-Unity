using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine.Rendering;
using System.IO;
using System.Linq;
using Aetheria.Weapons;

public static class AetheriaBowSetup
{
    const string Stem = "Aetheria_Sunspire_Bow";
    [MenuItem("Tools/Aetheria/Create Sunspire Bow Prefab")]
    public static void Build()
    {
        string path = AssetDatabase.FindAssets(Stem + " t:Model").Select(AssetDatabase.GUIDToAssetPath)
            .FirstOrDefault(p => Path.GetFileName(p) == Stem + ".fbx");
        if (path == null) { Debug.LogError("Place the complete bow folder under Assets first."); return; }
        string root = Path.GetDirectoryName(path).Replace('\\','/');
        var importer = (ModelImporter)AssetImporter.GetAtPath(path);
        importer.animationType = ModelImporterAnimationType.Generic;
        importer.optimizeGameObjects = false;
        importer.importAnimation = true;
        importer.importCameras = false; importer.importLights = false;
        importer.importTangents = ModelImporterTangents.CalculateMikk;
        importer.SaveAndReimport();
        var clips = importer.defaultClipAnimations;
        foreach (var clip in clips) clip.loopTime = false;
        importer.clipAnimations = clips; importer.SaveAndReimport();
        string pipeline = GraphicsSettings.currentRenderPipeline == null ? "BuiltIn" : GraphicsSettings.currentRenderPipeline.GetType().Name;
        bool hdrp = pipeline.Contains("HDRender"), urp = pipeline.Contains("Universal");
        var shader = Shader.Find(hdrp ? "HDRP/Lit" : urp ? "Universal Render Pipeline/Lit" : "Standard");
        if (!shader) { Debug.LogError("The active pipeline's Lit shader is unavailable."); return; }
        Texture2D Load(string suffix, bool srgb, bool normal = false)
        {
            string p = root + "/Textures/" + Stem + "_" + suffix + ".png";
            var ti = (TextureImporter)AssetImporter.GetAtPath(p);
            ti.textureType = normal ? TextureImporterType.NormalMap : TextureImporterType.Default;
            ti.sRGBTexture = srgb; ti.alphaSource = TextureImporterAlphaSource.FromInput;
            ti.alphaIsTransparency = false; ti.maxTextureSize = 2048; ti.wrapMode = TextureWrapMode.Clamp;
            ti.SaveAndReimport(); return AssetDatabase.LoadAssetAtPath<Texture2D>(p);
        }
        string matPath = root + "/" + Stem + ".mat";
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
        if (!mat) { mat = new Material(shader); AssetDatabase.CreateAsset(mat, matPath); } else mat.shader = shader;
        mat.SetTexture(hdrp ? "_BaseColorMap" : urp ? "_BaseMap" : "_MainTex", Load("BaseColor", true));
        mat.SetColor(hdrp || urp ? "_BaseColor" : "_Color", Color.white);
        mat.SetTexture(hdrp ? "_NormalMap" : "_BumpMap", Load("NormalGL", false, true));
        mat.EnableKeyword(hdrp ? "_NORMALMAP_TANGENT_SPACE" : "_NORMALMAP");
        if (hdrp) {
            mat.SetTexture("_MaskMap", Load("HDRPMask", false)); mat.EnableKeyword("_MASKMAP");
            mat.SetFloat("_Metallic", 1); mat.SetFloat("_SmoothnessRemapMin", 0); mat.SetFloat("_SmoothnessRemapMax", 1);
        } else {
            mat.SetTexture("_MetallicGlossMap", Load("UnityMetallicSmoothness", false));
            mat.EnableKeyword(urp ? "_METALLICSPECGLOSSMAP" : "_METALLICGLOSSMAP");
            mat.SetFloat("_Metallic", 1); mat.SetFloat(urp ? "_Smoothness" : "_GlossMapScale", 1);
            if (urp) mat.SetFloat("_WorkflowMode", 1);
        }
        var holder = new GameObject(Stem);
        var model = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(path));
        model.transform.SetParent(holder.transform, false);
        Transform Find(string name) => model.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == name);
        var draw = Find("String_Draw"); var upper = Find("String_Tip_Upper"); var lower = Find("String_Tip_Lower");
        var renderers = model.GetComponentsInChildren<SkinnedMeshRenderer>(true);
        var cable = renderers.FirstOrDefault(r => r.name == "Bow_String");
        var body = renderers.FirstOrDefault(r => r.name == "Bow_Body");
        if (!draw || !upper || !lower || !cable || !body) {
            Object.DestroyImmediate(holder); Debug.LogError("Expected two skinned meshes and named string controls were not found. Leave Optimize Game Objects disabled."); return;
        }
        foreach (var r in renderers) {
            r.sharedMaterial = mat;
            // Avoid a resting-pose bound culling the drawn cable.
            r.updateWhenOffscreen = true;
            Bounds bounds = r.localBounds; bounds.Expand(1.0f); r.localBounds = bounds;
        }
        var driver = holder.AddComponent<AetheriaBowStringDriver>(); driver.Configure(draw);
        var animator = model.GetComponent<Animator>();
        if (animator) { animator.runtimeAnimatorController = null; animator.cullingMode = AnimatorCullingMode.AlwaysAnimate; }
        string lodPath = root + "/" + Stem + "_Body_LOD1.fbx";
        var lodAsset = AssetDatabase.LoadAssetAtPath<GameObject>(lodPath);
        if (lodAsset) {
            var low = (GameObject)PrefabUtility.InstantiatePrefab(lodAsset); low.transform.SetParent(holder.transform, false);
            var lowRenderers = low.GetComponentsInChildren<Renderer>(); foreach (var r in lowRenderers) r.sharedMaterial = mat;
            var group = holder.AddComponent<LODGroup>();
            group.SetLODs(new[] { new LOD(.25f, new Renderer[] { body }), new LOD(0, lowRenderers) });
            group.RecalculateBounds();
            // String stays separate and skinned at both body LODs. Last LOD does not cull.
        }
        // Optional clip-driven demonstration. Not assigned while the script owns String_Draw.
        string controllerPath = root + "/BowClipDemo.controller";
        if (!AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath)) {
            var controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
            controller.AddParameter("Draw", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("Release", AnimatorControllerParameterType.Trigger);
            var sm = controller.layers[0].stateMachine;
            AnimationClip GetClip(string name) => AssetDatabase.LoadAllAssetsAtPath(path).OfType<AnimationClip>().FirstOrDefault(c => !c.name.StartsWith("__preview__") && c.name.EndsWith(name));
            var rest = sm.AddState("Rest"); rest.motion = GetClip("Bow_Rest");
            var drawing = sm.AddState("DrawAndHold"); drawing.motion = GetClip("Bow_Draw");
            var release = sm.AddState("Release"); release.motion = GetClip("Bow_Release"); sm.defaultState = rest;
            var a = rest.AddTransition(drawing); a.hasExitTime = false; a.duration = 0; a.AddCondition(AnimatorConditionMode.If, 0, "Draw");
            var b = drawing.AddTransition(release); b.hasExitTime = false; b.duration = .025f; b.AddCondition(AnimatorConditionMode.If, 0, "Release");
            var c = release.AddTransition(rest); c.hasExitTime = true; c.exitTime = 1; c.duration = 0;
        }
        string prefab = root + "/" + Stem + ".prefab";
        PrefabUtility.SaveAsPrefabAsset(holder, prefab); Object.DestroyImmediate(holder);
        EditorUtility.SetDirty(mat); AssetDatabase.SaveAssets();
        Selection.activeObject = AssetDatabase.LoadAssetAtPath<GameObject>(prefab);
        Debug.Log("Sunspire bow prefab created. Script-driven string is active. For clip-driven playback, disable the driver and assign BowClipDemo to the model Animator.");
    }
}
