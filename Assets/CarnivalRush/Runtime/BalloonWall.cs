using System.Collections.Generic;
using UnityEngine;

namespace CarnivalRush
{
    public sealed class BalloonWall : MonoBehaviour
    {
        [SerializeField] private Mesh sphere;
        [SerializeField] private Mesh cube;
        [SerializeField] private Material[] skins;
        [SerializeField] private Material dangerMark;
        [SerializeField] private Material stringMaterial;
        [SerializeField] private Material confetti;
        private readonly List<Balloon> balloons = new List<Balloon>();
        private readonly List<ParticleSystem> bursts = new List<ParticleSystem>();
        private readonly System.Random random = new System.Random();

        public void Configure(Mesh sphereMesh, Mesh cubeMesh, Material[] palette, Material mark,
            Material stringSkin, Material particleSkin)
        {
            sphere = sphereMesh;
            cube = cubeMesh;
            skins = palette;
            dangerMark = mark;
            stringMaterial = stringSkin;
            confetti = particleSkin;
        }

        public void Populate(int level)
        {
            Clear();
            int safe = RoundRules.SafeCount(level);
            int total = safe + RoundRules.BlackCount(level);
            int[] cells = RoundRules.ShuffledCells(random);
            int colorOffset = random.Next(skins.Length - 1);
            for (int i = 0; i < total; i++)
            {
                var (x, y) = RoundRules.Position(cells[i], random);
                bool dangerous = i >= safe;
                int skin = dangerous ? skins.Length - 1 : (i + colorOffset) % (skins.Length - 1);
                CreateBalloon(new Vector3(x, y, 0), skin, dangerous);
            }
            Physics.SyncTransforms();
        }

        public void Tick(float delta)
        {
            for (int i = balloons.Count - 1; i >= 0; i--)
            {
                if (!balloons[i].AnimatePop(delta))
                    continue;
                Destroy(balloons[i].gameObject);
                balloons.RemoveAt(i);
            }
            for (int i = bursts.Count - 1; i >= 0; i--)
            {
                if (!bursts[i].IsAlive())
                {
                    Destroy(bursts[i].gameObject);
                    bursts.RemoveAt(i);
                }
            }
        }

        public void SetEffectsPaused(bool paused)
        {
            foreach (ParticleSystem burst in bursts)
            {
                if (paused)
                    burst.Pause();
                else if (burst.IsPaused)
                    burst.Play();
            }
        }

        public void Burst(Balloon balloon)
        {
            var effect = new GameObject("Balloon confetti");
            effect.transform.SetParent(transform, false);
            effect.transform.position = balloon.transform.position + Vector3.back * 0.20f;
            var particles = effect.AddComponent<ParticleSystem>();
            particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = particles.main;
            main.playOnAwake = false;
            main.loop = false;
            main.duration = 0.4f;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.23f, 0.48f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.75f, 2.1f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.035f, 0.094f);
            main.startColor = new ParticleSystem.MinMaxGradient(balloon.Tint,
                balloon.Dangerous ? new Color(1f, 0.22f, 0.14f) : Color.white);
            main.gravityModifier = 0.65f;
            var emission = particles.emission;
            emission.enabled = false;
            var shape = particles.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.09f;
            effect.GetComponent<ParticleSystemRenderer>().sharedMaterial = confetti;
            particles.Play();
            particles.Emit(balloon.Dangerous ? 40 : 24);
            bursts.Add(particles);
        }

        private void CreateBalloon(Vector3 position, int skin, bool dangerous)
        {
            var root = new GameObject(dangerous ? "Danger balloon" : "Safe balloon");
            root.transform.SetParent(transform, false);
            root.transform.localPosition = position;

            var shell = Visual(sphere, "Balloon shell", root.transform, Vector3.zero,
                new Vector3(RoundRules.BalloonWidth, RoundRules.BalloonHeight, 0.62f), skins[skin]);
            shell.gameObject.AddComponent<MeshCollider>().sharedMesh = shell.GetComponent<MeshFilter>().sharedMesh;
            Visual(sphere, "Knot", root.transform, new Vector3(0, -0.322f, 0),
                new Vector3(0.10f, 0.073f, 0.085f), skins[skin]);
            Visual(cube, "String", root.transform, new Vector3(0, -0.366f, 0),
                new Vector3(0.018f, 0.049f, 0.018f), stringMaterial);

            if (dangerous)
            {
                for (int i = -1; i <= 1; i += 2)
                {
                    var line = Visual(cube, "Danger X", root.transform,
                        new Vector3(0, 0, -0.303f), new Vector3(0.31f, 0.051f, 0.035f), dangerMark);
                    line.localRotation = Quaternion.Euler(0, 0, i * 45f);
                }
            }
            var balloon = root.AddComponent<Balloon>();
            balloon.Configure(dangerous, skins[skin].color);
            balloons.Add(balloon);
        }

        private static Transform Visual(Mesh mesh, string label, Transform parent,
            Vector3 position, Vector3 scale, Material skin)
        {
            var part = new GameObject(label, typeof(MeshFilter), typeof(MeshRenderer));
            part.GetComponent<MeshFilter>().sharedMesh = mesh;
            part.transform.SetParent(parent, false);
            part.transform.localPosition = position;
            part.transform.localScale = scale;
            part.GetComponent<Renderer>().sharedMaterial = skin;
            return part.transform;
        }

        private void Clear()
        {
            foreach (Balloon balloon in balloons)
            {
                balloon.gameObject.SetActive(false);
                Destroy(balloon.gameObject);
            }
            foreach (ParticleSystem burst in bursts)
                Destroy(burst.gameObject);
            balloons.Clear();
            bursts.Clear();
        }
    }
}
