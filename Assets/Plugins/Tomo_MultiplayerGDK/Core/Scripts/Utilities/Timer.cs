using System;
using UnityEngine;

namespace TomoClub.Util
{
    public class Timer
    {
        private float currentTime = 0f;
        private float endTime;

        private float m_updateIntervalTime = 0.1f;
        private float updateIntervalTime
        {
            get => m_updateIntervalTime;
            set
            {
                m_updateIntervalTime = Mathf.Clamp(value, 0.1f, endTime);
            }
        }

        private float currentDeltaTime = 0f;

        private bool started = false;
        private bool autoRestart = false;

        public Action<float> OnTimerUpdate;
        public Action OnTimerComplete;

        public Timer(float endTime, float updateIntervalTime, bool autoRestart)
        {
            this.endTime = endTime;
            this.updateIntervalTime = updateIntervalTime;
            this.autoRestart = autoRestart;
        }

        public Timer(float endTime, float updateIntervalTime)
        {
            this.endTime = endTime;
            this.updateIntervalTime = updateIntervalTime;
        }

        public Timer(float endTime)
        {
            this.endTime = endTime;
        }

        public void StartTimer()
        {
            started = true;
        }

        public void ToggleTimer(bool pause)
        {
            started = !pause;
        }

        public void ResetTimer()
        {
            currentDeltaTime = 0f;
            currentTime = 0f;
        }

        public void RestartTimer()
        {
            ResetTimer();
            StartTimer();
        }

        public void RestartTimer(float endTime)
        {
            this.endTime = endTime;
            RestartTimer();
        }

        public void RestartTimer(float endTime, float updateIntervalTime)
        {

            this.endTime = endTime;
            this.updateIntervalTime = updateIntervalTime;
            RestartTimer();
        }

        public void RestartTimer(float endTime, float updateIntervalTime, bool autoRestart)
        {
            this.endTime = endTime;
            this.updateIntervalTime = updateIntervalTime;
            this.autoRestart = autoRestart;
            RestartTimer();

        }

        public void TickTimer()
        {
            if (!started) return;

            currentDeltaTime += Time.deltaTime;
            if(currentDeltaTime >= updateIntervalTime)
            {
                currentTime += updateIntervalTime;
                currentDeltaTime = 0f;
                OnTimerUpdate?.Invoke(currentTime);
            }

            if(currentTime >= endTime)
            {
                started = false;
                OnTimerComplete?.Invoke();
                if (autoRestart) RestartTimer();
            }
        }

    }
}
