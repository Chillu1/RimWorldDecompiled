using Verse;

namespace RimWorld
{
	public class Verb_PowerBeam : Verb
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
			if (base.EquipmentSource != null && !base.EquipmentSource.Destroyed)
			{
				base.EquipmentSource.Destroy();
			}
			return true;
		}

		public override float HighlightFieldRadiusAroundTarget(out bool needLOSToCenter)
		{
			needLOSToCenter = false;
			return 15f;
		}
	}
}
