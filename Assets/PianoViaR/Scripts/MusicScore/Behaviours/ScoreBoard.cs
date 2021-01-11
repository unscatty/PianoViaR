using UnityEngine;
using PianoViaR.Utils;
using Leap.Unity.Interaction;
using PianoViaR.Helpers;

namespace PianoViaR.Score.Behaviours
{
    [RequireComponent(typeof(InteractionBehaviour))]
    [RequireComponent(typeof(BoxCollider))]
    public class ScoreBoard : MonoBehaviour
    {
        public StaffsScroll staffs;
        InteractionBehaviour interaction;
        BoxCollider boxCollider;

        // Must be set in editor
        [SerializeField, Layer]
        public int disableLayer;
        int defaultLayer;
        // Start is called before the first frame update
        void Awake()
        {
            interaction = GetComponent<InteractionBehaviour>();
            boxCollider = GetComponent<BoxCollider>();
            defaultLayer = gameObject.layer;
        }

        public Vector3 BoxSize(bool force = true)
        {
            return this.gameObject.BoxSize(force);
        }

        private void OnTriggerEnter(Collider other)
        {
            staffs.CanCollide(true);
            Debug.Log("Can collide");
        }

        private void OnTriggerExit(Collider other)
        {
            staffs.CanCollide(false);
            Debug.Log("Can not collide");
        }

        public void ContactEnabled(bool enabled)
        {
            if (enabled)
            {
                EnableContact();
            }
            else
            {
                DisableContact();
            }
        }

        public void DisableContact()
        {
            gameObject.layer = disableLayer;
            boxCollider.enabled = false;
        }

        public void EnableContact()
        {
            gameObject.layer = defaultLayer;
            boxCollider.enabled = true;
        }
    }
}