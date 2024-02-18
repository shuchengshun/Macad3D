﻿using System;
using System.Collections.Generic;
using System.IO;
using Macad.Common.Serialization;
using Macad.Core;
using Macad.Core.Drawing;
using Macad.Core.Shapes;
using Macad.Exchange.Svg;
using Macad.Occt;

namespace Macad.Exchange;

public sealed class SvgExchanger : ISketchExporter, ISketchImporter, IDrawingExporter
{
    #region Exchanger

    public string Description { get; } = "Scalable Vector Graphics";
    public string[] Extensions { get; } = {"svg"};
                
    //--------------------------------------------------------------------------------------------------

    IExchangerSettings IExchanger.Settings
    {
        get { return Settings; }
        set
        {
            if (value is SvgSettings newSettings)
                Settings = newSettings;
        }
    }

    //--------------------------------------------------------------------------------------------------

    public bool CanExportToClipboard()
    {
        return true;
    }

    //--------------------------------------------------------------------------------------------------

    public bool CanImportFromClipboard(Clipboard clipboard)
    {
        return clipboard.ContainsData("image/svg+xml") || clipboard.ContainsData("image/x-inkscape-svg");
    }

    //--------------------------------------------------------------------------------------------------

    [AutoRegister]
    internal static void Register()
    {
        ExchangeRegistry.Register(new SvgExchanger());
    }

    //--------------------------------------------------------------------------------------------------

    #endregion

    #region Settings

    [SerializeType]
    public class SvgSettings : IExchangerSettings
    {
        [SerializeMember]
        public double DotsPerInch { get; set; } = 96.0;

        [SerializeMember]
        public bool TagGroupsAsLayers { get; set; } = false;
    }

    //--------------------------------------------------------------------------------------------------

    public SvgSettings Settings { get; private set; } = new SvgSettings();

    //--------------------------------------------------------------------------------------------------

    #endregion

    #region Helper

    bool _WriteToFile(string fileName, MemoryStream content)
    {
        if (content.Length > 0)
        {
            try
            {
                content.Position = 0;
                using (var output = new FileStream(fileName, FileMode.Create))
                {
                    content.CopyTo(output);
                }
                return true;
            }
            catch (Exception e)
            {
                Messages.Exception("Writing SVG to file " + fileName + " failed.", e);
            }
        }
        return false;
    }

    //--------------------------------------------------------------------------------------------------

    bool _WriteToClipboard(Clipboard clipboard, MemoryStream content)
    {
        // Known clipboard formats:
        // image/svg+xml
        // image/x-inkscape-svg
        if (content.Length > 0)
        {
            try
            {
                clipboard.SetData("image/svg+xml", content);
                return true;
            }
            catch (Exception e)
            {
                Messages.Exception("Writing SVG to clipboard failed.", e);
                return false;
            }
        }
        return false;
    }

    //--------------------------------------------------------------------------------------------------
        
    public static bool _ReadFromFile(string fileName, out Stream content)
    {
        try
        {
            var contentBytes = File.ReadAllBytes(fileName);
            if (contentBytes.Length > 0)
            {
                content = new MemoryStream(contentBytes);
                return true;
            }

            Messages.Error("Reading SVG from file " + fileName + " failed.");
        }
        catch (Exception e)
        {
            Messages.Exception("Reading SVG from file " + fileName + " failed.", e);
        }

        content = null;
        return false;
    }

    //--------------------------------------------------------------------------------------------------

    public static bool _ReadFromClipboard(Clipboard clipboard, out Stream content)
    {
        // Known clipboard formats:
        // image/svg+xml
        // image/x-inkscape-svg

        content = null;

        if (clipboard.ContainsData("image/svg+xml"))
        {
            content = clipboard.GetDataStream("image/svg+xml");
            return true;
        }
        else if (clipboard.ContainsData("image/x-inkscape-svg"))
        {
            content = clipboard.GetDataStream("image/x-inkscape-svg");
            return true;
        }

        Messages.Error("Reading SVG from clipboard failed.");
        return false;
    }

    //--------------------------------------------------------------------------------------------------

    #endregion

    #region ISketchExporter

    bool ISketchExporter.DoExport(string fileName, Sketch sketch)
    {
        bool result;
        using (new ProcessingScope(sketch, "Exporting sketch to SVG"))
        {
            SvgSketchExporter.DotsPerInch = Settings.DotsPerInch;
            result = _WriteToFile(fileName, SvgSketchExporter.Export(sketch));
        }

        return result;
    }

    //--------------------------------------------------------------------------------------------------

    bool ISketchExporter.DoExport(Clipboard clipboard, Sketch sketch)
    {
        bool result;
        using (new ProcessingScope(sketch, "Exporting sketch to SVG"))
        {
            SvgSketchExporter.DotsPerInch = Settings.DotsPerInch;
            result = _WriteToClipboard(clipboard, SvgSketchExporter.Export(sketch));
            CoreContext.Current?.MessageHandler.OnProcessingStopped();
        }

        return result;
    }

    //--------------------------------------------------------------------------------------------------

    #endregion

    #region ISketchImporter

    bool ISketchImporter.DoImport(string fileName, out IDictionary<int, Pnt2d> points, out IDictionary<int, SketchSegment> segments, out IEnumerable<SketchConstraint> constraints)
    {
        points = null;
        segments = null;
        constraints = null;

        bool result = false;
        using (new ProcessingScope(null, "Importing sketch from SVG"))
        {
            if (_ReadFromFile(fileName, out var content))
            {
                result = SvgSketchImporter.Import(content, out points, out segments);
            }
        }

        return result;
    }

    //--------------------------------------------------------------------------------------------------

    bool ISketchImporter.DoImport(Clipboard clipboard, out IDictionary<int, Pnt2d> points, out IDictionary<int, SketchSegment> segments, out IEnumerable<SketchConstraint> constraints)
    {
        points = null;
        segments = null;
        constraints = null;

        bool result = false;
        using (new ProcessingScope(null, "Importing sketch from SVG"))
        {
            if (_ReadFromClipboard(clipboard, out var content))
            {
                result = SvgSketchImporter.Import(content, out points, out segments);
            }
        }

        return result;
    }

    //--------------------------------------------------------------------------------------------------

    #endregion

    #region IDrawingExporter
        
    bool IDrawingExporter.DoExport(string fileName, Drawing drawing)
    {
        bool result;
        using (new ProcessingScope(null, "Exporting drawing to SVG"))
        {
            SvgExporterBase.DotsPerInch = Settings.DotsPerInch;
            SvgExporterBase.TagGroupsAsLayers = Settings.TagGroupsAsLayers;
            result = _WriteToFile(fileName, SvgDrawingExporter.Export(drawing));
        }

        return result;
    }

    #endregion
}