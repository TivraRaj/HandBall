

namespace TomoClub.Util
{
	public class CounterUp : Counter
	{
		public CounterUp(int timerEndTime) : base(timerEndTime)
		{
			//Default Constructor
		}

		public CounterUp()
		{

		}

		protected override void AssignCounterStartValue()
		{
			currentTime = 0;

		}

		protected override void UpdateCounterValue()
		{
			currentTime += 1;
		}

		protected override bool CheckForCompletion()
		{
			return (currentTime >= counterEndTime);

		}

	} 
}
