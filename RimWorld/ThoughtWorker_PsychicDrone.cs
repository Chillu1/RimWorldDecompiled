using System;
using RimWorld.Planet;
using Verse;

namespace RimWorld;

public class ThoughtWorker_PsychicDrone : ThoughtWorker
{
	protected override ThoughtState CurrentStateInternal(Pawn p)
	{
		PsychicDroneLevel psychicDroneLevel = PsychicDroneLevel.None;
		Map mapHeld = p.MapHeld;
		if (mapHeld != null)
		{
			PsychicDroneLevel highestPsychicDroneLevelFor = mapHeld.gameConditionManager.GetHighestPsychicDroneLevelFor(p.gender, mapHeld);
			if ((int)highestPsychicDroneLevelFor > (int)psychicDroneLevel)
			{
				psychicDroneLevel = highestPsychicDroneLevelFor;
			}
		}
		else if (p.IsCaravanMember())
		{
			foreach (Site site in Find.World.worldObjects.Sites)
			{
				foreach (SitePart part in site.parts)
				{
					if (!part.conditionCauser.DestroyedOrNull() && part.def.Worker is SitePartWorker_ConditionCauser_PsychicDroner)
					{
						CompCauseGameCondition_PsychicEmanation compCauseGameCondition_PsychicEmanation = part.conditionCauser.TryGetComp<CompCauseGameCondition_PsychicEmanation>();
						if (compCauseGameCondition_PsychicEmanation.ConditionDef.conditionClass == typeof(GameCondition_PsychicEmanation) && compCauseGameCondition_PsychicEmanation.InAoE(p.GetCaravan().Tile) && compCauseGameCondition_PsychicEmanation.gender == p.gender && (int)compCauseGameCondition_PsychicEmanation.Level > (int)psychicDroneLevel)
						{
							psychicDroneLevel = compCauseGameCondition_PsychicEmanation.Level;
						}
					}
				}
			}
			foreach (Map map in Find.Maps)
			{
				foreach (GameCondition activeCondition in map.gameConditionManager.ActiveConditions)
				{
					CompCauseGameCondition_PsychicEmanation compCauseGameCondition_PsychicEmanation2 = activeCondition.conditionCauser.TryGetComp<CompCauseGameCondition_PsychicEmanation>();
					if (compCauseGameCondition_PsychicEmanation2 != null && compCauseGameCondition_PsychicEmanation2.InAoE(p.GetCaravan().Tile) && compCauseGameCondition_PsychicEmanation2.gender == p.gender && (int)compCauseGameCondition_PsychicEmanation2.Level > (int)psychicDroneLevel)
					{
						psychicDroneLevel = compCauseGameCondition_PsychicEmanation2.Level;
					}
				}
			}
		}
		return psychicDroneLevel switch
		{
			PsychicDroneLevel.None => false, 
			PsychicDroneLevel.GoodMedium => ThoughtState.ActiveAtStage(0), 
			PsychicDroneLevel.BadLow => ThoughtState.ActiveAtStage(1), 
			PsychicDroneLevel.BadMedium => ThoughtState.ActiveAtStage(2), 
			PsychicDroneLevel.BadHigh => ThoughtState.ActiveAtStage(3), 
			PsychicDroneLevel.BadExtreme => ThoughtState.ActiveAtStage(4), 
			_ => throw new NotImplementedException(), 
		};
	}
}
