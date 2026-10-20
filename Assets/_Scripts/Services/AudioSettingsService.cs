using Abstractions.Interfaces;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Audio;

namespace Services
{
    public class AudioSettingsService : IService
    {
        private readonly AudioMixer _audioMixer;
        private readonly SaveLoadService _saveLoadService;

        private const string MusicParam = "Music";
        private const string SfxParam = "SFX";

        public float MusicVolume => _saveLoadService.Data.Settings.MusicVolume;
        public float SfxVolume => _saveLoadService.Data.Settings.SfxVolume;

        public AudioSettingsService(AudioMixer audioMixer, SaveLoadService saveLoadService)
        {
            _audioMixer = audioMixer;
            _saveLoadService = saveLoadService;
        }

        public async UniTask Initialize()
        {
            await UniTask.WaitForEndOfFrame();

            ApplySavedSettings();

            await UniTask.WaitForEndOfFrame();
        }

        public void SetMusicVolume(float volume)
        {
            _saveLoadService.Data.Settings.MusicVolume = volume;
            ApplyVolume(MusicParam, volume);
            _saveLoadService.SaveAsync().Forget();
        }

        public async UniTask SetSfxVolume(float volume)
        {
            _saveLoadService.Data.Settings.SfxVolume = volume;
            ApplyVolume(SfxParam, volume);
            await _saveLoadService.SaveAsync();
        }

        private void ApplySavedSettings()
        {
            ApplyVolume(MusicParam, _saveLoadService.Data.Settings.MusicVolume);
            ApplyVolume(SfxParam, _saveLoadService.Data.Settings.SfxVolume);
        }

        private void ApplyVolume(string parameterName, float linearVolume)
        {
            float db = linearVolume > 0.0001f ? Mathf.Log10(linearVolume) * 20f : -80f;
            _audioMixer.SetFloat(parameterName, db);
        }
    }
}