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

namespace VerusSententiaeProject
{
    class SAM
    {
        public string Break_Period { get; private set; }
        public List<string> Audio_File_Order { get; private set; }
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

        public SAM()
        {
            Audio_File_Order = new List<string>();
            _currentState = SamState.Start;
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

    }
}