using UnityEngine;

namespace Player
{
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private Animator animator;

        private InputHandler _inputHandler;
        private Rigidbody _playerRigidbody;

        private const float MoveSpeed = 5f;
        private const float RotateSpeed = 10f;

        private bool _isRunning;

        private static readonly int IsRunningHash = Animator.StringToHash("isRunning");

        private void Awake()
        {
            Initialize();
        }

        private void Update()
        {
            HandlePlayerMovement();
        }

        private void Initialize()
        {
            _inputHandler = GetComponent<InputHandler>();
            _playerRigidbody = GetComponent<Rigidbody>();
        }

        private void HandlePlayerMovement()
        {
            var inputVector = _inputHandler.GetMovementVectorNormalized();
            var moveDirection = new Vector3(inputVector.x, 0f, inputVector.y);
            
            _playerRigidbody.velocity = moveDirection * MoveSpeed;

            _isRunning = moveDirection != Vector3.zero;

            animator.SetBool(IsRunningHash, _isRunning);

            if (_isRunning)
            {
                transform.forward = Vector3.Slerp(transform.forward, moveDirection, Time.deltaTime * RotateSpeed);
            }
        }
    }
}