using UnityEngine;
using Photon.Pun;
using Photon.Realtime; // Player
using TMPro;
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
        return linea;
    }
}