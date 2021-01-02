using UnityEngine;
using PianoViaR.Utils;

namespace PianoViaR.Score.Behaviours
{
    [RequireComponent(typeof(BoxCollider))]
    public class ScoreBoard : MonoBehaviour
    {
        public StaffsScroll staffs;
        // Start is called before the first frame update
        void Start()
        {

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
    }
}