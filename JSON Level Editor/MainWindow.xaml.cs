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
using System.Reflection;

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
                filename = openFileDialog.FileName;
                directory = Path.GetDirectoryName(filename);
                directory = Path.GetDirectoryName(directory);

                loadLevel();
            }
        }

        /// <summary>
        /// Triggers when File > Save is clicked
        /// </summary>
        private void fileSave_Click(object sender, RoutedEventArgs e)
        {
            saveLevel();
        }

        private void saveLevel()
        {
            File.WriteAllText(filename, Regex.Unescape(levelData.ToString(Formatting.Indented).Replace("\"[", "[").Replace("]\"", "]").Replace("\"REMOVE_THIS\",", "")));
        }

        /// <summary>
        /// Load level from JSON file
        /// </summary>
        private void loadLevel()
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

                int rotation = 0;
                if ((string)cpData["facingAxis"] == "x")
                {
                    rotation = 90;
                }

                Display3DScene(cpModel.Text, position[0], position[1], position[2], rotation);
                createCheckpointElement(position[0], position[1], position[2], (string)cpData["facingAxis"]);
            }

            foreach (var botData in levelData["racingBots"])
            {
                List<string> position = new List<string>();
                foreach (string coordinates in botData["spawnPosition"])
                {
                    position.Add(coordinates);
                }

                createBotElement((string)botData["botName"], position[0], position[1], position[2], (string)botData["spawnDirection"], (string)botData["carModelFile"], (string)botData["carMaterialFile"], (string)botData["minAngle"]);
                Display3DScene((string)botData["carModelFile"], position[0], position[1], position[2], Int32.Parse((string)botData["spawnDirection"]));
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

            saveLevel();
            loadLevel();
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
            Matrix3D matrix = model3D.Content.Transform.Value;
            matrix.Rotate(new Quaternion(new Vector3D(1, 0, 0), 90));
            model3D.Content.Transform = new MatrixTransform3D(matrix);

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

            RotateTransform3D rotateTransform = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(1, 0, 0), 90));
            rotateTransform.CenterX = 0;
            rotateTransform.CenterY = 0;
            rotateTransform.CenterZ = 0;
            tempGeom.Transform = rotateTransform;

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
        private void Display3DScene(string fullModelPath, string x, string y, string z, int angle)
        {
            string modelPath = fullModelPath;
            modelPath = modelPath.Remove(0, 12);
            modelPath = modelPath.Replace("/", "\\");
            modelPath = directory + modelPath;

            int _x = Int32.Parse(x);
            int _y = Int32.Parse(y);
            int _z = Int32.Parse(z);

            ModelVisual3D model3D = new ModelVisual3D();
            ModelImporter import = new ModelImporter();
            Model3D model = import.Load(modelPath);

            model3D.Content = model;
            Matrix3D matrix = model3D.Content.Transform.Value;
            matrix.Rotate(new Quaternion(new Vector3D(1, 0, 0), 90));
            model3D.Content.Transform = new MatrixTransform3D(matrix);

            model3D.Transform = new TranslateTransform3D(new Vector3D(_x, -_z, _y)); ;

            if (angle != 0)
            {
                Matrix3D matrix2 = new Matrix3D();
                matrix2.Rotate(new Quaternion(new Vector3D(0, 0, 1), angle));
                matrix2.Translate(new Vector3D(_x, -_z, _y));
                model3D.Transform = new MatrixTransform3D(matrix2);
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
        /// Triggers when Checkpoints > Add is clicked
        /// </summary>
        private void cpAdd_Click(object sender, RoutedEventArgs e)
        {
            var tempArray = levelData["checkpoints"];
            string newJson = tempArray.ToString();
            newJson = newJson.Remove(newJson.Length - 1);
            newJson = newJson + ", {\"position\": [\"0\", \"0\", \"0\"], \"facingAxis\":\"x\"}]";
            levelData["checkpoints"] = newJson;

            saveLevel();
            loadLevel();
        }

        /// <summary>
        /// Called when Checkpoints > Remove is clicked
        /// </summary>
        private void cpRemove_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            string name = button.Name;
            int i = (int)Char.GetNumericValue(name[1]);
            levelData["checkpoints"][i] = "REMOVE_THIS";

            var tempArray = levelData["checkpoints"];
            string newJson = tempArray.ToString();
            newJson = newJson.Replace(",\r\n  \"REMOVE_THIS\"", "").Replace("\"REMOVE_THIS\",", "");
            levelData["checkpoints"] = newJson;

            saveLevel();
            loadLevel();
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

            StackPanel newStackPanel = new StackPanel();
            if (i%2 == 0)
            {
                newStackPanel.Background = Brushes.WhiteSmoke;
            }

            // Label
            DockPanel newDockPanelTop = new DockPanel();
            Label newLabel = new Label();
            newLabel.Content = "• Checkpoint " + i.ToString();
            DockPanel.SetDock(newLabel, Dock.Left);
            newDockPanelTop.Children.Add(newLabel);

            // Remove Button
            Button removeButton = new Button();
            removeButton.VerticalAlignment = VerticalAlignment.Center;
            removeButton.Content = "Remove";
            removeButton.Width = 80;
            removeButton.Margin = new Thickness(5);
            removeButton.Name = "R" + i.ToString();
            removeButton.Click += cpRemove_Click;
            removeButton.HorizontalAlignment = HorizontalAlignment.Right;
            DockPanel.SetDock(removeButton, Dock.Right);
            newDockPanelTop.Children.Add(removeButton);
            newStackPanel.Children.Add(newDockPanelTop);

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
            newTextBoxX.Text = x;
            newTextBoxX.TextChanged += cpPos_TextChanged;
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
            newTextBoxY.Text = y;
            newTextBoxY.TextChanged += cpPos_TextChanged;
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
            newTextBoxZ.Text = z;
            newTextBoxZ.TextChanged += cpPos_TextChanged;
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
            newComboBox.Name = "F" + i.ToString();
            newComboBox.Width = 100;
            newComboBox.VerticalAlignment = VerticalAlignment.Center;
            newComboBox.HorizontalAlignment = HorizontalAlignment.Left;
            ComboBoxItem axisX = new ComboBoxItem();
            axisX.Content = "X Axis";
            ComboBoxItem axisZ = new ComboBoxItem();
            axisZ.Content = "Z Axis";
            newComboBox.Items.Add(axisX);
            newComboBox.Items.Add(axisZ);
            newComboBox.SelectionChanged += cpFacing_SelectionChanged;

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

        private void botAdd_Click(object sender, RoutedEventArgs e)
        {
            createBotElement("Test Bot", "0", "0", "0", "90", "model_path", "texture_path", "10");
        }

        private void createBotElement(string name, string x, string y, string z, string rotation, string model, string texture, string difficulty)
        {
            int i = botStackPanel.Children.Count;

            StackPanel newStackPanel = new StackPanel();
            if (i % 2 == 0)
            {
                newStackPanel.Background = Brushes.WhiteSmoke;
            }

            DockPanel newDockPanelTop = new DockPanel();

            // Text Box
            Label newLabel = new Label();
            newLabel.Content = "• ";
            DockPanel.SetDock(newLabel, Dock.Left);
            newDockPanelTop.Children.Add(newLabel);

            TextBox newTextBox = new TextBox();
            newTextBox.Text = name;
            newTextBox.Name = "N" + i.ToString();
            newTextBox.VerticalAlignment = VerticalAlignment.Center;
            newTextBox.Width = 100;
            newTextBox.TextChanged += botName_TextChanged;
            newDockPanelTop.Children.Add(newTextBox);

            // Remove Button
            Button removeButton = new Button();
            removeButton.VerticalAlignment = VerticalAlignment.Center;
            removeButton.Content = "Remove";
            removeButton.Width = 80;
            removeButton.Margin = new Thickness(5);
            removeButton.Name = "B" + i.ToString();
            removeButton.HorizontalAlignment = HorizontalAlignment.Right;
            //removeButton.Click += cpRemove_Click;
            DockPanel.SetDock(removeButton, Dock.Right);
            newDockPanelTop.Children.Add(removeButton);
            newStackPanel.Children.Add(newDockPanelTop);

            // Model
            DockPanel newDockPanelModel = new DockPanel();

            Separator newSeparatorModel = new Separator();
            newSeparatorModel.Background = Brushes.Transparent;
            newSeparatorModel.Width = 15;
            DockPanel.SetDock(newSeparatorModel, Dock.Left);
            newDockPanelModel.Children.Add(newSeparatorModel);

            Label newLabelModel = new Label();
            newLabelModel.Content = "Model:";
            newDockPanelModel.Children.Add(newLabelModel);

            Button newButtonModel = new Button();
            newButtonModel.Content = "Load";
            newButtonModel.VerticalAlignment = VerticalAlignment.Center;
            newButtonModel.HorizontalAlignment = HorizontalAlignment.Right;
            newButtonModel.Width = 40;
            newButtonModel.Margin = new Thickness(5);
            newButtonModel.Name = "O" + i.ToString();
            newButtonModel.Click += botModelLoad_Click;
            DockPanel.SetDock(newButtonModel, Dock.Right);
            newDockPanelModel.Children.Add(newButtonModel);

            Border newBorderModel = new Border();
            newBorderModel.BorderThickness = new Thickness(1);
            newBorderModel.BorderBrush = Brushes.Silver;
            newBorderModel.VerticalAlignment = VerticalAlignment.Center;
            newBorderModel.HorizontalAlignment = HorizontalAlignment.Center;
            newBorderModel.Width = 110;
            DockPanel.SetDock(newBorderModel, Dock.Right);
            ScrollViewer newScrollViewerModel = new ScrollViewer();
            newScrollViewerModel.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            newScrollViewerModel.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;
            TextBlock newTextBlockModel = new TextBlock();
            newTextBlockModel.Name = "M" + i.ToString();
            newTextBlockModel.Background = Brushes.White;
            newTextBlockModel.Text = model;
            newScrollViewerModel.Content = newTextBlockModel;
            newBorderModel.Child = newScrollViewerModel;
            newDockPanelModel.Children.Add(newBorderModel);
            newStackPanel.Children.Add(newDockPanelModel);

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
            newTextBoxX.Name = "J" + i.ToString();
            newTextBoxX.Text = x;
            //newTextBoxX.TextChanged += cpPos_TextChanged;
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
            newTextBoxY.Name = "K" + i.ToString();
            newTextBoxY.Text = y;
            //newTextBoxY.TextChanged += cpPos_TextChanged;
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
            newTextBoxZ.Name = "L" + i.ToString();
            newTextBoxZ.Text = z;
            //newTextBoxZ.TextChanged += cpPos_TextChanged;
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

            // Rotation
            DockPanel newDockPanelRotation = new DockPanel();

            Separator newSeparatorRotation = new Separator();
            newSeparatorRotation.Background = Brushes.Transparent;
            newSeparatorRotation.Width = 15;
            DockPanel.SetDock(newSeparatorRotation, Dock.Left);
            newDockPanelRotation.Children.Add(newSeparatorRotation);

            Label newLabelRotation = new Label();
            TextBox newTextBoxRotation = new TextBox();
            newLabelRotation.Content = "Rotation: ";
            newTextBoxRotation.Name = "P" + i.ToString();
            newTextBoxRotation.Text = rotation;
            //newTextBoxRotation.TextChanged +=
            newTextBoxRotation.Width = 40;
            newTextBoxRotation.VerticalAlignment = VerticalAlignment.Center;
            newTextBoxRotation.HorizontalAlignment = HorizontalAlignment.Left;
            newTextBoxRotation.PreviewTextInput += cpCoordinates_PreviewTextInput;
            newDockPanelRotation.Children.Add(newLabelRotation);
            newDockPanelRotation.Children.Add(newTextBoxRotation);
            newStackPanel.Children.Add(newDockPanelRotation);

            // Difficulty
            DockPanel newDockPanelDiff = new DockPanel();

            Separator newSeparatorDiff = new Separator();
            newSeparatorDiff.Background = Brushes.Transparent;
            newSeparatorDiff.Width = 15;
            DockPanel.SetDock(newSeparatorDiff, Dock.Left);
            newDockPanelDiff.Children.Add(newSeparatorDiff);

            Label newLabelDiff = new Label();
            newLabelDiff.Content = "Expertise: ";
            newDockPanelDiff.Children.Add(newLabelDiff);

            ComboBox newComboBox = new ComboBox();
            newComboBox.Name = "D" + i.ToString();
            newComboBox.Width = 100;
            newComboBox.VerticalAlignment = VerticalAlignment.Center;
            newComboBox.HorizontalAlignment = HorizontalAlignment.Left;
            ComboBoxItem beginner = new ComboBoxItem();
            beginner.Content = "Beginner";
            ComboBoxItem competent = new ComboBoxItem();
            competent.Content = "Competent";
            ComboBoxItem proficient = new ComboBoxItem();
            proficient.Content = "Proficient";
            ComboBoxItem expert = new ComboBoxItem();
            expert.Content = "Expert";
            newComboBox.Items.Add(beginner);
            newComboBox.Items.Add(competent);
            newComboBox.Items.Add(proficient);
            newComboBox.Items.Add(expert);
            //newComboBox.SelectionChanged += cpFacing_SelectionChanged;

            switch (difficulty)
            {
                case "40":
                    newComboBox.SelectedIndex = 0;
                    break;

                case "30":
                    newComboBox.SelectedIndex = 1;
                    break;

                case "20":
                    newComboBox.SelectedIndex = 2;
                    break;

                case "10":
                    newComboBox.SelectedIndex = 3;
                    break;
            }

            newDockPanelDiff.Children.Add(newComboBox);

            newStackPanel.Children.Add(newDockPanelDiff);

            botStackPanel.Children.Add(newStackPanel);

        }

        private void botModelLoad_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            string name = button.Name;
            int index = (int)Char.GetNumericValue(name[1]);

            StackPanel stackPanel = (StackPanel)botStackPanel.Children[index];
            DockPanel dockPanel = (DockPanel)stackPanel.Children[1];
            Border border = (Border)dockPanel.Children[3];
            ScrollViewer scrollViewer = (ScrollViewer)border.Child;
            TextBlock textBlock = (TextBlock)scrollViewer.Content;

            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Wavefront files (*.obj)|*.obj";

            openFileDialog.InitialDirectory = Path.Combine(directory, "Models");
            if (openFileDialog.ShowDialog() == true)
            {
                string model = openFileDialog.SafeFileName;
                model = "../Resources/Models/" + model;
                textBlock.Text = model;
                levelData["racingBots"][index]["carModelFile"] = model;
            }
        }

        private void botName_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            string name = textBox.Name;
            int index = (int)char.GetNumericValue(name[1]);

            levelData["racingBots"][index]["botName"] = textBox.Text;
        }

        /// <summary>
        /// Called when a checkpoint positional value is changed
        /// </summary>
        private void cpPos_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox box = sender as TextBox;
            string name = box.Name;
            int index = (int)Char.GetNumericValue(name[1]);

            if (!int.TryParse(box.Text, out _))
            {
                return;
            }

            if (index >= levelData["checkpoints"].Count())
            {
                return;
            }

            switch (name[0])
            {
                case 'X':
                    {
                        updateCpPos(index, box.Text, 0);
                        return;
                    }

                case 'Y':
                    {
                        updateCpPos(index, box.Text, 1);
                        return;
                    }

                case 'Z':
                    {
                        updateCpPos(index, box.Text, 2);
                        return;
                    }
            }
        }

        private void updateCpPos(int index, string boxText, int axis)
        {
            var cpData = levelData["checkpoints"][index];
            cpData["position"][axis] = boxText;

            resetCheckpointPos(index);
        }

        /// <summary>
        /// Called when Checkpoints > Facing Axis is changed
        /// </summary>
        private void cpFacing_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox box = sender as ComboBox;
            string name = box.Name;
            int i = (int)Char.GetNumericValue(name[1]);
            var cpData = levelData["checkpoints"][i];

            switch(box.SelectedIndex)
            { 
                case 0:
                    cpData["facingAxis"] = "x";
                    break;

                case 1:
                    cpData["facingAxis"] = "z";
                    break;
            }

            resetCheckpointPos(i);
        }

        /// <summary>
        /// Resets checkpoint position to the one in levelData
        /// </summary>
        /// <param name="i">Checkpoint index</param>
        private void resetCheckpointPos(int i)
        {
            var cpData = levelData["checkpoints"][i];
            List<int> position = new List<int>();
            foreach (string coordinates in cpData["position"])
            {
                position.Add(Int32.Parse(coordinates));
            }

            Matrix3D matrix = new Matrix3D();

            if ((string)cpData["facingAxis"] == "x")
            {
                matrix.Rotate(new Quaternion(new Vector3D(0, 0, 1), 90));
            }

            matrix.Translate(new Vector3D(position[0], -position[2], position[1]));
            scene_viewport.Children[i + 2].Transform = new MatrixTransform3D(matrix);
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
