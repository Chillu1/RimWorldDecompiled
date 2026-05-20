using System;
using System.Linq;
using RimWorld;

namespace Verse.AI.Group;

public class TransitionAction_Message : TransitionAction
{
	public string message;

	public MessageTypeDef type;

	public TargetInfo lookTarget = TargetInfo.Invalid;

	public Func<TargetInfo> lookTargetGetter;

	public string repeatAvoiderTag;

	public float repeatAvoiderSeconds;

	private Func<bool> canSendMessage;

	public TransitionAction_Message(string message, string repeatAvoiderTag = null, float repeatAvoiderSeconds = 1f)
		: this(message, MessageTypeDefOf.NeutralEvent, repeatAvoiderTag, repeatAvoiderSeconds)
	{
	}

	public TransitionAction_Message(string message, MessageTypeDef messageType, string repeatAvoiderTag = null, float repeatAvoiderSeconds = 1f, Func<bool> canSendMessage = null)
	{
		this.message = message;
		type = messageType;
		this.repeatAvoiderTag = repeatAvoiderTag;
		this.repeatAvoiderSeconds = repeatAvoiderSeconds;
		this.canSendMessage = canSendMessage;
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
		if ((repeatAvoiderTag.NullOrEmpty() || MessagesRepeatAvoider.MessageShowAllowed(repeatAvoiderTag, repeatAvoiderSeconds)) && (canSendMessage == null || canSendMessage()))
		{
			TargetInfo targetInfo = ((lookTargetGetter != null) ? lookTargetGetter() : lookTarget);
			if (!targetInfo.IsValid)
			{
				targetInfo = trans.target.lord.ownedPawns.FirstOrDefault();
			}
			Messages.Message(message, targetInfo, type);
		}
	}
}
