
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

// Gestiona las salas: crear, unirse y mostrar su información.
public class GestorSala : MonoBehaviourPunCallbacks
{
    [Header("Paneles")]
    [SerializeField] private GameObject panelLobby;
    [SerializeField] private GameObject panelSala;

    [Header("Lobby")]
    [SerializeField] private TMP_InputField nombreJugadorInput; // G05
    [SerializeField] private TMP_InputField nombreSalaInput;
    [SerializeField] private TMP_Text mensajeLobbyText;
    [SerializeField] private Button[] botonesLobby;

    [Header("Configuración de sala")]
    [SerializeField] private TMP_Dropdown maxJugadoresDropdown;

    [Header("Sala")]
    [SerializeField] private TMP_Text infoSalaText;

    private const int MaxJugadores = 8;
    private const int MinJugadores = 2;

    private void Start()
    {
        panelLobby.SetActive(true);
        panelSala.SetActive(false);

        // Configurar las opciones del desplegable.
        maxJugadoresDropdown.ClearOptions();

        System.Collections.Generic.List<string> opciones =
            new System.Collections.Generic.List<string>();

        for (int i = MinJugadores; i <= MaxJugadores; i++)
        {
            opciones.Add(i.ToString());
        }

        maxJugadoresDropdown.AddOptions(opciones);
        maxJugadoresDropdown.value = 2; // Selección inicial: 4 jugadores
        maxJugadoresDropdown.RefreshShownValue();

        ActivarBotones(false);
        mensajeLobbyText.text = "Esperando la conexión con Photon...";
    }

    private void Update()
    {
        // Mostrar el estado actual de la conexión de Photon.
        mensajeLobbyText.text =
            "Estado de red: " + PhotonNetwork.NetworkClientState;
    }

    // ---------- Callbacks de conexión ----------

    public override void OnConnectedToMaster()
    {
        ActivarBotones(true);
        mensajeLobbyText.text = "Conectado. Crea una sala o únete a una.";
    }

    // ---------- Botones ----------

    public void CrearSala()
    {
        string nombre = nombreSalaInput.text.Trim();

        if (nombre.Length == 0)
        {
            mensajeLobbyText.text = "Escribe un nombre para la sala.";
            return;
        }

        // El desplegable empieza en 0: 2 jugadores.
        int maxJugadores = maxJugadoresDropdown.value + MinJugadores;

        PonerNombre();
        ActivarBotones(false);
        mensajeLobbyText.text = "Creando la sala " + nombre + "...";

        RoomOptions opciones = new RoomOptions
        {
            MaxPlayers = (byte)maxJugadores
        };

        PhotonNetwork.CreateRoom(nombre, opciones);
    }

    public void UnirseSala()
    {
        string nombre = nombreSalaInput.text.Trim();

        if (nombre.Length == 0)
        {
            mensajeLobbyText.text = "Escribe el nombre de la sala.";
            return;
        }

        PonerNombre();
        ActivarBotones(false);
        mensajeLobbyText.text = "Entrando en " + nombre + "...";

        PhotonNetwork.JoinRoom(nombre);
    }

    public void UnirseAleatoria()
    {
        PonerNombre();

        ActivarBotones(false);
        mensajeLobbyText.text = "Buscando una sala libre...";

        PhotonNetwork.JoinRandomRoom();
    }
    public void SalirSala() // G05
    {
        PhotonNetwork.LeaveRoom();
    }

    // ---------- Callbacks de salas ----------

    public override void OnJoinedRoom()
    {
        panelLobby.SetActive(false);
        panelSala.SetActive(true);

        ActualizarInfoSala();

        Debug.Log("Dentro de la sala " + PhotonNetwork.CurrentRoom.Name);
    }

    public override void OnCreateRoomFailed(short codigo, string mensaje)
    {
        mensajeLobbyText.text = "No se pudo crear la sala: " + mensaje;
        ActivarBotones(true);
    }

    public override void OnJoinRoomFailed(short codigo, string mensaje)
    {
        mensajeLobbyText.text = "No se pudo entrar: " + mensaje;
        ActivarBotones(true);
    }

    public override void OnJoinRandomFailed(short codigo, string mensaje)
    {
        mensajeLobbyText.text = "No hay salas libres. Creando una nueva...";

        // Crear una sala automática con el máximo seleccionado.
        int maxJugadores = maxJugadoresDropdown.value + MinJugadores;

        RoomOptions opciones = new RoomOptions
        {
            MaxPlayers = (byte)maxJugadores
        };

        PhotonNetwork.CreateRoom(null, opciones);
    }

    public override void OnPlayerEnteredRoom(Player nuevoJugador)
    {
        ActualizarInfoSala();
    }

    public override void OnPlayerLeftRoom(Player jugador)
    {
        ActualizarInfoSala();
    }

    public override void OnLeftRoom() // G05
    {
        panelSala.SetActive(false);
        panelLobby.SetActive(true);
        mensajeLobbyText.text = "Has salido de la sala.";
        // Al salir, Photon vuelve al Master Server y llama otra vez
        // a OnConnectedToMaster, que reactiva los botones.
    }


    // ---------- Ayudantes ----------
    // primera línea útil de CrearSala() , UnirseSala() y UnirseAleatoria()
    private void PonerNombre() // G05
    {
        string nombre = nombreJugadorInput.text.Trim();
        if (nombre.Length > 0)
        {
            PhotonNetwork.NickName = nombre; // si no, se queda el JugadorXXXX de la G01
        }
    }

    private void ActualizarInfoSala()
    {
        Room sala = PhotonNetwork.CurrentRoom;

        infoSalaText.text = "Sala: " + sala.Name
            + " · Jugadores: " + sala.PlayerCount
            + "/" + sala.MaxPlayers;
    }
    private void ActivarBotones(bool activos)
    {
        foreach (Button boton in botonesLobby)
        {
            boton.interactable = activos;
        }
    }
}
