# StickmanFighter

![Unity Version](https://img.shields.io/badge/Unity-2022.3.20f1-blue.svg)
![Platform](https://img.shields.io/badge/Platform-Android-green.svg)

StickmanFighter es un emocionante juego de lucha en 2D desarrollado en Unity. Este repositorio contiene el código fuente completo, activos y configuraciones necesarias para compilar y ejecutar el juego.

## Características

- Mecánicas de combate fluidas.
- Sistema de salud y eventos de combate.
- Interfaz de usuario intuitiva para móviles.
- Fondos con efecto parallax y suelo infinito.
- Pruebas unitarias incluidas para asegurar la estabilidad del código.

## Requisitos de Compilación

- **Unity Editor**: Versión 2022.3.20f1.
- **Plataforma de Destino**: Android (API Level 33).
- **Backend de Scripting**: IL2CPP.

## Cómo Compilar

1. Clona el repositorio: `git clone https://github.com/roddynaranjo2-ui/StickmanFighter.git`
2. Abre el proyecto en Unity Hub utilizando la versión recomendada.
3. Para compilar el APK automáticamente:
   - Ve a `Tools > Fix Scaffold` para asegurar que todas las etiquetas y escenas estén configuradas correctamente.
   - Ve a `Build > Build Android APK` para generar el archivo instalable.

## Estructura del Proyecto

- `Assets/Scripts`: Contiene toda la lógica del juego organizada por módulos (Combat, Enemy, Player, UI, etc.).
- `Assets/Scenes`: Escenas principales del juego (MainMenu, CombatScene).
- `Assets/Editor`: Scripts de utilidad para el editor y procesos de construcción.
- `Assets/Tests`: Pruebas de modo edición.

## CI/CD

El proyecto está preparado para integración continua utilizando GitHub Actions. La configuración se encuentra en `build_configs/main.yml` (debe moverse a `.github/workflows/main.yml` si se dispone de los permisos necesarios en el repositorio).

---
Desarrollado con ❤️ para StickmanFighter.
