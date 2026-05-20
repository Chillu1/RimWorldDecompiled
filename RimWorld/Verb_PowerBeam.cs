using Verse;
using Verse.AI;

namespace RimWorld;

public class Verb_PowerBeam : Verb_CastBase
{
	private const int DurationTicks = 600;

	protected override bool TryCastShot()
	{
		if (currentTarget.HasThing && currentTarget.Thing.Map != caster.Map)
		{
			return false;
		}
		PowerBeam obj = (PowerBeam)GenSpawn.Spawn(ThingDefOf.PowerBeam, currentTarget.Cell, caster.Map);
		obj.duration = 600;
		obj.instigator = caster;
		obj.weaponDef = ((base.EquipmentSource != null) ? base.EquipmentSource.def : null);
		obj.StartStrike();
		base.ReloadableCompSource?.UsedOnce();
		return true;
	}

	public override float HighlightFieldRadiusAroundTarget(out bool needLOSToCenter)
	{
		needLOSToCenter = false;
		return 15f;
	}
}
