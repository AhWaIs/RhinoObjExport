using System;
using Rhino;
using Rhino.Commands;
using Rhino.DocObjects;
using Rhino.Input.Custom;

namespace RhinoObjExport
{
    public class CompareVisualizeMeshes : Command
    {
        public CompareVisualizeMeshes()
        {
            Instance = this;
        }

        ///<summary>The only instance of the MyCommand command.</summary>
        public static CompareVisualizeMeshes Instance { get; private set; }

        public override string EnglishName => "CompareVisualizeMeshes";


        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {

            var have_preselected_objects = false;

            var go = new GetObject();
            go.SetCommandPrompt("Select objects");
            go.GeometryFilter = ObjectType.AnyObject;
            go.GroupSelect = true;
            go.SubObjectSelect = false;

            while (true)
            {
                go.Get();

                if (go.CommandResult() != Result.Success)
                    return go.CommandResult();

                if (go.ObjectsWerePreselected)
                {
                    have_preselected_objects = true;
                    go.EnablePreSelect(false, true);
                    go.AlreadySelectedObjectSelect = true;
                    go.EnableClearObjectsOnEntry(false);
                    go.DeselectAllBeforePostSelect = false;
                    go.EnableUnselectObjectsOnExit(false);
                    continue;
                }
                var firstObj = go.Objects()[0];

                break;
            }

            int objCount = go.ObjectCount;
            RhinoApp.WriteLine("Select Existing Object");
            go.Get();

            var secondObj = go.Objects()[0];


            if (have_preselected_objects)
            {
                for (var i = 0; i < go.ObjectCount; i++)
                {
                    var obj = go.Object(i).Object();
                    obj?.Select(false);
                }

                doc.Views.Redraw();
            }

            RhinoApp.WriteLine("Select object count: {0}", go.ObjectCount);

            return Result.Success;
        }
    }
}