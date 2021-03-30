using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

#if UNITY_EDITOR
using UnityEditor;
#endif


namespace PlantSim.Buttons
{
    public class CanvasScript : BaseLabelBtn
    {
        [Serializable]
        public class CanvasEvent : UnityEvent { }

        [SerializeField]
        private CanvasEvent onSceneStart = new CanvasEvent();

        protected override void Start()
        {
            base.Start();
            LanguageNotification.GetInstance().Notification();

            StartCoroutine(StartEvent());
        }

        private IEnumerator StartEvent()
        {
            yield return new WaitForEndOfFrame();
            onSceneStart.Invoke();
        }

        public override void UpdateLabel()
        {
            base.UpdateLabel();

            Label.text += LScene.GetInstance().Day;
        }
    }

#if UNITY_EDITOR

    [CustomEditor(typeof(CanvasScript))]
    public class CanvasScriptEditor : Editor
    {
        SerializedProperty m_OnSceneStart;

        private void OnEnable()
        {
            m_OnSceneStart = serializedObject.FindProperty("onSceneStart");
        }

    }

#endif
}

