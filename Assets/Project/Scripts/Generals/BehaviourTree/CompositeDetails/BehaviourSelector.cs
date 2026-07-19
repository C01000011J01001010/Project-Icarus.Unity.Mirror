using System.Collections;

// 등록된 행동들을 순차적으로 실행하며, 실행시킨 행동이 성공할 때 까지 등록된 행동을 실행
// if else문처럼 초기화와 행동을 하고 실패하면 다음으로 넘어감, 하지만 성공하면 거기서 멈춤
public class BehaviourSelector : BaseBehaviourComposite
{
    public override IEnumerator OnBehaviorStarted()
    {
        // 기본적으로 실패 상태에서 실행되도록 함
        isSucceeded = false;

        // 등록된 행동들을 생성하고 하나씩 순차적으로 실행
        foreach (var getTask in runnables)
        {
            //  행동 객체를 생성
            childBehaviour = getTask();

            // 행동을 초기화
            if (isSucceeded = childBehaviour.OnBehaviourInitialize(behaviourController))
            {
                // 행동 실행
                yield return childBehaviour.OnBehaviorStarted();

                // 실행한 행동의 결과를 확인하고 성공하면 여기서 종료
                if (isSucceeded = childBehaviour.isSucceeded) yield break;
            }
            // 초기화 또는 행동을 실패한 경우
            yield return null;
        }
    }
}
