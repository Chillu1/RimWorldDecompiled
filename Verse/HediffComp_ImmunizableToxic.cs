using RimWorld;

namespace Verse
{
	public class HediffComp_ImmunizableToxic : HediffComp_Immunizable
	{
		public override float SeverityChangePerDay()
		{
			float num = base.SeverityChangePerDay();
			if (num < 0f && base.Pawn.Spawned && ModsConfig.BiotechActive)
			{
				if (base.Pawn.Position.IsPolluted(base.Pawn.Map) && base.Pawn.GetStatValue(StatDefOf.ToxicEnvironmentResistance) < 1f)
				{
					num = 0f;
				}
				else if (!base.Pawn.Position.Roofed(base.Pawn.Map) && base.Pawn.Map.GameConditionManager.ActiveConditions.Any((GameCondition x) => x.def.conditionClass == typeof(GameCondition_ToxicFallout)) && base.Pawn.GetStatValue(StatDefOf.ToxicResistance) < 1f)
				{
					num = 0f;
				}
			}
			return num;
		}
	}
}
