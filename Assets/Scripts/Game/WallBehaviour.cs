using UnityEngine;

public class WallBehaviour : MonoBehaviour, CellBehaviour {

    public float speed;
    public float shakeTime;
    public LayerMask whatIsWall;
    private RaycastHit hitInfo;
    private bool shake = false;
    private bool isFixedWall;

    void Start()
    {
        isFixedWall = gameObject.GetComponent<WallMobility>() == null;
    }
	
    void Update()
    {
        if (shake && isFixedWall)
        {
            float AngleAmount = (Mathf.Cos(Time.time * speed) * 180) / Mathf.PI * 0.5f;
            AngleAmount = Mathf.Clamp(AngleAmount, -15, 15);
            transform.localRotation = Quaternion.Euler(0, AngleAmount, 0);
            Invoke("StopShake", shakeTime);
        }
    }


    void StopShake()
    {
        transform.localRotation = Quaternion.identity;
        shake = false;
    }

	void LateUpdate () {
        if (Input.GetMouseButtonUp(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hitInfo, 10, whatIsWall))
            {
                hitInfo.collider.gameObject.GetComponent<WallBehaviour>().shake = true;
            }
        }
    }

    public void InitBehavoiur(string character, GameObject g)
    {
        
    }
}
