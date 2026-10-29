using UnityEngine;
using Photon.Pun;
using Photon.Realtime; // Player
using TMPro;
using Hashtable = ExitGames.Client.Photon.Hashtable; // G07: la tabla de Photon, no la de System

// Muestra quién está en la sala y avisa de las entradas y salidas.
public class ListaJugadores : MonoBehaviourPunCallbacks
{
    [SerializeField] private TMP_Text jugadoresText;
    [SerializeField] private TMP_Text avisoText;

    public override void OnJoinedRoom()
    {
        avisoText.text = "Has entrado como " + PhotonNetwork.NickName;
        Refrescar();
    }
    public override void OnPlayerEnteredRoom(Player jugador)
    {
        avisoText.text = jugador.NickName + " ha entrado";
        Refrescar();
    }
    public override void OnPlayerLeftRoom(Player jugador)
    {
        avisoText.text = jugador.NickName + " se ha ido";
        Refrescar();
    }
    public override void OnMasterClientSwitched(Player nuevoAnfitrion)
    {
        avisoText.text = nuevoAnfitrion.NickName + " es el nuevo anfitrión";
        Refrescar();
    }
    // G07 · Lo llama el Toggle «Listo» (On Value Changed → Dynamic bool).
    public void CambiarListo(bool listo)
    {
        Hashtable propiedades = new Hashtable();
        propiedades["listo"] = listo;
        PhotonNetwork.LocalPlayer.SetCustomProperties(propiedades);
    }
    // G07 · Photon avisa a todos cuando cambian las propiedades de un jugador.
    // G07 · Photon avisa a todos cuando cambian las propiedades de un jugador.
    public override void OnPlayerPropertiesUpdate(Player jugador, Hashtable cambios)
    {
        Refrescar();
    }

    private void Refrescar()
    {
        string texto = "";
        foreach (Player jugador in PhotonNetwork.PlayerList)
        {
            texto += Describir(jugador) + "\n";
        }
        jugadoresText.text = texto;
    }
    private string Describir(Player jugador)
    {
        string linea = jugador.NickName;
        if (jugador.IsMasterClient) linea += " [anfitrión]";
        if (jugador.IsLocal) linea += " (tú)";
        if (jugador.CustomProperties.TryGetValue("listo", out object listo) && (bool)listo) // G07
        {
            linea += " · LISTO";
        }
        return linea;
    }
}