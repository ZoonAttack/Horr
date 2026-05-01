namespace ServiceImplementation.Mappings.Communication
{
    /// <summary>
    /// Static helper for generating truncated message body previews.
    /// 
    /// Rules:
    ///   - null or empty  →  ""
    ///   - body ≤ 50 chars  →  body as-is
    ///   - body > 50 chars  →  first 50 chars + "..."  (total = 53)
    /// </summary>
    public static class MessagePreviewHelper
    {
        private const int MaxPreviewLength = 50;
        private const string Ellipsis = "...";

        public static string GetPreview(string? body)
        {
            if (string.IsNullOrEmpty(body))
                return string.Empty;

            if (body.Length <= MaxPreviewLength)
                return body;

            return body.Substring(0, MaxPreviewLength) + Ellipsis;
        }
    }
}
