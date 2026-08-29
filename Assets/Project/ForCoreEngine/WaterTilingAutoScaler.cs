using UnityEngine;

namespace CoreEngine.LevelDesign
{
    [ExecuteAlways] // 에디터 씬 뷰에서도 실시간 작동[cite: 1]
    [RequireComponent(typeof(Renderer))]
    public class WaterTilingAutoScaler : MonoBehaviour
    {
        [Header("Auto Tiling Settings")]
        [Tooltip("체크하면 오브젝트의 X, Z 스케일에 비례하여 Normal Tiling이 자동 조절됩니다.")]
        [SerializeField] private bool _syncTilingWithScale = true;

        [Tooltip("스케일이 (1, 1, 1)일 때의 기본 Tiling 값입니다.")]
        [SerializeField] private Vector2 _baseTiling = new Vector2(10f, 10f);

        // 셰이더 그래프 내 Normal Tiling 프로퍼티의 실제 Reference ID
        private static readonly int NormalTilingProperty = Shader.PropertyToID("Vector2_4351ac2be1d74054986ec5378db9d578");

        private Renderer _renderer;
        private MaterialPropertyBlock _mpb;

        private void OnEnable()
        {
            _renderer = GetComponent<Renderer>();
            _mpb = new MaterialPropertyBlock();
            UpdateTiling();
        }

        private void Update()
        {
            // 씬 뷰에서 오브젝트의 스케일을 조작할 때 실시간 동기화[cite: 16]
            if (!Application.isPlaying && transform.hasChanged)
            {
                if (_syncTilingWithScale)
                {
                    UpdateTiling();
                }

                // 플래그 초기화 (무한루프 방지)[cite: 16]
                transform.hasChanged = false;
            }
        }

        private void OnValidate()
        {
            // 인스펙터에서 체크박스를 켜고 끄거나, Base Tiling 수치를 변경했을 때 즉각 반영
            if (_syncTilingWithScale)
            {
                UpdateTiling();
            }
        }

        private void UpdateTiling()
        {
            if (_renderer == null) return;

            // X축과 Z축의 스케일 비율을 기본 Tiling 값에 곱하여 계산
            Vector2 currentScale = new Vector2(transform.localScale.x, transform.localScale.z);
            Vector2 targetTiling = new Vector2(_baseTiling.x * currentScale.x, _baseTiling.y * currentScale.y);

            // 에디터에서 원본 머티리얼 에셋을 오염시키지 않기 위해 MaterialPropertyBlock 사용
            _renderer.GetPropertyBlock(_mpb);
            _mpb.SetVector(NormalTilingProperty, targetTiling);
            _renderer.SetPropertyBlock(_mpb);
        }
    }
}
