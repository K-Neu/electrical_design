namespace ElectricalDesigner.Domain.Common;

/// <summary>
/// Levée lorsqu'une opération de mutation du modèle (ex. ajouter un circuit,
/// créer un projet) violerait un invariant structurel. Contient la liste
/// complète des <see cref="ValidationIssue"/> détectées, afin qu'un appelant
/// (Application, App, tests) puisse les afficher toutes d'un coup plutôt que
/// de découvrir les erreurs une par une.
/// </summary>
public sealed class DomainValidationException : Exception
{
    public IReadOnlyList<ValidationIssue> Issues { get; }

    public DomainValidationException(IReadOnlyList<ValidationIssue> issues)
        : base(BuildMessage(issues))
    {
        Issues = issues;
    }

    private static string BuildMessage(IReadOnlyList<ValidationIssue> issues) =>
        issues.Count == 1
            ? issues[0].ToString()
            : $"{issues.Count} problème(s) de validation :{Environment.NewLine}"
              + string.Join(Environment.NewLine, issues.Select(i => $"  - {i}"));
}
