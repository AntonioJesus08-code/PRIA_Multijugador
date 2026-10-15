using UnityEngine;
using Photon.Pun; // PhotonNetwork y MonoBehaviourPunCallbacks
using Photon.Realtime; // DisconnectCause
using TMPro; // TMP_Text
// Heredamos de MonoBehaviourPunCallbacks en lugar de MonoBehaviour
// para que Photon pueda avisarnos de lo que pasa en la red.
public class ConexionPhoton : MonoBehaviourPunCallbacks
{
    // Aparece en el Inspector: ahí arrastraremos EstadoText.
    [SerializeField] private TMP_Text estadoText;
    private void Start()
    {
        estadoText.text = "Conectando con Photon...";
        // Nombre visible del jugador. Más adelante lo elegirá el usuario.
        PhotonNetwork.NickName = "Jugador" + Random.Range(1000, 10000);
        // Conecta usando el App ID y la región de PhotonServerSettings.
        PhotonNetwork.ConnectUsingSettings();
    }
    // Photon llama a este método cuando la conexión está lista.
    public override void OnConnectedToMaster()
    {
        string region = PhotonNetwork.CloudRegion;
        int ping = PhotonNetwork.GetPing();
        estadoText.text = "Conectado a Photon\n"
        + "Región: " + region + " · Ping: " + ping + " ms\n"
        + "Jugador: " + PhotonNetwork.NickName;
        Debug.Log("Conectado al Master Server. Región: " + region);
    }
    // Photon llama a este método si se pierde la conexión o no se consigue.
    public override void OnDisconnected(DisconnectCause causa)
    {
        estadoText.text = "Desconectado: " + causa;
        Debug.LogWarning("Photon se ha desconectado. Causa: " + causa);
    }
}