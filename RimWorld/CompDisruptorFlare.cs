using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld;

public class CompDisruptorFlare : ThingComp
{
	private const int BaseStunDuration = 120;

	private const int DestroyWarningTicks = 900;

	private Effecter destroyWarningEffecter;

	private CompDestroyAfterDelay destroyDelayedComp;

	private Effecter attachedEffecter;

	private CompGlower glowComp;

	public CompProperties_DisruptorFlare Props => (CompProperties_DisruptorFlare)props;

	private int WarnTick => destroyDelayedComp.DestructionTick - 900;

	private float WarnAlpha => Mathf.InverseLerp(WarnTick, destroyDelayedComp.DestructionTick, Find.TickManager.TicksGame);

	private float GlowRadius => Props.radius + 2f;

	public override void PostSpawnSetup(bool respawningAfterLoad)
	{
		base.PostSpawnSetup(respawningAfterLoad);
		glowComp = parent.GetComp<CompGlower>();
		if (glowComp != null)
		{
			glowComp.GlowRadius = GlowRadius;
		}
		destroyDelayedComp = parent.GetComp<CompDestroyAfterDelay>();
		if (respawningAfterLoad)
		{
			return;
		}
		foreach (Pawn item in parent.Map.mapPawns.AllPawnsSpawned.ToList())
		{
			if (item.Spawned && item.Position.InHorDistOf(parent.Position, Props.radius) && GenSight.LineOfSightToThing(parent.Position, item, parent.Map))
			{
				if (!item.health.hediffSet.HasHediff(HediffDefOf.DisruptorFlash))
				{
					PsychicStun(item);
				}
				item.health.AddHediff(HediffDefOf.DisruptorFlash);
				CompRevenant compRevenant = item.TryGetComp<CompRevenant>();
				if (compRevenant != null && item.IsPsychologicallyInvisible())
				{
					compRevenant.SetState(RevenantState.Escape);
				}
			}
		}
	}

	private void PsychicStun(Pawn pawn)
	{
		float statValue = pawn.GetStatValue(StatDefOf.PsychicSensitivity);
		if (statValue > 0f)
		{
			pawn.stances?.stunner?.StunFor(Mathf.RoundToInt(120f * statValue), null);
		}
	}

	public override void CompTick()
	{
		if (parent.Spawned)
		{
			if (attachedEffecter == null)
			{
				attachedEffecter = Props.effecterDef.SpawnAttached(parent, parent.MapHeld);
			}
			if (WarnTick - Find.TickManager.TicksGame > 80)
			{
				attachedEffecter?.EffectTick(parent, parent);
			}
		}
		else
		{
			attachedEffecter?.Cleanup();
			attachedEffecter = null;
		}
		if (Find.TickManager.TicksGame >= WarnTick)
		{
			if (destroyWarningEffecter == null)
			{
				destroyWarningEffecter = Props.destroyWarningEffecterDef.SpawnAttached(parent, parent.MapHeld);
			}
			destroyWarningEffecter.EffectTick(parent, parent);
			float warnAlpha = WarnAlpha;
			ColorInt glowColor = glowComp.GlowColor;
			glowColor.a = (byte)Mathf.Lerp(glowColor.a, 0f, warnAlpha);
			glowComp.GlowRadius = Mathf.Lerp(GlowRadius, 0.1f, warnAlpha);
			glowComp.GlowColor = glowColor;
			if (attachedEffecter != null)
			{
				attachedEffecter.children[0].colorOverride = new Color(1f, 1f, 1f, warnAlpha);
			}
		}
	}
}
