using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace RimWorld;

public class CompBladelinkWeapon : CompBiocodable
{
	private int lastKillTick = -1;

	private List<WeaponTraitDef> traits = new List<WeaponTraitDef>();

	private static readonly IntRange TraitsRange = new IntRange(1, 2);

	private bool oldBonded;

	private string oldBondedPawnLabel;

	private Pawn oldBondedPawn;

	public List<WeaponTraitDef> TraitsListForReading => traits;

	public int TicksSinceLastKill
	{
		get
		{
			if (lastKillTick < 0)
			{
				return 0;
			}
			return Find.TickManager.TicksAbs - lastKillTick;
		}
	}

	public override bool Biocodable
	{
		get
		{
			if (!traits.NullOrEmpty())
			{
				for (int i = 0; i < traits.Count; i++)
				{
					if (traits[i].neverBond)
					{
						return false;
					}
				}
			}
			return true;
		}
	}

	public override void PostPostMake()
	{
		InitializeTraits();
	}

	public override void PostDestroy(DestroyMode mode, Map previousMap)
	{
		UnCode();
	}

	public override void Notify_MapRemoved()
	{
		UnCode();
	}

	private void InitializeTraits()
	{
		IEnumerable<WeaponTraitDef> allDefs = DefDatabase<WeaponTraitDef>.AllDefs;
		if (traits == null)
		{
			traits = new List<WeaponTraitDef>();
		}
		using (new RandBlock(MapGenerator.mapBeingGenerated?.NextGenSeed ?? parent.HashOffset()))
		{
			int randomInRange = TraitsRange.RandomInRange;
			for (int i = 0; i < randomInRange; i++)
			{
				IEnumerable<WeaponTraitDef> source = allDefs.Where(CanAddTrait);
				if (source.Any())
				{
					traits.Add(source.RandomElementByWeight((WeaponTraitDef x) => x.commonality));
				}
			}
		}
	}

	private bool CanAddTrait(WeaponTraitDef trait)
	{
		if (trait.weaponCategory != WeaponCategoryDefOf.BladeLink)
		{
			return false;
		}
		if (!traits.NullOrEmpty())
		{
			for (int i = 0; i < traits.Count; i++)
			{
				if (trait.Overlaps(traits[i]))
				{
					return false;
				}
			}
		}
		return true;
	}

	public override void Notify_Equipped(Pawn pawn)
	{
		if (!ModLister.CheckRoyalty("Persona weapon"))
		{
			return;
		}
		base.Notify_Equipped(pawn);
		if (!traits.NullOrEmpty())
		{
			for (int i = 0; i < traits.Count; i++)
			{
				traits[i].Worker.Notify_Equipped(pawn);
			}
		}
	}

	public override void CodeFor(Pawn pawn)
	{
		if (base.Biocodable)
		{
			if (pawn.IsColonistPlayerControlled && base.CodedPawn == null)
			{
				Find.LetterStack.ReceiveLetter("LetterBladelinkWeaponBondedLabel".Translate(pawn.Named("PAWN"), parent.Named("WEAPON")), "LetterBladelinkWeaponBonded".Translate(pawn.Named("PAWN"), parent.Named("WEAPON")), LetterDefOf.PositiveEvent, new LookTargets(pawn));
			}
			base.CodeFor(pawn);
		}
	}

	protected override void OnCodedFor(Pawn pawn)
	{
		lastKillTick = GenTicks.TicksAbs;
		pawn.equipment.bondedWeapon = parent;
		if (!traits.NullOrEmpty())
		{
			for (int i = 0; i < traits.Count; i++)
			{
				traits[i].Worker.Notify_Bonded(pawn);
			}
		}
	}

	public override void Notify_KilledPawn(Pawn pawn)
	{
		lastKillTick = Find.TickManager.TicksAbs;
		if (!traits.NullOrEmpty())
		{
			for (int i = 0; i < traits.Count; i++)
			{
				traits[i].Worker.Notify_KilledPawn(pawn);
			}
		}
	}

	public void Notify_EquipmentLost(Pawn pawn)
	{
		if (!traits.NullOrEmpty())
		{
			for (int i = 0; i < traits.Count; i++)
			{
				traits[i].Worker.Notify_EquipmentLost(pawn);
			}
		}
	}

	public void Notify_WieldedOtherWeapon()
	{
		if (!traits.NullOrEmpty())
		{
			for (int i = 0; i < traits.Count; i++)
			{
				traits[i].Worker.Notify_OtherWeaponWielded(this);
			}
		}
	}

	public override void UnCode()
	{
		if (base.CodedPawn != null)
		{
			base.CodedPawn.equipment.bondedWeapon = null;
			if (!traits.NullOrEmpty())
			{
				for (int i = 0; i < traits.Count; i++)
				{
					traits[i].Worker.Notify_Unbonded(base.CodedPawn);
				}
			}
		}
		base.UnCode();
		lastKillTick = -1;
	}

	public override string CompInspectStringExtra()
	{
		string text = "";
		if (!traits.NullOrEmpty())
		{
			text += "Stat_Thing_PersonaWeaponTrait_Label".Translate() + ": " + traits.Select((WeaponTraitDef x) => x.label).ToCommaList().CapitalizeFirst();
		}
		if (Biocodable)
		{
			if (!text.NullOrEmpty())
			{
				text += "\n";
			}
			text = ((base.CodedPawn != null) ? (text + "BondedWith".Translate(base.CodedPawnLabel.ApplyTag(TagType.Name)).Resolve()) : ((string)(text + "NotBonded".Translate())));
		}
		return text;
	}

	public override void PostExposeData()
	{
		base.PostExposeData();
		Scribe_Values.Look(ref lastKillTick, "lastKillTick", -1);
		Scribe_Collections.Look(ref traits, "traits", LookMode.Def);
		if (Scribe.mode != LoadSaveMode.Saving)
		{
			Scribe_Values.Look(ref oldBonded, "bonded", defaultValue: false);
			Scribe_Values.Look(ref oldBondedPawnLabel, "bondedPawnLabel");
			Scribe_References.Look(ref oldBondedPawn, "bondedPawn", saveDestroyedThings: true);
		}
		if (Scribe.mode != LoadSaveMode.PostLoadInit)
		{
			return;
		}
		if (oldBonded)
		{
			CodeFor(oldBondedPawn);
		}
		if (traits == null)
		{
			traits = new List<WeaponTraitDef>();
		}
		if (oldBondedPawn != null)
		{
			if (string.IsNullOrEmpty(oldBondedPawnLabel) || !oldBonded)
			{
				codedPawnLabel = oldBondedPawn.Name.ToStringFull;
				biocoded = true;
			}
			if (oldBondedPawn.equipment.bondedWeapon == null)
			{
				oldBondedPawn.equipment.bondedWeapon = parent;
			}
			else if (oldBondedPawn.equipment.bondedWeapon != parent)
			{
				UnCode();
			}
		}
	}

	public override string TransformLabel(string label)
	{
		return label;
	}

	public override IEnumerable<StatDrawEntry> SpecialDisplayStats()
	{
		foreach (Faction allFaction in Find.FactionManager.AllFactions)
		{
			RoyalTitleDef minTitleToUse = ThingRequiringRoyalPermissionUtility.GetMinTitleToUse(parent.def, allFaction);
			if (minTitleToUse != null)
			{
				yield return new StatDrawEntry(StatCategoryDefOf.BasicsNonPawnImportant, "Stat_Thing_MinimumRoyalTitle_Name".Translate(allFaction.Named("FACTION")).Resolve(), minTitleToUse.GetLabelCapForBothGenders(), "Stat_Thing_Weapon_MinimumRoyalTitle_Desc".Translate(allFaction.Named("FACTION")).Resolve(), 2100);
			}
		}
		if (traits.NullOrEmpty())
		{
			yield break;
		}
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("Stat_Thing_PersonaWeaponTrait_Desc".Translate());
		stringBuilder.AppendLine();
		for (int i = 0; i < traits.Count; i++)
		{
			stringBuilder.AppendLine(traits[i].LabelCap + ": " + traits[i].description);
			if (i < traits.Count - 1)
			{
				stringBuilder.AppendLine();
			}
		}
		yield return new StatDrawEntry(parent.def.IsMeleeWeapon ? StatCategoryDefOf.Weapon_Melee : StatCategoryDefOf.Weapon_Ranged, "Stat_Thing_PersonaWeaponTrait_Label".Translate(), traits.Select((WeaponTraitDef x) => x.label).ToCommaList().CapitalizeFirst(), stringBuilder.ToString(), 1104);
	}
}
