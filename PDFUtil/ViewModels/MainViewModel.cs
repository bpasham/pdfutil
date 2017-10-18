using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using PDFUtil.Helpers;
using System.Threading.Tasks;
using System.Threading;

namespace PDFUtil.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        TaskScheduler _taskScheduler;

        public MainViewModel()
        {
            _suffixCount = true;
            _prefixScanOptions = new ObservableCollection<string>();
            _prefixScanOptions.Add("After");
            _prefixScanOptions.Add("Before");
            _prefixScanDirection = "After";

            this.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(MainViewModel_PropertyChanged);
        }

        void MainViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Error") return;

            this.OnPropertyChanged(p => p.Error);
        }

        string _error;

        public string Error
        {
            get { return _error; }
            set { _error = value;
            this.OnPropertyChanged(p => p.Error);
            }
        }
        
        string _inputFile;

        public string InputFile
        {
            get { return _inputFile; }
            set { _inputFile = value;
            this.OnPropertyChanged(p => p.InputFile);
            }
        }

        string _waterMark;

        public string WaterMark
        {
            get { return _waterMark; }
            set { _waterMark = value;
            this.OnPropertyChanged(p => p.WaterMark);
            }
        }
        string _inputFolder;

        public string InputFolder
        {
            get { return _inputFolder; }
            set
            {
                _inputFolder = value;
                this.OnPropertyChanged(p => p.InputFolder);
                this.OnPropertyChanged(p => p.ShowFolderOptions);
                this.OnPropertyChanged(p => p.ShowPagesPerSplit);
                this.OnPropertyChanged(p => p.ShowStartEnd);
            }
        }

        public bool ShowFolderOptions
        {
            get
            {                
                return !string.IsNullOrWhiteSpace(_inputFolder);
            }
        }

        string _outputFolder;

        public string OutputFolder
        {
            get { return _outputFolder; }
            set
            {
                _outputFolder = value;
                this.OnPropertyChanged(p => p.OutputFolder);
                this.OnPropertyChanged(p => p.ShowStartEnd);
            }
        }

        string _coverPath;

        public string CoverPath
        {
            get { return _coverPath; }
            set { _coverPath = value;
            this.OnPropertyChanged(p => p.CoverPath);
            }
        }

        string _footerPath;

        public string FooterPath
        {
            get { return _footerPath; }
            set
            {
                _footerPath = value;
                this.OnPropertyChanged(p => p.FooterPath);
            }
        }

        bool _hasCoverLetter;

        public bool HasCoverLetter
        {
            get { return _hasCoverLetter; }
            set
            {
                _hasCoverLetter = value;
                this.OnPropertyChanged(p => p.HasCoverLetter);
            }
        }
        Commands.DelegateCommand _runCommand;

        public Commands.DelegateCommand RunCommand
        {
            get {
                if (_runCommand == null)
                {
                    _runCommand = new Commands.DelegateCommand(Run, CanRun);
                }
                return _runCommand; 
            }            
        }

        void Run()
        {
            //Dummy
        }

        bool CanRun()
        {
            return (!string.IsNullOrWhiteSpace(InputFile) || !string.IsNullOrWhiteSpace(InputFolder)) && !string.IsNullOrWhiteSpace(OutputFolder);
        }

        int _startPage;

        public int StartPage
        {
            get { return _startPage; }
            set { _startPage = value;
            this.OnPropertyChanged(p => p.StartPage);
            }
        }

        int _endPage;

        public int EndPage
        {
            get { return _endPage; }
            set { _endPage = value;
            this.OnPropertyChanged(p => p.EndPage);
            }
        }

        int _extractLength;

        public int ExtractLength
        {
            get { return _extractLength; }
            set { _extractLength = value;
            this.OnPropertyChanged(p => p.ExtractLength);
            }
        }

        bool _suffixPageNumber;

        public bool SuffixPageNumber
        {
            get { return _suffixPageNumber; }
            set { _suffixPageNumber = value;
            this.OnPropertyChanged(p => p.SuffixPageNumber);
            }
        }

        bool _suffixCount;

        public bool SuffixCount
        {
            get { return _suffixCount; }
            set
            {
                _suffixCount = value;
                this.OnPropertyChanged(p => p.SuffixCount);
            }
        }

        bool _folderSplitFiles;

        public bool FolderSplitFiles
        {
            get { return _folderSplitFiles; }
            set
            {
                _folderSplitFiles = value;
                this.OnPropertyChanged(p => p.FolderSplitFiles);
                this.OnPropertyChanged(p => p.ShowPagesPerSplit);
            }
        }

        string _prefixPattern;

        public string PrefixPattern
        {
            get { return _prefixPattern; }
            set
            {
                _prefixPattern = value;
                this.OnPropertyChanged(p => p.PrefixPattern);
            }
        }

        private bool _searchForName;

        private string _prefixScanDirection;

        bool _folderIncludeSubFolders;

        public bool FolderIncludeSubFolders
        {
            get { return _folderIncludeSubFolders; }
            set { _folderIncludeSubFolders = value;
            this.OnPropertyChanged(p => p.FolderIncludeSubFolders);
            }
        }

        public bool ShowStartEnd
        {
            get { return string.IsNullOrWhiteSpace(_inputFolder); }
        }

        public bool ShowPagesPerSplit
        {
            get
            {
                return (_folderSplitFiles || !ShowFolderOptions);
            }
        }

        string _statusMessage;

        public string StatusMessage
        {
            get { return _statusMessage; }
            set
            {
                _statusMessage = value;
                this.OnPropertyChanged(p => p.StatusMessage);
            }
        }

        public void RunSplitMerge()
        {
            IsBusy = true;
            _taskScheduler = TaskScheduler.FromCurrentSynchronizationContext();
            Task.Factory.StartNew(RunAsync).ContinueWith(RunAsyncCompleted, _taskScheduler);
        }

        void RunAsyncCompleted(Task t)
        {
            if (t.IsFaulted && (t.Exception != null))
            {
                StatusMessage += ("\nERROR\n" + t.Exception.ToString());
            }
            StatusMessage += ("\nProcess Completed");
            IsBusy = false;
        }

        void RunAsync()
        {
            StatusMessage = "Starting...";
            StatusMessage += ("\nInput: " + _inputFile);
            StatusMessage += ("\nOutput: " + _outputFolder);
            if (!string.IsNullOrWhiteSpace(_coverPath)) //(_hasCoverLetter)
            {
                StatusMessage += ("\nCover: " + _coverPath);
            }
            else
            {
                StatusMessage += ("\nNo Cover Letter");
            }

            if (!string.IsNullOrWhiteSpace(_footerPath)) 
            {
                StatusMessage += ("\nFooter: " + _footerPath);
            }
            else
            {
                StatusMessage += ("\nNo Footer Document");
            }

            if (string.IsNullOrWhiteSpace(_inputFolder)) //Process Input File
            {
                ExtractPages(_inputFile, _outputFolder, _startPage, _endPage, _extractLength);
            }
            else //Process Folder
            {
                ProcessFolder(_inputFolder, _outputFolder);
            }
        }

        void ProcessFolder(string infolder, string outfolder)
        {
            StatusMessage += ("\nProcessing Folder: " + infolder);

            var files = Directory.GetFiles(infolder, "*.pdf");
            if (_folderSplitFiles && _extractLength <= 0)
            {
                ExtractLength = 1;
            }
            StatusMessage += ("\t" + files.Length.ToString() + " Files");

            foreach (string file in files)
            {
                StatusMessage += ("\n\t" + file);
                ExtractPages(file, outfolder, 0, 0, _extractLength);
            }

            if (!_folderIncludeSubFolders) return;
            
            var folders = Directory.GetDirectories(infolder);
            StatusMessage += ("\t" + folders.Length.ToString() + " Folders");

            foreach (var dir in folders)
            {
                StatusMessage += ("\n\t" + dir);

                string foldername = Path.GetFileNameWithoutExtension(dir);
                ProcessFolder(dir, Path.Combine(outfolder, foldername));
            }
        }

        bool _needPassword;

        public bool NeedPassword
        {
            get { return _needPassword; }
            set
            {
                _needPassword = value;
                this.OnPropertyChanged(p => p.NeedPassword);
            }
        }
        
        string _password;

        public string Password
        {
            get { return _password; }
            set { _password = value;
            this.OnPropertyChanged(p => p.Password);
            }
        }

        
        private ObservableCollection<string> _prefixScanOptions;
        public ObservableCollection<string> PrefixScanOptions
        {
            get { return _prefixScanOptions; }            
        }

        public string PrefixScanDirection
        {
            get { return _prefixScanDirection; }
            set
            {
                _prefixScanDirection = value;
                this.OnPropertyChanged(p => p.PrefixScanDirection);
            }
        }

        public bool SearchForName
        {
            get { return _searchForName; }
            set
            {
                _searchForName = value;
                this.OnPropertyChanged(p => p.SearchForName);
            }
        }

        public void Check()
        {

            // get input document
            try
            {
                PdfReader inputPdf = new PdfReader(new iTextSharp.text.pdf.RandomAccessFileOrArray(InputFile), null);
                // retrieve the total number of pages
                EndPage = inputPdf.NumberOfPages;
                inputPdf.Close();
            }
            catch (BadPasswordException ex)
            {
                NeedPassword = true;
                Error = "This file requires a Password";
            }
        }

        private Dictionary<int, string> GetFilePrefixes(PdfReader inputPdf, int start, int end)
        {
            Dictionary<int, string> pageText = new Dictionary<int, string>();
            for (int i = start; i <= end; i++)
            {
                string text = PdfTextExtractor.GetTextFromPage(inputPdf, i, new SimpleTextExtractionStrategy());

                if (string.IsNullOrWhiteSpace(text)) continue;

                text = Encoding.UTF8.GetString(ASCIIEncoding.Convert(Encoding.Default, Encoding.UTF8, Encoding.Default.GetBytes(text)));

                int index = text.IndexOf(_prefixPattern);
                if (index >= 0)
                {
                    if (_prefixScanDirection == "Before")
                    {
                        int ind2 = text.IndexOfAny(new[] { ' ', '\t', '\r', '\n' }, index + _prefixPattern.Length + 1);
                        if (ind2 > (index + _prefixPattern.Length + 1))
                        {
                            string prefix = text.Substring((index + _prefixPattern.Length),
                                                           ind2 - (index + _prefixPattern.Length));
                            pageText.Add(i, prefix);
                        }
                    }
                    else
                    {
                        int ind2 = text.IndexOfAny(new[] {' ', '\t', '\r', '\n'}, index + _prefixPattern.Length + 1);
                        if (ind2 > (index + _prefixPattern.Length + 1))
                        {
                            string prefix = text.Substring((index + _prefixPattern.Length),
                                                           ind2 - (index + _prefixPattern.Length));
                            pageText.Add(i, prefix);
                        }
                    }
                }
            }
            return pageText;
        }

        private Dictionary<int, string> GetFileText(PdfReader inputPdf, int start, int end)
        {
            Dictionary<int, string> pageText = new Dictionary<int, string>();
            for (int i = start; i <= end; i++)
            {
                string text = PdfTextExtractor.GetTextFromPage(inputPdf, i, new SimpleTextExtractionStrategy());

                if (string.IsNullOrWhiteSpace(text)) continue;

                text = Encoding.UTF8.GetString(ASCIIEncoding.Convert(Encoding.Default, Encoding.UTF8, Encoding.Default.GetBytes(text)));
                
                pageText.Add(i, text);                                
            }
            return pageText;
        }

        private void ExtractPages(string inputFile, string outputFile, int start, int end, int pages)
        {
            string filename = Path.GetFileNameWithoutExtension(inputFile);
            int count = 1;
            Dictionary<int, string> pageText = new Dictionary<int, string>();
            Dictionary<int, string> pageTextFull = new Dictionary<int, string>();
            // get input document
            PdfReader inputPdf = new PdfReader(new iTextSharp.text.pdf.RandomAccessFileOrArray(inputFile), null);

            // retrieve the total number of pages
            int pageCount = inputPdf.NumberOfPages;

            if (start <= 0) start = 1;
            if (end > pageCount) end = pageCount;
            if (end <= 0) end = pageCount;
            if (pages < 1) pages = pageCount;

            if (end < start || end > pageCount)
            {
                end = pageCount;
            }

            if (pages > pageCount)
            {
                pages = pageCount - start + 1;
            }

            if (_searchForName)
            {
                pageTextFull = GetFileText(inputPdf, start, end);
            }

            if (!string.IsNullOrWhiteSpace(_prefixPattern))
            {
                pageText = GetFilePrefixes(inputPdf, start, end);
            }

            PdfReader coverPdf = null;

            if (!string.IsNullOrWhiteSpace(_coverPath))
            {
                //Cover Page                        
                coverPdf = new PdfReader(new iTextSharp.text.pdf.RandomAccessFileOrArray(_coverPath), null);
            }

            PdfReader footerPdf = null;
            if (!string.IsNullOrWhiteSpace(_footerPath))
            {
                footerPdf = new PdfReader(new iTextSharp.text.pdf.RandomAccessFileOrArray(_footerPath), null);
            }

            for (int k = start; k <= end; k += pages)
            {
                // load the input document
                Document inputDoc = new Document(inputPdf.GetPageSizeWithRotation(k));

                string outfile = filename.Replace('(', '_').Replace(')', '_');               

                if(_searchForName)
                {
                    bool bgot = false;                    
                    string found = "";

                    //for (int i = k; i < (k + pages); i++)
                    //{
                    //    string text = PdfTextExtractor.GetTextFromPage(inputPdf, i, new SimpleTextExtractionStrategy());
                    //    text = Encoding.UTF8.GetString(ASCIIEncoding.Convert(Encoding.Default, Encoding.UTF8, Encoding.Default.GetBytes(text)));

                    //    if (string.IsNullOrWhiteSpace(text)) continue;
                    //    var x2 = Regex.Match(text, @"\{\#(.*?)\#\}");

                    //    found = x2.Groups[1].Value;
                       
                    //    if (!string.IsNullOrWhiteSpace(found))
                    //    {
                    //        bgot = true;
                    //        break;
                    //    }
                    //}
                    for (int i = k; i < (k + pages); i++)
                    {
                        string text = pageTextFull[i];

                        if (string.IsNullOrWhiteSpace(text)) continue;
                        var x2 = Regex.Match(text, @"\{\#(.*?)\#\}");

                        found = x2.Groups[1].Value;

                        if (!string.IsNullOrWhiteSpace(found))
                        {
                            bgot = true;
                            break;
                        }
                    }
                    if (bgot)
                    {
                        outfile = outfile + "_" + found;
                    }
                }
               

                if (!string.IsNullOrWhiteSpace(_prefixPattern))
                {
                    bool bgot = false;

                    for (int i = k; i < (k + pages); i++)
                    {
                        if (!pageText.ContainsKey(i)) continue;

                        string prefix = pageText[i];

                        if (string.IsNullOrWhiteSpace(prefix)) continue;

                        if (prefix.Length > 0)
                        {
                            outfile = "(" + prefix.Trim() + ")_" + outfile;
                            bgot = true;
                            break;
                        }
                    }
                    if (!bgot)
                    {
                        outfile = "XXXX_" + outfile;
                    }
                }
                
                if (string.IsNullOrWhiteSpace(outfile))
                {
                    outfile = filename.Replace('(', '_').Replace(')', '_') + "_" + count;
                }
                if (outfile.Length > 200)
                {
                    outfile = outfile.Substring(0, 195);
                }

                if (_suffixPageNumber)
                {
                    outfile = outfile + "-" + k.ToString();
                }
                else
                {
                    outfile = outfile + "-" + count.ToString();
                }

                outfile += ".pdf";

                if (!Directory.Exists(outputFile))
                {
                    Directory.CreateDirectory(outputFile);
                }

                string regexSearch = new string(Path.GetInvalidFileNameChars()) +
                                     new string(Path.GetInvalidFileNameChars());
                Regex r = new Regex(string.Format("[{0}]", Regex.Escape(regexSearch)));
                outfile = r.Replace(outfile, "");

                outfile = Path.Combine(outputFile, outfile);

                StatusMessage += ("\n\t\t" + outfile);

                // create the filestream
                using (FileStream fs = new FileStream(outfile, FileMode.Create))
                {
                    //MemoryStream ms = new MemoryStream();
                    // create the output writer 
                    PdfWriter outputWriter = PdfWriter.GetInstance(inputDoc, fs);

                    //Watermark
                    if (!string.IsNullOrWhiteSpace(_waterMark))
                    {
                        PdfWriterEvents writerEvent = new PdfWriterEvents(_waterMark, pages);
                        outputWriter.PageEvent = writerEvent;
                    }

                    inputDoc.Open();
                    PdfContentByte cb1 = outputWriter.DirectContent;

                    if (!string.IsNullOrWhiteSpace(_coverPath))
                    {                       
                        // copy pages from Cover document to output document first
                        for (int i = 1; i <= coverPdf.NumberOfPages; i++)
                        {
                            inputDoc.SetPageSize(coverPdf.GetPageSizeWithRotation(i));
                            inputDoc.NewPage();

                            PdfImportedPage page = outputWriter.GetImportedPage(coverPdf, i);

                            /*int rotation = coverPdf.GetPageRotation(i);

                            if (rotation == 90 || rotation == 270)
                            {
                                cb1.AddTemplate(page, 0, -1f, 1f, 0, 0, coverPdf.GetPageSizeWithRotation(i).Height);
                            }
                            else
                            {
                                cb1.AddTemplate(page, 1f, 0, 0, 1f, 0, 0);
                            }*/
                            var pageRotation = coverPdf.GetPageRotation(i);
                            var pageWidth = coverPdf.GetPageSizeWithRotation(i).Width;
                            var pageHeight = coverPdf.GetPageSizeWithRotation(i).Height;
                            switch (pageRotation)
                            {
                                case 0:
                                    {
                                        cb1.AddTemplate(page, 1f, 0, 0, 1f, 0, 0);
                                        break;
                                    }
                                case 90:
                                    {
                                        cb1.AddTemplate(page, 0, -1f, 1f, 0, 0, pageHeight);
                                        break;
                                    }
                                case 180:
                                    {
                                        cb1.AddTemplate(page, -1f, 0, 0, -1f, pageWidth, pageHeight);
                                        break;
                                    }
                                case 270:
                                    {
                                        cb1.AddTemplate(page, 0, 1f, -1f, 0, pageWidth, 0);
                                        break;
                                    }
                                default:
                                    throw new InvalidOperationException(string.Format(
                                        "Unexpected page rotation: [{0}].", pageRotation));
                            }
                        }                       
                    }

                    // copy pages from input to output document
                    for (int i = k; i < (k + pages); i++)
                    {
                        inputDoc.SetPageSize(inputPdf.GetPageSizeWithRotation(i));
                        inputDoc.NewPage();

                        PdfImportedPage page = outputWriter.GetImportedPage(inputPdf, i);
                        /*
                        int rotation = inputPdf.GetPageRotation(i);

                        if (rotation == 90 || rotation == 270)
                        {
                            cb1.AddTemplate(page, 0, -1f, 1f, 0, 0, inputPdf.GetPageSizeWithRotation(i).Height);
                        }
                        else
                        {
                            cb1.AddTemplate(page, 1f, 0, 0, 1f, 0, 0);
                        }*/
                        var pageRotation = inputPdf.GetPageRotation(i);
                        var pageWidth = inputPdf.GetPageSizeWithRotation(i).Width;
                        var pageHeight = inputPdf.GetPageSizeWithRotation(i).Height;
                        switch (pageRotation)
                        {
                            case 0:
                                {
                                    cb1.AddTemplate(page, 1f, 0, 0, 1f, 0, 0);
                                    break;
                                }
                            case 90:
                                {
                                    cb1.AddTemplate(page, 0, -1f, 1f, 0, 0, pageHeight);
                                    break;
                                }
                            case 180:
                                {
                                    cb1.AddTemplate(page, -1f, 0, 0, -1f, pageWidth, pageHeight);
                                    break;
                                }
                            case 270:
                                {
                                    cb1.AddTemplate(page, 0, 1f, -1f, 0, pageWidth, 0);
                                    break;
                                }
                            default:
                                throw new InvalidOperationException(string.Format(
                                    "Unexpected page rotation: [{0}].", pageRotation));
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(_footerPath))
                    {
                        // copy pages from Footer document to output document first
                        for (int i = 1; i <= footerPdf.NumberOfPages; i++)
                        {
                            inputDoc.SetPageSize(footerPdf.GetPageSizeWithRotation(i));
                            inputDoc.NewPage();

                            PdfImportedPage page = outputWriter.GetImportedPage(footerPdf, i);
                            /*
                            int rotation = footerPdf.GetPageRotation(i);

                            if (rotation == 90 || rotation == 270)
                            {
                                cb1.AddTemplate(page, 0, -1f, 1f, 0, 0, footerPdf.GetPageSizeWithRotation(i).Height);
                            }
                            else
                            {
                                cb1.AddTemplate(page, 1f, 0, 0, 1f, 0, 0);
                            }*/
                            var pageRotation = footerPdf.GetPageRotation(i);
                            var pageWidth = footerPdf.GetPageSizeWithRotation(i).Width;
                            var pageHeight = footerPdf.GetPageSizeWithRotation(i).Height;
                            switch (pageRotation)
                            {
                                case 0:
                                    {
                                        cb1.AddTemplate(page, 1f, 0, 0, 1f, 0, 0);
                                        break;
                                    }
                                case 90:
                                    {
                                        cb1.AddTemplate(page, 0, -1f, 1f, 0, 0, pageHeight);
                                        break;
                                    }
                                case 180:
                                    {
                                        cb1.AddTemplate(page, -1f, 0, 0, -1f, pageWidth, pageHeight);
                                        break;
                                    }
                                case 270:
                                    {
                                        cb1.AddTemplate(page, 0, 1f, -1f, 0, pageWidth, 0);
                                        break;
                                    }
                                default:
                                    throw new InvalidOperationException(string.Format(
                                        "Unexpected page rotation: [{0}].", pageRotation));
                            }
                        }
                    }
                    inputDoc.Close();
                    count++;
                }                
            }
            if (footerPdf != null)
            {
                footerPdf.Close();
            }
            if (coverPdf != null)
            {
                coverPdf.Close();
            }
            inputPdf.Close();
        }

        //Got this code from http://footheory.com/blogs/donnfelker/archive/2008/05/11/using-itextsharp-to-watermark-write-text-to-existing-pdf-s.aspx
        MemoryStream AddWatermarkText(string watermark, MemoryStream ms)
        {
            PdfReader reader = new PdfReader(ms);
            MemoryStream memoryStream = new MemoryStream();

            PdfStamper pdfStamper = new PdfStamper(reader, memoryStream);
            for (int i = 1; i <= reader.NumberOfPages; i++) // Must start at 1 because 0 is not an actual page.
            {
                Rectangle pageSize = reader.GetPageSizeWithRotation(i);
                PdfContentByte pdfPageContents = pdfStamper.GetUnderContent(i);
                pdfPageContents.BeginText(); // Start working with text.
                BaseFont baseFont = BaseFont.CreateFont(BaseFont.HELVETICA_BOLD, Encoding.ASCII.EncodingName, false);
                pdfPageContents.SetFontAndSize(baseFont, 40); // 40 point font
                pdfPageContents.SetRGBColorFill(255, 0, 0); // Sets the color of the font, RED in this instance
                float textAngle = (float)GetHypotenuseAngleInDegreesFrom(pageSize.Height, pageSize.Width);

                pdfPageContents.ShowTextAligned(PdfContentByte.ALIGN_CENTER, watermark, pageSize.Width / 2, pageSize.Height / 2, textAngle);
                pdfPageContents.EndText(); // Done working with text
            }
            pdfStamper.FormFlattening = true; // enable this if you want the PDF flattened.
            pdfStamper.Close(); // Always close the stamper or you'll have a 0 byte stream. 

            return memoryStream;
        }

        double GetHypotenuseAngleInDegreesFrom(double opposite, double adjacent)
        {
            //http://www.regentsprep.org/Regents/Math/rtritrig/LtrigA.htm
            // Tan <angle> = opposite/adjacent
            // Math.Atan2: http://msdn.microsoft.com/en-us/library/system.math.atan2(VS.80).aspx 

            double radians = Math.Atan2(opposite, adjacent); // Get Radians for Atan2
            double angle = radians * (180 / Math.PI); // Change back to degrees
            return angle;
        }
    }
}
