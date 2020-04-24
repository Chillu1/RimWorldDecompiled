using RimWorld;

namespace Verse
{
	public class HediffCompProperties_RecoveryThought : HediffCompProperties
	{
		public ThoughtDef thought;

		public HediffCompProperties_RecoveryThought()
		{
			compClass = typeof(HediffComp_RecoveryThought);
		}
	}
}
