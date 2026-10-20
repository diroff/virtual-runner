using System;

namespace Data
{
    [Serializable]
    public class GameData
    {
        public ProgressData Progress = new();
        public SettingsData Settings = new();
    }

    [Serializable]
    public class ProgressData
    {
        public string LastReachedLevelName = string.Empty;
    }

    [Serializable]
    public class SettingsData
    {
        public float MusicVolume = 1f;
        public float SfxVolume = 1f;
    }
}