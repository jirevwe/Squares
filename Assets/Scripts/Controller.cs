using UnityEngine;
using System.Collections.Generic;
using System.IO;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Controller : MonoBehaviour {

    public static Controller controller;

	public bool displayGridGizmos;
    public float moveRate;
	public float nodeRadius;
	public Node[,] grid;
    public string[,] level;
    public GameObject ground;
    public GameObject wall;
    public GameObject playerPrefab;
    public GameObject objectiveTile;
    public LayerMask whatIsPlayer;
    public LayerMask whatIsWall;
    public LayerMask whatIsTile;
    public bool debugMode;
    public List<Node> objectiveNodes;
    public Text gameTime;
    public Text movesText;
    public Texture movableBoxTexture;

    bool isPressed = false;
    Vector2 gridWorldSize;
    float nodeDiameter;
	public int gridSizeX, gridSizeY;
    RaycastHit hitInfo;
    GameObject objectSelected = null;
    GameObject TileHolder;
    GameObject WallHolder;
    GameObject ObjectiveHolder;
    bool selected;
    Vector3 initialClickPos;
    Vector3 finalClickPos;
    public List<GameObject> objectiveNodeGameobjects;
    float time;
    public float movesMade;
    TextAsset levelTextFile;

    public EasyTouch.SwipeDirection swipe;

    void Start()
    {
        controller = this;
        WallHolder = GameObject.FindGameObjectWithTag("WallHolder");
        TileHolder = GameObject.FindGameObjectWithTag("TileHolder");
        ObjectiveHolder = GameObject.FindGameObjectWithTag("ObjectiveHolder");
    }

	void Awake()
	{
        WallHolder = GameObject.FindGameObjectWithTag("WallHolder");
        TileHolder = GameObject.FindGameObjectWithTag("TileHolder");
        ObjectiveHolder = GameObject.FindGameObjectWithTag("ObjectiveHolder");

        levelTextFile = Resources.Load(PlayerPrefs.GetString("LEVEL_TO_LOAD")) as TextAsset;

        level = Load(levelTextFile);
        controller = this;
        nodeDiameter = nodeRadius * 2;

        gridWorldSize.x = level.GetUpperBound(0) + 1;
        gridWorldSize.y = level.GetUpperBound(1) + 1;

        gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
		gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);

        objectiveNodes = new List<Node>();
        objectiveNodeGameobjects = new List<GameObject>();

        CreateGrid();
    }

    void Update()
    {
        if (controller == null)
            controller = this;

        UpdateGameTime();
        movesText.text = movesMade.ToString();

        if (Input.GetKeyUp(KeyCode.Escape))
            SceneManager.LoadScene(0);
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

    void UpdateGameTime()
    {
        time += Time.deltaTime;

        var minutes = time / 60;            //Divide the guiTime by sixty to get the minutes.
        var seconds = time % 60;            //Use the euclidean division for the seconds.
        //var fraction = (time * 100) % 100;

        //update the label value
        //gameTime.text = string.Format("{0:00} : {1:00} : {2:000}", minutes, seconds, fraction);
        gameTime.text = string.Format("{0:00} : {1:00}", minutes, seconds);
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

                        //ground
                        g = Instantiate(ground, node.worldPosition, Quaternion.identity) as GameObject;
                        g.transform.parent = TileHolder.transform;

                        break;
                    case "1":
                        //immobile wall
                        node = grid[x, y];
                        node.walkable = false;

                        //wall
                        g = Instantiate(wall, node.worldPosition, Quaternion.identity) as GameObject;
                        g.transform.parent = WallHolder.transform;

                        break;
                    case "P":
                        //tile
                        node = grid[x, y];
                        node.walkable = true;
                        
                        //ground
                        g = Instantiate(ground, node.worldPosition, Quaternion.identity) as GameObject;
                        g.transform.parent = TileHolder.transform;

                        //player
                        g = Instantiate(playerPrefab, node.worldPosition, Quaternion.identity) as GameObject;

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
                        g.GetComponent<MeshRenderer>().materials[0].mainTexture = movableBoxTexture;
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
                        g.GetComponent<MeshRenderer>().materials[0].mainTexture = movableBoxTexture;
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
                        g.GetComponent<MeshRenderer>().materials[0].mainTexture = movableBoxTexture;
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
                        g.GetComponent<MeshRenderer>().materials[0].mainTexture = movableBoxTexture;
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
        }
        catch (IOException e)
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
