using Verse;
using Verse.Sound;

namespace RimWorld
{
	public class ActionOnTick_ReleaseLantern : ActionOnTick
	{
		public Pawn pawn;

		public int woodCost = 4;

		public override void Apply(LordJob_Ritual ritual)
		{
			if (ritual.lord.ownedPawns.Contains(pawn) && pawn.carryTracker.CarriedCount(ThingDefOf.WoodLog) >= woodCost)
			{
				pawn.carryTracker.DestroyCarriedThing();
				Thing newThing = ThingMaker.MakeThing(ThingDefOf.SkyLantern);
				IntVec3 intVec = pawn.Position + new IntVec3(-1, 0, 1);
				if (intVec.InBounds(pawn.Map))
				{
					GenSpawn.Spawn(newThing, intVec, pawn.Map);
					ritual.AddTagForPawn(pawn, "LaunchedSkyLantern");
				}
				SoundDefOf.Interact_ReleaseSkylantern.PlayOneShot(new TargetInfo(intVec, pawn.Map));
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_References.Look(ref pawn, "pawn");
			Scribe_Values.Look(ref woodCost, "woodCost", 0);
		}
	}
}
