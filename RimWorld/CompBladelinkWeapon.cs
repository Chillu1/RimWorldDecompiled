using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class CompBladelinkWeapon : ThingComp
	{
		private bool bonded;

		private string bondedPawnLabel;

		public Pawn bondedPawn;

		public override void Notify_Equipped(Pawn pawn)
		{
			if (pawn.IsColonistPlayerControlled && bondedPawn == null)
			{
				Find.LetterStack.ReceiveLetter("LetterBladelinkWeaponBondedLabel".Translate(pawn.Named("PAWN"), parent.Named("WEAPON")), "LetterBladelinkWeaponBonded".Translate(pawn.Named("PAWN"), parent.Named("WEAPON")).Resolve(), LetterDefOf.PositiveEvent);
			}
			bonded = true;
			bondedPawnLabel = pawn.Name.ToStringFull;
			bondedPawn = pawn;
		}

		public override string CompInspectStringExtra()
		{
			if (bondedPawn == null)
			{
				return "NotBonded".Translate();
			}
			return "BondedWith".Translate(bondedPawnLabel.ApplyTag(TagType.Name)).Resolve();
		}

		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Values.Look(ref bonded, "bonded", defaultValue: false);
			Scribe_Values.Look(ref bondedPawnLabel, "bondedPawnLabel");
			Scribe_References.Look(ref bondedPawn, "bondedPawn", saveDestroyedThings: true);
			if (Scribe.mode == LoadSaveMode.PostLoadInit && (string.IsNullOrEmpty(bondedPawnLabel) || !bonded) && bondedPawn != null)
			{
				bondedPawnLabel = bondedPawn.Name.ToStringFull;
				bonded = true;
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
		}

		public override void Notify_UsedWeapon(Pawn pawn)
		{
			if (pawn.Faction == Faction.OfPlayer && !pawn.health.hediffSet.HasHediff(HediffDefOf.PsychicSilencer))
			{
				float statValue = parent.GetStatValue(StatDefOf.Bladelink_DetectionChance);
				if (Rand.Chance(statValue))
				{
					foreach (Faction allFaction in Find.FactionManager.AllFactions)
					{
						if (ThingRequiringRoyalPermissionUtility.IsViolatingRulesOf(parent.def, pawn, allFaction))
						{
							allFaction.Notify_RoyalThingUseViolation(parent.def, pawn, parent.def.label, statValue);
						}
					}
				}
			}
		}
	}
}
