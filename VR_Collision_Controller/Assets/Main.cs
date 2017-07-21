using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;

public class Main : MonoBehaviour
{
    public class HMDPosMessage : MessageBase
    {
        public float hmdPosX;
        public float hmdPosZ;
    };

    int NUM_VIEW_MODES = 3;
    public enum viewMode
    {
        VIEW_MODE_AVATAR,
        VIEW_MODE_BOUNDINGBOX,
        VIEW_MODE_CAMERA
    }

    int NUM_PILLARS = 12;
    public enum pillarType
    {
        PILLAR_0,
        PILLAR_1,
        PILLAR_2,
        PILLAR_3,
        PILLAR_4,
        PILLAR_5,
        PILLAR_6,
        PILLAR_7,
        PILLAR_8,
        PILLAR_9,
        PILLAR_10,
        PILLAR_11
    };

    int NUM_OTHER_POSITIONS = 5;
    public enum userPosition
    {
        otherPos_A,
        otherPos_B,
        otherPos_C,
        otherPos_D,
        otherPos_E
    };

    //message types (no enum for uint_8t(short) values)
    const short MESSAGE_TYPE_SHOW_PILLAR    = 100;
    const short MESSAGE_TYPE_HIDE_PILLAR    = 101;
    const short MESSAGE_TYPE_SURVEY         = 102;
    const short NUM_MESSAGE_TYPES           = 103;
    const short MESSAGE_TYPE_MOVE_OTHERUSER = 104;
    const short MESSAGE_TYPE_SET_VIEWMODE   = 105;
    const short MESSAGE_TYPE_NEW_EXPERIMENT = 106;
    const short MESSAGE_TYPE_END_EXPERIMENT = 107;
    const short MESSAGE_TYPE_NEW_TRIAL      = 108;
    const short MESSAGE_TYPE_END_TRIAL      = 109;
    const short MESSAGE_TYPE_DEBUG_TOGGLE   = 110;
    const short MESSAGE_TYPE_HMD_POS        = 111;

    const string TEXT_START_TRIAL = "New Trial";
    const string TEXT_TRIAL_INPROGRESS = "trial in Progress ...";

    const string TEXT_DEBUG_ON = "debug ON";
    const string TEXT_DEBUG_OFF = "debug OFF";

    public Color m_button_Pillar_OffHighlight 		= new Color(1.0f, 1.0f, 1.0f, 1.0f);
	public Color m_button_Pillar_OnHighlight 		= new Color(1.0f, 1.0f, 1.0f, 1.0f);
    public Color m_button_Pillar_CollideHighlight   = new Color(1.0f, 1.0f, 1.0f, 1.0f);

	public Color m_button_OtherUser_OffHighlight 	= new Color(1.0f, 1.0f, 1.0f, 1.0f);
	public Color m_button_OtherUser_OnHighlight 	= new Color(1.0f, 1.0f, 1.0f, 1.0f);

	public Color m_button_Mode_OffHighlight 		= new Color(1.0f, 1.0f, 1.0f, 1.0f);
	public Color m_button_Mode_OnHighlight 			= new Color(1.0f, 1.0f, 1.0f, 1.0f);

	int m_pillar_Showing                            = -1;
	public List<GameObject> m_pillarButtons         = new List<GameObject>();
	public List<GameObject> m_otherUserPosButtons   = new List<GameObject>();
	public List<GameObject> m_modeButtons           = new List<GameObject>();

    bool m_debugOn = false;
    public GameObject m_newExperiment_Button;
    public GameObject m_endExperiment_Button;
    public GameObject m_newTrial_Button;
    public GameObject m_newTrial_Button_TEXT;
    public GameObject m_debug_Button;
    public GameObject m_debug_Button_Text;

    public GameObject m_avatar;
    public Vector3 m_avatarAdjustment_Scale;
    public Vector3 m_avatarAdjustment_Position;
    float m_halfScreen_Width     = 0.0f;
    float m_halfScreen_Height   = 0.0f;

	void Start()
	{
		NetworkManager.singleton.StartClient();

        //server
        NetworkManager.singleton.client.RegisterHandler(MESSAGE_TYPE_SURVEY, surveyMessage_Receive);                                //survey
        NetworkManager.singleton.client.RegisterHandler(MESSAGE_TYPE_END_TRIAL, endTrialMessage_Receive); 
        NetworkManager.singleton.client.RegisterHandler(MESSAGE_TYPE_HMD_POS, hmdPosMessage_Receive); 
        NetworkManager.singleton.client.RegisterHandler(MsgType.Connect, OnConnected);
        NetworkManager.singleton.client.RegisterHandler(MsgType.Disconnect, OnDisconnected);
        NetworkManager.singleton.client.RegisterHandler(MsgType.Error, OnError);

		//now set pillar showing bools and event listeners
		for ( int i = 0; i < m_pillarButtons.Count; i++ ) {
			Button pButton = m_pillarButtons[i].GetComponent<Button>();
			pillarType tempInt = (pillarType)i;
			pButton.onClick.AddListener( () => togglePillar(tempInt)  );
		}

		//now attach listeners for movement buttons
		for (int i = 0; i < m_otherUserPosButtons.Count; i++) {
			Button moveBut = m_otherUserPosButtons[i].GetComponent<Button>();
			userPosition tempInt = (userPosition)i;
			moveBut.onClick.AddListener( () => moveOtherUser(tempInt) );
		}

        //now attach listeners for movement buttons
        for (int i = 0; i < m_modeButtons.Count; i++)
        {
            Button modeBut = m_modeButtons[i].GetComponent<Button>();
            viewMode tempInt = (viewMode)i;
            modeBut.onClick.AddListener(() => setMode(tempInt));
        }

        //new trial button
        Button newTrialBut = m_newTrial_Button.GetComponent<Button>();
        newTrialBut.onClick.AddListener(() => newTrialMessage_Send());

        //new experiment button
        Button newExpBut = m_newExperiment_Button.GetComponent<Button>();
        newExpBut.onClick.AddListener(() => newExperimentMessage_Send());

        //new experiment button
        Button stopExpBut = m_endExperiment_Button.GetComponent<Button>();
        stopExpBut.onClick.AddListener(() => stopExperimentMessage_Send());

        //new debug button
        Button debugBut = m_debug_Button.GetComponent<Button>();
        debugBut.onClick.AddListener(() => toggleDebug());

        m_halfScreen_Width = Screen.width/2.0f;
        m_halfScreen_Height = Screen.height/2.0f;
    }

	//send message methods
    void newExperimentMessage_Send()
    {
        IntegerMessage intMess = new IntegerMessage();
		intMess.value = 0;
        NetworkManager.singleton.client.Send(MESSAGE_TYPE_NEW_EXPERIMENT, intMess);
    }

    void stopExperimentMessage_Send()
    {
        IntegerMessage intMess = new IntegerMessage();
		intMess.value = 0;
        NetworkManager.singleton.client.Send(MESSAGE_TYPE_END_EXPERIMENT, intMess);
    }

    void newTrialMessage_Send()
    {
        IntegerMessage intMess = new IntegerMessage();
		intMess.value = 0;
        NetworkManager.singleton.client.Send(MESSAGE_TYPE_NEW_TRIAL, intMess);

        m_newTrial_Button_TEXT.GetComponent<Text>().text = TEXT_TRIAL_INPROGRESS;
        m_newTrial_Button.GetComponent<Button>().interactable = false;
    }

	void showPillarMessage_Send( pillarType _pillarType )
	{
		//server
		IntegerMessage intMess = new IntegerMessage();
		intMess.value = (int)_pillarType;
        NetworkManager.singleton.client.Send(MESSAGE_TYPE_SHOW_PILLAR, intMess);
	}

	void hidePillarMessage_Send( pillarType _pillarType )
	{
		//server
		IntegerMessage intMess = new IntegerMessage();
		intMess.value = (int)_pillarType;
        NetworkManager.singleton.client.Send(MESSAGE_TYPE_HIDE_PILLAR, intMess);
	}

	void moveOtherUserMessage_Send( userPosition _userPos )
	{
		//server
		IntegerMessage intMess = new IntegerMessage();
		intMess.value = (int)_userPos;
        NetworkManager.singleton.client.Send(MESSAGE_TYPE_MOVE_OTHERUSER, intMess);
	}

    void setViewModeMessage_Send(viewMode _viewMode)
    {
        //server
        IntegerMessage intMess = new IntegerMessage();
        intMess.value = (int)_viewMode;
        NetworkManager.singleton.client.Send(MESSAGE_TYPE_SET_VIEWMODE, intMess);
    }

    void toggleDebugMessage_Send() {
        IntegerMessage intMess = new IntegerMessage();
        intMess.value = (m_debugOn)? 1 : 0 ;
        NetworkManager.singleton.client.Send(MESSAGE_TYPE_DEBUG_TOGGLE, intMess);
    }

    void surveyMessage_Receive(NetworkMessage message)
  	{
		//server
    	int recMess = message.ReadMessage<IntegerMessage> ().value;
    	Debug.Log("Survey Answer Message Received: " + recMess);
  	}

	void endTrialMessage_Receive(NetworkMessage message)
  	{
		//server
    	int recMess = message.ReadMessage<IntegerMessage> ().value;
    	Debug.Log("End Trial: " + recMess);

        m_newTrial_Button_TEXT.GetComponent<Text>().text = TEXT_START_TRIAL;
        m_newTrial_Button.GetComponent<Button>().interactable = true;
  	}

    void hmdPosMessage_Receive(NetworkMessage message)
  	{
		//server
        HMDPosMessage hmdMess = message.ReadMessage<HMDPosMessage>();
        float posX = hmdMess.hmdPosX;
        float posZ = hmdMess.hmdPosZ;

        Vector3 newPos = new Vector3( (posX * m_avatarAdjustment_Scale.x) + m_avatarAdjustment_Position.x, (posZ * m_avatarAdjustment_Scale.y) + m_avatarAdjustment_Position.y, m_avatar.transform.position.z );
        m_avatar.transform.position = newPos;

        //Debug.Log( "Avatar Pos: " +  m_avatar.transform.position );

        // foreach ( GameObject buttonGO in m_pillarButtons ) {
        //     buttonGO.GetComponent<Button>().image.color = m_button_Pillar_OffHighlight;
        // }

        // //set collide highlight
        // m_pillarButtons[recMess].GetComponent<Button>().image.color = m_button_Pillar_CollideHighlight;
        
        // //set target highlight if any
        // if ( m_pillar_Showing > -1 ) {
        //     m_pillarButtons[m_pillar_Showing].GetComponent<Button>().image.color = m_button_Pillar_OnHighlight;
        // }
  	}

    //lifecycle functionality

    public void OnConnected(NetworkMessage netMsg)
    {
        Debug.Log("Client Connected");
    }

	public void OnDisconnected(NetworkMessage netMsg)
    {
        Debug.Log("Client Disconnected");
    }

	void OnError(NetworkMessage netMsg)
    {
        var errorMsg = netMsg.ReadMessage<ErrorMessage>();
        Debug.Log("Client Error:" + errorMsg.errorCode);
    }

	//button toggle
	public void togglePillar( pillarType _pillarType )
	{
		int intVal = (int)_pillarType;

        if ( m_pillar_Showing == intVal ) {
            //if the same then we are hiding currently showing button
            m_pillar_Showing = -1;
            hidePillarMessage_Send( _pillarType );
			m_pillarButtons[intVal].GetComponent<Button>().image.color = m_button_Pillar_OffHighlight;
        }
        else {
            //show new pillar (hide previous)
            if ( m_pillar_Showing > -1 ) {
                //hide previous
                hidePillarMessage_Send( (pillarType)m_pillar_Showing );
			    m_pillarButtons[m_pillar_Showing].GetComponent<Button>().image.color = m_button_Pillar_OffHighlight;
            }

            //now show new
            m_pillar_Showing = intVal;
            showPillarMessage_Send( _pillarType );
		    m_pillarButtons[intVal].GetComponent<Button>().image.color = m_button_Pillar_OnHighlight;
        }
	}

    //button toggle
	public void toggleDebug()
	{
		m_debugOn = !m_debugOn;

        if ( m_debugOn ) {
            m_debug_Button_Text.GetComponent<Text>().text = TEXT_DEBUG_ON;
        }
        else {
            m_debug_Button_Text.GetComponent<Text>().text = TEXT_DEBUG_OFF;
        }

        toggleDebugMessage_Send();
	}

	//button toggle
	public void moveOtherUser( userPosition _userPos )
	{
		int intVal = (int)_userPos;

		//loop through all buttons and clear to normal then highlight the one clicked
		foreach ( GameObject moveBut in m_otherUserPosButtons ) {
			moveBut.GetComponent<Button>().image.color = m_button_OtherUser_OffHighlight;
		}
		m_otherUserPosButtons[intVal].GetComponent<Button>().image.color = m_button_OtherUser_OnHighlight;	

		//now send message to client
		moveOtherUserMessage_Send( _userPos );
	}

    public void setMode(viewMode _viewMode)
    {
        int intVal = (int)_viewMode;

        //loop through all buttons and clear to normal then highlight the one clicked
        foreach (GameObject modeBut in m_modeButtons)
        {
            modeBut.GetComponent<Button>().image.color = m_button_Mode_OffHighlight;
        }
        m_modeButtons[intVal].GetComponent<Button>().image.color = m_button_Mode_OnHighlight;

        //now send message to client
        setViewModeMessage_Send(_viewMode);
    }
}
