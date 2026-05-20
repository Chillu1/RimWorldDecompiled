namespace RimWorld;

public static class StoryDangerUtility
{
	public static float Scale(this StoryDanger d)
	{
		return d switch
		{
			StoryDanger.None => 0f, 
			StoryDanger.Low => 1f, 
			StoryDanger.High => 2f, 
			_ => 0f, 
		};
	}
}
