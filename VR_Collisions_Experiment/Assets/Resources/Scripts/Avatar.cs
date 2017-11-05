using UnityEngine;

public class Avatar : MonoBehaviour {

	string id = "";
    public GameObject boundingBox;
    public GameObject model_body;
    public GameObject model_hand_L;
    public GameObject model_Hand_R;

    // Use this for initialization
    void Start ()
    {}
	
	// Update is called once per frame
	void Update ()
    {}

	public string getID() 
	{
		return id;
	}

	//public UserObject getUserObject()
	//{
	//	Color col 	        = model_body.GetComponent<Renderer>().material.color;
	//	Vector3 pos_head    = MUFramework.toFrameworkPos( transform.position );

	//	UserObject userObj = new UserObject();
	//	userObj.id = id;
 //       userObj.color[0] = (int)(col.r * 255.0f);
 //       userObj.color[1] = (int)(col.g * 255.0f);
 //       userObj.color[2] = (int)(col.b * 255.0f);
 //       userObj.pos_head[0] = pos_head.x;
 //       userObj.pos_head[1] = pos_head.y;
 //       userObj.pos_head[2] = pos_head.z;
 //       userObj.message = "";
	//	return userObj;
	//}

	public void setUserObject( UserObject userObj )
	{
		id = userObj.id;

        Color matCol = new Color(userObj.color[0] / 255.0f, userObj.color[1] / 255.0f, userObj.color[2] / 255.0f);
        model_body.GetComponent<Renderer>().material.color = matCol;
        model_hand_L.GetComponent<Renderer>().material.color = matCol;
        model_Hand_R.GetComponent<Renderer>().material.color = matCol;

        model_body.GetComponent<Transform>().position   = MUFramework.toUnityPos(new Vector3(userObj.pos_head[INDEX.X], userObj.pos_head[INDEX.Y], userObj.pos_head[INDEX.Z]));
        model_hand_L.GetComponent<Transform>().position = MUFramework.toUnityPos(new Vector3(userObj.pos_hand_L[INDEX.X], userObj.pos_hand_L[INDEX.Y], userObj.pos_hand_L[INDEX.Z]));
        model_Hand_R.GetComponent<Transform>().position = MUFramework.toUnityPos(new Vector3(userObj.pos_hand_R[INDEX.X], userObj.pos_hand_R[INDEX.Y], userObj.pos_hand_R[INDEX.Z]));

        model_body.GetComponent<Transform>().rotation   = new Quaternion(userObj.rot_head[INDEX.X], userObj.rot_head[INDEX.Y], userObj.rot_head[INDEX.Z], userObj.rot_head[INDEX.W]);
        model_hand_L.GetComponent<Transform>().rotation = new Quaternion(userObj.rot_hand_L[INDEX.X], userObj.rot_hand_L[INDEX.Y], userObj.rot_hand_L[INDEX.Z], userObj.rot_hand_L[INDEX.W]);
        model_Hand_R.GetComponent<Transform>().rotation = new Quaternion(userObj.rot_hand_R[INDEX.X], userObj.rot_hand_R[INDEX.Y], userObj.rot_hand_R[INDEX.Z], userObj.rot_hand_R[INDEX.W]);
    }
}
