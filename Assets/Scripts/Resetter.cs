using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public abstract class Resetter : MonoBehaviour
{
    public GameObject HUD;
    public TextMeshProUGUI HUD_text;
    
    [HideInInspector]
    public RDManager _rdManager;
    
    [HideInInspector]
    public PathTrail pathTrail;
    
    [HideInInspector]
    public GameManager gameManager;
    [HideInInspector]
    public float rotateDir; //rotation direction, positive if rotate clockwise
    [HideInInspector]
    public float speedRatio;
    [HideInInspector]
    public Vector2 totalF;
    [HideInInspector]
    public float requiredRotateSteerAngle;
    
    
    // Start is called before the first frame update
    void Start()
    {
        _rdManager = GetComponent<RDManager>();
        pathTrail = gameObject.GetComponent<PathTrail>();
        gameManager = GameObject.Find("Game Manager").GetComponent<GameManager>();
    }

    public abstract void InitializeReset();

    public abstract void InjectResetting();
    
    public void EndReset()
    {
        HUD.SetActive(false);
        requiredRotateSteerAngle = 0;
        _rdManager.OnResetEnd();
    }
    
    public void setHUD(int rotateDir)
    {
        HUD.SetActive(true);
        if(rotateDir > 0)
            HUD_text.SetText("Spin slowly in Place till this massage disappears\n ->>");
        else
            HUD_text.SetText("Spin slowly in Place till this massage disappears\n <<-");
        
    }
}
