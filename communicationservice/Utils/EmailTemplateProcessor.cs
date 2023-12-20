namespace communicationservice.Utils
{
    using System.Collections.Generic;

    public static class EmailTemplateProcessor
    {
        public static string ProcessTemplate(Dictionary<string, string> replacements)
        {
            string emailTemplate = GetTemplate();

            foreach (KeyValuePair<string, string> replacement in replacements)
            {
                emailTemplate = ReplacePlaceholder(emailTemplate, replacement.Key, replacement.Value);
            }

            return emailTemplate;
        }

        private static string ReplacePlaceholder(string template, string placeholder, string value)
        {
            string formattedPlaceholder = $"{{{placeholder}}}";
            return template.Replace(formattedPlaceholder, value);
        }

        private static string GetTemplate()
        {
            return "<!DOCTYPE html>\n" +
                   "<html>\n" +
                   "<body>\n" +
                   "    <h1>Hi!</h1>\n" +
                   "    <p>We're excited to invite you to join our labelling project, \"{project_name}\".</p>\n" +
                   "    <p>To join the project, please click the link below:</p>\n" +
                   "    <p><a href=\"{project_link}\">Join the Project</a></p>\n" +
                   "    <p>Thank you for your interest in our project. We look forward to your valuable contributions.</p>\n" +
                   "    <p>Kind regards</p>\n" +
                   "</body>\n" +
                   "</html>";
        }
    }
}
