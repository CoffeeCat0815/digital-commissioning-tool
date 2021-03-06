﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ApplicationFacade;


public class PickAndPlaceNew : MonoBehaviour
{
    public GameObject selected;                 //= ausgewähltes Regal
    public bool isDragging;                     //= true wenn ein Regal ausgewählt
    float lastPosX;                             //= letzte MausPosition (X)
    float lastPosZ;                             //= letzte MausPosition (Z)
    public LayerMask m_LayerMask, mask;
    bool moveX, moveZ;                          //=true wenn jeweilige Achse ausgewählt     
    public int collision;                       //Anzahl der Kollisionen
    public bool onObject;                       //=true wenn auf einem Objekt
    public Material material1, material2;       //material bei Kollision ändern (material1 = ok(blau), material2 = Kollision(rot))
    Renderer rend;
    Transform invisibleWall;                    //zum färben (Kindelement von Regal: "Überzug" ohne Kollider)            
    int rotation;                               //gesamt Rotation       
    int rotationRight;                          //Rotation im Uhrzeigersinn
    int rotationLeft;                           //Rotation gen den Uhrzeigersinn


    // Start is called before the first frame update
    void Start()
    {
        //Beim starten alles auf Anfangswerte setzen: 
        isDragging = false;
        lastPosX = 0f;
        lastPosZ = 0f;
        invisibleWall = null;
        onObject = false;
        selected = null;
        rotation = 0;
        rotationRight = 0;
        rotationLeft = 0;
        SelectionManager.StorageSelected += OnStorageSelected;
    }

    //Auswahl Regal :
    private void OnStorageSelected(Transform storage)
    {
        selected = storage.gameObject;
        isDragging = true;

    }  
    
    //Gibt zurück ob Objekt getroffen:
    private bool HitSomething()
    {
        BoxCollider c = selected.GetComponent<BoxCollider>();
        Collider[] hitColliders = Physics.OverlapBox(invisibleWall.position, invisibleWall.localScale / 2, Quaternion.identity, m_LayerMask);
        bool collisonDetected = false;
        foreach (Collider collider in hitColliders)
        {
            if ((collider.gameObject != selected && collider.transform.parent != selected.transform && collider.transform.parent.parent != selected.transform && !collider.CompareTag("SelectableFloor")))         //keine Kollision mit sich selber oder dem Boden 
            {
                collisonDetected = true;
            }
        }
        return collisonDetected;
    }

    //Übergebenes Objekt rotieren
    private void Rotate(GameObject selected)
    {

        //rotation im Uhrzeigersinn (90° um X-Achse)
        if (Input.GetKeyDown(KeyCode.R) || Input.GetKeyDown(KeyCode.E))
        {
            //   Debug.Log("x: " + selected.transform.eulerAngles.x + " y: " + selected.transform.eulerAngles.y + " z : " + selected.transform.eulerAngles.z);

            if (rotationRight < 4)                      //ermitteln wie oft im Uhrzeigersinn rotiert wurde da nur 4 möglich sind => bei 4 = 0
            {
                rotationRight++;
            }

            if (rotationRight == 4)
            {
                rotationRight = 0;
            }
            rotation = rotationRight - rotationLeft;   //Rotation gegenden Uhrzeigersinn abziehen um einen Wert für die gemeinsame rotationzu erhalten
            if (rotation < 0)
            {
                rotation = rotation + 4;               //Um nicht zwichen positiven und negativen Werten unterscheiden zu müssen, wird der gesamtwert mit 4 addiert
            }

            //die einzelnen Rotationsmöglichkeiten:
            if (rotation == 0) { selected.transform.rotation = Quaternion.Euler(0, 0, 0); }
            if (rotation == 1) { selected.transform.rotation = Quaternion.Euler(0, 90, 0); }
            if (rotation == 2) { selected.transform.rotation = Quaternion.Euler(0, 180, 0); }
            if (rotation == 3) { selected.transform.rotation = Quaternion.Euler(0, 270, 0); }

            //Debug.Log("x: " + selected.transform.eulerAngles.x + " y: " + selected.transform.eulerAngles.y + " z : " + selected.transform.eulerAngles.z);
        }
        //Rotation gegen den Uhrzeigersinn (beinahe identisch wie im Uhrzeiger sinn nur die Rotationswerte werden vertauscht)
        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (rotationLeft < 4)
            {
                rotationLeft++;
            }

            if (rotationLeft == 4)
            {
                rotationLeft = 0;
            }
            rotation = rotationRight - rotationLeft;
            if (rotation < 0) { rotation = rotation + 4; }
            if (rotation == 0) { selected.transform.rotation = Quaternion.Euler(0, 0, 0); }
            if (rotation == 1) { selected.transform.rotation = Quaternion.Euler(0, 90, 0); }
            if (rotation == 2) { selected.transform.rotation = Quaternion.Euler(0, 180, 0); }
            if (rotation == 3) { selected.transform.rotation = Quaternion.Euler(0, 270, 0); }
            // selected.transform.rotation = selected.transform.rotation * Quaternion.AngleAxis(-90, Vector3.up);
        }

    }

    //Bewegen mit Maus
    private void MoveAnywhere(GameObject selected)
    {
        //um die Mausposition zu finden
        Ray ray2 = GameObject.FindGameObjectWithTag("EditorModeCamera").GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
        RaycastHit hit2;
        //                  (ray, hit, range, mask)
        if (Physics.Raycast(ray2, out hit2, Mathf.Infinity, mask))
        {
            //erhalte koordinaten x und z durch die Maus Position (y = 0 da nicht nach oben bzw. unten bewegt werden soll)
            float posX = hit2.point.x;
            float posZ = hit2.point.z;

            //aktualisierte maus position (wenn Mauszeiger bewegt wird)
            if (lastPosX != posX || lastPosZ != posZ)
            {
                lastPosX = posX;
                lastPosZ = posZ;

                //Cursor = Regal (Maus position dem Regal übergeben)
                selected.transform.position = new Vector3(posX, 0f, posZ);

                //(Wenn Objekt auf kamera zu fliegen => add layer floor -> boden hinzufügen und am würfel entfernen)
            }
            //Objekt platzieren
            PlaceObject();
        }

    }

    //Regal Platzieren 
    private void PlaceObject()
    {
        if (Input.GetKeyDown("return"))
        {
            if (HitSomething() == false)                         //Platzieren (mit enter taste) nur zulassen wenn das Objekt nicht auf (bzw. sich in) einem anderen Objekt steht
            {
                //Werte wieder auf Anfangswerte setzten:
                moveX = false;
                moveZ = false;
                isDragging = false;
                rotation = 0;
                rotationRight = 0;
                rotationLeft = 0;
                invisibleWall = null;
                selected = null;
            }
            /*else
            {   
                Debug.Log("Cant Place on Object");
            }*/
        }
    }

    //In X-Richtung Bewegen
    private void MoveInXAxis(GameObject selected)
    {

        //da die Achsenbewegung abhängig von der Kamera ist (Editormodus Kamera im Modehandler):
        ModeHandler modeHandler = GameObject.Find("SwitchModeButton").GetComponent<ModeHandler>();
        Vector3 cameraRight = modeHandler.EditorModeCamera.transform.right;                         //camera right vector         
        float x = 0;
        float speed = 2f;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.LeftArrow))
        {
            x = Input.GetAxis("Horizontal") * speed;                                                //neue x position je nachdem welche taste gedrückt 
        }
        else
        {
            x = Input.GetAxis("Mouse X") * speed;                                                   //x wert nach maus position (falls statt taste maus horizontal bewegt wird)
        }
        Vector3 pos = selected.transform.position;                                                  //pos = position des ausgweählten Objekts 

        Vector3 cR = cameraRight;                                                                   //cR = X-Achse (aus Kamera)
        cR.y = 0;
        cR = cameraRight.normalized;
        pos = new Vector3(pos.x, 0, (x * Time.deltaTime));                                          //aktualisiere pos mit neuem wert für pos.z
        pos = cR * pos.z;                                                                           //berechne pos mit camera.transform

        selected.transform.Translate(pos, Space.World);                                             //bewege Regal nach pos, Space.World damit unabhängig von rotation 


        if (Input.GetKeyDown(KeyCode.Y))                                                            //Y Taste wenn Achsen Bewegung gewechselt werden soll
        {
            moveX = false;
            moveZ = true;
        }

        if (Input.GetKeyDown(KeyCode.Z))
        {                                                          //Z Taste wenn wieder mit Maus Bewegt werden soll 
            moveX = false;
            moveZ = false;
        }
        PlaceObject();
    }

    //In Y-Richtung Bewegen
    private void MoveInYAxis(GameObject selected)
    {
        //Ähnlich wie MoveInXAxis nur mit der anderen Achse: 
        ModeHandler modeHandler = GameObject.Find("SwitchModeButton").GetComponent<ModeHandler>();
        Vector3 cameraForward = modeHandler.EditorModeCamera.transform.forward;                          //camera.transform.forward für vertikale position

        float z = 0;
        float speed = 2f;
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.DownArrow))
        {
            z = Input.GetAxis("Vertical") * speed;
        }
        else
        {
            z = Input.GetAxis("Mouse Y") * speed;
        }
        Vector3 pos = selected.transform.position;

        Vector3 cF = cameraForward;
        cF.y = 0;
        cF = cF.normalized;
        pos = new Vector3((z * Time.deltaTime), 0, 0);
        pos = cF * pos.x;
        selected.transform.Translate(pos, Space.World);
        if (Input.GetKeyDown(KeyCode.X))
        {
            moveX = true;
            moveZ = false;
        }
        if (Input.GetKeyDown(KeyCode.Z))
        {
            moveX = false;
            moveZ = false;
        }
        PlaceObject();

    }

    //Übergebenes Regal löschen
    private void DeleteObject(GameObject selected)
    {
        //Debug.Log("Delete " + selected.name);
        GameManager.GameWarehouse.RemoveStorageRack(GameManager.GameWarehouse.GetStorageRack(selected));
        isDragging = false;     //= false da kein Objekt mehr ausgewählt 
    }


    // Update is called once per frame
    void Update()
    {
        // temp = AddStorage.objectNumber;

        if (isDragging)
        {
            invisibleWall = selected.transform.Find("InvisibleWall");               //aus ausgwähltem Regal das Kindelement "invisibleWall" finden
            rend = invisibleWall.GetComponent<Renderer>();                          //renderer um später das Material zu ändern
            //temp = AddStorage.objectNumber;
            if (HitSomething())
            {
                onObject = true;
                rend.material = material2;          //material ändern wenn es sich auf einem Objekt befindet
            }
            else
            {
                onObject = false;
                rend.material = material1;          //material ändern wenn nicht mehr auf einem anderen Objekt ist    
            }
            Rotate(selected);
            if (!moveX && !moveZ)                                                   //Wenn keine gewünschte Achse Angegeben erfolgt die Bewegung durch die Mausposition
            {
                MoveAnywhere(selected);
            }
            if (Input.GetKeyDown(KeyCode.X))
            {
                moveX = true;
                moveZ = false;
            }
            if (moveX)                                                              //wenn x ausgewählt soll es nur möglich sein das Regal in X-Achse zu bewegen 
            {
                MoveInXAxis(selected);
            }
            if (Input.GetKeyDown(KeyCode.Y))
            {
                moveX = false;
                moveZ = true;
            }
            if (moveZ)
            {
                MoveInYAxis(selected);
            }
            if (Input.GetKey(KeyCode.Delete) || Input.GetKey(KeyCode.Backspace))
            {  //Mit entf oder backspace ausgewähltes Regal entfernen
                DeleteObject(selected);
            }
        }
    }
}






