﻿ using Macad.Common;
using Macad.Core.Shapes;
using Macad.Interaction.Panels;
using Macad.Occt;
using System.Windows.Input;
using Macad.Core;

namespace Macad.Interaction.Editors.Shapes;

public class ImprintEditor : Editor<Imprint>
{
    TranslateAxisLiveAction _TranslateAction;
    LabelHudElement _HudElement;
    bool _IsMoving;
    double _StartDepth;

    //--------------------------------------------------------------------------------------------------

    protected override void OnStart()
    {
        var panel = CreatePanel<ImprintPropertyPanel>(Entity, PropertyPanelSortingKey.Shapes);

        if (Entity.Sketch != null)
        {
            CreatePanel<SketchPropertyPanel>(Entity.Sketch, panel);
        }
    }
                
    //--------------------------------------------------------------------------------------------------

    protected override void OnToolsStart()
    {
        Shape.ShapeChanged += _Shape_ShapeChanged;
        _UpdateActions();
    }

    //--------------------------------------------------------------------------------------------------

    protected override void OnToolsStop()
    {
        _HudElement = null;
        _TranslateAction = null;
        Shape.ShapeChanged -= _Shape_ShapeChanged;              
    }

    //--------------------------------------------------------------------------------------------------

    void _Shape_ShapeChanged(Shape shape)
    {
        if (shape == Entity)
        {
            _UpdateActions();
        }
    }

    //--------------------------------------------------------------------------------------------------

    public override void EnrichContextMenu(ContextMenuItems itemList)
    {
        if (Entity.Sketch != null)
            itemList.AddCommand(SketchCommands.StartSketchEditor, Entity.Sketch);
    }

    //--------------------------------------------------------------------------------------------------

    #region Live Actions

    void _UpdateActions()
    {
        if (Entity?.Body == null
            || Entity.Mode == Imprint.ImprintMode.Cutout
            || !Entity.GetFinalExtrusionAxis(out Ax1 axis))
        {
            StopAllActions();
            _TranslateAction = null;
            return;
        }
        axis.Transform(Entity.Body.GetTransformation());

        if (_TranslateAction == null)
        {
            _TranslateAction = new()
            {
                Color = Colors.ActionBlue,
                Cursor = Cursors.SetHeight,
                NoResize = true,
                Length = 1.0,
            };
            _TranslateAction.Preview += _TranslateAction_Preview;
            _TranslateAction.Finished += _TranslateActionFinished;
            StartAction(_TranslateAction);
        }

        _TranslateAction.Axis = axis;
        if (!_IsMoving)
        {
            _StartDepth = Entity.Depth;
        }
    }

    //--------------------------------------------------------------------------------------------------

    void _TranslateAction_Preview(TranslateAxisLiveAction sender, TranslateAxisLiveAction.EventArgs args)
    {
        _IsMoving = true;
        SetHintMessage("Scale imprint depth using gizmo, press 'CTRL' to round to grid stepping.");

        double newDepth = _StartDepth + (Entity.Mode == Imprint.ImprintMode.Raise
                                             ? args.Distance
                                             : -args.Distance);

        if (args.MouseEventData.ModifierKeys.HasFlag(ModifierKeys.Control))
        {
            newDepth = Maths.RoundToNearest(newDepth, WorkspaceController.Workspace.GridStep);
        }

        if (newDepth < 0)
        {
            Entity.Mode = Entity.Mode == Imprint.ImprintMode.Raise ? Imprint.ImprintMode.Lower : Imprint.ImprintMode.Raise;
            _StartDepth = -_StartDepth;
            newDepth = -newDepth;
        }

        Entity.Depth = newDepth.Abs();

        _UpdateActions();
        if (_HudElement == null)
        {
            _HudElement = new LabelHudElement();
            Add(_HudElement);
        }
        _HudElement?.SetValue($"Depth: {Entity.Depth.ToInvariantString("F2")} mm");
    }

    //--------------------------------------------------------------------------------------------------

    void _TranslateActionFinished(TranslateAxisLiveAction sender, TranslateAxisLiveAction.EventArgs args)
    {
        _IsMoving = false;
        CommitChanges();
        _UpdateActions();
    }
    
    //--------------------------------------------------------------------------------------------------

    #endregion

    [AutoRegister]
    internal static void Register()
    {
        RegisterEditor<ImprintEditor>();
    }

    //--------------------------------------------------------------------------------------------------
}