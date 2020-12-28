using UnityEngine;
using PianoViaR.Utils;
using Leap.Unity.Interaction;

[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(InteractionBehaviour))]
public class StaffsScroll : MonoBehaviour
{
    BoxCollider boxCollider;
    public GameObject GameObject { get { return this.gameObject; } }
    // Start is called before the first frame update

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
    }

    public void AdaptToDimensions(Vector3 scoreBoxSize, Vector3 staffsDimensions)
    {
        ResetToNormal();

        float scaleY = scoreBoxSize.y / staffsDimensions.y;

        float colliderScaleX = staffsDimensions.x / scoreBoxSize.x;
        float colliderScaleY = staffsDimensions.y / scoreBoxSize.y;

        var newDimensions = staffsDimensions * scaleY;

        var currentSize = boxCollider.size;
        boxCollider.size = new Vector3(currentSize.x * colliderScaleX, currentSize.y * colliderScaleY, currentSize.z);
        // Adjust this object position to center left
        transform.localScale *= scaleY;

        // Center relative to its parent
        transform.localPosition = Vector3.zero;
        transform.Translate(new Vector3(((newDimensions.x - scoreBoxSize.x)) / 2, 0, 0), Space.Self);
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

    public void Clear()
    {
        gameObject.Clear();
    }
}
