using Verse;

namespace RimWorld;

public class Verb_EntitySkip : Verb_CastAbility
{
	public override bool IsApplicableTo(LocalTargetInfo target, bool showMessages = false)
	{
		if (!ModLister.AnomalyInstalled)
		{
			return false;
		}
		if (!base.IsApplicableTo(target, showMessages))
		{
			return false;
		}
		return true;
	}

	public override bool ValidateTarget(LocalTargetInfo target, bool showMessages = true)
	{
		if (!base.ValidateTarget(target, showMessages))
		{
			return false;
		}
		return true;
	}

	public override void OnGUI(LocalTargetInfo target)
	{
		DrawAttachmentExtraLabel(target);
	}
}
