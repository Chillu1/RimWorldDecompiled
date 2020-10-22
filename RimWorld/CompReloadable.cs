using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	public class CompReloadable : ThingComp, IVerbOwner
	{
		private int remainingCharges;

		private VerbTracker verbTracker;

		public CompProperties_Reloadable Props => props as CompProperties_Reloadable;

		public int RemainingCharges => remainingCharges;

		public int MaxCharges => Props.maxCharges;

		public ThingDef AmmoDef => Props.ammoDef;

		public bool CanBeUsed => remainingCharges > 0;

		public Pawn Wearer => ReloadableUtility.WearerOf(this);

		public List<VerbProperties> VerbProperties => parent.def.Verbs;

		public List<Tool> Tools => parent.def.tools;

		public ImplementOwnerTypeDef ImplementOwnerTypeDef => ImplementOwnerTypeDefOf.NativeVerb;

		public Thing ConstantCaster => Wearer;

		public VerbTracker VerbTracker
		{
			get
			{
				if (verbTracker == null)
				{
					verbTracker = new VerbTracker(this);
				}
				return verbTracker;
			}
		}

		public string LabelRemaining => $"{RemainingCharges} / {MaxCharges}";

		public List<Verb> AllVerbs => VerbTracker.AllVerbs;

		public string UniqueVerbOwnerID()
		{
			return "Reloadable_" + parent.ThingID;
		}

		public bool VerbsStillUsableBy(Pawn p)
		{
			return Wearer == p;
		}

		public override void PostPostMake()
		{
			base.PostPostMake();
			remainingCharges = MaxCharges;
		}

		public override string CompInspectStringExtra()
		{
			return "ChargesRemaining".Translate(Props.ChargeNounArgument) + ": " + LabelRemaining;
		}

		public override IEnumerable<StatDrawEntry> SpecialDisplayStats()
		{
			IEnumerable<StatDrawEntry> enumerable = base.SpecialDisplayStats();
			if (enumerable != null)
			{
				foreach (StatDrawEntry item in enumerable)
				{
					yield return item;
				}
			}
			yield return new StatDrawEntry(StatCategoryDefOf.Apparel, "Stat_Thing_ReloadChargesRemaining_Name".Translate(Props.ChargeNounArgument), LabelRemaining, "Stat_Thing_ReloadChargesRemaining_Desc".Translate(Props.ChargeNounArgument), 2749);
		}

		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Values.Look(ref remainingCharges, "remainingCharges", -999);
			Scribe_Deep.Look(ref verbTracker, "verbTracker", this);
			if (Scribe.mode == LoadSaveMode.PostLoadInit && remainingCharges == -999)
			{
				remainingCharges = MaxCharges;
			}
		}

		public override IEnumerable<Gizmo> CompGetWornGizmosExtra()
		{
			foreach (Gizmo item in base.CompGetWornGizmosExtra())
			{
				yield return item;
			}
			bool drafted = Wearer.Drafted;
			if ((drafted && !Props.displayGizmoWhileDrafted) || (!drafted && !Props.displayGizmoWhileUndrafted))
			{
				yield break;
			}
			ThingWithComps gear = parent;
			foreach (Verb allVerb in VerbTracker.AllVerbs)
			{
				if (allVerb.verbProps.hasStandardCommand)
				{
					yield return CreateVerbTargetCommand(gear, allVerb);
				}
			}
			if (Prefs.DevMode)
			{
				Command_Action command_Action = new Command_Action();
				command_Action.defaultLabel = "Debug: Reload to full";
				command_Action.action = delegate
				{
					remainingCharges = MaxCharges;
				};
				yield return command_Action;
			}
		}

		private Command_Reloadable CreateVerbTargetCommand(Thing gear, Verb verb)
		{
			Command_Reloadable command_Reloadable = new Command_Reloadable(this);
			command_Reloadable.defaultDesc = gear.def.description;
			command_Reloadable.hotKey = Props.hotKey;
			command_Reloadable.defaultLabel = verb.verbProps.label;
			command_Reloadable.verb = verb;
			if (verb.verbProps.defaultProjectile != null && verb.verbProps.commandIcon == null)
			{
				command_Reloadable.icon = verb.verbProps.defaultProjectile.uiIcon;
				command_Reloadable.iconAngle = verb.verbProps.defaultProjectile.uiIconAngle;
				command_Reloadable.iconOffset = verb.verbProps.defaultProjectile.uiIconOffset;
				command_Reloadable.overrideColor = verb.verbProps.defaultProjectile.graphicData.color;
			}
			else
			{
				command_Reloadable.icon = ((verb.UIIcon != BaseContent.BadTex) ? verb.UIIcon : gear.def.uiIcon);
				command_Reloadable.iconAngle = gear.def.uiIconAngle;
				command_Reloadable.iconOffset = gear.def.uiIconOffset;
				command_Reloadable.defaultIconColor = gear.DrawColor;
			}
			if (!Wearer.IsColonistPlayerControlled)
			{
				command_Reloadable.Disable();
			}
			else if (verb.verbProps.violent && Wearer.WorkTagIsDisabled(WorkTags.Violent))
			{
				command_Reloadable.Disable("IsIncapableOfViolenceLower".Translate(Wearer.LabelShort, Wearer).CapitalizeFirst() + ".");
			}
			else if (!CanBeUsed)
			{
				command_Reloadable.Disable(DisabledReason(MinAmmoNeeded(allowForcedReload: false), MaxAmmoNeeded(allowForcedReload: false)));
			}
			return command_Reloadable;
		}

		public string DisabledReason(int minNeeded, int maxNeeded)
		{
			if (AmmoDef == null)
			{
				return "CommandReload_NoCharges".Translate(Props.ChargeNounArgument);
			}
			return TranslatorFormattedStringExtensions.Translate(arg3: ((Props.ammoCountToRefill == 0) ? ((minNeeded == maxNeeded) ? minNeeded.ToString() : $"{minNeeded}-{maxNeeded}") : Props.ammoCountToRefill.ToString()).Named("COUNT"), key: "CommandReload_NoAmmo", arg1: Props.ChargeNounArgument, arg2: NamedArgumentUtility.Named(AmmoDef, "AMMO"));
		}

		public bool NeedsReload(bool allowForcedReload)
		{
			if (AmmoDef == null)
			{
				return false;
			}
			if (Props.ammoCountToRefill != 0)
			{
				if (!allowForcedReload)
				{
					return remainingCharges == 0;
				}
				return RemainingCharges != MaxCharges;
			}
			return RemainingCharges != MaxCharges;
		}

		public void ReloadFrom(Thing ammo)
		{
			if (!NeedsReload(allowForcedReload: true))
			{
				return;
			}
			if (Props.ammoCountToRefill != 0)
			{
				if (ammo.stackCount < Props.ammoCountToRefill)
				{
					return;
				}
				ammo.SplitOff(Props.ammoCountToRefill).Destroy();
				remainingCharges = MaxCharges;
			}
			else
			{
				if (ammo.stackCount < Props.ammoCountPerCharge)
				{
					return;
				}
				int num = Mathf.Clamp(ammo.stackCount / Props.ammoCountPerCharge, 0, MaxCharges - RemainingCharges);
				ammo.SplitOff(num * Props.ammoCountPerCharge).Destroy();
				remainingCharges += num;
			}
			if (Props.soundReload != null)
			{
				Props.soundReload.PlayOneShot(new TargetInfo(Wearer.Position, Wearer.Map));
			}
		}

		public int MinAmmoNeeded(bool allowForcedReload)
		{
			if (!NeedsReload(allowForcedReload))
			{
				return 0;
			}
			if (Props.ammoCountToRefill != 0)
			{
				return Props.ammoCountToRefill;
			}
			return Props.ammoCountPerCharge;
		}

		public int MaxAmmoNeeded(bool allowForcedReload)
		{
			if (!NeedsReload(allowForcedReload))
			{
				return 0;
			}
			if (Props.ammoCountToRefill != 0)
			{
				return Props.ammoCountToRefill;
			}
			return Props.ammoCountPerCharge * (MaxCharges - RemainingCharges);
		}

		public int MaxAmmoAmount()
		{
			if (AmmoDef == null)
			{
				return 0;
			}
			if (Props.ammoCountToRefill == 0)
			{
				return Props.ammoCountPerCharge * MaxCharges;
			}
			return Props.ammoCountToRefill;
		}

		public void UsedOnce()
		{
			if (remainingCharges > 0)
			{
				remainingCharges--;
			}
			if (Props.destroyOnEmpty && remainingCharges == 0 && !parent.Destroyed)
			{
				parent.Destroy();
			}
		}
	}
}
