using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BotonNumJugadores : MonoBehaviour
{
    public int NumJugadores;        //Numero de jugadores seleccionado
    
    // Start is called before the first frame update
    
    public void pulsarBoton()
    {
        //Asignamos el número de jugadores
        Complete.GameManager.numJugadores = NumJugadores;
        //Asignamos los jugadores de esa ronda iniciales
        Complete.GameManager.jugadoresRonda = NumJugadores;
        //Iniciamos la partida
        Complete.GameManager.inicio = true;
    }
}
