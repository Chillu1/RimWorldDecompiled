using System;
using Verse;

namespace RimWorld
{
	public class RitualStageAction_DropThing : RitualStageAction
	{
		public ThingDef def;

		public int count = 1;

		public override void Apply(LordJob_Ritual ritual)
		{
			foreach (Pawn participant in ritual.assignments.Participants)
			{
				ApplyToPawn(ritual, participant);
			}
		}

		public override void ApplyToPawn(LordJob_Ritual ritual, Pawn pawn)
		{
			Thing carriedThing = pawn.carryTracker.CarriedThing;
			if (carriedThing == null || carriedThing.def != def || carriedThing.stackCount < count || !pawn.carryTracker.TryDropCarriedThing(pawn.Position, ThingPlaceMode.Near, out var _))
			{
				int num = Math.Min(pawn.inventory.Count(def), count);
				pawn.inventory.DropCount(def, num);
			}
		}

		public override void ExposeData()
		{
			Scribe_Defs.Look(ref def, "def");
			Scribe_Values.Look(ref count, "count", 0);
		}
	}
}
