using System.Collections.Generic;
using UnityEngine;

namespace CoreEngine.LevelDesign
{
    public static class MapBakeProcessor
    {
        private static readonly int BakeColorProperty = Shader.PropertyToID("_BakeColor");
        private static readonly int StepsProperty = Shader.PropertyToID("_Steps");

        public static Texture2D CaptureTile(MapBakeSettingsSO settings, int col, int row)
        {
            if (settings == null) return null;
            Vector3 tileCenter = GetTileCenterPosition(settings, col, row);
            return RenderToTexture(settings, tileCenter, settings.tileSize, (int)settings.resolution);
        }

        public static Texture2D CaptureFullMapLOD(MapBakeSettingsSO settings, int overrideResolution = 2048)
        {
            if (settings == null) return null;
            Vector3 fullCenter = GetFullMapCenterPosition(settings);
            return RenderToTexture(settings, fullCenter, settings.totalMapSize, overrideResolution);
        }

        private static Vector3 GetTileCenterPosition(MapBakeSettingsSO settings, int col, int row)
        {
            float startX = settings.centerPosition.x - settings.totalMapSize.x / 2f + settings.tileSize.x / 2f;

            if (settings.projectionPlane == MapProjectionPlane.XY)
            {
                float startY = settings.centerPosition.y - settings.totalMapSize.y / 2f + settings.tileSize.y / 2f;
                float camZ = settings.centerPosition.z - settings.captureOffset;
                return new Vector3(startX + col * settings.tileSize.x, startY + row * settings.tileSize.y, camZ);
            }
            else
            {
                float startZ = settings.centerPosition.z - settings.totalMapSize.y / 2f + settings.tileSize.y / 2f;
                float camY = settings.centerPosition.y + settings.captureOffset;
                return new Vector3(startX + col * settings.tileSize.x, camY, startZ + row * settings.tileSize.y);
            }
        }

        private static Vector3 GetFullMapCenterPosition(MapBakeSettingsSO settings)
        {
            if (settings.projectionPlane == MapProjectionPlane.XY)
            {
                return new Vector3(settings.centerPosition.x, settings.centerPosition.y, settings.centerPosition.z - settings.captureOffset);
            }
            return new Vector3(settings.centerPosition.x, settings.centerPosition.y + settings.captureOffset, settings.centerPosition.z);
        }

        private static Texture2D RenderToTexture(MapBakeSettingsSO settings, Vector3 camPos, Vector2 size, int targetResolution)
        {
            Shader depthShader = Shader.Find("Map/DepthStepShader");
            if (depthShader == null) return null;

            Material bakeMaterial = new Material(depthShader);

            GameObject camObj = new GameObject("Temp_MapCamera");
            camObj.transform.position = camPos;
            camObj.transform.rotation = settings.projectionPlane == MapProjectionPlane.XY
                ? Quaternion.Euler(0f, 0f, 0f)
                : Quaternion.Euler(90f, 0f, 0f);

            Camera cam = camObj.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = size.y * 0.5f;
            cam.nearClipPlane = 0.1f;
            cam.farClipPlane = settings.maxDepth;
            cam.cullingMask = settings.renderMask;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = settings.backgroundColor;

            Renderer[] sceneRenderers = Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None);
            Terrain[] sceneTerrains = Object.FindObjectsByType<Terrain>(FindObjectsSortMode.None);

            Dictionary<Renderer, Material[]> origRenderers = new Dictionary<Renderer, Material[]>();
            Dictionary<Terrain, Material> origTerrains = new Dictionary<Terrain, Material>();

            MaterialPropertyBlock mpb = new MaterialPropertyBlock();
            Dictionary<string, Color> colorDict = ConvertToDictionary(settings.layerColors);

            foreach (Renderer r in sceneRenderers)
            {
                if (r == null || !r.enabled || r.gameObject == null) continue;
                if (((1 << r.gameObject.layer) & settings.renderMask) == 0) continue;

                Material[] sharedMats = r.sharedMaterials;
                if (sharedMats == null || sharedMats.Length == 0) continue;

                origRenderers[r] = sharedMats;
                Material[] tempMats = new Material[sharedMats.Length];
                for (int i = 0; i < tempMats.Length; i++) tempMats[i] = bakeMaterial;
                r.sharedMaterials = tempMats;

                Color targetColor = Color.white;
                if (settings.useLayerColor)
                {
                    string lName = LayerMask.LayerToName(r.gameObject.layer);
                    if (!string.IsNullOrEmpty(lName) && colorDict.TryGetValue(lName, out Color mappedColor))
                    {
                        targetColor = mappedColor;
                    }
                }

                mpb.SetColor(BakeColorProperty, targetColor);
                mpb.SetFloat(StepsProperty, (int)settings.depthSteps);
                r.SetPropertyBlock(mpb);
            }

            foreach (Terrain t in sceneTerrains)
            {
                if (t == null || !t.enabled || t.gameObject == null) continue;
                if (((1 << t.gameObject.layer) & settings.renderMask) == 0) continue;

                origTerrains[t] = t.materialTemplate;
                t.materialTemplate = bakeMaterial;

                Color targetColor = Color.white;
                if (settings.useLayerColor)
                {
                    string lName = LayerMask.LayerToName(t.gameObject.layer);
                    if (!string.IsNullOrEmpty(lName) && colorDict.TryGetValue(lName, out Color mappedColor))
                    {
                        targetColor = mappedColor;
                    }
                }

                mpb.SetColor(BakeColorProperty, targetColor);
                mpb.SetFloat(StepsProperty, (int)settings.depthSteps);
                t.SetSplatMaterialPropertyBlock(mpb);
            }

            int resX = targetResolution;
            int resY = Mathf.RoundToInt(targetResolution * (size.y / size.x));

            RenderTexture rt = new RenderTexture(resX, resY, 24, RenderTextureFormat.ARGB32);
            cam.targetTexture = rt;
            cam.Render();

            RenderTexture.active = rt;
            Texture2D texture = new Texture2D(resX, resY, TextureFormat.RGBA32, false);
            texture.ReadPixels(new Rect(0, 0, resX, resY), 0, 0);
            texture.Apply();

            cam.targetTexture = null;
            RenderTexture.active = null;

            foreach (var pair in origRenderers)
            {
                if (pair.Key != null) { pair.Key.sharedMaterials = pair.Value; pair.Key.SetPropertyBlock(null); }
            }
            foreach (var pair in origTerrains)
            {
                if (pair.Key != null) { pair.Key.materialTemplate = pair.Value; pair.Key.SetSplatMaterialPropertyBlock(new MaterialPropertyBlock()); }
            }

            if (Application.isPlaying) { Object.Destroy(camObj); Object.Destroy(rt); Object.Destroy(bakeMaterial); }
            else { Object.DestroyImmediate(camObj); Object.DestroyImmediate(rt); Object.DestroyImmediate(bakeMaterial); }

            return texture;
        }

        private static Dictionary<string, Color> ConvertToDictionary(List<LayerColorPair> list)
        {
            var dict = new Dictionary<string, Color>();
            if (list == null) return dict;
            foreach (var pair in list)
            {
                if (!string.IsNullOrEmpty(pair.layerName) && !dict.ContainsKey(pair.layerName))
                {
                    dict.Add(pair.layerName, pair.color);
                }
            }
            return dict;
        }
    }
}