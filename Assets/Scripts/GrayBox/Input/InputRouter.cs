using System.Collections.Generic;
using UnityEngine;

namespace ThreeCup.GrayBox
{
    /// <summary>
    /// Owns the only input state in the prototype. Characters never read keys directly,
    /// so switching the controlled side can never leak into rendering or movement rules.
    /// 单主控模式：WASD 驱动主控角色，方向键驱动另一个角色（两人可以同时动），
    /// Q 或鼠标左键点角色切换谁是主控。
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

            // Q 换手：WASD 交给另一个角色，方向键反过来接管刚才那个
            if (Input.GetKeyDown(KeyCode.Q)) ToggleActiveSide();

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

        /// <summary>高亮圈看这个：单主控下只有主控算"被操控"（虽然另一个也能用方向键走）。</summary>
        public bool IsControlled(PlayerSide side) =>
            mode == ControlMode.TwoPlayer || side == activeSide;

        public float GetHorizontal(PlayerSide side)
        {
            if (mode == ControlMode.TwoPlayer)
            {
                return side == PlayerSide.Left
                    ? Axis(KeyCode.A, KeyCode.D)
                    : Axis(KeyCode.LeftArrow, KeyCode.RightArrow);
            }

            // 单主控：WASD 给主控，方向键给另一个角色
            return side == activeSide
                ? Axis(KeyCode.A, KeyCode.D)
                : Axis(KeyCode.LeftArrow, KeyCode.RightArrow);
        }

        public bool GetJumpDown(PlayerSide side)
        {
            if (mode == ControlMode.TwoPlayer)
            {
                return side == PlayerSide.Left
                    ? Input.GetKeyDown(KeyCode.W)
                    : Input.GetKeyDown(KeyCode.UpArrow);
            }

            // 主控用 W（外加空格），另一个用 ↑
            return side == activeSide
                ? Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.Space)
                : Input.GetKeyDown(KeyCode.UpArrow);
        }

        static float Axis(KeyCode negative, KeyCode positive) =>
            (Input.GetKey(positive) ? 1f : 0f) - (Input.GetKey(negative) ? 1f : 0f);
    }
}
