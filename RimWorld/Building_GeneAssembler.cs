using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld;

[StaticConstructorOnStartup]
public class Building_GeneAssembler : Building, IThingHolder
{
	public ThingOwner innerContainer;

	private List<Genepack> genepacksToRecombine;

	private int architesRequired;

	private bool workingInt;

	private int lastWorkedTick = -999;

	private float workDone;

	private float totalWorkRequired;

	public string xenotypeName;

	public XenotypeIconDef iconDef;

	[Unsaved(false)]
	private float lastWorkAmount = -1f;

	[Unsaved(false)]
	private CompPowerTrader cachedPowerComp;

	[Unsaved(false)]
	private List<Genepack> tmpGenepacks = new List<Genepack>();

	[Unsaved(false)]
	private HashSet<Thing> tmpUsedFacilities = new HashSet<Thing>();

	[Unsaved(false)]
	private int? cachedComplexity;

	private const int CheckContainersInterval = 180;

	private static readonly Texture2D CancelIcon = ContentFinder<Texture2D>.Get("UI/Designators/Cancel");

	private static readonly CachedTexture RecombineIcon = new CachedTexture("UI/Gizmos/RecombineGenes");

	public float ProgressPercent => workDone / totalWorkRequired;

	public bool Working => workingInt;

	private CompPowerTrader PowerTraderComp => cachedPowerComp ?? (cachedPowerComp = this.TryGetComp<CompPowerTrader>());

	public bool PowerOn => PowerTraderComp.PowerOn;

	public List<Thing> ConnectedFacilities => this.TryGetComp<CompAffectedByFacilities>().LinkedFacilitiesListForReading;

	public int ArchitesCount
	{
		get
		{
			int num = 0;
			for (int i = 0; i < innerContainer.Count; i++)
			{
				if (innerContainer[i].def == ThingDefOf.ArchiteCapsule)
				{
					num += innerContainer[i].stackCount;
				}
			}
			return num;
		}
	}

	public int ArchitesRequiredNow => architesRequired - ArchitesCount;

	private HashSet<Thing> UsedFacilities
	{
		get
		{
			tmpUsedFacilities.Clear();
			if (!genepacksToRecombine.NullOrEmpty())
			{
				List<Thing> connectedFacilities = ConnectedFacilities;
				for (int i = 0; i < genepacksToRecombine.Count; i++)
				{
					for (int j = 0; j < connectedFacilities.Count; j++)
					{
						if (!tmpUsedFacilities.Contains(connectedFacilities[j]))
						{
							CompGenepackContainer compGenepackContainer = connectedFacilities[j].TryGetComp<CompGenepackContainer>();
							if (compGenepackContainer != null && compGenepackContainer.ContainedGenepacks.Contains(genepacksToRecombine[i]))
							{
								tmpUsedFacilities.Add(connectedFacilities[j]);
								break;
							}
						}
					}
				}
			}
			return tmpUsedFacilities;
		}
	}

	public AcceptanceReport CanBeWorkedOnNow
	{
		get
		{
			if (!Working)
			{
				return false;
			}
			if (ArchitesRequiredNow > 0)
			{
				return false;
			}
			if (!PowerOn)
			{
				return "NoPower".Translate().CapitalizeFirst();
			}
			foreach (Thing usedFacility in UsedFacilities)
			{
				CompPowerTrader compPowerTrader = usedFacility.TryGetComp<CompPowerTrader>();
				if (compPowerTrader != null && !compPowerTrader.PowerOn)
				{
					return "GenebankUnpowered".Translate();
				}
			}
			if (MaxComplexity() < TotalGCX)
			{
				return "GeneProcessorUnpowered".Translate();
			}
			return true;
		}
	}

	private int TotalGCX
	{
		get
		{
			if (!Working)
			{
				return 0;
			}
			if (!cachedComplexity.HasValue)
			{
				cachedComplexity = 0;
				if (!genepacksToRecombine.NullOrEmpty())
				{
					List<GeneDefWithType> list = new List<GeneDefWithType>();
					for (int i = 0; i < genepacksToRecombine.Count; i++)
					{
						if (genepacksToRecombine[i].GeneSet != null)
						{
							for (int j = 0; j < genepacksToRecombine[i].GeneSet.GenesListForReading.Count; j++)
							{
								list.Add(new GeneDefWithType(genepacksToRecombine[i].GeneSet.GenesListForReading[j], xenogene: true));
							}
						}
					}
					List<GeneDef> list2 = list.NonOverriddenGenes();
					for (int k = 0; k < list2.Count; k++)
					{
						cachedComplexity += list2[k].biostatCpx;
					}
				}
			}
			return cachedComplexity.Value;
		}
	}

	public override void PostPostMake()
	{
		if (!ModLister.CheckBiotech("Gene assembler"))
		{
			Destroy();
			return;
		}
		base.PostPostMake();
		innerContainer = new ThingOwner<Thing>(this);
	}

	protected override void Tick()
	{
		base.Tick();
		if (this.IsHashIntervalTick(250))
		{
			bool flag = lastWorkedTick + 250 + 2 >= Find.TickManager.TicksGame;
			PowerTraderComp.PowerOutput = (flag ? (0f - base.PowerComp.Props.PowerConsumption) : (0f - base.PowerComp.Props.idlePowerDraw));
		}
		if (Working && this.IsHashIntervalTick(180))
		{
			CheckAllContainersValid();
		}
	}

	public void Start(List<Genepack> packs, int architesRequired, string xenotypeName, XenotypeIconDef iconDef)
	{
		Reset();
		genepacksToRecombine = packs;
		this.architesRequired = architesRequired;
		this.xenotypeName = xenotypeName;
		this.iconDef = iconDef;
		workingInt = true;
		totalWorkRequired = GeneTuning.ComplexityToCreationHoursCurve.Evaluate(TotalGCX) * 2500f;
	}

	public void DoWork(float workAmount)
	{
		workDone += workAmount;
		lastWorkAmount = workAmount;
		lastWorkedTick = Find.TickManager.TicksGame;
	}

	public void Finish()
	{
		if (!genepacksToRecombine.NullOrEmpty())
		{
			SoundDefOf.GeneAssembler_Complete.PlayOneShot(SoundInfo.InMap(this));
			Xenogerm xenogerm = (Xenogerm)ThingMaker.MakeThing(ThingDefOf.Xenogerm);
			xenogerm.Initialize(genepacksToRecombine, xenotypeName, iconDef);
			if (GenPlace.TryPlaceThing(xenogerm, InteractionCell, base.Map, ThingPlaceMode.Near))
			{
				Messages.Message("MessageXenogermCompleted".Translate(), xenogerm, MessageTypeDefOf.PositiveEvent);
			}
		}
		if (architesRequired > 0)
		{
			for (int num = innerContainer.Count - 1; num >= 0; num--)
			{
				if (innerContainer[num].def == ThingDefOf.ArchiteCapsule)
				{
					Thing thing = innerContainer[num].SplitOff(Mathf.Min(innerContainer[num].stackCount, architesRequired));
					architesRequired -= thing.stackCount;
					thing.Destroy();
					if (architesRequired <= 0)
					{
						break;
					}
				}
			}
		}
		Reset();
	}

	public List<Genepack> GetGenepacks(bool includePowered, bool includeUnpowered)
	{
		tmpGenepacks.Clear();
		List<Thing> connectedFacilities = ConnectedFacilities;
		if (connectedFacilities != null)
		{
			foreach (Thing item in connectedFacilities)
			{
				CompGenepackContainer compGenepackContainer = item.TryGetComp<CompGenepackContainer>();
				if (compGenepackContainer != null)
				{
					bool flag = item.TryGetComp<CompPowerTrader>()?.PowerOn ?? true;
					if ((includePowered && flag) || (includeUnpowered && !flag))
					{
						tmpGenepacks.AddRange(compGenepackContainer.ContainedGenepacks);
					}
				}
			}
		}
		return tmpGenepacks;
	}

	public CompGenepackContainer GetGeneBankHoldingPack(Genepack pack)
	{
		List<Thing> connectedFacilities = ConnectedFacilities;
		if (connectedFacilities != null)
		{
			foreach (Thing item in connectedFacilities)
			{
				CompGenepackContainer compGenepackContainer = item.TryGetComp<CompGenepackContainer>();
				if (compGenepackContainer == null)
				{
					continue;
				}
				foreach (Genepack containedGenepack in compGenepackContainer.ContainedGenepacks)
				{
					if (containedGenepack == pack)
					{
						return compGenepackContainer;
					}
				}
			}
		}
		return null;
	}

	public int MaxComplexity()
	{
		int num = 6;
		List<Thing> connectedFacilities = ConnectedFacilities;
		if (connectedFacilities != null)
		{
			foreach (Thing item in connectedFacilities)
			{
				CompPowerTrader compPowerTrader = item.TryGetComp<CompPowerTrader>();
				if (compPowerTrader == null || compPowerTrader.PowerOn)
				{
					num += (int)item.GetStatValue(StatDefOf.GeneticComplexityIncrease);
				}
			}
		}
		return num;
	}

	private void Reset()
	{
		workingInt = false;
		genepacksToRecombine = null;
		xenotypeName = null;
		cachedComplexity = null;
		iconDef = XenotypeIconDefOf.Basic;
		workDone = 0f;
		lastWorkedTick = -999;
		architesRequired = 0;
		innerContainer.TryDropAll(def.hasInteractionCell ? InteractionCell : base.Position, base.Map, ThingPlaceMode.Near);
	}

	private void CheckAllContainersValid()
	{
		if (genepacksToRecombine.NullOrEmpty())
		{
			return;
		}
		List<Thing> connectedFacilities = ConnectedFacilities;
		for (int i = 0; i < genepacksToRecombine.Count; i++)
		{
			bool flag = false;
			for (int j = 0; j < connectedFacilities.Count; j++)
			{
				CompGenepackContainer compGenepackContainer = connectedFacilities[j].TryGetComp<CompGenepackContainer>();
				if (compGenepackContainer != null && compGenepackContainer.ContainedGenepacks.Contains(genepacksToRecombine[i]))
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				Messages.Message("MessageXenogermCancelledMissingPack".Translate(this), this, MessageTypeDefOf.NegativeEvent);
				Reset();
				break;
			}
		}
	}

	public void GetChildHolders(List<IThingHolder> outChildren)
	{
		ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, GetDirectlyHeldThings());
	}

	public ThingOwner GetDirectlyHeldThings()
	{
		return innerContainer;
	}

	public override IEnumerable<Gizmo> GetGizmos()
	{
		foreach (Gizmo gizmo in base.GetGizmos())
		{
			yield return gizmo;
		}
		Command_Action command_Action = new Command_Action();
		command_Action.defaultLabel = "Recombine".Translate() + "...";
		command_Action.defaultDesc = "RecombineDesc".Translate();
		command_Action.icon = RecombineIcon.Texture;
		command_Action.action = delegate
		{
			Find.WindowStack.Add(new Dialog_CreateXenogerm(this));
		};
		if (!def.IsResearchFinished)
		{
			command_Action.Disable("MissingRequiredResearch".Translate() + ": " + (from x in def.researchPrerequisites
				where !x.IsFinished
				select x.label).ToCommaList(useAnd: true).CapitalizeFirst());
		}
		else if (!PowerOn)
		{
			command_Action.Disable("CannotUseNoPower".Translate());
		}
		else if (!GetGenepacks(includePowered: true, includeUnpowered: false).Any())
		{
			command_Action.Disable("CannotUseReason".Translate("NoGenepacksAvailable".Translate().CapitalizeFirst()));
		}
		yield return command_Action;
		if (Working)
		{
			Command_Action command_Action2 = new Command_Action();
			command_Action2.defaultLabel = "CancelXenogerm".Translate();
			command_Action2.defaultDesc = "CancelXenogermDesc".Translate();
			command_Action2.action = Reset;
			command_Action2.icon = CancelIcon;
			yield return command_Action2;
			if (DebugSettings.ShowDevGizmos)
			{
				Command_Action command_Action3 = new Command_Action();
				command_Action3.defaultLabel = "DEV: Finish xenogerm";
				command_Action3.action = Finish;
				yield return command_Action3;
			}
		}
	}

	public override string GetInspectString()
	{
		string text = base.GetInspectString();
		if (Working)
		{
			if (!text.NullOrEmpty())
			{
				text += "\n";
			}
			text = string.Concat(text, "CreatingXenogerm".Translate() + ": " + xenotypeName.CapitalizeFirst() + "\n" + "ComplexityTotal".Translate() + ": ", TotalGCX.ToString());
			text += "\n" + "Progress".Translate() + ": " + ProgressPercent.ToStringPercent();
			int numTicks = Mathf.RoundToInt((totalWorkRequired - workDone) / ((lastWorkAmount > 0f) ? lastWorkAmount : this.GetStatValue(StatDefOf.AssemblySpeedFactor)));
			text = text + " (" + "DurationLeft".Translate(numTicks.ToStringTicksToPeriod()).Resolve() + ")";
			AcceptanceReport canBeWorkedOnNow = CanBeWorkedOnNow;
			if (!canBeWorkedOnNow.Accepted && !canBeWorkedOnNow.Reason.NullOrEmpty())
			{
				text = text + "\n" + ("AssemblyPaused".Translate() + ": " + canBeWorkedOnNow.Reason).Colorize(ColorLibrary.RedReadable);
			}
			if (architesRequired > 0)
			{
				text = string.Concat(text, "\n" + "ArchitesRequired".Translate() + ": ", ArchitesCount.ToString(), " / ", architesRequired.ToString());
			}
		}
		return text;
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Deep.Look(ref innerContainer, "innerContainer", this);
		Scribe_Collections.Look(ref genepacksToRecombine, "genepacksToRecombine", LookMode.Reference);
		Scribe_Values.Look(ref workingInt, "workingInt", defaultValue: false);
		Scribe_Values.Look(ref workDone, "workDone", 0f);
		Scribe_Values.Look(ref totalWorkRequired, "totalWorkRequired", 0f);
		Scribe_Values.Look(ref lastWorkedTick, "lastWorkedTick", -999);
		Scribe_Values.Look(ref architesRequired, "architesRequired", 0);
		Scribe_Values.Look(ref xenotypeName, "xenotypeName");
		Scribe_Defs.Look(ref iconDef, "iconDef");
		if (Scribe.mode == LoadSaveMode.PostLoadInit && iconDef == null)
		{
			iconDef = XenotypeIconDefOf.Basic;
		}
	}
}
