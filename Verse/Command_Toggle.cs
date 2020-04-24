using RimWorld;
using System;
using UnityEngine;

namespace Verse
{
	public class Command_Toggle : Command
	{
		public Func<bool> isActive;

		public Action toggleAction;

		public SoundDef turnOnSound = SoundDefOf.Checkbox_TurnedOn;

		public SoundDef turnOffSound = SoundDefOf.Checkbox_TurnedOff;

		public bool activateIfAmbiguous = true;

		public override SoundDef CurActivateSound
		{
			get
			{
				if (isActive())
				{
					return turnOffSound;
				}
				return turnOnSound;
			}
		}

		public override void ProcessInput(Event ev)
		{
			base.ProcessInput(ev);
			toggleAction();
		}

		public override GizmoResult GizmoOnGUI(Vector2 loc, float maxWidth)
		{
			GizmoResult result = base.GizmoOnGUI(loc, maxWidth);
			Rect rect = new Rect(loc.x, loc.y, GetWidth(maxWidth), 75f);
			Rect position = new Rect(rect.x + rect.width - 24f, rect.y, 24f, 24f);
			Texture2D image = isActive() ? Widgets.CheckboxOnTex : Widgets.CheckboxOffTex;
			GUI.DrawTexture(position, image);
			return result;
		}

		public override bool InheritInteractionsFrom(Gizmo other)
		{
			Command_Toggle command_Toggle = other as Command_Toggle;
			if (command_Toggle != null)
			{
				return command_Toggle.isActive() == isActive();
			}
			return false;
		}
	}
}
