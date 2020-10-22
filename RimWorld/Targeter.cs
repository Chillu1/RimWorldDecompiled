using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace RimWorld
{
	public class Targeter
	{
		public ITargetingSource targetingSource;

		public ITargetingSource targetingSourceParent;

		public List<Pawn> targetingSourceAdditionalPawns;

		private Action<LocalTargetInfo> action;

		private Pawn caster;

		private TargetingParameters targetParams;

		private Action actionWhenFinished;

		private Texture2D mouseAttachment;

		private Action<LocalTargetInfo> highlightAction;

		private Func<LocalTargetInfo, bool> targetValidator;

		private bool needsStopTargetingCall;

		public bool IsTargeting
		{
			get
			{
				if (targetingSource == null)
				{
					return action != null;
				}
				return true;
			}
		}

		public void BeginTargeting(ITargetingSource source, ITargetingSource parent = null)
		{
			if (source.Targetable)
			{
				targetingSource = source;
				targetingSourceAdditionalPawns = new List<Pawn>();
			}
			else
			{
				Verb getVerb = source.GetVerb;
				if (getVerb.verbProps.nonInterruptingSelfCast)
				{
					getVerb.TryStartCastOn(getVerb.Caster);
					return;
				}
				Job job = JobMaker.MakeJob(JobDefOf.UseVerbOnThing, getVerb.Caster);
				job.verbToUse = getVerb;
				source.CasterPawn.jobs.StartJob(job, JobCondition.InterruptForced);
			}
			action = null;
			caster = null;
			targetParams = null;
			actionWhenFinished = null;
			mouseAttachment = null;
			targetingSourceParent = parent;
			needsStopTargetingCall = false;
		}

		public void BeginTargeting(TargetingParameters targetParams, Action<LocalTargetInfo> action, Pawn caster = null, Action actionWhenFinished = null, Texture2D mouseAttachment = null)
		{
			targetingSource = null;
			targetingSourceParent = null;
			targetingSourceAdditionalPawns = null;
			this.action = action;
			this.targetParams = targetParams;
			this.caster = caster;
			this.actionWhenFinished = actionWhenFinished;
			this.mouseAttachment = mouseAttachment;
			highlightAction = null;
			targetValidator = null;
			needsStopTargetingCall = false;
		}

		public void BeginTargeting(TargetingParameters targetParams, Action<LocalTargetInfo> action, Action<LocalTargetInfo> highlightAction, Func<LocalTargetInfo, bool> targetValidator, Pawn caster = null, Action actionWhenFinished = null, Texture2D mouseAttachment = null)
		{
			targetingSource = null;
			targetingSourceParent = null;
			targetingSourceAdditionalPawns = null;
			this.action = action;
			this.targetParams = targetParams;
			this.caster = caster;
			this.actionWhenFinished = actionWhenFinished;
			this.mouseAttachment = mouseAttachment;
			this.highlightAction = highlightAction;
			this.targetValidator = targetValidator;
			needsStopTargetingCall = false;
		}

		public void BeginTargeting(TargetingParameters targetParams, ITargetingSource ability, Action<LocalTargetInfo> action, Action actionWhenFinished = null, Texture2D mouseAttachment = null)
		{
			targetingSource = null;
			targetingSourceParent = null;
			targetingSourceAdditionalPawns = null;
			this.action = action;
			this.actionWhenFinished = actionWhenFinished;
			caster = null;
			this.targetParams = targetParams;
			this.mouseAttachment = mouseAttachment;
			targetingSource = ability;
			highlightAction = null;
			targetValidator = null;
			needsStopTargetingCall = false;
		}

		public void StopTargeting()
		{
			if (actionWhenFinished != null)
			{
				Action obj = actionWhenFinished;
				actionWhenFinished = null;
				obj();
			}
			targetingSource = null;
			action = null;
			targetParams = null;
			highlightAction = null;
			targetValidator = null;
		}

		public void ProcessInputEvents()
		{
			ConfirmStillValid();
			if (!IsTargeting)
			{
				return;
			}
			if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
			{
				LocalTargetInfo localTargetInfo = CurrentTargetUnderMouse(mustBeHittableNowIfNotMelee: false);
				needsStopTargetingCall = true;
				if (targetingSource != null)
				{
					if (!targetingSource.ValidateTarget(localTargetInfo))
					{
						Event.current.Use();
						return;
					}
					OrderVerbForceTarget();
				}
				if (action != null)
				{
					if (targetValidator != null)
					{
						if (targetValidator(localTargetInfo))
						{
							action(localTargetInfo);
						}
						else
						{
							needsStopTargetingCall = false;
						}
					}
					else if (localTargetInfo.IsValid)
					{
						action(localTargetInfo);
					}
				}
				SoundDefOf.Tick_High.PlayOneShotOnCamera();
				if (targetingSource != null)
				{
					if (targetingSource.DestinationSelector != null)
					{
						BeginTargeting(targetingSource.DestinationSelector, targetingSource);
					}
					else if (targetingSource.MultiSelect && Event.current.shift)
					{
						BeginTargeting(targetingSource);
					}
					else if (targetingSourceParent != null && targetingSourceParent.MultiSelect && Event.current.shift)
					{
						BeginTargeting(targetingSourceParent);
					}
				}
				if (needsStopTargetingCall)
				{
					StopTargeting();
				}
				Event.current.Use();
			}
			if ((Event.current.type == EventType.MouseDown && Event.current.button == 1) || KeyBindingDefOf.Cancel.KeyDownEvent)
			{
				SoundDefOf.CancelMode.PlayOneShotOnCamera();
				StopTargeting();
				Event.current.Use();
			}
		}

		public void TargeterOnGUI()
		{
			if (targetingSource != null)
			{
				LocalTargetInfo target = CurrentTargetUnderMouse(mustBeHittableNowIfNotMelee: true);
				targetingSource.OnGUI(target);
			}
			if (action != null)
			{
				GenUI.DrawMouseAttachment(mouseAttachment ?? TexCommand.Attack);
			}
		}

		public void TargeterUpdate()
		{
			if (targetingSource != null)
			{
				targetingSource.DrawHighlight(CurrentTargetUnderMouse(mustBeHittableNowIfNotMelee: true));
			}
			if (action != null)
			{
				LocalTargetInfo localTargetInfo = CurrentTargetUnderMouse(mustBeHittableNowIfNotMelee: false);
				if (highlightAction != null)
				{
					highlightAction(localTargetInfo);
				}
				else if (localTargetInfo.IsValid)
				{
					GenDraw.DrawTargetHighlight(localTargetInfo);
				}
			}
		}

		public bool IsPawnTargeting(Pawn p)
		{
			if (caster == p)
			{
				return true;
			}
			if (targetingSource != null && targetingSource.CasterIsPawn)
			{
				if (targetingSource.CasterPawn == p)
				{
					return true;
				}
				for (int i = 0; i < targetingSourceAdditionalPawns.Count; i++)
				{
					if (targetingSourceAdditionalPawns[i] == p)
					{
						return true;
					}
				}
			}
			return false;
		}

		private void ConfirmStillValid()
		{
			if (caster != null && (caster.Map != Find.CurrentMap || caster.Destroyed || !Find.Selector.IsSelected(caster)))
			{
				StopTargeting();
			}
			if (targetingSource == null)
			{
				return;
			}
			Selector selector = Find.Selector;
			if (targetingSource.Caster.Map != Find.CurrentMap || targetingSource.Caster.Destroyed || !selector.IsSelected(targetingSource.Caster) || (targetingSource.GetVerb != null && !targetingSource.GetVerb.Available()))
			{
				StopTargeting();
				return;
			}
			for (int i = 0; i < targetingSourceAdditionalPawns.Count; i++)
			{
				if (targetingSourceAdditionalPawns[i].Destroyed || !selector.IsSelected(targetingSourceAdditionalPawns[i]))
				{
					StopTargeting();
					break;
				}
			}
		}

		private void OrderVerbForceTarget()
		{
			if (targetingSource.CasterIsPawn)
			{
				OrderPawnForceTarget(targetingSource);
				for (int i = 0; i < targetingSourceAdditionalPawns.Count; i++)
				{
					Verb targetingVerb = GetTargetingVerb(targetingSourceAdditionalPawns[i]);
					if (targetingVerb != null)
					{
						OrderPawnForceTarget(targetingVerb);
					}
				}
				return;
			}
			int numSelected = Find.Selector.NumSelected;
			List<object> selectedObjects = Find.Selector.SelectedObjects;
			for (int j = 0; j < numSelected; j++)
			{
				Building_Turret building_Turret = selectedObjects[j] as Building_Turret;
				if (building_Turret != null && building_Turret.Map == Find.CurrentMap)
				{
					LocalTargetInfo targ = CurrentTargetUnderMouse(mustBeHittableNowIfNotMelee: true);
					building_Turret.OrderAttack(targ);
				}
			}
		}

		public void OrderPawnForceTarget(ITargetingSource targetingSource)
		{
			LocalTargetInfo target = CurrentTargetUnderMouse(mustBeHittableNowIfNotMelee: true);
			if (target.IsValid)
			{
				targetingSource.OrderForceTarget(target);
			}
		}

		private LocalTargetInfo CurrentTargetUnderMouse(bool mustBeHittableNowIfNotMelee)
		{
			if (!IsTargeting)
			{
				return LocalTargetInfo.Invalid;
			}
			TargetingParameters targetingParameters = ((targetingSource != null) ? targetingSource.targetParams : targetParams);
			LocalTargetInfo localTargetInfo = GenUI.TargetsAtMouse_NewTemp(targetingParameters, thingsOnly: false, targetingSource).FirstOrFallback(LocalTargetInfo.Invalid);
			if (localTargetInfo.IsValid && targetingSource != null)
			{
				if (mustBeHittableNowIfNotMelee && !(localTargetInfo.Thing is Pawn) && !targetingSource.IsMeleeAttack)
				{
					if (targetingSourceAdditionalPawns != null && targetingSourceAdditionalPawns.Any())
					{
						bool flag = false;
						for (int i = 0; i < targetingSourceAdditionalPawns.Count; i++)
						{
							Verb targetingVerb = GetTargetingVerb(targetingSourceAdditionalPawns[i]);
							if (targetingVerb != null && targetingVerb.CanHitTarget(localTargetInfo))
							{
								flag = true;
								break;
							}
						}
						if (!flag)
						{
							localTargetInfo = LocalTargetInfo.Invalid;
						}
					}
					else if (!targetingSource.CanHitTarget(localTargetInfo))
					{
						localTargetInfo = LocalTargetInfo.Invalid;
					}
				}
				if (localTargetInfo == targetingSource.Caster && !targetingParameters.canTargetSelf)
				{
					localTargetInfo = LocalTargetInfo.Invalid;
				}
			}
			return localTargetInfo;
		}

		private Verb GetTargetingVerb(Pawn pawn)
		{
			Verb verb = pawn.equipment?.AllEquipmentVerbs.FirstOrDefault(SameVerb);
			if (verb != null)
			{
				return verb;
			}
			return pawn.apparel?.AllApparelVerbs.FirstOrDefault(SameVerb);
			bool SameVerb(Verb x)
			{
				return x.verbProps == targetingSource.GetVerb.verbProps;
			}
		}
	}
}
