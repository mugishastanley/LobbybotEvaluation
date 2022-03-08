using System.Collections.Generic;
using UnityEngine;

namespace CLARTE.Scenario
{
	public class AnyValidator : GroupValidator<HashSet<Validator>>
	{
		#region Members
		protected Validator validated;
		#endregion

		#region Validator implementation
		protected override void OnStateChanged(ValidatorState state)
		{
			switch(state)
			{
				case ValidatorState.VALIDATED:
					foreach(Validator v in children)
					{
						if(v != validated)
						{
							v.SetState(ValidatorState.DISABLED, false);
						}
					}

					break;
				default:
					validated = null;

					foreach(Validator v in children)
					{
						v.SetState(state, false);
					}

					break;
			}
		}

		protected override void RefreshState(ValidatorState state)
		{
			switch(state)
			{
				case ValidatorState.ENABLED:
				case ValidatorState.HIGHLIGHTED:
				case ValidatorState.VALIDATED:
					validated = null;

					foreach(Validator v in children)
					{
						if(v == null || v.State == ValidatorState.FAILED)
						{
							SetState(ValidatorState.FAILED);

							validated = null;

							break;
						}
						else if(v.State == ValidatorState.VALIDATED && validated == null)
						{
							validated = v;

							break;
						}
					}

					if(validated != null)
					{
						SetState(ValidatorState.VALIDATED);
					}
					else if(state == ValidatorState.VALIDATED)
					{
						SetState(Previous);
					}

					break;
				default:
					break;
			}
		}

		protected override void ComputeScore(out float score, out float weight)
		{
			if(validated != null && validated.State == ValidatorState.VALIDATED)
			{
				score = validated.Score;
				weight = validated.ScoreWeight;
			}
			else
			{
				score = 0;
				weight = 1;
			}
		}
		#endregion
	}
}
