using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

namespace RimWorld;

public class JobGiver_AICastAnimalWarcall : JobGiver_AICastAbility
{
	private const float MaxDistanceFromCaster = 30f;

	private const float MaxSquareDistanceFromTarget = 625f;

	private static readonly SimpleCurve DistanceSquaredToTargetSelectionWeightCurve = new SimpleCurve
	{
		new CurvePoint(100f, 1f),
		new CurvePoint(400f, 0.1f),
		new CurvePoint(625f, 0f)
	};

	private static List<Pawn> potentialTargets = new List<Pawn>();

	protected override LocalTargetInfo GetTarget(Pawn caster, Ability ability)
	{
		potentialTargets.Clear();
		IEnumerable<Thing> hostiles = from x in caster.Map.attackTargetsCache.GetPotentialTargetsFor(caster)
			select x.Thing;
		if (hostiles.EnumerableNullOrEmpty())
		{
			return LocalTargetInfo.Invalid;
		}
		foreach (Pawn item in caster.Map.mapPawns.AllPawnsSpawned)
		{
			if ((!item.IsMutant || !item.mutant.Def.preventsMentalBreaks) && item.RaceProps.Animal && item.Faction == null && item.MentalStateDef != MentalStateDefOf.BerserkWarcall && item.Position.InHorDistOf(caster.Position, 30f) && ability.CanApplyOn(new LocalTargetInfo(item)))
			{
				potentialTargets.Add(item);
			}
		}
		if (potentialTargets.TryRandomElementByWeight(delegate(Pawn x)
		{
			float num = 625f;
			foreach (Thing item2 in hostiles)
			{
				if (item2.Spawned)
				{
					float num2 = item2.Position.DistanceToSquared(x.Position);
					if (num2 < num)
					{
						num = num2;
					}
				}
			}
			return DistanceSquaredToTargetSelectionWeightCurve.Evaluate(num);
		}, out var result))
		{
			return new LocalTargetInfo(result);
		}
		return LocalTargetInfo.Invalid;
	}
}
