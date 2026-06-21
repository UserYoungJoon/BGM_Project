using UnityEngine;

namespace YoungJoon.L1.App
{
    // Android OS Vibrator를 JNI로 호출하는 걸 감싼 정적 유틸. 에디터/PC는 no-op.
    public static class Haptic
    {
        private static AndroidJavaObject _vibrator;
        private static AndroidJavaClass _effectClass;
        private static int _sdkInt;
        private static bool _hasVibrator;
        private static bool _init;

        private const string EnabledKey = "haptic";
        private static bool _enabled = PlayerPrefs.GetInt(EnabledKey, 1) == 1;
        public static bool Enabled => _enabled;

        public static void SetEnabled(bool on)
        {
            _enabled = on;
            PlayerPrefs.SetInt(EnabledKey, on ? 1 : 0);
        }

        private static bool IsAndroid => Application.platform == RuntimePlatform.Android;

        private static void EnsureInit()
        {
            if (_init) return;
            _init = true;
            try
            {
                using (var player = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                using (var activity = player.GetStatic<AndroidJavaObject>("currentActivity"))
                    _vibrator = activity.Call<AndroidJavaObject>("getSystemService", "vibrator");

                using (var ver = new AndroidJavaClass("android.os.Build$VERSION"))
                    _sdkInt = ver.GetStatic<int>("SDK_INT");

                if (_sdkInt >= 26)
                    _effectClass = new AndroidJavaClass("android.os.VibrationEffect");

                _hasVibrator = _vibrator != null && _vibrator.Call<bool>("hasVibrator");
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("[Haptic] init 실패: " + e.Message);
            }
        }

        // milliseconds: 진동 길이, amplitude: 1~255 세기(-1=기본). amplitude는 API 26+에서만 적용.
        public static void Vibrate(long milliseconds, int amplitude = -1)
        {
            if (!IsAndroid || !_enabled) return;
            EnsureInit();
            if (!_hasVibrator)
            {
                // 네이티브 못 쓰면 폴백. + 이 참조로 Unity가 VIBRATE 권한을 자동으로 매니페스트에 추가함.
                Handheld.Vibrate();
                return;
            }

            try
            {
                if (_sdkInt >= 26 && _effectClass != null)
                {
                    int amp = amplitude < 0 ? -1 : Mathf.Clamp(amplitude, 1, 255);
                    using (var fx = _effectClass.CallStatic<AndroidJavaObject>("createOneShot", milliseconds, amp))
                        _vibrator.Call("vibrate", fx);
                }
                else
                {
                    _vibrator.Call("vibrate", milliseconds);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("[Haptic] vibrate 실패: " + e.Message);
            }
        }

        public static void Light() => Vibrate(20, 90);
        public static void Medium() => Vibrate(35, 160);
        public static void Heavy() => Vibrate(55, 255);
    }
}
