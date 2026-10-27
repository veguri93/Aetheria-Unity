using UnityEngine;

namespace Aetheria.Weapons
{
    // Runs after an Animator/typical IK pass; adjust script order if your IK runs later.
    [DefaultExecutionOrder(10000)]
    public sealed class AetheriaBowStringDriver : MonoBehaviour
    {
        [SerializeField] Transform drawControl;
        [SerializeField] Vector3 restLocalPosition;
        [SerializeField] bool restCaptured;
        [SerializeField] Transform drawingHandTarget;
        [SerializeField] Vector3 handLocalOffset = Vector3.zero;
        [SerializeField, Range(0, 1)] float drawAmount;
        [SerializeField, Min(.01f)] float maximumDraw = .32f;
        [SerializeField, Min(.01f)] float releaseDuration = .16f;
        [SerializeField] bool followHand;
        bool releasing;
        float releaseElapsed;
        Vector3 releaseStart;

        public Transform DrawControl => drawControl;
        public float DrawAmount => drawAmount;
        public void Configure(Transform control)
        {
            drawControl = control;
            restCaptured = control != null;
            if (restCaptured) restLocalPosition = control.localPosition;
        }
        void Awake()
        {
            if (drawControl && !restCaptured) Configure(drawControl);
        }
        public void SetDraw(float amount)
        {
            drawAmount = Mathf.Clamp01(amount);
            followHand = false;
            releasing = false;
        }
        public void FollowDrawingHand(Transform target)
        {
            drawingHandTarget = target;
            followHand = target != null;
            releasing = false;
        }
        // Called by gameplay code or an animation-event relay on the character.
        // This moves only the cable. Spawn/release your arrow in your own combat code.
        public void Release()
        {
            if (!drawControl || !restCaptured) return;
            followHand = false;
            releasing = true;
            releaseElapsed = 0;
            releaseStart = drawControl.localPosition;
            drawAmount = 0;
        }
        public void ResetString()
        {
            releasing = false;
            followHand = false;
            drawAmount = 0;
            if (drawControl && restCaptured) drawControl.localPosition = restLocalPosition;
        }
        void OnDisable() { ResetString(); }
        void LateUpdate()
        {
            if (!drawControl || !restCaptured || !drawControl.parent) return;
            if (releasing)
            {
                releaseElapsed += Time.deltaTime;
                float t = Mathf.Clamp01(releaseElapsed / releaseDuration);
                // Rapid, smooth return; exact rest is restored on completion.
                drawControl.localPosition = Vector3.LerpUnclamped(releaseStart, restLocalPosition, 1 - Mathf.Pow(1 - t, 3));
                if (t >= 1) { releasing = false; drawControl.localPosition = restLocalPosition; }
                return;
            }
            Vector3 restWorld = drawControl.parent.TransformPoint(restLocalPosition);
            Vector3 desiredOffset = Vector3.left * (drawAmount * maximumDraw);
            if (followHand && drawingHandTarget)
            {
                Vector3 targetWorld = drawingHandTarget.TransformPoint(handLocalOffset);
                desiredOffset = transform.InverseTransformVector(targetWorld - restWorld);
                desiredOffset.x = Mathf.Clamp(desiredOffset.x, -maximumDraw, 0);
                desiredOffset.y = Mathf.Clamp(desiredOffset.y, -.12f, .12f);
                desiredOffset.z = Mathf.Clamp(desiredOffset.z, -.06f, .06f);
                drawAmount = -desiredOffset.x / maximumDraw;
            }
            drawControl.position = restWorld + transform.TransformVector(desiredOffset);
        }
    }
}
