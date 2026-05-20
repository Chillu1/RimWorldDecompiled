using Verse;

namespace RimWorld
{
	public class Thought_Counselled : Thought_Memory
	{
		public float moodOffsetOverride;

		public override int DurationTicks => durationTicksOverride;

		public override float MoodOffset()
		{
			return moodOffsetOverride;
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref moodOffsetOverride, "moodOffsetOverride", 0f);
		}
	}
}
