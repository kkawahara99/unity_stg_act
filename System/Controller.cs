using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Controller : MonoBehaviour
{
    public Vector2 _direction;
    [SerializeField] private InputActionPhase movePhase;
    public InputActionPhase MovePhase { get => movePhase; }
    [SerializeField] private InputActionPhase shootPhase;
    public InputActionPhase ShootPhase { get => shootPhase; }
    [SerializeField] private InputActionPhase slashPhase;
    public InputActionPhase SlashPhase { get => slashPhase; }
    [SerializeField] private InputActionPhase shieldPhase;
    public InputActionPhase ShieldPhase { get => shieldPhase; }
    [SerializeField] private InputActionPhase startPhase;
    public InputActionPhase StartPhase { get => startPhase; }

    public void SetMovePhase(InputActionPhase movePhase)
    {
        this.movePhase = movePhase;
    }

    public void SetShootPhase(InputActionPhase shootPhase)
    {
        this.shootPhase = shootPhase;
    }

    public void SetSlashPhase(InputActionPhase slashPhase)
    {
        this.slashPhase = slashPhase;
    }

    public void SetShieldPhase(InputActionPhase shieldPhase)
    {
        this.shieldPhase = shieldPhase;
    }

    public void SetStartPhase(InputActionPhase startPhase)
    {
        this.startPhase = startPhase;
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            // 1フレーム目はStartedにする
            movePhase = InputActionPhase.Started;
        }
        else
        {
            movePhase = context.phase;
        }
        _direction = context.ReadValue<Vector2>();
    }

    public void OnShoot(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            // 1フレーム目はStartedにする
            shootPhase = InputActionPhase.Started;
        }
        else
        {
            shootPhase = context.phase;
        }
    }

    public void OnSlash(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            // 1フレーム目はStartedにする
            slashPhase = InputActionPhase.Started;
        }
        else
        {
            slashPhase = context.phase;
        }
    }

    public void OnShield(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            // 1フレーム目はStartedにする
            shieldPhase = InputActionPhase.Started;
        }
        else
        {
            shieldPhase = context.phase;
        }
    }

    public void OnStart(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            // 1フレーム目はStartedにする
            startPhase = InputActionPhase.Started;
        }
        else
        {
            startPhase = context.phase;
        }
    }
}
