using UnityEngine;

public class boton : MonoBehaviour
{
    [Header("Animación puertas")]
    [SerializeField] private Animator puerta1;
    [SerializeField] private Animator puerta2;
    [SerializeField] private AudioSource sonidoPuerta;

    private bool estadoPuertas = false;

    private void AbrirPuertas()
    {
        sonidoPuerta.Play();
        puerta1.Play("abriPuertaIzq");
        puerta2.Play("abriPuertaDer");

    }

    public void AccionPuertas()
    {
        if (estadoPuertas == false)
        {
            AbrirPuertas();
            estadoPuertas = true;
        }
        else
        {
            CerrarPuertas();
            estadoPuertas = false;
        }
    }

    private void CerrarPuertas()
    {
        sonidoPuerta.Play();
        puerta1.Play("cerrarPuertaIzq");
        puerta2.Play("cerrarPuertaDer");
    }
}
