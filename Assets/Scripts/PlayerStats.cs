using System;
using UnityEngine;
using System.Collections;

[Serializable]
public class PlayerStats
{
    // Stats for the character
    [SerializeField] private int _armor;
    [SerializeField] private int _health;
    [SerializeField] private int _energy;
    [SerializeField] private int _damage;
    [SerializeField] private int _movementSpeed;
    [SerializeField] private float _attackSpeed;

    // Getters and Setters for each of the stats (values can only be set inside this class
    public int Armor
    {
        get
        {
            return _armor;
        }
        private set
        {
            _armor = value;
        }
    }

    public int Health
    {
        get
        {
            return _health;
        }
        private set
        {
            _health = value;
        }
    }

    public int Energy
    {
        get
        {
            return _energy;
        }
        private set
        {
            _energy = value;
        }
    }

    public int Damage
    {
        get
        {
            return _damage;
        }
        private set
        {
            _damage = value;
        }
    }

    public int MovementSpeed
    {
        get
        {
            return _movementSpeed;
        }
        private set
        {
            _movementSpeed = value;
        }
    }

    public float AttackSpeed
    {
        get
        {
            return _attackSpeed;
        }
        private set
        {
            _attackSpeed = value;
        }
    }


    // Use this for initialization
    public void Init () {
        Armor = 10;
        Health = 500;
        Energy = 150;
        Damage = 43;
        AttackSpeed = 0.93f;
        MovementSpeed = 350;
	
	}

    // Methods for changing the stats
    public void ChangeArmor(int amount)
    {
        Armor += amount;
    }

    public void ChangeHealth(int amount)
    {
        Health += amount;
    }

    public void ChangeEnergy(int amount)
    {
        Energy += amount;
    }

    public void ChangeDamage(int amount)
    {
        Damage += amount;
    }

    public void ChangeAttackSpeed(int amount)
    {
        AttackSpeed += amount;
    }

    public void ChangeMovementSpeed(int amount)
    {
        MovementSpeed += amount;
    }
}
