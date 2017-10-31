using UnityEngine;

public class Avatar : MonoBehaviour {

	string id = "";
    public GameObject boundingBox;
    public GameObject avatarModel;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {

	}

	public string getID() 
	{
		return id;
	}

	public UserObject getUserObject()
	{
		Color col 	= avatarModel.GetComponent<Renderer>().material.color;
		Vector3 pos = MUFramework.toFrameworkPos( transform.position );

		UserObject userObj = new UserObject();
		userObj.id = id;
		//userObj.r = (int)(col.r * 255.0f);
		//userObj.g = (int)(col.g * 255.0f);
		//userObj.b = (int)(col.b * 255.0f);
		//userObj.x = pos.x;
		//userObj.y = pos.y;
		//userObj.z = pos.z;
        userObj.color[0] = (int)(col.r * 255.0f);
        userObj.color[1] = (int)(col.g * 255.0f);
        userObj.color[2] = (int)(col.b * 255.0f);
        userObj.pos_head[0] = pos.x;
        userObj.pos_head[1] = pos.y;
        userObj.pos_head[2] = pos.z;
        userObj.message = "";
		return userObj;
	}

	public void setUserObject( UserObject userObj )
	{
		id = userObj.id;
        avatarModel.GetComponent<Renderer>().material.color	= new Color( userObj.color[0]/255.0f, userObj.color[1]/255.0f, userObj.color[2]/255.0f );
        GetComponent<Transform>().position 		            = MUFramework.toUnityPos( new Vector3(userObj.pos_head[0], userObj.pos_head[1], userObj.pos_head[2]) );
	}
}
