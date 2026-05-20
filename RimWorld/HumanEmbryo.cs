using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld;

[StaticConstructorOnStartup]
public class HumanEmbryo : GeneSetHolderBase
{
	private const int MinImplantationAge = 16;

	public static readonly CachedTexture ImplantIcon = new CachedTexture("UI/Gizmos/ImplantEmbryo");

	private static readonly Texture2D CancelImplantIcon = ContentFinder<Texture2D>.Get("UI/Designators/Cancel");

	public Thing implantTarget;

	private List<Building_GrowthVat> tmpEligibleVats = new List<Building_GrowthVat>();

	public Pawn Mother => this.TryGetComp<CompHasPawnSources>().pawnSources?.FirstOrDefault((Pawn p) => p.gender == Gender.Female);

	public Pawn Father => this.TryGetComp<CompHasPawnSources>().pawnSources?.FirstOrDefault((Pawn p) => p.gender == Gender.Male);

	protected override string InspectGeneDescription => "InspectGenesEmbryoDesc".Translate();

	public override void PostMake()
	{
		if (!ModLister.CheckBiotech("human embryo"))
		{
			Destroy();
		}
		else
		{
			base.PostMake();
		}
	}

	public override IEnumerable<Gizmo> GetGizmos()
	{
		foreach (Gizmo gizmo in base.GetGizmos())
		{
			yield return gizmo;
		}
		if (!Find.Storyteller.difficulty.ChildrenAllowed || base.MapHeld == null)
		{
			yield break;
		}
		EnsureImplantTargetValid();
		if (implantTarget == null)
		{
			List<FloatMenuOption> surrogateOptions = new List<FloatMenuOption>();
			foreach (Pawn item in base.MapHeld.mapPawns.FreeColonistsAndPrisonersSpawned)
			{
				FloatMenuOption floatMenuOption = CanImplantFloatOption(item, pawnAsLabel: true);
				if (floatMenuOption != null)
				{
					surrogateOptions.Add(floatMenuOption);
				}
			}
			Command_Action command_Action = new Command_Action
			{
				defaultLabel = "ImplantLabel".Translate() + "...",
				defaultDesc = "ImplantDescription".Translate(),
				icon = ImplantIcon.Texture,
				action = delegate
				{
					Find.WindowStack.Add(new FloatMenu(surrogateOptions));
				}
			};
			if (surrogateOptions.Count == 0)
			{
				command_Action.Disable("ImplantDisabledNoWomen".Translate());
			}
			yield return command_Action;
			Building_GrowthVat bestVat = BestAvailableGrowthVat();
			Command_Action command_Action2 = new Command_Action
			{
				defaultLabel = "InsertGrowthVatLabel".Translate() + "...",
				defaultDesc = "InsertEmbryoGrowthVatDesc".Translate(540000.ToStringTicksToPeriod()).Resolve(),
				icon = Building_GrowthVat.InsertEmbryoIcon.Texture,
				activateSound = SoundDefOf.Tick_Tiny,
				action = delegate
				{
					bestVat.SelectEmbryo(this);
					implantTarget = bestVat;
				}
			};
			if (bestVat == null)
			{
				command_Action2.Disable("ImplantDisabledNoVats".Translate());
			}
			yield return command_Action2;
		}
		else if (base.Spawned)
		{
			Command_Action command_Action3 = new Command_Action();
			command_Action3.defaultLabel = "ImplantEmbryoCancel".Translate();
			command_Action3.defaultDesc = "ImplantEmbryoCancel".Translate();
			command_Action3.icon = CancelImplantIcon;
			command_Action3.action = delegate
			{
				ImplantPawnValid(cancel: true);
				ImplantVatValid(cancel: true);
				implantTarget = null;
			};
			command_Action3.activateSound = SoundDefOf.Designate_Cancel;
			yield return command_Action3;
		}
	}

	private void EnsureImplantTargetValid()
	{
		if (implantTarget == null)
		{
			return;
		}
		if (implantTarget is Pawn pawn)
		{
			if (pawn.Destroyed || !ImplantPawnValid(cancel: false))
			{
				implantTarget = null;
			}
		}
		else if (implantTarget is Building_GrowthVat building_GrowthVat && (building_GrowthVat.Destroyed || !ImplantVatValid(cancel: false)))
		{
			implantTarget = null;
		}
	}

	private bool ImplantPawnValid(bool cancel)
	{
		if (implantTarget is Pawn pawn)
		{
			foreach (Bill item in pawn.BillStack)
			{
				if (!(item is Bill_Medical bill_Medical) || bill_Medical.uniqueRequiredIngredients.NullOrEmpty())
				{
					continue;
				}
				foreach (Thing uniqueRequiredIngredient in bill_Medical.uniqueRequiredIngredients)
				{
					if (this == uniqueRequiredIngredient)
					{
						if (cancel)
						{
							bill_Medical.billStack.Delete(bill_Medical);
							implantTarget = null;
							return false;
						}
						return true;
					}
				}
			}
		}
		return false;
	}

	private bool ImplantVatValid(bool cancel)
	{
		if (implantTarget is Building_GrowthVat building_GrowthVat)
		{
			if (cancel)
			{
				building_GrowthVat.selectedEmbryo = null;
				implantTarget = null;
				return false;
			}
			return building_GrowthVat.selectedEmbryo == this;
		}
		return false;
	}

	private Building_GrowthVat BestAvailableGrowthVat()
	{
		if (base.MapHeld == null)
		{
			return null;
		}
		List<Building> list = base.MapHeld.listerBuildings.AllBuildingsColonistOfDef(ThingDefOf.GrowthVat);
		tmpEligibleVats.Clear();
		foreach (Building item in list)
		{
			if (item is Building_GrowthVat { Working: false, selectedEmbryo: null, SelectedPawn: null } building_GrowthVat)
			{
				tmpEligibleVats.Add(building_GrowthVat);
			}
		}
		if (tmpEligibleVats.NullOrEmpty())
		{
			return null;
		}
		tmpEligibleVats.SortBy((Building_GrowthVat v) => (!v.PowerOn) ? 1 : 0, (Building_GrowthVat v) => (base.PositionHeld - v.Position).LengthHorizontal);
		return tmpEligibleVats[0];
	}

	private FloatMenuOption CanImplantFloatOption(Pawn pawn, bool pawnAsLabel)
	{
		AcceptanceReport acceptanceReport = CanImplantReport(pawn);
		if (acceptanceReport.Accepted)
		{
			return new FloatMenuOption(pawnAsLabel ? pawn.LabelShort : "ImplantLabel".Translate().ToString(), delegate
			{
				float num = PregnancyUtility.PregnancyChanceImplantEmbryo(pawn);
				if (num < 1f)
				{
					Dialog_MessageBox window = Dialog_MessageBox.CreateConfirmation("ImplantSurgeryConfirm".Translate(Label, pawn.LabelShort, num.ToStringPercent()), delegate
					{
						HealthCardUtility.CreateSurgeryBill(pawn, RecipeDefOf.ImplantEmbryo, null, new List<Thing> { this });
						implantTarget = pawn;
					}, destructive: true);
					Find.WindowStack.Add(window);
				}
				else
				{
					HealthCardUtility.CreateSurgeryBill(pawn, RecipeDefOf.ImplantEmbryo, null, new List<Thing> { this });
					implantTarget = pawn;
				}
			}, pawn, Color.white);
		}
		if (!acceptanceReport.Reason.NullOrEmpty())
		{
			return new FloatMenuOption(pawnAsLabel ? "DisabledOption".Translate(pawn.LabelShort, acceptanceReport.Reason).ToString() : ("ImplantCannotFloatOption".Translate().ToString() + ": " + acceptanceReport.Reason), null, pawn, Color.white);
		}
		return null;
	}

	private AcceptanceReport CanImplantReport(Pawn pawn)
	{
		if (pawn.gender != Gender.Female)
		{
			return false;
		}
		if (pawn.IsQuestLodger())
		{
			return false;
		}
		HashSet<Pawn> hashSet = new HashSet<Pawn>();
		base.Map.reservationManager.ReserversOf(pawn, hashSet);
		if (hashSet.Any())
		{
			Pawn pawn2 = hashSet.First();
			return "ReservedBy".Translate(pawn2.LabelShort, pawn2);
		}
		if (pawn.BillStack.Bills.Any((Bill b) => b.recipe == RecipeDefOf.ImplantEmbryo))
		{
			return "CannotImplantingOtherEmbryo".Translate();
		}
		if (pawn.ageTracker.AgeBiologicalYears < 16)
		{
			return "CannotMustBeAge".Translate(16).CapitalizeFirst();
		}
		if (pawn.health.hediffSet.HasHediff(HediffDefOf.PregnantHuman))
		{
			return "CannotPregnant".Translate();
		}
		if (pawn.Sterile())
		{
			return "CannotSterile".Translate();
		}
		return true;
	}

	public bool TryPopulateGenes()
	{
		bool success = true;
		if (geneSet == null)
		{
			geneSet = PregnancyUtility.GetInheritedGeneSet(Father, Mother, out success);
		}
		return success;
	}

	public override void Notify_DebugSpawned()
	{
		CompHasPawnSources compHasPawnSources = this.TryGetComp<CompHasPawnSources>();
		if (base.Map.mapPawns.AllPawns.Where((Pawn x) => x.RaceProps.Humanlike && x.gender == Gender.Male).TryRandomElement(out var result))
		{
			compHasPawnSources.AddSource(result);
		}
		if (base.Map.mapPawns.AllPawns.Where((Pawn x) => x.RaceProps.Humanlike && x.gender == Gender.Female).TryRandomElement(out var result2))
		{
			compHasPawnSources.AddSource(result2);
		}
		geneSet = PregnancyUtility.GetInheritedGeneSet(Father, Mother);
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_References.Look(ref implantTarget, "implantTarget");
		if (Scribe.mode == LoadSaveMode.PostLoadInit && geneSet == null)
		{
			TryPopulateGenes();
		}
	}
}
