using RimWorld;

namespace Verse;

public class HediffComp_SurgeryInspectableMetalhorror : HediffComp_SurgeryInspectable
{
	public new HediffCompProperties_SurgeryInspectableMetalHorror Props => (HediffCompProperties_SurgeryInspectableMetalHorror)props;

	public Hediff_MetalhorrorImplant Implant => (Hediff_MetalhorrorImplant)parent;

	public override SurgicalInspectionOutcome DoSurgicalInspection(Pawn surgeon)
	{
		if (!Find.AnalysisManager.TryGetAnalysisProgress(Implant.Biosignature, out var details) || !details.Satisfied)
		{
			return SurgicalInspectionOutcome.Nothing;
		}
		if (MetalhorrorUtility.IsInfected(surgeon, out var hediff))
		{
			hediff.LiedAboutInspection = true;
			return SurgicalInspectionOutcome.Nothing;
		}
		Emerge(surgeon);
		return SurgicalInspectionOutcome.DetectedNoLetter;
	}

	public override void DoSurgicalInspectionVisible(Pawn surgeon)
	{
		Emerge(surgeon);
	}

	private void Emerge(Pawn surgeon)
	{
		Implant.Emerge("MetalhorrorReasonSurgeryInspected".Translate(base.Pawn.Named("INFECTED"), surgeon.Named("PAWN")));
	}
}
