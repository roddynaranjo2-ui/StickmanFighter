// AudioBus.cs — Fachada estática para reproducir SFX desde cualquier sitio sin
// pasar referencias del AudioManager. FIX G-06 SPRINT #3.

using UnityEngine;

namespace StickmanFighter.Audio
{
    public enum SfxId
    {
        Jump,
        Land,
        Punch,
        Kick,
        Hit,
        Death,
        MenuClick
    }

    public static class AudioBus
    {
        public static void Play(SfxId id, Vector3 worldPos = default)
        {
            var mgr = AudioManager.Instance;
            if (mgr != null) mgr.PlaySfx(id, worldPos);
        }
    }
}
