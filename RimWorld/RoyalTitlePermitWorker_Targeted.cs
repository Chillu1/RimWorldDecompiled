using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

public abstract class RoyalTitlePermitWorker_Targeted : RoyalTitlePermitWorker, ITargetingSource
{
	protected bool free;

	protected Pawn caller;

	protected Map map;

	protected TargetingParameters targetingParameters;

	private static List<IntVec3> tempSourceList = new List<IntVec3>();

	public bool CasterIsPawn => true;

	public bool IsMeleeAttack => false;

	public bool Targetable => true;

	public bool MultiSelect => false;

	public bool HidePawnTooltips => false;

	public Thing Caster => caller;

	public Pawn CasterPawn => caller;

	public Verb GetVerb => null;

	public Texture2D UIIcon => null;

	public TargetingParameters targetParams => targetingParameters;

	public ITargetingSource DestinationSelector => null;

	protected float RangeClamped => Mathf.Min(def.royalAid.targetingRange, map.weatherManager.CurWeatherMaxRangeCap);

	public bool CanHitTarget(LocalTargetInfo target)
	{
		if (def.royalAid.targetingRequireLOS && !GenSight.LineOfSight(caller.Position, target.Cell, map, skipFirstCell: true))
		{
			bool flag = false;
			ShootLeanUtility.LeanShootingSourcesFromTo(caller.Position, target.Cell, map, tempSourceList);
			for (int i = 0; i < tempSourceList.Count; i++)
			{
				if (GenSight.LineOfSight(tempSourceList[i], target.Cell, map, skipFirstCell: true))
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				return false;
			}
		}
		return true;
	}

	public virtual bool ValidateTarget(LocalTargetInfo target, bool showMessages = true)
	{
		if (!CanHitTarget(target))
		{
			if (target.IsValid)
			{
				Messages.Message(def.LabelCap + ": " + "AbilityCannotHitTarget".Translate(), MessageTypeDefOf.RejectInput);
			}
			return false;
		}
		return true;
	}

	public virtual void DrawHighlight(LocalTargetInfo target)
	{
		GenDraw.DrawRadiusRing(caller.Position, RangeClamped, Color.white);
		if (target.IsValid)
		{
			GenDraw.DrawTargetHighlight(target);
		}
	}

	public virtual void OrderForceTarget(LocalTargetInfo target)
	{
	}

	public virtual void OnGUI(LocalTargetInfo target)
	{
		Texture2D icon = ((!target.IsValid) ? TexCommand.CannotShoot : ((!(UIIcon != null) || !(UIIcon != BaseContent.BadTex)) ? TexCommand.Attack : UIIcon));
		GenUI.DrawMouseAttachment(icon);
	}
}
