using Verse;

namespace RimWorld;

public class FloatMenuOptionProvider_Romance : FloatMenuOptionProvider
{
	protected override bool Drafted => false;

	protected override bool Undrafted => true;

	protected override bool Multiselect => false;

	protected override bool AppliesInt(FloatMenuContext context)
	{
		return ModsConfig.BiotechActive;
	}

	protected override FloatMenuOption GetSingleOptionFor(Pawn clickedPawn, FloatMenuContext context)
	{
		if (!clickedPawn.IsColonist || clickedPawn.HostFaction != null)
		{
			return null;
		}
		if (clickedPawn.Drafted)
		{
			return null;
		}
		FloatMenuOption option;
		float chance;
		bool flag = RelationsUtility.RomanceOption(context.FirstSelectedPawn, clickedPawn, out option, out chance);
		if (option == null)
		{
			return null;
		}
		option.Label = (flag ? "CanRomance" : "CannotRomance").Translate(option.Label);
		return option;
	}
}
