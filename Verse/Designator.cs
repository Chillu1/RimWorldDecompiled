using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse.Sound;

namespace Verse
{
	public abstract class Designator : Command
	{
		protected bool useMouseIcon;

		public bool isOrder;

		public SoundDef soundDragSustain;

		public SoundDef soundDragChanged;

		protected SoundDef soundSucceeded;

		protected SoundDef soundFailed = SoundDefOf.Designate_Failed;

		protected bool hasDesignateAllFloatMenuOption;

		protected string designateAllLabel;

		private string cachedTutorTagSelect;

		private string cachedTutorTagDesignate;

		protected string cachedHighlightTag;

		public Map Map => Find.CurrentMap;

		public virtual int DraggableDimensions => 0;

		public virtual bool DragDrawMeasurements => false;

		protected override bool DoTooltip => false;

		protected virtual DesignationDef Designation => null;

		public virtual float PanelReadoutTitleExtraRightMargin => 0f;

		public override string TutorTagSelect
		{
			get
			{
				if (tutorTag == null)
				{
					return null;
				}
				if (cachedTutorTagSelect == null)
				{
					cachedTutorTagSelect = "SelectDesignator-" + tutorTag;
				}
				return cachedTutorTagSelect;
			}
		}

		public string TutorTagDesignate
		{
			get
			{
				if (tutorTag == null)
				{
					return null;
				}
				if (cachedTutorTagDesignate == null)
				{
					cachedTutorTagDesignate = "Designate-" + tutorTag;
				}
				return cachedTutorTagDesignate;
			}
		}

		public override string HighlightTag
		{
			get
			{
				if (cachedHighlightTag == null && tutorTag != null)
				{
					cachedHighlightTag = "Designator-" + tutorTag;
				}
				return cachedHighlightTag;
			}
		}

		public override IEnumerable<FloatMenuOption> RightClickFloatMenuOptions
		{
			get
			{
				_003C_003Ec__DisplayClass30_0 _003C_003Ec__DisplayClass30_ = new _003C_003Ec__DisplayClass30_0();
				_003C_003Ec__DisplayClass30_._003C_003E4__this = this;
				foreach (FloatMenuOption rightClickFloatMenuOption in base.RightClickFloatMenuOptions)
				{
					yield return rightClickFloatMenuOption;
				}
				if (hasDesignateAllFloatMenuOption)
				{
					_003C_003Ec__DisplayClass30_0 _003C_003Ec__DisplayClass30_2 = _003C_003Ec__DisplayClass30_;
					int num = 0;
					List<Thing> things = Map.listerThings.AllThings;
					for (int i = 0; i < things.Count; i++)
					{
						Thing t = things[i];
						if (!t.Fogged() && CanDesignateThing(t).Accepted)
						{
							num++;
						}
					}
					if (num > 0)
					{
						yield return new FloatMenuOption(designateAllLabel + " (" + "CountToDesignate".Translate(num) + ")", delegate
						{
							for (int k = 0; k < things.Count; k++)
							{
								Thing t2 = things[k];
								if (!t2.Fogged() && _003C_003Ec__DisplayClass30_2._003C_003E4__this.CanDesignateThing(t2).Accepted)
								{
									_003C_003Ec__DisplayClass30_2._003C_003E4__this.DesignateThing(things[k]);
								}
							}
						});
					}
					else
					{
						yield return new FloatMenuOption(designateAllLabel + " (" + "NoneLower".Translate() + ")", null);
					}
				}
				_003C_003Ec__DisplayClass30_.designation = Designation;
				if (Designation == null)
				{
					yield break;
				}
				_003C_003Ec__DisplayClass30_0 _003C_003Ec__DisplayClass30_3 = _003C_003Ec__DisplayClass30_;
				int num2 = 0;
				List<Designation> designations = Map.designationManager.allDesignations;
				for (int j = 0; j < designations.Count; j++)
				{
					if (designations[j].def == _003C_003Ec__DisplayClass30_3.designation && RemoveAllDesignationsAffects(designations[j].target))
					{
						num2++;
					}
				}
				if (num2 > 0)
				{
					yield return new FloatMenuOption((string)("RemoveAllDesignations".Translate() + " (") + num2 + ")", delegate
					{
						for (int num3 = designations.Count - 1; num3 >= 0; num3--)
						{
							if (designations[num3].def == _003C_003Ec__DisplayClass30_3.designation && _003C_003Ec__DisplayClass30_3._003C_003E4__this.RemoveAllDesignationsAffects(designations[num3].target))
							{
								_003C_003Ec__DisplayClass30_3._003C_003E4__this.Map.designationManager.RemoveDesignation(designations[num3]);
							}
						}
					});
				}
				else
				{
					yield return new FloatMenuOption("RemoveAllDesignations".Translate() + " (" + "NoneLower".Translate() + ")", null);
				}
			}
		}

		public Designator()
		{
			activateSound = SoundDefOf.Tick_Tiny;
			designateAllLabel = "DesignateAll".Translate();
		}

		protected bool CheckCanInteract()
		{
			if (TutorSystem.TutorialMode && !TutorSystem.AllowAction(TutorTagSelect))
			{
				return false;
			}
			return true;
		}

		public override void ProcessInput(Event ev)
		{
			if (CheckCanInteract())
			{
				base.ProcessInput(ev);
				Find.DesignatorManager.Select(this);
			}
		}

		public virtual AcceptanceReport CanDesignateThing(Thing t)
		{
			return AcceptanceReport.WasRejected;
		}

		public virtual void DesignateThing(Thing t)
		{
			throw new NotImplementedException();
		}

		public abstract AcceptanceReport CanDesignateCell(IntVec3 loc);

		public virtual void DesignateMultiCell(IEnumerable<IntVec3> cells)
		{
			if (TutorSystem.TutorialMode && !TutorSystem.AllowAction(new EventPack(TutorTagDesignate, cells)))
			{
				return;
			}
			bool somethingSucceeded = false;
			bool flag = false;
			foreach (IntVec3 cell in cells)
			{
				if (CanDesignateCell(cell).Accepted)
				{
					DesignateSingleCell(cell);
					somethingSucceeded = true;
					if (!flag)
					{
						flag = ShowWarningForCell(cell);
					}
				}
			}
			Finalize(somethingSucceeded);
			if (TutorSystem.TutorialMode)
			{
				TutorSystem.Notify_Event(new EventPack(TutorTagDesignate, cells));
			}
		}

		public virtual void DesignateSingleCell(IntVec3 c)
		{
			throw new NotImplementedException();
		}

		public virtual bool ShowWarningForCell(IntVec3 c)
		{
			return false;
		}

		public new void Finalize(bool somethingSucceeded)
		{
			if (somethingSucceeded)
			{
				FinalizeDesignationSucceeded();
			}
			else
			{
				FinalizeDesignationFailed();
			}
		}

		protected virtual void FinalizeDesignationSucceeded()
		{
			if (soundSucceeded != null)
			{
				soundSucceeded.PlayOneShotOnCamera();
			}
		}

		protected virtual void FinalizeDesignationFailed()
		{
			if (soundFailed != null)
			{
				soundFailed.PlayOneShotOnCamera();
			}
			if (Find.DesignatorManager.Dragger.FailureReason != null)
			{
				Messages.Message(Find.DesignatorManager.Dragger.FailureReason, MessageTypeDefOf.RejectInput, historical: false);
			}
		}

		public virtual string LabelCapReverseDesignating(Thing t)
		{
			return LabelCap;
		}

		public virtual string DescReverseDesignating(Thing t)
		{
			return Desc;
		}

		public virtual Texture2D IconReverseDesignating(Thing t, out float angle, out Vector2 offset)
		{
			angle = iconAngle;
			offset = iconOffset;
			return icon;
		}

		protected virtual bool RemoveAllDesignationsAffects(LocalTargetInfo target)
		{
			return true;
		}

		public virtual void DrawMouseAttachments()
		{
			if (useMouseIcon)
			{
				GenUI.DrawMouseAttachment(icon, "", iconAngle, iconOffset);
			}
		}

		public virtual void DrawPanelReadout(ref float curY, float width)
		{
		}

		public virtual void DoExtraGuiControls(float leftX, float bottomY)
		{
		}

		public virtual void SelectedUpdate()
		{
		}

		public virtual void SelectedProcessInput(Event ev)
		{
		}

		public virtual void Rotate(RotationDirection rotDir)
		{
		}

		public virtual bool CanRemainSelected()
		{
			return true;
		}

		public virtual void Selected()
		{
		}

		public virtual void RenderHighlight(List<IntVec3> dragCells)
		{
			DesignatorUtility.RenderHighlightOverSelectableThings(this, dragCells);
		}
	}
}
