using ElectricalDesigner.Canvas.Geometry;
using ElectricalDesigner.Canvas.Scene;
using ElectricalDesigner.TestKit;

namespace ElectricalDesigner.Canvas.Tests.Scene;

public class CanvasObjectTests
{
    private static CanvasObject CreateSample() =>
        CanvasObject.Create(CanvasObjectId.New(), new Point2D(10, 20), new Size2D(30, 40));

    [Fact]
    public void Create_AvecIdentifiantVide_Echoue()
    {
        Assert.Throws<ArgumentException>(() =>
            CanvasObject.Create(CanvasObjectId.Empty, Point2D.Zero, new Size2D(10, 10)));
    }

    [Fact]
    public void Create_AvecTailleNegative_Echoue()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            CanvasObject.Create(CanvasObjectId.New(), Point2D.Zero, new Size2D(-1, 10)));
    }

    [Fact]
    public void MoveBy_DeplaceLaPosition()
    {
        var obj = CreateSample();

        obj.MoveBy(new Point2D(5, -5));

        Assert.Equal(15.0, obj.Position.X);
        Assert.Equal(15.0, obj.Position.Y);
    }

    [Fact]
    public void RotateBy_360Degres_RestitueLaGeometrieInitiale()
    {
        // Test de propriété (cahier §34.2) : une rotation de 360° restitue l'état initial.
        var obj = CreateSample();
        obj.SetRotation(37);

        obj.RotateBy(360);

        Assert.Equal(37.0, obj.RotationDegrees);
    }

    [Fact]
    public void SetRotation_NormaliseDansZeroTrisCentSoixante()
    {
        var obj = CreateSample();

        obj.SetRotation(-90);

        Assert.Equal(270.0, obj.RotationDegrees);
    }

    [Fact]
    public void Bounds_TientCompteDeLEchelle()
    {
        var obj = CreateSample();
        obj.SetScale(2.0);

        Assert.Equal(60.0, obj.Bounds.Width);
        Assert.Equal(80.0, obj.Bounds.Height);
    }

    [Fact]
    public void CloneAsNew_ProduitUnIdentifiantDistinctEtUnePositionDecalee()
    {
        var original = CreateSample();

        var clone = original.CloneAsNew(new Point2D(100, 0));

        Assert.False(clone.Id == original.Id);
        Assert.Equal(110.0, clone.Position.X);
        Assert.Equal(20.0, clone.Position.Y);
    }

    [Fact]
    public void CloneAsNew_NHeritePasDuVerrouillageDeLaSource()
    {
        var original = CreateSample();
        original.SetLocked(true);

        var clone = original.CloneAsNew(Point2D.Zero);

        Assert.False(clone.Locked);
    }

    [Fact]
    public void CloneAsNew_SansDemandeExpliciteDePreservation_EffaceLeLienMetier()
    {
        var original = CreateSample();
        original.LinkToBusinessObject(Guid.NewGuid());

        var clone = original.CloneAsNew(Point2D.Zero);

        Assert.True(clone.BusinessObjectId is null);
    }
}
