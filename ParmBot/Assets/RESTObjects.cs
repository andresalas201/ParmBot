using System.Collections;
using System.Text;
using System.Collections.Generic;

namespace RESTObjects
{

    [System.Serializable]
    public class Config
    {
        public string gemini_key;
        public string news_key;
    }

    [System.Serializable]
    public class GeminiResponse
    {
        public Candidate[] candidates;
    }

    [System.Serializable]
    public class Candidate
    {
        public Content content;
    }

    [System.Serializable]
    public class Content
    {
        public Part[] parts;
    }

    [System.Serializable]
    public class Part
    {
        public string text;
    }

    [System.Serializable]
    public class RequestBody
    {
        public List<ConversationTurn> contents;
        public SystemInstruction system_instruction;
        public Tool[] tools;
    }

    [System.Serializable]
    public class SystemInstruction
    {
        public Part[] parts;
    }

    [System.Serializable]
    public class ConversationTurn
    {
        public string role;
        public Part[] parts;
    }

    [System.Serializable]
    public class Tool
    {
        public GoogleSearch google_search;
    }

    [System.Serializable]
    public class GoogleSearch { }

    [System.Serializable]
    public class Article
    {
        public string title;
        public string url;
        public string body;
        public List<string> keywords;
    }

    [System.Serializable]
    public class ArticlesWrapper
    {
        public List<Article> results;
    }

    [System.Serializable]
    public class NewsApiResponse
    {
        public ArticlesWrapper articles;
    }
}