using TMPro;
using UnityEngine;
using UnityEngine.UI;
using YoungJoon.L1.App;
using YoungJoon.L1.Sound;

namespace YoungJoon.L1.UI
{
    public class OptionPopup : MonoBehaviour
    {
        [SerializeField] private GameObject _curtain;        // 켜고 끄는 본체(검은 커튼+내용)
        [SerializeField] private Button _openBtn;            // 설정
        [SerializeField] private Button _closeBtn;           // 닫기 (설정 버튼과 같은 자리)
        [SerializeField] private Slider _volumeSlider;
        [SerializeField] private TextMeshProUGUI _volumeValue;
        [SerializeField] private Toggle _vibrationToggle;

        private void Start()
        {
            _openBtn.onClick.AddListener(Open);
            _closeBtn.onClick.AddListener(Close);
            _volumeSlider.onValueChanged.AddListener(OnVolume);
            _vibrationToggle.onValueChanged.AddListener(OnVibration);
            _curtain.SetActive(false);
        }

        private void Open()
        {
            SoundManager.Instance.Play(SoundKey.Click);
            _volumeSlider.SetValueWithoutNotify(SoundManager.Instance.Volume);
            UpdateVolumeText(SoundManager.Instance.Volume);
            _vibrationToggle.SetIsOnWithoutNotify(Haptic.Enabled);
            _curtain.SetActive(true);
        }

        private void Close()
        {
            SoundManager.Instance.Play(SoundKey.Click);
            _curtain.SetActive(false);
        }

        private void OnVolume(float v)
        {
            SoundManager.Instance.SetVolume(v);
            UpdateVolumeText(v);
        }

        private void OnVibration(bool on)
        {
            Haptic.SetEnabled(on);
            if (on) Haptic.Light();
        }

        private void UpdateVolumeText(float v) => _volumeValue.text = Mathf.RoundToInt(v * 100f) + "%";
    }
}
