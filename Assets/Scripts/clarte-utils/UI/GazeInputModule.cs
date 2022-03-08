using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// UI Input module based on user gaze.
/// </summary>
/// <remarks>
/// To use:
/// 1. Drag onto your EventSystem game object.
/// 2. Disable any other Input Modules (eg: StandaloneInputModule & TouchInputModule) as they will fight over selections.
/// 3. Make sure your Canvas is in world space and has a GraphicRaycaster (should by default).
/// 4. If you have multiple cameras then make sure to drag your VR (center eye) camera into the canvas.
/// 
/// This imput module also allow input from mouse. This is useful to interact on UI on computer screen while another user
/// is interacting with gaze in VR application for example.
/// </remarks>
namespace CLARTE.UI
{
	public class GazeInputModule : PointerInputModule
	{
		#region Members
		[Header("Settings")]
		[Range(0.1f, 5f)]
		public float gazeTimeInSeconds = 1.5f;
		[Range(-1f, 1f)]
		public float verticalOffset = 0.2f;
		public bool useTimeBasedClick = true;
		public bool addPhysicsRaycaster = true;

		[Header("Assets")]
		public Camera eventCamera;
		public Shader overlayShader;
		public Sprite crosshair;
		public Sprite visualFeedback;
		public AudioClip audioFeedback;

		private PointerEventData pointerEventData;
		private List<RaycastResult> raycastResults;
		private GameObject selectedObject;
		private GameObject pointer;
		private Image progressRange;
		private AudioSource audioSource;
		private float selectionTime;
		private bool externalClick;
		#endregion

		#region MonoBehaviour callbacks
		protected override void Awake()
		{
			base.Awake();

			raycastResults = new List<RaycastResult>();

			if(eventCamera == null)
			{
				eventCamera = Camera.main;
			}

			if(eventCamera != null)
			{
				if(addPhysicsRaycaster)
				{
					PhysicsRaycaster raycaster = eventCamera.GetComponent<PhysicsRaycaster>();

					if(raycaster == null)
					{
						eventCamera.gameObject.AddComponent<PhysicsRaycaster>();
					}
				}

				Material material = new Material(overlayShader != null ? overlayShader : Shader.Find("UI/Default"));
				material.renderQueue = (int) UnityEngine.Rendering.RenderQueue.Overlay;

				GameObject feedback = new GameObject("Selection feedback");
				feedback.transform.parent = eventCamera.transform;
				feedback.transform.localPosition = Vector3.zero;
				feedback.transform.localRotation = Quaternion.identity;
				feedback.transform.localScale = Vector3.one;

				if(audioFeedback != null)
				{
					audioSource = feedback.AddComponent<AudioSource>();
					audioSource.clip = audioFeedback;
				}

				pointer = CreateCanvas("Pointer", feedback);

				if(crosshair != null)
				{
					Image crosshair_image = CreateImage("Crosshair", pointer, crosshair, material);

					if(crosshair_image != null)
					{
						crosshair_image.transform.localRotation = Quaternion.identity;
					}
				}

				if(visualFeedback != null)
				{
					progressRange = CreateImage("Progress", pointer, visualFeedback, material);

					if(progressRange != null)
					{
						progressRange.GetComponent<RectTransform>().localEulerAngles = new Vector3(0f, 0f, 45f);
						progressRange.color = new Color(1f, 1f, 1f, 0.75f);
						progressRange.type = Image.Type.Filled;
						progressRange.fillMethod = Image.FillMethod.Radial360;
						progressRange.fillOrigin = (int) Image.Origin360.Bottom;
						progressRange.fillClockwise = true;
						progressRange.fillAmount = 0f;

						Image progress_background = CreateImage("Background", progressRange.gameObject, visualFeedback, material);

						if(progress_background != null)
						{
							progress_background.color = new Color(0f, 0f, 0f, 0.25f);
						}
					}
				}

				feedback.SetActive(false);

				selectionTime = float.MaxValue;
			}
		}

		protected override void Start()
		{
			base.Start();

			// Because eventSystem is not defined in Awake
			pointerEventData = new PointerEventData(eventSystem);
		}

		protected override void OnEnable()
		{
			base.OnEnable();

			if(pointer != null)
			{
				pointer.transform.parent.gameObject.SetActive(true);
			}
		}

		protected override void OnDisable()
		{
			base.OnDisable();

			if(pointer != null)
			{
				pointer.transform.parent.gameObject.SetActive(false);
			}
		}
		#endregion

		#region Input handling
		public void Click()
		{
			externalClick = true;
		}

		public override void Process()
		{
			if(eventCamera != null)
			{
				HandleLook();
				HandleSelection();
				ProcessMouseEvent();
			}
		}

		private void HandleLook()
		{
			// Fake a pointer always being at the center of the screen
			pointerEventData.position = new Vector2(0.5f * eventCamera.pixelWidth, 0.5f * (1 - verticalOffset) * eventCamera.pixelHeight);
			pointerEventData.delta = Vector2.zero;

			raycastResults.Clear();

			eventSystem.RaycastAll(pointerEventData, raycastResults);

			pointerEventData.pointerCurrentRaycast = FindFirstRaycast(raycastResults);

			ProcessMove(pointerEventData);

			if(raycastResults.Count > 0)
			{
				Camera event_camera = pointerEventData.pointerCurrentRaycast.module.eventCamera;

				Ray ray = event_camera.ScreenPointToRay(pointerEventData.position);

				pointer.transform.position = ray.origin + pointerEventData.pointerCurrentRaycast.distance * ray.direction;

				progressRange.gameObject.SetActive(true);
			}
			else
			{
				Ray ray = eventCamera.ScreenPointToRay(pointerEventData.position);

				pointer.transform.position = ray.origin + ray.direction.normalized;

				progressRange.gameObject.SetActive(false);
			}
		}

		private void HandleSelection()
		{
			if(progressRange != null)
			{
				progressRange.fillAmount = 0f;
			}

			if(pointerEventData.pointerEnter != null)
			{
				// If the UI receiver has changed, reset the gaze delay timer
				GameObject handler = ExecuteEvents.GetEventHandler<IPointerClickHandler>(pointerEventData.pointerEnter);

				if(handler != null)
				{
					if(selectedObject != handler)
					{
						selectedObject = handler;
						selectionTime = Time.realtimeSinceStartup + gazeTimeInSeconds;
					}
				}
				else
				{
					selectedObject = null;
					selectionTime = float.MaxValue;
				}

				if(useTimeBasedClick && progressRange != null)
				{
					progressRange.fillAmount = Mathf.Clamp01((Time.realtimeSinceStartup - (selectionTime - gazeTimeInSeconds)) / gazeTimeInSeconds);
				}

				bool click = externalClick || (useTimeBasedClick && Time.realtimeSinceStartup > selectionTime);

				// If we have a handler and it's time to click, do it now
				if(selectedObject != null && click)
				{
					ExecuteEvents.ExecuteHierarchy(selectedObject, pointerEventData, ExecuteEvents.pointerClickHandler);

					PlayAudio();

					selectionTime = float.MaxValue;
				}
			}
			else
			{
				selectedObject = null;
				selectionTime = float.MaxValue;
			}

			externalClick = false;
		}

		private void ProcessMouseEvent()
		{
			MouseState mouse_data = GetMousePointerEventData();

			bool pressed = mouse_data.AnyPressesThisFrame();
			bool released = mouse_data.AnyReleasesThisFrame();

			MouseButtonEventData left_button_data = mouse_data.GetButtonState(PointerEventData.InputButton.Left).eventData;

			if(!UseMouse(pressed, released, left_button_data.buttonData))
			{
				return;
			}

			// Process the first mouse button fully
			ProcessMousePress(left_button_data);
			ProcessMove(left_button_data.buttonData);
			ProcessDrag(left_button_data.buttonData);

			// Now process right / middle clicks
			ProcessMousePress(mouse_data.GetButtonState(PointerEventData.InputButton.Right).eventData);
			ProcessDrag(mouse_data.GetButtonState(PointerEventData.InputButton.Right).eventData.buttonData);
			ProcessMousePress(mouse_data.GetButtonState(PointerEventData.InputButton.Middle).eventData);
			ProcessDrag(mouse_data.GetButtonState(PointerEventData.InputButton.Middle).eventData.buttonData);

			if(!Mathf.Approximately(left_button_data.buttonData.scrollDelta.sqrMagnitude, 0.0f))
			{
				GameObject scroll_handler = ExecuteEvents.GetEventHandler<IScrollHandler>(left_button_data.buttonData.pointerCurrentRaycast.gameObject);

				ExecuteEvents.ExecuteHierarchy(scroll_handler, left_button_data.buttonData, ExecuteEvents.scrollHandler);
			}
		}

		private static bool UseMouse(bool pressed, bool released, PointerEventData pointerData)
		{
			return pressed || released || pointerData.IsPointerMoving() || pointerData.IsScrolling();
		}

		private void ProcessMousePress(MouseButtonEventData data)
		{
			PointerEventData pointer_event = data.buttonData;
			GameObject current_over_go = pointer_event.pointerCurrentRaycast.gameObject;

			// PointerDown notification
			if(data.PressedThisFrame())
			{
				pointer_event.eligibleForClick = true;
				pointer_event.delta = Vector2.zero;
				pointer_event.dragging = false;
				pointer_event.useDragThreshold = true;
				pointer_event.pressPosition = pointer_event.position;
				pointer_event.pointerPressRaycast = pointer_event.pointerCurrentRaycast;

				DeselectIfSelectionChanged(current_over_go, pointer_event);

				// search for the control that will receive the press
				// if we can't find a press handler set the press
				// handler to be what would receive a click.
				GameObject newPressed = ExecuteEvents.ExecuteHierarchy(current_over_go, pointer_event, ExecuteEvents.pointerDownHandler);

				// didnt find a press handler... search for a click handler
				if(newPressed == null)
				{
					newPressed = ExecuteEvents.GetEventHandler<IPointerClickHandler>(current_over_go);
				}

				// Debug.Log("Pressed: " + newPressed);

				float time = Time.unscaledTime;

				if(newPressed == pointer_event.lastPress)
				{
					float diffTime = time - pointer_event.clickTime;

					if(diffTime < 0.3f)
					{
						++pointer_event.clickCount;
					}
					else
					{
						pointer_event.clickCount = 1;
					}

					pointer_event.clickTime = time;
				}
				else
				{
					pointer_event.clickCount = 1;
				}

				pointer_event.pointerPress = newPressed;
				pointer_event.rawPointerPress = current_over_go;

				pointer_event.clickTime = time;

				// Save the drag handler as well
				pointer_event.pointerDrag = ExecuteEvents.GetEventHandler<IDragHandler>(current_over_go);

				if(pointer_event.pointerDrag != null)
				{
					ExecuteEvents.Execute(pointer_event.pointerDrag, pointer_event, ExecuteEvents.initializePotentialDrag);
				}
			}

			// PointerUp notification
			if(data.ReleasedThisFrame())
			{
				// Debug.Log("Executing pressup on: " + pointer.pointerPress);
				ExecuteEvents.Execute(pointer_event.pointerPress, pointer_event, ExecuteEvents.pointerUpHandler);

				// Debug.Log("KeyCode: " + pointer.eventData.keyCode);

				// see if we mouse up on the same element that we clicked on...
				GameObject pointerUpHandler = ExecuteEvents.GetEventHandler<IPointerClickHandler>(current_over_go);

				// PointerClick and Drop events
				if(pointer_event.pointerPress == pointerUpHandler && pointer_event.eligibleForClick)
				{
					ExecuteEvents.Execute(pointer_event.pointerPress, pointer_event, ExecuteEvents.pointerClickHandler);
				}
				else if(pointer_event.pointerDrag != null)
				{
					ExecuteEvents.ExecuteHierarchy(current_over_go, pointer_event, ExecuteEvents.dropHandler);
				}

				pointer_event.eligibleForClick = false;
				pointer_event.pointerPress = null;
				pointer_event.rawPointerPress = null;

				if(pointer_event.pointerDrag != null && pointer_event.dragging)
				{
					ExecuteEvents.Execute(pointer_event.pointerDrag, pointer_event, ExecuteEvents.endDragHandler);
				}

				pointer_event.dragging = false;
				pointer_event.pointerDrag = null;

				// redo pointer enter / exit to refresh state
				// so that if we moused over somethign that ignored it before
				// due to having pressed on something else
				// it now gets it.
				if(current_over_go != pointer_event.pointerEnter)
				{
					HandlePointerExitAndEnter(pointer_event, null);
					HandlePointerExitAndEnter(pointer_event, current_over_go);
				}
			}
		}
		#endregion

		#region Audio handling
		private void PlayAudio(float volume = 1f, float pitch = 1f)
		{
			if(audioSource != null)
			{
				audioSource.volume = volume;
				audioSource.pitch = pitch;

				audioSource.Play();
			}
		}
		#endregion

		#region Internal methods
		private GameObject CreateCanvas(string name, GameObject parent)
		{
			pointer = new GameObject(name, typeof(RectTransform), typeof(Canvas));

			RectTransform rect_transform = pointer.GetComponent<RectTransform>();
			rect_transform.SetParent(parent.transform, false);
			rect_transform.localPosition = Vector3.zero;
			rect_transform.localRotation = Quaternion.identity;
			rect_transform.localScale = 0.001f * Vector3.one;
			rect_transform.pivot = 0.5f * Vector2.one;
			rect_transform.sizeDelta = 50f * Vector2.one;

			Canvas canvas = pointer.GetComponent<Canvas>();
			canvas.renderMode = RenderMode.WorldSpace;
			canvas.worldCamera = eventCamera;

			return pointer;
		}

		private Image CreateImage(string name, GameObject parent, Sprite sprite, Material material)
		{
			Image image = null;

			if(parent != null && sprite != null)
			{
				GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image));

				RectTransform rect_transform = go.GetComponent<RectTransform>();
				rect_transform.SetParent(parent.transform, false);
				rect_transform.localPosition = Vector3.zero;
				rect_transform.localRotation = Quaternion.identity;
				rect_transform.localScale = Vector3.one;
				rect_transform.pivot = 0.5f * Vector2.one;
				rect_transform.anchorMin = Vector2.zero;
				rect_transform.anchorMax = Vector2.one;
				rect_transform.sizeDelta = Vector2.zero;

				image = go.GetComponent<Image>();
				image.sprite = sprite;
				image.material = material;
				image.raycastTarget = false;
				image.preserveAspect = true;
			}

			return image;
		}
		#endregion
	}
}
