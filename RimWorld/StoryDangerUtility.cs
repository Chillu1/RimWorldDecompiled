namespace RimWorld
{
	public static class StoryDangerUtility
	{
		public static float Scale(this StoryDanger d)
		{
			switch (d)
			{
			case StoryDanger.None:
				return 0f;
			case StoryDanger.Low:
				return 1f;
			case StoryDanger.High:
				return 2f;
			default:
				return 0f;
			}
		}
	}
}
