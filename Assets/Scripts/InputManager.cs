using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class InputManager : MonoBehaviour
{
    
    public static InputManager instance;
    
    private Controls _controls;
    
    public Vector2 Move;
  
    
    public Vector2 Look;
    
    private void Awake()
    {
        if (instance != null)
            Destroy(this);
        else
            instance = this;

        _controls = new Controls();
        _controls.Enable();

       
        
    }
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _controls.LocoMotion.Move.performed += context => Move = context.ReadValue<Vector2>();
        _controls.LocoMotion.Look.performed += context => Look = context.ReadValue<Vector2>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
