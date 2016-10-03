using UnityEngine;

public class Player : MonoBehaviour {

    bool isPressed = false;
    Vector3 initialClickPos;
    Vector3 finalClickPos;
    public LayerMask whatIsPlayer;
    bool selected;
    RaycastHit hitInfo;
    GameObject objectSelected = null;
    Vector3 position = Vector3.zero;
    bool move = false;

    void Awake () {
        position = transform.position;
        Grid.instance.NodeFromWorldPoint(position).walkable = false;
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
            transform.position = Vector3.MoveTowards(transform.position, position, Time.deltaTime * Grid.instance.moveRate);
        }
        if (transform.position == position)
        {
            move = false;
            Grid.instance.swipe = EasyTouch.SwipeDirection.None;
            PlayerOnObjective(position);
        }
    }

    void GetInput()
    {
        if (move == true)
            return;

        if (Input.GetKeyUp(KeyCode.RightArrow) || Grid.instance.swipe == EasyTouch.SwipeDirection.Right)
        {
            OnInputRight();
        }
        else if (Input.GetKeyUp(KeyCode.LeftArrow) || Grid.instance.swipe == EasyTouch.SwipeDirection.Left)
        {
            OnInputLeft();
        }
        else if (Input.GetKeyUp(KeyCode.UpArrow) || Grid.instance.swipe == EasyTouch.SwipeDirection.Up)
        {
            OnInputUp();
        }
        else if (Input.GetKeyUp(KeyCode.DownArrow) || Grid.instance.swipe == EasyTouch.SwipeDirection.Down)
        {
            OnInputDown();
        }
    }


    public void OnInputUp()
    {
        Vector3 positionToMoveTo = Vector3.zero;

        Cell node = Grid.instance.NodeFromWorldPoint(transform.position);
        node.walkable = true;

        int currentX = node.gridX;
        for (; currentX > 0; currentX--)
        {
            if (((Cell)Grid.instance.grid[currentX, node.gridY]).walkable)
                positionToMoveTo = Grid.instance.grid[currentX, node.gridY].worldPosition;
            else break;
        }
        move = true;
        Grid.instance.NodeFromWorldPoint(positionToMoveTo).walkable = false;
        position = positionToMoveTo;
        if (transform.position != position)
            ++Grid.instance.movesMade;
    }

    public void OnInputDown()
    {
        Vector3 positionToMoveTo = Vector3.zero;

        Cell node = Grid.instance.NodeFromWorldPoint(transform.position);
        node.walkable = true;

        int currentX = node.gridX;
        for (; currentX < Grid.instance.gridSizeX; currentX++)
        {
            if (((Cell)Grid.instance.grid[currentX, node.gridY]).walkable)
                positionToMoveTo = Grid.instance.grid[currentX, node.gridY].worldPosition;
            else break;
        }
        move = true;
        Grid.instance.NodeFromWorldPoint(positionToMoveTo).walkable = false;
        position = positionToMoveTo;
        if (transform.position != position)
            ++Grid.instance.movesMade;
    }

    public void OnInputLeft()
    {
        Vector3 positionToMoveTo = Vector3.zero;

        Cell node = Grid.instance.NodeFromWorldPoint(transform.position);
        node.walkable = true;

        int currentY = node.gridY;
        for (; currentY > 0; currentY--)
        {
            if (((Cell)Grid.instance.grid[node.gridX, currentY]).walkable)
                positionToMoveTo = Grid.instance.grid[node.gridX, currentY].worldPosition;
            else break;
        }
        move = true;
        Grid.instance.NodeFromWorldPoint(positionToMoveTo).walkable = false;
        position = positionToMoveTo;
        if (transform.position != position)
            ++Grid.instance.movesMade;
    }

    public void OnInputRight()
    {
        Vector3 positionToMoveTo = Vector3.zero;

        Cell node = Grid.instance.NodeFromWorldPoint(transform.position);
        node.walkable = true;

        int currentY = node.gridY;
        for (; currentY < Grid.instance.gridSizeY; currentY++)
        {
            if (((Cell)Grid.instance.grid[node.gridX, currentY]).walkable)
                positionToMoveTo = Grid.instance.grid[node.gridX, currentY].worldPosition;
            else break;
        }
        move = true;
        Grid.instance.NodeFromWorldPoint(positionToMoveTo).walkable = false;
        position = positionToMoveTo;
        if (transform.position == position)
            return;
        ++Grid.instance.movesMade;
    }

    public void PlayerOnObjective(Vector3 currentPos)
    {
        Cell node = Grid.instance.objectiveNodes.Find(n => n.worldPosition == currentPos);
        if (node != null)
        {
            int index = Grid.instance.objectiveNodes.IndexOf(node);
            Grid.instance.objectiveNodes.RemoveAt(index);

            Grid.instance.objectiveNodeGameobjects[index].SetActive(false);
            Grid.instance.objectiveNodeGameobjects.RemoveAt(index);
        }
    }

    void LateUpdate()
    {
        if (!Grid.instance.debugMode)
            return;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Input.GetMouseButtonUp(1) && Physics.Raycast(ray, out hitInfo))
        {
            var vect = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            vect.y = 0;
            Debug.Log(Grid.instance.NodeFromWorldPoint(vect).walkable);
        }

        if (Input.GetMouseButtonDown(0) && Physics.Raycast(ray, out hitInfo, 10, whatIsPlayer))
        {
            selected = true;
            objectSelected = hitInfo.collider.gameObject;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            selected = true;
            objectSelected = null;
        }

        if (selected && objectSelected != null)
        {
            var vect = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            vect.y = 0;
            hitInfo.collider.transform.position = vect;
        }
    }

    void GetSwipe()
    {
        //touch input down
        if (Input.GetMouseButtonDown(0) && !isPressed)
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            var vect = ray.origin + (ray.direction * 100f);
            vect.y = 0;

            initialClickPos = vect;

            isPressed = true;
        }

        //touch input up
        if (Input.GetMouseButtonUp(0) && isPressed)
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            var vect = ray.origin + (ray.direction * 100f);
            vect.y = 0;

            finalClickPos = vect;
            isPressed = false;

            if (finalClickPos != initialClickPos)
            {
                float dz = Mathf.Abs(finalClickPos.z - initialClickPos.z);
                float dx = Mathf.Abs(finalClickPos.x - initialClickPos.x);

                float angleOfSwipe = Mathf.Rad2Deg * Mathf.Atan2(dz, dx);

                if (angleOfSwipe > 0 && angleOfSwipe < 30)
                {
                    //todo: I have not decided what to do here yet.
                }
            }
        }
    }
}
