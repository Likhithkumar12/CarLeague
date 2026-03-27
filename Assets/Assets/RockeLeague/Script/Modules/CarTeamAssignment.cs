using UnityEngine;
using Fusion;

/// <summary>
/// Attached to each car prefab.
/// Assigns team (0 = orange / 1 = blue) and applies a team colour material tint.
/// </summary>
public class CarTeamAssignment : NetworkBehaviour
{
    [Header("Team Colors")]
    [SerializeField] private Color team0Color = new Color(1f, 0.5f, 0f); // Orange
    [SerializeField] private Color team1Color = new Color(0.1f, 0.4f, 1f); // Blue

    [Header("Renderers to tint (body panels etc.)")]
    [SerializeField] private Renderer[] teamColorRenderers;

    [Header("Team Indicator UI (optional)")]
    [SerializeField] private GameObject teamIndicatorRoot;
    [SerializeField] private UnityEngine.UI.Image teamIndicatorImage;

    [Networked, OnChangedRender(nameof(OnTeamChanged))]
    public int Team { get; private set; } = -1;

    // ─── Called by NetworkManager on server ───────────────────────────────────

    public void SetTeam(int team)
    {
        Team = team; // Networked property — synced to all clients
    }

    // ─── Render callback (runs on all clients when Team changes) ─────────────

    private void OnTeamChanged()
    {
        ApplyTeamVisuals();
    }

    public override void Spawned()
    {
        ApplyTeamVisuals();
    }

    private void ApplyTeamVisuals()
    {
        if (Team < 0) return;

        Color teamColor = Team == 0 ? team0Color : team1Color;

        foreach (Renderer rend in teamColorRenderers)
        {
            if (rend == null) continue;

            // Clone material to avoid shared material modification
            Material mat = rend.material;
            mat.color = teamColor;
            // If using URP/HDRP lit shader:
            if (mat.HasProperty("_BaseColor"))
                mat.SetColor("_BaseColor", teamColor);
        }

        if (teamIndicatorImage != null)
            teamIndicatorImage.color = teamColor;

        if (teamIndicatorRoot != null)
            teamIndicatorRoot.SetActive(true);
    }

    // ─── Utility ──────────────────────────────────────────────────────────────

    public bool IsLocalPlayerTeam()
    {
        return Object.HasInputAuthority;
    }
}