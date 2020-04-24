using Verse;

namespace RimWorld
{
	public class CompQuality : ThingComp
	{
		private QualityCategory qualityInt = QualityCategory.Normal;

		public QualityCategory Quality => qualityInt;

		public void SetQuality(QualityCategory q, ArtGenerationContext source)
		{
			qualityInt = q;
			parent.TryGetComp<CompArt>()?.InitializeArt(source);
		}

		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Values.Look(ref qualityInt, "quality", QualityCategory.Awful);
		}

		public override void PostPostGeneratedForTrader(TraderKindDef trader, int forTile, Faction forFaction)
		{
			SetQuality(QualityUtility.GenerateQualityTraderItem(), ArtGenerationContext.Outsider);
		}

		public override bool AllowStackWith(Thing other)
		{
			if (other.TryGetQuality(out QualityCategory qc))
			{
				return qualityInt == qc;
			}
			return false;
		}

		public override void PostSplitOff(Thing piece)
		{
			base.PostSplitOff(piece);
			piece.TryGetComp<CompQuality>().qualityInt = qualityInt;
		}

		public override string CompInspectStringExtra()
		{
			return "QualityIs".Translate(Quality.GetLabel().CapitalizeFirst());
		}
	}
}
