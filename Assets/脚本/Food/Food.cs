using UnityEngine;

public class Food : MonoBehaviour
{
    public float energy = 0.2f;
    public float Eat()
    {
        WorldManager.FoodDisappear(gameObject);
        return energy;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.TryGetComponent(out AnimalMovement a))
            if (a.AnimalType.foodHabit != FoodHabit.Carnivorous || a.Hungry)
                a.satiety += (a.AnimalType.foodHabit == FoodHabit.Carnivorous ? 0.5f : 1) * Eat();
    }

    private void OnDestroy()
    {
        WorldManager.FoodDestoryed();
    }
}
