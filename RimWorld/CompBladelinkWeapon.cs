using System;
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
			if (!ModLister.RoyaltyInstalled)
			{
				Log.ErrorOnce("Persona weapons are a Royalty-specific game system. If you want to use this code please check ModLister.RoyaltyInstalled before calling it. See rules on the Ludeon forum for more info.", 988331);
				return;
			}
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

		[Obsolete("Will be removed in the future")]
		public override void Notify_UsedWeapon(Pawn pawn)
		{
		}
	}
}
