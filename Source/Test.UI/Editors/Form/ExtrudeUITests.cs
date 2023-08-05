﻿using Macad.Test.UI.Framework;
using NUnit.Framework;

namespace Macad.Test.UI.Editors.Form;

[TestFixture]
public class ExtrudeUITests: UITestBase
{
    [SetUp]
    public void SetUp()
    {
        Reset();
    }

    //--------------------------------------------------------------------------------------------------

    [Test]
    public void CreateFromSketch()
    {
        _CreateSketchBased();
        Assert.AreEqual("Extrude", Pipe.GetValue<string>("$Selected.Shape.Name"));
    }

    //--------------------------------------------------------------------------------------------------

    [Test]
    public void CreateFromSolid()
    {
        _CreateSolidBased();
        Assert.AreEqual("Extrude", Pipe.GetValue<string>("$Selected.Shape.Name"));
    }
    
    //--------------------------------------------------------------------------------------------------
        
    [Test]
    public void SymmetricOnSketch()
    {
        _CreateSketchBased();
        var panel = MainWindow.PropertyView.FindPanelByClass("ExtrudePropertyPanel");
        Assert.IsNotNull(panel);
        Assert.IsTrue(panel.ControlExists("Symmetric"));
        Assert.AreEqual(false, Pipe.GetValue<bool>("$Selected.Shape.Symmetric"));
        panel.ClickToggle("Symmetric");
        Assert.AreEqual(true, Pipe.GetValue<bool>("$Selected.Shape.Symmetric"));
        panel.ClickToggle("Symmetric");
        Assert.AreEqual(false, Pipe.GetValue<bool>("$Selected.Shape.Symmetric"));
    }

    //--------------------------------------------------------------------------------------------------
    
    [Test]
    public void SymmetricOnSolid()
    {
        _CreateSolidBased();
        var panel = MainWindow.PropertyView.FindPanelByClass("ExtrudePropertyPanel");
        Assert.IsNotNull(panel);
        Assert.IsFalse(panel.ControlExists("Symmetric"));
    }

    //--------------------------------------------------------------------------------------------------
        
    [Test]
    public void MergeFacesOnSketch()
    {
        _CreateSketchBased();
        var panel = MainWindow.PropertyView.FindPanelByClass("ExtrudePropertyPanel");
        Assert.IsNotNull(panel);
        Assert.IsFalse(panel.ControlExists("MergeFaces"));
    }

    //--------------------------------------------------------------------------------------------------
    
    [Test]
    public void MergeFacesOnSolid()
    {
        _CreateSolidBased();
        var panel = MainWindow.PropertyView.FindPanelByClass("ExtrudePropertyPanel");
        Assert.IsNotNull(panel);
        Assert.IsTrue(panel.ControlExists("MergeFaces"));
        Assert.AreEqual(true, Pipe.GetValue<bool>("$Selected.Shape.MergeFaces"));
        panel.ClickToggle("MergeFaces");
        Assert.AreEqual(false, Pipe.GetValue<bool>("$Selected.Shape.MergeFaces"));
        panel.ClickToggle("MergeFaces");
        Assert.AreEqual(true, Pipe.GetValue<bool>("$Selected.Shape.MergeFaces"));
    }

    //--------------------------------------------------------------------------------------------------
    //--------------------------------------------------------------------------------------------------
    
    void _CreateSketchBased()
    {
        TestDataGenerator.GenerateSketch(MainWindow);
        MainWindow.Ribbon.ClickButton("CloseSketchEditor");

        // Create on existing sketch
        MainWindow.Ribbon.SelectTab(RibbonTabs.Model);
        Assert.IsTrue(MainWindow.Ribbon.IsButtonEnabled("CreateExtrude"));
        MainWindow.Ribbon.ClickButton("CreateExtrude");
    }

    //--------------------------------------------------------------------------------------------------

    void _CreateSolidBased()
    {
        TestDataGenerator.GenerateBox(MainWindow);

        MainWindow.Ribbon.SelectTab(RibbonTabs.Model);
        Assert.IsTrue(MainWindow.Ribbon.IsButtonEnabled("CreateExtrude"));
        MainWindow.Ribbon.ClickButton("CreateExtrude");
        Assert.IsTrue(MainWindow.Ribbon.IsButtonChecked("CreateExtrude"));

        MainWindow.Viewport.ClickRelative(0.3, 0.33);
        Assert.IsFalse(MainWindow.Ribbon.IsButtonChecked("CreateExtrude"));
    }
}