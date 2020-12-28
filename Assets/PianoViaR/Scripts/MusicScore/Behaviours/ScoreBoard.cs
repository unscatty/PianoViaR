using UnityEngine;
using Leap.Unity.Interaction;
using PianoViaR.Utils;

[RequireComponent(typeof(InteractionBehaviour))]
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
    }

    private void OnTriggerExit(Collider other)
    {
        staffs.CanCollide(false);
    }
}
