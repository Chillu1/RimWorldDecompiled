using RimWorld;

namespace Verse;

public class Hediff_SentienceCatalyst : Hediff
{
	public override string TipStringExtra
	{
		get
		{
			string tipStringExtra = base.TipStringExtra;
			TrainabilityDef trainability = TrainableUtility.GetTrainability(pawn);
			if (pawn.training == null || trainability == null || trainability == pawn.RaceProps.trainability)
			{
				return tipStringExtra;
			}
			tipStringExtra += "\n";
			return tipStringExtra + ("Trainability".Translate() + ": " + trainability.LabelCap);
		}
	}
}
