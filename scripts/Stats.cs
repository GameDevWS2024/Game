public class PlayerStats
{
    public int Health { get; set; }
    public int Strength { get; set; }
    public int Speed { get; set; }
    public int Mana { get; set; }

    public PlayerStats(int health, int strength, int speed, int mana)
    {
        Health = health;
        Strength = strength;
        Speed = speed;
        Mana = mana;
    }

    public void TakeDamage(int damage)
    {
        Health -= damage;
        if (Health < 0)
        {
            Health = 0;
        }
    }

    public void Heal(int amount)
    {
        Health += amount;
        // Add logic to cap health if needed
    }
}
