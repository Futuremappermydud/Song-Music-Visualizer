using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace SongMusicVisualizer.Installers
{
    class MenuInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            new GameObject("SongMusicVisualizerController").AddComponent<SongMusicVisualizerController>();
        }
    }
}
