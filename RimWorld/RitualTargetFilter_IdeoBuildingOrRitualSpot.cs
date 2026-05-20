using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class RitualTargetFilter_IdeoBuildingOrRitualSpot : RitualTargetFilter_IdeoBuilding
	{
		public RitualTargetFilter_IdeoBuildingOrRitualSpot()
		{
		}

		public RitualTargetFilter_IdeoBuildingOrRitualSpot(RitualTargetFilterDef def)
			: base(def)
		{
		}

		protected override IEnumerable<Thing> ExtraCandidates(TargetInfo initiator)
		{
			Pawn pawn = (Pawn)initiator.Thing;
			return from s in initiator.Map.listerThings.ThingsOfDef(ThingDefOf.RitualSpot)
				where pawn.CanReach(s, PathEndMode.Touch, pawn.NormalMaxDanger())
				select s;
		}

		public override IEnumerable<string> GetTargetInfos(TargetInfo initiator)
		{
			foreach (string targetInfo in base.GetTargetInfos(initiator))
			{
				yield return targetInfo;
			}
			yield return ThingDefOf.RitualSpot.LabelCap;
		}
	}
}
