namespace Verse
{
	public class HediffCompProperties_MessageOnRemoval : HediffCompProperties_MessageBase
	{
		public bool messageOnZeroSeverity = true;

		public bool messageOnNonZeroSeverity = true;

		public HediffCompProperties_MessageOnRemoval()
		{
			compClass = typeof(HediffComp_MessageOnRemoval);
		}
	}
}
