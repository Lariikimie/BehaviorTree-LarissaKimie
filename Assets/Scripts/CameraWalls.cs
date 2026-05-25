using System.Collections.Generic;
using UnityEngine;

public class CameraWalls : MonoBehaviour
{
    [Header("Refs")]
    public Transform player;

    [Header("Occlusion")]
    public LayerMask occluderMask = ~0;     // Ajuste se quiser ignorar camadas
    public float transparentAlpha = 0.3f;   // Alpha alvo quando obstruindo

    [Header("Fade")]
    public float transitionAlpha = 0.5f;    // Duração (s) do fade linear

    Camera cam;

    class Fader
    {
        public Renderer r;
        public Color baseColor;     // cor original (inclui alpha original)
        public float currentAlpha;  // alpha atual aplicado
        public float targetAlpha;   // alpha desejado (0.3 quando bloqueando, 1.0 quando livre)
    }

    // Um Fader por Renderer (permite múltiplos objetos simultaneamente)
    readonly Dictionary<Renderer, Fader> faders = new();

    void Start()
    {
        cam = Camera.main;
    }

    void Update()
    {
        if (!player || !cam) return;

        Vector3 origin = cam.transform.position;
        Vector3 toPlayer = player.position - origin;
        float dist = toPlayer.magnitude;
        if (dist <= 0.0001f) return;

        Vector3 dir = toPlayer / dist;

        // Coleta todos os bloqueadores entre câmera e player
        RaycastHit[] hits = Physics.RaycastAll(origin, dir, dist, occluderMask, QueryTriggerInteraction.Ignore);

        // Conjunto de Renderers que DEVEM estar transparentes neste frame
        HashSet<Renderer> nowBlocked = new();

        foreach (var hit in hits)
        {
            // Ignora o próprio player (e hierarquia dele)
            if (hit.transform == player || hit.transform.IsChildOf(player)) continue;

            // Pode haver múltiplos renderers por objeto
            var renderers = hit.transform.GetComponentsInChildren<Renderer>();
            foreach (var r in renderers)
            {
                if (!r) continue;
                nowBlocked.Add(r);

                if (!faders.TryGetValue(r, out var f))
                {
                    var mat = r.material; // nota: isso instancia material p/ este renderer
                    Color baseColor = mat.HasProperty("_BaseColor")
                        ? mat.GetColor("_BaseColor")
                        : mat.HasProperty("_Color") ? mat.GetColor("_Color")
                        : Color.white;

                    f = new Fader
                    {
                        r = r,
                        baseColor = baseColor,
                        currentAlpha = baseColor.a,
                        targetAlpha = baseColor.a
                    };
                    faders.Add(r, f);
                }

                // Este renderer está obstruindo: objetivo = transparente
                f.targetAlpha = transparentAlpha;
            }
        }

        // Todos os que não estão mais bloqueando devem voltar para opaco (alpha original, tipicamente 1)
        foreach (var kv in faders)
        {
            if (!nowBlocked.Contains(kv.Key))
            {
                kv.Value.targetAlpha = 1f; // ou kv.Value.baseColor.a se quiser respeitar o alpha original
            }
        }

        // Passo linear por segundo para alcançar o alvo em 'transitionAlpha' segundos
        float step = (transitionAlpha <= 0f) ? 1f : Time.deltaTime / transitionAlpha;

        // Atualiza os fades e remove quem já foi restaurado
        List<Renderer> toRemove = null;

        foreach (var kv in faders)
        {
            var f = kv.Value;

            // MoveTowards garante transição linear, sem overshoot
            f.currentAlpha = Mathf.MoveTowards(f.currentAlpha, f.targetAlpha, step);

            // Aplica alpha atual
            var mat = f.r.material;
            if (mat.HasProperty("_BaseColor"))
            {
                Color c = f.baseColor; c.a = f.currentAlpha;
                mat.SetColor("_BaseColor", c);
            }
            else if (mat.HasProperty("_Color"))
            {
                Color c = f.baseColor; c.a = f.currentAlpha;
                mat.SetColor("_Color", c);
            }

            // Se voltou pro opaco, podemos limpar da tabela (opcional)
            if (Mathf.Approximately(f.currentAlpha, 1f) && Mathf.Approximately(f.targetAlpha, 1f))
            {
                // Restaura exatamente a cor base e remove
                if (toRemove == null) toRemove = new List<Renderer>();
                toRemove.Add(kv.Key);
            }
        }

        if (toRemove != null)
        {
            foreach (var r in toRemove)
            {
                var f = faders[r];
                var mat = r.material;
                if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", f.baseColor);
                else if (mat.HasProperty("_Color")) mat.SetColor("_Color", f.baseColor);
                faders.Remove(r);
            }
        }

        Debug.DrawRay(origin, dir * dist, Color.red);
    }
}
