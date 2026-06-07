# Guía de Configuración para Compilación Automática (CI/CD)

Para que el sistema de compilación automática funcione, necesitas añadir tres "Secrets" a tu repositorio de GitHub. Sigue estos pasos:

## 1. Obtener la Licencia de Unity (UNITY_LICENSE)

Este es el paso más importante. Como el servidor de GitHub no tiene interfaz gráfica, necesita este archivo para activarse.

1. Descarga e instala [Unity Hub](https://unity.com/download) en tu ordenador.
2. Inicia sesión con tu cuenta de Unity.
3. Ve a **Preferences > Licenses > Add**.
4. Selecciona **Get a free personal license**.
5. Una vez activada, localiza el archivo `.ulf` en tu ordenador:
   - **Windows**: `C:\ProgramData\Unity\Unity_v2022.x.ulf` (puede estar oculto).
   - **Mac**: `/Library/Application Support/Unity/Unity_v2022.x.ulf`.
6. Abre ese archivo con un editor de texto (como el Bloc de notas), copia **todo** su contenido.

## 2. Configurar los Secrets en GitHub

1. Entra en tu repositorio en GitHub: [StickmanFighter](https://github.com/roddynaranjo2-ui/StickmanFighter).
2. Ve a la pestaña **Settings** (Ajustes).
3. En el menú de la izquierda, busca **Secrets and variables > Actions**.
4. Haz clic en **New repository secret** para cada uno de estos tres:

| Nombre del Secret | Valor |
| :--- | :--- |
| **UNITY_LICENSE** | Pega aquí el contenido completo del archivo `.ulf` que copiaste. |
| **UNITY_EMAIL** | Tu correo de Unity (ej: `usuario@correo.com`). |
| **UNITY_PASSWORD** | Tu contraseña de Unity. |

## 3. ¡Listo!

Una vez guardados los tres secretos, ve a la pestaña **Actions** de tu repositorio. Verás que hay un proceso en marcha o puedes iniciarlo manualmente haciendo clic en "Run workflow".

Cuando termine (tarda unos minutos), podrás descargar el **APK** directamente desde los resultados del proceso ("Artifacts").
