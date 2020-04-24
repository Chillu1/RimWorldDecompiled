using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class QuestPart_Hyperlinks : QuestPart
	{
		public List<ThingDef> thingDefs = new List<ThingDef>();

		public List<Pawn> pawns = new List<Pawn>();

		private IEnumerable<Dialog_InfoCard.Hyperlink> cachedHyperlinks;

		public override IEnumerable<Dialog_InfoCard.Hyperlink> Hyperlinks
		{
			get
			{
				if (cachedHyperlinks == null)
				{
					cachedHyperlinks = GetHyperlinks();
				}
				return cachedHyperlinks;
			}
		}

		private IEnumerable<Dialog_InfoCard.Hyperlink> GetHyperlinks()
		{
			if (thingDefs != null)
			{
				for (int j = 0; j < thingDefs.Count; j++)
				{
					yield return new Dialog_InfoCard.Hyperlink(thingDefs[j]);
				}
			}
			if (pawns == null)
			{
				yield break;
			}
			for (int j = 0; j < pawns.Count; j++)
			{
				if (pawns[j].royalty != null && pawns[j].royalty.AllTitlesForReading.Any())
				{
					RoyalTitle mostSeniorTitle = pawns[j].royalty.MostSeniorTitle;
					if (mostSeniorTitle != null)
					{
						yield return new Dialog_InfoCard.Hyperlink(mostSeniorTitle.def, mostSeniorTitle.faction);
					}
				}
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Collections.Look(ref thingDefs, "thingDefs", LookMode.Undefined);
			Scribe_Collections.Look(ref pawns, "pawns", LookMode.Reference);
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				if (thingDefs == null)
				{
					thingDefs = new List<ThingDef>();
				}
				thingDefs.RemoveAll((ThingDef x) => x == null);
				if (pawns == null)
				{
					pawns = new List<Pawn>();
				}
				pawns.RemoveAll((Pawn x) => x == null);
			}
		}

		public override void ReplacePawnReferences(Pawn replace, Pawn with)
		{
			pawns.Replace(replace, with);
		}
	}
}
