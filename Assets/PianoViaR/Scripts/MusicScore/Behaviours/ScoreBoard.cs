using UnityEngine;
using PianoViaR.Utils;
using Leap.Unity.Interaction;

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
        public LayerMask disableMask;
        public GameObject GameObject { get { return this.gameObject; } }
        LayerMask defaultMask;
        // Start is called before the first frame update
        void Start()
        {
            interaction = GetComponent<InteractionBehaviour>();
            boxCollider = GetComponent<BoxCollider>();
            defaultMask = gameObject.layer;
        }

        // Update is called once per frame
        void Update()
        {

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

        public void DisableContact()
        {
            gameObject.layer = disableMask;
        }

        public void EnableContact()
        {
            gameObject.layer = defaultMask;
        }
    }
}