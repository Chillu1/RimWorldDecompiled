using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class ChoiceLetter_RansomDemand : ChoiceLetter
	{
		public Map map;

		public Faction faction;

		public Pawn kidnapped;

		public int fee;

		public override IEnumerable<DiaOption> Choices
		{
			get
			{
				if (base.ArchivedOnly)
				{
					yield return base.Option_Close;
					yield break;
				}
				DiaOption diaOption = new DiaOption("RansomDemand_Accept".Translate());
				diaOption.action = delegate
				{
					faction.kidnapped.RemoveKidnappedPawn(kidnapped);
					Find.WorldPawns.RemovePawn(kidnapped);
					IntVec3 result;
					if ((int)faction.def.techLevel < 4)
					{
						if (!CellFinder.TryFindRandomEdgeCellWith((IntVec3 c) => c.Standable(map) && map.reachability.CanReachColony(c), map, CellFinder.EdgeRoadChance_Friendly, out result) && !CellFinder.TryFindRandomEdgeCellWith((IntVec3 c) => c.Standable(map), map, CellFinder.EdgeRoadChance_Friendly, out result))
						{
							Log.Warning("Could not find any edge cell.");
							result = DropCellFinder.TradeDropSpot(map);
						}
						GenSpawn.Spawn(kidnapped, result, map);
					}
					else
					{
						result = DropCellFinder.TradeDropSpot(map);
						TradeUtility.SpawnDropPod(result, map, kidnapped);
					}
					CameraJumper.TryJump(result, map);
					TradeUtility.LaunchSilver(map, fee);
					Find.LetterStack.RemoveLetter(this);
				};
				diaOption.resolveTree = true;
				if (!TradeUtility.ColonyHasEnoughSilver(map, fee))
				{
					diaOption.Disable("NeedSilverLaunchable".Translate(fee.ToString()));
				}
				yield return diaOption;
				yield return base.Option_Reject;
				yield return base.Option_Postpone;
			}
		}

		public override bool CanShowInLetterStack
		{
			get
			{
				if (!base.CanShowInLetterStack)
				{
					return false;
				}
				if (!Find.Maps.Contains(map))
				{
					return false;
				}
				return faction.kidnapped.KidnappedPawnsListForReading.Contains(kidnapped);
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_References.Look(ref map, "map");
			Scribe_References.Look(ref faction, "faction");
			Scribe_References.Look(ref kidnapped, "kidnapped");
			Scribe_Values.Look(ref fee, "fee", 0);
		}
	}
}
