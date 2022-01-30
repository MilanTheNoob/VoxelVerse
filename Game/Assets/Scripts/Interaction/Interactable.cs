using System.Collections;
using UnityEngine;

public class Interactable : MonoBehaviour
{
    public Block Item;
    public int count;

    ItemReference itemReference;
    [HideInInspector] public int iters = 0;

    BoxCollider col;
    Vector3 roundedPos;

    private void Start()
    {
        count = 1;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.name == "Player")
        {
            print("collided interactable");

            Inventory.AddItem(Item);
            LeanTween.scale(gameObject, Vector3.zero, 0.5f).setOnComplete(() => { Destroy(gameObject); });
        }
    }
}
