using System;
using System.Collections.Generic;
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
using WpfAnimatedGif;
using System.Windows.Threading;
using System.Reflection;
using System.Diagnostics;

namespace VerusSententiaeProject
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DispatcherTimer splashTimer = new DispatcherTimer();
        private IAT _iat; 
        private SAM _sam;
        private List<string> _videoFiles;
        private int _currentVideoIndex;
        private List<string> trialAudioFiles = new List<string>();
        private List<string> trueTrialAudioFiles = new List<string>();
        private string CurrentValenceAnswer = string.Empty;

        public MainWindow()
        {

            InitializeComponent();
            LoadValenceImage(); 
            _iat = new IAT();
            _sam = new SAM();
            AttachIATEventHandlers();
            AttachSAMEventHandlers();
            // Setting the animated GIF for the splash screen
            var image = new BitmapImage();
            image.BeginInit();
            image.UriSource = new Uri(@"\IAT_Resources\Logo\logo.gif", UriKind.RelativeOrAbsolute);
            image.EndInit();
            ImageBehavior.SetAnimatedSource(img, image);  // Assuming SplashImage is the name of your Image control in XAML

            LoadSplashScreen();

            splashTimer.Interval = TimeSpan.FromSeconds(10);
            splashTimer.Tick += SplashTimer_Tick;
            splashTimer.Start();
            this.KeyDown += MainWindow_KeyDown; // 6. Attach the key down event handler
        }
        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            _iat.IAT_KeyDown(e.Key);
            if (e.KeyboardDevice.Modifiers == ModifierKeys.Alt && e.Key == Key.Y)
            {
                SkipVideo();
            }
        }

        private void AttachIATEventHandlers()
        {
            _iat.UpdateImage += image => DisplayedImage.Source = image;
            _iat.UpdateTextBlock += UpdateTextBlockUI;
            _iat.DisplayImages += UpdateDisplayedImage;
            _iat.ShowDescription += () => DescriptionTextBlockGrid.Visibility = Visibility.Visible;
            _iat.HideDescription += () => DescriptionTextBlockGrid.Visibility = Visibility.Collapsed;
            _iat.ShowDescriptionGrid += ShowDescriptionGridUI;
            _iat.ShowImageBox += ShowImageBoxUI;
            _iat.ResetDisplayedImage += ResetDisplayedImageUI;
            _iat.ShowPlusSymbol += ShowPlusSymbolUI;
            _iat.TestCompleted += ShowCompletionMessage;
            _iat.UpdateSpecialImage += UpdateThumbsImageUI;
            _iat.ShowThumbsImage += ToggleThumbsImageVisibility;
            _iat.ShowResultTextBlock += ToggleResultTextBlockVisibility;
            _iat.ShowMainContent += ToggleMainContentVisibility;

        }
        private void ToggleMainContentVisibility(bool isVisible)
        {
            MainContent.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
        }

        private void UpdateTextBlockUI(string text)
        {
            // Assuming you have a TextBlock control named 'DescriptionTextBlock'
            DescriptionTextBlock.Text = text;
        }

        private void UpdateDisplayedImage(ImageSource imageSource)
        {
            // Assuming you have an Image control named 'DisplayedImage'
            DisplayedImage.Source = imageSource;

            // If you have events for showing/hiding other elements like PlusSymbol,
            // handle them similarly
            // For example:
            // PlusSymbol.Visibility = Visibility.Visible or Visibility.Collapsed
        }

        private void ResetDisplayedImageUI()
        {
            DisplayedImage.Source = null; // Assuming DisplayedImage is an Image control in your XAML
        }

        private void ShowPlusSymbolUI(bool isVisible)
        {
            PlusSymbol.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed; // Update as per your UI element
        }

        private void ShowDescriptionGridUI(bool isVisible)
        {
            DescriptionTextBlockGrid.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
        }

        private void ShowImageBoxUI(bool isVisible)
        {
            ImageBox.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
        }

        private void ShowCompletionMessage(string message)
        {
            MessageBox.Show(message, "Completion", MessageBoxButton.OK, MessageBoxImage.Information);
            Close(); // Assuming this is meant to close the MainWindow
        }

        private void ToggleDescriptionGridVisibility(bool isVisible)
        {
            DescriptionTextBlockGrid.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
        }

        private void TogglePlusSymbolVisibility(bool isVisible)
        {
            PlusSymbol.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
        }

        private void UpdateThumbsImageUI(ImageSource imageSource)
        {
            ThumbsImage.Source = imageSource; // Assuming 'ThumbsImage' is an Image control
        }

        private void ToggleThumbsImageVisibility(bool isVisible)
        {
            ThumbsImage.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
        }

        private void ToggleResultTextBlockVisibility(bool isVisible)
        {
            ResultTextBlock.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
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
;           SplashScreen.Visibility = Visibility.Collapsed;
            Exam_Menu.Visibility = Visibility.Visible;
            StartScreen.Visibility = Visibility.Visible;

        }

        private void ContinueButton_Click(object sender, RoutedEventArgs e)
        {
            // Example: Call a method on the IAT instance
            _iat.ContinueButtonClick(); // Assuming you have a method like this in your IAT class
        }

        private void IAT_Button_Click(object sender, RoutedEventArgs e)
        {
            // Start the IAT test
            _iat.StartIATExam();
            Exam_Menu.Visibility = Visibility.Collapsed;
            StartScreen.Visibility = Visibility.Collapsed;
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

        private void SAM_Instruction_Button_Continue_Click(object sender, RoutedEventArgs e)
        {
            SamInstructionScreen.Visibility = Visibility.Collapsed;
            DemoIntroducer.Visibility = Visibility.Visible;
            VideoPlayerGrid.Visibility = Visibility.Visible;
            PlayIntroductionVideo();
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

        private void SkipVideo()
        {
            // Stop the current video
            VideoPlayer.Stop();

            // Increment the video index or collapse the video player grid if all videos have been played
            _currentVideoIndex++;
            if (_currentVideoIndex < _videoFiles.Count)
            {
                PlayIntroductionVideo();
            }
            else
            {
                VideoPlayerGrid.Visibility = Visibility.Collapsed;
                MessageBox.Show("All videos have been played.");
            }
        }


        private void VideoPlayer_MediaEnded(object sender, RoutedEventArgs e)
        {
            _currentVideoIndex++;
            if (_currentVideoIndex < _videoFiles.Count)
            {
                PlayIntroductionVideo(); // Play the next video
            }
            else
            {
                VideoPlayerGrid.Visibility = Visibility.Collapsed; // Collapse the video player when all videos have been played
                MessageBox.Show("All videos have been played.");
            }
        }

        private void LoadAudioFiles()
        {
            string baseDir = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string projectRootPath = Directory.GetParent(Directory.GetParent(Directory.GetParent(baseDir).FullName).FullName).FullName;
            string audioFolderPath = System.IO.Path.Combine(projectRootPath, "SAM_Resources", "Audio_Files");

            if (!Directory.Exists(audioFolderPath))
            {
                MessageBox.Show("Audio directory not found.");
                return;
            }

            var audioFormats = new[] { "*.mp3", "*.wav" }; // Add more formats if needed
            var allAudioFiles = audioFormats.SelectMany(format => Directory.EnumerateFiles(audioFolderPath, format)).ToList();

            foreach (var file in allAudioFiles)
            {
                if (System.IO.Path.GetFileName(file).Contains("trial"))
                {
                    trialAudioFiles.Add(file);
                }
                else
                {
                    trueTrialAudioFiles.Add(file);
                }
            }

            // Here you can add logic to use these lists as required
        }
        private void OutputTrialAudioFiles()
        {
            if (trialAudioFiles.Count == 0)
            {
                // If the list is empty, print this message
                Debug.WriteLine("The list is empty.");
            }
            else
            {
                // If the list is not empty, loop through and print each file
                foreach (var file in trialAudioFiles)
                {
                    Debug.WriteLine(file);
                }
            }
            Debug.WriteLine("Is this Console writing to line working at all?");
        }

        private void Demo_Introducer_Continue_Click(object sender, RoutedEventArgs e)
        {
            DemoIntroducer.Visibility= Visibility.Collapsed;
            DemoTrialBeginnerGrid.Visibility = Visibility.Visible;
            LoadAudioFiles();
            OutputTrialAudioFiles(); // This will output the contents of trialAudioFiles
        }

        private void Demo_Trial_Beginner_Continue_Button_Click(object sender, RoutedEventArgs e)
        {
            SoundDisplayedGrid.Visibility = Visibility.Visible;
            DemoTrialBeginnerGrid.Visibility = Visibility.Collapsed;

            if (trialAudioFiles.Any())
            {
                string firstAudioFile = trialAudioFiles.First();
                PlayAudio(firstAudioFile);
            }
        }

        private void PlayAudio(string audioFilePath)
        {
            MediaPlayer mediaPlayer = new MediaPlayer();
            mediaPlayer.Open(new Uri(audioFilePath, UriKind.RelativeOrAbsolute));
            mediaPlayer.MediaEnded += MediaPlayer_MediaEnded;
            mediaPlayer.Play();
        }

        private void LoadValenceImage()
        {
            try
            {
                // Get the base directory of the application.
                string baseDir = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                // Navigate up to the project root (adjust the number of 'Parent' calls as needed for your directory structure).
                string projectRootPath = Directory.GetParent(Directory.GetParent(Directory.GetParent(baseDir).FullName).FullName).FullName;
                // Combine the project root with the relative path to the image.
                string imagePath = System.IO.Path.Combine(projectRootPath, "SAM_Resources", "ValenceImage", "CorrectedValence.png"); // Update with the actual image name and format.

                // Load the image and set it to the Image control's Source property.
                BitmapImage image = new BitmapImage(new Uri(imagePath, UriKind.Absolute));
                ValenceImage.Source = image; // Make sure you have an Image control with x:Name="ValenceImage" in your XAML.
            }
            catch (Exception ex)
            {
                // If there's an error loading the image, show it in a MessageBox or handle it as needed.
                MessageBox.Show("Error loading image: " + ex.Message);
            }
        }

        private void ValenceRecorder()
        {
            // Set focus to the main window to capture key presses
            this.Focus();

            // You may also want to provide instructions to the user, e.g., via a TextBlock
            // InstructionsTextBlock.Text = "Press a number key to rate valence.";
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            // Check if ValenceRatingGrid is visible and then record the key press.
            if (ValenceRatingGrid.Visibility == Visibility.Visible)
            {
                // Assuming you want to capture number keys for valence rating (1-9)
                if (e.Key >= Key.D1 && e.Key <= Key.D9)
                {
                    // Store the key pressed as the current valence answer
                    CurrentValenceAnswer = e.Key.ToString().Substring(1); // This will get the number part of the key name (D1 -> 1)

                    // Optionally, you can then hide the ValenceRatingGrid and proceed to the next part of your application.
                    ValenceRatingGrid.Visibility = Visibility.Collapsed;

                    // Continue with other tasks, like showing the next audio file or saving the answer.
                    // You might call another method here to process the answer and continue the workflow.
                }
            }
        }


        private void MediaPlayer_MediaEnded(object sender, EventArgs e)

        {
            SoundDisplayedGrid.Visibility = Visibility.Collapsed;
            ValenceRatingGrid.Visibility = Visibility.Visible;
            ValenceRecorder();
        }
    }

}