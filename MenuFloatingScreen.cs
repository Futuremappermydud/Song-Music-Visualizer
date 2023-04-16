using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.ViewControllers;
using HMUI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace SongMusicVisualizer
{
    public class MenuFloatingScreen : BSMLResourceViewController, INotifyPropertyChanged
    {
        public override string ResourceName => string.Join(".", GetType().Namespace, GetType().Name);

        private float _songLength = 141.5f;
        private float _currentSongTime = 283f;

        private float _instantChangeThreshold = 0.5f;

        [UIObject("spectrogram")]
        private GameObject spectrogram;

        private IPreviewBeatmapLevel level;

        internal static AudioSource source;

        private GameObject[] spectrogramObjs = new GameObject[50];

        internal static bool isReady = false;
        private bool didSetSizeThisTime = false;
        private bool madeObjects = false;

        [UIComponent("passive-bar")]
        private ImageView passiveBar = null;

        [UIValue("scrub-pos")]
        private float Scrub => _songLength == 0 ? -30 : Mathf.Lerp(-50f, 50f, _currentSongTime / _songLength);

        [UIValue("time-text")]
        private string TimeText => $"[{string.Format("{0}:{1:00}", (int)(_currentSongTime / 60), _currentSongTime % 60)}-{string.Format("{0}:{1:00}", (int)(_songLength / 60), _songLength % 60)}]";

        private string SongText { get 
            {
                if (level != null) 
                    return level.songAuthorName + " - " + level.songName; 
                else
                    return  "No Selection";
            }
        }

        [UIComponent("song-text")]
        private TextMeshProUGUI songText;

        float[] samples = new float[64];
        float[] samples2 = new float[64];
        float[] processedSamples = new float[64];
        float[] processedSamples2 = new float[64];
        [UIAction("#post-parse")]
        protected void Parsed()
        {
            source = gameObject.AddComponent<AudioSource>();

            //passiveBar.color = Color.gray;
            var group = spectrogram.GetComponent<HorizontalLayoutGroup>();
            group.childControlHeight = false;
            group.childControlWidth = false;
            group.childAlignment = TextAnchor.MiddleCenter;

            //BS_Utils.Utilities.BSEvents.levelSelected += (LevelCollectionViewController _, IPreviewBeatmapLevel lvl) => { level = lvl; songText.text = SongText; };

            var material = Resources.FindObjectsOfTypeAll<Material>().First((Material mat) => { return mat.name == "UINoGlowRoundEdge"; });
            for (int i = 0; i < 50; i++)
            {
                GameObject line = spectrogramObjs[i];
                if (!line)
                {
                    line = new GameObject("line " + i);
                    line.transform.SetParent(spectrogram.transform, false);
                    var img = line.AddComponent<ImageView>();
                    img.SetImage("SongMusicVisualizer.pixel.png");
                    img.useSpriteMesh = true;
                    img.color = Color.gray;
                    img.material = material;
                    spectrogramObjs[i] = line;
                }
                (line.GetComponent<ImageView>().transform as RectTransform).sizeDelta = new Vector2(1f, 1f);
            }
            madeObjects = true;
        }

        void UpdateData(float[] data)
        {
            for (int i = 0; i < 50; i++)
            {
                GameObject line = spectrogramObjs[i];
                (line.GetComponent<ImageView>().transform as RectTransform).sizeDelta = new Vector2(1f, data[i]);
            }
        }

        void Update()
        {
            if(!isReady)
            {
                if(!didSetSizeThisTime)
                {
                    if (madeObjects)
                    {
                        didSetSizeThisTime = true;

                        var list = new List<float>(50);
                            
                        for (int i = 0; i < 50; ++i)
                            list.Add(0.1f);
                        UpdateData(list.ToArray());
                    }
                }
                return;
            }
            source.GetOutputData(samples, 0);
            source.GetSpectrumData(samples2, 0, FFTWindow.Rectangular);
            float deltaTime = Time.deltaTime;
            for (int i = 0; i < samples.Length; i++)
            {
                float num = Mathf.Log((samples[i] + (samples2[i]*35f)) + 1f) * (float)(i + 1);
                if (processedSamples[i] < num)
                {
                    if (num - processedSamples[i] > this._instantChangeThreshold)
                    {
                        processedSamples[i] = num;
                    }
                    else
                    {
                        processedSamples[i] = Mathf.Lerp(processedSamples[i], num, deltaTime * 12f);
                    }
                }
                else
                {
                    processedSamples[i] = Mathf.Lerp(processedSamples[i], num, deltaTime * 6f);
                }
            }
            processedSamples2 = Resample(processedSamples, 50);
            for (int i = 0; i < 50; i++)
            {
                GameObject line = spectrogramObjs[i];
                (line.GetComponent<ImageView>().transform as RectTransform).localScale = new Vector2(1f, Mathf.Max(processedSamples2[i]*12f, 0.1f));
            }
        }
        private float[] Resample(float[] source, int n)
        {
            //n destination length
            int m = source.Length; //source length
            float[] destination = new float[n];
            destination[0] = source[0];
            destination[n - 1] = source[m - 1];

            for (int i = 1; i < n - 1; i++)
            {
                float jd = (i * (m - 1) / (n - 1));
                int j = (int)jd;
                destination[i] = source[j] + (source[j + 1] - source[j]) * (jd - j);
            }
            return destination;
        }
    }

    [HarmonyLib.HarmonyPatch(typeof(SongPreviewPlayer), "CrossfadeTo", new Type[] { typeof(AudioClip), typeof(float), typeof(float), typeof(float), typeof(Action) })]
    class CrossfadePatch
    {
        static void Postfix(SongPreviewPlayer __instance, int ____activeChannel, SongPreviewPlayer.AudioSourceVolumeController[] ____audioSourceControllers, AudioClip audioClip, float musicVolume, float startTime, float duration, Action onFadeOutCallback)
        {
            MenuFloatingScreen.source = ____audioSourceControllers[____activeChannel].audioSource;
            MenuFloatingScreen.isReady = true;
        }
    }
    public static class Extension
    {
        public static double NextDoubleLinear(this System.Random random, double minValue, double maxValue)
        {
            double sample = random.NextDouble();
            return (maxValue * sample) + (minValue * (1d - sample));
        }
    }
}
