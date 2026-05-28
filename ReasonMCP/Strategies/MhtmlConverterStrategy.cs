using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using ReasonMCP.Interfaces;

namespace ReasonMCP.Strategies
{
    //  TODO:   Refactor:   This Strategy needs to be converted to the new pattern
    //  This strategy is not currently used
    //  While it is referenced, the scan worker is "turned off"
    public class MhtmlConverterStrategy : IFileConverterStrategy
    {
        private readonly IMhtmlConverterUtility _mhtmlConverter;
        private readonly IFileConverterUtility _fileConverter;
        private readonly ILogger<MhtmlConverterStrategy> _logger;

        public MhtmlConverterStrategy(
            IMhtmlConverterUtility mhtmlConverter,
            IFileConverterUtility fileConverter,
            ILogger<MhtmlConverterStrategy> logger
        )
        {
            _mhtmlConverter = mhtmlConverter;
            _fileConverter = fileConverter;
            _logger = logger;
        }

        public bool CanConvert(string filePath)
        {
            //  Ensure that the file is a single web page (.mhtml) file
            if (filePath.EndsWith(".mhtml", StringComparison.OrdinalIgnoreCase))
                return true;

            return false;
        }

        public async Task<bool> ConvertForIngestionAsync(string filePath)
        {
            // MHTML will require additional processing before being sent to mark down

            //  1.  send  the file off to the converting utility
            var convertedFile = await _mhtmlConverter.ConvertToTextAsync(filePath);

            //  2.  Create a temp directory to store the file and read from
            var fileName = Path.GetFileName(filePath);
            var fileInfo = new FileInfo(filePath);
            var directoryPath = fileInfo.DirectoryName;
            var tempFileRoot = directoryPath + @"\Temp";
            var tempFilePath = Path.Combine(tempFileRoot, fileName.Replace(".mhtml", ".md"));

            if (!Directory.Exists(tempFileRoot))
            {
                Directory.CreateDirectory(tempFileRoot);
            }

            File.WriteAllText(tempFilePath, convertedFile);

            return await _fileConverter.ConvertToMarkdown(tempFilePath);
        }
    }
}