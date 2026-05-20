using UnityEngine;

namespace Verse
{
	public static class OriginalEventUtility
	{
		private static EventType? originalType;

		public static EventType EventType => originalType ?? Event.current.rawType;

		public static void RecordOriginalEvent(Event e)
		{
			originalType = e.type;
		}

		public static void Reset()
		{
			originalType = null;
		}
	}
}
