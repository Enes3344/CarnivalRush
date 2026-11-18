using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace CarnivalRush.Editor
{
    public static class GenerateCarnival
    {
        private const string Root = "Assets/CarnivalRush/Generated";
        private const string ScenePath = Root + "/CarnivalRush.unity";

        [MenuItem("Tools/Carnival Rush/Generate Game")]
        public static void GenerateGame()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                return;
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;

            if (!AssetDatabase.IsValidFolder(Root))
                AssetDatabase.CreateFolder("Assets/CarnivalRush", "Generated");
            if (!AssetDatabase.IsValidFolder(Root + "/Materials"))
                AssetDatabase.CreateFolder(Root, "Materials");

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            BuildLighting();
            BalloonWall wall = BuildBooth();
            ToyShooter shooter = BuildPlayer();
            var hud = new GameObject("Carnival HUD", typeof(RectTransform)).AddComponent<CarnivalHud>();
            hud.Build();
            new GameObject("Event System", typeof(EventSystem), typeof(StandaloneInputModule));
            new GameObject("Carnival Game").AddComponent<CarnivalGame>().Configure(shooter, wall, hud);

            if (!EditorSceneManager.SaveScene(scene, ScenePath))
                throw new System.InvalidOperationException("Unable to save " + ScenePath);

            var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            int index = scenes.FindIndex(item => item.path == ScenePath);
            var entry = new EditorBuildSettingsScene(ScenePath, true);
            if (index < 0)
                scenes.Add(entry);
            else
                scenes[index] = entry;
            EditorBuildSettings.scenes = scenes.ToArray();
            AssetDatabase.SaveAssets();
            Debug.Log("Carnival Rush generated. Enter Play mode to start.");
        }

        private static BalloonWall BuildBooth()
        {
            var room = new GameObject("Carnival shooting gallery").transform;
            Material walnut = Skin("Walnut", 0x633B2C);
            Material oak = Skin("Amber oak", 0xB37043);
            Material plum = Skin("Plum wall", 0x3B283C);
            Material board = Skin("Target board", 0x312941);
            Material line = Skin("Board slats", 0x4A3850);
            Material red = Skin("Carnival red", 0xA82F42);
            Material cream = Skin("Ivory", 0xFFE3AF);
            Material gold = Skin("Marquee gold", 0xFFB847, 0.28f, 0.22f);

            var floor = Box("Main floor", room, 0, -0.17f, 2.6f, 13, 0.30f, 15, walnut);
            Mesh cube = floor.GetComponent<MeshFilter>().sharedMesh;
            for (int i = -9; i <= 9; i++)
                Box("Floor plank", room, i * 0.66f, -0.012f, 2.6f, 0.64f, 0.03f, 15,
                    i % 2 == 0 ? oak : walnut, false);
            Box("Ceiling", room, 0, 6.36f, 2.6f, 13, 0.22f, 15, plum);
            Box("Room back", room, 0, 3.0f, 7.56f, 13, 6.4f, 0.3f, plum);
            Box("Target backing", room, 0, 2.75f, 6.37f, 7.38f, 3.93f, 0.26f, board);
            for (int i = 0; i <= 8; i++)
                Box("Recessed board seam", room, 0, 0.81f + i * 0.48f, 6.23f, 7.3f, 0.022f, 0.015f, line, false);

            foreach (int side in new[] { -1, 1 })
            {
                Box("Side wall", room, side * 6.4f, 3.15f, 2.6f, 0.22f, 6.3f, 15, plum);
                Box("Booth upright", room, side * 3.8f, 2.73f, 6.0f, 0.28f, 4.13f, 0.45f, oak);
                Box("Gold edge", room, side * 3.8f, 2.73f, 5.76f, 0.08f, 4.12f, 0.045f, gold, false);
                MakeCrate(room, side * 4.65f, 0.35f, 4.4f, walnut, oak);
                MakeCrate(room, side * 5.04f, 1.0f, 4.73f, walnut, oak);
                for (int i = 0; i < 6; i++)
                    Box("Striped side drape", room, side * (4.13f + i * 0.28f), 3.0f,
                        6.75f + 0.06f * (i % 2), 0.28f, 4.6f, 0.17f, i % 2 == 0 ? red : cream);
            }

            Box("Counter front", room, 0, 0.60f, 0.1f, 7.8f, 1.2f, 0.27f, walnut);
            Box("Countertop", room, 0, 1.24f, 0.1f, 8.1f, 0.17f, 0.90f, oak);
            Box("Lower target rail", room, 0, 0.68f, 5.94f, 7.83f, 0.23f, 0.45f, oak);
            Box("Marquee backing", room, 0, 5.14f, 5.98f, 7.83f, 0.79f, 0.52f, red);
            Box("Marquee gold rail", room, 0, 4.76f, 5.69f, 7.8f, 0.095f, 0.12f, gold);
            Box("Marquee top rail", room, 0, 5.54f, 5.71f, 7.8f, 0.10f, 0.15f, gold);
            AddSign(room);
            for (int i = 0; i < 20; i++)
                Box("Striped awning", room, -3.705f + i * 0.39f, 5.73f, 5.81f,
                    0.39f, 0.23f, 0.87f, i % 2 == 0 ? red : cream);

            Material[] bulbs =
            {
                Skin("Amber bulb", 0xFFCA59, 0.4f, 0.8f),
                Skin("Coral bulb", 0xFF556B, 0.4f, 0.8f),
                Skin("Mint bulb", 0x68F4CB, 0.4f, 0.8f),
                Skin("Lilac bulb", 0xBA87FC, 0.4f, 0.8f)
            };
            Transform firstBulb = Part(PrimitiveType.Sphere, "Colored marquee bulb", room,
                new Vector3(-3.66f, 4.77f, 5.59f), Vector3.one * 0.13f, bulbs[0], false);
            Mesh sphere = firstBulb.GetComponent<MeshFilter>().sharedMesh;
            for (int i = 1; i < 19; i++)
            {
                float x = -3.66f + i * 0.406f;
                Part(PrimitiveType.Sphere, "Colored marquee bulb", room,
                    new Vector3(x, 4.77f, 5.59f), Vector3.one * 0.13f, bulbs[i % bulbs.Length], false);
            }

            var wall = new GameObject("Balloon targets").AddComponent<BalloonWall>();
            wall.transform.SetParent(room, false);
            wall.transform.localPosition = new Vector3(0, 2.75f, 5.61f);
            var palette = new[]
            {
                Skin("Cherry red", 0xEB3549, 0.61f, 0.10f),
                Skin("Sky blue", 0x34AEFF, 0.61f, 0.11f),
                Skin("Lemon yellow", 0xFFD83B, 0.61f, 0.13f),
                Skin("Apple green", 0x60DF56, 0.61f, 0.11f),
                Skin("Grape purple", 0xBA62FF, 0.61f, 0.13f),
                Skin("Danger black", 0x10101A, 0.34f)
            };
            wall.Configure(sphere, cube, palette, Skin("Hazard cross", 0xFFE9CB, 0.15f, 0.3f), cream,
                Skin("Confetti", 0xFFFFFF, shaderName: "Legacy Shaders/Particles/Alpha Blended"));
            return wall;
        }

        private static void AddSign(Transform parent)
        {
            var sign = new GameObject("Carnival Rush marquee", typeof(MeshRenderer), typeof(TextMesh));
            sign.transform.SetParent(parent, false);
            sign.transform.localPosition = new Vector3(0, 5.15f, 5.70f);
            var text = sign.GetComponent<TextMesh>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            sign.GetComponent<MeshRenderer>().sharedMaterial = text.font.material;
            text.text = "C A R N I V A L   R U S H";
            text.anchor = TextAnchor.MiddleCenter;
            text.alignment = TextAlignment.Center;
            text.fontSize = 72;
            text.fontStyle = FontStyle.Bold;
            text.characterSize = 0.055f;
            text.color = new Color32(255, 225, 174, 255);
        }

        private static void MakeCrate(Transform room, float x, float y, float z, Material wood, Material edge)
        {
            Box("Prize crate", room, x, y, z, 0.75f, 0.65f, 0.66f, wood);
            foreach (int side in new[] { -1, 1 })
            {
                Box("Crate horizontal batten", room, x, y + side * 0.26f, z - 0.34f,
                    0.73f, 0.09f, 0.06f, edge, false);
                var batten = Box("Crate cross brace", room, x, y, z - 0.35f,
                    0.75f, 0.062f, 0.06f, edge, false);
                batten.localRotation = Quaternion.Euler(0, 0, side * 38);
            }
        }

        private static ToyShooter BuildPlayer()
        {
            var player = new GameObject("Gallery player");
            player.transform.position = new Vector3(0, 2.75f, -1.7f);
            var cameraObject = new GameObject("First-person camera", typeof(Camera), typeof(AudioListener), typeof(AudioSource));
            cameraObject.tag = "MainCamera";
            cameraObject.transform.SetParent(player.transform, false);
            var camera = cameraObject.GetComponent<Camera>();
            camera.fieldOfView = 54f;
            camera.nearClipPlane = 0.035f;
            camera.farClipPlane = 40f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color32(39, 21, 32, 255);
            var source = cameraObject.GetComponent<AudioSource>();
            source.playOnAwake = false;

            Material turquoise = Skin("Toy turquoise", 0x1B9EAE, 0.48f);
            Material orange = Skin("Toy orange", 0xFF7039, 0.42f);
            Material white = Skin("Toy ivory", 0xFFEBC4, 0.35f);
            Material flashMat = Skin("Muzzle glow", 0xFFCF68, 0.2f, 2f);
            var gun = new GameObject("Toy blaster").transform;
            gun.SetParent(camera.transform, false);
            gun.localPosition = new Vector3(0.22f, -0.19f, 0.52f);
            Box("Blaster body", gun, 0, 0, 0, 0.18f, 0.15f, 0.31f, turquoise, false);
            Box("Grip", gun, 0, -0.115f, -0.08f, 0.11f, 0.19f, 0.095f, orange, false)
                .localRotation = Quaternion.Euler(16f, 0, 0);
            Box("Ivory inset", gun, 0, 0.071f, -0.025f, 0.123f, 0.032f, 0.22f, white, false);
            Part(PrimitiveType.Cylinder, "Orange muzzle", gun, new Vector3(0, 0.012f, 0.19f),
                new Vector3(0.145f, 0.067f, 0.145f), orange, false).localRotation = Quaternion.Euler(90, 0, 0);
            Part(PrimitiveType.Cylinder, "Muzzle inset", gun, new Vector3(0, 0.012f, 0.255f),
                new Vector3(0.073f, 0.008f, 0.073f), turquoise, false).localRotation = Quaternion.Euler(90, 0, 0);
            Transform flash = Part(PrimitiveType.Sphere, "Muzzle flash", gun, new Vector3(0, 0.012f, 0.315f),
                new Vector3(0.11f, 0.11f, 0.16f), flashMat, false);
            flash.gameObject.SetActive(false);
            foreach (Renderer renderer in gun.GetComponentsInChildren<Renderer>(true))
                renderer.shadowCastingMode = ShadowCastingMode.Off;
            var shooter = player.AddComponent<ToyShooter>();
            shooter.Configure(camera, gun, flash.gameObject, source);
            return shooter;
        }

        private static void BuildLighting()
        {
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color32(177, 151, 153, 255);
            var key = new GameObject("Warm gallery key").AddComponent<Light>();
            key.type = LightType.Directional;
            key.color = new Color32(255, 222, 188, 255);
            key.intensity = 0.85f;
            key.transform.rotation = Quaternion.Euler(26f, -21f, 0);
            AddPoint("Marquee glow left", new Vector3(-2.9f, 4.62f, 4.85f), 0xFFB872);
            AddPoint("Marquee glow right", new Vector3(2.9f, 4.62f, 4.85f), 0xFF9F7D);
        }

        private static void AddPoint(string label, Vector3 position, int rgb)
        {
            var light = new GameObject(label).AddComponent<Light>();
            light.type = LightType.Point;
            light.transform.position = position;
            light.intensity = 1.45f;
            light.range = 6.5f;
            light.color = Rgb(rgb);
        }

        private static Transform Box(string name, Transform parent, float x, float y, float z,
            float width, float height, float depth, Material material, bool solid = true)
        {
            return Part(PrimitiveType.Cube, name, parent, new Vector3(x, y, z),
                new Vector3(width, height, depth), material, solid);
        }

        private static Transform Part(PrimitiveType kind, string name, Transform parent,
            Vector3 position, Vector3 scale, Material material, bool solid)
        {
            GameObject piece = GameObject.CreatePrimitive(kind);
            piece.name = name;
            piece.transform.SetParent(parent, false);
            piece.transform.localPosition = position;
            piece.transform.localScale = scale;
            piece.GetComponent<Renderer>().sharedMaterial = material;
            if (!solid)
                Object.DestroyImmediate(piece.GetComponent<Collider>());
            return piece.transform;
        }

        private static Material Skin(string name, int rgb, float gloss = 0.18f,
            float glow = 0f, string shaderName = "Standard")
        {
            string path = $"{Root}/Materials/{name}.mat";
            Shader shader = Shader.Find(shaderName);
            if (shader == null)
                throw new System.InvalidOperationException("Missing Built-in shader: " + shaderName);

            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, path);
            }
            material.shader = shader;
            if (material.HasProperty("_Color"))
                material.SetColor("_Color", Rgb(rgb));
            if (material.HasProperty("_TintColor"))
                material.SetColor("_TintColor", Rgb(rgb));
            if (material.HasProperty("_Glossiness"))
                material.SetFloat("_Glossiness", gloss);
            if (material.HasProperty("_EmissionColor"))
            {
                if (glow > 0f)
                    material.EnableKeyword("_EMISSION");
                else
                    material.DisableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", Rgb(rgb) * glow);
            }
            EditorUtility.SetDirty(material);
            return material;
        }

        private static Color Rgb(int rgb) => new Color32(
            (byte)(rgb >> 16), (byte)(rgb >> 8), (byte)rgb, 255);
    }
}
