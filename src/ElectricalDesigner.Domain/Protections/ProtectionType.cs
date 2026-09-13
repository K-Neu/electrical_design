namespace ElectricalDesigner.Domain.Protections;

/// <summary>Cahier §9 : "Disjoncteur, fusible, différentiel, parafoudre ou protection spécialisée."</summary>
public enum ProtectionType
{
    CircuitBreaker,
    Fuse,
    ResidualCurrentDevice,
    SurgeProtectionDevice,
    Specialized,
}
