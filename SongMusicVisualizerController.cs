using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.FloatingScreen;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SongMusicVisualizer
{
    public class SongMusicVisualizerController : MonoBehaviour
    {
        public static SongMusicVisualizerController Instance { get; private set; }
        public FloatingScreen screen;

        private void Awake()
        {
            if (Instance != null)
            {
                GameObject.DestroyImmediate(this);
                return;
            }
            GameObject.DontDestroyOnLoad(this);
            Instance = this;
        }
        private void Start()
        {
            screen = FloatingScreen.CreateFloatingScreen(new Vector2(135f, 75f), false, new Vector3(0f, 0.05f, 1f), Quaternion.Euler(90f, 0f, 0f), 0f);
            //screen.HandleSide = FloatingScreen.Side.Top;
            //screen.HighlightHandle = true;
            screen.SetRootViewController(BeatSaberUI.CreateViewController<MenuFloatingScreen>(), HMUI.ViewController.AnimationType.In);
        }

        private void Update()
        {

        }

        private void OnDestroy()
        {
            Plugin.Log?.Debug($"{name}: OnDestroy()");
            if (Instance == this)
                Instance = null;

        }
    }
}
