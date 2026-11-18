using UnityEngine;

namespace CarnivalRush
{
    public sealed class ToyShooter : MonoBehaviour
    {
        [SerializeField] private Camera aimCamera;
        [SerializeField] private Transform gun;
        [SerializeField] private GameObject muzzleFlash;
        [SerializeField] private AudioSource audioSource;
        private Vector3 restingPosition;
        private float yaw;
        private float pitch;
        private float kick;
        private float flashTime;
        private AudioClip shot;
        private AudioClip pop;
        private AudioClip failure;

        public void Configure(Camera camera, Transform toy, GameObject flash, AudioSource source)
        {
            aimCamera = camera;
            gun = toy;
            muzzleFlash = flash;
            audioSource = source;
        }

        private void Awake()
        {
            restingPosition = gun.localPosition;
            shot = MakeClip("Toy blaster", 0.095f, 420f, 0.67f);
            pop = MakeClip("Balloon pop", 0.17f, 720f, 0.88f);
            failure = MakeClip("Failure buzzer", 0.42f, 173f, 0.10f);
        }

        private void OnDestroy()
        {
            Destroy(shot);
            Destroy(pop);
            Destroy(failure);
        }

        public void ResetAim()
        {
            yaw = 0;
            pitch = 0;
            kick = 0;
            flashTime = 0;
            aimCamera.transform.localRotation = Quaternion.identity;
            gun.localPosition = restingPosition;
            gun.localRotation = Quaternion.identity;
            muzzleFlash.SetActive(false);
        }

        public void Aim(float delta)
        {
            yaw = Mathf.Clamp(yaw + Input.GetAxisRaw("Mouse X") * 2.2f, -38f, 38f);
            pitch = Mathf.Clamp(pitch - Input.GetAxisRaw("Mouse Y") * 2.2f, -25f, 25f);
            aimCamera.transform.localRotation = Quaternion.Euler(pitch, yaw, 0);
            Animate(delta);
        }

        public void Animate(float delta)
        {
            kick = Mathf.MoveTowards(kick, 0f, delta * 7.5f);
            flashTime = Mathf.Max(0, flashTime - delta);
            gun.localPosition = restingPosition + new Vector3(0, 0.012f, -0.063f) * kick;
            gun.localRotation = Quaternion.Euler(-8f * kick, 0, 0);
            muzzleFlash.SetActive(flashTime > 0);
        }

        public Balloon Fire()
        {
            kick = 1f;
            flashTime = 0.045f;
            Animate(0);
            audioSource.PlayOneShot(shot, 0.45f);
            Ray ray = new Ray(aimCamera.transform.position, aimCamera.transform.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, 30f, Physics.DefaultRaycastLayers,
                QueryTriggerInteraction.Ignore))
                return hit.collider.GetComponentInParent<Balloon>();
            return null;
        }

        public void PlayPop() => audioSource.PlayOneShot(pop, 0.7f);
        public void PlayFailure() => audioSource.PlayOneShot(failure, 0.75f);

        private static AudioClip MakeClip(string label, float duration, float frequency, float noiseMix)
        {
            const int sampleRate = 22050;
            var samples = new float[Mathf.CeilToInt(sampleRate * duration)];
            var random = new System.Random(41);
            float phase = 0;
            for (int i = 0; i < samples.Length; i++)
            {
                float progress = (float)i / samples.Length;
                phase += 2f * Mathf.PI * frequency * (1f - 0.65f * progress) / sampleRate;
                float tone = Mathf.Sin(phase);
                float noise = (float)random.NextDouble() * 2f - 1f;
                float envelope = Mathf.Min(1f, i / 28f) * Mathf.Pow(1f - progress, 3f);
                samples[i] = Mathf.Lerp(tone, noise, noiseMix) * envelope * 0.6f;
            }
            var clip = AudioClip.Create(label, samples.Length, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }
    }
}
