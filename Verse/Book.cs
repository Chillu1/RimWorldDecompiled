using System.Collections.Generic;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse.AI;
using Verse.Grammar;

namespace Verse;

public class Book : ThingWithComps
{
	private struct BookSubjectSymbol
	{
		public string keyword;

		public List<(string, string)> subSymbols;
	}

	private string title;

	private bool isOpen;

	private bool descCanBeInvalidated;

	private float mentalBreakChancePerHour;

	private float joyFactor = 1f;

	private string descriptionFlavor;

	private string description;

	private Graphic openGraphic;

	private Graphic verticalGraphic;

	private CompBook cachedComp;

	private static List<BookSubjectSymbol> subjects = new List<BookSubjectSymbol>();

	public CompBook BookComp => cachedComp ?? (cachedComp = GetComp<CompBook>());

	private Graphic OpenGraphic => openGraphic ?? (openGraphic = BookComp.Props.openGraphic.Graphic);

	public Graphic VerticalGraphic => verticalGraphic ?? (verticalGraphic = BookComp.Props.verticalGraphic.Graphic);

	public override string LabelNoCount => title + GenLabel.LabelExtras(this, includeHp: true, includeQuality: true);

	public override string LabelNoParenthesis => title;

	public string FlavorUI => descriptionFlavor;

	public float MentalBreakChancePerHour => mentalBreakChancePerHour;

	public float JoyFactor => joyFactor;

	public string Title => title;

	public bool IsOpen
	{
		get
		{
			return isOpen;
		}
		set
		{
			isOpen = value;
		}
	}

	public virtual bool IsReadable => true;

	public override string DescriptionFlavor => DescriptionDetailed;

	public override string DescriptionDetailed
	{
		get
		{
			EnsureDescriptionUpToDate();
			return description;
		}
	}

	public override void PostPostMake()
	{
		base.PostPostMake();
		if (!this.HasComp<CompQuality>())
		{
			GenerateBook();
		}
	}

	public override void SpawnSetup(Map map, bool respawningAfterLoad)
	{
		base.SpawnSetup(map, respawningAfterLoad);
		LessonAutoActivator.TeachOpportunity(ConceptDefOf.Books, OpportunityType.GoodToKnow);
	}

	public override bool CanStackWith(Thing other)
	{
		return false;
	}

	protected override void DrawAt(Vector3 drawLoc, bool flip = false)
	{
		if (isOpen)
		{
			Rot4 rot = ((!(base.ParentHolder is Pawn_CarryTracker pawn_CarryTracker)) ? base.Rotation : pawn_CarryTracker.pawn.Rotation);
			OpenGraphic.Draw(drawLoc, flip ? rot.Opposite : rot, this);
		}
		else
		{
			base.DrawAt(drawLoc, flip);
		}
	}

	public void PawnReadNow(Pawn pawn)
	{
		Job job = JobMaker.MakeJob(JobDefOf.Reading, this);
		pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
	}

	public override void PostQualitySet()
	{
		base.PostQualitySet();
		GenerateBook();
	}

	public void OnBookReadTick(Pawn pawn, int delta, float roomBonusFactor)
	{
		float factor = pawn.GetStatValue(StatDefOf.ReadingSpeed) * roomBonusFactor * (float)delta;
		foreach (BookOutcomeDoer doer in BookComp.Doers)
		{
			doer.OnReadingTick(pawn, factor);
		}
		if (ModsConfig.AnomalyActive && MentalBreakChancePerHour > 0f && Rand.MTBEventOccurs(1f / MentalBreakChancePerHour, 2500f, delta) && pawn.mindState.mentalBreaker.TryGetRandomMentalBreak(BookComp.Props.mentalBreakIntensity, out var breakDef))
		{
			TaggedString taggedString = "BookMentalBreakMessage".Translate(Label);
			pawn.mindState.mentalBreaker.TryDoMentalBreak(taggedString, breakDef);
		}
	}

	public bool ProvidesOutcome(Pawn reader)
	{
		foreach (BookOutcomeDoer doer in BookComp.Doers)
		{
			if (doer.DoesProvidesOutcome(reader))
			{
				return true;
			}
		}
		return false;
	}

	public void SetMentalBreakChance(float chance)
	{
		mentalBreakChancePerHour = Mathf.Clamp01(chance);
	}

	public void SetJoyFactor(float factor)
	{
		joyFactor = factor;
	}

	public virtual IEnumerable<Dialog_InfoCard.Hyperlink> GetHyperlinks()
	{
		foreach (BookOutcomeDoer doer in BookComp.Doers)
		{
			foreach (Dialog_InfoCard.Hyperlink hyperlink in doer.GetHyperlinks())
			{
				yield return hyperlink;
			}
		}
	}

	public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn selPawn)
	{
		FloatMenuOption floatMenuOption = new FloatMenuOption("AssignReadNow".Translate(Label), delegate
		{
			PawnReadNow(selPawn);
		});
		if (!BookUtility.CanReadBook(this, selPawn, out var reason))
		{
			floatMenuOption.Label = string.Format("{0}: {1}", "AssignCannotReadNow".Translate(Label), reason);
			floatMenuOption.Disabled = true;
		}
		Pawn pawn = selPawn.Map.reservationManager.FirstRespectedReserver(this, selPawn) ?? selPawn.Map.physicalInteractionReservationManager.FirstReserverOf(this);
		if (pawn != null)
		{
			floatMenuOption.Label += " (" + "ReservedBy".Translate(pawn.LabelShort, pawn) + ")";
		}
		floatMenuOption.iconThing = this;
		yield return floatMenuOption;
		foreach (FloatMenuOption floatMenuOption2 in base.GetFloatMenuOptions(selPawn))
		{
			floatMenuOption2.iconThing = this;
			yield return floatMenuOption2;
		}
	}

	public override IEnumerable<StatDrawEntry> SpecialDisplayStats()
	{
		foreach (StatDrawEntry item in base.SpecialDisplayStats())
		{
			yield return item;
		}
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine();
		stringBuilder.AppendLine("Stat_RecreationType_Desc".Translate());
		stringBuilder.AppendLine();
		foreach (JoyKindDef allDef in DefDatabase<JoyKindDef>.AllDefs)
		{
			stringBuilder.AppendLine("  - " + allDef.LabelCap);
		}
		JoyKindDef reading = JoyKindDefOf.Reading;
		yield return new StatDrawEntry(StatCategoryDefOf.Basics, "StatsReport_JoyKind".Translate(), reading.LabelCap, stringBuilder.ToString(), 4750, reading.LabelCap);
	}

	public virtual void GenerateBook(Pawn author = null, long? fixedDate = null)
	{
		mentalBreakChancePerHour = 0f;
		subjects.Clear();
		GrammarRequest common = default(GrammarRequest);
		long absTicks = fixedDate ?? (GenTicks.TicksAbs - (long)(BookComp.Props.ageYearsRange.RandomInRange * 3600000f));
		common.Rules.Add(new Rule_String("date", GenDate.DateFullStringAt(absTicks, Vector2.zero)));
		common.Rules.Add(new Rule_String("date_season", GenDate.DateMonthYearStringAt(absTicks, Vector2.zero)));
		if (this.HasComp<CompQuality>())
		{
			common.Constants.Add("quality", ((int)GetComp<CompQuality>().Quality).ToString());
		}
		foreach (Rule rule in ((author == null) ? TaleData_Pawn.GenerateRandom(humanLike: true) : TaleData_Pawn.GenerateFrom(author)).GetRules("ANYPAWN", common.Constants))
		{
			common.Rules.Add(rule);
		}
		AppendDoerRules(author, ref common);
		AppendRulesForSubject(subjects, common.Rules, common.Constants, "primary", 0);
		AppendRulesForSubject(subjects, common.Rules, common.Constants, "secondary", 1);
		AppendRulesForSubject(subjects, common.Rules, common.Constants, "tertiary", 2);
		GrammarRequest request = common;
		request.Includes.Add(BookComp.Props.nameMaker);
		title = GenText.CapitalizeAsTitle(GrammarResolver.Resolve("title", request)).StripTags();
		GrammarRequest request2 = common;
		request2.Includes.Add(BookComp.Props.descriptionMaker);
		request2.Includes.Add(RulePackDefOf.TalelessImages);
		request2.Includes.Add(RulePackDefOf.ArtDescriptionRoot_Taleless);
		request2.Includes.Add(RulePackDefOf.ArtDescriptionUtility_Global);
		descriptionFlavor = GrammarResolver.Resolve("desc", request2).StripTags();
		description = GenerateFullDescription();
		subjects.Clear();
	}

	private void AppendDoerRules(Pawn author, ref GrammarRequest common)
	{
		foreach (BookOutcomeDoer doer in BookComp.Doers)
		{
			doer.Reset();
			doer.OnBookGenerated(author);
			IEnumerable<RulePack> topicRulePacks = doer.GetTopicRulePacks();
			if (topicRulePacks != null)
			{
				foreach (RulePack item in topicRulePacks)
				{
					common.IncludesBare.Add(item);
					List<(string, string)> list = new List<(string, string)>();
					foreach (Rule rule in item.Rules)
					{
						if (rule.keyword.StartsWith("subject_"))
						{
							list.Add((rule.keyword.Substring("subject_".Length), GrammarResolver.Resolve(rule.keyword, common, null, forceLog: false, null, null, null, capitalizeFirstSentence: false)));
						}
					}
					subjects.Add(new BookSubjectSymbol
					{
						keyword = GrammarResolver.Resolve("subject", common, null, forceLog: false, null, null, null, capitalizeFirstSentence: false),
						subSymbols = list
					});
				}
			}
			IEnumerable<Rule_String> topicRuleStrings = doer.GetTopicRuleStrings();
			if (topicRuleStrings == null)
			{
				continue;
			}
			foreach (Rule_String item2 in topicRuleStrings)
			{
				common.Rules.Add(item2);
			}
		}
	}

	private void EnsureDescriptionUpToDate()
	{
		if (descCanBeInvalidated)
		{
			description = GenerateFullDescription();
		}
	}

	private string GenerateFullDescription()
	{
		StringBuilder stringBuilder = new StringBuilder();
		descCanBeInvalidated = false;
		stringBuilder.AppendLine(title.Colorize(ColoredText.TipSectionTitleColor) + GenLabel.LabelExtras(this, includeHp: false, includeQuality: true) + "\n");
		stringBuilder.AppendLine(descriptionFlavor + "\n");
		if (MentalBreakChancePerHour > 0f)
		{
			stringBuilder.AppendLine(string.Format(" - {0}: {1}", "BookMentalBreak".Translate(), "PerHour".Translate(MentalBreakChancePerHour.ToStringPercent("0.0"))));
		}
		foreach (BookOutcomeDoer doer in BookComp.Doers)
		{
			string benefitsString = doer.GetBenefitsString();
			if (!string.IsNullOrEmpty(benefitsString))
			{
				if (doer.BenefitDetailsCanChange())
				{
					descCanBeInvalidated = true;
				}
				stringBuilder.AppendLine(benefitsString);
			}
		}
		return stringBuilder.ToString().TrimEndNewlines();
	}

	private static void AppendRulesForSubject(List<BookSubjectSymbol> subjects, List<Rule> rules, Dictionary<string, string> constants, string postfix, int i)
	{
		if (i < subjects.Count)
		{
			rules.Add(new Rule_String("subject_" + postfix, subjects[i].keyword));
			constants.Add("length_subject_" + postfix, subjects[i].keyword.Length.ToString());
			constants.Add("has_subject_" + postfix, "true");
			{
				foreach (var subSymbol in subjects[i].subSymbols)
				{
					rules.Add(new Rule_String("subject_" + postfix + "_" + subSymbol.Item1, subSymbol.Item2));
				}
				return;
			}
		}
		constants.Add("has_subject_" + postfix, "false");
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref title, "title");
		Scribe_Values.Look(ref descriptionFlavor, "descriptionFlavor");
		Scribe_Values.Look(ref mentalBreakChancePerHour, "mentalBreakChancePerHour", 0f);
		Scribe_Values.Look(ref joyFactor, "joyFactor", 0f);
		Scribe_Values.Look(ref isOpen, "isOpen", defaultValue: false);
		Scribe_Values.Look(ref descCanBeInvalidated, "descCanBeInvalidated", defaultValue: false);
		Scribe_Values.Look(ref description, "description");
	}
}
