using System.Collections.Generic;
using RimWorld.Planet;
using Verse;

namespace RimWorld
{
	public class CompAbilityEffect_Neuroquake : CompAbilityEffect
	{
		private Dictionary<Faction, Pair<bool, Pawn>> affectedFactions;

		private List<Pawn> giveMentalStateTo = new List<Pawn>();

		public new CompProperties_AbilityNeuroquake Props => (CompProperties_AbilityNeuroquake)props;

		public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
		{
			if (affectedFactions == null)
			{
				affectedFactions = new Dictionary<Faction, Pair<bool, Pawn>>();
			}
			else
			{
				affectedFactions.Clear();
			}
			giveMentalStateTo.Clear();
			foreach (Pawn item in parent.pawn.Map.mapPawns.AllPawnsSpawned)
			{
				if (CanApplyEffects(item) && !item.Fogged())
				{
					bool flag = !item.Spawned || item.Position.InHorDistOf(parent.pawn.Position, parent.def.EffectRadius);
					AffectGoodwill(item.FactionOrExtraMiniOrHomeFaction, !flag, item);
					if (!flag)
					{
						giveMentalStateTo.Add(item);
					}
					else
					{
						GiveNeuroquakeThought(item);
					}
				}
			}
			foreach (Map map in Find.Maps)
			{
				if (map == parent.pawn.Map || Find.WorldGrid.TraversalDistanceBetween(map.Tile, parent.pawn.Map.Tile, passImpassable: true, Props.worldRangeTiles + 1) > Props.worldRangeTiles)
				{
					continue;
				}
				foreach (Pawn allPawn in map.mapPawns.AllPawns)
				{
					if (CanApplyEffects(allPawn))
					{
						GiveNeuroquakeThought(allPawn);
					}
				}
			}
			foreach (Caravan caravan in Find.WorldObjects.Caravans)
			{
				if (Find.WorldGrid.TraversalDistanceBetween(caravan.Tile, parent.pawn.Map.Tile, passImpassable: true, Props.worldRangeTiles + 1) > Props.worldRangeTiles)
				{
					continue;
				}
				foreach (Pawn pawn in caravan.pawns)
				{
					if (CanApplyEffects(pawn))
					{
						GiveNeuroquakeThought(pawn);
					}
				}
			}
			foreach (Pawn item2 in giveMentalStateTo)
			{
				MentalStateDef mentalStateDef = null;
				mentalStateDef = (item2.RaceProps.IsMechanoid ? MentalStateDefOf.BerserkMechanoid : MentalStateDefOf.Berserk);
				CompAbilityEffect_GiveMentalState.TryGiveMentalStateWithDuration(mentalStateDef, item2, parent.def, StatDefOf.PsychicSensitivity);
				RestUtility.WakeUp(item2);
			}
			foreach (Faction allFaction in Find.FactionManager.AllFactions)
			{
				if (!allFaction.IsPlayer && !allFaction.defeated && !allFaction.HostileTo(Faction.OfPlayer))
				{
					AffectGoodwill(allFaction, gaveMentalBreak: false);
				}
			}
			foreach (KeyValuePair<Faction, Pair<bool, Pawn>> affectedFaction in affectedFactions)
			{
				Faction key = affectedFaction.Key;
				bool first = affectedFaction.Value.First;
				Pawn second = affectedFaction.Value.Second;
				key.TryAffectGoodwillWith(parent.pawn.Faction, first ? Props.goodwillImpactForBerserk : Props.goodwillImpactForNeuroquake, canSendMessage: true, canSendHostilityLetter: true, (first ? "GoodwillChangedReason_CausedBerserk" : "GoodwillChangedReason_CausedNeuroquakeEcho").Translate(second.Named("PAWN")), second);
			}
			base.Apply(target, dest);
			affectedFactions.Clear();
			giveMentalStateTo.Clear();
		}

		private void AffectGoodwill(Faction faction, bool gaveMentalBreak, Pawn p = null)
		{
			if (faction != null && !faction.IsPlayer && !faction.HostileTo(Faction.OfPlayer) && (!affectedFactions.TryGetValue(faction, out var value) || (!value.First && gaveMentalBreak)))
			{
				affectedFactions[faction] = new Pair<bool, Pawn>(gaveMentalBreak, p);
			}
		}

		private void GiveNeuroquakeThought(Pawn p)
		{
			p.needs?.mood?.thoughts.memories.TryGainMemory(ThoughtDefOf.NeuroquakeEcho);
		}

		private bool CanApplyEffects(Pawn p)
		{
			if (!p.Dead && !p.Downed && !p.Suspended)
			{
				return p.GetStatValue(StatDefOf.PsychicSensitivity) > float.Epsilon;
			}
			return false;
		}
	}
}
