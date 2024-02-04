using Rhino;
using Rhino.Commands;
using Rhino.FileIO;
using Rhino.Geometry;
using Rhino.Input;
using Rhino.Input.Custom;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RhinoObjExport
{
    public class RhinoObjExportCommand : Command
    {
        public RhinoObjExportCommand()
        {
            // Rhino only creates one instance of each command class defined in a
            // plug-in, so it is safe to store a refence in a static property.
            Instance = this;
        }

        ///<summary>The only instance of this command.</summary>
        public static RhinoObjExportCommand Instance { get; private set; }

        ///<returns>The command name as it appears on the Rhino command line.</returns>
        public override string EnglishName => "RhinoObjExportCommand";

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            // TODO: start here modifying the behaviour of your command.
            // ---
            RhinoApp.WriteLine("The {0} command will export to meshes now.", EnglishName);

            string folderPath = "C:\\Users\\waelext\\Documents\\Meshes";
            if (!System.IO.Directory.Exists(folderPath))
                System.IO.Directory.CreateDirectory(folderPath);

            // Define default meshing parameters
            var meshingParameters = new Rhino.Geometry.MeshingParameters();
            meshingParameters = Rhino.Geometry.MeshingParameters.Default;
            // You can customize these parameters to fit your mesh quality requirements
            // For example:
            // meshingParameters.MinimumEdgeLength = 0.1;
            // meshingParameters.MaximumEdgeLength = 10.0;
            // meshingParameters.Tolerance = 0.1;

            // Iterate over all objects in the document
            foreach (var obj in doc.Objects)
            {
                // Check if the object is valid and not deleted
                if (obj == null || obj.IsDeleted) continue;

                // Get the geometry of the object
                var geometry = obj.Geometry;
                if (geometry == null) continue;

                // If the geometry is a Brep, convert it to a mesh using the predefined meshing parameters
                Mesh[] meshes = null;
                if (geometry is Rhino.Geometry.Brep)
                {
                    meshes = Rhino.Geometry.Mesh.CreateFromBrep(geometry as Rhino.Geometry.Brep, meshingParameters);

                    //bool success = doc.Objects.Delete(obj);

                }
                else if (geometry is Rhino.Geometry.Surface)
                {
                    meshes = new Mesh[1];
                    var surf = geometry as Rhino.Geometry.Surface;
                    var mesh = Mesh.CreateFromSurface(surf, meshingParameters);
                    meshes[0] = mesh;

                }
                else if (geometry is Rhino.Geometry.Mesh)
                {
                    meshes = new Mesh[] { geometry as Rhino.Geometry.Mesh };

                }
                else
                {
                    var brep = Brep.TryConvertBrep(obj.Geometry);
                    if (brep != null && brep.IsValid)
                    {
                        meshes = Rhino.Geometry.Mesh.CreateFromBrep(geometry as Rhino.Geometry.Brep, meshingParameters);

                    }
                    else
                    {

                        RhinoApp.WriteLine("Object is" + obj.Geometry.GetType());
                        continue;
                    }
                }

                // Combine all meshes into a single mesh if there are multiple parts
                Mesh combinedMesh = new Mesh();
                if (meshes != null)
                {
                    foreach (var mesh in meshes)
                    {
                        combinedMesh.Append(mesh);
                    }
                }

                Mesh[] SingleMesh = new Mesh[] { combinedMesh };

                // Define the file path for the OBJ
                string objFilePath = System.IO.Path.Combine(folderPath, obj.Id.ToString() + ".obj");

                // Prepare the file write options
                var fileWriteOptions = new Rhino.FileIO.FileWriteOptions();
                fileWriteOptions.WriteSelectedObjectsOnly = true;

                var fileObjWriteOptions = new Rhino.FileIO.FileObjWriteOptions(fileWriteOptions);
                //fileWriteOptions.WriteSelectedObjectsOnly = true;
                fileObjWriteOptions.MeshParameters = meshingParameters;


                // Select the object to export it individually
                doc.Objects.UnselectAll();
                obj.Select(true);

                // Export the geometry as OBJ. Use the combinedMesh if we have meshed the object; otherwise, use the original geometry.
                if (combinedMesh != null && combinedMesh.Faces.Count > 0)
                {
                    // Temporarily add the mesh to the document to export it
                    var tempMeshId = doc.Objects.AddMesh(combinedMesh);
                    Rhino.FileIO.FileObj.Write(objFilePath, SingleMesh, fileObjWriteOptions);
                    // Remove the temporary mesh from the document
                    doc.Objects.Delete(tempMeshId, true);
                }
                else
                {
                    // Export the original geometry if it's not a type that needed meshing
                    Rhino.FileIO.FileObj.Write(objFilePath, SingleMesh, fileObjWriteOptions);
                }

                // Deselect the object
                obj.Select(false);
            }
            // Inform the user
            RhinoApp.WriteLine("Export completed.");

            return Result.Success;
            //RhinoApp.WriteLine("The {0} command added one line to the document.", EnglishName);

            // ---
            //return Result.Success;
        }
    }
}
