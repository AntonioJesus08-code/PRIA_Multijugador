using System.Collections.Generic; // List
using UnityEngine;
using Photon.Pun; // MonoBehaviourPun, PunRPC, RpcTarget
using TMPro;
// Chat de la sala. El objeto que lleva este script necesita un PhotonView.
public class Chat : MonoBehaviourPun
{
    [SerializeField] private TMP_InputField mensajeInput;
    [SerializeField] private TMP_Text chatText;
    private const int MaxLineas = 8;
    private readonly List<string> lineas = new List<string>();
    // Botón «Enviar»
    public void Enviar()
    {
        string texto = mensajeInput.text.Trim();
        if (texto.Length == 0 || !PhotonNetwork.InRoom) return;
        // Pedimos a Photon que ejecute RecibirMensaje en TODOS los jugadores de la sala.
        photonView.RPC(nameof(RecibirMensaje), RpcTarget.All, texto);
        mensajeInput.text = "";
        mensajeInput.ActivateInputField(); // el cursor vuelve al campo
    }
    // [PunRPC] marca el método como «se puede llamar por la red».
    // El último parámetro, PhotonMessageInfo, lo rellena Photon: quién lo envió y cuándo.
    [PunRPC]
    private void RecibirMensaje(string texto, PhotonMessageInfo info)
    {
        Escribir(info.Sender.NickName + ": " + texto);
    }
    private void Escribir(string linea)
    {
        lineas.Add(linea);
        if (lineas.Count > MaxLineas) lineas.RemoveAt(0); // solo las últimas
        chatText.text = string.Join("\n", lineas);
    }
}