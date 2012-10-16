// -----------------------------------------------------------------------
// <copyright file="FileSpecifications.cs" company="Aviva">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace LogSearchTool.Utilities
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// The specifications to perform the file search and pattern match operations 
    /// </summary>
    public class FileSpecifications
    {
        /// <summary>
        /// Gets or sets the remote archive path
        /// </summary>
        public string CopyFrom { get; set; }

        /// <summary>
        /// Gets or sets the local temporary archive path
        /// </summary>
        public string CopyTo { get; set; }

        /// <summary>
        /// Gets or sets the string/keyword to search for
        /// </summary>
        public string ToMatch { get; set; }

        /// <summary>
        /// Gets or sets the selected log file name
        /// </summary>
        public string File { get; set; }

        /// <summary>
        /// Gets or sets the selected date
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// Gets or sets the selected date
        /// </summary>
        public string DateExtension { get; set; }

        /// <summary>
        /// Gets or sets the extension of the file (compressed)
        /// </summary>
        public string FileExtension { get; set; }

        /// <summary>
        /// Gets or sets the list of servers according the specified search criteria
        /// </summary>
        public List<string> Servers { get; set; }
    }
}
