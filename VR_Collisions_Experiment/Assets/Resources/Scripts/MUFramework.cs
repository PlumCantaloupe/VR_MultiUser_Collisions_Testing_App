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

public struct INDEX
{
    public const int X = 0;
    public const int Y = 1;
    public const int Z = 2;
    public const int W = 3;

    public const int R = 0;
    public const int G = 1;
    public const int B = 2;
};

public class MUFramework : MonoBehaviour
{
    public SocketIOController io;
    public GameObject head;
    public GameObject rightHand;
    public GameObject leftHand;
    public List<UserObject> usersArr    = new List<UserObject>();
    public UserObject thisUserObj       = new UserObject();
    public playAreaSize playAreaSize    = new playAreaSize();

    //lets keep refs to avatar pieces
    Color avatar_origCol;
    GameObject avatar_model = null;
    GameObject avatar_hand_L = null;
    GameObject avatar_hand_R = null;

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

		io.On("connect", (SocketIOEvent e) => {
        	Debug.Log("SocketIO connected");
			io.Emit("newUser");
		});

		io.On("user_connect", (SocketIOEvent e) => {
			UserObject userObj = JSONHelper.FromJsonObject<UserObject>(e.data);

            thisUserObj.id = userObj.id;
            thisUserObj.color[INDEX.R] = userObj.color[INDEX.R];
            thisUserObj.color[INDEX.G] = userObj.color[INDEX.G];
            thisUserObj.color[INDEX.B] = userObj.color[INDEX.B];

            //avatar has already been connected for "this" user but want to change color appropriately
            avatar_model     = head.transform.Find("Avatar_Body").gameObject;
            avatar_hand_L    = leftHand.transform.Find("Avatar_Hand_L").gameObject;
            avatar_hand_R    = rightHand.transform.Find("Avatar_Hand_R").gameObject;

            avatar_origCol = new Color(userObj.color[INDEX.R] / 255.0f, userObj.color[INDEX.G] / 255.0f, userObj.color[INDEX.B] / 255.0f);
            setUserCol(avatar_origCol);

            Debug.Log("id received: " + userObj.id + " color: " + avatar_origCol.ToString());
        });

		io.On("usersData", (SocketIOEvent e) => {
            //Debug.Log("usersData");
            usersArr.Clear(); //reset

            //get all users noted in data sent from server
            UserObject[] userObjs = JSONHelper.FromJsonArray<UserObject>(e.data);
            //Debug.Log("userObjs.Length: " + userObjs.Length);
            for (int i = 0; i < userObjs.Length; i++) {
                if ( userObjs[i].id != thisUserObj.id) {
                    usersArr.Add((UserObject)userObjs[i]);
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
        Vector3 headPos = toFrameworkPos(new Vector3(head.transform.position.x, head.transform.position.y, head.transform.position.z));
        thisUserObj.pos_head[INDEX.X] = headPos.x;
        thisUserObj.pos_head[INDEX.Y] = headPos.y;
        thisUserObj.pos_head[INDEX.Z] = headPos.z;
        Vector3 handPos_L = toFrameworkPos(new Vector3(leftHand.transform.position.x, leftHand.transform.position.y, leftHand.transform.position.z));
        thisUserObj.pos_hand_L[INDEX.X] = handPos_L.x;
        thisUserObj.pos_hand_L[INDEX.Y] = handPos_L.y;
        thisUserObj.pos_hand_L[INDEX.Z] = handPos_L.z;
        Vector3 handPos_R = toFrameworkPos(new Vector3(rightHand.transform.position.x, rightHand.transform.position.y, rightHand.transform.position.z));
        thisUserObj.pos_hand_R[INDEX.X] = handPos_R.x;
        thisUserObj.pos_hand_R[INDEX.Y] = handPos_R.y;
        thisUserObj.pos_hand_R[INDEX.Z] = handPos_R.z;

        thisUserObj.rot_head[INDEX.W] = head.transform.rotation.w;
        thisUserObj.rot_head[INDEX.X] = head.transform.rotation.x;
        thisUserObj.rot_head[INDEX.Y] = head.transform.rotation.y;
        thisUserObj.rot_head[INDEX.Z] = head.transform.rotation.z;

        thisUserObj.rot_hand_L[INDEX.W] = leftHand.transform.rotation.w;
        thisUserObj.rot_hand_L[INDEX.X] = leftHand.transform.rotation.x;
        thisUserObj.rot_hand_L[INDEX.Y] = leftHand.transform.rotation.y;
        thisUserObj.rot_hand_L[INDEX.Z] = leftHand.transform.rotation.z;

        thisUserObj.rot_hand_R[INDEX.W] = rightHand.transform.rotation.w;
        thisUserObj.rot_hand_R[INDEX.X] = rightHand.transform.rotation.x;
        thisUserObj.rot_hand_R[INDEX.Y] = rightHand.transform.rotation.y;
        thisUserObj.rot_hand_R[INDEX.Z] = rightHand.transform.rotation.z;

        io.Emit("transformUpdate", JSONHelper.ToJsonObject<UserObject>(thisUserObj));

        //!!TDOD: Just checking NumUsers can lead to bugs as things may change ...

        //
        //now update all "other" avatar positions
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
                //only go ahead if we are not trying to create "this" avatar again
                bool hasBeenInitiatialized = false;

                //first check if there is element exist, and update if so
                foreach (GameObject gameObj in userElemArr) {
                    if (gameObj.GetComponent<Avatar>().getID() == userObj.id) {
                        hasBeenInitiatialized = true;
                        //break;
                    }
                }

                //if not initialized yet add avatar object
                if (!hasBeenInitiatialized) {
                    GameObject gameObj = (GameObject)Instantiate(Resources.Load("prefabs/Avatar"));

                    //find children
                    GameObject model_body = gameObj.transform.Find("Avatar_Body").gameObject;
                    GameObject model_hand_L = gameObj.transform.Find("Avatar_Hand_L").gameObject;
                    GameObject model_hand_R = gameObj.transform.Find("Avatar_Hand_R").gameObject;
                    GameObject boundingBox = gameObj.transform.Find("BoundingBox").gameObject;

                    //initialize
                    gameObj.GetComponent<Avatar>().model_body = model_body;  //set first!
                    gameObj.GetComponent<Avatar>().model_hand_L = model_hand_L;  //set first!
                    gameObj.GetComponent<Avatar>().model_Hand_R = model_hand_R;  //set first!
                    gameObj.GetComponent<Avatar>().boundingBox = boundingBox;   //set first!

                    gameObj.GetComponent<Avatar>().setUserObject(userObj);      //now set this                                                   //break;
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

    public void setUserCol(Color _matCol)
    {
        avatar_model.GetComponent<Renderer>().material.color = _matCol;
        avatar_hand_L.GetComponent<Renderer>().material.color = _matCol;
        avatar_hand_R.GetComponent<Renderer>().material.color = _matCol;
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
        vec.x = map(oldVec.x, 0.0f, 1.0f, -2.5f, 2.5f, true);
        vec.y = map(oldVec.y, 0.0f, 1.0f, 0.0f, 5.0f, true);
        vec.z = map(oldVec.z, 0.0f, 1.0f, -2.5f, 2.5f, true);
        return vec;
    }

    static public Vector3 toFrameworkPos(Vector3 oldVec)
    {
        Vector3 vec = new Vector3();
        vec.x = map(oldVec.x, -2.5f, 2.5f, 0.0f, 1.0f, true);
        vec.y = map(oldVec.y, 0.0f, 5.0f, 0.0f, 1.0f, true);
        vec.z = map(oldVec.z, -2.5f, 2.5f, 0.0f, 1.0f, true);
        return vec;
    }
}
