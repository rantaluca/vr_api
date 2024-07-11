using UnityEngine;

public class AttachOnCollision : MonoBehaviour
{
    private bool hasJoint = false;

    void OnCollisionEnter(Collision collision)
    {
        // Vérifiez si l'autre objet a un Rigidbody et qu'il n'y a pas encore de joint
        if (collision.gameObject.GetComponent<Rigidbody>() != null && !hasJoint && collision.gameObject.tag == "Collisable")
        {
            // Ajoutez un FixedJoint à cet objet
            gameObject.AddComponent<HingeJoint>();
		

            // Connectez le joint au Rigidbody de l'objet avec lequel il entre en collision
			gameObject.GetComponent<HingeJoint>().connectedBody = collision.gameObject.GetComponent<Rigidbody>() ;
            //joint.connectedBody = collision.rigidbody;

            // Marquez hasJoint pour éviter d'ajouter plusieurs joints
            hasJoint = true;

            // Optionnel : Configurer les propriétés du joint, par exemple :
            //joint.breakForce = 1000f;
            //joint.breakTorque = 1000f;
        }
    }
}
