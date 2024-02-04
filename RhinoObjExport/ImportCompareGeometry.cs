using System;
using System.Linq;
using Rhino;
using Rhino.Commands;
using Rhino.Display;
using Rhino.DocObjects;
using Rhino.FileIO;
using Rhino.Geometry;

namespace RhinoObjExport
{
    public class ImportCompareGeometry : Command
    {
        public ImportCompareGeometry()
        {
            Instance = this;
        }

        ///<summary>The only instance of the MyCommand command.</summary>
        public static ImportCompareGeometry Instance { get; private set; }

        public override string EnglishName => "ImportCompareGeometry";

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            var fileReadOptions = new FileReadOptions();
            fileReadOptions.UseScaleGeometry = true;
            fileReadOptions.BatchMode = true;
            //var objOptions = new FileObjReadOptions(fileReadOptions);
            //doc.Import("C:\\Users\\waelext\\Documents\\Hackathon\\A_moved.3dm");

            var incomingFile = Rhino.FileIO.File3dm.Read("C:\\Users\\waelext\\Documents\\Hackathon\\A_moved.3dm");
            bool exists = false;
            Brep existingBrep = null, incominBrep = null;

            foreach (var obj in incomingFile.Objects)
            {
                RhinoObject rhinoObject = doc.Objects.FindId(obj.Id);

                if (rhinoObject != null)
                {
                    existingBrep = Brep.TryConvertBrep(rhinoObject.Geometry);
                    incominBrep = Brep.TryConvertBrep(obj.Geometry);
                    if (existingBrep != null && incominBrep != null)
                    {
                        
                        //rhinoObject.Attributes.ObjectColor = System.Drawing.Color.Red;
                        //rhinoObject.Attributes.ColorSource = ObjectColorSource.ColorFromObject;
                        //rhinoObject.Attributes.PlotColor = System.Drawing.Color.Red;

                        doc.Views.Redraw();

                        var inCenter = Rhino.Geometry.AreaMassProperties.Compute(incominBrep).Centroid;
                        var existingCenter = Rhino.Geometry.AreaMassProperties.Compute(existingBrep).Centroid;
                        if (inCenter.DistanceTo(existingCenter) > doc.ModelAbsoluteTolerance)
                        {

                            var deletingAttributes = new ObjectAttributes();
                            deletingAttributes.ColorSource = ObjectColorSource.ColorFromObject;
                            deletingAttributes.PlotColor = System.Drawing.Color.Red;
                            deletingAttributes.ObjectColor = System.Drawing.Color.Red;
                            var dupId = doc.Objects.AddBrep(existingBrep, deletingAttributes);
                            
                            rhinoObject.Attributes = deletingAttributes;

                            Layer incomingLayer = new Layer();
                            incomingLayer.Name = "incomingObjs";
                            int layerIndex = doc.Layers.Add(incomingLayer);
                            var attributes = new ObjectAttributes();
                            attributes.ColorSource = ObjectColorSource.ColorFromObject;
                            attributes.PlotColor = System.Drawing.Color.Green;
                            attributes.LayerIndex = layerIndex;
                            attributes.ObjectColor = System.Drawing.Color.Green;


                            doc.Objects.Delete(rhinoObject);

                            var id = doc.Objects.AddBrep(incominBrep, attributes);
                            doc.Views.Redraw();

                            Eto.Forms.MessageBox.Show("Moved Geometry, Accept incoming Change ?", "Identity", Eto.Forms.MessageBoxType.Question);

                            doc.Objects.Hide(rhinoObject.Id, true);//TODO
                            doc.Objects.Hide(dupId, true);//TODO
                            doc.Views.Redraw();


                            //newObj.Attributes.LayerIndex = 0;


                        }
                    }
                    exists = true;
                }
                else
                {
                    exists = false;
                }
            }

            return Result.Success;
        }
    }
}