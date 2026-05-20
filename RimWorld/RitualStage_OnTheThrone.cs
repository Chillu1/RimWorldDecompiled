using Verse;

namespace RimWorld
{
	public class RitualStage_OnTheThrone : RitualStage
	{
		public override TargetInfo GetSecondFocus(LordJob_Ritual ritual)
		{
			return ritual.selectedTarget.Cell.GetFirstThing<Building_Throne>(ritual.Map);
		}
	}
}
