using System;


namespace TomoClub.Core
{
	public static class UserEvents
	{
		public static Action<int, int, int> UpdatePlayerReadyUp;
	}

	public static class UtilEvents
	{
		public static Action SetAndStartTimer;
		public static Action<string> ShowToastMessage;
		public static Action OnKickOutOver;
	}

}

