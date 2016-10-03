﻿using UnityEngine;
using System.Collections;
using System;

/// <summary>
/// Component that makes walls move
/// </summary>
public class WallMobility : MonoBehaviour, CellBehaviour {

    public MovementDirection direction;
    public float occurRate;
    public float moveSpeed;

    [HideInInspector]
    public LayerMask whatIsWall;
    [HideInInspector]
    public Cell[,] grid;
    [HideInInspector]
    public Cell node;

    Vector3 positionToMoveTo = Vector3.zero;
    public bool move, flip = false;
    RaycastHit hitInfo;

    public enum MovementDirection {
        UP, DOWN, LEFT, RIGHT
    }

    void Start()
    {
        positionToMoveTo = transform.position;
    }

    void LateUpdate()
    {
        if (Input.GetMouseButtonUp(0) && Grid.instance.swipe == EasyTouch.SwipeDirection.None) {

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hitInfo, 10, whatIsWall))
            {
                if (hitInfo.collider.transform.position == transform.position && hitInfo.transform.gameObject.GetComponent<WallMobility>() != null)
                {
                    if (flip)
                        StartCoroutine(Reverse());
                    else
                        StartCoroutine(Movement());
                }
            }
        }
    }

    void Update () {
        Move();
    }
	
    IEnumerator Reverse()
    {
        if (node != null && node.walkable)
        {
            yield return new WaitForSeconds(occurRate);

            Cell currentNode = Grid.instance.NodeFromWorldPoint(transform.position);
            currentNode.walkable = true;

            positionToMoveTo = node.worldPosition;
            node.walkable = false;

            move = true;
            flip = false;
        }

        yield return null;
    }

    IEnumerator Movement()
    {
        if (node != null)
        {
            yield return new WaitForSeconds(occurRate);

            int curr = 0;
            Cell currentCell = null;

            switch (direction)
            {
                case MovementDirection.DOWN:
                    MoveDown(positionToMoveTo, curr);

                    break;
                case MovementDirection.UP:
                    MoveUp(positionToMoveTo, curr);

                    break;
                case MovementDirection.LEFT:
                    MoveLeft(positionToMoveTo, curr);

                    break;
                case MovementDirection.RIGHT:
                    MoveRight(positionToMoveTo, curr);

                    break;
            }

            currentCell = Grid.instance.NodeFromWorldPoint(positionToMoveTo);
            currentCell.walkable = false;
            node.walkable = true;

            if (transform.position == positionToMoveTo)
                StartCoroutine(Reverse());
        }
        yield return null;
    }

    void MoveUp(Vector3 targetPosition, int curr)
    {
        curr = node.gridX;
        if (grid[--curr, node.gridY].walkable)
            positionToMoveTo = grid[curr, node.gridY].worldPosition;
        move = true;
        flip = true;
    }

    void MoveDown(Vector3 targetPosition, int curr)
    {
        curr = node.gridX;
        if (grid[++curr, node.gridY].walkable)
            positionToMoveTo = grid[curr, node.gridY].worldPosition;
        move = true;
        flip = true;
    }

    void MoveLeft(Vector3 targetPosition, int curr)
    {
        curr = node.gridY;
        if (grid[node.gridX, --curr].walkable)
            positionToMoveTo = grid[node.gridX, curr].worldPosition;
        move = true;
        flip = true;
    }

    void MoveRight(Vector3 targetPosition, int curr)
    {
        curr = node.gridY;
        if (grid[node.gridX, ++curr].walkable)
            positionToMoveTo = grid[node.gridX, curr].worldPosition;
        move = true;
        flip = true;
    }

    void Move()
    {
        if (transform.position != positionToMoveTo && move)
            transform.position = Vector3.MoveTowards(transform.position, positionToMoveTo, Time.deltaTime * moveSpeed);
        if (transform.position == positionToMoveTo && move)
        {
            move = false;
        }
    }

    public void InitBehavoiur(string character, GameObject g)
    {
        
    }
}
