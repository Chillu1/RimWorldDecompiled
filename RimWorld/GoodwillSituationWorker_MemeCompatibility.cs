using Verse;

namespace RimWorld
{
	public class GoodwillSituationWorker_MemeCompatibility : GoodwillSituationWorker
	{
		public override string GetPostProcessedLabel(Faction other)
		{
			if (def.otherMeme == null)
			{
				if (Applies(Faction.OfPlayer, other))
				{
					return "MemeGoodwillImpact_Player".Translate(base.GetPostProcessedLabel(other));
				}
				return "MemeGoodwillImpact_Other".Translate(base.GetPostProcessedLabel(other));
			}
			return base.GetPostProcessedLabel(other);
		}

		public override int GetNaturalGoodwillOffset(Faction other)
		{
			if (!Applies(other))
			{
				return 0;
			}
			return def.naturalGoodwillOffset;
		}

		private bool Applies(Faction other)
		{
			if (!Applies(Faction.OfPlayer, other))
			{
				return Applies(other, Faction.OfPlayer);
			}
			return true;
		}

		private bool Applies(Faction a, Faction b)
		{
			Ideo primaryIdeo = a.ideos.PrimaryIdeo;
			if (primaryIdeo == null)
			{
				return false;
			}
			if (def.versusAll)
			{
				return primaryIdeo.memes.Contains(def.meme);
			}
			Ideo primaryIdeo2 = b.ideos.PrimaryIdeo;
			if (primaryIdeo2 == null)
			{
				return false;
			}
			if (primaryIdeo.memes.Contains(def.meme))
			{
				return primaryIdeo2.memes.Contains(def.otherMeme);
			}
			return false;
		}
	}
}
