namespace Verse
{
	public static class UnityDataInitializer
	{
		public static bool initializing;

		public static void CopyUnityData()
		{
			initializing = true;
			try
			{
				UnityData.CopyUnityData();
			}
			finally
			{
				initializing = false;
			}
		}
	}
}
