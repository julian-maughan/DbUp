using System;

namespace DbUp.Support
{
    /// <summary>
    /// 
    /// </summary>
    public interface IObjectNameParser
    {
        /// <summary>
        /// Quotes the name of the SQL object in square brackets to allow Special characters in the object name.
        /// This function implements System.Data.SqlClient.SqlCommandBuilder.QuoteIdentifier() with an additional
        /// validation which is missing from the SqlCommandBuilder version.
        /// </summary>
        /// <param name="objectName">Name of the object to quote.</param>
        /// <returns>The quoted object name with trimmed whitespace</returns>
        string Quote(string objectName);

        /// <summary>
        /// Quotes the name of the SQL object in square brackets to allow Special characters in the object name.
        /// This function implements System.Data.SqlClient.SqlCommandBuilder.QuoteIdentifier() with an additional
        /// validation which is missing from the SqlCommandBuilder version.
        /// </summary>
        /// <param name="objectName">Name of the object to quote.</param>
        /// <param name="objectNameOptions">The settings which indicate if the whitespace should be dropped or not.</param>
        /// <returns>The quoted object name</returns>
        string Quote(string objectName, ObjectNameOptions objectNameOptions);
    }
}