using System.Collections.Generic;
using UnityEngine;

namespace CLARTE.Scenario
{
	public class SequenceValidator : GroupValidator<List<Validator>>
	{
		public enum Type
		{
			STRICT,
			SCORE_CONSTANT_PENALTY,
			SCORE_DISTANCE_PENALTY
		}

		#region Members
		public Type type;
		[Range(0f, 1f)]
		public float constantPenaltyFactor = 0.5f;

		protected List<bool> validated;
		protected List<int> validatedIndexes;
		protected int current;
		#endregion

		#region MonoBehaviour callbacks
		protected override void Awake()
		{
			validated = new List<bool>(transform.childCount);
			validatedIndexes = new List<int>(transform.childCount);

			base.Awake();

			Cleanup();
		}
		#endregion

		#region Validator implementation
		protected override void OnStateChanged(ValidatorState state)
		{
			Cleanup();

			switch(state)
			{
				case ValidatorState.ENABLED:
				case ValidatorState.HIGHLIGHTED:
				case ValidatorState.VALIDATED:
					int nb_children = children.Count;

					current = nb_children;

					for(int i = 0; i < nb_children; i++)
					{
						if(children[i].State == ValidatorState.VALIDATED)
						{
							validatedIndexes.Add(i);
							validated[i] = true;
						}
						else
						{
							current = i;

							break;
						}
					}

					for(int i = current + 1; i < nb_children; i++)
					{
						children[i].SetState(type == Type.STRICT ? ValidatorState.DISABLED : state, false);
					}

					if(current < children.Count)
					{
						if(state != ValidatorState.VALIDATED)
						{
							children[current].SetState(state, false);
						}
						else
						{
							SetState(Previous);
						}
					}
					else if(state != ValidatorState.VALIDATED)
					{
						SetState(ValidatorState.VALIDATED);
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
					int nb_children = children.Count;

					switch(type)
					{
						case Type.STRICT:
							for(int i = 0; i < current && i < nb_children; i++)
							{
								if(children[i].State != ValidatorState.VALIDATED)
								{
									Reset(state);

									return;
								}
							}

							while(current < nb_children && children[current].State == ValidatorState.VALIDATED)
							{
								validatedIndexes.Add(current);

								current++;

								if(current < nb_children)
								{
									children[current].SetState(state, false);
								}
							}

							if(current >= nb_children)
							{
								SetState(ValidatorState.VALIDATED);
							}

							break;
						default:
							for(int i = 0; i < nb_children; i++)
							{
								if((children[i].State == ValidatorState.VALIDATED) != validated[i])
								{
									if(validated[i])
									{
										Reset(state);

										return;
									}
									else
									{
										validated[i] = true;
										validatedIndexes.Add(i);
									}
								}
							}

							if(validatedIndexes.Count == nb_children)
							{
								SetState(ValidatorState.VALIDATED);
							}

							break;
					}

					break;
				default:
					break;
			}
		}

		protected override void ComputeScore(out float score, out float weight)
		{
			score = weight = 0;

			int count = children.Count;
			int nb_validated = validatedIndexes.Count;

			for(int i = 0; i < count; i++)
			{
				weight += children[i].ScoreWeight;

				if(i < nb_validated)
				{
					float child_score = children[validatedIndexes[i]].Score;

					if(validatedIndexes[i] == i)
					{
						score += child_score;
					}
					else
					{
						switch(type)
						{
							case Type.STRICT:
								score += child_score;
								break;
							case Type.SCORE_CONSTANT_PENALTY:
								score += child_score * constantPenaltyFactor;
								break;
							case Type.SCORE_DISTANCE_PENALTY:
								score += child_score / (1 + Mathf.Abs(validatedIndexes[i] - i));
								break;
								
						}
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

		#region Internal methods
		protected void Cleanup()
		{
			validated.Clear();
			validatedIndexes.Clear();

			current = 0;

			foreach(Validator v in children)
			{
				validated.Add(false);
			}
		}

		protected void Reset(ValidatorState state)
		{
			ForceStateChange(state == ValidatorState.VALIDATED ? Previous : state);
		}
		#endregion
	}
}
