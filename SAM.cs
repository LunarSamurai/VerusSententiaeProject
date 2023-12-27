using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.IO;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows;
using System.Windows.Input;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Controls;

namespace VerusSententiaeProject
{
    public class SAM
    {
        public string Break_Period { get; private set; }
        public List<string> Audio_File_Order { get; private set; }
        private List<string> _videoFiles;
        private int _currentVideoIndex;
        private List<string> trialAudioFiles = new List<string>();
        private List<string> trueTrialAudioFiles = new List<string>();
        private string CurrentValenceAnswer = string.Empty;
        public static Grid DemoIntroducer { get; set; }
        public static Grid VideoPlayerGrid { get; set; }
        public static Grid SamInstructionScreen { get; set; }
        public static Grid DemoTrialBeginnerGrid { get; set; }
        public static Grid SoundDisplayedGrid { get; set; }
        public static Grid ValenceRatingGrid { get; set; }
        public static Image ValenceImage { get; set; }
        public static Image ArousalImage { get; set; }
        public static Grid ArousalRatingGrid { get; set; }
        public static MediaElement VideoPlayer { get; set; }

        // Define states for the SAM exam
        private enum SamState
        {
            Start,
            ShowCode,
            DisplayMessage
            // Add other states as needed
        }

        private SamState _currentState;

        // Events for UI updates
        public event Action<string> UpdateTitle;
        public event Action<string> UpdateSubtitle;
        public event Action<string> ShowRandomCode;
        public event Action<string> DisplayMessage; // Event to display the message
        public static Action OnValenceRecordingStart;
        public static event Action<KeyEventArgs> OnSkipVideoKeyPress;


        public SAM()
        {
            Audio_File_Order = new List<string>();
            _currentState = SamState.Start;
            LoadValenceImage();
        }

        public void PrepareSamExam()
        {
            UpdateTitle?.Invoke("SAM Exam");
            UpdateSubtitle?.Invoke("SAM Testing Platform");
            _currentState = SamState.ShowCode;
            ShowCode();
        }

        public void StartSAMExam()
        {
            _currentState = SamState.DisplayMessage;
            DisplaySAMMessage();
        }

        private void ShowCode()
        {
            // Generate a random 6 digit number
            var random = new Random();
            string code = random.Next(100000, 999999).ToString();
            ShowRandomCode?.Invoke(code);
        }

        private void DisplaySAMMessage()
        {
            string message = ReadMessageFromFile("SAM_Resources/Description/Description.txt");
            DisplayMessage?.Invoke(message);
        }



        private string ReadMessageFromFile(string relativePath)
        {
            // The base directory where the application is running
            string baseDir = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

            string projectRootPath = Directory.GetParent(Directory.GetParent(Directory.GetParent(baseDir).FullName).FullName).FullName;


            // Combine the base directory with the relative path
            string fullPath = Path.Combine(projectRootPath, relativePath);

            // Check if the file exists
            if (!File.Exists(fullPath))
            {
                throw new FileNotFoundException($"File not found: {fullPath}");
            }

            // Read and return the content of the file
            return File.ReadAllText(fullPath);
        }

        public void ReadInputFile()
        {
            // The base directory where the application is running
            string baseDir = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

            // Relative path to the file from the base directory
            string relativePath = @"SAM_Resources\Input_Files\Input.txt";

            string projectRootPath = Directory.GetParent(Directory.GetParent(Directory.GetParent(baseDir).FullName).FullName).FullName;

            // Combine the base directory with the relative path
            string fullPath = Path.Combine(projectRootPath, relativePath);

            // Check if the file exists
            if (!File.Exists(fullPath))
            {
                throw new FileNotFoundException($"File not found: {fullPath}");
            }

            // Read the file line by line
            bool isReadingOrder = false;
            foreach (var line in File.ReadLines(fullPath))
            {
                if (line.Trim().Equals("Break_Period"))
                {
                    // Read the next line after "Break_Period"
                    Break_Period = File.ReadLines(fullPath)
                                       .SkipWhile(l => !l.Trim().Equals("Break_Period"))
                                       .Skip(1)
                                       .FirstOrDefault()?.Trim();
                    continue;
                }

                if (line.Trim().Equals("Name_Order"))
                {
                    isReadingOrder = true;
                    continue;
                }

                if (line.Trim().Equals("End_Order"))
                {
                    isReadingOrder = false;
                    continue;
                }

                if (isReadingOrder)
                {
                    Audio_File_Order.Add(line.Trim());
                }
            }


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

        public void SkipVideo()
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
            DemoIntroducer.Visibility = Visibility.Collapsed;
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

        // In SAM class

        public static bool IsValenceRecording { get; set; }

        public static void StartValenceRecording()
        {
            IsValenceRecording = true;
        }

        public static void RecordValenceRating(KeyEventArgs e)
        {
            // Check key and record rating

            IsValenceRecording = false;
            ValenceRatingGrid.Visibility = Visibility.Collapsed;
            ArousalRatingGrid.Visibility = Visibility.Visible;
        }


        private void MediaPlayer_MediaEnded(object sender, EventArgs e)

        {
            SoundDisplayedGrid.Visibility = Visibility.Collapsed;
            ValenceRatingGrid.Visibility = Visibility.Visible;
            OnValenceRecordingStart?.Invoke();
        }

    }
}