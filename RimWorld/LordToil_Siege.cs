using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld
{
	public class LordToil_Siege : LordToil
	{
		public Dictionary<Pawn, DutyDef> rememberedDuties = new Dictionary<Pawn, DutyDef>();

		private const float BaseRadiusMin = 14f;

		private const float BaseRadiusMax = 25f;

		private static readonly FloatRange MealCountRangePerRaider = new FloatRange(1f, 3f);

		private const int StartBuildingDelay = 450;

		private static readonly FloatRange BuilderCountFraction = new FloatRange(0.25f, 0.4f);

		private const float FractionLossesToAssault = 0.4f;

		private const int InitalShellsPerCannon = 5;

		private const int ReplenishAtShells = 4;

		private const int ShellReplenishCount = 10;

		private const int ReplenishAtMeals = 5;

		private const int MealReplenishCount = 12;

		public override IntVec3 FlagLoc => Data.siegeCenter;

		private LordToilData_Siege Data => (LordToilData_Siege)data;

		private IEnumerable<Frame> Frames
		{
			get
			{
				LordToilData_Siege data = Data;
				float radSquared = (data.baseRadius + 10f) * (data.baseRadius + 10f);
				List<Thing> framesList = base.Map.listerThings.ThingsInGroup(ThingRequestGroup.BuildingFrame);
				if (framesList.Count == 0)
				{
					yield break;
				}
				for (int i = 0; i < framesList.Count; i++)
				{
					Frame frame = (Frame)framesList[i];
					if (frame.Faction == lord.faction && (float)(frame.Position - data.siegeCenter).LengthHorizontalSquared < radSquared)
					{
						yield return frame;
					}
				}
			}
		}

		public override bool ForceHighStoryDanger => true;

		public LordToil_Siege(IntVec3 siegeCenter, float blueprintPoints)
		{
			data = new LordToilData_Siege();
			Data.siegeCenter = siegeCenter;
			Data.blueprintPoints = blueprintPoints;
		}

		public override void Init()
		{
			base.Init();
			LordToilData_Siege data = Data;
			data.baseRadius = Mathf.InverseLerp(14f, 25f, (float)lord.ownedPawns.Count / 50f);
			data.baseRadius = Mathf.Clamp(data.baseRadius, 14f, 25f);
			List<Thing> list = new List<Thing>();
			foreach (Blueprint_Build item2 in SiegeBlueprintPlacer.PlaceBlueprints(data.siegeCenter, base.Map, lord.faction, data.blueprintPoints))
			{
				data.blueprints.Add(item2);
				foreach (ThingDefCountClass cost in item2.MaterialsNeeded())
				{
					Thing thing = list.FirstOrDefault((Thing t) => t.def == cost.thingDef);
					if (thing != null)
					{
						thing.stackCount += cost.count;
					}
					else
					{
						Thing thing2 = ThingMaker.MakeThing(cost.thingDef);
						thing2.stackCount = cost.count;
						list.Add(thing2);
					}
				}
				ThingDef thingDef = item2.def.entityDefToBuild as ThingDef;
				if (thingDef != null)
				{
					ThingDef thingDef2 = TurretGunUtility.TryFindRandomShellDef(thingDef, allowEMP: false, mustHarmHealth: true, lord.faction.def.techLevel, allowAntigrainWarhead: false, 250f);
					if (thingDef2 != null)
					{
						Thing thing3 = ThingMaker.MakeThing(thingDef2);
						thing3.stackCount = 5;
						list.Add(thing3);
					}
				}
			}
			for (int i = 0; i < list.Count; i++)
			{
				list[i].stackCount = Mathf.CeilToInt((float)list[i].stackCount * Rand.Range(1f, 1.2f));
			}
			List<List<Thing>> list2 = new List<List<Thing>>();
			for (int j = 0; j < list.Count; j++)
			{
				while (list[j].stackCount > list[j].def.stackLimit)
				{
					int num = Mathf.CeilToInt((float)list[j].def.stackLimit * Rand.Range(0.9f, 0.999f));
					Thing thing4 = ThingMaker.MakeThing(list[j].def);
					thing4.stackCount = num;
					list[j].stackCount -= num;
					list.Add(thing4);
				}
			}
			List<Thing> list3 = new List<Thing>();
			for (int k = 0; k < list.Count; k++)
			{
				list3.Add(list[k]);
				if (k % 2 == 1 || k == list.Count - 1)
				{
					list2.Add(list3);
					list3 = new List<Thing>();
				}
			}
			List<Thing> list4 = new List<Thing>();
			int num2 = Mathf.RoundToInt(MealCountRangePerRaider.RandomInRange * (float)lord.ownedPawns.Count);
			for (int l = 0; l < num2; l++)
			{
				Thing item = ThingMaker.MakeThing(ThingDefOf.MealSurvivalPack);
				list4.Add(item);
			}
			list2.Add(list4);
			DropPodUtility.DropThingGroupsNear(data.siegeCenter, base.Map, list2);
			data.desiredBuilderFraction = BuilderCountFraction.RandomInRange;
		}

		public override void UpdateAllDuties()
		{
			LordToilData_Siege data = Data;
			if (lord.ticksInToil < 450)
			{
				for (int i = 0; i < lord.ownedPawns.Count; i++)
				{
					SetAsDefender(lord.ownedPawns[i]);
				}
				return;
			}
			rememberedDuties.Clear();
			int num = Mathf.RoundToInt((float)lord.ownedPawns.Count * data.desiredBuilderFraction);
			if (num <= 0)
			{
				num = 1;
			}
			int num2 = (from b in base.Map.listerThings.ThingsInGroup(ThingRequestGroup.BuildingArtificial)
				where b.def.hasInteractionCell && b.Faction == lord.faction && b.Position.InHorDistOf(FlagLoc, data.baseRadius)
				select b).Count();
			if (num < num2)
			{
				num = num2;
			}
			int num3 = 0;
			for (int j = 0; j < lord.ownedPawns.Count; j++)
			{
				Pawn pawn = lord.ownedPawns[j];
				if (pawn.mindState.duty.def == DutyDefOf.Build)
				{
					rememberedDuties.Add(pawn, DutyDefOf.Build);
					SetAsBuilder(pawn);
					num3++;
				}
			}
			int num4 = num - num3;
			for (int k = 0; k < num4; k++)
			{
				if (lord.ownedPawns.Where((Pawn pa) => !rememberedDuties.ContainsKey(pa) && CanBeBuilder(pa)).TryRandomElement(out Pawn result))
				{
					rememberedDuties.Add(result, DutyDefOf.Build);
					SetAsBuilder(result);
					num3++;
				}
			}
			for (int l = 0; l < lord.ownedPawns.Count; l++)
			{
				Pawn pawn2 = lord.ownedPawns[l];
				if (!rememberedDuties.ContainsKey(pawn2))
				{
					SetAsDefender(pawn2);
					rememberedDuties.Add(pawn2, DutyDefOf.Defend);
				}
			}
			if (num3 == 0)
			{
				lord.ReceiveMemo("NoBuilders");
			}
		}

		public override void Notify_PawnLost(Pawn victim, PawnLostCondition cond)
		{
			UpdateAllDuties();
			base.Notify_PawnLost(victim, cond);
		}

		public override void Notify_ConstructionFailed(Pawn pawn, Frame frame, Blueprint_Build newBlueprint)
		{
			base.Notify_ConstructionFailed(pawn, frame, newBlueprint);
			if (frame.Faction == lord.faction && newBlueprint != null)
			{
				Data.blueprints.Add(newBlueprint);
			}
		}

		private bool CanBeBuilder(Pawn p)
		{
			if (p.WorkTypeIsDisabled(WorkTypeDefOf.Construction) || p.WorkTypeIsDisabled(WorkTypeDefOf.Firefighter))
			{
				return false;
			}
			return true;
		}

		private void SetAsBuilder(Pawn p)
		{
			LordToilData_Siege data = Data;
			p.mindState.duty = new PawnDuty(DutyDefOf.Build, data.siegeCenter);
			p.mindState.duty.radius = data.baseRadius;
			int minLevel = Mathf.Max(ThingDefOf.Sandbags.constructionSkillPrerequisite, ThingDefOf.Turret_Mortar.constructionSkillPrerequisite);
			p.skills.GetSkill(SkillDefOf.Construction).EnsureMinLevelWithMargin(minLevel);
			p.workSettings.EnableAndInitialize();
			List<WorkTypeDef> allDefsListForReading = DefDatabase<WorkTypeDef>.AllDefsListForReading;
			for (int i = 0; i < allDefsListForReading.Count; i++)
			{
				WorkTypeDef workTypeDef = allDefsListForReading[i];
				if (workTypeDef == WorkTypeDefOf.Construction)
				{
					p.workSettings.SetPriority(workTypeDef, 1);
				}
				else
				{
					p.workSettings.Disable(workTypeDef);
				}
			}
		}

		private void SetAsDefender(Pawn p)
		{
			LordToilData_Siege data = Data;
			p.mindState.duty = new PawnDuty(DutyDefOf.Defend, data.siegeCenter);
			p.mindState.duty.radius = data.baseRadius;
		}

		public override void LordToilTick()
		{
			base.LordToilTick();
			LordToilData_Siege data = Data;
			if (lord.ticksInToil == 450)
			{
				lord.CurLordToil.UpdateAllDuties();
			}
			if (lord.ticksInToil > 450 && lord.ticksInToil % 500 == 0)
			{
				UpdateAllDuties();
			}
			if (Find.TickManager.TicksGame % 500 != 0)
			{
				return;
			}
			if (!Frames.Where((Frame frame) => !frame.Destroyed).Any() && !data.blueprints.Where((Blueprint blue) => !blue.Destroyed).Any() && !base.Map.listerThings.ThingsInGroup(ThingRequestGroup.BuildingArtificial).Any((Thing b) => b.Faction == lord.faction && b.def.building.buildingTags.Contains("Artillery")))
			{
				lord.ReceiveMemo("NoArtillery");
				return;
			}
			int num = GenRadial.NumCellsInRadius(20f);
			int num2 = 0;
			int num3 = 0;
			for (int i = 0; i < num; i++)
			{
				IntVec3 c = data.siegeCenter + GenRadial.RadialPattern[i];
				if (!c.InBounds(base.Map))
				{
					continue;
				}
				List<Thing> thingList = c.GetThingList(base.Map);
				for (int j = 0; j < thingList.Count; j++)
				{
					if (thingList[j].def.IsShell)
					{
						num2 += thingList[j].stackCount;
					}
					if (thingList[j].def == ThingDefOf.MealSurvivalPack)
					{
						num3 += thingList[j].stackCount;
					}
				}
			}
			if (num2 < 4)
			{
				ThingDef thingDef = TurretGunUtility.TryFindRandomShellDef(ThingDefOf.Turret_Mortar, allowEMP: false, mustHarmHealth: true, lord.faction.def.techLevel, allowAntigrainWarhead: false, 250f);
				if (thingDef != null)
				{
					DropSupplies(thingDef, 10);
				}
			}
			if (num3 < 5)
			{
				DropSupplies(ThingDefOf.MealSurvivalPack, 12);
			}
		}

		private void DropSupplies(ThingDef thingDef, int count)
		{
			List<Thing> list = new List<Thing>();
			Thing thing = ThingMaker.MakeThing(thingDef);
			thing.stackCount = count;
			list.Add(thing);
			DropPodUtility.DropThingsNear(Data.siegeCenter, base.Map, list);
		}

		public override void Cleanup()
		{
			LordToilData_Siege data = Data;
			data.blueprints.RemoveAll((Blueprint blue) => blue.Destroyed);
			for (int i = 0; i < data.blueprints.Count; i++)
			{
				data.blueprints[i].Destroy(DestroyMode.Cancel);
			}
			foreach (Frame item in Frames.ToList())
			{
				item.Destroy(DestroyMode.Cancel);
			}
		}
	}
}
