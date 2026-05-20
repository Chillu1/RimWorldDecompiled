using Verse;

namespace RimWorld
{
	public class Verb_CastAbilityTouch : Verb_CastAbility
	{
		public override void DrawHighlight(LocalTargetInfo target)
		{
			if (target.IsValid && IsApplicableTo(target))
			{
				GenDraw.DrawTargetHighlight(target);
				ability.DrawEffectPreviews(target);
			}
		}

		public override void OnGUI(LocalTargetInfo target)
		{
			if (ValidateTarget(target, showMessages: false))
			{
				GenUI.DrawMouseAttachment(UIIcon);
			}
			else
			{
				GenUI.DrawMouseAttachment(TexCommand.CannotShoot);
			}
			DrawAttachmentExtraLabel(target);
		}

		public override bool ValidateTarget(LocalTargetInfo target, bool showMessages = true)
		{
			if (!IsApplicableTo(target, showMessages))
			{
				return false;
			}
			for (int i = 0; i < ability.EffectComps.Count; i++)
			{
				if (!ability.EffectComps[i].Valid(target, showMessages))
				{
					return false;
				}
			}
			return true;
		}
	}
}
