﻿using UnityEngine;
using System.Collections.Generic;
using System;
using System.IO;
using System.Collections;

public class Controller : MonoBehaviour {

    public static Controller instance;

	public bool displayGridGizmos;
    public float moveRate;
	public float nodeRadius;
	public Node[,] grid;
    public string[,] level;
    public TextAsset levelTextFile;
    public GameObject ground;
    public GameObject wall;
    public GameObject playerPrefab;
    public GameObject objectiveTile;
    public LayerMask whatIsPlayer;
    public LayerMask whatIsWall;
    public LayerMask whatIsTile;
    public bool debugMode;
    public List<Node> objectiveNodes;

    bool isPressed = false;
    bool move = false; 
    GameObject player;
    Vector2 gridWorldSize;
    float nodeDiameter;
	int gridSizeX, gridSizeY;
    RaycastHit hitInfo;
    GameObject objectSelected = null;
    GameObject TileHolder;
    GameObject WallHolder;
    GameObject ObjectiveHolder;
    bool selected;
    Vector3 position = Vector3.zero;
    Vector3 initialClickPos;
    Vector3 finalClickPos;
    List<GameObject> objectiveNodeGameobjects;

    public EasyTouch.SwipeDirection swipe;

    void Start()
    {
        instance = this;
        WallHolder = GameObject.FindGameObjectWithTag("WallHolder");
        TileHolder = GameObject.FindGameObjectWithTag("TileHolder");
        ObjectiveHolder = GameObject.FindGameObjectWithTag("ObjectiveHolder");
    }

	void Awake()
	{
        WallHolder = GameObject.FindGameObjectWithTag("WallHolder");
        TileHolder = GameObject.FindGameObjectWithTag("TileHolder");
        ObjectiveHolder = GameObject.FindGameObjectWithTag("ObjectiveHolder");

        level = Load(levelTextFile);
        instance = this;
        nodeDiameter = nodeRadius * 2;

        gridWorldSize.x = level.GetUpperBound(0) + 1;
        gridWorldSize.y = level.GetUpperBound(1) + 1;

        gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
		gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);

        objectiveNodes = new List<Node>();
        objectiveNodeGameobjects = new List<GameObject>();

        CreateGrid();

        position = player.transform.position;
        NodeFromWorldPoint(position).walkable = false;
        move = true;
	}

    void Update()
    {
        if (instance == null)
            instance = this;

        GetInput();
        PlayerMove();

        if (Input.GetKeyUp(KeyCode.Escape))
            Application.Quit();
    }

    void LateUpdate()
    {
        if (!debugMode)
            return;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if(Input.GetMouseButtonUp(1) && Physics.Raycast(ray, out hitInfo))
        {
            var vect = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            vect.y = 0;
            Debug.Log(NodeFromWorldPoint(vect).walkable);
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

    void PlayerMove()
    {
        if (player.transform.position != position && position != Vector3.zero)
        {
            player.transform.position = Vector3.MoveTowards(player.transform.position, position, Time.deltaTime * moveRate);
        }
        if (player.transform.position == position)
        {
            move = false;
            swipe = EasyTouch.SwipeDirection.None;
            PlayerOnObjective(position);
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

                if(angleOfSwipe > 0 && angleOfSwipe < 30)
                {

                }
            }
        }
    }

    void GetInput()
    {
        if (move == true)
            return;

        //GetSwipe();

        if (Input.GetKeyUp(KeyCode.RightArrow) || swipe == EasyTouch.SwipeDirection.Right)
        {
            OnInputRight();
        }
        else if (Input.GetKeyUp(KeyCode.LeftArrow) || swipe == EasyTouch.SwipeDirection.Left)
        {
            OnInputLeft();
        }
        else if (Input.GetKeyUp(KeyCode.UpArrow) || swipe == EasyTouch.SwipeDirection.Up)
        {
            OnInputUp();
        }
        else if (Input.GetKeyUp(KeyCode.DownArrow) || swipe == EasyTouch.SwipeDirection.Down)
        {
            OnInputDown();
        }
    }

    public void OnInputUp()
    {
        Vector3 positionToMoveTo = Vector3.zero;

        Node node = NodeFromWorldPoint(player.transform.position);
        node.walkable = true;

        int currentX = node.gridX;
        for (; currentX > 0; currentX--)
        {
            if (grid[currentX, node.gridY].walkable)
            {
                positionToMoveTo = grid[currentX, node.gridY].worldPosition;
            }
            else
                break;
        }
        move = true;
        NodeFromWorldPoint(positionToMoveTo).walkable = false;
        position = positionToMoveTo;
    }

    public void OnInputDown()
    {
        Vector3 positionToMoveTo = Vector3.zero;

        Node node = NodeFromWorldPoint(player.transform.position);
        node.walkable = true;

        int currentX = node.gridX;
        for (; currentX < gridSizeX; currentX++)
        {
            if (grid[currentX, node.gridY].walkable)
            {
                positionToMoveTo = grid[currentX, node.gridY].worldPosition;
            }
            else
                break;
        }
        move = true;
        NodeFromWorldPoint(positionToMoveTo).walkable = false;
        position = positionToMoveTo;
    }

    public void OnInputLeft()
    {
        Vector3 positionToMoveTo = Vector3.zero;

        Node node = NodeFromWorldPoint(player.transform.position);
        node.walkable = true;

        int currentY = node.gridY;
        for (; currentY > 0; currentY--)
        {
            if (grid[node.gridX, currentY].walkable)
            {
                positionToMoveTo = grid[node.gridX, currentY].worldPosition;
            }
            else
                break;
        }
        move = true;
        NodeFromWorldPoint(positionToMoveTo).walkable = false;
        position = positionToMoveTo;
    }

    public void OnInputRight()
    {
        Vector3 positionToMoveTo = Vector3.zero;

        Node node = NodeFromWorldPoint(player.transform.position);
        node.walkable = true;

        int currentY = node.gridY;
        for (; currentY < gridSizeY; currentY++)
        {
            if (grid[node.gridX, currentY].walkable)
            {
                positionToMoveTo = grid[node.gridX, currentY].worldPosition;
            }
            else
                break;
        }
        move = true;
        NodeFromWorldPoint(positionToMoveTo).walkable = false;
        position = positionToMoveTo;
    }

    public void PlayerOnObjective(Vector3 currentPos)
    {
        Node node = objectiveNodes.Find(n => n.worldPosition == currentPos);
        if (node != null)
        {
            int index = objectiveNodes.IndexOf(node);
            objectiveNodes.RemoveAt(index);

            objectiveNodeGameobjects[index].SetActive(false);
            objectiveNodeGameobjects.RemoveAt(index);
        }
    }

    public int MaxSize
	{
		get
		{
			return gridSizeX * gridSizeY;
		}
	}

	void CreateGrid()
	{
		grid = new Node[gridSizeX, gridSizeY];
		Vector3 worldBottomLeft = transform.position - Vector3.right * gridWorldSize.x / 2 - Vector3.forward * gridWorldSize.y / 2;

		for (int x = 0; x < gridSizeX; x++)
		{
			for (int y = 0; y < gridSizeY; y++)
			{
				Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * nodeDiameter + nodeRadius) + Vector3.forward * (y * nodeDiameter + nodeRadius);
				grid[x, y] = new Node(false, worldPoint, x, y);

                Node node; GameObject g;
                switch (level[x, y])
                {
                    case "0":
                        //tile
                        node = grid[x, y];
                        node.walkable = true;
                        g = Instantiate(ground, node.worldPosition, Quaternion.identity) as GameObject;
                        g.transform.parent = TileHolder.transform;

                        break;
                    case "1":
                        //immobile wall
                        node = grid[x, y];
                        node.walkable = false;
                        g = Instantiate(wall, node.worldPosition, Quaternion.identity) as GameObject;
                        g.transform.parent = WallHolder.transform;

                        break;
                    case "P":
                        //tile
                        node = grid[x, y];
                        node.walkable = true;
                        g = Instantiate(ground, node.worldPosition, Quaternion.identity) as GameObject;
                        g.transform.parent = TileHolder.transform;

                        //player
                        player = Instantiate(playerPrefab, node.worldPosition, Quaternion.identity) as GameObject;

                        break;
                    case "D":
                        node = grid[x, y];
                        node.walkable = false;

                        //tile
                        g = Instantiate(ground, node.worldPosition, Quaternion.identity) as GameObject;
                        g.transform.parent = TileHolder.transform;

                        //mobile wall 
                        g = Instantiate(wall, node.worldPosition, Quaternion.identity) as GameObject;
                        g.transform.parent = WallHolder.transform;
                        g.AddComponent<WallMobility>();
                        g.GetComponent<WallMobility>().direction = WallMobility.MovementDirection.DOWN;
                        g.GetComponent<WallMobility>().occurRate = .1f;
                        g.GetComponent<WallMobility>().moveSpeed = 3f;
                        g.GetComponent<WallMobility>().whatIsWall = whatIsWall;
                        g.GetComponent<WallMobility>().node = node;
                        g.GetComponent<WallMobility>().grid = grid;

                        break;
                    case "U":
                        node = grid[x, y];
                        node.walkable = false;

                        //tile
                        g = Instantiate(ground, node.worldPosition, Quaternion.identity) as GameObject;
                        g.transform.parent = TileHolder.transform;

                        //mobile wall 
                        g = Instantiate(wall, node.worldPosition, Quaternion.identity) as GameObject;
                        g.transform.parent = WallHolder.transform;
                        g.AddComponent<WallMobility>();
                        g.GetComponent<WallMobility>().direction = WallMobility.MovementDirection.UP;
                        g.GetComponent<WallMobility>().occurRate = .1f;
                        g.GetComponent<WallMobility>().moveSpeed = 3f;
                        g.GetComponent<WallMobility>().whatIsWall = whatIsWall;
                        g.GetComponent<WallMobility>().node = node;
                        g.GetComponent<WallMobility>().grid = grid;

                        break;
                    case "L":
                        node = grid[x, y];
                        node.walkable = false;

                        //tile
                        g = Instantiate(ground, node.worldPosition, Quaternion.identity) as GameObject;
                        g.transform.parent = TileHolder.transform;

                        //mobile wall 
                        g = Instantiate(wall, node.worldPosition, Quaternion.identity) as GameObject;
                        g.transform.parent = WallHolder.transform;
                        g.AddComponent<WallMobility>();
                        g.GetComponent<WallMobility>().direction = WallMobility.MovementDirection.LEFT;
                        g.GetComponent<WallMobility>().occurRate = .1f;
                        g.GetComponent<WallMobility>().moveSpeed = 3f;
                        g.GetComponent<WallMobility>().whatIsWall = whatIsWall;
                        g.GetComponent<WallMobility>().node = node;
                        g.GetComponent<WallMobility>().grid = grid;

                        break;
                    case "R":
                        node = grid[x, y];
                        node.walkable = false;

                        //tile
                        g = Instantiate(ground, node.worldPosition, Quaternion.identity) as GameObject;
                        g.transform.parent = TileHolder.transform;

                        //mobile wall 
                        g = Instantiate(wall, node.worldPosition, Quaternion.identity) as GameObject;
                        g.transform.parent = WallHolder.transform;
                        g.AddComponent<WallMobility>();
                        g.GetComponent<WallMobility>().direction = WallMobility.MovementDirection.RIGHT;
                        g.GetComponent<WallMobility>().occurRate = .1f;
                        g.GetComponent<WallMobility>().moveSpeed = 3f;
                        g.GetComponent<WallMobility>().whatIsWall = whatIsWall;
                        g.GetComponent<WallMobility>().node = node;
                        g.GetComponent<WallMobility>().grid = grid;

                        break;
                    case "X":
                        node = grid[x, y];
                        node.walkable = true;
                        node.colored = true;
                        objectiveNodes.Add(node);

                        //tile
                        g = Instantiate(ground, node.worldPosition, Quaternion.identity) as GameObject;
                        g.transform.parent = TileHolder.transform;

                        //objective
                        g = Instantiate(objectiveTile, node.worldPosition, Quaternion.identity) as GameObject;
                        g.transform.parent = ObjectiveHolder.transform;
                        objectiveNodeGameobjects.Add(g);

                        break;
                }
            }
		}
	}

    private string[,] Load(TextAsset fileName)
    {
        string[,] levelFile = null;

        try
        {
            string allLines = fileName.text;
            string[] lines = allLines.Split(new char[] { '#' });

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].Trim(new char[] { '\n', '\r', '\t' });
                if (line != null)
                {
                    if (levelFile == null)
                        levelFile = new string[lines.Length, line.Length];
                    for (int j = 0; j < line.Length; j++)
                    {
                        levelFile[i, j] = line[j].ToString();
                    }
                }
            }
            return levelFile;
        }catch (IOException e)
        {
            Debug.LogError(string.Format("{0}\n", e.Message));
            return levelFile;
        }
    }

    public List<Node> GetNeighbours(Node node)
	{
		List<Node> neighbours = new List<Node>();

		for (int x = -1; x <= 1; x++)
		{
			for (int y = -1; y <= 1; y++)
			{
				if (x == 0 && y == 0)
					continue;

				int checkX = node.gridX + x;
				int checkY = node.gridY + y;

				if (checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY)
				{
					neighbours.Add(grid[checkX, checkY]);
				}
			}
		}

		return neighbours;
	}

	public Node NodeFromWorldPoint(Vector3 worldPosition)
	{
		float percentX = (worldPosition.x + gridWorldSize.x / 2) / gridWorldSize.x;
		float percentY = (worldPosition.z + gridWorldSize.y / 2) / gridWorldSize.y;
		percentX = Mathf.Clamp01(percentX);
		percentY = Mathf.Clamp01(percentY);

		int x = Mathf.RoundToInt((gridSizeX - 1) * percentX);
		int y = Mathf.RoundToInt((gridSizeY - 1) * percentY);
		return grid[x, y];
	}

    #region SwipeInput

    void OnEnable()
    {
        EasyTouch.On_SwipeStart += On_SwipeStart;
        EasyTouch.On_Swipe += On_Swipe;
        EasyTouch.On_SwipeEnd += On_SwipeEnd;
    }

    void OnDisable()
    {
        UnsubscribeEvent();

    }

    void OnDestroy()
    {
        UnsubscribeEvent();
    }

    void UnsubscribeEvent()
    {
        EasyTouch.On_SwipeStart -= On_SwipeStart;
        EasyTouch.On_Swipe -= On_Swipe;
        EasyTouch.On_SwipeEnd -= On_SwipeEnd;
    }


    // At the swipe beginning 
    private void On_SwipeStart(Gesture gesture)
    {

    }

    // During the swipe
    private void On_Swipe(Gesture gesture)
    {

    }

    // At the swipe end 
    private void On_SwipeEnd(Gesture gesture)
    {
        swipe = gesture.swipe;
    }

    #endregion
}
