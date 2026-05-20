using UnityEngine;
using Verse;

namespace RimWorld;

public class CompSpreadSludge : ThingComp
{
	private CompProperties_SpreadSludge Props => (CompProperties_SpreadSludge)props;

	private Pawn Pawn => parent as Pawn;

	public override void CompTick()
	{
		if (!Pawn.Spawned || Pawn.Downed)
		{
			return;
		}
		if (Props.abilityDef == AbilityDefOf.EggSpew)
		{
			Pawn_TrainingTracker training = Pawn.training;
			if (training != null && training.HasLearned(TrainableDefOf.EggSpew))
			{
				return;
			}
		}
		if (Props.abilityDef == AbilityDefOf.SludgeSpew)
		{
			Pawn_TrainingTracker training2 = Pawn.training;
			if (training2 != null && training2.HasLearned(TrainableDefOf.SludgeSpew))
			{
				return;
			}
		}
		float range;
		if (Rand.MTBEventOccurs(Props.mtbTicks, 1f, 1f))
		{
			Ability ability = Pawn.abilities?.GetAbility(Props.abilityDef);
			range = Props.abilityDef.verbProperties.range;
			if (ability != null && ((Props.tryAvoidSludge && CellFinder.TryFindRandomCellNear(parent.Position, parent.Map, Mathf.CeilToInt(range), (IntVec3 x) => ValidCell(x) && x.GetTerrain(parent.Map) != TerrainDefOf.InsectSludge, out var result)) || CellFinder.TryFindRandomCellNear(parent.Position, parent.Map, Mathf.CeilToInt(range), ValidCell, out result)))
			{
				ability.verb.TryStartCastOn(result, result, surpriseAttack: false, canHitNonTargetPawns: true, preventFriendlyFire: true);
			}
		}
		bool ValidCell(IntVec3 c)
		{
			if (!c.InBounds(parent.Map))
			{
				return false;
			}
			if (!c.InHorDistOf(parent.Position, range))
			{
				return false;
			}
			if (c.GetEdifice(parent.Map) != null)
			{
				return false;
			}
			TerrainDef terrain = c.GetTerrain(parent.Map);
			if (terrain.natural)
			{
				return !terrain.IsFloor;
			}
			return false;
		}
	}
}
