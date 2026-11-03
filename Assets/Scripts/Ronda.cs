using UnityEngine;
using UnityEngine.UI; // Button
using Photon.Pun;
using Photon.Realtime; // Player
using TMPro;
using Hashtable = ExitGames.Client.Photon.Hashtable;

// Minijuego de reflejos. El Master Client (anfitrión) hace de servidor:
// empieza la ronda, recibe los comandos de los jugadores, decide quién gana y lo anuncia.
// El objeto que lleva este script necesita un PhotonView.
public class Ronda : MonoBehaviourPun
{
    [SerializeField] private Button botonEmpezar; // solo para el anfitrión
    [SerializeField] private Button botonPulsar;
    [SerializeField] private TMP_Text rondaText;

    // Estado que solo usa el anfitrión
    private bool rondaActiva;
    private bool hayGanador;

    private void Start()
    {
        botonPulsar.interactable = false;
        rondaText.text = "Esperando a que el anfitrión empiece una ronda";
    }

    private void Update()
    {
        // Por defecto, asumimos que todos están listos hasta que se demuestre lo contrario
        bool todosListos = true;

        if (PhotonNetwork.InRoom)
        {
            foreach (Player jugador in PhotonNetwork.PlayerList)
            {
                // Comprobamos si tiene la CustomProperty "listo" y si es true
                if (jugador.CustomProperties.TryGetValue("listo", out object valorListo))
                {
                    if (valorListo is bool listo && !listo)
                    {
                        todosListos = false;
                        break; // Si uno falla, ya no hace falta seguir comprobando
                    }
                }
                else
                {
                    // Si la propiedad ni siquiera existe todavía, consideramos que no está listo
                    todosListos = false;
                    break;
                }
            }
        }
        else
        {
            todosListos = false;
        }

        // Solo el anfitrión puede empezar rondas, y EXCLUSIVAMENTE si todos están listos.
        botonEmpezar.interactable = PhotonNetwork.InRoom && PhotonNetwork.IsMasterClient && todosListos;
    }

    // ---------- Anfitrión ----------

    // Botón «Empezar ronda»
    public void Empezar()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        rondaActiva = true;
        hayGanador = false;
        // Servidor → clientes: procedimiento remoto en todos.
        photonView.RPC(nameof(RpcRondaEmpieza), RpcTarget.All);
    }

    // Cliente → servidor: solo se ejecuta en el anfitrión.
    [PunRPC]
    private void CmdPulsar(PhotonMessageInfo info)
    {
        if (!PhotonNetwork.IsMasterClient) return; // por si acaso
        if (!rondaActiva || hayGanador) return; // VALIDACIÓN: ronda abierta y sin ganador

        hayGanador = true;
        rondaActiva = false;
        SumarPunto(info.Sender);

        // Servidor → clientes: anunciar el resultado a todos.
        photonView.RPC(nameof(RpcAnunciarGanador), RpcTarget.All, info.Sender.NickName);
    }

    private void SumarPunto(Player jugador)
    {
        int puntos = 0;
        if (jugador.CustomProperties.TryGetValue("puntos", out object valor))
        {
            puntos = (int)valor;
        }

        Hashtable propiedades = new Hashtable();
        propiedades["puntos"] = puntos + 1;
        jugador.SetCustomProperties(propiedades); // el anfitrión puede cambiar las de otros
    }

    // ---------- Todos ----------

    [PunRPC]
    private void RpcRondaEmpieza()
    {
        rondaText.text = "¡YA! Pulsa el botón";
        botonPulsar.interactable = true;
    }

    // Botón «¡Pulsa!»
    public void Pulsar()
    {
        botonPulsar.interactable = false; // un solo intento por ronda
        // Cliente → servidor: un comando que solo recibe el Master Client.
        photonView.RPC(nameof(CmdPulsar), RpcTarget.MasterClient);
    }

    [PunRPC]
    private void RpcAnunciarGanador(string nombre)
    {
        rondaText.text = "Gana " + nombre;
        botonPulsar.interactable = false;
    }
}