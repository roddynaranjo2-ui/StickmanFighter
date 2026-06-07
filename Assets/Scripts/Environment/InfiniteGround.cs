// InfiniteGround.cs — replica el tile del suelo a izquierda/derecha del jugador
// para crear sensación de suelo infinito.
//
// FIX C-07: ahora puede auto-localizar al jugador (FindWithTag("Player")) y al tile prototype
// (buscando un GameObject de nombre "Ground" en la escena). Esto permite que el sistema funcione
// aunque el inspector no esté cableado.

using UnityEngine;

namespace StickmanFighter.Environment
{
    public sealed class InfiniteGround : MonoBehaviour
    {
        [SerializeField] private Transform? _player;
        [SerializeField] private Transform? _tilePrototype;
        [SerializeField] private float _tileWidth = 25.6f;
        [SerializeField] private int _tilesPerSide = 3;

        private Transform[] _tiles = System.Array.Empty<Transform>();

        private void Awake()
        {
            if (_player == null)
            {
                try
                {
                    var go = GameObject.FindWithTag("Player");
                    if (go != null) _player = go.transform;
                }
                catch { /* tag no registrada */ }

                if (_player == null)
                {
                    var pc = Object.FindAnyObjectByType<StickmanFighter.Character.PlayerController>();
                    if (pc != null) _player = pc.transform;
                }
            }

            if (_tilePrototype == null)
            {
                var ground = GameObject.Find("Ground");
                if (ground != null) _tilePrototype = ground.transform;
                else if (transform.childCount > 0) _tilePrototype = transform.GetChild(0);
            }
        }

        private void Start()
        {
            if (_tilePrototype == null) return;

            // Calcula automáticamente _tileWidth si el prototype tiene un SpriteRenderer.
            var sr = _tilePrototype.GetComponent<SpriteRenderer>();
            if (sr != null && sr.sprite != null && sr.bounds.size.x > 0.01f)
            {
                _tileWidth = sr.bounds.size.x;
            }

            int total = _tilesPerSide * 2 + 1;
            _tiles = new Transform[total];
            _tiles[0] = _tilePrototype;
            for (int i = 1; i < total; i++)
            {
                var clone = Instantiate(_tilePrototype, _tilePrototype.parent != null ? _tilePrototype.parent : transform);
                clone.name = _tilePrototype.name + "_clone_" + i;
                _tiles[i] = clone;
            }
            RepositionAll();
        }

        private void LateUpdate()
        {
            if (_player == null || _tiles.Length == 0) return;
            float px = _player.position.x;
            float refTileX = Mathf.Floor(px / _tileWidth) * _tileWidth;

            for (int i = 0; i < _tiles.Length; i++)
            {
                if (_tiles[i] == null) continue;
                int offset = i - _tilesPerSide;
                var pos = _tiles[i].position;
                pos.x = refTileX + offset * _tileWidth;
                _tiles[i].position = pos;
            }
        }

        private void RepositionAll()
        {
            if (_player == null) return;
            float px = _player.position.x;
            float refTileX = Mathf.Floor(px / _tileWidth) * _tileWidth;
            for (int i = 0; i < _tiles.Length; i++)
            {
                if (_tiles[i] == null) continue;
                int offset = i - _tilesPerSide;
                var pos = _tiles[i].position;
                pos.x = refTileX + offset * _tileWidth;
                _tiles[i].position = pos;
            }
        }
    }
}
