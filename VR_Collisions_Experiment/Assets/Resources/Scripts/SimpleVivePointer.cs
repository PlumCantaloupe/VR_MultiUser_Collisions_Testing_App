using UnityEngine;
using System.Collections;

public class SimpleVivePointer : MonoBehaviour
{
    public Color color;
    public float thickness = 0.06f;
    public GameObject pointer;
    public LayerMask _layerMask;

    public GameObject _gameManager;

    // Use this for initialization
    void Start()
    {
        pointer = GameObject.CreatePrimitive(PrimitiveType.Cube);
        pointer.transform.parent = this.transform;
        pointer.transform.localScale = new Vector3(thickness, thickness, 100f);
        pointer.transform.localPosition = new Vector3(0f, 0f, 50f);

        Destroy( pointer.GetComponent<BoxCollider>() );

        Material newMat = Resources.Load("Materials/ControllerPointer_Mat") as Material;
        pointer.GetComponent<MeshRenderer>().material = newMat;

        LeanTween.alpha(pointer, 0.2f, 0.0f);
    }

    // Update is called once per frame
    void Update()
    {
        Main main = _gameManager.GetComponent<Main>();
        main.removeSurveyHighlights();

        SteamVR_TrackedController controller = GetComponent<SteamVR_TrackedController>();

        if (controller) {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, controller.transform.forward, out hit, Mathf.Infinity, _layerMask)) {
                //Debug.Log("The ray hit survey button ");
                //Debug.Log( hit.collider.gameObject.name );

                main.interactWithSurveyButton( hit.transform.gameObject, controller.triggerPressed );
            }

            //adjust brightness of "laser"
            if (controller.triggerPressed) {
                LeanTween.alpha(pointer, 1.0f, 0.1f);
            }
            else {
                LeanTween.alpha(pointer, 0.2f, 0.1f);
            }
        }

        //also want to save trigger value here!!
    }
}
