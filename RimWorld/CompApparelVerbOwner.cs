using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class CompApparelVerbOwner : ThingComp, IVerbOwner
{
	private VerbTracker verbTracker;

	public CompProperties_ApparelVerbOwner Props => props as CompProperties_ApparelVerbOwner;

	public Pawn Wearer => (base.ParentHolder as Pawn_ApparelTracker)?.pawn;

	public List<VerbProperties> VerbProperties => parent.def.Verbs;

	public List<Tool> Tools => parent.def.tools;

	public ImplementOwnerTypeDef ImplementOwnerTypeDef => ImplementOwnerTypeDefOf.NativeVerb;

	public Thing ConstantCaster => Wearer;

	public virtual string GizmoExtraLabel => null;

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

	public List<Verb> AllVerbs => VerbTracker.AllVerbs;

	public string UniqueVerbOwnerID()
	{
		return "CompVerbOwner_" + parent.ThingID;
	}

	public bool VerbsStillUsableBy(Pawn p)
	{
		return Wearer == p;
	}

	public override void PostExposeData()
	{
		Scribe_Deep.Look(ref verbTracker, "verbTracker", this);
		if (Scribe.mode == LoadSaveMode.PostLoadInit)
		{
			SetupVerbs();
		}
	}

	public override void Initialize(CompProperties props)
	{
		base.Initialize(props);
		SetupVerbs();
	}

	private void SetupVerbs()
	{
		for (int i = 0; i < AllVerbs.Count; i++)
		{
			AllVerbs[i].caster = Wearer;
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
	}

	private Command_VerbTarget CreateVerbTargetCommand(Thing gear, Verb verb)
	{
		Command_VerbOwner command_VerbOwner = new Command_VerbOwner(this);
		command_VerbOwner.defaultDesc = gear.def.description;
		command_VerbOwner.hotKey = Props.hotKey;
		command_VerbOwner.defaultLabel = verb.verbProps.label;
		command_VerbOwner.verb = verb;
		if (verb.verbProps.defaultProjectile != null && verb.verbProps.commandIcon == null)
		{
			command_VerbOwner.icon = verb.verbProps.defaultProjectile.uiIcon;
			command_VerbOwner.iconAngle = verb.verbProps.defaultProjectile.uiIconAngle;
			command_VerbOwner.iconOffset = verb.verbProps.defaultProjectile.uiIconOffset;
			command_VerbOwner.overrideColor = verb.verbProps.defaultProjectile.graphicData.color;
		}
		else
		{
			command_VerbOwner.icon = ((verb.UIIcon != BaseContent.BadTex) ? verb.UIIcon : gear.def.uiIcon);
			command_VerbOwner.iconAngle = gear.def.uiIconAngle;
			command_VerbOwner.iconOffset = gear.def.uiIconOffset;
			command_VerbOwner.defaultIconColor = gear.DrawColor;
		}
		string reason;
		if (!Wearer.IsColonistPlayerControlled)
		{
			command_VerbOwner.Disable("CannotOrderNonControlled".Translate());
		}
		else if (verb.verbProps.violent && Wearer.WorkTagIsDisabled(WorkTags.Violent))
		{
			command_VerbOwner.Disable("IsIncapableOfViolenceLower".Translate(Wearer.LabelShort, Wearer).CapitalizeFirst() + ".");
		}
		else if (!CanBeUsed(out reason))
		{
			command_VerbOwner.Disable(reason);
		}
		return command_VerbOwner;
	}

	public virtual void UsedOnce()
	{
	}

	public virtual bool CanBeUsed(out string reason)
	{
		reason = "";
		if (parent.MapHeld == null)
		{
			return false;
		}
		if (parent.MapHeld.IsPocketMap && VerbProperties.Any((VerbProperties vp) => !vp.useableInPocketMaps))
		{
			reason = "CannotUseReason_PocketMap".Translate(parent.MapHeld.generatorDef.label);
			return false;
		}
		if (parent.MapHeld.Biome.inVacuum && VerbProperties.Any((VerbProperties vp) => !vp.useableInVacuum))
		{
			reason = "CannotFunctionInVacuum".Translate();
			return false;
		}
		return true;
	}
}
