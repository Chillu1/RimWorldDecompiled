using RimWorld;

namespace Verse.AI;

public class Verb_CastTargetEffectLances : Verb_CastTargetEffect
{
	public override void OnGUI(LocalTargetInfo target)
	{
		if (CanHitTarget(target) && verbProps.targetParams.CanTarget(target.ToTargetInfo(caster.Map)))
		{
			Pawn pawn = target.Pawn;
			if (pawn != null)
			{
				bool flag = target.Pawn.kindDef.isBoss;
				foreach (CompTargetEffect comp in base.EquipmentSource.GetComps<CompTargetEffect>())
				{
					if (!comp.CanApplyOn(pawn))
					{
						flag = true;
						break;
					}
				}
				if (flag)
				{
					GenUI.DrawMouseAttachment(TexCommand.CannotShoot);
					if (!string.IsNullOrEmpty(verbProps.invalidTargetPawn))
					{
						Widgets.MouseAttachedLabel(verbProps.invalidTargetPawn.CapitalizeFirst(), 0f, -20f);
					}
				}
				else if (pawn.GetStatValue(StatDefOf.PsychicSensitivity) <= 0f)
				{
					GenUI.DrawMouseAttachment(TexCommand.CannotShoot);
					Widgets.MouseAttachedLabel("CannotShootPawnIsPsychicallyDeaf".Translate(pawn), 0f, -20f);
				}
			}
			else
			{
				base.OnGUI(target);
			}
		}
		else
		{
			GenUI.DrawMouseAttachment(TexCommand.CannotShoot);
		}
	}

	public override bool ValidateTarget(LocalTargetInfo target, bool showMessages = true)
	{
		Pawn pawn = target.Pawn;
		if (pawn != null)
		{
			if (target.Pawn.kindDef.isBoss)
			{
				return false;
			}
			if (pawn.GetStatValue(StatDefOf.PsychicSensitivity) <= 0f)
			{
				return false;
			}
			foreach (CompTargetEffect comp in base.EquipmentSource.GetComps<CompTargetEffect>())
			{
				if (!comp.CanApplyOn(target.Pawn))
				{
					return false;
				}
			}
		}
		return base.ValidateTarget(target, showMessages);
	}
}
