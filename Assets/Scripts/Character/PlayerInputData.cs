// PlayerInputData.cs — Struct value type, sin asignaciones GC.
// Encapsula el input de un frame y desacopla origen (táctil/teclado) de la lógica del jugador.

namespace StickmanFighter.Character
{
    public struct PlayerInputData
    {
        public bool MoveForward;   // Continuo
        public bool MoveBackward;  // Continuo
        public bool Crouch;        // Continuo
        public bool JumpPressed;   // Edge-triggered (true solo en frame de presión)
        public bool PunchPressed;  // Edge-triggered
        public bool KickPressed;   // Edge-triggered
    }
}
