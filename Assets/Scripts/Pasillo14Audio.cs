using UnityEngine;

namespace Pasillo14
{
    /// <summary>Builds original looping music and menu cues from synthesized notes at runtime.</summary>
    public sealed class Pasillo14Audio : MonoBehaviour
    {
        public enum Theme { Menu, Gameplay, Pursuit, Caught }

        private const int SampleRate = 22050;
        private AudioSource musicSource = null!;
        private AudioSource uiSource = null!;
        private AudioClip menuClip = null!;
        private AudioClip gameplayClip = null!;
        private AudioClip pursuitClip = null!;
        private AudioClip caughtClip = null!;
        private AudioClip uiClip = null!;
        private Theme currentTheme;
        private float musicVolume = 0.65f;
        private float generalVolume = 0.8f;

        private void Awake()
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.spatialBlend = 0f;
            musicSource.playOnAwake = false;
            uiSource = gameObject.AddComponent<AudioSource>();
            uiSource.spatialBlend = 0f;
            uiSource.playOnAwake = false;

            menuClip = Compose("P14_Menu_Original", 96,
                new[] { 72, 76, 79, 76, 74, 77, 81, 77, 67, 72, 76, 72, 69, 72, 76, 72 },
                new[] { 48, 53, 45, 52 }, false);
            gameplayClip = Compose("P14_Gameplay_Original", 104,
                new[] { 69, 72, 76, 72, 67, 71, 74, 71, 65, 69, 72, 69, 67, 71, 74, 71 },
                new[] { 45, 43, 41, 43 }, false);
            pursuitClip = Compose("P14_Pursuit_Original", 118,
                new[] { 69, 68, 69, 76, 69, 68, 65, 68, 69, 68, 72, 76, 69, 68, 65, 64 },
                new[] { 38, 39, 38, 34 }, true);
            caughtClip = Compose("P14_GameOver_Original", 76,
                new[] { 69, 65, 62, 57, 53, -1, -1, -1, 69, 65, 62, 57, 53, -1, -1, -1 },
                new[] { 41, 38, 36, 33 }, true);
            uiClip = Compose("P14_UI_Cue", 150,
                new[] { 84, 91, -1, -1, 88, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
                new[] { -1, -1, -1, -1 }, false, 1);
        }

        public void SetTheme(Theme theme)
        {
            if (musicSource == null || (musicSource.isPlaying && currentTheme == theme))
                return;
            currentTheme = theme;
            musicSource.clip = theme switch
            {
                Theme.Menu => menuClip,
                Theme.Gameplay => gameplayClip,
                Theme.Pursuit => pursuitClip,
                _ => caughtClip
            };
            musicSource.volume = generalVolume * musicVolume;
            musicSource.Play();
        }

        public void SetVolumes(float music, float general)
        {
            musicVolume = Mathf.Clamp01(music);
            generalVolume = Mathf.Clamp01(general);
            if (musicSource != null) musicSource.volume = musicVolume * generalVolume;
            if (uiSource != null) uiSource.volume = generalVolume;
        }

        public void PlayUiSound()
        {
            if (uiSource != null && uiClip != null)
                uiSource.PlayOneShot(uiClip);
        }

        private static AudioClip Compose(string clipName, int bpm, int[] melody, int[] bass, bool tense, int bars = 4)
        {
            int beats = bars * 4;
            float beatSeconds = 60f / bpm;
            int sampleCount = Mathf.CeilToInt(beats * beatSeconds * SampleRate);
            var samples = new float[sampleCount];
            for (int beat = 0; beat < beats; beat++)
            {
                int note = melody[beat % melody.Length];
                if (note >= 0)
                    AddNote(samples, beat * beatSeconds, beatSeconds * (tense ? 0.72f : 0.84f), note, tense ? 0.19f : 0.14f, tense);

                int bassNote = bass[beat / 4 % bass.Length];
                if (bassNote >= 0 && beat % 2 == 0)
                    AddNote(samples, beat * beatSeconds, beatSeconds * 1.65f, bassNote, tense ? 0.16f : 0.11f, tense);

                if (!tense && beat % 4 == 2 && note >= 0)
                    AddNote(samples, beat * beatSeconds + beatSeconds * 0.5f, beatSeconds * 0.34f,
                        note + 12, 0.055f, false);
            }

            float peak = 0f;
            for (int i = 0; i < samples.Length; i++) peak = Mathf.Max(peak, Mathf.Abs(samples[i]));
            if (peak > 0.92f)
                for (int i = 0; i < samples.Length; i++) samples[i] *= 0.92f / peak;
            var clip = AudioClip.Create(clipName, sampleCount, 1, SampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private static void AddNote(float[] target, float startSeconds, float durationSeconds, int midiNote, float amplitude, bool tense)
        {
            int start = Mathf.Max(0, Mathf.FloorToInt(startSeconds * SampleRate));
            int length = Mathf.Min(target.Length - start, Mathf.CeilToInt(durationSeconds * SampleRate));
            float frequency = 440f * Mathf.Pow(2f, (midiNote - 69) / 12f);
            float attack = tense ? 0.012f : 0.025f;
            float release = tense ? 0.09f : 0.16f;
            for (int i = 0; i < length; i++)
            {
                float t = i / (float)SampleRate;
                float envelope = Mathf.Min(1f, t / attack) * Mathf.Min(1f, (durationSeconds - t) / release);
                envelope = Mathf.Max(0f, envelope) * Mathf.Exp(-t * (tense ? 1.5f : 2.8f));
                float phase = 2f * Mathf.PI * frequency * t;
                float tone = Mathf.Sin(phase) + 0.22f * Mathf.Sin(phase * 2f) + 0.07f * Mathf.Sin(phase * 3f);
                if (tense)
                    tone += 0.12f * Mathf.Sin(phase * 1.014f);
                target[start + i] += tone * envelope * amplitude;
            }
        }

        private void OnDestroy()
        {
            if (menuClip != null) Destroy(menuClip);
            if (gameplayClip != null) Destroy(gameplayClip);
            if (pursuitClip != null) Destroy(pursuitClip);
            if (caughtClip != null) Destroy(caughtClip);
            if (uiClip != null) Destroy(uiClip);
        }
    }
}
