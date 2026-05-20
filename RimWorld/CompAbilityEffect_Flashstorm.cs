using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

public class CompAbilityEffect_Flashstorm : CompAbilityEffect
{
	private HashSet<Faction> affectedFactionCache = new HashSet<Faction>();

	public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
	{
		base.Apply(target, dest);
		Map map = parent.pawn.Map;
		Thing conditionCauser = GenSpawn.Spawn(ThingDefOf.Flashstorm, target.Cell, parent.pawn.Map);
		GameCondition_Flashstorm gameCondition_Flashstorm = (GameCondition_Flashstorm)GameConditionMaker.MakeCondition(GameConditionDefOf.Flashstorm);
		gameCondition_Flashstorm.centerLocation = target.Cell.ToIntVec2;
		gameCondition_Flashstorm.areaRadiusOverride = new IntRange(Mathf.RoundToInt(parent.def.EffectRadius), Mathf.RoundToInt(parent.def.EffectRadius));
		gameCondition_Flashstorm.Duration = Mathf.RoundToInt(parent.def.EffectDuration(parent.pawn).SecondsToTicks());
		gameCondition_Flashstorm.suppressEndMessage = true;
		gameCondition_Flashstorm.initialStrikeDelay = new IntRange(60, 180);
		gameCondition_Flashstorm.conditionCauser = conditionCauser;
		gameCondition_Flashstorm.ambientSound = true;
		map.gameConditionManager.RegisterCondition(gameCondition_Flashstorm);
		ApplyGoodwillImpact(target, gameCondition_Flashstorm.AreaRadius);
	}

	private void ApplyGoodwillImpact(LocalTargetInfo target, int radius)
	{
		if (parent.pawn.Faction != Faction.OfPlayer)
		{
			return;
		}
		affectedFactionCache.Clear();
		foreach (Thing item in GenRadial.RadialDistinctThingsAround(target.Cell, parent.pawn.Map, radius, useCenter: true))
		{
			if (item is Pawn p && item.Faction != null && item.Faction != parent.pawn.Faction && !item.Faction.HostileTo(parent.pawn.Faction) && !affectedFactionCache.Contains(item.Faction) && (base.Props.applyGoodwillImpactToLodgers || !p.IsQuestLodger()))
			{
				affectedFactionCache.Add(item.Faction);
				Faction.OfPlayer.TryAffectGoodwillWith(item.Faction, base.Props.goodwillImpact, canSendMessage: true, canSendHostilityLetter: true, HistoryEventDefOf.UsedHarmfulAbility);
			}
		}
		affectedFactionCache.Clear();
	}

	public override bool Valid(LocalTargetInfo target, bool throwMessages = false)
	{
		if (target.Cell.Roofed(parent.pawn.Map))
		{
			if (throwMessages)
			{
				Messages.Message("CannotUseAbility".Translate(parent.def.label) + ": " + "AbilityRoofed".Translate(), target.ToTargetInfo(parent.pawn.Map), MessageTypeDefOf.RejectInput, historical: false);
			}
			return false;
		}
		return true;
	}
}
