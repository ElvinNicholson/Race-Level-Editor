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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;
using Path = System.IO.Path;

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
            cpActive.Text = (string)levelData["activeMaterialFile"];
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
    }
}
