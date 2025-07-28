using UnityEngine;

public class FallReset : MonoBehaviour
{
    public Transform player;          // Le XR Rig
    public float minY = 100;           // Seuil minimum autorisé
    public Transform resetPoint;      // Point de retour si le joueur tombe

    void Update()
    {
        //Debug.Log("Position du joueur : " + player.position.y);
        if (player.position.y < 100)
        {
            //Debug.Log("salut");
            ResetPlayerPosition();

        }
    }

    void ResetPlayerPosition()
    {
        CharacterController cc = player.GetComponent<CharacterController>();
        if (cc != null)
        {
            cc.enabled = false; //Désactiver avant de bouger le CharacterController
        }

        player.position = resetPoint.position;

        if (cc != null)
        {
            cc.enabled = true;
        }
    }
}
