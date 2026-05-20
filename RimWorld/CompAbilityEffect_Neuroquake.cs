using System.Collections.Generic;
using RimWorld.Planet;
using Verse;

namespace RimWorld;

public class CompAbilityEffect_Neuroquake : CompAbilityEffect
{
	private Dictionary<Faction, Pair<bool, Pawn>> affectedFactions;

	private List<Pawn> giveMentalStateTo = new List<Pawn>();

	private static List<IntVec3> cachedRadiusCells = new List<IntVec3>();

	private static IntVec3? cachedRadiusCellsTarget = null;

	private static Map cachedRadiusCellsMap = null;

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
				bool flag = !item.Spawned || item.Position.InHorDistOf(parent.pawn.Position, parent.def.EffectRadius) || !item.Position.InHorDistOf(parent.pawn.Position, Props.mentalStateRadius);
				AffectGoodwill(item.HomeFaction, !flag, item);
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
			CompAbilityEffect_GiveMentalState.TryGiveMentalState(mentalStateDef, item2, parent.def, StatDefOf.PsychicSensitivity, parent.pawn);
			RestUtility.WakeUp(item2);
		}
		foreach (Faction allFaction in Find.FactionManager.AllFactions)
		{
			if (!allFaction.IsPlayer && !allFaction.defeated && !allFaction.HostileTo(Faction.OfPlayer))
			{
				AffectGoodwill(allFaction, gaveMentalBreak: false);
			}
		}
		if (parent.pawn.Faction == Faction.OfPlayer)
		{
			foreach (KeyValuePair<Faction, Pair<bool, Pawn>> affectedFaction in affectedFactions)
			{
				Faction key = affectedFaction.Key;
				bool first = affectedFaction.Value.First;
				_ = affectedFaction.Value.Second;
				int goodwillChange = (first ? Props.goodwillImpactForBerserk : Props.goodwillImpactForNeuroquake);
				Faction.OfPlayer.TryAffectGoodwillWith(key, goodwillChange, canSendMessage: true, canSendHostilityLetter: true, HistoryEventDefOf.UsedHarmfulAbility);
			}
		}
		base.Apply(target, dest);
		affectedFactions.Clear();
		giveMentalStateTo.Clear();
	}

	private void AffectGoodwill(Faction faction, bool gaveMentalBreak, Pawn p = null)
	{
		if (faction != null && !faction.IsPlayer && !faction.HostileTo(Faction.OfPlayer) && (p == null || !p.IsSlaveOfColony) && (!affectedFactions.TryGetValue(faction, out var value) || (!value.First && gaveMentalBreak)))
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
		if (p.kindDef.isBoss)
		{
			return false;
		}
		if (!p.Dead && !p.Suspended)
		{
			return p.GetStatValue(StatDefOf.PsychicSensitivity) > float.Epsilon;
		}
		return false;
	}

	public override void OnGizmoUpdate()
	{
		if (!cachedRadiusCellsTarget.HasValue || cachedRadiusCellsTarget.Value == parent.pawn.Position || cachedRadiusCellsMap != parent.pawn.Map)
		{
			cachedRadiusCells.Clear();
			foreach (IntVec3 allCell in parent.pawn.Map.AllCells)
			{
				if (allCell.InHorDistOf(parent.pawn.Position, Props.mentalStateRadius))
				{
					cachedRadiusCells.Add(allCell);
				}
			}
			cachedRadiusCellsTarget = parent.pawn.Position;
		}
		GenDraw.DrawFieldEdges(cachedRadiusCells);
	}
}
