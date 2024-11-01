using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("How fast a player moves")]
    public float moveSpeed = 5f;
    public static Room activeRoom; // The room the player is in
    public LayerMask enemyLayerMask;
    // is the player moving?
    [HideInInspector]
    public bool isMoving;
    [HideInInspector]
    public Vector3 targetPos;
    [HideInInspector]
    public Vector3Int currentGridPos;
    public PlayerActions actions;
    public PlayerOrientationManger orient;
    private TimeManager _timeManInstance;
    private PlayerStatsManager _man;
    private bool _isAttacked;

    public delegate void PlayerMove(int steps);

    private void Start()
    {
        // Get the time manger 
        _timeManInstance = TimeManager.instance;
        _man = PlayerManager.instance.statsMan;

        if (actions == null)
        {
            Debug.LogError("player actions is null");
        }
        //currentGridPos = activeRoom.groundTilemap.WorldToCell(transform.position);
    }
    private void Update()
    {
        // Not moving don't care lol.. 
        if (!isMoving)
        {
            return;
        }
        // We are movin and grovin
        transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
        if (Vector3.Distance(transform.position, targetPos) < float.Epsilon)
        {
            isMoving = false;
            Debug.Log($"We moved cells: {currentGridPos}");
            floorPosition();
            Action movement = new Action(Action.TypeOfAction.Movement, currentGridPos, MoveToPosition);
            StartCoroutine(EnemyManager.instance.doAllEnemyActions(transform));
            currentGridPos = activeRoom.groundTilemap.WorldToCell(transform.position);
            _timeManInstance.IncrementIndex();
            _timeManInstance.addAction(movement);
        }
    }
    public static Vector2 SnapCardinal(Vector2 inputDir)
    {
        if (Mathf.Abs(inputDir.x) > Mathf.Abs(inputDir.y))
        {
            return new Vector2(Mathf.Sign(inputDir.x), 0);
        }
        return new Vector2(0, MathF.Sign(inputDir.y));
    }
    public void Move(Vector2 direction)
    {
        // No point and taking direction while moving.. 
        if (isMoving)
        {
            // Debug.Log("Player is already moving");
            return;
        }
        // Lets check if we can move.. 
        if (CanMove(direction))
        {
            Vector3Int gridPos = activeRoom.doorTilemap.WorldToCell(transform.position + (Vector3)direction);
            Vector3 movementScale = (Vector3)direction;
            if (activeRoom.doorTilemap.HasTile(gridPos))
            {
                movementScale = (Vector3)direction * 3;
            }
            targetPos = transform.position + movementScale;
            isMoving = true;
            _man.updateSteps(1);
            orient.swtichOrientation(direction);
            if (orient.isLeft)
            {
                PartcleManager.instance.makePartcleFX(PartcleManager.PartcleType.DustLeft, orient.leftDust);
            }
            else
            {
                PartcleManager.instance.makePartcleFX(PartcleManager.PartcleType.DustRight, orient.rightDust);
            }

            // Debug.Log($"Lets move?: isMoving? {isMoving}");
        }
    }
    public bool CanMove(Vector2 direction)
    {
        if (_man.currentSteps <= 0)
        {
            Debug.Log("Become unalive");
            return false;
        }

        if (EnemyManager.instance.enemyTurn)
        {
            _isAttacked = false;
            Debug.Log("Its the enemies turn");
            return false;
        }

        Vector3Int gridPos = activeRoom.groundTilemap.WorldToCell(transform.position + (Vector3)direction);

        if (FloorManager.instance.doorsAreOpen && activeRoom.doorTilemap.HasTile(gridPos)) // also check if doors are open
        {
            Debug.Log("We can go there!");
            return true;
        }
        else if (!activeRoom.groundTilemap.HasTile(gridPos) || activeRoom.collisionTilemap.HasTile(gridPos))
        {
            Debug.Log($"We can't go in dir:{direction}? its blocked: IsWall: {activeRoom.collisionTilemap.HasTile(gridPos)} or noGround: {!activeRoom.groundTilemap.HasTile(gridPos)}?");
            return false;
        }


        if (actions.checkAttack(_man.currentAttack, direction, _isAttacked))
        {
            _isAttacked = true;
            Debug.Log("We Enemy is right in front or behind us");
            StartCoroutine(EnemyManager.instance.doAllEnemyActions(transform));
            return false;
        }
        return true;
    }
    public void floorPosition()
    {
        transform.position = new Vector3(Mathf.Floor(transform.position.x), Mathf.Floor(transform.position.y), transform.position.z);
    }
    public bool MoveToPosition(Vector3 pos)
    {

        // take a direction!
        Vector2 direction = new Vector2(pos.x - transform.position.x, pos.y - transform.position.y).normalized;
        if (CanMove(direction))
        {
            // transform.position = pos;
            // currentGridPos = activeRoom.groundTilemap.WorldToCell(transform.position);
            StartCoroutine(FullscreenFXController.instance.TimeFX());
            Move(direction);
            return true;
        }
        else
        {
            Debug.Log("We cannot move");
            return false;
        }
    }
}