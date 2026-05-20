namespace Verse;

public class HediffComp_DisappearsDisableable : HediffComp_DisappearsPausable
{
	public bool disabled;

	public new HediffCompProperties_DisappearsDisableable Props => (HediffCompProperties_DisappearsDisableable)props;

	protected override bool Paused => disabled;

	public override string CompLabelInBracketsExtra
	{
		get
		{
			if (!disabled)
			{
				return base.CompLabelInBracketsExtra;
			}
			return null;
		}
	}

	public override void CompPostMake()
	{
		base.CompPostMake();
		disabled = Props.initiallyDisabled;
	}

	public override void CopyFrom(HediffComp other)
	{
		base.CopyFrom(other);
		if (other is HediffComp_DisappearsDisableable hediffComp_DisappearsDisableable)
		{
			disabled = hediffComp_DisappearsDisableable.disabled;
		}
	}
}
