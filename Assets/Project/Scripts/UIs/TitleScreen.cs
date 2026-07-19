using System.Collections;
using UnityEngine;

public class TitleScreen : BaseUi, IScenedUi
{

    [Header("Movement Settings")]
    public float moveRange = 100f;  // 움직일 범위 (픽셀 단위)
    public float moveSpeed = 30f;  // 이동 속도

    private Coroutine titleAnimation;

    private void OnDisable()
    {
        if (titleAnimation != null)
        {
            StopCoroutine(titleAnimation);
            titleAnimation = null;
        }
    }

    private void OnEnable()
    {
        if (titleAnimation != null) StopCoroutine(titleAnimation);
        titleAnimation = StartCoroutine(FloatingMovement());
    }

    public override void Exit()
    {
        if(titleAnimation != null)
        {
            StopCoroutine(titleAnimation);
            titleAnimation = null;
        }
    }

    public override IEnumerator Initialize()
    {
        // 초기화와 별개로 유지
        if (titleAnimation != null) StopCoroutine(titleAnimation);
        titleAnimation = StartCoroutine(FloatingMovement());
        Debug.Log("TitleScreen 실행");
        yield return null;
    }
    

    IEnumerator FloatingMovement()
    {
        RectTransform rectTransform = GetComponent<RectTransform>();
        Vector2 startPos = rectTransform.anchoredPosition;

        // 랜덤한 초기 방향 설정
        Vector2 direction = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;

        while (true)
        {
            // 현재 위치 계산
            Vector2 nextPos = rectTransform.anchoredPosition + (direction * moveSpeed * Time.deltaTime);

            // 시작 위치로부터 moveRange를 벗어나는지 체크 (좌우/상하)
            if (Mathf.Abs(nextPos.x - startPos.x) > moveRange)
            {
                direction = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
            }
            if (Mathf.Abs(nextPos.y - startPos.y) > moveRange)
            {
                direction = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
            }

            rectTransform.anchoredPosition += direction * moveSpeed * Time.deltaTime;
            yield return null;
        }
    }
}
