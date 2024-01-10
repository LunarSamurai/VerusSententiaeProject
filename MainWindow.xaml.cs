using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using WpfAnimatedGif;

namespace VerusSententiaeProject
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DispatcherTimer splashTimer = new DispatcherTimer();
        private SAM _sam;
        private List<string> _videoFiles;
        private int _currentVideoIndex;

        public MainWindow()
        {
            InitializeComponent();
            _sam = new SAM();
            AttachSAMEventHandlers();
            // Setting the animated GIF for the splash screen
            var image = new BitmapImage();
            image.BeginInit();
            image.UriSource = new Uri(@"\IAT_Resources\Logo\logo.gif", UriKind.RelativeOrAbsolute);
            image.EndInit();
            ImageBehavior.SetAnimatedSource(img, image);  // Assuming SplashImage is the name of your Image control in XAML
            _sam.LoadValenceImage();
            LoadSplashScreen();

            splashTimer.Interval = TimeSpan.FromSeconds(10);
            splashTimer.Tick += SplashTimer_Tick;
            splashTimer.Start();
            this.Focusable = true;
            this.Focus();
            
            SAM.SamInstructionScreen = this.SamInstructionScreen; // Link to the actual Grid
            _sam.PlayIntroductionVideoRequested += PlayIntroductionVideo;
            Loaded += MainWindow_Loaded;
        }
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Using the dispatcher to ensure this runs on the UI thread
            Dispatcher.Invoke(() =>
            {
                // Attempt to give the main window focus
                Focus();
            });
        }

        private void MainWindow_KeyUp(object sender, KeyEventArgs e)
        {
            Focus();
            Console.WriteLine("Key pressed");
            MessageBox.Show("Key pressed");
            ValenceRatingGrid.Visibility = Visibility.Collapsed;
            ArousalRatingGrid.Visibility = Visibility.Visible;

            if (e.Key >= Key.D1 && e.Key <= Key.D9)
            {
                MessageBox.Show("A key 1 - 9 was pressed successfully.");
                Console.WriteLine("A key 1 - 9 was pressed successfully.");
                ValenceRatingGrid.Visibility = Visibility.Collapsed;
                ArousalRatingGrid.Visibility = Visibility.Visible;
            }
            else
            {
                MessageBox.Show(e.Key.ToString(), "Something was pressed!");
                Console.WriteLine(e.Key.ToString() + " Something was pressed!");
            }
        }

        private void PlayIntroductionVideo()
        {
            string baseDir = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string projectRootPath = Directory.GetParent(Directory.GetParent(Directory.GetParent(baseDir).FullName).FullName).FullName;
            string videoFolderPath = System.IO.Path.Combine(projectRootPath, "SAM_Resources", "IntroductionVideo");

            if (!Directory.Exists(videoFolderPath))
            {
                MessageBox.Show("Video directory not found.");
                return;
            }

            var videoFormats = new[] { "*.mp4", "*.mov" };
            _videoFiles = videoFormats.SelectMany(format => Directory.EnumerateFiles(videoFolderPath, format)).ToList();

            if (_videoFiles.Count > 0 && _currentVideoIndex < _videoFiles.Count)
            {
                string videoPath = _videoFiles[_currentVideoIndex];
                VideoPlayer.Source = new Uri(videoPath, UriKind.Absolute);
                VideoPlayer.Play();
            }
            else
            {
                VideoPlayerGrid.Visibility = Visibility.Collapsed; // Collapse the video player if no videos are found
            }
        }


        public void VideoPlayer_MediaEnded(object sender, RoutedEventArgs e)
        {
            _currentVideoIndex++;
            if (_currentVideoIndex < _videoFiles.Count)
            {
                PlayIntroductionVideo(); // Play the next video
            }
            else
            {
                VideoPlayer.Stop();
                VideoPlayerGrid.Visibility = Visibility.Collapsed; // Collapse the video player when all videos have been played
                DemoIntroducer.Visibility = Visibility.Visible;
                MessageBox.Show("All videos have been played.");
            }
        }


        private void LoadSplashScreen()
        {
            // Load your GIF into SplashGif Image control
            // Example: SplashGif.Source = new BitmapImage(new Uri("path_to_your_gif"));
        }

        private void SplashTimer_Tick(object sender, EventArgs e)
        {
            splashTimer.Stop();
            // Hide SplashScreen and show MainMenu
            SplashScreen.Visibility = Visibility.Collapsed;
            Exam_Menu.Visibility = Visibility.Visible;
            StartScreen.Visibility = Visibility.Visible;

        }

        private void AttachSAMEventHandlers()
        {
            _sam.UpdateTitle += title => SamTitleTextBlock.Text = title;
            _sam.UpdateSubtitle += subtitle => SamSubtitleTextBlock.Text = subtitle;
            _sam.ShowRandomCode += code => SamCodeTextBlock.Text = code;
            _sam.DisplayMessage += message => SamMessageTextBlock.Text = message;
            // ... other event handlers
        }
        private void SAM_Button_Click(object sender, RoutedEventArgs e)
        {
            _sam.PrepareSamExam();
            Exam_Menu.Visibility = Visibility.Collapsed;
            StartScreen.Visibility = Visibility.Collapsed;
            SamStartScreen.Visibility = Visibility.Visible;
        }

        private void SAM_Button_Continue_Click(object sender, RoutedEventArgs e)
        {
            _sam.StartSAMExam();
            SamStartScreen.Visibility = Visibility.Collapsed;
            SamInstructionScreen.Visibility = Visibility.Visible;
            SAMInputFileAnalyze();

        }

        private void SAM_Instruction_Button_Continue_Click(object sender, RoutedEventArgs e)
        {
            _sam.SAM_Instruction_Button_Continue_Click(sender, e);
        }

        private void Demo_Introducer_Continue_Click(object sender, RoutedEventArgs e)
        {
            _sam.Demo_Introducer_Continue_Click(sender, e);
        }

        private void Demo_Trial_Beginner_Continue_Button_Click(object sender, RoutedEventArgs e)
        {
            _sam.Demo_Trial_Beginner_Continue_Button_Click(sender, e);
        }

        private void VideoPlayer_MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            MessageBox.Show("Media failed: " + e.ErrorException.Message);
        }

        private void SAMInputFileAnalyze()
        {
            // Ensure _sam is instantiated
            if (_sam == null)
            {
                _sam = new SAM();
            }

            // Read the input file using the SAM instance
            _sam.ReadInputFile();

            // Update the title on the screen
            UpdateSamDemoTitle("SAM DEMO");
        }


        private void UpdateSamDemoTitle(string title)
        {
            // Assuming you have a TextBlock or similar control for the title
            SamDemoTitleTextBlock.Text = title;
        }


        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

    }

}