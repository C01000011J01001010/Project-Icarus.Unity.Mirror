using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace CoreEngine.Manager
{
    public class ResourceManager : BaseManager
    {
        // 1. 공용 UI 저장소 (게임 종료 시까지 유지)
        private Dictionary<string, GameObject> _globalUIPrefabs = new();
        private List<AsyncOperationHandle<GameObject>> _globalUIHandles = new();

        // 2. 씬 전용 UI 저장소 (씬 전환 시 비워짐)
        private Dictionary<string, GameObject> _sceneUIPrefabs = new();
        private List<AsyncOperationHandle<GameObject>> _sceneUIHandles = new();

        public override IEnumerator Initialize()
        {
            // 리소스 매니저는 태어날 때 특별히 로드할 게 없다면 패스합니다.
            // 실제 로드는 UIManager나 SceneController가 요청할 때 수행합니다.
            yield break;
        }

        public override void Exit()
        {
            // BaseModule의 Exit 오버라이드: 모든 핸들 해제
            ReleaseSceneAssets();
            ReleaseAssets(_globalUIHandles, _globalUIPrefabs);
        }

        // ----------------------------------------------------------------------------------
        // [로드 로직]
        // ----------------------------------------------------------------------------------
        public IEnumerator LoadGlobalUIs(List<string> addresses)
        {
            yield return LoadAndCacheAssets(addresses, _globalUIPrefabs, _globalUIHandles);
        }

        public IEnumerator LoadSceneUIs(List<string> addresses)
        {
            yield return LoadAndCacheAssets(addresses, _sceneUIPrefabs, _sceneUIHandles);
        }

        private IEnumerator LoadAndCacheAssets(List<string> addresses, Dictionary<string, GameObject> dict, List<AsyncOperationHandle<GameObject>> handles)
        {
            foreach (string addr in addresses)
            {
                if (dict.ContainsKey(addr)) continue;

                var handle = Addressables.LoadAssetAsync<GameObject>(addr);
                yield return handle;

                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    dict.TryAdd(addr, handle.Result);
                    handles.Add(handle);
                }
                else
                {
                    Debug.LogError($"[ResourceManager] 에셋 로드 실패: {addr}");
                }
            }
        }

        // ----------------------------------------------------------------------------------
        // [사용 및 해제 로직]
        // ----------------------------------------------------------------------------------
        public GameObject GetGlobalUiPrefab(string address) => GetCachedPrefab(address, _globalUIPrefabs);
        public GameObject GetSceneUI(string address) => GetCachedPrefab(address, _sceneUIPrefabs);

        private GameObject GetCachedPrefab(string address, Dictionary<string, GameObject> dict)
        {
            if (dict.TryGetValue(address, out GameObject prefab)) return prefab;
            Debug.LogWarning($"[ResourceManager] 캐싱되지 않은 에셋 요청: {address}");
            return null;
        }

        public void ReleaseSceneAssets()
        {
            ReleaseAssets(_sceneUIHandles, _sceneUIPrefabs);
            Debug.Log("[ResourceManager] 씬 전용 에셋 메모리 해제 완료");
        }

        private void ReleaseAssets(List<AsyncOperationHandle<GameObject>> handles, Dictionary<string, GameObject> dict)
        {
            foreach (var handle in handles)
            {
                if (handle.IsValid()) Addressables.Release(handle);
            }
            handles.Clear();
            dict.Clear();
        }
    }
}