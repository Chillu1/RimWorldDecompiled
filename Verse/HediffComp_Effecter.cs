namespace Verse
{
	public class HediffComp_Effecter : HediffComp
	{
		public HediffCompProperties_Effecter Props => (HediffCompProperties_Effecter)props;

		public EffecterDef CurrentStateEffecter()
		{
			if (parent.CurStageIndex >= Props.severityIndices.min && (Props.severityIndices.max < 0 || parent.CurStageIndex <= Props.severityIndices.max))
			{
				return Props.stateEffecter;
			}
			return null;
		}
	}
}
