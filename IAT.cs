using System;
using System.Collections.Generic;
using System.Windows.Threading;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows;
using System.Windows.Input;

namespace VerusSententiaeProject
{
    public class IAT
    {
        // Declare necessary fields
        private DateTime _startTime;
        private List<string> _results = new List<string>();
        private List<string> _imagePaths = new List<string>();
        private int _currentImageIndex = 0;
        private bool _isCongruentMapping = true; // This can be randomized
        private DateTime _rsptStartTime;
        private DispatcherTimer _timer;
        private AppState _currentState;
        private string _currentImageName;
        private bool _secondSetDisplayed = false;
        private string descriptionValue;
        private string isMilitary;
        private string whatsThumbsUp;
        private string modifierThumbsUpValue;
        private string whatsThumbsDown;
        private string modifierThumbsDownValue;
        private List<string> militaryImages = new List<string>();
        private List<string> nonMilitaryImages = new List<string>();
        private string answer;
        private readonly DispatcherTimer _splashTimer = new DispatcherTimer();
        private readonly DispatcherTimer _thumbsTimer = new DispatcherTimer();
        private readonly DispatcherTimer _resultDisplayTimer = new DispatcherTimer();

        // Events or methods to update the UI in MainWindow
        // Events for UI updates
        public event Action<ImageSource> DisplayImages;
        public event Action<string> UpdateText;
        public event Action<ImageSource> UpdateImage;
        public event Action<string> UpdateTextBlock;
        public event Action ShowDescription;
        public event Action HideDescription;
        public event Action<bool> ShowDescriptionGrid;
        public event Action<bool> ShowImageBox;
        public event Action<bool> ShowPlusSymbol;
        public event Action ResetDisplayedImage;
        public event Action<string> TestCompleted;
        public event Action<ImageSource> UpdateSpecialImage;
        public event Action<bool> ShowThumbsImage;
        public event Action<bool> ShowResultTextBlock;
        public event Action<string> UpdateResultText;
        public event Action<bool> ShowMainContent;
        // Existing events...


        private enum AppState
        {
            Start,
            Description1,
            Modifier1,
            PlusSymbol,
            DisplayImage,
            AwaitingKeyPress,
            Description2,
            Modifier2,
            ThumbsUp,
            ThumbsDown,
            End
        }

        public IAT()
        {

            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2) };
            _timer.Tick += Timer_Tick;

            _thumbsTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2) };
            _thumbsTimer.Tick += ThumbsTimer_Tick;

            _resultDisplayTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2) };
            _resultDisplayTimer.Tick += ResultDisplayTimer_Tick;

            LoadImagesFromFolder("IAT_Resources", "Images");
            _currentState = AppState.Start;

            // Load the first image (plus symbol) and start the timer
            LoadNextImage();
        }
        public void StartIATExam()
        {
            ShowMainContent?.Invoke(true); // Show the MainContent
            ShowDescriptionGrid?.Invoke(true); // Show the DescriptionTextBlockGrid
            LoadNextImage();
        }
        private void StartButtonClick(object sender, RoutedEventArgs e)
        {
            ShowMainContent?.Invoke(true); // Show the MainContent
            ShowDescriptionGrid?.Invoke(true); // Show the DescriptionTextBlockGrid
        }

        // ... additional events as needed
        private void LoadImagesFromFolder(params string[] relativePathParts)
        {
            // Navigate up from the bin directory to the root of your project
            string rootProjectPath = Directory.GetParent(Directory.GetParent(Directory.GetParent(Directory.GetCurrentDirectory()).FullName).FullName).FullName;

            string folderPath = rootProjectPath;
            foreach (var part in relativePathParts)
            {
                folderPath = System.IO.Path.Combine(folderPath, part);
            }

            if (!Directory.Exists(folderPath))
            {
                throw new Exception($"Directory not found: {folderPath}");
            }

            _imagePaths = Directory.GetFiles(folderPath, "*.*", SearchOption.TopDirectoryOnly)
                                   .Where(file => new string[] { ".jpeg", ".jpg", ".png", ".bmp" }.Any(ext => file.EndsWith(ext)))
                                   .ToList();

            foreach (var imagePath in _imagePaths)
            {
                if (System.IO.Path.GetFileNameWithoutExtension(imagePath).Contains("Military"))
                {
                    militaryImages.Add(imagePath);
                }
                else
                {
                    nonMilitaryImages.Add(imagePath);
                }
            }
        }

        public void IAT_KeyDown(Key key)
        {
            ShowImageBox?.Invoke(false);
            if (_currentState == AppState.DisplayImage || _currentState == AppState.AwaitingKeyPress || _currentState == AppState.ThumbsUp || _currentState == AppState.ThumbsDown)
            {
                string answer = "Incorrect"; // Default to incorrect

                // If the current test is for "Military" images
                if (isMilitary == "Military")
                {
                    // If the image name contains "Military" and the key pressed matches modifierThumbsUpValue
                    if (militaryImages.Contains(_imagePaths[_currentImageIndex]) && key.ToString() == modifierThumbsUpValue)
                    {
                        answer = "Correct";
                    }
                    // If the image name does not contain "Military" and the key pressed matches modifierThumbsDownValue
                    else if (nonMilitaryImages.Contains(_imagePaths[_currentImageIndex]) && key.ToString() == modifierThumbsDownValue)
                    {
                        answer = "Correct";
                    }
                }
                // If the current test is for "NonMilitary" images
                else if (isMilitary == "NonMilitary")
                {
                    // If the image name does not contain "Military" and the key pressed matches modifierThumbsUpValue
                    if (nonMilitaryImages.Contains(_imagePaths[_currentImageIndex]) && key.ToString() == modifierThumbsUpValue)
                    {
                        answer = "Correct";
                    }
                    // If the image name contains "Military" and the key pressed matches modifierThumbsDownValue
                    else if (militaryImages.Contains(_imagePaths[_currentImageIndex]) && key.ToString() == modifierThumbsDownValue)
                    {
                        answer = "Correct";
                    }
                }

                ShowResultTextBlock?.Invoke(true); // Show the ResultTextBlock
                UpdateResultText?.Invoke(answer); // Update the result text

                // Delay for 2 seconds before continuing
                _resultDisplayTimer.Start();

                // Handle key presses during the ThumbsUp and ThumbsDown states
                if (_currentState == AppState.ThumbsUp || _currentState == AppState.ThumbsDown)
                {
                    if (key == Key.A || key == Key.L)
                    {
                        _thumbsTimer.Stop(); // Stop the timer if it's running
                        LoadNextImage();     // Load the next image/state
                        return;              // Return to prevent processing the rest of the method
                    }
                }

                // Check if the pressed key matches the thumbs-up modifier
                if (key.ToString() == modifierThumbsUpValue)
                {
                    _currentState = AppState.ThumbsUp;
                    LoadNextImage();
                }
                // Check if the pressed key matches the thumbs-down modifier
                else if (key.ToString() == modifierThumbsDownValue)
                {
                    _currentState = AppState.ThumbsDown;
                    LoadNextImage();
                }

                TimeSpan reactionTime = DateTime.Now - _rsptStartTime;
                _results.Add($"{descriptionValue}: {isMilitary}, ImageName: {_currentImageName}, Key Pressed: {key}, ThumbsUp: {modifierThumbsUpValue}, ThumbsDown: {modifierThumbsDownValue}, Answer: {answer}, pressed in {reactionTime.TotalMilliseconds} ms");

                if (_currentState == AppState.End)
                {
                    // Save results and show a completion message
                    SaveResults();
                    TestCompleted?.Invoke("Test completed! Results saved.");
                }
                else
                {
                    // Continue with the test
                    LoadNextImage();
                }
            }
        }

        private void LoadNextImage()
        {
            switch (_currentState)
            {
                case AppState.Start:
                    DisplayContent(AppState.Description1);
                    _currentState = AppState.Description1;
                    break;

                case AppState.Description1:
                    DisplayContent(AppState.Modifier1);
                    _currentState = AppState.Modifier1;
                    break;

                case AppState.Modifier1:
                    _currentState = AppState.PlusSymbol;
                    _timer.Start();
                    break;

                case AppState.ThumbsUp:
                    DisplaySpecialImage(@"\IAT_Resources\Thumbs\ThumbsUp\ThumbsUp.png");
                    _thumbsTimer.Start();
                    _timer.Stop();
                    break;

                case AppState.ThumbsDown:
                    DisplaySpecialImage(@"\IAT_Resources\Thumbs\ThumbsDown\ThumbsDown.png");
                    _thumbsTimer.Start();
                    _timer.Stop();
                    break;

                case AppState.PlusSymbol:
                    ShowDescriptionGrid?.Invoke(false); // Show the DescriptionTextBlockGrid
                    ShowImageBox?.Invoke(false); // Hide the ImageBox
                    if (_currentImageIndex >= _imagePaths.Count)
                    {
                        // If we've displayed all images, transition to the appropriate state
                        if (_secondSetDisplayed)
                        {
                            _currentState = AppState.End;
                        }
                        else
                        {
                            _secondSetDisplayed = true;
                            _currentImageIndex = 0;
                            DisplayContent(AppState.Description2);
                            _currentState = AppState.Description2;
                            ShowDescriptionGrid?.Invoke(true); // Show the DescriptionTextBlockGrid
                            ShowImageBox?.Invoke(false); // Hide the ImageBox
                        }
                    }
                    else
                    {
                        DisplayImage();
                    }
                    break;

                case AppState.DisplayImage:
                case AppState.AwaitingKeyPress:
                    _currentImageIndex++;

                    ResetDisplayedImage?.Invoke(); // Notify to reset the image
                    ShowPlusSymbol?.Invoke(true); // Show the PlusSymbol
                    ShowDescriptionGrid?.Invoke(false); // Hide the DescriptionTextBlockGrid
                    ShowImageBox?.Invoke(false); // Hide the ImageBox

                    _currentState = AppState.PlusSymbol;
                    _thumbsTimer.Stop();
                    _timer.Start();
                    break;

                case AppState.Description2:
                    DisplayContent(AppState.Modifier2);
                    _currentState = AppState.Modifier2;
                    _thumbsTimer.Stop();
                    _timer.Start();
                    break;

                case AppState.Modifier2:
                    _currentImageIndex = 0; // Reset image index to start over with the images
                    _currentState = AppState.PlusSymbol;
                    _thumbsTimer.Stop();
                    _timer.Start();
                    break;

                case AppState.End:
                    SaveResults();
                    TestCompleted?.Invoke("Test completed! Results saved.");
                    break;
            }
        }

        public void ContinueButtonClick()
        {
            ShowDescriptionGrid?.Invoke(false);
            ShowPlusSymbol?.Invoke(false);

            switch (_currentState)
            {
                case AppState.Description1:
                    _currentState = AppState.Modifier1;
                    DisplayContent(_currentState);
                    ShowDescriptionGrid?.Invoke(true); // Show description grid
                    break;

                case AppState.Modifier1:
                    _currentState = AppState.PlusSymbol;
                    ShowPlusSymbol?.Invoke(true); // Show plus symbol
                    _thumbsTimer.Stop();
                    _timer.Start();
                    break;

                case AppState.Description2:
                    _currentState = AppState.Modifier2;
                    DisplayContent(_currentState);
                    ShowDescriptionGrid?.Invoke(true); // Show description grid
                    break;

                case AppState.Modifier2:
                    _currentImageIndex = 0; // Reset image index
                    _currentState = AppState.PlusSymbol;
                    ShowPlusSymbol?.Invoke(true); // Show plus symbol
                    _thumbsTimer.Stop();
                    _timer.Start();
                    break;

                default:
                    LoadNextImage();
                    break;
            }
        }

        private string LoadContentFromFile(string relativePath)
        {
            // Get the base directory of the currently executing assembly
            string basePath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

            // Navigate up three directories from the bin directory to the root of your project
            string projectRootPath = Directory.GetParent(Directory.GetParent(Directory.GetParent(basePath).FullName).FullName).FullName;

            // Combine the project root directory with the relative path to get the absolute path
            string fullPath = System.IO.Path.Combine(projectRootPath, relativePath);

            // Check if the file exists
            if (!System.IO.File.Exists(fullPath))
            {
                throw new Exception($"File not found: {fullPath}");
            }

            // Read all lines from the file
            var lines = System.IO.File.ReadAllLines(fullPath);

            // Process the Description files
            if (relativePath == @"IAT_Resources\Description\Description1\Description1.txt" ||
                relativePath == @"IAT_Resources\Description\Description2\Description2.txt")
            {
                // Check if we have at least two lines
                if (lines.Length >= 2)
                {
                    // Set the two variables with the first two lines
                    descriptionValue = lines[0];
                    isMilitary = lines[1];

                    // Return the content of the file, skipping the first two lines
                    return string.Join(Environment.NewLine, lines.Skip(2));
                }
            }
            // Process the Modifier files
            else if (relativePath == @"IAT_Resources\Modifiers\Modifier1\Modifier1.txt" ||
                     relativePath == @"IAT_Resources\Modifiers\Modifier2\Modifier2.txt")
            {
                // Check if we have at least four lines
                if (lines.Length >= 4)
                {
                    // Set the four variables with the first four lines
                    whatsThumbsUp = lines[0];
                    modifierThumbsUpValue = lines[1];
                    whatsThumbsDown = lines[2];
                    modifierThumbsDownValue = lines[3];

                    // Return the content of the file, skipping the first four lines
                    return string.Join(Environment.NewLine, lines.Skip(4));
                }
            }

            // For other files, return the entire content
            return string.Join(Environment.NewLine, lines);
        }
        private void DisplayContent(AppState state)
        {
            string content = string.Empty;
            switch (state)
            {
                case AppState.Description1:
                    content = LoadContentFromFile(@"IAT_Resources\Description\Description1\Description1.txt");
                    break;
                case AppState.Modifier1:
                    content = LoadContentFromFile(@"IAT_Resources\Modifiers\Modifier1\Modifier1.txt");
                    break;
                case AppState.Description2:
                    content = LoadContentFromFile(@"IAT_Resources\Description\Description2\Description2.txt");
                    break;
                case AppState.Modifier2:
                    content = LoadContentFromFile(@"IAT_Resources\Modifiers\Modifier2\Modifier2.txt");
                    break;
            }
            // Raise an event instead of directly updating the UI
            UpdateTextBlock?.Invoke(content);
        }


        private void DisplayImage()
        {
            // Get the image path and create an ImageSource
            var imagePath = _imagePaths[_currentImageIndex];
            var imageSource = new BitmapImage(new Uri(imagePath, UriKind.Absolute));

            // Raise event to display the image
            DisplayImages?.Invoke(imageSource);

            // Update the internal state
            _currentImageName = System.IO.Path.GetFileNameWithoutExtension(imagePath);
            _currentState = AppState.DisplayImage;
            _rsptStartTime = DateTime.Now;

            // You can also raise other events if needed to show/hide elements
            // For example, if you need to change the visibility of PlusSymbol, 
            // you can have another event for that
        }

        private void DisplaySpecialImage(string relativeImagePath)
        {
            // Convert relative path to absolute path
            string basePath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            string projectRootPath = Directory.GetParent(Directory.GetParent(Directory.GetParent(basePath).FullName).FullName).FullName;
            string fullPath = System.IO.Path.Combine(projectRootPath, relativeImagePath.TrimStart('\\'));

            var imageSource = new BitmapImage(new Uri(fullPath, UriKind.Absolute));

            // Raise events to update the UI
            UpdateSpecialImage?.Invoke(imageSource);
            ShowThumbsImage?.Invoke(true); // Show the thumbs image
            ShowPlusSymbol?.Invoke(false); // Hide the plus symbol

            // Stop and start timers as necessary
            _timer.Stop();
            _thumbsTimer.Start();
        }

        private void ThumbsTimer_Tick(object sender, EventArgs e)
        {
            Console.WriteLine("ThumbsTimer Tick triggered. Hiding thumbs image...");
            _thumbsTimer.Stop();
            ShowImageBox?.Invoke(false); // Hide the ImageBox
            ShowThumbsImage?.Invoke(false); // Hide the ThumbsImage
            _currentState = AppState.PlusSymbol;
            _timer.Start();
            LoadNextImage();
        }

        private void ResultDisplayTimer_Tick(object sender, EventArgs e)
        {
            _resultDisplayTimer.Stop();
            ShowResultTextBlock?.Invoke(false); // Hide the ResultTextBlock
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            _timer.Stop();
            LoadNextImage();
        }

        // Call this method when you want to save the results
        private void SaveResults()
        {
            // Navigate up from the bin directory to the root of your project
            string rootProjectPath = Directory.GetParent(Directory.GetParent(Directory.GetParent(Directory.GetCurrentDirectory()).FullName).FullName).FullName;

            string resultsFolderPath = System.IO.Path.Combine(rootProjectPath, "IAT_Resources", "Results");

            // Check if the directory exists, if not, create it
            if (!Directory.Exists(resultsFolderPath))
            {
                Directory.CreateDirectory(resultsFolderPath);
            }

            string resultsPath = System.IO.Path.Combine(resultsFolderPath, "results.txt");
            System.IO.File.WriteAllLines(resultsPath, _results);
        }

        // Method to raise events to update UI
        private void UpdateUI()
        {
            // Example: Raise an event with the image to be displayed
            var imageSource = new BitmapImage(new Uri(_imagePaths[_currentImageIndex], UriKind.Absolute));
            UpdateImage?.Invoke(imageSource);

            // Example: Update TextBlock with description
            string content = LoadContentFromFile(@"IAT_Resources\Description\Description1\Description1.txt");
            UpdateTextBlock?.Invoke(content);

            // Example: Show or hide UI elements
            ShowDescription?.Invoke();
            // ...
        }

        // Add logic to interact with MainWindow and handle IAT test flow
    }
}