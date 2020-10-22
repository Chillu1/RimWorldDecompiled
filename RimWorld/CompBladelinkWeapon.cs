using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace RimWorld
{
	public class CompBladelinkWeapon : ThingComp
	{
		private bool bonded;

		private string bondedPawnLabel;

		private int lastKillTick = -1;

		private List<WeaponTraitDef> traits = new List<WeaponTraitDef>();

		public Pawn bondedPawn;

		private static readonly IntRange TraitsRange = new IntRange(1, 2);

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

		public bool Bondable
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
			UnBond();
		}

		private void InitializeTraits()
		{
			IEnumerable<WeaponTraitDef> allDefs = DefDatabase<WeaponTraitDef>.AllDefs;
			if (traits == null)
			{
				traits = new List<WeaponTraitDef>();
			}
			Rand.PushState(parent.HashOffset());
			int randomInRange = TraitsRange.RandomInRange;
			for (int i = 0; i < randomInRange; i++)
			{
				IEnumerable<WeaponTraitDef> source = allDefs.Where((WeaponTraitDef x) => CanAddTrait(x));
				if (source.Any())
				{
					traits.Add(source.RandomElementByWeight((WeaponTraitDef x) => x.commonality));
				}
			}
			Rand.PopState();
		}

		private bool CanAddTrait(WeaponTraitDef trait)
		{
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
			if (!ModLister.RoyaltyInstalled)
			{
				Log.ErrorOnce("Persona weapons are a Royalty-specific game system. If you want to use this code please check ModLister.RoyaltyInstalled before calling it. See rules on the Ludeon forum for more info.", 988331);
				return;
			}
			if (Bondable)
			{
				BondToPawn(pawn);
			}
			if (!traits.NullOrEmpty())
			{
				for (int i = 0; i < traits.Count; i++)
				{
					traits[i].Worker.Notify_Equipped(pawn);
				}
			}
		}

		private void BondToPawn(Pawn pawn)
		{
			if (pawn.IsColonistPlayerControlled && bondedPawn == null)
			{
				Find.LetterStack.ReceiveLetter("LetterBladelinkWeaponBondedLabel".Translate(pawn.Named("PAWN"), parent.Named("WEAPON")), "LetterBladelinkWeaponBonded".Translate(pawn.Named("PAWN"), parent.Named("WEAPON")), LetterDefOf.PositiveEvent, new LookTargets(pawn));
			}
			bonded = true;
			bondedPawnLabel = pawn.Name.ToStringFull;
			bondedPawn = pawn;
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

		public void UnBond()
		{
			if (bondedPawn != null)
			{
				bondedPawn.equipment.bondedWeapon = null;
				if (!traits.NullOrEmpty())
				{
					for (int i = 0; i < traits.Count; i++)
					{
						traits[i].Worker.Notify_Unbonded(bondedPawn);
					}
				}
			}
			bonded = false;
			bondedPawn = null;
			bondedPawnLabel = null;
			lastKillTick = -1;
		}

		public override string CompInspectStringExtra()
		{
			string text = "";
			if (!traits.NullOrEmpty())
			{
				text += "Stat_Thing_PersonaWeaponTrait_Label".Translate() + ": " + traits.Select((WeaponTraitDef x) => x.label).ToCommaList().CapitalizeFirst();
			}
			if (Bondable)
			{
				if (!text.NullOrEmpty())
				{
					text += "\n";
				}
				text = ((bondedPawn != null) ? (text + "BondedWith".Translate(bondedPawnLabel.ApplyTag(TagType.Name)).Resolve()) : ((string)(text + "NotBonded".Translate())));
			}
			return text;
		}

		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Values.Look(ref bonded, "bonded", defaultValue: false);
			Scribe_Values.Look(ref bondedPawnLabel, "bondedPawnLabel");
			Scribe_Values.Look(ref lastKillTick, "lastKillTick", -1);
			Scribe_References.Look(ref bondedPawn, "bondedPawn", saveDestroyedThings: true);
			Scribe_Collections.Look(ref traits, "traits", LookMode.Def);
			if (Scribe.mode != LoadSaveMode.PostLoadInit)
			{
				return;
			}
			if (traits == null)
			{
				traits = new List<WeaponTraitDef>();
			}
			if (bondedPawn != null)
			{
				if (string.IsNullOrEmpty(bondedPawnLabel) || !bonded)
				{
					bondedPawnLabel = bondedPawn.Name.ToStringFull;
					bonded = true;
				}
				if (bondedPawn.equipment.bondedWeapon == null)
				{
					bondedPawn.equipment.bondedWeapon = parent;
				}
				else if (bondedPawn.equipment.bondedWeapon != parent)
				{
					UnBond();
				}
			}
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
			yield return new StatDrawEntry(StatCategoryDefOf.Weapon, "Stat_Thing_PersonaWeaponTrait_Label".Translate(), traits.Select((WeaponTraitDef x) => x.label).ToCommaList().CapitalizeFirst(), stringBuilder.ToString(), 5404);
		}

		[Obsolete("Will be removed in the future")]
		public override void Notify_UsedWeapon(Pawn pawn)
		{
		}
	}
}
