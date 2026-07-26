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
            Shader outlineCompositeShader = Shader.Find("Map/OutlineComposite");

            if (depthShader == null)
            {
                Debug.LogError("[MapBakeProcessor] 'Map/DepthStepShader' 셰이더를 찾을 수 없습니다.");
                return null;
            }

            Material bakeMaterial = new Material(depthShader);
            Material outlineMaterial = outlineCompositeShader != null ? new Material(outlineCompositeShader) : null;

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
            cam.clearFlags = CameraClearFlags.SolidColor;

            Renderer[] sceneRenderers = Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None);
            Terrain[] sceneTerrains = Object.FindObjectsByType<Terrain>(FindObjectsSortMode.None);

            Dictionary<Renderer, Material[]> origRenderers = new Dictionary<Renderer, Material[]>();
            Dictionary<Terrain, Material> origTerrains = new Dictionary<Terrain, Material>();
            Dictionary<string, Color> colorDict = ConvertToDictionary(settings.layerColors);
            MaterialPropertyBlock mpb = new MaterialPropertyBlock();

            int resX = targetResolution;
            int resY = Mathf.RoundToInt(targetResolution * (size.y / size.x));

            // =========================================================
            // [Pass 1] Base Map (원본 지도) 렌더링
            // =========================================================
            cam.cullingMask = settings.renderMask;
            cam.backgroundColor = settings.backgroundColor;

            foreach (Renderer r in sceneRenderers)
            {
                if (r == null || !r.enabled || r.gameObject == null) continue;
                if (((1 << r.gameObject.layer) & settings.renderMask) == 0) continue;

                origRenderers[r] = r.sharedMaterials;
                Material[] tempMats = new Material[r.sharedMaterials.Length];
                for (int i = 0; i < tempMats.Length; i++) tempMats[i] = bakeMaterial;
                r.sharedMaterials = tempMats;

                Color targetColor = Color.white;
                if (settings.useLayerColor && colorDict.TryGetValue(LayerMask.LayerToName(r.gameObject.layer), out Color mappedColor))
                    targetColor = mappedColor;

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
                if (settings.useLayerColor && colorDict.TryGetValue(LayerMask.LayerToName(t.gameObject.layer), out Color mappedColor))
                    targetColor = mappedColor;

                mpb.SetColor(BakeColorProperty, targetColor);
                mpb.SetFloat(StepsProperty, (int)settings.depthSteps);
                t.SetSplatMaterialPropertyBlock(mpb);
            }

            RenderTexture rtBase = RenderTexture.GetTemporary(resX, resY, 24, RenderTextureFormat.ARGB32);
            cam.targetTexture = rtBase;
            cam.Render();

            // =========================================================
            // [Pass 2] 외곽선 설정 순회 및 Multi-Pass 렌더링 
            // =========================================================
            RenderTexture rtCurrent = rtBase;

            if (settings.outlineSettings != null && settings.outlineSettings.Count > 0 && outlineMaterial != null)
            {
                RenderTexture rtMask = RenderTexture.GetTemporary(resX, resY, 24, RenderTextureFormat.ARGB32);
                RenderTexture rtCover = RenderTexture.GetTemporary(resX, resY, 24, RenderTextureFormat.ARGB32); // 🌟 커버용 텍스처 추가

                Material maskTargetMat = new Material(depthShader);
                maskTargetMat.SetColor(BakeColorProperty, Color.white);
                maskTargetMat.SetFloat(StepsProperty, 0f);

                Material maskOccludeMat = new Material(depthShader);
                maskOccludeMat.SetColor(BakeColorProperty, Color.clear);
                maskOccludeMat.SetFloat(StepsProperty, 0f);

                foreach (var outline in settings.outlineSettings)
                {
                    if (string.IsNullOrEmpty(outline.layerName)) continue;
                    int targetLayer = LayerMask.NameToLayer(outline.layerName);
                    if (targetLayer == -1) continue;

                    // --------------------------------------------------
                    // 1. 실루엣 마스크 렌더링 (Target + Occluder)
                    // --------------------------------------------------
                    cam.cullingMask = (1 << targetLayer) | outline.occluderMask;
                    cam.backgroundColor = Color.clear;
                    cam.targetTexture = rtMask;

                    RenderTexture.active = rtMask; GL.Clear(true, true, Color.clear); RenderTexture.active = null;

                    foreach (Renderer r in sceneRenderers)
                    {
                        if (origRenderers.ContainsKey(r) && ((1 << r.gameObject.layer) & cam.cullingMask) != 0)
                        {
                            bool isTarget = r.gameObject.layer == targetLayer;
                            Material[] newMats = new Material[origRenderers[r].Length];
                            for (int i = 0; i < newMats.Length; i++) newMats[i] = isTarget ? maskTargetMat : maskOccludeMat;
                            r.sharedMaterials = newMats;
                            r.SetPropertyBlock(null);
                        }
                    }
                    foreach (Terrain t in sceneTerrains)
                    {
                        if (origTerrains.ContainsKey(t) && ((1 << t.gameObject.layer) & cam.cullingMask) != 0)
                        {
                            bool isTarget = t.gameObject.layer == targetLayer;
                            t.materialTemplate = isTarget ? maskTargetMat : maskOccludeMat;
                            t.SetSplatMaterialPropertyBlock(new MaterialPropertyBlock());
                        }
                    }
                    cam.Render();

                    // --------------------------------------------------
                    // 2. 커버(Cover) 마스크 렌더링 (앞에 있는 방해물들)
                    // --------------------------------------------------
                    cam.cullingMask = outline.coverMask;
                    cam.targetTexture = rtCover;
                    RenderTexture.active = rtCover; GL.Clear(true, true, Color.clear); RenderTexture.active = null;

                    if (outline.coverMask != 0)
                    {
                        foreach (Renderer r in sceneRenderers)
                        {
                            if (origRenderers.ContainsKey(r) && ((1 << r.gameObject.layer) & cam.cullingMask) != 0)
                            {
                                Material[] newMats = new Material[origRenderers[r].Length];
                                for (int i = 0; i < newMats.Length; i++) newMats[i] = maskTargetMat; // 커버 물체는 모두 하얀색으로
                                r.sharedMaterials = newMats;
                            }
                        }
                        foreach (Terrain t in sceneTerrains)
                        {
                            if (origTerrains.ContainsKey(t) && ((1 << t.gameObject.layer) & cam.cullingMask) != 0)
                            {
                                t.materialTemplate = maskTargetMat;
                            }
                        }
                        cam.Render();
                    }

                    // --------------------------------------------------
                    // 3. 합성 (Blit)
                    // --------------------------------------------------
                    RenderTexture rtNext = RenderTexture.GetTemporary(resX, resY, 0, RenderTextureFormat.ARGB32);
                    outlineMaterial.SetTexture("_MaskTex", rtMask);
                    outlineMaterial.SetTexture("_CoverTex", rtCover); // 🌟 커버 텍스처 전달
                    outlineMaterial.SetColor("_OutlineColor", outline.outlineColor);
                    outlineMaterial.SetFloat("_OutlineThickness", outline.outlineThickness);
                    outlineMaterial.SetVector("_PixelSize", new Vector4(1f / resX, 1f / resY, resX, resY));

                    Graphics.Blit(rtCurrent, rtNext, outlineMaterial);

                    if (rtCurrent != rtBase) RenderTexture.ReleaseTemporary(rtCurrent);
                    rtCurrent = rtNext;
                }

                RenderTexture.ReleaseTemporary(rtMask);
                RenderTexture.ReleaseTemporary(rtCover); // 🌟 해제 추가

                if (Application.isPlaying) { Object.Destroy(maskTargetMat); Object.Destroy(maskOccludeMat); }
                else { Object.DestroyImmediate(maskTargetMat); Object.DestroyImmediate(maskOccludeMat); }
            }

            else if (outlineMaterial == null && settings.outlineSettings != null && settings.outlineSettings.Count > 0)
            {
                Debug.LogWarning("[MapBakeProcessor] 'Map/OutlineComposite' 셰이더가 없어 외곽선을 그릴 수 없습니다.");
            }

            // =========================================================
            // 텍스처 추출 및 메모리 정리
            // =========================================================
            RenderTexture.active = rtCurrent;
            Texture2D texture = new Texture2D(resX, resY, TextureFormat.RGBA32, false);
            texture.ReadPixels(new Rect(0, 0, resX, resY), 0, 0);
            texture.Apply();

            cam.targetTexture = null;
            RenderTexture.active = null;

            // 객체 원래 상태로 복구
            foreach (var pair in origRenderers) { pair.Key.sharedMaterials = pair.Value; pair.Key.SetPropertyBlock(null); }
            foreach (var pair in origTerrains) { pair.Key.materialTemplate = pair.Value; pair.Key.SetSplatMaterialPropertyBlock(new MaterialPropertyBlock()); }

            if (rtCurrent != rtBase) RenderTexture.ReleaseTemporary(rtCurrent);
            RenderTexture.ReleaseTemporary(rtBase);

            if (Application.isPlaying)
            {
                Object.Destroy(camObj); Object.Destroy(bakeMaterial);
                if (outlineMaterial != null) Object.Destroy(outlineMaterial);
            }
            else
            {
                Object.DestroyImmediate(camObj); Object.DestroyImmediate(bakeMaterial);
                if (outlineMaterial != null) Object.DestroyImmediate(outlineMaterial);
            }

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