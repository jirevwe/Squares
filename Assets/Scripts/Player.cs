using UnityEngine;

public class Player : MonoBehaviour {

    Vector3 position = Vector3.zero;
    bool move = false;

    void Awake () {
        position = transform.position;
        Controller.controller.NodeFromWorldPoint(position).walkable = false;
        move = true;
    }
	
	void Update () {
        GetInput();
        PlayerMove();
    }

    void PlayerMove()
    {
        if (transform.position != position && position != Vector3.zero)
        {
            transform.position = Vector3.MoveTowards(transform.position, position, Time.deltaTime * Controller.controller.moveRate);
        }
        if (transform.position == position)
        {
            move = false;
            Controller.controller.swipe = EasyTouch.SwipeDirection.None;
            PlayerOnObjective(position);
        }
    }

    void GetInput()
    {
        if (move == true)
            return;

        if (Input.GetKeyUp(KeyCode.RightArrow) || Controller.controller.swipe == EasyTouch.SwipeDirection.Right)
        {
            OnInputRight();
        }
        else if (Input.GetKeyUp(KeyCode.LeftArrow) || Controller.controller.swipe == EasyTouch.SwipeDirection.Left)
        {
            OnInputLeft();
        }
        else if (Input.GetKeyUp(KeyCode.UpArrow) || Controller.controller.swipe == EasyTouch.SwipeDirection.Up)
        {
            OnInputUp();
        }
        else if (Input.GetKeyUp(KeyCode.DownArrow) || Controller.controller.swipe == EasyTouch.SwipeDirection.Down)
        {
            OnInputDown();
        }
    }


    public void OnInputUp()
    {
        Vector3 positionToMoveTo = Vector3.zero;

        Node node = Controller.controller.NodeFromWorldPoint(transform.position);
        node.walkable = true;

        int currentX = node.gridX;
        for (; currentX > 0; currentX--)
        {
            if (Controller.controller.grid[currentX, node.gridY].walkable)
                positionToMoveTo = Controller.controller.grid[currentX, node.gridY].worldPosition;
            else break;
        }
        move = true;
        Controller.controller.NodeFromWorldPoint(positionToMoveTo).walkable = false;
        position = positionToMoveTo;
        if (transform.position != position)
            ++Controller.controller.movesMade;
    }

    public void OnInputDown()
    {
        Vector3 positionToMoveTo = Vector3.zero;

        Node node = Controller.controller.NodeFromWorldPoint(transform.position);
        node.walkable = true;

        int currentX = node.gridX;
        for (; currentX < Controller.controller.gridSizeX; currentX++)
        {
            if (Controller.controller.grid[currentX, node.gridY].walkable)
                positionToMoveTo = Controller.controller.grid[currentX, node.gridY].worldPosition;
            else break;
        }
        move = true;
        Controller.controller.NodeFromWorldPoint(positionToMoveTo).walkable = false;
        position = positionToMoveTo;
        if (transform.position != position)
            ++Controller.controller.movesMade;
    }

    public void OnInputLeft()
    {
        Vector3 positionToMoveTo = Vector3.zero;

        Node node = Controller.controller.NodeFromWorldPoint(transform.position);
        node.walkable = true;

        int currentY = node.gridY;
        for (; currentY > 0; currentY--)
        {
            if (Controller.controller.grid[node.gridX, currentY].walkable)
                positionToMoveTo = Controller.controller.grid[node.gridX, currentY].worldPosition;
            else break;
        }
        move = true;
        Controller.controller.NodeFromWorldPoint(positionToMoveTo).walkable = false;
        position = positionToMoveTo;
        if (transform.position != position)
            ++Controller.controller.movesMade;
    }

    public void OnInputRight()
    {
        Vector3 positionToMoveTo = Vector3.zero;

        Node node = Controller.controller.NodeFromWorldPoint(transform.position);
        node.walkable = true;

        int currentY = node.gridY;
        for (; currentY < Controller.controller.gridSizeY; currentY++)
        {
            if (Controller.controller.grid[node.gridX, currentY].walkable)
                positionToMoveTo = Controller.controller.grid[node.gridX, currentY].worldPosition;
            else break;
        }
        move = true;
        Controller.controller.NodeFromWorldPoint(positionToMoveTo).walkable = false;
        position = positionToMoveTo;
        if (transform.position == position)
            return;
        ++Controller.controller.movesMade;
    }

    public void PlayerOnObjective(Vector3 currentPos)
    {
        Node node = Controller.controller.objectiveNodes.Find(n => n.worldPosition == currentPos);
        if (node != null)
        {
            int index = Controller.controller.objectiveNodes.IndexOf(node);
            Controller.controller.objectiveNodes.RemoveAt(index);

            Controller.controller.objectiveNodeGameobjects[index].SetActive(false);
            Controller.controller.objectiveNodeGameobjects.RemoveAt(index);
        }
    }
}
