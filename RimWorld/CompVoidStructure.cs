using System.Collections.Generic;
using Verse;
using Verse.Sound;

namespace RimWorld;

public class CompVoidStructure : CompInteractable
{
	private const int ActiveGraphicIndex = 1;

	protected bool activated;

	private Sustainer ambienceSustainer;

	private Effecter ambienceEffecter;

	protected virtual bool Activatable => !activated;

	public override bool Active => activated;

	protected virtual SoundDef AmbientSound
	{
		get
		{
			if (!Active)
			{
				return SoundDefOf.VoidStructure_AmbientPreActivate;
			}
			return SoundDefOf.VoidStructure_AmbientPostActivate;
		}
	}

	public override void CompTick()
	{
		base.CompTick();
		if (Active && ambienceEffecter == null)
		{
			ambienceEffecter = EffecterDefOf.VoidStructureActivatedAmbience.Spawn(parent, parent.Map);
		}
		if (AmbientSound != null)
		{
			if (ambienceSustainer == null || ambienceSustainer.Ended)
			{
				ambienceSustainer = AmbientSound.TrySpawnSustainer(parent);
			}
			if (ambienceSustainer.def != AmbientSound)
			{
				ambienceSustainer.End();
			}
		}
		ambienceEffecter?.EffectTick(parent, parent);
	}

	protected override void OnInteracted(Pawn caster)
	{
		parent.overrideGraphicIndex = 1;
		parent.DirtyMapMesh(parent.Map);
		EffecterDefOf.VoidStructureActivated.Spawn(parent, parent.Map);
		CompGlower comp = parent.GetComp<CompGlower>();
		if (comp != null)
		{
			comp.GlowRadius = comp.Props.glowRadius + 1f;
			comp.GlowColor = new ColorInt(240, 160, 184);
		}
		activated = true;
	}

	public override AcceptanceReport CanInteract(Pawn activateBy = null, bool checkOptionalItems = true)
	{
		if (!Activatable)
		{
			return false;
		}
		return base.CanInteract(activateBy, checkOptionalItems);
	}

	public void Reset()
	{
		activated = false;
	}

	public override string CompInspectStringExtra()
	{
		if (Active)
		{
			return "Activated".Translate().CapitalizeFirst();
		}
		if (!Activatable)
		{
			return "";
		}
		return base.CompInspectStringExtra();
	}

	public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
	{
		if (!Activatable)
		{
			yield break;
		}
		foreach (FloatMenuOption item in base.CompFloatMenuOptions(selPawn))
		{
			yield return item;
		}
	}

	public override IEnumerable<Gizmo> CompGetGizmosExtra()
	{
		if (!Activatable)
		{
			yield break;
		}
		foreach (Gizmo item in base.CompGetGizmosExtra())
		{
			yield return item;
		}
		if (DebugSettings.ShowDevGizmos)
		{
			yield return new Command_Action
			{
				defaultLabel = "DEV: Activate",
				action = delegate
				{
					Interact(parent.Map.mapPawns.FreeColonistsSpawned.RandomElement(), force: true);
				}
			};
		}
	}

	public override void PostExposeData()
	{
		base.PostExposeData();
		Scribe_Values.Look(ref activated, "activated", defaultValue: false);
	}
}
