using UnityEngine;

public class AttachOnCollision : MonoBehaviour
{
    private bool hasJoint = false;
    private bool has_touched_ground = false;

    void OnCollisionEnter(Collision collision)
    {
        Debug.Log(has_touched_ground);
        // Vérifiez si l'autre objet a un Rigidbody et qu'il n'y a pas encore de joint
        //get the neighbors of the object from the list 'words' in the parent object
        string[] words = gameObject.GetComponentInParent<LocalGPTRequest>().words;
        //get the index of the object in the list
        int index = System.Array.IndexOf(words, gameObject.tag);
        //get the neighbor of the object
        string neighbor = words[index + 1];

        if (collision.gameObject.GetComponent<Rigidbody>() != null && !hasJoint && collision.gameObject.tag == neighbor && has_touched_ground == true)
        {
            // Add a HingeJoint to this object
            HingeJoint hingeJoint = gameObject.AddComponent<HingeJoint>();

            // Connect the joint to the Rigidbody of the object it collides with
            hingeJoint.connectedBody = collision.gameObject.GetComponent<Rigidbody>();

            hingeJoint.anchor = new Vector3(0.5f, 0, 0); // Adjust this based on your object size

            // Set the axis for the hinge joint to rotate around the Y axis (vertical)
            hingeJoint.axis = Vector3.up;// Assuming the objects should rotate around the Y-axis

            // Optional: Configure joint properties, e.g.:
            // hingeJoint.breakForce = 1000f;
            // hingeJoint.breakTorque = 1000f;

            // Mark hasJoint to avoid adding multiple joints
            hasJoint = true;
        }

        if (collision.gameObject.name == "rabbids_coliseum_fbx")
        {
            has_touched_ground = true;
        }
        Debug.Log(collision.gameObject.name);    
    }
}
