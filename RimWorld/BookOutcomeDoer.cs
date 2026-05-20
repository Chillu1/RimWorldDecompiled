using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.Grammar;

namespace RimWorld;

public abstract class BookOutcomeDoer : ReadingOutcomeDoer
{
	private CompQuality compQualityCached;

	public new BookOutcomeProperties Props => (BookOutcomeProperties)props;

	public Book Book => (Book)base.Parent;

	private CompQuality CompQuality => compQualityCached ?? (compQualityCached = base.Parent.GetComp<CompQuality>());

	public QualityCategory Quality => CompQuality.Quality;

	public abstract bool DoesProvidesOutcome(Pawn reader);

	public virtual void OnBookGenerated(Pawn author = null)
	{
	}

	public virtual string GetBenefitsString(Pawn reader = null)
	{
		return "";
	}

	public virtual bool BenefitDetailsCanChange(Pawn reader = null)
	{
		return false;
	}

	public virtual IEnumerable<Dialog_InfoCard.Hyperlink> GetHyperlinks()
	{
		yield break;
	}

	public virtual IEnumerable<RulePack> GetTopicRulePacks()
	{
		return null;
	}

	public virtual IEnumerable<Rule_String> GetTopicRuleStrings()
	{
		return null;
	}

	public virtual IEnumerable<Gizmo> GetGizmos()
	{
		return Enumerable.Empty<Gizmo>();
	}

	public virtual void Reset()
	{
	}
}
