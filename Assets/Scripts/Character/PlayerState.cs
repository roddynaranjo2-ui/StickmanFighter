// PlayerState.cs — Clase base abstracta para todos los estados del jugador.

using System;
using UnityEngine;

namespace StickmanFighter.Character
{
    public abstract class PlayerState : IState
    {
        protected readonly PlayerController Player;
        protected readonly Rigidbody2D Rb;
        protected readonly Animator? Anim;

        protected PlayerState(PlayerController player)
        {
            Player = player ?? throw new ArgumentNullException(nameof(player));
            Rb = player.Rb;
            Anim = player.GetComponent<Animator>(); // Puede ser null en v1
        }

        public abstract void Enter();
        public abstract void Update();
        public virtual void FixedUpdate() { /* override opcional */ }
        public abstract void Exit();
    }
}
