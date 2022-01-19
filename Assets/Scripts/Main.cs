using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Main : MonoBehaviour
{
    private KeyCode[] keyCodes = {
         KeyCode.Alpha1,
         KeyCode.Alpha2,
         KeyCode.Alpha3,
     };

    private Vector3 mOffset;
    private float mZCoord;

    public GameObject[] objectPrefabs;

    public FlexibleColorPicker fcp;
    private Color oldColor;
    private Rigidbody rb = null;

    private GameObject selectedObject = null;
    private GameObject pickedUpObject = null;

    void Start()
    {
    }

    void Update()
    {
        var mousePos = Mouse.current.position.ReadValue();
        Ray ray = Camera.main.ScreenPointToRay(mousePos);

        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            Debug.DrawLine(ray.origin, hit.point, Color.red);

            // To place an object
            InstantiateObject(hit);

            // To move an object
            MoveObject(hit);

            // To open the color menu
            OpenColorMenu(hit);

            // If color menu is open then update selected object color
            UpdateColorMenu(hit);

            // To destroy an object
            DeleteObject(hit);
        }
    }

    /// <summary>
    /// Get mouse world point
    /// </summary>
    /// <returns>Mouse position</returns>
    private Vector3 GetMouseAsWorldPoint()
    {
        // Pixel coordinates of mouse (x,y)
        Vector3 mousePoint = Input.mousePosition;

        // z coordinate of game object on screen
        mousePoint.z = mZCoord;

        // Convert it to world points
        return Camera.main.ScreenToWorldPoint(mousePoint);

    }

    /// <summary>
    /// Spawn object
    /// </summary>
    /// <param name="hit">Selected object</param>
    private void InstantiateObject(RaycastHit hit)
    {
        if(pickedUpObject == null)
        {
            for (int i = 0; i < keyCodes.Length; i++)
            {
                // Select key
                if (Input.GetKeyDown(keyCodes[i]))
                {
                    pickedUpObject = Instantiate(objectPrefabs[i], new Vector3(hit.point.x,
                        hit.point.y,
                        hit.point.z),
                        Quaternion.identity);
                    rb = pickedUpObject.GetComponent<Rigidbody>();
                    mZCoord = Camera.main.WorldToScreenPoint(pickedUpObject.transform.position).z;
                    mOffset = pickedUpObject.transform.position - GetMouseAsWorldPoint();
                }
            }
        }       
    }

    /// <summary>
    /// Move selected object
    /// </summary>
    /// <param name="hit">Selected object</param>
    private void MoveObject(RaycastHit hit)
    {
        if (hit.collider.CompareTag("EditableObject"))   
        {
            // Pick up object
            if (Input.GetMouseButtonDown(0))
            {
                rb = hit.collider.GetComponent<Rigidbody>();

                pickedUpObject = hit.collider.gameObject;

                mZCoord = Camera.main.WorldToScreenPoint(pickedUpObject.transform.position).z;

                // Store offset = gameobject world pos - mouse world pos
                mOffset = pickedUpObject.transform.position - GetMouseAsWorldPoint();
            }

            // Put down object
            if (Input.GetMouseButtonUp(0))
            {
                rb.useGravity = true;
                pickedUpObject = null;
                rb = null;
            }

            // If object is picked up update its position
            if (pickedUpObject != null)
            {
                rb.MovePosition(GetMouseAsWorldPoint() + mOffset);
                rb.useGravity = false;
            }
        } 
    }

    /// <summary>
    /// Open color menu
    /// </summary>
    /// <param name="hit">Selected object</param>
    private void OpenColorMenu(RaycastHit hit)
    {
        if (Input.GetMouseButtonDown(1) && hit.collider.tag == "EditableObject" && !fcp.gameObject.activeSelf)
        {
            fcp.gameObject.SetActive(true);
            selectedObject = hit.collider.gameObject;
            fcp.color = selectedObject.GetComponent<Renderer>().material.color;
            oldColor = fcp.color;
        }
    }

    /// <summary>
    /// Update selected object material color
    /// </summary>
    /// <param name="hit">Selected object</param>
    private void UpdateColorMenu(RaycastHit hit)
    {
        if (fcp.gameObject.activeSelf)
        {
            selectedObject.GetComponent<Renderer>().material.color = fcp.color;
        }
    }

    /// <summary>
    /// Apply selected material
    /// </summary>
    public void ApplyMaterial()
    {
        oldColor = selectedObject.GetComponent<MeshRenderer>().material.color;
        fcp.gameObject.SetActive(false);
    }

    /// <summary>
    /// Cancel material selection
    /// </summary>
    public void CancelMaterial()
    {
        selectedObject.GetComponent<MeshRenderer>().material.color = oldColor;
        fcp.gameObject.SetActive(false);
    }

    /// <summary>
    /// Destroy editable object
    /// </summary>
    /// <param name="hit">Selected object</param>
    private void DeleteObject(RaycastHit hit)
    {
        if(Input.GetKey(KeyCode.D) && hit.collider.tag == "EditableObject")
        {
            Destroy(hit.collider.gameObject);
            fcp.gameObject.SetActive(false);
        }
    }
}
