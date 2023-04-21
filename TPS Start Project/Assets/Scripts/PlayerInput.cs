using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    // Project Settings의 Input에 할당된 이름과 매치되어야 한다.
    public string fireButtonName = "Fire1";
    public string jumpButtonName = "Jump";
    public string moveHorizontalAxisName = "Horizontal";
    public string moveVerticalAxisName = "Vertical";
    public string reloadButtonName = "Reload";

    public Vector2 moveInput { get; private set; }
    public bool fire { get; private set; }
    public bool reload { get; private set; }
    public bool jump { get; private set; }
    
    private void Update()
    {
        if (GameManager.Instance != null
            && GameManager.Instance.isGameover)
        {
            moveInput = Vector2.zero;
            fire = false;
            reload = false;
            jump = false;
            return;
        }

        // 입력 벡터 계산
        // 특정 키 입력 판단(true/false)이 아니라 GetAxis() 로 입력량을 얻으면 키보드 입력 뿐만 아니라
        // 패드나 스틱의 입력도 처리할 수 있다.
        moveInput = new Vector2(Input.GetAxis(moveHorizontalAxisName), Input.GetAxis(moveVerticalAxisName));
       
        // 키보드입력의 경우 대각선 입력(이동)시에 실제 이동거리가 커지는 현상이 있는데 이를 방지하기위해
        // 정규화하여 moveInput은 방향으로만 사용하고 이후에 speed를 곱해서 사용
        if (moveInput.sqrMagnitude > 1) moveInput = moveInput.normalized;

        // Magnitude 대신 sqrMagnitude를 사용하는 이유?
        // -> 1^2 = 1  |  1.1^2 > 1
        // -> 0.9^2 < 1
        // Magnitude 값이 1보다 크면 제곱해도 반드시 1보다 큼(1보다 작으면 제곱해도 1보다 작음)
        // but sqrMagnitude 는 제곱근(sqrt)연산을 하지 않으므로 연산면에서 효율적!

        jump = Input.GetButtonDown(jumpButtonName);
        fire = Input.GetButton(fireButtonName);
        reload = Input.GetButtonDown(reloadButtonName);
    }
}