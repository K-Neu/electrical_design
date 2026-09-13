using ElectricalDesigner.Domain.Boards;
using ElectricalDesigner.Domain.Circuits;
using ElectricalDesigner.Domain.Common;
using ElectricalDesigner.TestKit;

namespace ElectricalDesigner.Domain.Tests.Common;

public class EntityIdTests
{
    [Fact]
    public void New_GenereUnIdentifiantNonVide()
    {
        var id = EntityId<Circuit>.New();

        Assert.False(id.IsEmpty);
    }

    [Fact]
    public void New_GenereDesIdentifiantsDistincts()
    {
        var first = EntityId<Circuit>.New();
        var second = EntityId<Circuit>.New();

        Assert.False(first == second);
    }

    [Fact]
    public void Empty_EstBienVide()
    {
        Assert.True(EntityId<Circuit>.Empty.IsEmpty);
    }

    [Fact]
    public void DeuxIdentifiantsDeTypesDifferents_NeSontPasComparables()
    {
        // Ce test vérifie surtout, à la compilation, que EntityId<Circuit> et
        // EntityId<DistributionBoard> sont des types distincts : le code
        // suivant ne compilerait pas si ce n'était pas le cas.
        var circuitId = EntityId<Circuit>.New();
        var boardId = EntityId<DistributionBoard>.New();

        Assert.Equal(circuitId.Value.GetType(), boardId.Value.GetType());
        Assert.False(circuitId.GetType() == boardId.GetType());
    }
}
