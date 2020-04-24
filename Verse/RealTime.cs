using System.Threading;
using UnityEngine;

namespace Verse
{
	public static class RealTime
	{
		public static float deltaTime;

		public static float realDeltaTime;

		public static RealtimeMoteList moteList = new RealtimeMoteList();

		public static int frameCount;

		private static float lastRealTime = 0f;

		public static float LastRealTime => lastRealTime;

		public static void Update()
		{
			frameCount = Time.frameCount;
			deltaTime = Time.deltaTime;
			float realtimeSinceStartup = Time.realtimeSinceStartup;
			realDeltaTime = realtimeSinceStartup - lastRealTime;
			lastRealTime = realtimeSinceStartup;
			if (Current.ProgramState == ProgramState.Playing)
			{
				moteList.MoteListUpdate();
			}
			else
			{
				moteList.Clear();
			}
			if (DebugSettings.lowFPS && Time.deltaTime < 100f)
			{
				Thread.Sleep((int)(100f - Time.deltaTime));
			}
		}
	}
}
