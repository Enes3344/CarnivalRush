using UnityEngine;

namespace CarnivalRush
{
    public sealed class Balloon : MonoBehaviour
    {
        public bool Dangerous { get; private set; }
        public Color Tint { get; private set; }
        public bool Popped { get; private set; }
        private float elapsed;
        private const float Duration = 0.18f;

        public void Configure(bool dangerous, Color tint)
        {
            Dangerous = dangerous;
            Tint = tint;
        }

        public void Pop()
        {
            Popped = true;
            GetComponentInChildren<MeshCollider>().enabled = false;
        }

        public bool AnimatePop(float delta)
        {
            if (!Popped)
                return false;

            elapsed += delta;
            float t = elapsed / Duration;
            if (t >= 1f)
                return true;

            transform.localScale = Vector3.one * (t < 0.25f
                ? Mathf.Lerp(1f, 1.10f, t * 4f)
                : Mathf.Lerp(1.10f, 0f, (t - 0.25f) / 0.75f));
            return false;
        }
    }
}
