using UnityEngine;

namespace CLARTE.Scenario
{
	public abstract class Validator : MonoBehaviour
	{
		#region Members
		private ValidatorState state;
		private float score;
		private float scoreWeight;
		#endregion

		#region Abstract methods
		protected virtual void OnStateChanged(ValidatorState state) { }
		protected abstract void RefreshState(ValidatorState state);
		protected abstract void ComputeScore(out float score, out float weight);
		#endregion

		#region Getters / Setters
		protected ValidatorState Previous { get; private set; }

		public ValidatorState State
		{
			get
			{
				RefreshState(state);

				return state;
			}
		}

		public float Score
		{
			get
			{
				return score;
			}
		}

		public float ScoreWeight
		{
			get
			{
				return scoreWeight;
			}
		}
		#endregion

		#region MonoBehaviour callbacks
		protected virtual void Awake()
		{
			ForceStateChange(ValidatorState.DISABLED, false);
		}
		#endregion

		#region Public methods
		public void SetState(ValidatorState new_state, bool notify_parent = true)
		{
			if(state != new_state)
			{
				ForceStateChange(new_state, notify_parent);
			}
		}
		#endregion

		#region Internal methods
		protected void ForceStateChange(ValidatorState new_state, bool notify_parent = true)
		{
			Previous = state;

			state = new_state;

			OnStateChanged(state);

			RefreshState(state);

			ComputeScore(out score, out scoreWeight);

			Validator parent = transform.parent?.GetComponent<Validator>();

			if(notify_parent && parent != null)
			{
				parent.RefreshState(parent.state);
				parent.ComputeScore(out parent.score, out parent.scoreWeight);
			}
		}
		#endregion
	}
}
