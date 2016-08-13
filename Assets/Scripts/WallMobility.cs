using UnityEngine;
using System.Collections;

public class WallMobility : MonoBehaviour {

    public MovementDirection direction;
    public float occurRate;
    public float moveSpeed;

    [HideInInspector]
    public LayerMask whatIsWall;
    [HideInInspector]
    public Node[,] grid;
    [HideInInspector]
    public Node node;

    Vector3 positionToMoveTo = Vector3.zero;
    public bool move, flip = false;
    RaycastHit hitInfo;

    public enum MovementDirection {
        UP, DOWN, LEFT, RIGHT
    }

    void LateUpdate()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Input.GetMouseButtonUp(0) && Physics.Raycast(ray, out hitInfo, 10, whatIsWall))
        {
            if(hitInfo.collider.transform.position == transform.position && hitInfo.transform.gameObject.GetComponent<WallMobility>() != null)
            {
                if (flip)
                    StartCoroutine(Reverse());
                else
                    StartCoroutine(Movement());
            }
        }
    }

    void Update () {
        //if (Controller.instance != null && Controller.instance.grid != null && node == null)
        //{
        //    grid = Controller.instance.grid;
        //    node = Controller.instance.NodeFromWorldPoint(transform.position);
        //    originalPosition = node.worldPosition;
        //}

        Move();
    }
	
    IEnumerator Reverse()
    {
        if (node == null)
            yield return null;

        yield return new WaitForSeconds(occurRate);

        Node currentNode = Controller.instance.NodeFromWorldPoint(transform.position);
        currentNode.walkable = true;

        positionToMoveTo = node.worldPosition;
        node.walkable = false;

        move = true;
        flip = false;

        yield return null;
    }

    IEnumerator Movement()
    {
        if(node == null)
            yield return null;

        yield return new WaitForSeconds(occurRate);

        int curr = 0;
        Node currentNode;

        switch (direction)
        {
            case MovementDirection.DOWN:
                curr = node.gridX;
                if (grid[++curr, node.gridY].walkable)
                    positionToMoveTo = grid[curr, node.gridY].worldPosition;
                move = true;
                flip = true;

                break;
            case MovementDirection.UP:
                curr = node.gridX;
                if (grid[--curr, node.gridY].walkable)
                    positionToMoveTo = grid[curr, node.gridY].worldPosition;
                move = true;
                flip = true;

                break;
            case MovementDirection.LEFT:
                curr = node.gridY;
                if (grid[node.gridX, --curr].walkable)
                    positionToMoveTo = grid[node.gridX, curr].worldPosition;
                move = true;
                flip = true;

                break;
            case MovementDirection.RIGHT:
                curr = node.gridY;
                if (grid[node.gridX, ++curr].walkable)
                    positionToMoveTo = grid[node.gridX, curr].worldPosition;
                move = true;
                flip = true;

                break;
        }

        currentNode = Controller.instance.NodeFromWorldPoint(positionToMoveTo);
        currentNode.walkable = false;
        node.walkable = true;

        yield return null;
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
}
