using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class ScenPart_PlayerPawnsArriveMethod : ScenPart
	{
		private PlayerPawnsArriveMethod method;

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref method, "method", PlayerPawnsArriveMethod.Standing);
		}

		public override void DoEditInterface(Listing_ScenEdit listing)
		{
			if (Widgets.ButtonText(listing.GetScenPartRect(this, ScenPart.RowHeight), method.ToStringHuman()))
			{
				List<FloatMenuOption> list = new List<FloatMenuOption>();
				foreach (PlayerPawnsArriveMethod value in Enum.GetValues(typeof(PlayerPawnsArriveMethod)))
				{
					PlayerPawnsArriveMethod localM = value;
					list.Add(new FloatMenuOption(localM.ToStringHuman(), delegate
					{
						method = localM;
					}));
				}
				Find.WindowStack.Add(new FloatMenu(list));
			}
		}

		public override string Summary(Scenario scen)
		{
			if (method == PlayerPawnsArriveMethod.DropPods)
			{
				return "ScenPart_ArriveInDropPods".Translate();
			}
			return null;
		}

		public override void Randomize()
		{
			method = ((Rand.Value < 0.5f) ? PlayerPawnsArriveMethod.DropPods : PlayerPawnsArriveMethod.Standing);
		}

		public override void GenerateIntoMap(Map map)
		{
			if (Find.GameInitData != null)
			{
				List<List<Thing>> list = new List<List<Thing>>();
				foreach (Pawn startingAndOptionalPawn in Find.GameInitData.startingAndOptionalPawns)
				{
					List<Thing> list2 = new List<Thing>();
					list2.Add(startingAndOptionalPawn);
					list.Add(list2);
				}
				List<Thing> list3 = new List<Thing>();
				foreach (ScenPart allPart in Find.Scenario.AllParts)
				{
					list3.AddRange(allPart.PlayerStartingThings());
				}
				int num = 0;
				foreach (Thing item in list3)
				{
					if (item.def.CanHaveFaction)
					{
						item.SetFactionDirect(Faction.OfPlayer);
					}
					list[num].Add(item);
					num++;
					if (num >= list.Count)
					{
						num = 0;
					}
				}
				DropPodUtility.DropThingGroupsNear_NewTmp(MapGenerator.PlayerStartSpot, map, list, 110, Find.GameInitData.QuickStarted || method != PlayerPawnsArriveMethod.DropPods, leaveSlag: true, canRoofPunch: true, forbid: true, allowFogged: false);
			}
		}

		public override void PostMapGenerate(Map map)
		{
			if (Find.GameInitData != null && method == PlayerPawnsArriveMethod.DropPods)
			{
				PawnUtility.GiveAllStartingPlayerPawnsThought(ThoughtDefOf.CrashedTogether);
			}
		}
	}
}
