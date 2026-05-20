using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld
{
	public class LordToilData_AssaultColonyBreaching : LordToilData
	{
		public IntVec3 breachDest = IntVec3.Invalid;

		public IntVec3 breachStart = IntVec3.Invalid;

		public bool preferMelee;

		public BreachingGrid breachingGrid;

		public Thing currentTarget;

		public Pawn soloAttacker;

		public float maxRange = 12f;

		public LordToilData_AssaultColonyBreaching()
		{
		}

		public LordToilData_AssaultColonyBreaching(Lord lord)
		{
			breachingGrid = new BreachingGrid(lord.Map, lord);
		}

		public void Reset()
		{
			breachDest = IntVec3.Invalid;
			breachStart = IntVec3.Invalid;
			currentTarget = null;
			soloAttacker = null;
			breachingGrid.Reset();
		}

		public override void ExposeData()
		{
			Scribe_Values.Look(ref breachDest, "breachDest");
			Scribe_Values.Look(ref breachStart, "breachStart");
			Scribe_Values.Look(ref preferMelee, "preferMelee", defaultValue: false);
			Scribe_Deep.Look(ref breachingGrid, "breachingGrid");
			Scribe_References.Look(ref currentTarget, "currentTarget");
			Scribe_References.Look(ref soloAttacker, "soloAttacker");
			Scribe_Values.Look(ref maxRange, "maxRange", 0f);
		}
	}
}
