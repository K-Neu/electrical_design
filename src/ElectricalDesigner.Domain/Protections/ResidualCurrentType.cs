namespace ElectricalDesigner.Domain.Protections;

/// <summary>
/// Type de courant différentiel résiduel détecté (cahier §21.5). Pertinent
/// uniquement quand <see cref="ProtectionDevice.Type"/> vaut
/// <see cref="ProtectionType.ResidualCurrentDevice"/>.
/// </summary>
public enum ResidualCurrentType
{
    Ac,
    A,
    F,
    B,
}
