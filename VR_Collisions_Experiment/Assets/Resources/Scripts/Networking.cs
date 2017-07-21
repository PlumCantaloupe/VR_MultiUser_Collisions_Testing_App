using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;

public class Networking : MonoBehaviour
{
    //custom message type to send to remote controller
    public class HMDPosMessage : MessageBase
    {
        public float hmdPosX;
        public float hmdPosZ;
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

    void Start()
    {
        NetworkManager.singleton.StartServer();

        //client
        NetworkServer.RegisterHandler(MESSAGE_TYPE_SHOW_PILLAR, showPillarMessage_Receive);   //show pillar	
        NetworkServer.RegisterHandler(MESSAGE_TYPE_HIDE_PILLAR, hidePillarMessage_Receive);   //hide pillar
        NetworkServer.RegisterHandler(MESSAGE_TYPE_MOVE_OTHERUSER, moveOtherUser_Receive);    //move user
        NetworkServer.RegisterHandler(MESSAGE_TYPE_SET_VIEWMODE, setViewModeMessage_Receive); //set viewmode

        NetworkServer.RegisterHandler(MESSAGE_TYPE_NEW_EXPERIMENT, newExperimentMessage_Receive);
        NetworkServer.RegisterHandler(MESSAGE_TYPE_END_EXPERIMENT, endExperimentMessage_Receive);
        NetworkServer.RegisterHandler(MESSAGE_TYPE_NEW_TRIAL, newTrialMessage_Receive);

        NetworkServer.RegisterHandler(MESSAGE_TYPE_DEBUG_TOGGLE, debugToggleMesssage_Receive);

        NetworkServer.RegisterHandler(MsgType.Connect, OnConnected);
        NetworkServer.RegisterHandler(MsgType.Disconnect, OnDisconnected);
        NetworkServer.RegisterHandler(MsgType.Error, OnError);
    }

    public void surveyMessage_Send(int _surveyAnswer)
    {
        //client
        IntegerMessage intMess = new IntegerMessage();
        intMess.value = _surveyAnswer;
        NetworkServer.SendToAll(MESSAGE_TYPE_SURVEY, intMess);
    }

    public void endTrialMessage_Send()
    {
        //client
        IntegerMessage intMess = new IntegerMessage();
        intMess.value = 0;
        NetworkServer.SendToAll(MESSAGE_TYPE_END_TRIAL, intMess);
    }

    public void sendHMDPos_Send( float _posX, float _posZ )
    {
        //client
        HMDPosMessage posMess = new HMDPosMessage();
        posMess.hmdPosX = _posX;
        posMess.hmdPosZ = _posZ;
        NetworkServer.SendToAll(MESSAGE_TYPE_HMD_POS, posMess);
    }

    //receive message methods
    void showPillarMessage_Receive(NetworkMessage message)
    {
        int recMess = message.ReadMessage<IntegerMessage>().value;
        Debug.Log("Pillar Message Show Received: " + recMess);

        Main _main = GetComponent<Main>();
        _main.activatePillar(_main.m_pillars[recMess], true);
    }

    void hidePillarMessage_Receive(NetworkMessage message)
    {
        int recMess = message.ReadMessage<IntegerMessage>().value;
        Debug.Log("Pillar Message Hide Received: " + recMess);

        Main _main = GetComponent<Main>();
        _main.activatePillar(_main.m_pillars[recMess], false);
    }

    void moveOtherUser_Receive(NetworkMessage message)
    {
        int recMess = message.ReadMessage<IntegerMessage>().value;
        Debug.Log("Move Other User Message Received: " + recMess);

        Main _main = GetComponent<Main>();
        _main.moveOtherUser(_main.m_otherUser, (Main.userPosition)recMess, Main.OTHERUSER_MOVE_TIME);
    }

    void setViewModeMessage_Receive(NetworkMessage message)
    {
        int recMess = message.ReadMessage<IntegerMessage>().value;
        Debug.Log("Set Viewmode Message Received: " + recMess);

        Main _main = GetComponent<Main>();
        if(_main.m_trialInSession) {
            _main.setMode((Main.viewMode)recMess); //set immediately when in session for debugging
        }
        else {
            _main.setMode_Future((Main.viewMode)recMess); //don't want user to see what is being changed uitil they start trial
        }
        
    }

    void newExperimentMessage_Receive(NetworkMessage message)
    {
        int recMess = message.ReadMessage<IntegerMessage>().value;
        Debug.Log("New Experiment Message Received: " + recMess);

        Main _main = GetComponent<Main>();
        _main.startExperiment();
    }

    void endExperimentMessage_Receive(NetworkMessage message)
    {
        int recMess = message.ReadMessage<IntegerMessage>().value;
        Debug.Log("End Experiment Message Received: " + recMess);

        Main _main = GetComponent<Main>();
        _main.stopExperiment();
    }

    void newTrialMessage_Receive(NetworkMessage message)
    {
        int recMess = message.ReadMessage<IntegerMessage>().value;
        Debug.Log("New Trial Message Received: " + recMess);

        Main _main = GetComponent<Main>();
        _main.startTrial();
    }

    void debugToggleMesssage_Receive(NetworkMessage message)
    {
        int recMess = message.ReadMessage<IntegerMessage>().value;
        Debug.Log("Debug Toggle Message Received: " + recMess);

        Main _main = GetComponent<Main>();
        if (recMess == 1) {
            _main.m_debugElements.SetActive(true);
        }
        else if (recMess == 0) {
            _main.m_debugElements.SetActive(false);
        }
    }

    //client
    public void OnConnected(NetworkMessage netMsg)
    {
        Debug.Log("Server Connected");
    }

    public void OnDisconnected(NetworkMessage netMsg)
    {
        Debug.Log("Server Disconnected");
    }

    void OnError(NetworkMessage netMsg)
    {
        var errorMsg = netMsg.ReadMessage<ErrorMessage>();
        Debug.Log("Server Error:" + errorMsg.errorCode);
    }
}