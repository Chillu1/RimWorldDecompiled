namespace Verse;

[StaticConstructorOnStartup]
public class HediffComp_Chargeable : HediffComp
{
	private float charge;

	public HediffCompProperties_Chargeable Props => (HediffCompProperties_Chargeable)props;

	public float Charge
	{
		get
		{
			return charge;
		}
		protected set
		{
			charge = value;
			if (charge > Props.fullChargeAmount)
			{
				charge = Props.fullChargeAmount;
			}
		}
	}

	public bool CanActivate => charge >= Props.minChargeToActivate;

	public override string CompLabelInBracketsExtra => Props.labelInBrackets.Formatted(charge.Named("CHARGE"), (charge / Props.fullChargeAmount).Named("CHARGEFACTOR"));

	public override void CompPostMake()
	{
		base.CompPostMake();
		charge = Props.initialCharge;
	}

	public override void CompExposeData()
	{
		base.CompExposeData();
		Scribe_Values.Look(ref charge, "charge", Props.initialCharge);
	}

	public virtual void TryCharge(float desiredChargeAmount)
	{
		Charge += desiredChargeAmount;
	}

	public override void CompPostTickInterval(ref float severityAdjustment, int delta)
	{
		base.CompPostTickInterval(ref severityAdjustment, delta);
		if (Props.ticksToFullCharge > 0)
		{
			TryCharge(Props.fullChargeAmount / (float)Props.ticksToFullCharge * (float)delta);
		}
		else if (Props.ticksToFullCharge == 0)
		{
			TryCharge(Props.fullChargeAmount);
		}
	}

	public float GreedyConsume(float desiredCharge)
	{
		float num;
		if (desiredCharge >= charge)
		{
			num = charge;
			charge = 0f;
		}
		else
		{
			num = desiredCharge;
			charge -= num;
		}
		return num;
	}
}
