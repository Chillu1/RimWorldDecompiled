using Verse;

namespace RimWorld
{
	public class Verb_SmokePop : Verb
	{
		protected override bool TryCastShot()
		{
			Pop(base.ReloadableCompSource);
			return true;
		}

		public override float HighlightFieldRadiusAroundTarget(out bool needLOSToCenter)
		{
			needLOSToCenter = false;
			return base.EquipmentSource.GetStatValue(StatDefOf.SmokepopBeltRadius);
		}

		public override void DrawHighlight(LocalTargetInfo target)
		{
			DrawHighlightFieldRadiusAroundTarget(caster);
		}

		public static void Pop(CompReloadable comp)
		{
			if (comp != null && comp.CanBeUsed)
			{
				ThingWithComps parent = comp.parent;
				Pawn wearer = comp.Wearer;
				GenExplosion.DoExplosion(wearer.Position, wearer.Map, parent.GetStatValue(StatDefOf.SmokepopBeltRadius), DamageDefOf.Smoke, null, -1, -1f, null, null, null, null, ThingDefOf.Gas_Smoke, 1f);
				comp.UsedOnce();
			}
		}
	}
}
