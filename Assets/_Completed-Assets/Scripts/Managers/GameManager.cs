using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

namespace Complete
{
    public class GameManager : MonoBehaviour
    {
        public static bool inicio = false;          //Controlamos cuando inicia el juego.
        public static int numJugadores;             //Controlamos cúantos jugadores hay en la partida
        public GameObject PanelInicio;              //Panel Inicial de elección de número de jugadores
        public GameObject CamaraMapa;               //Cámara que muestra el mapa cuando hay tres jugadores

        private string textBotonStart;              //Texto del boton Start
        public GameObject botonStart;               //Boton Start
        private bool playerStart = false;                   //Controlamos si está activo

        public int m_NumRoundsToWin = 5;            // The number of rounds a single player has to win to win the game
        public float m_StartDelay = 3f;             // The delay between the start of RoundStarting and RoundPlaying phases
        public float m_EndDelay = 3f;               // The delay between the end of RoundPlaying and RoundEnding phases
        public CameraControl m_CameraControl;       // Reference to the CameraControl script for control during different phases
        public Text m_MessageText;                  // Reference to the overlay Text to display winning text, etc.
        public GameObject m_TankPrefab;             // Reference to the prefab the players will control
        public TankManager[] m_Tanks;               // A collection of managers for enabling and disabling different aspects of the tanks

        private int m_RoundNumber;                  // Which round the game is currently on
        private WaitForSeconds m_StartWait;         // Used to have a delay whilst the round starts
        private WaitForSeconds m_EndWait;           // Used to have a delay whilst the round or game ends
        private TankManager m_RoundWinner;          // Reference to the winner of the current round.  Used to make an announcement of who won
        private TankManager m_GameWinner;           // Reference to the winner of the game.  Used to make an announcement of who won

        public Camera MainCamera;                   //Camara principal
        public List<Camera> CamarasActivas;         //Cámaras activas durante el juego
        public List<Camera> CamarasInactivas;       //Cámaras que se desactivan durante el juego
        public static int jugadoresRonda = 0;       //Controlamos cuantos jugadores han participado en la ronda para habilitarlos en la siguiente.

        public static List<GameObject> tanks;       //Lista dónde almacenamos los tanques activos

        private void Start()
        {
            //Inicializamos las listas
            tanks = new List<GameObject>();
            CamarasActivas = new List<Camera>();
            CamarasInactivas = new List<Camera>();

            // Create the delays so they only have to be made once
            m_StartWait = new WaitForSeconds(m_StartDelay);
            m_EndWait = new WaitForSeconds(m_EndDelay);

            //Asignamos el Start del input
            textBotonStart = "Start"; 
        }

        private void Update()
        {
            //Esperamos a que se haya elegido el número de jugadores para empezar la partida.
            if (inicio)
            {
                inicio = false;
                PanelInicio.SetActive(false); //Quitamos el panel de incio
                SpawnAllTanks(); //Creamos los tanques
                SetCameraTargets(); //Creeamos los targets que seguirán las cámaras para cada tanque.
                StartCoroutine(GameLoop()); //Iniciamos el juego.
            }
            MostrarStart(); //Mostramos el de start en funcion del numero de jugadores en cada momento.
            MostrarCamaraMapa(); //Mostramos la cámara con el mapa si el número de jugadores es 3.
            DetectarStart(); //Detectamos si se pulsan los botones de start;

        }

        //Mostramos el mensaje de Start si hay menos de 4 jugadores en la partida
        private void MostrarStart()
        {
            if (jugadoresRonda < 4)
            {
                if (numJugadores == 2)
                {
                    botonStart.SetActive(true);
                    playerStart = true;
                }
                else if (numJugadores == 3)
                {
                    botonStart.SetActive(true);
                    playerStart = true;
                }
                else
                {
                    botonStart.SetActive(false);
                    playerStart = false;
                }
            }
        }

        //Mostramos la cámara cuando hay 3 jugadores para rellenar el hueco
        private void MostrarCamaraMapa()
        {
            if (numJugadores == 3)
            {
                CamaraMapa.SetActive(true);
            }
            else
            {
                CamaraMapa.SetActive(false);
            }
        }

        //Detectamos si un jugador le ha dado al Start
        private void DetectarStart()
        {
            if (playerStart && jugadoresRonda < 4)
            {
                if (Input.GetButtonDown(textBotonStart))
                {
                    //Creamos un nuevo jugador
                    NuevoJugador();
                }
            }
        }

        private void NuevoJugador()
        {
            //añadimos un nuevo jugador a la ronda para que lo tenga en cuenta para la siguiente.
            jugadoresRonda++;

            //Instanciamos el tanque
            m_Tanks[numJugadores].m_Instance =
                    Instantiate(m_TankPrefab, m_Tanks[numJugadores].m_SpawnPoint.position, m_Tanks[numJugadores].m_SpawnPoint.rotation) as GameObject;
            m_Tanks[numJugadores].m_PlayerNumber = numJugadores + 1;
            m_Tanks[numJugadores].Setup();
            tanks.Add(m_Tanks[numJugadores].m_Instance);

            m_CameraControl.m_Targets[numJugadores] = m_Tanks[numJugadores].m_Instance.transform;
            
            //Le añadimos la cámara
            AddCamera(numJugadores, MainCamera);
            //Actualizamos la posicion y el tamaño de todas las cámaras del juego.
            actualizarCamaras();
            //Contamos un jugador más.
            numJugadores++;
        }

        //Actualizamos la posicion y el tamaño de las cámaras en funcion de los jugadores
        public void actualizarCamaras()
        {
            for (int i = 0; i < CamarasActivas.Count; i++)
            {
                if (numJugadores > 2)
                {
                    switch (i)
                    {
                        case 0:
                            //Cámara superior izquierda
                            CamarasActivas[i].rect = new Rect(0.0f, 0.5f, 0.5f, 0.5f);
                            break;
                        case 1:
                            //Cámara superior derecha
                            CamarasActivas[i].rect = new Rect(0.5f, 0.5f, 0.5f, 0.5f);
                            break;
                        case 2:
                            //Cámara inferior izquierda
                            CamarasActivas[i].rect = new Rect(0.0f, 0.0f, 0.5f, 0.5f);
                            break;
                        case 3:
                            //Cámara inferior derecha
                            CamarasActivas[i].rect = new Rect(0.5f, 0.0f, 0.5f, 0.5f);
                            break;
                    }
                }
                else if (i == 0)
                {
                    //Camara superior 2 jugadores
                    CamarasActivas[i].rect = new Rect(0.0f, 0.5f, 1.0f, 0.5f);
                }
                else
                {
                    //Cámara inferior 2 jugadores
                    CamarasActivas[i].rect = new Rect(0.0f, 0.0f, 1.0f, 0.5f);
                }
            }
            m_CameraControl.SetStartPositionAndSize();
        }
      
        private void SpawnAllTanks()
		{
            //instanciamos todos los tanques al iniciar la partida
            // For all the tanks...
            for (int i = 0; i < numJugadores; i++)
			{
				// ... create them, set their player number and references needed for control
				m_Tanks[i].m_Instance =
					Instantiate (m_TankPrefab, m_Tanks[i].m_SpawnPoint.position, m_Tanks[i].m_SpawnPoint.rotation) as GameObject;
				m_Tanks[i].m_PlayerNumber = i + 1;
				m_Tanks[i].Setup();
				AddCamera (i, MainCamera);
                tanks.Add(m_Tanks[i].m_Instance);
			}

            MainCamera.gameObject.SetActive (false);
		}

        //Asignamos una cámara a cada tanque
		private void AddCamera (int i, Camera mainCam)
        {
			GameObject childCam = new GameObject ("Camera" + (i));
			Camera newCam = childCam.AddComponent<Camera>();
            
            newCam.CopyFrom(mainCam);
            newCam.GetComponent<Camera>().orthographicSize = 7f;
			childCam.transform.parent = m_Tanks[i].m_Instance.transform;

            if(numJugadores == 2) {

                if (i == 0)
                {
                    newCam.rect = new Rect(0.0f, 0.5f, 1.0f, 0.5f);
                    CamarasActivas.Add(newCam);
                }
                else
                {
                    newCam.rect = new Rect(0.0f, 0.0f, 1.0f, 0.5f);
                    CamarasActivas.Add(newCam);
                }
            }
            else
            {
                switch (i)
                {
                    case 0:
                        newCam.rect = new Rect(0.0f, 0.5f, 0.5f, 0.5f);
                        CamarasActivas.Add(newCam);
                        break;
                    case 1:
                        newCam.rect = new Rect(0.5f, 0.5f, 0.5f, 0.5f);
                        CamarasActivas.Add(newCam);
                        break;
                    case 2:
                        newCam.rect = new Rect(0.0f, 0.0f, 0.5f, 0.5f);
                        CamarasActivas.Add(newCam);
                        break;
                    case 3:
                        newCam.rect = new Rect(0.5f, 0.0f, 0.5f, 0.5f);
                        CamarasActivas.Add(newCam);
                        break;
                }
            }  
		}

        private void SetCameraTargets()
        {
            // Create a collection of transforms the same size as the number of tanks
            Transform[] targets = new Transform[m_Tanks.Length];

            // For each of these transforms...
            for (int i = 0; i < numJugadores; i++)
            {
                // ... set it to the appropriate tank transform
                targets[i] = m_Tanks[i].m_Instance.transform;
            }

            // These are the targets the camera should follow
            m_CameraControl.m_Targets = targets;
        }
        
        // This is called from start and will run each phase of the game one after another
        private IEnumerator GameLoop()
        {
            // Start off by running the 'RoundStarting' coroutine but don't return until it's finished
            yield return StartCoroutine (RoundStarting());

            // Once the 'RoundStarting' coroutine is finished, run the 'RoundPlaying' coroutine but don't return until it's finished
            yield return StartCoroutine (RoundPlaying());

            // Once execution has returned here, run the 'RoundEnding' coroutine, again don't return until it's finished
            yield return StartCoroutine (RoundEnding());

            // This code is not run until 'RoundEnding' has finished.  At which point, check if a game winner has been found
            if (m_GameWinner != null)
            {
                // If there is a game winner, restart the level
                SceneManager.LoadScene (0);
            }
            else
            {
                // If there isn't a winner yet, restart this coroutine so the loop continues
                // Note that this coroutine doesn't yield.  This means that the current version of the GameLoop will end
                StartCoroutine (GameLoop());
            }
        }


        private IEnumerator RoundStarting()
        {
            // As soon as the round starts reset the tanks and make sure they can't move
            ResetAllTanks();
            DisableTankControl();

            //Pasamos las cámaras de la lista de inactivas a la lista de cámras activas
            for (int i = 0; i < CamarasInactivas.Count; i++)
            {
                CamarasActivas.Add(CamarasInactivas[i]); 
            }

            for(int i = 0; i < CamarasActivas.Count; i++)
            {
                //Actiavamos todas las cámaras de la lista de cámaras activas
                CamarasActivas[i].gameObject.SetActive(true);
                CamarasActivas[i].GetComponent<Camera>().enabled = true;
            }
            //Habilitamos la cámara de la vista global
            CamaraMapa.GetComponent<Camera>().enabled = true;
            //Ordenamos la lista de las cámaras
            CamarasActivas = CamarasActivas.OrderBy(x => x.name).ToList();
            //Eliminamos las cámaras de la lista de inactivas
            CamarasInactivas.Clear();
            //Actualizamos la posicion y el tamaño de las cámaras en función al número de jugadores
            actualizarCamaras();

            // Snap the camera's zoom and position to something appropriate for the reset tanks
            m_CameraControl.SetStartPositionAndSize();

            // Increment the round number and display text showing the players what round it is
            m_RoundNumber++;
            m_MessageText.text = "ROUND " + m_RoundNumber;

            // Wait for the specified length of time until yielding control back to the game loop
            yield return m_StartWait;
        }


        private IEnumerator RoundPlaying()
        {
            // As soon as the round begins playing let the players control the tanks
            EnableTankControl();

            // Clear the text from the screen
            m_MessageText.text = string.Empty;

            // While there is not one tank left...
            while (!OneTankLeft())
            {
                // ... return on the next frame
                yield return null;
            }
        }


        private IEnumerator RoundEnding()
        {
            numJugadores = jugadoresRonda;
            tanks.Clear();
            for(int i = 0; i < numJugadores; i++)
            {
                tanks.Add(m_Tanks[i].m_Instance);
            }

            // Stop tanks from moving
            DisableTankControl();

            // Clear the winner from the previous round
            m_RoundWinner = null;

            // See if there is a winner now the round is over
            m_RoundWinner = GetRoundWinner();

            // If there is a winner, increment their score
            if (m_RoundWinner != null)
                m_RoundWinner.m_Wins++;

            // Now the winner's score has been incremented, see if someone has one the game
            m_GameWinner = GetGameWinner();

            // Get a message based on the scores and whether or not there is a game winner and display it
            string message = EndMessage();
            m_MessageText.text = message;

            // Wait for the specified length of time until yielding control back to the game loop
            yield return m_EndWait;
        }


        // This is used to check if there is one or fewer tanks remaining and thus the round should end
        private bool OneTankLeft()
        {
            //Si sólo queda un tanque o ninguno, terminamos la ronda.
            return tanks.Count <= 1;

        }
        
        
        // This function is to find out if there is a winner of the round
        // This function is called with the assumption that 1 or fewer tanks are currently active
        private TankManager GetRoundWinner()
        {
            // Go through all the tanks...
            for (int i = 0; i < numJugadores; i++)
            {
                // ... and if one of them is active, it is the winner so return it
                if (m_Tanks[i].m_Instance.activeSelf)
                {
                    return m_Tanks[i];
                }
            }

            // If none of the tanks are active it is a draw so return null
            return null;
        }


        // This function is to find out if there is a winner of the game
        private TankManager GetGameWinner()
        {
            // Go through all the tanks...
            for (int i = 0; i < numJugadores; i++)
            {
                // ... and if one of them has enough rounds to win the game, return it
                if (m_Tanks[i].m_Wins == m_NumRoundsToWin)
                {
                    return m_Tanks[i];
                }
            }

            // If no tanks have enough rounds to win, return null
            return null;
        }


        // Returns a string message to display at the end of each round.
        private string EndMessage()
        {
            // By default when a round ends there are no winners so the default end message is a draw
            string message = "DRAW!";

            // If there is a winner then change the message to reflect that
            if (m_RoundWinner != null)
            {
                message = m_RoundWinner.m_ColoredPlayerText + " WINS THE ROUND!";
            }

            // Add some line breaks after the initial message
            message += "\n\n\n\n";

            // Go through all the tanks and add each of their scores to the message
            for (int i = 0; i < numJugadores; i++)
            {
                message += m_Tanks[i].m_ColoredPlayerText + ": " + m_Tanks[i].m_Wins + " WINS\n";
            }

            // If there is a game winner, change the entire message to reflect that
            if (m_GameWinner != null)
            {
                message = m_GameWinner.m_ColoredPlayerText + " WINS THE GAME!";
            }

            return message;
        }


        // This function is used to turn all the tanks back on and reset their positions and properties
        private void ResetAllTanks()
        {
            for (int i = 0; i < numJugadores; i++)
            {
                m_Tanks[i].Reset();
                
            }
        }


        private void EnableTankControl()
        {
            for (int i = 0; i < numJugadores; i++)
            {
                m_Tanks[i].EnableControl();
            }
        }


        private void DisableTankControl()
        {
            for (int i = 0; i < numJugadores; i++)
            {
                m_Tanks[i].DisableControl();
            }
        }
    }
}