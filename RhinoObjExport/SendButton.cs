using System;
using Rhino;
using Rhino.Commands;
using Rhino.DocObjects;
using Rhino.Input.Custom;

namespace RhinoObjExport
{
    public class SendButton : Command
    {
        public SendButton()
        {
            Instance = this;
        }

        ///<summary>The only instance of the MyCommand command.</summary>
        public static SendButton Instance { get; private set; }

        public override string EnglishName => "SendButton";

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




                var file = Rhino.FileIO.File3dm.Read("C:\\Users\\waelext\\Documents\\Hackathon\\column only.3dm");
                var impObj = file.Objects.FindId(firstObj.ObjectId);
                if (impObj != null)
                {

                    var id = doc.Objects.Add(impObj.Geometry, impObj.Attributes);
                    doc.Views.Redraw();
                    var result = Eto.Forms.MessageBox.Show(
                    "Send modification",
                    "Identity",
                    Eto.Forms.MessageBoxButtons.YesNo,
                    Eto.Forms.MessageBoxType.Question);
                    if (result == Eto.Forms.DialogResult.Yes)
                    {
                        doc.Objects.Hide(id, true);
                        doc.Views.Redraw();
                    }
                }


                break;
            }

            //int objCount = go.ObjectCount;
            RhinoApp.WriteLine("Select Existing Object");



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
            // TODO: complete command.
            return Result.Success;
        }
    }
}