namespace ElectricalDesigner.TestKit;

/// <summary>
/// Marque une méthode comme test unitaire, découverte et exécutée par
/// <see cref="TestRunner"/> via réflexion.
///
/// Pourquoi un mini-framework "maison" plutôt que xUnit ?
/// --------------------------------------------------------
/// Le guide de développement recommande xUnit (guide §31 / cahier §31.1).
/// Cependant, l'environnement d'exécution utilisé pour amorcer ce dépôt n'a
/// pas accès à nuget.org (réseau restreint), donc aucun package NuGet ne
/// peut être restauré ici. Pour respecter le critère de sortie de la
/// Phase 0 ("exécuter les tests"), ce mini-framework reproduit délibérément
/// l'API la plus simple de xUnit : un attribut [Fact] et une classe Assert.
///
/// Migration prévue : dès que le dépôt tourne dans un environnement avec
/// accès NuGet (CI GitHub Actions notamment, cf. .github/workflows/ci.yml),
/// remplacer ce dossier TestKit par le package Microsoft.NET.Test.Sdk +
/// xunit + xunit.runner.visualstudio. Le remplacement est mécanique :
/// - `using ElectricalDesigner.Domain.Tests.TestKit;` → `using Xunit;`
/// - les méthodes [Fact] et les appels Assert.* restent syntaxiquement
///   identiques dans l'immense majorité des cas.
/// </summary>
[AttributeUsage(AttributeTargets.Method, Inherited = false)]
public sealed class FactAttribute : Attribute
{
    /// <summary>Raison pour laquelle un test est ignoré, si renseignée (miroir de xUnit Skip).</summary>
    public string? Skip { get; init; }
}
