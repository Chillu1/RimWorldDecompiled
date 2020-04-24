using Verse;

namespace RimWorld
{
	public class Verb_Bombardment : Verb
	{
		private const int DurationTicks = 540;

		protected override bool TryCastShot()
		{
			if (currentTarget.HasThing && currentTarget.Thing.Map != caster.Map)
			{
				return false;
			}
			Bombardment obj = (Bombardment)GenSpawn.Spawn(ThingDefOf.Bombardment, currentTarget.Cell, caster.Map);
			obj.duration = 540;
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
			return 23f;
		}
	}
}
