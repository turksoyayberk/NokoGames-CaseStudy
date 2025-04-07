using UnityEngine;

namespace Player
{
    public class InputHandler : MonoBehaviour
    {
        private PlayerInputs _playerInputs;

        private void Awake()
        {
            Initialize();
        }

        private void Initialize()
        {
            _playerInputs = new PlayerInputs();
            _playerInputs.Player.Enable();
        }

        public Vector2 GetMovementVectorNormalized()
        {
            var inputVector = _playerInputs.Player.Move.ReadValue<Vector2>();

            inputVector = inputVector.normalized;
            return inputVector;
        }
    }
}