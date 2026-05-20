using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace RimWorld;

public class Precept_Relic : Precept_ThingStyle
{
	private static List<ThingDef> usedThingsTmp = new List<ThingDef>();

	public ThingDef stuff;

	private bool relicGenerated;

	private Thing generatedRelic;

	private string tipLabelCached;

	public override string TipLabel
	{
		get
		{
			if (tipLabelCached == null)
			{
				tipLabelCached = base.LabelCap + "\n" + base.ThingDef.LabelCap;
			}
			return tipLabelCached;
		}
	}

	public override bool CanRegenerate => false;

	public bool CanGenerateRelic => !relicGenerated;

	public Thing GeneratedRelic => generatedRelic;

	protected override string ThingLabelCap => Find.ActiveLanguageWorker.PostProcessThingLabelForRelic(base.ThingLabelCap);

	public override string DescriptionForTip => base.ThingDef?.description ?? base.Description;

	public bool RelicInPlayerPossession
	{
		get
		{
			Thing thing = GeneratedRelic;
			if (thing == null || thing.Destroyed || !thing.EverSeenByPlayer || (thing.MapHeld == null && !ThingOwnerUtility.AnyParentIs<Caravan>(thing) && !ThingOwnerUtility.AnyParentIs<TravellingTransporters>(thing)))
			{
				return false;
			}
			return true;
		}
	}

	protected override string NameRootSymbol => "r_ideoRelicName";

	public static ThingDef GenerateStuffFor(ThingDef thing, Ideo ideo = null)
	{
		IEnumerable<ThingDef> source = GenStuff.AllowedStuffsFor(thing);
		if (ideo != null)
		{
			IEnumerable<ThingDef> alreadyUsedStuffs = (from p in ideo.PreceptsListForReading
				where p is Precept_Relic precept_Relic && precept_Relic.ThingDef == thing
				select ((Precept_Relic)p).stuff into p
				where p != null
				select p).Distinct();
			source = source.Where((ThingDef stuff) => !alreadyUsedStuffs.Contains(stuff));
		}
		return source.RandomElementByWeight((ThingDef stuff) => stuff.BaseMarketValue);
	}

	public void SetRandomStuff()
	{
		if (base.ThingDef.MadeFromStuff)
		{
			stuff = GenerateStuffFor(base.ThingDef, ideo);
		}
		else
		{
			stuff = null;
		}
	}

	protected override void Notify_ThingDefSet()
	{
		base.Notify_ThingDefSet();
		SetRandomStuff();
	}

	public override string GenerateNameRaw()
	{
		CompProperties_GeneratedName compProperties = base.ThingDef.GetCompProperties<CompProperties_GeneratedName>();
		if (compProperties != null)
		{
			return CompGeneratedNames.GenerateName(compProperties);
		}
		return base.GenerateNameRaw();
	}

	public Thing GenerateRelic()
	{
		if (stuff == null && base.ThingDef.MadeFromStuff)
		{
			Log.Warning("Tried generating relic with stuff, but precept had no stuff set? If this is an old savegame from testing this warning is to be expected.");
			stuff = GenerateStuffFor(base.ThingDef, ideo);
		}
		Thing thing = ((base.ThingDef.CompDefFor<CompQuality>() == null) ? ThingMaker.MakeThing(base.ThingDef, stuff) : new ThingStuffPairWithQuality(base.ThingDef, stuff, QualityCategory.Legendary).MakeThing(forceQuality: true));
		thing.StyleSourcePrecept = this;
		relicGenerated = true;
		generatedRelic = thing;
		return thing;
	}

	public override void ClearTipCache()
	{
		base.ClearTipCache();
		tipLabelCached = null;
	}

	public override IEnumerable<FloatMenuOption> EditFloatMenuOptions()
	{
		yield return EditFloatMenuOption();
	}

	public override string GetTip()
	{
		return base.GetTip() + "\n\n" + (ideo.classicMode ? "RelicTipClassic" : "RelicTip").Translate().Colorize(Color.grey);
	}

	public override void DrawIcon(Rect rect)
	{
		Widgets.DefIcon(rect, base.ThingDef, stuff, 1f, ideo.GetStyleFor(base.ThingDef));
	}

	public override void Notify_ThingLost(Thing thing, bool destroyed = false)
	{
		if (thing.def != base.ThingDef || QuestPart_NewColony.IsGeneratingNewColony)
		{
			return;
		}
		Find.HistoryEventsManager.RecordEvent(new HistoryEvent(destroyed ? HistoryEventDefOf.RelicDestroyed : HistoryEventDefOf.RelicLost, thing.Named("SUBJECT")));
		List<Faction> list = new List<Faction>();
		HashSet<Pawn> hashSet = new HashSet<Pawn>();
		if (thing.EverSeenByPlayer)
		{
			list.Add(Faction.OfPlayer);
		}
		if (thing.MapHeld != null)
		{
			foreach (Pawn item in thing.MapHeld.mapPawns.AllPawnsSpawned)
			{
				if (!item.Dead && item.Faction != null && !list.Contains(item.Faction))
				{
					list.Add(item.Faction);
				}
			}
		}
		foreach (Pawn item2 in PawnsFinder.AllMapsAndWorld_Alive)
		{
			if (item2.needs != null && item2.needs.mood != null && item2.needs.mood.thoughts != null && item2.needs.mood.thoughts.memories != null && !hashSet.Contains(item2) && list.Contains(item2.Faction))
			{
				item2.needs.mood.thoughts.memories.TryGainMemory(destroyed ? ThoughtDefOf.RelicDestroyed : ThoughtDefOf.RelicLost);
				if (item2.Faction == Faction.OfPlayer)
				{
					hashSet.Add(item2);
				}
			}
		}
		if (list.Contains(Faction.OfPlayer) && (destroyed || thing.MapHeld == null || !thing.MapHeld.IsPlayerHome))
		{
			TaggedString label = (destroyed ? "LetterLabelRelicDestroyed" : "LetterLabelRelicLost").Translate() + ": " + base.LabelCap;
			TaggedString text = (destroyed ? "LetterTextRelicDestroyed" : "LetterTextRelicLost").Translate(base.LabelCap, hashSet.Select((Pawn p) => p.LabelNoCountColored.Resolve()).ToList().ToLineList("- "));
			Find.LetterStack.ReceiveLetter(label, text, LetterDefOf.NegativeEvent, new LookTargets(hashSet.Where((Pawn p) => p.Faction == Faction.OfPlayer)));
		}
	}

	public void Notify_NewColonyStarted()
	{
		if (relicGenerated && !RelicInPlayerPossession)
		{
			relicGenerated = false;
		}
	}

	public override IEnumerable<StatDrawEntry> SpecialDisplayStats(Thing thing)
	{
		if (ideo.classicMode)
		{
			yield return new StatDrawEntry(StatCategoryDefOf.BasicsImportant, "Stat_Thing_RelicStatus".Translate(), "Relic".Translate().CapitalizeFirst(), "Stat_Thing_RelicStatus_Desc".Translate(), 1109);
			yield break;
		}
		yield return new StatDrawEntry(StatCategoryDefOf.BasicsImportant, "Stat_Thing_RelicOf_Name".Translate(), ideo.name, "Stat_Thing_RelicOf_Desc".Translate(), 1109, null, new Dialog_InfoCard.Hyperlink[1]
		{
			new Dialog_InfoCard.Hyperlink(ideo)
		});
	}

	public override string TransformThingLabel(string label)
	{
		return name + ", " + "Relic".Translate();
	}

	public override string InspectStringExtra(Thing thing)
	{
		if (ideo.classicMode)
		{
			return "Relic".Translate().CapitalizeFirst();
		}
		return "RelicOf".Translate(ideo.Named("IDEO")).Resolve();
	}

	private IEnumerable<Quest> GetQuestsToDeactivateIfLost()
	{
		List<Quest> allQuests = Find.QuestManager.QuestsListForReading;
		for (int i = 0; i < allQuests.Count; i++)
		{
			if (allQuests[i].Historical)
			{
				continue;
			}
			List<QuestPart> partsListForReading = allQuests[i].PartsListForReading;
			for (int j = 0; j < partsListForReading.Count; j++)
			{
				if (partsListForReading[j] is QuestPart_SubquestGenerator_RelicHunt questPart_SubquestGenerator_RelicHunt && questPart_SubquestGenerator_RelicHunt.relic == this)
				{
					yield return allQuests[i];
					break;
				}
			}
		}
	}

	public override bool TryGetLostByReformingWarning(out string warning)
	{
		warning = null;
		IEnumerable<Quest> questsToDeactivateIfLost = GetQuestsToDeactivateIfLost();
		if (questsToDeactivateIfLost.Any())
		{
			foreach (Quest item in questsToDeactivateIfLost)
			{
				if (!warning.NullOrEmpty())
				{
					warning += "\n\n";
				}
				warning += "ReformIdeoRelicHuntQuestEnd".Translate(base.LabelCap, item.name);
			}
			return true;
		}
		return false;
	}

	public override void Notify_RemovedByReforming()
	{
		foreach (Quest item in GetQuestsToDeactivateIfLost())
		{
			item.hidden = true;
			item.End(QuestEndOutcome.Unknown, sendLetter: false);
		}
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Defs.Look(ref stuff, "stuff");
		if (!GameDataSaveLoader.IsSavingOrLoadingExternalIdeo)
		{
			if (Scribe.mode == LoadSaveMode.Saving && relicGenerated && generatedRelic != null && (generatedRelic.Destroyed || generatedRelic.Discarded))
			{
				relicGenerated = false;
				generatedRelic = null;
			}
			Scribe_Values.Look(ref relicGenerated, "everGenerated", defaultValue: false);
			Scribe_References.Look(ref generatedRelic, "generatedRelic");
		}
	}

	public override void CopyTo(Precept other)
	{
		base.CopyTo(other);
		Precept_Relic obj = (Precept_Relic)other;
		obj.stuff = stuff;
		obj.relicGenerated = relicGenerated;
		obj.generatedRelic = generatedRelic;
	}

	public override string ToString()
	{
		return "Relic - " + base.LabelCap;
	}
}
