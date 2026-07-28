using System.Collections.Generic;
using UnityEngine;

namespace CoreEngine.LevelDesign
{
    public static class MapBakeProcessor
    {
        private static readonly int BakeColorProperty = Shader.PropertyToID("_BakeColor");
        private static readonly int StepsProperty = Shader.PropertyToID("_Steps");

        #region [1] Public API (외부 노출 메서드)

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

        #endregion

        #region [2] 메인 렌더링 파이프라인 (지휘관)

        private static Texture2D RenderToTexture(MapBakeSettingsSO settings, Vector3 camPos, Vector2 size, int targetResolution)
        {
            // 1. 셰이더 및 재질 로드
            Shader depthShader = Shader.Find("Map/DepthStepShader");
            Shader outlineCompositeShader = Shader.Find("Map/OutlineComposite");

            if (depthShader == null)
            {
                Debug.LogError("[MapBakeProcessor] 'Map/DepthStepShader' 셰이더를 찾을 수 없습니다.");
                return null;
            }

            Material bakeMaterial = new Material(depthShader);
            Material depthBakeMat = new Material(depthShader);
            Material outlineMaterial = outlineCompositeShader != null ? new Material(outlineCompositeShader) : null;

            // 2. 해상도 및 카메라 세팅
            int resX = targetResolution;
            int resY = Mathf.RoundToInt(targetResolution * (size.y / size.x));
            Camera cam = SetupBakeCamera(settings, camPos, size);

            // 3. 씬 객체 캐싱 및 원래 재질 백업
            Renderer[] sceneRenderers = Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None);
            Terrain[] sceneTerrains = Object.FindObjectsByType<Terrain>(FindObjectsSortMode.None);

            CacheOriginalMaterials(sceneRenderers, sceneTerrains,
                out var origRenderers, out var origTerrains);

            Dictionary<string, Color> colorDict = ConvertToDictionary(settings.layerColors);

            // =========================================================
            // 파이프라인 실행
            // =========================================================

            // [Pass 1] 원본 Base Map 렌더링
            RenderTexture rtBase = BakeBaseMap(cam, settings, sceneRenderers, sceneTerrains, bakeMaterial, colorDict, resX, resY);

            // [Pass 2] 하이브리드 외곽선 렌더링 및 합성 (forceEdgeMask 완벽 적용)
            RenderTexture rtFinal = ApplyHybridOutlines(cam, settings, rtBase, sceneRenderers, sceneTerrains, depthBakeMat, outlineMaterial, resX, resY);

            // 최종 텍스처 추출
            Texture2D finalTexture = ExtractTexture(rtFinal, resX, resY);

            // =========================================================
            // 정리 및 원상 복구
            // =========================================================

            RestoreOriginalMaterials(origRenderers, origTerrains);
            CleanupResources(cam.gameObject, bakeMaterial, depthBakeMat, outlineMaterial, rtBase, rtFinal);

            return finalTexture;
        }

        #endregion

        #region [3] 렌더링 패스 (Pass 1, Pass 2)

        /// <summary>
        /// [Pass 1] 객체의 원래 색상 또는 레이어 색상을 기반으로 기본 지도를 렌더링합니다.
        /// </summary>
        private static RenderTexture BakeBaseMap(Camera cam, MapBakeSettingsSO settings, Renderer[] renderers, Terrain[] terrains, Material bakeMat, Dictionary<string, Color> colorDict, int resX, int resY)
        {
            cam.cullingMask = settings.renderMask;
            cam.backgroundColor = settings.backgroundColor;

            MaterialPropertyBlock mpb = new MaterialPropertyBlock();
            RenderTexture rtBase = RenderTexture.GetTemporary(resX, resY, 24, RenderTextureFormat.ARGB32);
            cam.targetTexture = rtBase;

            // *요청 반영: None이면 매터리얼 덮어쓰기 로직을 통째로 생략 (씬 원본 텍스처/매터리얼 그대로 렌더링)
            if (settings.depthSteps != MapDepthSteps.None)
            {
                // Renderer 세팅
                foreach (Renderer r in renderers)
                {
                    if (r == null || !r.enabled || ((1 << r.gameObject.layer) & settings.renderMask) == 0) continue;

                    Material[] tempMats = new Material[r.sharedMaterials.Length];
                    for (int i = 0; i < tempMats.Length; i++) tempMats[i] = bakeMat;
                    r.sharedMaterials = tempMats;

                    Color targetColor = Color.white;
                    if (settings.useLayerColor && colorDict.TryGetValue(LayerMask.LayerToName(r.gameObject.layer), out Color mappedColor))
                        targetColor = mappedColor;

                    mpb.SetColor(BakeColorProperty, targetColor);
                    mpb.SetFloat(StepsProperty, (int)settings.depthSteps);

                    // *요청 반영: 셰이더에 최종 하한선 밝기 값 전달
                    mpb.SetFloat("_FinalDepthBrightness", settings.finalDepthBrightness);
                    r.SetPropertyBlock(mpb);
                }

                // Terrain 세팅
                foreach (Terrain t in terrains)
                {
                    if (t == null || !t.enabled || ((1 << t.gameObject.layer) & settings.renderMask) == 0) continue;

                    t.materialTemplate = bakeMat;

                    Color targetColor = Color.white;
                    if (settings.useLayerColor && colorDict.TryGetValue(LayerMask.LayerToName(t.gameObject.layer), out Color mappedColor))
                        targetColor = mappedColor;

                    mpb.SetColor(BakeColorProperty, targetColor);
                    mpb.SetFloat(StepsProperty, (int)settings.depthSteps);
                    mpb.SetFloat("_FinalDepthBrightness", settings.finalDepthBrightness);
                    t.SetSplatMaterialPropertyBlock(mpb);
                }
            }

            cam.Render();
            return rtBase;
        }

        /// <summary>
        /// [Pass 2] 타겟과 가림막 깊이를 비교하고, 강제 절단(forceEdgeMask)을 적용하여 예쁜 외곽선을 생성합성합니다.
        /// </summary>
        private static RenderTexture ApplyHybridOutlines(Camera cam, MapBakeSettingsSO settings, RenderTexture rtBase, Renderer[] renderers, Terrain[] terrains, Material depthBakeMat, Material outlineMaterial, int resX, int resY)
        {
            if (settings.outlineSettings == null || settings.outlineSettings.Count == 0 || outlineMaterial == null)
            {
                return rtBase; // 외곽선 세팅이 없으면 원본 그대로 반환
            }

            RenderTexture rtCurrent = rtBase;
            RenderTexture rtGlobalDepth = RenderTexture.GetTemporary(resX, resY, 24, RenderTextureFormat.RFloat);
            RenderTexture rtTargetDepth = RenderTexture.GetTemporary(resX, resY, 24, RenderTextureFormat.RFloat);

            // 깊이 측정용 재질 (하얀색)
            depthBakeMat.SetColor(BakeColorProperty, Color.white);
            depthBakeMat.SetFloat(StepsProperty, 10000f);

            // 🌟 강제 경계선(구멍 뚫기)용 재질 (검은색) 부활!
            Material holeCutMat = new Material(depthBakeMat.shader);
            holeCutMat.SetColor(BakeColorProperty, Color.black);
            holeCutMat.SetFloat(StepsProperty, 10000f);

            foreach (var outline in settings.outlineSettings)
            {
                if (!outline.isUse || string.IsNullOrEmpty(outline.layerName)) continue;
                int targetLayer = LayerMask.NameToLayer(outline.layerName);
                if (targetLayer == -1) continue;

                // 🌟 [추가된 안전장치] RenderMask에 포함되어 있지 않은 레이어라면 무시!
                if ((settings.renderMask.value & (1 << targetLayer)) == 0) continue;

                // 🌟 1. 전체 씬 깊이(Global Depth) 맵 렌더링 (forceEdgeMask 대상은 투명인간 취급)
                cam.cullingMask = settings.renderMask & ~outline.forceEdgeMask;
                cam.backgroundColor = Color.black;
                cam.targetTexture = rtGlobalDepth;
                RenderTexture.active = rtGlobalDepth; GL.Clear(true, true, Color.black); RenderTexture.active = null;

                // 전체 객체를 기본 깊이 측정 재질로 덮어씌움
                foreach (var r in renderers) { if (r != null) { r.SetPropertyBlock(null); r.sharedMaterials = new Material[] { depthBakeMat }; } }
                foreach (var t in terrains) { if (t != null) { t.materialTemplate = depthBakeMat; t.SetSplatMaterialPropertyBlock(new MaterialPropertyBlock()); } }
                cam.Render();

                // 🌟 2. 타겟 마스크 렌더링 (핵심: 타겟 + 강제 절단 대상을 같이 렌더링하여 해안선 생성)
                cam.cullingMask = (1 << targetLayer) | outline.forceEdgeMask;
                cam.targetTexture = rtTargetDepth;
                RenderTexture.active = rtTargetDepth; GL.Clear(true, true, Color.black); RenderTexture.active = null;

                // 타겟은 하얀색 깊이(depthBakeMat), 강제 절단 레이어(물 등)는 검은색(holeCutMat)으로 덮어씌움
                foreach (var r in renderers)
                {
                    if (r != null && ((1 << r.gameObject.layer) & cam.cullingMask) != 0)
                    {
                        bool isTarget = r.gameObject.layer == targetLayer;
                        r.sharedMaterials = new Material[] { isTarget ? depthBakeMat : holeCutMat };
                    }
                }
                foreach (var t in terrains)
                {
                    if (t != null && ((1 << t.gameObject.layer) & cam.cullingMask) != 0)
                    {
                        bool isTarget = t.gameObject.layer == targetLayer;
                        t.materialTemplate = isTarget ? depthBakeMat : holeCutMat;
                    }
                }
                cam.Render();

                // 3. 외곽선 Blit 합성
                RenderTexture rtNext = RenderTexture.GetTemporary(resX, resY, 0, RenderTextureFormat.ARGB32);
                outlineMaterial.SetTexture("_MaskTex", rtTargetDepth);
                outlineMaterial.SetTexture("_GlobalDepthTex", rtGlobalDepth);
                outlineMaterial.SetColor("_OutlineColor", outline.outlineColor);
                outlineMaterial.SetFloat("_OutlineThickness", outline.outlineThickness);
                outlineMaterial.SetFloat("_DepthThreshold", outline.depthThreshold);
                outlineMaterial.SetVector("_PixelSize", new Vector4(1f / resX, 1f / resY, resX, resY));

                Graphics.Blit(rtCurrent, rtNext, outlineMaterial);

                if (rtCurrent != rtBase) RenderTexture.ReleaseTemporary(rtCurrent);
                rtCurrent = rtNext;
            }

            RenderTexture.ReleaseTemporary(rtGlobalDepth);
            RenderTexture.ReleaseTemporary(rtTargetDepth);

            // 임시 생성한 구멍 뚫기 재질 해제
            if (Application.isPlaying) Object.Destroy(holeCutMat);
            else Object.DestroyImmediate(holeCutMat);

            return rtCurrent;
        }

        #endregion

        #region [4] 상태 보존 및 메모리 제어 (백업, 추출, 클린업)

        private static void CacheOriginalMaterials(Renderer[] renderers, Terrain[] terrains, out Dictionary<Renderer, Material[]> origRenderers, out Dictionary<Terrain, Material> origTerrains)
        {
            origRenderers = new Dictionary<Renderer, Material[]>();
            origTerrains = new Dictionary<Terrain, Material>();

            foreach (Renderer r in renderers)
            {
                if (r != null && r.sharedMaterials != null && r.sharedMaterials.Length > 0)
                    origRenderers[r] = r.sharedMaterials;
            }
            foreach (Terrain t in terrains)
            {
                if (t != null)
                    origTerrains[t] = t.materialTemplate;
            }
        }

        private static void RestoreOriginalMaterials(Dictionary<Renderer, Material[]> origRenderers, Dictionary<Terrain, Material> origTerrains)
        {
            foreach (var pair in origRenderers)
            {
                if (pair.Key != null)
                {
                    pair.Key.sharedMaterials = pair.Value;
                    pair.Key.SetPropertyBlock(null);
                }
            }
            foreach (var pair in origTerrains)
            {
                if (pair.Key != null)
                {
                    pair.Key.materialTemplate = pair.Value;
                    pair.Key.SetSplatMaterialPropertyBlock(new MaterialPropertyBlock());
                }
            }
        }

        private static Texture2D ExtractTexture(RenderTexture rtFinal, int resX, int resY)
        {
            RenderTexture.active = rtFinal;
            Texture2D texture = new Texture2D(resX, resY, TextureFormat.RGBA32, false);
            texture.ReadPixels(new Rect(0, 0, resX, resY), 0, 0);
            texture.Apply();
            RenderTexture.active = null;
            return texture;
        }

        private static void CleanupResources(GameObject camObj, Material bakeMat, Material depthMat, Material outlineMat, RenderTexture rtBase, RenderTexture rtFinal)
        {
            if (rtBase != null) RenderTexture.ReleaseTemporary(rtBase);
            if (rtFinal != null && rtFinal != rtBase) RenderTexture.ReleaseTemporary(rtFinal);

            if (Application.isPlaying)
            {
                Object.Destroy(camObj);
                Object.Destroy(bakeMat);
                Object.Destroy(depthMat);
                if (outlineMat != null) Object.Destroy(outlineMat);
            }
            else
            {
                Object.DestroyImmediate(camObj);
                Object.DestroyImmediate(bakeMat);
                Object.DestroyImmediate(depthMat);
                if (outlineMat != null) Object.DestroyImmediate(outlineMat);
            }
        }

        #endregion

        #region [5] 위치 및 유틸리티 헬퍼 (Helper)

        private static Camera SetupBakeCamera(MapBakeSettingsSO settings, Vector3 camPos, Vector2 size)
        {
            GameObject camObj = new GameObject("Temp_MapBakeCamera");
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
            return cam;
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

        #endregion
    }
}