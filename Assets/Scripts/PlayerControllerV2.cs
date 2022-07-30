using System;
using System.Collections;
using System.Collections.Generic;
using Mono.Cecil.Cil;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerControllerV2 : MonoBehaviour
{
    [Header("Layer Masks")] 
    [SerializeField] private LayerMask _groundLayer;
    
    [Header("Components")] 
    private Rigidbody2D _rigidbody2D;

    [Header("Movement Variables")] 
    [SerializeField] private float _movementAcceleration = 50f;
    [SerializeField] private float _maxMoveSpeed = 10f;
    [SerializeField] private float _linearDrag = 7f;
    private float _horizontalDirection;
    
    [Header("Ground Checker")] 
    [SerializeField] private Transform _groundChecker;
    private bool _onGround;
    
    [Header("Jump Variables")] 
    [SerializeField] private float _jumpForce = 12f;   
    [SerializeField] private float _airLinearDrag = 2.5f;
    [SerializeField] private float _fallMultiplier = 14f;
    [SerializeField] private float _lowJumpFallMultiplier = 5f;

    private bool _changingDirection => (_rigidbody2D.velocity.x > 0f && _horizontalDirection < 0f) ||
                                       (_rigidbody2D.velocity.x < 0f && _horizontalDirection > 0f);
    
    private bool _canJump => Input.GetKeyDown(KeyCode.Space) && IsGrounded();

    private void Start()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        _horizontalDirection = GetInput().x;
        if (_canJump) Jump();
    }

    private void FixedUpdate()
    {
        MoveCharacter();
        
        if (IsGrounded())
        {
            ApplyGroundLinearDrag();
        }
        else
        {
            ApplyAirLinearDrag();
            FallMultiplier();
        }
    }
    
    private void Jump()
    {
        _rigidbody2D.velocity = new Vector2(_rigidbody2D.velocity.x, 0f);
        _rigidbody2D.AddForce(Vector2.up * _jumpForce, ForceMode2D.Impulse);
    }

    private void FallMultiplier()
    {
        if (_rigidbody2D.velocity.y < 0)
        {
            _rigidbody2D.gravityScale = _fallMultiplier;
        }
        else if (_rigidbody2D.velocity.y > 0 && !Input.GetKey(KeyCode.Space))
        {
            _rigidbody2D.gravityScale = _lowJumpFallMultiplier;
        }
        else
        {
            _rigidbody2D.gravityScale = 1f;
        }
    }

    private void ApplyAirLinearDrag()
    {
        _rigidbody2D.drag = _airLinearDrag;
    }

    private Vector2 GetInput()
    {
        return new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
    }

    private void MoveCharacter()
    {
        _rigidbody2D.AddForce(new Vector2(_horizontalDirection, 0f) * _movementAcceleration);

        if (Mathf.Abs(_rigidbody2D.velocity.x) > _maxMoveSpeed)
        {
            _rigidbody2D.velocity =
                new Vector2(Mathf.Sign(_rigidbody2D.velocity.x) * _maxMoveSpeed, _rigidbody2D.velocity.y);
        }
    }
    
    private void ApplyGroundLinearDrag()
    {
        if (Mathf.Abs(_horizontalDirection) < 0.4f || _changingDirection)
        {
            _rigidbody2D.drag = _linearDrag;
        }
        else
        {
            _rigidbody2D.drag = 0f; 
        }
    }
    
    private bool IsGrounded()
    {
        return Physics2D.OverlapCircle(_groundChecker.position, 0.5f, _groundLayer);
    }
}