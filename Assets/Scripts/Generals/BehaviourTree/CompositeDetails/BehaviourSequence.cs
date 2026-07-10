using System.Collections;

// 등록된 행동들을 순차적으로 실행, 실행시킨 하나의 행동이 실패할 때까지 등록된 행동들을 실행
// 순차플로우와 동일, 실패하면 종료되는 점이 같음
public class BehaviourSequence : BaseBehaviourComposite
{
    public override IEnumerator OnBehaviorStarted()
    {
        // 기본적으로 성공 상태에서 실행되도록 함
        isSucceeded = true;

        // 등록된 행동들을 생성하고 하나씩 순차적으로 실행
        foreach(var getTask in runnables)
        {
            //  행동 객체를 생성
            childBehaviour = getTask();

            // 행동을 초기화
            if (childBehaviour.OnBehaviourInitialize(behaviourController))
            {
                // 행동 실행
                yield return childBehaviour.OnBehaviorStarted();

                // 성공했으면 다음으로 이동
                if (childBehaviour.isSucceeded) continue;
            }

            // 초기화 또는 행동 실패시
            isSucceeded = false;
            yield break; // 이 composite노드 실행을 중단
        }
    }


}
