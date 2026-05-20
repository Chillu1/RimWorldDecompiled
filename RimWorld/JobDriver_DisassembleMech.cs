using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_DisassembleMech : JobDriver
	{
		private const TargetIndex MechInd = TargetIndex.A;

		private const int DisassemblyTime = 300;

		private Pawn Mech => (Pawn)job.GetTarget(TargetIndex.A).Thing;

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return pawn.Reserve(Mech, job, 1, -1, null, errorOnFailed);
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			if (!ModLister.CheckBiotech("Disassemble mech"))
			{
				yield break;
			}
			this.FailOnDestroyedNullOrForbidden(TargetIndex.A);
			this.FailOn(() => Mech.IsFighting());
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
			yield return Toils_General.WaitWith(TargetIndex.A, 300, useProgressBar: true, maintainPosture: true, maintainSleep: false, TargetIndex.A).WithEffect(EffecterDefOf.ControlMech, TargetIndex.A);
			yield return Toils_General.Do(delegate
			{
				foreach (ThingDefCountClass item in MechanitorUtility.IngredientsFromDisassembly(Mech.def))
				{
					Thing thing = ThingMaker.MakeThing(item.thingDef);
					thing.stackCount = item.count;
					GenPlace.TryPlaceThing(thing, Mech.Position, Mech.Map, ThingPlaceMode.Near);
				}
				Mech.forceNoDeathNotification = true;
				Mech.Kill(null, null);
				Mech.forceNoDeathNotification = false;
				Mech.Corpse.Destroy();
			}).WithEffect(EffecterDefOf.ButcherMechanoid, TargetIndex.A).PlaySustainerOrSound(SoundDefOf.Recipe_ButcherCorpseMechanoid);
		}
	}
}
