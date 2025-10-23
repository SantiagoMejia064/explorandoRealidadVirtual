using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class AttachPointSwitcher : MonoBehaviour
{
    public XRGrabInteractable grab;
    public Transform leftAttach;
    public Transform rightAttach;

    [Header("Interactors (arrastra tus manos)")]
    public MonoBehaviour leftHand;   // Left Near-Far Interactor (o XR Direct Interactor)
    public MonoBehaviour rightHand;  // Right Near-Far Interactor (o XR Direct Interactor)

    Transform defaultAttach;

    void Awake()
    {
        if (!grab) grab = GetComponent<XRGrabInteractable>();
        if (grab.attachTransform == null)
        {
            var t = new GameObject("DefaultAttach").transform;
            t.SetParent(transform, false);
            grab.attachTransform = t;
        }
        defaultAttach = grab.attachTransform;
    }

    // ENLAZA ESTE MÉTODO a First Select Entered Y (por compatibilidad) a Select Entered
    public void OnSelectEntered(SelectEnterEventArgs args)
    {
        // 1) ¿Quién agarró?
        var interactorTr = args.interactorObject?.transform;
        var leftTr  = (leftHand  as Component)?.transform;
        var rightTr = (rightHand as Component)?.transform;

        // 2) Decide attach
        Transform chosen = defaultAttach;
        if (interactorTr && leftTr  && (interactorTr == leftTr  || interactorTr.IsChildOf(leftTr)))  chosen = leftAttach  ? leftAttach  : defaultAttach;
        if (interactorTr && rightTr && (interactorTr == rightTr || interactorTr.IsChildOf(rightTr))) chosen = rightAttach ? rightAttach : defaultAttach;

        grab.attachTransform = chosen;

        // 3) Reencajar inmediatamente para que el nuevo attach quede alineado con el de la mano
        var interAttach = GetInteractorAttachTransform(args);
        if (interAttach != null && chosen != null)
        {
            // A.world = G.world * A.local  =>  G.world = I.world * Inverse(A.local)
            Quaternion worldRot = interAttach.rotation * Quaternion.Inverse(chosen.localRotation);
            Vector3 worldPos    = interAttach.position - (worldRot * chosen.localPosition);

            // Mover el objeto (sin físicas bruscas)
            transform.SetPositionAndRotation(worldPos, worldRot);
        }
    }

    // (Opcional) Restaurar al soltar
    public void OnSelectExited(SelectExitEventArgs _)
    {
        grab.attachTransform = defaultAttach;
    }

    // Compatibilidad XRI 2.x / 3.x para obtener el attach de la mano
    Transform GetInteractorAttachTransform(SelectEnterEventArgs args)
    {
#if XRI_3 // si tu proyecto define un símbolo, ignora esto: también funciona sin él
        if (args.interactorObject is IXRInteractor i) return i.GetAttachTransform(grab);
#endif
        var xrb = args.interactorObject as XRBaseInteractor;
        if (xrb != null) return xrb.attachTransform;
        // Último recurso: el transform del interactor
        return args.interactorObject?.transform;
    }
}
