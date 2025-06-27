using UnityEngine;

public class Food : MonoBehaviour
{
    public float energy=0.1f;
    public float Eat()
    {
        WorldManager.FoodDisappear(gameObject);
        return energy;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Animal"))
            collision.gameObject.GetComponent<AnimalMovement>().satiety += Eat();
    }

    private void OnDestroy()
    {
        WorldManager.FoodDestoryed();
    }
}
