using UnityEngine;
using UnityEditor;

namespace GameFramework
{
    public class CameraFollowEditor : EditorWindow
    {
        public float SmoothSpeed = 10;
        public Transform Target;
        [MenuItem("Tools/Camera Follow Editor")]
        public static void ShowWindow()
        {
            EditorWindow.GetWindow<CameraFollowEditor>("Camera Follow Editor");
        }

        private void OnGUI()
        {
            SmoothSpeed = EditorGUILayout.FloatField("SmoothSpeed", SmoothSpeed);

            Target = (Transform)EditorGUILayout.ObjectField("Target", Target, typeof(Transform), true);
        }

        private void Update()
        {
            if (Target == null) return;
            Vector3 desiredPosition = Target.position;
            SceneView.lastActiveSceneView.pivot = Vector3.Lerp(SceneView.lastActiveSceneView.pivot, desiredPosition, SmoothSpeed * Time.deltaTime);
        }

    }
}