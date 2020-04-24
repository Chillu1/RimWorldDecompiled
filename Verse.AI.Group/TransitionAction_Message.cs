using RimWorld;
using System;
using System.Linq;

namespace Verse.AI.Group
{
	public class TransitionAction_Message : TransitionAction
	{
		public string message;

		public MessageTypeDef type;

		public TargetInfo lookTarget = TargetInfo.Invalid;

		public Func<TargetInfo> lookTargetGetter;

		public string repeatAvoiderTag;

		public float repeatAvoiderSeconds;

		public TransitionAction_Message(string message, string repeatAvoiderTag = null, float repeatAvoiderSeconds = 1f)
			: this(message, MessageTypeDefOf.NeutralEvent, repeatAvoiderTag, repeatAvoiderSeconds)
		{
		}

		public TransitionAction_Message(string message, MessageTypeDef messageType, string repeatAvoiderTag = null, float repeatAvoiderSeconds = 1f)
		{
			this.message = message;
			type = messageType;
			this.repeatAvoiderTag = repeatAvoiderTag;
			this.repeatAvoiderSeconds = repeatAvoiderSeconds;
		}

		public TransitionAction_Message(string message, MessageTypeDef messageType, TargetInfo lookTarget, string repeatAvoiderTag = null, float repeatAvoiderSeconds = 1f)
		{
			this.message = message;
			type = messageType;
			this.lookTarget = lookTarget;
			this.repeatAvoiderTag = repeatAvoiderTag;
			this.repeatAvoiderSeconds = repeatAvoiderSeconds;
		}

		public TransitionAction_Message(string message, MessageTypeDef messageType, Func<TargetInfo> lookTargetGetter, string repeatAvoiderTag = null, float repeatAvoiderSeconds = 1f)
		{
			this.message = message;
			type = messageType;
			this.lookTargetGetter = lookTargetGetter;
			this.repeatAvoiderTag = repeatAvoiderTag;
			this.repeatAvoiderSeconds = repeatAvoiderSeconds;
		}

		public override void DoAction(Transition trans)
		{
			if (repeatAvoiderTag.NullOrEmpty() || MessagesRepeatAvoider.MessageShowAllowed(repeatAvoiderTag, repeatAvoiderSeconds))
			{
				TargetInfo target = (lookTargetGetter != null) ? lookTargetGetter() : lookTarget;
				if (!target.IsValid)
				{
					target = trans.target.lord.ownedPawns.FirstOrDefault();
				}
				Messages.Message(message, target, type);
			}
		}
	}
}
