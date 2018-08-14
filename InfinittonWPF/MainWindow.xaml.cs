using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
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

namespace InfinittonWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public String StartProgramActionPath
        {
            get { return "GoExe.png"; }
        }

        public String FolderActionPath
        {
            get { return "Folder-icon.png"; }
        }

        public int SelectedNumber = -1;
        public Image SelectedImage = null;

        private ObservableCollection<IButtonPressAction> _Actions = new ObservableCollection<IButtonPressAction>();

        public event PropertyChangedEventHandler PropertyChanged;

        public void Notify(String val)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(val));
            }
        }

        public ObservableCollection<IButtonPressAction> Actions
        {
            get { return _Actions; }
        }

        public String Action0
        {
            get { return Actions[0].IconPath; }
            set { Actions[0].IconPath = value; Notify("Action0");}
        }

        public String Action1
        {
            get { return Actions[1].IconPath; }
            set { Actions[1].IconPath = value; Notify("Action1"); }
        }

        public String Action2
        {
            get { return Actions[2].IconPath; }
            set { Actions[2].IconPath = value; Notify("Action2"); }
        }

        public String Action3
        {
            get { return Actions[3].IconPath; }
            set { Actions[3].IconPath = value; Notify("Action3"); }
        }

        public String Action4
        {
            get { return Actions[4].IconPath; }
            set { Actions[4].IconPath = value; Notify("Action4"); }
        }

        public String Action5
        {
            get { return Actions[5].IconPath; }
            set { Actions[5].IconPath = value; Notify("Action5"); }
        }

        public String Action6
        {
            get { return Actions[6].IconPath; }
            set { Actions[6].IconPath = value; Notify("Action6"); }
        }

        public String Action7
        {
            get { return Actions[7].IconPath; }
            set { Actions[7].IconPath = value; Notify("Action7"); }
        }

        public String Action8
        {
            get { return Actions[8].IconPath; }
            set { Actions[8].IconPath = value; Notify("Action8"); }
        }

        public String Action9
        {
            get { return Actions[9].IconPath; }
            set { Actions[9].IconPath = value; Notify("Action9"); }
        }

        public String Action10
        {
            get { return Actions[10].IconPath; }
            set { Actions[10].IconPath = value; Notify("Action10"); }
        }

        public String Action11
        {
            get { return Actions[11].IconPath; }
            set { Actions[11].IconPath = value; Notify("Action11"); }
        }

        public String Action12
        {
            get { return Actions[12].IconPath; }
            set { Actions[12].IconPath = value; Notify("Action12"); }
        }

        public String Action13
        {
            get { return Actions[13].IconPath; }
            set { Actions[13].IconPath = value; Notify("Action13"); }
        }

        public String Action14
        {
            get { return Actions[14].IconPath; }
            set { Actions[14].IconPath = value; Notify("Action14"); }
        }

        private MainController controller;

        public MainWindow()
        {
            for (int i = 0; i < 15; i++)
            {
                _Actions.Add(new NullAction());
            }
            InitializeComponent();
            this.DataContext = this;
            
        }

        private void actionImageMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Image image = e.Source as Image;
            DragDrop.DoDragDrop(image, image.Tag, DragDropEffects.All);
        }

        private void imageDrop(object sender, DragEventArgs e)
        {
            Image imageControl = (Image)sender;
            int imageNum = int.Parse(imageControl.Tag as String);
            if ((e.Data.GetData(typeof(String)) != null))
            {
                String tag = e.Data.GetData(typeof(String)) as String;
                switch (tag)
                {
                    case "FolderAction":
                        Actions[imageNum] = new FolderAction();
                        break;
                    case "StartProgramAction":
                        Actions[imageNum] = new LaunchAction();
                        break;
                }
                controller.AddAction(imageNum + 1, Actions[imageNum]);
                Notify("Action" + imageNum.ToString());
            }
            else if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (File.Exists(files[0]))
                {
                    Actions[imageNum].IconPath = files[0];
                    controller.AddAction(imageNum + 1, Actions[imageNum]);
                    Notify("Action" + imageNum.ToString());
                }
            }
        }

        public void Refresh()
        {
            for (int i = 0; i < 15; i++)
            {
                Notify("Action" + i.ToString());
            }
        }

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            brightnessSlider.Value = Properties.Settings.Default.Brightness;
            controller = new MainController(this);
        }

        private void ImageMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                Image imageControl = (Image)sender;
                int imageNum = int.Parse(imageControl.Tag as String);
                controller.PerformAction(imageNum + 1);
            }
            else if (e.ClickCount == 1)
            {
                Image imageControl = (Image)sender;
                int imageNum = int.Parse(imageControl.Tag as String);
                if (SelectedNumber == imageNum)
                {
                    
                    return;
                }
                SelectedNumber = imageNum;
                Border parentBorder = imageControl.Parent as Border;
                if (parentBorder == null) return;
                parentBorder.BorderBrush = Brushes.Blue;
                if (SelectedImage != null)
                {
                    (SelectedImage.Parent as Border).BorderBrush = Brushes.DarkGray;
                }

                SelectedImage = imageControl;

                if (Actions[SelectedNumber] is LaunchAction)
                {
                    LaunchActionPanel.Visibility = Visibility.Visible;
                    tbPath.Text = (Actions[SelectedNumber] as LaunchAction).ExePath;
                    tbArgs.Text = (Actions[SelectedNumber] as LaunchAction).Args;
                }
                else
                {
                    tbPath.Text = tbArgs.Text = "";
                    LaunchActionPanel.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void BrightnessSlider_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (controller == null) return;
            int newVal = (int) e.NewValue;
            controller.SetDeviceBrightness(newVal);
            Properties.Settings.Default.Brightness = newVal;
            Properties.Settings.Default.Save();
        }

        private void discardButtonClick(object sender, RoutedEventArgs e)
        {
            if (Actions[SelectedNumber] is LaunchAction)
            {
                tbPath.Text = (Actions[SelectedNumber] as LaunchAction).ExePath;
                tbArgs.Text = (Actions[SelectedNumber] as LaunchAction).Args;
            }
        }

        private void saveButtonClick(object sender, RoutedEventArgs e)
        {
            if (Actions[SelectedNumber] is LaunchAction)
            {
                (Actions[SelectedNumber] as LaunchAction).ExePath = tbPath.Text;
                (Actions[SelectedNumber] as LaunchAction).Args = tbArgs.Text;
                controller.Save();
            }
            
        }
    }
}
