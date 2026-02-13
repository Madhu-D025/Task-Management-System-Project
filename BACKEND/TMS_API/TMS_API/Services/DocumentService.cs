
 
using AuthApplication.Models;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Net;
using TMS_API.DBContext;


namespace WM.Services
{
    public class DocumentService
    {
        private readonly AppDbContext _dbContext;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public DocumentService(AppDbContext dbContext, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _dbContext = dbContext;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;

        }
        // In TrainingModule.Services/DocumentService.cs

        public async Task DocumentsUpload(Documentdto doc)
        {
            try
            {
                var allowedExtensions = new List<string>
                {
                    ".pdf", ".jpg", ".jpeg", ".png",".xls",".xlsx",".mp4",".mkv",".vob",".avi",".mp3",".doc",".docx",".mov",
                    ".csv"
                };
                string fileSizeLimit = _configuration["AttachmentSize"];

                //if (doc.Documents.Count != doc.DocumentNameId.Count)
                //{
                //    throw new InvalidOperationException("The number of documents must match the number of document name IDs.");
                //}

                for (int i = 0; i < doc.Documents.Count; i++)
                {
                    var document = doc.Documents[i];
                    //var documentnameid = doc.DocumentNameId[i];

                    if (document != null && document.Length > 0)
                    {
                        // ... (File size and extension validation logic is correct)
                        var documentPath = Path.Combine($"Documents/{doc.FolderName}/{doc.FolderName}_{doc.Id}_Documents", Path.GetFileName(document.FileName));
                        if (!Directory.Exists(Path.GetDirectoryName(documentPath)))
                        {
                            Directory.CreateDirectory(Path.GetDirectoryName(documentPath));
                        }
                        using (var stream = new FileStream(documentPath, FileMode.Create))
                        {
                            await document.CopyToAsync(stream);
                        }

                        var newDocument = new DocumentMaster
                        {
                            DocumentType = doc.documentType,
                            DocumentId = doc.DocumentId,
                            DocumentName = document.FileName,
                            DocumentExtention = Path.GetExtension(document.FileName).ToLower(),
                            DocumentURL = _configuration["ApiURL"] + documentPath,
                            DocumentPath = documentPath,
                            CreatedOn = DateTime.Now,
                            //DocumentNameId = documentnameid,
                            CreatedBy = doc.UserID,
                            InitiationId= doc.InitiationId,
                        };
                        _dbContext.DocumentMaster.Add(newDocument);

                    }
                }
            }
            catch (Exception)
            {
                // === FIX: Re-throw the original exception to preserve the stack trace and inner exception details. ===
                throw;
            }
        }


        //public int GetPdfPageCount(string filePath)
        //{
        //    using var pdf = PdfSharpCore.Pdf.IO.PdfReader.Open(filePath, PdfSharpCore.Pdf.IO.PdfDocumentOpenMode.InformationOnly);
        //    return pdf.PageCount;
        //}
        //public int GetPdfPageCount(string filePath)
        //{
        //    using var pdfReader = new PdfReader(filePath);
        //    using var pdfDoc = new PdfDocument(pdfReader);
        //    return pdfDoc.GetNumberOfPages();
        //}
        //public int GetDocxPageCount(string filePath)
        //{
        //    using var doc = WordprocessingDocument.Open(filePath, false);
        //    var props = doc.ExtendedFilePropertiesPart.Properties;
        //    return int.TryParse(props.Pages?.Text, out int pageCount) ? pageCount : 0;
        //}
        //public async Task<double> GetVideoDurationInSeconds(string filePath)
        //{

        //    var ffmpegPath = Path.Combine(Directory.GetCurrentDirectory(), "Path", "To","ffmpeg");

        //    if (!Directory.Exists(ffmpegPath))
        //    {
        //        throw new DirectoryNotFoundException($"FFmpeg path not found: {ffmpegPath}");
        //    }
        //    FFmpeg.SetExecutablesPath(ffmpegPath); // Set once (e.g. wwwroot/ffmpeg)
        //    var mediaInfo = await FFmpeg.GetMediaInfo(filePath);
        //    return mediaInfo.Duration.TotalSeconds;
        //}





    }
}
