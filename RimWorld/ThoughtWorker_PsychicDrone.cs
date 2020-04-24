using RimWorld.Planet;
using System;
using Verse;

namespace RimWorld
{
	public class ThoughtWorker_PsychicDrone : ThoughtWorker
	{
		protected override ThoughtState CurrentStateInternal(Pawn p)
		{
			PsychicDroneLevel psychicDroneLevel = PsychicDroneLevel.None;
			if (p.Map != null)
			{
				PsychicDroneLevel highestPsychicDroneLevelFor = p.Map.gameConditionManager.GetHighestPsychicDroneLevelFor(p.gender);
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
			switch (psychicDroneLevel)
			{
			case PsychicDroneLevel.None:
				return false;
			case PsychicDroneLevel.GoodMedium:
				return ThoughtState.ActiveAtStage(0);
			case PsychicDroneLevel.BadLow:
				return ThoughtState.ActiveAtStage(1);
			case PsychicDroneLevel.BadMedium:
				return ThoughtState.ActiveAtStage(2);
			case PsychicDroneLevel.BadHigh:
				return ThoughtState.ActiveAtStage(3);
			case PsychicDroneLevel.BadExtreme:
				return ThoughtState.ActiveAtStage(4);
			default:
				throw new NotImplementedException();
			}
		}
	}
}
