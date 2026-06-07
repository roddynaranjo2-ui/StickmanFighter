// ParallaxBackground.cs — capas de fondo con parallax + repetición horizontal infinita.
// FIX C-06: auto-poblar _layers buscando ParallaxLayer_Sky/Far/Mid en la escena si está vacío.
// FIX P1-2: ParallaxFactor de la capa Far corregido de 0.85 → 0.20 (valor del prompt v3.0).
// FIX P1-3: Añadida la capa Sky (factor 0.05) usando bg_sky.png — antes el sprite no se renderizaba.

using System.Collections.Generic;
using UnityEngine;

namespace StickmanFighter.Environment
{
    [System.Serializable]
    public class ParallaxLayer
    {
        public Transform? LayerTransform;
        [Range(0f, 1f)] public float ParallaxFactor;
        public bool InfiniteHorizontal;
        public float SpriteWidth;
    }

    public sealed class ParallaxBackground : MonoBehaviour
    {
        [SerializeField] private Camera? _mainCamera;
        [SerializeField] private List<ParallaxLayer> _layers = new List<ParallaxLayer>();

        private float _previousCameraX;

        private void Start()
        {
            if (_mainCamera == null) _mainCamera = Camera.main;

            // FIX C-06: si _layers está vacío, auto-poblar buscando por nombre las capas en la escena.
            if (_layers.Count == 0)
            {
                AutoPopulateLayers();
            }

            if (_mainCamera != null) _previousCameraX = _mainCamera.transform.position.x;
        }

        private void AutoPopulateLayers()
        {
            // FIX P1-3: si no existe ParallaxLayer_Sky en la escena, lo creamos defensivamente con bg_sky.
            var sky = GameObject.Find("ParallaxLayer_Sky");
            if (sky == null) sky = TryCreateSkyLayer();

            var far = GameObject.Find("ParallaxLayer_Far");
            var mid = GameObject.Find("ParallaxLayer_Mid");

            // Sky — fondo casi inmóvil (sensación de horizonte).
            if (sky != null)
            {
                float w = GetSpriteWorldWidth(sky);
                _layers.Add(new ParallaxLayer
                {
                    LayerTransform = sky.transform,
                    ParallaxFactor = 0.05f,   // FIX P1-3: 5 % de velocidad de cámara.
                    InfiniteHorizontal = true,
                    SpriteWidth = w > 0f ? w : 30f
                });
            }

            if (far != null)
            {
                float w = GetSpriteWorldWidth(far);
                _layers.Add(new ParallaxLayer
                {
                    LayerTransform = far.transform,
                    ParallaxFactor = 0.20f,   // FIX P1-2: corregido de 0.85 → 0.20 (spec prompt v3.0).
                    InfiniteHorizontal = true,
                    SpriteWidth = w > 0f ? w : 10f
                });
            }
            if (mid != null)
            {
                float w = GetSpriteWorldWidth(mid);
                _layers.Add(new ParallaxLayer
                {
                    LayerTransform = mid.transform,
                    ParallaxFactor = 0.50f,
                    InfiniteHorizontal = true,
                    SpriteWidth = w > 0f ? w : 8f
                });
            }
        }

        /// <summary>FIX P1-3: crea por código una capa Sky con el sprite bg_sky para que el horizonte
        /// con gradiente sea visible. Si el sprite no se encuentra en Resources, se omite silenciosamente.</summary>
        private static GameObject? TryCreateSkyLayer()
        {
            // Intentar cargar desde Resources/Sprites/bg_sky (el usuario debe mover bg_sky.png a Assets/Resources/Sprites/ para que esto funcione en build).
            // Si no existe, retornamos null y la capa Sky simplemente no se renderiza (no rompe nada).
            var sprite = Resources.Load<Sprite>("Sprites/bg_sky");
            if (sprite == null) return null;

            var go = new GameObject("ParallaxLayer_Sky");
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = -30;   // detrás de Far (-20) y Mid (-10).

            // Posicionar centrado horizontalmente con la cámara, en z=10 para que quede al fondo.
            var cam = Camera.main;
            float camX = cam != null ? cam.transform.position.x : 0f;
            float camY = cam != null ? cam.transform.position.y : 0f;
            go.transform.position = new Vector3(camX, camY, 10f);
            return go;
        }

        private static float GetSpriteWorldWidth(GameObject go)
        {
            var sr = go.GetComponent<SpriteRenderer>();
            if (sr != null && sr.sprite != null)
            {
                return sr.bounds.size.x;
            }
            return 0f;
        }

        private void LateUpdate()
        {
            if (_mainCamera == null) return;

            float currentX = _mainCamera.transform.position.x;
            float deltaX = currentX - _previousCameraX;

            for (int i = 0; i < _layers.Count; i++)
            {
                var layer = _layers[i];
                if (layer.LayerTransform == null) continue;

                Vector3 pos = layer.LayerTransform.position;
                pos.x += deltaX * (1f - layer.ParallaxFactor);
                layer.LayerTransform.position = pos;

                if (layer.InfiniteHorizontal && layer.SpriteWidth > 0f)
                {
                    float distFromCam = Mathf.Abs(currentX - layer.LayerTransform.position.x);
                    if (distFromCam >= layer.SpriteWidth)
                    {
                        float dir = Mathf.Sign(currentX - layer.LayerTransform.position.x);
                        pos = layer.LayerTransform.position;
                        pos.x += dir * layer.SpriteWidth;
                        layer.LayerTransform.position = pos;
                    }
                }
            }

            _previousCameraX = currentX;
        }
    }
}
