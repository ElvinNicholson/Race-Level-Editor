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
using Newtonsoft.Json;
using Microsoft.VisualBasic;

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

            scene_viewport.RotateGesture = new MouseGesture(MouseAction.LeftClick);
            preview_viewport.RotateGesture = new MouseGesture(MouseAction.LeftClick);
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
            File.WriteAllText(filename, Regex.Unescape(levelData.ToString(Formatting.Indented)));
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
            Display3DPreview(cpModel.Text);

            cpActive.Text = (string)levelData["activeMaterialFile"];
            setTexture(cpActive.Text);

            cpNextActive.Text = (string)levelData["nextActiveMaterialFile"];
            cpInactive.Text = (string)levelData["inactiveMaterialFile"];

            resetSceneViewport();
            cpStackPanel.Children.Clear();

            foreach (var cpData in levelData["checkpoints"])
            {
                List<string> position = new List<string>();
                foreach (string coordinates in cpData["position"])
                {
                    position.Add(coordinates);
                }
                Display3DScene(cpModel.Text, position[0], position[1], position[2], (string)cpData["facingAxis"]);
                createCheckpointElement(position[0], position[1], position[2], (string)cpData["facingAxis"]);
            }
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

        private void cpActivePreview_Click(object sender, RoutedEventArgs e)
        {
            setTexture(cpActive.Text);  
        }
        private void cpNextActivePreview_Click(object sender, RoutedEventArgs e)
        {
            setTexture(cpNextActive.Text);
        }
        private void cpInactivePreview_Click(object sender, RoutedEventArgs e)
        {
            setTexture(cpInactive.Text);
        }

        /// <summary>
        /// Changes preview model to new model
        /// </summary>
        private void Display3DPreview(string fullModelPath)
        {
            string modelPath = fullModelPath;
            modelPath = modelPath.Remove(0, 12);
            modelPath = modelPath.Replace("/", "\\");
            modelPath = directory + modelPath;

            ModelVisual3D model3D = new ModelVisual3D();
            ModelImporter import = new ModelImporter();
            Model3D model = import.Load(modelPath);

            model3D.Content = model;
            model3D.Transform = createTransformGroup(new AxisAngleRotation3D(new Vector3D(1, 0, 0), 90), new Vector3D(0, 0, 0));

            if (preview_viewport.Children.Count > 3)
            {
                preview_viewport.Children.RemoveAt(preview_viewport.Children.Count - 1);
            }
            preview_viewport.Children.Add(model3D);
        }

        /// <summary>
        /// Changes texture of Preview 3D Model
        /// </summary>
        private void setTexture(string fullTexturePath)
        {
            string texturePath = fullTexturePath;
            texturePath = texturePath.Remove(0, 12);
            texturePath = texturePath.Replace("/", "\\");
            texturePath = directory + texturePath;

            Material material = MaterialHelper.CreateImageMaterial(texturePath, 1);
            ModelVisual3D tempModel = (ModelVisual3D)preview_viewport.Children.Last();
            Model3DGroup tempGroup = (Model3DGroup)tempModel.Content;

            GeometryModel3D tempGeom = (GeometryModel3D)tempGroup.Children.First();
            tempGeom.Material = material;
            tempGeom.Transform = createTransformGroup(new AxisAngleRotation3D(new Vector3D(1, 0, 0), 90), new Vector3D(0, 0, 0));

            Model3DGroup newGroup = new Model3DGroup();
            newGroup.Children.Add(tempGeom);

            ModelVisual3D newModel = new ModelVisual3D();
            newModel.Content = newGroup;

            preview_viewport.Children.RemoveAt(preview_viewport.Children.Count - 1);
            preview_viewport.Children.Add(newModel);
        }

        /// <summary>
        /// Add a new model to scene viewport
        /// </summary>
        /// <param name="fullModelPath">Model Path</param>
        /// <param name="x">X Coordinate</param>
        /// <param name="y">Y Coordinate</param>
        /// <param name="z">Z Coordinate</param>
        /// <param name="facing">Facing Axis</param>
        private void Display3DScene(string fullModelPath, string x, string y, string z, string facing)
        {
            string modelPath = fullModelPath;
            modelPath = modelPath.Remove(0, 12);
            modelPath = modelPath.Replace("/", "\\");
            modelPath = directory + modelPath;

            ModelVisual3D model3D = new ModelVisual3D();
            ModelImporter import = new ModelImporter();
            Model3D model = import.Load(modelPath);

            model3D.Content = model;
            model3D.Transform = createTransformGroup(new AxisAngleRotation3D(new Vector3D(1, 0, 0), 90), new Vector3D(Int32.Parse(x), -Int32.Parse(z), Int32.Parse(y)));

            if (facing == "x")
            {
                Matrix3D matrix = model3D.Content.Transform.Value;
                matrix.Rotate(new Quaternion(new Vector3D(0, 1, 0), 90));
                model3D.Content.Transform = new MatrixTransform3D(matrix);
            }

            scene_viewport.Children.Add(model3D);
        }

        private void resetSceneViewport()
        {
            scene_viewport.Children.Clear();

            GridLinesVisual3D grid = new GridLinesVisual3D();
            grid.Width = 500;
            grid.Length = 500;
            grid.Thickness = 0.1;
            grid.MinorDistance = 5;
            grid.MajorDistance = 10;
            grid.Fill = Brushes.LightGray;
            scene_viewport.Children.Add(grid);

            DefaultLights light = new DefaultLights();
            scene_viewport.Children.Add(light);
        }

        /// <summary>
        /// Creates a Transform group for model transform
        /// </summary>
        /// <param name="rotate">Rotation of model</param>
        /// <param name="translate">Position of model to origin (0, 0, 0)</param>
        /// <returns></returns>
        private Transform3DGroup createTransformGroup(AxisAngleRotation3D rotate, Vector3D translate)
        {
            RotateTransform3D rotateTransform = new RotateTransform3D(rotate);
            rotateTransform.CenterX = 0;
            rotateTransform.CenterY = 0;
            rotateTransform.CenterZ = 0;

            TranslateTransform3D translateTransform = new TranslateTransform3D(translate);

            Transform3DGroup transformGroup = new Transform3DGroup();
            transformGroup.Children.Add(rotateTransform);
            transformGroup.Children.Add(translateTransform);

            return transformGroup;
        }

        /// <summary>
        /// Triggers when Checkpoints > Add is clicked
        /// </summary>
        private void cpAdd_Click(object sender, RoutedEventArgs e)
        {
            createCheckpointElement("0", "0", "0", "x");
            Display3DScene(cpModel.Text, "0", "0", "0", "x");

            var tempArray = levelData["checkpoints"];
            string newJson = tempArray.ToString();
            newJson = newJson.Remove(newJson.Length - 1);
            newJson = @"" + newJson + ", {\"position\": [\"0\", \"0\", \"0\"], \"facingAxis\":\"x\"}]";
            var test = JsonConvert.DeserializeObject(newJson);

            levelData["checkpoints"] = newJson.Trim('"');
        }

        /// <summary>
        /// Creates a new checkpoint element
        /// </summary>
        /// <param name="x">X Coordinate</param>
        /// <param name="y">Y Coordinate</param>
        /// <param name="z">Z Coordinate</param>
        /// <param name="facing">Facing Axis</param>
        private void createCheckpointElement(string x, string y, string z, string facing)
        {
            int i = cpStackPanel.Children.Count;

            // Label
            StackPanel newStackPanel = new StackPanel();
            Label newLabel = new Label();
            newLabel.Content = "• Checkpoint " + i.ToString();
            newStackPanel.Children.Add(newLabel);

            // Coordinates
            DockPanel newDockPanel = new DockPanel();
            Grid newGrid = new Grid();
            newGrid.Height = 30;
            ColumnDefinition gridCol1 = new ColumnDefinition();
            gridCol1.Width = GridLength.Auto;
            ColumnDefinition gridCol2 = new ColumnDefinition();
            gridCol2.Width = GridLength.Auto;
            ColumnDefinition gridCol3 = new ColumnDefinition();
            gridCol3.Width = GridLength.Auto;
            newGrid.ColumnDefinitions.Add(gridCol1);
            newGrid.ColumnDefinitions.Add(gridCol2);
            newGrid.ColumnDefinitions.Add(gridCol3);

            DockPanel newDockPanelX = new DockPanel();
            Label newLabelX = new Label();
            TextBox newTextBoxX = new TextBox();
            newLabelX.Content = "X:";
            newTextBoxX.Name = "X" + i.ToString();
            newTextBoxX.TextChanged += cpPos_TextChanged;
            newTextBoxX.Text = x;
            newTextBoxX.Width = 40;
            newTextBoxX.VerticalAlignment = VerticalAlignment.Center;
            newTextBoxX.PreviewTextInput += cpCoordinates_PreviewTextInput;
            newDockPanelX.Children.Add(newLabelX);
            newDockPanelX.Children.Add(newTextBoxX);
            Grid.SetColumn(newDockPanelX, 0);
            newGrid.Children.Add(newDockPanelX);

            DockPanel newDockPanelY = new DockPanel();
            Label newLabelY = new Label();
            TextBox newTextBoxY = new TextBox();
            newLabelY.Content = "Y:";
            newTextBoxY.Name = "Y" + i.ToString();
            newTextBoxY.TextChanged += cpPos_TextChanged;
            newTextBoxY.Text = y;
            newTextBoxY.Width = 40;
            newTextBoxY.VerticalAlignment = VerticalAlignment.Center;
            newTextBoxY.PreviewTextInput += cpCoordinates_PreviewTextInput;
            newDockPanelY.Children.Add(newLabelY);
            newDockPanelY.Children.Add(newTextBoxY);
            Grid.SetColumn(newDockPanelY, 1);
            newGrid.Children.Add(newDockPanelY);

            DockPanel newDockPanelZ = new DockPanel();
            Label newLabelZ = new Label();
            TextBox newTextBoxZ = new TextBox();
            newLabelZ.Content = "Z:";
            newTextBoxZ.Name = "Z" + i.ToString();
            newTextBoxZ.TextChanged += cpPos_TextChanged;
            newTextBoxZ.Text = z;
            newTextBoxZ.Width = 40;
            newTextBoxZ.VerticalAlignment = VerticalAlignment.Center;
            newTextBoxZ.PreviewTextInput += cpCoordinates_PreviewTextInput;
            newDockPanelZ.Children.Add(newLabelZ);
            newDockPanelZ.Children.Add(newTextBoxZ);
            Grid.SetColumn(newDockPanelZ, 2);
            newGrid.Children.Add(newDockPanelZ);

            Separator newSeparator = new Separator();
            newSeparator.Background = Brushes.Transparent;
            newSeparator.Width = 15;
            DockPanel.SetDock(newSeparator, Dock.Left);

            newDockPanel.Children.Add(newSeparator);
            newDockPanel.Children.Add(newGrid);
            newStackPanel.Children.Add(newDockPanel);

            // Facing Axis
            DockPanel newDockPanelFacing = new DockPanel();

            Separator newSeparatorFacing = new Separator();
            newSeparatorFacing.Background = Brushes.Transparent;
            newSeparatorFacing.Width = 15;
            DockPanel.SetDock(newSeparatorFacing, Dock.Left);
            newDockPanelFacing.Children.Add(newSeparatorFacing);

            Label newLabelFacing = new Label();
            newLabelFacing.Content = "Facing Axis: ";
            newDockPanelFacing.Children.Add(newLabelFacing);

            ComboBox newComboBox = new ComboBox();
            newComboBox.Width = 100;
            newComboBox.VerticalAlignment = VerticalAlignment.Center;
            newComboBox.HorizontalAlignment = HorizontalAlignment.Left;
            ComboBoxItem axisX = new ComboBoxItem();
            axisX.Content = "X Axis";
            ComboBoxItem axisZ = new ComboBoxItem();
            axisZ.Content = "Z Axis";
            newComboBox.Items.Add(axisX);
            newComboBox.Items.Add(axisZ);

            if (facing == "x")
            {
                newComboBox.SelectedIndex = 0;
            }
            else
            {
                newComboBox.SelectedIndex = 1;
            }

            newDockPanelFacing.Children.Add(newComboBox);

            newStackPanel.Children.Add(newDockPanelFacing);

            cpStackPanel.Children.Add(newStackPanel);
        }

        /// <summary>
        /// Called when a checkpoint positional value is changed
        /// </summary>
        private void cpPos_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox box = sender as TextBox;
            string name = box.Name;

            if (!int.TryParse(box.Text, out _))
            {
                return;
            }

            switch (name[0])
            {
                case 'X':
                    {
                        int i = 0;
                        foreach (var cpData in levelData["checkpoints"])
                        {
                            if (i == (int)Char.GetNumericValue(name[1]))
                            {
                                cpData["position"][0] = box.Text;

                                List<int> position = new List<int>();
                                foreach (string coordinates in cpData["position"])
                                {
                                    position.Add(Int32.Parse(coordinates));
                                }

                                moveCheckpoint(position[0], position[1], position[2], i);
                                return;
                            }
                            i++;
                        }
                        return;
                    }

                case 'Y':
                    {
                        int i = 0;
                        foreach (var cpData in levelData["checkpoints"])
                        {
                            if (i == (int)Char.GetNumericValue(name[1]))
                            {
                                cpData["position"][1] = box.Text;

                                List<int> position = new List<int>();
                                foreach (string coordinates in cpData["position"])
                                {
                                    position.Add(Int32.Parse(coordinates));
                                }

                                moveCheckpoint(position[0], position[1], position[2], i);
                                return;
                            }
                            i++;
                        }
                        return;
                    }

                case 'Z':
                    {
                        int i = 0;
                        foreach (var cpData in levelData["checkpoints"])
                        {
                            if (i == (int)Char.GetNumericValue(name[1]))
                            {
                                cpData["position"][2] = box.Text;

                                List<int> position = new List<int>();
                                foreach (string coordinates in cpData["position"])
                                {
                                    position.Add(Int32.Parse(coordinates));
                                }

                                moveCheckpoint(position[0], position[1], position[2], i);
                                return;
                            }
                            i++;
                        }
                        return;
                    }
            }
        }

        /// <summary>
        /// Move checkpoint to new position
        /// </summary>
        /// <param name="x">X Coordinate</param>
        /// <param name="y">Y Coordinate</param>
        /// <param name="z">Z Coordinate</param>
        /// <param name="index">Index of checkpoint in scene_viewport</param>
        private void moveCheckpoint(int x, int y, int z, int index)
        {
            scene_viewport.Children[index + 2].Transform  = createTransformGroup(new AxisAngleRotation3D(new Vector3D(1, 0, 0), 90), new Vector3D(x, -z, y));
        }

        /// <summary>
        /// Only allow numbers and "-" sign
        /// </summary>
        private void cpCoordinates_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("^[-][0-9]+$|^[0-9]*[-]{0,1}[0-9]*$");
            e.Handled = !regex.IsMatch(e.Text);
        }
    }
}
