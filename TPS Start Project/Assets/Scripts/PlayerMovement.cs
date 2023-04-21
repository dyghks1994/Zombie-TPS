using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private CharacterController characterController;
    private PlayerInput playerInput;
    private PlayerShooter playerShooter;
    private Animator animator;
    
    private Camera followCam;
    
    public float speed = 6f;
    public float jumpVelocity = 20f;
    [Range(0.01f, 1f)] public float airControlPercent;  // 공중에 있을 때 보정할 속도 비율

    public float speedSmoothTime = 0.1f;
    public float turnSmoothTime = 0.1f;
    
    private float speedSmoothVelocity;
    private float turnSmoothVelocity;
    
    // Character Controller 는 rigidbody와 달리 중력에 영향을 받지 않고 속도를 직접 제어
    // y축도 마찬가지기 때문에 점프시에 적용할 y축 이동 속도를 제어할 변수를 생성
    private float currentVelocityY;
    
    public float currentSpeed =>
        new Vector2(characterController.velocity.x, characterController.velocity.z).magnitude;
    
    private void Start()
    {
        // 사용할 컴포넌트들 연결
        playerInput = GetComponent<PlayerInput>();
        playerShooter = GetComponent<PlayerShooter>();
        animator = GetComponent<Animator>();
        characterController = GetComponent<CharacterController>();
        followCam = Camera.main;
    }

    // 물리 갱신 주기에 맞춰 회전과 이동을 한다
    private void FixedUpdate()
    {
        // 이동 중일 때에만 플레이어 회전
        // TPS 게임에선 카메라의 방향과 캐릭터의 방향이 일치하지 않을 수 있기 때문에.
        // (유저는 카메라 회전을 통해 캐릭터를 둘러볼 수 있음)
        if (currentSpeed > 0.2f
            || playerInput.fire
            || playerShooter.aimState == PlayerShooter.AimState.HipFire)
        {
            Rotate();
        }

        Move(playerInput.moveInput);
        
        if (playerInput.jump) Jump();
    }

    private void Update()
    {
        UpdateAnimation(playerInput.moveInput);
    }

    public void Move(Vector2 moveInput)
    {
        // 타겟 스피드
        // 키보드의 경우 1로 정규화 될 것이고,
        // 패드(스틱)의 경우 1보다 작은 값이 입력 되어서 최대 스피드보다 느리게 이동할 수 있음
        var targetSpeed = speed * moveInput.magnitude;

        var moveDirection 
            = Vector3.Normalize(transform.forward * moveInput.y + transform.right * moveInput.x);

        // 현재 속도에서 타겟속도로 확 바뀌는게 아니라
        // 부드럽게 이어지도록 스무쓰 댐핑한다.
        var smoothTime 
            = characterController.isGrounded ? speedSmoothTime : speedSmoothTime / airControlPercent;

        targetSpeed = Mathf.SmoothDamp(currentSpeed, targetSpeed, ref speedSmoothVelocity, smoothTime);

        // 중력 가속도를 y축 속도에 매번 더해준다.
        // (공중에 있을 때 하강할 수 있도록)
        currentVelocityY += Time.deltaTime * Physics.gravity.y;

        // 방향 * 타겟 스피드 + y축 속도
        var Velocity = moveDirection * targetSpeed + Vector3.up * currentVelocityY;

        characterController.Move(Velocity * Time.deltaTime);

        // 바닥에 닿아 있다면 y축 속도는 0으로 리셋
        // 갱신 안하면 계속 y축 속도가 증가되다가 속도가 빨라져서
        // 지면을 뚫고 나갈 가능성 있음..
        if(characterController.isGrounded)
        {
            currentVelocityY = 0f;
        }
    }

    public void Rotate()
    {
        var targetRotation = followCam.transform.eulerAngles.y;

        targetRotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetRotation, ref 
            turnSmoothVelocity, turnSmoothTime);

        transform.eulerAngles = Vector3.up * targetRotation;
    }

    public void Jump()
    {
        if (!characterController.isGrounded) return;

        currentVelocityY = jumpVelocity;
    }

    private void UpdateAnimation(Vector2 moveInput)
    {
        var animationSpeedPercent = currentSpeed / speed;

        animator.SetFloat("Vertical Move", moveInput.y * animationSpeedPercent, 0.05f, Time.deltaTime);
        animator.SetFloat("Horizontal Move", moveInput.x * animationSpeedPercent, 0.05f, Time.deltaTime);
    }
}