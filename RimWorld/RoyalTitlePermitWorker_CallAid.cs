using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	[StaticConstructorOnStartup]
	public class RoyalTitlePermitWorker_CallAid : RoyalTitlePermitWorker
	{
		public static readonly Texture2D CommandTex = ContentFinder<Texture2D>.Get("UI/Commands/AttackSettlement");

		public override IEnumerable<FloatMenuOption> GetRoyalAidOptions(Map map, Pawn pawn, Faction faction)
		{
			if (faction.HostileTo(Faction.OfPlayer))
			{
				yield return new FloatMenuOption(def.LabelCap + ": " + "CommandCallRoyalAidFactionHostile".Translate(faction.Named("FACTION")), null);
				yield break;
			}
			if (!faction.def.allowedArrivalTemperatureRange.ExpandedBy(-4f).Includes(pawn.MapHeld.mapTemperature.SeasonalTemp))
			{
				yield return new FloatMenuOption(def.LabelCap + ": " + "BadTemperature".Translate(), null);
				yield break;
			}
			if (NeutralGroupIncidentUtility.AnyBlockingHostileLord(pawn.MapHeld, faction))
			{
				yield return new FloatMenuOption(def.LabelCap + ": " + "HostileVisitorsPresent".Translate(), null);
				yield break;
			}
			int permitLastUsedTick = pawn.royalty.GetPermitLastUsedTick(def);
			int num = Math.Max(GenTicks.TicksGame - permitLastUsedTick, 0);
			Action action = null;
			bool num2 = permitLastUsedTick < 0 || num >= def.CooldownTicks;
			int numTicks = (permitLastUsedTick > 0) ? Math.Max(def.CooldownTicks - num, 0) : 0;
			string t = def.LabelCap + ": ";
			if (num2)
			{
				t += "CommandCallRoyalAidFreeOption".Translate();
				action = delegate
				{
					CallAid(pawn, map, faction, free: true);
				};
			}
			else
			{
				if (pawn.royalty.GetFavor(faction) >= def.royalAid.favorCost)
				{
					action = delegate
					{
						CallAid(pawn, map, faction, free: false);
					};
				}
				t += "CommandCallRoyalAidFavorOption".Translate(numTicks.TicksToDays().ToString("0.0"), def.royalAid.favorCost, faction.Named("FACTION"));
			}
			yield return new FloatMenuOption(t, action, faction.def.FactionIcon, faction.Color);
		}

		private void CallAid(Pawn caller, Map map, Faction faction, bool free, float biocodeChance = 1f)
		{
			IncidentParms incidentParms = new IncidentParms();
			incidentParms.target = map;
			incidentParms.faction = faction;
			incidentParms.raidArrivalModeForQuickMilitaryAid = true;
			incidentParms.spawnCenter = caller.Position;
			incidentParms.biocodeApparelChance = biocodeChance;
			incidentParms.biocodeWeaponsChance = biocodeChance;
			if (def.royalAid.pawnKindDef != null)
			{
				incidentParms.pawnKind = def.royalAid.pawnKindDef;
				incidentParms.pawnCount = def.royalAid.pawnCount;
			}
			else
			{
				incidentParms.points = def.royalAid.points;
			}
			faction.lastMilitaryAidRequestTick = Find.TickManager.TicksGame;
			if (IncidentDefOf.RaidFriendly.Worker.TryExecute(incidentParms))
			{
				if (!free)
				{
					caller.royalty.TryRemoveFavor(faction, def.royalAid.favorCost);
				}
				caller.royalty.Notify_PermitUsed(def);
			}
			else
			{
				Log.Error("Could not send aid to map " + map + " from faction " + faction);
			}
		}
	}
}
