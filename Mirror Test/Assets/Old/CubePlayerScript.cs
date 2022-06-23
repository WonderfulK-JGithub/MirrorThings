using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using TMPro;
using UnityEngine.SceneManagement;

public class CubePlayerScript : NetworkBehaviour
{
    [SerializeField] TextMeshProUGUI playerNameText;
    [SerializeField] float moveSpeed;

    [Scene] [SerializeField] string otherScene;

    Rigidbody rb;
    Vector2 currentVelocity;

    string playerName;
    public void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    #region renewing Data
    public override void OnStartAuthority()
    {
        playerName = PlayerNameInput.DisplayName;
        playerNameText.text = playerName;

        CmdRenewAllData();
    }

    [Command]
    void CmdRenewAllData()
    {
        foreach (var item in FindObjectsOfType<CubePlayerScript>())
        {
            item.RenewData();
        }
    }

    [ClientRpc]
    public void RenewData()
    {
        if (!hasAuthority) return;
        DataToRenew data = new DataToRenew
        {
            playerName = playerName,
            velocity = rb.velocity,
        };
        CmdRenewData(data);
        
    }

    [Command]
    void CmdRenewData(DataToRenew data)
    {
        RpcRenewData(data);
    }
    
    [ClientRpc]
    void RpcRenewData(DataToRenew data)
    {
        playerName = data.playerName;
        playerNameText.text = playerName;

        rb.velocity = data.velocity;
    }

    struct DataToRenew
    {
        public string playerName;

        public Vector3 velocity;
    }

    #endregion


    void Update()
    {
        if (!hasAuthority) return;

        if(Input.GetKeyDown(KeyCode.P))
        {
            
        }
        
        //currentVelocity = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        CmdNewVelocity(new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")));
    }

    [Command]
    void CmdNewVelocity(Vector2 input)
    {
        RpcNewVelocity(input);
    }

    [ClientRpc]
    void RpcNewVelocity(Vector2 input)
    {
        currentVelocity = input;
    }


    private void FixedUpdate()
    {
        rb.velocity = new Vector3(currentVelocity.x * moveSpeed, rb.velocity.y, currentVelocity.y * moveSpeed);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!hasAuthority) return;

        Component interactable = other.GetComponent(typeof(IInteractable));

        if(interactable != null)
        {
            CmdInteract((other.gameObject));
        }
    }

    [Command]
    void CmdInteract(GameObject interaction)
    {
        RpcInteract(interaction);
    }
    [ClientRpc]
    void RpcInteract(GameObject interaction)
    {
        (interaction.GetComponent(typeof(IInteractable)) as IInteractable).Interact();
    }
}
