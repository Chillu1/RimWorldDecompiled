using Verse;
using Verse.AI;

namespace RimWorld;

public class Verb_MechCluster : Verb_CastBase
{
	public const float Points = 2500f;

	protected override bool TryCastShot()
	{
		if (Faction.OfMechanoids == null)
		{
			Messages.Message("MessageNoFactionForVerbMechCluster".Translate(), caster, MessageTypeDefOf.RejectInput, null, historical: false);
			return false;
		}
		if (currentTarget.HasThing && currentTarget.Thing.Map != caster.Map)
		{
			return false;
		}
		MechClusterUtility.SpawnCluster(currentTarget.Cell, caster.Map, MechClusterGenerator.GenerateClusterSketch(2500f, caster.Map, startDormant: true, forceNoConditionCauser: true));
		base.ReloadableCompSource?.UsedOnce();
		return true;
	}

	public override float HighlightFieldRadiusAroundTarget(out bool needLOSToCenter)
	{
		needLOSToCenter = false;
		return 23f;
	}
}
