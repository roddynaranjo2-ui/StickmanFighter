// HealthSystemTests.cs — Tests EditMode para HealthSystem (G-02 SPRINT #3).

using NUnit.Framework;
using UnityEngine;
using StickmanFighter.Combat;

public class HealthSystemTests
{
    private GameObject? _go;
    private HealthSystem? _hs;

    [SetUp]
    public void SetUp()
    {
        _go = new GameObject("TestHealth");
        _hs = _go.AddComponent<HealthSystem>();
        // Invoke Awake manually (EditMode no llama OnEnable/Awake automáticamente igual)
        var awake = typeof(HealthSystem).GetMethod("Awake",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        awake?.Invoke(_hs, null);
    }

    [TearDown]
    public void TearDown()
    {
        if (_go != null) Object.DestroyImmediate(_go);
    }

    [Test]
    public void StartsWithMaxHealth()
    {
        Assert.AreEqual(_hs!.MaxHealth, _hs.CurrentHealth);
        Assert.IsTrue(_hs.IsAlive);
    }

    [Test]
    public void ApplyDamage_ReducesHp()
    {
        int prev = _hs!.CurrentHealth;
        bool applied = _hs.ApplyDamage(20, Vector2.zero);
        Assert.IsTrue(applied);
        Assert.AreEqual(prev - 20, _hs.CurrentHealth);
    }

    [Test]
    public void ApplyDamage_WhileInvulnerable_Blocked()
    {
        _hs!.ApplyDamage(10, Vector2.zero);
        int afterFirst = _hs.CurrentHealth;
        bool secondApplied = _hs.ApplyDamage(10, Vector2.zero);
        Assert.IsFalse(secondApplied);
        Assert.AreEqual(afterFirst, _hs.CurrentHealth);
    }

    [Test]
    public void Death_FiresOnDied_Once()
    {
        int deaths = 0;
        _hs!.OnDied += () => deaths++;
        _hs.ApplyDamage(9999, Vector2.zero);
        Assert.AreEqual(0, _hs.CurrentHealth);
        Assert.IsFalse(_hs.IsAlive);
        Assert.AreEqual(1, deaths);
    }

    [Test]
    public void Heal_ClampsToMax()
    {
        _hs!.ApplyDamage(30, Vector2.zero);
        _hs.Heal(9999);
        Assert.AreEqual(_hs.MaxHealth, _hs.CurrentHealth);
    }

    [Test]
    public void ResetHealth_Restores()
    {
        _hs!.ApplyDamage(50, Vector2.zero);
        _hs.ResetHealth();
        Assert.AreEqual(_hs.MaxHealth, _hs.CurrentHealth);
        Assert.IsFalse(_hs.IsInvulnerable);
    }
}
