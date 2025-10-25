using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class Pistola : MonoBehaviour
{
    public GameObject ShootFx, HitFx;
    public Transform firePoint;
    public LineRenderer line;
    public int damage = 25;

    [Header("Puntos de anclaje")]
    [SerializeField] private Transform attachPointWeapon;

    //Empty que debe tener la mano izquierda al agarrar este arma
    [SerializeField] private Transform leftHandPose;
    //Empty que debe tener la mano derecha al agarrar este arma
    [SerializeField] private Transform rightHandPose;

    //esta variable es la referencia al componente de XRI para ser agarrado
    private XRGrabInteractable grab;
    private Rigidbody rb;

    //Este diccionario es para recordar el attach original de cada interactor (osea de cada mano) y restaurarlo al soltar
    private readonly Dictionary<XRBaseInteractor, Transform> originalAttachByInteractor = new();

    void Awake()
    {
        grab = GetComponent<XRGrabInteractable>();
        rb = GetComponent<Rigidbody>();
    }


    void OnEnable()
    {/*
        grab.hoverEntered.AddListener(OnHoverEntered); //Se suscribe al evento cuando una mano entra en hover (osea antes de agarrar)
        grab.selectEntered.AddListener(OnSelectEntered); //Se suscribe al evento cuando una mano agarra el arma
        grab.selectExited.AddListener(OnSelectExited); //Este es para cuando suelta el arma
        */
        grab.activated.AddListener(OnActivated); //El evento de activación para disparar
/*
        //Esta es una configuración estable de agarre:
        //No se usa attach dinámico porque quiero snap exacto a los puntos definidos
        grab.useDynamicAttach = false;

        //Se reduce la interpolación de encaje para evitar “flotar” el primer frame
        grab.attachEaseInTime = Mathf.Min(grab.attachEaseInTime, 0.05f);

        //Aseguramos que el XRGrabInteractable apunte al attach FIJO del arma
        if (grab.attachTransform != attachPointWeapon && attachPointWeapon != null)
            grab.attachTransform = attachPointWeapon;*/
    }

    void OnDisable()
    {
        //Quitamos las suscripciones para evitar fugas o duplicados por si acaso
        //grab.hoverEntered.RemoveListener(OnHoverEntered);
        //grab.selectEntered.RemoveListener(OnSelectEntered);
        //grab.selectExited.RemoveListener(OnSelectExited);
        grab.activated.RemoveListener(OnActivated);
    }
/**
    //Antes de agarrar, definimos qué pose/attach debe usar la MANO que está tocando
    private void OnHoverEntered(HoverEnterEventArgs args)
    {
        //Obtenemos el transform del interactor o de la mano que entra en contacto
        var interactorTr = args.interactorObject.transform;

        //Intentamos obtener el componente base del interactor (o de la mano)
        if (!interactorTr.TryGetComponent<XRBaseInteractor>(out var interactor))
            return; //Si no hay interactor válido, salimos del metodo

        //Detectamos si es la mano derecha por nombre
        bool isRight = interactorTr.name.Contains("Right") || interactorTr.name.Contains("right");

        //Aqui elegimos la pose objetivo de la mano según sea derecha o izquierda
        Transform targetHandPose = isRight ? rightHandPose : leftHandPose;
        if (targetHandPose == null) return;

        //Guardamos el attach original solo una vez
        if (!originalAttachByInteractor.ContainsKey(interactor))
            originalAttachByInteractor[interactor] = interactor.attachTransform;

        //Y Cambiamos el punto de attach de dicha mano a la pose adecuada para este arma
        interactor.attachTransform = targetHandPose;
    }

    //Cuando el arma ya fue agarrada, se reencaja al final del frame por seguridad
    private void OnSelectEntered(SelectEnterEventArgs _)
    {
        //Si se tiene un attach válido del arma, iniciamos la rutina de re-snap
        if (grab.attachTransform != null)
            StartCoroutine(ResnapAtEndOfFrame());
    }

    //Metodo para que al soltar el arma restauramos el attach original de la mano
    private void OnSelectExited(SelectExitEventArgs args)
    {
        //Cogemos el transform de la mano que soltó
        var interactorTr = args.interactorObject.transform;

        //Validamos si la mano tiene XRBaseInteractor
        if (interactorTr.TryGetComponent<XRBaseInteractor>(out var interactor))
        {
            //Buscamos su attach original y se restaura (segun vi, es para no afectar otros objetos)
            if (originalAttachByInteractor.TryGetValue(interactor, out var original))
                interactor.attachTransform = original;
        }
    }

    //Corrutina que espera a que termine el frame y hace “re-snap” exacto al attach del arma
    private IEnumerator ResnapAtEndOfFrame()
    {
        //Esperamos hasta el final del frame actual
        yield return new WaitForEndOfFrame();

        //Obtenemos el attach actual del arma
        var at = grab.attachTransform;
        if (at == null) yield break; //Si no hay, termina

        //Luego colocamos la pistola exactamente en posición y rotación del attach del arma
        transform.SetPositionAndRotation(at.position, at.rotation);

        //Por si acasom, validamos si tiene Rigidbody y NO es cinemático, se sincroniza físicas
        if (rb && !rb.isKinematic)
        {
            //Ponemos en cero velocidades para evitar arrastres
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            //Y se mueve rígidamente a la posición y rotación objetivo
            rb.MovePosition(at.position);
            rb.MoveRotation(at.rotation);
        }
    }
*/
    //Metodo que es el evento de activación, osea que inicie el disparo
    private void OnActivated(ActivateEventArgs _)
    {
        StartCoroutine(Disparo());
    }

    private IEnumerator Disparo()
    {
        RaycastHit hit;
        bool hitInfo = Physics.Raycast(firePoint.position, firePoint.forward, out hit, 50f);
        Instantiate(ShootFx, firePoint.position, Quaternion.identity);

        if (hitInfo)
        {
            line.SetPosition(0, firePoint.position);
            line.SetPosition(1, hit.point);
            Instantiate(HitFx, hit.point, Quaternion.identity);
        }
        else
        {
            line.SetPosition(0, firePoint.position);
            line.SetPosition(1, firePoint.position + firePoint.forward * 20f);
        }

        line.enabled = true;
        yield return new WaitForSeconds(0.02f);
        line.enabled = false;
    }

}
