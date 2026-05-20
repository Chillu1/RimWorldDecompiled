using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld;

[StaticConstructorOnStartup]
public class Xenogerm : GeneSetHolderBase
{
	private Pawn targetPawn;

	public string xenotypeName;

	public XenotypeIconDef iconDef;

	private static readonly CachedTexture ImplantTex = new CachedTexture("UI/Gizmos/ImplantGenes");

	private static readonly Texture2D CancelIcon = ContentFinder<Texture2D>.Get("UI/Designators/Cancel");

	public override string LabelNoCount
	{
		get
		{
			if (xenotypeName.NullOrEmpty())
			{
				return base.LabelNoCount;
			}
			return "NamedXenogerm".Translate(xenotypeName.Named("NAME"));
		}
	}

	private int RequiredMedicineForImplanting
	{
		get
		{
			int num = 0;
			for (int i = 0; i < RecipeDefOf.ImplantXenogerm.ingredients.Count; i++)
			{
				IngredientCount ingredientCount = RecipeDefOf.ImplantXenogerm.ingredients[i];
				if (ingredientCount.filter.Allows(ThingDefOf.MedicineIndustrial))
				{
					num += (int)ingredientCount.GetBaseCount();
				}
			}
			return num;
		}
	}

	public override void Notify_DebugSpawned()
	{
		geneSet = GeneUtility.GenerateGeneSet();
		xenotypeName = "Unique".Translate();
		iconDef = XenotypeIconDefOf.Basic;
	}

	public override void PostMake()
	{
		if (!ModLister.CheckBiotech("xenogerm"))
		{
			Destroy();
		}
		else
		{
			base.PostMake();
		}
	}

	public void SetTargetPawn(Pawn newTarget)
	{
		int trueMax = HediffDefOf.XenogerminationComa.CompProps<HediffCompProperties_Disappears>().disappearsAfterTicks.TrueMax;
		TaggedString text = "ImplantXenogermWarningDesc".Translate(newTarget.Named("PAWN"), trueMax.ToStringTicksToPeriod().Named("COMADURATION"));
		if (newTarget.genes.Xenogenes.Any())
		{
			text += "\n\n" + "ImplantXenogermWarningOverwriteXenogenes".Translate(newTarget.Named("PAWN"), newTarget.genes.XenotypeLabelCap.Named("XENOTYPE"), newTarget.genes.Xenogenes.Select((Gene x) => x.LabelCap).ToLineList("  - ").Named("XENOGENES"));
		}
		int num = GeneUtility.MetabolismAfterImplanting(newTarget, geneSet);
		text += "\n\n" + "ImplantXenogermWarningNewMetabolism".Translate(newTarget.Named("PAWN"), num.Named("MET"), GeneTuning.MetabolismToFoodConsumptionFactorCurve.Evaluate(num).ToStringPercent().Named("CONSUMPTION"));
		text += "\n\n" + "WouldYouLikeToContinue".Translate();
		Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation(text, delegate
		{
			Bill bill = targetPawn?.BillStack?.Bills?.FirstOrDefault((Bill x) => x.xenogerm == this);
			if (bill != null)
			{
				targetPawn.BillStack.Delete(bill);
			}
			HealthCardUtility.CreateSurgeryBill(newTarget, RecipeDefOf.ImplantXenogerm, null).xenogerm = this;
			targetPawn = newTarget;
			SendImplantationLetter(newTarget);
		}, destructive: true));
	}

	private void SendImplantationLetter(Pawn targetPawn)
	{
		string arg = string.Empty;
		if (!targetPawn.InBed() && !targetPawn.Map.listerBuildings.allBuildingsColonist.Any((Building x) => x is Building_Bed && RestUtility.CanUseBedEver(targetPawn, x.def) && ((Building_Bed)x).Medical))
		{
			arg = "XenogermOrderedImplantedBedNeeded".Translate(targetPawn.Named("PAWN"));
		}
		int requiredMedicineForImplanting = RequiredMedicineForImplanting;
		string arg2 = string.Empty;
		if (targetPawn.Map.listerThings.ThingsInGroup(ThingRequestGroup.Medicine).Sum((Thing x) => x.stackCount) < requiredMedicineForImplanting)
		{
			arg2 = "XenogermOrderedImplantedMedicineNeeded".Translate(requiredMedicineForImplanting.Named("MEDICINENEEDED"));
		}
		Find.LetterStack.ReceiveLetter("LetterLabelXenogermOrderedImplanted".Translate(), "LetterXenogermOrderedImplanted".Translate(targetPawn.Named("PAWN"), requiredMedicineForImplanting.Named("MEDICINENEEDED"), arg.Named("BEDINFO"), arg2.Named("MEDICINEINFO")), LetterDefOf.NeutralEvent);
	}

	public void Initialize(List<Genepack> genepacks, string xenotypeName, XenotypeIconDef iconDef)
	{
		this.xenotypeName = xenotypeName;
		this.iconDef = iconDef;
		geneSet = new GeneSet();
		for (int i = 0; i < genepacks.Count; i++)
		{
			if (genepacks[i].GeneSet != null)
			{
				List<GeneDef> genesListForReading = genepacks[i].GeneSet.GenesListForReading;
				for (int j = 0; j < genesListForReading.Count; j++)
				{
					geneSet.AddGene(genesListForReading[j]);
				}
			}
		}
	}

	public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn selPawn)
	{
		foreach (FloatMenuOption floatMenuOption in base.GetFloatMenuOptions(selPawn))
		{
			yield return floatMenuOption;
		}
		if (!ModsConfig.BiotechActive || selPawn.genes == null)
		{
			yield break;
		}
		int num = GeneUtility.MetabolismAfterImplanting(selPawn, geneSet);
		if (num < GeneTuning.BiostatRange.TrueMin)
		{
			yield return new FloatMenuOption("CannotGenericWorkCustom".Translate(string.Concat("OrderImplantationIntoPawn".Translate(selPawn.Named("PAWN")).Resolve().UncapitalizeFirst() + ": " + "ResultingMetTooLow".Translate() + " (", num.ToString(), ")")), null);
			yield break;
		}
		if (PawnIdeoDisallowsImplanting(selPawn))
		{
			yield return new FloatMenuOption("CannotGenericWorkCustom".Translate("OrderImplantationIntoPawn".Translate(selPawn.Named("PAWN")).Resolve().UncapitalizeFirst() + ": " + "IdeoligionForbids".Translate()), null);
			yield break;
		}
		yield return new FloatMenuOption("OrderImplantationIntoPawn".Translate(selPawn.Named("PAWN")) + " (" + xenotypeName + ")", delegate
		{
			SetTargetPawn(selPawn);
		});
	}

	public bool PawnIdeoDisallowsImplanting(Pawn selPawn)
	{
		if (!ModsConfig.BiotechActive || !ModsConfig.IdeologyActive)
		{
			return false;
		}
		if (!IdeoUtility.DoerWillingToDo(HistoryEventDefOf.PropagateBloodfeederGene, selPawn) && base.GeneSet.GenesListForReading.Any((GeneDef x) => x == GeneDefOf.Bloodfeeder))
		{
			return true;
		}
		if (!IdeoUtility.DoerWillingToDo(HistoryEventDefOf.BecomeNonPreferredXenotype, selPawn))
		{
			return true;
		}
		return false;
	}

	public override IEnumerable<Gizmo> GetGizmos()
	{
		foreach (Gizmo gizmo in base.GetGizmos())
		{
			yield return gizmo;
		}
		if (geneSet == null)
		{
			yield break;
		}
		if (targetPawn == null)
		{
			yield return new Command_Action
			{
				defaultLabel = "ImplantXenogerm".Translate() + "...",
				defaultDesc = "ImplantXenogermDesc".Translate(RequiredMedicineForImplanting.Named("MEDICINEAMOUNT")),
				icon = ImplantTex.Texture,
				action = delegate
				{
					List<FloatMenuOption> list = new List<FloatMenuOption>();
					foreach (Pawn item in base.Map.mapPawns.AllPawnsSpawned)
					{
						Pawn pawn = item;
						if (!pawn.IsQuestLodger() && pawn.genes != null && (pawn.IsColonistPlayerControlled || pawn.IsPrisonerOfColony || pawn.IsSlaveOfColony || (pawn.IsColonySubhuman && pawn.IsGhoul)))
						{
							int num = GeneUtility.MetabolismAfterImplanting(pawn, geneSet);
							if (num < GeneTuning.BiostatRange.TrueMin)
							{
								list.Add(new FloatMenuOption(string.Concat(pawn.LabelShortCap + ": " + "ResultingMetTooLow".Translate() + " (", num.ToString(), ")"), null, pawn, Color.white));
							}
							else if (PawnIdeoDisallowsImplanting(pawn))
							{
								list.Add(new FloatMenuOption(pawn.LabelShortCap + ": " + "IdeoligionForbids".Translate(), null, pawn, Color.white));
							}
							else
							{
								list.Add(new FloatMenuOption(pawn.LabelShortCap + ", " + pawn.genes.XenotypeLabelCap, delegate
								{
									SetTargetPawn(pawn);
								}, pawn, Color.white));
							}
						}
					}
					if (!list.Any())
					{
						list.Add(new FloatMenuOption("NoImplantablePawns".Translate(), null));
					}
					Find.WindowStack.Add(new FloatMenu(list));
				}
			};
			yield break;
		}
		yield return new Command_Action
		{
			defaultLabel = "CancelImplanting".Translate(),
			defaultDesc = "CancelImplantingDesc".Translate(targetPawn.Named("PAWN")),
			icon = CancelIcon,
			action = delegate
			{
				Bill bill = targetPawn.BillStack?.Bills.FirstOrDefault((Bill x) => x.xenogerm == this);
				if (bill != null)
				{
					targetPawn.BillStack.Delete(bill);
				}
			}
		};
	}

	public void Notify_BillRemoved()
	{
		targetPawn = null;
	}

	public bool ValidateTarget(LocalTargetInfo target, bool showMessages = true)
	{
		if (!target.IsValid || target.Pawn == null)
		{
			return false;
		}
		if (target.Pawn.IsQuestLodger())
		{
			if (showMessages)
			{
				Messages.Message("MessageCannotImplantInTempFactionMembers".Translate(), target.Pawn, MessageTypeDefOf.RejectInput, historical: false);
			}
			return false;
		}
		if (!target.Pawn.IsColonist && !target.Pawn.IsPrisonerOfColony && !target.Pawn.IsSlaveOfColony)
		{
			if (showMessages)
			{
				Messages.Message("MessageCanOnlyTargetColonistsPrisonersAndSlaves".Translate(), target.Pawn, MessageTypeDefOf.RejectInput, historical: false);
			}
			return false;
		}
		return true;
	}

	public override void DrawExtraSelectionOverlays()
	{
		base.DrawExtraSelectionOverlays();
		if (targetPawn != null && targetPawn.Map == base.Map)
		{
			GenDraw.DrawLineBetween(this.TrueCenter(), targetPawn.TrueCenter());
		}
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_References.Look(ref targetPawn, "targetPawn");
		Scribe_Values.Look(ref xenotypeName, "xenotypeName");
		Scribe_Defs.Look(ref iconDef, "iconDef");
		if (Scribe.mode == LoadSaveMode.PostLoadInit && iconDef == null)
		{
			iconDef = XenotypeIconDefOf.Basic;
		}
	}
}
