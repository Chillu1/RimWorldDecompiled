using System.Text;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class CompRottable : ThingComp
	{
		private float rotProgressInt;

		public CompProperties_Rottable PropsRot => (CompProperties_Rottable)props;

		public float RotProgressPct => RotProgress / (float)PropsRot.TicksToRotStart;

		public float RotProgress
		{
			get
			{
				return rotProgressInt;
			}
			set
			{
				RotStage stage = Stage;
				rotProgressInt = value;
				if (stage != Stage)
				{
					StageChanged();
				}
			}
		}

		public RotStage Stage
		{
			get
			{
				if (RotProgress < (float)PropsRot.TicksToRotStart)
				{
					return RotStage.Fresh;
				}
				if (RotProgress < (float)PropsRot.TicksToDessicated)
				{
					return RotStage.Rotting;
				}
				return RotStage.Dessicated;
			}
		}

		public int TicksUntilRotAtCurrentTemp
		{
			get
			{
				float ambientTemperature = parent.AmbientTemperature;
				ambientTemperature = Mathf.RoundToInt(ambientTemperature);
				return TicksUntilRotAtTemp(ambientTemperature);
			}
		}

		public bool Active
		{
			get
			{
				if (PropsRot.disableIfHatcher)
				{
					CompHatcher compHatcher = parent.TryGetComp<CompHatcher>();
					if (compHatcher != null && !compHatcher.TemperatureDamaged)
					{
						return false;
					}
				}
				return true;
			}
		}

		public override void PostSpawnSetup(bool respawningAfterLoad)
		{
			base.PostSpawnSetup(respawningAfterLoad);
		}

		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Values.Look(ref rotProgressInt, "rotProg", 0f);
		}

		public override void CompTick()
		{
			Tick(1);
		}

		public override void CompTickRare()
		{
			Tick(250);
		}

		private void Tick(int interval)
		{
			if (!Active)
			{
				return;
			}
			float rotProgress = RotProgress;
			float num = GenTemperature.RotRateAtTemperature(parent.AmbientTemperature);
			RotProgress += num * (float)interval;
			if (Stage == RotStage.Rotting && PropsRot.rotDestroys)
			{
				if (parent.IsInAnyStorage() && parent.SpawnedOrAnyParentSpawned)
				{
					Messages.Message("MessageRottedAwayInStorage".Translate(parent.Label, parent).CapitalizeFirst(), new TargetInfo(parent.PositionHeld, parent.MapHeld), MessageTypeDefOf.NegativeEvent);
					LessonAutoActivator.TeachOpportunity(ConceptDefOf.SpoilageAndFreezers, OpportunityType.GoodToKnow);
				}
				parent.Destroy();
			}
			else if (Mathf.FloorToInt(rotProgress / 60000f) != Mathf.FloorToInt(RotProgress / 60000f) && ShouldTakeRotDamage())
			{
				if (Stage == RotStage.Rotting && PropsRot.rotDamagePerDay > 0f)
				{
					parent.TakeDamage(new DamageInfo(DamageDefOf.Rotting, GenMath.RoundRandom(PropsRot.rotDamagePerDay)));
				}
				else if (Stage == RotStage.Dessicated && PropsRot.dessicatedDamagePerDay > 0f)
				{
					parent.TakeDamage(new DamageInfo(DamageDefOf.Rotting, GenMath.RoundRandom(PropsRot.dessicatedDamagePerDay)));
				}
			}
		}

		private bool ShouldTakeRotDamage()
		{
			Thing thing = parent.ParentHolder as Thing;
			if (thing != null && thing.def.category == ThingCategory.Building && thing.def.building.preventDeteriorationInside)
			{
				return false;
			}
			return true;
		}

		public override void PreAbsorbStack(Thing otherStack, int count)
		{
			float t = (float)count / (float)(parent.stackCount + count);
			float rotProgress = ((ThingWithComps)otherStack).GetComp<CompRottable>().RotProgress;
			RotProgress = Mathf.Lerp(RotProgress, rotProgress, t);
		}

		public override void PostSplitOff(Thing piece)
		{
			((ThingWithComps)piece).GetComp<CompRottable>().RotProgress = RotProgress;
		}

		public override void PostIngested(Pawn ingester)
		{
			if (Stage != 0 && FoodUtility.GetFoodPoisonChanceFactor(ingester) > float.Epsilon)
			{
				FoodUtility.AddFoodPoisoningHediff(ingester, parent, FoodPoisonCause.Rotten);
			}
		}

		public override string CompInspectStringExtra()
		{
			if (!Active)
			{
				return null;
			}
			StringBuilder stringBuilder = new StringBuilder();
			switch (Stage)
			{
			case RotStage.Fresh:
				stringBuilder.Append("RotStateFresh".Translate() + ".");
				break;
			case RotStage.Rotting:
				stringBuilder.Append("RotStateRotting".Translate() + ".");
				break;
			case RotStage.Dessicated:
				stringBuilder.Append("RotStateDessicated".Translate() + ".");
				break;
			}
			if ((float)PropsRot.TicksToRotStart - RotProgress > 0f)
			{
				float num = GenTemperature.RotRateAtTemperature(Mathf.RoundToInt(parent.AmbientTemperature));
				int ticksUntilRotAtCurrentTemp = TicksUntilRotAtCurrentTemp;
				stringBuilder.AppendLine();
				if (num < 0.001f)
				{
					stringBuilder.Append("CurrentlyFrozen".Translate() + ".");
				}
				else if (num < 0.999f)
				{
					stringBuilder.Append("CurrentlyRefrigerated".Translate(ticksUntilRotAtCurrentTemp.ToStringTicksToPeriod()) + ".");
				}
				else
				{
					stringBuilder.Append("NotRefrigerated".Translate(ticksUntilRotAtCurrentTemp.ToStringTicksToPeriod()) + ".");
				}
			}
			return stringBuilder.ToString();
		}

		public int ApproxTicksUntilRotWhenAtTempOfTile(int tile, int ticksAbs)
		{
			float temperatureFromSeasonAtTile = GenTemperature.GetTemperatureFromSeasonAtTile(ticksAbs, tile);
			return TicksUntilRotAtTemp(temperatureFromSeasonAtTile);
		}

		public int TicksUntilRotAtTemp(float temp)
		{
			if (!Active)
			{
				return 72000000;
			}
			float num = GenTemperature.RotRateAtTemperature(temp);
			if (num <= 0f)
			{
				return 72000000;
			}
			float num2 = (float)PropsRot.TicksToRotStart - RotProgress;
			if (num2 <= 0f)
			{
				return 0;
			}
			return Mathf.RoundToInt(num2 / num);
		}

		private void StageChanged()
		{
			(parent as Corpse)?.RotStageChanged();
		}

		public void RotImmediately()
		{
			if (RotProgress < (float)PropsRot.TicksToRotStart)
			{
				RotProgress = PropsRot.TicksToRotStart;
			}
		}
	}
}
