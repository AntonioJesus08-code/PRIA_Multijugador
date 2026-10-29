using System; // DateTime
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // Toggle
using Photon.Pun;
using ExitGames.Client.Photon; // PhotonPeer
using TMPro;
// Chat de la sala. El objeto que lleva este script necesita un PhotonView.
public class Chat : MonoBehaviourPun
{
    [SerializeField] private TMP_InputField mensajeInput;
    [SerializeField] private TMP_Text chatText;
    [SerializeField] private Toggle importanteToggle; // G07
    private const int MaxLineas = 8;
    private readonly List<string> lineas = new List<string>();
    private void Awake() // G07
    {
        // Registramos el tipo con un código de un byte que no use Photon.
        // PUN ya usa 16, 17, 22 y 23; Photon recomienda elegir desde 255 hacia abajo.
        PhotonPeer.RegisterType(typeof(MensajeChat), 255,
        MensajeChat.Serializar, MensajeChat.Deserializar);
    }
    // Botón «Enviar»
    public void Enviar()
    {
        string texto = mensajeInput.text.Trim();
        if (texto.Length == 0 || !PhotonNetwork.InRoom) return;
        MensajeChat mensaje = new MensajeChat(); // G07
        mensaje.Texto = texto;
        mensaje.Hora = DateTime.Now.ToString("HH:mm");
        mensaje.Importante = importanteToggle.isOn;
        photonView.RPC(nameof(RecibirMensaje), RpcTarget.All, mensaje);
        mensajeInput.text = "";
        mensajeInput.ActivateInputField();
    }
    [PunRPC]
    private void RecibirMensaje(MensajeChat mensaje, PhotonMessageInfo info) // G07
    {
        string linea = "[" + mensaje.Hora + "] " + info.Sender.NickName + ": " + mensaje.Texto;
        if (mensaje.Importante)
        {
            linea = "<b><color=#E74C3C>" + linea + "</color></b>"; // texto enriquecido de TMP
        }
        Escribir(linea);
    }
    private void Escribir(string linea)
    {
        lineas.Add(linea);
        if (lineas.Count > MaxLineas) lineas.RemoveAt(0);
        chatText.text = string.Join("\n", lineas);
    }
}