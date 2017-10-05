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

	Vector3 toUnityPos( Vector3 oldVec )
	{
		Vector3 vec = new Vector3();
		vec.x = map( oldVec.x, 0.0f, 1.0f, -3.0f, 3.0f );
		vec.y = 0.0f;
		vec.z = map( oldVec.z, 0.0f, 1.0f, -3.0f, 3.0f );
		return vec;
	}

	Vector3 toFrameworkPos( Vector3 oldVec )
	{
		Vector3 vec = new Vector3();
		vec.x = map( oldVec.x, -3.0f, 3.0f, 0.0f, 1.0f );
		vec.y = 0.0f;
		vec.z = map( oldVec.z, -3.0f, 3.0f, 0.0f, 1.0f );
		return vec;
	}

	public string getID() 
	{
		return id;
	}

	public UserObject getUserObject()
	{
		Color col 	= avatarModel.GetComponent<Renderer>().material.color;
		Vector3 pos = toFrameworkPos( transform.position );

		UserObject userObj = new UserObject();
		userObj.id = id;
		userObj.r = (int)(col.r * 255.0f);
		userObj.g = (int)(col.g * 255.0f);
		userObj.b = (int)(col.b * 255.0f);
		userObj.x = pos.x;
		userObj.y = pos.y;
		userObj.z = pos.z;
		userObj.message = "";
		return userObj;
	}

	public void setUserObject( UserObject userObj )
	{
		id = userObj.id;
        avatarModel.GetComponent<Renderer>().material.color	= new Color( userObj.r/255.0f, userObj.g/255.0f, userObj.b/255.0f );
		GetComponent<Transform>().position 		            = toUnityPos( new Vector3(userObj.x, userObj.y, userObj.z) );
	}

	float map(float x, float in_min, float in_max, float out_min, float out_max)
    {
        return (x - in_min) * (out_max - out_min) / (in_max - in_min) + out_min;
    }
}
