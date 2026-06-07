// CombatEventsTests.cs — Tests EditMode para el bus pub/sub de combate (G-02 SPRINT #3).

using NUnit.Framework;
using UnityEngine;
using StickmanFighter.Combat;

public class CombatEventsTests
{
    [SetUp]
    public void SetUp() => CombatEvents.ResetForTests();

    [TearDown]
    public void TearDown() => CombatEvents.ResetForTests();

    [Test]
    public void RaiseHit_NotifiesSubscriber_WithCorrectArgs()
    {
        int callCount = 0;
        Vector2 capturedAttacker = Vector2.zero;
        Vector2 capturedVictim = Vector2.zero;
        int capturedDamage = 0;
        AttackType capturedType = AttackType.Punch;

        CombatEvents.OnHit += (a, v, d, t) =>
        {
            callCount++;
            capturedAttacker = a; capturedVictim = v; capturedDamage = d; capturedType = t;
        };

        CombatEvents.RaiseHit(new Vector2(1f, 2f), new Vector2(3f, 4f), 15, AttackType.Kick);

        Assert.AreEqual(1, callCount);
        Assert.AreEqual(new Vector2(1f, 2f), capturedAttacker);
        Assert.AreEqual(new Vector2(3f, 4f), capturedVictim);
        Assert.AreEqual(15, capturedDamage);
        Assert.AreEqual(AttackType.Kick, capturedType);
    }

    [Test]
    public void ResetForTests_RemovesSubscribers()
    {
        int count = 0;
        CombatEvents.OnHit += (_, _, _, _) => count++;
        CombatEvents.ResetForTests();
        CombatEvents.RaiseHit(Vector2.zero, Vector2.zero, 10, AttackType.Punch);
        Assert.AreEqual(0, count);
    }
}
