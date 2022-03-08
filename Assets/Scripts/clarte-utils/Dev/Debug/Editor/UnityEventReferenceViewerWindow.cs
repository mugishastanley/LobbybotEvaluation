// Based from https://github.com/recstazy/UnityEventReferenceViewer

using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using System.Linq;

namespace CLARTE.Dev.Debug.Editor
{
    public class UnityEventReferenceViewerWindow : EditorWindow
    {
        #region Fields
        private int mainSpacing = 20;
        private int tabulation = 30;
        private float leftColoumnRelativeWidth = 0.6f;

        private static List<EventReferenceInfo> dependencies;
        private string searchString = "";

        private Rect drawableRect;

        Vector2 scroll = Vector2.zero;
        #endregion

        [MenuItem("Window/UnityEvent Reference Viewer")]
        public static void OpenWindow()
        {
            UnityEventReferenceViewerWindow window = GetWindow<UnityEventReferenceViewerWindow>();
            window.titleContent = new GUIContent("UnityEvent Reference Viewer");
            window.minSize = new Vector2(250, 100);
        }

        [DidReloadScripts]
        private static void RefreshDependencies()
        {
            FindDependencies();
        }

        private void OnFocus()
        {
            RefreshDependencies();
        }

        private void OnEnable()
        {
            FindDependencies();
        }

        private void OnDisable()
        {
            dependencies = null;
        }

        private void OnGUI()
        {
            DrawWindow();
        }

        private void DrawWindow()
        {
            const float field_height = 16f;
            const float label_width = 100f;
            const float button_width = 40f;

            int drawnVertically = 0;
            drawableRect = GetDrawableRect();

            EditorGUI.LabelField(new Rect(drawableRect.position, new Vector2(label_width, field_height)), "Search method");
            searchString = EditorGUI.TextField(new Rect(drawableRect.position + Vector2.right * label_width, new Vector2(drawableRect.width - (label_width + button_width), field_height)), searchString);
            bool search = GUI.Button(new Rect(drawableRect.position + Vector2.right * (drawableRect.width - button_width), new Vector2(button_width, field_height)), "OK");

            if (search)
            {
                if (searchString != "")
                {
                    FindDependencies(searchString);
                }
                else
                {
                    FindDependencies();
                }
            }

            if (dependencies != null)
            {
                GUILayout.Space(80);
                scroll = GUILayout.BeginScrollView(scroll, false, false);

                int i = 0;

                foreach (EventReferenceInfo d in dependencies)
                {
                    int verticalSpace = (d.Listeners.Count + 1) * 16 + mainSpacing;
                    Vector2 position = new Vector2(drawableRect.position.x, 0f) + Vector2.up * drawnVertically;

                    DrawDependencies(d, position);

                    drawnVertically += verticalSpace;
                    i++;
                }

                GUILayout.Space(drawnVertically + 20);

                GUILayout.EndScrollView();
            }
        }

        private void DrawDependencies(EventReferenceInfo dependency, Vector2 position)
        {
            float width = drawableRect.width * leftColoumnRelativeWidth;

            EditorGUI.ObjectField(new Rect(position, new Vector2(width - tabulation, 16f)), dependency.Owner, typeof(MonoBehaviour), true);

            for (int i = 0; i < dependency.Listeners.Count; i++)
            {
                Vector2 positionRoot = position + Vector2.up * 16 + Vector2.up * 16 * i;
                EditorGUI.ObjectField(new Rect(positionRoot + Vector2.right * tabulation, new Vector2(width - tabulation, 16f)), dependency.Listeners[i], typeof(MonoBehaviour), true);

                Vector2 labelPosition = new Vector2(drawableRect.width * leftColoumnRelativeWidth + tabulation * 1.5f, positionRoot.y);
                EditorGUI.LabelField(new Rect(labelPosition, new Vector2(drawableRect.width * (1 - leftColoumnRelativeWidth) - tabulation / 1.5f, 16f)), dependency.MethodNames[i]);
            }
        }

        private static void FindDependencies(string methodName)
        {
            List<EventReferenceInfo> depens = UnityEventReferenceFinder.FindAllUnityEventsReferences();
            List<EventReferenceInfo> onlyWithName = new List<EventReferenceInfo>();

            foreach (EventReferenceInfo d in depens)
            {
                int[] indexes = d.MethodNames.Where(m => m.ToLower().Contains(methodName.ToLower())).Select(n => d.MethodNames.IndexOf(n)).ToArray();

                if (indexes.Length > 0)
                {
                    EventReferenceInfo info = new EventReferenceInfo();
                    info.Owner = d.Owner;

                    foreach (int i in indexes)
                    {
                        info.Listeners.Add(d.Listeners[i]);
                        info.MethodNames.Add(d.MethodNames[i]);
                    }

                    onlyWithName.Add(info);
                }
            }

            dependencies = onlyWithName.Count > 0 ? onlyWithName : depens;
        }

        private static void FindDependencies()
        {
            dependencies = UnityEventReferenceFinder.FindAllUnityEventsReferences();
        }

        private Rect GetDrawableRect()
        {
            return new Rect(Vector2.one * 30f, position.size - Vector2.one * 60f);
        }
    }
}
