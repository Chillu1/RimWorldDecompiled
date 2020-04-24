using UnityEngine;
using Verse;

namespace RimWorld
{
	public abstract class Lesson : IExposable
	{
		public float startRealTime = -999f;

		public const float KnowledgeForAutoVanish = 0.2f;

		protected float AgeSeconds
		{
			get
			{
				if (startRealTime < 0f)
				{
					startRealTime = Time.realtimeSinceStartup;
				}
				return Time.realtimeSinceStartup - startRealTime;
			}
		}

		public virtual ConceptDef Concept => null;

		public virtual InstructionDef Instruction => null;

		public virtual float MessagesYOffset => 0f;

		public virtual string DefaultRejectInputMessage => null;

		public virtual void ExposeData()
		{
		}

		public virtual void OnActivated()
		{
			startRealTime = Time.realtimeSinceStartup;
		}

		public virtual void PostDeactivated()
		{
		}

		public abstract void LessonOnGUI();

		public virtual void LessonUpdate()
		{
		}

		public virtual void Notify_KnowledgeDemonstrated(ConceptDef conc)
		{
		}

		public virtual void Notify_Event(EventPack ep)
		{
		}

		public virtual AcceptanceReport AllowAction(EventPack ep)
		{
			return true;
		}
	}
}
