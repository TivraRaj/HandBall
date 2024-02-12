using System;
using System.Collections.Generic;
using System.Linq;

namespace TomoClub.Util
{
	public static class Utilities
	{
		public static string CovertTimeToString(int time)
		{
			TimeSpan converted = TimeSpan.FromSeconds(time);
			return converted.ToString("mm':'ss");

		}

		public static List<T> ScrambleList<T>(this List<T> listToScramble)
		{
			List<T> tempList = new List<T>();

			//Copy data to the temp list
			for (int i = 0; i < listToScramble.Count; i++)
			{
				tempList.Add(listToScramble[i]);
			}

			var rnd = new Random();
			var randomized = tempList.OrderBy(item => rnd.Next()); //Randomize and return a linq

			return randomized.ToList();
		}
	} 
}
