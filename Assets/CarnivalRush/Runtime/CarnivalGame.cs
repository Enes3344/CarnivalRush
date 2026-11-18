using UnityEngine;

namespace CarnivalRush
{
    public sealed class CarnivalGame : MonoBehaviour
    {
        private enum State { Ready, Playing, Paused, Cleared, GameOver }

        [SerializeField] private ToyShooter shooter;
        [SerializeField] private BalloonWall wall;
        [SerializeField] private CarnivalHud hud;
        private const string HighScoreKey = "CarnivalRush.HighScore";
        private const string BestLevelKey = "CarnivalRush.HighestCompletedLevel";
        private State state;
        private State beforePause;
        private int level;
        private int score;
        private int remaining;
        private int highestCompleted;
        private int highScore;
        private int bestLevel;
        private float timeLeft;
        private float clearDelay;
        private float failureFlash;
        private int inputFrame;
        private float countdownSpeed;

        public void Configure(ToyShooter player, BalloonWall targets, CarnivalHud canvas)
        {
            shooter = player;
            wall = targets;
            hud = canvas;
        }

        private void Start()
        {
            highScore = PlayerPrefs.GetInt(HighScoreKey);
            bestLevel = PlayerPrefs.GetInt(BestLevelKey);
            hud.Primary.onClick.AddListener(PrimaryAction);
            hud.Restart.onClick.AddListener(Restart);
            level = 1;
            PrepareLevel();
            state = State.Ready;
            hud.ShowMenu("Carnival Rush", "Pop every colored balloon before time runs out.\nAvoid black balloons marked with an X.", "Start game", false);
            SetCursor(false);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (state == State.Paused)
                    Resume();
                else if (state == State.Playing || state == State.Cleared)
                    Pause();
                return;
            }

            float delta = Time.unscaledDeltaTime;
            if (state == State.Paused || state == State.Ready)
                return;

            if ((state == State.Playing || state == State.Cleared)
                && Time.frameCount > inputFrame + 1 && Cursor.lockState != CursorLockMode.Locked)
            {
                Pause();
                return;
            }

            wall.Tick(delta);
            failureFlash = Mathf.Max(0f, failureFlash - delta * 2.6f);
            hud.SetFlash(failureFlash);
            if (state == State.GameOver)
            {
                shooter.Animate(delta);
                return;
            }

            if (state == State.Cleared)
            {
                shooter.Aim(delta);
                clearDelay -= delta;
                if (clearDelay <= 0f)
                {
                    if (level < int.MaxValue)
                        level++;
                    PrepareLevel();
                    BeginPlay();
                }
                return;
            }

            // Keep the countdown on its own clock. Resolve expiry before same-frame shots.
            timeLeft = Mathf.Max(0, timeLeft - delta * countdownSpeed);
            UpdateHud();
            if (timeLeft <= 0f)
            {
                Fail("Time ran out.");
                return;
            }

            if (Time.frameCount <= inputFrame)
                return;
            shooter.Aim(delta);
            if (Cursor.lockState != CursorLockMode.Locked || !Input.GetMouseButtonDown(0))
                return;

            Balloon target = shooter.Fire();
            if (target == null || target.Popped)
                return;
            target.Pop();
            wall.Burst(target);
            if (target.Dangerous)
            {
                Fail("You hit a black balloon.");
                return;
            }

            shooter.PlayPop();
            remaining--;
            AddPoints(10);
            if (remaining == 0)
                CompleteLevel();
            UpdateHud();
        }

        private void OnApplicationFocus(bool focused)
        {
            if (!focused && (state == State.Playing || state == State.Cleared))
                Pause();
        }

        private void PrimaryAction()
        {
            if (state == State.Ready)
                BeginPlay();
            else if (state == State.Paused)
                Resume();
            else if (state == State.GameOver)
                Restart();
        }

        private void Restart()
        {
            level = 1;
            score = 0;
            highestCompleted = 0;
            failureFlash = 0;
            hud.SetFlash(0);
            wall.SetEffectsPaused(false);
            PrepareLevel();
            BeginPlay();
        }

        private void PrepareLevel()
        {
            shooter.ResetAim();
            remaining = RoundRules.SafeCount(level);
            timeLeft = RoundRules.Seconds(level);
            countdownSpeed = RoundRules.CountdownSpeed(level);
            wall.Populate(level);
            UpdateHud();
        }

        private void BeginPlay()
        {
            state = State.Playing;
            hud.HideMenu();
            hud.SetBanner(string.Empty);
            SetCursor(true);
            inputFrame = Time.frameCount;
        }

        private void CompleteLevel()
        {
            state = State.Cleared;
            clearDelay = 0.75f;
            highestCompleted = level;
            AddPoints(100);
            if (highestCompleted > bestLevel)
            {
                bestLevel = highestCompleted;
                PlayerPrefs.SetInt(BestLevelKey, bestLevel);
            }
            PlayerPrefs.Save();
            hud.SetBanner("LEVEL CLEARED   +100");
        }

        private void Pause()
        {
            beforePause = state;
            state = State.Paused;
            wall.SetEffectsPaused(true);
            hud.ShowMenu("Paused", "The round will continue when you resume.", "Resume", true);
            SetCursor(false);
        }

        private void Resume()
        {
            state = beforePause;
            wall.SetEffectsPaused(false);
            hud.HideMenu();
            SetCursor(true);
            inputFrame = Time.frameCount;
        }

        private void Fail(string reason)
        {
            state = State.GameOver;
            shooter.PlayFailure();
            failureFlash = 0.65f;
            hud.SetFlash(failureFlash);
            PlayerPrefs.Save();
            hud.ShowMenu("Game Over", $"{reason}\nScore  {score:N0}   •   Highest completed  {highestCompleted}", "Restart", false);
            SetCursor(false);
        }

        private void AddPoints(int points)
        {
            score += Mathf.Min(points, int.MaxValue - score);
            if (score > highScore)
            {
                highScore = score;
                PlayerPrefs.SetInt(HighScoreKey, highScore);
                PlayerPrefs.Save();
            }
        }

        private void UpdateHud() =>
            hud.UpdateReadouts(score, highScore, level, bestLevel, remaining, timeLeft, countdownSpeed);

        private static void SetCursor(bool captured)
        {
            Cursor.lockState = captured ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !captured;
        }
    }
}
