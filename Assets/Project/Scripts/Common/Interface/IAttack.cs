public interface IAttack
{
    // AttackCode
    public int Code { get;}

    public bool IsAttackAreaChecking { get;}

    // IsAttackAreaChecking가 true이면 매 프레임 호출
    public void CheckAttackArea();

    // IsAttackAreaChecking 관리
    public void OnAttackAreaCheckStarted();

    public void OnAttackAreaCheckFinished();


    // 기즈모 그리는 함수
    public void OnDrawGizmosSelected();
}