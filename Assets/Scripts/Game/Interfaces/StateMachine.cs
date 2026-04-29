// 상태 전환을 관리할 인터페이스 (CustomerController 등이 상속받음)
public interface IStateMachine
{
    void ChangeState(BaseState newState);
}

// 모든 개별 행동(상태)의 뼈대가 되는 클래스입니다.
public abstract class BaseState
{
    protected IStateMachine stateMachine;

    public BaseState(IStateMachine stateMachine)
    {
        this.stateMachine = stateMachine;
    }

    // 상태에 진입할 때 1회 호출
    public abstract void Enter();

    // 매 프레임(Update)마다 호출 (이동, 시간 체크 등)
    public abstract void Tick();

    // 상태를 빠져나갈 때 1회 호출 (데이터 정리 등)
    public abstract void Exit();
}