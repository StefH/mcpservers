﻿using System.Text;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Stef.Validation;

namespace ModelContextProtocolServer.OpenXml.Stdio.Services;

internal static class WordDocumentReader
{
    public static string GetTextFromWordDocument(string filePath)
    {
        Guard.NotNullOrEmpty(filePath);

        using var doc = WordprocessingDocument.Open(filePath, false);

        if (doc.MainDocumentPart == null)
        {
            return string.Empty;
        }

        var body = ReadBody(doc.MainDocumentPart);
        if (body == null)
        {
            return string.Empty;
        }

        var textBuilder = new StringBuilder();

        var paragraphs = body.Elements();
        foreach (var element in paragraphs)
        {
            textBuilder.AppendLine(GetTextFromElement(element));
            textBuilder.AppendLine();
        }

        return textBuilder.ToString().Trim();
    }

    private static Body? ReadBody(MainDocumentPart main)
    {
        // For some reason, for some documents, we need to read twice.
        try
        {
            return main.Document.Body;
        }
        catch
        {
            return main.Document.Body;
        }
    }

    private static string GetTextFromElement(OpenXmlElement? element)
    {
        return element == null ? string.Empty : string.Join("", element.Descendants<Text>().Select(t => t.Text));
    }
}