using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld;

public sealed class EntityCodex : IExposable
{
	private Dictionary<EntityCategoryDef, bool> hiddenCategories;

	private Dictionary<EntityCodexEntryDef, bool> hiddenEntries;

	private HashSet<ThingDef> discoveredEntities;

	public bool debug_UnhideAllResearch;

	public const string EntityDiscoveredSignal = "EntityDiscovered";

	private const int EntityDiscoveredLetterDelayTicks = 600;

	private const int MonolithInteractableLetterDelayTicks = 900;

	public EntityCodex()
	{
		hiddenCategories = new Dictionary<EntityCategoryDef, bool>();
		hiddenEntries = new Dictionary<EntityCodexEntryDef, bool>();
		discoveredEntities = new HashSet<ThingDef>();
		foreach (EntityCategoryDef allDef in DefDatabase<EntityCategoryDef>.AllDefs)
		{
			hiddenCategories.Add(allDef, value: true);
		}
	}

	public static int EntryCountInCategory(EntityCategoryDef def)
	{
		int num = 0;
		foreach (EntityCodexEntryDef allDef in DefDatabase<EntityCodexEntryDef>.AllDefs)
		{
			if (allDef.category == def)
			{
				num++;
			}
		}
		return num;
	}

	public int DiscoveredCount(EntityCategoryDef def)
	{
		int num = 0;
		foreach (EntityCodexEntryDef allDef in DefDatabase<EntityCodexEntryDef>.AllDefs)
		{
			if (allDef.category == def && Find.EntityCodex.Discovered(allDef))
			{
				num++;
			}
		}
		return num;
	}

	public bool Hidden(ResearchProjectDef def)
	{
		if (!ModsConfig.AnomalyActive)
		{
			return false;
		}
		if (debug_UnhideAllResearch)
		{
			return false;
		}
		if (def.knowledgeCategory != null && Find.Anomaly.HighestLevelReached < 1 && Find.Anomaly.GenerateMonolith)
		{
			return true;
		}
		foreach (EntityCodexEntryDef allDef in DefDatabase<EntityCodexEntryDef>.AllDefs)
		{
			if (allDef.discoveredResearchProjects.NotNullAndContains(def))
			{
				return !Discovered(allDef);
			}
		}
		return false;
	}

	public bool Discovered(EntityCategoryDef def)
	{
		if (!ModLister.CheckAnomaly("Entity codex"))
		{
			return false;
		}
		if (hiddenCategories.TryGetValue(def, out var value))
		{
			return !value;
		}
		return false;
	}

	public bool Discovered(EntityCodexEntryDef entry)
	{
		if (!ModLister.CheckAnomaly("Entity codex"))
		{
			return false;
		}
		if (!Discovered(entry.category))
		{
			return false;
		}
		if (hiddenEntries.TryGetValue(entry, out var value))
		{
			return !value;
		}
		return false;
	}

	public void SetDiscovered(List<EntityCodexEntryDef> entries, ThingDef discoveredDef = null, Thing discoveredThing = null)
	{
		foreach (EntityCodexEntryDef entry in entries)
		{
			SetDiscovered(entry, discoveredDef, discoveredThing);
		}
		Find.HiddenItemsManager.SetDiscovered(discoveredDef);
	}

	public void SetDiscovered(EntityCodexEntryDef entry, ThingDef discoveredDef = null, Thing discoveredThing = null)
	{
		if (!ModLister.CheckAnomaly("Entity codex"))
		{
			return;
		}
		if (discoveredDef != null)
		{
			discoveredEntities.Add(discoveredDef);
		}
		if (Discovered(entry))
		{
			return;
		}
		hiddenEntries[entry] = false;
		hiddenCategories[entry.category] = false;
		Find.SignalManager.SendSignal(new Signal("EntityDiscovered", global: true));
		if (Current.ProgramState != ProgramState.Playing || !Find.Anomaly.AnomalyStudyEnabled)
		{
			return;
		}
		if (discoveredThing != null)
		{
			Messages.Message(string.Format("{0}: {1}", "MessageEntityDiscovered".Translate(), entry.LabelCap), discoveredThing, MessageTypeDefOf.PositiveEvent);
		}
		if (!entry.discoveredResearchProjects.NullOrEmpty())
		{
			string text = string.Empty;
			if (!entry.discoveredResearchProjects.NullOrEmpty())
			{
				string arg = discoveredThing?.LabelShort ?? discoveredDef?.label ?? entry.label;
				if (discoveredThing is Pawn { IsMutant: not false } pawn)
				{
					arg = pawn.mutant.Def.label;
				}
				text += "LetterTextEntityDiscoveryResearch".Translate(arg.Named("ENTITY")) + ":\n" + ((IEnumerable<ResearchProjectDef>)entry.discoveredResearchProjects).Select((Func<ResearchProjectDef, string>)((ResearchProjectDef r) => r.LabelCap)).ToLineList("  - ");
			}
			if (!text.NullOrEmpty())
			{
				ChoiceLetter_EntityDiscovered choiceLetter_EntityDiscovered = (ChoiceLetter_EntityDiscovered)LetterMaker.MakeLetter("LetterLabelEntityDiscovery".Translate(), text, LetterDefOf.EntityDiscovered);
				choiceLetter_EntityDiscovered.codexEntry = entry;
				if (discoveredThing != null)
				{
					choiceLetter_EntityDiscovered.lookTargets = discoveredThing;
				}
				Find.LetterStack.ReceiveLetter(choiceLetter_EntityDiscovered, null, 600);
			}
		}
		if (Find.Anomaly.GenerateMonolith && Find.Anomaly.monolith != null)
		{
			MonolithLevelDef levelDef = Find.Anomaly.LevelDef;
			if (levelDef.level > Find.Anomaly.lastLevelActivationLetterSent && Find.Anomaly.monolith.CanActivate(out var _, out var _) && !levelDef.activatableLetterLabel.NullOrEmpty() && !levelDef.activatableLetterText.NullOrEmpty())
			{
				Find.Anomaly.lastLevelActivationLetterSent = levelDef.level;
				Find.LetterStack.ReceiveLetter(levelDef.activatableLetterLabel, levelDef.activatableLetterText, LetterDefOf.PositiveEvent, Find.Anomaly.monolith, null, null, null, null, 900);
			}
		}
	}

	public bool Discovered(ThingDef def)
	{
		if (ModLister.CheckAnomaly("Entity codex"))
		{
			return discoveredEntities.Contains(def);
		}
		return false;
	}

	public void Debug_DiscoverAll()
	{
		foreach (EntityCodexEntryDef allDef in DefDatabase<EntityCodexEntryDef>.AllDefs)
		{
			SetDiscovered(allDef);
		}
		foreach (ThingDef allDef2 in DefDatabase<ThingDef>.AllDefs)
		{
			if (allDef2.entityCodexEntry != null)
			{
				discoveredEntities.Add(allDef2);
			}
		}
		foreach (EntityCategoryDef allDef3 in DefDatabase<EntityCategoryDef>.AllDefs)
		{
			hiddenCategories[allDef3] = false;
		}
	}

	public void Reset()
	{
		hiddenCategories.Clear();
		hiddenEntries.Clear();
		discoveredEntities.Clear();
		foreach (EntityCategoryDef allDef in DefDatabase<EntityCategoryDef>.AllDefs)
		{
			hiddenCategories.Add(allDef, value: true);
		}
		foreach (EntityCodexEntryDef allDef2 in DefDatabase<EntityCodexEntryDef>.AllDefs)
		{
			hiddenEntries.Add(allDef2, !allDef2.startDiscovered);
		}
	}

	public void ExposeData()
	{
		Scribe_Collections.Look(ref hiddenCategories, "hiddenCategories", LookMode.Def, LookMode.Value);
		Scribe_Collections.Look(ref hiddenEntries, "hiddenEntries", LookMode.Def, LookMode.Value);
		Scribe_Collections.Look(ref discoveredEntities, "discoveredEntities", LookMode.Def);
		Scribe_Values.Look(ref debug_UnhideAllResearch, "debug_UnhideAllResearch", defaultValue: false);
		if (Scribe.mode != LoadSaveMode.PostLoadInit)
		{
			return;
		}
		if (discoveredEntities == null)
		{
			discoveredEntities = new HashSet<ThingDef>();
		}
		Dictionary<EntityCategoryDef, bool> dictionary = new Dictionary<EntityCategoryDef, bool>();
		foreach (EntityCategoryDef allDef in DefDatabase<EntityCategoryDef>.AllDefs)
		{
			dictionary.Add(allDef, value: true);
		}
		foreach (KeyValuePair<EntityCategoryDef, bool> hiddenCategory in hiddenCategories)
		{
			if (dictionary.ContainsKey(hiddenCategory.Key))
			{
				dictionary[hiddenCategory.Key] = hiddenCategory.Value;
			}
		}
		hiddenCategories = dictionary;
		Dictionary<EntityCodexEntryDef, bool> dictionary2 = new Dictionary<EntityCodexEntryDef, bool>();
		foreach (EntityCodexEntryDef allDef2 in DefDatabase<EntityCodexEntryDef>.AllDefs)
		{
			dictionary2.Add(allDef2, !allDef2.startDiscovered);
		}
		foreach (KeyValuePair<EntityCodexEntryDef, bool> hiddenEntry in hiddenEntries)
		{
			if (dictionary2.ContainsKey(hiddenEntry.Key))
			{
				dictionary2[hiddenEntry.Key] = hiddenEntry.Value;
			}
		}
		hiddenEntries = dictionary2;
		foreach (ThingDef discoveredEntity in discoveredEntities)
		{
			Find.HiddenItemsManager.SetDiscovered(discoveredEntity);
		}
	}
}
