namespace ElectricalDesigner.Domain.Common;

/// <summary>
/// Identifiant universel fortement typé (guide de développement §5.1) : tout objet
/// métier a un <c>id: UUID</c>. Le paramètre générique <typeparamref name="TEntity"/>
/// est un "type fantôme" — il n'est jamais instancié, il sert uniquement à empêcher le
/// compilateur d'accepter, par exemple, un <c>EntityId&lt;Circuit&gt;</c> là où un
/// <c>EntityId&lt;DistributionBoard&gt;</c> est attendu. Cela élimine par construction
/// toute une classe d'erreurs (transposition accidentelle d'identifiants).
///
/// La position graphique n'est jamais utilisée comme identité (guide §5.1) : ce type
/// est la SEULE façon de désigner une entité métier dans tout le code.
/// </summary>
public readonly record struct EntityId<TEntity>(Guid Value)
{
    public static EntityId<TEntity> New() => new(Guid.NewGuid());

    public static readonly EntityId<TEntity> Empty = new(Guid.Empty);

    public bool IsEmpty => Value == Guid.Empty;

    public override string ToString() => Value.ToString();
}
