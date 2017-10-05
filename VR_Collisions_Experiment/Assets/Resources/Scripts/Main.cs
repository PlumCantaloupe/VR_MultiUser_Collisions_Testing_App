using UnityEngine;
using System.Collections.Generic;
using System.IO;
using UnitySocketIO;
using UnitySocketIO.Events;

public class Main : MonoBehaviour
{
	//
	static public float OTHERUSER_MOVE_TIME                     = 3.0f;
    static public float COLLISION_OTHERUSER_EFFECT_RADIUS_SQR   = 0.547f;
    static public float PILLARCOLLISION_EFFECT_RADIUS_SQR       = 0.1f;
    static public float BOUNDINGBOX_EFFECT_RADIUS_SQR           = 1.5f;
    static public float BOUNDINGBOX_ALPHA__EASE_TIME            = 0.3f;
    static public float DATA_TIME_INTERVAL_S                    = 0.3f;

    int NUM_VIEW_MODES = 3;
    public enum viewMode {
        VIEW_MODE_AVATAR,
        VIEW_MODE_BOUNDINGBOX,
        VIEW_MODE_CAMERA,
        VIEW_MODE_INTERSTITIAL,
        VIEW_MODE_SURVEY
    }

    int NUM_PILLARS = 12;
    public enum pillarType {
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

    int NUM_OTHER_POSITIONS = 6;
    public enum userPosition {
		otherPos_A,
		otherPos_B,
		otherPos_C,
		otherPos_D,
		otherPos_E,
        otherPos_NONE
	};

    int NUM_SURVEY_SELECTIONS = 7;
    public enum surveySelection
    {
        SURVEY_SELECT_1 = 1,
        SURVEY_SELECT_2 = 2,
        SURVEY_SELECT_3 = 3,
        SURVEY_SELECT_4 = 4,
        SURVEY_SELECT_5 = 5,
        SURVEY_SELECT_6 = 6,
        SURVEY_SELECT_7 = 7,
    };

    public viewMode m_viewMode = viewMode.VIEW_MODE_INTERSTITIAL;
    public viewMode m_viewModePrev = viewMode.VIEW_MODE_INTERSTITIAL;
    public viewMode m_futureViewMode = viewMode.VIEW_MODE_INTERSTITIAL;
    public userPosition m_userPosition = userPosition.otherPos_E;
    public GameObject m_HMDCam;
	public GameObject m_otherUser;
    public GameObject m_otherUser_Avatar;
    public GameObject m_otherUser_BoundingBox;
    public GameObject m_Room;
    public GameObject m_Room_Interstitial;
    public GameObject m_ViveCameraQuad;
    public GameObject m_viveController;
    public GameObject m_survey;
    public GameObject m_submitSurvery_Button;
    public GameObject m_debugElements;
    public List<GameObject> m_pillars           = new List<GameObject>();
    public List<GameObject> m_surveyButtons     = new List<GameObject>();
    public List<Vector3> m_otherPositions       = new List<Vector3>() { };

    public int m_chosenSurveyButton             = -1;

    public Color m_Pillar_OffHighlight          = new Color(1.0f, 1.0f, 1.0f, 1.0f);
    public Color m_Pillar_OnHighlight           = new Color(1.0f, 1.0f, 1.0f, 1.0f);
    public Color m_Pillar_Collision_Highlight   = new Color(1.0f, 1.0f, 1.0f, 1.0f);

    public Color m_surveyButton_ON_Col;
    public Color m_surveyButton_OFF_Col;
    public Color m_surveyButton_Highlight_Col;

    //visible debug
    public GameObject m_selectedPillar;
    public GameObject m_collidedPillar;
    public bool m_trialInSession        = false;
    public bool m_experimentInSession   = false;

    public float m_startMoveTime        = 0.0f;
    public float m_endMoveTime          = 0.0f;
    public float m_prevTime             = 0.0f;

    bool m_prev_withinBB_AOE        = false;
    bool m_prev_Colliding           = false;

    //data logging
    int m_trialCounter              = 0;
    int m_trialType                 = -1;
    int m_fromPillar                = -1;
    int m_ToPillar                  = -1;
    int m_numCollisions             = 0;
    List<float> m_trackedTime       = new List<float>();
    List<float> m_trackedAnxiety    = new List<float>();
    List<float> m_trackedPos_X      = new List<float>();
    List<float> m_trackedPos_Z      = new List<float>();
    string m_experimentID;

    public Vector3 HELLO_TEST = new Vector3(0.0f, 0.0f, 0.0f);

    void awake()
	{
        Application.runInBackground = true;
        SteamVR_Render.instance.pauseGameWhenDashboardIsVisible = false; //!! this didn't work so changed in SteamVR source. Bug was stoppping time when CameraOverlay was up ...
    }

    // init
    void Start()
    {
        //avatar default
        setMode(viewMode.VIEW_MODE_INTERSTITIAL);
        setMode_Future(viewMode.VIEW_MODE_AVATAR);
    }

    void Update()
    {
        //check where HMD is (don't care about y-value)
        Vector3 hmdPos = HELLO_TEST;//m_HMDCam.GetComponent<Transform>().position;
        hmdPos.y = 0.0f; //no need for y-axis as taller/shoter people may trigger differently

        checkforPillarCollision(hmdPos);
        adjustBoundingBoxTransparency(hmdPos);
        checkForCollisions(hmdPos);
    }
    
    public void startExperiment()
    {
        m_trialCounter = 0;
        m_experimentID = System.DateTime.Now.ToString("yyyy_MM_dd-hh_mm");
        m_experimentInSession = true;
    }

    //not really necessary but have it in for the future
    public void stopExperiment()
    {
        m_experimentInSession = false;
    }

    public void startTrial()
    {
        if ( !m_experimentInSession ) {
            startExperiment();
        }

        //reset everything possible to current conditions
        m_trialCounter++;

        m_trialType             = (int)m_futureViewMode;
        m_fromPillar            = m_collidedPillar.GetComponent<Pillar>().m_id;
        m_ToPillar              = m_selectedPillar.GetComponent<Pillar>().m_id;
        m_chosenSurveyButton    = -1;
        m_numCollisions         = 0;
        m_trackedAnxiety.Clear();
        m_trackedTime.Clear();
        m_trackedPos_X.Clear();
        m_trackedPos_Z.Clear();

        m_collidedPillar = null;

        float currTime      = Time.time;
        m_startMoveTime     = currTime;
        m_prevTime          = 0.0f;
        m_endMoveTime       = 0.0f;
        m_prev_withinBB_AOE = true; //this needs to be true so that it is not flipped immediately
        m_prev_Colliding    = true; //this needs to be true so that it is not flipped immediately
        m_trialInSession    = true;

        setMode(m_futureViewMode);
    }

    public void stopTrial()
    {
        //log all data to a text file as a .
        string path = @"D:\Dropbox\Temp_Workspace\VR_Experimental_Data__" + m_experimentID + "__" + m_trialCounter + "-" + m_viewModePrev + "-" + m_userPosition + ".csv";
        if (!File.Exists(path)) {
            // Create a .csv file to write to  (used custom function for list types)
            using (StreamWriter sw = File.CreateText(path)) 
            {
                sw.WriteLine("Experiment ID,"   + m_experimentID);
                sw.WriteLine("Trial,"           + m_trialCounter);
                sw.WriteLine("View Type,"       + m_viewModePrev);
                sw.WriteLine("Collision Type,"  + m_userPosition);
                sw.WriteLine("From Pillar,"     + m_fromPillar);
                sw.WriteLine("To Pillar,"       + m_ToPillar);
                sw.WriteLine("Survey Response," + m_chosenSurveyButton);
                sw.WriteLine("Movement Time,"   + (m_endMoveTime - m_startMoveTime).ToString());
                sw.WriteLine("Num Collisions,"  + m_numCollisions);

                //now list out all tracked data as per const interval
                sw.WriteLine("Tracked Time"         + convertListToString(m_trackedTime));
                sw.WriteLine("Tracked Positions X"  + convertListToString(m_trackedPos_X));
                sw.WriteLine("Tracked Positions Z"  + convertListToString(m_trackedPos_Z));
                sw.WriteLine("Tracked Anxiety"      + convertListToString(m_trackedAnxiety));
            }
        }

        //GetComponent<Networking>().endTrialMessage_Send();
        setMode( viewMode.VIEW_MODE_INTERSTITIAL);
    }

    string convertListToString( List<float> _floatList)
    {
        string listStr = "";
        foreach ( float val in _floatList ) {
            listStr += "," + val.ToString();
        }
        return listStr;
    }

    void checkforPillarCollision(Vector3 _hmdPos)
    {
        foreach ( GameObject pillar in m_pillars ) {
            Vector3 pillarPos =pillar.GetComponent<Transform>().position;
            pillarPos.y = 0.0f;
            Vector3 diffVec = _hmdPos - pillarPos;

            if (diffVec.sqrMagnitude < PILLARCOLLISION_EFFECT_RADIUS_SQR) {
                //user is on top of pillar
                if (pillar == m_selectedPillar && m_trialInSession) {
                    showSurvey();
                } else {
                    pillar.GetComponent<Renderer>().material.SetColor("_Color", m_Pillar_Collision_Highlight);
                    m_collidedPillar = pillar;
                }

                break; //no need to check the rest as we can only be in one place at a time ;)
            }
            else {
                if (pillar == m_selectedPillar) {
                    pillar.GetComponent<Renderer>().material.SetColor("_Color", m_Pillar_OnHighlight);
                }
                else {
                    pillar.GetComponent<Renderer>().material.SetColor("_Color", m_Pillar_OffHighlight);
                }
            }
        }
    }

    void adjustBoundingBoxTransparency(Vector3 _hmdPos)
    {
        if (m_viewMode == viewMode.VIEW_MODE_BOUNDINGBOX) {
            Vector3 avatarPos = m_otherUser.GetComponent<Transform>().position;
            avatarPos.y = 0.0f;
            Vector3 diffVec = _hmdPos - avatarPos;
            float sqrMag = diffVec.sqrMagnitude;

            //float newAlpha = map(sqrMag, 0.0f, BOUNDINGBOX_EFFECT_RADIUS_SQR, 1.0f, 0.0f);
            //newAlpha = Mathf.Clamp(newAlpha, 0.0f, 1.0f);
            //Color col = m_otherUser_BoundingBox.GetComponent<Renderer>().material.GetColor("_Color");
            //col.a = newAlpha;
            //m_otherUser_BoundingBox.GetComponent<Renderer>().material.SetColor("_Color", col);

            if (sqrMag < BOUNDINGBOX_EFFECT_RADIUS_SQR) {
                if (!m_prev_withinBB_AOE) {
                    LeanTween.alpha(m_otherUser_BoundingBox, 1.0f, BOUNDINGBOX_ALPHA__EASE_TIME).setEase(LeanTweenType.easeInOutQuad);
                    m_prev_withinBB_AOE = true;
                }
            }
            else {
                if (m_prev_withinBB_AOE) {
                    LeanTween.alpha(m_otherUser_BoundingBox, 0.0f, BOUNDINGBOX_ALPHA__EASE_TIME).setEase(LeanTweenType.easeInOutQuad);
                    m_prev_withinBB_AOE = false;
                }
            }
        }
        //else {
        //    Color col = m_otherUser_BoundingBox.GetComponent<Renderer>().material.GetColor("_Color");
        //    col.a = 0.0f;
        //    m_otherUser_BoundingBox.GetComponent<Renderer>().material.SetColor("_Color", col);
        //}
    }

    //void showGameObject( GameObject _gameObject, bool _show )
    //{
    //    Color col = _gameObject.GetComponent<Renderer>().material.GetColor("_Color");
    //    col.a = (_show) ? 1.0f : 0.0f;
    //    _gameObject.GetComponent<Renderer>().material.SetColor("_Color", col);
    //}

    //mapping function from Processing
    float map(float value, float istart, float istop, float ostart, float ostop)
    {
        return ostart + (ostop - ostart) * ((value - istart) / (istop - istart));
    }

    void checkForCollisions(Vector3 _hmdPos)
    {
        if ((m_viewMode != viewMode.VIEW_MODE_INTERSTITIAL) || (m_viewMode != viewMode.VIEW_MODE_SURVEY) || (m_userPosition != userPosition.otherPos_NONE)) {
            //m_prev_Colliding
            Vector3 diffVec = _hmdPos - m_otherUser.transform.position;
            if (diffVec.sqrMagnitude < COLLISION_OTHERUSER_EFFECT_RADIUS_SQR) {
                if (!m_prev_Colliding) {
                    m_numCollisions++;
                    m_prev_Colliding = true;

                    m_otherUser_Avatar.GetComponent<Renderer>().material.SetColor("_Color", m_Pillar_OffHighlight);
                    m_otherUser_BoundingBox.GetComponent<Renderer>().material.SetColor("_Color", m_Pillar_OffHighlight);
                }
            }
            else {
                if (m_prev_Colliding) {
                    m_prev_Colliding = false;

                    m_otherUser_Avatar.GetComponent<Renderer>().material.SetColor("_Color", m_Pillar_Collision_Highlight);
                    m_otherUser_BoundingBox.GetComponent<Renderer>().material.SetColor("_Color", m_Pillar_Collision_Highlight);  
                }
            }
        }
    }

    //functions for research
    public void activatePillar( GameObject _pillar, bool _activate )
    {
        m_selectedPillar = (_activate) ? _pillar : null;
        _pillar.GetComponent<Renderer>().material.SetColor("_Color", (_activate) ? m_Pillar_OnHighlight : m_Pillar_OffHighlight);
	}

	public void moveOtherUser( GameObject _otherUser, userPosition _userPos, float _easeTime )
    {
        m_userPosition = _userPos;

        if ( m_userPosition == userPosition.otherPos_NONE) {
            _otherUser.SetActive(false);
        }
        else {
            _otherUser.SetActive(true);
            _otherUser.transform.position = m_otherPositions[(int)_userPos];
            //Vector3 oldPos = _otherUser.GetComponent<Transform>().position;
            //LeanTween.move(_otherUser, m_otherPositions[(int)_userPos], _easeTime).setEase(LeanTweenType.easeInOutQuad);
        }
    }

	public void showSurvey()
	{
        m_endMoveTime = Time.time;
        m_trialInSession = false;
        setMode(viewMode.VIEW_MODE_SURVEY);
	}

    public void submitSurvey()
    {
        stopTrial();

        //GetComponent<Networking>().surveyMessage_Send(m_chosenSurveyButton);
        resetSurvey();
        setMode(viewMode.VIEW_MODE_INTERSTITIAL);
    }

    public void resetSurvey()
    {
        m_chosenSurveyButton = -1;
        removeSurveyHighlights();
    }

    //some code for a custom in-game "form" billboard (always facing user)
    public void interactWithSurveyButton( GameObject _selection, bool _clicked )
    {
        _selection.GetComponent<Renderer>().material.SetColor("_Color", m_surveyButton_Highlight_Col); //highlight

        SurveyButton surveyBut = _selection.GetComponent<SurveyButton>();
        if (surveyBut) {
            if (_clicked) {
                m_chosenSurveyButton = surveyBut.m_id;  //found survey button
                //_selection.GetComponent<Renderer>().material.SetColor("_Color", m_surveyButton_ON_Col);
                Debug.Log("Survey Response Noted: " + m_chosenSurveyButton);
            }
        }
        else {
            //try to see if submit button
            SubmitSurveyButton submitBut = _selection.GetComponent<SubmitSurveyButton>();
            if (submitBut) {
                if (_clicked) {
                    if ( m_chosenSurveyButton > -1 ) {
                        submitSurvey();
                        Debug.Log("Survey Submitted");
                    }   
                }   
            }
        }
    }

    public void removeSurveyHighlights()
    {
        foreach (GameObject surveyBut in m_surveyButtons) {
            surveyBut.GetComponent<Renderer>().material.SetColor("_Color", m_surveyButton_OFF_Col);
            if ( m_chosenSurveyButton > -1 ) {
                if ( surveyBut.GetComponent<SurveyButton>().m_id == m_chosenSurveyButton ) {
                    surveyBut.GetComponent<Renderer>().material.SetColor("_Color", m_surveyButton_ON_Col);
                }
            }
        }
    }

    //want to save the mode view as opposed to lifecycle modes (that may have changed to something irrelevant before we log data)
    public void setMode_Future(viewMode _viewMode)
    {
        m_futureViewMode = _viewMode;
    }

    public void setMode( viewMode _viewMode )
    {
        m_viewMode = _viewMode;

        if ( (_viewMode != viewMode.VIEW_MODE_INTERSTITIAL) && (_viewMode != viewMode.VIEW_MODE_SURVEY)) {
            m_viewModePrev = _viewMode;
        }

        switch ( (int)_viewMode )
        {
            case (int)viewMode.VIEW_MODE_AVATAR:
                {
                    m_otherUser_Avatar.SetActive(true);
                    m_otherUser_BoundingBox.SetActive(false);
                    m_Room.SetActive(true);
                    m_Room_Interstitial.SetActive(false);
                    //m_ViveCameraQuad.SetActive(false);
                    m_survey.SetActive(false);
                } break;
            case (int)viewMode.VIEW_MODE_BOUNDINGBOX:
                {


                    //!!
                    m_otherUser_Avatar.SetActive(false);
                    m_otherUser_BoundingBox.SetActive(true);
                    m_Room.SetActive(true);
                    m_Room_Interstitial.SetActive(false);
                    //m_ViveCameraQuad.SetActive(false);
                    m_survey.SetActive(false);

                    LeanTween.alpha(m_otherUser_BoundingBox, 0.0f, 0.0f);
                } break;
            case (int)viewMode.VIEW_MODE_CAMERA:
                {
                    m_otherUser_Avatar.SetActive(false);
                    m_otherUser_BoundingBox.SetActive(false);
                    m_Room.SetActive(false);
                    m_Room_Interstitial.SetActive(false);
                    //m_ViveCameraQuad.SetActive(true);
                    m_survey.SetActive(false);
                } break;
            case (int)viewMode.VIEW_MODE_INTERSTITIAL: {
                    m_otherUser_Avatar.SetActive(false);
                    m_otherUser_BoundingBox.SetActive(false);
                    m_Room.SetActive(false);
                    m_Room_Interstitial.SetActive(true);
                    //m_ViveCameraQuad.SetActive(false);
                    m_survey.SetActive(false);
                }
                break;
            case (int)viewMode.VIEW_MODE_SURVEY: {
                    m_otherUser_Avatar.SetActive(false);
                    m_otherUser_BoundingBox.SetActive(false);
                    m_Room.SetActive(false);
                    m_Room_Interstitial.SetActive(true);
                    //m_ViveCameraQuad.SetActive(false);
                    m_survey.SetActive(true);
                }
                break;
            default:
                {
                    Debug.Log("WARNING: No valid view mode selected ...");
                } break;

        }
    }
}
