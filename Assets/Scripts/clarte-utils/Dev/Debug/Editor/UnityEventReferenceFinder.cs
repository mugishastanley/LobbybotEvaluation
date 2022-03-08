// Based from https://github.com/recstazy/UnityEventReferenceViewer

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;

namespace CLARTE.Dev.Debug.Editor
{
    public class EventReferenceInfo
    {
        public MonoBehaviour Owner { get; set; }
        public List<Object> Listeners { get; set; } = new List<Object>();
        public List<string> MethodNames { get; set; } = new List<string>();
    }

    public class UnityEventReferenceFinder : MonoBehaviour
    {
        [ContextMenu("FindReferences")]
        public void FindReferences()
        {
            FindAllUnityEventsReferences();
        }

        public static List<EventReferenceInfo> FindAllUnityEventsReferences()
        {
            MonoBehaviour[] behaviours = Resources.FindObjectsOfTypeAll<MonoBehaviour>();
            Dictionary<MonoBehaviour, List<UnityEventBase>> events = new Dictionary<MonoBehaviour, List<UnityEventBase>>();

            foreach (MonoBehaviour b in behaviours)
            {
                TypeInfo info = b.GetType().GetTypeInfo();
                List<FieldInfo> evnts = info.DeclaredFields.Where(f => f.FieldType.IsSubclassOf(typeof(UnityEventBase))).ToList();

                foreach (FieldInfo e in evnts)
                {
                    if(!events.TryGetValue(b, out List<UnityEventBase> events_list))
                    {
                        events_list = new List<UnityEventBase>();

                        events.Add(b, events_list);
                    }

                    events_list.Add(e.GetValue(b) as UnityEventBase);
                }
            }

            List<EventReferenceInfo> infos = new List<EventReferenceInfo>();

            foreach (KeyValuePair<MonoBehaviour, List<UnityEventBase>> p in events)
            {
                foreach (UnityEventBase e in p.Value)
                {
                    int count = e.GetPersistentEventCount();
                    EventReferenceInfo info = new EventReferenceInfo();
                    info.Owner = p.Key;

                    for (int i = 0; i < count; i++)
                    {
                        Object obj = e.GetPersistentTarget(i);
                        string method = e.GetPersistentMethodName(i);

                        info.Listeners.Add(obj);
                        info.MethodNames.Add(obj.GetType().Name.ToString() + "." + method);
                    }

                    infos.Add(info);
                }
            }

            return infos;
        }
    }
}