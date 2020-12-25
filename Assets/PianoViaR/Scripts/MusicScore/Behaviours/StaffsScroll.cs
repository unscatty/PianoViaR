using UnityEngine;
using PianoViaR.Utils;

[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(Rigidbody))]
public class StaffsScroll : MonoBehaviour
{
    private BoxCollider boxCollider;
    public GameObject GameObject { get { return this.gameObject; } }
    // Start is called before the first frame update
    void Start()
    {
        boxCollider = GetComponent<BoxCollider>();
    }

    // Update is called once per frame
    void Update()
    {

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
        transform.position += Vector3.right * (newDimensions.x - scoreBoxSize.x) / 2;
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
