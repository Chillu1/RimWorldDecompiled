using System.Collections.Generic;
using System.Linq;
using LudeonTK;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class CompGrowsFleshmassTendrils : ThingComp
	{
		private static readonly IntRange GrowthIntervalTicksRange = new IntRange(5, 15);

		private static readonly IntRange BranchDist = new IntRange(20, 50);

		[TweakValue("Fleshmass Heart", 0f, 1f)]
		private static float GrowCoreChance = 0.1f;

		[TweakValue("Fleshmass Heart", 0f, 1f)]
		private static float ThickenTendrilChance = 0.6f;

		[TweakValue("Fleshmass Heart", 0f, 300f)]
		private static int GrowthsBeforeThickening = 100;

		[TweakValue("Fleshmass Heart", 0f, 20f)]
		private static int CoreRadius = 15;

		[TweakValue("Fleshmass Heart", 0f, 10f)]
		private static int MinTendrils = 3;

		private List<FleshTendril> tendrils = new List<FleshTendril>();

		protected int growthPoints;

		private int nextGrowth = -99999;

		private int fleshmassUntilFleshbeastBirth = -1;

		protected List<IntVec3> contiguousFleshmass = new List<IntVec3>();

		private float furthestGrownSquared;

		private bool fleshMassWasDestroyed;

		private bool drawDebug;

		public CompProperties_GrowsFleshmassTendrils Props => (CompProperties_GrowsFleshmassTendrils)props;

		protected Map Map => parent.Map;

		private IntVec3 Position => parent.Position;

		public int GrowthPoints => growthPoints;

		public IEnumerable<IntVec3> ContiguousFleshmass => contiguousFleshmass;

		public override void PostExposeData()
		{
			Scribe_Collections.Look(ref tendrils, "tendrils", LookMode.Deep);
			Scribe_Values.Look(ref growthPoints, "growthPoints", 0);
			Scribe_Values.Look(ref nextGrowth, "nextGrowth", 0);
			Scribe_Values.Look(ref fleshmassUntilFleshbeastBirth, "fleshmassUntilFleshbeastBirth", 0);
		}

		public override void PostSpawnSetup(bool respawningAfterLoad)
		{
			if (!respawningAfterLoad && !parent.BeingTransportedOnGravship)
			{
				growthPoints = Mathf.RoundToInt(Props.startingGrowthPointsByThreat.Evaluate(StorytellerUtility.DefaultThreatPointsNow(Map)));
				foreach (IntVec3 cell in parent.OccupiedRect().Cells)
				{
					CreateTendril(cell);
				}
				if (Props.fleshbeastBirthThresholdRange.HasValue)
				{
					fleshmassUntilFleshbeastBirth = Props.fleshbeastBirthThresholdRange.Value.RandomInRange;
				}
			}
			else
			{
				RecalculateContiguousFleshmass();
			}
		}

		public override void CompTick()
		{
			base.CompTick();
			if (growthPoints > 0 && Find.TickManager.TicksGame >= nextGrowth)
			{
				Grow();
				growthPoints--;
				nextGrowth = Find.TickManager.TicksGame + GrowthIntervalTicksRange.RandomInRange;
			}
			if (drawDebug)
			{
				DrawDebug();
			}
		}

		protected virtual void Grow()
		{
			if (fleshMassWasDestroyed)
			{
				CheckForDetachedTendrils();
				fleshMassWasDestroyed = false;
			}
			IntVec3 intVec = IntVec3.Invalid;
			int num = 0;
			while (intVec == IntVec3.Invalid && num < 10)
			{
				intVec = FindValidGrowthPoint();
				num++;
			}
			if (!(intVec == IntVec3.Invalid))
			{
				Thing thing = GenSpawn.Spawn(ThingDefOf.Fleshmass_Active, intVec, Map, WipeMode.FullRefund);
				thing.SetFaction(parent.Faction);
				thing.TryGetComp<CompFleshmass>().source = parent;
				contiguousFleshmass.Add(intVec);
				furthestGrownSquared = Mathf.Max(furthestGrownSquared, intVec.DistanceToSquared(Position));
				EffecterDefOf.MeatExplosionTiny.Spawn(intVec, Map).Cleanup();
			}
		}

		private IntVec3 FindValidGrowthPoint()
		{
			float value = Rand.Value;
			if (value < GrowCoreChance)
			{
				return GrowCore();
			}
			if (value < GrowCoreChance + ThickenTendrilChance && contiguousFleshmass.Count > GrowthsBeforeThickening)
			{
				return ThickenTendril();
			}
			return GrowTendril();
		}

		private IntVec3 GrowCore()
		{
			IntVec3 startCell = parent.OccupiedRect().Cells.RandomElement();
			IntVec3 curPos = startCell;
			int num = 0;
			while (curPos.InHorDistOf(startCell, CoreRadius) && num < 1000)
			{
				GenAdj.CardinalDirections.TryRandomElementByWeight(delegate(IntVec3 d)
				{
					int num2 = startCell.DistanceToSquared(curPos);
					return Mathf.Max(startCell.DistanceToSquared(curPos + d) - num2, 0);
				}, out var result);
				curPos += result;
				if (AdjacentFleshmass(curPos) == 0)
				{
					return IntVec3.Invalid;
				}
				if (CanPlaceFleshmass(curPos))
				{
					return curPos;
				}
				num++;
			}
			return IntVec3.Invalid;
		}

		private IntVec3 GrowTendril()
		{
			if (tendrils.Count < MinTendrils)
			{
				for (int i = 0; i < MinTendrils - tendrils.Count; i++)
				{
					if (contiguousFleshmass.Count == 0)
					{
						CreateTendril(parent.OccupiedRect().RandomCell);
					}
					else
					{
						CreateTendril(contiguousFleshmass.RandomElement());
					}
				}
			}
			FleshTendril tendril = tendrils.RandomElement();
			if (GenAdj.CardinalDirections.TryRandomElementByWeight(delegate(IntVec3 dir)
			{
				if (!CanPlaceFleshmass(tendril.currentPos + dir))
				{
					return 0f;
				}
				return (tendril.sourceNode.DistanceToSquared(tendril.currentPos + dir) <= tendril.sourceNode.DistanceToSquared(tendril.currentPos)) ? 0f : Mathf.Max(Position.DistanceTo(tendril.currentPos + dir) - Position.DistanceTo(tendril.currentPos), 0.5f);
			}, out var result))
			{
				IntVec3 intVec = tendril.currentPos + result * 2;
				if (!intVec.InBounds(Map) || CellContainsFleshmass(intVec))
				{
					tendrils.Remove(tendril);
					return IntVec3.Invalid;
				}
				tendril.currentPos += result;
				tendril.length++;
				if (tendril.length > BranchDist.RandomInRange)
				{
					CreateBranch(tendril);
				}
				return tendril.currentPos;
			}
			tendrils.Remove(tendril);
			return IntVec3.Invalid;
		}

		private IntVec3 ThickenTendril()
		{
			contiguousFleshmass.Where((IntVec3 cell) => AdjacentFleshmass(cell) < 4).TryRandomElementByWeight((IntVec3 cell) => furthestGrownSquared - (float)Position.DistanceToSquared(cell), out var cellToThicken);
			if (GenAdj.AdjacentCells.TryRandomElementByWeight(delegate(IntVec3 dir)
			{
				IntVec3 cell = cellToThicken + dir;
				int num = AdjacentFleshmass(cell);
				if (!CanPlaceFleshmass(cell))
				{
					return 0f;
				}
				return CellContainsFleshmass(cell) ? 0f : (num switch
				{
					1 => 1f, 
					2 => 10f, 
					3 => 100f, 
					4 => 1000f, 
					_ => 0f, 
				});
			}, out var result))
			{
				return cellToThicken + result;
			}
			return IntVec3.Invalid;
		}

		private void CreateBranch(FleshTendril tendril)
		{
			tendrils.Remove(tendril);
			CreateTendril(tendril.currentPos);
			CreateTendril(tendril.currentPos);
		}

		private void CreateTendril(IntVec3 pos)
		{
			tendrils.Add(new FleshTendril
			{
				sourceNode = pos,
				currentPos = pos
			});
		}

		public override void PostSwapMap()
		{
			CheckForDetachedTendrils();
		}

		private void RecalculateContiguousFleshmass()
		{
			furthestGrownSquared = 0f;
			contiguousFleshmass.Clear();
			Map.floodFiller.FloodFill(Position, CellContainsFleshmass, delegate(IntVec3 pos)
			{
				contiguousFleshmass.Add(pos);
			});
			furthestGrownSquared = contiguousFleshmass.Max((IntVec3 cell) => cell.DistanceToSquared(Position));
		}

		private bool CellContainsFleshmass(IntVec3 cell)
		{
			Building edifice = cell.GetEdifice(Map);
			if (edifice == null)
			{
				return false;
			}
			if (edifice.def != ThingDefOf.Fleshmass_Active && edifice.def != ThingDefOf.FleshmassSpitter && edifice.def != ThingDefOf.NerveBundle)
			{
				return edifice.HasComp<CompGrowsFleshmassTendrils>();
			}
			return true;
		}

		private bool CanPlaceFleshmass(IntVec3 cell)
		{
			if (!cell.InBounds(Map))
			{
				return false;
			}
			if (!cell.SupportsStructureType(Map, ThingDefOf.Fleshmass_Active.terrainAffordanceNeeded))
			{
				return false;
			}
			foreach (Thing thing in cell.GetThingList(Map))
			{
				if (!(thing is Building_Door) && !(thing is Building { IsClearableFreeBuilding: not false }) && !(thing is Blueprint) && thing.def?.building?.isPowerConduit != true && thing.def?.building?.isAttachment != true)
				{
					if (!GenConstruct.CanPlaceBlueprintOver(ThingDefOf.Fleshmass_Active, thing.def))
					{
						return false;
					}
					ThingDef def = thing.def;
					if (def != null && def.preventSkyfallersLandingOn)
					{
						return false;
					}
					if (thing is Pawn pawn && pawn.RaceProps.Humanlike)
					{
						return false;
					}
				}
			}
			return true;
		}

		protected int AdjacentFleshmass(IntVec3 cell)
		{
			int num = 0;
			for (int i = 0; i < GenAdj.CardinalDirections.Length; i++)
			{
				IntVec3 intVec = cell + GenAdj.CardinalDirections[i];
				if (intVec.InBounds(Map) && CellContainsFleshmass(intVec))
				{
					num++;
				}
			}
			return num;
		}

		public void Notify_FleshmassDestroyed(Thing mass)
		{
			fleshMassWasDestroyed = true;
		}

		public void Notify_FleshmassDestroyedByPlayer(Thing mass)
		{
			if (fleshmassUntilFleshbeastBirth > 0)
			{
				fleshmassUntilFleshbeastBirth--;
			}
			if (fleshmassUntilFleshbeastBirth == 0)
			{
				FleshbeastUtility.DoFleshbeastResponse(this, mass.Position);
				fleshmassUntilFleshbeastBirth = Props.fleshbeastBirthThresholdRange.Value.RandomInRange;
			}
		}

		public void CheckForDetachedTendrils()
		{
			RecalculateContiguousFleshmass();
			tendrils.RemoveAll((FleshTendril tendril) => !contiguousFleshmass.Contains(tendril.currentPos));
			if (contiguousFleshmass.Count >= 10)
			{
				return;
			}
			foreach (IntVec3 cell in parent.OccupiedRect().Cells)
			{
				CreateTendril(cell);
			}
		}

		private void DrawDebug()
		{
			if (fleshMassWasDestroyed)
			{
				CheckForDetachedTendrils();
				fleshMassWasDestroyed = false;
			}
			foreach (FleshTendril tendril in tendrils)
			{
				GenDraw.DrawFieldEdges(new List<IntVec3> { tendril.currentPos }, Color.green);
			}
		}

		public override string CompInspectStringExtra()
		{
			if (DebugSettings.godMode)
			{
				return "DEV: Fleshmass until fleshbeast birth: " + fleshmassUntilFleshbeastBirth;
			}
			return "";
		}

		public override IEnumerable<Gizmo> CompGetGizmosExtra()
		{
			if (DebugSettings.ShowDevGizmos)
			{
				yield return new Command_Toggle
				{
					defaultLabel = "DEV: View tendril growth debug",
					defaultDesc = "View tendril growth debug",
					isActive = () => drawDebug,
					toggleAction = delegate
					{
						drawDebug = !drawDebug;
					}
				};
			}
		}
	}
}
