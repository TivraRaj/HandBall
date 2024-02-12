using UnityEngine;
using System;

namespace TomoClub.Util
{
	public abstract class Counter
	{
		protected int currentTime; //in seconds
		protected int counterEndTime; // in seconds

		protected float perFrameTime;//timer per frame (for frame rate independence)

		private bool hasStarted = false;// to check for timer start
		private bool isPaused = false;// to check for timer pause

		public Action<int> CounterUpdatePerSecond; //to update per second activity for that timer
		public Action CounterCompleted; //to update when timer is completed

		/// <summary>
		/// Sets the timer with value of timerEndTime
		/// </summary>
		public Counter(int timerEndTime)
		{
			this.counterEndTime = timerEndTime;
			AssignCounterStartValue();
		}

		/// <summary>
		/// Sets the timer with default value of 5 seconds
		/// </summary>
		public Counter()
		{
			counterEndTime = 5;
			AssignCounterStartValue();
		}


		//Update Timer should run per frame
		public void UpdateCounter()
		{
			if (!hasStarted || isPaused) return;

			CounterCount();

			if (CheckForCompletion())
			{
				hasStarted = false;
				AssignCounterStartValue();
				CounterCompleted?.Invoke();
			}

		}

		private void CounterCount()
		{
			perFrameTime += Time.deltaTime;

			if (perFrameTime >= 1f)
			{
				perFrameTime = 0f;
				UpdateCounterValue();
				CounterUpdatePerSecond?.Invoke(currentTime);
			}
		}

		protected abstract void UpdateCounterValue();

		protected abstract bool CheckForCompletion();


		public void PauseCounter()
		{
			if (hasStarted) isPaused = true;
		}

		public void PlayCounter()
		{
			if (hasStarted) isPaused = false;
		}


		public void ResetCounter()
		{
			hasStarted = false;
			AssignCounterStartValue();
		}

		/// <summary>
		/// Resets from a different point and start the timer
		/// </summary>
		public void SetAndStartCounter(int counterResetFrom)
		{
			//Stop Timer
			hasStarted = false;
			SetCounter(counterResetFrom);
			StartCounter();
		}

		public void SetCounter(int counterResetFrom)
		{
			counterEndTime = counterResetFrom;
			AssignCounterStartValue();
		}

		public void RestartCounter()
		{
			AssignCounterStartValue();
			StartCounter();
		}

		public void StartCounter() => hasStarted = true;


		protected abstract void AssignCounterStartValue();

		public bool IsRunning()
		{
			return hasStarted && !isPaused;
		}


	} 
}
