using UnityEngine;
using Verse;

namespace RimWorld
{
	public class CompEggLayer : ThingComp
	{
		private float eggProgress;

		private int fertilizationCount;

		private Pawn fertilizedBy;

		private bool Active
		{
			get
			{
				Pawn pawn = parent as Pawn;
				if (Props.eggLayFemaleOnly && pawn != null && pawn.gender != Gender.Female)
				{
					return false;
				}
				if (pawn != null && !pawn.ageTracker.CurLifeStage.milkable)
				{
					return false;
				}
				return true;
			}
		}

		public bool CanLayNow
		{
			get
			{
				if (!Active)
				{
					return false;
				}
				return eggProgress >= 1f;
			}
		}

		public bool FullyFertilized => fertilizationCount >= Props.eggFertilizationCountMax;

		private bool ProgressStoppedBecauseUnfertilized
		{
			get
			{
				if (Props.eggProgressUnfertilizedMax < 1f && fertilizationCount == 0)
				{
					return eggProgress >= Props.eggProgressUnfertilizedMax;
				}
				return false;
			}
		}

		public CompProperties_EggLayer Props => (CompProperties_EggLayer)props;

		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Values.Look(ref eggProgress, "eggProgress", 0f);
			Scribe_Values.Look(ref fertilizationCount, "fertilizationCount", 0);
			Scribe_References.Look(ref fertilizedBy, "fertilizedBy");
		}

		public override void CompTick()
		{
			if (Active)
			{
				float num = 1f / (Props.eggLayIntervalDays * 60000f);
				Pawn pawn = parent as Pawn;
				if (pawn != null)
				{
					num *= PawnUtility.BodyResourceGrowthSpeed(pawn);
				}
				eggProgress += num;
				if (eggProgress > 1f)
				{
					eggProgress = 1f;
				}
				if (ProgressStoppedBecauseUnfertilized)
				{
					eggProgress = Props.eggProgressUnfertilizedMax;
				}
			}
		}

		public void Fertilize(Pawn male)
		{
			fertilizationCount = Props.eggFertilizationCountMax;
			fertilizedBy = male;
		}

		public virtual Thing ProduceEgg()
		{
			if (!Active)
			{
				Log.Error("LayEgg while not Active: " + parent);
			}
			eggProgress = 0f;
			int randomInRange = Props.eggCountRange.RandomInRange;
			if (randomInRange == 0)
			{
				return null;
			}
			Thing thing;
			if (fertilizationCount > 0)
			{
				thing = ThingMaker.MakeThing(Props.eggFertilizedDef);
				fertilizationCount = Mathf.Max(0, fertilizationCount - randomInRange);
			}
			else
			{
				thing = ThingMaker.MakeThing(Props.eggUnfertilizedDef);
			}
			thing.stackCount = randomInRange;
			CompHatcher compHatcher = thing.TryGetComp<CompHatcher>();
			if (compHatcher != null)
			{
				compHatcher.hatcheeFaction = parent.Faction;
				Pawn pawn = parent as Pawn;
				if (pawn != null)
				{
					compHatcher.hatcheeParent = pawn;
				}
				if (fertilizedBy != null)
				{
					compHatcher.otherParent = fertilizedBy;
				}
			}
			return thing;
		}

		public override string CompInspectStringExtra()
		{
			if (!Active)
			{
				return null;
			}
			string text = "EggProgress".Translate() + ": " + eggProgress.ToStringPercent();
			if (fertilizationCount > 0)
			{
				text += "\n" + "Fertilized".Translate();
			}
			else if (ProgressStoppedBecauseUnfertilized)
			{
				text += "\n" + "ProgressStoppedUntilFertilized".Translate();
			}
			return text;
		}
	}
}
