using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;
using Path = System.IO.Path;
using HelixToolkit.Wpf;

namespace JSON_Level_Editor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private JObject levelData;
        private string filename;
        private string directory;

        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Triggers when File > Open is clicked
        /// </summary>
        private void fileOpen_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "JSON files (*.json)|*.json";
            if (openFileDialog.ShowDialog() == true) 
            {
                filename= openFileDialog.FileName;
                directory = Path.GetDirectoryName(filename);
                directory = Path.GetDirectoryName(directory);

                readJsonFile();
            }
        }

        /// <summary>
        /// Triggers when File > Save is clicked
        /// </summary>
        private void fileSave_Click(object sender, RoutedEventArgs e) 
        {
            File.WriteAllText(filename, levelData.ToString(Newtonsoft.Json.Formatting.Indented));
        }

        /// <summary>
        /// Reads JSON File
        /// </summary>
        private void readJsonFile()
        {
            levelData = JObject.Parse(File.ReadAllText(filename));

            // Race
            raceName.Text = (string)levelData["raceName"];
            raceLap.Text = (string)levelData["numberOfLaps"];

            // Checkpoint
            cpModel.Text = (string)levelData["modelFile"];
            Display3D(cpModel.Text);

            cpActive.Text = (string)levelData["activeMaterialFile"];
            setTexture(cpActive.Text);

            cpNextActive.Text = (string)levelData["nextActiveMaterialFile"];
            cpInactive.Text = (string)levelData["inactiveMaterialFile"];
        }

        private void raceName_TextChanged(object sender, TextChangedEventArgs e)
        {
            levelData["raceName"] = raceName.Text;
        }

        private void raceLap_TextChanged(object sender, TextChangedEventArgs e) 
        {
            levelData["numberOfLaps"] = raceLap.Text;
        }

        /// <summary>
        /// Only allow number to be input to Lap
        /// </summary>
        private void raceLap_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        /// <summary>
        /// Triggers when Checkpoint > Model > Load is clicked
        /// </summary>
        private void cpModelLoad_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Wavefront files (*.obj)|*.obj";

            openFileDialog.InitialDirectory = Path.Combine(directory, "Models");
            if (openFileDialog.ShowDialog() == true)
            {
                string model = openFileDialog.SafeFileName;
                model = "../Resources/Models/" + model;
                cpModel.Text = model;
                levelData["modelFile"] = model;
            }
        }

        /// <summary>
        /// Triggers when Checkpoint > Textures > Active > Load is clicked
        /// </summary>
        private void cpActiveLoad_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image files (*.png;*.jpeg)|*.png;*.jpeg";

            openFileDialog.InitialDirectory = Path.Combine(directory, "Materials");
            if (openFileDialog.ShowDialog() == true)
            {
                string texture = openFileDialog.SafeFileName;
                texture = "../Resources/Materials/" + texture;
                cpActive.Text = texture;
                levelData["activeMaterialFile"] = texture;
            }
        }

        /// <summary>
        /// Triggers when Checkpoint > Textures > Next Active > Load is clicked
        /// </summary>
        private void cpNextActiveLoad_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image files (*.png;*.jpeg)|*.png;*.jpeg";

            openFileDialog.InitialDirectory = Path.Combine(directory, "Materials");
            if (openFileDialog.ShowDialog() == true)
            {
                string texture = openFileDialog.SafeFileName;
                texture = "../Resources/Materials/" + texture;
                cpNextActive.Text = texture;
                levelData["nextActiveMaterialFile"] = texture;
            }
        }

        /// <summary>
        /// Triggers when Checkpoint > Textures > Inactive > Load is clicked
        /// </summary>
        private void cpInactiveLoad_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image files (*.png;*.jpeg)|*.png;*.jpeg";

            openFileDialog.InitialDirectory = Path.Combine(directory, "Materials");
            if (openFileDialog.ShowDialog() == true)
            {
                string texture = openFileDialog.SafeFileName;
                texture = "../Resources/Materials/" + texture;
                cpInactive.Text = texture;
                levelData["inactiveMaterialFile"] = texture;
            }
        }

        /// <summary>
        /// Changes 3D Viewport to new model
        /// </summary>
        private void Display3D(string fullModelPath)
        {
            string modelPath = fullModelPath;
            modelPath = modelPath.Remove(0, 12);
            modelPath = modelPath.Replace("/", "\\");
            modelPath = directory + modelPath;

            ModelVisual3D model3D = new ModelVisual3D();
            Model3D model = null;

            try
            {
                viewport.RotateGesture = new MouseGesture(MouseAction.LeftClick);

                ModelImporter import = new ModelImporter();

                model = import.Load(modelPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception Error : " + ex.StackTrace);
            }

            model3D.Content = model;
            RotateTransform3D rotateTransform = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(1, 0, 0), 90));
            rotateTransform.CenterX = 0;
            rotateTransform.CenterY = 0;
            rotateTransform.CenterZ = 0;
            model3D.Transform = rotateTransform;

            if (viewport.Children.Count > 3)
            {
                viewport.Children.RemoveAt(viewport.Children.Count - 1);
            }
            viewport.Children.Add(model3D);
        }

        private void setTexture(string fullTexturePath)
        {
            string texturePath = fullTexturePath;
            texturePath = texturePath.Remove(0, 12);
            texturePath = texturePath.Replace("/", "\\");
            texturePath = directory + texturePath;

            Material material = MaterialHelper.CreateImageMaterial(texturePath, 1);
            ModelVisual3D tempModel = (ModelVisual3D)viewport.Children.Last();
            Model3DGroup tempGroup = (Model3DGroup)tempModel.Content;
            RotateTransform3D tempTransform = (RotateTransform3D)viewport.Children.Last().Transform;

            GeometryModel3D tempGeom = (GeometryModel3D)tempGroup.Children.First();
            tempGeom.Material = material;
            tempGeom.Transform = tempTransform;

            Model3DGroup newGroup = new Model3DGroup();
            newGroup.Children.Add(tempGeom);

            ModelVisual3D newModel = new ModelVisual3D();
            newModel.Content = newGroup;

            viewport.Children.RemoveAt(viewport.Children.Count - 1);
            viewport.Children.Add(newModel);
        }
    }
}
