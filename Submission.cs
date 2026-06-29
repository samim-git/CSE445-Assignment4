using System;
using System.Xml.Schema;
using System.Xml;
using Newtonsoft.Json;
using System.IO;

/**
* This template file is created for ASU CSE445 Distributed SW Dev Assignment 4.
* Please do not modify or delete any existing class/variable/method names.
However, you can add more variables and functions.
* Uploading this file directly will not pass the autograder's compilation check,
resulting in a grade of 0.
* **/
namespace ConsoleApp1
{
    public class Submission
    {
        // Your actual GitHub Raw URLs pasted into the existing template variables
        public static string xmlURL = "https://raw.githubusercontent.com/samim-git/CSE445-Assignment4/refs/heads/main/NationalParks.xml";
        public static string xmlErrorURL = "https://raw.githubusercontent.com/samim-git/CSE445-Assignment4/refs/heads/main/NationalParksErrors.xml";
        public static string xsdURL = "https://raw.githubusercontent.com/samim-git/CSE445-Assignment4/refs/heads/main/NationalParks.xsd";

        // Main method left exactly as requested by the template
        public static void Main(string[] args)
        {
            // --- TEST CASE 1 ---
            // Checks if our clean XML file perfectly matches the XSD schema rules
            string result = Verification(xmlURL, xsdURL);
            Console.WriteLine("=== Test Case 1: Validating Clean XML ===");
            Console.WriteLine(result);
            Console.WriteLine();

            // --- TEST CASE 2 ---
            // Tests the error file. It will catch the undeclared root error, 
            // and then stop at the broken syntax tag on Line 51 because a broken 
            // tag completely halts standard XML stream readers.
            Console.WriteLine("=== Test Case 2: Validating Fault-Injected XML ===");
            result = Verification(xmlErrorURL, xsdURL);
            Console.WriteLine(result);
            Console.WriteLine();

            // --- TEST CASE 3 ---
            // Downloads the clean XML and translates it into standard JSON formatting
            Console.WriteLine("=== Test Case 3: Converting XML to JSON ===");
            result = Xml2Json(xmlURL);
            Console.WriteLine(result);
            Console.WriteLine();

            // Prevents the console window from closing instantly when debugging locally
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        // Q2.1
        // This method validates an XML file against an XSD schema rulebook
        public static string Verification(string xmlUrl, string xsdUrl)
        {
            // A dynamic string container used to collect any errors we find
            System.Text.StringBuilder errorLog = new System.Text.StringBuilder();

            try
            {
                // Use the professor's helper method to download the files as plain text strings
                string xmlContent = DownloadContent(xmlUrl);
                string xsdContent = DownloadContent(xsdUrl);

                // 1. Create a configuration tool to customize how the XML is read
                XmlReaderSettings settings = new XmlReaderSettings();

                // Tell the reader that we want it to validate the file against schema rules
                settings.ValidationType = ValidationType.Schema;

                // 2. Load the downloaded XSD schema text into the validator settings
                using (StringReader xsdStringReader = new StringReader(xsdContent))
                using (XmlReader xsdReader = XmlReader.Create(xsdStringReader))
                {
                    settings.Schemas.Add(null, xsdReader);
                }

                // 3. Define an event handler. This block triggers automatically 
                // every time the reader finds an element breaking an XSD rule.
                settings.ValidationEventHandler += (sender, args) =>
                {
                    // Record the exact line location and what went wrong
                    errorLog.AppendLine($"[{args.Severity}] Line {args.Exception.LineNumber}, Position {args.Exception.LinePosition}: {args.Message}");
                };

                // 4. Parse the downloaded XML text using our validation options
                using (StringReader xmlStringReader = new StringReader(xmlContent))
                using (XmlReader reader = XmlReader.Create(xmlStringReader, settings))
                {
                    // This loop forces the reader to step through the document line-by-line.
                    // Schema validation occurs automatically behind the scenes.
                    while (reader.Read()) { }
                }
            }
            catch (XmlException ex)
            {
                // FATAL XML SYNTAX ERROR: Triggers if the XML file is physically broken 
                // (like a missing closing tag). This stops the reader immediately.
                errorLog.AppendLine($"[Fatal XML Error] Line {ex.LineNumber}, Position {ex.LinePosition}: {ex.Message}");
            }
            catch (Exception ex)
            {
                // GENERAL ERROR: Triggers if there is a network issue or unexpected problem
                errorLog.AppendLine($"[General Error] {ex.Message}");
            }

            // 5. Evaluate the results and return the exact string expected by the autograder
            if (errorLog.Length == 0)
            {
                return "No Error"; // Exact success string required by the template comment
            }
            else
            {
                return errorLog.ToString(); // Returns the list of captured errors
            }
        }

        // This method converts valid XML data into standard JSON formatting
        public static string Xml2Json(string xmlUrl)
        {
            try
            {
                // Use the professor's helper method to download the XML file contents as text
                string xmlContent = DownloadContent(xmlUrl);

                // 1. Create an XML Document structure in memory and load the text into it
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(xmlContent);

                // 2. Use the Newtonsoft package to translate the XML tree into a clean JSON string.
                // It automatically prefixes attributes with '@' and handles missing optional elements.
                string jsonText = JsonConvert.SerializeXmlNode(doc, Newtonsoft.Json.Formatting.Indented);

                // 3. Return the fully converted JSON text layout
                return jsonText;
            }
            catch (Exception ex)
            {
                // Return a clean JSON error block if downloading or converting fails
                return $"{{ \"error\": \"Conversion failed: {ex.Message}\" }}";
            }
        }

        // Helper method to download content from URL
        private static string DownloadContent(string url)
        {
            using (System.Net.WebClient client = new System.Net.WebClient())
            {
                return client.DownloadString(url);
            }
        }
    }
}
