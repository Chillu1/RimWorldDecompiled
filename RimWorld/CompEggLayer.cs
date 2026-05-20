using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

public class CompEggLayer : ThingComp
{
	private const int EggTickInterval = 2500;

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
			if (pawn.Sterile())
			{
				return false;
			}
			if (ModsConfig.AnomalyActive && pawn.IsShambler)
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
			if (Active)
			{
				return eggProgress >= 1f;
			}
			return false;
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
		if (parent.IsHashIntervalTick(2500) && Active)
		{
			float num = 2500f / (Props.eggLayIntervalDays * 60000f);
			if (parent is Pawn pawn)
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

	public ThingDef NextEggType()
	{
		if (fertilizationCount > 0)
		{
			return Props.eggFertilizedDef;
		}
		return Props.eggUnfertilizedDef;
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
		Thing thing = null;
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
			if (parent is Pawn hatcheeParent)
			{
				compHatcher.hatcheeParent = hatcheeParent;
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

	public override IEnumerable<Gizmo> CompGetGizmosExtra()
	{
		if (DebugSettings.ShowDevGizmos)
		{
			yield return new Command_Action
			{
				defaultLabel = "DEV: LayEgg",
				action = delegate
				{
					eggProgress = 1f;
				}
			};
		}
	}
}
