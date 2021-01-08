using UnityEngine;
using PianoViaR.Utils;
using Leap.Unity.Interaction;
using System.Collections;

namespace PianoViaR.Score.Behaviours
{
    [RequireComponent(typeof(BoxCollider))]
    [RequireComponent(typeof(InteractionBehaviour))]
    public class StaffsScroll : MonoBehaviour
    {
        BoxCollider boxCollider;
        public GameObject GameObject { get { return this.gameObject; } }
        // Start is called before the first frame update
        private Vector3 leftPosition;
        public float positionResetTime; // 2 seconds

        private void Awake()
        {
            Initialize();
        }

        void Start()
        {
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void Initialize()
        {
            if (boxCollider == null)
                boxCollider = GetComponent<BoxCollider>();

            if (leftPosition == null)
                leftPosition = Vector3.zero;

            if (positionResetTime <= 0)
                positionResetTime = 1;
        }

        public Vector3 AdaptToDimensionsScroll(Vector3 scoreBoxSize, Vector3 staffsDimensions)
        {
            var newDimensions = AdaptToDimensionsY(scoreBoxSize, staffsDimensions);

            transform.Translate(new Vector3(((newDimensions.x - scoreBoxSize.x)) / 2, 0, 0), Space.Self);

            leftPosition = transform.localPosition;

            return newDimensions;
        }

        public Vector3 AdaptToDimensions(Vector3 scoreBoxSize, Vector3 staffsDimensions, Axis axis)
        {
            ResetToNormal();

            var newDimensions = gameObject.AdaptToDimensions(boxCollider, scoreBoxSize, staffsDimensions, axis);

            // Center relative to its parent
            transform.localPosition = Vector3.zero;

            return newDimensions;
        }

        public Vector3 AdaptToDimensionsY(Vector3 scoreBoxSize, Vector3 staffsDimensions)
        {
            return AdaptToDimensions(scoreBoxSize, staffsDimensions, Axis.Y);
        }

        public Vector3 AdaptToDimensionsX(Vector3 scoreBoxSize, Vector3 staffsDimensions)
        {
            return AdaptToDimensions(scoreBoxSize, staffsDimensions, Axis.X);
        }

        public void CanCollide(bool canCollide)
        {
            boxCollider.enabled = canCollide;
        }

        public void ResetToNormal()
        {
            transform.position = Vector3.zero;
            transform.localScale = Vector3.one;
            // Re-center box collider
            boxCollider.center = Vector3.zero;
            boxCollider.size = Vector3.one;
        }

        public void RestoreToLeft()
        {
            // transform.localPosition = leftPosition;
            StartCoroutine(RestorePosition(leftPosition, positionResetTime));
        }

        public void Clear()
        {
            gameObject.Clear();
        }

        IEnumerator RestorePosition(Vector3 targetPosition, float duration)
        {
            float time = 0;
            Vector3 startPositionX = new Vector3(transform.localPosition.x, 0, 0);
            Vector3 targetPositionX = new Vector3(targetPosition.x, 0, 0);

            while (time < duration)
            {
                transform.localPosition = Vector3.Lerp(startPositionX, targetPositionX, time / duration);
                time += Time.deltaTime;
                yield return null;
            }
            transform.localPosition = targetPosition;
        }
    }
}