import os
import requests
import json
from datetime import datetime, timedelta, timezone

configFile = open("ParmBot/Assets/Resources/config.json")
jsonFile = json.load(configFile)
gemini_key = jsonFile["gemini_key"]
model = "gemini-3.1-flash-lite"


def summarize(articles):
    # Build a combined prompt from all articles
    articles_text = ""
    for i, article in enumerate(articles, 1):
        body = article.get("body", "")
        keywords = article.get("keywords", [])
        keywords_str = ", ".join(keywords) if isinstance(keywords, list) else keywords
        articles_text += f"--- Article {i} ---\nKeywords: {keywords_str}\n{body}\n\n"

    prompt = (
        "You are a news summarizer. Below are multiple articles. "
        "Write a single, cohesive summary that covers the key points from all of them. "
        "Do NOT invent or infer any information that is not explicitly stated in the articles. "
        "Use the provided keywords to guide emphasis and focus.\n\n"
        f"{articles_text}"
        "Provide a clear, concise joint summary:"
    )
    url = f"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={gemini_key}"

    payload = {
        "contents": [
            {
                "parts": [
                    {"text": prompt}
                ]
            }
        ]
    }

    response = requests.post(url, json=payload)
    response.raise_for_status()

    result = response.json()
    summary = result["candidates"][0]["content"]["parts"][0]["text"]
    return summary