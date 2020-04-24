using RimWorld.Planet;
using Verse;

namespace RimWorld
{
	public class HediffComp_PsychicSuppression : HediffComp
	{
		public override bool CompShouldRemove
		{
			get
			{
				if (base.Pawn.SpawnedOrAnyParentSpawned)
				{
					GameCondition_PsychicSuppression activeCondition = base.Pawn.MapHeld.gameConditionManager.GetActiveCondition<GameCondition_PsychicSuppression>();
					if (activeCondition != null && base.Pawn.gender == activeCondition.gender)
					{
						return false;
					}
				}
				else if (base.Pawn.IsCaravanMember())
				{
					bool result = true;
					{
						foreach (Site site in Find.World.worldObjects.Sites)
						{
							foreach (SitePart part in site.parts)
							{
								if (part.def.Worker is SitePartWorker_ConditionCauser_PsychicSuppressor)
								{
									CompCauseGameCondition_PsychicSuppression compCauseGameCondition_PsychicSuppression = part.conditionCauser.TryGetComp<CompCauseGameCondition_PsychicSuppression>();
									if (compCauseGameCondition_PsychicSuppression.ConditionDef.conditionClass == typeof(GameCondition_PsychicSuppression) && compCauseGameCondition_PsychicSuppression.InAoE(base.Pawn.GetCaravan().Tile) && compCauseGameCondition_PsychicSuppression.gender == base.Pawn.gender)
									{
										result = false;
									}
								}
							}
						}
						return result;
					}
				}
				return true;
			}
		}
	}
}
