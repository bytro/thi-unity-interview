using UnityEditor;
using UnityEngine;

// Run via Tools > Interview > Setup Scene to populate SampleScene with test geometry.
public static class SceneSetupHelper
{
    [MenuItem("Tools/Interview/Setup Scene")]
    public static void SetupScene()
    {
        // Ground plane
        CreatePrimitive(PrimitiveType.Plane, Vector3.zero, Vector3.one * 6f, "Ground");

        // A cluster of shapes at varying depths and heights
        CreatePrimitive(PrimitiveType.Cube,     new Vector3(-2f,  0.5f,  0f),  Vector3.one,           "Cube");
        CreatePrimitive(PrimitiveType.Sphere,   new Vector3( 0f,  0.5f,  0f),  Vector3.one,           "Sphere");
        CreatePrimitive(PrimitiveType.Cylinder, new Vector3( 2f,  1f,    0f),  new Vector3(1, 2, 1),  "Cylinder");
        CreatePrimitive(PrimitiveType.Capsule,  new Vector3(-1f,  1.1f, -2f),  Vector3.one,           "Capsule");
        CreatePrimitive(PrimitiveType.Cube,     new Vector3( 1.5f, 0.3f,-2f),  new Vector3(2, 0.5f, 1), "FlatBox");

        // Directional light
        if (Object.FindFirstObjectByType<Light>() == null)
        {
            var lightGo = new GameObject("Directional Light");
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1f;
            lightGo.transform.rotation = Quaternion.Euler(45f, -30f, 0f);
        }

        // Camera at a useful angle
        var cam = Camera.main;
        if (cam != null)
        {
            cam.transform.position = new Vector3(0f, 4f, -7f);
            cam.transform.rotation = Quaternion.Euler(25f, 0f, 0f);
        }

        Debug.Log("[Interview] Scene populated. Add OutlineFeature to Assets/Settings/PC_Renderer.asset to test.");
    }

    private static void CreatePrimitive(PrimitiveType type, Vector3 position, Vector3 scale, string name)
    {
        var go = GameObject.CreatePrimitive(type);
        go.name = name;
        go.transform.position = position;
        go.transform.localScale = scale;
    }
}
