using UnityEngine;

namespace Game.Common
{
    public class Item : MonoBehaviour
    {
        private MeshFilter _meshFilter;
        private MeshRenderer _meshRenderer;
        private BoxCollider _boxCollider;

        public ItemType itemType;
        public bool IsPlaced { get; private set; }

        private void Awake()
        {
            Initialize();
        }

        private void Initialize()
        {
            _meshFilter = GetComponent<MeshFilter>();
            _meshRenderer = GetComponent<MeshRenderer>();
            _boxCollider = GetComponent<BoxCollider>();
            IsPlaced = false;
        }

        public void SetPlaced(bool placed)
        {
            IsPlaced = placed;
        }

        public void InitializeItem(ItemSO itemSo)
        {
            if (_meshFilter != null && itemSo.mesh != null)
                _meshFilter.mesh = itemSo.mesh;

            if (_meshRenderer != null && itemSo.material != null)
                _meshRenderer.material = itemSo.material;

            itemType = itemSo.itemType;

            UpdateCollider();
        }

        private void UpdateCollider()
        {
            if (_boxCollider is not null && _meshFilter is not null)
            {
                _boxCollider.center = _meshFilter.mesh.bounds.center;
                _boxCollider.size = _meshFilter.mesh.bounds.size;
            }
        }

        public float GetColliderBoundSizeY()
        {
            return _boxCollider.size.y;
        }

        public void ResetItem()
        {
            itemType = ItemType.None;
            IsPlaced = false;
        }
    }
}

public enum ItemType
{
    None,
    Raw,
    Processed
}