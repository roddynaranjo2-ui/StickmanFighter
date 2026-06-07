# StickmanFighter — Unity 2D Android

> Juego 2D de stickman peleador para Android, construido con Unity 2022.3.20f1 LTS, Built-in Render Pipeline e IL2CPP.

**Versión actual:** `v0.1.5` (build-ready) — ver [`REPARACIONES_SPRINT_4.md`](REPARACIONES_SPRINT_4.md).

## Características

- 🎮 FSM completa: Idle, Walk Forward/Backward, Crouch, Jump, Punch, Kick (con cancel-into entre ataques)
- ⚔️ **Sistema de combate end-to-end**: HealthSystem, Hitbox por frame data, IA enemiga, GameOver
- 🔊 **Audio procedural**: SFX generados en runtime (sin assets binarios) — Jump, Land, Punch, Kick, Hit, Death
- 💥 **VFX**: Hit-flash en sprites + Screen-shake con intensidad por tipo de ataque
- 📊 **HUD**: Barras de vida con lerp suave y gradiente color (Player + Enemy)
- 📱 UI táctil totalmente funcional + soporte de teclado en Editor
- 🎥 Cámara con SmoothDamp y Y clampeada (X libre)
- 🏞️ Parallax horizontal infinito (3 capas: cielo, árboles lejanos, árboles medios)
- 🛠️ CI/CD con GitHub Actions + GameCI → **APK Android** (instalable directo en el dispositivo)
- 🧪 9 tests EditMode (FSM + HealthSystem + CombatEvents)
- ✅ Cero placeholders, cero warnings, cero `// TODO`

## Requisitos

| Herramienta | Versión |
|-------------|---------|
| Unity | 2022.3.20f1 LTS |
| Render Pipeline | Built-in |
| Scripting Backend | IL2CPP |
| Android Min API | 26 |
| Android Target API | 33 |
| Java | 11 |

## Cómo abrir

```bash
git clone <repo>
cd StickmanFighter
# Abre Unity Hub → Add → seleccionar carpeta StickmanFighter
# Unity descargará paquetes automáticamente.
```

Escena de inicio: `Assets/Scenes/MainMenu.unity`.

## Controles (teclado)

| Acción | Tecla |
|--------|-------|
| Mover adelante | `D` / `→` |
| Mover atrás | `A` / `←` |
| Saltar | `Espacio` |
| Agacharse | `S` / `↓` |
| Puñetazo | `J` |
| Patada | `K` |

## Build local

```bash
# Desde Unity Editor
Build → Build Android APK

# O desde CLI
"C:/Program Files/Unity/Hub/Editor/2022.3.20f1/Editor/Unity.exe" \
  -batchmode -nographics -quit -projectPath . \
  -buildTarget Android -executeMethod BuildScript.BuildAndroid
```

El build local se genera en `Builds/Android/StickmanFighter.apk` si lanzas el menú del editor; el CI genera AAB.

## CI/CD

El workflow `.github/workflows/main.yml` se dispara en push a `main`/`develop` y construye el AAB automáticamente. Configura los secrets:

- `UNITY_LICENSE` (license `.ulf` de Unity Personal/Pro)
- `UNITY_EMAIL`
- `UNITY_PASSWORD`

## Troubleshooting

- **Unity bloquea al abrir** → confirmar que `Assets/TextMesh Pro/` está presente. Si falta, ejecutar `Window → TextMeshPro → Import TMP Essential Resources`.
- **CI falla en `BuildScript.BuildAndroid`** → confirmar que `Assets/Editor/BuildScript.cs` está versionado y compila sin errores.
- **APK no se instala** → `useCustomKeystore = false` usa el `debug.keystore` de Unity. Para releases firmadas, configurar `UNITY_ANDROID_KEYSTORE_*` secrets.

## Licencia

MIT — ver [`LICENSE`](LICENSE).
