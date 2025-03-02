using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class HealthSystemTest
{
    private GameObject player;
    private HealthSystem healthSystem;

    [SetUp]
    public void Setup()
    {
        // Create a new GameObject for testing
        player = new GameObject("Player");
        healthSystem = player.AddComponent<HealthSystem>();

        // Initialize with 100 health
        healthSystem.Initialize(100);
    }

    [TearDown]
    public void Teardown()
    {
        Object.Destroy(player);
    }

    [Test]
    public void HealthDecreasesWhenTakingDamage()
    {
        healthSystem.TakeDamage(30);
        Assert.AreEqual(70, healthSystem.CurrentHealth, "Health did not decrease correctly when taking damage.");
    }

    [Test]
    public void HealthDoesNotGoBelowZero()
    {
        healthSystem.TakeDamage(200);
        Assert.AreEqual(0, healthSystem.CurrentHealth, "Health should not go below zero.");
    }

    [Test]
    public void HealingRestoresHealth()
    {
        healthSystem.TakeDamage(50);
        healthSystem.Heal(30);
        Assert.AreEqual(80, healthSystem.CurrentHealth, "Healing did not restore health correctly.");
    }

    [Test]
    public void HealthDoesNotExceedMax()
    {
        healthSystem.Heal(500);
        Assert.AreEqual(100, healthSystem.CurrentHealth, "Health should not exceed maximum limit.");
    }
}
