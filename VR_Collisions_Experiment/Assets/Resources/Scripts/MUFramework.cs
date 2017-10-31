using UnityEngine;
using System.Collections.Generic;
using UnitySocketIO;
using UnitySocketIO.Events;

[System.Serializable]
public struct playAreaSize
{
    public float halfWidth_x;
    public float halfWidth_z;
};

public class MUFramework : MonoBehaviour
{
    public SocketIOController io;
    public GameObject head;
    public GameObject rightHand;
    public GameObject leftHand;
    public List<UserObject> usersArr    = new List<UserObject>();
    public UserObject thisUserObj       = null;
    public playAreaSize playAreaSize    = new playAreaSize();


    void Awake()
	{
		Application.runInBackground = true;
	}

	// Use this for initialization
	void Start () 
	{
        //find and set vrPlayArea
        Valve.VR.HmdQuad_t rect = new Valve.VR.HmdQuad_t();
        SteamVR_PlayArea.GetBounds(SteamVR_PlayArea.Size.Calibrated, ref rect);
        playAreaSize.halfWidth_x = Mathf.Abs(rect.vCorners2.v0 - rect.vCorners0.v0)/2.0f;
        playAreaSize.halfWidth_z = Mathf.Abs(rect.vCorners2.v2 - rect.vCorners0.v2)/2.0f;

        //now setupo network events
        string thisUserID = "";

		io.On("connect", (SocketIOEvent e) => {
        	Debug.Log("SocketIO connected");
			io.Emit("newUser");
		});

		io.On("user_connect", (SocketIOEvent e) => {
			UserObject userObj = JSONHelper.FromJsonObject<UserObject>(e.data);
			if (thisUserID.Equals("") ) {
                thisUserID = userObj.id;
				Debug.Log( "id received: " + thisUserID);
			}
		});

		io.On("usersData", (SocketIOEvent e) => {
            thisUserObj = null;
            usersArr.Clear(); //reset

            //get all users noted in data sent from server
            UserObject[] userObjs = JSONHelper.FromJsonArray<UserObject>(e.data);
            //Debug.Log("userObjs.Length: " + userObjs.Length);
            for (int i = 0; i < userObjs.Length; i++) {
                usersArr.Add( (UserObject)userObjs[i]);
                if ( userObjs[i].id == thisUserID ) {
                    thisUserObj = usersArr[usersArr.Count - 1];
                }
            }
		});
		
		io.Connect();
	}
	
	// Update is called once per frame
	void Update () 
    {
        //
        //send this updated userdata acording to avatar positioning via VR
        //
        Vector3 frameworkPos = toFrameworkPos(new Vector3(head.transform.position.x, head.transform.position.y, head.transform.position.z));
        //thisUserObj.x = frameworkPos.x;
        //thisUserObj.y = frameworkPos.y;
        //thisUserObj.z = frameworkPos.z;
        thisUserObj.pos_head[0] = frameworkPos.x;
        thisUserObj.pos_head[1] = frameworkPos.y;
        thisUserObj.pos_head[2] = frameworkPos.z;
        io.Emit("posUpdate", JSONHelper.ToJsonObject<UserObject>(thisUserObj));

        //
        //now update all avatars positions
        //
        GameObject[] userElemArr = GameObject.FindGameObjectsWithTag("Player");
        if (userElemArr.Length > usersArr.Count) {
            Debug.Log("someone disconnected");
            //!!someone has disconnected
            bool idFound = false;

            //iterate backwards so we don't remove in the middle of an array
            for ( int i = userElemArr.Length-1; i >= 0; i-- ) {
                GameObject gameObj = (GameObject)userElemArr[i];
                for ( int j = 0; j < usersArr.Count; j++ ) {
                    UserObject userObj = usersArr[j];
                    if ( gameObj.GetComponent<Avatar>().getID() == userObj.id ) {
                        idFound = true;
                        //break;
                    }
                }

                if (!idFound) {
                    Debug.Log("Deleting Player Object");
                    Destroy( gameObj );
                    //break;
                }
            }

		}
		else if (userElemArr.Length < usersArr.Count) {
			//!!someone has connected
            Debug.Log("someone connected: " + userElemArr.Length + " " + usersArr.Count);
            foreach ( UserObject userObj in usersArr ) {
                bool hasBeenInitiatialized = false;

                //first check if there is element exist, and update if so
                foreach (GameObject gameObj in userElemArr) {
                    if ( gameObj.GetComponent<Avatar>().getID() == userObj.id ) {
                        hasBeenInitiatialized = true;
                        //break;
                    }
                }

                if ( !hasBeenInitiatialized ) {
                    GameObject gameObj = (GameObject)Instantiate(Resources.Load("prefabs/Avatar"));
                    
                    //find children
                    GameObject avatar_model = gameObj.transform.Find("Avatar_Body").gameObject;
                    GameObject boundingBox  = gameObj.transform.Find("BoundingBox").gameObject;

                    //initialize
                    gameObj.GetComponent<Avatar>().avatarModel = avatar_model;  //set first!
                    gameObj.GetComponent<Avatar>().boundingBox = boundingBox;   //set first!

                    gameObj.GetComponent<Avatar>().setUserObject(userObj);      //now set this
                    //break;
                }
            }
		}
		else {
			//update all positions
            //Debug.Log("update positions");
			foreach (GameObject gameObj in userElemArr) {
				foreach (UserObject userObj in usersArr ) {
                    if ( gameObj.GetComponent<Avatar>().getID() == userObj.id ) {
                        gameObj.GetComponent<Avatar>().setUserObject(userObj);
                    }
				}
			}
		}
	}

    static public float map(float value, float in_min, float in_max, float out_min, float out_max, bool doClamp)
    {
        float val = (value - in_min) * (out_max - out_min) / (in_max - in_min) + out_min;
        if (doClamp) {
            val = Mathf.Min( Mathf.Max(val, out_min), out_max);
        }
        return val;
    }

    static public Vector3 toUnityPos(Vector3 oldVec)
    {
        Vector3 vec = new Vector3();
        vec.x = map(oldVec.x, 0.0f, 1.0f, -5.0f, 5.0f, true);
        vec.y = map(oldVec.y, 0.0f, 1.0f, 0.0f, 5.0f, true);
        vec.z = map(oldVec.z, 0.0f, 1.0f, -5.0f, 5.0f, true);
        return vec;
    }

    static public Vector3 toFrameworkPos(Vector3 oldVec)
    {
        Vector3 vec = new Vector3();
        vec.x = map(oldVec.x, -5.0f, 5.0f, 0.0f, 1.0f, true);
        vec.y = map(oldVec.y, 0.0f, 5.0f, 0.0f, 1.0f, true);
        vec.z = map(oldVec.z, -5.0f, 5.0f, 0.0f, 1.0f, true);
        return vec;
    }
}
