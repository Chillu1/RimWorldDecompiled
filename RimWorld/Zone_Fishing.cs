using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class Zone_Fishing : Zone
	{
		private bool allowed = true;

		public int repeatCount = 1;

		public int targetCount = 100;

		public float targetPopulationPct = 0.5f;

		public FishRepeatMode repeatMode = FishRepeatMode.TargetCount;

		public bool pauseWhenSatisfied;

		public int unpauseAtCount;

		private bool pausedDueToResourceCount;

		private bool cachedIsFrozen;

		private int cachedFrozenTicks = int.MinValue;

		private int lastPauseCheckTicks = int.MinValue;

		private readonly ITab[] ITabs = new ITab[1]
		{
			new ITab_Fishing()
		};

		private const string FishTexPath = "UI/Designators/ZoneCreate_Fishing";

		private const int FrozenCheckInterval = 60;

		private List<IntVec3> tmpFishableCells = new List<IntVec3>();

		public override bool IsMultiselectable => true;

		protected override Color NextZoneColor => ZoneColorUtility.NextFishingZoneColor();

		public bool Allowed => allowed;

		public bool ShouldFishNow
		{
			get
			{
				if (Allowed && OverTargetPopulation && !PausedDueToResourceCount)
				{
					return UnderTargetResourceCount;
				}
				return false;
			}
		}

		private bool AllFishableCellsFrozenNow => cells.FindIndex(delegate(IntVec3 c)
		{
			TerrainDef terrain = c.GetTerrain(base.Map);
			return terrain == null || !terrain.temporary || terrain != TerrainDefOf.ThinIce;
		}) < 0;

		private bool UnderTargetResourceCount => repeatMode switch
		{
			FishRepeatMode.DoForever => true, 
			FishRepeatMode.TargetCount => OwnedFishCount < targetCount, 
			FishRepeatMode.RepeatCount => repeatCount > 0, 
			_ => OwnedFishCount < repeatCount, 
		};

		public int OwnedFishCount => base.Map.listerThings.ThingsInGroup(ThingRequestGroup.Fish).Sum((Thing t) => t.stackCount) + CarriedFishCount();

		private bool OverTargetPopulation
		{
			get
			{
				if (base.Map.TileInfo.MaxFishPopulation <= 0f || cells.Count == 0 || FishType == WaterBodyType.None)
				{
					return false;
				}
				return base.Map.waterBodyTracker.PopulationPercentAt(base.Cells[0]) > targetPopulationPct;
			}
		}

		public WaterBodyType FishType
		{
			get
			{
				if (cells.Count == 0)
				{
					return WaterBodyType.None;
				}
				return base.Cells[0].GetWaterBodyType(base.Map);
			}
		}

		public bool AllFishableCellsFrozen
		{
			get
			{
				if (Find.TickManager.TicksGame < cachedFrozenTicks + 60)
				{
					return cachedIsFrozen;
				}
				cachedFrozenTicks = Find.TickManager.TicksGame;
				cachedIsFrozen = AllFishableCellsFrozenNow;
				return cachedIsFrozen;
			}
		}

		public bool PausedDueToResourceCount
		{
			get
			{
				if (Find.TickManager.TicksGame < lastPauseCheckTicks + 60)
				{
					return pausedDueToResourceCount;
				}
				RecheckPausedDueToResourceCount();
				return pausedDueToResourceCount;
			}
		}

		public bool HasAnyFishableCells
		{
			get
			{
				for (int i = 0; i < cells.Count; i++)
				{
					if (IsFishable(cells[i]))
					{
						return true;
					}
				}
				return false;
			}
		}

		public List<IntVec3> FishbleCells
		{
			get
			{
				tmpFishableCells.Clear();
				for (int i = 0; i < cells.Count; i++)
				{
					IntVec3 intVec = cells[i];
					if (IsFishable(intVec))
					{
						tmpFishableCells.Add(intVec);
					}
				}
				return tmpFishableCells;
			}
		}

		public IntVec3 RandomFishableCell
		{
			get
			{
				if (FishbleCells.TryRandomElement(out var result))
				{
					return result;
				}
				return IntVec3.Invalid;
			}
		}

		public bool IsFishable(IntVec3 cell)
		{
			if (cell.GetTerrain(base.Map).IsWater)
			{
				return cell.GetWaterBodyType(base.Map) == FishType;
			}
			return false;
		}

		public Zone_Fishing()
		{
		}

		public Zone_Fishing(ZoneManager zoneManager)
			: base("Zone_Fishing".Translate(), zoneManager)
		{
		}

		public override IEnumerable<InspectTabBase> GetInspectTabs()
		{
			return ITabs;
		}

		public override IEnumerable<Gizmo> GetZoneAddGizmos()
		{
			yield return DesignatorUtility.FindAllowedDesignator<Designator_ZoneAdd_Fishing_Expand>();
		}

		private int CarriedFishCount()
		{
			int num = 0;
			foreach (Pawn item in base.Map.mapPawns.FreeColonistsSpawned)
			{
				Thing carriedThing = item.carryTracker.CarriedThing;
				if (carriedThing != null && ThingRequestGroup.Fish.Includes(carriedThing.def) && !carriedThing.Spawned)
				{
					num += carriedThing.stackCount;
				}
			}
			return num;
		}

		public void Notify_Fished()
		{
			if (repeatMode == FishRepeatMode.RepeatCount)
			{
				repeatCount--;
			}
			RecheckPausedDueToResourceCount();
		}

		public void RecheckPausedDueToResourceCount()
		{
			lastPauseCheckTicks = Find.TickManager.TicksGame;
			if (repeatMode != FishRepeatMode.TargetCount || !pauseWhenSatisfied)
			{
				pausedDueToResourceCount = false;
			}
			else if (pausedDueToResourceCount)
			{
				pausedDueToResourceCount = OwnedFishCount > unpauseAtCount;
			}
			else
			{
				pausedDueToResourceCount = OwnedFishCount >= targetCount;
			}
		}

		public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn selPawn)
		{
			foreach (FloatMenuOption floatMenuOption in base.GetFloatMenuOptions(selPawn))
			{
				yield return floatMenuOption;
			}
			if (!ShouldFishNow)
			{
				if (!OverTargetPopulation)
				{
					yield return new FloatMenuOption("CannotFish".Translate().CapitalizeFirst() + ": " + "FishingSpotUnderTargetPopulation".Translate().CapitalizeFirst(), null);
				}
				else if (!Allowed)
				{
					yield return new FloatMenuOption("CannotFish".Translate().CapitalizeFirst() + ": " + "ForbiddenLower".Translate().CapitalizeFirst(), null);
				}
				else if (!UnderTargetResourceCount)
				{
					yield return new FloatMenuOption("CannotFish".Translate().CapitalizeFirst() + ": " + "FishingSpotOverTargetResourceCount".Translate().CapitalizeFirst(), null);
				}
				else if (PausedDueToResourceCount)
				{
					yield return new FloatMenuOption("CannotFish".Translate().CapitalizeFirst() + ": " + "Paused".Translate().CapitalizeFirst(), null);
				}
				yield break;
			}
			if (selPawn.Ideo != null && !new HistoryEvent(HistoryEventDefOf.SlaughteredFish, selPawn.Named(HistoryEventArgsNames.Doer)).Notify_PawnAboutToDo_Job())
			{
				yield return new FloatMenuOption(string.Format("{0}: {1} {2}", "CannotFish".Translate().CapitalizeFirst(), "IdeoligionForbids".Translate(), "Fishing".Translate()), null);
				yield break;
			}
			if (selPawn.WorkTypeIsDisabled(WorkTypeDefOf.Fishing))
			{
				yield return new FloatMenuOption("CannotFish".Translate().CapitalizeFirst() + ": " + "CannotPrioritizeWorkTypeDisabled".Translate(WorkTypeDefOf.Fishing.gerundLabel), null);
				yield break;
			}
			if (!FishbleCells.Any())
			{
				if (AllFishableCellsFrozenNow)
				{
					yield return new FloatMenuOption("CannotFish".Translate().CapitalizeFirst() + ": " + "FishingSpotFrozen".Translate().CapitalizeFirst(), null);
				}
				else
				{
					yield return new FloatMenuOption("CannotFish".Translate().CapitalizeFirst() + ": " + "FishingSpotBlocked".Translate().CapitalizeFirst(), null);
				}
				yield break;
			}
			if (!selPawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
			{
				yield return new FloatMenuOption("CannotFish".Translate().CapitalizeFirst() + ": " + "IncapableOfCapacity".Translate(PawnCapacityDefOf.Manipulation.label).CapitalizeFirst(), null);
				yield break;
			}
			int num = 0;
			bool flag = false;
			while (num < 60)
			{
				IntVec3 cell = RandomFishableCell;
				if (cell.IsValid && cell.Standable(base.Map) && !cell.IsForbidden(selPawn) && selPawn.CanReserve(cell))
				{
					IntVec3 spot = WorkGiver_Fish.BestStandSpotFor(selPawn, cell);
					if (spot.IsValid && (flag || !spot.GetTerrain(base.Map).IsWater))
					{
						yield return new FloatMenuOption("PrioritizeGenericSimple".Translate("Fishing".Translate()), delegate
						{
							selPawn.jobs.TryTakeOrderedJob(JobMaker.MakeJob(JobDefOf.Fish, cell, spot), JobTag.Misc);
						});
						yield break;
					}
				}
				num++;
				if (num >= 60 && !flag)
				{
					flag = true;
					num = 0;
				}
			}
			if (cells.All((IntVec3 c) => c.IsForbidden(selPawn)))
			{
				yield return new FloatMenuOption("CannotFish".Translate().CapitalizeFirst() + ": " + "CannotPrioritizeForbiddenOutsideAllowedArea".Translate().CapitalizeFirst(), null);
			}
		}

		public override IEnumerable<Gizmo> GetGizmos()
		{
			yield return new Command_Hide_ZoneFishing(this);
			foreach (Gizmo gizmo in base.GetGizmos())
			{
				yield return gizmo;
			}
			yield return new Command_Toggle
			{
				defaultLabel = "CommandAllowFish".Translate(),
				defaultDesc = "CommandAllowFishDesc".Translate(),
				icon = TexCommand.ForbidOff,
				hotKey = KeyBindingDefOf.Command_ItemForbid,
				isActive = () => allowed,
				toggleAction = delegate
				{
					allowed = !allowed;
				}
			};
		}

		public override string GetInspectString()
		{
			string inspectString = base.GetInspectString();
			float num = base.Map.waterBodyTracker.FishPopulationAt(base.Cells[0]);
			inspectString += string.Format("\n{0}: {1:F0} ({2})", "FishPopulation".Translate().CapitalizeFirst(), num, FishingUtility.FishPopulationLabel(num));
			if (base.Map.gameConditionManager.FishPopulationOffsetFactorPerDay(base.Map, out var culprit) < 0f && culprit != null)
			{
				inspectString += string.Format("\n{0}: {1}", "FishPopulationDecline".Translate().CapitalizeFirst(), culprit.LabelCap);
			}
			if (!ShouldFishNow)
			{
				if (!UnderTargetResourceCount)
				{
					inspectString += "\n" + "CannotFish".Translate().CapitalizeFirst() + " (" + "FishingSpotOverTargetResourceCount".Translate() + ")";
				}
				else if (PausedDueToResourceCount)
				{
					inspectString += "\n" + "CannotFish".Translate().CapitalizeFirst() + " (" + "Paused".Translate() + ")";
				}
				else if (!OverTargetPopulation)
				{
					inspectString += "\n" + "CannotFish".Translate().CapitalizeFirst() + " (" + "FishingSpotUnderTargetPopulation".Translate() + ")";
				}
				else if (!Allowed)
				{
					inspectString += "\n" + "CannotFish".Translate().CapitalizeFirst() + " (" + "ForbiddenLower".Translate() + ")";
				}
			}
			else if (AllFishableCellsFrozen)
			{
				inspectString += "\n" + "CannotFish".Translate().CapitalizeFirst() + " (" + "FishingSpotFrozen".Translate() + ")";
			}
			else if (!FishbleCells.Any())
			{
				inspectString += "\n" + "CannotFish".Translate().CapitalizeFirst() + " (" + "FishingSpotBlocked".Translate() + ")";
			}
			return inspectString;
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref allowed, "allowed", defaultValue: true);
			Scribe_Values.Look(ref repeatMode, "repeatMode", FishRepeatMode.TargetCount);
			Scribe_Values.Look(ref repeatCount, "repeatCount", 1);
			Scribe_Values.Look(ref targetCount, "targetCount", 100);
			Scribe_Values.Look(ref targetPopulationPct, "targetPopulationPct", 0.5f);
			Scribe_Values.Look(ref pauseWhenSatisfied, "pauseWhenSatisfied", defaultValue: false);
			Scribe_Values.Look(ref unpauseAtCount, "unpauseAtCount", 50);
			Scribe_Values.Look(ref pausedDueToResourceCount, "pausedDueToResourceCount", defaultValue: false);
		}
	}
}
