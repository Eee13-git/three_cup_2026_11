using System.Collections.Generic;
using UnityEngine;

namespace ThreeCup.GrayBox
{
    /// <summary>
    /// Owns the only input state in the prototype. Characters never read keys directly,
    /// so switching the controlled side can never leak into rendering or movement rules.
    /// </summary>
    public sealed class InputRouter : MonoBehaviour
    {
        [SerializeField] ControlMode mode = ControlMode.SinglePlayer;
        [SerializeField] PlayerSide activeSide = PlayerSide.Left;

        readonly List<PlayerMotor2D> players = new List<PlayerMotor2D>();

        public ControlMode Mode => mode;
        public PlayerSide ActiveSide => activeSide;

        public void Register(PlayerMotor2D player)
        {
            if (player != null && !players.Contains(player)) players.Add(player);
        }

        public void Unregister(PlayerMotor2D player) => players.Remove(player);

        void Update()
        {
            if (mode != ControlMode.SinglePlayer) return;
            if (!Input.GetMouseButtonDown(0)) return;

            var cam = Camera.main;
            if (cam == null) return;

            TrySelectAt(cam.ScreenToWorldPoint(Input.mousePosition));
        }

        /// <summary>
        /// Switches control only when the click actually lands on a character.
        /// Clicks on empty space are ignored, so switching has to be aimed.
        /// </summary>
        public bool TrySelectAt(Vector2 world)
        {
            var clicked = FindPlayerAt(world);
            if (clicked == null) return false;

            activeSide = clicked.Side;
            return true;
        }

        PlayerMotor2D FindPlayerAt(Vector2 world)
        {
            foreach (var player in players)
            {
                if (player == null) continue;

                var col = player.GetComponent<Collider2D>();
                if (col != null && col.OverlapPoint(world)) return player;
            }

            return null;
        }

        public void SetMode(ControlMode value) => mode = value;

        public void SetActiveSide(PlayerSide value) => activeSide = value;

        public void ToggleActiveSide() =>
            activeSide = activeSide == PlayerSide.Left ? PlayerSide.Right : PlayerSide.Left;

        public bool IsControlled(PlayerSide side) =>
            mode == ControlMode.TwoPlayer || side == activeSide;

        public float GetHorizontal(PlayerSide side)
        {
            if (!IsControlled(side)) return 0f;

            if (mode == ControlMode.TwoPlayer)
            {
                return side == PlayerSide.Left
                    ? Axis(KeyCode.A, KeyCode.D)
                    : Axis(KeyCode.LeftArrow, KeyCode.RightArrow);
            }

            // Single player: only one character listens, so both key sets may drive it.
            float wasd = Axis(KeyCode.A, KeyCode.D);
            return Mathf.Approximately(wasd, 0f) ? Axis(KeyCode.LeftArrow, KeyCode.RightArrow) : wasd;
        }

        public bool GetJumpDown(PlayerSide side)
        {
            if (!IsControlled(side)) return false;

            if (mode == ControlMode.TwoPlayer)
            {
                return side == PlayerSide.Left
                    ? Input.GetKeyDown(KeyCode.W)
                    : Input.GetKeyDown(KeyCode.UpArrow);
            }

            return Input.GetKeyDown(KeyCode.W)
                || Input.GetKeyDown(KeyCode.UpArrow)
                || Input.GetKeyDown(KeyCode.Space);
        }

        static float Axis(KeyCode negative, KeyCode positive) =>
            (Input.GetKey(positive) ? 1f : 0f) - (Input.GetKey(negative) ? 1f : 0f);
    }
}
