using RimWorld;
using System.Collections.Generic;
using System.Linq;

namespace Verse.AI
{
	public class MentalState_BingingDrug : MentalState_Binging
	{
		public ChemicalDef chemical;

		public DrugCategory drugCategory;

		private static List<ChemicalDef> addictions = new List<ChemicalDef>();

		public override string InspectLine => string.Format(base.InspectLine, chemical.label);

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Defs.Look(ref chemical, "chemical");
			Scribe_Values.Look(ref drugCategory, "drugCategory", DrugCategory.None);
		}

		public override void PostStart(string reason)
		{
			base.PostStart(reason);
			ChooseRandomChemical();
			if (PawnUtility.ShouldSendNotificationAbout(pawn))
			{
				string str = "LetterLabelDrugBinge".Translate(chemical.label).CapitalizeFirst() + ": " + pawn.LabelShortCap;
				string text = "LetterDrugBinge".Translate(pawn.Label, chemical.label, pawn).CapitalizeFirst();
				if (!reason.NullOrEmpty())
				{
					text = text + "\n\n" + reason;
				}
				Find.LetterStack.ReceiveLetter(str, text, LetterDefOf.ThreatSmall, pawn);
			}
		}

		public override void PostEnd()
		{
			base.PostEnd();
			if (PawnUtility.ShouldSendNotificationAbout(pawn))
			{
				Messages.Message("MessageNoLongerBingingOnDrug".Translate(pawn.LabelShort, chemical.label, pawn), pawn, MessageTypeDefOf.SituationResolved);
			}
		}

		private void ChooseRandomChemical()
		{
			addictions.Clear();
			List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
			for (int i = 0; i < hediffs.Count; i++)
			{
				Hediff_Addiction hediff_Addiction = hediffs[i] as Hediff_Addiction;
				if (hediff_Addiction != null && AddictionUtility.CanBingeOnNow(pawn, hediff_Addiction.Chemical, DrugCategory.Any))
				{
					addictions.Add(hediff_Addiction.Chemical);
				}
			}
			if (addictions.Count > 0)
			{
				chemical = addictions.RandomElement();
				drugCategory = DrugCategory.Any;
				addictions.Clear();
				return;
			}
			chemical = DefDatabase<ChemicalDef>.AllDefsListForReading.Where((ChemicalDef x) => AddictionUtility.CanBingeOnNow(pawn, x, def.drugCategory)).RandomElementWithFallback();
			if (chemical != null)
			{
				drugCategory = def.drugCategory;
				return;
			}
			chemical = DefDatabase<ChemicalDef>.AllDefsListForReading.Where((ChemicalDef x) => AddictionUtility.CanBingeOnNow(pawn, x, DrugCategory.Any)).RandomElementWithFallback();
			if (chemical != null)
			{
				drugCategory = DrugCategory.Any;
				return;
			}
			chemical = DefDatabase<ChemicalDef>.AllDefsListForReading.RandomElement();
			drugCategory = DrugCategory.Any;
		}
	}
}
