using System.Collections.Generic;
using UnityEngine;

namespace CLARTE.Scenario
{
	public class AllValidator : GroupValidator<List<Validator>>
	{
        #region Accessors
		public int ChildCount { get => children.Count; }
        #endregion

        #region Validator implementation
        protected override void OnStateChanged(ValidatorState state)
		{
			switch(state)
			{
				case ValidatorState.ENABLED:
				case ValidatorState.HIGHLIGHTED:
					foreach(Validator v in children)
					{
						if(v.State != ValidatorState.VALIDATED)
						{
							v.SetState(state, false);
						}
					}

					break;
				default:
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
					bool validated = true;

					foreach(Validator v in children)
					{
						if(v == null || v.State == ValidatorState.FAILED)
						{
							SetState(ValidatorState.FAILED);

							validated = false;

							break;
						}
						else if(v.State != ValidatorState.VALIDATED)
						{
							validated = false;

							break;
						}
					}

					if(validated)
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
			score = weight = 0;

			foreach(Validator v in children)
			{
				if(v != null)
				{
					weight += v.ScoreWeight;

					if(v.State == ValidatorState.VALIDATED)
					{
						score += v.Score;
					}
				}
			}

			if(Mathf.Abs(weight) < Vector3.kEpsilon)
			{
				score = 0;
				weight = 1;
			}
		}
		#endregion
	}
}
