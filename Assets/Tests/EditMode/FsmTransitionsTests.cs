// FsmTransitionsTests.cs — Tests EditMode para garantizar
// que el StateMachine y las transiciones reales del jugador no tienen regresiones.

using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using StickmanFighter.Character;

public class FsmTransitionsTests
{
    private GameObject? _playerGo;
    private PlayerController? _player;

    private class TestState : IState
    {
        public int EnterCount;
        public int ExitCount;
        public int UpdateCount;
        public void Enter() => EnterCount++;
        public void Exit()  => ExitCount++;
        public void Update() => UpdateCount++;
        public void FixedUpdate() { }
    }

    [SetUp]
    public void SetUp()
    {
        _playerGo = new GameObject("TestPlayer", typeof(Rigidbody2D), typeof(BoxCollider2D));
        _player = _playerGo.AddComponent<PlayerController>();
        _playerGo.tag = "Player";

        // Neutraliza cualquier residuo de física en EditMode.
        var rb = _playerGo.GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.simulated = true;
        rb.gravityScale = 0f;

        SetGrounded(_player, false);
    }

    [TearDown]
    public void TearDown()
    {
        if (_playerGo != null)
        {
            Object.DestroyImmediate(_playerGo);
            _playerGo = null;
            _player = null;
        }
    }

    private static void SetGrounded(PlayerController player, bool grounded)
    {
        var field = typeof(PlayerController).GetField("<IsGrounded>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.IsNotNull(field, "Could not locate IsGrounded backing field");
        field!.SetValue(player, grounded);
    }

    [Test]
    public void Initialize_CallsEnterOnce()
    {
        var sm = new StateMachine();
        var s = new TestState();
        sm.Initialize(s);
        Assert.AreEqual(1, s.EnterCount);
        Assert.AreSame(s, sm.CurrentState);
    }

    [Test]
    public void ChangeState_CallsExitThenEnter()
    {
        var sm = new StateMachine();
        var a = new TestState();
        var b = new TestState();
        sm.Initialize(a);

        sm.ChangeState(b);

        Assert.AreEqual(1, a.ExitCount,  "a.Exit() should have been called");
        Assert.AreEqual(1, b.EnterCount, "b.Enter() should have been called");
        Assert.AreSame(b, sm.CurrentState);
    }

    [Test]
    public void ChangeState_SameState_IsNoOp()
    {
        var sm = new StateMachine();
        var s = new TestState();
        sm.Initialize(s);
        sm.ChangeState(s);
        Assert.AreEqual(1, s.EnterCount, "Enter should not be called again for same state");
        Assert.AreEqual(0, s.ExitCount,  "Exit should not be called for same state");
    }

    [Test]
    public void Update_DispatchedToCurrentState()
    {
        var sm = new StateMachine();
        var s = new TestState();
        sm.Initialize(s);
        sm.Update();
        sm.Update();
        Assert.AreEqual(2, s.UpdateCount);
    }

    [Test]
    public void IdleState_ForwardInput_TransitionsToWalkForward()
    {
        var input = new PlayerInputData { MoveForward = true };
        _player!.InputData = input;

        _player.IdleState.Update();

        Assert.AreSame(_player.WalkForwardState, _player.StateMachine.CurrentState);
    }

    [Test]
    public void IdleState_BackwardInput_TransitionsToWalkBackward()
    {
        var input = new PlayerInputData { MoveBackward = true };
        _player!.InputData = input;

        _player.IdleState.Update();

        Assert.AreSame(_player.WalkBackwardState, _player.StateMachine.CurrentState);
    }

    [Test]
    public void IdleState_CrouchWhenGrounded_TransitionsToCrouch()
    {
        SetGrounded(_player!, true);
        _player.InputData = new PlayerInputData { Crouch = true };

        _player.IdleState.Update();

        Assert.AreSame(_player.CrouchState, _player.StateMachine.CurrentState);
    }

    [Test]
    public void IdleState_JumpWhenGrounded_TransitionsToJump()
    {
        SetGrounded(_player!, true);
        _player.InputData = new PlayerInputData { JumpPressed = true };

        _player.IdleState.Update();

        Assert.AreSame(_player.JumpState, _player.StateMachine.CurrentState);
    }
}
