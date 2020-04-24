using RimWorld;
using System.Text;

namespace Verse
{
	public class Hediff_AddedPart : Hediff_Implant
	{
		public override string TipStringExtra
		{
			get
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append(base.TipStringExtra);
				stringBuilder.AppendLine("Efficiency".Translate() + ": " + def.addedPartProps.partEfficiency.ToStringPercent());
				return stringBuilder.ToString();
			}
		}

		public override void PostAdd(DamageInfo? dinfo)
		{
			base.PostAdd(dinfo);
			pawn.health.RestorePart(base.Part, this, checkStateChange: false);
			for (int i = 0; i < base.Part.parts.Count; i++)
			{
				Hediff_MissingPart hediff_MissingPart = (Hediff_MissingPart)HediffMaker.MakeHediff(HediffDefOf.MissingBodyPart, pawn);
				hediff_MissingPart.IsFresh = true;
				hediff_MissingPart.lastInjury = HediffDefOf.SurgicalCut;
				hediff_MissingPart.Part = base.Part.parts[i];
				pawn.health.hediffSet.AddDirect(hediff_MissingPart);
			}
		}
	}
}
