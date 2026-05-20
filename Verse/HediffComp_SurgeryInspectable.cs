using RimWorld;

namespace Verse;

public class HediffComp_SurgeryInspectable : HediffComp
{
	public HediffCompProperties_SurgeryInspectable Props => (HediffCompProperties_SurgeryInspectable)props;

	public virtual SurgicalInspectionOutcome DoSurgicalInspection(Pawn surgeon)
	{
		return SurgicalInspectionOutcome.Detected;
	}

	public virtual void DoSurgicalInspectionVisible(Pawn surgeon)
	{
	}
}
