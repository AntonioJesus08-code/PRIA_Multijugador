using UnityEngine;
using UnityEngine.UI; // Button
using Photon.Pun;
using Photon.Realtime; // Player, RaiseEventOptions, ReceiverGroup, IOnEventCallback
using ExitGames.Client.Photon; // EventData, SendOptions
using TMPro;
using Hashtable = ExitGames.Client.Photon.Hashtable;

// Minijuego de reflejos. El Master Client (anfitrión) hace de servidor:
// empieza la ronda, recibe los comandos de los jugadores, decide quién gana y lo anuncia.
// G09: la ronda se anuncia con un EVENTO y todos usan el reloj del servidor.
// El objeto que lleva este script necesita un PhotonView.
public class Ronda : MonoBehaviourPun, IOnEventCallback // G09
{
    [SerializeField] private Button botonEmpezar; // solo para el anfitrión
    [SerializeField] private Button botonPulsar;
    [SerializeField] private TMP_Text rondaText;

    // G09 · Códigos de los eventos: números entre 1 y 199.
    private const byte EventoInicioRonda = 1;
    private const byte EventoFinPartida = 2; // Nuevo evento para anunciar el campeón

    // Estado que solo usa el anfitrión
    private bool rondaActiva;
    private bool hayGanador;
    private int numeroRonda; // G09

    // G09 · Estado de todos los jugadores
    private double horaYa; // hora del servidor en que hay que pulsar
    private bool esperandoYa; // estamos en «preparados...»

    // G09 · Registrarse para recibir eventos (y darse de baja al desactivarse).
    private void OnEnable() { PhotonNetwork.AddCallbackTarget(this); }
    private void OnDisable() { PhotonNetwork.RemoveCallbackTarget(this); }

    private void Start()
    {
        botonPulsar.interactable = false;
        rondaText.text = "Esperando a que el anfitrión empiece una ronda";
    }

    private void Update()
    {
        // Comprobación de que TODOS los jugadores estén listos
        bool todosListos = true;

        if (PhotonNetwork.InRoom)
        {
            foreach (Player jugador in PhotonNetwork.PlayerList)
            {
                if (jugador.CustomProperties.TryGetValue("listo", out object valorListo))
                {
                    if (valorListo is bool listo && !listo)
                    {
                        todosListos = false;
                        break;
                    }
                }
                else
                {
                    todosListos = false;
                    break;
                }
            }
        }
        else
        {
            todosListos = false;
        }

        // Solo el anfitrión puede empezar rondas, y exclusivamente si todos están listos.
        botonEmpezar.interactable = PhotonNetwork.InRoom && PhotonNetwork.IsMasterClient && todosListos;

        // G09 · Todos usan el mismo reloj (el del servidor): el «¡YA!» sale a la vez en todos.
        if (esperandoYa && PhotonNetwork.Time >= horaYa)
        {
            esperandoYa = false;
            rondaText.text = "¡YA! Pulsa el botón";
        }
    }

    // ---------- Anfitrión ----------

    // Botón «Empezar ronda»
    public void Empezar()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        numeroRonda++; // G09
        rondaActiva = true;
        hayGanador = false;

        // G09 · El «¡YA!» será entre 2 y 5 segundos después, en hora del servidor.
        horaYa = PhotonNetwork.Time + Random.Range(2f, 5f);

        // G09 · Servidor → clientes: un EVENTO en lugar de un RPC.
        object[] datos = { numeroRonda, horaYa };
        RaiseEventOptions opciones = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        PhotonNetwork.RaiseEvent(EventoInicioRonda, datos, opciones, SendOptions.SendReliable);
    }

    // Cliente → servidor: solo se ejecuta en el anfitrión.
    [PunRPC]
    private void CmdPulsar(PhotonMessageInfo info)
    {
        if (!PhotonNetwork.IsMasterClient) return; // por si acaso
        if (!rondaActiva || hayGanador) return; // VALIDACIÓN: ronda abierta y sin ganador

        // G09 · SentServerTime: cuándo pulsó, en hora del servidor. La latencia no perjudica a nadie.
        if (info.SentServerTime < horaYa)
        {
            photonView.RPC(nameof(RpcFalsaSalida), info.Sender); // solo a ese jugador
            return;
        }

        hayGanador = true;
        rondaActiva = false;
        SumarPunto(info.Sender);

        // Comprobar si el jugador ha llegado a 5 puntos para finalizar la partida
        int puntosActuales = 0;
        if (info.Sender.CustomProperties.TryGetValue("puntos", out object valPuntos))
        {
            puntosActuales = (int)valPuntos;
        }

        if (puntosActuales >= 5)
        {
            // Enviar EventoFinPartida (código 2) a todos con el nombre del campeón
            object[] datosFin = { info.Sender.NickName };
            RaiseEventOptions opcionesFin = new RaiseEventOptions { Receivers = ReceiverGroup.All };
            PhotonNetwork.RaiseEvent(EventoFinPartida, datosFin, opcionesFin, SendOptions.SendReliable);
            return;
        }

        // Servidor → clientes: anunciar el resultado a todos.
        int milisegundos = (int)((info.SentServerTime - horaYa) * 1000); // G09
        photonView.RPC(nameof(RpcAnunciarGanador), RpcTarget.All, info.Sender.NickName, milisegundos);
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

    // G09 · Photon llama a OnEvent con CUALQUIER evento: miramos el código.
    public void OnEvent(EventData evento)
    {
        if (evento.Code == EventoInicioRonda)
        {
            object[] datos = (object[])evento.CustomData;
            int numero = (int)datos[0];
            horaYa = (double)datos[1];
            esperandoYa = true;
            botonPulsar.interactable = true; // si pulsas antes del «¡YA!», es falsa salida
            rondaText.text = "Ronda " + numero + ": preparados...";
        }
        else if (evento.Code == EventoFinPartida)
        {
            object[] datos = (object[])evento.CustomData;
            string campeon = (string)datos[0];
            esperandoYa = false;
            rondaActiva = false;
            rondaText.text = "¡FIN DE LA PARTIDA! Campeón: " + campeon;
            botonPulsar.interactable = false;
            botonEmpezar.interactable = false; // Bloquear botón de empezar definitivamente
        }
    }

    // Botón «¡Pulsa!»
    public void Pulsar()
    {
        botonPulsar.interactable = false; // un solo intento por ronda
        // Cliente → servidor: un comando que solo recibe el Master Client.
        photonView.RPC(nameof(CmdPulsar), RpcTarget.MasterClient);
    }

    [PunRPC]
    private void RpcFalsaSalida() // G09
    {
        rondaText.text = "¡Falsa salida! Has pulsado antes de tiempo";
    }

    [PunRPC]
    private void RpcAnunciarGanador(string nombre, int milisegundos) // G09: + milisegundos
    {
        esperandoYa = false; // G09
        rondaText.text = "Gana " + nombre + " en " + milisegundos + " ms"; // G09
        botonPulsar.interactable = false;
    }
}