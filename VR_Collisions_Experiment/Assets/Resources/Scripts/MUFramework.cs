using UnityEngine;
using System.Collections.Generic;
using UnitySocketIO;
using UnitySocketIO.Events;

public class MUFramework : MonoBehaviour 
{
	public SocketIOController io;
	List<UserObject> usersArr = new List<UserObject>();

	void Awake()
	{
		Application.runInBackground = true;
	}

	// Use this for initialization
	void Start () 
	{
        string userID = "";

		io.On("connect", (SocketIOEvent e) => {
        	Debug.Log("SocketIO connected");
			io.Emit("newUser");
		});

		io.On("user_connect", (SocketIOEvent e) => {
			UserObject userObj = JSONHelper.FromJsonObject<UserObject>(e.data);
			if ( userID.Equals("") ) {
				userID = userObj.id;
				Debug.Log( "id received: " + userID );
			}
		});

		io.On("usersData", (SocketIOEvent e) => {
			usersArr.Clear(); //reset

			//get all users noted in data sent from server
			UserObject[] userObjs = JSONHelper.FromJsonArray<UserObject>(e.data);
            for (int i = 0; i < userObjs.Length; i++) {
                usersArr.Add( (UserObject)userObjs[i]);
            }
		});
		
		io.Connect();
	}
	
	// Update is called once per frame
	void Update () 
    {
		GameObject[] userElemArr = GameObject.FindGameObjectsWithTag("Player");

		// Update the state of the world for the elapsed time since last render
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
                    Destroy( gameObj );
                    //break;
                }
            }

		}
		else if (userElemArr.Length < usersArr.Count) {
			//!!someone has connected
            Debug.Log("someone connected");
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
                    gameObj.GetComponent<Avatar>().setUserObject( userObj );
                    //break;
                }
            }
		}
		else {
			//update all positions
            Debug.Log("update positions");
			foreach (GameObject gameObj in userElemArr) {
				foreach (UserObject userObj in usersArr ) {
                    if ( gameObj.GetComponent<Avatar>().getID() == userObj.id ) {
                        gameObj.GetComponent<Avatar>().setUserObject( userObj );
                    }
				}
			}
		}
	}
}
