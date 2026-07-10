using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class BSense_Sight : BaseSense
{
    // 시야각
    public float sightMaxAngle;

    // 시야거리
    public float sightRadius;

    // 시야 감지 최소 높이
    public float detectHeight;

    // 감지 상태 비활성화까지 걸리는 시간
    public float maxDetectionTime;

    // 감지할 오브젝트 레이어
    public LayerMask detectLayer;

    public int numberOfRay => (int)(sightMaxAngle * 0.1);

    // 시각 활성화 여부
    public bool isSightSenseEnabled {  get; private set; }

    // 감지된 객체와 감지된 시간
    public Dictionary<GameObject/*detected Object*/, float/*detected Time*/> detectedTargets = new();

    public event System.Action<GameObject> onTargetDetected;
    public event System.Action<GameObject> onTargetLost;

    private RaycastDebugInfo[] debugSightRayInfos;

    public override void OnSenseUpdated()
    {
        base.OnSenseUpdated();

        // 감지 상태 갱신
        UpdateDetectState();

        // 근처의 목표물 존재 여부 확인
        isSightSenseEnabled = IsDetedted();
        if(isSightSenseEnabled)
        {
            // 감지 내용 갱신
            UpdateSightSense();
        }
    }

    // 근처의 목표물 존재 여부 확인
    private bool IsDetedted()
    {
        Vector3 origin = behaviourController.transform.position;

        // 시야 거리 내부에 존재하는 오브젝트를 모두 확인
        // 중심위치로부터 원을만들어 충돌체를 반환하는 메서드
        Collider[] insideSightRadius = 
            Physics.OverlapSphere(origin, sightRadius, detectLayer, QueryTriggerInteraction.Ignore); 
        
        return insideSightRadius.Length > 0;
    }

    private void UpdateSightSense()
    {
        int rayCount = numberOfRay + 1;

        Vector3 origin = behaviourController.transform.position + Vector3.up * detectHeight;

        // 적 캐릭터의 앞 방향
        Vector3 forwardDirection = behaviourController.transform.forward;

        // yawAngle은 zx 평면에서의 각
        // zx평면에서는 기존 y축에 x, 기존 x축에 z가 대응됨
        float forwardYawAngle = Mathf.Atan2(forwardDirection.x, forwardDirection.z) *Mathf.Rad2Deg;

        // 레이 발사 시작 각도(우측끝부터 시작)
        float startYawAngle = forwardYawAngle - (sightMaxAngle * 0.5f);

        if (debugSightRayInfos is null) debugSightRayInfos = new RaycastDebugInfo[rayCount];

        for (int i = 0; i < rayCount; i++)
        {
            float angle = (startYawAngle + i * numberOfRay) * Mathf.Deg2Rad;

            Vector3 direction = new(
                Mathf.Sin(angle), // zx평면에서 기존 y축(sin)에 x
                forwardDirection.y,
                Mathf.Cos(angle)); // zx평면에서 기존 x축(cos)에 z


            Ray ray = new(origin, direction);
            bool detected = PhysicsExtensions.Raycast(
                out debugSightRayInfos[i],
                ray,
                out RaycastHit hitInfo,
                sightRadius,
                Physics.AllLayers,
                QueryTriggerInteraction.Ignore
                );

            if(detected)
            {
                //Ray에 감지된 게임 오브젝트
                GameObject detectedGameObject = hitInfo.transform.gameObject;

                // GameObject.Layer는 비트마스크가 아닌 레이어번호를 반환함
                // 1 << detectedGameObject.layer로 비트마스크를 얻을 수 있음
                int detectedObjectLayerMask = 1 << detectedGameObject.layer;

                if((detectLayer.value & detectedObjectLayerMask) != 0) // 0 이면 다른 레이어
                {
                    // 감지 상태로 설정
                    DetectTarget(detectedGameObject);
                }
            }

            if (debugSightRayInfos[i] is null)
            {

            }
        }
    }

    private void DetectTarget(GameObject target)
    {
        // 이전에 이미 감지를 한 경우
        if (detectedTargets.ContainsKey(target))
        {
            // 마지막으로 감지된 시간을 갱신
            detectedTargets[target] = Time.time;
        }

        // 처음 가지된 경우
        else
        {
            detectedTargets.Add(target, Time.time);
        }

        onTargetDetected?.Invoke(target);
    }

    // 감지 상태 갱신
    private void UpdateDetectState()
    {
        float currentTime = Time.time;

        // 제거 대상
        List<GameObject> lostTargetList = new();

        foreach(KeyValuePair<GameObject,float> target in detectedTargets)
        {
            // 감지된 시간을 얻음
            float detectedTime = target.Value;

            if(detectedTime + maxDetectionTime <= currentTime)
            {
                lostTargetList.Add(target.Key);
            }
        }

        foreach(GameObject lostTarget in lostTargetList)
        {
            detectedTargets.Remove(lostTarget);
            onTargetLost?.Invoke(lostTarget);
        }
    }

#if UNITY_EDITOR
    public override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();

        Vector3 origin = behaviourController.transform.position;
        Vector3 forward = behaviourController.transform.forward;

        Color drawColor = detectedTargets.Count > 0 ? Color.red : Color.green;
        //drawColor.a = 0.4f;

        //Gizmos.color = drawColor;
        //Gizmos.DrawWireSphere(origin, sightRadius);

        origin += Vector3.up * detectHeight;
        Handles.color = drawColor;
        Gizmos.color = drawColor;

        // 어떤 객체를 감지한 경우 그리도록 설정
        if(isSightSenseEnabled)
        {
            // 시야 범위 그리기
            Handles.DrawWireDisc(origin, Vector3.up, sightRadius);
    
            // 시야각 그리기
            Handles.DrawSolidArc(origin, Vector3.up, forward, sightMaxAngle * 0.5f, sightRadius);
            Handles.DrawSolidArc(origin, Vector3.down, forward, sightMaxAngle * 0.5f, sightRadius);
        }

        // 감지중인 객체가 존재한다면
        if (detectedTargets.Count > 0)
        {
            foreach (var target in detectedTargets)
            {
                Vector3 targetPosition = target.Key.transform.position + Vector3.up * detectHeight;
                Gizmos.DrawWireSphere(targetPosition, 1.0f);
                Gizmos.DrawLine(origin, targetPosition);
            }
        }

        if (debugSightRayInfos is not null)
        {
            foreach (RaycastDebugInfo debugInfo in debugSightRayInfos)
            {
                debugInfo?.Draw();
            }
        }
    }
#endif
}
