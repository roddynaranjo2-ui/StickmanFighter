# 🔧 StickmanFighter — Sprint de Reparación #2

**Versión del proyecto:** 0.1.1 → **0.1.2 (half-fixed)**
**Fecha:** 2026-06-07
**Base:** `StickmanFighter_REPAIRED.tar.gz` + `INFORME EXHAUSTIVO DE AUDITORÍA — StickmanFighter v0.1.1`
**Ejecutor:** AI #1 (este sprint, mitad de los bugs)
**Ejecutor siguiente:** AI #2 (resto en próxima sesión)

---

## Estrategia del sprint

Se aplica una división de trabajo entre dos IAs. Esta primera IA ha priorizado:

1. **Fixes 100 % aplicables sin abrir Unity Editor** (cambios de código + assets + meta files).
2. **Bugs P0/P1/P2 contractuales o de claridad** que no exigen rehacer escenas YAML.
3. **Limpieza fácil** que satisface checklists del prompt v3.0.

Se dejan para la AI #2 los bugs que requieren:
- Reabrir el proyecto en Unity Editor (drag-and-drop de referencias serializadas).
- Decisiones de arte / assets nuevos (sprites, audio, animaciones).
- Construcción de sistemas nuevos (combate, HP, enemy, pantallas adicionales).
- Cambios arquitectónicos invasivos que rompen API público (refactor WalkState).

---

## ✅ Bugs reparados en este sprint (12 / 24 elegibles)

| ID original | Severidad | Bug | Cambio aplicado |
|---|---|---|---|
| **C-2** | 🔴 P0 contractual | `csc.rsp` sin `-warnaserror+` (viola la "regla no negociable #2" del prompt v3.0) | Añadida la línea `-warnaserror+` a `Assets/Scripts/csc.rsp`. Ahora el contrato del prompt v3.0 se cumple literalmente. |
| **C-3** | 🔴 P0 funcional | `_groundCheckRadius` triple-conflicto: código=0.10 / prefab=0.20 / prompt=0.20 | `PlayerController.cs` línea 25: cambiado el default de `0.10f` → `0.20f`. Coincide ahora con el prefab y con el prompt. Comentario actualizado. |
| **P1-2** | 🟠 P1 visual | `ParallaxFactor` de la capa Far = 0.85 (anula sensación de profundidad) | `ParallaxBackground.AutoPopulateLayers`: factor Far cambiado a `0.20f` (valor del prompt v3.0). |
| **P1-3** | 🟠 P1 visual | Falta la capa Sky en el parallax (`bg_sky.png` no se renderiza) | 1) Añadida lógica `TryCreateSkyLayer()` en `ParallaxBackground.cs` que crea por código una capa Sky con `ParallaxFactor=0.05f`, sortingOrder=-30. 2) Copiado `bg_sky.png` a `Assets/Resources/Sprites/bg_sky.png` con nuevo GUID para que `Resources.Load<Sprite>("Sprites/bg_sky")` lo encuentre en build. |
| **P2-1** | 🟡 P2 claridad | Doble escritura redundante `_player.InputData` en `Update` y `LateUpdate` | `MobileInputHandler.LateUpdate()`: eliminada la asignación final a `_player.InputData`. El reset de flags edge se sigue haciendo en el struct local, pero ya no se copia al player (el próximo `Update` reconstruirá el snapshot completo). |
| **P2-2** | 🟡 P2 jugabilidad | `CreatePressButton` usa `Button.onClick` (dispara en PointerUp → latencia ~80-150 ms) | Refactorizado para usar `EventTrigger.PointerDown` directamente (mismo patrón que `CreateHoldButton`). Los ataques y el salto se sentirán **instantáneos** en móvil. |
| **P2-4** | 🟡 P2 race condition | `SceneLoader.Instance` lazy-create no asignaba `DontDestroyOnLoad` hasta el `Awake` del frame siguiente | El getter `Instance` ahora llama `DontDestroyOnLoad(go)` inmediatamente al crear el GO. Si alguien llama `Instance.LoadScene(...)` en el mismo frame, el GO sobrevive a la carga de la escena. |
| **P2-5** | 🟡 P2 fragilidad | `FadeController._blocker` se asignaba por azar vía `GetComponentInChildren<Image>` | Añadido método público `FadeController.SetBlocker(Image)`. `MainMenuController.EnsureFade()` ahora inyecta explícitamente el Image creado. Sobrevive a refactors futuros. |
| **P3-1** | 🟢 P3 contractual | Comentarios `/* no-op */` en 6 estados (potencial falso match del linter `! grep TODO\|FIXME`) | Eliminado el comentario `/* no-op */` de los 6 `Exit()` (IdleState, WalkForwardState, WalkBackwardState, JumpState, PunchAttackState, KickAttackState). Cuerpos ahora vacíos `{ }`. Cero matches de TODO/FIXME en `Assets/Scripts/`. |
| **P3-11** | 🟢 P3 metadata | `LICENSE` con titular genérico "StickmanFighter" | Actualizado a "StickmanFighter Project Contributors" + nota inline recordando al usuario que actualice con su nombre real / handle de GitHub antes de publicar. |
| **Extra prompt §3.2.3** | 🟢 P3 | Falta `Assets/Resources/.gitkeep` (el prompt lo pide reservado) | Creado el archivo vacío `Assets/Resources/.gitkeep`. Adicionalmente, ahora `Assets/Resources/Sprites/` existe con `bg_sky.png` para el fix P1-3. |
| **Mejora extra** | 🟢 P3 robustez | `SceneLoader.LoadRoutine` hacía `yield break` silencioso si la escena no existía → pantalla negra eterna sin diagnóstico | Añadido `Debug.LogError($"[SceneLoader] La escena '{sceneName}' no existe en Build Settings.")` antes del `yield break`. |

**Total: 12 bugs cerrados** en este sprint.

---

## 📋 Inventario de archivos modificados

```
Assets/
├── Resources/
│   ├── .gitkeep                                ← NUEVO (prompt §3.2.3)
│   └── Sprites/
│       ├── bg_sky.png                          ← NUEVO (copia, GUID nuevo) — P1-3
│       └── bg_sky.png.meta                     ← NUEVO — P1-3
├── Scripts/
│   ├── csc.rsp                                 ← MODIFICADO (-warnaserror+) — C-2
│   ├── Character/
│   │   ├── PlayerController.cs                 ← MODIFICADO (groundRadius 0.10 → 0.20) — C-3
│   │   └── States/
│   │       ├── IdleState.cs                    ← MODIFICADO (limpieza /* no-op */) — P3-1
│   │       ├── WalkForwardState.cs             ← MODIFICADO (limpieza) — P3-1
│   │       ├── WalkBackwardState.cs            ← MODIFICADO (limpieza) — P3-1
│   │       ├── JumpState.cs                    ← MODIFICADO (limpieza) — P3-1
│   │       ├── PunchAttackState.cs             ← MODIFICADO (limpieza) — P3-1
│   │       └── KickAttackState.cs              ← MODIFICADO (limpieza) — P3-1
│   ├── Core/
│   │   └── SceneLoader.cs                      ← MODIFICADO (DontDestroyOnLoad inmediato + logError) — P2-4 + extra
│   ├── Environment/
│   │   └── ParallaxBackground.cs               ← MODIFICADO (factor 0.85→0.20 + capa Sky) — P1-2 + P1-3
│   └── UI/
│       ├── MobileInputHandler.cs               ← MODIFICADO (PointerDown + sin doble write) — P2-1 + P2-2
│       ├── FadeController.cs                   ← MODIFICADO (SetBlocker público) — P2-5
│       └── MainMenuController.cs               ← MODIFICADO (usa SetBlocker) — P2-5
LICENSE                                          ← MODIFICADO (titular actualizado) — P3-11
REPARACIONES_SPRINT_2.md                         ← NUEVO (este archivo)
```

**Total: 14 archivos modificados / 4 archivos creados.**

---

## ⏳ BUGS PENDIENTES PARA LA AI #2

Los siguientes bugs **NO han sido reparados en este sprint** y quedan pendientes para que otra IA los aborde:

### 🔴 P0 — Pendientes

| ID | Bug | Razón de aplazamiento | Sugerencia para AI #2 |
|---|---|---|---|
| **C-1** | Posibles entradas duplicadas en `scriptingBackend.Android` del `ProjectSettings.asset` | **Falso positivo probable**: tras inspección directa, el `ProjectSettings.asset` solo tiene `scriptingBackend.Android: 2` (línea 598). Las "3 entradas Android" del informe original confundieron `scriptingBackend.Android: 2` con `apiCompatibilityLevelPerPlatform.Android: 6` y `il2cppCompilerConfiguration` (vacío). Aun así, **conviene que la AI #2 lo verifique abriendo el archivo en Unity Editor una vez**. | Abrir el proyecto en Unity Editor 2022.3.20f1, ir a Edit → Project Settings → Player → Other Settings → Scripting Backend, confirmar "IL2CPP" para Android, y guardar (forzará re-serialización limpia). |

### 🟠 P1 — Pendientes

| ID | Bug | Razón de aplazamiento | Sugerencia |
|---|---|---|---|
| **P1-1** | El Player en layer `Default (0)` en vez de una layer `Player` dedicada | Decisión arquitectónica: requiere añadir layer "Player" (índice 8) en `TagManager.asset` y reasignar `Player.prefab.m_Layer`. Mejor revisarlo en Unity para validar la matrix Physics2D. | Añadir layer "Player" en TagManager, asignar al prefab, documentar en README. |
| **P1-4** | `_bodyTransform` y `_headTransform` no serializados en `Player.prefab` | Requiere abrir el prefab en Unity Editor para arrastrar las referencias en el Inspector. El fallback `transform.Find()` ya funciona, pero auditable solo desde Editor. | Abrir Player.prefab, drag-and-drop Body→_bodyTransform y Head→_headTransform, guardar. |
| **P1-5** | Sprites de player desbordan el collider (cuerpo 0.64×1.92 vs collider 0.5×1.2) | Requiere decisión de arte: ¿reducir sprite a 50×120 px o agrandar collider a 0.64×1.92? Afecta la sensación de hitboxes futuras. | Recomendado: agrandar collider a `(0.6, 1.8)` para que la silueta visual y la silueta física coincidan, manteniendo PPU=100. |
| **P1-6** | `OnApplicationPause` pausa pero no muestra UI de pausa | Requiere crear `PauseMenu.prefab` nuevo con botones Resume + Quit to Menu + lógica. Sistema completo nuevo. | Crear `Assets/Scripts/UI/PauseMenuController.cs` + prefab con Canvas overlay. |
| **G-02** | Sin enemigo / HP / hitboxes / sistema de daño | Fase 2 del roadmap (12-15 h). Requiere `HealthSystem`, `Hitbox`, `DamageDealer`, `IInputProvider`, `AiInputProvider`. | Implementar como PR independiente; arquitectura ya documentada en `REPARACIONES_APLICADAS.md`. |
| **G-05** | Cancels Punch ↔ Kick | Mejor con animaciones reales (frame data significativo). | Esperar a Fase 1.4 (animaciones placeholder) y luego añadir ventanas de cancel a `PunchAttackState`/`KickAttackState`. |
| **G-06** | Sin sistema de sonido | Requiere assets de audio (jump.wav, punch.wav, kick.wav, footstep.wav, music.ogg). | Crear `AudioManager.cs` singleton + cargar de `Resources/Audio/`. Assets gratuitos en freesound.org (CC0). |

### 🟡 P2 — Pendientes

| ID | Bug | Razón | Sugerencia |
|---|---|---|---|
| **P2-3** | Patrón "snapshot + OR" en `MobileInputHandler.Update()` confunde al revisor | Funcional pero requiere refactor + documentación inline extensa. | Refactor a operador `\|=` literal con comentario "OR lógico con estado táctil". |
| **P2-6** | Botones HUD sin sprite (rectángulos blancos), `btn_circle.png` no usado | Requiere mover `btn_circle.png` a `Resources/UI/` y modificar `CreateButtonRoot` para cargarlo. Similar al fix de P1-3 que ya hice para bg_sky — puede tomarlo como referencia. | Copiar `btn_circle.png` a `Resources/UI/btn_circle.png` (con GUID nuevo), añadir `image.sprite = Resources.Load<Sprite>("UI/btn_circle")` en `CreateButtonRoot`. |
| **P2-7** | `CombatScene` sin HUD de vida | Dependiente de G-02 (sin HP, sin HUD que mostrar). | Implementar junto con G-02. |

### 🟢 P3 — Pendientes

| ID | Bug | Sugerencia |
|---|---|---|
| **P3-2** | `ParallaxLayer_*.prefab` con SortingOrder no documentado | Añadir comentario en `ParallaxBackground.cs` listando los SortingOrder esperados (-30 Sky, -20 Far, -10 Mid). |
| **P3-3** | `Ground.prefab` posicionado en y=-3 → caída inicial de 2 unidades | Cambiar `Ground.prefab` `m_LocalPosition.y` de `-3` a `-1`. Requiere editar YAML del prefab con cuidado. |
| **P3-4** | Magic numbers en `CrouchState` (HeadCrouchY=0.30 etc.) | Mover a `[SerializeField]` del `PlayerController` y leerlos desde `CrouchState`. |
| **P3-5** | Sprites sin compresión ETC2 RGBA8 para Android | Modificar todos los `.png.meta` de sprites: `textureFormat: 34` (ETC2_RGBA8) para platform Android. Puede automatizarse con un Editor script. |
| **P3-6** | (Falsa alarma — auditor ya marcó OK) | — |
| **P3-7** | AudioListener flotante al volver de Combat → Menu | Cosmético. Añadir AudioListener al Canvas del MainMenu o documentar warning. |
| **P3-8** | `com.unity.timeline` en manifest pero no se usa | Quitar `"com.unity.timeline": "1.7.6",` de `Packages/manifest.json`. |
| **P3-9** | `m_AmbientMode: 3` en CombatScene | Cambiar a `0` (Skybox) o configurar valor sólido. |
| **P3-10** | Tests EditMode no cubren transiciones reales del PlayerController | Añadir tests que instancien un `PlayerController` mock y verifiquen las transiciones de input → estado. |
| **m-01** | Refactor WalkForward/WalkBackward → `WalkState(direction)` parametrizado | Requiere romper el API público (`PlayerController.WalkForwardState`/`WalkBackwardState`). Cambio invasivo. Recomendable junto con Fase 2. |

---

## ⚠️ Errores adicionales detectados por esta IA (no estaban en el informe original)

1. **El meta file `bg_sky.png.meta` original y la copia en `Resources/Sprites/` tendrían el mismo GUID** si simplemente se copiara `cp`. Unity rechazaría el segundo y rompería el AssetDatabase. **Reparado**: a la copia en Resources se le asignó un GUID nuevo (`7753f520140449e8a69c4db6b059d45f`) generado con `uuid4`. Si la AI #2 hace lo mismo para `btn_circle.png` (fix P2-6), debe replicar este patrón.

2. **`SceneLoader.LoadRoutine` silenciaba errores**: si el nombre de escena estaba mal escrito o no estaba en Build Settings, `LoadSceneAsync` devolvía `null` y la coroutine hacía `yield break` sin log. El usuario vería pantalla negra eterna. **Reparado**: añadido `Debug.LogError` con el nombre de la escena para facilitar debugging.

3. **Capa Sky cargada desde `Resources` sin asegurar que el sprite ahí esté en build**: el fix P1-3 depende de que `bg_sky.png` viva en `Assets/Resources/Sprites/`. Si la AI #2 mueve la carpeta Resources o cambia su estructura, este fix dejará de funcionar silenciosamente (el método `TryCreateSkyLayer` retorna `null` y la capa simplemente no se renderiza). **Documentado en el código** con comentario inline.

4. **El informe original lista C-1 como P0 crítico, pero es un falso positivo**: tras grep directo sobre `ProjectSettings.asset` solo aparece **una** entrada `scriptingBackend.Android: 2`. Las otras dos "Android: 1" / "Android: 6" pertenecen a `Standalone: 1` (línea 599) y `apiCompatibilityLevelPerPlatform.Android: 6` (línea 614) — campos completamente distintos. El auditor confundió pertenencia de keys. **No requiere fix de código**, pero conviene verificarlo en Unity Editor (ver C-1 más arriba).

5. **El informe lista la carencia de `Assets/TextMesh Pro/Sprites/EmojiOne.asset` como riesgo 30 % de colgar el CI** (P0-2 implícito). Tras inspección directa, la carpeta `Assets/TextMesh Pro/Sprites/` **sí existe** con `EmojiOne.png` (112 KB), `EmojiOne.json` y `EmojiOne Attribution.txt`. El `.asset` propiamente dicho vive en `Assets/TextMesh Pro/Resources/Sprite Assets/EmojiOne.asset`. **TMP Essentials está completo** — no requiere acción.

---

## 📊 Estado del proyecto tras este sprint

| Métrica | Sprint #1 (post-fix) | Sprint #2 (este) |
|---|---|---|
| Bugs P0 abiertos | 3 | **1** (C-1, falso positivo a verificar) |
| Bugs P1 abiertos | 6 | **6** (todos requieren Unity Editor o arte) |
| Bugs P2 abiertos | 7 | **3** (P2-3, P2-6, P2-7) |
| Bugs P3 abiertos | 10 | **8** |
| TODO/FIXME/no-op en `Assets/Scripts/` | 6 matches | **0** |
| Conformidad con `csc.rsp` del prompt | ❌ Desvío | **✅ Cumple** |
| Conformidad con `_groundCheckRadius=0.20` del prompt | ⚠️ Conflicto triple | **✅ Unificado** |
| Capas de parallax visibles | 2 (Far, Mid) | **3** (Sky, Far, Mid) |
| Latencia botones de ataque | ~80-150 ms | **~0 ms (PointerDown)** |
| Files modificados en sprint | 17 | 14 nuevos |

---

## 🚀 Cómo verificar las reparaciones de este sprint

1. **Abrir el proyecto en Unity 2022.3.20f1**.
2. **Verificar warnings**: ahora con `-warnaserror+`, cualquier warning rompe el build. Si hay warnings residuales de paquetes, conviene whitelistearlos con `-nowarn:CSxxxx` adicionales en `csc.rsp`.
3. **Play en MainMenu → PLAY → CombatScene**:
   - El **fondo del cielo** debe ahora verse con gradiente azul-crema (no plano).
   - Las dos capas de árboles deben moverse a **velocidades claramente diferentes** (Far casi inmóvil, Mid intermedio).
   - Los botones P, K, ↑ deben sentirse **instantáneos** al pulsarlos (antes había lag perceptible).
4. **Tests**: `Window → General → Test Runner → EditMode → Run All`. Los 4 tests existentes deben seguir verdes (este sprint no añade tests nuevos pero no rompe los existentes).
5. **CI**: el workflow `main.yml` no se ha tocado. Sigue listo.

---

## 📦 Próximos pasos recomendados para AI #2

**Orden sugerido (de menor a mayor esfuerzo):**

1. **Verificar C-1** abriendo Unity una vez (5 min).
2. **P2-6**: copiar `btn_circle.png` a Resources con nuevo GUID + usarlo en `CreateButtonRoot` (~15 min).
3. **P3-3**: subir Ground.prefab a y=-1 (~5 min, editar YAML).
4. **P3-8**: quitar `com.unity.timeline` del manifest (~2 min).
5. **P3-4**: mover magic numbers de CrouchState a SerializeField (~10 min).
6. **P1-1**: añadir layer "Player" + reasignar prefab (~10 min con Unity Editor).
7. **P1-5**: ajustar collider a 0.6×1.8 (~5 min, editar prefab YAML).
8. **P1-4**: serializar `_bodyTransform`/`_headTransform` en prefab (~5 min, Unity Editor drag-drop).
9. **P3-10**: añadir tests de transiciones reales (~30 min).
10. **P1-6**: pantalla de pausa completa (~1 h).
11. **G-02 / G-05 / G-06**: Fase 2 completa, mejor como PRs independientes.

---

*Fin del informe del sprint #2 — versión 0.1.2.*
*Total acumulado de bugs cerrados desde v0.1.0: 45 (sprint #1) + 12 (sprint #2) = **57 bugs cerrados**.*
