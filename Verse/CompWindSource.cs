namespace Verse
{
	public class CompWindSource : ThingComp
	{
		public float wind;

		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Values.Look(ref wind, "wind", 0f);
		}
	}
}
