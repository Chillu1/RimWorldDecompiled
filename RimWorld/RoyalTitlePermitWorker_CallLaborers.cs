using System;
using System.Collections.Generic;
using RimWorld.QuestGen;
using Verse;

namespace RimWorld
{
	public class RoyalTitlePermitWorker_CallLaborers : RoyalTitlePermitWorker
	{
		public override IEnumerable<FloatMenuOption> GetRoyalAidOptions(Map map, Pawn pawn, Faction faction)
		{
			if (AidDisabled(map, pawn, faction, out var reason))
			{
				yield return new FloatMenuOption(def.LabelCap + ": " + reason, null);
				yield break;
			}
			Action action = null;
			string description = def.LabelCap + " (" + "CommandCallLaborersNumLaborers".Translate(def.royalAid.pawnCount) + "): ";
			if (FillAidOption(pawn, faction, ref description, out var free))
			{
				action = delegate
				{
					CallLaborers(pawn, map, faction, free);
				};
			}
			yield return new FloatMenuOption(description, action, faction.def.FactionIcon, faction.Color);
		}

		private void CallLaborers(Pawn pawn, Map map, Faction faction, bool free)
		{
			if (!faction.HostileTo(Faction.OfPlayer))
			{
				QuestScriptDef permit_CallLaborers = QuestScriptDefOf.Permit_CallLaborers;
				Slate slate = new Slate();
				slate.Set("map", map);
				slate.Set("laborersCount", def.royalAid.pawnCount);
				slate.Set("permitFaction", faction);
				slate.Set("laborersPawnKind", def.royalAid.pawnKindDef);
				slate.Set("laborersDurationDays", def.royalAid.aidDurationDays);
				QuestUtility.GenerateQuestAndMakeAvailable(permit_CallLaborers, slate);
				pawn.royalty.GetPermit(def, faction).Notify_Used();
				if (!free)
				{
					pawn.royalty.TryRemoveFavor(faction, def.royalAid.favorCost);
				}
			}
		}
	}
}
