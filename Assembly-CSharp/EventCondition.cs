using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class EventCondition
{
	public bool Test(Animator animator)
	{
		if (this.conditions.Count == 0)
		{
			return true;
		}
		for (int i = 0; i < this.conditions.Count; i++)
		{
			EventConditionEntry eventConditionEntry = this.conditions[i];
			if (!string.IsNullOrEmpty(eventConditionEntry.conditionParam))
			{
				EventConditionParamTypes conditionParamType = eventConditionEntry.conditionParamType;
				if (conditionParamType != EventConditionParamTypes.Int)
				{
					if (conditionParamType != EventConditionParamTypes.Float)
					{
						if (conditionParamType == EventConditionParamTypes.Boolean)
						{
							bool @bool = animator.GetBool(eventConditionEntry.conditionParam);
							if (@bool != eventConditionEntry.boolValue)
							{
								return false;
							}
						}
					}
					else
					{
						float @float = animator.GetFloat(eventConditionEntry.conditionParam);
						EventConditionModes conditionMode = eventConditionEntry.conditionMode;
						if (conditionMode != EventConditionModes.GreaterThan)
						{
							if (conditionMode == EventConditionModes.LessThan)
							{
								if (@float >= eventConditionEntry.floatValue)
								{
									return false;
								}
							}
						}
						else if (@float <= eventConditionEntry.floatValue)
						{
							return false;
						}
					}
				}
				else
				{
					int integer = animator.GetInteger(eventConditionEntry.conditionParam);
					switch (eventConditionEntry.conditionMode)
					{
					case EventConditionModes.Equal:
						if (integer != eventConditionEntry.intValue)
						{
							return false;
						}
						break;
					case EventConditionModes.NotEqual:
						if (integer == eventConditionEntry.intValue)
						{
							return false;
						}
						break;
					case EventConditionModes.GreaterThan:
						if (integer <= eventConditionEntry.intValue)
						{
							return false;
						}
						break;
					case EventConditionModes.LessThan:
						if (integer >= eventConditionEntry.intValue)
						{
							return false;
						}
						break;
					case EventConditionModes.GreaterEqualThan:
						if (integer < eventConditionEntry.intValue)
						{
							return false;
						}
						break;
					case EventConditionModes.LessEqualThan:
						if (integer > eventConditionEntry.intValue)
						{
							return false;
						}
						break;
					}
				}
			}
		}
		return true;
	}

	public void Reset()
	{
		this.conditions.Clear();
	}

	public List<EventConditionEntry> conditions = new List<EventConditionEntry>();
}
