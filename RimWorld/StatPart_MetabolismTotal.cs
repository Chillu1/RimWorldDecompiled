using Verse;

namespace RimWorld
{
	public class StatPart_MetabolismTotal : StatPart_BiostatTotal
	{
		public override string ExplanationLabelBase => "FactorForMetabolism".Translate();

		protected override float CurveXGetter(StatRequest req)
		{
			Pawn obj = (Pawn)req.Thing;
			int num = 0;
			foreach (Gene item in obj.genes.GenesListForReading)
			{
				if (!item.Overridden)
				{
					num += item.def.biostatMet;
				}
			}
			return num;
		}
	}
}
