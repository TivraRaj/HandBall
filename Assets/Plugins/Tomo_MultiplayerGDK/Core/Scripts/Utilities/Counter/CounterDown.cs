
namespace TomoClub.Util
{
	public class CounterDown : Counter
	{

		public CounterDown(int timerEndTime) : base(timerEndTime)
		{
			//Default Constructor
		}

		public CounterDown()
		{

		}

		protected override void AssignCounterStartValue()
		{
			currentTime = counterEndTime;
		}

		protected override void UpdateCounterValue()
		{
			currentTime -= 1;
		}

		protected override bool CheckForCompletion()
		{
			if (currentTime <= 0)
				return true;
			else
				return false;

		}
	} 
}
